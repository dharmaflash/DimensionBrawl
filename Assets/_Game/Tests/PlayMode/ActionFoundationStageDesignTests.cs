using System.Collections.Generic;
using DimensionBrawl.LevelDesign;
using NUnit.Framework;
using UnityEditor;

namespace DimensionBrawl.Tests
{
    public sealed class ActionFoundationStageDesignTests
    {
        private const string SegmentRootPath = "Assets/_Game/DesignData/Profiles/ActionFoundation/StageDesign/Segments";
        private const string TemplateRootPath = "Assets/_Game/DesignData/Profiles/ActionFoundation/StageDesign/Templates";

        private static readonly string[] SegmentPaths =
        {
            SegmentRootPath + "/DB_Segment_EntryRead.asset",
            SegmentRootPath + "/DB_Segment_BasicPressure.asset",
            SegmentRootPath + "/DB_Segment_BreakGate.asset",
            SegmentRootPath + "/DB_Segment_BacklinePressure.asset",
            SegmentRootPath + "/DB_Segment_PressureRescue.asset",
            SegmentRootPath + "/DB_Segment_ReliefReset.asset",
            SegmentRootPath + "/DB_Segment_BossBreakHandoff.asset",
            SegmentRootPath + "/DB_Segment_FinalStand.asset"
        };

        private static readonly string[] TemplatePaths =
        {
            TemplateRootPath + "/DB_StageTemplate_S1_1_BreakGate.asset",
            TemplateRootPath + "/DB_StageTemplate_S1_2_BacklineSignal.asset",
            TemplateRootPath + "/DB_StageTemplate_S1_3_TankRescue.asset",
            TemplateRootPath + "/DB_StageTemplate_S1_4_HealPocket.asset",
            TemplateRootPath + "/DB_StageTemplate_S1_5_BossStand.asset"
        };

        [Test]
        public void LinearStageDesignTemplatesCoverFirstStageSet()
        {
            var coveredKinds = new HashSet<LinearStageSegmentKind>();
            for (int i = 0; i < SegmentPaths.Length; i++)
            {
                LinearStageSegmentProfile segment = LoadRequired<LinearStageSegmentProfile>(SegmentPaths[i]);
                Assert.Greater(segment.PocketCount, 0, $"{segment.SegmentId} should contain at least one pocket.");
                coveredKinds.Add(segment.SegmentKind);
            }

            Assert.IsTrue(coveredKinds.Contains(LinearStageSegmentKind.EntryRead));
            Assert.IsTrue(coveredKinds.Contains(LinearStageSegmentKind.BasicPressure));
            Assert.IsTrue(coveredKinds.Contains(LinearStageSegmentKind.BreakGate));
            Assert.IsTrue(coveredKinds.Contains(LinearStageSegmentKind.BacklinePressure));
            Assert.IsTrue(coveredKinds.Contains(LinearStageSegmentKind.PressureRescue));
            Assert.IsTrue(coveredKinds.Contains(LinearStageSegmentKind.Relief));
            Assert.IsTrue(coveredKinds.Contains(LinearStageSegmentKind.BossBreakHandoff));
            Assert.IsTrue(coveredKinds.Contains(LinearStageSegmentKind.FinalStand));

            for (int i = 0; i < TemplatePaths.Length; i++)
            {
                LinearStageTemplateProfile template = LoadRequired<LinearStageTemplateProfile>(TemplatePaths[i]);
                Assert.GreaterOrEqual(template.SegmentCount, 4, $"{template.StageTemplateId} should describe a linear route, not a single encounter.");
                Assert.AreEqual(LinearStageSegmentKind.EntryRead, template.GetSegment(0).SegmentKind, $"{template.StageTemplateId} should begin with EntryRead.");
                Assert.AreEqual(LinearStageSegmentKind.FinalStand, template.GetSegment(template.SegmentCount - 1).SegmentKind, $"{template.StageTemplateId} should end with FinalStand.");
                Assert.IsTrue(ContainsSegment(template, LinearStageSegmentKind.Relief), $"{template.StageTemplateId} should include a relief beat between pressure spikes.");
            }
        }

        [Test]
        public void LinearStagePocketsReferencePromotedRoleProfilesOnly()
        {
            for (int i = 0; i < SegmentPaths.Length; i++)
            {
                LinearStageSegmentProfile segment = LoadRequired<LinearStageSegmentProfile>(SegmentPaths[i]);
                for (int pocketIndex = 0; pocketIndex < segment.PocketCount; pocketIndex++)
                {
                    LinearStagePocket pocket = segment.GetPocket(pocketIndex);
                    Assert.IsTrue(pocket.HasObjective, $"{segment.SegmentId}/{pocket.PocketId} should declare an objective kind and cue.");
                    if (!pocket.AllowsNoEnemies)
                    {
                        Assert.Greater(pocket.EnemyRoleCount, 0, $"{segment.SegmentId}/{pocket.PocketId} should reference at least one role.");
                    }

                    for (int roleIndex = 0; roleIndex < pocket.EnemyRoleCount; roleIndex++)
                    {
                        StageEnemyRoleSlot slot = pocket.GetEnemyRole(roleIndex);
                        Assert.NotNull(slot.Role, $"{segment.SegmentId}/{pocket.PocketId} has a missing role reference.");
                        Assert.IsTrue(slot.HasValidCountRange, $"{slot.Role.RoleId} should have a valid min/max count range.");
                        Assert.Greater(slot.SelectionWeight, 0f, $"{slot.Role.RoleId} should have a positive selection weight.");

                        string path = AssetDatabase.GetAssetPath(slot.Role);
                        Assert.IsTrue(path.StartsWith("Assets/_Game/"), $"{slot.Role.RoleId} should reference a game-owned role asset.");
                        Assert.IsFalse(path.Contains("/_Imported/"), $"{slot.Role.RoleId} should not reference imported source packs.");
                    }
                }
            }
        }

        private static bool ContainsSegment(LinearStageTemplateProfile template, LinearStageSegmentKind kind)
        {
            for (int i = 0; i < template.SegmentCount; i++)
            {
                if (template.GetSegment(i).SegmentKind == kind)
                {
                    return true;
                }
            }

            return false;
        }

        private static T LoadRequired<T>(string assetPath) where T : UnityEngine.Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            Assert.NotNull(asset, $"Missing required asset at {assetPath}.");
            return asset;
        }
    }
}
