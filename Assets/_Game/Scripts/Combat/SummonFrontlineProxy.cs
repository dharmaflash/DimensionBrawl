using UnityEngine;

namespace DimensionBrawl.Combat
{
    [DisallowMultipleComponent]
    public sealed class SummonFrontlineProxy : MonoBehaviour
    {
        [SerializeField] private Transform projectileOrigin;
        [SerializeField] private SummonPressureScreen pressureScreen;
        [SerializeField] private bool faceTargetOnActivate = true;
        [SerializeField, Min(0f)] private float defaultAdvanceDistance = 0f;
        [SerializeField, Min(0.01f)] private float defaultAdvanceSeconds = 0.25f;

        private Vector3 baseScale = Vector3.one;
        private float remainingLifetime;
        private Vector3 advanceStartPosition;
        private Vector3 advanceTargetPosition;
        private float advanceSeconds = 0.25f;
        private float advanceElapsed;
        private bool active;
        private int activeTier;

        public bool IsActive => active && gameObject.activeInHierarchy;
        public int ActiveTier => activeTier;
        public Transform ProjectileOrigin => projectileOrigin != null ? projectileOrigin : transform;
        public SummonPressureScreen PressureScreen => pressureScreen;
        public Vector3 AdvanceStartPosition => advanceStartPosition;
        public Vector3 AdvanceTargetPosition => advanceTargetPosition;
        public float AdvanceProgress01 => advanceSeconds > 0f ? Mathf.Clamp01(advanceElapsed / advanceSeconds) : 1f;
        public bool IsAdvancing => IsActive
            && AdvanceProgress01 < 1f
            && (advanceTargetPosition - advanceStartPosition).sqrMagnitude > 0.0001f;

        private void Awake()
        {
            baseScale = transform.localScale;
            if (pressureScreen == null)
            {
                pressureScreen = GetComponentInChildren<SummonPressureScreen>(includeInactive: true);
            }
        }

        public void ConfigurePresentation(Transform newProjectileOrigin, SummonPressureScreen newPressureScreen)
        {
            projectileOrigin = newProjectileOrigin;
            pressureScreen = newPressureScreen;
        }

        public void Activate(
            Vector3 position,
            Vector3 facingDirection,
            int tier,
            float lifetimeSeconds,
            float scaleMultiplier)
        {
            Activate(
                position,
                facingDirection,
                tier,
                lifetimeSeconds,
                scaleMultiplier,
                defaultAdvanceDistance,
                defaultAdvanceSeconds);
        }

        public void Activate(
            Vector3 position,
            Vector3 facingDirection,
            int tier,
            float lifetimeSeconds,
            float scaleMultiplier,
            float advanceDistance,
            float advanceDurationSeconds)
        {
            activeTier = Mathf.Clamp(tier, 1, 3);
            remainingLifetime = Mathf.Max(0.05f, lifetimeSeconds);
            transform.position = position;
            transform.localScale = baseScale * Mathf.Max(0.01f, scaleMultiplier);

            Vector3 planarDirection = ResolvePlanarDirection(facingDirection);
            float safeAdvanceDistance = Mathf.Max(0f, advanceDistance);
            advanceSeconds = Mathf.Max(0.01f, advanceDurationSeconds);
            advanceElapsed = 0f;
            advanceStartPosition = position;
            advanceTargetPosition = position + planarDirection * safeAdvanceDistance;

            if (faceTargetOnActivate && planarDirection.sqrMagnitude > 0.0001f)
            {
                transform.rotation = Quaternion.LookRotation(planarDirection, Vector3.up);
            }

            active = true;
            gameObject.SetActive(true);
        }

        public void Tick(float deltaTime)
        {
            if (!active || deltaTime <= 0f)
            {
                return;
            }

            Advance(deltaTime);
            remainingLifetime -= deltaTime;
            if (remainingLifetime <= 0f)
            {
                Deactivate();
            }
        }

        public void Deactivate()
        {
            if (pressureScreen != null)
            {
                pressureScreen.Deactivate();
            }

            active = false;
            remainingLifetime = 0f;
            advanceElapsed = advanceSeconds;
            gameObject.SetActive(false);
        }

        private void Update()
        {
            Tick(Time.deltaTime);
        }

        private void Advance(float deltaTime)
        {
            if (advanceElapsed >= advanceSeconds || advanceStartPosition == advanceTargetPosition)
            {
                return;
            }

            advanceElapsed = Mathf.Min(advanceSeconds, advanceElapsed + deltaTime);
            float t = Mathf.SmoothStep(0f, 1f, AdvanceProgress01);
            transform.position = Vector3.LerpUnclamped(advanceStartPosition, advanceTargetPosition, t);
        }

        private static Vector3 ResolvePlanarDirection(Vector3 direction)
        {
            Vector3 planarDirection = Vector3.ProjectOnPlane(direction, Vector3.up);
            if (planarDirection.sqrMagnitude > 0.0001f)
            {
                return planarDirection.normalized;
            }

            return Vector3.forward;
        }
    }
}
