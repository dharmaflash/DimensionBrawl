using System.Collections;
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
        [SerializeField, Min(0.02f)] private float targetRefreshIntervalSeconds = 0.12f;

        private Coroutine targetRefreshRoutine;

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
            targetRefreshRoutine = StartCoroutine(RunTargetRefresh());
        }

        private void OnDisable()
        {
            if (targetSelector != null)
            {
                targetSelector.TargetChanged -= HandleTargetChanged;
            }

            if (targetRefreshRoutine != null)
            {
                StopCoroutine(targetRefreshRoutine);
                targetRefreshRoutine = null;
            }
        }

        private void HandleTargetChanged(CombatHealth _)
        {
            ApplyTargets();
        }

        private IEnumerator RunTargetRefresh()
        {
            var wait = new WaitForSeconds(Mathf.Max(0.02f, targetRefreshIntervalSeconds));
            while (true)
            {
                yield return wait;
                ApplyTargets();
            }
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
