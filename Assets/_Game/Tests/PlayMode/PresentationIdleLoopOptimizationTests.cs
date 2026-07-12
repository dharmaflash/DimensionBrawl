using System.Collections;
using System.Reflection;
using DimensionBrawl.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace DimensionBrawl.Tests
{
    public sealed class PresentationIdleLoopOptimizationTests
    {
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
            Assert.IsNull(
                typeof(ActionCinematicCueDirector).GetMethod("Update", flags),
                "Cinematic framing should derive remaining time from its clock instead of polling while idle.");
            Assert.IsNull(
                typeof(DimensionBrawl.Player.PlayerSkill1LaserSweepAction).GetMethod("Update", flags),
                "Laser-sweep VFX should return through its finite tail routine instead of an idle poll.");
            Assert.IsNull(
                typeof(DimensionBrawl.LevelDesign.OlympusStationCombatStageRuntimeBossTargetBinder).GetMethod(
                    "LateUpdate",
                    flags),
                "The Station boss binder should finish in Awake, OnEnable, and Start without a permanent empty callback.");
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
                typeof(DimensionBrawl.LevelDesign.OlympusStationStageClearOverlay).GetMethod("Update", flags),
                "Station clear presentation should react to pocket-clear and boss-death events.");
            Assert.IsNull(
                typeof(OlympusTutorialOverlayPresenter).GetMethod("Update", flags),
                "Tutorial overlay animation should run only while a finite transition is active.");

            System.Type combatHudBinder = System.Type.GetType(
                "DimensionBrawl.UI.BossBarrageLaneReviewCombatHudBinder, Assembly-CSharp");
            Assert.IsNotNull(combatHudBinder);
            Assert.IsNull(
                combatHudBinder.GetMethod("Update", flags),
                "Combat HUD polling should run at its reviewed refresh rate instead of every render frame.");

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

            System.Type sceneEntryNoticeOverlay = System.Type.GetType(
                "DimensionBrawl.UI.SceneEntryNoticeOverlay, Assembly-CSharp");
            Assert.IsNotNull(sceneEntryNoticeOverlay);
            Assert.IsNull(
                sceneEntryNoticeOverlay.GetMethod("Update", flags),
                "Scene-entry notice animation should stay inside its finite playback routine.");

            System.Type reviewOverlayHud = System.Type.GetType(
                "DimensionBrawl.UI.BossBarrageLaneReviewOverlayHud, DimensionBrawl.Runtime");
            Assert.IsNotNull(reviewOverlayHud);
            Assert.IsNull(
                reviewOverlayHud.GetMethod("Update", flags),
                "Pause and result overlays should react to input and pocket result events.");
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
        public IEnumerator StageClearOverlayBindsLateBossAndReactsToDeathEvent()
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            GameObject overlayRoot = new GameObject("LateBoundStageClearOverlayTest");
            GameObject bossRoot = null;
            float previousTimeScale = Time.timeScale;
            try
            {
                overlayRoot.SetActive(false);
                var overlay = overlayRoot.AddComponent<
                    DimensionBrawl.LevelDesign.OlympusStationStageClearOverlay>();
                overlayRoot.SetActive(true);
                yield return null;

                bossRoot = new GameObject("BossBarrageLaneReview_BossProxy_NeedleLock");
                DimensionBrawl.Combat.CombatHealth bossHealth =
                    bossRoot.AddComponent<DimensionBrawl.Combat.CombatHealth>();
                bossHealth.ConfigureTeam(DimensionBrawl.Combat.DamageTeam.Enemy);
                bossHealth.ConfigureMaxHealth(100f);

                FieldInfo bossHealthField = overlay.GetType().GetField("bossHealth", flags);
                Assert.IsNotNull(bossHealthField);
                float timeoutAt = Time.realtimeSinceStartup + 1f;
                while (bossHealthField.GetValue(overlay) != bossHealth
                    && Time.realtimeSinceStartup < timeoutAt)
                {
                    yield return null;
                }

                Assert.AreSame(bossHealth, bossHealthField.GetValue(overlay));
                Assert.IsTrue(bossHealth.TryApplyDamage(new DimensionBrawl.Combat.DamageInfo(
                    source: null,
                    sourceTeam: DimensionBrawl.Combat.DamageTeam.Player,
                    amount: bossHealth.MaxHealth,
                    point: bossRoot.transform.position,
                    direction: Vector3.forward,
                    hitStopSeconds: 0f)));

                FieldInfo shownField = overlay.GetType().GetField("shown", flags);
                Assert.IsNotNull(shownField);
                Assert.IsTrue((bool)shownField.GetValue(overlay));
            }
            finally
            {
                Object.DestroyImmediate(overlayRoot);
                if (bossRoot != null)
                {
                    Object.DestroyImmediate(bossRoot);
                }

                Time.timeScale = previousTimeScale;
            }

            yield return null;
            LogAssert.NoUnexpectedReceived();
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

                controller.SetCinematicInputLocked(true);
                controller.QueueCombatModeSwap();
                Assert.IsTrue(controller.IsMeleeMode);

                controller.SetCinematicInputLocked(false);
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
                    Object.FindObjectsByType<MovementFootstepAudioScheduler>(
                        FindObjectsInactive.Include,
                        FindObjectsSortMode.None);
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

                for (int frame = 0; frame < 180 && player.IsBlending; frame++)
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
