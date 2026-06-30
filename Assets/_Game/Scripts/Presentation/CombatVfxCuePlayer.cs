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

        public bool PlayCue(
            CombatVfxCueId cueId,
            Transform anchor,
            Vector3 planarDirection,
            float intensity = 1f,
            float audioIntensity = -1f)
        {
            return PlayCue(cueId, anchor, planarDirection, intensity, audioIntensity, Vector3.zero);
        }

        public bool PlayCue(
            CombatVfxCueId cueId,
            Transform anchor,
            Vector3 planarDirection,
            float intensity,
            float audioIntensity,
            Vector3 additionalLocalPositionOffset)
        {
            if (profile == null || !profile.TryGetCue(cueId, out CombatVfxCue cue))
            {
                return false;
            }

            if (!profile.AllowsPlayback(cueId))
            {
                return true;
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
                instanceTransform.localPosition = cue.LocalPositionOffset + additionalLocalPositionOffset;
                instanceTransform.localRotation = localRotation;
            }
            else
            {
                Vector3 basePosition = anchor != null ? anchor.position : transform.position;
                Quaternion baseRotation = anchor != null ? anchor.rotation : transform.rotation;
                Quaternion worldRotation = cue.AlignForwardToDirection ? localRotation : baseRotation * localRotation;
                Vector3 localPosition = cue.LocalPositionOffset + additionalLocalPositionOffset;
                instanceTransform.SetPositionAndRotation(basePosition + baseRotation * localPosition, worldRotation);
            }

            float scale = Mathf.Max(0f, intensity);
            float resolvedAudioScale = audioIntensity >= 0f ? Mathf.Max(0f, audioIntensity) : scale;
            instanceTransform.localScale = cue.LocalScale * Mathf.Max(0.001f, scale);
            instance.SetActive(true);
            float audioTailSeconds = PlayEffects(instance, resolvedAudioScale);

            float releaseSeconds = Mathf.Max(cue.LifetimeSeconds, audioTailSeconds);
            if (releaseSeconds > 0f)
            {
                StartCoroutine(ReleaseAfterSeconds(instance, releaseSeconds));
            }

            return true;
        }

        public void StopAllActiveCuesForReview()
        {
            if (pooledRoot == null)
            {
                pooledRoot = transform;
            }

            foreach (Queue<GameObject> pool in poolsByPrefab.Values)
            {
                pool.Clear();
            }

            List<KeyValuePair<GameObject, GameObject>> instances =
                new List<KeyValuePair<GameObject, GameObject>>(prefabByInstance);
            for (int i = 0; i < instances.Count; i++)
            {
                GameObject instance = instances[i].Key;
                GameObject prefab = instances[i].Value;
                if (instance == null || prefab == null)
                {
                    continue;
                }

                StopEffects(instance);
                instance.SetActive(false);
                instance.transform.SetParent(pooledRoot, worldPositionStays: false);
                GetPool(prefab).Enqueue(instance);
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
                if (!profile.AllowsPlayback(cueId) || !profile.TryGetCue(cueId, out CombatVfxCue cue) || cue.PrewarmCount <= 0)
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

        private static float PlayEffects(GameObject instance, float audioScale)
        {
            float audioTailSeconds = 0f;
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

            CombatVfxCueAudioRandomizer[] audioRandomizers = instance.GetComponentsInChildren<CombatVfxCueAudioRandomizer>(includeInactive: true);
            HashSet<AudioSource> randomizedSources = null;
            for (int i = 0; i < audioRandomizers.Length; i++)
            {
                CombatVfxCueAudioRandomizer audioRandomizer = audioRandomizers[i];
                if (audioRandomizer == null || !audioRandomizer.Play(audioScale) || audioRandomizer.Source == null)
                {
                    continue;
                }

                randomizedSources ??= new HashSet<AudioSource>();
                randomizedSources.Add(audioRandomizer.Source);
                audioTailSeconds = Mathf.Max(audioTailSeconds, audioRandomizer.LastPlayedClipDurationSeconds);
            }

            AudioSource[] audioSources = instance.GetComponentsInChildren<AudioSource>(includeInactive: true);
            for (int i = 0; i < audioSources.Length; i++)
            {
                AudioSource audioSource = audioSources[i];
                if (randomizedSources != null && randomizedSources.Contains(audioSource))
                {
                    continue;
                }

                if (audioSource.clip == null || !audioSource.enabled || !audioSource.gameObject.activeInHierarchy)
                {
                    continue;
                }

                audioSource.Stop();
                audioSource.Play();
                if (audioSource.clip != null)
                {
                    float pitch = Mathf.Abs(audioSource.pitch) > 0.001f ? Mathf.Abs(audioSource.pitch) : 1f;
                    audioTailSeconds = Mathf.Max(audioTailSeconds, audioSource.clip.length / pitch);
                }
            }

            return audioTailSeconds;
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

            AudioSource[] audioSources = instance.GetComponentsInChildren<AudioSource>(includeInactive: true);
            for (int i = 0; i < audioSources.Length; i++)
            {
                audioSources[i].Stop();
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
