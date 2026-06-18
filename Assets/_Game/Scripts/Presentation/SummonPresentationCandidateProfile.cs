using UnityEngine;

namespace DimensionBrawl.Presentation
{
    public enum SummonPresentationSide
    {
        PlayerSummon,
        BossPressure
    }

    [CreateAssetMenu(
        fileName = "DB_SummonPresentationCandidate",
        menuName = "DimensionBrawl/Presentation/Summon Presentation Candidate")]
    public sealed class SummonPresentationCandidateProfile : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string candidateId = "Summon.Presentation";
        [SerializeField] private string displayName = "Summon Presentation Candidate";
        [SerializeField] private SummonPresentationSide side = SummonPresentationSide.PlayerSummon;

        [Header("Promoted Presentation")]
        [SerializeField] private GameObject actorPrefab;
        [SerializeField] private GameObject visualSourceAsset;
        [SerializeField] private string visualChildName = "Visual";
        [SerializeField] private string sourceRoleId = "SciFiSoldier.Elite";
        [SerializeField] private RuntimeAnimatorController animatorController;
        [SerializeField] private CombatVfxCueProfile vfxCueProfile;

        [Header("Review Notes")]
        [SerializeField] private string animationRead = "Uses the promoted role Animator until a dedicated summon animation set is reviewed.";
        [SerializeField] private string vfxRead = "Uses current proxy pulse and shared combat VFX cue reads.";
        [SerializeField] private string replacementPlan = "Swap the actor prefab or visual source after a dedicated summon model is promoted.";
        [SerializeField] private string ownershipNotes = "Presentation candidate only; gameplay tuning stays in summon/boss pressure profiles.";

        public string CandidateId => candidateId;
        public string DisplayName => displayName;
        public SummonPresentationSide Side => side;
        public GameObject ActorPrefab => actorPrefab;
        public GameObject VisualSourceAsset => visualSourceAsset;
        public string VisualChildName => visualChildName;
        public string SourceRoleId => sourceRoleId;
        public RuntimeAnimatorController AnimatorController => animatorController;
        public CombatVfxCueProfile VfxCueProfile => vfxCueProfile;
        public string AnimationRead => animationRead;
        public string VfxRead => vfxRead;
        public string ReplacementPlan => replacementPlan;
        public string OwnershipNotes => ownershipNotes;
    }
}
