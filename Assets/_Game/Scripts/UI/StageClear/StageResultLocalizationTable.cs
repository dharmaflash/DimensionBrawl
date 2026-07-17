using System;
using System.Collections.Generic;
using System.Text;
using DimensionBrawl.LevelDesign;
using UnityEngine;

namespace DimensionBrawl.UI.StageClear
{
    [Serializable]
    public sealed class StageResultLocalizedString
    {
        [SerializeField] private string key;
        [SerializeField, TextArea] private string value;

        public string Key => key;
        public string Value => value;
    }

    [Serializable]
    public sealed class StageResultLocaleDefinition
    {
        [SerializeField] private string localeId;
        [SerializeField] private StageResultLocalizedString[] entries = Array.Empty<StageResultLocalizedString>();

        public string LocaleId => localeId;
        public int EntryCount => entries != null ? entries.Length : 0;

        public StageResultLocalizedString GetEntry(int index)
        {
            if (entries == null || index < 0 || index >= entries.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return entries[index];
        }

        internal bool TryResolve(string key, out string value)
        {
            if (!string.IsNullOrWhiteSpace(key) && entries != null)
            {
                for (int i = 0; i < entries.Length; i++)
                {
                    StageResultLocalizedString entry = entries[i];
                    if (entry != null && string.Equals(entry.Key, key, StringComparison.Ordinal))
                    {
                        value = entry.Value ?? string.Empty;
                        return true;
                    }
                }
            }

            value = string.Empty;
            return false;
        }
    }

    [CreateAssetMenu(
        menuName = "DimensionBrawl/UI/Stage Result Localization Table",
        fileName = "DB_StageResultLocalization")]
    public sealed class StageResultLocalizationTable : ScriptableObject
    {
        [SerializeField, Min(1)] private int schemaVersion = 1;
        [SerializeField] private string tableId;
        [SerializeField, Min(1)] private int tableRevision = 1;
        [SerializeField] private string defaultLocaleId = "ko-KR";
        [SerializeField] private StageResultLocaleDefinition[] locales = Array.Empty<StageResultLocaleDefinition>();

        public int SchemaVersion => schemaVersion;
        public string TableId => tableId;
        public int TableRevision => tableRevision;
        public string DefaultLocaleId => defaultLocaleId;
        public int LocaleCount => locales != null ? locales.Length : 0;

