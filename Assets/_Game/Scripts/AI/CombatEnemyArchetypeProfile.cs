using System;
using UnityEngine;

namespace DimensionBrawl.AI
{
    public enum CombatEnemyArchetypeKind
    {
        MobileSoldier,
        StaticTurret,
        BossCandidate
    }

    [CreateAssetMenu(menuName = "DimensionBrawl/Profiles/Combat Enemy Archetype Profile", fileName = "DB_CombatEnemyArchetype")]
    public sealed class CombatEnemyArchetypeProfile : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string archetypeId = "SciFiSoldier.Melee";
        [SerializeField] private string displayName = "Sci-fi Melee Soldier";
        [SerializeField] private CombatEnemyArchetypeKind archetypeKind = CombatEnemyArchetypeKind.MobileSoldier;
        [SerializeField] private bool participatesInActionFoundationRoleMap = true;

        [Header("Role Mapping")]
        [SerializeField] private CombatEnemyRoleProfile[] compatibleRoles = Array.Empty<CombatEnemyRoleProfile>();

        [Header("Presentation Candidates")]
        [SerializeField] private GameObject gameplayPrefab;
        [SerializeField] private GameObject visualPrefab;
        [SerializeField] private bool requiresDedicatedPrefabPromotion;
        [SerializeField] private bool candidateForFutureSummonAiReuse = true;

        [Header("Promotion Notes")]
        [SerializeField] private string sourceCandidate = "Promoted game-owned MaintenanceWorker visual.";
        [SerializeField] private string promotionPlan = "Use the existing promoted soldier visual until a dedicated prefab is reviewed.";
        [SerializeField] private string usageNotes = "Maps role intent to presentation candidates without changing pattern deck data.";

        public string ArchetypeId => archetypeId;
        public string DisplayName => displayName;
        public CombatEnemyArchetypeKind ArchetypeKind => archetypeKind;
        public bool ParticipatesInActionFoundationRoleMap => participatesInActionFoundationRoleMap;
        public int CompatibleRoleCount => compatibleRoles != null ? compatibleRoles.Length : 0;
        public GameObject GameplayPrefab => gameplayPrefab;
        public GameObject VisualPrefab => visualPrefab;
        public bool RequiresDedicatedPrefabPromotion => requiresDedicatedPrefabPromotion;
        public bool CandidateForFutureSummonAiReuse => candidateForFutureSummonAiReuse;
        public string SourceCandidate => sourceCandidate;
        public string PromotionPlan => promotionPlan;
        public string UsageNotes => usageNotes;

        public CombatEnemyRoleProfile GetCompatibleRole(int index)
        {
            if (compatibleRoles == null || index < 0 || index >= compatibleRoles.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return compatibleRoles[index];
        }
    }
}
