using UnityEngine;

namespace IsekaiBrawl.Gameplay
{
    public static class BattleLaneUtility
    {
        public const int DefaultLaneCount = 5;
        private const float OuterLaneRatio = 0.72f;
        private const float InnerLaneRatio = 0.34f;

        public static int ClampLaneIndex(int laneIndex, int laneCount = DefaultLaneCount)
        {
            return Mathf.Clamp(laneIndex, 0, Mathf.Max(0, laneCount - 1));
        }

        public static float[] BuildLaneAnchors(float laneHalfWidth)
        {
            laneHalfWidth = Mathf.Max(0.1f, laneHalfWidth);
            float outerLane = laneHalfWidth * OuterLaneRatio;
            float innerLane = laneHalfWidth * InnerLaneRatio;
            return new[]
            {
                -outerLane,
                -innerLane,
                0f,
                innerLane,
                outerLane
            };
        }

        public static float GetLaneCenterX(int laneIndex, float laneHalfWidth)
        {
            float[] lanes = BuildLaneAnchors(laneHalfWidth);
            return lanes[ClampLaneIndex(laneIndex, lanes.Length)];
        }

        public static int GetNearestLaneIndex(float currentX, float[] laneAnchors)
        {
            if (laneAnchors == null || laneAnchors.Length == 0)
            {
                return DefaultLaneCount / 2;
            }

            int bestIndex = 0;
            float bestDistance = float.MaxValue;
            for (int index = 0; index < laneAnchors.Length; index++)
            {
                float distance = Mathf.Abs(currentX - laneAnchors[index]);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestIndex = index;
                }
            }

            return bestIndex;
        }
    }
}
