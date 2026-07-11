using System.Collections;
using DimensionBrawl.Combat;
using UnityEngine;

namespace DimensionBrawl.Presentation
{
    public enum CombatHitFeedbackTier
    {
        Light,
        Heavy,
        Critical,
        Break
    }

    public sealed class CombatHitFeedback : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
        private static readonly int EmissionColorLdrId = Shader.PropertyToID("_EmissionColorLDR");
        private static readonly int EmissionColorHdrId = Shader.PropertyToID("_EmissionColorHDR");
        private static readonly int EmissionStrengthId = Shader.PropertyToID("_EmissionStrength");
        private static readonly int UseEmissionId = Shader.PropertyToID("_UseEmission");
        private const float MinimumVisibleFlashSeconds = 0.2f;
        private const float MinimumHitColorBlend = 0.96f;
        private const float MinimumHitEmissionBoost = 3.25f;
        private const float MinimumRecoilReturnSeconds = 0.035f;

        [Header("References")]
        [SerializeField] private CombatHealth health;
        [SerializeField] private Renderer[] flashRenderers;
        [SerializeField] private ActionCameraController cameraController;
        [SerializeField] private Transform visualRecoilRoot;

        [Header("Flash")]
        [SerializeField] private bool renderHitFeedback;
        [SerializeField] private bool applyIdleColorOnEnable;
        [SerializeField] private Color idleColor = new Color(0.3f, 0.85f, 1f);
        [Tooltip("First-pass visible feedback value. The combat timing source only gives stagger/hit-stop ranges, so this remains Inspector-visible.")]
        [SerializeField, Min(0f)] private float flashSeconds = 0.12f;
        [SerializeField] private Color hitColor = new Color(1f, 0.36f, 0.18f, 1f);
        [SerializeField] private Color hitEmissionColor = new Color(1f, 0.7f, 0.28f, 1f);
        [SerializeField, Range(0f, 1f)] private float hitColorBlend = 0.72f;
        [SerializeField, Min(0f)] private float hitEmissionBoost = 1.35f;
        [SerializeField] private Color deathColor = new Color(0.15f, 0.15f, 0.15f);

        [Header("Impact")]
        [SerializeField] private bool playCameraImpulse = true;
        [SerializeField, Min(0f)] private float cameraImpulseCooldownSeconds = 0.07f;
        [SerializeField, Min(0f)] private float lightCameraPlanarKick = 0.035f;
        [SerializeField, Min(0f)] private float heavyCameraPlanarKick = 0.095f;
        [SerializeField, Min(0f)] private float lightCameraVerticalKick = 0.012f;
        [SerializeField, Min(0f)] private float heavyCameraVerticalKick = 0.04f;
        [SerializeField, Min(0f)] private float lightCameraCueSeconds = 0.075f;
        [SerializeField, Min(0f)] private float heavyCameraCueSeconds = 0.125f;
        [SerializeField] private float lightCameraFieldOfViewDelta = -0.25f;
        [SerializeField] private float heavyCameraFieldOfViewDelta = -0.85f;
        [SerializeField] private float lightCameraDistanceDelta = 0.015f;
        [SerializeField] private float heavyCameraDistanceDelta = 0.075f;
        [SerializeField] private float heavyCameraFocusHeightDelta = 0.035f;
        [SerializeField] private bool playCameraMicroShake = true;

        [Header("Reaction")]
        [SerializeField] private bool playVisualRecoil = true;
        [SerializeField, Min(0f)] private float visualRecoilCooldownSeconds = 0.045f;
        [SerializeField, Min(0f)] private float lightRecoilDistance = 0.018f;
        [SerializeField, Min(0f)] private float heavyRecoilDistance = 0.06f;
        [SerializeField, Min(0f)] private float lightRecoilDegrees = 1.4f;
        [SerializeField, Min(0f)] private float heavyRecoilDegrees = 4.5f;
        [SerializeField, Min(0f)] private float recoilReturnSeconds = 0.095f;

