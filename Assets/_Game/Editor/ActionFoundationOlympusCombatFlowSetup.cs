using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using DimensionBrawl.AI;
using DimensionBrawl.Combat;
using DimensionBrawl.Enemies;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.Player;
using DimensionBrawl.Presentation;
using DimensionBrawl.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;

namespace DimensionBrawl.Editor
{
    public static class ActionFoundationOlympusCombatFlowSetup
    {
        private const string StageScenePath = "Assets/_Game/Scenes/OlympusCorridorInvasionStage.unity";
        private const string SourceCombatScenePath = ActionFoundationBossBarrageLaneReviewSetup.ReviewScenePath;
        private const string CombatStartAnchorName = "Gameplay_CombatStartAnchor";
        private const string TimelineDirectorName = "IntroGatePodReview_TimelineDirector";
        private const string PlayerRevealHandoffCameraName = "CM_03_src_c10_player_reveal_rina_quest_start";
        private const string RevealShotId = "src_c10_player_reveal_rina_quest_start";
        private const string RevealAnimationTrackName = "Player Reveal Rina Camera Motion";
        private const string RevealHandoffMatchClipName = "AC_OlympusIntro_RevealHandoffCombatMatch";
        private const string RevealHandoffMatchClipPath =
            "Assets/_Game/DesignData/Timelines/Cinematics/AC_OlympusIntro_RevealHandoffCombatMatch.anim";
        private const string CombatStartVisualActivationTrackName = "Combat Start Inori Active";
        private const string CombatStartVisualBodyTrackName = "Combat Start Inori Body";
        private const string FirstPersonResidualActivationTrackName = "First Person Inori Residual Active";
        private const string FlowRootName = "OlympusCorridor_CombatFlowRoot";
        private const string CombatPackageRootName = "OlympusCorridor_BossBarrageCombatPackage";
        private const string IntroSwordGateRootName = "OlympusCorridor_IntroSwordGate";
        private const string InvasionBridgeRootName = "IntroGatePodReview_InvasionBridge";
        private const string StairTriggerName = "OlympusCorridor_StairToCorridorCombatTrigger";
        private const string StairBlockerName = "OlympusCorridor_IntroSwordGate_StairBlocker";
        private const string CorridorBoundsRootName = "OlympusCorridor_CorridorCombatBounds";
        private const string CombatCameraName = "OlympusCorridor_Combat_MainCamera";
        private const float OlympusCorridorGameplayYawDegrees = 90f;

        private const string SourcePlayerRootName = "Player_CombatGirl_ActionFoundation";
        private const string SourceMainCameraRootName = "Main Camera";
        private const string SourceHudRootName = "BossBarrageLaneReview_DebugHud";
        private const string SourceCombatHudCanvasRootName = "BossBarrageLaneReview_CombatHudCanvas";
        private const string SourceCombatHudEventSystemRootName = "BossBarrageLaneReview_CombatHudEventSystem";
        private const string SourceCombatVfxRootName = "ActionFoundation_CombatVfxPool";
        private const string SourceArenaVfxRootName = "ActionFoundation_ArenaVfx";
        private const string SourceArenaGridRootName = "ActionFoundation_ArenaGrid";
        private const string SourceLaneRootName = "BossBarrageLaneReview_SummonLaneSpace";
        private const string SourceProjectilePoolRootName = "BossBarrageLaneReview_ProjectilePool";
        private const string SourceActionCuePoolRootName = "BossBarrageLaneReview_ActionCuePool";
        private const string SourceSummonActorPoolRootName = "BossBarrageLaneReview_SummonActorPool";
        private const string SourceBossSummonActorPoolRootName = "BossBarrageLaneReview_BossSummonActorPool";
        private const string SourceBossRootName = "BossBarrageLaneReview_BossProxy_NeedleLock";
        private const string SourceCloseThreatRootName = "BossBarrageLaneReview_CloseThreat_ClosePunish";
        private const string SourcePocketOwnerRootName = "BossBarrageLaneReview_PocketOwner";
        private const string SourceMarkersRootName = "BossBarrageLaneReview_Markers";
        private const string SourceAmbientVfxRootName = "BossBarrageLaneReview_AmbientVfx";
        private const string SourceTelegraphRootName = "BossBarrageLaneReview_BossBarrageTelegraphMarkers";
        private const string IntroGatePodRuntimePayloadRootName = "IntroGatePodPortPayload_CutsceneRuntime";
        private const string CombatStartVisualPlacementName = "IntroGatePodReview_CombatStartInoriPlacement";
        private const string FirstPersonResidualVisualRootName = "IntroGatePodReview_InoriPlacement";
        private const string CutsceneCommandoIdleStateName = "IdleAim";
        private const string ValidationReportPath = "C:/tmp/DimensionBrawl-OlympusCombatFlow-Validation.txt";
        private const float HudRevealDelaySeconds = 0.08f;
        private const float HudRevealDurationSeconds = 0.18f;
        private const float CharacterGroundClearance = 0.015f;
        private const float CharacterGroundSnapTolerance = 0.005f;
        private const float CommandoStrideBobHeight = 0f;

        private static readonly Vector3 SourcePlayerStartPosition = new Vector3(0f, 0f, -8.5f);
        private static readonly Vector3 CenteredCombatCameraOffset = new Vector3(0f, 0.68f, -4.25f);
        private static readonly Vector3 CenteredCombatLookOffset = new Vector3(0f, 1.18f, 1.5f);
        private static readonly Vector3 CenteredCombatAimCameraOffset = new Vector3(0f, 0.18f, 0.12f);
        private static readonly Vector3 CenteredCombatAimFocusOffset = new Vector3(0f, 0.06f, 1.05f);
        private static readonly Vector3 CenteredCombatCameraLocalPosition =
            CenteredCombatLookOffset + CenteredCombatCameraOffset;
        private static readonly Vector3 CenteredCombatFocusLocalPosition =
            CenteredCombatLookOffset;
        private static readonly Vector3[] IntroSwordEnemyCombatSlotLocalPositions =
        {
            new Vector3(-1.65f, 0f, 3.2f),
            new Vector3(0f, 0f, 3.7f),
            new Vector3(1.65f, 0f, 3.2f)
        };

        private static readonly float[] IntroSwordCommandoMoveDurations =
        {
            4.35f,
            4.37f,
            4.33f
        };

        private static readonly string[] CutsceneCommandoNames =
        {
            "IntroGatePodReview_Commando_01",
            "IntroGatePodReview_Commando_02",
            "IntroGatePodReview_Commando_03"
        };

        private static readonly string[] PlayerShieldWeaponNames =
        {
            "Weapon_Round_Shield",
            "Weapon_Shiled"
        };

        [MenuItem("DimensionBrawl/Apply Olympus Corridor Combat Flow")]
        public static void ApplyOlympusCorridorCombatFlowMenu()
        {
            ApplyOlympusCorridorCombatFlow();
            Debug.Log("Applied Olympus corridor combat flow.");
        }

        public static void RunBatchApplyOlympusCorridorCombatFlow()
        {
            ApplyOlympusCorridorCombatFlow();
        }

        public static void RunBatchValidateOlympusCorridorCombatFlow()
        {
            bool waitingStopStartsHandoff = ValidateIntroDirectorStoppedFromWaitingStartsHandoff();
            Scene stageScene = EditorSceneManager.OpenScene(StageScenePath, OpenSceneMode.Single);
            GameObject packageRoot = RequireObjectInScene(stageScene, CombatPackageRootName);
            GameObject flowRoot = RequireObjectInScene(stageScene, FlowRootName);
            PlayableDirector introDirector = FindObjectByName<PlayableDirector>(stageScene, TimelineDirectorName);
            var report = new List<string>
            {
                "Olympus corridor combat flow validation",
                $"Scene: {stageScene.path}",
                $"FlowRoot activeSelf={flowRoot.activeSelf} activeInHierarchy={flowRoot.activeInHierarchy}",
                $"Package activeSelf={packageRoot.activeSelf} activeInHierarchy={packageRoot.activeInHierarchy}",
                $"Director stopped while waiting starts handoff={waitingStopStartsHandoff}"
            };
            GameObject playerRoot = RequireChildObject(packageRoot.transform, SourcePlayerRootName);
            bool authoringPlayerRootInactive =
                !playerRoot.activeSelf && !playerRoot.activeInHierarchy;

            int unexpectedActiveDirectChildren = 0;
            report.Add("Direct package children:");
            for (int i = 0; i < packageRoot.transform.childCount; i++)
            {
                Transform child = packageRoot.transform.GetChild(i);
                bool allowedActive = string.Equals(child.name, StairTriggerName, StringComparison.Ordinal)
                    || string.Equals(child.name, StairBlockerName, StringComparison.Ordinal);
                if (child.gameObject.activeInHierarchy && !allowedActive)
                {
                    unexpectedActiveDirectChildren++;
                }

                report.Add(
                    $"  {child.name} activeSelf={child.gameObject.activeSelf} activeInHierarchy={child.gameObject.activeInHierarchy}");
            }

            int activeRendererCount = 0;
            Renderer[] renderers = packageRoot.GetComponentsInChildren<Renderer>(includeInactive: true);
            report.Add("Active renderers under package:");
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy)
                {
                    continue;
                }

                if (renderer.transform.IsChildOf(playerRoot.transform))
                {
                    continue;
                }

                activeRendererCount++;
                report.Add($"  {renderer.gameObject.name}");
            }

            if (activeRendererCount == 0)
            {
                report.Add("  <none>");
            }

            report.Add($"Unexpected active direct children: {unexpectedActiveDirectChildren}");
            report.Add($"Active non-player renderer count: {activeRendererCount}");
            report.Add($"Player root inactive at scene start={authoringPlayerRootInactive}");

            OlympusCorridorCombatFlowController flowController =
                RequireComponent<OlympusCorridorCombatFlowController>(flowRoot, "Olympus corridor combat flow controller");
            GameObject hudRoot = RequireChildObject(packageRoot.transform, SourceHudRootName);
            BossBarrageLaneReviewHud reviewHud =
                RequireComponent<BossBarrageLaneReviewHud>(hudRoot, "Olympus validation review HUD");
            BossBarrageLaneReviewMobileHud mobileHud =
                RequireComponent<BossBarrageLaneReviewMobileHud>(hudRoot, "Olympus validation mobile HUD");
            GameObject combatCameraRoot = RequireChildObject(packageRoot.transform, CombatCameraName);
            PlayerCombatModeController combatModeController =
                RequireComponent<PlayerCombatModeController>(playerRoot, "combat mode controller");
            GameObject introSwordGateRoot = RequireChildObject(packageRoot.transform, IntroSwordGateRootName);
            GameObject canonicalEncounterRoot = RequireChildObject(packageRoot.transform, "ActionFoundationTestEncounter");
            GameObject corridorBoundsRoot = RequireChildObject(packageRoot.transform, CorridorBoundsRootName);
            GameObject runtimePayloadRoot = FindRootOrDescendant(stageScene, IntroGatePodRuntimePayloadRootName);
            GameObject firstPersonResidualRoot = FindRootOrDescendant(stageScene, FirstPersonResidualVisualRootName);
            GameObject arenaVfxRoot = FindDirectChildObject(packageRoot.transform, SourceArenaVfxRootName);
            GameObject arenaGridRoot = FindDirectChildObject(packageRoot.transform, SourceArenaGridRootName);
            IntroGatePodInvasionBridgeCue invasionBridge =
                FindObjectByName<IntroGatePodInvasionBridgeCue>(stageScene, InvasionBridgeRootName);
            GameObject[] cutsceneCommandos = RequireCutsceneCommandos(stageScene);
            int obsoleteIntroEnemyCount = CountObjectsWithNames(
                stageScene,
                "OlympusCorridor_IntroSwordGate_Left_CloseGuard",
                "OlympusCorridor_IntroSwordGate_Center_EntryProbe",
                "OlympusCorridor_IntroSwordGate_Right_LungeChaser");
            TimelineAsset timeline = introDirector != null ? introDirector.playableAsset as TimelineAsset : null;
            double directorDuration = introDirector != null && !double.IsInfinity(introDirector.duration)
                ? introDirector.duration
                : (timeline != null ? timeline.duration : 0d);
            double revealClipEnd = FindClipEnd(timeline, RevealShotId, -1d);
            double revealAnimationClipEnd = FindTrackClipEnd(timeline, RevealAnimationTrackName, -1d);
            FinalHandoffBindingSnapshot finalHandoffBinding = CaptureFinalHandoffBinding(
                stageScene,
                introDirector,
                timeline,
                playerRoot);
            double introHandoffSeconds = ResolveIntroHandoffSeconds(introDirector);

            SampleIntroHandoffMoment(introDirector, invasionBridge, introHandoffSeconds);

            Camera activeIntroCameraBeforeHandoff =
                FindActiveGameplayHandoffIntroCamera(stageScene, packageRoot.transform);
            CameraPresentationSnapshot introCameraPresentationBeforeHandoff =
                CaptureCameraPresentation(activeIntroCameraBeforeHandoff);
            GameObject[] environmentRendererExclusions =
                BuildEnvironmentRendererExclusions(packageRoot, firstPersonResidualRoot, cutsceneCommandos);
            HashSet<string> environmentRenderersBeforeHandoff =
                CaptureActiveSceneRendererPathSet(stageScene, environmentRendererExclusions);
            Vector3[] cutsceneCommandoPositionsBeforeHandoff = CaptureWorldPositions(cutsceneCommandos);
            CommandoTempoSnapshot commandoTempoSnapshot =
                CaptureCommandoTempoSnapshot(invasionBridge);
            CommandoGroundingSnapshot commandoGroundingSnapshot =
                CaptureCommandoGroundingSnapshot(invasionBridge, packageRoot.transform);
            PlayerGroundSnapshot playerGroundBeforeHandoff =
                CapturePlayerGroundSnapshot(playerRoot, introSwordGateRoot.transform);
            Vector3 firstPersonResidualPositionBefore =
                firstPersonResidualRoot != null ? firstPersonResidualRoot.transform.position : default;
            Quaternion firstPersonResidualRotationBefore =
                firstPersonResidualRoot != null ? firstPersonResidualRoot.transform.rotation : default;
            bool firstPersonResidualHiddenBeforeHandoff =
                firstPersonResidualRoot == null || !firstPersonResidualRoot.activeInHierarchy;
            bool playerRootActiveBeforeHandoff =
                playerRoot.activeSelf && playerRoot.activeInHierarchy;

            AppendCameraDiagnostics(
                report,
                "Pre-handoff sampled camera diagnostics",
                stageScene,
                packageRoot.transform,
                playerRoot);

            InvokePrivate(flowController, "BeginIntroSwordGate");

            Camera combatCamera = RequireComponent<Camera>(combatCameraRoot, "Olympus corridor combat camera");
            CameraPhaseSnapshot introCameraSnapshot = CaptureCameraPhase(
                stageScene,
                packageRoot.transform,
                combatCamera,
                playerRoot);
            ActionCameraPosePrediction introCameraPrediction = CaptureActionCameraPosePrediction(
                combatCamera,
                packageRoot.transform,
                playerRoot);
            float packageYawDegrees = NormalizeYaw(packageRoot.transform.eulerAngles.y);
            float packageYawDeltaDegrees =
                Mathf.Abs(Mathf.DeltaAngle(packageYawDegrees, OlympusCorridorGameplayYawDegrees));
            bool handoffHudActive = hudRoot.activeInHierarchy;
            bool handoffCameraActive = combatCameraRoot.activeInHierarchy;
            bool handoffPlayerActive = playerRoot.activeInHierarchy;
            bool playerRootActiveAfterHandoff =
                playerRoot.activeSelf && playerRoot.activeInHierarchy;
            bool playerRootActiveAfterDirectorStop =
                SimulateIntroDirectorStopAfterHandoff(introDirector, flowController, playerRoot);
            bool handoffIntroGateActive = introSwordGateRoot.activeInHierarchy;
            bool handoffCanonicalEncounterInactive = !canonicalEncounterRoot.activeInHierarchy;
            bool handoffCorridorBoundsInactive = !corridorBoundsRoot.activeInHierarchy;
            bool handoffPayloadKeptActive =
                runtimePayloadRoot != null && runtimePayloadRoot.activeInHierarchy;
            bool firstPersonResidualHiddenAfterHandoff =
                firstPersonResidualRoot == null || !firstPersonResidualRoot.activeInHierarchy;
            bool firstPersonResidualTransformPreserved =
                firstPersonResidualRoot == null
                || (Vector3.Distance(firstPersonResidualRoot.transform.position, firstPersonResidualPositionBefore)
                    <= 0.001f
                    && Quaternion.Angle(firstPersonResidualRoot.transform.rotation, firstPersonResidualRotationBefore)
                    <= 0.05f);
            bool handoffArenaVfxInactive = arenaVfxRoot == null || !arenaVfxRoot.activeInHierarchy;
            bool handoffArenaGridInactive = arenaGridRoot == null || !arenaGridRoot.activeInHierarchy;
            bool invasionBridgeDisabledAfterHandoff = invasionBridge == null || !invasionBridge.enabled;
            bool handoffUsesCutsceneCommandos = AllObjectsNamed(cutsceneCommandos, CutsceneCommandoNames);
            bool handoffCutsceneCommandosActive = AllObjectsActiveInHierarchy(cutsceneCommandos);
            bool handoffCutsceneCommandosCombatEnabled =
                AllCutsceneCommandoCombatBehavioursEnabled(cutsceneCommandos);
            bool handoffCutsceneCommandosAtCombatSlots =
                CutsceneCommandosAtCombatSlots(cutsceneCommandos, packageRoot.transform, out string commandoSlotSummary);
            Vector3[] cutsceneCommandoPositionsAfterHandoff = CaptureWorldPositions(cutsceneCommandos);
            float maxCommandoHandoffWorldDelta = CalculateMaxWorldDelta(
                cutsceneCommandoPositionsBeforeHandoff,
                cutsceneCommandoPositionsAfterHandoff);
            bool handoffCutsceneCommandosNoVisiblePop =
                maxCommandoHandoffWorldDelta <= 0.12f;
            float minimumCommandoPlayerPlanarDistance =
                MinimumPlanarDistanceToPlayer(cutsceneCommandos, playerRoot.transform);
            bool handoffCutsceneCommandosNotOverlappingPlayer =
                minimumCommandoPlayerPlanarDistance >= 2.4f;
            bool obsoleteIntroEnemiesRemoved = obsoleteIntroEnemyCount == 0;
            CameraPresentationSnapshot combatCameraPresentationAfterHandoff =
                CaptureCameraPresentation(combatCamera);
            bool combatCameraPresentationMatchesIntro =
                CameraPresentationsMatch(
                    introCameraPresentationBeforeHandoff,
                    combatCameraPresentationAfterHandoff);
            HashSet<string> environmentRenderersAfterHandoff =
                CaptureActiveSceneRendererPathSet(stageScene, environmentRendererExclusions);
            bool handoffEnvironmentRenderersStable =
                RendererPathSetsEqual(environmentRenderersBeforeHandoff, environmentRenderersAfterHandoff);
            PlayerGroundSnapshot playerGroundAfterHandoff =
                CapturePlayerGroundSnapshot(playerRoot, introSwordGateRoot.transform);
            bool handoffPlayerNotSunk =
                playerGroundAfterHandoff.IsValid && playerGroundAfterHandoff.IsWithinGroundTolerance;
            bool handoffHudStartsHiddenForReveal =
                reviewHud.HudOpacity <= 0.001f && mobileHud.HudOpacity <= 0.001f;
            PlayerSwordOnlySnapshot swordOnlySnapshot =
                CapturePlayerSwordOnlySnapshot(playerRoot, combatModeController);
            bool combatPackageAlignedToCorridor = packageYawDeltaDegrees <= 0.25f;
            bool combatCameraKeepsSourcePose =
                Vector3.Distance(introCameraSnapshot.CombatCameraLocalPosition, CenteredCombatCameraLocalPosition)
                <= 0.05f;
            bool revealShotCoversHandoffTail = directorDuration <= 0d || revealClipEnd >= directorDuration - 0.06d;
            bool revealAnimationCoversOrHoldsTail =
                directorDuration <= 0d
                || revealAnimationClipEnd >= directorDuration - 0.06d
                || TrackHasPostExtrapolation(timeline, RevealAnimationTrackName, TimelineClip.ClipExtrapolation.Hold);

