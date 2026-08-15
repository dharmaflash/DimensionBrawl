using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using DimensionBrawl.Combat;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.Presentation;

namespace DimensionBrawl.Editor.AuditionPV.Tests
{
    public sealed class AuditionPvStationPhase2PerfectDodgeCaptureTests
    {
        [Test]
        public void G05Contract_IsExactQhdSixtyFpsHudOnRange()
        {
            AuditionPvShotManifestEntry shot =
                AuditionPvStationPhase2PerfectDodgeCapture.CreateShotManifestEntry();

            Assert.That(shot.id, Is.EqualTo("g05"));
            Assert.That(shot.scenePath, Is.EqualTo(
                "Assets/_Game/Scenes/OlympusStationCombatStage.unity"));
            Assert.That(shot.startFrame, Is.EqualTo(0));
            Assert.That(shot.endFrame, Is.EqualTo(196));
            Assert.That(shot.expectedFrameCount, Is.EqualTo(197));
            Assert.That(shot.hudMode, Is.EqualTo("hud-on"));
            Assert.That(AuditionPvCaptureContract.Width, Is.EqualTo(2560));
            Assert.That(AuditionPvCaptureContract.Height, Is.EqualTo(1440));
            Assert.That(AuditionPvCaptureContract.Fps, Is.EqualTo(60));
            Assert.That(
                AuditionPvStationPhase2PerfectDodgeCapture
                    .ProductScreenDomainAlpha,
                Is.EqualTo(0.14f));
            Assert.That(
                AuditionPvStationPhase2PerfectDodgeCapture
                    .ProductScreenInvertAlpha,
                Is.EqualTo(0.015f));
            Assert.That(
                AuditionPvStationPhase2PerfectDodgeCapture
                    .ProductScreenEdgeAlpha,
                Is.EqualTo(0.18f));
            Assert.That(
                AuditionPvStationPhase2PerfectDodgeCapture
                    .ProductScreenGlitchAlpha,
                Is.EqualTo(0.03f));
            Assert.That(
                AuditionPvStationPhase2PerfectDodgeCapture
                    .ProductScreenDomainSeconds,
                Is.EqualTo(0.42f));
            Assert.That(
                shot.notes,
                Does.Contain("used without a capture-time visual override"));
        }

        [Test]
        public void GameplayBeatFrames_AreExactAndRangeChecked()
        {
            Assert.That(
                AuditionPvStationPhase2PerfectDodgeCapture.BeginWindupFrame,
                Is.EqualTo(1));
            Assert.That(
                AuditionPvStationPhase2PerfectDodgeCapture.FirePendingWaveFrame,
                Is.EqualTo(71));
            Assert.That(
                AuditionPvStationPhase2PerfectDodgeCapture.QueueDodgeFrame,
                Is.EqualTo(186));
            Assert.That(
                AuditionPvStationPhase2PerfectDodgeCapture.ImpactFrame,
                Is.EqualTo(188));
            Assert.That(
                AuditionPvStationPhase2PerfectDodgeCapture.PhaseTwoSettleFrames,
                Is.EqualTo(90));
            Assert.That(
                AuditionPvStationPhase2PerfectDodgeCapture.FrameTimeSeconds(188),
                Is.EqualTo(3.1333333f).Within(0.000001f));
            Assert.That(
                AuditionPvStationPhase2PerfectDodgeCapture.FrameFileName(0),
                Is.EqualTo("frame_0000.png"));
            Assert.That(
                AuditionPvStationPhase2PerfectDodgeCapture.FrameFileName(196),
                Is.EqualTo("frame_0196.png"));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                AuditionPvStationPhase2PerfectDodgeCapture.FrameFileName(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                AuditionPvStationPhase2PerfectDodgeCapture.FrameFileName(197));
        }

        [Test]
        public void Baselines_AreBl03AtZeroAndBl06AtFirstScreenDomainFrame()
        {
            AuditionPvBaselineManifestEntry[] baselines =
                AuditionPvStationPhase2PerfectDodgeCapture
                    .CreateBaselineManifestEntries();

            Assert.That(baselines, Has.Length.EqualTo(2));
            Assert.That(baselines[0].id, Is.EqualTo("bl03"));
            Assert.That(baselines[0].shotId, Is.EqualTo("g05"));
            Assert.That(baselines[0].sourceFrame, Is.EqualTo(0));
            Assert.That(baselines[0].fileName, Is.EqualTo(
                "BL03_AKAZA_PHASE2_CRUSHNET__HUDON__t00.000000.png"));
            Assert.That(baselines[0].hudMode, Is.EqualTo("hud-on"));
            Assert.That(baselines[0].status, Is.EqualTo("captured"));

            Assert.That(baselines[1].id, Is.EqualTo("bl06"));
            Assert.That(baselines[1].shotId, Is.EqualTo("g05"));
            Assert.That(baselines[1].sourceFrame, Is.EqualTo(189));
            Assert.That(baselines[1].fileName, Is.EqualTo(
                "BL06_AKAZA_PHASE2_PERFECT_DODGE__HUDON__t03.150000.png"));
            Assert.That(baselines[1].hudMode, Is.EqualTo("hud-on"));
            Assert.That(baselines[1].status, Is.EqualTo("captured"));
        }

