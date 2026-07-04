using System.Collections;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.Player;
using DimensionBrawl.Presentation;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace DimensionBrawl.Tests
{
    public sealed class OlympusCorridorCombatFlowPlayModeTests
    {
        private const string ScenePath = "Assets/_Game/Scenes/OlympusCorridorInvasionStage.unity";
        private const string DirectorName = "IntroGatePodReview_TimelineDirector";
        private const string FlowRootName = "OlympusCorridor_CombatFlowRoot";
        private const string PlayerRootName = "Player_CombatGirl_ActionFoundation";
        private const string CombatPackageRootName = "OlympusCorridor_BossBarrageCombatPackage";
        private const string IntroSwordGateRootName = "OlympusCorridor_IntroSwordGate";
        private const string CombatHudRootName = "PF_UI_CombatHud";
        private const string TutorialTimingReportPath = "C:/tmp/DimensionBrawl-OlympusTutorialTimingReport.md";
        private const string TutorialAimFovReportPath = "C:/tmp/DimensionBrawl-OlympusAimFovReport.md";
        private const float ExpectedMinimumTutorialStepSeconds = 0.85f;

        [Test]
        public void TutorialDialogueAudioCueDefaultsCoverEveryGuideLine()
        {
            OlympusCorridorTutorialDirector.DialogueAudioCue[] cues =
                OlympusCorridorTutorialDirector.CreateDefaultDialogueAudioCueSlots();

            Assert.That(cues.Length, Is.EqualTo(12));
            Assert.That(cues[0].CueId, Is.EqualTo(OlympusCorridorTutorialDirector.DialogueAudioCueId.MeleeCue));
            Assert.That(cues[1].CueId, Is.EqualTo(OlympusCorridorTutorialDirector.DialogueAudioCueId.MoveCue));
            Assert.That(cues[2].CueId, Is.EqualTo(OlympusCorridorTutorialDirector.DialogueAudioCueId.SwapToRangedCue));
            Assert.That(cues[3].CueId, Is.EqualTo(OlympusCorridorTutorialDirector.DialogueAudioCueId.FireCue));
            Assert.That(cues[4].CueId, Is.EqualTo(OlympusCorridorTutorialDirector.DialogueAudioCueId.DodgeCue));
            Assert.That(cues[5].CueId, Is.EqualTo(OlympusCorridorTutorialDirector.DialogueAudioCueId.ClearTargetsCue));
            Assert.That(cues[6].CueId, Is.EqualTo(OlympusCorridorTutorialDirector.DialogueAudioCueId.MeleeConfirm));
            Assert.That(cues[7].CueId, Is.EqualTo(OlympusCorridorTutorialDirector.DialogueAudioCueId.MoveConfirm));
            Assert.That(cues[8].CueId, Is.EqualTo(OlympusCorridorTutorialDirector.DialogueAudioCueId.SwapToRangedConfirm));
            Assert.That(cues[9].CueId, Is.EqualTo(OlympusCorridorTutorialDirector.DialogueAudioCueId.FireConfirm));
            Assert.That(cues[10].CueId, Is.EqualTo(OlympusCorridorTutorialDirector.DialogueAudioCueId.DodgeConfirm));
            Assert.That(cues[11].CueId, Is.EqualTo(OlympusCorridorTutorialDirector.DialogueAudioCueId.ClearTargetsConfirm));
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
                    "남은 적을 처치하면 기초 전투 검증이 완료됩니다.",
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
            ExpectKnownMissingSupportDragonPrefabLogs();
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
            for (int i = 0; i < 60; i++)
            {
                yield return null;
                minFireFieldOfView = Mathf.Min(minFireFieldOfView, combatCamera.fieldOfView);
                AppendCameraFovSample(report, i + 1, cameraController, combatCamera, rangedAimController);
            }

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

            report.AppendLine();
            report.AppendLine("## Static Step Gates");
            report.AppendLine("- Cue phase: at least `0.85s` focus/read window with step input muted.");
            report.AppendLine("- AwaitingAction phase: only live observer events can commit completion.");
            report.AppendLine("- Committed phase: at least `1.15s` RECORDED confirmation before the next cue; Fire keeps held-fire aim alive during this beat.");
            report.AppendLine("- Move gate: `0.75m` confirmed position movement inside the tutorial area.");
            report.AppendLine("- Fire gate: `0.7s` real aim preview hold after Ready + fire event + player-side target damage/death.");
            report.AppendLine("- Clear gate: all tutorial targets defeated.");
            Directory.CreateDirectory(Path.GetDirectoryName(TutorialTimingReportPath));
            File.WriteAllText(TutorialTimingReportPath, report.ToString());
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
                yield return DriveUntilStep(
                    tutorialDirector,
                    "SwapToRanged",
                    8f,
                    "move input diagnostic",
                    report,
                    () => player.SetMoveInput(Vector2.up),
                    () => player.SetMoveInput(Vector2.zero));

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
                for (int i = 0; i < 60; i++)
                {
                    yield return null;
                    minFireFieldOfView = Mathf.Min(minFireFieldOfView, combatCamera.fieldOfView);
                    AppendCameraFovSample(report, i + 1, cameraController, combatCamera, rangedAimController);
                }

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

        private static IEnumerator MoveUntilStep(
            OlympusCorridorTutorialDirector tutorialDirector,
            PlayerMovementController player,
            string expectedStep,
            float timeoutSeconds,
            StringBuilder report)
        {
            float startedAt = Time.realtimeSinceStartup;
            float gameplayStartedAt = Time.time;
            Vector3 startPosition = player.transform.position;
            int frames = 0;
            while (tutorialDirector.CurrentStepId != expectedStep)
            {
                Assert.Less(
                    Time.realtimeSinceStartup - startedAt,
                    timeoutSeconds,
                    $"Timed out waiting for {expectedStep} from move input.");
                player.SetMoveInput(Vector2.up);
                frames++;
                yield return null;
            }

            player.SetMoveInput(Vector2.zero);
            float movedDistance = Vector3.ProjectOnPlane(
                player.transform.position - startPosition,
                Vector3.up).magnitude;
            Assert.GreaterOrEqual(
                movedDistance,
                0.65f,
                "Move tutorial should require a visible amount of player displacement, not only a run-start event.");
            report.AppendLine($"- Move displacement before completion: `{movedDistance:0.00}m`.");
            AppendStepTiming(report, expectedStep, "move input", frames, gameplayStartedAt);
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
                    $"Timed out waiting for {expectedStep} from dodge input.");
                actionController.QueueDodge();
                frames++;
                yield return null;
            }

            AppendStepTiming(report, expectedStep, "dodge input", frames, gameplayStartedAt);
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

        private static void ExpectKnownMissingSupportDragonPrefabLogs()
        {
            const string supportDragonGuid = "bffbfb5b2823ee54692bcc11c2a88512";
            const string humanoidBossGuid = "a000f0e5a2493904492a06a283982f07";
            bool missingSupportDragon = string.IsNullOrWhiteSpace(
                UnityEditor.AssetDatabase.GUIDToAssetPath(supportDragonGuid));
            bool missingHumanoidBoss = string.IsNullOrWhiteSpace(
                UnityEditor.AssetDatabase.GUIDToAssetPath(humanoidBossGuid));
            if (!missingSupportDragon && !missingHumanoidBoss)
            {
                return;
            }

            LogAssert.Expect(
                LogType.Error,
                new Regex("Problem detected while opening the Scene file: 'Assets/_Game/Scenes/OlympusCorridorInvasionStage.unity'"));
            if (missingHumanoidBoss)
            {
                LogAssert.Expect(
                    LogType.Error,
                    new Regex("Prefab instance problem\\. Missing Prefab Asset: 'BossBarrageLaneReview_HumanoidBossVisual_SciFiSoldier_01_Commando"));
            }

            if (missingSupportDragon)
            {
                LogAssert.Expect(
                    LogType.Error,
                    new Regex("Prefab instance problem\\. Missing Prefab Asset: 'BossBarrageLaneReview_CinematicSupportDragon_Volcano"));
            }
        }
    }
}
