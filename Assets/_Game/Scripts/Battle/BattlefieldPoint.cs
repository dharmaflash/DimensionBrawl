using System.Collections.Generic;
using UnityEngine;

namespace IsekaiBrawl.Gameplay
{
    public enum BattlefieldPointType
    {
        ReadyPocket = 0,
        JoinPocket = 1,
        BlockerHoldPocket = 2,
        BreachPocket = 3,
        ObjectivePocket = 4,
        CoreSiegePocket = 5,
        ApproachPocket = 6,
        FallbackPocket = 7,
        ObjectiveAnchor = 8,
        AdvancePocket = 9,
        SupportLeftPocket = 10,
        SupportCenterPocket = 11,
        SupportRightPocket = 12,
        PeekPocket = 13
    }

    public enum BattlefieldPointUnlockRule
    {
        Always = 0,
        RequiresRecentSummon = 1,
        RequiresAlliedPresence = 2,
        RequiresSiegeClear = 3
    }

    public class BattlefieldPoint : MonoBehaviour
    {
        private static readonly List<BattlefieldPoint> ActivePoints = new();

        [SerializeField] private int laneIndex = BattleLaneUtility.DefaultLaneCount / 2;
        [SerializeField] private BattlefieldPointType pointType = BattlefieldPointType.ApproachPocket;
        [SerializeField] private BattlefieldPointUnlockRule unlockRule = BattlefieldPointUnlockRule.Always;
        [SerializeField] private float priorityWeight = 1f;

        public int LaneIndex => laneIndex;
        public BattlefieldPointType PointType => pointType;
        public BattlefieldPointUnlockRule UnlockRule => unlockRule;
        public float PriorityWeight => priorityWeight;

        private void OnEnable()
        {
            if (!ActivePoints.Contains(this))
            {
                ActivePoints.Add(this);
            }
        }

        private void OnDisable()
        {
            ActivePoints.Remove(this);
        }

        public void Configure(
            int newLaneIndex,
            BattlefieldPointType newPointType,
            BattlefieldPointUnlockRule newUnlockRule,
            float newPriorityWeight)
        {
            laneIndex = BattleLaneUtility.ClampLaneIndex(newLaneIndex);
            pointType = newPointType;
            unlockRule = newUnlockRule;
            priorityWeight = newPriorityWeight;
        }

        public static BattlefieldPoint FindClosestInLane(
            int laneIndex,
            BattlefieldPointType pointType,
            bool isPlayerTeam,
            float referenceZ = 0f)
        {
            int resolvedLaneIndex = BattleLaneUtility.ClampLaneIndex(laneIndex);
            BattlefieldPoint bestPoint = null;
            float bestDepth = isPlayerTeam ? float.MaxValue : float.MinValue;
            float bestPriority = float.MinValue;

            for (int index = 0; index < ActivePoints.Count; index++)
            {
                BattlefieldPoint point = ActivePoints[index];
                if (point == null || point.pointType != pointType || point.laneIndex != resolvedLaneIndex)
                {
                    continue;
                }

                float pointZ = point.transform.position.z;
                if (isPlayerTeam)
                {
                    if (pointZ + 0.05f < referenceZ)
                    {
                        continue;
                    }

                    if (pointZ < bestDepth - 0.01f ||
                        (Mathf.Abs(pointZ - bestDepth) <= 0.01f && point.priorityWeight > bestPriority))
                    {
                        bestDepth = pointZ;
                        bestPriority = point.priorityWeight;
                        bestPoint = point;
                    }
                }
                else
                {
                    if (pointZ - 0.05f > referenceZ)
                    {
                        continue;
                    }

                    if (pointZ > bestDepth + 0.01f ||
                        (Mathf.Abs(pointZ - bestDepth) <= 0.01f && point.priorityWeight > bestPriority))
                    {
                        bestDepth = pointZ;
                        bestPriority = point.priorityWeight;
                        bestPoint = point;
                    }
                }
            }

            return bestPoint;
        }

        public static BattlefieldPoint FindHighestPriorityInLane(int laneIndex, BattlefieldPointType pointType)
        {
            int resolvedLaneIndex = BattleLaneUtility.ClampLaneIndex(laneIndex);
            BattlefieldPoint bestPoint = null;
            float bestPriority = float.MinValue;

            for (int index = 0; index < ActivePoints.Count; index++)
            {
                BattlefieldPoint point = ActivePoints[index];
                if (point == null || point.pointType != pointType || point.laneIndex != resolvedLaneIndex)
                {
                    continue;
                }

                if (point.priorityWeight <= bestPriority)
                {
                    continue;
                }

                bestPriority = point.priorityWeight;
                bestPoint = point;
            }

            return bestPoint;
        }

        public static BattlefieldPoint[] FindAllInLane(int laneIndex, BattlefieldPointType pointType)
        {
            int resolvedLaneIndex = BattleLaneUtility.ClampLaneIndex(laneIndex);
            List<BattlefieldPoint> results = new();
            for (int index = 0; index < ActivePoints.Count; index++)
            {
                BattlefieldPoint point = ActivePoints[index];
                if (point != null && point.pointType == pointType && point.laneIndex == resolvedLaneIndex)
                {
                    results.Add(point);
                }
            }

            return results.ToArray();
        }
    }
}
