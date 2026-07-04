using UnityEngine;

namespace DimensionBrawl.Presentation
{
    [DisallowMultipleComponent]
    public sealed class OlympusTutorialOverlayPresenter : MonoBehaviour
    {
        public enum FocusKind
        {
            None,
            MeleeAttack,
            MoveStick,
            SwapMode,
            RangedAttack,
            Dodge,
            Route
        }

        public enum GuideState
        {
            Focus,
            Ready,
            Confirmed
        }

        private const float ReferenceWidth = 1920f;
        private const float ReferenceHeight = 1080f;

        [SerializeField] private bool visible;
        [SerializeField] private string speaker = "OPERATOR";
        [SerializeField] private string dialogue = string.Empty;
        [SerializeField] private string inputLabel = string.Empty;
        [SerializeField] private FocusKind focusKind;
        [SerializeField] private GuideState guideState;
        [SerializeField] private Vector2 focusAnchor = new Vector2(0.78f, 0.70f);
        [SerializeField, Min(0.01f)] private float transitionSeconds = 0.36f;
        [SerializeField, Min(1f)] private float dialogueCharactersPerSecond = 22f;
        [Header("Communicator Panel")]
        [SerializeField] private bool useCommunicatorPanel = true;
        [SerializeField, Range(0f, 1f)] private float communicatorScanlineAlpha = 0.18f;
        [SerializeField, Range(0f, 1f)] private float communicatorGlitchAlpha = 0.20f;
        [SerializeField, Min(0f)] private float communicatorSweepSpeed = 1.18f;
        [SerializeField, Min(0f)] private float signalPulseSpeed = 6.2f;
        [SerializeField, Min(0f)] private float warningBootSeconds = 0.58f;
        [SerializeField, Range(0f, 1f)] private float warningBootMaxAlpha = 0.82f;
        [SerializeField] private string warningBootTitle = "- WARNING -";
        [SerializeField] private string warningBootSubtitle = "CELESTIAL SYSTEM LINK";
        [Header("Communicator Audio")]
        [SerializeField] private AudioSource communicatorAudioSource;
        [SerializeField] private AudioClip communicatorOpenSfx;
        [SerializeField, Range(0f, 1f)] private float communicatorOpenSfxVolume = 0.82f;
        [Header("Replaceable Tutorial Resources")]
        [SerializeField] private bool showReplaceablePlaceholders = true;
        [SerializeField] private Texture2D operatorPortrait;
        [SerializeField] private Texture2D inoriPortrait;
        [SerializeField] private Texture2D dialogueFrameTexture;
        [SerializeField] private Texture2D focusMarkerTexture;
        [SerializeField] private Texture2D meleeAttackIcon;
        [SerializeField] private Texture2D moveStickIcon;
        [SerializeField] private Texture2D swapModeIcon;
        [SerializeField] private Texture2D rangedAttackIcon;
        [SerializeField] private Texture2D dodgeIcon;
        [SerializeField] private Texture2D routeIcon;

        private GUIStyle speakerStyle;
        private GUIStyle dialogueStyle;
        private GUIStyle inputStyle;
        private GUIStyle hintStyle;
        private GUIStyle panelStyle;
        private GUIStyle markerStyle;
        private GUIStyle markerFillStyle;
        private GUIStyle portraitStyle;
        private GUIStyle portraitLabelStyle;
        private GUIStyle chipStyle;
        private GUIStyle stateStyle;
        private GUIStyle progressStyle;
        private GUIStyle glyphStyle;
        private GUIStyle warningTitleStyle;
        private GUIStyle warningSubtitleStyle;
        private Texture2D whiteTexture;
        private string outgoingSpeaker = "OPERATOR";
        private string outgoingDialogue = string.Empty;
        private string outgoingInputLabel = string.Empty;
        private FocusKind outgoingFocusKind;
        private GuideState outgoingGuideState;
        private int outgoingProgressStepIndex;
        private int outgoingProgressStepCount;
        private string outgoingPhaseLabel = string.Empty;
        private Vector2 outgoingFocusAnchor = new Vector2(0.78f, 0.70f);
        private Vector2 transitionStartFocusAnchor = new Vector2(0.78f, 0.70f);
        private float transitionTimer = 1f;
        private float dialogueRevealTimer;
        private float warningBootTimer;
        private bool hasOutgoingCue;
        private bool hasPlayedOpenSfx;
        private int progressStepIndex;
        private int progressStepCount;
        private string phaseLabel = string.Empty;

        public bool Visible => visible;
        public FocusKind CurrentFocusKind => focusKind;
        public GuideState CurrentGuideState => guideState;
        public Vector2 CurrentFocusAnchor => ResolveAnimatedFocusAnchor();
        public Vector2 CurrentFocusCenterGuiPoint => ResolveFocusCenterGuiPoint(ResolveAnimatedFocusAnchor());
        public Rect CurrentFocusMarkerGuiRect => ResolveFocusMarkerRect(ResolveScale());
        public Rect CurrentDialoguePanelGuiRect => ResolveDialoguePanelRect(ResolveScale());

