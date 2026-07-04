using DimensionBrawl.LevelDesign;
using DimensionBrawl.Player;
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
        [SerializeField] private Transform trackedPlayer;

        [Header("Pressure Position")]
        [SerializeField, Range(0f, 1f)] private float restRisk01 = 0.18f;
        [SerializeField, Range(0f, 1f)] private float maxCommitRisk01 = 0.9f;
        [SerializeField, Min(0f)] private float advanceRiskPerSecond = 0.46f;
        [SerializeField, Min(0f)] private float retreatRiskPerSecond = 0.38f;
        [SerializeField] private bool returnToRestWhenActionsDisabled = true;
        [SerializeField] private bool movementEnabled = true;

        [Header("Response Movement")]
        [SerializeField, Min(0f)] private float actionIntentHoldSeconds = 1.65f;
        [SerializeField, Range(0f, 1f)] private float holdBacklineRisk01 = 0.22f;
        [SerializeField, Range(0f, 1f)] private float strafeFireRisk01 = 0.52f;
        [SerializeField, Range(0f, 1f)] private float specialCommitRisk01 = 0.82f;
        [SerializeField, Range(0f, 1f)] private float summonRetreatRisk01 = 0.1f;
        [SerializeField, Range(0f, 1f)] private float punishCommitRisk01 = 0.9f;
        [SerializeField] private bool lateralStrafeEnabled = true;
        [SerializeField, Min(0f)] private float lateralStrafeUnitsPerSecond = 0.9f;
        [SerializeField, Range(0f, 1f)] private float lateralStrafeHalfWidthRatio = 0.34f;

        [Header("Player Response")]
        [SerializeField] private bool playerResponseEnabled = true;
        [SerializeField, Range(0f, 1f)] private float playerLateralFollowStrength = 0.82f;
        [SerializeField, Range(0f, 1f)] private float playerResponseHalfWidthRatio = 0.52f;
        [SerializeField, Min(0f)] private float playerResponseLateralUnitsPerSecond = 2.6f;
        [SerializeField, Range(0f, 1f)] private float playerFlankOffsetRatio = 0.18f;
        [SerializeField, Min(0.05f)] private float playerFlankSwitchSeconds = 0.9f;
        [SerializeField, Range(0f, 1f)] private float commitPlayerFollowBoost = 0.24f;
        [SerializeField] private bool faceTrackedPlayer = true;
        [SerializeField, Min(0f)] private float turnDegreesPerSecond = 780f;

        [Header("Forward Pressure Motion")]
        [SerializeField] private bool forwardPressureOscillationEnabled = true;
        [SerializeField, Range(0f, 1f)] private float idleForwardRiskAmplitude = 0.025f;
        [SerializeField, Range(0f, 1f)] private float actionForwardRiskAmplitude = 0.05f;
        [SerializeField, Min(0.05f)] private float forwardOscillationSeconds = 2.35f;
        [SerializeField, Range(0f, 1f)] private float commitRiskBoost = 0.04f;
        [SerializeField, Range(0f, 1f)] private float retreatRiskDip = 0.035f;

        [Header("Movement Animation")]
        [SerializeField] private Animator movementAnimator;
        [SerializeField] private string movementSpeedParameter = "MoveSpeed";
        [SerializeField] private string alternateMovementSpeedParameter = "Speed";
        [SerializeField] private string basicFireTrigger = "Attack";
        [SerializeField] private string retreatStepTrigger = "RetreatBackstep";
        [SerializeField, Min(0f)] private float animatorMoveSpeedScale = 0.28f;
        [SerializeField, Range(0f, 0.5f)] private float animatorDampSeconds = 0.1f;
        [SerializeField, Min(0f)] private float basicFireMovementLockSeconds = 0.34f;
        [SerializeField, Min(0f)] private float retreatAnimationRiskDelta = 0.025f;
        [SerializeField, Min(0f)] private float retreatTriggerCooldownSeconds = 1.05f;

        private float currentTargetRisk01;
        private int lateralStrafeDirection = 1;
        private int playerFlankDirection = 1;
        private float playerFlankTimer;
        private float forwardOscillationTimer;
        private float retreatTriggerCooldown;
        private float basicFireMovementLockTimer;
        private int observedBasicFireVolleys = -1;
        private bool triedAutoResolvePlayer;

        public float CurrentTargetRisk01 => currentTargetRisk01;
        public float CurrentRisk01 => EvaluateCurrentRisk01();
        public bool MovementEnabled => movementEnabled;
        public Transform TrackedPlayer => trackedPlayer;
        public bool PlayerResponseEnabled => playerResponseEnabled;

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
            playerLateralFollowStrength = Mathf.Clamp01(playerLateralFollowStrength);
            playerResponseHalfWidthRatio = Mathf.Clamp01(playerResponseHalfWidthRatio);
            playerResponseLateralUnitsPerSecond = Mathf.Max(0f, playerResponseLateralUnitsPerSecond);
            playerFlankOffsetRatio = Mathf.Clamp01(playerFlankOffsetRatio);
            playerFlankSwitchSeconds = Mathf.Max(0.05f, playerFlankSwitchSeconds);
            commitPlayerFollowBoost = Mathf.Clamp01(commitPlayerFollowBoost);
            turnDegreesPerSecond = Mathf.Max(0f, turnDegreesPerSecond);
            idleForwardRiskAmplitude = Mathf.Clamp01(idleForwardRiskAmplitude);
            actionForwardRiskAmplitude = Mathf.Clamp01(actionForwardRiskAmplitude);
            forwardOscillationSeconds = Mathf.Max(0.05f, forwardOscillationSeconds);
            commitRiskBoost = Mathf.Clamp01(commitRiskBoost);
            retreatRiskDip = Mathf.Clamp01(retreatRiskDip);
            animatorMoveSpeedScale = Mathf.Max(0f, animatorMoveSpeedScale);
            animatorDampSeconds = Mathf.Clamp(animatorDampSeconds, 0f, 0.5f);
            basicFireMovementLockSeconds = Mathf.Max(0f, basicFireMovementLockSeconds);
            retreatAnimationRiskDelta = Mathf.Max(0f, retreatAnimationRiskDelta);
            retreatTriggerCooldownSeconds = Mathf.Max(0f, retreatTriggerCooldownSeconds);
        }

        public void ConfigureReferences(
            SummonLaneSpace newLaneSpace,
            BossPressureCostLadder newCostLadder,
            BossPressureActionDirector newActionDirector = null,
            Transform newMovedTransform = null,
            Transform newTrackedPlayer = null)
        {
            laneSpace = newLaneSpace;
            costLadder = newCostLadder;
            actionDirector = newActionDirector;
            movedTransform = newMovedTransform;
            trackedPlayer = newTrackedPlayer;
            triedAutoResolvePlayer = trackedPlayer != null;
        }

        public void SetMovementEnabled(bool enabled)
        {
            movementEnabled = enabled;
        }

        public void Tick(float deltaTime)
        {
            if (!movementEnabled || deltaTime <= 0f)
            {
                return;
            }

            ResolveMissingReferences();
            if (laneSpace == null)
            {
                return;
            }

            Transform targetTransform = MovedTransform;
            if (targetTransform == null)
            {
                return;
            }

            TickBasicFireMovementLock(deltaTime);
            if (basicFireMovementLockTimer > 0f)
            {
                currentTargetRisk01 = EvaluateCurrentRisk01(targetTransform.position);
                ApplyMovementAnimation(targetTransform, targetTransform.position, targetTransform.position, 0f, deltaTime);
                ApplyFacing(targetTransform, deltaTime);
                return;
            }

            BossPressureMovementIntent movementIntent = ResolveCurrentMovementIntent();
            currentTargetRisk01 = ResolveTargetRisk01(movementIntent, deltaTime);
            float currentRisk01 = EvaluateCurrentRisk01(targetTransform.position);
            float riskSpeed = currentTargetRisk01 >= currentRisk01
                ? advanceRiskPerSecond
                : retreatRiskPerSecond;
            float nextRisk01 = Mathf.MoveTowards(
                currentRisk01,
                currentTargetRisk01,
                riskSpeed * deltaTime);
            Vector3 previousPosition = targetTransform.position;
            ApplyRiskPosition(targetTransform, nextRisk01, deltaTime, movementIntent);
            ApplyMovementAnimation(targetTransform, previousPosition, targetTransform.position, nextRisk01 - currentRisk01, deltaTime);
            ApplyFacing(targetTransform, deltaTime);
        }

        private void Update()
        {
            Tick(Time.deltaTime * CombatTimeDilationReceiver.ResolveTimeScale(this));
        }

        private float ResolveTargetRisk01(BossPressureMovementIntent movementIntent, float deltaTime)
        {
            if (returnToRestWhenActionsDisabled && actionDirector != null && !actionDirector.ActionsEnabled)
            {
                return restRisk01;
            }

            float targetRisk01;
            if (TryResolveActionIntentRisk(movementIntent, out float intentRisk01))
            {
                targetRisk01 = intentRisk01;
            }
            else
            {
                float pressure01 = 0f;
                if (costLadder != null)
                {
                    pressure01 = costLadder.CanSpend ? 1f : costLadder.CurrentTierFillRatio;
                }

                targetRisk01 = Mathf.Lerp(restRisk01, maxCommitRisk01, Mathf.Clamp01(pressure01));
            }

            return ApplyForwardPressureMotion(targetRisk01, movementIntent, deltaTime);
        }

        private bool TryResolveActionIntentRisk(BossPressureMovementIntent movementIntent, out float risk01)
        {
            risk01 = 0f;
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

        private float ApplyForwardPressureMotion(
            float baseRisk01,
            BossPressureMovementIntent movementIntent,
            float deltaTime)
        {
            if (!forwardPressureOscillationEnabled || deltaTime <= 0f)
            {
                return Mathf.Clamp01(baseRisk01);
            }

            forwardOscillationTimer = Mathf.Repeat(
                forwardOscillationTimer + deltaTime / Mathf.Max(0.05f, forwardOscillationSeconds),
                1f);
            float wave = Mathf.Sin(forwardOscillationTimer * Mathf.PI * 2f);
            float amplitude = movementIntent == BossPressureMovementIntent.CostPressure
                || movementIntent == BossPressureMovementIntent.HoldBacklineFire
                    ? idleForwardRiskAmplitude
                    : actionForwardRiskAmplitude;
            float bias = movementIntent switch
            {
                BossPressureMovementIntent.CommitForward => commitRiskBoost,
                BossPressureMovementIntent.StrafeFire => commitRiskBoost * 0.35f,
                BossPressureMovementIntent.RetreatAndSummon => -retreatRiskDip,
                _ => 0f
            };

            return Mathf.Clamp(baseRisk01 + wave * amplitude + bias, 0f, maxCommitRisk01);
        }

        private BossPressureMovementIntent ResolveCurrentMovementIntent()
        {
            if (actionDirector == null || !actionDirector.ActionsEnabled)
            {
                return BossPressureMovementIntent.CostPressure;
            }

            if (!actionDirector.HasLastQueuedActionSlot
                || actionDirector.LastActionAgeSeconds > actionIntentHoldSeconds)
            {
                return actionDirector.LastBasicShotAgeSeconds <= actionIntentHoldSeconds
                    ? BossPressureMovementIntent.StrafeFire
                    : BossPressureMovementIntent.CostPressure;
            }

            return ResolveMovementIntent(actionDirector.LastMovementIntent);
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

        private void ApplyRiskPosition(
            Transform targetTransform,
            float risk01,
            float deltaTime,
            BossPressureMovementIntent movementIntent)
        {
            Vector3 currentPosition = targetTransform.position;
            Vector2 laneCoordinates = laneSpace.GetLaneCoordinates(currentPosition);
            float targetLaneZ = Mathf.Lerp(laneSpace.BossProxyZ, laneSpace.ForwardBoundaryZ, Mathf.Clamp01(risk01));
            float targetLateralX = ResolveTargetLateralX(
                laneCoordinates.x,
                deltaTime,
                movementIntent);
            targetTransform.position = laneSpace.GetBattlefieldWorldPoint(
                targetLateralX,
                targetLaneZ,
                currentPosition.y);
        }

        private void ApplyMovementAnimation(
            Transform targetTransform,
            Vector3 previousPosition,
            Vector3 nextPosition,
            float riskDelta,
            float deltaTime)
        {
            if (deltaTime <= 0f)
            {
                return;
            }

            if (movementAnimator == null && targetTransform != null)
            {
                movementAnimator = targetTransform.GetComponentInChildren<Animator>(includeInactive: true);
            }

            if (movementAnimator == null)
            {
                return;
            }

            retreatTriggerCooldown = Mathf.Max(0f, retreatTriggerCooldown - deltaTime);

            Vector3 planarDelta = nextPosition - previousPosition;
            planarDelta.y = 0f;
            float normalizedSpeed = Mathf.Clamp01(
                planarDelta.magnitude / Mathf.Max(0.001f, deltaTime) * animatorMoveSpeedScale);
            bool wroteMoveSpeed = TrySetAnimatorFloat(movementSpeedParameter, normalizedSpeed, deltaTime);
            if (!wroteMoveSpeed)
            {
                TrySetAnimatorFloat(alternateMovementSpeedParameter, normalizedSpeed, deltaTime);
            }

            if (riskDelta <= -retreatAnimationRiskDelta && retreatTriggerCooldown <= 0f)
            {
                TrySetAnimatorTrigger(retreatStepTrigger);
                retreatTriggerCooldown = retreatTriggerCooldownSeconds;
            }
        }

        private void TickBasicFireMovementLock(float deltaTime)
        {
            basicFireMovementLockTimer = Mathf.Max(0f, basicFireMovementLockTimer - deltaTime);
            if (actionDirector == null)
            {
                return;
            }

            int totalVolleys = actionDirector.TotalBasicShotVolleys;
            if (observedBasicFireVolleys < 0)
            {
                observedBasicFireVolleys = totalVolleys;
                return;
            }

            if (totalVolleys <= observedBasicFireVolleys)
            {
                return;
            }

            observedBasicFireVolleys = totalVolleys;
            if (actionDirector.LastBasicShotProjectileCount <= 0)
            {
                return;
            }

            basicFireMovementLockTimer = Mathf.Max(basicFireMovementLockTimer, basicFireMovementLockSeconds);
            TrySetAnimatorTrigger(basicFireTrigger);
        }

        private float ResolveTargetLateralX(
            float currentLateralX,
            float deltaTime,
            BossPressureMovementIntent movementIntent)
        {
            if (!lateralStrafeEnabled
                || lateralStrafeHalfWidthRatio <= 0f
                || deltaTime <= 0f
                || (actionDirector != null && !actionDirector.ActionsEnabled))
            {
                return currentLateralX;
            }

            if (playerResponseEnabled && trackedPlayer != null)
            {
                return ResolvePlayerResponsiveLateralX(currentLateralX, deltaTime, movementIntent);
            }

            if (lateralStrafeUnitsPerSecond <= 0f)
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

        private float ResolvePlayerResponsiveLateralX(
            float currentLateralX,
            float deltaTime,
            BossPressureMovementIntent movementIntent)
        {
            TickPlayerFlank(deltaTime);

            float laneHalfWidth = Mathf.Max(0f, laneSpace.HalfWidth);
            float limit = laneHalfWidth * Mathf.Max(playerResponseHalfWidthRatio, lateralStrafeHalfWidthRatio);
            if (limit <= 0.001f)
            {
                return currentLateralX;
            }

            float playerLateralX = laneSpace.GetLaneCoordinates(trackedPlayer.position).x;
            float flankOffset = laneHalfWidth * playerFlankOffsetRatio;
            if (Mathf.Abs(playerLateralX) > laneHalfWidth * 0.55f)
            {
                playerFlankDirection = playerLateralX >= 0f ? -1 : 1;
            }

            float followStrength = playerLateralFollowStrength;
            float resolvedFlankOffset = flankOffset;
            switch (movementIntent)
            {
                case BossPressureMovementIntent.CommitForward:
                    followStrength = Mathf.Clamp01(followStrength + commitPlayerFollowBoost);
                    resolvedFlankOffset *= 0.35f;
                    break;
                case BossPressureMovementIntent.RetreatAndSummon:
                    followStrength *= -0.58f;
                    resolvedFlankOffset *= 0.45f;
                    break;
                case BossPressureMovementIntent.HoldBacklineFire:
                    resolvedFlankOffset *= 0.55f;
                    break;
            }

            float desiredLateralX = Mathf.Clamp(
                playerLateralX * followStrength + playerFlankDirection * resolvedFlankOffset,
                -limit,
                limit);
            float responseSpeed = Mathf.Max(lateralStrafeUnitsPerSecond, playerResponseLateralUnitsPerSecond);
            if (movementIntent == BossPressureMovementIntent.CommitForward
                || movementIntent == BossPressureMovementIntent.RetreatAndSummon)
            {
                responseSpeed *= 1.2f;
            }

            return Mathf.MoveTowards(currentLateralX, desiredLateralX, responseSpeed * deltaTime);
        }

        private void TickPlayerFlank(float deltaTime)
        {
            playerFlankTimer -= deltaTime;
            if (playerFlankTimer > 0f)
            {
                return;
            }

            playerFlankDirection = playerFlankDirection >= 0 ? -1 : 1;
            playerFlankTimer = playerFlankSwitchSeconds;
        }

        private void ApplyFacing(Transform targetTransform, float deltaTime)
        {
            if (!faceTrackedPlayer || trackedPlayer == null)
            {
                return;
            }

            Vector3 toPlayer = trackedPlayer.position - targetTransform.position;
            toPlayer.y = 0f;
            if (toPlayer.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(toPlayer.normalized, Vector3.up);
            targetTransform.rotation = turnDegreesPerSecond <= 0f
                ? targetRotation
                : Quaternion.RotateTowards(
                    targetTransform.rotation,
                    targetRotation,
                    turnDegreesPerSecond * deltaTime);
        }

        private void ResolveMissingReferences()
        {
            if (laneSpace == null)
            {
                laneSpace = GetComponentInParent<SummonLaneSpace>();
            }

            if (costLadder == null)
            {
                costLadder = GetComponent<BossPressureCostLadder>();
            }

            if (actionDirector == null)
            {
                actionDirector = GetComponent<BossPressureActionDirector>();
            }

            if (movementAnimator == null)
            {
                movementAnimator = MovedTransform != null
                    ? MovedTransform.GetComponentInChildren<Animator>(includeInactive: true)
                    : GetComponentInChildren<Animator>(includeInactive: true);
            }

            if (trackedPlayer != null || triedAutoResolvePlayer)
            {
                return;
            }

            triedAutoResolvePlayer = true;
            PlayerMovementController player = FindFirstObjectByType<PlayerMovementController>();
            if (player != null)
            {
                trackedPlayer = player.transform;
            }
        }

        private bool TrySetAnimatorFloat(string parameterName, float value, float deltaTime)
        {
            if (movementAnimator == null || string.IsNullOrWhiteSpace(parameterName))
            {
                return false;
            }

            int parameterHash = Animator.StringToHash(parameterName);
            if (!HasAnimatorParameter(parameterHash, AnimatorControllerParameterType.Float))
            {
                return false;
            }

            movementAnimator.SetFloat(parameterHash, value, animatorDampSeconds, deltaTime);
            return true;
        }

        private bool TrySetAnimatorTrigger(string parameterName)
        {
            if (movementAnimator == null || string.IsNullOrWhiteSpace(parameterName))
            {
                return false;
            }

            int parameterHash = Animator.StringToHash(parameterName);
            if (!HasAnimatorParameter(parameterHash, AnimatorControllerParameterType.Trigger))
            {
                return false;
            }

            movementAnimator.SetTrigger(parameterHash);
            return true;
        }

        private bool HasAnimatorParameter(int parameterHash, AnimatorControllerParameterType parameterType)
        {
            if (movementAnimator == null || movementAnimator.runtimeAnimatorController == null)
            {
                return false;
            }

            AnimatorControllerParameter[] parameters = movementAnimator.parameters;
            for (int i = 0; i < parameters.Length; i++)
            {
                AnimatorControllerParameter parameter = parameters[i];
                if (parameter.nameHash == parameterHash && parameter.type == parameterType)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
