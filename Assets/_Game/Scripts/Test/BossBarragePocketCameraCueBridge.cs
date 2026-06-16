using DimensionBrawl.Presentation;
using UnityEngine;

namespace DimensionBrawl.Test
{
    public sealed class BossBarragePocketCameraCueBridge : MonoBehaviour
    {
        [SerializeField] private BossBarragePocketReviewOwner pocketReviewOwner;
        [SerializeField] private ActionCameraCueDriver cameraCueDriver;

        public BossBarragePocketReviewOwner PocketReviewOwner => pocketReviewOwner;
        public ActionCameraCueDriver CameraCueDriver => cameraCueDriver;

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

            pocketReviewOwner.SummonBlockOpportunityOpened += HandleSummonBlockOpportunityOpened;
            pocketReviewOwner.SummonFollowupWindowOpened += HandleSummonFollowupWindowOpened;
            pocketReviewOwner.SummonFollowupHitConfirmed += HandleSummonFollowupHitConfirmed;
            pocketReviewOwner.SummonFollowupMissed += HandleSummonFollowupMissed;
        }

        private void OnDisable()
        {
            if (pocketReviewOwner == null)
            {
                return;
            }

            pocketReviewOwner.SummonBlockOpportunityOpened -= HandleSummonBlockOpportunityOpened;
            pocketReviewOwner.SummonFollowupWindowOpened -= HandleSummonFollowupWindowOpened;
            pocketReviewOwner.SummonFollowupHitConfirmed -= HandleSummonFollowupHitConfirmed;
            pocketReviewOwner.SummonFollowupMissed -= HandleSummonFollowupMissed;
        }

        private void HandleSummonBlockOpportunityOpened()
        {
            if (cameraCueDriver != null)
            {
                cameraCueDriver.RequestSummonBlockOpportunityCue();
            }
        }

        private void HandleSummonFollowupWindowOpened(int tier)
        {
            if (cameraCueDriver != null)
            {
                cameraCueDriver.RequestSummonFollowupWindowCue(tier);
            }
        }

        private void HandleSummonFollowupHitConfirmed(int tier, float damage)
        {
            if (cameraCueDriver != null)
            {
                cameraCueDriver.RequestSummonFollowupHitCue(tier, damage);
            }
        }

        private void HandleSummonFollowupMissed()
        {
            if (cameraCueDriver != null)
            {
                cameraCueDriver.RequestSummonFollowupMissedCue();
            }
        }
    }
}
