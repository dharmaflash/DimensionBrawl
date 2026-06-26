using System.Collections;
using System.Reflection;
using DimensionBrawl.UI;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace DimensionBrawl.Tests
{
    public sealed class ActionFoundationFireReticleSceneContractTests
    {
        private static readonly string[] ScenePaths =
        {
            "Assets/_Game/Scenes/ActionFoundationBossBarrageLaneReview.unity",
            "Assets/_Game/Scenes/ActionFoundationFrontlineMotivationReview.unity",
            "Assets/_Game/Scenes/ActionFoundationBossSummonDuelReview.unity"
        };

        [UnityTest]
        public IEnumerator CombatReviewFireReticlesStayAtInputHeight()
        {
            for (int i = 0; i < ScenePaths.Length; i++)
            {
                EditorSceneManager.LoadSceneInPlayMode(ScenePaths[i], new LoadSceneParameters(LoadSceneMode.Single));
                yield return null;

                BossBarrageLaneReviewMobileHud mobileHud = RequireObject<BossBarrageLaneReviewMobileHud>();
                Assert.IsTrue(
                    GetBool(mobileHud, "fireAimReticleUsesScreenCenter"),
                    $"{ScenePaths[i]} should keep the fire reticle at the input crosshair height when target assist is acquired.");
                Assert.IsFalse(
                    GetBool(mobileHud, "fireAimReticleFollowsAssist"),
                    $"{ScenePaths[i]} should show assist through reticle emphasis, not by moving the input reticle to target height.");
            }
        }

        private static T RequireObject<T>() where T : Component
        {
            T[] found = Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Assert.AreEqual(1, found.Length, $"Expected exactly one {typeof(T).Name} in the loaded scene.");
            return found[0];
        }

        private static bool GetBool(Object target, string fieldName)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field, $"{target.name} is missing private field {fieldName}.");
            return (bool)field.GetValue(target);
        }
    }
}
