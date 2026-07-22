using System;
using System.Collections.Generic;
using UnityEngine;

namespace DimensionBrawl.Presentation.Narrative
{
    public enum NarrativePortraitSlot
    {
        None,
        Left,
        Center,
        Right
    }

    public enum NarrativePortraitCommandType
    {
        Present,
        HideSpeaker,
        ClearFocus,
        ClearStage
    }

    [CreateAssetMenu(
        menuName = "DimensionBrawl/Narrative/Narrative Sequence Profile",
        fileName = "DB_NarrativeSequence")]
    public sealed class NarrativeSequenceProfile : ScriptableObject
    {
        [Serializable]
        public sealed class ChoiceEntry
        {
            [SerializeField] private string choiceId = string.Empty;
            [SerializeField] private string textLocalizationKey = string.Empty;
            [SerializeField, TextArea] private string stagingFallbackKorean = string.Empty;
            [SerializeField] private string responseLineId = string.Empty;
            [SerializeField] private string responseTextLocalizationKey = string.Empty;
            [SerializeField, TextArea] private string responseStagingFallbackKorean = string.Empty;

            public ChoiceEntry()
            {
            }

            public ChoiceEntry(
                string choiceId,
                string textLocalizationKey,
                string stagingFallbackKorean,
                string responseLineId = "",
                string responseTextLocalizationKey = "",
                string responseStagingFallbackKorean = "")
            {
                Configure(
                    choiceId,
                    textLocalizationKey,
                    stagingFallbackKorean,
                    responseLineId,
                    responseTextLocalizationKey,
                    responseStagingFallbackKorean);
            }

            public string ChoiceId => choiceId;
            public string TextLocalizationKey => textLocalizationKey;
            public string StagingFallbackKorean => stagingFallbackKorean;
            public string ResponseLineId => responseLineId;
            public string ResponseTextLocalizationKey => responseTextLocalizationKey;
            public string ResponseStagingFallbackKorean => responseStagingFallbackKorean;
            public bool HasResponse => !string.IsNullOrWhiteSpace(responseTextLocalizationKey)
                || !string.IsNullOrWhiteSpace(responseStagingFallbackKorean);

            public void Configure(
                string newChoiceId,
                string newTextLocalizationKey,
                string newStagingFallbackKorean,
                string newResponseLineId = "",
                string newResponseTextLocalizationKey = "",
                string newResponseStagingFallbackKorean = "")
            {
                choiceId = newChoiceId ?? string.Empty;
                textLocalizationKey = newTextLocalizationKey ?? string.Empty;
                stagingFallbackKorean = newStagingFallbackKorean ?? string.Empty;
                responseLineId = newResponseLineId ?? string.Empty;
                responseTextLocalizationKey = newResponseTextLocalizationKey ?? string.Empty;
                responseStagingFallbackKorean = newResponseStagingFallbackKorean ?? string.Empty;
            }

            internal ChoiceEntry DeepCopy()
            {
                return new ChoiceEntry(
                    ChoiceId,
                    TextLocalizationKey,
                    StagingFallbackKorean,
                    ResponseLineId,
                    ResponseTextLocalizationKey,
                    ResponseStagingFallbackKorean);
            }
        }

        [Serializable]
        public sealed class PortraitCommandEntry
        {
            [SerializeField] private NarrativePortraitCommandType commandType;
            [SerializeField] private string speakerId = string.Empty;
            [SerializeField] private NarrativePortraitSlot portraitSlot;
            [SerializeField] private string expressionId = string.Empty;
            [SerializeField] private Sprite portraitSpriteOverride;

            public PortraitCommandEntry()
            {
            }

            public PortraitCommandEntry(
                NarrativePortraitCommandType commandType,
                string speakerId = "",
                NarrativePortraitSlot portraitSlot = NarrativePortraitSlot.None,
                string expressionId = "",
                Sprite portraitSpriteOverride = null)
            {
                Configure(
                    commandType,
                    speakerId,
                    portraitSlot,
                    expressionId,
                    portraitSpriteOverride);
            }

            public NarrativePortraitCommandType CommandType => commandType;
            public string SpeakerId => speakerId;
            public NarrativePortraitSlot PortraitSlot => portraitSlot;
            public string ExpressionId => expressionId;
            public Sprite PortraitSpriteOverride => portraitSpriteOverride;

            public void Configure(
                NarrativePortraitCommandType newCommandType,
                string newSpeakerId,
                NarrativePortraitSlot newPortraitSlot,
                string newExpressionId,
                Sprite newPortraitSpriteOverride)
            {
                commandType = newCommandType;
                speakerId = newSpeakerId ?? string.Empty;
                portraitSlot = newPortraitSlot;
                expressionId = newExpressionId ?? string.Empty;
                portraitSpriteOverride = newPortraitSpriteOverride;
            }

