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
        [SerializeField, Min(40f)] private float buttonSize = 118f;
        [SerializeField, Min(0f)] private float buttonGap = 18f;
        [SerializeField, Min(0f)] private float margin = 34f;
        [SerializeField, Range(0.5f, 2f)] private float scale = 1f;
        [SerializeField] private Color buttonColor = new Color(0.04f, 0.11f, 0.16f, 0.58f);
        [SerializeField] private Color heldButtonColor = new Color(0.18f, 0.84f, 1f, 0.72f);
        [SerializeField] private Color pendingButtonColor = new Color(0.04f, 0.08f, 0.11f, 0.44f);
        [SerializeField] private Color actionTextColor = Color.white;
        [SerializeField] private string summonSlot1Label = "S1 SHIELD";
        [SerializeField] private string summonSlot2Label = "S2 ARROW";
        [SerializeField] private string summonSlot3Label = "S3 TANK";
        [SerializeField] private string lockedSummonLabel = "NEXT";

        [Header("Look Aim")]
        [SerializeField] private bool screenDragControlsAim = true;
        [SerializeField] private bool rightMouseDragControlsAim = true;
        [SerializeField] private bool leftMouseDragControlsAim;
        [SerializeField] private bool fireDragControlsAim = true;
        [SerializeField] private bool routeAimToMovementLook;
        [SerializeField, Range(0f, 1f)] private float lookAimDragDeadZone = 0.08f;
        [SerializeField, Min(8f)] private float lookAimDragRadius = 230f;
        [SerializeField, Min(8f)] private float lookAimKnobSize = 30f;
        [SerializeField, Range(0f, 1f)] private float lookAimScreenMinX;

        [Header("Review Reticle")]
        [SerializeField] private bool showFireAimReticle = true;
        [SerializeField, Min(4f)] private float fireAimReticleSize = 34f;
        [SerializeField, Min(0f)] private float fireAimReticleGap = 9f;
        [SerializeField, Min(1f)] private float fireAimReticleThickness = 2f;
        [SerializeField] private Color fireAimReticleColor = new Color(0.82f, 0.96f, 1f, 0.88f);

        private Rect moveUpRect;
        private Rect moveDownRect;
        private Rect moveLeftRect;
        private Rect moveRightRect;
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
        private Vector2 firePointerStartGuiPoint;
        private Vector2 firePointerCurrentGuiPoint;
        private bool lookPointerHeld;
        private bool lookPointerUsesMouse;
        private bool lookPointerUsesRightMouse;
        private int lookPointerTouchId = -1;
        private Vector2 lookPointerStartGuiPoint;
        private Vector2 lookPointerCurrentGuiPoint;
        private Vector2 lookAimInput;
        private Vector2 fireAimInput;
        private bool hudLookAimActive;

        public string MoveActionName => moveActionName;
        public string BasicDefenseActionName => basicDefenseActionName;
        public string DodgeActionName => dodgeActionName;
        public string Skill1ActionName => skill1ActionName;
        public string SummonSlot1ActionName => summonSlot1ActionName;
        public string SummonSlot2ActionName => summonSlot2ActionName;
        public string SummonSlot3ActionName => summonSlot3ActionName;
        public string RangedAimActionName => rangedAimActionName;
        public string WeaponSwapActionName => weaponSwapActionName;

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
            movement?.SetMoveInput(Vector2.zero);
            ReleaseHudLookAim();
            rangedBasicAttackAction?.SetFireHeld(false);
            ClearFirePointerState();
            ClearLookPointerState();
            previousBasicHeld = false;
        }

        private void Update()
        {
            BuildLayout();
            UpdateFirePointerState();
            UpdateLookPointerState();

            bool lookPointerBlocksDeviceFallback = lookPointerHeld && (!lookPointerUsesMouse || !lookPointerUsesRightMouse);
            bool anyHudHeld = IsHeld(moveUpRect)
                || IsHeld(moveDownRect)
                || IsHeld(moveLeftRect)
                || IsHeld(moveRightRect)
                || firePointerHeld
                || lookPointerBlocksDeviceFallback
                || IsHeld(dodgeRect)
                || IsHeld(swapRect)
                || IsHeld(skillRect)
                || IsHeld(summonSlot1Rect)
                || IsHeld(summonSlot2Rect)
                || IsHeld(summonSlot3Rect);

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

            if (IsPressed(summonSlot2Rect))
            {
                summonSlot2Action?.QueueSummon();
            }

            if (IsPressed(summonSlot3Rect))
            {
                summonSlot3Action?.QueueSummon();
            }
        }

        private void OnGUI()
        {
            if (!showHud)
            {
                return;
            }

            BuildLayout();
            EnsureStyles();
            DrawButton(moveUpRect, "UP", IsHeld(moveUpRect));
            DrawButton(moveDownRect, "DN", IsHeld(moveDownRect));
            DrawButton(moveLeftRect, "L", IsHeld(moveLeftRect));
            DrawButton(moveRightRect, "R", IsHeld(moveRightRect));
            DrawButton(basicRect, combatModeController != null && combatModeController.IsMeleeMode ? "SLASH" : "FIRE", firePointerHeld);
            DrawLookAimGuide();
            DrawFireAimReticle();
            DrawButton(dodgeRect, "DODGE", IsHeld(dodgeRect));
            DrawButton(swapRect, "SWAP", false);
            DrawButton(skillRect, "SKILL", false);
            DrawSummonButtons();
        }

        private Vector2 ResolveMoveInput()
        {
            Vector2 input = Vector2.zero;
            if (IsHeld(moveLeftRect))
            {
                input.x -= 1f;
            }

            if (IsHeld(moveRightRect))
            {
                input.x += 1f;
            }

            if (IsHeld(moveUpRect))
            {
                input.y += 1f;
            }

            if (IsHeld(moveDownRect))
            {
                input.y -= 1f;
            }

            return Vector2.ClampMagnitude(input, 1f);
        }

        private void BuildLayout()
        {
            float resolvedScale = ResolveScale();
            float size = buttonSize * resolvedScale;
            float gap = buttonGap * resolvedScale;
            float edge = margin * resolvedScale;

            float leftBaseX = edge + size;
            float leftBaseY = Screen.height - edge - size * 2f;
            moveUpRect = new Rect(leftBaseX, leftBaseY - size - gap, size, size);
            moveDownRect = new Rect(leftBaseX, leftBaseY + size + gap, size, size);
            moveLeftRect = new Rect(leftBaseX - size - gap, leftBaseY, size, size);
            moveRightRect = new Rect(leftBaseX + size + gap, leftBaseY, size, size);

            float rightX = Screen.width - edge - size;
            float bottomY = Screen.height - edge - size;
            basicRect = new Rect(rightX, bottomY, size, size);
            aimRect = Rect.zero;
            dodgeRect = new Rect(rightX, bottomY - size - gap, size, size);
            swapRect = new Rect(rightX - size - gap, bottomY - size - gap, size, size);
            skillRect = new Rect(rightX - size - gap, bottomY, size, size);

            float summonWidth = size * 1.55f;
            float summonHeight = size * 0.72f;
            float summonGap = gap * 0.48f;
            float summonX = Screen.width - edge - summonWidth;
            summonSlot1Rect = new Rect(summonX, edge, summonWidth, summonHeight);
            summonSlot2Rect = new Rect(summonX, edge + summonHeight + summonGap, summonWidth, summonHeight);
            summonSlot3Rect = new Rect(summonX, edge + (summonHeight + summonGap) * 2f, summonWidth, summonHeight);
        }

        private float ResolveScale()
        {
            float screenScale = Mathf.Clamp(Screen.height / 1440f, 0.72f, 1.35f);
            return screenScale * Mathf.Max(0.5f, scale);
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
                    BeginFirePointer(point, usesMouse: false, touch.touchId.ReadValue());
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

                UpdateFirePointer(ToGuiPoint(touch.position.ReadValue()));
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
                    UpdateFirePointer(point);
                }
                else
                {
                    ClearFirePointerState();
                }

                return;
            }

            if (Mouse.current.leftButton.wasPressedThisFrame && basicRect.Contains(point))
            {
                BeginFirePointer(point, usesMouse: true, touchId: -1);
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

        private void BeginFirePointer(Vector2 point, bool usesMouse, int touchId)
        {
            firePointerHeld = true;
            firePointerPressed = true;
            firePointerUsesMouse = usesMouse;
            firePointerTouchId = touchId;
            firePointerStartGuiPoint = point;
            UpdateFirePointer(point);
        }

        private void UpdateFirePointer(Vector2 point)
        {
            firePointerCurrentGuiPoint = point;
            fireAimInput = ResolveDragAimInput(firePointerStartGuiPoint, firePointerCurrentGuiPoint);
        }

        private void ClearFirePointerState()
        {
            firePointerHeld = false;
            firePointerPressed = false;
            firePointerUsesMouse = false;
            firePointerTouchId = -1;
            firePointerStartGuiPoint = Vector2.zero;
            firePointerCurrentGuiPoint = Vector2.zero;
            fireAimInput = Vector2.zero;
        }

        private void BeginLookPointer(Vector2 point, bool usesMouse, bool usesRightMouse, int touchId)
        {
            lookPointerHeld = true;
            lookPointerUsesMouse = usesMouse;
            lookPointerUsesRightMouse = usesRightMouse;
            lookPointerTouchId = touchId;
            lookPointerStartGuiPoint = point;
            UpdateLookPointer(point);
        }

        private void UpdateLookPointer(Vector2 point)
        {
            lookPointerCurrentGuiPoint = point;
            lookAimInput = ResolveDragAimInput(lookPointerStartGuiPoint, lookPointerCurrentGuiPoint);
        }

        private void ClearLookPointerState()
        {
            lookPointerHeld = false;
            lookPointerUsesMouse = false;
            lookPointerUsesRightMouse = false;
            lookPointerTouchId = -1;
            lookPointerStartGuiPoint = Vector2.zero;
            lookPointerCurrentGuiPoint = Vector2.zero;
            lookAimInput = Vector2.zero;
        }

        private void UpdateHudLookAim()
        {
            bool shouldHoldFireAim = fireDragControlsAim
                && firePointerHeld
                && (combatModeController == null || combatModeController.IsRangedMode);
            bool shouldHoldLookAim = screenDragControlsAim && lookPointerHeld;
            bool shouldHoldAnyAim = shouldHoldFireAim || shouldHoldLookAim;
            if (!shouldHoldAnyAim && !hudLookAimActive)
            {
                return;
            }

            Vector2 aimInput = ResolveHudAimInput(shouldHoldFireAim, shouldHoldLookAim);
            movement?.SetLookInput(routeAimToMovementLook ? aimInput : Vector2.zero);
            rangedBasicAttackAction?.SetAimInput(aimInput);
            aimController?.SetAimHeld(shouldHoldAnyAim);
            hudLookAimActive = shouldHoldAnyAim;
        }

        private void ReleaseHudLookAim()
        {
            if (!hudLookAimActive)
            {
                return;
            }

            movement?.SetLookInput(Vector2.zero);
            rangedBasicAttackAction?.SetAimInput(Vector2.zero);
            aimController?.SetAimHeld(false);
            hudLookAimActive = false;
        }

        private Vector2 ResolveDragAimInput(Vector2 startGuiPoint, Vector2 currentGuiPoint)
        {
            float radius = Mathf.Max(1f, lookAimDragRadius * ResolveScale());
            Vector2 delta = currentGuiPoint - startGuiPoint;
            Vector2 input = Vector2.ClampMagnitude(new Vector2(delta.x, -delta.y) / radius, 1f);
            return input.sqrMagnitude >= lookAimDragDeadZone * lookAimDragDeadZone ? input : Vector2.zero;
        }

        private Vector2 ResolveHudAimInput(bool shouldHoldFireAim, bool shouldHoldLookAim)
        {
            if (shouldHoldFireAim && shouldHoldLookAim)
            {
                return fireAimInput.sqrMagnitude >= lookAimInput.sqrMagnitude ? fireAimInput : lookAimInput;
            }

            if (shouldHoldFireAim)
            {
                return fireAimInput;
            }

            return shouldHoldLookAim ? lookAimInput : Vector2.zero;
        }

        private bool IsLookAimStartPoint(Vector2 point)
        {
            return point.x >= Screen.width * lookAimScreenMinX && !IsHudControlPoint(point);
        }

        private bool IsHudControlPoint(Vector2 point)
        {
            return moveUpRect.Contains(point)
                || moveDownRect.Contains(point)
                || moveLeftRect.Contains(point)
                || moveRightRect.Contains(point)
                || basicRect.Contains(point)
                || dodgeRect.Contains(point)
                || swapRect.Contains(point)
                || skillRect.Contains(point)
                || summonSlot1Rect.Contains(point)
                || summonSlot2Rect.Contains(point)
                || summonSlot3Rect.Contains(point);
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
            GUI.Box(rect, label, held ? heldButtonStyle : pending ? pendingButtonStyle : buttonStyle);
        }

        private void DrawSummonButtons()
        {
            DrawButton(summonSlot1Rect, BuildSummonSlot1Label(), IsHeld(summonSlot1Rect));
            DrawButton(
                summonSlot2Rect,
                BuildSupportSummonLabel(summonSlot2Action, summonSlot2Label),
                IsHeld(summonSlot2Rect),
                pending: summonSlot2Action == null);
            DrawButton(
                summonSlot3Rect,
                BuildSupportSummonLabel(summonSlot3Action, summonSlot3Label),
                IsHeld(summonSlot3Rect),
                pending: summonSlot3Action == null);
        }

        private string BuildSummonSlot1Label()
        {
            int availableTier = energyLadder != null ? energyLadder.AvailableTier : 0;
            if (availableTier > 0)
            {
                string tierName = TryGetSummonTierShortName(availableTier);
                return $"{summonSlot1Label}\nREADY LV{availableTier} {tierName}".TrimEnd();
            }

            if (energyLadder == null)
            {
                return $"{summonSlot1Label}\nREADY?";
            }

            int chargingTier = Mathf.Clamp(energyLadder.ChargingTier, 1, 3);
            int fillPercent = Mathf.RoundToInt(energyLadder.CurrentTierFillRatio * 100f);
            return $"{summonSlot1Label}\nLV{chargingTier} {fillPercent}%";
        }

        private string TryGetSummonTierShortName(int tier)
        {
            if (summonSlot1Action == null
                || !summonSlot1Action.TryGetTierReadout(
                    tier,
                    out SummonSlotActionProfile.SummonTierReadout readout)
                || string.IsNullOrWhiteSpace(readout.TierLabel))
            {
                return string.Empty;
            }

            string displayName = readout.TierLabel.Trim();
            int firstSpaceIndex = displayName.IndexOf(' ');
            if (firstSpaceIndex <= 0 || firstSpaceIndex >= displayName.Length - 1)
            {
                return displayName.Length <= 10 ? displayName : string.Empty;
            }

            string trailingName = displayName.Substring(firstSpaceIndex + 1).Trim();
            return trailingName.Length <= 10 ? trailingName : string.Empty;
        }

        private string BuildPendingSummonLabel(string slotLabel)
        {
            return $"{slotLabel}\n{lockedSummonLabel}";
        }

        private string BuildSupportSummonLabel(PlayerSupportSummonSlotAction supportAction, string slotLabel)
        {
            if (supportAction == null)
            {
                return BuildPendingSummonLabel(slotLabel);
            }

            int availableTier = energyLadder != null ? energyLadder.AvailableTier : 0;
            if (availableTier > 0)
            {
                string tierName = TryGetSupportSummonTierShortName(supportAction, availableTier);
                return $"{slotLabel}\nREADY LV{availableTier} {tierName}".TrimEnd();
            }

            if (energyLadder == null)
            {
                return $"{slotLabel}\nREADY?";
            }

            int chargingTier = Mathf.Clamp(energyLadder.ChargingTier, 1, 3);
            int fillPercent = Mathf.RoundToInt(energyLadder.CurrentTierFillRatio * 100f);
            return $"{slotLabel}\nLV{chargingTier} {fillPercent}%";
        }

        private static string TryGetSupportSummonTierShortName(
            PlayerSupportSummonSlotAction supportAction,
            int tier)
        {
            if (supportAction == null
                || !supportAction.TryGetTierReadout(
                    tier,
                    out SummonSlotActionProfile.SummonTierReadout readout)
                || string.IsNullOrWhiteSpace(readout.TierLabel))
            {
                return string.Empty;
            }

            string displayName = readout.TierLabel.Trim();
            int firstSpaceIndex = displayName.IndexOf(' ');
            if (firstSpaceIndex <= 0 || firstSpaceIndex >= displayName.Length - 1)
            {
                return displayName.Length <= 10 ? displayName : string.Empty;
            }

            string trailingName = displayName.Substring(firstSpaceIndex + 1).Trim();
            return trailingName.Length <= 10 ? trailingName : string.Empty;
        }

        private void DrawLookAimGuide()
        {
            if (!TryGetAimGuide(out Vector2 startGuiPoint, out Vector2 input))
            {
                return;
            }

            float resolvedScale = ResolveScale();
            float radius = lookAimDragRadius * resolvedScale;
            float knobSize = lookAimKnobSize * resolvedScale;
            Vector2 knobCenter = startGuiPoint + new Vector2(input.x, -input.y) * radius;
            Rect knobRect = new Rect(
                knobCenter.x - knobSize * 0.5f,
                knobCenter.y - knobSize * 0.5f,
                knobSize,
                knobSize);
            GUI.Box(knobRect, string.Empty, heldButtonStyle);
        }

        private bool TryGetAimGuide(out Vector2 startGuiPoint, out Vector2 input)
        {
            bool hasFireAim = fireDragControlsAim && firePointerHeld && fireAimInput.sqrMagnitude > 0.0001f;
            bool hasLookAim = lookPointerHeld && lookAimInput.sqrMagnitude > 0.0001f;
            if (hasFireAim && (!hasLookAim || fireAimInput.sqrMagnitude >= lookAimInput.sqrMagnitude))
            {
                startGuiPoint = firePointerStartGuiPoint;
                input = fireAimInput;
                return true;
            }

            if (hasLookAim)
            {
                startGuiPoint = lookPointerStartGuiPoint;
                input = lookAimInput;
                return true;
            }

            startGuiPoint = Vector2.zero;
            input = Vector2.zero;
            return false;
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

            Vector2 center = ResolveFireAimReticleGuiPoint();
            float resolvedScale = ResolveScale();
            float size = fireAimReticleSize * resolvedScale;
            float gap = fireAimReticleGap * resolvedScale;
            float thickness = fireAimReticleThickness * resolvedScale;

            Color previousColor = GUI.color;
            GUI.color = fireAimReticleColor;
            DrawReticleSegment(new Rect(center.x - gap - size, center.y - thickness * 0.5f, size, thickness));
            DrawReticleSegment(new Rect(center.x + gap, center.y - thickness * 0.5f, size, thickness));
            DrawReticleSegment(new Rect(center.x - thickness * 0.5f, center.y - gap - size, thickness, size));
            DrawReticleSegment(new Rect(center.x - thickness * 0.5f, center.y + gap, thickness, size));
            GUI.color = previousColor;
        }

        private bool IsRangedAimReticleVisible()
        {
            if (combatModeController != null && !combatModeController.IsRangedMode)
            {
                return false;
            }

            return true;
        }

        private Vector2 ResolveFireAimReticleGuiPoint()
        {
            Vector2 viewportPoint = new Vector2(0.5f, 0.5f);
            if (rangedBasicAttackAction != null
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
