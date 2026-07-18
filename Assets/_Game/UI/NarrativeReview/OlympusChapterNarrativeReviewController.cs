using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.Presentation.Narrative;
using DimensionBrawl.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

namespace DimensionBrawl.UI.NarrativeReview
{
    public enum NarrativeReviewPhase
    {
        None = 0,
        ChapterEntry = 1,
        VisualNovel = 2,
        StoryTransitionBlocked = 3,
        TutorialCutscene = 4,
        StageBriefing = 5,
        Complete = 6
    }

    [DisallowMultipleComponent]
    public sealed class OlympusChapterNarrativeReviewController : MonoBehaviour
    {
        private const string ReviewStageLabel = "REVIEW SAMPLE / TEMP_DO_NOT_SHIP";

        [Header("Canonical Data")]
        [SerializeField] private NarrativeSequenceProfile narrativeProfile;
        [SerializeField] private UIStageCatalog stageCatalog;
        [SerializeField] private PlayableDirector cutsceneDirector;
        [SerializeField] private StageCutscenePort cutscenePort;

        [Header("Story to Tutorial Review Gate")]
        [SerializeField] private OlympusStoryTutorialTransitionReviewGate storyTutorialTransitionGate;

        [Header("Flow Groups")]
        [SerializeField] private CanvasGroup chapterEntryGroup;
        [SerializeField] private CanvasGroup visualNovelGroup;
        [SerializeField] private CanvasGroup cutsceneControlsGroup;
        [SerializeField] private CanvasGroup stageBriefingGroup;
        [SerializeField] private CanvasGroup completeGroup;

        [Header("Chapter Entry")]
        [SerializeField] private TMP_Text chapterEyebrowText;
        [SerializeField] private TMP_Text chapterTitleText;
        [SerializeField] private TMP_Text chapterStageTitleText;
        [SerializeField] private TMP_Text chapterObjectiveText;
        [SerializeField] private TMP_Text chapterStatusText;
        [SerializeField] private Button chapterEnterButton;

        [Header("Visual Novel")]
        [SerializeField] private TMP_Text narrativeSequenceText;
        [SerializeField] private TMP_Text narrativeSpeakerText;
        [SerializeField] private TMP_Text narrativeLineText;
        [SerializeField] private TMP_Text narrativeProgressText;
        [SerializeField] private CanvasGroup leftPortraitGroup;
        [SerializeField] private CanvasGroup centerPortraitGroup;
        [SerializeField] private CanvasGroup rightPortraitGroup;
        [SerializeField] private Image leftPortraitImage;
        [SerializeField] private Image centerPortraitImage;
        [SerializeField] private Image rightPortraitImage;
        [SerializeField] private Button narrativeNextButton;
        [SerializeField] private Button narrativeAutoButton;
        [SerializeField] private TMP_Text narrativeAutoButtonText;
        [SerializeField] private Button narrativeSkipButton;
        [SerializeField] private Button narrativeLogButton;
        [SerializeField] private CanvasGroup narrativeChoiceGroup;
        [SerializeField] private Button firstChoiceButton;
        [SerializeField] private TMP_Text firstChoiceText;
        [SerializeField] private Button secondChoiceButton;
        [SerializeField] private TMP_Text secondChoiceText;
        [SerializeField, Min(1f)] private float typewriterCharactersPerSecond = 38f;
        [SerializeField, Min(0f)] private float autoAdvanceTailSeconds = 0.45f;

        [Header("Cutscene")]
        [SerializeField] private TMP_Text cutsceneLabelText;
        [SerializeField] private TMP_Text cutsceneProgressText;
        [SerializeField] private Button cutsceneSkipButton;

        [Header("Stage Briefing")]
        [SerializeField] private TMP_Text briefingTitleText;
        [SerializeField] private TMP_Text briefingObjectiveText;
        [SerializeField] private TMP_Text briefingCombatLessonText;
        [SerializeField] private TMP_Text briefingThreatText;
        [SerializeField] private TMP_Text briefingSummonText;
        [SerializeField] private TMP_Text briefingDurationText;
        [SerializeField] private GameObject briefingRewardRow;
        [SerializeField] private TMP_Text briefingRewardText;
        [SerializeField] private TMP_Text briefingDigestText;
        [SerializeField] private TMP_Text briefingStatusText;
        [SerializeField] private Button briefingCompleteButton;

        [Header("Complete")]
        [SerializeField] private TMP_Text completeTitleText;
        [SerializeField] private TMP_Text completeSummaryText;
        [SerializeField] private Button restartButton;

        [Header("Utility Panels")]
        [SerializeField] private CanvasGroup logGroup;
        [SerializeField] private TMP_Text logText;
        [SerializeField] private Button logCloseButton;
        [SerializeField] private CanvasGroup skipConfirmGroup;
        [SerializeField] private Button skipConfirmButton;
        [SerializeField] private Button skipCancelButton;

        [Header("Audio")]
        [SerializeField] private AudioSource voiceAudioSource;

        private NarrativeSequenceSession narrativeSession;
        private UIStageRouteProjection stageProjection;
        private Coroutine typewriterRoutine;
        private Coroutine autoAdvanceRoutine;
        private bool lineFullyRevealed;
        private bool autoAdvanceEnabled;
        private bool presentingChoiceResponse;
        private bool interactionsBound;
        private bool cutsceneCompletionIssued;
        private int completionDispatchCount;
        private float nextNarrativeInputAllowedAt;
        private string currentVisibleLineText = string.Empty;
        private string narrativeChoiceSummary = "none";
        private Action<NarrativeSequenceCompletionReason> narrativeCompletedHandler;
        private readonly NarrativeTutorialReviewLifecycleSession reviewLifecycle =
            new NarrativeTutorialReviewLifecycleSession();
        private long activeReviewGeneration;
        private NarrativeTutorialReviewReceipt lastReviewReceipt;
        private StoryTutorialReviewReceipt lastStoryTutorialReceipt;
        private PlayableDirector subscribedCutsceneDirector;
        private Action<PlayableDirector> cutsceneStoppedHandler;

        public NarrativeReviewPhase CurrentPhase { get; private set; }
        public NarrativeSequenceSession NarrativeSession => narrativeSession;
        public UIStageRouteProjection StageProjection => stageProjection;
        public int CompletionDispatchCount => completionDispatchCount;
        public bool AutoAdvanceEnabled => autoAdvanceEnabled;
        public bool HasValidCutsceneBoundary => TryResolveCutsceneBoundary(out _);
        public NarrativeTutorialReviewPhase ReviewLifecyclePhase => reviewLifecycle.Phase;
        public NarrativeTutorialReviewReceipt LastReviewReceipt => lastReviewReceipt;
        public long ActiveReviewGeneration => activeReviewGeneration;
        public bool CanEnterReviewBriefing =>
            lastReviewReceipt.CanEnterReviewBriefing
            && lastStoryTutorialReceipt.CanDispatchReviewTutorialStart
            && storyTutorialTransitionGate != null
            && storyTutorialTransitionGate.WasTutorialStartConfirmedFor(
                activeReviewGeneration);
        public StoryTutorialReviewTransitionPhase StoryTransitionPhase =>
            storyTutorialTransitionGate != null
                ? storyTutorialTransitionGate.Phase
                : StoryTutorialReviewTransitionPhase.Idle;
        public StoryTutorialReviewReceipt LastStoryTutorialReceipt =>
            lastStoryTutorialReceipt;
        public int TutorialStartProbeCount =>
            storyTutorialTransitionGate != null
                && storyTutorialTransitionGate.TutorialStartProbe != null
                    ? storyTutorialTransitionGate.TutorialStartProbe.DispatchCount
                    : 0;
        public bool HasConfirmedTutorialStartForActiveGeneration =>
            activeReviewGeneration > 0
            && lastStoryTutorialReceipt.Generation == activeReviewGeneration
            && storyTutorialTransitionGate != null
            && storyTutorialTransitionGate.WasTutorialStartConfirmedFor(
                activeReviewGeneration);

