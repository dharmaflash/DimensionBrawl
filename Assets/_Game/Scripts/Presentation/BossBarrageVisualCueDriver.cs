using System;
using DimensionBrawl.Combat;
using UnityEngine;

namespace DimensionBrawl.Presentation
{
    [DisallowMultipleComponent]
    public sealed class BossBarrageVisualCueDriver : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        [Serializable]
        public struct PatternAnimationCue
        {
            [SerializeField] private string patternId;
            [SerializeField] private string windupTrigger;
            [SerializeField] private string releaseTrigger;
            [SerializeField] private Color windupColor;
            [SerializeField] private Color releaseColor;
            [SerializeField, Min(0f)] private float windupPulseScale;
            [SerializeField, Min(0f)] private float releasePulseScale;
            [SerializeField] private bool useWorldVfxCueOverride;
            [SerializeField] private CombatVfxCueId windupWorldCueId;
            [SerializeField] private CombatVfxCueId releaseWorldCueId;
            [SerializeField, Min(0f)] private float windupWorldCueIntensity;
            [SerializeField, Min(0f)] private float releaseWorldCueIntensity;

            public PatternAnimationCue(
                string patternId,
                string windupTrigger,
                string releaseTrigger,
                Color windupColor,
                Color releaseColor,
                float windupPulseScale,
                float releasePulseScale,
                bool useWorldVfxCueOverride = false,
                CombatVfxCueId windupWorldCueId = CombatVfxCueId.EliteAuraSignal,
                CombatVfxCueId releaseWorldCueId = CombatVfxCueId.EnemyRetreatShotActive,
                float windupWorldCueIntensity = 1f,
                float releaseWorldCueIntensity = 1f)
            {
                this.patternId = patternId;
                this.windupTrigger = windupTrigger;
                this.releaseTrigger = releaseTrigger;
                this.windupColor = windupColor;
                this.releaseColor = releaseColor;
                this.windupPulseScale = windupPulseScale;
                this.releasePulseScale = releasePulseScale;
                this.useWorldVfxCueOverride = useWorldVfxCueOverride;
                this.windupWorldCueId = windupWorldCueId;
                this.releaseWorldCueId = releaseWorldCueId;
                this.windupWorldCueIntensity = windupWorldCueIntensity;
                this.releaseWorldCueIntensity = releaseWorldCueIntensity;
            }

            public string PatternId => patternId;
            public string WindupTrigger => windupTrigger;
            public string ReleaseTrigger => releaseTrigger;
            public Color WindupColor => windupColor;
            public Color ReleaseColor => releaseColor;
            public float WindupPulseScale => windupPulseScale;
            public float ReleasePulseScale => releasePulseScale;
            public bool UseWorldVfxCueOverride => useWorldVfxCueOverride;
            public CombatVfxCueId WindupWorldCueId => windupWorldCueId;
            public CombatVfxCueId ReleaseWorldCueId => releaseWorldCueId;
            public float WindupWorldCueIntensity => windupWorldCueIntensity > 0f ? windupWorldCueIntensity : 1f;
            public float ReleaseWorldCueIntensity => releaseWorldCueIntensity > 0f ? releaseWorldCueIntensity : 1f;

            public bool Matches(string candidatePatternId)
            {
                return string.Equals(patternId, candidatePatternId, StringComparison.OrdinalIgnoreCase);
            }
        }

        [Serializable]
        public struct PressureActionCue
        {
            [SerializeField] private BossPressureActionKind actionKind;
            [SerializeField] private string trigger;
            [SerializeField] private Color color;
            [SerializeField, Min(0.01f)] private float durationSeconds;
            [SerializeField, Min(0f)] private float pulseScale;
            [SerializeField, Min(0f)] private float tierPulseBonus;

            public PressureActionCue(
                BossPressureActionKind actionKind,
                string trigger,
                Color color,
                float durationSeconds,
                float pulseScale,
                float tierPulseBonus)
            {
                this.actionKind = actionKind;
                this.trigger = trigger;
                this.color = color;
                this.durationSeconds = Mathf.Max(0.01f, durationSeconds);
                this.pulseScale = Mathf.Max(0f, pulseScale);
                this.tierPulseBonus = Mathf.Max(0f, tierPulseBonus);
            }

