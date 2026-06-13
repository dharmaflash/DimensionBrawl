using System;
using UnityEngine;

namespace DimensionBrawl.AI
{
    public enum CombatEnemyRunSegment
    {
        EntryRead,
        BreakGate,
        Backline,
        PressureRescue,
        BossBreakHandoff,
        FinalStand
    }

    [CreateAssetMenu(menuName = "DimensionBrawl/Profiles/Combat Enemy Role Profile", fileName = "DB_CombatEnemyRole")]
    public sealed class CombatEnemyRoleProfile : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string roleId = "SciFiSoldier.EntryProbe";
        [SerializeField] private string displayName = "Entry Probe";
        [SerializeField] private bool eliteRole;
        [SerializeField] private CombatEnemyRunSegment preferredSegment = CombatEnemyRunSegment.EntryRead;

        [Header("Linear Run Use")]
        [SerializeField, Min(0)] private int suggestedMinCount = 1;
        [SerializeField, Min(0)] private int suggestedMaxCount = 2;
        [SerializeField] private string pressurePurpose = "Teach one readable threat.";
        [SerializeField] private string intendedAnswer = "Move, read the windup, then dodge.";

        [Header("Combat Data")]
        [SerializeField] private CombatAiPatternProfile startingPattern;
        [SerializeField] private CombatAiPatternDeck patternDeck;
        [SerializeField] private CombatAiElitePatternProfile[] eliteProfiles = Array.Empty<CombatAiElitePatternProfile>();

        [Header("Reuse")]
        [SerializeField] private bool candidateForFutureSummonAiReuse = true;

        public string RoleId => roleId;
        public string DisplayName => displayName;
        public bool EliteRole => eliteRole;
        public CombatEnemyRunSegment PreferredSegment => preferredSegment;
        public int SuggestedMinCount => suggestedMinCount;
        public int SuggestedMaxCount => suggestedMaxCount;
        public string PressurePurpose => pressurePurpose;
        public string IntendedAnswer => intendedAnswer;
        public CombatAiPatternProfile StartingPattern => startingPattern;
        public CombatAiPatternDeck PatternDeck => patternDeck;
        public int EliteProfileCount => eliteProfiles != null ? eliteProfiles.Length : 0;
        public bool CandidateForFutureSummonAiReuse => candidateForFutureSummonAiReuse;

        public CombatAiElitePatternProfile GetEliteProfile(int index)
        {
            if (eliteProfiles == null || index < 0 || index >= eliteProfiles.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return eliteProfiles[index];
        }
    }
}
