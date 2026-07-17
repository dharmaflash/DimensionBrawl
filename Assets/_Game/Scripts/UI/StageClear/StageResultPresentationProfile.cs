using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using DimensionBrawl.LevelDesign;
using UnityEngine;

namespace DimensionBrawl.UI.StageClear
{
    public enum StageResultProofValueFormat
    {
        Literal = 0,
        Count = 1,
        ActualNumber = 2,
        ActualDuration = 3
    }

    [Serializable]
    public sealed class StageResultProofPresentationRule
    {
        [SerializeField] private string proofId;
        [SerializeField] private string textKey;
        [SerializeField] private bool requireQualified = true;
        [SerializeField] private StageResultProofValueFormat valueFormat;

        public string ProofId => proofId;
        public string TextKey => textKey;
        public bool RequireQualified => requireQualified;
        public StageResultProofValueFormat ValueFormat => valueFormat;
    }

    public sealed class StageResultPresentationRowSnapshot
    {
        internal StageResultPresentationRowSnapshot(
            string proofId,
            string proofDigest,
            string localizedText)
        {
            ProofId = proofId ?? string.Empty;
            ProofDigest = proofDigest ?? string.Empty;
            LocalizedText = localizedText ?? string.Empty;
        }

        public string ProofId { get; }
        public string ProofDigest { get; }
        public string LocalizedText { get; }
    }

    public sealed class StageResultPresentationSnapshot
    {
        private readonly StageResultPresentationRowSnapshot[] proofRows;

        internal StageResultPresentationSnapshot(
            StageRunResultSummary summary,
            string profileId,
            int profileRevision,
            string profileDigest,
            string localizationTableId,
            int localizationTableRevision,
            string localizationDigest,
            string localeId,
            string stageCode,
            string stageTitle,
            string totalActiveTimeLabel,
            string combatActiveTimeLabel,
            string recordsCategoryLabel,
            string primaryActionLabel,
            string lobbyActionLabel,
            Color stageTitleColor,
            StageResultPresentationRowSnapshot[] proofRows)
        {
            SourceResultSummaryId = summary.ResultSummaryId;
            SourceResultSummaryDigest = summary.ResultSummaryDigest;
            PlayableStageId = summary.Identity.PlayableStageId;
            Outcome = summary.Outcome;
            ProfileId = profileId ?? string.Empty;
            ProfileRevision = profileRevision;
            ProfileDigest = profileDigest ?? string.Empty;
            LocalizationTableId = localizationTableId ?? string.Empty;
            LocalizationTableRevision = localizationTableRevision;
            LocalizationDigest = localizationDigest ?? string.Empty;
            LocaleId = localeId ?? string.Empty;
            StageCode = stageCode ?? string.Empty;
            StageTitle = stageTitle ?? string.Empty;
            TotalActiveElapsedMilliseconds = summary.OutcomeFact.TotalActiveElapsedMilliseconds;
            CombatActiveElapsedMilliseconds = summary.OutcomeFact.CombatActiveElapsedMilliseconds;
            TotalActiveTimeLabel = totalActiveTimeLabel ?? string.Empty;
            CombatActiveTimeLabel = combatActiveTimeLabel ?? string.Empty;
            TotalActiveTimeValue = StageResultPresentationProfile.FormatDuration(
                TotalActiveElapsedMilliseconds);
            CombatActiveTimeValue = StageResultPresentationProfile.FormatDuration(
                CombatActiveElapsedMilliseconds);
            RecordsCategoryLabel = recordsCategoryLabel ?? string.Empty;
            PrimaryActionLabel = primaryActionLabel ?? string.Empty;
            LobbyActionLabel = lobbyActionLabel ?? string.Empty;
            StageTitleColor = stageTitleColor;
            this.proofRows = proofRows ?? Array.Empty<StageResultPresentationRowSnapshot>();
            CanonicalDigest = ComputeCanonicalDigest();
        }

