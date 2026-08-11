using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DimensionBrawl.UI
{
    [DefaultExecutionOrder(-10000)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UITransitionPresenter))]
    public sealed class UISceneTransitionHandoffOwner : MonoBehaviour, IUITransitionHandoffProvider
    {
        [SerializeField] private UITransitionPresenter transitionPresenter;
        [SerializeField] private int persistentSortingOrder = 30000;
        [SerializeField, Range(1f, 3f)] private float readyTimeoutSeconds = 2.75f;
        [SerializeField, HideInInspector] private bool ownsPersistentRoot;
        [SerializeField, HideInInspector] private UIRouteInteractableGate[] inputLockedGates =
            Array.Empty<UIRouteInteractableGate>();
        [SerializeField, HideInInspector] private EventSystem[] suspendedEventSystems =
            Array.Empty<EventSystem>();
        [SerializeField, HideInInspector] private bool[] suspendedEventSystemEnabled = Array.Empty<bool>();
        [SerializeField, HideInInspector] private GameObject[] suspendedEventSystemSelection =
            Array.Empty<GameObject>();
        [SerializeField, HideInInspector] private int inputLeaseOwnerInstanceId;
        [SerializeField, HideInInspector] private uint inputLeaseGeneration;
        [SerializeField, HideInInspector] private uint lastIssuedGeneration;

        private static UISceneTransitionHandoffOwner currentOwner;
        private static uint nextGeneration;

        private UISceneTransitionTicket activeTicket;
        private UISceneTransitionTicket activationTicket;
        private bool hasActiveTicket;
        private bool activationRequested;
        private bool destinationArrived;
        private int destinationSceneHandle;
        private double destinationArrivedAt;
        private Coroutine revealRoutine;
        private bool sceneLoadedSubscribed;

        public event Action<UISceneTransitionTicket, Scene> DestinationArrived;
        public event Action<UISceneTransitionTicket> HandoffCompleted;
        public event Action<UISceneTransitionTicket, string> HandoffFailed;

        public static UISceneTransitionHandoffOwner CurrentOwner => currentOwner != null
            ? currentOwner
            : null;

        public UITransitionPresenter Presenter => transitionPresenter;
        public bool HasActiveTicket => hasActiveTicket;
        public UISceneTransitionTicket ActiveTicket => hasActiveTicket ? activeTicket : default;
        public bool HasDestinationArrived => hasActiveTicket && destinationArrived;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            currentOwner = null;
            nextGeneration = 0;
        }

        private void Reset()
        {
            transitionPresenter = GetComponent<UITransitionPresenter>();
        }

        private void Awake()
        {
            ResolvePresenter();
            if (!TryClaimOwnership())
            {
                return;
            }

            PromoteOverlayRoot();
            transitionPresenter?.HideImmediate();
        }

        private void OnEnable()
        {
            bool reclaimedAfterStaticReset = false;
            if (currentOwner == null && ownsPersistentRoot)
            {
                currentOwner = this;
                if (nextGeneration < lastIssuedGeneration)
                {
                    nextGeneration = lastIssuedGeneration;
                }

                reclaimedAfterStaticReset = true;
                PromoteOverlayRoot();
            }

            if (currentOwner != this)
            {
                DestroyDuplicateRoot();
                return;
            }

            UITransitionHandoffService.TryRegisterProvider(this);
            SubscribeSceneLoaded();
            if (reclaimedAfterStaticReset)
            {
                ResetOrphanedHandoff();
            }
        }

        private void OnDisable()
        {
            UITransitionHandoffService.UnregisterProvider(this);
            UnsubscribeSceneLoaded();
        }

        private void OnDestroy()
        {
            UITransitionHandoffService.UnregisterProvider(this);
            UnsubscribeSceneLoaded();
            ForceRestoreDestinationInput();
            if (currentOwner == this)
            {
                currentOwner = null;
            }
        }

        private void Update()
        {
            if (!hasActiveTicket || !destinationArrived || revealRoutine != null)
            {
                return;
            }

            if (Time.realtimeSinceStartupAsDouble - destinationArrivedAt
                < Mathf.Max(1f, readyTimeoutSeconds))
            {
                return;
            }

            string reason =
                $"Destination scene '{activeTicket.DestinationSceneName}' did not report ready before the transition timeout.";
            FailOpen(activeTicket, reason);
        }

        public bool TryBeginRoute(
            UIScreenRouteTable.Route route,
            out UISceneTransitionTicket ticket,
            out string error)
        {
            return TryBeginRoute(route, SceneManager.GetActiveScene(), out ticket, out error);
        }

        public bool TryBeginRoute(
            UIScreenRouteTable.Route route,
            Scene sourceScene,
            out UISceneTransitionTicket ticket,
            out string error)
        {
            ticket = default;
            error = string.Empty;

            if (currentOwner != this || !ownsPersistentRoot)
            {
                error = "The transition overlay is not the persistent handoff owner.";
                return false;
            }

            if (hasActiveTicket)
            {
                error = $"Transition handoff generation {activeTicket.Generation} is still active.";
                return false;
            }

            string destinationName = route.SceneName?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(destinationName))
            {
                error = "The destination scene name is missing.";
                return false;
            }

            uint generation = NextGeneration();
            ticket = new UISceneTransitionTicket(
                GetInstanceID(),
                generation,
                route.RouteId,
                sourceScene.IsValid() ? sourceScene.handle : 0,
                destinationName,
                NormalizeScenePath(route.ScenePath));

            activeTicket = ticket;
            activationTicket = default;
            hasActiveTicket = true;
            activationRequested = false;
            destinationArrived = false;
            destinationSceneHandle = 0;
            destinationArrivedAt = 0d;
            revealRoutine = null;
            return true;
        }

        public bool TryBeginTerminalHandoff(
            UITransitionHandoffDestination destination,
            Func<UITransitionDispatchResult> dispatch,
            Action<string> failed,
            out string error)
        {
            error = string.Empty;
            if (!destination.IsValid)
            {
                error = "The terminal transition destination is invalid.";
                return false;
            }

            if (dispatch == null)
            {
                error = "The terminal transition dispatch callback is missing.";
                return false;
            }

            UIRouteId routeId;
            switch (destination.Kind)
            {
                case UITransitionDestinationKind.Lobby:
                    routeId = UIRouteId.Lobby;
                    break;
                case UITransitionDestinationKind.Combat:
                    routeId = UIRouteId.Combat;
                    break;
                default:
                    error = $"Terminal transition destination kind '{destination.Kind}' is unsupported.";
                    return false;
            }

            var route = new UIScreenRouteTable.Route(
                routeId,
                destination.SceneName,
                destination.ScenePath,
                string.Empty,
                string.Empty,
                false,
                0f);
            if (!TryBeginRoute(route, out UISceneTransitionTicket ticket, out error))
            {
                return false;
            }

            StartCoroutine(TerminalHandoffRoutine(route, ticket, dispatch, failed));
            return true;
        }

        public bool TryMarkActivationRequested(UISceneTransitionTicket ticket)
        {
            if (!IsCurrentTicket(ticket) || activationRequested)
            {
                return false;
            }

            activationTicket = ticket;
            activationRequested = true;
            return true;
        }

        public bool TryMarkDestinationArrived(UISceneTransitionTicket ticket, Scene scene)
        {
            if (!IsCurrentTicket(ticket)
                || !activationRequested
                || activationTicket != ticket
                || !IsExpectedDestination(ticket, scene))
            {
                return false;
            }

            if (destinationArrived)
            {
                return destinationSceneHandle == scene.handle;
            }

            destinationArrived = true;
            destinationSceneHandle = scene.handle;
            destinationArrivedAt = Time.realtimeSinceStartupAsDouble;
            AcquireDestinationInput(ticket, scene);
            transitionPresenter?.ShowCoveredImmediate();
            DestinationArrived?.Invoke(ticket, scene);
            return true;
        }

        public static bool TryMarkCurrentDestinationReady(Scene scene)
        {
            UISceneTransitionHandoffOwner owner = CurrentOwner;
            return owner != null && owner.TryMarkDestinationReady(owner.ActiveTicket, scene);
        }

        public static bool IsSceneInputLocked(Scene scene)
        {
            UISceneTransitionHandoffOwner owner = CurrentOwner;
            if (owner == null
                || !owner.hasActiveTicket
                || !owner.destinationArrived
                || !scene.IsValid()
                || !scene.isLoaded
                || scene.handle != owner.destinationSceneHandle
                || !owner.OwnsDestinationInput(owner.activeTicket))
            {
                return false;
            }

            return IsExpectedDestination(owner.activeTicket, scene);
        }

        bool IUITransitionHandoffProvider.IsSceneInputLocked(Scene scene)
        {
            return IsSceneInputLocked(scene);
        }

        public bool TryMarkDestinationReady(UISceneTransitionTicket ticket, Scene scene)
        {
            if (!IsCurrentTicket(ticket)
                || !destinationArrived
                || destinationSceneHandle != scene.handle
                || !IsExpectedDestination(ticket, scene))
            {
                return false;
            }

            if (revealRoutine == null)
            {
                revealRoutine = StartCoroutine(RevealDestination(ticket));
            }

            return true;
        }

        public IEnumerator FailAndRestore(UISceneTransitionTicket ticket, string reason)
        {
            if (!IsCurrentTicket(ticket))
            {
                yield break;
            }

            if (revealRoutine != null)
            {
                StopCoroutine(revealRoutine);
                revealRoutine = null;
            }

            if (transitionPresenter != null)
            {
                yield return transitionPresenter.PlayIn();
            }

            if (!IsCurrentTicket(ticket))
            {
                yield break;
            }

            string detail = NormalizeFailureReason(reason);
            RestoreDestinationInput(ticket);
            HandoffFailed?.Invoke(ticket, detail);
            ClearActiveTicket();
        }

        public bool FailOpen(UISceneTransitionTicket ticket, string reason)
        {
            if (!IsCurrentTicket(ticket))
            {
                return false;
            }

            if (revealRoutine != null)
            {
                StopCoroutine(revealRoutine);
                revealRoutine = null;
            }

            string detail = NormalizeFailureReason(reason);
            Debug.LogWarning($"UI scene transition handoff failed open: {detail}", this);
            transitionPresenter?.HideImmediate();
            RestoreDestinationInput(ticket);
            HandoffFailed?.Invoke(ticket, detail);
            ClearActiveTicket();
            return true;
        }

        private IEnumerator RevealDestination(UISceneTransitionTicket ticket)
        {
            if (transitionPresenter != null)
            {
                yield return transitionPresenter.PlayIn();
            }

            if (!IsCurrentTicket(ticket))
            {
                yield break;
            }

            revealRoutine = null;
            RestoreDestinationInput(ticket);
            HandoffCompleted?.Invoke(ticket);
            ClearActiveTicket();
        }

        private IEnumerator TerminalHandoffRoutine(
            UIScreenRouteTable.Route route,
            UISceneTransitionTicket ticket,
            Func<UITransitionDispatchResult> dispatch,
            Action<string> failed)
        {
            if (transitionPresenter != null)
            {
                yield return transitionPresenter.PlayOut(route);
            }

            if (!IsCurrentTicket(ticket))
            {
                NotifyTerminalHandoffFailure(
                    failed,
                    $"Transition handoff generation {ticket.Generation} became stale before scene activation.");
                yield break;
            }

            if (!TryMarkActivationRequested(ticket))
            {
                string activationError =
                    $"Transition handoff generation {ticket.Generation} could not seal scene activation.";
                yield return FailAndRestore(ticket, activationError);
                NotifyTerminalHandoffFailure(failed, activationError);
                yield break;
            }

            UITransitionDispatchResult result;
            try
            {
                result = dispatch();
            }
            catch (Exception exception)
            {
                result = UITransitionDispatchResult.Failure(exception.Message);
            }

            if (result.Succeeded)
            {
                yield break;
            }

            string dispatchError = NormalizeFailureReason(result.Error);
            yield return FailAndRestore(ticket, dispatchError);
            NotifyTerminalHandoffFailure(failed, dispatchError);
        }

        private void NotifyTerminalHandoffFailure(Action<string> failed, string error)
        {
            if (failed == null)
            {
                return;
            }

            try
            {
                failed(error);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }

        private bool TryClaimOwnership()
        {
            if (currentOwner != null && currentOwner != this)
            {
                DestroyDuplicateRoot();
                return false;
            }

            currentOwner = this;
            ownsPersistentRoot = true;
            if (nextGeneration < lastIssuedGeneration)
            {
                nextGeneration = lastIssuedGeneration;
            }

            if (!UITransitionHandoffService.TryRegisterProvider(this))
            {
                currentOwner = null;
                ownsPersistentRoot = false;
                DestroyDuplicateRoot();
                return false;
            }

            return true;
        }

        private void PromoteOverlayRoot()
        {
            Canvas localCanvas = GetComponent<Canvas>();
            Canvas sourceCanvas = localCanvas != null ? localCanvas : GetComponentInParent<Canvas>();
            CanvasScaler sourceScaler = sourceCanvas != null
                ? sourceCanvas.GetComponent<CanvasScaler>()
                : null;

            if (transform.parent != null)
            {
                transform.SetParent(null, false);
            }

            Canvas persistentCanvas = localCanvas != null ? localCanvas : gameObject.AddComponent<Canvas>();
            CanvasScaler persistentScaler = GetComponent<CanvasScaler>();
            if (persistentScaler == null)
            {
                persistentScaler = gameObject.AddComponent<CanvasScaler>();
            }

            if (sourceCanvas != null && sourceCanvas != persistentCanvas)
            {
                CopyCanvas(sourceCanvas, persistentCanvas);
            }
            else if (sourceCanvas == null)
            {
                persistentCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }

            if (sourceScaler != null && sourceScaler != persistentScaler)
            {
                CopyCanvasScaler(sourceScaler, persistentScaler);
            }

            persistentCanvas.overrideSorting = true;
            persistentCanvas.sortingOrder = persistentSortingOrder;
            if (GetComponent<GraphicRaycaster>() == null)
            {
                gameObject.AddComponent<GraphicRaycaster>();
            }

            RectTransform rect = transform as RectTransform;
            if (rect != null)
            {
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = Vector2.zero;
                rect.localRotation = Quaternion.identity;
                rect.localScale = Vector3.one;
            }

            DontDestroyOnLoad(gameObject);
        }

        private void AcquireDestinationInput(UISceneTransitionTicket ticket, Scene scene)
        {
            if (!ticket.IsValid || !scene.IsValid() || !scene.isLoaded)
            {
                return;
            }

            ForceRestoreDestinationInput();

            var gates = new List<UIRouteInteractableGate>();
            var eventSystems = new List<EventSystem>();
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                GameObject root = roots[i];
                if (root == null)
                {
                    continue;
                }

                gates.AddRange(root.GetComponentsInChildren<UIRouteInteractableGate>(true));
                eventSystems.AddRange(root.GetComponentsInChildren<EventSystem>(true));
            }

            inputLeaseOwnerInstanceId = ticket.OwnerInstanceId;
            inputLeaseGeneration = ticket.Generation;
            inputLockedGates = gates.ToArray();
            for (int i = 0; i < inputLockedGates.Length; i++)
            {
                inputLockedGates[i]?.AcquireTransitionRouteLock(ticket);
            }

            suspendedEventSystems = eventSystems.ToArray();
            suspendedEventSystemEnabled = new bool[suspendedEventSystems.Length];
            suspendedEventSystemSelection = new GameObject[suspendedEventSystems.Length];
            for (int i = 0; i < suspendedEventSystems.Length; i++)
            {
                EventSystem eventSystem = suspendedEventSystems[i];
                if (eventSystem == null)
                {
                    continue;
                }

                suspendedEventSystemEnabled[i] = eventSystem.enabled;
                suspendedEventSystemSelection[i] = eventSystem.currentSelectedGameObject;
                eventSystem.enabled = false;
            }
        }

        private void RestoreDestinationInput(UISceneTransitionTicket ticket)
        {
            if (!OwnsDestinationInput(ticket))
            {
                return;
            }

            RestoreDestinationInput(ticket, false);
        }

        private void ForceRestoreDestinationInput()
        {
            RestoreDestinationInput(default, true);
        }

        private void RestoreDestinationInput(UISceneTransitionTicket ticket, bool force)
        {
            bool hasStoredLease = inputLeaseOwnerInstanceId != 0 && inputLeaseGeneration != 0;
            if (!force && !OwnsDestinationInput(ticket))
            {
                return;
            }

            if (hasStoredLease)
            {
                for (int i = 0; i < inputLockedGates.Length; i++)
                {
                    UIRouteInteractableGate gate = inputLockedGates[i];
                    if (gate == null)
                    {
                        continue;
                    }

                    if (ticket.IsValid)
                    {
                        gate.ReleaseTransitionRouteLock(ticket);
                    }
                    else
                    {
                        gate.ReleaseTransitionRouteLocksOwnedBy(
                            inputLeaseOwnerInstanceId,
                            inputLeaseGeneration);
                    }
                }
            }

            int eventSystemCount = Mathf.Min(
                suspendedEventSystems != null ? suspendedEventSystems.Length : 0,
                suspendedEventSystemEnabled != null ? suspendedEventSystemEnabled.Length : 0);
            for (int i = 0; i < eventSystemCount; i++)
            {
                EventSystem eventSystem = suspendedEventSystems[i];
                if (eventSystem != null)
                {
                    eventSystem.enabled = suspendedEventSystemEnabled[i];
                }
            }

            int selectionCount = Mathf.Min(
                eventSystemCount,
                suspendedEventSystemSelection != null ? suspendedEventSystemSelection.Length : 0);
            for (int i = 0; i < selectionCount; i++)
            {
                EventSystem eventSystem = suspendedEventSystems[i];
                GameObject selection = suspendedEventSystemSelection[i];
                if (eventSystem != null
                    && eventSystem.enabled
                    && selection != null
                    && selection.activeInHierarchy)
                {
                    eventSystem.SetSelectedGameObject(selection);
                }
            }

            inputLockedGates = Array.Empty<UIRouteInteractableGate>();
            suspendedEventSystems = Array.Empty<EventSystem>();
            suspendedEventSystemEnabled = Array.Empty<bool>();
            suspendedEventSystemSelection = Array.Empty<GameObject>();
            inputLeaseOwnerInstanceId = 0;
            inputLeaseGeneration = 0;
        }

        private bool OwnsDestinationInput(UISceneTransitionTicket ticket)
        {
            return ticket.IsValid
                && inputLeaseOwnerInstanceId == ticket.OwnerInstanceId
                && inputLeaseGeneration == ticket.Generation;
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode _)
        {
            if (!activationRequested)
            {
                return;
            }

            TryMarkDestinationArrived(activationTicket, scene);
        }

        private void SubscribeSceneLoaded()
        {
            if (sceneLoadedSubscribed)
            {
                return;
            }

            SceneManager.sceneLoaded += HandleSceneLoaded;
            sceneLoadedSubscribed = true;
        }

        private void UnsubscribeSceneLoaded()
        {
            if (!sceneLoadedSubscribed)
            {
                return;
            }

            SceneManager.sceneLoaded -= HandleSceneLoaded;
            sceneLoadedSubscribed = false;
        }

        private void DestroyDuplicateRoot()
        {
            transitionPresenter?.HideImmediate();
            if (Application.isPlaying)
            {
                Destroy(gameObject);
            }
            else
            {
                DestroyImmediate(gameObject);
            }
        }

        private void ResetOrphanedHandoff()
        {
            if (revealRoutine != null)
            {
                StopCoroutine(revealRoutine);
                revealRoutine = null;
            }

            ForceRestoreDestinationInput();
            ClearActiveTicket();
            transitionPresenter?.HideImmediate();
        }

        private void ResolvePresenter()
        {
            if (transitionPresenter == null)
            {
                transitionPresenter = GetComponent<UITransitionPresenter>();
            }
        }

        private bool IsCurrentTicket(UISceneTransitionTicket ticket)
        {
            return hasActiveTicket
                && ticket.IsValid
                && ticket.OwnerInstanceId == GetInstanceID()
                && ticket.Generation == activeTicket.Generation
                && ticket == activeTicket;
        }

        private static bool IsExpectedDestination(UISceneTransitionTicket ticket, Scene scene)
        {
            if (!scene.IsValid()
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

        private void ClearActiveTicket()
        {
            activeTicket = default;
            activationTicket = default;
            hasActiveTicket = false;
            activationRequested = false;
            destinationArrived = false;
            destinationSceneHandle = 0;
            destinationArrivedAt = 0d;
            revealRoutine = null;
        }

        private uint NextGeneration()
        {
            if (nextGeneration < lastIssuedGeneration)
            {
                nextGeneration = lastIssuedGeneration;
            }

            unchecked
            {
                nextGeneration++;
            }

            if (nextGeneration == 0)
            {
                nextGeneration = 1;
            }

            lastIssuedGeneration = nextGeneration;
            return nextGeneration;
        }

        private static string NormalizeScenePath(string path)
        {
            return string.IsNullOrWhiteSpace(path)
                ? string.Empty
                : path.Trim().Replace('\\', '/');
        }

        private static string NormalizeFailureReason(string reason)
        {
            return string.IsNullOrWhiteSpace(reason)
                ? "Unknown transition handoff failure."
                : reason.Trim();
        }

        private static void CopyCanvas(Canvas source, Canvas destination)
        {
            destination.renderMode = source.renderMode;
            destination.worldCamera = source.worldCamera;
            destination.planeDistance = source.planeDistance;
            destination.pixelPerfect = source.pixelPerfect;
            destination.targetDisplay = source.targetDisplay;
            destination.sortingLayerID = source.sortingLayerID;
            destination.additionalShaderChannels = source.additionalShaderChannels;
        }

        private static void CopyCanvasScaler(CanvasScaler source, CanvasScaler destination)
        {
            destination.uiScaleMode = source.uiScaleMode;
            destination.referencePixelsPerUnit = source.referencePixelsPerUnit;
            destination.scaleFactor = source.scaleFactor;
            destination.referenceResolution = source.referenceResolution;
            destination.screenMatchMode = source.screenMatchMode;
            destination.matchWidthOrHeight = source.matchWidthOrHeight;
            destination.physicalUnit = source.physicalUnit;
            destination.fallbackScreenDPI = source.fallbackScreenDPI;
            destination.defaultSpriteDPI = source.defaultSpriteDPI;
            destination.dynamicPixelsPerUnit = source.dynamicPixelsPerUnit;
        }

#if UNITY_INCLUDE_TESTS
        public static void ResetForTests()
        {
            UISceneTransitionHandoffOwner[] owners = FindObjectsByType<UISceneTransitionHandoffOwner>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            currentOwner = null;
            nextGeneration = 0;
            for (int i = 0; i < owners.Length; i++)
            {
                UISceneTransitionHandoffOwner owner = owners[i];
                if (owner == null)
                {
                    continue;
                }

                owner.UnsubscribeSceneLoaded();
                owner.DestinationArrived = null;
                owner.HandoffCompleted = null;
                owner.HandoffFailed = null;
                owner.ResetOrphanedHandoff();
                DestroyImmediate(owner.gameObject);
            }
        }
#endif
    }
}
