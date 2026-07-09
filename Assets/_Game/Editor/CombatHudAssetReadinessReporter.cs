using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace DimensionBrawl.Editor
{
    public static class CombatHudAssetReadinessReporter
    {
        private const string CombatHudPrefabPath = "Assets/_Game/UI/CombatHud/PF_UI_CombatHud.prefab";
        private const string DimensionHudArtRoot = "Assets/_Game/UI/CombatHud/Art/DimensionHud";
        private const string CombatHudDisabledSummonIconMaterialPath =
            "Assets/_Game/UI/CombatHud/Materials/DB_UI_SummonIconDisabledGrayscale.mat";
        private const string CombatHudDisabledSummonIconShaderName = "DimensionBrawl/UI/GrayscaleTint";

        private static readonly string[] DimensionHudSpriteNames =
        {
            "Hud_BossCostBackground",
            "Hud_BossCostFill",
            "Hud_BossHpBackground",
            "Hud_BossHpFill",
            "Hud_BossNameArea",
            "Hud_BossSymbol",
            "Hud_ButtonAttack",
            "Hud_ButtonDodge",
            "Hud_ButtonPause",
            "Hud_ButtonSettings",
            "Hud_ButtonSkill",
            "Hud_ButtonSwap",
            "Hud_JoystickKnob",
            "Hud_JoystickPanel",
            "Hud_PlayerHpAmountArea",
            "Hud_PlayerHpBackground",
            "Hud_PlayerHpFill",
            "Hud_PlayerMpAmountArea",
            "Hud_PlayerMpBackground",
            "Hud_PlayerMpFill",
            "Hud_PlayerNameArea",
            "Hud_PlayerSymbol",
            "Hud_SummonSlot1Icon",
            "Hud_SummonSlot1Frame",
            "Hud_SummonSlot2Icon",
            "Hud_SummonSlot2Frame",
            "Hud_SummonSlot3Icon",
            "Hud_SummonSlot3Frame",
            "Hud_TopLeftPanel"
        };

        private static readonly string[] GeneratedSpritePaths =
        {
            "Assets/_Game/UI/CombatHud/Generated/DB_UI_SummonProgressRing.png",
            "Assets/_Game/UI/CombatHud/Generated/DB_UI_SummonReadyGlow.png",
            "Assets/_Game/UI/CombatHud/Generated/DB_UI_SummonReadySparkRing.png",
            "Assets/_Game/UI/CombatHud/Generated/DB_UI_ActionCooldownRing.png",
            "Assets/_Game/UI/CombatHud/Generated/DB_UI_ActionReadyGlow.png"
        };

        private static readonly string[] RequiredPrefabChildren =
        {
            "Timer",
            "Objective",
            "ActionFeedback",
            "InputMode",
            "HealthText",
            "ResourceText",
            "AmmoText",
            "PauseButton",
            "MoveJoystickRing",
            "MoveJoystickKnob",
            "BasicAttackButton",
            "DodgeButton",
            "Skill1Button",
            "UltimateButton",
            "SummonSlot1Button",
            "SummonSlot2Button",
            "SummonSlot3Button"
        };

        [MenuItem("DimensionBrawl/Reports/Combat HUD Asset Readiness")]
        public static void ReportMenu()
        {
            ReportCurrentReadiness();
        }

        public static bool ReportCurrentReadiness()
        {
            List<string> issues = new List<string>();
            CheckDimensionHudSprites(issues);
            CheckGeneratedSprites(issues);
            CheckDisabledSummonIconMaterial(issues);
            CheckCombatHudPrefab(issues);

            if (issues.Count == 0)
            {
                Debug.Log("Combat HUD asset readiness passed. Report is read-only; no assets, prefabs, scenes, or ProjectSettings were modified.");
                return true;
            }

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Combat HUD asset readiness warning. Report is read-only; no assets, prefabs, scenes, or ProjectSettings were modified.");
            for (int i = 0; i < issues.Count; i++)
            {
                builder.Append("- ");
                builder.AppendLine(issues[i]);
            }

            Debug.LogWarning(builder.ToString());
            return false;
        }

        private static void CheckDimensionHudSprites(List<string> issues)
        {
            for (int i = 0; i < DimensionHudSpriteNames.Length; i++)
            {
                string path = $"{DimensionHudArtRoot}/{DimensionHudSpriteNames[i]}.png";
                CheckSpriteAsset(path, issues);
            }
        }

        private static void CheckGeneratedSprites(List<string> issues)
        {
            for (int i = 0; i < GeneratedSpritePaths.Length; i++)
            {
                CheckSpriteAsset(GeneratedSpritePaths[i], issues);
            }
        }

        private static void CheckSpriteAsset(string path, List<string> issues)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
            {
                issues.Add($"Missing sprite asset: {path}");
                return;
            }

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                issues.Add($"Missing texture importer for sprite asset: {path}");
                return;
            }

            if (importer.textureType != TextureImporterType.Sprite)
            {
                issues.Add($"Sprite asset is not imported as Sprite: {path}");
            }

            if (importer.spriteImportMode != SpriteImportMode.Single)
            {
                issues.Add($"Sprite asset is not imported as single sprite: {path}");
            }
        }

        private static void CheckDisabledSummonIconMaterial(List<string> issues)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(CombatHudDisabledSummonIconMaterialPath);
            if (material == null)
            {
                issues.Add($"Missing disabled summon icon material: {CombatHudDisabledSummonIconMaterialPath}");
                return;
            }

            if (material.shader == null)
            {
                issues.Add($"Disabled summon icon material has no shader: {CombatHudDisabledSummonIconMaterialPath}");
                return;
            }

            if (!string.Equals(material.shader.name, CombatHudDisabledSummonIconShaderName, System.StringComparison.Ordinal))
            {
                issues.Add(
                    $"Disabled summon icon material shader mismatch: expected {CombatHudDisabledSummonIconShaderName}, found {material.shader.name}");
            }
        }

        private static void CheckCombatHudPrefab(List<string> issues)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CombatHudPrefabPath);
            if (prefab == null)
            {
                issues.Add($"Missing Combat HUD prefab: {CombatHudPrefabPath}");
                return;
            }

            for (int i = 0; i < RequiredPrefabChildren.Length; i++)
            {
                if (FindDescendant(prefab.transform, RequiredPrefabChildren[i]) == null)
                {
                    issues.Add($"Combat HUD prefab is missing child: {RequiredPrefabChildren[i]}");
                }
            }

            RequireChildComponent<Text>(prefab.transform, "Timer", issues);
            RequireChildComponent<Text>(prefab.transform, "Objective", issues);
            RequireChildComponent<Text>(prefab.transform, "ActionFeedback", issues);
            RequireChildComponent<Text>(prefab.transform, "HealthText", issues);
            RequireChildComponent<Text>(prefab.transform, "ResourceText", issues);
            RequireChildComponent<Text>(prefab.transform, "AmmoText", issues);
            RequireChildComponent<Image>(prefab.transform, "BasicAttackButton", issues);
            RequireChildComponent<Image>(prefab.transform, "DodgeButton", issues);
            RequireChildComponent<Image>(prefab.transform, "Skill1Button", issues);
        }

        private static void RequireChildComponent<T>(Transform root, string childName, List<string> issues)
            where T : Component
        {
            Transform child = FindDescendant(root, childName);
            if (child == null)
            {
                return;
            }

            if (child.GetComponent<T>() == null)
            {
                issues.Add($"Combat HUD prefab child {childName} is missing {typeof(T).Name}.");
            }
        }

        private static Transform FindDescendant(Transform root, string childName)
        {
            if (root == null)
            {
                return null;
            }

            if (string.Equals(root.name, childName, System.StringComparison.Ordinal))
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform result = FindDescendant(root.GetChild(i), childName);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }
    }
}
