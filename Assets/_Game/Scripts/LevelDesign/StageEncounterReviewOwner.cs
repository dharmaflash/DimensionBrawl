using System;
using DimensionBrawl.Combat;
using UnityEngine;

namespace DimensionBrawl.LevelDesign
{
    public sealed class StageEncounterReviewOwner : MonoBehaviour
    {
        [Header("Route Data")]
        [SerializeField] private LinearStageTemplateProfile stageTemplate;
        [SerializeField] private Transform player;

        [Header("Authored Pocket Bindings")]
        [SerializeField] private StageEncounterPocketBinding[] pockets = Array.Empty<StageEncounterPocketBinding>();

        private bool[] completedPockets = Array.Empty<bool>();
        private int currentPocketIndex = -1;
        private bool routeComplete;

        public LinearStageTemplateProfile StageTemplate => stageTemplate;
        public Transform Player => player;
        public int PocketCount => pockets != null ? pockets.Length : 0;
        public int CurrentPocketIndex => currentPocketIndex;
        public int CompletedPocketCount => CountCompletedPockets();
        public bool IsRouteComplete => routeComplete;
        public int RemainingEnemyCount => currentPocketIndex >= 0 ? pockets[currentPocketIndex].CountAliveEnemies() : 0;
        public LinearStageObjectiveKind CurrentObjectiveKind => TryGetCurrentPocket(out StageEncounterPocketBinding pocket)
            && pocket.TryResolvePocket(stageTemplate, out _, out LinearStagePocket stagePocket)
                ? stagePocket.ObjectiveKind
                : LinearStageObjectiveKind.None;
        public string CurrentObjectiveCue => TryGetCurrentPocket(out StageEncounterPocketBinding pocket)
            && pocket.TryResolvePocket(stageTemplate, out _, out LinearStagePocket stagePocket)
                ? stagePocket.ObjectiveCue
                : string.Empty;

        public void Configure(
            LinearStageTemplateProfile newStageTemplate,
            Transform newPlayer,
            StageEncounterPocketBinding[] newPockets)
        {
            stageTemplate = newStageTemplate;
            player = newPlayer;
            pockets = newPockets ?? Array.Empty<StageEncounterPocketBinding>();
            ResetProgress();
        }

        public StageEncounterPocketBinding GetPocketBinding(int index)
        {
            if (pockets == null || index < 0 || index >= pockets.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return pockets[index];
        }

        public bool IsPocketCompleted(int index)
        {
            if (index < 0 || index >= PocketCount)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            EnsureCompletionBuffer();
            return completedPockets[index];
        }

        public bool TryGetCurrentPocket(out StageEncounterPocketBinding pocket)
        {
            if (pockets != null && currentPocketIndex >= 0 && currentPocketIndex < pockets.Length)
            {
                pocket = pockets[currentPocketIndex];
                return true;
            }

            pocket = default;
            return false;
        }

        public void ResetProgress()
        {
            completedPockets = pockets != null ? new bool[pockets.Length] : Array.Empty<bool>();
            currentPocketIndex = -1;
            routeComplete = false;
        }

        public void RefreshProgress()
        {
            EnsureCompletionBuffer();
            if (routeComplete || pockets == null || pockets.Length == 0 || player == null)
            {
                return;
            }

            if (currentPocketIndex >= 0 && pockets[currentPocketIndex].IsCleared)
            {
                completedPockets[currentPocketIndex] = true;
                currentPocketIndex = -1;
            }

            if (currentPocketIndex < 0)
            {
                TryEnterNextPocket();
            }
        }

        private void OnEnable()
        {
            ResetProgress();
            RefreshProgress();
        }

        private void Update()
        {
            RefreshProgress();
        }

        private void TryEnterNextPocket()
        {
            int nextIndex = FindFirstIncompletePocketIndex();
            if (nextIndex < 0)
            {
                routeComplete = true;
                return;
            }

            if (!pockets[nextIndex].ContainsPosition(player.position))
            {
                return;
            }

            currentPocketIndex = nextIndex;
            if (pockets[currentPocketIndex].IsCleared)
            {
                completedPockets[currentPocketIndex] = true;
                currentPocketIndex = -1;
                TryEnterNextPocket();
            }
        }

        private int FindFirstIncompletePocketIndex()
        {
            for (int i = 0; i < completedPockets.Length; i++)
            {
                if (!completedPockets[i])
                {
                    return i;
                }
            }

            return -1;
        }

        private int CountCompletedPockets()
        {
            EnsureCompletionBuffer();
            int count = 0;
            for (int i = 0; i < completedPockets.Length; i++)
            {
                if (completedPockets[i])
                {
                    count++;
                }
            }

            return count;
        }

        private void EnsureCompletionBuffer()
        {
            int pocketCount = PocketCount;
            if (completedPockets == null || completedPockets.Length != pocketCount)
            {
                completedPockets = new bool[pocketCount];
                currentPocketIndex = -1;
                routeComplete = false;
            }
        }
    }

    [Serializable]
    public struct StageEncounterPocketBinding
    {
        [SerializeField] private string label;
        [SerializeField] private int segmentIndex;
        [SerializeField] private int pocketIndex;
        [SerializeField] private Transform enterCenter;
        [SerializeField, Min(0f)] private float enterRadius;
        [SerializeField] private CombatHealth[] enemies;

        public StageEncounterPocketBinding(
            string label,
            int segmentIndex,
            int pocketIndex,
            Transform enterCenter,
            float enterRadius,
            CombatHealth[] enemies)
        {
            this.label = label;
            this.segmentIndex = segmentIndex;
            this.pocketIndex = pocketIndex;
            this.enterCenter = enterCenter;
            this.enterRadius = enterRadius;
            this.enemies = enemies ?? Array.Empty<CombatHealth>();
        }

        public string Label => label;
        public int SegmentIndex => segmentIndex;
        public int PocketIndex => pocketIndex;
        public Transform EnterCenter => enterCenter;
        public float EnterRadius => enterRadius;
        public int EnemyCount => enemies != null ? enemies.Length : 0;
        public bool IsCleared => CountAliveEnemies() == 0;

        public bool ContainsPosition(Vector3 position)
        {
            if (enterCenter == null)
            {
                return false;
            }

            Vector3 offset = Vector3.ProjectOnPlane(position - enterCenter.position, Vector3.up);
            return enterRadius <= 0f || offset.sqrMagnitude <= enterRadius * enterRadius;
        }

        public int CountAliveEnemies()
        {
            if (enemies == null)
            {
                return 0;
            }

            int aliveCount = 0;
            for (int i = 0; i < enemies.Length; i++)
            {
                if (enemies[i] != null && enemies[i].IsAlive)
                {
                    aliveCount++;
                }
            }

            return aliveCount;
        }

        public bool TryResolvePocket(
            LinearStageTemplateProfile template,
            out LinearStageSegmentProfile segment,
            out LinearStagePocket pocket)
        {
            segment = null;
            pocket = default;
            if (template == null || segmentIndex < 0 || segmentIndex >= template.SegmentCount)
            {
                return false;
            }

            segment = template.GetSegment(segmentIndex);
            if (pocketIndex < 0 || pocketIndex >= segment.PocketCount)
            {
                return false;
            }

            pocket = segment.GetPocket(pocketIndex);
            return true;
        }
    }
}
