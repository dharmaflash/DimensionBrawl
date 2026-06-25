using UnityEngine;

namespace DimensionBrawl.Test
{
    internal sealed class BossBarragePocketPressurePacing
    {
        private float closeThreatReliefTimer;
        private float summonPressureBreakTimer;
        private float summonFollowupWindowTimer;
        private bool closeThreatReliefActive;
        private bool summonPressureBreakActive;
        private bool summonFollowupWindowActive;

        public bool IsCloseThreatReliefActive => closeThreatReliefActive;
        public bool IsSummonPressureBreakActive => summonPressureBreakActive;
        public bool IsSummonFollowupWindowActive => summonFollowupWindowActive;
        public float CloseThreatReliefRemainingSeconds => closeThreatReliefTimer;
        public float SummonPressureBreakRemainingSeconds => summonPressureBreakTimer;
        public float SummonFollowupWindowRemainingSeconds => summonFollowupWindowTimer;
        public bool ShouldPauseBarrage => closeThreatReliefActive || summonPressureBreakActive;

        public void Reset()
        {
            closeThreatReliefTimer = 0f;
            summonPressureBreakTimer = 0f;
            summonFollowupWindowTimer = 0f;
            closeThreatReliefActive = false;
            summonPressureBreakActive = false;
            summonFollowupWindowActive = false;
        }

        public void StartCloseThreatRelief(float seconds)
        {
            closeThreatReliefTimer = Mathf.Max(0f, seconds);
            closeThreatReliefActive = closeThreatReliefTimer > 0f;
        }

        public void StartSummonPressureBreak(float reliefSeconds, float followupWindowSeconds)
        {
            summonPressureBreakTimer = Mathf.Max(0f, reliefSeconds);
            summonFollowupWindowTimer = Mathf.Max(0f, followupWindowSeconds);
            summonPressureBreakActive = summonPressureBreakTimer > 0f;
            summonFollowupWindowActive = summonFollowupWindowTimer > 0f;
        }

        public void StartSummonFollowupWindow(float followupWindowSeconds)
        {
            summonFollowupWindowTimer = Mathf.Max(0f, followupWindowSeconds);
            summonFollowupWindowActive = summonFollowupWindowTimer > 0f;
        }

        public void EndSummonFollowupWindow()
        {
            summonFollowupWindowTimer = 0f;
            summonFollowupWindowActive = false;
        }

        public void Tick(float deltaTime)
        {
            float safeDeltaTime = Mathf.Max(0f, deltaTime);
            TickCloseThreatRelief(safeDeltaTime);
            TickSummonPressureBreak(safeDeltaTime);
            TickSummonFollowupWindow(safeDeltaTime);
        }

        private void TickCloseThreatRelief(float deltaTime)
        {
            if (!closeThreatReliefActive)
            {
                return;
            }

            closeThreatReliefTimer = Mathf.Max(0f, closeThreatReliefTimer - deltaTime);
            if (closeThreatReliefTimer <= 0f)
            {
                closeThreatReliefActive = false;
            }
        }

        private void TickSummonPressureBreak(float deltaTime)
        {
            if (!summonPressureBreakActive)
            {
                return;
            }

            summonPressureBreakTimer = Mathf.Max(0f, summonPressureBreakTimer - deltaTime);
            if (summonPressureBreakTimer <= 0f)
            {
                summonPressureBreakActive = false;
            }
        }

        private void TickSummonFollowupWindow(float deltaTime)
        {
            if (!summonFollowupWindowActive)
            {
                return;
            }

            summonFollowupWindowTimer = Mathf.Max(0f, summonFollowupWindowTimer - deltaTime);
            if (summonFollowupWindowTimer <= 0f)
            {
                summonFollowupWindowActive = false;
            }
        }
    }
}
