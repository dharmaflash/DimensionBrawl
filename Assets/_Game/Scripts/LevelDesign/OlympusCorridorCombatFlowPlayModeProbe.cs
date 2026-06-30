using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using DimensionBrawl.Combat;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

namespace DimensionBrawl.LevelDesign
{
    [DisallowMultipleComponent]
    public sealed class OlympusCorridorCombatFlowPlayModeProbe : MonoBehaviour
    {
        [SerializeField] private string resultPath =
            "C:/tmp/DimensionBrawl-OlympusCombatFlow-PlayMode.result";
        [SerializeField] private string reportPath =
            "C:/tmp/DimensionBrawl-OlympusCombatFlow-PlayMode.txt";
        [SerializeField, Min(1f)] private float routeTimeoutSeconds = 45f;

        private const string StageClearExitAnchorName = "StageClear_CorridorExit";
        private const float InputRouteTolerance = 0.8f;
        private const float InputRouteMinimumProgress = 0.02f;
        private const float InputRouteStallSeconds = 2.25f;
        private const float NearbyColliderRadius = 2.6f;

        public void Configure(string newResultPath, string newReportPath, float newRouteTimeoutSeconds)
        {
            resultPath = string.IsNullOrWhiteSpace(newResultPath) ? resultPath : newResultPath;
            reportPath = string.IsNullOrWhiteSpace(newReportPath) ? reportPath : newReportPath;
            routeTimeoutSeconds = Mathf.Max(1f, newRouteTimeoutSeconds);
        }

        private void Start()
        {
            StartCoroutine(VerifyRoutine());
        }

