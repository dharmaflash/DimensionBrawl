using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;

namespace DimensionBrawl.Editor.AuditionPV.Tests
{
    public sealed class AuditionPvCityHitDodgeSummonCaptureTests
    {
        [Test]
        public void SourceContract_HasExactSelectedRangeAndRealHandles()
        {
            Assert.That(
                AuditionPvCityHitDodgeSummonCapture.SegmentId,
                Is.EqualTo("PV_S030"));
            Assert.That(
                AuditionPvCityHitDodgeSummonCapture.ShotId,
                Is.EqualTo("s030"));
            Assert.That(
                AuditionPvCityHitDodgeSummonCapture.FirstSourceFrame,
                Is.Zero);
            Assert.That(
                AuditionPvCityHitDodgeSummonCapture.LastSourceFrame,
                Is.EqualTo(719));
            Assert.That(
                AuditionPvCityHitDodgeSummonCapture.SourceFrameCount,
                Is.EqualTo(720));
            Assert.That(
                AuditionPvCityHitDodgeSummonCapture.SelectedFirstSourceFrame,
                Is.EqualTo(180));
            Assert.That(
                AuditionPvCityHitDodgeSummonCapture.SelectedLastSourceFrame,
                Is.EqualTo(539));
            Assert.That(
                AuditionPvCityHitDodgeSummonCapture.SelectedFrameCount,
                Is.EqualTo(360));
            Assert.That(
                AuditionPvCityHitDodgeSummonCapture.PreHandleFrameCount,
                Is.EqualTo(180));
            Assert.That(
                AuditionPvCityHitDodgeSummonCapture.PostHandleFrameCount,
                Is.EqualTo(180));

            Assert.That(
                AuditionPvCityHitDodgeSummonCapture.SourceFrameRole(0),
                Is.EqualTo("pre-handle"));
            Assert.That(
                AuditionPvCityHitDodgeSummonCapture.SourceFrameRole(179),
                Is.EqualTo("pre-handle"));
            Assert.That(
                AuditionPvCityHitDodgeSummonCapture.SourceFrameRole(180),
                Is.EqualTo("selected"));
            Assert.That(
                AuditionPvCityHitDodgeSummonCapture.SourceFrameRole(539),
                Is.EqualTo("selected"));
            Assert.That(
                AuditionPvCityHitDodgeSummonCapture.SourceFrameRole(540),
                Is.EqualTo("post-handle"));
            Assert.That(
                AuditionPvCityHitDodgeSummonCapture.SourceFrameRole(719),
                Is.EqualTo("post-handle"));
            Assert.That(
                AuditionPvCityHitDodgeSummonCapture
                    .SourceToSelectedLogicalFrame(180),
                Is.Zero);
            Assert.That(
                AuditionPvCityHitDodgeSummonCapture
                    .SourceToSelectedLogicalFrame(539),
                Is.EqualTo(359));
        }

        [Test]
        public void ShotManifest_DescribesPhysicalSourceAndSemanticChain()
        {
            AuditionPvShotManifestEntry shot =
                AuditionPvCityHitDodgeSummonCapture
                    .CreateShotManifestEntry();

            Assert.That(shot.id, Is.EqualTo("s030"));
            Assert.That(
                shot.scenePath,
                Is.EqualTo(
                    "Assets/_Game/Scenes/CityHeroPocketStage.unity"));
            Assert.That(shot.startFrame, Is.Zero);
            Assert.That(shot.endFrame, Is.EqualTo(719));
            Assert.That(shot.expectedFrameCount, Is.EqualTo(720));
            Assert.That(shot.hudMode, Is.EqualTo("hud-on"));
            Assert.That(shot.notes, Does.Contain("select f180..f539"));
            Assert.That(shot.notes, Does.Contain("180-frame pre/post handles"));
            Assert.That(shot.notes, Does.Contain("PerfectDodgeTriggered"));
            Assert.That(shot.notes, Does.Contain("tier-2 ChargeBruiser"));
        }

        [Test]
        public void SixtySecondGate_UsesExactSuiteAndSemanticBeatIds()
        {
            Assert.That(
                AuditionPvCityHitDodgeSummonCapture.GateEvidenceTestSuite,
                Is.EqualTo("AuditionPvSixtySecondEvidence"));
            Assert.That(
                AuditionPvCityHitDodgeSummonCapture.GateSemanticBeatIds(),
                Is.EqualTo(new[]
                {
                    "player-hit",
                    "perfect-dodge",
                    "summon-chain"
                }));
            Assert.That(
                AuditionPvCityHitDodgeSummonCapture.GateCameraId,
                Is.EqualTo("city-hero-pocket-action-camera"));
            Assert.That(
                AuditionPvCityHitDodgeSummonCapture.GateGameplayState,
                Does.Contain("perfect-dodge"));
            Assert.That(
                AuditionPvCityHitDodgeSummonCapture.GateTimelineId,
                Does.Contain("source-000-719-select-180-539"));
        }

