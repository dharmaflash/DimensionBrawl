using DimensionBrawl.AI;
using DimensionBrawl.Combat;
using UnityEngine;

namespace DimensionBrawl.Presentation
{
    [DisallowMultipleComponent]
    public sealed class EnemyCombatVfxCueDriver : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour agentSource;
        [SerializeField] private CombatHealth health;
        [SerializeField] private CombatVfxCuePlayer cuePlayer;
        [SerializeField] private Transform cueAnchor;

        private ICombatAiAgent agent;

        private void Awake()
        {
            if (health == null)
            {
                health = GetComponent<CombatHealth>();
            }

            if (cuePlayer == null)
            {
                cuePlayer = GetComponent<CombatVfxCuePlayer>();
            }

            ResolveAgent();
        }

        private void OnEnable()
        {
            ResolveAgent();
            if (agent != null)
            {
                agent.PatternStateChanged += HandlePatternStateChanged;
            }

            if (health != null)
            {
                health.Damaged += HandleDamaged;
                health.Died += HandleDied;
            }
        }

        private void OnDisable()
        {
            if (agent != null)
            {
                agent.PatternStateChanged -= HandlePatternStateChanged;
            }

            if (health != null)
            {
                health.Damaged -= HandleDamaged;
                health.Died -= HandleDied;
            }
        }

        private void HandlePatternStateChanged(CombatAiPatternState state, CombatAiPatternProfile profile)
        {
            switch (state)
            {
                case CombatAiPatternState.Windup:
                    Play(CombatVfxCueId.EnemyWindup, ResolveThreatDirection(), 1f);
                    break;
                case CombatAiPatternState.AttackActive:
                    Play(CombatVfxCueId.EnemyAttackActive, ResolveThreatDirection(), 1f);
                    break;
            }
        }

        private void HandleDamaged(DamageInfo damageInfo)
        {
            Play(CombatVfxCueId.EnemyHit, damageInfo.Direction, 1f);
        }

        private void HandleDied()
        {
            Play(CombatVfxCueId.EnemyDeath, ResolveThreatDirection(), 1f);
        }

        private void Play(CombatVfxCueId cueId, Vector3 direction, float intensity)
        {
            if (cuePlayer == null)
            {
                return;
            }

            cuePlayer.PlayCue(cueId, cueAnchor != null ? cueAnchor : transform, direction, intensity);
        }

        private void ResolveAgent()
        {
            if (agentSource is ICombatAiAgent configuredAgent)
            {
                agent = configuredAgent;
                return;
            }

            MonoBehaviour[] components = GetComponents<MonoBehaviour>();
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] is ICombatAiAgent foundAgent)
                {
                    agentSource = components[i];
                    agent = foundAgent;
                    return;
                }
            }

            agent = null;
        }

        private Vector3 ResolveThreatDirection()
        {
            if (agent != null
                && agent.TargetSensor != null
                && agent.TargetSensor.TryGetCurrentTarget(out Transform target, out CombatHealth _)
                && target != null)
            {
                Vector3 direction = Vector3.ProjectOnPlane(target.position - transform.position, Vector3.up);
                if (direction.sqrMagnitude > 0.0001f)
                {
                    return direction.normalized;
                }
            }

            return transform.forward;
        }
    }
}