        private IEnumerator VerifyRoutine()
        {
            var report = new StringBuilder();
            var result = new ProbeResult();
            float deadline = Time.realtimeSinceStartup + routeTimeoutSeconds;
            report.AppendLine("Olympus corridor combat flow Play Mode verification");
            report.AppendLine($"Scene={SceneManager.GetActiveScene().path}");

            yield return null;

            OlympusCorridorCombatFlowController flow =
                FindFirst<OlympusCorridorCombatFlowController>();
            if (flow == null)
            {
                Finish(false, report, "Missing OlympusCorridorCombatFlowController.");
                yield break;
            }

            PlayableDirector director = GetField<PlayableDirector>(flow, "introDirector");
            Player.PlayerMovementController player = GetField<Player.PlayerMovementController>(flow, "player");
            Transform stairTriggerCenter = GetField<Transform>(flow, "stairTriggerCenter");
            CombatHealth[] introEnemies = GetField<CombatHealth[]>(flow, "introSwordEnemies");
            CombatHealth[] corridorTargets = GetField<CombatHealth[]>(flow, "corridorTargets");
            CombatHealth[] corridorClearTargets = GetField<CombatHealth[]>(flow, "corridorClearTargets");
            GameObject[] corridorBoundsRoots = GetField<GameObject[]>(flow, "corridorBoundsRoots");

            report.AppendLine($"controllerFound=True");
            report.AppendLine($"introEnemies={CountNonNull(introEnemies)}");
            report.AppendLine($"corridorTargets={CountNonNull(corridorTargets)}");
            report.AppendLine($"corridorClearTargets={CountNonNull(corridorClearTargets)}");

            ForceIntroHandoff(director, flow, report);
            yield return WaitFor(
                () => player != null && player.gameObject.activeInHierarchy,
                deadline,
                "player active after intro handoff",
                report,
                result);
            if (result.Failed)
            {
                Finish(false, report, result.FailureReason);
                yield break;
            }

            bool introDamageApplied = ApplyLethalDamageToAll(introEnemies, DamageTeam.Player);
            report.AppendLine($"introDamageApplied={introDamageApplied}");
            yield return WaitFor(
                () => flow.IntroGateCleared,
                deadline,
                "intro gate cleared from CombatHealth.Died events",
                report,
                result);
            if (result.Failed)
            {
                Finish(false, report, result.FailureReason);
                yield break;
            }

            report.AppendLine($"laneConstraintAfterIntroClear={player.LaneConstraintEnabled}");
            AppendMovementState(player, "afterIntroClear", report);

            if (player == null || stairTriggerCenter == null)
            {
                Finish(false, report, "Missing player or stair trigger center.");
                yield break;
            }

            yield return MovePlayerWithInputToPosition(
                player,
                stairTriggerCenter.position,
                deadline,
                "stairInputTraversal",
                report,
                result);
            if (result.Failed)
            {
                Finish(false, report, result.FailureReason);
                yield break;
            }

            yield return WaitFor(
                () => flow.CorridorCombatStarted,
                deadline,
                "corridor combat started from Update trigger check",
                report,
                result);
            if (result.Failed)
            {
                Finish(false, report, result.FailureReason);
                yield break;
            }

            report.AppendLine($"laneConstraintDuringCorridorCombat={player.LaneConstraintEnabled}");
            AppendMovementState(player, "duringCorridorCombat", report);

            int corridorTargetsAliveBeforeClear = CountActiveAlive(corridorTargets);
            int corridorClearTargetsAliveBeforeClear = CountActiveAlive(corridorClearTargets);
            bool clearDamageApplied = ApplyLethalDamageToAll(corridorClearTargets, DamageTeam.Player);
            report.AppendLine($"corridorTargetsAliveBeforeClear={corridorTargetsAliveBeforeClear}");
            report.AppendLine($"corridorClearTargetsAliveBeforeClear={corridorClearTargetsAliveBeforeClear}");
            report.AppendLine($"corridorClearDamageApplied={clearDamageApplied}");

            yield return WaitFor(
                () => flow.StageCleared,
                deadline,
                "stage cleared from corridor clear target Died event",
                report,
                result);
            if (result.Failed)
            {
                Finish(false, report, result.FailureReason);
                yield break;
            }

            yield return null;

            int corridorTargetsAliveAfterClear = CountActiveAlive(corridorTargets);
            int corridorClearTargetsAliveAfterClear = CountActiveAlive(corridorClearTargets);
            bool nonClearCandidateStillAlive =
                CountNonNull(corridorTargets) > CountNonNull(corridorClearTargets)
                && corridorTargetsAliveAfterClear > corridorClearTargetsAliveAfterClear;
            bool boundsInactive = !AnyActiveInHierarchy(corridorBoundsRoots);
            report.AppendLine($"corridorTargetsAliveAfterClear={corridorTargetsAliveAfterClear}");
            report.AppendLine($"corridorClearTargetsAliveAfterClear={corridorClearTargetsAliveAfterClear}");
            report.AppendLine($"nonClearCandidateStillAlive={nonClearCandidateStillAlive}");
            report.AppendLine($"corridorBoundsInactive={boundsInactive}");
            report.AppendLine($"laneConstraintAfterStageClear={player.LaneConstraintEnabled}");
            AppendMovementState(player, "afterStageClear", report);

            yield return MovePlayerWithInputToSceneObject(
                player,
                StageClearExitAnchorName,
                deadline,
                report,
                result);
            if (result.Failed)
            {
                Finish(false, report, result.FailureReason);
                yield break;
            }

            bool passed =
                introDamageApplied
                && flow.IntroGateCleared
                && flow.StageCleared
                && clearDamageApplied
                && corridorClearTargetsAliveAfterClear == 0
                && nonClearCandidateStillAlive
                && boundsInactive;
            Finish(passed, report, passed ? "PASS" : "One or more Play Mode checks failed.");
        }

        private static void ForceIntroHandoff(
            PlayableDirector director,
            OlympusCorridorCombatFlowController flow,
            StringBuilder report)
        {
            if (director == null)
            {
                report.AppendLine("introDirector=<null>");
                return;
            }

            double handoffSeconds = GetField<double>(flow, "introHandoffSeconds");
            double duration = director.duration;
            double targetTime = handoffSeconds > 0d
                ? handoffSeconds + 0.2d
                : (double.IsInfinity(duration) ? 0d : duration);
            director.time = Math.Max(0d, targetTime);
            director.Evaluate();
            report.AppendLine($"introDirectorForcedTime={director.time:0.###}");
        }

        private static IEnumerator WaitFor(
            Func<bool> condition,
            float deadline,
            string label,
            StringBuilder report,
            ProbeResult result)
        {
            while (!condition())
            {
                if (Time.realtimeSinceStartup > deadline)
                {
                    result.Fail($"Timed out waiting for {label}.");
                    report.AppendLine($"{label}=TIMEOUT");
                    yield break;
                }

                yield return null;
            }

            report.AppendLine($"{label}=True");
        }

