using System;
using System.Reflection;
using DimensionBrawl.Player;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace DimensionBrawl.Tests
{
    public sealed class CombatHudActionTruthPlayModeTests
    {
        private const string CombatHudPrefabPath =
            "Assets/_Game/UI/CombatHud/PF_UI_CombatHud.prefab";

        [Test]
        public void SkillReadoutTracksTheActualTutorialInputLock()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CombatHudPrefabPath);
            Assert.That(prefab, Is.Not.Null);
            GameObject instance = UnityEngine.Object.Instantiate(prefab);
            GameObject player = new GameObject("Combat HUD Truth Player");
            try
            {
                Type presenterType = RequireProductType("DimensionBrawl.UI.CombatHudPresenter");
                Type binderType = RequireProductType("DimensionBrawl.UI.BossBarrageLaneReviewCombatHudBinder");
                Component presenter = instance.GetComponentInChildren(presenterType, includeInactive: true);
                Assert.That(presenter, Is.Not.Null);
                Component binder = instance.AddComponent(binderType);
                PlayerSkill1Action skill = player.AddComponent<PlayerSkill1Action>();
                SetPrivateField(binder, "hudPresenter", presenter);
                SetPrivateField(binder, "skill1Action", skill);

                Transform skillButton = RequireUniqueNamedTransform(instance, "Skill1Button");
                CanvasGroup canvasGroup = skillButton.GetComponent<CanvasGroup>();
                Text label = RequireUniqueNamedText(skillButton, "Label");
                Assert.That(canvasGroup, Is.Not.Null);

                skill.SetCinematicInputLocked(PlayerInputLockSource.CorridorTutorial, true);
                RequireMethod(binderType, "RefreshHudNow").Invoke(binder, null);
                Assert.That(label.text, Is.EqualTo("LOCKED"));
                Assert.That(canvasGroup.alpha, Is.EqualTo(0.45f).Within(0.001f));
                Assert.That(canvasGroup.interactable, Is.False);
                Assert.That(canvasGroup.blocksRaycasts, Is.False);

                skill.SetCinematicInputLocked(PlayerInputLockSource.CorridorTutorial, false);
                RequireMethod(binderType, "RefreshHudNow").Invoke(binder, null);
                Assert.That(label.text, Is.EqualTo("SKILL"));
                Assert.That(canvasGroup.alpha, Is.EqualTo(0.65f).Within(0.001f));
                Assert.That(canvasGroup.interactable, Is.True);
                Assert.That(canvasGroup.blocksRaycasts, Is.True);

                skill.enabled = false;
                RequireMethod(binderType, "RefreshHudNow").Invoke(binder, null);
                Assert.That(label.text, Is.EqualTo("LOCKED"),
                    "The tutorial disables optional actions at the component boundary.");
                Assert.That(canvasGroup.alpha, Is.EqualTo(0.45f).Within(0.001f));
                Assert.That(canvasGroup.interactable, Is.False);
                Assert.That(canvasGroup.blocksRaycasts, Is.False);

                skill.enabled = true;
                RequireMethod(binderType, "RefreshHudNow").Invoke(binder, null);
                Assert.That(label.text, Is.EqualTo("SKILL"));
                Assert.That(canvasGroup.interactable, Is.True);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(player);
                UnityEngine.Object.DestroyImmediate(instance);
            }
        }

        private static Type RequireProductType(string fullName)
        {
            Type type = Type.GetType($"{fullName}, Assembly-CSharp", throwOnError: false);
            Assert.That(type, Is.Not.Null, $"Missing product type {fullName}.");
            return type;
        }

        private static MethodInfo RequireMethod(Type type, string methodName)
        {
            MethodInfo method = type.GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, $"Missing {type.FullName}.{methodName}.");
            return method;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"Missing field {fieldName}.");
            field.SetValue(target, value);
        }

        private static Transform RequireUniqueNamedTransform(GameObject root, string objectName)
        {
            Transform match = null;
            int count = 0;
            foreach (Transform candidate in root.GetComponentsInChildren<Transform>(includeInactive: true))
            {
                if (candidate.name != objectName)
                {
                    continue;
                }

                match = candidate;
                count++;
            }

            Assert.That(count, Is.EqualTo(1), $"Expected one {objectName}.");
            return match;
        }

        private static Text RequireUniqueNamedText(Transform root, string objectName)
        {
            Text match = null;
            int count = 0;
            foreach (Text candidate in root.GetComponentsInChildren<Text>(includeInactive: true))
            {
                if (candidate.name != objectName)
                {
                    continue;
                }

                match = candidate;
                count++;
            }

            Assert.That(count, Is.EqualTo(1), $"Expected one {objectName} under {root.name}.");
            return match;
        }
    }
}
