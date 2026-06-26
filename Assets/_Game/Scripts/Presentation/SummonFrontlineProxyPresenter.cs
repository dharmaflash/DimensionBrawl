using DimensionBrawl.Combat;
using UnityEngine;

namespace DimensionBrawl.Presentation
{
    [DisallowMultipleComponent]
    public sealed class SummonFrontlineProxyPresenter : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        [SerializeField] private SummonFrontlineProxy proxy;
        [SerializeField] private SummonFrontlineClash clash;
        [SerializeField] private CombatHealth health;
        [SerializeField] private Animator animator;
        [SerializeField] private string moveSpeedParameter = "MoveSpeed";
        [SerializeField] private string spawnTrigger = "EliteSummonPackage";
        [SerializeField] private string attackTrigger = "Attack";
        [SerializeField] private string hitTrigger = "Hit";
        [SerializeField] private string deathTrigger = "Death";
        [SerializeField, Min(0f)] private float animatorMoveSpeedScale = 1f;
        [SerializeField] private Transform pulseRoot;
        [SerializeField] private Renderer[] actorRenderers = System.Array.Empty<Renderer>();

        [Header("VFX Cues")]
        [SerializeField] private CombatVfxCuePlayer cuePlayer;
        [SerializeField] private Transform vfxAnchor;
        [SerializeField] private Transform vfxDirectionTarget;
        [SerializeField] private CombatVfxCueId entryCueId = CombatVfxCueId.EliteSummonSignal;
        [SerializeField] private CombatVfxCueId attackCueId = CombatVfxCueId.EnemyAttackActive;
        [SerializeField] private CombatVfxCueId clashCueId = CombatVfxCueId.EliteShieldSignal;
        [SerializeField] private CombatVfxCueId damageCueId = CombatVfxCueId.EnemyHit;
        [SerializeField] private CombatVfxCueId deathCueId = CombatVfxCueId.EnemyDeath;
        [SerializeField, Min(0f)] private float entryCueIntensity = 0.95f;
        [SerializeField, Min(0f)] private float attackCueIntensity = 0.9f;
        [SerializeField, Min(0f)] private float clashCueIntensity = 1.0f;
        [SerializeField, Min(0f)] private float damageCueIntensity = 0.9f;
        [SerializeField, Range(0.1f, 1f)] private float pressureDamageCueScale = 0.64f;
        [SerializeField, Min(0f)] private float deathCueIntensity = 1.05f;
        [SerializeField, Min(0f)] private float tierCueIntensityStep = 0.1f;

        [SerializeField] private Color tierOneColor = new Color(0.24f, 1f, 0.78f, 0.78f);
        [SerializeField] private Color tierTwoColor = new Color(0.38f, 0.74f, 1f, 0.9f);
        [SerializeField] private Color tierThreeColor = new Color(1f, 0.76f, 0.24f, 1f);
        [SerializeField] private Color flashColor = new Color(1f, 1f, 1f, 1f);
        [SerializeField] private Color clashFlashColor = new Color(1f, 0.9f, 0.38f, 1f);
        [SerializeField] private Color attackFlashColor = new Color(1f, 0.74f, 0.24f, 1f);
        [SerializeField] private Color damageFlashColor = new Color(1f, 0.24f, 0.18f, 1f);
        [SerializeField] private Color deathFlashColor = new Color(0.08f, 0.08f, 0.08f, 1f);
        [SerializeField, Min(0f)] private float entryFlashSeconds = 0.22f;
        [SerializeField, Min(0f)] private float impactFlashSeconds = 0.18f;
        [SerializeField, Min(0f)] private float clashFlashSeconds = 0.14f;
        [SerializeField, Min(0f)] private float attackFlashSeconds = 0.12f;
        [SerializeField, Min(0f)] private float damageFlashSeconds = 0.16f;
        [SerializeField, Min(0f)] private float deathFlashSeconds = 0.22f;
        [SerializeField, Range(0.2f, 1f)] private float impactFlashProgress = 0.86f;
        [SerializeField, Min(0.01f)] private float pulseSpeed = 8f;
        [SerializeField, Min(0f)] private float pulseScale = 0.08f;
        [SerializeField, Min(0f)] private float tierScaleStep = 0.18f;
        [SerializeField, Min(0f)] private float flashScale = 0.22f;
        [SerializeField, Min(0f)] private float clashFlashScale = 0.16f;
        [SerializeField, Min(0f)] private float attackFlashScale = 0.14f;
        [SerializeField, Min(0f)] private float damageFlashScale = 0.18f;
        [SerializeField, Min(0f)] private float deathFlashScale = 0.28f;

