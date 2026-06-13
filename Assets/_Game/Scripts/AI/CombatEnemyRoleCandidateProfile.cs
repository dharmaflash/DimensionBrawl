using DimensionBrawl.Presentation;
using UnityEngine;

namespace DimensionBrawl.AI
{
    [CreateAssetMenu(menuName = "DimensionBrawl/Profiles/Combat Enemy Role Candidate Profile", fileName = "DB_CombatEnemyRoleCandidate")]
    public sealed class CombatEnemyRoleCandidateProfile : ScriptableObject
    {
        [Header("Role")]
        [SerializeField] private CombatEnemyRoleProfile role;
        [SerializeField] private CombatEnemyArchetypeProfile primaryArchetype;

        [Header("Promoted Candidates")]
        [SerializeField] private GameObject rolePrefab;
        [SerializeField] private GameObject promotedVisualSource;
        [SerializeField] private GameObject optionalStaticTurretVisualPrefab;
        [SerializeField] private CombatVfxCueProfile vfxCueProfile;

        [Header("Review Notes")]
        [SerializeField] private string prefabStrategy = "Dedicated role prefab variant.";
        [SerializeField] private string animationRead = "Use promoted Animator clips and role pattern triggers.";
        [SerializeField] private string vfxRead = "Use pattern/elite cue overrides from the shared combat VFX profile.";
        [SerializeField] private string reuseJustification = "Role remains candidate-safe for future summon AI reuse.";

        public CombatEnemyRoleProfile Role => role;
        public CombatEnemyArchetypeProfile PrimaryArchetype => primaryArchetype;
        public GameObject RolePrefab => rolePrefab;
        public GameObject PromotedVisualSource => promotedVisualSource;
        public GameObject OptionalStaticTurretVisualPrefab => optionalStaticTurretVisualPrefab;
        public CombatVfxCueProfile VfxCueProfile => vfxCueProfile;
        public string PrefabStrategy => prefabStrategy;
        public string AnimationRead => animationRead;
        public string VfxRead => vfxRead;
        public string ReuseJustification => reuseJustification;
    }
}
