using System;
using System.Collections.Generic;
using DimensionBrawl.LevelDesign;
using UnityEngine;

namespace DimensionBrawl.UI.StageClear
{
    [CreateAssetMenu(
        menuName = "DimensionBrawl/UI/Stage Result Presentation Catalog",
        fileName = "DB_StageResultPresentationCatalog")]
    public sealed class StageResultPresentationCatalog : ScriptableObject
    {
        [SerializeField, Min(1)] private int schemaVersion = 1;
        [SerializeField] private string catalogId;
        [SerializeField, Min(1)] private int catalogRevision = 1;
        [SerializeField] private StageResultLocalizationTable localizationTable;
        [SerializeField] private StageResultPresentationProfile[] profiles =
            Array.Empty<StageResultPresentationProfile>();

        public int SchemaVersion => schemaVersion;
        public string CatalogId => catalogId;
        public int CatalogRevision => catalogRevision;
        public StageResultLocalizationTable LocalizationTable => localizationTable;
        public int ProfileCount => profiles != null ? profiles.Length : 0;

        public StageResultPresentationProfile GetProfile(int index)
        {
            if (profiles == null || index < 0 || index >= profiles.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return profiles[index];
        }

        public bool TryValidate(out string error)
        {
            error = string.Empty;
            if (schemaVersion != 1
                || catalogRevision < 1
                || string.IsNullOrWhiteSpace(catalogId))
            {
                error = "Stage-result presentation catalog identity is incomplete or unsupported.";
                return false;
            }

            if (localizationTable == null || !localizationTable.TryValidate(out error))
            {
                if (string.IsNullOrWhiteSpace(error))
                {
                    error = "Stage-result presentation catalog has no valid localization table.";
                }

                return false;
            }

            if (profiles == null || profiles.Length == 0)
            {
                error = "Stage-result presentation catalog has no stage profiles.";
                return false;
            }

            var stageIds = new HashSet<string>(StringComparer.Ordinal);
            string previousStageId = string.Empty;
            for (int i = 0; i < profiles.Length; i++)
            {
                StageResultPresentationProfile profile = profiles[i];
                if (profile == null || !profile.TryValidate(localizationTable, out error))
                {
                    if (string.IsNullOrWhiteSpace(error))
                    {
                        error = $"Stage-result presentation profile {i} is missing.";
                    }

                    return false;
                }

                if (!stageIds.Add(profile.PlayableStageId))
                {
                    error = $"Stage-result presentation catalog duplicates {profile.PlayableStageId}.";
                    return false;
                }

                if (i > 0
                    && string.Compare(previousStageId, profile.PlayableStageId, StringComparison.Ordinal) >= 0)
                {
                    error = "Stage-result presentation profiles must use ascending playable-stage ID order.";
                    return false;
                }

                previousStageId = profile.PlayableStageId;
            }

            return true;
        }

        public bool TryValidateExactSources(
            string playableStageId,
            StageResultPresentationProfile expectedProfile,
            StageResultLocalizationTable expectedLocalization,
            out string error)
        {
            error = string.Empty;
            try
            {
                if (!TryValidate(out error)
                    || string.IsNullOrWhiteSpace(playableStageId)
                    || expectedProfile == null
                    || expectedLocalization == null
                    || !ReferenceEquals(localizationTable, expectedLocalization))
                {
                    if (string.IsNullOrWhiteSpace(error))
                    {
                        error = "Stage-result definition does not reference the exact canonical localization source.";
                    }

                    return false;
                }

                StageResultPresentationProfile resolvedProfile = null;
                for (int i = 0; i < profiles.Length; i++)
                {
                    StageResultPresentationProfile profile = profiles[i];
                    if (string.Equals(
                            profile.PlayableStageId,
                            playableStageId,
                            StringComparison.Ordinal))
                    {
                        resolvedProfile = profile;
                        break;
                    }
                }

                if (!ReferenceEquals(resolvedProfile, expectedProfile))
                {
                    error = "Stage-result definition does not reference the exact canonical presentation profile.";
                    return false;
                }

                return true;
            }
            catch (Exception)
            {
                error = "Stage-result canonical presentation authority is damaged.";
                return false;
            }
        }

        public bool TryCreateSnapshot(
            StageRunResultSummary summary,
            string requestedLocaleId,
            out StageResultPresentationSnapshot snapshot,
            out string error)
        {
            snapshot = null;
            if (!TryValidate(out error))
            {
                return false;
            }

            if (summary == null || summary.Identity == null)
            {
                error = "Committed result summary is missing its stage identity.";
                return false;
            }

            for (int i = 0; i < profiles.Length; i++)
            {
                StageResultPresentationProfile profile = profiles[i];
                if (string.Equals(
                    profile.PlayableStageId,
                    summary.Identity.PlayableStageId,
                    StringComparison.Ordinal))
                {
                    return profile.TryCreateSnapshot(
                        summary,
                        localizationTable,
                        requestedLocaleId,
                        out snapshot,
                        out error);
                }
            }

            error = $"No stage-result presentation profile exists for {summary.Identity.PlayableStageId}.";
            return false;
        }
    }
}
