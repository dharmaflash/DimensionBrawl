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
        };

        [UnityTest]
        public IEnumerator CombatReviewFireReticlesMatchAuthoredAimRoutes()
        {
            for (int i = 0; i < ScenePaths.Length; i++)
            {
                EditorSceneManager.LoadSceneInPlayMode(ScenePaths[i], new LoadSceneParameters(LoadSceneMode.Single));
                yield return null;

                BossBarrageLaneReviewMobileHud mobileHud = RequireObject<BossBarrageLaneReviewMobileHud>();
                bool expectsScreenCenter = !ScenePaths[i].Contains("BossBarrageLaneReview");
                bool expectsAssistFollowing = !ScenePaths[i].Contains("FrontlineMotivationReview");
                Assert.AreEqual(
                    expectsScreenCenter,
                    GetBool(mobileHud, "fireAimReticleUsesScreenCenter"),
                    $"{ScenePaths[i]} should preserve its authored pointer-versus-center fire reticle route.");
                Assert.AreEqual(
                    expectsAssistFollowing,
                    GetBool(mobileHud, "fireAimReticleFollowsAssist"),
                    $"{ScenePaths[i]} should preserve its authored aim-assist reticle route.");
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
