using UnityEngine;

namespace DimensionBrawl.Presentation
{
    [DisallowMultipleComponent]
    public sealed class CinematicTutorialPromptPresenter : MonoBehaviour
    {
        [Header("Camera")]
        [SerializeField] private Camera targetCamera;
        [SerializeField, Min(0.5f)] private float promptDistance = 2.35f;

        [Header("Layout")]
        [SerializeField] private Vector2 defaultScreenAnchor = new Vector2(0.5f, 0.72f);
        [SerializeField] private Vector2 backdropSize = new Vector2(0.70f, 0.18f);
        [SerializeField, Min(0.01f)] private float titleCharacterSize = 0.014f;
        [SerializeField, Min(0.01f)] private float guideCharacterSize = 0.009f;
        [SerializeField] private Vector3 titleLocalOffset = new Vector3(0f, 0.026f, -0.018f);
        [SerializeField] private Vector3 guideLocalOffset = new Vector3(0f, -0.037f, -0.018f);
        [SerializeField, Min(0.1f)] private float minimumVisibleSeconds = 0.85f;

        [Header("Color")]
        [SerializeField] private Color qteColor = new Color(1.00f, 0.92f, 0.32f, 1f);
        [SerializeField] private Color warningColor = new Color(1.00f, 0.34f, 0.30f, 1f);
        [SerializeField] private Color tutorialColor = new Color(0.38f, 0.92f, 1.00f, 1f);
        [SerializeField] private Color guideColor = Color.white;
        [SerializeField] private Color shadowColor = new Color(0f, 0f, 0f, 0.92f);
        [SerializeField] private Color backdropColor = new Color(0.02f, 0.025f, 0.035f, 0.72f);

        private Transform promptRoot;
        private Transform backdropTransform;
        private TextMesh titleText;
        private TextMesh titleShadowText;
        private TextMesh guideText;
        private TextMesh guideShadowText;
        private Renderer backdropRenderer;
        private Material backdropMaterial;
        private Vector2 activeAnchor;
        private float remainingSeconds;
        private string activeCueId;

        public string ActiveCueId => activeCueId;
        public bool HasActivePrompt => remainingSeconds > 0f;

        private void Awake()
        {
            ResolveTargetCamera();
            EnsureVisuals();
            HidePrompt();
        }

        private void OnDisable()
        {
            HidePrompt();
        }

        private void Update()
        {
            if (remainingSeconds <= 0f)
            {
                return;
            }

            remainingSeconds = Mathf.Max(0f, remainingSeconds - Time.unscaledDeltaTime);
            if (remainingSeconds <= 0f)
            {
                HidePrompt();
                return;
            }

            UpdatePromptPose();
        }

        public void ShowCue(CinematicSequenceProfile.TutorialCue cue)
        {
            if (!cue.Enabled)
            {
                return;
            }

            ResolveTargetCamera();
            EnsureVisuals();
            activeCueId = cue.CueId;
            activeAnchor = ResolveAnchor(cue.ScreenAnchor);
            remainingSeconds = Mathf.Max(cue.DurationSeconds, minimumVisibleSeconds);

            string title = ResolveTitle(cue);
            string guide = ResolveGuide(cue);
            Color titleColor = ResolveTitleColor(cue.CueKind);
            ApplyText(titleText, title, titleCharacterSize, titleColor);
            ApplyText(titleShadowText, title, titleCharacterSize, shadowColor);
            ApplyText(guideText, guide, guideCharacterSize, guideColor);
            ApplyText(guideShadowText, guide, guideCharacterSize, shadowColor);

            if (backdropRenderer != null)
            {
                backdropRenderer.enabled = true;
            }

            promptRoot.gameObject.SetActive(true);
            UpdatePromptPose();
        }

        public void HidePrompt()
        {
            remainingSeconds = 0f;
            activeCueId = string.Empty;
            if (promptRoot != null)
            {
                promptRoot.gameObject.SetActive(false);
            }
        }

