using DimensionBrawl.AI;
using DimensionBrawl.Combat;
using UnityEngine;

namespace DimensionBrawl.Presentation
{
    [DisallowMultipleComponent]
    public sealed class EnemyActionCameraCueDriver : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MonoBehaviour agentSource;
        [SerializeField] private ActionCameraController cameraController;
        [SerializeField] private Transform cueSpace;

        private ICombatAiAgent agent;

        public MonoBehaviour AgentSource => agentSource;
        public ActionCameraController CameraController => cameraController;

        private void Awake()
        {
            if (cameraController == null)
            {
                cameraController = GetComponent<ActionCameraController>();
            }
        }

        private void OnEnable()
        {
            ResolveAgent();
            if (agent != null)
            {
                agent.PatternStateChanged += HandlePatternStateChanged;
            }
        }

        private void OnDisable()
        {
            if (agent != null)
            {
                agent.PatternStateChanged -= HandlePatternStateChanged;
            }
        }

        private void HandlePatternStateChanged(CombatAiPatternState state, CombatAiPatternProfile profile)
        {
            switch (state)
            {
                case CombatAiPatternState.Windup:
                    RequestCue(profile != null ? profile.WindupCameraCue : DefaultWindupCue, ResolveWindupScale(profile));
                    break;
                case CombatAiPatternState.AttackActive:
                    RequestCue(profile != null ? profile.ActiveCameraCue : DefaultActiveCue, ResolveActiveScale(profile));
                    break;
                case CombatAiPatternState.Death:
                    RequestCue(profile != null ? profile.DeathCameraCue : DefaultDeathCue, ResolveDeathScale(profile));
                    break;
            }
        }

        private void RequestCue(
            CombatAiCameraCue cue,
            float scale)
        {
            if (!cue.enabled || cameraController == null)
            {
                return;
            }

            Transform space = cueSpace != null ? cueSpace : (agentSource != null ? agentSource.transform : transform);
            Vector3 offset = space.TransformDirection(cue.localOffset);
            Vector3 threatDirection = ResolveThreatDirection();
            if (threatDirection.sqrMagnitude > 0.0001f)
            {
                offset += threatDirection.normalized * cue.planarDirectionOffset;
            }

            float clampedScale = Mathf.Max(0f, scale);
            cameraController.RequestCue(
                offset * clampedScale * cue.finisherScale,
                cue.durationSeconds,
                cue.fieldOfViewDelta * clampedScale * cue.finisherScale,
                cue.cameraDistanceDelta * clampedScale * cue.finisherScale,
                cue.focusHeightDelta * clampedScale * cue.finisherScale);
        }

        private void ResolveAgent()
        {
            agent = agentSource as ICombatAiAgent;
        }

        private static float ResolveWindupScale(CombatAiPatternProfile profile)
        {
            return profile != null ? profile.WindupThreatLevel : 1f;
        }

        private static float ResolveActiveScale(CombatAiPatternProfile profile)
        {
            return profile != null ? profile.ActiveCameraCueStrength : 1f;
        }

        private static float ResolveDeathScale(CombatAiPatternProfile profile)
        {
            return profile != null ? profile.DeathCameraCueStrength : 0.6f;
        }

        private Vector3 ResolveThreatDirection()
        {
            if (agent != null
                && agent.TargetSensor != null
                && agent.TargetSensor.TryGetCurrentTarget(out Transform target, out CombatHealth _)
                && target != null
                && agentSource != null)
            {
                Vector3 fromTargetToThreat = Vector3.ProjectOnPlane(agentSource.transform.position - target.position, Vector3.up);
                if (fromTargetToThreat.sqrMagnitude > 0.0001f)
                {
                    return fromTargetToThreat.normalized;
                }
            }

            return agentSource != null ? Vector3.ProjectOnPlane(agentSource.transform.forward, Vector3.up).normalized : transform.forward;
        }

        private static CombatAiCameraCue DefaultWindupCue => new CombatAiCameraCue
        {
            enabled = true,
            localOffset = new Vector3(0.06f, 0.02f, -0.06f),
            planarDirectionOffset = 0.06f,
            fieldOfViewDelta = 0.4f,
            cameraDistanceDelta = -0.04f,
            focusHeightDelta = 0.01f,
            durationSeconds = 0.20f,
            finisherScale = 1f
        };

        private static CombatAiCameraCue DefaultActiveCue => new CombatAiCameraCue
        {
            enabled = true,
            localOffset = new Vector3(0f, 0.03f, 0.08f),
            planarDirectionOffset = 0.08f,
            fieldOfViewDelta = 0.7f,
            cameraDistanceDelta = -0.08f,
            focusHeightDelta = 0.02f,
            durationSeconds = 0.18f,
            finisherScale = 1f
        };

        private static CombatAiCameraCue DefaultDeathCue => new CombatAiCameraCue
        {
            enabled = true,
            localOffset = new Vector3(0f, 0.02f, 0.10f),
            planarDirectionOffset = 0.06f,
            fieldOfViewDelta = -0.6f,
            cameraDistanceDelta = 0.08f,
            focusHeightDelta = -0.02f,
            durationSeconds = 0.24f,
            finisherScale = 1f
        };
    }
}
