using DimensionBrawl.Combat;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DimensionBrawl.LevelDesign
{
    [DefaultExecutionOrder(-10000)]
    [DisallowMultipleComponent]
    public sealed class OneRowStageRunBootstrap : MonoBehaviour
    {
        [SerializeField] private PlayableStageDefinition playableStageDefinition;
        [SerializeField] private CombatEncounterController encounter;
        [SerializeField] private OneRowStageRunFactAdapter factAdapter;
        [SerializeField] private OneRowStageRunResultPresenter resultPresenter;

        private bool hasStarted;

        public bool HasAdmittedRun { get; private set; }
        public StageRunContext AdmittedContext { get; private set; }
        public string LastAdmissionError { get; private set; } = string.Empty;

        private void Start()
        {
            if (hasStarted)
            {
                return;
            }

            hasStarted = true;
            TryAdmitOneRowRun();
        }

        public bool TryAdmitOneRowRun()
        {
            HasAdmittedRun = false;
            AdmittedContext = null;
            if (encounter == null
                || encounter.gameObject.scene.handle != gameObject.scene.handle
                || SceneManager.GetActiveScene().handle != gameObject.scene.handle
                || !encounter.UsesCoordinatedTerminalResolution)
            {
                return RejectBeforeAdmission(
                    "A one-row stage-run bootstrap requires one active-scene coordinated encounter.");
            }

            if (!TryValidateEncounterSubjects(out string readinessError))
            {
                return RejectBeforeAdmission(readinessError);
            }

            if (factAdapter == null || !factAdapter.isActiveAndEnabled)
            {
                return RejectBeforeAdmission(
                    "A one-row stage scene requires one live terminal fact adapter.");
            }

            if (!factAdapter.TryValidateAuthoring(encounter, out readinessError))
            {
                return RejectBeforeAdmission(readinessError);
            }

            if (resultPresenter == null || !resultPresenter.isActiveAndEnabled)
            {
                return RejectBeforeAdmission(
                    "A one-row stage scene requires one live terminal result presenter.");
            }

            if (!resultPresenter.TryValidateAuthoring(
                encounter,
                factAdapter,
                out readinessError))
            {
                return RejectBeforeAdmission(readinessError);
            }

            if (!HasExactlyOneLiveCoordinatedEncounter())
            {
                return RejectBeforeAdmission(
                    "A one-row stage scene must contain exactly one live coordinated encounter.");
            }

            if (!OneRowStageRunAdapterContract.TryValidateDefinition(
                playableStageDefinition,
                out StageRunRouteSnapshot expectedSnapshot,
                out string error))
            {
                return RejectBeforeAdmission(error);
            }

            if (!StageRunRuntime.TryAdmitFirstSegment(
                playableStageDefinition,
                gameObject.scene,
                out StageRunContext context,
                out error))
            {
                return RejectBeforeAdmission(error);
            }

            if (!OneRowStageRunAdapterContract.TryValidateContext(
                    context,
                    gameObject.scene.handle,
                    out error)
                || context.LifecycleState != StageRunLifecycleState.StationActive
                || !context.IsCurrentSegmentTerminalActive
                || !context.TryBindTerminalAdmissionEncounter(encounter, out error)
                || !context.TryBindTerminalStageAdapter(
                    this,
                    TerminalStageRunAdapterRole.EntryBootstrap,
                    out error)
                || !context.TryBindTerminalStageAdapter(
                    factAdapter,
                    TerminalStageRunAdapterRole.FactCollection,
                    out error)
                || !context.TryBindTerminalStageAdapter(
                    resultPresenter,
                    TerminalStageRunAdapterRole.ResultPresentation,
                    out error)
                || !string.Equals(
                    context.RouteSnapshot.CanonicalDigest,
                    expectedSnapshot.CanonicalDigest,
                    System.StringComparison.Ordinal))
            {
                LastAdmissionError = string.IsNullOrWhiteSpace(error)
                    ? "One-row stage-run admission did not activate the exact authored route."
                    : error;
                return false;
            }

            if (!factAdapter.BindToActiveRun())
            {
                return RejectAfterAdmission(
                    factAdapter,
                    StageRunAbortReason.TerminalFactAdapterLost,
                    factAdapter.LastFactError);
            }

            if (!resultPresenter.BindToActiveRun())
            {
                return RejectAfterAdmission(
                    resultPresenter,
                    StageRunAbortReason.TerminalResultPresenterLost,
                    resultPresenter.CanonicalStageRunBindingError);
            }

            HasAdmittedRun = true;
            AdmittedContext = context;
            LastAdmissionError = string.Empty;
            return true;
        }

        private bool RejectBeforeAdmission(string error)
        {
            LastAdmissionError = string.IsNullOrWhiteSpace(error)
                ? "One-row stage-run admission readiness failed."
                : error;
            StageRunContext context = StageRunRuntime.ActiveContext;
            bool exactExistingEncounter = context != null
                && context.CurrentSceneHandle == gameObject.scene.handle
                && context.IsTerminalAdmissionEncounter(encounter);
            if (encounter != null && !exactExistingEncounter)
            {
                encounter.enabled = false;
            }

            return false;
        }

        private bool RejectAfterAdmission(
            Component adapter,
            StageRunAbortReason reason,
            string error)
        {
            LastAdmissionError = string.IsNullOrWhiteSpace(error)
                ? "A one-row stage adapter could not bind after admission."
                : error;
            if (!StageRunRuntime.TryAbortFromTerminalAdapterLoss(
                    adapter,
                    encounter,
                    reason,
                    out _,
                    out _)
                && encounter != null)
            {
                encounter.enabled = false;
            }

            return false;
        }

        private bool TryValidateEncounterSubjects(out string error)
        {
            error = string.Empty;
            CombatHealth playerHealth = encounter.PlayerHealth;
            CombatHealth enemyHealth = encounter.EnemyHealth;
            if (playerHealth == null
                || enemyHealth == null
                || ReferenceEquals(playerHealth, enemyHealth)
                || playerHealth.gameObject.scene.handle != gameObject.scene.handle
                || enemyHealth.gameObject.scene.handle != gameObject.scene.handle
                || !playerHealth.isActiveAndEnabled
                || !enemyHealth.isActiveAndEnabled
                || !playerHealth.IsAlive
                || !enemyHealth.IsAlive
                || playerHealth.IsTerminalMutationBound
                || enemyHealth.IsTerminalMutationBound)
            {
                error =
                    "A one-row bootstrap requires distinct, live, unbound same-scene terminal subjects.";
                return false;
            }

            return true;
        }

        private bool HasExactlyOneLiveCoordinatedEncounter()
        {
            int count = 0;
            GameObject[] roots = gameObject.scene.GetRootGameObjects();
            for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
            {
                CombatEncounterController[] encounters =
                    roots[rootIndex].GetComponentsInChildren<CombatEncounterController>(true);
                for (int encounterIndex = 0; encounterIndex < encounters.Length; encounterIndex++)
                {
                    CombatEncounterController candidate = encounters[encounterIndex];
                    if (candidate == null
                        || !candidate.isActiveAndEnabled
                        || !candidate.UsesCoordinatedTerminalResolution)
                    {
                        continue;
                    }

                    count++;
                    if (!ReferenceEquals(candidate, encounter))
                    {
                        return false;
                    }
                }
            }

            return count == 1;
        }
    }
}
