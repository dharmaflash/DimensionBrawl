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

        [Tooltip("Uses the reference docs' lower recovery range around 0.10s as the first stop-settle cue length.")]
        [SerializeField, Min(0f)] private float stopSettleSeconds = 0.12f;

        [SerializeField] private bool cameraRelativeMovement = true;
        [SerializeField] private Camera referenceCamera;
        [SerializeField] private float gravity = -24f;

        [Header("Animation Requests")]
        [SerializeField] private Animator animator;
        [SerializeField] private string moveSpeedParameter = "MoveSpeed";
        [SerializeField] private string moveXParameter = "MoveX";
        [SerializeField] private string moveYParameter = "MoveY";
        [SerializeField] private string stoppingParameter = "IsStopping";

        private CharacterController characterController;
        private Vector3 planarVelocity;
        private Vector3 moveIntentDirection;
        private float verticalVelocity;
        private Vector3 externalPlanarVelocity;
        private float externalPlanarTimer;
        private float stopSettleTimer;
        private bool inputWasMoving;
        private bool enabledMoveAction;
        private bool enabledLookAction;

        public Vector3 PlanarVelocity => planarVelocity;
        public Vector3 MoveIntentDirection => moveIntentDirection;
        public bool IsStopSettling => stopSettleTimer > 0f;
        public Vector3 FacingDirection => transform.forward;

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
            externalPlanarTimer = Mathf.Max(0f, durationSeconds);
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
            UpdateMoveIntent(desiredMoveDirection);
            UpdatePlanarVelocity(desiredMoveDirection, moveInput.magnitude, deltaTime);
            UpdateFacing(desiredMoveDirection, lookInput, deltaTime);
            UpdateStopSettle(moveInput.sqrMagnitude > 0f, deltaTime);
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
            Vector2 input = ReadSharedInput(lookAction, mobileLookInput);

            if (useDeviceFallbackWhenActionMissing && IsActionMissing(lookAction) && input.sqrMagnitude <= 0f)
            {
                input = ReadLookDeviceFallback();
            }

            return input;
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

        private static Vector2 ReadLookDeviceFallback()
        {
            if (Gamepad.current == null)
            {
                return Vector2.zero;
            }

            return Vector2.ClampMagnitude(Gamepad.current.rightStick.ReadValue(), 1f);
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

        private void UpdateStopSettle(bool inputIsMoving, float deltaTime)
        {
            if (inputWasMoving && !inputIsMoving)
            {
                stopSettleTimer = stopSettleSeconds;
            }

            if (stopSettleTimer > 0f)
            {
                stopSettleTimer = Mathf.Max(0f, stopSettleTimer - deltaTime);
            }

            inputWasMoving = inputIsMoving;
        }

        private void MoveCharacter(float deltaTime)
        {
            if (characterController.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -1f;
            }

            verticalVelocity += gravity * deltaTime;
            Vector3 motion = (externalPlanarTimer > 0f ? externalPlanarVelocity : planarVelocity) + Vector3.up * verticalVelocity;
            characterController.Move(motion * deltaTime);
        }

        private void UpdateExternalPlanarBurst(float deltaTime)
        {
            if (externalPlanarTimer <= 0f)
            {
                return;
            }

            externalPlanarTimer = Mathf.Max(0f, externalPlanarTimer - deltaTime);
        }

        private void UpdateAnimation(Vector2 moveInput)
        {
            if (animator == null)
            {
                return;
            }

            float normalizedSpeed = moveSpeed > 0f ? planarVelocity.magnitude / moveSpeed : 0f;
            SetAnimatorFloat(moveSpeedParameter, normalizedSpeed);
            SetAnimatorFloat(moveXParameter, moveInput.x);
            SetAnimatorFloat(moveYParameter, moveInput.y);
            SetAnimatorBool(stoppingParameter, IsStopSettling);
        }

        private void SetAnimatorFloat(string parameterName, float value)
        {
            if (!string.IsNullOrWhiteSpace(parameterName))
            {
                animator.SetFloat(parameterName, value);
            }
        }

        private void SetAnimatorBool(string parameterName, bool value)
        {
            if (!string.IsNullOrWhiteSpace(parameterName))
            {
                animator.SetBool(parameterName, value);
            }
        }
    }
}
