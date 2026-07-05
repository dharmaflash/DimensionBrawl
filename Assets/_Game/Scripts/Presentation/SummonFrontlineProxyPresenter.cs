using System.Collections;
using System.Collections.Generic;
using DimensionBrawl.Combat;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DimensionBrawl.Presentation
{
    [DisallowMultipleComponent]
    public sealed class SummonFrontlineProxyPresenter : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int BaseMapId = Shader.PropertyToID("_BaseMap");
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        private static readonly int MainColorId = Shader.PropertyToID("_MainColor");
        private static readonly int TintColorId = Shader.PropertyToID("_TintColor");
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
        private static readonly int EmissionMapId = Shader.PropertyToID("_EmissionMap");
        private static readonly int EmissionColorLdrId = Shader.PropertyToID("_EmissionColorLDR");
        private static readonly int EmissionColorHdrId = Shader.PropertyToID("_EmissionColorHDR");
        private static readonly int EmissionStrengthId = Shader.PropertyToID("_EmissionStrength");
        private static readonly int UseEmissionId = Shader.PropertyToID("_UseEmission");
        private static readonly Color ForcedAllySummonTint = new Color(0.96f, 0.99f, 1f, 1f);
        private static readonly Color ForcedAllySummonEmission = new Color(0.72f, 0.92f, 1f, 1f);
        private const float ForcedAllySummonTintBlend = 0.94f;
        private const float ForcedAllySummonEmissionBoost = 2.85f;
        private const float MinimumVisibleDamageFlashSeconds = 0.28f;
        private const float MinimumDamageFlashBlend = 0.96f;
        private const float MinimumDamageEmissionBoost = 4.5f;
        private const float MinimumDamageVfxAnchorLocalY = 0.35f;
        private const string DamageVfxAnchorName = "DamageVfxAnchor";

        [SerializeField] private SummonFrontlineProxy proxy;
        [SerializeField] private SummonFrontlineClash clash;
        [SerializeField] private CombatHealth health;
        [SerializeField] private Animator animator;
        [SerializeField] private string moveSpeedParameter = "MoveSpeed";
        [SerializeField] private string spawnTrigger = "EliteSummonPackage";
        [SerializeField] private bool lockAdvanceDuringSpawnState;
        [SerializeField] private string spawnStateName = "EliteSummonPackage";
        [SerializeField, Min(0f)] private float spawnMovementLockSeconds;
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
        [SerializeField] private bool tintByOwnerTeam = true;
        [SerializeField] private Color allyTeamTint = new Color(0.92f, 0.98f, 1f, 1f);
        [SerializeField] private Color enemyTeamTint = new Color(1f, 0.16f, 0.08f, 1f);
        [SerializeField, Range(0f, 1f)] private float allyTeamTintBlend = 0.78f;
        [SerializeField, Range(0f, 1f)] private float enemyTeamTintBlend = 0.62f;
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
        private readonly List<AllyRendererMaterialState> allyMaterialStates = new List<AllyRendererMaterialState>(4);
        private Vector3 pulseBaseScale = Vector3.one;
        private float entryFlashTimer;
        private float impactFlashTimer;
        private float clashFlashTimer;
        private float attackFlashTimer;
        private float damageFlashTimer;
        private float deathFlashTimer;
        private float spawnMovementLockTimer;
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
        public bool LockAdvanceDuringSpawnState => lockAdvanceDuringSpawnState;
        public string SpawnStateName => spawnStateName;
        public float SpawnMovementLockSeconds => spawnMovementLockSeconds;
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
            RestoreAllyMaterialOverrides();
            wasActive = false;
            wasAttacking = false;
            spawnMovementLockTimer = 0f;
            proxy?.SetAdvancePresentationLocked(false);
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

            if (spawnMovementLockTimer > 0f)
            {
                spawnMovementLockTimer = Mathf.Max(0f, spawnMovementLockTimer - deltaTime);
            }

            RefreshNow();
        }

        public void ConfigurePresentation(
            SummonFrontlineProxy newProxy,
            Transform newPulseRoot,
            Renderer[] newActorRenderers)
        {
            UnsubscribeHealth();
            RestoreAllyMaterialOverrides();
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
                if (IsPlayerSideOwner())
                {
                    EnsureAllyMaterialOverrides();
                }
                else
                {
                    RestoreAllyMaterialOverrides();
                }

                int tier = active
                    ? Mathf.Clamp(proxy.ActiveTier, 1, 3)
                    : Mathf.Clamp(lastObservedTier > 0 ? lastObservedTier : proxy.ActiveTier, 1, 3);
                if (!wasActive)
                {
                    entryFlashTimer = Mathf.Max(entryFlashTimer, entryFlashSeconds);
                    impactFlashedThisActivation = false;
                    entryFlashCount++;
                    BeginSpawnMovementLock();
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
                RefreshAdvancePresentationLock(active);
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
                RestoreAllyMaterialOverrides();
                proxy?.SetAdvancePresentationLocked(false);
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
            Color color = tier switch
            {
                1 => tierOneColor,
                2 => tierTwoColor,
                _ => tierThreeColor
            };

            return ApplyTeamTint(color);
        }

        private Color ApplyTeamTint(Color color)
        {
            DamageTeam team = health != null
                ? health.Team
                : proxy != null
                    ? proxy.OwnerTeam
                    : DamageTeam.Neutral;
            if (team == DamageTeam.Neutral)
            {
                return color;
            }

            bool isPlayerSide = CombatTeamUtility.IsPlayerSide(team);
            if (!tintByOwnerTeam && !isPlayerSide)
            {
                return color;
            }

            Color target = isPlayerSide ? ForcedAllySummonTint : enemyTeamTint;
            float blend = isPlayerSide
                ? Mathf.Max(allyTeamTintBlend, ForcedAllySummonTintBlend)
                : enemyTeamTintBlend;
            Color tinted = Color.Lerp(color, target, Mathf.Clamp01(blend));
            tinted.a = color.a;
            return tinted;
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
                propertyBlock.SetColor(MainColorId, color);
                propertyBlock.SetColor(TintColorId, color);
                Color emissionColor = ResolvePresentationEmissionColor(color);
                propertyBlock.SetColor(EmissionColorId, emissionColor);
                propertyBlock.SetColor(EmissionColorLdrId, emissionColor);
                propertyBlock.SetColor(EmissionColorHdrId, emissionColor);
                propertyBlock.SetFloat(UseEmissionId, 1f);
                propertyBlock.SetFloat(EmissionStrengthId, ResolvePresentationEmissionStrength());
                actorRenderer.SetPropertyBlock(propertyBlock);
            }
        }

        private void EnsureAllyMaterialOverrides()
        {
            if (allyMaterialStates.Count > 0 || actorRenderers == null)
            {
                return;
            }

            for (int i = 0; i < actorRenderers.Length; i++)
            {
                Renderer actorRenderer = actorRenderers[i];
                if (!CanOverrideAllyRenderer(actorRenderer))
                {
                    continue;
                }

                Material[] originalMaterials = actorRenderer.sharedMaterials;
                Material[] runtimeMaterials = new Material[originalMaterials.Length];
                for (int materialIndex = 0; materialIndex < originalMaterials.Length; materialIndex++)
                {
                    runtimeMaterials[materialIndex] = CreateAllyTintMaterial(originalMaterials[materialIndex]);
                }

                allyMaterialStates.Add(new AllyRendererMaterialState(
                    actorRenderer,
                    originalMaterials,
                    runtimeMaterials));
                actorRenderer.sharedMaterials = runtimeMaterials;
            }
        }

        private static bool CanOverrideAllyRenderer(Renderer actorRenderer)
        {
            if (actorRenderer == null
                || actorRenderer is ParticleSystemRenderer
                || actorRenderer is TrailRenderer
                || actorRenderer is LineRenderer)
            {
                return false;
            }

            Material[] materials = actorRenderer.sharedMaterials;
            return materials != null && materials.Length > 0;
        }

        private Material CreateAllyTintMaterial(Material source)
        {
            Material material = source != null
                ? new Material(source)
                : new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            material.name = source != null
                ? source.name + " (AllyWhiteRuntime)"
                : "AllyWhiteRuntime";
            ConfigureAllyTintMaterial(material);
            return material;
        }

        private static void ConfigureAllyTintMaterial(Material material)
        {
            if (material == null)
            {
                return;
            }

            Color tint = ForcedAllySummonTint;
            Color emission = ForcedAllySummonEmission * ForcedAllySummonEmissionBoost;
            emission.a = 1f;
            SetTextureIfPresent(material, BaseMapId, Texture2D.whiteTexture);
            SetTextureIfPresent(material, MainTexId, Texture2D.whiteTexture);
            SetTextureIfPresent(material, EmissionMapId, Texture2D.whiteTexture);
            SetColorIfPresent(material, BaseColorId, tint);
            SetColorIfPresent(material, ColorId, tint);
            SetColorIfPresent(material, MainColorId, tint);
            SetColorIfPresent(material, TintColorId, tint);
            SetColorIfPresent(material, EmissionColorId, emission);
            SetColorIfPresent(material, EmissionColorLdrId, emission);
            SetColorIfPresent(material, EmissionColorHdrId, emission);
            SetFloatIfPresent(material, UseEmissionId, 1f);
            SetFloatIfPresent(material, EmissionStrengthId, ForcedAllySummonEmissionBoost);
            material.EnableKeyword("_EMISSION");
        }

        private void RestoreAllyMaterialOverrides()
        {
            for (int i = 0; i < allyMaterialStates.Count; i++)
            {
                AllyRendererMaterialState state = allyMaterialStates[i];
                if (state.Renderer != null)
                {
                    state.Renderer.sharedMaterials = state.OriginalMaterials;
                }

                DestroyRuntimeMaterials(state.RuntimeMaterials);
            }

            allyMaterialStates.Clear();
        }

        private static void DestroyRuntimeMaterials(Material[] materials)
        {
            if (materials == null)
            {
                return;
            }

            for (int i = 0; i < materials.Length; i++)
            {
                Material material = materials[i];
                if (material == null)
                {
                    continue;
                }

                if (Application.isPlaying)
                {
                    Destroy(material);
                }
                else
                {
                    DestroyImmediate(material);
                }
            }
        }

        private static void SetTextureIfPresent(Material material, int propertyId, Texture texture)
        {
            if (material != null && material.HasProperty(propertyId))
            {
                material.SetTexture(propertyId, texture);
            }
        }

        private static void SetColorIfPresent(Material material, int propertyId, Color color)
        {
            if (material != null && material.HasProperty(propertyId))
            {
                material.SetColor(propertyId, color);
            }
        }

        private static void SetFloatIfPresent(Material material, int propertyId, float value)
        {
            if (material != null && material.HasProperty(propertyId))
            {
                material.SetFloat(propertyId, value);
            }
        }

        private Color ResolvePresentationEmissionColor(Color color)
        {
            if (!IsPlayerSideOwner())
            {
                return color * 1.25f;
            }

            Color emission = ForcedAllySummonEmission * ForcedAllySummonEmissionBoost;
            emission.a = 1f;
            return emission;
        }

        private float ResolvePresentationEmissionStrength()
        {
            return IsPlayerSideOwner() ? ForcedAllySummonEmissionBoost : 1.25f;
        }

        private bool IsPlayerSideOwner()
        {
            DamageTeam team = health != null
                ? health.Team
                : proxy != null
                    ? proxy.OwnerTeam
                    : DamageTeam.Neutral;
            return CombatTeamUtility.IsPlayerSide(team);
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
                    ? Mathf.Max(proxy.CurrentMoveSpeed, proxy.ActiveMoveSpeed) * animatorMoveSpeedScale
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

        private void RefreshAdvancePresentationLock(bool active)
        {
            if (proxy == null)
            {
                return;
            }

            if (active && spawnMovementLockTimer > 0f)
            {
                proxy.SetAdvancePresentationLocked(true);
                return;
            }

            if (!lockAdvanceDuringSpawnState)
            {
                proxy.SetAdvancePresentationLocked(false);
                return;
            }

            bool shouldLock = active
                && IsAnimatorInState(spawnStateName);
            proxy.SetAdvancePresentationLocked(shouldLock);
        }

        private void BeginSpawnMovementLock()
        {
            if (spawnMovementLockSeconds <= 0f)
            {
                return;
            }

            spawnMovementLockTimer = Mathf.Max(
                spawnMovementLockTimer,
                spawnMovementLockSeconds);
        }

        private bool IsAnimatorInState(string stateName)
        {
            if (animator == null
                || animator.runtimeAnimatorController == null
                || animator.layerCount <= 0
                || string.IsNullOrWhiteSpace(stateName))
            {
                return false;
            }

            const int Layer = 0;
            AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(Layer);
            if (IsAnimatorStateName(currentState, stateName))
            {
                return true;
            }

            return animator.IsInTransition(Layer)
                && IsAnimatorStateName(animator.GetNextAnimatorStateInfo(Layer), stateName);
        }

        private static bool IsAnimatorStateName(AnimatorStateInfo stateInfo, string stateName)
        {
            return stateInfo.IsName(stateName)
                || stateInfo.IsName("Base Layer." + stateName);
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

            if (renderDamageFeedback
                && HasAnimatorParameter(hitTrigger, AnimatorControllerParameterType.Trigger)
                && TryConsumeFullBodyHitReaction(damageInfo)
                && TriggerAnimator(hitTrigger))
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
            DeathDissolvePresenter.EnsureAndPlay(gameObject);
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

        private sealed class AllyRendererMaterialState
        {
            public AllyRendererMaterialState(Renderer renderer, Material[] originalMaterials, Material[] runtimeMaterials)
            {
                Renderer = renderer;
                OriginalMaterials = originalMaterials;
                RuntimeMaterials = runtimeMaterials;
            }

            public Renderer Renderer { get; }
            public Material[] OriginalMaterials { get; }
            public Material[] RuntimeMaterials { get; }
        }
    }

    [DisallowMultipleComponent]
    public sealed class DeathDissolvePresenter : MonoBehaviour
    {
        private const string InabBurnShaderName = "INab Studio/Interactive Effect Burn";
        private const string InabSmoothShaderName = "INab Studio/Interactive Effect Smooth";
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
        private static readonly int CutoffId = Shader.PropertyToID("_Cutoff");
        private static readonly int MaskValueId = Shader.PropertyToID("_Masks_Value");
        private static readonly int PositionVectorId = Shader.PropertyToID("_PositionVector");
        private static readonly int ScaleVectorId = Shader.PropertyToID("_ScaleVector");
        private static readonly int UpVectorId = Shader.PropertyToID("_UpVector");
        private static readonly int RightVectorId = Shader.PropertyToID("_RightVector");
        private static readonly int ForwardVectorId = Shader.PropertyToID("_ForwardVector");
        private static readonly int EdgeWidthId = Shader.PropertyToID("_EdgeWidth");
        private static readonly int EdgeSmoothnessId = Shader.PropertyToID("_EdgeSmoothness");
        private static readonly int EdgeColorId = Shader.PropertyToID("_EdgeColor");
        private static readonly int BurnColorId = Shader.PropertyToID("_BurnColor");
        private static readonly int BurnOffsetId = Shader.PropertyToID("_BurnOffset");
        private static readonly int BurnHardnessId = Shader.PropertyToID("_BurnHardness");
        private static readonly int InvertId = Shader.PropertyToID("_Invert");
        private static bool sceneHooked;
        private static Shader cachedDissolveShader;

        [SerializeField] private CombatHealth health;
        [SerializeField] private Renderer[] targetRenderers = new Renderer[0];
        [SerializeField] private bool playOnDeath = true;
        [SerializeField] private bool includePlayerTeam;
        [SerializeField, Min(0.05f)] private float durationSeconds = 0.9f;
        [SerializeField, Min(0f)] private float lingerSeconds = 0.12f;
        [SerializeField, Range(0f, 1f)] private float initialCutoff = 0.001f;
        [SerializeField, Range(0f, 1f)] private float finalCutoff = 0.96f;
        [SerializeField, Min(0f)] private float edgeWidth = 0.075f;
        [SerializeField, Min(0f)] private float edgeSmoothness = 0.08f;
        [SerializeField, Min(0f)] private float emissionBoost = 2.8f;
        [SerializeField] private Color edgeColor = new Color(0.82f, 0.96f, 1f, 1f);
        [SerializeField] private Color burnColor = new Color(0.58f, 0.88f, 1f, 1f);

        private readonly List<RendererState> rendererStates = new List<RendererState>(8);
        private Coroutine dissolveRoutine;
        private bool subscribed;
        private bool playing;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InstallOnLoadedScene()
        {
            if (!sceneHooked)
            {
                SceneManager.sceneLoaded += OnSceneLoaded;
                sceneHooked = true;
            }

            AttachToSceneCombatants();
        }

        public static DeathDissolvePresenter EnsureAndPlay(GameObject host)
        {
            if (host == null)
            {
                return null;
            }

            DeathDissolvePresenter presenter = host.GetComponent<DeathDissolvePresenter>();
            if (presenter == null)
            {
                presenter = host.AddComponent<DeathDissolvePresenter>();
            }

            presenter.PlayNow();
            return presenter;
        }

        public void PlayNow()
        {
            ResolveReferences();
            if (playing || !CanPlayForHealth())
            {
                return;
            }

            if (dissolveRoutine != null)
            {
                StopCoroutine(dissolveRoutine);
            }

            dissolveRoutine = StartCoroutine(PlayDissolveRoutine());
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            AttachToSceneCombatants();
        }

        private static void AttachToSceneCombatants()
        {
            CombatHealth[] healths = FindObjectsByType<CombatHealth>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int i = 0; i < healths.Length; i++)
            {
                CombatHealth candidate = healths[i];
                if (candidate == null || !ShouldAutoInstall(candidate))
                {
                    continue;
                }

                if (candidate.GetComponent<DeathDissolvePresenter>() == null)
                {
                    candidate.gameObject.AddComponent<DeathDissolvePresenter>();
                }
            }
        }

        private static bool ShouldAutoInstall(CombatHealth candidate)
        {
            return candidate.Team == DamageTeam.Enemy
                || candidate.Team == DamageTeam.AllySummon;
        }

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();
            RestoreRendererStates();
            SubscribeHealth();
        }

        private void OnDisable()
        {
            UnsubscribeHealth();
            if (dissolveRoutine != null)
            {
                StopCoroutine(dissolveRoutine);
                dissolveRoutine = null;
            }

            playing = false;
            RestoreRendererStates();
        }

        private void OnDestroy()
        {
            UnsubscribeHealth();
            RestoreRendererStates();
        }

        private void ResolveReferences()
        {
            if (health == null)
            {
                health = GetComponent<CombatHealth>();
            }

            if (targetRenderers == null || targetRenderers.Length == 0)
            {
                targetRenderers = GetComponentsInChildren<Renderer>(includeInactive: true);
            }
        }

        private void SubscribeHealth()
        {
            if (subscribed || health == null)
            {
                return;
            }

            health.Died += HandleDied;
            subscribed = true;
        }

        private void UnsubscribeHealth()
        {
            if (!subscribed || health == null)
            {
                return;
            }

            health.Died -= HandleDied;
            subscribed = false;
        }

        private void HandleDied()
        {
            if (playOnDeath)
            {
                PlayNow();
            }
        }

        private bool CanPlayForHealth()
        {
            if (health == null)
            {
                return true;
            }

            return includePlayerTeam || health.Team != DamageTeam.Player;
        }

        private IEnumerator PlayDissolveRoutine()
        {
            playing = true;
            PrepareRendererStates();
            float elapsed = 0f;
            float duration = Mathf.Max(0.05f, durationSeconds);
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
                ApplyDissolve(t);
                yield return null;
            }

            ApplyDissolve(1f);
            SetRenderersEnabled(false);
            float linger = Mathf.Max(0f, lingerSeconds);
            if (linger > 0f)
            {
                yield return new WaitForSecondsRealtime(linger);
            }

            dissolveRoutine = null;
            playing = false;
        }

        private void PrepareRendererStates()
        {
            RestoreRendererStates();
            ResolveReferences();
            rendererStates.Clear();
            Shader dissolveShader = ResolveDissolveShader();
            if (targetRenderers == null)
            {
                return;
            }

            for (int i = 0; i < targetRenderers.Length; i++)
            {
                Renderer targetRenderer = targetRenderers[i];
                if (!CanDissolveRenderer(targetRenderer))
                {
                    continue;
                }

                Material[] originalSharedMaterials = targetRenderer.sharedMaterials;
                if (originalSharedMaterials == null || originalSharedMaterials.Length == 0)
                {
                    continue;
                }

                Material[] runtimeMaterials = new Material[originalSharedMaterials.Length];
                for (int materialIndex = 0; materialIndex < originalSharedMaterials.Length; materialIndex++)
                {
                    runtimeMaterials[materialIndex] = CreateRuntimeMaterial(
                        originalSharedMaterials[materialIndex],
                        dissolveShader);
                }

                rendererStates.Add(new RendererState(
                    targetRenderer,
                    originalSharedMaterials,
                    runtimeMaterials,
                    targetRenderer.enabled));
                targetRenderer.sharedMaterials = runtimeMaterials;
                targetRenderer.enabled = true;
            }
        }

        private static bool CanDissolveRenderer(Renderer targetRenderer)
        {
            return targetRenderer != null
                && !(targetRenderer is ParticleSystemRenderer)
                && !(targetRenderer is TrailRenderer)
                && !(targetRenderer is LineRenderer);
        }

        private Material CreateRuntimeMaterial(Material source, Shader dissolveShader)
        {
            Material material = source != null
                ? new Material(source)
                : new Material(dissolveShader != null ? dissolveShader : Shader.Find("Universal Render Pipeline/Lit"));
            material.name = source != null
                ? source.name + " (DeathDissolve)"
                : "DeathDissolve Runtime";

            if (dissolveShader != null)
            {
                material.shader = dissolveShader;
            }

            CopyCommonMaterialProperties(source, material);
            ConfigureDissolveMaterial(material);
            return material;
        }

        private void CopyCommonMaterialProperties(Material source, Material target)
        {
            if (source == null || target == null)
            {
                return;
            }

            CopyTexture(source, target, "_BaseMap");
            CopyTexture(source, target, "_MainTex");
            CopyTexture(source, target, "_BumpMap");
            CopyTexture(source, target, "_MetallicGlossMap");
            CopyColor(source, target, BaseColorId);
            CopyColor(source, target, ColorId);
            CopyColor(source, target, EmissionColorId);
        }

        private static void CopyTexture(Material source, Material target, string propertyName)
        {
            if (!source.HasProperty(propertyName) || !target.HasProperty(propertyName))
            {
                return;
            }

            target.SetTexture(propertyName, source.GetTexture(propertyName));
            target.SetTextureScale(propertyName, source.GetTextureScale(propertyName));
            target.SetTextureOffset(propertyName, source.GetTextureOffset(propertyName));
        }

        private static void CopyColor(Material source, Material target, int propertyId)
        {
            if (source.HasProperty(propertyId) && target.HasProperty(propertyId))
            {
                target.SetColor(propertyId, source.GetColor(propertyId));
            }
        }

        private void ConfigureDissolveMaterial(Material material)
        {
            if (material == null)
            {
                return;
            }

            material.EnableKeyword("_TYPE_PLANE");
            SetFloatIfPresent(material, CutoffId, initialCutoff);
            SetFloatIfPresent(material, MaskValueId, 0f);
            SetFloatIfPresent(material, EdgeWidthId, edgeWidth);
            SetFloatIfPresent(material, EdgeSmoothnessId, edgeSmoothness);
            SetFloatIfPresent(material, BurnOffsetId, 0.08f);
            SetFloatIfPresent(material, BurnHardnessId, 0.18f);
            SetFloatIfPresent(material, InvertId, 0f);
            SetColorIfPresent(material, EdgeColorId, edgeColor * emissionBoost);
            SetColorIfPresent(material, BurnColorId, burnColor * emissionBoost);
            SetColorIfPresent(material, EmissionColorId, edgeColor * emissionBoost);
        }

        private void ApplyDissolve(float t)
        {
            for (int i = 0; i < rendererStates.Count; i++)
            {
                RendererState state = rendererStates[i];
                if (state.Renderer == null || state.RuntimeMaterials == null)
                {
                    continue;
                }

                Bounds bounds = state.Renderer.bounds;
                Vector3 bottom = new Vector3(bounds.center.x, bounds.min.y - 0.15f, bounds.center.z);
                Vector3 top = new Vector3(bounds.center.x, bounds.max.y + 0.3f, bounds.center.z);
                Vector3 maskPosition = Vector3.Lerp(bottom, top, t);
                Vector4 scale = new Vector4(
                    Mathf.Max(0.1f, bounds.size.x + 0.75f),
                    Mathf.Max(0.1f, bounds.size.y + 0.75f),
                    Mathf.Max(0.1f, bounds.size.z + 0.75f),
                    0f);
                Vector4 up = Vector3.up;
                Vector4 right = state.Renderer.transform.right;
                Vector4 forward = state.Renderer.transform.forward;
                float cutoff = Mathf.Lerp(initialCutoff, finalCutoff, t);
                float edge = Mathf.Lerp(edgeWidth, 0f, t);
                float remaining = 1f - t;

                for (int materialIndex = 0; materialIndex < state.RuntimeMaterials.Length; materialIndex++)
                {
                    Material material = state.RuntimeMaterials[materialIndex];
                    if (material == null)
                    {
                        continue;
                    }

                    SetFloatIfPresent(material, CutoffId, cutoff);
                    SetFloatIfPresent(material, MaskValueId, t);
                    SetFloatIfPresent(material, EdgeWidthId, edge);
                    SetVectorIfPresent(material, PositionVectorId, maskPosition);
                    SetVectorIfPresent(material, ScaleVectorId, scale);
                    SetVectorIfPresent(material, UpVectorId, up);
                    SetVectorIfPresent(material, RightVectorId, right);
                    SetVectorIfPresent(material, ForwardVectorId, forward);
                    ApplyFallbackFade(material, remaining);
                }
            }
        }

        private void ApplyFallbackFade(Material material, float remaining)
        {
            Color color = Color.white;
            if (material.HasProperty(BaseColorId))
            {
                color = material.GetColor(BaseColorId);
            }
            else if (material.HasProperty(ColorId))
            {
                color = material.GetColor(ColorId);
            }

            color.a = Mathf.Clamp01(color.a * remaining);
            SetColorIfPresent(material, BaseColorId, color);
            SetColorIfPresent(material, ColorId, color);
            SetColorIfPresent(material, EmissionColorId, edgeColor * (emissionBoost * remaining));
        }

        private static Shader ResolveDissolveShader()
        {
            if (cachedDissolveShader != null)
            {
                return cachedDissolveShader;
            }

            cachedDissolveShader = Shader.Find(InabBurnShaderName);
            if (cachedDissolveShader == null)
            {
                cachedDissolveShader = Shader.Find(InabSmoothShaderName);
            }

            return cachedDissolveShader;
        }

        private static void SetFloatIfPresent(Material material, int propertyId, float value)
        {
            if (material != null && material.HasProperty(propertyId))
            {
                material.SetFloat(propertyId, value);
            }
        }

        private static void SetColorIfPresent(Material material, int propertyId, Color value)
        {
            if (material != null && material.HasProperty(propertyId))
            {
                material.SetColor(propertyId, value);
            }
        }

        private static void SetVectorIfPresent(Material material, int propertyId, Vector4 value)
        {
            if (material != null && material.HasProperty(propertyId))
            {
                material.SetVector(propertyId, value);
            }
        }

        private void SetRenderersEnabled(bool enabled)
        {
            for (int i = 0; i < rendererStates.Count; i++)
            {
                Renderer targetRenderer = rendererStates[i].Renderer;
                if (targetRenderer != null)
                {
                    targetRenderer.enabled = enabled;
                }
            }
        }

        private void RestoreRendererStates()
        {
            for (int i = 0; i < rendererStates.Count; i++)
            {
                RendererState state = rendererStates[i];
                if (state.Renderer == null)
                {
                    continue;
                }

                state.Renderer.sharedMaterials = state.OriginalSharedMaterials;
                state.Renderer.enabled = state.OriginalEnabled;
                DestroyRuntimeMaterials(state.RuntimeMaterials);
            }

            rendererStates.Clear();
        }

        private static void DestroyRuntimeMaterials(Material[] runtimeMaterials)
        {
            if (runtimeMaterials == null)
            {
                return;
            }

            for (int i = 0; i < runtimeMaterials.Length; i++)
            {
                Material material = runtimeMaterials[i];
                if (material == null)
                {
                    continue;
                }

                if (Application.isPlaying)
                {
                    Destroy(material);
                }
                else
                {
                    DestroyImmediate(material);
                }
            }
        }

        private sealed class RendererState
        {
            public RendererState(
                Renderer renderer,
                Material[] originalSharedMaterials,
                Material[] runtimeMaterials,
                bool originalEnabled)
            {
                Renderer = renderer;
                OriginalSharedMaterials = originalSharedMaterials;
                RuntimeMaterials = runtimeMaterials;
                OriginalEnabled = originalEnabled;
            }

            public Renderer Renderer { get; }
            public Material[] OriginalSharedMaterials { get; }
            public Material[] RuntimeMaterials { get; }
            public bool OriginalEnabled { get; }
        }
    }
}