        public string SourceResultSummaryId { get; }
        public string SourceResultSummaryDigest { get; }
        public string PlayableStageId { get; }
        public StageRouteOutcome Outcome { get; }
        public string ProfileId { get; }
        public int ProfileRevision { get; }
        public string ProfileDigest { get; }
        public string LocalizationTableId { get; }
        public int LocalizationTableRevision { get; }
        public string LocalizationDigest { get; }
        public string LocaleId { get; }
        public string StageCode { get; }
        public string StageTitle { get; }
        public long TotalActiveElapsedMilliseconds { get; }
        public long CombatActiveElapsedMilliseconds { get; }
        public string TotalActiveTimeLabel { get; }
        public string CombatActiveTimeLabel { get; }
        public string TotalActiveTimeValue { get; }
        public string CombatActiveTimeValue { get; }
        public string RecordsCategoryLabel { get; }
        public string PrimaryActionLabel { get; }
        public string LobbyActionLabel { get; }
        public Color StageTitleColor { get; }
        public int ProofRowCount => proofRows.Length;
        public string CanonicalDigest { get; }

        internal string RecomputeCanonicalDigest()
        {
            return ComputeCanonicalDigest();
        }

        public StageResultPresentationRowSnapshot GetProofRow(int index)
        {
            if (index < 0 || index >= proofRows.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return proofRows[index];
        }

        private string ComputeCanonicalDigest()
        {
            StringBuilder builder = new(2048);
            StageCanonicalDigest.Append(builder, "presentation.sourceResultId", SourceResultSummaryId);
            StageCanonicalDigest.Append(builder, "presentation.sourceResultDigest", SourceResultSummaryDigest);
            StageCanonicalDigest.Append(builder, "presentation.playableStageId", PlayableStageId);
            StageCanonicalDigest.Append(builder, "presentation.outcome", (int)Outcome);
            StageCanonicalDigest.Append(builder, "presentation.profileId", ProfileId);
            StageCanonicalDigest.Append(builder, "presentation.profileRevision", ProfileRevision);
            StageCanonicalDigest.Append(builder, "presentation.profileDigest", ProfileDigest);
            StageCanonicalDigest.Append(builder, "presentation.localizationTableId", LocalizationTableId);
            StageCanonicalDigest.Append(
                builder,
                "presentation.localizationTableRevision",
                LocalizationTableRevision);
            StageCanonicalDigest.Append(builder, "presentation.localizationDigest", LocalizationDigest);
            StageCanonicalDigest.Append(builder, "presentation.localeId", LocaleId);
            StageCanonicalDigest.Append(builder, "presentation.stageCode", StageCode);
            StageCanonicalDigest.Append(builder, "presentation.stageTitle", StageTitle);
            StageCanonicalDigest.Append(
                builder,
                "presentation.totalActiveElapsedMilliseconds",
                TotalActiveElapsedMilliseconds);
            StageCanonicalDigest.Append(
                builder,
                "presentation.combatActiveElapsedMilliseconds",
                CombatActiveElapsedMilliseconds);
            StageCanonicalDigest.Append(builder, "presentation.totalTimeLabel", TotalActiveTimeLabel);
            StageCanonicalDigest.Append(builder, "presentation.combatTimeLabel", CombatActiveTimeLabel);
            StageCanonicalDigest.Append(builder, "presentation.totalTimeValue", TotalActiveTimeValue);
            StageCanonicalDigest.Append(builder, "presentation.combatTimeValue", CombatActiveTimeValue);
            StageCanonicalDigest.Append(builder, "presentation.recordsCategory", RecordsCategoryLabel);
            StageCanonicalDigest.Append(builder, "presentation.primaryAction", PrimaryActionLabel);
            StageCanonicalDigest.Append(builder, "presentation.lobbyAction", LobbyActionLabel);
            StageCanonicalDigest.Append(
                builder,
                "presentation.stageTitleColor",
                ColorUtility.ToHtmlStringRGBA(StageTitleColor));
            StageCanonicalDigest.Append(builder, "presentation.proofRowCount", proofRows.Length);
            for (int i = 0; i < proofRows.Length; i++)
            {
                StageResultPresentationRowSnapshot row = proofRows[i];
                string prefix = $"presentation.proofRow[{i}]";
                StageCanonicalDigest.Append(builder, prefix + ".proofId", row?.ProofId);
                StageCanonicalDigest.Append(builder, prefix + ".proofDigest", row?.ProofDigest);
                StageCanonicalDigest.Append(builder, prefix + ".localizedText", row?.LocalizedText);
            }

            return StageCanonicalDigest.Compute(builder.ToString());
        }
    }

