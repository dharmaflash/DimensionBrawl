using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using DimensionBrawl.LevelDesign;
using UnityEngine;

namespace DimensionBrawl.UI.StageClear
{
    public sealed class StageResultProofPresentationRuleSnapshot
    {
        internal StageResultProofPresentationRuleSnapshot(
            string proofId,
            string textKey,
            bool requireQualified,
            StageResultProofValueFormat valueFormat)
        {
            ProofId = proofId ?? string.Empty;
            TextKey = textKey ?? string.Empty;
            RequireQualified = requireQualified;
            ValueFormat = valueFormat;
        }

        public string ProofId { get; }
        public string TextKey { get; }
        public bool RequireQualified { get; }
        public StageResultProofValueFormat ValueFormat { get; }
    }

    public sealed class StageResultPresentationProfileSnapshot
    {
        private readonly StageResultProofPresentationRuleSnapshot[] proofRules;

        private StageResultPresentationProfileSnapshot(
            StageResultPresentationProfile profile,
            StageResultProofPresentationRuleSnapshot[] proofRules)
        {
            SchemaVersion = profile.SchemaVersion;
            ProfileId = profile.ProfileId ?? string.Empty;
            ProfileRevision = profile.ProfileRevision;
            PlayableStageId = profile.PlayableStageId ?? string.Empty;
            SupportedRunSchemaVersion = profile.SupportedRunSchemaVersion;
            StageCode = profile.StageCode ?? string.Empty;
            TitleFormatKey = profile.TitleFormatKey ?? string.Empty;
            StageNameKey = profile.StageNameKey ?? string.Empty;
            ClearStatusKey = profile.ClearStatusKey ?? string.Empty;
            FailStatusKey = profile.FailStatusKey ?? string.Empty;
            TotalActiveTimeLabelKey = profile.TotalActiveTimeLabelKey ?? string.Empty;
            CombatActiveTimeLabelKey = profile.CombatActiveTimeLabelKey ?? string.Empty;
            RecordsCategoryKey = profile.RecordsCategoryKey ?? string.Empty;
            ReplayActionKey = profile.ReplayActionKey ?? string.Empty;
            RetryActionKey = profile.RetryActionKey ?? string.Empty;
            LobbyActionKey = profile.LobbyActionKey ?? string.Empty;
            ProofRowLimit = profile.ProofRowLimit;
            ClearTitleColor = profile.ClearTitleColor;
            FailTitleColor = profile.FailTitleColor;
            this.proofRules = proofRules ?? Array.Empty<StageResultProofPresentationRuleSnapshot>();
            CanonicalDigest = RecomputeCanonicalDigest();
        }

        public int SchemaVersion { get; }
        public string ProfileId { get; }
        public int ProfileRevision { get; }
        public string PlayableStageId { get; }
        public int SupportedRunSchemaVersion { get; }
        public string StageCode { get; }
        public string TitleFormatKey { get; }
        public string StageNameKey { get; }
        public string ClearStatusKey { get; }
        public string FailStatusKey { get; }
        public string TotalActiveTimeLabelKey { get; }
        public string CombatActiveTimeLabelKey { get; }
        public string RecordsCategoryKey { get; }
        public string ReplayActionKey { get; }
        public string RetryActionKey { get; }
        public string LobbyActionKey { get; }
        public int ProofRowLimit { get; }
        public Color ClearTitleColor { get; }
        public Color FailTitleColor { get; }
        public int ProofRuleCount => proofRules.Length;
        public string CanonicalDigest { get; }

