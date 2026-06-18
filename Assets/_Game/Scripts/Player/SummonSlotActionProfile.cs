using System;
using UnityEngine;

namespace DimensionBrawl.Player
{
    [CreateAssetMenu(
        fileName = "DB_SummonSlotAction",
        menuName = "DimensionBrawl/Player/Summon Slot Action")]
    public sealed class SummonSlotActionProfile : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string actionId = "SummonSlot1";

        [Header("Tier Settings")]
        [SerializeField] private PlayerSummonSlot1Action.SummonTierSettings[] tierSettings =
            Array.Empty<PlayerSummonSlot1Action.SummonTierSettings>();

        public string ActionId => actionId;
        public int TierCount => tierSettings != null ? tierSettings.Length : 0;

        private void OnValidate()
        {
            if (tierSettings == null)
            {
                return;
            }

            for (int i = 0; i < tierSettings.Length; i++)
            {
                PlayerSummonSlot1Action.SummonTierSettings settings = tierSettings[i];
                settings.Normalize();
                tierSettings[i] = settings;
            }
        }

        public PlayerSummonSlot1Action.SummonTierSettings[] CopyTierSettings()
        {
            return tierSettings != null
                ? (PlayerSummonSlot1Action.SummonTierSettings[])tierSettings.Clone()
                : Array.Empty<PlayerSummonSlot1Action.SummonTierSettings>();
        }
    }
}