            public BossPressureActionKind ActionKind => actionKind;
            public string Trigger => trigger;
            public Color Color => color;
            public float DurationSeconds => durationSeconds;
            public float PulseScale => pulseScale;
            public float TierPulseBonus => tierPulseBonus;

            public bool Matches(BossPressureActionKind candidateKind)
            {
                return actionKind == candidateKind;
            }

            public float ResolvePulseScale(int tier)
            {
                return pulseScale + tierPulseBonus * Mathf.Max(0, tier - 1);
            }
        }

        [Header("References")]
        [SerializeField] private BossBarrageEmitter bossBarrageEmitter;
        [SerializeField] private BossPressureActionDirector bossPressureActionDirector;
        [SerializeField] private Animator animator;
        [SerializeField] private Transform pulseRoot;
        [SerializeField] private Renderer[] pulseRenderers = Array.Empty<Renderer>();

        [Header("Default Cue")]
        [SerializeField] private string defaultWindupTrigger = "EliteAuraBuffer";
        [SerializeField] private string defaultReleaseTrigger = "AttackLinePressure";
        [SerializeField] private Color baseColor = new Color(1f, 0.55f, 0.05f, 1f);
        [SerializeField] private Color defaultWindupColor = new Color(1f, 0.78f, 0.24f, 1f);
        [SerializeField] private Color defaultReleaseColor = new Color(1f, 0.92f, 0.56f, 1f);
        [SerializeField, Min(0f)] private float defaultWindupPulseScale = 0.18f;
        [SerializeField, Min(0f)] private float defaultReleasePulseScale = 0.36f;
        [SerializeField, Min(0.01f)] private float releaseFlashSeconds = 0.22f;
        [SerializeField, Min(0f)] private float pulseSpeed = 14f;

        [Header("World VFX Cues")]
        [SerializeField] private CombatVfxCuePlayer cuePlayer;
        [SerializeField] private Transform vfxAnchor;
        [SerializeField] private Transform vfxDirectionTarget;
        [SerializeField] private CombatVfxCueId windupCueId = CombatVfxCueId.EliteAuraSignal;
        [SerializeField] private CombatVfxCueId releaseCueId = CombatVfxCueId.EnemyRetreatShotActive;
        [SerializeField] private CombatVfxCueId skillPressureCueId = CombatVfxCueId.EnemyLinePressureWindup;
        [SerializeField] private CombatVfxCueId summonPressureCueId = CombatVfxCueId.EliteSummonSignal;
        [SerializeField] private CombatVfxCueId punishPressureCueId = CombatVfxCueId.EliteArmorBreakSignal;
        [SerializeField, Min(0f)] private float windupCueIntensity = 0.86f;
        [SerializeField, Min(0f)] private float releaseCueIntensity = 1.05f;
        [SerializeField, Min(0f)] private float pressureActionCueIntensity = 0.95f;
        [SerializeField, Min(0f)] private float tierCueIntensityStep = 0.08f;

        [Header("Damage Feedback")]
        [SerializeField] private CombatHealth damageFeedbackHealth;
        [SerializeField] private CombatVfxCueId damageCueId = CombatVfxCueId.EnemyHit;
        [SerializeField, Min(0f)] private float damageCueIntensity = 0.65f;
        [SerializeField, Range(0.1f, 1f)] private float pressureDamageCueScale = 0.78f;
        [SerializeField] private bool playDamageVfx = true;
        [SerializeField] private Renderer[] damageFlashRenderers = Array.Empty<Renderer>();
        [SerializeField] private Color damageFlashColor = new Color(1f, 0.18f, 0.08f, 1f);
        [SerializeField] private Color damageFlashEmissionColor = new Color(1f, 0.48f, 0.24f, 1f);
        [SerializeField, Min(0.01f)] private float damageFlashSeconds = 0.14f;

        [Header("Pattern Cues")]
        [SerializeField] private PatternAnimationCue[] patternCues = Array.Empty<PatternAnimationCue>();

        [Header("Pressure Action Cues")]
        [SerializeField] private PressureActionCue[] pressureActionCues = Array.Empty<PressureActionCue>();