        public void ConfigureCore(
            NarrativeSequenceProfile newNarrativeProfile,
            UIStageCatalog newStageCatalog,
            PlayableDirector newCutsceneDirector,
            StageCutscenePort newCutscenePort,
            AudioSource newVoiceAudioSource)
        {
            EnsureRuntimeConfigurationCanMutate();
            bool rebind = interactionsBound;
            if (rebind)
            {
                UnbindInteractions();
            }

            narrativeProfile = newNarrativeProfile;
            stageCatalog = newStageCatalog;
            cutsceneDirector = newCutsceneDirector;
            cutscenePort = newCutscenePort;
            voiceAudioSource = newVoiceAudioSource;
            RebindAfterRuntimeConfiguration(rebind);
        }

        public void ConfigureStoryTutorialTransitionGate(
            OlympusStoryTutorialTransitionReviewGate newTransitionGate)
        {
            EnsureRuntimeConfigurationCanMutate();
            storyTutorialTransitionGate = newTransitionGate;
        }

        public void ConfigureFlowGroups(
            CanvasGroup newChapterEntryGroup,
            CanvasGroup newVisualNovelGroup,
            CanvasGroup newCutsceneControlsGroup,
            CanvasGroup newStageBriefingGroup,
            CanvasGroup newCompleteGroup)
        {
            EnsureRuntimeConfigurationCanMutate();
            chapterEntryGroup = newChapterEntryGroup;
            visualNovelGroup = newVisualNovelGroup;
            cutsceneControlsGroup = newCutsceneControlsGroup;
            stageBriefingGroup = newStageBriefingGroup;
            completeGroup = newCompleteGroup;
        }

        public void ConfigureChapterView(
            TMP_Text eyebrow,
            TMP_Text title,
            TMP_Text stageTitle,
            TMP_Text objective,
            TMP_Text status,
            Button enterButton)
        {
            EnsureRuntimeConfigurationCanMutate();
            bool rebind = interactionsBound;
            if (rebind)
            {
                UnbindInteractions();
            }

            chapterEyebrowText = eyebrow;
            chapterTitleText = title;
            chapterStageTitleText = stageTitle;
            chapterObjectiveText = objective;
            chapterStatusText = status;
            chapterEnterButton = enterButton;
            RebindAfterRuntimeConfiguration(rebind);
        }

        public void ConfigureNarrativeView(
            TMP_Text sequence,
            TMP_Text speaker,
            TMP_Text line,
            TMP_Text progress,
            CanvasGroup leftPortrait,
            CanvasGroup centerPortrait,
            CanvasGroup rightPortrait,
            Image leftImage,
            Image centerImage,
            Image rightImage,
            Button nextButton,
            Button autoButton,
            TMP_Text autoButtonText,
            Button skipButton,
            Button logButton,
            CanvasGroup choiceGroup,
            Button choiceAButton,
            TMP_Text choiceAText,
            Button choiceBButton,
            TMP_Text choiceBText)
        {
            EnsureRuntimeConfigurationCanMutate();
            bool rebind = interactionsBound;
            if (rebind)
            {
                UnbindInteractions();
            }

            narrativeSequenceText = sequence;
            narrativeSpeakerText = speaker;
            narrativeLineText = line;
            narrativeProgressText = progress;
            leftPortraitGroup = leftPortrait;
            centerPortraitGroup = centerPortrait;
            rightPortraitGroup = rightPortrait;
            leftPortraitImage = leftImage;
            centerPortraitImage = centerImage;
            rightPortraitImage = rightImage;
            narrativeNextButton = nextButton;
            narrativeAutoButton = autoButton;
            narrativeAutoButtonText = autoButtonText;
            narrativeSkipButton = skipButton;
            narrativeLogButton = logButton;
            narrativeChoiceGroup = choiceGroup;
            firstChoiceButton = choiceAButton;
            firstChoiceText = choiceAText;
            secondChoiceButton = choiceBButton;
            secondChoiceText = choiceBText;
            RebindAfterRuntimeConfiguration(rebind);
        }

        public void ConfigureCutsceneView(
            TMP_Text label,
            TMP_Text progress,
            Button skipButton)
        {
            EnsureRuntimeConfigurationCanMutate();
            bool rebind = interactionsBound;
            if (rebind)
            {
                UnbindInteractions();
            }

            cutsceneLabelText = label;
            cutsceneProgressText = progress;
            cutsceneSkipButton = skipButton;
            RebindAfterRuntimeConfiguration(rebind);
        }

        public void ConfigureBriefingView(
            TMP_Text title,
            TMP_Text objective,
            TMP_Text combatLesson,
            TMP_Text threat,
            TMP_Text summon,
            TMP_Text duration,
            GameObject rewardRow,
            TMP_Text reward,
            TMP_Text digest,
            TMP_Text status,
            Button completeButton)
        {
            EnsureRuntimeConfigurationCanMutate();
            bool rebind = interactionsBound;
            if (rebind)
            {
                UnbindInteractions();
            }

            briefingTitleText = title;
            briefingObjectiveText = objective;
            briefingCombatLessonText = combatLesson;
            briefingThreatText = threat;
            briefingSummonText = summon;
            briefingDurationText = duration;
            briefingRewardRow = rewardRow;
            briefingRewardText = reward;
            briefingDigestText = digest;
            briefingStatusText = status;
            briefingCompleteButton = completeButton;
            RebindAfterRuntimeConfiguration(rebind);
        }

        public void ConfigureCompleteView(
            TMP_Text title,
            TMP_Text summary,
            Button newRestartButton)
        {
            EnsureRuntimeConfigurationCanMutate();
            bool rebind = interactionsBound;
            if (rebind)
            {
                UnbindInteractions();
            }

            completeTitleText = title;
            completeSummaryText = summary;
            restartButton = newRestartButton;
            RebindAfterRuntimeConfiguration(rebind);
        }

        public void ConfigureUtilityPanels(
            CanvasGroup newLogGroup,
            TMP_Text newLogText,
            Button newLogCloseButton,
            CanvasGroup newSkipConfirmGroup,
            Button newSkipConfirmButton,
            Button newSkipCancelButton)
        {
            EnsureRuntimeConfigurationCanMutate();
            bool rebind = interactionsBound;
            if (rebind)
            {
                UnbindInteractions();
            }

            logGroup = newLogGroup;
            logText = newLogText;
            logCloseButton = newLogCloseButton;
            skipConfirmGroup = newSkipConfirmGroup;
            skipConfirmButton = newSkipConfirmButton;
            skipCancelButton = newSkipCancelButton;
            RebindAfterRuntimeConfiguration(rebind);
        }

