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
        ElitePhaseSwapSignal,
        SummonFollowupWindow,
        SummonFollowupHit,
        SummonFollowupMissed,
        SummonBlockOpportunity,
        PlayerRangedMuzzleFlash,
        PlayerRangedProjectileImpact,
        PocketCleared,
        PocketFailed,
        PlayerDamaged,
        PlayerCritical,
        PlayerPerfectDodgeTimeField,
        PlayerPerfectDodgePulsewave,
        PlayerPerfectDodgeHoloCube,
        PlayerPerfectDodgeWindow,
        PlayerSummonPreSpawnPortal,
        PlayerSummonLandingCrater,
        PlayerSummonDragonBreathAudio,
        PlayerPerfectDodgeShieldBlockImpact
    }

    public enum CombatVfxCuePlaybackMode
    {
        AllAuthoredCues,
        ReviewedCombatFeedbackOnly,
        PlayerRangedOnly
    }

    [CreateAssetMenu(menuName = "DimensionBrawl/Presentation/Combat VFX Cue Profile", fileName = "DB_CombatVfxCueProfile")]
    public sealed class CombatVfxCueProfile : ScriptableObject
    {
        [SerializeField] private CombatVfxCuePlaybackMode playbackMode = CombatVfxCuePlaybackMode.AllAuthoredCues;
        [SerializeField] private CombatVfxCue[] cues = Array.Empty<CombatVfxCue>();

        public CombatVfxCuePlaybackMode PlaybackMode => playbackMode;

        public bool AllowsPlayback(CombatVfxCueId cueId)
        {
            switch (playbackMode)
            {
                case CombatVfxCuePlaybackMode.ReviewedCombatFeedbackOnly:
                    return cueId == CombatVfxCueId.PlayerBasicAttackHit
                        || cueId == CombatVfxCueId.PlayerDodgeStart
                        || cueId == CombatVfxCueId.PlayerRangedMuzzleFlash
                        || cueId == CombatVfxCueId.PlayerRangedProjectileImpact
                        || cueId == CombatVfxCueId.PlayerDamaged
                        || cueId == CombatVfxCueId.PlayerCritical
                        || cueId == CombatVfxCueId.EnemyHit
                        || cueId == CombatVfxCueId.EnemyDeath
                        || cueId == CombatVfxCueId.EliteSummonSignal
                        || cueId == CombatVfxCueId.SummonBlockOpportunity
                        || cueId == CombatVfxCueId.SummonFollowupWindow
                        || cueId == CombatVfxCueId.SummonFollowupHit
                        || cueId == CombatVfxCueId.PlayerPerfectDodgeTimeField
                        || cueId == CombatVfxCueId.PlayerPerfectDodgePulsewave
                        || cueId == CombatVfxCueId.PlayerPerfectDodgeHoloCube
                        || cueId == CombatVfxCueId.PlayerPerfectDodgeWindow
                        || cueId == CombatVfxCueId.PlayerPerfectDodgeShieldBlockImpact
                        || cueId == CombatVfxCueId.PlayerSummonPreSpawnPortal
                        || cueId == CombatVfxCueId.PlayerSummonLandingCrater
                        || cueId == CombatVfxCueId.PlayerSummonDragonBreathAudio;
                case CombatVfxCuePlaybackMode.PlayerRangedOnly:
                    return cueId == CombatVfxCueId.PlayerRangedMuzzleFlash;
                default:
                    return true;
            }
        }

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
        [SerializeField] private AudioClip[] audioClips;
        [SerializeField, Min(0f)] private float audioBaseVolume;
        [SerializeField, Min(0f)] private float audioMinimumPitch;
        [SerializeField, Min(0f)] private float audioMaximumPitch;
        [SerializeField, Min(0f)] private float audioMinimumVolumeMultiplier;
        [SerializeField, Min(0f)] private float audioMaximumVolumeMultiplier;
        [SerializeField, Range(0f, 1f)] private float audioSpatialBlend;
        [SerializeField, Min(0f)] private float audioMinDistance;
        [SerializeField, Min(0f)] private float audioMaxDistance;
        [SerializeField, Range(0, 256)] private int audioPriority;

        public CombatVfxCueId CueId => cueId;
        public GameObject Prefab => prefab;
        public Vector3 LocalPositionOffset => localPositionOffset;
        public Vector3 LocalEulerOffset => localEulerOffset;
        public Vector3 LocalScale => localScale == Vector3.zero ? Vector3.one : localScale;
        public float LifetimeSeconds => lifetimeSeconds;
        public int PrewarmCount => prewarmCount;
        public bool ParentToAnchor => parentToAnchor;
        public bool AlignForwardToDirection => alignForwardToDirection;
        public int AudioClipCount => audioClips != null ? audioClips.Length : 0;
        public float AudioBaseVolume => Mathf.Max(0f, audioBaseVolume);
        public float AudioMinimumPitch => audioMinimumPitch > 0f ? Mathf.Min(audioMinimumPitch, AudioMaximumPitch) : 1f;
        public float AudioMaximumPitch => audioMaximumPitch > 0f ? Mathf.Max(audioMinimumPitch, audioMaximumPitch) : 1f;
        public float AudioMinimumVolumeMultiplier => audioMinimumVolumeMultiplier > 0f ? Mathf.Min(audioMinimumVolumeMultiplier, AudioMaximumVolumeMultiplier) : 1f;
        public float AudioMaximumVolumeMultiplier => audioMaximumVolumeMultiplier > 0f ? Mathf.Max(audioMinimumVolumeMultiplier, audioMaximumVolumeMultiplier) : 1f;
        public float AudioSpatialBlend => Mathf.Clamp01(audioSpatialBlend);
        public float AudioMinDistance => Mathf.Max(0f, audioMinDistance);
        public float AudioMaxDistance => Mathf.Max(AudioMinDistance + 0.01f, audioMaxDistance);
        public int AudioPriority => Mathf.Clamp(audioPriority, 0, 256);

        public AudioClip GetAudioClip(int index)
        {
            return audioClips[index];
        }
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
