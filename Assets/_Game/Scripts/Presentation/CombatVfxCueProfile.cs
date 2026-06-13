using System;
using DimensionBrawl.AI;
using UnityEngine;

namespace DimensionBrawl.Presentation
{
    public enum CombatVfxCueId
    {
        PlayerBasicAttackStart,
        PlayerBasicAttackHit,
        PlayerDodgeStart,
        EnemyWindup,
        EnemyAttackActive,
        EnemyHit,
        EnemyDeath,
        EliteSignal,
        EnemyClosePunishWindup,
        EnemyClosePunishActive,
        EnemyLungeStrikeWindup,
        EnemyLungeStrikeActive,
        EnemyHeavyWindupWindup,
        EnemyHeavyWindupActive,
        EnemyLinePressureWindup,
        EnemyLinePressureActive,
        EnemyFanPressureWindup,
        EnemyFanPressureActive,
        EnemyRetreatShotWindup,
        EnemyRetreatShotActive,
        EnemyRetreatBlinkWindup,
        EnemyRetreatBlinkActive,
        EnemyGuardBreakWindup,
        EnemyGuardBreakActive,
        EliteShieldSignal,
        EliteArmorBreakSignal,
        EliteAuraSignal,
        EliteSummonSignal,
        ElitePhaseSwapSignal
    }

    [CreateAssetMenu(menuName = "DimensionBrawl/Presentation/Combat VFX Cue Profile", fileName = "DB_CombatVfxCueProfile")]
    public sealed class CombatVfxCueProfile : ScriptableObject
    {
        [SerializeField] private CombatVfxCue[] cues = Array.Empty<CombatVfxCue>();

        public bool TryGetCue(CombatVfxCueId cueId, out CombatVfxCue cue)
        {
            if (cues != null)
            {
                for (int i = 0; i < cues.Length; i++)
                {
                    if (cues[i].CueId == cueId && cues[i].Prefab != null)
                    {
                        cue = cues[i];
                        return true;
                    }
                }
            }

            cue = default;
            return false;
        }
    }

    [Serializable]
    public struct CombatVfxCue
    {
        [SerializeField] private CombatVfxCueId cueId;
        [SerializeField] private GameObject prefab;
        [SerializeField] private Vector3 localPositionOffset;
        [SerializeField] private Vector3 localEulerOffset;
        [SerializeField] private Vector3 localScale;
        [SerializeField, Min(0f)] private float lifetimeSeconds;
        [SerializeField, Min(0)] private int prewarmCount;
        [SerializeField] private bool parentToAnchor;
        [SerializeField] private bool alignForwardToDirection;

        public CombatVfxCueId CueId => cueId;
        public GameObject Prefab => prefab;
        public Vector3 LocalPositionOffset => localPositionOffset;
        public Vector3 LocalEulerOffset => localEulerOffset;
        public Vector3 LocalScale => localScale == Vector3.zero ? Vector3.one : localScale;
        public float LifetimeSeconds => lifetimeSeconds;
        public int PrewarmCount => prewarmCount;
        public bool ParentToAnchor => parentToAnchor;
        public bool AlignForwardToDirection => alignForwardToDirection;
    }

    [Serializable]
    public struct CombatPatternVfxCueOverride
    {
        [SerializeField] private CombatAiPatternProfile patternProfile;
        [SerializeField] private CombatVfxCueId windupCueId;
        [SerializeField] private CombatVfxCueId attackActiveCueId;
        [SerializeField, Min(0f)] private float windupIntensity;
        [SerializeField, Min(0f)] private float attackActiveIntensity;

        public CombatPatternVfxCueOverride(
            CombatAiPatternProfile patternProfile,
            CombatVfxCueId windupCueId,
            CombatVfxCueId attackActiveCueId,
            float windupIntensity = 1f,
            float attackActiveIntensity = 1f)
        {
            this.patternProfile = patternProfile;
            this.windupCueId = windupCueId;
            this.attackActiveCueId = attackActiveCueId;
            this.windupIntensity = windupIntensity;
            this.attackActiveIntensity = attackActiveIntensity;
        }

        public CombatAiPatternProfile PatternProfile => patternProfile;
        public CombatVfxCueId WindupCueId => windupCueId;
        public CombatVfxCueId AttackActiveCueId => attackActiveCueId;
        public float WindupIntensity => windupIntensity > 0f ? windupIntensity : 1f;
        public float AttackActiveIntensity => attackActiveIntensity > 0f ? attackActiveIntensity : 1f;
    }

    [Serializable]
    public struct CombatEliteVfxCueOverride
    {
        [SerializeField] private CombatAiElitePatternProfile eliteProfile;
        [SerializeField] private CombatVfxCueId signalCueId;
        [SerializeField, Min(0f)] private float intensity;

        public CombatEliteVfxCueOverride(
            CombatAiElitePatternProfile eliteProfile,
            CombatVfxCueId signalCueId,
            float intensity = 1f)
        {
            this.eliteProfile = eliteProfile;
            this.signalCueId = signalCueId;
            this.intensity = intensity;
        }

        public CombatAiElitePatternProfile EliteProfile => eliteProfile;
        public CombatVfxCueId SignalCueId => signalCueId;
        public float Intensity => intensity > 0f ? intensity : 1f;
    }
}
