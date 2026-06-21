using UnityEngine;

namespace DimensionBrawl.Presentation
{
    [DisallowMultipleComponent]
    public sealed class RetargetedHandWeaponAttachment : MonoBehaviour
    {
        [SerializeField] private Transform followTarget;
        [SerializeField] private Transform orientationReference;
        [SerializeField] private Vector3 localPosition;
        [SerializeField] private Vector3 referenceLocalForward = Vector3.forward;
        [SerializeField] private Vector3 referenceLocalUp = Vector3.up;

        public bool IsConfigured => followTarget != null && orientationReference != null;

        public void Configure(
            Transform newFollowTarget,
            Transform newOrientationReference,
            Vector3 newLocalPosition,
            Vector3 newReferenceLocalForward,
            Vector3 newReferenceLocalUp)
        {
            followTarget = newFollowTarget;
            orientationReference = newOrientationReference;
            localPosition = newLocalPosition;
            referenceLocalForward = newReferenceLocalForward;
            referenceLocalUp = newReferenceLocalUp;
            ApplyAttachment();
        }

        private void OnEnable()
        {
            ApplyAttachment();
        }

        private void LateUpdate()
        {
            ApplyAttachment();
        }

        private void ApplyAttachment()
        {
            if (followTarget == null)
            {
                return;
            }

            transform.position = followTarget.TransformPoint(localPosition);
            if (orientationReference == null)
            {
                return;
            }

            Vector3 forward = orientationReference.TransformDirection(referenceLocalForward);
            Vector3 up = orientationReference.TransformDirection(referenceLocalUp);
            if (forward.sqrMagnitude < 0.0001f || up.sqrMagnitude < 0.0001f)
            {
                return;
            }

            transform.rotation = Quaternion.LookRotation(forward.normalized, up.normalized);
        }
    }
}
