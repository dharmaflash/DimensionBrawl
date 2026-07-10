using System.Collections;
using System.Reflection;
using IsekaiBrawl.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace DimensionBrawl.Tests
{
    public sealed class MobileBattleControlsPlayModeTests
    {
        [UnityTest]
        public IEnumerator JoystickInputReachesPlayerMovementRoute()
        {
            GameObject controlsObject = new("MobileBattleControlsTest", typeof(RectTransform));
            MobileBattleControls controls = controlsObject.AddComponent<MobileBattleControls>();

            try
            {
                yield return null;

                SetField(controls, "isTouchLayoutActive", true);
                SetField(controls, "currentInputMode", MobileMoveInputMode.Joystick);
                SetField(controls, "moveVector", new Vector2(0.75f, -0.25f));

                Assert.IsTrue(
                    MobileBattleControls.TryGetMoveInput(out Vector2 moveInput),
                    "An active mobile joystick should contribute movement input.");
                Assert.That(moveInput.x, Is.EqualTo(0.75f).Within(0.0001f));
                Assert.That(moveInput.y, Is.EqualTo(-0.25f).Within(0.0001f));

                SetField(controls, "isDirectDodgeModeActive", true);
                Assert.IsFalse(
                    MobileBattleControls.TryGetMoveInput(out moveInput),
                    "Direct-dodge mode should suppress ordinary movement input.");
                Assert.That(moveInput, Is.EqualTo(Vector2.zero));
            }
            finally
            {
                Object.Destroy(controlsObject);
            }

            yield return null;
        }

        private static void SetField<T>(MobileBattleControls target, string fieldName, T value)
        {
            FieldInfo field = typeof(MobileBattleControls).GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Expected MobileBattleControls field '{fieldName}'.");
            field.SetValue(target, value);
        }
    }
}