        [Test]
        public void ExistingAssets_ArePinnedByExactPublicPathsAndGuids()
        {
            Assert.That(
                AssetDatabase.AssetPathToGUID(
                    AuditionPvCityHitDodgeSummonCapture
                        .ChargeBruiserProfilePath),
                Is.EqualTo(
                    AuditionPvCityHitDodgeSummonCapture
                        .ChargeBruiserProfileGuid));
            Assert.That(
                AssetDatabase.AssetPathToGUID(
                    AuditionPvCityHitDodgeSummonCapture
                        .RifleCrossfirePrefabPath),
                Is.EqualTo(
                    AuditionPvCityHitDodgeSummonCapture
                        .RifleCrossfirePrefabGuid));

            string[] paths = AuditionPvCityHitDodgeSummonCapture
                .ExplicitProductDependencyPaths();
            CollectionAssert.Contains(
                paths,
                "Assets/_Game/Scripts/Combat/SummonEnergyLadder.cs");
            CollectionAssert.Contains(
                paths,
                "Assets/_Game/Scripts/Player/PlayerSummonSlot1Action.cs");
            CollectionAssert.Contains(
                paths,
                "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_SummonSlot1_ChargeBruiser.asset");
            CollectionAssert.Contains(
                paths,
                "Assets/_Game/Prefabs/Combat/PF_SummonSlot1Projectile_AssistBolt.prefab");
            CollectionAssert.Contains(
                paths,
                "Assets/_Game/Prefabs/Combat/PF_SummonSlot1Actor_Proxy.prefab");
            CollectionAssert.Contains(
                paths,
                "Assets/_Game/UI/CombatHud/CombatHudInputBridge.cs");
            CollectionAssert.Contains(
                paths,
                AuditionPvCityHitDodgeSummonCapture.CaptureScriptPath);
            CollectionAssert.Contains(
                paths,
                AuditionPvCityHitDodgeSummonCapture.RunnerScriptPath);
            CollectionAssert.Contains(
                paths,
                AuditionPvCityHitDodgeSummonCapture.CaptureTestPath);
            CollectionAssert.Contains(
                paths,
                AuditionPvCityHitDodgeSummonCapture.RunnerTestPath);
            Assert.That(
                paths.Distinct(StringComparer.OrdinalIgnoreCase).Count(),
                Is.EqualTo(paths.Length));
        }

        [Test]
        public void CaptureSource_UsesHudAndActualEventsWithoutForbiddenBypasses()
        {
            string source = File.ReadAllText(
                ProjectAbsolutePath(
                    AuditionPvCityHitDodgeSummonCapture
                        .CaptureScriptPath));

            Assert.That(source, Does.Contain("hudInput.RequestDodge();"));
            Assert.That(
                source,
                Does.Contain("hudInput.RequestSummonSlot1();"));
            Assert.That(
                source,
                Does.Contain("playerAction.PerfectDodgeTriggered +="));
            Assert.That(source, Does.Contain("playerHealth.Damaged +="));
            Assert.That(source, Does.Contain("enemyHealth.Damaged +="));
            Assert.That(
                source,
                Does.Contain("summon.SummonSlot1Used +="));
            Assert.That(
                source,
                Does.Contain("projectileDriver.LastFiredProjectile"));
            Assert.That(source, Does.Not.Contain(".TryApplyDamage("));
            Assert.That(source, Does.Not.Contain(".QueueDodge("));
            Assert.That(source, Does.Not.Contain(".QueueSummonSlot1("));
            Assert.That(source, Does.Not.Contain(".TryUseSummonSlot1("));
            Assert.That(source, Does.Not.Contain("BindingFlags.NonPublic"));
            Assert.That(source, Does.Not.Contain("AddComponent<Canvas"));
            Assert.That(source, Does.Not.Contain("AddComponent<VisualEffect"));
            Assert.That(source, Does.Not.Contain("AddComponent<SummonLaneSpace"));
        }

