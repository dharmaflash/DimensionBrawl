using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace DimensionBrawl.Editor.AuditionPV.Tests
{
    public sealed class AuditionPvStationPhaseOneBossLowAngleCaptureTests
    {
        [Test]
        public void SourceContract_PartitionsSixHundredFramesExactly()
        {
            Assert.That(
                AuditionPvStationPhaseOneBossLowAngleCapture.SourceFrameCount,
                Is.EqualTo(600));
            Assert.That(
                AuditionPvStationPhaseOneBossLowAngleCapture.PreHandleFrameCount,
                Is.EqualTo(180));
            Assert.That(
                AuditionPvStationPhaseOneBossLowAngleCapture.SelectedFrameCount,
                Is.EqualTo(240));
            Assert.That(
                AuditionPvStationPhaseOneBossLowAngleCapture.PostHandleFrameCount,
                Is.EqualTo(180));
            Assert.That(
                AuditionPvStationPhaseOneBossLowAngleCapture.PreHandleFrameCount
                + AuditionPvStationPhaseOneBossLowAngleCapture.SelectedFrameCount
                + AuditionPvStationPhaseOneBossLowAngleCapture.PostHandleFrameCount,
                Is.EqualTo(
                    AuditionPvStationPhaseOneBossLowAngleCapture.SourceFrameCount));

            Assert.That(
                AuditionPvStationPhaseOneBossLowAngleCapture
                    .SourceToSelectedLogicalFrame(179),
                Is.EqualTo(-1));
            Assert.That(
                AuditionPvStationPhaseOneBossLowAngleCapture
                    .SourceToSelectedLogicalFrame(180),
                Is.EqualTo(0));
            Assert.That(
                AuditionPvStationPhaseOneBossLowAngleCapture
                    .SourceToSelectedLogicalFrame(419),
                Is.EqualTo(239));
            Assert.That(
                AuditionPvStationPhaseOneBossLowAngleCapture
                    .SourceToSelectedLogicalFrame(420),
                Is.EqualTo(-1));
            Assert.That(
                Enumerable.Range(0, 600).Count(frame =>
                    AuditionPvStationPhaseOneBossLowAngleCapture
                        .SourceFrameRole(frame) == "selected"),
                Is.EqualTo(240));
        }

        [Test]
        public void RailProgress_HoldsPreAndPostHandleEndpoints()
        {
            Assert.That(
                AuditionPvStationPhaseOneBossLowAngleCapture
                    .RailProgressForSourceFrame(0),
                Is.Zero);
            Assert.That(
                AuditionPvStationPhaseOneBossLowAngleCapture
                    .RailProgressForSourceFrame(179),
                Is.Zero);
            Assert.That(
                AuditionPvStationPhaseOneBossLowAngleCapture
                    .RailProgressForSourceFrame(180),
                Is.Zero);
            Assert.That(
                AuditionPvStationPhaseOneBossLowAngleCapture
                    .RailProgressForSourceFrame(419),
                Is.EqualTo(1f));
            Assert.That(
                AuditionPvStationPhaseOneBossLowAngleCapture
                    .RailProgressForSourceFrame(420),
                Is.EqualTo(1f));
            Assert.That(
                AuditionPvStationPhaseOneBossLowAngleCapture
                    .RailProgressForSourceFrame(599),
                Is.EqualTo(1f));
        }

        [Test]
        public void ThreeRailPresets_AreDistinctDeterministicAndLowAngle()
        {
            var bounds = new Bounds(
                new Vector3(0f, 2.5f, 18f),
                new Vector3(2.2f, 5f, 1.8f));
            string[] ids = Enumerable.Range(1, 3)
                .Select(ordinal =>
                    AuditionPvStationPhaseOneBossLowAngleCapture
                        .GetRailPreset(ordinal).Id)
                .ToArray();
            Assert.That(ids.Distinct().Count(), Is.EqualTo(3));

            for (int ordinal = 1; ordinal <= 3; ordinal++)
            {
                AuditionPvStationPhaseOneBossLowAngleCapture.RailPreset preset =
                    AuditionPvStationPhaseOneBossLowAngleCapture
                        .GetRailPreset(ordinal);
                for (int sample = 0; sample <= 24; sample++)
                {
                    float progress = sample / 24f;
                    AuditionPvStationPhaseOneBossLowAngleCapture.CameraPose first =
                        AuditionPvStationPhaseOneBossLowAngleCapture.ResolveRailPose(
                            bounds,
                            Vector3.back,
                            preset,
                            progress);
                    AuditionPvStationPhaseOneBossLowAngleCapture.CameraPose second =
                        AuditionPvStationPhaseOneBossLowAngleCapture.ResolveRailPose(
                            bounds,
                            Vector3.back,
                            preset,
                            progress);
                    Assert.That(Vector3.Distance(first.Position, second.Position), Is.Zero);
                    Assert.That(Quaternion.Angle(first.Rotation, second.Rotation), Is.Zero);
                    Assert.That(first.FieldOfView, Is.EqualTo(40f));

                    AuditionPvStationPhaseOneBossLowAngleCapture.CameraComposition
                        composition = AuditionPvStationPhaseOneBossLowAngleCapture
                            .EvaluateComposition(bounds, first);
                    Assert.That(composition.AllCornersInFront, Is.True);
                    Assert.That(
                        composition.ProjectedHeight,
                        Is.InRange(
                            AuditionPvStationPhaseOneBossLowAngleCapture
                                .MinimumSelectedProjectedHeight,
                            AuditionPvStationPhaseOneBossLowAngleCapture
                                .MaximumSelectedProjectedHeight));
                    Assert.That(
                        composition.EyeHeightRatio,
                        Is.LessThanOrEqualTo(
                            AuditionPvStationPhaseOneBossLowAngleCapture
                                .MaximumLowAngleEyeRatio));
                    Assert.That((first.Rotation * Vector3.forward).y, Is.GreaterThan(0f));
                }
            }
        }

        [Test]
        public void ShotManifest_DeclaresFullSourceAndSelectedJoin()
        {
            for (int ordinal = 1; ordinal <= 3; ordinal++)
            {
                AuditionPvShotManifestEntry shot =
                    AuditionPvStationPhaseOneBossLowAngleCapture
                        .CreateShotManifestEntry(ordinal);
                Assert.That(shot.id, Is.EqualTo("s050"));
                Assert.That(shot.scenePath, Is.EqualTo(
                    AuditionPvStationPhaseOneBossLowAngleCapture.StationScenePath));
                Assert.That(shot.startFrame, Is.Zero);
                Assert.That(shot.endFrame, Is.EqualTo(599));
                Assert.That(shot.expectedFrameCount, Is.EqualTo(600));
                Assert.That(shot.hudMode, Is.EqualTo("hud-off"));
                Assert.That(shot.notes, Does.Contain("select f180..f419"));
                Assert.That(shot.notes, Does.Contain($"take ordinal {ordinal}"));
                Assert.That(
                    AuditionPvStationPhaseOneBossLowAngleCapture.CameraId(ordinal),
                    Does.StartWith("station-gameplay-camera-s050-"));
                Assert.That(
                    AuditionPvStationPhaseOneBossLowAngleCapture
                        .DeterministicSeed(ordinal),
                    Is.EqualTo(
                        AuditionPvStationPhaseOneBossLowAngleCapture
                            .DeterministicRandomSeed + ordinal));

                AuditionPvBaselineManifestEntry[] baselines =
                    AuditionPvStationPhaseOneBossLowAngleCapture
                        .CreateBaselineManifestEntries(ordinal);
                Assert.That(
                    baselines.Select(value => value.sourceFrame),
                    Is.EqualTo(new[] { 180, 300, 419 }));
                Assert.That(baselines.All(value => value.hudMode == "hud-off"),
                    Is.True);
            }
        }

        [Test]
        public void CanonicalPhaseOnePrefabGuid_IsPinnedToExpectedAsset()
        {
            string actualGuid = AssetDatabase.AssetPathToGUID(
                AuditionPvStationPhaseOneBossLowAngleCapture
                    .PhaseOneVisualPrefabPath);
            Assert.That(actualGuid, Is.EqualTo(
                AuditionPvStationPhaseOneBossLowAngleCapture
                    .PhaseOneVisualPrefabGuid));
            Assert.That(
                AssetDatabase.LoadAssetAtPath<GameObject>(
                    AuditionPvStationPhaseOneBossLowAngleCapture
                        .PhaseOneVisualPrefabPath),
                Is.Not.Null);
            Assert.That(
                AssetDatabase.LoadAssetAtPath<SceneAsset>(
                    AuditionPvStationPhaseOneBossLowAngleCapture.StationScenePath),
                Is.Not.Null);
        }

        [Test]
        public void InvalidTakeAndSourceFrames_FailClosed()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                AuditionPvStationPhaseOneBossLowAngleCapture.GetRailPreset(0));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                AuditionPvStationPhaseOneBossLowAngleCapture.GetRailPreset(4));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                AuditionPvStationPhaseOneBossLowAngleCapture.FrameFileName(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                AuditionPvStationPhaseOneBossLowAngleCapture.FrameFileName(600));
        }

        [Test]
        public void GateBucket_UsesNonCoreS050RoleAndBothExactBeatIds()
        {
            AuditionPvSixtySecondRequiredBucket bucket =
                AuditionPvSixtySecondGateManifestValidator.RequiredBuckets
                    .Single(value => value.bucketId == "PV_S050");
            Assert.That(bucket.role, Is.EqualTo(
                AuditionPvStationPhaseOneBossLowAngleGoldenRunner
                    .CombinedSemanticFact));
            Assert.That(
                bucket.requiredBeatIds,
                Is.EqualTo(
                    AuditionPvStationPhaseOneBossLowAngleGoldenRunner
                        .RequiredSemanticBeatFacts));
        }
    }
}
