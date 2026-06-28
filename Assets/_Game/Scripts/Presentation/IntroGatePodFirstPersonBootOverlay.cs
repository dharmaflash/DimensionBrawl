using UnityEngine;
using UnityEngine.UI;

namespace DimensionBrawl.Presentation
{
    [DisallowMultipleComponent]
    public sealed class IntroGatePodFirstPersonBootOverlay : MonoBehaviour
    {
        private static readonly int AlphaId = Shader.PropertyToID("_Alpha");
        private static readonly int NoiseStrengthId = Shader.PropertyToID("_NoiseStrength");
        private static readonly int ScanlineStrengthId = Shader.PropertyToID("_ScanlineStrength");
        private static readonly int JitterStrengthId = Shader.PropertyToID("_JitterStrength");
        private static readonly int PhaseId = Shader.PropertyToID("_Phase");

        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image glitchImage;
        [SerializeField] private Material glitchMaterial;
        [SerializeField] private CanvasGroup hudGroup;
        [SerializeField] private RectTransform leftStatusBar;
        [SerializeField] private RectTransform rightStatusBar;
        [SerializeField, Min(0f)] private float statusBarMaxWidth = 430f;
        [SerializeField, Min(0f)] private float statusBarThickness = 2f;

        public bool HasBindings =>
            canvasGroup != null
            && glitchImage != null
            && glitchMaterial != null
            && hudGroup != null
            && leftStatusBar != null
            && rightStatusBar != null;

        public void Configure(
            CanvasGroup newCanvasGroup,
            Image newGlitchImage,
            Material newGlitchMaterial,
            CanvasGroup newHudGroup,
            RectTransform newLeftStatusBar,
            RectTransform newRightStatusBar,
            float newStatusBarMaxWidth,
            float newStatusBarThickness)
        {
            canvasGroup = newCanvasGroup;
            glitchImage = newGlitchImage;
            glitchMaterial = newGlitchMaterial;
            hudGroup = newHudGroup;
            leftStatusBar = newLeftStatusBar;
            rightStatusBar = newRightStatusBar;
            statusBarMaxWidth = Mathf.Max(0f, newStatusBarMaxWidth);
            statusBarThickness = Mathf.Max(0f, newStatusBarThickness);
            Clear();
        }

        public void Apply(in IntroGatePodFirstPersonBootFrame frame)
        {
            float rootAlpha = Mathf.Max(frame.GlitchAlpha, frame.HudAlpha);
            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Clamp01(rootAlpha);
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }

            if (glitchImage != null)
            {
                glitchImage.enabled = frame.GlitchAlpha > 0.001f;
                glitchImage.raycastTarget = false;
                if (glitchMaterial != null && glitchImage.material != glitchMaterial)
                {
                    glitchImage.material = glitchMaterial;
                }
            }

            ApplyGlitchMaterial(frame);
            ApplyStatusBars(frame);
        }

        public void Clear()
        {
            Apply(new IntroGatePodFirstPersonBootFrame(0f, 0f, 0f, 0f, 0f, statusBarMaxWidth, statusBarThickness));
        }

        private void ApplyGlitchMaterial(in IntroGatePodFirstPersonBootFrame frame)
        {
            if (glitchMaterial == null)
            {
                return;
            }

            glitchMaterial.SetFloat(AlphaId, Mathf.Clamp01(frame.GlitchAlpha));
            glitchMaterial.SetFloat(NoiseStrengthId, Mathf.Max(0f, frame.GlitchStrength));
            glitchMaterial.SetFloat(ScanlineStrengthId, Mathf.Clamp01(0.62f * frame.GlitchAlpha));
            glitchMaterial.SetFloat(JitterStrengthId, Mathf.Clamp01(0.42f * frame.GlitchStrength));
            glitchMaterial.SetFloat(PhaseId, frame.Phase);
        }

        private void ApplyStatusBars(in IntroGatePodFirstPersonBootFrame frame)
        {
            if (hudGroup != null)
            {
                hudGroup.alpha = Mathf.Clamp01(frame.HudAlpha);
                hudGroup.interactable = false;
                hudGroup.blocksRaycasts = false;
            }

            float width = Mathf.Max(0f, frame.StatusBarMaxWidth) * Mathf.Clamp01(frame.HudOpenAmount);
            float thickness = Mathf.Max(0f, frame.StatusBarThickness);
            ApplyBarSize(leftStatusBar, width, thickness);
            ApplyBarSize(rightStatusBar, width, thickness);
        }

        private static void ApplyBarSize(RectTransform bar, float width, float thickness)
        {
            if (bar == null)
            {
                return;
            }

            bar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            bar.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, thickness);
        }

        private void Awake()
        {
            Clear();
        }

        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                Clear();
            }
        }
    }
}
