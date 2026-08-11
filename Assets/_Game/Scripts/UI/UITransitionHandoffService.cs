using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DimensionBrawl.UI
{
    public enum UITransitionDestinationKind
    {
        None = 0,
        Lobby = 20,
        Combat = 40
    }

    public readonly struct UITransitionHandoffDestination
    {
        public UITransitionHandoffDestination(
            UITransitionDestinationKind kind,
            string sceneName,
            string scenePath)
        {
            Kind = kind;
            SceneName = sceneName?.Trim() ?? string.Empty;
            ScenePath = scenePath?.Trim() ?? string.Empty;
        }

        public UITransitionDestinationKind Kind { get; }
        public string SceneName { get; }
        public string ScenePath { get; }
        public bool IsValid => (Kind == UITransitionDestinationKind.Lobby
                || Kind == UITransitionDestinationKind.Combat)
            && !string.IsNullOrWhiteSpace(SceneName);
    }

    public readonly struct UITransitionDispatchResult
    {
        public UITransitionDispatchResult(bool succeeded, string error)
        {
            Succeeded = succeeded;
            Error = error ?? string.Empty;
        }

        public bool Succeeded { get; }
        public string Error { get; }

        public static UITransitionDispatchResult Success()
        {
            return new UITransitionDispatchResult(true, string.Empty);
        }

        public static UITransitionDispatchResult Failure(string error)
        {
            return new UITransitionDispatchResult(false, error);
        }
    }

    public interface IUITransitionHandoffProvider
    {
        bool IsSceneInputLocked(Scene scene);

        bool TryBeginTerminalHandoff(
            UITransitionHandoffDestination destination,
            Func<UITransitionDispatchResult> dispatch,
            Action<string> failed,
            out string error);
    }

    public static class UITransitionHandoffService
    {
        private static IUITransitionHandoffProvider provider;

        public static bool HasProvider => provider != null;

        public static bool IsSceneInputLocked(Scene scene)
        {
            return provider != null && provider.IsSceneInputLocked(scene);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            provider = null;
        }

        public static bool TryRegisterProvider(IUITransitionHandoffProvider candidate)
        {
            if (candidate == null)
            {
                return false;
            }

            if (provider != null && !ReferenceEquals(provider, candidate))
            {
                return false;
            }

            provider = candidate;
            return true;
        }

        public static void UnregisterProvider(IUITransitionHandoffProvider candidate)
        {
            if (ReferenceEquals(provider, candidate))
            {
                provider = null;
            }
        }

        public static bool TryBeginTerminalHandoff(
            UITransitionHandoffDestination destination,
            Func<UITransitionDispatchResult> dispatch,
            Action<string> failed,
            out string error)
        {
            if (provider == null)
            {
                error = "The persistent UI transition handoff provider is unavailable.";
                return false;
            }

            return provider.TryBeginTerminalHandoff(
                destination,
                dispatch,
                failed,
                out error);
        }

#if UNITY_INCLUDE_TESTS
        public static void ResetForTests()
        {
            provider = null;
        }
#endif
    }
}
