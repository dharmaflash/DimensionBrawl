using System.Collections;
using System.Reflection;
using DimensionBrawl.Player;
using DimensionBrawl.UI;
using NUnit.Framework;
using UnityEngine;
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
        public IEnumerator LegacyMobileHudDisableIgnoresDestroyedMovementBinding()
        {
            GameObject movementObject = new("DestroyedLegacyHudMovementBinding", typeof(CharacterController));
            PlayerMovementController movementController = movementObject.AddComponent<PlayerMovementController>();
            GameObject hudObject = new("BossBarrageLaneReviewMobileHud");
            BossBarrageLaneReviewMobileHud mobileHud = hudObject.AddComponent<BossBarrageLaneReviewMobileHud>();
            FieldInfo movementField = typeof(BossBarrageLaneReviewMobileHud).GetField(
                "movement",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(movementField);
            movementField.SetValue(mobileHud, movementController);

            Object.Destroy(movementObject);
            yield return null;

            Assert.DoesNotThrow(() => hudObject.SetActive(false));
            LogAssert.NoUnexpectedReceived();

            Object.Destroy(hudObject);
            yield return null;
        }
    }
}
