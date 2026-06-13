using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    public sealed class LoginStartPromptPresenter : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private Graphic promptGraphic;
        [SerializeField] private Graphic glowGraphic;
        [SerializeField] private RectTransform[] scaleTargets = Array.Empty<RectTransform>();
        [SerializeField, Min(0.1f)] private float pulseSeconds = 1.85f;
        [SerializeField, Range(0f, 1f)] private float promptAlphaMin = 0.68f;
        [SerializeField, Range(0f, 1f)] private float promptAlphaMax = 1f;
        [SerializeField, Range(0f, 1f)] private float glowAlphaMin = 0.42f;
        [SerializeField, Range(0f, 1f)] private float glowAlphaMax = 0.92f;
        [SerializeField, Min(0f)] private float idleScaleAmplitude = 0.012f;
        [SerializeField, Min(0f)] private float pressedScaleInset = 0.018f;
        [SerializeField, Min(0f)] private float confirmScaleBoost = 0.028f;
        [SerializeField, Min(0.01f)] private float confirmSeconds = 0.16f;
        [SerializeField] private bool useUnscaledTime = true;

        private Color promptBaseColor;
        private Color glowBaseColor;
        private Vector3[] baseScales = Array.Empty<Vector3>();
        private bool cached;
        private bool pointerHeld;
        private float confirmTimer;

        private void Reset()
        {
            promptGraphic = GetComponent<Graphic>();
            scaleTargets = new[] { transform as RectTransform };
        }

        private void Awake()
        {
            CacheBaseState();
        }

        private void OnEnable()
        {
            CacheBaseState();
            pointerHeld = false;
            confirmTimer = 0f;
        }

        private void OnDisable()
        {
            RestoreBaseState();
        }

        private void Update()
        {
            if (confirmTimer > 0f)
            {
                confirmTimer = Mathf.Max(0f, confirmTimer - DeltaTime);
            }

            float cycle = Mathf.Max(0.1f, pulseSeconds);
            float wave = Mathf.Sin(TimeNow / cycle * Mathf.PI * 2f) * 0.5f + 0.5f;
            float pulse = Mathf.SmoothStep(0f, 1f, wave);
            float confirm = ConfirmPulse;

            float promptAlpha = Mathf.Lerp(promptAlphaMin, promptAlphaMax, pulse);
            float glowAlpha = Mathf.Lerp(glowAlphaMin, glowAlphaMax, pulse);
            if (pointerHeld)
            {
                promptAlpha -= 0.08f;
                glowAlpha += 0.08f;
            }

            promptAlpha += confirm * 0.12f;
            glowAlpha += confirm * 0.22f;
            float scale = 1f + ((pulse - 0.5f) * 2f * idleScaleAmplitude) + (confirm * confirmScaleBoost);
            if (pointerHeld)
            {
                scale -= pressedScaleInset;
            }

            Apply(promptAlpha, glowAlpha, scale);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            pointerHeld = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            pointerHeld = false;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            pointerHeld = false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            pointerHeld = false;
            confirmTimer = Mathf.Max(0.01f, confirmSeconds);
        }

        private void CacheBaseState()
        {
            if (cached)
            {
                return;
            }

            promptBaseColor = promptGraphic != null ? promptGraphic.color : Color.white;
            glowBaseColor = glowGraphic != null ? glowGraphic.color : Color.white;
            baseScales = new Vector3[scaleTargets.Length];
            for (int i = 0; i < scaleTargets.Length; i++)
            {
                baseScales[i] = scaleTargets[i] != null ? scaleTargets[i].localScale : Vector3.one;
            }

            cached = true;
        }

        private void RestoreBaseState()
        {
            if (!cached)
            {
                return;
            }

            if (promptGraphic != null)
            {
                promptGraphic.color = promptBaseColor;
            }

            if (glowGraphic != null)
            {
                glowGraphic.color = glowBaseColor;
            }

            for (int i = 0; i < scaleTargets.Length && i < baseScales.Length; i++)
            {
                if (scaleTargets[i] != null)
                {
                    scaleTargets[i].localScale = baseScales[i];
                }
            }
        }

        private void Apply(float promptAlpha, float glowAlpha, float scale)
        {
            SetAlpha(promptGraphic, promptBaseColor, promptAlpha);
            SetAlpha(glowGraphic, glowBaseColor, glowAlpha);

            for (int i = 0; i < scaleTargets.Length && i < baseScales.Length; i++)
            {
                if (scaleTargets[i] == null)
                {
                    continue;
                }

                Vector3 baseScale = baseScales[i];
                scaleTargets[i].localScale = new Vector3(baseScale.x * scale, baseScale.y * scale, baseScale.z);
            }
        }

        private float ConfirmPulse
        {
            get
            {
                if (confirmTimer <= 0f)
                {
                    return 0f;
                }

                float progress = 1f - Mathf.Clamp01(confirmTimer / Mathf.Max(0.01f, confirmSeconds));
                return Mathf.Sin(progress * Mathf.PI);
            }
        }

        private float DeltaTime => useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        private float TimeNow => useUnscaledTime ? Time.unscaledTime : Time.time;

        private static void SetAlpha(Graphic graphic, Color baseColor, float alpha)
        {
            if (graphic == null)
            {
                return;
            }

            Color color = baseColor;
            color.a = baseColor.a * Mathf.Clamp01(alpha);
            graphic.color = color;
        }
    }
}