        [Header("Hit Stop")]
        [SerializeField] private bool playHitStop = true;
        [SerializeField, Range(0.02f, 1f)] private float hitStopTimeScale = 0.18f;
        [SerializeField, Min(0f)] private float hitStopCooldownSeconds = 0.055f;
        [SerializeField, Range(0f, 1f)] private float lightHitStopScale = 0.72f;
        [SerializeField, Range(0f, 2f)] private float heavyHitStopScale = 1.1f;

        [Header("Tiering")]
        [SerializeField, Range(0.01f, 1f)] private float heavyDamageHealthRatio = 0.12f;
        [SerializeField, Range(0.01f, 1f)] private float criticalHealthRatio = 0.35f;

        private MaterialPropertyBlock propertyBlock;
        private Coroutine flashRoutine;
        private Coroutine recoilRoutine;
        private static CombatHitFeedback activeHitStopOwner;
        private static Coroutine activeHitStopRoutine;
        private static float activeHitStopRestoreTimeScale = 1f;
        private int damageFlashCount;
        private int cameraImpulseRequestCount;
        private int visualRecoilRequestCount;
        private int hitStopRequestCount;
        private float nextCameraImpulseTime;
        private float nextVisualRecoilTime;
        private float nextHitStopTime;
        private Transform activeRecoilRoot;
        private Vector3 recoilBaseLocalPosition;
        private Quaternion recoilBaseLocalRotation;
        private bool recoilBaseCaptured;
        private CombatHitFeedbackTier lastHitFeedbackTier = CombatHitFeedbackTier.Light;

        public bool RenderHitFeedback => renderHitFeedback;
        public int FlashRendererCount => flashRenderers != null ? flashRenderers.Length : 0;
        public int DamageFlashCount => damageFlashCount;
        public int CameraImpulseRequestCount => cameraImpulseRequestCount;
        public int VisualRecoilRequestCount => visualRecoilRequestCount;
        public int HitStopRequestCount => hitStopRequestCount;
        public CombatHitFeedbackTier LastHitFeedbackTier => lastHitFeedbackTier;

        private void Awake()
        {
            if (health == null)
            {
                health = GetComponent<CombatHealth>();
            }

            propertyBlock = new MaterialPropertyBlock();
        }

