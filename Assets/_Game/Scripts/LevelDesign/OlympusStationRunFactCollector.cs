using System;
using DimensionBrawl.Combat;
using DimensionBrawl.Player;
using DimensionBrawl.UI;
using UnityEngine;

namespace DimensionBrawl.LevelDesign
{
    [DefaultExecutionOrder(9000)]
    [DisallowMultipleComponent]
    public sealed class OlympusStationRunFactCollector : MonoBehaviour
    {
        [SerializeField] private CombatEncounterController encounter;
        [SerializeField] private CombatHealth playerHealth;
        [SerializeField] private PlayerActionController playerActionController;
        [SerializeField] private SummonEnergyLadder summonEnergyLadder;
        [SerializeField] private PlayerSummonSlot1Action summonSlot1Action;
        [SerializeField] private PlayerSupportSummonSlotAction[] supportSummonActions =
            Array.Empty<PlayerSupportSummonSlotAction>();
        [SerializeField] private BossBarrageEncounterController bossEncounter;
        [SerializeField] private MonoBehaviour resultSurfaceBehaviour;

        private StageRunContext boundContext;
        private MonoBehaviour entryGuideBehaviour;
        private ICombatEntryGuideGate entryGuide;
        private bool subscribed;
        private bool stationCoordinatorRegistered;

        private ICombatSessionOverlay ResultSurface => resultSurfaceBehaviour as ICombatSessionOverlay;

        public bool IsBound => boundContext != null && subscribed;
        public string BoundRunId => boundContext?.Identity.RunId ?? string.Empty;
        public CombatEntryGuideState GuideState => entryGuide?.State ?? CombatEntryGuideState.NotStarted;
        public string LastFactError { get; private set; } = string.Empty;

        private void OnEnable()
        {
            if (HasLiveBinding())
            {
                Subscribe();
            }
            else
            {
                boundContext = null;
                stationCoordinatorRegistered = false;
            }
        }

        private void Start()
        {
            BindToActiveRun();
        }

        private void Update()
        {
            if (boundContext != null && !HasLiveBinding())
            {
                Unsubscribe();
                boundContext = null;
                stationCoordinatorRegistered = false;
                return;
            }

            if (boundContext != null
                && !stationCoordinatorRegistered
                && encounter != null
                && encounter.TerminalCoordinator != null)
            {
                if (!StageRunRuntime.TryRegisterStationCoordinator(encounter, out string coordinatorError))
                {
                    LastFactError = coordinatorError;
                    return;
                }

                stationCoordinatorRegistered = true;
            }

            PulseActiveClock();
        }

        private void OnDisable()
        {
            if (HasLiveBinding()
                && StageRunRuntime.TryAbortFromStationAdapterLoss(
                    this,
                    encounter,
                    StageRunAbortReason.StationFactCollectorLost,
                    out _,
                    out _))
            {
                boundContext = null;
            }

            Unsubscribe();
        }

        private bool HasLiveBinding()
        {
            if (boundContext == null
                || !ReferenceEquals(boundContext, StageRunRuntime.ActiveContext)
                || boundContext.CurrentSceneHandle != gameObject.scene.handle)
            {
                return false;
            }

            return boundContext.LifecycleState == StageRunLifecycleState.StationActive
                || boundContext.LifecycleState == StageRunLifecycleState.TerminalFinalizing
                || boundContext.LifecycleState
                    == StageRunLifecycleState.TerminalFinalizationOwnersSealed
                || boundContext.LifecycleState == StageRunLifecycleState.OutcomeFactsSealed
                || boundContext.LifecycleState == StageRunLifecycleState.CommitRequested
                || boundContext.LifecycleState == StageRunLifecycleState.CommitRecoveryPending;
        }

        public bool BindToActiveRun()
        {
            StageRunContext context = StageRunRuntime.ActiveContext;
            if (context == null
                || context.LifecycleState != StageRunLifecycleState.StationActive
                || context.CurrentSceneHandle != gameObject.scene.handle)
            {
                LastFactError = "No active canonical Station run is available for fact collection.";
                return false;
            }

            if (!TryValidateReferences(out string referenceError))
            {
                LastFactError = referenceError;
                return false;
            }

            if (boundContext != null && !ReferenceEquals(boundContext, context))
            {
                Unsubscribe();
                stationCoordinatorRegistered = false;
            }

            boundContext = context;
            if (!boundContext.TryBindStationFactCollector(gameObject.scene.handle, out string bindError))
            {
                LastFactError = bindError;
                return false;
            }

            Subscribe();
            if (encounter.TerminalCoordinator != null)
            {
                if (!StageRunRuntime.TryRegisterStationCoordinator(encounter, out string coordinatorError))
                {
                    LastFactError = coordinatorError;
                    return false;
                }

                stationCoordinatorRegistered = true;
            }

            LastFactError = string.Empty;
            if (entryGuide.State == CombatEntryGuideState.Released
                && !boundContext.TryMarkStationGuideReleased(out string releaseError))
            {
                LastFactError = releaseError;
                return false;
            }

            PulseActiveClock();
            return true;
        }

