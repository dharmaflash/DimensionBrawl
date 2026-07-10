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
        [SerializeField] private bool renderVisuals;
        [SerializeField] private Color activeColor = new Color(0.22f, 1f, 0.82f, 0.09f);
        [SerializeField] private Color tierTwoColor = new Color(0.38f, 0.74f, 1f, 0.10f);
        [SerializeField] private Color tierThreeColor = new Color(1f, 0.76f, 0.24f, 0.12f);
        [SerializeField] private Color interceptColor = new Color(0.92f, 1f, 1f, 0.16f);
        [SerializeField, Range(0.18f, 1f)] private float visualRadiusScale = 0.18f;
        [SerializeField, Min(0f)] private float activationFlashSeconds = 0.08f;
        [SerializeField, Min(0f)] private float interceptFlashSeconds = 0.12f;
        [SerializeField, Min(0f)] private float finalHitLingerSeconds = 0.1f;
        [SerializeField, Min(0.01f)] private float pulseSpeed = 9f;
        [SerializeField, Min(0f)] private float pulseScale = 0.04f;
        [SerializeField, Min(0f)] private float interceptPunchSeconds = 0.14f;
        [SerializeField, Min(0f)] private float interceptPunchDistance = 0.18f;
        [SerializeField, Min(0f)] private float interceptPunchScale = 0.16f;

        [Header("VFX Cues")]
        [SerializeField] private CombatVfxCuePlayer cuePlayer;
        [SerializeField] private Transform vfxAnchor;
        [SerializeField] private Transform vfxDirectionTarget;
        [SerializeField] private CombatVfxCueId activationCueId = CombatVfxCueId.EliteShieldSignal;
        [SerializeField] private CombatVfxCueId interceptCueId = CombatVfxCueId.SummonBlockOpportunity;
        [SerializeField, Min(0f)] private float activationCueIntensity = 0.48f;
        [SerializeField, Min(0f)] private float interceptCueIntensity = 0.58f;
        [SerializeField, Min(0f)] private float tierCueIntensityStep = 0.08f;

        private MaterialPropertyBlock propertyBlock;
        private Vector3 visualBaseScale = Vector3.one;
        private Vector3 visualBaseLocalPosition;
        private Vector3 interceptPunchLocalDirection = Vector3.back;
        private float flashTimer;
        private float lingerTimer;
        private float interceptPunchTimer;
        private float lastKnownRadius = 1.35f;
        private int interceptFlashCount;
        private int activationVfxCueRequestCount;
        private int interceptVfxCueRequestCount;
        private int lastObservedTier = 1;
        private bool showing;
        private bool subscribed;

        public SummonPressureScreen PressureScreen => pressureScreen;
        public bool IsShowing => showing;
        public int RendererCount => screenRenderers != null ? screenRenderers.Length : 0;
        public int InterceptFlashCount => interceptFlashCount;
        public int ActivationVfxCueRequestCount => activationVfxCueRequestCount;
        public int InterceptVfxCueRequestCount => interceptVfxCueRequestCount;
        public int LastObservedTier => lastObservedTier;
        public float VisualRadiusScale => visualRadiusScale;
        public bool RenderVisuals => renderVisuals;

        private void Awake()
        {
            propertyBlock ??= new MaterialPropertyBlock();
            if (pressureScreen == null)
                pressureScreen = GetComponentInParent<SummonPressureScreen>();

            if (visualRoot == null)
                visualRoot = transform;

            visualBaseScale = visualRoot.localScale;
            visualBaseLocalPosition = visualRoot.localPosition;
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

            if (interceptPunchTimer > 0f)
                interceptPunchTimer = Mathf.Max(0f, interceptPunchTimer - deltaTime);

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
            visualBaseLocalPosition = visualRoot.localPosition;
            screenRenderers = newScreenRenderers ?? System.Array.Empty<Renderer>();
            Subscribe();
            RefreshNow();
        }

        public void ConfigureVfxCuePlayer(
            CombatVfxCuePlayer newCuePlayer,
            Transform newVfxAnchor,
            Transform newVfxDirectionTarget)
        {
            cuePlayer = newCuePlayer;
            vfxAnchor = newVfxAnchor;
            vfxDirectionTarget = newVfxDirectionTarget;
        }

        public void RefreshNow()
        {
            if (pressureScreen != null && pressureScreen.IsActive)
            {
                lastKnownRadius = pressureScreen.ActiveRadius;
                lastObservedTier = pressureScreen.ActiveTier;
                SetShowing(true);
            }
            else if (lingerTimer <= 0f)
            {
                SetShowing(false);
            }

            RefreshVisual();
        }

        public void DismissImmediately()
        {
            flashTimer = 0f;
            lingerTimer = 0f;
            interceptPunchTimer = 0f;
            if (visualRoot != null && visualRoot != transform)
            {
                visualRoot.localPosition = visualBaseLocalPosition;
            }

            SetShowing(false);
        }

        private void OnScreenActivated(SummonPressureScreen screen)
        {
            lastKnownRadius = screen.ActiveRadius;
            lastObservedTier = screen.ActiveTier;
            flashTimer = Mathf.Max(flashTimer, activationFlashSeconds);
            lingerTimer = 0f;
            interceptPunchTimer = 0f;
            SetShowing(true);
            if (PlayVfxCue(activationCueId, screen.ActiveTier, activationCueIntensity))
            {
                activationVfxCueRequestCount++;
            }

            RefreshVisual();
        }

        private void OnScreenIntercepted(SummonPressureScreen screen, BossBarrageProjectile projectile)
        {
            HandleInterceptedProjectile(screen, projectile != null ? projectile.transform : null);
        }

        private void OnActionProjectileIntercepted(SummonPressureScreen screen, LaneActionProjectile projectile)
        {
            HandleInterceptedProjectile(screen, projectile != null ? projectile.transform : null);
        }

        private void OnSkillBeamIntercepted(SummonPressureScreen screen)
        {
            HandleInterceptedProjectile(screen, null);
        }

        private void HandleInterceptedProjectile(SummonPressureScreen screen, Transform projectileTransform)
        {
            lastKnownRadius = screen.ActiveRadius;
            lastObservedTier = screen.ActiveTier;
            flashTimer = Mathf.Max(flashTimer, interceptFlashSeconds);
            lingerTimer = Mathf.Max(lingerTimer, finalHitLingerSeconds);
            interceptPunchTimer = Mathf.Max(interceptPunchTimer, interceptPunchSeconds);
            interceptPunchLocalDirection = ResolveInterceptPunchLocalDirection(projectileTransform);
            interceptFlashCount++;
            SetShowing(true);
            if (PlayVfxCue(interceptCueId, screen.ActiveTier, interceptCueIntensity, projectileTransform))
            {
                interceptVfxCueRequestCount++;
            }

            RefreshVisual();
        }

        private void OnScreenDeactivated(SummonPressureScreen screen)
        {
            if (lingerTimer <= 0f)
                SetShowing(false);
        }

        private void RefreshVisual()
        {
            if (!showing || visualRoot == null || !renderVisuals)
            {
                return;
            }

            float radius = pressureScreen != null && pressureScreen.IsActive
                ? pressureScreen.ActiveRadius
                : lastKnownRadius;
            lastKnownRadius = Mathf.Max(0.05f, radius);

            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseScale;
            float flash = ResolveFlashWeight();
            float punch = ResolvePunchWeight();
            float radiusScale = Mathf.Clamp(visualRadiusScale, 0.18f, 1f);
            float scale = Mathf.Max(0.05f, lastKnownRadius)
                * radiusScale
                * 2f
                * (pulse + flash * 0.12f + punch * interceptPunchScale);
            visualRoot.localScale = Vector3.Scale(visualBaseScale, new Vector3(scale, scale, scale));
            if (visualRoot != transform)
            {
                visualRoot.localPosition = visualBaseLocalPosition
                    + interceptPunchLocalDirection * (interceptPunchDistance * punch);
            }

            Color color = Color.Lerp(ResolveTierColor(lastObservedTier), interceptColor, flash);
            if (pressureScreen == null || !pressureScreen.IsActive)
            {
                float lingerAlpha = finalHitLingerSeconds > 0f ? Mathf.Clamp01(lingerTimer / finalHitLingerSeconds) : 0f;
                color.a *= lingerAlpha;
            }

            ApplyColor(color);
        }

        private Color ResolveTierColor(int tier)
        {
            return Mathf.Clamp(tier, 1, 3) switch
            {
                2 => tierTwoColor,
                3 => tierThreeColor,
                _ => activeColor
            };
        }

        private float ResolveFlashWeight()
        {
            float longestFlash = Mathf.Max(activationFlashSeconds, interceptFlashSeconds);
            return longestFlash > 0f ? Mathf.Clamp01(flashTimer / longestFlash) : 0f;
        }

        private float ResolvePunchWeight()
        {
            if (interceptPunchSeconds <= 0f)
            {
                return 0f;
            }

            return Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(interceptPunchTimer / interceptPunchSeconds));
        }

        private Vector3 ResolveInterceptPunchLocalDirection(Transform projectileTransform)
        {
            Vector3 worldDirection = Vector3.zero;
            if (pressureScreen != null && projectileTransform != null)
            {
                worldDirection = Vector3.ProjectOnPlane(
                    pressureScreen.transform.position - projectileTransform.position,
                    Vector3.up);
            }

            if (worldDirection.sqrMagnitude <= 0.0001f)
            {
                Transform directionSource = pressureScreen != null ? pressureScreen.transform : transform;
                worldDirection = -Vector3.ProjectOnPlane(directionSource.forward, Vector3.up);
            }

            if (worldDirection.sqrMagnitude <= 0.0001f)
            {
                worldDirection = Vector3.back;
            }

            Transform parent = visualRoot != null ? visualRoot.parent : null;
            Vector3 localDirection = parent != null
                ? parent.InverseTransformDirection(worldDirection.normalized)
                : worldDirection.normalized;
            return localDirection.sqrMagnitude > 0.0001f ? localDirection.normalized : Vector3.back;
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

        private bool PlayVfxCue(
            CombatVfxCueId cueId,
            int tier,
            float baseIntensity,
            Transform directionSource = null)
        {
            CombatVfxCuePlayer resolvedCuePlayer = ResolveCuePlayer();
            if (resolvedCuePlayer == null)
            {
                return false;
            }

            Transform anchor = vfxAnchor != null ? vfxAnchor : (pressureScreen != null ? pressureScreen.transform : transform);
            float intensity = baseIntensity + Mathf.Max(0, tier - 1) * tierCueIntensityStep;
            return resolvedCuePlayer.PlayCue(cueId, anchor, ResolveVfxDirection(anchor, directionSource), intensity);
        }

        private CombatVfxCuePlayer ResolveCuePlayer()
        {
            if (cuePlayer != null)
            {
                return cuePlayer;
            }

            cuePlayer = GetComponent<CombatVfxCuePlayer>();
            return cuePlayer;
        }

        private Vector3 ResolveVfxDirection(Transform anchor, Transform directionSource)
        {
            if (anchor != null && directionSource != null)
            {
                Vector3 impactDirection = Vector3.ProjectOnPlane(anchor.position - directionSource.position, Vector3.up);
                if (impactDirection.sqrMagnitude > 0.0001f)
                {
                    return impactDirection.normalized;
                }
            }

            if (anchor != null && vfxDirectionTarget != null)
            {
                Vector3 targetDirection = Vector3.ProjectOnPlane(vfxDirectionTarget.position - anchor.position, Vector3.up);
                if (targetDirection.sqrMagnitude > 0.0001f)
                {
                    return targetDirection.normalized;
                }
            }

            if (anchor != null)
            {
                Vector3 forward = Vector3.ProjectOnPlane(anchor.forward, Vector3.up);
                if (forward.sqrMagnitude > 0.0001f)
                {
                    return forward.normalized;
                }
            }

            return Vector3.forward;
        }

        private void SetShowing(bool value)
        {
            showing = value;
            bool shouldRender = value && renderVisuals;
            if (visualRoot != null
                && visualRoot != transform
                && visualRoot.gameObject != gameObject
                && visualRoot.gameObject.activeSelf != shouldRender)
            {
                visualRoot.gameObject.SetActive(shouldRender);
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
            pressureScreen.ActionProjectileIntercepted += OnActionProjectileIntercepted;
            pressureScreen.SkillBeamIntercepted += OnSkillBeamIntercepted;
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
            pressureScreen.ActionProjectileIntercepted -= OnActionProjectileIntercepted;
            pressureScreen.SkillBeamIntercepted -= OnSkillBeamIntercepted;
            pressureScreen.Deactivated -= OnScreenDeactivated;
            subscribed = false;
        }

    }
}
