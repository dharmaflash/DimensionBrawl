using DimensionBrawl.Player;
using DimensionBrawl.Presentation;
using UnityEngine;

namespace DimensionBrawl.Test
{
    public sealed class BossBarragePocketCameraCueBridge : MonoBehaviour
    {
        [SerializeField] private BossBarragePocketReviewOwner pocketReviewOwner;
        [SerializeField] private PlayerSummonSlot1Action summonSlot1Action;
        [SerializeField] private ActionCameraCueDriver cameraCueDriver;
        [SerializeField] private ActionCinematicCueDirector cinematicCueDirector;

        private int bossScreenSuppressCueRequestCount;

        public BossBarragePocketReviewOwner PocketReviewOwner => pocketReviewOwner;
        public PlayerSummonSlot1Action SummonSlot1Action => summonSlot1Action;
        public ActionCameraCueDriver CameraCueDriver => cameraCueDriver;
        public ActionCinematicCueDirector CinematicCueDirector => cinematicCueDirector;
        public int BossScreenSuppressCueRequestCount => bossScreenSuppressCueRequestCount;

        private void Awake()
        {
            if (pocketReviewOwner == null)
            {
                pocketReviewOwner = GetComponent<BossBarragePocketReviewOwner>();
            }

            if (summonSlot1Action == null)
            {
                ResolveSummonSlot1Action();
            }
        }

        private void OnEnable()
        {
            ResolveSummonSlot1Action();

            if (pocketReviewOwner != null)
            {
                pocketReviewOwner.SummonBlockOpportunityOpened += HandleSummonBlockOpportunityOpened;
                pocketReviewOwner.SummonFollowupWindowOpened += HandleSummonFollowupWindowOpened;
                pocketReviewOwner.SummonFollowupHitConfirmed += HandleSummonFollowupHitConfirmed;
                pocketReviewOwner.SummonFollowupMissed += HandleSummonFollowupMissed;
                pocketReviewOwner.BossScreenSuppressedByFollowupConfirmed += HandleBossScreenSuppressedByFollowupConfirmed;
                pocketReviewOwner.CounterWaveObserved += HandleCounterWaveObserved;
                pocketReviewOwner.CounterWaveStabilized += HandleCounterWaveStabilized;
                pocketReviewOwner.PocketCleared += HandlePocketCleared;
                pocketReviewOwner.PocketFailed += HandlePocketFailed;
            }

            if (summonSlot1Action != null)
            {
                summonSlot1Action.SummonPressureBlocked += HandleSummonPressureBlocked;
            }
        }

        private void OnDisable()
        {
            if (pocketReviewOwner != null)
            {
                pocketReviewOwner.SummonBlockOpportunityOpened -= HandleSummonBlockOpportunityOpened;
                pocketReviewOwner.SummonFollowupWindowOpened -= HandleSummonFollowupWindowOpened;
                pocketReviewOwner.SummonFollowupHitConfirmed -= HandleSummonFollowupHitConfirmed;
                pocketReviewOwner.SummonFollowupMissed -= HandleSummonFollowupMissed;
                pocketReviewOwner.BossScreenSuppressedByFollowupConfirmed -= HandleBossScreenSuppressedByFollowupConfirmed;
                pocketReviewOwner.CounterWaveObserved -= HandleCounterWaveObserved;
                pocketReviewOwner.CounterWaveStabilized -= HandleCounterWaveStabilized;
                pocketReviewOwner.PocketCleared -= HandlePocketCleared;
                pocketReviewOwner.PocketFailed -= HandlePocketFailed;
            }

            if (summonSlot1Action != null)
            {
                summonSlot1Action.SummonPressureBlocked -= HandleSummonPressureBlocked;
            }
        }

        private void HandleSummonPressureBlocked(int tier)
        {
            cameraCueDriver?.RequestSummonPressureBlockCue(tier);
        }

        private void ResolveSummonSlot1Action()
        {
            if (summonSlot1Action == null)
            {
                summonSlot1Action = FindFirstObjectByType<PlayerSummonSlot1Action>(FindObjectsInactive.Include);
            }
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

        private void HandleBossScreenSuppressedByFollowupConfirmed(int tier, int suppressedCount)
        {
            if (cameraCueDriver != null)
            {
                cameraCueDriver.RequestBossScreenSuppressCue(tier);
                bossScreenSuppressCueRequestCount++;
            }

            RequestCinematic(ActionCinematicCueProfile.CueKind.BossPressureBreak, tier);
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
