using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DimensionBrawl.Player
{
    public enum PlayerCombatMode
    {
        Ranged = 0,
        Melee = 1
    }

    [DisallowMultipleComponent]
    public sealed class PlayerCombatModeController : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputActionReference combatModeSwapAction;
        [SerializeField] private bool useKeyboardWhenActionMissing = true;
        [SerializeField] private Key keyboardTestKey = Key.Tab;

        [Header("References")]
        [SerializeField] private PlayerActionController actionController;
        [SerializeField] private GameObject rangedVisualRoot;
        [SerializeField] private GameObject meleeVisualRoot;

        [Header("Profiles")]
        [SerializeField] private PlayerActionProfile rangedActionProfile;
        [SerializeField] private PlayerActionProfile meleeActionProfile;
        [SerializeField] private PlayerCombatMode startingMode = PlayerCombatMode.Ranged;

        private bool actionEnabledHere;
        private bool queuedSwap;

        public PlayerCombatMode CurrentMode { get; private set; }
        public PlayerActionProfile CurrentActionProfile => CurrentMode == PlayerCombatMode.Melee
            ? meleeActionProfile
            : rangedActionProfile;
        public bool IsRangedMode => CurrentMode == PlayerCombatMode.Ranged;
        public bool IsMeleeMode => CurrentMode == PlayerCombatMode.Melee;

        public event Action<PlayerCombatMode> CombatModeChanged;

        private void Awake()
        {
            if (actionController == null)
            {
                actionController = GetComponent<PlayerActionController>();
            }
        }

        private void OnEnable()
        {
            actionEnabledHere = EnableActionIfNeeded(combatModeSwapAction);
            ApplyMode(startingMode, true);
        }

        private void OnDisable()
        {
            DisableActionIfOwned(combatModeSwapAction, actionEnabledHere);
            actionEnabledHere = false;
            queuedSwap = false;
        }

        private void Update()
        {
            if (ReadSwapPressed())
            {
                ToggleCombatMode();
            }
        }

        public void QueueCombatModeSwap()
        {
            queuedSwap = true;
        }

        public void ToggleCombatMode()
        {
            SetCombatMode(CurrentMode == PlayerCombatMode.Ranged
                ? PlayerCombatMode.Melee
                : PlayerCombatMode.Ranged);
        }

        public void SetRangedMode()
        {
            SetCombatMode(PlayerCombatMode.Ranged);
        }

        public void SetMeleeMode()
        {
            SetCombatMode(PlayerCombatMode.Melee);
        }

        public void SetCombatMode(PlayerCombatMode combatMode)
        {
            ApplyMode(combatMode, false);
        }

        private void ApplyMode(PlayerCombatMode combatMode, bool force)
        {
            if (!force && CurrentMode == combatMode)
            {
                return;
            }

            CurrentMode = combatMode;
            actionController?.SetActionProfile(CurrentActionProfile);
            SetVisualRootActive(rangedVisualRoot, combatMode == PlayerCombatMode.Ranged);
            SetVisualRootActive(meleeVisualRoot, combatMode == PlayerCombatMode.Melee);
            CombatModeChanged?.Invoke(CurrentMode);
        }

        private bool ReadSwapPressed()
        {
            bool pressed = queuedSwap;
            queuedSwap = false;

            if (combatModeSwapAction != null && combatModeSwapAction.action != null)
            {
                pressed |= combatModeSwapAction.action.WasPressedThisFrame();
            }

            if (pressed || !useKeyboardWhenActionMissing || !IsActionMissing(combatModeSwapAction))
            {
                return pressed;
            }

            return Keyboard.current != null
                && Keyboard.current[keyboardTestKey] != null
                && Keyboard.current[keyboardTestKey].wasPressedThisFrame;
        }

        private static void SetVisualRootActive(GameObject root, bool active)
        {
            if (root != null && root.activeSelf != active)
            {
                root.SetActive(active);
            }
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

        private static bool IsActionMissing(InputActionReference actionReference)
        {
            return actionReference == null || actionReference.action == null;
        }
    }
}
