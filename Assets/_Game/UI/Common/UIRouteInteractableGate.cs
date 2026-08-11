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

        private Selectable[] baselineSelectables = Array.Empty<Selectable>();
        private bool[] authoredInteractable = Array.Empty<bool>();
        private bool[] capabilityInteractable = Array.Empty<bool>();
        private bool routerRouteLocked;
        private bool externalRouteLocked;
        private bool globalRouteLocked;

        public bool GlobalRouteLocked => globalRouteLocked;

        private void Awake()
        {
            CaptureAuthoredBaselines();
        }

        private void OnEnable()
        {
            EnsureAuthoredBaselines();
            if (!globalRouteLocked)
            {
                CaptureCapabilityBaselines();
            }

            Subscribe();
            Apply(router != null ? router.CurrentState : UISceneFlowState.Idle);
        }

        private void OnDisable()
        {
            Unsubscribe();
            routerRouteLocked = false;
            Apply(UISceneFlowState.Idle);
        }

        public void SetGlobalRouteLocked(bool locked)
        {
            externalRouteLocked = locked;
            ApplyLockState();
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
            routerRouteLocked = disableWhileRouting && state.IsRouting;
            ApplyLockState();
        }

        private void ApplyLockState()
        {
            EnsureAuthoredBaselines();
            bool nextGlobalRouteLocked = routerRouteLocked || externalRouteLocked;
            if (nextGlobalRouteLocked && !globalRouteLocked)
            {
                CaptureCapabilityBaselines();
            }

            globalRouteLocked = nextGlobalRouteLocked;
            for (int i = 0; i < selectables.Length; i++)
            {
                Selectable selectable = selectables[i];
                if (selectable != null)
                {
                    selectable.interactable = capabilityInteractable[i] && !globalRouteLocked;
                }
            }

            float alpha = globalRouteLocked ? routingAlpha : idleAlpha;
            for (int i = 0; i < dimGroups.Length; i++)
            {
                if (dimGroups[i] != null)
                {
                    dimGroups[i].alpha = alpha;
                }
            }
        }

        private void CaptureAuthoredBaselines()
        {
            int count = selectables != null ? selectables.Length : 0;
            baselineSelectables = new Selectable[count];
            authoredInteractable = new bool[count];
            capabilityInteractable = new bool[count];

            for (int i = 0; i < count; i++)
            {
                Selectable selectable = selectables[i];
                baselineSelectables[i] = selectable;
                bool authoredState = selectable != null && selectable.interactable;
                authoredInteractable[i] = authoredState;
                capabilityInteractable[i] = authoredState;
            }
        }

        private void EnsureAuthoredBaselines()
        {
            int count = selectables != null ? selectables.Length : 0;
            if (baselineSelectables.Length != count
                || authoredInteractable.Length != count
                || capabilityInteractable.Length != count)
            {
                CaptureAuthoredBaselines();
                return;
            }

            for (int i = 0; i < count; i++)
            {
                if (baselineSelectables[i] == selectables[i])
                {
                    continue;
                }

                Selectable selectable = selectables[i];
                baselineSelectables[i] = selectable;
                bool authoredState = selectable != null && selectable.interactable;
                authoredInteractable[i] = authoredState;
                capabilityInteractable[i] = authoredState;
            }
        }

        private void CaptureCapabilityBaselines()
        {
            EnsureAuthoredBaselines();
            for (int i = 0; i < selectables.Length; i++)
            {
                Selectable selectable = selectables[i];
                capabilityInteractable[i] = authoredInteractable[i]
                    && selectable != null
                    && selectable.interactable;
            }
        }
    }
}
