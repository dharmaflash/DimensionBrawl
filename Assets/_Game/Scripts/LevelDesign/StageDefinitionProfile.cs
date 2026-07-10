using System;
using UnityEngine;

namespace DimensionBrawl.LevelDesign
{
    public enum StageSpawnKind
    {
        Player = 0,
        Boss = 1,
        Add = 2,
        Rift = 3,
        Objective = 4
    }

    public enum StageRuntimeStateKind
    {
        StageSpawner = 0,
        CutsceneHandoff = 1,
        StageClear = 2,
        FieldObject = 3
    }

    [CreateAssetMenu(
        menuName = "DimensionBrawl/Profiles/Stage Definition Profile",
        fileName = "DB_StageDefinition")]
    public sealed class StageDefinitionProfile : ScriptableObject
    {
        [Serializable]
        public struct AnchorRef
        {
            [SerializeField] private string anchorId;
            [SerializeField] private string groupId;
            [SerializeField] private Vector3 expectedPosition;
            [SerializeField] private Vector3 expectedEuler;
            [TextArea, SerializeField] private string purpose;

            public string AnchorId => anchorId;
            public string GroupId => groupId;
            public Vector3 ExpectedPosition => expectedPosition;
            public Vector3 ExpectedEuler => expectedEuler;
            public string Purpose => purpose;
        }

        [Serializable]
        public struct SpawnRef
        {
            [SerializeField] private string spawnId;
            [SerializeField] private StageSpawnKind spawnKind;
            [SerializeField] private int positionId;
            [SerializeField] private string anchorId;
            [SerializeField] private string payloadId;
            [SerializeField, Min(0)] private int count;
            [SerializeField, Min(0f)] private float delaySeconds;
            [TextArea, SerializeField] private string note;

            public string SpawnId => spawnId;
            public StageSpawnKind SpawnKind => spawnKind;
            public int PositionId => positionId;
            public string AnchorId => anchorId;
            public string PayloadId => payloadId;
            public int Count => Mathf.Max(0, count);
            public float DelaySeconds => Mathf.Max(0f, delaySeconds);
            public string Note => note;
        }

        [Serializable]
        public struct CutsceneHandoffRef
        {
            [SerializeField] private string handoffId;
            [SerializeField] private string anchorId;
            [SerializeField] private string cinematicProfileId;
            [SerializeField] private string timelineAssetPath;
            [SerializeField] private string nextEventId;
            [TextArea, SerializeField] private string purpose;

            public string HandoffId => handoffId;
            public string AnchorId => anchorId;
            public string CinematicProfileId => cinematicProfileId;
            public string TimelineAssetPath => timelineAssetPath;
            public string NextEventId => nextEventId;
            public string Purpose => purpose;
        }

        [Serializable]
        public struct RuntimeStateRef
        {
            [SerializeField] private string stateId;
            [SerializeField] private StageRuntimeStateKind stateKind;
            [SerializeField] private int positionId;
            [SerializeField] private string anchorId;
            [SerializeField] private string conditionId;
            [TextArea, SerializeField] private string note;

            public string StateId => stateId;
            public StageRuntimeStateKind StateKind => stateKind;
            public int PositionId => positionId;
            public string AnchorId => anchorId;
            public string ConditionId => conditionId;
            public string Note => note;
        }

        [Serializable]
        public struct SourceReference
        {
            [SerializeField] private string sourceId;
            [SerializeField] private string sourcePath;
            [TextArea, SerializeField] private string localTakeaway;

            public string SourceId => sourceId;
            public string SourcePath => sourcePath;
            public string LocalTakeaway => localTakeaway;
        }

        [Header("Identity")]
        [SerializeField] private string stageId = "OLYMPUS-CORRIDOR-INTRO-COMBAT-01";
        [SerializeField] private string displayName = "Olympus Corridor Invasion";
        [SerializeField] private string chapterId = "OLYMPUS-INVASION";
        [SerializeField] private string previousStageId;
        [SerializeField] private string nextStageId;

        [Header("Shared Map")]
        [SerializeField] private string mapScenePath = "Assets/_Game/Scenes/OlympusCorridorInvasionStage.unity";
        [SerializeField] private string mapRootName = "OlympusCorridorStageRoot";
        [SerializeField] private string mapContentRootName = "OlympusCorridorStageMap";
        [SerializeField] private Vector3 mapScale = new Vector3(1.5f, 1.5f, 1.5f);
        [SerializeField] private string layoutId = "OLYMPUS_CORRIDOR_LONG_01";
        [SerializeField] private string scenePrefabSource = string.Empty;

        [Header("Design Contract")]
        [TextArea, SerializeField] private string objective =
            "Intro cutscene exits into a shared Olympus corridor combat stage, then boss entrance and combat start share the same anchor set.";
        [TextArea, SerializeField] private string clearCondition =
            "Stage clear is authored as a corridor exit/state hook, not as a reward or combat balance contract.";
        [TextArea, SerializeField] private string excludedScope =
            "No enemy damage tuning, skill balance, reward payout, or final NavMesh bake in this definition.";

        [Header("Anchors")]
        [SerializeField] private AnchorRef[] anchors = Array.Empty<AnchorRef>();

        [Header("Stage Spawners")]
        [SerializeField] private SpawnRef[] spawns = Array.Empty<SpawnRef>();

        [Header("Cutscene Handoffs")]
        [SerializeField] private CutsceneHandoffRef[] cutsceneHandoffs = Array.Empty<CutsceneHandoffRef>();

        [Header("Runtime State")]
        [SerializeField] private RuntimeStateRef[] runtimeStates = Array.Empty<RuntimeStateRef>();

        [Header("Source References")]
        [SerializeField] private SourceReference[] sourceReferences = Array.Empty<SourceReference>();

        public string StageId => stageId;
        public string DisplayName => displayName;
        public string ChapterId => chapterId;
        public string PreviousStageId => previousStageId;
        public string NextStageId => nextStageId;
        public string MapScenePath => mapScenePath;
        public string MapRootName => mapRootName;
        public string MapContentRootName => mapContentRootName;
        public Vector3 MapScale => mapScale;
        public string LayoutId => layoutId;
        public string ScenePrefabSource => scenePrefabSource;
        public string Objective => objective;
        public string ClearCondition => clearCondition;
        public string ExcludedScope => excludedScope;
        public int AnchorCount => anchors != null ? anchors.Length : 0;
        public int SpawnCount => spawns != null ? spawns.Length : 0;
        public int CutsceneHandoffCount => cutsceneHandoffs != null ? cutsceneHandoffs.Length : 0;
        public int RuntimeStateCount => runtimeStates != null ? runtimeStates.Length : 0;
        public int SourceReferenceCount => sourceReferences != null ? sourceReferences.Length : 0;

        public AnchorRef GetAnchor(int index)
        {
            if (anchors == null || index < 0 || index >= anchors.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return anchors[index];
        }

        public SpawnRef GetSpawn(int index)
        {
            if (spawns == null || index < 0 || index >= spawns.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return spawns[index];
        }

        public CutsceneHandoffRef GetCutsceneHandoff(int index)
        {
            if (cutsceneHandoffs == null || index < 0 || index >= cutsceneHandoffs.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return cutsceneHandoffs[index];
        }

        public RuntimeStateRef GetRuntimeState(int index)
        {
            if (runtimeStates == null || index < 0 || index >= runtimeStates.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return runtimeStates[index];
        }

        public SourceReference GetSourceReference(int index)
        {
            if (sourceReferences == null || index < 0 || index >= sourceReferences.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return sourceReferences[index];
        }
    }
}
