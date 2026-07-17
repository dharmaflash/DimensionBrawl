using System;
using System.Collections.Generic;
using UnityEngine;

namespace DimensionBrawl.UI.LobbyOperationsReview
{
    public enum LobbyOperationsReviewEntryKind
    {
        None = 0,
        Notice = 10,
        Mailbox = 20,
        Missions = 30,
        EventCalendar = 40
    }

    public enum LobbyOperationsReviewProductionDisposition
    {
        None = 0,
        LocalReviewFixture = 10,
        ReviewShellNoProductCommitment = 20,
        DefinitionOnlyReviewShell = 30
    }

    public enum LobbyOperationsReviewServiceDisposition
    {
        None = 0,
        NotRequiredForReview = 10,
        NoVerifiedSource = 20
    }

    public enum LobbyOperationsReviewAccountDisposition
    {
        None = 0,
        NotRequiredForReview = 10,
        NoVerifiedSource = 20
    }

    public enum LobbyOperationsReviewServerClockDisposition
    {
        None = 0,
        NotRequiredForReview = 10,
        NoVerifiedSource = 20
    }

    public enum LobbyOperationsReviewScheduleDisposition
    {
        None = 0,
        NotRequiredForReview = 10,
        NoVerifiedSource = 20,
        DefinitionOnlyNoVerdict = 30
    }

    public enum LobbyOperationsReviewProgressDisposition
    {
        None = 0,
        NotRequiredForReview = 10,
        NoVerifiedSource = 20
    }

    public enum LobbyOperationsReviewAttentionDisposition
    {
        None = 0,
        NotRequiredForReview = 10,
        NoVerifiedSource = 20
    }

    public enum LobbyOperationsReviewActionDisposition
    {
        None = 0,
        LocalReviewConfirm = 10,
        ExplanationOnly = 20
    }

    [CreateAssetMenu(menuName = "DimensionBrawl/UI/Review/Lobby Operations Review Profile")]
    public sealed class LobbyOperationsReviewProfile : ScriptableObject
    {
        public const int RequiredEntryCount = 4;
        public const string NoticeEntryId = "review.operations.notice";
        public const string MailboxEntryId = "review.operations.mailbox";
        public const string MissionsEntryId = "review.operations.missions";
        public const string EventCalendarEntryId = "review.operations.event-calendar";

        [Serializable]
        public sealed class EntryDefinition
        {
            [SerializeField] private string entryId = string.Empty;
            [SerializeField] private LobbyOperationsReviewEntryKind kind;
            [SerializeField] private string titleLocalizationKey = string.Empty;
            [SerializeField, TextArea(1, 2)] private string titleFallback = string.Empty;
            [SerializeField] private string explanationLocalizationKey = string.Empty;
            [SerializeField, TextArea(2, 5)] private string explanationFallback = string.Empty;
            [SerializeField] private LobbyOperationsReviewProductionDisposition productionDisposition;
            [SerializeField] private LobbyOperationsReviewServiceDisposition serviceDisposition;
            [SerializeField] private LobbyOperationsReviewAccountDisposition accountDisposition;
            [SerializeField] private LobbyOperationsReviewServerClockDisposition serverClockDisposition;
            [SerializeField] private LobbyOperationsReviewScheduleDisposition scheduleDisposition;
            [SerializeField] private LobbyOperationsReviewProgressDisposition progressDisposition;
            [SerializeField] private LobbyOperationsReviewAttentionDisposition attentionDisposition;
            [SerializeField] private LobbyOperationsReviewActionDisposition actionDisposition;

            public EntryDefinition()
            {
            }

            public EntryDefinition(
                string entryId,
                LobbyOperationsReviewEntryKind kind,
                string titleLocalizationKey,
                string titleFallback,
                string explanationLocalizationKey,
                string explanationFallback,
                LobbyOperationsReviewProductionDisposition productionDisposition,
                LobbyOperationsReviewServiceDisposition serviceDisposition,
                LobbyOperationsReviewAccountDisposition accountDisposition,
                LobbyOperationsReviewServerClockDisposition serverClockDisposition,
                LobbyOperationsReviewScheduleDisposition scheduleDisposition,
                LobbyOperationsReviewProgressDisposition progressDisposition,
                LobbyOperationsReviewAttentionDisposition attentionDisposition,
                LobbyOperationsReviewActionDisposition actionDisposition)
            {
                Configure(
                    entryId,
                    kind,
                    titleLocalizationKey,
                    titleFallback,
                    explanationLocalizationKey,
                    explanationFallback,
                    productionDisposition,
                    serviceDisposition,
                    accountDisposition,
                    serverClockDisposition,
                    scheduleDisposition,
                    progressDisposition,
                    attentionDisposition,
                    actionDisposition);
            }

