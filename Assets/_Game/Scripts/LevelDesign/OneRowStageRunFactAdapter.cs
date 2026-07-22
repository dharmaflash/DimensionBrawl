using System;
using System.Collections.Generic;
using DimensionBrawl.Combat;
using DimensionBrawl.Player;
using DimensionBrawl.UI;
using UnityEngine;

namespace DimensionBrawl.LevelDesign
{
    [DefaultExecutionOrder(9000)]
    [DisallowMultipleComponent]
    public sealed class OneRowStageRunFactAdapter : MonoBehaviour, ITerminalStageRunAdapterLossOwner
    {
        [SerializeField] private CombatEncounterController encounter;
        [SerializeField] private CombatHealth playerHealth;
        [SerializeField] private PlayerActionController playerActionController;
        [SerializeField] private SummonEnergyLadder summonEnergyLadder;
        [SerializeField] private PlayerSummonSlot1Action summonSlot1Action;
        [SerializeField] private PlayerSupportSummonSlotAction[] supportSummonActions =
            Array.Empty<PlayerSupportSummonSlotAction>();
        [SerializeField] private MonoBehaviour resultSurfaceBehaviour;

        private StageRunContext boundContext;
        private bool subscribed;
        private bool coordinatorRegistered;
        private bool hasStarted;
        private bool sourcesSealed;
        private CombatEncounterController sealedEncounter;
        private CombatHealth sealedPlayerHealth;
        private Transform sealedPlayerRoot;
        private PlayerActionController sealedPlayerActionController;
        private SummonEnergyLadder sealedSummonEnergyLadder;
        private PlayerSummonSlot1Action sealedSummonSlot1Action;
        private PlayerSupportSummonSlotAction[] sealedSupportSummonActions =
            Array.Empty<PlayerSupportSummonSlotAction>();
        private MonoBehaviour sealedResultSurfaceBehaviour;

        private ICombatSessionOverlay ResultSurface => resultSurfaceBehaviour as ICombatSessionOverlay;

        StageRunAbortReason ITerminalStageRunAdapterLossOwner.AdapterLossReason =>
            StageRunAbortReason.TerminalFactAdapterLost;

        public bool IsBound => boundContext != null && subscribed && HasLiveBinding();
        public string BoundRunId => boundContext?.Identity.RunId ?? string.Empty;
        public string LastFactError { get; private set; } = string.Empty;

        private void OnEnable()
        {
            if (hasStarted)
            {
                BindToActiveRun();
            }
        }

        private void Start()
        {
            hasStarted = true;
            BindToActiveRun();
        }

        private void Update()
        {
            if (boundContext != null && !HasLiveBinding())
            {
                Unsubscribe();
                boundContext = null;
                coordinatorRegistered = false;
                return;
            }

            if (boundContext != null && !TryValidateLiveSealedSources(out string sourceError))
            {
                LastFactError = sourceError;
                StageRunRuntime.TryAbortFromTerminalAdapterLoss(
                    this,
                    encounter,
                    StageRunAbortReason.TerminalFactAdapterLost,
                    out _,
                    out _);
                Unsubscribe();
                boundContext = null;
                coordinatorRegistered = false;
                return;
            }

            if (boundContext != null
                && !coordinatorRegistered
                && encounter != null
                && encounter.TerminalCoordinator != null)
            {
                if (!StageRunRuntime.TryRegisterTerminalCoordinator(
                    encounter,
                    out string coordinatorError))
                {
                    LastFactError = coordinatorError;
                    return;
                }

                coordinatorRegistered = true;
            }

            PulseActiveClock();
        }

        private void OnDisable()
        {
            if (HasLiveBinding()
                && StageRunRuntime.TryAbortFromTerminalAdapterLoss(
                    this,
                    encounter,
                    StageRunAbortReason.TerminalFactAdapterLost,
                    out _,
                    out _))
            {
                boundContext = null;
            }

            Unsubscribe();
        }