        private void OnEnable()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            BindInteractions();
            if (CurrentPhase != NarrativeReviewPhase.None)
            {
                BeginChapterEntry();
            }
        }

        private void BindInteractions()
        {
            if (interactionsBound)
            {
                return;
            }

            AddButtonListener(chapterEnterButton, BeginVisualNovel);
            AddButtonListener(narrativeNextButton, HandleNarrativeNextClicked);
            AddButtonListener(narrativeAutoButton, ToggleAutoAdvance);
            AddButtonListener(narrativeSkipButton, ShowSkipConfirmation);
            AddButtonListener(narrativeLogButton, ShowLog);
            AddButtonListener(firstChoiceButton, HandleFirstChoiceClicked);
            AddButtonListener(secondChoiceButton, HandleSecondChoiceClicked);
            AddButtonListener(cutsceneSkipButton, SkipCutscene);
            AddButtonListener(briefingCompleteButton, CompleteReview);
            AddButtonListener(restartButton, BeginChapterEntry);
            AddButtonListener(logCloseButton, HideLog);
            AddButtonListener(skipConfirmButton, ConfirmNarrativeSkip);
            AddButtonListener(skipCancelButton, HideSkipConfirmation);

            interactionsBound = true;
        }

        private void Start()
        {
            BeginChapterEntry();
        }

        private void Update()
        {
            if (CurrentPhase != NarrativeReviewPhase.TutorialCutscene
                || cutsceneDirector == null)
            {
                return;
            }

            if (!cutsceneCompletionIssued
                && cutsceneDirector.state == PlayState.Playing
                && IsEnteringMandatoryCutsceneEndFrame(cutsceneDirector))
            {
                FinalizeTutorialCutscene(
                    activeReviewGeneration,
                    NarrativeTutorialReviewTerminalReason.Completed,
                    applyMandatoryFinalState: true,
                    stopPlayback: true);
                return;
            }

            if (cutsceneProgressText == null)
            {
                return;
            }

            double duration = Math.Max(0.001d, cutsceneDirector.duration);
            float normalized = Mathf.Clamp01((float)(cutsceneDirector.time / duration));
            cutsceneProgressText.text = $"SIGNAL LINK  {normalized * 100f:00}%";
        }

        private void OnDisable()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            long generation = activeReviewGeneration;
            StoryTutorialReviewSignalResult storyTerminalRequest =
                RequestStoryTransitionTerminal(
                    generation,
                    StoryTutorialReviewTerminalReason.OwnerDisabled);
            NarrativeTutorialReviewSignalResult terminalRequest =
                RequestReviewTerminal(
                    generation,
                    NarrativeTutorialReviewTerminalReason.OwnerDisabled);

            UnsubscribeCutsceneStopped();
            if (CurrentPhase == NarrativeReviewPhase.TutorialCutscene
                && cutsceneDirector != null
                && cutsceneDirector.playableGraph.IsValid())
            {
                cutsceneDirector.Stop();
            }

            UnbindInteractions();
            ReleaseNarrativeSession();
            StopNarrativeRoutines();
            StopVoice();
            if (storyTerminalRequest == StoryTutorialReviewSignalResult.Accepted
                || storyTerminalRequest == StoryTutorialReviewSignalResult.AlreadyAccepted)
            {
                RestoreAndSealStoryTransition(
                    generation,
                    storyOwnedWorkReleased: true,
                    tutorialTargetAvailable: false,
                    out _);
            }

