using System;
using UnityEngine;

namespace DimensionBrawl.UI
{
    public enum UIStateMessageKind
    {
        Empty = 0,
        Locked = 10,
        Loading = 20,
        Error = 30,
        ComingSoon = 40,
        Maintenance = 50
    }

    [CreateAssetMenu(menuName = "DimensionBrawl/UI/State Message Catalog")]
    public sealed class UIStateMessageCatalog : ScriptableObject
    {
        [Serializable]
        public struct StateMessageEntry
        {
            [SerializeField] private string id;
            [SerializeField] private UIStateMessageKind kind;
            [SerializeField] private string titleTextKey;
            [SerializeField] private string bodyTextKey;
            [SerializeField] private string iconKey;
            [SerializeField] private string actionTextKey;
            [SerializeField] private bool actionVisible;
            [SerializeField] private bool actionInteractable;
            [SerializeField] private string cueId;

            public string Id => id;
            public UIStateMessageKind Kind => kind;
            public string TitleTextKey => titleTextKey;
            public string BodyTextKey => bodyTextKey;
            public string IconKey => iconKey;
            public string ActionTextKey => actionTextKey;
            public bool ActionVisible => actionVisible;
            public bool ActionInteractable => actionInteractable;
            public string CueId => cueId;
        }

        [SerializeField] private StateMessageEntry[] states = Array.Empty<StateMessageEntry>();

        public bool TryGetState(string id, out StateMessageEntry entry)
        {
            for (int i = 0; i < states.Length; i++)
            {
                if (string.Equals(states[i].Id, id, StringComparison.Ordinal))
                {
                    entry = states[i];
                    return true;
                }
            }

            entry = default;
            return false;
        }
    }
}
