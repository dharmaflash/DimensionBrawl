using UnityEngine;

namespace DimensionBrawl.Presentation
{
    public sealed partial class ActionCinematicCueDirector
    {
        private void ApplyTimeScale(float timeScale)
        {
            if (!hasStoredTimeScale)
            {
                storedTimeScale = Time.timeScale;
                hasStoredTimeScale = true;
            }

            Time.timeScale = Mathf.Clamp(timeScale, 0.05f, 1f);
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

            Time.timeScale = storedTimeScale;
            hasStoredTimeScale = false;
        }
    }
}
