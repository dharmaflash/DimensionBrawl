using System;
using System.Collections;
using System.Reflection;
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
                InvokePublic(pointerAction, "SetInputBlocked", true);
                ((IPointerDownHandler)pointerAction).OnPointerDown(
                    Pointer(14, PointerEventData.InputButton.Left));
                Assert.AreEqual(1, requestCount);

                InvokePublic(pointerAction, "SetInputBlocked", false);
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
        public IEnumerator CanonicalInputsKeepTheirPointerAndExplicitBlockReleasesThem()
        {
            GameObject eventSystemObject = new("CanonicalInputEventSystem", typeof(EventSystem));
            GameObject aimObject = new("AimDrag", typeof(RectTransform));
            GameObject joystickObject = new("Joystick", typeof(RectTransform));
            GameObject actionObject = new("Action", typeof(RectTransform), typeof(Button));
            Type aimType = RequireType("DimensionBrawl.UI.CombatHudAimDragInput");
            Type joystickType = RequireType("DimensionBrawl.UI.CombatHudVirtualJoystick");
            Type bridgeType = RequireType("DimensionBrawl.UI.CombatHudInputBridge");
            Type pointerActionType = RequireType("DimensionBrawl.UI.CombatHudPointerActionInput");
            Type actionIdType = RequireType("DimensionBrawl.UI.CombatHudActionId");
            Component aimDrag = aimObject.AddComponent(aimType);
            Component joystick = joystickObject.AddComponent(joystickType);
            Component inputBridge = actionObject.AddComponent(bridgeType);
            Component pointerAction = actionObject.AddComponent(pointerActionType);
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

                InvokePublic(aimDrag, "SetInputBlocked", true);
                InvokePublic(joystick, "SetInputBlocked", true);
                InvokePublic(pointerAction, "SetInputBlocked", true);
                AssertInputBlockedAndReleased(aimDrag);
                AssertInputBlockedAndReleased(joystick);
                AssertInputBlockedAndReleased(pointerAction);

                InvokePublic(aimDrag, "SetInputBlocked", false);
                InvokePublic(joystick, "SetInputBlocked", false);
                InvokePublic(pointerAction, "SetInputBlocked", false);
                Assert.IsFalse(GetPublicProperty<bool>(aimDrag, "IsInputBlocked"));
                Assert.IsFalse(GetPublicProperty<bool>(joystick, "IsInputBlocked"));
                Assert.IsFalse(GetPublicProperty<bool>(pointerAction, "IsInputBlocked"));
            }
            finally
            {
                Object.DestroyImmediate(actionObject);
                Object.DestroyImmediate(joystickObject);
                Object.DestroyImmediate(aimObject);
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
            Type type = Type.GetType($"{fullName}, Assembly-CSharp", throwOnError: false);
            Assert.IsNotNull(type, $"Missing runtime type {fullName}.");
            return type;
        }

        private static object InvokePublic(object target, string methodName, params object[] arguments)
        {
            MethodInfo method = target.GetType().GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.Public);
            Assert.IsNotNull(method, $"Missing public method {methodName} on {target.GetType().Name}.");
            return method.Invoke(target, arguments);
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
