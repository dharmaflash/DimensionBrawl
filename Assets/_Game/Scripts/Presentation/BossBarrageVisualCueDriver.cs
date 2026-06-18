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

            public PatternAnimationCue(
                string patternId,
                string windupTrigger,
                string releaseTrigger,
                Color windupColor,
                Color releaseColor,
                float windupPulseScale,
                float releasePulseScale)
            {
                this.patternId = patternId;
                this.windupTrigger = windupTrigger;
                this.releaseTrigger = releaseTrigger;
                this.windupColor = windupColor;
                this.releaseColor = releaseColor;
                this.windupPulseScale = windupPulseScale;
                this.releasePulseScale = releasePulseScale;
            }

            public string PatternId => patternId;
            public string WindupTrigger => windupTrigger;
            public string ReleaseTrigger => releaseTrigger;
            public Color WindupColor => windupColor;
            public Color ReleaseColor => releaseColor;
            public float WindupPulseScale => windupPulseScale;
            public float ReleasePulseScale => releasePulseScale;

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

        [Header("Pattern Cues")]
        [SerializeField] private PatternAnimationCue[] patternCues = Array.Empty<PatternAnimationCue>();

        [Header("Pressure Action Cues")]
        [SerializeField] private PressureActionCue[] pressureActionCues = Array.Empty<PressureActionCue>();

        private MaterialPropertyBlock propertyBlock;
        private Vector3 baseScale = Vector3.one;
        private Color activeColor;
        private float activePulseScale;
        private float cueTimer;
        private float cueDuration = 0.01f;
        private bool subscribed;
        private bool pressureActionSubscribed;
        private string lastWindupTrigger = string.Empty;
        private string lastReleaseTrigger = string.Empty;
        private string lastPressureActionTrigger = string.Empty;
        private BossPressureActionKind lastPressureActionKind;
        private int lastPressureActionTier;
        private int pressureActionCueRequestCount;

        public BossBarrageEmitter BossBarrageEmitter => bossBarrageEmitter;
        public BossPressureActionDirector BossPressureActionDirector => bossPressureActionDirector;
        public Animator Animator => animator;
        public Transform PulseRoot => pulseRoot;
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
                    0.42f),
                new PatternAnimationCue(
                    "CoverFire",
                    "EliteAuraBuffer",
                    "AttackRetreatShot",
                    new Color(0.35f, 0.85f, 1f, 1f),
                    new Color(0.68f, 0.96f, 1f, 1f),
                    0.16f,
                    0.33f),
                new PatternAnimationCue(
                    "EscortScreen",
                    "EliteSummonPackage",
                    "AttackFanPressure",
                    new Color(0.42f, 1f, 0.62f, 1f),
                    new Color(0.76f, 1f, 0.86f, 1f),
                    0.22f,
                    0.38f),
                new PatternAnimationCue(
                    "LayeredSalvo",
                    "EliteSummonPackage",
                    "AttackHeavy",
                    new Color(1f, 0.46f, 0.18f, 1f),
                    new Color(1f, 0.72f, 0.36f, 1f),
                    0.26f,
                    0.48f),
                new PatternAnimationCue(
                    "StaggeredCrossfire",
                    "ElitePhaseSwap",
                    "AttackFanPressure",
                    new Color(0.74f, 0.48f, 1f, 1f),
                    new Color(0.92f, 0.78f, 1f, 1f),
                    0.24f,
                    0.42f),
                new PatternAnimationCue(
                    "TwinSweep",
                    "EliteAuraBuffer",
                    "AttackLinePressure",
                    new Color(0.3f, 0.9f, 1f, 1f),
                    new Color(0.62f, 1f, 1f, 1f),
                    0.18f,
                    0.36f),
                new PatternAnimationCue(
                    "LeftClamp",
                    "EliteAuraBuffer",
                    "AttackLinePressure",
                    new Color(1f, 0.36f, 0.72f, 1f),
                    new Color(1f, 0.62f, 0.88f, 1f),
                    0.2f,
                    0.4f),
                new PatternAnimationCue(
                    "RightClamp",
                    "EliteAuraBuffer",
                    "AttackLinePressure",
                    new Color(1f, 0.36f, 0.72f, 1f),
                    new Color(1f, 0.62f, 0.88f, 1f),
                    0.2f,
                    0.4f),
                new PatternAnimationCue(
                    "PunishNet",
                    "EliteSummonPackage",
                    "AttackHeavy",
                    new Color(1f, 0.22f, 0.18f, 1f),
                    new Color(1f, 0.66f, 0.38f, 1f),
                    0.3f,
                    0.52f),
                new PatternAnimationCue(
                    "LinePressure",
                    "EliteAuraBuffer",
                    "AttackLinePressure",
                    new Color(1f, 0.82f, 0.22f, 1f),
                    new Color(1f, 0.98f, 0.58f, 1f),
                    0.2f,
                    0.38f)
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
        }

        private void Update()
        {
            if (cueTimer > 0f)
            {
                cueTimer = Mathf.Max(0f, cueTimer - Time.deltaTime);
            }

            RefreshPulse();
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
        }

        private void OnWaveFired(BossBarrageEmitter emitter, BossBarragePatternProfile pattern, int spawnedCount)
        {
            PatternAnimationCue cue = ResolveCue(pattern);
            string trigger = string.IsNullOrWhiteSpace(cue.ReleaseTrigger) ? defaultReleaseTrigger : cue.ReleaseTrigger;
            lastReleaseTrigger = trigger;
            TriggerAnimator(trigger);
            StartCue(cue.ReleaseColor, releaseFlashSeconds, cue.ReleasePulseScale);
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
        }

        private void Subscribe()
        {
            SubscribeBarrageEmitter();
            SubscribePressureActionSource();
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

        private void Unsubscribe()
        {
            UnsubscribeBarrageEmitter();
            UnsubscribePressureActionSource();
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
    }
}