        private MaterialPropertyBlock propertyBlock;
        private Vector3 pulseBaseScale = Vector3.one;
        private float entryFlashTimer;
        private float impactFlashTimer;
        private float clashFlashTimer;
        private float attackFlashTimer;
        private float damageFlashTimer;
        private float deathFlashTimer;
        private bool wasActive;
        private bool wasAttacking;
        private bool subscribedToHealth;
        private bool impactFlashedThisActivation;
        private int lastObservedTier;
        private int lastObservedClashCount;
        private int entryFlashCount;
        private int impactFlashCount;
        private int clashFlashCount;
        private int attackFlashCount;
        private int damageFlashCount;
        private int deathFlashCount;
        private int animatorSpawnTriggerCount;
        private int animatorAttackTriggerCount;
        private int animatorHitTriggerCount;
        private int animatorDeathTriggerCount;
        private int animatorMoveSpeedSetCount;
        private int entryVfxCueRequestCount;
        private int attackVfxCueRequestCount;
        private int clashVfxCueRequestCount;
        private int damageVfxCueRequestCount;
        private int deathVfxCueRequestCount;
        private float lastDamageCueIntensity;
        private float lastDamageCuePolicyScale = 1f;
        private DamageResponsePolicy lastDamageResponsePolicy = DamageResponsePolicy.Default;
        private CombatControlLockPolicy lastDamageControlLockPolicy = CombatControlLockPolicy.InterruptAction;
        private bool lastDamageCueInterruptedAction;

        public SummonFrontlineProxy Proxy => proxy;
        public SummonFrontlineClash Clash => clash;
        public CombatHealth Health => health;
        public Animator Animator => animator;
        public string MoveSpeedParameter => moveSpeedParameter;
        public string SpawnTrigger => spawnTrigger;
        public string AttackTrigger => attackTrigger;
        public string HitTrigger => hitTrigger;
        public string DeathTrigger => deathTrigger;
        public CombatVfxCuePlayer CuePlayer => cuePlayer;
        public Transform VfxAnchor => vfxAnchor;
        public Transform VfxDirectionTarget => vfxDirectionTarget;
        public CombatVfxCueId EntryCueId => entryCueId;
        public CombatVfxCueId AttackCueId => attackCueId;
        public CombatVfxCueId ClashCueId => clashCueId;
        public CombatVfxCueId DamageCueId => damageCueId;
        public CombatVfxCueId DeathCueId => deathCueId;
        public Transform PulseRoot => pulseRoot;
        public int RendererCount => actorRenderers != null ? actorRenderers.Length : 0;
        public bool IsShowing => proxy != null && proxy.IsPresentationVisible;
        public int LastObservedTier => lastObservedTier;
        public int LastObservedClashCount => lastObservedClashCount;
        public int EntryFlashCount => entryFlashCount;
        public int ImpactFlashCount => impactFlashCount;
        public int ClashFlashCount => clashFlashCount;
        public int AttackFlashCount => attackFlashCount;
        public int DamageFlashCount => damageFlashCount;
        public int DeathFlashCount => deathFlashCount;
        public int AnimatorSpawnTriggerCount => animatorSpawnTriggerCount;
        public int AnimatorAttackTriggerCount => animatorAttackTriggerCount;
        public int AnimatorHitTriggerCount => animatorHitTriggerCount;
        public int AnimatorDeathTriggerCount => animatorDeathTriggerCount;
        public int AnimatorMoveSpeedSetCount => animatorMoveSpeedSetCount;
        public int EntryVfxCueRequestCount => entryVfxCueRequestCount;
        public int AttackVfxCueRequestCount => attackVfxCueRequestCount;
        public int ClashVfxCueRequestCount => clashVfxCueRequestCount;
        public int DamageVfxCueRequestCount => damageVfxCueRequestCount;
        public int DeathVfxCueRequestCount => deathVfxCueRequestCount;
        public float PressureDamageCueScale => pressureDamageCueScale;
        public float LastDamageCueIntensity => lastDamageCueIntensity;
        public float LastDamageCuePolicyScale => lastDamageCuePolicyScale;
        public DamageResponsePolicy LastDamageResponsePolicy => lastDamageResponsePolicy;
        public CombatControlLockPolicy LastDamageControlLockPolicy => lastDamageControlLockPolicy;
        public bool LastDamageCueInterruptedAction => lastDamageCueInterruptedAction;

