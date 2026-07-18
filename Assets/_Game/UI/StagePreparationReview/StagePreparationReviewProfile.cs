using System;
using System.Collections.Generic;
using DimensionBrawl.Player;
using UnityEngine;

namespace DimensionBrawl.UI.StagePreparationReview
{
    [CreateAssetMenu(
        menuName = "DimensionBrawl/UI/Review/Stage Preparation Review Profile",
        fileName = "DB_StagePreparationReview")]
    public sealed class StagePreparationReviewProfile : ScriptableObject
    {
        public const int RequiredSlotCount = 3;
        public const int RequiredTierCount = 3;

        [Serializable]
        public sealed class SlotDefinition
        {
            [SerializeField] private string slotId = string.Empty;
            [SerializeField] private string titleLocalizationKey = string.Empty;
            [SerializeField, TextArea(1, 2)] private string titleFallback = string.Empty;
            [SerializeField, TextArea(1, 2)] private string roleFallback = string.Empty;
            [SerializeField] private SummonSlotActionProfile actionProfile;
            [SerializeField] private Sprite icon;

            public SlotDefinition()
            {
            }

            public SlotDefinition(
                string slotId,
                string titleLocalizationKey,
                string titleFallback,
                string roleFallback,
                SummonSlotActionProfile actionProfile,
                Sprite icon)
            {
                Configure(
                    slotId,
                    titleLocalizationKey,
                    titleFallback,
                    roleFallback,
                    actionProfile,
                    icon);
            }

            public string SlotId => slotId;
            public string TitleLocalizationKey => titleLocalizationKey;
            public string TitleFallback => titleFallback;
            public string RoleFallback => roleFallback;
            public SummonSlotActionProfile ActionProfile => actionProfile;
            public Sprite Icon => icon;
            public string ActionId => actionProfile != null
                ? actionProfile.ActionId
                : string.Empty;
            public int TierCount => actionProfile != null
                ? actionProfile.TierCount
                : 0;
            public int TierReadoutCount => actionProfile != null
                ? actionProfile.TierReadoutCount
                : 0;

            public void Configure(
                string newSlotId,
                string newTitleLocalizationKey,
                string newTitleFallback,
                string newRoleFallback,
                SummonSlotActionProfile newActionProfile,
                Sprite newIcon)
            {
                slotId = newSlotId ?? string.Empty;
                titleLocalizationKey = newTitleLocalizationKey ?? string.Empty;
                titleFallback = newTitleFallback ?? string.Empty;
                roleFallback = newRoleFallback ?? string.Empty;
                actionProfile = newActionProfile;
                icon = newIcon;
            }

            public bool TryGetTierReadout(
                int tier,
                out SummonSlotActionProfile.SummonTierReadout readout)
            {
                if (actionProfile == null
                    || tier < 1
                    || tier > RequiredTierCount)
                {
                    readout = default;
                    return false;
                }

                return actionProfile.TryGetTierReadout(tier, out readout);
            }

            internal SlotDefinition DeepCopy()
            {
                return new SlotDefinition(
                    SlotId,
                    TitleLocalizationKey,
                    TitleFallback,
                    RoleFallback,
                    ActionProfile,
                    Icon);
            }
        }

        [Header("Review Identity")]
        [SerializeField] private string reviewId = "PREP-01";
        [SerializeField] private string canonicalCatalogEntryId = string.Empty;
        [SerializeField] private string titleLocalizationKey = string.Empty;
        [SerializeField, TextArea(1, 2)] private string titleFallback = string.Empty;

        [Header("Fixed Pilot Presentation")]
        [SerializeField] private string pilotPresentationId = string.Empty;
        [SerializeField] private string pilotTitleLocalizationKey = string.Empty;
        [SerializeField, TextArea(1, 2)] private string pilotTitleFallback = string.Empty;
        [SerializeField, TextArea(1, 3)] private string pilotBoundaryFallback = string.Empty;

        [Header("Canonical Runtime Preset")]
        [SerializeField] private SlotDefinition[] slots = Array.Empty<SlotDefinition>();

        public string ReviewId => reviewId;
        public string CanonicalCatalogEntryId => canonicalCatalogEntryId;
        public string TitleLocalizationKey => titleLocalizationKey;
        public string TitleFallback => titleFallback;
        public string PilotPresentationId => pilotPresentationId;
        public string PilotTitleLocalizationKey => pilotTitleLocalizationKey;
        public string PilotTitleFallback => pilotTitleFallback;
        public string PilotBoundaryFallback => pilotBoundaryFallback;
        public int SlotCount => slots?.Length ?? 0;
        public SlotDefinition[] Slots => CreateSlotSnapshot();

        public void Configure(
            string newReviewId,
            string newCanonicalCatalogEntryId,
            string newTitleLocalizationKey,
            string newTitleFallback,
            string newPilotPresentationId,
            string newPilotTitleLocalizationKey,
            string newPilotTitleFallback,
            string newPilotBoundaryFallback,
            SlotDefinition[] newSlots)
        {
            reviewId = newReviewId ?? string.Empty;
            canonicalCatalogEntryId = newCanonicalCatalogEntryId ?? string.Empty;
            titleLocalizationKey = newTitleLocalizationKey ?? string.Empty;
            titleFallback = newTitleFallback ?? string.Empty;
            pilotPresentationId = newPilotPresentationId ?? string.Empty;
            pilotTitleLocalizationKey = newPilotTitleLocalizationKey ?? string.Empty;
            pilotTitleFallback = newPilotTitleFallback ?? string.Empty;
            pilotBoundaryFallback = newPilotBoundaryFallback ?? string.Empty;
            slots = CloneSlots(newSlots);
        }

