using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    public sealed class UITabStripPresenter : MonoBehaviour
    {
        [SerializeField] private Button[] tabButtons = new Button[0];
        [SerializeField] private Text selectedLabelText;
        [SerializeField] private string[] labels = new string[0];
        [SerializeField] private int selectedIndex;
        [SerializeField] private UnityEvent<int> selectionChanged = new UnityEvent<int>();

        private UnityAction[] tabListeners = new UnityAction[0];

        private void OnEnable()
        {
            tabListeners = new UnityAction[tabButtons.Length];
            for (int i = 0; i < tabButtons.Length; i++)
            {
                int capturedIndex = i;
                if (tabButtons[i] != null)
                {
                    tabListeners[i] = () => Select(capturedIndex);
                    tabButtons[i].onClick.AddListener(tabListeners[i]);
                }
            }

            ApplySelection();
        }

        private void OnDisable()
        {
            for (int i = 0; i < tabButtons.Length; i++)
            {
                if (tabButtons[i] != null && tabListeners.Length > i && tabListeners[i] != null)
                {
                    tabButtons[i].onClick.RemoveListener(tabListeners[i]);
                }
            }
        }

        public void Select(int index)
        {
            selectedIndex = Mathf.Clamp(index, 0, Mathf.Max(0, labels.Length - 1));
            ApplySelection();
            selectionChanged.Invoke(selectedIndex);
        }

        private void ApplySelection()
        {
            if (selectedLabelText != null)
            {
                selectedLabelText.text = labels.Length > selectedIndex ? labels[selectedIndex] : string.Empty;
            }
        }
    }
}
