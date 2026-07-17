using System;
using System.Collections;
using System.Reflection;
using DimensionBrawl.Player;
using DimensionBrawl.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace DimensionBrawl.Tests
{
    public sealed class CombatPointerInputOwnershipPlayModeTests
    {
        private int requestCount;
        private int holdStartedCount;
        private int holdReleasedCount;

        [Test]
        public void InputLockOperationsRequireExactlyOneOwner()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                PlayerInputLockMask.WithState(
                    PlayerInputLockSource.None,
                    PlayerInputLockSource.None,
                    true));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                PlayerInputLockMask.WithState(
                    PlayerInputLockSource.None,
                    PlayerInputLockSource.CinematicCue | PlayerInputLockSource.CorridorTutorial,
                    true));
        }

        [UnityTest]
        public IEnumerator CombatPointerGateAllowsAimSurfaceAndBlocksInteractiveUi()
        {
            GameObject eventSystemObject = new("CombatPointerGateEventSystem", typeof(EventSystem));
            GameObject canvasObject = new(
                "CombatPointerGateCanvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            GameObject aimObject = new("AimDragArea", typeof(RectTransform), typeof(Image));
            aimObject.transform.SetParent(canvasObject.transform, false);
            RectTransform aimRect = aimObject.GetComponent<RectTransform>();
            aimRect.anchorMin = Vector2.zero;
            aimRect.anchorMax = Vector2.one;
            aimRect.offsetMin = Vector2.zero;
            aimRect.offsetMax = Vector2.zero;
            aimObject.GetComponent<Image>().color = Color.clear;
            aimObject.AddComponent(RequireType("DimensionBrawl.UI.CombatHudAimDragInput"));

            GameObject buttonObject = new(
                "InteractiveCombatButton",
                typeof(RectTransform),
                typeof(Image),
                typeof(Button));
            buttonObject.transform.SetParent(canvasObject.transform, false);
            RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
            buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
            buttonRect.sizeDelta = new Vector2(240f, 160f);

            Type gateType = RequireType("DimensionBrawl.Player.CombatPointerInputGate");
            MethodInfo gateMethod = gateType.GetMethod(
                "CanConsumeAtScreenPosition",
                BindingFlags.Static | BindingFlags.NonPublic);
            Assert.IsNotNull(gateMethod);
            Vector2 screenCenter = new(Screen.width * 0.5f, Screen.height * 0.5f);

            try
            {
                Canvas.ForceUpdateCanvases();
                yield return null;
                Assert.IsFalse(
                    (bool)gateMethod.Invoke(null, new object[] { screenCenter }),
                    "An interactive UI control must own the pointer instead of leaking an Attack action.");

                buttonObject.SetActive(false);
                Canvas.ForceUpdateCanvases();
                yield return null;
                Assert.IsTrue(
                    (bool)gateMethod.Invoke(null, new object[] { screenCenter }),
                    "The full-screen authored aim surface must allow the same LMB to drive combat fire.");
            }
            finally
            {
                Object.DestroyImmediate(buttonObject);
                Object.DestroyImmediate(aimObject);
                Object.DestroyImmediate(canvasObject);
                Object.DestroyImmediate(eventSystemObject);
            }

            yield return null;
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator PointerActionRequestsOnceAndKeepsItsOwningPointer()
        {
            GameObject eventSystemObject = new("PointerActionEventSystem", typeof(EventSystem));
            GameObject inputObject = new("PointerAction", typeof(RectTransform), typeof(Button));
            Type bridgeType = RequireType("DimensionBrawl.UI.CombatHudInputBridge");
            Type pointerActionType = RequireType("DimensionBrawl.UI.CombatHudPointerActionInput");
            Type actionIdType = RequireType("DimensionBrawl.UI.CombatHudActionId");
            Component inputBridge = inputObject.AddComponent(bridgeType);
            Component pointerAction = inputObject.AddComponent(pointerActionType);
            Button button = inputObject.GetComponent<Button>();
            object basicAttack = Enum.Parse(actionIdType, "BasicAttack");
            InvokePublic(pointerAction, "Configure", inputBridge, basicAttack, true);
            SubscribeGenericEvent(inputBridge, "ActionRequested", nameof(HandleActionRequested), actionIdType);
            SubscribeGenericEvent(inputBridge, "ActionHoldChanged", nameof(HandleActionHoldChanged), actionIdType);

            try
            {
                PointerEventData firstPointer = Pointer(11, PointerEventData.InputButton.Left);
                PointerEventData secondPointer = Pointer(12, PointerEventData.InputButton.Left);
                ((IPointerDownHandler)pointerAction).OnPointerDown(firstPointer);
                ((IPointerDownHandler)pointerAction).OnPointerDown(secondPointer);

                Assert.AreEqual(1, requestCount);
                Assert.AreEqual(1, holdStartedCount);
                Assert.IsTrue(GetPublicProperty<bool>(pointerAction, "IsPointerHeld"));

                ((IPointerUpHandler)pointerAction).OnPointerUp(secondPointer);
                Assert.IsTrue(
                    GetPublicProperty<bool>(pointerAction, "IsPointerHeld"),
                    "A different finger must not release the owning pointer.");
                Assert.AreEqual(0, holdReleasedCount);

                ((IPointerUpHandler)pointerAction).OnPointerUp(firstPointer);
                Assert.IsFalse(GetPublicProperty<bool>(pointerAction, "IsPointerHeld"));
                Assert.AreEqual(1, holdReleasedCount);

                ((IPointerDownHandler)pointerAction).OnPointerDown(
                    Pointer(-2, PointerEventData.InputButton.Right));
                Assert.AreEqual(1, requestCount, "Right-click must not trigger a combat action.");

                button.interactable = false;
                ((IPointerDownHandler)pointerAction).OnPointerDown(
                    Pointer(13, PointerEventData.InputButton.Left));
                Assert.AreEqual(1, requestCount, "A disabled visual button must disable its pointer route.");

                button.interactable = true;
                object stationEntryGuide = InputLockSource("StationEntryGuide");
                InvokePublic(pointerAction, "SetInputBlocked", stationEntryGuide, true);
                ((IPointerDownHandler)pointerAction).OnPointerDown(
                    Pointer(14, PointerEventData.InputButton.Left));
                Assert.AreEqual(1, requestCount);

                InvokePublic(pointerAction, "SetInputBlocked", stationEntryGuide, false);
                ((ISubmitHandler)pointerAction).OnSubmit(new BaseEventData(EventSystem.current));
                Assert.AreEqual(2, requestCount, "Submit should provide the only non-pointer action route.");
            }
            finally
            {
                Object.DestroyImmediate(inputObject);
                Object.DestroyImmediate(eventSystemObject);
            }

            yield return null;
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator CanonicalInputsKeepTheirPointerAndTutorialLockReleasesThem()
        {
            GameObject eventSystemObject = new("CanonicalInputEventSystem", typeof(EventSystem));
            GameObject aimObject = new("AimDrag", typeof(RectTransform));
            GameObject joystickObject = new("Joystick", typeof(RectTransform));
            GameObject actionObject = new("Action", typeof(RectTransform), typeof(Button));
            GameObject tutorialObject = new("TutorialBridge");
            Type aimType = RequireType("DimensionBrawl.UI.CombatHudAimDragInput");
            Type joystickType = RequireType("DimensionBrawl.UI.CombatHudVirtualJoystick");
            Type bridgeType = RequireType("DimensionBrawl.UI.CombatHudInputBridge");
            Type pointerActionType = RequireType("DimensionBrawl.UI.CombatHudPointerActionInput");
            Type actionIdType = RequireType("DimensionBrawl.UI.CombatHudActionId");
            Type tutorialType = RequireType("DimensionBrawl.LevelDesign.OlympusStationCombatIntroTutorialBridge");
            Component aimDrag = aimObject.AddComponent(aimType);
            Component joystick = joystickObject.AddComponent(joystickType);
            Component inputBridge = actionObject.AddComponent(bridgeType);
            Component pointerAction = actionObject.AddComponent(pointerActionType);
            Component tutorial = tutorialObject.AddComponent(tutorialType);
            InvokePublic(
                pointerAction,
                "Configure",
                inputBridge,
                Enum.Parse(actionIdType, "BasicAttack"),
                true);

            try
            {
                PointerEventData firstPointer = Pointer(21, PointerEventData.InputButton.Left);
                PointerEventData secondPointer = Pointer(22, PointerEventData.InputButton.Left);
                ((IPointerDownHandler)aimDrag).OnPointerDown(firstPointer);
                ((IPointerDownHandler)aimDrag).OnPointerDown(secondPointer);
                ((IPointerDownHandler)joystick).OnPointerDown(firstPointer);
                ((IPointerDownHandler)joystick).OnPointerDown(secondPointer);
                ((IPointerDownHandler)pointerAction).OnPointerDown(firstPointer);
                ((IPointerDownHandler)pointerAction).OnPointerDown(secondPointer);

                ((IPointerUpHandler)aimDrag).OnPointerUp(secondPointer);
                ((IPointerUpHandler)joystick).OnPointerUp(secondPointer);
                ((IPointerUpHandler)pointerAction).OnPointerUp(secondPointer);
                Assert.IsTrue(GetPublicProperty<bool>(aimDrag, "IsPointerHeld"));
                Assert.IsTrue(GetPublicProperty<bool>(joystick, "IsPointerHeld"));
                Assert.IsTrue(GetPublicProperty<bool>(pointerAction, "IsPointerHeld"));

                InvokePrivate(tutorial, "SetGameplayInputLocked", true);
                AssertInputBlockedAndReleased(aimDrag);
                AssertInputBlockedAndReleased(joystick);
                AssertInputBlockedAndReleased(pointerAction);

                InvokePrivate(tutorial, "SetGameplayInputLocked", false);
                Assert.IsFalse(GetPublicProperty<bool>(aimDrag, "IsInputBlocked"));
                Assert.IsFalse(GetPublicProperty<bool>(joystick, "IsInputBlocked"));
                Assert.IsFalse(GetPublicProperty<bool>(pointerAction, "IsInputBlocked"));
            }
            finally
            {
                Object.DestroyImmediate(tutorialObject);
                Object.DestroyImmediate(actionObject);
                Object.DestroyImmediate(joystickObject);
                Object.DestroyImmediate(aimObject);
                Object.DestroyImmediate(eventSystemObject);
            }

            yield return null;
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator OverlappingInputOwnersReleaseOnlyTheirOwnLocks()
        {
            GameObject eventSystemObject = new("OverlappingInputEventSystem", typeof(EventSystem));
            GameObject playerObject = new("OverlappingInputPlayer", typeof(CharacterController));
            GameObject aimObject = new("OverlappingAimDrag", typeof(RectTransform));
            GameObject joystickObject = new("OverlappingJoystick", typeof(RectTransform));
            GameObject actionObject = new("OverlappingAction", typeof(RectTransform));
            Component movement = playerObject.AddComponent(
                RequireType("DimensionBrawl.Player.PlayerMovementController"));
            Component aimDrag = aimObject.AddComponent(
                RequireType("DimensionBrawl.UI.CombatHudAimDragInput"));
            Component joystick = joystickObject.AddComponent(
                RequireType("DimensionBrawl.UI.CombatHudVirtualJoystick"));
            Component pointerAction = actionObject.AddComponent(
                RequireType("DimensionBrawl.UI.CombatHudPointerActionInput"));
            object stationEntryGuide = InputLockSource("StationEntryGuide");
            object corridorTutorial = InputLockSource("CorridorTutorial");
            object editorVerification = InputLockSource("EditorVerification");

            try
            {
                InvokePublic(joystick, "Configure", movement, null);
                InvokePublic(joystick, "SetInputBlocked", stationEntryGuide, true);
                InvokePublic(joystick, "SetInputBlocked", corridorTutorial, true);

                PointerEventData pointer = Pointer(51, PointerEventData.InputButton.Left);
                ((IPointerDownHandler)joystick).OnPointerDown(pointer);
                InvokePublic(joystick, "SetInputBlocked", stationEntryGuide, false);
                Assert.IsTrue(GetPublicProperty<bool>(joystick, "IsInputBlocked"));
                Assert.IsFalse(GetPublicProperty<bool>(joystick, "IsPointerHeld"));

                InvokePublic(joystick, "SetInputBlocked", corridorTutorial, false);
                Assert.IsFalse(GetPublicProperty<bool>(joystick, "IsInputBlocked"));
                Assert.IsTrue(
                    GetPublicProperty<bool>(joystick, "IsPointerHeld"),
                    "A deferred touch should activate only after the final lock owner releases it.");

                Component[] hudInputs = { aimDrag, pointerAction };
                for (int i = 0; i < hudInputs.Length; i++)
                {
                    InvokePublic(hudInputs[i], "SetInputBlocked", stationEntryGuide, true);
                    InvokePublic(hudInputs[i], "SetInputBlocked", corridorTutorial, true);
                    InvokePublic(hudInputs[i], "SetInputBlocked", stationEntryGuide, false);
                    Assert.IsTrue(GetPublicProperty<bool>(hudInputs[i], "IsInputBlocked"));
                    InvokePublic(hudInputs[i], "SetInputBlocked", corridorTutorial, false);
                    Assert.IsFalse(GetPublicProperty<bool>(hudInputs[i], "IsInputBlocked"));
                }

                InvokePublic(movement, "SetCinematicMoveInputLocked", stationEntryGuide, true);
                InvokePublic(movement, "SetCinematicMoveInputLocked", corridorTutorial, true);
                InvokePublic(movement, "SetCinematicMoveInputLocked", stationEntryGuide, false);
                Assert.IsTrue(GetPublicProperty<bool>(movement, "IsCinematicMoveInputLocked"));
                InvokePublic(movement, "SetCinematicMoveInputLocked", corridorTutorial, false);
                Assert.IsFalse(GetPublicProperty<bool>(movement, "IsCinematicMoveInputLocked"));

                InvokePublic(movement, "SetSharedMoveInputBlocked", editorVerification, true);
                InvokePublic(movement, "SetSharedMoveInputBlocked", stationEntryGuide, true);
                InvokePublic(movement, "SetSharedMoveInputBlocked", editorVerification, false);
                Assert.IsTrue(GetPublicProperty<bool>(movement, "IsSharedMoveInputBlocked"));
                InvokePublic(movement, "SetSharedMoveInputBlocked", stationEntryGuide, false);
                Assert.IsFalse(GetPublicProperty<bool>(movement, "IsSharedMoveInputBlocked"));
            }
            finally
            {
                Object.DestroyImmediate(actionObject);
                Object.DestroyImmediate(joystickObject);
                Object.DestroyImmediate(aimObject);
                Object.DestroyImmediate(playerObject);
                Object.DestroyImmediate(eventSystemObject);
            }

            yield return null;
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator CombatActionLocksReleaseOnlyTheirOwnSource()
        {
            string[] typeNames =
            {
                "DimensionBrawl.Player.PlayerActionController",
                "DimensionBrawl.Player.PlayerCombatModeController",
                "DimensionBrawl.Player.PlayerRangedBasicAttackAction",
                "DimensionBrawl.Player.PlayerSkill1Action",
                "DimensionBrawl.Player.PlayerSummonSlot1Action",
                "DimensionBrawl.Player.PlayerSupportSummonSlotAction"
            };
            object stationEntryGuide = InputLockSource("StationEntryGuide");
            object cinematicCue = InputLockSource("CinematicCue");
            object corridorTutorial = InputLockSource("CorridorTutorial");
            GameObject root = new("OverlappingCombatActionLocks");
            root.SetActive(false);

            try
            {
                Component rangedBasicAttack = null;
                for (int i = 0; i < typeNames.Length; i++)
                {
                    Component input = root.AddComponent(RequireType(typeNames[i]));
                    InvokePublic(input, "SetCinematicInputLocked", stationEntryGuide, true);
                    InvokePublic(input, "SetCinematicInputLocked", cinematicCue, true);
                    InvokePublic(input, "SetCinematicInputLocked", stationEntryGuide, false);
                    Assert.IsTrue(
                        GetPublicProperty<bool>(input, "IsCinematicInputLocked"),
                        $"{input.GetType().Name} released another owner's lock.");
                    InvokePublic(input, "SetCinematicInputLocked", cinematicCue, false);
                    Assert.IsFalse(GetPublicProperty<bool>(input, "IsCinematicInputLocked"));

                    if (input.GetType().Name == "PlayerRangedBasicAttackAction")
                    {
                        rangedBasicAttack = input;
                    }
                }

                Assert.IsNotNull(rangedBasicAttack);
                SetPrivateField(rangedBasicAttack, "mobileFireHeld", true);
                SetPrivateField(rangedBasicAttack, "currentFireHeld", true);
                InvokePublic(
                    rangedBasicAttack,
                    "SetCinematicInputLocked",
                    corridorTutorial,
                    true,
                    true);
                Assert.IsTrue(
                    GetPrivateField<bool>(rangedBasicAttack, "preserveHeldAimWhileCinematicLocked"));

                InvokePublic(
                    rangedBasicAttack,
                    "SetCinematicInputLocked",
                    cinematicCue,
                    true,
                    false);
                Assert.IsFalse(
                    GetPrivateField<bool>(rangedBasicAttack, "preserveHeldAimWhileCinematicLocked"),
                    "One non-preserving owner must suppress held aim while locks overlap.");
                Assert.IsTrue(GetPublicProperty<bool>(rangedBasicAttack, "IsCinematicInputLocked"));

                InvokePublic(rangedBasicAttack, "SetCinematicInputLocked", cinematicCue, false);
                Assert.IsTrue(GetPublicProperty<bool>(rangedBasicAttack, "IsCinematicInputLocked"));
                InvokePublic(rangedBasicAttack, "SetCinematicInputLocked", corridorTutorial, false);
                Assert.IsFalse(GetPublicProperty<bool>(rangedBasicAttack, "IsCinematicInputLocked"));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }

            yield return null;
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator FlowEnableChecksDoNotEraseLiveMobileMovement()
        {
            GameObject playerObject = new("FlowMovementPlayer", typeof(CharacterController));
            GameObject flowObject = new("FlowMovementController");
            Component movement = playerObject.AddComponent(
                RequireType("DimensionBrawl.Player.PlayerMovementController"));
            Component flow = flowObject.AddComponent(
                RequireType("DimensionBrawl.LevelDesign.OlympusCorridorCombatFlowController"));
            SetPrivateField(flow, "player", movement);

            try
            {
                InvokePublic(movement, "SetMoveInput", Vector2.right);
                InvokePrivate(flow, "EnsurePlayerMovementEnabled");
                InvokePrivate(flow, "EnsurePlayerMovementEnabled");
                Assert.That(
                    GetPrivateField<Vector2>(movement, "mobileMoveInput"),
                    Is.EqualTo(Vector2.right),
                    "A phase poll may enable movement components but must not overwrite a live joystick value.");

                InvokePrivate(flow, "ClearPlayerInputForPhaseTransition");
                Assert.That(
                    GetPrivateField<Vector2>(movement, "mobileMoveInput"),
                    Is.EqualTo(Vector2.zero),
                    "Only an explicit phase transition may clear the shared movement value.");
            }
            finally
            {
                Object.DestroyImmediate(flowObject);
                Object.DestroyImmediate(playerObject);
            }

            yield return null;
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator CorridorTutorialCompletionReleasesMovementAndJoystickLocks()
        {
            GameObject eventSystemObject = new("TutorialCompletionEventSystem", typeof(EventSystem));
            Type movementType = RequireType("DimensionBrawl.Player.PlayerMovementController");
            Type joystickType = RequireType("DimensionBrawl.UI.CombatHudVirtualJoystick");
            Type tutorialType = RequireType("DimensionBrawl.LevelDesign.OlympusCorridorTutorialDirector");
            GameObject playerObject = new("TutorialCompletionPlayer", typeof(CharacterController));
            GameObject joystickObject = new("TutorialCompletionJoystick", typeof(RectTransform));
            GameObject tutorialObject = new("TutorialCompletionDirector");
            Component movement = playerObject.AddComponent(movementType);
            Component joystick = joystickObject.AddComponent(joystickType);
            Component tutorial = tutorialObject.AddComponent(tutorialType);
            InvokePublic(joystick, "Configure", movement, null);
            SetPrivateField(tutorial, "player", movement);
            SetPrivateField(tutorial, "moveInputGateBehaviour", joystick);

            try
            {
                ((IPointerDownHandler)joystick).OnPointerDown(
                    Pointer(41, PointerEventData.InputButton.Left));
                Assert.IsTrue(GetPublicProperty<bool>(joystick, "IsPointerHeld"));

                InvokePrivate(tutorial, "SetMovementInputLocked", true);
                Assert.IsTrue(GetPublicProperty<bool>(movement, "IsCinematicMoveInputLocked"));
                Assert.IsTrue(GetPublicProperty<bool>(joystick, "IsInputBlocked"));
                Assert.IsFalse(GetPublicProperty<bool>(joystick, "IsPointerHeld"));

                InvokePrivate(tutorial, "CompleteTutorial");
                Assert.IsFalse(GetPublicProperty<bool>(movement, "IsCinematicMoveInputLocked"));
                Assert.IsFalse(GetPublicProperty<bool>(joystick, "IsInputBlocked"));
            }
            finally
            {
                Object.DestroyImmediate(tutorialObject);
                Object.DestroyImmediate(joystickObject);
                Object.DestroyImmediate(playerObject);
                Object.DestroyImmediate(eventSystemObject);
            }

            yield return null;
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator JoystickCarriesBlockedTutorialPressIntoActionWindow()
        {
            GameObject eventSystemObject = new("DeferredJoystickEventSystem", typeof(EventSystem));
            Type movementType = RequireType("DimensionBrawl.Player.PlayerMovementController");
            Type joystickType = RequireType("DimensionBrawl.UI.CombatHudVirtualJoystick");
            Type tutorialType = RequireType("DimensionBrawl.LevelDesign.OlympusCorridorTutorialDirector");
            GameObject playerObject = new("DeferredJoystickPlayer", typeof(CharacterController));
            GameObject joystickObject = new("DeferredJoystick", typeof(RectTransform));
            GameObject tutorialObject = new("DeferredJoystickTutorial");
            Component movement = playerObject.AddComponent(movementType);
            Component joystick = joystickObject.AddComponent(joystickType);
            Component tutorial = tutorialObject.AddComponent(tutorialType);
            InvokePublic(joystick, "Configure", movement, null);
            SetPrivateField(tutorial, "player", movement);
            SetPrivateField(tutorial, "moveInputGateBehaviour", joystick);

            try
            {
                InvokePrivate(tutorial, "SetMovementInputLocked", true);
                Assert.IsTrue(GetPublicProperty<bool>(movement, "IsCinematicMoveInputLocked"));
                PointerEventData pointer = Pointer(42, PointerEventData.InputButton.Left);
                pointer.position = Vector2.zero;
                ((IPointerDownHandler)joystick).OnPointerDown(pointer);

                pointer.position = new Vector2(80f, 0f);
                ((IDragHandler)joystick).OnDrag(pointer);
                Assert.IsFalse(GetPublicProperty<bool>(joystick, "IsPointerHeld"));
                Assert.AreEqual(Vector2.zero, GetPublicProperty<Vector2>(joystick, "CurrentInput"));

                InvokePrivate(tutorial, "SetMovementInputLocked", false);
                Assert.IsFalse(GetPublicProperty<bool>(movement, "IsCinematicMoveInputLocked"));
                Assert.IsTrue(
                    GetPublicProperty<bool>(joystick, "IsPointerHeld"),
                    "A pointer that began on the stick during the tutorial cue should be adopted when movement opens.");
                Assert.That(
                    GetPublicProperty<Vector2>(joystick, "CurrentInput").sqrMagnitude,
                    Is.GreaterThan(0.1f),
                    "The latest blocked drag position should become live without requiring a second touch.");
                Assert.That(
                    GetPrivateField<Vector2>(movement, "mobileMoveInput").sqrMagnitude,
                    Is.GreaterThan(0.1f),
                    "Unlocking must release the player before the joystick replays its held value.");

                ((IPointerUpHandler)joystick).OnPointerUp(pointer);
                Assert.IsFalse(GetPublicProperty<bool>(joystick, "IsPointerHeld"));
                Assert.AreEqual(Vector2.zero, GetPublicProperty<Vector2>(joystick, "CurrentInput"));
            }
            finally
            {
                Object.DestroyImmediate(tutorialObject);
                Object.DestroyImmediate(joystickObject);
                Object.DestroyImmediate(playerObject);
                Object.DestroyImmediate(eventSystemObject);
            }

            yield return null;
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator LookPeekUsesBoundedCameraOrbitWithoutAimModifier()
        {
            GameObject cameraObject = new("LookPeekCamera", typeof(Camera), typeof(ActionCameraController));
            ActionCameraController cameraController = cameraObject.GetComponent<ActionCameraController>();

            try
            {
                cameraController.SetLookPeekInput(Vector2.right);
                InvokePrivate(cameraController, "UpdateLookPeekOrbitOffsets", 1f);

                Assert.Greater(
                    cameraController.LookPeekYawOffsetDegrees,
                    0.1f,
                    "Empty-screen drag should rotate the camera without enabling aim.");
                Assert.LessOrEqual(cameraController.LookPeekYawOffsetDegrees, 45f);
                Assert.AreEqual(Vector2.zero, cameraController.AimOrbitInput);
                Assert.AreEqual(0f, cameraController.AimWeight, 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(cameraObject);
            }

            yield return null;
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator LobbyDragEndsWithoutTapWhenDisabled()
        {
            GameObject eventSystemObject = new("LobbyInputEventSystem", typeof(EventSystem));
            GameObject bridgeObject = new("LobbyStageInput", typeof(RectTransform), typeof(Image));
            Type bridgeType = RequireType("DimensionBrawl.UI.LobbyCharacterStageInputBridge");
            Type channelType = RequireType("DimensionBrawl.UI.LobbyCharacterStageInputChannel");
            Component bridge = bridgeObject.AddComponent(bridgeType);
            ScriptableObject channel = ScriptableObject.CreateInstance(channelType);
            SetPrivateField(bridge, "inputChannel", channel);
            int beginCount = 0;
            int endCount = 0;
            int tapCount = 0;
            channelType.GetEvent("BeginInteractionRequested")?.AddEventHandler(channel, (Action)(() => beginCount++));
            channelType.GetEvent("EndInteractionRequested")?.AddEventHandler(channel, (Action)(() => endCount++));
            channelType.GetEvent("TapRequested")?.AddEventHandler(channel, (Action)(() => tapCount++));

            try
            {
                ((IPointerDownHandler)bridge).OnPointerDown(Pointer(31, PointerEventData.InputButton.Left));
                ((IPointerDownHandler)bridge).OnPointerDown(Pointer(32, PointerEventData.InputButton.Left));
                ((IPointerUpHandler)bridge).OnPointerUp(Pointer(32, PointerEventData.InputButton.Left));
                Assert.AreEqual(1, beginCount);
                Assert.AreEqual(0, endCount);

                ((Behaviour)bridge).enabled = false;
                Assert.AreEqual(1, endCount);
                Assert.AreEqual(0, tapCount, "Lifecycle cancellation must not be interpreted as a tap.");
            }
            finally
            {
                Object.DestroyImmediate(channel);
                Object.DestroyImmediate(bridgeObject);
                Object.DestroyImmediate(eventSystemObject);
            }

            yield return null;
            LogAssert.NoUnexpectedReceived();
        }

        private void HandleActionRequested<T>(T actionId)
        {
            requestCount++;
        }

        private void HandleActionHoldChanged<T>(T actionId, bool held)
        {
            if (held)
            {
                holdStartedCount++;
            }
            else
            {
                holdReleasedCount++;
            }
        }

        private void SubscribeGenericEvent(
            Component target,
            string eventName,
            string handlerMethodName,
            Type genericArgument)
        {
            EventInfo eventInfo = target.GetType().GetEvent(eventName, BindingFlags.Instance | BindingFlags.Public);
            Assert.IsNotNull(eventInfo);
            MethodInfo method = GetType().GetMethod(
                handlerMethodName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method);
            Delegate handler = Delegate.CreateDelegate(
                eventInfo.EventHandlerType,
                this,
                method.MakeGenericMethod(genericArgument));
            eventInfo.AddEventHandler(target, handler);
        }

        private static void AssertInputBlockedAndReleased(Component input)
        {
            Assert.IsTrue(GetPublicProperty<bool>(input, "IsInputBlocked"));
            Assert.IsFalse(GetPublicProperty<bool>(input, "IsPointerHeld"));
        }

        private static PointerEventData Pointer(int pointerId, PointerEventData.InputButton button)
        {
            return new PointerEventData(EventSystem.current)
            {
                pointerId = pointerId,
                button = button,
                position = new Vector2(200f, 160f),
                pressPosition = new Vector2(200f, 160f)
            };
        }

        private static Type RequireType(string fullName)
        {
            Type type = Type.GetType($"{fullName}, Assembly-CSharp", throwOnError: false)
                ?? Type.GetType($"{fullName}, DimensionBrawl.Runtime", throwOnError: false);
            Assert.IsNotNull(type, $"Missing runtime type {fullName}.");
            return type;
        }

        private static object InputLockSource(string name)
        {
            return Enum.Parse(RequireType("DimensionBrawl.Player.PlayerInputLockSource"), name);
        }

        private static object InvokePublic(object target, string methodName, params object[] arguments)
        {
            MethodInfo method = FindCompatibleMethod(
                target.GetType(),
                methodName,
                BindingFlags.Instance | BindingFlags.Public,
                arguments);
            Assert.IsNotNull(method, $"Missing public method {methodName} on {target.GetType().Name}.");
            return method.Invoke(target, arguments);
        }

        private static MethodInfo FindCompatibleMethod(
            Type type,
            string methodName,
            BindingFlags bindingFlags,
            object[] arguments)
        {
            MethodInfo[] methods = type.GetMethods(bindingFlags);
            for (int methodIndex = 0; methodIndex < methods.Length; methodIndex++)
            {
                MethodInfo candidate = methods[methodIndex];
                if (candidate.Name != methodName)
                {
                    continue;
                }

                ParameterInfo[] parameters = candidate.GetParameters();
                if (parameters.Length != arguments.Length)
                {
                    continue;
                }

                bool compatible = true;
                for (int argumentIndex = 0; argumentIndex < arguments.Length; argumentIndex++)
                {
                    object argument = arguments[argumentIndex];
                    if (argument != null && !parameters[argumentIndex].ParameterType.IsInstanceOfType(argument))
                    {
                        compatible = false;
                        break;
                    }
                }

                if (compatible)
                {
                    return candidate;
                }
            }

            return null;
        }

        private static object InvokePrivate(object target, string methodName, params object[] arguments)
        {
            MethodInfo method = target.GetType().GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method, $"Missing private method {methodName} on {target.GetType().Name}.");
            return method.Invoke(target, arguments);
        }

        private static T GetPublicProperty<T>(object target, string propertyName)
        {
            PropertyInfo property = target.GetType().GetProperty(
                propertyName,
                BindingFlags.Instance | BindingFlags.Public);
            Assert.IsNotNull(property, $"Missing public property {propertyName} on {target.GetType().Name}.");
            return (T)property.GetValue(target);
        }

        private static T GetPrivateField<T>(object target, string fieldName)
        {
            FieldInfo field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Missing private field {fieldName} on {target.GetType().Name}.");
            return (T)field.GetValue(target);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Missing private field {fieldName} on {target.GetType().Name}.");
            field.SetValue(target, value);
        }
    }
}