        private MaterialPropertyBlock propertyBlock;
        private MaterialPropertyBlock damagePropertyBlock;
        private Vector3 baseScale = Vector3.one;
        private Color activeColor;
        private float activePulseScale;
        private float cueTimer;
        private float cueDuration = 0.01f;
        private float damageFlashTimer;
        private bool subscribed;
        private bool pressureActionSubscribed;
        private bool damageFeedbackSubscribed;
        private string lastWindupTrigger = string.Empty;
        private string lastReleaseTrigger = string.Empty;
        private string lastPressureActionTrigger = string.Empty;
        private BossPressureActionKind lastPressureActionKind;
        private int lastPressureActionTier;
        private int pressureActionCueRequestCount;
        private int windupWorldVfxCueRequestCount;
        private int releaseWorldVfxCueRequestCount;
        private int pressureActionWorldVfxCueRequestCount;
        private int damageWorldVfxCueRequestCount;
        private CombatVfxCueId lastPressureActionWorldVfxCueId;
        private float lastDamageCueIntensity;

        public BossBarrageEmitter BossBarrageEmitter => bossBarrageEmitter;
        public BossPressureActionDirector BossPressureActionDirector => bossPressureActionDirector;
        public Animator Animator => animator;
        public Transform PulseRoot => pulseRoot;
        public CombatVfxCuePlayer CuePlayer => cuePlayer;
        public Transform VfxAnchor => vfxAnchor != null ? vfxAnchor : pulseRoot;
        public Transform VfxDirectionTarget => vfxDirectionTarget;
        public CombatVfxCueId WindupCueId => windupCueId;
        public CombatVfxCueId ReleaseCueId => releaseCueId;
        public CombatVfxCueId SkillPressureCueId => skillPressureCueId;
        public CombatVfxCueId SummonPressureCueId => summonPressureCueId;
        public CombatVfxCueId PunishPressureCueId => punishPressureCueId;
        public CombatVfxCueId DamageCueId => damageCueId;
        public bool PlayDamageVfx => playDamageVfx;
        public int DamageFlashRendererCount => damageFlashRenderers != null ? damageFlashRenderers.Length : 0;
        public int PulseRendererCount => pulseRenderers != null ? pulseRenderers.Length : 0;
        public int PatternCueCount => patternCues != null ? patternCues.Length : 0;
        public int PressureActionCueCount => pressureActionCues != null ? pressureActionCues.Length : 0;
        public bool IsCueActive => cueTimer > 0f;
        public string LastWindupTrigger => lastWindupTrigger;
        public string LastReleaseTrigger => lastReleaseTrigger;
        public string LastPressureActionTrigger => lastPressureActionTrigger;
        public BossPressureActionKind LastPressureActionKind => lastPressureActionKind;
        public int LastPressureActionTier => lastPressureActionTier;
        public int PressureActionCueRequestCount => pressureActionCueRequestCount;
        public int WindupWorldVfxCueRequestCount => windupWorldVfxCueRequestCount;
        public int ReleaseWorldVfxCueRequestCount => releaseWorldVfxCueRequestCount;
        public int PressureActionWorldVfxCueRequestCount => pressureActionWorldVfxCueRequestCount;
        public int DamageWorldVfxCueRequestCount => damageWorldVfxCueRequestCount;
        public CombatVfxCueId LastPressureActionWorldVfxCueId => lastPressureActionWorldVfxCueId;
        public float LastDamageCueIntensity => lastDamageCueIntensity;

        public bool TryGetPatternCue(int index, out PatternAnimationCue cue)
        {
            if (patternCues == null || index < 0 || index >= patternCues.Length)
            {
                cue = default;
                return false;
            }

            cue = patternCues[index];
            return true;
        }

        public bool TryGetPressureActionCue(int index, out PressureActionCue cue)
        {
            if (pressureActionCues == null || index < 0 || index >= pressureActionCues.Length)
            {
                cue = default;
                return false;
            }

            cue = pressureActionCues[index];
            return true;
        }

        public void ConfigurePresentation(
            BossBarrageEmitter newEmitter,
            Animator newAnimator,
            Transform newPulseRoot,
            Renderer[] newPulseRenderers)
        {
            Unsubscribe();
            bossBarrageEmitter = newEmitter;
            animator = newAnimator;
            pulseRoot = newPulseRoot;
            pulseRenderers = newPulseRenderers != null ? (Renderer[])newPulseRenderers.Clone() : Array.Empty<Renderer>();
            damageFlashRenderers = Array.Empty<Renderer>();
            CaptureBaseScale();
            ApplyColor(baseColor);
            Subscribe();
        }

