using System.Collections;
using UnityEngine;

namespace IsekaiBrawl.Gameplay
{
    [RequireComponent(typeof(Collider))]
    public class SummonUnit : MonoBehaviour
    {
        [SerializeField] private float destroyDelay = 0.2f;
        [SerializeField] private float hitFlashDuration = 0.08f;
        [SerializeField] private Color hitFlashColor = new(1f, 0.85f, 0.85f, 1f);
        [SerializeField] private Color healFlashColor = new(0.62f, 1f, 0.78f, 1f);
        [SerializeField] private float attackPulseDuration = 0.08f;
        [SerializeField] private float attackPulseScaleMultiplier = 1.08f;
        [SerializeField] private Vector3 healthBarOffset = new(0f, 1.45f, 0f);
        [SerializeField] private float heroAggroBonusRange = 1.4f;
        [SerializeField] private float heroAggroSameLaneTolerance = 2.1f;
        [SerializeField] private float heroAggroFrontlineLeadRequired = 4.6f;
        [SerializeField] private float rangedRearBuffer = 3.8f;
        [SerializeField] private float supportRearBuffer = 4.8f;
        [SerializeField] private float formationHoldTolerance = 0.55f;
        [SerializeField] private float retreatSpeedMultiplier = 0.82f;
        [SerializeField] private float rangedStagingDepth = 11.5f;
        [SerializeField] private float supportStagingDepth = 9.2f;
        [SerializeField] private float tankDamageReduction = 0.18f;
        [SerializeField] private float meleeHeroDamageMultiplier = 1.24f;
        [SerializeField] private float rangedStructureDamagePenalty = 0.72f;
        [SerializeField] private float rangedBaseDamagePenalty = 0.62f;

        private Collider cachedCollider;
        private Renderer[] cachedRenderers;
        private Color[] baseColors;
        private SummonData summonData;
        private bool isPlayerTeam;
        private float currentHP;
        private float nextActionTime;
        private bool isDying;
        private SummonUnit currentTarget;
        private SummonUnit currentHealTarget;
        private BattleStructure currentStructureTarget;
        private Transform opposingBase;
        private Vector3 moveDirection;
        private int opposingLayerMask;
        private int friendlyLayerMask;
        private PlayerController currentHeroTarget;
        private Coroutine flashRoutine;
        private Coroutine attackPulseRoutine;
        private Vector3 initialLocalScale;
        private float supportBuffExpiresAt;
        private float supportDamageMultiplier = 1f;
        private float supportMoveSpeedMultiplier = 1f;
        private Transform healthBarRoot;
        private Transform healthBarFill;
        private Renderer healthBarFillRenderer;

        public bool IsAlive => !isDying && currentHP > 0f;
        public bool IsPlayerTeam => isPlayerTeam;
        public float CurrentHP => currentHP;
        public float MaxHP => summonData != null ? summonData.maxHP : 0f;
        public int AssignedLaneIndex { get; private set; } = BattleLaneUtility.DefaultLaneCount / 2;

        private void Awake()
        {
            cachedCollider = GetComponent<Collider>();
            cachedRenderers = GetComponentsInChildren<Renderer>(true);
            baseColors = new Color[cachedRenderers.Length];
            for (int index = 0; index < cachedRenderers.Length; index++)
            {
                Renderer renderer = cachedRenderers[index];
                baseColors[index] = renderer != null && renderer.material.HasProperty("_Color")
                    ? renderer.material.color
                    : Color.white;
            }

            initialLocalScale = transform.localScale;
            EnsureHealthBar();
        }