        private void ResolveTargetCamera()
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }
        }

        private void EnsureVisuals()
        {
            if (promptRoot != null)
            {
                return;
            }

            GameObject rootObject = new GameObject("CinematicTutorialPrompt");
            promptRoot = rootObject.transform;
            promptRoot.SetParent(transform, worldPositionStays: false);

            GameObject backdropObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
            backdropObject.name = "Backdrop";
            backdropObject.transform.SetParent(promptRoot, worldPositionStays: false);
            backdropObject.transform.localPosition = new Vector3(0f, -0.015f, 0.020f);
            backdropObject.transform.localScale = new Vector3(backdropSize.x, backdropSize.y, 1f);
            Collider collider = backdropObject.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            backdropTransform = backdropObject.transform;
            backdropRenderer = backdropObject.GetComponent<Renderer>();
            backdropMaterial = CreateBackdropMaterial();
            if (backdropRenderer != null && backdropMaterial != null)
            {
                backdropRenderer.sharedMaterial = backdropMaterial;
            }

            titleShadowText = CreateText("TitleShadow", titleLocalOffset + new Vector3(0.006f, -0.006f, 0.004f), shadowColor);
            titleText = CreateText("Title", titleLocalOffset, tutorialColor);
            guideShadowText = CreateText("GuideShadow", guideLocalOffset + new Vector3(0.004f, -0.004f, 0.004f), shadowColor);
            guideText = CreateText("Guide", guideLocalOffset, guideColor);
        }

        private TextMesh CreateText(string objectName, Vector3 localPosition, Color color)
        {
            GameObject textObject = new GameObject(objectName);
            textObject.transform.SetParent(promptRoot, worldPositionStays: false);
            textObject.transform.localPosition = localPosition;
            textObject.transform.localRotation = Quaternion.identity;
            TextMesh textMesh = textObject.AddComponent<TextMesh>();
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.richText = false;
            textMesh.fontSize = 96;
            textMesh.characterSize = titleCharacterSize;
            textMesh.color = color;
            return textMesh;
        }

        private Material CreateBackdropMaterial()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit")
                ?? Shader.Find("Sprites/Default")
                ?? Shader.Find("Unlit/Color")
                ?? Shader.Find("Standard");
            if (shader == null)
            {
                return null;
            }

            Material material = new Material(shader)
            {
                name = "CinematicTutorialPrompt_Backdrop"
            };
            material.color = backdropColor;
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", backdropColor);
            }

            if (material.HasProperty("_Surface"))
            {
                material.SetFloat("_Surface", 1f);
            }

            if (material.HasProperty("_AlphaClip"))
            {
                material.SetFloat("_AlphaClip", 0f);
            }

            material.renderQueue = 3000;
            return material;
        }

        private void ApplyText(TextMesh textMesh, string value, float characterSize, Color color)
        {
            if (textMesh == null)
            {
                return;
            }

            textMesh.text = value;
            textMesh.characterSize = characterSize;
            textMesh.color = color;
        }

        private void UpdatePromptPose()
        {
            if (targetCamera == null || promptRoot == null)
            {
                return;
            }

            if (promptRoot.parent != targetCamera.transform)
            {
                promptRoot.SetParent(targetCamera.transform, worldPositionStays: false);
            }

            float height = 2f * promptDistance * Mathf.Tan(targetCamera.fieldOfView * Mathf.Deg2Rad * 0.5f);
            float width = height * Mathf.Max(0.1f, targetCamera.aspect);
            float x = (activeAnchor.x - 0.5f) * width;
            float y = (activeAnchor.y - 0.5f) * height;
            promptRoot.localPosition = new Vector3(x, y, promptDistance);
            promptRoot.localRotation = Quaternion.identity;

            if (backdropTransform != null)
            {
                backdropTransform.localScale = new Vector3(backdropSize.x, backdropSize.y, 1f);
            }
        }

        private Vector2 ResolveAnchor(Vector2 cueAnchor)
        {
            Vector2 anchor = cueAnchor == Vector2.zero ? defaultScreenAnchor : cueAnchor;
            return new Vector2(Mathf.Clamp01(anchor.x), Mathf.Clamp01(anchor.y));
        }

        private static string ResolveTitle(CinematicSequenceProfile.TutorialCue cue)
        {
            if (!string.IsNullOrWhiteSpace(cue.GuideText))
            {
                return cue.GuideText.Trim();
            }

            if (!string.IsNullOrWhiteSpace(cue.PromptKey))
            {
                return cue.PromptKey.Trim().Replace('_', ' ');
            }

            return cue.CueKind.ToString().ToUpperInvariant();
        }

        private static string ResolveGuide(CinematicSequenceProfile.TutorialCue cue)
        {
            switch (cue.CueKind)
            {
                case CinematicSequenceProfile.TutorialCueKind.QtePrompt:
                    return "TIMING";
                case CinematicSequenceProfile.TutorialCueKind.WarningPrompt:
                    return "EVADE";
                case CinematicSequenceProfile.TutorialCueKind.SkillPrompt:
                    return "CAST";
                case CinematicSequenceProfile.TutorialCueKind.UltimatePrompt:
                    return "READY";
                case CinematicSequenceProfile.TutorialCueKind.ClickPrompt:
                    return "BASIC";
                default:
                    return string.IsNullOrWhiteSpace(cue.PromptKey)
                        ? "GUIDE"
                        : cue.PromptKey.Trim().Replace('_', ' ');
            }
        }

        private Color ResolveTitleColor(CinematicSequenceProfile.TutorialCueKind cueKind)
        {
            switch (cueKind)
            {
                case CinematicSequenceProfile.TutorialCueKind.QtePrompt:
                    return qteColor;
                case CinematicSequenceProfile.TutorialCueKind.WarningPrompt:
                    return warningColor;
                default:
                    return tutorialColor;
            }
        }
    }
}
