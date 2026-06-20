using System;
using UnityEngine;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    public sealed class UIPanelRouter : MonoBehaviour
    {
        [Serializable]
        public struct PanelBinding
        {
            [SerializeField] private string panelId;
            [SerializeField] private UIPanelRoot panelRoot;
            [SerializeField] private bool closeOthersOnOpen;

            public string PanelId => panelId;
            public UIPanelRoot PanelRoot => panelRoot;
            public bool CloseOthersOnOpen => closeOthersOnOpen;
        }

        [SerializeField] private PanelBinding[] panels = Array.Empty<PanelBinding>();
        [SerializeField] private bool hideAllOnAwake = true;

        private void Awake()
        {
            if (hideAllOnAwake)
            {
                HideAll();
            }
        }

        public void ShowPanel(string panelId)
        {
            if (!TryGetPanel(panelId, out PanelBinding binding))
            {
                return;
            }

            if (binding.CloseOthersOnOpen)
            {
                HideAllExcept(panelId);
            }

            if (binding.PanelRoot != null)
            {
                binding.PanelRoot.Show();
            }
        }

        public void HidePanel(string panelId)
        {
            if (TryGetPanel(panelId, out PanelBinding binding) && binding.PanelRoot != null)
            {
                binding.PanelRoot.Hide();
            }
        }

        public void TogglePanel(string panelId)
        {
            if (!TryGetPanel(panelId, out PanelBinding binding) || binding.PanelRoot == null)
            {
                return;
            }

            if (binding.PanelRoot.IsVisible)
            {
                binding.PanelRoot.Hide();
                return;
            }

            ShowPanel(panelId);
        }

        public void HideAll()
        {
            for (int i = 0; i < panels.Length; i++)
            {
                if (panels[i].PanelRoot != null)
                {
                    panels[i].PanelRoot.Hide();
                }
            }
        }

        private void HideAllExcept(string panelId)
        {
            for (int i = 0; i < panels.Length; i++)
            {
                if (!string.Equals(panels[i].PanelId, panelId, StringComparison.Ordinal)
                    && panels[i].PanelRoot != null)
                {
                    panels[i].PanelRoot.Hide();
                }
            }
        }

        private bool TryGetPanel(string panelId, out PanelBinding binding)
        {
            for (int i = 0; i < panels.Length; i++)
            {
                if (string.Equals(panels[i].PanelId, panelId, StringComparison.Ordinal))
                {
                    binding = panels[i];
                    return true;
                }
            }

            binding = default;
            return false;
        }
    }
}