            report.Add("Final cutscene-to-gameplay actor binding:");
            report.Add($"  obsoleteCombatStartVisualRemoved={finalHandoffBinding.ObsoleteCombatStartVisualRemoved}");
            report.Add($"  bodyTrackFound={finalHandoffBinding.BodyTrackFound}");
            report.Add($"  bodyBinding={finalHandoffBinding.BodyBindingPath}");
            report.Add($"  bodyTrackBoundToCombatPlayer={finalHandoffBinding.BodyTrackBoundToCombatPlayer}");
            report.Add($"  activationTrackFound={finalHandoffBinding.ActivationTrackFound}");
            report.Add($"  activationBinding={finalHandoffBinding.ActivationBindingPath}");
            report.Add($"  activationTrackBoundToCombatPlayer={finalHandoffBinding.ActivationTrackBoundToCombatPlayer}");
            report.Add($"  activationPostPlaybackActive={finalHandoffBinding.ActivationPostPlaybackActive}");

            report.Add("Intro sword handoff sample:");
            report.Add($"  {SourceHudRootName} activeSelf={hudRoot.activeSelf} activeInHierarchy={hudRoot.activeInHierarchy}");
            report.Add($"  {CombatCameraName} activeSelf={combatCameraRoot.activeSelf} activeInHierarchy={combatCameraRoot.activeInHierarchy}");
            report.Add("  active enabled cameras:");
            for (int i = 0; i < introCameraSnapshot.ActiveEnabledCameras.Count; i++)
            {
                Camera activeCamera = introCameraSnapshot.ActiveEnabledCameras[i];
                report.Add($"    {GetHierarchyPath(activeCamera.transform)}");
            }

            if (introCameraSnapshot.ActiveEnabledCameras.Count == 0)
            {
                report.Add("    <none>");
            }

            report.Add($"  {SourcePlayerRootName} activeSelf={playerRoot.activeSelf} activeInHierarchy={playerRoot.activeInHierarchy}");
            report.Add($"  {SourcePlayerRootName} activeBeforeHandoff={playerRootActiveBeforeHandoff}");
            report.Add($"  {SourcePlayerRootName} activeAfterHandoff={playerRootActiveAfterHandoff}");
            report.Add($"  {SourcePlayerRootName} activeAfterDirectorStop={playerRootActiveAfterDirectorStop}");
            report.Add($"  {IntroSwordGateRootName} activeSelf={introSwordGateRoot.activeSelf} activeInHierarchy={introSwordGateRoot.activeInHierarchy}");
            report.Add($"  ActionFoundationTestEncounter activeInHierarchy={canonicalEncounterRoot.activeInHierarchy}");
            report.Add($"  {CorridorBoundsRootName} activeInHierarchy={corridorBoundsRoot.activeInHierarchy}");
            report.Add($"  {IntroGatePodRuntimePayloadRootName} activeInHierarchy={handoffPayloadKeptActive}");
            report.Add($"  {FirstPersonResidualVisualRootName} hiddenBeforeHandoff={firstPersonResidualHiddenBeforeHandoff}");
            report.Add($"  {FirstPersonResidualVisualRootName} hiddenAfterHandoff={firstPersonResidualHiddenAfterHandoff}");
            report.Add($"  {FirstPersonResidualVisualRootName} transformPreserved={firstPersonResidualTransformPreserved}");
            report.Add($"  {SourceArenaVfxRootName} inactiveAtHandoff={handoffArenaVfxInactive}");
            report.Add($"  {SourceArenaGridRootName} inactiveAtHandoff={handoffArenaGridInactive}");
            report.Add($"  preHandoffSampleTime={Math.Max(0d, introHandoffSeconds - 0.02d):0.###}");
            report.Add(
                $"  preHandoffActiveIntroCamera={FormatObjectReference(activeIntroCameraBeforeHandoff)}");
            report.Add(
                $"  cameraPresentationMatchesIntro={combatCameraPresentationMatchesIntro}");
            AppendCameraPresentationSnapshot(report, "introCameraBeforeHandoff", introCameraPresentationBeforeHandoff);
            AppendCameraPresentationSnapshot(report, CombatCameraName + "AfterHandoff", combatCameraPresentationAfterHandoff);
            report.Add(
                $"  environmentRendererContinuity={handoffEnvironmentRenderersStable} before={environmentRenderersBeforeHandoff.Count} after={environmentRenderersAfterHandoff.Count}");
            report.Add($"  hudRevealStartsAtZero={handoffHudStartsHiddenForReveal}");
            report.Add($"  {InvasionBridgeRootName} disabledAfterHandoff={invasionBridgeDisabledAfterHandoff}");
            report.Add($"  cutsceneCommandos={FormatObjectNames(cutsceneCommandos)}");
            report.Add($"  cutsceneCommandosActive={handoffCutsceneCommandosActive}");
            report.Add($"  cutsceneCommandosCombatEnabled={handoffCutsceneCommandosCombatEnabled}");
            report.Add($"  cutsceneCommandosAtCombatSlots={handoffCutsceneCommandosAtCombatSlots}");
            report.Add($"  cutsceneCommandosSlotSummary={commandoSlotSummary}");
            report.Add(
                $"  cutsceneCommandosMaxHandoffDelta={maxCommandoHandoffWorldDelta:0.###} noVisiblePop={handoffCutsceneCommandosNoVisiblePop}");
            report.Add(
                $"  cutsceneCommandosTempo valid={commandoTempoSnapshot.IsValid} maxMoveDuration={commandoTempoSnapshot.MaxMoveDurationSeconds:0.###} minMoveSpeed={commandoTempoSnapshot.MinMoveSpeed:0.###}");
            report.Add($"  cutsceneCommandosTempoSummary={commandoTempoSnapshot.Summary}");
            report.Add(
                $"  cutsceneCommandosGrounding valid={commandoGroundingSnapshot.IsValid} withinTolerance={commandoGroundingSnapshot.IsWithinTolerance} minRootY={commandoGroundingSnapshot.MinRootY:0.###} maxRootY={commandoGroundingSnapshot.MaxRootY:0.###} strideBobHeight={commandoGroundingSnapshot.StrideBobHeight:0.###}");
            report.Add($"  cutsceneCommandosGroundingSummary={commandoGroundingSnapshot.Summary}");
            report.Add($"  minCommandoPlayerPlanarDistance={minimumCommandoPlayerPlanarDistance:0.###}");
            report.Add($"  obsoleteTempIntroEnemiesRemoved={obsoleteIntroEnemiesRemoved}");
            AppendPlayerGroundSnapshot(report, "playerGroundBeforeHandoff", playerGroundBeforeHandoff);
            AppendPlayerGroundSnapshot(report, "playerGroundAfterHandoff", playerGroundAfterHandoff);
            AppendPlayerSwordOnlySnapshot(report, swordOnlySnapshot);
            report.Add($"  {CombatPackageRootName} yaw={packageYawDegrees:0.###} deltaFromCorridor={packageYawDeltaDegrees:0.###}");
            AppendCameraPhaseSnapshot(report, CombatCameraName, introCameraSnapshot);
            AppendActionCameraPosePredictionSummary(report, CombatCameraName, introCameraPrediction);
            AppendCameraDiagnostics(
                report,
                "Intro sword handoff camera diagnostics",
                stageScene,
                packageRoot.transform,
                playerRoot);

            InvokePrivate(flowController, "BeginCorridorCombat");

            CameraPhaseSnapshot corridorCameraSnapshot = CaptureCameraPhase(
                stageScene,
                packageRoot.transform,
                combatCamera,
                playerRoot);
            ActionCameraPosePrediction corridorCameraPrediction = CaptureActionCameraPosePrediction(
                combatCamera,
                packageRoot.transform,
                playerRoot);
            bool corridorHudActive = hudRoot.activeInHierarchy;
            bool corridorCameraActive = combatCameraRoot.activeInHierarchy;
            bool corridorPlayerActive = playerRoot.activeInHierarchy;
            bool corridorCanonicalEncounterActive = canonicalEncounterRoot.activeInHierarchy;
            bool corridorBoundsActive = corridorBoundsRoot.activeInHierarchy;
            bool corridorArenaVfxInactive = arenaVfxRoot == null || !arenaVfxRoot.activeInHierarchy;
            bool corridorArenaGridInactive = arenaGridRoot == null || !arenaGridRoot.activeInHierarchy;
            bool corridorCameraKeepsSourcePose =
                Vector3.Distance(corridorCameraSnapshot.CombatCameraLocalPosition, CenteredCombatCameraLocalPosition)
                <= 0.05f;

            report.Add("Corridor combat sample:");
            report.Add($"  {SourceHudRootName} activeSelf={hudRoot.activeSelf} activeInHierarchy={hudRoot.activeInHierarchy}");
            report.Add($"  {CombatCameraName} activeSelf={combatCameraRoot.activeSelf} activeInHierarchy={combatCameraRoot.activeInHierarchy}");
            report.Add("  active enabled cameras:");
            for (int i = 0; i < corridorCameraSnapshot.ActiveEnabledCameras.Count; i++)
            {
                Camera activeCamera = corridorCameraSnapshot.ActiveEnabledCameras[i];
                report.Add($"    {GetHierarchyPath(activeCamera.transform)}");
            }

            if (corridorCameraSnapshot.ActiveEnabledCameras.Count == 0)
            {
                report.Add("    <none>");
            }

            report.Add($"  {SourcePlayerRootName} activeSelf={playerRoot.activeSelf} activeInHierarchy={playerRoot.activeInHierarchy}");
            report.Add($"  {IntroSwordGateRootName} activeSelf={introSwordGateRoot.activeSelf} activeInHierarchy={introSwordGateRoot.activeInHierarchy}");
            report.Add($"  ActionFoundationTestEncounter activeInHierarchy={canonicalEncounterRoot.activeInHierarchy}");
            report.Add($"  {CorridorBoundsRootName} activeInHierarchy={corridorBoundsRoot.activeInHierarchy}");
            report.Add($"  {SourceArenaVfxRootName} inactiveInCorridor={corridorArenaVfxInactive}");
            report.Add($"  {SourceArenaGridRootName} inactiveInCorridor={corridorArenaGridInactive}");
            AppendCameraPhaseSnapshot(report, CombatCameraName, corridorCameraSnapshot);
            AppendActionCameraPosePredictionSummary(report, CombatCameraName, corridorCameraPrediction);
            AppendCameraDiagnostics(
                report,
                "Corridor combat camera diagnostics",
                stageScene,
                packageRoot.transform,
                playerRoot);

            report.Add("Timeline handoff coverage:");
            report.Add($"  Director duration={directorDuration:0.###}");
            report.Add($"  {RevealShotId} end={revealClipEnd:0.###}");
            report.Add($"  {RevealAnimationTrackName} end={revealAnimationClipEnd:0.###}");
            report.Add(
                $"  {RevealAnimationTrackName} postHold={TrackHasPostExtrapolation(timeline, RevealAnimationTrackName, TimelineClip.ClipExtrapolation.Hold)}");