        private void OnEnable()
        {
            if (health == null || !renderHitFeedback)
            {
                return;
            }

            health.Damaged += HandleDamaged;
            health.Died += HandleDied;

            if (applyIdleColorOnEnable)
            {
                ApplyFlatColor(idleColor, Color.black);
            }
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.Damaged -= HandleDamaged;
                health.Died -= HandleDied;
            }

            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
                flashRoutine = null;
            }

            if (recoilRoutine != null)
            {
                StopCoroutine(recoilRoutine);
                recoilRoutine = null;
            }

            RestoreVisualRecoilRoot();

            if (activeHitStopOwner == this)
            {
                if (activeHitStopRoutine != null)
                {
                    StopCoroutine(activeHitStopRoutine);
                }

                Time.timeScale = activeHitStopRestoreTimeScale;
                activeHitStopOwner = null;
                activeHitStopRoutine = null;
            }
        }

        private void HandleDamaged(DamageInfo damageInfo)
        {
            if (!renderHitFeedback)
            {
                return;
            }

            if (!DamageResponsePolicyUtility.PlaysDamagePresentation(damageInfo.ResponsePolicy))
            {
                return;
            }

            damageFlashCount++;
            lastHitFeedbackTier = ResolveHitFeedbackTier(damageInfo);
            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
            }

            Color resolvedHitColor = ResolveTierHitColor(lastHitFeedbackTier);
            Color resolvedEmissionColor = ResolveTierEmissionColor(lastHitFeedbackTier);
            float resolvedFlashSeconds = ResolveTierFlashSeconds(lastHitFeedbackTier);
            ApplyFlash(resolvedHitColor, resolvedEmissionColor, 1f);
            flashRoutine = StartCoroutine(Flash(resolvedHitColor, resolvedEmissionColor, resolvedFlashSeconds, clearAfter: true));

            RequestCameraImpulse(damageInfo, lastHitFeedbackTier);
            RequestVisualRecoil(damageInfo, lastHitFeedbackTier);
            RequestHitStop(damageInfo, lastHitFeedbackTier);
        }

        private void HandleDied()
        {
            if (!renderHitFeedback)
            {
                return;
            }

            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
            }

            ApplyFlatColor(deathColor, Color.black);
        }

        private IEnumerator Flash(Color color, Color emissionColor, float seconds, bool clearAfter)
        {
            float duration = Mathf.Max(MinimumVisibleFlashSeconds, seconds);
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float normalized = Mathf.Clamp01(1f - elapsed / duration);
                float weight = Mathf.SmoothStep(0f, 1f, normalized);
                ApplyFlash(color, emissionColor, weight);
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            if (clearAfter)
            {
                if (applyIdleColorOnEnable)
                {
                    ApplyFlatColor(idleColor, Color.black);
                }
                else
                {
                    ClearColor();
                }
            }

            flashRoutine = null;
        }

        private void ApplyFlash(Color color, Color emissionColor, float weight)
        {
            if (flashRenderers == null)
            {
                return;
            }

            for (int i = 0; i < flashRenderers.Length; i++)
            {
                Renderer targetRenderer = flashRenderers[i];
                if (targetRenderer == null)
                {
                    continue;
                }

                propertyBlock ??= new MaterialPropertyBlock();
                targetRenderer.GetPropertyBlock(propertyBlock);
                Color baseColor = ResolveRendererBaseColor(targetRenderer);
                Color visibleHitColor = Color.Lerp(Color.white, color, 0.5f);
                Color flashColor = Color.Lerp(
                    baseColor,
                    visibleHitColor,
                    Mathf.Clamp01(Mathf.Max(hitColorBlend, MinimumHitColorBlend) * weight));
                flashColor.a = baseColor.a;
                Color boostedEmission = emissionColor * (Mathf.Max(hitEmissionBoost, MinimumHitEmissionBoost) * weight);
                boostedEmission.a = 1f;

                propertyBlock.SetColor(BaseColorId, flashColor);
                propertyBlock.SetColor(ColorId, flashColor);
                propertyBlock.SetColor(EmissionColorId, boostedEmission);
                propertyBlock.SetColor(EmissionColorLdrId, boostedEmission);
                propertyBlock.SetColor(EmissionColorHdrId, boostedEmission);
                propertyBlock.SetFloat(EmissionStrengthId, Mathf.Max(hitEmissionBoost, MinimumHitEmissionBoost) * weight);
                propertyBlock.SetFloat(UseEmissionId, weight > 0f ? 1f : 0f);
                targetRenderer.SetPropertyBlock(propertyBlock);
            }
        }

        private void ApplyFlatColor(Color color, Color emissionColor)
        {
            if (flashRenderers == null)
            {
                return;
            }

            for (int i = 0; i < flashRenderers.Length; i++)
            {
                Renderer targetRenderer = flashRenderers[i];
                if (targetRenderer == null)
                {
                    continue;
                }

                propertyBlock ??= new MaterialPropertyBlock();
                targetRenderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor(BaseColorId, color);
                propertyBlock.SetColor(ColorId, color);
                propertyBlock.SetColor(EmissionColorId, emissionColor);
                propertyBlock.SetColor(EmissionColorLdrId, emissionColor);
                propertyBlock.SetColor(EmissionColorHdrId, emissionColor);
                propertyBlock.SetFloat(EmissionStrengthId, 0f);
                propertyBlock.SetFloat(UseEmissionId, emissionColor.maxColorComponent > 0f ? 1f : 0f);
                targetRenderer.SetPropertyBlock(propertyBlock);
            }
        }

        private static Color ResolveRendererBaseColor(Renderer targetRenderer)
        {
            Material[] materials = targetRenderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
            {
                Material material = materials[i];
                if (material == null)
                {
                    continue;
                }

                if (material.HasProperty(BaseColorId))
                {
                    return material.GetColor(BaseColorId);
                }

                if (material.HasProperty(ColorId))
                {
                    return material.GetColor(ColorId);
                }
            }

            return Color.white;
        }

        private void ClearColor()
        {
            if (flashRenderers == null)
            {
                return;
            }

            for (int i = 0; i < flashRenderers.Length; i++)
            {
                if (flashRenderers[i] != null)
                {
                    flashRenderers[i].SetPropertyBlock(null);
                }
            }
        }

        private CombatHitFeedbackTier ResolveHitFeedbackTier(DamageInfo damageInfo)
        {
            if (damageInfo.ResponsePolicy == DamageResponsePolicy.Break
                || damageInfo.ResponsePolicy == DamageResponsePolicy.Knockdown)
            {
                return CombatHitFeedbackTier.Break;
            }

            if (health != null && health.MaxHealth > 0f && health.HealthRatio <= criticalHealthRatio)
            {
                return CombatHitFeedbackTier.Critical;
            }

            float damageRatio = health != null && health.MaxHealth > 0f
                ? damageInfo.Amount / health.MaxHealth
                : 0f;
            bool heavyResponse = damageInfo.ResponsePolicy == DamageResponsePolicy.Stagger
                || DamageResponsePolicyUtility.PlaysFullBodyHitAnimation(damageInfo);
            if (heavyResponse || damageRatio >= heavyDamageHealthRatio)
            {
                return CombatHitFeedbackTier.Heavy;
            }

            return CombatHitFeedbackTier.Light;
        }

        private Color ResolveTierHitColor(CombatHitFeedbackTier tier)
        {
            switch (tier)
            {
                case CombatHitFeedbackTier.Break:
                    return Color.Lerp(hitColor, Color.white, 0.58f);
                case CombatHitFeedbackTier.Critical:
                    return Color.Lerp(hitColor, new Color(1f, 0.05f, 0.04f, 1f), 0.5f);
                case CombatHitFeedbackTier.Heavy:
                    return Color.Lerp(hitColor, Color.white, 0.32f);
                default:
                    return hitColor;
            }
        }

        private Color ResolveTierEmissionColor(CombatHitFeedbackTier tier)
        {
            switch (tier)
            {
                case CombatHitFeedbackTier.Break:
                    return Color.Lerp(hitEmissionColor, Color.white, 0.65f);
                case CombatHitFeedbackTier.Critical:
                    return Color.Lerp(hitEmissionColor, new Color(1f, 0.08f, 0.02f, 1f), 0.5f);
                case CombatHitFeedbackTier.Heavy:
                    return Color.Lerp(hitEmissionColor, Color.white, 0.26f);
                default:
                    return hitEmissionColor;
            }
        }

        private float ResolveTierFlashSeconds(CombatHitFeedbackTier tier)
        {
            switch (tier)
            {
                case CombatHitFeedbackTier.Break:
                    return flashSeconds * 1.55f;
                case CombatHitFeedbackTier.Critical:
                    return flashSeconds * 1.38f;
                case CombatHitFeedbackTier.Heavy:
                    return flashSeconds * 1.22f;
                default:
                    return flashSeconds;
            }
        }

        private void RequestCameraImpulse(DamageInfo damageInfo, CombatHitFeedbackTier tier)
        {
            if (!playCameraImpulse || Time.unscaledTime < nextCameraImpulseTime)
            {
                return;
            }

            ActionCameraController resolvedCameraController = ResolveCameraController();
            if (resolvedCameraController == null)
            {
                return;
            }

            float tierWeight = ResolveTierWeight(tier);
            if (playCameraMicroShake)
            {
                resolvedCameraController.RequestDamageHitFeedback(damageInfo.Direction, tierWeight);
            }
            else
            {
                Vector3 planarDirection = ResolvePlanarDirection(damageInfo.Direction);
                resolvedCameraController.RequestCue(
                    -planarDirection * Mathf.Lerp(lightCameraPlanarKick, heavyCameraPlanarKick, tierWeight)
                        + Vector3.up * Mathf.Lerp(lightCameraVerticalKick, heavyCameraVerticalKick, tierWeight),
                    Mathf.Lerp(lightCameraCueSeconds, heavyCameraCueSeconds, tierWeight),
                    Mathf.Lerp(lightCameraFieldOfViewDelta, heavyCameraFieldOfViewDelta, tierWeight),
                    Mathf.Lerp(lightCameraDistanceDelta, heavyCameraDistanceDelta, tierWeight),
                    Mathf.Lerp(0f, heavyCameraFocusHeightDelta, tierWeight));
            }

            cameraImpulseRequestCount++;
            nextCameraImpulseTime = Time.unscaledTime + cameraImpulseCooldownSeconds;
        }

        private ActionCameraController ResolveCameraController()
        {
            if (cameraController == null)
            {
                cameraController = ActionCameraController.ActiveInstance;
            }

            return cameraController;
        }

        private void RequestVisualRecoil(DamageInfo damageInfo, CombatHitFeedbackTier tier)
        {
            if (!playVisualRecoil || Time.unscaledTime < nextVisualRecoilTime)
            {
                return;
            }

            Transform recoilRoot = ResolveVisualRecoilRoot();
            if (recoilRoot == null)
            {
                return;
            }

            if (recoilRoutine != null)
            {
                StopCoroutine(recoilRoutine);
                RestoreVisualRecoilRoot();
            }

            CaptureVisualRecoilRoot(recoilRoot);
            Vector3 localDirection = ResolveLocalRecoilDirection(recoilRoot, damageInfo.Direction);
            float tierWeight = ResolveTierWeight(tier);
            float distance = Mathf.Lerp(lightRecoilDistance, heavyRecoilDistance, tierWeight);
            float degrees = Mathf.Lerp(lightRecoilDegrees, heavyRecoilDegrees, tierWeight);
            Vector3 targetPosition = recoilBaseLocalPosition + localDirection * distance;
            Quaternion targetRotation = recoilBaseLocalRotation * Quaternion.Euler(
                -Mathf.Abs(localDirection.z) * degrees * 0.45f,
                localDirection.x * degrees,
                -localDirection.x * degrees * 0.35f);
            recoilRoot.localPosition = targetPosition;
            recoilRoot.localRotation = targetRotation;
            recoilRoutine = StartCoroutine(ReturnVisualRecoil(recoilRoot, recoilBaseLocalPosition, recoilBaseLocalRotation));
            visualRecoilRequestCount++;
            nextVisualRecoilTime = Time.unscaledTime + visualRecoilCooldownSeconds;
        }

        private Transform ResolveVisualRecoilRoot()
        {
            if (visualRecoilRoot != null)
            {
                return visualRecoilRoot;
            }

            if (flashRenderers == null)
            {
                return null;
            }

            for (int i = 0; i < flashRenderers.Length; i++)
            {
                if (flashRenderers[i] != null)
                {
                    return flashRenderers[i].transform;
                }
            }

            return null;
        }

        private void CaptureVisualRecoilRoot(Transform recoilRoot)
        {
            if (activeRecoilRoot == recoilRoot && recoilBaseCaptured)
            {
                return;
            }

            activeRecoilRoot = recoilRoot;
            recoilBaseLocalPosition = recoilRoot.localPosition;
            recoilBaseLocalRotation = recoilRoot.localRotation;
            recoilBaseCaptured = true;
        }

        private IEnumerator ReturnVisualRecoil(Transform recoilRoot, Vector3 baseLocalPosition, Quaternion baseLocalRotation)
        {
            float duration = Mathf.Max(MinimumRecoilReturnSeconds, recoilReturnSeconds);
            float elapsed = 0f;
            Vector3 startPosition = recoilRoot.localPosition;
            Quaternion startRotation = recoilRoot.localRotation;

            while (elapsed < duration && recoilRoot != null)
            {
                float normalized = Mathf.Clamp01(elapsed / duration);
                float weight = Mathf.SmoothStep(0f, 1f, normalized);
                recoilRoot.localPosition = Vector3.Lerp(startPosition, baseLocalPosition, weight);
                recoilRoot.localRotation = Quaternion.Slerp(startRotation, baseLocalRotation, weight);
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            if (recoilRoot != null)
            {
                recoilRoot.localPosition = baseLocalPosition;
                recoilRoot.localRotation = baseLocalRotation;
            }

            recoilRoutine = null;
        }

        private void RestoreVisualRecoilRoot()
        {
            if (!recoilBaseCaptured || activeRecoilRoot == null)
            {
                return;
            }

            activeRecoilRoot.localPosition = recoilBaseLocalPosition;
            activeRecoilRoot.localRotation = recoilBaseLocalRotation;
        }

        private Vector3 ResolveLocalRecoilDirection(Transform recoilRoot, Vector3 damageDirection)
        {
            Vector3 worldDirection = -ResolvePlanarDirection(damageDirection);
            if (recoilRoot.parent != null)
            {
                worldDirection = recoilRoot.parent.InverseTransformDirection(worldDirection);
            }

            worldDirection.y = 0f;
            if (worldDirection.sqrMagnitude <= 0.0001f)
            {
                return Vector3.back;
            }

            return worldDirection.normalized;
        }

        private void RequestHitStop(DamageInfo damageInfo, CombatHitFeedbackTier tier)
        {
            if (!playHitStop || damageInfo.HitStopSeconds <= 0f || Time.unscaledTime < nextHitStopTime)
            {
                return;
            }

            if (Time.timeScale < 0.95f)
            {
                return;
            }

            float tierWeight = ResolveTierWeight(tier);
            float durationScale = Mathf.Lerp(lightHitStopScale, heavyHitStopScale, tierWeight);
            float duration = damageInfo.HitStopSeconds * durationScale;
            if (duration <= 0f)
            {
                return;
            }

            if (activeHitStopRoutine != null && activeHitStopOwner != null)
            {
                activeHitStopOwner.StopCoroutine(activeHitStopRoutine);
                Time.timeScale = activeHitStopRestoreTimeScale;
            }

            activeHitStopOwner = this;
            activeHitStopRestoreTimeScale = Time.timeScale;
            activeHitStopRoutine = StartCoroutine(ApplyHitStop(duration, Mathf.Clamp(hitStopTimeScale, 0.02f, 1f)));
            hitStopRequestCount++;
            nextHitStopTime = Time.unscaledTime + hitStopCooldownSeconds;
        }

        private IEnumerator ApplyHitStop(float seconds, float timeScale)
        {
            Time.timeScale = timeScale;
            float elapsed = 0f;
            while (elapsed < seconds)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            if (activeHitStopOwner == this)
            {
                Time.timeScale = activeHitStopRestoreTimeScale;
                activeHitStopOwner = null;
                activeHitStopRoutine = null;
            }
        }

        private float ResolveTierWeight(CombatHitFeedbackTier tier)
        {
            switch (tier)
            {
                case CombatHitFeedbackTier.Break:
                    return 1f;
                case CombatHitFeedbackTier.Critical:
                    return 0.85f;
                case CombatHitFeedbackTier.Heavy:
                    return 0.65f;
                default:
                    return 0f;
            }
        }

        private Vector3 ResolvePlanarDirection(Vector3 direction)
        {
            Vector3 planarDirection = Vector3.ProjectOnPlane(direction, Vector3.up);
            if (planarDirection.sqrMagnitude > 0.0001f)
            {
                return planarDirection.normalized;
            }

            planarDirection = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
            if (planarDirection.sqrMagnitude > 0.0001f)
            {
                return planarDirection.normalized;
            }

            return Vector3.forward;
        }
    }
}
