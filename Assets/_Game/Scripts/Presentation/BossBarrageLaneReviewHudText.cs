using DimensionBrawl.Combat;
using DimensionBrawl.LevelDesign;
using UnityEngine;

namespace DimensionBrawl.Presentation
{
    internal readonly struct BossBarrageFrontlineReadout
    {
        public BossBarrageFrontlineReadout(
            string state,
            int allyCount,
            int enemyCount,
            string allyHealthText,
            string enemyHealthText)
        {
            State = state;
            AllyCount = allyCount;
            EnemyCount = enemyCount;
            AllyHealthText = allyHealthText;
            EnemyHealthText = enemyHealthText;
        }

        public string State { get; }
        public int AllyCount { get; }
        public int EnemyCount { get; }
        public string AllyHealthText { get; }
        public string EnemyHealthText { get; }
    }

    internal static class BossBarrageLaneReviewHudText
    {
        public static bool IsEmptyHintLine(string hintLine)
        {
            return string.IsNullOrWhiteSpace(hintLine) || string.Equals(hintLine, "Hint: -", System.StringComparison.Ordinal);
        }

        public static string ResolveCompactPhaseLabel(string phase)
        {
            return phase switch
            {
                "ThreatDefense" => "Threat",
                "SummonBlock" => "Summon Block",
                "SummonFollowup" => "Follow-up",
                "PressureBreak" => "Break",
                "CounterWave" => "Counter",
                "Cleared" => "Clear",
                "Failed" => "Fail",
                _ => phase
            };
        }

        public static string ResolveCompactRiskBandLabel(SummonEnergyRiskBand riskBand)
        {
            return riskBand switch
            {
                SummonEnergyRiskBand.BackSafety => "Back",
                SummonEnergyRiskBand.MidCharge => "Mid",
                SummonEnergyRiskBand.ForwardRisk => "Front",
                _ => riskBand.ToString()
            };
        }

        public static string ShortenPatternId(string patternId)
        {
            if (string.IsNullOrWhiteSpace(patternId))
            {
                return "-";
            }

            const int maxLength = 18;
            return patternId.Length <= maxLength
                ? patternId
                : patternId.Substring(0, maxLength);
        }

        public static string ResolveRiskBandLabel(SummonEnergyRiskBand riskBand)
        {
            return riskBand switch
            {
                SummonEnergyRiskBand.ForwardRisk => "ForwardRisk",
                SummonEnergyRiskBand.MidCharge => "MidCharge",
                _ => "BackSafety"
            };
        }

        public static string ResolveActiveFrontlineTuningText()
        {
            int allyCount = 0;
            int enemyCount = 0;
            string allyText = "--";
            string enemyText = "--";
            int proxyCount = SummonFrontlineProxy.ActiveRegisteredProxyCount;
            for (int i = 0; i < proxyCount; i++)
            {
                if (!SummonFrontlineProxy.TryGetActiveRegisteredProxy(i, out SummonFrontlineProxy proxy)
                    || proxy == null
                    || proxy.Health == null)
                {
                    continue;
                }

                if (CombatTeamUtility.IsPlayerSide(proxy.Health.Team))
                {
                    allyCount++;
                    if (allyCount == 1)
                    {
                        allyText = ResolveFrontlineUnitTuningText(proxy);
                    }
                }
                else
                {
                    enemyCount++;
                    if (enemyCount == 1)
                    {
                        enemyText = ResolveFrontlineUnitTuningText(proxy);
                    }
                }
            }

            return $"A{allyCount} {allyText}   E{enemyCount} {enemyText}";
        }

