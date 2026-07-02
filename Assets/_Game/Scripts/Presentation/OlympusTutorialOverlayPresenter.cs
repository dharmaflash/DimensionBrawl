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

        private GUIStyle speakerStyle;
        private GUIStyle dialogueStyle;
        private GUIStyle inputStyle;
        private GUIStyle hintStyle;
        private GUIStyle panelStyle;
        private GUIStyle markerStyle;
        private GUIStyle markerFillStyle;
        private Texture2D whiteTexture;
        private string outgoingSpeaker = "OPERATOR";
        private string outgoingDialogue = string.Empty;
        private string outgoingInputLabel = string.Empty;
        private FocusKind outgoingFocusKind;
        private GuideState outgoingGuideState;
        private Vector2 outgoingFocusAnchor = new Vector2(0.78f, 0.70f);
        private Vector2 transitionStartFocusAnchor = new Vector2(0.78f, 0.70f);
        private float transitionTimer = 1f;
        private float dialogueRevealTimer;
        private bool hasOutgoingCue;

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
        }

        public void Hide()
        {
            visible = false;
            dialogue = string.Empty;
            inputLabel = string.Empty;
            focusKind = FocusKind.None;
            guideState = GuideState.Focus;
            hasOutgoingCue = false;
            transitionTimer = transitionSeconds;
            dialogueRevealTimer = 0f;
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
            DrawDialoguePanel(scale);
            DrawFocusMarker(scale);
        }

        private void DrawDialoguePanel(float scale)
        {
            Rect panelRect = ResolveDialoguePanelRect(scale);
            float incomingAlpha = ResolveIncomingAlpha();
            float outgoingAlpha = hasOutgoingCue ? 1f - incomingAlpha : 0f;

            Color previousColor = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, Mathf.Max(incomingAlpha, outgoingAlpha));
            GUI.Box(panelRect, GUIContent.none, panelStyle);
            GUI.color = previousColor;

            if (hasOutgoingCue)
            {
                DrawDialogueContent(
                    panelRect,
                    scale,
                    outgoingSpeaker,
                    outgoingDialogue,
                    outgoingInputLabel,
                    outgoingGuideState,
                    outgoingAlpha,
                    outgoingDialogue.Length);
            }

            DrawDialogueContent(
                panelRect,
                scale,
                speaker,
                dialogue,
                inputLabel,
                guideState,
                incomingAlpha,
                ResolveVisibleDialogueCharacterCount());
        }

        private void DrawDialogueContent(
            Rect panelRect,
            float scale,
            string contentSpeaker,
            string contentDialogue,
            string contentInputLabel,
            GuideState contentGuideState,
            float alpha,
            int visibleDialogueCharacters)
        {
            if (alpha <= 0.001f)
            {
                return;
            }

            Color previousColor = GUI.color;
            GUI.color = ResolveContentGuiColor(contentGuideState, alpha);

            Rect speakerRect = new Rect(
                panelRect.x + 28f * scale,
                panelRect.y + 20f * scale,
                panelRect.width - 44f * scale,
                34f * scale);
            GUI.Label(speakerRect, contentSpeaker, speakerStyle);

            Rect dialogueRect = new Rect(
                panelRect.x + 28f * scale,
                panelRect.y + 64f * scale,
                panelRect.width - 56f * scale,
                92f * scale);
            GUI.Label(dialogueRect, TruncateDialogue(contentDialogue, visibleDialogueCharacters), dialogueStyle);

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
            if (hasOutgoingCue)
            {
                DrawFocusMarker(
                    scale,
                    outgoingFocusKind,
                    outgoingGuideState,
                    outgoingFocusAnchor,
                    outgoingInputLabel,
                    1f - incomingAlpha);
            }

            DrawFocusMarker(scale, focusKind, guideState, ResolveAnimatedFocusAnchor(), inputLabel, incomingAlpha);
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
            float width = Mathf.Min(Screen.width - 56f * scale, 860f * scale);
            float height = 196f * scale;
            return new Rect(
                38f * scale,
                Mathf.Max(24f * scale, Screen.height * 0.5f - height * 0.5f),
                width,
                height);
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
