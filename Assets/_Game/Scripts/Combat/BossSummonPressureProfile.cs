using System;
using UnityEngine;

namespace DimensionBrawl.Combat
{
    [CreateAssetMenu(
        fileName = "DB_BossSummonPressure",
        menuName = "DimensionBrawl/Combat/Boss Summon Pressure")]
    public sealed class BossSummonPressureProfile : ScriptableObject
    {
        [Serializable]
        public struct BossSummonTierReadout
        {
            public string TierLabel;
            [TextArea] public string StageRole;
            [TextArea] public string PlayerRead;
            [TextArea] public string SummonRead;

            public bool HasReadout =>
                !string.IsNullOrWhiteSpace(TierLabel)
                && !string.IsNullOrWhiteSpace(StageRole)
                && !string.IsNullOrWhiteSpace(PlayerRead)
                && !string.IsNullOrWhiteSpace(SummonRead);
        }

        [Header("Identity")]
        [SerializeField] private string pressureId = "BossSummonPressure";

        [Header("Tier Settings")]
        [SerializeField] private BossSummonPressureAction.BossSummonTierSettings[] tierSettings =
            Array.Empty<BossSummonPressureAction.BossSummonTierSettings>();
        [SerializeField] private BossSummonTierReadout[] tierReadouts = Array.Empty<BossSummonTierReadout>();

        public string PressureId => pressureId;
        public int TierCount => tierSettings != null ? tierSettings.Length : 0;
        public int TierReadoutCount => tierReadouts != null ? tierReadouts.Length : 0;
        public int ResponseSlotCount => TierCount;
        public int ResponseSlotReadoutCount => TierReadoutCount;

        private void OnValidate()
        {
            if (tierSettings == null)
            {
                return;
            }

            for (int i = 0; i < tierSettings.Length; i++)
            {
                BossSummonPressureAction.BossSummonTierSettings settings = tierSettings[i];
                settings.Normalize();
                tierSettings[i] = settings;
            }
        }

        public BossSummonPressureAction.BossSummonTierSettings[] CopyTierSettings()
        {
            if (tierSettings == null)
            {
                return Array.Empty<BossSummonPressureAction.BossSummonTierSettings>();
            }

            BossSummonPressureAction.BossSummonTierSettings[] copy =
                (BossSummonPressureAction.BossSummonTierSettings[])tierSettings.Clone();
            for (int i = 0; i < copy.Length; i++)
            {
                copy[i].Normalize();
            }

            return copy;
        }

        public BossSummonPressureAction.BossSummonTierSettings[] CopyResponseSlotSettings()
        {
            return CopyTierSettings();
        }

        public bool TryGetTierReadout(int tier, out BossSummonTierReadout readout)
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

        public bool TryGetResponseSlotReadout(int responseSlot, out BossSummonTierReadout readout)
        {
            return TryGetTierReadout(responseSlot, out readout);
        }
    }
}