            public string EntryId => entryId;
            public LobbyOperationsReviewEntryKind Kind => kind;
            public string TitleLocalizationKey => titleLocalizationKey;
            public string TitleFallback => titleFallback;
            public string ExplanationLocalizationKey => explanationLocalizationKey;
            public string ExplanationFallback => explanationFallback;
            public LobbyOperationsReviewProductionDisposition ProductionDisposition =>
                productionDisposition;
            public LobbyOperationsReviewServiceDisposition ServiceDisposition =>
                serviceDisposition;
            public LobbyOperationsReviewAccountDisposition AccountDisposition =>
                accountDisposition;
            public LobbyOperationsReviewServerClockDisposition ServerClockDisposition =>
                serverClockDisposition;
            public LobbyOperationsReviewScheduleDisposition ScheduleDisposition =>
                scheduleDisposition;
            public LobbyOperationsReviewProgressDisposition ProgressDisposition =>
                progressDisposition;
            public LobbyOperationsReviewAttentionDisposition AttentionDisposition =>
                attentionDisposition;
            public LobbyOperationsReviewActionDisposition ActionDisposition =>
                actionDisposition;

            public void Configure(
                string newEntryId,
                LobbyOperationsReviewEntryKind newKind,
                string newTitleLocalizationKey,
                string newTitleFallback,
                string newExplanationLocalizationKey,
                string newExplanationFallback,
                LobbyOperationsReviewProductionDisposition newProductionDisposition,
                LobbyOperationsReviewServiceDisposition newServiceDisposition,
                LobbyOperationsReviewAccountDisposition newAccountDisposition,
                LobbyOperationsReviewServerClockDisposition newServerClockDisposition,
                LobbyOperationsReviewScheduleDisposition newScheduleDisposition,
                LobbyOperationsReviewProgressDisposition newProgressDisposition,
                LobbyOperationsReviewAttentionDisposition newAttentionDisposition,
                LobbyOperationsReviewActionDisposition newActionDisposition)
            {
                entryId = newEntryId ?? string.Empty;
                kind = newKind;
                titleLocalizationKey = newTitleLocalizationKey ?? string.Empty;
                titleFallback = newTitleFallback ?? string.Empty;
                explanationLocalizationKey = newExplanationLocalizationKey ?? string.Empty;
                explanationFallback = newExplanationFallback ?? string.Empty;
                productionDisposition = newProductionDisposition;
                serviceDisposition = newServiceDisposition;
                accountDisposition = newAccountDisposition;
                serverClockDisposition = newServerClockDisposition;
                scheduleDisposition = newScheduleDisposition;
                progressDisposition = newProgressDisposition;
                attentionDisposition = newAttentionDisposition;
                actionDisposition = newActionDisposition;
            }

            internal EntryDefinition DeepCopy()
            {
                return new EntryDefinition(
                    EntryId,
                    Kind,
                    TitleLocalizationKey,
                    TitleFallback,
                    ExplanationLocalizationKey,
                    ExplanationFallback,
                    ProductionDisposition,
                    ServiceDisposition,
                    AccountDisposition,
                    ServerClockDisposition,
                    ScheduleDisposition,
                    ProgressDisposition,
                    AttentionDisposition,
                    ActionDisposition);
            }
        }

        [SerializeField] private EntryDefinition[] entries = Array.Empty<EntryDefinition>();

        public int EntryCount => entries?.Length ?? 0;
        public EntryDefinition[] Entries => CreateEntrySnapshot();

        public void Configure(EntryDefinition[] newEntries)
        {
            entries = CloneEntries(newEntries);
        }

