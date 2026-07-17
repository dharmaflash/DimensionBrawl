using System;
using System.Collections.Generic;

namespace DimensionBrawl.UI.LobbyOperationsReview
{
    public enum LobbyOperationsReviewPhase
    {
        Closed = 0,
        Directory = 10,
        EntryDetail = 20,
        ReviewConfirm = 30
    }

    public sealed class LobbyOperationsReviewSession
    {
        private readonly LobbyOperationsReviewProfile.EntryDefinition[] entries;
        private readonly Dictionary<string, LobbyOperationsReviewProfile.EntryDefinition>
            entriesById;

        private string selectedEntryId = string.Empty;
        private bool reviewAcknowledged;

        public LobbyOperationsReviewSession(LobbyOperationsReviewProfile profile)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            if (!profile.TryValidate(out string validationError))
            {
                throw new ArgumentException(validationError, nameof(profile));
            }

            entries = profile.CreateEntrySnapshot();
            entriesById = new Dictionary<
                string,
                LobbyOperationsReviewProfile.EntryDefinition>(
                entries.Length,
                StringComparer.Ordinal);
            for (int index = 0; index < entries.Length; index++)
            {
                LobbyOperationsReviewProfile.EntryDefinition entry = entries[index];
                entriesById.Add(entry.EntryId, entry);
            }
        }

        public LobbyOperationsReviewPhase Phase { get; private set; } =
            LobbyOperationsReviewPhase.Closed;
        public int EntryCount => entries.Length;
        public string SelectedEntryId => selectedEntryId;
        public bool IsReviewAcknowledged => reviewAcknowledged;
        public LobbyOperationsReviewProfile.EntryDefinition SelectedEntry =>
            TryGetSelectedEntryInternal(out LobbyOperationsReviewProfile.EntryDefinition entry)
                ? entry.DeepCopy()
                : null;

        public LobbyOperationsReviewProfile.EntryDefinition GetEntry(int index)
        {
            if (index < 0 || index >= entries.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return entries[index].DeepCopy();
        }

        public bool TryGetEntry(
            string entryId,
            out LobbyOperationsReviewProfile.EntryDefinition entry)
        {
            if (!string.IsNullOrWhiteSpace(entryId)
                && entriesById.TryGetValue(
                    entryId,
                    out LobbyOperationsReviewProfile.EntryDefinition found))
            {
                entry = found.DeepCopy();
                return true;
            }

            entry = null;
            return false;
        }

        public bool TryOpenDrawer()
        {
            if (Phase != LobbyOperationsReviewPhase.Closed)
            {
                return false;
            }

            selectedEntryId = string.Empty;
            Phase = LobbyOperationsReviewPhase.Directory;
            return true;
        }

        public bool TrySelectEntry(string entryId)
        {
            if (Phase != LobbyOperationsReviewPhase.Directory
                || string.IsNullOrWhiteSpace(entryId)
                || !entriesById.ContainsKey(entryId))
            {
                return false;
            }

            selectedEntryId = entryId;
            Phase = LobbyOperationsReviewPhase.EntryDetail;
            return true;
        }

        public bool TryOpenReviewConfirm()
        {
            if (Phase != LobbyOperationsReviewPhase.EntryDetail
                || !TryGetSelectedEntryInternal(
                    out LobbyOperationsReviewProfile.EntryDefinition selectedEntry)
                || selectedEntry.ActionDisposition
                    != LobbyOperationsReviewActionDisposition.LocalReviewConfirm)
            {
                return false;
            }

            Phase = LobbyOperationsReviewPhase.ReviewConfirm;
            return true;
        }

        public bool TryAcknowledgeReview(out string entryId)
        {
            entryId = string.Empty;
            if (reviewAcknowledged
                || Phase != LobbyOperationsReviewPhase.ReviewConfirm
                || !TryGetSelectedEntryInternal(
                    out LobbyOperationsReviewProfile.EntryDefinition selectedEntry)
                || selectedEntry.ActionDisposition
                    != LobbyOperationsReviewActionDisposition.LocalReviewConfirm)
            {
                return false;
            }

            reviewAcknowledged = true;
            entryId = selectedEntry.EntryId;
            return true;
        }

        public bool TryBack()
        {
            switch (Phase)
            {
                case LobbyOperationsReviewPhase.ReviewConfirm:
                    Phase = LobbyOperationsReviewPhase.EntryDetail;
                    return true;
                case LobbyOperationsReviewPhase.EntryDetail:
                    selectedEntryId = string.Empty;
                    Phase = LobbyOperationsReviewPhase.Directory;
                    return true;
                case LobbyOperationsReviewPhase.Directory:
                    selectedEntryId = string.Empty;
                    Phase = LobbyOperationsReviewPhase.Closed;
                    return true;
                default:
                    return false;
            }
        }

        public bool TryClose()
        {
            if (Phase == LobbyOperationsReviewPhase.Closed)
            {
                return false;
            }

            selectedEntryId = string.Empty;
            Phase = LobbyOperationsReviewPhase.Closed;
            return true;
        }

        private bool TryGetSelectedEntryInternal(
            out LobbyOperationsReviewProfile.EntryDefinition entry)
        {
            if (!string.IsNullOrWhiteSpace(selectedEntryId)
                && entriesById.TryGetValue(selectedEntryId, out entry))
            {
                return true;
            }

            entry = null;
            return false;
        }
    }
}