        public bool BindToActiveRun()
        {
            StageRunContext context = StageRunRuntime.ActiveContext;
            if (!OneRowStageRunAdapterContract.TryValidateContext(
                context,
                gameObject.scene.handle,
                out string error)
                || context.LifecycleState != StageRunLifecycleState.StationActive
                || !context.IsCurrentSegmentTerminalActive)
            {
                LastFactError = string.IsNullOrWhiteSpace(error)
                    ? "No active one-row terminal run is available for fact collection."
                    : error;
                return false;
            }

            if (!TryValidateReferences(out error))
            {
                LastFactError = error;
                return false;
            }

            if (boundContext != null && !ReferenceEquals(boundContext, context))
            {
                Unsubscribe();
                boundContext = null;
                coordinatorRegistered = false;
            }

            if (!context.TryBindTerminalStageAdapter(
                this,
                TerminalStageRunAdapterRole.FactCollection,
                out error)
                || !context.TryBindTerminalFactCollector(gameObject.scene.handle, out error))
            {
                LastFactError = error;
                return false;
            }

            if (encounter.TerminalCoordinator != null)
            {
                if (!StageRunRuntime.TryRegisterTerminalCoordinator(encounter, out error))
                {
                    LastFactError = error;
                    return false;
                }

                coordinatorRegistered = true;
            }

            boundContext = context;
            Subscribe();
            LastFactError = string.Empty;
            return PulseActiveClock();
        }

        public bool PrepareForTerminal()
        {
            if (!IsBound && !BindToActiveRun())
            {
                return false;
            }

            if (!TryValidateReferences(out string error))
            {
                LastFactError = error;
                return false;
            }

            return PulseActiveClock();
        }

        internal bool TryValidateAuthoring(
            CombatEncounterController expectedEncounter,
            out string error)
        {
            if (!ReferenceEquals(encounter, expectedEncounter))
            {
                error = "The one-row fact adapter does not reference the bootstrap encounter.";
                return false;
            }

            return TryValidateReferences(out error);
        }

        private bool TryValidateReferences(out string error)
        {
            error = string.Empty;
            if (encounter == null
                || playerHealth == null
                || !ReferenceEquals(playerHealth, encounter.PlayerHealth)
                || ResultSurface == null
                || encounter.gameObject.scene.handle != gameObject.scene.handle
                || playerHealth.gameObject.scene.handle != gameObject.scene.handle
                || !encounter.UsesCoordinatedTerminalResolution)
            {
                error =
                    "A one-row fact adapter requires the coordinated encounter's exact same-scene player health source.";
                return false;
            }

            if (!resultSurfaceBehaviour.isActiveAndEnabled
                || resultSurfaceBehaviour.gameObject.scene.handle != gameObject.scene.handle)
            {
                error = "The one-row fact adapter's combat-session surface is not live in its scene.";
                return false;
            }

            Transform playerRoot = playerHealth.transform.root;
            if (!TryValidateOptionalSingleton(
                    playerRoot,
                    playerActionController,
                    "player action",
                    out error)
                || !TryValidateOptionalSingleton(
                    playerRoot,
                    summonEnergyLadder,
                    "summon energy",
                    out error)
                || !TryValidateOptionalSingleton(
                    playerRoot,
                    summonSlot1Action,
                    "summon slot 1",
                    out error)
                || !TryValidateSupportSummonCoverage(playerRoot, out error))
            {
                return false;
            }

            if (sourcesSealed)
            {
                return TryValidateSealedConfiguration(out error);
            }

            SealValidatedSources(playerRoot);
            return true;
        }

        private void SealValidatedSources(Transform playerRoot)
        {
            sealedEncounter = encounter;
            sealedPlayerHealth = playerHealth;
            sealedPlayerRoot = playerRoot;
            sealedPlayerActionController = playerActionController;
            sealedSummonEnergyLadder = summonEnergyLadder;
            sealedSummonSlot1Action = summonSlot1Action;
            sealedResultSurfaceBehaviour = resultSurfaceBehaviour;
            sealedSupportSummonActions = supportSummonActions.Length == 0
                ? Array.Empty<PlayerSupportSummonSlotAction>()
                : (PlayerSupportSummonSlotAction[])supportSummonActions.Clone();
            sourcesSealed = true;
        }