        public void Init(SummonData data, bool belongsToPlayerTeam)
        {
            summonData = data;
            isPlayerTeam = belongsToPlayerTeam;
            currentHP = summonData != null ? summonData.maxHP : 0f;
            moveDirection = belongsToPlayerTeam ? Vector3.forward : Vector3.back;
            opposingBase = BattleManager.Instance != null ? BattleManager.Instance.GetOpposingBaseTransform(belongsToPlayerTeam) : null;
            opposingLayerMask = LayerMask.GetMask(belongsToPlayerTeam ? "EnemySummon" : "PlayerSummon");
            friendlyLayerMask = LayerMask.GetMask(belongsToPlayerTeam ? "PlayerSummon" : "EnemySummon");

            int layer = LayerMask.NameToLayer(belongsToPlayerTeam ? "PlayerSummon" : "EnemySummon");
            if (layer >= 0)
            {
                gameObject.layer = layer;
            }

            gameObject.name = data != null ? data.summonName : "SummonUnit";
            initialLocalScale = transform.localScale;
            supportBuffExpiresAt = 0f;
            supportDamageMultiplier = 1f;
            supportMoveSpeedMultiplier = 1f;
            currentHeroTarget = belongsToPlayerTeam ? null : BattleManager.Instance != null ? BattleManager.Instance.PlayerController : null;
            AssignedLaneIndex = BattleManager.Instance != null
                ? BattleManager.Instance.GetNearestLaneIndex(transform.position.x)
                : BattleLaneUtility.DefaultLaneCount / 2;
            AlignToBattleFloor();
            UpdateHealthBar();
            BattlePresentationController.Instance?.SpawnBurst(
                transform.position + Vector3.up * 0.8f,
                belongsToPlayerTeam ? new Color(0.46f, 0.94f, 1f, 1f) : new Color(1f, 0.56f, 0.38f, 1f),
                14,
                0.15f,
                2.4f,
                0.08f,
                0.38f);
        }

        public void SetAssignedLane(int laneIndex)
        {
            AssignedLaneIndex = BattleLaneUtility.ClampLaneIndex(laneIndex);
        }

        private void Update()
        {
            if (summonData == null || !IsAlive)
            {
                return;
            }

            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Battle)
            {
                return;
            }

            if (ShouldSupportAllies())
            {
                currentHealTarget = AcquireHealTarget();
                if (currentHealTarget != null)
                {
                    RotateTowards(currentHealTarget.transform.position);
                    TryHealTarget();
                    return;
                }
            }

            if (currentTarget == null || !currentTarget.IsAlive || !IsTargetInRange(currentTarget.transform.position))
            {
                currentTarget = AcquireTarget();
            }

            if (currentStructureTarget == null || currentStructureTarget.IsDestroyed || !IsTargetInRange(currentStructureTarget.transform.position))
            {
                currentStructureTarget = AcquireStructureTarget();
            }

            if (!isPlayerTeam)
            {
                if (currentTarget == null && currentStructureTarget == null)
                {
                    if (currentHeroTarget == null && BattleManager.Instance != null)
                    {
                        currentHeroTarget = BattleManager.Instance.PlayerController;
                    }

                    if (currentHeroTarget == null || !CanTargetHero(currentHeroTarget))
                    {
                        currentHeroTarget = AcquireHeroTarget();
                    }
                }
                else
                {
                    currentHeroTarget = null;
                }
            }

            if (currentTarget != null)
            {
                RotateTowards(currentTarget.transform.position);
                TryAttackTarget();
                return;
            }

            if (currentStructureTarget != null)
            {
                RotateTowards(currentStructureTarget.transform.position);
                TryAttackStructure();
                return;
            }

            if (currentHeroTarget != null)
            {
                RotateTowards(currentHeroTarget.transform.position);
                TryAttackHero();
                return;
            }

            if (TryGetForwardStructureGate(out BattleStructure forwardStructureGate))
            {
                if (TryMoveTowardForwardStructure(forwardStructureGate))
                {
                    RotateTowards(forwardStructureGate.transform.position);
                    return;
                }

                currentStructureTarget = forwardStructureGate;
                RotateTowards(currentStructureTarget.transform.position);
                TryAttackStructure();
                return;
            }

            if (IsOpposingBaseInRange())
            {
                RotateTowards(opposingBase.position);
                TryAttackBase();
                return;
            }

            if (TryMaintainFormation(out Vector3 formationPosition))
            {
                transform.position = formationPosition;
                RotateTowards(formationPosition + moveDirection);
                return;
            }

            Vector3 nextPosition = transform.position + (moveDirection * GetEffectiveMoveSpeed() * Time.deltaTime);
            nextPosition.y = transform.position.y;
            transform.position = nextPosition;
            RotateTowards(transform.position + moveDirection);
        }

