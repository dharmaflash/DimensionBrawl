using System;
using UnityEngine;

namespace DimensionBrawl.UI
{
    public enum UIPanelKind
    {
        Modal = 0,
        Drawer = 10,
        Overlay = 20,
        Inline = 30
    }

    [CreateAssetMenu(menuName = "DimensionBrawl/UI/Panel Catalog")]
    public sealed class UIPanelCatalog : ScriptableObject
    {
        [Serializable]
        public struct PanelEntry
        {
            [SerializeField] private string panelId;
            [SerializeField] private UIPanelKind panelKind;
            [SerializeField] private GameObject prefab;
            [SerializeField] private string openCueId;
            [SerializeField] private string closeCueId;
            [SerializeField] private UICachePolicy cachePolicy;

            public string PanelId => panelId;
            public UIPanelKind PanelKind => panelKind;
            public GameObject Prefab => prefab;
            public string OpenCueId => openCueId;
            public string CloseCueId => closeCueId;
            public UICachePolicy CachePolicy => cachePolicy;
        }

        [SerializeField] private PanelEntry[] panels = Array.Empty<PanelEntry>();

        public bool TryGetPanel(string panelId, out PanelEntry panel)
        {
            for (int i = 0; i < panels.Length; i++)
            {
                if (string.Equals(panels[i].PanelId, panelId, StringComparison.Ordinal))
                {
                    panel = panels[i];
                    return true;
                }
            }

            panel = default;
            return false;
        }
    }
}
