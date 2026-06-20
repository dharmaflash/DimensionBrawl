using System;
using UnityEngine;

namespace DimensionBrawl.UI
{
    [CreateAssetMenu(menuName = "DimensionBrawl/UI/Component Catalog")]
    public sealed class UIComponentCatalog : ScriptableObject
    {
        [Serializable]
        public struct ComponentEntry
        {
            [SerializeField] private string key;
            [SerializeField] private string category;
            [SerializeField] private GameObject prefab;
            [SerializeField] private UICachePolicy cachePolicy;

            public string Key => key;
            public string Category => category;
            public GameObject Prefab => prefab;
            public UICachePolicy CachePolicy => cachePolicy;
        }

        [SerializeField] private ComponentEntry[] components = Array.Empty<ComponentEntry>();

        public bool TryGetComponent(string key, out ComponentEntry component)
        {
            for (int i = 0; i < components.Length; i++)
            {
                if (string.Equals(components[i].Key, key, StringComparison.Ordinal))
                {
                    component = components[i];
                    return true;
                }
            }

            component = default;
            return false;
        }
    }
}
