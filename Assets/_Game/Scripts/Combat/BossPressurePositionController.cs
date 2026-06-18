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

        private float currentTargetRisk01;

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
            ApplyRiskPosition(targetTransform, nextRisk01);
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

            float pressure01 = 0f;
            if (costLadder != null)
            {
                pressure01 = costLadder.CanSpend ? 1f : costLadder.CurrentTierFillRatio;
            }

            return Mathf.Lerp(restRisk01, maxCommitRisk01, Mathf.Clamp01(pressure01));
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

        private void ApplyRiskPosition(Transform targetTransform, float risk01)
        {
            Vector3 currentPosition = targetTransform.position;
            Vector2 laneCoordinates = laneSpace.GetLaneCoordinates(currentPosition);
            float targetLaneZ = Mathf.Lerp(laneSpace.BossProxyZ, laneSpace.ForwardBoundaryZ, Mathf.Clamp01(risk01));
            targetTransform.position = laneSpace.GetBattlefieldWorldPoint(
                laneCoordinates.x,
                targetLaneZ,
                currentPosition.y);
        }
    }
}
