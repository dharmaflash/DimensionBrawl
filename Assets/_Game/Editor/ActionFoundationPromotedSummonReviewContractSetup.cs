using System;
using DimensionBrawl.Player;
using DimensionBrawl.Test;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace DimensionBrawl.Editor
{
    public static class ActionFoundationPromotedSummonReviewContractSetup
    {
        public static void ApplyToActiveScene()
        {
            ApplyToRoot(null);
        }

        public static void ValidateActiveScene()
        {
            ValidateRoot(null);
        }

        public static void ApplyToRoot(GameObject playerRoot)
        {
            PlayerSummonSlot1Action summonSlot1Action =
                RequireComponentInScope<PlayerSummonSlot1Action>(playerRoot, "player SummonSlot1 action");
            PlayerSupportSummonSlotAction summonSlot2Action =
                RequireSupportSummonSlotAction(playerRoot, BossBarrageSummonReviewContract.Slot2ActionName);
            PlayerSupportSummonSlotAction summonSlot3Action =
                RequireSupportSummonSlotAction(playerRoot, BossBarrageSummonReviewContract.Slot3ActionName);

            summonSlot1Action.ConfigureRequiredSummonMana(BossBarrageSummonReviewContract.Slot1RequiredMana);
            summonSlot1Action.ConfigureSlotCooldown(BossBarrageSummonReviewContract.Slot1CooldownSeconds);
            summonSlot1Action.ConfigureSummonActionProfile(
                LoadAsset<SummonSlotActionProfile>(ActionFoundationBossBarrageLaneReviewSetup.SummonSlot1ActionProfilePath));
            MarkDirty(summonSlot1Action);

            summonSlot2Action.ConfigureSlot(
                BossBarrageSummonReviewContract.Slot2ActionName,
                Key.Digit2,
                new Vector2(-1.55f, 0.35f));
            summonSlot2Action.ConfigureRequiredSummonMana(BossBarrageSummonReviewContract.Slot2RequiredMana);
            summonSlot2Action.ConfigureMinimumSummonTier(BossBarrageSummonReviewContract.Slot2MinimumTier);
            summonSlot2Action.ConfigureSlotCooldown(BossBarrageSummonReviewContract.Slot2CooldownSeconds);
            summonSlot2Action.ConfigureSummonActionProfile(
                LoadAsset<SummonSlotActionProfile>(ActionFoundationBossBarrageLaneReviewSetup.SummonSlot2ActionProfilePath));
            MarkDirty(summonSlot2Action);

            summonSlot3Action.ConfigureSlot(
                BossBarrageSummonReviewContract.Slot3ActionName,
                Key.Digit3,
                new Vector2(1.55f, 0.55f));
            summonSlot3Action.ConfigureRequiredSummonMana(BossBarrageSummonReviewContract.Slot3RequiredMana);
            summonSlot3Action.ConfigureMinimumSummonTier(BossBarrageSummonReviewContract.Slot3MinimumTier);
            summonSlot3Action.ConfigureSlotCooldown(BossBarrageSummonReviewContract.Slot3CooldownSeconds);
            summonSlot3Action.ConfigureSummonActionProfile(
                LoadAsset<SummonSlotActionProfile>(ActionFoundationBossBarrageLaneReviewSetup.SummonSlot3ActionProfilePath));
            MarkDirty(summonSlot3Action);

        }

        public static void ValidateRoot(GameObject playerRoot)
        {
            PlayerSummonSlot1Action summonSlot1Action =
                RequireComponentInScope<PlayerSummonSlot1Action>(playerRoot, "player SummonSlot1 action");
            PlayerSupportSummonSlotAction summonSlot2Action =
                RequireSupportSummonSlotAction(playerRoot, BossBarrageSummonReviewContract.Slot2ActionName);
            PlayerSupportSummonSlotAction summonSlot3Action =
                RequireSupportSummonSlotAction(playerRoot, BossBarrageSummonReviewContract.Slot3ActionName);

            ValidateFloat(
                summonSlot1Action,
                "requiredSummonMana",
                BossBarrageSummonReviewContract.Slot1RequiredMana);
            ValidateFloat(
                summonSlot1Action,
                "slotCooldownSeconds",
                BossBarrageSummonReviewContract.Slot1CooldownSeconds);
            ValidateObjectReference(
                summonSlot1Action,
                "summonActionProfile",
                LoadAsset<SummonSlotActionProfile>(ActionFoundationBossBarrageLaneReviewSetup.SummonSlot1ActionProfilePath));

            ValidateSupportSummonSlotAction(
                summonSlot2Action,
                BossBarrageSummonReviewContract.Slot2,
                ActionFoundationBossBarrageLaneReviewSetup.SummonSlot2ActionProfilePath);
            ValidateSupportSummonSlotAction(
                summonSlot3Action,
                BossBarrageSummonReviewContract.Slot3,
                ActionFoundationBossBarrageLaneReviewSetup.SummonSlot3ActionProfilePath);

        }

        private static void ValidateSupportSummonSlotAction(
            PlayerSupportSummonSlotAction action,
            BossBarrageSummonSlotReviewContract contract,
            string actionProfilePath)
        {
            ValidateString(action, "slotActionName", contract.ActionName);
            ValidateInt(action, "minimumSummonTier", contract.MinimumTier);
            ValidateFloat(action, "requiredSummonMana", contract.RequiredMana);
            ValidateFloat(action, "slotCooldownSeconds", contract.CooldownSeconds);
            ValidateObjectReference(
                action,
                "summonActionProfile",
                LoadAsset<SummonSlotActionProfile>(actionProfilePath));
        }

        private static PlayerSupportSummonSlotAction RequireSupportSummonSlotAction(
            GameObject playerRoot,
            string slotActionName)
        {
            PlayerSupportSummonSlotAction[] matches = playerRoot != null
                ? playerRoot.GetComponents<PlayerSupportSummonSlotAction>()
                : UnityEngine.Object.FindObjectsByType<PlayerSupportSummonSlotAction>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None);

            for (int i = 0; i < matches.Length; i++)
            {
                if (matches[i] != null && string.Equals(matches[i].SlotActionName, slotActionName, StringComparison.Ordinal))
                {
                    return matches[i];
                }
            }

            throw new InvalidOperationException($"Scene is missing support summon action {slotActionName}.");
        }

        private static T RequireComponentInScope<T>(GameObject root, string label) where T : Component
        {
            T component = root != null
                ? root.GetComponent<T>()
                : FindSingleComponentInActiveScene<T>();
            if (component == null)
            {
                throw new InvalidOperationException($"Scene is missing {label}.");
            }

            return component;
        }

        private static T FindSingleComponentInActiveScene<T>() where T : Component
        {
            Scene scene = SceneManager.GetActiveScene();
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                T component = roots[i].GetComponentInChildren<T>(includeInactive: true);
                if (component != null)
                {
                    return component;
                }
            }

            return null;
        }

        private static T LoadAsset<T>(string assetPath) where T : UnityEngine.Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (asset == null)
            {
                throw new InvalidOperationException($"Missing required asset at {assetPath}.");
            }

            return asset;
        }

        private static void ValidateObjectReference(
            UnityEngine.Object target,
            string propertyName,
            UnityEngine.Object expected)
        {
            UnityEngine.Object actual = RequireProperty(new SerializedObject(target), propertyName).objectReferenceValue;
            if (actual != expected)
            {
                string expectedName = expected != null ? expected.name : "null";
                string actualName = actual != null ? actual.name : "null";
                throw new InvalidOperationException(
                    $"{target.name}.{propertyName} expected {expectedName}, found {actualName}.");
            }
        }

        private static void ValidateString(UnityEngine.Object target, string propertyName, string expected)
        {
            string actual = RequireProperty(new SerializedObject(target), propertyName).stringValue;
            if (!string.Equals(actual, expected, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"{target.name}.{propertyName} expected {expected}, found {actual}.");
            }
        }

        private static void ValidateInt(UnityEngine.Object target, string propertyName, int expected)
        {
            int actual = RequireProperty(new SerializedObject(target), propertyName).intValue;
            if (actual != expected)
            {
                throw new InvalidOperationException(
                    $"{target.name}.{propertyName} expected {expected}, found {actual}.");
            }
        }

        private static void ValidateFloat(UnityEngine.Object target, string propertyName, float expected)
        {
            float actual = RequireProperty(new SerializedObject(target), propertyName).floatValue;
            if (Mathf.Abs(actual - expected) > 0.001f)
            {
                throw new InvalidOperationException(
                    $"{target.name}.{propertyName} expected {expected}, found {actual}.");
            }
        }

        private static SerializedProperty RequireProperty(SerializedObject serializedObject, string propertyName)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                throw new InvalidOperationException(
                    $"{serializedObject.targetObject.name} is missing serialized property {propertyName}.");
            }

            return property;
        }

        private static void MarkDirty(UnityEngine.Object target)
        {
            EditorUtility.SetDirty(target);
            if (target is Component component)
            {
                EditorUtility.SetDirty(component.gameObject);
                PrefabUtility.RecordPrefabInstancePropertyModifications(component);
            }
        }
    }
}
