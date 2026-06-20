using UnityEngine;
using UnityEngine.UI;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    public sealed class UIInputPromptPresenter : MonoBehaviour
    {
        [SerializeField] private UIInputPromptCatalog promptCatalog;
        [SerializeField] private Text labelText;
        [SerializeField] private string actionName = "Confirm";
        [SerializeField] private UIInputPromptDevice device = UIInputPromptDevice.KeyboardMouse;

        private void OnEnable()
        {
            Apply();
        }

        public void SetPrompt(string newActionName, UIInputPromptDevice newDevice)
        {
            actionName = newActionName;
            device = newDevice;
            Apply();
        }

        private void Apply()
        {
            if (labelText == null)
            {
                return;
            }

            if (promptCatalog != null && promptCatalog.TryGetPrompt(actionName, device, out UIInputPromptCatalog.PromptEntry prompt))
            {
                labelText.text = prompt.PromptLabel;
                return;
            }

            labelText.text = actionName;
        }
    }
}
