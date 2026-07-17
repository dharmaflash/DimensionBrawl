using System;
using DimensionBrawl.Presentation.Narrative;
using NUnit.Framework;
using UnityEngine;

namespace DimensionBrawl.Tests
{
    public sealed class NarrativeSequenceSessionPlayModeTests
    {
        [Test]
        public void NormalFlowRecordsSeenLinesAndCompletesOnceAtSequenceEnd()
        {
            NarrativeSequenceProfile profile = CreateProfile(
                CreateLine("review.olympus.prologue.line.01", "올림포스 신호를 확인했다."),
                CreateLine("review.olympus.prologue.line.02", "게이트를 연다."));
            try
            {
                var session = new NarrativeSequenceSession(profile);
                int completionCount = 0;
                NarrativeSequenceCompletionReason completionReason = default;
                session.Completed += reason =>
                {
                    completionCount++;
                    completionReason = reason;
                };

                Assert.That(session.CurrentLine.LineId, Is.EqualTo("review.olympus.prologue.line.01"));
                Assert.That(session.SeenLineIds, Is.EqualTo(new[]
                {
                    "review.olympus.prologue.line.01"
                }));

                Assert.That(session.Advance(), Is.EqualTo(NarrativeAdvanceResult.Advanced));
                Assert.That(session.CurrentLine.LineId, Is.EqualTo("review.olympus.prologue.line.02"));
                Assert.That(session.SeenLineIds, Is.EqualTo(new[]
                {
                    "review.olympus.prologue.line.01",
                    "review.olympus.prologue.line.02"
                }));

                Assert.That(session.Advance(), Is.EqualTo(NarrativeAdvanceResult.Completed));
                Assert.That(session.IsCompleted, Is.True);
                Assert.That(session.CurrentLine, Is.Null);
                Assert.That(completionCount, Is.EqualTo(1));
                Assert.That(completionReason, Is.EqualTo(NarrativeSequenceCompletionReason.Normal));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(profile);
            }
        }

        [Test]
        public void ChoiceBlocksAdvanceUntilSelectionThenRejoinsNextLine()
        {
            var immediateEntry = new NarrativeSequenceProfile.ChoiceEntry(
                "review.olympus.prologue.choice.enter",
                "narrative.review.olympus.prologue.choice.enter",
                "즉시 진입한다");
            var verifyEntry = new NarrativeSequenceProfile.ChoiceEntry(
                "review.olympus.prologue.choice.verify",
                "narrative.review.olympus.prologue.choice.verify",
                "상황을 한 번 더 확인한다");
            NarrativeSequenceProfile profile = CreateProfile(
                CreateLine(
                    "review.olympus.prologue.line.choice",
                    "어떻게 진행할까?",
                    immediateEntry,
                    verifyEntry),
                CreateLine("review.olympus.prologue.line.rejoin", "진입 절차를 개시한다."));
            try
            {
                var session = new NarrativeSequenceSession(profile);

                Assert.That(session.IsAwaitingChoice, Is.True);
                Assert.That(session.Advance(), Is.EqualTo(NarrativeAdvanceResult.AwaitingChoice));
                Assert.That(session.TrySelectChoice("review.olympus.prologue.choice.unknown"), Is.False);
                Assert.That(session.TrySelectChoice(verifyEntry.ChoiceId), Is.True);
                Assert.That(session.TrySelectChoice(immediateEntry.ChoiceId), Is.False);
                Assert.That(session.IsAwaitingChoice, Is.False);
                Assert.That(session.SelectedChoiceIds, Is.EqualTo(new[] { verifyEntry.ChoiceId }));

                Assert.That(session.Advance(), Is.EqualTo(NarrativeAdvanceResult.Advanced));
                Assert.That(session.CurrentLine.LineId, Is.EqualTo("review.olympus.prologue.line.rejoin"));
                Assert.That(session.Advance(), Is.EqualTo(NarrativeAdvanceResult.Completed));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(profile);
            }
        }