        public void TakeDamage(float amount)
        {
            if (amount <= 0f || !IsAlive)
            {
                return;
            }

            if (summonData != null && summonData.summonType == SummonType.Tank)
            {
                amount *= Mathf.Clamp01(1f - tankDamageReduction);
            }

            currentHP = Mathf.Max(0f, currentHP - amount);
            PlayColorFlash(hitFlashColor);
            UpdateHealthBar();
            BattlePresentationController.Instance?.ShowWorldText(transform.position + new Vector3(0f, 1.6f, 0f), $"-{Mathf.CeilToInt(amount)}", hitFlashColor, 2.8f, 0.48f);
            BattlePresentationController.Instance?.SpawnBurst(transform.position + Vector3.up * 0.8f, hitFlashColor, 8, 0.1f, 1.8f, 0.05f, 0.25f);
            if (currentHP <= 0f)
            {
                StartCoroutine(DieRoutine());
            }
        }

        public void Heal(float amount)
        {
            if (amount <= 0f || !IsAlive || summonData == null)
            {
                return;
            }

            float previousHp = currentHP;
            currentHP = Mathf.Min(summonData.maxHP, currentHP + amount);
            if (currentHP > previousHp)
            {
                UpdateHealthBar();
                PlayColorFlash(healFlashColor);
                if (amount >= 8f)
                {
                    BattlePresentationController.Instance?.ShowWorldText(transform.position + new Vector3(0f, 1.8f, 0f), $"+{Mathf.CeilToInt(amount)}", healFlashColor, 3.2f, 0.68f);
                }
            }
        }

        public void ApplyHeroSupport(float duration, float damageMultiplier, float moveSpeedMultiplier, float immediateHeal)
        {
            if (!IsAlive)
            {
                return;
            }

            supportBuffExpiresAt = Mathf.Max(supportBuffExpiresAt, Time.time + Mathf.Max(0.1f, duration));
            supportDamageMultiplier = Mathf.Max(supportDamageMultiplier, Mathf.Max(1f, damageMultiplier));
            supportMoveSpeedMultiplier = Mathf.Max(supportMoveSpeedMultiplier, Mathf.Max(1f, moveSpeedMultiplier));

            if (immediateHeal > 0f)
            {
                Heal(immediateHeal);
            }
        }

        private SummonUnit AcquireTarget()
        {
            Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, summonData.attackRange, opposingLayerMask);
            float closestDistance = float.MaxValue;
            SummonUnit bestTarget = null;

            for (int index = 0; index < nearbyColliders.Length; index++)
            {
                SummonUnit target = nearbyColliders[index].GetComponentInParent<SummonUnit>();
                if (target == null || !target.IsAlive)
                {
                    continue;
                }

                if (!IsSameLanePosition(target.transform.position) || !IsForwardTarget(target.transform.position))
                {
                    continue;
                }

                float sqrDistance = (target.transform.position - transform.position).sqrMagnitude;
                if (sqrDistance >= closestDistance)
                {
                    continue;
                }

                closestDistance = sqrDistance;
                bestTarget = target;
            }

