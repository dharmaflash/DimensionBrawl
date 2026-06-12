using DimensionBrawl.Combat;
using UnityEngine;

namespace DimensionBrawl.AI
{
    public interface ICombatAiAgent
    {
        CombatHealth SelfHealth { get; }
        CombatTargetSensor TargetSensor { get; }
        CombatAiPatternProfile PatternProfile { get; }
        string ActorTypeId { get; }
        string PatternId { get; }
        string AttackAnimationTrigger { get; }
        string HitAnimationTrigger { get; }
        string DeathAnimationTrigger { get; }

        void ConfigurePattern(CombatAiPatternProfile profile);
        void ConfigureTarget(Transform newTarget, CombatHealth newTargetHealth);
    }
}
