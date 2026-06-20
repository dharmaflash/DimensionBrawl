using UnityEngine;
using UnityEngine.UI;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    public sealed class UISceneFlowStatusPresenter : MonoBehaviour
    {
        [SerializeField] private UISceneFlowRouter router;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Text routeText;
        [SerializeField] private Text phaseText;
        [SerializeField] private Text progressText;
        [SerializeField] private Image progressFill;
        [SerializeField] private bool hideWhenIdle;

        private void Reset()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        private void OnEnable()
        {
            Subscribe();
            Apply(router != null ? router.CurrentState : UISceneFlowState.Idle);
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        public void Bind(UISceneFlowRouter sceneRouter)
        {
            if (router == sceneRouter)
            {
                Apply(router != null ? router.CurrentState : UISceneFlowState.Idle);
                return;
            }

            Unsubscribe();
            router = sceneRouter;
            Subscribe();
            Apply(router != null ? router.CurrentState : UISceneFlowState.Idle);
        }

        private void Subscribe()
        {
            if (router != null)
            {
                router.StateChanged += HandleStateChanged;
            }
        }

        private void Unsubscribe()
        {
            if (router != null)
            {
                router.StateChanged -= HandleStateChanged;
            }
        }

        private void HandleStateChanged(UISceneFlowState state)
        {
            Apply(state);
        }

        private void Apply(UISceneFlowState state)
        {
            bool visible = !hideWhenIdle || state.IsRouting || state.Phase != UISceneFlowPhase.Idle;
            SetVisible(visible);

            string routeLabel = state.RouteId == UIRouteId.None ? "Route: Idle" : $"Route: {state.RouteId}";
            if (!string.IsNullOrWhiteSpace(state.SceneName))
            {
                routeLabel = $"{routeLabel} -> {state.SceneName}";
            }

            string phaseLabel = string.IsNullOrWhiteSpace(state.StatusText)
                ? state.Phase.ToString()
                : $"{state.Phase} | {state.StatusText}";
            float clampedProgress = Mathf.Clamp01(state.NormalizedProgress);

            SetText(routeText, routeLabel);
            SetText(phaseText, phaseLabel);
            SetText(progressText, $"{Mathf.RoundToInt(clampedProgress * 100f)}%");

            if (progressFill != null)
            {
                progressFill.fillAmount = clampedProgress;
            }
        }

        private void SetVisible(bool visible)
        {
            if (canvasGroup == null)
            {
                return;
            }

            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        private static void SetText(Text target, string value)
        {
            if (target != null)
            {
                target.text = value;
            }
        }
    }
}
