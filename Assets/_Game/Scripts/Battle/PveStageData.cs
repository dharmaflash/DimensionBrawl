using System;
using System.Collections.Generic;
using UnityEngine;

namespace IsekaiBrawl.Gameplay
{
    public enum PveFinalObjectiveType
    {
        Core = 0,
        BossCore = 1
    }

    public enum PveProjectileEmitterType
    {
        Direct = 0,
        Line = 1,
        Spread = 2
    }

    public enum PveProjectileEmitterTriggerMode
    {
        OnEncounterStart = 0,
        OnGroupCleared = 1,
        LoopWhileAlive = 2
    }

    [Serializable]
    public sealed class PveEnemyPlacement
    {
        public SummonData summonData;
        [Range(0, 4)] public int laneIndex = 2;
        public float depthZ = 24f;
        public float lateralOffset;
        public float spawnDelay;
    }

    [Serializable]
    public sealed class PveStructurePlacement
    {
        public BattleStructureRole structureRole = BattleStructureRole.FrontlineBlocker;
        [Range(0, 4)] public int laneIndex = 2;
        public float depthZ = 32f;
        public float maxHpOverride = 140f;
        public float energyRewardOverride;
        public Vector3 worldScale = new(1.05f, 1.4f, 1.05f);
        public Color tint = new(0.9f, 0.78f, 0.36f, 1f);
    }

    [Serializable]
    public sealed class PveProjectileEmitterPlacement
    {
        public string emitterId = "Emitter";
        [Range(0, 4)] public int laneIndex = 2;
        public float depthZ = 30f;
        public PveProjectileEmitterType emitterType = PveProjectileEmitterType.Direct;
        public PveProjectileEmitterTriggerMode triggerMode = PveProjectileEmitterTriggerMode.OnEncounterStart;
        public float interval = 4.6f;
        public float leadTime = 0.9f;
        public float damage = 10f;
        public bool usesWarningLine = true;
    }

    [Serializable]
    public sealed class PveEncounterGroup
    {
        public string groupId = "Encounter";
        public float triggerZ = 18f;
        public bool mustClearToAdvance = true;
        public float spawnOnEnterDelay;
        public float cameraStopZ = -1f;
        public List<PveEnemyPlacement> enemyPlacements = new();
        public List<PveStructurePlacement> structurePlacements = new();
        public List<PveProjectileEmitterPlacement> projectileEmitterPlacements = new();
    }

    [CreateAssetMenu(fileName = "PveStageData", menuName = "IsekaiBrawl/PVE Stage Data")]
    public sealed class PveStageData : ScriptableObject
    {
        [SerializeField] private string stageId = "stage_01";
        [SerializeField] private string displayName = "스토리 전투";
        [SerializeField] [TextArea] private string description = string.Empty;
        [SerializeField] private float timeLimit = 165f;
        [SerializeField] private float startingEnergyOverride = -1f;
        [SerializeField] private float laneLengthOverride = -1f;
        [SerializeField] private PveFinalObjectiveType finalObjectiveType = PveFinalObjectiveType.Core;
        [SerializeField] private float finalObjectiveHP = 1000f;
        [SerializeField] private List<PveEncounterGroup> encounterGroups = new();

        public string StageId => stageId;
        public string DisplayName => displayName;
        public string Description => description;
        public float TimeLimit => timeLimit;
        public float StartingEnergyOverride => startingEnergyOverride;
        public float LaneLengthOverride => laneLengthOverride;
        public PveFinalObjectiveType FinalObjectiveType => finalObjectiveType;
        public float FinalObjectiveHP => finalObjectiveHP;
        public IReadOnlyList<PveEncounterGroup> EncounterGroups => encounterGroups;

