using System.Collections;
using System.Reflection;
using DimensionBrawl.Debugging;
using DimensionBrawl.Player;
using DimensionBrawl.Presentation;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace DimensionBrawl.Tests
{
    public sealed class PresentationIdleLoopOptimizationTests
    {
        private bool stageClearSceneTouchedByTest;

        [UnityTearDown]
        public IEnumerator CleanupStageClearSceneAfterTest()
        {
            if (!stageClearSceneTouchedByTest)
            {
                yield break;
            }

            int minimumFrame = Time.frameCount + 2;
            float timeoutAt = Time.realtimeSinceStartup + 3f;
            while (Time.realtimeSinceStartup <= timeoutAt)
            {
                bool leaseBusy = DimensionBrawl.LevelDesign.OlympusStageClearOverlay
                    .IsResultScenePreloadLeaseBusyForTests;
                Scene scene = SceneManager.GetSceneByName("UI_StageClear");
                if (!leaseBusy && scene.IsValid() && scene.isLoaded)
                {
                    AsyncOperation unload = SceneManager.UnloadSceneAsync(scene);
                    while (unload != null && !unload.isDone)
                    {
                        yield return null;
                    }
                }

                Scene remaining = SceneManager.GetSceneByName("UI_StageClear");
                if (Time.frameCount >= minimumFrame
                    && !leaseBusy
                    && !remaining.IsValid())
                {
                    break;
                }

                yield return null;
            }

            bool finalLeaseBusy = DimensionBrawl.LevelDesign.OlympusStageClearOverlay
                .IsResultScenePreloadLeaseBusyForTests;
            Scene finalScene = SceneManager.GetSceneByName("UI_StageClear");
            stageClearSceneTouchedByTest = false;
            Assert.That(finalLeaseBusy, Is.False,
                "The owned result-scene preload lease did not settle during teardown.");
            Assert.That(finalScene.IsValid(), Is.False,
                "UI_StageClear remained loaded or loading after teardown.");
        }

        [Test]
        public void EventDrivenPresentationDriversDoNotDeclareIdleUpdateLoops()
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            Assert.IsNull(
                typeof(SummonEnergyVfxCuePresenter).GetMethod("Update", flags),
                "Summon energy presentation should react to ladder events instead of polling every frame.");
            Assert.IsNull(
                typeof(CinematicBlendShapeExpressionPlayer).GetMethod("Update", flags),
                "Blend-shape presentation should run only while a finite blend routine is active.");
            Assert.IsNull(
                typeof(BossBarrageLaneTelegraphPresenter).GetMethod("Update", flags),
                "Boss lane telegraphs should animate only from windup through their finite release flash.");
            Assert.IsNull(
                typeof(BossBarrageVisualCueDriver).GetMethod("Update", flags),
                "Boss pulse and damage feedback should run only while a visual timer is active.");
            Assert.IsNull(
                typeof(PlayerRangedBasicVfxCueDriver).GetMethod("Update", flags),
                "Physical impact VFX should poll release times only while pooled impacts are active.");
            Assert.IsNull(
                typeof(CombatVfxCuePlayer).GetMethod("Update", flags),
                "Combat VFX pools should poll release times only while scheduled content is active.");
            Assert.IsNull(
                typeof(ActionScreenCuePresenter).GetMethod("Update", flags),
                "Screen cues should tick only while a finite cue or damage feedback timer is active.");
            Assert.IsNull(
                typeof(PerfectDodgeTimeWarp).GetMethod("Update", flags),
                "Perfect-dodge time warp should refresh receivers only while the warp is active.");
            System.Type combatSessionOverlayPresenter = System.Type.GetType(
                "DimensionBrawl.UI.CombatSessionOverlayPresenter, Assembly-CSharp");
            Assert.IsNotNull(combatSessionOverlayPresenter);
            Assert.IsNull(
                combatSessionOverlayPresenter.GetMethod("LateUpdate", flags),
                "Pause ownership should be guarded only while the session overlay owns a finite pause lease.");
            Assert.IsNull(
                typeof(ActionCinematicCueDirector).GetMethod("Update", flags),
                "Cinematic framing should derive remaining time from its clock instead of polling while idle.");
            Assert.IsNull(
                typeof(DimensionBrawl.Player.PlayerSkill1LaserSweepAction).GetMethod("Update", flags),
                "Laser-sweep VFX should return through its finite tail routine instead of an idle poll.");
            Assert.IsNull(
                typeof(MovementFootstepAudioPresenter).GetMethod("Update", flags),
                "Footstep presenters should share one scheduler instead of each crossing the native Update boundary.");
            Assert.IsNull(
                typeof(DimensionBrawl.Player.PlayerSupportSummonSlotAction).GetMethod("Update", flags),
                "Support summon slots should use input callbacks and finite feedback timers instead of idle polling.");
            Assert.IsNull(
                typeof(DimensionBrawl.Player.PlayerSkill1Action).GetMethod("Update", flags),
                "Skill1 should use input callbacks and a finite blocked-feedback timer instead of idle polling.");
            Assert.IsNull(
                typeof(DimensionBrawl.Player.PlayerSummonSlot1Action).GetMethod("Update", flags),
                "The primary summon slot should use input callbacks and finite cooldown timers instead of idle polling.");
            Assert.IsNull(
                typeof(DimensionBrawl.Player.PlayerCombatModeController).GetMethod("Update", flags),
                "Combat mode swaps should use input callbacks instead of polling every frame.");
            Assert.IsNull(
                typeof(ActionCameraTargetBridge).GetMethod("LateUpdate", flags),
                "Camera target binding should react immediately to target events and use low-frequency retargeting.");
            Assert.IsNull(
                typeof(DimensionBrawl.Combat.CombatTimeDilationReceiver).GetMethod("Update", flags),
                "Time-dilation receivers should tick only while a finite slowdown is active.");
            Assert.IsNull(
                typeof(SpatialOneShotAudioPool).GetMethod("Update", flags),
                "Spatial audio pools should scan release times only while one-shots are active.");
            Assert.IsNull(
                typeof(SpatialOneShotVfxPool).GetMethod("Update", flags),
                "Spatial VFX pools should scan release times only while effects are active.");
            Assert.IsNull(
                typeof(DimensionBrawl.LevelDesign.OlympusStageClearOverlay).GetMethod("Update", flags),
                "Stage clear presentation should run only after the canonical flow signals completion.");
            Assert.IsNull(
                typeof(DimensionBrawl.LevelDesign.OlympusStationBossTerminalAftermathPresenter)
                    .GetMethod("Update", flags),
                "Boss-terminal aftermath should tick only while its finite death bridge is active.");
            Assert.IsNull(
                typeof(OlympusTutorialOverlayPresenter).GetMethod("Update", flags),
                "Tutorial overlay animation should run only while a finite transition is active.");
            Assert.IsNull(
                typeof(DimensionBrawl.LevelDesign.OlympusCorridorCombatFlowController).GetMethod("Update", flags),
                "Corridor flow should observe only its active intro, HUD reveal, and gameplay phases.");
            Assert.IsNull(
                typeof(DimensionBrawl.Combat.SummonEnergyLadder).GetMethod("Update", flags),
                "Summon energy should share the reviewed-rate combat resource scheduler.");
            Assert.IsNull(
                typeof(DimensionBrawl.Combat.BossPressureCostLadder).GetMethod("Update", flags),
                "Boss pressure cost should share the reviewed-rate combat resource scheduler.");
            Assert.IsNull(
                typeof(DimensionBrawl.Combat.BossBasicFireEmitter).GetMethod("Update", flags),
                "Boss basic fire cadence should use the shared boss combat scheduler.");
            Assert.IsNull(
                typeof(DimensionBrawl.Combat.BossPressureActionDirector).GetMethod("Update", flags),
                "Boss pressure decisions should use the shared boss combat scheduler.");
            Assert.IsNull(
                typeof(DimensionBrawl.Combat.BossBarrageEmitter).GetMethod("Update", flags),
                "Boss barrage cadence should use the shared boss combat scheduler.");
            Assert.IsNull(
                typeof(DimensionBrawl.Combat.EnemySummonPacingDirector).GetMethod("Update", flags),
                "Enemy summon pacing should use the shared boss combat scheduler.");

            System.Type combatHudBinder = System.Type.GetType(
                "DimensionBrawl.UI.BossBarrageLaneReviewCombatHudBinder, Assembly-CSharp");
            Assert.IsNotNull(combatHudBinder);
            Assert.IsNull(
                combatHudBinder.GetMethod("Update", flags),
                "Combat HUD polling should run at its configured refresh rate instead of every render frame.");

            System.Type combatHudPresenter = System.Type.GetType(
                "DimensionBrawl.UI.CombatHudPresenter, Assembly-CSharp");
            Assert.IsNotNull(combatHudPresenter);
            Assert.IsNull(
                combatHudPresenter.GetMethod("Update", flags),
                "Combat HUD feedback should tick only while a damage or meter flash is active.");

            System.Type combatHudAimDragInput = System.Type.GetType(
                "DimensionBrawl.UI.CombatHudAimDragInput, Assembly-CSharp");
            Assert.IsNotNull(combatHudAimDragInput);
            Assert.IsNull(
                combatHudAimDragInput.GetMethod("Update", flags),
                "Combat HUD aim drag should react to pointer and input-action events instead of polling every frame.");

        }

        [Test]
        public void StageClearEntranceUsesExactlyTwentyEightPresentationClockSamples()
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            GameObject root = new GameObject("ManualClockStageClearEntrance");
            root.SetActive(false);
            IEnumerator routine = null;
            try
            {
                DimensionBrawl.UI.StageClear.StageClearScreenPresenter presenter =
                    root.AddComponent<
                        DimensionBrawl.UI.StageClear.StageClearScreenPresenter>();
                presenter.GetType().GetField("playEntranceOnEnable", flags)
                    ?.SetValue(presenter, false);
                presenter.GetType().GetField("entranceDelaySeconds", flags)
                    ?.SetValue(presenter, 0.02f);
                presenter.GetType().GetField("entranceDurationSeconds", flags)
                    ?.SetValue(presenter, 0.42f);
                MethodInfo method = presenter.GetType().GetMethod(
                    "EntranceRoutine",
                    flags);
                Assert.That(method, Is.Not.Null);
                routine = (IEnumerator)method.Invoke(presenter, null);
                Assert.That(routine, Is.Not.Null);

                using (PresentationClock.ManualLease lease =
                    PresentationClock.AcquireManual(this, 60))
                {
                    for (int sample = 0; sample < 28; sample++)
                    {
                        lease.SetFrame(sample);
                        Assert.That(routine.MoveNext(), Is.True, $"sample={sample}");
                        Assert.That(presenter.EntranceCompleted, Is.False,
                            $"sample={sample}");
                    }

                    lease.SetFrame(28);
                    Assert.That(routine.MoveNext(), Is.False);
                    Assert.That(presenter.EntranceCompleted, Is.True);
                }

                Assert.That(PresentationClock.IsManuallyDriven, Is.False);
            }
            finally
            {
                (routine as System.IDisposable)?.Dispose();
                Object.DestroyImmediate(root);
            }
        }

        [UnityTest]
        public IEnumerator StageClearAdditivePreloadBecomesAvailableOnTheFollowingPlayerFrame()
        {
            const string sceneName = "UI_StageClear";
            const string scenePath = "Assets/_Game/Scenes/UI/UI_StageClear.unity";
            Scene existing = SceneManager.GetSceneByName(sceneName);
            if (existing.IsValid() && existing.isLoaded)
            {
                AsyncOperation existingUnload = SceneManager.UnloadSceneAsync(existing);
                while (existingUnload != null && !existingUnload.isDone)
                {
                    yield return null;
                }
            }

            stageClearSceneTouchedByTest = true;
            System.Exception loadFailure = null;
            Scene loading = default;
            try
            {
                loading = EditorSceneManager.LoadSceneInPlayMode(
                    scenePath,
                    new LoadSceneParameters(LoadSceneMode.Additive));
            }
            catch (System.Exception exception)
            {
                loadFailure = exception;
            }

            bool immediateValid = loading.IsValid();
            bool immediateLoaded = loading.isLoaded;

            yield return null;

            Scene loaded = SceneManager.GetSceneByName(sceneName);
            int presenterCount = 0;
            bool loadedOnFollowingFrame = loaded.IsValid() && loaded.isLoaded;
            if (loadedOnFollowingFrame)
            {
                GameObject[] roots = loaded.GetRootGameObjects();
                for (int i = 0; i < roots.Length; i++)
                {
                    presenterCount += roots[i].GetComponentsInChildren<
                        DimensionBrawl.UI.StageClear.StageClearScreenPresenter>(true).Length;
                }
            }

            if (loadedOnFollowingFrame)
            {
                AsyncOperation unload = SceneManager.UnloadSceneAsync(loaded);
                while (unload != null && !unload.isDone)
                {
                    yield return null;
                }
            }

            Assert.That(loadFailure, Is.Null);
            Assert.That(immediateValid, Is.True);
            Assert.That(immediateLoaded, Is.False,
                "The terminal result scene must be requested one frame before its exact presentation frame.");
            Assert.That(loadedOnFollowingFrame, Is.True);
            Assert.That(presenterCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator StageClearOverlayDestroyCancelsItsDeferredAdditivePreload()
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            const string sceneName = "UI_StageClear";
            Scene existing = SceneManager.GetSceneByName(sceneName);
            if (existing.IsValid() && existing.isLoaded)
            {
                AsyncOperation existingUnload = SceneManager.UnloadSceneAsync(existing);
                while (existingUnload != null && !existingUnload.isDone)
                {
                    yield return null;
                }
            }

            stageClearSceneTouchedByTest = true;
            GameObject root = new GameObject("OwnedStageClearPreloadCancellationTest");
            root.SetActive(false);
            var overlay = root.AddComponent<
                DimensionBrawl.LevelDesign.OlympusStageClearOverlay>();
            overlay.GetType().GetField("postBossDefeatHoldSeconds", flags)
                ?.SetValue(overlay, 0f);
            overlay.GetType().GetField("combatHudExitSeconds", flags)
                ?.SetValue(overlay, 0f);
            int failureCount = 0;
            overlay.PresentationFailed += (_, _) => failureCount++;
            root.SetActive(true);
            float previousTimeScale = Time.timeScale;
            bool requested = overlay.TryShow(null, out string requestError);
            bool busyBeforeDestroy = DimensionBrawl.LevelDesign.OlympusStageClearOverlay
                .IsResultScenePreloadLeaseBusyForTests;
            bool shownBeforeDestroy = overlay.IsShown;
            LogAssert.Expect(
                LogType.Warning,
                "[OlympusStageClearOverlay] The result overlay was disabled before presentation acknowledgement.");
            Object.DestroyImmediate(root);
            root = null;
            bool busyImmediatelyAfterDestroy = DimensionBrawl.LevelDesign
                .OlympusStageClearOverlay.IsResultScenePreloadLeaseBusyForTests;

            GameObject retryRoot = new GameObject("BlockedStageClearPreloadRetryTest");
            var retryOverlay = retryRoot.AddComponent<
                DimensionBrawl.LevelDesign.OlympusStageClearOverlay>();
            bool retryAccepted = retryOverlay.TryShow(null, out string retryError);
            Object.DestroyImmediate(retryRoot);

            float timeoutAt = Time.realtimeSinceStartup + 3f;
            while (DimensionBrawl.LevelDesign.OlympusStageClearOverlay
                    .IsResultScenePreloadLeaseBusyForTests
                && Time.realtimeSinceStartup <= timeoutAt)
            {
                yield return null;
            }

            Scene remaining = SceneManager.GetSceneByName(sceneName);
            while (remaining.IsValid()
                && remaining.isLoaded
                && Time.realtimeSinceStartup <= timeoutAt)
            {
                yield return null;
                remaining = SceneManager.GetSceneByName(sceneName);
            }

            try
            {
                Assert.That(requested, Is.True, requestError);
                Assert.That(busyBeforeDestroy, Is.True);
                Assert.That(busyImmediatelyAfterDestroy, Is.True);
                Assert.That(shownBeforeDestroy, Is.True);
                Assert.That(failureCount, Is.EqualTo(1));
                Assert.That(retryAccepted, Is.False);
                Assert.That(retryError, Does.Contain("still being cleaned up"));
                Assert.That(DimensionBrawl.LevelDesign.OlympusStageClearOverlay
                    .IsResultScenePreloadLeaseBusyForTests, Is.False);
                Assert.That(remaining.IsValid() && remaining.isLoaded, Is.False);
                Assert.That(Time.timeScale, Is.EqualTo(previousTimeScale).Within(0.0001f));
            }
            finally
            {
                if (root != null)
                {
                    Object.DestroyImmediate(root);
                }

                Time.timeScale = previousTimeScale;
            }
        }

        [Test]
        public void StageClearOverlayPublishesOnlyOneTerminalFailure()
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            GameObject root = new GameObject("IdempotentStageClearFailureTest");
            try
            {
                var overlay = root.AddComponent<
                    DimensionBrawl.LevelDesign.OlympusStageClearOverlay>();
                int failureCount = 0;
                overlay.PresentationFailed += (_, _) => failureCount++;
                MethodInfo fail = overlay.GetType().GetMethod("FailPresentation", flags);
                Assert.That(fail, Is.Not.Null);
                LogAssert.Expect(
                    LogType.Warning,
                    "[OlympusStageClearOverlay] injected terminal failure");
                fail.Invoke(overlay, new object[] { "injected terminal failure", false });
                fail.Invoke(overlay, new object[] { "duplicate terminal failure", false });
                Assert.That(failureCount, Is.EqualTo(1));
                Assert.That(overlay.LastPresentationError,
                    Is.EqualTo("injected terminal failure"));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [UnityTest]
        public IEnumerator TutorialOverlayFiniteAnimationResumesAfterReenableAndSettles()
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            GameObject root = new GameObject("FiniteTutorialOverlayTest");
            try
            {
                OlympusTutorialOverlayPresenter presenter =
                    root.AddComponent<OlympusTutorialOverlayPresenter>();
                presenter.GetType().GetField("transitionSeconds", flags)?.SetValue(presenter, 0.05f);
                presenter.GetType().GetField("warningBootSeconds", flags)?.SetValue(presenter, 0.05f);
                presenter.GetType().GetField("dialogueCharactersPerSecond", flags)?.SetValue(presenter, 200f);

                FieldInfo animationRoutineField = presenter.GetType().GetField("animationRoutine", flags);
                FieldInfo transitionTimerField = presenter.GetType().GetField("transitionTimer", flags);
                FieldInfo warningBootTimerField = presenter.GetType().GetField("warningBootTimer", flags);
                Assert.IsNotNull(animationRoutineField);
                Assert.IsNotNull(transitionTimerField);
                Assert.IsNotNull(warningBootTimerField);

                presenter.Show(
                    "OPERATOR",
                    "Move.",
                    "MOVE",
                    OlympusTutorialOverlayPresenter.FocusKind.MoveStick,
                    new Vector2(0.16f, 0.16f));
                Assert.IsNotNull(animationRoutineField.GetValue(presenter));

                yield return null;
                presenter.enabled = false;
                Assert.IsNull(animationRoutineField.GetValue(presenter));
                presenter.enabled = true;
                Assert.IsNotNull(animationRoutineField.GetValue(presenter));

                float timeoutAt = Time.realtimeSinceStartup + 1f;
                while (animationRoutineField.GetValue(presenter) != null
                    && Time.realtimeSinceStartup < timeoutAt)
                {
                    yield return null;
                }

                Assert.IsNull(animationRoutineField.GetValue(presenter));
                Assert.That((float)transitionTimerField.GetValue(presenter), Is.EqualTo(0.05f).Within(0.001f));
                Assert.That((float)warningBootTimerField.GetValue(presenter), Is.GreaterThanOrEqualTo(0.05f));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }

            yield return null;
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator StageClearOverlayWaitsForExplicitFlowSignal()
        {
            GameObject overlayRoot = new GameObject("ExplicitStageClearOverlayTest");
            float previousTimeScale = Time.timeScale;
            try
            {
                overlayRoot.SetActive(false);
                var overlay = overlayRoot.AddComponent<
                    DimensionBrawl.LevelDesign.OlympusStageClearOverlay>();
                overlayRoot.SetActive(true);
                yield return null;

                Assert.IsFalse(overlay.IsShown);
                Assert.AreEqual(previousTimeScale, Time.timeScale);

                overlay.Show();

                Assert.IsTrue(overlay.IsShown);
                Assert.AreEqual(0f, Time.timeScale);
            }
            finally
            {
                LogAssert.Expect(
                    LogType.Warning,
                    "[OlympusStageClearOverlay] The result overlay was disabled before presentation acknowledgement.");
                Object.DestroyImmediate(overlayRoot);
                Time.timeScale = previousTimeScale;
            }

            yield return null;
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void StageClearOverlayRestoresOwnedTimeScaleOnNonTerminalDisable()
        {
            const float combatTimeScale = 0.35f;
            GameObject overlayRoot = new GameObject("OwnedStageClearTimeScaleTest");
            float initialTimeScale = Time.timeScale;
            try
            {
                overlayRoot.SetActive(false);
                var overlay = overlayRoot.AddComponent<
                    DimensionBrawl.LevelDesign.OlympusStageClearOverlay>();
                overlayRoot.SetActive(true);
                Time.timeScale = combatTimeScale;

                overlay.Show();
                Assert.That(Time.timeScale, Is.Zero);

                overlayRoot.SetActive(false);

                Assert.That(Time.timeScale, Is.EqualTo(combatTimeScale).Within(0.0001f));
            }
            finally
            {
                Object.DestroyImmediate(overlayRoot);
                Time.timeScale = initialTimeScale;
            }
        }

        [Test]
        public void StageClearOverlayDoesNotRestoreAfterTerminalLoaderSupersedesTimeScale()
        {
            const float combatTimeScale = 0.35f;
            const float terminalTimeScale = 1f;
            GameObject overlayRoot = new GameObject("SupersededStageClearTimeScaleTest");
            float initialTimeScale = Time.timeScale;
            try
            {
                overlayRoot.SetActive(false);
                var overlay = overlayRoot.AddComponent<
                    DimensionBrawl.LevelDesign.OlympusStageClearOverlay>();
                overlayRoot.SetActive(true);
                Time.timeScale = combatTimeScale;

                overlay.Show();
                Assert.That(Time.timeScale, Is.Zero);

                Time.timeScale = terminalTimeScale;
                overlayRoot.SetActive(false);

                Assert.That(Time.timeScale, Is.EqualTo(terminalTimeScale).Within(0.0001f));
            }
            finally
            {
                Object.DestroyImmediate(overlayRoot);
                Time.timeScale = initialTimeScale;
            }
        }

        [UnityTest]
        public IEnumerator CombatModeQueueExecutesImmediatelyAndHonorsCinematicLock()
        {
            GameObject root = new GameObject("EventDrivenCombatModeQueueTest");
            try
            {
                DimensionBrawl.Player.PlayerCombatModeController controller =
                    root.AddComponent<DimensionBrawl.Player.PlayerCombatModeController>();
                Assert.IsTrue(controller.IsRangedMode);

                controller.QueueCombatModeSwap();
                Assert.IsTrue(controller.IsMeleeMode);

                controller.SetCinematicInputLocked(PlayerInputLockSource.EditorVerification, true);
                controller.QueueCombatModeSwap();
                Assert.IsTrue(controller.IsMeleeMode);

                controller.SetCinematicInputLocked(PlayerInputLockSource.EditorVerification, false);
                controller.QueueCombatModeSwap();
                Assert.IsTrue(controller.IsRangedMode);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }

            yield return null;
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator FootstepPresentersShareOneActiveScheduler()
        {
            int initialPresenterCount = MovementFootstepAudioScheduler.RegisteredPresenterCount;
            bool initiallyTicking = MovementFootstepAudioScheduler.IsTicking;
            GameObject first = new GameObject("FootstepScheduler_First");
            GameObject second = new GameObject("FootstepScheduler_Second");
            try
            {
                MovementFootstepAudioPresenter firstPresenter =
                    first.AddComponent<MovementFootstepAudioPresenter>();
                MovementFootstepAudioPresenter secondPresenter =
                    second.AddComponent<MovementFootstepAudioPresenter>();
                yield return null;

                MovementFootstepAudioScheduler[] schedulers =
                    Resources.FindObjectsOfTypeAll<MovementFootstepAudioScheduler>();
                Assert.AreEqual(1, schedulers.Length);
                Assert.AreEqual(
                    initialPresenterCount + 2,
                    MovementFootstepAudioScheduler.RegisteredPresenterCount);
                Assert.IsTrue(MovementFootstepAudioScheduler.IsTicking);

                firstPresenter.enabled = false;
                yield return null;
                Assert.AreEqual(
                    initialPresenterCount + 1,
                    MovementFootstepAudioScheduler.RegisteredPresenterCount);
                Assert.IsTrue(MovementFootstepAudioScheduler.IsTicking);

                secondPresenter.enabled = false;
                yield return null;
                Assert.AreEqual(
                    initialPresenterCount,
                    MovementFootstepAudioScheduler.RegisteredPresenterCount);
                Assert.AreEqual(initiallyTicking, MovementFootstepAudioScheduler.IsTicking);
            }
            finally
            {
                Object.DestroyImmediate(first);
                Object.DestroyImmediate(second);
            }
        }

        [UnityTest]
        public IEnumerator CombatResourceLaddersShareSchedulerAndStopWhenDisabled()
        {
            int initialEnergyCount =
                DimensionBrawl.Combat.CombatResourceTickScheduler.RegisteredEnergyLadderCount;
            int initialBossCostCount =
                DimensionBrawl.Combat.CombatResourceTickScheduler.RegisteredBossCostLadderCount;
            bool initiallyTicking = DimensionBrawl.Combat.CombatResourceTickScheduler.IsTicking;
            GameObject energyRoot = new GameObject("ScheduledEnergyLadderTest");
            GameObject bossCostRoot = new GameObject("ScheduledBossCostLadderTest");
            try
            {
                DimensionBrawl.Combat.SummonEnergyLadder energy =
                    energyRoot.AddComponent<DimensionBrawl.Combat.SummonEnergyLadder>();
                DimensionBrawl.Combat.BossPressureCostLadder bossCost =
                    bossCostRoot.AddComponent<DimensionBrawl.Combat.BossPressureCostLadder>();

                Assert.AreEqual(
                    initialEnergyCount + 1,
                    DimensionBrawl.Combat.CombatResourceTickScheduler.RegisteredEnergyLadderCount);
                Assert.AreEqual(
                    initialBossCostCount + 1,
                    DimensionBrawl.Combat.CombatResourceTickScheduler.RegisteredBossCostLadderCount);
                Assert.IsTrue(DimensionBrawl.Combat.CombatResourceTickScheduler.IsTicking);

                var inventory = new MobilePerformanceSceneResult();
                MobilePerformanceBenchmarkRunner.CaptureRuntimeInventory(inventory);
                MobilePerformanceFrameLoopInventory schedulerLoop = inventory.FrameLoops.Find(
                    loop => loop.TypeName == typeof(DimensionBrawl.Combat.CombatResourceTickScheduler).FullName);
                Assert.IsNotNull(
                    schedulerLoop,
                    "Global combat resource scheduling should remain visible in runtime loop budgets.");
                Assert.AreEqual(1, schedulerLoop.UpdateInstances);

                yield return new WaitForSeconds(0.12f);
                Assert.Greater(energy.CurrentMana, 0f);
                Assert.Greater(bossCost.CurrentTierCost, 0f);

                energy.enabled = false;
                bossCost.enabled = false;
                Assert.AreEqual(
                    initialEnergyCount,
                    DimensionBrawl.Combat.CombatResourceTickScheduler.RegisteredEnergyLadderCount);
                Assert.AreEqual(
                    initialBossCostCount,
                    DimensionBrawl.Combat.CombatResourceTickScheduler.RegisteredBossCostLadderCount);
                Assert.AreEqual(
                    initiallyTicking,
                    DimensionBrawl.Combat.CombatResourceTickScheduler.IsTicking);
            }
            finally
            {
                Object.DestroyImmediate(energyRoot);
                Object.DestroyImmediate(bossCostRoot);
            }

            yield return null;
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator BossCadenceDriversShareSchedulerAndStopWhenDisabled()
        {
            int initialBasicFireCount =
                DimensionBrawl.Combat.BossCombatCadenceScheduler.RegisteredBasicFireEmitterCount;
            int initialActionDirectorCount =
                DimensionBrawl.Combat.BossCombatCadenceScheduler.RegisteredActionDirectorCount;
            int initialBarrageCount =
                DimensionBrawl.Combat.BossCombatCadenceScheduler.RegisteredBarrageEmitterCount;
            int initialPacingCount =
                DimensionBrawl.Combat.BossCombatCadenceScheduler.RegisteredSummonPacingDirectorCount;
            bool initiallyTicking = DimensionBrawl.Combat.BossCombatCadenceScheduler.IsTicking;
            GameObject root = new GameObject("ScheduledBossCadenceTest");
            try
            {
                DimensionBrawl.Combat.BossBasicFireEmitter basicFire =
                    root.AddComponent<DimensionBrawl.Combat.BossBasicFireEmitter>();
                DimensionBrawl.Combat.BossPressureActionDirector actionDirector =
                    root.AddComponent<DimensionBrawl.Combat.BossPressureActionDirector>();
                DimensionBrawl.Combat.BossBarrageEmitter barrage =
                    root.AddComponent<DimensionBrawl.Combat.BossBarrageEmitter>();
                DimensionBrawl.Combat.EnemySummonPacingDirector pacing =
                    root.AddComponent<DimensionBrawl.Combat.EnemySummonPacingDirector>();

                Assert.AreEqual(
                    initialBasicFireCount + 1,
                    DimensionBrawl.Combat.BossCombatCadenceScheduler.RegisteredBasicFireEmitterCount);
                Assert.AreEqual(
                    initialActionDirectorCount + 1,
                    DimensionBrawl.Combat.BossCombatCadenceScheduler.RegisteredActionDirectorCount);
                Assert.AreEqual(
                    initialBarrageCount + 1,
                    DimensionBrawl.Combat.BossCombatCadenceScheduler.RegisteredBarrageEmitterCount);
                Assert.AreEqual(
                    initialPacingCount + 1,
                    DimensionBrawl.Combat.BossCombatCadenceScheduler.RegisteredSummonPacingDirectorCount);
                Assert.IsTrue(DimensionBrawl.Combat.BossCombatCadenceScheduler.IsTicking);

                var inventory = new MobilePerformanceSceneResult();
                MobilePerformanceBenchmarkRunner.CaptureRuntimeInventory(inventory);
                MobilePerformanceFrameLoopInventory schedulerLoop = inventory.FrameLoops.Find(
                    loop => loop.TypeName == typeof(DimensionBrawl.Combat.BossCombatCadenceScheduler).FullName);
                Assert.IsNotNull(
                    schedulerLoop,
                    "Global boss cadence scheduling should remain visible in runtime loop budgets.");
                Assert.AreEqual(1, schedulerLoop.UpdateInstances);

                basicFire.enabled = false;
                actionDirector.enabled = false;
                barrage.enabled = false;
                pacing.enabled = false;
                Assert.AreEqual(
                    initialBasicFireCount,
                    DimensionBrawl.Combat.BossCombatCadenceScheduler.RegisteredBasicFireEmitterCount);
                Assert.AreEqual(
                    initialActionDirectorCount,
                    DimensionBrawl.Combat.BossCombatCadenceScheduler.RegisteredActionDirectorCount);
                Assert.AreEqual(
                    initialBarrageCount,
                    DimensionBrawl.Combat.BossCombatCadenceScheduler.RegisteredBarrageEmitterCount);
                Assert.AreEqual(
                    initialPacingCount,
                    DimensionBrawl.Combat.BossCombatCadenceScheduler.RegisteredSummonPacingDirectorCount);
                Assert.AreEqual(initiallyTicking, DimensionBrawl.Combat.BossCombatCadenceScheduler.IsTicking);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }

            yield return null;
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator SupportSummonBlockedHintUsesFiniteTimerRoutine()
        {
            GameObject root = new GameObject("SupportSummonFiniteFeedbackTest");
            try
            {
                DimensionBrawl.Player.PlayerSupportSummonSlotAction action =
                    root.AddComponent<DimensionBrawl.Player.PlayerSupportSummonSlotAction>();

                action.QueueSummon();
                Assert.IsTrue(action.ShowUseBlockedHint);
                Assert.AreEqual("Energy system missing", action.LastUseBlockedReason);

                float startedAt = Time.realtimeSinceStartup;
                while (action.ShowUseBlockedHint)
                {
                    Assert.Less(
                        Time.realtimeSinceStartup - startedAt,
                        2f,
                        "Support summon blocked feedback did not settle in real time.");
                    yield return null;
                }

                Assert.IsFalse(action.ShowUseBlockedHint);
                Assert.IsNull(action.LastUseBlockedReason);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [UnityTest]
        public IEnumerator PrimarySkillAndSummonBlockedHintsUseFiniteTimerRoutines()
        {
            GameObject skillRoot = new GameObject("Skill1FiniteFeedbackTest");
            GameObject summonRoot = new GameObject("SummonSlot1FiniteFeedbackTest");
            try
            {
                DimensionBrawl.Player.PlayerSkill1Action skill =
                    skillRoot.AddComponent<DimensionBrawl.Player.PlayerSkill1Action>();
                DimensionBrawl.Player.PlayerSummonSlot1Action summon =
                    summonRoot.AddComponent<DimensionBrawl.Player.PlayerSummonSlot1Action>();

                skill.QueueSkill1();
                summon.QueueSummonSlot1();
                Assert.IsTrue(skill.ShowUseBlockedHint);
                Assert.IsTrue(summon.ShowUseBlockedHint);

                float startedAt = Time.realtimeSinceStartup;
                while (skill.ShowUseBlockedHint || summon.ShowUseBlockedHint)
                {
                    Assert.Less(
                        Time.realtimeSinceStartup - startedAt,
                        2f,
                        "Primary action blocked feedback did not settle in real time.");
                    yield return null;
                }

                Assert.IsNull(skill.LastUseBlockedReason);
                Assert.IsNull(summon.LastUseBlockedReason);
            }
            finally
            {
                Object.DestroyImmediate(skillRoot);
                Object.DestroyImmediate(summonRoot);
            }
        }

        [UnityTest]
        public IEnumerator TimeDilationReceiverRestoresAndStopsFiniteRoutine()
        {
            GameObject root = new GameObject("FiniteTimeDilationTest");
            try
            {
                DimensionBrawl.Combat.CombatTimeDilationReceiver receiver =
                    root.AddComponent<DimensionBrawl.Combat.CombatTimeDilationReceiver>();
                receiver.ApplyTimeDilation(0.35f, 0.04f, 0.04f);
                Assert.IsTrue(receiver.IsDilationActive);

                for (int frame = 0; frame < 180 && receiver.IsDilationActive; frame++)
                {
                    yield return null;
                }

                Assert.IsFalse(receiver.IsDilationActive);
                Assert.AreEqual(1f, receiver.CurrentTimeScale, 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [UnityTest]
        public IEnumerator BlendShapeRoutineSettlesExactlyAndStops()
        {
            GameObject root = new GameObject("BlendShapeRoutineTest");
            Mesh mesh = new Mesh { name = "BlendShapeRoutineTestMesh" };
            float previousTimeScale = Time.timeScale;
            try
            {
                Time.timeScale = 1f;
                mesh.vertices = new[] { Vector3.zero, Vector3.right, Vector3.up };
                mesh.triangles = new[] { 0, 1, 2 };
                mesh.AddBlendShapeFrame(
                    "Smile",
                    100f,
                    new[] { Vector3.up * 0.01f, Vector3.zero, Vector3.zero },
                    new Vector3[3],
                    new Vector3[3]);

                SkinnedMeshRenderer renderer = root.AddComponent<SkinnedMeshRenderer>();
                renderer.sharedMesh = mesh;
                CinematicBlendShapeExpressionPlayer player =
                    root.AddComponent<CinematicBlendShapeExpressionPlayer>();
                player.Configure(new[]
                {
                    new CinematicBlendShapeExpressionPlayer.ExpressionPreset(
                        "Smile",
                        new[] { new CinematicBlendShapeExpressionPlayer.ShapeWeight("Smile", 72f) })
                });

                Assert.IsTrue(player.PlayExpression("Smile"));
                Assert.IsTrue(player.IsBlending);

                float settleDeadline = Time.realtimeSinceStartup + 2f;
                while (player.IsBlending && Time.realtimeSinceStartup < settleDeadline)
                {
                    yield return null;
                }

                Assert.IsFalse(player.IsBlending, "The finite blend routine should stop after reaching its target.");
                Assert.AreEqual(0, player.ActiveTargetCount, "Settled blend targets should not remain in the frame loop.");
                Assert.AreEqual(72f, renderer.GetBlendShapeWeight(0), 0.001f);
            }
            finally
            {
                Time.timeScale = previousTimeScale;
                Object.DestroyImmediate(root);
                Object.DestroyImmediate(mesh);
            }
        }

    }
}
