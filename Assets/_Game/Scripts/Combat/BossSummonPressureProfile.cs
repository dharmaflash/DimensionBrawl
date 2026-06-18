using System;
using UnityEngine;

namespace DimensionBrawl.Combat
{
    [CreateAssetMenu(
        fileName = "DB_BossSummonPressure",
        menuName = "DimensionBrawl/Combat/Boss Summon Pressure")]
    public sealed class BossSummonPressureProfile : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string pressureId = "BossSummonPressure";

        [Header("Tier Settings")]
        [SerializeField] private BossSummonPressureAction.BossSummonTierSettings[] tierSettings =
            Array.Empty<BossSummonPressureAction.BossSummonTierSettings>();

        public string PressureId => pressureId;
        public int TierCount => tierSettings != null ? tierSettings.Length : 0;

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
            return tierSettings != null
                ? (BossSummonPressureAction.BossSummonTierSettings[])tierSettings.Clone()
                : Array.Empty<BossSummonPressureAction.BossSummonTierSettings>();
        }
    }
}
