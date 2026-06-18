using System;
using UnityEngine;

namespace DimensionBrawl.Player
{
    [CreateAssetMenu(
        fileName = "DB_SummonSlotAction",
        menuName = "DimensionBrawl/Player/Summon Slot Action")]
    public sealed class SummonSlotActionProfile : ScriptableObject
    {
        [Serializable]
        public struct SummonTierReadout
        {
            public string TierLabel;
            [TextArea] public string StageRole;
            [TextArea] public string PlayerUse;
            [TextArea] public string SummonRead;

            public bool HasReadout =>
                !string.IsNullOrWhiteSpace(TierLabel)
                && !string.IsNullOrWhiteSpace(StageRole)
                && !string.IsNullOrWhiteSpace(PlayerUse)
                && !string.IsNullOrWhiteSpace(SummonRead);
        }

        [Header("Identity")]
        [SerializeField] private string actionId = "SummonSlot1";

        [Header("Tier Settings")]
        [SerializeField] private PlayerSummonSlot1Action.SummonTierSettings[] tierSettings =
            Array.Empty<PlayerSummonSlot1Action.SummonTierSettings>();

        [Header("Tier Readouts")]
        [SerializeField] private SummonTierReadout[] tierReadouts = Array.Empty<SummonTierReadout>();

        public string ActionId => actionId;
        public int TierCount => tierSettings != null ? tierSettings.Length : 0;
        public int TierReadoutCount => tierReadouts != null ? tierReadouts.Length : 0;

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

        public bool TryGetTierReadout(int tier, out SummonTierReadout readout)
        {
            int index = Mathf.Clamp(tier - 1, 0, Mathf.Max(0, TierReadoutCount - 1));
            if (tierReadouts == null || tierReadouts.Length == 0 || index < 0 || index >= tierReadouts.Length)
            {
                readout = default;
                return false;
            }

            readout = tierReadouts[index];
            return readout.HasReadout;
        }
    }
}