        private void Awake()
        {
            propertyBlock ??= new MaterialPropertyBlock();
            ResolveReferences();

            if (pulseRoot != null)
            {
                pulseBaseScale = pulseRoot.localScale;
            }

            if (actorRenderers == null || actorRenderers.Length == 0)
            {
                actorRenderers = GetComponentsInChildren<Renderer>(includeInactive: true);
            }
        }

        private void OnEnable()
        {
            SubscribeHealth();
            RefreshNow();
        }

        private void OnDisable()
        {
            UnsubscribeHealth();
            wasActive = false;
            wasAttacking = false;
            SetAnimatorMoveSpeed(0f);
            SetPulseVisible(false);
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            if (entryFlashTimer > 0f)
            {
                entryFlashTimer = Mathf.Max(0f, entryFlashTimer - deltaTime);
            }

            if (impactFlashTimer > 0f)
            {
                impactFlashTimer = Mathf.Max(0f, impactFlashTimer - deltaTime);
            }

            if (clashFlashTimer > 0f)
            {
                clashFlashTimer = Mathf.Max(0f, clashFlashTimer - deltaTime);
            }

            if (attackFlashTimer > 0f)
            {
                attackFlashTimer = Mathf.Max(0f, attackFlashTimer - deltaTime);
            }

            if (damageFlashTimer > 0f)
            {
                damageFlashTimer = Mathf.Max(0f, damageFlashTimer - deltaTime);
            }

            if (deathFlashTimer > 0f)
            {
                deathFlashTimer = Mathf.Max(0f, deathFlashTimer - deltaTime);
            }

            RefreshNow();
        }

        public void ConfigurePresentation(
            SummonFrontlineProxy newProxy,
            Transform newPulseRoot,
            Renderer[] newActorRenderers)
        {
            UnsubscribeHealth();
            proxy = newProxy;
            health = proxy != null ? proxy.Health : null;
            pulseRoot = newPulseRoot;
            pulseBaseScale = pulseRoot != null ? pulseRoot.localScale : Vector3.one;
            actorRenderers = newActorRenderers ?? System.Array.Empty<Renderer>();
            SubscribeHealth();
            RefreshNow();
        }

        public void ConfigureAnimator(Animator newAnimator)
        {
            animator = newAnimator;
            RefreshNow();
        }

        public void ConfigureClashReference(SummonFrontlineClash newClash)
        {
            clash = newClash;
            lastObservedClashCount = clash != null ? clash.TotalClashCount : 0;
            RefreshNow();
        }

        public void ConfigureVfxCuePlayer(
            CombatVfxCuePlayer newCuePlayer,
            Transform newVfxAnchor,
            Transform newVfxDirectionTarget)
        {
            cuePlayer = newCuePlayer;
            vfxAnchor = newVfxAnchor;
            vfxDirectionTarget = newVfxDirectionTarget;
        }

