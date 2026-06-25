using System;
using System.Collections;
using System.Reflection;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.Presentation;
using DimensionBrawl.Test;
using DimensionBrawl.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace DimensionBrawl.Tests
{
    public sealed class ActionFoundationFrontlineMotivationReviewSceneTests
    {
        private const string ScenePath =
            "Assets/_Game/Scenes/ActionFoundationFrontlineMotivationReview.unity";
        private const string StageProfilePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_FrontlineWaveStage_MotivationReview.asset";
        private const string PocketOwnerRootName = "BossBarrageLaneReview_PocketOwner";
        private const string HudRootName = "BossBarrageLaneReview_DebugHud";

        [UnityTest]
        public IEnumerator FrontlineMotivationReviewScenePreservesRouteContract()
        {
            EditorSceneManager.LoadSceneInPlayMode(ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;

            Assert.AreEqual(ScenePath, SceneManager.GetActiveScene().path);

            FrontlineWaveStageProfile stageProfile =
                AssetDatabase.LoadAssetAtPath<FrontlineWaveStageProfile>(StageProfilePath);
            Assert.NotNull(stageProfile);
            Assert.AreEqual("FRONTLINE-MOTIVATION-REVIEW-01", stageProfile.StageId);
            Assert.AreEqual(90f, stageProfile.TargetDurationSeconds);
            Assert.GreaterOrEqual(stageProfile.BeatCount, 6);
            Assert.GreaterOrEqual(stageProfile.SourceReferenceCount, 3);

            BossBarragePocketReviewOwner pocketOwner =
                RequireComponent<BossBarragePocketReviewOwner>(RequireRoot(PocketOwnerRootName), "pocket owner");
            BossBarrageLaneReviewHud reviewHud =
                RequireComponent<BossBarrageLaneReviewHud>(RequireRoot(HudRootName), "review HUD");
            BossBarrageLaneReviewOverlayHud overlayHud =
                RequireComponent<BossBarrageLaneReviewOverlayHud>(RequireRoot(HudRootName), "overlay HUD");

            Assert.AreSame(stageProfile, GetObjectReference<FrontlineWaveStageProfile>(pocketOwner, "stageProfile"));
            Assert.AreSame(stageProfile, GetObjectReference<FrontlineWaveStageProfile>(reviewHud, "stageProfile"));
            Assert.AreEqual(stageProfile.ObjectiveStepCount, pocketOwner.ObjectiveStepCount);
            Assert.AreEqual("ActionFoundationFrontlineMotivationReview", overlayHud.RetrySceneName);
            Assert.AreEqual(ScenePath, overlayHud.RetryScenePath);

            Assert.That(reviewHud.CompactObjectiveReadout, Does.Contain("Route 1/3"));
            Assert.That(reviewHud.CompactObjectiveReadout, Does.Not.Contain("Boss"));
            Assert.That(pocketOwner.ObjectiveCue, Does.Contain("line").IgnoreCase);

            ForcePocketState(pocketOwner, "Cleared");
            SetField(pocketOwner, "closeThreatDefeated", true);
            SetField(pocketOwner, "usedSummonSlot1", true);
            SetField(pocketOwner, "blockedBossPressureWithSummon", true);
            SetField(pocketOwner, "skill1FollowupHitConfirmed", true);
            SetField(pocketOwner, "skill1FollowupDamage", 123f);
            SetField(pocketOwner, "resultElapsedSeconds", 42f);

            Assert.IsTrue(reviewHud.ShouldShowResultBanner);
            Assert.AreEqual("FRONTLINE STABILIZED", reviewHud.ResultBannerTitle);
            Assert.That(reviewHud.ResultBannerDetail, Does.Contain("Summon route analyzed"));
            Assert.That(reviewHud.ResultBannerDetail, Does.Contain("Route 3/3"));
            Assert.That(reviewHud.ResultBannerDetail, Does.Not.Contain("BOSS CLEAR"));
        }

        private static GameObject RequireRoot(string objectName)
        {
            GameObject root = GameObject.Find(objectName);
            Assert.NotNull(root, $"Missing scene object {objectName}.");
            return root;
        }

        private static T RequireComponent<T>(GameObject gameObject, string label) where T : Component
        {
            T component = gameObject.GetComponent<T>();
            Assert.NotNull(component, $"{gameObject.name} is missing {label}.");
            return component;
        }

        private static T GetObjectReference<T>(UnityEngine.Object target, string fieldName)
            where T : UnityEngine.Object
        {
            FieldInfo field = RequireField(target.GetType(), fieldName);
            return field.GetValue(target) as T;
        }

        private static void ForcePocketState(BossBarragePocketReviewOwner owner, string stateName)
        {
            Type stateType = typeof(BossBarragePocketReviewOwner).GetNestedType(
                "PocketState",
                BindingFlags.NonPublic);
            Assert.NotNull(stateType, "PocketState enum is missing.");
            object value = Enum.Parse(stateType, stateName);
            SetField(owner, "state", value);
        }

        private static void SetField(UnityEngine.Object target, string fieldName, object value)
        {
            FieldInfo field = RequireField(target.GetType(), fieldName);
            field.SetValue(target, value);
        }

        private static FieldInfo RequireField(Type type, string fieldName)
        {
            FieldInfo field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field, $"{type.Name} is missing private field {fieldName}.");
            return field;
        }
    }
}
