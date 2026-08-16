using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DimensionBrawl.Combat;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.Player;
using DimensionBrawl.Presentation;
using DimensionBrawl.UI;
using IsekaiBrawl.Gameplay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;
using UnityEngine.UI;

namespace DimensionBrawl.Editor
{
    public static class RuntimeSceneWiringReadinessReporter
    {
        private const string ReportPath = "C:/tmp/DimensionBrawl-RuntimeSceneWiringReadinessReport.md";

        private static readonly SceneExpectation[] MinimumSceneExpectations =
        {
            new("Assets/_Game/Scenes/OlympusCorridorInvasionStage.unity", SceneContractKind.CanonicalCombat),
            new("Assets/_Game/Scenes/OlympusStationCombatStage.unity", SceneContractKind.CanonicalCombat)
        };

        private const string StationCombatScenePath =
            "Assets/_Game/Scenes/OlympusStationCombatStage.unity";
        private const string CanonicalBossRootName =
            "BossBarrageLaneReview_BossProxy_NeedleLock";
        private const string CanonicalBossVisualName =
            "BossBarrageLaneReview_HumanoidBossVisual_SciFiSoldier_01_Commando";
        private const string CanonicalBossVisualPrefabPath =
            ActionFoundationSciFiSoldier01VisualSetup.CommandoVisualPrefabPath;
        private const string CanonicalBossAnimatorControllerPath =
            ActionFoundationSciFiSoldier01VisualSetup.ControllerPath;

        [MenuItem("DimensionBrawl/Reports/Runtime Scene Wiring Readiness")]
        public static void ReportMenu()
        {
            ReportCurrentReadiness();
        }

        public static bool ReportCurrentReadiness()
        {
            ReportBuilder report = new();
            SceneSetup[] setup = EditorSceneManager.GetSceneManagerSetup();
            if (HasDirtyOpenScene(out string dirtyScenePath))
            {
                report.AddIssue($"Open scene is dirty before inspection: {dirtyScenePath}");
                report.AppendSummary();
                WriteReport(report);
                Debug.LogWarning($"Runtime scene wiring readiness failed before inspection. See {ReportPath}");
                return false;
            }

            try
            {
                for (int i = 0; i < MinimumSceneExpectations.Length; i++)
                {
                    InspectScene(MinimumSceneExpectations[i], report);
                }
            }
            finally
            {
                if (setup.Length > 0)
                {
                    EditorSceneManager.RestoreSceneManagerSetup(setup);
                }
            }

            report.AppendSummary();
            WriteReport(report);

            if (report.Passed)
            {
                Debug.Log($"Runtime scene wiring readiness passed. Report is read-only; no assets, prefabs, scenes, or ProjectSettings were modified. See {ReportPath}");
                return true;
            }

            Debug.LogWarning($"Runtime scene wiring readiness found issues. Report is read-only; no assets, prefabs, scenes, or ProjectSettings were modified. See {ReportPath}");
            return false;
        }