        private static IEnumerator MovePlayerWithInputToSceneObject(
            Player.PlayerMovementController player,
            string targetObjectName,
            float deadline,
            StringBuilder report,
            ProbeResult result)
        {
            GameObject targetObject = FindSceneObject(targetObjectName);
            if (targetObject == null)
            {
                result.Fail($"Missing {targetObjectName} anchor.");
                yield break;
            }

            yield return MovePlayerWithInputToPosition(
                player,
                targetObject.transform.position,
                deadline,
                $"{targetObjectName}InputTraversal",
                report,
                result);
        }

        private static IEnumerator MovePlayerWithInputToPosition(
            Player.PlayerMovementController player,
            Vector3 target,
            float deadline,
            string label,
            StringBuilder report,
            ProbeResult result)
        {
            if (player == null)
            {
                result.Fail($"Missing player for {label}.");
                yield break;
            }

            Vector3 start = player.transform.position;
            report.AppendLine($"{label}Start={FormatVector3(start)}");
            report.AppendLine($"{label}Target={FormatVector3(target)}");
            report.AppendLine($"{label}LaneConstraint={player.LaneConstraintEnabled}");
            report.AppendLine($"{label}CinematicLocked={player.IsCinematicMoveInputLocked}");
            float minPlanarDistance = float.PositiveInfinity;
            Vector3 bestPosition = start;
            float lastProgressAt = Time.realtimeSinceStartup;
            int frames = 0;
            while (Time.realtimeSinceStartup <= deadline)
            {
                Vector3 current = player.transform.position;
                Vector3 planar = Vector3.ProjectOnPlane(target - current, Vector3.up);
                float distance = planar.magnitude;
                if (distance < minPlanarDistance - InputRouteMinimumProgress)
                {
                    minPlanarDistance = distance;
                    bestPosition = current;
                    lastProgressAt = Time.realtimeSinceStartup;
                }

                if (distance <= InputRouteTolerance)
                {
                    player.ClearScriptedInputOverride();
                    report.AppendLine($"{label}=True");
                    report.AppendLine($"{label}Frames={frames}");
                    report.AppendLine($"{label}Final={FormatVector3(player.transform.position)}");
                    report.AppendLine($"{label}Distance={distance:0.###}");
                    yield break;
                }

                if (Time.realtimeSinceStartup - lastProgressAt > InputRouteStallSeconds)
                {
                    player.ClearScriptedInputOverride();
                    report.AppendLine($"{label}Best={FormatVector3(bestPosition)}");
                    report.AppendLine($"{label}Final={FormatVector3(player.transform.position)}");
                    report.AppendLine($"{label}BestDistance={minPlanarDistance:0.###}");
                    AppendNearbySolidColliders(
                        player.transform.position,
                        player.transform,
                        $"{label}Blocked",
                        report);
                    result.Fail(
                        $"{label} stalled; best planar distance={minPlanarDistance:0.###}.");
                    yield break;
                }

                Vector2 moveInput = BuildMoveInputForWorldDirection(player, planar.normalized);
                player.SetScriptedInputOverride(moveInput, moveInput);
                frames++;
                yield return null;
            }

            player.ClearScriptedInputOverride();
            report.AppendLine($"{label}Best={FormatVector3(bestPosition)}");
            report.AppendLine($"{label}Final={FormatVector3(player.transform.position)}");
            report.AppendLine($"{label}BestDistance={minPlanarDistance:0.###}");
            AppendNearbySolidColliders(
                player.transform.position,
                player.transform,
                $"{label}TimedOut",
                report);
            result.Fail(
                $"Timed out during {label}; best planar distance={minPlanarDistance:0.###}.");
        }

        private static Vector2 BuildMoveInputForWorldDirection(
            Player.PlayerMovementController player,
            Vector3 worldDirection)
        {
            Vector3 direction = Vector3.ProjectOnPlane(worldDirection, Vector3.up);
            if (direction.sqrMagnitude <= 0.0001f)
            {
                return Vector2.zero;
            }

            direction.Normalize();
            bool cameraRelative = GetField<bool>(player, "cameraRelativeMovement");
            Camera referenceCamera = GetField<Camera>(player, "referenceCamera");
            Vector3 forward = Vector3.forward;
            Vector3 right = Vector3.right;
            if (cameraRelative && referenceCamera != null)
            {
                Transform cameraTransform = referenceCamera.transform;
                forward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
                right = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;
            }

            return Vector2.ClampMagnitude(
                new Vector2(
                    Vector3.Dot(direction, right),
                    Vector3.Dot(direction, forward)),
                1f);
        }