            bool passed = unexpectedActiveDirectChildren == 0
                && activeRendererCount == 0
                && authoringPlayerRootInactive
                && waitingStopStartsHandoff
                && handoffHudActive
                && handoffCameraActive
                && introCameraSnapshot.OnlyCombatCameraEnabled
                && handoffPlayerActive
                && playerRootActiveBeforeHandoff
                && playerRootActiveAfterHandoff
                && playerRootActiveAfterDirectorStop
                && handoffIntroGateActive
                && handoffCanonicalEncounterInactive
                && handoffCorridorBoundsInactive
                && handoffPayloadKeptActive
                && firstPersonResidualHiddenBeforeHandoff
                && firstPersonResidualHiddenAfterHandoff
                && firstPersonResidualTransformPreserved
                && handoffArenaVfxInactive
                && handoffArenaGridInactive
                && combatCameraPresentationMatchesIntro
                && handoffEnvironmentRenderersStable
                && handoffHudStartsHiddenForReveal
                && invasionBridgeDisabledAfterHandoff
                && handoffUsesCutsceneCommandos
                && handoffCutsceneCommandosActive
                && handoffCutsceneCommandosCombatEnabled
                && handoffCutsceneCommandosAtCombatSlots
                && handoffCutsceneCommandosNoVisiblePop
                && commandoTempoSnapshot.IsValid
                && commandoTempoSnapshot.MaxMoveDurationSeconds <= 5.2f
                && commandoTempoSnapshot.MinMoveSpeed >= 0.75f
                && commandoGroundingSnapshot.IsValid
                && commandoGroundingSnapshot.IsWithinTolerance
                && handoffCutsceneCommandosNotOverlappingPlayer
                && playerGroundBeforeHandoff.IsValid
                && playerGroundBeforeHandoff.IsWithinGroundTolerance
                && handoffPlayerNotSunk
                && obsoleteIntroEnemiesRemoved
                && swordOnlySnapshot.IsValid
                && combatPackageAlignedToCorridor
                && introCameraSnapshot.CombatCameraCentersPlayer
                && introCameraSnapshot.CombatCameraCentersPlayerRenderer
                && introCameraPrediction.BaseCentersPlayer
                && introCameraPrediction.FullAimCentersPlayer
                && corridorHudActive
                && corridorCameraActive
                && corridorCameraSnapshot.OnlyCombatCameraEnabled
                && corridorPlayerActive
                && corridorCanonicalEncounterActive
                && corridorBoundsActive
                && corridorArenaVfxInactive
                && corridorArenaGridInactive
                && corridorCameraSnapshot.CombatCameraCentersPlayer
                && corridorCameraSnapshot.CombatCameraCentersPlayerRenderer
                && corridorCameraPrediction.BaseCentersPlayer
                && corridorCameraPrediction.FullAimCentersPlayer
                && finalHandoffBinding.ObsoleteCombatStartVisualRemoved
                && finalHandoffBinding.BodyTrackBoundToCombatPlayer
                && finalHandoffBinding.ActivationTrackBoundToCombatPlayer
                && finalHandoffBinding.ActivationPostPlaybackActive
                && revealShotCoversHandoffTail
                && revealAnimationCoversOrHoldsTail;
            report.Add(passed ? "RESULT: PASS" : "RESULT: FAIL");
            File.WriteAllLines(ValidationReportPath, report);
            if (!passed)
            {
                throw new InvalidOperationException(
                    $"Olympus corridor combat flow validation failed. See {ValidationReportPath}");
            }
        }

        private static bool ValidateIntroDirectorStoppedFromWaitingStartsHandoff()
        {
            Scene stageScene = EditorSceneManager.OpenScene(StageScenePath, OpenSceneMode.Single);
            GameObject packageRoot = RequireObjectInScene(stageScene, CombatPackageRootName);
            GameObject flowRoot = RequireObjectInScene(stageScene, FlowRootName);
            GameObject playerRoot = RequireChildObject(packageRoot.transform, SourcePlayerRootName);
            GameObject introSwordGateRoot = RequireChildObject(packageRoot.transform, IntroSwordGateRootName);
            OlympusCorridorCombatFlowController flowController =
                RequireComponent<OlympusCorridorCombatFlowController>(
                    flowRoot,
                    "Olympus corridor combat flow controller");
            PlayableDirector introDirector = FindObjectByName<PlayableDirector>(stageScene, TimelineDirectorName);
            if (introDirector != null)
            {
                introDirector.time = 0d;
                introDirector.Evaluate();
            }

            InvokePrivate(flowController, "PrepareInitialState");
            InvokePrivate(flowController, "HandleIntroDirectorCompleted");

            return playerRoot.activeSelf
                && playerRoot.activeInHierarchy
                && introSwordGateRoot.activeSelf
                && introSwordGateRoot.activeInHierarchy;
        }

        public static void ApplyOlympusCorridorCombatFlow()
        {
            AssetDatabase.Refresh();
            if (!AssetDatabase.LoadAssetAtPath<SceneAsset>(StageScenePath))
            {
                throw new InvalidOperationException($"Missing Olympus stage scene: {StageScenePath}");
            }

            if (!AssetDatabase.LoadAssetAtPath<SceneAsset>(SourceCombatScenePath))
            {
                throw new InvalidOperationException($"Missing canonical combat scene: {SourceCombatScenePath}");
            }

            Scene stageScene = EditorSceneManager.OpenScene(StageScenePath, OpenSceneMode.Single);
            Transform combatStartAnchor = RequireObjectInScene(stageScene, CombatStartAnchorName).transform;
            Transform combatCameraHandoffPose =
                RequireObjectInScene(stageScene, PlayerRevealHandoffCameraName).transform;
            PlayableDirector introDirector = FindObjectByName<PlayableDirector>(stageScene, TimelineDirectorName);
            double introHandoffSeconds = ResolveIntroHandoffSeconds(introDirector);
            RemoveRootIfPresent(stageScene, FlowRootName);

            GameObject flowRoot = CreateRoot(stageScene, FlowRootName, Vector3.zero, Quaternion.identity);
            GameObject packageRoot = CreateChild(
                flowRoot.transform,
                CombatPackageRootName,
                combatStartAnchor.position,
                ResolveGameplayCorridorRotation());

            List<GameObject> importedRoots = MoveCanonicalCombatRoots(stageScene);
            MapImportedRoots(importedRoots, packageRoot.transform, combatStartAnchor);
            RenameCombatCamera(importedRoots);

            GameObject playerRoot = RequireChildObject(packageRoot.transform, SourcePlayerRootName);
            PlayerMovementController player = RequireComponent<PlayerMovementController>(playerRoot, "combat player movement");
            CombatHealth playerHealth = RequireComponent<CombatHealth>(playerRoot, "combat player health");
            PlayerCombatTargetSelector targetSelector =
                RequireComponent<PlayerCombatTargetSelector>(playerRoot, "combat player target selector");
            PlayerCombatModeController combatModeController =
                RequireComponent<PlayerCombatModeController>(playerRoot, "combat mode controller");
            PlayerRangedBasicAttackAction rangedBasic =
                playerRoot.GetComponent<PlayerRangedBasicAttackAction>();
            PlayerSkill1Action skill1 = playerRoot.GetComponent<PlayerSkill1Action>();
            PlayerSummonSlot1Action summonSlot1 = playerRoot.GetComponent<PlayerSummonSlot1Action>();
            PlayerSupportSummonSlotAction[] supportSummons =
                playerRoot.GetComponents<PlayerSupportSummonSlotAction>();
            ActionCameraController cameraController =
                RequireComponent<ActionCameraController>(
                    RequireChildObject(packageRoot.transform, CombatCameraName),
                    "combat action camera");
            ConfigureCenteredCombatCamera(cameraController, packageRoot.transform);
            BindCombatPlayerToFinalCutsceneHandoff(stageScene, introDirector, playerRoot);
            ConfigurePlayerSwordOnlyHandoff(playerRoot, combatModeController);
            SnapCombatPlayerToHandoffGround(introDirector, playerRoot, packageRoot.transform);
            StabilizeRevealHandoffTimeline(introDirector, packageRoot.transform, playerRoot);

            Transform introSwordGateRoot = CreateChild(
                packageRoot.transform,
                IntroSwordGateRootName,
                packageRoot.transform.position,
                packageRoot.transform.rotation).transform;
            CombatHealth[] introEnemies = ConfigureCutsceneCommandoIntroEnemies(
                stageScene,
                playerHealth,
                cameraController,
                packageRoot.transform,
                introHandoffSeconds,
                out Behaviour[] introEnemyGameplayBehaviours);

            Transform stairTrigger = CreateStairTrigger(packageRoot.transform).transform;
            Collider stairBlocker = CreateStairBlocker(packageRoot.transform);
            GameObject boundsRoot = CreateCorridorBounds(packageRoot.transform);

            GameObject bossRoot = RequireChildObject(packageRoot.transform, SourceBossRootName);
            GameObject closeThreatRoot = RequireChildObject(packageRoot.transform, SourceCloseThreatRootName);
            CombatHealth bossHealth = RequireComponent<CombatHealth>(bossRoot, "canonical boss health");
            CombatHealth closeThreatHealth = RequireComponent<CombatHealth>(closeThreatRoot, "canonical close threat health");
            GameObject hudRoot = RequireChildObject(packageRoot.transform, SourceHudRootName);
            BossBarrageLaneReviewHud reviewHud =
                RequireComponent<BossBarrageLaneReviewHud>(hudRoot, "Olympus handoff review HUD");
            BossBarrageLaneReviewMobileHud mobileHud =
                RequireComponent<BossBarrageLaneReviewMobileHud>(hudRoot, "Olympus handoff mobile HUD");
            Camera[] introCameras = FindIntroCamerasToDisable(stageScene, packageRoot.transform);
            AudioListener[] introAudioListeners = FindIntroAudioListenersToDisable(stageScene, packageRoot.transform);
            Behaviour[] cutsceneBehaviours = FindCutsceneBehavioursToDisableOnHandoff(stageScene);
            GameObject[] cutsceneRoots = FindCutsceneRootsToDisable(stageScene);
            GameObject[] handoffRoots = ResolveHandoffRoots(packageRoot.transform);
            GameObject[] alwaysDisabledRoots = ResolveAlwaysDisabledRoots(packageRoot.transform);
            GameObject[] corridorRoots = ResolveCorridorCombatRoots(packageRoot.transform);
            GameObject[] boundsRoots = { boundsRoot };
            CombatHealth[] corridorTargets = { closeThreatHealth, bossHealth };

            OlympusCorridorCombatFlowController flowController =
                flowRoot.AddComponent<OlympusCorridorCombatFlowController>();
            flowController.Configure(
                introDirector,
                introHandoffSeconds,
                introCameras,
                introAudioListeners,
                cutsceneBehaviours,
                cutsceneRoots,
                handoffRoots,
                alwaysDisabledRoots,
                cameraController,
                combatCameraHandoffPose,
                introSwordGateRoot.gameObject,
                introEnemies,
                introEnemyGameplayBehaviours,
                new[] { stairBlocker },
                stairTrigger,
                2.75f,
                corridorRoots,
                boundsRoots,
                corridorTargets,
                player,
                combatModeController,
                targetSelector,
                rangedBasic,
                skill1,
                summonSlot1,
                supportSummons,
                reviewHud,
                mobileHud,
                HudRevealDelaySeconds,
                HudRevealDurationSeconds);

            flowController.enabled = true;
            ApplyAuthoringInitialState(
                handoffRoots,
                alwaysDisabledRoots,
                introSwordGateRoot.gameObject,
                introEnemyGameplayBehaviours,
                corridorRoots,
                boundsRoots,
                new[] { stairBlocker });
            SetObjectActive(playerRoot, false);
            EditorUtility.SetDirty(flowController);
            EditorUtility.SetDirty(flowRoot);
            EditorUtility.SetDirty(packageRoot);
            EditorSceneManager.MarkSceneDirty(stageScene);
            EditorSceneManager.SaveScene(stageScene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static List<GameObject> MoveCanonicalCombatRoots(Scene stageScene)
        {
            Scene sourceScene = default;
            bool sourceSceneOpened = false;
            try
            {
                sourceScene = EditorSceneManager.OpenScene(SourceCombatScenePath, OpenSceneMode.Additive);
                sourceSceneOpened = true;
                GameObject[] sourceRoots = sourceScene.GetRootGameObjects();
                var imported = new List<GameObject>();
                for (int i = 0; i < sourceRoots.Length; i++)
                {
                    GameObject root = sourceRoots[i];
                    if (root == null || !ShouldImportCanonicalRoot(root.name))
                    {
                        continue;
                    }

                    SceneManager.MoveGameObjectToScene(root, stageScene);
                    imported.Add(root);
                }

                if (imported.Count == 0)
                {
                    throw new InvalidOperationException("No canonical combat roots were imported.");
                }

                return imported;
            }
            finally
            {
                if (sourceSceneOpened && sourceScene.IsValid())
                {
                    EditorSceneManager.CloseScene(sourceScene, removeScene: true);
                }
            }
        }

        private static bool ShouldImportCanonicalRoot(string rootName)
        {
            return string.Equals(rootName, SourceMainCameraRootName, StringComparison.Ordinal)
                || string.Equals(rootName, SourcePlayerRootName, StringComparison.Ordinal)
                || string.Equals(rootName, "ActionFoundationTestEncounter", StringComparison.Ordinal)
                || string.Equals(rootName, "WIN_MARKER_SoldierDefeated", StringComparison.Ordinal)
                || string.Equals(rootName, "FAIL_MARKER_PlayerDefeated", StringComparison.Ordinal)
                || string.Equals(rootName, SourceArenaVfxRootName, StringComparison.Ordinal)
                || string.Equals(rootName, SourceArenaGridRootName, StringComparison.Ordinal)
                || string.Equals(rootName, SourceCombatVfxRootName, StringComparison.Ordinal)
                || rootName.StartsWith("BossBarrageLaneReview_", StringComparison.Ordinal);
        }

        private static void MapImportedRoots(
            List<GameObject> importedRoots,
            Transform packageRoot,
            Transform combatStartAnchor)
        {
            Quaternion targetRotation = ResolveGameplayCorridorRotation();
            Vector3 targetPosition = combatStartAnchor.position;
            for (int i = 0; i < importedRoots.Count; i++)
            {
                GameObject root = importedRoots[i];
                if (IsNonSpatialImportedRoot(root.name))
                {
                    root.transform.SetParent(packageRoot, worldPositionStays: false);
                    root.transform.localPosition = Vector3.zero;
                    root.transform.localRotation = Quaternion.identity;
                    root.transform.localScale = Vector3.one;
                    EditorUtility.SetDirty(root);
                    continue;
                }

                Vector3 sourceOffset = root.transform.position - SourcePlayerStartPosition;
                Vector3 mappedPosition = targetPosition + targetRotation * sourceOffset;
                Quaternion mappedRotation = targetRotation * root.transform.rotation;
                root.transform.SetPositionAndRotation(mappedPosition, mappedRotation);
                root.transform.SetParent(packageRoot, worldPositionStays: true);
                EditorUtility.SetDirty(root);
            }
        }

        private static bool IsNonSpatialImportedRoot(string rootName)
        {
            return string.Equals(rootName, SourceCombatHudCanvasRootName, StringComparison.Ordinal)
                || string.Equals(rootName, SourceCombatHudEventSystemRootName, StringComparison.Ordinal);
        }

        private static void RenameCombatCamera(List<GameObject> importedRoots)
        {
            for (int i = 0; i < importedRoots.Count; i++)
            {
                if (importedRoots[i] != null
                    && string.Equals(importedRoots[i].name, SourceMainCameraRootName, StringComparison.Ordinal))
                {
                    importedRoots[i].name = CombatCameraName;
                    EditorUtility.SetDirty(importedRoots[i]);
                    return;
                }
            }

            throw new InvalidOperationException("Imported canonical combat camera was not found.");
        }

        private static void ConfigureCenteredCombatCamera(
            ActionCameraController cameraController,
            Transform packageRoot)
        {
            SerializedObject serialized = new SerializedObject(cameraController);
            SetVector3Property(serialized, "cameraOffset", CenteredCombatCameraOffset);
            SetVector3Property(serialized, "lookOffset", packageRoot.rotation * CenteredCombatLookOffset);
            SetFloatProperty(serialized, "threatBias", 0f);
            SetFloatProperty(serialized, "maxLeadFromPlayerSpeed", 0f);
            SetFloatProperty(serialized, "targetYawAssist", 0f);
            SetFloatProperty(serialized, "manualYawSpeedDegrees", 0f);
            SetFloatProperty(serialized, "mouseYawDegreesPerPixel", 0f);
            SetVector3Property(serialized, "aimCameraOffset", CenteredCombatAimCameraOffset);
            SetVector3Property(serialized, "aimFocusOffset", CenteredCombatAimFocusOffset);
            SetBoolProperty(serialized, "aimOrbitUsesInput", false);
            SetBoolProperty(serialized, "aimOrbitRotatesCameraPosition", false);
            SetBoolProperty(serialized, "aimAssistUsesYawTarget", false);
            SetBoolProperty(serialized, "useFixedRearYaw", true);
            SetBoolProperty(serialized, "useDeviceFallbackWhenActionMissing", false);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            SetObjectReference(cameraController, "fixedRearYawReference", packageRoot);

            Transform cameraTransform = cameraController.transform;
            cameraTransform.localPosition = CenteredCombatCameraLocalPosition;
            Vector3 focusWorld = packageRoot.TransformPoint(CenteredCombatFocusLocalPosition);
            Vector3 lookDirection = focusWorld - cameraTransform.position;
            if (lookDirection.sqrMagnitude > 0.0001f)
            {
                cameraTransform.rotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
            }

            EditorUtility.SetDirty(cameraTransform);
            EditorUtility.SetDirty(cameraController);

            Camera camera = cameraController.GetComponent<Camera>();
            if (camera != null)
            {
                UniversalAdditionalCameraData cameraData = camera.GetUniversalAdditionalCameraData();
                cameraData.renderPostProcessing = false;
                EditorUtility.SetDirty(camera);
                EditorUtility.SetDirty(cameraData);
            }
        }

        private static void BindCombatPlayerToFinalCutsceneHandoff(
            Scene scene,
            PlayableDirector introDirector,
            GameObject playerRoot)
        {
            if (introDirector == null)
            {
                throw new InvalidOperationException("Olympus intro director is missing; cannot bind gameplay handoff actor.");
            }

            TimelineAsset timeline = introDirector.playableAsset as TimelineAsset
                ?? throw new InvalidOperationException("Olympus intro director is not bound to a TimelineAsset.");
            Animator playerAnimator = playerRoot.GetComponent<Animator>();
            if (playerAnimator == null)
            {
                playerAnimator = playerRoot.GetComponentInChildren<Animator>(includeInactive: true);
            }

            if (playerAnimator == null)
            {
                throw new InvalidOperationException(
                    $"{playerRoot.name} has no Animator for final cutscene handoff binding.");
            }

            AnimationTrack bodyTrack =
                FindOutputTrack<AnimationTrack>(timeline, CombatStartVisualBodyTrackName)
                ?? throw new InvalidOperationException(
                    $"Timeline is missing `{CombatStartVisualBodyTrackName}` for final player handoff.");
            introDirector.SetGenericBinding(bodyTrack, playerAnimator);
            EditorUtility.SetDirty(bodyTrack);

            ActivationTrack activationTrack =
                FindOutputTrack<ActivationTrack>(timeline, CombatStartVisualActivationTrackName)
                ?? throw new InvalidOperationException(
                    $"Timeline is missing `{CombatStartVisualActivationTrackName}` for final player activation.");
            introDirector.SetGenericBinding(activationTrack, playerRoot);
            double revealStartSeconds = FindClipStart(timeline, RevealShotId, Math.Max(0d, timeline.duration - 0.65d));
            ReplaceActivationTrackWithWindowClip(
                activationTrack,
                revealStartSeconds,
                Math.Max(0.1d, timeline.duration - revealStartSeconds),
                "Combat player active for reveal handoff");
            activationTrack.postPlaybackState = ActivationTrack.PostPlaybackState.Active;
            EditorUtility.SetDirty(activationTrack);

            GameObject obsoleteCombatStartVisual = FindRootOrDescendant(scene, CombatStartVisualPlacementName);
            if (obsoleteCombatStartVisual != null)
            {
                UnityEngine.Object.DestroyImmediate(obsoleteCombatStartVisual);
            }

            EditorUtility.SetDirty(playerRoot);
            EditorUtility.SetDirty(playerAnimator);
            EditorUtility.SetDirty(introDirector);
            EditorUtility.SetDirty(timeline);
        }

        private static void ReplaceActivationTrackWithWindowClip(
            ActivationTrack activationTrack,
            double startSeconds,
            double durationSeconds,
            string displayName)
        {
            if (activationTrack == null)
            {
                return;
            }

            DeleteAllTimelineClips(activationTrack);
            TimelineClip activeClip = activationTrack.CreateDefaultClip();
            activeClip.displayName = displayName;
            activeClip.start = Math.Max(0d, startSeconds);
            activeClip.duration = Math.Max(0.1d, durationSeconds);
            activeClip.blendInDuration = 0d;
            activeClip.blendOutDuration = 0d;
            EditorUtility.SetDirty(activationTrack);
        }

        private static void SnapCombatPlayerToHandoffGround(
            PlayableDirector introDirector,
            GameObject playerRoot,
            Transform groundRoot)
        {
            if (introDirector == null || playerRoot == null)
            {
                return;
            }

            double originalTime = introDirector.time;
            bool originalActiveSelf = playerRoot.activeSelf;
            playerRoot.SetActive(true);

            double sampleTime = Math.Max(0d, ResolveIntroHandoffSeconds(introDirector) - 0.02d);
            introDirector.time = sampleTime;
            introDirector.Evaluate();

            SnapActiveRendererBoundsToGround(playerRoot, groundRoot);

            introDirector.time = originalTime;
            introDirector.Evaluate();
            playerRoot.SetActive(originalActiveSelf);

            EditorUtility.SetDirty(playerRoot.transform);
            EditorUtility.SetDirty(playerRoot);
        }

        private static bool SnapActiveRendererBoundsToGround(GameObject root, Transform groundRoot)
        {
            if (root == null
                || !TryCalculateActiveRendererBounds(root, skinnedOnly: true, out Bounds rendererBounds))
            {
                return false;
            }

            float groundY = groundRoot != null ? groundRoot.position.y : root.transform.position.y;
            float targetMinY = groundY + CharacterGroundClearance;
            float deltaY = targetMinY - rendererBounds.min.y;
            if (Mathf.Abs(deltaY) <= CharacterGroundSnapTolerance)
            {
                return true;
            }

            Transform transform = root.transform;
            Vector3 position = transform.position;
            position.y += deltaY;
            transform.position = position;
            EditorUtility.SetDirty(transform);
            return true;
        }

        private static void AddOrUpdateRevealHandoffMatchClip(
            PlayableDirector director,
            TimelineAsset timeline,
            Transform packageRoot,
            GameObject playerRoot,
            double timelineDuration)
        {
            if (director == null || timeline == null || packageRoot == null || playerRoot == null)
            {
                return;
            }

            AnimationTrack track = FindOutputTrack<AnimationTrack>(timeline, RevealAnimationTrackName);
            if (track == null)
            {
                return;
            }

            Transform revealCameraTransform = ResolveTransformReference(director.GetGenericBinding(track));
            if (revealCameraTransform == null)
            {
                return;
            }

            DeleteClipsByDisplayName(track, RevealHandoffMatchClipName);
            double matchStartSeconds = FindTrackClipEnd(track, timelineDuration - 0.65d);
            matchStartSeconds = Math.Min(matchStartSeconds, Math.Max(0d, timelineDuration - 0.1d));
            double matchDuration = Math.Max(0.1d, timelineDuration - matchStartSeconds);

            double originalDirectorTime = director.time;
            director.time = matchStartSeconds;
            director.Evaluate();

            Vector3 startLocalPosition = revealCameraTransform.localPosition;
            Quaternion startLocalRotation = revealCameraTransform.localRotation;
            CalculateSettledCombatCameraPose(
                packageRoot,
                playerRoot,
                out Vector3 targetWorldPosition,
                out Quaternion targetWorldRotation);

            Transform parent = revealCameraTransform.parent;
            Vector3 targetLocalPosition = parent != null
                ? parent.InverseTransformPoint(targetWorldPosition)
                : targetWorldPosition;
            Quaternion targetLocalRotation = parent != null
                ? Quaternion.Inverse(parent.rotation) * targetWorldRotation
                : targetWorldRotation;
            if (Quaternion.Dot(startLocalRotation, targetLocalRotation) < 0f)
            {
                targetLocalRotation = Negate(targetLocalRotation);
            }

            AnimationClip matchClip = LoadOrCreateAnimationClip(RevealHandoffMatchClipPath);
            BuildTransformMatchClip(
                matchClip,
                startLocalPosition,
                startLocalRotation,
                targetLocalPosition,
                targetLocalRotation,
                (float)matchDuration);

            TimelineClip timelineClip = track.CreateClip(matchClip);
            timelineClip.displayName = RevealHandoffMatchClipName;
            timelineClip.start = matchStartSeconds;
            timelineClip.duration = matchDuration;
            timelineClip.blendInDuration = Math.Min(0.18d, matchDuration * 0.35d);
            SetTimelineClipPostExtrapolation(timelineClip, TimelineClip.ClipExtrapolation.Hold);

            if (timelineClip.asset is AnimationPlayableAsset playableAsset)
            {
                playableAsset.removeStartOffset = false;
                playableAsset.applyFootIK = false;
                playableAsset.loop = AnimationPlayableAsset.LoopMode.Off;
                EditorUtility.SetDirty(playableAsset);
            }

            director.time = originalDirectorTime;
            EditorUtility.SetDirty(matchClip);
            EditorUtility.SetDirty(track);
            EditorUtility.SetDirty(director);
        }

        private static void CalculateSettledCombatCameraPose(
            Transform packageRoot,
            GameObject playerRoot,
            out Vector3 position,
            out Quaternion rotation)
        {
            Vector3 playerPosition = playerRoot.transform.position;
            float groundY = packageRoot.position.y;
            if (TryCalculateActiveRendererBounds(playerRoot, skinnedOnly: true, out Bounds rendererBounds))
            {
                float targetMinY = groundY + CharacterGroundClearance;
                if (Mathf.Abs(targetMinY - rendererBounds.min.y) > CharacterGroundSnapTolerance)
                {
                    playerPosition.y += targetMinY - rendererBounds.min.y;
                }
            }

            Quaternion yawRotation = Quaternion.Euler(0f, packageRoot.eulerAngles.y, 0f);
            Vector3 focus = playerPosition + packageRoot.rotation * CenteredCombatLookOffset;
            position = focus + yawRotation * CenteredCombatCameraOffset;
            Vector3 lookDirection = focus - position;
            rotation = lookDirection.sqrMagnitude > 0.0001f
                ? Quaternion.LookRotation(lookDirection.normalized, Vector3.up)
                : packageRoot.rotation;
        }

        private static AnimationClip LoadOrCreateAnimationClip(string path)
        {
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip != null)
            {
                return clip;
            }

            clip = new AnimationClip { name = Path.GetFileNameWithoutExtension(path), frameRate = 60f };
            AssetDatabase.CreateAsset(clip, path);
            return clip;
        }

        private static void BuildTransformMatchClip(
            AnimationClip clip,
            Vector3 startPosition,
            Quaternion startRotation,
            Vector3 endPosition,
            Quaternion endRotation,
            float duration)
        {
            clip.ClearCurves();
            clip.frameRate = 60f;
            float clampedDuration = Mathf.Max(0.1f, duration);
            SetTransformCurve(clip, "m_LocalPosition.x", startPosition.x, endPosition.x, clampedDuration);
            SetTransformCurve(clip, "m_LocalPosition.y", startPosition.y, endPosition.y, clampedDuration);
            SetTransformCurve(clip, "m_LocalPosition.z", startPosition.z, endPosition.z, clampedDuration);
            SetTransformCurve(clip, "m_LocalRotation.x", startRotation.x, endRotation.x, clampedDuration);
            SetTransformCurve(clip, "m_LocalRotation.y", startRotation.y, endRotation.y, clampedDuration);
            SetTransformCurve(clip, "m_LocalRotation.z", startRotation.z, endRotation.z, clampedDuration);
            SetTransformCurve(clip, "m_LocalRotation.w", startRotation.w, endRotation.w, clampedDuration);
            clip.EnsureQuaternionContinuity();
        }

        private static void SetTransformCurve(
            AnimationClip clip,
            string propertyName,
            float startValue,
            float endValue,
            float duration)
        {
            AnimationCurve curve = AnimationCurve.EaseInOut(0f, startValue, duration, endValue);
            AnimationUtility.SetEditorCurve(
                clip,
                EditorCurveBinding.FloatCurve(string.Empty, typeof(Transform), propertyName),
                curve);
        }

        private static Quaternion Negate(Quaternion value)
        {
            return new Quaternion(-value.x, -value.y, -value.z, -value.w);
        }

        private static void ConfigurePlayerSwordOnlyHandoff(
            GameObject playerRoot,
            PlayerCombatModeController combatModeController)
        {
            if (playerRoot == null || combatModeController == null)
            {
                return;
            }

            SerializedObject serialized = new SerializedObject(combatModeController);
            serialized.Update();
            SerializedProperty startingMode = serialized.FindProperty("startingMode");
            if (startingMode != null)
            {
                startingMode.enumValueIndex = (int)PlayerCombatMode.Melee;
            }

            GameObject rangedWeaponRoot =
                ResolveGameObjectReference(GetObjectReferenceProperty(serialized, "rangedWeaponRoot"));
            GameObject meleeWeaponRoot =
                ResolveGameObjectReference(GetObjectReferenceProperty(serialized, "meleeWeaponRoot"));
            SetObjectActive(rangedWeaponRoot, false);
            SetObjectActive(meleeWeaponRoot, true);
            SetExactNamedDescendantsActive(meleeWeaponRoot, "Weapon_Sword", true);
            for (int i = 0; i < PlayerShieldWeaponNames.Length; i++)
            {
                SetExactNamedDescendantsActive(playerRoot, PlayerShieldWeaponNames[i], false);
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(combatModeController);
            EditorUtility.SetDirty(playerRoot);
        }

        private static T FindOutputTrack<T>(TimelineAsset timeline, string trackName)
            where T : TrackAsset
        {
            if (timeline == null || string.IsNullOrWhiteSpace(trackName))
            {
                return null;
            }

            foreach (TrackAsset track in timeline.GetOutputTracks())
            {
                if (track is T typedTrack && string.Equals(track.name, trackName, StringComparison.Ordinal))
                {
                    return typedTrack;
                }
            }

            return null;
        }

        private static Quaternion ResolveGameplayCorridorRotation()
        {
            return Quaternion.Euler(0f, OlympusCorridorGameplayYawDegrees, 0f);
        }

        private static void StabilizeRevealHandoffTimeline(
            PlayableDirector director,
            Transform packageRoot,
            GameObject playerRoot)
        {
            TimelineAsset timeline = director != null ? director.playableAsset as TimelineAsset : null;
            if (timeline == null)
            {
                return;
            }

            double duration = ResolveTimelineDuration(timeline, director);
            if (duration <= 0d)
            {
                return;
            }

            foreach (TrackAsset track in timeline.GetOutputTracks())
            {
                if (track == null)
                {
                    continue;
                }

                bool trackDirty = false;
                foreach (TimelineClip clip in track.GetClips())
                {
                    if (clip == null)
                    {
                        continue;
                    }

                    if (string.Equals(clip.displayName, RevealShotId, StringComparison.Ordinal))
                    {
                        double targetDuration = Math.Max(0.1d, duration - clip.start);
                        if (clip.duration < targetDuration)
                        {
                            clip.duration = targetDuration;
                        }

                        SetTimelineClipPostExtrapolation(clip, TimelineClip.ClipExtrapolation.Hold);
                        trackDirty = true;
                    }
                    else if (string.Equals(track.name, RevealAnimationTrackName, StringComparison.Ordinal))
                    {
                        TimelineClip.ClipExtrapolation extrapolation =
                            packageRoot != null && playerRoot != null
                                ? TimelineClip.ClipExtrapolation.None
                                : TimelineClip.ClipExtrapolation.Hold;
                        SetTimelineClipPostExtrapolation(clip, extrapolation);
                        trackDirty = true;
                    }
                }

                if (trackDirty)
                {
                    EditorUtility.SetDirty(track);
                }
            }

            AddOrUpdateRevealHandoffMatchClip(director, timeline, packageRoot, playerRoot, duration);
            AddOrUpdateFirstPersonResidualActivationTrack(director, timeline);

            EditorUtility.SetDirty(timeline);
        }

        private static void AddOrUpdateFirstPersonResidualActivationTrack(
            PlayableDirector director,
            TimelineAsset timeline)
        {
            if (director == null || timeline == null)
            {
                return;
            }

            GameObject firstPersonResidualRoot =
                FindRootOrDescendant(director.gameObject.scene, FirstPersonResidualVisualRootName);
            if (firstPersonResidualRoot == null)
            {
                return;
            }

            double revealStartSeconds = FindClipStart(timeline, RevealShotId, -1d);
            if (revealStartSeconds <= 0d)
            {
                return;
            }

            RemoveTimelineTrack(timeline, FirstPersonResidualActivationTrackName, director);
            ActivationTrack track = timeline.CreateTrack<ActivationTrack>(FirstPersonResidualActivationTrackName);
            director.SetGenericBinding(track, firstPersonResidualRoot);
            TimelineClip activeClip = track.CreateDefaultClip();
            activeClip.displayName = "First person residual visible before reveal";
            activeClip.start = 0d;
            activeClip.duration = Math.Max(0.1d, revealStartSeconds - 0.04d);
            track.postPlaybackState = ActivationTrack.PostPlaybackState.Inactive;

            EditorUtility.SetDirty(track);
            EditorUtility.SetDirty(firstPersonResidualRoot);
            EditorUtility.SetDirty(director);
        }

        private static CombatHealth[] ConfigureCutsceneCommandoIntroEnemies(
            Scene scene,
            CombatHealth playerHealth,
            ActionCameraController cameraController,
            Transform packageRoot,
            double introHandoffSeconds,
            out Behaviour[] gameplayBehaviours)
        {
            IntroGatePodInvasionBridgeCue invasionBridge =
                FindObjectByName<IntroGatePodInvasionBridgeCue>(scene, InvasionBridgeRootName);
            if (invasionBridge != null)
            {
                AlignAndHoldInvasionBridgeCommandosForCombatHandoff(
                    invasionBridge,
                    packageRoot,
                    introHandoffSeconds);
            }

            var healths = new CombatHealth[CutsceneCommandoNames.Length];
            var behaviours = new List<Behaviour>();
            for (int i = 0; i < CutsceneCommandoNames.Length; i++)
            {
                GameObject enemy = RequireObjectInScene(scene, CutsceneCommandoNames[i]);
                ApplyCutsceneCommandoEndPose(invasionBridge, enemy.transform);

                BasicSoldierEnemy soldier = RequireComponent<BasicSoldierEnemy>(enemy, enemy.name);
                CombatHealth enemyHealth = RequireComponent<CombatHealth>(enemy, enemy.name + " health");
                CombatTargetSensor targetSensor =
                    RequireComponent<CombatTargetSensor>(enemy, enemy.name + " target sensor");
                targetSensor.ConfigureTargetCandidates(new[] { playerHealth }, refreshNow: false);
                soldier.ConfigureTarget(null, null);
                SetObjectReference(soldier, "targetSensor", targetSensor);
                SetObjectReference(soldier, "selfHealth", enemyHealth);

                EnemyActionCameraCueDriver cameraCueDriver =
                    enemy.GetComponent<EnemyActionCameraCueDriver>();
                if (cameraCueDriver != null)
                {
                    SetObjectReference(cameraCueDriver, "agentSource", soldier);
                    SetObjectReference(cameraCueDriver, "cameraController", cameraController);
                    SetObjectReference(cameraCueDriver, "cueSpace", enemy.transform);
                }

                CollectIntroCommandoGameplayBehaviours(enemy, behaviours);
                SetIntroCommandoGameplayEnabled(enemy, false);
                enemy.SetActive(false);
                healths[i] = enemyHealth;
                EditorUtility.SetDirty(enemy);
            }

            gameplayBehaviours = behaviours.ToArray();
            return healths;
        }

        private static void AlignAndHoldInvasionBridgeCommandosForCombatHandoff(
            IntroGatePodInvasionBridgeCue invasionBridge,
            Transform packageRoot,
            double introHandoffSeconds)
        {
            SerializedObject serialized = new SerializedObject(invasionBridge);
            SerializedProperty commandos = serialized.FindProperty("commandos");
            if (commandos == null || !commandos.isArray)
            {
                return;
            }

            float holdEndSeconds = (float)Math.Max(introHandoffSeconds + 0.35d, introHandoffSeconds);
            for (int i = 0; i < commandos.arraySize; i++)
            {
                SerializedProperty cue = commandos.GetArrayElementAtIndex(i);
                AlignCommandoCuePathToCombatSlot(cue, packageRoot, i);
                SerializedProperty startSeconds = cue.FindPropertyRelative("startSeconds");
                SerializedProperty endSeconds = cue.FindPropertyRelative("endSeconds");
                SerializedProperty attackStartSeconds = cue.FindPropertyRelative("attackStartSeconds");
                SerializedProperty attackStateName = cue.FindPropertyRelative("attackStateName");
                float cueStartSeconds = startSeconds != null ? startSeconds.floatValue : 0f;
                float moveDuration = i < IntroSwordCommandoMoveDurations.Length
                    ? IntroSwordCommandoMoveDurations[i]
                    : 4.35f;
                float moveEndSeconds = cueStartSeconds + moveDuration;
                if (attackStartSeconds != null)
                {
                    attackStartSeconds.floatValue = moveEndSeconds;
                }

                if (attackStateName != null)
                {
                    attackStateName.stringValue = CutsceneCommandoIdleStateName;
                }

                if (endSeconds != null)
                {
                    endSeconds.floatValue = Mathf.Max(moveEndSeconds + 0.01f, holdEndSeconds);
                }
            }

            SetFloatProperty(serialized, "commandoStrideBobHeight", CommandoStrideBobHeight);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(invasionBridge);
        }

        private static void AlignCommandoCuePathToCombatSlot(
            SerializedProperty cue,
            Transform packageRoot,
            int index)
        {
            if (cue == null
                || packageRoot == null
                || index < 0
                || index >= IntroSwordEnemyCombatSlotLocalPositions.Length)
            {
                return;
            }

            SerializedProperty rootProperty = cue.FindPropertyRelative("root");
            SerializedProperty startLocalPosition = cue.FindPropertyRelative("startLocalPosition");
            SerializedProperty endLocalPosition = cue.FindPropertyRelative("endLocalPosition");
            if (rootProperty == null || startLocalPosition == null || endLocalPosition == null)
            {
                return;
            }

            Transform root = rootProperty.objectReferenceValue as Transform;
            Transform localSpace = root != null && root.parent != null ? root.parent : root;
            if (localSpace == null)
            {
                return;
            }

            Vector3 originalStart = startLocalPosition.vector3Value;
            Vector3 originalEnd = endLocalPosition.vector3Value;
            Vector3 pathDelta = originalStart - originalEnd;
            Vector3 desiredEndWorld =
                packageRoot.TransformPoint(IntroSwordEnemyCombatSlotLocalPositions[index]);
            Vector3 desiredEndLocal = localSpace.InverseTransformPoint(desiredEndWorld);

            endLocalPosition.vector3Value = desiredEndLocal;
            startLocalPosition.vector3Value = desiredEndLocal + pathDelta;
        }

        private static void ApplyCutsceneCommandoEndPose(
            IntroGatePodInvasionBridgeCue invasionBridge,
            Transform commando)
        {
            if (invasionBridge == null || commando == null)
            {
                return;
            }

            IntroGatePodInvasionBridgeCue.CommandoCue[] commandos = invasionBridge.Commandos;
            for (int i = 0; i < commandos.Length; i++)
            {
                IntroGatePodInvasionBridgeCue.CommandoCue cue = commandos[i];
                if (cue.Root != commando)
                {
                    continue;
                }

                commando.localPosition = cue.EndLocalPosition;
                commando.localRotation = Quaternion.Euler(cue.LocalEulerAngles);
                Animator animator = cue.Animator;
                if (animator != null)
                {
                    animator.Play(CutsceneCommandoIdleStateName, 0, 0f);
                    if (animator.gameObject.activeInHierarchy)
                    {
                        animator.Update(0f);
                    }

                    EditorUtility.SetDirty(animator);
                }

                EditorUtility.SetDirty(commando);
                return;
            }
        }

        private static void CollectIntroCommandoGameplayBehaviours(
            GameObject enemy,
            List<Behaviour> behaviours)
        {
            AddIfPresent(enemy.GetComponent<CombatHealth>());
            AddIfPresent(enemy.GetComponent<CombatTargetSensor>());
            AddIfPresent(enemy.GetComponent<BasicSoldierEnemy>());
            AddIfPresent(enemy.GetComponent<EnemyActionCameraCueDriver>());
            AddIfPresent(enemy.GetComponent<EnemyCombatVfxCueDriver>());
            AddIfPresent(enemy.GetComponent<EnemyAttackTelegraphPresenter>());
            AddIfPresent(enemy.GetComponent<CombatHitFeedback>());
            AddIfPresent(enemy.GetComponent<CombatVfxCuePlayer>());

            void AddIfPresent(Behaviour behaviour)
            {
                if (behaviour != null && !behaviours.Contains(behaviour))
                {
                    behaviours.Add(behaviour);
                }
            }
        }

        private static void SetIntroCommandoGameplayEnabled(GameObject enemy, bool enabled)
        {
            if (enemy == null)
            {
                return;
            }

            SetBehaviourEnabled(enemy.GetComponent<CombatHealth>(), enabled);
            SetBehaviourEnabled(enemy.GetComponent<CombatTargetSensor>(), enabled);
            SetBehaviourEnabled(enemy.GetComponent<BasicSoldierEnemy>(), enabled);
            SetBehaviourEnabled(enemy.GetComponent<EnemyActionCameraCueDriver>(), enabled);
            SetBehaviourEnabled(enemy.GetComponent<EnemyCombatVfxCueDriver>(), enabled);
            SetBehaviourEnabled(enemy.GetComponent<EnemyAttackTelegraphPresenter>(), enabled);
            SetBehaviourEnabled(enemy.GetComponent<CombatHitFeedback>(), enabled);
            SetBehaviourEnabled(enemy.GetComponent<CombatVfxCuePlayer>(), enabled);
        }

        private static GameObject CreateStairTrigger(Transform packageRoot)
        {
            GameObject trigger = CreateChild(
                packageRoot,
                StairTriggerName,
                packageRoot.TransformPoint(new Vector3(0f, 0f, 9.5f)),
                packageRoot.rotation);
            SphereCollider collider = trigger.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = 2.75f;
            EditorUtility.SetDirty(collider);
            return trigger;
        }

        private static Collider CreateStairBlocker(Transform packageRoot)
        {
            GameObject blocker = CreateChild(
                packageRoot,
                StairBlockerName,
                packageRoot.TransformPoint(new Vector3(0f, 1.65f, 7.35f)),
                packageRoot.rotation);
            BoxCollider collider = blocker.AddComponent<BoxCollider>();
            collider.size = new Vector3(8.5f, 3.3f, 0.45f);
            collider.isTrigger = false;
            EditorUtility.SetDirty(collider);
            return collider;
        }

        private static GameObject CreateCorridorBounds(Transform packageRoot)
        {
            GameObject root = CreateChild(
                packageRoot,
                CorridorBoundsRootName,
                packageRoot.position,
                packageRoot.rotation);
            CreateBoundCollider(root.transform, "LeftWall", new Vector3(-4.25f, 1.7f, 31f), new Vector3(0.35f, 3.4f, 52f));
            CreateBoundCollider(root.transform, "RightWall", new Vector3(4.25f, 1.7f, 31f), new Vector3(0.35f, 3.4f, 52f));
            CreateBoundCollider(root.transform, "BackGate", new Vector3(0f, 1.7f, 8.5f), new Vector3(8.5f, 3.4f, 0.35f));
            CreateBoundCollider(root.transform, "ForwardLimit", new Vector3(0f, 1.7f, 56f), new Vector3(8.5f, 3.4f, 0.35f));
            return root;
        }

        private static void CreateBoundCollider(
            Transform parent,
            string name,
            Vector3 localPosition,
            Vector3 size)
        {
            GameObject bound = CreateChild(parent, name, parent.TransformPoint(localPosition), parent.rotation);
            BoxCollider collider = bound.AddComponent<BoxCollider>();
            collider.size = size;
            collider.isTrigger = false;
            EditorUtility.SetDirty(collider);
        }

        private static GameObject[] ResolveHandoffRoots(Transform packageRoot)
        {
            return FilterExisting(
                RequireChildObject(packageRoot, CombatCameraName),
                FindDirectChildObject(packageRoot, SourceHudRootName),
                FindDirectChildObject(packageRoot, SourceCombatHudCanvasRootName),
                FindDirectChildObject(packageRoot, SourceCombatHudEventSystemRootName),
                FindDirectChildObject(packageRoot, SourceCombatVfxRootName));
        }

        private static GameObject[] ResolveAlwaysDisabledRoots(Transform packageRoot)
        {
            return FilterExisting(
                FindDirectChildObject(packageRoot, SourceArenaVfxRootName),
                FindDirectChildObject(packageRoot, SourceArenaGridRootName));
        }

        private static GameObject[] ResolveCorridorCombatRoots(Transform packageRoot)
        {
            var roots = new List<GameObject>();
            for (int i = 0; i < packageRoot.childCount; i++)
            {
                Transform child = packageRoot.GetChild(i);
                if (child == null || ShouldExcludeFromCorridorRoots(child.name))
                {
                    continue;
                }

                roots.Add(child.gameObject);
            }

            return roots.ToArray();
        }

        private static bool ShouldExcludeFromCorridorRoots(string rootName)
        {
            return string.Equals(rootName, SourcePlayerRootName, StringComparison.Ordinal)
                || string.Equals(rootName, CombatCameraName, StringComparison.Ordinal)
                || string.Equals(rootName, SourceCombatVfxRootName, StringComparison.Ordinal)
                || string.Equals(rootName, SourceHudRootName, StringComparison.Ordinal)
                || string.Equals(rootName, SourceCombatHudCanvasRootName, StringComparison.Ordinal)
                || string.Equals(rootName, SourceCombatHudEventSystemRootName, StringComparison.Ordinal)
                || string.Equals(rootName, SourceArenaVfxRootName, StringComparison.Ordinal)
                || string.Equals(rootName, SourceArenaGridRootName, StringComparison.Ordinal)
                || string.Equals(rootName, IntroSwordGateRootName, StringComparison.Ordinal)
                || string.Equals(rootName, StairTriggerName, StringComparison.Ordinal)
                || string.Equals(rootName, StairBlockerName, StringComparison.Ordinal)
                || string.Equals(rootName, CorridorBoundsRootName, StringComparison.Ordinal);
        }

        private static Camera[] FindIntroCamerasToDisable(Scene scene, Transform combatPackageRoot)
        {
            var cameras = new List<Camera>();
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                Camera[] rootCameras = roots[i].GetComponentsInChildren<Camera>(includeInactive: true);
                for (int j = 0; j < rootCameras.Length; j++)
                {
                    if (rootCameras[j] != null && !rootCameras[j].transform.IsChildOf(combatPackageRoot))
                    {
                        cameras.Add(rootCameras[j]);
                    }
                }
            }

            return cameras.ToArray();
        }

        private static AudioListener[] FindIntroAudioListenersToDisable(Scene scene, Transform combatPackageRoot)
        {
            var listeners = new List<AudioListener>();
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                AudioListener[] rootListeners =
                    roots[i].GetComponentsInChildren<AudioListener>(includeInactive: true);
                for (int j = 0; j < rootListeners.Length; j++)
                {
                    if (rootListeners[j] != null && !rootListeners[j].transform.IsChildOf(combatPackageRoot))
                    {
                        listeners.Add(rootListeners[j]);
                    }
                }
            }

            return listeners.ToArray();
        }

        private static Behaviour[] FindCutsceneBehavioursToDisableOnHandoff(Scene scene)
        {
            var behaviours = new List<Behaviour>();
            IntroGatePodInvasionBridgeCue invasionBridge =
                FindObjectByName<IntroGatePodInvasionBridgeCue>(scene, InvasionBridgeRootName);
            if (invasionBridge != null)
            {
                behaviours.Add(invasionBridge);
            }

            return behaviours.ToArray();
        }

        private static GameObject[] FindCutsceneRootsToDisable(Scene scene)
        {
            var roots = new List<GameObject>();
            GameObject combatStartVisual = FindRootOrDescendant(scene, CombatStartVisualPlacementName);
            if (combatStartVisual != null)
            {
                roots.Add(combatStartVisual);
            }

            GameObject firstPersonResidualVisual = FindRootOrDescendant(scene, FirstPersonResidualVisualRootName);
            if (firstPersonResidualVisual != null)
            {
                roots.Add(firstPersonResidualVisual);
            }

            return roots.ToArray();
        }

        private static void ApplyAuthoringInitialState(
            GameObject[] handoffRoots,
            GameObject[] alwaysDisabledRoots,
            GameObject introSwordGateRoot,
            Behaviour[] introSwordEnemyGameplayBehaviours,
            GameObject[] corridorCombatRoots,
            GameObject[] corridorBoundsRoots,
            Collider[] stairBlockers)
        {
            SetObjectsActive(handoffRoots, false);
            SetObjectsActive(alwaysDisabledRoots, false);
            SetBehavioursEnabled(introSwordEnemyGameplayBehaviours, false);
            SetObjectActive(introSwordGateRoot, false);
            SetObjectsActive(corridorCombatRoots, false);
            SetObjectsActive(corridorBoundsRoots, false);
            for (int i = 0; i < stairBlockers.Length; i++)
            {
                if (stairBlockers[i] != null)
                {
                    stairBlockers[i].enabled = true;
                    EditorUtility.SetDirty(stairBlockers[i]);
                }
            }
        }

        private static void SetObjectsActive(GameObject[] objects, bool active)
        {
            for (int i = 0; i < objects.Length; i++)
            {
                SetObjectActive(objects[i], active);
            }
        }

        private static void SetObjectActive(GameObject target, bool active)
        {
            if (target != null)
            {
                target.SetActive(active);
                EditorUtility.SetDirty(target);
            }
        }

        private static void SetBehavioursEnabled(Behaviour[] behaviours, bool enabled)
        {
            if (behaviours == null)
            {
                return;
            }

            for (int i = 0; i < behaviours.Length; i++)
            {
                SetBehaviourEnabled(behaviours[i], enabled);
            }
        }

        private static void SetBehaviourEnabled(Behaviour behaviour, bool enabled)
        {
            if (behaviour != null)
            {
                behaviour.enabled = enabled;
                EditorUtility.SetDirty(behaviour);
            }
        }

        private static double ResolveIntroHandoffSeconds(PlayableDirector director)
        {
            if (director != null && !double.IsInfinity(director.duration) && director.duration > 0d)
            {
                return Math.Max(0d, director.duration - 0.05d);
            }

            return 36.5d;
        }

        private static GameObject[] FilterExisting(params GameObject[] objects)
        {
            var filtered = new List<GameObject>();
            for (int i = 0; i < objects.Length; i++)
            {
                if (objects[i] != null)
                {
                    filtered.Add(objects[i]);
                }
            }

            return filtered.ToArray();
        }

        private static double ResolveTimelineDuration(TimelineAsset timeline, PlayableDirector director)
        {
            if (director != null && !double.IsInfinity(director.duration) && director.duration > 0d)
            {
                return director.duration;
            }

            if (timeline != null && !double.IsInfinity(timeline.duration) && timeline.duration > 0d)
            {
                return timeline.duration;
            }

            return timeline != null ? timeline.fixedDuration : 0d;
        }

        private static double FindClipEnd(TimelineAsset timeline, string displayName, double fallback)
        {
            if (timeline == null)
            {
                return fallback;
            }

            double end = fallback;
            foreach (TrackAsset track in timeline.GetOutputTracks())
            {
                if (track == null)
                {
                    continue;
                }

                foreach (TimelineClip clip in track.GetClips())
                {
                    if (clip != null && string.Equals(clip.displayName, displayName, StringComparison.Ordinal))
                    {
                        end = Math.Max(end, clip.end);
                    }
                }
            }

            return end;
        }

        private static double FindClipStart(TimelineAsset timeline, string displayName, double fallback)
        {
            if (timeline == null)
            {
                return fallback;
            }

            double start = double.PositiveInfinity;
            foreach (TrackAsset track in timeline.GetOutputTracks())
            {
                if (track == null)
                {
                    continue;
                }

                foreach (TimelineClip clip in track.GetClips())
                {
                    if (clip != null && string.Equals(clip.displayName, displayName, StringComparison.Ordinal))
                    {
                        start = Math.Min(start, clip.start);
                    }
                }
            }

            return double.IsPositiveInfinity(start) ? fallback : start;
        }

        private static List<Camera> FindActiveEnabledCameras(Scene scene)
        {
            var cameras = new List<Camera>();
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                Camera[] rootCameras = roots[i].GetComponentsInChildren<Camera>(includeInactive: true);
                for (int j = 0; j < rootCameras.Length; j++)
                {
                    Camera camera = rootCameras[j];
                    if (camera != null && camera.enabled && camera.gameObject.activeInHierarchy)
                    {
                        cameras.Add(camera);
                    }
                }
            }

            return cameras;
        }

        private static Camera[] FindAllSceneCameras(Scene scene)
        {
            var cameras = new List<Camera>();
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                Camera[] rootCameras = roots[i].GetComponentsInChildren<Camera>(includeInactive: true);
                for (int j = 0; j < rootCameras.Length; j++)
                {
                    if (rootCameras[j] != null)
                    {
                        cameras.Add(rootCameras[j]);
                    }
                }
            }

            return cameras.ToArray();
        }

        private static void SampleIntroHandoffMoment(
            PlayableDirector director,
            IntroGatePodInvasionBridgeCue invasionBridge,
            double introHandoffSeconds)
        {
            double sampleTime = Math.Max(0d, introHandoffSeconds - 0.02d);
            if (director != null)
            {
                director.time = sampleTime;
                director.Evaluate();
            }

            if (invasionBridge != null)
            {
                invasionBridge.Sample((float)sampleTime);
            }
        }

        private static bool SimulateIntroDirectorStopAfterHandoff(
            PlayableDirector director,
            OlympusCorridorCombatFlowController flowController,
            GameObject playerRoot)
        {
            if (playerRoot == null)
            {
                return false;
            }

            if (director != null)
            {
                double duration = !double.IsInfinity(director.duration) && director.duration > 0d
                    ? director.duration
                    : director.time;
                director.time = duration;
                director.Evaluate();
                director.Stop();
            }

            if (flowController != null)
            {
                InvokePrivate(flowController, "HandleIntroDirectorCompleted");
            }

            return playerRoot.activeSelf && playerRoot.activeInHierarchy;
        }

        private static Camera FindActiveGameplayHandoffIntroCamera(Scene scene, Transform packageRoot)
        {
            List<Camera> cameras = FindActiveEnabledCameras(scene);
            Camera fallback = null;
            for (int i = 0; i < cameras.Count; i++)
            {
                Camera camera = cameras[i];
                if (camera == null
                    || (packageRoot != null && camera.transform.IsChildOf(packageRoot)))
                {
                    continue;
                }

                if (string.Equals(camera.name, PlayerRevealHandoffCameraName, StringComparison.Ordinal))
                {
                    return camera;
                }

                fallback ??= camera;
            }

            return fallback;
        }

        private static CameraPresentationSnapshot CaptureCameraPresentation(Camera camera)
        {
            if (camera == null)
            {
                return default;
            }

            UniversalAdditionalCameraData cameraData =
                camera.GetComponent<UniversalAdditionalCameraData>();
            return new CameraPresentationSnapshot(
                true,
                GetHierarchyPath(camera.transform),
                camera.fieldOfView,
                camera.clearFlags,
                camera.backgroundColor,
                camera.allowHDR,
                camera.allowMSAA,
                camera.nearClipPlane,
                camera.farClipPlane,
                cameraData != null,
                cameraData != null && cameraData.renderPostProcessing,
                cameraData != null ? cameraData.antialiasing : AntialiasingMode.None,
                cameraData != null ? cameraData.antialiasingQuality : AntialiasingQuality.Low);
        }

        private static bool CameraPresentationsMatch(
            CameraPresentationSnapshot source,
            CameraPresentationSnapshot target)
        {
            if (!source.IsValid || !target.IsValid)
            {
                return false;
            }

            bool universalDataMatches = !source.HasUniversalData
                || (target.HasUniversalData
                    && source.RenderPostProcessing == target.RenderPostProcessing
                    && source.Antialiasing == target.Antialiasing
                    && source.AntialiasingQuality == target.AntialiasingQuality);
            return Mathf.Abs(source.FieldOfView - target.FieldOfView) <= 0.05f
                && source.ClearFlags == target.ClearFlags
                && ColorDistance(source.BackgroundColor, target.BackgroundColor) <= 0.01f
                && source.AllowHDR == target.AllowHDR
                && source.AllowMSAA == target.AllowMSAA
                && Mathf.Abs(source.NearClipPlane - target.NearClipPlane) <= 0.01f
                && Mathf.Abs(source.FarClipPlane - target.FarClipPlane) <= 0.5f
                && universalDataMatches;
        }

        private static float ColorDistance(Color a, Color b)
        {
            return Mathf.Abs(a.r - b.r)
                + Mathf.Abs(a.g - b.g)
                + Mathf.Abs(a.b - b.b)
                + Mathf.Abs(a.a - b.a);
        }

        private static GameObject[] BuildEnvironmentRendererExclusions(
            GameObject combatPackageRoot,
            GameObject firstPersonResidualRoot,
            GameObject[] cutsceneCommandos)
        {
            var exclusions = new List<GameObject>();
            if (combatPackageRoot != null)
            {
                exclusions.Add(combatPackageRoot);
            }

            if (firstPersonResidualRoot != null)
            {
                exclusions.Add(firstPersonResidualRoot);
            }

            if (cutsceneCommandos != null)
            {
                for (int i = 0; i < cutsceneCommandos.Length; i++)
                {
                    if (cutsceneCommandos[i] != null)
                    {
                        exclusions.Add(cutsceneCommandos[i]);
                    }
                }
            }

            return exclusions.ToArray();
        }

        private static HashSet<string> CaptureActiveSceneRendererPathSet(
            Scene scene,
            GameObject[] excludedRoots)
        {
            var paths = new HashSet<string>();
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                GameObject root = roots[i];
                if (root == null || IsTransformUnderAny(root.transform, excludedRoots))
                {
                    continue;
                }

                Renderer[] renderers = root.GetComponentsInChildren<Renderer>(includeInactive: true);
                for (int j = 0; j < renderers.Length; j++)
                {
                    Renderer renderer = renderers[j];
                    if (renderer == null
                        || !renderer.enabled
                        || !renderer.gameObject.activeInHierarchy
                        || IsTransformUnderAny(renderer.transform, excludedRoots))
                    {
                        continue;
                    }

                    paths.Add(GetHierarchyPath(renderer.transform));
                }
            }

            return paths;
        }

        private static HashSet<string> CaptureActiveRendererPathSet(
            GameObject root,
            GameObject[] excludedRoots)
        {
            var paths = new HashSet<string>();
            if (root == null)
            {
                return paths;
            }

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(includeInactive: true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null
                    || !renderer.enabled
                    || !renderer.gameObject.activeInHierarchy
                    || IsTransformUnderAny(renderer.transform, excludedRoots))
                {
                    continue;
                }

                paths.Add(GetHierarchyPath(renderer.transform));
            }

            return paths;
        }

        private static bool RendererPathSetsEqual(HashSet<string> before, HashSet<string> after)
        {
            if (before == null || after == null)
            {
                return before == after;
            }

            return before.SetEquals(after);
        }

        private static bool IsTransformUnderAny(Transform transform, GameObject[] roots)
        {
            if (transform == null || roots == null)
            {
                return false;
            }

            for (int i = 0; i < roots.Length; i++)
            {
                GameObject root = roots[i];
                if (root != null
                    && (transform == root.transform || transform.IsChildOf(root.transform)))
                {
                    return true;
                }
            }

            return false;
        }

        private static Vector3[] CaptureWorldPositions(GameObject[] objects)
        {
            if (objects == null)
            {
                return Array.Empty<Vector3>();
            }

            var positions = new Vector3[objects.Length];
            for (int i = 0; i < objects.Length; i++)
            {
                positions[i] = objects[i] != null ? objects[i].transform.position : default;
            }

            return positions;
        }

        private static float CalculateMaxWorldDelta(Vector3[] before, Vector3[] after)
        {
            if (before == null || after == null)
            {
                return float.PositiveInfinity;
            }

            int count = Math.Min(before.Length, after.Length);
            float maxDelta = 0f;
            for (int i = 0; i < count; i++)
            {
                maxDelta = Mathf.Max(maxDelta, Vector3.Distance(before[i], after[i]));
            }

            return before.Length == after.Length ? maxDelta : float.PositiveInfinity;
        }

        private static PlayerGroundSnapshot CapturePlayerGroundSnapshot(
            GameObject playerRoot,
            Transform groundRoot)
        {
            if (playerRoot == null)
            {
                return default;
            }

            float groundY = groundRoot != null ? groundRoot.position.y : playerRoot.transform.position.y;
            bool hasRendererBounds =
                TryCalculateActiveRendererBounds(playerRoot, skinnedOnly: true, out Bounds rendererBounds);
            CharacterController characterController = playerRoot.GetComponent<CharacterController>();
            bool hasControllerBounds = characterController != null;
            float rendererMinY = hasRendererBounds ? rendererBounds.min.y : float.PositiveInfinity;
            float controllerMinY = hasControllerBounds
                ? characterController.bounds.min.y
                : float.PositiveInfinity;
            float resolvedMinY = hasRendererBounds ? rendererMinY : controllerMinY;
            return new PlayerGroundSnapshot(
                hasRendererBounds,
                rendererMinY,
                hasControllerBounds,
                controllerMinY,
                groundY,
                resolvedMinY);
        }

        private readonly struct CameraPhaseSnapshot
        {
            public readonly List<Camera> ActiveEnabledCameras;
            public readonly Vector3 CombatCameraLocalPosition;
            public readonly Vector3 PlayerBodyViewport;
            public readonly bool HasPlayerSkinnedBounds;
            public readonly Vector3 PlayerSkinnedCenterViewport;
            public readonly bool HasPlayerAllBounds;
            public readonly Vector3 PlayerAllRendererCenterViewport;
            public readonly bool OnlyCombatCameraEnabled;
            public readonly bool CombatCameraCentersPlayer;
            public readonly bool CombatCameraCentersPlayerRenderer;

            public CameraPhaseSnapshot(
                List<Camera> activeEnabledCameras,
                Vector3 combatCameraLocalPosition,
                Vector3 playerBodyViewport,
                bool hasPlayerSkinnedBounds,
                Vector3 playerSkinnedCenterViewport,
                bool hasPlayerAllBounds,
                Vector3 playerAllRendererCenterViewport,
                bool onlyCombatCameraEnabled,
                bool combatCameraCentersPlayer,
                bool combatCameraCentersPlayerRenderer)
            {
                ActiveEnabledCameras = activeEnabledCameras;
                CombatCameraLocalPosition = combatCameraLocalPosition;
                PlayerBodyViewport = playerBodyViewport;
                HasPlayerSkinnedBounds = hasPlayerSkinnedBounds;
                PlayerSkinnedCenterViewport = playerSkinnedCenterViewport;
                HasPlayerAllBounds = hasPlayerAllBounds;
                PlayerAllRendererCenterViewport = playerAllRendererCenterViewport;
                OnlyCombatCameraEnabled = onlyCombatCameraEnabled;
                CombatCameraCentersPlayer = combatCameraCentersPlayer;
                CombatCameraCentersPlayerRenderer = combatCameraCentersPlayerRenderer;
            }
        }

        private readonly struct CameraPresentationSnapshot
        {
            public readonly bool IsValid;
            public readonly string CameraPath;
            public readonly float FieldOfView;
            public readonly CameraClearFlags ClearFlags;
            public readonly Color BackgroundColor;
            public readonly bool AllowHDR;
            public readonly bool AllowMSAA;
            public readonly float NearClipPlane;
            public readonly float FarClipPlane;
            public readonly bool HasUniversalData;
            public readonly bool RenderPostProcessing;
            public readonly AntialiasingMode Antialiasing;
            public readonly AntialiasingQuality AntialiasingQuality;

            public CameraPresentationSnapshot(
                bool isValid,
                string cameraPath,
                float fieldOfView,
                CameraClearFlags clearFlags,
                Color backgroundColor,
                bool allowHDR,
                bool allowMSAA,
                float nearClipPlane,
                float farClipPlane,
                bool hasUniversalData,
                bool renderPostProcessing,
                AntialiasingMode antialiasing,
                AntialiasingQuality antialiasingQuality)
            {
                IsValid = isValid;
                CameraPath = cameraPath;
                FieldOfView = fieldOfView;
                ClearFlags = clearFlags;
                BackgroundColor = backgroundColor;
                AllowHDR = allowHDR;
                AllowMSAA = allowMSAA;
                NearClipPlane = nearClipPlane;
                FarClipPlane = farClipPlane;
                HasUniversalData = hasUniversalData;
                RenderPostProcessing = renderPostProcessing;
                Antialiasing = antialiasing;
                AntialiasingQuality = antialiasingQuality;
            }
        }

        private readonly struct PlayerGroundSnapshot
        {
            public readonly bool HasRendererBounds;
            public readonly float RendererMinY;
            public readonly bool HasControllerBounds;
            public readonly float ControllerMinY;
            public readonly float GroundY;
            public readonly float ResolvedMinY;

            public PlayerGroundSnapshot(
                bool hasRendererBounds,
                float rendererMinY,
                bool hasControllerBounds,
                float controllerMinY,
                float groundY,
                float resolvedMinY)
            {
                HasRendererBounds = hasRendererBounds;
                RendererMinY = rendererMinY;
                HasControllerBounds = hasControllerBounds;
                ControllerMinY = controllerMinY;
                GroundY = groundY;
                ResolvedMinY = resolvedMinY;
            }

            public bool IsValid => HasRendererBounds || HasControllerBounds;
            public bool IsWithinGroundTolerance =>
                IsValid && ResolvedMinY >= GroundY - 0.005f && ResolvedMinY <= GroundY + 0.055f;
        }

        private readonly struct CommandoTempoSnapshot
        {
            public readonly bool IsValid;
            public readonly float MaxMoveDurationSeconds;
            public readonly float MinMoveSpeed;
            public readonly string Summary;

            public CommandoTempoSnapshot(
                bool isValid,
                float maxMoveDurationSeconds,
                float minMoveSpeed,
                string summary)
            {
                IsValid = isValid;
                MaxMoveDurationSeconds = maxMoveDurationSeconds;
                MinMoveSpeed = minMoveSpeed;
                Summary = summary;
            }
        }

        private readonly struct CommandoGroundingSnapshot
        {
            public readonly bool IsValid;
            public readonly float MinRootY;
            public readonly float MaxRootY;
            public readonly float StrideBobHeight;
            public readonly string Summary;

            public CommandoGroundingSnapshot(
                bool isValid,
                float minRootY,
                float maxRootY,
                float strideBobHeight,
                string summary)
            {
                IsValid = isValid;
                MinRootY = minRootY;
                MaxRootY = maxRootY;
                StrideBobHeight = strideBobHeight;
                Summary = summary;
            }

            public bool IsWithinTolerance =>
                IsValid && StrideBobHeight <= 0.001f && MinRootY >= -0.005f && MaxRootY <= 0.025f;
        }

        private readonly struct ActionCameraPosePrediction
        {
            public readonly bool HasPrediction;
            public readonly float YawDegrees;
            public readonly Vector3 BaseLocalToPackage;
            public readonly Vector3 BasePlayerBodyViewport;
            public readonly Vector3 FullAimLocalToPackage;
            public readonly Vector3 FullAimPlayerBodyViewport;
            public readonly bool BaseCentersPlayer;
            public readonly bool FullAimCentersPlayer;

            public ActionCameraPosePrediction(
                bool hasPrediction,
                float yawDegrees,
                Vector3 baseLocalToPackage,
                Vector3 basePlayerBodyViewport,
                Vector3 fullAimLocalToPackage,
                Vector3 fullAimPlayerBodyViewport,
                bool baseCentersPlayer,
                bool fullAimCentersPlayer)
            {
                HasPrediction = hasPrediction;
                YawDegrees = yawDegrees;
                BaseLocalToPackage = baseLocalToPackage;
                BasePlayerBodyViewport = basePlayerBodyViewport;
                FullAimLocalToPackage = fullAimLocalToPackage;
                FullAimPlayerBodyViewport = fullAimPlayerBodyViewport;
                BaseCentersPlayer = baseCentersPlayer;
                FullAimCentersPlayer = fullAimCentersPlayer;
            }
        }

        private readonly struct FinalHandoffBindingSnapshot
        {
            public readonly bool ObsoleteCombatStartVisualRemoved;
            public readonly bool BodyTrackFound;
            public readonly string BodyBindingPath;
            public readonly bool BodyTrackBoundToCombatPlayer;
            public readonly bool ActivationTrackFound;
            public readonly string ActivationBindingPath;
            public readonly bool ActivationTrackBoundToCombatPlayer;
            public readonly bool ActivationPostPlaybackActive;

            public FinalHandoffBindingSnapshot(
                bool obsoleteCombatStartVisualRemoved,
                bool bodyTrackFound,
                string bodyBindingPath,
                bool bodyTrackBoundToCombatPlayer,
                bool activationTrackFound,
                string activationBindingPath,
                bool activationTrackBoundToCombatPlayer,
                bool activationPostPlaybackActive)
            {
                ObsoleteCombatStartVisualRemoved = obsoleteCombatStartVisualRemoved;
                BodyTrackFound = bodyTrackFound;
                BodyBindingPath = bodyBindingPath;
                BodyTrackBoundToCombatPlayer = bodyTrackBoundToCombatPlayer;
                ActivationTrackFound = activationTrackFound;
                ActivationBindingPath = activationBindingPath;
                ActivationTrackBoundToCombatPlayer = activationTrackBoundToCombatPlayer;
                ActivationPostPlaybackActive = activationPostPlaybackActive;
            }
        }

        private readonly struct PlayerSwordOnlySnapshot
        {
            public readonly bool StartingModeMelee;
            public readonly bool RangedWeaponInactive;
            public readonly bool MeleeWeaponActive;
            public readonly bool SwordActive;
            public readonly bool ShieldsInactive;
            public readonly string RangedWeaponPath;
            public readonly string MeleeWeaponPath;

            public PlayerSwordOnlySnapshot(
                bool startingModeMelee,
                bool rangedWeaponInactive,
                bool meleeWeaponActive,
                bool swordActive,
                bool shieldsInactive,
                string rangedWeaponPath,
                string meleeWeaponPath)
            {
                StartingModeMelee = startingModeMelee;
                RangedWeaponInactive = rangedWeaponInactive;
                MeleeWeaponActive = meleeWeaponActive;
                SwordActive = swordActive;
                ShieldsInactive = shieldsInactive;
                RangedWeaponPath = rangedWeaponPath;
                MeleeWeaponPath = meleeWeaponPath;
            }

            public bool IsValid =>
                StartingModeMelee
                && RangedWeaponInactive
                && MeleeWeaponActive
                && SwordActive
                && ShieldsInactive;
        }

        private static PlayerSwordOnlySnapshot CapturePlayerSwordOnlySnapshot(
            GameObject playerRoot,
            PlayerCombatModeController combatModeController)
        {
            if (playerRoot == null || combatModeController == null)
            {
                return default;
            }

            SerializedObject serialized = new SerializedObject(combatModeController);
            serialized.Update();
            SerializedProperty startingMode = serialized.FindProperty("startingMode");
            bool startingModeMelee =
                startingMode != null && startingMode.enumValueIndex == (int)PlayerCombatMode.Melee;
            GameObject rangedWeaponRoot =
                ResolveGameObjectReference(GetObjectReferenceProperty(serialized, "rangedWeaponRoot"));
            GameObject meleeWeaponRoot =
                ResolveGameObjectReference(GetObjectReferenceProperty(serialized, "meleeWeaponRoot"));
            bool rangedWeaponInactive = rangedWeaponRoot != null && !rangedWeaponRoot.activeInHierarchy;
            bool meleeWeaponActive = meleeWeaponRoot != null && meleeWeaponRoot.activeInHierarchy;
            bool swordActive = HasExactNamedDescendantActiveInHierarchy(meleeWeaponRoot, "Weapon_Sword");
            bool shieldsInactive = true;
            for (int i = 0; i < PlayerShieldWeaponNames.Length; i++)
            {
                shieldsInactive &= !HasExactNamedDescendantActiveInHierarchy(
                    playerRoot,
                    PlayerShieldWeaponNames[i]);
            }

            return new PlayerSwordOnlySnapshot(
                startingModeMelee,
                rangedWeaponInactive,
                meleeWeaponActive,
                swordActive,
                shieldsInactive,
                FormatObjectReference(rangedWeaponRoot),
                FormatObjectReference(meleeWeaponRoot));
        }

        private static void AppendPlayerSwordOnlySnapshot(
            List<string> report,
            PlayerSwordOnlySnapshot snapshot)
        {
            report.Add("  playerSwordOnlyHandoff:");
            report.Add($"    startingModeMelee={snapshot.StartingModeMelee}");
            report.Add($"    rangedWeapon={snapshot.RangedWeaponPath} inactive={snapshot.RangedWeaponInactive}");
            report.Add($"    meleeWeapon={snapshot.MeleeWeaponPath} active={snapshot.MeleeWeaponActive}");
            report.Add($"    swordActive={snapshot.SwordActive}");
            report.Add($"    shieldsInactive={snapshot.ShieldsInactive}");
        }

        private static FinalHandoffBindingSnapshot CaptureFinalHandoffBinding(
            Scene scene,
            PlayableDirector director,
            TimelineAsset timeline,
            GameObject playerRoot)
        {
            bool obsoleteVisualRemoved = FindRootOrDescendant(scene, CombatStartVisualPlacementName) == null;
            AnimationTrack bodyTrack = FindOutputTrack<AnimationTrack>(timeline, CombatStartVisualBodyTrackName);
            ActivationTrack activationTrack =
                FindOutputTrack<ActivationTrack>(timeline, CombatStartVisualActivationTrackName);
            UnityEngine.Object bodyBinding = bodyTrack != null && director != null
                ? director.GetGenericBinding(bodyTrack)
                : null;
            UnityEngine.Object activationBinding = activationTrack != null && director != null
                ? director.GetGenericBinding(activationTrack)
                : null;

            bool bodyBoundToPlayer =
                playerRoot != null && IsObjectReferenceInHierarchy(bodyBinding, playerRoot.transform);
            bool activationBoundToPlayer =
                playerRoot != null && ResolveTransformReference(activationBinding) == playerRoot.transform;
            bool postPlaybackActive =
                activationTrack != null
                && activationTrack.postPlaybackState == ActivationTrack.PostPlaybackState.Active;

            return new FinalHandoffBindingSnapshot(
                obsoleteVisualRemoved,
                bodyTrack != null,
                FormatObjectReference(bodyBinding),
                bodyBoundToPlayer,
                activationTrack != null,
                FormatObjectReference(activationBinding),
                activationBoundToPlayer,
                postPlaybackActive);
        }

        private static CameraPhaseSnapshot CaptureCameraPhase(
            Scene scene,
            Transform packageRoot,
            Camera combatCamera,
            GameObject playerRoot)
        {
            List<Camera> activeEnabledCameras = FindActiveEnabledCameras(scene);
            Vector3 combatCameraLocal = combatCamera != null && packageRoot != null
                ? packageRoot.InverseTransformPoint(combatCamera.transform.position)
                : default;
            Vector3 playerBodyViewport = combatCamera != null && playerRoot != null
                ? combatCamera.WorldToViewportPoint(playerRoot.transform.position + Vector3.up * 1.05f)
                : default;
            bool hasPlayerSkinnedBounds =
                TryCalculateActiveRendererBounds(playerRoot, skinnedOnly: true, out Bounds playerSkinnedBounds);
            bool hasPlayerAllBounds =
                TryCalculateActiveRendererBounds(playerRoot, skinnedOnly: false, out Bounds playerAllBounds);
            Vector3 playerSkinnedCenterViewport = hasPlayerSkinnedBounds && combatCamera != null
                ? combatCamera.WorldToViewportPoint(playerSkinnedBounds.center)
                : default;
            Vector3 playerAllCenterViewport = hasPlayerAllBounds && combatCamera != null
                ? combatCamera.WorldToViewportPoint(playerAllBounds.center)
                : default;
            bool onlyCombatCameraEnabled =
                activeEnabledCameras.Count == 1 && activeEnabledCameras[0] == combatCamera;
            bool combatCameraCentersPlayer =
                playerBodyViewport.z > 0f && Math.Abs(playerBodyViewport.x - 0.5f) <= 0.055f;
            bool combatCameraCentersPlayerRenderer =
                hasPlayerSkinnedBounds
                && playerSkinnedCenterViewport.z > 0f
                && Math.Abs(playerSkinnedCenterViewport.x - 0.5f) <= 0.055f;

            return new CameraPhaseSnapshot(
                activeEnabledCameras,
                combatCameraLocal,
                playerBodyViewport,
                hasPlayerSkinnedBounds,
                playerSkinnedCenterViewport,
                hasPlayerAllBounds,
                playerAllCenterViewport,
                onlyCombatCameraEnabled,
                combatCameraCentersPlayer,
                combatCameraCentersPlayerRenderer);
        }

        private static ActionCameraPosePrediction CaptureActionCameraPosePrediction(
            Camera camera,
            Transform packageRoot,
            GameObject playerRoot)
        {
            ActionCameraController controller = camera != null
                ? camera.GetComponent<ActionCameraController>()
                : null;
            if (controller == null)
            {
                return default;
            }

            var serialized = new SerializedObject(controller);
            serialized.Update();
            return CaptureActionCameraPosePrediction(camera, controller, serialized, packageRoot, playerRoot);
        }

        private static ActionCameraPosePrediction CaptureActionCameraPosePrediction(
            Camera camera,
            ActionCameraController controller,
            SerializedObject serialized,
            Transform packageRoot,
            GameObject playerRoot)
        {
            Transform target = controller.Target != null ? controller.Target : playerRoot != null ? playerRoot.transform : null;
            if (target == null)
            {
                return default;
            }

            Vector3 cameraOffset = GetVector3Property(serialized, "cameraOffset");
            Vector3 lookOffset = GetVector3Property(serialized, "lookOffset");
            Vector3 aimCameraOffset = GetVector3Property(serialized, "aimCameraOffset");
            Vector3 aimFocusOffset = GetVector3Property(serialized, "aimFocusOffset");
            bool useFixedRearYaw = GetBoolProperty(serialized, "useFixedRearYaw");
            Transform fixedRearYawReference =
                ResolveTransformReference(GetObjectReferenceProperty(serialized, "fixedRearYawReference"));
            Transform yawReference = fixedRearYawReference != null ? fixedRearYawReference : target;
            float yawDegrees = useFixedRearYaw
                ? NormalizeYaw(yawReference.eulerAngles.y + GetFloatProperty(serialized, "fixedRearYawOffsetDegrees"))
                : NormalizeYaw(GetFloatProperty(serialized, "orbitYawDegrees"));
            Quaternion baseRotation = Quaternion.Euler(0f, yawDegrees, 0f);
            Vector3 baseFocus = target.position + lookOffset;
            Transform threat = controller.Threat;
            if (threat != null)
            {
                Vector3 threatOffset = Vector3.ProjectOnPlane(threat.position - target.position, Vector3.up)
                    * GetFloatProperty(serialized, "threatBias");
                baseFocus += Vector3.ClampMagnitude(
                    threatOffset,
                    GetFloatProperty(serialized, "maxThreatFocusOffset"));
            }

            baseFocus += Vector3.ProjectOnPlane(target.forward, Vector3.up)
                * GetFloatProperty(serialized, "maxLeadFromPlayerSpeed");

            Vector3 basePosition = baseFocus + baseRotation * cameraOffset;
            Quaternion baseLookRotation = BuildLookRotation(basePosition, baseFocus, camera.transform.rotation);
            Vector3 aimPosition = baseFocus + baseRotation * (cameraOffset + aimCameraOffset);
            Vector3 aimFocus = baseFocus + baseRotation * aimFocusOffset;
            Quaternion aimLookRotation = BuildLookRotation(aimPosition, aimFocus, camera.transform.rotation);
            Vector3 playerBodyPoint = playerRoot != null
                ? playerRoot.transform.position + Vector3.up * 1.05f
                : target.position + Vector3.up * 1.05f;
            Vector3 predictedBaseViewport =
                ProjectWorldPointWithTemporaryCameraPose(camera, basePosition, baseLookRotation, playerBodyPoint);
            Vector3 predictedAimViewport =
                ProjectWorldPointWithTemporaryCameraPose(camera, aimPosition, aimLookRotation, playerBodyPoint);
            Vector3 baseLocalToPackage = packageRoot != null
                ? packageRoot.InverseTransformPoint(basePosition)
                : basePosition;
            Vector3 aimLocalToPackage = packageRoot != null
                ? packageRoot.InverseTransformPoint(aimPosition)
                : aimPosition;
            bool baseCentersPlayer =
                predictedBaseViewport.z > 0f && Math.Abs(predictedBaseViewport.x - 0.5f) <= 0.055f;
            bool fullAimCentersPlayer =
                predictedAimViewport.z > 0f && Math.Abs(predictedAimViewport.x - 0.5f) <= 0.055f;

            return new ActionCameraPosePrediction(
                true,
                yawDegrees,
                baseLocalToPackage,
                predictedBaseViewport,
                aimLocalToPackage,
                predictedAimViewport,
                baseCentersPlayer,
                fullAimCentersPlayer);
        }

        private static void AppendCameraPhaseSnapshot(
            List<string> report,
            string cameraName,
            CameraPhaseSnapshot snapshot)
        {
            report.Add($"  {cameraName} localPosition={snapshot.CombatCameraLocalPosition:F3}");
            report.Add($"  {cameraName} playerBodyViewport={snapshot.PlayerBodyViewport:F3}");
            report.Add(
                $"  {cameraName} playerSkinnedCenterViewport={snapshot.PlayerSkinnedCenterViewport:F3} hasBounds={snapshot.HasPlayerSkinnedBounds}");
            report.Add(
                $"  {cameraName} playerAllRendererCenterViewport={snapshot.PlayerAllRendererCenterViewport:F3} hasBounds={snapshot.HasPlayerAllBounds}");
            report.Add($"  {cameraName} onlyCombatCameraEnabled={snapshot.OnlyCombatCameraEnabled}");
            report.Add($"  {cameraName} centersPlayerBody={snapshot.CombatCameraCentersPlayer}");
            report.Add($"  {cameraName} centersPlayerRenderer={snapshot.CombatCameraCentersPlayerRenderer}");
        }

        private static void AppendCameraPresentationSnapshot(
            List<string> report,
            string label,
            CameraPresentationSnapshot snapshot)
        {
            if (!snapshot.IsValid)
            {
                report.Add($"  {label}=<none>");
                return;
            }

            report.Add(
                $"  {label} path={snapshot.CameraPath} fov={snapshot.FieldOfView:0.###} clearFlags={snapshot.ClearFlags} hdr={snapshot.AllowHDR} msaa={snapshot.AllowMSAA}");
            report.Add(
                $"  {label} background={snapshot.BackgroundColor} near={snapshot.NearClipPlane:0.###} far={snapshot.FarClipPlane:0.###}");
            report.Add(
                $"  {label} urpData={snapshot.HasUniversalData} postProcess={snapshot.RenderPostProcessing} aa={snapshot.Antialiasing}/{snapshot.AntialiasingQuality}");
        }

        private static void AppendPlayerGroundSnapshot(
            List<string> report,
            string label,
            PlayerGroundSnapshot snapshot)
        {
            report.Add(
                $"  {label} valid={snapshot.IsValid} withinTolerance={snapshot.IsWithinGroundTolerance} groundY={snapshot.GroundY:0.###} resolvedMinY={snapshot.ResolvedMinY:0.###}");
            report.Add(
                $"  {label} rendererMinY={snapshot.RendererMinY:0.###} hasRendererBounds={snapshot.HasRendererBounds} controllerMinY={snapshot.ControllerMinY:0.###} hasControllerBounds={snapshot.HasControllerBounds}");
        }

        private static void AppendActionCameraPosePredictionSummary(
            List<string> report,
            string cameraName,
            ActionCameraPosePrediction prediction)
        {
            if (!prediction.HasPrediction)
            {
                report.Add($"  {cameraName} predictedActionPose=<none>");
                return;
            }

            report.Add(
                $"  {cameraName} predictedBase yaw={prediction.YawDegrees:0.###} localToPackage={prediction.BaseLocalToPackage:F3} playerBodyViewport={prediction.BasePlayerBodyViewport:F3} centersPlayer={prediction.BaseCentersPlayer}");
            report.Add(
                $"  {cameraName} predictedFullAim localToPackage={prediction.FullAimLocalToPackage:F3} playerBodyViewport={prediction.FullAimPlayerBodyViewport:F3} centersPlayer={prediction.FullAimCentersPlayer}");
        }

        private static void AppendCameraDiagnostics(
            List<string> report,
            string label,
            Scene scene,
            Transform packageRoot,
            GameObject playerRoot)
        {
            report.Add($"{label}:");
            Camera[] cameras = FindAllSceneCameras(scene);
            report.Add($"  allCameras={cameras.Length}");
            if (cameras.Length == 0)
            {
                report.Add("  <none>");
                return;
            }

            Vector3 playerBodyPoint = playerRoot != null
                ? playerRoot.transform.position + Vector3.up * 1.05f
                : default;
            bool hasSkinnedBounds =
                TryCalculateActiveRendererBounds(playerRoot, skinnedOnly: true, out Bounds skinnedBounds);
            bool hasAllBounds =
                TryCalculateActiveRendererBounds(playerRoot, skinnedOnly: false, out Bounds allBounds);

            for (int i = 0; i < cameras.Length; i++)
            {
                Camera camera = cameras[i];
                Transform cameraTransform = camera.transform;
                Vector3 localToPackage = packageRoot != null
                    ? packageRoot.InverseTransformPoint(cameraTransform.position)
                    : cameraTransform.localPosition;
                Vector3 playerBodyViewport = playerRoot != null
                    ? camera.WorldToViewportPoint(playerBodyPoint)
                    : default;
                Vector3 skinnedViewport = hasSkinnedBounds
                    ? camera.WorldToViewportPoint(skinnedBounds.center)
                    : default;
                Vector3 allViewport = hasAllBounds
                    ? camera.WorldToViewportPoint(allBounds.center)
                    : default;

                report.Add(
                    $"  {GetHierarchyPath(cameraTransform)} activeSelf={camera.gameObject.activeSelf} activeInHierarchy={camera.gameObject.activeInHierarchy} enabled={camera.enabled} tag={ReadTag(camera.gameObject)} depth={camera.depth:0.###} fov={camera.fieldOfView:0.###}");
                report.Add(
                    $"    localToPackage={FormatVector3(localToPackage)} local={FormatVector3(cameraTransform.localPosition)} worldPos={FormatVector3(cameraTransform.position)} worldEuler={FormatVector3(cameraTransform.eulerAngles)}");
                report.Add(
                    $"    playerBodyViewport={FormatVector3(playerBodyViewport)} playerSkinnedCenterViewport={FormatVector3(skinnedViewport)} hasSkinnedBounds={hasSkinnedBounds} playerAllRendererCenterViewport={FormatVector3(allViewport)} hasAllBounds={hasAllBounds}");
                AppendActionCameraControllerDiagnostics(report, camera, packageRoot, playerRoot);
            }
        }

        private static void AppendActionCameraControllerDiagnostics(
            List<string> report,
            Camera camera,
            Transform packageRoot,
            GameObject playerRoot)
        {
            ActionCameraController controller = camera.GetComponent<ActionCameraController>();
            if (controller == null)
            {
                return;
            }

            var serialized = new SerializedObject(controller);
            serialized.Update();
            report.Add("    ActionCameraController:");
            report.Add(
                $"      target={FormatObjectReference(controller.Target)} threat={FormatObjectReference(controller.Threat)}");
            report.Add(
                $"      cameraOffset={FormatVector3(GetVector3Property(serialized, "cameraOffset"))} lookOffset={FormatVector3(GetVector3Property(serialized, "lookOffset"))}");
            report.Add(
                $"      aimCameraOffset={FormatVector3(GetVector3Property(serialized, "aimCameraOffset"))} aimFocusOffset={FormatVector3(GetVector3Property(serialized, "aimFocusOffset"))}");
            report.Add(
                $"      useFixedRearYaw={GetBoolProperty(serialized, "useFixedRearYaw")} fixedRearYawReference={FormatObjectReference(GetObjectReferenceProperty(serialized, "fixedRearYawReference"))} fixedRearYawOffsetDegrees={GetFloatProperty(serialized, "fixedRearYawOffsetDegrees"):0.###}");
            report.Add(
                $"      orbitYawDegrees={GetFloatProperty(serialized, "orbitYawDegrees"):0.###} targetYawAssist={GetFloatProperty(serialized, "targetYawAssist"):0.###} threatBias={GetFloatProperty(serialized, "threatBias"):0.###} maxLeadFromPlayerSpeed={GetFloatProperty(serialized, "maxLeadFromPlayerSpeed"):0.###}");
            report.Add(
                $"      aimTargetWeight={GetFloatProperty(serialized, "aimTargetWeight"):0.###} aimWeight={GetFloatProperty(serialized, "aimWeight"):0.###} aimOrbitUsesInput={GetBoolProperty(serialized, "aimOrbitUsesInput")} aimOrbitRotatesCameraPosition={GetBoolProperty(serialized, "aimOrbitRotatesCameraPosition")} aimAssistUsesYawTarget={GetBoolProperty(serialized, "aimAssistUsesYawTarget")}");
            AppendPredictedActionCameraPoses(report, camera, controller, serialized, packageRoot, playerRoot);
        }

        private static void AppendPredictedActionCameraPoses(
            List<string> report,
            Camera camera,
            ActionCameraController controller,
            SerializedObject serialized,
            Transform packageRoot,
            GameObject playerRoot)
        {
            Transform target = controller.Target != null ? controller.Target : playerRoot != null ? playerRoot.transform : null;
            if (target == null)
            {
                return;
            }

            Vector3 cameraOffset = GetVector3Property(serialized, "cameraOffset");
            Vector3 lookOffset = GetVector3Property(serialized, "lookOffset");
            Vector3 aimCameraOffset = GetVector3Property(serialized, "aimCameraOffset");
            Vector3 aimFocusOffset = GetVector3Property(serialized, "aimFocusOffset");
            bool useFixedRearYaw = GetBoolProperty(serialized, "useFixedRearYaw");
            Transform fixedRearYawReference =
                ResolveTransformReference(GetObjectReferenceProperty(serialized, "fixedRearYawReference"));
            Transform yawReference = fixedRearYawReference != null ? fixedRearYawReference : target;
            float yawDegrees = useFixedRearYaw
                ? NormalizeYaw(yawReference.eulerAngles.y + GetFloatProperty(serialized, "fixedRearYawOffsetDegrees"))
                : NormalizeYaw(GetFloatProperty(serialized, "orbitYawDegrees"));
            Quaternion baseRotation = Quaternion.Euler(0f, yawDegrees, 0f);
            Vector3 baseFocus = target.position + lookOffset;
            Transform threat = controller.Threat;
            if (threat != null)
            {
                Vector3 threatOffset = Vector3.ProjectOnPlane(threat.position - target.position, Vector3.up)
                    * GetFloatProperty(serialized, "threatBias");
                baseFocus += Vector3.ClampMagnitude(
                    threatOffset,
                    GetFloatProperty(serialized, "maxThreatFocusOffset"));
            }

            baseFocus += Vector3.ProjectOnPlane(target.forward, Vector3.up)
                * GetFloatProperty(serialized, "maxLeadFromPlayerSpeed");

            Vector3 basePosition = baseFocus + baseRotation * cameraOffset;
            Quaternion baseLookRotation = BuildLookRotation(basePosition, baseFocus, camera.transform.rotation);
            Vector3 aimPosition = baseFocus + baseRotation * (cameraOffset + aimCameraOffset);
            Vector3 aimFocus = baseFocus + baseRotation * aimFocusOffset;
            Quaternion aimLookRotation = BuildLookRotation(aimPosition, aimFocus, camera.transform.rotation);
            Vector3 playerBodyPoint = playerRoot != null
                ? playerRoot.transform.position + Vector3.up * 1.05f
                : target.position + Vector3.up * 1.05f;
            Vector3 predictedBaseViewport =
                ProjectWorldPointWithTemporaryCameraPose(camera, basePosition, baseLookRotation, playerBodyPoint);
            Vector3 predictedAimViewport =
                ProjectWorldPointWithTemporaryCameraPose(camera, aimPosition, aimLookRotation, playerBodyPoint);
            Vector3 baseLocalToPackage = packageRoot != null
                ? packageRoot.InverseTransformPoint(basePosition)
                : basePosition;
            Vector3 aimLocalToPackage = packageRoot != null
                ? packageRoot.InverseTransformPoint(aimPosition)
                : aimPosition;

            report.Add(
                $"      predictedBase yaw={yawDegrees:0.###} localToPackage={FormatVector3(baseLocalToPackage)} playerBodyViewport={FormatVector3(predictedBaseViewport)}");
            report.Add(
                $"      predictedFullAim localToPackage={FormatVector3(aimLocalToPackage)} playerBodyViewport={FormatVector3(predictedAimViewport)}");
        }

        private static Quaternion BuildLookRotation(Vector3 position, Vector3 focus, Quaternion fallback)
        {
            Vector3 lookDirection = focus - position;
            return lookDirection.sqrMagnitude > 0.0001f
                ? Quaternion.LookRotation(lookDirection.normalized, Vector3.up)
                : fallback;
        }

        private static Vector3 ProjectWorldPointWithTemporaryCameraPose(
            Camera camera,
            Vector3 position,
            Quaternion rotation,
            Vector3 worldPoint)
        {
            Transform cameraTransform = camera.transform;
            Vector3 originalPosition = cameraTransform.position;
            Quaternion originalRotation = cameraTransform.rotation;
            cameraTransform.SetPositionAndRotation(position, rotation);
            Vector3 viewportPoint = camera.WorldToViewportPoint(worldPoint);
            cameraTransform.SetPositionAndRotation(originalPosition, originalRotation);
            return viewportPoint;
        }

        private static Vector3 GetVector3Property(SerializedObject serialized, string propertyName)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            return property != null ? property.vector3Value : default;
        }

        private static float GetFloatProperty(SerializedObject serialized, string propertyName)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            return property != null ? property.floatValue : 0f;
        }

        private static bool GetBoolProperty(SerializedObject serialized, string propertyName)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            return property != null && property.boolValue;
        }

        private static UnityEngine.Object GetObjectReferenceProperty(
            SerializedObject serialized,
            string propertyName)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            return property != null ? property.objectReferenceValue : null;
        }

        private static Transform ResolveTransformReference(UnityEngine.Object reference)
        {
            if (reference is Transform transform)
            {
                return transform;
            }

            if (reference is GameObject gameObject)
            {
                return gameObject.transform;
            }

            return reference is Component component ? component.transform : null;
        }

        private static GameObject ResolveGameObjectReference(UnityEngine.Object reference)
        {
            if (reference is GameObject gameObject)
            {
                return gameObject;
            }

            if (reference is Component component)
            {
                return component.gameObject;
            }

            return null;
        }

        private static void SetExactNamedDescendantsActive(
            GameObject root,
            string objectName,
            bool active)
        {
            if (root == null || string.IsNullOrWhiteSpace(objectName))
            {
                return;
            }

            Transform[] descendants = root.GetComponentsInChildren<Transform>(includeInactive: true);
            for (int i = 0; i < descendants.Length; i++)
            {
                Transform descendant = descendants[i];
                if (descendant != null && string.Equals(descendant.name, objectName, StringComparison.Ordinal))
                {
                    SetObjectActive(descendant.gameObject, active);
                }
            }
        }

        private static bool HasExactNamedDescendantActiveInHierarchy(
            GameObject root,
            string objectName)
        {
            if (root == null || string.IsNullOrWhiteSpace(objectName))
            {
                return false;
            }

            Transform[] descendants = root.GetComponentsInChildren<Transform>(includeInactive: true);
            for (int i = 0; i < descendants.Length; i++)
            {
                Transform descendant = descendants[i];
                if (descendant != null
                    && string.Equals(descendant.name, objectName, StringComparison.Ordinal)
                    && descendant.gameObject.activeInHierarchy)
                {
                    return true;
                }
            }

            return false;
        }

        private static GameObject[] RequireCutsceneCommandos(Scene scene)
        {
            var commandos = new GameObject[CutsceneCommandoNames.Length];
            for (int i = 0; i < CutsceneCommandoNames.Length; i++)
            {
                commandos[i] = RequireObjectInScene(scene, CutsceneCommandoNames[i]);
            }

            return commandos;
        }

        private static bool AllObjectsNamed(GameObject[] objects, string[] names)
        {
            if (objects == null || names == null || objects.Length != names.Length)
            {
                return false;
            }

            for (int i = 0; i < objects.Length; i++)
            {
                if (objects[i] == null || !string.Equals(objects[i].name, names[i], StringComparison.Ordinal))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool AllObjectsActiveInHierarchy(GameObject[] objects)
        {
            if (objects == null || objects.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < objects.Length; i++)
            {
                if (objects[i] == null || !objects[i].activeInHierarchy)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool AllCutsceneCommandoCombatBehavioursEnabled(GameObject[] commandos)
        {
            if (commandos == null || commandos.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < commandos.Length; i++)
            {
                GameObject commando = commandos[i];
                if (commando == null
                    || !IsBehaviourEnabled<CombatHealth>(commando)
                    || !IsBehaviourEnabled<CombatTargetSensor>(commando)
                    || !IsBehaviourEnabled<BasicSoldierEnemy>(commando))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool CutsceneCommandosAtCombatSlots(
            GameObject[] commandos,
            Transform packageRoot,
            out string summary)
        {
            summary = "<none>";
            if (commandos == null
                || packageRoot == null
                || commandos.Length != IntroSwordEnemyCombatSlotLocalPositions.Length)
            {
                return false;
            }

            var parts = new List<string>(commandos.Length);
            bool allAtSlots = true;
            for (int i = 0; i < commandos.Length; i++)
            {
                GameObject commando = commandos[i];
                if (commando == null)
                {
                    parts.Add("<null>");
                    allAtSlots = false;
                    continue;
                }

                Vector3 localPosition = packageRoot.InverseTransformPoint(commando.transform.position);
                Vector3 expected = IntroSwordEnemyCombatSlotLocalPositions[i];
                float planarDistance = Vector2.Distance(
                    new Vector2(localPosition.x, localPosition.z),
                    new Vector2(expected.x, expected.z));
                bool atSlot = planarDistance <= 0.18f && localPosition.z > 2.4f;
                allAtSlots &= atSlot;
                parts.Add(
                    $"{commando.name}:local={FormatVector3(localPosition)} expected={FormatVector3(expected)} planarDelta={planarDistance:0.###}");
            }

            summary = string.Join(" | ", parts);
            return allAtSlots;
        }

        private static CommandoTempoSnapshot CaptureCommandoTempoSnapshot(
            IntroGatePodInvasionBridgeCue invasionBridge)
        {
            if (invasionBridge == null)
            {
                return default;
            }

            IntroGatePodInvasionBridgeCue.CommandoCue[] commandos = invasionBridge.Commandos;
            if (commandos == null || commandos.Length == 0)
            {
                return default;
            }

            var parts = new List<string>(commandos.Length);
            float maxDuration = 0f;
            float minSpeed = float.PositiveInfinity;
            bool valid = true;
            for (int i = 0; i < commandos.Length; i++)
            {
                IntroGatePodInvasionBridgeCue.CommandoCue cue = commandos[i];
                float moveEndSeconds = !string.IsNullOrWhiteSpace(cue.AttackStateName)
                    && cue.AttackStartSeconds > cue.StartSeconds
                        ? cue.AttackStartSeconds
                        : cue.EndSeconds;
                float duration = Mathf.Max(0f, moveEndSeconds - cue.StartSeconds);
                float distance = Vector3.ProjectOnPlane(
                    cue.EndLocalPosition - cue.StartLocalPosition,
                    Vector3.up).magnitude;
                float speed = duration > 0.001f ? distance / duration : 0f;
                maxDuration = Mathf.Max(maxDuration, duration);
                minSpeed = Mathf.Min(minSpeed, speed);
                valid &= duration > 0.001f && speed > 0.001f;
                parts.Add(
                    $"{cue.Root?.name ?? "<null>"} duration={duration:0.###}s distance={distance:0.###} speed={speed:0.###}");
            }

            return new CommandoTempoSnapshot(
                valid,
                maxDuration,
                float.IsPositiveInfinity(minSpeed) ? 0f : minSpeed,
                string.Join(" | ", parts));
        }

        private static CommandoGroundingSnapshot CaptureCommandoGroundingSnapshot(
            IntroGatePodInvasionBridgeCue invasionBridge,
            Transform packageRoot)
        {
            if (invasionBridge == null)
            {
                return default;
            }

            IntroGatePodInvasionBridgeCue.CommandoCue[] commandos = invasionBridge.Commandos;
            if (commandos == null || commandos.Length == 0)
            {
                return default;
            }

            var serialized = new SerializedObject(invasionBridge);
            serialized.Update();
            SerializedProperty strideBobHeightProperty = serialized.FindProperty("commandoStrideBobHeight");
            float strideBobHeight = strideBobHeightProperty != null ? strideBobHeightProperty.floatValue : 0f;
            var parts = new List<string>(commandos.Length);
            bool valid = strideBobHeight <= 0.001f;
            float minRootY = float.PositiveInfinity;
            float maxRootY = float.NegativeInfinity;

            for (int i = 0; i < commandos.Length; i++)
            {
                IntroGatePodInvasionBridgeCue.CommandoCue cue = commandos[i];
                Transform root = cue.Root;
                if (root == null)
                {
                    valid = false;
                    parts.Add("<null>");
                    continue;
                }

                Transform localSpace = root.parent != null ? root.parent : root;
                Vector3 startWorld = localSpace.TransformPoint(cue.StartLocalPosition);
                Vector3 endWorld = localSpace.TransformPoint(cue.EndLocalPosition);
                Vector3 startPackageLocal = packageRoot != null
                    ? packageRoot.InverseTransformPoint(startWorld)
                    : cue.StartLocalPosition;
                Vector3 endPackageLocal = packageRoot != null
                    ? packageRoot.InverseTransformPoint(endWorld)
                    : cue.EndLocalPosition;
                float cueMinY = Mathf.Min(startPackageLocal.y, endPackageLocal.y);
                float cueMaxY = Mathf.Max(startPackageLocal.y, endPackageLocal.y);
                minRootY = Mathf.Min(minRootY, cueMinY);
                maxRootY = Mathf.Max(maxRootY, cueMaxY);
                parts.Add($"{root.name} rootY={cueMinY:0.###}..{cueMaxY:0.###}");
            }

            if (float.IsPositiveInfinity(minRootY) || float.IsNegativeInfinity(maxRootY))
            {
                valid = false;
                minRootY = 0f;
                maxRootY = 0f;
            }

            return new CommandoGroundingSnapshot(
                valid,
                minRootY,
                maxRootY,
                strideBobHeight,
                string.Join(" | ", parts));
        }

        private static float MinimumPlanarDistanceToPlayer(GameObject[] commandos, Transform playerRoot)
        {
            if (commandos == null || commandos.Length == 0 || playerRoot == null)
            {
                return 0f;
            }

            float minimumDistance = float.PositiveInfinity;
            for (int i = 0; i < commandos.Length; i++)
            {
                GameObject commando = commandos[i];
                if (commando == null)
                {
                    return 0f;
                }

                Vector3 offset = Vector3.ProjectOnPlane(
                    commando.transform.position - playerRoot.position,
                    Vector3.up);
                minimumDistance = Mathf.Min(minimumDistance, offset.magnitude);
            }

            return float.IsPositiveInfinity(minimumDistance) ? 0f : minimumDistance;
        }

        private static bool IsBehaviourEnabled<T>(GameObject gameObject)
            where T : Behaviour
        {
            T behaviour = gameObject != null ? gameObject.GetComponent<T>() : null;
            return behaviour != null && behaviour.enabled;
        }

        private static int CountObjectsWithNames(Scene scene, params string[] names)
        {
            int count = 0;
            for (int i = 0; i < names.Length; i++)
            {
                if (FindRootOrDescendant(scene, names[i]) != null)
                {
                    count++;
                }
            }

            return count;
        }

        private static string FormatObjectNames(GameObject[] objects)
        {
            if (objects == null || objects.Length == 0)
            {
                return "<none>";
            }

            var names = new List<string>(objects.Length);
            for (int i = 0; i < objects.Length; i++)
            {
                names.Add(objects[i] != null ? objects[i].name : "<null>");
            }

            return string.Join(", ", names);
        }

        private static bool IsObjectReferenceInHierarchy(UnityEngine.Object reference, Transform root)
        {
            Transform transform = ResolveTransformReference(reference);
            return transform != null && root != null && (transform == root || transform.IsChildOf(root));
        }

        private static string FormatObjectReference(UnityEngine.Object reference)
        {
            Transform transform = ResolveTransformReference(reference);
            if (transform != null)
            {
                return GetHierarchyPath(transform);
            }

            return reference != null ? reference.name : "<null>";
        }

        private static string FormatVector3(Vector3 value)
        {
            return $"({value.x:0.###}, {value.y:0.###}, {value.z:0.###})";
        }

        private static string ReadTag(GameObject gameObject)
        {
            try
            {
                return gameObject.tag;
            }
            catch (UnityException)
            {
                return "<missing>";
            }
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

        private static bool TryCalculateActiveRendererBounds(
            GameObject root,
            bool skinnedOnly,
            out Bounds bounds)
        {
            bounds = default;
            if (root == null)
            {
                return false;
            }

            bool hasBounds = false;
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(includeInactive: true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null
                    || !renderer.enabled
                    || !renderer.gameObject.activeInHierarchy
                    || (skinnedOnly && !(renderer is SkinnedMeshRenderer)))
                {
                    continue;
                }

                if (!hasBounds)
                {
                    bounds = renderer.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            return hasBounds;
        }

        private static float NormalizeYaw(float yaw)
        {
            yaw %= 360f;
            return yaw < 0f ? yaw + 360f : yaw;
        }

        private static double FindTrackClipEnd(TimelineAsset timeline, string trackName, double fallback)
        {
            if (timeline == null)
            {
                return fallback;
            }

            double end = fallback;
            foreach (TrackAsset track in timeline.GetOutputTracks())
            {
                if (track == null || !string.Equals(track.name, trackName, StringComparison.Ordinal))
                {
                    continue;
                }

                foreach (TimelineClip clip in track.GetClips())
                {
                    if (clip != null)
                    {
                        end = Math.Max(end, clip.end);
                    }
                }
            }

            return end;
        }

        private static double FindTrackClipEnd(TrackAsset track, double fallback)
        {
            if (track == null)
            {
                return fallback;
            }

            double end = fallback;
            foreach (TimelineClip clip in track.GetClips())
            {
                if (clip != null)
                {
                    end = Math.Max(end, clip.end);
                }
            }

            return end;
        }

        private static bool RemoveTimelineTrack(
            TimelineAsset timeline,
            string trackName,
            PlayableDirector director)
        {
            if (timeline == null || string.IsNullOrWhiteSpace(trackName))
            {
                return false;
            }

            var matches = new List<TrackAsset>();
            foreach (TrackAsset track in timeline.GetOutputTracks())
            {
                if (track != null && string.Equals(track.name, trackName, StringComparison.Ordinal))
                {
                    matches.Add(track);
                }
            }

            for (int i = 0; i < matches.Count; i++)
            {
                if (director != null)
                {
                    director.SetGenericBinding(matches[i], null);
                }

                timeline.DeleteTrack(matches[i]);
            }

            return matches.Count > 0;
        }

        private static void DeleteClipsByDisplayName(TrackAsset track, string displayName)
        {
            if (track == null)
            {
                return;
            }

            var matches = new List<TimelineClip>();
            foreach (TimelineClip clip in track.GetClips())
            {
                if (clip != null && string.Equals(clip.displayName, displayName, StringComparison.Ordinal))
                {
                    matches.Add(clip);
                }
            }

            for (int i = 0; i < matches.Count; i++)
            {
                track.DeleteClip(matches[i]);
            }
        }

        private static void DeleteAllTimelineClips(TrackAsset track)
        {
            if (track == null)
            {
                return;
            }

            var clips = new List<TimelineClip>();
            foreach (TimelineClip clip in track.GetClips())
            {
                if (clip != null)
                {
                    clips.Add(clip);
                }
            }

            for (int i = 0; i < clips.Count; i++)
            {
                track.DeleteClip(clips[i]);
            }
        }

        private static bool TrackHasPostExtrapolation(
            TimelineAsset timeline,
            string trackName,
            TimelineClip.ClipExtrapolation extrapolation)
        {
            if (timeline == null)
            {
                return false;
            }

            foreach (TrackAsset track in timeline.GetOutputTracks())
            {
                if (track == null || !string.Equals(track.name, trackName, StringComparison.Ordinal))
                {
                    continue;
                }

                foreach (TimelineClip clip in track.GetClips())
                {
                    if (clip != null && GetTimelineClipPostExtrapolation(clip).Equals(extrapolation))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static void SetTimelineClipPostExtrapolation(
            TimelineClip clip,
            TimelineClip.ClipExtrapolation extrapolation)
        {
            if (clip == null)
            {
                return;
            }

            const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
            typeof(TimelineClip).GetField("m_PostExtrapolationMode", Flags)?.SetValue(clip, extrapolation);
            typeof(TimelineClip).GetField("m_PostExtrapolationTime", Flags)?.SetValue(clip, double.PositiveInfinity);
        }

        private static TimelineClip.ClipExtrapolation GetTimelineClipPostExtrapolation(TimelineClip clip)
        {
            if (clip == null)
            {
                return TimelineClip.ClipExtrapolation.None;
            }

            const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
            FieldInfo field = typeof(TimelineClip).GetField("m_PostExtrapolationMode", Flags);
            return field != null
                ? (TimelineClip.ClipExtrapolation)field.GetValue(clip)
                : TimelineClip.ClipExtrapolation.None;
        }

        private static GameObject CreateRoot(Scene scene, string name, Vector3 position, Quaternion rotation)
        {
            GameObject root = new GameObject(name);
            SceneManager.MoveGameObjectToScene(root, scene);
            root.transform.SetPositionAndRotation(position, rotation);
            root.transform.localScale = Vector3.one;
            return root;
        }

        private static GameObject CreateChild(
            Transform parent,
            string name,
            Vector3 position,
            Quaternion rotation)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(parent, worldPositionStays: true);
            child.transform.SetPositionAndRotation(position, rotation);
            child.transform.localScale = Vector3.one;
            return child;
        }

        private static Quaternion ResolveFlatRotation(Transform source)
        {
            Vector3 forward = Vector3.ProjectOnPlane(source.forward, Vector3.up);
            if (forward.sqrMagnitude <= 0.0001f)
            {
                forward = Vector3.forward;
            }

            return Quaternion.LookRotation(forward.normalized, Vector3.up);
        }

        private static T LoadRequired<T>(string assetPath) where T : UnityEngine.Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (asset == null)
            {
                throw new InvalidOperationException($"Missing required asset: {assetPath}");
            }

            return asset;
        }

        private static T RequireComponent<T>(GameObject root, string label) where T : Component
        {
            T component = root.GetComponent<T>() ?? root.GetComponentInChildren<T>(includeInactive: true);
            if (component == null)
            {
                throw new InvalidOperationException($"Missing {typeof(T).Name} on {label}.");
            }

            return component;
        }

        private static void InvokePrivate(object target, string methodName)
        {
            MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null)
            {
                throw new InvalidOperationException($"Missing private method `{methodName}` on {target.GetType().Name}.");
            }

            method.Invoke(target, null);
        }

        private static GameObject RequireChildObject(Transform parent, string childName)
        {
            GameObject child = FindDirectChildObject(parent, childName);
            if (child == null)
            {
                throw new InvalidOperationException($"Missing child `{childName}` under `{parent.name}`.");
            }

            return child;
        }

        private static GameObject FindDirectChildObject(Transform parent, string childName)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (child != null && string.Equals(child.name, childName, StringComparison.Ordinal))
                {
                    return child.gameObject;
                }
            }

            return null;
        }

        private static GameObject RequireObjectInScene(Scene scene, string objectName)
        {
            GameObject found = FindRootOrDescendant(scene, objectName);
            if (found == null)
            {
                throw new InvalidOperationException($"Missing `{objectName}` in {scene.path}.");
            }

            return found;
        }

        private static T FindObjectByName<T>(Scene scene, string objectName) where T : Component
        {
            GameObject found = FindRootOrDescendant(scene, objectName);
            return found != null ? found.GetComponent<T>() : null;
        }

        private static GameObject FindRootOrDescendant(Scene scene, string objectName)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                GameObject root = roots[i];
                if (root == null)
                {
                    continue;
                }

                if (string.Equals(root.name, objectName, StringComparison.Ordinal))
                {
                    return root;
                }

                Transform found = FindDescendant(root.transform, objectName);
                if (found != null)
                {
                    return found.gameObject;
                }
            }

            return null;
        }

        private static Transform FindDescendant(Transform root, string objectName)
        {
            for (int i = 0; i < root.childCount; i++)
            {
                Transform child = root.GetChild(i);
                if (child == null)
                {
                    continue;
                }

                if (string.Equals(child.name, objectName, StringComparison.Ordinal))
                {
                    return child;
                }

                Transform nested = FindDescendant(child, objectName);
                if (nested != null)
                {
                    return nested;
                }
            }

            return null;
        }

        private static void RemoveRootIfPresent(Scene scene, string rootName)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = roots.Length - 1; i >= 0; i--)
            {
                GameObject root = roots[i];
                if (root != null && string.Equals(root.name, rootName, StringComparison.Ordinal))
                {
                    UnityEngine.Object.DestroyImmediate(root);
                }
            }
        }

        private static void SetObjectReference(UnityEngine.Object target, string propertyName, UnityEngine.Object value)
        {
            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(propertyName)
                ?? throw new InvalidOperationException($"{target.name} has no serialized property `{propertyName}`.");
            property.objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetVector3Property(SerializedObject serialized, string propertyName, Vector3 value)
        {
            SerializedProperty property = serialized.FindProperty(propertyName)
                ?? throw new InvalidOperationException($"{serialized.targetObject.name} has no serialized property `{propertyName}`.");
            property.vector3Value = value;
        }

        private static void SetFloatProperty(SerializedObject serialized, string propertyName, float value)
        {
            SerializedProperty property = serialized.FindProperty(propertyName)
                ?? throw new InvalidOperationException($"{serialized.targetObject.name} has no serialized property `{propertyName}`.");
            property.floatValue = value;
        }

        private static void SetBoolProperty(SerializedObject serialized, string propertyName, bool value)
        {
            SerializedProperty property = serialized.FindProperty(propertyName)
                ?? throw new InvalidOperationException($"{serialized.targetObject.name} has no serialized property `{propertyName}`.");
            property.boolValue = value;
        }

    }
}
