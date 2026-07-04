using System.Collections.Generic;
using DimensionBrawl.Combat;
using DimensionBrawl.Enemies;
using DimensionBrawl.Player;
using UnityEngine;

namespace DimensionBrawl.Presentation
{
    [DisallowMultipleComponent]
    public sealed class PerfectDodgeTimeWarp : MonoBehaviour
    {
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
        }

        private void Update()
        {
            float deltaTime = Time.unscaledDeltaTime > 0f ? Time.unscaledDeltaTime : Time.deltaTime;
            if (timer > 0f)
            {
                timer = Mathf.Max(0f, timer - deltaTime);
                RefreshActiveReceivers(deltaTime);
            }

            if (!ownsGlobalHitStop)
            {
                return;
            }

            hitStopTimer = Mathf.Max(0f, hitStopTimer - deltaTime);
            if (hitStopTimer > 0f)
            {
                ApplyScale(appliedTimeScale);
                return;
            }

            RestoreIfStillOwner();
            ownsGlobalHitStop = false;
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

            ApplyToHostileHealthRoots(origin, playerTeam);
            ApplyToBossProjectiles(origin, playerTeam);
            ApplyToLaneProjectiles(origin, playerTeam);
            ApplyToBasicSoldiers(origin, playerTeam);
            ApplyToBossBarrageEmitters(origin, playerTeam);
            ApplyToBossBasicFireEmitters(origin, playerTeam);
            ApplyToEnemySummonPacingDirectors(origin, playerTeam);
            ApplyToBossLaserSummonPatterns(origin, playerTeam);
            lastAffectedReceiverCount = affectedReceivers.Count;
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

        private void ApplyToHostileHealthRoots(Vector3 origin, DamageTeam playerTeam)
        {
            CombatHealth[] healths = UnityEngine.Object.FindObjectsByType<CombatHealth>(FindObjectsSortMode.None);
            for (int i = 0; i < healths.Length; i++)
            {
                CombatHealth health = healths[i];
                if (health == null
                    || !health.IsAlive
                    || !CombatTeamUtility.AreHostile(health.Team, playerTeam)
                    || health.transform == transform)
                {
                    continue;
                }

                TryApplyReceiver(health.gameObject, origin, ResolveDistanceWeight(health.transform.position, origin));
            }
        }

        private void ApplyToBossProjectiles(Vector3 origin, DamageTeam playerTeam)
        {
            BossBarrageProjectile[] projectiles =
                UnityEngine.Object.FindObjectsByType<BossBarrageProjectile>(FindObjectsSortMode.None);
            for (int i = 0; i < projectiles.Length; i++)
            {
                BossBarrageProjectile projectile = projectiles[i];
                if (projectile == null
                    || !projectile.IsActive
                    || !CombatTeamUtility.AreHostile(projectile.SourceTeam, playerTeam))
                {
                    continue;
                }

                TryApplyReceiver(projectile.gameObject, origin, ResolveDistanceWeight(projectile.transform.position, origin));
            }
        }

        private void ApplyToLaneProjectiles(Vector3 origin, DamageTeam playerTeam)
        {
            LaneActionProjectile[] projectiles =
                UnityEngine.Object.FindObjectsByType<LaneActionProjectile>(FindObjectsSortMode.None);
            for (int i = 0; i < projectiles.Length; i++)
            {
                LaneActionProjectile projectile = projectiles[i];
                if (projectile == null
                    || !projectile.IsActive
                    || !CombatTeamUtility.AreHostile(projectile.SourceTeam, playerTeam))
                {
                    continue;
                }

                TryApplyReceiver(projectile.gameObject, origin, ResolveDistanceWeight(projectile.transform.position, origin));
            }
        }

        private void ApplyToBasicSoldiers(Vector3 origin, DamageTeam playerTeam)
        {
            BasicSoldierEnemy[] soldiers =
                UnityEngine.Object.FindObjectsByType<BasicSoldierEnemy>(FindObjectsSortMode.None);
            for (int i = 0; i < soldiers.Length; i++)
            {
                BasicSoldierEnemy soldier = soldiers[i];
                if (soldier == null
                    || soldier.SelfHealth == null
                    || !soldier.SelfHealth.IsAlive
                    || !CombatTeamUtility.AreHostile(soldier.SelfHealth.Team, playerTeam))
                {
                    continue;
                }

                TryApplyReceiver(soldier.gameObject, origin, ResolveDistanceWeight(soldier.transform.position, origin));
            }
        }

        private void ApplyToBossBarrageEmitters(Vector3 origin, DamageTeam playerTeam)
        {
            BossBarrageEmitter[] emitters =
                UnityEngine.Object.FindObjectsByType<BossBarrageEmitter>(FindObjectsSortMode.None);
            for (int i = 0; i < emitters.Length; i++)
            {
                BossBarrageEmitter emitter = emitters[i];
                if (emitter == null || !IsHostileEmitterRoot(emitter.gameObject, playerTeam))
                {
                    continue;
                }

                TryApplyReceiver(emitter.gameObject, origin, ResolveDistanceWeight(emitter.transform.position, origin));
            }
        }

        private void ApplyToBossBasicFireEmitters(Vector3 origin, DamageTeam playerTeam)
        {
            BossBasicFireEmitter[] emitters =
                UnityEngine.Object.FindObjectsByType<BossBasicFireEmitter>(FindObjectsSortMode.None);
            for (int i = 0; i < emitters.Length; i++)
            {
                BossBasicFireEmitter emitter = emitters[i];
                if (emitter == null || !IsHostileEmitterRoot(emitter.gameObject, playerTeam))
                {
                    continue;
                }

                TryApplyReceiver(emitter.gameObject, origin, ResolveDistanceWeight(emitter.transform.position, origin));
            }
        }

        private void ApplyToEnemySummonPacingDirectors(Vector3 origin, DamageTeam playerTeam)
        {
            EnemySummonPacingDirector[] directors =
                UnityEngine.Object.FindObjectsByType<EnemySummonPacingDirector>(FindObjectsSortMode.None);
            for (int i = 0; i < directors.Length; i++)
            {
                EnemySummonPacingDirector director = directors[i];
                if (director == null || !IsHostileEmitterRoot(director.gameObject, playerTeam))
                {
                    continue;
                }

                TryApplyReceiver(director.gameObject, origin, ResolveDistanceWeight(director.transform.position, origin));
            }
        }

        private void ApplyToBossLaserSummonPatterns(Vector3 origin, DamageTeam playerTeam)
        {
            BossLaserSummonPattern[] patterns =
                UnityEngine.Object.FindObjectsByType<BossLaserSummonPattern>(FindObjectsSortMode.None);
            for (int i = 0; i < patterns.Length; i++)
            {
                BossLaserSummonPattern pattern = patterns[i];
                if (pattern == null || !IsHostileBossLaserPattern(pattern, playerTeam))
                {
                    continue;
                }

                TryApplyReceiver(pattern.gameObject, origin, ResolveDistanceWeight(pattern.transform.position, origin));
            }
        }

        private bool IsHostileBossLaserPattern(BossLaserSummonPattern pattern, DamageTeam playerTeam)
        {
            if (pattern == null)
            {
                return false;
            }

            SummonFrontlineProxy proxy = pattern.GetComponent<SummonFrontlineProxy>();
            if (proxy != null)
            {
                return proxy.IsActive && CombatTeamUtility.AreHostile(proxy.OwnerTeam, playerTeam);
            }

            return IsHostileEmitterRoot(pattern.gameObject, playerTeam);
        }

        private bool IsHostileEmitterRoot(GameObject owner, DamageTeam playerTeam)
        {
            CombatHealth source = owner != null ? owner.GetComponentInParent<CombatHealth>() : null;
            return source == null || CombatTeamUtility.AreHostile(source.Team, playerTeam);
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
