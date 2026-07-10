using System.Collections;
using System.Reflection;
using DimensionBrawl.Player;
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
    }
}
