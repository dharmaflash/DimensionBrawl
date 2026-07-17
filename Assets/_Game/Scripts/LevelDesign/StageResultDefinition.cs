using System;
using DimensionBrawl.UI.StageClear;
using UnityEngine;

namespace DimensionBrawl.LevelDesign
{
    [CreateAssetMenu(
        menuName = "DimensionBrawl/Profiles/Stage Result Definition",
        fileName = "DB_StageResultDefinition")]
    public sealed class StageResultDefinition : ScriptableObject
    {
        [SerializeField, Min(1)] private int schemaVersion = 1;
        [SerializeField] private string resultDefinitionId;
        [SerializeField, Min(1)] private int revision = 1;
        [SerializeField, Min(1)] private int evaluationContentRevision = 1;
        [SerializeField] private string playableStageId;
        [SerializeField, Min(1)] private int supportedRunSchemaVersion = 1;
        [SerializeField] private StageResultProgressionReferenceDisposition masterySetDisposition =
            StageResultProgressionReferenceDisposition.NotAdmittedByCurrentSchema;
        [SerializeField] private string masterySetId;
        [SerializeField, Min(0)] private int masterySetRevision;
        [SerializeField] private string masterySetSemanticDigest;
        [SerializeField] private StageResultProgressionReferenceDisposition
            requiredFactCapabilitiesDisposition =
                StageResultProgressionReferenceDisposition.NotAdmittedByCurrentSchema;
        [SerializeField, Min(0)] private int requiredFactCapabilityCount;
        [SerializeField] private string requiredFactCapabilitiesDigest;
        [SerializeField] private StageResultProgressionReferenceDisposition
            allowedSemanticProofsDisposition =
                StageResultProgressionReferenceDisposition.NotAdmittedByCurrentSchema;
        [SerializeField, Min(0)] private int allowedSemanticProofCount;
        [SerializeField] private string allowedSemanticProofsDigest;
        [SerializeField, Min(1)] private int presentationBindingRevision = 1;
        [SerializeField] private StageResultPresentationCatalog canonicalPresentationCatalog;
        [SerializeField] private StageResultPresentationProfile presentationProfile;
        [SerializeField] private StageResultLocalizationTable localizationTable;
        [SerializeField] private StageResultLocaleResolutionPolicy localeResolutionPolicy =
            StageResultLocaleResolutionPolicy.ExactThenLanguageThenDefaultOrdinalIgnoreCase;
        [SerializeField] private StageResultActionPresentationMapping[] actionMappings =
            Array.Empty<StageResultActionPresentationMapping>();
        [SerializeField] private string evaluationContentDigest;
        [SerializeField] private string presentationBindingDigest;
        [SerializeField] private string presentationSourceDigest;

        public int SchemaVersion => schemaVersion;
        public string ResultDefinitionId => resultDefinitionId;
        public int Revision => revision;
        public int EvaluationContentRevision => evaluationContentRevision;
        public string PlayableStageId => playableStageId;
        public int SupportedRunSchemaVersion => supportedRunSchemaVersion;
        public StageResultProgressionReferenceDisposition MasterySetDisposition =>
            masterySetDisposition;
        public string MasterySetId => masterySetId;
        public int MasterySetRevision => masterySetRevision;
        public string MasterySetSemanticDigest => masterySetSemanticDigest;
        public StageResultProgressionReferenceDisposition RequiredFactCapabilitiesDisposition =>
            requiredFactCapabilitiesDisposition;
        public int RequiredFactCapabilityCount => requiredFactCapabilityCount;
        public string RequiredFactCapabilitiesDigest => requiredFactCapabilitiesDigest;
        public StageResultProgressionReferenceDisposition AllowedSemanticProofsDisposition =>
            allowedSemanticProofsDisposition;
        public int AllowedSemanticProofCount => allowedSemanticProofCount;
        public string AllowedSemanticProofsDigest => allowedSemanticProofsDigest;
        public int PresentationBindingRevision => presentationBindingRevision;
        public StageResultPresentationCatalog CanonicalPresentationCatalog =>
            canonicalPresentationCatalog;
        public StageResultPresentationProfile PresentationProfile => presentationProfile;
        public StageResultLocalizationTable LocalizationTable => localizationTable;
        public StageResultLocaleResolutionPolicy LocaleResolutionPolicy => localeResolutionPolicy;
        public int ActionMappingCount => actionMappings != null ? actionMappings.Length : 0;
        public string EvaluationContentDigest => evaluationContentDigest;
        public string PresentationBindingDigest => presentationBindingDigest;
        public string PresentationSourceDigest => presentationSourceDigest;

        public StageResultActionPresentationMapping GetActionMapping(int index)
        {
            if (actionMappings == null || index < 0 || index >= actionMappings.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return actionMappings[index];
        }

        public bool TryCreateSnapshot(
            out StageResultDefinitionSnapshot snapshot,
            out string error)
        {
            return StageResultDefinitionSnapshot.TryCreate(this, out snapshot, out error);
        }

        public bool TryComputeCanonicalDigests(
            out string evaluationContentDigest,
            out string presentationBindingDigest,
            out string presentationSourceDigest,
            out string error)
        {
            return StageResultDefinitionSnapshot.TryComputeCanonicalDigests(
                this,
                out evaluationContentDigest,
                out presentationBindingDigest,
                out presentationSourceDigest,
                out error);
        }
    }
}