        private bool TryValidateLiveSealedSources(out string error)
        {
            if (!TryValidateSealedConfiguration(out error))
            {
                return false;
            }

            if (sealedEncounter == null
                || !sealedEncounter.isActiveAndEnabled
                || sealedEncounter.gameObject.scene.handle != gameObject.scene.handle
                || !sealedEncounter.UsesCoordinatedTerminalResolution
                || sealedPlayerHealth == null
                || !sealedPlayerHealth.isActiveAndEnabled
                || sealedPlayerHealth.gameObject.scene.handle != gameObject.scene.handle
                || !ReferenceEquals(sealedEncounter.PlayerHealth, sealedPlayerHealth)
                || sealedPlayerRoot == null
                || sealedPlayerHealth.transform.root != sealedPlayerRoot
                || sealedResultSurfaceBehaviour == null
                || !sealedResultSurfaceBehaviour.isActiveAndEnabled
                || sealedResultSurfaceBehaviour.gameObject.scene.handle != gameObject.scene.handle
                || sealedResultSurfaceBehaviour is not ICombatSessionOverlay)
            {
                error = "A sealed one-row fact source is no longer live in the admitted scene.";
                return false;
            }

            if ((sealedPlayerActionController != null
                    && !IsLiveOnSealedPlayerRoot(sealedPlayerActionController))
                || (sealedSummonEnergyLadder != null
                    && !IsLiveOnSealedPlayerRoot(sealedSummonEnergyLadder))
                || (sealedSummonSlot1Action != null
                    && !IsLiveOnSealedPlayerRoot(sealedSummonSlot1Action)))
            {
                error = "A sealed one-row player fact source is no longer live.";
                return false;
            }

            for (int i = 0; i < sealedSupportSummonActions.Length; i++)
            {
                PlayerSupportSummonSlotAction action = sealedSupportSummonActions[i];
                if (!IsLiveOnSealedPlayerRoot(action))
                {
                    error = "The sealed one-row support-summon source set became inactive.";
                    return false;
                }
            }

            return true;
        }

        private bool TryValidateSealedConfiguration(out string error)
        {
            error = string.Empty;
            if (!sourcesSealed
                || !ReferenceEquals(encounter, sealedEncounter)
                || !ReferenceEquals(playerHealth, sealedPlayerHealth)
                || !ReferenceEquals(playerActionController, sealedPlayerActionController)
                || !ReferenceEquals(summonEnergyLadder, sealedSummonEnergyLadder)
                || !ReferenceEquals(summonSlot1Action, sealedSummonSlot1Action)
                || !ReferenceEquals(resultSurfaceBehaviour, sealedResultSurfaceBehaviour)
                || supportSummonActions == null
                || supportSummonActions.Length != sealedSupportSummonActions.Length)
            {
                error = "The sealed one-row fact-source configuration changed during the run.";
                return false;
            }

            for (int i = 0; i < sealedSupportSummonActions.Length; i++)
            {
                if (!ReferenceEquals(
                    supportSummonActions[i],
                    sealedSupportSummonActions[i]))
                {
                    error = "The sealed one-row support-summon source set changed during the run.";
                    return false;
                }
            }

            return true;
        }

        private bool IsLiveOnSealedPlayerRoot(Component source)
        {
            return source != null
                && source.transform.root == sealedPlayerRoot
                && source.gameObject.scene.handle == gameObject.scene.handle
                && source.gameObject.activeInHierarchy
                && (source is not Behaviour behaviour || behaviour.isActiveAndEnabled);
        }

