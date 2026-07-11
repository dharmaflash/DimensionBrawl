using System.Collections;
using System.Collections.Generic;
using DimensionBrawl.Combat;
using DimensionBrawl.Player;
using UnityEngine;

namespace DimensionBrawl.Presentation
{
    [DisallowMultipleComponent]
    public sealed class PerfectDodgeTimeWarp : MonoBehaviour
    {
        private sealed class ReceiverContext
        {
            public CombatTimeDilationReceiver Receiver;
            public GameObject Owner;
            public CombatHealth Health;
            public BossBarrageProjectile BossProjectile;
            public LaneActionProjectile LaneProjectile;
            public BossLaserSummonPattern LaserPattern;
            public SummonFrontlineProxy LaserProxy;
            public bool IsHostileEmitter;
        }

        [SerializeField] private PlayerActionController actionController;
        [SerializeField, Range(0.02f, 1f)] private float timeScale = 0.18f;
        [SerializeField, Min(0.01f)] private float durationSeconds = 3f;
        [SerializeField, Min(0f)] private float blendOutSeconds = 0.42f;
        [SerializeField, Range(0.02f, 1f)] private float globalHitStopTimeScale = 0.08f;
        [SerializeField, Min(0f)] private float globalHitStopSeconds = 0.055f;
        [SerializeField, Min(0.1f)] private float radius = 42f;
        [SerializeField, Min(0f)] private float innerRadius = 18f;
        [SerializeField, Min(0.02f)] private float receiverRefreshIntervalSeconds = 0.08f;

        private readonly HashSet<int> affectedReceivers = new HashSet<int>();
        private readonly Dictionary<int, ReceiverContext> receiverContexts =
            new Dictionary<int, ReceiverContext>(32);
        private float previousTimeScale = 1f;
        private float appliedTimeScale = 1f;
        private float timer;
        private float hitStopTimer;
        private float receiverRefreshTimer;
        private bool ownsGlobalHitStop;
        private bool hasLastDamageInfo;
        private DamageInfo lastDamageInfo;
        private CombatHealth playerHealth;
        private int lastAffectedReceiverCount;
        private int receiverRefreshCount;
        private int receiverContextBuildCount;
        private Coroutine warpRoutine;

        private const float RestoreTolerance = 0.08f;

        public bool IsWarpActive => timer > 0f || ownsGlobalHitStop;
        public float TimeScale => timeScale;
        public float DurationSeconds => durationSeconds;
        public float BlendOutSeconds => blendOutSeconds;
        public float GlobalHitStopTimeScale => globalHitStopTimeScale;
        public float GlobalHitStopSeconds => globalHitStopSeconds;
        public float Radius => radius;
        public float InnerRadius => innerRadius;
        public float ReceiverRefreshIntervalSeconds => receiverRefreshIntervalSeconds;
        public int LastAffectedReceiverCount => lastAffectedReceiverCount;
        public int ReceiverRefreshCount => receiverRefreshCount;
        public int ReceiverContextCacheCount => receiverContexts.Count;
        public int ReceiverContextBuildCount => receiverContextBuildCount;

        private void Awake()
        {
            if (actionController == null)
            {
                actionController = GetComponent<PlayerActionController>();
            }

            playerHealth = actionController != null
                ? actionController.GetComponent<CombatHealth>()
                : GetComponent<CombatHealth>();
        }

        private void OnEnable()
        {
            if (actionController != null)
            {
                actionController.PerfectDodgeTriggered += HandlePerfectDodgeTriggered;
            }
        }

        private void OnDisable()
        {
            StopWarpRoutine();
            if (actionController != null)
            {
                actionController.PerfectDodgeTriggered -= HandlePerfectDodgeTriggered;
            }

            RestoreIfStillOwner();
            ownsGlobalHitStop = false;
            timer = 0f;
            hitStopTimer = 0f;
            receiverRefreshTimer = 0f;
            hasLastDamageInfo = false;
            receiverContexts.Clear();
        }

        private IEnumerator RefreshWarpUntilSettled()
        {
            yield return null;

            while (isActiveAndEnabled && IsWarpActive)
            {
                float deltaTime = Time.unscaledDeltaTime > 0f ? Time.unscaledDeltaTime : Time.deltaTime;
                if (timer > 0f)
                {
                    timer = Mathf.Max(0f, timer - deltaTime);
                    RefreshActiveReceivers(deltaTime);
                }

                if (ownsGlobalHitStop)
                {
                    hitStopTimer = Mathf.Max(0f, hitStopTimer - deltaTime);
                    if (hitStopTimer > 0f)
                    {
                        ApplyScale(appliedTimeScale);
                    }
                    else
                    {
                        RestoreIfStillOwner();
                        ownsGlobalHitStop = false;
                    }
                }

                if (!IsWarpActive)
                {
                    break;
                }

                yield return null;
            }

            warpRoutine = null;
        }

