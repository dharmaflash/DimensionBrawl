using System;
using UnityEngine;
using UnityEngine.Events;

namespace DimensionBrawl.Combat
{
    [DefaultExecutionOrder(10000)]
    public sealed class CombatEncounterController : MonoBehaviour
    {
        private enum EncounterState
        {
            Running,
            Won,
            Failed,
            Faulted
        }

        private static long nextRunGeneration;

        [Header("Combatants")]
        [SerializeField] private CombatHealth playerHealth;
        [SerializeField] private CombatHealth enemyHealth;
        [SerializeField] private bool useCoordinatedTerminalResolution;

        [Header("Inspectable Result Markers")]
        [SerializeField] private GameObject winMarker;
        [SerializeField] private GameObject failMarker;
        [SerializeField] private UnityEvent onWon = new UnityEvent();
        [SerializeField] private UnityEvent onFailed = new UnityEvent();

        private EncounterState state;
        private EncounterTerminalResolutionCoordinator terminalCoordinator;
        private bool hasStarted;

        public bool IsRunning => state == EncounterState.Running;
        public bool IsWon => state == EncounterState.Won;
        public bool IsFailed => state == EncounterState.Failed;
        public bool IsFaulted => state == EncounterState.Faulted;
        public CombatHealth PlayerHealth => playerHealth;
        public CombatHealth EnemyHealth => enemyHealth;
        public bool UsesCoordinatedTerminalResolution => useCoordinatedTerminalResolution;
        public long RunGeneration => terminalCoordinator?.RunGeneration ?? 0;
        public EncounterTerminalResolutionCoordinator TerminalCoordinator => terminalCoordinator;
        public bool HasTerminalResolution { get; private set; }
        public EncounterTerminalResolution TerminalResolution { get; private set; }
        public bool HasDiagnostic { get; private set; }
        public EncounterTerminalDiagnostic Diagnostic { get; private set; }

        public event Action Won;
        public event Action Failed;
        public event Action<EncounterTerminalResolution> TerminalResolved;
        public event Action<EncounterTerminalDiagnostic> DiagnosticAborted;
        public static event Action<CombatEncounterController> CoordinatedRunStarted;
        public static event Action<CombatEncounterController> CoordinatedRunStopping;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetRunGeneration()
        {
            nextRunGeneration = 0;
            CoordinatedRunStarted = null;
            CoordinatedRunStopping = null;
        }

        public void ConfigureCombatants(CombatHealth newPlayerHealth, CombatHealth newEnemyHealth)
        {
            if (HasLiveTerminalCoordinator())
            {
                if (playerHealth == newPlayerHealth && enemyHealth == newEnemyHealth)
                {
                    return;
                }

                terminalCoordinator.ReportSubjectRebindAttempt();
                SetMarkers();
                return;
            }

            StopRunBindings();
            playerHealth = newPlayerHealth;
            enemyHealth = newEnemyHealth;

            if (isActiveAndEnabled)
            {
                BeginRun();
            }

            SetMarkers();
        }

        private bool HasLiveTerminalCoordinator()
        {
            if (terminalCoordinator == null)
            {
                return false;
            }

            EncounterTerminalCoordinatorState coordinatorState = terminalCoordinator.State;
            return coordinatorState != EncounterTerminalCoordinatorState.Unbound
                && coordinatorState != EncounterTerminalCoordinatorState.TerminalClosed
                && coordinatorState != EncounterTerminalCoordinatorState.Faulted
                && coordinatorState != EncounterTerminalCoordinatorState.Cancelled;
        }

        public void ConfigureTerminalResolutionPolicy(bool useCoordinator)
        {
            if (useCoordinatedTerminalResolution == useCoordinator)
            {
                return;
            }

            StopRunBindings();
            useCoordinatedTerminalResolution = useCoordinator;
            if (isActiveAndEnabled)
            {
                BeginRun();
                SetMarkers();
            }
        }

        public CombatRootAdmissionResult AdmitCombatRoot(
            Action<CanonicalCombatRootContext> producer)
        {
            return AdmitCombatRoot("combat.external-root", producer);
        }

        public CombatRootAdmissionResult AdmitCombatRoot(
            string causeIdentity,
            Action<CanonicalCombatRootContext> producer)
        {
            if (terminalCoordinator == null)
            {
                return new CombatRootAdmissionResult(
                    CombatRootAdmissionDisposition.Rejected,
                    0,
                    EncounterTerminalCoordinatorState.Unbound);
            }

            return CanonicalCombatRootAdmission.Run(
                terminalCoordinator,
                causeIdentity,
                producer);
        }

