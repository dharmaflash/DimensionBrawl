using System;
using System.Collections.Generic;
using DimensionBrawl.AI;
using DimensionBrawl.LevelDesign;
using UnityEditor;
using UnityEngine;

namespace DimensionBrawl.Editor
{
    public static class ActionFoundationStageDesignSetup
    {
        public const string StageDesignRoot = ActionFoundationProfileSetup.ProfileRoot + "/StageDesign";
        public const string SegmentRoot = StageDesignRoot + "/Segments";
        public const string TemplateRoot = StageDesignRoot + "/Templates";

        private static readonly string[] SegmentPaths =
        {
            SegmentRoot + "/DB_Segment_EntryRead.asset",
            SegmentRoot + "/DB_Segment_BasicPressure.asset",
            SegmentRoot + "/DB_Segment_BreakGate.asset",
            SegmentRoot + "/DB_Segment_BacklinePressure.asset",
            SegmentRoot + "/DB_Segment_PressureRescue.asset",
            SegmentRoot + "/DB_Segment_ReliefReset.asset",
            SegmentRoot + "/DB_Segment_BossBreakHandoff.asset",
            SegmentRoot + "/DB_Segment_FinalStand.asset"
        };

        private static readonly string[] TemplatePaths =
        {
            TemplateRoot + "/DB_StageTemplate_S1_1_BreakGate.asset",
            TemplateRoot + "/DB_StageTemplate_S1_2_BacklineSignal.asset",
            TemplateRoot + "/DB_StageTemplate_S1_3_TankRescue.asset",
            TemplateRoot + "/DB_StageTemplate_S1_4_HealPocket.asset",
            TemplateRoot + "/DB_StageTemplate_S1_5_BossStand.asset"
        };

        private static readonly LinearStageSegmentKind[] RequiredSegmentKinds =
        {
            LinearStageSegmentKind.EntryRead,
            LinearStageSegmentKind.BasicPressure,
            LinearStageSegmentKind.BreakGate,
            LinearStageSegmentKind.BacklinePressure,
            LinearStageSegmentKind.PressureRescue,
            LinearStageSegmentKind.Relief,
            LinearStageSegmentKind.BossBreakHandoff,
            LinearStageSegmentKind.FinalStand
        };

        [MenuItem("DimensionBrawl/Validate Action Foundation Stage Design Templates")]
        public static void ValidateStageDesignTemplatesMenu()
        {
            ValidateStageDesignAssets();
            Debug.Log("ActionFoundation linear stage design template validation passed.");
        }

        public static void ValidateStageDesignAssets()
        {
            ActionFoundationEnemyRoleDeckSetup.ValidateEnemyRoleAssets();

            var coveredKinds = new HashSet<LinearStageSegmentKind>();
            for (int i = 0; i < SegmentPaths.Length; i++)
            {
                LinearStageSegmentProfile segment = LoadRequired<LinearStageSegmentProfile>(SegmentPaths[i]);
                ValidateSegment(segment, SegmentPaths[i]);
                coveredKinds.Add(segment.SegmentKind);
            }

            for (int i = 0; i < RequiredSegmentKinds.Length; i++)
            {
                if (!coveredKinds.Contains(RequiredSegmentKinds[i]))
                {
                    throw new InvalidOperationException($"Stage segment catalog does not cover {RequiredSegmentKinds[i]}.");
                }
            }

            for (int i = 0; i < TemplatePaths.Length; i++)
            {
                LinearStageTemplateProfile template = LoadRequired<LinearStageTemplateProfile>(TemplatePaths[i]);
                ValidateTemplate(template, TemplatePaths[i]);
            }
        }

        private static void ValidateSegment(LinearStageSegmentProfile segment, string assetPath)
        {
            if (string.IsNullOrWhiteSpace(segment.SegmentId))
            {
                throw new InvalidOperationException($"{assetPath} has no segment id.");
            }

            if (string.IsNullOrWhiteSpace(segment.CombatLesson))
            {
                throw new InvalidOperationException($"{segment.SegmentId} has no combat lesson.");
            }

            if (segment.RecommendedDurationSeconds <= 0f)
            {
                throw new InvalidOperationException($"{segment.SegmentId} has no usable duration.");
            }

            if (segment.PocketCount == 0)
            {
                throw new InvalidOperationException($"{segment.SegmentId} has no encounter pockets.");
            }

            for (int i = 0; i < segment.PocketCount; i++)
            {
                ValidatePocket(segment, segment.GetPocket(i));
            }
        }

