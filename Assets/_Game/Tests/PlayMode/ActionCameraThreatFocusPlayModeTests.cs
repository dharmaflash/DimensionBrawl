using System.Collections;
using DimensionBrawl.Presentation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace DimensionBrawl.Tests
{
    public sealed class ActionCameraThreatFocusPlayModeTests
    {
        [UnityTest]
        public IEnumerator ThreatFocusCanFrameAnEnemyWithoutMovingTheShoulderAnchor()
        {
            GameObject target = new("ThreatFocus_Target");
            GameObject threat = new("ThreatFocus_Threat");
            GameObject cameraObject = new("ThreatFocus_Camera");
            try
            {
                target.transform.position = Vector3.zero;
                threat.transform.position = new Vector3(0f, 0f, 10f);
                cameraObject.SetActive(false);
                cameraObject.AddComponent<Camera>();
                ActionCameraController controller =
                    cameraObject.AddComponent<ActionCameraController>();

                SerializedObject serialized = new(controller);
                Set(serialized, "cameraOffset", new Vector3(0f, 1f, -4f));
                Set(serialized, "lookOffset", new Vector3(0f, 1f, 0f));
                Set(serialized, "followSmoothTime", 0f);
                Set(serialized, "rotationSmooth", 1000f);
                Set(serialized, "useFixedRearYaw", true);
                Set(serialized, "fixedRearYawReference", target.transform);
                Set(serialized, "threatBias", 0.5f);
                Set(serialized, "maxThreatFocusOffset", 10f);
                Set(serialized, "threatFocusAffectsCameraPosition", false);
                Set(serialized, "maxLeadFromPlayerSpeed", 0f);
                Set(serialized, "enableMicroShake", false);
                serialized.ApplyModifiedPropertiesWithoutUndo();

                controller.ConfigureTargets(target.transform, threat.transform);
                cameraObject.transform.SetPositionAndRotation(
                    new Vector3(0f, 2f, -4f),
                    Quaternion.LookRotation(new Vector3(0f, -1f, 9f)));
                cameraObject.SetActive(true);
                yield return null;

                Assert.That(cameraObject.transform.position.x, Is.EqualTo(0f).Within(0.0001f));
                Assert.That(cameraObject.transform.position.y, Is.EqualTo(2f).Within(0.0001f));
                Assert.That(cameraObject.transform.position.z, Is.EqualTo(-4f).Within(0.0001f),
                    "Threat framing moved the fixed shoulder anchor toward the enemy.");
                Vector3 expectedLook = new Vector3(0f, 1f, 5f) - cameraObject.transform.position;
                Assert.That(Vector3.Angle(cameraObject.transform.forward, expectedLook),
                    Is.LessThan(0.05f));
            }
            finally
            {
                Object.DestroyImmediate(cameraObject);
                Object.DestroyImmediate(threat);
                Object.DestroyImmediate(target);
            }
        }

        [UnityTest]
        public IEnumerator LegacyThreatFocusStillMovesTheCameraPositionByDefault()
        {
            GameObject target = new("LegacyThreatFocus_Target");
            GameObject threat = new("LegacyThreatFocus_Threat");
            GameObject cameraObject = new("LegacyThreatFocus_Camera");
            try
            {
                target.transform.position = Vector3.zero;
                threat.transform.position = new Vector3(0f, 0f, 10f);
                cameraObject.SetActive(false);
                cameraObject.AddComponent<Camera>();
                ActionCameraController controller =
                    cameraObject.AddComponent<ActionCameraController>();

                SerializedObject serialized = new(controller);
                Set(serialized, "cameraOffset", new Vector3(0f, 1f, -4f));
                Set(serialized, "lookOffset", new Vector3(0f, 1f, 0f));
                Set(serialized, "followSmoothTime", 0f);
                Set(serialized, "rotationSmooth", 1000f);
                Set(serialized, "useFixedRearYaw", true);
                Set(serialized, "fixedRearYawReference", target.transform);
                Set(serialized, "threatBias", 0.5f);
                Set(serialized, "maxThreatFocusOffset", 10f);
                Set(serialized, "maxLeadFromPlayerSpeed", 0f);
                Set(serialized, "enableMicroShake", false);
                serialized.ApplyModifiedPropertiesWithoutUndo();

                controller.ConfigureTargets(target.transform, threat.transform);
                cameraObject.SetActive(true);
                yield return null;

                Assert.That(cameraObject.transform.position.z, Is.EqualTo(1f).Within(0.0001f),
                    "The opt-in City framing mode changed the legacy default rig.");
            }
            finally
            {
                Object.DestroyImmediate(cameraObject);
                Object.DestroyImmediate(threat);
                Object.DestroyImmediate(target);
            }
        }

        private static void Set(SerializedObject serialized, string propertyName, Vector3 value)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            Assert.That(property, Is.Not.Null, propertyName);
            property.vector3Value = value;
        }

        private static void Set(SerializedObject serialized, string propertyName, float value)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            Assert.That(property, Is.Not.Null, propertyName);
            property.floatValue = value;
        }

        private static void Set(SerializedObject serialized, string propertyName, bool value)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            Assert.That(property, Is.Not.Null, propertyName);
            property.boolValue = value;
        }

        private static void Set(
            SerializedObject serialized,
            string propertyName,
            Object value)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            Assert.That(property, Is.Not.Null, propertyName);
            property.objectReferenceValue = value;
        }
    }
}