        public void ConfigurePressureActionSource(BossPressureActionDirector newBossPressureActionDirector)
        {
            UnsubscribePressureActionSource();
            bossPressureActionDirector = newBossPressureActionDirector;
            SubscribePressureActionSource();
        }

        public void ConfigureWorldVfx(
            CombatVfxCuePlayer newCuePlayer,
            Transform newVfxAnchor,
            Transform newVfxDirectionTarget)
        {
            cuePlayer = newCuePlayer;
            vfxAnchor = newVfxAnchor;
            vfxDirectionTarget = newVfxDirectionTarget;
        }

        public void ResetToDefaultPatternCues()
        {
            patternCues = new[]
            {
                new PatternAnimationCue(
                    "NeedleLock",
                    "EliteAuraBuffer",
                    "AttackRetreatShot",
                    new Color(1f, 0.7f, 0.18f, 1f),
                    new Color(1f, 0.9f, 0.4f, 1f),
                    0.2f,
                    0.42f,
                    true,
                    CombatVfxCueId.EnemyRetreatShotWindup,
                    CombatVfxCueId.EnemyRetreatShotActive,
                    1.02f,
                    1.12f),
                new PatternAnimationCue(
                    "CoverFire",
                    "EliteAuraBuffer",
                    "AttackRetreatShot",
                    new Color(0.35f, 0.85f, 1f, 1f),
                    new Color(0.68f, 0.96f, 1f, 1f),
                    0.16f,
                    0.33f,
                    true,
                    CombatVfxCueId.EnemyFanPressureWindup,
                    CombatVfxCueId.EnemyRetreatShotActive,
                    0.95f,
                    1.08f),
                new PatternAnimationCue(
                    "EscortScreen",
                    "EliteSummonPackage",
                    "AttackFanPressure",
                    new Color(0.42f, 1f, 0.62f, 1f),
                    new Color(0.76f, 1f, 0.86f, 1f),
                    0.22f,
                    0.38f,
                    true,
                    CombatVfxCueId.EliteSummonSignal,
                    CombatVfxCueId.EnemyFanPressureActive,
                    1.15f,
                    1.14f),
                new PatternAnimationCue(
                    "LayeredSalvo",
                    "EliteSummonPackage",
                    "AttackHeavy",
                    new Color(1f, 0.46f, 0.18f, 1f),
                    new Color(1f, 0.72f, 0.36f, 1f),
                    0.26f,
                    0.48f,
                    true,
                    CombatVfxCueId.EnemyHeavyWindupWindup,
                    CombatVfxCueId.EnemyHeavyWindupActive,
                    1.12f,
                    1.22f),
                new PatternAnimationCue(
                    "StaggeredCrossfire",
                    "ElitePhaseSwap",
                    "AttackFanPressure",
                    new Color(0.74f, 0.48f, 1f, 1f),
                    new Color(0.92f, 0.78f, 1f, 1f),
                    0.24f,
                    0.42f,
                    true,
                    CombatVfxCueId.ElitePhaseSwapSignal,
                    CombatVfxCueId.EnemyFanPressureActive,
                    1.1f,
                    1.16f),
                new PatternAnimationCue(
                    "TwinSweep",
                    "EliteAuraBuffer",
                    "AttackLinePressure",
                    new Color(0.3f, 0.9f, 1f, 1f),
                    new Color(0.62f, 1f, 1f, 1f),
                    0.18f,
                    0.36f,
                    true,
                    CombatVfxCueId.EnemyLinePressureWindup,
                    CombatVfxCueId.EnemyLinePressureActive,
                    1.02f,
                    1.16f),
                new PatternAnimationCue(
                    "LeftClamp",
                    "EliteAuraBuffer",
                    "AttackLinePressure",
                    new Color(1f, 0.36f, 0.72f, 1f),
                    new Color(1f, 0.62f, 0.88f, 1f),
                    0.2f,
                    0.4f,
                    true,
                    CombatVfxCueId.EnemyLinePressureWindup,
                    CombatVfxCueId.EnemyLinePressureActive,
                    1.06f,
                    1.18f),
                new PatternAnimationCue(
                    "RightClamp",
                    "EliteAuraBuffer",
                    "AttackLinePressure",
                    new Color(1f, 0.36f, 0.72f, 1f),
                    new Color(1f, 0.62f, 0.88f, 1f),
                    0.2f,
                    0.4f,
                    true,
                    CombatVfxCueId.EnemyLinePressureWindup,
                    CombatVfxCueId.EnemyLinePressureActive,
                    1.06f,
                    1.18f),
                new PatternAnimationCue(
                    "PunishNet",
                    "EliteSummonPackage",
                    "AttackHeavy",
                    new Color(1f, 0.22f, 0.18f, 1f),
                    new Color(1f, 0.66f, 0.38f, 1f),
                    0.3f,
                    0.52f,
                    true,
                    CombatVfxCueId.EnemyGuardBreakWindup,
                    CombatVfxCueId.EnemyGuardBreakActive,
                    1.18f,
                    1.28f),
                new PatternAnimationCue(
                    "LinePressure",
                    "EliteAuraBuffer",
                    "AttackLinePressure",
                    new Color(1f, 0.82f, 0.22f, 1f),
                    new Color(1f, 0.98f, 0.58f, 1f),
                    0.2f,
                    0.38f,
                    true,
                    CombatVfxCueId.EnemyLinePressureWindup,
                    CombatVfxCueId.EnemyLinePressureActive,
                    1.08f,
                    1.2f)
            };
        }