    [CreateAssetMenu(
        menuName = "DimensionBrawl/UI/Stage Result Presentation Profile",
        fileName = "DB_StageResultPresentation")]
    public sealed class StageResultPresentationProfile : ScriptableObject
    {
        [SerializeField, Min(1)] private int schemaVersion = 1;
        [SerializeField] private string profileId;
        [SerializeField, Min(1)] private int profileRevision = 1;
        [SerializeField] private string playableStageId;
        [SerializeField, Min(1)] private int supportedRunSchemaVersion = 1;
        [SerializeField] private string stageCode;
        [SerializeField] private string titleFormatKey;
        [SerializeField] private string stageNameKey;
        [SerializeField] private string clearStatusKey;
        [SerializeField] private string failStatusKey;
        [SerializeField] private string totalActiveTimeLabelKey;
        [SerializeField] private string combatActiveTimeLabelKey;
        [SerializeField] private string recordsCategoryKey;
        [SerializeField] private string replayActionKey;
        [SerializeField] private string retryActionKey;
        [SerializeField] private string lobbyActionKey;
        [SerializeField, Range(1, 3)] private int proofRowLimit = 3;
        [SerializeField] private StageResultProofPresentationRule[] proofRules =
            Array.Empty<StageResultProofPresentationRule>();
        [SerializeField] private Color clearTitleColor =
            new(0.06666667f, 0.14901961f, 0.34509805f, 1f);
        [SerializeField] private Color failTitleColor = new(0.62f, 0.08f, 0.12f, 1f);

        public int SchemaVersion => schemaVersion;
        public string ProfileId => profileId;
        public int ProfileRevision => profileRevision;
        public string PlayableStageId => playableStageId;
        public int SupportedRunSchemaVersion => supportedRunSchemaVersion;
        public int ProofRuleCount => proofRules != null ? proofRules.Length : 0;
        public int ProofRowLimit => proofRowLimit;
        internal string StageCode => stageCode;
        internal string TitleFormatKey => titleFormatKey;
        internal string StageNameKey => stageNameKey;
        internal string ClearStatusKey => clearStatusKey;
        internal string FailStatusKey => failStatusKey;
        internal string TotalActiveTimeLabelKey => totalActiveTimeLabelKey;
        internal string CombatActiveTimeLabelKey => combatActiveTimeLabelKey;
        internal string RecordsCategoryKey => recordsCategoryKey;
        internal string ReplayActionKey => replayActionKey;
        internal string RetryActionKey => retryActionKey;
        internal string LobbyActionKey => lobbyActionKey;
        internal Color ClearTitleColor => clearTitleColor;
        internal Color FailTitleColor => failTitleColor;

