using System;
using System.IO;
using System.Linq;
using DimensionBrawl.Combat;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace DimensionBrawl.Editor.AuditionPV.Tests
{
    public sealed class AuditionPvStationPhase2SummonCounterGoldenTests
    {
        [Test]
        public void CaptureContract_IsExactSixSecondG06()
        {
            Assert.That(
                AuditionPvStationPhase2SummonCounterCapture.ShotId,
                Is.EqualTo("g06"));
            Assert.That(
                AuditionPvStationPhase2SummonCounterCapture.FirstFrame,
                Is.Zero);
            Assert.That(
                AuditionPvStationPhase2SummonCounterCapture.LastFrame,
                Is.EqualTo(359));
            Assert.That(
                AuditionPvStationPhase2SummonCounterCapture.ExpectedFrameCount,
                Is.EqualTo(360));
            Assert.That(
                AuditionPvStationPhase2SummonCounterCapture.FrameTimeSeconds(359),
                Is.EqualTo(359f / 60f).Within(0.000001f));
            Assert.That(
                AuditionPvStationPhase2SummonCounterCapture.FrameFileName(359),
                Is.EqualTo("frame_0359.png"));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                AuditionPvStationPhase2SummonCounterCapture.FrameFileName(360));
        }

        [Test]
        public void RunnerTimeout_AllowsObservedQhdPngEncodingHeadroom()
        {
            string source = File.ReadAllText(ProjectAbsolutePath(
                AuditionPvStationPhase2SummonCounterGoldenRunner
                    .RunnerScriptPath));

            Assert.That(
                source,
                Does.Contain("private const double ShotTimeoutSeconds = 90d;"));
        }

        [Test]
        public void CaptureSchedule_PreservesG05AndAddsAuthoredSlot1Counter()
        {
            Assert.That(
                AuditionPvStationPhase2SummonCounterCapture.BeginWindupFrame,
                Is.EqualTo(1));
            Assert.That(
                AuditionPvStationPhase2SummonCounterCapture.FirePendingWaveFrame,
                Is.EqualTo(71));
            Assert.That(
                AuditionPvStationPhase2SummonCounterCapture.QueueDodgeFrame,
                Is.EqualTo(186));
            Assert.That(
                AuditionPvStationPhase2SummonCounterCapture.ImpactFrame,
                Is.EqualTo(188));
            Assert.That(
                AuditionPvStationPhase2SummonCounterCapture.ReleaseSummonInputFrame,
                Is.EqualTo(221));
            Assert.That(
                AuditionPvStationPhase2SummonCounterCapture.QueueSummonFrame,
                Is.EqualTo(222));
            Assert.That(
                AuditionPvStationPhase2SummonCounterCapture.RelockSummonInputFrame,
                Is.EqualTo(223));
            Assert.That(
                AuditionPvStationPhase2SummonCounterCapture.RetainedProjectileInterceptFrame,
                Is.EqualTo(250));
            Assert.That(
                AuditionPvStationPhase2SummonCounterCapture.Bl07SourceFrame,
                Is.EqualTo(251));
            Assert.That(
                AuditionPvStationPhase2SummonCounterCapture.AuthoredSummonTier,
                Is.EqualTo(2));
            Assert.That(
                AuditionPvStationPhase2SummonCounterCapture.AuthoredSummonManaCost,
                Is.EqualTo(200f));
            Assert.That(
                AuditionPvStationPhase2SummonCounterCapture.AuthoredEnergyAfterUse,
                Is.EqualTo(100f));
            Assert.That(
                AuditionPvStationPhase2SummonCounterCapture.AuthoredCounterDamage,
                Is.EqualTo(29.439999f).Within(0.000001f));
        }

        [Test]
        public void Manifest_IsAssemblerCompatibleG06WithThreeByteExactBaselines()
        {
            AuditionPvShotManifestEntry shot =
                AuditionPvStationPhase2SummonCounterCapture.CreateShotManifestEntry();
            AuditionPvBaselineManifestEntry[] baselines =
                AuditionPvStationPhase2SummonCounterCapture
                    .CreateBaselineManifestEntries();

            Assert.That(shot.id, Is.EqualTo("g06"));
            Assert.That(shot.startFrame, Is.Zero);
            Assert.That(shot.endFrame, Is.EqualTo(359));
            Assert.That(shot.expectedFrameCount, Is.EqualTo(360));
            Assert.That(shot.hudMode, Is.EqualTo("hud-on"));
            Assert.That(shot.notes, Does.Contain("QueueSummonSlot1 f222"));
            Assert.That(shot.notes, Does.Contain("automatic 29.44 counter"));
            Assert.That(baselines.Select(value => value.id),
                Is.EqualTo(new[] { "bl03", "bl06", "bl07" }));
            Assert.That(baselines[2].shotId, Is.EqualTo("g06"));
            Assert.That(baselines[2].sourceFrame, Is.EqualTo(251));
            Assert.That(baselines[2].fileName,
                Is.EqualTo(
                    "BL07_AKAZA_PHASE2_SUMMON_COUNTER__HUDON__t04.183333.png"));
        }

        [Test]
        public void Dependencies_PinTheActualSummonInterceptCounterProductChain()
        {
            string[] dependencies =
                AuditionPvStationPhase2SummonCounterCapture
                    .ExplicitProductDependencyPaths();
            string[] expected =
            {
                AuditionPvStationPhase2SummonCounterCapture.PlayerSummonActionPath,
                AuditionPvStationPhase2SummonCounterCapture.PlayerSummonRuntimePath,
                AuditionPvStationPhase2SummonCounterCapture.SummonSlotProfilePath,
                AuditionPvStationPhase2SummonCounterCapture.SummonEnergyLadderPath,
                AuditionPvStationPhase2SummonCounterCapture.SummonPressureScreenPath,
                AuditionPvStationPhase2SummonCounterCapture.SummonFrontlineProxyPath,
                AuditionPvStationPhase2SummonCounterCapture.LaneActionProjectilePath,
                AuditionPvStationPhase2SummonCounterCapture.SummonActorPrefabPath,
                AuditionPvStationPhase2SummonCounterCapture.SummonProjectilePrefabPath,
                AuditionPvStationPhase2SummonCounterCapture.SummonEntryCuePrefabPath
            };

            foreach (string path in expected)
            {
                Assert.That(dependencies, Does.Contain(path), path);
                Assert.That(
                    AssetDatabase.LoadMainAssetAtPath(path) != null
                        || File.Exists(ProjectAbsolutePath(path)),
                    Is.True,
                    path);
            }
        }

        [Test]
        public void CaptureSource_UsesIndependentDirectorAndOnlyPublicProductActions()
        {
            string source = File.ReadAllText(ProjectAbsolutePath(
                AuditionPvStationPhase2SummonCounterCapture.CaptureScriptPath));
            int counterPathStart = source.IndexOf(
                "private void ApplyActualPressureScreenIntercept()",
                StringComparison.Ordinal);
            int counterPathEnd = source.IndexOf(
                "private void DeactivateCaptureSummonArtifacts()",
                StringComparison.Ordinal);

            Assert.That(counterPathStart, Is.GreaterThanOrEqualTo(0));
            Assert.That(counterPathEnd, Is.GreaterThan(counterPathStart));
            string counterPath = source.Substring(
                counterPathStart,
                counterPathEnd - counterPathStart);

            Assert.That(source,
                Does.Contain("public sealed class AuditionPvStationPhase2SummonCounterDirector"));
            Assert.That(source,
                Does.Not.Contain("AuditionPvStationPhase2PerfectDodgeDirector"));
            Assert.That(source, Does.Contain("playerSummon.QueueSummonSlot1();"));
            Assert.That(counterPath,
                Does.Contain("interceptedCrushNetProjectile.TryApplyImpact("));
            Assert.That(counterPath,
                Does.Contain("IsExactCaptureOwnedRetainedProjectileSet(activeProjectiles)"));
            Assert.That(counterPath,
                Does.Not.Contain("emitter.CurrentPattern != crushNet"));
            Assert.That(source,
                Does.Contain("projectileLeases[leaseIndex].Projectile == candidate"));
            Assert.That(counterPath,
                Does.Contain("counterProjectile.DamageApplied +="));
            Assert.That(source, Does.Contain("bossHealth.Damaged += HandleBossDamaged;"));
            Assert.That(source,
                Does.Not.Contain("ConfigureRequiredSummonMana("));
            Assert.That(counterPath,
                Does.Not.Contain("counterProjectile.TryApplyImpact("));
            Assert.That(counterPath,
                Does.Not.Contain("bossHealth.TryApplyDamage("));
            Assert.That(source,
                Does.Contain("Time.fixedDeltaTime = 1f / AuditionPvCaptureContract.Fps;"));
            Assert.That(source,
                Does.Contain("Time.fixedDeltaTime = savedFixedDeltaTime;"));
        }

        [Test]
        public void GoldenRunner_RawWarmupMapsExactlyToLogicalSixSeconds()
        {
            Assert.That(
                AuditionPvStationPhase2SummonCounterGoldenRunner.RawWarmupFrame,
                Is.Zero);
            Assert.That(
                AuditionPvStationPhase2SummonCounterGoldenRunner.RawFirstShotFrame,
                Is.EqualTo(1));
            Assert.That(
                AuditionPvStationPhase2SummonCounterGoldenRunner.RawLastShotFrame,
                Is.EqualTo(360));
            Assert.That(
                AuditionPvStationPhase2SummonCounterGoldenRunner.ExpectedRawFrameCount,
                Is.EqualTo(361));
            Assert.That(
                AuditionPvStationPhase2SummonCounterGoldenRunner.RawFrameFileName(360),
                Is.EqualTo("frame_0360.png"));
            Assert.That(AuditionPvCaptureContract.Width, Is.EqualTo(2560));
            Assert.That(AuditionPvCaptureContract.Height, Is.EqualTo(1440));
            Assert.That(AuditionPvCaptureContract.Fps, Is.EqualTo(60));
        }

        [Test]
        public void RuntimeProof_AcceptsExactProductCounterCrossProof()
        {
            Assert.DoesNotThrow(() =>
                AuditionPvStationPhase2SummonCounterGoldenRunner
                    .ValidateRuntimeProof(CreatePassingRuntimeProof()));
        }

        [Test]
        public void RuntimeProof_RejectsMissingNaturalCounterDamageEvent()
        {
            AuditionPvStationPhase2SummonCounterGoldenRunner.RuntimeProof proof =
                CreatePassingRuntimeProof();
            proof.counterProjectileDamageAppliedCount = 0;

            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvStationPhase2SummonCounterGoldenRunner
                    .ValidateRuntimeProof(proof));
        }

        [Test]
        public void RuntimeProof_RejectsRetainedProjectileIdentitySubstitution()
        {
            AuditionPvStationPhase2SummonCounterGoldenRunner.RuntimeProof proof =
                CreatePassingRuntimeProof();
            proof.retainedProjectileIdentitySetExact = false;

            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvStationPhase2SummonCounterGoldenRunner
                    .ValidateRuntimeProof(proof));
        }

        [Test]
        public void RuntimeProof_RejectsUnreleasedCaptureEvents()
        {
            AuditionPvStationPhase2SummonCounterGoldenRunner.RuntimeProof proof =
                CreatePassingRuntimeProof();
            proof.captureEventsReleased = false;

            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvStationPhase2SummonCounterGoldenRunner
                    .ValidateRuntimeProof(proof));
        }

        [Test]
        public void PixelDeltaGates_RequireVisiblePerfectDodgeAndCounterChanges()
        {
            var before = Enumerable.Repeat(
                    new Color32(20, 30, 40, 255),
                    128)
                .ToArray();
            var after = Enumerable.Repeat(
                    new Color32(80, 110, 140, 255),
                    128)
                .ToArray();
            AuditionPvStationPhase2SummonCounterGoldenRunner.ScreenDeltaMetrics delta =
                AuditionPvStationPhase2SummonCounterGoldenRunner
                    .EvaluateScreenDelta(before, after);

            Assert.DoesNotThrow(() =>
                AuditionPvStationPhase2SummonCounterGoldenRunner
                    .ValidateScreenDelta(delta));
            Assert.DoesNotThrow(() =>
                AuditionPvStationPhase2SummonCounterGoldenRunner
                    .ValidateCounterDelta(delta));
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvStationPhase2SummonCounterGoldenRunner
                    .ValidateCounterDelta(
                        AuditionPvStationPhase2SummonCounterGoldenRunner
                            .EvaluateScreenDelta(before, before)));
        }

        private static AuditionPvStationPhase2SummonCounterGoldenRunner.RuntimeProof
            CreatePassingRuntimeProof()
        {
            return new AuditionPvStationPhase2SummonCounterGoldenRunner.RuntimeProof
            {
                directorCompleted = true,
                lastLogicalFrame = 359,
                presentedFrameCount = 360,
                presentedFramesExact = true,
                presentationClockExact = true,
                perfectDodgeCount = 1,
                firedProjectileCount = 7,
                usedActualCrushNetPattern = true,
                impactAppliedOrBlocked = true,
                impactProjectileInactive = true,
                damageBlockedObservationCount = 1,
                damageModifyingObservationCount = 0,
                playerHealthUnchanged = true,
                cameraCueRequested = true,
                screenCueRequested = true,
                screenCueActiveAtBaselineFrame = true,
                productScreenProfileActive = true,
                bossRiskAtFirstFrame = 0.6f,
                bossRiskAtFireFrame = 0.9f,
                bossRiskAtImpactFrame = 0.91f,
                exactHudRenderable = true,
                exactHudResources = true,
                exactEnergyBinding = true,
                hudAmmo = 12,
                hudMagazineSize = 12,
                hudEnergyMana = 100f,
                hudEnergyMaxMana = 300f,
                summonEnergyBeforeUse = 300f,
                summonEnergyAfterUse = 100f,
                summonSpentTier = 2,
                summonUseCountDelta = 1,
                summonInterceptCountDelta = 1,
                summonUsedEventCount = 1,
                summonBlockedEventCount = 1,
                screenInterceptEventCount = 1,
                screenFirstObservedFrame = 239,
                summonPressureScreenTier = 2,
                summonPressureScreenRemainingIntercepts = 1,
                uniqueSummonPressureScreenObserved = true,
                retainedProjectileCountBeforeIntercept = 6,
                retainedProjectileIdentitySetExact = true,
                retainedProjectileImpactApplied = true,
                retainedProjectileInactive = true,
                activeCounterProjectileCountAfterIntercept = 1,
                bossDamageEventCount = 1,
                bossAllyDamageEventCount = 1,
                bossCounterDamageEventCount = 1,
                bossCounterDamageFrame = 280,
                counterProjectileDamageAppliedCount = 1,
                counterProjectileDamageAppliedFrame = 280,
                authoredCounterDamage = 29.439999f,
                bossCounterDamageAmount = 29.439999f,
                bossCounterHealthDelta = 29.439999f,
                fixedDeltaTimeExact = true,
                recorderWarmupEndOfFrameCount = 2,
                recorderPaddingActiveAtLogicalFrameZero = true,
                recorderAutoStoppedAfterLastFrame = true,
                stateRestored = true,
                screenProfileRestored = true,
                fixedDeltaTimeRestored = true,
                captureInputLocksReleased = true,
                captureHudStateRestored = true,
                captureEventsReleased = true,
                captureSummonArtifactsReleased = true,
                bossCompositionRestored = true,
                presentationClockReleased = true,
                cadenceSuspensionCountAfterRestore = 0
            };
        }

        private static string ProjectAbsolutePath(string projectRelativePath)
        {
            return Path.GetFullPath(Path.Combine(
                Application.dataPath,
                "..",
                projectRelativePath.Replace('/', Path.DirectorySeparatorChar)));
        }
    }
}
