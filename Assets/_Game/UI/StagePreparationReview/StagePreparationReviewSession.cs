using System;
using System.Collections.Generic;

namespace DimensionBrawl.UI.StagePreparationReview
{
    public enum StagePreparationReviewPhase
    {
        StageIntel = 0,
        LoadoutOverview = 10,
        SummonDetail = 20,
        ReviewConfirm = 30
    }

    public sealed class StagePreparationReviewSession
    {
        private readonly StagePreparationReviewProfile.SlotDefinition[] slots;
        private readonly Dictionary<string, StagePreparationReviewProfile.SlotDefinition>
            slotsById;
        private readonly Dictionary<string, int> selectedTiersBySlotId;

        private string selectedSlotId = string.Empty;
        private bool reviewAccepted;

        public StagePreparationReviewSession(StagePreparationReviewProfile profile)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            if (!profile.TryValidate(out string validationError))
            {
                throw new ArgumentException(validationError, nameof(profile));
            }

            ReviewId = profile.ReviewId;
            CanonicalCatalogEntryId = profile.CanonicalCatalogEntryId;
            slots = profile.CreateSlotSnapshot();
            slotsById = new Dictionary<
                string,
                StagePreparationReviewProfile.SlotDefinition>(
                slots.Length,
                StringComparer.Ordinal);
            selectedTiersBySlotId = new Dictionary<string, int>(
                slots.Length,
                StringComparer.Ordinal);
            for (int i = 0; i < slots.Length; i++)
            {
                StagePreparationReviewProfile.SlotDefinition slot = slots[i];
                slotsById.Add(slot.SlotId, slot);
                selectedTiersBySlotId.Add(slot.SlotId, 1);
            }
        }

        public string ReviewId { get; }
        public string CanonicalCatalogEntryId { get; }
        public StagePreparationReviewPhase Phase { get; private set; } =
            StagePreparationReviewPhase.StageIntel;
        public int SlotCount => slots.Length;
        public string SelectedSlotId => selectedSlotId;
        public bool IsReviewAccepted => reviewAccepted;
        public StagePreparationReviewProfile.SlotDefinition SelectedSlot =>
            TryGetSelectedSlotInternal(
                out StagePreparationReviewProfile.SlotDefinition selected)
                ? selected.DeepCopy()
                : null;
        public int SelectedTier => TryGetSelectedTier(selectedSlotId, out int tier)
            ? tier
            : 0;

        public StagePreparationReviewProfile.SlotDefinition GetSlot(int index)
        {
            if (index < 0 || index >= slots.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return slots[index].DeepCopy();
        }

        public bool TryGetSlot(
            string slotId,
            out StagePreparationReviewProfile.SlotDefinition slot)
        {
            if (!string.IsNullOrWhiteSpace(slotId)
                && slotsById.TryGetValue(
                    slotId,
                    out StagePreparationReviewProfile.SlotDefinition found))
            {
                slot = found.DeepCopy();
                return true;
            }

            slot = null;
            return false;
        }

        public bool TryGetSelectedTier(string slotId, out int tier)
        {
            if (!string.IsNullOrWhiteSpace(slotId)
                && selectedTiersBySlotId.TryGetValue(slotId, out tier))
            {
                return true;
            }

            tier = 0;
            return false;
        }

        public bool TryOpenLoadout()
        {
            if (Phase != StagePreparationReviewPhase.StageIntel)
            {
                return false;
            }

            selectedSlotId = string.Empty;
            Phase = StagePreparationReviewPhase.LoadoutOverview;
            return true;
        }

        public bool TryInspectSlot(string slotId)
        {
            if ((Phase != StagePreparationReviewPhase.LoadoutOverview
                    && Phase != StagePreparationReviewPhase.SummonDetail)
                || string.IsNullOrWhiteSpace(slotId)
                || !slotsById.ContainsKey(slotId))
            {
                return false;
            }

            selectedSlotId = slotId;
            Phase = StagePreparationReviewPhase.SummonDetail;
            return true;
        }

        public bool TrySelectTier(int tier)
        {
            if (Phase != StagePreparationReviewPhase.SummonDetail
                || string.IsNullOrWhiteSpace(selectedSlotId)
                || tier < 1
                || tier > StagePreparationReviewProfile.RequiredTierCount)
            {
                return false;
            }

            selectedTiersBySlotId[selectedSlotId] = tier;
            return true;
        }

        public bool TryReturnToLoadout()
        {
            if (Phase != StagePreparationReviewPhase.SummonDetail
                && Phase != StagePreparationReviewPhase.ReviewConfirm)
            {
                return false;
            }

            selectedSlotId = string.Empty;
            Phase = StagePreparationReviewPhase.LoadoutOverview;
            return true;
        }

        public bool TryReturnToStageIntel()
        {
            if (Phase != StagePreparationReviewPhase.LoadoutOverview)
            {
                return false;
            }

            selectedSlotId = string.Empty;
            Phase = StagePreparationReviewPhase.StageIntel;
            return true;
        }

        public bool TryOpenReviewConfirm()
        {
            if (Phase != StagePreparationReviewPhase.LoadoutOverview)
            {
                return false;
            }

            selectedSlotId = string.Empty;
            Phase = StagePreparationReviewPhase.ReviewConfirm;
            return true;
        }

        public bool TryAcceptReview()
        {
            if (Phase != StagePreparationReviewPhase.ReviewConfirm
                || reviewAccepted)
            {
                return false;
            }

            reviewAccepted = true;
            return true;
        }

        public StagePreparationReviewSelection[] CreateSelectionSnapshot()
        {
            var snapshot = new StagePreparationReviewSelection[slots.Length];
            for (int i = 0; i < slots.Length; i++)
            {
                StagePreparationReviewProfile.SlotDefinition slot = slots[i];
                snapshot[i] = new StagePreparationReviewSelection(
                    slot.SlotId,
                    slot.ActionId,
                    selectedTiersBySlotId[slot.SlotId]);
            }

            return snapshot;
        }

        private bool TryGetSelectedSlotInternal(
            out StagePreparationReviewProfile.SlotDefinition slot)
        {
            slot = null;
            return !string.IsNullOrWhiteSpace(selectedSlotId)
                && slotsById.TryGetValue(selectedSlotId, out slot);
        }
    }

    public readonly struct StagePreparationReviewSelection
    {
        public StagePreparationReviewSelection(
            string slotId,
            string actionId,
            int selectedTier)
        {
            SlotId = slotId ?? string.Empty;
            ActionId = actionId ?? string.Empty;
            SelectedTier = selectedTier;
        }

        public string SlotId { get; }
        public string ActionId { get; }
        public int SelectedTier { get; }
    }
}