        public void Show(
            string newSpeaker,
            string newDialogue,
            string newInputLabel,
            FocusKind newFocusKind,
            Vector2 newFocusAnchor)
        {
            string resolvedSpeaker = string.IsNullOrWhiteSpace(newSpeaker) ? "OPERATOR" : newSpeaker;
            string resolvedDialogue = newDialogue ?? string.Empty;
            string resolvedInputLabel = newInputLabel ?? string.Empty;
            Vector2 resolvedFocusAnchor = new Vector2(Mathf.Clamp01(newFocusAnchor.x), Mathf.Clamp01(newFocusAnchor.y));

            if (IsSameGuide(resolvedSpeaker, resolvedDialogue, resolvedInputLabel, newFocusKind))
            {
                focusAnchor = resolvedFocusAnchor;
                return;
            }

            if (visible)
            {
                CaptureOutgoingCue();
                transitionStartFocusAnchor = ResolveAnimatedFocusAnchor();
                hasOutgoingCue = true;
            }
            else
            {
                transitionStartFocusAnchor = resolvedFocusAnchor;
                hasOutgoingCue = false;
                warningBootTimer = 0f;
                hasPlayedOpenSfx = false;
            }

            speaker = resolvedSpeaker;
            dialogue = resolvedDialogue;
            inputLabel = resolvedInputLabel;
            focusKind = newFocusKind;
            guideState = GuideState.Focus;
            focusAnchor = resolvedFocusAnchor;
            transitionTimer = 0f;
            dialogueRevealTimer = 0f;
            visible = true;
            TryPlayCommunicatorOpenSfx();
        }

        public void Hide()
        {
            visible = false;
            dialogue = string.Empty;
            inputLabel = string.Empty;
            focusKind = FocusKind.None;
            guideState = GuideState.Focus;
            progressStepIndex = 0;
            progressStepCount = 0;
            phaseLabel = string.Empty;
            hasOutgoingCue = false;
            hasPlayedOpenSfx = false;
            transitionTimer = transitionSeconds;
            dialogueRevealTimer = 0f;
            warningBootTimer = 0f;
        }

        public void ConfigureCommunicatorAudio(
            AudioSource audioSource,
            AudioClip openSfx,
            float volume)
        {
            communicatorAudioSource = audioSource;
            communicatorOpenSfx = openSfx;
            communicatorOpenSfxVolume = Mathf.Clamp01(volume);
        }

        public void SetGuideProgress(int stepIndex, int stepCount, string newPhaseLabel)
        {
            progressStepIndex = Mathf.Max(0, stepIndex);
            progressStepCount = Mathf.Max(0, stepCount);
            phaseLabel = newPhaseLabel ?? string.Empty;
        }

        public void SetGuideState(GuideState newGuideState)
        {
            guideState = newGuideState;
        }

        private void Update()
        {
            if (!visible)
            {
                return;
            }

            transitionTimer = Mathf.Min(transitionTimer + Time.unscaledDeltaTime, transitionSeconds);
            dialogueRevealTimer += Time.unscaledDeltaTime;
            warningBootTimer += Time.unscaledDeltaTime;
            if (hasOutgoingCue && transitionTimer >= transitionSeconds)
            {
                hasOutgoingCue = false;
            }
        }

        private void OnGUI()
        {
            if (!visible)
            {
                return;
            }

            EnsureStyles();
            float scale = ResolveScale();
            DrawWarningBootOverlay(scale);
            DrawDialoguePanel(scale);
            DrawFocusMarker(scale);
        }

        private void DrawDialoguePanel(float scale)
        {
            Rect basePanelRect = ResolveDialoguePanelRect(scale);
            float incomingAlpha = ResolveIncomingAlpha();
            float bootRevealAlpha = ResolveBootRevealAlpha();
            float outgoingAlpha = hasOutgoingCue ? 1f - incomingAlpha : 0f;
            float panelAlpha = Mathf.Max(incomingAlpha, outgoingAlpha) * bootRevealAlpha;
            Rect panelRect = ResolveCommunicatorPanelRect(basePanelRect, scale, incomingAlpha);

            if (useCommunicatorPanel)
            {
                DrawCommunicatorPanel(panelRect, scale, panelAlpha);
            }
            else
            {
                Color previousColor = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, panelAlpha);
                if (dialogueFrameTexture != null)
                {
                    GUI.DrawTexture(panelRect, dialogueFrameTexture, ScaleMode.StretchToFill, true);
                }
                else
                {
                    GUI.Box(panelRect, GUIContent.none, panelStyle);
                }

                GUI.color = previousColor;
            }

            if (hasOutgoingCue)
            {
                DrawDialogueContent(
                    panelRect,
                    scale,
                    outgoingSpeaker,
                    outgoingDialogue,
                    outgoingInputLabel,
                    outgoingFocusKind,
                    outgoingGuideState,
                    outgoingProgressStepIndex,
                    outgoingProgressStepCount,
                    outgoingPhaseLabel,
                    outgoingAlpha,
                    outgoingDialogue.Length,
                    ResolveOutgoingContentOffset(scale, incomingAlpha));
            }

            DrawDialogueContent(
                panelRect,
                scale,
                speaker,
                dialogue,
                inputLabel,
                focusKind,
                guideState,
                progressStepIndex,
                progressStepCount,
                phaseLabel,
                incomingAlpha * bootRevealAlpha,
                ResolveVisibleDialogueCharacterCount(),
                ResolveIncomingContentOffset(scale, incomingAlpha));
        }

