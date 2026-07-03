using System.Collections.Generic;
using DimensionBrawl.Player;
using UnityEngine;

namespace DimensionBrawl.Presentation
{
    [DisallowMultipleComponent]
    public sealed class RifleGirlNativeGameplayAnimatorBridge : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Animator animator;
        [SerializeField] private PlayerMovementController movement;
        [SerializeField] private PlayerActionController actionController;
        [SerializeField] private PlayerCombatModeController combatModeController;
        [SerializeField] private PlayerRangedAimController rangedAimController;
        [SerializeField] private PlayerRangedBasicAttackAction rangedBasicAttackAction;

        [Header("Native RifleGirl Triggers")]
        [SerializeField] private string normalIdleTrigger = "IDLE";
        [SerializeField] private string normalWalkTrigger = "WALK";
        [SerializeField] private string normalRunTrigger = "RUN";
        [SerializeField] private string idleTrigger = "IDLE 0";
        [SerializeField] private string shootTrigger = "SHOOT";
        [SerializeField] private string autoShootTrigger = "AUTO SHOOT";
        [SerializeField] private string reloadTrigger = "RELOAD";
        [SerializeField] private string jogTrigger = "JOG";
        [SerializeField] private string walkForwardTrigger = "WALK F";
        [SerializeField] private string walkBackTrigger = "WALK B";
        [SerializeField] private string walkForwardLeftTrigger = "WALK FL";
        [SerializeField] private string walkForwardRightTrigger = "WALK FR";
        [SerializeField] private string walkBackLeftTrigger = "WALK BL";
        [SerializeField] private string walkBackRightTrigger = "WALK BR";
        [SerializeField] private string dodgeTrigger = "EVADE";

        [Header("Promoted Animator Parameters")]
        [SerializeField] private string moveSpeedParameter = "MoveSpeed";
        [SerializeField] private string moveXParameter = "MoveX";
        [SerializeField] private string moveYParameter = "MoveY";
        [SerializeField] private string aimingParameter = "IsAiming";
        [SerializeField, Min(0f)] private float movementDampSeconds = 0.06f;

        [Header("Locomotion Read")]
        [SerializeField, Min(0f)] private float movingSpeedThreshold = 0.08f;
        [SerializeField, Min(0f)] private float normalRunSpeedThreshold = 2.4f;
        [SerializeField, Min(0f)] private float aimJogSpeedThreshold = 2.8f;
        [SerializeField, Range(0f, 1f)] private float diagonalThreshold = 0.35f;
        [SerializeField, Min(0f)] private float autoFireWindowSeconds = 0.38f;
        [SerializeField, Min(0f)] private float fireAimPoseLingerSeconds = 0.22f;
        [SerializeField] private bool useNativeAutoShootLoop = true;
        [SerializeField] private bool triggerAutoShootOncePerHold = true;
        [SerializeField, Min(0f)] private float stationaryFirePoseHoldSeconds = 0.36f;
        [SerializeField, Min(0f)] private float reloadPoseHoldSeconds = 0.62f;
        [SerializeField] private bool keepMovingLocomotionDuringFire = true;
        [SerializeField, Min(0f)] private float locomotionTriggerHoldSeconds = 0.18f;
        [SerializeField, Min(0f)] private float dodgePoseSuppressSeconds = 0.42f;

        private float lastFireTime = -100f;
        private float aimPoseUntil;
        private float moveTriggerSuppressedUntil;
        private string lastLocomotionTrigger = string.Empty;
        private float lastLocomotionTriggerTime = -100f;
        private bool autoShootLoopTriggered;
        private readonly HashSet<string> cachedParameters = new HashSet<string>();
        private RuntimeAnimatorController cachedController;

        public void Configure(
            Animator newAnimator,
            PlayerMovementController newMovement,
            PlayerActionController newActionController,
            PlayerCombatModeController newCombatModeController,
            PlayerRangedAimController newRangedAimController,
            PlayerRangedBasicAttackAction newRangedBasicAttackAction)
        {
            animator = newAnimator;
            movement = newMovement;
            actionController = newActionController;
            combatModeController = newCombatModeController;
            rangedAimController = newRangedAimController;
            rangedBasicAttackAction = newRangedBasicAttackAction;
        }