        private void OnEnable()
        {
            if (useCoordinatedTerminalResolution && !hasStarted)
            {
                ResetRunState();
            }
            else
            {
                BeginRun();
            }

            SetMarkers();
        }

        private void Start()
        {
            hasStarted = true;
            if (useCoordinatedTerminalResolution
                && terminalCoordinator == null
                && isActiveAndEnabled)
            {
                BeginRun();
                SetMarkers();
            }
        }

        private void OnDisable()
        {
            StopRunBindings();
        }

        private void BeginRun()
        {
            StopRunBindings();
            ResetRunState();

            if (playerHealth == null || enemyHealth == null)
            {
                return;
            }

            if (!useCoordinatedTerminalResolution)
            {
                SubscribeLegacyHealthEvents();
                return;
            }

            long runGeneration = ++nextRunGeneration;
            if (!EncounterTerminalResolutionCoordinator.TryCreate(
                runGeneration,
                playerHealth,
                enemyHealth,
                out terminalCoordinator,
                out EncounterTerminalDiagnostic diagnostic))
            {
                HandleTerminalDiagnostic(diagnostic);
                return;
            }

            terminalCoordinator.Resolved += HandleTerminalResolved;
            terminalCoordinator.DiagnosticAborted += HandleTerminalDiagnostic;
            CoordinatedRunStarted?.Invoke(this);
        }

        private void ResetRunState()
        {
            state = EncounterState.Running;
            HasTerminalResolution = false;
            TerminalResolution = default;
            HasDiagnostic = false;
            Diagnostic = default;
        }

        private void StopRunBindings()
        {
            UnsubscribeLegacyHealthEvents();
            DisposeTerminalCoordinator();
        }

        private void SubscribeLegacyHealthEvents()
        {
            playerHealth.Died -= HandleLegacyPlayerDied;
            enemyHealth.Died -= HandleLegacyEnemyDied;
            playerHealth.Died += HandleLegacyPlayerDied;
            enemyHealth.Died += HandleLegacyEnemyDied;
        }

        private void UnsubscribeLegacyHealthEvents()
        {
            if (playerHealth != null)
            {
                playerHealth.Died -= HandleLegacyPlayerDied;
            }

            if (enemyHealth != null)
            {
                enemyHealth.Died -= HandleLegacyEnemyDied;
            }
        }

        private void DisposeTerminalCoordinator()
        {
            if (terminalCoordinator == null)
            {
                return;
            }

            CoordinatedRunStopping?.Invoke(this);
            terminalCoordinator.Resolved -= HandleTerminalResolved;
            terminalCoordinator.DiagnosticAborted -= HandleTerminalDiagnostic;
            terminalCoordinator.Dispose();
            terminalCoordinator = null;
        }

        private void HandleLegacyEnemyDied()
        {
            PublishLegacyOutcome(won: true);
        }

        private void HandleLegacyPlayerDied()
        {
            PublishLegacyOutcome(won: false);
        }

        private void PublishLegacyOutcome(bool won)
        {
            if (state != EncounterState.Running)
            {
                return;
            }

            state = won ? EncounterState.Won : EncounterState.Failed;
            SetMarkers();
            if (won)
            {
                Won?.Invoke();
                onWon.Invoke();
                return;
            }

            Failed?.Invoke();
            onFailed.Invoke();
        }

        private void HandleTerminalResolved(EncounterTerminalResolution resolution)
        {
            if (state != EncounterState.Running)
            {
                return;
            }

            TerminalResolution = resolution;
            HasTerminalResolution = true;
            state = resolution.Outcome == EncounterTerminalOutcome.Clear
                ? EncounterState.Won
                : EncounterState.Failed;
            SetMarkers();
            TerminalResolved?.Invoke(resolution);

            if (state == EncounterState.Won)
            {
                Won?.Invoke();
                onWon.Invoke();
                return;
            }

            Failed?.Invoke();
            onFailed.Invoke();
        }

        private void HandleTerminalDiagnostic(EncounterTerminalDiagnostic diagnostic)
        {
            if (state != EncounterState.Running)
            {
                return;
            }

            Diagnostic = diagnostic;
            HasDiagnostic = true;
            state = EncounterState.Faulted;
            SetMarkers();
            DiagnosticAborted?.Invoke(diagnostic);
        }

        private void SetMarkers()
        {
            if (winMarker != null)
            {
                winMarker.SetActive(state == EncounterState.Won);
            }

            if (failMarker != null)
            {
                failMarker.SetActive(state == EncounterState.Failed);
            }
        }
    }
}
