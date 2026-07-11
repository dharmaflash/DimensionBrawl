using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace DimensionBrawl.Presentation
{
    [DisallowMultipleComponent]
    public sealed class CombatVfxCuePlayer : MonoBehaviour
    {
        private sealed class CueInstanceComponents
        {
            public CombatVfxCueVisual[] CueVisuals;
            public ParticleSystem[] Particles;
            public VisualEffect[] VisualEffects;
            public CombatVfxCueAudioRandomizer[] AudioRandomizers;
            public AudioSource[] AudioSources;
        }

        private sealed class PooledAudioSource
        {
            public GameObject Root;
            public AudioSource Source;
            public float ReleaseTime;
            public bool Active;
        }

        private struct ScheduledCueRelease
        {
            public GameObject Instance;
            public float ReleaseTime;
        }

        [SerializeField] private CombatVfxCueProfile profile;
        [SerializeField] private Transform pooledRoot;
        [SerializeField, Range(1, 16)] private int profileAudioPrewarmCount = 4;

        private readonly Dictionary<GameObject, Queue<GameObject>> poolsByPrefab = new Dictionary<GameObject, Queue<GameObject>>();
        private readonly Dictionary<GameObject, GameObject> prefabByInstance = new Dictionary<GameObject, GameObject>();
        private readonly Dictionary<GameObject, CueInstanceComponents> componentsByInstance =
            new Dictionary<GameObject, CueInstanceComponents>();
        private readonly List<ScheduledCueRelease> scheduledCueReleases = new List<ScheduledCueRelease>(16);
        private readonly List<PooledAudioSource> profileAudioPool = new List<PooledAudioSource>(8);
        private readonly HashSet<AudioSource> randomizedAudioSources = new HashSet<AudioSource>();
        private int activeProfileAudioSourceCount;
        private Coroutine releaseRoutine;

        public CombatVfxCueProfile Profile => profile;
        public int ProfileAudioPoolSize => profileAudioPool.Count;
        public int ActiveProfileAudioSourceCount => activeProfileAudioSourceCount;
        public int ScheduledCueReleaseCount => scheduledCueReleases.Count;

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
            PrewarmProfileAudioSources();
        }

        private void OnEnable()
        {
            StartReleaseRoutineIfNeeded();
        }

        private void OnDisable()
        {
            StopReleaseRoutine();
            ReleaseAllProfileAudioSources();
        }

        private void OnDestroy()
        {
            StopReleaseRoutine();
            for (int i = 0; i < profileAudioPool.Count; i++)
            {
                if (profileAudioPool[i]?.Root != null)
                {
                    Destroy(profileAudioPool[i].Root);
                }
            }

            profileAudioPool.Clear();
            activeProfileAudioSourceCount = 0;
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
            float embeddedAudioTailSeconds = PlayEffects(instance, resolvedAudioScale);
            PlayCueAudio(cue, anchor, instanceTransform.position, resolvedAudioScale);

            float releaseSeconds = Mathf.Max(cue.LifetimeSeconds, embeddedAudioTailSeconds);
            if (releaseSeconds > 0f)
            {
                scheduledCueReleases.Add(new ScheduledCueRelease
                {
                    Instance = instance,
                    ReleaseTime = Time.time + releaseSeconds
                });
                StartReleaseRoutineIfNeeded();
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

            scheduledCueReleases.Clear();
            foreach (KeyValuePair<GameObject, GameObject> pair in prefabByInstance)
            {
                GameObject instance = pair.Key;
                GameObject prefab = pair.Value;
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
            componentsByInstance[instance] = new CueInstanceComponents
            {
                CueVisuals = instance.GetComponentsInChildren<CombatVfxCueVisual>(includeInactive: true),
                Particles = instance.GetComponentsInChildren<ParticleSystem>(includeInactive: true),
                VisualEffects = instance.GetComponentsInChildren<VisualEffect>(includeInactive: true),
                AudioRandomizers = instance.GetComponentsInChildren<CombatVfxCueAudioRandomizer>(includeInactive: true),
                AudioSources = instance.GetComponentsInChildren<AudioSource>(includeInactive: true)
            };
            instance.SetActive(false);
            return instance;
        }

        private void ReleaseExpiredCueInstances()
        {
            float now = Time.time;
            for (int i = scheduledCueReleases.Count - 1; i >= 0; i--)
            {
                ScheduledCueRelease scheduled = scheduledCueReleases[i];
                if (now < scheduled.ReleaseTime)
                {
                    continue;
                }

                scheduledCueReleases.RemoveAt(i);
                ReleaseInstance(scheduled.Instance);
            }
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

        private float PlayEffects(GameObject instance, float audioScale)
        {
            CueInstanceComponents components = ResolveInstanceComponents(instance);
            float audioTailSeconds = 0f;
            for (int i = 0; i < components.CueVisuals.Length; i++)
            {
                components.CueVisuals[i].Restart();
            }

            for (int i = 0; i < components.Particles.Length; i++)
            {
                components.Particles[i].Clear(withChildren: true);
                components.Particles[i].Play(withChildren: true);
            }

            for (int i = 0; i < components.VisualEffects.Length; i++)
            {
                components.VisualEffects[i].Reinit();
                components.VisualEffects[i].Play();
            }

            randomizedAudioSources.Clear();
            for (int i = 0; i < components.AudioRandomizers.Length; i++)
            {
                CombatVfxCueAudioRandomizer audioRandomizer = components.AudioRandomizers[i];
                if (audioRandomizer == null || !audioRandomizer.Play(audioScale) || audioRandomizer.Source == null)
                {
                    continue;
                }

                randomizedAudioSources.Add(audioRandomizer.Source);
                audioTailSeconds = Mathf.Max(audioTailSeconds, audioRandomizer.LastPlayedClipDurationSeconds);
            }

            for (int i = 0; i < components.AudioSources.Length; i++)
            {
                AudioSource audioSource = components.AudioSources[i];
                if (randomizedAudioSources.Contains(audioSource))
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

        private void PlayCueAudio(CombatVfxCue cue, Transform anchor, Vector3 fallbackPosition, float audioScale)
        {
            if (cue.AudioClipCount <= 0 || cue.AudioBaseVolume <= 0f)
            {
                return;
            }

            AudioClip clip = PickCueAudioClip(cue);
            if (clip == null)
            {
                return;
            }

            float pitch = Random.Range(cue.AudioMinimumPitch, cue.AudioMaximumPitch);
            if (pitch <= 0.001f)
            {
                pitch = 1f;
            }

            PooledAudioSource pooledAudio = AcquireProfileAudioSource();
            if (pooledAudio == null || pooledAudio.Root == null || pooledAudio.Source == null)
            {
                return;
            }

            pooledAudio.Root.transform.SetParent(null, worldPositionStays: false);
            pooledAudio.Root.transform.position = anchor != null ? anchor.position : fallbackPosition;
            pooledAudio.Root.SetActive(true);
            AudioSource audioSource = pooledAudio.Source;
            audioSource.clip = clip;
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.volume = cue.AudioBaseVolume
                * Mathf.Max(0f, audioScale)
                * Random.Range(cue.AudioMinimumVolumeMultiplier, cue.AudioMaximumVolumeMultiplier);
            audioSource.pitch = pitch;
            audioSource.spatialBlend = cue.AudioSpatialBlend;
            audioSource.dopplerLevel = 0f;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.minDistance = cue.AudioMinDistance;
            audioSource.maxDistance = cue.AudioMaxDistance;
            audioSource.priority = cue.AudioPriority;
            audioSource.Play();
            pooledAudio.Active = true;
            activeProfileAudioSourceCount++;
            pooledAudio.ReleaseTime = Time.time + clip.length / pitch + 0.1f;
            StartReleaseRoutineIfNeeded();
        }

        private IEnumerator ReleaseScheduledContentUntilIdle()
        {
            yield return null;

            while (isActiveAndEnabled && HasScheduledContent())
            {
                ReleaseExpiredCueInstances();
                ReleaseExpiredProfileAudioSources();
                if (!HasScheduledContent())
                {
                    break;
                }

                yield return null;
            }

            releaseRoutine = null;
        }

        private bool HasScheduledContent()
        {
            return scheduledCueReleases.Count > 0 || activeProfileAudioSourceCount > 0;
        }

        private void StartReleaseRoutineIfNeeded()
        {
            if (releaseRoutine == null && Application.isPlaying && isActiveAndEnabled && HasScheduledContent())
            {
                releaseRoutine = StartCoroutine(ReleaseScheduledContentUntilIdle());
            }
        }

        private void StopReleaseRoutine()
        {
            if (releaseRoutine == null)
            {
                return;
            }

            StopCoroutine(releaseRoutine);
            releaseRoutine = null;
        }

        private CueInstanceComponents ResolveInstanceComponents(GameObject instance)
        {
            if (componentsByInstance.TryGetValue(instance, out CueInstanceComponents components))
            {
                return components;
            }

            components = new CueInstanceComponents
            {
                CueVisuals = instance.GetComponentsInChildren<CombatVfxCueVisual>(includeInactive: true),
                Particles = instance.GetComponentsInChildren<ParticleSystem>(includeInactive: true),
                VisualEffects = instance.GetComponentsInChildren<VisualEffect>(includeInactive: true),
                AudioRandomizers = instance.GetComponentsInChildren<CombatVfxCueAudioRandomizer>(includeInactive: true),
                AudioSources = instance.GetComponentsInChildren<AudioSource>(includeInactive: true)
            };
            componentsByInstance[instance] = components;
            return components;
        }

        private void PrewarmProfileAudioSources()
        {
            if (!HasProfileAudioCues())
            {
                return;
            }

            int targetCount = Mathf.Max(1, profileAudioPrewarmCount);
            while (profileAudioPool.Count < targetCount)
            {
                CreateProfileAudioSource();
            }
        }

        private bool HasProfileAudioCues()
        {
            if (profile == null)
            {
                return false;
            }

            int cueCount = System.Enum.GetValues(typeof(CombatVfxCueId)).Length;
            for (int i = 0; i < cueCount; i++)
            {
                CombatVfxCueId cueId = (CombatVfxCueId)i;
                if (profile.TryGetCue(cueId, out CombatVfxCue cue) && cue.AudioClipCount > 0)
                {
                    return true;
                }
            }

            return false;
        }

        private PooledAudioSource AcquireProfileAudioSource()
        {
            ReleaseExpiredProfileAudioSources();
            for (int i = 0; i < profileAudioPool.Count; i++)
            {
                PooledAudioSource pooledAudio = profileAudioPool[i];
                if (pooledAudio != null && pooledAudio.Root != null && !pooledAudio.Active)
                {
                    return pooledAudio;
                }
            }

            return CreateProfileAudioSource();
        }

        private PooledAudioSource CreateProfileAudioSource()
        {
            Transform parent = pooledRoot != null ? pooledRoot : transform;
            GameObject root = new GameObject($"CombatVfxCueAudio_Pooled_{profileAudioPool.Count:00}");
            root.transform.SetParent(parent, worldPositionStays: false);
            AudioSource source = root.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            root.SetActive(false);
            PooledAudioSource pooledAudio = new PooledAudioSource
            {
                Root = root,
                Source = source
            };
            profileAudioPool.Add(pooledAudio);
            return pooledAudio;
        }

        private void ReleaseExpiredProfileAudioSources()
        {
            if (activeProfileAudioSourceCount <= 0)
            {
                return;
            }

            float now = Time.time;
            for (int i = 0; i < profileAudioPool.Count; i++)
            {
                PooledAudioSource pooledAudio = profileAudioPool[i];
                if (pooledAudio != null && pooledAudio.Active && now >= pooledAudio.ReleaseTime)
                {
                    ReleaseProfileAudioSource(pooledAudio);
                }
            }
        }

        private void ReleaseAllProfileAudioSources()
        {
            if (activeProfileAudioSourceCount <= 0)
            {
                return;
            }

            for (int i = 0; i < profileAudioPool.Count; i++)
            {
                PooledAudioSource pooledAudio = profileAudioPool[i];
                if (pooledAudio != null && pooledAudio.Active)
                {
                    ReleaseProfileAudioSource(pooledAudio);
                }
            }
        }

        private void ReleaseProfileAudioSource(PooledAudioSource pooledAudio)
        {
            if (pooledAudio == null || !pooledAudio.Active)
            {
                return;
            }

            pooledAudio.Active = false;
            activeProfileAudioSourceCount = Mathf.Max(0, activeProfileAudioSourceCount - 1);
            pooledAudio.ReleaseTime = 0f;
            if (pooledAudio.Root == null || pooledAudio.Source == null)
            {
                return;
            }

            pooledAudio.Source.Stop();
            pooledAudio.Source.clip = null;
            pooledAudio.Root.SetActive(false);
            if (isActiveAndEnabled && gameObject.activeInHierarchy)
            {
                Transform parent = pooledRoot != null ? pooledRoot : transform;
                pooledAudio.Root.transform.SetParent(parent, worldPositionStays: false);
            }
        }

        private static AudioClip PickCueAudioClip(CombatVfxCue cue)
        {
            int clipCount = cue.AudioClipCount;
            if (clipCount <= 0)
            {
                return null;
            }

            int startIndex = Random.Range(0, clipCount);
            for (int i = 0; i < clipCount; i++)
            {
                AudioClip clip = cue.GetAudioClip((startIndex + i) % clipCount);
                if (clip != null)
                {
                    return clip;
                }
            }

            return null;
        }

        private void StopEffects(GameObject instance)
        {
            CueInstanceComponents components = ResolveInstanceComponents(instance);
            for (int i = 0; i < components.CueVisuals.Length; i++)
            {
                components.CueVisuals[i].StopNow();
            }

            for (int i = 0; i < components.Particles.Length; i++)
            {
                components.Particles[i].Stop(withChildren: true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            for (int i = 0; i < components.VisualEffects.Length; i++)
            {
                components.VisualEffects[i].Stop();
            }

            for (int i = 0; i < components.AudioSources.Length; i++)
            {
                components.AudioSources[i].Stop();
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
