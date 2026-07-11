using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.VFX;

namespace DimensionBrawl.Presentation
{
    [DisallowMultipleComponent]
    public sealed class SpatialOneShotVfxPool : MonoBehaviour
    {
        private sealed class Entry
        {
            public GameObject Prefab;
            public GameObject Root;
            public ParticleSystem[] Particles;
            public VisualEffect[] VisualEffects;
            public Vector3 BaseScale;
            public float ReleaseTime;
            public bool Active;
        }

        private static SpatialOneShotVfxPool activeInstance;
        private readonly List<Entry> entries = new(16);
        private Coroutine releaseRoutine;

        public static SpatialOneShotVfxPool ActiveInstance => activeInstance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            activeInstance = null;
        }

        public static SpatialOneShotVfxPool GetOrCreate(Component sceneOwner)
        {
            if (activeInstance != null)
            {
                return activeInstance;
            }

            GameObject root = new GameObject("[SpatialOneShotVfxPool]");
            if (sceneOwner != null && sceneOwner.gameObject.scene.IsValid())
            {
                SceneManager.MoveGameObjectToScene(root, sceneOwner.gameObject.scene);
            }

            return root.AddComponent<SpatialOneShotVfxPool>();
        }

        public int GetPoolSize(GameObject prefab)
        {
            int count = 0;
            for (int index = 0; index < entries.Count; index++)
            {
                Entry entry = entries[index];
                if (entry != null && entry.Prefab == prefab && entry.Root != null)
                {
                    count++;
                }
            }

            return count;
        }

        public int GetActiveCount(GameObject prefab)
        {
            int count = 0;
            for (int index = 0; index < entries.Count; index++)
            {
                Entry entry = entries[index];
                if (entry != null && entry.Prefab == prefab && entry.Root != null && entry.Active)
                {
                    count++;
                }
            }

            return count;
        }

        public void Prewarm(GameObject prefab, int count)
        {
            if (prefab == null || count <= 0)
            {
                return;
            }

            while (GetPoolSize(prefab) < count)
            {
                CreateEntry(prefab);
            }
        }

        public void Play(
            GameObject prefab,
            Vector3 position,
            Quaternion rotation,
            float scale,
            float lifetimeSeconds)
        {
            if (prefab == null)
            {
                return;
            }

            ReleaseExpired();
            Entry entry = FindInactiveEntry(prefab) ?? CreateEntry(prefab);
            if (entry?.Root == null)
            {
                return;
            }

            Transform instanceTransform = entry.Root.transform;
            instanceTransform.SetPositionAndRotation(position, rotation);
            instanceTransform.localScale = entry.BaseScale * Mathf.Max(0.01f, scale);
            entry.Root.SetActive(true);
            RestartParticles(entry.Particles);
            RestartVisualEffects(entry.VisualEffects);
            entry.Active = true;
            entry.ReleaseTime = Time.time + Mathf.Max(0.05f, lifetimeSeconds);
            EnsureReleaseRoutine();
        }

        private void Awake()
        {
            if (activeInstance != null && activeInstance != this)
            {
                Destroy(gameObject);
                return;
            }

            activeInstance = this;
        }

        private void OnEnable()
        {
            EnsureReleaseRoutine();
        }

        private void OnDisable()
        {
            releaseRoutine = null;
        }

        private void OnDestroy()
        {
            if (activeInstance == this)
            {
                activeInstance = null;
            }
        }

        private Entry FindInactiveEntry(GameObject prefab)
        {
            for (int index = 0; index < entries.Count; index++)
            {
                Entry entry = entries[index];
                if (entry != null && entry.Prefab == prefab && entry.Root != null && !entry.Active)
                {
                    return entry;
                }
            }

            return null;
        }

        private Entry CreateEntry(GameObject prefab)
        {
            GameObject instance = Instantiate(prefab, transform);
            instance.name = $"{prefab.name}_Pooled_{GetPoolSize(prefab):00}";
            Entry entry = new Entry
            {
                Prefab = prefab,
                Root = instance,
                Particles = instance.GetComponentsInChildren<ParticleSystem>(includeInactive: true),
                VisualEffects = instance.GetComponentsInChildren<VisualEffect>(includeInactive: true),
                BaseScale = instance.transform.localScale
            };
            instance.SetActive(false);
            entries.Add(entry);
            return entry;
        }

        private void ReleaseExpired()
        {
            float now = Time.time;
            for (int index = 0; index < entries.Count; index++)
            {
                Entry entry = entries[index];
                if (entry != null && entry.Active && now >= entry.ReleaseTime)
                {
                    Release(entry);
                }
            }
        }

        private void EnsureReleaseRoutine()
        {
            if (!isActiveAndEnabled || releaseRoutine != null || !HasActiveEntries())
            {
                return;
            }

            releaseRoutine = StartCoroutine(RunReleaseRoutine());
        }

        private IEnumerator RunReleaseRoutine()
        {
            while (HasActiveEntries())
            {
                yield return null;
                ReleaseExpired();
            }

            releaseRoutine = null;
        }

        private bool HasActiveEntries()
        {
            for (int index = 0; index < entries.Count; index++)
            {
                if (entries[index] != null && entries[index].Active)
                {
                    return true;
                }
            }

            return false;
        }

        private static void Release(Entry entry)
        {
            entry.Active = false;
            entry.ReleaseTime = 0f;
            if (entry.Root == null)
            {
                return;
            }

            StopParticles(entry.Particles);
            StopVisualEffects(entry.VisualEffects);
            entry.Root.SetActive(false);
            entry.Root.transform.localScale = entry.BaseScale;
        }

        private static void RestartParticles(ParticleSystem[] particles)
        {
            for (int index = 0; index < particles.Length; index++)
            {
                ParticleSystem particle = particles[index];
                if (particle == null)
                {
                    continue;
                }

                particle.Clear(withChildren: true);
                particle.Play(withChildren: true);
            }
        }

        private static void StopParticles(ParticleSystem[] particles)
        {
            for (int index = 0; index < particles.Length; index++)
            {
                ParticleSystem particle = particles[index];
                if (particle != null)
                {
                    particle.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
            }
        }

        private static void RestartVisualEffects(VisualEffect[] visualEffects)
        {
            for (int index = 0; index < visualEffects.Length; index++)
            {
                VisualEffect visualEffect = visualEffects[index];
                if (visualEffect == null)
                {
                    continue;
                }

                visualEffect.Stop();
                visualEffect.Reinit();
                visualEffect.Play();
            }
        }

        private static void StopVisualEffects(VisualEffect[] visualEffects)
        {
            for (int index = 0; index < visualEffects.Length; index++)
            {
                VisualEffect visualEffect = visualEffects[index];
                if (visualEffect != null)
                {
                    visualEffect.Stop();
                }
            }
        }
    }
}
