using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DimensionBrawl.Player
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerMovementController : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private InputActionReference lookAction;

        [Tooltip("Shared mobile HUD hook for the canonical Move action. PC/gamepad input still uses moveAction.")]
        [SerializeField] private Vector2 mobileMoveInput;

        [Tooltip("Shared mobile HUD hook for the canonical Look/TargetBias action.")]
        [SerializeField] private Vector2 mobileLookInput;

        [Tooltip("Editor test fallback only. Serialized InputActionReference still takes priority when assigned.")]
        [SerializeField] private bool useDeviceFallbackWhenActionMissing = true;

        [Tooltip("Uses the reference docs' 0.08-0.12s input-buffer range as a starting dead zone scale.")]
        [SerializeField, Range(0f, 0.5f)] private float inputDeadZone = 0.1f;

        [Header("Movement")]
        [Tooltip("Temporary exposed tuning value. No collected movement-speed default exists yet.")]
        [SerializeField, Min(0f)] private float moveSpeed = 5.5f;

        [Tooltip("Temporary exposed tuning value. Kept visible so movement feel is not hidden in code.")]
        [SerializeField, Min(0f)] private float acceleration = 28f;

        [Tooltip("Temporary exposed tuning value. Deceleration starts higher than acceleration to support a crisp stop-settle feel.")]
        [SerializeField, Min(0f)] private float deceleration = 38f;

        [Tooltip("Degrees per second. Exposed for action-feel tuning and readable direction changes.")]
        [SerializeField, Min(0f)] private float turnRateDegrees = 720f;

        [SerializeField, Min(0f)] private float stopThreshold = 0.08f;

        [Tooltip("Uses the reference docs' 0.18-0.35s light settle/stagger range as a first visible foot-settle cue length.")]
        [SerializeField, Min(0f)] private float stopSettleSeconds = 0.26f;

        [Tooltip("Keeps the final movement direction alive briefly so stop-settle reads like a foot plant instead of a hard idle snap.")]
        [SerializeField, Min(0f)] private float stopSettleInputHoldSeconds = 0.16f;

        [SerializeField] private bool cameraRelativeMovement = true;
        [SerializeField] private Camera referenceCamera;
        [SerializeField] private float gravity = -24f;

        [Header("Animation Requests")]
        [SerializeField] private Animator animator;
        [SerializeField] private string moveSpeedParameter = "MoveSpeed";
        [SerializeField] private string moveXParameter = "MoveX";
        [SerializeField] private string moveYParameter = "MoveY";
        [SerializeField] private string stoppingParameter = "IsStopping";
        [SerializeField] private string startRunTrigger = "StartRun";
        [SerializeField] private string stopStepTrigger = "StopStep";
        [SerializeField] private string turnLeft90Trigger = "TurnLeft90";
        [SerializeField] private string turnRight90Trigger = "TurnRight90";
        [Tooltip("Uses the reference 0.08-0.12s action feel range to soften visual run-to-idle without slowing input response.")]
        [SerializeField, Min(0f)] private float animatorMoveDampSeconds = 0.06f;
        [Tooltip("Keeps the run animation alive briefly during stop-settle so the CombatGirl visual reads a small foot-settle instead of an instant idle snap.")]
        [SerializeField, Range(0f, 1f)] private float stopSettleAnimatorSpeedFloor = 0.24f;
        [Tooltip("Large direction changes request imported 90-degree turn clips instead of relying only on code rotation.")]
        [SerializeField, Range(0f, 180f)] private float sharpTurnTriggerAngle = 65f;
        [SerializeField, Range(0f, 1f)] private float sharpTurnMinimumSpeedRatio = 0.35f;
        [SerializeField, Min(0f)] private float sharpTurnCooldownSeconds = 0.32f;

        private CharacterController characterController;
        private Vector3 planarVelocity;
        private Vector3 moveIntentDirection;
        private Vector3 currentMoveDirection;
        private Vector2 currentMoveInput;
        private float verticalVelocity;
        private Vector3 externalPlanarVelocity;
        private float externalPlanarDuration;
        private float externalPlanarTimer;
        private float stopSettleTimer;
        private float stopSettleInputHoldTimer;
        private float sharpTurnCooldownTimer;
        private Vector2 lastMoveInput;
        private Vector2 stopSettleHeldMoveInput;
        private Vector3 requestedFacingDirection;
        private float requestedFacingTimer;
        private bool inputWasMoving;
        private bool enabledMoveAction;
        private bool enabledLookAction;

        public Vector3 PlanarVelocity => planarVelocity;
        public Vector3 MoveIntentDirection => moveIntentDirection;
        public Vector3 CurrentMoveDirection => currentMoveDirection;
        public bool HasMoveInput => currentMoveInput.sqrMagnitude > 0f;
        public bool IsStopSettling => stopSettleTimer > 0f;
        public Vector3 FacingDirection => transform.forward;

        public event Action RunStarted;
        public event Action StopSettleStarted;
        public event Action<float> SharpTurnStarted;

        public void SetMoveInput(Vector2 input)
        {
            mobileMoveInput = Vector2.ClampMagnitude(input, 1f);
        }

        public void SetLookInput(Vector2 input)
        {
            mobileLookInput = Vector2.ClampMagnitude(input, 1f);
        }

        public void BeginExternalPlanarBurst(Vector3 velocity, float durationSeconds)
        {
            externalPlanarVelocity = Vector3.ProjectOnPlane(velocity, Vector3.up);
            externalPlanarDuration = Mathf.Max(0f, durationSeconds);
            externalPlanarTimer = externalPlanarDuration;
        }

        public void RequestFacingDirection(Vector3 direction, float holdSeconds, bool snapImmediately)
        {
            Vector3 planarDirection = Vector3.ProjectOnPlane(direction, Vector3.up);
            if (planarDirection.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            requestedFacingDirection = planarDirection.normalized;
            requestedFacingTimer = Mathf.Max(requestedFacingTimer, holdSeconds);

            if (snapImmediately)
            {
                transform.rotation = Quaternion.LookRotation(requestedFacingDirection, Vector3.up);
            }
        }

        public bool TryGetCurrentMoveDirection(out Vector3 direction)
        {
            Vector2 moveInput = ApplyDeadZone(ReadMoveInput());
            Vector3 desiredMoveDirection = BuildWorldDirection(moveInput);
            UpdateCurrentMoveInput(moveInput, desiredMoveDirection);
            direction = currentMoveDirection;
            return HasMoveInput;
        }

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
        }

        private void OnEnable()
        {
            enabledMoveAction = EnableActionIfNeeded(moveAction);
            enabledLookAction = EnableActionIfNeeded(lookAction);
        }

        private void OnDisable()
        {
            DisableActionIfOwned(moveAction, enabledMoveAction);
            DisableActionIfOwned(lookAction, enabledLookAction);
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            Vector2 moveInput = ApplyDeadZone(ReadMoveInput());
            Vector2 lookInput = ApplyDeadZone(ReadLookInput());

            Vector3 desiredMoveDirection = BuildWorldDirection(moveInput);
            UpdateCurrentMoveInput(moveInput, desiredMoveDirection);
            UpdateMoveIntent(desiredMoveDirection);
            UpdatePlanarVelocity(desiredMoveDirection, moveInput.magnitude, deltaTime);
            UpdateFacing(desiredMoveDirection, lookInput, deltaTime);
            UpdateStopSettle(moveInput, deltaTime);
            UpdateSharpTurnCooldown(deltaTime);
            UpdateExternalPlanarBurst(deltaTime);
            MoveCharacter(deltaTime);
            UpdateAnimation(moveInput);
        }

        private static bool EnableActionIfNeeded(InputActionReference actionReference)
        {
            if (actionReference == null || actionReference.action == null || actionReference.action.enabled)
            {
                return false;
            }

            actionReference.action.Enable();
            return true;
        }

        private static void DisableActionIfOwned(InputActionReference actionReference, bool enabledHere)
        {
            if (enabledHere && actionReference != null && actionReference.action != null)
            {
                actionReference.action.Disable();
            }
        }

        private Vector2 ReadMoveInput()
        {
            Vector2 input = ReadSharedInput(moveAction, mobileMoveInput);

            if (useDeviceFallbackWhenActionMissing && IsActionMissing(moveAction) && input.sqrMagnitude <= 0f)
            {
                input = ReadMoveDeviceFallback();
            }

            return input;
        }

        private Vector2 ReadLookInput()
        {
            return ReadSharedInput(lookAction, mobileLookInput);
        }

        private static Vector2 ReadSharedInput(InputActionReference actionReference, Vector2 mobileInput)
        {
            Vector2 actionInput = Vector2.zero;

            if (actionReference != null && actionReference.action != null)
            {
                actionInput = actionReference.action.ReadValue<Vector2>();
            }

            return mobileInput.sqrMagnitude > actionInput.sqrMagnitude ? mobileInput : actionInput;
        }

        private static bool IsActionMissing(InputActionReference actionReference)
        {
            return actionReference == null || actionReference.action == null;
        }

        private static Vector2 ReadMoveDeviceFallback()
        {
            Vector2 input = Vector2.zero;

            if (Keyboard.current != null)
            {
                input.x += Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed ? 1f : 0f;
                input.x -= Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed ? 1f : 0f;
                input.y += Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed ? 1f : 0f;
                input.y -= Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed ? 1f : 0f;
            }

            if (Gamepad.current != null)
            {
                Vector2 gamepadInput = Gamepad.current.leftStick.ReadValue();
                if (gamepadInput.sqrMagnitude > input.sqrMagnitude)
                {
                    input = gamepadInput;
                }
            }

            return Vector2.ClampMagnitude(input, 1f);
        }

        private Vector2 ApplyDeadZone(Vector2 input)
        {
            if (input.sqrMagnitude < inputDeadZone * inputDeadZone)
            {
                return Vector2.zero;
            }

            return Vector2.ClampMagnitude(input, 1f);
        }

        private Vector3 BuildWorldDirection(Vector2 input)
        {
            if (input.sqrMagnitude <= 0f)
            {
                return Vector3.zero;
            }

            Vector3 forward = Vector3.forward;
            Vector3 right = Vector3.right;

            if (cameraRelativeMovement && referenceCamera != null)
            {
                Transform cameraTransform = referenceCamera.transform;
                forward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
                right = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;
            }

            Vector3 direction = forward * input.y + right * input.x;
            return direction.sqrMagnitude > 1f ? direction.normalized : direction;
        }

        private void UpdateCurrentMoveInput(Vector2 moveInput, Vector3 desiredDirection)
        {
            currentMoveInput = moveInput;
            currentMoveDirection = desiredDirection.sqrMagnitude > 0f ? desiredDirection.normalized : Vector3.zero;
        }

        private void UpdatePlanarVelocity(Vector3 desiredDirection, float inputAmount, float deltaTime)
        {
            Vector3 desiredVelocity = desiredDirection * (moveSpeed * inputAmount);
            float rate = desiredVelocity.sqrMagnitude > planarVelocity.sqrMagnitude ? acceleration : deceleration;
            planarVelocity = Vector3.MoveTowards(planarVelocity, desiredVelocity, rate * deltaTime);

            if (desiredVelocity.sqrMagnitude <= 0f && planarVelocity.magnitude <= stopThreshold)
            {
                planarVelocity = Vector3.zero;
            }
        }

        private void UpdateMoveIntent(Vector3 desiredDirection)
        {
            if (desiredDirection.sqrMagnitude > 0f)
            {
                moveIntentDirection = desiredDirection.normalized;
            }
        }

        private void UpdateFacing(Vector3 desiredMoveDirection, Vector2 lookInput, float deltaTime)
        {
            Vector3 facingDirection = BuildWorldDirection(lookInput);

            TryRequestSharpTurn(desiredMoveDirection);

            if (requestedFacingTimer > 0f)
            {
                requestedFacingTimer = Mathf.Max(0f, requestedFacingTimer - deltaTime);
                facingDirection = requestedFacingDirection;
            }

            if (facingDirection.sqrMagnitude <= 0f)
            {
                facingDirection = desiredMoveDirection;
            }

            if (facingDirection.sqrMagnitude <= 0f)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(facingDirection.normalized, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                turnRateDegrees * deltaTime);
        }

        private void UpdateStopSettle(Vector2 moveInput, float deltaTime)
        {
            bool inputIsMoving = moveInput.sqrMagnitude > 0f;
            if (inputIsMoving)
            {
                lastMoveInput = moveInput;
            }

            if (!inputWasMoving && inputIsMoving)
            {
                stopSettleTimer = 0f;
                stopSettleInputHoldTimer = 0f;
                stopSettleHeldMoveInput = Vector2.zero;
                SetAnimatorTriggerIfLocomotionState(startRunTrigger, "Idle", "StopStep");
                RunStarted?.Invoke();
            }

            if (inputWasMoving && !inputIsMoving)
            {
                stopSettleTimer = stopSettleSeconds;
                stopSettleInputHoldTimer = stopSettleInputHoldSeconds;
                stopSettleHeldMoveInput = lastMoveInput.sqrMagnitude > 0f ? lastMoveInput.normalized : Vector2.up;
                if (stopSettleSeconds > 0f)
                {
                    SetAnimatorTrigger(stopStepTrigger);
                    StopSettleStarted?.Invoke();
                }
            }

            if (stopSettleTimer > 0f)
            {
                stopSettleTimer = Mathf.Max(0f, stopSettleTimer - deltaTime);
            }

            if (stopSettleInputHoldTimer > 0f)
            {
                stopSettleInputHoldTimer = Mathf.Max(0f, stopSettleInputHoldTimer - deltaTime);
                if (stopSettleInputHoldTimer <= 0f)
                {
                    stopSettleHeldMoveInput = Vector2.zero;
                }
            }

            inputWasMoving = inputIsMoving;
        }

        private void TryRequestSharpTurn(Vector3 desiredMoveDirection)
        {
            if (desiredMoveDirection.sqrMagnitude <= 0.0001f || sharpTurnCooldownTimer > 0f)
            {
                return;
            }

            float speedRatio = moveSpeed > 0f ? planarVelocity.magnitude / moveSpeed : 0f;
            if (speedRatio < sharpTurnMinimumSpeedRatio)
            {
                return;
            }

            float signedAngle = Vector3.SignedAngle(transform.forward, desiredMoveDirection.normalized, Vector3.up);
            if (Mathf.Abs(signedAngle) < sharpTurnTriggerAngle)
            {
                return;
            }

            if (!AnimatorIsInBaseState("Run"))
            {
                return;
            }

            SetAnimatorTrigger(signedAngle < 0f ? turnLeft90Trigger : turnRight90Trigger);
            sharpTurnCooldownTimer = sharpTurnCooldownSeconds;
            SharpTurnStarted?.Invoke(signedAngle);
        }

        private void UpdateSharpTurnCooldown(float deltaTime)
        {
            if (sharpTurnCooldownTimer > 0f)
            {
                sharpTurnCooldownTimer = Mathf.Max(0f, sharpTurnCooldownTimer - deltaTime);
            }
        }

        private void MoveCharacter(float deltaTime)
        {
            if (characterController.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -1f;
            }

            verticalVelocity += gravity * deltaTime;
            Vector3 motion = (externalPlanarTimer > 0f ? ResolveExternalPlanarVelocity() : planarVelocity) + Vector3.up * verticalVelocity;
            characterController.Move(motion * deltaTime);
        }

        private Vector3 ResolveExternalPlanarVelocity()
        {
            if (externalPlanarDuration <= 0f)
            {
                return Vector3.zero;
            }

            float remainingRatio = Mathf.Clamp01(externalPlanarTimer / externalPlanarDuration);
            float speedWeight = Mathf.SmoothStep(0f, 1f, remainingRatio);
            return externalPlanarVelocity * speedWeight;
        }

        private void UpdateExternalPlanarBurst(float deltaTime)
        {
            if (externalPlanarTimer <= 0f)
            {
                return;
            }

            externalPlanarTimer = Mathf.Max(0f, externalPlanarTimer - deltaTime);
            if (externalPlanarTimer <= 0f)
            {
                externalPlanarVelocity = Vector3.zero;
                externalPlanarDuration = 0f;
            }
        }

        private void UpdateAnimation(Vector2 moveInput)
        {
            if (animator == null)
            {
                return;
            }

            float normalizedSpeed = moveSpeed > 0f ? planarVelocity.magnitude / moveSpeed : 0f;
            if (IsStopSettling)
            {
                normalizedSpeed = Mathf.Max(normalizedSpeed, stopSettleAnimatorSpeedFloor);
            }

            Vector2 animationMoveInput = stopSettleInputHoldTimer > 0f && moveInput.sqrMagnitude <= 0f
                ? stopSettleHeldMoveInput
                : moveInput;

            SetAnimatorFloat(moveSpeedParameter, normalizedSpeed, animatorMoveDampSeconds);
            SetAnimatorFloat(moveXParameter, animationMoveInput.x, animatorMoveDampSeconds);
            SetAnimatorFloat(moveYParameter, animationMoveInput.y, animatorMoveDampSeconds);
            SetAnimatorBool(stoppingParameter, IsStopSettling);
        }

        private void SetAnimatorFloat(string parameterName, float value)
        {
            if (!string.IsNullOrWhiteSpace(parameterName))
            {
                animator.SetFloat(parameterName, value);
            }
        }

        private void SetAnimatorFloat(string parameterName, float value, float dampSeconds)
        {
            if (string.IsNullOrWhiteSpace(parameterName))
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

        private void SetAnimatorBool(string parameterName, bool value)
        {
            if (!string.IsNullOrWhiteSpace(parameterName))
            {
                animator.SetBool(parameterName, value);
            }
        }

        private void SetAnimatorTrigger(string parameterName)
        {
            if (!string.IsNullOrWhiteSpace(parameterName) && animator != null)
            {
                animator.SetTrigger(parameterName);
            }
        }

        private void SetAnimatorTriggerIfLocomotionState(string parameterName, string stateNameA, string stateNameB)
        {
            if (AnimatorIsInBaseState(stateNameA) || AnimatorIsInBaseState(stateNameB))
            {
                SetAnimatorTrigger(parameterName);
            }
        }

        private bool AnimatorIsInBaseState(string stateName)
        {
            if (animator == null || string.IsNullOrWhiteSpace(stateName))
            {
                return false;
            }

            if (animator.GetCurrentAnimatorStateInfo(0).IsName(stateName))
            {
                return true;
            }

            return animator.IsInTransition(0) && animator.GetNextAnimatorStateInfo(0).IsName(stateName);
        }
    }
}
