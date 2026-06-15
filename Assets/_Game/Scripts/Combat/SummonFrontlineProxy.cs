using UnityEngine;

namespace DimensionBrawl.Combat
{
    [DisallowMultipleComponent]
    public sealed class SummonFrontlineProxy : MonoBehaviour
    {
        [SerializeField] private Transform projectileOrigin;
        [SerializeField] private SummonPressureScreen pressureScreen;
        [SerializeField] private bool faceTargetOnActivate = true;

        private Vector3 baseScale = Vector3.one;
        private float remainingLifetime;
        private bool active;
        private int activeTier;

        public bool IsActive => active && gameObject.activeInHierarchy;
        public int ActiveTier => activeTier;
        public Transform ProjectileOrigin => projectileOrigin != null ? projectileOrigin : transform;
        public SummonPressureScreen PressureScreen => pressureScreen;

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
            activeTier = Mathf.Clamp(tier, 1, 3);
            remainingLifetime = Mathf.Max(0.05f, lifetimeSeconds);
            transform.position = position;
            transform.localScale = baseScale * Mathf.Max(0.01f, scaleMultiplier);

            Vector3 planarDirection = ResolvePlanarDirection(facingDirection);
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
            gameObject.SetActive(false);
        }

        private void Update()
        {
            Tick(Time.deltaTime);
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
