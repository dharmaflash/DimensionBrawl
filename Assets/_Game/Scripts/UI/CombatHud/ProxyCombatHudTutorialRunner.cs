using System;
using UnityEngine;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    public sealed class ProxyCombatHudTutorialRunner : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private PgrCombatHudProxyMappingCatalog mappingCatalog;
        [SerializeField] private ProxyCombatHudTutorialStep[] tutorialSteps = Array.Empty<ProxyCombatHudTutorialStep>();

        [Header("Runtime References")]
        [SerializeField] private ProxyCombatHudTargetResolver targetResolver;
        [SerializeField] private ProxyCombatHudOverlayPresenter overlayPresenter;
        [SerializeField] private ProxyCombatHudTutorialObserver combatObserver;

        [Header("Playback")]
        [SerializeField] private bool useDefaultP0MappingsWhenCatalogMissing = true;
        [SerializeField] private bool startFirstStepOnEnable;

        private PgrCombatHudProxyMapping activeMapping;
        private ProxyCombatHudTutorialStep activeStep;
        private ProxyCombatHudInputPolicy activeInputPolicy;
        private bool hasActiveMapping;
        private bool running;
        private float activeElapsedSeconds;
        private int activeStepIndex = -1;
        private int completedStepCount;
        private string lastCompletedMappingId;
        private ProxyCombatHudCompletionKind lastCompletionReason;
        private ProxyCombatHudInputEvent lastRejectedInput;
        private ProxyCombatHudInputEvent lastAcceptedInput;

        public event Action<PgrCombatHudProxyMapping> StepStarted;
        public event Action<PgrCombatHudProxyMapping, ProxyCombatHudCompletionKind> StepCompleted;

        public bool IsRunning => running;
        public int ActiveStepIndex => activeStepIndex;
        public int CompletedStepCount => completedStepCount;
        public string ActiveMappingId => hasActiveMapping ? activeMapping.MappingId : string.Empty;
        public string LastCompletedMappingId => lastCompletedMappingId;
        public ProxyCombatHudCompletionKind LastCompletionReason => lastCompletionReason;
        public ProxyCombatHudInputEvent LastRejectedInput => lastRejectedInput;
        public ProxyCombatHudInputEvent LastAcceptedInput => lastAcceptedInput;
        public ProxyCombatHudInputPolicy ActiveInputPolicy => activeInputPolicy;

        private void Awake()
        {
            if (targetResolver == null)
            {
                targetResolver = GetComponent<ProxyCombatHudTargetResolver>();
            }

            if (overlayPresenter == null)
            {
                overlayPresenter = GetComponent<ProxyCombatHudOverlayPresenter>();
            }

            if (combatObserver == null)
            {
                combatObserver = GetComponent<ProxyCombatHudTutorialObserver>();
            }
        }

        private void OnEnable()
        {
            SubscribeObserver();
            if (startFirstStepOnEnable && tutorialSteps.Length > 0)
            {
                StartStepAt(0);
            }
        }

        private void OnDisable()
        {
            UnsubscribeObserver();
            overlayPresenter?.Hide();
            running = false;
            hasActiveMapping = false;
        }

        private void Update()
        {
            Tick(Time.unscaledDeltaTime);
        }

        public void Configure(
            PgrCombatHudProxyMappingCatalog newMappingCatalog,
            ProxyCombatHudTargetResolver newTargetResolver,
            ProxyCombatHudOverlayPresenter newOverlayPresenter,
            ProxyCombatHudTutorialObserver newCombatObserver)
        {
            UnsubscribeObserver();
            mappingCatalog = newMappingCatalog;
            targetResolver = newTargetResolver;
            overlayPresenter = newOverlayPresenter;
            combatObserver = newCombatObserver;
            SubscribeObserver();
        }

        public void ConfigureSteps(ProxyCombatHudTutorialStep[] steps)
        {
            tutorialSteps = steps ?? Array.Empty<ProxyCombatHudTutorialStep>();
        }

        public bool StartStepAt(int stepIndex)
        {
            if (stepIndex < 0 || stepIndex >= tutorialSteps.Length)
            {
                return false;
            }

            activeStepIndex = stepIndex;
            return BeginStep(tutorialSteps[stepIndex]);
        }

        public bool BeginStep(ProxyCombatHudTutorialStep step)
        {
            if (step == null || !step.Enabled || !TryResolveMapping(step, out PgrCombatHudProxyMapping mapping))
            {
                return false;
            }

            activeStep = step;
            activeMapping = mapping;
            hasActiveMapping = true;
            running = true;
            activeElapsedSeconds = 0f;
            lastCompletionReason = ProxyCombatHudCompletionKind.None;
            lastAcceptedInput = ProxyCombatHudInputEvent.None;
            lastRejectedInput = ProxyCombatHudInputEvent.None;
            activeInputPolicy = ResolveInputPolicy(step, mapping);

            System.Collections.Generic.IReadOnlyList<RectTransform> targets = Array.Empty<RectTransform>();
            bool resolvedTarget = targetResolver != null && targetResolver.TryResolve(mapping.ProxyHudObject, out targets);
            overlayPresenter?.Show(mapping, targets, ResolveGuideText(step, mapping), !resolvedTarget);
            StepStarted?.Invoke(mapping);
            return true;
        }

        public bool BeginMapping(
            string mappingId,
            string guideText = "",
            float durationSeconds = 0f,
            ProxyCombatHudInputPolicy inputPolicy = ProxyCombatHudInputPolicy.Default,
            bool completeOnAcceptedInput = false)
        {
            return BeginStep(ProxyCombatHudTutorialStep.ForMappingId(
                mappingId,
                guideText,
                durationSeconds,
                inputPolicy,
                completeOnAcceptedInput));
        }

        public bool TryAcceptInput(ProxyCombatHudInputEvent inputEvent)
        {
            if (!running || !hasActiveMapping)
            {
                return true;
            }

            if (inputEvent.Kind == ProxyCombatHudInputKind.ReadAcknowledged)
            {
                ObserveCompletion(new ProxyCombatHudCompletionEvent(ProxyCombatHudCompletionKind.ReadAcknowledged));
                return true;
            }

            bool matches = activeMapping.AcceptsInput(inputEvent);
            if (matches)
            {
                lastAcceptedInput = inputEvent;
                if (activeStep.CompleteOnAcceptedInput)
                {
                    CompleteActiveStep(ProxyCombatHudCompletionKind.InputAccepted);
                }

                return true;
            }

            lastRejectedInput = inputEvent;
            return activeInputPolicy != ProxyCombatHudInputPolicy.GateRequestedInput;
        }

        public void ObserveCompletion(ProxyCombatHudCompletionEvent completionEvent)
        {
            if (!running || !hasActiveMapping)
            {
                return;
            }

            if (activeMapping.MatchesCompletion(completionEvent))
            {
                CompleteActiveStep(completionEvent.Kind);
            }
        }

        public void Tick(float deltaSeconds)
        {
            if (!running || !hasActiveMapping)
            {
                return;
            }

            activeElapsedSeconds += Mathf.Max(0f, deltaSeconds);
            if (activeStep.DurationSeconds > 0f
                && activeElapsedSeconds >= activeStep.DurationSeconds
                && activeMapping.MatchesCompletion(new ProxyCombatHudCompletionEvent(ProxyCombatHudCompletionKind.DurationElapsed)))
            {
                CompleteActiveStep(ProxyCombatHudCompletionKind.DurationElapsed);
            }
        }

        private bool TryResolveMapping(ProxyCombatHudTutorialStep step, out PgrCombatHudProxyMapping mapping)
        {
            if (!string.IsNullOrWhiteSpace(step.MappingId))
            {
                if (mappingCatalog != null && mappingCatalog.TryFindByMappingId(step.MappingId, out mapping))
                {
                    return true;
                }

                if (useDefaultP0MappingsWhenCatalogMissing &&
                    PgrCombatHudProxyMappingCatalog.TryFindDefaultP0ByMappingId(step.MappingId, out mapping))
                {
                    return true;
                }
            }

            if (!string.IsNullOrWhiteSpace(step.PgrMaskTarget))
            {
                if (mappingCatalog != null && mappingCatalog.TryResolve(step.PgrMaskTarget, step.PgrClickKey, out mapping))
                {
                    return true;
                }

                if (useDefaultP0MappingsWhenCatalogMissing &&
                    PgrCombatHudProxyMappingCatalog.TryResolveDefaultP0(step.PgrMaskTarget, step.PgrClickKey, out mapping))
                {
                    return true;
                }
            }

            mapping = default;
            return false;
        }

        private static ProxyCombatHudInputPolicy ResolveInputPolicy(
            ProxyCombatHudTutorialStep step,
            PgrCombatHudProxyMapping mapping)
        {
            if (step.InputPolicy != ProxyCombatHudInputPolicy.Default)
            {
                return step.InputPolicy;
            }

            return mapping.HasInput
                ? ProxyCombatHudInputPolicy.GateRequestedInput
                : ProxyCombatHudInputPolicy.ObserveOnly;
        }

        private static string ResolveGuideText(
            ProxyCombatHudTutorialStep step,
            PgrCombatHudProxyMapping mapping)
        {
            if (!string.IsNullOrWhiteSpace(step.GuideText))
            {
                return step.GuideText.Trim();
            }

            if (!string.IsNullOrWhiteSpace(mapping.SampleTexts))
            {
                return mapping.SampleTexts.Trim();
            }

            return mapping.MappingId;
        }

        private void CompleteActiveStep(ProxyCombatHudCompletionKind reason)
        {
            if (!running || !hasActiveMapping)
            {
                return;
            }

            PgrCombatHudProxyMapping completedMapping = activeMapping;
            running = false;
            hasActiveMapping = false;
            completedStepCount++;
            lastCompletedMappingId = completedMapping.MappingId;
            lastCompletionReason = reason;
            overlayPresenter?.Hide();
            StepCompleted?.Invoke(completedMapping, reason);
        }

        private void SubscribeObserver()
        {
            if (combatObserver != null)
            {
                combatObserver.CompletionObserved += ObserveCompletion;
            }
        }

        private void UnsubscribeObserver()
        {
            if (combatObserver != null)
            {
                combatObserver.CompletionObserved -= ObserveCompletion;
            }
        }
    }
}
