using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace DimensionBrawl.Presentation
{
    [DisallowMultipleComponent]
    public sealed class CombatVfxCuePlayer : MonoBehaviour
    {
        [SerializeField] private CombatVfxCueProfile profile;
        [SerializeField] private Transform pooledRoot;

        private readonly Dictionary<GameObject, Queue<GameObject>> poolsByPrefab = new Dictionary<GameObject, Queue<GameObject>>();
        private readonly Dictionary<GameObject, GameObject> prefabByInstance = new Dictionary<GameObject, GameObject>();

        public CombatVfxCueProfile Profile => profile;

        private void Awake()
        {
            if (pooledRoot == null)
            {
                pooledRoot = transform;
            }
        }

        private void Start()
        {
            PrewarmKnownCues();
        }

        public void PlayCue(CombatVfxCueId cueId, Transform anchor, Vector3 planarDirection, float intensity = 1f)
        {
            if (profile == null || !profile.TryGetCue(cueId, out CombatVfxCue cue))
            {
                return;
            }

            GameObject instance = GetInstance(cue.Prefab);
            Transform instanceTransform = instance.transform;
            Transform parent = cue.ParentToAnchor ? anchor : null;
            instanceTransform.SetParent(parent, worldPositionStays: false);

            Quaternion localRotation = Quaternion.Euler(cue.LocalEulerOffset);
            if (cue.AlignForwardToDirection && TryResolvePlanarDirection(planarDirection, anchor, out Vector3 direction))
            {
                localRotation = Quaternion.LookRotation(direction, Vector3.up) * localRotation;
            }

            if (parent != null)
            {
                instanceTransform.localPosition = cue.LocalPositionOffset;
                instanceTransform.localRotation = localRotation;
            }
            else
            {
                Vector3 basePosition = anchor != null ? anchor.position : transform.position;
                Quaternion baseRotation = anchor != null ? anchor.rotation : transform.rotation;
                Quaternion worldRotation = cue.AlignForwardToDirection ? localRotation : baseRotation * localRotation;
                instanceTransform.SetPositionAndRotation(basePosition + baseRotation * cue.LocalPositionOffset, worldRotation);
            }

            float scale = Mathf.Max(0f, intensity);
            instanceTransform.localScale = cue.LocalScale * Mathf.Max(0.001f, scale);
            instance.SetActive(true);
            PlayEffects(instance);

            if (cue.LifetimeSeconds > 0f)
            {
                StartCoroutine(ReleaseAfterSeconds(instance, cue.LifetimeSeconds));
            }
        }

        private void PrewarmKnownCues()
        {
            if (profile == null)
            {
                return;
            }

            for (int i = 0; i < System.Enum.GetValues(typeof(CombatVfxCueId)).Length; i++)
            {
                CombatVfxCueId cueId = (CombatVfxCueId)i;
                if (!profile.TryGetCue(cueId, out CombatVfxCue cue) || cue.PrewarmCount <= 0)
                {
                    continue;
                }

                Queue<GameObject> pool = GetPool(cue.Prefab);
                for (int j = pool.Count; j < cue.PrewarmCount; j++)
                {
                    GameObject instance = CreateInstance(cue.Prefab);
                    ReleaseInstance(instance);
                }
            }
        }

        private GameObject GetInstance(GameObject prefab)
        {
            Queue<GameObject> pool = GetPool(prefab);
            while (pool.Count > 0)
            {
                GameObject pooled = pool.Dequeue();
                if (pooled != null)
                {
                    return pooled;
                }
            }

            return CreateInstance(prefab);
        }

        private Queue<GameObject> GetPool(GameObject prefab)
        {
            if (!poolsByPrefab.TryGetValue(prefab, out Queue<GameObject> pool))
            {
                pool = new Queue<GameObject>();
                poolsByPrefab.Add(prefab, pool);
            }

            return pool;
        }

        private GameObject CreateInstance(GameObject prefab)
        {
            GameObject instance = Instantiate(prefab, pooledRoot);
            instance.name = prefab.name;
            prefabByInstance[instance] = prefab;
            instance.SetActive(false);
            return instance;
        }

        private IEnumerator ReleaseAfterSeconds(GameObject instance, float seconds)
        {
            yield return new WaitForSeconds(seconds);
            ReleaseInstance(instance);
        }

        private void ReleaseInstance(GameObject instance)
        {
            if (instance == null || !prefabByInstance.TryGetValue(instance, out GameObject prefab))
            {
                return;
            }

            StopEffects(instance);
            instance.SetActive(false);
            instance.transform.SetParent(pooledRoot, worldPositionStays: false);
            GetPool(prefab).Enqueue(instance);
        }

        private static void PlayEffects(GameObject instance)
        {
            CombatVfxCueVisual[] cueVisuals = instance.GetComponentsInChildren<CombatVfxCueVisual>(includeInactive: true);
            for (int i = 0; i < cueVisuals.Length; i++)
            {
                cueVisuals[i].Restart();
            }

            ParticleSystem[] particles = instance.GetComponentsInChildren<ParticleSystem>(includeInactive: true);
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].Clear(withChildren: true);
                particles[i].Play(withChildren: true);
            }

            VisualEffect[] visualEffects = instance.GetComponentsInChildren<VisualEffect>(includeInactive: true);
            for (int i = 0; i < visualEffects.Length; i++)
            {
                visualEffects[i].Reinit();
                visualEffects[i].Play();
            }
        }

        private static void StopEffects(GameObject instance)
        {
            CombatVfxCueVisual[] cueVisuals = instance.GetComponentsInChildren<CombatVfxCueVisual>(includeInactive: true);
            for (int i = 0; i < cueVisuals.Length; i++)
            {
                cueVisuals[i].StopNow();
            }

            ParticleSystem[] particles = instance.GetComponentsInChildren<ParticleSystem>(includeInactive: true);
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].Stop(withChildren: true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            VisualEffect[] visualEffects = instance.GetComponentsInChildren<VisualEffect>(includeInactive: true);
            for (int i = 0; i < visualEffects.Length; i++)
            {
                visualEffects[i].Stop();
            }
        }

        private static bool TryResolvePlanarDirection(Vector3 planarDirection, Transform anchor, out Vector3 direction)
        {
            direction = Vector3.ProjectOnPlane(planarDirection, Vector3.up);
            if (direction.sqrMagnitude > 0.0001f)
            {
                direction.Normalize();
                return true;
            }

            if (anchor == null)
            {
                return false;
            }

            direction = Vector3.ProjectOnPlane(anchor.forward, Vector3.up);
            if (direction.sqrMagnitude <= 0.0001f)
            {
                return false;
            }

            direction.Normalize();
            return true;
        }
    }
}