        public StageResultProofPresentationRule GetProofRule(int index)
        {
            if (proofRules == null || index < 0 || index >= proofRules.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return proofRules[index];
        }

        public bool TryValidate(StageResultLocalizationTable localization, out string error)
        {
            error = string.Empty;
            if (schemaVersion != 1
                || profileRevision < 1
                || supportedRunSchemaVersion != 1
                || string.IsNullOrWhiteSpace(profileId)
                || string.IsNullOrWhiteSpace(playableStageId)
                || string.IsNullOrWhiteSpace(stageCode))
            {
                error = "Stage-result presentation profile identity is incomplete or unsupported.";
                return false;
            }

            if (proofRowLimit < 1 || proofRowLimit > 3)
            {
                error = "Stage-result presentation profile proof row limit must be 1-3.";
                return false;
            }

            if (localization == null || !localization.TryValidate(out error))
            {
                if (string.IsNullOrWhiteSpace(error))
                {
                    error = "Stage-result presentation profile has no localization table.";
                }

                return false;
            }

            string[] requiredKeys =
            {
                titleFormatKey,
                stageNameKey,
                clearStatusKey,
                failStatusKey,
                totalActiveTimeLabelKey,
                combatActiveTimeLabelKey,
                recordsCategoryKey,
                replayActionKey,
                retryActionKey,
                lobbyActionKey
            };
            for (int i = 0; i < requiredKeys.Length; i++)
            {
                if (!TryValidateLocalizationKey(localization, requiredKeys[i], out error))
                {
                    return false;
                }
            }

            if (!localization.TryResolve(
                    localization.DefaultLocaleId,
                    titleFormatKey,
                    out string titleFormat))
            {
                error = "Stage-result title format is missing from the default locale.";
                return false;
            }

            try
            {
                _ = string.Format(CultureInfo.InvariantCulture, titleFormat, "stage", "status");
            }
            catch (FormatException)
            {
                error = "Stage-result title format must accept stage and status arguments.";
                return false;
            }

            var proofIds = new HashSet<string>(StringComparer.Ordinal);
            if (proofRules == null)
            {
                proofRules = Array.Empty<StageResultProofPresentationRule>();
            }

            for (int i = 0; i < proofRules.Length; i++)
            {
                StageResultProofPresentationRule rule = proofRules[i];
                if (rule == null
                    || string.IsNullOrWhiteSpace(rule.ProofId)
                    || !proofIds.Add(rule.ProofId))
                {
                    error = $"Stage-result proof rule {i} has a missing or duplicate proof ID.";
                    return false;
                }

                if (!TryValidateLocalizationKey(localization, rule.TextKey, out error))
                {
                    return false;
                }
            }

            return true;
        }

        public bool TryCreateSnapshot(
            StageRunResultSummary summary,
            StageResultLocalizationTable localization,
            string requestedLocaleId,
            out StageResultPresentationSnapshot snapshot,
            out string error)
        {
            snapshot = null;
            if (!TryValidate(localization, out error))
            {
                return false;
            }

            if (summary == null
                || summary.Identity == null
                || summary.OutcomeFact == null
                || summary.Identity.SchemaVersion != supportedRunSchemaVersion
                || !string.Equals(
                    summary.Identity.PlayableStageId,
                    playableStageId,
                    StringComparison.Ordinal))
            {
                error = "Committed result does not match the presentation profile identity/schema.";
                return false;
            }

            if (summary.Outcome != StageRouteOutcome.Clear
                && summary.Outcome != StageRouteOutcome.Fail)
            {
                error = "Stage-result presentation received an unsupported outcome.";
                return false;
            }

            if (!localization.TryResolveLocale(requestedLocaleId, out string localeId, out error))
            {
                return false;
            }

            string statusKey = summary.Outcome == StageRouteOutcome.Clear
                ? clearStatusKey
                : failStatusKey;
            string primaryActionKey = summary.Outcome == StageRouteOutcome.Clear
                ? replayActionKey
                : retryActionKey;
            if (!TryResolve(localization, localeId, titleFormatKey, out string titleFormat, out error)
                || !TryResolve(localization, localeId, stageNameKey, out string stageName, out error)
                || !TryResolve(localization, localeId, statusKey, out string status, out error)
                || !TryResolve(
                    localization,
                    localeId,
                    totalActiveTimeLabelKey,
                    out string totalTimeLabel,
                    out error)
                || !TryResolve(
                    localization,
                    localeId,
                    combatActiveTimeLabelKey,
                    out string combatTimeLabel,
                    out error)
                || !TryResolve(
                    localization,
                    localeId,
                    recordsCategoryKey,
                    out string recordsCategory,
                    out error)
                || !TryResolve(
                    localization,
                    localeId,
                    primaryActionKey,
                    out string primaryAction,
                    out error)
                || !TryResolve(
                    localization,
                    localeId,
                    lobbyActionKey,
                    out string lobbyAction,
                    out error))
            {
                return false;
            }

            string stageTitle;
            try
            {
                stageTitle = string.Format(CultureInfo.InvariantCulture, titleFormat, stageName, status);
            }
            catch (FormatException)
            {
                error = "Stage-result localized title format is invalid.";
                return false;
            }

            var rows = new List<StageResultPresentationRowSnapshot>(proofRowLimit);
            for (int i = 0; i < ProofRuleCount && rows.Count < proofRowLimit; i++)
            {
                StageResultProofPresentationRule rule = proofRules[i];
                if (!summary.TryGetSemanticProof(rule.ProofId, out StageRunSemanticProofFact proof)
                    || (rule.RequireQualified && !proof.Qualified))
                {
                    continue;
                }

                if (!TryResolve(
                    localization,
                    localeId,
                    rule.TextKey,
                    out string rowFormat,
                    out error))
                {
                    return false;
                }

                if (!TryFormatProofRow(rule, proof, rowFormat, out string rowText, out error))
                {
                    return false;
                }

                rows.Add(new StageResultPresentationRowSnapshot(
                    proof.ProofId,
                    proof.CanonicalDigest,
                    rowText));
            }

            snapshot = new StageResultPresentationSnapshot(
                summary,
                profileId,
                profileRevision,
                ComputeCanonicalDigest(),
                localization.TableId,
                localization.TableRevision,
                localization.ComputeCanonicalDigest(),
                localeId,
                stageCode,
                stageTitle,
                totalTimeLabel,
                combatTimeLabel,
                recordsCategory,
                primaryAction,
                lobbyAction,
                summary.Outcome == StageRouteOutcome.Clear ? clearTitleColor : failTitleColor,
                rows.ToArray());
            error = string.Empty;
            return true;
        }

        public string ComputeCanonicalDigest()
        {
            StringBuilder builder = new(2048);
            StageCanonicalDigest.Append(builder, "profile.schemaVersion", schemaVersion);
            StageCanonicalDigest.Append(builder, "profile.id", profileId);
            StageCanonicalDigest.Append(builder, "profile.revision", profileRevision);
            StageCanonicalDigest.Append(builder, "profile.playableStageId", playableStageId);
            StageCanonicalDigest.Append(
                builder,
                "profile.supportedRunSchemaVersion",
                supportedRunSchemaVersion);
            StageCanonicalDigest.Append(builder, "profile.stageCode", stageCode);
            StageCanonicalDigest.Append(builder, "profile.titleFormatKey", titleFormatKey);
            StageCanonicalDigest.Append(builder, "profile.stageNameKey", stageNameKey);
            StageCanonicalDigest.Append(builder, "profile.clearStatusKey", clearStatusKey);
            StageCanonicalDigest.Append(builder, "profile.failStatusKey", failStatusKey);
            StageCanonicalDigest.Append(
                builder,
                "profile.totalActiveTimeLabelKey",
                totalActiveTimeLabelKey);
            StageCanonicalDigest.Append(
                builder,
                "profile.combatActiveTimeLabelKey",
                combatActiveTimeLabelKey);
            StageCanonicalDigest.Append(builder, "profile.recordsCategoryKey", recordsCategoryKey);
            StageCanonicalDigest.Append(builder, "profile.replayActionKey", replayActionKey);
            StageCanonicalDigest.Append(builder, "profile.retryActionKey", retryActionKey);
            StageCanonicalDigest.Append(builder, "profile.lobbyActionKey", lobbyActionKey);
            StageCanonicalDigest.Append(builder, "profile.proofRowLimit", proofRowLimit);
            StageCanonicalDigest.Append(builder, "profile.proofRuleCount", ProofRuleCount);
            for (int i = 0; i < ProofRuleCount; i++)
            {
                StageResultProofPresentationRule rule = proofRules[i];
                string prefix = $"profile.proofRule[{i}]";
                StageCanonicalDigest.Append(builder, prefix + ".proofId", rule?.ProofId);
                StageCanonicalDigest.Append(builder, prefix + ".textKey", rule?.TextKey);
                StageCanonicalDigest.Append(
                    builder,
                    prefix + ".requireQualified",
                    rule != null && rule.RequireQualified);
                StageCanonicalDigest.Append(
                    builder,
                    prefix + ".valueFormat",
                    rule != null ? (int)rule.ValueFormat : -1);
            }

            StageCanonicalDigest.Append(
                builder,
                "profile.clearTitleColor",
                ColorUtility.ToHtmlStringRGBA(clearTitleColor));
            StageCanonicalDigest.Append(
                builder,
                "profile.failTitleColor",
                ColorUtility.ToHtmlStringRGBA(failTitleColor));
            return StageCanonicalDigest.Compute(builder.ToString());
        }

        internal static string FormatDuration(long milliseconds)
        {
            long safeMilliseconds = Math.Max(0L, milliseconds);
            long totalSeconds = safeMilliseconds / 1000L;
            if (safeMilliseconds % 1000L != 0L)
            {
                totalSeconds++;
            }

            long hours = totalSeconds / 3600L;
            long minutes = (totalSeconds / 60L) % 60L;
            long seconds = totalSeconds % 60L;
            return hours > 0L
                ? string.Format(CultureInfo.InvariantCulture, "{0}:{1:00}:{2:00}", hours, minutes, seconds)
                : string.Format(CultureInfo.InvariantCulture, "{0:00}:{1:00}", minutes, seconds);
        }

        private static bool TryValidateLocalizationKey(
            StageResultLocalizationTable localization,
            string key,
            out string error)
        {
            if (string.IsNullOrWhiteSpace(key)
                || !localization.TryResolve(localization.DefaultLocaleId, key, out _))
            {
                error = $"Stage-result presentation localization key is missing: {key ?? "<null>"}.";
                return false;
            }

            error = string.Empty;
            return true;
        }

        private static bool TryResolve(
            StageResultLocalizationTable localization,
            string localeId,
            string key,
            out string value,
            out string error)
        {
            if (!localization.TryResolve(localeId, key, out value))
            {
                error = $"Stage-result localization {localeId} is missing key {key}.";
                return false;
            }

            error = string.Empty;
            return true;
        }

        private static bool TryFormatProofRow(
            StageResultProofPresentationRule rule,
            StageRunSemanticProofFact proof,
            string localizedFormat,
            out string text,
            out string error)
        {
            error = string.Empty;
            try
            {
                switch (rule.ValueFormat)
                {
                    case StageResultProofValueFormat.Literal:
                        text = localizedFormat;
                        return true;
                    case StageResultProofValueFormat.Count:
                        text = string.Format(
                            CultureInfo.InvariantCulture,
                            localizedFormat,
                            proof.Count);
                        return true;
                    case StageResultProofValueFormat.ActualNumber:
                        text = string.Format(
                            CultureInfo.InvariantCulture,
                            localizedFormat,
                            proof.ActualValue.ToString("0.##", CultureInfo.InvariantCulture));
                        return true;
                    case StageResultProofValueFormat.ActualDuration:
                        long milliseconds = proof.ActualValue >= long.MaxValue
                            ? long.MaxValue
                            : (long)Math.Ceiling(proof.ActualValue);
                        text = string.Format(
                            CultureInfo.InvariantCulture,
                            localizedFormat,
                            FormatDuration(milliseconds));
                        return true;
                    default:
                        text = string.Empty;
                        error = $"Stage-result proof rule {rule.ProofId} has an unsupported value format.";
                        return false;
                }
            }
            catch (FormatException)
            {
                text = string.Empty;
                error = $"Stage-result proof localization format is invalid for {rule.ProofId}.";
                return false;
            }
        }
    }
}