        private bool TryValidateOptionalSingleton<T>(
            Transform playerRoot,
            T configured,
            string label,
            out string error)
            where T : Component
        {
            error = string.Empty;
            T resolved = null;
            T[] candidates = playerRoot.GetComponentsInChildren<T>(true);
            for (int i = 0; i < candidates.Length; i++)
            {
                T candidate = candidates[i];
                if (candidate == null || !candidate.gameObject.activeInHierarchy)
                {
                    continue;
                }

                if (candidate is Behaviour behaviour && !behaviour.isActiveAndEnabled)
                {
                    continue;
                }

                if (resolved != null)
                {
                    error = $"The one-row player root contains more than one live {label} source.";
                    return false;
                }

                resolved = candidate;
            }

            if (!ReferenceEquals(resolved, configured))
            {
                error = resolved == null
                    ? $"The one-row fact adapter declares a {label} source that is not live on the player root."
                    : $"The one-row fact adapter does not bind the exact live {label} source.";
                return false;
            }

            return true;
        }

        private bool TryValidateSupportSummonCoverage(Transform playerRoot, out string error)
        {
            error = string.Empty;
            supportSummonActions ??= Array.Empty<PlayerSupportSummonSlotAction>();
            var configured = new HashSet<PlayerSupportSummonSlotAction>();
            for (int i = 0; i < supportSummonActions.Length; i++)
            {
                PlayerSupportSummonSlotAction action = supportSummonActions[i];
                if (action == null
                    || !configured.Add(action)
                    || action.transform.root != playerRoot
                    || !action.isActiveAndEnabled)
                {
                    error = "The one-row fact adapter contains a missing, duplicate, foreign, or inactive support-summon source.";
                    return false;
                }
            }

            PlayerSupportSummonSlotAction[] candidates =
                playerRoot.GetComponentsInChildren<PlayerSupportSummonSlotAction>(true);
            int liveCount = 0;
            for (int i = 0; i < candidates.Length; i++)
            {
                PlayerSupportSummonSlotAction candidate = candidates[i];
                if (candidate == null || !candidate.isActiveAndEnabled)
                {
                    continue;
                }

                liveCount++;
                if (!configured.Contains(candidate))
                {
                    error = "The one-row fact adapter omits a live support-summon source from the player root.";
                    return false;
                }
            }

            if (liveCount != configured.Count)
            {
                error = "The one-row support-summon source set is not exact.";
                return false;
            }

            return true;
        }

        internal bool UsesResultSurface(MonoBehaviour expectedSurface)
        {
            return expectedSurface != null
                && ReferenceEquals(resultSurfaceBehaviour, expectedSurface)
                && ReferenceEquals(ResultSurface, expectedSurface as ICombatSessionOverlay);
        }

        private bool HasLiveBinding()
        {
            return boundContext != null
                && subscribed
                && ReferenceEquals(boundContext, StageRunRuntime.ActiveContext)
                && boundContext.IsTerminalStageAdapterOwner(
                    this,
                    TerminalStageRunAdapterRole.FactCollection)
                && OneRowStageRunAdapterContract.TryValidateContext(
                    boundContext,
                    gameObject.scene.handle,
                    out _)
                && OneRowStageRunAdapterContract.IsFactBindingLifecycle(
                    boundContext.LifecycleState);
        }

        private void Subscribe()
        {
            if (subscribed || boundContext == null)
            {
                return;
            }

            sealedPlayerHealth.Damaged += HandlePlayerDamaged;
            sealedPlayerHealth.Died += HandlePlayerDied;
            if (sealedPlayerActionController != null)
            {
                sealedPlayerActionController.PerfectDodgeTriggered += HandlePerfectDodge;
            }

            if (sealedSummonSlot1Action != null)
            {
                sealedSummonSlot1Action.SummonSlot1Used += HandleSummonSlot1Used;
            }

            for (int i = 0; i < sealedSupportSummonActions.Length; i++)
            {
                sealedSupportSummonActions[i].SummonUsed += HandleSupportSummonUsed;
            }

            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed)
            {
                return;
            }

            if (sealedPlayerHealth != null)
            {
                sealedPlayerHealth.Damaged -= HandlePlayerDamaged;
                sealedPlayerHealth.Died -= HandlePlayerDied;
            }

