using DimensionBrawl.Combat;
using UnityEngine;
using UnityEngine.Serialization;

namespace DimensionBrawl.Presentation
{
    [DisallowMultipleComponent]
    public sealed class SummonProxyVisualMotionPresenter : MonoBehaviour
    {
        [SerializeField] private SummonFrontlineProxy proxy;
        [SerializeField] private Transform motionRoot;
        [SerializeField, Min(0f)] private float airborneHeight;
        [SerializeField, Min(0f)] private float jumpArcHeight;
        [SerializeField, Min(0f)] private float tierArcHeightStep;
        [SerializeField, Range(0f, 1f)] private float arcStartProgress;
        [SerializeField, Range(0f, 1f)] private float arcEndProgress = 0.92f;
        [SerializeField, Min(0f)] private float landingSettleSeconds = 0.12f;
        [SerializeField, Min(0f)] private float landingDip = 0.08f;
        [FormerlySerializedAs("jumpVfxRoot")]
        [SerializeField] private Transform movementVfxRoot;
        [FormerlySerializedAs("jumpVfxParticles")]
        [SerializeField] private ParticleSystem[] movementVfxParticles = System.Array.Empty<ParticleSystem>();

        private Vector3 baseLocalPosition;
        private bool hasBasePose;
        private bool wasArcAirborne;
        private bool movementVfxVisible;
        private float landingTimer;

        public SummonFrontlineProxy Proxy => proxy;
        public Transform MotionRoot => motionRoot;
        public Transform MovementVfxRoot => movementVfxRoot;
        public float AirborneHeight => airborneHeight;
        public float JumpArcHeight => jumpArcHeight;
        public float TierArcHeightStep => tierArcHeightStep;
        public int MovementVfxParticleCount => movementVfxParticles != null ? movementVfxParticles.Length : 0;

        private void Awake()
        {
            ResolveReferences();
            CaptureBasePose();
        }

        private void OnEnable()
        {
            ResolveReferences();
            CaptureBasePose();
            wasArcAirborne = false;
            landingTimer = 0f;
            ApplyMotion(0f);
        }

        private void OnDisable()
        {
            SetMovementVfxVisible(false);
            ResetMotion();
        }

        private void LateUpdate()
        {
            ApplyMotion(Time.deltaTime);
        }

        private void ApplyMotion(float deltaTime)
        {
            ResolveReferences();
            CaptureBasePose();
            if (motionRoot == null)
            {
                SetMovementVfxVisible(false);
                return;
            }

            bool visible = proxy != null && proxy.IsPresentationVisible;
            if (!visible)
            {
                wasArcAirborne = false;
                landingTimer = 0f;
                SetMovementVfxVisible(false);
                ResetMotion();
                return;
            }

            float arcHeight = ResolveArcHeight();
            bool airborne = arcHeight > 0.001f;
            SetMovementVfxVisible(ShouldShowMovementVfx(airborne));
            if (wasArcAirborne && !airborne && landingSettleSeconds > 0f)
            {
                landingTimer = Mathf.Max(landingTimer, landingSettleSeconds);
            }

            wasArcAirborne = airborne;
            float landingOffset = ResolveLandingOffset(deltaTime);
            motionRoot.localPosition = baseLocalPosition + Vector3.up * (airborneHeight + arcHeight + landingOffset);
        }

        private bool ShouldShowMovementVfx(bool airborne)
        {
            if (proxy == null || !proxy.IsActive || !proxy.IsAdvancing)
            {
                return false;
            }

            if (jumpArcHeight > 0.001f)
            {
                return airborne;
            }

            return true;
        }

        private float ResolveArcHeight()
        {
            if (proxy == null || !proxy.IsActive || !proxy.IsAdvancing || jumpArcHeight <= 0f)
            {
                return 0f;
            }

            float endProgress = Mathf.Max(arcStartProgress + 0.01f, arcEndProgress);
            float progress01 = Mathf.InverseLerp(arcStartProgress, endProgress, proxy.AdvanceProgress01);
            if (progress01 <= 0f || progress01 >= 1f)
            {
                return 0f;
            }

            float tierBonus = Mathf.Max(0, proxy.ActiveTier - 1) * tierArcHeightStep;
            return Mathf.Sin(progress01 * Mathf.PI) * (jumpArcHeight + tierBonus);
        }

        private float ResolveLandingOffset(float deltaTime)
        {
            if (landingTimer <= 0f || landingSettleSeconds <= 0f || landingDip <= 0f)
            {
                landingTimer = 0f;
                return 0f;
            }

            float settle01 = Mathf.Clamp01(landingTimer / landingSettleSeconds);
            landingTimer = Mathf.Max(0f, landingTimer - Mathf.Max(0f, deltaTime));
            return -landingDip * settle01;
        }

        private void ResolveReferences()
        {
            if (proxy == null)
            {
                proxy = GetComponent<SummonFrontlineProxy>();
            }

            if (motionRoot == null)
            {
                motionRoot = transform;
            }

            if (movementVfxRoot != null && (movementVfxParticles == null || movementVfxParticles.Length == 0))
            {
                movementVfxParticles = movementVfxRoot.GetComponentsInChildren<ParticleSystem>(includeInactive: true);
            }
        }

        private void CaptureBasePose()
        {
            if (hasBasePose || motionRoot == null)
            {
                return;
            }

            baseLocalPosition = motionRoot.localPosition;
            hasBasePose = true;
        }

        private void ResetMotion()
        {
            if (motionRoot != null && hasBasePose)
            {
                motionRoot.localPosition = baseLocalPosition;
            }
        }

        private void SetMovementVfxVisible(bool visible)
        {
            if (movementVfxRoot == null)
            {
                movementVfxVisible = false;
                return;
            }

            bool isActive = movementVfxRoot.gameObject.activeSelf;
            if (movementVfxVisible == visible && isActive == visible)
            {
                return;
            }

            movementVfxVisible = visible;
            if (isActive != visible)
            {
                movementVfxRoot.gameObject.SetActive(visible);
            }

            if (movementVfxParticles == null || movementVfxParticles.Length == 0)
            {
                movementVfxParticles = movementVfxRoot.GetComponentsInChildren<ParticleSystem>(includeInactive: true);
            }

            if (movementVfxParticles == null)
            {
                return;
            }

            for (int i = 0; i < movementVfxParticles.Length; i++)
            {
                ParticleSystem particle = movementVfxParticles[i];
                if (particle == null)
                {
                    continue;
                }

                if (visible)
                {
                    particle.Clear(withChildren: true);
                    particle.Play(withChildren: true);
                }
                else
                {
                    particle.Stop(
                        withChildren: true,
                        ParticleSystemStopBehavior.StopEmittingAndClear);
                }
            }
        }
    }
}