        public void RefreshNow()
        {
            ResolveReferences();
            bool visible = proxy != null && proxy.IsPresentationVisible;
            bool active = proxy != null && proxy.IsActive;
            if (visible)
            {
                int tier = active
                    ? Mathf.Clamp(proxy.ActiveTier, 1, 3)
                    : Mathf.Clamp(lastObservedTier > 0 ? lastObservedTier : proxy.ActiveTier, 1, 3);
                if (!wasActive)
                {
                    entryFlashTimer = Mathf.Max(entryFlashTimer, entryFlashSeconds);
                    impactFlashedThisActivation = false;
                    entryFlashCount++;
                    if (PlayVfxCue(entryCueId, tier, entryCueIntensity))
                    {
                        entryVfxCueRequestCount++;
                    }

                    if (active && TriggerAnimator(spawnTrigger))
                    {
                        animatorSpawnTriggerCount++;
                    }
                }

                lastObservedTier = tier;
                ObserveClashCount();
                ObserveAttackState(active);
                if (active && !impactFlashedThisActivation && proxy.AdvanceProgress01 >= impactFlashProgress)
                {
                    impactFlashTimer = Mathf.Max(impactFlashTimer, impactFlashSeconds);
                    impactFlashedThisActivation = true;
                    impactFlashCount++;
                }

                SetPulseVisible(true);
                RefreshAnimator(active);
                RefreshVisual(tier);
            }
            else
            {
                SetAnimatorMoveSpeed(0f);
                SetPulseVisible(false);
            }

            wasActive = active;
            wasAttacking = active && proxy.CurrentState == SummonFrontlineProxyState.Attacking;
        }

        private void RefreshVisual(int tier)
        {
            Color tierColor = ResolveTierColor(tier);
            float flash = ResolveEntryImpactFlashWeight();
            float clashFlash = ResolveClashFlashWeight();
            float attackFlash = ResolveFlashWeight(attackFlashTimer, attackFlashSeconds);
            float damageFlash = ResolveFlashWeight(damageFlashTimer, damageFlashSeconds);
            float deathFlash = ResolveFlashWeight(deathFlashTimer, deathFlashSeconds);
            Color color = Color.Lerp(tierColor, flashColor, flash);
            color = Color.Lerp(color, clashFlashColor, clashFlash);
            color = Color.Lerp(color, attackFlashColor, attackFlash);
            color = Color.Lerp(color, damageFlashColor, damageFlash);
            color = Color.Lerp(color, deathFlashColor, deathFlash);
            ApplyColor(color);

            if (pulseRoot == null)
            {
                return;
            }

            float tierScale = 1f + (Mathf.Clamp(tier, 1, 3) - 1) * tierScaleStep;
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseScale;
            float scale = tierScale * (pulse
                + flash * flashScale
                + clashFlash * clashFlashScale
                + attackFlash * attackFlashScale
                + damageFlash * damageFlashScale
                + deathFlash * deathFlashScale);
            pulseRoot.localScale = pulseBaseScale * Mathf.Max(0.01f, scale);
        }

        private void ObserveClashCount()
        {
            if (clash == null)
            {
                return;
            }

            int currentClashCount = clash.TotalClashCount;
            if (currentClashCount > lastObservedClashCount)
            {
                clashFlashTimer = Mathf.Max(clashFlashTimer, clashFlashSeconds);
                clashFlashCount += currentClashCount - lastObservedClashCount;
                if (PlayVfxCue(clashCueId, Mathf.Max(lastObservedTier, 1), clashCueIntensity))
                {
                    clashVfxCueRequestCount++;
                }
            }

            lastObservedClashCount = currentClashCount;
        }

        private void ObserveAttackState(bool active)
        {
            bool attacking = active && proxy.CurrentState == SummonFrontlineProxyState.Attacking;
            if (attacking && !wasAttacking)
            {
                attackFlashTimer = Mathf.Max(attackFlashTimer, attackFlashSeconds);
                attackFlashCount++;
                if (PlayVfxCue(attackCueId, Mathf.Max(lastObservedTier, 1), attackCueIntensity))
                {
                    attackVfxCueRequestCount++;
                }

                if (TriggerAnimator(attackTrigger))
                {
                    animatorAttackTriggerCount++;
                }
            }
        }

        private Color ResolveTierColor(int tier)
        {
            return tier switch
            {
                1 => tierOneColor,
                2 => tierTwoColor,
                _ => tierThreeColor
            };
        }

