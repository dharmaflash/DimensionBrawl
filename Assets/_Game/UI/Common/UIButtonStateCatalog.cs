using System;
using UnityEngine;

namespace DimensionBrawl.UI
{
    public enum UIButtonVisualState
    {
        Normal = 0,
        Highlighted = 10,
        Disabled = 20,
        Locked = 30,
        Ready = 40,
        Active = 50
    }

    [CreateAssetMenu(menuName = "DimensionBrawl/UI/Button State Catalog")]
    public sealed class UIButtonStateCatalog : ScriptableObject
    {
        [Serializable]
        public struct ButtonStateEntry
        {
            [SerializeField] private UIButtonVisualState stateId;
            [SerializeField] private string labelKey;
            [SerializeField] private Color backgroundColor;
            [SerializeField] private Color textColor;
            [SerializeField] private bool interactable;
            [SerializeField] private string tooltipTextKey;
            [SerializeField] private string cueId;

            public UIButtonVisualState StateId => stateId;
            public string LabelKey => labelKey;
            public Color BackgroundColor => backgroundColor;
            public Color TextColor => textColor;
            public bool Interactable => interactable;
            public string TooltipTextKey => tooltipTextKey;
            public string CueId => cueId;
        }

        [SerializeField] private ButtonStateEntry[] states = Array.Empty<ButtonStateEntry>();

        public bool TryGetState(UIButtonVisualState stateId, out ButtonStateEntry state)
        {
            for (int i = 0; i < states.Length; i++)
            {
                if (states[i].StateId == stateId)
                {
                    state = states[i];
                    return true;
                }
            }

            state = default;
            return false;
        }
    }
}
