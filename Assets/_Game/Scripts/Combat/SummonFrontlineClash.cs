using UnityEngine;

namespace DimensionBrawl.Combat
{
    public enum SummonFrontlineClashTargetKind
    {
        None = 0,
        HostileSummon = 1,
        HostileBody = 2
    }

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
        [SerializeField, Range(0f, 1f)] private float playerBodyDamageMultiplier = 0.12f;
        [SerializeField, Min(0f)] private float playerBodyMaxDamagePerHit = 4f;
        [SerializeField, Range(0f, 1f)] private float hostileBodyDamageMultiplier = 1f;
        [SerializeField, Min(0f)] private float hostileBodyMaxDamagePerHit;
        [SerializeField] private LayerMask contactLayers = Physics.DefaultRaycastLayers;
        [SerializeField] private bool prioritizeHostileSummons = true;

        [Header("Contact Damage VFX")]
        [SerializeField] private GameObject contactDamageVfxPrefab;
        [SerializeField, Min(0.01f)] private float contactDamageVfxScale = 0.52f;
        [SerializeField, Min(0f)] private float contactDamageVfxHeightOffset = 0.55f;
        [SerializeField, Min(0.05f)] private float contactDamageVfxLifetimeSeconds = 0.72f;

        private readonly Collider[] contactBuffer = new Collider[12];
        private float nextDamageTime;
        private float clashFeedbackTimer;
        private int totalClashCount;
        private int lastOpponentTier;
        private DamageTeam lastOpponentTeam = DamageTeam.Neutral;
        private float lastDamageAmount;
        private SummonFrontlineClashTargetKind lastTargetKind = SummonFrontlineClashTargetKind.None;

        public bool IsClashing => proxy != null && proxy.IsActive && clashFeedbackTimer > 0f;
        public int TotalClashCount => totalClashCount;
        public int LastOpponentTier => lastOpponentTier;
        public DamageTeam LastOpponentTeam => lastOpponentTeam;
        public float LastDamageAmount => lastDamageAmount;
        public SummonFrontlineClashTargetKind LastTargetKind => lastTargetKind;
        public float ContactDamagePerSecond => contactDamagePerSecond;
        public float ContactDamageIntervalSeconds => contactDamageIntervalSeconds;
        public float PlayerBodyDamageMultiplier => playerBodyDamageMultiplier;
        public float PlayerBodyMaxDamagePerHit => playerBodyMaxDamagePerHit;
        public float HostileBodyDamageMultiplier => hostileBodyDamageMultiplier;
        public float HostileBodyMaxDamagePerHit => hostileBodyMaxDamagePerHit;
        public float EngageRadius => engageRadius;

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnValidate()
        {
            hostileBodyDamageMultiplier = Mathf.Clamp01(hostileBodyDamageMultiplier);
            hostileBodyMaxDamagePerHit = Mathf.Max(0f, hostileBodyMaxDamagePerHit);
        }

        private void OnEnable()
        {
            nextDamageTime = 0f;
            clashFeedbackTimer = 0f;
        }

