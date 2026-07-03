using System.Collections;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.Player;
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
        private const string IntroSwordGateRootName = "OlympusCorridor_IntroSwordGate";
        private const string TutorialTimingReportPath = "C:/tmp/DimensionBrawl-OlympusTutorialTimingReport.md";
        private const float ExpectedMinimumTutorialStepSeconds = 0.85f;

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
        [Timeout(90000)]
        public IEnumerator TutorialRuntimeInputsAdvanceByExpectedTriggers()
        {
            var report = new StringBuilder();
            OlympusCorridorCombatFlowController flowController =
                RequireComponent<OlympusCorridorCombatFlowController>(
                    FlowRootName,
                    "Olympus corridor combat flow controller");
            OlympusCorridorTutorialDirector tutorialDirector =
                RequireComponent<OlympusCorridorTutorialDirector>(
                    FlowRootName,
                    "Olympus corridor tutorial director");
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

            yield return MoveUntilStep(
                tutorialDirector,
                player,
                "SwapToRanged",
                8f,
                report);
            Assert.AreEqual("SwapToRanged", tutorialDirector.CurrentStepId);
            Assert.That(tutorialDirector.LastCompletionRecord, Does.StartWith("Move:"));
            Assert.IsTrue(combatModeController.IsMeleeMode, "Swap step should start from sword mode.");

            yield return QueueSwapUntilStep(
                tutorialDirector,
                combatModeController,
                "Fire",
                5f,
                report);
            Assert.AreEqual("Fire", tutorialDirector.CurrentStepId);
            Assert.That(tutorialDirector.LastCompletionRecord, Does.StartWith("SwapToRanged:"));
            Assert.IsTrue(combatModeController.IsRangedMode, "Fire step should enter ranged mode only after the swap input.");

            yield return QueueFireUntilStep(
                tutorialDirector,
                rangedBasicAttackAction,
                "Dodge",
                8f,
                report);
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

            report.AppendLine();
            report.AppendLine("## Static Step Gates");
            report.AppendLine("- Cue phase: `0.45s` focus/read window with step input muted.");
            report.AppendLine("- AwaitingAction phase: only live observer events can commit completion.");
            report.AppendLine("- Committed phase: `0.55s` RECORDED confirmation before the next cue.");
            report.AppendLine("- Move gate: `1.25m` or movement started event.");
            report.AppendLine("- Fire gate: `0.7s` aim preview lead after Ready + fire event + player-side target damage/death.");
            report.AppendLine("- Clear gate: all tutorial targets defeated.");
            Directory.CreateDirectory(Path.GetDirectoryName(TutorialTimingReportPath));
            File.WriteAllText(TutorialTimingReportPath, report.ToString());
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

            AppendStepTiming(report, expectedStep, triggerLabel, frames, startedAt);
        }

        private static IEnumerator MoveUntilStep(
            OlympusCorridorTutorialDirector tutorialDirector,
            PlayerMovementController player,
            string expectedStep,
            float timeoutSeconds,
            StringBuilder report)
        {
            float startedAt = Time.realtimeSinceStartup;
            int frames = 0;
            while (tutorialDirector.CurrentStepId != expectedStep)
            {
                Assert.Less(
                    Time.realtimeSinceStartup - startedAt,
                    timeoutSeconds,
                    $"Timed out waiting for {expectedStep} from move input.");
                player.SetMoveInput(Vector2.right);
                frames++;
                yield return null;
            }

            player.SetMoveInput(Vector2.zero);
            AppendStepTiming(report, expectedStep, "move input", frames, startedAt);
        }

        private static IEnumerator QueueSwapUntilStep(
            OlympusCorridorTutorialDirector tutorialDirector,
            PlayerCombatModeController combatModeController,
            string expectedStep,
            float timeoutSeconds,
            StringBuilder report)
        {
            float startedAt = Time.realtimeSinceStartup;
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

            AppendStepTiming(report, expectedStep, "swap input", frames, startedAt);
        }

        private static IEnumerator QueueFireUntilStep(
            OlympusCorridorTutorialDirector tutorialDirector,
            PlayerRangedBasicAttackAction rangedBasicAttackAction,
            string expectedStep,
            float timeoutSeconds,
            StringBuilder report)
        {
            float startedAt = Time.realtimeSinceStartup;
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

            AppendStepTiming(report, expectedStep, "ranged fire hit", frames, startedAt);
        }

        private static IEnumerator QueueDodgeUntilStep(
            OlympusCorridorTutorialDirector tutorialDirector,
            PlayerActionController actionController,
            string expectedStep,
            float timeoutSeconds,
            StringBuilder report)
        {
            float startedAt = Time.realtimeSinceStartup;
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

            AppendStepTiming(report, expectedStep, "dodge input", frames, startedAt);
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

        private static void AppendStepTiming(
            StringBuilder report,
            string step,
            string trigger,
            int frames,
            float startedAt)
        {
            float elapsedSeconds = Time.realtimeSinceStartup - startedAt;
            Assert.GreaterOrEqual(
                elapsedSeconds,
                ExpectedMinimumTutorialStepSeconds,
                $"{step} advanced too quickly from {trigger}.");
            report.AppendLine(
                $"- `{step}` via {trigger}: `{frames}` frames, `{elapsedSeconds:0.000}s`.");
        }

        private static void ExpectKnownMissingSupportDragonPrefabLogs()
        {
            const string supportDragonGuid = "bffbfb5b2823ee54692bcc11c2a88512";
            if (!string.IsNullOrWhiteSpace(UnityEditor.AssetDatabase.GUIDToAssetPath(supportDragonGuid)))
            {
                return;
            }

            LogAssert.Expect(
                LogType.Error,
                new Regex("Problem detected while opening the Scene file: 'Assets/_Game/Scenes/OlympusCorridorInvasionStage.unity'"));
            LogAssert.Expect(
                LogType.Error,
                new Regex("Prefab instance problem\\. Missing Prefab Asset: 'BossBarrageLaneReview_CinematicSupportDragon_Volcano"));
        }
    }
}