            internal PortraitCommandEntry DeepCopy()
            {
                return new PortraitCommandEntry(
                    CommandType,
                    SpeakerId,
                    PortraitSlot,
                    ExpressionId,
                    PortraitSpriteOverride);
            }
        }

        [Serializable]
        public sealed class LineEntry
        {
            [SerializeField] private string lineId = string.Empty;
            [SerializeField] private string textLocalizationKey = string.Empty;
            [SerializeField, TextArea(2, 4)] private string stagingFallbackKorean = string.Empty;
            [SerializeField] private string speakerId = string.Empty;
            [SerializeField] private NarrativePortraitSlot portraitSlot;
            [SerializeField] private string expressionId = string.Empty;
            [SerializeField] private Sprite portraitSprite;
            [SerializeField] private AudioClip voiceClip;
            [SerializeField, Min(0f)] private float autoAdvanceSecondsPerCharacterOverride;
            [SerializeField] private ChoiceEntry[] choices = Array.Empty<ChoiceEntry>();
            [SerializeField] private PortraitCommandEntry[] portraitCommands =
                Array.Empty<PortraitCommandEntry>();

            public LineEntry()
            {
            }

            public LineEntry(
                string lineId,
                string textLocalizationKey,
                string stagingFallbackKorean,
                string speakerId,
                NarrativePortraitSlot portraitSlot,
                string expressionId,
                Sprite portraitSprite = null,
                AudioClip voiceClip = null,
                float autoAdvanceSecondsPerCharacterOverride = 0f,
                ChoiceEntry[] choices = null,
                PortraitCommandEntry[] portraitCommands = null)
            {
                Configure(
                    lineId,
                    textLocalizationKey,
                    stagingFallbackKorean,
                    speakerId,
                    portraitSlot,
                    expressionId,
                    portraitSprite,
                    voiceClip,
                    autoAdvanceSecondsPerCharacterOverride,
                    choices,
                    portraitCommands);
            }

            public string LineId => lineId;
            public string TextLocalizationKey => textLocalizationKey;
            public string StagingFallbackKorean => stagingFallbackKorean;
            public string SpeakerId => speakerId;
            public NarrativePortraitSlot PortraitSlot => portraitSlot;
            public string ExpressionId => expressionId;
            public Sprite PortraitSprite => portraitSprite;
            public AudioClip VoiceClip => voiceClip;
            public float AutoAdvanceSecondsPerCharacterOverride =>
                Mathf.Max(0f, autoAdvanceSecondsPerCharacterOverride);
            public ChoiceEntry[] Choices => CloneChoices(choices);
            public bool HasChoices => choices != null && choices.Length > 0;
            public PortraitCommandEntry[] PortraitCommands =>
                ClonePortraitCommands(portraitCommands);
            public bool HasPortraitCommands =>
                portraitCommands != null && portraitCommands.Length > 0;

            public void Configure(
                string newLineId,
                string newTextLocalizationKey,
                string newStagingFallbackKorean,
                string newSpeakerId,
                NarrativePortraitSlot newPortraitSlot,
                string newExpressionId,
                Sprite newPortraitSprite,
                AudioClip newVoiceClip,
                float newAutoAdvanceSecondsPerCharacterOverride,
                ChoiceEntry[] newChoices,
                PortraitCommandEntry[] newPortraitCommands = null)
            {
                lineId = newLineId ?? string.Empty;
                textLocalizationKey = newTextLocalizationKey ?? string.Empty;
                stagingFallbackKorean = newStagingFallbackKorean ?? string.Empty;
                speakerId = newSpeakerId ?? string.Empty;
                portraitSlot = newPortraitSlot;
                expressionId = newExpressionId ?? string.Empty;
                portraitSprite = newPortraitSprite;
                voiceClip = newVoiceClip;
                autoAdvanceSecondsPerCharacterOverride =
                    Mathf.Max(0f, newAutoAdvanceSecondsPerCharacterOverride);
                choices = CloneChoices(newChoices);
                portraitCommands = ClonePortraitCommands(newPortraitCommands);
            }

            public float ResolveAutoAdvanceDelaySeconds(
                float defaultSecondsPerCharacter,
                int visibleCharacterCount)
            {
                float secondsPerCharacter = AutoAdvanceSecondsPerCharacterOverride > 0f
                    ? AutoAdvanceSecondsPerCharacterOverride
                    : Mathf.Max(0f, defaultSecondsPerCharacter);
                return Mathf.Max(0, visibleCharacterCount) * secondsPerCharacter;
            }

