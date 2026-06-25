using System;
using System.Collections.Generic;
using UnityEngine;

namespace IsekaiBrawl.Gameplay
{
    public enum SummonSpawnFailureReason
    {
        None = 0,
        InvalidData = 1,
        NotInBattle = 2,
        Cooldown = 3,
        NoSpawnPoint = 4,
        NotEnoughEnergy = 5
    }

    public enum SummonPlacementRecommendationTag
    {
        None = 0,
        Break = 1,
        Hold = 2,
        Reward = 3,
        Save = 4
    }

    public readonly struct SummonPlacementResult
    {
        public SummonPlacementResult(
            int laneIndex,
            bool canPlace,
            string failureReason,
            SummonPlacementRecommendationTag recommendationTag,
            SummonSpawnFailureReason failureCode)
        {
            LaneIndex = laneIndex;
            CanPlace = canPlace;
            FailureReason = failureReason ?? string.Empty;
            RecommendationTag = recommendationTag;
            FailureCode = failureCode;
        }

        public int LaneIndex { get; }
        public bool CanPlace { get; }
        public string FailureReason { get; }
        public SummonPlacementRecommendationTag RecommendationTag { get; }
        public SummonSpawnFailureReason FailureCode { get; }
    }

    public class SummonSpawner : MonoBehaviour
    {
        public event System.Action<SummonData, Vector3, bool> OnSummonSpawned;
        public event System.Action<int> OnSelectedLaneChanged;

        [SerializeField] private Transform summonSpawnPoint;
        [SerializeField] private List<SummonData> availableSummons = new();
        [SerializeField] private bool expandPrototypeDeck = true;
        [SerializeField] private float laneOffsetSpacing = 0.9f;
        [SerializeField] private float forwardOffsetSpacing = 0.45f;
        [SerializeField] private float spawnCooldown = 1.15f;
        [SerializeField] private int selectedLaneIndex = BattleLaneUtility.DefaultLaneCount / 2;
        [SerializeField] private bool spawnAroundPlayer = true;
        [SerializeField] private float playerDropHalfWidth = 5.4f;
        [SerializeField] private float playerDropRearRange = 0.7f;
        [SerializeField] private float playerDropForwardRange = 2.8f;
        [SerializeField] private float defaultPlayerDropForwardOffset = 1.25f;
        [SerializeField] private float laneAttractionWeight = 0.12f;

        private readonly List<SummonData> runtimeDeck = new();
        private readonly int[] playerLaneSpawnCounts = new int[BattleLaneUtility.DefaultLaneCount];
        private int totalSummonsSpawned;
        private float nextSpawnAllowedTime;
        private bool hasPendingPlacementWorldPosition;
        private Vector3 pendingPlacementWorldPosition;

        public IReadOnlyList<SummonData> AvailableSummons => runtimeDeck;
        public float RemainingSpawnCooldown => Mathf.Max(0f, nextSpawnAllowedTime - Time.time);
        public int SelectedLaneIndex => selectedLaneIndex;
        public SummonSpawnFailureReason LastSpawnFailureReason { get; private set; }
        public SummonPlacementResult LastPlacementResult { get; private set; }

        private void Awake()
        {
            Array.Clear(playerLaneSpawnCounts, 0, playerLaneSpawnCounts.Length);
            BuildRuntimeDeck();
        }

        private void Start()
        {
            if (summonSpawnPoint == null && BattleManager.Instance != null)
            {
                summonSpawnPoint = BattleManager.Instance.SummonSpawnPoint;
            }
        }

        public bool TrySpawnSummon(SummonData data)
        {
            return TrySpawnSummon(data, selectedLaneIndex, out _, out _);
        }

        public bool TrySpawnSummon(SummonData data, int laneIndex)
        {
            return TrySpawnSummon(data, laneIndex, out _, out _);
        }

        public SummonPlacementResult EvaluatePlacement(SummonData data, int laneIndex)
        {
            int resolvedLaneIndex = BattleLaneUtility.ClampLaneIndex(laneIndex);
            SummonPlacementRecommendationTag recommendationTag = ResolveRecommendationTag(resolvedLaneIndex);

            if (data == null || data.prefab == null || BattleEnergySystem.Instance == null)
            {
                return BuildPlacementResult(resolvedLaneIndex, false, SummonSpawnFailureReason.InvalidData, recommendationTag);
            }

            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Battle)
            {
                return BuildPlacementResult(resolvedLaneIndex, false, SummonSpawnFailureReason.NotInBattle, recommendationTag);
            }

            if (Time.time < nextSpawnAllowedTime)
            {
                return BuildPlacementResult(resolvedLaneIndex, false, SummonSpawnFailureReason.Cooldown, recommendationTag);
            }

