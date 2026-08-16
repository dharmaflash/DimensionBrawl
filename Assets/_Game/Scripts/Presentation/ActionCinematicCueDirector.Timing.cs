using UnityEngine;

namespace DimensionBrawl.Presentation
{
    public sealed partial class ActionCinematicCueDirector
    {
        private float lastAppliedTimeScale = 1f;

        private void ApplyTimeScale(float timeScale)
        {
            if (!hasStoredTimeScale)
            {
                storedTimeScale = Time.timeScale;
                hasStoredTimeScale = true;
            }

            lastAppliedTimeScale = Mathf.Clamp(timeScale, 0.05f, 1f);
            Time.timeScale = lastAppliedTimeScale;
        }

        private float TickTimeScaleTimer(float timer, float deltaTime)
        {
            if (!hasStoredTimeScale || timer <= 0f)
            {
                return timer;
            }

            timer = Mathf.Max(0f, timer - Mathf.Max(0f, deltaTime));
            if (timer <= 0f)
            {
                RestoreTimeScale();
            }

            return timer;
        }

        private void RestoreTimeScale()
        {
            if (!hasStoredTimeScale)
            {
                return;
            }

            // Time scale is shared global state. Restore only while the value is
            // still the one this director authored; an external lethal hit-stop
            // may have taken ownership after the cinematic began.
            bool stillOwnsCurrentValue = Mathf.Approximately(
                Time.timeScale,
                lastAppliedTimeScale);
            float restoreValue = storedTimeScale;
            hasStoredTimeScale = false;
            lastAppliedTimeScale = 1f;
            if (stillOwnsCurrentValue)
            {
                Time.timeScale = restoreValue;
            }
        }
    }
}