        public static BossBarrageFrontlineReadout ResolveFrontlineProxyReadout()
        {
            int allyCount = 0;
            int enemyCount = 0;
            bool allyAdvancing = false;
            bool enemyAdvancing = false;
            bool clashing = false;
            float allyLowestHealth01 = 1f;
            float enemyLowestHealth01 = 1f;

            int proxyCount = SummonFrontlineProxy.ActiveRegisteredProxyCount;
            for (int i = 0; i < proxyCount; i++)
            {
                if (!SummonFrontlineProxy.TryGetActiveRegisteredProxy(i, out SummonFrontlineProxy proxy)
                    || proxy == null
                    || proxy.Health == null)
                {
                    continue;
                }

                bool isPlayerSide = CombatTeamUtility.IsPlayerSide(proxy.Health.Team);
                if (isPlayerSide)
                {
                    allyCount++;
                    allyAdvancing |= proxy.IsAdvancing;
                    allyLowestHealth01 = Mathf.Min(allyLowestHealth01, proxy.HealthRatio);
                }
                else
                {
                    enemyCount++;
                    enemyAdvancing |= proxy.IsAdvancing;
                    enemyLowestHealth01 = Mathf.Min(enemyLowestHealth01, proxy.HealthRatio);
                }

                clashing |= proxy.IsAdvanceHeld || proxy.CurrentState == SummonFrontlineProxyState.Attacking;
            }

            string state = ResolveFrontlineState(allyCount, enemyCount, allyAdvancing, enemyAdvancing, clashing);
            return new BossBarrageFrontlineReadout(
                state,
                allyCount,
                enemyCount,
                ResolveHealthPercentText(allyCount, allyLowestHealth01),
                ResolveHealthPercentText(enemyCount, enemyLowestHealth01));
        }

        public static string ResolveSummonLifecycleLine(
            int activeActorCount,
            float remainingLifetimeSeconds,
            float advanceProgress01,
            bool hasHealth,
            float healthRatio,
            bool isClashing,
            int clashCount,
            SummonFrontlineProxyExitReason exitReason)
        {
            if (activeActorCount > 0)
            {
                string health = hasHealth ? $" hp {healthRatio * 100f:0}%" : " hp --";
                string clash = clashCount > 0 || isClashing
                    ? $" clash {clashCount}{(isClashing ? "*" : string.Empty)}"
                    : string.Empty;
                string lifetime = float.IsPositiveInfinity(remainingLifetimeSeconds)
                    ? " life hold"
                    : $" life {remainingLifetimeSeconds:0.0}s";
                return $"{lifetime} adv {advanceProgress01 * 100f:0}%{health}{clash}";
            }

            return exitReason == SummonFrontlineProxyExitReason.None
                ? " idle"
                : $" exit {ResolveSummonExitReasonLabel(exitReason)}";
        }

        private static string ResolveFrontlineUnitTuningText(SummonFrontlineProxy proxy)
        {
            if (proxy == null)
            {
                return "--";
            }

            SummonFrontlineClash clash = proxy.GetComponent<SummonFrontlineClash>();
            string health = proxy.HasHealth ? $"{proxy.CurrentHealth:0}/{proxy.MaxHealth:0}" : "--";
            string dps = clash != null ? $"{clash.ContactDamagePerSecond:0}" : "--";
            return $"T{proxy.ActiveTier} {proxy.CurrentState} hp {health} spd {proxy.ActiveMoveSpeed:0.00} dps {dps}";
        }

        private static string ResolveFrontlineState(
            int allyCount,
            int enemyCount,
            bool allyAdvancing,
            bool enemyAdvancing,
            bool clashing)
        {
            if (allyCount > 0 && enemyCount > 0)
            {
                return clashing ? "clash" : "contest";
            }

            if (allyCount > 0)
            {
                return allyAdvancing ? "ally push" : "ally hold";
            }

            if (enemyCount > 0)
            {
                return enemyAdvancing ? "boss push" : "boss hold";
            }

            return "open";
        }

        private static string ResolveHealthPercentText(int count, float health01)
        {
            return count > 0 ? $"{Mathf.Clamp01(health01) * 100f:0}%" : "--";
        }

        private static string ResolveSummonExitReasonLabel(SummonFrontlineProxyExitReason exitReason)
        {
            return exitReason switch
            {
                SummonFrontlineProxyExitReason.LifetimeExpired => "time",
                SummonFrontlineProxyExitReason.Defeated => "defeated",
                SummonFrontlineProxyExitReason.Recalled => "recalled",
                SummonFrontlineProxyExitReason.Suppressed => "suppressed",
                _ => "-"
            };
        }
    }
}
