using UnityEngine;

namespace DimensionBrawl.LevelDesign
{
    public enum StageAnchorUsageKind
    {
        Generic = 0,
        CombatSpawn = 1,
        CutsceneHandoff = 2,
        RuntimeState = 3
    }

    public sealed class StageAnchorPoint : MonoBehaviour
    {
        [SerializeField] private string anchorId;
        [SerializeField] private string groupId;
        [SerializeField] private StageAnchorUsageKind usageKind;
        [SerializeField] private int positionId;
        [SerializeField] private StageSpawnKind spawnKind;
        [SerializeField] private StageRuntimeStateKind runtimeStateKind;
        [TextArea, SerializeField] private string purpose;

        public string AnchorId => anchorId;
        public string GroupId => groupId;
        public StageAnchorUsageKind UsageKind => usageKind;
        public int PositionId => positionId;
        public StageSpawnKind SpawnKind => spawnKind;
        public StageRuntimeStateKind RuntimeStateKind => runtimeStateKind;
        public string Purpose => purpose;
        public bool HasPositionId => positionId > 0;
    }
}