            if (sealedPlayerActionController != null)
            {
                sealedPlayerActionController.PerfectDodgeTriggered -= HandlePerfectDodge;
            }

            if (sealedSummonSlot1Action != null)
            {
                sealedSummonSlot1Action.SummonSlot1Used -= HandleSummonSlot1Used;
            }

            if (sealedSupportSummonActions != null)
            {
                for (int i = 0; i < sealedSupportSummonActions.Length; i++)
                {
                    if (sealedSupportSummonActions[i] != null)
                    {
                        sealedSupportSummonActions[i].SummonUsed -= HandleSupportSummonUsed;
                    }
                }
            }

            subscribed = false;
        }

        private bool PulseActiveClock()
        {
            if (!HasLiveBinding()
                || (boundContext.LifecycleState != StageRunLifecycleState.StationActive
                    && boundContext.LifecycleState != StageRunLifecycleState.TerminalFinalizing
                    && boundContext.LifecycleState
                        != StageRunLifecycleState.TerminalFinalizationOwnersSealed))
            {
                return false;
            }

            ICombatSessionOverlay sealedResultSurface =
                sealedResultSurfaceBehaviour as ICombatSessionOverlay;
            CombatSessionOverlayMode mode = sealedResultSurface?.Mode
                ?? CombatSessionOverlayMode.Hidden;
            bool paused = mode == CombatSessionOverlayMode.Pause
                || mode == CombatSessionOverlayMode.Settings;
            bool combatEligible = coordinatorRegistered
                && sealedEncounter != null
                && sealedEncounter.IsRunning
                && sealedEncounter.TerminalCoordinator != null;
            bool forwardRiskEligible = combatEligible
                && sealedSummonEnergyLadder != null
                && sealedSummonEnergyLadder.CurrentRiskBand == SummonEnergyRiskBand.ForwardRisk;
            if (!boundContext.TryPulseActiveTime(
                Time.realtimeSinceStartupAsDouble,
                Application.isBatchMode || Application.isFocused,
                paused,
                combatEligible,
                forwardRiskEligible,
                out string error))
            {
                LastFactError = error;
                return false;
            }

            return true;
        }

        private void HandlePlayerDamaged(DamageInfo damageInfo)
        {
            if (!TryGetLiveBoundContext(out StageRunContext context)
                || sealedPlayerHealth == null
                || !CombatTeamUtility.AreHostile(
                    sealedPlayerHealth.Team,
                    damageInfo.SourceTeam))
            {
                return;
            }

            Record(context.TryRecordResolvedPlayerDamage(damageInfo.Amount, out string error), error);
        }

        private void HandlePlayerDied()
        {
            if (TryGetLiveBoundContext(out StageRunContext context))
            {
                Record(context.TryRecordPlayerDown(out string error), error);
            }
        }

        private void HandlePerfectDodge(DamageInfo _)
        {
            if (TryGetLiveBoundContext(out StageRunContext context))
            {
                Record(context.TryRecordPerfectDodge(out string error), error);
            }
        }

        private void HandleSummonSlot1Used(int spentTier)
        {
            if (TryGetLiveBoundContext(out StageRunContext context))
            {
                Record(
                    context.TryRecordSummonUse(
                        "SummonSlot1",
                        spentTier,
                        out string error),
                    error);
            }
        }

        private void HandleSupportSummonUsed(PlayerSupportSummonSlotAction action, int spentTier)
        {
            if (!TryGetLiveBoundContext(out StageRunContext context))
            {
                return;
            }

            string slotRoleId = action != null ? action.SlotActionName : string.Empty;
            Record(context.TryRecordSummonUse(slotRoleId, spentTier, out string error), error);
        }

        private bool TryGetLiveBoundContext(out StageRunContext context)
        {
            context = boundContext;
            if (!HasLiveBinding())
            {
                return false;
            }

            if (TryValidateLiveSealedSources(out string error))
            {
                return true;
            }

            LastFactError = error;
            return false;
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