        private void DrawDialogueContent(
            Rect panelRect,
            float scale,
            string contentSpeaker,
            string contentDialogue,
            string contentInputLabel,
            FocusKind contentFocusKind,
            GuideState contentGuideState,
            int contentProgressStepIndex,
            int contentProgressStepCount,
            string contentPhaseLabel,
            float alpha,
            int visibleDialogueCharacters,
            Vector2 contentOffset)
        {
            if (alpha <= 0.001f)
            {
                return;
            }

            panelRect = OffsetRect(panelRect, contentOffset);
            Color previousColor = GUI.color;
            GUI.color = ResolveContentGuiColor(contentGuideState, alpha);

            bool usePortraitColumn =
                showReplaceablePlaceholders || ResolvePortraitTexture(contentSpeaker) != null;
            float contentInset = 26f * scale;
            float portraitSize = usePortraitColumn ? 122f * scale : 0f;
            float contentX = panelRect.x + contentInset;
            float contentWidth = panelRect.width - contentInset * 2f;
            if (usePortraitColumn)
            {
                Rect portraitRect = new Rect(
                    panelRect.x + contentInset,
                    panelRect.y + 24f * scale,
                    portraitSize,
                    portraitSize);
                DrawPortrait(portraitRect, contentSpeaker, alpha);
                contentX = portraitRect.xMax + 20f * scale;
                contentWidth = panelRect.xMax - contentInset - contentX;
            }

            Rect actionRect = new Rect(
                panelRect.xMax - 198f * scale,
                panelRect.y + 18f * scale,
                166f * scale,
                42f * scale);
            DrawActionStatus(actionRect, contentFocusKind, contentGuideState, contentPhaseLabel, alpha);

            Rect speakerRect = new Rect(
                contentX,
                panelRect.y + 20f * scale,
                Mathf.Max(120f * scale, actionRect.x - contentX - 12f * scale),
                34f * scale);
            GUI.Label(speakerRect, contentSpeaker, speakerStyle);

            Rect dialogueRect = new Rect(
                contentX,
                panelRect.y + 64f * scale,
                contentWidth,
                96f * scale);
            string visibleDialogue = TruncateDialogue(contentDialogue, visibleDialogueCharacters);
            if (!string.IsNullOrEmpty(contentDialogue)
                && visibleDialogueCharacters < contentDialogue.Length
                && Mathf.PingPong(Time.unscaledTime * 5f, 1f) > 0.48f)
            {
                visibleDialogue += "_";
            }

            GUI.Label(dialogueRect, visibleDialogue, dialogueStyle);

            DrawProgressReadout(
                panelRect,
                scale,
                contentProgressStepIndex,
                contentProgressStepCount,
                alpha);

            if (string.IsNullOrWhiteSpace(contentInputLabel))
            {
                GUI.color = previousColor;
                return;
            }

            Rect inputRect = new Rect(
                panelRect.x + panelRect.width - 190f * scale,
                panelRect.y + panelRect.height - 52f * scale,
                154f * scale,
                34f * scale);
            GUI.Box(inputRect, contentInputLabel, inputStyle);
            GUI.color = previousColor;
        }

        private void DrawFocusMarker(float scale)
        {
            float incomingAlpha = ResolveIncomingAlpha();
            float bootRevealAlpha = ResolveBootRevealAlpha();
            if (hasOutgoingCue)
            {
                DrawFocusMarker(
                    scale,
                    outgoingFocusKind,
                    outgoingGuideState,
                    outgoingFocusAnchor,
                    outgoingInputLabel,
                    (1f - incomingAlpha) * bootRevealAlpha);
            }

            DrawFocusMarker(
                scale,
                focusKind,
                guideState,
                ResolveAnimatedFocusAnchor(),
                inputLabel,
                incomingAlpha * bootRevealAlpha);
        }

        private void DrawFocusMarker(
            float scale,
            FocusKind markerFocusKind,
            GuideState markerGuideState,
            Vector2 markerFocusAnchor,
            string markerInputLabel,
            float alpha)
        {
            if (markerFocusKind == FocusKind.None || alpha <= 0.001f)
            {
                return;
            }

            Rect markerRect = ResolveFocusMarkerRect(scale, markerFocusAnchor);
            Rect fillRect = new Rect(
                markerRect.x + 8f * scale,
                markerRect.y + 8f * scale,
                markerRect.width - 16f * scale,
                markerRect.height - 16f * scale);

            Color previousColor = GUI.color;
            GUI.color = ResolveMarkerFillColor(markerGuideState, alpha);
            GUI.Box(fillRect, GUIContent.none, markerFillStyle);
            GUI.color = previousColor;
            DrawBorder(markerRect, 4f * scale, ResolveFocusColor(markerFocusKind, markerGuideState), alpha);
            DrawFocusGlyph(fillRect, markerFocusKind, alpha);

            if (!string.IsNullOrWhiteSpace(markerInputLabel))
            {
                Rect hintRect = new Rect(
                    markerRect.x - 28f * scale,
                    markerRect.y - 34f * scale,
                    markerRect.width + 56f * scale,
                    28f * scale);
                previousColor = GUI.color;
                GUI.color = ResolveContentGuiColor(markerGuideState, alpha);
                GUI.Label(hintRect, markerInputLabel, hintStyle);
                GUI.color = previousColor;
            }
        }

        private Rect ResolveDialoguePanelRect(float scale)
        {
            float width = Mathf.Min(Screen.width - 56f * scale, 940f * scale);
            float height = 214f * scale;
            return new Rect(
                38f * scale,
                Mathf.Max(24f * scale, Screen.height * 0.5f - height * 0.5f),
                width,
                height);
        }

        private float ResolveBootRevealAlpha()
        {
            if (!useCommunicatorPanel || hasOutgoingCue || warningBootSeconds <= 0.001f)
            {
                return 1f;
            }

            float start = warningBootSeconds * 0.46f;
            float normalized = Mathf.InverseLerp(start, warningBootSeconds, warningBootTimer);
            return Mathf.SmoothStep(0f, 1f, normalized);
        }

