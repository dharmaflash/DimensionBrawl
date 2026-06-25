using System;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace IsekaiBrawl.Gameplay
{
    public enum HeroFireTargetClass
    {
        None = 0,
        EnemySummon = 1,
        Blocker = 2,
        Objective = 3,
        Boss = 4,
        Base = 5
    }

    public readonly struct HeroFireDirective
    {
        public HeroFireDirective(
            Transform targetTransform,
            Vector3 aimPosition,
            HeroFireTargetClass targetClass,
            int targetLaneIndex,
            bool canShootFromCurrentAnchor,
            bool needsMicroReposition)
        {
            TargetTransform = targetTransform;
            AimPosition = aimPosition;
            TargetClass = targetClass;
            TargetLaneIndex = targetLaneIndex;
            CanShootFromCurrentAnchor = canShootFromCurrentAnchor;
            NeedsMicroReposition = needsMicroReposition;
        }

        public Transform TargetTransform { get; }
        public Vector3 AimPosition { get; }
        public HeroFireTargetClass TargetClass { get; }
        public int TargetLaneIndex { get; }
        public bool CanShootFromCurrentAnchor { get; }
        public bool NeedsMicroReposition { get; }
        public bool HasTarget => TargetClass != HeroFireTargetClass.None;
        public string RoleLabel => TargetClass switch
        {
            HeroFireTargetClass.EnemySummon => "SUMMON",
            HeroFireTargetClass.Blocker => "BLOCK",
            HeroFireTargetClass.Objective => "OBJECT",
            HeroFireTargetClass.Boss => "BOSS",
            HeroFireTargetClass.Base => "BASE",
            _ => "NONE"
        };
    }

    [RequireComponent(typeof(PlayerController))]
    public class PlayerSkillController : MonoBehaviour
    {
        public event Action OnSkillActivated;
        public event Action<float, float> OnCooldownChanged;
        public event Action<bool> OnFrontlineHoldChanged;
        public event Action OnOverdriveTriggered;

        [SerializeField] private string activeSkillName = "Rally Order";
        [SerializeField] private float activeSkillEnergyCost = 46f;
        [SerializeField] private float activeSkillCooldown = 10f;
        [SerializeField] private float activeSkillRange = 6.2f;
        [SerializeField] private float activeSkillDamage = 12f;
        [SerializeField] private float projectileClearRadiusMultiplier = 1.4f;
        [SerializeField] private float activeSkillShakeDuration = 0.14f;
        [SerializeField] private float activeSkillShakeMagnitude = 0.14f;
        [SerializeField] private float frontlineHoldRange = 5.6f;
        [SerializeField] private float frontlineHoldThresholdZ = 13.5f;
        [SerializeField] private int frontlineHoldRequiredAllies = 2;
        [SerializeField] private float frontlineHoldCostReduction = 8f;
        [SerializeField] private float frontlineHoldEnergyRefund = 6f;
        [SerializeField] private float dodgeOverdriveDuration = 4f;
        [SerializeField] private float dodgeOverdriveCostReduction = 12f;
        [SerializeField] private ParticleSystem activeSkillEffect;

        private float cooldownRemaining;
        private int enemySummonLayerMask;
        private int playerSummonLayerMask;
        private PlayerController playerController;
        private float dodgeOverdriveExpiresAt;
        private bool isFrontlineHoldActive;
        private int escortedAlliesCount;

        public string ActiveSkillName => activeSkillName;
        public float ActiveSkillEnergyCost => ResolveCurrentSkillCost();
        public float CooldownRemaining => cooldownRemaining;
        public float CooldownDuration => activeSkillCooldown;
        public string InputHint => "[Space/E]";
        public bool IsFrontlineHoldActive => isFrontlineHoldActive;
        public bool IsDodgeOverdriveActive => Time.time <= dodgeOverdriveExpiresAt;
        public float DodgeOverdriveRemaining => Mathf.Max(0f, dodgeOverdriveExpiresAt - Time.time);
        public int EscortedAlliesCount => escortedAlliesCount;
        public string PassiveSummary => BuildPassiveSummary();
        public bool CanActivate =>
            cooldownRemaining <= 0f &&
            BattleEnergySystem.Instance != null &&
            BattleEnergySystem.Instance.CurrentEnergy >= ActiveSkillEnergyCost;

        private void Awake()
        {
            enemySummonLayerMask = LayerMask.GetMask("EnemySummon");
            playerSummonLayerMask = LayerMask.GetMask("PlayerSummon");
            playerController = GetComponent<PlayerController>();
            if (activeSkillEffect == null)
            {
                activeSkillEffect = CreateActiveSkillEffect();
            }
        }

        private void OnEnable()
        {
            if (playerController != null)
            {
                playerController.OnJustDodgeRewarded += HandleJustDodgeRewarded;
            }
        }

        private void OnDisable()
        {
            if (playerController != null)
            {
                playerController.OnJustDodgeRewarded -= HandleJustDodgeRewarded;
            }
        }

        private void Start()
        {
            UpdatePassiveStates(forceNotify: true);
            NotifyCooldownChanged();
        }

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Battle)
            {
                return;
            }

            if (cooldownRemaining > 0f)
            {
                float previousCooldown = cooldownRemaining;
                cooldownRemaining = Mathf.Max(0f, cooldownRemaining - Time.deltaTime);
                if (!Mathf.Approximately(previousCooldown, cooldownRemaining))
                {
                    NotifyCooldownChanged();
                }
            }

            if (playerController != null && !playerController.CanAct)
            {
                UpdatePassiveStates(forceNotify: false);
                return;
            }

            UpdatePassiveStates(forceNotify: false);

            if (ReadSkillPressed())
            {
                _ = TryActivateSkill();
            }
        }

        public bool TryActivateSkill()
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Battle)
            {
                return false;
            }

            if (playerController != null && !playerController.CanAct)
            {
                return false;
            }

            if (cooldownRemaining > 0f || BattleEnergySystem.Instance == null)
            {
                return false;
            }

            float energyCost = ActiveSkillEnergyCost;
            if (!BattleEnergySystem.Instance.SpendEnergy(energyCost))
            {
                return false;
            }

            ActivatePulse(energyCost);
            cooldownRemaining = activeSkillCooldown;
            NotifyCooldownChanged();
            OnSkillActivated?.Invoke();
            return true;
        }

        private void ActivatePulse(float energyCost)
        {
            if (activeSkillEffect != null)
            {
                activeSkillEffect.Play();
            }

            float rangeSquared = activeSkillRange * activeSkillRange;
            bool hadFrontlineHold = isFrontlineHoldActive;
            bool hadOverdrive = IsDodgeOverdriveActive;
            float supportDuration = 4.5f + (hadFrontlineHold ? 0.6f : 0f) + (hadOverdrive ? 0.8f : 0f);
            float supportDamageMultiplier = 1.5f + (hadFrontlineHold ? 0.12f : 0f) + (hadOverdrive ? 0.1f : 0f);
            float supportMoveMultiplier = 1.35f + (hadOverdrive ? 0.08f : 0f);
            float supportHeal = 16f + (hadFrontlineHold ? 6f : 0f) + (hadOverdrive ? 8f : 0f);
            float enemyPulseDamage = activeSkillDamage + (hadOverdrive ? 6f : 0f);
            int buffedAllies = 0;
            if (playerSummonLayerMask != 0)
            {
                Collider[] alliedSummonColliders = Physics.OverlapSphere(transform.position, activeSkillRange, playerSummonLayerMask);
                for (int index = 0; index < alliedSummonColliders.Length; index++)
                {
                    SummonUnit summonUnit = alliedSummonColliders[index].GetComponentInParent<SummonUnit>();
                    if (summonUnit == null || !summonUnit.IsPlayerTeam || !summonUnit.IsAlive)
                    {
                        continue;
                    }

                    summonUnit.ApplyHeroSupport(supportDuration, supportDamageMultiplier, supportMoveMultiplier, supportHeal);
                    buffedAllies++;
                }
            }
            else
            {
                SummonUnit[] alliedUnits = FindObjectsByType<SummonUnit>(FindObjectsSortMode.None);
                for (int index = 0; index < alliedUnits.Length; index++)
                {
                    SummonUnit summonUnit = alliedUnits[index];
                    if (summonUnit == null || !summonUnit.IsPlayerTeam || !summonUnit.IsAlive)
                    {
                        continue;
                    }

                    if ((summonUnit.transform.position - transform.position).sqrMagnitude > rangeSquared)
                    {
                        continue;
                    }

                    summonUnit.ApplyHeroSupport(supportDuration, supportDamageMultiplier, supportMoveMultiplier, supportHeal);
                    buffedAllies++;
                }
            }

            if (enemySummonLayerMask != 0)
            {
                Collider[] enemySummonColliders = Physics.OverlapSphere(transform.position, activeSkillRange, enemySummonLayerMask);
                for (int index = 0; index < enemySummonColliders.Length; index++)
                {
                    SummonUnit summonUnit = enemySummonColliders[index].GetComponentInParent<SummonUnit>();
                    if (summonUnit == null || summonUnit.IsPlayerTeam)
                    {
                        continue;
                    }

                    summonUnit.TakeDamage(enemyPulseDamage);
                }
            }
            else
            {
                SummonUnit[] summonUnits = FindObjectsByType<SummonUnit>(FindObjectsSortMode.None);
                for (int index = 0; index < summonUnits.Length; index++)
                {
                    SummonUnit summonUnit = summonUnits[index];
                    if (summonUnit == null || summonUnit.IsPlayerTeam)
                    {
                        continue;
                    }

                    if ((summonUnit.transform.position - transform.position).sqrMagnitude > rangeSquared)
                    {
                        continue;
                    }

                    summonUnit.TakeDamage(enemyPulseDamage);
                }
            }

            float clearRadiusMultiplier = projectileClearRadiusMultiplier + (hadOverdrive ? 0.18f : 0f);
            int clearedProjectiles =
                EnemyProjectile.ClearProjectilesInRadius(transform.position, activeSkillRange * clearRadiusMultiplier) +
                EnemyLineProjectile.ClearProjectilesInRadius(transform.position, activeSkillRange * clearRadiusMultiplier);
            float energyRefund = 0f;
            if (hadFrontlineHold && buffedAllies >= frontlineHoldRequiredAllies)
            {
                energyRefund += frontlineHoldEnergyRefund;
            }

            if (energyRefund > 0f)
            {
                BattleEnergySystem.Instance?.AddEnergy(energyRefund);
            }

            if (hadOverdrive)
            {
                dodgeOverdriveExpiresAt = 0f;
            }

            if (buffedAllies > 0 || clearedProjectiles > 0)
            {
                BattlePresentationController.Instance?.ShowWorldText(
                    transform.position + new Vector3(0f, 2f, 0f),
                    energyRefund > 0f
                        ? $"RALLY +{buffedAllies} / CLEAR {clearedProjectiles} / REFUND {Mathf.CeilToInt(energyRefund)}"
                        : $"RALLY +{buffedAllies} / CLEAR {clearedProjectiles}",
                    new Color(0.95f, 0.92f, 0.45f, 1f),
                    4f,
                    0.6f);
            }

            if (hadOverdrive)
            {
                BattlePresentationController.Instance?.ShowScreenFlash(new Color(0.52f, 0.95f, 1f, 1f), 0.1f, 0.18f);
            }

            CameraShake.Instance?.PlayShake(activeSkillShakeDuration, activeSkillShakeMagnitude);
            UpdatePassiveStates(forceNotify: true);
        }

        private void NotifyCooldownChanged()
        {
            OnCooldownChanged?.Invoke(cooldownRemaining, activeSkillCooldown);
        }

        private void HandleJustDodgeRewarded(float _)
        {
            dodgeOverdriveExpiresAt = Time.time + dodgeOverdriveDuration;
            OnOverdriveTriggered?.Invoke();
            UpdatePassiveStates(forceNotify: true);
        }

        private void UpdatePassiveStates(bool forceNotify)
        {
            bool previousFrontlineHold = isFrontlineHoldActive;
            escortedAlliesCount = CountEscortedAllies();
            isFrontlineHoldActive = ResolveFrontlineHoldState(escortedAlliesCount);

            if (forceNotify || previousFrontlineHold != isFrontlineHoldActive)
            {
                OnFrontlineHoldChanged?.Invoke(isFrontlineHoldActive);
            }
        }

        private int CountEscortedAllies()
        {
            if (playerController == null)
            {
                return 0;
            }

            int escortedCount = 0;
            float rangeSquared = frontlineHoldRange * frontlineHoldRange;
            if (playerSummonLayerMask != 0)
            {
                Collider[] colliders = Physics.OverlapSphere(transform.position, frontlineHoldRange, playerSummonLayerMask);
                for (int index = 0; index < colliders.Length; index++)
                {
                    SummonUnit unit = colliders[index] != null ? colliders[index].GetComponentInParent<SummonUnit>() : null;
                    if (unit == null || !unit.IsAlive || !unit.IsPlayerTeam)
                    {
                        continue;
                    }

                    Vector3 delta = unit.transform.position - transform.position;
                    if (delta.sqrMagnitude > rangeSquared || delta.z < -2.4f)
                    {
                        continue;
                    }

                    escortedCount++;
                }

                return escortedCount;
            }

            SummonUnit[] allUnits = FindObjectsByType<SummonUnit>(FindObjectsSortMode.None);
            for (int index = 0; index < allUnits.Length; index++)
            {
                SummonUnit unit = allUnits[index];
                if (unit == null || !unit.IsAlive || !unit.IsPlayerTeam)
                {
                    continue;
                }

                Vector3 delta = unit.transform.position - transform.position;
                if (delta.sqrMagnitude > rangeSquared || delta.z < -2.4f)
                {
                    continue;
                }

                escortedCount++;
            }

            return escortedCount;
        }

        private bool ResolveFrontlineHoldState(int nearbyEscortCount)
        {
            if (playerController == null || nearbyEscortCount < frontlineHoldRequiredAllies || playerController.transform.position.z < frontlineHoldThresholdZ)
            {
                return false;
            }

            if (BattleManager.Instance == null || !BattleManager.Instance.TryGetFrontlineState(out BattleManager.FrontlineState frontlineState))
            {
                return false;
            }

            return playerController.transform.position.z >= frontlineState.PlayerFrontZ - 3.8f;
        }

        private float ResolveCurrentSkillCost()
        {
            float cost = activeSkillEnergyCost;
            if (isFrontlineHoldActive)
            {
                cost -= frontlineHoldCostReduction;
            }

            if (IsDodgeOverdriveActive)
            {
                cost -= dodgeOverdriveCostReduction;
            }

            return Mathf.Max(20f, cost);
        }

        private string BuildPassiveSummary()
        {
            string frontlineText = isFrontlineHoldActive
                ? "Hold READY"
                : $"Hold {Mathf.Min(escortedAlliesCount, frontlineHoldRequiredAllies)}/{frontlineHoldRequiredAllies}";
            string dodgeText = IsDodgeOverdriveActive
                ? $"Dodge {DodgeOverdriveRemaining:0.0}s"
                : "Dodge JUST -> Burst";
            return $"{frontlineText}  |  {dodgeText}";
        }

        private static bool ReadSkillPressed()
        {
            if (MobileBattleControls.ConsumeSkillPressed())
            {
                return true;
            }

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.eKey.wasPressedThisFrame))
            {
                return true;
            }

            if (Gamepad.current != null && (Gamepad.current.rightShoulder.wasPressedThisFrame || Gamepad.current.buttonSouth.wasPressedThisFrame))
            {
                return true;
            }
