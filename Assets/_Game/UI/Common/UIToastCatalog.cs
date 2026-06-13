using System;
using UnityEngine;

namespace DimensionBrawl.UI
{
    [CreateAssetMenu(menuName = "DimensionBrawl/UI/Toast Catalog")]
    public sealed class UIToastCatalog : ScriptableObject
    {
        [Serializable]
        public struct ToastEntry
        {
            [SerializeField] private string id;
            [SerializeField] private string messageTextKey;
            [SerializeField] private string iconKey;
            [SerializeField] private Color accentColor;
            [SerializeField, Min(0f)] private float durationSeconds;
            [SerializeField] private int priority;
            [SerializeField] private string cueId;

            public string Id => id;
            public string MessageTextKey => messageTextKey;
            public string IconKey => iconKey;
            public Color AccentColor => accentColor;
            public float DurationSeconds => durationSeconds;
            public int Priority => priority;
            public string CueId => cueId;
        }

        [SerializeField] private ToastEntry[] toasts = Array.Empty<ToastEntry>();

        public bool TryGetToast(string id, out ToastEntry toast)
        {
            for (int i = 0; i < toasts.Length; i++)
            {
                if (string.Equals(toasts[i].Id, id, StringComparison.Ordinal))
                {
                    toast = toasts[i];
                    return true;
                }
            }

            toast = default;
            return false;
        }
    }
}
