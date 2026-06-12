using System;
using System.Collections.Generic;
using DimensionBrawl.Combat;
using DimensionBrawl.Enemies;
using DimensionBrawl.Player;
using DimensionBrawl.Presentation;
using DimensionBrawl.Test;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DimensionBrawl.Editor
{
    public static class ActionFoundationSceneValidator
    {
        private const string ScenePath = "Assets/_Game/Scenes/ActionFoundationTest.unity";
        private const string PlayerVisualName = "CombatGirlSwordShield_PlayerVisual";
        private const string LegacyPlayerVisualName = "CombatGirls_Sword_Shield";
        private const string CombatGirlMaterialRoot = "Assets/_Game/Art/Characters/Player/CombatGirlSwordShield/Materials";

        [MenuItem("DimensionBrawl/Validate Action Foundation Test Scene")]
        public static void ValidateMenu()
        {
            ValidateSceneAsset();
            Debug.Log("Action foundation test scene validation passed.");
        }

        [MenuItem("DimensionBrawl/Reapply Action Foundation CombatGirl Materials")]
        public static void ReapplyCombatGirlMaterialsMenu()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != ScenePath)
            {
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }

            GameObject[] roots = scene.GetRootGameObjects();
            GameObject playerVisual = FindNamedObject(roots, PlayerVisualName) ?? FindNamedObject(roots, LegacyPlayerVisualName);
            if (playerVisual == null)
            {
                throw new InvalidOperationException($"Missing required {PlayerVisualName} in {ScenePath}.");
            }

            playerVisual.name = PlayerVisualName;
            int reassignedCount = ReapplyCombatGirlMaterials(playerVisual);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log($"Reapplied CombatGirl materials on {reassignedCount} renderer slots in {ScenePath}.");
        }

        public static void ValidateSceneAsset()
        {
            Scene previousScene = SceneManager.GetActiveScene();
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            try
            {
                ValidateScene(scene);
                Debug.Log("Action foundation test scene validation passed.");
            }
            finally
            {
                if (previousScene.IsValid() && !string.IsNullOrEmpty(previousScene.path) && previousScene.path != ScenePath)
                {
                    EditorSceneManager.OpenScene(previousScene.path, OpenSceneMode.Single);
                }
            }
        }

        private static void ValidateScene(Scene scene)
        {
            if (!scene.IsValid())
            {
                throw new InvalidOperationException($"Scene is invalid: {ScenePath}");
            }

            GameObject[] roots = scene.GetRootGameObjects();
            PlayerMovementController movement = RequireObject<PlayerMovementController>(roots, "player movement");
            PlayerActionController playerActions = RequireObject<PlayerActionController>(roots, "player actions");
            BasicSoldierEnemy soldier = RequireObject<BasicSoldierEnemy>(roots, "basic soldier");
            ActionCameraController cameraController = RequireObject<ActionCameraController>(roots, "action camera");
            Type dodgeFeedbackType = RequireType("DimensionBrawl.Presentation.PlayerDodgeFeedback, DimensionBrawl.Runtime");
            Component dodgeFeedback = RequireObject(roots, dodgeFeedbackType, "player dodge feedback");
            GameObject playerVisual = RequireNamedObject(roots, PlayerVisualName, "player visual");
            Animator playerVisualAnimator = RequireComponent<Animator>(playerVisual, "player visual animator");
            List<CombatHitFeedback> hitFeedbacks = CollectObjects<CombatHitFeedback>(roots);
            ActionFoundationTestEncounter encounter = RequireObject<ActionFoundationTestEncounter>(roots, "test encounter");

            List<CombatHealth> healths = CollectObjects<CombatHealth>(roots);
            RequireAtLeast(healths.Count, 2, "player and enemy health components");
            RequireAtLeast(hitFeedbacks.Count, 2, "player and enemy hit feedback components");

            ValidateReference(movement, "referenceCamera");
            ValidateReference(movement, "animator");
            ValidateReference(playerActions, "movement");
            ValidateReference(playerActions, "health");
            ValidateReference(playerActions, "animator");
            ValidateArraySize(playerActions, "basicCombo", 3);
            ValidateFloat(playerActions, "dodgeDurationSeconds", 0.62f);
            ValidateFloat(playerActions, "dodgeInvulnerableFromSeconds", 0.05f);
            ValidateFloat(playerActions, "dodgeInvulnerableToSeconds", 0.40f);
            ValidateFloat(playerActions, "dodgeRecoverySeconds", 0.18f);

            ValidateReference(soldier, "target");
            ValidateReference(soldier, "targetHealth");
            ValidateReference(soldier, "selfHealth");
            ValidateReference(soldier, "telegraphIndicator");
            ValidateFloat(soldier, "telegraphSeconds", 0.65f);
            ValidateFloat(soldier, "activeSeconds", 0.14f);
            ValidateFloat(soldier, "recoverySeconds", 0.45f);
            ValidateFloat(soldier, "hitReactionSeconds", 0.24f);

            ValidateReference(cameraController, "target");
            ValidateReference(cameraController, "threat");
            ValidateFloat(cameraController, "defaultCueSeconds", 0.24f);

            ValidateReference(playerVisualAnimator, "m_Avatar");
            ValidateReference(playerVisualAnimator, "m_Controller");
            ValidateBool(playerVisualAnimator, "m_ApplyRootMotion", false);
            ValidateCombatGirlMaterials(playerVisual);

            ValidateReference(dodgeFeedback, "actionController");
            ValidateArrayMinSize(dodgeFeedback, "targetRenderers", 1);

            ValidateReference(encounter, "playerHealth");
            ValidateReference(encounter, "enemyHealth");
            ValidateReference(encounter, "winMarker");
            ValidateReference(encounter, "failMarker");
        }

        private static T RequireObject<T>(GameObject[] roots, string label) where T : Component
        {
            List<T> found = CollectObjects<T>(roots);
            if (found.Count == 0)
            {
                throw new InvalidOperationException($"Missing required {label}.");
            }

            return found[0];
        }

        private static GameObject RequireNamedObject(GameObject[] roots, string objectName, string label)
        {
            GameObject found = FindNamedObject(roots, objectName);
            if (found == null)
            {
                throw new InvalidOperationException($"Missing required {label}: {objectName}.");
            }

            return found;
        }

        private static GameObject FindNamedObject(GameObject[] roots, string objectName)
        {
            for (int i = 0; i < roots.Length; i++)
            {
                if (roots[i] == null)
                {
                    continue;
                }

                Transform[] transforms = roots[i].GetComponentsInChildren<Transform>(true);
                for (int j = 0; j < transforms.Length; j++)
                {
                    if (transforms[j] != null && transforms[j].name == objectName)
                    {
                        return transforms[j].gameObject;
                    }
                }
            }

            return null;
        }

        private static T RequireComponent<T>(GameObject owner, string label) where T : Component
        {
            if (!owner.TryGetComponent(out T component))
            {
                throw new InvalidOperationException($"{owner.name} is missing required {label}.");
            }

            return component;
        }

        private static Component RequireObject(GameObject[] roots, Type componentType, string label)
        {
            List<Component> found = CollectObjects(roots, componentType);
            if (found.Count == 0)
            {
                throw new InvalidOperationException($"Missing required {label}.");
            }

            return found[0];
        }

        private static Type RequireType(string typeName)
        {
            Type type = Type.GetType(typeName);
            if (type == null)
            {
                throw new InvalidOperationException($"Missing required runtime type {typeName}.");
            }

            return type;
        }

        private static List<T> CollectObjects<T>(GameObject[] roots) where T : Component
        {
            List<T> results = new List<T>();

            for (int i = 0; i < roots.Length; i++)
            {
                if (roots[i] == null)
                {
                    continue;
                }

                results.AddRange(roots[i].GetComponentsInChildren<T>(true));
            }

            return results;
        }

        private static List<Component> CollectObjects(GameObject[] roots, Type componentType)
        {
            List<Component> results = new List<Component>();

            for (int i = 0; i < roots.Length; i++)
            {
                if (roots[i] == null)
                {
                    continue;
                }

                results.AddRange(roots[i].GetComponentsInChildren(componentType, true));
            }

            return results;
        }

        private static int ReapplyCombatGirlMaterials(GameObject playerVisual)
        {
            Dictionary<string, Material> materialByAlias = BuildCombatGirlMaterialAliases();
            Renderer[] renderers = playerVisual.GetComponentsInChildren<Renderer>(true);
            int reassignedCount = 0;

            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer targetRenderer = renderers[i];
                Material[] sharedMaterials = targetRenderer.sharedMaterials;
                bool changed = false;

                for (int j = 0; j < sharedMaterials.Length; j++)
                {
                    string key = ResolveMaterialAlias(sharedMaterials[j], targetRenderer);
                    if (key == null || !materialByAlias.TryGetValue(key, out Material replacement))
                    {
                        continue;
                    }

                    if (sharedMaterials[j] != replacement)
                    {
                        sharedMaterials[j] = replacement;
                        changed = true;
                        reassignedCount++;
                    }
                }

                if (changed)
                {
                    targetRenderer.sharedMaterials = sharedMaterials;
                    EditorUtility.SetDirty(targetRenderer);
                }
            }

            return reassignedCount;
        }

        private static Dictionary<string, Material> BuildCombatGirlMaterialAliases()
        {
            Dictionary<string, Material> materials = new Dictionary<string, Material>();
            AddCombatGirlMaterial(materials, "DB_CombatGirl_Body", "Body", "Body");
            AddCombatGirlMaterial(materials, "DB_CombatGirl_Eye", "Eye", "Eye", "eye");
            AddCombatGirlMaterial(materials, "DB_CombatGirl_Face", "Face", "Face");
            AddCombatGirlMaterial(materials, "DB_CombatGirl_ShieldCloth01", "ShieldCloth01", "Shield_Cloth", "Shiled_Cloth", "Shield_Sword_Cloth_01");
            AddCombatGirlMaterial(materials, "DB_CombatGirl_ShieldHair01", "ShieldHair01", "Shield_Hair", "Shiled_Hair", "Shield_Sword_Hair_01");
            AddCombatGirlMaterial(materials, "DB_CombatGirl_SquireCloth", "SquireCloth", "Squire_Cloth");
            AddCombatGirlMaterial(materials, "DB_CombatGirl_WeaponAxe01", "WeaponAxe01", "Weapon_Axe_Shiled", "Weapon_Shield_Axe_01");
            AddCombatGirlMaterial(materials, "DB_CombatGirl_WeaponSword01", "WeaponSword01", "Shield_Weapon", "Weapon_Shiled_Sword", "Weapon_Sword_Shiled", "Weapon_Shield_Sword_01");
            return materials;
        }

        private static void AddCombatGirlMaterial(Dictionary<string, Material> materials, string assetName, string fileAlias, params string[] aliases)
        {
            string path = $"{CombatGirlMaterialRoot}/{assetName}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                throw new InvalidOperationException($"Missing CombatGirl material asset: {path}");
            }

            materials[NormalizeMaterialAlias(assetName)] = material;
            materials[NormalizeMaterialAlias(fileAlias)] = material;
            for (int i = 0; i < aliases.Length; i++)
            {
                materials[NormalizeMaterialAlias(aliases[i])] = material;
            }
        }

        private static string ResolveMaterialAlias(Material material, Renderer renderer)
        {
            if (material != null)
            {
                return NormalizeMaterialAlias(material.name);
            }

            return renderer != null ? NormalizeMaterialAlias(renderer.name) : null;
        }

        private static string NormalizeMaterialAlias(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            string normalized = value.Replace("(Instance)", string.Empty).ToLowerInvariant();
            char[] buffer = new char[normalized.Length];
            int count = 0;
            for (int i = 0; i < normalized.Length; i++)
            {
                char c = normalized[i];
                if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
                {
                    buffer[count] = c;
                    count++;
                }
            }

            return new string(buffer, 0, count);
        }

        private static void ValidateCombatGirlMaterials(GameObject playerVisual)
        {
            Renderer[] renderers = playerVisual.GetComponentsInChildren<Renderer>(true);
            RequireAtLeast(renderers.Length, 1, "CombatGirl visual renderers");

            for (int i = 0; i < renderers.Length; i++)
            {
                Material[] sharedMaterials = renderers[i].sharedMaterials;
                for (int j = 0; j < sharedMaterials.Length; j++)
                {
                    Material material = sharedMaterials[j];
                    if (material == null)
                    {
                        throw new InvalidOperationException($"{renderers[i].name} has a missing CombatGirl material slot.");
                    }

                    string materialPath = AssetDatabase.GetAssetPath(material).Replace('\\', '/');
                    if (!materialPath.StartsWith(CombatGirlMaterialRoot + "/", StringComparison.Ordinal))
                    {
                        throw new InvalidOperationException($"{renderers[i].name} uses non-game-owned CombatGirl material: {materialPath}");
                    }
                }
            }
        }

        private static void RequireAtLeast(int actual, int expected, string label)
        {
            if (actual < expected)
            {
                throw new InvalidOperationException($"Expected at least {expected} {label}, found {actual}.");
            }
        }

        private static void ValidateReference(UnityEngine.Object target, string propertyName)
        {
            SerializedProperty property = FindProperty(target, propertyName);
            if (property.objectReferenceValue == null)
            {
                throw new InvalidOperationException($"{target.name} has no reference assigned for {propertyName}.");
            }
        }

        private static void ValidateArraySize(UnityEngine.Object target, string propertyName, int expected)
        {
            SerializedProperty property = FindProperty(target, propertyName);
            if (!property.isArray || property.arraySize != expected)
            {
                throw new InvalidOperationException($"{target.name}.{propertyName} expected array size {expected}, found {property.arraySize}.");
            }
        }

        private static void ValidateArrayMinSize(UnityEngine.Object target, string propertyName, int minimum)
        {
            SerializedProperty property = FindProperty(target, propertyName);
            if (!property.isArray || property.arraySize < minimum)
            {
                throw new InvalidOperationException($"{target.name}.{propertyName} expected at least {minimum}, found {property.arraySize}.");
            }
        }

        private static void ValidateFloat(UnityEngine.Object target, string propertyName, float expected)
        {
            SerializedProperty property = FindProperty(target, propertyName);
            if (!Mathf.Approximately(property.floatValue, expected))
            {
                throw new InvalidOperationException($"{target.name}.{propertyName} expected {expected}, found {property.floatValue}.");
            }
        }

        private static void ValidateBool(UnityEngine.Object target, string propertyName, bool expected)
        {
            SerializedProperty property = FindProperty(target, propertyName);
            if (property.boolValue != expected)
            {
                throw new InvalidOperationException($"{target.name}.{propertyName} expected {expected}, found {property.boolValue}.");
            }
        }

        private static SerializedProperty FindProperty(UnityEngine.Object target, string propertyName)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                throw new InvalidOperationException($"{target.name} is missing serialized property {propertyName}.");
            }

            return property;
        }
    }
}
