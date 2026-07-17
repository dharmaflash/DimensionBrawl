using System;
using UnityEngine;

namespace DimensionBrawl.LevelDesign
{
    [CreateAssetMenu(
        menuName = "DimensionBrawl/Profiles/Stage Progression Node",
        fileName = "DB_StageProgressionNode")]
    public sealed class StageProgressionNode : ScriptableObject
    {
        [SerializeField, Min(1)] private int schemaVersion = 1;
        [SerializeField] private string progressionNodeId;
        [SerializeField, Min(1)] private int revision = 1;
        [SerializeField, Min(1)] private int contentRevision = 1;
        [SerializeField] private StageResultProgressionReferenceDisposition battleStageDisposition =
            StageResultProgressionReferenceDisposition.NotAuthoredForCurrentSchema;
        [SerializeField] private string battleStageId;
        [SerializeField] private StageProgressionPrerequisiteRef[] prerequisites =
            Array.Empty<StageProgressionPrerequisiteRef>();
        [SerializeField] private StageProgressionRecommendedNextRef[] recommendedNext =
            Array.Empty<StageProgressionRecommendedNextRef>();
        [SerializeField] private StageResultProgressionReferenceDisposition preBattleStoryDisposition =
            StageResultProgressionReferenceDisposition.NotAuthoredForCurrentSchema;
        [SerializeField] private string preBattleStoryId;
        [SerializeField] private StageResultProgressionReferenceDisposition postBattleStoryDisposition =
            StageResultProgressionReferenceDisposition.NotAuthoredForCurrentSchema;
        [SerializeField] private string postBattleStoryId;
        [SerializeField] private StageResultProgressionReferenceDisposition afterClearScriptDisposition =
            StageResultProgressionReferenceDisposition.NotAuthoredForCurrentSchema;
        [SerializeField] private string afterClearScriptId;
        [SerializeField] private StageResultProgressionReferenceDisposition rewardPlanDisposition =
            StageResultProgressionReferenceDisposition.NotAdmittedByCurrentSchema;
        [SerializeField] private string rewardPlanId;
        [SerializeField, Min(0)] private int rewardPlanRevision;
        [SerializeField] private string rewardPlanDigest;
        [SerializeField, Min(1)] private int bindingRevision = 1;
        [SerializeField] private string playableStageId;
        [SerializeField, Min(1)] private int routeRevision = 1;
        [SerializeField] private string canonicalRouteDigest;
        [SerializeField] private string progressionGraphId;
        [SerializeField, Min(1)] private int progressionGraphRevision = 1;
        [SerializeField] private string contentDigest;
        [SerializeField] private string bindingDigest;

        public int SchemaVersion => schemaVersion;
        public string ProgressionNodeId => progressionNodeId;
        public int Revision => revision;
        public int ContentRevision => contentRevision;
        public StageResultProgressionReferenceDisposition BattleStageDisposition =>
            battleStageDisposition;
        public string BattleStageId => battleStageId;
        public int PrerequisiteCount => prerequisites != null ? prerequisites.Length : 0;
        public int RecommendedNextCount => recommendedNext != null ? recommendedNext.Length : 0;
        public StageResultProgressionReferenceDisposition PreBattleStoryDisposition =>
            preBattleStoryDisposition;
        public string PreBattleStoryId => preBattleStoryId;
        public StageResultProgressionReferenceDisposition PostBattleStoryDisposition =>
            postBattleStoryDisposition;
        public string PostBattleStoryId => postBattleStoryId;
        public StageResultProgressionReferenceDisposition AfterClearScriptDisposition =>
            afterClearScriptDisposition;
        public string AfterClearScriptId => afterClearScriptId;
        public StageResultProgressionReferenceDisposition RewardPlanDisposition =>
            rewardPlanDisposition;
        public string RewardPlanId => rewardPlanId;
        public int RewardPlanRevision => rewardPlanRevision;
        public string RewardPlanDigest => rewardPlanDigest;
        public int BindingRevision => bindingRevision;
        public string PlayableStageId => playableStageId;
        public int RouteRevision => routeRevision;
        public string CanonicalRouteDigest => canonicalRouteDigest;
        public string ProgressionGraphId => progressionGraphId;
        public int ProgressionGraphRevision => progressionGraphRevision;
        public string ContentDigest => contentDigest;
        public string BindingDigest => bindingDigest;

        public StageProgressionPrerequisiteRef GetPrerequisite(int index)
        {
            if (prerequisites == null || index < 0 || index >= prerequisites.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return prerequisites[index];
        }

        public StageProgressionRecommendedNextRef GetRecommendedNext(int index)
        {
            if (recommendedNext == null || index < 0 || index >= recommendedNext.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return recommendedNext[index];
        }

        public bool TryCreateSnapshot(
            out StageProgressionNodeSnapshot snapshot,
            out string error)
        {
            return StageProgressionNodeSnapshot.TryCreate(this, out snapshot, out error);
        }

        public bool TryComputeCanonicalDigests(
            out string contentDigest,
            out string bindingDigest,
            out string error)
        {
            return StageProgressionNodeSnapshot.TryComputeCanonicalDigests(
                this,
                out contentDigest,
                out bindingDigest,
                out error);
        }
    }
}