        private static bool ApplyLethalDamageToAll(CombatHealth[] healths, DamageTeam sourceTeam)
        {
            if (healths == null || healths.Length == 0)
            {
                return false;
            }

            bool applied = true;
            for (int i = 0; i < healths.Length; i++)
            {
                CombatHealth health = healths[i];
                if (health == null)
                {
                    applied = false;
                    continue;
                }

                health.ResetHealthToFull();
                applied &= health.TryApplyDamage(new DamageInfo(
                    null,
                    sourceTeam,
                    health.MaxHealth + 1000f,
                    health.transform.position,
                    Vector3.forward,
                    0f));
            }

            return applied;
        }

        private static void SetPlayerPosition(Player.PlayerMovementController player, Vector3 position)
        {
            CharacterController controller =
                player != null ? player.GetComponent<CharacterController>() : null;
            bool wasEnabled = controller != null && controller.enabled;
            if (controller != null)
            {
                controller.enabled = false;
            }

            if (player != null)
            {
                player.transform.position = position;
            }

            if (controller != null)
            {
                controller.enabled = wasEnabled;
            }

            Physics.SyncTransforms();
        }

        private static T GetField<T>(object target, string fieldName)
        {
            if (target == null)
            {
                return default;
            }

            FieldInfo field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field == null)
            {
                return default;
            }

            object value = field.GetValue(target);
            return value is T typed ? typed : default;
        }

        private static T FindFirst<T>() where T : UnityEngine.Object
        {
            T[] objects = UnityEngine.Object.FindObjectsByType<T>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            return objects.Length > 0 ? objects[0] : null;
        }

