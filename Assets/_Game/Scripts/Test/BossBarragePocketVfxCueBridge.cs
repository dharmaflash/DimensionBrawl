using DimensionBrawl.Presentation;
using UnityEngine;

namespace DimensionBrawl.Test
{
    public sealed class BossBarragePocketVfxCueBridge : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BossBarragePocketReviewOwner pocketReviewOwner;
        [SerializeField] private CombatVfxCuePlayer cuePlayer;
        [SerializeField] private Transform followupWindowAnchor;
        [SerializeField] private Transform followupHitAnchor;
        [SerializeField] private Transform followupMissedAnchor;
        [SerializeField] private Transform pocketClearAnchor;
        [SerializeField] private Transform pocketFailAnchor;
        [SerializeField] private Transform directionTarget;

        [Header("Cue Intensity")]
        [SerializeField, Min(0f)] private float blockOpportunityIntensity = 1.05f;
        [SerializeField, Min(0f)] private float windowIntensity = 1.15f;
        [SerializeField, Min(0f)] private float hitIntensity = 1.3f;
        [SerializeField, Min(0f)] private float missedIntensity = 0.85f;
        [SerializeField, Min(0f)] private float pocketClearIntensity = 1.42f;
        [SerializeField, Min(0f)] private float pocketFailIntensity = 1.48f;
        [SerializeField] private CombatVfxCueId pocketFailAccentCueId = CombatVfxCueId.EnemyClosePunishActive;
        [SerializeField, Min(0f)] private float pocketFailAccentIntensity = 1.32f;
        [SerializeField, Min(0f)] private float tierIntensityStep = 0.12f;

        private int summonBlockOpportunityCueRequestCount;
        private int followupWindowCueRequestCount;
        private int followupHitCueRequestCount;
        private int followupMissedCueRequestCount;
        private int pocketClearCueRequestCount;
        private int pocketFailCueRequestCount;
        private int pocketFailAccentCueRequestCount;
        private int lastFollowupWindowTier;
        private int lastFollowupHitTier;
        private float lastFollowupHitDamage;

        public BossBarragePocketReviewOwner PocketReviewOwner => pocketReviewOwner;
        public CombatVfxCuePlayer CuePlayer => cuePlayer;
        public Transform FollowupWindowAnchor => followupWindowAnchor;
        public Transform FollowupHitAnchor => followupHitAnchor;
        public Transform FollowupMissedAnchor => followupMissedAnchor;
        public Transform PocketClearAnchor => pocketClearAnchor != null ? pocketClearAnchor : followupHitAnchor;
        public Transform PocketFailAnchor => pocketFailAnchor != null ? pocketFailAnchor : followupMissedAnchor;
        public Transform DirectionTarget => directionTarget;
        public int SummonBlockOpportunityCueRequestCount => summonBlockOpportunityCueRequestCount;
        public int FollowupWindowCueRequestCount => followupWindowCueRequestCount;
        public int FollowupHitCueRequestCount => followupHitCueRequestCount;
        public int FollowupMissedCueRequestCount => followupMissedCueRequestCount;
        public int PocketClearCueRequestCount => pocketClearCueRequestCount;
        public int PocketFailCueRequestCount => pocketFailCueRequestCount;
        public int PocketFailAccentCueRequestCount => pocketFailAccentCueRequestCount;
        public int LastFollowupWindowTier => lastFollowupWindowTier;
        public int LastFollowupHitTier => lastFollowupHitTier;
        public float LastFollowupHitDamage => lastFollowupHitDamage;
        public float PocketClearIntensity => pocketClearIntensity;
        public float PocketFailIntensity => pocketFailIntensity;
        public CombatVfxCueId PocketFailAccentCueId => pocketFailAccentCueId;
        public float PocketFailAccentIntensity => pocketFailAccentIntensity;

        private void Awake()
        {
            if (pocketReviewOwner == null)
            {
                pocketReviewOwner = GetComponent<BossBarragePocketReviewOwner>();
            }
        }

        private void OnEnable()
        {
            if (pocketReviewOwner == null)
            {
                return;
            }

            pocketReviewOwner.SummonFollowupWindowOpened += HandleSummonFollowupWindowOpened;
            pocketReviewOwner.SummonFollowupHitConfirmed += HandleSummonFollowupHitConfirmed;
            pocketReviewOwner.SummonFollowupMissed += HandleSummonFollowupMissed;
            pocketReviewOwner.SummonBlockOpportunityOpened += HandleSummonBlockOpportunityOpened;
            pocketReviewOwner.PocketCleared += HandlePocketCleared;
            pocketReviewOwner.PocketFailed += HandlePocketFailed;
        }

        private void OnDisable()
        {
            if (pocketReviewOwner == null)
            {
                return;
            }

            pocketReviewOwner.SummonFollowupWindowOpened -= HandleSummonFollowupWindowOpened;
            pocketReviewOwner.SummonFollowupHitConfirmed -= HandleSummonFollowupHitConfirmed;
            pocketReviewOwner.SummonFollowupMissed -= HandleSummonFollowupMissed;
            pocketReviewOwner.SummonBlockOpportunityOpened -= HandleSummonBlockOpportunityOpened;
            pocketReviewOwner.PocketCleared -= HandlePocketCleared;
            pocketReviewOwner.PocketFailed -= HandlePocketFailed;
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
            lastFollowupHitTier = tier;
            lastFollowupHitDamage = damage;
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
    }
}
