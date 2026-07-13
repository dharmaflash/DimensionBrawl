using DimensionBrawl.Combat;
using DimensionBrawl.Player;
using UnityEngine;
using UnityEngine.Serialization;

namespace DimensionBrawl.Presentation
{
    public sealed class BossBarragePocketCameraCueBridge : MonoBehaviour
    {
        [FormerlySerializedAs("pocketReviewOwner")]
        [SerializeField] private BossBarrageEncounterController encounterController;
        [SerializeField] private PlayerSummonSlot1Action summonSlot1Action;
        [SerializeField] private ActionCameraCueDriver cameraCueDriver;
        [SerializeField] private ActionCinematicCueDirector cinematicCueDirector;

        private int bossScreenSuppressCueRequestCount;

        public BossBarrageEncounterController EncounterController => encounterController;
        public PlayerSummonSlot1Action SummonSlot1Action => summonSlot1Action;
        public ActionCameraCueDriver CameraCueDriver => cameraCueDriver;
        public ActionCinematicCueDirector CinematicCueDirector => cinematicCueDirector;
        public int BossScreenSuppressCueRequestCount => bossScreenSuppressCueRequestCount;

        private void Awake()
        {
            if (encounterController == null)
            {
                encounterController = GetComponent<BossBarrageEncounterController>();
            }

            if (summonSlot1Action == null)
            {
                ResolveSummonSlot1Action();
            }
        }

        private void OnEnable()
        {
            if (encounterController != null)
            {
                encounterController.SummonBlockOpportunityOpened += HandleSummonBlockOpportunityOpened;
                encounterController.SummonFollowupWindowOpened += HandleSummonFollowupWindowOpened;
                encounterController.SummonFollowupHitConfirmed += HandleSummonFollowupHitConfirmed;
                encounterController.SummonFollowupMissed += HandleSummonFollowupMissed;
                encounterController.BossScreenSuppressedByFollowupConfirmed += HandleBossScreenSuppressedByFollowupConfirmed;
                encounterController.CounterWaveObserved += HandleCounterWaveObserved;
                encounterController.CounterWaveStabilized += HandleCounterWaveStabilized;
                encounterController.PocketCleared += HandlePocketCleared;
                encounterController.PocketFailed += HandlePocketFailed;
            }

        }

        private void OnDisable()
        {
            if (encounterController != null)
            {
                encounterController.SummonBlockOpportunityOpened -= HandleSummonBlockOpportunityOpened;
                encounterController.SummonFollowupWindowOpened -= HandleSummonFollowupWindowOpened;
                encounterController.SummonFollowupHitConfirmed -= HandleSummonFollowupHitConfirmed;
                encounterController.SummonFollowupMissed -= HandleSummonFollowupMissed;
                encounterController.BossScreenSuppressedByFollowupConfirmed -= HandleBossScreenSuppressedByFollowupConfirmed;
                encounterController.CounterWaveObserved -= HandleCounterWaveObserved;
                encounterController.CounterWaveStabilized -= HandleCounterWaveStabilized;
                encounterController.PocketCleared -= HandlePocketCleared;
                encounterController.PocketFailed -= HandlePocketFailed;
            }

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

        private void HandleCounterWaveObserved(BossBarrageEncounterController.CounterWaveSource source)
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

        private static int ResolveCounterWaveCueTier(BossBarrageEncounterController.CounterWaveSource source)
        {
            return source == BossBarrageEncounterController.CounterWaveSource.BossSummonRelease
                || source == BossBarrageEncounterController.CounterWaveSource.FollowupMissed
                ? 2
                : 1;
        }
    }
}
