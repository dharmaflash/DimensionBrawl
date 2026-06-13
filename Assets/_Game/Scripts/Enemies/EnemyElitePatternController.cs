using System;
using DimensionBrawl.AI;
using DimensionBrawl.Combat;
using UnityEngine;

namespace DimensionBrawl.Enemies
{
    [DisallowMultipleComponent]
    public sealed class EnemyElitePatternController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CombatHealth health;
        [SerializeField] private BasicSoldierEnemy soldier;
        [SerializeField] private Animator animator;
        [SerializeField] private Renderer cueRenderer;
        [SerializeField] private CombatHealth[] auraProtectedTargets = Array.Empty<CombatHealth>();
        [SerializeField] private GameObject[] summonSignalObjects = Array.Empty<GameObject>();

        [Header("Profiles")]
        [SerializeField] private CombatAiElitePatternProfile[] eliteProfiles = Array.Empty<CombatAiElitePatternProfile>();

        [Header("Presentation")]
        [SerializeField] private string colorProperty = "_BaseColor";

        private ElitePatternRuntimeState[] states = Array.Empty<ElitePatternRuntimeState>();
        private MaterialPropertyBlock propertyBlock;

        public int ProfileCount => eliteProfiles != null ? eliteProfiles.Length : 0;
        public bool HasActiveSignal => TryGetActiveSignal(out _);
        public bool HasPhaseSwapped { get; private set; }
        public event Action<CombatAiElitePatternProfile> SignalTriggered;

        private void Awake()
        {
            if (health == null)
            {
                health = GetComponent<CombatHealth>();
            }

            if (soldier == null)
            {
                soldier = GetComponent<BasicSoldierEnemy>();
            }

            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>(true);
            }