            if (terminalRequest == NarrativeTutorialReviewSignalResult.Accepted)
            {
                SealReviewRelease(generation, cleanupSucceeded: true);
            }
        }

        private void UnbindInteractions()
        {
            if (!interactionsBound)
            {
                return;
            }

            RemoveButtonListener(chapterEnterButton, BeginVisualNovel);
            RemoveButtonListener(narrativeNextButton, HandleNarrativeNextClicked);
            RemoveButtonListener(narrativeAutoButton, ToggleAutoAdvance);
            RemoveButtonListener(narrativeSkipButton, ShowSkipConfirmation);
            RemoveButtonListener(narrativeLogButton, ShowLog);
            RemoveButtonListener(firstChoiceButton, HandleFirstChoiceClicked);
            RemoveButtonListener(secondChoiceButton, HandleSecondChoiceClicked);
            RemoveButtonListener(cutsceneSkipButton, SkipCutscene);
            RemoveButtonListener(briefingCompleteButton, CompleteReview);
            RemoveButtonListener(restartButton, BeginChapterEntry);
            RemoveButtonListener(logCloseButton, HideLog);
            RemoveButtonListener(skipConfirmButton, ConfirmNarrativeSkip);
            RemoveButtonListener(skipCancelButton, HideSkipConfirmation);
            UnsubscribeCutsceneStopped();

            interactionsBound = false;
        }

        private void RebindAfterRuntimeConfiguration(bool rebind)
        {
            if (rebind && Application.isPlaying && isActiveAndEnabled)
            {
                BindInteractions();
            }
        }

        private void EnsureRuntimeConfigurationCanMutate()
        {
            if (Application.isPlaying
                && isActiveAndEnabled
                && (CurrentPhase == NarrativeReviewPhase.VisualNovel
                    || CurrentPhase == NarrativeReviewPhase.TutorialCutscene))
            {
                throw new InvalidOperationException(
                    "Runtime configuration cannot change during an active narrative or tutorial review.");
            }
        }

        public void BeginChapterEntry()
        {
            CancelActiveReviewForReset();
            ReleaseNarrativeSession();
            StopNarrativeRoutines();
            StopVoice();
            autoAdvanceEnabled = false;
            presentingChoiceResponse = false;
            nextNarrativeInputAllowedAt = 0f;
            currentVisibleLineText = string.Empty;
            narrativeChoiceSummary = "none";
            UpdateAutoButtonLabel();
            cutsceneCompletionIssued = false;
            completionDispatchCount = 0;
            activeReviewGeneration = 0;
            lastReviewReceipt = default;
            lastStoryTutorialReceipt = default;
            ResolveStageProjection();
            CurrentPhase = NarrativeReviewPhase.ChapterEntry;
            ShowOnly(chapterEntryGroup);
            HideUtilityPanels();

            SetText(chapterEyebrowText, "CHAPTER 00 / OLYMPUS SIGNAL");
            SetText(chapterTitleText, "게이트 신호");
            SetText(chapterStageTitleText, stageProjection != null ? stageProjection.DisplayName : "작전 경로 확인 불가");
            SetText(chapterObjectiveText, stageProjection != null ? stageProjection.Summary : string.Empty);
            SetText(chapterStatusText, stageProjection != null
                ? "STORY ENTRY READY  ·  REVIEW-ONLY"
                : "CANONICAL BRIEFING UNAVAILABLE");
            SetButtonInteractable(chapterEnterButton, stageProjection != null && IsNarrativeProfileValid());
        }

        public void BeginVisualNovel()
        {
            if (CurrentPhase != NarrativeReviewPhase.ChapterEntry || !IsNarrativeProfileValid())
            {
                return;
            }

            ReleaseNarrativeSession();
            activeReviewGeneration = reviewLifecycle.Begin();
            lastReviewReceipt = default;
            lastStoryTutorialReceipt = default;
            string transitionError = "Story transition gate is missing.";
            if (storyTutorialTransitionGate == null
                || !storyTutorialTransitionGate.TryBeginStory(
                    activeReviewGeneration,
                    out transitionError))
            {
                NarrativeTutorialReviewSignalResult terminalRequest =
                    RequestReviewTerminal(
                        activeReviewGeneration,
                        NarrativeTutorialReviewTerminalReason.StoryTransitionUnavailable);
                if (terminalRequest == NarrativeTutorialReviewSignalResult.Accepted)
                {
                    SealReviewRelease(activeReviewGeneration, cleanupSucceeded: true);
                }

                SetText(
                    chapterStatusText,
                    "STORY TRANSITION BINDINGS UNAVAILABLE / REVIEW BLOCKED"
                    + (string.IsNullOrWhiteSpace(transitionError)
                        ? string.Empty
                        : $"\n{transitionError}"));
                SetButtonInteractable(chapterEnterButton, false);
                return;
            }

            long generation = activeReviewGeneration;
            narrativeSession = new NarrativeSequenceSession(narrativeProfile);
            narrativeCompletedHandler = completionReason =>
                HandleNarrativeCompleted(generation, completionReason);
            narrativeSession.Completed += narrativeCompletedHandler;
            CurrentPhase = NarrativeReviewPhase.VisualNovel;
            ShowOnly(visualNovelGroup);
            HideUtilityPanels();
            SetText(narrativeSequenceText, $"{ReviewStageLabel}  /  {narrativeProfile.SequenceId}");
            PresentCurrentNarrativeLine();
        }

        private void HandleNarrativeNextClicked()
        {
            if (CurrentPhase != NarrativeReviewPhase.VisualNovel || narrativeSession == null)
            {
                return;
            }

            if (Time.unscaledTime < nextNarrativeInputAllowedAt)
            {
                return;
            }

            nextNarrativeInputAllowedAt = Time.unscaledTime + 0.10f;

            if (RevealCurrentNarrativeLine())
            {
                return;
            }

            if (presentingChoiceResponse)
            {
                presentingChoiceResponse = false;
                NarrativeAdvanceResult responseAdvance = narrativeSession.Advance();
                if (responseAdvance == NarrativeAdvanceResult.Advanced)
                {
                    PresentCurrentNarrativeLine();
                }

                return;
            }

            NarrativeAdvanceResult result = narrativeSession.Advance();
            if (result == NarrativeAdvanceResult.Advanced)
            {
                PresentCurrentNarrativeLine();
            }
            else if (result == NarrativeAdvanceResult.AwaitingChoice)
            {
                ShowCurrentChoices();
            }
        }

        private void HandleFirstChoiceClicked()
        {
            SelectChoiceAt(0);
        }

        private void HandleSecondChoiceClicked()
        {
            SelectChoiceAt(1);
        }

        private void SelectChoiceAt(int choiceIndex)
        {
            if (CurrentPhase != NarrativeReviewPhase.VisualNovel
                || narrativeSession == null
                || narrativeSession.CurrentLine == null)
            {
                return;
            }

            NarrativeSequenceProfile.ChoiceEntry[] choices = narrativeSession.CurrentLine.Choices;
            if (choices == null || choiceIndex < 0 || choiceIndex >= choices.Length)
            {
                return;
            }

            NarrativeSequenceProfile.ChoiceEntry selectedChoice = choices[choiceIndex];
            if (!narrativeSession.TrySelectChoice(selectedChoice.ChoiceId))
            {
                return;
            }

            HideChoices();
            if (selectedChoice.HasResponse)
            {
                PresentChoiceResponse(selectedChoice);
                return;
            }

            NarrativeAdvanceResult result = narrativeSession.Advance();
            if (result == NarrativeAdvanceResult.Advanced)
            {
                PresentCurrentNarrativeLine();
            }
        }

        private void PresentCurrentNarrativeLine()
        {
            StopNarrativeRoutines();
            StopVoice();
            HideChoices();

            NarrativeSequenceProfile.LineEntry line = narrativeSession?.CurrentLine;
            if (line == null)
            {
                return;
            }

            presentingChoiceResponse = false;
            currentVisibleLineText = ResolveLocalizedText(
                line.TextLocalizationKey,
                line.StagingFallbackKorean);
            SetText(narrativeSpeakerText, ResolveSpeakerName(line.SpeakerId));
            SetText(narrativeLineText, currentVisibleLineText);
            SetText(
                narrativeProgressText,
                $"{narrativeSession.CurrentLineIndex + 1:00} / {narrativeProfile.LineCount:00}");
            ApplyPortrait(line);

            if (voiceAudioSource != null && line.VoiceClip != null)
            {
                voiceAudioSource.clip = line.VoiceClip;
                voiceAudioSource.Play();
            }

            lineFullyRevealed = false;
            SetButtonInteractable(narrativeNextButton, true);
            typewriterRoutine = StartCoroutine(TypeCurrentLineRoutine());
        }

        private void PresentChoiceResponse(NarrativeSequenceProfile.ChoiceEntry choice)
        {
            StopNarrativeRoutines();
            StopVoice();
            HideChoices();

            NarrativeSequenceProfile.LineEntry sourceLine = narrativeSession?.CurrentLine;
            if (choice == null || sourceLine == null)
            {
                return;
            }

            presentingChoiceResponse = true;
            currentVisibleLineText = ResolveLocalizedText(
                choice.ResponseTextLocalizationKey,
                choice.ResponseStagingFallbackKorean);
            SetText(narrativeSpeakerText, ResolveSpeakerName(sourceLine.SpeakerId));
            SetText(narrativeLineText, currentVisibleLineText);
            SetText(narrativeProgressText, "CHOICE RESPONSE");
            ApplyPortrait(sourceLine);
            lineFullyRevealed = false;
            SetButtonInteractable(narrativeNextButton, true);
            typewriterRoutine = StartCoroutine(TypeCurrentLineRoutine());
        }

        private IEnumerator TypeCurrentLineRoutine()
        {
            if (narrativeLineText == null)
            {
                lineFullyRevealed = true;
                FinishCurrentLineReveal();
                yield break;
            }

            narrativeLineText.ForceMeshUpdate();
            int visibleCharacters = narrativeLineText.textInfo.characterCount;
            narrativeLineText.maxVisibleCharacters = 0;
            float secondsPerCharacter = 1f / Mathf.Max(1f, typewriterCharactersPerSecond);
            float elapsed = 0f;

            while (narrativeLineText.maxVisibleCharacters < visibleCharacters)
            {
                elapsed += Time.unscaledDeltaTime;
                int target = Mathf.Clamp(
                    Mathf.FloorToInt(elapsed / secondsPerCharacter),
                    0,
                    visibleCharacters);
                narrativeLineText.maxVisibleCharacters = target;
                yield return null;
            }

            typewriterRoutine = null;
            lineFullyRevealed = true;
            FinishCurrentLineReveal();
        }

        private void RevealCurrentLineImmediately()
        {
            if (typewriterRoutine != null)
            {
                StopCoroutine(typewriterRoutine);
                typewriterRoutine = null;
            }

            if (narrativeLineText != null)
            {
                narrativeLineText.maxVisibleCharacters = int.MaxValue;
            }

            lineFullyRevealed = true;
            FinishCurrentLineReveal();
        }

        public bool RevealCurrentNarrativeLine()
        {
            if (CurrentPhase != NarrativeReviewPhase.VisualNovel
                || narrativeSession == null
                || lineFullyRevealed)
            {
                return false;
            }

            RevealCurrentLineImmediately();
            return true;
        }

        private void FinishCurrentLineReveal()
        {
            NarrativeSequenceProfile.LineEntry line = narrativeSession?.CurrentLine;
            if (line == null)
            {
                return;
            }

            if (line.HasChoices && narrativeSession.IsAwaitingChoice)
            {
                ShowCurrentChoices();
                SetButtonInteractable(narrativeNextButton, false);
                return;
            }

            SetButtonInteractable(narrativeNextButton, true);
            QueueAutoAdvance();
        }

        private void ShowCurrentChoices()
        {
            NarrativeSequenceProfile.LineEntry line = narrativeSession?.CurrentLine;
            NarrativeSequenceProfile.ChoiceEntry[] choices = line?.Choices;
            if (choices == null || choices.Length == 0)
            {
                HideChoices();
                return;
            }

            SetGroupVisible(narrativeChoiceGroup, true);
            ConfigureChoice(firstChoiceButton, firstChoiceText, choices, 0);
            ConfigureChoice(secondChoiceButton, secondChoiceText, choices, 1);
            StopAutoAdvanceRoutine();
        }

        private static void ConfigureChoice(
            Button button,
            TMP_Text label,
            IReadOnlyList<NarrativeSequenceProfile.ChoiceEntry> choices,
            int index)
        {
            bool visible = choices != null && index >= 0 && index < choices.Count;
            if (button != null)
            {
                button.gameObject.SetActive(visible);
                button.interactable = visible;
            }

            if (label != null)
            {
                label.text = visible
                    ? ResolveLocalizedText(
                        choices[index].TextLocalizationKey,
                        choices[index].StagingFallbackKorean)
                    : string.Empty;
            }
        }

        private void HideChoices()
        {
            SetGroupVisible(narrativeChoiceGroup, false);
        }

        private void ToggleAutoAdvance()
        {
            if (CurrentPhase != NarrativeReviewPhase.VisualNovel)
            {
                return;
            }

            autoAdvanceEnabled = !autoAdvanceEnabled;
            UpdateAutoButtonLabel();
            if (autoAdvanceEnabled && lineFullyRevealed && narrativeSession?.IsAwaitingChoice == false)
            {
                QueueAutoAdvance();
            }
            else
            {
                StopAutoAdvanceRoutine();
            }
        }

        private void UpdateAutoButtonLabel()
        {
            SetText(narrativeAutoButtonText, autoAdvanceEnabled ? "AUTO  ON" : "AUTO  OFF");
        }

        private void QueueAutoAdvance()
        {
            StopAutoAdvanceRoutine();
            if (!autoAdvanceEnabled
                || !lineFullyRevealed
                || narrativeSession == null
                || narrativeSession.IsAwaitingChoice
                || IsUtilityPanelOpen())
            {
                return;
            }

            autoAdvanceRoutine = StartCoroutine(AutoAdvanceCurrentLineRoutine());
        }

        private IEnumerator AutoAdvanceCurrentLineRoutine()
        {
            NarrativeSequenceProfile.LineEntry line = narrativeSession.CurrentLine;
            int characterCount = string.IsNullOrEmpty(currentVisibleLineText)
                ? 0
                : currentVisibleLineText.Length;
            float dataDelay = line.ResolveAutoAdvanceDelaySeconds(
                narrativeProfile.DefaultAutoAdvanceSecondsPerCharacter,
                characterCount);
            float voiceDelay = line.VoiceClip != null ? line.VoiceClip.length : 0f;
            float delay = Mathf.Max(dataDelay, voiceDelay) + autoAdvanceTailSeconds;
            float elapsed = 0f;
            while (elapsed < delay)
            {
                if (!autoAdvanceEnabled || IsUtilityPanelOpen())
                {
                    autoAdvanceRoutine = null;
                    yield break;
                }

                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            autoAdvanceRoutine = null;
            HandleNarrativeNextClicked();
        }

        private void ShowSkipConfirmation()
        {
            if (CurrentPhase != NarrativeReviewPhase.VisualNovel)
            {
                return;
            }

            StopAutoAdvanceRoutine();
            SetGroupVisible(skipConfirmGroup, true);
        }

        private void HideSkipConfirmation()
        {
            SetGroupVisible(skipConfirmGroup, false);
            QueueAutoAdvance();
        }

        private void ConfirmNarrativeSkip()
        {
            SetGroupVisible(skipConfirmGroup, false);
            narrativeSession?.Skip();
        }

        private void ShowLog()
        {
            if (CurrentPhase != NarrativeReviewPhase.VisualNovel || narrativeSession == null)
            {
                return;
            }

            StopAutoAdvanceRoutine();
            SetText(logText, BuildNarrativeLog());
            SetGroupVisible(logGroup, true);
        }

        private void HideLog()
        {
            SetGroupVisible(logGroup, false);
            QueueAutoAdvance();
        }

        private string BuildNarrativeLog()
        {
            var builder = new StringBuilder(1024);
            builder.AppendLine("COMMUNICATION LOG");
            builder.AppendLine();
            IReadOnlyList<string> seenIds = narrativeSession.SeenLineIds;
            for (int seenIndex = seenIds.Count - 1; seenIndex >= 0; seenIndex--)
            {
                if (!narrativeSession.TryResolveSeenEntry(
                    seenIds[seenIndex],
                    out string speakerId,
                    out string localizationKey,
                    out string fallbackText))
                {
                    continue;
                }

                builder.Append(ResolveSpeakerName(speakerId));
                builder.Append("  /  ");
                builder.AppendLine(seenIds[seenIndex]);
                builder.AppendLine(ResolveLocalizedText(localizationKey, fallbackText));
                builder.AppendLine();
            }

            return builder.ToString();
        }

        private void HandleNarrativeCompleted(
            long callbackGeneration,
            NarrativeSequenceCompletionReason completionReason)
        {
            if (callbackGeneration != activeReviewGeneration
                || CurrentPhase != NarrativeReviewPhase.VisualNovel
                || narrativeSession == null
                || !narrativeSession.IsCompleted)
            {
                return;
            }

            StoryTutorialReviewTerminalReason storyReason =
                completionReason == NarrativeSequenceCompletionReason.Skipped
                    ? StoryTutorialReviewTerminalReason.Skipped
                    : StoryTutorialReviewTerminalReason.Completed;
            StoryTutorialReviewSignalResult terminalRequest =
                RequestStoryTransitionTerminal(callbackGeneration, storyReason);
            if (terminalRequest != StoryTutorialReviewSignalResult.Accepted)
            {
                return;
            }

            bool tutorialTargetAvailable = TryResolveCutsceneBoundary(out _);
            CaptureNarrativeChoiceSummary();
            ReleaseNarrativeSession();
            StopNarrativeRoutines();
            StopVoice();
            HideUtilityPanels();

            StoryTutorialReviewSignalResult restoreResult =
                RestoreAndSealStoryTransition(
                    callbackGeneration,
                    storyOwnedWorkReleased: true,
                    tutorialTargetAvailable: tutorialTargetAvailable,
                    out StoryTutorialReviewReceipt receipt);
            if (restoreResult != StoryTutorialReviewSignalResult.Accepted
                || !receipt.CanDispatchReviewTutorialStart)
            {
                BlockStoryTutorialTransition(callbackGeneration, receipt);
                return;
            }

            BeginTutorialCutscene();
        }

        private void BeginTutorialCutscene()
        {
            if (CurrentPhase != NarrativeReviewPhase.VisualNovel
                || !lastStoryTutorialReceipt.CanDispatchReviewTutorialStart
                || lastStoryTutorialReceipt.Generation != activeReviewGeneration
                || storyTutorialTransitionGate == null)
            {
                return;
            }

            long generation = activeReviewGeneration;
            if (!TryResolveCutsceneBoundary(out PlayableDirector resolvedDirector)
                || resolvedDirector.playableAsset == null)
            {
                BlockStoryTutorialTransition(generation, lastStoryTutorialReceipt);
                return;
            }

            if (!storyTutorialTransitionGate.TryClaimTutorialStart(generation))
            {
                return;
            }

            if (!reviewLifecycle.TryBeginTutorial(generation))
            {
                return;
            }

            CurrentPhase = NarrativeReviewPhase.TutorialCutscene;
            ShowOnly(cutsceneControlsGroup);
            SetText(cutsceneLabelText, "TUTORIAL CUTSCENE / GATE LINK");
            SetText(cutsceneProgressText, "SIGNAL LINK  00%");
            cutsceneCompletionIssued = false;

            BindResolvedCutsceneDirector(resolvedDirector);
            SubscribeCutsceneStopped(resolvedDirector, generation);
            try
            {
                cutsceneDirector.time = 0d;
                cutsceneDirector.Evaluate();
                cutsceneDirector.Play();
                if (cutsceneDirector.state != PlayState.Playing
                    || !storyTutorialTransitionGate.ConfirmTutorialStarted(generation))
                {
                    throw new InvalidOperationException(
                        "Review tutorial playback did not produce one confirmed start.");
                }
            }
            catch (Exception)
            {
                SetText(
                    cutsceneProgressText,
                    "CUTSCENE START FAILED / REVIEW BRIEFING BLOCKED");
                FinalizeTutorialCutscene(
                    generation,
                    NarrativeTutorialReviewTerminalReason.BindingUnavailable,
                    applyMandatoryFinalState: false,
                    stopPlayback: true);
            }
        }

        public void SkipCutscene()
        {
            if (CurrentPhase != NarrativeReviewPhase.TutorialCutscene || cutsceneCompletionIssued)
            {
                return;
            }

            FinalizeTutorialCutscene(
                activeReviewGeneration,
                NarrativeTutorialReviewTerminalReason.Skipped,
                applyMandatoryFinalState: true,
                stopPlayback: true);
        }

        private void HandleCutsceneStopped(PlayableDirector stoppedDirector, long callbackGeneration)
        {
            if (stoppedDirector == cutsceneDirector
                && CurrentPhase == NarrativeReviewPhase.TutorialCutscene)
            {
                FinalizeTutorialCutscene(
                    callbackGeneration,
                    HasReachedMandatoryCutsceneEnd(stoppedDirector)
                        ? NarrativeTutorialReviewTerminalReason.Completed
                        : NarrativeTutorialReviewTerminalReason.Cancelled,
                    applyMandatoryFinalState: false,
                    stopPlayback: false);
            }
        }

        private static bool HasReachedMandatoryCutsceneEnd(PlayableDirector director)
        {
            if (director == null)
            {
                return false;
            }

            double duration = director.duration;
            if (double.IsNaN(duration) || double.IsInfinity(duration) || duration <= 0d)
            {
                return false;
            }

            double tolerance = Math.Max(0.001d, duration * 0.0001d);
            return director.time >= duration - tolerance;
        }

        private static bool IsEnteringMandatoryCutsceneEndFrame(PlayableDirector director)
        {
            if (director == null || director.time <= 0d)
            {
                return false;
            }

            double duration = director.duration;
            if (double.IsNaN(duration) || double.IsInfinity(duration) || duration <= 0d)
            {
                return false;
            }

            double clockDelta = director.timeUpdateMode == DirectorUpdateMode.GameTime
                ? Time.deltaTime
                : director.timeUpdateMode == DirectorUpdateMode.UnscaledGameTime
                    ? Time.unscaledDeltaTime
                    : 0d;
            double frameAdvance = Math.Max(0.001d, clockDelta * 1.5d);
            return director.time + frameAdvance >= duration;
        }

        private void FinalizeTutorialCutscene(
            long generation,
            NarrativeTutorialReviewTerminalReason terminalReason,
            bool applyMandatoryFinalState,
            bool stopPlayback)
        {
            NarrativeTutorialReviewSignalResult terminalRequest =
                RequestReviewTerminal(generation, terminalReason);
            if (terminalRequest != NarrativeTutorialReviewSignalResult.Accepted)
            {
                return;
            }

            cutsceneCompletionIssued = true;
            if (terminalReason == NarrativeTutorialReviewTerminalReason.Cancelled)
            {
                SetText(
                    cutsceneProgressText,
                    "CUTSCENE STOPPED EARLY / REVIEW BRIEFING BLOCKED");
            }

            UnsubscribeCutsceneStopped();
            if (cutsceneDirector != null)
            {
                if (applyMandatoryFinalState)
                {
                    cutsceneDirector.time = Math.Max(0d, cutsceneDirector.duration);
                    cutsceneDirector.Evaluate();
                }

                if (stopPlayback && cutsceneDirector.playableGraph.IsValid())
                {
                    cutsceneDirector.Stop();
                }
            }

            StopVoice();
            if (!SealReviewRelease(generation, cleanupSucceeded: true)
                || !lastReviewReceipt.CanEnterReviewBriefing)
            {
                return;
            }

            completionDispatchCount++;
            BeginStageBriefing();
        }

        private bool TryResolveCutsceneBoundary(out PlayableDirector director)
        {
            director = cutsceneDirector != null
                ? cutsceneDirector
                : cutscenePort != null
                    ? cutscenePort.RuntimeDirector
                    : null;
            if (director == null || cutscenePort == null)
            {
                return false;
            }

            return cutscenePort.RuntimeDirector == director
                && cutscenePort.PortKind == StageCutscenePortKind.Intro
                && cutscenePort.HasPayloadRoot
                && director.playableAsset != null
                && !string.IsNullOrWhiteSpace(cutscenePort.PortId)
                && !string.IsNullOrWhiteSpace(cutscenePort.HandoffId)
                && !string.IsNullOrWhiteSpace(cutscenePort.AnchorId)
                && !string.IsNullOrWhiteSpace(cutscenePort.RuntimeStateId);
        }

        private void BindResolvedCutsceneDirector(PlayableDirector resolvedDirector)
        {
            cutsceneDirector = resolvedDirector;
        }

        private void SubscribeCutsceneStopped(PlayableDirector director, long generation)
        {
            UnsubscribeCutsceneStopped();
            if (director == null || generation <= 0)
            {
                return;
            }

            subscribedCutsceneDirector = director;
            cutsceneStoppedHandler = stoppedDirector =>
                HandleCutsceneStopped(stoppedDirector, generation);
            subscribedCutsceneDirector.stopped += cutsceneStoppedHandler;
        }

        private void UnsubscribeCutsceneStopped()
        {
            if (subscribedCutsceneDirector != null && cutsceneStoppedHandler != null)
            {
                subscribedCutsceneDirector.stopped -= cutsceneStoppedHandler;
            }

            subscribedCutsceneDirector = null;
            cutsceneStoppedHandler = null;
        }

        private NarrativeTutorialReviewSignalResult RequestReviewTerminal(
            long generation,
            NarrativeTutorialReviewTerminalReason terminalReason)
        {
            if (generation <= 0)
            {
                return NarrativeTutorialReviewSignalResult.StaleGeneration;
            }

            return reviewLifecycle.TryRequestTerminal(generation, terminalReason);
        }

        private bool SealReviewRelease(long generation, bool cleanupSucceeded)
        {
            NarrativeTutorialReviewSignalResult sealResult =
                reviewLifecycle.TrySealRelease(
                    generation,
                    cleanupSucceeded,
                    out NarrativeTutorialReviewReceipt receipt);
            if (sealResult != NarrativeTutorialReviewSignalResult.Accepted)
            {
                return false;
            }

            lastReviewReceipt = receipt;
            return true;
        }

        private void CancelActiveReviewForReset()
        {
            long generation = activeReviewGeneration;
            StoryTutorialReviewSignalResult storyTerminalRequest =
                RequestStoryTransitionTerminal(
                    generation,
                    StoryTutorialReviewTerminalReason.Cancelled);
            NarrativeTutorialReviewSignalResult terminalRequest =
                RequestReviewTerminal(
                    generation,
                    NarrativeTutorialReviewTerminalReason.Cancelled);
            if (terminalRequest == NarrativeTutorialReviewSignalResult.Accepted)
            {
                UnsubscribeCutsceneStopped();
                if (cutsceneDirector != null && cutsceneDirector.playableGraph.IsValid())
                {
                    cutsceneDirector.Stop();
                }

                ReleaseNarrativeSession();
                StopNarrativeRoutines();
                StopVoice();
            }

            if (storyTerminalRequest == StoryTutorialReviewSignalResult.Accepted
                || storyTerminalRequest == StoryTutorialReviewSignalResult.AlreadyAccepted)
            {
                RestoreAndSealStoryTransition(
                    generation,
                    storyOwnedWorkReleased: true,
                    tutorialTargetAvailable: false,
                    out _);
            }

            if (terminalRequest == NarrativeTutorialReviewSignalResult.Accepted)
            {
                SealReviewRelease(generation, cleanupSucceeded: true);
            }
        }

        private StoryTutorialReviewSignalResult RequestStoryTransitionTerminal(
            long generation,
            StoryTutorialReviewTerminalReason terminalReason)
        {
            if (generation <= 0 || storyTutorialTransitionGate == null)
            {
                return StoryTutorialReviewSignalResult.StaleGeneration;
            }

            return storyTutorialTransitionGate.TryRequestTerminal(
                generation,
                terminalReason);
        }

        private StoryTutorialReviewSignalResult RestoreAndSealStoryTransition(
            long generation,
            bool storyOwnedWorkReleased,
            bool tutorialTargetAvailable,
            out StoryTutorialReviewReceipt receipt)
        {
            receipt = default;
            if (storyTutorialTransitionGate == null)
            {
                return StoryTutorialReviewSignalResult.InvalidPhase;
            }

            StoryTutorialReviewSignalResult result =
                storyTutorialTransitionGate.RestoreAndSeal(
                    generation,
                    storyOwnedWorkReleased,
                    tutorialTargetAvailable,
                    out receipt);
            if ((result == StoryTutorialReviewSignalResult.Accepted
                    || result == StoryTutorialReviewSignalResult.AlreadyAccepted)
                && receipt.IsSealed)
            {
                lastStoryTutorialReceipt = receipt;
            }

            return result;
        }

        private void BlockStoryTutorialTransition(
            long generation,
            StoryTutorialReviewReceipt receipt)
        {
            if (generation <= 0 || generation != activeReviewGeneration)
            {
                return;
            }

            if (receipt.IsSealed)
            {
                lastStoryTutorialReceipt = receipt;
            }

            NarrativeTutorialReviewSignalResult terminalRequest =
                RequestReviewTerminal(
                    generation,
                    NarrativeTutorialReviewTerminalReason.StoryTransitionUnavailable);
            if (terminalRequest == NarrativeTutorialReviewSignalResult.Accepted)
            {
                SealReviewRelease(generation, cleanupSucceeded: true);
            }

            CurrentPhase = NarrativeReviewPhase.StoryTransitionBlocked;
            ShowOnly(visualNovelGroup);
            HideUtilityPanels();
            SetText(
                narrativeProgressText,
                "STORY RELEASED / TUTORIAL START BLOCKED");
            SetText(
                narrativeLineText,
                receipt.IsSealed && !receipt.StateRestoreSucceeded
                    ? "PRESENTATION STATE RESTORE FAILED / REVIEW STOPPED"
                    : "TUTORIAL BOUNDARY UNAVAILABLE / REVIEW STOPPED");
            SetButtonInteractable(narrativeNextButton, false);
            SetButtonInteractable(narrativeAutoButton, false);
            SetButtonInteractable(narrativeSkipButton, false);
        }

        public void BeginStageBriefing()
        {
            if (CurrentPhase != NarrativeReviewPhase.TutorialCutscene
                || !CanEnterReviewBriefing)
            {
                return;
            }

            ResolveStageProjection();
            CurrentPhase = NarrativeReviewPhase.StageBriefing;
            ShowOnly(stageBriefingGroup);
            HideUtilityPanels();

            if (stageProjection == null || stageProjection.Briefing == null)
            {
                SetText(briefingStatusText, "CANONICAL BRIEFING UNAVAILABLE");
                SetButtonInteractable(briefingCompleteButton, false);
                return;
            }

            StageBriefingReadModel briefing = stageProjection.Briefing;
            SetText(briefingTitleText, briefing.Title);
            SetText(briefingObjectiveText, briefing.Objective);
            SetText(briefingCombatLessonText, briefing.CombatLesson);
            SetText(
                briefingThreatText,
                string.IsNullOrWhiteSpace(stageProjection.ThreatTags)
                    ? "— NOT SPECIFIED —"
                    : stageProjection.ThreatTags);
            SetText(
                briefingSummonText,
                string.IsNullOrWhiteSpace(stageProjection.RecommendedSummonRole)
                    ? "— OPEN SLOT —"
                    : stageProjection.RecommendedSummonRole);
            SetText(briefingDurationText, FormatDuration(briefing.TargetRunDurationMilliseconds));
            bool hasReward = !string.IsNullOrWhiteSpace(stageProjection.RewardPreview);
            if (briefingRewardRow != null)
            {
                briefingRewardRow.SetActive(hasReward);
            }

            SetText(briefingRewardText, hasReward ? stageProjection.RewardPreview : string.Empty);
            SetText(briefingDigestText, $"BRIEFING DIGEST  {ShortDigest(stageProjection.CanonicalBriefingDigest)}");
            SetText(
                briefingStatusText,
                "REVIEW BRIEFING READY / NO STAGERUN MUTATION");
            SetButtonInteractable(briefingCompleteButton, true);
        }

        public void CompleteReview()
        {
            if (CurrentPhase != NarrativeReviewPhase.StageBriefing)
            {
                return;
            }

            CurrentPhase = NarrativeReviewPhase.Complete;
            ShowOnly(completeGroup);
            SetText(completeTitleText, "REVIEW FLOW COMPLETE");
            SetText(
                completeSummaryText,
                "ChapterEntry → VisualNovel → TutorialCutscene → StageBriefing\n"
                + $"choice: {narrativeChoiceSummary}\n"
                + $"review generation: {lastReviewReceipt.Generation}\n"
                + $"terminal: {lastReviewReceipt.TerminalReason}\n"
                + $"review briefing dispatch: {completionDispatchCount}\n"
                + "StageRun mutation: none");
        }

        private void ResolveStageProjection()
        {
            stageProjection = null;
            if (stageCatalog == null)
            {
                return;
            }

            stageCatalog.TryCreateFirstRouteProjection(
                UIRouteId.Combat,
                out stageProjection,
                out _);
        }

        private bool IsNarrativeProfileValid()
        {
            return narrativeProfile != null && narrativeProfile.TryValidate(out _);
        }

        private void CaptureNarrativeChoiceSummary()
        {
            narrativeChoiceSummary = narrativeSession != null
                && narrativeSession.SelectedChoiceIds.Count > 0
                    ? string.Join(", ", narrativeSession.SelectedChoiceIds)
                    : "none";
        }

        private void ReleaseNarrativeSession()
        {
            if (narrativeSession != null)
            {
                if (narrativeCompletedHandler != null)
                {
                    narrativeSession.Completed -= narrativeCompletedHandler;
                }

                narrativeSession = null;
            }

            narrativeCompletedHandler = null;
        }

        private void StopNarrativeRoutines()
        {
            if (typewriterRoutine != null)
            {
                StopCoroutine(typewriterRoutine);
                typewriterRoutine = null;
            }

            StopAutoAdvanceRoutine();
            if (narrativeLineText != null)
            {
                narrativeLineText.maxVisibleCharacters = int.MaxValue;
            }
        }

        private void StopAutoAdvanceRoutine()
        {
            if (autoAdvanceRoutine != null)
            {
                StopCoroutine(autoAdvanceRoutine);
                autoAdvanceRoutine = null;
            }
        }

        private void StopVoice()
        {
            if (voiceAudioSource == null)
            {
                return;
            }

            voiceAudioSource.Stop();
            voiceAudioSource.clip = null;
        }

        private void ApplyPortrait(NarrativeSequenceProfile.LineEntry line)
        {
            ApplyPortraitSlot(leftPortraitGroup, leftPortraitImage, line, NarrativePortraitSlot.Left);
            ApplyPortraitSlot(centerPortraitGroup, centerPortraitImage, line, NarrativePortraitSlot.Center);
            ApplyPortraitSlot(rightPortraitGroup, rightPortraitImage, line, NarrativePortraitSlot.Right);
        }

        private static void ApplyPortraitSlot(
            CanvasGroup group,
            Image image,
            NarrativeSequenceProfile.LineEntry line,
            NarrativePortraitSlot slot)
        {
            if (group == null)
            {
                return;
            }

            bool active = line != null
                && line.PortraitSlot == slot
                && line.PortraitSprite != null;
            group.alpha = active ? 1f : 0f;
            group.interactable = false;
            group.blocksRaycasts = false;
            if (image != null)
            {
                image.sprite = active && line != null ? line.PortraitSprite : null;
                image.preserveAspect = true;
            }
        }

        private void ShowOnly(CanvasGroup target)
        {
            SetGroupVisible(chapterEntryGroup, target == chapterEntryGroup);
            SetGroupVisible(visualNovelGroup, target == visualNovelGroup);
            SetGroupVisible(cutsceneControlsGroup, target == cutsceneControlsGroup);
            SetGroupVisible(stageBriefingGroup, target == stageBriefingGroup);
            SetGroupVisible(completeGroup, target == completeGroup);
        }

        private void HideUtilityPanels()
        {
            SetGroupVisible(logGroup, false);
            SetGroupVisible(skipConfirmGroup, false);
            HideChoices();
        }

        private bool IsUtilityPanelOpen()
        {
            return IsGroupVisible(logGroup) || IsGroupVisible(skipConfirmGroup);
        }

        private static bool IsGroupVisible(CanvasGroup group)
        {
            return group != null && group.alpha > 0.5f && group.gameObject.activeInHierarchy;
        }

        private static void SetGroupVisible(CanvasGroup group, bool visible)
        {
            if (group == null)
            {
                return;
            }

            group.alpha = visible ? 1f : 0f;
            group.interactable = visible;
            group.blocksRaycasts = visible;
        }

        private static void SetText(TMP_Text target, string value)
        {
            if (target != null)
            {
                target.text = value ?? string.Empty;
            }
        }

        private static void SetButtonInteractable(Button button, bool interactable)
        {
            if (button != null)
            {
                button.interactable = interactable;
            }
        }

        private static void AddButtonListener(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button != null)
            {
                button.onClick.AddListener(action);
            }
        }

        private static void RemoveButtonListener(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button != null)
            {
                button.onClick.RemoveListener(action);
            }
        }

        private static string ResolveSpeakerName(string speakerId)
        {
            return speakerId switch
            {
                "system" => "SYSTEM",
                "field_agent" => "현장 요원",
                "operator" => "작전 오퍼레이터",
                _ => string.IsNullOrWhiteSpace(speakerId) ? "NARRATION" : speakerId
            };
        }

        private static string ResolveLocalizedText(string localizationKey, string stagingFallback)
        {
            if (!string.IsNullOrWhiteSpace(stagingFallback))
            {
                return stagingFallback;
            }

            return string.IsNullOrWhiteSpace(localizationKey)
                ? string.Empty
                : $"[{localizationKey}]";
        }

        private static string FormatDuration(int milliseconds)
        {
            if (milliseconds <= 0)
            {
                return "--:--";
            }

            TimeSpan duration = TimeSpan.FromMilliseconds(milliseconds);
            return $"{(int)duration.TotalMinutes:00}:{duration.Seconds:00}";
        }

        private static string ShortDigest(string digest)
        {
            if (string.IsNullOrWhiteSpace(digest))
            {
                return "unavailable";
            }

            return digest.Length <= 12 ? digest : digest.Substring(0, 12);
        }
    }
}
