using DimensionBrawl.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DimensionBrawl.UI
{
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

        [Header("Canonical Actions")]
        [SerializeField] private string moveActionName = "Move";
        [SerializeField] private string basicDefenseActionName = "BasicDefenseAttack";
        [SerializeField] private string dodgeActionName = "Dodge";
        [SerializeField] private string skill1ActionName = "Skill1";
        [SerializeField] private string summonSlot1ActionName = "SummonSlot1";
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
        [SerializeField] private Color actionTextColor = Color.white;

        [Header("Fire Aim")]
        [SerializeField] private bool fireButtonHoldsAim = true;
        [SerializeField, Range(0f, 1f)] private float fireAimDragDeadZone = 0.18f;
        [SerializeField, Min(8f)] private float fireAimDragRadius = 110f;
        [SerializeField, Min(8f)] private float fireAimKnobSize = 34f;

        private Rect moveUpRect;
        private Rect moveDownRect;
        private Rect moveLeftRect;
        private Rect moveRightRect;
        private Rect basicRect;
        private Rect aimRect;
        private Rect dodgeRect;
        private Rect swapRect;
        private Rect skillRect;
        private Rect summonRect;
        private GUIStyle buttonStyle;
        private GUIStyle heldButtonStyle;
        private bool previousBasicHeld;
        private bool firePointerHeld;
        private bool firePointerPressed;
        private bool firePointerUsesMouse;
        private int firePointerTouchId = -1;
        private Vector2 firePointerStartGuiPoint;
        private Vector2 firePointerCurrentGuiPoint;
        private Vector2 fireAimInput;

        public string MoveActionName => moveActionName;
        public string BasicDefenseActionName => basicDefenseActionName;
        public string DodgeActionName => dodgeActionName;
        public string Skill1ActionName => skill1ActionName;
        public string SummonSlot1ActionName => summonSlot1ActionName;
        public string RangedAimActionName => rangedAimActionName;
        public string WeaponSwapActionName => weaponSwapActionName;

        public void Configure(
            PlayerMovementController newMovement,
            PlayerActionController newActionController,
            PlayerCombatModeController newCombatModeController,
            PlayerRangedAimController newAimController,
            PlayerRangedBasicAttackAction newRangedBasicAttackAction,
            PlayerSkill1Action newSkill1Action,
            PlayerSummonSlot1Action newSummonSlot1Action)
        {
            movement = newMovement;
            actionController = newActionController;
            combatModeController = newCombatModeController;
            aimController = newAimController;
            rangedBasicAttackAction = newRangedBasicAttackAction;
            skill1Action = newSkill1Action;
            summonSlot1Action = newSummonSlot1Action;
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
        }

        private void OnDisable()
        {
            movement?.SetMoveInput(Vector2.zero);
            aimController?.SetAimHeld(false);
            rangedBasicAttackAction?.SetFireHeld(false);
            rangedBasicAttackAction?.ClearAimInput();
            ClearFirePointerState();
            previousBasicHeld = false;
        }

        private void Update()
        {
            BuildLayout();
            UpdateFirePointerState();

            bool anyHudHeld = IsHeld(moveUpRect)
                || IsHeld(moveDownRect)
                || IsHeld(moveLeftRect)
                || IsHeld(moveRightRect)
                || firePointerHeld
                || IsHeld(dodgeRect)
                || IsHeld(swapRect)
                || IsHeld(skillRect)
                || IsHeld(summonRect);

            if (anyHudHeld)
            {
                actionController?.SuppressBasicAttackDeviceFallbackThisFrame();
                rangedBasicAttackAction?.SuppressDeviceFallbackThisFrame();
            }

            Vector2 moveInput = ResolveMoveInput();
            movement?.SetMoveInput(moveInput);

            bool basicHeld = firePointerHeld;
            bool basicPressed = firePointerPressed;
            if (combatModeController == null || combatModeController.IsRangedMode)
            {
                if (basicHeld || previousBasicHeld)
                {
                    rangedBasicAttackAction?.SetFireHeld(basicHeld);
                    rangedBasicAttackAction?.SetAimInput(basicHeld ? fireAimInput : Vector2.zero);
                    if (fireButtonHoldsAim)
                    {
                        aimController?.SetAimHeld(basicHeld);
                    }
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
                    rangedBasicAttackAction?.ClearAimInput();
                    if (fireButtonHoldsAim)
                    {
                        aimController?.SetAimHeld(false);
                    }
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

            if (IsPressed(summonRect))
            {
                summonSlot1Action?.QueueSummonSlot1();
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
            DrawFireAimGuide();
            DrawButton(dodgeRect, "DODGE", IsHeld(dodgeRect));
            DrawButton(swapRect, "SWAP", false);
            DrawButton(skillRect, "SKILL", false);
            DrawButton(summonRect, "SUMMON", false);
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
            summonRect = new Rect(Screen.width - edge - size, edge, size * 1.35f, size * 0.82f);
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
            fireAimInput = ResolveFireAimInput();
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

        private Vector2 ResolveFireAimInput()
        {
            float radius = Mathf.Max(1f, fireAimDragRadius * ResolveScale());
            Vector2 delta = firePointerCurrentGuiPoint - firePointerStartGuiPoint;
            Vector2 input = Vector2.ClampMagnitude(new Vector2(delta.x, -delta.y) / radius, 1f);
            return input.sqrMagnitude >= fireAimDragDeadZone * fireAimDragDeadZone ? input : Vector2.zero;
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

        private void DrawButton(Rect rect, string label, bool held)
        {
            GUI.Box(rect, label, held ? heldButtonStyle : buttonStyle);
        }

        private void DrawFireAimGuide()
        {
            if (!firePointerHeld || fireAimInput.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            float resolvedScale = ResolveScale();
            float radius = fireAimDragRadius * resolvedScale;
            float knobSize = fireAimKnobSize * resolvedScale;
            Vector2 center = basicRect.center;
            Vector2 knobCenter = center + new Vector2(fireAimInput.x, -fireAimInput.y) * radius;
            Rect knobRect = new Rect(
                knobCenter.x - knobSize * 0.5f,
                knobCenter.y - knobSize * 0.5f,
                knobSize,
                knobSize);
            GUI.Box(knobRect, string.Empty, heldButtonStyle);
        }

        private void EnsureStyles()
        {
            if (buttonStyle != null && heldButtonStyle != null)
            {
                return;
            }

            buttonStyle = CreateButtonStyle(buttonColor);
            heldButtonStyle = CreateButtonStyle(heldButtonColor);
        }

        private GUIStyle CreateButtonStyle(Color color)
        {
            var style = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize = Mathf.RoundToInt(Mathf.Clamp(Screen.height / 48f, 18f, 34f)),
                normal = { textColor = actionTextColor },
                padding = new RectOffset(4, 4, 4, 4)
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
    }
}