        public StageResultProofPresentationRuleSnapshot GetProofRule(int index)
        {
            if (index < 0 || index >= proofRules.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return proofRules[index];
        }

        internal static bool TryCreate(
            StageResultPresentationProfile profile,
            out StageResultPresentationProfileSnapshot snapshot,
            out string error)
        {
            snapshot = null;
            error = string.Empty;
            if (profile == null)
            {
                error = "Stage-result presentation profile is missing.";
                return false;
            }

            var rules = new StageResultProofPresentationRuleSnapshot[profile.ProofRuleCount];
            for (int i = 0; i < rules.Length; i++)
            {
                StageResultProofPresentationRule rule = profile.GetProofRule(i);
                if (rule == null)
                {
                    error = $"Stage-result presentation proof rule {i} is missing.";
                    return false;
                }

                rules[i] = new StageResultProofPresentationRuleSnapshot(
                    rule.ProofId,
                    rule.TextKey,
                    rule.RequireQualified,
                    rule.ValueFormat);
            }

            var candidate = new StageResultPresentationProfileSnapshot(profile, rules);
            if (!string.Equals(
                    candidate.CanonicalDigest,
                    profile.ComputeCanonicalDigest(),
                    StringComparison.Ordinal))
            {
                error = "Deep-copied stage-result profile digest does not match its source.";
                return false;
            }

            snapshot = candidate;
            return true;
        }

        internal bool TryValidateIntegrity(out string error)
        {
            error = string.Empty;
            if (SchemaVersion != 1
                || ProfileRevision < 1
                || SupportedRunSchemaVersion != 1
                || string.IsNullOrWhiteSpace(ProfileId)
                || string.IsNullOrWhiteSpace(PlayableStageId)
                || string.IsNullOrWhiteSpace(StageCode)
                || ProofRowLimit < 1
                || ProofRowLimit > 3
                || !string.Equals(
                    CanonicalDigest,
                    RecomputeCanonicalDigest(),
                    StringComparison.Ordinal))
            {
                error = "Deep-copied stage-result presentation profile is damaged.";
                return false;
            }

            error = string.Empty;
            return true;
        }

        private string RecomputeCanonicalDigest()
        {
            StringBuilder builder = new(2048);
            StageCanonicalDigest.Append(builder, "profile.schemaVersion", SchemaVersion);
            StageCanonicalDigest.Append(builder, "profile.id", ProfileId);
            StageCanonicalDigest.Append(builder, "profile.revision", ProfileRevision);
            StageCanonicalDigest.Append(builder, "profile.playableStageId", PlayableStageId);
            StageCanonicalDigest.Append(
                builder,
                "profile.supportedRunSchemaVersion",
                SupportedRunSchemaVersion);
            StageCanonicalDigest.Append(builder, "profile.stageCode", StageCode);
            StageCanonicalDigest.Append(builder, "profile.titleFormatKey", TitleFormatKey);
            StageCanonicalDigest.Append(builder, "profile.stageNameKey", StageNameKey);
            StageCanonicalDigest.Append(builder, "profile.clearStatusKey", ClearStatusKey);
            StageCanonicalDigest.Append(builder, "profile.failStatusKey", FailStatusKey);
            StageCanonicalDigest.Append(
                builder,
                "profile.totalActiveTimeLabelKey",
                TotalActiveTimeLabelKey);
            StageCanonicalDigest.Append(
                builder,
                "profile.combatActiveTimeLabelKey",
                CombatActiveTimeLabelKey);
            StageCanonicalDigest.Append(builder, "profile.recordsCategoryKey", RecordsCategoryKey);
            StageCanonicalDigest.Append(builder, "profile.replayActionKey", ReplayActionKey);
            StageCanonicalDigest.Append(builder, "profile.retryActionKey", RetryActionKey);
            StageCanonicalDigest.Append(builder, "profile.lobbyActionKey", LobbyActionKey);
            StageCanonicalDigest.Append(builder, "profile.proofRowLimit", ProofRowLimit);
            StageCanonicalDigest.Append(builder, "profile.proofRuleCount", proofRules.Length);
            for (int i = 0; i < proofRules.Length; i++)
            {
                StageResultProofPresentationRuleSnapshot rule = proofRules[i];
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
                ColorUtility.ToHtmlStringRGBA(ClearTitleColor));
            StageCanonicalDigest.Append(
                builder,
                "profile.failTitleColor",
                ColorUtility.ToHtmlStringRGBA(FailTitleColor));
            return StageCanonicalDigest.Compute(builder.ToString());
        }
    }