        private float ResolveEntryImpactFlashWeight()
        {
            float entry = entryFlashSeconds > 0f ? Mathf.Clamp01(entryFlashTimer / entryFlashSeconds) : 0f;
            float impact = impactFlashSeconds > 0f ? Mathf.Clamp01(impactFlashTimer / impactFlashSeconds) : 0f;
            return Mathf.Max(entry, impact);
        }

        private float ResolveClashFlashWeight()
        {
            return clashFlashSeconds > 0f ? Mathf.Clamp01(clashFlashTimer / clashFlashSeconds) : 0f;
        }

        private static float ResolveFlashWeight(float timer, float seconds)
        {
            return seconds > 0f ? Mathf.Clamp01(timer / seconds) : 0f;
        }

        private void ApplyColor(Color color)
        {
            propertyBlock ??= new MaterialPropertyBlock();
            if (actorRenderers == null)
            {
                return;
            }

            for (int i = 0; i < actorRenderers.Length; i++)
            {
                Renderer actorRenderer = actorRenderers[i];
                if (actorRenderer == null)
                {
                    continue;
                }

                actorRenderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor(BaseColorId, color);
                propertyBlock.SetColor(ColorId, color);
                propertyBlock.SetColor(EmissionColorId, color * 1.25f);
                actorRenderer.SetPropertyBlock(propertyBlock);
            }
        }

        private void SetPulseVisible(bool value)
        {
            if (pulseRoot != null && pulseRoot.gameObject.activeSelf != value)
            {
                pulseRoot.gameObject.SetActive(value);
            }
        }

        private void RefreshAnimator(bool active)
        {
            float moveSpeed = active
                && proxy != null
                && proxy.CurrentState == SummonFrontlineProxyState.Advancing
                    ? proxy.ActiveMoveSpeed * animatorMoveSpeedScale
                    : 0f;
            SetAnimatorMoveSpeed(moveSpeed);
        }

        private void SetAnimatorMoveSpeed(float value)
        {
            if (!HasAnimatorParameter(moveSpeedParameter, AnimatorControllerParameterType.Float))
            {
                return;
            }

            animator.SetFloat(moveSpeedParameter, Mathf.Max(0f, value));
            animatorMoveSpeedSetCount++;
        }

        private bool TriggerAnimator(string triggerName)
        {
            if (!HasAnimatorParameter(triggerName, AnimatorControllerParameterType.Trigger))
            {
                return false;
            }

            animator.SetTrigger(triggerName);
            return true;
        }

