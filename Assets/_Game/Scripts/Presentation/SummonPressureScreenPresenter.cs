using DimensionBrawl.Combat;
using UnityEngine;

namespace DimensionBrawl.Presentation
{
    [DisallowMultipleComponent]
    public sealed class SummonPressureScreenPresenter : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        [SerializeField] private SummonPressureScreen pressureScreen;
        [SerializeField] private Transform visualRoot;
        [SerializeField] private Renderer[] screenRenderers = System.Array.Empty<Renderer>();
        [SerializeField] private Color activeColor = new Color(0.22f, 1f, 0.82f, 0.42f);
        [SerializeField] private Color interceptColor = new Color(0.92f, 1f, 1f, 0.88f);
        [SerializeField, Min(0f)] private float activationFlashSeconds = 0.12f;
        [SerializeField, Min(0f)] private float interceptFlashSeconds = 0.18f;
        [SerializeField, Min(0f)] private float finalHitLingerSeconds = 0.16f;
        [SerializeField, Min(0.01f)] private float pulseSpeed = 9f;
        [SerializeField, Min(0f)] private float pulseScale = 0.04f;

        private MaterialPropertyBlock propertyBlock;
        private Vector3 visualBaseScale = Vector3.one;
        private float flashTimer;
        private float lingerTimer;
        private float lastKnownRadius = 1.35f;
        private bool showing;
        private bool subscribed;

        public SummonPressureScreen PressureScreen => pressureScreen;
        public bool IsShowing => showing;
        public int RendererCount => screenRenderers != null ? screenRenderers.Length : 0;

        private void Awake()
        {
            propertyBlock ??= new MaterialPropertyBlock();
            if (pressureScreen == null)
                pressureScreen = GetComponentInParent<SummonPressureScreen>();

            if (visualRoot == null)
                visualRoot = transform;

            visualBaseScale = visualRoot.localScale;
            if (screenRenderers == null || screenRenderers.Length == 0)
                screenRenderers = GetComponentsInChildren<Renderer>(includeInactive: true);

            SetShowing(false);
        }

        private void OnEnable()
        {
            Subscribe();
            RefreshNow();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            if (flashTimer > 0f)
                flashTimer = Mathf.Max(0f, flashTimer - deltaTime);

            if (pressureScreen == null || !pressureScreen.IsActive)
            {
                if (lingerTimer > 0f)
                    lingerTimer = Mathf.Max(0f, lingerTimer - deltaTime);
                else
                {
                    SetShowing(false);
                    return;
                }
            }

            RefreshVisual();
        }

        public void ConfigurePresentation(
            SummonPressureScreen newPressureScreen,
            Transform newVisualRoot,
            Renderer[] newScreenRenderers)
        {
            Unsubscribe();
            pressureScreen = newPressureScreen;
            visualRoot = newVisualRoot != null ? newVisualRoot : transform;
            visualBaseScale = visualRoot.localScale;
            screenRenderers = newScreenRenderers ?? System.Array.Empty<Renderer>();
            Subscribe();
            RefreshNow();
        }

        public void RefreshNow()
        {
            if (pressureScreen != null && pressureScreen.IsActive)
            {
                lastKnownRadius = pressureScreen.ActiveRadius;
                SetShowing(true);
            }
            else if (lingerTimer <= 0f)
            {
                SetShowing(false);
            }

            RefreshVisual();
        }

        private void OnScreenActivated(SummonPressureScreen screen)
        {
            lastKnownRadius = screen.ActiveRadius;
            flashTimer = Mathf.Max(flashTimer, activationFlashSeconds);
            lingerTimer = 0f;
            SetShowing(true);
            RefreshVisual();
        }

        private void OnScreenIntercepted(SummonPressureScreen screen, BossBarrageProjectile projectile)
        {
            lastKnownRadius = screen.ActiveRadius;
            flashTimer = Mathf.Max(flashTimer, interceptFlashSeconds);
            lingerTimer = Mathf.Max(lingerTimer, finalHitLingerSeconds);
            SetShowing(true);
            RefreshVisual();
        }

        private void OnScreenDeactivated(SummonPressureScreen screen)
        {
            if (lingerTimer <= 0f)
                SetShowing(false);
        }

        private void RefreshVisual()
        {
            if (!showing || visualRoot == null)
            {
                return;
            }

            float radius = pressureScreen != null && pressureScreen.IsActive
                ? pressureScreen.ActiveRadius
                : lastKnownRadius;
            lastKnownRadius = Mathf.Max(0.05f, radius);

            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseScale;
            float flash = ResolveFlashWeight();
            float scale = Mathf.Max(0.05f, lastKnownRadius) * 2f * (pulse + flash * 0.12f);
            visualRoot.localScale = Vector3.Scale(visualBaseScale, new Vector3(scale, scale, scale));

            Color color = Color.Lerp(activeColor, interceptColor, flash);
            if (pressureScreen == null || !pressureScreen.IsActive)
            {
                float lingerAlpha = finalHitLingerSeconds > 0f ? Mathf.Clamp01(lingerTimer / finalHitLingerSeconds) : 0f;
                color.a *= lingerAlpha;
            }

            ApplyColor(color);
        }

        private float ResolveFlashWeight()
        {
            float longestFlash = Mathf.Max(activationFlashSeconds, interceptFlashSeconds);
            return longestFlash > 0f ? Mathf.Clamp01(flashTimer / longestFlash) : 0f;
        }

        private void ApplyColor(Color color)
        {
            propertyBlock ??= new MaterialPropertyBlock();
            for (int i = 0; i < screenRenderers.Length; i++)
            {
                Renderer screenRenderer = screenRenderers[i];
                if (screenRenderer == null)
                    continue;

                screenRenderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor(BaseColorId, color);
                propertyBlock.SetColor(ColorId, color);
                screenRenderer.SetPropertyBlock(propertyBlock);
            }
        }

        private void SetShowing(bool value)
        {
            showing = value;
            if (visualRoot != null
                && visualRoot != transform
                && visualRoot.gameObject != gameObject
                && visualRoot.gameObject.activeSelf != value)
            {
                visualRoot.gameObject.SetActive(value);
            }
        }

        private void Subscribe()
        {
            if (pressureScreen == null || subscribed)
            {
                return;
            }

            pressureScreen.Activated += OnScreenActivated;
            pressureScreen.Intercepted += OnScreenIntercepted;
            pressureScreen.Deactivated += OnScreenDeactivated;
            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (pressureScreen == null || !subscribed)
            {
                return;
            }

            pressureScreen.Activated -= OnScreenActivated;
            pressureScreen.Intercepted -= OnScreenIntercepted;
            pressureScreen.Deactivated -= OnScreenDeactivated;
            subscribed = false;
        }
    }
}