    public sealed class StageResultLocalizedEntrySnapshot
    {
        internal StageResultLocalizedEntrySnapshot(string key, string value)
        {
            Key = key ?? string.Empty;
            Value = value ?? string.Empty;
        }

        public string Key { get; }
        public string Value { get; }
    }

    public sealed class StageResultLocaleSnapshot
    {
        private readonly StageResultLocalizedEntrySnapshot[] entries;

        internal StageResultLocaleSnapshot(
            string localeId,
            StageResultLocalizedEntrySnapshot[] entries)
        {
            LocaleId = localeId ?? string.Empty;
            this.entries = entries ?? Array.Empty<StageResultLocalizedEntrySnapshot>();
        }

        public string LocaleId { get; }
        public int EntryCount => entries.Length;

        public StageResultLocalizedEntrySnapshot GetEntry(int index)
        {
            if (index < 0 || index >= entries.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return entries[index];
        }

        internal bool TryResolve(string key, out string value)
        {
            for (int i = 0; i < entries.Length; i++)
            {
                StageResultLocalizedEntrySnapshot entry = entries[i];
                if (entry != null && string.Equals(entry.Key, key, StringComparison.Ordinal))
                {
                    value = entry.Value;
                    return true;
                }
            }

            value = string.Empty;
            return false;
        }
    }

    public sealed class StageResultLocalizationSnapshot
    {
        private readonly StageResultLocaleSnapshot[] locales;

        private StageResultLocalizationSnapshot(
            StageResultLocalizationTable table,
            StageResultLocaleSnapshot[] locales)
        {
            SchemaVersion = table.SchemaVersion;
            TableId = table.TableId ?? string.Empty;
            TableRevision = table.TableRevision;
            DefaultLocaleId = table.DefaultLocaleId ?? string.Empty;
            this.locales = locales ?? Array.Empty<StageResultLocaleSnapshot>();
            CanonicalDigest = RecomputeCanonicalDigest();
        }

        public int SchemaVersion { get; }
        public string TableId { get; }
        public int TableRevision { get; }
        public string DefaultLocaleId { get; }
        public int LocaleCount => locales.Length;
        public string CanonicalDigest { get; }

