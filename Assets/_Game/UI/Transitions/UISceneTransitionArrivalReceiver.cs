using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DimensionBrawl.UI
{
    [DefaultExecutionOrder(-9000)]
    [DisallowMultipleComponent]
    public sealed class UISceneTransitionArrivalReceiver : MonoBehaviour
    {
        private static int claimedOwnerInstanceId;
        private static uint claimedGeneration;
        private static int claimedSceneHandle;

        private Coroutine readinessRoutine;
        private UISceneTransitionTicket observedTicket;
        private int observedSceneHandle;

        public uint ObservedGeneration => observedTicket.Generation;
        public bool HasCrossedRenderLayoutBoundary { get; private set; }
        public bool ReadySignalAttempted { get; private set; }
        public bool ReadySignalAccepted { get; private set; }
        public int ReadySignalAttemptCount { get; private set; }
        public int LastObservedReadinessSourceCount { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            claimedOwnerInstanceId = 0;
            claimedGeneration = 0;
            claimedSceneHandle = 0;
        }

        private void OnEnable()
        {
            TryBeginObservation();
        }

        private void OnDisable()
        {
            if (readinessRoutine != null)
            {
                StopCoroutine(readinessRoutine);
                readinessRoutine = null;
            }
        }

        private void TryBeginObservation()
        {
            if (readinessRoutine != null)
            {
                return;
            }

            UISceneTransitionHandoffOwner owner = UISceneTransitionHandoffOwner.CurrentOwner;
            if (owner == null || !owner.HasActiveTicket)
            {
                return;
            }

            Scene scene = gameObject.scene;
            UISceneTransitionTicket ticket = owner.ActiveTicket;
            if (!IsExpectedDestination(ticket, scene))
            {
                return;
            }

            observedTicket = ticket;
            observedSceneHandle = scene.handle;
            HasCrossedRenderLayoutBoundary = false;
            ReadySignalAttempted = false;
            ReadySignalAccepted = false;
            ReadySignalAttemptCount = 0;
            LastObservedReadinessSourceCount = 0;
            readinessRoutine = StartCoroutine(WaitForSceneReadiness(owner, ticket, scene));
        }

        private IEnumerator WaitForSceneReadiness(
            UISceneTransitionHandoffOwner owner,
            UISceneTransitionTicket ticket,
            Scene scene)
        {
            yield return null;
            if (!IsCurrentObservation(owner, ticket, scene))
            {
                readinessRoutine = null;
                yield break;
            }

            Canvas.ForceUpdateCanvases();
            HasCrossedRenderLayoutBoundary = true;

            while (IsCurrentObservation(owner, ticket, scene))
            {
                Scene activeScene = SceneManager.GetActiveScene();
                if (!owner.HasDestinationArrived
                    || !activeScene.IsValid()
                    || activeScene.handle != scene.handle
                    || !AreAllSceneReadinessSourcesReady(scene))
                {
                    yield return null;
                    continue;
                }

                if (!TryClaimReadySignal(ticket, scene))
                {
                    readinessRoutine = null;
                    yield break;
                }

                ReadySignalAttempted = true;
                ReadySignalAttemptCount++;
                ReadySignalAccepted = UISceneTransitionHandoffOwner.TryMarkCurrentDestinationReady(scene);
                readinessRoutine = null;
                yield break;
            }

            readinessRoutine = null;
        }

        private bool AreAllSceneReadinessSourcesReady(Scene scene)
        {
            int sourceCount = 0;
            GameObject[] roots = scene.GetRootGameObjects();
            for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
            {
                MonoBehaviour[] behaviours = roots[rootIndex].GetComponentsInChildren<MonoBehaviour>(true);
                for (int behaviourIndex = 0; behaviourIndex < behaviours.Length; behaviourIndex++)
                {
                    MonoBehaviour behaviour = behaviours[behaviourIndex];
                    if (behaviour is not IUISceneTransitionReadinessSource source)
                    {
                        continue;
                    }

                    sourceCount++;
                    if (!source.IsSceneTransitionReady)
                    {
                        LastObservedReadinessSourceCount = sourceCount;
                        return false;
                    }
                }
            }

            LastObservedReadinessSourceCount = sourceCount;
            return true;
        }

        private bool IsCurrentObservation(
            UISceneTransitionHandoffOwner owner,
            UISceneTransitionTicket ticket,
            Scene scene)
        {
            return isActiveAndEnabled
                && owner != null
                && UISceneTransitionHandoffOwner.CurrentOwner == owner
                && owner.HasActiveTicket
                && owner.ActiveTicket == ticket
                && scene.IsValid()
                && scene.isLoaded
                && scene.handle == observedSceneHandle
                && gameObject.scene.handle == observedSceneHandle
                && IsExpectedDestination(ticket, scene);
        }

        private static bool TryClaimReadySignal(UISceneTransitionTicket ticket, Scene scene)
        {
            if (claimedOwnerInstanceId == ticket.OwnerInstanceId
                && claimedGeneration == ticket.Generation
                && claimedSceneHandle == scene.handle)
            {
                return false;
            }

            claimedOwnerInstanceId = ticket.OwnerInstanceId;
            claimedGeneration = ticket.Generation;
            claimedSceneHandle = scene.handle;
            return true;
        }

        private static bool IsExpectedDestination(UISceneTransitionTicket ticket, Scene scene)
        {
            if (!ticket.IsValid
                || !scene.IsValid()
                || !scene.isLoaded
                || !string.Equals(
                    ticket.DestinationSceneName,
                    scene.name,
                    StringComparison.Ordinal))
            {
                return false;
            }

            string expectedPath = NormalizeScenePath(ticket.DestinationScenePath);
            return string.IsNullOrEmpty(expectedPath)
                || string.Equals(
                    expectedPath,
                    NormalizeScenePath(scene.path),
                    StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeScenePath(string path)
        {
            return string.IsNullOrWhiteSpace(path)
                ? string.Empty
                : path.Trim().Replace('\\', '/');
        }

#if UNITY_INCLUDE_TESTS
        public static void ResetForTests()
        {
            ResetStaticState();
        }
#endif
    }
}
