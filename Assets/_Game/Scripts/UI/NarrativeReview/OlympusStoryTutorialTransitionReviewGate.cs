using System;
using DimensionBrawl.Presentation.Narrative;
using UnityEngine;

namespace DimensionBrawl.UI.NarrativeReview
{
    /// <summary>
    /// Owns only the TEMP_DO_NOT_SHIP review scene's story-presentation overrides.
    /// It cannot mutate route, StageRun, combat, tutorial facts, save, or progression.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    [DisallowMultipleComponent]
    public sealed class OlympusStoryTutorialTransitionReviewGate : MonoBehaviour
    {
        [Header("Review-only direct bindings")]
        [SerializeField] private Camera gameplayCamera;
        [SerializeField] private Camera narrativePresentationCamera;
        [SerializeField] private CanvasGroup gameplayHud;
        [SerializeField] private Behaviour gameplayInput;
        [SerializeField] private AudioListener gameplayListener;
        [SerializeField] private AudioListener narrativePresentationListener;
        [SerializeField] private ReviewTutorialStartProbe tutorialStartProbe;

        private readonly StoryTutorialReviewTransitionSession lifecycle =
            new StoryTutorialReviewTransitionSession();
        private PresentationSnapshot snapshot;
        private StoryTutorialReviewReceipt lastReceipt;
        private bool lastDispatchSucceeded;
        private bool tutorialStartClaimed;

        public Camera GameplayCamera => gameplayCamera;
        public Camera NarrativePresentationCamera => narrativePresentationCamera;
        public CanvasGroup GameplayHud => gameplayHud;
        public Behaviour GameplayInput => gameplayInput;
        public AudioListener GameplayListener => gameplayListener;
        public AudioListener NarrativePresentationListener => narrativePresentationListener;
        public ReviewTutorialStartProbe TutorialStartProbe => tutorialStartProbe;
        public StoryTutorialReviewTransitionPhase Phase => lifecycle.Phase;
        public long ActiveGeneration => lifecycle.CurrentGeneration;
        public StoryTutorialReviewReceipt LastReceipt => lastReceipt;
        public bool LastDispatchSucceeded => lastDispatchSucceeded;
        public bool HasValidBindings => TryValidateBindings(out _);

        public void Configure(
            Camera newGameplayCamera,
            Camera newNarrativePresentationCamera,
            CanvasGroup newGameplayHud,
            Behaviour newGameplayInput,
            AudioListener newGameplayListener,
            AudioListener newNarrativePresentationListener,
            ReviewTutorialStartProbe newTutorialStartProbe)
        {
            if (lifecycle.Phase == StoryTutorialReviewTransitionPhase.StoryPresenting
                || lifecycle.Phase == StoryTutorialReviewTransitionPhase.Terminating)
            {
                throw new InvalidOperationException(
                    "Story transition bindings cannot change while the review lease is active.");
            }

            gameplayCamera = newGameplayCamera;
            narrativePresentationCamera = newNarrativePresentationCamera;
            gameplayHud = newGameplayHud;
            gameplayInput = newGameplayInput;
            gameplayListener = newGameplayListener;
            narrativePresentationListener = newNarrativePresentationListener;
            tutorialStartProbe = newTutorialStartProbe;
        }

        public bool TryBeginStory(long generation, out string error)
        {
            error = string.Empty;
            if (!TryValidateBindings(out error))
            {
                return false;
            }

            PresentationSnapshot candidate = CaptureSnapshot();
            StoryTutorialReviewSignalResult beginResult = lifecycle.TryBegin(generation);
            if (beginResult != StoryTutorialReviewSignalResult.Accepted)
            {
                error = $"Story transition generation was rejected: {beginResult}.";
                return false;
            }

            snapshot = candidate;
            lastReceipt = default;
            lastDispatchSucceeded = false;
            tutorialStartClaimed = false;
            try
            {
                gameplayCamera.enabled = false;
                narrativePresentationCamera.enabled = true;

                gameplayHud.alpha = 0f;
                gameplayHud.interactable = false;
                gameplayHud.blocksRaycasts = false;
                gameplayHud.gameObject.SetActive(false);

                gameplayInput.enabled = false;
                gameplayListener.enabled = false;
                narrativePresentationListener.enabled = true;
                Time.timeScale = 0f;
                return true;
            }
            catch (Exception exception)
            {
                lifecycle.TryRequestTerminal(
                    generation,
                    StoryTutorialReviewTerminalReason.StateApplyFailed);
                RestoreAndSeal(
                    generation,
                    storyOwnedWorkReleased: false,
                    tutorialTargetAvailable: false,
                    out _);
                error = "Story presentation overrides could not be applied: " + exception.Message;
                return false;
            }
        }

