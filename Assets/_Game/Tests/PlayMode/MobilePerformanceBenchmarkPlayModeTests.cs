using System.Collections;
using DimensionBrawl.Debugging;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace DimensionBrawl.Tests
{
    public sealed class MobilePerformanceBenchmarkPlayModeTests
    {
        [Test]
        public void BenchmarkUsesOnlyCanonicalCombatScenesInReviewedOrder()
        {
            CollectionAssert.AreEqual(
                new[]
                {
                    "Assets/_Game/Scenes/OlympusStationCombatStage.unity",
                    "Assets/_Game/Scenes/OlympusCorridorInvasionStage.unity",
                    "Assets/_Game/Scenes/ActionFoundationBossBarrageLaneReview.unity",
                    "Assets/_Game/Scenes/ActionFoundationFrontlineMotivationReview.unity"
                },
                MobilePerformanceBenchmarkRunner.CanonicalScenePaths);
        }

        [Test]
        public void MetricSummaryCalculatesStableNearestRankPercentiles()
        {
            double[] samples = { 1d, 2d, 3d, 4d, 100d };
            MobilePerformanceMetricSummary summary = MobilePerformanceStatistics.Summarize(
                samples,
                samples.Length,
                "Frame",
                "ms",
                valid: true);

            Assert.That(summary.Valid, Is.True);
            Assert.That(summary.SampleCount, Is.EqualTo(5));
            Assert.That(summary.Average, Is.EqualTo(22d).Within(0.0001d));
            Assert.That(summary.P50, Is.EqualTo(3d));
            Assert.That(summary.P95, Is.EqualTo(100d));
            Assert.That(summary.P99, Is.EqualTo(100d));
            Assert.That(summary.Maximum, Is.EqualTo(100d));
        }

        [Test]
        public void MetricSummaryRejectsUnavailableOrEmptyRecorders()
        {
            MobilePerformanceMetricSummary invalid = MobilePerformanceStatistics.Summarize(
                new[] { 1d, 2d },
                2,
                "Render Thread",
                "ms",
                valid: false);
            MobilePerformanceMetricSummary empty = MobilePerformanceStatistics.Summarize(
                System.Array.Empty<double>(),
                0,
                "Frame",
                "ms",
                valid: true);

            Assert.That(invalid.Valid, Is.False);
            Assert.That(empty.Valid, Is.False);
            Assert.That(empty.SampleCount, Is.Zero);
        }

        [Test]
        public void FrameDurationConversionReportsFpsAndRejectsInvalidSamples()
        {
            Assert.That(
                MobilePerformanceStatistics.ToFramesPerSecond(1000d / 60d),
                Is.EqualTo(60d).Within(0.0001d));
            Assert.That(MobilePerformanceStatistics.ToFramesPerSecond(0d), Is.Zero);
            Assert.That(MobilePerformanceStatistics.ToFramesPerSecond(double.NaN), Is.Zero);
            Assert.That(MobilePerformanceStatistics.ToFramesPerSecond(double.PositiveInfinity), Is.Zero);
        }

        [Test]
        public void BenchmarkReportRoundTripsTextureStreamingAndDevicePressureMetrics()
        {
            MobilePerformanceSceneResult scene = new()
            {
                Label = "Olympus Station Combat",
                ScenePath = "Assets/_Game/Scenes/OlympusStationCombatStage.unity",
                SceneLoadSeconds = 2.75f,
                MaximumThermalStatus = 4,
                ThermalStatusSampleCount = 6,
                ThermalStatusSampleCounts = new[] { 1, 2, 1, 1, 1, 0, 0 },
                LowMemoryEventCount = 2,
                TextureStreamingActive = true,
                StreamingTextureCount = 321UL,
                NonStreamingTextureCount = 45UL,
                StreamingRendererCount = 678UL,
                ActiveRendererCount = 901,
                ShadowCasterCount = 23,
                ActiveLightCount = 17,
                ActiveColliderCount = 456,
                EnvironmentDetailCandidateRendererCount = 321,
                EnvironmentDetailCulledRendererCount = 123,
                EnvironmentDetailCandidateColliderCount = 210,
                EnvironmentDetailCulledColliderCount = 98,
                ActiveFrameLoopBehaviourCount = 24,
                ActiveUpdateBehaviourCount = 21,
                ActiveLateUpdateBehaviourCount = 4,
                ActiveFixedUpdateBehaviourCount = 1,
                ConsolidatedFootstepPresenterCount = 3,
                TextureCurrentMemoryMebibytes = CreateMetric("Texture Current Memory", "MiB", 128.5d),
                StreamingTexturePendingLoads = CreateMetric("Streaming Texture Pending Loads", "count", 3d),
                GlobalTextureMipmapLimit = CreateMetric("Global Texture Mipmap Limit", "level", 1d),
                StreamingMemoryBudgetMebibytes = CreateMetric("Streaming Memory Budget", "MiB", 256d),
                LodBias = CreateMetric("LOD Bias", "scale", 0.8d),
                ShadowDistance = CreateMetric("Shadow Distance", "m", 48d)
            };
            scene.FrameLoops.Add(new MobilePerformanceFrameLoopInventory
            {
                TypeName = "DimensionBrawl.Presentation.MovementFootstepAudioScheduler",
                UpdateInstances = 1
            });
            MobilePerformanceBenchmarkReport report = new()
            {
                GeneratedUtc = "2026-07-11T00:00:00.0000000Z",
                Completed = true
            };
            report.Scenes.Add(scene);

            string json = JsonUtility.ToJson(report);
            MobilePerformanceBenchmarkReport restored =
                JsonUtility.FromJson<MobilePerformanceBenchmarkReport>(json);

            Assert.That(restored.Completed, Is.True);
            Assert.That(restored.Scenes, Has.Count.EqualTo(1));
            MobilePerformanceSceneResult restoredScene = restored.Scenes[0];
            Assert.That(restoredScene.SceneLoadSeconds, Is.EqualTo(2.75f).Within(0.0001f));
            Assert.That(restoredScene.MaximumThermalStatus, Is.EqualTo(4));
            Assert.That(restoredScene.ThermalStatusSampleCount, Is.EqualTo(6));
            CollectionAssert.AreEqual(new[] { 1, 2, 1, 1, 1, 0, 0 }, restoredScene.ThermalStatusSampleCounts);
            Assert.That(restoredScene.LowMemoryEventCount, Is.EqualTo(2));
            Assert.That(restoredScene.TextureStreamingActive, Is.True);
            Assert.That(restoredScene.StreamingTextureCount, Is.EqualTo(321UL));
            Assert.That(restoredScene.NonStreamingTextureCount, Is.EqualTo(45UL));
            Assert.That(restoredScene.StreamingRendererCount, Is.EqualTo(678UL));
            Assert.That(restoredScene.ActiveRendererCount, Is.EqualTo(901));
            Assert.That(restoredScene.ShadowCasterCount, Is.EqualTo(23));
            Assert.That(restoredScene.ActiveLightCount, Is.EqualTo(17));
            Assert.That(restoredScene.ActiveColliderCount, Is.EqualTo(456));
            Assert.That(restoredScene.EnvironmentDetailCandidateRendererCount, Is.EqualTo(321));
            Assert.That(restoredScene.EnvironmentDetailCulledRendererCount, Is.EqualTo(123));
            Assert.That(restoredScene.EnvironmentDetailCandidateColliderCount, Is.EqualTo(210));
            Assert.That(restoredScene.EnvironmentDetailCulledColliderCount, Is.EqualTo(98));
            Assert.That(restoredScene.ActiveFrameLoopBehaviourCount, Is.EqualTo(24));
            Assert.That(restoredScene.ActiveUpdateBehaviourCount, Is.EqualTo(21));
            Assert.That(restoredScene.ActiveLateUpdateBehaviourCount, Is.EqualTo(4));
            Assert.That(restoredScene.ActiveFixedUpdateBehaviourCount, Is.EqualTo(1));
            Assert.That(restoredScene.ConsolidatedFootstepPresenterCount, Is.EqualTo(3));
            Assert.That(restoredScene.FrameLoops, Has.Count.EqualTo(1));
            Assert.That(
                restoredScene.FrameLoops[0].TypeName,
                Is.EqualTo("DimensionBrawl.Presentation.MovementFootstepAudioScheduler"));
            Assert.That(restoredScene.FrameLoops[0].UpdateInstances, Is.EqualTo(1));
            Assert.That(restoredScene.TextureCurrentMemoryMebibytes.Average, Is.EqualTo(128.5d));
            Assert.That(restoredScene.StreamingTexturePendingLoads.Maximum, Is.EqualTo(3d));
            Assert.That(restoredScene.GlobalTextureMipmapLimit.Average, Is.EqualTo(1d));
            Assert.That(restoredScene.StreamingMemoryBudgetMebibytes.Average, Is.EqualTo(256d));
            Assert.That(restoredScene.LodBias.Average, Is.EqualTo(0.8d).Within(0.0001d));
            Assert.That(restoredScene.ShadowDistance.Average, Is.EqualTo(48d));
        }

        [UnityTest]
        [Timeout(90000)]
        public IEnumerator CanonicalCombatScenesStayInsideReviewedRuntimeLoopBudgets()
        {
            string[] scenePaths =
            {
                "Assets/_Game/Scenes/OlympusStationCombatStage.unity",
                "Assets/_Game/Scenes/OlympusCorridorInvasionStage.unity",
                "Assets/_Game/Scenes/ActionFoundationBossBarrageLaneReview.unity",
                "Assets/_Game/Scenes/ActionFoundationFrontlineMotivationReview.unity"
            };
            int[] maximumFrameLoopCounts = { 22, 21, 20, 17 };
            int[] expectedFootstepPresenterCounts = { 2, 0, 3, 3 };

            for (int sceneIndex = 0; sceneIndex < scenePaths.Length; sceneIndex++)
            {
                EditorSceneManager.LoadSceneInPlayMode(
                    scenePaths[sceneIndex],
                    new LoadSceneParameters(LoadSceneMode.Single));
                yield return null;
                yield return null;

                var result = new MobilePerformanceSceneResult();
                MobilePerformanceBenchmarkRunner.CaptureRuntimeInventory(result);
                Debug.Log(
                    $"[MobilePerformance] RuntimeLoopBudget scene={scenePaths[sceneIndex]} "
                    + $"total={result.ActiveFrameLoopBehaviourCount} "
                    + $"update={result.ActiveUpdateBehaviourCount} "
                    + $"late={result.ActiveLateUpdateBehaviourCount} "
                    + $"fixed={result.ActiveFixedUpdateBehaviourCount} "
                    + $"footsteps={result.ConsolidatedFootstepPresenterCount} "
                    + $"activeColliders={result.ActiveColliderCount} "
                    + $"culledDetailColliders={result.EnvironmentDetailCulledColliderCount}");
                Debug.Log(
                    $"[MobilePerformance] RuntimeFrameLoopTypes scene={scenePaths[sceneIndex]} "
                    + FormatFrameLoops(result));

                Assert.That(
                    result.ActiveFrameLoopBehaviourCount,
                    Is.LessThanOrEqualTo(maximumFrameLoopCounts[sceneIndex]),
                    $"{scenePaths[sceneIndex]} exceeded its reviewed runtime callback budget. "
                    + FormatFrameLoops(result));
                Assert.That(
                    result.ConsolidatedFootstepPresenterCount,
                    Is.EqualTo(expectedFootstepPresenterCounts[sceneIndex]));
                Assert.That(result.ActiveFixedUpdateBehaviourCount, Is.Zero);

                if (sceneIndex == 1)
                {
                    DimensionBrawl.LevelDesign.OlympusCorridorCombatFlowController flowController =
                        Object.FindFirstObjectByType<DimensionBrawl.LevelDesign.OlympusCorridorCombatFlowController>();
                    Assert.That(flowController, Is.Not.Null);
                    flowController.SkipIntroCutscene();
                    yield return null;
                    yield return null;

                    var postHandoffResult = new MobilePerformanceSceneResult();
                    MobilePerformanceBenchmarkRunner.CaptureRuntimeInventory(postHandoffResult);
                    Debug.Log(
                        "[MobilePerformance] RuntimeLoopBudget scene=OlympusCorridorPostHandoff "
                        + $"total={postHandoffResult.ActiveFrameLoopBehaviourCount} "
                        + $"update={postHandoffResult.ActiveUpdateBehaviourCount} "
                        + $"late={postHandoffResult.ActiveLateUpdateBehaviourCount} "
                        + $"fixed={postHandoffResult.ActiveFixedUpdateBehaviourCount} "
                        + $"footsteps={postHandoffResult.ConsolidatedFootstepPresenterCount} "
                        + $"activeColliders={postHandoffResult.ActiveColliderCount} "
                        + $"culledDetailColliders={postHandoffResult.EnvironmentDetailCulledColliderCount}");
                    Debug.Log(
                        "[MobilePerformance] RuntimeFrameLoopTypes scene=OlympusCorridorPostHandoff "
                        + FormatFrameLoops(postHandoffResult));
                    Assert.That(
                        postHandoffResult.ActiveFrameLoopBehaviourCount,
                        Is.LessThanOrEqualTo(15),
                        "Olympus corridor gameplay handoff exceeded its reviewed runtime callback budget. "
                        + FormatFrameLoops(postHandoffResult));
                    Assert.That(
                        FindFrameLoop(postHandoffResult, "Unity.Cinemachine.CinemachineCamera"),
                        Is.Null,
                        "Intro virtual cameras should leave the runtime loop after the gameplay handoff.");
                    Assert.That(
                        FindFrameLoop(postHandoffResult, "Unity.Cinemachine.CinemachineBrain"),
                        Is.Null,
                        "The intro camera brain should leave the runtime loop after the gameplay handoff.");
                    Assert.That(
                        FindFrameLoop(
                            postHandoffResult,
                            "DimensionBrawl.Presentation.IntroGatePodCutsceneCueDirector"),
                        Is.Null,
                        "The intro cue director should leave the runtime loop after the gameplay handoff.");
                    Assert.That(
                        FindFrameLoop(
                            postHandoffResult,
                            "DimensionBrawl.Presentation.IntroGatePodFirstPersonRendererMask"),
                        Is.Null,
                        "The first-person cutscene renderer mask should leave the runtime loop after handoff.");
                }
            }
        }

        private static MobilePerformanceFrameLoopInventory FindFrameLoop(
            MobilePerformanceSceneResult result,
            string typeName)
        {
            for (int i = 0; i < result.FrameLoops.Count; i++)
            {
                if (result.FrameLoops[i].TypeName == typeName)
                {
                    return result.FrameLoops[i];
                }
            }

            return null;
        }

        private static string FormatFrameLoops(MobilePerformanceSceneResult result)
        {
            var builder = new System.Text.StringBuilder();
            for (int i = 0; i < result.FrameLoops.Count; i++)
            {
                MobilePerformanceFrameLoopInventory loop = result.FrameLoops[i];
                if (i > 0)
                {
                    builder.Append(", ");
                }

                builder.Append(loop.TypeName)
                    .Append("[U=").Append(loop.UpdateInstances)
                    .Append(" L=").Append(loop.LateUpdateInstances)
                    .Append(" F=").Append(loop.FixedUpdateInstances)
                    .Append(']');
            }

            return builder.ToString();
        }

        private static MobilePerformanceMetricSummary CreateMetric(string label, string unit, double value)
        {
            return new MobilePerformanceMetricSummary
            {
                Label = label,
                Unit = unit,
                Valid = true,
                SampleCount = 1,
                Average = value,
                P50 = value,
                P95 = value,
                P99 = value,
                Maximum = value
            };
        }
    }
}
