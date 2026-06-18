using System;
using UnityEngine;
using UnityEngine.Animations;

namespace DimensionBrawl.Presentation
{
    [DisallowMultipleComponent]
    public sealed class RifleGirlWeaponSocketDriver : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Animator animator;
        [SerializeField] private ParentConstraint rifleConstraint;
        [SerializeField] private Transform leftHandIkTarget;

        [Header("Commands")]
        [SerializeField] private string defaultCommands = "To_Hand_R_Socket, IK_ON_Left_Handle";
        [SerializeField] private string handSocketCommand = "To_Hand_R_Socket";
        [SerializeField] private string holsterSocketCommand = "To_Put_Socket_Rifle";
        [SerializeField] private string aimSocketCommand = "To_add_weapon_r";
        [SerializeField] private string leftIkOnCommand = "IK_ON_Left_Handle";
        [SerializeField] private string leftIkOffCommand = "IK_OFF_Left_Handle";

        [Header("IK")]
        [SerializeField] private AvatarIKGoal leftIkGoal = AvatarIKGoal.LeftHand;
        [SerializeField, Range(0f, 1f)] private float leftIkMaxWeight = 1f;
        [SerializeField, Min(0f)] private float leftIkBlendSpeed = 15f;

        private float leftIkCurrentWeight;
        private float leftIkTargetWeight;

        public bool IsConfigured => animator != null && rifleConstraint != null && leftHandIkTarget != null;

        public void Configure(Animator newAnimator, ParentConstraint newRifleConstraint, Transform newLeftHandIkTarget)
        {
            animator = newAnimator;
            rifleConstraint = newRifleConstraint;
            leftHandIkTarget = newLeftHandIkTarget;
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
            SwitchSocketByString(defaultCommands);
            leftIkCurrentWeight = leftIkTargetWeight;
        }

        public void SwitchSocket(AnimationEvent animationEvent)
        {
            SwitchSocketByString(animationEvent.stringParameter);
        }

        public void SwitchSocketByString(string commands)
        {
            if (string.IsNullOrWhiteSpace(commands))
            {
                return;
            }

            string[] splitCommands = commands.Split(',');
            for (int i = 0; i < splitCommands.Length; i++)
            {
                ApplyCommand(splitCommands[i].Trim());
            }
        }

        private void ApplyCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                return;
            }

            if (string.Equals(command, leftIkOnCommand, StringComparison.OrdinalIgnoreCase))
            {
                leftIkTargetWeight = leftIkMaxWeight;
                return;
            }

            if (string.Equals(command, leftIkOffCommand, StringComparison.OrdinalIgnoreCase))
            {
                leftIkTargetWeight = 0f;
                return;
            }

            if (string.Equals(command, handSocketCommand, StringComparison.OrdinalIgnoreCase))
            {
                SetRifleConstraintSource(0);
                return;
            }

            if (string.Equals(command, holsterSocketCommand, StringComparison.OrdinalIgnoreCase))
            {
                SetRifleConstraintSource(1);
                return;
            }

            if (string.Equals(command, aimSocketCommand, StringComparison.OrdinalIgnoreCase))
            {
                SetRifleConstraintSource(2);
            }
        }

        private void SetRifleConstraintSource(int activeIndex)
        {
            if (rifleConstraint == null)
            {
                return;
            }

            rifleConstraint.constraintActive = true;
            int count = rifleConstraint.sourceCount;
            for (int i = 0; i < count; i++)
            {
                ConstraintSource source = rifleConstraint.GetSource(i);
                source.weight = i == activeIndex ? 1f : 0f;
                rifleConstraint.SetSource(i, source);
            }
        }

        private void OnAnimatorIK(int layerIndex)
        {
            if (animator == null || leftHandIkTarget == null)
            {
                return;
            }

            leftIkCurrentWeight = Mathf.Lerp(
                leftIkCurrentWeight,
                leftIkTargetWeight,
                Time.deltaTime * leftIkBlendSpeed);

            animator.SetIKPositionWeight(leftIkGoal, leftIkCurrentWeight);
            animator.SetIKRotationWeight(leftIkGoal, leftIkCurrentWeight);
            animator.SetIKPosition(leftIkGoal, leftHandIkTarget.position);
            animator.SetIKRotation(leftIkGoal, leftHandIkTarget.rotation);
        }
    }
}
