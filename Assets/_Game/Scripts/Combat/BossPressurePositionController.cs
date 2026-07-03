using DimensionBrawl.LevelDesign;
using UnityEngine;

namespace DimensionBrawl.Combat
{
    [DisallowMultipleComponent]
    public sealed class BossPressurePositionController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SummonLaneSpace laneSpace;
        [SerializeField] private BossPressureCostLadder costLadder;
        [SerializeField] private BossPressureActionDirector actionDirector;
        [SerializeField] private Transform movedTransform;

        [Header("Pressure Position")]
        [SerializeField, Range(0f, 1f)] private float restRisk01 = 0.08f;
        [SerializeField, Range(0f, 1f)] private float maxCommitRisk01 = 0.62f;
        [SerializeField, Min(0f)] private float advanceRiskPerSecond = 0.25f;
        [SerializeField, Min(0f)] private float retreatRiskPerSecond = 0.42f;
        [SerializeField] private bool returnToRestWhenActionsDisabled = true;
        [SerializeField] private bool movementEnabled = true;

        [Header("Response Movement")]
        [SerializeField, Min(0f)] private float actionIntentHoldSeconds = 1.35f;
        [SerializeField, Range(0f, 1f)] private float holdBacklineRisk01 = 0.16f;
        [SerializeField, Range(0f, 1f)] private float strafeFireRisk01 = 0.34f;
        [SerializeField, Range(0f, 1f)] private float specialCommitRisk01 = 0.68f;
        [SerializeField, Range(0f, 1f)] private float summonRetreatRisk01 = 0.18f;
        [SerializeField, Range(0f, 1f)] private float punishCommitRisk01 = 0.74f;
        [SerializeField] private bool lateralStrafeEnabled = true;
        [SerializeField, Min(0f)] private float lateralStrafeUnitsPerSecond = 1.25f;
        [SerializeField, Range(0f, 1f)] private float lateralStrafeHalfWidthRatio = 0.34f;

        private float currentTargetRisk01;
        private int lateralStrafeDirection = 1;

        public float CurrentTargetRisk01 => currentTargetRisk01;
        public float CurrentRisk01 => EvaluateCurrentRisk01();
        public bool MovementEnabled => movementEnabled;

        private Transform MovedTransform => movedTransform != null ? movedTransform : transform;

        private void OnValidate()
        {
            restRisk01 = Mathf.Clamp01(restRisk01);
            maxCommitRisk01 = Mathf.Clamp01(maxCommitRisk01);
            if (maxCommitRisk01 < restRisk01)
            {
                maxCommitRisk01 = restRisk01;
            }

            actionIntentHoldSeconds = Mathf.Max(0f, actionIntentHoldSeconds);
            holdBacklineRisk01 = Mathf.Clamp01(holdBacklineRisk01);
            strafeFireRisk01 = Mathf.Clamp01(strafeFireRisk01);
            specialCommitRisk01 = Mathf.Clamp01(specialCommitRisk01);
            summonRetreatRisk01 = Mathf.Clamp01(summonRetreatRisk01);
            punishCommitRisk01 = Mathf.Clamp01(punishCommitRisk01);
            lateralStrafeUnitsPerSecond = Mathf.Max(0f, lateralStrafeUnitsPerSecond);
            lateralStrafeHalfWidthRatio = Mathf.Clamp01(lateralStrafeHalfWidthRatio);
        }

        public void ConfigureReferences(
            SummonLaneSpace newLaneSpace,
            BossPressureCostLadder newCostLadder,
            BossPressureActionDirector newActionDirector = null,
            Transform newMovedTransform = null)
        {
            laneSpace = newLaneSpace;
            costLadder = newCostLadder;
            actionDirector = newActionDirector;
            movedTransform = newMovedTransform;
        }

        public void SetMovementEnabled(bool enabled)
        {
            movementEnabled = enabled;
        }

        public void Tick(float deltaTime)
        {
            if (!movementEnabled || deltaTime <= 0f || laneSpace == null)
            {
                return;
            }

            Transform targetTransform = MovedTransform;
            if (targetTransform == null)
            {
                return;
            }

            currentTargetRisk01 = ResolveTargetRisk01();
            float currentRisk01 = EvaluateCurrentRisk01(targetTransform.position);
            float riskSpeed = currentTargetRisk01 >= currentRisk01
                ? advanceRiskPerSecond
                : retreatRiskPerSecond;
            float nextRisk01 = Mathf.MoveTowards(
                currentRisk01,
                currentTargetRisk01,
                riskSpeed * deltaTime);
            ApplyRiskPosition(targetTransform, nextRisk01, deltaTime);
        }

        private void Update()
        {
            Tick(Time.deltaTime);
        }

