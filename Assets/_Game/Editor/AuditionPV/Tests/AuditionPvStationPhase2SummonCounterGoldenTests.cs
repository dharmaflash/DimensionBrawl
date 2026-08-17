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
        public void CaptureContract_IsExactTwelveSecondSourceWithSixSecondLogicalCore()
        {
            Assert.That(
                AuditionPvStationPhase2SummonCounterCapture.ShotId,
                Is.EqualTo("g06"));
            Assert.That(
                AuditionPvStationPhase2SummonCounterCapture.FirstFrame,
                Is.Zero);
            Assert.That(
                AuditionPvStationPhase2SummonCounterCapture.LastFrame,
                Is.EqualTo(719));
            Assert.That(
                AuditionPvStationPhase2SummonCounterCapture.ExpectedFrameCount,
                Is.EqualTo(720));
            Assert.That(
                AuditionPvStationPhase2SummonCounterCapture.LogicalFirstFrame,
                Is.Zero);
            Assert.That(
                AuditionPvStationPhase2SummonCounterCapture.LogicalLastFrame,
                Is.EqualTo(359));
            Assert.That(
                AuditionPvStationPhase2SummonCounterCapture
                    .LogicalExpectedFrameCount,
                Is.EqualTo(360));
            Assert.That(
                AuditionPvStationPhase2SummonCounterCapture.HandleFrameCount,
                Is.EqualTo(180));
            Assert.That(
                AuditionPvStationPhase2SummonCounterCapture.SelectStartFrame,
                Is.EqualTo(180));
            Assert.That(
                AuditionPvStationPhase2SummonCounterCapture.SelectEndFrame,
                Is.EqualTo(539));
            Assert.That(
                AuditionPvStationPhase2SummonCounterCapture
                    .RecommendedSelectStartFrame,
                Is.EqualTo(360));
            Assert.That(
                AuditionPvStationPhase2SummonCounterCapture
                    .RecommendedSelectEndFrame,
                Is.EqualTo(496));
            Assert.That(
                AuditionPvStationPhase2SummonCounterCapture
                    .LogicalToSourceFrame(359),
                Is.EqualTo(539));
            Assert.That(
                AuditionPvStationPhase2SummonCounterCapture
                    .SourceToLogicalFrame(180),
                Is.Zero);
            Assert.That(
                AuditionPvStationPhase2SummonCounterCapture
                    .SourceToLogicalFrame(719),
                Is.EqualTo(-1));
            Assert.That(
                AuditionPvStationPhase2SummonCounterCapture.FrameTimeSeconds(719),
                Is.EqualTo(719f / 60f).Within(0.000001f));
            Assert.That(
                AuditionPvStationPhase2SummonCounterCapture.FrameFileName(719),
                Is.EqualTo("frame_0719.png"));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                AuditionPvStationPhase2SummonCounterCapture.FrameFileName(720));
        }

        [Test]
        public void RunnerTimeout_AllowsObservedQhdPngEncodingHeadroom()
        {
            string source = File.ReadAllText(ProjectAbsolutePath(
                AuditionPvStationPhase2SummonCounterGoldenRunner
                    .RunnerScriptPath));

            Assert.That(
                source,
                Does.Contain("private const double ShotTimeoutSeconds = 210d;"));
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
                Is.EqualTo(431));
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
                AuditionPvStationPhase2SummonCounterCapture
                    .AuthoredFollowupEnergyPulse,
                Is.EqualTo(300f));
            Assert.That(
                AuditionPvStationPhase2SummonCounterCapture.RequestSkill1Frame,
                Is.EqualTo(276));
            Assert.That(
                AuditionPvStationPhase2SummonCounterCapture.AuthoredSkill1Tier,
                Is.EqualTo(3));
            Assert.That(
                AuditionPvStationPhase2SummonCounterCapture
                    .AuthoredTierThreeLaserDamage,
                Is.EqualTo(54f));
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
            Assert.That(shot.endFrame, Is.EqualTo(719));
            Assert.That(shot.expectedFrameCount, Is.EqualTo(720));
            Assert.That(shot.hudMode, Is.EqualTo("hud-on"));
            Assert.That(shot.notes, Does.Contain("QueueSummonSlot1 f222"));
            Assert.That(shot.notes, Does.Contain("automatic 29.44 counter"));
            Assert.That(shot.notes, Does.Contain("HUD RequestSkill1 f276"));
            Assert.That(shot.notes, Does.Contain("source f180..f539"));
            Assert.That(baselines.Select(value => value.id),
                Is.EqualTo(new[] { "bl03", "bl06", "bl07" }));
            Assert.That(baselines[2].shotId, Is.EqualTo("g06"));
            Assert.That(
                baselines.Select(value => value.sourceFrame),
                Is.EqualTo(new[] { 180, 369, 431 }));
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
                AuditionPvStationPhase2SummonCounterCapture.SummonEntryCuePrefabPath,
                AuditionPvStationPhase2SummonCounterCapture.BossEncounterPath,
                AuditionPvStationPhase2SummonCounterCapture.FrontlineStageProfilePath,
                AuditionPvStationPhase2SummonCounterCapture.SummonOpportunityProfilePath,
                AuditionPvStationPhase2SummonCounterCapture.PlayerSkill1ActionPath,
                AuditionPvStationPhase2SummonCounterCapture.PlayerSkill1LaserSweepPath,
                AuditionPvStationPhase2SummonCounterCapture
                    .PlayerSkill1LaserSweepPrefabPath,
                AuditionPvStationPhase2SummonCounterCapture.CombatHudInputBridgePath,
                AuditionPvStationPhase2SummonCounterCapture.CombatHudBinderPath,
                AuditionPvStationPhase2SummonCounterCapture.ActionCameraCueDriverPath,
                AuditionPvStationPhase2SummonCounterCapture
                    .ActionCinematicCueDirectorPath,
                AuditionPvStationPhase2SummonCounterCapture
                    .ActionCinematicCueProfilePath,
                AuditionPvStationPhase2SummonCounterCapture
                    .BossBarragePocketCameraCueBridgePath,
                AuditionPvStationPhase2SummonCounterCapture
                    .SixtySecondGateManifestPath
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
                Does.Contain("encounter.Tick(0f);"));
            Assert.That(source,
                Does.Contain("combatHudInputBridge.RequestSkill1();"));
            Assert.That(source,
                Does.Not.Contain("playerSkill1.QueueSkill1("));
            Assert.That(source,
                Does.Not.Contain("playerSkill1.TryUseSkill1("));
            Assert.That(source,
                Does.Not.Contain("playerSkill1LaserSweep.TryCastLaserSweep("));
            Assert.That(source,
                Does.Not.Contain("actionCinematicCueDirector.TryPlay("));
            Assert.That(source,
                Does.Contain("Time.fixedDeltaTime = 1f / AuditionPvCaptureContract.Fps;"));
            Assert.That(source,
                Does.Contain("Time.fixedDeltaTime = savedFixedDeltaTime;"));
        }

        [Test]
        public void GoldenRunner_RawWarmupMapsExactlyToCanonicalHandledSource()
        {
            Assert.That(
                AuditionPvStationPhase2SummonCounterGoldenRunner.RawWarmupFrame,
                Is.Zero);
            Assert.That(
                AuditionPvStationPhase2SummonCounterGoldenRunner.RawFirstShotFrame,
                Is.EqualTo(1));
            Assert.That(
                AuditionPvStationPhase2SummonCounterGoldenRunner.RawLastShotFrame,
                Is.EqualTo(720));
            Assert.That(
                AuditionPvStationPhase2SummonCounterGoldenRunner.ExpectedRawFrameCount,
                Is.EqualTo(721));
            Assert.That(
                AuditionPvStationPhase2SummonCounterGoldenRunner.RawFrameFileName(720),
                Is.EqualTo("frame_0720.png"));
            Assert.That(AuditionPvCaptureContract.Width, Is.EqualTo(2560));
            Assert.That(AuditionPvCaptureContract.Height, Is.EqualTo(1440));
            Assert.That(AuditionPvCaptureContract.Fps, Is.EqualTo(60));
        }

        [Test]
        public void CanonicalFrameLedger_CoversEveryHandledSourceFrame()
        {
            Assert.That(
                AuditionPvStationPhase2SummonCounterCapture
                    .FrameHashLedgerFileName,
                Is.EqualTo("frame_hashes.sha256"));
            Assert.That(
                AuditionPvStationPhase2SummonCounterCapture
                    .FrameLedgerRelativePath(0),
                Is.EqualTo("frames/g06/frame_0000.png"));
            Assert.That(
                AuditionPvStationPhase2SummonCounterCapture
                    .FrameLedgerRelativePath(719),
                Is.EqualTo("frames/g06/frame_0719.png"));

            string runnerSource = File.ReadAllText(ProjectAbsolutePath(
                AuditionPvStationPhase2SummonCounterGoldenRunner
                    .RunnerScriptPath));
            Assert.That(runnerSource,
                Does.Contain(".BuildFrameHashLedger(frameDirectory)"));
            Assert.That(runnerSource,
                Does.Contain(".ValidateFrameHashLedger("));
            Assert.That(runnerSource,
                Does.Contain("WriteTextNew(frameHashLedgerPath, frameHashLedger);"));
        }

        [Test]
        public void GateEvidence_AuthorsExactShotAndReusableS070S080SemanticBeats()
        {
            Assert.That(
                AuditionPvStationPhase2SummonCounterCapture
                    .GateEvidenceTestSuite,
                Is.EqualTo("AuditionPvSixtySecondEvidence"));
            Assert.That(
                AuditionPvStationPhase2SummonCounterCapture
                    .GateSemanticBeatIds(),
                Is.EqualTo(new[]
                {
                    "boss-pattern-1",
                    "olympus-hud-gameplay",
                    "perfect-dodge",
                    "summon-defense",
                    "player-tier3-ultimate"
                }));

            string runnerSource = File.ReadAllText(ProjectAbsolutePath(
                AuditionPvStationPhase2SummonCounterGoldenRunner
                    .RunnerScriptPath));
            Assert.That(runnerSource,
                Does.Contain(".CaptureCoreSha256(captureCoreManifest)"));
            Assert.That(runnerSource,
                Does.Contain("\"shot-authorship/\""));
            Assert.That(runnerSource,
                Does.Contain("\"shot-authorship-runtime/\""));
            Assert.That(runnerSource,
                Does.Contain("\"semantic-beat/\" + spec.beatId"));
            Assert.That(runnerSource,
                Does.Contain("artifact-sha256={artifactSha256}; semantic-fact={spec.beatId}"));
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
        public void RuntimeProof_RejectsMissingEncounterPulseOrHudSkillTraversal()
        {
            AuditionPvStationPhase2SummonCounterGoldenRunner.RuntimeProof proof =
                CreatePassingRuntimeProof();
            proof.encounterFollowupPulseTraversed = false;

            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvStationPhase2SummonCounterGoldenRunner
                    .ValidateRuntimeProof(proof));

            proof = CreatePassingRuntimeProof();
            proof.hudSkill1RequestTraversed = false;
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvStationPhase2SummonCounterGoldenRunner
                    .ValidateRuntimeProof(proof));
        }

        [Test]
        public void RuntimeProof_RejectsWrongCinematicOrderOrMissingPostHandle()
        {
            AuditionPvStationPhase2SummonCounterGoldenRunner.RuntimeProof proof =
                CreatePassingRuntimeProof();
            proof.cinematicPlayCountDeltaAtSkill1 = 1;

            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvStationPhase2SummonCounterGoldenRunner
                    .ValidateRuntimeProof(proof));

            proof = CreatePassingRuntimeProof();
            proof.recordedPostHandleFrameCount = 179;
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
                hudEnergyMana = 0f,
                hudEnergyMaxMana = 300f,
                summonEnergyBeforeUse = 300f,
                summonEnergyAfterUse = 100f,
                interceptEnergyBeforePulse = 100f,
                interceptEnergyAfterPulse = 300f,
                encounterFollowupPulseTraversed = true,
                followupWindowActiveAfterIntercept = true,
                encounterGrantedSummonFollowupEnergy = true,
                encounterSummonFollowupEnergyPulse = 300f,
                encounterLastSummonPressureBreakTier = 2,
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
                skill1UsedEventCount = 1,
                skill1UsedFrame = 276,
                skill1UsedTier = 3,
                skill1UseCountDelta = 1,
                skill1EnergyBeforeRequest = 300f,
                skill1EnergyAfterRequest = 0f,
                hudSkill1RequestTraversed = true,
                laserSweepActiveAfterHudRequest = true,
                laserSweepInactiveAfterPostHandle = true,
                summonFollowupHitEventCount = 1,
                summonFollowupHitFrame = 276,
                summonFollowupHitTier = 3,
                summonFollowupHitDamage = 54f,
                encounterUsedSkill1DuringSummonFollowup = true,
                encounterSkill1FollowupHitConfirmed = true,
                encounterHighestSkill1FollowupHitTier = 3,
                cinematicPlayCountDeltaAtSkill1 = 2,
                cinematicFollowupPlayCountDeltaAtSkill1 = 1,
                cinematicFollowupThenUltimateExact = true,
                fixedDeltaTimeExact = true,
                recorderWarmupEndOfFrameCount = 2,
                recorderPreHandleEndOfFrameCount = 180,
                canonicalSourceFrameCount = 720,
                logicalFirstSourceFrame = 180,
                logicalLastSourceFrame = 539,
                recordedPreHandleFrameCount = 180,
                recordedPostHandleFrameCount = 180,
                recorderPaddingActiveAtLogicalFrameZero = true,
                recorderAutoStoppedAfterLastFrame = true,
                stateRestored = true,
                screenProfileRestored = true,
                fixedDeltaTimeRestored = true,
                captureInputLocksReleased = true,
                captureHudStateRestored = true,
                captureEventsReleased = true,
                captureSummonArtifactsReleased = true,
                captureSkillArtifactsReleased = true,
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
