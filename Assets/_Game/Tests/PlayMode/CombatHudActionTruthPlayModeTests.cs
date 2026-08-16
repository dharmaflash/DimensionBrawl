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
        private static readonly string[] SharedCombatHudPrefabPaths =
        {
            CombatHudPrefabPath,
            "Assets/_Game/UI/CombatHud/PF_UI_CombatHud_CelestialV2_Staging.prefab",
            "Assets/_Game/UI/CombatHud/PF_UI_CombatHud_CelestialTarget_Staging.prefab"
        };
        private static readonly string[] CombatHudAssemblerPaths =
        {
            "Assets/_Game/Editor/CombatHud/CombatHudCelestialV2PrefabAssembler.cs",
            "Assets/_Game/Editor/CombatHud/CombatHudCelestialTargetPrefabAssembler.cs"
        };
        private static readonly string[] EarlyObjectiveCopy =
        {
            "근접 위협을 먼저 처치하세요",
            "소환 에너지를 충전하세요",
            "소환으로 탄막을 막으세요"
        };

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

        [Test]
        public void SharedBossHeaderIsNeutralAndLaneBinderAppliesStageDisplayName()
        {
            for (int i = 0; i < SharedCombatHudPrefabPaths.Length; i++)
            {
                GameObject sharedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                    SharedCombatHudPrefabPaths[i]);
                Assert.That(sharedPrefab, Is.Not.Null, SharedCombatHudPrefabPaths[i]);
                Assert.That(
                    RequireUniqueNamedText(sharedPrefab.transform, "BossNameText").text,
                    Is.EqualTo("BOSS"),
                    $"{SharedCombatHudPrefabPaths[i]} must stay stage-neutral.");
            }

            for (int i = 0; i < CombatHudAssemblerPaths.Length; i++)
            {
                MonoScript assembler = AssetDatabase.LoadAssetAtPath<MonoScript>(
                    CombatHudAssemblerPaths[i]);
                Assert.That(assembler, Is.Not.Null, CombatHudAssemblerPaths[i]);
                Assert.That(assembler.text, Does.Contain("\"BOSS\""));
                Assert.That(assembler.text, Does.Not.Contain("ARCHON PROXY"));
            }

            GameObject instance = UnityEngine.Object.Instantiate(
                AssetDatabase.LoadAssetAtPath<GameObject>(CombatHudPrefabPath));
            try
            {
                Type presenterType = RequireProductType("DimensionBrawl.UI.CombatHudPresenter");
                Type binderType = RequireProductType(
                    "DimensionBrawl.UI.BossBarrageLaneReviewCombatHudBinder");
                Component presenter = instance.GetComponentInChildren(
                    presenterType,
                    includeInactive: true);
                Assert.That(presenter, Is.Not.Null);
                Text bossNameText = RequireUniqueNamedText(instance.transform, "BossNameText");
                MethodInfo setBossName = RequireMethod(presenterType, "SetBossName");
                Assert.That(setBossName.IsPublic, Is.True);

                setBossName.Invoke(presenter, new object[] { "STAGE BOSS" });
                Assert.That(bossNameText.text, Is.EqualTo("STAGE BOSS"));

                Component binder = instance.AddComponent(binderType);
                SetPrivateField(binder, "hudPresenter", presenter);
                FieldInfo displayNameField = binderType.GetField(
                    "bossDisplayName",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.That(displayNameField, Is.Not.Null);
                Assert.That(displayNameField.GetValue(binder), Is.EqualTo("AKAZA"));
                RequireMethod(binderType, "RefreshHudNow").Invoke(binder, null);
                Assert.That(bossNameText.text, Is.EqualTo("AKAZA"));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(instance);
            }
        }

        [Test]
        public void EarlyObjectiveCopyFitsCanonicalHudWithoutInternalTokens()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CombatHudPrefabPath);
            Assert.That(prefab, Is.Not.Null);
            GameObject instance = UnityEngine.Object.Instantiate(prefab);
            try
            {
                Type presenterType = RequireProductType("DimensionBrawl.UI.CombatHudPresenter");
                Component presenter = instance.GetComponentInChildren(
                    presenterType,
                    includeInactive: true);
                Assert.That(presenter, Is.Not.Null);
                Text objective = RequireUniqueNamedText(instance.transform, "Objective");
                MethodInfo setObjective = RequireMethod(presenterType, "SetObjective");
                Rect objectiveRect = objective.rectTransform.rect;
                Assert.That(objectiveRect.width, Is.GreaterThan(0f));
                Assert.That(objectiveRect.height, Is.GreaterThan(0f));

                for (int i = 0; i < EarlyObjectiveCopy.Length; i++)
                {
                    string copy = EarlyObjectiveCopy[i];
                    setObjective.Invoke(presenter, new object[] { copy });
                    Canvas.ForceUpdateCanvases();

                    Assert.That(objective.text, Is.EqualTo(copy));
                    Assert.That(objective.text, Does.Not.Contain("SummonSlot1"));
                    Assert.That(objective.text, Does.Not.Contain("LV"));
                    Assert.That(objective.text, Does.Not.Contain(":"));
                    Assert.That(
                        objective.preferredHeight,
                        Is.LessThanOrEqualTo(objectiveRect.height + 1f),
                        $"Objective copy is vertically clipped: {copy}");
                }
            }
            finally
            {
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