        private void DrawWarningBootOverlay(float scale)
        {
            if (!useCommunicatorPanel
                || hasOutgoingCue
                || warningBootSeconds <= 0.001f
                || warningBootTimer >= warningBootSeconds)
            {
                return;
            }

            float normalized = Mathf.Clamp01(warningBootTimer / warningBootSeconds);
            float open = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(normalized / 0.45f));
            float exit = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.72f, 1f, normalized));
            float flicker = 0.68f + 0.32f * Mathf.Round(Mathf.PingPong(Time.unscaledTime * 18f, 1f));
            float alpha = warningBootMaxAlpha * open * exit * flicker;
            if (alpha <= 0.001f)
            {
                return;
            }

            float width = Mathf.Lerp(120f * scale, Screen.width * 0.74f, open);
            float height = 116f * scale;
            Rect bandRect = new Rect(
                Screen.width * 0.5f - width * 0.5f,
                Screen.height * 0.24f - height * 0.5f,
                width,
                height);
            Color lineColor = new Color(0.9f, 0.98f, 1f, alpha);
            Color panelColor = new Color(0.025f, 0.022f, 0.018f, 0.78f * alpha);

            DrawSolidRect(bandRect, panelColor);
            DrawWarningBootDither(bandRect, scale, alpha);
            DrawSolidRect(new Rect(bandRect.x, bandRect.y + 18f * scale, bandRect.width, 2f * scale), lineColor);
            DrawSolidRect(new Rect(bandRect.x, bandRect.yMax - 18f * scale, bandRect.width, 2f * scale), lineColor);

            float innerLineWidth = bandRect.width * 0.62f;
            float innerLineX = bandRect.center.x - innerLineWidth * 0.5f;
            DrawSolidRect(new Rect(innerLineX, bandRect.center.y - 4f * scale, innerLineWidth, 2f * scale), lineColor);
            DrawSolidRect(new Rect(innerLineX - 4f * scale, bandRect.center.y - 6f * scale, 4f * scale, 4f * scale), lineColor);
            DrawSolidRect(new Rect(innerLineX + innerLineWidth, bandRect.center.y - 6f * scale, 4f * scale, 4f * scale), lineColor);

            Color previous = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, alpha);
            Rect titleRect = new Rect(bandRect.x, bandRect.y + 28f * scale, bandRect.width, 38f * scale);
            GUI.Label(titleRect, warningBootTitle, warningTitleStyle);
            Rect subtitleRect = new Rect(bandRect.x, bandRect.y + 70f * scale, bandRect.width, 26f * scale);
            GUI.Label(subtitleRect, warningBootSubtitle, warningSubtitleStyle);
            GUI.color = previous;
        }

        private void DrawWarningBootDither(Rect bandRect, float scale, float alpha)
        {
            float dot = Mathf.Max(1f, 2f * scale);
            float gap = 12f * scale;
            float topY = bandRect.y + 8f * scale;
            float bottomY = bandRect.yMax - 10f * scale;
            int count = Mathf.FloorToInt(bandRect.width / gap);
            for (int i = 0; i < count; i++)
            {
                float seed = Mathf.Repeat(Mathf.Sin((i + 1) * 15.17f) * 1743.31f, 1f);
                if (seed < 0.28f)
                {
                    continue;
                }

                float x = bandRect.x + i * gap;
                Color color = new Color(0.88f, 0.95f, 1f, (0.16f + seed * 0.20f) * alpha);
                DrawSolidRect(new Rect(x, topY + seed * 5f * scale, dot, dot), color);
                DrawSolidRect(new Rect(x + gap * 0.46f, bottomY - seed * 5f * scale, dot, dot), color);
            }
        }

        private Rect ResolveCommunicatorPanelRect(Rect basePanelRect, float scale, float incomingAlpha)
        {
            if (!useCommunicatorPanel)
            {
                return basePanelRect;
            }

            float slideDistance = 48f * scale;
            return new Rect(
                basePanelRect.x - (1f - incomingAlpha) * slideDistance,
                basePanelRect.y,
                basePanelRect.width,
                basePanelRect.height);
        }

        private Vector2 ResolveIncomingContentOffset(float scale, float incomingAlpha)
        {
            if (!useCommunicatorPanel)
            {
                return Vector2.zero;
            }

            return new Vector2((1f - incomingAlpha) * 34f * scale, 0f);
        }

        private Vector2 ResolveOutgoingContentOffset(float scale, float incomingAlpha)
        {
            if (!useCommunicatorPanel)
            {
                return Vector2.zero;
            }

            return new Vector2(-incomingAlpha * 24f * scale, 0f);
        }

        private void DrawCommunicatorPanel(Rect panelRect, float scale, float alpha)
        {
            float resolvedAlpha = Mathf.Clamp01(alpha);
            if (resolvedAlpha <= 0.001f)
            {
                return;
            }

            Color focusColor = ResolveFocusColor(focusKind, guideState);
            if (dialogueFrameTexture != null)
            {
                Color previous = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, resolvedAlpha);
                GUI.DrawTexture(panelRect, dialogueFrameTexture, ScaleMode.StretchToFill, true);
                GUI.color = previous;
            }
            else
            {
                DrawSolidRect(panelRect, new Color(0.006f, 0.014f, 0.021f, 0.88f * resolvedAlpha));
                Rect innerRect = new Rect(
                    panelRect.x + 5f * scale,
                    panelRect.y + 5f * scale,
                    panelRect.width - 10f * scale,
                    panelRect.height - 10f * scale);
                DrawSolidRect(innerRect, new Color(0.018f, 0.034f, 0.046f, 0.68f * resolvedAlpha));
            }

            DrawCommunicatorScanlines(panelRect, scale, resolvedAlpha);
            DrawCommunicatorSweep(panelRect, scale, focusColor, resolvedAlpha);
            DrawCommunicatorSignalBars(panelRect, scale, focusColor, resolvedAlpha);
            DrawCommunicatorGlitch(panelRect, scale, focusColor, resolvedAlpha);
            DrawBorder(panelRect, Mathf.Max(1f, 2f * scale), focusColor, 0.62f * resolvedAlpha);
            DrawCommunicatorCorners(panelRect, scale, focusColor, resolvedAlpha);
        }

        private void DrawCommunicatorScanlines(Rect panelRect, float scale, float alpha)
        {
            if (communicatorScanlineAlpha <= 0f)
            {
                return;
            }

            float gap = Mathf.Max(7f, 9f * scale);
            float offset = Mathf.Repeat(Time.unscaledTime * 22f, gap);
            float lineHeight = Mathf.Max(1f, scale);
            for (float y = panelRect.y + offset; y < panelRect.yMax; y += gap)
            {
                DrawSolidRect(
                    new Rect(panelRect.x + 6f * scale, y, panelRect.width - 12f * scale, lineHeight),
                    new Color(0.55f, 0.92f, 1f, communicatorScanlineAlpha * 0.36f * alpha));
            }
        }

        private void DrawCommunicatorSweep(Rect panelRect, float scale, Color focusColor, float alpha)
        {
            if (communicatorSweepSpeed <= 0f)
            {
                return;
            }

            float sweep = Mathf.Repeat(Time.unscaledTime * communicatorSweepSpeed, 1f);
            float x = Mathf.Lerp(panelRect.x + 10f * scale, panelRect.xMax - 10f * scale, sweep);
            DrawSolidRect(
                new Rect(x - 18f * scale, panelRect.y + 7f * scale, 36f * scale, panelRect.height - 14f * scale),
                new Color(focusColor.r, focusColor.g, focusColor.b, 0.035f * alpha));
            DrawSolidRect(
                new Rect(x, panelRect.y + 7f * scale, Mathf.Max(1f, 2f * scale), panelRect.height - 14f * scale),
                new Color(focusColor.r, focusColor.g, focusColor.b, 0.16f * alpha));
        }

        private void DrawCommunicatorSignalBars(Rect panelRect, float scale, Color focusColor, float alpha)
        {
            float barWidth = 7f * scale;
            float gap = 5f * scale;
            float maxHeight = 35f * scale;
            float startX = panelRect.x + 18f * scale;
            float baseY = panelRect.y + 19f * scale + maxHeight;

            for (int i = 0; i < 5; i++)
            {
                float pulse = 0.35f + 0.65f * Mathf.Abs(Mathf.Sin(Time.unscaledTime * signalPulseSpeed + i * 0.84f));
                if (guideState == GuideState.Confirmed)
                {
                    pulse = Mathf.Max(pulse, 0.82f);
                }

                float height = Mathf.Lerp(10f * scale, maxHeight, pulse);
                Rect backRect = new Rect(startX + i * (barWidth + gap), baseY - maxHeight, barWidth, maxHeight);
                Rect fillRect = new Rect(backRect.x, baseY - height, barWidth, height);
                DrawSolidRect(backRect, new Color(0.55f, 0.9f, 1f, 0.10f * alpha));
                DrawSolidRect(fillRect, new Color(focusColor.r, focusColor.g, focusColor.b, 0.70f * alpha));
            }
        }

        private void DrawCommunicatorGlitch(Rect panelRect, float scale, Color focusColor, float alpha)
        {
            float transitionNoise = 1f - ResolveIncomingAlpha();
            float confirmedPulse = guideState == GuideState.Confirmed
                ? 0.22f * Mathf.PingPong(Time.unscaledTime * 3.2f, 1f)
                : 0.06f;
            float intensity = Mathf.Clamp01(transitionNoise + confirmedPulse);
            if (communicatorGlitchAlpha <= 0f || intensity <= 0.02f)
            {
                return;
            }

            float tick = Mathf.Floor(Time.unscaledTime * 18f);
            for (int i = 0; i < 6; i++)
            {
                float seed = Mathf.Repeat(Mathf.Sin((i + 1) * 12.9898f + tick * 78.233f) * 43758.5453f, 1f);
                if (seed < 0.44f)
                {
                    continue;
                }

                float seedY = Mathf.Repeat(Mathf.Sin((i + 3) * 23.713f + tick * 31.19f) * 24634.634f, 1f);
                float width = Mathf.Lerp(26f, 86f, seed) * scale;
                float height = Mathf.Lerp(3f, 9f, seedY) * scale;
                Rect blockRect = new Rect(
                    panelRect.x + 18f * scale + seed * (panelRect.width - width - 36f * scale),
                    panelRect.y + 14f * scale + seedY * (panelRect.height - height - 28f * scale),
                    width,
                    height);
                DrawSolidRect(
                    blockRect,
                    new Color(focusColor.r, focusColor.g, focusColor.b, communicatorGlitchAlpha * intensity * alpha));
            }
        }

        private void DrawCommunicatorCorners(Rect panelRect, float scale, Color focusColor, float alpha)
        {
            float length = 38f * scale;
            float thickness = Mathf.Max(1f, 3f * scale);
            float resolvedAlpha = 0.86f * alpha;

            DrawSolidRect(new Rect(panelRect.x, panelRect.y, length, thickness), WithAlpha(focusColor, resolvedAlpha));
            DrawSolidRect(new Rect(panelRect.x, panelRect.y, thickness, length), WithAlpha(focusColor, resolvedAlpha));
            DrawSolidRect(new Rect(panelRect.xMax - length, panelRect.y, length, thickness), WithAlpha(focusColor, resolvedAlpha));
            DrawSolidRect(new Rect(panelRect.xMax - thickness, panelRect.y, thickness, length), WithAlpha(focusColor, resolvedAlpha));
            DrawSolidRect(new Rect(panelRect.x, panelRect.yMax - thickness, length, thickness), WithAlpha(focusColor, resolvedAlpha));
            DrawSolidRect(new Rect(panelRect.x, panelRect.yMax - length, thickness, length), WithAlpha(focusColor, resolvedAlpha));
            DrawSolidRect(new Rect(panelRect.xMax - length, panelRect.yMax - thickness, length, thickness), WithAlpha(focusColor, resolvedAlpha));
            DrawSolidRect(new Rect(panelRect.xMax - thickness, panelRect.yMax - length, thickness, length), WithAlpha(focusColor, resolvedAlpha));
        }

        private void TryPlayCommunicatorOpenSfx()
        {
            if (hasPlayedOpenSfx)
            {
                return;
            }

            hasPlayedOpenSfx = true;
            if (communicatorAudioSource == null || communicatorOpenSfx == null)
            {
                return;
            }

            communicatorAudioSource.PlayOneShot(communicatorOpenSfx, communicatorOpenSfxVolume);
        }

        private Vector2 ResolveFocusCenterGuiPoint(Vector2 anchor)
        {
            return new Vector2(anchor.x * Screen.width, (1f - anchor.y) * Screen.height);
        }

        private Rect ResolveFocusMarkerRect(float scale)
        {
            return ResolveFocusMarkerRect(scale, ResolveAnimatedFocusAnchor());
        }

        private Rect ResolveFocusMarkerRect(float scale, Vector2 markerFocusAnchor)
        {
            Vector2 center = ResolveFocusCenterGuiPoint(markerFocusAnchor);
            float pulse = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 7f);
            float size = (92f + pulse * 20f) * scale;
            return new Rect(center.x - size * 0.5f, center.y - size * 0.5f, size, size);
        }

        private void DrawBorder(Rect rect, float thickness, Color color, float alpha)
        {
            Color previous = GUI.color;
            GUI.color = new Color(color.r, color.g, color.b, color.a * Mathf.Clamp01(alpha));
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, thickness), whiteTexture);
            GUI.DrawTexture(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), whiteTexture);
            GUI.DrawTexture(new Rect(rect.x, rect.y, thickness, rect.height), whiteTexture);
            GUI.DrawTexture(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), whiteTexture);
            GUI.color = previous;
        }

        private void DrawSolidRect(Rect rect, Color color)
        {
            Color previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, whiteTexture);
            GUI.color = previous;
        }

        private static Rect OffsetRect(Rect rect, Vector2 offset)
        {
            return new Rect(rect.x + offset.x, rect.y + offset.y, rect.width, rect.height);
        }

        private static Color WithAlpha(Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, color.a * Mathf.Clamp01(alpha));
        }

        private void DrawPortrait(Rect rect, string contentSpeaker, float alpha)
        {
            Texture2D portrait = ResolvePortraitTexture(contentSpeaker);
            Color previous = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, Mathf.Clamp01(alpha));
            if (portrait != null)
            {
                GUI.DrawTexture(rect, portrait, ScaleMode.ScaleToFit, true);
            }
            else
            {
                GUI.Box(rect, GUIContent.none, portraitStyle);
                GUI.Label(rect, ResolveSpeakerPlaceholder(contentSpeaker), portraitLabelStyle);
            }

            GUI.color = previous;
        }

        private void DrawActionStatus(
            Rect rect,
            FocusKind contentFocusKind,
            GuideState contentGuideState,
            string contentPhaseLabel,
            float alpha)
        {
            Color previous = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, Mathf.Clamp01(alpha));
            GUI.Box(rect, GUIContent.none, chipStyle);

            Rect glyphRect = new Rect(rect.x + 6f, rect.y + 6f, rect.height - 12f, rect.height - 12f);
            DrawFocusGlyph(glyphRect, contentFocusKind, alpha);

            Rect stateRect = new Rect(rect.xMax - 58f, rect.y + 7f, 48f, rect.height - 14f);
            GUI.Box(stateRect, ResolveStateLabel(contentGuideState, contentPhaseLabel), stateStyle);

            Rect labelRect = new Rect(glyphRect.xMax + 6f, rect.y + 4f, stateRect.x - glyphRect.xMax - 10f, rect.height - 8f);
            GUI.Label(labelRect, ResolveFocusLabel(contentFocusKind), glyphStyle);
            GUI.color = previous;
        }

        private void DrawProgressReadout(
            Rect panelRect,
            float scale,
            int stepIndex,
            int stepCount,
            float alpha)
        {
            if (stepIndex <= 0 || stepCount <= 0)
            {
                return;
            }

            Rect labelRect = new Rect(
                panelRect.x + 28f * scale,
                panelRect.yMax - 46f * scale,
                126f * scale,
                26f * scale);

            Color previous = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, Mathf.Clamp01(alpha));
            GUI.Label(labelRect, $"STEP {stepIndex}/{stepCount}", progressStyle);

            float dotSize = 8f * scale;
            float gap = 8f * scale;
            float startX = labelRect.xMax + 10f * scale;
            float centerY = labelRect.y + labelRect.height * 0.5f;
            for (int i = 0; i < stepCount; i++)
            {
                bool filled = i < stepIndex;
                GUI.color = filled
                    ? new Color(0.42f, 0.95f, 1f, 0.9f * Mathf.Clamp01(alpha))
                    : new Color(0.72f, 0.84f, 0.9f, 0.24f * Mathf.Clamp01(alpha));
                GUI.DrawTexture(
                    new Rect(startX + i * (dotSize + gap), centerY - dotSize * 0.5f, dotSize, dotSize),
                    whiteTexture);
            }

            GUI.color = previous;
        }

        private void DrawFocusGlyph(Rect rect, FocusKind markerFocusKind, float alpha)
        {
            Texture2D icon = ResolveFocusIcon(markerFocusKind);
            Color previous = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, Mathf.Clamp01(alpha));
            if (focusMarkerTexture != null)
            {
                GUI.DrawTexture(rect, focusMarkerTexture, ScaleMode.ScaleToFit, true);
            }

            if (icon != null)
            {
                GUI.DrawTexture(rect, icon, ScaleMode.ScaleToFit, true);
            }
            else if (showReplaceablePlaceholders)
            {
                GUI.Label(rect, ResolveFocusShortLabel(markerFocusKind), glyphStyle);
            }

            GUI.color = previous;
        }

        private Texture2D ResolvePortraitTexture(string contentSpeaker)
        {
            if (!string.IsNullOrEmpty(contentSpeaker)
                && (contentSpeaker.Contains("\uc774\ub178\ub9ac")
                    || contentSpeaker.Contains("Inori")))
            {
                return inoriPortrait;
            }

            return operatorPortrait;
        }

        private Texture2D ResolveFocusIcon(FocusKind markerFocusKind)
        {
            switch (markerFocusKind)
            {
                case FocusKind.MeleeAttack:
                    return meleeAttackIcon;
                case FocusKind.MoveStick:
                    return moveStickIcon;
                case FocusKind.SwapMode:
                    return swapModeIcon;
                case FocusKind.RangedAttack:
                    return rangedAttackIcon;
                case FocusKind.Dodge:
                    return dodgeIcon;
                case FocusKind.Route:
                    return routeIcon;
                default:
                    return null;
            }
        }

        private static string ResolveSpeakerPlaceholder(string contentSpeaker)
        {
            return "UNKNOWN";
        }

        private static string ResolveStateLabel(GuideState contentGuideState, string contentPhaseLabel)
        {
            if (!string.IsNullOrWhiteSpace(contentPhaseLabel))
            {
                return contentPhaseLabel;
            }

            switch (contentGuideState)
            {
                case GuideState.Ready:
                    return "ACT";
                case GuideState.Confirmed:
                    return "OK";
                default:
                    return "READ";
            }
        }

        private static string ResolveFocusLabel(FocusKind markerFocusKind)
        {
            switch (markerFocusKind)
            {
                case FocusKind.MeleeAttack:
                    return "MELEE";
                case FocusKind.MoveStick:
                    return "MOVE";
                case FocusKind.SwapMode:
                    return "MODE";
                case FocusKind.RangedAttack:
                    return "FIRE";
                case FocusKind.Dodge:
                    return "DODGE";
                case FocusKind.Route:
                    return "ROUTE";
                default:
                    return "GUIDE";
            }
        }

        private static string ResolveFocusShortLabel(FocusKind markerFocusKind)
        {
            switch (markerFocusKind)
            {
                case FocusKind.MeleeAttack:
                    return "ATK";
                case FocusKind.MoveStick:
                    return "MOVE";
                case FocusKind.SwapMode:
                    return "MODE";
                case FocusKind.RangedAttack:
                    return "FIRE";
                case FocusKind.Dodge:
                    return "EVD";
                case FocusKind.Route:
                    return "GO";
                default:
                    return string.Empty;
            }
        }

        private Color ResolveFocusColor(FocusKind targetFocusKind, GuideState targetGuideState)
        {
            if (targetGuideState == GuideState.Confirmed)
            {
                return new Color(0.42f, 1f, 0.56f, 0.98f);
            }

            switch (targetFocusKind)
            {
                case FocusKind.Dodge:
                    return targetGuideState == GuideState.Ready
                        ? new Color(1f, 0.38f, 0.28f, 0.96f)
                        : new Color(1f, 0.55f, 0.46f, 0.62f);
                case FocusKind.Route:
                    return targetGuideState == GuideState.Ready
                        ? new Color(0.64f, 1f, 0.42f, 0.96f)
                        : new Color(0.7f, 1f, 0.56f, 0.62f);
                case FocusKind.SwapMode:
                    return targetGuideState == GuideState.Ready
                        ? new Color(1f, 0.84f, 0.32f, 0.96f)
                        : new Color(1f, 0.9f, 0.54f, 0.62f);
                default:
                    return targetGuideState == GuideState.Ready
                        ? new Color(0.38f, 0.94f, 1f, 0.96f)
                        : new Color(0.56f, 0.96f, 1f, 0.62f);
            }
        }

        private static Color ResolveMarkerFillColor(GuideState targetGuideState, float alpha)
        {
            float resolvedAlpha = Mathf.Clamp01(alpha);
            switch (targetGuideState)
            {
                case GuideState.Confirmed:
                    return new Color(0.12f, 1f, 0.34f, 0.28f * resolvedAlpha);
                case GuideState.Ready:
                    return new Color(1f, 1f, 1f, resolvedAlpha);
                default:
                    return new Color(1f, 1f, 1f, 0.55f * resolvedAlpha);
            }
        }

        private static Color ResolveContentGuiColor(GuideState targetGuideState, float alpha)
        {
            float resolvedAlpha = Mathf.Clamp01(alpha);
            switch (targetGuideState)
            {
                case GuideState.Confirmed:
                    return new Color(0.78f, 1f, 0.82f, resolvedAlpha);
                case GuideState.Ready:
                    return new Color(1f, 1f, 1f, resolvedAlpha);
                default:
                    return new Color(0.86f, 0.94f, 1f, 0.72f * resolvedAlpha);
            }
        }

        private float ResolveScale()
        {
            float widthScale = Screen.width / ReferenceWidth;
            float heightScale = Screen.height / ReferenceHeight;
            return Mathf.Clamp(Mathf.Min(widthScale, heightScale), 0.72f, 1.25f);
        }

        private bool IsSameGuide(
            string resolvedSpeaker,
            string resolvedDialogue,
            string resolvedInputLabel,
            FocusKind resolvedFocusKind)
        {
            return visible
                && speaker == resolvedSpeaker
                && dialogue == resolvedDialogue
                && inputLabel == resolvedInputLabel
                && focusKind == resolvedFocusKind;
        }

        private void CaptureOutgoingCue()
        {
            outgoingSpeaker = speaker;
            outgoingDialogue = dialogue;
            outgoingInputLabel = inputLabel;
            outgoingFocusKind = focusKind;
            outgoingGuideState = guideState;
            outgoingProgressStepIndex = progressStepIndex;
            outgoingProgressStepCount = progressStepCount;
            outgoingPhaseLabel = phaseLabel;
            outgoingFocusAnchor = ResolveAnimatedFocusAnchor();
        }

        private float ResolveIncomingAlpha()
        {
            if (transitionSeconds <= 0f)
            {
                return 1f;
            }

            float normalized = Mathf.Clamp01(transitionTimer / transitionSeconds);
            return Mathf.SmoothStep(0f, 1f, normalized);
        }

        private Vector2 ResolveAnimatedFocusAnchor()
        {
            float progress = ResolveIncomingAlpha();
            return Vector2.LerpUnclamped(transitionStartFocusAnchor, focusAnchor, progress);
        }

        private int ResolveVisibleDialogueCharacterCount()
        {
            if (string.IsNullOrEmpty(dialogue))
            {
                return 0;
            }

            int visibleCharacters = Mathf.CeilToInt(dialogueRevealTimer * dialogueCharactersPerSecond);
            return Mathf.Clamp(visibleCharacters, 0, dialogue.Length);
        }

        private static string TruncateDialogue(string value, int visibleCharacters)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            int count = Mathf.Clamp(visibleCharacters, 0, value.Length);
            return count >= value.Length ? value : value.Substring(0, count);
        }

        private void EnsureStyles()
        {
            if (whiteTexture == null)
            {
                whiteTexture = Texture2D.whiteTexture;
            }

            if (panelStyle != null)
            {
                return;
            }

            panelStyle = CreateBoxStyle(new Color(0.015f, 0.022f, 0.032f, 0.88f));
            markerStyle = CreateBoxStyle(new Color(0.12f, 0.92f, 1f, 0.26f));
            markerFillStyle = CreateBoxStyle(new Color(0.06f, 0.12f, 0.16f, 0.26f));
            portraitStyle = CreateBoxStyle(new Color(0.02f, 0.035f, 0.052f, 0.95f));
            chipStyle = CreateBoxStyle(new Color(0.06f, 0.095f, 0.12f, 0.92f));
            stateStyle = CreateBoxStyle(new Color(0.42f, 0.95f, 1f, 0.95f));
            stateStyle.alignment = TextAnchor.MiddleCenter;
            stateStyle.fontSize = 16;
            stateStyle.fontStyle = FontStyle.Bold;
            stateStyle.normal.textColor = Color.black;
            speakerStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.43f, 0.94f, 1f, 1f) }
            };
            dialogueStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = 31,
                wordWrap = true,
                normal = { textColor = Color.white }
            };
            inputStyle = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                normal =
                {
                    textColor = Color.black,
                    background = CreateTexture(new Color(0.42f, 0.95f, 1f, 0.95f))
                }
            };
            hintStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 23,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
            portraitLabelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 29,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.72f, 0.97f, 1f, 0.95f) }
            };
            progressStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.75f, 0.92f, 1f, 0.88f) }
            };
            glyphStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 19,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
            warningTitleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 30,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.92f, 0.98f, 1f, 1f) }
            };
            warningSubtitleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.88f, 0.94f, 1f, 0.86f) }
            };
        }

        private static GUIStyle CreateBoxStyle(Color color)
        {
            return new GUIStyle(GUI.skin.box)
            {
                normal = { background = CreateTexture(color) },
                border = new RectOffset(4, 4, 4, 4)
            };
        }

        private static Texture2D CreateTexture(Color color)
        {
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }
    }
}