        public SlotDefinition GetSlot(int index)
        {
            if (index < 0 || index >= SlotCount)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return slots[index]?.DeepCopy();
        }

        public bool TryGetSlot(string slotId, out SlotDefinition slot)
        {
            if (!string.IsNullOrWhiteSpace(slotId) && slots != null)
            {
                for (int i = 0; i < slots.Length; i++)
                {
                    if (slots[i] != null
                        && string.Equals(slots[i].SlotId, slotId, StringComparison.Ordinal))
                    {
                        slot = slots[i].DeepCopy();
                        return true;
                    }
                }
            }

            slot = null;
            return false;
        }

        public bool TryValidate(out string error)
        {
            var issues = new List<string>();
            CollectValidationIssues(issues);
            error = issues.Count > 0 ? string.Join("\n", issues) : string.Empty;
            return issues.Count == 0;
        }

        public void CollectValidationIssues(List<string> issues)
        {
            if (issues == null)
            {
                return;
            }

            string label = string.IsNullOrWhiteSpace(name)
                ? nameof(StagePreparationReviewProfile)
                : name;
            if (string.IsNullOrWhiteSpace(reviewId))
            {
                issues.Add($"{label}: review id is missing.");
            }

            if (string.IsNullOrWhiteSpace(canonicalCatalogEntryId))
            {
                issues.Add($"{label}: canonical catalog entry id is missing.");
            }

            if (string.IsNullOrWhiteSpace(titleLocalizationKey)
                || string.IsNullOrWhiteSpace(titleFallback))
            {
                issues.Add($"{label}: review title key and neutral fallback are required.");
            }

            if (string.IsNullOrWhiteSpace(pilotPresentationId)
                || string.IsNullOrWhiteSpace(pilotTitleLocalizationKey)
                || string.IsNullOrWhiteSpace(pilotTitleFallback)
                || string.IsNullOrWhiteSpace(pilotBoundaryFallback))
            {
                issues.Add($"{label}: fixed pilot presentation fields are incomplete.");
            }

            SlotDefinition[] resolved = slots ?? Array.Empty<SlotDefinition>();
            if (resolved.Length != RequiredSlotCount)
            {
                issues.Add(
                    $"{label}: exactly {RequiredSlotCount} canonical runtime slots are required; found {resolved.Length}.");
            }

            var slotIds = new HashSet<string>(StringComparer.Ordinal);
            var actionIds = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < resolved.Length; i++)
            {
                SlotDefinition slot = resolved[i];
                string expectedSlotId = $"SummonSlot{i + 1}";
                if (slot == null)
                {
                    issues.Add($"{label}: slot {i} is null.");
                    continue;
                }

                if (!string.Equals(slot.SlotId, expectedSlotId, StringComparison.Ordinal))
                {
                    issues.Add(
                        $"{label}: slot {i} must use ordered id '{expectedSlotId}', found '{slot.SlotId}'.");
                }

                if (string.IsNullOrWhiteSpace(slot.SlotId)
                    || !slotIds.Add(slot.SlotId))
                {
                    issues.Add($"{label}: slot {i} has a missing or duplicate slot id.");
                }

                if (string.IsNullOrWhiteSpace(slot.TitleLocalizationKey)
                    || string.IsNullOrWhiteSpace(slot.TitleFallback)
                    || string.IsNullOrWhiteSpace(slot.RoleFallback))
                {
                    issues.Add($"{label}: slot '{slot.SlotId}' presentation copy is incomplete.");
                }

                if (slot.ActionProfile == null)
                {
                    issues.Add($"{label}: slot '{slot.SlotId}' has no canonical action profile.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(slot.ActionId)
                    || !actionIds.Add(slot.ActionId)
                    || !slot.ActionId.StartsWith(
                        slot.SlotId + ".",
                        StringComparison.Ordinal))
                {
                    issues.Add(
                        $"{label}: slot '{slot.SlotId}' action id '{slot.ActionId}' is missing, duplicated, or owned by another slot.");
                }

                if (slot.TierCount != RequiredTierCount
                    || slot.TierReadoutCount != RequiredTierCount)
                {
                    issues.Add(
                        $"{label}: slot '{slot.SlotId}' must expose exactly {RequiredTierCount} authored tiers and readouts.");
                }

                for (int tier = 1; tier <= RequiredTierCount; tier++)
                {
                    if (!slot.TryGetTierReadout(tier, out _))
                    {
                        issues.Add(
                            $"{label}: slot '{slot.SlotId}' tier {tier} has no complete canonical readout.");
                    }
                }

                if (slot.Icon == null)
                {
                    issues.Add($"{label}: slot '{slot.SlotId}' has no reviewed icon.");
                }
            }
        }

        internal SlotDefinition[] CreateSlotSnapshot()
        {
            return CloneSlots(slots);
        }

        private static SlotDefinition[] CloneSlots(SlotDefinition[] source)
        {
            if (source == null || source.Length == 0)
            {
                return Array.Empty<SlotDefinition>();
            }

            var clone = new SlotDefinition[source.Length];
            for (int i = 0; i < source.Length; i++)
            {
                clone[i] = source[i]?.DeepCopy();
            }

            return clone;
        }

        private void OnValidate()
        {
            slots ??= Array.Empty<SlotDefinition>();
        }
    }
}