        private bool HasAnimatorParameter(string parameterName, AnimatorControllerParameterType expectedType)
        {
            if (animator == null
                || animator.runtimeAnimatorController == null
                || string.IsNullOrWhiteSpace(parameterName))
            {
                return false;
            }

            AnimatorControllerParameter[] parameters = animator.parameters;
            for (int i = 0; i < parameters.Length; i++)
            {
                AnimatorControllerParameter parameter = parameters[i];
                if (parameter.type == expectedType
                    && string.Equals(parameter.name, parameterName, System.StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private void HandleDamaged(DamageInfo damageInfo)
        {
            if (!DamageResponsePolicyUtility.PlaysDamagePresentation(damageInfo.ResponsePolicy))
            {
                return;
            }

            damageFlashTimer = Mathf.Max(damageFlashTimer, damageFlashSeconds);
            damageFlashCount++;
            float policyScale = ResolveDamageCuePolicyScale(damageInfo);
            bool interruptsAction = DamageResponsePolicyUtility.InterruptsAction(damageInfo.ControlLockPolicy);
            float damageIntensity = ResolveTieredCueIntensity(damageCueIntensity, Mathf.Max(lastObservedTier, 1)) * policyScale;
            lastDamageCueIntensity = damageIntensity;
            lastDamageCuePolicyScale = policyScale;
            lastDamageResponsePolicy = damageInfo.ResponsePolicy;
            lastDamageControlLockPolicy = damageInfo.ControlLockPolicy;
            lastDamageCueInterruptedAction = interruptsAction;
            if (PlayVfxCue(damageCueId, damageIntensity))
            {
                damageVfxCueRequestCount++;
            }

            if (ShouldPlayHitAnimation(damageInfo) && TriggerAnimator(hitTrigger))
            {
                animatorHitTriggerCount++;
            }

            RefreshNow();
        }

        private static bool ShouldPlayHitAnimation(DamageInfo damageInfo)
        {
            return DamageResponsePolicyUtility.PlaysFullBodyHitAnimation(damageInfo.ResponsePolicy);
        }

        private float ResolveDamageCuePolicyScale(DamageInfo damageInfo)
        {
            return DamageResponsePolicyUtility.InterruptsAction(damageInfo.ControlLockPolicy)
                ? 1f
                : Mathf.Clamp(pressureDamageCueScale, 0.1f, 1f);
        }

        private void HandleDied()
        {
            deathFlashTimer = Mathf.Max(deathFlashTimer, deathFlashSeconds);
            deathFlashCount++;
            if (PlayVfxCue(deathCueId, Mathf.Max(lastObservedTier, 1), deathCueIntensity))
            {
                deathVfxCueRequestCount++;
            }

            if (TriggerAnimator(deathTrigger))
            {
                animatorDeathTriggerCount++;
            }

            SetAnimatorMoveSpeed(0f);
            RefreshNow();
        }

        private void ResolveReferences()
        {
            if (proxy == null)
            {
                proxy = GetComponent<SummonFrontlineProxy>();
            }

            if (clash == null)
            {
                clash = GetComponent<SummonFrontlineClash>();
            }

            if (health == null && proxy != null)
            {
                health = proxy.Health;
            }

            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>(includeInactive: true);
            }

            if (vfxAnchor == null)
            {
                vfxAnchor = transform;
            }
        }

        private bool PlayVfxCue(CombatVfxCueId cueId, int tier, float baseIntensity)
        {
            CombatVfxCuePlayer resolvedCuePlayer = ResolveCuePlayer();
            if (resolvedCuePlayer == null)
            {
                return false;
            }

            Transform anchor = vfxAnchor != null ? vfxAnchor : transform;
            return resolvedCuePlayer.PlayCue(cueId, anchor, ResolveVfxDirection(anchor), ResolveTieredCueIntensity(baseIntensity, tier));
        }

        private bool PlayVfxCue(CombatVfxCueId cueId, float intensity)
        {
            CombatVfxCuePlayer resolvedCuePlayer = ResolveCuePlayer();
            if (resolvedCuePlayer == null)
            {
                return false;
            }

            Transform anchor = vfxAnchor != null ? vfxAnchor : transform;
            return resolvedCuePlayer.PlayCue(cueId, anchor, ResolveVfxDirection(anchor), intensity);
        }

        private float ResolveTieredCueIntensity(float baseIntensity, int tier)
        {
            return baseIntensity + Mathf.Max(0, tier - 1) * tierCueIntensityStep;
        }

        private CombatVfxCuePlayer ResolveCuePlayer()
        {
            if (cuePlayer != null)
            {
                return cuePlayer;
            }

            cuePlayer = GetComponent<CombatVfxCuePlayer>();
            return cuePlayer;
        }

        private Vector3 ResolveVfxDirection(Transform anchor)
        {
            if (vfxDirectionTarget != null)
            {
                Vector3 targetDirection = Vector3.ProjectOnPlane(vfxDirectionTarget.position - anchor.position, Vector3.up);
                if (targetDirection.sqrMagnitude > 0.0001f)
                {
                    return targetDirection.normalized;
                }
            }

            Vector3 forward = Vector3.ProjectOnPlane(anchor.forward, Vector3.up);
            return forward.sqrMagnitude > 0.0001f ? forward.normalized : Vector3.forward;
        }

        private void SubscribeHealth()
        {
            ResolveReferences();
            if (health == null || subscribedToHealth)
            {
                return;
            }

            health.Damaged += HandleDamaged;
            health.Died += HandleDied;
            subscribedToHealth = true;
        }

        private void UnsubscribeHealth()
        {
            if (health == null || !subscribedToHealth)
            {
                return;
            }

            health.Damaged -= HandleDamaged;
            health.Died -= HandleDied;
            subscribedToHealth = false;
        }
    }
}
