using UnityEngine;
using UnityEngine.UI;

namespace DimensionBrawl.UI
{
    public enum UIPanelRequestMode
    {
        Show = 0,
        Hide = 10,
        Toggle = 20,
        HideAll = 30
    }

    [DisallowMultipleComponent]
    public sealed class UIPanelRequestButton : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private UIPanelRouter router;
        [SerializeField] private UIPanelRequestMode requestMode = UIPanelRequestMode.Show;
        [SerializeField] private string panelId;

        private void Reset()
        {
            button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            if (button != null)
            {
                button.onClick.AddListener(HandleClicked);
            }
        }

        private void OnDisable()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(HandleClicked);
            }
        }

        public void Configure(UIPanelRouter panelRouter, string targetPanelId, UIPanelRequestMode mode)
        {
            router = panelRouter;
            panelId = targetPanelId;
            requestMode = mode;
        }

        private void HandleClicked()
        {
            if (router == null)
            {
                return;
            }

            switch (requestMode)
            {
                case UIPanelRequestMode.Show:
                    router.ShowPanel(panelId);
                    break;
                case UIPanelRequestMode.Hide:
                    router.HidePanel(panelId);
                    break;
                case UIPanelRequestMode.Toggle:
                    router.TogglePanel(panelId);
                    break;
                case UIPanelRequestMode.HideAll:
                    router.HideAll();
                    break;
            }
        }
    }
}
