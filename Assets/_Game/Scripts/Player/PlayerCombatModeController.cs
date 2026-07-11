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
        [SerializeField] private PlayerMovementController movementController;
        [SerializeField] private PlayerRangedAimController rangedAimController;
        [SerializeField] private PlayerRangedBasicAttackAction rangedBasicAttackAction;
        [SerializeField] private GameObject rangedVisualRoot;
        [SerializeField] private GameObject meleeVisualRoot;
        [SerializeField] private GameObject rangedWeaponRoot;
        [SerializeField] private GameObject meleeWeaponRoot;
        [SerializeField] private Animator rangedAnimator;
        [SerializeField] private Animator meleeAnimator;
        [SerializeField] private RuntimeAnimatorController rangedAnimatorController;
        [SerializeField] private RuntimeAnimatorController meleeAnimatorController;
        [SerializeField] private bool routeAnimatorsByMode = true;
        [Tooltip("Ranged visual uses a native animator bridge, so generic movement/action Animator parameters are not routed to it.")]
        [SerializeField] private bool rangedAnimatorUsesExternalPresentationBridge;
        [Tooltip("Use one character body and swap only weapons/Animator Controller for combat mode changes.")]
        [SerializeField] private bool useSingleCharacterVisual;

        [Header("Profiles")]
        [SerializeField] private PlayerActionProfile rangedActionProfile;
        [SerializeField] private PlayerActionProfile meleeActionProfile;
        [SerializeField] private PlayerCombatMode startingMode = PlayerCombatMode.Ranged;

        private bool actionEnabledHere;
        private bool queuedSwap;
        private bool cinematicInputLocked;
        private InputAction subscribedSwapInputAction;
        private InputAction keyboardFallbackAction;

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

            if (movementController == null)
            {
                movementController = GetComponent<PlayerMovementController>();
            }

            if (rangedAimController == null)
            {
                rangedAimController = GetComponent<PlayerRangedAimController>();
            }

            if (rangedBasicAttackAction == null)
            {
                rangedBasicAttackAction = GetComponent<PlayerRangedBasicAttackAction>();
            }
        }

        private void OnEnable()
        {
            actionEnabledHere = EnableActionIfNeeded(combatModeSwapAction);
            SubscribeInput();
            ApplyMode(startingMode, true);
        }

        private void OnDisable()
        {
            UnsubscribeInput();
            DisableActionIfOwned(combatModeSwapAction, actionEnabledHere);
            actionEnabledHere = false;
            queuedSwap = false;
        }

        public void QueueCombatModeSwap()
        {
            if (!CanAcceptQueuedInput())
            {
                queuedSwap = false;
                return;
            }

            queuedSwap = true;
            ConsumeQueuedSwap();
        }

        public void SetCinematicInputLocked(bool locked)
        {
            cinematicInputLocked = locked;
            if (locked)
            {
                queuedSwap = false;
            }
        }

        public void ToggleCombatMode()
        {
            SetCombatMode(CurrentMode == PlayerCombatMode.Ranged
                ? PlayerCombatMode.Melee
                : PlayerCombatMode.Ranged);
        }

        public void SetRangedMode()
        {
            queuedSwap = false;
            SetCombatMode(PlayerCombatMode.Ranged);
        }

        public void SetMeleeMode()
        {
            queuedSwap = false;
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
            ApplyVisualMode(combatMode);
            Animator activeAnimator = ResolveActiveAnimator(combatMode);
            ApplyAnimatorController(activeAnimator, combatMode);
            RoutePresentationAnimator(activeAnimator, combatMode);
            CombatModeChanged?.Invoke(CurrentMode);
        }

        private void ApplyVisualMode(PlayerCombatMode combatMode)
        {
            if (useSingleCharacterVisual)
            {
                SetVisualRootActive(rangedVisualRoot, true);
                SetVisualRootActive(meleeVisualRoot, false);
                SetVisualRootActive(rangedWeaponRoot, combatMode == PlayerCombatMode.Ranged);
                SetVisualRootActive(meleeWeaponRoot, combatMode == PlayerCombatMode.Melee);
                return;
            }

            SetVisualRootActive(rangedVisualRoot, combatMode == PlayerCombatMode.Ranged);
            SetVisualRootActive(meleeVisualRoot, combatMode == PlayerCombatMode.Melee);
        }

        private Animator ResolveActiveAnimator(PlayerCombatMode combatMode)
        {
            if (useSingleCharacterVisual)
            {
                return rangedAnimator != null ? rangedAnimator : meleeAnimator;
            }

            return combatMode == PlayerCombatMode.Ranged ? rangedAnimator : meleeAnimator;
        }

        private void ApplyAnimatorController(Animator activeAnimator, PlayerCombatMode combatMode)
        {
            if (!useSingleCharacterVisual || activeAnimator == null)
            {
                return;
            }

            RuntimeAnimatorController targetController = combatMode == PlayerCombatMode.Ranged
                ? rangedAnimatorController
                : meleeAnimatorController;
            if (targetController == null || activeAnimator.runtimeAnimatorController == targetController)
            {
                return;
            }

            activeAnimator.runtimeAnimatorController = targetController;
            activeAnimator.Rebind();
            if (activeAnimator.isActiveAndEnabled)
            {
                activeAnimator.Update(0f);
            }
        }

        private void RoutePresentationAnimator(Animator activeAnimator, PlayerCombatMode combatMode)
        {
            if (routeAnimatorsByMode && activeAnimator != null)
            {
                bool externalBridge = combatMode == PlayerCombatMode.Ranged
                    && rangedAnimatorUsesExternalPresentationBridge;
                Animator movementActionAnimator = externalBridge ? null : activeAnimator;
                movementController?.SetAnimator(movementActionAnimator);
                actionController?.SetAnimator(movementActionAnimator);
            }

            if (activeAnimator != null)
            {
                rangedAimController?.SetAnimator(activeAnimator);
                rangedBasicAttackAction?.SetAnimator(activeAnimator);
            }
        }

        private void SubscribeInput()
        {
            InputAction action = combatModeSwapAction != null ? combatModeSwapAction.action : null;
            if (action != null)
            {
                subscribedSwapInputAction = action;
                subscribedSwapInputAction.performed += HandleSwapPerformed;
                return;
            }

            if (!useKeyboardWhenActionMissing
                || keyboardTestKey == Key.None
                || Application.isMobilePlatform)
            {
                return;
            }

            keyboardFallbackAction = new InputAction(
                "CombatModeSwap.KeyboardFallback",
                InputActionType.Button,
                $"<Keyboard>/{keyboardTestKey}");
            keyboardFallbackAction.performed += HandleSwapPerformed;
            keyboardFallbackAction.Enable();
        }

        private void UnsubscribeInput()
        {
            if (subscribedSwapInputAction != null)
            {
                subscribedSwapInputAction.performed -= HandleSwapPerformed;
                subscribedSwapInputAction = null;
            }

            if (keyboardFallbackAction == null)
            {
                return;
            }

            keyboardFallbackAction.performed -= HandleSwapPerformed;
            keyboardFallbackAction.Disable();
            keyboardFallbackAction.Dispose();
            keyboardFallbackAction = null;
        }

        private void HandleSwapPerformed(InputAction.CallbackContext context)
        {
            if (CanAcceptQueuedInput())
            {
                ToggleCombatMode();
            }
        }

        private void ConsumeQueuedSwap()
        {
            if (!queuedSwap)
            {
                return;
            }

            queuedSwap = false;
            if (CanAcceptQueuedInput())
            {
                ToggleCombatMode();
            }
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

        private bool CanAcceptQueuedInput()
        {
            return isActiveAndEnabled && !cinematicInputLocked;
        }
    }
}