        [Test]
        public void ExplicitDependencies_ResolveScenePatternAndCaptureRuntimeCode()
        {
            string[] dependencies =
                AuditionPvStationPhase2PerfectDodgeCapture
                    .ExplicitProductDependencyPaths();

            Assert.That(dependencies, Does.Contain(
                AuditionPvStationPhase2PerfectDodgeCapture.StationScenePath));
            Assert.That(dependencies, Does.Contain(
                AuditionPvStationPhase2PerfectDodgeCapture.CrushNetProfilePath));
            Assert.That(dependencies, Does.Contain(
                AuditionPvStationPhase2PerfectDodgeCapture
                    .PhaseTwoOpeningProfilePath));
            Assert.That(dependencies, Does.Contain(
                AuditionPvStationPhase2PerfectDodgeCapture.CaptureScriptPath));
            Assert.That(dependencies, Does.Contain(
                AuditionPvStationPhase2PerfectDodgeCapture.PresentationClockPath));
            Assert.That(dependencies, Does.Contain(
                AuditionPvStationPhase2PerfectDodgeCapture.PhaseTwoFlowPath));
            Assert.That(dependencies, Does.Contain(
                AuditionPvStationPhase2PerfectDodgeCapture.BarrageEmitterPath));
            Assert.That(dependencies, Does.Contain(
                AuditionPvStationPhase2PerfectDodgeCapture.BarrageProjectilePath));
            Assert.That(dependencies, Does.Contain(
                AuditionPvStationPhase2PerfectDodgeCapture.CadenceSchedulerPath));
            Assert.That(dependencies, Does.Contain(
                AuditionPvStationPhase2PerfectDodgeCapture.PlayerActionPath));
            Assert.That(dependencies, Does.Contain(
                AuditionPvStationPhase2PerfectDodgeCapture.ActionCameraPath));
            Assert.That(dependencies, Does.Contain(
                AuditionPvStationPhase2PerfectDodgeCapture.ActionScreenPath));
            Assert.That(dependencies, Does.Contain(
                AuditionPvStationPhase2PerfectDodgeCapture
                    .PerfectDodgeTimeWarpPath));

            foreach (string dependency in dependencies)
            {
                Assert.That(
                    AssetDatabase.LoadMainAssetAtPath(dependency),
                    Is.Not.Null,
                    dependency);
            }
        }

        [Test]
        public void OutputReservation_IsCreateNewAndUsesRecorder516G05FrameFolder()
        {
            string root = Path.Combine(
                Path.GetTempPath(),
                "DimensionBrawl_G05Output_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            try
            {
                using AuditionPvStationPhase2PerfectDodgeOutput first =
                    AuditionPvStationPhase2PerfectDodgeCapture
                        .ReserveNewOutputForRoot(root, "g05-contract-test");
                using AuditionPvStationPhase2PerfectDodgeOutput second =
                    AuditionPvStationPhase2PerfectDodgeCapture
                        .ReserveNewOutputForRoot(root, "g05-contract-test");

                Assert.That(first.captureId, Is.EqualTo("g05-contract-test"));
                Assert.That(second.captureId, Is.EqualTo("g05-contract-test_r002"));
                Assert.That(second.outputDirectory, Is.Not.EqualTo(first.outputDirectory));
                Assert.That(Directory.Exists(first.outputDirectory), Is.True);
                Assert.That(Directory.Exists(first.baselineDirectory), Is.True);
                Assert.That(
                    first.recorderSettings.imageSettings.OutputFile
                        .Replace('\\', '/'),
                    Does.Contain("/frames/g05/frame_"));
                Assert.DoesNotThrow(() =>
                    AuditionPvRecorderSettingsFactory.Validate(
                        first.recorderSettings));
            }
            finally
            {
                if (Directory.Exists(root))
                {
                    Directory.Delete(root, recursive: true);
                }
            }
        }

        [Test]
        public void CaptureImplementation_UsesOnlyPublicProductPathsAndNoTimeFreeze()
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName
                ?? throw new InvalidOperationException(
                    "Could not resolve project root.");
            string sourcePath = Path.Combine(
                projectRoot,
                AuditionPvStationPhase2PerfectDodgeCapture.CaptureScriptPath);
            string source = File.ReadAllText(sourcePath);

            Assert.That(source, Does.Contain("flow.EncounterController"));
            Assert.That(source, Does.Contain("flow.BarrageEmitter"));
            Assert.That(source, Does.Contain("flow.PressureActionDirector"));
            Assert.That(source, Does.Contain("flow.PressurePositionController"));
            Assert.That(source, Does.Contain("flow.CombatHudCanvasGroup"));
            Assert.That(source, Does.Contain("flow.PlayerActionController"));
            Assert.That(source, Does.Contain("flow.PlayerRangedBasicAttackAction"));
            Assert.That(source, Does.Contain("encounter.EnergyLadder"));
            Assert.That(source, Does.Contain("bossHealth.TryApplyDamage(damage)"));
            Assert.That(source, Does.Contain("flow.TrySkipTransition()"));
            Assert.That(source, Does.Contain("emitter.BeginWindup()"));
            Assert.That(source, Does.Contain("emitter.FirePendingWave()"));
            Assert.That(source, Does.Contain(
                "emitter.CopyActiveProjectiles(activeProjectiles)"));
            Assert.That(source, Does.Contain("playerAction.QueueDodge()"));
            Assert.That(source, Does.Contain("impactProjectile.TryApplyImpact("));
            Assert.That(source, Does.Contain("combatHud.alpha = 1f;"));
            Assert.That(source, Does.Contain("PerfectDodgeScreenDomainRuntime.Clear()"));
            Assert.That(source, Does.Contain(
                "PresentationClock.AcquireManual("));
            Assert.That(source, Does.Contain(
                "BossCombatCadenceScheduler.AcquireExternalSuspension(this)"));
            Assert.That(source, Does.Contain(
                "pressurePositionController.TryAcquireMovementIntentOverride("));
            Assert.That(source, Does.Contain(
                "BossPressureMovementIntent.CommitForward"));
            Assert.That(source, Does.Contain(
                "BossPressureActionKind.PunishOverextend"));
            Assert.That(source, Does.Contain(
                "EditorSceneManager.OpenScene(StationScenePath"));
            Assert.That(source, Does.Not.Contain("PerfectDodgeTriggered?.Invoke"));
            Assert.That(source, Does.Not.Contain("System.Reflection"));
            Assert.That(source, Does.Not.Contain("GetComponentsInChildren<CanvasGroup"));
            Assert.That(source, Does.Not.Contain("Time.timeScale = 0"));
            Assert.That(source, Does.Not.Contain("EditorSceneManager.Save"));
            Assert.That(source, Does.Not.Contain(
                "ConfigurePerfectDodgeDomainPresentation("));
            Assert.That(source, Does.Not.Contain("CaptureOnlyScreen"));
        }

