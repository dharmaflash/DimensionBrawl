using UnityEngine;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    public sealed class LobbyCharacterAnimatorPresenter : MonoBehaviour
    {
        [SerializeField] private Transform targetRoot;
        [SerializeField] private RuntimeAnimatorController controller;
        [SerializeField] private Avatar avatar;
        [SerializeField] private string defaultStateName = "Idle";
        [SerializeField, Range(0f, 1f)] private float normalizedStartTime;
        [SerializeField] private bool useUnscaledTime = true;

        private Animator animator;

        private void Reset()
        {
            targetRoot = transform;
        }

        private void OnEnable()
        {
            BindAnimator();
            PlayDefaultState();
        }

        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            BindAnimator();
        }

        private void BindAnimator()
        {
            Transform resolvedRoot = ResolveTargetRoot();
            if (resolvedRoot == null)
            {
                animator = null;
                return;
            }

            animator = resolvedRoot.GetComponentInChildren<Animator>(true);
            if (animator == null)
            {
                return;
            }

            if (avatar != null)
            {
                animator.avatar = avatar;
            }

            if (controller != null && animator.runtimeAnimatorController != controller)
            {
                animator.runtimeAnimatorController = controller;
            }

            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            animator.updateMode = useUnscaledTime ? AnimatorUpdateMode.UnscaledTime : AnimatorUpdateMode.Normal;
        }

        private void PlayDefaultState()
        {
            if (animator == null || animator.runtimeAnimatorController == null || string.IsNullOrWhiteSpace(defaultStateName))
            {
                return;
            }

            animator.Play(defaultStateName, 0, normalizedStartTime);
            animator.Update(0f);
        }

        private Transform ResolveTargetRoot()
        {
            if (targetRoot == null)
            {
                targetRoot = transform;
            }

            return targetRoot;
        }
    }
}
