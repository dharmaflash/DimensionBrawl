using System;
using UnityEngine;

namespace DimensionBrawl.UI
{
    public enum UIMotionTarget
    {
        Screen = 0,
        Panel = 10,
        Button = 20,
        Toast = 30,
        Hud = 40,
        Loading = 50
    }

    public enum UIMotionEasing
    {
        Linear = 0,
        EaseOut = 10,
        EaseInOut = 20,
        DisplaySpring = 30
    }

    [CreateAssetMenu(menuName = "DimensionBrawl/UI/Motion Catalog")]
    public sealed class UIMotionCatalog : ScriptableObject
    {
        [Serializable]
        public struct MotionEntry
        {
            [SerializeField] private string id;
            [SerializeField] private UIMotionTarget target;
            [SerializeField, Min(0f)] private float durationSeconds;
            [SerializeField, Min(0f)] private float delaySeconds;
            [SerializeField] private float fadeFrom;
            [SerializeField] private float fadeTo;
            [SerializeField] private float scaleFrom;
            [SerializeField] private float scaleTo;
            [SerializeField] private UIMotionEasing easing;
            [SerializeField] private bool blocksInputDuringMotion;

            public string Id => id;
            public UIMotionTarget Target => target;
            public float DurationSeconds => durationSeconds;
            public float DelaySeconds => delaySeconds;
            public float FadeFrom => fadeFrom;
            public float FadeTo => fadeTo;
            public float ScaleFrom => scaleFrom;
            public float ScaleTo => scaleTo;
            public UIMotionEasing Easing => easing;
            public bool BlocksInputDuringMotion => blocksInputDuringMotion;
        }

        [SerializeField] private MotionEntry[] motions = Array.Empty<MotionEntry>();

        public bool TryGetMotion(string id, out MotionEntry motion)
        {
            for (int i = 0; i < motions.Length; i++)
            {
                if (string.Equals(motions[i].Id, id, StringComparison.Ordinal))
                {
                    motion = motions[i];
                    return true;
                }
            }

            motion = default;
            return false;
        }
    }
}
