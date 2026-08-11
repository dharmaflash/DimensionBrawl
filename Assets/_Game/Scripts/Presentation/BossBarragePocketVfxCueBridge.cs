using DimensionBrawl.Combat;
using UnityEngine;
using UnityEngine.Serialization;

namespace DimensionBrawl.Presentation
{
    public sealed class BossBarragePocketVfxCueBridge : MonoBehaviour
    {
        [Header("References")]
        [FormerlySerializedAs("pocketReviewOwner")]
        [SerializeField] private BossBarrageEncounterController encounterController;
        [SerializeField] private CombatVfxCuePlayer cuePlayer;
        [SerializeField] private BossBarrageVisualCueDriver bossVisualCueDriver;
        [SerializeField] private Transform followupWindowAnchor;
        [SerializeField] private Transform followupHitAnchor;
        [SerializeField] private Transform followupMissedAnchor;
        [SerializeField] private Transform bossScreenSuppressAnchor;
        [SerializeField] private Transform counterWaveAnchor;
        [SerializeField] private Transform counterWaveStabilizedAnchor;
        [SerializeField] private Transform pocketClearAnchor;
        [SerializeField] private Transform pocketFailAnchor;
        [SerializeField] private Transform directionTarget;

        [Header("Cue Intensity")]
        [SerializeField, Min(0f)] private float blockOpportunityIntensity = 1.05f;
        [SerializeField, Min(0f)] private float windowIntensity = 1.15f;
        [SerializeField, Min(0f)] private float hitIntensity = 1.18f;
        [SerializeField, Min(0f)] private float missedIntensity = 0.85f;
        [SerializeField, Min(0f)] private float bossScreenSuppressIntensity = 1.12f;
        [SerializeField, Min(0f)] private float counterWaveIntensity = 0.94f;
        [SerializeField, Min(0f)] private float counterWaveStabilizedIntensity = 0.88f;
        [SerializeField, Min(0f)] private float pocketClearIntensity = 0.92f;
        [SerializeField, Min(0f)] private float pocketFailIntensity = 1.02f;
        [SerializeField] private CombatVfxCueId bossScreenSuppressCueId = CombatVfxCueId.EliteArmorBreakSignal;
        [SerializeField] private CombatVfxCueId counterWaveCueId = CombatVfxCueId.EnemyLinePressureActive;
        [SerializeField] private CombatVfxCueId counterWaveStabilizedCueId = CombatVfxCueId.EliteShieldSignal;
        [SerializeField] private CombatVfxCueId pocketFailAccentCueId = CombatVfxCueId.EnemyClosePunishActive;
        [SerializeField, Min(0f)] private float pocketFailAccentIntensity = 0.88f;
        [SerializeField, Min(0f)] private float tierIntensityStep = 0.12f;

        private int summonBlockOpportunityCueRequestCount;
        private int followupWindowCueRequestCount;
        private int followupHitCueRequestCount;
        private int followupMissedCueRequestCount;
        private int bossScreenSuppressCueRequestCount;
        private int counterWaveCueRequestCount;
        private int counterWaveStabilizedCueRequestCount;
        private int pocketClearCueRequestCount;
        private int pocketFailCueRequestCount;
        private int pocketFailAccentCueRequestCount;
        private int lastFollowupWindowTier;
        private int lastFollowupHitTier;
        private int lastBossScreenSuppressTier;
        private float lastFollowupHitDamage;
        private BossBarrageEncounterController.CounterWaveSource lastCounterWaveSource =
            BossBarrageEncounterController.CounterWaveSource.None;