        private static GameObject FindSceneObject(string objectName)
        {
            Scene scene = SceneManager.GetActiveScene();
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                GameObject found = FindDescendantOrSelf(roots[i], objectName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static GameObject FindDescendantOrSelf(GameObject root, string objectName)
        {
            if (root == null)
            {
                return null;
            }

            if (string.Equals(root.name, objectName, StringComparison.Ordinal))
            {
                return root;
            }

            Transform transform = root.transform;
            for (int i = 0; i < transform.childCount; i++)
            {
                GameObject found = FindDescendantOrSelf(transform.GetChild(i).gameObject, objectName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static bool AnyActiveInHierarchy(GameObject[] objects)
        {
            if (objects == null)
            {
                return false;
            }

            for (int i = 0; i < objects.Length; i++)
            {
                if (objects[i] != null && objects[i].activeInHierarchy)
                {
                    return true;
                }
            }

            return false;
        }

        private static int CountActiveAlive(CombatHealth[] healths)
        {
            if (healths == null)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < healths.Length; i++)
            {
                CombatHealth health = healths[i];
                if (health != null && health.gameObject.activeInHierarchy && health.IsAlive)
                {
                    count++;
                }
            }

            return count;
        }

        private static int CountNonNull(CombatHealth[] healths)
        {
            if (healths == null)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < healths.Length; i++)
            {
                if (healths[i] != null)
                {
                    count++;
                }
            }

            return count;
        }

        private static void AppendMovementState(
            Player.PlayerMovementController player,
            string label,
            StringBuilder report)
        {
            if (player == null)
            {
                report.AppendLine($"{label}Movement=<null>");
                return;
            }

            report.AppendLine($"{label}Position={FormatVector3(player.transform.position)}");
            report.AppendLine($"{label}PlayerMovementEnabled={player.enabled}");
            CharacterController characterController = player.GetComponent<CharacterController>();
            report.AppendLine(
                $"{label}CharacterControllerEnabled={characterController != null && characterController.enabled}");
            report.AppendLine($"{label}LaneConstraint={player.LaneConstraintEnabled}");
            report.AppendLine($"{label}CinematicLocked={player.IsCinematicMoveInputLocked}");
            report.AppendLine(
                $"{label}ActionMoveScaleActive={GetField<bool>(player, "actionMoveInputScaleActive")} scale={GetField<float>(player, "actionMoveInputSpeedScale"):0.###}");
            report.AppendLine(
                $"{label}CinematicMoveScaleActive={GetField<bool>(player, "cinematicMoveInputScaleActive")} scale={GetField<float>(player, "cinematicMoveInputSpeedScale"):0.###}");
            report.AppendLine($"{label}PlanarVelocity={FormatVector3(player.PlanarVelocity)}");
        }

        private static void AppendNearbySolidColliders(
            Vector3 center,
            Transform ignoredRoot,
            string label,
            StringBuilder report)
        {
            Collider[] colliders = Physics.OverlapSphere(
                center,
                NearbyColliderRadius,
                ~0,
                QueryTriggerInteraction.Ignore);
            var entries = new List<ColliderDiagnostic>();
            for (int i = 0; i < colliders.Length; i++)
            {
                Collider collider = colliders[i];
                if (collider == null
                    || !collider.enabled
                    || collider.isTrigger
                    || !collider.gameObject.activeInHierarchy)
                {
                    continue;
                }

                Transform colliderTransform = collider.transform;
                if (ignoredRoot != null
                    && (colliderTransform == ignoredRoot || colliderTransform.IsChildOf(ignoredRoot)))
                {
                    continue;
                }

                Bounds bounds = collider.bounds;
                float distance = Vector3.Distance(center, bounds.ClosestPoint(center));
                entries.Add(new ColliderDiagnostic(
                    distance,
                    collider.GetType().Name,
                    collider.gameObject.layer,
                    FormatVector3(bounds.center),
                    FormatVector3(bounds.size),
                    GetHierarchyPath(colliderTransform)));
            }

            entries.Sort((left, right) => left.Distance.CompareTo(right.Distance));
            report.AppendLine(
                $"{label}NearbySolidColliders={entries.Count} radius={NearbyColliderRadius:0.###} center={FormatVector3(center)}");
            int count = Mathf.Min(entries.Count, 12);
            for (int i = 0; i < count; i++)
            {
                ColliderDiagnostic entry = entries[i];
                report.AppendLine(
                    $"{label}Collider{i + 1:00}=distance={entry.Distance:0.###} type={entry.ColliderType} layer={entry.Layer} center={entry.Center} size={entry.Size} path={entry.Path}");
            }
        }

        private void Finish(bool passed, StringBuilder report, string message)
        {
            report.AppendLine($"message={message}");
            report.AppendLine(passed ? "RESULT=PASS" : "RESULT=FAIL");
            WriteText(reportPath, report.ToString());
            WriteText(resultPath, $"RESULT={(passed ? "PASS" : "FAIL")}\nREPORT={reportPath}\nMESSAGE={message}\n");
            Debug.Log($"[OlympusCorridorCombatFlowPlayModeProbe] {(passed ? "PASS" : "FAIL")} {message}");
        }

        private static void WriteText(string path, string text)
        {
            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(path, text);
        }

        private static string FormatVector3(Vector3 value)
        {
            return $"({value.x:0.###}, {value.y:0.###}, {value.z:0.###})";
        }

        private static string GetHierarchyPath(Transform transform)
        {
            if (transform == null)
            {
                return "<null>";
            }

            var names = new List<string>();
            Transform current = transform;
            while (current != null)
            {
                names.Add(current.name);
                current = current.parent;
            }

            names.Reverse();
            return string.Join("/", names);
        }

        private readonly struct ColliderDiagnostic
        {
            public ColliderDiagnostic(
                float distance,
                string colliderType,
                int layer,
                string center,
                string size,
                string path)
            {
                Distance = distance;
                ColliderType = colliderType;
                Layer = layer;
                Center = center;
                Size = size;
                Path = path;
            }

            public float Distance { get; }
            public string ColliderType { get; }
            public int Layer { get; }
            public string Center { get; }
            public string Size { get; }
            public string Path { get; }
        }

        private sealed class ProbeResult
        {
            public bool Failed { get; private set; }
            public string FailureReason { get; private set; } = string.Empty;

            public void Fail(string reason)
            {
                Failed = true;
                FailureReason = reason;
            }
        }
    }
}
