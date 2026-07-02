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
        private static readonly int EmissionColorLdrId = Shader.PropertyToID("_EmissionColorLDR");
        private static readonly int EmissionColorHdrId = Shader.PropertyToID("_EmissionColorHDR");
        private static readonly int EmissionStrengthId = Shader.PropertyToID("_EmissionStrength");
        private static readonly int UseEmissionId = Shader.PropertyToID("_UseEmission");
        private const float MinimumVisibleDamageFlashSeconds = 0.2f;
        private const float MinimumDamageFlashBlend = 0.96f;
        private const float MinimumDamageEmissionBoost = 3.25f;
        private const float MinimumDamageVfxAnchorLocalY = 0.35f;
        private const string DamageVfxAnchorName = "DamageVfxAnchor";

        [SerializeField] private SummonFrontlineProxy proxy;
        [SerializeField] private SummonFrontlineClash clash;
        [SerializeField] private CombatHealth health;
        [SerializeField] private Animator animator;
        [SerializeField] private string moveSpeedParameter = "MoveSpeed";
        [SerializeField] private string spawnTrigger = "EliteSummonPackage";
        [SerializeField] private string attackTrigger = "Attack";
        [SerializeField] private string hitTrigger = string.Empty;
        [SerializeField] private string deathTrigger = "Death";
        [SerializeField, Min(0f)] private float animatorMoveSpeedScale = 1f;
        [SerializeField] private Transform pulseRoot;
        [SerializeField] private Renderer[] actorRenderers = System.Array.Empty<Renderer>();
        [SerializeField] private Renderer[] damageFlashRenderers = System.Array.Empty<Renderer>();

        [Header("VFX Cues")]
        [SerializeField] private CombatVfxCuePlayer cuePlayer;
        [SerializeField] private Transform vfxAnchor;
        [SerializeField] private Transform damageVfxAnchor;
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
        [SerializeField] private bool playDamageVfx = true;
        [SerializeField] private bool renderDamageFeedback = true;

        [SerializeField] private Color tierOneColor = new Color(0.24f, 1f, 0.78f, 0.78f);
        [SerializeField] private Color tierTwoColor = new Color(0.38f, 0.74f, 1f, 0.9f);
        [SerializeField] private Color tierThreeColor = new Color(1f, 0.76f, 0.24f, 1f);
        [SerializeField] private Color flashColor = new Color(1f, 1f, 1f, 1f);
        [SerializeField] private Color clashFlashColor = new Color(1f, 0.9f, 0.38f, 1f);
        [SerializeField] private Color attackFlashColor = new Color(1f, 0.74f, 0.24f, 1f);
        [SerializeField] private Color damageFlashColor = new Color(1f, 0.24f, 0.18f, 1f);
        [SerializeField] private Color damageFlashEmissionColor = new Color(1f, 0.68f, 0.24f, 1f);
        [SerializeField] private Color deathFlashColor = new Color(0.08f, 0.08f, 0.08f, 1f);
        [SerializeField, Min(0f)] private float entryFlashSeconds = 0.22f;
        [SerializeField, Min(0f)] private float impactFlashSeconds = 0.18f;
        [SerializeField, Min(0f)] private float clashFlashSeconds = 0.14f;
        [SerializeField, Min(0f)] private float attackFlashSeconds = 0.12f;
        [SerializeField, Min(0f)] private float damageFlashSeconds = 0.2f;
        [SerializeField, Min(0f)] private float deathFlashSeconds = 0.22f;
        [SerializeField, Min(0f)] private float fullBodyHitReactionCooldownSeconds = 0.42f;
        [SerializeField] private bool heavyHitReactionBypassesCooldown = true;
        [SerializeField, Range(0.2f, 1f)] private float impactFlashProgress = 0.86f;
        [SerializeField, Min(0.01f)] private float pulseSpeed = 8f;
        [SerializeField, Min(0f)] private float pulseScale = 0.08f;
        [SerializeField] private bool renderPulseVisuals;
        [SerializeField, Min(0f)] private float tierScaleStep = 0.18f;
        [SerializeField, Min(0f)] private float flashScale = 0.22f;
        [SerializeField, Min(0f)] private float clashFlashScale = 0.16f;
        [SerializeField, Min(0f)] private float attackFlashScale = 0.14f;
        [SerializeField, Min(0f)] private float damageFlashScale = 0.18f;
        [SerializeField, Range(0f, 1f)] private float damageFlashColorBlend = 0.98f;
        [SerializeField, Min(0f)] private float damageFlashEmissionBoost = 3.4f;
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
        private int suppressedAnimatorHitTriggerCount;
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
        private bool lastFullBodyHitReactionSuppressed;
        private float nextFullBodyHitReactionTime;

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
        public Transform DamageVfxAnchor => damageVfxAnchor;
        public Transform VfxDirectionTarget => vfxDirectionTarget;
        public CombatVfxCueId EntryCueId => entryCueId;
        public CombatVfxCueId AttackCueId => attackCueId;
        public CombatVfxCueId ClashCueId => clashCueId;
        public CombatVfxCueId DamageCueId => damageCueId;
        public CombatVfxCueId DeathCueId => deathCueId;
        public Transform PulseRoot => pulseRoot;
        public int RendererCount => actorRenderers != null ? actorRenderers.Length : 0;
        public int DamageFlashRendererCount => ResolveDamageFlashRenderers().Length;
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
        public int SuppressedAnimatorHitTriggerCount => suppressedAnimatorHitTriggerCount;
        public int AnimatorDeathTriggerCount => animatorDeathTriggerCount;
        public int AnimatorMoveSpeedSetCount => animatorMoveSpeedSetCount;
        public int EntryVfxCueRequestCount => entryVfxCueRequestCount;
        public int AttackVfxCueRequestCount => attackVfxCueRequestCount;
        public int ClashVfxCueRequestCount => clashVfxCueRequestCount;
        public int DamageVfxCueRequestCount => damageVfxCueRequestCount;
        public int DeathVfxCueRequestCount => deathVfxCueRequestCount;
        public float PressureDamageCueScale => pressureDamageCueScale;
        public bool PlayDamageVfx => playDamageVfx;
        public bool RenderDamageFeedback => renderDamageFeedback;
        public float FullBodyHitReactionCooldownSeconds => fullBodyHitReactionCooldownSeconds;
        public float LastDamageCueIntensity => lastDamageCueIntensity;
        public float LastDamageCuePolicyScale => lastDamageCuePolicyScale;
        public DamageResponsePolicy LastDamageResponsePolicy => lastDamageResponsePolicy;
        public CombatControlLockPolicy LastDamageControlLockPolicy => lastDamageControlLockPolicy;
        public bool LastDamageCueInterruptedAction => lastDamageCueInterruptedAction;
        public bool LastFullBodyHitReactionSuppressed => lastFullBodyHitReactionSuppressed;
        public bool RenderPulseVisuals => renderPulseVisuals;

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
            float damageFlash = ResolveFlashWeight(
                damageFlashTimer,
                Mathf.Max(damageFlashSeconds, MinimumVisibleDamageFlashSeconds));
            float deathFlash = ResolveFlashWeight(deathFlashTimer, deathFlashSeconds);
            Color color = Color.Lerp(tierColor, flashColor, flash);
            color = Color.Lerp(color, clashFlashColor, clashFlash);
            color = Color.Lerp(color, attackFlashColor, attackFlash);
            color = Color.Lerp(color, damageFlashColor, damageFlash);
            color = Color.Lerp(color, deathFlashColor, deathFlash);
            ApplyColor(color);
            ApplyDamageFlash(damageFlash);

            if (pulseRoot == null || !renderPulseVisuals)
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

        private void ApplyDamageFlash(float weight)
        {
            Renderer[] targets = ResolveDamageFlashRenderers();
            if (targets.Length == 0)
            {
                return;
            }

            float clampedWeight = Mathf.Clamp01(weight);
            float blend = Mathf.Clamp01(Mathf.Max(damageFlashColorBlend, MinimumDamageFlashBlend) * clampedWeight);
            float emissionBoost = Mathf.Max(damageFlashEmissionBoost, MinimumDamageEmissionBoost) * clampedWeight;
            Color visibleDamageColor = Color.Lerp(Color.white, damageFlashColor, 0.5f);
            Color boostedEmission = damageFlashEmissionColor * emissionBoost;
            boostedEmission.a = 1f;

            propertyBlock ??= new MaterialPropertyBlock();
            for (int i = 0; i < targets.Length; i++)
            {
                Renderer targetRenderer = targets[i];
                if (targetRenderer == null)
                {
                    continue;
                }

                targetRenderer.GetPropertyBlock(propertyBlock);
                Color baseColor = ResolveRendererBaseColor(targetRenderer);
                Color flashBodyColor = Color.Lerp(baseColor, visibleDamageColor, blend);
                flashBodyColor.a = baseColor.a;
                propertyBlock.SetColor(BaseColorId, flashBodyColor);
                propertyBlock.SetColor(ColorId, flashBodyColor);
                propertyBlock.SetColor(EmissionColorId, boostedEmission);
                propertyBlock.SetColor(EmissionColorLdrId, boostedEmission);
                propertyBlock.SetColor(EmissionColorHdrId, boostedEmission);
                propertyBlock.SetFloat(EmissionStrengthId, emissionBoost);
                propertyBlock.SetFloat(UseEmissionId, clampedWeight > 0f ? 1f : 0f);
                targetRenderer.SetPropertyBlock(propertyBlock);
            }
        }

        private Renderer[] ResolveDamageFlashRenderers()
        {
            if (damageFlashRenderers != null && damageFlashRenderers.Length > 0)
            {
                return damageFlashRenderers;
            }

            Renderer[] foundRenderers = GetComponentsInChildren<Renderer>(includeInactive: true);
            if (foundRenderers == null || foundRenderers.Length == 0)
            {
                damageFlashRenderers = System.Array.Empty<Renderer>();
                return damageFlashRenderers;
            }

            var resolvedRenderers = new System.Collections.Generic.List<Renderer>(foundRenderers.Length);
            for (int i = 0; i < foundRenderers.Length; i++)
            {
                Renderer renderer = foundRenderers[i];
                if (renderer == null || !renderer.enabled || IsActorRenderer(renderer))
                {
                    continue;
                }

                if (pulseRoot != null && renderer.transform.IsChildOf(pulseRoot))
                {
                    continue;
                }

                resolvedRenderers.Add(renderer);
            }

            damageFlashRenderers = resolvedRenderers.ToArray();
            return damageFlashRenderers;
        }

        private bool IsActorRenderer(Renderer candidate)
        {
            if (candidate == null || actorRenderers == null)
            {
                return false;
            }

            for (int i = 0; i < actorRenderers.Length; i++)
            {
                if (actorRenderers[i] == candidate)
                {
                    return true;
                }
            }

            return false;
        }

        private static Color ResolveRendererBaseColor(Renderer targetRenderer)
        {
            Material[] materials = targetRenderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
            {
                Material material = materials[i];
                if (material == null)
                {
                    continue;
                }

                if (material.HasProperty(BaseColorId))
                {
                    return material.GetColor(BaseColorId);
                }

                if (material.HasProperty(ColorId))
                {
                    return material.GetColor(ColorId);
                }
            }

            return Color.white;
        }

        private void SetPulseVisible(bool value)
        {
            bool shouldRender = value && renderPulseVisuals;
            if (pulseRoot != null && pulseRoot.gameObject.activeSelf != shouldRender)
            {
                pulseRoot.gameObject.SetActive(shouldRender);
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
            lastFullBodyHitReactionSuppressed = false;
            if (!DamageResponsePolicyUtility.PlaysDamagePresentation(damageInfo.ResponsePolicy))
            {
                return;
            }

            float policyScale = ResolveDamageCuePolicyScale(damageInfo);
            bool interruptsAction = DamageResponsePolicyUtility.InterruptsAction(damageInfo.ControlLockPolicy);
            float damageIntensity = ResolveTieredCueIntensity(damageCueIntensity, Mathf.Max(lastObservedTier, 1)) * policyScale;
            lastDamageCueIntensity = damageIntensity;
            lastDamageCuePolicyScale = policyScale;
            lastDamageResponsePolicy = damageInfo.ResponsePolicy;
            lastDamageControlLockPolicy = damageInfo.ControlLockPolicy;
            lastDamageCueInterruptedAction = interruptsAction;
            if (playDamageVfx && PlayVfxCue(damageCueId, damageIntensity))
            {
                damageVfxCueRequestCount++;
            }

            if (renderDamageFeedback)
            {
                damageFlashTimer = Mathf.Max(
                    damageFlashTimer,
                    Mathf.Max(damageFlashSeconds, MinimumVisibleDamageFlashSeconds));
                damageFlashCount++;
            }

            if (renderDamageFeedback && TryConsumeFullBodyHitReaction(damageInfo) && TriggerAnimator(hitTrigger))
            {
                animatorHitTriggerCount++;
            }

            RefreshNow();
        }

        private bool TryConsumeFullBodyHitReaction(DamageInfo damageInfo)
        {
            if (!DamageResponsePolicyUtility.PlaysFullBodyHitAnimation(damageInfo))
            {
                return false;
            }

            bool bypassesCooldown = heavyHitReactionBypassesCooldown
                && (damageInfo.ControlLockPolicy == CombatControlLockPolicy.HardLock
                    || IsHeavyFullBodyHitReaction(damageInfo.ResponsePolicy));
            float now = Time.time;
            if (!bypassesCooldown && now < nextFullBodyHitReactionTime)
            {
                suppressedAnimatorHitTriggerCount++;
                lastFullBodyHitReactionSuppressed = true;
                return false;
            }

            nextFullBodyHitReactionTime = now + Mathf.Max(0f, fullBodyHitReactionCooldownSeconds);
            return true;
        }

        private static bool IsHeavyFullBodyHitReaction(DamageResponsePolicy responsePolicy)
        {
            return responsePolicy == DamageResponsePolicy.Break
                || responsePolicy == DamageResponsePolicy.Knockdown;
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

            Transform anchor = ResolveCueAnchor(cueId);
            return resolvedCuePlayer.PlayCue(cueId, anchor, ResolveVfxDirection(anchor), ResolveTieredCueIntensity(baseIntensity, tier));
        }

        private bool PlayVfxCue(CombatVfxCueId cueId, float intensity)
        {
            CombatVfxCuePlayer resolvedCuePlayer = ResolveCuePlayer();
            if (resolvedCuePlayer == null)
            {
                return false;
            }

            Transform anchor = ResolveCueAnchor(cueId);
            return resolvedCuePlayer.PlayCue(cueId, anchor, ResolveVfxDirection(anchor), intensity);
        }

        private Transform ResolveCueAnchor(CombatVfxCueId cueId)
        {
            if (cueId == damageCueId)
            {
                return ResolveDamageVfxAnchor();
            }

            return vfxAnchor != null ? vfxAnchor : transform;
        }

        private Transform ResolveDamageVfxAnchor()
        {
            if (damageVfxAnchor == null)
            {
                Transform existing = transform.Find(DamageVfxAnchorName);
                if (existing != null)
                {
                    damageVfxAnchor = existing;
                }
            }

            if (damageVfxAnchor == null)
            {
                var anchorObject = new GameObject(DamageVfxAnchorName);
                damageVfxAnchor = anchorObject.transform;
                damageVfxAnchor.SetParent(transform, worldPositionStays: false);
            }

            UpdateDamageVfxAnchorFromRenderers();
            return damageVfxAnchor != null ? damageVfxAnchor : (vfxAnchor != null ? vfxAnchor : transform);
        }

        private void UpdateDamageVfxAnchorFromRenderers()
        {
            if (damageVfxAnchor == null || !TryResolveDamageFlashBounds(out Bounds bounds))
            {
                return;
            }

            Vector3 localCenter = transform.InverseTransformPoint(bounds.center);
            if (localCenter.y < MinimumDamageVfxAnchorLocalY
                && damageVfxAnchor.localPosition.y >= MinimumDamageVfxAnchorLocalY)
            {
                return;
            }

            damageVfxAnchor.localPosition = localCenter;
            damageVfxAnchor.rotation = transform.rotation;
            damageVfxAnchor.localScale = Vector3.one;
        }

        private bool TryResolveDamageFlashBounds(out Bounds bounds)
        {
            Renderer[] targets = ResolveDamageFlashRenderers();
            bounds = default;
            bool hasBounds = false;
            for (int i = 0; i < targets.Length; i++)
            {
                Renderer target = targets[i];
                if (target == null || !target.enabled)
                {
                    continue;
                }

                if (!hasBounds)
                {
                    bounds = target.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(target.bounds);
                }
            }

            return hasBounds;
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