        public bool PrepareForTerminal()
        {
            if (!IsBound && !BindToActiveRun())
            {
                return false;
            }

            return PulseActiveClock();
        }

        private bool TryValidateReferences(out string error)
        {
            error = string.Empty;
            if (encounter == null
                || playerHealth == null
                || playerActionController == null
                || summonEnergyLadder == null
                || summonSlot1Action == null
                || bossEncounter == null
                || ResultSurface == null)
            {
                error = "Station fact collector is missing an authored combat, player, summon, boss, or pause reference.";
                return false;
            }

            if (supportSummonActions == null || supportSummonActions.Length == 0)
            {
                error = "Station fact collector has no authored support-summon sources.";
                return false;
            }

            for (int i = 0; i < supportSummonActions.Length; i++)
            {
                if (supportSummonActions[i] == null)
                {
                    error = "Station fact collector contains a missing support-summon source.";
                    return false;
                }
            }

            if (!TryResolveEntryGuide(out error))
            {
                return false;
            }

            return true;
        }

        private bool TryResolveEntryGuide(out string error)
        {
            error = string.Empty;
            if (entryGuideBehaviour != null
                && entryGuideBehaviour.gameObject.scene.handle == gameObject.scene.handle
                && entryGuideBehaviour is ICombatEntryGuideGate resolvedGate)
            {
                entryGuide = resolvedGate;
                return true;
            }

            entryGuideBehaviour = null;
            entryGuide = null;
            GameObject[] roots = gameObject.scene.GetRootGameObjects();
            for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
            {
                MonoBehaviour[] behaviours = roots[rootIndex].GetComponentsInChildren<MonoBehaviour>(true);
                for (int behaviourIndex = 0; behaviourIndex < behaviours.Length; behaviourIndex++)
                {
                    MonoBehaviour behaviour = behaviours[behaviourIndex];
                    if (behaviour is not ICombatEntryGuideGate gate)
                    {
                        continue;
                    }

                    if (entryGuide != null)
                    {
                        error = "Station scene contains more than one combat-entry guide gate.";
                        entryGuideBehaviour = null;
                        entryGuide = null;
                        return false;
                    }

                    entryGuideBehaviour = behaviour;
                    entryGuide = gate;
                }
            }

            if (entryGuide == null)
            {
                error = "Station scene has no combat-entry guide gate.";
                return false;
            }

            return true;
        }

        private void Subscribe()
        {
            if (subscribed || boundContext == null || entryGuide == null)
            {
                return;
            }

            playerHealth.Damaged += HandlePlayerDamaged;
            playerHealth.Died += HandlePlayerDied;
            playerActionController.PerfectDodgeTriggered += HandlePerfectDodge;
            summonSlot1Action.SummonSlot1Used += HandleSummonSlot1Used;
            summonSlot1Action.SummonPressureBlocked += HandleSummonSlot1PressureBlocked;
            for (int i = 0; i < supportSummonActions.Length; i++)
            {
                supportSummonActions[i].SummonUsed += HandleSupportSummonUsed;
                supportSummonActions[i].SummonPressureBlocked += HandleSupportSummonPressureBlocked;
            }

            bossEncounter.SummonFollowupHitConfirmed += HandleSummonFollowupHitConfirmed;
            bossEncounter.CounterWaveStabilized += HandleCounterWaveStabilized;
            entryGuide.StateChanged += HandleGuideStateChanged;
            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed)
            {
                return;
            }

            if (playerHealth != null)
            {
                playerHealth.Damaged -= HandlePlayerDamaged;
                playerHealth.Died -= HandlePlayerDied;
            }

            if (playerActionController != null)
            {
                playerActionController.PerfectDodgeTriggered -= HandlePerfectDodge;
            }

            if (summonSlot1Action != null)
            {
                summonSlot1Action.SummonSlot1Used -= HandleSummonSlot1Used;
                summonSlot1Action.SummonPressureBlocked -= HandleSummonSlot1PressureBlocked;
            }

            if (supportSummonActions != null)
            {
                for (int i = 0; i < supportSummonActions.Length; i++)
                {
                    PlayerSupportSummonSlotAction action = supportSummonActions[i];
                    if (action == null)
                    {
                        continue;
                    }

                    action.SummonUsed -= HandleSupportSummonUsed;
                    action.SummonPressureBlocked -= HandleSupportSummonPressureBlocked;
                }
            }

