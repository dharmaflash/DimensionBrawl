using System;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace DimensionBrawl.Tests
{
    public sealed class CombatHudCanonicalWiringPlayModeTests
    {
        private const string CombatHudPrefabPath =
            "Assets/_Game/UI/CombatHud/PF_UI_CombatHud.prefab";
        private const string CorridorScenePath =
            "Assets/_Game/Scenes/OlympusCorridorInvasionStage.unity";
        private const string StationScenePath =
            "Assets/_Game/Scenes/OlympusStationCombatStage.unity";
        private const string CourtyardScenePath =
            "Assets/_Game/Scenes/OlympusCourtyardDrillStage.unity";
        private const string CombatHudPrefabGuid = "4e5297b5734b6664b935ffb1ae9b48b6";
        private const long PauseButtonSourceFileId = 30444043197168018L;
        private const long TopSettingsSourceFileId = 5704358636811254415L;

        [Test]
        public void CanonicalPrefabOwnsPauseInputAndHidesRedundantTopSettingsAffordance()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CombatHudPrefabPath);
            Assert.That(prefab, Is.Not.Null, $"Missing canonical combat HUD prefab: {CombatHudPrefabPath}");

            Transform pauseButton = RequireUniqueNamedTransform(prefab, "PauseButton");
            Type pointerInputType = RequireProductType("DimensionBrawl.UI.CombatHudPointerActionInput");
            Component[] pauseInputs = pauseButton.GetComponents(pointerInputType);
            Assert.That(
                pauseInputs,
                Has.Length.EqualTo(1),
                "Pause input must be authored once on the canonical prefab, not as a scene override.");
            Assert.That(ReadProperty(pauseInputs[0], "ActionId").ToString(), Is.EqualTo("Pause"));
            Assert.That(ReadProperty<bool>(pauseInputs[0], "SendsHoldState"), Is.False);

            var serializedPauseInput = new SerializedObject(pauseInputs[0]);
            SerializedProperty inputBridgeProperty = serializedPauseInput.FindProperty("inputBridge");
            Assert.That(inputBridgeProperty, Is.Not.Null);
            Assert.That(
                inputBridgeProperty.objectReferenceValue,
                Is.Not.Null,
                "Canonical Pause input must explicitly target the canonical HUD input bridge.");
            Assert.That(
                inputBridgeProperty.objectReferenceValue.GetType().FullName,
                Is.EqualTo("DimensionBrawl.UI.CombatHudInputBridge"));

            Transform skinRoot = RequireUniqueNamedTransform(prefab, "DimensionHudSkinRoot");
            Transform topSettings = skinRoot.Find("SettingsButton");
            Assert.That(topSettings, Is.Not.Null, "Dimension HUD skin lost its top Settings affordance.");
            Assert.That(
                topSettings.gameObject.activeSelf,
                Is.False,
                "The non-functional top Settings affordance must stay hidden until it owns a direct route.");
            Graphic topSettingsGraphic = topSettings.GetComponent<Graphic>();
            Assert.That(topSettingsGraphic, Is.Not.Null);
            Assert.That(topSettingsGraphic.raycastTarget, Is.False);

            Type overlayType = RequireProductType("DimensionBrawl.UI.CombatSessionOverlayPresenter");
            Component sessionOverlay = prefab.GetComponentInChildren(overlayType, includeInactive: true);
            Assert.That(sessionOverlay, Is.Not.Null);
            var serializedOverlay = new SerializedObject(sessionOverlay);
            Button routedSettingsButton =
                serializedOverlay.FindProperty("settingsButton")?.objectReferenceValue as Button;
            Assert.That(
                routedSettingsButton,
                Is.Not.Null,
                "Hiding the redundant HUD icon must not remove the pause overlay's real Settings route.");
            Assert.That(routedSettingsButton.gameObject.activeSelf, Is.True);
        }

        [TestCase(CorridorScenePath)]
        [TestCase(StationScenePath)]
        [TestCase(CourtyardScenePath)]
        public void CanonicalScenesInheritPauseInputWithoutSceneAddedOverride(string scenePath)
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            Assert.That(projectRoot, Is.Not.Null.And.Not.Empty);
            string absoluteScenePath = Path.Combine(projectRoot, scenePath);
            Assert.That(File.Exists(absoluteScenePath), Is.True, $"Missing canonical scene: {scenePath}");

            string sceneYaml = File.ReadAllText(absoluteScenePath);
            string prefabSource =
                $"m_SourcePrefab: {{fileID: 100100000, guid: {CombatHudPrefabGuid}, type: 3}}";
            Assert.That(
                CountOccurrences(sceneYaml, prefabSource),
                Is.EqualTo(1),
                $"{scenePath} must contain exactly one canonical combat HUD prefab instance.");

            string sceneAddedPauseTarget =
                $"targetCorrespondingSourceObject: {{fileID: {PauseButtonSourceFileId}, " +
                $"guid: {CombatHudPrefabGuid}, type: 3}}";
            Assert.That(
                sceneYaml,
                Does.Not.Contain(sceneAddedPauseTarget),
                $"{scenePath} must inherit Pause input from the prefab without a scene-added component.");

            string settingsActiveOverride =
                $"target: {{fileID: {TopSettingsSourceFileId}, guid: {CombatHudPrefabGuid}, type: 3}}";
            Assert.That(
                sceneYaml,
                Does.Not.Contain(settingsActiveOverride),
                $"{scenePath} must not revive the canonical HUD's hidden false Settings affordance.");
        }

        private static Transform RequireUniqueNamedTransform(GameObject root, string objectName)
        {
            Transform match = null;
            int count = 0;
            Transform[] transforms = root.GetComponentsInChildren<Transform>(includeInactive: true);
            for (int i = 0; i < transforms.Length; i++)
            {
                Transform candidate = transforms[i];
                if (candidate == null || candidate.name != objectName)
                {
                    continue;
                }

                match = candidate;
                count++;
            }

            Assert.That(count, Is.EqualTo(1), $"Expected one {objectName} under {root.name}.");
            return match;
        }

        private static int CountOccurrences(string source, string value)
        {
            int count = 0;
            int startIndex = 0;
            while ((startIndex = source.IndexOf(value, startIndex, System.StringComparison.Ordinal)) >= 0)
            {
                count++;
                startIndex += value.Length;
            }

            return count;
        }

        private static Type RequireProductType(string fullName)
        {
            Type type = Type.GetType($"{fullName}, Assembly-CSharp", throwOnError: false);
            Assert.That(type, Is.Not.Null, $"Missing product type {fullName}.");
            return type;
        }

        private static object ReadProperty(object instance, string propertyName)
        {
            var property = instance.GetType().GetProperty(
                propertyName,
                System.Reflection.BindingFlags.Instance
                    | System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.NonPublic);
            Assert.That(property, Is.Not.Null, $"Missing property {propertyName}.");
            return property.GetValue(instance);
        }

        private static T ReadProperty<T>(object instance, string propertyName)
        {
            return (T)ReadProperty(instance, propertyName);
        }
    }
}
