using System;
using System.Collections.Generic;
using DimensionBrawl.AI;
using DimensionBrawl.Combat;
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

    [DefaultExecutionOrder(9500)]
    [DisallowMultipleComponent]
    public sealed class StageCountOneEncounterExecutor : MonoBehaviour
    {
        private const float PositionTolerance = 0.0001f;
        private const float RotationToleranceDegrees = 0.001f;

        private static readonly Dictionary<int, StageCountOneEncounterExecutor> SceneOwners = new();

        [SerializeField] private StageDefinitionSceneBinding sceneBinding;
        [SerializeField] private string spawnId = "add-left";
        [SerializeField] private CombatEnemyArchetypeProfile[] payloadMappings =
            Array.Empty<CombatEnemyArchetypeProfile>();
        [SerializeField] private StageEncounterActivationKind activationKind =
            StageEncounterActivationKind.CombatEntryGuideReleased;
        [SerializeField] private bool requireActiveStageRun = true;
        [SerializeField] private bool cancelOnTerminalEncounter = true;

        private StageDefinitionProfile.SpawnRef configuredSpawn;
        private StageAnchorPoint configuredAnchor;
        private GameObject configuredPrefab;
        private ICombatEntryGuideGate guideGate;
        private CombatEncounterController terminalEncounter;
        private GameObject ownedRoot;
        private CombatHealth ownedHealth;
        private bool runtimeInitialized;
        private bool activationDelayArmed;
        private float activationReadyTime;

        public StageDefinitionSceneBinding SceneBinding => sceneBinding;
        public string SpawnId => spawnId;
        public StageEncounterActivationKind ActivationKind => activationKind;
        public bool RequiresActiveStageRun => requireActiveStageRun;
        public bool CancelsOnTerminalEncounter => cancelOnTerminalEncounter;
        public int PayloadMappingCount => payloadMappings != null ? payloadMappings.Length : 0;
        public StageCountOneEncounterState State { get; private set; }
        public string LastError { get; private set; } = string.Empty;
        public CombatHealth OwnedHealth => ownedHealth;
        public int OwnedObjectCount => ownedRoot != null ? 1 : 0;
        public bool HasSceneLease { get; private set; }
        public int ActivationCount { get; private set; }
        public int CompletionCount { get; private set; }
        public int CancellationCount { get; private set; }
        public Vector3 LastSpawnPosition { get; private set; }
        public Quaternion LastSpawnRotation { get; private set; } = Quaternion.identity;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSceneOwners()
        {
            SceneOwners.Clear();
        }

        public CombatEnemyArchetypeProfile GetPayloadMapping(int index)
        {
            if (payloadMappings == null || index < 0 || index >= payloadMappings.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return payloadMappings[index];
        }

        public bool TryActivate(out string error)
        {
            error = string.Empty;
            if (State == StageCountOneEncounterState.Active)
            {
                return true;
            }

            if (IsTerminalState() || ActivationCount > 0)
            {
                error = "The count-one encounter has already reached its one-shot terminal state.";
                return false;
            }

            if (!TryInitializeRuntime(out error))
            {
                Fail(error);
                return false;
            }

            if (!TryValidateActiveRun(out error))
            {
                return false;
            }

            if (activationKind == StageEncounterActivationKind.CombatEntryGuideReleased
                && guideGate.State != CombatEntryGuideState.Released)
            {
                error = "The combat-entry guide has not released gameplay.";
                return false;
            }

            if (activationDelayArmed && Time.time < activationReadyTime)
            {
                error = "The authored spawn delay has not elapsed.";
                return false;
            }

            if (terminalEncounter != null
                && (terminalEncounter.IsWon || terminalEncounter.IsFailed || terminalEncounter.IsFaulted))
            {
                error = "The authoritative encounter is already terminal.";
                Cancel(error);
                return false;
            }

            if (!TryAcquireSceneLease(out error))
            {
                Fail(error);
                return false;
            }

            try
            {
                GameObject root = new GameObject($"[Runtime] Stage Encounter {spawnId}");
                root.SetActive(false);
                SceneManager.MoveGameObjectToScene(root, gameObject.scene);
                root.transform.SetParent(sceneBinding.transform, true);
                ownedRoot = root;

                LastSpawnPosition = configuredAnchor.transform.position;
                LastSpawnRotation = configuredAnchor.transform.rotation;
                GameObject instance = Instantiate(
                    configuredPrefab,
                    LastSpawnPosition,
                    LastSpawnRotation,
                    root.transform);
                CombatHealth[] healthComponents = instance.GetComponentsInChildren<CombatHealth>(true);
                if (healthComponents.Length != 1 || healthComponents[0].Team != DamageTeam.Enemy)
                {
                    error = "The instantiated payload must contain exactly one Enemy CombatHealth.";
                    Fail(error);
                    return false;
                }

                ownedHealth = healthComponents[0];
                ownedHealth.Died += HandleOwnedHealthDied;
                State = StageCountOneEncounterState.Active;
                LastError = string.Empty;
                ActivationCount++;
                root.SetActive(true);
                return true;
            }
            catch (Exception exception)
            {
                error = $"Count-one payload activation failed: {exception.GetType().Name}: {exception.Message}";
                Fail(error);
                return false;
            }
        }

        private void Start()
        {
            EvaluateActivation();
        }

        private void Update()
        {
            if (State == StageCountOneEncounterState.Active)
            {
                if (ownedRoot == null || ownedHealth == null)
                {
                    Fail("The active count-one payload was destroyed outside its executor.");
                    return;
                }

                if (!ownedRoot.activeInHierarchy || !ownedHealth.isActiveAndEnabled)
                {
                    Fail("The active count-one payload was disabled outside its executor.");
                    return;
                }

                if (!ownedHealth.IsAlive)
                {
                    Complete();
                    return;
                }

                if (!TryValidateActiveRun(out string runError))
                {
                    Cancel(runError);
                }

                return;
            }

            if (!IsTerminalState())
            {
                EvaluateActivation();
            }
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
            if (IsTerminalState() || State == StageCountOneEncounterState.Active)
            {
                return;
            }

            if (!TryInitializeRuntime(out string initializationError))
            {
                Fail(initializationError);
                return;
            }

            if (terminalEncounter != null
                && (terminalEncounter.IsWon || terminalEncounter.IsFailed || terminalEncounter.IsFaulted))
            {
                Cancel("The authoritative encounter ended before the count-one payload activated.");
                return;
            }

            if (!TryValidateActiveRun(out string runError))
            {
                State = StageCountOneEncounterState.WaitingForRun;
                LastError = runError;
                return;
            }

            if (activationKind == StageEncounterActivationKind.CombatEntryGuideReleased)
            {
                if (guideGate.State == CombatEntryGuideState.Interrupted)
                {
                    Cancel("The combat-entry guide was interrupted before releasing gameplay.");
                    return;
                }

                if (guideGate.State != CombatEntryGuideState.Released)
                {
                    State = StageCountOneEncounterState.WaitingForActivation;
                    LastError = string.Empty;
                    return;
                }
            }

            if (!activationDelayArmed)
            {
                activationDelayArmed = true;
                activationReadyTime = Time.time + configuredSpawn.DelaySeconds;
            }

            if (Time.time < activationReadyTime)
            {
                State = StageCountOneEncounterState.WaitingForActivation;
                LastError = string.Empty;
                return;
            }

            TryActivate(out _);
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
                || !TryResolveTerminalEncounter(out error))
            {
                return false;
            }

            if (guideGate != null)
            {
                guideGate.StateChanged -= HandleGuideStateChanged;
                guideGate.StateChanged += HandleGuideStateChanged;
            }

            if (terminalEncounter != null)
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
                error = "The count-one executor does not belong to a loaded scene.";
                return false;
            }

            if (sceneBinding == null || sceneBinding.gameObject.scene != scene)
            {
                error = "The count-one executor requires a scene-local StageDefinitionSceneBinding.";
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

            int spawnMatchCount = 0;
            for (int i = 0; i < definition.SpawnCount; i++)
            {
                StageDefinitionProfile.SpawnRef candidate = definition.GetSpawn(i);
                if (string.Equals(candidate.SpawnId, spawnId, StringComparison.Ordinal))
                {
                    configuredSpawn = candidate;
                    spawnMatchCount++;
                }
            }

            if (spawnMatchCount != 1
                || configuredSpawn.SpawnKind != StageSpawnKind.Add
                || configuredSpawn.Count != 1
                || configuredSpawn.PositionId <= 0
                || string.IsNullOrWhiteSpace(configuredSpawn.AnchorId)
                || string.IsNullOrWhiteSpace(configuredSpawn.PayloadId)
                || float.IsNaN(configuredSpawn.DelaySeconds)
                || float.IsInfinity(configuredSpawn.DelaySeconds))
            {
                error = "The configured spawn must resolve to one valid count-one Add row.";
                return false;
            }

            int expectedAnchorMatchCount = 0;
            StageDefinitionProfile.AnchorRef expectedAnchor = default;
            for (int i = 0; i < definition.AnchorCount; i++)
            {
                StageDefinitionProfile.AnchorRef candidate = definition.GetAnchor(i);
                if (string.Equals(candidate.AnchorId, configuredSpawn.AnchorId, StringComparison.Ordinal))
                {
                    expectedAnchor = candidate;
                    expectedAnchorMatchCount++;
                }
            }

            int liveAnchorMatchCount = 0;
            for (int i = 0; i < sceneBinding.AnchorPointCount; i++)
            {
                StageAnchorPoint candidate = sceneBinding.GetAnchorPoint(i);
                if (candidate != null
                    && string.Equals(candidate.AnchorId, configuredSpawn.AnchorId, StringComparison.Ordinal))
                {
                    configuredAnchor = candidate;
                    liveAnchorMatchCount++;
                }
            }

            if (expectedAnchorMatchCount != 1
                || liveAnchorMatchCount != 1
                || configuredAnchor.gameObject.scene != scene
                || !configuredAnchor.transform.IsChildOf(sceneBinding.transform)
                || configuredAnchor.UsageKind != StageAnchorUsageKind.CombatSpawn
                || configuredAnchor.SpawnKind != StageSpawnKind.Add
                || configuredAnchor.PositionId != configuredSpawn.PositionId
                || !string.Equals(configuredAnchor.GroupId, expectedAnchor.GroupId, StringComparison.Ordinal))
            {
                error = "The count-one Add row does not resolve to one matching live spawn anchor.";
                return false;
            }

            Vector3 localPosition = sceneBinding.transform.InverseTransformPoint(
                configuredAnchor.transform.position);
            Quaternion localRotation = Quaternion.Inverse(sceneBinding.transform.rotation)
                * configuredAnchor.transform.rotation;
            if (!Approximately(localPosition, expectedAnchor.ExpectedPosition, PositionTolerance)
                || Quaternion.Angle(localRotation, Quaternion.Euler(expectedAnchor.ExpectedEuler))
                    > RotationToleranceDegrees)
            {
                error = "The live Add anchor does not match its binding-root-relative authored pose.";
                return false;
            }

            int payloadMatchCount = 0;
            CombatEnemyArchetypeProfile payload = null;
            if (payloadMappings != null)
            {
                for (int i = 0; i < payloadMappings.Length; i++)
                {
                    CombatEnemyArchetypeProfile candidate = payloadMappings[i];
                    if (candidate != null
                        && string.Equals(
                            candidate.ArchetypeId,
                            configuredSpawn.PayloadId,
                            StringComparison.Ordinal))
                    {
                        payload = candidate;
                        payloadMatchCount++;
                    }
                }
            }

            configuredPrefab = payload != null ? payload.GameplayPrefab : null;
            CombatHealth[] prefabHealth = configuredPrefab != null
                ? configuredPrefab.GetComponentsInChildren<CombatHealth>(true)
                : Array.Empty<CombatHealth>();
            if (payloadMatchCount != 1
                || configuredPrefab == null
                || prefabHealth.Length != 1
                || prefabHealth[0].Team != DamageTeam.Enemy)
            {
                error = "The Add payload must map to one prefab with exactly one Enemy CombatHealth.";
                return false;
            }

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
            if (!cancelOnTerminalEncounter)
            {
                terminalEncounter = null;
                return true;
            }

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
                error = "Terminal cancellation requires exactly one scene-local CombatEncounterController.";
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
            StageDefinitionProfile definition = sceneBinding != null ? sceneBinding.StageDefinition : null;
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
                    error = "Another count-one executor already owns this loaded scene.";
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
                Cancel("The combat-entry guide was interrupted before encounter activation.");
                return;
            }

            if (guideState == CombatEntryGuideState.Released)
            {
                EvaluateActivation();
            }
        }

        private void HandleOwnedHealthDied()
        {
            if (State == StageCountOneEncounterState.Active)
            {
                Complete();
            }
        }

        private void HandleTerminalEncounterEnded()
        {
            Cancel("The authoritative Station encounter reached a terminal outcome.");
        }

        private void HandleTerminalEncounterFaulted(EncounterTerminalDiagnostic diagnostic)
        {
            Cancel($"The authoritative Station encounter faulted: {diagnostic.Reason}.");
        }

        private void Complete()
        {
            if (State != StageCountOneEncounterState.Active)
            {
                return;
            }

            State = StageCountOneEncounterState.Completed;
            LastError = string.Empty;
            CompletionCount++;
            CleanupOwnedObject();
        }

        private void Cancel(string reason)
        {
            if (IsTerminalState())
            {
                return;
            }

            State = StageCountOneEncounterState.Cancelled;
            LastError = reason ?? string.Empty;
            CancellationCount++;
            CleanupOwnedObject();
        }

        private void Fail(string error)
        {
            if (IsTerminalState())
            {
                return;
            }

            State = StageCountOneEncounterState.Faulted;
            LastError = error ?? string.Empty;
            CleanupOwnedObject();
        }

        private void CleanupOwnedObject()
        {
            CombatHealth health = ownedHealth;
            GameObject root = ownedRoot;
            ownedHealth = null;
            ownedRoot = null;

            if (health != null)
            {
                health.Died -= HandleOwnedHealthDied;
            }

            if (root == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(root);
            }
            else
            {
                DestroyImmediate(root);
            }
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
                Cancel("The count-one executor was disabled before its owned lifetime completed.");
            }
            else
            {
                CleanupOwnedObject();
            }

            ReleaseSceneLease();
        }

        private bool IsTerminalState()
        {
            return State == StageCountOneEncounterState.Completed
                || State == StageCountOneEncounterState.Cancelled
                || State == StageCountOneEncounterState.Faulted;
        }

        private static bool Approximately(Vector3 left, Vector3 right, float tolerance)
        {
            return Mathf.Abs(left.x - right.x) <= tolerance
                && Mathf.Abs(left.y - right.y) <= tolerance
                && Mathf.Abs(left.z - right.z) <= tolerance;
        }
    }
}