        [Test]
        public void RuntimeProof_RequiresActualOrderedProductChain()
        {
            AuditionPvCityHitDodgeSummonRuntimeProof proof =
                CreateValidProof();

            Assert.DoesNotThrow(() =>
                AuditionPvCityHitDodgeSummonCapture
                    .ValidateRuntimeProof(proof));

            proof.usedActualPerfectDodgeSemantics = false;
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvCityHitDodgeSummonCapture
                    .ValidateRuntimeProof(proof));
            proof.usedActualPerfectDodgeSemantics = true;
            proof.summonSpentTier = 3;
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvCityHitDodgeSummonCapture
                    .ValidateRuntimeProof(proof));
            proof.summonSpentTier = 2;
            proof.summonDamageFrame = 540;
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvCityHitDodgeSummonCapture
                    .ValidateRuntimeProof(proof));
        }

        [Test]
        public void Baselines_AreBoundToActualRuntimeBeatFrames()
        {
            AuditionPvCityHitDodgeSummonRuntimeProof proof =
                CreateValidProof();

            AuditionPvBaselineManifestEntry[] baselines =
                AuditionPvCityHitDodgeSummonCapture
                    .CreateBaselineManifestEntries(proof);

            Assert.That(baselines, Has.Length.EqualTo(3));
            Assert.That(baselines[0].id, Is.EqualTo("s030-player-hit"));
            Assert.That(baselines[0].sourceFrame, Is.EqualTo(245));
            Assert.That(baselines[1].id, Is.EqualTo("s030-perfect-dodge"));
            Assert.That(baselines[1].sourceFrame, Is.EqualTo(401));
            Assert.That(baselines[2].id, Is.EqualTo("s030-summon-chain"));
            Assert.That(baselines[2].sourceFrame, Is.EqualTo(509));
            Assert.That(
                baselines.All(entry => entry.hudMode == "hud-on"
                    && entry.status == "captured"
                    && entry.sourceFrame >= 180
                    && entry.sourceFrame <= 539),
                Is.True);
        }

        internal static AuditionPvCityHitDodgeSummonRuntimeProof
            CreateValidProof()
        {
            return new AuditionPvCityHitDodgeSummonRuntimeProof
            {
                freshSceneValidated = true,
                directorCompleted = true,
                productBindingsExact = true,
                hudRenderableEverySelectedFrame = true,
                noLaneSpace = true,
                existingProductRootsOnly = true,
                usedNaturalHostileProjectile = true,
                usedHudDodgePath = true,
                usedActualPerfectDodgeSemantics = true,
                usedHudSummonSlot1Path = true,
                usedTierTwoChargeBruiser = true,
                usedActualAllySummonDamage = true,
                perfectDodgePreservedHealth = true,
                hostileHitReducedHealth = true,
                summonDamageReducedEnemyHealth = true,
                selectedBeatOrderExact = true,
                presentedFramesExact = true,
                selectedMappingExact = true,
                presentationClockExact = true,
                recorderPaddingActiveAtSourceFrameZero = true,
                recorderAutoStoppedAfterLastFrame = true,
                stateRestored = true,
                captureArtifactsReleased = true,
                presentationClockReleased = true,
                freshSceneReopened = true,
                deterministicRandomSeed =
                    AuditionPvCityHitDodgeSummonCapture
                        .DeterministicRandomSeed,
                lastSourceFrame = 719,
                presentedFrameCount = 720,
                preHandlePresentedFrameCount = 180,
                selectedPresentedFrameCount = 360,
                postHandlePresentedFrameCount = 180,
                recorderWarmupEndOfFrameCount = 2,
                hostileProjectileFiredCount = 2,
                hostileDamageCount = 1,
                hudDodgeRequestCount = 1,
                dodgeStartedCount = 1,
                perfectDodgeCount = 1,
                dodgeEndedCount = 1,
                hudSummonSlot1RequestCount = 1,
                summonSlot1UsedCount = 1,
                allySummonDamageCount = 1,
                firstHostileHitFrame = 244,
                dodgeRequestFrame = 390,
                dodgeStartedFrame = 390,
                perfectDodgeFrame = 400,
                dodgeEndedFrame = 434,
                summonRequestFrame = 435,
                summonUsedFrame = 435,
                summonDamageFrame = 508,
                summonSpentTier = 2,
                summonProjectileCount = 2,
                playerHealthAtStart = 100f,
                playerHealthAfterHostileHit = 86f,
                playerHealthAtDodgeRequest = 86f,
                playerHealthAtPerfectDodge = 86f,
                enemyHealthBeforeSummonDamage = 90f,
                enemyHealthAfterSummonDamage = 0f,
                summonEnergyBeforeRequest = 200f,
                summonEnergyAfterUse = 0f,
                events = Enumerable.Range(0, 8)
                    .Select(index =>
                        new AuditionPvCityHitDodgeSummonEvent
                        {
                            eventName = "event-" + index,
                            sourceFrame = 200 + index,
                            selectedLogicalFrame = 20 + index,
                            unityFrame = 1000 + index
                        })
                    .ToArray()
            };
        }

        private static string ProjectAbsolutePath(string assetPath)
        {
            string projectRoot = Directory.GetParent(
                    UnityEngine.Application.dataPath)?.FullName
                ?? throw new InvalidOperationException(
                    "Unity project root is missing.");
            return Path.GetFullPath(Path.Combine(projectRoot, assetPath));
        }
    }
}
