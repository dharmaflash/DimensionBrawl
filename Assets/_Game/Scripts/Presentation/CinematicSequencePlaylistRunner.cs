using System;
using System.Collections;
using UnityEngine;

namespace DimensionBrawl.Presentation
{
    [DisallowMultipleComponent]
    public sealed class CinematicSequencePlaylistRunner : MonoBehaviour
    {
        [Serializable]
        public struct PlaylistEntry
        {
            [SerializeField] private CinematicSequenceProfile profile;
            [SerializeField, Min(0f)] private float delayAfterSeconds;
            [SerializeField] private bool usePlanarDirectionOverride;
            [SerializeField] private Vector3 planarDirectionOverride;

            public CinematicSequenceProfile Profile => profile;
            public float DelayAfterSeconds => Mathf.Max(0f, delayAfterSeconds);
            public bool UsePlanarDirectionOverride => usePlanarDirectionOverride;
            public Vector3 PlanarDirectionOverride => planarDirectionOverride;
        }

        [SerializeField] private CinematicSequenceRunner runner;
        [SerializeField] private PlaylistEntry[] entries = Array.Empty<PlaylistEntry>();
        [SerializeField] private bool playOnStart;
        [SerializeField, Min(0f)] private float startDelaySeconds;
        [SerializeField] private bool loop;

        private Coroutine activeRoutine;
        private int completedEntryCount;
        private string lastCompletedProfileId;

        public bool IsPlaying => activeRoutine != null;
        public int EntryCount => entries != null ? entries.Length : 0;
        public int CompletedEntryCount => completedEntryCount;
        public string LastCompletedProfileId => lastCompletedProfileId;

        private void Awake()
        {
            if (runner == null)
            {
                runner = GetComponent<CinematicSequenceRunner>();
            }
        }

        private void Start()
        {
            if (playOnStart)
            {
                TryPlay();
            }
        }

        private void OnDisable()
        {
            Stop();
        }

        public bool TryPlay()
        {
            if (activeRoutine != null || runner == null || EntryCount == 0)
            {
                return false;
            }

            completedEntryCount = 0;
            lastCompletedProfileId = string.Empty;
            activeRoutine = StartCoroutine(PlayRoutine());
            return true;
        }

        public void Stop()
        {
            if (activeRoutine != null)
            {
                StopCoroutine(activeRoutine);
                activeRoutine = null;
            }

            runner?.Stop();
        }

        private IEnumerator PlayRoutine()
        {
            if (startDelaySeconds > 0f)
            {
                yield return new WaitForSeconds(startDelaySeconds);
            }

            do
            {
                PlaylistEntry[] resolvedEntries = entries ?? Array.Empty<PlaylistEntry>();
                for (int i = 0; i < resolvedEntries.Length; i++)
                {
                    PlaylistEntry entry = resolvedEntries[i];
                    CinematicSequenceProfile profile = entry.Profile;
                    if (profile == null)
                    {
                        continue;
                    }

                    Vector3 direction = ResolvePlanarDirection(entry);
                    if (!runner.TryPlayProfile(profile, direction))
                    {
                        continue;
                    }

                    while (runner.IsPlaying)
                    {
                        yield return null;
                    }

                    completedEntryCount++;
                    lastCompletedProfileId = profile.SequenceId;

                    float delayAfter = entry.DelayAfterSeconds;
                    if (delayAfter > 0f)
                    {
                        yield return new WaitForSeconds(delayAfter);
                    }
                }
            }
            while (loop);

            activeRoutine = null;
        }

        private Vector3 ResolvePlanarDirection(PlaylistEntry entry)
        {
            Vector3 direction = entry.UsePlanarDirectionOverride
                ? entry.PlanarDirectionOverride
                : transform.forward;
            direction = Vector3.ProjectOnPlane(direction, Vector3.up);
            return direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
        }
    }
}