        public StoryTutorialReviewSignalResult TryRequestTerminal(
            long generation,
            StoryTutorialReviewTerminalReason reason)
        {
            return lifecycle.TryRequestTerminal(generation, reason);
        }

        public StoryTutorialReviewSignalResult RestoreAndSeal(
            long generation,
            bool storyOwnedWorkReleased,
            bool tutorialTargetAvailable,
            out StoryTutorialReviewReceipt receipt)
        {
            receipt = default;
            if (generation <= 0 || generation != lifecycle.CurrentGeneration)
            {
                return StoryTutorialReviewSignalResult.StaleGeneration;
            }

            if (lifecycle.Phase == StoryTutorialReviewTransitionPhase.Terminated)
            {
                receipt = lastReceipt;
                return StoryTutorialReviewSignalResult.AlreadyAccepted;
            }

            if (lifecycle.Phase != StoryTutorialReviewTransitionPhase.Terminating)
            {
                return StoryTutorialReviewSignalResult.InvalidPhase;
            }

            bool restoreSucceeded = RestoreSnapshotBestEffort();
            StoryTutorialReviewSignalResult sealResult = lifecycle.TrySealRelease(
                generation,
                storyOwnedWorkReleased,
                restoreSucceeded,
                tutorialTargetAvailable,
                out receipt);
            if (sealResult != StoryTutorialReviewSignalResult.Accepted)
            {
                return sealResult;
            }

            lastReceipt = receipt;
            return sealResult;
        }

        public bool TryClaimTutorialStart(long generation)
        {
            if (tutorialStartClaimed
                || generation <= 0
                || generation != lifecycle.CurrentGeneration
                || lifecycle.Phase != StoryTutorialReviewTransitionPhase.Terminated
                || !lastReceipt.CanDispatchReviewTutorialStart)
            {
                return false;
            }

            tutorialStartClaimed = true;
            return true;
        }

        public bool ConfirmTutorialStarted(long generation)
        {
            if (!tutorialStartClaimed
                || lastDispatchSucceeded
                || generation <= 0
                || generation != lifecycle.CurrentGeneration
                || tutorialStartProbe == null
                || !tutorialStartProbe.TryRecord(lastReceipt))
            {
                return false;
            }

            lastDispatchSucceeded = true;
            return true;
        }

        public bool HasTutorialStartClaimFor(long generation)
        {
            return tutorialStartClaimed
                && generation > 0
                && generation == lifecycle.CurrentGeneration;
        }

        public bool WasTutorialStartConfirmedFor(long generation)
        {
            return lastDispatchSucceeded
                && tutorialStartProbe != null
                && tutorialStartProbe.WasDispatchedFor(generation);
        }

        public bool TryValidateBindings(out string error)
        {
            error = string.Empty;
            if (gameplayCamera == null
                || narrativePresentationCamera == null
                || gameplayHud == null
                || gameplayInput == null
                || gameplayListener == null
                || narrativePresentationListener == null
                || tutorialStartProbe == null)
            {
                error = "Every story transition review binding is required.";
                return false;
            }

            if (gameplayCamera == narrativePresentationCamera
                || gameplayListener == narrativePresentationListener)
            {
                error = "Gameplay and narrative camera/listener bindings must be distinct.";
                return false;
            }

            if (gameplayListener.gameObject != gameplayCamera.gameObject
                || narrativePresentationListener.gameObject
                    != narrativePresentationCamera.gameObject)
            {
                error = "Each review listener must belong to its directly bound camera.";
                return false;
            }

            if (gameplayInput == this || gameplayInput == tutorialStartProbe)
            {
                error = "The gameplay input binding must be an independent review component.";
                return false;
            }

            if (gameplayHud.gameObject == gameObject)
            {
                error = "The gameplay HUD cannot own the transition gate.";
                return false;
            }

            if (transform.IsChildOf(gameplayHud.transform)
                || tutorialStartProbe.transform.IsChildOf(gameplayHud.transform))
            {
                error = "The transition gate and tutorial probe cannot be owned by the leased HUD.";
                return false;
            }

            if (gameplayCamera.gameObject.scene != gameObject.scene
                || narrativePresentationCamera.gameObject.scene != gameObject.scene
                || gameplayHud.gameObject.scene != gameObject.scene
                || gameplayInput.gameObject.scene != gameObject.scene
                || tutorialStartProbe.gameObject.scene != gameObject.scene)
            {
                error = "Every review transition binding must belong to the gate's scene.";
                return false;
            }

            return true;
        }