        public BossBarrageEncounterController EncounterController => encounterController;
        public CombatVfxCuePlayer CuePlayer => cuePlayer;
        public Transform FollowupWindowAnchor => followupWindowAnchor;
        public Transform FollowupHitAnchor => followupHitAnchor;
        public Transform FollowupMissedAnchor => followupMissedAnchor;
        public Transform BossScreenSuppressAnchor => bossScreenSuppressAnchor != null ? bossScreenSuppressAnchor : FollowupHitAnchor;
        public Transform CounterWaveAnchor => counterWaveAnchor != null ? counterWaveAnchor : FollowupMissedAnchor;
        public Transform CounterWaveStabilizedAnchor => counterWaveStabilizedAnchor != null ? counterWaveStabilizedAnchor : FollowupWindowAnchor;
        public Transform PocketClearAnchor => pocketClearAnchor != null ? pocketClearAnchor : followupHitAnchor;
        public Transform PocketFailAnchor => pocketFailAnchor != null ? pocketFailAnchor : followupMissedAnchor;
        public Transform DirectionTarget => directionTarget;
        public int SummonBlockOpportunityCueRequestCount => summonBlockOpportunityCueRequestCount;
        public int FollowupWindowCueRequestCount => followupWindowCueRequestCount;
        public int FollowupHitCueRequestCount => followupHitCueRequestCount;
        public int FollowupMissedCueRequestCount => followupMissedCueRequestCount;
        public int BossScreenSuppressCueRequestCount => bossScreenSuppressCueRequestCount;
        public int CounterWaveCueRequestCount => counterWaveCueRequestCount;
        public int CounterWaveStabilizedCueRequestCount => counterWaveStabilizedCueRequestCount;
        public int PocketClearCueRequestCount => pocketClearCueRequestCount;
        public int PocketFailCueRequestCount => pocketFailCueRequestCount;
        public int PocketFailAccentCueRequestCount => pocketFailAccentCueRequestCount;
        public int LastFollowupWindowTier => lastFollowupWindowTier;
        public int LastFollowupHitTier => lastFollowupHitTier;
        public int LastBossScreenSuppressTier => lastBossScreenSuppressTier;
        public float LastFollowupHitDamage => lastFollowupHitDamage;
        public BossBarrageEncounterController.CounterWaveSource LastCounterWaveSource => lastCounterWaveSource;
        public CombatVfxCueId CounterWaveCueId => counterWaveCueId;
        public CombatVfxCueId CounterWaveStabilizedCueId => counterWaveStabilizedCueId;
        public CombatVfxCueId BossScreenSuppressCueId => bossScreenSuppressCueId;
        public float HitIntensity => hitIntensity;
        public float BossScreenSuppressIntensity => bossScreenSuppressIntensity;
        public float CounterWaveIntensity => counterWaveIntensity;
        public float CounterWaveStabilizedIntensity => counterWaveStabilizedIntensity;
        public float PocketClearIntensity => pocketClearIntensity;
        public float PocketFailIntensity => pocketFailIntensity;
        public CombatVfxCueId PocketFailAccentCueId => pocketFailAccentCueId;
        public float PocketFailAccentIntensity => pocketFailAccentIntensity;
        public BossBarrageVisualCueDriver BossVisualCueDriver => bossVisualCueDriver;

        private void Awake()
        {
            if (encounterController == null)
            {
                encounterController = GetComponent<BossBarrageEncounterController>();
            }
        }

        private void OnEnable()
        {
            if (encounterController == null)
            {
                return;
            }

            encounterController.SummonFollowupWindowOpened += HandleSummonFollowupWindowOpened;
            encounterController.SummonFollowupHitConfirmed += HandleSummonFollowupHitConfirmed;
            encounterController.SummonFollowupMissed += HandleSummonFollowupMissed;
            encounterController.BossScreenSuppressedByFollowupConfirmed += HandleBossScreenSuppressedByFollowupConfirmed;
            encounterController.SummonBlockOpportunityOpened += HandleSummonBlockOpportunityOpened;
            encounterController.CounterWaveObserved += HandleCounterWaveObserved;
            encounterController.CounterWaveStabilized += HandleCounterWaveStabilized;
            encounterController.PocketCleared += HandlePocketCleared;
            encounterController.PocketFailed += HandlePocketFailed;
        }

        private void OnDisable()
        {
            if (encounterController == null)
            {
                return;
            }

            encounterController.SummonFollowupWindowOpened -= HandleSummonFollowupWindowOpened;
            encounterController.SummonFollowupHitConfirmed -= HandleSummonFollowupHitConfirmed;
            encounterController.SummonFollowupMissed -= HandleSummonFollowupMissed;
            encounterController.BossScreenSuppressedByFollowupConfirmed -= HandleBossScreenSuppressedByFollowupConfirmed;
            encounterController.SummonBlockOpportunityOpened -= HandleSummonBlockOpportunityOpened;
            encounterController.CounterWaveObserved -= HandleCounterWaveObserved;
            encounterController.CounterWaveStabilized -= HandleCounterWaveStabilized;
            encounterController.PocketCleared -= HandlePocketCleared;
            encounterController.PocketFailed -= HandlePocketFailed;
        }

        private void HandleSummonBlockOpportunityOpened()
        {
            if (Play(CombatVfxCueId.SummonBlockOpportunity, followupWindowAnchor, 1, blockOpportunityIntensity))
            {
                summonBlockOpportunityCueRequestCount++;
            }
        }

