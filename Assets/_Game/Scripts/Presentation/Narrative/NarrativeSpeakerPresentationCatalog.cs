using System;
using System.Collections.Generic;
using UnityEngine;

namespace DimensionBrawl.Presentation.Narrative
{
    public readonly struct NarrativeSpeakerPresentation
    {
        public NarrativeSpeakerPresentation(
            string speakerId,
            string stagingDisplayName,
            NarrativePortraitSlot defaultPortraitSlot,
            string expressionId,
            Sprite portraitSprite)
        {
            SpeakerId = speakerId ?? string.Empty;
            StagingDisplayName = stagingDisplayName ?? string.Empty;
            DefaultPortraitSlot = defaultPortraitSlot;
            ExpressionId = expressionId ?? string.Empty;
            PortraitSprite = portraitSprite;
        }

        public string SpeakerId { get; }
        public string StagingDisplayName { get; }
        public NarrativePortraitSlot DefaultPortraitSlot { get; }
        public string ExpressionId { get; }
        public Sprite PortraitSprite { get; }
        public bool HasPortrait => PortraitSprite != null;
    }

    [CreateAssetMenu(
        menuName = "DimensionBrawl/Narrative/Narrative Speaker Presentation Catalog",
        fileName = "DB_NarrativeSpeakerPresentationCatalog")]
    public sealed class NarrativeSpeakerPresentationCatalog : ScriptableObject
    {
        [Serializable]
        public sealed class ExpressionEntry
        {
            [SerializeField] private string expressionId = string.Empty;
            [SerializeField] private Sprite portraitSprite;

            public ExpressionEntry()
            {
            }

            public ExpressionEntry(string expressionId, Sprite portraitSprite)
            {
                Configure(expressionId, portraitSprite);
            }

            public string ExpressionId => expressionId;
            public Sprite PortraitSprite => portraitSprite;

            public void Configure(string newExpressionId, Sprite newPortraitSprite)
            {
                expressionId = newExpressionId ?? string.Empty;
                portraitSprite = newPortraitSprite;
            }

            internal ExpressionEntry DeepCopy()
            {
                return new ExpressionEntry(ExpressionId, PortraitSprite);
            }
        }

        [Serializable]
        public sealed class SpeakerEntry
        {
            [SerializeField] private string speakerId = string.Empty;
            [SerializeField] private string stagingDisplayName = string.Empty;
            [SerializeField] private NarrativePortraitSlot defaultPortraitSlot;
            [SerializeField] private ExpressionEntry[] expressions = Array.Empty<ExpressionEntry>();

            public SpeakerEntry()
            {
            }

            public SpeakerEntry(
                string speakerId,
                string stagingDisplayName,
                NarrativePortraitSlot defaultPortraitSlot,
                ExpressionEntry[] expressions)
            {
                Configure(speakerId, stagingDisplayName, defaultPortraitSlot, expressions);
            }

            public string SpeakerId => speakerId;
            public string StagingDisplayName => stagingDisplayName;
            public NarrativePortraitSlot DefaultPortraitSlot => defaultPortraitSlot;
            public ExpressionEntry[] Expressions => CloneExpressions(expressions);

            public void Configure(
                string newSpeakerId,
                string newStagingDisplayName,
                NarrativePortraitSlot newDefaultPortraitSlot,
                ExpressionEntry[] newExpressions)
            {
                speakerId = newSpeakerId ?? string.Empty;
                stagingDisplayName = newStagingDisplayName ?? string.Empty;
                defaultPortraitSlot = newDefaultPortraitSlot;
                expressions = CloneExpressions(newExpressions);
            }

            internal bool TryResolveExpression(
                string requestedExpressionId,
                out ExpressionEntry resolvedExpression)
            {
                ExpressionEntry[] source = expressions ?? Array.Empty<ExpressionEntry>();
                resolvedExpression = FindExpression(source, requestedExpressionId)
                    ?? FindExpression(source, "neutral");
                if (resolvedExpression == null)
                {
                    for (int i = 0; i < source.Length; i++)
                    {
                        if (source[i] != null && source[i].PortraitSprite != null)
                        {
                            resolvedExpression = source[i];
                            break;
                        }
                    }
                }

                return resolvedExpression != null && resolvedExpression.PortraitSprite != null;
            }

            internal SpeakerEntry DeepCopy()
            {
                return new SpeakerEntry(
                    SpeakerId,
                    StagingDisplayName,
                    DefaultPortraitSlot,
                    CloneExpressions(expressions));
            }

            private static ExpressionEntry FindExpression(
                IReadOnlyList<ExpressionEntry> source,
                string expressionId)
            {
                if (source == null || string.IsNullOrWhiteSpace(expressionId))
                {
                    return null;
                }

                for (int i = 0; i < source.Count; i++)
                {
                    ExpressionEntry candidate = source[i];
                    if (candidate != null
                        && string.Equals(
                            candidate.ExpressionId,
                            expressionId,
                            StringComparison.Ordinal))
                    {
                        return candidate;
                    }
                }

                return null;
            }

            private static ExpressionEntry[] CloneExpressions(ExpressionEntry[] source)
            {
                if (source == null || source.Length == 0)
                {
                    return Array.Empty<ExpressionEntry>();
                }

                var clone = new ExpressionEntry[source.Length];
                for (int i = 0; i < source.Length; i++)
                {
                    clone[i] = source[i]?.DeepCopy();
                }

                return clone;
            }
        }

        [SerializeField] private string catalogId = string.Empty;
        [SerializeField] private SpeakerEntry[] speakers = Array.Empty<SpeakerEntry>();