        private void HandlePerfectDodgeTriggered(DamageInfo damageInfo)
        {
            if (Time.timeScale <= 0.01f)
            {
                return;
            }

            timer = Mathf.Max(timer, durationSeconds + blendOutSeconds);
            lastDamageInfo = damageInfo;
            hasLastDamageInfo = true;
            receiverRefreshTimer = 0f;
            ApplyThreatTimeDilation(damageInfo);
            BeginGlobalHitStop();
            StartWarpRoutineIfNeeded();
        }

        private void StartWarpRoutineIfNeeded()
        {
            if (warpRoutine == null && Application.isPlaying && isActiveAndEnabled && IsWarpActive)
            {
                warpRoutine = StartCoroutine(RefreshWarpUntilSettled());
            }
        }

        private void StopWarpRoutine()
        {
            if (warpRoutine == null)
            {
                return;
            }

            StopCoroutine(warpRoutine);
            warpRoutine = null;
        }

        private void RefreshActiveReceivers(float deltaTime)
        {
            if (timer <= 0f)
            {
                return;
            }

            receiverRefreshTimer -= Mathf.Max(0f, deltaTime);
            if (receiverRefreshTimer > 0f)
            {
                return;
            }

            receiverRefreshTimer = Mathf.Max(0.02f, receiverRefreshIntervalSeconds);
            ApplyThreatTimeDilation(hasLastDamageInfo ? lastDamageInfo : default);
            receiverRefreshCount++;
        }

        private void ApplyThreatTimeDilation(DamageInfo damageInfo)
        {
            affectedReceivers.Clear();
            lastAffectedReceiverCount = 0;
            Vector3 origin = actionController != null ? actionController.transform.position : transform.position;
            DamageTeam playerTeam = playerHealth != null ? playerHealth.Team : DamageTeam.Player;

            if (damageInfo.Source != null
                && CombatTeamUtility.AreHostile(damageInfo.SourceTeam, playerTeam))
            {
                TryApplyReceiver(damageInfo.Source.gameObject, origin, 1f);
            }

            ApplyToRegisteredReceivers(origin, playerTeam);
            lastAffectedReceiverCount = affectedReceivers.Count;
        }

        private void ApplyToRegisteredReceivers(Vector3 origin, DamageTeam playerTeam)
        {
            IReadOnlyList<CombatTimeDilationReceiver> receivers = CombatTimeDilationReceiver.ActiveInstances;
            for (int i = 0; i < receivers.Count; i++)
            {
                CombatTimeDilationReceiver receiver = receivers[i];
                if (!TryResolveHostileReceiverPosition(receiver, playerTeam, out Vector3 position))
                {
                    continue;
                }

                TryApplyReceiver(receiver.gameObject, origin, ResolveDistanceWeight(position, origin));
            }
        }

        private bool TryResolveHostileReceiverPosition(
            CombatTimeDilationReceiver receiver,
            DamageTeam playerTeam,
            out Vector3 position)
        {
            position = default;
            if (receiver == null || !receiver.gameObject.activeInHierarchy || receiver.gameObject == gameObject)
            {
                return false;
            }

            GameObject owner = receiver.gameObject;
            ReceiverContext context = ResolveReceiverContext(receiver);
            if (context.Health != null)
            {
                position = context.Health.transform.position;
                return context.Health.IsAlive
                    && CombatTeamUtility.AreHostile(context.Health.Team, playerTeam);
            }

            if (context.BossProjectile != null)
            {
                position = context.BossProjectile.transform.position;
                return context.BossProjectile.IsActive
                    && CombatTeamUtility.AreHostile(context.BossProjectile.SourceTeam, playerTeam);
            }

            if (context.LaneProjectile != null)
            {
                position = context.LaneProjectile.transform.position;
                return context.LaneProjectile.IsActive
                    && CombatTeamUtility.AreHostile(context.LaneProjectile.SourceTeam, playerTeam);
            }

            if (context.LaserPattern != null)
            {
                position = context.LaserPattern.transform.position;
                return context.LaserProxy != null
                    ? context.LaserProxy.IsActive
                        && CombatTeamUtility.AreHostile(context.LaserProxy.OwnerTeam, playerTeam)
                    : context.Health == null
                        || CombatTeamUtility.AreHostile(context.Health.Team, playerTeam);
            }

            if (!context.IsHostileEmitter)
            {
                return false;
            }

            position = owner.transform.position;
            return context.Health == null || CombatTeamUtility.AreHostile(context.Health.Team, playerTeam);
        }

