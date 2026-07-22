using System;
using System.Collections.Generic;
using DimensionBrawl.AI;
using DimensionBrawl.Combat;
using DimensionBrawl.Enemies;
using DimensionBrawl.Player;
using DimensionBrawl.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DimensionBrawl.LevelDesign
{
    public enum StageCountOneEncounterState
    {
        Uninitialized = 0,
        WaitingForRun = 1,
        WaitingForActivation = 2,
        Active = 3,
        Completed = 4,
        Cancelled = 5,
        Faulted = 6
    }

    public enum StageEncounterActivationKind
    {
        SceneReady = 0,
        CombatEntryGuideReleased = 1
    }

    public enum StageAddEncounterTicketState
    {
        Pending = 0,
        Active = 1,
        Completed = 2,
        Cancelled = 3,
        Faulted = 4
    }

    public readonly struct StageAddEncounterTicketSnapshot
    {
        public StageAddEncounterTicketSnapshot(
            int sourceOrdinal,
            string spawnId,
            string anchorId,
            string payloadId,
            int positionId,
            float delaySeconds,
            StageAddEncounterTicketState state,
            int activationSequence,
            int terminalSequence,
            GameObject root,
            CombatHealth health,
            ICombatAiAgent agent,
            CombatTargetSensor sensor,
            BasicSoldierProjectileAttackDriver projectileDriver,
            Transform projectileRoot,
            bool participationRegistered,
            Vector3 spawnPosition,
            Quaternion spawnRotation)
        {
            SourceOrdinal = sourceOrdinal;
            SpawnId = spawnId ?? string.Empty;
            AnchorId = anchorId ?? string.Empty;
            PayloadId = payloadId ?? string.Empty;
            PositionId = positionId;
            DelaySeconds = delaySeconds;
            State = state;
            ActivationSequence = activationSequence;
            TerminalSequence = terminalSequence;
            Root = root;
            Health = health;
            Agent = agent;
            Sensor = sensor;
            ProjectileDriver = projectileDriver;
            ProjectileRoot = projectileRoot;
            ParticipationRegistered = participationRegistered;
            SpawnPosition = spawnPosition;
            SpawnRotation = spawnRotation;
        }

        public int SourceOrdinal { get; }
        public string SpawnId { get; }
        public string AnchorId { get; }
        public string PayloadId { get; }
        public int PositionId { get; }
        public float DelaySeconds { get; }
        public StageAddEncounterTicketState State { get; }
        public int ActivationSequence { get; }
        public int TerminalSequence { get; }
        public GameObject Root { get; }
        public CombatHealth Health { get; }
        public ICombatAiAgent Agent { get; }
        public CombatTargetSensor Sensor { get; }
        public BasicSoldierProjectileAttackDriver ProjectileDriver { get; }
        public Transform ProjectileRoot { get; }
        public bool ParticipationRegistered { get; }
        public Vector3 SpawnPosition { get; }
        public Quaternion SpawnRotation { get; }
        public bool WasActivated => ActivationSequence > 0;
        public bool HasOwnedHierarchy => Root != null;
    }

    public readonly struct StageAddEncounterTicketReceipt
    {
        public StageAddEncounterTicketReceipt(
            int sourceOrdinal,
            string spawnId,
            string anchorId,
            string payloadId,
            int positionId,
            float delaySeconds,
            StageAddEncounterTicketState finalState,
            int activationSequence,
            int terminalSequence,
            bool participationReleased,
            bool hierarchyInactive)
        {
            SourceOrdinal = sourceOrdinal;
            SpawnId = spawnId ?? string.Empty;
            AnchorId = anchorId ?? string.Empty;
            PayloadId = payloadId ?? string.Empty;
            PositionId = positionId;
            DelaySeconds = delaySeconds;
            FinalState = finalState;
            ActivationSequence = activationSequence;
            TerminalSequence = terminalSequence;
            ParticipationReleased = participationReleased;
            HierarchyInactive = hierarchyInactive;
        }

        public int SourceOrdinal { get; }
        public string SpawnId { get; }
        public string AnchorId { get; }
        public string PayloadId { get; }
        public int PositionId { get; }
        public float DelaySeconds { get; }
        public StageAddEncounterTicketState FinalState { get; }
        public int ActivationSequence { get; }
        public int TerminalSequence { get; }
        public bool ParticipationReleased { get; }
        public bool HierarchyInactive { get; }
        public bool WasActivated => ActivationSequence > 0;
    }

    public sealed class StageAddEncounterPlanReceipt
    {
        private readonly StageAddEncounterTicketReceipt[] tickets;

        public StageAddEncounterPlanReceipt(
            StageCountOneEncounterState finalState,
            string reason,
            int closeSequence,
            StageAddEncounterTicketReceipt[] ticketReceipts,
            bool quiescent)
        {
            FinalState = finalState;
            Reason = reason ?? string.Empty;
            CloseSequence = closeSequence;
            tickets = ticketReceipts != null
                ? (StageAddEncounterTicketReceipt[])ticketReceipts.Clone()
                : Array.Empty<StageAddEncounterTicketReceipt>();
            IsQuiescent = quiescent;
        }

        public StageCountOneEncounterState FinalState { get; }
        public string Reason { get; }
        public int CloseSequence { get; }
        public int TicketCount => tickets.Length;
        public bool IsQuiescent { get; }

        public StageAddEncounterTicketReceipt GetTicket(int index)
        {
            if (index < 0 || index >= tickets.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return tickets[index];
        }

        public bool TryValidateIntegrity(out string error)
        {
            error = string.Empty;
            if (FinalState != StageCountOneEncounterState.Completed
                && FinalState != StageCountOneEncounterState.Cancelled
                && FinalState != StageCountOneEncounterState.Faulted)
            {
                error = "The ordered Add receipt requires a terminal executor state.";
                return false;
            }

            if (CloseSequence <= 0 || !IsQuiescent)
            {
                error = "The ordered Add receipt requires a positive close sequence and quiescent cleanup.";
                return false;
            }

            var spawnIds = new HashSet<string>(StringComparer.Ordinal);
            var anchorIds = new HashSet<string>(StringComparer.Ordinal);
            var positionIds = new HashSet<int>();
            float priorDelay = -1f;
            int priorActivationSequence = 0;
            bool containsCancelled = false;
            bool containsFaulted = false;
            for (int i = 0; i < tickets.Length; i++)
            {
                StageAddEncounterTicketReceipt ticket = tickets[i];
                if (ticket.SourceOrdinal < 0
                    || string.IsNullOrWhiteSpace(ticket.SpawnId)
                    || !spawnIds.Add(ticket.SpawnId)
                    || string.IsNullOrWhiteSpace(ticket.AnchorId)
                    || !anchorIds.Add(ticket.AnchorId)
                    || string.IsNullOrWhiteSpace(ticket.PayloadId)
                    || ticket.PositionId <= 0
                    || !positionIds.Add(ticket.PositionId)
                    || !float.IsFinite(ticket.DelaySeconds)
                    || ticket.DelaySeconds < 0f
                    || ticket.DelaySeconds < priorDelay
                    || !Enum.IsDefined(typeof(StageAddEncounterTicketState), ticket.FinalState)
                    || ticket.FinalState == StageAddEncounterTicketState.Pending
                    || ticket.FinalState == StageAddEncounterTicketState.Active
                    || ticket.TerminalSequence <= 0
                    || ticket.TerminalSequence >= CloseSequence
                    || !ticket.ParticipationReleased
                    || !ticket.HierarchyInactive)
                {
                    error = $"Ordered Add ticket receipt {i} is incomplete or non-quiescent.";
                    return false;
                }

                if (i > 0 && tickets[i - 1].SourceOrdinal >= ticket.SourceOrdinal)
                {
                    error = "Ordered Add ticket receipts are not in strict source order.";
                    return false;
                }

                priorDelay = ticket.DelaySeconds;
                if (ticket.WasActivated)
                {
                    if (ticket.ActivationSequence <= priorActivationSequence
                        || ticket.ActivationSequence >= ticket.TerminalSequence)
                    {
                        error = "Ordered Add activation and terminal sequences are inconsistent.";
                        return false;
                    }

                    priorActivationSequence = ticket.ActivationSequence;
                }
                else if (ticket.FinalState == StageAddEncounterTicketState.Completed)
                {
                    error = "A completed ordered Add ticket was never activated.";
                    return false;
                }

                containsCancelled |= ticket.FinalState == StageAddEncounterTicketState.Cancelled;
                containsFaulted |= ticket.FinalState == StageAddEncounterTicketState.Faulted;
            }

            if (FinalState == StageCountOneEncounterState.Completed)
            {
                if (tickets.Length == 0)
                {
                    error = "A completed ordered Add receipt requires at least one ticket.";
                    return false;
                }

                for (int i = 0; i < tickets.Length; i++)
                {
                    if (tickets[i].FinalState != StageAddEncounterTicketState.Completed)
                    {
                        error = "A completed ordered Add receipt contains a non-completed ticket.";
                        return false;
                    }
                }
            }
            else if (FinalState == StageCountOneEncounterState.Cancelled)
            {
                if (tickets.Length == 0 || !containsCancelled || containsFaulted)
                {
                    error = "A cancelled ordered Add receipt requires at least one cancelled ticket and no faulted tickets.";
                    return false;
                }
            }
            else if (tickets.Length > 0 && !containsFaulted)
            {
                error = "A faulted ordered Add receipt with tickets requires at least one faulted ticket.";
                return false;
            }

            return true;
        }
    }

    [DefaultExecutionOrder(9500)]
    [DisallowMultipleComponent]
    public sealed class StageCountOneEncounterExecutor : MonoBehaviour
    {
        private const float PositionTolerance = 0.0001f;
        private const float RotationToleranceDegrees = 0.001f;

        private static readonly Dictionary<int, StageCountOneEncounterExecutor> SceneOwners = new();

        [SerializeField] private StageDefinitionSceneBinding sceneBinding;
        [SerializeField] private StageEncounterActivationKind activationKind =
            StageEncounterActivationKind.CombatEntryGuideReleased;
        [SerializeField] private bool requireActiveStageRun = true;
        [SerializeField] private bool cancelOnTerminalEncounter = true;

        private readonly List<Ticket> tickets = new();
        private ICombatEntryGuideGate guideGate;
        private CombatEncounterController terminalEncounter;
        private CombatHealth terminalPlayerHealth;
        private PlayerCombatTargetSelector playerTargetSelector;
        private bool runtimeInitialized;
        private bool planStaged;
        private bool activationInProgress;
        private bool activationEpochArmed;
        private bool healthInactiveSubscribed;
        private float activationEpoch;
        private int eventSequence;

        public StageDefinitionSceneBinding SceneBinding => sceneBinding;
        public StageEncounterActivationKind ActivationKind => activationKind;
        public bool RequiresActiveStageRun => requireActiveStageRun;
        public bool CancelsOnTerminalEncounter => cancelOnTerminalEncounter;
        public StageCountOneEncounterState State { get; private set; }
        public string LastError { get; private set; } = string.Empty;
        public PlayerCombatTargetSelector PlayerTargetSelector => playerTargetSelector;
        public int TicketCount => tickets.Count;
        public int PendingTicketCount => CountTickets(StageAddEncounterTicketState.Pending);
        public int ActiveTicketCount => CountTickets(StageAddEncounterTicketState.Active);
        public int CompletedTicketCount => CountTickets(StageAddEncounterTicketState.Completed);
        public int CancelledTicketCount => CountTickets(StageAddEncounterTicketState.Cancelled);
        public int FaultedTicketCount => CountTickets(StageAddEncounterTicketState.Faulted);
        public int ActivatedTicketCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < tickets.Count; i++)
                {
                    if (tickets[i].ActivationSequence > 0)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        public int ActiveParticipationCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < tickets.Count; i++)
                {
                    if (tickets[i].ParticipationRegistered)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        public bool HasCombatantParticipation => ActiveParticipationCount > 0;
        public int OwnedObjectCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < tickets.Count; i++)
                {
                    if (tickets[i].Root != null)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        public GameObject OwnedRoot => FindFirstOwnedTicket()?.Root;
        public CombatHealth OwnedHealth => FindFirstActiveTicket()?.Health;
        public ICombatAiAgent OwnedAgent => FindFirstActiveTicket()?.Agent;
        public CombatTargetSensor OwnedSensor => FindFirstActiveTicket()?.Sensor;
        public bool HasSceneLease { get; private set; }
        public int ActivationCount { get; private set; }
        public int CompletionCount { get; private set; }
        public int CancellationCount { get; private set; }
        public int FaultCount { get; private set; }
        public Vector3 LastSpawnPosition { get; private set; }
        public Quaternion LastSpawnRotation { get; private set; } = Quaternion.identity;
        public StageAddEncounterPlanReceipt LastReceipt { get; private set; }
        public bool IsQuiescent
        {
            get
            {
                if (OwnedObjectCount != 0 || ActiveParticipationCount != 0)
                {
                    return false;
                }

                for (int i = 0; i < tickets.Count; i++)
                {
                    if (!tickets[i].ParticipationReleasedAtCleanup
                        || !tickets[i].HierarchyInactiveAtCleanup)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSceneOwners()
        {
            SceneOwners.Clear();
        }

        public StageAddEncounterTicketSnapshot GetTicketSnapshot(int index)
        {
            if (index < 0 || index >= tickets.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            Ticket ticket = tickets[index];
            return new StageAddEncounterTicketSnapshot(
                ticket.SourceOrdinal,
                ticket.Spawn.SpawnId,
                ticket.Spawn.AnchorId,
                ticket.Spawn.PayloadId,
                ticket.Spawn.PositionId,
                ticket.Spawn.AuthoredDelaySeconds,
                ticket.State,
                ticket.ActivationSequence,
                ticket.TerminalSequence,
                ticket.Root,
                ticket.Health,
                ticket.Agent,
                ticket.Sensor,
                ticket.ProjectileDriver,
                ticket.ProjectileRoot,
                ticket.ParticipationRegistered,
                ticket.SpawnPosition,
                ticket.SpawnRotation);
        }

        public bool TryActivate(out string error)
        {
            error = string.Empty;
            if (IsTerminalState())
            {
                error = "The ordered Add encounter has already reached its terminal state.";
                return false;
            }

            if (!TryInitializeRuntime(out error))
            {
                FailPlan(null, error);
                return false;
            }

            if (!TryValidateActiveRun(out error))
            {
                if (planStaged || ActivatedTicketCount > 0)
                {
                    CancelPlan(error);
                }

                return false;
            }

            if (!TryEnsurePlanStaged(out Ticket stagingFault, out error))
            {
                FailPlan(stagingFault, error);
                return false;
            }

            if (activationKind == StageEncounterActivationKind.CombatEntryGuideReleased
                && guideGate.State != CombatEntryGuideState.Released)
            {
                error = "The combat-entry guide has not released gameplay.";
                return false;
            }

            if (terminalEncounter != null
                && (terminalEncounter.IsWon || terminalEncounter.IsFailed || terminalEncounter.IsFaulted))
            {
                error = "The authoritative encounter is already terminal.";
                CancelPlan(error);
                return false;
            }

            ArmActivationEpoch();
            int activatedBefore = ActivatedTicketCount;
            if (!TryActivateDueTickets(out error))
            {
                return false;
            }

            RefreshPlanState();
            if (ActivatedTicketCount == activatedBefore && PendingTicketCount > 0)
            {
                error = "No ordered Add ticket has reached its authored release-relative deadline.";
                return false;
            }

            return State == StageCountOneEncounterState.Active
                || State == StageCountOneEncounterState.Completed;
        }

        private void Start()
        {
            EvaluateActivation();
        }

        private void Update()
        {
            if (IsTerminalState())
            {
                return;
            }

            if (!runtimeInitialized || !planStaged)
            {
                EvaluateActivation();
                return;
            }

            if (!TryValidateActiveRun(out string runError))
            {
                CancelPlan(runError);
                return;
            }

            for (int i = 0; i < tickets.Count; i++)
            {
                Ticket ticket = tickets[i];
                if (ticket.State == StageAddEncounterTicketState.Active)
                {
                    if (!TryValidateActiveTicket(ticket, out string ticketError))
                    {
                        FailPlan(ticket, ticketError);
                        return;
                    }

                    if (ticket.Health != null && !ticket.Health.IsAlive)
                    {
                        CompleteTicket(ticket);
                        if (IsTerminalState())
                        {
                            return;
                        }
                    }
                }
                else if (ticket.State == StageAddEncounterTicketState.Pending
                    && (ticket.Root == null
                        || ticket.Root.activeSelf
                        || ticket.ParticipationRegistered))
                {
                    FailPlan(ticket, "A pending ordered Add ticket lost its inactive staged ownership boundary.");
                    return;
                }
            }

            if (PendingTicketCount > 0)
            {
                if (activationKind == StageEncounterActivationKind.CombatEntryGuideReleased)
                {
                    if (guideGate.State == CombatEntryGuideState.Interrupted)
                    {
                        CancelPlan("The combat-entry guide was interrupted before all Add tickets activated.");
                        return;
                    }

                    if (guideGate.State != CombatEntryGuideState.Released)
                    {
                        State = StageCountOneEncounterState.WaitingForActivation;
                        return;
                    }
                }

                ArmActivationEpoch();
                if (!TryActivateDueTickets(out _))
                {
                    return;
                }
            }

            RefreshPlanState();
        }

        private void OnDisable()
        {
            Shutdown();
        }

        private void OnDestroy()
        {
            Shutdown();
        }

        private void EvaluateActivation()
        {
            if (IsTerminalState())
            {
                return;
            }

            if (!TryInitializeRuntime(out string initializationError))
            {
                FailPlan(null, initializationError);
                return;
            }

            if (terminalEncounter != null
                && (terminalEncounter.IsWon || terminalEncounter.IsFailed || terminalEncounter.IsFaulted))
            {
                CancelPlan("The authoritative encounter ended before the ordered Add plan activated.");
                return;
            }

            if (!TryValidateActiveRun(out string runError))
            {
                State = StageCountOneEncounterState.WaitingForRun;
                LastError = runError;
                return;
            }

            if (!TryEnsurePlanStaged(out Ticket stagingFault, out string stagingError))
            {
                FailPlan(stagingFault, stagingError);
                return;
            }

            if (activationKind == StageEncounterActivationKind.CombatEntryGuideReleased)
            {
                if (guideGate.State == CombatEntryGuideState.Interrupted)
                {
                    CancelPlan("The combat-entry guide was interrupted before releasing gameplay.");
                    return;
                }

                if (guideGate.State != CombatEntryGuideState.Released)
                {
                    State = StageCountOneEncounterState.WaitingForActivation;
                    LastError = string.Empty;
                    return;
                }
            }

            ArmActivationEpoch();
            TryActivateDueTickets(out _);
            RefreshPlanState();
        }

        private bool TryInitializeRuntime(out string error)
        {
            error = string.Empty;
            if (runtimeInitialized)
            {
                return true;
            }

            if (!TryResolveAuthoring(out error)
                || !TryResolveGuideGate(out error)
                || !TryResolveTerminalEncounter(out error)
                || !TryResolveCombatantParticipationOwner(out error))
            {
                return false;
            }

            if (guideGate != null)
            {
                guideGate.StateChanged -= HandleGuideStateChanged;
                guideGate.StateChanged += HandleGuideStateChanged;
            }

            if (terminalEncounter != null && cancelOnTerminalEncounter)
            {
                terminalEncounter.Won -= HandleTerminalEncounterEnded;
                terminalEncounter.Failed -= HandleTerminalEncounterEnded;
                terminalEncounter.DiagnosticAborted -= HandleTerminalEncounterFaulted;
                terminalEncounter.Won += HandleTerminalEncounterEnded;
                terminalEncounter.Failed += HandleTerminalEncounterEnded;
                terminalEncounter.DiagnosticAborted += HandleTerminalEncounterFaulted;
            }

            runtimeInitialized = true;
            return true;
        }

        private bool TryResolveAuthoring(out string error)
        {
            error = string.Empty;
            Scene scene = gameObject.scene;
            if (!scene.IsValid() || !scene.isLoaded)
            {
                error = "The ordered Add executor does not belong to a loaded scene.";
                return false;
            }

            if (sceneBinding == null || sceneBinding.gameObject.scene != scene)
            {
                error = "The ordered Add executor requires a scene-local StageDefinitionSceneBinding.";
                return false;
            }

            StageDefinitionProfile definition = sceneBinding.StageDefinition;
            if (definition == null)
            {
                error = "The scene binding has no StageDefinitionProfile.";
                return false;
            }

            if (!string.Equals(definition.MapScenePath, scene.path, StringComparison.Ordinal))
            {
                error = "The bound stage definition does not own the executor scene path.";
                return false;
            }

            var resolved = new List<Ticket>();
            var spawnIds = new HashSet<string>(StringComparer.Ordinal);
            var anchorIds = new HashSet<string>(StringComparer.Ordinal);
            var positionIds = new HashSet<int>();
            float priorDelay = -1f;
            for (int sourceOrdinal = 0; sourceOrdinal < definition.SpawnCount; sourceOrdinal++)
            {
                StageDefinitionProfile.SpawnRef spawn = definition.GetSpawn(sourceOrdinal);
                if (spawn.SpawnKind != StageSpawnKind.Add)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(spawn.SpawnId)
                    || !spawnIds.Add(spawn.SpawnId)
                    || string.IsNullOrWhiteSpace(spawn.AnchorId)
                    || !anchorIds.Add(spawn.AnchorId)
                    || spawn.PositionId <= 0
                    || !positionIds.Add(spawn.PositionId)
                    || string.IsNullOrWhiteSpace(spawn.PayloadId)
                    || spawn.AuthoredCount != 1
                    || !float.IsFinite(spawn.AuthoredDelaySeconds)
                    || spawn.AuthoredDelaySeconds < 0f
                    || spawn.AuthoredDelaySeconds < priorDelay)
                {
                    error = $"Add row {sourceOrdinal} is malformed, duplicated, or not in nondecreasing delay order.";
                    return false;
                }

                priorDelay = spawn.AuthoredDelaySeconds;
                int expectedAnchorCount = 0;
                StageDefinitionProfile.AnchorRef expectedAnchor = default;
                for (int anchorIndex = 0; anchorIndex < definition.AnchorCount; anchorIndex++)
                {
                    StageDefinitionProfile.AnchorRef candidate = definition.GetAnchor(anchorIndex);
                    if (string.Equals(candidate.AnchorId, spawn.AnchorId, StringComparison.Ordinal))
                    {
                        expectedAnchor = candidate;
                        expectedAnchorCount++;
                    }
                }

                int liveAnchorCount = 0;
                StageAnchorPoint liveAnchor = null;
                for (int anchorIndex = 0; anchorIndex < sceneBinding.AnchorPointCount; anchorIndex++)
                {
                    StageAnchorPoint candidate = sceneBinding.GetAnchorPoint(anchorIndex);
                    if (candidate != null
                        && string.Equals(candidate.AnchorId, spawn.AnchorId, StringComparison.Ordinal))
                    {
                        liveAnchor = candidate;
                        liveAnchorCount++;
                    }
                }

                if (expectedAnchorCount != 1
                    || liveAnchorCount != 1
                    || liveAnchor == null
                    || string.IsNullOrWhiteSpace(expectedAnchor.GroupId)
                    || !IsFinite(expectedAnchor.ExpectedPosition)
                    || !IsFinite(expectedAnchor.ExpectedEuler)
                    || !IsFinite(liveAnchor.transform.position)
                    || !IsFinite(liveAnchor.transform.rotation)
                    || liveAnchor.gameObject.scene != scene
                    || !liveAnchor.transform.IsChildOf(sceneBinding.transform)
                    || liveAnchor.UsageKind != StageAnchorUsageKind.CombatSpawn
                    || liveAnchor.SpawnKind != StageSpawnKind.Add
                    || liveAnchor.PositionId != spawn.PositionId
                    || !string.Equals(liveAnchor.GroupId, expectedAnchor.GroupId, StringComparison.Ordinal))
                {
                    error = $"Add row '{spawn.SpawnId}' does not resolve to one matching live spawn anchor.";
                    return false;
                }

                Vector3 localPosition = sceneBinding.transform.InverseTransformPoint(
                    liveAnchor.transform.position);
                Quaternion localRotation = Quaternion.Inverse(sceneBinding.transform.rotation)
                    * liveAnchor.transform.rotation;
                if (!Approximately(localPosition, expectedAnchor.ExpectedPosition, PositionTolerance)
                    || Quaternion.Angle(
                            localRotation,
                            Quaternion.Euler(expectedAnchor.ExpectedEuler))
                        > RotationToleranceDegrees)
                {
                    error = $"Live Add anchor '{spawn.AnchorId}' does not match its binding-root-relative authored pose.";
                    return false;
                }

                CombatEnemyArchetypeProfile archetype = spawn.PayloadArchetype;
                GameObject prefab = archetype != null ? archetype.GameplayPrefab : null;
                CombatHealth[] prefabHealth = prefab != null
                    ? prefab.GetComponentsInChildren<CombatHealth>(true)
                    : Array.Empty<CombatHealth>();
                if (archetype == null
                    || !string.Equals(archetype.ArchetypeId, spawn.PayloadId, StringComparison.Ordinal)
                    || archetype.RequiresDedicatedPrefabPromotion
                    || prefab == null
                    || prefabHealth.Length != 1
                    || prefabHealth[0].Team != DamageTeam.Enemy
                    || !prefabHealth[0].enabled
                    || !prefabHealth[0].gameObject.activeSelf)
                {
                    error = $"Add row '{spawn.SpawnId}' must directly own one promoted archetype with exactly one enabled, active-self Enemy CombatHealth prefab.";
                    return false;
                }

                resolved.Add(new Ticket(
                    sourceOrdinal,
                    spawn,
                    expectedAnchor,
                    liveAnchor,
                    archetype,
                    prefab));
            }

            if (resolved.Count == 0)
            {
                error = "The ordered Add executor requires at least one authored Add row.";
                return false;
            }

            tickets.Clear();
            tickets.AddRange(resolved);
            return true;
        }

        private bool TryResolveGuideGate(out string error)
        {
            error = string.Empty;
            if (activationKind != StageEncounterActivationKind.CombatEntryGuideReleased)
            {
                guideGate = null;
                return true;
            }

            int matchCount = 0;
            GameObject[] roots = gameObject.scene.GetRootGameObjects();
            for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
            {
                MonoBehaviour[] behaviours = roots[rootIndex].GetComponentsInChildren<MonoBehaviour>(true);
                for (int componentIndex = 0; componentIndex < behaviours.Length; componentIndex++)
                {
                    if (behaviours[componentIndex] is ICombatEntryGuideGate candidate)
                    {
                        guideGate = candidate;
                        matchCount++;
                    }
                }
            }

            if (matchCount != 1)
            {
                error = "Guide-release activation requires exactly one scene-local ICombatEntryGuideGate.";
                return false;
            }

            return true;
        }

        private bool TryResolveTerminalEncounter(out string error)
        {
            error = string.Empty;
            int matchCount = 0;
            GameObject[] roots = gameObject.scene.GetRootGameObjects();
            for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
            {
                CombatEncounterController[] encounters =
                    roots[rootIndex].GetComponentsInChildren<CombatEncounterController>(true);
                for (int componentIndex = 0; componentIndex < encounters.Length; componentIndex++)
                {
                    terminalEncounter = encounters[componentIndex];
                    matchCount++;
                }
            }

            if (matchCount != 1)
            {
                error = "Ordered Add participation requires exactly one scene-local CombatEncounterController.";
                return false;
            }

            return true;
        }

        private bool TryResolveCombatantParticipationOwner(out string error)
        {
            error = string.Empty;
            Scene scene = gameObject.scene;
            terminalPlayerHealth = terminalEncounter != null ? terminalEncounter.PlayerHealth : null;
            CombatHealth terminalEnemyHealth = terminalEncounter != null
                ? terminalEncounter.EnemyHealth
                : null;
            if (terminalPlayerHealth == null
                || terminalEnemyHealth == null
                || terminalPlayerHealth.gameObject.scene != scene
                || terminalEnemyHealth.gameObject.scene != scene
                || terminalPlayerHealth.Team != DamageTeam.Player
                || terminalEnemyHealth.Team != DamageTeam.Enemy)
            {
                error = "Add participation requires the exact scene-local terminal player and boss health pair.";
                return false;
            }

            PlayerCombatTargetSelector[] selectors =
                terminalPlayerHealth.GetComponents<PlayerCombatTargetSelector>();
            if (selectors.Length != 1
                || !ReferenceEquals(selectors[0].SelfHealth, terminalPlayerHealth)
                || !selectors[0].ContainsAuthoredTargetCandidate(terminalEnemyHealth))
            {
                error = "Add participation requires one player selector that retains the authored terminal boss.";
                return false;
            }

            playerTargetSelector = selectors[0];
            return true;
        }

        private bool TryEnsurePlanStaged(out Ticket faultTicket, out string error)
        {
            faultTicket = null;
            error = string.Empty;
            if (planStaged)
            {
                return true;
            }

            if (!TryAcquireSceneLease(out error))
            {
                return false;
            }

            try
            {
                for (int i = 0; i < tickets.Count; i++)
                {
                    Ticket ticket = tickets[i];
                    faultTicket = ticket;
                    GameObject root = new GameObject(
                        $"[Runtime] Stage Add {ticket.SourceOrdinal:00} {ticket.Spawn.SpawnId}");
                    root.SetActive(false);
                    SceneManager.MoveGameObjectToScene(root, gameObject.scene);
                    root.transform.SetParent(sceneBinding.transform, true);
                    ticket.Root = root;
                    ticket.SpawnPosition = ticket.Anchor.transform.position;
                    ticket.SpawnRotation = ticket.Anchor.transform.rotation;

                    GameObject instance = Instantiate(
                        ticket.Prefab,
                        ticket.SpawnPosition,
                        ticket.SpawnRotation,
                        root.transform);
                    CombatHealth[] healthComponents =
                        instance.GetComponentsInChildren<CombatHealth>(true);
                    if (healthComponents.Length != 1
                        || healthComponents[0].Team != DamageTeam.Enemy
                        || !healthComponents[0].enabled
                        || !healthComponents[0].gameObject.activeSelf)
                    {
                        error = $"Staged ticket '{ticket.Spawn.SpawnId}' does not contain exactly one enabled, active-self Enemy CombatHealth.";
                        return false;
                    }

                    ticket.Health = healthComponents[0];
                    if (!TryPrepareTicketCombatant(ticket, instance, out error))
                    {
                        return false;
                    }

                    Ticket captured = ticket;
                    ticket.DeathHandler = () => HandleTicketHealthDied(captured);
                    ticket.Health.Died += ticket.DeathHandler;
                }

                CombatHealth.BecameInactive -= HandleCombatHealthBecameInactive;
                CombatHealth.BecameInactive += HandleCombatHealthBecameInactive;
                healthInactiveSubscribed = true;
                planStaged = true;
                faultTicket = null;
                return true;
            }
            catch (Exception exception)
            {
                error = $"Ordered Add staging failed: {exception.GetType().Name}: {exception.Message}";
                return false;
            }
        }

        private bool TryPrepareTicketCombatant(
            Ticket ticket,
            GameObject instance,
            out string error)
        {
            error = string.Empty;
            ICombatAiAgent resolvedAgent = null;
            int agentCount = 0;
            MonoBehaviour[] behaviours = instance.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is ICombatAiAgent candidate)
                {
                    resolvedAgent = candidate;
                    agentCount++;
                }
            }

            CombatTargetSensor resolvedSensor = resolvedAgent?.TargetSensor;
            MonoBehaviour resolvedAgentBehaviour = resolvedAgent as MonoBehaviour;
            CombatTargetSensor[] resolvedSensors =
                instance.GetComponentsInChildren<CombatTargetSensor>(true);
            SummonFrontlineProxy[] summonProxies =
                instance.GetComponentsInChildren<SummonFrontlineProxy>(true);
            BasicSoldierProjectileAttackDriver[] projectileDrivers =
                instance.GetComponentsInChildren<BasicSoldierProjectileAttackDriver>(true);
            if (agentCount != 1
                || resolvedAgent == null
                || resolvedAgentBehaviour == null
                || !resolvedAgentBehaviour.enabled
                || !resolvedAgentBehaviour.gameObject.activeSelf
                || !ReferenceEquals(resolvedAgent.SelfHealth, ticket.Health)
                || resolvedSensor == null
                || !resolvedSensor.enabled
                || !resolvedSensor.gameObject.activeSelf
                || resolvedSensors.Length != 1
                || !ReferenceEquals(resolvedSensors[0], resolvedSensor)
                || !ReferenceEquals(resolvedSensor.SelfHealth, ticket.Health)
                || (resolvedSensor.transform != instance.transform
                    && !resolvedSensor.transform.IsChildOf(instance.transform))
                || resolvedSensor.TargetCandidateCount != 0
                || summonProxies.Length != 0)
            {
                error = $"Ticket '{ticket.Spawn.SpawnId}' must contain one coherent AI agent, one empty target sensor, and no summon proxy.";
                return false;
            }

            CombatAiPatternProfile participationPattern = resolvedAgent.PatternProfile;
            if (participationPattern == null
                || (resolvedSensor.SearchRadius > 0f
                    && resolvedSensor.SearchRadius + PositionTolerance
                        < participationPattern.AttackRange))
            {
                error = $"Ticket '{ticket.Spawn.SpawnId}' requires a starting pattern covered by its target sensor.";
                return false;
            }

            BasicSoldierProjectileAttackDriver projectileDriver = null;
            if (participationPattern.AttackShape == CombatAiAttackShape.ProjectileLine)
            {
                BasicSoldierEnemy basicSoldier = resolvedAgent as BasicSoldierEnemy;
                if (basicSoldier == null
                    || projectileDrivers.Length != 1
                    || projectileDrivers[0] == null
                    || !projectileDrivers[0].enabled
                    || !projectileDrivers[0].gameObject.activeSelf
                    || !projectileDrivers[0].IsConfiguredFor(
                        basicSoldier,
                        ticket.Health,
                        resolvedSensor))
                {
                    error = $"Ticket '{ticket.Spawn.SpawnId}' requires one coherent bounded projectile driver for its ProjectileLine pattern.";
                    return false;
                }

                projectileDriver = projectileDrivers[0];
                GameObject projectileRootObject = new GameObject("Projectiles");
                projectileRootObject.transform.SetParent(ticket.Root.transform, worldPositionStays: false);
                projectileRootObject.transform.localPosition = Vector3.zero;
                projectileRootObject.transform.localRotation = Quaternion.identity;
                projectileRootObject.transform.localScale = Vector3.one;
                ticket.ProjectileRoot = projectileRootObject.transform;
                projectileDriver.ConfigureRuntimeProjectileRoot(ticket.ProjectileRoot);
                if (!projectileDriver.HasIndependentRuntimeProjectileRoot
                    || !ReferenceEquals(ticket.ProjectileRoot.parent, ticket.Root.transform))
                {
                    error = $"Ticket '{ticket.Spawn.SpawnId}' failed to bind its fixed ticket-owned projectile root.";
                    return false;
                }
            }
            else if (projectileDrivers.Length != 0)
            {
                error = $"Ticket '{ticket.Spawn.SpawnId}' must not carry a projectile driver for a non-projectile starting pattern.";
                return false;
            }

            CombatHealth terminalEnemyHealth = terminalEncounter != null
                ? terminalEncounter.EnemyHealth
                : null;
            if (terminalPlayerHealth == null
                || terminalEnemyHealth == null
                || playerTargetSelector == null
                || !terminalPlayerHealth.IsAlive
                || !terminalEnemyHealth.IsAlive
                || !terminalPlayerHealth.isActiveAndEnabled
                || !terminalEnemyHealth.isActiveAndEnabled
                || !terminalPlayerHealth.gameObject.activeInHierarchy
                || !terminalEnemyHealth.gameObject.activeInHierarchy
                || !playerTargetSelector.isActiveAndEnabled
                || !playerTargetSelector.gameObject.activeInHierarchy)
            {
                error = "The exact terminal player, boss, and selector must be active before Add staging.";
                return false;
            }

            ticket.Agent = resolvedAgent;
            ticket.Sensor = resolvedSensor;
            ticket.ProjectileDriver = projectileDriver;
            ticket.Agent.ConfigureTarget(terminalPlayerHealth.transform, terminalPlayerHealth);
            ticket.Sensor.ConfigureTargetCandidates(
                new[] { terminalPlayerHealth },
                refreshNow: false);
            return true;
        }

        private void ArmActivationEpoch()
        {
            if (activationEpochArmed)
            {
                return;
            }

            activationEpoch = Time.time;
            activationEpochArmed = true;
        }

        private bool TryActivateDueTickets(out string error)
        {
            error = string.Empty;
            if (activationInProgress)
            {
                error = "The ordered Add activation batch is already in progress.";
                return false;
            }

            activationInProgress = true;
            Ticket faultTicket = null;
            Ticket activatingTicket = null;
            string faultError = string.Empty;
            try
            {
                for (int i = 0; i < tickets.Count; i++)
                {
                    Ticket ticket = tickets[i];
                    if (ticket.State != StageAddEncounterTicketState.Pending
                        || Time.time < activationEpoch + ticket.Spawn.AuthoredDelaySeconds)
                    {
                        continue;
                    }

                    activatingTicket = ticket;
                    if (!TryActivateTicket(ticket, out error))
                    {
                        faultTicket = ticket;
                        faultError = error;
                        break;
                    }
                }
            }
            catch (Exception exception)
            {
                faultTicket = activatingTicket;
                faultError =
                    $"Ordered Add activation threw {exception.GetType().Name}: {exception.Message}";
            }
            finally
            {
                activationInProgress = false;
            }

            if (faultTicket != null)
            {
                error = faultError;
                if (!IsTerminalState())
                {
                    FailPlan(faultTicket, faultError);
                }

                return false;
            }

            return true;
        }

        private bool TryActivateTicket(Ticket ticket, out string error)
        {
            error = string.Empty;
            if (ticket.Root == null
                || ticket.Health == null
                || ticket.Agent == null
                || ticket.Sensor == null
                || ticket.Root.activeSelf)
            {
                error = $"Pending ticket '{ticket.Spawn.SpawnId}' lost its staged instance before activation.";
                return false;
            }

            if (!TryValidateProjectileParticipationLease(
                    ticket,
                    requireActiveHierarchy: false,
                    out error))
            {
                return false;
            }

            ticket.Root.SetActive(true);
            MonoBehaviour agentBehaviour = ticket.Agent as MonoBehaviour;
            if (agentBehaviour == null
                || !agentBehaviour.isActiveAndEnabled
                || !ticket.Sensor.isActiveAndEnabled)
            {
                error = $"Activated ticket '{ticket.Spawn.SpawnId}' has an inactive AI agent or sensor.";
                return false;
            }

            if (!TryValidateProjectileParticipationLease(
                    ticket,
                    requireActiveHierarchy: true,
                    out error))
            {
                return false;
            }

            bool acquiredInitialSensorTarget = ticket.Sensor.RefreshTarget();
            if (acquiredInitialSensorTarget
                && !ReferenceEquals(ticket.Sensor.CurrentTargetHealth, terminalPlayerHealth))
            {
                error = $"Activated ticket '{ticket.Spawn.SpawnId}' acquired a subject other than the exact terminal player.";
                return false;
            }

            if (!playerTargetSelector.TryRegisterRuntimeTargetCandidate(
                    ticket.Health,
                    out string registrationError,
                    refreshNow: false))
            {
                error = $"The player selector rejected ticket '{ticket.Spawn.SpawnId}': {registrationError}";
                return false;
            }

            ticket.ParticipationRegistered = true;
            if (IsTerminalState()
                || ticket.Root == null
                || ticket.Health == null
                || !playerTargetSelector.ContainsRuntimeTargetCandidate(ticket.Health))
            {
                error = $"Ticket '{ticket.Spawn.SpawnId}' lost its participation lease during activation.";
                return false;
            }

            ticket.State = StageAddEncounterTicketState.Active;
            ticket.ActivationSequence = ++eventSequence;
            if (ActivationCount == 0)
            {
                ActivationCount = 1;
                LastSpawnPosition = ticket.SpawnPosition;
                LastSpawnRotation = ticket.SpawnRotation;
            }

            LastError = string.Empty;
            return true;
        }

        private bool TryValidateActiveTicket(Ticket ticket, out string error)
        {
            error = string.Empty;
            if (ticket.Root == null || ticket.Health == null)
            {
                error = $"Active ticket '{ticket.Spawn.SpawnId}' was destroyed outside its executor.";
                return false;
            }

            if (!ticket.Root.activeInHierarchy || !ticket.Health.isActiveAndEnabled)
            {
                error = $"Active ticket '{ticket.Spawn.SpawnId}' was disabled outside its executor.";
                return false;
            }

            MonoBehaviour agentBehaviour = ticket.Agent as MonoBehaviour;
            CombatHealth terminalEnemyHealth = terminalEncounter != null
                ? terminalEncounter.EnemyHealth
                : null;
            if (agentBehaviour == null
                || !agentBehaviour.isActiveAndEnabled
                || ticket.Sensor == null
                || !ticket.Sensor.isActiveAndEnabled
                || !ReferenceEquals(ticket.Agent.SelfHealth, ticket.Health)
                || !ReferenceEquals(ticket.Agent.TargetSensor, ticket.Sensor)
                || ticket.Sensor.TargetCandidateCount != 1
                || !ticket.Sensor.ContainsTargetCandidate(terminalPlayerHealth)
                || terminalPlayerHealth == null
                || !terminalPlayerHealth.IsAlive
                || !terminalPlayerHealth.isActiveAndEnabled
                || playerTargetSelector == null
                || !playerTargetSelector.isActiveAndEnabled
                || !ticket.ParticipationRegistered
                || !playerTargetSelector.ContainsRuntimeTargetCandidate(ticket.Health)
                || terminalEnemyHealth == null
                || !playerTargetSelector.ContainsAuthoredTargetCandidate(terminalEnemyHealth))
            {
                error = $"Active ticket '{ticket.Spawn.SpawnId}' lost its exact bidirectional combatant participation lease.";
                return false;
            }

            if ((ticket.Agent.CurrentPatternState == CombatAiPatternState.Windup
                    || ticket.Agent.CurrentPatternState == CombatAiPatternState.AttackActive)
                && !ReferenceEquals(ticket.Sensor.CurrentTargetHealth, terminalPlayerHealth))
            {
                error = $"Active ticket '{ticket.Spawn.SpawnId}' entered an attack phase without the exact terminal player.";
                return false;
            }

            if (!TryValidateProjectileParticipationLease(
                    ticket,
                    requireActiveHierarchy: true,
                    out error))
            {
                return false;
            }

            return true;
        }

        private static bool TryValidateProjectileParticipationLease(
            Ticket ticket,
            bool requireActiveHierarchy,
            out string error)
        {
            error = string.Empty;
            if (ticket == null || ticket.Agent == null)
            {
                error = "An ordered Add ticket lost its AI owner before projectile lease validation.";
                return false;
            }

            BasicSoldierProjectileAttackDriver driver = ticket.ProjectileDriver;
            Transform projectileRoot = ticket.ProjectileRoot;
            bool currentPatternRequiresProjectile = ticket.Agent.PatternProfile != null
                && ticket.Agent.PatternProfile.AttackShape == CombatAiAttackShape.ProjectileLine;
            if (driver == null)
            {
                if (projectileRoot != null || currentPatternRequiresProjectile)
                {
                    error = $"Ticket '{ticket.Spawn.SpawnId}' lost its exact projectile participation lease.";
                    return false;
                }

                return true;
            }

            BasicSoldierEnemy soldier = ticket.Agent as BasicSoldierEnemy;
            bool activeHierarchyIsValid = !requireActiveHierarchy
                || (driver.isActiveAndEnabled
                    && projectileRoot != null
                    && projectileRoot.gameObject.activeInHierarchy);
            if (soldier == null
                || ticket.Health == null
                || ticket.Sensor == null
                || ticket.Root == null
                || projectileRoot == null
                || !driver.enabled
                || !driver.gameObject.activeSelf
                || !activeHierarchyIsValid
                || !driver.IsConfiguredFor(soldier, ticket.Health, ticket.Sensor)
                || !ReferenceEquals(driver.RuntimeProjectileRoot, projectileRoot)
                || !driver.HasIndependentRuntimeProjectileRoot
                || !ReferenceEquals(projectileRoot.parent, ticket.Root.transform)
                || driver.OwnedProjectileCount > driver.MaxOwnedProjectileCount
                || driver.ActiveProjectileCount > driver.OwnedProjectileCount)
            {
                error = $"Ticket '{ticket.Spawn.SpawnId}' lost its exact projectile participation lease.";
                return false;
            }

            return true;
        }

        private bool TryValidateActiveRun(out string error)
        {
            error = string.Empty;
            if (!requireActiveStageRun)
            {
                return true;
            }

            StageRunContext context = StageRunRuntime.ActiveContext;
            StageDefinitionProfile definition = sceneBinding != null
                ? sceneBinding.StageDefinition
                : null;
            Scene scene = gameObject.scene;
            if (context == null)
            {
                error = "No canonical stage run owns this Station scene.";
                return false;
            }

            if (context.LifecycleState != StageRunLifecycleState.StationActive
                || context.CurrentSceneHandle != scene.handle
                || definition == null
                || !string.Equals(
                    context.CurrentSegment.StageDefinitionId,
                    definition.StageId,
                    StringComparison.Ordinal)
                || !string.Equals(context.CurrentSegment.ScenePath, scene.path, StringComparison.Ordinal))
            {
                error = "The active run does not own this exact Station segment and scene.";
                return false;
            }

            return true;
        }

        private bool TryAcquireSceneLease(out string error)
        {
            error = string.Empty;
            int sceneHandle = gameObject.scene.handle;
            if (SceneOwners.TryGetValue(sceneHandle, out StageCountOneEncounterExecutor owner))
            {
                if (owner == null)
                {
                    SceneOwners.Remove(sceneHandle);
                }
                else if (!ReferenceEquals(owner, this))
                {
                    error = "Another ordered Add executor already owns this loaded scene.";
                    return false;
                }
            }

            SceneOwners[sceneHandle] = this;
            HasSceneLease = true;
            return true;
        }

        private void ReleaseSceneLease()
        {
            if (!HasSceneLease)
            {
                return;
            }

            int sceneHandle = gameObject.scene.handle;
            if (SceneOwners.TryGetValue(sceneHandle, out StageCountOneEncounterExecutor owner)
                && ReferenceEquals(owner, this))
            {
                SceneOwners.Remove(sceneHandle);
            }

            HasSceneLease = false;
        }

        private void HandleGuideStateChanged(CombatEntryGuideState guideState)
        {
            if (guideState == CombatEntryGuideState.Interrupted)
            {
                CancelPlan("The combat-entry guide was interrupted before ordered Add activation completed.");
                return;
            }

            if (guideState == CombatEntryGuideState.Released)
            {
                EvaluateActivation();
            }
        }

        private void HandleTicketHealthDied(Ticket ticket)
        {
            if (ticket != null && ticket.State == StageAddEncounterTicketState.Active)
            {
                CompleteTicket(ticket);
            }
        }

        private void HandleCombatHealthBecameInactive(CombatHealth health)
        {
            CombatHealth terminalEnemyHealth = terminalEncounter != null
                ? terminalEncounter.EnemyHealth
                : null;
            if (ReferenceEquals(health, terminalPlayerHealth)
                || ReferenceEquals(health, terminalEnemyHealth))
            {
                if (!IsTerminalState())
                {
                    CancelPlan("An authoritative terminal combat subject became inactive during Add participation.");
                }

                return;
            }

            Ticket ticket = FindTicketByHealth(health);
            if (ticket == null || ticket.State != StageAddEncounterTicketState.Active)
            {
                return;
            }

            Scene ownerScene = gameObject.scene;
            Scene activeScene = SceneManager.GetActiveScene();
            bool sceneExitInProgress = !ownerScene.isLoaded
                || !activeScene.IsValid()
                || activeScene.handle != ownerScene.handle;
            if (sceneExitInProgress)
            {
                CancelPlan("The owning Station scene exited while ordered Add participation was active.");
            }
            else
            {
                FailPlan(ticket, $"Active ticket '{ticket.Spawn.SpawnId}' health became inactive outside executor cleanup.");
            }
        }

        private void HandleTerminalEncounterEnded()
        {
            CancelPlan("The authoritative Station encounter reached a terminal outcome.");
        }

        private void HandleTerminalEncounterFaulted(EncounterTerminalDiagnostic diagnostic)
        {
            CancelPlan($"The authoritative Station encounter faulted: {diagnostic.Reason}.");
        }

        private void CompleteTicket(Ticket ticket)
        {
            if (ticket == null || ticket.State != StageAddEncounterTicketState.Active)
            {
                return;
            }

            ticket.State = StageAddEncounterTicketState.Completed;
            ticket.TerminalSequence = ++eventSequence;
            if (!CleanupTicket(ticket, out string cleanupError))
            {
                ticket.State = StageAddEncounterTicketState.Faulted;
                FailPlan(ticket, cleanupError);
                return;
            }

            if (CompletedTicketCount == tickets.Count)
            {
                CompletePlan();
            }
            else
            {
                RefreshPlanState();
            }
        }

        private void CompletePlan()
        {
            if (IsTerminalState())
            {
                return;
            }

            State = StageCountOneEncounterState.Completed;
            LastError = string.Empty;
            CompletionCount++;
            UnsubscribeHealthInactive();
            SealReceipt("All ordered Add tickets completed independently.");
        }

        private void CancelPlan(string reason)
        {
            if (IsTerminalState())
            {
                return;
            }

            bool cleanupFault = false;
            string cleanupError = string.Empty;
            for (int i = 0; i < tickets.Count; i++)
            {
                Ticket ticket = tickets[i];
                if (ticket.State == StageAddEncounterTicketState.Pending
                    || ticket.State == StageAddEncounterTicketState.Active)
                {
                    ticket.State = StageAddEncounterTicketState.Cancelled;
                    ticket.TerminalSequence = ++eventSequence;
                }

                if (!CleanupTicket(ticket, out string ticketCleanupError))
                {
                    ticket.State = StageAddEncounterTicketState.Faulted;
                    cleanupFault = true;
                    cleanupError = AppendError(cleanupError, ticketCleanupError);
                }
            }

            UnsubscribeHealthInactive();
            if (cleanupFault)
            {
                State = StageCountOneEncounterState.Faulted;
                LastError = AppendError(reason, cleanupError);
                FaultCount++;
            }
            else
            {
                State = StageCountOneEncounterState.Cancelled;
                LastError = reason ?? string.Empty;
                CancellationCount++;
            }

            SealReceipt(LastError);
        }

        private void FailPlan(Ticket faultTicket, string error)
        {
            if (IsTerminalState())
            {
                return;
            }

            string combinedError = error ?? string.Empty;
            for (int i = 0; i < tickets.Count; i++)
            {
                Ticket ticket = tickets[i];
                if (ReferenceEquals(ticket, faultTicket)
                    || (faultTicket == null
                        && ticket.State != StageAddEncounterTicketState.Completed
                        && ticket.State != StageAddEncounterTicketState.Cancelled))
                {
                    ticket.State = StageAddEncounterTicketState.Faulted;
                    if (ticket.TerminalSequence <= 0)
                    {
                        ticket.TerminalSequence = ++eventSequence;
                    }
                }
                else if (ticket.State == StageAddEncounterTicketState.Pending
                    || ticket.State == StageAddEncounterTicketState.Active)
                {
                    ticket.State = StageAddEncounterTicketState.Cancelled;
                    ticket.TerminalSequence = ++eventSequence;
                }

                if (!CleanupTicket(ticket, out string cleanupError))
                {
                    ticket.State = StageAddEncounterTicketState.Faulted;
                    combinedError = AppendError(combinedError, cleanupError);
                }
            }

            UnsubscribeHealthInactive();
            State = StageCountOneEncounterState.Faulted;
            LastError = combinedError;
            FaultCount++;
            SealReceipt(LastError);
        }

        private bool CleanupTicket(Ticket ticket, out string error)
        {
            error = string.Empty;
            if (ticket == null)
            {
                return true;
            }

            CombatHealth health = ticket.Health;
            GameObject root = ticket.Root;
            ICombatAiAgent agent = ticket.Agent;
            CombatTargetSensor sensor = ticket.Sensor;
            bool participationRegistered = ticket.ParticipationRegistered;

            if (!ReferenceEquals(health, null) && ticket.DeathHandler != null)
            {
                health.Died -= ticket.DeathHandler;
            }

            try
            {
                if (!ReferenceEquals(sensor, null))
                {
                    sensor.ConfigureTargetCandidates(Array.Empty<CombatHealth>(), refreshNow: false);
                    sensor.enabled = false;
                }
            }
            catch (Exception exception)
            {
                error = AppendError(error, exception.Message);
            }

            try
            {
                MonoBehaviour agentBehaviour = agent as MonoBehaviour;
                if (!ReferenceEquals(agentBehaviour, null))
                {
                    agent.ConfigureTarget(null, null);
                    agentBehaviour.enabled = false;
                }
            }
            catch (Exception exception)
            {
                error = AppendError(error, exception.Message);
            }

            try
            {
                if (!ReferenceEquals(root, null) && root != null)
                {
                    root.SetActive(false);
                }
            }
            catch (Exception exception)
            {
                error = AppendError(error, exception.Message);
            }

            try
            {
                if (participationRegistered
                    && !ReferenceEquals(playerTargetSelector, null)
                    && !ReferenceEquals(health, null))
                {
                    playerTargetSelector.UnregisterRuntimeTargetCandidate(
                        health,
                        refreshNow: false);
                }
            }
            catch (Exception exception)
            {
                error = AppendError(error, exception.Message);
            }

            bool participationReleased = !participationRegistered
                || ReferenceEquals(playerTargetSelector, null)
                || ReferenceEquals(health, null)
                || !playerTargetSelector.ContainsRuntimeTargetCandidate(health);
            bool hierarchyInactive = ReferenceEquals(root, null)
                || root == null
                || !root.activeSelf;
            ticket.ParticipationReleasedAtCleanup = participationReleased;
            ticket.HierarchyInactiveAtCleanup = hierarchyInactive;
            ticket.ParticipationRegistered = !participationReleased;

            if (participationReleased && hierarchyInactive)
            {
                ticket.Health = null;
                ticket.Root = null;
                ticket.Agent = null;
                ticket.Sensor = null;
                ticket.ProjectileDriver = null;
                ticket.ProjectileRoot = null;
                ticket.DeathHandler = null;
                ticket.ParticipationRegistered = false;

                if (!ReferenceEquals(root, null) && root != null)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(root);
                    }
                    else
                    {
                        DestroyImmediate(root);
                    }
                }
            }

            if (!participationReleased || !hierarchyInactive)
            {
                error = AppendError(
                    error,
                    $"Ticket '{ticket.Spawn.SpawnId}' did not reach synchronous quiescence.");
            }

            return string.IsNullOrEmpty(error);
        }

        private void RefreshPlanState()
        {
            if (IsTerminalState())
            {
                return;
            }

            if (CompletedTicketCount == tickets.Count && tickets.Count > 0)
            {
                CompletePlan();
            }
            else if (ActiveTicketCount > 0)
            {
                State = StageCountOneEncounterState.Active;
                LastError = string.Empty;
            }
            else if (PendingTicketCount > 0)
            {
                State = StageCountOneEncounterState.WaitingForActivation;
                LastError = string.Empty;
            }
        }

        private void SealReceipt(string reason)
        {
            var receipts = new StageAddEncounterTicketReceipt[tickets.Count];
            for (int i = 0; i < tickets.Count; i++)
            {
                Ticket ticket = tickets[i];
                receipts[i] = new StageAddEncounterTicketReceipt(
                    ticket.SourceOrdinal,
                    ticket.Spawn.SpawnId,
                    ticket.Spawn.AnchorId,
                    ticket.Spawn.PayloadId,
                    ticket.Spawn.PositionId,
                    ticket.Spawn.AuthoredDelaySeconds,
                    ticket.State,
                    ticket.ActivationSequence,
                    ticket.TerminalSequence,
                    ticket.ParticipationReleasedAtCleanup,
                    ticket.HierarchyInactiveAtCleanup);
            }

            LastReceipt = new StageAddEncounterPlanReceipt(
                State,
                reason,
                ++eventSequence,
                receipts,
                IsQuiescent);
        }

        private void Shutdown()
        {
            if (guideGate != null)
            {
                guideGate.StateChanged -= HandleGuideStateChanged;
            }

            if (terminalEncounter != null)
            {
                terminalEncounter.Won -= HandleTerminalEncounterEnded;
                terminalEncounter.Failed -= HandleTerminalEncounterEnded;
                terminalEncounter.DiagnosticAborted -= HandleTerminalEncounterFaulted;
            }

            if (!IsTerminalState() && runtimeInitialized)
            {
                CancelPlan("The ordered Add executor was disabled before its owned lifetime completed.");
            }
            else
            {
                for (int i = 0; i < tickets.Count; i++)
                {
                    CleanupTicket(tickets[i], out _);
                }

                UnsubscribeHealthInactive();
            }

            ReleaseSceneLease();
        }

        private void UnsubscribeHealthInactive()
        {
            if (!healthInactiveSubscribed)
            {
                return;
            }

            CombatHealth.BecameInactive -= HandleCombatHealthBecameInactive;
            healthInactiveSubscribed = false;
        }

        private int CountTickets(StageAddEncounterTicketState state)
        {
            int count = 0;
            for (int i = 0; i < tickets.Count; i++)
            {
                if (tickets[i].State == state)
                {
                    count++;
                }
            }

            return count;
        }

        private Ticket FindFirstOwnedTicket()
        {
            for (int i = 0; i < tickets.Count; i++)
            {
                if (tickets[i].Root != null)
                {
                    return tickets[i];
                }
            }

            return null;
        }

        private Ticket FindFirstActiveTicket()
        {
            for (int i = 0; i < tickets.Count; i++)
            {
                if (tickets[i].State == StageAddEncounterTicketState.Active)
                {
                    return tickets[i];
                }
            }

            return null;
        }

        private Ticket FindTicketByHealth(CombatHealth health)
        {
            if (ReferenceEquals(health, null))
            {
                return null;
            }

            for (int i = 0; i < tickets.Count; i++)
            {
                if (ReferenceEquals(tickets[i].Health, health))
                {
                    return tickets[i];
                }
            }

            return null;
        }

        private bool IsTerminalState()
        {
            return State == StageCountOneEncounterState.Completed
                || State == StageCountOneEncounterState.Cancelled
                || State == StageCountOneEncounterState.Faulted;
        }

        private static string AppendError(string current, string next)
        {
            if (string.IsNullOrWhiteSpace(next))
            {
                return current ?? string.Empty;
            }

            return string.IsNullOrWhiteSpace(current)
                ? next
                : current + " | " + next;
        }

        private static bool Approximately(Vector3 left, Vector3 right, float tolerance)
        {
            return Mathf.Abs(left.x - right.x) <= tolerance
                && Mathf.Abs(left.y - right.y) <= tolerance
                && Mathf.Abs(left.z - right.z) <= tolerance;
        }

        private static bool IsFinite(Vector3 value)
        {
            return float.IsFinite(value.x)
                && float.IsFinite(value.y)
                && float.IsFinite(value.z);
        }

        private static bool IsFinite(Quaternion value)
        {
            return float.IsFinite(value.x)
                && float.IsFinite(value.y)
                && float.IsFinite(value.z)
                && float.IsFinite(value.w);
        }

        private sealed class Ticket
        {
            public Ticket(
                int sourceOrdinal,
                StageDefinitionProfile.SpawnRef spawn,
                StageDefinitionProfile.AnchorRef expectedAnchor,
                StageAnchorPoint anchor,
                CombatEnemyArchetypeProfile archetype,
                GameObject prefab)
            {
                SourceOrdinal = sourceOrdinal;
                Spawn = spawn;
                ExpectedAnchor = expectedAnchor;
                Anchor = anchor;
                Archetype = archetype;
                Prefab = prefab;
                State = StageAddEncounterTicketState.Pending;
                SpawnRotation = Quaternion.identity;
            }

            public int SourceOrdinal { get; }
            public StageDefinitionProfile.SpawnRef Spawn { get; }
            public StageDefinitionProfile.AnchorRef ExpectedAnchor { get; }
            public StageAnchorPoint Anchor { get; }
            public CombatEnemyArchetypeProfile Archetype { get; }
            public GameObject Prefab { get; }
            public StageAddEncounterTicketState State { get; set; }
            public int ActivationSequence { get; set; }
            public int TerminalSequence { get; set; }
            public GameObject Root { get; set; }
            public CombatHealth Health { get; set; }
            public ICombatAiAgent Agent { get; set; }
            public CombatTargetSensor Sensor { get; set; }
            public BasicSoldierProjectileAttackDriver ProjectileDriver { get; set; }
            public Transform ProjectileRoot { get; set; }
            public Action DeathHandler { get; set; }
            public bool ParticipationRegistered { get; set; }
            public bool ParticipationReleasedAtCleanup { get; set; } = true;
            public bool HierarchyInactiveAtCleanup { get; set; } = true;
            public Vector3 SpawnPosition { get; set; }
            public Quaternion SpawnRotation { get; set; }
        }
    }
}