        private void Update()
        {
            Tick(Time.deltaTime * CombatTimeDilationReceiver.ResolveTimeScale(this));
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

        public void ConfigurePlayerBodyDamage(float damageMultiplier, float maxDamagePerHit)
        {
            playerBodyDamageMultiplier = Mathf.Clamp01(damageMultiplier);
            playerBodyMaxDamagePerHit = Mathf.Max(0f, maxDamagePerHit);
        }

        public void ConfigureHostileBodyDamage(float damageMultiplier, float maxDamagePerHit)
        {
            hostileBodyDamageMultiplier = Mathf.Clamp01(damageMultiplier);
            hostileBodyMaxDamagePerHit = Mathf.Max(0f, maxDamagePerHit);
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

            if (!TryResolveClashTarget(
                    other,
                    out SummonFrontlineProxy otherProxy,
                    out CombatHealth otherHealth,
                    out SummonFrontlineClashTargetKind targetKind))
            {
                return false;
            }

            proxy.RequestAdvanceHold(clashHoldSeconds);
            proxy.FaceTowards(otherHealth.transform.position);
            if (otherProxy != null)
            {
                otherProxy.RequestAdvanceHold(clashHoldSeconds);
                otherProxy.FaceTowards(transform.position);
            }

            clashFeedbackTimer = Mathf.Max(clashFeedbackTimer, clashFeedbackSeconds);

            if (Time.time < nextDamageTime)
            {
                return true;
            }

            float interval = Mathf.Max(0.05f, contactDamageIntervalSeconds);
            float damageAmount = ResolveDamageAmount(interval, targetKind, otherHealth);
            Vector3 hitPoint = other.ClosestPoint(transform.position);
            var damageInfo = new DamageInfo(
                health,
                health.Team,
                damageAmount,
                hitPoint,
                ResolveHitDirection(otherHealth, otherProxy),
                0f,
                ResolveResponsePolicy(targetKind, otherHealth),
                ResolveControlLockPolicy(targetKind, otherHealth));

            if (otherHealth.TryApplyDamage(damageInfo))
            {
                totalClashCount++;
                lastOpponentTier = otherProxy != null ? otherProxy.ActiveTier : 0;
                lastOpponentTeam = otherHealth.Team;
                lastDamageAmount = damageAmount;
                lastTargetKind = targetKind;
                proxy.NotifyAttackPerformed(clashFeedbackSeconds);
                otherProxy?.NotifyAttackPerformed(clashFeedbackSeconds);
                SpawnContactDamageVfx(hitPoint, damageInfo.Direction);
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

            Collider bestCandidate = null;
            int bestPriority = int.MaxValue;
            float bestDistanceSqr = float.PositiveInfinity;
            for (int i = 0; i < count; i++)
            {
                Collider candidate = contactBuffer[i];
                if (candidate != null
                    && TryResolveClashTarget(
                        candidate,
                        out _,
                        out _,
                        out SummonFrontlineClashTargetKind targetKind))
                {
                    int priority = ResolveTargetPriority(targetKind);
                    float distanceSqr = (candidate.transform.position - transform.position).sqrMagnitude;
                    if (priority < bestPriority
                        || (priority == bestPriority && distanceSqr < bestDistanceSqr))
                    {
                        bestCandidate = candidate;
                        bestPriority = priority;
                        bestDistanceSqr = distanceSqr;
                    }
                }

                contactBuffer[i] = null;
            }

            if (bestCandidate != null)
            {
                TryProcessClash(bestCandidate);
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

        private float ResolveDamageAmount(
            float interval,
            SummonFrontlineClashTargetKind targetKind,
            CombatHealth targetHealth)
        {
            int tier = proxy != null ? Mathf.Clamp(proxy.ActiveTier, 1, 3) : 1;
            float tierScale = 1f + (tier - 1) * tierDamageBonus;
            float amount = contactDamagePerSecond * interval * tierScale;
            if (targetKind == SummonFrontlineClashTargetKind.HostileBody && IsPlayerBody(targetHealth))
            {
                amount *= Mathf.Clamp01(playerBodyDamageMultiplier);
                if (playerBodyMaxDamagePerHit > 0f)
                {
                    amount = Mathf.Min(amount, playerBodyMaxDamagePerHit);
                }
            }
            else if (targetKind == SummonFrontlineClashTargetKind.HostileBody)
            {
                amount *= Mathf.Clamp01(hostileBodyDamageMultiplier);
                if (hostileBodyMaxDamagePerHit > 0f)
                {
                    amount = Mathf.Min(amount, hostileBodyMaxDamagePerHit);
                }
            }

            return amount;
        }

        private static DamageResponsePolicy ResolveResponsePolicy(
            SummonFrontlineClashTargetKind targetKind,
            CombatHealth targetHealth)
        {
            return targetKind == SummonFrontlineClashTargetKind.HostileSummon || IsPlayerBody(targetHealth)
                ? DamageResponsePolicy.FlashOnly
                : DamageResponsePolicy.Default;
        }

        private static CombatControlLockPolicy ResolveControlLockPolicy(
            SummonFrontlineClashTargetKind targetKind,
            CombatHealth targetHealth)
        {
            return targetKind == SummonFrontlineClashTargetKind.HostileSummon || IsPlayerBody(targetHealth)
                ? CombatControlLockPolicy.None
                : CombatControlLockPolicy.InterruptAction;
        }

        private static bool IsPlayerBody(CombatHealth targetHealth)
        {
            return targetHealth != null
                && (targetHealth.GetComponentInParent<DimensionBrawl.Player.PlayerMovementController>() != null
                    || targetHealth.GetComponentInParent<IsekaiBrawl.Gameplay.PlayerController>() != null);
        }

        private void SpawnContactDamageVfx(Vector3 hitPoint, Vector3 direction)
        {
            if (contactDamageVfxPrefab == null)
            {
                return;
            }

            Vector3 spawnPoint = hitPoint + Vector3.up * contactDamageVfxHeightOffset;
            Quaternion rotation = ResolveContactDamageVfxRotation(direction);
            GameObject instance = Instantiate(contactDamageVfxPrefab, spawnPoint, rotation);
            instance.transform.localScale *= Mathf.Max(0.01f, contactDamageVfxScale);
            Destroy(instance, Mathf.Max(0.05f, contactDamageVfxLifetimeSeconds));
        }

        private Quaternion ResolveContactDamageVfxRotation(Vector3 direction)
        {
            Vector3 planarDirection = Vector3.ProjectOnPlane(direction, Vector3.up);
            if (planarDirection.sqrMagnitude <= 0.0001f)
            {
                planarDirection = transform.forward;
            }

            return Quaternion.LookRotation(planarDirection.normalized, Vector3.up);
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

        private bool TryResolveClashTarget(
            Collider other,
            out SummonFrontlineProxy otherProxy,
            out CombatHealth otherHealth,
            out SummonFrontlineClashTargetKind targetKind)
        {
            otherProxy = null;
            otherHealth = null;
            targetKind = SummonFrontlineClashTargetKind.None;

            if (other == null || other.GetComponentInParent<SummonPressureScreen>() != null)
            {
                return false;
            }

            otherProxy = other.GetComponentInParent<SummonFrontlineProxy>();
            if (otherProxy == proxy || (otherProxy != null && !otherProxy.IsActive))
            {
                return false;
            }

            otherHealth = otherProxy != null
                ? otherProxy.Health ?? other.GetComponentInParent<CombatHealth>()
                : other.GetComponentInParent<CombatHealth>();
            if (otherHealth == null
                || otherHealth == health
                || !otherHealth.IsAlive
                || !CombatTeamUtility.AreHostile(health.Team, otherHealth.Team))
            {
                return false;
            }

            targetKind = otherProxy != null
                ? SummonFrontlineClashTargetKind.HostileSummon
                : SummonFrontlineClashTargetKind.HostileBody;
            return true;
        }

        private int ResolveTargetPriority(SummonFrontlineClashTargetKind targetKind)
        {
            if (!prioritizeHostileSummons)
            {
                return targetKind == SummonFrontlineClashTargetKind.None ? 99 : 0;
            }

            return targetKind switch
            {
                SummonFrontlineClashTargetKind.HostileSummon => 0,
                SummonFrontlineClashTargetKind.HostileBody => 1,
                _ => 99
            };
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
