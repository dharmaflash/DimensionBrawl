using System;
using UnityEngine;

namespace DimensionBrawl.UI
{
    public enum UILayerId
    {
        Background = 0,
        Screen = 10,
        Panel = 20,
        Overlay = 30,
        Transition = 40,
        Debug = 50
    }

    [CreateAssetMenu(menuName = "DimensionBrawl/UI/Layer Catalog")]
    public sealed class UILayerCatalog : ScriptableObject
    {
        [Serializable]
        public struct LayerEntry
        {
            [SerializeField] private UILayerId layerId;
            [SerializeField] private string sortingLayerName;
            [SerializeField] private int sortingOrder;
            [SerializeField] private bool blocksRaycasts;
            [SerializeField] private bool modal;

            public UILayerId LayerId => layerId;
            public string SortingLayerName => sortingLayerName;
            public int SortingOrder => sortingOrder;
            public bool BlocksRaycasts => blocksRaycasts;
            public bool Modal => modal;
        }

        [SerializeField] private LayerEntry[] layers = Array.Empty<LayerEntry>();

        public bool TryGetLayer(UILayerId layerId, out LayerEntry layer)
        {
            for (int i = 0; i < layers.Length; i++)
            {
                if (layers[i].LayerId == layerId)
                {
                    layer = layers[i];
                    return true;
                }
            }

            layer = default;
            return false;
        }
    }
}
