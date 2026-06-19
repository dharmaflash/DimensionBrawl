using UnityEngine;

namespace DimensionBrawl.Combat
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SummonFrontlineProxy))]
    public sealed class SummonFrontlineClash : MonoBehaviour
    {
        [SerializeField] private SummonFrontlineProxy proxy;
        [SerializeField] private CombatHealth health;
        [SerializeField, Min(0f)] private float contactDamagePerSecond = 32f;
        [SerializeField, Min(0.05f)] private float contactDamageIntervalSeconds = 0.35f;
        [SerializeField, Min(0f)] private float tierDamageBonus = 0.16f;
        [SerializeField, Min(0f)] private float clashHoldSeconds = 0.24f;
        [SerializeField, Min(0f)] private float clashFeedbackSeconds = 0.28f;
        [SerializeField, Min(0.05f)] private float engageRadius = 0.95f;
        [SerializeField, Min(0f)] private float engageCenterHeight = 0.9f;
        [SerializeField] private LayerMask contactLayers = Physics.DefaultRaycastLayers;

        private readonly Collider[] contactBuffer = new Collider[12];
        private float nextDamageTime;
        private float clashFeedbackTimer;
        private int totalClashCount;
        private int lastOpponentTier;
        private DamageTeam lastOpponentTeam = DamageTeam.Neutral;
        private float lastDamageAmount;

        public bool IsClashing => proxy != null && proxy.IsActive && clashFeedbackTimer > 0f;
        public int TotalClashCount => totalClashCount;
        public int LastOpponentTier => lastOpponentTier;
        public DamageTeam LastOpponentTeam => lastOpponentTeam;
        public float LastDamageAmount => lastDamageAmount;
        public float EngageRadius => engageRadius;

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            nextDamageTime = 0f;
            clashFeedbackTimer = 0f;
        }

        private void Update()
        {
            Tick(Time.deltaTime);
        }

        public void ConfigureReferences(SummonFrontlineProxy newProxy, CombatHealth newHealth)
        {
            proxy = newProxy;
            health = newHealth;
        }

        public void ConfigureTuning(
            float damagePerSecond,
            float damageIntervalSeconds,
            float damageTierBonus,
            float holdSeconds)
        {
            ConfigureTuning(
                damagePerSecond,
                damageIntervalSeconds,
                damageTierBonus,
                holdSeconds,
                engageRadius);
        }

        public void ConfigureTuning(
            float damagePerSecond,
            float damageIntervalSeconds,
            float damageTierBonus,
            float holdSeconds,
            float newEngageRadius)
        {
            contactDamagePerSecond = Mathf.Max(0f, damagePerSecond);
            contactDamageIntervalSeconds = Mathf.Max(0.05f, damageIntervalSeconds);
            tierDamageBonus = Mathf.Max(0f, damageTierBonus);
            clashHoldSeconds = Mathf.Max(0f, holdSeconds);
            engageRadius = Mathf.Max(0.05f, newEngageRadius);
        }

        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f)
            {
                return;
            }

            ScanNearbyContacts();
            if (clashFeedbackTimer <= 0f)
            {
                return;
            }

            clashFeedbackTimer = Mathf.Max(0f, clashFeedbackTimer - deltaTime);
        }

        public bool TryProcessClash(Collider other)
        {
            ResolveReferences();
            if (other == null
                || proxy == null
                || health == null
                || !proxy.IsActive
                || !health.IsAlive)
            {
                return false;
            }

            if (other.GetComponentInParent<SummonPressureScreen>() != null)
            {
                return false;
            }

            SummonFrontlineProxy otherProxy = other.GetComponentInParent<SummonFrontlineProxy>();
            if (otherProxy == proxy || (otherProxy != null && !otherProxy.IsActive))
            {
                return false;
            }

            CombatHealth otherHealth = otherProxy != null
                ? otherProxy.Health ?? other.GetComponentInParent<CombatHealth>()
                : other.GetComponentInParent<CombatHealth>();
            if (otherHealth == null
                || otherHealth == health
                || !otherHealth.IsAlive
                || !CombatTeamUtility.AreHostile(health.Team, otherHealth.Team))
            {
                return false;
            }

            proxy.RequestAdvanceHold(clashHoldSeconds);
            if (otherProxy != null)
            {
                otherProxy.RequestAdvanceHold(clashHoldSeconds);
            }

            clashFeedbackTimer = Mathf.Max(clashFeedbackTimer, clashFeedbackSeconds);

            if (Time.time < nextDamageTime)
            {
                return true;
            }

            float interval = Mathf.Max(0.05f, contactDamageIntervalSeconds);
            float damageAmount = ResolveDamageAmount(interval);
            var damageInfo = new DamageInfo(
                health,
                health.Team,
                damageAmount,
                other.ClosestPoint(transform.position),
                ResolveHitDirection(otherHealth, otherProxy),
                0f);

            if (otherHealth.TryApplyDamage(damageInfo))
            {
                totalClashCount++;
                lastOpponentTier = otherProxy != null ? otherProxy.ActiveTier : 0;
                lastOpponentTeam = otherHealth.Team;
                lastDamageAmount = damageAmount;
                proxy.NotifyAttackPerformed(clashFeedbackSeconds);
                otherProxy?.NotifyAttackPerformed(clashFeedbackSeconds);
            }

            nextDamageTime = Time.time + interval;
            return true;
        }

        private void ScanNearbyContacts()
        {
            ResolveReferences();
            if (proxy == null || health == null || !proxy.IsActive || !health.IsAlive)
            {
                return;
            }

            Vector3 center = transform.position + Vector3.up * engageCenterHeight;
            int count = Physics.OverlapSphereNonAlloc(
                center,
                engageRadius,
                contactBuffer,
                contactLayers,
                QueryTriggerInteraction.Collide);
            for (int i = 0; i < count; i++)
            {
                Collider candidate = contactBuffer[i];
                if (candidate != null)
                {
                    TryProcessClash(candidate);
                    contactBuffer[i] = null;
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            TryProcessClash(other);
        }

        private void OnTriggerStay(Collider other)
        {
            TryProcessClash(other);
        }

        private float ResolveDamageAmount(float interval)
        {
            int tier = proxy != null ? Mathf.Clamp(proxy.ActiveTier, 1, 3) : 1;
            float tierScale = 1f + (tier - 1) * tierDamageBonus;
            return contactDamagePerSecond * interval * tierScale;
        }

        private Vector3 ResolveHitDirection(CombatHealth otherHealth, SummonFrontlineProxy otherProxy)
        {
            Transform targetTransform = otherProxy != null
                ? otherProxy.transform
                : otherHealth != null
                    ? otherHealth.transform
                    : null;
            return targetTransform != null
                ? targetTransform.position - transform.position
                : transform.forward;
        }

        private void ResolveReferences()
        {
            if (proxy == null)
            {
                proxy = GetComponent<SummonFrontlineProxy>();
            }

            if (health == null)
            {
                health = proxy != null ? proxy.Health : GetComponent<CombatHealth>();
            }
        }
    }
}