        [Test]
        public void EncounterEnergyLadder_IsPublicReadOnlyCaptureBinding()
        {
            System.Reflection.PropertyInfo property = typeof(
                    BossBarrageEncounterController)
                .GetProperty(nameof(BossBarrageEncounterController.EnergyLadder));

            Assert.That(property, Is.Not.Null);
            Assert.That(property.GetMethod, Is.Not.Null);
            Assert.That(property.GetMethod.IsPublic, Is.True);
            Assert.That(property.SetMethod, Is.Null);
        }

        [Test]
        public void RecorderShotEntry_IsPublicAndScreenProfileClampsAndRestores()
        {
            System.Reflection.MethodInfo recorderEntry = typeof(
                    AuditionPvStationPhase2PerfectDodgeDirector)
                .GetMethod(nameof(
                    AuditionPvStationPhase2PerfectDodgeDirector
                        .BeginShotForRecorder));
            Assert.That(recorderEntry, Is.Not.Null);
            Assert.That(recorderEntry.IsPublic, Is.True);

            var root = new GameObject("[G05_ScreenProfileContract]");
            root.SetActive(false);
            try
            {
                ActionScreenCuePresenter presenter =
                    root.AddComponent<ActionScreenCuePresenter>();
                presenter.ConfigurePerfectDodgeDomainPresentation(
                    true,
                    2f,
                    -1f,
                    2f,
                    2f);

                Assert.That(presenter.PlayPerfectDodgeScreenDomain, Is.True);
                Assert.That(presenter.MaxPerfectDodgeDomainAlpha, Is.EqualTo(0.65f));
                Assert.That(presenter.MaxPerfectDodgeInvertAlpha, Is.Zero);
                Assert.That(presenter.MaxPerfectDodgeEdgeAlpha, Is.EqualTo(0.75f));
                Assert.That(presenter.PerfectDodgeGlitchOverlayAlpha, Is.EqualTo(1f));

                presenter.ConfigurePerfectDodgeDomainPresentation(
                    false,
                    0f,
                    0f,
                    0f,
                    0f);
                Assert.That(presenter.PlayPerfectDodgeScreenDomain, Is.False);
                Assert.That(PresentationClock.IsManuallyDriven, Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [UnityTest]
        [Timeout(30000)]
        public IEnumerator CadenceExternalSuspension_IsOwnerScopedPrunesDestroyedOwnerAndStopsTicks()
        {
            BossBarragePatternProfile profile =
                AssetDatabase.LoadAssetAtPath<BossBarragePatternProfile>(
                    AuditionPvStationPhase2PerfectDodgeCapture
                        .CrushNetProfilePath);
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            yield return new EnterPlayMode();

            var firstOwner = new GameObject("[G05_CadenceLeaseOwner_A]");
            var secondOwner = new GameObject("[G05_CadenceLeaseOwner_B]");
            var staleOwner = new GameObject("[G05_CadenceLeaseOwner_Stale]");
            var laneRoot = new GameObject("[G05_CadenceLane]");
            var trackedRoot = new GameObject("[G05_CadenceTarget]");
            var emitterRoot = new GameObject("[G05_CadenceEmitter]");
            emitterRoot.SetActive(false);

            IDisposable first = null;
            IDisposable second = null;
            IDisposable stale = null;
            bool beganWindup = false;
            bool heldWindupWhileSuspended = false;
            bool resumedRealEmitterTick = false;
            bool ownerCountWasStable = false;
            bool destroyedOwnerWasPruned = false;
            bool nullOwnerRejected = false;
            bool finalCountWasZero = false;
            int destroyedOwnerCountAfterPrune = -1;
            int waveCount = 0;
            try
            {
                try
                {
                    BossCombatCadenceScheduler.AcquireExternalSuspension(null);
                }
                catch (ArgumentNullException)
                {
                    nullOwnerRejected = true;
                }

                first = BossCombatCadenceScheduler.AcquireExternalSuspension(
                    firstOwner);
                second = BossCombatCadenceScheduler.AcquireExternalSuspension(
                    secondOwner);
                ownerCountWasStable =
                    BossCombatCadenceScheduler.IsExternallySuspended
                    && BossCombatCadenceScheduler.ExternalSuspensionCount == 2;
                second.Dispose();
                second.Dispose();
                second = null;

                SummonLaneSpace lane = laneRoot.AddComponent<SummonLaneSpace>();
                BossBarrageEmitter emitter =
                    emitterRoot.AddComponent<BossBarrageEmitter>();
                emitter.ConfigureReferences(lane, trackedRoot.transform, null);
                emitter.ConfigurePattern(profile, null, 0);
                emitterRoot.SetActive(true);
                emitter.WaveFired += (_, _, _) => waveCount++;
                beganWindup = emitter.BeginWindup();

                yield return new WaitForSecondsRealtime(
                    profile.WindupSeconds + 0.25f);
                heldWindupWhileSuspended =
                    emitter.IsWindupActive && waveCount == 0;

                first.Dispose();
                first = null;
                double resumeDeadline = Time.realtimeSinceStartupAsDouble + 2d;
                while (waveCount == 0
                    && Time.realtimeSinceStartupAsDouble < resumeDeadline)
                {
                    yield return new WaitForSecondsRealtime(0.02f);
                }

                resumedRealEmitterTick =
                    waveCount == 1 && !emitter.IsWindupActive;

                stale = BossCombatCadenceScheduler.AcquireExternalSuspension(
                    staleOwner);
                UnityEngine.Object.Destroy(staleOwner);
                double pruneDeadline = Time.realtimeSinceStartupAsDouble + 2d;
                while (staleOwner != null
                    && Time.realtimeSinceStartupAsDouble < pruneDeadline)
                {
                    yield return new WaitForSecondsRealtime(0.02f);
                }

                destroyedOwnerCountAfterPrune =
                    BossCombatCadenceScheduler.ExternalSuspensionCount;
                destroyedOwnerWasPruned =
                    staleOwner == null
                    && destroyedOwnerCountAfterPrune == 0
                    && !BossCombatCadenceScheduler.IsExternallySuspended;
                stale.Dispose();
                stale = null;
                finalCountWasZero =
                    BossCombatCadenceScheduler.ExternalSuspensionCount == 0;
            }
            finally
            {
                first?.Dispose();
                second?.Dispose();
                stale?.Dispose();
                UnityEngine.Object.Destroy(firstOwner);
                UnityEngine.Object.Destroy(secondOwner);
                if (staleOwner != null)
                {
                    UnityEngine.Object.Destroy(staleOwner);
                }

                UnityEngine.Object.Destroy(emitterRoot);
                UnityEngine.Object.Destroy(trackedRoot);
                UnityEngine.Object.Destroy(laneRoot);
            }

            yield return null;
            yield return new ExitPlayMode();
            AuditionPvStationPhase2PerfectDodgeCapture
                .ReopenProductSceneAfterPlayMode();

            Assert.That(profile, Is.Not.Null, "CrushNet profile binding.");
            Assert.That(nullOwnerRejected, Is.True, "Null lease owner rejection.");
            Assert.That(ownerCountWasStable, Is.True, "Two owner-scoped leases.");
            Assert.That(beganWindup, Is.True, "Real emitter windup start.");
            Assert.That(
                heldWindupWhileSuspended,
                Is.True,
                $"Suspended tick leaked: waves={waveCount}.");
            Assert.That(
                resumedRealEmitterTick,
                Is.True,
                $"Emitter did not resume exactly once: waves={waveCount}.");
            Assert.That(
                destroyedOwnerWasPruned,
                Is.True,
                $"Destroyed lease owner was not pruned: "
                    + $"ownerNull={staleOwner == null}, "
                    + $"count={destroyedOwnerCountAfterPrune}, "
                    + $"suspended={BossCombatCadenceScheduler.IsExternallySuspended}.");
            Assert.That(
                finalCountWasZero,
                Is.True,
                $"Final external suspension count was "
                    + $"{BossCombatCadenceScheduler.ExternalSuspensionCount}.");
        }

        [UnityTest]
        [Timeout(120000)]
        public IEnumerator FreshStationMidWindupDisable_RestoresEmitterHudScreenClockAndSchedulers()
        {
            EditorSceneManager.OpenScene(
                AuditionPvStationPhase2PerfectDodgeCapture.StationScenePath,
                OpenSceneMode.Single);
            yield return new EnterPlayMode();
            // Cross one actual player frame for scene lifecycle stabilization;
            // the bound energy risk baseline is sampled explicitly below.
            yield return new WaitForSecondsRealtime(0.02f);

            Exception capturedFailure = null;
            AuditionPvStationPhase2PerfectDodgeDirector director = null;
            OlympusStationAkazaPhase2FlowController flow = null;
            ActionScreenCuePresenter screen = null;
            BossBarragePatternProfile crushNet =
                AssetDatabase.LoadAssetAtPath<BossBarragePatternProfile>(
                    AuditionPvStationPhase2PerfectDodgeCapture
                        .CrushNetProfilePath);
            BossBarragePatternProfile authoredOpening =
                AssetDatabase.LoadAssetAtPath<BossBarragePatternProfile>(
                    AuditionPvStationPhase2PerfectDodgeCapture
                        .PhaseTwoOpeningProfilePath);
            int savedCaptureFramerate = Time.captureFramerate;
            int savedTargetFrameRate = Application.targetFrameRate;
            float savedHudAlpha = 0f;
            bool savedHudInteractable = false;
            bool savedHudBlocksRaycasts = false;
            bool savedEncounterSuspended = false;
            bool savedMoveInputLocked = false;
            bool savedActionInputLocked = false;
            bool savedRangedInputLocked = false;
            UnityEngine.Random.State savedRandomState = UnityEngine.Random.state;
            float savedEnergyMana = 0f;
            float savedEnergyRate = 0f;
            bool sceneScreenDefaultsWereExact = false;
            bool reachedWindup = false;
            bool productScreenProfileWasActive = false;
            bool stateRestored = false;
            bool bossCompositionRestored = false;
            bool emitterClearedCaptureWindup = false;
            bool authoredPriorityRestored = false;
            string authoredPriorityRestoreDiagnostics = "not observed";
            bool hudRestored = false;
            bool screenRestored = false;
            bool encounterRestored = false;
            bool energyRestored = false;
            string energyRestoreDiagnostics = "not observed";
            bool clockRestored = false;
            bool cadenceRestored = false;
            bool framePacingRestored = false;
            bool inputRestored = false;
            bool randomRestored = false;

            flow = UnityEngine.Object.FindFirstObjectByType<
                    OlympusStationAkazaPhase2FlowController>(
                    FindObjectsInactive.Include);
                ActionScreenCuePresenter[] screens =
                    UnityEngine.Object.FindObjectsByType<ActionScreenCuePresenter>(
                            FindObjectsInactive.Include,
                            FindObjectsSortMode.None)
                        .Where(candidate =>
                            candidate != null
                            && candidate.gameObject.scene
                                == SceneManager.GetActiveScene())
                        .ToArray();
                if (flow == null || screens.Length != 1)
                {
                    throw new InvalidOperationException(
                        $"G05 restore test expected one Flow and screen presenter; "
                        + $"flow={(flow != null)}, screens={screens.Length}.");
                }

                screen = screens[0];
                savedHudAlpha = flow.CombatHudCanvasGroup.alpha;
                savedHudInteractable = flow.CombatHudCanvasGroup.interactable;
                savedHudBlocksRaycasts =
                    flow.CombatHudCanvasGroup.blocksRaycasts;
                savedEncounterSuspended =
                    flow.EncounterController.IsExternalCombatSuspended;
                savedMoveInputLocked =
                    flow.PlayerMovement.IsCinematicMoveInputLocked;
                savedActionInputLocked =
                    flow.PlayerActionController.IsCinematicInputLocked;
                savedRangedInputLocked =
                    flow.PlayerRangedBasicAttackAction.IsCinematicInputLocked;
                savedRandomState = UnityEngine.Random.state;
                SummonEnergyLadder baselineEnergy =
                    flow.EncounterController.EnergyLadder;
                if (baselineEnergy.CurrentEnergyPerSecond <= 0f
                    || baselineEnergy.IsCapped)
                {
                    throw new InvalidOperationException(
                        "G05 restore test requires enabled, uncapped fresh energy gain.");
                }

                baselineEnergy.SetGainEnabled(false);
                try
                {
                    baselineEnergy.Tick(1f / AuditionPvCaptureContract.Fps);
                }
                finally
                {
                    baselineEnergy.SetGainEnabled(true);
                }

                savedEnergyMana = flow.EncounterController.EnergyLadder.CurrentMana;
                savedEnergyRate =
                    flow.EncounterController.EnergyLadder.CurrentEnergyPerSecond;
                sceneScreenDefaultsWereExact = HasExactProductScreenProfile(screen);

                director = AuditionPvStationPhase2PerfectDodgeCapture
                    .AttachToFreshActiveScene();
                IEnumerator preparation = director.PrepareFreshProductState();
                while (capturedFailure == null)
                {
                    bool moved = TryMoveNext(
                        preparation,
                        out object yielded,
                        out Exception iterationFailure);
                    if (iterationFailure != null)
                    {
                        capturedFailure = iterationFailure;
                        break;
                    }

                    if (!moved)
                    {
                        break;
                    }

                    yield return yielded;
                }

                if (capturedFailure == null)
                {
                    productScreenProfileWasActive =
                        director.ProductScreenProfileActive;
                    director.FramePresented += frame =>
                        reachedWindup |= frame
                            >= AuditionPvStationPhase2PerfectDodgeCapture
                                .BeginWindupFrame;
                    CaptureException(director.BeginShot, ref capturedFailure);
                    double deadline = Time.realtimeSinceStartupAsDouble + 3d;
                    while (capturedFailure == null
                        && !reachedWindup
                        && director.Failure == null
                        && Time.realtimeSinceStartupAsDouble < deadline)
                    {
                        yield return new WaitForSecondsRealtime(0.02f);
                    }

                    capturedFailure ??= director.Failure;
                }

                if (capturedFailure == null && director != null)
                {
                    reachedWindup &= flow.BarrageEmitter.IsWindupActive;
                    director.enabled = false;
                    stateRestored = director.StateRestored;
                    bossCompositionRestored = director.BossCompositionRestored;
                    emitterClearedCaptureWindup =
                        !flow.BarrageEmitter.IsWindupActive;
                    authoredPriorityRestored =
                        flow.BarrageEmitter.HasQueuedPriorityPattern
                        && flow.BarrageEmitter.QueuedPriorityPattern
                            == authoredOpening
                        && flow.BarrageEmitter.QueuedPriorityWavesRemaining == 1;
                    authoredPriorityRestoreDiagnostics =
                        $"has={flow.BarrageEmitter.HasQueuedPriorityPattern}, "
                        + $"queued={(flow.BarrageEmitter.QueuedPriorityPattern != null ? flow.BarrageEmitter.QueuedPriorityPattern.name : "none")}, "
                        + $"waves={flow.BarrageEmitter.QueuedPriorityWavesRemaining}";
                    hudRestored =
                        flow.CombatHudCanvasGroup.alpha == savedHudAlpha
                        && flow.CombatHudCanvasGroup.interactable
                            == savedHudInteractable
                        && flow.CombatHudCanvasGroup.blocksRaycasts
                            == savedHudBlocksRaycasts;
                    screenRestored = director.ScreenProfileRestored
                        && HasExactProductScreenProfile(screen)
                        && !PerfectDodgeScreenDomainRuntime.HasActiveCue;
                    encounterRestored =
                        flow.EncounterController.IsExternalCombatSuspended
                            == savedEncounterSuspended;
                    energyRestored = Mathf.Abs(
                            flow.EncounterController.EnergyLadder.CurrentMana
                                - savedEnergyMana) <= 0.001f
                        && Mathf.Abs(
                            flow.EncounterController.EnergyLadder
                                .CurrentEnergyPerSecond - savedEnergyRate) <= 0.001f;
                    energyRestoreDiagnostics =
                        $"mana={savedEnergyMana:F6}->"
                        + $"{flow.EncounterController.EnergyLadder.CurrentMana:F6}, "
                        + $"rate={savedEnergyRate:F6}->"
                        + $"{flow.EncounterController.EnergyLadder.CurrentEnergyPerSecond:F6}";
                    clockRestored = !PresentationClock.IsManuallyDriven;
                    cadenceRestored =
                        BossCombatCadenceScheduler.ExternalSuspensionCount == 0
                        && !BossCombatCadenceScheduler.IsExternallySuspended;
                    framePacingRestored =
                        Time.captureFramerate == savedCaptureFramerate
                        && Application.targetFrameRate == savedTargetFrameRate;
                    inputRestored =
                        flow.PlayerMovement.IsCinematicMoveInputLocked
                            == savedMoveInputLocked
                        && flow.PlayerActionController.IsCinematicInputLocked
                            == savedActionInputLocked
                        && flow.PlayerRangedBasicAttackAction
                                .IsCinematicInputLocked
                            == savedRangedInputLocked;
                    randomRestored = UnityEngine.Random.state.Equals(
                        savedRandomState);
            }

            if (director != null)
            {
                try
                {
                    director.RestoreShotState();
                }
                catch (Exception restoreException)
                {
                    capturedFailure = capturedFailure == null
                        ? restoreException
                        : new AggregateException(
                            capturedFailure,
                            restoreException);
                }

                UnityEngine.Object.Destroy(director.gameObject);
                yield return null;
            }

            yield return new ExitPlayMode();
            AuditionPvStationPhase2PerfectDodgeCapture
                .ReopenProductSceneAfterPlayMode();

            Assert.That(capturedFailure, Is.Null, "Preparation/restore exception.");
            Assert.That(crushNet, Is.Not.Null, "CrushNet profile binding.");
            Assert.That(authoredOpening, Is.Not.Null, "Authored opening binding.");
            Assert.That(sceneScreenDefaultsWereExact, Is.True, "Scene screen defaults.");
            Assert.That(productScreenProfileWasActive, Is.True, "Product screen profile.");
            Assert.That(reachedWindup, Is.True, "f1 capture windup.");
            Assert.That(stateRestored, Is.True, "Director restore seal.");
            Assert.That(bossCompositionRestored, Is.True, "Boss composition restore.");
            Assert.That(emitterClearedCaptureWindup, Is.True, "Capture windup cleanup.");
            Assert.That(
                authoredPriorityRestored,
                Is.True,
                "Authored opening restore: "
                    + authoredPriorityRestoreDiagnostics
                    + ".");
            Assert.That(hudRestored, Is.True, "HUD CanvasGroup restore.");
            Assert.That(screenRestored, Is.True, "Screen-domain profile/runtime restore.");
            Assert.That(encounterRestored, Is.True, "Encounter suspension restore.");
            Assert.That(
                energyRestored,
                Is.True,
                "Summon energy restore: " + energyRestoreDiagnostics + ".");
            Assert.That(clockRestored, Is.True, "PresentationClock restore.");
            Assert.That(cadenceRestored, Is.True, "Cadence lease restore.");
            Assert.That(framePacingRestored, Is.True, "Frame pacing restore.");
            Assert.That(inputRestored, Is.True, "Input lock restore.");
            Assert.That(randomRestored, Is.True, "Unity Random state restore.");
            Assert.That(
                SceneManager.GetActiveScene().path,
                Is.EqualTo(
                    AuditionPvStationPhase2PerfectDodgeCapture.StationScenePath));
        }

        [UnityTest]
        [Timeout(120000)]
        public IEnumerator FreshStationPreparation_RepeatsExactCameraPoseAcrossTwoTakes()
        {
            var positions = new Vector3[2];
            var rotations = new Quaternion[2];
            var fieldsOfView = new float[2];

            for (int take = 0; take < 2; take++)
            {
                EditorSceneManager.OpenScene(
                    AuditionPvStationPhase2PerfectDodgeCapture.StationScenePath,
                    OpenSceneMode.Single);
                yield return new EnterPlayMode();
                yield return null;

                Exception capturedFailure = null;
                AuditionPvStationPhase2PerfectDodgeDirector director =
                    CaptureException(
                        AuditionPvStationPhase2PerfectDodgeCapture
                            .AttachToFreshActiveScene,
                        ref capturedFailure);
                IEnumerator preparation = director != null
                    ? CaptureException(
                        director.PrepareFreshProductState,
                        ref capturedFailure)
                    : null;
                while (capturedFailure == null && preparation != null)
                {
                    bool moved = TryMoveNext(
                        preparation,
                        out object yielded,
                        out Exception iterationFailure);
                    if (iterationFailure != null)
                    {
                        capturedFailure = iterationFailure;
                        break;
                    }

                    if (!moved)
                    {
                        break;
                    }

                    yield return yielded;
                }

                if (capturedFailure == null && director != null)
                {
                    positions[take] = director.PreparedCameraPosition;
                    rotations[take] = director.PreparedCameraRotation;
                    fieldsOfView[take] = director.PreparedCameraFieldOfView;
                }

                if (director != null)
                {
                    try
                    {
                        director.RestoreShotState();
                    }
                    catch (Exception restoreException)
                    {
                        capturedFailure = capturedFailure == null
                            ? restoreException
                            : new AggregateException(
                                capturedFailure,
                                restoreException);
                    }

                    UnityEngine.Object.Destroy(director.gameObject);
                    yield return null;
                }

                yield return new ExitPlayMode();
                AuditionPvStationPhase2PerfectDodgeCapture
                    .ReopenProductSceneAfterPlayMode();
                Assert.That(capturedFailure, Is.Null, $"fresh take {take + 1}");
            }

            Assert.That(
                Vector3.Distance(positions[0], positions[1]),
                Is.LessThanOrEqualTo(0.001f),
                $"BL03 prepared camera position drifted between fresh takes: "
                    + $"take1={positions[0]}, take2={positions[1]}.");
            Assert.That(
                Quaternion.Angle(rotations[0], rotations[1]),
                Is.LessThanOrEqualTo(0.01f),
                "BL03 prepared camera rotation drifted between fresh takes.");
            Assert.That(
                Mathf.Abs(fieldsOfView[0] - fieldsOfView[1]),
                Is.LessThanOrEqualTo(0.001f),
                "BL03 prepared camera FOV drifted between fresh takes.");
            Assert.That(
                SceneManager.GetActiveScene().path,
                Is.EqualTo(
                    AuditionPvStationPhase2PerfectDodgeCapture.StationScenePath));
        }

        [UnityTest]
        [Timeout(120000)]
        public IEnumerator FreshStationProductState_FiresRealCrushNetAndPerfectDodgesOnce()
        {
            EditorSceneManager.OpenScene(
                AuditionPvStationPhase2PerfectDodgeCapture.StationScenePath,
                OpenSceneMode.Single);
            yield return new EnterPlayMode();
            yield return null;

            AuditionPvStationPhase2PerfectDodgeDirector director = null;
            Exception capturedFailure = null;
            bool complete = false;
            int lastFrame = -1;
            int perfectDodgeCount = 0;
            int projectileCount = 0;
            bool usedActualPattern = false;
            bool impactResolved = false;
            bool projectileInactive = false;
            bool healthUnchanged = false;
            bool cameraCueRequested = false;
            bool screenCueRequested = false;
            bool screenCueActiveAtBaselineFrame = false;
            bool exactHudRenderable = false;
            bool exactHudResources = false;
            bool stateRestored = false;
            bool bossCompositionRestored = false;
            bool screenProfileRestored = false;
            bool cadenceRestored = false;
            bool clockRestored = false;
            bool framePacingRestored = false;
            bool inputRestored = false;
            bool randomRestored = false;
            bool sceneScreenDefaultsWereExact = false;
            bool productScreenProfileWasActive = false;
            bool preparationSafetyExpired = false;
            bool exactEnergyBinding = false;
            int blockedDamageCount = 0;
            int modifyingDamageCount = 0;
            int hudAmmo = -1;
            int hudMagazineSize = -1;
            float hudEnergyMana = -1f;
            float hudEnergyMaxMana = -1f;
            int observedFrameCount = 0;
            int nextObservedFrame = 0;
            bool observedExactFrameSequence = true;
            float preparedCameraFieldOfView = 0f;
            Vector3 preparedCameraPosition = Vector3.zero;
            float bossRiskAtFirstFrame = 0f;
            float bossRiskAtFireFrame = 0f;
            float bossRiskAtImpactFrame = 0f;
            int savedCaptureFramerate = Time.captureFramerate;
            int savedTargetFrameRate = Application.targetFrameRate;
            bool savedMoveInputLocked = false;
            bool savedActionInputLocked = false;
            bool savedRangedInputLocked = false;
            UnityEngine.Random.State savedRandomState = UnityEngine.Random.state;
            ActionScreenCuePresenter screen =
                UnityEngine.Object.FindObjectsByType<ActionScreenCuePresenter>(
                        FindObjectsInactive.Include,
                        FindObjectsSortMode.None)
                    .Single(candidate =>
                        candidate.gameObject.scene
                            == SceneManager.GetActiveScene());
            sceneScreenDefaultsWereExact = HasExactProductScreenProfile(screen);

            OlympusStationAkazaPhase2FlowController initialFlow =
                UnityEngine.Object.FindFirstObjectByType<
                    OlympusStationAkazaPhase2FlowController>(
                    FindObjectsInactive.Include);
            if (initialFlow == null)
            {
                capturedFailure = new InvalidOperationException(
                    "G05 product test could not resolve its exact initial Flow.");
            }
            else
            {
                savedMoveInputLocked =
                    initialFlow.PlayerMovement.IsCinematicMoveInputLocked;
                savedActionInputLocked =
                    initialFlow.PlayerActionController.IsCinematicInputLocked;
                savedRangedInputLocked = initialFlow.PlayerRangedBasicAttackAction
                    .IsCinematicInputLocked;
                savedRandomState = UnityEngine.Random.state;
            }

            director = CaptureException(
                AuditionPvStationPhase2PerfectDodgeCapture.AttachToFreshActiveScene,
                ref capturedFailure);
            IEnumerator preparation = director != null
                ? CaptureException(
                    director.PrepareFreshProductState,
                    ref capturedFailure)
                : null;
            while (capturedFailure == null && preparation != null)
            {
                bool moved = TryMoveNext(
                    preparation,
                    out object yielded,
                    out Exception iterationFailure);
                if (iterationFailure != null)
                {
                    capturedFailure = iterationFailure;
                    break;
                }

                if (!moved)
                {
                    break;
                }

                yield return yielded;
            }

            if (capturedFailure == null && director != null)
            {
                director.FramePresented += frameIndex =>
                {
                    observedExactFrameSequence &= frameIndex == nextObservedFrame;
                    nextObservedFrame++;
                    observedFrameCount++;
                };
                CaptureException(director.BeginShot, ref capturedFailure);
                double deadline = Time.realtimeSinceStartupAsDouble + 10d;
                while (capturedFailure == null
                    && !director.IsComplete
                    && director.Failure == null
                    && Time.realtimeSinceStartupAsDouble < deadline)
                {
                    yield return new WaitForSecondsRealtime(1f / 60f);
                }

                capturedFailure = capturedFailure ?? director.Failure;
                complete = director.IsComplete;
                lastFrame = director.CurrentFrame;
                perfectDodgeCount = director.PerfectDodgeCount;
                projectileCount = director.FiredProjectileCount;
                usedActualPattern = director.UsedActualCrushNetPattern;
                impactResolved = director.ImpactAppliedOrBlocked;
                projectileInactive = director.ImpactProjectileInactive;
                healthUnchanged = director.PlayerHealthUnchanged;
                cameraCueRequested = director.CameraCueRequested;
                screenCueRequested = director.ScreenCueRequested;
                screenCueActiveAtBaselineFrame =
                    director.ScreenCueActiveAtBaselineFrame;
                preparedCameraFieldOfView = director.PreparedCameraFieldOfView;
                preparedCameraPosition = director.PreparedCameraPosition;
                exactHudRenderable = director.IsExactHudRenderable;
                exactHudResources = director.IsHudResourceStateExact;
                productScreenProfileWasActive =
                    director.ProductScreenProfileActive;
                preparationSafetyExpired =
                    director.PreparationSafetyExpiredBeforeDodge;
                exactEnergyBinding = director.UsesExactEnergyLadderBinding;
                blockedDamageCount = director.DamageBlockedObservationCount;
                modifyingDamageCount = director.DamageModifyingObservationCount;
                hudAmmo = director.HudAmmo;
                hudMagazineSize = director.HudMagazineSize;
                hudEnergyMana = director.HudEnergyMana;
                hudEnergyMaxMana = director.HudEnergyMaxMana;
                bossRiskAtFirstFrame = director.BossRiskAtFirstFrame;
                bossRiskAtFireFrame = director.BossRiskAtFireFrame;
                bossRiskAtImpactFrame = director.BossRiskAtImpactFrame;
            }

            if (director != null)
            {
                try
                {
                    director.RestoreShotState();
                }
                catch (Exception restoreException)
                {
                    capturedFailure = capturedFailure == null
                        ? restoreException
                        : new AggregateException(
                            capturedFailure,
                            restoreException);
                }

                stateRestored = director.StateRestored;
                bossCompositionRestored = director.BossCompositionRestored;
                screenProfileRestored = director.ScreenProfileRestored
                    && HasExactProductScreenProfile(screen)
                    && !PerfectDodgeScreenDomainRuntime.HasActiveCue;
                cadenceRestored =
                    BossCombatCadenceScheduler.ExternalSuspensionCount == 0
                    && !BossCombatCadenceScheduler.IsExternallySuspended;
                clockRestored = !PresentationClock.IsManuallyDriven;
                framePacingRestored =
                    Time.captureFramerate == savedCaptureFramerate
                    && Application.targetFrameRate == savedTargetFrameRate;
                inputRestored = initialFlow != null
                    && initialFlow.PlayerMovement.IsCinematicMoveInputLocked
                        == savedMoveInputLocked
                    && initialFlow.PlayerActionController.IsCinematicInputLocked
                        == savedActionInputLocked
                    && initialFlow.PlayerRangedBasicAttackAction
                            .IsCinematicInputLocked
                        == savedRangedInputLocked;
                randomRestored = UnityEngine.Random.state.Equals(
                    savedRandomState);
                UnityEngine.Object.Destroy(director.gameObject);
                yield return null;
            }

            yield return new ExitPlayMode();
            AuditionPvStationPhase2PerfectDodgeCapture
                .ReopenProductSceneAfterPlayMode();

            Assert.That(capturedFailure, Is.Null);
            Assert.That(complete, Is.True);
            Assert.That(lastFrame, Is.EqualTo(196));
            Assert.That(perfectDodgeCount, Is.EqualTo(1));
            Assert.That(projectileCount, Is.EqualTo(7));
            Assert.That(usedActualPattern, Is.True);
            Assert.That(impactResolved, Is.True);
            Assert.That(projectileInactive, Is.True);
            Assert.That(healthUnchanged, Is.True);
            Assert.That(cameraCueRequested, Is.True);
            Assert.That(screenCueRequested, Is.True);
            Assert.That(screenCueActiveAtBaselineFrame, Is.True);
            Assert.That(exactHudRenderable, Is.True);
            Assert.That(exactHudResources, Is.True);
            Assert.That(stateRestored, Is.True);
            Assert.That(bossCompositionRestored, Is.True);
            Assert.That(sceneScreenDefaultsWereExact, Is.True);
            Assert.That(productScreenProfileWasActive, Is.True);
            Assert.That(screenProfileRestored, Is.True);
            Assert.That(cadenceRestored, Is.True);
            Assert.That(clockRestored, Is.True);
            Assert.That(framePacingRestored, Is.True);
            Assert.That(inputRestored, Is.True);
            Assert.That(randomRestored, Is.True);
            Assert.That(preparationSafetyExpired, Is.True);
            Assert.That(exactEnergyBinding, Is.True);
            Assert.That(blockedDamageCount, Is.EqualTo(1));
            Assert.That(modifyingDamageCount, Is.Zero);
            Assert.That(hudAmmo, Is.EqualTo(hudMagazineSize));
            Assert.That(hudEnergyMana, Is.EqualTo(hudEnergyMaxMana).Within(0.001f));
            Assert.That(observedExactFrameSequence, Is.True);
            Assert.That(observedFrameCount, Is.EqualTo(197));
            Assert.That(preparedCameraFieldOfView, Is.GreaterThan(0f));
            Assert.That(float.IsNaN(preparedCameraPosition.x), Is.False);
            Assert.That(bossRiskAtFirstFrame, Is.GreaterThanOrEqualTo(0.58f));
            Assert.That(bossRiskAtFireFrame, Is.GreaterThanOrEqualTo(0.86f));
            Assert.That(bossRiskAtImpactFrame, Is.GreaterThanOrEqualTo(0.88f));
            Assert.That(
                SceneManager.GetActiveScene().path,
                Is.EqualTo(
                    AuditionPvStationPhase2PerfectDodgeCapture.StationScenePath));
        }

        private static bool HasExactProductScreenProfile(
            ActionScreenCuePresenter screen)
        {
            return screen != null
                && screen.PlayPerfectDodgeScreenDomain
                && Mathf.Abs(
                    screen.MaxPerfectDodgeDomainAlpha
                        - AuditionPvStationPhase2PerfectDodgeCapture
                            .ProductScreenDomainAlpha) <= 0.0001f
                && Mathf.Abs(
                    screen.MaxPerfectDodgeInvertAlpha
                        - AuditionPvStationPhase2PerfectDodgeCapture
                            .ProductScreenInvertAlpha) <= 0.0001f
                && Mathf.Abs(
                    screen.MaxPerfectDodgeEdgeAlpha
                        - AuditionPvStationPhase2PerfectDodgeCapture
                            .ProductScreenEdgeAlpha) <= 0.0001f
                && Mathf.Abs(
                    screen.PerfectDodgeGlitchOverlayAlpha
                        - AuditionPvStationPhase2PerfectDodgeCapture
                            .ProductScreenGlitchAlpha) <= 0.0001f
                && Mathf.Abs(
                    screen.PerfectDodgeDomainSeconds
                        - AuditionPvStationPhase2PerfectDodgeCapture
                            .ProductScreenDomainSeconds) <= 0.0001f;
        }

        private static T CaptureException<T>(
            Func<T> action,
            ref Exception failure)
            where T : class
        {
            if (failure != null)
            {
                return null;
            }

            try
            {
                return action();
            }
            catch (Exception exception)
            {
                failure = exception;
                return null;
            }
        }

        private static void CaptureException(
            Action action,
            ref Exception failure)
        {
            if (failure != null)
            {
                return;
            }

            try
            {
                action();
            }
            catch (Exception exception)
            {
                failure = exception;
            }
        }

        private static bool TryMoveNext(
            IEnumerator enumerator,
            out object yielded,
            out Exception failure)
        {
            try
            {
                bool moved = enumerator.MoveNext();
                yielded = moved ? enumerator.Current : null;
                failure = null;
                return moved;
            }
            catch (Exception exception)
            {
                yielded = null;
                failure = exception;
                return false;
            }
        }
    }
}
