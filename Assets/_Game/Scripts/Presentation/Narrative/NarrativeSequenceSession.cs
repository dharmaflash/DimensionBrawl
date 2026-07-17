using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DimensionBrawl.Presentation.Narrative
{
    public enum NarrativeSequenceCompletionReason
    {
        Normal,
        Skipped
    }

    public enum NarrativeAdvanceResult
    {
        Advanced,
        AwaitingChoice,
        Completed,
        AlreadyCompleted
    }

    public sealed class NarrativeSequenceSession
    {
        private readonly NarrativeSequenceProfile.LineEntry[] lines;
        private readonly List<string> seenLineIds = new List<string>();
        private readonly List<string> selectedChoiceIds = new List<string>();
        private readonly ReadOnlyCollection<string> seenLineIdsView;
        private readonly ReadOnlyCollection<string> selectedChoiceIdsView;
        private int currentLineIndex;
        private int resolvedChoiceLineIndex = -1;
        private bool isCompleted;
        private NarrativeSequenceCompletionReason completionReason;

        public NarrativeSequenceSession(NarrativeSequenceProfile profile)
        {
            Profile = profile != null
                ? profile
                : throw new ArgumentNullException(nameof(profile));
            if (!profile.TryValidate(out string validationError))
            {
                throw new ArgumentException(validationError, nameof(profile));
            }

            lines = (NarrativeSequenceProfile.LineEntry[])profile.Lines.Clone();
            seenLineIdsView = seenLineIds.AsReadOnly();
            selectedChoiceIdsView = selectedChoiceIds.AsReadOnly();
            currentLineIndex = 0;
            RecordCurrentLineAsSeen();
        }

        public event Action<NarrativeSequenceCompletionReason> Completed;

        public NarrativeSequenceProfile Profile { get; }
        public int CurrentLineIndex => currentLineIndex;
        public NarrativeSequenceProfile.LineEntry CurrentLine =>
            CurrentLineInternal?.DeepCopy();
        private NarrativeSequenceProfile.LineEntry CurrentLineInternal =>
            !isCompleted && currentLineIndex >= 0 && currentLineIndex < lines.Length
                ? lines[currentLineIndex]
                : null;
        public bool IsCompleted => isCompleted;
        public bool IsAwaitingChoice =>
            !isCompleted
            && CurrentLine != null
            && CurrentLine.HasChoices
            && resolvedChoiceLineIndex != currentLineIndex;
        public NarrativeSequenceCompletionReason CompletionReason => completionReason;
        public IReadOnlyList<string> SeenLineIds => seenLineIdsView;
        public IReadOnlyList<string> SelectedChoiceIds => selectedChoiceIdsView;

        public NarrativeAdvanceResult Advance()
        {
            if (isCompleted)
            {
                return NarrativeAdvanceResult.AlreadyCompleted;
            }

            if (IsAwaitingChoice)
            {
                return NarrativeAdvanceResult.AwaitingChoice;
            }

            if (currentLineIndex >= lines.Length - 1)
            {
                Complete(NarrativeSequenceCompletionReason.Normal);
                return NarrativeAdvanceResult.Completed;
            }

            currentLineIndex++;
            RecordCurrentLineAsSeen();
            return NarrativeAdvanceResult.Advanced;
        }

        public bool TrySelectChoice(string choiceId)
        {
            if (isCompleted
                || !IsAwaitingChoice
                || string.IsNullOrWhiteSpace(choiceId))
            {
                return false;
            }

            NarrativeSequenceProfile.LineEntry currentLine = CurrentLineInternal;
            NarrativeSequenceProfile.ChoiceEntry[] choices = currentLine.Choices;
            for (int i = 0; i < choices.Length; i++)
            {
                NarrativeSequenceProfile.ChoiceEntry choice = choices[i];
                if (choice != null
                    && string.Equals(choice.ChoiceId, choiceId, StringComparison.Ordinal))
                {
                    selectedChoiceIds.Add(choice.ChoiceId);
                    resolvedChoiceLineIndex = currentLineIndex;
                    if (choice.HasResponse && !string.IsNullOrWhiteSpace(choice.ResponseLineId))
                    {
                        seenLineIds.Add(choice.ResponseLineId);
                    }

                    return true;
                }
            }

            return false;
        }

        public bool Skip()
        {
            if (isCompleted)
            {
                return false;
            }

            Complete(NarrativeSequenceCompletionReason.Skipped);
            return true;
        }

        public bool TryResolveSeenEntry(
            string lineId,
            out string speakerId,
            out string textLocalizationKey,
            out string stagingFallbackKorean)
        {
            speakerId = string.Empty;
            textLocalizationKey = string.Empty;
            stagingFallbackKorean = string.Empty;
            if (string.IsNullOrWhiteSpace(lineId))
            {
                return false;
            }

            for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                NarrativeSequenceProfile.LineEntry line = lines[lineIndex];
                if (line == null)
                {
                    continue;
                }

                if (string.Equals(line.LineId, lineId, StringComparison.Ordinal))
                {
                    speakerId = line.SpeakerId;
                    textLocalizationKey = line.TextLocalizationKey;
                    stagingFallbackKorean = line.StagingFallbackKorean;
                    return true;
                }

                NarrativeSequenceProfile.ChoiceEntry[] choices = line.Choices;
                for (int choiceIndex = 0; choiceIndex < choices.Length; choiceIndex++)
                {
                    NarrativeSequenceProfile.ChoiceEntry choice = choices[choiceIndex];
                    if (choice == null
                        || !string.Equals(choice.ResponseLineId, lineId, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    speakerId = line.SpeakerId;
                    textLocalizationKey = choice.ResponseTextLocalizationKey;
                    stagingFallbackKorean = choice.ResponseStagingFallbackKorean;
                    return true;
                }
            }

            return false;
        }

        private void RecordCurrentLineAsSeen()
        {
            NarrativeSequenceProfile.LineEntry line = CurrentLineInternal;
            if (line != null)
            {
                seenLineIds.Add(line.LineId);
            }
        }

        private void Complete(NarrativeSequenceCompletionReason reason)
        {
            if (isCompleted)
            {
                return;
            }

            isCompleted = true;
            completionReason = reason;
            currentLineIndex = lines.Length;
            Completed?.Invoke(reason);
        }
    }
}