        private void HandleSummonFollowupWindowOpened(int tier)
        {
            lastFollowupWindowTier = tier;
            if (Play(CombatVfxCueId.SummonFollowupWindow, followupWindowAnchor, tier, windowIntensity))
            {
                followupWindowCueRequestCount++;
            }
        }

        private void HandleSummonFollowupHitConfirmed(int tier, float damage)
        {
            if (!(damage > 0f))
            {
                return;
            }

            lastFollowupHitTier = tier;
            lastFollowupHitDamage = damage;
            bossVisualCueDriver?.RequestFollowupHitReaction(tier, damage);
            if (Play(CombatVfxCueId.SummonFollowupHit, followupHitAnchor, tier, hitIntensity))
            {
                followupHitCueRequestCount++;
            }
        }

        private void HandleSummonFollowupMissed()
        {
            if (Play(CombatVfxCueId.SummonFollowupMissed, followupMissedAnchor, 1, missedIntensity))
            {
                followupMissedCueRequestCount++;
            }
        }

        private void HandleBossScreenSuppressedByFollowupConfirmed(int tier, int suppressedCount)
        {
            int resolvedTier = Mathf.Clamp(tier, 1, 3);
            float suppressedScale = Mathf.Clamp01(suppressedCount / 3f);
            float intensity = bossScreenSuppressIntensity + suppressedScale * 0.18f;
            if (Play(bossScreenSuppressCueId, BossScreenSuppressAnchor, resolvedTier, intensity))
            {
                bossScreenSuppressCueRequestCount++;
                lastBossScreenSuppressTier = resolvedTier;
            }
        }

        private void HandleCounterWaveObserved(BossBarrageEncounterController.CounterWaveSource source)
        {
            lastCounterWaveSource = source;
            if (Play(counterWaveCueId, CounterWaveAnchor, ResolveCounterWaveCueTier(source), ResolveCounterWaveCueIntensity(source)))
            {
                counterWaveCueRequestCount++;
            }
        }

        private void HandleCounterWaveStabilized()
        {
            if (Play(counterWaveStabilizedCueId, CounterWaveStabilizedAnchor, 2, counterWaveStabilizedIntensity))
            {
                counterWaveStabilizedCueRequestCount++;
            }
        }

        private void HandlePocketCleared()
        {
            if (Play(CombatVfxCueId.PocketCleared, PocketClearAnchor, 1, pocketClearIntensity))
            {
                pocketClearCueRequestCount++;
            }
        }

        private void HandlePocketFailed()
        {
            if (Play(CombatVfxCueId.PocketFailed, PocketFailAnchor, 1, pocketFailIntensity))
            {
                pocketFailCueRequestCount++;
            }

            if (Play(pocketFailAccentCueId, PocketFailAnchor, 1, pocketFailAccentIntensity))
            {
                pocketFailAccentCueRequestCount++;
            }
        }

        private bool Play(CombatVfxCueId cueId, Transform anchor, int tier, float baseIntensity)
        {
            if (cuePlayer == null || anchor == null)
            {
                return false;
            }

            return cuePlayer.PlayCue(cueId, anchor, ResolveCueDirection(anchor), ResolveTierIntensity(tier, baseIntensity));
        }

        private Vector3 ResolveCueDirection(Transform anchor)
        {
            if (directionTarget != null)
            {
                Vector3 targetDirection = Vector3.ProjectOnPlane(directionTarget.position - anchor.position, Vector3.up);
                if (targetDirection.sqrMagnitude > 0.0001f)
                {
                    return targetDirection.normalized;
                }
            }

            Vector3 forward = Vector3.ProjectOnPlane(anchor.forward, Vector3.up);
            return forward.sqrMagnitude > 0.0001f ? forward.normalized : Vector3.forward;
        }

        private float ResolveTierIntensity(int tier, float baseIntensity)
        {
            return Mathf.Max(0f, baseIntensity + Mathf.Max(0, tier - 1) * tierIntensityStep);
        }

        private static int ResolveCounterWaveCueTier(BossBarrageEncounterController.CounterWaveSource source)
        {
            return source == BossBarrageEncounterController.CounterWaveSource.BossSummonRelease
                || source == BossBarrageEncounterController.CounterWaveSource.FollowupMissed
                ? 2
                : 1;
        }

        private float ResolveCounterWaveCueIntensity(BossBarrageEncounterController.CounterWaveSource source)
        {
            return source == BossBarrageEncounterController.CounterWaveSource.BossSummonRelease
                || source == BossBarrageEncounterController.CounterWaveSource.FollowupMissed
                ? counterWaveIntensity + tierIntensityStep
                : counterWaveIntensity;
        }
    }
}