        public StageResultLocaleDefinition GetLocale(int index)
        {
            if (locales == null || index < 0 || index >= locales.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return locales[index];
        }

        public bool TryResolveLocale(
            string requestedLocaleId,
            out string resolvedLocaleId,
            out string error)
        {
            error = string.Empty;
            StageResultLocaleDefinition locale = FindLocaleExact(requestedLocaleId)
                ?? FindLocaleByLanguage(requestedLocaleId)
                ?? FindLocaleExact(defaultLocaleId);
            if (locale == null)
            {
                resolvedLocaleId = string.Empty;
                error = "Stage-result localization has no resolvable default locale.";
                return false;
            }

            resolvedLocaleId = locale.LocaleId;
            return true;
        }

        public bool TryResolve(string resolvedLocaleId, string key, out string value)
        {
            StageResultLocaleDefinition locale = FindLocaleExact(resolvedLocaleId);
            if (locale != null && locale.TryResolve(key, out value))
            {
                return true;
            }

            value = string.Empty;
            return false;
        }

        public bool TryValidate(out string error)
        {
            error = string.Empty;
            if (schemaVersion != 1)
            {
                error = "Stage-result localization schemaVersion must be 1.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(tableId) || tableRevision < 1)
            {
                error = "Stage-result localization requires a stable table ID and positive revision.";
                return false;
            }

            if (locales == null || locales.Length == 0)
            {
                error = "Stage-result localization requires at least one locale.";
                return false;
            }

            var localeIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            HashSet<string> canonicalKeys = null;
            string previousLocaleId = string.Empty;
            for (int localeIndex = 0; localeIndex < locales.Length; localeIndex++)
            {
                StageResultLocaleDefinition locale = locales[localeIndex];
                if (locale == null || string.IsNullOrWhiteSpace(locale.LocaleId))
                {
                    error = $"Stage-result localization locale {localeIndex} has no locale ID.";
                    return false;
                }

                if (!localeIds.Add(locale.LocaleId))
                {
                    error = $"Stage-result localization contains duplicate locale {locale.LocaleId}.";
                    return false;
                }

                if (localeIndex > 0
                    && string.Compare(previousLocaleId, locale.LocaleId, StringComparison.Ordinal) >= 0)
                {
                    error = "Stage-result localization locales must use ascending ordinal locale ID order.";
                    return false;
                }

                previousLocaleId = locale.LocaleId;
                if (locale.EntryCount == 0)
                {
                    error = $"Stage-result localization locale {locale.LocaleId} has no entries.";
                    return false;
                }

                var keys = new HashSet<string>(StringComparer.Ordinal);
                string previousKey = string.Empty;
                for (int entryIndex = 0; entryIndex < locale.EntryCount; entryIndex++)
                {
                    StageResultLocalizedString entry = locale.GetEntry(entryIndex);
                    if (entry == null
                        || string.IsNullOrWhiteSpace(entry.Key)
                        || string.IsNullOrWhiteSpace(entry.Value))
                    {
                        error = $"Stage-result localization {locale.LocaleId} entry {entryIndex} is incomplete.";
                        return false;
                    }

                    if (!keys.Add(entry.Key))
                    {
                        error = $"Stage-result localization {locale.LocaleId} duplicates key {entry.Key}.";
                        return false;
                    }

                    if (entryIndex > 0
                        && string.Compare(previousKey, entry.Key, StringComparison.Ordinal) >= 0)
                    {
                        error = $"Stage-result localization {locale.LocaleId} keys must use ascending ordinal order.";
                        return false;
                    }

                    previousKey = entry.Key;
                }

                if (canonicalKeys == null)
                {
                    canonicalKeys = keys;
                }
                else if (!canonicalKeys.SetEquals(keys))
                {
                    error = $"Stage-result localization locale {locale.LocaleId} does not have key parity.";
                    return false;
                }
            }

            if (!localeIds.Contains(defaultLocaleId))
            {
                error = "Stage-result localization default locale is not authored.";
                return false;
            }

            return true;
        }

        public string ComputeCanonicalDigest()
        {
            StringBuilder builder = new(4096);
            StageCanonicalDigest.Append(builder, "localization.schemaVersion", schemaVersion);
            StageCanonicalDigest.Append(builder, "localization.tableId", tableId);
            StageCanonicalDigest.Append(builder, "localization.tableRevision", tableRevision);
            StageCanonicalDigest.Append(builder, "localization.defaultLocaleId", defaultLocaleId);
            StageCanonicalDigest.Append(builder, "localization.localeCount", LocaleCount);
            for (int localeIndex = 0; localeIndex < LocaleCount; localeIndex++)
            {
                StageResultLocaleDefinition locale = locales[localeIndex];
                string prefix = $"localization.locale[{localeIndex}]";
                StageCanonicalDigest.Append(builder, prefix + ".id", locale?.LocaleId);
                StageCanonicalDigest.Append(builder, prefix + ".entryCount", locale?.EntryCount ?? 0);
                if (locale == null)
                {
                    continue;
                }

                for (int entryIndex = 0; entryIndex < locale.EntryCount; entryIndex++)
                {
                    StageResultLocalizedString entry = locale.GetEntry(entryIndex);
                    string entryPrefix = $"{prefix}.entry[{entryIndex}]";
                    StageCanonicalDigest.Append(builder, entryPrefix + ".key", entry?.Key);
                    StageCanonicalDigest.Append(builder, entryPrefix + ".value", entry?.Value);
                }
            }

            return StageCanonicalDigest.Compute(builder.ToString());
        }

        private StageResultLocaleDefinition FindLocaleExact(string localeId)
        {
            if (locales != null && !string.IsNullOrWhiteSpace(localeId))
            {
                for (int i = 0; i < locales.Length; i++)
                {
                    StageResultLocaleDefinition candidate = locales[i];
                    if (candidate != null
                        && string.Equals(candidate.LocaleId, localeId, StringComparison.OrdinalIgnoreCase))
                    {
                        return candidate;
                    }
                }
            }

            return null;
        }

        private StageResultLocaleDefinition FindLocaleByLanguage(string localeId)
        {
            if (locales == null || string.IsNullOrWhiteSpace(localeId))
            {
                return null;
            }

            int separator = localeId.IndexOfAny(new[] { '-', '_' });
            string language = separator > 0 ? localeId.Substring(0, separator) : localeId;
            for (int i = 0; i < locales.Length; i++)
            {
                StageResultLocaleDefinition candidate = locales[i];
                if (candidate == null || string.IsNullOrWhiteSpace(candidate.LocaleId))
                {
                    continue;
                }

                int candidateSeparator = candidate.LocaleId.IndexOfAny(new[] { '-', '_' });
                string candidateLanguage = candidateSeparator > 0
                    ? candidate.LocaleId.Substring(0, candidateSeparator)
                    : candidate.LocaleId;
                if (string.Equals(candidateLanguage, language, StringComparison.OrdinalIgnoreCase))
                {
                    return candidate;
                }
            }

            return null;
        }
    }
}
