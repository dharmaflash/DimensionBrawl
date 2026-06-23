using UnityEngine;

namespace DimensionBrawl.Presentation
{
    [CreateAssetMenu(
        fileName = "DB_InoriRiflePoseTuning",
        menuName = "DimensionBrawl/Presentation/Inori Rifle Pose Tuning Profile")]
    public sealed class InoriRiflePoseTuningProfile : ScriptableObject
    {
        [Header("Status")]
        [SerializeField] private bool enabledForGameplay;

        [Header("Pose Anchor")]
        [SerializeField] private Vector3 poseAnchorLocalPosition = new Vector3(0.08f, 1.2f, 0.28f);
        [SerializeField] private Vector3 poseAnchorLocalEulerAngles = new Vector3(0f, 0f, 0f);

        [Header("Grip Targets")]
        [SerializeField] private Vector3 rightGripLocalPosition = new Vector3(0f, 0f, 0f);
        [SerializeField] private Vector3 rightGripLocalEulerAngles = new Vector3(0f, 0f, 0f);
        [SerializeField] private Vector3 leftHandleLocalPosition = new Vector3(-0.261f, 0.067f, 0.061f);
        [SerializeField] private Vector3 leftHandleLocalEulerAngles = new Vector3(133.522f, -296.891f, -119.992004f);

        [Header("IK Weights")]
        [SerializeField, Range(0f, 1f)] private float rightIkPositionWeight;
        [SerializeField, Range(0f, 1f)] private float rightIkRotationWeight;
        [SerializeField, Range(0f, 1f)] private float leftIkPositionWeight = 0.85f;
        [SerializeField, Range(0f, 1f)] private float leftIkRotationWeight;

        public bool EnabledForGameplay => enabledForGameplay;
        public Vector3 PoseAnchorLocalPosition => poseAnchorLocalPosition;
        public Quaternion PoseAnchorLocalRotation => Quaternion.Euler(poseAnchorLocalEulerAngles);
        public Vector3 RightGripLocalPosition => rightGripLocalPosition;
        public Quaternion RightGripLocalRotation => Quaternion.Euler(rightGripLocalEulerAngles);
        public Vector3 LeftHandleLocalPosition => leftHandleLocalPosition;
        public Quaternion LeftHandleLocalRotation => Quaternion.Euler(leftHandleLocalEulerAngles);
        public float RightIkPositionWeight => rightIkPositionWeight;
        public float RightIkRotationWeight => rightIkRotationWeight;
        public float LeftIkPositionWeight => leftIkPositionWeight;
        public float LeftIkRotationWeight => leftIkRotationWeight;
    }
}
