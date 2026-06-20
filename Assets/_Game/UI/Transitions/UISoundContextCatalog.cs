using System;
using UnityEngine;

namespace DimensionBrawl.UI
{
    [CreateAssetMenu(menuName = "DimensionBrawl/UI/Sound Context Catalog")]
    public sealed class UISoundContextCatalog : ScriptableObject
    {
        [Serializable]
        public struct SoundContext
        {
            [SerializeField] private string id;
            [SerializeField] private string description;
            [SerializeField, Range(0f, 1f)] private float volume;
            [SerializeField, Min(0f)] private float fadeCrossSeconds;
            [SerializeField] private bool loop;
            [SerializeField, Min(0f)] private float loopIntervalSeconds;

            public string Id => id;
            public string Description => description;
            public float Volume => volume;
            public float FadeCrossSeconds => fadeCrossSeconds;
            public bool Loop => loop;
            public float LoopIntervalSeconds => loopIntervalSeconds;
        }

        [SerializeField] private SoundContext[] contexts = Array.Empty<SoundContext>();

        public bool TryGetContext(string id, out SoundContext context)
        {
            for (int i = 0; i < contexts.Length; i++)
            {
                if (string.Equals(contexts[i].Id, id, StringComparison.Ordinal))
                {
                    context = contexts[i];
                    return true;
                }
            }

            context = default;
            return false;
        }
    }
}