        private static bool HasDirtyOpenScene(out string dirtyScenePath)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.isDirty)
                {
                    dirtyScenePath = string.IsNullOrWhiteSpace(scene.path) ? scene.name : scene.path;
                    return true;
                }
            }

            dirtyScenePath = string.Empty;
            return false;
        }

        private static void InspectScene(SceneExpectation expectation, ReportBuilder report)
        {
            Scene scene = EditorSceneManager.OpenScene(expectation.ScenePath, OpenSceneMode.Single);
            bool dirtyBefore = scene.isDirty;
            GameObject[] roots = scene.GetRootGameObjects();
            int transformCount = 0;
            int missingScriptCount = 0;

            for (int i = 0; i < roots.Length; i++)
            {
                transformCount += roots[i].GetComponentsInChildren<Transform>(true).Length;
                missingScriptCount += GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(roots[i]);
            }

            report.InspectedSceneCount++;
            report.AppendLine($"## {expectation.ScenePath}");
            report.AppendLine();
            report.AppendLine($"- roots: {roots.Length}");
            report.AppendLine($"- transforms: {transformCount}");
            report.AppendLine($"- missing scripts: {missingScriptCount}");
            report.AppendLine($"- dirty before inspection: {dirtyBefore}");

            if (missingScriptCount > 0)
            {
                report.AddIssue($"{expectation.ScenePath}: missing MonoBehaviour script slots = {missingScriptCount}");
            }

            CheckGenericRuntimeComponents(expectation, report);
            CheckSceneRoleContract(expectation, report);

            bool dirtyAfter = scene.isDirty;
            report.AppendLine($"- dirty after inspection: {dirtyAfter}");
            if (dirtyBefore != dirtyAfter || dirtyAfter)
            {
                report.AddIssue($"{expectation.ScenePath}: dirty flag changed or remained dirty during inspection.");
            }

            report.AppendLine();
        }

        private static void CheckGenericRuntimeComponents(SceneExpectation expectation, ReportBuilder report)
        {
            BattleManager[] managers = Object.FindObjectsByType<BattleManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            report.CheckedContractCount++;
            report.AppendLine($"- BattleManager count: {managers.Length}");

            if (managers.Length > 1)
            {
                report.AddIssue($"{expectation.ScenePath}: scene has multiple BattleManager instances.");
            }

            BattleHUD[] huds = Object.FindObjectsByType<BattleHUD>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            report.CheckedContractCount++;
            report.AppendLine($"- BattleHUD count: {huds.Length}");

            for (int i = 0; i < huds.Length; i++)
            {
                Canvas canvas = huds[i].GetComponentInParent<Canvas>(true);
                if (canvas == null)
                {
                    report.AddIssue($"{expectation.ScenePath}: BattleHUD '{huds[i].name}' is not under a Canvas.");
                }
            }
            MobileBattleControls[] controls =
                Object.FindObjectsByType<MobileBattleControls>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            report.CheckedContractCount++;
            report.AppendLine($"- MobileBattleControls count: {controls.Length}");

            if (controls.Length == 0 && huds.Length > 0)
            {
                report.AppendLine("- MobileBattleControls absence classified as runtime-created by BattleHUD.");
            }

            for (int i = 0; i < controls.Length; i++)
            {
                Canvas canvas = controls[i].GetComponentInParent<Canvas>(true);
                if (canvas == null)
                {
                    report.AddIssue($"{expectation.ScenePath}: MobileBattleControls '{controls[i].name}' is not under a Canvas.");
                }
            }

            PveEncounterDirector[] directors =
                Object.FindObjectsByType<PveEncounterDirector>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            report.CheckedContractCount++;
            report.AppendLine($"- PveEncounterDirector count: {directors.Length}");

            for (int i = 0; i < directors.Length; i++)
            {
                SerializedObject serializedDirector = new(directors[i]);
                SerializedProperty defaultStage = serializedDirector.FindProperty("defaultStage");
                SerializedProperty allowRuntimeStageRootBootstrap =
                    serializedDirector.FindProperty("allowRuntimeStageRootBootstrap");
                bool hasDefaultStage = defaultStage != null && defaultStage.objectReferenceValue != null;
                bool allowsRuntimeStageRootBootstrap =
                    allowRuntimeStageRootBootstrap != null && allowRuntimeStageRootBootstrap.boolValue;

                report.AppendLine(
                    $"- PveEncounterDirector '{directors[i].name}': defaultStage={(hasDefaultStage ? "set" : "null")}, runtime root bootstrap={allowsRuntimeStageRootBootstrap}");

                if (!hasDefaultStage && !allowsRuntimeStageRootBootstrap)
                {
                    report.AddIssue(
                        $"{expectation.ScenePath}: PveEncounterDirector '{directors[i].name}' has no defaultStage and no documented runtime bootstrap fallback.");
                }
            }
        }

        private static void CheckSceneRoleContract(SceneExpectation expectation, ReportBuilder report)
        {
            report.CheckedContractCount++;
            report.AppendLine($"- scene contract kind: {expectation.ContractKind}");

            switch (expectation.ContractKind)
            {
                case SceneContractKind.CanonicalCombat:
                    CheckCanonicalCombatContract(expectation, report);
                    break;
                default:
                    report.AddIssue($"{expectation.ScenePath}: unknown scene contract kind {expectation.ContractKind}.");
                    break;
            }
        }

        private static void CheckCanonicalCombatContract(SceneExpectation expectation, ReportBuilder report)
        {
            RequireSingle<BossBarrageEncounterController>(expectation, report);
            RequireSingle<CombatHudPresenter>(expectation, report);
            RequireSingle<CombatHudInputBridge>(expectation, report);
            RequireSingle<BossBarrageLaneReviewCombatHudBinder>(expectation, report);
            RequireSingle<CombatSessionOverlayPresenter>(expectation, report);

            if (expectation.ScenePath == StationCombatScenePath)
            {
                CheckStationResultOwnership(expectation, report);
            }

            CheckCanonicalBossVisual(expectation, report);
        }

        private static void CheckStationResultOwnership(SceneExpectation expectation, ReportBuilder report)
        {
            RequireSingle<CombatEncounterController>(expectation, report);
            RequireSingle<OlympusStageClearOverlay>(expectation, report);

            OlympusStationCombatResultPresenter[] presenters =
                Object.FindObjectsByType<OlympusStationCombatResultPresenter>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None);
            report.AppendLine($"- {nameof(OlympusStationCombatResultPresenter)} count: {presenters.Length}");
            if (presenters.Length != 1)
            {
                report.AddIssue(
                    $"{expectation.ScenePath}: expected exactly one {nameof(OlympusStationCombatResultPresenter)}, found {presenters.Length}.");
                return;
            }

            SerializedObject serializedPresenter = new(presenters[0]);
            SerializedProperty encounter = serializedPresenter.FindProperty("encounter");
            SerializedProperty stageClearOverlay = serializedPresenter.FindProperty("stageClearOverlay");
            SerializedProperty resultSurfaceBehaviour = serializedPresenter.FindProperty("resultSurfaceBehaviour");
            bool hasEncounter = encounter != null && encounter.objectReferenceValue is CombatEncounterController;
            bool hasStageClearOverlay =
                stageClearOverlay != null && stageClearOverlay.objectReferenceValue is OlympusStageClearOverlay;
            bool hasResultSurface =
                resultSurfaceBehaviour != null
                && resultSurfaceBehaviour.objectReferenceValue is MonoBehaviour behaviour
                && behaviour is ICombatSessionOverlay;
            report.AppendLine($"- Station result encounter: {(hasEncounter ? "set" : "missing")}");
            report.AppendLine($"- Station clear overlay: {(hasStageClearOverlay ? "set" : "missing")}");
            report.AppendLine($"- Station fail surface: {(hasResultSurface ? "set" : "missing")}");
            if (!hasEncounter || !hasStageClearOverlay || !hasResultSurface)
            {
                report.AddIssue(
                    $"{expectation.ScenePath}: Station result presenter must use authored encounter, clear-overlay, and fail-surface references.");
            }

            CheckStationBossAftermathOwnership(
                expectation,
                presenters[0],
                stageClearOverlay?.objectReferenceValue as OlympusStageClearOverlay,
                report);
        }

        private static void CheckStationBossAftermathOwnership(
            SceneExpectation expectation,
            OlympusStationCombatResultPresenter resultPresenter,
            OlympusStageClearOverlay stageClearOverlay,
            ReportBuilder report)
        {
            OlympusStationBossTerminalAftermathPresenter[] aftermaths =
                Object.FindObjectsByType<OlympusStationBossTerminalAftermathPresenter>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None);
            report.AppendLine(
                $"- {nameof(OlympusStationBossTerminalAftermathPresenter)} count: {aftermaths.Length}");
            if (aftermaths.Length != 1)
            {
                report.AddIssue(
                    $"{expectation.ScenePath}: expected exactly one authored Station boss-terminal aftermath gate, found {aftermaths.Length}.");
                return;
            }

            OlympusStationBossTerminalAftermathPresenter aftermath = aftermaths[0];
            SerializedObject serializedAftermath = new(aftermath);
            SerializedObject serializedResult = new(resultPresenter);
            SerializedObject serializedOverlay = stageClearOverlay != null
                ? new SerializedObject(stageClearOverlay)
                : null;
            bool exactGateOwners = serializedResult.FindProperty("bossTerminalAftermath")
                    ?.objectReferenceValue == aftermath
                && serializedOverlay?.FindProperty("bossTerminalAftermath")
                    ?.objectReferenceValue == aftermath;

            BossBarrageEncounterController[] bossEncounters =
                Object.FindObjectsByType<BossBarrageEncounterController>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None);
            PlayerMovementController[] movements =
                Object.FindObjectsByType<PlayerMovementController>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None);
            BossBarrageCameraCueDriver[] cameraDrivers =
                Object.FindObjectsByType<BossBarrageCameraCueDriver>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None);
            ActionCinematicCueDirector[] cinematicDirectors =
                Object.FindObjectsByType<ActionCinematicCueDirector>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None);
            OlympusStationBossTerminalFinisherCameraController[] finisherCameraControllers =
                Object.FindObjectsByType<OlympusStationBossTerminalFinisherCameraController>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None);
            bool hasExactSubjects = bossEncounters.Length == 1
                && movements.Length == 1
                && cameraDrivers.Length == 1
                && cinematicDirectors.Length == 1;
            CombatHealth bossHealth = hasExactSubjects
                ? new SerializedObject(bossEncounters[0]).FindProperty("bossHealth")
                    ?.objectReferenceValue as CombatHealth
                : null;
            PlayerMovementController movement = hasExactSubjects ? movements[0] : null;
            BossBarrageVisualCueDriver visualDriver =
                bossHealth != null ? bossHealth.GetComponent<BossBarrageVisualCueDriver>() : null;
            PlayerSupportSummonSlotAction[] supports = movement != null
                ? movement.GetComponents<PlayerSupportSummonSlotAction>()
                : System.Array.Empty<PlayerSupportSummonSlotAction>();
            PlayerSupportSummonSlotAction summon2 = System.Array.Find(
                supports,
                candidate => candidate != null && candidate.SlotActionName == "SummonSlot2");
            PlayerSupportSummonSlotAction summon3 = System.Array.Find(
                supports,
                candidate => candidate != null && candidate.SlotActionName == "SummonSlot3");

            bool exactReferences = hasExactSubjects
                && visualDriver != null
                && serializedAftermath.FindProperty("bossHealth")?.objectReferenceValue == bossHealth
                && serializedAftermath.FindProperty("cameraCueDriver")?.objectReferenceValue == cameraDrivers[0]
                && finisherCameraControllers.Length == 1
                && serializedAftermath.FindProperty("finisherCameraController")?.objectReferenceValue
                    == finisherCameraControllers[0]
                && serializedAftermath.FindProperty("actionCinematicCueDirector")?.objectReferenceValue
                    == cinematicDirectors[0]
                && cinematicDirectors[0].CameraController == cameraDrivers[0].CameraController
                && serializedAftermath.FindProperty("visualCueDriver")?.objectReferenceValue == visualDriver
                && serializedAftermath.FindProperty("playerMovement")?.objectReferenceValue == movement
                && serializedAftermath.FindProperty("playerActionController")?.objectReferenceValue
                    == movement.GetComponent<PlayerActionController>()
                && serializedAftermath.FindProperty("playerSkill1Action")?.objectReferenceValue
                    == movement.GetComponent<PlayerSkill1Action>()
                && serializedAftermath.FindProperty("playerSummonSlot1Action")?.objectReferenceValue
                    == movement.GetComponent<PlayerSummonSlot1Action>()
                && serializedAftermath.FindProperty("playerSummonSlot2Action")?.objectReferenceValue == summon2
                && serializedAftermath.FindProperty("playerSummonSlot3Action")?.objectReferenceValue == summon3
                && serializedAftermath.FindProperty("playerRangedBasicAttackAction")?.objectReferenceValue
                    == movement.GetComponent<PlayerRangedBasicAttackAction>()
                && serializedAftermath.FindProperty("playerCombatModeController")?.objectReferenceValue
                    == movement.GetComponent<PlayerCombatModeController>();

            Transform clearMarker = FindSceneTransform(
                SceneManager.GetActiveScene(),
                OlympusContinuousStageSetup.StationPocketClearMarkerName);
            Transform terminalBoundaryVisual = FindSceneTransform(
                SceneManager.GetActiveScene(),
                OlympusContinuousStageSetup.StationTerminalBoundaryVisualName);
            BossBarrageLaneReviewCombatHudBinder[] hudBinders =
                Object.FindObjectsByType<BossBarrageLaneReviewCombatHudBinder>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None);
            bool exactProductCleanup = hasExactSubjects
                && clearMarker != null
                && !clearMarker.gameObject.activeSelf
                && new SerializedObject(bossEncounters[0]).FindProperty("clearMarker")
                    ?.objectReferenceValue == null
                && terminalBoundaryVisual != null
                && terminalBoundaryVisual.gameObject.activeSelf
                && serializedAftermath.FindProperty("terminalBoundaryVisualRoot")
                    ?.objectReferenceValue == terminalBoundaryVisual.gameObject
                && hudBinders.Length == 1
                && string.Equals(
                    new SerializedObject(hudBinders[0]).FindProperty("bossDisplayName")
                        ?.stringValue,
                    OlympusContinuousStageSetup.StationBossDisplayName,
                    System.StringComparison.Ordinal);
            bool exactTiming = Approximately(
                    serializedAftermath.FindProperty("aftermathDurationSeconds")?.floatValue ?? -1f,
                    2.6f)
                && Approximately(
                    serializedAftermath.FindProperty("unattachedResultLeaseTimeoutSeconds")?.floatValue ?? -1f,
                    2f)
                && Approximately(
                    serializedAftermath.FindProperty("initialHitStopRecoveryGraceSeconds")?.floatValue ?? -1f,
                    0.35f);
            report.AppendLine($"- Station aftermath result/overlay owners: {(exactGateOwners ? "exact" : "invalid")}");
            report.AppendLine($"- Station aftermath source/camera takeover/input references: {(exactReferences ? "exact" : "invalid")}");
            report.AppendLine($"- Station aftermath timing (2.6/2.0/0.35): {(exactTiming ? "exact" : "invalid")}");
            report.AppendLine($"- Station terminal product cleanup/boss label: {(exactProductCleanup ? "exact" : "invalid")}");
            if (!exactGateOwners || !exactReferences || !exactTiming || !exactProductCleanup)
            {
                report.AddIssue(
                    $"{expectation.ScenePath}: Station aftermath gate must keep its exact result/overlay owners, action-camera takeover, finisher camera, boss presentation sources, eight player input owners, terminal visual cleanup, AKAZA label, and authored timing.");
            }

            if (!hasExactSubjects)
            {
                return;
            }

            CheckStationBossTerminalFinisherCamera(
                expectation,
                aftermath,
                bossHealth,
                cameraDrivers[0],
                report);

            SerializedObject serializedCamera = new(cameraDrivers[0]);
            SerializedProperty deathCamera = serializedCamera.FindProperty("bossDeathCue");
            bool exactCamera = deathCamera != null
                && deathCamera.FindPropertyRelative("enabled")?.boolValue == true
                && Approximately(
                    deathCamera.FindPropertyRelative("durationSeconds")?.floatValue ?? -1f,
                    1.65f)
                && Approximately(
                    serializedCamera.FindProperty("bossDeathCueReleaseSeconds")?.floatValue ?? -1f,
                    0.35f);
            SerializedObject serializedVisual = visualDriver != null
                ? new SerializedObject(visualDriver)
                : null;
            bool exactVisual = serializedVisual != null
                && serializedVisual.FindProperty("bossDeathCueId")?.intValue
                    == (int)CombatVfxCueId.EnemyDeath
                && Approximately(
                    serializedVisual.FindProperty("bossDeathCueIntensity")?.floatValue ?? -1f,
                    1.15f)
                && Approximately(
                    serializedVisual.FindProperty("bossDeathAudioIntensity")?.floatValue ?? -1f,
                    1f)
                && Approximately(
                    serializedVisual.FindProperty("bossDeathPulseScale")?.floatValue ?? -1f,
                    0.42f);
            report.AppendLine($"- Station boss-death camera envelope: {(exactCamera ? "exact" : "invalid")}");
            report.AppendLine($"- Station boss-death VFX/audio cue: {(exactVisual ? "exact" : "invalid")}");
            if (!exactCamera || !exactVisual)
            {
                report.AddIssue(
                    $"{expectation.ScenePath}: Station boss-death camera and VFX/audio authoring drifted from the reviewed contract.");
            }
        }

        private static void CheckStationBossTerminalFinisherCamera(
            SceneExpectation expectation,
            OlympusStationBossTerminalAftermathPresenter aftermath,
            CombatHealth bossHealth,
            BossBarrageCameraCueDriver cameraCueDriver,
            ReportBuilder report)
        {
            OlympusStationBossTerminalFinisherCameraController[] controllers =
                Object.FindObjectsByType<OlympusStationBossTerminalFinisherCameraController>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None);
            report.AppendLine(
                $"- {nameof(OlympusStationBossTerminalFinisherCameraController)} count: {controllers.Length}");
            if (controllers.Length != 1)
            {
                report.AddIssue(
                    $"{expectation.ScenePath}: expected exactly one authored Station boss-terminal finisher camera, found {controllers.Length}.");
                return;
            }

            OlympusStationBossTerminalFinisherCameraController controller = controllers[0];
            PlayableDirector director = controller.FinisherDirector;
            TimelineAsset timeline = controller.FinisherTimeline;
            Camera gameplayCamera = controller.GameplayCamera;
            Camera finisherCamera = controller.FinisherCamera;
            Animator animator = controller.GetComponent<Animator>();
            bool exactOwner = aftermath.FinisherCameraController == controller
                && bossHealth != null
                && controller.gameObject.name
                    == OlympusContinuousStageSetup.StationBossTerminalFinisherCameraRigName
                && controller.transform.parent == bossHealth.transform
                && animator != null
                && animator.enabled
                && animator.runtimeAnimatorController == null
                && animator.cullingMode == AnimatorCullingMode.AlwaysAnimate
                && animator.updateMode == AnimatorUpdateMode.UnscaledTime;

            bool exactCameraComponents = gameplayCamera != null
                && finisherCamera != null
                && cameraCueDriver.CameraController != null
                && gameplayCamera == cameraCueDriver.CameraController.GetComponent<Camera>()
                && gameplayCamera != finisherCamera
                && gameplayCamera.gameObject.activeInHierarchy
                && finisherCamera.gameObject.activeInHierarchy
                && gameplayCamera.enabled
                && !finisherCamera.enabled
                && finisherCamera.transform == controller.transform
                && Approximately(
                    finisherCamera.fieldOfView,
                    OlympusContinuousStageSetup.StationBossTerminalFinisherFieldOfView)
                && finisherCamera.GetComponentsInChildren<AudioListener>(true).Length == 0;
            UniversalAdditionalCameraData cameraData = finisherCamera != null
                ? finisherCamera.GetComponent<UniversalAdditionalCameraData>()
                : null;
            exactCameraComponents = exactCameraComponents
                && cameraData != null
                && cameraData.renderPostProcessing
                && cameraData.antialiasing
                    == AntialiasingMode.SubpixelMorphologicalAntiAliasing
                && cameraData.antialiasingQuality == AntialiasingQuality.High;

            bool exactTimeline = director != null
                && timeline != null
                && director.playableAsset == timeline
                && !director.playOnAwake
                && director.extrapolationMode == DirectorWrapMode.Hold
                && director.timeUpdateMode == DirectorUpdateMode.Manual
                && director.state != PlayState.Playing
                && timeline.durationMode == TimelineAsset.DurationMode.FixedLength
                && System.Math.Abs(
                    timeline.fixedDuration
                        - OlympusStationBossTerminalFinisherCameraController
                            .RequiredTimelineDurationSeconds) <= 0.0001d
                && System.Math.Abs(
                    timeline.duration
                        - OlympusStationBossTerminalFinisherCameraController
                            .RequiredTimelineDurationSeconds) <= 0.0001d
                && System.Math.Abs(timeline.editorSettings.frameRate - 60d) <= 0.0001d
                && AssetDatabase.GetAssetPath(timeline)
                    == OlympusContinuousStageSetup
                        .StationBossTerminalFinisherTimelinePath
                && Approximately(
                    controller.ResultCoverReleaseSeconds,
                    OlympusStationBossTerminalFinisherCameraController
                        .RequiredResultCoverReleaseSeconds);

            var rootTracks = timeline != null
                ? new List<TrackAsset>(timeline.GetRootTracks())
                : new List<TrackAsset>();
            AnimationTrack cameraTrack = rootTracks.Count == 1
                ? rootTracks[0] as AnimationTrack
                : null;
            var clips = cameraTrack != null
                ? new List<TimelineClip>(cameraTrack.GetClips())
                : new List<TimelineClip>();
            TimelineClip timelineClip = clips.Count == 1 ? clips[0] : null;
            AnimationPlayableAsset playable = timelineClip?.asset as AnimationPlayableAsset;
            AnimationClip cameraClip = playable?.clip;
            exactTimeline = exactTimeline
                && cameraTrack != null
                && cameraTrack.trackOffset == TrackOffset.Auto
                && director.GetGenericBinding(cameraTrack) == animator
                && timelineClip != null
                && System.Math.Abs(timelineClip.start) <= 0.0001d
                && System.Math.Abs(timelineClip.clipIn) <= 0.0001d
                && System.Math.Abs(
                    timelineClip.duration
                        - OlympusStationBossTerminalFinisherCameraController
                            .RequiredTimelineDurationSeconds) <= 0.0001d
                && System.Math.Abs(timelineClip.timeScale - 1d) <= 0.0001d
                && timelineClip.preExtrapolationMode
                    == TimelineClip.ClipExtrapolation.None
                && timelineClip.postExtrapolationMode
                    == TimelineClip.ClipExtrapolation.Hold
                && playable != null
                && !playable.removeStartOffset
                && !playable.applyFootIK
                && playable.loop == AnimationPlayableAsset.LoopMode.Off
                && !playable.useTrackMatchFields
                && cameraClip != null
                && AssetDatabase.GetAssetPath(cameraClip)
                    == OlympusContinuousStageSetup
                        .StationBossTerminalFinisherCameraClipPath
                && Approximately(cameraClip.frameRate, 60f)
                && Approximately(
                    cameraClip.length,
                    OlympusContinuousStageSetup
                        .StationBossTerminalFinisherDurationSeconds)
                && HasExactStationFinisherCameraCurves(cameraClip);

            Quaternion expectedStartRotation = Quaternion.LookRotation(
                OlympusContinuousStageSetup.StationBossTerminalFinisherLookTarget
                    - OlympusContinuousStageSetup
                        .StationBossTerminalFinisherStartLocalPosition,
                Vector3.up);
            bool exactInitialPose = controller.transform.localPosition
                    == OlympusContinuousStageSetup
                        .StationBossTerminalFinisherStartLocalPosition
                && Quaternion.Angle(
                    controller.transform.localRotation,
                    expectedStartRotation) <= 0.01f;
            bool configurationValid = controller.ValidateConfiguration(
                out string configurationError);

            report.AppendLine(
                $"- Station finisher stable boss-root owner: {(exactOwner ? "exact" : "invalid")}");
            report.AppendLine(
                $"- Station finisher exclusive Camera/FOV46/no-listener: {(exactCameraComponents ? "exact" : "invalid")}");
            report.AppendLine(
                $"- Station finisher manual Timeline 2.6s + terminal hold: {(exactTimeline ? "exact" : "invalid")}");
            report.AppendLine(
                $"- Station finisher authored t=0 pose: {(exactInitialPose ? "exact" : "invalid")}");
            report.AppendLine(
                $"- Station finisher controller configuration: {(configurationValid ? "exact" : configurationError)}");
            if (!exactOwner
                || !exactCameraComponents
                || !exactTimeline
                || !exactInitialPose
                || !configurationValid)
            {
                report.AddIssue(
                    $"{expectation.ScenePath}: Station boss-terminal finisher must retain its stable boss-root local rig, exact FOV46 Camera cut, 2.6s manually sampled Timeline with terminal hold, zero AudioListeners, and 0.46s result-cover release.");
            }
        }

        private static bool HasExactStationFinisherCameraCurves(AnimationClip clip)
        {
            EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(clip);
            string[] expectedProperties =
            {
                "m_LocalPosition.x",
                "m_LocalPosition.y",
                "m_LocalPosition.z",
                "m_LocalRotation.x",
                "m_LocalRotation.y",
                "m_LocalRotation.z",
                "m_LocalRotation.w",
            };
            if (bindings.Length != expectedProperties.Length
                || bindings.Any(binding => binding.type != typeof(Transform)
                    || !string.IsNullOrEmpty(binding.path))
                || !bindings.Select(binding => binding.propertyName)
                    .OrderBy(value => value, System.StringComparer.Ordinal)
                    .SequenceEqual(expectedProperties.OrderBy(
                        value => value,
                        System.StringComparer.Ordinal)))
            {
                return false;
            }

            Vector3 start = OlympusContinuousStageSetup
                .StationBossTerminalFinisherStartLocalPosition;
            Vector3 settle = OlympusContinuousStageSetup
                .StationBossTerminalFinisherSettleLocalPosition;
            Quaternion startRotation = Quaternion.LookRotation(
                OlympusContinuousStageSetup.StationBossTerminalFinisherLookTarget - start,
                Vector3.up);
            Quaternion settleRotation = Quaternion.LookRotation(
                OlympusContinuousStageSetup.StationBossTerminalFinisherLookTarget - settle,
                Vector3.up);
            return HasExactStationFinisherCurve(
                    clip, "m_LocalPosition.x", start.x, settle.x)
                && HasExactStationFinisherCurve(
                    clip, "m_LocalPosition.y", start.y, settle.y)
                && HasExactStationFinisherCurve(
                    clip, "m_LocalPosition.z", start.z, settle.z)
                && HasExactStationFinisherCurve(
                    clip, "m_LocalRotation.x", startRotation.x, settleRotation.x)
                && HasExactStationFinisherCurve(
                    clip, "m_LocalRotation.y", startRotation.y, settleRotation.y)
                && HasExactStationFinisherCurve(
                    clip, "m_LocalRotation.z", startRotation.z, settleRotation.z)
                && HasExactStationFinisherCurve(
                    clip, "m_LocalRotation.w", startRotation.w, settleRotation.w);
        }

        private static bool HasExactStationFinisherCurve(
            AnimationClip clip,
            string propertyName,
            float startValue,
            float settleValue)
        {
            AnimationCurve curve = AnimationUtility.GetEditorCurve(
                clip,
                EditorCurveBinding.FloatCurve(
                    string.Empty,
                    typeof(Transform),
                    propertyName));
            if (curve == null || curve.length != 3)
            {
                return false;
            }

            Keyframe[] keys = curve.keys;
            return Approximately(keys[0].time, 0f)
                && Approximately(keys[0].value, startValue)
                && Approximately(
                    keys[1].time,
                    OlympusContinuousStageSetup
                        .StationBossTerminalFinisherSettleSeconds)
                && Approximately(keys[1].value, settleValue)
                && Approximately(
                    keys[2].time,
                    OlympusContinuousStageSetup
                        .StationBossTerminalFinisherDurationSeconds)
                && Approximately(keys[2].value, settleValue);
        }

        private static bool Approximately(float left, float right)
        {
            return Mathf.Abs(left - right) <= 0.0001f;
        }

        private static void CheckCanonicalBossVisual(SceneExpectation expectation, ReportBuilder report)
        {
            Transform bossRoot = FindSceneTransform(SceneManager.GetActiveScene(), CanonicalBossRootName);
            if (bossRoot == null)
            {
                report.AddIssue($"{expectation.ScenePath}: missing canonical boss root '{CanonicalBossRootName}'.");
                return;
            }

            Transform visual = bossRoot.Find(CanonicalBossVisualName);
            report.AppendLine($"- Canonical boss visual: {(visual != null ? CanonicalBossVisualName : "missing")}");
            if (visual == null)
            {
                report.AddIssue(
                    $"{expectation.ScenePath}: canonical boss must use '{CanonicalBossVisualName}'.");
                return;
            }

            string prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(visual.gameObject);
            report.AppendLine($"- Canonical boss visual prefab: {prefabPath}");
            if (prefabPath != CanonicalBossVisualPrefabPath)
            {
                report.AddIssue(
                    $"{expectation.ScenePath}: canonical boss visual source must be {CanonicalBossVisualPrefabPath}, found {prefabPath}.");
            }

            Animator animator = visual.GetComponentInChildren<Animator>(includeInactive: true);
            string controllerPath = animator != null && animator.runtimeAnimatorController != null
                ? AssetDatabase.GetAssetPath(animator.runtimeAnimatorController)
                : string.Empty;
            report.AppendLine($"- Canonical boss Animator controller: {controllerPath}");
            if (animator == null || controllerPath != CanonicalBossAnimatorControllerPath)
            {
                report.AddIssue(
                    $"{expectation.ScenePath}: canonical boss visual must keep the authored SciFiSoldier01 Commando Animator controller.");
                return;
            }

            BossBarrageVisualCueDriver cueDriver = bossRoot.GetComponent<BossBarrageVisualCueDriver>();
            SerializedProperty boundAnimator = cueDriver != null
                ? new SerializedObject(cueDriver).FindProperty("animator")
                : null;
            if (boundAnimator == null || boundAnimator.objectReferenceValue != animator)
            {
                report.AddIssue(
                    $"{expectation.ScenePath}: boss visual cue driver must target the canonical Commando Animator.");
            }

            BossBarragePocketVfxCueBridge[] pocketVfxBridges =
                Object.FindObjectsByType<BossBarragePocketVfxCueBridge>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None);
            BossBarragePocketVfxCueBridge pocketVfxBridge = null;
            int scenePocketVfxBridgeCount = 0;
            Scene activeScene = SceneManager.GetActiveScene();
            for (int i = 0; i < pocketVfxBridges.Length; i++)
            {
                BossBarragePocketVfxCueBridge candidate = pocketVfxBridges[i];
                if (candidate == null || candidate.gameObject.scene.handle != activeScene.handle)
                {
                    continue;
                }

                pocketVfxBridge = candidate;
                scenePocketVfxBridgeCount++;
            }

            report.AppendLine($"- Boss follow-up VFX bridge count: {scenePocketVfxBridgeCount}");
            if (scenePocketVfxBridgeCount != 1)
            {
                report.AddIssue(
                    $"{expectation.ScenePath}: expected exactly one boss follow-up VFX bridge, found {scenePocketVfxBridgeCount}.");
            }
            else if (cueDriver == null || pocketVfxBridge.BossVisualCueDriver != cueDriver)
            {
                report.AddIssue(
                    $"{expectation.ScenePath}: boss follow-up VFX bridge must target the canonical boss visual cue driver.");
            }

            CombatHealth encounterBossHealth = null;
            if (pocketVfxBridge != null && pocketVfxBridge.EncounterController != null)
            {
                encounterBossHealth = new SerializedObject(pocketVfxBridge.EncounterController)
                    .FindProperty("bossHealth")
                    .objectReferenceValue as CombatHealth;
            }

            report.AppendLine(
                $"- Boss follow-up reaction owner: {(encounterBossHealth != null ? encounterBossHealth.name : "missing")}");
            if (encounterBossHealth == null || encounterBossHealth.transform != bossRoot)
            {
                report.AddIssue(
                    $"{expectation.ScenePath}: follow-up encounter health and visual reaction must share the canonical boss root.");
            }

            try
            {
                ActionFoundationSciFiSoldier01VisualSetup.ValidateCanonicalCommandoArsenal(visual.gameObject);
                report.AppendLine("- Canonical boss Commando arsenal: complete");
            }
            catch (System.InvalidOperationException exception)
            {
                report.AddIssue(
                    $"{expectation.ScenePath}: canonical boss Commando arsenal is incomplete: {exception.Message}");
            }

            if (visual.GetComponentInChildren<CombatHealth>(includeInactive: true) != null)
            {
                report.AddIssue(
                    $"{expectation.ScenePath}: canonical boss visual prefab must not duplicate gameplay health ownership.");
            }

            Renderer[] renderers = visual.GetComponentsInChildren<Renderer>(includeInactive: true);
            int enabledRendererCount = 0;
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null && renderers[i].enabled && !renderers[i].forceRenderingOff)
                {
                    enabledRendererCount++;
                }
            }

            report.AppendLine($"- Canonical boss enabled renderers: {enabledRendererCount}/{renderers.Length}");
            if (enabledRendererCount == 0)
            {
                report.AddIssue($"{expectation.ScenePath}: canonical Commando boss visual has no enabled renderers.");
            }
        }

        private static Transform FindSceneTransform(Scene scene, string objectName)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
            {
                Transform[] transforms = roots[rootIndex].GetComponentsInChildren<Transform>(includeInactive: true);
                for (int transformIndex = 0; transformIndex < transforms.Length; transformIndex++)
                {
                    if (transforms[transformIndex].name == objectName)
                    {
                        return transforms[transformIndex];
                    }
                }
            }

            return null;
        }

        private static void RequireSingle<T>(SceneExpectation expectation, ReportBuilder report, string label = null)
            where T : Object
        {
            T[] instances = Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            string resolvedLabel = label ?? typeof(T).Name;
            report.AppendLine($"- {resolvedLabel} count: {instances.Length}");
            if (instances.Length != 1)
            {
                report.AddIssue($"{expectation.ScenePath}: expected exactly one {resolvedLabel}, found {instances.Length}.");
            }
        }

        private static void WriteReport(ReportBuilder report)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath));
            File.WriteAllText(ReportPath, report.ToString(), Encoding.UTF8);
        }

        private readonly struct SceneExpectation
        {
            public SceneExpectation(string scenePath, SceneContractKind contractKind)
            {
                ScenePath = scenePath;
                ContractKind = contractKind;
            }

            public string ScenePath { get; }
            public SceneContractKind ContractKind { get; }
        }

        private enum SceneContractKind
        {
            CanonicalCombat
        }

        private sealed class ReportBuilder
        {
            private readonly StringBuilder builder = new();
            private readonly List<string> issues = new();

            public int InspectedSceneCount { get; set; }
            public int CheckedContractCount { get; set; }
            public bool Passed => InspectedSceneCount > 0 && CheckedContractCount > 0 && issues.Count == 0;

            public ReportBuilder()
            {
                builder.AppendLine("# Runtime Scene Wiring Readiness Report");
                builder.AppendLine();
                builder.AppendLine("Authority: read-only reporter.");
                builder.AppendLine("Not proved: this edit-mode inspection does not prove full PlayMode behavior, input feel, HUD animation, or actual mobile touch flow.");
                builder.AppendLine();
            }

            public void AppendLine(string value = "")
            {
                builder.AppendLine(value);
            }

            public void AddIssue(string issue)
            {
                issues.Add(issue);
            }

            public void AppendSummary()
            {
                builder.AppendLine("## Summary");
                builder.AppendLine();
                builder.AppendLine($"- inspected scenes: {InspectedSceneCount}");
                builder.AppendLine($"- checked contracts: {CheckedContractCount}");
                builder.AppendLine($"- unexpected issues: {issues.Count}");
                builder.AppendLine($"- result: {(Passed ? "PASS" : "FAIL")}");
                builder.AppendLine();

                if (issues.Count == 0)
                {
                    builder.AppendLine("No unexpected scene-wiring issues were detected.");
                    return;
                }

                builder.AppendLine("Issues:");
                for (int i = 0; i < issues.Count; i++)
                {
                    builder.Append("- ");
                    builder.AppendLine(issues[i]);
                }
            }

            public override string ToString()
            {
                return builder.ToString();
            }
        }
    }
}