        public void ResetToDefaultPressureActionCues()
        {
            pressureActionCues = new[]
            {
                new PressureActionCue(
                    BossPressureActionKind.SkillPattern,
                    "AttackLinePressure",
                    new Color(1f, 0.88f, 0.34f, 1f),
                    0.28f,
                    0.28f,
                    0.06f),
                new PressureActionCue(
                    BossPressureActionKind.SummonPressure,
                    "EliteSummonPackage",
                    new Color(0.35f, 1f, 0.78f, 1f),
                    0.36f,
                    0.34f,
                    0.08f),
                new PressureActionCue(
                    BossPressureActionKind.PunishOverextend,
                    "AttackHeavy",
                    new Color(1f, 0.24f, 0.18f, 1f),
                    0.40f,
                    0.42f,
                    0.10f)
            };
        }

        private void Awake()
        {
            if (bossBarrageEmitter == null)
            {
                bossBarrageEmitter = GetComponentInParent<BossBarrageEmitter>();
            }

            if (bossPressureActionDirector == null)
            {
                bossPressureActionDirector = GetComponentInParent<BossPressureActionDirector>();
            }

            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>(true);
            }

            if (pulseRoot == null)
            {
                pulseRoot = transform;
            }

            if (pulseRenderers == null || pulseRenderers.Length == 0)
            {
                pulseRenderers = pulseRoot.GetComponentsInChildren<Renderer>(true);
            }

            if (damageFeedbackHealth == null)
            {
                damageFeedbackHealth = GetComponentInParent<CombatHealth>();
            }

            if (damageFlashRenderers == null || damageFlashRenderers.Length == 0)
            {
                damageFlashRenderers = ResolveDamageFlashRenderers();
            }