        [Test]
        public void ChoiceResponseIsRecordedAndSessionSnapshotRejectsExternalMutation()
        {
            var choice = new NarrativeSequenceProfile.ChoiceEntry(
                "review.olympus.prologue.choice.verify",
                "narrative.review.olympus.prologue.choice.verify",
                "상황을 한 번 더 확인한다",
                "review.olympus.prologue.response.verify",
                "narrative.review.olympus.prologue.response.verify",
                "스캔을 한 번 더 돌릴게요. 결과는 같아요.");
            NarrativeSequenceProfile.LineEntry authoredLine = CreateLine(
                "review.olympus.prologue.line.choice",
                "어떻게 진행할까?",
                choice);
            NarrativeSequenceProfile profile = CreateProfile(
                authoredLine,
                CreateLine("review.olympus.prologue.line.rejoin", "진입 절차를 개시한다."));
            try
            {
                var session = new NarrativeSequenceSession(profile);

                authoredLine.Configure(
                    "mutated.external.line",
                    string.Empty,
                    "외부에서 변경됨",
                    "mutated",
                    NarrativePortraitSlot.None,
                    string.Empty,
                    null,
                    null,
                    0f,
                    Array.Empty<NarrativeSequenceProfile.ChoiceEntry>());
                NarrativeSequenceProfile.LineEntry exposedCurrent = session.CurrentLine;
                exposedCurrent.Configure(
                    "mutated.session.view",
                    string.Empty,
                    "세션 뷰에서 변경됨",
                    "mutated",
                    NarrativePortraitSlot.None,
                    string.Empty,
                    null,
                    null,
                    0f,
                    Array.Empty<NarrativeSequenceProfile.ChoiceEntry>());

                Assert.That(session.CurrentLine.LineId, Is.EqualTo("review.olympus.prologue.line.choice"));
                Assert.That(session.TrySelectChoice(choice.ChoiceId), Is.True);
                Assert.That(session.SeenLineIds, Is.EqualTo(new[]
                {
                    "review.olympus.prologue.line.choice",
                    "review.olympus.prologue.response.verify"
                }));
                Assert.That(
                    session.TryResolveSeenEntry(
                        "review.olympus.prologue.response.verify",
                        out string speakerId,
                        out string localizationKey,
                        out string fallbackText),
                    Is.True);
                Assert.That(speakerId, Is.EqualTo("review.operator"));
                Assert.That(localizationKey, Is.EqualTo("narrative.review.olympus.prologue.response.verify"));
                Assert.That(fallbackText, Is.EqualTo("스캔을 한 번 더 돌릴게요. 결과는 같아요."));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(profile);
            }
        }

        [Test]
        public void SkipCompletesWithSkippedReasonWithoutMarkingFutureLinesSeen()
        {
            NarrativeSequenceProfile profile = CreateProfile(
                CreateLine("review.olympus.prologue.line.01", "첫 줄"),
                CreateLine("review.olympus.prologue.line.02", "둘째 줄"));
            try
            {
                var session = new NarrativeSequenceSession(profile);
                int completionCount = 0;
                session.Completed += reason =>
                {
                    completionCount++;
                    Assert.That(reason, Is.EqualTo(NarrativeSequenceCompletionReason.Skipped));
                };

                Assert.That(session.Skip(), Is.True);

                Assert.That(session.IsCompleted, Is.True);
                Assert.That(session.CompletionReason, Is.EqualTo(NarrativeSequenceCompletionReason.Skipped));
                Assert.That(session.SeenLineIds, Is.EqualTo(new[]
                {
                    "review.olympus.prologue.line.01"
                }));
                Assert.That(completionCount, Is.EqualTo(1));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(profile);
            }
        }

        [Test]
        public void RepeatedTerminalRequestsDoNotPublishDuplicateCompletion()
        {
            NarrativeSequenceProfile profile = CreateProfile(
                CreateLine("review.olympus.prologue.line.only", "게이트를 연다."));
            try
            {
                var session = new NarrativeSequenceSession(profile);
                int completionCount = 0;
                bool reentrantSkipAccepted = true;
                session.Completed += _ =>
                {
                    completionCount++;
                    reentrantSkipAccepted = session.Skip();
                };

                Assert.That(session.Advance(), Is.EqualTo(NarrativeAdvanceResult.Completed));
                Assert.That(reentrantSkipAccepted, Is.False);
                Assert.That(session.Advance(), Is.EqualTo(NarrativeAdvanceResult.AlreadyCompleted));
                Assert.That(session.Skip(), Is.False);
                Assert.That(completionCount, Is.EqualTo(1));
                Assert.That(session.CompletionReason, Is.EqualTo(NarrativeSequenceCompletionReason.Normal));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(profile);
            }
        }

        [Test]
        public void InvalidProfileIsRejectedBeforeSessionStarts()
        {
            NarrativeSequenceProfile profile =
                ScriptableObject.CreateInstance<NarrativeSequenceProfile>();
            profile.Configure(
                "review.olympus.prologue",
                0.04f,
                new[]
                {
                    CreateLine("review.olympus.prologue.line.duplicate", "첫 줄"),
                    CreateLine("review.olympus.prologue.line.duplicate", "둘째 줄")
                });
            try
            {
                Assert.That(profile.TryValidate(out string validationError), Is.False);
                Assert.That(validationError, Does.Contain("duplicate line id"));

                ArgumentException exception = Assert.Throws<ArgumentException>(
                    () => new NarrativeSequenceSession(profile));
                Assert.That(exception.Message, Does.Contain("duplicate line id"));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(profile);
            }
        }

        private static NarrativeSequenceProfile CreateProfile(
            params NarrativeSequenceProfile.LineEntry[] lines)
        {
            NarrativeSequenceProfile profile =
                ScriptableObject.CreateInstance<NarrativeSequenceProfile>();
            profile.Configure(
                "review.olympus.prologue",
                0.04f,
                lines);
            Assert.That(profile.TryValidate(out string validationError), Is.True, validationError);
            return profile;
        }

        private static NarrativeSequenceProfile.LineEntry CreateLine(
            string lineId,
            string stagingFallbackKorean,
            params NarrativeSequenceProfile.ChoiceEntry[] choices)
        {
            return new NarrativeSequenceProfile.LineEntry(
                lineId,
                "narrative." + lineId,
                stagingFallbackKorean,
                "review.operator",
                NarrativePortraitSlot.Right,
                "neutral",
                choices: choices);
        }
    }
}
