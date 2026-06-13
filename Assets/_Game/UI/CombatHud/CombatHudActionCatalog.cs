using System;
using UnityEngine;

namespace DimensionBrawl.UI
{
    public enum CombatHudCooldownDisplayMode
    {
        None = 0,
        Radial = 10,
        Bar = 20,
        Count = 30
    }

    [CreateAssetMenu(menuName = "DimensionBrawl/UI/Combat HUD Action Catalog")]
    public sealed class CombatHudActionCatalog : ScriptableObject
    {
        [Serializable]
        public struct ActionEntry
        {
            [SerializeField] private CombatHudActionId actionId;
            [SerializeField] private string displayName;
            [SerializeField] private string canonicalName;
            [SerializeField] private string placeholderState;
            [SerializeField] private CombatHudCooldownDisplayMode cooldownDisplayMode;
            [SerializeField] private bool enabledInV1;

            public CombatHudActionId ActionId => actionId;
            public string DisplayName => displayName;
            public string CanonicalName => canonicalName;
            public string PlaceholderState => placeholderState;
            public CombatHudCooldownDisplayMode CooldownDisplayMode => cooldownDisplayMode;
            public bool EnabledInV1 => enabledInV1;
        }

        [SerializeField] private ActionEntry[] actions = Array.Empty<ActionEntry>();

        public bool TryGetAction(CombatHudActionId actionId, out ActionEntry action)
        {
            for (int i = 0; i < actions.Length; i++)
            {
                if (actions[i].ActionId == actionId)
                {
                    action = actions[i];
                    return true;
                }
            }

            action = default;
            return false;
        }
    }
}