        private void Awake()
        {
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }
        }

        private void OnEnable()
        {
            if (rangedBasicAttackAction != null)
            {
                rangedBasicAttackAction.RangedFireStarted += HandleRangedFireStarted;
                rangedBasicAttackAction.RangedReloadStarted += HandleRangedReloadStarted;
                rangedBasicAttackAction.RangedReloadCanceled += HandleRangedReloadCanceled;
            }

            if (actionController != null)
            {
                actionController.DodgeStarted += HandleDodgeStarted;
            }

            if (rangedAimController != null)
            {
                rangedAimController.AimModeChanged += HandleAimModeChanged;
            }

            if (combatModeController != null)
            {
                combatModeController.CombatModeChanged += HandleCombatModeChanged;
            }

            ApplyMovementParameters(true);
            ApplyNativeLocomotion(true);
        }

        private void OnDisable()
        {
            if (rangedBasicAttackAction != null)
            {
                rangedBasicAttackAction.RangedFireStarted -= HandleRangedFireStarted;
                rangedBasicAttackAction.RangedReloadStarted -= HandleRangedReloadStarted;
                rangedBasicAttackAction.RangedReloadCanceled -= HandleRangedReloadCanceled;
            }

            if (actionController != null)
            {
                actionController.DodgeStarted -= HandleDodgeStarted;
            }

            if (rangedAimController != null)
            {
                rangedAimController.AimModeChanged -= HandleAimModeChanged;
            }

            if (combatModeController != null)
            {
                combatModeController.CombatModeChanged -= HandleCombatModeChanged;
            }
        }

        private void Update()
        {
            UpdateContinuousFireState();

            if (animator == null || movement == null || (combatModeController != null && !combatModeController.IsRangedMode))
            {
                return;
            }

            if (Time.time < moveTriggerSuppressedUntil)
            {
                return;
            }

            ApplyMovementParameters(false);
            ApplyNativeLocomotion(false);
        }

        private void ApplyMovementParameters(bool immediate)
        {
            if (animator == null)
            {
                return;
            }

            Vector3 planarVelocity = movement != null
                ? Vector3.ProjectOnPlane(movement.PlanarVelocity, Vector3.up)
                : Vector3.zero;
            float speed = planarVelocity.magnitude;
            Vector3 localDirection = Vector3.zero;
            if (movement != null && speed > movingSpeedThreshold)
            {
                localDirection = movement.transform.InverseTransformDirection(planarVelocity.normalized);
            }

            float damp = immediate ? 0f : movementDampSeconds;
            SetFloat(moveSpeedParameter, speed, damp);
            SetFloat(moveXParameter, localDirection.x, damp);
            SetFloat(moveYParameter, localDirection.z, damp);
            SetBool(aimingParameter, IsAimPoseActive);
        }

        private void HandleRangedFireStarted()
        {
            if (animator == null || !IsRangedModeActive)
            {
                return;
            }

            float now = Time.time;
            bool isAutoFire = now - lastFireTime <= autoFireWindowSeconds;
            lastFireTime = now;
            bool keepLocomotion = keepMovingLocomotionDuringFire && IsMovingForNativeLocomotion();
            float poseHoldSeconds = keepLocomotion
                ? fireAimPoseLingerSeconds
                : Mathf.Max(fireAimPoseLingerSeconds, stationaryFirePoseHoldSeconds);
            aimPoseUntil = now + poseHoldSeconds;
            ApplyMovementParameters(!keepLocomotion);

            if (keepLocomotion)
            {
                autoShootLoopTriggered = false;
                moveTriggerSuppressedUntil = 0f;
                ApplyNativeLocomotion(false);
                return;
            }

            moveTriggerSuppressedUntil = aimPoseUntil;
            if (TriggerStationaryFire(isAutoFire))
            {
                ResetLastLocomotionTrigger();
            }
        }

        private void HandleRangedReloadStarted()
        {
            if (animator == null || !IsRangedModeActive)
            {
                return;
            }

            float now = Time.time;
            float poseHoldSeconds = Mathf.Max(fireAimPoseLingerSeconds, reloadPoseHoldSeconds);
            aimPoseUntil = Mathf.Max(aimPoseUntil, now + poseHoldSeconds);
            moveTriggerSuppressedUntil = Mathf.Max(moveTriggerSuppressedUntil, now + poseHoldSeconds);
            autoShootLoopTriggered = false;
            ApplyMovementParameters(true);
            if (Trigger(reloadTrigger))
            {
                ResetLastLocomotionTrigger();
            }
        }

        private void HandleRangedReloadCanceled()
        {
            if (animator == null || !IsRangedModeActive)
            {
                return;
            }

            moveTriggerSuppressedUntil = 0f;
            autoShootLoopTriggered = false;
            ApplyMovementParameters(true);
            ApplyNativeLocomotion(true);
        }

        private void HandleAimModeChanged(bool isAiming)
        {
            if (!isAiming)
            {
                aimPoseUntil = 0f;
            }

            ApplyMovementParameters(true);
            ApplyNativeLocomotion(true);
        }

        private void HandleCombatModeChanged(PlayerCombatMode combatMode)
        {
            aimPoseUntil = 0f;
            moveTriggerSuppressedUntil = 0f;
            autoShootLoopTriggered = false;
            ResetLastLocomotionTrigger();
            if (combatMode == PlayerCombatMode.Ranged)
            {
                ApplyMovementParameters(true);
                ApplyNativeLocomotion(true);
            }
        }

        private void HandleDodgeStarted()
        {
            if (!IsRangedModeActive)
            {
                return;
            }

            moveTriggerSuppressedUntil = Time.time + dodgePoseSuppressSeconds;
            autoShootLoopTriggered = false;
            ResetLastLocomotionTrigger();
            Trigger(dodgeTrigger);
        }

        private bool IsRangedModeActive => combatModeController == null || combatModeController.IsRangedMode;

        private bool IsAimPoseActive => (rangedAimController != null && rangedAimController.IsAiming)
            || Time.time < aimPoseUntil;

        private bool IsMovingForNativeLocomotion()
        {
            if (movement == null)
            {
                return false;
            }

            Vector3 planarVelocity = Vector3.ProjectOnPlane(movement.PlanarVelocity, Vector3.up);
            float threshold = movingSpeedThreshold * movingSpeedThreshold;
            return planarVelocity.sqrMagnitude > threshold;
        }

        private void UpdateContinuousFireState()
        {
            if (rangedBasicAttackAction != null
                && rangedBasicAttackAction.IsFireHeld)
            {
                return;
            }

            if (Time.time - lastFireTime > autoFireWindowSeconds)
            {
                autoShootLoopTriggered = false;
            }
        }

        private bool TriggerStationaryFire(bool isAutoFire)
        {
            if (triggerAutoShootOncePerHold && isAutoFire)
            {
                if (!useNativeAutoShootLoop)
                {
                    return false;
                }

                if (autoShootLoopTriggered)
                {
                    return false;
                }

                autoShootLoopTriggered = Trigger(autoShootTrigger);
                return autoShootLoopTriggered;
            }

            autoShootLoopTriggered = false;
            string trigger = isAutoFire ? autoShootTrigger : shootTrigger;
            return Trigger(trigger);
        }

        private void ApplyNativeLocomotion(bool force)
        {
            if (animator == null || movement == null || !IsRangedModeActive)
            {
                return;
            }

            string trigger = ResolveLocomotionTrigger();
            bool triggerChanged = !string.Equals(trigger, lastLocomotionTrigger, System.StringComparison.Ordinal);
            if (!force && !triggerChanged)
            {
                return;
            }

            if (!force
                && triggerChanged
                && !string.IsNullOrEmpty(lastLocomotionTrigger)
                && Time.time - lastLocomotionTriggerTime < locomotionTriggerHoldSeconds)
            {
                return;
            }

            if (Trigger(trigger))
            {
                lastLocomotionTrigger = trigger;
                lastLocomotionTriggerTime = Time.time;
            }
        }

        private string ResolveLocomotionTrigger()
        {
            Vector3 planarVelocity = Vector3.ProjectOnPlane(movement.PlanarVelocity, Vector3.up);
            float speed = planarVelocity.magnitude;
            bool aimPose = IsAimPoseActive;
            if (speed <= movingSpeedThreshold)
            {
                return aimPose ? idleTrigger : normalIdleTrigger;
            }

            if (!aimPose)
            {
                return speed >= normalRunSpeedThreshold ? normalRunTrigger : normalWalkTrigger;
            }

            Vector3 localDirection = movement.transform.InverseTransformDirection(planarVelocity.normalized);
            if (localDirection.z < -diagonalThreshold)
            {
                if (localDirection.x < -diagonalThreshold)
                {
                    return walkBackLeftTrigger;
                }

                if (localDirection.x > diagonalThreshold)
                {
                    return walkBackRightTrigger;
                }

                return walkBackTrigger;
            }

            if (localDirection.x < -diagonalThreshold)
            {
                return walkForwardLeftTrigger;
            }

            if (localDirection.x > diagonalThreshold)
            {
                return walkForwardRightTrigger;
            }

            return speed >= aimJogSpeedThreshold ? jogTrigger : walkForwardTrigger;
        }

        private void SetFloat(string parameterName, float value, float dampSeconds)
        {
            if (animator == null || string.IsNullOrWhiteSpace(parameterName) || !HasParameter(parameterName))
            {
                return;
            }

            if (dampSeconds <= 0f)
            {
                animator.SetFloat(parameterName, value);
                return;
            }

            animator.SetFloat(parameterName, value, dampSeconds, Time.deltaTime);
        }

        private void SetBool(string parameterName, bool value)
        {
            if (animator != null && !string.IsNullOrWhiteSpace(parameterName) && HasParameter(parameterName))
            {
                animator.SetBool(parameterName, value);
            }
        }

        private bool HasParameter(string parameterName)
        {
            RefreshParameterCacheIfNeeded();
            return cachedParameters.Contains(parameterName);
        }

        private void RefreshParameterCacheIfNeeded()
        {
            RuntimeAnimatorController controller = animator != null ? animator.runtimeAnimatorController : null;
            if (controller == cachedController)
            {
                return;
            }

            cachedController = controller;
            cachedParameters.Clear();
            if (animator == null)
            {
                return;
            }

            AnimatorControllerParameter[] parameters = animator.parameters;
            for (int i = 0; i < parameters.Length; i++)
            {
                cachedParameters.Add(parameters[i].name);
            }
        }

        private bool Trigger(string triggerName)
        {
            if (animator != null && !string.IsNullOrWhiteSpace(triggerName) && HasParameter(triggerName))
            {
                animator.SetTrigger(triggerName);
                return true;
            }

            return false;
        }

        private void ResetLastLocomotionTrigger()
        {
            lastLocomotionTrigger = string.Empty;
            lastLocomotionTriggerTime = -100f;
        }
    }
}
