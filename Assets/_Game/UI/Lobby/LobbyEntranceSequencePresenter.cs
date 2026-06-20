using System;
using System.Collections;
using UnityEngine;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    public sealed class LobbyEntranceSequencePresenter : MonoBehaviour
    {
        [Serializable]
        private struct SequenceEntry
        {
            [SerializeField] private UIMotionPresenter motionPresenter;
            [SerializeField] private string motionId;
            [SerializeField, Min(0f)] private float startDelaySeconds;

            public UIMotionPresenter MotionPresenter => motionPresenter;
            public string MotionId => motionId;
            public float StartDelaySeconds => startDelaySeconds;
        }

        [SerializeField] private SequenceEntry[] entries = Array.Empty<SequenceEntry>();
        [SerializeField, Min(0f)] private float initialDelaySeconds = 0.08f;
        [SerializeField] private bool playOnEnable = true;
        [SerializeField] private bool replayOnEnable;
        [SerializeField] private bool snapToEndOnDisable = true;
        [SerializeField] private bool useUnscaledTime = true;

        private Coroutine playRoutine;
        private bool hasPlayed;

        private void OnEnable()
        {
            if (playOnEnable && (replayOnEnable || !hasPlayed))
            {
                Play();
            }
        }

        private void OnDisable()
        {
            if (playRoutine != null)
            {
                StopCoroutine(playRoutine);
                playRoutine = null;
            }

            if (snapToEndOnDisable)
            {
                SnapToEnd();
            }
        }

        public void Play()
        {
            if (playRoutine != null)
            {
                StopCoroutine(playRoutine);
            }

            ApplyStartStates();
            playRoutine = StartCoroutine(PlayRoutine());
            hasPlayed = true;
        }

        public void ResetSequenceState()
        {
            hasPlayed = false;
        }

        private IEnumerator PlayRoutine()
        {
            if (initialDelaySeconds > 0f)
            {
                yield return WaitSeconds(initialDelaySeconds);
            }

            float previousDelay = 0f;

            for (int i = 0; i < entries.Length; i++)
            {
                SequenceEntry entry = entries[i];
                if (!IsValid(entry))
                {
                    continue;
                }

                float currentDelay = Mathf.Max(0f, entry.StartDelaySeconds);
                float waitSeconds = currentDelay - previousDelay;
                if (waitSeconds > 0f)
                {
                    yield return WaitSeconds(waitSeconds);
                }

                entry.MotionPresenter.PlayMotion(entry.MotionId);
                previousDelay = currentDelay;
            }

            playRoutine = null;
        }

        private void ApplyStartStates()
        {
            for (int i = 0; i < entries.Length; i++)
            {
                SequenceEntry entry = entries[i];
                if (IsValid(entry))
                {
                    entry.MotionPresenter.ApplyStartState(entry.MotionId);
                }
            }
        }

        private void SnapToEnd()
        {
            for (int i = 0; i < entries.Length; i++)
            {
                SequenceEntry entry = entries[i];
                if (IsValid(entry))
                {
                    entry.MotionPresenter.ApplyEndState(entry.MotionId);
                }
            }
        }

        private IEnumerator WaitSeconds(float seconds)
        {
            if (useUnscaledTime)
            {
                yield return new WaitForSecondsRealtime(seconds);
                yield break;
            }

            yield return new WaitForSeconds(seconds);
        }

        private static bool IsValid(SequenceEntry entry)
        {
            return entry.MotionPresenter != null && !string.IsNullOrWhiteSpace(entry.MotionId);
        }
    }
}
