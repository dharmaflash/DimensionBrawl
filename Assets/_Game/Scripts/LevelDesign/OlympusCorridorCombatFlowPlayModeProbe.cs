using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using DimensionBrawl.Combat;
using DimensionBrawl.Presentation;
using DimensionBrawl.UI;
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
            OlympusCorridorTutorialDirector tutorialDirector =
                GetField<OlympusCorridorTutorialDirector>(flow, "tutorialDirector");
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

            bool tutorialPath = flow.TutorialRunning
                || flow.TutorialCompleted
                || (tutorialDirector != null && tutorialDirector.TutorialEnabled);
            bool preCorridorGateSatisfied;
            bool introDamageApplied = false;
            if (tutorialPath)
            {
                if (tutorialDirector == null)
                {
                    tutorialDirector = GetField<OlympusCorridorTutorialDirector>(flow, "tutorialDirector");
                }

                report.AppendLine("tutorialPath=True");
                report.AppendLine(
                    $"tutorialBeforeInputs running={flow.TutorialRunning} completed={flow.TutorialCompleted} step={tutorialDirector?.CurrentStepId}");
                yield return CompleteTutorialWithRuntimeInputs(
                    flow,
                    tutorialDirector,
                    player,
                    introEnemies,
                    deadline,
                    report,
                    result);
                if (result.Failed)
                {
                    Finish(false, report, result.FailureReason);
                    yield break;
                }

                preCorridorGateSatisfied = flow.TutorialCompleted;
                report.AppendLine(
                    $"tutorialAfterInputs running={flow.TutorialRunning} completed={flow.TutorialCompleted} step={tutorialDirector?.CurrentStepId}");
            }
            else
            {
                report.AppendLine("tutorialPath=False");
                introDamageApplied = ApplyLethalDamageToAll(introEnemies, DamageTeam.Player);
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

                preCorridorGateSatisfied = introDamageApplied && flow.IntroGateCleared;
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
                preCorridorGateSatisfied
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

        private static IEnumerator CompleteTutorialWithRuntimeInputs(
            OlympusCorridorCombatFlowController flow,
            OlympusCorridorTutorialDirector tutorialDirector,
            Player.PlayerMovementController player,
            CombatHealth[] tutorialTargets,
            float deadline,
            StringBuilder report,
            ProbeResult result)
        {
            if (tutorialDirector == null)
            {
                result.Fail("Missing tutorial director for tutorial path.");
                yield break;
            }

            if (player == null)
            {
                result.Fail("Missing player for tutorial path.");
                yield break;
            }

            Player.PlayerActionController actionController = player.GetComponent<Player.PlayerActionController>();
            Player.PlayerCombatModeController combatModeController =
                player.GetComponent<Player.PlayerCombatModeController>();
            Player.PlayerRangedBasicAttackAction rangedBasicAttackAction =
                player.GetComponent<Player.PlayerRangedBasicAttackAction>();
            GameObject combatHudRoot = FindSceneObject("BossBarrageLaneReview_CombatHudCanvas")
                ?? FindSceneObject("PF_UI_CombatHud");
            BossBarrageLaneReviewMobileHud mobileHud = FindFirst<BossBarrageLaneReviewMobileHud>();
            OlympusTutorialOverlayPresenter overlayPresenter = FindFirst<OlympusTutorialOverlayPresenter>();
            if (actionController == null)
            {
                result.Fail("Missing PlayerActionController for tutorial path.");
                yield break;
            }

            if (combatModeController == null)
            {
                result.Fail("Missing PlayerCombatModeController for tutorial path.");
                yield break;
            }

            yield return null;
            AppendTutorialUiDiagnostics("tutorialUi_Melee", tutorialDirector, combatHudRoot, mobileHud, overlayPresenter, report);

            yield return QueueBasicAttackUntilStep(
                flow,
                tutorialDirector,
                actionController,
                "Move",
                deadline,
                report,
                result);
            if (result.Failed || flow.TutorialCompleted)
            {
                yield break;
            }

            yield return null;
            AppendTutorialUiDiagnostics("tutorialUi_Move", tutorialDirector, combatHudRoot, mobileHud, overlayPresenter, report);

            Vector3 tutorialMoveTarget = player.transform.position
                + Vector3.ProjectOnPlane(player.transform.right, Vector3.up).normalized * 2f;
            yield return MovePlayerWithInputToPosition(
                player,
                tutorialMoveTarget,
                deadline,
                "tutorialMoveInput",
                report,
                result);
            if (result.Failed)
            {
                yield break;
            }

            yield return WaitFor(
                () => tutorialDirector.CurrentStepId == "SwapToRanged" || flow.TutorialCompleted,
                deadline,
                "tutorial advanced to swap step from movement input",
                report,
                result);
            if (result.Failed || flow.TutorialCompleted)
            {
                yield break;
            }

            yield return null;
            AppendTutorialUiDiagnostics("tutorialUi_SwapToRanged", tutorialDirector, combatHudRoot, mobileHud, overlayPresenter, report);

            yield return QueueSwapUntilStep(
                flow,
                tutorialDirector,
                combatModeController,
                "Fire",
                deadline,
                report,
                result);
            if (result.Failed || flow.TutorialCompleted)
            {
                yield break;
            }

            yield return null;
            AppendTutorialUiDiagnostics("tutorialUi_Fire", tutorialDirector, combatHudRoot, mobileHud, overlayPresenter, report);
            AppendTutorialFireDiagnostics("tutorialFireBeforeInput", player, rangedBasicAttackAction, tutorialTargets, report);

            yield return QueueFireUntilStep(
                flow,
                tutorialDirector,
                rangedBasicAttackAction,
                player,
                tutorialTargets,
                "Dodge",
                deadline,
                report,
                result);
            if (result.Failed || flow.TutorialCompleted)
            {
                yield break;
            }

            yield return null;
            AppendTutorialUiDiagnostics("tutorialUi_Dodge", tutorialDirector, combatHudRoot, mobileHud, overlayPresenter, report);

            int dodgeQueueFrames = 0;
            while (tutorialDirector.CurrentStepId != "ClearTargets" && !flow.TutorialCompleted)
            {
                if (Time.realtimeSinceStartup > deadline)
                {
                    result.Fail("Timed out advancing tutorial dodge step to clear-targets step.");
                    yield break;
                }

                actionController.QueueDodge();
                dodgeQueueFrames++;
                yield return null;
            }

            report.AppendLine($"tutorialDodgeQueuedFrames={dodgeQueueFrames}");
            int tutorialTargetsAliveAfterDodge = CountActiveAlive(tutorialTargets);
            report.AppendLine($"tutorialTargetsAliveAfterDodge={tutorialTargetsAliveAfterDodge}");
            if (flow.TutorialCompleted && tutorialTargetsAliveAfterDodge > 0)
            {
                result.Fail("Tutorial completed while tutorial targets were still alive.");
                yield break;
            }

            if (!flow.TutorialCompleted)
            {
                yield return null;
                AppendTutorialUiDiagnostics("tutorialUi_ClearTargets", tutorialDirector, combatHudRoot, mobileHud, overlayPresenter, report);
                int tutorialTargetsAliveBeforeClear = CountActiveAlive(tutorialTargets);
                bool tutorialClearDamageApplied = ApplyLethalDamageToAll(tutorialTargets, DamageTeam.Player);
                report.AppendLine($"tutorialTargetsAliveBeforeClear={tutorialTargetsAliveBeforeClear}");
                report.AppendLine($"tutorialClearDamageApplied={tutorialClearDamageApplied}");

                yield return WaitFor(
                    () => flow.TutorialCompleted,
                    deadline,
                    "tutorial completed after all tutorial targets defeated",
                    report,
                    result);
                if (result.Failed)
                {
                    yield break;
                }
            }

            report.AppendLine("tutorial completed from runtime inputs=True");
        }

        private static IEnumerator QueueBasicAttackUntilStep(
            OlympusCorridorCombatFlowController flow,
            OlympusCorridorTutorialDirector tutorialDirector,
            Player.PlayerActionController actionController,
            string expectedStep,
            float deadline,
            StringBuilder report,
            ProbeResult result)
        {
            int frames = 0;
            while (tutorialDirector.CurrentStepId != expectedStep && !flow.TutorialCompleted)
            {
                if (Time.realtimeSinceStartup > deadline)
                {
                    result.Fail($"Timed out waiting for tutorial step {expectedStep} from basic attack.");
                    yield break;
                }

                actionController.QueueBasicAttack();
                frames++;
                yield return null;
            }

            report.AppendLine($"tutorialMeleeQueuedFrames={frames}");
            report.AppendLine($"tutorial advanced to {expectedStep} step from melee input=True");
        }

        private static IEnumerator QueueSwapUntilStep(
            OlympusCorridorCombatFlowController flow,
            OlympusCorridorTutorialDirector tutorialDirector,
            Player.PlayerCombatModeController combatModeController,
            string expectedStep,
            float deadline,
            StringBuilder report,
            ProbeResult result)
        {
            int frames = 0;
            while (tutorialDirector.CurrentStepId != expectedStep && !flow.TutorialCompleted)
            {
                if (Time.realtimeSinceStartup > deadline)
                {
                    result.Fail($"Timed out waiting for tutorial step {expectedStep} from swap input.");
                    yield break;
                }

                combatModeController.QueueCombatModeSwap();
                frames++;
                yield return null;
            }

            report.AppendLine($"tutorialSwapQueuedFrames={frames}");
            report.AppendLine($"tutorial advanced to {expectedStep} step from swap input=True");
        }

        private static IEnumerator QueueFireUntilStep(
            OlympusCorridorCombatFlowController flow,
            OlympusCorridorTutorialDirector tutorialDirector,
            Player.PlayerRangedBasicAttackAction rangedBasicAttackAction,
            Player.PlayerMovementController player,
            CombatHealth[] tutorialTargets,
            string expectedStep,
            float deadline,
            StringBuilder report,
            ProbeResult result)
        {
            if (rangedBasicAttackAction == null)
            {
                result.Fail("Missing PlayerRangedBasicAttackAction for tutorial fire step.");
                yield break;
            }

            int frames = 0;
            while (tutorialDirector.CurrentStepId != expectedStep && !flow.TutorialCompleted)
            {
                if (Time.realtimeSinceStartup > deadline)
                {
                    AppendTutorialFireDiagnostics(
                        "tutorialFireTimeout",
                        player,
                        rangedBasicAttackAction,
                        tutorialTargets,
                        report);
                    result.Fail($"Timed out waiting for tutorial step {expectedStep} from fire input.");
                    yield break;
                }

                rangedBasicAttackAction.QueueFire();
                frames++;
                yield return null;
            }

            report.AppendLine($"tutorialFireQueuedFrames={frames}");
            report.AppendLine($"tutorial advanced to {expectedStep} step from fire input=True");
        }

        private static void AppendTutorialFireDiagnostics(
            string label,
            Player.PlayerMovementController player,
            Player.PlayerRangedBasicAttackAction rangedBasicAttackAction,
            CombatHealth[] tutorialTargets,
            StringBuilder report)
        {
            if (rangedBasicAttackAction == null)
            {
                report.AppendLine($"{label}RangedAction=<null>");
                return;
            }

            report.AppendLine(
                $"{label}RangedActionEnabled={rangedBasicAttackAction.enabled} gameObjectActive={rangedBasicAttackAction.gameObject.activeInHierarchy}");
            report.AppendLine(
                $"{label}FireReady={rangedBasicAttackAction.IsFireReady} cooldown={rangedBasicAttackAction.FireCooldownRemaining:0.###} blocked='{rangedBasicAttackAction.LastUseBlockedReason}' activeProjectiles={rangedBasicAttackAction.ActiveProjectileCount}");
            report.AppendLine(
                $"{label}AimPreviewActive={rangedBasicAttackAction.IsAimPreviewActive} hasAimAssist={rangedBasicAttackAction.HasAimAssistTarget} aimAssistTarget={GetHierarchyPath(rangedBasicAttackAction.AimAssistTargetHealth != null ? rangedBasicAttackAction.AimAssistTargetHealth.transform : null)}");
            if (rangedBasicAttackAction.TryGetAimPreviewDirection(out Vector3 aimDirection))
            {
                report.AppendLine($"{label}AimDirection={FormatVector3(aimDirection)}");
            }

            if (rangedBasicAttackAction.TryGetAimPreviewWorldPoint(out Vector3 aimPoint))
            {
                report.AppendLine($"{label}AimPoint={FormatVector3(aimPoint)}");
            }

            Transform fireOrigin = rangedBasicAttackAction.FireOrigin;
            report.AppendLine(
                $"{label}FireOrigin={(fireOrigin != null ? FormatVector3(fireOrigin.position) : "<null>")}");
            if (player != null)
            {
                Player.PlayerCombatModeController combatModeController =
                    player.GetComponent<Player.PlayerCombatModeController>();
                report.AppendLine(
                    $"{label}CombatMode={(combatModeController != null ? combatModeController.CurrentMode.ToString() : "<null>")} isRanged={combatModeController != null && combatModeController.IsRangedMode}");
                report.AppendLine($"{label}PlayerPosition={FormatVector3(player.transform.position)}");
            }

            if (tutorialTargets == null)
            {
                return;
            }

            for (int i = 0; i < tutorialTargets.Length; i++)
            {
                CombatHealth target = tutorialTargets[i];
                if (target == null)
                {
                    report.AppendLine($"{label}Target{i}=<null>");
                    continue;
                }

                float playerDistance = player != null
                    ? Vector3.Distance(
                        Vector3.ProjectOnPlane(target.transform.position, Vector3.up),
                        Vector3.ProjectOnPlane(player.transform.position, Vector3.up))
                    : -1f;
                report.AppendLine(
                    $"{label}Target{i} alive={target.IsAlive} active={target.gameObject.activeInHierarchy} hp={target.CurrentHealth:0.###}/{target.MaxHealth:0.###} pos={FormatVector3(target.transform.position)} planarPlayerDistance={playerDistance:0.###}");
            }
        }

        private static void AppendTutorialUiDiagnostics(
            string label,
            OlympusCorridorTutorialDirector tutorialDirector,
            GameObject combatHudRoot,
            BossBarrageLaneReviewMobileHud mobileHud,
            OlympusTutorialOverlayPresenter overlayPresenter,
            StringBuilder report)
        {
            report.AppendLine($"{label}Screen={Screen.width}x{Screen.height}");
            report.AppendLine($"{label}Step={tutorialDirector?.CurrentStepId ?? "<null>"}");
            if (combatHudRoot == null)
            {
                report.AppendLine($"{label}CombatHud=<null>");
            }
            else
            {
                AppendCombatHudRect(label, "Move", combatHudRoot, "MoveJoystickRing", report);
                AppendCombatHudRect(label, "Basic", combatHudRoot, "BasicAttackButton", report);
                AppendCombatHudRect(label, "Dodge", combatHudRoot, "DodgeButton", report);
                AppendCombatHudRect(label, "Swap", combatHudRoot, "UltimateButton", report);
            }

            if (mobileHud == null)
            {
                report.AppendLine($"{label}MobileHud=<null>");
            }
            else
            {
                report.AppendLine(
                    $"{label}MobileHudMoveRect={FormatRect(mobileHud.MoveJoystickGuiRect)} anchor={FormatVector2(mobileHud.MoveJoystickScreenAnchor)}");
                report.AppendLine(
                    $"{label}MobileHudBasicRect={FormatRect(mobileHud.BasicButtonGuiRect)} anchor={FormatVector2(mobileHud.BasicButtonScreenAnchor)}");
                report.AppendLine(
                    $"{label}MobileHudDodgeRect={FormatRect(mobileHud.DodgeButtonGuiRect)} anchor={FormatVector2(mobileHud.DodgeButtonScreenAnchor)}");
                report.AppendLine(
                    $"{label}MobileHudSwapRect={FormatRect(mobileHud.SwapButtonGuiRect)} anchor={FormatVector2(mobileHud.SwapButtonScreenAnchor)}");
            }

            if (overlayPresenter == null)
            {
                report.AppendLine($"{label}Overlay=<null>");
                return;
            }

            Vector2 overlayCenter = overlayPresenter.CurrentFocusCenterGuiPoint;
            report.AppendLine(
                $"{label}Overlay visible={overlayPresenter.Visible} kind={overlayPresenter.CurrentFocusKind} anchor={FormatVector2(overlayPresenter.CurrentFocusAnchor)} center={FormatVector2(overlayCenter)} markerRect={FormatRect(overlayPresenter.CurrentFocusMarkerGuiRect)} dialogueRect={FormatRect(overlayPresenter.CurrentDialoguePanelGuiRect)}");

            if (TryResolveCombatHudRectForFocus(
                    combatHudRoot,
                    overlayPresenter.CurrentFocusKind,
                    out Rect combatHudRect))
            {
                float combatHudDistance = Vector2.Distance(overlayCenter, combatHudRect.center);
                report.AppendLine(
                    $"{label}OverlayToCombatHudDistance={combatHudDistance:0.###} combatHudCenter={FormatVector2(combatHudRect.center)}");
            }
            else
            {
                report.AppendLine($"{label}OverlayToCombatHudDistance=<n/a>");
            }

            if (TryResolveMobileHudRectForFocus(
                    mobileHud,
                    overlayPresenter.CurrentFocusKind,
                    out Rect mobileHudRect))
            {
                float mobileHudDistance = Vector2.Distance(overlayCenter, mobileHudRect.center);
                report.AppendLine(
                    $"{label}OverlayToMobileHudDistance={mobileHudDistance:0.###} mobileHudCenter={FormatVector2(mobileHudRect.center)}");
            }
            else
            {
                report.AppendLine($"{label}OverlayToMobileHudDistance=<n/a>");
            }
        }

        private static void AppendCombatHudRect(
            string label,
            string rectLabel,
            GameObject combatHudRoot,
            string objectName,
            StringBuilder report)
        {
            bool found = TryGetCombatHudGuiRect(combatHudRoot, objectName, out Rect rect);
            if (found)
            {
                report.AppendLine(
                    $"{label}CombatHud{rectLabel}Rect={FormatRect(rect)} anchor={FormatVector2(ToScreenAnchor(rect.center))}");
            }
            else
            {
                report.AppendLine($"{label}CombatHud{rectLabel}Rect=<missing>");
            }
        }

        private static bool TryResolveCombatHudRectForFocus(
            GameObject combatHudRoot,
            OlympusTutorialOverlayPresenter.FocusKind focusKind,
            out Rect rect)
        {
            if (combatHudRoot == null)
            {
                rect = default;
                return false;
            }

            string objectName = ResolveCombatHudObjectName(focusKind);
            if (string.IsNullOrWhiteSpace(objectName))
            {
                rect = default;
                return false;
            }

            return TryGetCombatHudGuiRect(combatHudRoot, objectName, out rect);
        }

        private static string ResolveCombatHudObjectName(OlympusTutorialOverlayPresenter.FocusKind focusKind)
        {
            switch (focusKind)
            {
                case OlympusTutorialOverlayPresenter.FocusKind.MeleeAttack:
                case OlympusTutorialOverlayPresenter.FocusKind.RangedAttack:
                    return "BasicAttackButton";
                case OlympusTutorialOverlayPresenter.FocusKind.Dodge:
                    return "DodgeButton";
                case OlympusTutorialOverlayPresenter.FocusKind.MoveStick:
                    return "MoveJoystickRing";
                case OlympusTutorialOverlayPresenter.FocusKind.SwapMode:
                    return "UltimateButton";
                default:
                    return null;
            }
        }

        private static bool TryGetCombatHudGuiRect(GameObject combatHudRoot, string objectName, out Rect rect)
        {
            GameObject target = combatHudRoot != null
                ? FindDescendantOrSelf(combatHudRoot, objectName)
                : FindSceneObject(objectName);
            RectTransform rectTransform = target != null ? target.GetComponent<RectTransform>() : null;
            return TryGetGuiRect(rectTransform, out rect);
        }

        private static bool TryResolveMobileHudRectForFocus(
            BossBarrageLaneReviewMobileHud mobileHud,
            OlympusTutorialOverlayPresenter.FocusKind focusKind,
            out Rect rect)
        {
            if (mobileHud == null)
            {
                rect = default;
                return false;
            }

            switch (focusKind)
            {
                case OlympusTutorialOverlayPresenter.FocusKind.MeleeAttack:
                case OlympusTutorialOverlayPresenter.FocusKind.RangedAttack:
                    rect = mobileHud.BasicButtonGuiRect;
                    return true;
                case OlympusTutorialOverlayPresenter.FocusKind.Dodge:
                    rect = mobileHud.DodgeButtonGuiRect;
                    return true;
                case OlympusTutorialOverlayPresenter.FocusKind.MoveStick:
                    rect = mobileHud.MoveJoystickGuiRect;
                    return true;
                case OlympusTutorialOverlayPresenter.FocusKind.SwapMode:
                    rect = mobileHud.SwapButtonGuiRect;
                    return true;
                default:
                    rect = default;
                    return false;
            }
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

        private static string FormatVector2(Vector2 value)
        {
            return $"({value.x:0.###}, {value.y:0.###})";
        }

        private static string FormatRect(Rect value)
        {
            return $"(x={value.x:0.###}, y={value.y:0.###}, w={value.width:0.###}, h={value.height:0.###})";
        }

        private static Vector2 ToScreenAnchor(Vector2 guiPoint)
        {
            if (Screen.width <= 0 || Screen.height <= 0)
            {
                return Vector2.zero;
            }

            return new Vector2(
                Mathf.Clamp01(guiPoint.x / Screen.width),
                Mathf.Clamp01(1f - guiPoint.y / Screen.height));
        }

        private static bool TryGetGuiRect(RectTransform rectTransform, out Rect rect)
        {
            if (rectTransform == null || Screen.width <= 0 || Screen.height <= 0)
            {
                rect = default;
                return false;
            }

            Canvas canvas = rectTransform.GetComponentInParent<Canvas>();
            Camera camera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? canvas.worldCamera
                : null;
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);

            Vector2 min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
            Vector2 max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
            for (int i = 0; i < corners.Length; i++)
            {
                Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(camera, corners[i]);
                Vector2 guiPoint = new Vector2(screenPoint.x, Screen.height - screenPoint.y);
                min = Vector2.Min(min, guiPoint);
                max = Vector2.Max(max, guiPoint);
            }

            rect = Rect.MinMaxRect(min.x, min.y, max.x, max.y);
            return rect.width > 0.01f && rect.height > 0.01f;
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