            internal LineEntry DeepCopy()
            {
                return new LineEntry(
                    LineId,
                    TextLocalizationKey,
                    StagingFallbackKorean,
                    SpeakerId,
                    PortraitSlot,
                    ExpressionId,
                    PortraitSprite,
                    VoiceClip,
                    AutoAdvanceSecondsPerCharacterOverride,
                    CloneChoices(choices),
                    ClonePortraitCommands(portraitCommands));
            }

            private static ChoiceEntry[] CloneChoices(ChoiceEntry[] source)
            {
                if (source == null || source.Length == 0)
                {
                    return Array.Empty<ChoiceEntry>();
                }

                var clone = new ChoiceEntry[source.Length];
                for (int i = 0; i < source.Length; i++)
                {
                    clone[i] = source[i]?.DeepCopy();
                }

                return clone;
            }

            private static PortraitCommandEntry[] ClonePortraitCommands(
                PortraitCommandEntry[] source)
            {
                if (source == null || source.Length == 0)
                {
                    return Array.Empty<PortraitCommandEntry>();
                }

                var clone = new PortraitCommandEntry[source.Length];
                for (int i = 0; i < source.Length; i++)
                {
                    clone[i] = source[i]?.DeepCopy();
                }

                return clone;
            }
        }

        private const float MinimumSecondsPerCharacter = 0.001f;

        [SerializeField] private string sequenceId = string.Empty;
        [SerializeField, Min(MinimumSecondsPerCharacter)]
        private float defaultAutoAdvanceSecondsPerCharacter = 0.04f;
        [SerializeField] private LineEntry[] lines = Array.Empty<LineEntry>();

        public string SequenceId => sequenceId;
        public float DefaultAutoAdvanceSecondsPerCharacter =>
            Mathf.Max(MinimumSecondsPerCharacter, defaultAutoAdvanceSecondsPerCharacter);
        public LineEntry[] Lines => CreateLineSnapshot();
        public int LineCount => lines?.Length ?? 0;

        public void Configure(
            string newSequenceId,
            float newDefaultAutoAdvanceSecondsPerCharacter,
            LineEntry[] newLines)
        {
            sequenceId = newSequenceId ?? string.Empty;
            defaultAutoAdvanceSecondsPerCharacter = Mathf.Max(
                MinimumSecondsPerCharacter,
                newDefaultAutoAdvanceSecondsPerCharacter);
            lines = CloneLines(newLines);
        }

        public LineEntry GetLine(int index)
        {
            if (index < 0 || index >= LineCount)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return lines[index]?.DeepCopy();
        }

        internal LineEntry[] CreateLineSnapshot()
        {
            return CloneLines(lines);
        }

        public float ResolveAutoAdvanceDelaySeconds(LineEntry line, int visibleCharacterCount)
        {
            if (line == null)
            {
                return 0f;
            }

            return line.ResolveAutoAdvanceDelaySeconds(
                DefaultAutoAdvanceSecondsPerCharacter,
                visibleCharacterCount);
        }

        public bool TryValidate(out string error)
        {
            var issues = new List<string>();
            CollectValidationIssues(issues);
            error = issues.Count > 0 ? string.Join("\n", issues) : string.Empty;
            return issues.Count == 0;
        }

