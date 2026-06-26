using DimensionBrawl.Presentation;
using UnityEngine;

namespace DimensionBrawl.Test
{
    public sealed class BossBarragePocketCameraCueBridge : MonoBehaviour
    {
        [SerializeField] private BossBarragePocketReviewOwner pocketReviewOwner;
        [SerializeField] private ActionCameraCueDriver cameraCueDriver;
        [SerializeField] private ActionCinematicCueDirector cinematicCueDirector;

        public BossBarragePocketReviewOwner PocketReviewOwner => pocketReviewOwner;
        public ActionCameraCueDriver CameraCueDriver => cameraCueDriver;
        public ActionCinematicCueDirector CinematicCueDirector => cinematicCueDirector;

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
            pocketReviewOwner.CounterWaveObserved += HandleCounterWaveObserved;
            pocketReviewOwner.CounterWaveStabilized += HandleCounterWaveStabilized;
            pocketReviewOwner.PocketCleared += HandlePocketCleared;
            pocketReviewOwner.PocketFailed += HandlePocketFailed;
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
            pocketReviewOwner.CounterWaveObserved -= HandleCounterWaveObserved;
            pocketReviewOwner.CounterWaveStabilized -= HandleCounterWaveStabilized;
            pocketReviewOwner.PocketCleared -= HandlePocketCleared;
            pocketReviewOwner.PocketFailed -= HandlePocketFailed;
        }

        private void HandleSummonBlockOpportunityOpened()
        {
            if (cameraCueDriver != null)
            {
                cameraCueDriver.RequestSummonBlockOpportunityCue();
            }

            RequestCinematic(ActionCinematicCueProfile.CueKind.SummonEmpower, 2);
        }

        private void HandleSummonFollowupWindowOpened(int tier)
        {
            if (cameraCueDriver != null)
            {
                cameraCueDriver.RequestSummonFollowupWindowCue(tier);
            }

            RequestCinematic(ActionCinematicCueProfile.CueKind.BossPressureBreak, tier);
        }

        private void HandleSummonFollowupHitConfirmed(int tier, float damage)
        {
            if (cameraCueDriver != null)
            {
                cameraCueDriver.RequestSummonFollowupHitCue(tier, damage);
            }

            RequestCinematic(ActionCinematicCueProfile.CueKind.SummonFollowupHit, tier);
        }

        private void HandleSummonFollowupMissed()
        {
            if (cameraCueDriver != null)
            {
                cameraCueDriver.RequestSummonFollowupMissedCue();
            }

            RequestCinematic(ActionCinematicCueProfile.CueKind.SummonRecall, 2);
        }

        private void HandleCounterWaveObserved(BossBarragePocketReviewOwner.CounterWaveSource source)
        {
            cameraCueDriver?.RequestCounterWaveCue(ResolveCounterWaveCueTier(source));
        }

        private void HandleCounterWaveStabilized()
        {
            cameraCueDriver?.RequestCounterWaveStabilizedCue(2);
        }

        private void HandlePocketCleared()
        {
            cameraCueDriver?.RequestPocketClearCue(3);
            RequestCinematic(ActionCinematicCueProfile.CueKind.PocketClear, 3);
        }

        private void HandlePocketFailed()
        {
            cameraCueDriver?.RequestPocketFailCue(1);
            RequestCinematic(ActionCinematicCueProfile.CueKind.PocketFail, 1);
        }

        private bool RequestCinematic(ActionCinematicCueProfile.CueKind kind, int tier)
        {
            return cinematicCueDirector != null && cinematicCueDirector.TryPlay(kind, tier);
        }

        private static int ResolveCounterWaveCueTier(BossBarragePocketReviewOwner.CounterWaveSource source)
        {
            return source == BossBarragePocketReviewOwner.CounterWaveSource.BossSummonRelease
                || source == BossBarragePocketReviewOwner.CounterWaveSource.FollowupMissed
                ? 2
                : 1;
        }
    }
}
