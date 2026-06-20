using System;
using UnityEngine;

namespace DimensionBrawl.UI
{
    [CreateAssetMenu(menuName = "DimensionBrawl/UI/Text Catalog")]
    public sealed class UITextCatalog : ScriptableObject
    {
        [Serializable]
        public struct TextEntry
        {
            [SerializeField] private string key;
            [SerializeField] private string domain;
            [SerializeField, TextArea] private string value;
            [SerializeField, TextArea] private string note;

            public string Key => key;
            public string Domain => domain;
            public string Value => value;
            public string Note => note;
        }

        [SerializeField] private TextEntry[] entries = Array.Empty<TextEntry>();

        public bool TryGetText(string key, out string value)
        {
            for (int i = 0; i < entries.Length; i++)
            {
                if (string.Equals(entries[i].Key, key, StringComparison.Ordinal))
                {
                    value = entries[i].Value;
                    return true;
                }
            }

            value = string.Empty;
            return false;
        }
    }
}
