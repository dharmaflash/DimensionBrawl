using System.Collections.Generic;
using UnityEngine;

namespace DimensionBrawl.Combat
{
    public enum BossLaserSummonPatternState
    {
        Inactive = 0,
        WaitingForAdvance = 1,
        Telegraph = 2,
        Active = 3,
        Recovery = 4,
        Reposition = 5
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(SummonFrontlineProxy))]
    public sealed class BossLaserSummonPattern : MonoBehaviour
    {
        [SerializeField] private SummonFrontlineProxy proxy;
        [SerializeField] private CombatHealth sourceHealth;
        [SerializeField] private Transform target;
        [SerializeField] private CombatHealth targetHealth;
        [SerializeField] private Transform laserOrigin;
        [SerializeField] private LayerMask targetLayers = Physics.DefaultRaycastLayers;

        [Header("Cadence")]
        [SerializeField, Min(0.05f)] private float telegraphSeconds = 0.78f;
        [SerializeField, Min(0f)] private float aimLockSeconds = 0.2f;
        [SerializeField, Min(0.05f)] private float activeSeconds = 0.92f;
        [SerializeField, Min(0f)] private float recoverySeconds = 0.42f;
        [SerializeField, Min(0.05f)] private float repositionSeconds = 0.62f;

        [Header("Laser")]
        [SerializeField, Min(0.1f)] private float laserLength = 22f;
        [SerializeField, Min(0.01f)] private float hitRadius = 0.62f;
        [SerializeField, Min(0f)] private float targetHeightOffset = 1.05f;
        [SerializeField, Min(0f)] private float damagePerSecond = 58f;
        [SerializeField, Min(0.05f)] private float damageIntervalSeconds = 0.12f;
        [SerializeField, Min(0f)] private float tierDamageBonus = 0.12f;

        [Header("Movement")]
        [SerializeField, Min(0f)] private float desiredDistanceFromTarget = 4.2f;
        [SerializeField, Min(0f)] private float strafeDistance = 1.45f;
        [SerializeField, Min(0f)] private float repositionMoveSpeed = 3.8f;

        [Header("Presentation")]
        [SerializeField] private LineRenderer telegraphLine;
        [SerializeField] private Color telegraphStartColor = new Color(0.1f, 0.95f, 1f, 0.12f);
        [SerializeField] private Color telegraphEndColor = new Color(0.1f, 0.95f, 1f, 0.72f);
        [SerializeField] private Color activeColor = new Color(0.85f, 1f, 1f, 0.95f);
        [SerializeField, Min(0.001f)] private float telegraphStartWidth = 0.025f;
        [SerializeField, Min(0.001f)] private float telegraphEndWidth = 0.075f;
        [SerializeField, Min(0.001f)] private float activeWidth = 0.16f;

        private readonly Collider[] hitBuffer = new Collider[32];
        private readonly List<CombatHealth> uniqueTargets = new List<CombatHealth>(8);
        private BossLaserSummonPatternState state;
        private DamageTeam sourceTeam = DamageTeam.Enemy;
        private Vector3 lockedDirection = Vector3.back;
        private float stateTimer;
        private float nextDamageTime;
        private int cycleIndex;
        private int totalDamageTickCount;
        private Material runtimeLineMaterial;

        public BossLaserSummonPatternState CurrentState => state;
        public bool IsLaserActive => state == BossLaserSummonPatternState.Active;
        public int TotalDamageTickCount => totalDamageTickCount;
        public float TelegraphProgress01 => state == BossLaserSummonPatternState.Telegraph
            ? Mathf.Clamp01(stateTimer / Mathf.Max(0.05f, telegraphSeconds))
            : 0f;

        private void Awake()
        {
            ResolveReferences();
            EnsureTelegraphLine();
            HideLine();
        }

        private void OnEnable()
        {
            ResolveReferences();
            EnsureTelegraphLine();
            EnterInactive();
        }

        private void OnDisable()
        {
            HideLine();
            state = BossLaserSummonPatternState.Inactive;
        }

        private void OnDestroy()
        {
            if (runtimeLineMaterial != null)
            {
                Destroy(runtimeLineMaterial);
                runtimeLineMaterial = null;
            }
        }

        private void Update()
        {
            ResolveReferences();
            if (proxy == null || !proxy.IsActive)
            {
                EnterInactive();
                return;
            }

            float deltaTime = Time.deltaTime;
            if (deltaTime <= 0f)
            {
                return;
            }

            switch (state)
            {
                case BossLaserSummonPatternState.Inactive:
                    EnterWaitingForAdvance();
                    break;
                case BossLaserSummonPatternState.WaitingForAdvance:
                    UpdateWaitingForAdvance();
                    break;
                case BossLaserSummonPatternState.Telegraph:
                    UpdateTelegraph(deltaTime);
                    break;
                case BossLaserSummonPatternState.Active:
                    UpdateActive(deltaTime);
                    break;
                case BossLaserSummonPatternState.Recovery:
                    UpdateRecovery(deltaTime);
                    break;
                case BossLaserSummonPatternState.Reposition:
                    UpdateReposition(deltaTime);
                    break;
            }
        }

        public void ConfigurePattern(
            Transform newTarget,
            DamageTeam newSourceTeam,
            float newDamagePerSecond,
            float newDamageIntervalSeconds,
            float newMoveSpeed)
        {
            target = newTarget;
            targetHealth = target != null ? target.GetComponentInParent<CombatHealth>() : null;
            sourceTeam = newSourceTeam;
            damagePerSecond = Mathf.Max(0f, newDamagePerSecond);
            damageIntervalSeconds = Mathf.Max(0.05f, newDamageIntervalSeconds);
            if (newMoveSpeed > 0f)
            {
                repositionMoveSpeed = newMoveSpeed;
            }
        }

        private void UpdateWaitingForAdvance()
        {
            HideLine();
            if (proxy.IsAdvancing || proxy.AdvanceProgress01 < 0.96f)
            {
                return;
            }

            EnterTelegraph();
        }

        private void UpdateTelegraph(float deltaTime)
        {
            stateTimer += deltaTime;
            proxy.RequestAdvanceHold(0.08f);

            float lockStartTime = Mathf.Max(0f, telegraphSeconds - aimLockSeconds);
            if (stateTimer <= lockStartTime)
            {
                lockedDirection = ResolveTargetDirection();
            }

            FaceLockedDirection();
            float progress = Mathf.Clamp01(stateTimer / Mathf.Max(0.05f, telegraphSeconds));
            ShowLine(
                Color.Lerp(telegraphStartColor, telegraphEndColor, progress),
                Mathf.Lerp(telegraphStartWidth, telegraphEndWidth, progress));

            if (stateTimer >= telegraphSeconds)
            {
                EnterActive();
            }
        }

        private void UpdateActive(float deltaTime)
        {
            stateTimer += deltaTime;
            proxy.RequestAdvanceHold(0.08f);
            FaceLockedDirection();
            ShowLine(activeColor, activeWidth);

            if (Time.time >= nextDamageTime)
            {
                ApplyLaserDamage(Mathf.Max(0.05f, damageIntervalSeconds));
                nextDamageTime = Time.time + Mathf.Max(0.05f, damageIntervalSeconds);
            }

            if (stateTimer >= activeSeconds)
            {
                EnterRecovery();
            }
        }

        private void UpdateRecovery(float deltaTime)
        {
            stateTimer += deltaTime;
            proxy.RequestAdvanceHold(0.08f);
            HideLine();
            if (stateTimer >= recoverySeconds)
            {
                EnterReposition();
            }
        }

        private void UpdateReposition(float deltaTime)
        {
            stateTimer += deltaTime;
            HideLine();
            if (!proxy.IsAdvancing || stateTimer >= repositionSeconds + 0.18f)
            {
                EnterTelegraph();
            }
        }

        private void EnterInactive()
        {
            if (state == BossLaserSummonPatternState.Inactive)
            {
                HideLine();
                return;
            }

            state = BossLaserSummonPatternState.Inactive;
            stateTimer = 0f;
            HideLine();
        }

        private void EnterWaitingForAdvance()
        {
            state = BossLaserSummonPatternState.WaitingForAdvance;
            stateTimer = 0f;
            HideLine();
        }

        private void EnterTelegraph()
        {
            state = BossLaserSummonPatternState.Telegraph;
            stateTimer = 0f;
            lockedDirection = ResolveTargetDirection();
            FaceLockedDirection();
            ShowLine(telegraphStartColor, telegraphStartWidth);
        }

        private void EnterActive()
        {
            state = BossLaserSummonPatternState.Active;
            stateTimer = 0f;
            nextDamageTime = 0f;
            proxy.NotifyAttackPerformed(activeSeconds + 0.05f);
            ShowLine(activeColor, activeWidth);
        }

        private void EnterRecovery()
        {
            state = BossLaserSummonPatternState.Recovery;
            stateTimer = 0f;
            HideLine();
        }

        private void EnterReposition()
        {
            state = BossLaserSummonPatternState.Reposition;
            stateTimer = 0f;
            cycleIndex++;
            proxy.BeginAdvanceTo(
                ResolveRepositionTarget(),
                repositionSeconds,
                repositionMoveSpeed);
        }

        private void ApplyLaserDamage(float interval)
        {
            Vector3 origin = ResolveLaserOrigin();
            Vector3 end = origin + lockedDirection * Mathf.Max(0.1f, laserLength);
            int hitCount = Physics.OverlapCapsuleNonAlloc(
                origin,
                end,
                Mathf.Max(0.01f, hitRadius),
                hitBuffer,
                targetLayers,
                QueryTriggerInteraction.Collide);

            uniqueTargets.Clear();
            for (int i = 0; i < hitCount; i++)
            {
                Collider hit = hitBuffer[i];
                hitBuffer[i] = null;
                if (hit == null)
                {
                    continue;
                }

                CombatHealth health = hit.GetComponentInParent<CombatHealth>();
                if (health == null
                    || health == sourceHealth
                    || !health.IsAlive
                    || !CombatTeamUtility.AreHostile(sourceTeam, health.Team)
                    || uniqueTargets.Contains(health))
                {
                    continue;
                }

                uniqueTargets.Add(health);
                Vector3 point = ClosestPointOnSegment(origin, end, health.transform.position);
                float tierScale = 1f + Mathf.Max(0, proxy.ActiveTier - 1) * tierDamageBonus;
                DamageInfo damageInfo = new DamageInfo(
                    sourceHealth,
                    sourceTeam,
                    damagePerSecond * interval * tierScale,
                    point,
                    lockedDirection,
                    0f,
                    DamageResponsePolicy.FlashOnly,
                    CombatControlLockPolicy.None);
                if (health.TryApplyDamage(damageInfo))
                {
                    totalDamageTickCount++;
                }
            }
        }

        private Vector3 ResolveRepositionTarget()
        {
            Vector3 currentPosition = transform.position;
            Vector3 targetPosition = ResolveTargetPoint();
            Vector3 away = Vector3.ProjectOnPlane(currentPosition - targetPosition, Vector3.up);
            if (away.sqrMagnitude <= 0.0001f)
            {
                away = -Vector3.ProjectOnPlane(transform.forward, Vector3.up);
            }

            if (away.sqrMagnitude <= 0.0001f)
            {
                away = Vector3.back;
            }

            away.Normalize();
            Vector3 right = Vector3.Cross(Vector3.up, away).normalized;
            float side = cycleIndex % 2 == 0 ? 1f : -1f;
            Vector3 desired = targetPosition
                + away * Mathf.Max(0f, desiredDistanceFromTarget)
                + right * (side * Mathf.Max(0f, strafeDistance));
            desired.y = currentPosition.y;
            return desired;
        }

        private Vector3 ResolveTargetPoint()
        {
            if (target != null)
            {
                return target.position + Vector3.up * Mathf.Max(0f, targetHeightOffset);
            }

            if (targetHealth != null)
            {
                return targetHealth.transform.position + Vector3.up * Mathf.Max(0f, targetHeightOffset);
            }

            return transform.position + transform.forward * Mathf.Max(1f, desiredDistanceFromTarget);
        }

        private Vector3 ResolveTargetDirection()
        {
            Vector3 origin = ResolveLaserOrigin();
            Vector3 direction = ResolveTargetPoint() - origin;
            if (direction.sqrMagnitude <= 0.0001f)
            {
                direction = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
            }

            if (direction.sqrMagnitude <= 0.0001f)
            {
                direction = Vector3.back;
            }

            return direction.normalized;
        }

        private Vector3 ResolveLaserOrigin()
        {
            Transform origin = laserOrigin != null
                ? laserOrigin
                : proxy != null
                    ? proxy.ProjectileOrigin
                    : transform;
            return origin.position;
        }

        private void FaceLockedDirection()
        {
            Vector3 planarDirection = Vector3.ProjectOnPlane(lockedDirection, Vector3.up);
            if (planarDirection.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            transform.rotation = Quaternion.LookRotation(planarDirection.normalized, Vector3.up);
        }

        private void ShowLine(Color color, float width)
        {
            EnsureTelegraphLine();
            if (telegraphLine == null)
            {
                return;
            }

            Vector3 origin = ResolveLaserOrigin();
            Vector3 end = origin + lockedDirection * Mathf.Max(0.1f, laserLength);
            telegraphLine.gameObject.SetActive(true);
            telegraphLine.positionCount = 2;
            telegraphLine.SetPosition(0, origin);
            telegraphLine.SetPosition(1, end);
            telegraphLine.startColor = color;
            telegraphLine.endColor = color;
            telegraphLine.startWidth = width;
            telegraphLine.endWidth = width;
        }

        private void HideLine()
        {
            if (telegraphLine != null)
            {
                telegraphLine.gameObject.SetActive(false);
            }
        }

        private void EnsureTelegraphLine()
        {
            if (telegraphLine != null)
            {
                return;
            }

            GameObject lineObject = new GameObject("BossLaserTelegraphLine");
            lineObject.transform.SetParent(transform, worldPositionStays: false);
            telegraphLine = lineObject.AddComponent<LineRenderer>();
            telegraphLine.useWorldSpace = true;
            telegraphLine.textureMode = LineTextureMode.Stretch;
            telegraphLine.numCapVertices = 3;
            telegraphLine.numCornerVertices = 2;
            telegraphLine.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            telegraphLine.receiveShadows = false;
            Shader shader = Shader.Find("Sprites/Default") ?? Shader.Find("Universal Render Pipeline/Unlit");
            if (shader != null)
            {
                runtimeLineMaterial = new Material(shader)
                {
                    name = "BossLaserTelegraphLine_Runtime"
                };
                telegraphLine.sharedMaterial = runtimeLineMaterial;
            }
        }

        private void ResolveReferences()
        {
            if (proxy == null)
            {
                proxy = GetComponent<SummonFrontlineProxy>();
            }

            if (sourceHealth == null)
            {
                sourceHealth = proxy != null ? proxy.Health : GetComponent<CombatHealth>();
            }

            if (laserOrigin == null && proxy != null)
            {
                laserOrigin = proxy.ProjectileOrigin;
            }

            if (targetHealth == null && target != null)
            {
                targetHealth = target.GetComponentInParent<CombatHealth>();
            }
        }

        private static Vector3 ClosestPointOnSegment(Vector3 start, Vector3 end, Vector3 point)
        {
            Vector3 segment = end - start;
            float lengthSqr = segment.sqrMagnitude;
            if (lengthSqr <= 0.0001f)
            {
                return start;
            }

            float t = Vector3.Dot(point - start, segment) / lengthSqr;
            return start + segment * Mathf.Clamp01(t);
        }
    }
}