            propertyBlock = new MaterialPropertyBlock();
            ResetRuntimeStates();
        }

        private void OnEnable()
        {
            ResetRuntimeStates();
            if (health != null)
            {
                health.DamageModifying += HandleDamageModifying;
                health.Damaged += HandleDamaged;
            }

            SubscribeAuraTargets();
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.DamageModifying -= HandleDamageModifying;
                health.Damaged -= HandleDamaged;
            }

            UnsubscribeAuraTargets();
            SetSummonSignalObjectsActive(false);
            ClearCueColor();
        }

        private void Update()
        {
            if (health == null || !health.IsAlive)
            {
                ClearCueColor();
                return;
            }

            for (int i = 0; i < states.Length; i++)
            {
                CombatAiElitePatternProfile profile = ResolveProfile(i);
                if (profile == null)
                {
                    continue;
                }

                UpdateProfileState(profile, ref states[i]);
            }

            ApplyReadableCue();
            ApplySummonSignalObjects();
        }

        public bool TryGetProfileState(string patternId, out float guardMeter, out bool isBroken, out bool isSignalActive)
        {
            for (int i = 0; i < states.Length; i++)
            {
                CombatAiElitePatternProfile profile = ResolveProfile(i);
                if (profile == null || !string.Equals(profile.PatternId, patternId, StringComparison.Ordinal))
                {
                    continue;
                }

                guardMeter = states[i].GuardMeter;
                isBroken = states[i].IsBroken;
                isSignalActive = Time.time < states[i].SignalUntilTime;
                return true;
            }

            guardMeter = 0f;
            isBroken = false;
            isSignalActive = false;
            return false;
        }

        private void ResetRuntimeStates()
        {
            int profileCount = eliteProfiles != null ? eliteProfiles.Length : 0;
            if (states == null || states.Length != profileCount)
            {
                states = new ElitePatternRuntimeState[profileCount];
            }

            for (int i = 0; i < states.Length; i++)
            {
                CombatAiElitePatternProfile profile = ResolveProfile(i);
                states[i] = new ElitePatternRuntimeState
                {
                    GuardMeter = profile != null ? profile.GuardMeter : 0f,
                    IsBroken = false,
                    HasTriggered = false,
                    BrokenUntilTime = -1f,
                    NextRefreshTime = -1f,
                    SignalUntilTime = -1f
                };
            }

            HasPhaseSwapped = false;
        }

        private void HandleDamageModifying(DamageModificationContext context)
        {
            if (context == null || health == null || !health.IsAlive)
            {
                return;
            }

            for (int i = 0; i < states.Length; i++)
            {
                CombatAiElitePatternProfile profile = ResolveProfile(i);
                if (profile == null || !ShouldRunProfile(profile))
                {
                    continue;
                }

                if (profile.PatternKind != CombatAiElitePatternKind.ShieldCycle
                    && profile.PatternKind != CombatAiElitePatternKind.ArmorBreak)
                {
                    continue;
                }

                if (states[i].IsBroken)
                {
                    continue;
                }

                if (states[i].GuardMeter <= 0f)
                {
                    continue;
                }

                context.ScaleAmount(profile.DamageTakenMultiplier);
                states[i].GuardMeter = Mathf.Max(0f, states[i].GuardMeter - context.DamageInfo.Amount);
                if (states[i].GuardMeter <= 0f)
                {
                    BreakProfile(profile, ref states[i]);
                }
            }
        }

        private void HandleDamaged(DamageInfo _)
        {
            if (health == null || !health.IsAlive)
            {
                return;
            }

            for (int i = 0; i < states.Length; i++)
            {
                CombatAiElitePatternProfile profile = ResolveProfile(i);
                if (profile == null || !ShouldRunProfile(profile))
                {
                    continue;
                }

                if (profile.PatternKind == CombatAiElitePatternKind.SummonPackage)
                {
                    TriggerSignal(profile, ref states[i]);
                }
            }
        }

        private void HandleAuraTargetDamageModifying(DamageModificationContext context)
        {
            if (context == null)
            {
                return;
            }

            if (TryGetRunningProfile(CombatAiElitePatternKind.AuraBuffer, out CombatAiElitePatternProfile auraProfile))
            {
                context.ScaleAmount(auraProfile.DamageTakenMultiplier);
            }
        }

        private void UpdateProfileState(CombatAiElitePatternProfile profile, ref ElitePatternRuntimeState state)
        {
            if (!ShouldRunProfile(profile))
            {
                return;
            }

            switch (profile.PatternKind)
            {
                case CombatAiElitePatternKind.ShieldCycle:
                    TriggerSignalOnce(profile, ref state);
                    RefreshGuardMeter(profile, ref state);
                    break;
                case CombatAiElitePatternKind.ArmorBreak:
                    TriggerSignalOnce(profile, ref state);
                    ClearBreakWhenExpired(ref state);
                    break;
                case CombatAiElitePatternKind.AuraBuffer:
                    TriggerSignal(profile, ref state);
                    break;
                case CombatAiElitePatternKind.SummonPackage:
                    TriggerSignal(profile, ref state);
                    break;
                case CombatAiElitePatternKind.PhaseSwap:
                    TryApplyPhaseSwap(profile, ref state);
                    break;
            }
        }

        private void BreakProfile(CombatAiElitePatternProfile profile, ref ElitePatternRuntimeState state)
        {
            state.IsBroken = true;
            state.BrokenUntilTime = Time.time + profile.BreakSeconds;
            state.NextRefreshTime = state.BrokenUntilTime + profile.RefreshSeconds;
            state.SignalUntilTime = Mathf.Max(state.SignalUntilTime, Time.time + profile.SignalSeconds);
            TriggerProfileAnimation(profile);
            SignalTriggered?.Invoke(profile);
        }

        private void RefreshGuardMeter(CombatAiElitePatternProfile profile, ref ElitePatternRuntimeState state)
        {
            if (!state.IsBroken)
            {
                return;
            }

            if (Time.time < state.NextRefreshTime)
            {
                return;
            }

            state.IsBroken = false;
            state.GuardMeter = profile.GuardMeter;
            state.SignalUntilTime = Mathf.Max(state.SignalUntilTime, Time.time + profile.SignalSeconds);
            TriggerProfileAnimation(profile);
            SignalTriggered?.Invoke(profile);
        }

        private static void ClearBreakWhenExpired(ref ElitePatternRuntimeState state)
        {
            if (state.IsBroken && Time.time >= state.BrokenUntilTime)
            {
                state.IsBroken = false;
            }
        }

        private void TriggerSignal(CombatAiElitePatternProfile profile, ref ElitePatternRuntimeState state)
        {
            if (state.HasTriggered)
            {
                return;
            }

            state.HasTriggered = true;
            state.SignalUntilTime = Time.time + profile.SignalSeconds;
            TriggerProfileAnimation(profile);
            SignalTriggered?.Invoke(profile);
        }

        private void TryApplyPhaseSwap(CombatAiElitePatternProfile profile, ref ElitePatternRuntimeState state)
        {
            if (state.HasTriggered)
            {
                return;
            }

            state.HasTriggered = true;
            HasPhaseSwapped = true;
            state.SignalUntilTime = Time.time + profile.SignalSeconds;
            TriggerProfileAnimation(profile);
            SignalTriggered?.Invoke(profile);

            if (soldier == null)
            {
                return;
            }

            if (profile.ReplacementPatternDeck != null)
            {
                soldier.ConfigurePatternDeck(profile.ReplacementPatternDeck);
            }

            if (profile.ReplacementPatternProfile != null)
            {
                soldier.ConfigurePattern(profile.ReplacementPatternProfile);
            }
        }

        private void TriggerSignalOnce(CombatAiElitePatternProfile profile, ref ElitePatternRuntimeState state)
        {
            if (state.HasTriggered)
            {
                return;
            }

            state.HasTriggered = true;
            state.SignalUntilTime = Time.time + profile.SignalSeconds;
            TriggerProfileAnimation(profile);
            SignalTriggered?.Invoke(profile);
        }

        private void TriggerProfileAnimation(CombatAiElitePatternProfile profile)
        {
            if (animator == null || profile == null || string.IsNullOrWhiteSpace(profile.SignalAnimationTrigger))
            {
                return;
            }

            animator.SetTrigger(profile.SignalAnimationTrigger);
        }

        private bool ShouldRunProfile(CombatAiElitePatternProfile profile)
        {
            return health != null
                && health.HealthRatio <= profile.TriggerHealthRatio;
        }

        private bool TryGetRunningProfile(CombatAiElitePatternKind kind, out CombatAiElitePatternProfile runningProfile)
        {
            for (int i = 0; i < states.Length; i++)
            {
                CombatAiElitePatternProfile profile = ResolveProfile(i);
                if (profile == null || profile.PatternKind != kind || !ShouldRunProfile(profile))
                {
                    continue;
                }

                runningProfile = profile;
                return true;
            }

            runningProfile = null;
            return false;
        }

        private bool HasActiveSignalOfKind(CombatAiElitePatternKind kind)
        {
            for (int i = 0; i < states.Length; i++)
            {
                CombatAiElitePatternProfile profile = ResolveProfile(i);
                if (profile != null
                    && profile.PatternKind == kind
                    && Time.time < states[i].SignalUntilTime)
                {
                    return true;
                }
            }

            return false;
        }

        private bool TryGetActiveSignal(out CombatAiElitePatternProfile activeProfile)
        {
            for (int i = 0; i < states.Length; i++)
            {
                CombatAiElitePatternProfile profile = ResolveProfile(i);
                if (profile != null && Time.time < states[i].SignalUntilTime)
                {
                    activeProfile = profile;
                    return true;
                }
            }

            activeProfile = null;
            return false;
        }

        private CombatAiElitePatternProfile ResolveProfile(int index)
        {
            return eliteProfiles != null && index >= 0 && index < eliteProfiles.Length
                ? eliteProfiles[index]
                : null;
        }

        private void ApplyReadableCue()
        {
            if (cueRenderer == null || !TryGetActiveSignal(out CombatAiElitePatternProfile profile))
            {
                ClearCueColor();
                return;
            }

            propertyBlock ??= new MaterialPropertyBlock();
            cueRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(colorProperty, profile.CueColor);
            propertyBlock.SetColor("_BaseColor", profile.CueColor);
            propertyBlock.SetColor("_Color", profile.CueColor);
            cueRenderer.SetPropertyBlock(propertyBlock);
        }

        private void ApplySummonSignalObjects()
        {
            SetSummonSignalObjectsActive(HasActiveSignalOfKind(CombatAiElitePatternKind.SummonPackage));
        }

        private void SetSummonSignalObjectsActive(bool active)
        {
            if (summonSignalObjects == null)
            {
                return;
            }

            for (int i = 0; i < summonSignalObjects.Length; i++)
            {
                if (summonSignalObjects[i] != null && summonSignalObjects[i].activeSelf != active)
                {
                    summonSignalObjects[i].SetActive(active);
                }
            }
        }

        private void SubscribeAuraTargets()
        {
            if (auraProtectedTargets == null)
            {
                return;
            }

            for (int i = 0; i < auraProtectedTargets.Length; i++)
            {
                if (auraProtectedTargets[i] != null)
                {
                    auraProtectedTargets[i].DamageModifying += HandleAuraTargetDamageModifying;
                }
            }
        }

        private void UnsubscribeAuraTargets()
        {
            if (auraProtectedTargets == null)
            {
                return;
            }

            for (int i = 0; i < auraProtectedTargets.Length; i++)
            {
                if (auraProtectedTargets[i] != null)
                {
                    auraProtectedTargets[i].DamageModifying -= HandleAuraTargetDamageModifying;
                }
            }
        }

        private void ClearCueColor()
        {
            if (cueRenderer != null)
            {
                cueRenderer.SetPropertyBlock(null);
            }
        }

        private struct ElitePatternRuntimeState
        {
            public float GuardMeter;
            public bool IsBroken;
            public bool HasTriggered;
            public float BrokenUntilTime;
            public float NextRefreshTime;
            public float SignalUntilTime;
        }
    }
}