        public string CatalogId => catalogId;
        public SpeakerEntry[] Speakers => CloneSpeakers(speakers);
        public int SpeakerCount => speakers?.Length ?? 0;

        public void Configure(string newCatalogId, SpeakerEntry[] newSpeakers)
        {
            catalogId = newCatalogId ?? string.Empty;
            speakers = CloneSpeakers(newSpeakers);
        }

        public bool TryResolve(
            string speakerId,
            string expressionId,
            out NarrativeSpeakerPresentation presentation)
        {
            SpeakerEntry speaker = FindSpeaker(speakerId);
            if (speaker == null
                || !speaker.TryResolveExpression(expressionId, out ExpressionEntry expression))
            {
                presentation = default;
                return false;
            }

            presentation = new NarrativeSpeakerPresentation(
                speaker.SpeakerId,
                speaker.StagingDisplayName,
                speaker.DefaultPortraitSlot,
                expression.ExpressionId,
                expression.PortraitSprite);
            return true;
        }

        public bool TryResolveDisplayName(string speakerId, out string displayName)
        {
            SpeakerEntry speaker = FindSpeaker(speakerId);
            displayName = speaker?.StagingDisplayName ?? string.Empty;
            return !string.IsNullOrWhiteSpace(displayName);
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

            string label = string.IsNullOrWhiteSpace(name)
                ? nameof(NarrativeSpeakerPresentationCatalog)
                : name;
            if (string.IsNullOrWhiteSpace(catalogId))
            {
                issues.Add($"{label}: catalog id is empty.");
            }

            SpeakerEntry[] resolvedSpeakers = speakers ?? Array.Empty<SpeakerEntry>();
            if (resolvedSpeakers.Length == 0)
            {
                issues.Add($"{label}: no speaker presentations are authored.");
                return;
            }

            var speakerIds = new HashSet<string>(StringComparer.Ordinal);
            for (int speakerIndex = 0; speakerIndex < resolvedSpeakers.Length; speakerIndex++)
            {
                SpeakerEntry speaker = resolvedSpeakers[speakerIndex];
                if (speaker == null)
                {
                    issues.Add($"{label}: speaker {speakerIndex} is null.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(speaker.SpeakerId))
                {
                    issues.Add($"{label}: speaker {speakerIndex} has no speaker id.");
                }
                else if (!speakerIds.Add(speaker.SpeakerId))
                {
                    issues.Add($"{label}: duplicate speaker id '{speaker.SpeakerId}'.");
                }

                if (string.IsNullOrWhiteSpace(speaker.StagingDisplayName))
                {
                    issues.Add($"{label}: speaker '{speaker.SpeakerId}' has no staging display name.");
                }

                if (!Enum.IsDefined(
                        typeof(NarrativePortraitSlot),
                        speaker.DefaultPortraitSlot))
                {
                    issues.Add(
                        $"{label}: speaker '{speaker.SpeakerId}' has undefined default portrait slot '{(int)speaker.DefaultPortraitSlot}'.");
                }
                else if (speaker.DefaultPortraitSlot == NarrativePortraitSlot.None)
                {
                    issues.Add($"{label}: speaker '{speaker.SpeakerId}' has no default portrait slot.");
                }

                ExpressionEntry[] expressions = speaker.Expressions;
                if (expressions.Length == 0)
                {
                    issues.Add($"{label}: speaker '{speaker.SpeakerId}' has no expressions.");
                    continue;
                }

                var expressionIds = new HashSet<string>(StringComparer.Ordinal);
                for (int expressionIndex = 0; expressionIndex < expressions.Length; expressionIndex++)
                {
                    ExpressionEntry expression = expressions[expressionIndex];
                    if (expression == null)
                    {
                        issues.Add(
                            $"{label}: speaker '{speaker.SpeakerId}' expression {expressionIndex} is null.");
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(expression.ExpressionId))
                    {
                        issues.Add(
                            $"{label}: speaker '{speaker.SpeakerId}' expression {expressionIndex} has no id.");
                    }
                    else if (!expressionIds.Add(expression.ExpressionId))
                    {
                        issues.Add(
                            $"{label}: speaker '{speaker.SpeakerId}' has duplicate expression id '{expression.ExpressionId}'.");
                    }

                    if (expression.PortraitSprite == null)
                    {
                        issues.Add(
                            $"{label}: speaker '{speaker.SpeakerId}' expression '{expression.ExpressionId}' has no portrait Sprite.");
                    }
                }
            }
        }

        private SpeakerEntry FindSpeaker(string speakerId)
        {
            if (string.IsNullOrWhiteSpace(speakerId))
            {
                return null;
            }

            SpeakerEntry[] source = speakers ?? Array.Empty<SpeakerEntry>();
            for (int i = 0; i < source.Length; i++)
            {
                SpeakerEntry candidate = source[i];
                if (candidate != null
                    && string.Equals(candidate.SpeakerId, speakerId, StringComparison.Ordinal))
                {
                    return candidate;
                }
            }

            return null;
        }

        private static SpeakerEntry[] CloneSpeakers(SpeakerEntry[] source)
        {
            if (source == null || source.Length == 0)
            {
                return Array.Empty<SpeakerEntry>();
            }

            var clone = new SpeakerEntry[source.Length];
            for (int i = 0; i < source.Length; i++)
            {
                clone[i] = source[i]?.DeepCopy();
            }

            return clone;
        }

        private void OnValidate()
        {
            speakers ??= Array.Empty<SpeakerEntry>();
        }
    }
}
