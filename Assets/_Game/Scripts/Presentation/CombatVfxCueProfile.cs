using System;
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
        EliteSignal
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
}