            return bestTarget;
        }

        private BattleStructure AcquireStructureTarget()
        {
            if (summonData == null)
            {
                return null;
            }

            BattleManager battleManager = BattleManager.Instance;
            if (isPlayerTeam && battleManager != null &&
                battleManager.TryGetPreferredInterventionStructure(AssignedLaneIndex, out BattleStructure priorityStructure))
            {
                Vector3 priorityOffset = priorityStructure.transform.position - transform.position;
                if (priorityOffset.sqrMagnitude <= (summonData.attackRange + 0.35f) * (summonData.attackRange + 0.35f) &&
                    IsForwardTarget(priorityStructure.transform.position))
                {
                    return priorityStructure;
                }
            }

            BattleStructure blocker = BattleStructure.FindClosestActiveInLane(
                transform.position,
                summonData.attackRange + 0.35f,
                AssignedLaneIndex,
                isPlayerTeam,
                BattleStructureRole.FrontlineBlocker,
                0.25f);
            if (blocker != null)
            {
                return blocker;
            }

            return BattleStructure.FindClosestActiveInLaneAnyRole(
                transform.position,
                summonData.attackRange + 0.35f,
                AssignedLaneIndex,
                isPlayerTeam,
                0.25f);
        }

        private bool TryGetForwardStructureGate(out BattleStructure structure)
        {
            structure = null;
            if (summonData == null || BattleManager.Instance == null)
            {
                return false;
            }

            structure = BattleStructure.FindNearestActiveInLaneAlongAdvance(AssignedLaneIndex, isPlayerTeam);
            return structure != null && !structure.IsDestroyed && IsForwardTarget(structure.transform.position);
        }

        private bool TryMoveTowardForwardStructure(BattleStructure structure)
        {
            if (structure == null || summonData == null)
            {
                return false;
            }

            Vector3 currentPosition = transform.position;
            Vector3 targetPosition = structure.transform.position;
            float stopRange = Mathf.Max(0.8f, summonData.attackRange - 0.1f);
            float signedForwardOffset = isPlayerTeam
                ? targetPosition.z - currentPosition.z
                : currentPosition.z - targetPosition.z;
            if (signedForwardOffset <= stopRange)
            {
                return false;
            }

            float moveStep = GetEffectiveMoveSpeed() * Time.deltaTime;
            float targetX = BattleManager.Instance != null
                ? BattleManager.Instance.GetLaneCenterX(AssignedLaneIndex)
                : targetPosition.x;
            float targetZ = isPlayerTeam
                ? targetPosition.z - stopRange
                : targetPosition.z + stopRange;
            Vector3 nextPosition = new(
                Mathf.MoveTowards(currentPosition.x, targetX, moveStep),
                currentPosition.y,
                Mathf.MoveTowards(currentPosition.z, targetZ, moveStep));
            transform.position = nextPosition;
            return true;
        }

        private SummonUnit AcquireHealTarget()
        {
            Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, summonData.healRadius, friendlyLayerMask);
            float lowestHealthRatio = 1f;
            SummonUnit bestTarget = null;

            for (int index = 0; index < nearbyColliders.Length; index++)
            {
                SummonUnit ally = nearbyColliders[index].GetComponentInParent<SummonUnit>();
                if (ally == null || ally == this || !ally.IsAlive || ally.MaxHP <= 0f)
                {
                    continue;
                }

                float healthRatio = ally.CurrentHP / ally.MaxHP;
                if (healthRatio >= 0.97f || healthRatio >= lowestHealthRatio)
                {
                    continue;
                }

                lowestHealthRatio = healthRatio;
                bestTarget = ally;
            }

            return bestTarget;
        }

        private PlayerController AcquireHeroTarget()
        {
            if (isPlayerTeam || summonData == null || summonData.summonType == SummonType.Support)
            {
                return null;
            }

            PlayerController playerController = BattleManager.Instance != null ? BattleManager.Instance.PlayerController : null;
            return CanTargetHero(playerController) ? playerController : null;
        }

        private bool CanTargetHero(PlayerController playerController)
        {
            if (playerController == null || playerController.CurrentHP <= 0f || summonData == null || summonData.summonType != SummonType.Melee)
            {
                return false;
            }

            Vector3 heroOffset = playerController.transform.position - transform.position;
            heroOffset.y = 0f;
            float heroAggroRange = summonData.attackRange + heroAggroBonusRange;
            if (heroOffset.sqrMagnitude > heroAggroRange * heroAggroRange || Mathf.Abs(heroOffset.x) > heroAggroSameLaneTolerance)
            {
                return false;
            }

            if (BattleManager.Instance != null && BattleManager.Instance.TryGetPlayerTerritoryState(out BattleManager.PlayerTerritoryState territoryState))
            {
                if (territoryState.OverextendDistance > 0.2f || territoryState.IsInEnemyBaseZone)
                {
                    return true;
                }
            }

            if (BattleManager.Instance != null && BattleManager.Instance.TryGetFrontlineState(out BattleManager.FrontlineState frontlineState))
            {
                return playerController.transform.position.z >= frontlineState.PlayerFrontZ + heroAggroFrontlineLeadRequired;
            }

            return playerController.transform.position.z >= transform.position.z - 0.8f;
        }

        private bool IsTargetInRange(Vector3 targetPosition)
        {
            float range = summonData.attackRange;
            return (targetPosition - transform.position).sqrMagnitude <= range * range;
        }

        private bool IsOpposingBaseInRange()
        {
            return opposingBase != null && IsTargetInRange(opposingBase.position);
        }

        private bool ShouldSupportAllies()
        {
            return summonData != null && summonData.healAmount > 0f && summonData.healRadius > 0f;
        }

        private void TryAttackTarget()
        {
            if (Time.time < nextActionTime || currentTarget == null)
            {
                return;
            }

            nextActionTime = Time.time + summonData.attackCooldown;
            PlayAttackPulse();
            ApplyDamageToTarget(currentTarget, GetEffectiveAttackDamage());
            PlayActionImpact(currentTarget.transform.position, new Color(1f, 0.88f, 0.75f, 1f));
        }

        private void TryAttackStructure()
        {
            if (Time.time < nextActionTime || currentStructureTarget == null)
            {
                return;
            }

            nextActionTime = Time.time + summonData.attackCooldown;
            PlayAttackPulse();
            float multiplier = summonData.structureDamageMultiplier > 0f ? summonData.structureDamageMultiplier : 1f;
            currentStructureTarget.TakeDamage(
                GetEffectiveAttackDamage() *
                multiplier *
                ResolveRoleStructureDamageMultiplier() *
                ResolveObjectivePressureDamageMultiplier(currentStructureTarget),
                isPlayerTeam);
            PlayActionImpact(currentStructureTarget.transform.position, new Color(1f, 0.82f, 0.45f, 1f));
        }

        private void TryAttackBase()
        {
            if (Time.time < nextActionTime)
            {
                return;
            }

            nextActionTime = Time.time + summonData.attackCooldown;
            PlayAttackPulse();
            if (BattleManager.Instance == null)
            {
                return;
            }

            float multiplier = summonData.baseDamageMultiplier > 0f ? summonData.baseDamageMultiplier : 1f;
            float baseDamage = GetEffectiveAttackDamage() * multiplier * ResolveRoleBaseDamageMultiplier();
            if (isPlayerTeam)
            {
                BattleManager.Instance.DamageEnemyBase(baseDamage);
                PlayActionImpact(opposingBase.position, new Color(0.52f, 0.95f, 1f, 1f));
                return;
            }

            BattleManager.Instance.DamagePlayerBase(baseDamage);
            PlayActionImpact(opposingBase.position, new Color(1f, 0.52f, 0.42f, 1f));
        }

        private void TryAttackHero()
        {
            if (Time.time < nextActionTime || currentHeroTarget == null || !CanTargetHero(currentHeroTarget))
            {
                return;
            }

            nextActionTime = Time.time + summonData.attackCooldown;
            PlayAttackPulse();
            currentHeroTarget.TakeDamage(GetEffectiveAttackDamage() * meleeHeroDamageMultiplier);
            PlayActionImpact(currentHeroTarget.transform.position, new Color(1f, 0.7f, 0.62f, 1f));
        }

        private void TryHealTarget()
        {
            if (Time.time < nextActionTime || currentHealTarget == null)
            {
                return;
            }

            float cooldown = summonData.healCooldown > 0f ? summonData.healCooldown : summonData.attackCooldown;
            nextActionTime = Time.time + cooldown;
            PlayAttackPulse();
            currentHealTarget.Heal(summonData.healAmount);
            PlayActionImpact(currentHealTarget.transform.position, healFlashColor);
        }

        private void ApplyDamageToTarget(SummonUnit primaryTarget, float amount)
        {
            if (primaryTarget == null)
            {
                return;
            }

            primaryTarget.TakeDamage(amount);

            if (summonData.splashRadius <= 0.01f)
            {
                return;
            }

            Collider[] splashTargets = Physics.OverlapSphere(primaryTarget.transform.position, summonData.splashRadius, opposingLayerMask);
            for (int index = 0; index < splashTargets.Length; index++)
            {
                SummonUnit splashTarget = splashTargets[index].GetComponentInParent<SummonUnit>();
                if (splashTarget == null || splashTarget == primaryTarget || !splashTarget.IsAlive)
                {
                    continue;
                }

                splashTarget.TakeDamage(amount * 0.55f);
            }
        }

        private IEnumerator DieRoutine()
        {
            if (isDying)
            {
                yield break;
            }

            isDying = true;
            if (attackPulseRoutine != null)
            {
                StopCoroutine(attackPulseRoutine);
                attackPulseRoutine = null;
            }

            if (cachedCollider != null)
            {
                cachedCollider.enabled = false;
            }

            if (healthBarRoot != null)
            {
                healthBarRoot.gameObject.SetActive(false);
            }

            BattlePresentationController.Instance?.SpawnBurst(transform.position + Vector3.up, isPlayerTeam ? new Color(0.45f, 0.92f, 1f, 1f) : new Color(1f, 0.58f, 0.42f, 1f), 18, 0.18f, 3f, 0.12f, 0.46f);

            Vector3 startScale = transform.localScale;
            float elapsed = 0f;
            while (elapsed < destroyDelay)
            {
                elapsed += Time.deltaTime;
                float normalizedTime = destroyDelay <= 0.001f ? 1f : Mathf.Clamp01(elapsed / destroyDelay);
                transform.localScale = Vector3.Lerp(startScale, Vector3.zero, normalizedTime);
                yield return null;
            }

            Destroy(gameObject);
        }

        private void PlayAttackPulse()
        {
            if (!gameObject.activeInHierarchy || isDying)
            {
                return;
            }

            if (attackPulseRoutine != null)
            {
                StopCoroutine(attackPulseRoutine);
            }

            attackPulseRoutine = StartCoroutine(AttackPulseRoutine());
        }

        private IEnumerator AttackPulseRoutine()
        {
            Vector3 pulseScale = initialLocalScale * attackPulseScaleMultiplier;
            float elapsed = 0f;

            while (elapsed < attackPulseDuration)
            {
                elapsed += Time.deltaTime;
                float halfDuration = Mathf.Max(attackPulseDuration * 0.5f, 0.001f);
                if (elapsed <= halfDuration)
                {
                    float blend = Mathf.Clamp01(elapsed / halfDuration);
                    transform.localScale = Vector3.Lerp(initialLocalScale, pulseScale, blend);
                }
                else
                {
                    float blend = Mathf.Clamp01((elapsed - halfDuration) / halfDuration);
                    transform.localScale = Vector3.Lerp(pulseScale, initialLocalScale, blend);
                }

                yield return null;
            }

            transform.localScale = initialLocalScale;
            attackPulseRoutine = null;
        }

        private void PlayColorFlash(Color color)
        {
            if (!gameObject.activeInHierarchy || cachedRenderers == null || cachedRenderers.Length == 0)
            {
                return;
            }

            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
            }

            flashRoutine = StartCoroutine(ColorFlashRoutine(color));
        }

        private IEnumerator ColorFlashRoutine(Color color)
        {
            SetRenderersColor(color);
            yield return new WaitForSeconds(hitFlashDuration);
            RestoreBaseColors();
            flashRoutine = null;
        }

        private void SetRenderersColor(Color color)
        {
            for (int index = 0; index < cachedRenderers.Length; index++)
            {
                Renderer renderer = cachedRenderers[index];
                if (renderer == null || !renderer.material.HasProperty("_Color"))
                {
                    continue;
                }

                renderer.material.color = color;
            }
        }

        private void RestoreBaseColors()
        {
            for (int index = 0; index < cachedRenderers.Length; index++)
            {
                Renderer renderer = cachedRenderers[index];
                if (renderer == null || !renderer.material.HasProperty("_Color"))
                {
                    continue;
                }

                renderer.material.color = baseColors[index];
            }
        }

        private void AlignToBattleFloor()
        {
            if (cachedCollider == null)
            {
                return;
            }

            float bottomY = cachedCollider.bounds.min.y;
            float floorY = 0f;
            float liftAmount = floorY - bottomY;
            if (Mathf.Abs(liftAmount) <= 0.001f)
            {
                return;
            }

            transform.position += Vector3.up * liftAmount;
        }

        private bool TryMaintainFormation(out Vector3 nextPosition)
        {
            nextPosition = transform.position;
            if (summonData == null || (summonData.summonType != SummonType.Ranged && summonData.summonType != SummonType.Support))
            {
                return false;
            }

            if (!TryGetFormationAnchor(out Vector3 anchorPosition))
            {
                return false;
            }

            float rearBuffer = summonData.summonType == SummonType.Support ? supportRearBuffer : rangedRearBuffer;
            float desiredZ = isPlayerTeam ? anchorPosition.z - rearBuffer : anchorPosition.z + rearBuffer;
            float zDelta = desiredZ - transform.position.z;
            float xDelta = anchorPosition.x - transform.position.x;

            if (Mathf.Abs(zDelta) <= formationHoldTolerance && Mathf.Abs(xDelta) <= 0.35f)
            {
                return true;
            }

            float moveSpeedMultiplier = zDelta < 0f ? retreatSpeedMultiplier : 1f;
            Vector3 desiredStep = new(
                Mathf.Clamp(xDelta, -1f, 1f),
                0f,
                Mathf.Clamp(zDelta, -1f, 1f));
            nextPosition = transform.position + (desiredStep * GetEffectiveMoveSpeed() * moveSpeedMultiplier * Time.deltaTime);
            nextPosition.y = transform.position.y;
            return true;
        }

        private bool TryGetFormationAnchor(out Vector3 anchorPosition)
        {
            anchorPosition = transform.position;
            SummonUnit[] alliedUnits = FindObjectsByType<SummonUnit>(FindObjectsSortMode.None);
            bool foundFrontliner = false;
            float bestFrontZ = isPlayerTeam ? float.MinValue : float.MaxValue;
            float weightedX = 0f;
            float weightTotal = 0f;

            for (int index = 0; index < alliedUnits.Length; index++)
            {
                SummonUnit alliedUnit = alliedUnits[index];
                if (alliedUnit == null || alliedUnit == this || !alliedUnit.IsAlive || alliedUnit.IsPlayerTeam != isPlayerTeam)
                {
                    continue;
                }

                if (alliedUnit.AssignedLaneIndex != AssignedLaneIndex)
                {
                    continue;
                }

                if (alliedUnit.summonData == null || alliedUnit.summonData.summonType == SummonType.Support)
                {
                    continue;
                }

                bool isFrontliner = alliedUnit.summonData.summonType == SummonType.Tank || alliedUnit.summonData.summonType == SummonType.Melee;
                float unitWeight = isFrontliner ? 1.6f : 0.8f;
                if (isPlayerTeam)
                {
                    if (alliedUnit.transform.position.z > bestFrontZ)
                    {
                        bestFrontZ = alliedUnit.transform.position.z;
                    }
                }
                else if (alliedUnit.transform.position.z < bestFrontZ)
                {
                    bestFrontZ = alliedUnit.transform.position.z;
                }

                weightedX += alliedUnit.transform.position.x * unitWeight;
                weightTotal += unitWeight;
                foundFrontliner |= isFrontliner;
            }

            if (!foundFrontliner || weightTotal <= 0.001f)
            {
                return TryGetStagingAnchor(out anchorPosition);
            }

            anchorPosition = new Vector3(weightedX / weightTotal, transform.position.y, bestFrontZ);
            return true;
        }

        private bool TryGetStagingAnchor(out Vector3 anchorPosition)
        {
            anchorPosition = transform.position;
            if (summonData == null || (summonData.summonType != SummonType.Ranged && summonData.summonType != SummonType.Support))
            {
                return false;
            }

            Transform summonSpawn = BattleManager.Instance != null ? BattleManager.Instance.GetSummonSpawnPoint(isPlayerTeam) : null;
            if (summonSpawn == null)
            {
                return false;
            }

            float stagingDepth = summonData.summonType == SummonType.Support ? supportStagingDepth : rangedStagingDepth;
            float anchorZ = isPlayerTeam
                ? summonSpawn.position.z + stagingDepth
                : summonSpawn.position.z - stagingDepth;
            float anchorX = BattleManager.Instance != null
                ? BattleManager.Instance.GetLaneCenterX(AssignedLaneIndex)
                : summonSpawn.position.x;
            anchorPosition = new Vector3(anchorX, transform.position.y, anchorZ);
            return true;
        }

        private bool IsSameLanePosition(Vector3 targetPosition)
        {
            BattleManager battleManager = BattleManager.Instance;
            return battleManager == null || battleManager.GetNearestLaneIndex(targetPosition.x) == AssignedLaneIndex;
        }

        private bool IsForwardTarget(Vector3 targetPosition)
        {
            float signedForwardOffset = isPlayerTeam
                ? targetPosition.z - transform.position.z
                : transform.position.z - targetPosition.z;
            return signedForwardOffset >= -0.25f;
        }

        private void RotateTowards(Vector3 targetPosition)
        {
            Vector3 lookDirection = targetPosition - transform.position;
            lookDirection.y = 0f;
            if (lookDirection.sqrMagnitude <= 0.001f)
            {
                return;
            }

            transform.rotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
        }

        private void PlayActionImpact(Vector3 targetPosition, Color color)
        {
            BattlePresentationController.Instance?.SpawnBurst(targetPosition + Vector3.up * 0.7f, color, 8, 0.11f, 1.6f, 0.05f, 0.22f);
        }

        private float GetEffectiveAttackDamage()
        {
            return summonData == null ? 0f : summonData.attackDamage * GetCurrentSupportDamageMultiplier();
        }

        private float GetEffectiveMoveSpeed()
        {
            return summonData == null ? 0f : summonData.moveSpeed * GetCurrentSupportMoveMultiplier();
        }

        private float GetCurrentSupportDamageMultiplier()
        {
            if (Time.time <= supportBuffExpiresAt)
            {
                return Mathf.Max(1f, supportDamageMultiplier);
            }

            supportDamageMultiplier = 1f;
            return 1f;
        }

        private float GetCurrentSupportMoveMultiplier()
        {
            if (Time.time <= supportBuffExpiresAt)
            {
                return Mathf.Max(1f, supportMoveSpeedMultiplier);
            }

            supportMoveSpeedMultiplier = 1f;
            return 1f;
        }

        private float ResolveRoleStructureDamageMultiplier()
        {
            if (summonData == null)
            {
                return 1f;
            }

            return summonData.summonType switch
            {
                SummonType.Ranged => rangedStructureDamagePenalty,
                SummonType.Support => 0.45f,
                _ => 1f
            };
        }

        private float ResolveObjectivePressureDamageMultiplier(BattleStructure structure)
        {
            if (summonData == null || structure == null)
            {
                return 1f;
            }

            bool isBreaker = summonData.summonType == SummonType.Melee && summonData.structureDamageMultiplier >= 1.8f;
            if (structure.Role == BattleStructureRole.FrontlineBlocker)
            {
                if (isBreaker)
                {
                    return 1.35f;
                }

                if (summonData.summonType == SummonType.Melee)
                {
                    return 1.16f;
                }
            }

            return 1f;
        }

        private float ResolveRoleBaseDamageMultiplier()
        {
            if (summonData == null)
            {
                return 1f;
            }

            return summonData.summonType switch
            {
                SummonType.Ranged => rangedBaseDamagePenalty,
                SummonType.Support => 0.35f,
                _ => 1f
            };
        }

        private void LateUpdate()
        {
            UpdateHealthBarFacing();
        }

        private void EnsureHealthBar()
        {
            if (healthBarRoot != null)
            {
                return;
            }

            GameObject rootObject = new("WorldHealthBar");
            rootObject.transform.SetParent(transform, false);
            rootObject.transform.localPosition = healthBarOffset;
            healthBarRoot = rootObject.transform;

            GameObject backObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            backObject.name = "Back";
            backObject.transform.SetParent(healthBarRoot, false);
            backObject.transform.localScale = new Vector3(0.92f, 0.1f, 0.05f);
            Destroy(backObject.GetComponent<Collider>());
            Renderer backRenderer = backObject.GetComponent<Renderer>();
            if (backRenderer != null)
            {
                backRenderer.material = new Material(Shader.Find("Sprites/Default"));
                backRenderer.material.color = new Color(0f, 0f, 0f, 0.5f);
            }

            GameObject fillObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fillObject.name = "Fill";
            fillObject.transform.SetParent(healthBarRoot, false);
            fillObject.transform.localScale = new Vector3(0.84f, 0.06f, 0.03f);
            fillObject.transform.localPosition = new Vector3(-0.04f, 0f, -0.01f);
            Destroy(fillObject.GetComponent<Collider>());
            healthBarFill = fillObject.transform;
            healthBarFillRenderer = fillObject.GetComponent<Renderer>();
            if (healthBarFillRenderer != null)
            {
                healthBarFillRenderer.material = new Material(Shader.Find("Sprites/Default"));
            }
        }

        private void UpdateHealthBar()
        {
            if (healthBarFill == null || summonData == null)
            {
                return;
            }

            float normalized = summonData.maxHP <= 0.001f ? 0f : Mathf.Clamp01(currentHP / summonData.maxHP);
            healthBarFill.localScale = new Vector3(Mathf.Max(0.035f, 0.84f * normalized), 0.06f, 0.03f);
            healthBarFill.localPosition = new Vector3((-0.42f) + (0.42f * normalized), 0f, -0.01f);
            if (healthBarFillRenderer != null)
            {
                Color healthyColor = isPlayerTeam ? new Color(0.36f, 0.95f, 1f, 1f) : new Color(1f, 0.68f, 0.35f, 1f);
                Color criticalColor = new(1f, 0.34f, 0.34f, 1f);
                healthBarFillRenderer.material.color = Color.Lerp(criticalColor, healthyColor, normalized);
            }
        }

        private void UpdateHealthBarFacing()
        {
            if (healthBarRoot == null || Camera.main == null)
            {
                return;
            }

            healthBarRoot.forward = Camera.main.transform.forward;
        }
    }
}
