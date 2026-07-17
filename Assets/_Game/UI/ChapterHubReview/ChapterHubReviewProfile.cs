using System;
using System.Collections.Generic;
using UnityEngine;

namespace DimensionBrawl.UI.ChapterHubReview
{
    public enum ChapterHubReviewContentStatus
    {
        None = 0,
        CanonicalPlayable = 10,
        InProduction = 20,
        Announced = 30
    }

    [CreateAssetMenu(
        menuName = "DimensionBrawl/UI/Review/Chapter Hub Review Profile",
        fileName = "DB_ChapterHubReview")]
    public sealed class ChapterHubReviewProfile : ScriptableObject
    {
        [Serializable]
        public sealed class ChapterDefinition
        {
            [SerializeField] private string chapterId = string.Empty;
            [SerializeField] private string episodeCode = string.Empty;
            [SerializeField] private string titleLocalizationKey = string.Empty;
            [SerializeField, TextArea(1, 2)] private string titleFallback = string.Empty;

            public ChapterDefinition()
            {
            }

            public ChapterDefinition(
                string chapterId,
                string episodeCode,
                string titleLocalizationKey,
                string titleFallback)
            {
                Configure(chapterId, episodeCode, titleLocalizationKey, titleFallback);
            }

            public string ChapterId => chapterId;
            public string EpisodeCode => episodeCode;
            public string TitleLocalizationKey => titleLocalizationKey;
            public string TitleFallback => titleFallback;

            public void Configure(
                string newChapterId,
                string newEpisodeCode,
                string newTitleLocalizationKey,
                string newTitleFallback)
            {
                chapterId = newChapterId ?? string.Empty;
                episodeCode = newEpisodeCode ?? string.Empty;
                titleLocalizationKey = newTitleLocalizationKey ?? string.Empty;
                titleFallback = newTitleFallback ?? string.Empty;
            }

            internal ChapterDefinition DeepCopy()
            {
                return new ChapterDefinition(
                    ChapterId,
                    EpisodeCode,
                    TitleLocalizationKey,
                    TitleFallback);
            }
        }

        [Serializable]
        public sealed class StageDefinition
        {
            [SerializeField] private string stageId = string.Empty;
            [SerializeField] private string chapterId = string.Empty;
            [SerializeField] private string stageCode = string.Empty;
            [SerializeField] private string titleLocalizationKey = string.Empty;
            [SerializeField, TextArea(1, 2)] private string titleFallback = string.Empty;
            [SerializeField] private Vector2 normalizedMapPosition = new Vector2(0.5f, 0.5f);
            [SerializeField] private ChapterHubReviewContentStatus contentStatus;
            [SerializeField] private string canonicalCatalogEntryId = string.Empty;

            public StageDefinition()
            {
            }

            public StageDefinition(
                string stageId,
                string chapterId,
                string stageCode,
                string titleLocalizationKey,
                string titleFallback,
                Vector2 normalizedMapPosition,
                ChapterHubReviewContentStatus contentStatus,
                string canonicalCatalogEntryId = "")
            {
                Configure(
                    stageId,
                    chapterId,
                    stageCode,
                    titleLocalizationKey,
                    titleFallback,
                    normalizedMapPosition,
                    contentStatus,
                    canonicalCatalogEntryId);
            }

            public string StageId => stageId;
            public string ChapterId => chapterId;
            public string StageCode => stageCode;
            public string TitleLocalizationKey => titleLocalizationKey;
            public string TitleFallback => titleFallback;
            public Vector2 NormalizedMapPosition => normalizedMapPosition;
            public ChapterHubReviewContentStatus ContentStatus => contentStatus;
            public string CanonicalCatalogEntryId => canonicalCatalogEntryId;
            public bool IsCanonicalPlayable =>
                contentStatus == ChapterHubReviewContentStatus.CanonicalPlayable;

            public void Configure(
                string newStageId,
                string newChapterId,
                string newStageCode,
                string newTitleLocalizationKey,
                string newTitleFallback,
                Vector2 newNormalizedMapPosition,
                ChapterHubReviewContentStatus newContentStatus,
                string newCanonicalCatalogEntryId = "")
            {
                stageId = newStageId ?? string.Empty;
                chapterId = newChapterId ?? string.Empty;
                stageCode = newStageCode ?? string.Empty;
                titleLocalizationKey = newTitleLocalizationKey ?? string.Empty;
                titleFallback = newTitleFallback ?? string.Empty;
                normalizedMapPosition = newNormalizedMapPosition;
                contentStatus = newContentStatus;
                canonicalCatalogEntryId = newCanonicalCatalogEntryId ?? string.Empty;
            }

            internal StageDefinition DeepCopy()
            {
                return new StageDefinition(
                    StageId,
                    ChapterId,
                    StageCode,
                    TitleLocalizationKey,
                    TitleFallback,
                    NormalizedMapPosition,
                    ContentStatus,
                    CanonicalCatalogEntryId);
            }
        }

        [SerializeField] private ChapterDefinition[] chapters = Array.Empty<ChapterDefinition>();
        [SerializeField] private StageDefinition[] stages = Array.Empty<StageDefinition>();

        public int ChapterCount => chapters?.Length ?? 0;
        public int StageCount => stages?.Length ?? 0;
        public ChapterDefinition[] Chapters => CreateChapterSnapshot();
        public StageDefinition[] Stages => CreateStageSnapshot();

        public void Configure(
            ChapterDefinition[] newChapters,
            StageDefinition[] newStages)
        {
            chapters = CloneChapters(newChapters);
            stages = CloneStages(newStages);
        }

        public ChapterDefinition GetChapter(int index)
        {
            if (index < 0 || index >= ChapterCount)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return chapters[index]?.DeepCopy();
        }

