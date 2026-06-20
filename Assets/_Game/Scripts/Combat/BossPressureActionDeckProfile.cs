using System;
using UnityEngine;

namespace DimensionBrawl.Combat
{
    [CreateAssetMenu(
        fileName = "DB_BossPressureActionDeck",
        menuName = "DimensionBrawl/Combat/Boss Pressure Action Deck")]
    public sealed class BossPressureActionDeckProfile : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string deckId = "PocketReviewBoss";

        [Header("Recovery")]
        [SerializeField, Min(0f)] private float globalRecoverySeconds = 0.35f;

        [Header("Slots")]
        [SerializeField] private BossPressureActionDirector.BossPressureActionSlot[] actionSlots =
            Array.Empty<BossPressureActionDirector.BossPressureActionSlot>();

        public string DeckId => deckId;
        public float GlobalRecoverySeconds => globalRecoverySeconds;
        public int ActionSlotCount => actionSlots != null ? actionSlots.Length : 0;

        private void OnValidate()
        {
            if (actionSlots == null)
            {
                return;
            }

            for (int i = 0; i < actionSlots.Length; i++)
            {
                BossPressureActionDirector.BossPressureActionSlot slot = actionSlots[i];
                slot.MinimumTier = Mathf.Clamp(slot.MinimumTier, 1, 3);
                slot.QueuedWaves = Mathf.Max(1, slot.QueuedWaves);
                slot.MinimumIntervalSeconds = Mathf.Max(0f, slot.MinimumIntervalSeconds);
                slot.MinimumPlayerForwardRisk01 = Mathf.Clamp01(slot.MinimumPlayerForwardRisk01);
                slot.MaximumPlayerForwardRisk01 = Mathf.Clamp01(slot.MaximumPlayerForwardRisk01);
                slot.MinimumPlayerSummonTier = Mathf.Clamp(slot.MinimumPlayerSummonTier, 1, 3);
                if (slot.MaximumPlayerForwardRisk01 < slot.MinimumPlayerForwardRisk01)
                {
                    slot.MaximumPlayerForwardRisk01 = slot.MinimumPlayerForwardRisk01;
                }

                actionSlots[i] = slot;
            }
        }

        public BossPressureActionDirector.BossPressureActionSlot[] CopyActionSlots()
        {
            return actionSlots != null
                ? (BossPressureActionDirector.BossPressureActionSlot[])actionSlots.Clone()
                : Array.Empty<BossPressureActionDirector.BossPressureActionSlot>();
        }

        public bool TryGetActionSlot(int index, out BossPressureActionDirector.BossPressureActionSlot slot)
        {
            if (actionSlots == null || index < 0 || index >= actionSlots.Length)
            {
                slot = default;
                return false;
            }

            slot = actionSlots[index];
            return true;
        }
    }
}
