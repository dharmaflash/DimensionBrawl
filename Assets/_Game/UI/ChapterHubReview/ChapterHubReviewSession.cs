using System;
using System.Collections.Generic;

namespace DimensionBrawl.UI.ChapterHubReview
{
    public enum ChapterHubReviewPhase
    {
        Overview = 0,
        StageMap = 10,
        StageDetail = 20,
        ReviewConfirm = 30
    }

    public sealed class ChapterHubReviewSession
    {
        private readonly ChapterHubReviewProfile.ChapterDefinition[] chapters;
        private readonly ChapterHubReviewProfile.StageDefinition[] stages;
        private readonly Dictionary<string, ChapterHubReviewProfile.ChapterDefinition>
            chaptersById;
        private readonly Dictionary<string, ChapterHubReviewProfile.StageDefinition>
            stagesById;

        private string selectedChapterId = string.Empty;
        private string selectedStageId = string.Empty;
        private bool confirmationAccepted;

        public ChapterHubReviewSession(ChapterHubReviewProfile profile)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            if (!profile.TryValidate(out string validationError))
            {
                throw new ArgumentException(validationError, nameof(profile));
            }

            chapters = profile.CreateChapterSnapshot();
            stages = profile.CreateStageSnapshot();
            chaptersById = new Dictionary<
                string,
                ChapterHubReviewProfile.ChapterDefinition>(
                chapters.Length,
                StringComparer.Ordinal);
            stagesById = new Dictionary<
                string,
                ChapterHubReviewProfile.StageDefinition>(
                stages.Length,
                StringComparer.Ordinal);

            for (int i = 0; i < chapters.Length; i++)
            {
                ChapterHubReviewProfile.ChapterDefinition chapter = chapters[i];
                chaptersById.Add(chapter.ChapterId, chapter);
            }

            for (int i = 0; i < stages.Length; i++)
            {
                ChapterHubReviewProfile.StageDefinition stage = stages[i];
                stagesById.Add(stage.StageId, stage);
            }
        }

        public ChapterHubReviewPhase Phase { get; private set; } =
            ChapterHubReviewPhase.Overview;
        public int ChapterCount => chapters.Length;
        public int StageCount => stages.Length;
        public string SelectedChapterId => selectedChapterId;
        public string SelectedStageId => selectedStageId;
        public bool IsConfirmationAccepted => confirmationAccepted;
        public ChapterHubReviewProfile.ChapterDefinition SelectedChapter =>
            TryGetSelectedChapterInternal(out ChapterHubReviewProfile.ChapterDefinition chapter)
                ? chapter.DeepCopy()
                : null;
        public ChapterHubReviewProfile.StageDefinition SelectedStage =>
            TryGetSelectedStageInternal(out ChapterHubReviewProfile.StageDefinition stage)
                ? stage.DeepCopy()
                : null;