        private void OnDisable()
        {
            if (!Application.isPlaying
                || lifecycle.Phase == StoryTutorialReviewTransitionPhase.Idle
                || lifecycle.Phase == StoryTutorialReviewTransitionPhase.Terminated)
            {
                return;
            }

            long generation = lifecycle.CurrentGeneration;
            lifecycle.TryRequestTerminal(
                generation,
                StoryTutorialReviewTerminalReason.OwnerDisabled);
            RestoreAndSeal(
                generation,
                storyOwnedWorkReleased: false,
                tutorialTargetAvailable: false,
                out _);
        }

        private PresentationSnapshot CaptureSnapshot()
        {
            return new PresentationSnapshot(
                gameplayCamera.enabled,
                narrativePresentationCamera.enabled,
                gameplayHud.gameObject.activeSelf,
                gameplayHud.alpha,
                gameplayHud.interactable,
                gameplayHud.blocksRaycasts,
                gameplayInput.enabled,
                gameplayListener.enabled,
                narrativePresentationListener.enabled,
                Time.timeScale);
        }

        private bool RestoreSnapshotBestEffort()
        {
            if (!snapshot.IsValid)
            {
                return false;
            }

            bool succeeded = true;
            succeeded &= TryRestore(() => gameplayCamera.enabled = snapshot.GameplayCameraEnabled);
            succeeded &= TryRestore(
                () => narrativePresentationCamera.enabled = snapshot.NarrativeCameraEnabled);
            succeeded &= TryRestore(() => gameplayInput.enabled = snapshot.GameplayInputEnabled);
            succeeded &= TryRestore(
                () => gameplayListener.enabled = snapshot.GameplayListenerEnabled);
            succeeded &= TryRestore(
                () => narrativePresentationListener.enabled = snapshot.NarrativeListenerEnabled);
            succeeded &= TryRestore(() =>
            {
                if (snapshot.GameplayHudActiveSelf)
                {
                    gameplayHud.gameObject.SetActive(true);
                }

                gameplayHud.alpha = snapshot.GameplayHudAlpha;
                gameplayHud.interactable = snapshot.GameplayHudInteractable;
                gameplayHud.blocksRaycasts = snapshot.GameplayHudBlocksRaycasts;
                gameplayHud.gameObject.SetActive(snapshot.GameplayHudActiveSelf);
            });
            succeeded &= TryRestore(() => Time.timeScale = snapshot.TimeScale);
            snapshot = default;
            return succeeded;
        }

        private static bool TryRestore(Action restore)
        {
            try
            {
                restore();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private readonly struct PresentationSnapshot
        {
            public PresentationSnapshot(
                bool gameplayCameraEnabled,
                bool narrativeCameraEnabled,
                bool gameplayHudActiveSelf,
                float gameplayHudAlpha,
                bool gameplayHudInteractable,
                bool gameplayHudBlocksRaycasts,
                bool gameplayInputEnabled,
                bool gameplayListenerEnabled,
                bool narrativeListenerEnabled,
                float timeScale)
            {
                IsValid = true;
                GameplayCameraEnabled = gameplayCameraEnabled;
                NarrativeCameraEnabled = narrativeCameraEnabled;
                GameplayHudActiveSelf = gameplayHudActiveSelf;
                GameplayHudAlpha = gameplayHudAlpha;
                GameplayHudInteractable = gameplayHudInteractable;
                GameplayHudBlocksRaycasts = gameplayHudBlocksRaycasts;
                GameplayInputEnabled = gameplayInputEnabled;
                GameplayListenerEnabled = gameplayListenerEnabled;
                NarrativeListenerEnabled = narrativeListenerEnabled;
                TimeScale = timeScale;
            }

            public bool IsValid { get; }
            public bool GameplayCameraEnabled { get; }
            public bool NarrativeCameraEnabled { get; }
            public bool GameplayHudActiveSelf { get; }
            public float GameplayHudAlpha { get; }
            public bool GameplayHudInteractable { get; }
            public bool GameplayHudBlocksRaycasts { get; }
            public bool GameplayInputEnabled { get; }
            public bool GameplayListenerEnabled { get; }
            public bool NarrativeListenerEnabled { get; }
            public float TimeScale { get; }
        }
    }
}