        private float ResolveTargetRisk01()
        {
            if (returnToRestWhenActionsDisabled && actionDirector != null && !actionDirector.ActionsEnabled)
            {
                return restRisk01;
            }

            if (TryResolveActionIntentRisk(out float intentRisk01))
            {
                return intentRisk01;
            }

            float pressure01 = 0f;
            if (costLadder != null)
            {
                pressure01 = costLadder.CanSpend ? 1f : costLadder.CurrentTierFillRatio;
            }

            return Mathf.Lerp(restRisk01, maxCommitRisk01, Mathf.Clamp01(pressure01));
        }

        private bool TryResolveActionIntentRisk(out float risk01)
        {
            risk01 = 0f;
            if (actionDirector == null
                || !actionDirector.ActionsEnabled)
            {
                return false;
            }

            if (!actionDirector.HasLastQueuedActionSlot
                || actionDirector.LastActionAgeSeconds > actionIntentHoldSeconds)
            {
                if (actionDirector.LastBasicShotAgeSeconds <= actionIntentHoldSeconds)
                {
                    risk01 = strafeFireRisk01;
                    return true;
                }

                return false;
            }

            BossPressureMovementIntent movementIntent = ResolveMovementIntent(actionDirector.LastMovementIntent);
            switch (movementIntent)
            {
                case BossPressureMovementIntent.HoldBacklineFire:
                    risk01 = holdBacklineRisk01;
                    return true;
                case BossPressureMovementIntent.StrafeFire:
                    risk01 = strafeFireRisk01;
                    return true;
                case BossPressureMovementIntent.CommitForward:
                    risk01 = actionDirector.LastActionKind == BossPressureActionKind.PunishOverextend
                        ? punishCommitRisk01
                        : specialCommitRisk01;
                    return true;
                case BossPressureMovementIntent.RetreatAndSummon:
                    risk01 = summonRetreatRisk01;
                    return true;
                default:
                    return false;
            }
        }

        private BossPressureMovementIntent ResolveMovementIntent(BossPressureMovementIntent configuredIntent)
        {
            if (configuredIntent != BossPressureMovementIntent.CostPressure)
            {
                return configuredIntent;
            }

            switch (actionDirector.LastActionKind)
            {
                case BossPressureActionKind.SummonPressure:
                    return BossPressureMovementIntent.RetreatAndSummon;
                case BossPressureActionKind.PunishOverextend:
                case BossPressureActionKind.SpecialSkill:
                    return BossPressureMovementIntent.CommitForward;
                case BossPressureActionKind.BasicShot:
                case BossPressureActionKind.SkillPattern:
                    return BossPressureMovementIntent.StrafeFire;
                default:
                    return BossPressureMovementIntent.CostPressure;
            }
        }

        private float EvaluateCurrentRisk01()
        {
            Transform targetTransform = MovedTransform;
            return targetTransform != null ? EvaluateCurrentRisk01(targetTransform.position) : 0f;
        }

        private float EvaluateCurrentRisk01(Vector3 worldPosition)
        {
            if (costLadder != null)
            {
                return costLadder.EvaluateBossForwardRisk01(worldPosition);
            }

            if (laneSpace == null)
            {
                return 0f;
            }

            float laneZ = laneSpace.GetLaneCoordinates(worldPosition).y;
            float clampedZ = Mathf.Clamp(laneZ, laneSpace.ForwardBoundaryZ, laneSpace.BossProxyZ);
            return Mathf.Clamp01(Mathf.InverseLerp(laneSpace.BossProxyZ, laneSpace.ForwardBoundaryZ, clampedZ));
        }

        private void ApplyRiskPosition(Transform targetTransform, float risk01, float deltaTime)
        {
            Vector3 currentPosition = targetTransform.position;
            Vector2 laneCoordinates = laneSpace.GetLaneCoordinates(currentPosition);
            float targetLaneZ = Mathf.Lerp(laneSpace.BossProxyZ, laneSpace.ForwardBoundaryZ, Mathf.Clamp01(risk01));
            float targetLateralX = ResolveTargetLateralX(laneCoordinates.x, deltaTime);
            targetTransform.position = laneSpace.GetBattlefieldWorldPoint(
                targetLateralX,
                targetLaneZ,
                currentPosition.y);
        }

        private float ResolveTargetLateralX(float currentLateralX, float deltaTime)
        {
            if (!lateralStrafeEnabled
                || lateralStrafeUnitsPerSecond <= 0f
                || lateralStrafeHalfWidthRatio <= 0f
                || deltaTime <= 0f
                || (actionDirector != null && !actionDirector.ActionsEnabled))
            {
                return currentLateralX;
            }

            float limit = Mathf.Max(0f, laneSpace.HalfWidth) * lateralStrafeHalfWidthRatio;
            if (limit <= 0.001f)
            {
                return currentLateralX;
            }

            float nextLateralX = currentLateralX + (lateralStrafeDirection * lateralStrafeUnitsPerSecond * deltaTime);
            if (nextLateralX > limit)
            {
                nextLateralX = limit;
                lateralStrafeDirection = -1;
            }
            else if (nextLateralX < -limit)
            {
                nextLateralX = -limit;
                lateralStrafeDirection = 1;
            }

            return nextLateralX;
        }
    }
}
