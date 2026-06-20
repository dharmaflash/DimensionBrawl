using System;
using UnityEngine;
using UnityEngine.UI;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    public sealed class UIRouteInteractableGate : MonoBehaviour
    {
        [SerializeField] private UISceneFlowRouter router;
        [SerializeField] private Selectable[] selectables = Array.Empty<Selectable>();
        [SerializeField] private CanvasGroup[] dimGroups = Array.Empty<CanvasGroup>();
        [SerializeField] private bool disableWhileRouting = true;
        [SerializeField, Range(0.1f, 1f)] private float routingAlpha = 0.68f;
        [SerializeField, Range(0.1f, 1f)] private float idleAlpha = 1f;

        private void OnEnable()
        {
            Subscribe();
            Apply(router != null ? router.CurrentState : UISceneFlowState.Idle);
        }

        private void OnDisable()
        {
            Unsubscribe();
            Apply(UISceneFlowState.Idle);
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
            bool locked = disableWhileRouting && state.IsRouting;
            for (int i = 0; i < selectables.Length; i++)
            {
                if (selectables[i] != null)
                {
                    selectables[i].interactable = !locked;
                }
            }

            float alpha = locked ? routingAlpha : idleAlpha;
            for (int i = 0; i < dimGroups.Length; i++)
            {
                if (dimGroups[i] != null)
                {
                    dimGroups[i].alpha = alpha;
                }
            }
        }
    }
}