            Transform spawnPoint = summonSpawnPoint != null
                ? summonSpawnPoint
                : BattleManager.Instance != null ? BattleManager.Instance.SummonSpawnPoint : null;
            if (spawnPoint == null)
            {
                return BuildPlacementResult(resolvedLaneIndex, false, SummonSpawnFailureReason.NoSpawnPoint, recommendationTag);
            }

            if (BattleEnergySystem.Instance.CurrentEnergy < data.energyCost)
            {
                return BuildPlacementResult(resolvedLaneIndex, false, SummonSpawnFailureReason.NotEnoughEnergy, recommendationTag);
            }

            return new SummonPlacementResult(
                resolvedLaneIndex,
                true,
                string.Empty,
                recommendationTag,
                SummonSpawnFailureReason.None);
        }

        public bool TrySpawnSummon(SummonData data, int laneIndex, out SummonSpawnFailureReason failureReason)
        {
            return TrySpawnSummon(data, laneIndex, out _, out failureReason);
        }

        public bool TrySpawnSummon(SummonData data, int laneIndex, out SummonPlacementResult placementResult)
        {
            return TrySpawnSummon(data, laneIndex, out placementResult, out _);
        }

        public bool TrySpawnSummon(
            SummonData data,
            int laneIndex,
            out SummonPlacementResult placementResult,
            out SummonSpawnFailureReason failureReason)
        {
            placementResult = EvaluatePlacement(data, laneIndex);
            LastPlacementResult = placementResult;
            failureReason = placementResult.FailureCode;
            LastSpawnFailureReason = failureReason;
            if (!placementResult.CanPlace)
            {
                ClearPendingPlacementWorldPosition();
                return false;
            }

            if (data == null || BattleEnergySystem.Instance == null || !BattleEnergySystem.Instance.SpendEnergy(data.energyCost))
            {
                placementResult = BuildPlacementResult(
                    placementResult.LaneIndex,
                    false,
                    SummonSpawnFailureReason.NotEnoughEnergy,
                    placementResult.RecommendationTag);
                LastPlacementResult = placementResult;
                failureReason = placementResult.FailureCode;
                LastSpawnFailureReason = failureReason;
                ClearPendingPlacementWorldPosition();
                return false;
            }

            Transform spawnPoint = summonSpawnPoint != null
                ? summonSpawnPoint
                : BattleManager.Instance != null ? BattleManager.Instance.SummonSpawnPoint : null;
            int resolvedLaneIndex = placementResult.LaneIndex;
            if (selectedLaneIndex != resolvedLaneIndex)
            {
                selectedLaneIndex = resolvedLaneIndex;
                OnSelectedLaneChanged?.Invoke(selectedLaneIndex);
            }
            Vector3 baseSpawnPosition = ResolveSpawnBasePosition(resolvedLaneIndex, spawnPoint != null ? spawnPoint.position : Vector3.zero);
            Vector3 spawnOffset = GetFormationOffset(totalSummonsSpawned, resolvedLaneIndex, isPlayerTeam: true);
            Quaternion spawnRotation = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;
            GameObject summonObject = Instantiate(data.prefab, baseSpawnPosition + spawnOffset, spawnRotation);
            SummonUnit summonUnit = summonObject.GetComponent<SummonUnit>();
            if (summonUnit != null)
            {
                summonUnit.Init(data, true);
                summonUnit.SetAssignedLane(resolvedLaneIndex);
            }

            OnSummonSpawned?.Invoke(data, summonObject.transform.position, true);
            totalSummonsSpawned++;
            nextSpawnAllowedTime = Time.time + spawnCooldown;
            placementResult = new SummonPlacementResult(
                resolvedLaneIndex,
                true,
                string.Empty,
                placementResult.RecommendationTag,
                SummonSpawnFailureReason.None);
            LastPlacementResult = placementResult;
            failureReason = SummonSpawnFailureReason.None;
            LastSpawnFailureReason = failureReason;
            ClearPendingPlacementWorldPosition();
            return true;
        }

        public void SetSelectedLane(int laneIndex)
        {
            int clampedLaneIndex = BattleLaneUtility.ClampLaneIndex(laneIndex);
            if (selectedLaneIndex == clampedLaneIndex)
            {
                return;
            }

            selectedLaneIndex = clampedLaneIndex;
            OnSelectedLaneChanged?.Invoke(selectedLaneIndex);
        }

        public void SetPendingPlacementWorldPosition(Vector3 worldPosition)
        {
            pendingPlacementWorldPosition = worldPosition;
            hasPendingPlacementWorldPosition = true;
        }

        public void ClearPendingPlacementWorldPosition()
        {
            hasPendingPlacementWorldPosition = false;
            pendingPlacementWorldPosition = Vector3.zero;
        }

        private void BuildRuntimeDeck()
        {
            runtimeDeck.Clear();
            if (availableSummons.Count == 0)
            {
                return;
            }

            if (!expandPrototypeDeck && availableSummons.Count > 0)
            {
                runtimeDeck.AddRange(availableSummons);
                return;
            }

            runtimeDeck.AddRange(PrototypeDeckFactory.BuildPrototypeDeck(availableSummons));
        }

