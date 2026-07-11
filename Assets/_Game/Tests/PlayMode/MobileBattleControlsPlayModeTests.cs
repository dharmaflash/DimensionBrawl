using System.Collections;
using System.Collections.Generic;
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

        [UnityTest]
        public IEnumerator StableVisualStateReusesCachedMobileReadoutStrings()
        {
            GameObject controlsObject = new("MobileBattleControlsReadoutCacheTest", typeof(RectTransform));
            MobileBattleControls controls = controlsObject.AddComponent<MobileBattleControls>();

            try
            {
                yield return null;

                SetField(controls, "isTouchLayoutActive", true);
                SetField(controls, "isSummonPlacementActive", false);
                MethodInfo updateVisuals = typeof(MobileBattleControls).GetMethod(
                    "UpdateButtonVisualState",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.IsNotNull(updateVisuals);

                updateVisuals.Invoke(controls, null);
                Component laneStatusText = GetField<Component>(controls, "laneStatusText");
                Dictionary<int, string> readoutCache =
                    GetField<Dictionary<int, string>>(controls, "laneStatusReadoutCache");
                Assert.IsNotNull(laneStatusText);
                string firstReadout = GetText(laneStatusText);
                int firstCacheSize = readoutCache.Count;

                updateVisuals.Invoke(controls, null);

                Assert.AreSame(
                    firstReadout,
                    GetText(laneStatusText),
                    "Equivalent visual refreshes should reuse the cached lane readout string instance.");
                Assert.AreEqual(firstCacheSize, readoutCache.Count);

                SetField(controls, "isSummonPlacementActive", true);
                SetField(controls, "previewLaneIndex", 2);
                updateVisuals.Invoke(controls, null);
                string placementReadout = GetText(laneStatusText);
                int placementCacheSize = readoutCache.Count;
                updateVisuals.Invoke(controls, null);

                Assert.AreSame(placementReadout, GetText(laneStatusText));
                Assert.AreEqual(placementCacheSize, readoutCache.Count);
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

        private static T GetField<T>(MobileBattleControls target, string fieldName)
        {
            FieldInfo field = typeof(MobileBattleControls).GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Expected MobileBattleControls field '{fieldName}'.");
            return (T)field.GetValue(target);
        }

        private static string GetText(Component textComponent)
        {
            PropertyInfo textProperty = textComponent.GetType().GetProperty("text", BindingFlags.Instance | BindingFlags.Public);
            Assert.IsNotNull(textProperty);
            return (string)textProperty.GetValue(textComponent);
        }
    }
}
