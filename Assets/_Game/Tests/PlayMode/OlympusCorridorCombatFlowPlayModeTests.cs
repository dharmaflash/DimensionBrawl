using System.Collections;
using DimensionBrawl.LevelDesign;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace DimensionBrawl.Tests
{
    public sealed class OlympusCorridorCombatFlowPlayModeTests
    {
        private const string ScenePath = "Assets/_Game/Scenes/OlympusCorridorInvasionStage.unity";
        private const string DirectorName = "IntroGatePodReview_TimelineDirector";
        private const string FlowRootName = "OlympusCorridor_CombatFlowRoot";
        private const string PlayerRootName = "Player_CombatGirl_ActionFoundation";
        private const string IntroSwordGateRootName = "OlympusCorridor_IntroSwordGate";

        [UnitySetUp]
        public IEnumerator LoadOlympusCorridorScene()
        {
            Time.timeScale = 1f;
            EditorSceneManager.LoadSceneInPlayMode(ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator ResetTimeScale()
        {
            Time.timeScale = 1f;
            yield return null;
        }

        [UnityTest]
        public IEnumerator IntroDirectorStopAfterTimeResetStartsCombatHandoff()
        {
            PlayableDirector director =
                RequireComponent<PlayableDirector>(DirectorName, "Olympus intro PlayableDirector");
            GameObject playerRoot = RequireSceneObject(PlayerRootName);
            GameObject introSwordGateRoot = RequireSceneObject(IntroSwordGateRootName);
            OlympusCorridorCombatFlowController flowController =
                RequireComponent<OlympusCorridorCombatFlowController>(
                    FlowRootName,
                    "Olympus corridor combat flow controller");

            Assert.IsNotNull(flowController);
            Assert.IsFalse(playerRoot.activeInHierarchy, "Player should stay hidden during the intro opening.");

            director.Play();
            yield return null;

            director.time = 0d;
            director.Stop();
            yield return null;
            yield return null;

            Assert.IsTrue(playerRoot.activeSelf, "Player activeSelf should be restored after intro director stop.");
            Assert.IsTrue(playerRoot.activeInHierarchy, "Player should be visible after intro director stop.");
            Assert.IsTrue(introSwordGateRoot.activeInHierarchy, "Intro sword gate combat should start after director stop.");
        }

        [UnityTest]
        public IEnumerator IntroDirectorEndEvaluationKeepsCombatPlayerActive()
        {
            PlayableDirector director =
                RequireComponent<PlayableDirector>(DirectorName, "Olympus intro PlayableDirector");
            GameObject playerRoot = RequireSceneObject(PlayerRootName);
            GameObject introSwordGateRoot = RequireSceneObject(IntroSwordGateRootName);
            OlympusCorridorCombatFlowController flowController =
                RequireComponent<OlympusCorridorCombatFlowController>(
                    FlowRootName,
                    "Olympus corridor combat flow controller");

            Assert.IsNotNull(flowController);
            Assert.IsFalse(playerRoot.activeInHierarchy, "Player should stay hidden during the intro opening.");

            director.Play();
            yield return null;

            director.time = System.Math.Max(0d, director.duration - 0.01d);
            director.Evaluate();
            yield return null;
            yield return null;

            Assert.IsTrue(playerRoot.activeSelf, "Player root activeSelf should be true at intro tail.");
            Assert.IsTrue(playerRoot.activeInHierarchy, "Player root should be active in hierarchy at intro tail.");
            Assert.IsTrue(introSwordGateRoot.activeInHierarchy, "Intro sword gate should be active at intro tail.");

            director.time = director.duration;
            director.Evaluate();
            director.Stop();
            yield return null;
            yield return null;

            Assert.IsTrue(playerRoot.activeSelf, "Player root activeSelf should stay true after director end.");
            Assert.IsTrue(playerRoot.activeInHierarchy, "Player root should stay active after director end.");
            Assert.IsTrue(introSwordGateRoot.activeInHierarchy, "Intro sword gate should stay active after director end.");
        }

        private static T RequireComponent<T>(string objectName, string label)
            where T : Component
        {
            GameObject gameObject = RequireSceneObject(objectName);
            T component = gameObject.GetComponent<T>();
            Assert.IsNotNull(component, $"Missing {label} on {objectName}.");
            return component;
        }

        private static GameObject RequireSceneObject(string objectName)
        {
            GameObject gameObject = FindSceneObjectIncludingInactive(objectName);
            Assert.IsNotNull(gameObject, $"Missing scene object: {objectName}");
            return gameObject;
        }

        private static GameObject FindSceneObjectIncludingInactive(string objectName)
        {
            GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();
            for (int i = 0; i < objects.Length; i++)
            {
                GameObject candidate = objects[i];
                if (candidate != null
                    && candidate.scene.IsValid()
                    && string.Equals(candidate.name, objectName, System.StringComparison.Ordinal))
                {
                    return candidate;
                }
            }

            return null;
        }
    }
}
