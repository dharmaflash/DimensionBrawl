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

        [Header("Close Punish")]
        [SerializeField] private ActionCameraCueProfile.CameraCue closeWindupCue = new ActionCameraCueProfile.CameraCue
        {
            enabled = true,
            localOffset = new Vector3(0.06f, 0.02f, -0.06f),
            planarDirectionOffset = 0.06f,
            fieldOfViewDelta = 0.4f,
            cameraDistanceDelta = -0.04f,
            focusHeightDelta = 0.01f,
            durationSeconds = 0.18f,
            finisherScale = 1f
        };

        [SerializeField] private ActionCameraCueProfile.CameraCue closeActiveCue = new ActionCameraCueProfile.CameraCue
        {
            enabled = true,
            localOffset = new Vector3(0f, 0.03f, 0.08f),
            planarDirectionOffset = 0.08f,
            fieldOfViewDelta = 0.7f,
            cameraDistanceDelta = -0.08f,
            focusHeightDelta = 0.02f,
            durationSeconds = 0.16f,
            finisherScale = 1f
        };

        [Header("Lunge Strike")]
        [SerializeField] private ActionCameraCueProfile.CameraCue lungeWindupCue = new ActionCameraCueProfile.CameraCue
        {
            enabled = true,
            localOffset = new Vector3(0.09f, 0.03f, -0.10f),
            planarDirectionOffset = 0.12f,
            fieldOfViewDelta = 0.8f,
            cameraDistanceDelta = -0.08f,
            focusHeightDelta = 0.02f,
            durationSeconds = 0.24f,
            finisherScale = 1f
        };

        [SerializeField] private ActionCameraCueProfile.CameraCue lungeActiveCue = new ActionCameraCueProfile.CameraCue
        {
            enabled = true,
            localOffset = new Vector3(0f, 0.04f, 0.14f),
            planarDirectionOffset = 0.18f,
            fieldOfViewDelta = 1.4f,
            cameraDistanceDelta = -0.16f,
            focusHeightDelta = 0.03f,
            durationSeconds = 0.24f,
            finisherScale = 1f
        };

        [Header("Heavy Windup")]
        [SerializeField] private ActionCameraCueProfile.CameraCue heavyWindupCue = new ActionCameraCueProfile.CameraCue
        {
            enabled = true,
            localOffset = new Vector3(0.12f, 0.04f, -0.14f),
            planarDirectionOffset = 0.16f,
            fieldOfViewDelta = 1.1f,
            cameraDistanceDelta = -0.12f,
            focusHeightDelta = 0.04f,
            durationSeconds = 0.30f,
            finisherScale = 1f
        };

        [SerializeField] private ActionCameraCueProfile.CameraCue heavyActiveCue = new ActionCameraCueProfile.CameraCue
        {
            enabled = true,
            localOffset = new Vector3(0f, 0.05f, 0.18f),
            planarDirectionOffset = 0.14f,
            fieldOfViewDelta = 1.8f,
            cameraDistanceDelta = -0.18f,
            focusHeightDelta = 0.04f,
            durationSeconds = 0.26f,
            finisherScale = 1f
        };

        [Header("Shared")]
        [SerializeField] private ActionCameraCueProfile.CameraCue deathCue = new ActionCameraCueProfile.CameraCue
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
                    RequestCue(ResolveWindupCue(profile), ResolveWindupScale(profile), ResolveWindupDuration(profile));
                    break;
                case CombatAiPatternState.AttackActive:
                    RequestCue(ResolveActiveCue(profile), ResolveActiveScale(profile), ResolveActiveDuration(profile));
                    break;
                case CombatAiPatternState.Death:
                    RequestCue(deathCue, ResolveDeathScale(profile), deathCue.durationSeconds);
                    break;
            }
        }

        private void RequestCue(
            ActionCameraCueProfile.CameraCue cue,
            float scale,
            float durationSeconds)
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
                offset * clampedScale,
                durationSeconds,
                cue.fieldOfViewDelta * clampedScale,
                cue.cameraDistanceDelta * clampedScale,
                cue.focusHeightDelta * clampedScale);
        }

        private void ResolveAgent()
        {
            agent = agentSource as ICombatAiAgent;
        }

        private ActionCameraCueProfile.CameraCue ResolveWindupCue(CombatAiPatternProfile profile)
        {
            return ResolveCameraCueKind(profile) switch
            {
                CombatAiCameraCueKind.LungeStrike => lungeWindupCue,
                CombatAiCameraCueKind.HeavyWindup => heavyWindupCue,
                _ => closeWindupCue
            };
        }

        private ActionCameraCueProfile.CameraCue ResolveActiveCue(CombatAiPatternProfile profile)
        {
            return ResolveCameraCueKind(profile) switch
            {
                CombatAiCameraCueKind.LungeStrike => lungeActiveCue,
                CombatAiCameraCueKind.HeavyWindup => heavyActiveCue,
                _ => closeActiveCue
            };
        }

        private static CombatAiCameraCueKind ResolveCameraCueKind(CombatAiPatternProfile profile)
        {
            return profile != null ? profile.CameraCueKind : CombatAiCameraCueKind.ClosePunish;
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

        private static float ResolveWindupDuration(CombatAiPatternProfile profile)
        {
            if (profile == null)
            {
                return 0.20f;
            }

            return Mathf.Clamp(profile.TelegraphSeconds * 0.35f, 0.14f, 0.32f);
        }

        private static float ResolveActiveDuration(CombatAiPatternProfile profile)
        {
            if (profile == null)
            {
                return 0.18f;
            }

            return Mathf.Clamp(profile.ActiveSeconds + 0.08f, 0.12f, 0.28f);
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
    }
}