        private static void ValidatePocket(LinearStageSegmentProfile segment, LinearStagePocket pocket)
        {
            if (string.IsNullOrWhiteSpace(pocket.PocketId))
            {
                throw new InvalidOperationException($"{segment.SegmentId} has a pocket without an id.");
            }

            if (pocket.TargetDurationSeconds <= 0f)
            {
                throw new InvalidOperationException($"{segment.SegmentId}/{pocket.PocketId} has no usable duration.");
            }

            if (!pocket.HasObjective)
            {
                throw new InvalidOperationException($"{segment.SegmentId}/{pocket.PocketId} has no objective kind or objective cue.");
            }

            if (!pocket.AllowsNoEnemies && pocket.EnemyRoleCount == 0)
            {
                throw new InvalidOperationException($"{segment.SegmentId}/{pocket.PocketId} is not relief but has no enemy roles.");
            }

            for (int i = 0; i < pocket.EnemyRoleCount; i++)
            {
                StageEnemyRoleSlot slot = pocket.GetEnemyRole(i);
                ValidateRoleSlot(segment, pocket, slot);
            }
        }

        private static void ValidateRoleSlot(LinearStageSegmentProfile segment, LinearStagePocket pocket, StageEnemyRoleSlot slot)
        {
            if (!slot.HasRole)
            {
                throw new InvalidOperationException($"{segment.SegmentId}/{pocket.PocketId} has a missing role reference.");
            }

            if (!slot.HasValidCountRange)
            {
                throw new InvalidOperationException($"{segment.SegmentId}/{pocket.PocketId}/{slot.Role.RoleId} has invalid count range.");
            }

            if (slot.SelectionWeight <= 0f)
            {
                throw new InvalidOperationException($"{segment.SegmentId}/{pocket.PocketId}/{slot.Role.RoleId} has no selection weight.");
            }

            string rolePath = AssetDatabase.GetAssetPath(slot.Role);
            if (!rolePath.StartsWith("Assets/_Game/", StringComparison.Ordinal) || rolePath.Contains("/_Imported/", StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"{segment.SegmentId}/{pocket.PocketId}/{slot.Role.RoleId} references a non-promoted role asset: {rolePath}");
            }
        }

        private static void ValidateTemplate(LinearStageTemplateProfile template, string assetPath)
        {
            if (string.IsNullOrWhiteSpace(template.StageTemplateId))
            {
                throw new InvalidOperationException($"{assetPath} has no stage template id.");
            }

            if (template.TargetRunDurationSeconds < 120f)
            {
                throw new InvalidOperationException($"{template.StageTemplateId} is below the 3-5 minute story run envelope.");
            }

            if (template.SegmentCount < 4)
            {
                throw new InvalidOperationException($"{template.StageTemplateId} does not describe enough linear route beats.");
            }

            ValidateRouteEndpoints(template);
            ValidateRouteSegmentRefs(template);
        }

        private static void ValidateRouteEndpoints(LinearStageTemplateProfile template)
        {
            LinearStageSegmentProfile first = template.GetSegment(0);
            LinearStageSegmentProfile last = template.GetSegment(template.SegmentCount - 1);
            if (first == null || first.SegmentKind != LinearStageSegmentKind.EntryRead)
            {
                throw new InvalidOperationException($"{template.StageTemplateId} must begin with EntryRead.");
            }

            if (last == null || last.SegmentKind != LinearStageSegmentKind.FinalStand)
            {
                throw new InvalidOperationException($"{template.StageTemplateId} must end with FinalStand.");
            }
        }

        private static void ValidateRouteSegmentRefs(LinearStageTemplateProfile template)
        {
            bool hasRelief = false;
            for (int i = 0; i < template.SegmentCount; i++)
            {
                LinearStageSegmentProfile segment = template.GetSegment(i);
                if (segment == null)
                {
                    throw new InvalidOperationException($"{template.StageTemplateId} has an empty segment reference at index {i}.");
                }

                string segmentPath = AssetDatabase.GetAssetPath(segment);
                if (!segmentPath.StartsWith(SegmentRoot, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException($"{template.StageTemplateId} references a segment outside the stage design root: {segmentPath}");
                }

                if (segment.SegmentKind == LinearStageSegmentKind.Relief)
                {
                    hasRelief = true;
                }
            }

            if (!hasRelief)
            {
                throw new InvalidOperationException($"{template.StageTemplateId} needs at least one relief segment between pressure spikes.");
            }
        }

        private static T LoadRequired<T>(string assetPath) where T : UnityEngine.Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (asset == null)
            {
                throw new InvalidOperationException($"Missing required asset at {assetPath}.");
            }

            return asset;
        }
    }
}