        public void CollectValidationIssues(List<string> issues)
        {
            if (issues == null)
            {
                return;
            }

            string profileLabel = string.IsNullOrWhiteSpace(name)
                ? nameof(NarrativeSequenceProfile)
                : name;
            if (string.IsNullOrWhiteSpace(sequenceId))
            {
                issues.Add($"{profileLabel}: sequence id is empty.");
            }

            LineEntry[] resolvedLines = lines ?? Array.Empty<LineEntry>();
            if (resolvedLines.Length == 0)
            {
                issues.Add($"{profileLabel}: no narrative lines are authored.");
                return;
            }

            var lineIds = new HashSet<string>(StringComparer.Ordinal);
            var choiceIds = new HashSet<string>(StringComparer.Ordinal);
            for (int lineIndex = 0; lineIndex < resolvedLines.Length; lineIndex++)
            {
                LineEntry line = resolvedLines[lineIndex];
                if (line == null)
                {
                    issues.Add($"{profileLabel}: line {lineIndex} is null.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(line.LineId))
                {
                    issues.Add($"{profileLabel}: line {lineIndex} has no line id.");
                }
                else if (!lineIds.Add(line.LineId))
                {
                    issues.Add($"{profileLabel}: duplicate line id '{line.LineId}'.");
                }

                if (string.IsNullOrWhiteSpace(line.TextLocalizationKey)
                    && string.IsNullOrWhiteSpace(line.StagingFallbackKorean))
                {
                    issues.Add(
                        $"{profileLabel}: line '{line.LineId}' has neither a text localization key nor staging fallback Korean text.");
                }

                ChoiceEntry[] resolvedChoices = line.Choices;
                if (resolvedChoices.Length > 2)
                {
                    issues.Add(
                        $"{profileLabel}: line '{line.LineId}' has {resolvedChoices.Length} choices; at most 2 are supported.");
                }

                for (int choiceIndex = 0; choiceIndex < resolvedChoices.Length; choiceIndex++)
                {
                    ChoiceEntry choice = resolvedChoices[choiceIndex];
                    if (choice == null)
                    {
                        issues.Add(
                            $"{profileLabel}: line '{line.LineId}' choice {choiceIndex} is null.");
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(choice.ChoiceId))
                    {
                        issues.Add(
                            $"{profileLabel}: line '{line.LineId}' choice {choiceIndex} has no choice id.");
                    }
                    else if (!choiceIds.Add(choice.ChoiceId))
                    {
                        issues.Add($"{profileLabel}: duplicate choice id '{choice.ChoiceId}'.");
                    }

                    if (string.IsNullOrWhiteSpace(choice.TextLocalizationKey)
                        && string.IsNullOrWhiteSpace(choice.StagingFallbackKorean))
                    {
                        issues.Add(
                            $"{profileLabel}: choice '{choice.ChoiceId}' has neither a text localization key nor staging fallback Korean text.");
                    }

                    if (choice.HasResponse && string.IsNullOrWhiteSpace(choice.ResponseLineId))
                    {
                        issues.Add(
                            $"{profileLabel}: choice '{choice.ChoiceId}' has response text but no response line id.");
                    }
                    else if (!choice.HasResponse && !string.IsNullOrWhiteSpace(choice.ResponseLineId))
                    {
                        issues.Add(
                            $"{profileLabel}: choice '{choice.ChoiceId}' has a response line id but no response text.");
                    }
                    else if (!string.IsNullOrWhiteSpace(choice.ResponseLineId)
                        && !lineIds.Add(choice.ResponseLineId))
                    {
                        issues.Add(
                            $"{profileLabel}: duplicate line id '{choice.ResponseLineId}'.");
                    }
                }

                PortraitCommandEntry[] portraitCommands = line.PortraitCommands;
                for (int commandIndex = 0; commandIndex < portraitCommands.Length; commandIndex++)
                {
                    PortraitCommandEntry command = portraitCommands[commandIndex];
                    if (command == null)
                    {
                        issues.Add(
                            $"{profileLabel}: line '{line.LineId}' portrait command {commandIndex} is null.");
                        continue;
                    }

                    if (!Enum.IsDefined(
                            typeof(NarrativePortraitCommandType),
                            command.CommandType))
                    {
                        issues.Add(
                            $"{profileLabel}: line '{line.LineId}' portrait command {commandIndex} has undefined command type '{(int)command.CommandType}'.");
                    }

                    if (!Enum.IsDefined(
                            typeof(NarrativePortraitSlot),
                            command.PortraitSlot))
                    {
                        issues.Add(
                            $"{profileLabel}: line '{line.LineId}' portrait command {commandIndex} has undefined portrait slot '{(int)command.PortraitSlot}'.");
                    }

                    bool requiresSpeaker = command.CommandType == NarrativePortraitCommandType.Present
                        || command.CommandType == NarrativePortraitCommandType.HideSpeaker;
                    if (requiresSpeaker && string.IsNullOrWhiteSpace(command.SpeakerId))
                    {
                        issues.Add(
                            $"{profileLabel}: line '{line.LineId}' portrait command {commandIndex} requires a speaker id.");
                    }
                }
            }
        }

        private static LineEntry[] CloneLines(LineEntry[] source)
        {
            if (source == null || source.Length == 0)
            {
                return Array.Empty<LineEntry>();
            }

            var clone = new LineEntry[source.Length];
            for (int i = 0; i < source.Length; i++)
            {
                clone[i] = source[i]?.DeepCopy();
            }

            return clone;
        }

        private void OnValidate()
        {
            defaultAutoAdvanceSecondsPerCharacter = Mathf.Max(
                MinimumSecondsPerCharacter,
                defaultAutoAdvanceSecondsPerCharacter);
            lines ??= Array.Empty<LineEntry>();
        }
    }
}