        public ChapterHubReviewProfile.ChapterDefinition GetChapter(int index)
        {
            if (index < 0 || index >= chapters.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return chapters[index].DeepCopy();
        }

        public ChapterHubReviewProfile.StageDefinition GetStage(int index)
        {
            if (index < 0 || index >= stages.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return stages[index].DeepCopy();
        }

        public bool TryGetChapter(
            string chapterId,
            out ChapterHubReviewProfile.ChapterDefinition chapter)
        {
            if (!string.IsNullOrWhiteSpace(chapterId)
                && chaptersById.TryGetValue(chapterId, out ChapterHubReviewProfile.ChapterDefinition found))
            {
                chapter = found.DeepCopy();
                return true;
            }

            chapter = null;
            return false;
        }

        public bool TryGetStage(
            string stageId,
            out ChapterHubReviewProfile.StageDefinition stage)
        {
            if (!string.IsNullOrWhiteSpace(stageId)
                && stagesById.TryGetValue(stageId, out ChapterHubReviewProfile.StageDefinition found))
            {
                stage = found.DeepCopy();
                return true;
            }

            stage = null;
            return false;
        }

        public ChapterHubReviewProfile.StageDefinition[] GetStagesForChapter(string chapterId)
        {
            if (string.IsNullOrWhiteSpace(chapterId) || !chaptersById.ContainsKey(chapterId))
            {
                return Array.Empty<ChapterHubReviewProfile.StageDefinition>();
            }

            var matchingStages = new List<ChapterHubReviewProfile.StageDefinition>();
            for (int i = 0; i < stages.Length; i++)
            {
                ChapterHubReviewProfile.StageDefinition stage = stages[i];
                if (string.Equals(stage.ChapterId, chapterId, StringComparison.Ordinal))
                {
                    matchingStages.Add(stage.DeepCopy());
                }
            }

            return matchingStages.ToArray();
        }

        public bool TrySelectChapter(string chapterId)
        {
            if (Phase != ChapterHubReviewPhase.Overview
                || string.IsNullOrWhiteSpace(chapterId)
                || !chaptersById.ContainsKey(chapterId))
            {
                return false;
            }

            selectedChapterId = chapterId;
            selectedStageId = string.Empty;
            Phase = ChapterHubReviewPhase.StageMap;
            return true;
        }

        public bool TrySelectStage(string stageId)
        {
            if (Phase != ChapterHubReviewPhase.StageMap
                || string.IsNullOrWhiteSpace(stageId)
                || !stagesById.TryGetValue(
                    stageId,
                    out ChapterHubReviewProfile.StageDefinition stage)
                || !string.Equals(stage.ChapterId, selectedChapterId, StringComparison.Ordinal))
            {
                return false;
            }

            selectedStageId = stageId;
            Phase = ChapterHubReviewPhase.StageDetail;
            return true;
        }

        public bool TryOpenReviewConfirm()
        {
            if (Phase != ChapterHubReviewPhase.StageDetail
                || !TryGetSelectedStageInternal(
                    out ChapterHubReviewProfile.StageDefinition selectedStage)
                || !selectedStage.IsCanonicalPlayable
                || string.IsNullOrWhiteSpace(selectedStage.CanonicalCatalogEntryId))
            {
                return false;
            }

            Phase = ChapterHubReviewPhase.ReviewConfirm;
            return true;
        }

        public bool TryBack()
        {
            switch (Phase)
            {
                case ChapterHubReviewPhase.ReviewConfirm:
                    Phase = ChapterHubReviewPhase.StageDetail;
                    return true;
                case ChapterHubReviewPhase.StageDetail:
                    selectedStageId = string.Empty;
                    Phase = ChapterHubReviewPhase.StageMap;
                    return true;
                case ChapterHubReviewPhase.StageMap:
                    selectedStageId = string.Empty;
                    selectedChapterId = string.Empty;
                    Phase = ChapterHubReviewPhase.Overview;
                    return true;
                default:
                    return false;
            }
        }

        public bool TryConfirmSelectedStage(out string canonicalCatalogEntryId)
        {
            canonicalCatalogEntryId = string.Empty;
            if (confirmationAccepted
                || Phase != ChapterHubReviewPhase.ReviewConfirm
                || !TryGetSelectedStageInternal(
                    out ChapterHubReviewProfile.StageDefinition selectedStage)
                || !selectedStage.IsCanonicalPlayable
                || string.IsNullOrWhiteSpace(selectedStage.CanonicalCatalogEntryId))
            {
                return false;
            }

            confirmationAccepted = true;
            canonicalCatalogEntryId = selectedStage.CanonicalCatalogEntryId;
            return true;
        }

        private bool TryGetSelectedChapterInternal(
            out ChapterHubReviewProfile.ChapterDefinition chapter)
        {
            if (!string.IsNullOrWhiteSpace(selectedChapterId)
                && chaptersById.TryGetValue(selectedChapterId, out chapter))
            {
                return true;
            }

            chapter = null;
            return false;
        }

        private bool TryGetSelectedStageInternal(
            out ChapterHubReviewProfile.StageDefinition stage)
        {
            if (!string.IsNullOrWhiteSpace(selectedStageId)
                && stagesById.TryGetValue(selectedStageId, out stage))
            {
                return true;
            }

            stage = null;
            return false;
        }
    }
}
