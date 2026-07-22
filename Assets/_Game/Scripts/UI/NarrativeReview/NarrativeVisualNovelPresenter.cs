using System;
using DimensionBrawl.Presentation.Narrative;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DimensionBrawl.UI.NarrativeReview
{
    public readonly struct NarrativePortraitSlotSnapshot
    {
        public NarrativePortraitSlotSnapshot(
            NarrativePortraitSlot slot,
            string speakerId,
            string expressionId,
            Sprite portraitSprite,
            bool isFocused)
        {
            Slot = slot;
            SpeakerId = speakerId ?? string.Empty;
            ExpressionId = expressionId ?? string.Empty;
            PortraitSprite = portraitSprite;
            IsFocused = isFocused;
        }

        public NarrativePortraitSlot Slot { get; }
        public string SpeakerId { get; }
        public string ExpressionId { get; }
        public Sprite PortraitSprite { get; }
        public bool IsFocused { get; }
        public bool IsOccupied => !string.IsNullOrWhiteSpace(SpeakerId);
        public bool HasPortraitSprite => PortraitSprite != null;
    }

    public readonly struct NarrativeVisualNovelPresentationSnapshot
    {
        public NarrativeVisualNovelPresentationSnapshot(
            string currentLineId,
            bool lineFullyRevealed,
            bool autoAdvanceEnabled,
            bool choicesVisible,
            bool logVisible,
            bool skipConfirmationVisible,
            NarrativePortraitSlotSnapshot left,
            NarrativePortraitSlotSnapshot center,
            NarrativePortraitSlotSnapshot right)
        {
            CurrentLineId = currentLineId ?? string.Empty;
            LineFullyRevealed = lineFullyRevealed;
            AutoAdvanceEnabled = autoAdvanceEnabled;
            ChoicesVisible = choicesVisible;
            LogVisible = logVisible;
            SkipConfirmationVisible = skipConfirmationVisible;
            Left = left;
            Center = center;
            Right = right;
        }

        public string CurrentLineId { get; }
        public bool LineFullyRevealed { get; }
        public bool AutoAdvanceEnabled { get; }
        public bool ChoicesVisible { get; }
        public bool LogVisible { get; }
        public bool SkipConfirmationVisible { get; }
        public NarrativePortraitSlotSnapshot Left { get; }
        public NarrativePortraitSlotSnapshot Center { get; }
        public NarrativePortraitSlotSnapshot Right { get; }

        public NarrativePortraitSlotSnapshot GetSlot(NarrativePortraitSlot slot)
        {
            return slot switch
            {
                NarrativePortraitSlot.Left => Left,
                NarrativePortraitSlot.Center => Center,
                NarrativePortraitSlot.Right => Right,
                _ => default
            };
        }
    }

    /// <summary>
    /// Review-safe visual-novel presentation state and persistent three-slot portrait stage.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NarrativeVisualNovelPresenter : MonoBehaviour
    {
        private const float DefaultInactivePortraitAlpha = 0.48f;

        [Header("Presentation Data")]
        [SerializeField] private NarrativeSpeakerPresentationCatalog speakerCatalog;

        [Header("Persistent Portrait Stage")]
        [SerializeField] private CanvasGroup leftPortraitGroup;
        [SerializeField] private CanvasGroup centerPortraitGroup;
        [SerializeField] private CanvasGroup rightPortraitGroup;
        [SerializeField] private Image leftPortraitImage;
        [SerializeField] private Image centerPortraitImage;
        [SerializeField] private Image rightPortraitImage;
        [SerializeField] private TMP_Text leftExpressionText;
        [SerializeField] private TMP_Text centerExpressionText;
        [SerializeField] private TMP_Text rightExpressionText;
        [SerializeField, Range(0.1f, 0.9f)]
        private float inactivePortraitAlpha = DefaultInactivePortraitAlpha;

        private readonly PortraitStageState portraitStage = new PortraitStageState();
        private string currentLineId = string.Empty;
        private bool lineFullyRevealed;
        private bool autoAdvanceEnabled;
        private bool choicesVisible;
        private bool logVisible;
        private bool skipConfirmationVisible;

        public NarrativeSpeakerPresentationCatalog SpeakerCatalog => speakerCatalog;
        public string LastPortraitCommandStatus { get; private set; } = "not-presented";
        public NarrativeVisualNovelPresentationSnapshot Snapshot => new(
            currentLineId,
            lineFullyRevealed,
            autoAdvanceEnabled,
            choicesVisible,
            logVisible,
            skipConfirmationVisible,
            portraitStage.Left,
            portraitStage.Center,
            portraitStage.Right);

        public void Configure(
            NarrativeSpeakerPresentationCatalog newSpeakerCatalog,
            CanvasGroup newLeftPortraitGroup,
            CanvasGroup newCenterPortraitGroup,
            CanvasGroup newRightPortraitGroup,
            Image newLeftPortraitImage,
            Image newCenterPortraitImage,
            Image newRightPortraitImage,
            TMP_Text newLeftExpressionText,
            TMP_Text newCenterExpressionText,
            TMP_Text newRightExpressionText)
        {
            speakerCatalog = newSpeakerCatalog;
            leftPortraitGroup = newLeftPortraitGroup;
            centerPortraitGroup = newCenterPortraitGroup;
            rightPortraitGroup = newRightPortraitGroup;
            leftPortraitImage = newLeftPortraitImage;
            centerPortraitImage = newCenterPortraitImage;
            rightPortraitImage = newRightPortraitImage;
            leftExpressionText = newLeftExpressionText;
            centerExpressionText = newCenterExpressionText;
            rightExpressionText = newRightExpressionText;
            RefreshPortraitStage();
        }

        public void ResetPresentation()
        {
            portraitStage.ClearStage();
            currentLineId = string.Empty;
            lineFullyRevealed = false;
            autoAdvanceEnabled = false;
            choicesVisible = false;
            logVisible = false;
            skipConfirmationVisible = false;
            LastPortraitCommandStatus = "reset";
            RefreshPortraitStage();
        }

        public void PresentLine(NarrativeSequenceProfile.LineEntry line)
        {
            currentLineId = line?.LineId ?? string.Empty;
            lineFullyRevealed = false;
            portraitStage.ApplyLine(line, speakerCatalog);
            LastPortraitCommandStatus = BuildPortraitCommandStatus(line);
            RefreshPortraitStage();
        }

        public void PresentChoiceResponse(
            NarrativeSequenceProfile.LineEntry sourceLine,
            string responseLineId)
        {
            currentLineId = responseLineId ?? string.Empty;
            lineFullyRevealed = false;
            portraitStage.ApplyLine(sourceLine, speakerCatalog);
            LastPortraitCommandStatus = BuildPortraitCommandStatus(sourceLine);
            RefreshPortraitStage();
        }

        public void SetLineFullyRevealed(bool value)
        {
            lineFullyRevealed = value;
        }

        public void SetAutoAdvanceEnabled(bool value)
        {
            autoAdvanceEnabled = value;
        }

        public void SetChoicesVisible(bool value)
        {
            choicesVisible = value;
        }

        public void SetLogVisible(bool value)
        {
            logVisible = value;
        }

        public void SetSkipConfirmationVisible(bool value)
        {
            skipConfirmationVisible = value;
        }

        public bool TryResolveDisplayName(string speakerId, out string displayName)
        {
            displayName = string.Empty;
            return speakerCatalog != null
                && speakerCatalog.TryResolveDisplayName(speakerId, out displayName);
        }

        private void RefreshPortraitStage()
        {
            ApplySlot(
                leftPortraitGroup,
                leftPortraitImage,
                leftExpressionText,
                portraitStage.Left);
            ApplySlot(
                centerPortraitGroup,
                centerPortraitImage,
                centerExpressionText,
                portraitStage.Center);
            ApplySlot(
                rightPortraitGroup,
                rightPortraitImage,
                rightExpressionText,
                portraitStage.Right);
        }

        private string BuildPortraitCommandStatus(NarrativeSequenceProfile.LineEntry line)
        {
            return $"line:{line?.LineId ?? "none"};"
                + $"left:{portraitStage.Left.SpeakerId}/{portraitStage.Left.ExpressionId}/"
                + $"{portraitStage.Left.IsOccupied}/{portraitStage.Left.IsFocused};"
                + $"center:{portraitStage.Center.SpeakerId}/{portraitStage.Center.ExpressionId}/"
                + $"{portraitStage.Center.IsOccupied}/{portraitStage.Center.IsFocused};"
                + $"right:{portraitStage.Right.SpeakerId}/{portraitStage.Right.ExpressionId}/"
                + $"{portraitStage.Right.IsOccupied}/{portraitStage.Right.IsFocused}";
        }

        private void ApplySlot(
            CanvasGroup group,
            Image image,
            TMP_Text expressionText,
            NarrativePortraitSlotSnapshot state)
        {
            if (group != null)
            {
                group.alpha = state.IsOccupied
                    ? state.IsFocused ? 1f : inactivePortraitAlpha
                    : 0f;
                group.interactable = false;
                group.blocksRaycasts = false;
            }

            if (image != null)
            {
                image.sprite = state.HasPortraitSprite ? state.PortraitSprite : null;
                image.enabled = state.HasPortraitSprite;
                image.preserveAspect = true;
            }

            if (expressionText != null)
            {
                expressionText.text = state.IsOccupied
                    ? state.ExpressionId.ToUpperInvariant()
                    : string.Empty;
            }
        }

        private sealed class PortraitStageState
        {
            public NarrativePortraitSlotSnapshot Left { get; private set; }
            public NarrativePortraitSlotSnapshot Center { get; private set; }
            public NarrativePortraitSlotSnapshot Right { get; private set; }

            public void ApplyLine(
                NarrativeSequenceProfile.LineEntry line,
                NarrativeSpeakerPresentationCatalog catalog)
            {
                if (line == null)
                {
                    ClearFocus();
                    return;
                }

                NarrativeSequenceProfile.PortraitCommandEntry[] commands =
                    line.PortraitCommands;
                if (commands.Length == 0)
                {
                    ApplyCompatibilityLine(line, catalog);
                    return;
                }

                for (int i = 0; i < commands.Length; i++)
                {
                    ApplyCommand(commands[i], catalog);
                }
            }

            public void ClearStage()
            {
                Left = default;
                Center = default;
                Right = default;
            }

            private void ApplyCompatibilityLine(
                NarrativeSequenceProfile.LineEntry line,
                NarrativeSpeakerPresentationCatalog catalog)
            {
                bool catalogResolved = catalog != null
                    && catalog.TryResolve(
                        line.SpeakerId,
                        line.ExpressionId,
                        out NarrativeSpeakerPresentation unresolved);
                if (line.PortraitSprite == null && !catalogResolved)
                {
                    ClearFocus();
                    return;
                }

                Present(
                    line.SpeakerId,
                    line.PortraitSlot,
                    line.ExpressionId,
                    line.PortraitSprite,
                    catalog);
            }

            private void ApplyCommand(
                NarrativeSequenceProfile.PortraitCommandEntry command,
                NarrativeSpeakerPresentationCatalog catalog)
            {
                if (command == null)
                {
                    return;
                }

                switch (command.CommandType)
                {
                    case NarrativePortraitCommandType.Present:
                        Present(
                            command.SpeakerId,
                            command.PortraitSlot,
                            command.ExpressionId,
                            command.PortraitSpriteOverride,
                            catalog);
                        break;
                    case NarrativePortraitCommandType.HideSpeaker:
                        HideSpeaker(command.SpeakerId);
                        break;
                    case NarrativePortraitCommandType.ClearFocus:
                        ClearFocus();
                        break;
                    case NarrativePortraitCommandType.ClearStage:
                        ClearStage();
                        break;
                    default:
                        break;
                }
            }

            private void Present(
                string speakerId,
                NarrativePortraitSlot requestedSlot,
                string requestedExpressionId,
                Sprite portraitOverride,
                NarrativeSpeakerPresentationCatalog catalog)
            {
                NarrativePortraitSlot slot = requestedSlot;
                string expressionId = requestedExpressionId ?? string.Empty;
                Sprite portrait = portraitOverride;
                if (catalog != null
                    && catalog.TryResolve(
                        speakerId,
                        requestedExpressionId,
                        out NarrativeSpeakerPresentation resolved))
                {
                    if (slot == NarrativePortraitSlot.None)
                    {
                        slot = resolved.DefaultPortraitSlot;
                    }

                    if (string.IsNullOrWhiteSpace(expressionId)
                        || portraitOverride == null)
                    {
                        expressionId = resolved.ExpressionId;
                    }

                    if (portrait == null)
                    {
                        portrait = resolved.PortraitSprite;
                    }
                }

                if (string.IsNullOrWhiteSpace(speakerId)
                    || slot == NarrativePortraitSlot.None
                    || !Enum.IsDefined(typeof(NarrativePortraitSlot), slot))
                {
                    ClearFocus();
                    return;
                }

                HideSpeaker(speakerId);
                var presented = new NarrativePortraitSlotSnapshot(
                    slot,
                    speakerId,
                    expressionId,
                    portrait,
                    isFocused: true);
                SetSlot(slot, presented);
                FocusOnly(slot);
            }

            private void HideSpeaker(string speakerId)
            {
                if (string.IsNullOrWhiteSpace(speakerId))
                {
                    return;
                }

                if (string.Equals(Left.SpeakerId, speakerId, StringComparison.Ordinal))
                {
                    Left = default;
                }

                if (string.Equals(Center.SpeakerId, speakerId, StringComparison.Ordinal))
                {
                    Center = default;
                }

                if (string.Equals(Right.SpeakerId, speakerId, StringComparison.Ordinal))
                {
                    Right = default;
                }
            }

            private void ClearFocus()
            {
                Left = WithFocus(Left, false);
                Center = WithFocus(Center, false);
                Right = WithFocus(Right, false);
            }

            private void FocusOnly(NarrativePortraitSlot slot)
            {
                Left = WithFocus(Left, slot == NarrativePortraitSlot.Left);
                Center = WithFocus(Center, slot == NarrativePortraitSlot.Center);
                Right = WithFocus(Right, slot == NarrativePortraitSlot.Right);
            }

            private void SetSlot(
                NarrativePortraitSlot slot,
                NarrativePortraitSlotSnapshot state)
            {
                switch (slot)
                {
                    case NarrativePortraitSlot.Left:
                        Left = state;
                        break;
                    case NarrativePortraitSlot.Center:
                        Center = state;
                        break;
                    case NarrativePortraitSlot.Right:
                        Right = state;
                        break;
                    default:
                        break;
                }
            }

            private static NarrativePortraitSlotSnapshot WithFocus(
                NarrativePortraitSlotSnapshot state,
                bool focused)
            {
                return state.IsOccupied
                    ? new NarrativePortraitSlotSnapshot(
                        state.Slot,
                        state.SpeakerId,
                        state.ExpressionId,
                        state.PortraitSprite,
                        focused)
                    : default;
            }
        }
    }
}