        public EntryDefinition GetEntry(int index)
        {
            if (index < 0 || index >= EntryCount)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return entries[index]?.DeepCopy();
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

            string profileLabel = string.IsNullOrWhiteSpace(name)
                ? nameof(LobbyOperationsReviewProfile)
                : name;
            EntryDefinition[] resolvedEntries = entries ?? Array.Empty<EntryDefinition>();
            var entryIds = new HashSet<string>(StringComparer.Ordinal);
            var kinds = new HashSet<LobbyOperationsReviewEntryKind>();

            if (resolvedEntries.Length != RequiredEntryCount)
            {
                issues.Add(
                    $"{profileLabel}: exactly {RequiredEntryCount} entries are required; found {resolvedEntries.Length}.");
            }

            for (int index = 0; index < resolvedEntries.Length; index++)
            {
                EntryDefinition entry = resolvedEntries[index];
                if (entry == null)
                {
                    issues.Add($"{profileLabel}: entry {index} is null.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(entry.EntryId))
                {
                    issues.Add($"{profileLabel}: entry {index} has no stable entry id.");
                }
                else if (!entryIds.Add(entry.EntryId))
                {
                    issues.Add($"{profileLabel}: duplicate entry id '{entry.EntryId}'.");
                }

                if (!Enum.IsDefined(typeof(LobbyOperationsReviewEntryKind), entry.Kind)
                    || entry.Kind == LobbyOperationsReviewEntryKind.None)
                {
                    issues.Add(
                        $"{profileLabel}: entry '{entry.EntryId}' has unsupported kind '{entry.Kind}'.");
                }
                else if (!kinds.Add(entry.Kind))
                {
                    issues.Add($"{profileLabel}: duplicate entry kind '{entry.Kind}'.");
                }

                if (string.IsNullOrWhiteSpace(entry.TitleFallback))
                {
                    issues.Add(
                        $"{profileLabel}: entry '{entry.EntryId}' has no fallback title for local review.");
                }

                if (string.IsNullOrWhiteSpace(entry.ExplanationFallback))
                {
                    issues.Add(
                        $"{profileLabel}: entry '{entry.EntryId}' has no fallback explanation for local review.");
                }

                ValidateDefinedDisposition(
                    profileLabel,
                    entry.EntryId,
                    nameof(entry.ProductionDisposition),
                    entry.ProductionDisposition,
                    LobbyOperationsReviewProductionDisposition.None,
                    issues);
                ValidateDefinedDisposition(
                    profileLabel,
                    entry.EntryId,
                    nameof(entry.ServiceDisposition),
                    entry.ServiceDisposition,
                    LobbyOperationsReviewServiceDisposition.None,
                    issues);
                ValidateDefinedDisposition(
                    profileLabel,
                    entry.EntryId,
                    nameof(entry.AccountDisposition),
                    entry.AccountDisposition,
                    LobbyOperationsReviewAccountDisposition.None,
                    issues);
                ValidateDefinedDisposition(
                    profileLabel,
                    entry.EntryId,
                    nameof(entry.ServerClockDisposition),
                    entry.ServerClockDisposition,
                    LobbyOperationsReviewServerClockDisposition.None,
                    issues);
                ValidateDefinedDisposition(
                    profileLabel,
                    entry.EntryId,
                    nameof(entry.ScheduleDisposition),
                    entry.ScheduleDisposition,
                    LobbyOperationsReviewScheduleDisposition.None,
                    issues);
                ValidateDefinedDisposition(
                    profileLabel,
                    entry.EntryId,
                    nameof(entry.ProgressDisposition),
                    entry.ProgressDisposition,
                    LobbyOperationsReviewProgressDisposition.None,
                    issues);
                ValidateDefinedDisposition(
                    profileLabel,
                    entry.EntryId,
                    nameof(entry.AttentionDisposition),
                    entry.AttentionDisposition,
                    LobbyOperationsReviewAttentionDisposition.None,
                    issues);
                ValidateDefinedDisposition(
                    profileLabel,
                    entry.EntryId,
                    nameof(entry.ActionDisposition),
                    entry.ActionDisposition,
                    LobbyOperationsReviewActionDisposition.None,
                    issues);

                ValidateExactContract(profileLabel, index, entry, issues);
            }
        }

        public static EntryDefinition[] CreateDefaultEntries()
        {
            return new[]
            {
                new EntryDefinition(
                    NoticeEntryId,
                    LobbyOperationsReviewEntryKind.Notice,
                    "ui.review.operations.notice.title",
                    "운영 안내 샘플",
                    "ui.review.operations.notice.explanation",
                    "로컬 UI 검토용 안내 샘플입니다. 실제 공지나 서비스 응답을 나타내지 않습니다.",
                    LobbyOperationsReviewProductionDisposition.LocalReviewFixture,
                    LobbyOperationsReviewServiceDisposition.NotRequiredForReview,
                    LobbyOperationsReviewAccountDisposition.NotRequiredForReview,
                    LobbyOperationsReviewServerClockDisposition.NotRequiredForReview,
                    LobbyOperationsReviewScheduleDisposition.NotRequiredForReview,
                    LobbyOperationsReviewProgressDisposition.NotRequiredForReview,
                    LobbyOperationsReviewAttentionDisposition.NotRequiredForReview,
                    LobbyOperationsReviewActionDisposition.LocalReviewConfirm),
                new EntryDefinition(
                    MailboxEntryId,
                    LobbyOperationsReviewEntryKind.Mailbox,
                    "ui.review.operations.mailbox.title",
                    "우편함",
                    "ui.review.operations.mailbox.explanation",
                    "서비스와 계정 데이터 소스가 연결되지 않은 설명 전용 검토 항목입니다.",
                    LobbyOperationsReviewProductionDisposition.ReviewShellNoProductCommitment,
                    LobbyOperationsReviewServiceDisposition.NoVerifiedSource,
                    LobbyOperationsReviewAccountDisposition.NoVerifiedSource,
                    LobbyOperationsReviewServerClockDisposition.NotRequiredForReview,
                    LobbyOperationsReviewScheduleDisposition.NotRequiredForReview,
                    LobbyOperationsReviewProgressDisposition.NotRequiredForReview,
                    LobbyOperationsReviewAttentionDisposition.NoVerifiedSource,
                    LobbyOperationsReviewActionDisposition.ExplanationOnly),
                new EntryDefinition(
                    MissionsEntryId,
                    LobbyOperationsReviewEntryKind.Missions,
                    "ui.review.operations.missions.title",
                    "미션",
                    "ui.review.operations.missions.explanation",
                    "계정 진행 데이터 소스가 연결되지 않아 진행 상태를 추론하지 않는 설명 전용 검토 항목입니다.",
                    LobbyOperationsReviewProductionDisposition.ReviewShellNoProductCommitment,
                    LobbyOperationsReviewServiceDisposition.NotRequiredForReview,
                    LobbyOperationsReviewAccountDisposition.NoVerifiedSource,
                    LobbyOperationsReviewServerClockDisposition.NotRequiredForReview,
                    LobbyOperationsReviewScheduleDisposition.NotRequiredForReview,
                    LobbyOperationsReviewProgressDisposition.NoVerifiedSource,
                    LobbyOperationsReviewAttentionDisposition.NoVerifiedSource,
                    LobbyOperationsReviewActionDisposition.ExplanationOnly),
                new EntryDefinition(
                    EventCalendarEntryId,
                    LobbyOperationsReviewEntryKind.EventCalendar,
                    "ui.review.operations.event_calendar.title",
                    "이벤트 일정",
                    "ui.review.operations.event_calendar.explanation",
                    "서버 시각과 일정 판정 데이터 소스가 연결되지 않은 정의 전용 검토 항목입니다.",
                    LobbyOperationsReviewProductionDisposition.DefinitionOnlyReviewShell,
                    LobbyOperationsReviewServiceDisposition.NoVerifiedSource,
                    LobbyOperationsReviewAccountDisposition.NotRequiredForReview,
                    LobbyOperationsReviewServerClockDisposition.NoVerifiedSource,
                    LobbyOperationsReviewScheduleDisposition.DefinitionOnlyNoVerdict,
                    LobbyOperationsReviewProgressDisposition.NotRequiredForReview,
                    LobbyOperationsReviewAttentionDisposition.NoVerifiedSource,
                    LobbyOperationsReviewActionDisposition.ExplanationOnly)
            };
        }

        internal EntryDefinition[] CreateEntrySnapshot()
        {
            return CloneEntries(entries);
        }

        private static void ValidateExactContract(
            string profileLabel,
            int index,
            EntryDefinition entry,
            List<string> issues)
        {
            EntryDefinition[] expectedEntries = CreateDefaultEntries();
            if (index >= expectedEntries.Length)
            {
                issues.Add(
                    $"{profileLabel}: entry {index} is outside the required operations review order.");
                return;
            }

            EntryDefinition expected = expectedEntries[index];
            if (entry.Kind != expected.Kind ||
                !string.Equals(entry.EntryId, expected.EntryId, StringComparison.Ordinal))
            {
                issues.Add(
                    $"{profileLabel}: entry {index} must be '{expected.EntryId}' with kind '{expected.Kind}' in the required order.");
            }

            RequireExactText(
                profileLabel,
                entry,
                nameof(entry.TitleLocalizationKey),
                entry.TitleLocalizationKey,
                expected.TitleLocalizationKey,
                issues);
            RequireExactText(
                profileLabel,
                entry,
                nameof(entry.TitleFallback),
                entry.TitleFallback,
                expected.TitleFallback,
                issues);
            RequireExactText(
                profileLabel,
                entry,
                nameof(entry.ExplanationLocalizationKey),
                entry.ExplanationLocalizationKey,
                expected.ExplanationLocalizationKey,
                issues);
            RequireExactText(
                profileLabel,
                entry,
                nameof(entry.ExplanationFallback),
                entry.ExplanationFallback,
                expected.ExplanationFallback,
                issues);

            RequireExactDisposition(
                profileLabel,
                entry,
                nameof(entry.ProductionDisposition),
                entry.ProductionDisposition,
                expected.ProductionDisposition,
                issues);
            RequireExactDisposition(
                profileLabel,
                entry,
                nameof(entry.ServiceDisposition),
                entry.ServiceDisposition,
                expected.ServiceDisposition,
                issues);
            RequireExactDisposition(
                profileLabel,
                entry,
                nameof(entry.AccountDisposition),
                entry.AccountDisposition,
                expected.AccountDisposition,
                issues);
            RequireExactDisposition(
                profileLabel,
                entry,
                nameof(entry.ServerClockDisposition),
                entry.ServerClockDisposition,
                expected.ServerClockDisposition,
                issues);
            RequireExactDisposition(
                profileLabel,
                entry,
                nameof(entry.ScheduleDisposition),
                entry.ScheduleDisposition,
                expected.ScheduleDisposition,
                issues);
            RequireExactDisposition(
                profileLabel,
                entry,
                nameof(entry.ProgressDisposition),
                entry.ProgressDisposition,
                expected.ProgressDisposition,
                issues);
            RequireExactDisposition(
                profileLabel,
                entry,
                nameof(entry.AttentionDisposition),
                entry.AttentionDisposition,
                expected.AttentionDisposition,
                issues);
            RequireExactDisposition(
                profileLabel,
                entry,
                nameof(entry.ActionDisposition),
                entry.ActionDisposition,
                expected.ActionDisposition,
                issues);
        }

        private static void RequireExactText(
            string profileLabel,
            EntryDefinition entry,
            string fieldName,
            string actual,
            string expected,
            List<string> issues)
        {
            if (!string.Equals(actual, expected, StringComparison.Ordinal))
            {
                issues.Add(
                    $"{profileLabel}: entry '{entry.EntryId}' {fieldName} must match the authored review contract.");
            }
        }

        private static void RequireExactDisposition<T>(
            string profileLabel,
            EntryDefinition entry,
            string dispositionName,
            T actual,
            T expected,
            List<string> issues)
            where T : struct
        {
            if (!EqualityComparer<T>.Default.Equals(actual, expected))
            {
                issues.Add(
                    $"{profileLabel}: entry '{entry.EntryId}' {dispositionName} must be '{expected}', not '{actual}'.");
            }
        }

        private static void ValidateDefinedDisposition<T>(
            string profileLabel,
            string entryId,
            string dispositionName,
            T value,
            T noneValue,
            List<string> issues)
            where T : struct, Enum
        {
            if (!Enum.IsDefined(typeof(T), value)
                || EqualityComparer<T>.Default.Equals(value, noneValue))
            {
                issues.Add(
                    $"{profileLabel}: entry '{entryId}' has unsupported {dispositionName} '{value}'.");
            }
        }

        private static EntryDefinition[] CloneEntries(EntryDefinition[] source)
        {
            if (source == null || source.Length == 0)
            {
                return Array.Empty<EntryDefinition>();
            }

            var clone = new EntryDefinition[source.Length];
            for (int index = 0; index < source.Length; index++)
            {
                clone[index] = source[index]?.DeepCopy();
            }

            return clone;
        }

        private void OnValidate()
        {
            entries ??= Array.Empty<EntryDefinition>();
        }
    }
}
