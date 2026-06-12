using DimensionBrawl.Combat;
using DimensionBrawl.Player;
using UnityEngine;

namespace DimensionBrawl.Presentation
{
    [DisallowMultipleComponent]
    public sealed class ActionCameraTargetBridge : MonoBehaviour
    {
        [SerializeField] private ActionCameraController cameraController;
        [SerializeField] private PlayerCombatTargetSelector targetSelector;
        [SerializeField] private Transform followTarget;

        public ActionCameraController CameraController => cameraController;
        public PlayerCombatTargetSelector TargetSelector => targetSelector;
        public Transform FollowTarget => followTarget;

        private void Awake()
        {
            if (cameraController == null)
            {
                cameraController = GetComponent<ActionCameraController>();
            }
        }

        private void OnEnable()
        {
            if (targetSelector != null)
            {
                targetSelector.TargetChanged += HandleTargetChanged;
            }

            ApplyTargets();
        }

        private void OnDisable()
        {
            if (targetSelector != null)
            {
                targetSelector.TargetChanged -= HandleTargetChanged;
            }
        }

        private void LateUpdate()
        {
            ApplyTargets();
        }

        private void HandleTargetChanged(CombatHealth _)
        {
            ApplyTargets();
        }

        private void ApplyTargets()
        {
            if (cameraController == null || followTarget == null)
            {
                return;
            }

            Transform threat = null;
            if (targetSelector != null
                && targetSelector.TryGetCurrentTarget(out Transform selectedThreat, out CombatHealth _))
            {
                threat = selectedThreat;
            }

            cameraController.ConfigureTargets(followTarget, threat);
        }
    }
}
