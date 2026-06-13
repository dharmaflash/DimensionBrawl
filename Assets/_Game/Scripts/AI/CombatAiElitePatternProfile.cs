using UnityEngine;

namespace DimensionBrawl.AI
{
    public enum CombatAiElitePatternKind
    {
        ShieldCycle,
        ArmorBreak,
        AuraBuffer,
        SummonPackage,
        PhaseSwap
    }

    [CreateAssetMenu(menuName = "DimensionBrawl/Profiles/Combat AI Elite Pattern Profile")]
    public sealed class CombatAiElitePatternProfile : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string patternId = "ShieldCycle";
        [SerializeField] private CombatAiElitePatternKind patternKind = CombatAiElitePatternKind.ShieldCycle;

        [Header("Trigger")]
        [SerializeField, Range(0f, 1f)] private float triggerHealthRatio = 1f;
        [SerializeField, Min(0f)] private float signalSeconds = 1.1f;

        [Header("Guard/Break")]
        [SerializeField, Min(0f)] private float guardMeter = 60f;
        [SerializeField, Range(0f, 1f)] private float damageTakenMultiplier = 0.45f;
        [SerializeField, Min(0f)] private float breakSeconds = 0.75f;
        [SerializeField, Min(0f)] private float refreshSeconds = 4.5f;

        [Header("Phase")]
        [SerializeField] private CombatAiPatternProfile replacementPatternProfile;
        [SerializeField] private CombatAiPatternDeck replacementPatternDeck;

        [Header("Presentation")]
        [SerializeField] private Color cueColor = new Color(0.3f, 0.75f, 1f, 1f);
        [SerializeField] private string signalAnimationTrigger = string.Empty;

        public string PatternId => patternId;
        public CombatAiElitePatternKind PatternKind => patternKind;
        public float TriggerHealthRatio => triggerHealthRatio;
        public float SignalSeconds => signalSeconds;
        public float GuardMeter => guardMeter;
        public float DamageTakenMultiplier => damageTakenMultiplier;
        public float BreakSeconds => breakSeconds;
        public float RefreshSeconds => refreshSeconds;
        public CombatAiPatternProfile ReplacementPatternProfile => replacementPatternProfile;
        public CombatAiPatternDeck ReplacementPatternDeck => replacementPatternDeck;
        public Color CueColor => cueColor;
        public string SignalAnimationTrigger => signalAnimationTrigger;
    }
}
