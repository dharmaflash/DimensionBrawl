using System.Collections;
using System.Reflection;
using DimensionBrawl.Player;
using DimensionBrawl.Presentation;
using DimensionBrawl.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;

namespace DimensionBrawl.Tests
{
    public sealed class CombatHudAimDragInputPlayModeTests
    {
        [UnityTest]
        public IEnumerator DisableIgnoresDestroyedPlayerBindings()
        {
            GameObject movementObject = new("DestroyedMovementBinding", typeof(CharacterController));
            PlayerMovementController movementController = movementObject.AddComponent<PlayerMovementController>();
            GameObject inputObject = new("CombatHudAimDragInput", typeof(RectTransform));
            System.Type aimDragInputType = System.Type.GetType(
                "DimensionBrawl.UI.CombatHudAimDragInput, Assembly-CSharp",
                throwOnError: true);
            Component aimDragInput = inputObject.AddComponent(aimDragInputType);
            MethodInfo configure = aimDragInputType.GetMethod("Configure", BindingFlags.Instance | BindingFlags.Public);
            Assert.IsNotNull(configure);
            configure.Invoke(aimDragInput, new object[] { movementController, null, null, null });

            Object.Destroy(movementObject);
            yield return null;

            Assert.DoesNotThrow(() => inputObject.SetActive(false));
            LogAssert.NoUnexpectedReceived();

            Object.Destroy(inputObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator KeyboardPeekReactsToPreviewAndCombatModeEventsWithoutPolling()
        {
            GameObject playerObject = new("EventDrivenKeyboardPeekPlayer");
            PlayerCombatModeController combatModeController =
                playerObject.AddComponent<PlayerCombatModeController>();
            PlayerRangedAimController aimController =
                playerObject.AddComponent<PlayerRangedAimController>();
            PlayerRangedBasicAttackAction rangedAction =
                playerObject.AddComponent<PlayerRangedBasicAttackAction>();
            GameObject inputObject = new("EventDrivenCombatHudAimDragInput", typeof(RectTransform));
            System.Type aimDragInputType = System.Type.GetType(
                "DimensionBrawl.UI.CombatHudAimDragInput, Assembly-CSharp",
                throwOnError: true);
            Component aimDragInput = inputObject.AddComponent(aimDragInputType);

            try
            {
                MethodInfo configure = aimDragInputType.GetMethod(
                    "Configure",
                    BindingFlags.Instance | BindingFlags.Public);
                MethodInfo refreshKeyboardAim = aimDragInputType.GetMethod(
                    "RefreshKeyboardAim",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                FieldInfo keyboardPeekInput = aimDragInputType.GetField(
                    "keyboardPeekInput",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                PropertyInfo currentAimInput = aimDragInputType.GetProperty(
                    "CurrentAimInput",
                    BindingFlags.Instance | BindingFlags.Public);
                FieldInfo holdFireActivatesAim = typeof(PlayerRangedBasicAttackAction).GetField(
                    "holdFireActivatesAim",
                    BindingFlags.Instance | BindingFlags.NonPublic);

                Assert.IsNotNull(configure);
                Assert.IsNotNull(refreshKeyboardAim);
                Assert.IsNotNull(keyboardPeekInput);
                Assert.IsNotNull(currentAimInput);
                Assert.IsNotNull(holdFireActivatesAim);

                holdFireActivatesAim.SetValue(rangedAction, false);
                configure.Invoke(
                    aimDragInput,
                    new object[]
                    {
                        null,
                        combatModeController,
                        aimController,
                        rangedAction
                    });
                keyboardPeekInput.SetValue(aimDragInput, Vector2.left);
                refreshKeyboardAim.Invoke(aimDragInput, null);
                Assert.That((Vector2)currentAimInput.GetValue(aimDragInput), Is.EqualTo(Vector2.zero));

                rangedAction.SetExternalAimPreviewHeld(true);
                Assert.That((Vector2)currentAimInput.GetValue(aimDragInput), Is.EqualTo(Vector2.left));

                combatModeController.SetMeleeMode();
                Assert.That((Vector2)currentAimInput.GetValue(aimDragInput), Is.EqualTo(Vector2.zero));

                combatModeController.SetRangedMode();
                Assert.That((Vector2)currentAimInput.GetValue(aimDragInput), Is.EqualTo(Vector2.left));

                rangedAction.SetExternalAimPreviewHeld(false);
                Assert.That((Vector2)currentAimInput.GetValue(aimDragInput), Is.EqualTo(Vector2.zero));
                Assert.IsNull(
                    aimDragInputType.GetMethod(
                        "Update",
                        BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly));
            }
            finally
            {
                Object.DestroyImmediate(inputObject);
                Object.DestroyImmediate(playerObject);
            }

            yield return null;
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator PointerDragUsesCameraOnlyOrbitUntilRangedFireIsHeld()
        {
            GameObject playerObject = new("CameraOnlyDragPlayer", typeof(CharacterController));
            PlayerMovementController movementController = playerObject.AddComponent<PlayerMovementController>();
            PlayerCombatModeController combatModeController = playerObject.AddComponent<PlayerCombatModeController>();
            PlayerRangedAimController aimController = playerObject.AddComponent<PlayerRangedAimController>();
            PlayerRangedBasicAttackAction rangedAction = playerObject.AddComponent<PlayerRangedBasicAttackAction>();
            GameObject cameraObject = new("CameraOnlyDragCamera", typeof(Camera), typeof(ActionCameraController));
            Camera controlledCamera = cameraObject.GetComponent<Camera>();
            controlledCamera.tag = "MainCamera";
            cameraObject.transform.SetPositionAndRotation(new Vector3(0f, 2f, -4f), Quaternion.identity);
            ActionCameraController cameraController = cameraObject.GetComponent<ActionCameraController>();
            cameraController.ConfigureTargets(playerObject.transform, null);
            GameObject inputObject = new("CameraOnlyCombatHudAimDragInput", typeof(RectTransform));
            System.Type aimDragInputType = System.Type.GetType(
                "DimensionBrawl.UI.CombatHudAimDragInput, Assembly-CSharp",
                throwOnError: true);
            Component aimDragInput = inputObject.AddComponent(aimDragInputType);
            GameObject eventSystemObject = new("CameraOnlyDragEventSystem", typeof(EventSystem));

            try
            {
                combatModeController.SetRangedMode();
                aimController.ConfigureReferences(combatModeController, cameraController, null, movementController);
                rangedAction.ConfigureReferences(
                    combatModeController,
                    aimController,
                    movementController,
                    null,
                    null,
                    cameraController,
                    null);

                MethodInfo configure = aimDragInputType.GetMethod(
                    "Configure",
                    BindingFlags.Instance | BindingFlags.Public);
                MethodInfo setCameraController = aimDragInputType.GetMethod(
                    "SetCameraController",
                    BindingFlags.Instance | BindingFlags.Public);
                PropertyInfo currentAimInput = aimDragInputType.GetProperty(
                    "CurrentAimInput",
                    BindingFlags.Instance | BindingFlags.Public);
                Assert.IsNotNull(configure);
                Assert.IsNotNull(setCameraController);
                Assert.IsNotNull(currentAimInput);
                configure.Invoke(
                    aimDragInput,
                    new object[] { movementController, combatModeController, aimController, rangedAction });
                setCameraController.Invoke(aimDragInput, new object[] { cameraController });

                PointerEventData pointerDown = new(EventSystem.current) { position = new Vector2(400f, 300f) };
                PointerEventData pointerDrag = new(EventSystem.current)
                {
                    position = new Vector2(560f, 300f),
                    delta = new Vector2(160f, 0f)
                };
                Quaternion playerRotationBeforeDrag = playerObject.transform.rotation;

                ((IPointerDownHandler)aimDragInput).OnPointerDown(pointerDown);
                ((IDragHandler)aimDragInput).OnDrag(pointerDrag);

                Assert.Greater(cameraController.LookPeekInput.x, 0.1f);
                Assert.AreEqual(Vector2.zero, (Vector2)currentAimInput.GetValue(aimDragInput));
                Assert.AreEqual(Vector2.zero, aimController.AimInput);
                Assert.AreEqual(Vector2.zero, rangedAction.AimInput);
                Assert.AreEqual(
                    Vector2.zero,
                    GetPrivateField<Vector2>(movementController, "mobileLookInput"));
                Assert.IsTrue(GetPrivateField<bool>(movementController, "sharedFacingRequestsBlocked"));

                yield return null;
                yield return null;

                Assert.Less(
                    Quaternion.Angle(playerRotationBeforeDrag, playerObject.transform.rotation),
                    0.5f,
                    "Empty-screen camera drag should not rotate the player outside ranged fire.");
                Assert.Greater(
                    cameraController.LookPeekYawOffsetDegrees,
                    0.1f,
                    "Camera-only drag should rotate the camera rig within its bounded orbit.");

                ((IPointerUpHandler)aimDragInput).OnPointerUp(pointerDrag);
                Assert.AreEqual(Vector2.zero, cameraController.LookPeekInput);
                Assert.IsFalse(GetPrivateField<bool>(movementController, "sharedFacingRequestsBlocked"));

                rangedAction.SetFireHeld(true);
                Assert.IsTrue(rangedAction.IsFireHeld);
                ((IPointerDownHandler)aimDragInput).OnPointerDown(pointerDown);
                ((IDragHandler)aimDragInput).OnDrag(pointerDrag);

                Vector2 routedAimInput = (Vector2)currentAimInput.GetValue(aimDragInput);
                Assert.Greater(routedAimInput.x, 0.1f);
                Assert.AreEqual(routedAimInput.x, aimController.AimInput.x, 0.001f);
                Assert.AreEqual(routedAimInput.x, rangedAction.AimInput.x, 0.001f);
                Assert.AreEqual(routedAimInput.x, cameraController.AimOrbitInput.x, 0.001f);
                Assert.AreEqual(Vector2.zero, cameraController.LookPeekInput);
                Assert.IsFalse(GetPrivateField<bool>(movementController, "sharedFacingRequestsBlocked"));

                ((IPointerUpHandler)aimDragInput).OnPointerUp(pointerDrag);
                rangedAction.SetFireHeld(false);
            }
            finally
            {
                Object.DestroyImmediate(eventSystemObject);
                Object.DestroyImmediate(inputObject);
                Object.DestroyImmediate(cameraObject);
                Object.DestroyImmediate(playerObject);
            }

            yield return null;
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator VirtualJoystickDisableIgnoresDestroyedPlayerBinding()
        {
            GameObject movementObject = new("DestroyedJoystickMovementBinding", typeof(CharacterController));
            PlayerMovementController movementController = movementObject.AddComponent<PlayerMovementController>();
            GameObject inputObject = new("CombatHudVirtualJoystick", typeof(RectTransform));
            System.Type joystickType = System.Type.GetType(
                "DimensionBrawl.UI.CombatHudVirtualJoystick, Assembly-CSharp",
                throwOnError: true);
            Component joystick = inputObject.AddComponent(joystickType);
            MethodInfo configure = joystickType.GetMethod("Configure", BindingFlags.Instance | BindingFlags.Public);
            Assert.IsNotNull(configure);
            configure.Invoke(joystick, new object[] { movementController, null });

            Object.Destroy(movementObject);
            yield return null;

            Assert.DoesNotThrow(() => inputObject.SetActive(false));
            LogAssert.NoUnexpectedReceived();

            Object.Destroy(inputObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator BossBarrageBinderDisableIgnoresDestroyedActionBinding()
        {
            GameObject actionObject = new("DestroyedRangedActionBinding");
            PlayerRangedBasicAttackAction rangedAction = actionObject.AddComponent<PlayerRangedBasicAttackAction>();
            GameObject binderObject = new("BossBarrageLaneReviewCombatHudBinder");
            System.Type binderType = System.Type.GetType(
                "DimensionBrawl.UI.BossBarrageLaneReviewCombatHudBinder, Assembly-CSharp",
                throwOnError: true);
            Component binder = binderObject.AddComponent(binderType);
            FieldInfo rangedActionField = binderType.GetField(
                "rangedBasicAttackAction",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(rangedActionField);
            rangedActionField.SetValue(binder, rangedAction);

            Object.Destroy(actionObject);
            yield return null;

            Assert.DoesNotThrow(() => binderObject.SetActive(false));
            LogAssert.NoUnexpectedReceived();

            Object.Destroy(binderObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator BossBarrageBinderRunsAndStopsReviewedRateRefreshRoutine()
        {
            GameObject binderObject = new("ReviewedRateCombatHudBinder", typeof(RectTransform));
            binderObject.SetActive(false);
            System.Type presenterType = System.Type.GetType(
                "DimensionBrawl.UI.CombatHudPresenter, Assembly-CSharp",
                throwOnError: true);
            System.Type binderType = System.Type.GetType(
                "DimensionBrawl.UI.BossBarrageLaneReviewCombatHudBinder, Assembly-CSharp",
                throwOnError: true);
            Component presenter = binderObject.AddComponent(presenterType);
            Behaviour binder = binderObject.AddComponent(binderType) as Behaviour;

            try
            {
                Assert.IsNotNull(binder);
                FieldInfo presenterField = binderType.GetField(
                    "hudPresenter",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                FieldInfo refreshRoutineField = binderType.GetField(
                    "hudRefreshRoutine",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                MethodInfo refreshNow = binderType.GetMethod(
                    "RefreshHudNow",
                    BindingFlags.Instance | BindingFlags.Public);
                Assert.IsNotNull(presenterField);
                Assert.IsNotNull(refreshRoutineField);
                Assert.IsNotNull(refreshNow);
                Assert.IsNull(
                    binderType.GetMethod(
                        "Update",
                        BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly));

                presenterField.SetValue(binder, presenter);
                binderObject.SetActive(true);
                Assert.IsNotNull(refreshRoutineField.GetValue(binder));
                Assert.DoesNotThrow(() => refreshNow.Invoke(binder, null));

                binder.enabled = false;
                Assert.IsNull(refreshRoutineField.GetValue(binder));
            }
            finally
            {
                Object.DestroyImmediate(binderObject);
            }

            yield return null;
            LogAssert.NoUnexpectedReceived();
        }

        private static T GetPrivateField<T>(object target, string fieldName)
        {
            FieldInfo field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Missing private field {fieldName} on {target.GetType().Name}.");
            return (T)field.GetValue(target);
        }
    }
}