        private Vector3 GetFormationOffset(int spawnIndex, int laneIndex, bool isPlayerTeam)
        {
            int[] laneCounts = playerLaneSpawnCounts;
            int laneSpawnIndex = laneCounts[laneIndex];
            laneCounts[laneIndex] = laneSpawnIndex + 1;

            int lateralSlot = laneSpawnIndex % 3;
            int depthSlot = laneSpawnIndex / 3;
            float lateralJitter = (lateralSlot - 1) * laneOffsetSpacing * 0.18f;
            float xOffset = lateralJitter;
            float zOffset = depthSlot * forwardOffsetSpacing;
            if (!isPlayerTeam)
            {
                zOffset *= -1f;
            }

            return new Vector3(xOffset, 0f, zOffset);
        }

        private Vector3 ResolveSpawnBasePosition(int laneIndex, Vector3 fallbackPosition)
        {
            BattleManager battleManager = BattleManager.Instance;
            float laneCenterX = battleManager != null
                ? battleManager.GetLaneCenterX(laneIndex)
                : BattleLaneUtility.GetLaneCenterX(laneIndex, 5.75f);

            Vector3 resolved = fallbackPosition;
            resolved.x = laneCenterX;

            if (!spawnAroundPlayer)
            {
                return resolved;
            }

            PlayerController playerController = battleManager != null ? battleManager.PlayerController : null;
            if (playerController == null)
            {
                return resolved;
            }

            Vector3 playerPosition = playerController.transform.position;
            Vector3 baseDropPosition = hasPendingPlacementWorldPosition
                ? pendingPlacementWorldPosition
                : playerPosition + new Vector3(0f, 0f, defaultPlayerDropForwardOffset);

            baseDropPosition.y = fallbackPosition.y;
            baseDropPosition = ClampLocalDropPosition(baseDropPosition, playerPosition, laneIndex, battleManager, playerController);
            return baseDropPosition;
        }

        private Vector3 ClampLocalDropPosition(
            Vector3 worldPosition,
            Vector3 playerPosition,
            int laneIndex,
            BattleManager battleManager,
            PlayerController playerController)
        {
            Vector3 clamped = worldPosition;
            clamped.x = Mathf.Clamp(clamped.x, playerPosition.x - playerDropHalfWidth, playerPosition.x + playerDropHalfWidth);
            clamped.z = Mathf.Clamp(clamped.z, playerPosition.z - playerDropRearRange, playerPosition.z + playerDropForwardRange);
            clamped = playerController.ClampToMovementBounds(clamped);

            float laneCenterX = battleManager != null
                ? battleManager.GetLaneCenterX(laneIndex)
                : BattleLaneUtility.GetLaneCenterX(laneIndex, 5.75f);
            clamped.x = Mathf.Lerp(clamped.x, laneCenterX, Mathf.Clamp01(laneAttractionWeight));
            clamped.y = playerPosition.y;
            return clamped;
        }

        private SummonPlacementRecommendationTag ResolveRecommendationTag(int laneIndex)
        {
            BattleManager battleManager = BattleManager.Instance;
            if (battleManager == null || !battleManager.TryGetSummonLanePreview(laneIndex, out BattleManager.SummonLanePreview preview))
            {
                return SummonPlacementRecommendationTag.Save;
            }

            return preview.PreviewState switch
            {
                BattleManager.SummonLanePreviewState.Reward => SummonPlacementRecommendationTag.Reward,
                BattleManager.SummonLanePreviewState.Break => SummonPlacementRecommendationTag.Break,
                BattleManager.SummonLanePreviewState.Stall => SummonPlacementRecommendationTag.Hold,
                _ => SummonPlacementRecommendationTag.None
            };
        }

        private static SummonPlacementResult BuildPlacementResult(
            int laneIndex,
            bool canPlace,
            SummonSpawnFailureReason failureReason,
            SummonPlacementRecommendationTag recommendationTag)
        {
            return new SummonPlacementResult(
                laneIndex,
                canPlace,
                ResolveFailureReasonText(failureReason),
                recommendationTag,
                failureReason);
        }

        private static string ResolveFailureReasonText(SummonSpawnFailureReason failureReason)
        {
            return failureReason switch
            {
                SummonSpawnFailureReason.NotEnoughEnergy => "에너지 부족",
                SummonSpawnFailureReason.Cooldown => "재사용 대기 중",
                SummonSpawnFailureReason.NoSpawnPoint => "배치 불가",
                SummonSpawnFailureReason.InvalidData => "배치 불가",
                SummonSpawnFailureReason.NotInBattle => "배치 불가",
                _ => string.Empty
            };
        }
    }
}
