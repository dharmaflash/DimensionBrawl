using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using DimensionBrawl.AI;
using DimensionBrawl.Combat;
using DimensionBrawl.Enemies;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.Player;
using DimensionBrawl.Presentation;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace DimensionBrawl.Tests
{
    public sealed class CityHeroPocketRuntimePlayModeTests
    {
        private const string ScenePath =
            "Assets/_Game/Scenes/CityHeroPocketStage.unity";
        private const string RifleCrossfirePatternPath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_BasicSoldier_RifleCrossfire.asset";

        [UnitySetUp]
        public IEnumerator LoadCityPocketIfAuthored()
        {
            Assert.That(AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath),
                Is.Not.Null,
                "Committed CityHeroPocket scene output is missing; run setup before PlayMode gates.");

            Time.timeScale = 1f;
            EditorSceneManager.LoadSceneInPlayMode(
                ScenePath,
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator RestoreTimeScale()
        {
            Type overlayType = Type.GetType(
                "DimensionBrawl.UI.CombatSessionOverlayPresenter, Assembly-CSharp",
                throwOnError: false);
            Component overlay = FindFirstOptional(overlayType);
            if (overlay != null)
            {
                MethodInfo resume = overlay.GetType().GetMethod(
                    "Resume",
                    BindingFlags.Instance | BindingFlags.Public);
                resume?.Invoke(overlay, null);
            }
            Time.timeScale = 1f;
            yield return null;
            Time.timeScale = 1f;
        }

        [UnityTest]
        [Timeout(15000)]
        public IEnumerator FirstFramesBindEnemyProjectilesAndKeepGameplayFramed()
        {
            Camera camera = RequireSingle<Camera>();
            Vector3 reviewedG02Start = new(-0.35f, 2.35f, -10.2f);
            Assert.That(Vector3.Distance(camera.transform.position, reviewedG02Start),
                Is.LessThan(0.2f),
                "ActionCamera moved away from the reviewed G02 anchor on its first LateUpdate.");
            Vector3 reviewedLookAt = new(-0.2f, 1.15f, 1.7f);
            Assert.That(Vector3.Angle(
                    camera.transform.forward,
                    reviewedLookAt - camera.transform.position),
                Is.LessThan(2f),
                "ActionCamera first-frame look focus drifted from the reviewed G02 framing.");

            CityHeroPocketEnemyProjectileRootBinder binder =
                RequireSingle<CityHeroPocketEnemyProjectileRootBinder>();
            BasicSoldierProjectileAttackDriver driver =
                RequireSingle<BasicSoldierProjectileAttackDriver>();
            Assert.That(binder.IsConfigured, Is.True);
            Assert.That(driver.RuntimeProjectileRoot, Is.SameAs(binder.ProjectileRoot));
            Assert.That(driver.HasIndependentRuntimeProjectileRoot, Is.True);

            BasicSoldierEnemy soldier = RequireSingle<BasicSoldierEnemy>();
            soldier.enabled = false;
            CombatAiPatternProfile pattern =
                AssetDatabase.LoadAssetAtPath<CombatAiPatternProfile>(RifleCrossfirePatternPath);
            Assert.That(pattern, Is.Not.Null);
            InvokePatternState(driver, CombatAiPatternState.Tracking, pattern);
            InvokePatternState(driver, CombatAiPatternState.AttackActive, pattern);
            Assert.That(driver.LastFiredProjectile, Is.Not.Null);
            Assert.That(driver.LastFiredProjectile.transform.parent,
                Is.SameAs(binder.ProjectileRoot),
                "A fresh-load enemy shot escaped the scene-owned projectile root.");

            for (int i = 0; i < 12; i++)
            {
                yield return null;
            }

            CombatEncounterController encounter = RequireSingle<CombatEncounterController>();
            Assert.That(camera.transform.position.y, Is.InRange(2.0f, 2.75f),
                "ActionCamera pivot+shoulder mapping lifted the runtime view above the G02 envelope.");
            RequireSafeViewportPoint(
                camera,
                encounter.PlayerHealth.transform.position + Vector3.up * 1.1f,
                "player chest");
            RequireSafeViewportPoint(
                camera,
                encounter.EnemyHealth.transform.position + Vector3.up * 1.1f,
                "primary enemy chest");

            Component overlay = RequireSingle(ResolveHudType("CombatSessionOverlayPresenter"));
            InvokePublic(overlay, "ShowPause");
            yield return null;
            Transform retry = FindDescendant(overlay.transform, "RetryButton");
            Assert.That(retry == null || !retry.gameObject.activeSelf, Is.True,
                "City direct-load proof exposed the shared Corridor retry action.");
            if (retry != null && retry.TryGetComponent(out Button retryButton))
            {
                string sceneBeforeClick = SceneManager.GetActiveScene().path;
                retryButton.onClick.Invoke();
                yield return null;
                Assert.That(SceneManager.GetActiveScene().path, Is.EqualTo(sceneBeforeClick),
                    "Hidden City Retry still dispatched the shared Corridor route.");
            }
            InvokePublic(overlay, "Resume");
            yield return null;
            Assert.That(GetPublicProperty<object>(overlay, "Mode").ToString(),
                Is.EqualTo("Hidden"));
            Assert.That(Time.timeScale, Is.GreaterThan(0f));
        }

        [UnityTest]
        [Timeout(15000)]
        public IEnumerator SceneInstanceSummonS1ExecutesThroughExistingHudSlotAndRuntimeRoots()
        {
            SummonEnergyLadder energy = RequireSingle<SummonEnergyLadder>();
            PlayerSummonSlot1Action summon = RequireSingle<PlayerSummonSlot1Action>();
            CombatEncounterController encounter = RequireSingle<CombatEncounterController>();
            PlayerMovementController movement = RequireSingle<PlayerMovementController>();
            PlayerRangedBasicAttackAction ranged =
                RequireSingle<PlayerRangedBasicAttackAction>();
            Component hudBinder = RequireSingle(ResolveHudType("OneRowCombatHudBinder"));
            RectTransform summonButton = RequireRectTransform("SummonSlot1Button");
            Button summonVisualButton = summonButton.GetComponent<Button>();
            Component summonPointer = summonButton.GetComponent(
                ResolveHudType("CombatHudPointerActionInput"));
            SerializedObject serializedEnergy = new(energy);
            SerializedObject serializedSummon = new(summon);

            Assert.That(energy.gameObject, Is.SameAs(movement.gameObject),
                "City summon energy must be a player-root scene-instance component.");
            Assert.That(summon.gameObject, Is.SameAs(movement.gameObject),
                "City summon S1 must be a player-root scene-instance component.");
            Assert.That(serializedEnergy.FindProperty("laneSpace").objectReferenceValue,
                Is.Null);
            Assert.That(serializedEnergy.FindProperty("trackedPlayer").objectReferenceValue,
                Is.SameAs(movement.transform));
            Assert.That(serializedSummon.FindProperty("energyLadder").objectReferenceValue,
                Is.SameAs(energy));
            Assert.That(serializedSummon.FindProperty("sourceHealth").objectReferenceValue,
                Is.SameAs(encounter.PlayerHealth));
            Assert.That(serializedSummon.FindProperty("frontlineTargetHealth")
                    .objectReferenceValue,
                Is.SameAs(encounter.EnemyHealth));
            Assert.That(serializedSummon.FindProperty("laneSpace").objectReferenceValue,
                Is.Null);
            Assert.That(serializedSummon.FindProperty("projectileRoot").objectReferenceValue,
                Is.SameAs(ranged.ProjectileRoot));
            Transform cueRoot = serializedSummon.FindProperty("cueRoot")
                .objectReferenceValue as Transform;
            Assert.That(cueRoot, Is.Not.Null);
            Assert.That(cueRoot.name, Is.EqualTo("CityHeroPocketRuntime"));
            Assert.That(serializedSummon.FindProperty("summonActorRoot")
                    .objectReferenceValue,
                Is.SameAs(cueRoot));
            Assert.That(
                AssetDatabase.GetAssetPath(
                    serializedSummon.FindProperty("summonActionProfile")
                        .objectReferenceValue),
                Is.EqualTo(
                    "Assets/_Game/DesignData/Profiles/ActionFoundation/" +
                    "DB_SummonSlot1_ChargeBruiser.asset"));
            Assert.That(summon.RequiredSummonMana, Is.EqualTo(200f).Within(0.0001f));
            Assert.That(summon.SlotCooldownSeconds, Is.EqualTo(9.5f).Within(0.0001f));
            Assert.That(new SerializedObject(hudBinder).FindProperty("summonSlot1Action")
                    .objectReferenceValue,
                Is.SameAs(summon));
            Assert.That(summonVisualButton, Is.Not.Null);
            Assert.That(summonVisualButton.IsInteractable(), Is.True);
            Assert.That(summonPointer, Is.Not.Null);
            Assert.That(GetPublicProperty<object>(summonPointer, "ActionId").ToString(),
                Is.EqualTo("SummonSlot1"));
            Assert.That(GetPublicProperty<bool>(summonPointer, "SendsHoldState"), Is.False);

            energy.SetGainEnabled(false);
            energy.ResetLadder();
            energy.GrantCurrentTierEnergy(200f);
            Assert.That(energy.CurrentMana, Is.EqualTo(200f).Within(0.0001f));
            Assert.That(energy.AvailableTier, Is.EqualTo(2));

            int usedCount = 0;
            int spentTier = 0;
            void ObserveSummonUsed(int tier)
            {
                usedCount++;
                spentTier = tier;
            }
            summon.SummonSlot1Used += ObserveSummonUsed;
            try
            {
                var pointer = new PointerEventData(EventSystem.current)
                {
                    button = PointerEventData.InputButton.Left,
                    pointerId = -101,
                    position = ScreenPoint(summonButton)
                };
                ExecuteEvents.Execute(
                    summonButton.gameObject,
                    pointer,
                    ExecuteEvents.pointerDownHandler);
                ExecuteEvents.Execute(
                    summonButton.gameObject,
                    pointer,
                    ExecuteEvents.pointerUpHandler);
                yield return null;

                Assert.That(usedCount, Is.EqualTo(1),
                    "The existing S1 HUD pointer did not execute the authored summon action.");
                Assert.That(spentTier, Is.EqualTo(2));
                Assert.That(summon.TotalUseCount, Is.EqualTo(1));
                Assert.That(summon.LastSpentTier, Is.EqualTo(2));
                Assert.That(summon.LastFiredProjectileCount, Is.GreaterThan(0));
                Assert.That(energy.CurrentMana, Is.EqualTo(0f).Within(0.0001f));
                Assert.That(summon.IsSlotOnCooldown, Is.True);

                yield return new WaitForSeconds(0.35f);
                Assert.That(summon.ActiveSummonActorCount, Is.EqualTo(1),
                    "The existing ChargeBruiser actor did not enter through the runtime root.");
                Assert.That(summon.LastSummonActorHasHealth, Is.True);
            }
            finally
            {
                summon.SummonSlot1Used -= ObserveSummonUsed;
                summon.DismissActivePressureScreens();
                summon.ClearSlotCooldown();
                energy.ResetLadder();
                energy.SetGainEnabled(true);
            }
        }

        [UnityTest]
        [Timeout(15000)]
        public IEnumerator RangedOnlyInoriMovesDodgesAndFiresThroughNativeBridge()
        {
            RifleGirlNativeGameplayAnimatorBridge bridge =
                RequireSingle<RifleGirlNativeGameplayAnimatorBridge>();
            PlayerMovementController movement = RequireSingle<PlayerMovementController>();
            PlayerActionController action = RequireSingle<PlayerActionController>();
            PlayerCombatModeController mode = RequireSingle<PlayerCombatModeController>();
            PlayerRangedAimController aim = RequireSingle<PlayerRangedAimController>();
            PlayerRangedBasicAttackAction ranged =
                RequireSingle<PlayerRangedBasicAttackAction>();
            Assert.That(bridge.isActiveAndEnabled, Is.True);
            Assert.That(mode.IsRangedMode, Is.True);
            Assert.That(new SerializedObject(movement).FindProperty("animator").objectReferenceValue,
                Is.Null);
            Assert.That(new SerializedObject(action).FindProperty("animator").objectReferenceValue,
                Is.Null);

            Keyboard keyboard = Keyboard.current;
            bool ownsKeyboard = keyboard == null;
            if (ownsKeyboard)
            {
                keyboard = InputSystem.AddDevice<Keyboard>();
            }
            try
            {
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Tab));
                yield return null;
                InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                yield return null;
                Assert.That(mode.IsRangedMode, Is.True,
                    "The compact ranged-only player still accepted the Station Tab swap fallback.");
            }
            finally
            {
                if (ownsKeyboard && keyboard != null && keyboard.added)
                {
                    InputSystem.RemoveDevice(keyboard);
                }
            }

            Vector3 start = movement.transform.position;
            movement.SetMoveInput(Vector2.up);
            for (int i = 0; i < 8; i++)
            {
                yield return null;
            }
            movement.SetMoveInput(Vector2.zero);
            Assert.That(Vector3.ProjectOnPlane(movement.transform.position - start, Vector3.up).magnitude,
                Is.GreaterThan(0.05f), "Fresh-load Inori did not move.");

            movement.SetMoveInput(Vector2.right);
            action.QueueDodge();
            yield return null;
            Assert.That(action.IsDodging, Is.True,
                "Fresh-load Inori did not enter the reviewed dodge state.");
            movement.SetMoveInput(Vector2.zero);
            while (action.IsDodging)
            {
                yield return null;
            }

            aim.SetAimMode(true);
            yield return null;
            Assert.That(ranged.TryFire(), Is.True, ranged.LastUseBlockedReason);
            Assert.That(ranged.ProjectileRoot, Is.Not.Null);
            Assert.That(ranged.ProjectileRoot.name,
                Is.EqualTo("CityHeroPocket_PlayerProjectiles"));
        }

        [UnityTest]
        [Timeout(20000)]
        public IEnumerator InputModuleDispatchesHudFireDodgeJoystickAndAimDrag()
        {
            Canvas.ForceUpdateCanvases();
            InputSettings.EditorInputBehaviorInPlayMode previousEditorInputBehavior =
                InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSettings.BackgroundBehavior previousBackgroundBehavior =
                InputSystem.settings.backgroundBehavior;
            InputSystem.settings.editorInputBehaviorInPlayMode =
                InputSettings.EditorInputBehaviorInPlayMode
                    .AllDeviceInputAlwaysGoesToGameView;
            InputSystem.settings.backgroundBehavior =
                InputSettings.BackgroundBehavior.IgnoreFocus;
            Mouse mouse = null;
            try
            {
                mouse = InputSystem.AddDevice<Mouse>("CityHeroPocketTestMouse");
                Assert.That(mouse.enabled, Is.True,
                    "The batch-mode test Mouse was disabled by focus/background policy.");
                PlayerRangedBasicAttackAction ranged =
                    RequireSingle<PlayerRangedBasicAttackAction>();
                PlayerActionController action = RequireSingle<PlayerActionController>();
                PlayerMovementController movement = RequireSingle<PlayerMovementController>();
                Component joystick = RequireSingle(ResolveHudType("CombatHudVirtualJoystick"));
                Component aimDrag = RequireSingle(ResolveHudType("CombatHudAimDragInput"));
                Component hudBinder = RequireSingle(
                    ResolveHudType("OneRowCombatHudBinder"));
                Camera camera = RequireSingle<Camera>();
                InputSystemUIInputModule inputModule =
                    RequireSingle<InputSystemUIInputModule>();
                Assert.That(inputModule.isActiveAndEnabled, Is.True,
                    "City InputSystemUIInputModule is not active and enabled.");
                Assert.That(EventSystem.current, Is.Not.Null);
                Assert.That(EventSystem.current.currentInputModule,
                    Is.SameAs(inputModule),
                    "City EventSystem is not processing through InputSystemUIInputModule.");
                Assert.That(inputModule.point, Is.Not.Null);
                Assert.That(inputModule.point.action, Is.Not.Null);
                Assert.That(inputModule.point.action.enabled, Is.True,
                    "City UI point action is disabled.");
                Assert.That(inputModule.leftClick, Is.Not.Null);
                Assert.That(inputModule.leftClick.action, Is.Not.Null);
                Assert.That(inputModule.leftClick.action.enabled, Is.True,
                    "City UI left-click action is disabled.");
                string moduleControlBindings =
                    $"pointControls='{DescribeActionControls(inputModule.point.action)}'; " +
                    $"leftClickControls='{DescribeActionControls(inputModule.leftClick.action)}'; " +
                    $"testMouse='{mouse.path}',enabled={mouse.enabled}";

                int firedCount = 0;
                int fireInputStartedCount = 0;
                int leftClickPerformedCount = 0;
                void ObserveFired(LaneActionProjectile _) => firedCount++;
                void ObserveFireInputStarted() => fireInputStartedCount++;
                void ObserveLeftClick(InputAction.CallbackContext _) =>
                    leftClickPerformedCount++;
                ranged.RangedProjectileFired += ObserveFired;
                ranged.RangedFireInputStarted += ObserveFireInputStarted;
                inputModule.leftClick.action.performed += ObserveLeftClick;
                RectTransform attackButton = RequireRectTransform("BasicAttackButton");
                Button attackVisualButton = attackButton.GetComponent<Button>();
                Assert.That(attackVisualButton, Is.Not.Null);
                Component attackPointer = attackButton.GetComponent(
                    ResolveHudType("CombatHudPointerActionInput"));
                Assert.That(attackPointer, Is.Not.Null,
                    "BasicAttackButton lost its pointer action component.");
                Assert.That(GetPublicProperty<object>(attackPointer, "ActionId").ToString(),
                    Is.EqualTo("BasicAttack"));
                Assert.That(GetPublicProperty<bool>(attackPointer, "SendsHoldState"), Is.True);
                Assert.That(GetPublicProperty<bool>(attackPointer, "IsInputBlocked"), Is.False);
                Assert.That(attackVisualButton.IsInteractable(), Is.True);
                Assert.That(GetPublicProperty<bool>(hudBinder, "IsCombatMenuInputLocked"), Is.False);
                Vector2 attackPoint = ScreenPoint(attackButton);
                string attackRaycast = RequireTopRaycastWithin(attackButton, attackPoint);
                var attackInputStates = new List<string>();
                yield return Click(mouse, attackPoint, inputModule, attackInputStates);
                ranged.RangedProjectileFired -= ObserveFired;
                ranged.RangedFireInputStarted -= ObserveFireInputStarted;
                inputModule.leftClick.action.performed -= ObserveLeftClick;
                string fireDiagnostic =
                    $"{attackRaycast}; pointerHeld=" +
                    $"{GetPublicProperty<bool>(attackPointer, "IsPointerHeld")}; " +
                    $"pointerBlocked={GetPublicProperty<bool>(attackPointer, "IsInputBlocked")}; " +
                    $"buttonInteractable={attackVisualButton.IsInteractable()}; " +
                    $"binderLocked={GetPublicProperty<bool>(hudBinder, "IsCombatMenuInputLocked")}; " +
                    $"fireInputStarted={fireInputStartedCount}; " +
                    $"leftClickPerformed={leftClickPerformedCount}; " +
                    $"lastBlockedReason='{ranged.LastUseBlockedReason}'; " +
                    $"{moduleControlBindings}; " +
                    $"queuedStates='{string.Join(" | ", attackInputStates)}'";
                Assert.That(fireInputStartedCount, Is.GreaterThan(0),
                    "HUD pointer reached no ranged fire-input request. " + fireDiagnostic);
                Assert.That(firedCount, Is.GreaterThan(0),
                    "InputSystemUIInputModule did not dispatch a real ranged projectile fire. " +
                    fireDiagnostic);
                Assert.That(ranged.ActiveProjectileCount, Is.GreaterThan(0),
                    "HUD fire did not activate a projectile from the prewarmed pool.");

                RectTransform dodgeButton = RequireRectTransform("DodgeButton");
                yield return Click(mouse, ScreenPoint(dodgeButton));
                Assert.That(action.IsDodging, Is.True,
                    "InputSystemUIInputModule did not dispatch Dodge pointer input.");
                while (action.IsDodging)
                {
                    yield return null;
                }

                Vector3 moveStart = movement.transform.position;
                RectTransform joystickRect = (RectTransform)joystick.transform;
                Vector2 joystickCenter = ScreenPoint(joystickRect);
                yield return Drag(
                    mouse,
                    joystickCenter,
                    joystickCenter + Vector2.up * Mathf.Max(80f, joystickRect.rect.height * 0.35f),
                    keepPressedForFrames: 5);
                Assert.That(Vector3.ProjectOnPlane(
                        movement.transform.position - moveStart,
                        Vector3.up).magnitude,
                    Is.GreaterThan(0.02f),
                    "InputSystemUIInputModule did not dispatch joystick drag movement.");

                Vector3 cameraForwardBefore = camera.transform.forward;
                Vector2 aimStart = new(Screen.width * 0.5f, Screen.height * 0.58f);
                yield return Drag(
                    mouse,
                    aimStart,
                    aimStart + Vector2.right * Mathf.Max(180f, Screen.width * 0.12f),
                    keepPressedForFrames: 4);
                yield return null;
                Assert.That(Vector3.Angle(cameraForwardBefore, camera.transform.forward),
                    Is.GreaterThan(0.25f),
                    "InputSystemUIInputModule did not dispatch AimDragArea look input.");
                Assert.That(GetPublicProperty<bool>(aimDrag, "IsPointerHeld"), Is.False,
                    "AimDragArea did not release its pointer lease.");

                Component overlay = RequireSingle(
                    ResolveHudType("CombatSessionOverlayPresenter"));
                RectTransform pauseButton = RequireRectTransform("PauseButton");
                yield return Click(mouse, ScreenPoint(pauseButton));
                Assert.That(GetPublicProperty<object>(overlay, "Mode").ToString(),
                    Is.EqualTo("Pause"),
                    "InputSystemUIInputModule did not dispatch Pause through the HUD bridge.");
                Assert.That(Time.timeScale, Is.EqualTo(0f).Within(0.0001f));
                Assert.That(GetPublicProperty<bool>(hudBinder, "IsCombatMenuInputLocked"), Is.True,
                    "Pause did not lock non-menu combat input.");
                InvokePublic(overlay, "Resume");
                yield return null;
                Assert.That(GetPublicProperty<object>(overlay, "Mode").ToString(),
                    Is.EqualTo("Hidden"));
                Assert.That(Time.timeScale, Is.GreaterThan(0f));
                Assert.That(GetPublicProperty<bool>(hudBinder, "IsCombatMenuInputLocked"), Is.False,
                    "Resume did not release the combat-input lock.");
            }
            finally
            {
                if (mouse != null && mouse.added)
                {
                    InputSystem.RemoveDevice(mouse);
                }
                InputSystem.settings.editorInputBehaviorInPlayMode =
                    previousEditorInputBehavior;
                InputSystem.settings.backgroundBehavior =
                    previousBackgroundBehavior;
            }
        }

        [UnityTest]
        [Timeout(20000)]
        public IEnumerator WonGatedExitUsesRealCharacterTriggerAndRestoresExactlyOnceState()
        {
            CityHeroPocketExitTransitionController transition =
                RequireSingle<CityHeroPocketExitTransitionController>();
            CombatEncounterController encounter = RequireSingle<CombatEncounterController>();
            CharacterController playerController = transition.PlayerController;
            Assert.That(transition.IsConfigured, Is.True);
            Assert.That(transition.Encounter, Is.SameAs(encounter));
            Assert.That(transition.PlayerController, Is.SameAs(playerController));
            Assert.That(transition.ExitTrigger.isTrigger, Is.True);
            Assert.That(transition.IsArmed, Is.False);
            Assert.That(
                transition.IgnoredLaneActionProjectileTriggerEnterCount,
                Is.Zero);
            Assert.That(transition.RejectedTriggerEnterCount, Is.Zero);

            yield return CrossExitTrigger(transition);
            Assert.That(transition.TriggerAcceptedCount, Is.Zero,
                "Crossing the real exit trigger before Won armed the product transition.");
            Assert.That(transition.TransitionStartedCount, Is.Zero);

            yield return PlacePlayerBeforeExit(transition);
            bool lethalDamageApplied = false;
            CombatRootAdmissionResult admission = encounter.AdmitCombatRoot(
                "city.exit-transition.playmode-proof",
                context =>
                {
                    lethalDamageApplied = context.TryApplyDamage(
                        encounter.EnemyHealth,
                        new DamageInfo(
                            encounter.PlayerHealth,
                            DamageTeam.Player,
                            encounter.EnemyHealth.MaxHealth * 2f,
                            encounter.EnemyHealth.transform.position,
                            Vector3.forward,
                            0f,
                            DamageResponsePolicy.DamageOnly,
                            CombatControlLockPolicy.None));
                });
            Assert.That(
                admission.Disposition,
                Is.EqualTo(CombatRootAdmissionDisposition.Executed));
            Assert.That(lethalDamageApplied, Is.True);
            Assert.That(encounter.IsWon, Is.True);
            Assert.That(transition.IsArmed, Is.True,
                "The configured real encounter Won event did not arm the exit.");

            int hudHiddenFrame = -1;
            int fullCoverFrame = -1;
            int exitReadyFrame = -1;
            transition.HudHidden += () => hudHiddenFrame = transition.PresentationFrame;
            transition.FullCover += () => fullCoverFrame = transition.PresentationFrame;
            transition.ExitReady += () => exitReadyFrame = transition.PresentationFrame;
            SetForeignCinematicCueLocks(transition, locked: true);

            yield return CrossExitTrigger(transition);
            Assert.That(transition.TriggerAcceptedCount, Is.EqualTo(1));
            Assert.That(transition.TransitionStartedCount, Is.EqualTo(1));
            Assert.That(transition.IsTransitionRunning, Is.True);
            Assert.That(transition.IsInputLocked, Is.True);
            Assert.That(transition.IsAiLocked, Is.True);
            Assert.That(transition.PlayerMovement.IsCinematicMoveInputLocked, Is.True);
            Assert.That(transition.PlayerAction.IsCinematicInputLocked, Is.True);
            Assert.That(transition.PlayerCombatMode.IsCinematicInputLocked, Is.True);
            Assert.That(transition.PlayerRangedAttack.IsCinematicInputLocked, Is.True);
            Assert.That(transition.EnemyAi.enabled, Is.False);
            Assert.That(transition.EnemyProjectileDriver.enabled, Is.False);

            int timeoutFrames = 360;
            while (!transition.IsExitReady && timeoutFrames-- > 0)
            {
                yield return null;
            }
            Assert.That(timeoutFrames, Is.GreaterThan(0),
                "City exit did not reach its fixed-frame opaque-cover terminal state.");
            Assert.That(transition.PresentationFrame,
                Is.EqualTo(CityHeroPocketExitTransitionController.ExitReadyFrame));
            Assert.That(hudHiddenFrame,
                Is.EqualTo(CityHeroPocketExitTransitionController.HudFadeFrameCount));
            Assert.That(fullCoverFrame,
                Is.EqualTo(CityHeroPocketExitTransitionController.ExitReadyFrame));
            Assert.That(exitReadyFrame,
                Is.EqualTo(CityHeroPocketExitTransitionController.ExitReadyFrame));
            Assert.That(transition.HudHiddenCount, Is.EqualTo(1));
            Assert.That(transition.FullCoverCount, Is.EqualTo(1));
            Assert.That(transition.ExitReadyCount, Is.EqualTo(1));
            Assert.That(
                transition.IgnoredLaneActionProjectileTriggerEnterCount,
                Is.Zero);
            Assert.That(transition.RejectedTriggerEnterCount, Is.Zero,
                "A non-player collider contaminated the reviewed exit run.");
            Assert.That(transition.TriggerAcceptedCount, Is.EqualTo(1));
            Assert.That(transition.TransitionStartedCount, Is.EqualTo(1));
            Assert.That(transition.IsTransitionRunning, Is.False);
            Assert.That(transition.IsHudHidden, Is.True);
            Assert.That(transition.IsFullCover, Is.True);
            Assert.That(transition.IsExitReady, Is.True);
            Assert.That(transition.HudCanvasGroup.alpha, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(transition.CoverCanvasGroup.alpha, Is.EqualTo(1f).Within(0.0001f));
            Assert.That((transition.PortalRoot.localScale
                    - transition.PortalAuthoredScale).sqrMagnitude,
                Is.LessThan(0.0001f));

            yield return PlacePlayerBeforeExit(transition);
            yield return CrossExitTrigger(transition);
            Assert.That(transition.TriggerAcceptedCount, Is.EqualTo(1),
                "A second trigger entry duplicated the completed transition.");
            Assert.That(transition.ExitReadyCount, Is.EqualTo(1));

            transition.ResetForRestart();
            Assert.That(transition.IsArmed, Is.False);
            Assert.That(transition.IsTransitionRunning, Is.False);
            Assert.That(transition.IsHudHidden, Is.False);
            Assert.That(transition.IsFullCover, Is.False);
            Assert.That(transition.IsExitReady, Is.False);
            Assert.That(transition.IsInputLocked, Is.False);
            Assert.That(transition.IsAiLocked, Is.False);
            Assert.That(transition.PresentationFrame, Is.Zero);
            Assert.That(
                transition.IgnoredLaneActionProjectileTriggerEnterCount,
                Is.Zero);
            Assert.That(transition.RejectedTriggerEnterCount, Is.Zero);
            Assert.That(transition.TriggerAcceptedCount, Is.Zero);
            Assert.That(transition.TransitionStartedCount, Is.Zero);
            Assert.That(transition.HudHiddenCount, Is.Zero);
            Assert.That(transition.FullCoverCount, Is.Zero);
            Assert.That(transition.ExitReadyCount, Is.Zero);
            Assert.That(transition.HudCanvasGroup.alpha, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(transition.HudCanvasGroup.interactable, Is.True);
            Assert.That(transition.HudCanvasGroup.blocksRaycasts, Is.True);
            Assert.That(transition.CoverCanvasGroup.alpha, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(transition.PortalRoot.gameObject.activeSelf, Is.False);
            Assert.That((transition.PortalRoot.localScale
                    - transition.PortalAuthoredScale
                        * CityHeroPocketExitTransitionController.InitialPortalScaleFactor)
                    .sqrMagnitude,
                Is.LessThan(0.0001f));
            Assert.That(transition.PlayerMovement.IsCinematicMoveInputLocked, Is.True,
                "City reset cleared another cinematic input-lock owner.");
            Assert.That(transition.PlayerAction.IsCinematicInputLocked, Is.True);
            Assert.That(transition.PlayerCombatMode.IsCinematicInputLocked, Is.True);
            Assert.That(transition.PlayerRangedAttack.IsCinematicInputLocked, Is.True);
            Assert.That(transition.EnemyAi.enabled, Is.True);
            Assert.That(transition.EnemyProjectileDriver.enabled, Is.True);

            SetForeignCinematicCueLocks(transition, locked: false);
            Assert.That(transition.PlayerMovement.IsCinematicMoveInputLocked, Is.False);
            Assert.That(transition.PlayerAction.IsCinematicInputLocked, Is.False);
            Assert.That(transition.PlayerCombatMode.IsCinematicInputLocked, Is.False);
            Assert.That(transition.PlayerRangedAttack.IsCinematicInputLocked, Is.False);

            yield return PlacePlayerBeforeExit(transition);
            yield return CrossExitTrigger(transition);
            Assert.That(transition.TriggerAcceptedCount, Is.Zero,
                "Reset inferred a new arm from stale Encounter.IsWon state.");
        }

        [UnityTest]
        [Timeout(15000)]
        public IEnumerator DisableAndDestroyRestoreOwnedTransitionStateIdempotently()
        {
            CityHeroPocketExitTransitionController transition =
                RequireSingle<CityHeroPocketExitTransitionController>();
            CombatEncounterController encounter = RequireSingle<CombatEncounterController>();
            CanvasGroup hudGroup = transition.HudCanvasGroup;
            yield return PlacePlayerBeforeExit(transition);

            bool lethalDamageApplied = false;
            CombatRootAdmissionResult admission = encounter.AdmitCombatRoot(
                "city.exit-transition.disable-proof",
                context => lethalDamageApplied = context.TryApplyDamage(
                    encounter.EnemyHealth,
                    new DamageInfo(
                        encounter.PlayerHealth,
                        DamageTeam.Player,
                        encounter.EnemyHealth.MaxHealth * 2f,
                        encounter.EnemyHealth.transform.position,
                        Vector3.forward,
                        0f,
                        DamageResponsePolicy.DamageOnly,
                        CombatControlLockPolicy.None)));
            Assert.That(admission.Disposition,
                Is.EqualTo(CombatRootAdmissionDisposition.Executed));
            Assert.That(lethalDamageApplied, Is.True);
            Assert.That(transition.IsArmed, Is.True);

            SetForeignCinematicCueLocks(transition, locked: true);
            yield return CrossExitTrigger(transition);
            for (int i = 0; i < 30; i++)
            {
                yield return null;
            }
            Assert.That(transition.IsTransitionRunning, Is.True);
            Assert.That(transition.HudCanvasGroup.alpha, Is.LessThan(1f));
            Assert.That(transition.PortalRoot.gameObject.activeSelf, Is.True);
            Assert.That(transition.IsInputLocked, Is.True);
            Assert.That(transition.IsAiLocked, Is.True);

            transition.enabled = false;
            yield return null;
            AssertTeardownStateRestored(transition);
            Assert.That(transition.PlayerMovement.IsCinematicMoveInputLocked, Is.True,
                "OnDisable cleared the independent CinematicCue owner.");
            Assert.That(transition.PlayerAction.IsCinematicInputLocked, Is.True);
            Assert.That(transition.PlayerCombatMode.IsCinematicInputLocked, Is.True);
            Assert.That(transition.PlayerRangedAttack.IsCinematicInputLocked, Is.True);

            transition.enabled = false;
            UnityEngine.Object.Destroy(transition);
            yield return null;
            Assert.That(transition == null, Is.True);
            Assert.That(RequireSingle<PlayerMovementController>().IsCinematicMoveInputLocked,
                Is.True,
                "Idempotent OnDestroy cleared the foreign lock after OnDisable restoration.");
            Assert.That(RequireSingle<BasicSoldierEnemy>().enabled, Is.True);
            Assert.That(RequireSingle<BasicSoldierProjectileAttackDriver>().enabled, Is.True);
            Assert.That(hudGroup.alpha, Is.EqualTo(1f).Within(0.0001f));

            PlayerMovementController movement = RequireSingle<PlayerMovementController>();
            PlayerActionController action = RequireSingle<PlayerActionController>();
            PlayerCombatModeController mode = RequireSingle<PlayerCombatModeController>();
            PlayerRangedBasicAttackAction ranged =
                RequireSingle<PlayerRangedBasicAttackAction>();
            movement.SetCinematicMoveInputLocked(PlayerInputLockSource.CinematicCue, false);
            action.SetCinematicInputLocked(PlayerInputLockSource.CinematicCue, false);
            mode.SetCinematicInputLocked(PlayerInputLockSource.CinematicCue, false);
            ranged.SetCinematicInputLocked(PlayerInputLockSource.CinematicCue, false);
            Assert.That(movement.IsCinematicMoveInputLocked, Is.False);
            Assert.That(action.IsCinematicInputLocked, Is.False);
            Assert.That(mode.IsCinematicInputLocked, Is.False);
            Assert.That(ranged.IsCinematicInputLocked, Is.False);
        }

        [UnityTest]
        [Timeout(10000)]
        public IEnumerator ActiveLaneActionProjectileEntryIsAuditedAndIgnored()
        {
            CityHeroPocketExitTransitionController transition =
                RequireSingle<CityHeroPocketExitTransitionController>();
            CombatEncounterController encounter = RequireSingle<CombatEncounterController>();
            yield return PlacePlayerBeforeExit(transition);

            bool lethalDamageApplied = false;
            CombatRootAdmissionResult admission = encounter.AdmitCombatRoot(
                "city.exit-transition.projectile-traffic-proof",
                context => lethalDamageApplied = context.TryApplyDamage(
                    encounter.EnemyHealth,
                    new DamageInfo(
                        encounter.PlayerHealth,
                        DamageTeam.Player,
                        encounter.EnemyHealth.MaxHealth * 2f,
                        encounter.EnemyHealth.transform.position,
                        Vector3.forward,
                        0f,
                        DamageResponsePolicy.DamageOnly,
                        CombatControlLockPolicy.None)));
            Assert.That(admission.Disposition,
                Is.EqualTo(CombatRootAdmissionDisposition.Executed));
            Assert.That(lethalDamageApplied, Is.True);
            Assert.That(transition.IsArmed, Is.True);

            int ignoredEventCount = 0;
            Collider ignoredCollider = null;
            LaneActionProjectile ignoredProjectile = null;
            int rejectedEventCount = 0;
            void HandleProjectileIgnored(
                Collider collider,
                LaneActionProjectile projectile)
            {
                ignoredEventCount++;
                ignoredCollider = collider;
                ignoredProjectile = projectile;
            }
            void HandleRejected(Collider _)
            {
                rejectedEventCount++;
            }
            transition.LaneActionProjectileTriggerEnterIgnored +=
                HandleProjectileIgnored;
            transition.TriggerEnterRejected += HandleRejected;

            GameObject projectileObject = new(
                "CityExitActiveLaneProjectileProof",
                typeof(SphereCollider),
                typeof(Rigidbody),
                typeof(LaneActionProjectile));
            SphereCollider projectileCollider =
                projectileObject.GetComponent<SphereCollider>();
            projectileCollider.radius = 0.15f;
            LaneActionProjectile projectile =
                projectileObject.GetComponent<LaneActionProjectile>();
            projectile.Configure(
                encounter.PlayerHealth,
                DamageTeam.Player,
                12f,
                Vector3.forward,
                0f,
                2f,
                projectileCollider.radius);

            Vector3 triggerCenter = transition.ExitTrigger.bounds.center;
            projectileObject.transform.position = triggerCenter
                - Vector3.forward * (transition.ExitTrigger.bounds.extents.z + 1f);
            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();

            projectileObject.transform.position = triggerCenter;
            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(
                transition.IgnoredLaneActionProjectileTriggerEnterCount,
                Is.EqualTo(1));
            Assert.That(ignoredEventCount, Is.EqualTo(1));
            Assert.That(ignoredCollider, Is.SameAs(projectileCollider));
            Assert.That(ignoredProjectile, Is.SameAs(projectile));
            Assert.That(transition.RejectedTriggerEnterCount, Is.Zero);
            Assert.That(rejectedEventCount, Is.Zero);
            Assert.That(transition.TriggerAcceptedCount, Is.Zero);
            Assert.That(transition.TransitionStartedCount, Is.Zero);
            Assert.That(transition.IsArmed, Is.True);
            Assert.That(transition.IsTransitionRunning, Is.False);
            Assert.That(transition.IsInputLocked, Is.False);
            Assert.That(transition.IsAiLocked, Is.False);
            Assert.That(transition.HudCanvasGroup.alpha, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(transition.CoverCanvasGroup.alpha, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(transition.PortalRoot.gameObject.activeSelf, Is.False);

            transition.ResetForRestart();
            Assert.That(
                transition.IgnoredLaneActionProjectileTriggerEnterCount,
                Is.Zero);
            Assert.That(transition.RejectedTriggerEnterCount, Is.Zero);
            Assert.That(ignoredEventCount, Is.EqualTo(1),
                "Reset must not synthesize an ignored projectile event.");
            Assert.That(rejectedEventCount, Is.Zero,
                "Reset must not synthesize a rejected trigger event.");

            transition.LaneActionProjectileTriggerEnterIgnored -=
                HandleProjectileIgnored;
            transition.TriggerEnterRejected -= HandleRejected;
            UnityEngine.Object.Destroy(projectileObject);
            yield return null;
        }

        [UnityTest]
        [Timeout(10000)]
        public IEnumerator RealWrongColliderEntryIsCountedButCannotChangeProductState()
        {
            CityHeroPocketExitTransitionController transition =
                RequireSingle<CityHeroPocketExitTransitionController>();
            Assert.That(
                transition.IgnoredLaneActionProjectileTriggerEnterCount,
                Is.Zero);
            Assert.That(transition.RejectedTriggerEnterCount, Is.Zero);
            Assert.That(transition.TriggerAcceptedCount, Is.Zero);
            Assert.That(transition.TransitionStartedCount, Is.Zero);
            Assert.That(transition.IsArmed, Is.False);

            int rejectedEventCount = 0;
            Collider rejectedCollider = null;
            void HandleTriggerEnterRejected(Collider collider)
            {
                rejectedEventCount++;
                rejectedCollider = collider;
            }
            transition.TriggerEnterRejected += HandleTriggerEnterRejected;

            GameObject wrongColliderObject = new(
                "CityExitWrongColliderProof",
                typeof(SphereCollider));
            SphereCollider wrongCollider =
                wrongColliderObject.GetComponent<SphereCollider>();
            wrongCollider.radius = 0.15f;
            Vector3 triggerCenter = transition.ExitTrigger.bounds.center;
            wrongColliderObject.transform.position = triggerCenter
                - Vector3.forward * (transition.ExitTrigger.bounds.extents.z + 1f);
            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();

            wrongColliderObject.transform.position = triggerCenter;
            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(transition.RejectedTriggerEnterCount, Is.EqualTo(1),
                "A real non-player collider entry was not observed by the exit trigger.");
            Assert.That(rejectedEventCount, Is.EqualTo(1));
            Assert.That(rejectedCollider, Is.SameAs(wrongCollider));
            Assert.That(
                transition.IgnoredLaneActionProjectileTriggerEnterCount,
                Is.Zero);
            Assert.That(transition.TriggerAcceptedCount, Is.Zero);
            Assert.That(transition.TransitionStartedCount, Is.Zero);
            Assert.That(transition.IsArmed, Is.False);
            Assert.That(transition.IsTransitionRunning, Is.False);
            Assert.That(transition.IsInputLocked, Is.False);
            Assert.That(transition.IsAiLocked, Is.False);
            Assert.That(transition.HudCanvasGroup.alpha, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(transition.CoverCanvasGroup.alpha, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(transition.PortalRoot.gameObject.activeSelf, Is.False);

            transition.ResetForRestart();
            Assert.That(
                transition.IgnoredLaneActionProjectileTriggerEnterCount,
                Is.Zero);
            Assert.That(transition.RejectedTriggerEnterCount, Is.Zero);
            Assert.That(transition.TriggerAcceptedCount, Is.Zero);
            Assert.That(transition.TransitionStartedCount, Is.Zero);
            Assert.That(rejectedEventCount, Is.EqualTo(1),
                "Reset must not synthesize a rejected trigger event.");

            transition.TriggerEnterRejected -= HandleTriggerEnterRejected;
            UnityEngine.Object.Destroy(wrongColliderObject);
            yield return null;
        }

        private static void AssertTeardownStateRestored(
            CityHeroPocketExitTransitionController transition)
        {
            Assert.That(transition.IsArmed, Is.False);
            Assert.That(transition.IsTransitionRunning, Is.False);
            Assert.That(transition.IsHudHidden, Is.False);
            Assert.That(transition.IsFullCover, Is.False);
            Assert.That(transition.IsExitReady, Is.False);
            Assert.That(transition.IsInputLocked, Is.False);
            Assert.That(transition.IsAiLocked, Is.False);
            Assert.That(transition.PresentationFrame, Is.Zero);
            Assert.That(
                transition.IgnoredLaneActionProjectileTriggerEnterCount,
                Is.Zero);
            Assert.That(transition.RejectedTriggerEnterCount, Is.Zero);
            Assert.That(transition.TriggerAcceptedCount, Is.Zero);
            Assert.That(transition.TransitionStartedCount, Is.Zero);
            Assert.That(transition.HudHiddenCount, Is.Zero);
            Assert.That(transition.FullCoverCount, Is.Zero);
            Assert.That(transition.ExitReadyCount, Is.Zero);
            Assert.That(transition.HudCanvasGroup.alpha, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(transition.HudCanvasGroup.interactable, Is.True);
            Assert.That(transition.HudCanvasGroup.blocksRaycasts, Is.True);
            Assert.That(transition.CoverCanvasGroup.alpha, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(transition.PortalRoot.gameObject.activeSelf, Is.False);
            Assert.That(transition.EnemyAi.enabled, Is.True);
            Assert.That(transition.EnemyProjectileDriver.enabled, Is.True);
        }

        private static void SetForeignCinematicCueLocks(
            CityHeroPocketExitTransitionController transition,
            bool locked)
        {
            transition.PlayerMovement.SetCinematicMoveInputLocked(
                PlayerInputLockSource.CinematicCue,
                locked);
            transition.PlayerAction.SetCinematicInputLocked(
                PlayerInputLockSource.CinematicCue,
                locked);
            transition.PlayerCombatMode.SetCinematicInputLocked(
                PlayerInputLockSource.CinematicCue,
                locked);
            transition.PlayerRangedAttack.SetCinematicInputLocked(
                PlayerInputLockSource.CinematicCue,
                locked);
        }

        private static IEnumerator PlacePlayerBeforeExit(
            CityHeroPocketExitTransitionController transition)
        {
            CharacterController controller = transition.PlayerController;
            bool wasEnabled = controller.enabled;
            controller.enabled = false;
            Vector3 triggerCenter = transition.ExitTrigger.bounds.center;
            controller.transform.position = new Vector3(
                triggerCenter.x,
                0f,
                transition.ExitTrigger.bounds.min.z - controller.radius - 0.35f);
            controller.enabled = wasEnabled;
            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();
        }

        private static IEnumerator CrossExitTrigger(
            CityHeroPocketExitTransitionController transition)
        {
            yield return PlacePlayerBeforeExit(transition);
            CharacterController controller = transition.PlayerController;
            float crossingDistance = transition.ExitTrigger.bounds.center.z
                - controller.bounds.center.z
                + 0.05f;
            controller.Move(Vector3.forward * crossingDistance);
            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
        }

        private static IEnumerator Click(
            Mouse mouse,
            Vector2 screenPosition,
            InputSystemUIInputModule inputModule = null,
            List<string> diagnostics = null)
        {
            InputSystem.QueueStateEvent(mouse, new MouseState { position = screenPosition });
            yield return null;
            diagnostics?.Add(DescribeInputState("move", mouse, inputModule));
            InputSystem.QueueStateEvent(
                mouse,
                new MouseState { position = screenPosition }.WithButton(MouseButton.Left));
            yield return null;
            diagnostics?.Add(DescribeInputState("press", mouse, inputModule));
            InputSystem.QueueStateEvent(mouse, new MouseState { position = screenPosition });
            yield return null;
            diagnostics?.Add(DescribeInputState("release", mouse, inputModule));
        }

        private static IEnumerator Drag(
            Mouse mouse,
            Vector2 start,
            Vector2 end,
            int keepPressedForFrames)
        {
            InputSystem.QueueStateEvent(mouse, new MouseState { position = start });
            yield return null;
            InputSystem.QueueStateEvent(
                mouse,
                new MouseState { position = start }.WithButton(MouseButton.Left));
            yield return null;
            InputSystem.QueueStateEvent(
                mouse,
                new MouseState { position = end, delta = end - start }
                    .WithButton(MouseButton.Left));
            for (int i = 0; i < keepPressedForFrames; i++)
            {
                yield return null;
            }
            InputSystem.QueueStateEvent(mouse, new MouseState { position = end });
            yield return null;
        }

        private static string DescribeInputState(
            string phase,
            Mouse mouse,
            InputSystemUIInputModule inputModule)
        {
            string pointAction = inputModule?.point?.action == null
                ? "<null>"
                : $"enabled={inputModule.point.action.enabled}," +
                    $"value={inputModule.point.action.ReadValue<Vector2>()}";
            string clickAction = inputModule?.leftClick?.action == null
                ? "<null>"
                : $"enabled={inputModule.leftClick.action.enabled}," +
                    $"pressed={inputModule.leftClick.action.IsPressed()}";
            return $"{phase}:mousePos={mouse.position.ReadValue()}," +
                $"mousePressed={mouse.leftButton.isPressed}," +
                $"point[{pointAction}],click[{clickAction}]";
        }

        private static string DescribeActionControls(InputAction action)
        {
            if (action == null)
            {
                return "<null>";
            }

            var controls = new List<string>();
            for (int i = 0; i < action.controls.Count; i++)
            {
                controls.Add(action.controls[i].path);
            }
            return controls.Count == 0 ? "<none>" : string.Join(",", controls);
        }

        private static RectTransform RequireRectTransform(string objectName)
        {
            Transform[] transforms = UnityEngine.Object.FindObjectsByType<Transform>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int i = 0; i < transforms.Length; i++)
            {
                if (transforms[i].name == objectName && transforms[i] is RectTransform rect)
                {
                    return rect;
                }
            }
            Assert.Fail($"City HUD is missing RectTransform '{objectName}'.");
            return null;
        }

        private static Vector2 ScreenPoint(RectTransform rect)
        {
            var corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            return (Vector2)((corners[0] + corners[2]) * 0.5f);
        }

        private static string RequireTopRaycastWithin(
            RectTransform expectedTarget,
            Vector2 screenPosition)
        {
            EventSystem eventSystem = EventSystem.current;
            Assert.That(eventSystem, Is.Not.Null,
                "City HUD requires one active EventSystem for pointer dispatch.");
            var eventData = new PointerEventData(eventSystem)
            {
                position = screenPosition,
                button = PointerEventData.InputButton.Left,
            };
            var hits = new List<RaycastResult>();
            eventSystem.RaycastAll(eventData, hits);
            string hitNames = hits.Count == 0
                ? "<none>"
                : string.Join(" | ", hits.ConvertAll(hit =>
                    BuildHierarchyPath(hit.gameObject.transform)));
            Assert.That(hits, Is.Not.Empty,
                $"HUD raycast at {screenPosition} returned no target.");
            Transform topHit = hits[0].gameObject.transform;
            Assert.That(topHit == expectedTarget || topHit.IsChildOf(expectedTarget), Is.True,
                $"HUD raycast at {screenPosition} hit '{hitNames}' instead of " +
                $"'{expectedTarget.name}' or one of its children.");
            GameObject pointerHandler =
                ExecuteEvents.GetEventHandler<IPointerDownHandler>(hits[0].gameObject);
            Assert.That(pointerHandler, Is.Not.Null,
                $"Top HUD raycast '{BuildHierarchyPath(topHit)}' has no pointer-down handler.");
            Assert.That(pointerHandler.transform == expectedTarget
                    || pointerHandler.transform.IsChildOf(expectedTarget),
                Is.True,
                $"Top HUD raycast routes pointer-down to " +
                $"'{BuildHierarchyPath(pointerHandler.transform)}', not '{expectedTarget.name}'.");
            return $"raycastTop='{BuildHierarchyPath(topHit)}'; " +
                $"pointerHandler='{BuildHierarchyPath(pointerHandler.transform)}'; " +
                $"allHits='{hitNames}'";
        }

        private static string BuildHierarchyPath(Transform transform)
        {
            if (transform == null)
            {
                return "<null>";
            }

            string path = transform.name;
            while (transform.parent != null)
            {
                transform = transform.parent;
                path = transform.name + "/" + path;
            }
            return path;
        }

        private static void RequireSafeViewportPoint(
            Camera camera,
            Vector3 worldPosition,
            string label)
        {
            Vector3 viewport = camera.WorldToViewportPoint(worldPosition);
            Assert.That(viewport.z, Is.GreaterThan(camera.nearClipPlane), $"{label} is behind camera.");
            Assert.That(viewport.x, Is.InRange(0.05f, 0.95f), $"{label} escaped horizontal safe frame.");
            Assert.That(viewport.y, Is.InRange(0.05f, 0.95f), $"{label} escaped vertical safe frame.");
        }

        private static void InvokePatternState(
            BasicSoldierProjectileAttackDriver driver,
            CombatAiPatternState state,
            CombatAiPatternProfile pattern)
        {
            MethodInfo method = typeof(BasicSoldierProjectileAttackDriver).GetMethod(
                "HandlePatternStateChanged",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);
            method.Invoke(driver, new object[] { state, pattern });
        }

        private static T RequireSingle<T>() where T : Component
        {
            T[] found = UnityEngine.Object.FindObjectsByType<T>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            Assert.That(found, Has.Length.EqualTo(1),
                $"City scene requires exactly one {typeof(T).Name}.");
            return found[0];
        }

        private static Type ResolveHudType(string typeName)
        {
            Type type = Type.GetType(
                $"DimensionBrawl.UI.{typeName}, Assembly-CSharp",
                throwOnError: true);
            Assert.That(type, Is.Not.Null);
            return type;
        }

        private static Component RequireSingle(Type expectedType)
        {
            MonoBehaviour[] behaviours = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            Component found = null;
            int count = 0;
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (!expectedType.IsInstanceOfType(behaviours[i]))
                {
                    continue;
                }
                found = behaviours[i];
                count++;
            }
            Assert.That(count, Is.EqualTo(1),
                $"City scene requires exactly one {expectedType.Name}; found {count}.");
            return found;
        }

        private static Component FindFirstOptional(Type expectedType)
        {
            if (expectedType == null)
            {
                return null;
            }

            MonoBehaviour[] behaviours = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (expectedType.IsInstanceOfType(behaviours[i]))
                {
                    return behaviours[i];
                }
            }
            return null;
        }

        private static T GetPublicProperty<T>(Component target, string propertyName)
        {
            PropertyInfo property = target.GetType().GetProperty(
                propertyName,
                BindingFlags.Instance | BindingFlags.Public);
            Assert.That(property, Is.Not.Null,
                $"{target.GetType().Name} is missing public property '{propertyName}'.");
            return (T)property.GetValue(target);
        }

        private static void InvokePublic(Component target, string methodName)
        {
            MethodInfo method = target.GetType().GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.Public);
            Assert.That(method, Is.Not.Null,
                $"{target.GetType().Name} is missing public method '{methodName}'.");
            method.Invoke(target, null);
        }

        private static Transform FindDescendant(Transform root, string name)
        {
            if (root == null)
            {
                return null;
            }
            if (root.name == name)
            {
                return root;
            }
            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindDescendant(root.GetChild(i), name);
                if (found != null)
                {
                    return found;
                }
            }
            return null;
        }
    }
}
