using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace DimensionBrawl.Test
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator))]
    public sealed class RifleGirlNativeMotionReviewDriver : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private bool showReviewHud = true;

        [Header("Native RifleGirl Trigger Names")]
        [SerializeField] private string idleTrigger = "IDLE 0";
        [SerializeField] private string shootTrigger = "SHOOT";
        [SerializeField] private string autoShootTrigger = "AUTO SHOOT";
        [SerializeField] private string reloadTrigger = "RELOAD";
        [SerializeField] private string aimJogTrigger = "JOG";
        [SerializeField] private string walkForwardTrigger = "WALK F";
        [SerializeField] private string walkBackTrigger = "WALK B";
        [SerializeField] private string walkLeftTrigger = "WALK FL";
        [SerializeField] private string walkRightTrigger = "WALK FR";
        [SerializeField] private string turnLeftTrigger = "TURN L";
        [SerializeField] private string turnRightTrigger = "TURN R";
        [SerializeField] private string crouchIdleTrigger = "CROUCH IDLE 0";
        [SerializeField] private string takeGunTrigger = "TAKE";
        [SerializeField] private string putGunTrigger = "PUT";

        private string lastCommand = "None";

        private void Awake()
        {
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }
        }

        public void Configure(Animator newAnimator)
        {
            animator = newAnimator;
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || animator == null)
            {
                return;
            }

            TriggerIfPressed(keyboard, Key.Digit1, idleTrigger);
            TriggerIfPressed(keyboard, Key.F, shootTrigger);
            TriggerIfPressed(keyboard, Key.G, autoShootTrigger);
            TriggerIfPressed(keyboard, Key.R, reloadTrigger);
            TriggerIfPressed(keyboard, Key.Q, turnLeftTrigger);
            TriggerIfPressed(keyboard, Key.E, turnRightTrigger);
            TriggerIfPressed(keyboard, Key.C, crouchIdleTrigger);
            TriggerIfPressed(keyboard, Key.T, takeGunTrigger);
            TriggerIfPressed(keyboard, Key.Y, putGunTrigger);

            if (keyboard.wKey.wasPressedThisFrame)
            {
                Trigger(IsShiftHeld(keyboard) ? aimJogTrigger : walkForwardTrigger);
            }

            if (keyboard.sKey.wasPressedThisFrame)
            {
                Trigger(walkBackTrigger);
            }

            if (keyboard.aKey.wasPressedThisFrame)
            {
                Trigger(walkLeftTrigger);
            }

            if (keyboard.dKey.wasPressedThisFrame)
            {
                Trigger(walkRightTrigger);
            }
        }

        private void OnGUI()
        {
            if (!showReviewHud)
            {
                return;
            }

            GUILayout.BeginArea(new Rect(16f, 16f, 420f, 210f), GUI.skin.box);
            GUILayout.Label("RifleGirl Native Motion Review");
            GUILayout.Label("1 Idle / F Shoot / G AutoShoot / R Reload");
            GUILayout.Label("W WalkF / Shift+W Jog / S WalkB / A,D Diagonal Walk");
            GUILayout.Label("Q,E Aim Turn / C CrouchIdle / T Take / Y Put");
            GUILayout.Label($"Last: {lastCommand}");
            GUILayout.EndArea();
        }

        private void TriggerIfPressed(Keyboard keyboard, Key key, string triggerName)
        {
            KeyControl control = keyboard[key];
            if (control != null && control.wasPressedThisFrame)
            {
                Trigger(triggerName);
            }
        }

        private void Trigger(string triggerName)
        {
            if (string.IsNullOrWhiteSpace(triggerName))
            {
                return;
            }

            animator.SetTrigger(triggerName);
            lastCommand = triggerName;
        }

        private static bool IsShiftHeld(Keyboard keyboard)
        {
            return keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;
        }
    }
}