#else
            return Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E);
#endif

            return false;
        }

        private ParticleSystem CreateActiveSkillEffect()
        {
            GameObject effectObject = new("ActiveSkillEffect");
            effectObject.transform.SetParent(transform, false);
            effectObject.transform.localPosition = new Vector3(0f, 0.8f, 0f);

            ParticleSystem particleSystem = effectObject.AddComponent<ParticleSystem>();
            particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ParticleSystem.MainModule main = particleSystem.main;
            main.playOnAwake = false;
            main.duration = 0.55f;
            main.loop = false;
            main.startLifetime = 0.35f;
            main.startSpeed = 5f;
            main.startSize = 0.45f;
            main.startColor = new Color(0.95f, 0.9f, 0.35f, 1f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            ParticleSystem.EmissionModule emission = particleSystem.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 36) });

            ParticleSystem.ShapeModule shape = particleSystem.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.35f;

            ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = particleSystem.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 1f, 1f, 0f));

            particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            return particleSystem;
        }
    }

    [RequireComponent(typeof(PlayerController))]
    public class PlayerCombatController : MonoBehaviour
    {
        [SerializeField] private float shotCooldown = 0.78f;
        [SerializeField] private float projectileSpeed = 18f;
        [SerializeField] private float projectileDamage = 11f;
        [SerializeField] private float structureDamageMultiplier = 0.75f;
        [SerializeField] private float baseDamage = 7f;
        [SerializeField] private float projectileLifetime = 3.4f;
        [SerializeField] private float energyOnHit = 0f;
        [SerializeField] private float impactShakeDuration = 0.08f;
        [SerializeField] private float impactShakeMagnitude = 0.08f;
        [SerializeField] private float autoAttackRange = 9.5f;
        [SerializeField] private float rallyRadius = 4.9f;
        [SerializeField] private float rallyPulseInterval = 1f;
        [SerializeField] private float rallyDuration = 1.5f;
        [SerializeField] private float rallyDamageMultiplier = 1.34f;
        [SerializeField] private float rallyMoveSpeedMultiplier = 1.22f;
        [SerializeField] private float rallyHealPerPulse = 6f;
        [SerializeField] private float frontlineSupportEnergy = 0.9f;
        [SerializeField] private float frontlineSupportThreshold = 14f;
        [SerializeField] private float sameLaneTolerance = 1.9f;
        [SerializeField] private float escortDepthTolerance = 4.25f;

        private float cooldownRemaining;
        private float rallyTimer;
        private int playerSummonLayerMask;
        private PlayerController playerController;
        private string currentAutoTargetRoleLabel = "NONE";

        public string BasicAttackHint => "Command Shot";
        public string SupportSummary => "Lane Rally / Frontline Hold";
        public string CurrentAutoTargetRoleLabel => currentAutoTargetRoleLabel;
        public float AutoAttackRange => autoAttackRange;
        public float SameLaneTolerance => sameLaneTolerance;

        public void ConfigureEconomyTuning(float hitEnergyReward, float frontlineEnergyReward, float threshold)
        {
            energyOnHit = Mathf.Max(0f, hitEnergyReward);
            frontlineSupportEnergy = Mathf.Max(0f, frontlineEnergyReward);
            frontlineSupportThreshold = Mathf.Max(0f, threshold);
        }

        private void Awake()
        {
            playerSummonLayerMask = LayerMask.GetMask("PlayerSummon");
            playerController = GetComponent<PlayerController>();
        }

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Battle)
            {
                return;
            }

            if (cooldownRemaining > 0f)
            {
                cooldownRemaining = Mathf.Max(0f, cooldownRemaining - Time.deltaTime);
            }

            rallyTimer -= Time.deltaTime;

            if (playerController != null && !playerController.CanAct)
            {
                return;
            }

            if (cooldownRemaining <= 0f)
            {
                TryAutoFire();
            }

            if (rallyTimer <= 0f)
            {
                PulseRallySupport();
            }
        }

        private void TryAutoFire()
        {
            HeroFireDirective fireDirective = BuildFireDirective(transform.position);
            currentAutoTargetRoleLabel = fireDirective.RoleLabel;
            if (!fireDirective.HasTarget || !fireDirective.CanShootFromCurrentAnchor)
            {
                return;
            }

            Vector3 forward = fireDirective.AimPosition - transform.position;
            forward.y = 0f;
            if (forward.sqrMagnitude <= 0.001f)
            {
                forward = Vector3.forward;
            }

            transform.rotation = Quaternion.LookRotation(forward.normalized, Vector3.up);
            Vector3 spawnPosition = transform.position + Vector3.up * 1.15f + forward.normalized * 0.75f;

            GameObject projectileObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectileObject.name = "PlayerProjectile";
            projectileObject.transform.position = spawnPosition;
            projectileObject.transform.localScale = Vector3.one * 0.22f;

            Collider projectileCollider = projectileObject.GetComponent<Collider>();
            projectileCollider.isTrigger = true;

            Rigidbody rigidbodyComponent = projectileObject.AddComponent<Rigidbody>();
            rigidbodyComponent.useGravity = false;
            rigidbodyComponent.isKinematic = true;

            Renderer projectileRenderer = projectileObject.GetComponent<Renderer>();
            if (projectileRenderer != null)
            {
                projectileRenderer.material.color = new Color(0.45f, 0.92f, 1f, 1f);
            }

            PlayerProjectile projectile = projectileObject.AddComponent<PlayerProjectile>();
            projectile.Initialize(
                forward.normalized,
                projectileSpeed,
                projectileDamage,
                baseDamage,
                structureDamageMultiplier,
                projectileLifetime,
                energyOnHit,
                impactShakeDuration,
                impactShakeMagnitude);

            cooldownRemaining = shotCooldown;
        }

        public HeroFireDirective BuildFireDirective(Vector3 origin)
        {
            int escortLaneIndex = playerController != null
                ? playerController.EscortLaneIndex
                : BattleLaneUtility.DefaultLaneCount / 2;

            return BuildFireDirective(origin, escortLaneIndex);
        }

        public HeroFireDirective BuildFireDirective(Vector3 origin, int escortLaneIndex)
        {
            float rangeSquared = autoAttackRange * autoAttackRange;
            escortLaneIndex = BattleLaneUtility.ClampLaneIndex(escortLaneIndex);
            currentAutoTargetRoleLabel = "NONE";
            Transform lockedTargetTransform = null;
            int lockedLaneIndex = escortLaneIndex;
            ManualTargetLockKind lockedTargetKind = ManualTargetLockKind.None;
            bool hasManualLock = playerController != null &&
                playerController.TryGetManualTargetLock(out lockedTargetTransform, out lockedLaneIndex, out lockedTargetKind);

            bool sameLaneManualLock = hasManualLock && lockedLaneIndex == escortLaneIndex;
            if (sameLaneManualLock)
            {
                HeroFireDirective sameLaneDirective = BuildLockedDirective(
                    origin,
                    rangeSquared,
                    lockedTargetTransform,
                    lockedLaneIndex,
                    lockedTargetKind);
                if (sameLaneDirective.HasTarget)
                {
                    return sameLaneDirective;
                }
            }

            if (TryResolvePriorityStructureDirective(origin, rangeSquared, escortLaneIndex, out HeroFireDirective structureDirective))
            {
                return structureDirective;
            }

            if (TryResolveSameLaneEnemyDirective(origin, rangeSquared, escortLaneIndex, out HeroFireDirective sameLaneEnemyDirective))
            {
                return sameLaneEnemyDirective;
            }

            if (hasManualLock)
            {
                HeroFireDirective crossLaneDirective = BuildLockedDirective(
                    origin,
                    rangeSquared,
                    lockedTargetTransform,
                    lockedLaneIndex,
                    lockedTargetKind);
                if (crossLaneDirective.HasTarget && crossLaneDirective.CanShootFromCurrentAnchor)
                {
                    return crossLaneDirective;
                }
            }

            if (TryResolveEnemyBaseDirective(origin, rangeSquared, out HeroFireDirective baseDirective))
            {
                return baseDirective;
            }

            return default;
        }

        private bool TryResolveSameLaneEnemyDirective(
            Vector3 origin,
            float rangeSquared,
            int escortLaneIndex,
            out HeroFireDirective directive)
        {
            directive = default;
            if (playerController != null &&
                playerController.CurrentEscortPhase == BattleManager.EscortPhase.Ready)
            {
                return false;
            }

            SummonUnit[] summonUnits = FindObjectsByType<SummonUnit>(FindObjectsSortMode.None);
            float bestDistance = float.MaxValue;
            Transform bestTargetTransform = null;
            Vector3 bestAimPosition = Vector3.zero;

            for (int index = 0; index < summonUnits.Length; index++)
            {
                SummonUnit summonUnit = summonUnits[index];
                if (summonUnit == null || !summonUnit.IsAlive || summonUnit.IsPlayerTeam)
                {
                    continue;
                }

                if (summonUnit.AssignedLaneIndex != escortLaneIndex)
                {
                    continue;
                }

                Vector3 targetOffset = summonUnit.transform.position - origin;
                if (targetOffset.z < -0.5f || targetOffset.sqrMagnitude > rangeSquared || Mathf.Abs(targetOffset.x) > sameLaneTolerance)
                {
                    continue;
                }

                float weightedDistance = targetOffset.sqrMagnitude - (targetOffset.z * 0.45f);
                if (weightedDistance >= bestDistance)
                {
                    continue;
                }

                bestDistance = weightedDistance;
                bestTargetTransform = summonUnit.transform;
                bestAimPosition = summonUnit.transform.position + Vector3.up * 0.6f;
            }

            if (bestTargetTransform != null)
            {
                directive = new HeroFireDirective(
                    bestTargetTransform,
                    bestAimPosition,
                    HeroFireTargetClass.EnemySummon,
                    escortLaneIndex,
                    canShootFromCurrentAnchor: true,
                    needsMicroReposition: false);
                return true;
            }

            return false;
        }

        private bool TryResolvePriorityStructureDirective(
            Vector3 origin,
            float rangeSquared,
            int escortLaneIndex,
            out HeroFireDirective directive)
        {
            directive = default;

            BattleStructure structure = null;
            BattleManager battleManager = BattleManager.Instance;
            if (battleManager != null)
            {
                battleManager.TryGetPreferredInterventionStructure(escortLaneIndex, out structure);
            }

            if (structure == null)
            {
                structure = BattleStructure.FindClosestActiveInLaneAnyRole(
                    origin,
                    autoAttackRange,
                    escortLaneIndex,
                    isPlayerTeam: true,
                    backwardAllowance: 0.35f);
            }

            if (structure != null && !structure.IsDestroyed)
            {
                Vector3 targetOffset = structure.transform.position - origin;
                if (targetOffset.z >= -0.5f && Mathf.Abs(targetOffset.x) <= sameLaneTolerance + 0.4f)
                {
                    directive = new HeroFireDirective(
                        structure.transform,
                        structure.transform.position + Vector3.up * 0.45f,
                        structure.Role == BattleStructureRole.FrontlineBlocker
                            ? HeroFireTargetClass.Blocker
                            : HeroFireTargetClass.Objective,
                        escortLaneIndex,
                        canShootFromCurrentAnchor: true,
                        needsMicroReposition: false);
                    return true;
                }

                directive = new HeroFireDirective(
                    structure.transform,
                    structure.transform.position + Vector3.up * 0.45f,
                    structure.Role == BattleStructureRole.FrontlineBlocker
                        ? HeroFireTargetClass.Blocker
                        : HeroFireTargetClass.Objective,
                    escortLaneIndex,
                    canShootFromCurrentAnchor: false,
                    needsMicroReposition: true);
                return true;
            }

            return false;
        }

        private bool TryResolveEnemyBaseDirective(Vector3 origin, float rangeSquared, out HeroFireDirective directive)
        {
            directive = default;
            Transform enemyBase = BattleManager.Instance != null ? BattleManager.Instance.GetOpposingBaseTransform(isPlayerTeam: true) : null;
            if (enemyBase != null)
            {
                Vector3 targetOffset = enemyBase.position - origin;
                if (targetOffset.z >= -0.5f && targetOffset.sqrMagnitude <= rangeSquared && Mathf.Abs(targetOffset.x) <= sameLaneTolerance + 0.75f)
                {
                    directive = new HeroFireDirective(
                        enemyBase,
                        enemyBase.position + Vector3.up * 0.45f,
                        HeroFireTargetClass.Base,
                        BattleLaneUtility.DefaultLaneCount / 2,
                        canShootFromCurrentAnchor: true,
                        needsMicroReposition: false);
                    return true;
                }
            }

            return false;
        }

        private HeroFireDirective BuildLockedDirective(
            Vector3 origin,
            float rangeSquared,
            Transform lockedTargetTransform,
            int lockedLaneIndex,
            ManualTargetLockKind lockedTargetKind)
        {
            if (lockedTargetTransform == null)
            {
                return default;
            }

            float verticalOffset = lockedTargetKind switch
            {
                ManualTargetLockKind.Boss => 1f,
                ManualTargetLockKind.Structure => 0.45f,
                _ => 0.6f
            };
            Vector3 aimPosition = lockedTargetTransform.position + (Vector3.up * verticalOffset);
            HeroFireTargetClass targetClass = lockedTargetKind switch
            {
                ManualTargetLockKind.Boss => HeroFireTargetClass.Boss,
                ManualTargetLockKind.Structure => HeroFireTargetClass.Objective,
                _ => HeroFireTargetClass.EnemySummon
            };

            bool canShoot = TryResolveLockedTargetPosition(
                origin,
                rangeSquared,
                lockedTargetTransform,
                lockedTargetKind,
                out Vector3 lockedTargetPosition,
                out _);
            if (canShoot)
            {
                aimPosition = lockedTargetPosition;
            }

            return new HeroFireDirective(
                lockedTargetTransform,
                aimPosition,
                targetClass,
                BattleLaneUtility.ClampLaneIndex(lockedLaneIndex),
                canShoot,
                needsMicroReposition: !canShoot);
        }

        private bool TryResolveLockedTargetPosition(
            Vector3 origin,
            float rangeSquared,
            Transform lockedTargetTransform,
            ManualTargetLockKind lockedTargetKind,
            out Vector3 targetPosition,
            out string targetRoleLabel)
        {
            targetPosition = Vector3.zero;
            targetRoleLabel = "NONE";
            if (lockedTargetTransform == null)
            {
                return false;
            }

            float verticalOffset = lockedTargetKind switch
            {
                ManualTargetLockKind.Boss => 1f,
                ManualTargetLockKind.Structure => 0.45f,
                _ => 0.6f
            };
            Vector3 resolvedTargetPosition = lockedTargetTransform.position + (Vector3.up * verticalOffset);
            Vector3 targetOffset = resolvedTargetPosition - origin;
            float laneTolerance = lockedTargetKind switch
            {
                ManualTargetLockKind.Boss => sameLaneTolerance + 2.2f,
                ManualTargetLockKind.Structure => sameLaneTolerance + 0.9f,
                _ => sameLaneTolerance + 0.4f
            };

            if (targetOffset.z < -0.5f || targetOffset.sqrMagnitude > rangeSquared || Mathf.Abs(targetOffset.x) > laneTolerance)
            {
                return false;
            }

            targetPosition = resolvedTargetPosition;
            targetRoleLabel = lockedTargetKind switch
            {
                ManualTargetLockKind.Boss => "BOSS",
                ManualTargetLockKind.Structure => "STRUCT",
                _ => "LOCK"
            };
            return true;
        }

        private void PulseRallySupport()
        {
            rallyTimer = rallyPulseInterval;

            SummonUnit[] alliedUnits = GatherAlliedUnits();
            int buffedUnitCount = 0;
            for (int index = 0; index < alliedUnits.Length; index++)
            {
                SummonUnit alliedUnit = alliedUnits[index];
                if (alliedUnit == null || !alliedUnit.IsAlive || !alliedUnit.IsPlayerTeam)
                {
                    continue;
                }

                if (playerController != null && alliedUnit.AssignedLaneIndex != playerController.CurrentLaneIndex)
                {
                    continue;
                }

                if ((alliedUnit.transform.position - transform.position).sqrMagnitude > rallyRadius * rallyRadius)
                {
                    continue;
                }

                float horizontalOffset = Mathf.Abs(alliedUnit.transform.position.x - transform.position.x);
                if (horizontalOffset > sameLaneTolerance)
                {
                    continue;
                }

                float depthOffset = alliedUnit.transform.position.z - transform.position.z;
                if (depthOffset < -escortDepthTolerance || depthOffset > rallyRadius)
                {
                    continue;
                }

                alliedUnit.ApplyHeroSupport(rallyDuration, rallyDamageMultiplier, rallyMoveSpeedMultiplier, rallyHealPerPulse);
                buffedUnitCount++;
            }

        }

        private SummonUnit[] GatherAlliedUnits()
        {
            if (playerSummonLayerMask != 0)
            {
                Collider[] colliders = Physics.OverlapSphere(transform.position, rallyRadius, playerSummonLayerMask);
                SummonUnit[] units = new SummonUnit[colliders.Length];
                for (int index = 0; index < colliders.Length; index++)
                {
                    units[index] = colliders[index] != null ? colliders[index].GetComponentInParent<SummonUnit>() : null;
                }

                return units;
            }

            SummonUnit[] allUnits = FindObjectsByType<SummonUnit>(FindObjectsSortMode.None);
            return allUnits;
        }
    }

    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerProjectile : MonoBehaviour
    {
        private Vector3 moveDirection = Vector3.forward;
        private float moveSpeed = 18f;
        private float damage = 20f;
        private float baseDamage = 16f;
        private float structureDamageMultiplier = 1.2f;
        private float maxLifetime = 3f;
        private float energyOnHit = 3f;
        private float shakeDuration = 0.08f;
        private float shakeMagnitude = 0.08f;
        private float lifetime;
        private bool hasHit;

        private void Awake()
        {
            Rigidbody rigidbodyComponent = GetComponent<Rigidbody>();
            rigidbodyComponent.useGravity = false;
            rigidbodyComponent.isKinematic = true;

            Collider colliderComponent = GetComponent<Collider>();
            colliderComponent.isTrigger = true;
            EnsureTrail(new Color(0.45f, 0.92f, 1f, 0.85f));
        }

        private void Update()
        {
            transform.position += moveDirection * moveSpeed * Time.deltaTime;
            transform.Rotate(Vector3.right, 320f * Time.deltaTime, Space.Self);
            lifetime += Time.deltaTime;
            if (lifetime >= maxLifetime)
            {
                Destroy(gameObject);
            }
        }

        public void Initialize(
            Vector3 direction,
            float projectileMoveSpeed,
            float projectileDamage,
            float projectileBaseDamage,
            float projectileStructureDamageMultiplier,
            float lifetimeSeconds,
            float energyReward,
            float impactShakeDuration,
            float impactShakeMagnitude)
        {
            moveDirection = direction.sqrMagnitude > 0.001f ? direction.normalized : Vector3.forward;
            moveSpeed = projectileMoveSpeed;
            damage = projectileDamage;
            baseDamage = projectileBaseDamage;
            structureDamageMultiplier = projectileStructureDamageMultiplier;
            maxLifetime = lifetimeSeconds;
            energyOnHit = energyReward;
            shakeDuration = impactShakeDuration;
            shakeMagnitude = impactShakeMagnitude;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (hasHit)
            {
                return;
            }

            SummonUnit summonUnit = other.GetComponent<SummonUnit>();
            if (summonUnit == null)
            {
                summonUnit = other.GetComponentInParent<SummonUnit>();
            }

            if (summonUnit != null)
            {
                if (summonUnit.IsPlayerTeam)
                {
                    return;
                }

                summonUnit.TakeDamage(damage);
                ResolveImpact(transform.position, true);
                return;
            }

            BattleStructure structure = other.GetComponent<BattleStructure>();
            if (structure == null)
            {
                structure = other.GetComponentInParent<BattleStructure>();
            }

            if (structure != null)
            {
                structure.TakeDamage(damage * structureDamageMultiplier, causedByPlayerTeam: true);
                ResolveImpact(transform.position, true);
                return;
            }

            EnemyAI enemyBoss = other.GetComponent<EnemyAI>();
            if (enemyBoss == null)
            {
                enemyBoss = other.GetComponentInParent<EnemyAI>();
            }

            if (enemyBoss != null)
            {
                ResolveImpact(transform.position, false);
                return;
            }

            Transform enemyBase = BattleManager.Instance != null ? BattleManager.Instance.GetOpposingBaseTransform(isPlayerTeam: true) : null;
            if (enemyBase != null && (other.transform == enemyBase || other.transform.IsChildOf(enemyBase)))
            {
                BattleManager.Instance.DamageEnemyBase(baseDamage);
                ResolveImpact(transform.position, true);
            }
        }

        private void ResolveImpact(Vector3 position, bool rewardEnergy)
        {
            hasHit = true;
            if (rewardEnergy)
            {
                BattleEnergySystem.Instance?.AddEnergy(energyOnHit);
            }

            CameraShake.Instance?.PlayShake(shakeDuration, shakeMagnitude);
            SpawnImpactBurst(position);
            Destroy(gameObject);
        }

        private static void SpawnImpactBurst(Vector3 position)
        {
            GameObject effectObject = new("PlayerProjectileImpact");
            effectObject.transform.position = position;

            ParticleSystem particleSystem = effectObject.AddComponent<ParticleSystem>();
            particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ParticleSystem.MainModule main = particleSystem.main;
            main.playOnAwake = false;
            main.duration = 0.24f;
            main.loop = false;
            main.startLifetime = 0.18f;
            main.startSpeed = 2.8f;
            main.startSize = 0.2f;
            main.startColor = new Color(0.55f, 0.95f, 1f, 1f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            ParticleSystem.EmissionModule emission = particleSystem.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 12) });

            ParticleSystem.ShapeModule shape = particleSystem.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.08f;

            particleSystem.Play();
            Destroy(effectObject, 0.5f);
        }

        private void EnsureTrail(Color color)
        {
            TrailRenderer trailRenderer = GetComponent<TrailRenderer>();
            if (trailRenderer == null)
            {
                trailRenderer = gameObject.AddComponent<TrailRenderer>();
            }

            trailRenderer.time = 0.16f;
            trailRenderer.startWidth = 0.12f;
            trailRenderer.endWidth = 0.01f;
            trailRenderer.alignment = LineAlignment.View;
            trailRenderer.minVertexDistance = 0.02f;
            trailRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            trailRenderer.receiveShadows = false;
            trailRenderer.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
            Gradient gradient = new();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(color, 0f),
                    new GradientColorKey(Color.white, 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0.9f, 0f),
                    new GradientAlphaKey(0f, 1f)
                });
            trailRenderer.colorGradient = gradient;
        }
    }
}
