using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using DimensionBrawl.Combat;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.Player;
using DimensionBrawl.Presentation;
using DimensionBrawl.UI.StageClear;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace DimensionBrawl.Tests
{
    public sealed class OlympusCorridorCombatFlowPlayModeTests
    {
        private const string ScenePath = "Assets/_Game/Scenes/OlympusCorridorInvasionStage.unity";
        private const string StationScenePath = "Assets/_Game/Scenes/OlympusStationCombatStage.unity";
        private const string DirectorName = "IntroGatePodReview_TimelineDirector";
        private const string FlowRootName = "OlympusCorridor_CombatFlowRoot";
        private const string PlayerRootName = "Player_CombatGirl_ActionFoundation";
        private const string PlayerVisualRootName = "CombatGirlSwordShield_PlayerVisual";
        private const string CombatPackageRootName = "OlympusCorridor_BossBarrageCombatPackage";
        private const string CombatCameraName = "OlympusCorridor_Combat_MainCamera";
        private const string IntroSwordGateRootName = "OlympusCorridor_IntroSwordGate";
        private const string CombatHudRootName = "PF_UI_CombatHud";
        private const string PlayerRevealCameraRigRootName = "IntroGatePodReview_PlayerRevealCameraRig";
        private const string CutsceneCinemachineShotsRootName = "IntroGatePodReview_CinemachineShots";
        private const string BombingPreludeRootName = "IntroGatePodBombingPrelude_Olympus";
        private const string CutsceneCueDirectorRootName = "IntroGatePodReview_CueDirector";
        private const string FirstPersonRendererMaskRootName = "IntroGatePodReview_FirstPersonRendererMask";
        private const string TutorialTimingReportPath = "C:/tmp/DimensionBrawl-OlympusTutorialTimingReport.md";
        private const string TutorialAimFovReportPath = "C:/tmp/DimensionBrawl-OlympusAimFovReport.md";
        private const string FullRouteReportPath = "C:/tmp/DimensionBrawl-OlympusCanonicalFullRouteReport.md";
        private const float ExpectedMinimumTutorialStepSeconds = 0.85f;

        [Test]
        public void CorridorFlowAndTutorialDoNotRetainLegacyHudFallbackFields()
        {
            const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

            Assert.IsNull(
                typeof(OlympusCorridorCombatFlowController).GetField("reviewHud", PrivateInstance));
            Assert.IsNull(
                typeof(OlympusCorridorCombatFlowController).GetField("mobileHud", PrivateInstance));
            Assert.IsNull(
                typeof(OlympusCorridorTutorialDirector).GetField("mobileHud", PrivateInstance));
        }

        [Test]
        public void TutorialDialogueAudioCueDefaultsCoverEveryGuideLine()
        {
            OlympusCorridorTutorialDirector.DialogueAudioCue[] cues =
                OlympusCorridorTutorialDirector.CreateDefaultDialogueAudioCueSlots();

            Assert.That(cues.Length, Is.EqualTo(13));
            Assert.That(cues[0].CueId, Is.EqualTo(OlympusCorridorTutorialDirector.DialogueAudioCueId.SoldierChallenge));
            Assert.That(cues[1].CueId, Is.EqualTo(OlympusCorridorTutorialDirector.DialogueAudioCueId.MeleeCue));
            Assert.That(cues[2].CueId, Is.EqualTo(OlympusCorridorTutorialDirector.DialogueAudioCueId.MoveCue));
            Assert.That(cues[3].CueId, Is.EqualTo(OlympusCorridorTutorialDirector.DialogueAudioCueId.SwapToRangedCue));
            Assert.That(cues[4].CueId, Is.EqualTo(OlympusCorridorTutorialDirector.DialogueAudioCueId.FireCue));
            Assert.That(cues[5].CueId, Is.EqualTo(OlympusCorridorTutorialDirector.DialogueAudioCueId.DodgeCue));
            Assert.That(cues[6].CueId, Is.EqualTo(OlympusCorridorTutorialDirector.DialogueAudioCueId.ClearTargetsCue));
            Assert.That(cues[7].CueId, Is.EqualTo(OlympusCorridorTutorialDirector.DialogueAudioCueId.MeleeConfirm));
            Assert.That(cues[8].CueId, Is.EqualTo(OlympusCorridorTutorialDirector.DialogueAudioCueId.MoveConfirm));
            Assert.That(cues[9].CueId, Is.EqualTo(OlympusCorridorTutorialDirector.DialogueAudioCueId.SwapToRangedConfirm));
            Assert.That(cues[10].CueId, Is.EqualTo(OlympusCorridorTutorialDirector.DialogueAudioCueId.FireConfirm));
            Assert.That(cues[11].CueId, Is.EqualTo(OlympusCorridorTutorialDirector.DialogueAudioCueId.DodgeConfirm));
            Assert.That(cues[12].CueId, Is.EqualTo(OlympusCorridorTutorialDirector.DialogueAudioCueId.ClearTargetsConfirm));
        }

        [Test]
        public void TutorialOverlayBackdropOnlyHighlightsActionPrompts()
        {
            var presenterObject = new GameObject("Tutorial Overlay Backdrop Test");
            try
            {
                OlympusTutorialOverlayPresenter presenter =
                    presenterObject.AddComponent<OlympusTutorialOverlayPresenter>();

                presenter.Show(
                    "천계관리시스템",
                    "조이스틱을 사용해 파란 영역 안에서 이동할 수 있습니다.",
                    "이동",
                    OlympusTutorialOverlayPresenter.FocusKind.MoveStick,
                    new Vector2(0.16f, 0.16f));

                Assert.IsTrue(presenter.CurrentFocusBackdropActive);

                presenter.SetGuideState(OlympusTutorialOverlayPresenter.GuideState.Ready);
                Assert.IsTrue(presenter.CurrentFocusBackdropActive);

                presenter.SetGuideState(OlympusTutorialOverlayPresenter.GuideState.Confirmed);
                Assert.IsFalse(presenter.CurrentFocusBackdropActive);

                presenter.Show(
                    "천계관리시스템",
                    "남은 적을 처치하십시오.",
                    "전투 완료",
                    OlympusTutorialOverlayPresenter.FocusKind.Route,
                    new Vector2(0.5f, 0.76f));

                Assert.IsFalse(presenter.CurrentFocusBackdropActive);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(presenterObject);
            }
        }

        [Test]
        public void TutorialOverlayDialoguePanelSitsAboveScreenCenter()
        {
            var presenterObject = new GameObject("Tutorial Overlay Panel Position Test");
            try
            {
                OlympusTutorialOverlayPresenter presenter =
                    presenterObject.AddComponent<OlympusTutorialOverlayPresenter>();

                presenter.Show(
                    "천계관리시스템",
                    "근접 공격 버튼을 사용해 가까운 적을 공격할 수 있습니다.",
                    "근접 공격",
                    OlympusTutorialOverlayPresenter.FocusKind.MeleeAttack,
                    new Vector2(0.92f, 0.10f));

                Rect panelRect = presenter.CurrentDialoguePanelGuiRect;
                Assert.Less(panelRect.center.y, Screen.height * 0.5f);
                Assert.GreaterOrEqual(panelRect.yMin, 0f);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(presenterObject);
            }
        }

        [UnitySetUp]
        public IEnumerator LoadOlympusCorridorScene()
        {
            Time.timeScale = 1f;
            EditorSceneManager.LoadSceneInPlayMode(ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator ResetTimeScale()
        {
            Time.timeScale = 1f;
            yield return null;
        }

        [UnityTest]
        public IEnumerator IntroDirectorStopAfterTimeResetStartsCombatHandoff()
        {
            PlayableDirector director =
                RequireComponent<PlayableDirector>(DirectorName, "Olympus intro PlayableDirector");
            GameObject playerRoot = RequireSceneObject(PlayerRootName);
            GameObject introSwordGateRoot = RequireSceneObject(IntroSwordGateRootName);
            OlympusCorridorCombatFlowController flowController =
                RequireComponent<OlympusCorridorCombatFlowController>(
                    FlowRootName,
                    "Olympus corridor combat flow controller");

            Assert.IsNotNull(flowController);
            Assert.IsFalse(playerRoot.activeInHierarchy, "Player should stay hidden during the intro opening.");

            director.Play();
            yield return null;

            director.time = 0d;
            director.Stop();
            yield return null;
            yield return null;

            Assert.IsTrue(playerRoot.activeSelf, "Player activeSelf should be restored after intro director stop.");
            Assert.IsTrue(playerRoot.activeInHierarchy, "Player should be visible after intro director stop.");
            Assert.IsTrue(introSwordGateRoot.activeInHierarchy, "Intro sword gate combat should start after director stop.");
        }

        [UnityTest]
        public IEnumerator IntroDirectorEndEvaluationKeepsCombatPlayerActive()
        {
            PlayableDirector director =
                RequireComponent<PlayableDirector>(DirectorName, "Olympus intro PlayableDirector");
            GameObject playerRoot = RequireSceneObject(PlayerRootName);
            GameObject introSwordGateRoot = RequireSceneObject(IntroSwordGateRootName);
            OlympusCorridorCombatFlowController flowController =
                RequireComponent<OlympusCorridorCombatFlowController>(
                    FlowRootName,
                    "Olympus corridor combat flow controller");

            Assert.IsNotNull(flowController);
            Assert.IsFalse(playerRoot.activeInHierarchy, "Player should stay hidden during the intro opening.");

            director.Play();
            yield return null;

            director.time = System.Math.Max(0d, director.duration - 0.01d);
            director.Evaluate();
            yield return null;
            yield return null;

            Assert.IsTrue(playerRoot.activeSelf, "Player root activeSelf should be true at intro tail.");
            Assert.IsTrue(playerRoot.activeInHierarchy, "Player root should be active in hierarchy at intro tail.");
            Assert.IsTrue(introSwordGateRoot.activeInHierarchy, "Intro sword gate should be active at intro tail.");

            director.time = director.duration;
            director.Evaluate();
            director.Stop();
            yield return null;
            yield return null;

            Assert.IsTrue(playerRoot.activeSelf, "Player root activeSelf should stay true after director end.");
            Assert.IsTrue(playerRoot.activeInHierarchy, "Player root should stay active after director end.");
            Assert.IsTrue(introSwordGateRoot.activeInHierarchy, "Intro sword gate should stay active after director end.");
        }

        [UnityTest]
        public IEnumerator IntroNaturalTailPauseReleasesTimelineAndRestoresPlayerPoseBeforeHandoff()
        {
            PlayableDirector director =
                RequireComponent<PlayableDirector>(DirectorName, "Olympus intro PlayableDirector");
            OlympusCorridorCombatFlowController flowController =
                RequireComponent<OlympusCorridorCombatFlowController>(
                    FlowRootName,
                    "Olympus corridor combat flow controller");
            Transform playerRoot = RequireSceneObject(PlayerRootName).transform;
            Transform playerVisualRoot = RequireSceneObject(PlayerVisualRootName).transform;
            Vector3 expectedPlayerLocalPosition = playerRoot.localPosition;
            Quaternion expectedPlayerLocalRotation = playerRoot.localRotation;
            Vector3 expectedVisualLocalPosition = playerVisualRoot.localPosition;
            Quaternion expectedVisualLocalRotation = playerVisualRoot.localRotation;

            director.time = director.duration;
            director.Evaluate();
            director.Pause();

            Assert.IsTrue(
                director.playableGraph.IsValid(),
                "The reproduction requires the Timeline graph to remain held at its natural tail.");

            yield return null;
            yield return null;

            Assert.IsTrue(
                flowController.TutorialRunning,
                "The held natural tail should still hand off into the tutorial.");
            Assert.IsFalse(
                director.playableGraph.IsValid(),
                "Gameplay handoff must release a held Timeline graph so it cannot keep driving player transforms.");
            Assert.That(
                Vector3.Distance(
                    Vector3.ProjectOnPlane(playerRoot.localPosition, Vector3.up),
                    Vector3.ProjectOnPlane(expectedPlayerLocalPosition, Vector3.up)),
                Is.LessThanOrEqualTo(0.001f),
                "The player root must begin gameplay at its canonical authored planar pose.");
            Assert.That(
                Quaternion.Angle(playerRoot.localRotation, expectedPlayerLocalRotation),
                Is.LessThanOrEqualTo(0.1f),
                "The player root rotation must match the canonical authored pose.");
            Assert.That(
                Vector3.Distance(playerVisualRoot.localPosition, expectedVisualLocalPosition),
                Is.LessThanOrEqualTo(0.001f),
                "The Timeline-bound player visual must not retain the cutscene tail offset.");
            Assert.That(
                Quaternion.Angle(playerVisualRoot.localRotation, expectedVisualLocalRotation),
                Is.LessThanOrEqualTo(0.1f),
                "The Timeline-bound player visual must not retain the cutscene tail rotation.");
        }

        [UnityTest]
        public IEnumerator IntroPlayerRevealDoesNotMoveGameplayRootBeforeHandoff()
        {
            PlayableDirector director =
                RequireComponent<PlayableDirector>(DirectorName, "Olympus intro PlayableDirector");
            Transform playerRoot = RequireSceneObject(PlayerRootName).transform;
            Vector3 expectedWorldPosition = playerRoot.position;
            Quaternion expectedWorldRotation = playerRoot.rotation;

            director.time = 29d;
            director.Evaluate();

            Assert.IsTrue(
                playerRoot.gameObject.activeInHierarchy,
                "The Timeline reproduction point should activate the gameplay player for the reveal shot.");

            yield return null;
            yield return null;

            Assert.That(
                Vector3.Distance(playerRoot.position, expectedWorldPosition),
                Is.LessThanOrEqualTo(0.001f),
                "The reveal shot must not let gameplay movement or lane clamping relocate the player root.");
            Assert.That(
                Quaternion.Angle(playerRoot.rotation, expectedWorldRotation),
                Is.LessThanOrEqualTo(0.1f),
                "The reveal shot must preserve the authored gameplay facing until handoff.");
        }

        [UnityTest]
        public IEnumerator CombatHudJoystickIsMutedBeforeIntroHandoff()
        {
            CanvasGroup combatHudCanvasGroup = RequireComponent<CanvasGroup>(
                CombatHudRootName,
                "combat HUD root canvas group");

            yield return null;

            AssertCombatHudInputMuted(combatHudCanvasGroup, "before intro handoff");
        }

        [UnityTest]
        public IEnumerator CombatHudJoystickUnlocksOnlyAfterHudReveal()
        {
            OlympusCorridorCombatFlowController flowController =
                RequireComponent<OlympusCorridorCombatFlowController>(
                    FlowRootName,
                    "Olympus corridor combat flow controller");
            CanvasGroup combatHudCanvasGroup = RequireComponent<CanvasGroup>(
                CombatHudRootName,
                "combat HUD root canvas group");

            flowController.SkipIntroCutscene();
            AssertCombatHudInputMuted(combatHudCanvasGroup, "immediately after intro skip");

            float startedAt = Time.realtimeSinceStartup;
            while (!combatHudCanvasGroup.interactable || !combatHudCanvasGroup.blocksRaycasts)
            {
                Assert.Less(
                    Time.realtimeSinceStartup - startedAt,
                    2f,
                    "Combat HUD joystick never unlocked after the HUD reveal.");
                yield return null;
            }

            Assert.That(combatHudCanvasGroup.alpha, Is.EqualTo(1f).Within(0.001f));
        }

        [UnityTest]
        public IEnumerator IntroOnlyPresentationLeavesTheRuntimeAfterHandoff()
        {
            OlympusCorridorCombatFlowController flowController =
                RequireComponent<OlympusCorridorCombatFlowController>(
                    FlowRootName,
                    "Olympus corridor combat flow controller");
            GameObject revealCameraRig = RequireSceneObject(PlayerRevealCameraRigRootName);
            GameObject cinemachineShots = RequireSceneObject(CutsceneCinemachineShotsRootName);
            GameObject bombingPrelude = RequireSceneObject(BombingPreludeRootName);
            Behaviour cueDirector = RequireBehaviourByTypeName(
                CutsceneCueDirectorRootName,
                "DimensionBrawl.Presentation.IntroGatePodCutsceneCueDirector");
            Behaviour rendererMask = RequireBehaviourByTypeName(
                FirstPersonRendererMaskRootName,
                "DimensionBrawl.Presentation.IntroGatePodFirstPersonRendererMask");
            Behaviour introBrain = RequireBehaviourByTypeName(
                "Main Camera",
                "Unity.Cinemachine.CinemachineBrain");
            Camera introCamera = RequireComponent<Camera>("Main Camera", "intro camera");
            Camera combatCamera = RequireComponent<Camera>(CombatCameraName, "combat camera");
            float introFieldOfView = introCamera.fieldOfView;
            CameraClearFlags introClearFlags = introCamera.clearFlags;
            Color introBackground = introCamera.backgroundColor;

            Assert.IsTrue(revealCameraRig.activeInHierarchy);
            Assert.IsTrue(cinemachineShots.activeInHierarchy);
            Assert.IsTrue(bombingPrelude.activeInHierarchy);
            Assert.IsTrue(cueDirector.enabled);
            Assert.IsTrue(rendererMask.enabled);
            Assert.IsTrue(introBrain.enabled);

            flowController.SkipIntroCutscene();
            Assert.That(combatCamera.fieldOfView, Is.EqualTo(introFieldOfView).Within(0.001f));
            Assert.AreEqual(introClearFlags, combatCamera.clearFlags);
            Assert.That(
                Vector4.Distance(introBackground, combatCamera.backgroundColor),
                Is.LessThanOrEqualTo(0.001f));
            yield return null;
            yield return null;

            Assert.IsFalse(revealCameraRig.activeSelf);
            Assert.IsFalse(cinemachineShots.activeSelf);
            Assert.IsFalse(bombingPrelude.activeSelf);
            Assert.IsFalse(cueDirector.enabled);
            Assert.IsFalse(rendererMask.enabled);
            Assert.IsFalse(introBrain.enabled);
        }

        [UnityTest]
        public IEnumerator CorridorCombatKeepsAuthoredBoundsAndTargets()
        {
            OlympusCorridorCombatFlowController flowController =
                RequireComponent<OlympusCorridorCombatFlowController>(
                    FlowRootName,
                    "Olympus corridor combat flow controller");
            GameObject boundsRoot = RequireSceneObject("OlympusCorridor_CorridorCombatBounds");
            GameObject stairTrigger = RequireSceneObject("OlympusCorridor_StairToCorridorCombatTrigger");
            GameObject traversalSupport = RequireSceneObject("OlympusCorridor_IntroStairTraversalSupport");
            GameObject[] boundsRoots = ReadPrivateField<GameObject[]>(flowController, "corridorBoundsRoots");
            CombatHealth[] targets = ReadPrivateField<CombatHealth[]>(flowController, "corridorTargets");
            CombatHealth[] clearTargets = ReadPrivateField<CombatHealth[]>(flowController, "corridorClearTargets");
            Transform stairTriggerCenter = ReadPrivateField<Transform>(flowController, "stairTriggerCenter");
            float stairTriggerRadius = ReadPrivateField<float>(flowController, "stairTriggerRadius");
            OlympusStageClearOverlay stageClearOverlay =
                ReadPrivateField<OlympusStageClearOverlay>(flowController, "stageClearOverlay");
            AudioClip combatPhaseBgmClip =
                ReadPrivateField<AudioClip>(flowController, "combatPhaseBgmClip");

            Assert.That(boundsRoots, Has.Length.EqualTo(1));
            Assert.AreSame(boundsRoot, boundsRoots[0]);
            Assert.IsFalse(boundsRoot.activeSelf, "Corridor bounds should stay dormant until corridor combat starts.");
            Assert.That(boundsRoot.GetComponentsInChildren<BoxCollider>(true), Has.Length.EqualTo(4));
            Assert.That(targets, Has.Length.EqualTo(2));
            Assert.That(clearTargets, Has.Length.EqualTo(1));
            Assert.AreSame(clearTargets[0], targets[0]);
            Assert.AreEqual(DamageTeam.Enemy, targets[0].Team);
            Assert.AreEqual(DamageTeam.Enemy, targets[1].Team);
            Assert.AreSame(stairTrigger.transform, stairTriggerCenter);
            Assert.That(stairTriggerRadius, Is.EqualTo(2.75f).Within(0.001f));
            Assert.IsNotNull(stageClearOverlay);
            Assert.AreSame(flowController.gameObject, stageClearOverlay.gameObject);
            Assert.IsNotNull(combatPhaseBgmClip);
            Assert.IsTrue(stairTrigger.GetComponent<SphereCollider>().isTrigger);
            Assert.IsNotNull(traversalSupport.GetComponent<BoxCollider>());
            yield return null;
        }

        [UnityTest]
        public IEnumerator TutorialSceneAssignsInstructionVoiceClips()
        {
            OlympusCorridorCombatFlowController flowController =
                RequireComponent<OlympusCorridorCombatFlowController>(
                    FlowRootName,
                    "Olympus corridor combat flow controller");

            yield return null;

            OlympusCorridorTutorialDirector.DialogueAudioCue[] cues =
                ReadPrivateField<OlympusCorridorTutorialDirector.DialogueAudioCue[]>(
                    flowController,
                    "tutorialOverlayDialogueAudioCues");

            Assert.That(cues.Length, Is.EqualTo(13));
            for (int i = 0; i <= 6; i++)
            {
                Assert.IsNotNull(cues[i].Clip, $"Tutorial instruction voice cue {cues[i].CueId} should be assigned.");
            }

            for (int i = 7; i < cues.Length; i++)
            {
                Assert.IsNull(cues[i].Clip, $"Fast confirmation cue {cues[i].CueId} should stay silent.");
            }
        }

        [UnityTest]
        public IEnumerator TutorialStartsWithReadableSoldierChallenge()
        {
            OlympusCorridorCombatFlowController flowController =
                RequireComponent<OlympusCorridorCombatFlowController>(
                    FlowRootName,
                    "Olympus corridor combat flow controller");

            flowController.SkipIntroCutscene();
            yield return null;
            yield return null;

            OlympusCorridorTutorialDirector tutorialDirector =
                RequireComponent<OlympusCorridorTutorialDirector>(
                    FlowRootName,
                    "Olympus corridor tutorial director");
            OlympusTutorialOverlayPresenter overlayPresenter =
                RequireComponent<OlympusTutorialOverlayPresenter>(
                    FlowRootName,
                    "Olympus tutorial overlay presenter");

            Assert.AreEqual("SoldierChallenge", tutorialDirector.CurrentStepId);
            Assert.AreEqual("Cue", tutorialDirector.CurrentPhaseId);
            Assert.AreEqual("병사", ReadPrivateField<string>(overlayPresenter, "speaker"));
            Assert.AreEqual("뭐하는 놈이냐!", ReadPrivateField<string>(overlayPresenter, "dialogue"));

            float startedAt = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - startedAt < 1.1f)
            {
                yield return null;
            }

            Assert.AreEqual(
                "SoldierChallenge",
                tutorialDirector.CurrentStepId,
                "Soldier challenge should stay readable before the first system instruction.");
        }

        [UnityTest]
        public IEnumerator TutorialCueRejectsStaleMoveAndCombatInput()
        {
            var report = new StringBuilder();
            OlympusCorridorCombatFlowController flowController =
                RequireComponent<OlympusCorridorCombatFlowController>(
                    FlowRootName,
                    "Olympus corridor combat flow controller");
            GameObject playerRoot = RequireSceneObject(PlayerRootName);
            PlayerMovementController player = RequireComponent<PlayerMovementController>(
                PlayerRootName,
                "player movement controller");
            PlayerActionController actionController = RequireComponent<PlayerActionController>(
                PlayerRootName,
                "player action controller");
            PlayerCombatModeController combatModeController = RequireComponent<PlayerCombatModeController>(
                PlayerRootName,
                "player combat mode controller");
            PlayerRangedBasicAttackAction rangedBasicAttackAction =
                RequireComponent<PlayerRangedBasicAttackAction>(
                    PlayerRootName,
                    "player ranged basic action");
            PlayerSkill1Action skill1Action = RequireComponent<PlayerSkill1Action>(
                PlayerRootName,
                "player skill 1 action");
            PlayerSummonSlot1Action summonSlot1Action = RequireComponent<PlayerSummonSlot1Action>(
                PlayerRootName,
                "player summon slot 1 action");
            PlayerSupportSummonSlotAction[] supportSummonActions =
                playerRoot.GetComponentsInChildren<PlayerSupportSummonSlotAction>(true);

            flowController.SkipIntroCutscene();
            yield return null;
            yield return null;

            OlympusCorridorTutorialDirector tutorialDirector =
                RequireComponent<OlympusCorridorTutorialDirector>(
                    FlowRootName,
                    "Olympus corridor tutorial director");

            yield return WaitForStep(tutorialDirector, "Melee", 5f, report);
            Assert.AreEqual("Cue", tutorialDirector.CurrentPhaseId, "Tutorial should begin with a locked cue window.");

            player.SetMoveInput(Vector2.right);
            player.SetLookInput(Vector2.right);
            actionController.QueueBasicAttack();
            actionController.QueueDodge();
            combatModeController.QueueCombatModeSwap();
            rangedBasicAttackAction.QueueFire();
            rangedBasicAttackAction.SetFireHeld(true);
            rangedBasicAttackAction.SetExternalAimPreviewHeld(true);
            rangedBasicAttackAction.SetAimInput(Vector2.right);
            skill1Action.QueueSkill1();
            summonSlot1Action.QueueSummonSlot1();
            for (int i = 0; i < supportSummonActions.Length; i++)
            {
                supportSummonActions[i].QueueSummon();
            }

            AssertPrivateVector2Zero(player, "mobileMoveInput", "move input should not be stored during a tutorial cue.");
            AssertPrivateVector2Zero(player, "mobileLookInput", "look input should not be stored during a tutorial cue.");
            AssertPrivateBoolFalse(actionController, "mobileAttackQueued", "basic attack should not queue during a tutorial cue.");
            AssertPrivateBoolFalse(actionController, "mobileDodgeQueued", "dodge should not queue during a tutorial cue.");
            AssertPrivateBoolFalse(combatModeController, "queuedSwap", "combat mode swap should not queue during a tutorial cue.");
            AssertPrivateBoolFalse(rangedBasicAttackAction, "queuedFire", "ranged fire should not queue while disabled or locked.");
            AssertPrivateBoolFalse(rangedBasicAttackAction, "mobileFireHeld", "held fire should not persist while disabled or locked.");
            AssertPrivateBoolFalse(rangedBasicAttackAction, "currentFireHeld", "held fire should not become active while disabled or locked.");
            AssertPrivateBoolFalse(rangedBasicAttackAction, "externalAimPreviewHeld", "aim preview should not persist while disabled or locked.");
            AssertPrivateVector2Zero(rangedBasicAttackAction, "aimInput", "aim input should not persist while disabled or locked.");
            AssertPrivateBoolFalse(skill1Action, "queued", "Skill1 should not queue while disabled.");
            AssertPrivateBoolFalse(summonSlot1Action, "queued", "SummonSlot1 should not queue while disabled.");
            for (int i = 0; i < supportSummonActions.Length; i++)
            {
                AssertPrivateBoolFalse(
                    supportSummonActions[i],
                    "queued",
                    $"Support summon slot {i + 2} should not queue while disabled.");
            }

            yield return null;

            Assert.AreEqual("Melee", tutorialDirector.CurrentStepId);
            Assert.IsTrue(combatModeController.IsMeleeMode, "Rejected early inputs should not break the initial sword lock.");
        }

        [UnityTest]
        public IEnumerator DesktopInputActionsAndKeyboardFallbackKeysAreAuthored()
        {
            OlympusCorridorCombatFlowController flowController =
                RequireComponent<OlympusCorridorCombatFlowController>(
                    FlowRootName,
                    "Olympus corridor combat flow controller");
            PlayerMovementController player = RequireComponent<PlayerMovementController>(
                PlayerRootName,
                "player movement controller");
            PlayerActionController actionController = RequireComponent<PlayerActionController>(
                PlayerRootName,
                "player action controller");
            PlayerRangedBasicAttackAction rangedAttack =
                RequireComponent<PlayerRangedBasicAttackAction>(
                    PlayerRootName,
                    "player ranged basic action");
            PlayerSkill1Action skill1Action = RequireComponent<PlayerSkill1Action>(
                PlayerRootName,
                "player skill 1 action");
            PlayerCombatModeController combatMode = RequireComponent<PlayerCombatModeController>(
                PlayerRootName,
                "player combat mode controller");
            PlayerLockTargetController lockTarget = RequireComponent<PlayerLockTargetController>(
                PlayerRootName,
                "player lock target controller");
            PlayerSummonSlot1Action summonSlot1 = RequireComponent<PlayerSummonSlot1Action>(
                PlayerRootName,
                "player summon slot 1 action");
            PlayerSupportSummonSlotAction[] supportSummons =
                RequireSceneObject(PlayerRootName).GetComponents<PlayerSupportSummonSlotAction>();

            InputActionReference moveAction =
                ReadPrivateField<InputActionReference>(player, "moveAction");
            InputActionReference attackAction =
                ReadPrivateField<InputActionReference>(actionController, "basicAttackAction");
            InputActionReference dodgeAction =
                ReadPrivateField<InputActionReference>(actionController, "dodgeAction");
            InputActionReference skillAction =
                ReadPrivateField<InputActionReference>(skill1Action, "skillAction");

            AssertInputActionBinding(
                moveAction,
                "Move",
                "<Keyboard>/w",
                "<Keyboard>/a",
                "<Keyboard>/s",
                "<Keyboard>/d");
            AssertInputActionBinding(
                attackAction,
                "Attack",
                "<Mouse>/leftButton",
                "<Keyboard>/f");
            AssertInputActionBindingAbsent(
                attackAction,
                "Attack",
                "<Keyboard>/enter",
                "<Touchscreen>/primaryTouch/tap");
            AssertInputActionBinding(
                dodgeAction,
                "Dodge",
                "<Keyboard>/space",
                "<Keyboard>/leftShift");
            AssertInputActionBinding(skillAction, "Skill1", "<Keyboard>/r");
            Assert.AreSame(
                attackAction,
                ReadPrivateField<InputActionReference>(rangedAttack, "fireAction"),
                "Melee and ranged mode must consume the same authored Attack action reference.");
            Assert.IsFalse(
                ReadPrivateField<bool>(rangedAttack, "manageFireActionLifecycle"),
                "The always-enabled melee controller must be the sole lifecycle owner of the shared Attack action.");

            flowController.SkipIntroCutscene();
            yield return null;
            yield return null;
            Assert.IsTrue(attackAction.action.enabled);
            rangedAttack.enabled = false;
            Assert.IsTrue(
                attackAction.action.enabled,
                "Disabling the ranged adapter must not disable the shared melee Attack action.");
            rangedAttack.enabled = true;
            Assert.IsTrue(attackAction.action.enabled);

            Assert.That(ReadPrivateField<Key>(rangedAttack, "keyboardTestKey"), Is.EqualTo(Key.F));
            Assert.That(ReadPrivateField<Key>(skill1Action, "keyboardTestKey"), Is.EqualTo(Key.R));
            Assert.That(ReadPrivateField<Key>(combatMode, "keyboardTestKey"), Is.EqualTo(Key.Tab));
            Assert.That(ReadPrivateField<Key>(lockTarget, "keyboardFocusKey"), Is.EqualTo(Key.T));
            Assert.That(ReadPrivateField<Key>(summonSlot1, "keyboardTestKey"), Is.EqualTo(Key.Digit1));
            Assert.That(supportSummons.Length, Is.EqualTo(2));

            var supportKeys = new HashSet<Key>();
            for (int i = 0; i < supportSummons.Length; i++)
            {
                supportKeys.Add(ReadPrivateField<Key>(supportSummons[i], "keyboardTestKey"));
            }

            CollectionAssert.AreEquivalent(new[] { Key.Digit2, Key.Digit3 }, supportKeys);
            yield return null;
        }

        [UnityTest]
        [Timeout(90000)]
        public IEnumerator TutorialRuntimeInputsAdvanceByExpectedTriggers()
        {
            var report = new StringBuilder();
            OlympusCorridorCombatFlowController flowController =
                RequireComponent<OlympusCorridorCombatFlowController>(
                    FlowRootName,
                    "Olympus corridor combat flow controller");
            GameObject playerRoot = RequireSceneObject(PlayerRootName);
            PlayerMovementController player = RequireComponent<PlayerMovementController>(
                PlayerRootName,
                "player movement controller");
            PlayerActionController actionController = RequireComponent<PlayerActionController>(
                PlayerRootName,
                "player action controller");
            PlayerCombatModeController combatModeController = RequireComponent<PlayerCombatModeController>(
                PlayerRootName,
                "player combat mode controller");
            PlayerRangedBasicAttackAction rangedBasicAttackAction =
                RequireComponent<PlayerRangedBasicAttackAction>(
                    PlayerRootName,
                    "player ranged basic action");
            PlayerRangedAimController rangedAimController =
                RequireComponent<PlayerRangedAimController>(
                    PlayerRootName,
                    "player ranged aim controller");
            ActionCameraController cameraController =
                RequireComponent<ActionCameraController>(
                    "OlympusCorridor_Combat_MainCamera",
                    "combat action camera controller");
            Camera combatCamera = RequireComponent<Camera>(
                "OlympusCorridor_Combat_MainCamera",
                "combat camera");

            report.AppendLine("# Olympus Corridor Tutorial Runtime Timing");
            report.AppendLine();
            report.AppendLine("- Loaded scene in PlayMode.");
            report.AppendLine("- Used the flow skip handoff, then runtime player input queues.");
            report.AppendLine("- PASS means each tutorial step advanced from its intended trigger.");
            report.AppendLine();

            Assert.IsFalse(playerRoot.activeInHierarchy, "Player should be hidden before intro handoff.");
            flowController.SkipIntroCutscene();
            yield return null;
            yield return null;
            AppendCameraSnapshot(report, "After skip handoff", cameraController, combatCamera, rangedAimController);
            Assert.AreSame(combatCamera, Camera.main, "Combat camera should be the active MainCamera after intro handoff.");
            Assert.That(
                ReadPrivateField<float>(cameraController, "baseFieldOfView"),
                Is.GreaterThan(45f),
                "Combat camera aim base FOV should stay on the authored gameplay value instead of inheriting the intro handoff lens.");

            OlympusCorridorTutorialDirector tutorialDirector =
                RequireComponent<OlympusCorridorTutorialDirector>(
                    FlowRootName,
                    "Olympus corridor tutorial director");

            Assert.IsTrue(playerRoot.activeInHierarchy, "Player should be active after intro handoff.");
            yield return WaitForStep(tutorialDirector, "Melee", 5f, report);
            Assert.AreEqual("Melee", tutorialDirector.CurrentStepId);
            Assert.IsTrue(combatModeController.IsMeleeMode, "Tutorial must begin in sword/melee mode.");
            report.AppendLine($"- Handoff step: `{tutorialDirector.CurrentStepId}`, mode `{combatModeController.CurrentMode}`.");

            combatModeController.QueueCombatModeSwap();
            yield return null;
            yield return null;
            Assert.AreEqual("Melee", tutorialDirector.CurrentStepId, "Early swap input must not advance before the swap step.");
            Assert.IsTrue(combatModeController.IsMeleeMode, "Early swap input must not break the initial sword lock.");
            report.AppendLine("- Early swap guard: stayed in `Melee` and `Melee` mode.");

            yield return QueueBasicAttackUntilStep(
                tutorialDirector,
                actionController,
                "Move",
                "melee hit",
                8f,
                report);
            Assert.AreEqual("Move", tutorialDirector.CurrentStepId);
            Assert.That(tutorialDirector.LastCompletionRecord, Does.StartWith("Melee:"));
            AssertBossTelegraphsSuppressed("move step after melee local-defense cue");
            report.AppendLine("- Boss telegraph suppression: presenters disabled during the tutorial pocket.");

            yield return MoveUntilStep(
                tutorialDirector,
                player,
                flowController,
                combatCamera,
                "SwapToRanged",
                8f,
                report);
            Assert.AreEqual("SwapToRanged", tutorialDirector.CurrentStepId);
            Assert.That(tutorialDirector.LastCompletionRecord, Does.StartWith("Move:"));
            Assert.IsTrue(combatModeController.IsMeleeMode, "Swap step should start from sword mode.");
            float preFireFieldOfView = combatCamera.fieldOfView;
            Vector3 preFireCameraPosition = combatCamera.transform.position;
            AppendCameraSnapshot(report, "Before ranged swap", cameraController, combatCamera, rangedAimController);

            yield return QueueSwapUntilStep(
                tutorialDirector,
                combatModeController,
                "Fire",
                5f,
                report);
            Assert.AreEqual("Fire", tutorialDirector.CurrentStepId);
            Assert.That(tutorialDirector.LastCompletionRecord, Does.StartWith("SwapToRanged:"));
            Assert.IsTrue(combatModeController.IsRangedMode, "Fire step should enter ranged mode only after the swap input.");
            AssertPrivateBoolFalse(
                rangedBasicAttackAction,
                "externalAimPreviewHeld",
                "Fire tutorial must use the real held-fire path, not the external aim preview shortcut.");
            Assert.IsFalse(rangedAimController.IsAiming, "Fire cue should not auto-aim before the fire button is held.");
            yield return WaitForPhase(tutorialDirector, "AwaitingAction", 2f, report);
            rangedBasicAttackAction.SetFireHeld(true);
            yield return null;
            AppendCameraSnapshot(report, "Held fire aim started", cameraController, combatCamera, rangedAimController);
            report.AppendLine();
            report.AppendLine("## Held Fire Aim FOV Trace");
            report.AppendLine("| Frame | Time | Combat FOV | Main Camera | Base FOV | Aim Target | Aim Weight | Is Aiming |");
            report.AppendLine("|---:|---:|---:|---|---:|---:|---:|---|");
            float minFireFieldOfView = combatCamera.fieldOfView;
            float fireAimSettleDeadline = Time.realtimeSinceStartup + 2f;
            int fireAimFrame = 0;
            do
            {
                yield return null;
                minFireFieldOfView = Mathf.Min(minFireFieldOfView, combatCamera.fieldOfView);
                fireAimFrame++;
                AppendCameraFovSample(report, fireAimFrame, cameraController, combatCamera, rangedAimController);
            }
            while ((ReadPrivateField<float>(cameraController, "aimWeight") <= 0.75f
                    || minFireFieldOfView >= preFireFieldOfView - 3f
                    || Vector3.Distance(preFireCameraPosition, combatCamera.transform.position) <= 0.35f)
                && Time.realtimeSinceStartup < fireAimSettleDeadline);

            Assert.IsTrue(rangedAimController.IsAiming, "Fire cue should put the player ranged aim controller into aim mode.");
            Assert.That(
                ReadPrivateField<float>(cameraController, "aimWeight"),
                Is.GreaterThan(0.75f),
                "Held fire should drive the camera aim weight high enough to be visible.");
            Assert.Less(
                minFireFieldOfView,
                preFireFieldOfView - 3f,
                "Held fire should visibly tighten FOV through the normal combat aim path.");
            float fireAimCameraShift = Vector3.Distance(preFireCameraPosition, combatCamera.transform.position);
            report.AppendLine($"- Fire aim camera shift: `{fireAimCameraShift:0.00}m`.");
            Assert.Greater(
                fireAimCameraShift,
                0.35f,
                "Held fire should move the visible combat camera through the normal combat aim path, not only change FOV.");
            AppendCameraSnapshot(report, "Fire aim settled", cameraController, combatCamera, rangedAimController);
            AppendAndAssertAimCameraLaneComposition(report, "Fire aim settled", combatCamera, player);
            yield return WaitForPhase(tutorialDirector, "Committed", 3f, report);
            Assert.AreEqual("Fire", tutorialDirector.CurrentStepId);
            Assert.IsTrue(rangedAimController.IsAiming, "Held-fire aim should stay active through the Fire confirmation beat.");
            Assert.That(
                ReadPrivateField<float>(cameraController, "aimWeight"),
                Is.GreaterThan(0.5f),
                "Fire confirmation should not clear the normal held-fire camera aim.");
            rangedBasicAttackAction.QueueFire();
            yield return null;
            AssertPrivateBoolFalse(
                rangedBasicAttackAction,
                "queuedFire",
                "Fire confirmation should preserve held aim without accepting fresh fire queues.");
            report.AppendLine("- Fire confirmation held-fire aim: still active.");

            yield return WaitForStep(tutorialDirector, "Dodge", 3f, report);
            rangedBasicAttackAction.SetFireHeld(false);
            Assert.AreEqual("Dodge", tutorialDirector.CurrentStepId);
            Assert.That(tutorialDirector.LastCompletionRecord, Does.StartWith("Fire:"));

            yield return QueueDodgeUntilStep(
                tutorialDirector,
                actionController,
                player,
                flowController,
                combatCamera,
                "ClearTargets",
                5f,
                report);
            Assert.AreEqual("ClearTargets", tutorialDirector.CurrentStepId);
            Assert.That(tutorialDirector.LastCompletionRecord, Does.StartWith("Dodge:"));
            AppendCameraSnapshot(report, "ClearTargets step reached", cameraController, combatCamera, rangedAimController);
            yield return WaitForPhase(tutorialDirector, "AwaitingAction", 2f, report);
            float clearStepPreFireFieldOfView = combatCamera.fieldOfView;
            rangedBasicAttackAction.SetFireHeld(true);
            float clearStepMinFireFieldOfView = combatCamera.fieldOfView;
            for (int i = 0; i < 45; i++)
            {
                yield return null;
                clearStepMinFireFieldOfView = Mathf.Min(clearStepMinFireFieldOfView, combatCamera.fieldOfView);
            }

            Assert.IsTrue(rangedAimController.IsAiming, "ClearTargets should keep normal held-fire aim available for cleanup enemies.");
            Assert.Less(
                clearStepMinFireFieldOfView,
                clearStepPreFireFieldOfView - 2f,
                "ClearTargets held fire should use the normal combat aim zoom.");
            AppendCameraSnapshot(report, "ClearTargets held fire aim", cameraController, combatCamera, rangedAimController);
            AppendAndAssertAimCameraLaneComposition(report, "ClearTargets held fire aim", combatCamera, player);

            CombatHealth[] tutorialTargets =
                ReadPrivateField<CombatHealth[]>(tutorialDirector, "tutorialTargets");
            Assert.That(tutorialTargets.Length, Is.GreaterThan(0));
            Scene corridorSceneBeforeTutorialCompletion = SceneManager.GetActiveScene();
            int playerInstanceIdBeforeTutorialCompletion = player.gameObject.GetInstanceID();
            string runIdBeforeTutorialCompletion = flowController.CanonicalStageRunId;
            Vector3 playerPositionBeforeTutorialCompletion = player.transform.position;
            for (int i = 0; i < tutorialTargets.Length; i++)
            {
                CombatHealth target = tutorialTargets[i];
                Assert.IsNotNull(target, $"Tutorial target {i} should exist before clear completion.");
                Assert.IsTrue(target.gameObject.activeSelf, $"Tutorial target {i} should start active.");
                target.TryApplyDamage(new DamageInfo(
                    null,
                    DamageTeam.Player,
                    target.MaxHealth + 999f,
                    target.transform.position,
                    Vector3.forward,
                    0f,
                    DamageResponsePolicy.DamageOnly));
            }

            yield return WaitForStep(tutorialDirector, "Completed", 4f, report);
            rangedBasicAttackAction.SetFireHeld(false);
            for (int i = 0; i < tutorialTargets.Length; i++)
            {
                if (tutorialTargets[i] != null)
                {
                    Assert.IsFalse(
                        tutorialTargets[i].gameObject.activeSelf,
                        $"Tutorial target {i} should be hidden after tutorial completion.");
                }
            }

            report.AppendLine();
            report.AppendLine("## Static Step Gates");
            report.AppendLine("- Cue phase: at least `0.85s` focus/read window with step input muted.");
            report.AppendLine("- AwaitingAction phase: only live observer events can commit completion.");
            report.AppendLine("- Committed phase: at least `1.15s` RECORDED confirmation before the next cue; Fire keeps held-fire aim alive during this beat.");
            report.AppendLine("- Move gate: `0.75m` confirmed position movement inside the tutorial area.");
            report.AppendLine("- Fire gate: `0.7s` real aim preview hold after Ready + fire event + player-side target damage/death.");
            report.AppendLine("- Clear gate: all tutorial targets defeated.");
            yield return WalkDownAuthoredStairsThroughJoystick(
                flowController,
                player,
                combatCamera,
                corridorSceneBeforeTutorialCompletion,
                playerInstanceIdBeforeTutorialCompletion,
                runIdBeforeTutorialCompletion,
                playerPositionBeforeTutorialCompletion,
                report,
                18f);
            report.AppendLine($"- Tutorial completion and stair traversal scene: `{SceneManager.GetActiveScene().path}`.");
            Directory.CreateDirectory(Path.GetDirectoryName(TutorialTimingReportPath));
            File.WriteAllText(TutorialTimingReportPath, report.ToString());
        }

        [UnityTest]
        [Timeout(90000)]
        public IEnumerator CanonicalFullRouteCompletesTutorialStationGuideVictoryAndReplay()
        {
            var report = new StringBuilder();
            report.AppendLine("# Olympus Canonical Full Route");
            report.AppendLine();
            report.AppendLine("- Corridor intro uses the production skip handoff.");
            report.AppendLine("- Tutorial steps use runtime action queues and the EventSystem joystick path.");
            report.AppendLine("- Station entry guide advances through its public request surface.");
            report.AppendLine("- Victory commits a Clear result and Replay uses the real typed-action button listener.");
            report.AppendLine();

            OlympusCorridorCombatFlowController flow =
                RequireComponent<OlympusCorridorCombatFlowController>(
                    FlowRootName,
                    "Olympus corridor combat flow controller");
            PlayerMovementController player = RequireComponent<PlayerMovementController>(
                PlayerRootName,
                "player movement controller");
            PlayerActionController actionController = RequireComponent<PlayerActionController>(
                PlayerRootName,
                "player action controller");
            PlayerCombatModeController combatModeController = RequireComponent<PlayerCombatModeController>(
                PlayerRootName,
                "player combat mode controller");
            PlayerRangedBasicAttackAction rangedAttack = RequireComponent<PlayerRangedBasicAttackAction>(
                PlayerRootName,
                "player ranged basic action");
            Camera combatCamera = RequireComponent<Camera>(
                CombatCameraName,
                "combat camera");

            flow.SkipIntroCutscene();
            yield return null;
            yield return null;

            OlympusCorridorTutorialDirector tutorial =
                RequireComponent<OlympusCorridorTutorialDirector>(
                    FlowRootName,
                    "Olympus corridor tutorial director");
            yield return WaitForStep(tutorial, "Melee", 5f, report);
            yield return QueueBasicAttackUntilStep(
                tutorial,
                actionController,
                "Move",
                "melee hit",
                8f,
                report);
            yield return MoveUntilStep(
                tutorial,
                player,
                flow,
                combatCamera,
                "SwapToRanged",
                8f,
                report);
            yield return QueueSwapUntilStep(tutorial, combatModeController, "Fire", 5f, report);
            yield return WaitForPhase(tutorial, "AwaitingAction", 3f, report);

            rangedAttack.SetFireHeld(true);
            yield return WaitForStep(tutorial, "Dodge", 6f, report);
            rangedAttack.SetFireHeld(false);
            yield return QueueDodgeUntilStep(
                tutorial,
                actionController,
                player,
                flow,
                combatCamera,
                "ClearTargets",
                5f,
                report);
            yield return WaitForPhase(tutorial, "AwaitingAction", 3f, report);

            CombatHealth[] tutorialTargets =
                ReadPrivateField<CombatHealth[]>(tutorial, "tutorialTargets");
            Assert.That(tutorialTargets, Is.Not.Empty);
            Scene corridorSceneBeforeTutorialCompletion = SceneManager.GetActiveScene();
            int playerInstanceIdBeforeTutorialCompletion = player.gameObject.GetInstanceID();
            string runIdBeforeTutorialCompletion = flow.CanonicalStageRunId;
            Vector3 playerPositionBeforeTutorialCompletion = player.transform.position;
            for (int i = 0; i < tutorialTargets.Length; i++)
            {
                CombatHealth target = tutorialTargets[i];
                Assert.That(target, Is.Not.Null);
                Assert.That(ApplyLethalDamage(target, DamageTeam.Player), Is.True);
            }

            yield return WalkDownAuthoredStairsThroughJoystick(
                flow,
                player,
                combatCamera,
                corridorSceneBeforeTutorialCompletion,
                playerInstanceIdBeforeTutorialCompletion,
                runIdBeforeTutorialCompletion,
                playerPositionBeforeTutorialCompletion,
                report,
                18f);
            report.AppendLine("- Player walked down the authored stairs into lower combat without a scene load.");

            Behaviour stationGuide = RequireActiveSceneBehaviour(
                "DimensionBrawl.LevelDesign.OlympusStationCombatIntroTutorialBridge");
            yield return CompleteStationGuide(stationGuide, report, 12f);
            yield return new WaitForSecondsRealtime(0.05f);

            CombatEncounterController encounter =
                UnityEngine.Object.FindFirstObjectByType<CombatEncounterController>();
            Assert.That(encounter, Is.Not.Null);
            Assert.That(encounter.UsesCoordinatedTerminalResolution, Is.True);
            Assert.That(
                encounter.IsRunning,
                Is.True,
                $"Station encounter was not running before authored victory. "
                + $"Won={encounter.IsWon}, Failed={encounter.IsFailed}, Faulted={encounter.IsFaulted}, "
                + $"Diagnostic={encounter.Diagnostic.Reason}: {encounter.Diagnostic.Message}");
            CombatHealth bossHealth = ReadPrivateField<CombatHealth>(encounter, "enemyHealth");
            Assert.That(ApplyLethalDamage(bossHealth, DamageTeam.Player), Is.True);
            Assert.That(encounter.IsWon, Is.True);
            report.AppendLine("- Station encounter committed authored victory.");

            StageClearScreenPresenter clearPresenter = null;
            float clearDeadline = Time.realtimeSinceStartup + 8f;
            while (clearPresenter == null || !IsStageClearInteractive(clearPresenter))
            {
                Assert.Less(
                    Time.realtimeSinceStartup,
                    clearDeadline,
                    "Timed out waiting for the additive product result surface.");
                clearPresenter = FindSingleStageClearPresenter();
                yield return null;
            }

            Button retryButton = ReadPrivateField<Button>(clearPresenter, "retryButton");
            Assert.That(clearPresenter.IsConfigured, Is.True, clearPresenter.LastActionError);
            Assert.That(clearPresenter.ResultSummary.Outcome, Is.EqualTo(StageRouteOutcome.Clear));
            StageRunResultSummary fullRouteSummary = clearPresenter.ResultSummary;
            Assert.That(fullRouteSummary.OutcomeFact.OutcomeDisposition, Is.EqualTo(StageOutcomeDisposition.Clear));
            Assert.That(fullRouteSummary.OutcomeFact.ClearReason, Is.EqualTo(StageClearReason.BossTerminal));
            Assert.That(fullRouteSummary.OutcomeFact.TotalActiveElapsedMilliseconds, Is.GreaterThan(0));
            Assert.That(fullRouteSummary.OutcomeFact.CombatActiveElapsedMilliseconds, Is.GreaterThan(0));
            Assert.That(
                fullRouteSummary.OutcomeFact.TotalActiveElapsedMilliseconds,
                Is.GreaterThan(fullRouteSummary.OutcomeFact.CombatActiveElapsedMilliseconds));
            Assert.That(fullRouteSummary.SegmentResultCount, Is.EqualTo(2));
            Assert.That(fullRouteSummary.GetSegmentResult(0).Completed, Is.True);
            Assert.That(fullRouteSummary.GetSegmentResult(0).ActiveElapsedMilliseconds, Is.GreaterThan(0));
            Assert.That(fullRouteSummary.GetSegmentResult(1).Completed, Is.True);
            Assert.That(
                fullRouteSummary.TutorialRouteSummaryFact.RouteState,
                Is.EqualTo(StageTutorialRouteState.Completed));
            Assert.That(
                fullRouteSummary.TutorialRouteSummaryFact.ObservationElapsedMilliseconds,
                Is.GreaterThan(0));
            Assert.That(fullRouteSummary.TutorialRouteSummaryFact.CoverageCount, Is.EqualTo(7));
            Assert.That(
                fullRouteSummary.TutorialRouteSummaryFact.GetCoverage(0).LessonId,
                Is.EqualTo("soldier_challenge"));
            Assert.That(
                fullRouteSummary.TutorialRouteSummaryFact.GetCoverage(6).LessonId,
                Is.EqualTo("clear_targets"));
            Assert.That(
                fullRouteSummary.TryGetSemanticProof(
                    StageRunFactVocabulary.SurvivalNoPlayerDownProofId,
                    out StageRunSemanticProofFact fullRouteSurvivalProof),
                Is.True);
            Assert.That(fullRouteSurvivalProof.Qualified, Is.True);
            Assert.That(clearPresenter.PrimaryActionId, Is.EqualTo("olympus-invasion.replay"));
            Assert.That(retryButton, Is.Not.Null);
            Assert.That(retryButton.IsInteractable(), Is.True);
            retryButton.onClick.Invoke();
            yield return WaitForActiveScenePath(ScenePath, 8f);

            OlympusCorridorCombatFlowController freshFlow =
                RequireComponent<OlympusCorridorCombatFlowController>(
                    FlowRootName,
                    "fresh replay flow controller");
            GameObject freshPlayer = RequireSceneObject(PlayerRootName);
            Assert.That(freshFlow.StageCleared, Is.False);
            Assert.That(freshFlow.StageClearOverlayShown, Is.False);
            Assert.That(freshPlayer.activeInHierarchy, Is.False);
            Assert.That(SceneManager.GetSceneByName("UI_StageClear").isLoaded, Is.False);
            Assert.That(Time.timeScale, Is.EqualTo(1f).Within(0.0001f));
            report.AppendLine("- Replay loaded a fresh Corridor intro run.");
            report.AppendLine();
            report.AppendLine("RESULT: PASS");

            Directory.CreateDirectory(Path.GetDirectoryName(FullRouteReportPath));
            File.WriteAllText(FullRouteReportPath, report.ToString());
        }

        [UnityTest]
        [Timeout(90000)]
        public IEnumerator TutorialFireAimFovUsesVisibleCombatCamera()
        {
            var report = new StringBuilder();
            OlympusCorridorCombatFlowController flowController =
                RequireComponent<OlympusCorridorCombatFlowController>(
                    FlowRootName,
                    "Olympus corridor combat flow controller");
            PlayerMovementController player = RequireComponent<PlayerMovementController>(
                PlayerRootName,
                "player movement controller");
            PlayerActionController actionController = RequireComponent<PlayerActionController>(
                PlayerRootName,
                "player action controller");
            PlayerCombatModeController combatModeController = RequireComponent<PlayerCombatModeController>(
                PlayerRootName,
                "player combat mode controller");
            PlayerRangedBasicAttackAction rangedBasicAttackAction =
                RequireComponent<PlayerRangedBasicAttackAction>(
                    PlayerRootName,
                    "player ranged basic action");
            PlayerRangedAimController rangedAimController =
                RequireComponent<PlayerRangedAimController>(
                    PlayerRootName,
                    "player ranged aim controller");
            ActionCameraController cameraController =
                RequireComponent<ActionCameraController>(
                    "OlympusCorridor_Combat_MainCamera",
                    "combat action camera controller");
            Camera combatCamera = RequireComponent<Camera>(
                "OlympusCorridor_Combat_MainCamera",
                "combat camera");

            bool combatCameraWasMainAfterHandoff = false;
            float preFireFieldOfView = 0f;
            float minFireFieldOfView = 0f;
            Vector3 preFireCameraPosition = Vector3.zero;
            float fireAimCameraShift = 0f;
            report.AppendLine("# Olympus Corridor Fire Aim FOV Diagnostic");
            report.AppendLine();
            report.AppendLine("- This diagnostic bypasses minimum step-duration assertions so it can reach the Fire cue.");
            report.AppendLine("- It records enabled cameras, `Camera.main`, combat camera FOV, and aim weights during the Fire cue.");

            try
            {
                flowController.SkipIntroCutscene();
                yield return null;
                yield return null;
                AppendCameraSnapshot(report, "After skip handoff", cameraController, combatCamera, rangedAimController);
                combatCameraWasMainAfterHandoff = Camera.main == combatCamera;

                OlympusCorridorTutorialDirector tutorialDirector =
                    RequireComponent<OlympusCorridorTutorialDirector>(
                        FlowRootName,
                        "Olympus corridor tutorial director");

                yield return WaitForStep(tutorialDirector, "Melee", 5f, report);
                yield return DriveUntilStep(
                    tutorialDirector,
                    "Move",
                    8f,
                    "melee hit diagnostic",
                    report,
                    actionController.QueueBasicAttack);
                yield return MoveUntilStep(
                    tutorialDirector,
                    player,
                    flowController,
                    combatCamera,
                    "SwapToRanged",
                    12f,
                    report);

                preFireFieldOfView = combatCamera.fieldOfView;
                preFireCameraPosition = combatCamera.transform.position;
                AppendCameraSnapshot(report, "Before ranged swap", cameraController, combatCamera, rangedAimController);
                yield return DriveUntilStep(
                    tutorialDirector,
                    "Fire",
                    5f,
                    "swap input diagnostic",
                    report,
                    combatModeController.QueueCombatModeSwap);

                AssertPrivateBoolFalse(
                    rangedBasicAttackAction,
                    "externalAimPreviewHeld",
                    "Fire diagnostic must not rely on the external aim preview shortcut.");
                Assert.IsFalse(rangedAimController.IsAiming, "Fire cue should not auto-aim before the fire button is held.");
                yield return WaitForPhase(tutorialDirector, "AwaitingAction", 2f, report);
                rangedBasicAttackAction.SetFireHeld(true);
                yield return null;
                AppendCameraSnapshot(report, "Held fire aim started", cameraController, combatCamera, rangedAimController);
                report.AppendLine();
                report.AppendLine("## Held Fire Aim FOV Trace");
                report.AppendLine("| Frame | Time | Combat FOV | Main Camera | Base FOV | Aim Target | Aim Weight | Is Aiming |");
                report.AppendLine("|---:|---:|---:|---|---:|---:|---:|---|");
                minFireFieldOfView = combatCamera.fieldOfView;
                float fireAimSettleDeadline = Time.realtimeSinceStartup + 2f;
                int fireAimFrame = 0;
                do
                {
                    yield return null;
                    minFireFieldOfView = Mathf.Min(minFireFieldOfView, combatCamera.fieldOfView);
                    fireAimFrame++;
                    AppendCameraFovSample(report, fireAimFrame, cameraController, combatCamera, rangedAimController);
                }
                while ((ReadPrivateField<float>(cameraController, "aimWeight") <= 0.75f
                        || minFireFieldOfView >= preFireFieldOfView - 3f
                        || Vector3.Distance(preFireCameraPosition, combatCamera.transform.position) <= 0.35f)
                    && Time.realtimeSinceStartup < fireAimSettleDeadline);

                AppendCameraSnapshot(report, "Fire aim settled", cameraController, combatCamera, rangedAimController);
                AppendAndAssertAimCameraLaneComposition(report, "Fire aim settled", combatCamera, player);
                fireAimCameraShift = Vector3.Distance(preFireCameraPosition, combatCamera.transform.position);
                report.AppendLine($"- Fire aim camera shift: `{fireAimCameraShift:0.00}m`.");
            }
            finally
            {
                Directory.CreateDirectory(Path.GetDirectoryName(TutorialAimFovReportPath));
                File.WriteAllText(TutorialAimFovReportPath, report.ToString());
            }

            Assert.IsTrue(combatCameraWasMainAfterHandoff, "Combat camera should become Camera.main after intro handoff.");
            Assert.AreSame(combatCamera, Camera.main, "Combat camera should remain the active MainCamera during the Fire cue.");
            Assert.IsTrue(rangedAimController.IsAiming, "Fire cue should put the player ranged aim controller into aim mode.");
            Assert.That(
                ReadPrivateField<float>(cameraController, "aimWeight"),
                Is.GreaterThan(0.75f),
                "Fire cue should drive the camera aim weight high enough to be visible.");
            Assert.Less(
                minFireFieldOfView,
                preFireFieldOfView - 3f,
                "Fire cue should tighten FOV by a visible amount, not only by a tiny numeric delta.");
            Assert.Greater(
                fireAimCameraShift,
                0.35f,
                "Fire cue should move the visible combat camera into the authored aim rig, not only change FOV.");
        }

        private static T RequireComponent<T>(string objectName, string label)
            where T : Component
        {
            GameObject gameObject = RequireSceneObject(objectName);
            T component = gameObject.GetComponent<T>();
            Assert.IsNotNull(component, $"Missing {label} on {objectName}.");
            return component;
        }

        private static Behaviour RequireBehaviourByTypeName(string objectName, string fullTypeName)
        {
            GameObject gameObject = RequireSceneObject(objectName);
            Behaviour[] behaviours = gameObject.GetComponents<Behaviour>();
            for (int i = 0; i < behaviours.Length; i++)
            {
                Behaviour behaviour = behaviours[i];
                if (behaviour != null && behaviour.GetType().FullName == fullTypeName)
                {
                    return behaviour;
                }
            }

            Assert.Fail($"Missing behaviour {fullTypeName} on {objectName}.");
            return null;
        }

        private static GameObject RequireSceneObject(string objectName)
        {
            GameObject gameObject = FindSceneObjectIncludingInactive(objectName);
            Assert.IsNotNull(gameObject, $"Missing scene object: {objectName}");
            return gameObject;
        }

        private static GameObject FindSceneObjectIncludingInactive(string objectName)
        {
            GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();
            for (int i = 0; i < objects.Length; i++)
            {
                GameObject candidate = objects[i];
                if (candidate != null
                    && candidate.scene.IsValid()
                    && string.Equals(candidate.name, objectName, System.StringComparison.Ordinal))
                {
                    return candidate;
                }
            }

            return null;
        }

        private static IEnumerator QueueBasicAttackUntilStep(
            OlympusCorridorTutorialDirector tutorialDirector,
            PlayerActionController actionController,
            string expectedStep,
            string triggerLabel,
            float timeoutSeconds,
            StringBuilder report)
        {
            float startedAt = Time.realtimeSinceStartup;
            float gameplayStartedAt = Time.time;
            int frames = 0;
            while (tutorialDirector.CurrentStepId != expectedStep)
            {
                Assert.Less(
                    Time.realtimeSinceStartup - startedAt,
                    timeoutSeconds,
                    $"Timed out waiting for {expectedStep} from {triggerLabel}.");
                actionController.QueueBasicAttack();
                frames++;
                yield return null;
            }

            AppendStepTiming(report, expectedStep, triggerLabel, frames, gameplayStartedAt);
        }

        private sealed class MoveJoystickGesture
        {
            private readonly PointerEventData pointerData;
            private readonly Vector2 center;
            private readonly float dragDistance;
            private bool released;

            public MoveJoystickGesture(
                Behaviour joystick,
                PointerEventData newPointerData,
                Vector2 newCenter,
                float newDragDistance)
            {
                Joystick = joystick;
                pointerData = newPointerData;
                center = newCenter;
                dragDistance = newDragDistance;
            }

            public Behaviour Joystick { get; }

            public void Drag(Vector2 input)
            {
                Assert.IsFalse(released, "A released joystick pointer cannot be dragged again.");
                Assert.IsNotNull(pointerData.pointerDrag, "The move joystick lost its drag handler.");
                Vector2 clampedInput = Vector2.ClampMagnitude(input, 1f);
                pointerData.position = center + clampedInput * dragDistance;
                ExecuteEvents.Execute(pointerData.pointerDrag, pointerData, ExecuteEvents.dragHandler);
            }

            public void Release()
            {
                if (released)
                {
                    return;
                }

                released = true;
                if (pointerData.pointerPress != null)
                {
                    ExecuteEvents.Execute(
                        pointerData.pointerPress,
                        pointerData,
                        ExecuteEvents.pointerUpHandler);
                }
            }
        }

        private static MoveJoystickGesture BeginMoveJoystickGesture(
            PlayerMovementController player,
            int pointerId)
        {
            Behaviour joystick = RequireBehaviourByTypeName(
                "MoveJoystickRing",
                "DimensionBrawl.UI.CombatHudVirtualJoystick");
            Image joystickImage = joystick.GetComponent<Image>();
            Assert.IsNotNull(joystickImage, "The virtual joystick needs a Graphic for EventSystem raycasts.");
            Assert.IsTrue(
                joystickImage.raycastTarget,
                "The virtual joystick Graphic must accept raycasts or real touch input cannot reach its pointer handlers.");
            Assert.AreSame(
                player,
                ReadPrivateField<PlayerMovementController>(joystick, "movementController"),
                "The scene joystick must drive the active corridor player.");

            EventSystem eventSystem = EventSystem.current;
            Assert.IsNotNull(eventSystem, "The corridor scene needs an active EventSystem for the virtual joystick.");

            RectTransform joystickRect = joystick.GetComponent<RectTransform>();
            Canvas canvas = joystick.GetComponentInParent<Canvas>();
            Assert.IsNotNull(canvas, "The virtual joystick must be under a Canvas.");
            Camera eventCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            Vector2 center = RectTransformUtility.WorldToScreenPoint(
                eventCamera,
                joystickRect.TransformPoint(joystickRect.rect.center));

            var pointerData = new PointerEventData(eventSystem)
            {
                pointerId = pointerId,
                button = PointerEventData.InputButton.Left,
                position = center,
                pressPosition = center
            };
            var raycastResults = new List<RaycastResult>();
            eventSystem.RaycastAll(pointerData, raycastResults);

            GameObject raycastTarget = null;
            RaycastResult joystickRaycast = default;
            for (int i = 0; i < raycastResults.Count; i++)
            {
                RaycastResult candidate = raycastResults[i];
                if (ExecuteEvents.GetEventHandler<IPointerDownHandler>(candidate.gameObject) != joystick.gameObject)
                {
                    continue;
                }

                raycastTarget = candidate.gameObject;
                joystickRaycast = candidate;
                break;
            }

            Assert.IsNotNull(
                raycastTarget,
                "A real EventSystem raycast at the visible move stick must resolve to the virtual joystick.");

            pointerData.pointerCurrentRaycast = joystickRaycast;
            pointerData.pointerPressRaycast = joystickRaycast;
            pointerData.rawPointerPress = raycastTarget;
            pointerData.pointerPress = ExecuteEvents.ExecuteHierarchy(
                raycastTarget,
                pointerData,
                ExecuteEvents.pointerDownHandler);
            pointerData.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(raycastTarget);
            Assert.AreSame(
                joystick.gameObject,
                pointerData.pointerPress,
                "The EventSystem should select the virtual joystick as the pointer-down handler.");
            Assert.AreSame(
                joystick.gameObject,
                pointerData.pointerDrag,
                "The EventSystem should select the virtual joystick as the drag handler.");

            float dragDistance = Mathf.Max(48f, joystickRect.rect.width * 0.3f);
            return new MoveJoystickGesture(joystick, pointerData, center, dragDistance);
        }

        private static IEnumerator WalkDownAuthoredStairsThroughJoystick(
            OlympusCorridorCombatFlowController flowController,
            PlayerMovementController player,
            Camera combatCamera,
            Scene expectedScene,
            int expectedPlayerInstanceId,
            string expectedRunId,
            Vector3 positionBeforeTutorialCompletion,
            StringBuilder report,
            float timeoutSeconds)
        {
            Assert.IsNotNull(flowController);
            Assert.IsNotNull(player);
            Assert.IsNotNull(combatCamera);
            Assert.That(expectedScene.IsValid(), Is.True);
            Assert.That(expectedRunId, Is.Not.Empty, "The canonical run must exist before tutorial completion.");

            Collider[] stairBlockers = ReadPrivateField<Collider[]>(flowController, "stairBlockers");
            float readyStartedAt = Time.realtimeSinceStartup;
            while (!flowController.TutorialCompleted
                || player.IsCinematicMoveInputLocked
                || HasEnabledCollider(stairBlockers))
            {
                Assert.Less(
                    Time.realtimeSinceStartup - readyStartedAt,
                    3f,
                    "Tutorial completion did not release the player and stair blockers.");
                AssertCanonicalTraversalContinuity(
                    flowController,
                    player,
                    expectedScene,
                    expectedPlayerInstanceId,
                    expectedRunId);
                yield return null;
            }

            Assert.That(flowController.CorridorCombatStarted, Is.False);
            Assert.That(
                Vector3.Distance(player.transform.position, positionBeforeTutorialCompletion),
                Is.LessThan(1.25f),
                "Tutorial completion must not teleport the player to the stair trigger.");

            BoxCollider upperLandingSupport = RequireSceneObject(
                "OlympusCorridor_IntroUpperLandingTraversalSupport").GetComponent<BoxCollider>();
            BoxCollider stairTraversalSupport = RequireSceneObject(
                "OlympusCorridor_IntroStairTraversalSupport").GetComponent<BoxCollider>();
            Assert.IsNotNull(upperLandingSupport);
            Assert.IsNotNull(stairTraversalSupport);
            Assert.IsTrue(upperLandingSupport.enabled);
            Assert.IsTrue(stairTraversalSupport.enabled);
            Assert.IsFalse(upperLandingSupport.isTrigger);
            Assert.IsFalse(stairTraversalSupport.isTrigger);
            Assert.That(upperLandingSupport.gameObject.scene.handle, Is.EqualTo(expectedScene.handle));
            Assert.That(stairTraversalSupport.gameObject.scene.handle, Is.EqualTo(expectedScene.handle));

            GameObject triggerObject = RequireSceneObject("OlympusCorridor_StairToCorridorCombatTrigger");
            SphereCollider stairTrigger = triggerObject.GetComponent<SphereCollider>();
            Assert.IsNotNull(stairTrigger);
            Assert.IsTrue(stairTrigger.enabled);
            Assert.IsTrue(stairTrigger.isTrigger);
            Assert.That(stairTrigger.gameObject.scene.handle, Is.EqualTo(expectedScene.handle));
            Assert.AreSame(
                stairTrigger.transform,
                ReadPrivateField<Transform>(flowController, "stairTriggerCenter"));
            float triggerRadius = ReadPrivateField<float>(flowController, "stairTriggerRadius");
            Assert.That(triggerRadius, Is.EqualTo(stairTrigger.radius).Within(0.001f));

            Assert.IsTrue(ReadPrivateField<bool>(player, "cameraRelativeMovement"));
            Assert.AreSame(
                combatCamera,
                ReadPrivateField<Camera>(player, "referenceCamera"),
                "The stair joystick projection must use the player's authored movement camera.");

            CharacterController characterController = player.GetComponent<CharacterController>();
            Assert.IsNotNull(characterController);
            Assert.IsTrue(characterController.enabled);

            Vector3 startPosition = player.transform.position;
            Transform stairEntryAnchor = ReadPrivateField<Transform>(flowController, "stairEntryAnchor");
            Assert.IsNotNull(stairEntryAnchor);
            Vector3 triggerPosition = stairTrigger.transform.TransformPoint(stairTrigger.center);
            Transform lowerPlayerStart = RequireSceneObject("PlayerStartAnchor").transform;
            Assert.That(
                Vector3.Distance(triggerPosition, lowerPlayerStart.position),
                Is.LessThan(0.01f),
                "The Station-entry trigger must use the authored lower PlayerStartAnchor instead of a hard-coded midpoint.");

            Transform lowerCombatPlacement = RequireSceneObject("OlympusCorridor_CombatPocketPlacement").transform;
            Transform lowerCombatRuntimeRoot = RequireSceneObject("OlympusStation_LowerCombatRuntimeRoot").transform;
            Assert.That(
                Vector3.Distance(lowerCombatRuntimeRoot.position, lowerCombatPlacement.position),
                Is.LessThan(0.01f));
            Assert.That(
                Quaternion.Angle(lowerCombatRuntimeRoot.rotation, lowerCombatPlacement.rotation),
                Is.LessThan(0.1f));
            GameObject encounterObject = RequireSceneObject("CombatEncounter");
            GameObject bossObject = RequireSceneObject("BossBarrageLaneReview_BossProxy_NeedleLock");
            Assert.AreSame(lowerCombatRuntimeRoot, encounterObject.transform.parent);
            Assert.AreSame(lowerCombatRuntimeRoot, bossObject.transform.parent);
            Assert.That(
                Vector3.Distance(encounterObject.transform.position, lowerCombatPlacement.position),
                Is.LessThan(0.01f),
                "The Station encounter must be restored to the authored lower combat frame.");
            Assert.That(
                Vector3.Distance(
                    bossObject.transform.position,
                    lowerCombatPlacement.TransformPoint(new Vector3(0f, 1.6f, 18f))),
                Is.LessThan(0.02f),
                "The Station boss must use its original direct-root pose under the lower combat frame.");
            Vector3 canonicalRouteDirection = Vector3.ProjectOnPlane(
                triggerPosition - stairEntryAnchor.position,
                Vector3.up).normalized;
            Vector3 canonicalRouteRight = Vector3.Cross(Vector3.up, canonicalRouteDirection).normalized;
            float routeHalfWidth = Mathf.Min(
                upperLandingSupport.size.x,
                stairTraversalSupport.size.x) * 0.5f;
            float startLateralOffset = Mathf.Abs(Vector3.Dot(
                startPosition - stairEntryAnchor.position,
                canonicalRouteRight));
            Assert.That(
                startLateralOffset,
                Is.LessThanOrEqualTo(routeHalfWidth - characterController.radius + 0.25f),
                "The tutorial must finish inside the authored upper landing route width.");

            float directPlanarDistance = Vector3.ProjectOnPlane(triggerPosition - startPosition, Vector3.up).magnitude;
            float expectedVerticalDescent = startPosition.y - triggerPosition.y;
            Assert.That(directPlanarDistance, Is.GreaterThan(25f));
            Assert.That(expectedVerticalDescent, Is.GreaterThan(5f));

            float authoredMoveSpeed = ReadPrivateField<float>(player, "moveSpeed");
            Assert.That(authoredMoveSpeed, Is.GreaterThan(0f));
            float walkStartedAt = Time.realtimeSinceStartup;
            float planarTravelDistance = 0f;
            float furthestRouteProgress = Vector3.Dot(
                Vector3.ProjectOnPlane(startPosition - stairEntryAnchor.position, Vector3.up),
                canonicalRouteDirection);
            int movementFrames = 0;
            Vector3 previousPosition = startPosition;
            var gesture = BeginMoveJoystickGesture(player, 72);
            Assert.IsFalse(
                ReadPublicProperty<bool>(gesture.Joystick, "IsInputBlocked"),
                "The move joystick must be available after tutorial completion.");

            try
            {
                while (!flowController.CorridorCombatStarted)
                {
                    Assert.Less(
                        Time.realtimeSinceStartup - walkStartedAt,
                        timeoutSeconds,
                        "Timed out while physically walking down the authored stairs. "
                        + $"start={startPosition}, player={player.transform.position}, trigger={triggerPosition}, "
                        + $"distance={Vector3.Distance(player.transform.position, triggerPosition):0.00}, "
                        + $"travel={planarTravelDistance:0.00}, "
                        + $"upperLocal={upperLandingSupport.transform.InverseTransformPoint(player.transform.position)}, "
                        + $"stairLocal={stairTraversalSupport.transform.InverseTransformPoint(player.transform.position)}, "
                        + $"joystick={ReadPublicProperty<Vector2>(gesture.Joystick, "CurrentInput")}, "
                        + $"mobile={ReadPrivateField<Vector2>(player, "mobileMoveInput")}, "
                        + $"moveDirection={player.CurrentMoveDirection}, "
                        + $"planarVelocity={player.PlanarVelocity}, "
                        + $"cameraForward={Vector3.ProjectOnPlane(combatCamera.transform.forward, Vector3.up).normalized}, "
                        + $"targetAlignment={Vector3.Dot(player.CurrentMoveDirection, Vector3.ProjectOnPlane(triggerPosition - player.transform.position, Vector3.up).normalized):0.000}, "
                        + $"velocity={characterController.velocity}, "
                        + $"nearby={DescribeNearbyColliders(player.transform.position)}, "
                        + $"surfaces={DescribePathSurfaces(player.transform.position, triggerPosition)}.");
                    AssertCanonicalTraversalContinuity(
                        flowController,
                        player,
                        expectedScene,
                        expectedPlayerInstanceId,
                        expectedRunId);
                    Assert.IsTrue(characterController.enabled);

                    float distanceBeforeMove = Vector3.Distance(player.transform.position, triggerPosition);
                    if (distanceBeforeMove > triggerRadius)
                    {
                        Assert.IsFalse(
                            flowController.CorridorCombatStarted,
                            "Lower combat must remain closed before the player physically reaches the trigger.");
                        Vector2 joystickInput = ResolveCameraRelativeJoystickInput(
                            player.transform.position,
                            triggerPosition,
                            combatCamera);
                        gesture.Drag(joystickInput);
                        Assert.That(
                            ReadPublicProperty<Vector2>(gesture.Joystick, "CurrentInput").sqrMagnitude,
                            Is.GreaterThan(0.5f),
                            "The EventSystem drag must keep a real movement value on the virtual joystick.");
                    }
                    else
                    {
                        gesture.Drag(Vector2.zero);
                    }

                    yield return null;
                    movementFrames++;

                    Vector3 currentPosition = player.transform.position;
                    float planarStep = Vector3.ProjectOnPlane(
                        currentPosition - previousPosition,
                        Vector3.up).magnitude;
                    float maximumFrameStep = authoredMoveSpeed * Mathf.Max(Time.deltaTime, 0.001f) * 1.5f + 0.18f;
                    Assert.That(
                        planarStep,
                        Is.LessThanOrEqualTo(maximumFrameStep),
                        "Player displacement exceeded the authored walking budget; a transform snap likely occurred.");
                    planarTravelDistance += planarStep;
                    previousPosition = currentPosition;

                    float routeProgress = Vector3.Dot(
                        Vector3.ProjectOnPlane(currentPosition - stairEntryAnchor.position, Vector3.up),
                        canonicalRouteDirection);
                    float lateralOffset = Mathf.Abs(Vector3.Dot(
                        currentPosition - stairEntryAnchor.position,
                        canonicalRouteRight));
                    Assert.That(
                        lateralOffset,
                        Is.LessThanOrEqualTo(routeHalfWidth - characterController.radius + 0.25f),
                        "The joystick route left the authored stair width.");
                    Assert.That(
                        routeProgress,
                        Is.GreaterThanOrEqualTo(furthestRouteProgress - 0.6f),
                        "The stair traversal unexpectedly reversed instead of walking toward lower combat.");
                    furthestRouteProgress = Mathf.Max(furthestRouteProgress, routeProgress);
                    float supportDistance = Mathf.Min(
                        Vector3.Distance(upperLandingSupport.ClosestPoint(currentPosition), currentPosition),
                        Vector3.Distance(stairTraversalSupport.ClosestPoint(currentPosition), currentPosition));
                    bool hasGroundRay = TryFindAuthoredGroundSurface(
                        currentPosition,
                        expectedScene,
                        out RaycastHit groundHit);
                    Assert.IsTrue(
                        characterController.isGrounded || hasGroundRay,
                        $"No authored non-trigger ground remained beneath the walking player at {currentPosition}; "
                        + $"controllerGrounded={characterController.isGrounded}, "
                        + $"routeProgress={routeProgress:0.00}, lateral={lateralOffset:0.00}, "
                        + $"supportDistance={supportDistance:0.000}.");
                    if (hasGroundRay)
                    {
                        Assert.That(
                            currentPosition.y - groundHit.point.y,
                            Is.InRange(-0.05f, 1.1f),
                            $"The player lost contact with the authored stair/floor surface. "
                            + $"surface={groundHit.collider.name}, hit={groundHit.point}, player={currentPosition}.");
                    }

                    if (flowController.CorridorCombatStarted)
                    {
                        Assert.That(
                            Vector3.Distance(currentPosition, triggerPosition),
                            Is.LessThanOrEqualTo(triggerRadius + 0.05f),
                            "Lower combat opened before the player entered the three-dimensional trigger.");
                    }
                }
            }
            finally
            {
                gesture.Release();
            }

            yield return null;
            Vector3 finalPosition = player.transform.position;
            Assert.IsTrue(
                player.LaneConstraintEnabled,
                "The authored Station lane constraint must re-enable after the physical stair entry.");
            float walkElapsedSeconds = Time.realtimeSinceStartup - walkStartedAt;
            AssertCanonicalTraversalContinuity(
                flowController,
                player,
                expectedScene,
                expectedPlayerInstanceId,
                expectedRunId);
            Assert.That(flowController.CorridorCombatStarted, Is.True);
            Assert.That(movementFrames, Is.GreaterThanOrEqualTo(30));
            Assert.That(walkElapsedSeconds, Is.GreaterThanOrEqualTo(3f));
            Assert.That(planarTravelDistance, Is.GreaterThanOrEqualTo(directPlanarDistance * 0.82f));
            Assert.That(startPosition.y - finalPosition.y, Is.GreaterThanOrEqualTo(5f));
            Assert.That(
                Vector3.Distance(finalPosition, triggerPosition),
                Is.LessThanOrEqualTo(triggerRadius + 0.05f));
            Assert.IsFalse(ReadPublicProperty<bool>(gesture.Joystick, "IsPointerHeld"));
            Assert.That(
                ReadPublicProperty<Vector2>(gesture.Joystick, "CurrentInput").sqrMagnitude,
                Is.EqualTo(0f).Within(0.0001f));
            Assert.That(ReadPrivateField<Vector2>(player, "mobileMoveInput").sqrMagnitude, Is.EqualTo(0f).Within(0.0001f));
            Assert.IsFalse(player.HasMoveInput, "Pointer-up must leave no movement input behind.");

            report.AppendLine(
                $"- Stair traversal via EventSystem joystick: `{movementFrames}` frames, "
                + $"`{walkElapsedSeconds:0.000}s`, `{planarTravelDistance:0.00}m` planar travel, "
                + $"`{startPosition.y - finalPosition.y:0.00}m` descent.");
        }

        private static bool HasEnabledCollider(Collider[] colliders)
        {
            if (colliders == null)
            {
                return false;
            }

            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != null && colliders[i].enabled)
                {
                    return true;
                }
            }

            return false;
        }

        private static string DescribeNearbyColliders(Vector3 position)
        {
            Collider[] nearby = Physics.OverlapSphere(
                position,
                2f,
                Physics.AllLayers,
                QueryTriggerInteraction.Ignore);
            var description = new StringBuilder();
            for (int i = 0; i < nearby.Length; i++)
            {
                Collider collider = nearby[i];
                if (collider == null)
                {
                    continue;
                }

                if (description.Length > 0)
                {
                    description.Append('|');
                }

                description.Append(collider.name);
                description.Append(':');
                description.Append(collider.GetType().Name);
            }

            return description.ToString();
        }

        private static string DescribePathSurfaces(Vector3 position, Vector3 target)
        {
            Vector3 direction = Vector3.ProjectOnPlane(target - position, Vector3.up).normalized;
            var description = new StringBuilder();
            for (int sample = 0; sample < 5; sample++)
            {
                Vector3 samplePosition = position + direction * (sample * 0.75f);
                RaycastHit[] hits = Physics.RaycastAll(
                    samplePosition + Vector3.up * 4f,
                    Vector3.down,
                    8f,
                    Physics.AllLayers,
                    QueryTriggerInteraction.Ignore);
                System.Array.Sort(hits, (left, right) => left.distance.CompareTo(right.distance));
                if (description.Length > 0)
                {
                    description.Append('/');
                }

                description.Append(sample);
                description.Append('=');
                int appended = 0;
                for (int hitIndex = 0; hitIndex < hits.Length && appended < 3; hitIndex++)
                {
                    Collider collider = hits[hitIndex].collider;
                    if (collider == null || collider is CharacterController)
                    {
                        continue;
                    }

                    if (appended > 0)
                    {
                        description.Append('|');
                    }

                    description.Append(collider.name);
                    description.Append('@');
                    description.Append(hits[hitIndex].point.y.ToString("0.00"));
                    appended++;
                }
            }

            return description.ToString();
        }

        private static bool TryFindAuthoredGroundSurface(
            Vector3 playerPosition,
            Scene expectedScene,
            out RaycastHit groundHit)
        {
            RaycastHit[] hits = Physics.RaycastAll(
                playerPosition + Vector3.up * 1.5f,
                Vector3.down,
                3f,
                Physics.AllLayers,
                QueryTriggerInteraction.Ignore);
            System.Array.Sort(hits, (left, right) => left.distance.CompareTo(right.distance));
            for (int i = 0; i < hits.Length; i++)
            {
                Collider collider = hits[i].collider;
                if (collider == null
                    || collider is CharacterController
                    || collider.isTrigger
                    || collider.gameObject.scene.handle != expectedScene.handle
                    || hits[i].point.y > playerPosition.y + 0.05f)
                {
                    continue;
                }

                groundHit = hits[i];
                return true;
            }

            groundHit = default;
            return false;
        }

        private static void AssertCanonicalTraversalContinuity(
            OlympusCorridorCombatFlowController flowController,
            PlayerMovementController player,
            Scene expectedScene,
            int expectedPlayerInstanceId,
            string expectedRunId)
        {
            Assert.That(SceneManager.GetActiveScene().handle, Is.EqualTo(expectedScene.handle));
            Assert.That(player.gameObject.scene.handle, Is.EqualTo(expectedScene.handle));
            Assert.That(player.gameObject.GetInstanceID(), Is.EqualTo(expectedPlayerInstanceId));
            Assert.That(flowController.gameObject.scene.handle, Is.EqualTo(expectedScene.handle));
            Assert.That(flowController.CanonicalStageRunId, Is.EqualTo(expectedRunId));
            Assert.IsFalse(
                SceneManager.GetSceneByPath(StationScenePath).isLoaded,
                "The authored stair traversal must not load the duplicate Station scene.");
        }

        private static Vector2 ResolveCameraRelativeJoystickInput(
            Vector3 playerPosition,
            Vector3 targetPosition,
            Camera movementCamera)
        {
            Vector3 desiredDirection = Vector3.ProjectOnPlane(targetPosition - playerPosition, Vector3.up);
            Assert.That(desiredDirection.sqrMagnitude, Is.GreaterThan(0.0001f));
            desiredDirection.Normalize();

            Vector3 cameraForward = Vector3.ProjectOnPlane(movementCamera.transform.forward, Vector3.up);
            Vector3 cameraRight = Vector3.ProjectOnPlane(movementCamera.transform.right, Vector3.up);
            Assert.That(cameraForward.sqrMagnitude, Is.GreaterThan(0.0001f));
            Assert.That(cameraRight.sqrMagnitude, Is.GreaterThan(0.0001f));
            cameraForward.Normalize();
            cameraRight.Normalize();

            return Vector2.ClampMagnitude(
                new Vector2(
                    Vector3.Dot(desiredDirection, cameraRight),
                    Vector3.Dot(desiredDirection, cameraForward)),
                1f);
        }

        private static IEnumerator MoveUntilStep(
            OlympusCorridorTutorialDirector tutorialDirector,
            PlayerMovementController player,
            OlympusCorridorCombatFlowController flowController,
            Camera combatCamera,
            string expectedStep,
            float timeoutSeconds,
            StringBuilder report)
        {
            float startedAt = Time.realtimeSinceStartup;
            float gameplayStartedAt = Time.time;
            Vector3 startPosition = player.transform.position;
            int frames = 0;

            Behaviour joystick = RequireBehaviourByTypeName(
                "MoveJoystickRing",
                "DimensionBrawl.UI.CombatHudVirtualJoystick");
            Image joystickImage = joystick.GetComponent<Image>();
            Assert.IsNotNull(joystickImage, "The virtual joystick needs a Graphic for EventSystem raycasts.");
            Assert.IsTrue(
                joystickImage.raycastTarget,
                "The virtual joystick Graphic must accept raycasts or real touch input cannot reach its pointer handlers.");
            Assert.AreSame(
                player,
                ReadPrivateField<PlayerMovementController>(joystick, "movementController"),
                "The scene joystick must drive the active corridor player.");

            EventSystem eventSystem = EventSystem.current;
            Assert.IsNotNull(eventSystem, "The corridor scene needs an active EventSystem for the virtual joystick.");

            RectTransform joystickRect = joystick.GetComponent<RectTransform>();
            Canvas canvas = joystick.GetComponentInParent<Canvas>();
            Assert.IsNotNull(canvas, "The virtual joystick must be under a Canvas.");
            Camera eventCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            Vector2 center = RectTransformUtility.WorldToScreenPoint(
                eventCamera,
                joystickRect.TransformPoint(joystickRect.rect.center));

            var pointerData = new PointerEventData(eventSystem)
            {
                pointerId = 71,
                button = PointerEventData.InputButton.Left,
                position = center,
                pressPosition = center
            };
            var raycastResults = new List<RaycastResult>();
            eventSystem.RaycastAll(pointerData, raycastResults);

            GameObject raycastTarget = null;
            RaycastResult joystickRaycast = default;
            for (int i = 0; i < raycastResults.Count; i++)
            {
                RaycastResult candidate = raycastResults[i];
                if (ExecuteEvents.GetEventHandler<IPointerDownHandler>(candidate.gameObject) != joystick.gameObject)
                {
                    continue;
                }

                raycastTarget = candidate.gameObject;
                joystickRaycast = candidate;
                break;
            }

            Assert.IsNotNull(
                raycastTarget,
                "A real EventSystem raycast at the visible move stick must resolve to the virtual joystick.");

            pointerData.pointerCurrentRaycast = joystickRaycast;
            pointerData.pointerPressRaycast = joystickRaycast;
            pointerData.rawPointerPress = raycastTarget;
            pointerData.pointerPress = ExecuteEvents.ExecuteHierarchy(
                raycastTarget,
                pointerData,
                ExecuteEvents.pointerDownHandler);
            pointerData.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(raycastTarget);
            Assert.AreSame(
                joystick.gameObject,
                pointerData.pointerPress,
                "The EventSystem should select the virtual joystick as the pointer-down handler.");
            Assert.AreSame(
                joystick.gameObject,
                pointerData.pointerDrag,
                "The EventSystem should select the virtual joystick as the drag handler.");
            Assert.IsTrue(
                ReadPublicProperty<bool>(joystick, "IsInputBlocked"),
                "The Move cue should keep gameplay input locked before its action window.");
            Assert.IsFalse(
                ReadPublicProperty<bool>(joystick, "IsPointerHeld"),
                "A pointer pressed during the cue must not drive movement before the action window opens.");

            Transform stairTriggerCenter = ReadPrivateField<Transform>(flowController, "stairTriggerCenter");
            Assert.IsNotNull(stairTriggerCenter, "The Move lesson needs the authored stair destination.");
            Assert.IsNotNull(combatCamera, "The Move lesson needs the player's authored movement camera.");
            Assert.AreSame(
                combatCamera,
                ReadPrivateField<Camera>(player, "referenceCamera"),
                "The Move lesson and the stair walk must use the same authored movement camera.");

            float dragDistance = Mathf.Max(48f, joystickRect.rect.width * 0.3f);
            Vector2 routeForwardInput = ResolveCameraRelativeJoystickInput(
                player.transform.position,
                stairTriggerCenter.position,
                combatCamera);
            pointerData.position = center + routeForwardInput * dragDistance;
            ExecuteEvents.Execute(pointerData.pointerDrag, pointerData, ExecuteEvents.dragHandler);

            while (tutorialDirector.CurrentPhaseId != "AwaitingAction")
            {
                Assert.Less(
                    Time.realtimeSinceStartup - startedAt,
                    timeoutSeconds,
                    "Timed out waiting for the Move tutorial input window.");
                yield return null;
            }

            Assert.IsFalse(
                ReadPublicProperty<bool>(joystick, "IsInputBlocked"),
                "The joystick should unlock for the Move tutorial action window.");
            Assert.IsTrue(
                ReadPublicProperty<bool>(joystick, "IsPointerHeld"),
                "The joystick should adopt the same held pointer when the tutorial action window opens.");
            Assert.That(
                ReadPublicProperty<Vector2>(joystick, "CurrentInput").sqrMagnitude,
                Is.GreaterThan(0.01f),
                "A drag begun during the cue should become live without requiring a second touch.");
            Assert.That(
                ReadPrivateField<Vector2>(player, "mobileMoveInput").sqrMagnitude,
                Is.GreaterThan(0.01f),
                "The unlocked joystick must replay its held value after the player movement lock is released.");

            while (tutorialDirector.CurrentStepId != expectedStep
                && tutorialDirector.CurrentPhaseId == "AwaitingAction")
            {
                Assert.Less(
                    Time.realtimeSinceStartup - startedAt,
                    timeoutSeconds,
                    $"Timed out waiting for {expectedStep} from a real joystick drag.");
                Assert.IsFalse(
                    ReadPublicProperty<bool>(joystick, "IsInputBlocked"),
                    "The Move action window should not relock the joystick during an active drag.");
                Assert.That(
                    ReadPublicProperty<Vector2>(joystick, "CurrentInput").sqrMagnitude,
                    Is.GreaterThan(0.01f),
                    "The held joystick value should remain live without synthetic repeat drag events.");
                Assert.That(
                    ReadPrivateField<Vector2>(player, "mobileMoveInput").sqrMagnitude,
                    Is.GreaterThan(0.01f),
                    "The active player should retain virtual joystick input while the pointer remains held.");
                frames++;
                yield return null;
            }

            ExecuteEvents.Execute(pointerData.pointerPress, pointerData, ExecuteEvents.pointerUpHandler);
            Assert.IsFalse(
                ReadPublicProperty<bool>(joystick, "IsPointerHeld"),
                "The virtual joystick should release its active pointer.");
            Assert.That(
                ReadPublicProperty<Vector2>(joystick, "CurrentInput").sqrMagnitude,
                Is.EqualTo(0f).Within(0.0001f));

            while (tutorialDirector.CurrentStepId != expectedStep)
            {
                Assert.Less(
                    Time.realtimeSinceStartup - startedAt,
                    timeoutSeconds,
                    $"Timed out waiting for {expectedStep} after the Move action committed.");
                yield return null;
            }

            float movedDistance = Vector3.ProjectOnPlane(
                player.transform.position - startPosition,
                Vector3.up).magnitude;
            Assert.GreaterOrEqual(
                movedDistance,
                0.65f,
                "Move tutorial should require a visible amount of player displacement, not only a run-start event.");
            Vector3 routeDirection = Vector3.ProjectOnPlane(
                stairTriggerCenter.position - startPosition,
                Vector3.up).normalized;
            Vector3 routeDisplacement = Vector3.ProjectOnPlane(
                player.transform.position - startPosition,
                Vector3.up);
            Assert.That(
                Vector3.Dot(routeDisplacement, routeDirection),
                Is.GreaterThan(0.35f),
                "The Move lesson must advance toward the authored stair route, not merely move in any direction.");
            Assert.That(
                Vector3.ProjectOnPlane(
                    stairTriggerCenter.position - player.transform.position,
                    Vector3.up).magnitude,
                Is.LessThan(Vector3.ProjectOnPlane(
                    stairTriggerCenter.position - startPosition,
                    Vector3.up).magnitude - 0.4f),
                "The Move lesson must reduce planar distance to the stair destination.");
            report.AppendLine($"- Move displacement before completion: `{movedDistance:0.00}m`.");
            AppendStepTiming(report, expectedStep, "EventSystem joystick drag", frames, gameplayStartedAt);
        }

        private static IEnumerator QueueSwapUntilStep(
            OlympusCorridorTutorialDirector tutorialDirector,
            PlayerCombatModeController combatModeController,
            string expectedStep,
            float timeoutSeconds,
            StringBuilder report)
        {
            float startedAt = Time.realtimeSinceStartup;
            float gameplayStartedAt = Time.time;
            int frames = 0;
            while (tutorialDirector.CurrentStepId != expectedStep)
            {
                Assert.Less(
                    Time.realtimeSinceStartup - startedAt,
                    timeoutSeconds,
                    $"Timed out waiting for {expectedStep} from swap input.");
                combatModeController.QueueCombatModeSwap();
                frames++;
                yield return null;
            }

            AppendStepTiming(report, expectedStep, "swap input", frames, gameplayStartedAt);
        }

        private static IEnumerator QueueFireUntilStep(
            OlympusCorridorTutorialDirector tutorialDirector,
            PlayerRangedBasicAttackAction rangedBasicAttackAction,
            string expectedStep,
            float timeoutSeconds,
            StringBuilder report)
        {
            float startedAt = Time.realtimeSinceStartup;
            float gameplayStartedAt = Time.time;
            int frames = 0;
            while (tutorialDirector.CurrentStepId != expectedStep)
            {
                Assert.Less(
                    Time.realtimeSinceStartup - startedAt,
                    timeoutSeconds,
                    $"Timed out waiting for {expectedStep} from fire input.");
                rangedBasicAttackAction.QueueFire();
                frames++;
                yield return null;
            }

            AppendStepTiming(report, expectedStep, "ranged fire hit", frames, gameplayStartedAt);
        }

        private static IEnumerator QueueDodgeUntilStep(
            OlympusCorridorTutorialDirector tutorialDirector,
            PlayerActionController actionController,
            PlayerMovementController player,
            OlympusCorridorCombatFlowController flowController,
            Camera combatCamera,
            string expectedStep,
            float timeoutSeconds,
            StringBuilder report)
        {
            float startedAt = Time.realtimeSinceStartup;
            float gameplayStartedAt = Time.time;
            int frames = 0;
            Transform stairTriggerCenter = ReadPrivateField<Transform>(flowController, "stairTriggerCenter");
            Assert.IsNotNull(stairTriggerCenter, "The Dodge lesson needs the authored stair destination.");
            var gesture = BeginMoveJoystickGesture(player, 73);
            try
            {
                while (tutorialDirector.CurrentStepId != expectedStep)
                {
                    Assert.Less(
                        Time.realtimeSinceStartup - startedAt,
                        timeoutSeconds,
                        $"Timed out waiting for {expectedStep} from directional dodge input.");
                    Vector2 routeForwardInput = ResolveCameraRelativeJoystickInput(
                        player.transform.position,
                        stairTriggerCenter.position,
                        combatCamera);
                    gesture.Drag(routeForwardInput);
                    if (!ReadPublicProperty<bool>(gesture.Joystick, "IsInputBlocked")
                        && ReadPublicProperty<Vector2>(gesture.Joystick, "CurrentInput").sqrMagnitude > 0.5f)
                    {
                        actionController.QueueDodge();
                    }

                    frames++;
                    yield return null;
                }
            }
            finally
            {
                gesture.Release();
            }

            Assert.That(
                ReadPublicProperty<Vector2>(gesture.Joystick, "CurrentInput").sqrMagnitude,
                Is.EqualTo(0f).Within(0.0001f));
            AppendStepTiming(report, expectedStep, "directional dodge input", frames, gameplayStartedAt);
        }

        private static IEnumerator DriveUntilStep(
            OlympusCorridorTutorialDirector tutorialDirector,
            string expectedStep,
            float timeoutSeconds,
            string trigger,
            StringBuilder report,
            System.Action driveInput,
            System.Action cleanupInput = null)
        {
            float startedAt = Time.realtimeSinceStartup;
            int frames = 0;
            while (tutorialDirector.CurrentStepId != expectedStep)
            {
                Assert.Less(
                    Time.realtimeSinceStartup - startedAt,
                    timeoutSeconds,
                    $"Timed out waiting for {expectedStep} from {trigger}.");
                driveInput();
                frames++;
                yield return null;
            }

            cleanupInput?.Invoke();
            float elapsedSeconds = Time.realtimeSinceStartup - startedAt;
            report.AppendLine(
                $"- `{expectedStep}` via {trigger}: `{frames}` frames, `{elapsedSeconds:0.000}s`.");
        }

        private static IEnumerator WaitForStep(
            OlympusCorridorTutorialDirector tutorialDirector,
            string expectedStep,
            float timeoutSeconds,
            StringBuilder report)
        {
            float startedAt = Time.realtimeSinceStartup;
            while (tutorialDirector.CurrentStepId != expectedStep)
            {
                Assert.Less(
                    Time.realtimeSinceStartup - startedAt,
                    timeoutSeconds,
                    $"Timed out waiting for {expectedStep}.");
                yield return null;
            }

            report.AppendLine($"- Reached `{expectedStep}` after `{Time.realtimeSinceStartup - startedAt:0.000}s`.");
        }

        private static IEnumerator WaitForPhase(
            OlympusCorridorTutorialDirector tutorialDirector,
            string expectedPhase,
            float timeoutSeconds,
            StringBuilder report)
        {
            float startedAt = Time.realtimeSinceStartup;
            while (tutorialDirector.CurrentPhaseId != expectedPhase)
            {
                Assert.Less(
                    Time.realtimeSinceStartup - startedAt,
                    timeoutSeconds,
                    $"Timed out waiting for tutorial phase {expectedPhase}.");
                yield return null;
            }

            report.AppendLine(
                $"- Reached phase `{expectedPhase}` after `{Time.realtimeSinceStartup - startedAt:0.000}s`.");
        }

        private static IEnumerator WaitForActiveScenePath(string expectedPath, float timeoutSeconds)
        {
            float startedAt = Time.realtimeSinceStartup;
            while (!string.Equals(
                SceneManager.GetActiveScene().path.Replace('\\', '/'),
                expectedPath,
                System.StringComparison.Ordinal))
            {
                Assert.Less(
                    Time.realtimeSinceStartup - startedAt,
                    timeoutSeconds,
                    $"Timed out waiting for active scene {expectedPath}.");
                yield return null;
            }

            yield return null;
            Assert.AreEqual(expectedPath, SceneManager.GetActiveScene().path.Replace('\\', '/'));
        }

        private static IEnumerator CompleteStationGuide(
            Behaviour stationGuide,
            StringBuilder report,
            float timeoutSeconds)
        {
            MethodInfo requestAdvance = stationGuide.GetType().GetMethod(
                "RequestAdvance",
                BindingFlags.Instance | BindingFlags.Public);
            Assert.That(requestAdvance, Is.Not.Null);

            float deadline = Time.realtimeSinceStartup + timeoutSeconds;
            bool observedGuide = false;
            bool requestedForCurrentPrompt = false;
            int advanceCount = 0;
            while (true)
            {
                bool guidePlaying = ReadPublicProperty<bool>(stationGuide, "IsGuidePlaying");
                bool awaitingAdvance = ReadPublicProperty<bool>(stationGuide, "IsAwaitingAdvance");
                observedGuide |= guidePlaying || awaitingAdvance;

                if (!awaitingAdvance)
                {
                    requestedForCurrentPrompt = false;
                }
                else if (!requestedForCurrentPrompt)
                {
                    requestAdvance.Invoke(stationGuide, null);
                    requestedForCurrentPrompt = true;
                    advanceCount++;
                }

                if (observedGuide
                    && !guidePlaying
                    && !awaitingAdvance
                    && Time.timeScale > 0.99f)
                {
                    break;
                }

                Assert.Less(
                    Time.realtimeSinceStartup,
                    deadline,
                    "Timed out completing the Station entry guide.");
                yield return null;
            }

            Assert.That(advanceCount, Is.EqualTo(2));
            report.AppendLine($"- Station guide prompts advanced: `{advanceCount}`.");
        }

        private static Behaviour RequireActiveSceneBehaviour(string fullTypeName)
        {
            System.Type type = System.Type.GetType(fullTypeName + ", Assembly-CSharp")
                ?? System.Type.GetType(fullTypeName + ", DimensionBrawl.Runtime");
            Assert.That(type, Is.Not.Null, $"Missing product type {fullTypeName}.");

            Behaviour found = null;
            UnityEngine.Object[] objects = Resources.FindObjectsOfTypeAll(type);
            Scene activeScene = SceneManager.GetActiveScene();
            for (int i = 0; i < objects.Length; i++)
            {
                if (objects[i] is not Behaviour candidate || candidate.gameObject.scene != activeScene)
                {
                    continue;
                }

                Assert.That(found, Is.Null, $"Active scene owns duplicate {type.Name} components.");
                found = candidate;
            }

            Assert.That(found, Is.Not.Null, $"Active scene is missing {type.Name}.");
            return found;
        }

        private static StageClearScreenPresenter FindSingleStageClearPresenter()
        {
            Scene clearScene = SceneManager.GetSceneByName("UI_StageClear");
            if (!clearScene.IsValid() || !clearScene.isLoaded)
            {
                return null;
            }

            StageClearScreenPresenter found = null;
            GameObject[] roots = clearScene.GetRootGameObjects();
            for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
            {
                StageClearScreenPresenter[] presenters =
                    roots[rootIndex].GetComponentsInChildren<StageClearScreenPresenter>(true);
                for (int presenterIndex = 0; presenterIndex < presenters.Length; presenterIndex++)
                {
                    Assert.That(found, Is.Null, "The product clear scene must own one presenter.");
                    found = presenters[presenterIndex];
                }
            }

            return found;
        }

        private static bool IsStageClearInteractive(StageClearScreenPresenter presenter)
        {
            if (presenter == null || !presenter.isActiveAndEnabled)
            {
                return false;
            }

            CanvasGroup canvasGroup = ReadPrivateField<CanvasGroup>(presenter, "canvasGroup");
            return canvasGroup != null && canvasGroup.interactable && canvasGroup.blocksRaycasts;
        }

        private static bool ApplyLethalDamage(CombatHealth health, DamageTeam sourceTeam)
        {
            if (health == null)
            {
                return false;
            }

            if (!health.IsTerminalMutationBound)
            {
                health.ResetHealthToFull();
            }

            return health.TryApplyDamage(new DamageInfo(
                null,
                sourceTeam,
                health.MaxHealth + 1f,
                health.transform.position,
                Vector3.forward,
                0f,
                DamageResponsePolicy.DamageOnly,
                CombatControlLockPolicy.None));
        }

        private static void AppendAndAssertAimCameraLaneComposition(
            StringBuilder report,
            string label,
            Camera combatCamera,
            PlayerMovementController player)
        {
            Assert.IsNotNull(combatCamera, $"{label}: missing combat camera.");
            Assert.IsNotNull(player, $"{label}: missing player movement controller.");

            Transform combatPackageRoot = RequireSceneObject(CombatPackageRootName).transform;
            Vector3 laneForward = Vector3.ProjectOnPlane(combatPackageRoot.forward, Vector3.up);
            Assert.Greater(
                laneForward.sqrMagnitude,
                0.0001f,
                $"{label}: combat package forward must define a planar lane axis.");
            laneForward.Normalize();

            Vector3 laneRight = Vector3.Cross(Vector3.up, laneForward).normalized;
            Vector3 toCamera = Vector3.ProjectOnPlane(
                combatCamera.transform.position - player.transform.position,
                Vector3.up);
            Vector3 cameraForward = Vector3.ProjectOnPlane(combatCamera.transform.forward, Vector3.up);
            Assert.Greater(
                cameraForward.sqrMagnitude,
                0.0001f,
                $"{label}: combat camera forward must define a planar view direction.");
            cameraForward.Normalize();

            float behindMeters = -Vector3.Dot(toCamera, laneForward);
            float lateralMeters = Vector3.Dot(toCamera, laneRight);
            float viewForwardDot = Vector3.Dot(cameraForward, laneForward);

            report.AppendLine();
            report.AppendLine($"## Aim Camera Lane Composition: {label}");
            report.AppendLine($"- Lane forward `{FormatVector3(laneForward)}`, lane right `{FormatVector3(laneRight)}`.");
            report.AppendLine(
                $"- Camera behind `{behindMeters:0.00}m`, lateral `{lateralMeters:0.00}m`, planar distance `{toCamera.magnitude:0.00}m`, view forward dot `{viewForwardDot:0.00}`.");

            Assert.Greater(
                behindMeters,
                0.75f,
                $"{label}: aim camera should remain behind the player in combat-lane space.");
            Assert.LessOrEqual(
                Mathf.Abs(lateralMeters),
                2.35f,
                $"{label}: aim camera should not swing into a side-wall composition.");
            Assert.Greater(
                viewForwardDot,
                0.55f,
                $"{label}: aim camera should keep looking down the combat lane.");
        }

        private static void AppendStepTiming(
            StringBuilder report,
            string step,
            string trigger,
            int frames,
            float gameplayStartedAt)
        {
            float elapsedSeconds = Time.time - gameplayStartedAt;
            Assert.GreaterOrEqual(
                elapsedSeconds,
                ExpectedMinimumTutorialStepSeconds,
                $"{step} advanced too quickly from {trigger}.");
            report.AppendLine(
                $"- `{step}` via {trigger}: `{frames}` frames, `{elapsedSeconds:0.000}s`.");
        }

        private static void AppendCameraSnapshot(
            StringBuilder report,
            string label,
            ActionCameraController cameraController,
            Camera combatCamera,
            PlayerRangedAimController rangedAimController)
        {
            Camera mainCamera = Camera.main;
            report.AppendLine();
            report.AppendLine($"## Camera Snapshot: {label}");
            report.AppendLine(
                $"- Combat camera `{combatCamera.name}` active `{combatCamera.gameObject.activeInHierarchy}`, enabled `{combatCamera.enabled}`, FOV `{combatCamera.fieldOfView:0.00}`, position `{FormatVector3(combatCamera.transform.position)}`.");
            report.AppendLine(
                $"- Camera.main `{FormatCamera(mainCamera)}`.");
            report.AppendLine(
                $"- Action camera base `{ReadPrivateField<float>(cameraController, "baseFieldOfView"):0.00}`, aim target `{ReadPrivateField<float>(cameraController, "aimTargetWeight"):0.00}`, aim weight `{ReadPrivateField<float>(cameraController, "aimWeight"):0.00}`.");
            report.AppendLine(
                $"- Ranged aim can `{rangedAimController.CanAim}`, active `{rangedAimController.IsAiming}`.");
            report.AppendLine("- Enabled runtime cameras:");

            Camera[] cameras = Object.FindObjectsByType<Camera>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int i = 0; i < cameras.Length; i++)
            {
                Camera camera = cameras[i];
                if (!camera.enabled || !camera.gameObject.activeInHierarchy)
                {
                    continue;
                }

                report.AppendLine(
                    $"  - `{camera.name}` tag `{camera.tag}`, FOV `{camera.fieldOfView:0.00}`, depth `{camera.depth:0.0}`.");
            }
        }

        private static void AppendCameraFovSample(
            StringBuilder report,
            int frame,
            ActionCameraController cameraController,
            Camera combatCamera,
            PlayerRangedAimController rangedAimController)
        {
            report.AppendLine(
                $"| {frame} | {Time.time:0.000} | {combatCamera.fieldOfView:0.00} | `{FormatCamera(Camera.main)}` | {ReadPrivateField<float>(cameraController, "baseFieldOfView"):0.00} | {ReadPrivateField<float>(cameraController, "aimTargetWeight"):0.00} | {ReadPrivateField<float>(cameraController, "aimWeight"):0.00} | `{rangedAimController.IsAiming}` |");
        }

        private static string FormatCamera(Camera camera)
        {
            if (camera == null)
            {
                return "<none>";
            }

            return $"{camera.name} / FOV {camera.fieldOfView:0.00}";
        }

        private static string FormatVector3(Vector3 value)
        {
            return $"{value.x:0.00}, {value.y:0.00}, {value.z:0.00}";
        }

        private static void AssertBossTelegraphsSuppressed(string context)
        {
            BossBarrageLaneTelegraphPresenter[] presenters =
                Object.FindObjectsByType<BossBarrageLaneTelegraphPresenter>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None);
            Assert.IsNotEmpty(presenters, $"{context}: expected at least one boss telegraph presenter in the scene.");

            for (int i = 0; i < presenters.Length; i++)
            {
                BossBarrageLaneTelegraphPresenter presenter = presenters[i];
                Assert.IsFalse(presenter.enabled, $"{context}: boss telegraph presenter should be disabled.");
                Assert.Zero(presenter.VisibleMarkerCount, $"{context}: disabled boss telegraph should not keep visible markers.");
            }
        }

        private static void AssertCombatHudInputMuted(CanvasGroup canvasGroup, string context)
        {
            Assert.IsNotNull(canvasGroup);
            Assert.That(canvasGroup.alpha, Is.EqualTo(0f).Within(0.001f), $"HUD should be hidden {context}.");
            Assert.IsFalse(canvasGroup.interactable, $"HUD should not accept input {context}.");
            Assert.IsFalse(canvasGroup.blocksRaycasts, $"HUD should not block raycasts {context}.");
        }

        private static void AssertInputActionBinding(
            InputActionReference actionReference,
            string expectedActionName,
            params string[] expectedPaths)
        {
            Assert.IsNotNull(actionReference, $"{expectedActionName} InputActionReference is not authored.");
            Assert.IsNotNull(actionReference.action, $"{expectedActionName} InputActionReference has no action.");
            Assert.That(actionReference.action.name, Is.EqualTo(expectedActionName));

            for (int pathIndex = 0; pathIndex < expectedPaths.Length; pathIndex++)
            {
                string expectedPath = expectedPaths[pathIndex];
                bool found = false;
                for (int bindingIndex = 0; bindingIndex < actionReference.action.bindings.Count; bindingIndex++)
                {
                    string effectivePath = actionReference.action.bindings[bindingIndex].effectivePath;
                    if (string.Equals(
                        effectivePath,
                        expectedPath,
                        System.StringComparison.OrdinalIgnoreCase))
                    {
                        found = true;
                        break;
                    }
                }

                Assert.IsTrue(
                    found,
                    $"{expectedActionName} is missing authored desktop binding {expectedPath}.");
            }
        }

        private static void AssertInputActionBindingAbsent(
            InputActionReference actionReference,
            string expectedActionName,
            params string[] forbiddenPaths)
        {
            Assert.IsNotNull(actionReference, $"{expectedActionName} InputActionReference is not authored.");
            Assert.IsNotNull(actionReference.action, $"{expectedActionName} InputActionReference has no action.");
            for (int pathIndex = 0; pathIndex < forbiddenPaths.Length; pathIndex++)
            {
                string forbiddenPath = forbiddenPaths[pathIndex];
                for (int bindingIndex = 0; bindingIndex < actionReference.action.bindings.Count; bindingIndex++)
                {
                    Assert.IsFalse(
                        string.Equals(
                            actionReference.action.bindings[bindingIndex].effectivePath,
                            forbiddenPath,
                            System.StringComparison.OrdinalIgnoreCase),
                        $"{expectedActionName} must not consume {forbiddenPath}; that route belongs to UI/mobile input ownership.");
                }
            }
        }

        private static void AssertPrivateBoolFalse(object target, string fieldName, string message)
        {
            Assert.IsFalse(ReadPrivateField<bool>(target, fieldName), message);
        }

        private static void AssertPrivateBoolTrue(object target, string fieldName, string message)
        {
            Assert.IsTrue(ReadPrivateField<bool>(target, fieldName), message);
        }

        private static void AssertPrivateVector2Zero(object target, string fieldName, string message)
        {
            Vector2 value = ReadPrivateField<Vector2>(target, fieldName);
            Assert.That(value.sqrMagnitude, Is.EqualTo(0f).Within(0.0001f), message);
        }

        private static T ReadPrivateField<T>(object target, string fieldName)
        {
            Assert.IsNotNull(target);
            FieldInfo fieldInfo = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(fieldInfo, $"Missing private field `{fieldName}` on {target.GetType().Name}.");
            return (T)fieldInfo.GetValue(target);
        }

        private static T ReadPublicProperty<T>(object target, string propertyName)
        {
            Assert.IsNotNull(target);
            PropertyInfo propertyInfo = target.GetType().GetProperty(
                propertyName,
                BindingFlags.Instance | BindingFlags.Public);
            Assert.IsNotNull(propertyInfo, $"Missing public property `{propertyName}` on {target.GetType().Name}.");
            return (T)propertyInfo.GetValue(target);
        }

    }
}
