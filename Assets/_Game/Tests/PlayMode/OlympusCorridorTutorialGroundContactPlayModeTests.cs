using System.Collections;
using System.Reflection;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.Player;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace DimensionBrawl.Tests
{
    public sealed class OlympusCorridorTutorialGroundContactPlayModeTests
    {
        private const string ScenePath = "Assets/_Game/Scenes/OlympusCorridorInvasionStage.unity";
        private const string FlowRootName = "OlympusCorridor_CombatFlowRoot";
        private const string PlayerRootName = "Player_CombatGirl_ActionFoundation";
        private const float GroundTolerance = 0.02f;
        private const float RootSpanTolerance = 0.01f;

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            Time.timeScale = 1f;
            EditorSceneManager.LoadSceneInPlayMode(ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator ResetInputAndTimeScale()
        {
            PlayerMovementController player = FindComponent<PlayerMovementController>(PlayerRootName);
            player?.SetMoveInput(Vector2.zero);
            Time.timeScale = 1f;
            yield return null;
        }

        [UnityTest]
        [Timeout(30000)]
        public IEnumerator MoveLessonStopStepKeepsPlayerPresentationAboveGround()
        {
            OlympusCorridorCombatFlowController flow = RequireComponent<OlympusCorridorCombatFlowController>(
                FlowRootName);
            PlayerMovementController player = RequireComponent<PlayerMovementController>(PlayerRootName);
            PlayerActionController action = RequireComponent<PlayerActionController>(PlayerRootName);
            PlayerCombatModeController mode = RequireComponent<PlayerCombatModeController>(PlayerRootName);
            CharacterController characterController = player.GetComponent<CharacterController>();
            Animator animator = ReadPrivateField<Animator>(mode, "rangedAnimator");
            Assert.IsNotNull(characterController);
            Assert.IsNotNull(animator);
            Assert.IsTrue(animator.isHuman, "The canonical single-character body must use a Humanoid Animator.");

            flow.SkipIntroCutscene();
            yield return null;
            yield return null;

            OlympusCorridorTutorialDirector tutorial = RequireComponent<OlympusCorridorTutorialDirector>(FlowRootName);
            yield return WaitForStep(tutorial, "Melee", 5f);
            yield return WaitForPhase(tutorial, "AwaitingAction", 5f);
            yield return DriveUntilStep(tutorial, "Move", 8f, action.QueueBasicAttack);
            yield return WaitForPhase(tutorial, "AwaitingAction", 5f);

            Transform stairTarget = ReadPrivateField<Transform>(flow, "stairTriggerCenter");
            Camera movementCamera = ReadPrivateField<Camera>(player, "referenceCamera");
            Assert.IsNotNull(stairTarget);
            Assert.IsNotNull(movementCamera);

            float moveDeadline = Time.realtimeSinceStartup + 8f;
            while (tutorial.CurrentPhaseId == "AwaitingAction")
            {
                player.SetMoveInput(ResolveCameraInput(player.transform.position, stairTarget.position, movementCamera));
                yield return null;
                Assert.Less(Time.realtimeSinceStartup, moveDeadline, "Move lesson did not commit from live input.");
            }

            player.SetMoveInput(Vector2.zero);
            yield return WaitForAnimatorState(animator, "StopStep", 3f);

            Transform leftFoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            Transform rightFoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);
            Assert.IsNotNull(leftFoot);
            Assert.IsNotNull(rightFoot);

            float minimumRootY = float.PositiveInfinity;
            float maximumRootY = float.NegativeInfinity;
            float minimumControllerBottomY = float.PositiveInfinity;
            float maximumControllerBottomY = float.NegativeInfinity;
            int stopStepFrames = 0;
            float stopStepDeadline = Time.realtimeSinceStartup + 3f;
            while (animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.StopStep"))
            {
                float groundY = ResolveGroundY(player.transform);
                float rootY = player.transform.position.y;
                float controllerBottomY = player.transform.TransformPoint(characterController.center).y
                    - characterController.height * Mathf.Abs(player.transform.lossyScale.y) * 0.5f;
                minimumRootY = Mathf.Min(minimumRootY, rootY);
                maximumRootY = Mathf.Max(maximumRootY, rootY);
                minimumControllerBottomY = Mathf.Min(minimumControllerBottomY, controllerBottomY);
                maximumControllerBottomY = Mathf.Max(maximumControllerBottomY, controllerBottomY);

                Assert.That(
                    leftFoot.position.y,
                    Is.GreaterThanOrEqualTo(groundY - GroundTolerance),
                    "StopStep must not pull the left foot beneath the floor.");
                Assert.That(
                    rightFoot.position.y,
                    Is.GreaterThanOrEqualTo(groundY - GroundTolerance),
                    "StopStep must not pull the right foot beneath the floor.");
                Assert.That(
                    ResolveActiveRendererMinimumY(player.gameObject),
                    Is.GreaterThanOrEqualTo(groundY - GroundTolerance),
                    "StopStep must not pull the visible body or shoes beneath the floor.");

                stopStepFrames++;
                yield return null;
                Assert.Less(Time.realtimeSinceStartup, stopStepDeadline, "StopStep did not return to Idle.");
            }

            Assert.That(stopStepFrames, Is.GreaterThan(1), "The test must sample the authored StopStep state.");
            Assert.That(maximumRootY - minimumRootY, Is.LessThanOrEqualTo(RootSpanTolerance));
            Assert.That(
                maximumControllerBottomY - minimumControllerBottomY,
                Is.LessThanOrEqualTo(RootSpanTolerance));
            Assert.IsTrue(
                animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Idle"),
                "The grounded stop presentation must return to Idle.");
        }

        private static IEnumerator DriveUntilStep(
            OlympusCorridorTutorialDirector tutorial,
            string expectedStep,
            float timeoutSeconds,
            System.Action input)
        {
            float deadline = Time.realtimeSinceStartup + timeoutSeconds;
            while (tutorial.CurrentStepId != expectedStep)
            {
                input();
                yield return null;
                Assert.Less(Time.realtimeSinceStartup, deadline, $"Timed out waiting for {expectedStep}.");
            }
        }

        private static IEnumerator WaitForStep(
            OlympusCorridorTutorialDirector tutorial,
            string expectedStep,
            float timeoutSeconds)
        {
            float deadline = Time.realtimeSinceStartup + timeoutSeconds;
            while (tutorial.CurrentStepId != expectedStep)
            {
                yield return null;
                Assert.Less(Time.realtimeSinceStartup, deadline, $"Timed out waiting for {expectedStep}.");
            }
        }

        private static IEnumerator WaitForPhase(
            OlympusCorridorTutorialDirector tutorial,
            string expectedPhase,
            float timeoutSeconds)
        {
            float deadline = Time.realtimeSinceStartup + timeoutSeconds;
            while (tutorial.CurrentPhaseId != expectedPhase)
            {
                yield return null;
                Assert.Less(Time.realtimeSinceStartup, deadline, $"Timed out waiting for {expectedPhase}.");
            }
        }

        private static IEnumerator WaitForAnimatorState(Animator animator, string stateName, float timeoutSeconds)
        {
            string fullStateName = "Base Layer." + stateName;
            float deadline = Time.realtimeSinceStartup + timeoutSeconds;
            while (!animator.GetCurrentAnimatorStateInfo(0).IsName(fullStateName))
            {
                yield return null;
                Assert.Less(Time.realtimeSinceStartup, deadline, $"Animator did not enter {stateName}.");
            }
        }

        private static float ResolveActiveRendererMinimumY(GameObject playerRoot)
        {
            float minimumY = float.PositiveInfinity;
            SkinnedMeshRenderer[] renderers = playerRoot.GetComponentsInChildren<SkinnedMeshRenderer>(false);
            for (int i = 0; i < renderers.Length; i++)
            {
                SkinnedMeshRenderer renderer = renderers[i];
                if (renderer.enabled && renderer.gameObject.activeInHierarchy)
                {
                    minimumY = Mathf.Min(minimumY, renderer.bounds.min.y);
                }
            }

            Assert.IsFalse(float.IsPositiveInfinity(minimumY), "The player needs an active body renderer.");
            return minimumY;
        }

        private static float ResolveGroundY(Transform player)
        {
            Vector3 origin = player.position + Vector3.up * 2.5f;
            RaycastHit[] hits = Physics.RaycastAll(origin, Vector3.down, 8f, ~0, QueryTriggerInteraction.Ignore);
            float closestDistance = float.PositiveInfinity;
            float groundY = float.NaN;
            for (int i = 0; i < hits.Length; i++)
            {
                Transform hitTransform = hits[i].collider.transform;
                if (hitTransform == player || hitTransform.IsChildOf(player))
                {
                    continue;
                }

                if (hits[i].distance < closestDistance)
                {
                    closestDistance = hits[i].distance;
                    groundY = hits[i].point.y;
                }
            }

            Assert.IsFalse(float.IsNaN(groundY), "The tutorial player needs authored ground beneath it.");
            return groundY;
        }

        private static Vector2 ResolveCameraInput(Vector3 playerPosition, Vector3 targetPosition, Camera camera)
        {
            Vector3 direction = Vector3.ProjectOnPlane(targetPosition - playerPosition, Vector3.up).normalized;
            Vector3 forward = Vector3.ProjectOnPlane(camera.transform.forward, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(camera.transform.right, Vector3.up).normalized;
            return Vector2.ClampMagnitude(
                new Vector2(Vector3.Dot(direction, right), Vector3.Dot(direction, forward)),
                1f);
        }

        private static T RequireComponent<T>(string objectName) where T : Component
        {
            T component = FindComponent<T>(objectName);
            Assert.IsNotNull(component, $"Missing {typeof(T).Name} on {objectName}.");
            return component;
        }

        private static T FindComponent<T>(string objectName) where T : Component
        {
            GameObject target = FindSceneObjectIncludingInactive(objectName);
            return target != null ? target.GetComponent<T>() : null;
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

        private static T ReadPrivateField<T>(object target, string fieldName)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Missing field {fieldName} on {target.GetType().Name}.");
            return (T)field.GetValue(target);
        }
    }
}
