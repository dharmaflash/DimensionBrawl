using System;
using System.Collections;
using System.Reflection;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.Presentation.Narrative;
using DimensionBrawl.UI.NarrativeReview;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace DimensionBrawl.Tests
{
    public sealed class OlympusChapterNarrativeReviewControllerPlayModeTests
    {
        private const string ControllerTypeName =
            "DimensionBrawl.UI.NarrativeReview.OlympusChapterNarrativeReviewController";

        [UnityTest]
        public IEnumerator RuntimeConfigurationBindsButtonsAndReenableResetsReviewState()
        {
            using var fixture = new ControllerFixture(withChoiceResponsePortrait: false);

            fixture.Root.SetActive(true);
            yield return null;

            AssertPhase(fixture.Controller, "ChapterEntry");
            fixture.ChapterEnterButton.onClick.Invoke();
            AssertPhase(fixture.Controller, "VisualNovel");
            Assert.That(ReadProperty<NarrativeSequenceSession>(
                fixture.Controller,
                "NarrativeSession"), Is.Not.Null);

            fixture.NarrativeAutoButton.onClick.Invoke();
            Assert.That(ReadProperty<bool>(fixture.Controller, "AutoAdvanceEnabled"), Is.True);

            fixture.Root.SetActive(false);
            Assert.That(ReadProperty<NarrativeSequenceSession>(
                fixture.Controller,
                "NarrativeSession"), Is.Null);

            fixture.Root.SetActive(true);
            yield return null;

            AssertPhase(fixture.Controller, "ChapterEntry");
            Assert.That(ReadProperty<bool>(fixture.Controller, "AutoAdvanceEnabled"), Is.False);
            Assert.That(ReadProperty<int>(fixture.Controller, "CompletionDispatchCount"), Is.Zero);
            Assert.That(fixture.ChapterEntryGroup.alpha, Is.EqualTo(1f));
            Assert.That(fixture.VisualNovelGroup.alpha, Is.Zero);

            fixture.ChapterEnterButton.onClick.Invoke();
            AssertPhase(fixture.Controller, "VisualNovel");
            Assert.That(ReadProperty<NarrativeSequenceSession>(
                fixture.Controller,
                "NarrativeSession"), Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator DisablingDuringVisualNovelDetachesPriorSessionBeforeFreshGeneration()
        {
            using var fixture = new ControllerFixture(withChoiceResponsePortrait: false);

            fixture.Root.SetActive(true);
            yield return null;
            fixture.ChapterEnterButton.onClick.Invoke();

            NarrativeSequenceSession priorSession = ReadProperty<NarrativeSequenceSession>(
                fixture.Controller,
                "NarrativeSession");
            long priorGeneration = ReadProperty<long>(
                fixture.Controller,
                "ActiveReviewGeneration");

            fixture.Root.SetActive(false);

            NarrativeTutorialReviewReceipt disabledReceipt =
                ReadProperty<NarrativeTutorialReviewReceipt>(
                    fixture.Controller,
                    "LastReviewReceipt");
            Assert.That(disabledReceipt.Generation, Is.EqualTo(priorGeneration));
            Assert.That(disabledReceipt.TerminalReason,
                Is.EqualTo(NarrativeTutorialReviewTerminalReason.OwnerDisabled));
            Assert.That(disabledReceipt.TutorialEntered, Is.False);
            Assert.That(disabledReceipt.CleanupSucceeded, Is.True);
            Assert.That(disabledReceipt.CanEnterReviewBriefing, Is.False);
            Assert.That(ReadProperty<NarrativeSequenceSession>(
                fixture.Controller,
                "NarrativeSession"), Is.Null);
            Assert.That(ReadProperty<int>(
                fixture.Controller,
                "CompletionDispatchCount"), Is.Zero);
            Assert.That(fixture.GameplayCamera.enabled, Is.True);
            Assert.That(fixture.NarrativeCamera.enabled, Is.False);
            Assert.That(fixture.GameplayListener.enabled, Is.True);
            Assert.That(fixture.NarrativeListener.enabled, Is.False);
            Assert.That(fixture.GameplayInput.enabled, Is.True);
            Assert.That(fixture.GameplayHud.gameObject.activeSelf, Is.True);
            Assert.That(ReadProperty<int>(
                fixture.Controller,
                "TutorialStartProbeCount"), Is.Zero);

            fixture.Root.SetActive(true);
            yield return null;
            fixture.ChapterEnterButton.onClick.Invoke();

            long freshGeneration = ReadProperty<long>(
                fixture.Controller,
                "ActiveReviewGeneration");
            NarrativeSequenceSession freshSession = ReadProperty<NarrativeSequenceSession>(
                fixture.Controller,
                "NarrativeSession");
            Assert.That(freshGeneration, Is.GreaterThan(priorGeneration));
            AssertPhase(fixture.Controller, "VisualNovel");

            priorSession.Skip();

            AssertPhase(fixture.Controller, "VisualNovel");
            Assert.That(ReadProperty<long>(
                fixture.Controller,
                "ActiveReviewGeneration"), Is.EqualTo(freshGeneration));
            Assert.That(ReadProperty<NarrativeSequenceSession>(
                fixture.Controller,
                "NarrativeSession"), Is.SameAs(freshSession));
            Assert.That(ReadProperty<int>(
                fixture.Controller,
                "CompletionDispatchCount"), Is.Zero);
            Assert.That(ReadProperty<NarrativeTutorialReviewReceipt>(
                fixture.Controller,
                "LastReviewReceipt").IsValid, Is.False);
        }

        [UnityTest]
        public IEnumerator TutorialCannotBeginBeforeNarrativeSessionCompletes()
        {
            using var fixture = new ControllerFixture(
                withChoiceResponsePortrait: false,
                withCutscenePort: true);

            fixture.Root.SetActive(true);
            yield return null;
            fixture.ChapterEnterButton.onClick.Invoke();

            NarrativeSequenceSession session = ReadProperty<NarrativeSequenceSession>(
                fixture.Controller,
                "NarrativeSession");
            long generation = ReadProperty<long>(
                fixture.Controller,
                "ActiveReviewGeneration");
            Assert.That(session.IsCompleted, Is.False);

            Invoke(fixture.Controller, "BeginTutorialCutscene");

            AssertPhase(fixture.Controller, "VisualNovel");
            Assert.That(ReadProperty<NarrativeSequenceSession>(
                fixture.Controller,
                "NarrativeSession"), Is.SameAs(session));
            Assert.That(ReadProperty<long>(
                fixture.Controller,
                "ActiveReviewGeneration"), Is.EqualTo(generation));
            Assert.That(ReadProperty<NarrativeTutorialReviewPhase>(
                fixture.Controller,
                "ReviewLifecyclePhase"), Is.EqualTo(
                    NarrativeTutorialReviewPhase.VisualNovel));
            Assert.That(fixture.CutsceneDirector.playableGraph.IsValid(), Is.False);
            Assert.That(ReadProperty<NarrativeTutorialReviewReceipt>(
                fixture.Controller,
                "LastReviewReceipt").IsValid, Is.False);
        }

        [UnityTest]
        public IEnumerator MissingStoryTransitionGateFailsClosedBeforeAnyPresentationMutation()
        {
            using var fixture = new ControllerFixture(withChoiceResponsePortrait: false);
            Invoke(
                fixture.Controller,
                "ConfigureStoryTutorialTransitionGate",
                new object[] { null });

            fixture.Root.SetActive(true);
            yield return null;
            fixture.ChapterEnterButton.onClick.Invoke();

            AssertPhase(fixture.Controller, "ChapterEntry");
            Assert.That(ReadProperty<NarrativeSequenceSession>(
                fixture.Controller,
                "NarrativeSession"), Is.Null);
            Assert.That(fixture.GameplayCamera.enabled, Is.True);
            Assert.That(fixture.NarrativeCamera.enabled, Is.False);
            Assert.That(fixture.GameplayListener.enabled, Is.True);
            Assert.That(fixture.NarrativeListener.enabled, Is.False);
            Assert.That(fixture.GameplayInput.enabled, Is.True);
            Assert.That(fixture.GameplayHud.gameObject.activeSelf, Is.True);
            NarrativeTutorialReviewReceipt receipt =
                ReadProperty<NarrativeTutorialReviewReceipt>(
                    fixture.Controller,
                    "LastReviewReceipt");
            Assert.That(receipt.TerminalReason,
                Is.EqualTo(
                    NarrativeTutorialReviewTerminalReason.StoryTransitionUnavailable));
            Assert.That(receipt.TutorialEntered, Is.False);
            Assert.That(receipt.CleanupSucceeded, Is.True);
            Assert.That(receipt.CanEnterReviewBriefing, Is.False);
        }

        [UnityTest]
        public IEnumerator NormalStoryCompletionRestoresEveryReviewDomainBeforeTutorialPlay()
        {
            using var fixture = new ControllerFixture(
                withChoiceResponsePortrait: false,
                withCutscenePort: true);

            Time.timeScale = 0.5f;
            fixture.GameplayHud.alpha = 0.37f;
            fixture.GameplayHud.interactable = false;
            fixture.GameplayHud.blocksRaycasts = true;
            fixture.GameplayInput.enabled = true;
            fixture.GameplayCamera.enabled = true;
            fixture.NarrativeCamera.enabled = false;
            fixture.GameplayListener.enabled = true;
            fixture.NarrativeListener.enabled = false;
            StageRunContext runContextBefore = StageRunRuntime.ActiveContext;
            StageRunAbortRecord abortRecordBefore = StageRunRuntime.LastAbortRecord;

            bool tutorialPlayedAfterRestore = false;
            fixture.CutsceneDirector.played += _ =>
            {
                StoryTutorialReviewReceipt receipt =
                    ReadProperty<StoryTutorialReviewReceipt>(
                        fixture.Controller,
                        "LastStoryTutorialReceipt");
                Assert.That(receipt.CanDispatchReviewTutorialStart, Is.True);
                Assert.That(Time.timeScale, Is.EqualTo(0.5f));
                Assert.That(fixture.GameplayCamera.enabled, Is.True);
                Assert.That(fixture.NarrativeCamera.enabled, Is.False);
                Assert.That(fixture.GameplayListener.enabled, Is.True);
                Assert.That(fixture.NarrativeListener.enabled, Is.False);
                Assert.That(fixture.GameplayInput.enabled, Is.True);
                Assert.That(fixture.GameplayHud.gameObject.activeSelf, Is.True);
                Assert.That(fixture.GameplayHud.alpha, Is.EqualTo(0.37f));
                Assert.That(fixture.GameplayHud.interactable, Is.False);
                Assert.That(fixture.GameplayHud.blocksRaycasts, Is.True);
                tutorialPlayedAfterRestore = true;
            };

            fixture.Root.SetActive(true);
            yield return null;
            fixture.ChapterEnterButton.onClick.Invoke();

            AssertPhase(fixture.Controller, "VisualNovel");
            Assert.That(Time.timeScale, Is.Zero);
            Assert.That(fixture.GameplayCamera.enabled, Is.False);
            Assert.That(fixture.NarrativeCamera.enabled, Is.True);
            Assert.That(fixture.GameplayListener.enabled, Is.False);
            Assert.That(fixture.NarrativeListener.enabled, Is.True);
            Assert.That(fixture.GameplayInput.enabled, Is.False);
            Assert.That(fixture.GameplayHud.gameObject.activeSelf, Is.False);

            fixture.NarrativeNextButton.onClick.Invoke();

            Assert.That(tutorialPlayedAfterRestore, Is.True);
            Assert.That(ReadProperty<int>(
                fixture.Controller,
                "TutorialStartProbeCount"), Is.EqualTo(1));
            AssertPhase(fixture.Controller, "TutorialCutscene");
            Assert.That(ReadProperty<NarrativeSequenceSession>(
                fixture.Controller,
                "NarrativeSession"), Is.Null);
            StoryTutorialReviewReceipt finalReceipt =
                ReadProperty<StoryTutorialReviewReceipt>(
                    fixture.Controller,
                    "LastStoryTutorialReceipt");
            Assert.That(finalReceipt.TerminalReason,
                Is.EqualTo(StoryTutorialReviewTerminalReason.Completed));
            Assert.That(finalReceipt.StoryOwnedWorkReleased, Is.True);
            Assert.That(finalReceipt.StateRestoreSucceeded, Is.True);
            Assert.That(StageRunRuntime.ActiveContext, Is.SameAs(runContextBefore));
            Assert.That(StageRunRuntime.LastAbortRecord, Is.SameAs(abortRecordBefore));
        }

        [UnityTest]
        public IEnumerator ChoiceResponsePreservesPriorPortraitAndSkipFinalizesExactlyOnce()
        {
            using var fixture = new ControllerFixture(
                withChoiceResponsePortrait: true,
                withCutscenePort: true);

            fixture.Root.SetActive(true);
            yield return null;
            fixture.ChapterEnterButton.onClick.Invoke();

            AssertPhase(fixture.Controller, "VisualNovel");
            Assert.That(fixture.LeftPortraitImage.sprite, Is.SameAs(fixture.MarkerSprite));
            Assert.That(fixture.LeftPortraitGroup.alpha, Is.EqualTo(1f));
            Assert.That(fixture.NarrativeChoiceGroup.alpha, Is.EqualTo(1f));

            fixture.FirstChoiceButton.onClick.Invoke();
            NarrativeSequenceSession session = ReadProperty<NarrativeSequenceSession>(
                fixture.Controller,
                "NarrativeSession");
            Assert.That(session.SelectedChoiceIds, Is.EqualTo(new[]
            {
                "review.olympus.prologue.choice.verify"
            }));
            Assert.That(session.SeenLineIds, Does.Contain(
                "review.olympus.prologue.response.verify"));

            fixture.NarrativeNextButton.onClick.Invoke();

            Assert.That(session.CurrentLine.LineId, Is.EqualTo(
                "review.olympus.prologue.line.rejoin"));
            Assert.That(fixture.LeftPortraitImage.sprite, Is.SameAs(fixture.MarkerSprite));
            Assert.That(fixture.RightPortraitImage.sprite, Is.Null);
            Assert.That(fixture.LeftPortraitGroup.alpha, Is.EqualTo(0.48f).Within(0.001f));
            Assert.That(fixture.RightPortraitGroup.alpha, Is.Zero);
            NarrativeVisualNovelPresentationSnapshot presentation =
                ReadProperty<NarrativeVisualNovelPresentationSnapshot>(
                    fixture.Controller,
                    "NarrativePresentationSnapshot");
            Assert.That(presentation.Left.SpeakerId, Is.EqualTo("operator"));
            Assert.That(presentation.Left.IsFocused, Is.False);
            Assert.That(presentation.Right.IsOccupied, Is.False);

            fixture.NarrativeSkipButton.onClick.Invoke();
            Assert.That(fixture.SkipConfirmGroup.alpha, Is.EqualTo(1f));
            fixture.SkipConfirmButton.onClick.Invoke();

            AssertPhase(fixture.Controller, "TutorialCutscene");
            Assert.That(session.IsCompleted, Is.True);
            Assert.That(
                session.CompletionReason,
                Is.EqualTo(NarrativeSequenceCompletionReason.Skipped));
            Assert.That(ReadProperty<NarrativeSequenceSession>(
                fixture.Controller,
                "NarrativeSession"), Is.Null);
            Assert.That(ReadField<string>(
                fixture.Controller,
                "narrativeChoiceSummary"), Is.EqualTo(
                    "review.olympus.prologue.choice.verify"));
            Invoke(fixture.Controller, "SkipCutscene");

            AssertPhase(fixture.Controller, "StageBriefing");
            Assert.That(ReadProperty<int>(
                fixture.Controller,
                "CompletionDispatchCount"), Is.EqualTo(1));
            NarrativeTutorialReviewReceipt receipt =
                ReadProperty<NarrativeTutorialReviewReceipt>(
                    fixture.Controller,
                    "LastReviewReceipt");
            Assert.That(receipt.TerminalReason,
                Is.EqualTo(NarrativeTutorialReviewTerminalReason.Skipped));
            Assert.That(receipt.CanEnterReviewBriefing, Is.True);
            StoryTutorialReviewReceipt storyReceipt =
                ReadProperty<StoryTutorialReviewReceipt>(
                    fixture.Controller,
                    "LastStoryTutorialReceipt");
            Assert.That(storyReceipt.TerminalReason,
                Is.EqualTo(StoryTutorialReviewTerminalReason.Skipped));
            Assert.That(storyReceipt.CanDispatchReviewTutorialStart, Is.True);
            Assert.That(ReadProperty<int>(
                fixture.Controller,
                "TutorialStartProbeCount"), Is.EqualTo(1));

            fixture.SkipConfirmButton.onClick.Invoke();
            Invoke(fixture.Controller, "SkipCutscene");

            AssertPhase(fixture.Controller, "StageBriefing");
            Assert.That(ReadProperty<int>(
                fixture.Controller,
                "CompletionDispatchCount"), Is.EqualTo(1));
            NarrativeTutorialReviewReceipt afterDuplicateSignals =
                ReadProperty<NarrativeTutorialReviewReceipt>(
                    fixture.Controller,
                    "LastReviewReceipt");
            Assert.That(afterDuplicateSignals.Generation, Is.EqualTo(receipt.Generation));
            Assert.That(afterDuplicateSignals.TerminalReason, Is.EqualTo(receipt.TerminalReason));
            Assert.That(afterDuplicateSignals.CanEnterReviewBriefing, Is.True);
        }

        [UnityTest]
        public IEnumerator UnscaledDirectorEndBoundaryFinalizesWhileGameTimeIsPaused()
        {
            using var fixture = new ControllerFixture(
                withChoiceResponsePortrait: false,
                withCutscenePort: true,
                cutsceneUpdateMode: DirectorUpdateMode.UnscaledGameTime);

            fixture.Root.SetActive(true);
            yield return null;

            Assert.That(ReadField<PlayableDirector>(
                fixture.Controller,
                "cutsceneDirector"), Is.Null);
            Assert.That(ReadProperty<bool>(
                fixture.Controller,
                "HasValidCutsceneBoundary"), Is.True);
            fixture.ChapterEnterButton.onClick.Invoke();
            fixture.NarrativeNextButton.onClick.Invoke();

            AssertPhase(fixture.Controller, "TutorialCutscene");
            Assert.That(ReadField<PlayableDirector>(
                fixture.Controller,
                "cutsceneDirector"), Is.SameAs(fixture.CutsceneDirector));
            Assert.That(ReadProperty<NarrativeSequenceSession>(
                fixture.Controller,
                "NarrativeSession"), Is.Null);
            Assert.That(fixture.CutsceneDirector.state, Is.EqualTo(PlayState.Playing));

            float previousTimeScale = Time.timeScale;
            try
            {
                Time.timeScale = 0f;
                yield return WaitForDirectorToStop(fixture.CutsceneDirector);
            }
            finally
            {
                Time.timeScale = previousTimeScale;
            }

            AssertPhase(fixture.Controller, "StageBriefing");
            Assert.That(ReadProperty<int>(
                fixture.Controller,
                "CompletionDispatchCount"), Is.EqualTo(1));

            fixture.CutsceneDirector.Stop();
            Invoke(fixture.Controller, "SkipCutscene");

            AssertPhase(fixture.Controller, "StageBriefing");
            Assert.That(ReadProperty<int>(
                fixture.Controller,
                "CompletionDispatchCount"), Is.EqualTo(1));
            NarrativeTutorialReviewReceipt receipt =
                ReadProperty<NarrativeTutorialReviewReceipt>(
                    fixture.Controller,
                    "LastReviewReceipt");
            Assert.That(receipt.TerminalReason,
                Is.EqualTo(NarrativeTutorialReviewTerminalReason.Completed));
            Assert.That(receipt.CanEnterReviewBriefing, Is.True);
        }

        [UnityTest]
        public IEnumerator GameTimeDirectorEndBoundaryFinalizesAtNonDefaultTimeScale()
        {
            using var fixture = new ControllerFixture(
                withChoiceResponsePortrait: false,
                withCutscenePort: true);

            fixture.Root.SetActive(true);
            yield return null;
            fixture.ChapterEnterButton.onClick.Invoke();
            fixture.NarrativeNextButton.onClick.Invoke();

            Assert.That(fixture.CutsceneDirector.timeUpdateMode,
                Is.EqualTo(DirectorUpdateMode.GameTime));
            float previousTimeScale = Time.timeScale;
            try
            {
                Time.timeScale = 0.5f;
                yield return WaitForDirectorToStop(fixture.CutsceneDirector);
            }
            finally
            {
                Time.timeScale = previousTimeScale;
            }

            AssertPhase(fixture.Controller, "StageBriefing");
            Assert.That(ReadProperty<int>(
                fixture.Controller,
                "CompletionDispatchCount"), Is.EqualTo(1));
            NarrativeTutorialReviewReceipt receipt =
                ReadProperty<NarrativeTutorialReviewReceipt>(
                    fixture.Controller,
                    "LastReviewReceipt");
            Assert.That(receipt.TerminalReason,
                Is.EqualTo(NarrativeTutorialReviewTerminalReason.Completed));
            Assert.That(receipt.CanEnterReviewBriefing, Is.True);
        }

        [UnityTest]
        public IEnumerator StoppingCutsceneBeforeEndFailsClosedWithoutBriefingReadiness()
        {
            using var fixture = new ControllerFixture(
                withChoiceResponsePortrait: false,
                withCutscenePort: true);

            fixture.Root.SetActive(true);
            yield return null;
            fixture.ChapterEnterButton.onClick.Invoke();
            fixture.NarrativeNextButton.onClick.Invoke();

            AssertPhase(fixture.Controller, "TutorialCutscene");
            Assert.That(fixture.CutsceneDirector.time,
                Is.LessThan(fixture.CutsceneDirector.duration));
            fixture.CutsceneDirector.Stop();
            yield return null;

            AssertPhase(fixture.Controller, "TutorialCutscene");
            Assert.That(ReadProperty<int>(
                fixture.Controller,
                "CompletionDispatchCount"), Is.Zero);
            NarrativeTutorialReviewReceipt receipt =
                ReadProperty<NarrativeTutorialReviewReceipt>(
                    fixture.Controller,
                    "LastReviewReceipt");
            Assert.That(receipt.TerminalReason,
                Is.EqualTo(NarrativeTutorialReviewTerminalReason.Cancelled));
            Assert.That(receipt.TutorialEntered, Is.True);
            Assert.That(receipt.CleanupSucceeded, Is.True);
            Assert.That(receipt.CanEnterReviewBriefing, Is.False);
        }

        [UnityTest]
        public IEnumerator RuntimeReconfigurationDuringTutorialCannotDropOwnedDirectorGraph()
        {
            using var fixture = new ControllerFixture(
                withChoiceResponsePortrait: false,
                withCutscenePort: true);

            fixture.Root.SetActive(true);
            yield return null;
            fixture.ChapterEnterButton.onClick.Invoke();
            fixture.NarrativeNextButton.onClick.Invoke();

            Assert.That(fixture.CutsceneDirector.playableGraph.IsValid(), Is.True);
            TargetInvocationException exception = Assert.Throws<TargetInvocationException>(
                () => Invoke(
                    fixture.Controller,
                    "ConfigureCore",
                    fixture.Profile,
                    null,
                    null,
                    null,
                    null));
            Assert.That(exception.InnerException, Is.TypeOf<InvalidOperationException>());
            Assert.That(ReadField<PlayableDirector>(
                fixture.Controller,
                "cutsceneDirector"), Is.SameAs(fixture.CutsceneDirector));
            Assert.That(fixture.CutsceneDirector.playableGraph.IsValid(), Is.True);

            fixture.CutsceneDirector.Stop();
            yield return null;

            AssertPhase(fixture.Controller, "TutorialCutscene");
            Assert.That(ReadProperty<int>(
                fixture.Controller,
                "CompletionDispatchCount"), Is.Zero);
            NarrativeTutorialReviewReceipt receipt =
                ReadProperty<NarrativeTutorialReviewReceipt>(
                    fixture.Controller,
                    "LastReviewReceipt");
            Assert.That(receipt.TerminalReason,
                Is.EqualTo(NarrativeTutorialReviewTerminalReason.Cancelled));
            Assert.That(receipt.CanEnterReviewBriefing, Is.False);
        }

        [UnityTest]
        public IEnumerator SkippingPausedCutsceneAppliesEndStateAndReleasesGraph()
        {
            using var fixture = new ControllerFixture(
                withChoiceResponsePortrait: false,
                withCutscenePort: true);

            fixture.Root.SetActive(true);
            yield return null;
            fixture.ChapterEnterButton.onClick.Invoke();
            fixture.NarrativeNextButton.onClick.Invoke();
            fixture.CutsceneDirector.Pause();

            AssertPhase(fixture.Controller, "TutorialCutscene");
            Assert.That(fixture.CutsceneDirector.playableGraph.IsValid(), Is.True);
            Assert.That(fixture.CutsceneEndStateObserved, Is.False);
            Invoke(fixture.Controller, "SkipCutscene");

            AssertPhase(fixture.Controller, "StageBriefing");
            Assert.That(fixture.CutsceneEndStateObserved, Is.True);
            Assert.That(fixture.CutsceneDirector.playableGraph.IsValid(), Is.False);
            Assert.That(ReadProperty<int>(
                fixture.Controller,
                "CompletionDispatchCount"), Is.EqualTo(1));
            NarrativeTutorialReviewReceipt receipt =
                ReadProperty<NarrativeTutorialReviewReceipt>(
                    fixture.Controller,
                    "LastReviewReceipt");
            Assert.That(receipt.TerminalReason,
                Is.EqualTo(NarrativeTutorialReviewTerminalReason.Skipped));
            Assert.That(receipt.CanEnterReviewBriefing, Is.True);
        }

        [UnityTest]
        public IEnumerator DisablingDuringCutsceneStopsDirectorWithoutBriefingReadiness()
        {
            using var fixture = new ControllerFixture(
                withChoiceResponsePortrait: false,
                withCutscenePort: true);

            fixture.Root.SetActive(true);
            yield return null;
            fixture.ChapterEnterButton.onClick.Invoke();
            fixture.NarrativeNextButton.onClick.Invoke();

            AssertPhase(fixture.Controller, "TutorialCutscene");
            Assert.That(fixture.CutsceneDirector.state, Is.EqualTo(PlayState.Playing));

            fixture.Root.SetActive(false);

            Assert.That(fixture.CutsceneDirector.state, Is.Not.EqualTo(PlayState.Playing));
            AssertPhase(fixture.Controller, "TutorialCutscene");
            Assert.That(ReadProperty<NarrativeSequenceSession>(
                fixture.Controller,
                "NarrativeSession"), Is.Null);
            Assert.That(ReadProperty<int>(
                fixture.Controller,
                "CompletionDispatchCount"), Is.Zero);
            Assert.That(fixture.GameplayCamera.enabled, Is.True);
            Assert.That(fixture.NarrativeCamera.enabled, Is.False);
            Assert.That(fixture.GameplayListener.enabled, Is.True);
            Assert.That(fixture.NarrativeListener.enabled, Is.False);
            Assert.That(fixture.GameplayInput.enabled, Is.True);
            Assert.That(fixture.GameplayHud.gameObject.activeSelf, Is.True);
            Assert.That(ReadProperty<int>(
                fixture.Controller,
                "TutorialStartProbeCount"), Is.EqualTo(1));
            NarrativeTutorialReviewReceipt receipt =
                ReadProperty<NarrativeTutorialReviewReceipt>(
                    fixture.Controller,
                    "LastReviewReceipt");
            Assert.That(receipt.TerminalReason,
                Is.EqualTo(NarrativeTutorialReviewTerminalReason.OwnerDisabled));
            Assert.That(receipt.CleanupSucceeded, Is.True);
            Assert.That(receipt.CanEnterReviewBriefing, Is.False);

            fixture.CutsceneDirector.Stop();
            Invoke(fixture.Controller, "SkipCutscene");

            AssertPhase(fixture.Controller, "TutorialCutscene");
            Assert.That(ReadProperty<int>(
                fixture.Controller,
                "CompletionDispatchCount"), Is.Zero);
            NarrativeTutorialReviewReceipt afterStaleSignals =
                ReadProperty<NarrativeTutorialReviewReceipt>(
                    fixture.Controller,
                    "LastReviewReceipt");
            Assert.That(afterStaleSignals.Generation, Is.EqualTo(receipt.Generation));
            Assert.That(afterStaleSignals.TerminalReason, Is.EqualTo(receipt.TerminalReason));
        }

        [UnityTest]
        public IEnumerator DisablingPausedCutsceneReleasesGraphWithoutBriefingReadiness()
        {
            using var fixture = new ControllerFixture(
                withChoiceResponsePortrait: false,
                withCutscenePort: true);

            fixture.Root.SetActive(true);
            yield return null;
            fixture.ChapterEnterButton.onClick.Invoke();
            fixture.NarrativeNextButton.onClick.Invoke();
            fixture.CutsceneDirector.Pause();

            Assert.That(fixture.CutsceneDirector.playableGraph.IsValid(), Is.True);
            fixture.Root.SetActive(false);

            Assert.That(fixture.CutsceneDirector.playableGraph.IsValid(), Is.False);
            Assert.That(ReadProperty<int>(
                fixture.Controller,
                "CompletionDispatchCount"), Is.Zero);
            NarrativeTutorialReviewReceipt receipt =
                ReadProperty<NarrativeTutorialReviewReceipt>(
                    fixture.Controller,
                    "LastReviewReceipt");
            Assert.That(receipt.TerminalReason,
                Is.EqualTo(NarrativeTutorialReviewTerminalReason.OwnerDisabled));
            Assert.That(receipt.CanEnterReviewBriefing, Is.False);
        }

        [UnityTest]
        public IEnumerator ResettingPausedCutsceneReleasesGraphAndClearsPriorReceipt()
        {
            using var fixture = new ControllerFixture(
                withChoiceResponsePortrait: false,
                withCutscenePort: true);

            fixture.Root.SetActive(true);
            yield return null;
            fixture.ChapterEnterButton.onClick.Invoke();
            fixture.NarrativeNextButton.onClick.Invoke();
            fixture.CutsceneDirector.Pause();

            Assert.That(fixture.CutsceneDirector.playableGraph.IsValid(), Is.True);
            Invoke(fixture.Controller, "BeginChapterEntry");

            AssertPhase(fixture.Controller, "ChapterEntry");
            Assert.That(fixture.CutsceneDirector.playableGraph.IsValid(), Is.False);
            Assert.That(ReadProperty<long>(
                fixture.Controller,
                "ActiveReviewGeneration"), Is.Zero);
            Assert.That(ReadProperty<int>(
                fixture.Controller,
                "CompletionDispatchCount"), Is.Zero);
            Assert.That(ReadProperty<NarrativeTutorialReviewReceipt>(
                fixture.Controller,
                "LastReviewReceipt").IsValid, Is.False);
        }

        [UnityTest]
        public IEnumerator PriorGenerationCutsceneStopCannotFinalizeFreshTutorial()
        {
            using var fixture = new ControllerFixture(
                withChoiceResponsePortrait: false,
                withCutscenePort: true);

            fixture.Root.SetActive(true);
            yield return null;
            fixture.ChapterEnterButton.onClick.Invoke();
            fixture.NarrativeNextButton.onClick.Invoke();

            long priorGeneration = ReadProperty<long>(
                fixture.Controller,
                "ActiveReviewGeneration");
            Invoke(fixture.Controller, "SkipCutscene");
            AssertPhase(fixture.Controller, "StageBriefing");

            Invoke(fixture.Controller, "BeginChapterEntry");
            fixture.ChapterEnterButton.onClick.Invoke();
            fixture.NarrativeNextButton.onClick.Invoke();

            long freshGeneration = ReadProperty<long>(
                fixture.Controller,
                "ActiveReviewGeneration");
            Assert.That(freshGeneration, Is.GreaterThan(priorGeneration));
            AssertPhase(fixture.Controller, "TutorialCutscene");
            Assert.That(fixture.CutsceneDirector.state, Is.EqualTo(PlayState.Playing));

            Invoke(
                fixture.Controller,
                "HandleCutsceneStopped",
                fixture.CutsceneDirector,
                priorGeneration);

            AssertPhase(fixture.Controller, "TutorialCutscene");
            Assert.That(ReadProperty<long>(
                fixture.Controller,
                "ActiveReviewGeneration"), Is.EqualTo(freshGeneration));
            Assert.That(ReadProperty<int>(
                fixture.Controller,
                "CompletionDispatchCount"), Is.Zero);
            Assert.That(ReadProperty<NarrativeTutorialReviewReceipt>(
                fixture.Controller,
                "LastReviewReceipt").IsValid, Is.False);
            Assert.That(fixture.CutsceneDirector.state, Is.EqualTo(PlayState.Playing));

            yield return WaitForDirectorToStop(fixture.CutsceneDirector);

            AssertPhase(fixture.Controller, "StageBriefing");
            Assert.That(ReadProperty<int>(
                fixture.Controller,
                "CompletionDispatchCount"), Is.EqualTo(1));
            NarrativeTutorialReviewReceipt freshReceipt =
                ReadProperty<NarrativeTutorialReviewReceipt>(
                    fixture.Controller,
                    "LastReviewReceipt");
            Assert.That(freshReceipt.Generation, Is.EqualTo(freshGeneration));
            Assert.That(freshReceipt.TerminalReason,
                Is.EqualTo(NarrativeTutorialReviewTerminalReason.Completed));
            Assert.That(freshReceipt.CanEnterReviewBriefing, Is.True);
        }

        [UnityTest]
        public IEnumerator MissingCutsceneBoundaryFailsClosedWithoutBriefing()
        {
            using var fixture = new ControllerFixture(withChoiceResponsePortrait: false);

            fixture.Root.SetActive(true);
            yield return null;
            fixture.ChapterEnterButton.onClick.Invoke();
            fixture.NarrativeNextButton.onClick.Invoke();

            AssertPhase(fixture.Controller, "StoryTransitionBlocked");
            Assert.That(ReadProperty<int>(
                fixture.Controller,
                "CompletionDispatchCount"), Is.Zero);
            Assert.That(ReadProperty<bool>(
                fixture.Controller,
                "CanEnterReviewBriefing"), Is.False);
            NarrativeTutorialReviewReceipt receipt =
                ReadProperty<NarrativeTutorialReviewReceipt>(
                    fixture.Controller,
                    "LastReviewReceipt");
            Assert.That(receipt.TerminalReason,
                Is.EqualTo(
                    NarrativeTutorialReviewTerminalReason.StoryTransitionUnavailable));
            Assert.That(receipt.TutorialEntered, Is.False);
            Assert.That(receipt.CleanupSucceeded, Is.True);
            Assert.That(receipt.CanEnterReviewBriefing, Is.False);
            StoryTutorialReviewReceipt storyReceipt =
                ReadProperty<StoryTutorialReviewReceipt>(
                    fixture.Controller,
                    "LastStoryTutorialReceipt");
            Assert.That(storyReceipt.StateRestoreSucceeded, Is.True);
            Assert.That(storyReceipt.TutorialTargetAvailable, Is.False);
            Assert.That(storyReceipt.CanDispatchReviewTutorialStart, Is.False);
            Assert.That(ReadProperty<int>(
                fixture.Controller,
                "TutorialStartProbeCount"), Is.Zero);
            Assert.That(fixture.GameplayCamera.enabled, Is.True);
            Assert.That(fixture.NarrativeCamera.enabled, Is.False);
            Assert.That(fixture.GameplayListener.enabled, Is.True);
            Assert.That(fixture.NarrativeListener.enabled, Is.False);
            Assert.That(fixture.GameplayInput.enabled, Is.True);
            Assert.That(fixture.GameplayHud.gameObject.activeSelf, Is.True);

            Invoke(fixture.Controller, "SkipCutscene");
            AssertPhase(fixture.Controller, "StoryTransitionBlocked");
            Assert.That(ReadProperty<int>(
                fixture.Controller,
                "CompletionDispatchCount"), Is.Zero);
        }

        private static void AssertPhase(Component controller, string expectedPhase)
        {
            object phase = ReadProperty(controller, "CurrentPhase");
            Assert.That(phase.ToString(), Is.EqualTo(expectedPhase));
        }

        private static IEnumerator WaitForDirectorToStop(PlayableDirector director)
        {
            float deadline = Time.realtimeSinceStartup + 1f;
            while (director.state == PlayState.Playing
                && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.That(director.state, Is.Not.EqualTo(PlayState.Playing));
        }

        private static Type RequireControllerType()
        {
            Type type = Type.GetType(ControllerTypeName + ", DimensionBrawl.Runtime")
                ?? Type.GetType(ControllerTypeName + ", Assembly-CSharp");
            Assert.That(type, Is.Not.Null, $"Missing product type {ControllerTypeName}.");
            return type;
        }

        private static MethodInfo RequireMethod(Type type, string methodName)
        {
            MethodInfo method = type.GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, $"Missing method {type.Name}.{methodName}.");
            return method;
        }

        private static void Invoke(Component target, string methodName, params object[] arguments)
        {
            RequireMethod(target.GetType(), methodName).Invoke(target, arguments);
        }

        private static object ReadProperty(Component target, string propertyName)
        {
            PropertyInfo property = target.GetType().GetProperty(
                propertyName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.That(
                property,
                Is.Not.Null,
                $"Missing property {target.GetType().Name}.{propertyName}.");
            return property.GetValue(target);
        }

        private static T ReadProperty<T>(Component target, string propertyName)
        {
            object value = ReadProperty(target, propertyName);
            return value == null ? default : (T)value;
        }

        private static T ReadField<T>(Component target, string fieldName)
        {
            FieldInfo field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(
                field,
                Is.Not.Null,
                $"Missing field {target.GetType().Name}.{fieldName}.");
            object value = field.GetValue(target);
            return value == null ? default : (T)value;
        }

        private sealed class ControllerFixture : IDisposable
        {
            public ControllerFixture(
                bool withChoiceResponsePortrait,
                bool withCutscenePort = false,
                DirectorUpdateMode cutsceneUpdateMode = DirectorUpdateMode.GameTime)
            {
                initialTimeScale = Time.timeScale;
                MarkerTexture = new Texture2D(2, 2)
                {
                    name = "NarrativeReviewPortraitMarkerTexture"
                };
                MarkerSprite = Sprite.Create(
                    MarkerTexture,
                    new Rect(0f, 0f, 2f, 2f),
                    new Vector2(0.5f, 0.5f));
                MarkerSprite.name = "NarrativeReviewPortraitMarker";
                Profile = withChoiceResponsePortrait
                    ? CreateChoiceResponseProfile(MarkerSprite)
                    : CreateSimpleProfile();

                Root = new GameObject("OlympusNarrativeReviewControllerTest");
                Root.SetActive(false);
                Controller = Root.AddComponent(RequireControllerType());
                Presenter = Root.AddComponent<NarrativeVisualNovelPresenter>();

                GameObject gameplayCameraOwner = new GameObject("GameplayCamera");
                gameplayCameraOwner.transform.SetParent(Root.transform, false);
                GameplayCamera = gameplayCameraOwner.AddComponent<Camera>();
                GameplayListener = gameplayCameraOwner.AddComponent<AudioListener>();
                GameObject narrativeCameraOwner = new GameObject("NarrativeCamera");
                narrativeCameraOwner.transform.SetParent(Root.transform, false);
                NarrativeCamera = narrativeCameraOwner.AddComponent<Camera>();
                NarrativeCamera.enabled = false;
                NarrativeListener = narrativeCameraOwner.AddComponent<AudioListener>();
                NarrativeListener.enabled = false;

                GameplayHud = CreateGroup(Root.transform, "GameplayHud");
                GameplayHud.alpha = 0.73f;
                GameplayHud.interactable = true;
                GameplayHud.blocksRaycasts = false;
                GameplayInput = Root.AddComponent<ReviewGameplayInputProbe>();
                TutorialStartProbe = Root.AddComponent<ReviewTutorialStartProbe>();
                TransitionGate =
                    Root.AddComponent<OlympusStoryTutorialTransitionReviewGate>();
                TransitionGate.Configure(
                    GameplayCamera,
                    NarrativeCamera,
                    GameplayHud,
                    GameplayInput,
                    GameplayListener,
                    NarrativeListener,
                    TutorialStartProbe);
                if (withCutscenePort)
                {
                    GameObject cutsceneOwner = new GameObject("CutscenePayloadRoot");
                    cutsceneOwner.transform.SetParent(Root.transform, false);
                    CutsceneDirector = cutsceneOwner.AddComponent<PlayableDirector>();
                    CutsceneDirector.playOnAwake = false;
                    CutsceneDirector.extrapolationMode = DirectorWrapMode.None;
                    CutsceneDirector.timeUpdateMode = cutsceneUpdateMode;
                    CutsceneAsset = ScriptableObject.CreateInstance<ReviewPlayableAsset>();
                    CutsceneAsset.name = "NarrativeReviewTestPlayable";
                    CutsceneDirector.playableAsset = CutsceneAsset;
                    CutscenePort = cutsceneOwner.AddComponent<StageCutscenePort>();
                    CutscenePort.Configure(
                        "review.olympus.prologue.intro",
                        StageCutscenePortKind.Intro,
                        "review.olympus.prologue.handoff",
                        "review.olympus.prologue.anchor",
                        "review.olympus.prologue.runtime",
                        cutsceneOwner.transform,
                        "Review-only port-bound director test.");
                    CutscenePort.ConfigurePresentationBinding(null, CutsceneDirector);
                }

                ChapterEntryGroup = CreateGroup(Root.transform, "ChapterEntryGroup");
                VisualNovelGroup = CreateGroup(Root.transform, "VisualNovelGroup");
                CutsceneGroup = CreateGroup(Root.transform, "CutsceneGroup");
                StageBriefingGroup = CreateGroup(Root.transform, "StageBriefingGroup");
                CompleteGroup = CreateGroup(Root.transform, "CompleteGroup");
                LeftPortraitGroup = CreateGroup(Root.transform, "LeftPortraitGroup");
                CenterPortraitGroup = CreateGroup(Root.transform, "CenterPortraitGroup");
                RightPortraitGroup = CreateGroup(Root.transform, "RightPortraitGroup");
                NarrativeChoiceGroup = CreateGroup(Root.transform, "NarrativeChoiceGroup");
                SkipConfirmGroup = CreateGroup(Root.transform, "SkipConfirmGroup");

                LeftPortraitImage = CreateImage(LeftPortraitGroup.transform, "LeftPortraitImage");
                CenterPortraitImage = CreateImage(
                    CenterPortraitGroup.transform,
                    "CenterPortraitImage");
                RightPortraitImage = CreateImage(
                    RightPortraitGroup.transform,
                    "RightPortraitImage");
                ChapterEnterButton = CreateButton(Root.transform, "ChapterEnterButton");
                NarrativeNextButton = CreateButton(Root.transform, "NarrativeNextButton");
                NarrativeAutoButton = CreateButton(Root.transform, "NarrativeAutoButton");
                NarrativeSkipButton = CreateButton(Root.transform, "NarrativeSkipButton");
                FirstChoiceButton = CreateButton(Root.transform, "FirstChoiceButton");
                SecondChoiceButton = CreateButton(Root.transform, "SecondChoiceButton");
                SkipConfirmButton = CreateButton(Root.transform, "SkipConfirmButton");
                SkipCancelButton = CreateButton(Root.transform, "SkipCancelButton");

                ConfigureController();
            }

            public GameObject Root { get; }
            public Component Controller { get; }
            public NarrativeVisualNovelPresenter Presenter { get; }
            public NarrativeSequenceProfile Profile { get; }
            public Texture2D MarkerTexture { get; }
            public Sprite MarkerSprite { get; }
            public PlayableAsset CutsceneAsset { get; }
            public PlayableDirector CutsceneDirector { get; }
            public StageCutscenePort CutscenePort { get; }
            public Camera GameplayCamera { get; }
            public Camera NarrativeCamera { get; }
            public AudioListener GameplayListener { get; }
            public AudioListener NarrativeListener { get; }
            public CanvasGroup GameplayHud { get; }
            public ReviewGameplayInputProbe GameplayInput { get; }
            public ReviewTutorialStartProbe TutorialStartProbe { get; }
            public OlympusStoryTutorialTransitionReviewGate TransitionGate { get; }
            public CanvasGroup ChapterEntryGroup { get; }
            public CanvasGroup VisualNovelGroup { get; }
            public CanvasGroup CutsceneGroup { get; }
            public CanvasGroup StageBriefingGroup { get; }
            public CanvasGroup CompleteGroup { get; }
            public CanvasGroup LeftPortraitGroup { get; }
            public CanvasGroup CenterPortraitGroup { get; }
            public CanvasGroup RightPortraitGroup { get; }
            public CanvasGroup NarrativeChoiceGroup { get; }
            public CanvasGroup SkipConfirmGroup { get; }
            public Image LeftPortraitImage { get; }
            public Image CenterPortraitImage { get; }
            public Image RightPortraitImage { get; }
            public Button ChapterEnterButton { get; }
            public Button NarrativeNextButton { get; }
            public Button NarrativeAutoButton { get; }
            public Button NarrativeSkipButton { get; }
            public Button FirstChoiceButton { get; }
            public Button SecondChoiceButton { get; }
            public Button SkipConfirmButton { get; }
            public Button SkipCancelButton { get; }
            public bool CutsceneEndStateObserved =>
                CutsceneAsset is ReviewPlayableAsset asset && asset.EndStateObserved;

            private readonly float initialTimeScale;

            public void Dispose()
            {
                Time.timeScale = initialTimeScale;
                UnityEngine.Object.DestroyImmediate(Root);
                UnityEngine.Object.DestroyImmediate(Profile);
                UnityEngine.Object.DestroyImmediate(CutsceneAsset);
                UnityEngine.Object.DestroyImmediate(MarkerSprite);
                UnityEngine.Object.DestroyImmediate(MarkerTexture);
            }

            private void ConfigureController()
            {
                Presenter.Configure(
                    null,
                    LeftPortraitGroup,
                    CenterPortraitGroup,
                    RightPortraitGroup,
                    LeftPortraitImage,
                    CenterPortraitImage,
                    RightPortraitImage,
                    null,
                    null,
                    null);
                Invoke(Controller, "ConfigureCore", Profile, null, null, CutscenePort, null);
                Invoke(Controller, "ConfigureNarrativePresenter", Presenter);
                Invoke(
                    Controller,
                    "ConfigureStoryTutorialTransitionGate",
                    TransitionGate);
                Invoke(
                    Controller,
                    "ConfigureFlowGroups",
                    ChapterEntryGroup,
                    VisualNovelGroup,
                    CutsceneGroup,
                    StageBriefingGroup,
                    CompleteGroup);
                Invoke(
                    Controller,
                    "ConfigureChapterView",
                    null,
                    null,
                    null,
                    null,
                    null,
                    ChapterEnterButton);
                Invoke(
                    Controller,
                    "ConfigureNarrativeView",
                    null,
                    null,
                    null,
                    null,
                    LeftPortraitGroup,
                    CenterPortraitGroup,
                    RightPortraitGroup,
                    LeftPortraitImage,
                    CenterPortraitImage,
                    RightPortraitImage,
                    NarrativeNextButton,
                    NarrativeAutoButton,
                    null,
                    NarrativeSkipButton,
                    null,
                    NarrativeChoiceGroup,
                    FirstChoiceButton,
                    null,
                    SecondChoiceButton,
                    null);
                Invoke(
                    Controller,
                    "ConfigureUtilityPanels",
                    null,
                    null,
                    null,
                    SkipConfirmGroup,
                    SkipConfirmButton,
                    SkipCancelButton);
            }
        }

        private static NarrativeSequenceProfile CreateSimpleProfile()
        {
            return CreateProfile(new NarrativeSequenceProfile.LineEntry(
                "review.olympus.prologue.line.only",
                "narrative.review.olympus.prologue.line.only",
                "게이트 신호를 확인했다.",
                "operator",
                NarrativePortraitSlot.None,
                "neutral"));
        }

        private static NarrativeSequenceProfile CreateChoiceResponseProfile(Sprite portrait)
        {
            var choice = new NarrativeSequenceProfile.ChoiceEntry(
                "review.olympus.prologue.choice.verify",
                "narrative.review.olympus.prologue.choice.verify",
                "상황을 한 번 더 확인한다",
                "review.olympus.prologue.response.verify",
                "narrative.review.olympus.prologue.response.verify",
                "스캔을 한 번 더 돌릴게요. 결과는 같아요.");
            return CreateProfile(
                new NarrativeSequenceProfile.LineEntry(
                    "review.olympus.prologue.line.choice",
                    "narrative.review.olympus.prologue.line.choice",
                    "어떻게 진행할까?",
                    "operator",
                    NarrativePortraitSlot.Left,
                    "alert",
                    portraitSprite: portrait,
                    choices: new[] { choice }),
                new NarrativeSequenceProfile.LineEntry(
                    "review.olympus.prologue.line.rejoin",
                    "narrative.review.olympus.prologue.line.rejoin",
                    "진입 절차를 개시한다.",
                    "field_agent",
                    NarrativePortraitSlot.Right,
                    "neutral"));
        }

        private static NarrativeSequenceProfile CreateProfile(
            params NarrativeSequenceProfile.LineEntry[] lines)
        {
            NarrativeSequenceProfile profile =
                ScriptableObject.CreateInstance<NarrativeSequenceProfile>();
            profile.Configure("review.olympus.prologue", 0.04f, lines);
            Assert.That(profile.TryValidate(out string validationError), Is.True, validationError);
            return profile;
        }

        private static CanvasGroup CreateGroup(Transform parent, string name)
        {
            var owner = new GameObject(name, typeof(RectTransform), typeof(CanvasGroup));
            owner.transform.SetParent(parent, false);
            return owner.GetComponent<CanvasGroup>();
        }

        private static Image CreateImage(Transform parent, string name)
        {
            var owner = new GameObject(
                name,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            owner.transform.SetParent(parent, false);
            return owner.GetComponent<Image>();
        }

        private static Button CreateButton(Transform parent, string name)
        {
            var owner = new GameObject(
                name,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Button));
            owner.transform.SetParent(parent, false);
            return owner.GetComponent<Button>();
        }

        private sealed class ReviewPlayableAsset : PlayableAsset
        {
            public override double duration => 0.15d;
            public bool EndStateObserved { get; private set; }

            public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
            {
                EndStateObserved = false;
                ScriptPlayable<ReviewEndStateProbeBehaviour> playable =
                    ScriptPlayable<ReviewEndStateProbeBehaviour>.Create(graph);
                playable.SetDuration(duration);
                playable.GetBehaviour().Configure(this);
                return playable;
            }

            public void ObserveTime(double observedTime)
            {
                if (observedTime >= duration - 0.001d)
                {
                    EndStateObserved = true;
                }
            }
        }

        private sealed class ReviewEndStateProbeBehaviour : PlayableBehaviour
        {
            private ReviewPlayableAsset owner;

            public ReviewEndStateProbeBehaviour()
            {
            }

            public void Configure(ReviewPlayableAsset newOwner)
            {
                owner = newOwner;
            }

            public override void PrepareFrame(Playable playable, FrameData info)
            {
                owner?.ObserveTime(playable.GetTime());
            }
        }
    }
}
