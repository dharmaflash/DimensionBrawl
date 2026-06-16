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

        [Header("References")]
        [SerializeField] private BossBarrageEmitter bossBarrageEmitter;
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

        private MaterialPropertyBlock propertyBlock;
        private Vector3 baseScale = Vector3.one;
        private Color activeColor;
        private float activePulseScale;
        private float cueTimer;
        private float cueDuration = 0.01f;
        private bool subscribed;
        private string lastWindupTrigger = string.Empty;
        private string lastReleaseTrigger = string.Empty;

        public BossBarrageEmitter BossBarrageEmitter => bossBarrageEmitter;
        public Animator Animator => animator;
        public Transform PulseRoot => pulseRoot;
        public int PulseRendererCount => pulseRenderers != null ? pulseRenderers.Length : 0;
        public int PatternCueCount => patternCues != null ? patternCues.Length : 0;
        public bool IsCueActive => cueTimer > 0f;
        public string LastWindupTrigger => lastWindupTrigger;
        public string LastReleaseTrigger => lastReleaseTrigger;

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

        private void Awake()
        {
            if (bossBarrageEmitter == null)
            {
                bossBarrageEmitter = GetComponentInParent<BossBarrageEmitter>();
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

        private void Subscribe()
        {
            if (subscribed || bossBarrageEmitter == null)
            {
                return;
            }

            bossBarrageEmitter.WindupStarted += OnWindupStarted;
            bossBarrageEmitter.WaveFired += OnWaveFired;
            subscribed = true;
        }

        private void Unsubscribe()
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
