using System;
using DimensionBrawl.AI;
using DimensionBrawl.Combat;
using DimensionBrawl.Enemies;
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
        [SerializeField] private EnemyElitePatternController elitePatternController;
        [SerializeField, Min(0f)] private float damageCueIntensity = 1f;
        [SerializeField, Range(0.1f, 1f)] private float pressureDamageCueScale = 0.66f;
        [SerializeField] private CombatPatternVfxCueOverride[] patternCueOverrides = Array.Empty<CombatPatternVfxCueOverride>();
        [SerializeField] private CombatEliteVfxCueOverride[] eliteCueOverrides = Array.Empty<CombatEliteVfxCueOverride>();

        private ICombatAiAgent agent;
        private int damageVfxCueRequestCount;
        private float lastDamageCueIntensity;
        private float lastDamageCuePolicyScale = 1f;
        private DamageResponsePolicy lastDamageResponsePolicy = DamageResponsePolicy.Default;
        private CombatControlLockPolicy lastDamageControlLockPolicy = CombatControlLockPolicy.InterruptAction;
        private bool lastDamageCueInterruptedAction;

        public int DamageVfxCueRequestCount => damageVfxCueRequestCount;
        public float DamageCueIntensity => damageCueIntensity;
        public float PressureDamageCueScale => pressureDamageCueScale;
        public float LastDamageCueIntensity => lastDamageCueIntensity;
        public float LastDamageCuePolicyScale => lastDamageCuePolicyScale;
        public DamageResponsePolicy LastDamageResponsePolicy => lastDamageResponsePolicy;
        public CombatControlLockPolicy LastDamageControlLockPolicy => lastDamageControlLockPolicy;
        public bool LastDamageCueInterruptedAction => lastDamageCueInterruptedAction;

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

            if (elitePatternController == null)
            {
                elitePatternController = GetComponent<EnemyElitePatternController>();
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

            if (elitePatternController != null)
            {
                elitePatternController.SignalTriggered += HandleEliteSignalTriggered;
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

            if (elitePatternController != null)
            {
                elitePatternController.SignalTriggered -= HandleEliteSignalTriggered;
            }
        }

        private void HandlePatternStateChanged(CombatAiPatternState state, CombatAiPatternProfile profile)
        {
            switch (state)
            {
                case CombatAiPatternState.Windup:
                    Play(
                        ResolvePatternCueId(profile, windup: true, CombatVfxCueId.EnemyWindup, out float windupIntensity),
                        ResolveThreatDirection(),
                        windupIntensity);
                    break;
                case CombatAiPatternState.AttackActive:
                    Play(
                        ResolvePatternCueId(profile, windup: false, CombatVfxCueId.EnemyAttackActive, out float activeIntensity),
                        ResolveThreatDirection(),
                        activeIntensity);
                    break;
            }
        }

        private void HandleDamaged(DamageInfo damageInfo)
        {
            if (!DamageResponsePolicyUtility.PlaysDamagePresentation(damageInfo.ResponsePolicy))
            {
                return;
            }

            float policyScale = ResolveDamageCuePolicyScale(damageInfo);
            float intensity = damageCueIntensity * policyScale;
            bool interruptsAction = DamageResponsePolicyUtility.InterruptsAction(damageInfo.ControlLockPolicy);
            lastDamageCueIntensity = intensity;
            lastDamageCuePolicyScale = policyScale;
            lastDamageResponsePolicy = damageInfo.ResponsePolicy;
            lastDamageControlLockPolicy = damageInfo.ControlLockPolicy;
            lastDamageCueInterruptedAction = interruptsAction;

            if (Play(CombatVfxCueId.EnemyHit, damageInfo.Direction, intensity))
            {
                damageVfxCueRequestCount++;
            }
        }

        private void HandleDied()
        {
            Play(CombatVfxCueId.EnemyDeath, ResolveThreatDirection(), 1f);
        }

        private void HandleEliteSignalTriggered(CombatAiElitePatternProfile profile)
        {
            Play(
                ResolveEliteCueId(profile, out float intensity),
                ResolveThreatDirection(),
                intensity);
        }

        private float ResolveDamageCuePolicyScale(DamageInfo damageInfo)
        {
            return DamageResponsePolicyUtility.InterruptsAction(damageInfo.ControlLockPolicy)
                ? 1f
                : Mathf.Clamp(pressureDamageCueScale, 0.1f, 1f);
        }

        private bool Play(CombatVfxCueId cueId, Vector3 direction, float intensity)
        {
            if (cuePlayer == null)
            {
                return false;
            }

            return cuePlayer.PlayCue(cueId, cueAnchor != null ? cueAnchor : transform, direction, intensity);
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

        private CombatVfxCueId ResolvePatternCueId(
            CombatAiPatternProfile profile,
            bool windup,
            CombatVfxCueId fallbackCueId,
            out float intensity)
        {
            if (profile != null && patternCueOverrides != null)
            {
                for (int i = 0; i < patternCueOverrides.Length; i++)
                {
                    CombatPatternVfxCueOverride cueOverride = patternCueOverrides[i];
                    if (cueOverride.PatternProfile != profile)
                    {
                        continue;
                    }

                    intensity = windup
                        ? cueOverride.WindupIntensity
                        : cueOverride.AttackActiveIntensity;
                    return windup
                        ? cueOverride.WindupCueId
                        : cueOverride.AttackActiveCueId;
                }
            }

            intensity = 1f;
            return fallbackCueId;
        }

        private CombatVfxCueId ResolveEliteCueId(CombatAiElitePatternProfile profile, out float intensity)
        {
            if (profile != null && eliteCueOverrides != null)
            {
                for (int i = 0; i < eliteCueOverrides.Length; i++)
                {
                    CombatEliteVfxCueOverride cueOverride = eliteCueOverrides[i];
                    if (cueOverride.EliteProfile != profile)
                    {
                        continue;
                    }

                    intensity = cueOverride.Intensity;
                    return cueOverride.SignalCueId;
                }
            }

            intensity = 1f;
            return CombatVfxCueId.EliteSignal;
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