            if (bossEncounter != null)
            {
                bossEncounter.SummonFollowupHitConfirmed -= HandleSummonFollowupHitConfirmed;
                bossEncounter.CounterWaveStabilized -= HandleCounterWaveStabilized;
            }

            if (entryGuide != null)
            {
                entryGuide.StateChanged -= HandleGuideStateChanged;
            }

            subscribed = false;
        }

        private bool PulseActiveClock()
        {
            if (boundContext == null
                || boundContext.CurrentSceneHandle != gameObject.scene.handle
                || (boundContext.LifecycleState != StageRunLifecycleState.StationActive
                    && boundContext.LifecycleState != StageRunLifecycleState.TerminalFinalizing
                    && boundContext.LifecycleState != StageRunLifecycleState.TerminalFinalizationOwnersSealed))
            {
                return false;
            }

            CombatSessionOverlayMode overlayMode = ResultSurface?.Mode ?? CombatSessionOverlayMode.Hidden;
            bool explicitlyPaused = overlayMode == CombatSessionOverlayMode.Pause
                || overlayMode == CombatSessionOverlayMode.Settings;
            bool combatEligible = entryGuide?.State == CombatEntryGuideState.Released
                && encounter != null
                && encounter.IsRunning;
            bool forwardRiskEligible = combatEligible
                && summonEnergyLadder != null
                && summonEnergyLadder.CurrentRiskBand == SummonEnergyRiskBand.ForwardRisk;
            if (!boundContext.TryPulseActiveTime(
                    Time.realtimeSinceStartupAsDouble,
                    Application.isBatchMode || Application.isFocused,
                    explicitlyPaused,
                    combatEligible,
                    forwardRiskEligible,
                    out string error))
            {
                LastFactError = error;
                return false;
            }

            return true;
        }

        private void HandleGuideStateChanged(CombatEntryGuideState state)
        {
            if (state != CombatEntryGuideState.Released || boundContext == null)
            {
                return;
            }

            if (!boundContext.TryMarkStationGuideReleased(out string error))
            {
                LastFactError = error;
                return;
            }

            PulseActiveClock();
        }

        private void HandlePlayerDamaged(DamageInfo damageInfo)
        {
            if (!CombatTeamUtility.AreHostile(playerHealth.Team, damageInfo.SourceTeam))
            {
                return;
            }

            Record(boundContext.TryRecordResolvedPlayerDamage(damageInfo.Amount, out string error), error);
        }

        private void HandlePlayerDied()
        {
            Record(boundContext.TryRecordPlayerDown(out string error), error);
        }

        private void HandlePerfectDodge(DamageInfo _)
        {
            Record(boundContext.TryRecordPerfectDodge(out string error), error);
        }

        private void HandleSummonSlot1Used(int spentTier)
        {
            Record(boundContext.TryRecordSummonUse("SummonSlot1", spentTier, out string error), error);
        }

        private void HandleSupportSummonUsed(PlayerSupportSummonSlotAction action, int spentTier)
        {
            string slotRoleId = action != null ? action.SlotActionName : string.Empty;
            Record(boundContext.TryRecordSummonUse(slotRoleId, spentTier, out string error), error);
        }

        private void HandleSummonSlot1PressureBlocked(int tier)
        {
            RecordPressureBlockProof(tier);
        }

        private void HandleSupportSummonPressureBlocked(PlayerSupportSummonSlotAction _, int tier)
        {
            RecordPressureBlockProof(tier);
        }

        private void RecordPressureBlockProof(int tier)
        {
            Record(
                boundContext.TryRecordSemanticProof(
                    StageRunFactVocabulary.SummonPressureBlockProofId,
                    StageRunFactVocabulary.SummonPressureScreenSourceKind,
                    tier,
                    true,
                    out string error),
                error);
        }

        private void HandleSummonFollowupHitConfirmed(int _, float damage)
        {
            Record(
                boundContext.TryRecordSemanticProof(
                    StageRunFactVocabulary.SummonFollowupHitProofId,
                    StageRunFactVocabulary.BossFollowupConfirmationSourceKind,
                    Math.Max(0f, damage),
                    true,
                    out string error),
                error);
        }

        private void HandleCounterWaveStabilized()
        {
            Record(
                boundContext.TryRecordSemanticProof(
                    StageRunFactVocabulary.SummonCounterRecoveryProofId,
                    StageRunFactVocabulary.CounterWaveStabilizationSourceKind,
                    1d,
                    true,
                    out string error),
                error);
        }

        private void Record(bool accepted, string error)
        {
            if (!accepted)
            {
                LastFactError = error ?? string.Empty;
            }
        }
    }
}