        public StageResultLocaleSnapshot GetLocale(int index)
        {
            if (index < 0 || index >= locales.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return locales[index];
        }

        internal static bool TryCreate(
            StageResultLocalizationTable table,
            out StageResultLocalizationSnapshot snapshot,
            out string error)
        {
            snapshot = null;
            error = string.Empty;
            if (table == null || !table.TryValidate(out error))
            {
                if (string.IsNullOrWhiteSpace(error))
                {
                    error = "Stage-result localization table is missing.";
                }

                return false;
            }

            var localeCopies = new StageResultLocaleSnapshot[table.LocaleCount];
            for (int localeIndex = 0; localeIndex < localeCopies.Length; localeIndex++)
            {
                StageResultLocaleDefinition locale = table.GetLocale(localeIndex);
                var entries = new StageResultLocalizedEntrySnapshot[locale.EntryCount];
                for (int entryIndex = 0; entryIndex < entries.Length; entryIndex++)
                {
                    StageResultLocalizedString entry = locale.GetEntry(entryIndex);
                    entries[entryIndex] = new StageResultLocalizedEntrySnapshot(
                        entry.Key,
                        entry.Value);
                }

                localeCopies[localeIndex] = new StageResultLocaleSnapshot(
                    locale.LocaleId,
                    entries);
            }

            var candidate = new StageResultLocalizationSnapshot(table, localeCopies);
            if (!string.Equals(
                    candidate.CanonicalDigest,
                    table.ComputeCanonicalDigest(),
                    StringComparison.Ordinal))
            {
                error = "Deep-copied stage-result localization digest does not match its source.";
                return false;
            }

            snapshot = candidate;
            error = string.Empty;
            return true;
        }

        internal bool TryValidateIntegrity(out string error)
        {
            error = string.Empty;
            if (SchemaVersion != 1
                || TableRevision < 1
                || string.IsNullOrWhiteSpace(TableId)
                || locales.Length == 0
                || !string.Equals(
                    CanonicalDigest,
                    RecomputeCanonicalDigest(),
                    StringComparison.Ordinal))
            {
                error = "Deep-copied stage-result localization is damaged.";
                return false;
            }

            error = string.Empty;
            return true;
        }

        internal bool TryResolveLocale(
            string requestedLocaleId,
            out string resolvedLocaleId,
            out string error)
        {
            StageResultLocaleSnapshot locale = FindLocaleExact(requestedLocaleId)
                ?? FindLocaleByLanguage(requestedLocaleId)
                ?? FindLocaleExact(DefaultLocaleId);
            if (locale == null)
            {
                resolvedLocaleId = string.Empty;
                error = "Stage-result localization snapshot has no resolvable locale.";
                return false;
            }

            resolvedLocaleId = locale.LocaleId;
            error = string.Empty;
            return true;
        }

        internal bool TryResolve(string localeId, string key, out string value)
        {
            StageResultLocaleSnapshot locale = FindLocaleExact(localeId);
            if (locale != null && locale.TryResolve(key, out value))
            {
                return true;
            }

            value = string.Empty;
            return false;
        }

        private string RecomputeCanonicalDigest()
        {
            StringBuilder builder = new(4096);
            StageCanonicalDigest.Append(builder, "localization.schemaVersion", SchemaVersion);
            StageCanonicalDigest.Append(builder, "localization.tableId", TableId);
            StageCanonicalDigest.Append(builder, "localization.tableRevision", TableRevision);
            StageCanonicalDigest.Append(builder, "localization.defaultLocaleId", DefaultLocaleId);
            StageCanonicalDigest.Append(builder, "localization.localeCount", locales.Length);
            for (int localeIndex = 0; localeIndex < locales.Length; localeIndex++)
            {
                StageResultLocaleSnapshot locale = locales[localeIndex];
                string prefix = $"localization.locale[{localeIndex}]";
                StageCanonicalDigest.Append(builder, prefix + ".id", locale?.LocaleId);
                StageCanonicalDigest.Append(builder, prefix + ".entryCount", locale?.EntryCount ?? 0);
                if (locale == null)
                {
                    continue;
                }

                for (int entryIndex = 0; entryIndex < locale.EntryCount; entryIndex++)
                {
                    StageResultLocalizedEntrySnapshot entry = locale.GetEntry(entryIndex);
                    string entryPrefix = $"{prefix}.entry[{entryIndex}]";
                    StageCanonicalDigest.Append(builder, entryPrefix + ".key", entry?.Key);
                    StageCanonicalDigest.Append(builder, entryPrefix + ".value", entry?.Value);
                }
            }

            return StageCanonicalDigest.Compute(builder.ToString());
        }

        private StageResultLocaleSnapshot FindLocaleExact(string localeId)
        {
            if (!string.IsNullOrWhiteSpace(localeId))
            {
                for (int i = 0; i < locales.Length; i++)
                {
                    StageResultLocaleSnapshot locale = locales[i];
                    if (locale != null
                        && string.Equals(
                            locale.LocaleId,
                            localeId,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        return locale;
                    }
                }
            }

            return null;
        }

        private StageResultLocaleSnapshot FindLocaleByLanguage(string localeId)
        {
            if (string.IsNullOrWhiteSpace(localeId))
            {
                return null;
            }

            string language = GetLanguage(localeId);
            for (int i = 0; i < locales.Length; i++)
            {
                StageResultLocaleSnapshot locale = locales[i];
                if (locale != null
                    && string.Equals(
                        GetLanguage(locale.LocaleId),
                        language,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return locale;
                }
            }

            return null;
        }

        private static string GetLanguage(string localeId)
        {
            int separator = localeId.IndexOfAny(new[] { '-', '_' });
            return separator > 0 ? localeId.Substring(0, separator) : localeId;
        }
    }

    public sealed class StageResultPresentationSourceSnapshot
    {
        private readonly StageResultActionPresentationMappingSnapshot[] actionMappings;

        internal StageResultPresentationSourceSnapshot(
            string resultDefinitionId,
            int presentationBindingRevision,
            int supportedRunSchemaVersion,
            StageResultLocaleResolutionPolicy localeResolutionPolicy,
            StageResultPresentationProfileSnapshot profile,
            StageResultLocalizationSnapshot localization,
            StageResultActionPresentationMappingSnapshot[] actionMappings)
        {
            SchemaVersion = 1;
            SourceKind = StageResultPresentationSourceKind
                .DeepCopiedProfileLocalizationAndMappingsAtAdmission;
            ResultDefinitionId = resultDefinitionId ?? string.Empty;
            PresentationBindingRevision = presentationBindingRevision;
            SupportedRunSchemaVersion = supportedRunSchemaVersion;
            LocaleResolutionPolicy = localeResolutionPolicy;
            Profile = profile;
            Localization = localization;
            this.actionMappings = actionMappings
                ?? Array.Empty<StageResultActionPresentationMappingSnapshot>();
            PresentationBindingDigest = RecomputePresentationBindingDigest();
            CanonicalDigest = RecomputeCanonicalDigest();
        }

        public int SchemaVersion { get; }
        public StageResultPresentationSourceKind SourceKind { get; }
        public string ResultDefinitionId { get; }
        public int PresentationBindingRevision { get; }
        public int SupportedRunSchemaVersion { get; }
        public StageResultLocaleResolutionPolicy LocaleResolutionPolicy { get; }
        public StageResultPresentationProfileSnapshot Profile { get; }
        public StageResultLocalizationSnapshot Localization { get; }
        public int ActionMappingCount => actionMappings.Length;
        public string PresentationBindingDigest { get; }
        public string CanonicalDigest { get; }

        public StageResultActionPresentationMappingSnapshot GetActionMapping(int index)
        {
            if (index < 0 || index >= actionMappings.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return actionMappings[index];
        }

        internal bool TryValidateIntegrity(out string error)
        {
            error = string.Empty;
            if (SchemaVersion != 1
                || SourceKind != StageResultPresentationSourceKind
                    .DeepCopiedProfileLocalizationAndMappingsAtAdmission
                || PresentationBindingRevision < 1
                || SupportedRunSchemaVersion != 1
                || LocaleResolutionPolicy
                    != StageResultLocaleResolutionPolicy
                        .ExactThenLanguageThenDefaultOrdinalIgnoreCase
                || string.IsNullOrWhiteSpace(ResultDefinitionId)
                || Profile == null
                || Localization == null
                || !Profile.TryValidateIntegrity(out error)
                || !Localization.TryValidateIntegrity(out error)
                || !string.Equals(
                    PresentationBindingDigest,
                    RecomputePresentationBindingDigest(),
                    StringComparison.Ordinal)
                || !string.Equals(
                    CanonicalDigest,
                    RecomputeCanonicalDigest(),
                    StringComparison.Ordinal))
            {
                if (string.IsNullOrWhiteSpace(error))
                {
                    error = "Deep-copied stage-result presentation source is damaged.";
                }

                return false;
            }

            error = string.Empty;
            return true;
        }

        public bool TryCreatePresentation(
            StageRunResultSummary summary,
            string requestedLocaleId,
            out StageResultPresentationSnapshot snapshot,
            out string error)
        {
            snapshot = null;
            if (!TryValidateIntegrity(out error))
            {
                return false;
            }

            if (summary == null
                || summary.Identity == null
                || summary.OutcomeFact == null
                || summary.Identity.SchemaVersion != SupportedRunSchemaVersion
                || !string.Equals(
                    summary.Identity.PlayableStageId,
                    Profile.PlayableStageId,
                    StringComparison.Ordinal)
                || (summary.Outcome != StageRouteOutcome.Clear
                    && summary.Outcome != StageRouteOutcome.Fail))
            {
                error = "Committed result does not match the copied presentation source.";
                return false;
            }

            if (!TryResolveActionMappings(
                    summary,
                    out StageResultActionPresentationMappingSnapshot primaryMapping,
                    out StageResultActionPresentationMappingSnapshot secondaryMapping,
                    out error)
                || !Localization.TryResolveLocale(
                    requestedLocaleId,
                    out string localeId,
                    out error))
            {
                return false;
            }

            string statusKey = summary.Outcome == StageRouteOutcome.Clear
                ? Profile.ClearStatusKey
                : Profile.FailStatusKey;
            if (!TryResolve(localeId, Profile.TitleFormatKey, out string titleFormat, out error)
                || !TryResolve(localeId, Profile.StageNameKey, out string stageName, out error)
                || !TryResolve(localeId, statusKey, out string status, out error)
                || !TryResolve(
                    localeId,
                    Profile.TotalActiveTimeLabelKey,
                    out string totalTimeLabel,
                    out error)
                || !TryResolve(
                    localeId,
                    Profile.CombatActiveTimeLabelKey,
                    out string combatTimeLabel,
                    out error)
                || !TryResolve(
                    localeId,
                    Profile.RecordsCategoryKey,
                    out string recordsCategory,
                    out error)
                || !TryResolve(
                    localeId,
                    primaryMapping.LabelKey,
                    out string primaryAction,
                    out error)
                || !TryResolve(
                    localeId,
                    secondaryMapping.LabelKey,
                    out string secondaryAction,
                    out error))
            {
                return false;
            }

            string stageTitle;
            try
            {
                stageTitle = string.Format(
                    CultureInfo.InvariantCulture,
                    titleFormat,
                    stageName,
                    status);
            }
            catch (FormatException)
            {
                error = "Stage-result localized title format is invalid.";
                return false;
            }

            var rows = new List<StageResultPresentationRowSnapshot>(Profile.ProofRowLimit);
            for (int i = 0; i < Profile.ProofRuleCount && rows.Count < Profile.ProofRowLimit; i++)
            {
                StageResultProofPresentationRuleSnapshot rule = Profile.GetProofRule(i);
                if (!summary.TryGetSemanticProof(rule.ProofId, out StageRunSemanticProofFact proof)
                    || (rule.RequireQualified && !proof.Qualified))
                {
                    continue;
                }

                if (!TryResolve(localeId, rule.TextKey, out string format, out error)
                    || !TryFormatProofRow(rule, proof, format, out string text, out error))
                {
                    return false;
                }

                rows.Add(new StageResultPresentationRowSnapshot(
                    proof.ProofId,
                    proof.CanonicalDigest,
                    text));
            }

            snapshot = new StageResultPresentationSnapshot(
                summary,
                Profile.ProfileId,
                Profile.ProfileRevision,
                Profile.CanonicalDigest,
                Localization.TableId,
                Localization.TableRevision,
                Localization.CanonicalDigest,
                localeId,
                Profile.StageCode,
                stageTitle,
                totalTimeLabel,
                combatTimeLabel,
                recordsCategory,
                primaryAction,
                secondaryAction,
                summary.Outcome == StageRouteOutcome.Clear
                    ? Profile.ClearTitleColor
                    : Profile.FailTitleColor,
                rows.ToArray());
            error = string.Empty;
            return true;
        }

        internal string RecomputePresentationBindingDigest()
        {
            StringBuilder builder = new(4096);
            StageCanonicalDigest.Append(builder, "resultPresentation.schemaVersion", 1);
            StageCanonicalDigest.Append(
                builder,
                "resultPresentation.resultDefinitionId",
                ResultDefinitionId);
            StageCanonicalDigest.Append(
                builder,
                "resultPresentation.presentationBindingRevision",
                PresentationBindingRevision);
            StageCanonicalDigest.Append(
                builder,
                "resultPresentation.playableStageId",
                Profile?.PlayableStageId);
            StageCanonicalDigest.Append(
                builder,
                "resultPresentation.supportedRunSchemaVersion",
                SupportedRunSchemaVersion);
            StageCanonicalDigest.Append(
                builder,
                "resultPresentation.profileSchemaVersion",
                Profile?.SchemaVersion ?? 0);
            StageCanonicalDigest.Append(builder, "resultPresentation.profileId", Profile?.ProfileId);
            StageCanonicalDigest.Append(
                builder,
                "resultPresentation.profileRevision",
                Profile?.ProfileRevision ?? 0);
            StageCanonicalDigest.Append(
                builder,
                "resultPresentation.profileDigest",
                Profile?.CanonicalDigest);
            StageCanonicalDigest.Append(
                builder,
                "resultPresentation.localizationSchemaVersion",
                Localization?.SchemaVersion ?? 0);
            StageCanonicalDigest.Append(
                builder,
                "resultPresentation.localizationTableId",
                Localization?.TableId);
            StageCanonicalDigest.Append(
                builder,
                "resultPresentation.localizationTableRevision",
                Localization?.TableRevision ?? 0);
            StageCanonicalDigest.Append(
                builder,
                "resultPresentation.localizationDigest",
                Localization?.CanonicalDigest);
            StageCanonicalDigest.Append(
                builder,
                "resultPresentation.localeResolutionPolicy",
                (int)LocaleResolutionPolicy);
            StageCanonicalDigest.Append(
                builder,
                "resultPresentation.actionMappingCount",
                actionMappings.Length);
            for (int i = 0; i < actionMappings.Length; i++)
            {
                StageResultActionPresentationMappingSnapshot mapping = actionMappings[i];
                string prefix = $"resultPresentation.actionMapping[{i}]";
                StageCanonicalDigest.Append(builder, prefix + ".outcome", (int)mapping.Outcome);
                StageCanonicalDigest.Append(builder, prefix + ".actionId", mapping.ActionId);
                StageCanonicalDigest.Append(builder, prefix + ".labelKey", mapping.LabelKey);
                StageCanonicalDigest.Append(builder, prefix + ".role", (int)mapping.Role);
                StageCanonicalDigest.Append(builder, prefix + ".displayOrder", mapping.DisplayOrder);
            }

            return StageCanonicalDigest.Compute(builder.ToString());
        }

        private string RecomputeCanonicalDigest()
        {
            StringBuilder builder = new(2048);
            StageCanonicalDigest.Append(builder, "presentationSource.schemaVersion", SchemaVersion);
            StageCanonicalDigest.Append(builder, "presentationSource.sourceKind", (int)SourceKind);
            StageCanonicalDigest.Append(
                builder,
                "presentationSource.resultDefinitionId",
                ResultDefinitionId);
            StageCanonicalDigest.Append(
                builder,
                "presentationSource.presentationBindingRevision",
                PresentationBindingRevision);
            StageCanonicalDigest.Append(
                builder,
                "presentationSource.supportedRunSchemaVersion",
                SupportedRunSchemaVersion);
            StageCanonicalDigest.Append(
                builder,
                "presentationSource.profileSchemaVersion",
                Profile?.SchemaVersion ?? 0);
            StageCanonicalDigest.Append(builder, "presentationSource.profileId", Profile?.ProfileId);
            StageCanonicalDigest.Append(
                builder,
                "presentationSource.profileRevision",
                Profile?.ProfileRevision ?? 0);
            StageCanonicalDigest.Append(
                builder,
                "presentationSource.profileDigest",
                Profile?.CanonicalDigest);
            StageCanonicalDigest.Append(
                builder,
                "presentationSource.localizationSchemaVersion",
                Localization?.SchemaVersion ?? 0);
            StageCanonicalDigest.Append(
                builder,
                "presentationSource.localizationTableId",
                Localization?.TableId);
            StageCanonicalDigest.Append(
                builder,
                "presentationSource.localizationTableRevision",
                Localization?.TableRevision ?? 0);
            StageCanonicalDigest.Append(
                builder,
                "presentationSource.localizationDigest",
                Localization?.CanonicalDigest);
            StageCanonicalDigest.Append(
                builder,
                "presentationSource.localeResolutionPolicy",
                (int)LocaleResolutionPolicy);
            StageCanonicalDigest.Append(
                builder,
                "presentationSource.actionPresentationBindingDigest",
                PresentationBindingDigest);
            return StageCanonicalDigest.Compute(builder.ToString());
        }

        private bool TryResolveActionMappings(
            StageRunResultSummary summary,
            out StageResultActionPresentationMappingSnapshot primary,
            out StageResultActionPresentationMappingSnapshot secondary,
            out string error)
        {
            primary = null;
            secondary = null;
            int matchedActionCount = 0;
            for (int i = 0; i < actionMappings.Length; i++)
            {
                StageResultActionPresentationMappingSnapshot mapping = actionMappings[i];
                if (mapping == null || mapping.Outcome != summary.Outcome)
                {
                    continue;
                }

                if (!TryFindOfferedAction(summary, mapping.ActionId))
                {
                    error = $"Presentation mapping cannot create unoffered action {mapping.ActionId}.";
                    return false;
                }

                matchedActionCount++;
                if (mapping.Role == StageResultActionPresentationRole.Primary && primary == null)
                {
                    primary = mapping;
                }
                else if (mapping.Role == StageResultActionPresentationRole.Secondary && secondary == null)
                {
                    secondary = mapping;
                }
                else
                {
                    error = "Stage-result action presentation roles are duplicated or unsupported.";
                    return false;
                }
            }

            if (primary == null
                || secondary == null
                || matchedActionCount != summary.OfferedActionCount)
            {
                error = "Committed result actions do not have an exact presentation mapping.";
                return false;
            }

            string expectedPrimaryKey = summary.Outcome == StageRouteOutcome.Clear
                ? Profile.ReplayActionKey
                : Profile.RetryActionKey;
            if (!string.Equals(primary.LabelKey, expectedPrimaryKey, StringComparison.Ordinal)
                || !string.Equals(
                    secondary.LabelKey,
                    Profile.LobbyActionKey,
                    StringComparison.Ordinal))
            {
                error = "Stage-result action mapping label does not match the copied profile key.";
                return false;
            }

            error = string.Empty;
            return true;
        }

        private bool TryResolve(
            string localeId,
            string key,
            out string value,
            out string error)
        {
            if (!Localization.TryResolve(localeId, key, out value))
            {
                error = $"Stage-result localization {localeId} is missing key {key}.";
                return false;
            }

            error = string.Empty;
            return true;
        }

        private static bool TryFindOfferedAction(
            StageRunResultSummary summary,
            string actionId)
        {
            for (int i = 0; i < summary.OfferedActionCount; i++)
            {
                StageRunActionSnapshot action = summary.GetOfferedAction(i);
                if (action != null
                    && string.Equals(action.ActionId, actionId, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool TryFormatProofRow(
            StageResultProofPresentationRuleSnapshot rule,
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
                            StageResultPresentationProfile.FormatDuration(milliseconds));
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
