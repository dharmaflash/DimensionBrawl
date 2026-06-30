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

        private const float ReferenceWidth = 1920f;
        private const float ReferenceHeight = 1080f;

        [SerializeField] private bool visible;
        [SerializeField] private string speaker = "OPERATOR";
        [SerializeField] private string dialogue = string.Empty;
        [SerializeField] private string inputLabel = string.Empty;
        [SerializeField] private FocusKind focusKind;
        [SerializeField] private Vector2 focusAnchor = new Vector2(0.78f, 0.70f);

        private GUIStyle speakerStyle;
        private GUIStyle dialogueStyle;
        private GUIStyle inputStyle;
        private GUIStyle hintStyle;
        private GUIStyle panelStyle;
        private GUIStyle markerStyle;
        private GUIStyle markerFillStyle;
        private Texture2D whiteTexture;

        public bool Visible => visible;
        public FocusKind CurrentFocusKind => focusKind;
        public Vector2 CurrentFocusAnchor => focusAnchor;
        public Vector2 CurrentFocusCenterGuiPoint => ResolveFocusCenterGuiPoint(focusAnchor);
        public Rect CurrentFocusMarkerGuiRect => ResolveFocusMarkerRect(ResolveScale());
        public Rect CurrentDialoguePanelGuiRect => ResolveDialoguePanelRect(ResolveScale());

        public void Show(
            string newSpeaker,
            string newDialogue,
            string newInputLabel,
            FocusKind newFocusKind,
            Vector2 newFocusAnchor)
        {
            speaker = string.IsNullOrWhiteSpace(newSpeaker) ? "OPERATOR" : newSpeaker;
            dialogue = newDialogue ?? string.Empty;
            inputLabel = newInputLabel ?? string.Empty;
            focusKind = newFocusKind;
            focusAnchor = new Vector2(Mathf.Clamp01(newFocusAnchor.x), Mathf.Clamp01(newFocusAnchor.y));
            visible = true;
        }

        public void Hide()
        {
            visible = false;
            dialogue = string.Empty;
            inputLabel = string.Empty;
            focusKind = FocusKind.None;
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
            GUI.Box(panelRect, GUIContent.none, panelStyle);

            Rect speakerRect = new Rect(
                panelRect.x + 28f * scale,
                panelRect.y + 20f * scale,
                panelRect.width - 44f * scale,
                34f * scale);
            GUI.Label(speakerRect, speaker, speakerStyle);

            Rect dialogueRect = new Rect(
                panelRect.x + 28f * scale,
                panelRect.y + 64f * scale,
                panelRect.width - 56f * scale,
                92f * scale);
            GUI.Label(dialogueRect, dialogue, dialogueStyle);

            if (string.IsNullOrWhiteSpace(inputLabel))
            {
                return;
            }

            Rect inputRect = new Rect(
                panelRect.x + panelRect.width - 190f * scale,
                panelRect.y + panelRect.height - 52f * scale,
                154f * scale,
                34f * scale);
            GUI.Box(inputRect, inputLabel, inputStyle);
        }

        private void DrawFocusMarker(float scale)
        {
            if (focusKind == FocusKind.None)
            {
                return;
            }

            Rect markerRect = ResolveFocusMarkerRect(scale);
            Rect fillRect = new Rect(
                markerRect.x + 8f * scale,
                markerRect.y + 8f * scale,
                markerRect.width - 16f * scale,
                markerRect.height - 16f * scale);

            GUI.Box(fillRect, GUIContent.none, markerFillStyle);
            DrawBorder(markerRect, 4f * scale, ResolveFocusColor());

            if (!string.IsNullOrWhiteSpace(inputLabel))
            {
                Rect hintRect = new Rect(
                    markerRect.x - 28f * scale,
                    markerRect.y - 34f * scale,
                    markerRect.width + 56f * scale,
                    28f * scale);
                GUI.Label(hintRect, inputLabel, hintStyle);
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
            Vector2 center = ResolveFocusCenterGuiPoint(focusAnchor);
            float pulse = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 7f);
            float size = (92f + pulse * 20f) * scale;
            return new Rect(center.x - size * 0.5f, center.y - size * 0.5f, size, size);
        }

        private void DrawBorder(Rect rect, float thickness, Color color)
        {
            Color previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, thickness), whiteTexture);
            GUI.DrawTexture(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), whiteTexture);
            GUI.DrawTexture(new Rect(rect.x, rect.y, thickness, rect.height), whiteTexture);
            GUI.DrawTexture(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), whiteTexture);
            GUI.color = previous;
        }

        private Color ResolveFocusColor()
        {
            switch (focusKind)
            {
                case FocusKind.Dodge:
                    return new Color(1f, 0.38f, 0.28f, 0.96f);
                case FocusKind.Route:
                    return new Color(0.64f, 1f, 0.42f, 0.96f);
                case FocusKind.SwapMode:
                    return new Color(1f, 0.84f, 0.32f, 0.96f);
                default:
                    return new Color(0.38f, 0.94f, 1f, 0.96f);
            }
        }

        private float ResolveScale()
        {
            float widthScale = Screen.width / ReferenceWidth;
            float heightScale = Screen.height / ReferenceHeight;
            return Mathf.Clamp(Mathf.Min(widthScale, heightScale), 0.72f, 1.25f);
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
