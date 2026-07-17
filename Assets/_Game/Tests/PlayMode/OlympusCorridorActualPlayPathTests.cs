using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.Presentation;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace DimensionBrawl.Tests
{
    public sealed class OlympusCorridorActualPlayPathTests
    {
        private const string ScenePath = "Assets/_Game/Scenes/OlympusCorridorInvasionStage.unity";
        private const string IntroProfilePath =
            "Assets/_Game/DesignData/Profiles/Cinematics/DB_Cinematic_IntroGatePodAwakening_OlympusBombingPrelude.asset";
        private const string IntroPlayablePath =
            "Assets/_Game/DesignData/Timelines/Cinematics/DB_Timeline_IntroGatePodAwakening_OlympusBombingPrelude.playable";
        private const string IntroProfileGuid = "2392b944287ab3b4f8c3cff3318a7168";
        private const string IntroPlayableGuid = "78db0cf6d732b004db26927620f65656";
        private const string ReportPath = "C:/tmp/DimensionBrawl-OlympusActualPlayPathReport.md";
        private const string DirectorName = "IntroGatePodReview_TimelineDirector";
        private const string FlowRootName = "OlympusCorridor_CombatFlowRoot";
        private const string PlayerRootName = "Player_CombatGirl_ActionFoundation";
        private const string IntroSwordGateRootName = "OlympusCorridor_IntroSwordGate";
        private const string CombatCameraName = "OlympusCorridor_Combat_MainCamera";
        private const double DefaultIntroHandoffSeconds = 36.5d;

        [UnitySetUp]
        public IEnumerator LoadOlympusCorridorScene()
        {
            Time.timeScale = 1f;
            EditorSceneManager.LoadSceneInPlayMode(ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator ResetTimeScale()
        {
            Time.timeScale = 1f;
            yield return null;
        }

        [UnityTest]
        public IEnumerator CanonicalIntroPresentationSpineMatchesNaturalRuntimeConsumer()
        {
            OlympusCorridorCombatFlowController flow =
                FindSceneComponent<OlympusCorridorCombatFlowController>(FlowRootName);
            PlayableDirector director = FindSceneComponent<PlayableDirector>(DirectorName);
            Assert.That(flow, Is.Not.Null);
            Assert.That(director, Is.Not.Null);

            PlayableStageDefinition route = ReadPrivateField<PlayableStageDefinition>(
                flow,
                "playableStageDefinition");
            Assert.That(route, Is.Not.Null);
            StageDefinitionProfile corridorDefinition = route.GetSceneSegment(0).StageDefinition;
            StageDefinitionSceneBinding sceneBinding = FindSceneBinding(corridorDefinition);
            Assert.That(sceneBinding, Is.Not.Null);
            Assert.That(
                route.CanonicalRouteDigest,
                Is.EqualTo("878dac821103cdca2d2ad29a3fab8bce27109e9a5c1d551b14eccb736fd252d0"));

            StagePresentationHandoffRef presentation = route.GetSceneSegment(0).EntryPresentation;
            Assert.That(presentation, Is.Not.Null);
            Assert.That(presentation.IsPresent, Is.True);
            Assert.That(sceneBinding.CutscenePortCount, Is.EqualTo(1));

            StageDefinitionProfile stageDefinition = sceneBinding.StageDefinition;
            Assert.That(stageDefinition, Is.Not.Null);
            Assert.That(sceneBinding.AnchorPointCount, Is.EqualTo(stageDefinition.AnchorCount));
            for (int i = 0; i < stageDefinition.AnchorCount; i++)
            {
                StageDefinitionProfile.AnchorRef expectedAnchor = stageDefinition.GetAnchor(i);
                Assert.That(
                    sceneBinding.TryGetAnchorPoint(expectedAnchor.AnchorId, out StageAnchorPoint sceneAnchor),
                    Is.True,
                    $"Definition anchor {expectedAnchor.AnchorId} is absent from the loaded Corridor scene.");
                Assert.That(sceneAnchor, Is.Not.Null);
                Assert.That(sceneAnchor.GroupId, Is.EqualTo(expectedAnchor.GroupId));
            }

            CinematicSequenceProfile profile =
                AssetDatabase.LoadAssetAtPath<CinematicSequenceProfile>(IntroProfilePath);
            PlayableAsset playable = AssetDatabase.LoadAssetAtPath<PlayableAsset>(IntroPlayablePath);
            Assert.That(profile, Is.Not.Null);
            Assert.That(playable, Is.Not.Null);
            Assert.That(AssetDatabase.AssetPathToGUID(IntroProfilePath), Is.EqualTo(IntroProfileGuid));
            Assert.That(AssetDatabase.AssetPathToGUID(IntroPlayablePath), Is.EqualTo(IntroPlayableGuid));
            Assert.That(presentation.CinematicProfile, Is.SameAs(profile));
            Assert.That(presentation.ExpectedPlayableAsset, Is.SameAs(playable));
            Assert.That(presentation.StageDefinition, Is.SameAs(sceneBinding.StageDefinition));

            Assert.That(
                sceneBinding.TryGetCutscenePort(presentation.HandoffId, out StageCutscenePort port),
                Is.True);
            Assert.That(port.PortId, Is.EqualTo(presentation.ExpectedPortId));
            Assert.That(port.PresentationProfile, Is.SameAs(profile));
            Assert.That(port.RuntimeDirector, Is.SameAs(director));
            Assert.That(director.playableAsset, Is.SameAs(playable));

            int outputCount = 0;
            foreach (PlayableBinding output in playable.outputs)
            {
                Assert.That(output.sourceObject, Is.Not.Null, $"{output.streamName} has no source object.");
                Assert.That(
                    AssetDatabase.GetAssetPath(output.sourceObject),
                    Is.EqualTo(IntroPlayablePath),
                    $"{output.streamName} does not belong to the canonical combined Timeline.");
                Assert.That(
                    director.GetGenericBinding(output.sourceObject),
                    Is.Not.Null,
                    $"{output.streamName} has no runtime scene binding.");
                outputCount++;
            }

            Assert.That(outputCount, Is.EqualTo(39));
            Assert.That(
                ReadPrivateField<PlayableDirector>(flow, "introDirector"),
                Is.SameAs(director));

            yield return null;
        }

        [UnityTest]
        [Timeout(90000)]
        public IEnumerator ActualPlayPathStartsDirectorAndReachesIntroHandoffWithoutManualControl()
        {
            StringBuilder report = new StringBuilder();
            report.AppendLine("# Olympus Corridor Actual Play Path");
            report.AppendLine();
            report.AppendLine("- Scene is loaded through `EditorSceneManager.LoadSceneInPlayMode`.");
            report.AppendLine("- The test does not call `PlayableDirector.Play`, `Evaluate`, `Stop`, or private handoff methods.");
            report.AppendLine("- PASS requires natural director time progression and natural handoff state changes.");
            report.AppendLine();

            PlayableDirector director = FindSceneComponent<PlayableDirector>(DirectorName);
            OlympusCorridorCombatFlowController flowController =
                FindSceneComponent<OlympusCorridorCombatFlowController>(FlowRootName);
            GameObject playerRoot = FindSceneObject(PlayerRootName);
            GameObject introSwordGateRoot = FindSceneObject(IntroSwordGateRootName);
            Camera combatCamera = FindSceneComponent<Camera>(CombatCameraName);

            bool missingRequiredObject = false;
            missingRequiredObject |= AppendMissing(report, director, DirectorName);
            missingRequiredObject |= AppendMissing(report, flowController, FlowRootName);
            missingRequiredObject |= AppendMissing(report, playerRoot, PlayerRootName);
            missingRequiredObject |= AppendMissing(report, introSwordGateRoot, IntroSwordGateRootName);
            missingRequiredObject |= AppendMissing(report, combatCamera, CombatCameraName);

            if (missingRequiredObject)
            {
                WriteReport(report, "FAIL");
                Assert.Fail($"Missing required object(s). See {ReportPath}");
            }

            double handoffSeconds = ReadPrivateDouble(
                flowController,
                "introHandoffSeconds",
                DefaultIntroHandoffSeconds);
            double duration = double.IsInfinity(director.duration) ? 0d : Math.Max(0d, director.duration);
            double initialTime = director.time;
            PlayState initialState = director.state;
            bool playerInitiallyHidden = !playerRoot.activeInHierarchy;
            bool gateInitiallyHidden = !introSwordGateRoot.activeInHierarchy;
            string initialActiveCameras = CaptureActiveCameraNames();

            report.AppendLine("## Initial State");
            report.AppendLine($"- director.playOnAwake: `{director.playOnAwake}`");
            report.AppendLine($"- director.state: `{initialState}`");
            report.AppendLine($"- director.time: `{initialTime:0.000}`");
            report.AppendLine($"- director.duration: `{duration:0.000}`");
            report.AppendLine($"- director.timeUpdateMode: `{director.timeUpdateMode}`");
            report.AppendLine($"- handoffSeconds: `{handoffSeconds:0.000}`");
            report.AppendLine($"- player.activeInHierarchy: `{playerRoot.activeInHierarchy}`");
            report.AppendLine($"- introSwordGate.activeInHierarchy: `{introSwordGateRoot.activeInHierarchy}`");
            report.AppendLine($"- combatCamera.activeInHierarchy: `{combatCamera.gameObject.activeInHierarchy}`");
            report.AppendLine($"- active cameras: `{initialActiveCameras}`");
            report.AppendLine();

            double maxStartupTime = initialTime;
            bool observedPlaying = initialState == PlayState.Playing;
            string startupActiveCameras = initialActiveCameras;
            float startupDeadline = Time.realtimeSinceStartup + 2f;
            while (Time.realtimeSinceStartup < startupDeadline)
            {
                maxStartupTime = Math.Max(maxStartupTime, director.time);
                observedPlaying |= director.state == PlayState.Playing;
                startupActiveCameras = CaptureActiveCameraNames();
                yield return null;
            }

            bool directorNaturallyAdvanced = maxStartupTime > initialTime + 0.03d;
            bool actualPlaybackStarted = observedPlaying || directorNaturallyAdvanced;

            report.AppendLine("## Startup Observation");
            report.AppendLine($"- observedPlayingWithin2s: `{observedPlaying}`");
            report.AppendLine($"- maxTimeWithin2s: `{maxStartupTime:0.000}`");
            report.AppendLine($"- directorNaturallyAdvanced: `{directorNaturallyAdvanced}`");
            report.AppendLine($"- active cameras after startup: `{startupActiveCameras}`");
            report.AppendLine();

            bool handoffTimeReached = false;
            bool playerActivated = playerRoot.activeInHierarchy;
            bool gateActivated = introSwordGateRoot.activeInHierarchy;
            bool combatCameraActivated = combatCamera.gameObject.activeInHierarchy;
            bool directorResetBeforeHandoff = false;
            double firstPlayerActiveDirectorTime = playerActivated ? director.time : -1d;
            double firstGateActiveDirectorTime = gateActivated ? director.time : -1d;
            double maxObservedDirectorTime = maxStartupTime;
            PlayState finalDirectorState = director.state;
            string handoffActiveCameras = startupActiveCameras;

            if (actualPlaybackStarted)
            {
                float maxRealtimeWaitSeconds = Mathf.Clamp((float)(handoffSeconds - director.time + 8d), 8f, 55f);
                float handoffDeadline = Time.realtimeSinceStartup + maxRealtimeWaitSeconds;
                while (Time.realtimeSinceStartup < handoffDeadline)
                {
                    maxObservedDirectorTime = Math.Max(maxObservedDirectorTime, director.time);
                    finalDirectorState = director.state;

                    if (director.time >= handoffSeconds - 0.05d)
                    {
                        handoffTimeReached = true;
                    }

                    if (!playerActivated && playerRoot.activeInHierarchy)
                    {
                        playerActivated = true;
                        firstPlayerActiveDirectorTime = director.time;
                    }

                    if (!gateActivated && introSwordGateRoot.activeInHierarchy)
                    {
                        gateActivated = true;
                        firstGateActiveDirectorTime = director.time;
                    }

                    combatCameraActivated |= combatCamera.gameObject.activeInHierarchy;
                    handoffActiveCameras = CaptureActiveCameraNames();
                    directorResetBeforeHandoff |=
                        maxObservedDirectorTime < handoffSeconds - 0.1d
                        && maxObservedDirectorTime > 0.2d
                        && director.state != PlayState.Playing
                        && director.time <= 0.001d;

                    if (handoffTimeReached && playerRoot.activeInHierarchy && introSwordGateRoot.activeInHierarchy)
                    {
                        yield return null;
                        yield return null;
                        break;
                    }

                    yield return null;
                }
            }

            report.AppendLine("## Handoff Observation");
            report.AppendLine($"- maxObservedDirectorTime: `{maxObservedDirectorTime:0.000}`");
            report.AppendLine($"- finalDirectorState: `{finalDirectorState}`");
            report.AppendLine($"- handoffTimeReached: `{handoffTimeReached}`");
            report.AppendLine($"- playerInitiallyHidden: `{playerInitiallyHidden}`");
            report.AppendLine($"- gateInitiallyHidden: `{gateInitiallyHidden}`");
            report.AppendLine($"- playerActivated: `{playerActivated}`");
            report.AppendLine($"- firstPlayerActiveDirectorTime: `{firstPlayerActiveDirectorTime:0.000}`");
            report.AppendLine($"- introSwordGateActivated: `{gateActivated}`");
            report.AppendLine($"- firstGateActiveDirectorTime: `{firstGateActiveDirectorTime:0.000}`");
            report.AppendLine($"- combatCameraActivated: `{combatCameraActivated}`");
            report.AppendLine($"- active cameras at handoff: `{handoffActiveCameras}`");
            report.AppendLine($"- directorResetBeforeHandoff: `{directorResetBeforeHandoff}`");
            report.AppendLine($"- final player.activeSelf: `{playerRoot.activeSelf}`");
            report.AppendLine($"- final player.activeInHierarchy: `{playerRoot.activeInHierarchy}`");
            report.AppendLine($"- final introSwordGate.activeInHierarchy: `{introSwordGateRoot.activeInHierarchy}`");
            report.AppendLine();

            bool finalPlayerActiveSelf = playerRoot.activeSelf;
            bool finalPlayerActiveInHierarchy = playerRoot.activeInHierarchy;
            bool finalIntroSwordGateActive = introSwordGateRoot.activeInHierarchy;

            StringBuilder issues = new StringBuilder();
            AppendIssueIf(issues, !playerInitiallyHidden, "Player was not hidden at actual scene start.");
            AppendIssueIf(issues, !gateInitiallyHidden, "Intro sword gate was already active at actual scene start.");
            AppendIssueIf(issues, string.IsNullOrWhiteSpace(initialActiveCameras), "No active camera was available at actual scene start.");
            AppendIssueIf(issues, actualPlaybackStarted && string.IsNullOrWhiteSpace(startupActiveCameras), "No active camera was available during startup playback.");
            AppendIssueIf(issues, !actualPlaybackStarted, "PlayableDirector did not start or advance through the actual Play path.");
            AppendIssueIf(issues, directorResetBeforeHandoff, "PlayableDirector reset/stopped before reaching the handoff window.");
            AppendIssueIf(issues, actualPlaybackStarted && !handoffTimeReached, "PlayableDirector did not reach the configured handoff time.");
            AppendIssueIf(issues, actualPlaybackStarted && !playerActivated, "Player never became active through the actual Play path.");
            AppendIssueIf(issues, actualPlaybackStarted && !gateActivated, "Intro sword gate never became active through the actual Play path.");
            AppendIssueIf(issues, actualPlaybackStarted && !combatCameraActivated, "Combat camera never became active through the actual Play path.");
            AppendIssueIf(issues, actualPlaybackStarted && string.IsNullOrWhiteSpace(handoffActiveCameras), "No active camera was available at the handoff sample.");
            AppendIssueIf(issues, actualPlaybackStarted && !finalPlayerActiveSelf, "Player ended inactive after the natural handoff sample.");
            AppendIssueIf(issues, actualPlaybackStarted && !finalPlayerActiveInHierarchy, "Player ended inactive in hierarchy after the natural handoff sample.");
            AppendIssueIf(issues, actualPlaybackStarted && !finalIntroSwordGateActive, "Intro sword gate ended inactive after the natural handoff sample.");

            bool passed = issues.Length == 0;
            report.AppendLine("## Result");
            report.AppendLine(passed ? "PASS" : "FAIL");
            if (!passed)
            {
                report.AppendLine();
                report.AppendLine("### Issues");
                report.Append(issues);
            }

            WriteReport(report, passed ? "PASS" : "FAIL");
            Assert.IsTrue(passed, $"Actual Play path verification failed. See {ReportPath}\n{issues}");
        }

        private static bool AppendMissing(StringBuilder report, UnityEngine.Object value, string objectName)
        {
            if (value != null)
            {
                return false;
            }

            report.AppendLine($"- Missing required scene object/component: `{objectName}`");
            return true;
        }

        private static void AppendIssueIf(StringBuilder issues, bool condition, string issue)
        {
            if (condition)
            {
                issues.Append("- ");
                issues.AppendLine(issue);
            }
        }

        private static double ReadPrivateDouble(object target, string fieldName, double fallback)
        {
            if (target == null)
            {
                return fallback;
            }

            FieldInfo field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null)
            {
                return fallback;
            }

            object value = field.GetValue(target);
            return value is double doubleValue ? doubleValue : fallback;
        }

        private static T ReadPrivateField<T>(object target, string fieldName)
            where T : class
        {
            FieldInfo field = target?.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            return field?.GetValue(target) as T;
        }

        private static StageDefinitionSceneBinding FindSceneBinding(
            StageDefinitionProfile expectedDefinition)
        {
            StageDefinitionSceneBinding result = null;
            StageDefinitionSceneBinding[] components =
                Resources.FindObjectsOfTypeAll<StageDefinitionSceneBinding>();
            for (int i = 0; i < components.Length; i++)
            {
                StageDefinitionSceneBinding candidate = components[i];
                if (candidate == null
                    || !candidate.gameObject.scene.IsValid()
                    || candidate.StageDefinition != expectedDefinition)
                {
                    continue;
                }

                Assert.That(
                    result,
                    Is.Null,
                    $"Expected one scene binding for {expectedDefinition?.StageId}.");
                result = candidate;
            }

            return result;
        }

        private static T FindSceneComponent<T>(string objectName)
            where T : Component
        {
            GameObject gameObject = FindSceneObject(objectName);
            return gameObject != null ? gameObject.GetComponent<T>() : null;
        }

        private static GameObject FindSceneObject(string objectName)
        {
            GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();
            for (int i = 0; i < objects.Length; i++)
            {
                GameObject candidate = objects[i];
                if (candidate != null
                    && candidate.scene.IsValid()
                    && string.Equals(candidate.name, objectName, StringComparison.Ordinal))
                {
                    return candidate;
                }
            }

            return null;
        }

        private static string CaptureActiveCameraNames()
        {
            Camera[] cameras = Camera.allCameras;
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < cameras.Length; i++)
            {
                Camera camera = cameras[i];
                if (camera == null || !camera.isActiveAndEnabled || !camera.gameObject.activeInHierarchy)
                {
                    continue;
                }

                if (builder.Length > 0)
                {
                    builder.Append(", ");
                }

                builder.Append(camera.name);
            }

            return builder.ToString();
        }

        private static void WriteReport(StringBuilder report, string result)
        {
            report.AppendLine();
            report.AppendLine($"`RESULT: {result}`");
            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath));
            File.WriteAllText(ReportPath, report.ToString());
        }
    }
}