            CaptureBaseScale();
            ApplyColor(baseColor);
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
            ResetPulse();
            ClearDamageFlash();
        }

        private void Update()
        {
            if (cueTimer > 0f)
            {
                cueTimer = Mathf.Max(0f, cueTimer - Time.deltaTime);
            }

            RefreshPulse();
            RefreshDamageFlash();
        }

        private void OnWindupStarted(BossBarrageEmitter emitter, BossBarragePatternProfile pattern)
        {
            PatternAnimationCue cue = ResolveCue(pattern);
            string trigger = string.IsNullOrWhiteSpace(cue.WindupTrigger) ? defaultWindupTrigger : cue.WindupTrigger;
            lastWindupTrigger = trigger;
            TriggerAnimator(trigger);
            StartCue(
                cue.WindupColor,
                Mathf.Max(0.01f, pattern != null ? pattern.WindupSeconds : 0.01f),
                cue.WindupPulseScale);
            if (PlayWorldVfx(ResolveWindupWorldVfxCueId(cue), 1, windupCueIntensity * cue.WindupWorldCueIntensity + cue.WindupPulseScale))
            {
                windupWorldVfxCueRequestCount++;
            }
        }

        private void OnWaveFired(BossBarrageEmitter emitter, BossBarragePatternProfile pattern, int spawnedCount)
        {
            PatternAnimationCue cue = ResolveCue(pattern);
            string trigger = string.IsNullOrWhiteSpace(cue.ReleaseTrigger) ? defaultReleaseTrigger : cue.ReleaseTrigger;
            lastReleaseTrigger = trigger;
            TriggerAnimator(trigger);
            StartCue(cue.ReleaseColor, releaseFlashSeconds, cue.ReleasePulseScale);
            if (PlayWorldVfx(ResolveReleaseWorldVfxCueId(cue), 1, releaseCueIntensity * cue.ReleaseWorldCueIntensity + cue.ReleasePulseScale))
            {
                releaseWorldVfxCueRequestCount++;
            }
        }

        private void OnPressureActionQueued(
            BossPressureActionDirector director,
            BossPressureActionKind actionKind,
            BossBarragePatternProfile pattern,
            int spentTier)
        {
            PressureActionCue cue = ResolvePressureActionCue(actionKind);
            string trigger = string.IsNullOrWhiteSpace(cue.Trigger) ? defaultReleaseTrigger : cue.Trigger;
            lastPressureActionKind = actionKind;
            lastPressureActionTrigger = trigger;
            lastPressureActionTier = Mathf.Clamp(spentTier, 1, 3);
            pressureActionCueRequestCount++;
            TriggerAnimator(trigger);
            StartCue(cue.Color, cue.DurationSeconds, cue.ResolvePulseScale(lastPressureActionTier));
            CombatVfxCueId worldCueId = ResolvePressureActionWorldVfxCueId(actionKind);
            lastPressureActionWorldVfxCueId = worldCueId;
            if (PlayWorldVfx(worldCueId, lastPressureActionTier, pressureActionCueIntensity + cue.ResolvePulseScale(lastPressureActionTier)))
            {
                pressureActionWorldVfxCueRequestCount++;
            }
        }

        private void OnDamaged(DamageInfo damageInfo)
        {
            if (!DamageResponsePolicyUtility.PlaysDamagePresentation(damageInfo.ResponsePolicy))
            {
                return;
            }

            float policyScale = DamageResponsePolicyUtility.InterruptsAction(damageInfo.ControlLockPolicy)
                ? 1f
                : Mathf.Clamp(pressureDamageCueScale, 0.1f, 1f);
            lastDamageCueIntensity = damageCueIntensity * policyScale;
            damageFlashTimer = Mathf.Max(damageFlashTimer, damageFlashSeconds);
            StartCue(new Color(1f, 0.22f, 0.12f, 1f), 0.14f, 0.26f);

            if (playDamageVfx && PlayWorldVfx(damageCueId, 1, lastDamageCueIntensity))
            {
                damageWorldVfxCueRequestCount++;
            }
        }

        private void Subscribe()
        {
            SubscribeBarrageEmitter();
            SubscribePressureActionSource();
            SubscribeDamageFeedback();
        }

        private void SubscribeBarrageEmitter()
        {
            if (subscribed || bossBarrageEmitter == null)
            {
                return;
            }

            bossBarrageEmitter.WindupStarted += OnWindupStarted;
            bossBarrageEmitter.WaveFired += OnWaveFired;
            subscribed = true;
        }

        private void SubscribePressureActionSource()
        {
            if (pressureActionSubscribed || bossPressureActionDirector == null)
            {
                return;
            }

            bossPressureActionDirector.ActionQueued += OnPressureActionQueued;
            pressureActionSubscribed = true;
        }

        private void SubscribeDamageFeedback()
        {
            if (damageFeedbackSubscribed)
            {
                return;
            }

            if (damageFeedbackHealth == null)
            {
                damageFeedbackHealth = GetComponentInParent<CombatHealth>();
            }

            if (damageFeedbackHealth == null)
            {
                return;
            }

            damageFeedbackHealth.Damaged += OnDamaged;
            damageFeedbackSubscribed = true;
        }

        private void Unsubscribe()
        {
            UnsubscribeBarrageEmitter();
            UnsubscribePressureActionSource();
            UnsubscribeDamageFeedback();
        }

        private void UnsubscribeBarrageEmitter()
        {
            if (!subscribed || bossBarrageEmitter == null)
            {
                subscribed = false;
                return;
            }

            bossBarrageEmitter.WindupStarted -= OnWindupStarted;
            bossBarrageEmitter.WaveFired -= OnWaveFired;
            subscribed = false;
        }

        private void UnsubscribePressureActionSource()
        {
            if (!pressureActionSubscribed || bossPressureActionDirector == null)
            {
                pressureActionSubscribed = false;
                return;
            }

            bossPressureActionDirector.ActionQueued -= OnPressureActionQueued;
            pressureActionSubscribed = false;
        }

        private void UnsubscribeDamageFeedback()
        {
            if (!damageFeedbackSubscribed || damageFeedbackHealth == null)
            {
                damageFeedbackSubscribed = false;
                return;
            }

            damageFeedbackHealth.Damaged -= OnDamaged;
            damageFeedbackSubscribed = false;
        }

        private PatternAnimationCue ResolveCue(BossBarragePatternProfile pattern)
        {
            if (pattern != null && patternCues != null)
            {
                for (int i = 0; i < patternCues.Length; i++)
                {
                    if (patternCues[i].Matches(pattern.PatternId))
                    {
                        return patternCues[i];
                    }
                }
            }

            return new PatternAnimationCue(
                pattern != null ? pattern.PatternId : string.Empty,
                defaultWindupTrigger,
                defaultReleaseTrigger,
                defaultWindupColor,
                defaultReleaseColor,
                defaultWindupPulseScale,
                defaultReleasePulseScale);
        }

        private PressureActionCue ResolvePressureActionCue(BossPressureActionKind actionKind)
        {
            if (pressureActionCues != null)
            {
                for (int i = 0; i < pressureActionCues.Length; i++)
                {
                    if (pressureActionCues[i].Matches(actionKind))
                    {
                        return pressureActionCues[i];
                    }
                }
            }

            return new PressureActionCue(
                actionKind,
                defaultReleaseTrigger,
                defaultReleaseColor,
                releaseFlashSeconds,
                defaultReleasePulseScale,
                0f);
        }

        private void StartCue(Color cueColor, float duration, float pulseScale)
        {
            activeColor = cueColor;
            activePulseScale = pulseScale;
            cueDuration = Mathf.Max(0.01f, duration);
            cueTimer = cueDuration;
            ApplyColor(cueColor);
            RefreshPulse();
        }

        private void RefreshPulse()
        {
            if (pulseRoot == null)
            {
                return;
            }

            if (cueTimer <= 0f)
            {
                pulseRoot.localScale = baseScale;
                ApplyColor(baseColor);
                return;
            }

            float cue01 = Mathf.Clamp01(cueTimer / Mathf.Max(0.01f, cueDuration));
            float wave = Mathf.Sin(Time.time * pulseSpeed) * 0.5f + 0.5f;
            float scale = 1f + activePulseScale * cue01 * (0.42f + wave * 0.58f);
            pulseRoot.localScale = baseScale * scale;
            ApplyColor(Color.Lerp(baseColor, activeColor, cue01));
        }

        private void ResetPulse()
        {
            cueTimer = 0f;
            if (pulseRoot != null)
            {
                pulseRoot.localScale = baseScale;
            }

            ApplyColor(baseColor);
        }

        private void RefreshDamageFlash()
        {
            if (damageFlashTimer <= 0f)
            {
                return;
            }

            damageFlashTimer = Mathf.Max(0f, damageFlashTimer - Time.deltaTime);
            if (damageFlashTimer <= 0f)
            {
                ClearDamageFlash();
                return;
            }

            float flash01 = Mathf.Clamp01(damageFlashTimer / Mathf.Max(0.01f, damageFlashSeconds));
            float weight = Mathf.SmoothStep(0f, 1f, flash01);
            ApplyDamageFlash(
                Color.Lerp(Color.white, damageFlashColor, weight),
                damageFlashEmissionColor * (0.35f + weight * 1.65f));
        }

        private Renderer[] ResolveDamageFlashRenderers()
        {
            Transform root = animator != null ? animator.transform : transform;
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(includeInactive: true);
            if (renderers == null || renderers.Length == 0)
            {
                return Array.Empty<Renderer>();
            }

            var results = new System.Collections.Generic.List<Renderer>(renderers.Length);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer candidate = renderers[i];
                if (candidate == null || IsPulseRenderer(candidate))
                {
                    continue;
                }

                results.Add(candidate);
            }

            return results.ToArray();
        }

        private bool IsPulseRenderer(Renderer renderer)
        {
            if (pulseRenderers == null)
            {
                return false;
            }

            for (int i = 0; i < pulseRenderers.Length; i++)
            {
                if (pulseRenderers[i] == renderer)
                {
                    return true;
                }
            }

            return false;
        }

        private void ApplyDamageFlash(Color color, Color emissionColor)
        {
            if (damageFlashRenderers == null)
            {
                return;
            }

            damagePropertyBlock ??= new MaterialPropertyBlock();
            for (int i = 0; i < damageFlashRenderers.Length; i++)
            {
                Renderer targetRenderer = damageFlashRenderers[i];
                if (targetRenderer == null)
                {
                    continue;
                }

                targetRenderer.GetPropertyBlock(damagePropertyBlock);
                damagePropertyBlock.SetColor(BaseColorId, color);
                damagePropertyBlock.SetColor(ColorId, color);
                damagePropertyBlock.SetColor("_EmissionColor", emissionColor);
                targetRenderer.SetPropertyBlock(damagePropertyBlock);
            }
        }

        private void ClearDamageFlash()
        {
            if (damageFlashRenderers == null)
            {
                return;
            }

            for (int i = 0; i < damageFlashRenderers.Length; i++)
            {
                if (damageFlashRenderers[i] != null)
                {
                    damageFlashRenderers[i].SetPropertyBlock(null);
                }
            }
        }

        private void CaptureBaseScale()
        {
            if (pulseRoot != null)
            {
                baseScale = pulseRoot.localScale;
            }
        }

        private void ApplyColor(Color color)
        {
            if (pulseRenderers == null || pulseRenderers.Length == 0)
            {
                return;
            }

            propertyBlock ??= new MaterialPropertyBlock();
            for (int i = 0; i < pulseRenderers.Length; i++)
            {
                Renderer renderer = pulseRenderers[i];
                if (renderer == null)
                {
                    continue;
                }

                renderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor(BaseColorId, color);
                propertyBlock.SetColor(ColorId, color);
                renderer.SetPropertyBlock(propertyBlock);
            }
        }

        private void TriggerAnimator(string triggerName)
        {
            if (animator == null || string.IsNullOrWhiteSpace(triggerName) || !HasAnimatorTrigger(triggerName))
            {
                return;
            }

            animator.SetTrigger(triggerName);
        }

        private bool HasAnimatorTrigger(string triggerName)
        {
            AnimatorControllerParameter[] parameters = animator.parameters;
            for (int i = 0; i < parameters.Length; i++)
            {
                AnimatorControllerParameter parameter = parameters[i];
                if (parameter.type == AnimatorControllerParameterType.Trigger
                    && string.Equals(parameter.name, triggerName, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private CombatVfxCueId ResolvePressureActionWorldVfxCueId(BossPressureActionKind actionKind)
        {
            return actionKind switch
            {
                BossPressureActionKind.SummonPressure => summonPressureCueId,
                BossPressureActionKind.PunishOverextend => punishPressureCueId,
                _ => skillPressureCueId
            };
        }

        private CombatVfxCueId ResolveWindupWorldVfxCueId(PatternAnimationCue cue)
        {
            return cue.UseWorldVfxCueOverride ? cue.WindupWorldCueId : windupCueId;
        }

        private CombatVfxCueId ResolveReleaseWorldVfxCueId(PatternAnimationCue cue)
        {
            return cue.UseWorldVfxCueOverride ? cue.ReleaseWorldCueId : releaseCueId;
        }

        private bool PlayWorldVfx(CombatVfxCueId cueId, int tier, float baseIntensity)
        {
            if (cuePlayer == null)
            {
                return false;
            }

            Transform anchor = VfxAnchor != null ? VfxAnchor : transform;
            float intensity = baseIntensity + Mathf.Max(0, tier - 1) * tierCueIntensityStep;
            return cuePlayer.PlayCue(cueId, anchor, ResolveWorldVfxDirection(anchor), Mathf.Max(0f, intensity));
        }

        private Vector3 ResolveWorldVfxDirection(Transform anchor)
        {
            if (anchor != null && vfxDirectionTarget != null)
            {
                Vector3 targetDirection = Vector3.ProjectOnPlane(vfxDirectionTarget.position - anchor.position, Vector3.up);
                if (targetDirection.sqrMagnitude > 0.0001f)
                {
                    return targetDirection.normalized;
                }
            }

            if (anchor != null)
            {
                Vector3 forward = Vector3.ProjectOnPlane(anchor.forward, Vector3.up);
                if (forward.sqrMagnitude > 0.0001f)
                {
                    return forward.normalized;
                }
            }

            return Vector3.back;
        }
    }
}
