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
        private bool previousAimHeld;
        private bool previousBasicHeld;

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
            previousAimHeld = false;
            previousBasicHeld = false;
        }

        private void Update()
        {
            BuildLayout();

            bool anyHudHeld = IsHeld(moveUpRect)
                || IsHeld(moveDownRect)
                || IsHeld(moveLeftRect)
                || IsHeld(moveRightRect)
                || IsHeld(basicRect)
                || IsHeld(aimRect)
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

            bool aimHeld = IsHeld(aimRect);
            if (aimHeld || previousAimHeld)
            {
                aimController?.SetAimHeld(aimHeld);
            }
            previousAimHeld = aimHeld;

            bool basicHeld = IsHeld(basicRect);
            bool basicPressed = IsPressed(basicRect);
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
            DrawButton(aimRect, "AIM", IsHeld(aimRect));
            DrawButton(basicRect, combatModeController != null && combatModeController.IsMeleeMode ? "SLASH" : "FIRE", IsHeld(basicRect));
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
            aimRect = new Rect(rightX - size - gap, bottomY, size, size);
            dodgeRect = new Rect(rightX, bottomY - size - gap, size, size);
            swapRect = new Rect(rightX - size - gap, bottomY - size - gap, size, size);
            skillRect = new Rect(rightX - (size + gap) * 2f, bottomY - size * 0.5f, size, size);
            summonRect = new Rect(Screen.width - edge - size, edge, size * 1.35f, size * 0.82f);
        }

        private float ResolveScale()
        {
            float screenScale = Mathf.Clamp(Screen.height / 1440f, 0.72f, 1.35f);
            return screenScale * Mathf.Max(0.5f, scale);
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