        public StageDefinition GetStage(int index)
        {
            if (index < 0 || index >= StageCount)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return stages[index]?.DeepCopy();
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
                ? nameof(ChapterHubReviewProfile)
                : name;
            ChapterDefinition[] resolvedChapters = chapters ?? Array.Empty<ChapterDefinition>();
            StageDefinition[] resolvedStages = stages ?? Array.Empty<StageDefinition>();
            var chapterIds = new HashSet<string>(StringComparer.Ordinal);
            var stageIds = new HashSet<string>(StringComparer.Ordinal);

            if (resolvedChapters.Length == 0)
            {
                issues.Add($"{profileLabel}: no chapters are authored.");
            }

            for (int chapterIndex = 0; chapterIndex < resolvedChapters.Length; chapterIndex++)
            {
                ChapterDefinition chapter = resolvedChapters[chapterIndex];
                if (chapter == null)
                {
                    issues.Add($"{profileLabel}: chapter {chapterIndex} is null.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(chapter.ChapterId))
                {
                    issues.Add($"{profileLabel}: chapter {chapterIndex} has no chapter id.");
                }
                else if (!chapterIds.Add(chapter.ChapterId))
                {
                    issues.Add($"{profileLabel}: duplicate chapter id '{chapter.ChapterId}'.");
                }

                if (string.IsNullOrWhiteSpace(chapter.EpisodeCode))
                {
                    issues.Add($"{profileLabel}: chapter '{chapter.ChapterId}' has no episode code.");
                }

                if (string.IsNullOrWhiteSpace(chapter.TitleFallback))
                {
                    issues.Add(
                        $"{profileLabel}: chapter '{chapter.ChapterId}' has no fallback title for local review.");
                }
            }

            if (resolvedStages.Length == 0)
            {
                issues.Add($"{profileLabel}: no stages are authored.");
            }

            for (int stageIndex = 0; stageIndex < resolvedStages.Length; stageIndex++)
            {
                StageDefinition stage = resolvedStages[stageIndex];
                if (stage == null)
                {
                    issues.Add($"{profileLabel}: stage {stageIndex} is null.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(stage.StageId))
                {
                    issues.Add($"{profileLabel}: stage {stageIndex} has no stage id.");
                }
                else if (!stageIds.Add(stage.StageId))
                {
                    issues.Add($"{profileLabel}: duplicate stage id '{stage.StageId}'.");
                }

                if (string.IsNullOrWhiteSpace(stage.ChapterId)
                    || !chapterIds.Contains(stage.ChapterId))
                {
                    issues.Add(
                        $"{profileLabel}: stage '{stage.StageId}' references unknown chapter '{stage.ChapterId}'.");
                }

                if (string.IsNullOrWhiteSpace(stage.StageCode))
                {
                    issues.Add($"{profileLabel}: stage '{stage.StageId}' has no stage code.");
                }

                if (string.IsNullOrWhiteSpace(stage.TitleFallback))
                {
                    issues.Add(
                        $"{profileLabel}: stage '{stage.StageId}' has no fallback title for local review.");
                }

                Vector2 position = stage.NormalizedMapPosition;
                if (!IsNormalizedCoordinate(position.x) || !IsNormalizedCoordinate(position.y))
                {
                    issues.Add(
                        $"{profileLabel}: stage '{stage.StageId}' map position must be finite and within 0..1 on both axes.");
                }

                if (!Enum.IsDefined(typeof(ChapterHubReviewContentStatus), stage.ContentStatus)
                    || stage.ContentStatus == ChapterHubReviewContentStatus.None)
                {
                    issues.Add(
                        $"{profileLabel}: stage '{stage.StageId}' has unsupported content status '{stage.ContentStatus}'.");
                    continue;
                }

                bool hasCanonicalCatalogEntryId =
                    !string.IsNullOrWhiteSpace(stage.CanonicalCatalogEntryId);
                if (stage.ContentStatus == ChapterHubReviewContentStatus.CanonicalPlayable
                    && !hasCanonicalCatalogEntryId)
                {
                    issues.Add(
                        $"{profileLabel}: canonical stage '{stage.StageId}' has no canonical catalog entry id.");
                }
                else if (stage.ContentStatus != ChapterHubReviewContentStatus.CanonicalPlayable
                    && hasCanonicalCatalogEntryId)
                {
                    issues.Add(
                        $"{profileLabel}: noncanonical stage '{stage.StageId}' must not declare a canonical catalog entry id.");
                }
            }
        }

        internal ChapterDefinition[] CreateChapterSnapshot()
        {
            return CloneChapters(chapters);
        }

        internal StageDefinition[] CreateStageSnapshot()
        {
            return CloneStages(stages);
        }

        private static bool IsNormalizedCoordinate(float value)
        {
            return !float.IsNaN(value)
                && !float.IsInfinity(value)
                && value >= 0f
                && value <= 1f;
        }

        private static ChapterDefinition[] CloneChapters(ChapterDefinition[] source)
        {
            if (source == null || source.Length == 0)
            {
                return Array.Empty<ChapterDefinition>();
            }

            var clone = new ChapterDefinition[source.Length];
            for (int i = 0; i < source.Length; i++)
            {
                clone[i] = source[i]?.DeepCopy();
            }

            return clone;
        }

        private static StageDefinition[] CloneStages(StageDefinition[] source)
        {
            if (source == null || source.Length == 0)
            {
                return Array.Empty<StageDefinition>();
            }

            var clone = new StageDefinition[source.Length];
            for (int i = 0; i < source.Length; i++)
            {
                clone[i] = source[i]?.DeepCopy();
            }

            return clone;
        }

        private void OnValidate()
        {
            chapters ??= Array.Empty<ChapterDefinition>();
            stages ??= Array.Empty<StageDefinition>();
        }
    }
}