        private ReceiverContext ResolveReceiverContext(CombatTimeDilationReceiver receiver)
        {
            int id = receiver.GetInstanceID();
            if (receiverContexts.TryGetValue(id, out ReceiverContext context)
                && context != null
                && context.Receiver == receiver
                && context.Owner == receiver.gameObject)
            {
                return context;
            }

            GameObject owner = receiver.gameObject;
            context = new ReceiverContext
            {
                Receiver = receiver,
                Owner = owner,
                Health = owner.GetComponent<CombatHealth>() ?? owner.GetComponentInParent<CombatHealth>(),
                BossProjectile = owner.GetComponent<BossBarrageProjectile>(),
                LaneProjectile = owner.GetComponent<LaneActionProjectile>(),
                LaserPattern = owner.GetComponent<BossLaserSummonPattern>(),
                IsHostileEmitter = owner.TryGetComponent<BossBarrageEmitter>(out _)
                    || owner.TryGetComponent<BossBasicFireEmitter>(out _)
                    || owner.TryGetComponent<EnemySummonPacingDirector>(out _)
            };
            if (context.LaserPattern != null)
            {
                context.LaserProxy = context.LaserPattern.GetComponent<SummonFrontlineProxy>();
            }

            receiverContexts[id] = context;
            receiverContextBuildCount++;
            return context;
        }

        private void BeginGlobalHitStop()
        {
            if (globalHitStopSeconds <= 0f)
            {
                return;
            }

            previousTimeScale = ownsGlobalHitStop ? previousTimeScale : Time.timeScale;
            appliedTimeScale = Mathf.Min(previousTimeScale, Mathf.Clamp(globalHitStopTimeScale, 0.02f, 1f));
            hitStopTimer = Mathf.Max(hitStopTimer, globalHitStopSeconds);
            ownsGlobalHitStop = true;
            ApplyScale(appliedTimeScale);
        }

        private void TryApplyReceiver(GameObject owner, Vector3 origin, float intensity01)
        {
            if (owner == null || intensity01 <= 0f || owner == gameObject)
            {
                return;
            }

            CombatTimeDilationReceiver receiver = CombatTimeDilationReceiver.Ensure(owner);
            if (receiver == null)
            {
                return;
            }

            int id = receiver.GetInstanceID();
            if (!affectedReceivers.Add(id))
            {
                return;
            }

            float holdSeconds = ResolveCurrentReceiverHoldSeconds();
            if (holdSeconds <= 0f)
            {
                return;
            }

            receiver.ApplyTimeDilation(timeScale, holdSeconds, blendOutSeconds, intensity01);
        }

        private float ResolveCurrentReceiverHoldSeconds()
        {
            if (timer <= 0f)
            {
                return 0f;
            }

            return Mathf.Max(0.05f, timer - Mathf.Max(0f, blendOutSeconds));
        }

        private float ResolveDistanceWeight(Vector3 position, Vector3 origin)
        {
            float safeRadius = Mathf.Max(0.1f, radius);
            float distance = Vector3.ProjectOnPlane(position - origin, Vector3.up).magnitude;
            if (distance > safeRadius)
            {
                return 0f;
            }

            float safeInnerRadius = Mathf.Clamp(innerRadius, 0f, safeRadius);
            if (distance <= safeInnerRadius)
            {
                return 1f;
            }

            float outer01 = 1f - Mathf.InverseLerp(safeInnerRadius, safeRadius, distance);
            return Mathf.Lerp(0.82f, 1f, Mathf.SmoothStep(0f, 1f, outer01));
        }

        private void ApplyScale(float scale)
        {
            if (Time.timeScale > 0.01f)
            {
                Time.timeScale = Mathf.Clamp(scale, 0.02f, 1f);
            }
        }

        private void RestoreIfStillOwner()
        {
            if (Time.timeScale <= 0.01f)
            {
                return;
            }

            float lowerOwnedScale = Mathf.Min(appliedTimeScale, previousTimeScale) - RestoreTolerance;
            float upperOwnedScale = Mathf.Max(appliedTimeScale, previousTimeScale) + RestoreTolerance;
            if (Time.timeScale >= lowerOwnedScale && Time.timeScale <= upperOwnedScale)
            {
                Time.timeScale = Mathf.Max(0.01f, previousTimeScale);
            }
        }
    }
}