        public static PveStageData CreateRuntimePrototypeStage(IReadOnlyList<SummonData> sourceDeck)
        {
            PveStageData stage = CreateInstance<PveStageData>();
            stage.hideFlags = HideFlags.DontSave;
            stage.stageId = "story_runtime_stage_01";
            stage.displayName = "스토리 전투";
            stage.description = "고정 배치 전장을 돌파하며 장치 직격과 보스 패턴을 회피하는 기본 스테이지입니다.";
            stage.timeLimit = 165f;
            stage.startingEnergyOverride = 40f;
            stage.laneLengthOverride = 84f;
            stage.finalObjectiveType = PveFinalObjectiveType.BossCore;
            stage.finalObjectiveHP = 1000f;
            stage.encounterGroups = BuildRuntimeEncounterGroups(sourceDeck);
            return stage;
        }

        private static List<PveEncounterGroup> BuildRuntimeEncounterGroups(IReadOnlyList<SummonData> sourceDeck)
        {
            SummonData rush = FindSummonByShortLabel(sourceDeck, "Rush");
            SummonData arrow = FindSummonByShortLabel(sourceDeck, "Arrow");
            SummonData breakCard = FindSummonByShortLabel(sourceDeck, "Break");

            List<PveEncounterGroup> groups = new();

            PveEncounterGroup groupOne = new()
            {
                groupId = "approach",
                triggerZ = 8f,
                mustClearToAdvance = true,
                spawnOnEnterDelay = 0f
            };
            groupOne.structurePlacements.Add(new PveStructurePlacement
            {
                structureRole = BattleStructureRole.FrontlineBlocker,
                laneIndex = 2,
                depthZ = 24.8f,
                maxHpOverride = 126f,
                worldScale = new Vector3(0.96f, 1.26f, 0.96f),
                tint = new Color(0.86f, 0.74f, 0.34f, 1f)
            });
            groupOne.structurePlacements.Add(new PveStructurePlacement
            {
                structureRole = BattleStructureRole.FrontlineBlocker,
                laneIndex = 1,
                depthZ = 28.6f,
                maxHpOverride = 132f,
                worldScale = new Vector3(1f, 1.32f, 1f),
                tint = new Color(0.84f, 0.72f, 0.32f, 1f)
            });
            if (rush != null)
            {
                groupOne.enemyPlacements.Add(new PveEnemyPlacement { summonData = rush, laneIndex = 1, depthZ = 19.6f, lateralOffset = -0.12f });
                groupOne.enemyPlacements.Add(new PveEnemyPlacement { summonData = rush, laneIndex = 2, depthZ = 18.8f, lateralOffset = -0.06f, spawnDelay = 0.08f });
                groupOne.enemyPlacements.Add(new PveEnemyPlacement { summonData = rush, laneIndex = 3, depthZ = 19.2f, lateralOffset = 0.12f, spawnDelay = 0.16f });
            }
            if (arrow != null)
            {
                groupOne.enemyPlacements.Add(new PveEnemyPlacement { summonData = arrow, laneIndex = 2, depthZ = 22.6f, lateralOffset = 0.1f, spawnDelay = 0.24f });
            }

            PveEncounterGroup groupTwo = new()
            {
                groupId = "turret_hold",
                triggerZ = 26f,
                mustClearToAdvance = true,
                spawnOnEnterDelay = 0.25f
            };
            groupTwo.structurePlacements.Add(new PveStructurePlacement
            {
                structureRole = BattleStructureRole.FrontlineBlocker,
                laneIndex = 2,
                depthZ = 38.2f,
                maxHpOverride = 168f,
                worldScale = new Vector3(1.08f, 1.42f, 1.08f),
                tint = new Color(0.92f, 0.66f, 0.28f, 1f)
            });
            groupTwo.structurePlacements.Add(new PveStructurePlacement
            {
                structureRole = BattleStructureRole.RewardObjective,
                laneIndex = 3,
                depthZ = 45.8f,
                maxHpOverride = 92f,
                energyRewardOverride = 18f,
                worldScale = new Vector3(0.88f, 1.08f, 0.88f),
                tint = new Color(0.34f, 0.96f, 0.64f, 1f)
            });
            groupTwo.projectileEmitterPlacements.Add(new PveProjectileEmitterPlacement
            {
                emitterId = "Turret_L4",
                laneIndex = 3,
                depthZ = 42.2f,
                emitterType = PveProjectileEmitterType.Direct,
                triggerMode = PveProjectileEmitterTriggerMode.LoopWhileAlive,
                interval = 5.2f,
                leadTime = 0.9f,
                damage = 10f,
                usesWarningLine = true
            });
            if (arrow != null)
            {
                groupTwo.enemyPlacements.Add(new PveEnemyPlacement { summonData = arrow, laneIndex = 1, depthZ = 33.8f, lateralOffset = -0.16f });
                groupTwo.enemyPlacements.Add(new PveEnemyPlacement { summonData = arrow, laneIndex = 2, depthZ = 35.6f, lateralOffset = -0.08f, spawnDelay = 0.12f });
                groupTwo.enemyPlacements.Add(new PveEnemyPlacement { summonData = arrow, laneIndex = 3, depthZ = 34.6f, lateralOffset = 0.16f, spawnDelay = 0.2f });
            }

            PveEncounterGroup groupThree = new()
            {
                groupId = "final_gate",
                triggerZ = 48f,
                mustClearToAdvance = false,
                spawnOnEnterDelay = 0f
            };
            groupThree.structurePlacements.Add(new PveStructurePlacement
            {
                structureRole = BattleStructureRole.SiegeObjective,
                laneIndex = 2,
                depthZ = 50.6f,
                maxHpOverride = 238f,
                worldScale = new Vector3(1.32f, 1.18f, 1.32f),
                tint = new Color(1f, 0.48f, 0.28f, 1f)
            });
            groupThree.projectileEmitterPlacements.Add(new PveProjectileEmitterPlacement
            {
                emitterId = "GateLine_L3",
                laneIndex = 2,
                depthZ = 51.4f,
                emitterType = PveProjectileEmitterType.Line,
                triggerMode = PveProjectileEmitterTriggerMode.OnEncounterStart,
                interval = 8.8f,
                leadTime = 1.05f,
                damage = 14f,
                usesWarningLine = true
            });
            groupThree.projectileEmitterPlacements.Add(new PveProjectileEmitterPlacement
            {
                emitterId = "BossSupportTurret_L3",
                laneIndex = 2,
                depthZ = 55.2f,
                emitterType = PveProjectileEmitterType.Direct,
                triggerMode = PveProjectileEmitterTriggerMode.LoopWhileAlive,
                interval = 6.1f,
                leadTime = 0.95f,
                damage = 11f,
                usesWarningLine = true
            });
            if (breakCard != null)
            {
                groupThree.enemyPlacements.Add(new PveEnemyPlacement { summonData = breakCard, laneIndex = 2, depthZ = 47.2f, lateralOffset = -0.1f });
            }

            groups.Add(groupOne);
            groups.Add(groupTwo);
            groups.Add(groupThree);
            return groups;
        }

        private static SummonData FindSummonByShortLabel(IReadOnlyList<SummonData> sourceDeck, string shortLabel)
        {
            if (sourceDeck == null || sourceDeck.Count == 0 || string.IsNullOrWhiteSpace(shortLabel))
            {
                return null;
            }

            for (int index = 0; index < sourceDeck.Count; index++)
            {
                SummonData summonData = sourceDeck[index];
                if (summonData != null && string.Equals(summonData.shortLabel, shortLabel, StringComparison.OrdinalIgnoreCase))
                {
                    return summonData;
                }
            }

            return sourceDeck[0];
        }
    }

    public static class PveStageContext
    {
        public static PveStageData SelectedStage { get; private set; }

        public static void SetStage(PveStageData stage)
        {
            SelectedStage = stage;
        }

        public static void Clear()
        {
            SelectedStage = null;
        }
    }

}
