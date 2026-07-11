using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DimensionBrawl.Presentation
{
    [DisallowMultipleComponent]
    public sealed class SpatialOneShotAudioPool : MonoBehaviour
    {
        private sealed class Entry
        {
            public GameObject Root;
            public AudioSource Source;
            public float ReleaseTime;
            public bool Active;
        }

        [SerializeField, Range(1, 32)] private int prewarmCount = 8;

        private readonly List<Entry> entries = new List<Entry>(12);
        private Coroutine releaseRoutine;

        public int PoolSize => entries.Count;
        public int ActiveCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < entries.Count; i++)
                {
                    if (entries[i] != null && entries[i].Active)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        private void Awake()
        {
            Prewarm();
        }

        private void OnDisable()
        {
            releaseRoutine = null;
            ReleaseAll();
        }

        private void OnDestroy()
        {
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i]?.Root != null)
                {
                    Destroy(entries[i].Root);
                }
            }

            entries.Clear();
        }

        public void ConfigurePrewarmCount(int count)
        {
            prewarmCount = Mathf.Clamp(count, 1, 32);
            Prewarm();
        }

        public bool Play(
            AudioClip clip,
            Vector3 position,
            float volume,
            float pitch,
            float spatialBlend,
            float minDistance,
            float maxDistance,
            int priority)
        {
            if (clip == null || volume <= 0f)
            {
                return false;
            }

            Entry entry = Acquire();
            if (entry == null || entry.Root == null || entry.Source == null)
            {
                return false;
            }

            float safePitch = Mathf.Max(0.01f, Mathf.Abs(pitch));
            entry.Root.transform.SetParent(null, worldPositionStays: false);
            entry.Root.transform.position = position;
            entry.Root.SetActive(true);

            AudioSource source = entry.Source;
            source.Stop();
            source.clip = clip;
            source.playOnAwake = false;
            source.loop = false;
            source.volume = Mathf.Clamp01(volume);
            source.pitch = safePitch;
            source.spatialBlend = Mathf.Clamp01(spatialBlend);
            source.dopplerLevel = 0f;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.minDistance = Mathf.Max(0.01f, minDistance);
            source.maxDistance = Mathf.Max(source.minDistance, maxDistance);
            source.priority = Mathf.Clamp(priority, 0, 256);
            source.Play();

            entry.Active = true;
            entry.ReleaseTime = Time.time + clip.length / safePitch + 0.1f;
            EnsureReleaseRoutine();
            return true;
        }

        public void ReleaseAll()
        {
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i] != null && entries[i].Active)
                {
                    Release(entries[i]);
                }
            }
        }

        private void Prewarm()
        {
            while (entries.Count < Mathf.Max(1, prewarmCount))
            {
                CreateEntry();
            }
        }

        private Entry Acquire()
        {
            ReleaseExpired();
            for (int i = 0; i < entries.Count; i++)
            {
                Entry entry = entries[i];
                if (entry != null && entry.Root != null && !entry.Active)
                {
                    return entry;
                }
            }

            return CreateEntry();
        }

        private Entry CreateEntry()
        {
            GameObject root = new GameObject($"SpatialOneShotAudio_Pooled_{entries.Count:00}");
            root.transform.SetParent(transform, worldPositionStays: false);
            AudioSource source = root.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            root.SetActive(false);
            Entry entry = new Entry
            {
                Root = root,
                Source = source
            };
            entries.Add(entry);
            return entry;
        }

        private void ReleaseExpired()
        {
            float now = Time.time;
            for (int i = 0; i < entries.Count; i++)
            {
                Entry entry = entries[i];
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
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i] != null && entries[i].Active)
                {
                    return true;
                }
            }

            return false;
        }

        private void Release(Entry entry)
        {
            entry.Active = false;
            entry.ReleaseTime = 0f;
            if (entry.Root == null || entry.Source == null)
            {
                return;
            }

            entry.Source.Stop();
            entry.Source.clip = null;
            entry.Root.SetActive(false);
            if (isActiveAndEnabled && gameObject.activeInHierarchy)
            {
                entry.Root.transform.SetParent(transform, worldPositionStays: false);
            }
        }
    }
}
