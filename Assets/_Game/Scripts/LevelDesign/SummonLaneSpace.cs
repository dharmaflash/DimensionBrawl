using UnityEngine;

namespace DimensionBrawl.LevelDesign
{
    public sealed class SummonLaneSpace : MonoBehaviour
    {
        [Header("Player Zone")]
        [SerializeField, Min(0.1f)] private float halfWidth = 5f;
        [SerializeField] private float backLimitZ = -12f;
        [SerializeField] private float forwardBoundaryZ = 0f;

        [Header("Battlefield Anchors")]
        [SerializeField] private float bossProxyZ = 18f;
        [SerializeField] private float summonEntryZ = 1.5f;

        public float HalfWidth => halfWidth;
        public float BackLimitZ => Mathf.Min(backLimitZ, forwardBoundaryZ);
        public float ForwardBoundaryZ => Mathf.Max(backLimitZ, forwardBoundaryZ);
        public float BossProxyZ => Mathf.Max(ForwardBoundaryZ, bossProxyZ);
        public float SummonEntryZ => Mathf.Max(ForwardBoundaryZ, summonEntryZ);

        public Vector2 GetLaneCoordinates(Vector3 worldPosition)
        {
            Vector3 localPosition = transform.InverseTransformPoint(worldPosition);
            return new Vector2(localPosition.x, localPosition.z);
        }

        public Vector3 ClampPlayerPosition(Vector3 worldPosition)
        {
            Vector3 localPosition = transform.InverseTransformPoint(worldPosition);
            localPosition.x = Mathf.Clamp(localPosition.x, -halfWidth, halfWidth);
            localPosition.z = Mathf.Clamp(localPosition.z, BackLimitZ, ForwardBoundaryZ);
            return transform.TransformPoint(localPosition);
        }

        public float EvaluateForwardRisk01(Vector3 worldPosition)
        {
            Vector3 localPosition = transform.InverseTransformPoint(worldPosition);
            return Mathf.InverseLerp(BackLimitZ, ForwardBoundaryZ, Mathf.Clamp(localPosition.z, BackLimitZ, ForwardBoundaryZ));
        }

        public bool IsPastForwardBoundary(Vector3 worldPosition, float tolerance = 0.01f)
        {
            Vector3 localPosition = transform.InverseTransformPoint(worldPosition);
            return localPosition.z > ForwardBoundaryZ + Mathf.Max(0f, tolerance);
        }

        public Vector3 GetLaneWorldPoint(float lateralX, float laneZ, float worldY = 0f)
        {
            Vector3 localPoint = new Vector3(
                Mathf.Clamp(lateralX, -halfWidth, halfWidth),
                0f,
                laneZ);
            Vector3 worldPoint = transform.TransformPoint(localPoint);
            worldPoint.y = worldY;
            return worldPoint;
        }

        public Vector3 GetBattlefieldWorldPoint(float lateralX, float laneZ, float worldY = 0f)
        {
            Vector3 localPoint = new Vector3(lateralX, 0f, laneZ);
            Vector3 worldPoint = transform.TransformPoint(localPoint);
            worldPoint.y = worldY;
            return worldPoint;
        }
    }
}
