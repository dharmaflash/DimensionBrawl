using DimensionBrawl.Combat;
using DimensionBrawl.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DimensionBrawl.UI
{
    // Review-only mobile controls for the boss barrage lane slice; production HUD should be authored separately.
    [DefaultExecutionOrder(-50)]
    [DisallowMultipleComponent]
    public sealed class BossBarrageLaneReviewMobileHud : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerMovementController movement;
        [SerializeField] private PlayerActionController actionController;
        [SerializeField] private PlayerCombatModeController combatModeController;
        [SerializeField] private PlayerRangedAimController aimController;
        [SerializeField] private PlayerRangedBasicAttackAction rangedBasicAttackAction;
        [SerializeField] private PlayerLockTargetController lockTargetController;
        [SerializeField] private PlayerSkill1Action skill1Action;
        [SerializeField] private PlayerSummonSlot1Action summonSlot1Action;
        [SerializeField] private PlayerSupportSummonSlotAction summonSlot2Action;
        [SerializeField] private PlayerSupportSummonSlotAction summonSlot3Action;
        [SerializeField] private SummonEnergyLadder energyLadder;

        [Header("Canonical Actions")]
        [SerializeField] private string moveActionName = "Move";
        [SerializeField] private string basicDefenseActionName = "BasicDefenseAttack";
        [SerializeField] private string dodgeActionName = "Dodge";
        [SerializeField] private string skill1ActionName = "Skill1";
        [SerializeField] private string summonSlot1ActionName = "SummonSlot1";
        [SerializeField] private string summonSlot2ActionName = "SummonSlot2";
        [SerializeField] private string summonSlot3ActionName = "SummonSlot3";
        [SerializeField] private string rangedAimActionName = "RangedAim";
        [SerializeField] private string weaponSwapActionName = "WeaponSwap";

        [Header("Display")]
        [SerializeField] private bool showHud = true;
        [SerializeField] private bool drawHudVisuals = true;
        [SerializeField] private bool useSingleSummonButton;
        [SerializeField, Range(0f, 1f)] private float hudOpacity = 1f;
        [SerializeField, Min(40f)] private float buttonSize = 168f;
        [SerializeField, Min(0f)] private float buttonGap = 38f;
        [SerializeField, Min(0f)] private float margin = 96f;
        [SerializeField, Min(40f)] private float minimumActionButtonSize = 124f;
        [SerializeField, Min(0f)] private float minimumButtonGap = 30f;
        [SerializeField, Min(0f)] private float minimumTouchEdgeInset = 96f;
        [SerializeField, Range(0.2f, 0.65f)] private float summonButtonGroupCenterY01 = 0.42f;
        [SerializeField, Min(0.1f)] private float summonButtonGapMultiplier = 1.05f;
        [SerializeField, Range(0.5f, 2f)] private float scale = 1f;
        [SerializeField] private Color buttonColor = new Color(0.04f, 0.11f, 0.16f, 0.58f);
        [SerializeField] private Color heldButtonColor = new Color(0.18f, 0.84f, 1f, 0.72f);
        [SerializeField] private Color pendingButtonColor = new Color(0.04f, 0.08f, 0.11f, 0.44f);
        [SerializeField] private Color actionTextColor = Color.white;
        [SerializeField] private string summonSlot1Label = "SUMMON";
        [SerializeField] private string summonSlot2Label = "S2 LASER";
        [SerializeField] private string summonSlot3Label = "S3 DRAGON";
        [SerializeField] private string lockedSummonLabel = "NEXT";
        [SerializeField, Min(0f)] private float summonReadyPulseSeconds = 0.85f;

        [Header("Chrome")]
        [SerializeField] private Color fireAccentColor = new Color(1f, 0.74f, 0.34f, 1f);
        [SerializeField] private Color dodgeAccentColor = new Color(0.38f, 0.9f, 1f, 1f);
        [SerializeField] private Color skillAccentColor = new Color(1f, 0.86f, 0.46f, 1f);
        [SerializeField] private Color summonSlot1AccentColor = new Color(1f, 0.78f, 0.36f, 1f);
        [SerializeField] private Color summonSlot2AccentColor = new Color(0.42f, 0.9f, 1f, 1f);
        [SerializeField] private Color summonSlot3AccentColor = new Color(0.78f, 0.72f, 1f, 1f);

        [Header("Move Joystick")]
        [SerializeField, Min(48f)] private float moveJoystickRadius = 236f;
        [SerializeField, Min(16f)] private float moveJoystickKnobSize = 98f;
        [SerializeField, Range(0f, 0.95f)] private float moveJoystickDeadZone = 0.12f;
        [SerializeField, Min(1f)] private float moveJoystickTouchRadiusScale = 1.18f;
        [SerializeField, Min(48f)] private float minimumMoveJoystickRadius = 180f;
        [SerializeField, Min(16f)] private float minimumMoveJoystickKnobSize = 76f;

        [Header("Look Aim")]
        [SerializeField] private bool screenDragControlsAim = true;
        [SerializeField] private bool rightMouseDragControlsAim = true;
        [SerializeField] private bool leftMouseDragControlsAim;
        [SerializeField] private bool routeAimToMovementLook;
        [SerializeField] private bool keyboardPeekControlsAim = true;
        [SerializeField] private Key keyboardPeekLeftKey = Key.Q;
        [SerializeField] private Key keyboardPeekRightKey = Key.E;
        [SerializeField] private bool keyboardPeekRequiresActiveAim = true;
        [SerializeField, Range(0f, 1f)] private float lookAimDragDeadZone = 0.08f;
        [SerializeField, Min(0.0001f)] private float lookAimDragSensitivity = 0.00435f;
        [SerializeField, Range(0f, 1f)] private float lookAimScreenMinX;

        [Header("Review Reticle")]
        [SerializeField] private bool showFireAimReticle = true;
        [SerializeField] private bool fireAimReticleUsesScreenCenter = true;
        [SerializeField, Min(4f)] private float fireAimReticleSize = 34f;
        [SerializeField, Min(0f)] private float fireAimReticleGap = 9f;
        [SerializeField, Min(1f)] private float fireAimReticleThickness = 2f;
        [SerializeField] private Color fireAimReticleColor = new Color(0.82f, 0.96f, 1f, 0.88f);
        [SerializeField] private Color fireAimAssistReticleColor = new Color(0.38f, 1f, 0.72f, 0.96f);
        [SerializeField, Range(0f, 1f)] private float fireAimAssistGapTighten = 0.35f;
        [SerializeField, Range(0f, 1f)] private float fireAimAssistSizeBoost = 0.22f;
        [SerializeField, Range(0f, 1f)] private float fireAimAssistThicknessBoost = 0.45f;
        [SerializeField] private bool fireAimReticleFollowsAssist = true;
        [SerializeField, Min(0f)] private float fireAimAssistReticleMaxOffset = 96f;

        [Header("Lock Target")]
        [SerializeField] private bool showLockTargetMarker = true;
        [SerializeField, Min(8f)] private float lockTargetMarkerSize = 52f;
        [SerializeField, Min(0f)] private float lockTargetMarkerGap = 12f;
        [SerializeField, Min(1f)] private float lockTargetMarkerThickness = 3f;
        [SerializeField] private Color lockTargetMarkerColor = new Color(0.62f, 0.98f, 1f, 0.96f);
        [SerializeField] private Color hardLockTargetMarkerColor = new Color(1f, 0.86f, 0.32f, 1f);
        [SerializeField, Range(0f, 1f)] private float lockTargetMarkerPulseBoost = 0.18f;
        [SerializeField, Min(4f)] private float lockTargetCoreDotSize = 13f;
        [SerializeField, Min(6f)] private float lockTargetCoreHaloSize = 34f;
        [SerializeField] private Color lockTargetCoreColor = new Color(1f, 0.08f, 0.04f, 1f);
        [SerializeField] private Color hardLockTargetCoreColor = new Color(1f, 0.20f, 0.06f, 1f);

        private Rect moveJoystickRect;
        private Rect moveJoystickTouchRect;
        private Rect basicRect;
        private Rect aimRect;
        private Rect dodgeRect;
        private Rect swapRect;
        private Rect skillRect;
        private Rect summonSlot1Rect;
        private Rect summonSlot2Rect;
        private Rect summonSlot3Rect;
        private GUIStyle buttonStyle;
        private GUIStyle heldButtonStyle;
        private GUIStyle pendingButtonStyle;
        private Texture2D reticleTexture;
        private bool previousBasicHeld;
        private bool firePointerHeld;
        private bool firePointerPressed;
        private bool firePointerUsesMouse;
        private int firePointerTouchId = -1;
        private bool movePointerHeld;
        private bool movePointerUsesMouse;
        private int movePointerTouchId = -1;
        private Vector2 moveJoystickCenterGuiPoint;
        private Vector2 moveJoystickInput;
        private bool lookPointerHeld;
        private bool lookPointerUsesMouse;
        private bool lookPointerUsesRightMouse;
        private int lookPointerTouchId = -1;
        private Vector2 lookPointerCurrentGuiPoint;
        private Vector2 lookAimRawInput;
        private Vector2 lookAimInput;
        private bool hudLookAimActive;
        private bool previousSummonSlot1Ready;
        private bool previousSummonSlot2Ready;
        private bool previousSummonSlot3Ready;
        private float summonSlot1ReadyPulseTimer;
        private float summonSlot2ReadyPulseTimer;
        private float summonSlot3ReadyPulseTimer;

        public string MoveActionName => moveActionName;
        public string BasicDefenseActionName => basicDefenseActionName;
        public string DodgeActionName => dodgeActionName;
        public string Skill1ActionName => skill1ActionName;
        public string SummonSlot1ActionName => summonSlot1ActionName;
        public string SummonSlot2ActionName => summonSlot2ActionName;
        public string SummonSlot3ActionName => summonSlot3ActionName;
        public Rect MoveJoystickGuiRect => ResolveCurrentGuiRect(ref moveJoystickRect);
        public Rect MoveJoystickTouchGuiRect => ResolveCurrentGuiRect(ref moveJoystickTouchRect);
        public Rect BasicButtonGuiRect => ResolveCurrentGuiRect(ref basicRect);
        public Rect DodgeButtonGuiRect => ResolveCurrentGuiRect(ref dodgeRect);
        public Rect SwapButtonGuiRect => ResolveCurrentGuiRect(ref swapRect);
        public Rect SummonSlot1GuiRect => ResolveCurrentGuiRect(ref summonSlot1Rect);
        public Rect SummonSlot2GuiRect => ResolveCurrentGuiRect(ref summonSlot2Rect);
        public Rect SummonSlot3GuiRect => ResolveCurrentGuiRect(ref summonSlot3Rect);
        public Vector2 MoveJoystickScreenAnchor => ToScreenAnchor(MoveJoystickGuiRect.center);
        public Vector2 BasicButtonScreenAnchor => ToScreenAnchor(BasicButtonGuiRect.center);
        public Vector2 DodgeButtonScreenAnchor => ToScreenAnchor(DodgeButtonGuiRect.center);
        public Vector2 SwapButtonScreenAnchor => ToScreenAnchor(SwapButtonGuiRect.center);
        public string RangedAimActionName => rangedAimActionName;
        public string WeaponSwapActionName => weaponSwapActionName;
        public bool HasActiveReviewPointerInput => movePointerHeld || firePointerHeld || lookPointerHeld;
        public bool IsReviewLookAimActive => hudLookAimActive;
        public bool WasBasicFireHeldLastFrame => previousBasicHeld;
        public float HudScale => scale;
        public float HudOpacity => hudOpacity;
        public bool UseSingleSummonButton => useSingleSummonButton;

        public void SetHudScale(float value)
        {
            scale = Mathf.Clamp(value, 0.5f, 2f);
        }

        public void SetHudOpacity(float opacity)
        {
            hudOpacity = Mathf.Clamp01(opacity);
        }

        public void Configure(
            PlayerMovementController newMovement,
            PlayerActionController newActionController,
            PlayerCombatModeController newCombatModeController,
            PlayerRangedAimController newAimController,
            PlayerRangedBasicAttackAction newRangedBasicAttackAction,
            PlayerSkill1Action newSkill1Action,
            PlayerSummonSlot1Action newSummonSlot1Action,
            SummonEnergyLadder newEnergyLadder = null,
            PlayerSupportSummonSlotAction newSummonSlot2Action = null,
            PlayerSupportSummonSlotAction newSummonSlot3Action = null)
        {
            movement = newMovement;
            actionController = newActionController;
            combatModeController = newCombatModeController;
            aimController = newAimController;
            rangedBasicAttackAction = newRangedBasicAttackAction;
            skill1Action = newSkill1Action;
            summonSlot1Action = newSummonSlot1Action;
            energyLadder = newEnergyLadder;
            summonSlot2Action = newSummonSlot2Action;
            summonSlot3Action = newSummonSlot3Action;
        }

        public void SetLockTargetController(PlayerLockTargetController newLockTargetController)
        {
            lockTargetController = newLockTargetController;
        }

        private void Awake()
        {
            if (movement == null)
            {
                movement = FindFirstComponentOnSelfOrParent<PlayerMovementController>();
            }

            if (actionController == null && movement != null)
            {
                actionController = movement.GetComponent<PlayerActionController>();
            }

            if (combatModeController == null && movement != null)
            {
                combatModeController = movement.GetComponent<PlayerCombatModeController>();
            }

            if (aimController == null && movement != null)
            {
                aimController = movement.GetComponent<PlayerRangedAimController>();
            }

            if (rangedBasicAttackAction == null && movement != null)
            {
                rangedBasicAttackAction = movement.GetComponent<PlayerRangedBasicAttackAction>();
            }

            if (lockTargetController == null && movement != null)
            {
                lockTargetController = movement.GetComponent<PlayerLockTargetController>();
            }

            if (skill1Action == null && movement != null)
            {
                skill1Action = movement.GetComponent<PlayerSkill1Action>();
            }

            if (summonSlot1Action == null && movement != null)
            {
                summonSlot1Action = movement.GetComponent<PlayerSummonSlot1Action>();
            }

            if (summonSlot2Action == null && movement != null)
            {
                summonSlot2Action = FindSupportSummonAction(summonSlot2ActionName);
            }

            if (summonSlot3Action == null && movement != null)
            {
                summonSlot3Action = FindSupportSummonAction(summonSlot3ActionName);
            }

            if (energyLadder == null && movement != null)
            {
                energyLadder = movement.GetComponent<SummonEnergyLadder>();
            }
        }

        private void OnDisable()
        {
            ReleaseHudControls();
        }

        private void Update()
        {
            if (!showHud || hudOpacity <= 0.001f)
            {
                if (HasHeldReviewControl())
                {
                    ReleaseHudControls();
                }

                return;
            }

            BuildLayout();
            UpdateMovePointerState();
            UpdateFirePointerState();
            UpdateLookPointerState();

            bool lookPointerBlocksDeviceFallback = lookPointerHeld && (!lookPointerUsesMouse || !lookPointerUsesRightMouse);
            bool anyHudHeld = movePointerHeld
                || firePointerHeld
                || lookPointerBlocksDeviceFallback
                || IsHeld(dodgeRect)
                || IsHeld(swapRect)
                || IsHeld(skillRect)
                || IsHeld(summonSlot1Rect)
                || AreSupportSummonButtonsVisible() && (IsHeld(summonSlot2Rect) || IsHeld(summonSlot3Rect));

            if (anyHudHeld)
            {
                actionController?.SuppressBasicAttackDeviceFallbackThisFrame();
                rangedBasicAttackAction?.SuppressDeviceFallbackThisFrame();
            }

            Vector2 moveInput = ResolveMoveInput();
            movement?.SetMoveInput(moveInput);
            UpdateHudLookAim();

            bool basicHeld = firePointerHeld;
            bool basicPressed = firePointerPressed;
            if (combatModeController == null || combatModeController.IsRangedMode)
            {
                if (basicHeld || previousBasicHeld)
                {
                    rangedBasicAttackAction?.SetFireHeld(basicHeld);
                }

                if (basicPressed)
                {
                    rangedBasicAttackAction?.QueueFire();
                }
            }
            else
            {
                if (previousBasicHeld)
                {
                    rangedBasicAttackAction?.SetFireHeld(false);
                }

                if (basicPressed)
                {
                    actionController?.QueueBasicAttack();
                }
            }
            previousBasicHeld = basicHeld;

            if (IsPressed(dodgeRect))
            {
                actionController?.QueueDodge();
            }

            if (IsPressed(swapRect))
            {
                combatModeController?.QueueCombatModeSwap();
            }

            if (IsPressed(skillRect))
            {
                skill1Action?.QueueSkill1();
            }

            if (IsPressed(summonSlot1Rect))
            {
                summonSlot1Action?.QueueSummonSlot1();
            }

            if (AreSupportSummonButtonsVisible() && IsPressed(summonSlot2Rect))
            {
                summonSlot2Action?.QueueSummon();
            }

            if (AreSupportSummonButtonsVisible() && IsPressed(summonSlot3Rect))
            {
                summonSlot3Action?.QueueSummon();
            }

            UpdateSummonReadinessFeedback(Time.unscaledDeltaTime);
        }

        private void ReleaseHudControls()
        {
            movement?.SetMoveInput(Vector2.zero);
            ReleaseHudLookAim();
            if (previousBasicHeld || firePointerHeld)
            {
                rangedBasicAttackAction?.SetFireHeld(false);
            }

            ClearFirePointerState();
            ClearMovePointerState();
            ClearLookPointerState();
            previousBasicHeld = false;
        }

        private bool HasHeldReviewControl()
        {
            return previousBasicHeld
                || firePointerHeld
                || movePointerHeld
                || lookPointerHeld
                || hudLookAimActive;
        }

        private void OnGUI()
        {
            if (!showHud || !drawHudVisuals || hudOpacity <= 0.001f)
            {
                return;
            }

            Color previousGuiColor = GUI.color;
            Color previousContentColor = GUI.contentColor;
            Color previousBackgroundColor = GUI.backgroundColor;
            float previousChromeOpacity = BossBarrageLaneReviewHudChrome.BeginOpacity(hudOpacity);
            GUI.color = WithHudOpacity(previousGuiColor);
            GUI.contentColor = WithHudOpacity(previousContentColor);
            GUI.backgroundColor = WithHudOpacity(previousBackgroundColor);
            BuildLayout();
            EnsureStyles();
            DrawMoveJoystick();
            DrawButton(basicRect, combatModeController != null && combatModeController.IsMeleeMode ? "SLASH" : "FIRE", firePointerHeld);
            DrawFireAimReticle();
            DrawLockTargetMarker();
            DrawButton(dodgeRect, "DODGE", IsHeld(dodgeRect));
            DrawButton(swapRect, "SWAP", false);
            DrawButton(skillRect, "SKILL", false);
            DrawSummonButtons();
            BossBarrageLaneReviewHudChrome.EndOpacity(previousChromeOpacity);
            GUI.color = previousGuiColor;
            GUI.contentColor = previousContentColor;
            GUI.backgroundColor = previousBackgroundColor;
        }

        private Vector2 ResolveMoveInput()
        {
            return movePointerHeld ? moveJoystickInput : Vector2.zero;
        }

        private void BuildLayout()
        {
            float resolvedScale = ResolveScale();
            float size = ResolveActionButtonSize(resolvedScale);
            float gap = ResolveButtonGap(resolvedScale);
            float edge = ResolveTouchEdgeInset(resolvedScale);

            float joystickRadius = ResolveMoveJoystickRadius(resolvedScale);
            float joystickTouchRadius = joystickRadius * moveJoystickTouchRadiusScale;
            moveJoystickCenterGuiPoint = new Vector2(
                edge + joystickTouchRadius,
                Screen.height - edge - joystickTouchRadius);
            moveJoystickRect = RectFromCenter(moveJoystickCenterGuiPoint, joystickRadius * 2f);
            moveJoystickTouchRect = RectFromCenter(moveJoystickCenterGuiPoint, joystickTouchRadius * 2f);

            float rightX = Mathf.Max(edge, Screen.width - edge - size);
            float bottomY = Mathf.Max(edge, Screen.height - edge - size);
            float secondaryX = Mathf.Max(edge, rightX - size - gap);
            float upperY = Mathf.Max(edge, bottomY - size - gap);
            basicRect = new Rect(rightX, bottomY, size, size);
            aimRect = Rect.zero;
            dodgeRect = new Rect(rightX, upperY, size, size);
            swapRect = new Rect(secondaryX, upperY, size, size);
            skillRect = new Rect(secondaryX, bottomY, size, size);

            bool showSupportSummonButtons = AreSupportSummonButtonsVisible();
            float summonWidth = showSupportSummonButtons ? size * 1.55f : size * 1.72f;
            float summonHeight = size * 0.72f;
            float summonGap = Mathf.Max(gap * summonButtonGapMultiplier, minimumButtonGap);
            int summonButtonCount = showSupportSummonButtons ? 3 : 1;
            float summonGroupHeight = summonHeight * summonButtonCount + summonGap * Mathf.Max(0, summonButtonCount - 1);
            float desiredSummonY = Screen.height * summonButtonGroupCenterY01 - summonGroupHeight * 0.5f;
            float actionClusterTopY = Mathf.Min(upperY, bottomY);
            float maxSummonY = actionClusterTopY - gap - summonGroupHeight;
            float summonY = Mathf.Clamp(desiredSummonY, edge, Mathf.Max(edge, maxSummonY));
            float summonX = Screen.width - edge - summonWidth;
            summonSlot1Rect = new Rect(summonX, summonY, summonWidth, summonHeight);
            summonSlot2Rect = showSupportSummonButtons
                ? new Rect(summonX, summonY + summonHeight + summonGap, summonWidth, summonHeight)
                : Rect.zero;
            summonSlot3Rect = showSupportSummonButtons
                ? new Rect(summonX, summonY + (summonHeight + summonGap) * 2f, summonWidth, summonHeight)
                : Rect.zero;
        }

        private Rect ResolveCurrentGuiRect(ref Rect rect)
        {
            BuildLayout();
            return rect;
        }

        private static Vector2 ToScreenAnchor(Vector2 guiPoint)
        {
            if (Screen.width <= 0 || Screen.height <= 0)
            {
                return Vector2.zero;
            }

            return new Vector2(
                Mathf.Clamp01(guiPoint.x / Screen.width),
                Mathf.Clamp01(1f - guiPoint.y / Screen.height));
        }

        private float ResolveScale()
        {
            float screenScale = Mathf.Clamp(Screen.height / 1440f, 0.72f, 1.35f);
            return screenScale * Mathf.Max(0.5f, scale);
        }

        private float ResolveActionButtonSize(float resolvedScale)
        {
            return Mathf.Max(buttonSize * resolvedScale, minimumActionButtonSize);
        }

        private float ResolveButtonGap(float resolvedScale)
        {
            return Mathf.Max(buttonGap * resolvedScale, minimumButtonGap);
        }

        private float ResolveTouchEdgeInset(float resolvedScale)
        {
            return Mathf.Max(margin * resolvedScale, minimumTouchEdgeInset);
        }

        private float ResolveMoveJoystickRadius(float resolvedScale)
        {
            return Mathf.Max(moveJoystickRadius * resolvedScale, minimumMoveJoystickRadius);
        }

        private float ResolveMoveJoystickKnobSize(float resolvedScale)
        {
            return Mathf.Max(moveJoystickKnobSize * resolvedScale, minimumMoveJoystickKnobSize);
        }

        private void UpdateMovePointerState()
        {
            if (TryUpdateMoveTouchPointer())
            {
                return;
            }

            UpdateMoveMousePointer();
        }

        private bool TryUpdateMoveTouchPointer()
        {
            if (Touchscreen.current == null)
            {
                if (!movePointerUsesMouse)
                {
                    ClearMovePointerState();
                }

                return false;
            }

            if (movePointerHeld && !movePointerUsesMouse)
            {
                return UpdateActiveMoveTouchPointer();
            }

            foreach (var touch in Touchscreen.current.touches)
            {
                if (touch.phase.ReadValue() != UnityEngine.InputSystem.TouchPhase.Began)
                {
                    continue;
                }

                Vector2 point = ToGuiPoint(touch.position.ReadValue());
                if (moveJoystickTouchRect.Contains(point))
                {
                    BeginMovePointer(point, usesMouse: false, touch.touchId.ReadValue());
                    return true;
                }
            }

            return false;
        }

        private bool UpdateActiveMoveTouchPointer()
        {
            foreach (var touch in Touchscreen.current.touches)
            {
                if (touch.touchId.ReadValue() != movePointerTouchId)
                {
                    continue;
                }

                if (!touch.press.isPressed)
                {
                    ClearMovePointerState();
                    return true;
                }

                UpdateMovePointer(ToGuiPoint(touch.position.ReadValue()));
                return true;
            }

            ClearMovePointerState();
            return true;
        }

        private void UpdateMoveMousePointer()
        {
            if (Mouse.current == null)
            {
                if (movePointerUsesMouse)
                {
                    ClearMovePointerState();
                }

                return;
            }

            Vector2 point = ToGuiPoint(Mouse.current.position.ReadValue());
            if (movePointerHeld && movePointerUsesMouse)
            {
                if (Mouse.current.leftButton.isPressed)
                {
                    UpdateMovePointer(point);
                }
                else
                {
                    ClearMovePointerState();
                }

                return;
            }

            if (Mouse.current.leftButton.wasPressedThisFrame && moveJoystickTouchRect.Contains(point))
            {
                BeginMovePointer(point, usesMouse: true, touchId: -1);
            }
        }

        private void BeginMovePointer(Vector2 point, bool usesMouse, int touchId)
        {
            movePointerHeld = true;
            movePointerUsesMouse = usesMouse;
            movePointerTouchId = touchId;
            UpdateMovePointer(point);
        }

        private void UpdateMovePointer(Vector2 point)
        {
            moveJoystickInput = ResolveMoveJoystickInput(point);
        }

        private void ClearMovePointerState()
        {
            movePointerHeld = false;
            movePointerUsesMouse = false;
            movePointerTouchId = -1;
            moveJoystickInput = Vector2.zero;
        }

        private Vector2 ResolveMoveJoystickInput(Vector2 currentGuiPoint)
        {
            float radius = Mathf.Max(1f, ResolveMoveJoystickRadius(ResolveScale()));
            Vector2 delta = currentGuiPoint - moveJoystickCenterGuiPoint;
            Vector2 input = Vector2.ClampMagnitude(new Vector2(delta.x, -delta.y) / radius, 1f);
            float magnitude = input.magnitude;
            if (magnitude < moveJoystickDeadZone)
            {
                return Vector2.zero;
            }

            float adjustedMagnitude = Mathf.InverseLerp(moveJoystickDeadZone, 1f, magnitude);
            return magnitude > 0f ? input.normalized * adjustedMagnitude : Vector2.zero;
        }

        private void UpdateFirePointerState()
        {
            firePointerPressed = false;
            if (TryUpdateFireTouchPointer())
            {
                return;
            }

            UpdateFireMousePointer();
        }

        private bool TryUpdateFireTouchPointer()
        {
            if (Touchscreen.current == null)
            {
                if (!firePointerUsesMouse)
                {
                    ClearFirePointerState();
                }

                return false;
            }

            if (firePointerHeld && !firePointerUsesMouse)
            {
                return UpdateActiveFireTouchPointer();
            }

            foreach (var touch in Touchscreen.current.touches)
            {
                if (touch.phase.ReadValue() != UnityEngine.InputSystem.TouchPhase.Began)
                {
                    continue;
                }

                Vector2 point = ToGuiPoint(touch.position.ReadValue());
                if (basicRect.Contains(point))
                {
                    BeginFirePointer(usesMouse: false, touch.touchId.ReadValue());
                    return true;
                }
            }

            return false;
        }

        private bool UpdateActiveFireTouchPointer()
        {
            foreach (var touch in Touchscreen.current.touches)
            {
                if (touch.touchId.ReadValue() != firePointerTouchId)
                {
                    continue;
                }

                if (!touch.press.isPressed)
                {
                    ClearFirePointerState();
                    return true;
                }

                return true;
            }

            ClearFirePointerState();
            return true;
        }

        private void UpdateFireMousePointer()
        {
            if (Mouse.current == null)
            {
                if (firePointerUsesMouse)
                {
                    ClearFirePointerState();
                }

                return;
            }

            Vector2 point = ToGuiPoint(Mouse.current.position.ReadValue());
            if (firePointerHeld && firePointerUsesMouse)
            {
                if (Mouse.current.leftButton.isPressed)
                {
                    return;
                }
                else
                {
                    ClearFirePointerState();
                }

                return;
            }

            if (Mouse.current.leftButton.wasPressedThisFrame && basicRect.Contains(point))
            {
                BeginFirePointer(usesMouse: true, touchId: -1);
            }
        }

        private void UpdateLookPointerState()
        {
            if (!screenDragControlsAim)
            {
                ClearLookPointerState();
                return;
            }

            if (TryUpdateLookTouchPointer())
            {
                return;
            }

            UpdateLookMousePointer();
        }

        private bool TryUpdateLookTouchPointer()
        {
            if (Touchscreen.current == null)
            {
                if (!lookPointerUsesMouse)
                {
                    ClearLookPointerState();
                }

                return false;
            }

            if (lookPointerHeld && !lookPointerUsesMouse)
            {
                return UpdateActiveLookTouchPointer();
            }

            foreach (var touch in Touchscreen.current.touches)
            {
                if (touch.phase.ReadValue() != UnityEngine.InputSystem.TouchPhase.Began)
                {
                    continue;
                }

                Vector2 point = ToGuiPoint(touch.position.ReadValue());
                if (IsLookAimStartPoint(point))
                {
                    BeginLookPointer(point, usesMouse: false, usesRightMouse: false, touch.touchId.ReadValue());
                    return true;
                }
            }

            return false;
        }

        private bool UpdateActiveLookTouchPointer()
        {
            foreach (var touch in Touchscreen.current.touches)
            {
                if (touch.touchId.ReadValue() != lookPointerTouchId)
                {
                    continue;
                }

                if (!touch.press.isPressed)
                {
                    ClearLookPointerState();
                    return true;
                }

                UpdateLookPointer(ToGuiPoint(touch.position.ReadValue()));
                return true;
            }

            ClearLookPointerState();
            return true;
        }

        private void UpdateLookMousePointer()
        {
            if (Mouse.current == null)
            {
                if (lookPointerUsesMouse)
                {
                    ClearLookPointerState();
                }

                return;
            }

            Vector2 point = ToGuiPoint(Mouse.current.position.ReadValue());
            if (lookPointerHeld && lookPointerUsesMouse)
            {
                bool stillHeld = lookPointerUsesRightMouse
                    ? Mouse.current.rightButton.isPressed
                    : Mouse.current.leftButton.isPressed;
                if (stillHeld)
                {
                    UpdateLookPointer(point);
                }
                else
                {
                    ClearLookPointerState();
                }

                return;
            }

            if (rightMouseDragControlsAim && Mouse.current.rightButton.wasPressedThisFrame && IsLookAimStartPoint(point))
            {
                BeginLookPointer(point, usesMouse: true, usesRightMouse: true, touchId: -1);
                return;
            }

            if (leftMouseDragControlsAim && Mouse.current.leftButton.wasPressedThisFrame && IsLookAimStartPoint(point))
            {
                BeginLookPointer(point, usesMouse: true, usesRightMouse: false, touchId: -1);
            }
        }

        private void BeginFirePointer(bool usesMouse, int touchId)
        {
            firePointerHeld = true;
            firePointerPressed = true;
            firePointerUsesMouse = usesMouse;
            firePointerTouchId = touchId;
        }

        private void ClearFirePointerState()
        {
            firePointerHeld = false;
            firePointerPressed = false;
            firePointerUsesMouse = false;
            firePointerTouchId = -1;
        }

        private void BeginLookPointer(Vector2 point, bool usesMouse, bool usesRightMouse, int touchId)
        {
            lookPointerHeld = true;
            lookPointerUsesMouse = usesMouse;
            lookPointerUsesRightMouse = usesRightMouse;
            lookPointerTouchId = touchId;
            lookPointerCurrentGuiPoint = point;
            lookAimRawInput = Vector2.zero;
            lookAimInput = Vector2.zero;
            UpdateLookPointer(point);
        }

        private void UpdateLookPointer(Vector2 point)
        {
            Vector2 delta = point - lookPointerCurrentGuiPoint;
            lookPointerCurrentGuiPoint = point;
            lookAimInput = ResolveDragAimInput(delta);
        }

        private void ClearLookPointerState()
        {
            lookPointerHeld = false;
            lookPointerUsesMouse = false;
            lookPointerUsesRightMouse = false;
            lookPointerTouchId = -1;
            lookPointerCurrentGuiPoint = Vector2.zero;
            lookAimRawInput = Vector2.zero;
            lookAimInput = Vector2.zero;
        }

        private void UpdateHudLookAim()
        {
            bool pointerAimActive = screenDragControlsAim && lookPointerHeld;
            Vector2 keyboardPeekInput = ResolveKeyboardPeekAimInput();
            bool keyboardPeekActive = keyboardPeekInput.sqrMagnitude > 0.0001f && IsKeyboardPeekAllowed();
            bool shouldRouteLookAim = pointerAimActive || keyboardPeekActive;
            if (!shouldRouteLookAim && !hudLookAimActive)
            {
                return;
            }

            Vector2 aimInput = Vector2.zero;
            if (pointerAimActive)
            {
                aimInput += lookAimInput;
            }

            if (keyboardPeekActive)
            {
                aimInput += keyboardPeekInput;
            }

            aimInput = Vector2.ClampMagnitude(aimInput, 1f);
            movement?.SetLookInput(routeAimToMovementLook ? aimInput : Vector2.zero);
            rangedBasicAttackAction?.SetAimInput(aimInput);
            aimController?.SetAimInput(aimInput);
            aimController?.SetAimHeld(pointerAimActive);
            hudLookAimActive = shouldRouteLookAim;
        }

        private void ReleaseHudLookAim()
        {
            if (!hudLookAimActive)
            {
                return;
            }

            movement?.SetLookInput(Vector2.zero);
            rangedBasicAttackAction?.SetAimInput(Vector2.zero);
            aimController?.SetAimInput(Vector2.zero);
            aimController?.SetAimHeld(false);
            hudLookAimActive = false;
        }

        private Vector2 ResolveDragAimInput(Vector2 guiDelta)
        {
            float sensitivity = Mathf.Max(0.0001f, lookAimDragSensitivity) / Mathf.Max(0.01f, ResolveScale());
            lookAimRawInput = Vector2.ClampMagnitude(
                lookAimRawInput + new Vector2(guiDelta.x, -guiDelta.y) * sensitivity,
                1f);
            return lookAimRawInput.sqrMagnitude >= lookAimDragDeadZone * lookAimDragDeadZone
                ? lookAimRawInput
                : Vector2.zero;
        }

        private Vector2 ResolveKeyboardPeekAimInput()
        {
            if (!keyboardPeekControlsAim || Keyboard.current == null)
            {
                return Vector2.zero;
            }

            float x = 0f;
            if (IsKeyboardKeyPressed(keyboardPeekLeftKey))
            {
                x -= 1f;
            }

            if (IsKeyboardKeyPressed(keyboardPeekRightKey))
            {
                x += 1f;
            }

            return Mathf.Abs(x) > 0f ? new Vector2(Mathf.Clamp(x, -1f, 1f), 0f) : Vector2.zero;
        }

        private bool IsKeyboardPeekAllowed()
        {
            if (combatModeController != null && !combatModeController.IsRangedMode)
            {
                return false;
            }

            if (!keyboardPeekRequiresActiveAim)
            {
                return true;
            }

            return (aimController != null && aimController.IsAiming)
                || (rangedBasicAttackAction != null && rangedBasicAttackAction.IsAimPreviewActive);
        }

        private static bool IsKeyboardKeyPressed(Key key)
        {
            if (key == Key.None || Keyboard.current == null)
            {
                return false;
            }

            var control = Keyboard.current[key];
            return control != null && control.isPressed;
        }

        private bool IsLookAimStartPoint(Vector2 point)
        {
            return point.x >= Screen.width * lookAimScreenMinX && !IsHudControlPoint(point);
        }

        private bool IsHudControlPoint(Vector2 point)
        {
            return moveJoystickTouchRect.Contains(point)
                || basicRect.Contains(point)
                || dodgeRect.Contains(point)
                || swapRect.Contains(point)
                || skillRect.Contains(point)
                || summonSlot1Rect.Contains(point)
                || AreSupportSummonButtonsVisible() && (summonSlot2Rect.Contains(point) || summonSlot3Rect.Contains(point));
        }

        private bool IsHeld(Rect rect)
        {
            if (Touchscreen.current != null)
            {
                foreach (var touch in Touchscreen.current.touches)
                {
                    if (!touch.press.isPressed)
                    {
                        continue;
                    }

                    if (rect.Contains(ToGuiPoint(touch.position.ReadValue())))
                    {
                        return true;
                    }
                }
            }

            return Mouse.current != null
                && Mouse.current.leftButton.isPressed
                && rect.Contains(ToGuiPoint(Mouse.current.position.ReadValue()));
        }

        private bool IsPressed(Rect rect)
        {
            if (Touchscreen.current != null)
            {
                foreach (var touch in Touchscreen.current.touches)
                {
                    if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began
                        && rect.Contains(ToGuiPoint(touch.position.ReadValue())))
                    {
                        return true;
                    }
                }
            }

            return Mouse.current != null
                && Mouse.current.leftButton.wasPressedThisFrame
                && rect.Contains(ToGuiPoint(Mouse.current.position.ReadValue()));
        }

        private void DrawButton(Rect rect, string label, bool held, bool pending = false)
        {
            BossBarrageLaneReviewHudChrome.DrawActionButton(
                rect,
                label,
                held,
                pending,
                ResolveActionAccent(label));
        }

        private void DrawMoveJoystick()
        {
            float resolvedScale = ResolveScale();
            float knobSize = ResolveMoveJoystickKnobSize(resolvedScale);
            BossBarrageLaneReviewHudChrome.DrawJoystick(
                moveJoystickRect,
                moveJoystickInput,
                movePointerHeld,
                knobSize,
                dodgeAccentColor);
        }

        private void DrawSummonButtons()
        {
            bool slot1Ready = IsPrimarySummonReady();
            bool slot1Pending = summonSlot1Action == null;
            BossBarrageLaneReviewHudChrome.DrawSummonSlot(
                summonSlot1Rect,
                BossBarrageLaneReviewMobileHudLabels.BuildPrimarySummonLabel(
                    summonSlot1Label,
                    energyLadder,
                    summonSlot1Action),
                IsHeld(summonSlot1Rect),
                pending: slot1Pending,
                BossBarrageLaneReviewMobileHudLabels.ResolvePrimarySummonFill01(
                    energyLadder,
                    summonSlot1Action),
                summonSlot1AccentColor,
                ready: slot1Ready,
                unavailable: !slot1Ready && !slot1Pending,
                readyPulse01: ResolveSummonReadyPulse01(summonSlot1ReadyPulseTimer),
                iconKind: 1);
            if (!AreSupportSummonButtonsVisible())
            {
                return;
            }

            bool slot2Ready = IsSupportSummonReady(summonSlot2Action);
            bool slot2Pending = summonSlot2Action == null;
            BossBarrageLaneReviewHudChrome.DrawSummonSlot(
                summonSlot2Rect,
                BossBarrageLaneReviewMobileHudLabels.BuildSupportSummonLabel(
                    summonSlot2Action,
                    summonSlot2Label,
                    lockedSummonLabel,
                    energyLadder),
                IsHeld(summonSlot2Rect),
                pending: slot2Pending,
                BossBarrageLaneReviewMobileHudLabels.ResolveSupportSummonFill01(
                    energyLadder,
                    summonSlot2Action),
                summonSlot2AccentColor,
                ready: slot2Ready,
                unavailable: !slot2Ready && !slot2Pending,
                readyPulse01: ResolveSummonReadyPulse01(summonSlot2ReadyPulseTimer),
                iconKind: 2);
            bool slot3Ready = IsSupportSummonReady(summonSlot3Action);
            bool slot3Pending = summonSlot3Action == null;
            BossBarrageLaneReviewHudChrome.DrawSummonSlot(
                summonSlot3Rect,
                BossBarrageLaneReviewMobileHudLabels.BuildSupportSummonLabel(
                    summonSlot3Action,
                    summonSlot3Label,
                    lockedSummonLabel,
                    energyLadder),
                IsHeld(summonSlot3Rect),
                pending: slot3Pending,
                BossBarrageLaneReviewMobileHudLabels.ResolveSupportSummonFill01(
                    energyLadder,
                    summonSlot3Action),
                summonSlot3AccentColor,
                ready: slot3Ready,
                unavailable: !slot3Ready && !slot3Pending,
                readyPulse01: ResolveSummonReadyPulse01(summonSlot3ReadyPulseTimer),
                iconKind: 3);
        }

        private void UpdateSummonReadinessFeedback(float deltaTime)
        {
            UpdateSummonReadyPulse(
                IsPrimarySummonReady(),
                ref previousSummonSlot1Ready,
                ref summonSlot1ReadyPulseTimer,
                deltaTime);
            UpdateSummonReadyPulse(
                AreSupportSummonButtonsVisible() && IsSupportSummonReady(summonSlot2Action),
                ref previousSummonSlot2Ready,
                ref summonSlot2ReadyPulseTimer,
                deltaTime);
            UpdateSummonReadyPulse(
                AreSupportSummonButtonsVisible() && IsSupportSummonReady(summonSlot3Action),
                ref previousSummonSlot3Ready,
                ref summonSlot3ReadyPulseTimer,
                deltaTime);
        }

        private void UpdateSummonReadyPulse(
            bool ready,
            ref bool previousReady,
            ref float pulseTimer,
            float deltaTime)
        {
            if (ready && !previousReady)
            {
                pulseTimer = summonReadyPulseSeconds;
            }
            else if (pulseTimer > 0f)
            {
                pulseTimer = Mathf.Max(0f, pulseTimer - Mathf.Max(0f, deltaTime));
            }

            previousReady = ready;
        }

        private float ResolveSummonReadyPulse01(float pulseTimer)
        {
            return summonReadyPulseSeconds > 0.001f
                ? Mathf.Clamp01(pulseTimer / summonReadyPulseSeconds)
                : 0f;
        }

        private bool IsPrimarySummonReady()
        {
            return summonSlot1Action != null
                && energyLadder != null
                && !summonSlot1Action.IsSlotOnCooldown
                && energyLadder.AvailableTier > 0
                && energyLadder.CanSpendMana(summonSlot1Action.RequiredSummonMana);
        }

        private bool IsSupportSummonReady(PlayerSupportSummonSlotAction supportAction)
        {
            return supportAction != null
                && energyLadder != null
                && !supportAction.IsSlotOnCooldown
                && energyLadder.AvailableTier >= supportAction.MinimumSummonTier
                && energyLadder.CanSpendMana(supportAction.RequiredSummonMana);
        }

        private bool AreSupportSummonButtonsVisible()
        {
            return !useSingleSummonButton;
        }

        private void DrawFireAimReticle()
        {
            if (!showFireAimReticle || !IsRangedAimReticleVisible())
            {
                return;
            }

            EnsureReticleTexture();
            if (reticleTexture == null)
            {
                return;
            }

            Vector2 rawCenter = ResolveFireAimReticleGuiPoint();
            float resolvedScale = ResolveScale();
            float assistStrength = ResolveFireAimAssistStrength();
            Vector2 assistCenter = rawCenter;
            bool hasLockAssistPoint = false;
            bool hasAssistPoint = fireAimReticleFollowsAssist
                && TryResolveFireAimAssistGuiPoint(rawCenter, resolvedScale, out assistCenter, out hasLockAssistPoint);
            float size = fireAimReticleSize * resolvedScale * (1f + fireAimAssistSizeBoost * assistStrength);
            float gap = fireAimReticleGap * resolvedScale * (1f - fireAimAssistGapTighten * assistStrength);
            float thickness = fireAimReticleThickness * resolvedScale * (1f + fireAimAssistThicknessBoost * assistStrength);

            Color previousColor = GUI.color;
            if (!hasAssistPoint)
            {
                DrawFireAimReticleAt(
                    rawCenter,
                    fireAimReticleSize * resolvedScale,
                    fireAimReticleGap * resolvedScale,
                    fireAimReticleThickness * resolvedScale,
                    fireAimReticleColor);
            }
            else if (!hasLockAssistPoint)
            {
                Color rawColor = fireAimReticleColor;
                rawColor.a *= 0.38f;
                DrawFireAimReticleAt(
                    rawCenter,
                    fireAimReticleSize * resolvedScale,
                    fireAimReticleGap * resolvedScale,
                    fireAimReticleThickness * resolvedScale,
                    rawColor);
            }

            if (hasAssistPoint)
            {
                DrawFireAimReticleAt(assistCenter, size, gap, thickness, fireAimAssistReticleColor);
            }
            else if (assistStrength > 0f)
            {
                DrawFireAimReticleAt(rawCenter, size, gap, thickness, Color.Lerp(fireAimReticleColor, fireAimAssistReticleColor, assistStrength));
            }

            GUI.color = previousColor;
        }

        private void DrawFireAimReticleAt(Vector2 center, float size, float gap, float thickness, Color color)
        {
            GUI.color = WithHudOpacity(color);
            DrawReticleSegment(new Rect(center.x - gap - size, center.y - thickness * 0.5f, size, thickness));
            DrawReticleSegment(new Rect(center.x + gap, center.y - thickness * 0.5f, size, thickness));
            DrawReticleSegment(new Rect(center.x - thickness * 0.5f, center.y - gap - size, thickness, size));
            DrawReticleSegment(new Rect(center.x - thickness * 0.5f, center.y + gap, thickness, size));
        }

        private void DrawLockTargetMarker()
        {
            if (!showLockTargetMarker
                || lockTargetController == null
                || !lockTargetController.HasLockTarget
                || !lockTargetController.TryGetLockViewportPoint(out Vector2 viewportPoint))
            {
                return;
            }

            EnsureReticleTexture();
            if (reticleTexture == null)
            {
                return;
            }

            float resolvedScale = ResolveScale();
            float pulse01 = 0.5f + Mathf.Sin(Time.time * 9f) * 0.5f;
            float size = lockTargetMarkerSize * resolvedScale * (1f + lockTargetMarkerPulseBoost * pulse01);
            float gap = lockTargetMarkerGap * resolvedScale;
            float thickness = lockTargetMarkerThickness * resolvedScale;
            Vector2 center = new Vector2(viewportPoint.x * Screen.width, (1f - viewportPoint.y) * Screen.height);
            Color color = lockTargetController.CurrentLockType == PlayerLockTargetController.LockTargetType.HardLock
                ? hardLockTargetMarkerColor
                : lockTargetMarkerColor;
            color = Color.Lerp(color, Color.white, 0.18f * pulse01);

            Color previousColor = GUI.color;
            DrawFireAimReticleAt(center, size, gap, thickness, color);
            DrawLockTargetCore(
                center,
                resolvedScale,
                pulse01,
                lockTargetController.CurrentLockType == PlayerLockTargetController.LockTargetType.HardLock
                    ? hardLockTargetCoreColor
                    : lockTargetCoreColor);
            GUI.color = previousColor;
        }

        private void DrawLockTargetCore(Vector2 center, float resolvedScale, float pulse01, Color coreColor)
        {
            float haloSize = lockTargetCoreHaloSize * resolvedScale * (1f + 0.14f * pulse01);
            for (int i = 0; i < 4; i++)
            {
                float step01 = i / 3f;
                float size = Mathf.Lerp(haloSize, lockTargetCoreDotSize * resolvedScale * 1.8f, step01);
                Color haloColor = Color.Lerp(coreColor, Color.white, step01 * 0.35f);
                haloColor.a *= Mathf.Lerp(0.12f, 0.34f, step01) * (0.72f + 0.28f * pulse01);
                GUI.color = WithHudOpacity(haloColor);
                DrawReticleSegment(RectFromCenter(center, size));
            }

            float dotSize = lockTargetCoreDotSize * resolvedScale * (1f + 0.12f * pulse01);
            float glintSize = Mathf.Max(2f, dotSize * 0.28f);
            GUI.color = WithHudOpacity(coreColor);
            DrawReticleSegment(RectFromCenter(center, dotSize));
            GUI.color = WithHudOpacity(Color.white);
            DrawReticleSegment(RectFromCenter(center + new Vector2(-dotSize * 0.16f, -dotSize * 0.16f), glintSize));
        }

        private Color WithHudOpacity(Color color)
        {
            color.a *= hudOpacity;
            return color;
        }

        private bool IsRangedAimReticleVisible()
        {
            if (combatModeController != null && !combatModeController.IsRangedMode)
            {
                return false;
            }

            return true;
        }

        private float ResolveFireAimAssistStrength()
        {
            if (lockTargetController != null && lockTargetController.HasLockTarget)
            {
                return Mathf.Clamp01(lockTargetController.LockStrength01);
            }

            if (rangedBasicAttackAction == null
                || !rangedBasicAttackAction.TryGetAimPreviewDirection(out _)
                || !rangedBasicAttackAction.HasAimAssistTarget)
            {
                return 0f;
            }

            return Mathf.Clamp01(rangedBasicAttackAction.AimAssistStrength01);
        }

        private bool TryResolveFireAimAssistGuiPoint(
            Vector2 rawCenter,
            float resolvedScale,
            out Vector2 assistCenter,
            out bool isLockAssistPoint)
        {
            assistCenter = rawCenter;
            isLockAssistPoint = false;
            if (lockTargetController != null
                && lockTargetController.TryGetLockViewportPoint(out Vector2 lockViewportPoint))
            {
                assistCenter = new Vector2(
                    lockViewportPoint.x * Screen.width,
                    (1f - lockViewportPoint.y) * Screen.height);
                isLockAssistPoint = true;
                return ClampAssistReticlePoint(rawCenter, resolvedScale, ref assistCenter, clampToMaxOffset: false);
            }

            if (rangedBasicAttackAction == null
                || !rangedBasicAttackAction.TryGetAimAssistPreviewViewportPoint(out Vector2 assistViewportPoint))
            {
                return false;
            }

            assistCenter = new Vector2(
                assistViewportPoint.x * Screen.width,
                (1f - assistViewportPoint.y) * Screen.height);
            return ClampAssistReticlePoint(rawCenter, resolvedScale, ref assistCenter, clampToMaxOffset: true);
        }

        private bool ClampAssistReticlePoint(
            Vector2 rawCenter,
            float resolvedScale,
            ref Vector2 assistCenter,
            bool clampToMaxOffset)
        {
            Vector2 offset = assistCenter - rawCenter;
            float maxOffset = fireAimAssistReticleMaxOffset * resolvedScale;
            if (clampToMaxOffset && maxOffset > 0f)
            {
                offset = Vector2.ClampMagnitude(offset, maxOffset);
                assistCenter = rawCenter + offset;
            }

            return offset.sqrMagnitude > 1f;
        }

        private Vector2 ResolveFireAimReticleGuiPoint()
        {
            Vector2 viewportPoint = new Vector2(0.5f, 0.5f);
            if (!fireAimReticleUsesScreenCenter
                && rangedBasicAttackAction != null
                && rangedBasicAttackAction.TryGetAimPreviewViewportPoint(out Vector2 actionViewportPoint))
            {
                viewportPoint = actionViewportPoint;
            }

            return new Vector2(
                viewportPoint.x * Screen.width,
                (1f - viewportPoint.y) * Screen.height);
        }

        private void DrawReticleSegment(Rect rect)
        {
            GUI.DrawTexture(rect, reticleTexture, ScaleMode.StretchToFill);
        }

        private void EnsureReticleTexture()
        {
            if (reticleTexture == null)
            {
                reticleTexture = MakeTexture(Color.white);
            }
        }

        private void EnsureStyles()
        {
            if (buttonStyle != null && heldButtonStyle != null && pendingButtonStyle != null)
            {
                return;
            }

            buttonStyle = CreateButtonStyle(buttonColor);
            heldButtonStyle = CreateButtonStyle(heldButtonColor);
            pendingButtonStyle = CreateButtonStyle(pendingButtonColor);
        }

        private GUIStyle CreateButtonStyle(Color color)
        {
            var style = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize = Mathf.RoundToInt(Mathf.Clamp(Screen.height / 48f, 18f, 34f)),
                normal = { textColor = actionTextColor },
                padding = new RectOffset(4, 4, 4, 4),
                wordWrap = true
            };
            style.normal.background = MakeTexture(color);
            return style;
        }

        private Color ResolveActionAccent(string label)
        {
            if (label == "DODGE" || label == "SWAP")
            {
                return dodgeAccentColor;
            }

            if (label == "SKILL")
            {
                return skillAccentColor;
            }

            return fireAccentColor;
        }

        private static Texture2D MakeTexture(Color color)
        {
            var texture = new Texture2D(1, 1)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }

        private static Vector2 ToGuiPoint(Vector2 screenPoint)
        {
            return new Vector2(screenPoint.x, Screen.height - screenPoint.y);
        }

        private static Rect RectFromCenter(Vector2 center, float size)
        {
            return new Rect(center.x - size * 0.5f, center.y - size * 0.5f, size, size);
        }

        private T FindFirstComponentOnSelfOrParent<T>() where T : Component
        {
            return GetComponent<T>() ?? GetComponentInParent<T>();
        }

        private PlayerSupportSummonSlotAction FindSupportSummonAction(string actionName)
        {
            if (movement == null)
            {
                return null;
            }

            PlayerSupportSummonSlotAction[] actions =
                movement.GetComponents<PlayerSupportSummonSlotAction>();
            for (int i = 0; i < actions.Length; i++)
            {
                if (actions[i] != null && actions[i].SlotActionName == actionName)
                {
                    return actions[i];
                }
            }

            return null;
        }
    }
}
