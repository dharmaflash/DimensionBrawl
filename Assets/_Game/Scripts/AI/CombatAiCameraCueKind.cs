using System;
using UnityEngine;

namespace DimensionBrawl.AI
{
    public enum CombatAiCameraCueKind
    {
        ClosePunish,
        LungeStrike,
        HeavyWindup,
        LinePressure,
        FanPressure,
        RetreatShot,
        RetreatBlink,
        GuardBreak,
        ShieldCycle,
        ArmorBreak,
        AuraBuffer,
        SummonPackage,
        PhaseSwap
    }

    [Serializable]
    public struct CombatAiCameraCue
    {
        public bool enabled;
        public Vector3 localOffset;
        public float planarDirectionOffset;
        public float fieldOfViewDelta;
        public float cameraDistanceDelta;
        public float focusHeightDelta;
        public float durationSeconds;
        public float finisherScale;
    }
}
