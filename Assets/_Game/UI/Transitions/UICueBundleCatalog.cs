using System;
using UnityEngine;

namespace DimensionBrawl.UI
{
    public enum UICueCleanupPolicy
    {
        OnCueEnd = 0,
        OnScreenExit = 10,
        Manual = 20
    }

    [CreateAssetMenu(menuName = "DimensionBrawl/UI/Cue Bundle Catalog")]
    public sealed class UICueBundleCatalog : ScriptableObject
    {
        [Serializable]
        public struct CueBundle
        {
            [SerializeField] private string id;
            [SerializeField] private string eventId;
            [SerializeField] private string uiMotionId;
            [SerializeField] private string sfxId;
            [SerializeField] private string vfxId;
            [SerializeField] private string cameraCueId;
            [SerializeField, Min(0f)] private float durationSeconds;
            [SerializeField] private UICueCleanupPolicy cleanupPolicy;

            public string Id => id;
            public string EventId => eventId;
            public string UiMotionId => uiMotionId;
            public string SfxId => sfxId;
            public string VfxId => vfxId;
            public string CameraCueId => cameraCueId;
            public float DurationSeconds => durationSeconds;
            public UICueCleanupPolicy CleanupPolicy => cleanupPolicy;
        }

        [SerializeField] private CueBundle[] cueBundles = Array.Empty<CueBundle>();

        public bool TryGetCue(string id, out CueBundle cueBundle)
        {
            for (int i = 0; i < cueBundles.Length; i++)
            {
                if (string.Equals(cueBundles[i].Id, id, StringComparison.Ordinal))
                {
                    cueBundle = cueBundles[i];
                    return true;
                }
            }

            cueBundle = default;
            return false;
        }
    }
}
