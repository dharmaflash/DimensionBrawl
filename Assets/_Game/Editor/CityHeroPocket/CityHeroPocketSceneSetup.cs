using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using DimensionBrawl.AI;
using DimensionBrawl.Combat;
using DimensionBrawl.Enemies;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.Player;
using DimensionBrawl.Presentation;
using DimensionBrawl.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DimensionBrawl.Editor.CityHeroPocket
{
    /// <summary>
    /// Authors the direct-load CITY-GATE-01 product proof from owned, promoted assets.
    /// The Station scene is read only as the canonical Inori ranged-player wiring source;
    /// no Station, Corridor, shared UI or shared render-pipeline asset is modified.
    /// </summary>
    public static class CityHeroPocketSceneSetup
    {
        public const string ScenePath =
            "Assets/_Game/Scenes/CityHeroPocketStage.unity";
        public const string PlayerPrefabPath =
            "Assets/_Game/Prefabs/Player/PF_Player_Inori_RangedActionFoundation.prefab";
        public const string CityLookProfilePath =
            "Assets/_Game/Art/Environment/CityHeroPocket/Profiles/DB_CityHeroPocket_PostProcess.asset";

        public const string SourceStationScenePath =
            "Assets/_Game/Scenes/OlympusStationCombatStage.unity";
        public const string SourceStationPlayerName =
            "Player_CombatGirl_ActionFoundation";
        public const string SourceInoriVisualName =
            "BossBarrageLaneReview_RangedVisual_Inori";
        public const string PlayerRootName =
            "Player_CityHeroPocket_Inori";
        public const string EnemyRootName =
            "Enemy_CityHeroPocket_RifleCrossfire";
        public const string StageRootName =
            "CityHeroPocketStageRoot";
        public const string MapRootName =
            "CityHeroPocketMap";
        public const string RuntimeRootName =
            "CityHeroPocketRuntime";
        public const string HudRootName =
            "PF_UI_CombatHud";
        public const string GlobalVolumeName =
            "CityHeroPocket_GameplayBasePostProcess";
        public const string PlayerProjectileRootName =
            "CityHeroPocket_PlayerProjectiles";
        public const string EnemyProjectileRootName =
            "CityHeroPocket_EnemyProjectiles";
        public const string ExitTriggerName =
            "CityHeroPocket_ExitTrigger";
        public const string TransitionFocusName =
            "CityHeroPocket_TransitionFocus";
        public const string ExitPortalRootName =
            "CityHeroPocket_ExitPortal";
        public const string ExitCoverRootName =
            "CityHeroPocket_ExitCover";
        public const string DodgeBeatAnchorName =
            "CityHeroPocket_DodgeBeatAnchor";
        public const string ReserveEnemyAnchorName =
            "CityHeroPocket_ReserveEnemyAnchor";
        public const string ProductObjectiveText =
            "Clear the intersection and reach the breach.";

        public const string TokyoRoot =
            "Assets/_Game/Art/Environment/CityHeroPocket/TokyoStreet";

        private const string InoriAnimatorControllerPath =
            "Assets/_Game/Art/Animations/Player/RifleGirl/DB_RifleGirl_RangedCandidate.controller";
        private const string DodgeProfilePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_PlayerAction_BossBarrageLocalDefense.asset";
        private const string PlayerProjectilePrefabPath =
            "Assets/_Game/Prefabs/Combat/PF_PlayerRangedBasicProjectile_AimBolt.prefab";
        private const string EnemyPrefabPath =
            "Assets/_Game/Prefabs/Enemies/ActionFoundation/PF_Enemy_SciFiSoldier_Ranged_RifleCrossfire.prefab";
        private const string HudPrefabPath =
            "Assets/_Game/UI/CombatHud/PF_UI_CombatHud.prefab";
        public const string ExitPortalPrefabPath =
            "Assets/_Game/Art/VFX/CombatCues/Prefabs/DB_VFX_PlayerSummonPreSpawnPortal.prefab";

        internal const string AsphaltMaterialPath =
            "Assets/_Game/Art/Environment/CityHeroPocket/Materials/DB_CityHeroPocket_Asphalt.mat";
        internal const string SidewalkMaterialPath =
            "Assets/_Game/Art/Environment/CityHeroPocket/Materials/DB_CityHeroPocket_Sidewalk.mat";
        internal static readonly Color AsphaltColor =
            new(0.075f, 0.085f, 0.105f, 1f);
        internal static readonly Color SidewalkColor =
            new(0.31f, 0.32f, 0.33f, 1f);
        internal const float AsphaltMetallic = 0.03f;
        internal const float AsphaltSmoothness = 0.22f;
        internal const float SidewalkMetallic = 0f;
        internal const float SidewalkSmoothness = 0.28f;

        private const string TokyoPrefabRoot = TokyoRoot + "/Prefabs";

        public static readonly string[] RequiredTokyoPrefabPaths =
        {
            TokyoPrefabRoot + "/BG_House_05.prefab",
            TokyoPrefabRoot + "/Decals/Crossroad_02_Marking.prefab",
            TokyoPrefabRoot + "/Decals/Pedestrian_Crossing_01_Marking.prefab",
            TokyoPrefabRoot + "/Street/Traffic_Light_01.prefab",
            TokyoPrefabRoot + "/Street/Traffic_Light_Pedestrian_02.prefab",
            TokyoPrefabRoot + "/Street/Kerb_Stone_5m_01.prefab",
            TokyoPrefabRoot + "/Street/Kerb_Stone_Angle_01.prefab",
            TokyoPrefabRoot + "/Street/Step_Corner_5m_02.prefab",
            TokyoPrefabRoot + "/House/Interior/Wall_4m_01.prefab",
            TokyoPrefabRoot + "/House/Interior/Showcase_Store_01.prefab",
            TokyoPrefabRoot + "/House/Interior/Windows_V01/Wall_Windows_01.3.prefab",
            TokyoPrefabRoot + "/House/Balcony_02.prefab",
            TokyoPrefabRoot + "/House/External_Staircase_01.prefab",
            TokyoPrefabRoot + "/House/Window_Blinds_03.prefab",
            TokyoPrefabRoot + "/House/Visor_01.prefab",
            TokyoPrefabRoot + "/Environment/Tiers_Conditioners.prefab",
            TokyoPrefabRoot + "/Environment/Vending_Machine_01.prefab",
            TokyoPrefabRoot + "/Environment/Signboard_05.prefab",
            TokyoPrefabRoot + "/Environment/Bicycle_03.prefab",
            TokyoPrefabRoot + "/Environment/Mini_Truck.prefab",
            TokyoPrefabRoot + "/Street/Electric_Post_Big_01.prefab",
            TokyoPrefabRoot + "/Street/Wires_10m_01.prefab",
            TokyoPrefabRoot + "/Street/Wires_10m_02.prefab",
            TokyoPrefabRoot + "/Street/Wires_10m_03.prefab",
        };

        public const int TokyoModuleInstanceCount = 69;
        public const int TokyoModuleLod0RendererSlots = 80;
        public const int ProductLod0RendererSlots = 84;
        public const string LayoutRecipeSchema =
            "dimension-brawl-city-hero-pocket-layout/v1";
        public const string LayoutBoundsEvidenceSha256 =
            "597FE19CBAECB15A0487138704C7239A6A5953EF8DFFD77E6B88256FE0983909";
        public const string LayoutRecipeJsonSha256 =
            "79F1AE7CB76DDFDB751FA99B19453F60CDEE48DED8C76AF67F28E1B96A047552";
        public const string TokyoModuleGoldenSha256 =
            "03C8FD3B04AB2C4E72637F0391E0ED3F65FEF5837690DAF00D223B59DDA9CE16";
        public const string BoundaryLayerDeviation =
            "CITY-GATE-01 uses built-in Ignore Raycast layer 2 for isolated lane boundaries; " +
            "it intentionally does not mutate the shared TagManager to add PlayerBoundary.";
        public const string RendererBudgetCorrection =
            "Showcase_Store_01 owns one LOD0 renderer plus one renderer outside its LODGroup; " +
            "three placements correct the imported Tokyo total from 77 to 80, plus 4 surfaces.";

        public static readonly Vector3 PlayerPosition = new(-1.2f, 0f, -6.4f);
        public static readonly Vector3 EnemyPosition = new(0.4f, 0f, 5.6f);
        public static readonly Vector3 CameraPosition = new(-0.35f, 2.35f, -10.2f);
        public static readonly Vector3 CameraLookAt = new(-0.2f, 1.15f, 1.7f);
        public static readonly Vector3 ExitTriggerPosition = new(0f, 1f, 7.6f);
        public static readonly Vector3 ExitTriggerSize = new(10.8f, 2f, 0.6f);
        public static readonly Vector3 ExitTriggerCenter = new(0f, 0.05f, 0f);
        public static readonly Vector3 TransitionFocusPosition = new(0f, 2.8f, 10.55f);
        public static readonly Vector3 ExitPortalEuler = new(90f, 0f, 0f);
        public static readonly Vector3 ExitPortalAuthoredScale = new(0.92f, 0.92f, 0.92f);
        public static readonly Color ExitCoverColor = new(0.84f, 0.97f, 1f, 1f);
        public static readonly Vector3 DodgeBeatAnchorPosition = new(2.1f, 0f, 6.55f);
        public static readonly Vector3 ReserveEnemyAnchorPosition = new(-2.65f, 0f, 8.25f);

        private static readonly string[] ObsoletePlayerChildNames =
        {
            "CombatGirlPlaceholderBody",
            "ShortComboSwordProxy",
            "CombatGirlSwordShield_PlayerVisual",
            "BossBarrageLaneReview_MeleeWeapons_CombatGirlSwordShield",
        };

        // Generated from CITY_HERO_POCKET_LAYOUT_RECIPE_20260815.json and
        // cross-checked against exact Unity 6000.3.5f2 combined Renderer bounds.
        // Root transforms are authoritative; do not ground or fit these instances.
        private static readonly ModuleSpec[] TokyoModuleSpecs =
        {
            new("BG_L_00", TokyoPrefabRoot + "/BG_House_05.prefab", new Vector3(-7.66f, 0f, -8.75f), Vector3.zero, Vector3.one),
            new("BG_L_01", TokyoPrefabRoot + "/BG_House_05.prefab", new Vector3(-7.48f, 0f, -2.55f), Vector3.zero, Vector3.one),
            new("BG_L_02", TokyoPrefabRoot + "/BG_House_05.prefab", new Vector3(-7.72f, 0f, 3.85f), Vector3.zero, Vector3.one),
            new("BG_L_03", TokyoPrefabRoot + "/BG_House_05.prefab", new Vector3(-7.55f, 0f, 10.2f), Vector3.zero, Vector3.one),
            new("BG_R_00", TokyoPrefabRoot + "/BG_House_05.prefab", new Vector3(7.58f, 0f, -10.15f), new Vector3(0f, 180f, 0f), Vector3.one),
            new("BG_R_01", TokyoPrefabRoot + "/BG_House_05.prefab", new Vector3(7.42f, 0f, -3.8f), new Vector3(0f, 180f, 0f), Vector3.one),
            new("BG_R_02", TokyoPrefabRoot + "/BG_House_05.prefab", new Vector3(7.72f, 0f, 2.55f), new Vector3(0f, 180f, 0f), Vector3.one),
            new("BG_R_03", TokyoPrefabRoot + "/BG_House_05.prefab", new Vector3(7.5f, 0f, 8.9f), new Vector3(0f, 180f, 0f), Vector3.one),
            new("BG_END_L", TokyoPrefabRoot + "/BG_House_05.prefab", new Vector3(-0.4f, 0f, 11.4f), new Vector3(0f, 90f, 0f), Vector3.one),
            new("BG_END_R", TokyoPrefabRoot + "/BG_House_05.prefab", new Vector3(5.95f, 0.22f, 11.65f), new Vector3(0f, 90f, 0f), new Vector3(1.03f, 1.03f, 1.03f)),
            new("FAC_L_SHOW_N", TokyoPrefabRoot + "/House/Interior/Showcase_Store_01.prefab", new Vector3(-7.25f, 0f, 8.65f), Vector3.zero, Vector3.one),
            new("FAC_L_WIN_M", TokyoPrefabRoot + "/House/Interior/Windows_V01/Wall_Windows_01.3.prefab", new Vector3(-7.15f, 0f, 4.65f), Vector3.zero, Vector3.one),
            new("FAC_L_WALL_M", TokyoPrefabRoot + "/House/Interior/Wall_4m_01.prefab", new Vector3(-7.15f, 0f, 0.65f), Vector3.zero, Vector3.one),
            new("FAC_L_SHOW_S", TokyoPrefabRoot + "/House/Interior/Showcase_Store_01.prefab", new Vector3(-7.25f, 0f, -3.35f), Vector3.zero, Vector3.one),
            new("FAC_L_WIN_S", TokyoPrefabRoot + "/House/Interior/Windows_V01/Wall_Windows_01.3.prefab", new Vector3(-7.15f, 0f, -7.35f), Vector3.zero, Vector3.one),
            new("FAC_R_WIN_S", TokyoPrefabRoot + "/House/Interior/Windows_V01/Wall_Windows_01.3.prefab", new Vector3(7.15f, 0f, -8.65f), new Vector3(0f, 180f, 0f), Vector3.one),
            new("FAC_R_WALL_S", TokyoPrefabRoot + "/House/Interior/Wall_4m_01.prefab", new Vector3(7.15f, 0f, -4.65f), new Vector3(0f, 180f, 0f), Vector3.one),
            new("FAC_R_SHOW_M", TokyoPrefabRoot + "/House/Interior/Showcase_Store_01.prefab", new Vector3(7.25f, 0f, -0.65f), new Vector3(0f, 180f, 0f), Vector3.one),
            new("FAC_R_WIN_M", TokyoPrefabRoot + "/House/Interior/Windows_V01/Wall_Windows_01.3.prefab", new Vector3(7.15f, 0f, 3.35f), new Vector3(0f, 180f, 0f), Vector3.one),
            new("FAC_R_WALL_N", TokyoPrefabRoot + "/House/Interior/Wall_4m_01.prefab", new Vector3(7.15f, 0f, 7.35f), new Vector3(0f, 180f, 0f), Vector3.one),
            new("DEC_BALCONY", TokyoPrefabRoot + "/House/Balcony_02.prefab", new Vector3(-7.35f, 3.65f, 4.6f), Vector3.zero, Vector3.one),
            new("DEC_STAIR_END", TokyoPrefabRoot + "/House/External_Staircase_01.prefab", new Vector3(5f, 0f, 10.8f), new Vector3(0f, -90f, 0f), Vector3.one),
            new("DEC_BLINDS", TokyoPrefabRoot + "/House/Window_Blinds_03.prefab", new Vector3(-7.05f, 3.3f, -5.3f), Vector3.zero, Vector3.one),
            new("DEC_VISOR", TokyoPrefabRoot + "/House/Visor_01.prefab", new Vector3(-7.65f, 2.35f, 0.6f), Vector3.zero, Vector3.one),
            new("DEC_AC", TokyoPrefabRoot + "/Environment/Tiers_Conditioners.prefab", new Vector3(7.05f, 0f, 5.15f), new Vector3(0f, 180f, 0f), Vector3.one),
            new("PROP_VENDING", TokyoPrefabRoot + "/Environment/Vending_Machine_01.prefab", new Vector3(6.9f, 0f, -2f), new Vector3(0f, 180f, 0f), Vector3.one),
            new("PROP_SIGN", TokyoPrefabRoot + "/Environment/Signboard_05.prefab", new Vector3(-6.75f, 0f, 1.1f), Vector3.zero, Vector3.one),
            new("PROP_BICYCLE", TokyoPrefabRoot + "/Environment/Bicycle_03.prefab", new Vector3(6.55f, 0f, -5.2f), Vector3.zero, Vector3.one),
            new("PROP_TRUCK", TokyoPrefabRoot + "/Environment/Mini_Truck.prefab", new Vector3(-7.25f, 0f, -6f), new Vector3(0f, 90f, 0f), Vector3.one),
            new("POST_L_S", TokyoPrefabRoot + "/Street/Electric_Post_Big_01.prefab", new Vector3(-6.95f, 0f, -5.45f), Vector3.zero, Vector3.one),
            new("POST_L_N", TokyoPrefabRoot + "/Street/Electric_Post_Big_01.prefab", new Vector3(-6.95f, 0f, 4.45f), Vector3.zero, Vector3.one),
            new("POST_R_S", TokyoPrefabRoot + "/Street/Electric_Post_Big_01.prefab", new Vector3(6.95f, 0f, -4f), new Vector3(0f, 180f, 0f), Vector3.one),
            new("POST_R_N", TokyoPrefabRoot + "/Street/Electric_Post_Big_01.prefab", new Vector3(6.95f, 0f, 5.95f), new Vector3(0f, 180f, 0f), Vector3.one),
            new("WIRE_L_HIGH", TokyoPrefabRoot + "/Street/Wires_10m_01.prefab", new Vector3(-6.95f, 0f, 4.45f), new Vector3(0f, -90f, 0f), Vector3.one),
            new("WIRE_L_MID", TokyoPrefabRoot + "/Street/Wires_10m_02.prefab", new Vector3(-6.95f, 0f, 4.45f), new Vector3(0f, -90f, 0f), Vector3.one),
            new("WIRE_L_LOW", TokyoPrefabRoot + "/Street/Wires_10m_03.prefab", new Vector3(-6.95f, 6.63f, 4.45f), new Vector3(0f, -90f, 0f), Vector3.one),
            new("WIRE_R_MID", TokyoPrefabRoot + "/Street/Wires_10m_02.prefab", new Vector3(6.95f, 0f, 5.95f), new Vector3(0f, -90f, 0f), Vector3.one),
            new("WIRE_CROSS", TokyoPrefabRoot + "/Street/Wires_10m_03.prefab", new Vector3(6.4f, 7.95f, 5.95f), Vector3.zero, new Vector3(1.3f, 1f, 1f)),
            new("SIGNAL_L", TokyoPrefabRoot + "/Street/Traffic_Light_01.prefab", new Vector3(-6.9f, 0f, 8.75f), new Vector3(0f, 180f, 0f), Vector3.one),
            new("SIGNAL_R", TokyoPrefabRoot + "/Street/Traffic_Light_01.prefab", new Vector3(6.9f, 0f, 8.75f), Vector3.zero, Vector3.one),
            new("PED_SIGNAL_L", TokyoPrefabRoot + "/Street/Traffic_Light_Pedestrian_02.prefab", new Vector3(-6.96f, 3.2f, 8.45f), new Vector3(0f, 180f, 0f), Vector3.one),
            new("PED_SIGNAL_R", TokyoPrefabRoot + "/Street/Traffic_Light_Pedestrian_02.prefab", new Vector3(6.96f, 3.2f, 8.45f), Vector3.zero, Vector3.one),
            new("KERB_L_00", TokyoPrefabRoot + "/Street/Kerb_Stone_5m_01.prefab", new Vector3(-6.25f, 0f, -10f), new Vector3(0f, 180f, 0f), Vector3.one),
            new("KERB_L_01", TokyoPrefabRoot + "/Street/Kerb_Stone_5m_01.prefab", new Vector3(-6.25f, 0f, -7.5f), new Vector3(0f, 180f, 0f), Vector3.one),
            new("KERB_L_02", TokyoPrefabRoot + "/Street/Kerb_Stone_5m_01.prefab", new Vector3(-6.25f, 0f, -5f), new Vector3(0f, 180f, 0f), Vector3.one),
            new("KERB_L_03", TokyoPrefabRoot + "/Street/Kerb_Stone_5m_01.prefab", new Vector3(-6.25f, 0f, -2.5f), new Vector3(0f, 180f, 0f), Vector3.one),
            new("KERB_L_04", TokyoPrefabRoot + "/Street/Kerb_Stone_5m_01.prefab", new Vector3(-6.25f, 0f, 0f), new Vector3(0f, 180f, 0f), Vector3.one),
            new("KERB_L_05", TokyoPrefabRoot + "/Street/Kerb_Stone_5m_01.prefab", new Vector3(-6.25f, 0f, 2.5f), new Vector3(0f, 180f, 0f), Vector3.one),
            new("KERB_L_06", TokyoPrefabRoot + "/Street/Kerb_Stone_5m_01.prefab", new Vector3(-6.25f, 0f, 5f), new Vector3(0f, 180f, 0f), Vector3.one),
            new("KERB_L_07", TokyoPrefabRoot + "/Street/Kerb_Stone_5m_01.prefab", new Vector3(-6.25f, 0f, 7.5f), new Vector3(0f, 180f, 0f), Vector3.one),
            new("KERB_R_00", TokyoPrefabRoot + "/Street/Kerb_Stone_5m_01.prefab", new Vector3(6.25f, 0f, 10f), Vector3.zero, Vector3.one),
            new("KERB_R_01", TokyoPrefabRoot + "/Street/Kerb_Stone_5m_01.prefab", new Vector3(6.25f, 0f, 7.5f), Vector3.zero, Vector3.one),
            new("KERB_R_02", TokyoPrefabRoot + "/Street/Kerb_Stone_5m_01.prefab", new Vector3(6.25f, 0f, 5f), Vector3.zero, Vector3.one),
            new("KERB_R_03", TokyoPrefabRoot + "/Street/Kerb_Stone_5m_01.prefab", new Vector3(6.25f, 0f, 2.5f), Vector3.zero, Vector3.one),
            new("KERB_R_04", TokyoPrefabRoot + "/Street/Kerb_Stone_5m_01.prefab", new Vector3(6.25f, 0f, 0f), Vector3.zero, Vector3.one),
            new("KERB_R_05", TokyoPrefabRoot + "/Street/Kerb_Stone_5m_01.prefab", new Vector3(6.25f, 0f, -2.5f), Vector3.zero, Vector3.one),
            new("KERB_R_06", TokyoPrefabRoot + "/Street/Kerb_Stone_5m_01.prefab", new Vector3(6.25f, 0f, -5f), Vector3.zero, Vector3.one),
            new("KERB_R_07", TokyoPrefabRoot + "/Street/Kerb_Stone_5m_01.prefab", new Vector3(6.25f, 0f, -7.5f), Vector3.zero, Vector3.one),
            new("KERB_ANGLE_L_N", TokyoPrefabRoot + "/Street/Kerb_Stone_Angle_01.prefab", new Vector3(-6.25f, 0f, 11.25f), new Vector3(0f, 180f, 0f), Vector3.one),
            new("KERB_ANGLE_R_N", TokyoPrefabRoot + "/Street/Kerb_Stone_Angle_01.prefab", new Vector3(6.25f, 0f, 11.25f), new Vector3(0f, 90f, 0f), Vector3.one),
            new("STEP_L_S", TokyoPrefabRoot + "/Street/Step_Corner_5m_02.prefab", new Vector3(-6.25f, 0f, -9.75f), new Vector3(0f, 180f, 0f), Vector3.one),
            new("STEP_R_S", TokyoPrefabRoot + "/Street/Step_Corner_5m_02.prefab", new Vector3(6.25f, 0f, -9.75f), Vector3.zero, Vector3.one),
            new("ROAD_MARK_L", TokyoPrefabRoot + "/Decals/Crossroad_02_Marking.prefab", new Vector3(-2.1f, 0.006f, 0.8f), new Vector3(0f, 90f, 0f), Vector3.one),
            new("ROAD_MARK_R", TokyoPrefabRoot + "/Decals/Crossroad_02_Marking.prefab", new Vector3(2.1f, 0.006f, 0.8f), new Vector3(0f, 90f, 0f), Vector3.one),
            new("CROSSWALK_00", TokyoPrefabRoot + "/Decals/Pedestrian_Crossing_01_Marking.prefab", new Vector3(-4.72f, 0.006f, 7.2f), Vector3.zero, new Vector3(1f, 1f, 0.92446f)),
            new("CROSSWALK_01", TokyoPrefabRoot + "/Decals/Pedestrian_Crossing_01_Marking.prefab", new Vector3(-2.36f, 0.006f, 7.2f), Vector3.zero, new Vector3(1f, 1f, 0.92446f)),
            new("CROSSWALK_02", TokyoPrefabRoot + "/Decals/Pedestrian_Crossing_01_Marking.prefab", new Vector3(0f, 0.006f, 7.2f), Vector3.zero, new Vector3(1f, 1f, 0.92446f)),
            new("CROSSWALK_03", TokyoPrefabRoot + "/Decals/Pedestrian_Crossing_01_Marking.prefab", new Vector3(2.36f, 0.006f, 7.2f), Vector3.zero, new Vector3(1f, 1f, 0.92446f)),
            new("CROSSWALK_04", TokyoPrefabRoot + "/Decals/Pedestrian_Crossing_01_Marking.prefab", new Vector3(4.72f, 0.006f, 7.2f), Vector3.zero, new Vector3(1f, 1f, 0.92446f)),
        };

        internal static IReadOnlyList<ModuleSpec> ReviewedTokyoModuleSpecs =>
            TokyoModuleSpecs;

        internal static string ComputeTokyoModuleGoldenSha256()
        {
            var builder = new StringBuilder(TokyoModuleSpecs.Length * 128);
            for (int i = 0; i < TokyoModuleSpecs.Length; i++)
            {
                ModuleSpec spec = TokyoModuleSpecs[i];
                builder.Append(spec.Id)
                    .Append('|')
                    .Append(System.IO.Path.GetFileNameWithoutExtension(spec.PrefabPath))
                    .Append('|');
                AppendGoldenFloat(builder, spec.Position.x);
                AppendGoldenFloat(builder, spec.Position.y);
                AppendGoldenFloat(builder, spec.Position.z);
                AppendGoldenFloat(builder, spec.Euler.x);
                AppendGoldenFloat(builder, spec.Euler.y);
                AppendGoldenFloat(builder, spec.Euler.z);
                AppendGoldenFloat(builder, spec.Scale.x);
                AppendGoldenFloat(builder, spec.Scale.y);
                builder.Append(spec.Scale.z.ToString("R", CultureInfo.InvariantCulture))
                    .Append('\n');
            }

            using SHA256 sha = SHA256.Create();
            byte[] digest = sha.ComputeHash(
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)
                    .GetBytes(builder.ToString()));
            return BitConverter.ToString(digest).Replace("-", string.Empty);
        }

        private static void AppendGoldenFloat(StringBuilder builder, float value)
        {
            builder.Append(value.ToString("R", CultureInfo.InvariantCulture)).Append(',');
        }

        [MenuItem("DimensionBrawl/Setup/PV Build City Hero Pocket Stage")]
        public static void BuildFromMenu()
        {
            Build();
            Debug.Log("[CityHeroPocketSceneSetup] SCENE_SETUP_PASS");
        }

        public static void RunBatchSetup()
        {
            try
            {
                Build();
                Debug.Log("[CityHeroPocketSceneSetup] BATCH_SETUP_PASS");
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Debug.LogError("[CityHeroPocketSceneSetup] BATCH_SETUP_FAIL");
                EditorApplication.Exit(1);
            }
        }

        public static void Build()
        {
            Require(!EditorApplication.isPlayingOrWillChangePlaymode,
                "City Hero Pocket authoring cannot run while entering or in Play Mode.");
            RefuseDirtyOpenScenes();
            RequireSourceAssets();
            EnsureOutputFolders();

            SceneSetup[] previousSetup = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                EnsureCompactInoriRangedPlayerPrefab();

                Scene scene = EditorSceneManager.NewScene(
                    NewSceneSetup.EmptyScene,
                    NewSceneMode.Single);

                Material asphalt = EnsureOwnedLitMaterial(
                    AsphaltMaterialPath,
                    AsphaltColor,
                    metallic: AsphaltMetallic,
                    smoothness: AsphaltSmoothness);
                Material sidewalk = EnsureOwnedLitMaterial(
                    SidewalkMaterialPath,
                    SidewalkColor,
                    metallic: SidewalkMetallic,
                    smoothness: SidewalkSmoothness);
                VolumeProfile look = EnsureCityLookProfile();

                GameObject stageRoot = CreateSceneRoot(scene, StageRootName, active: false);
                CreateCityMap(scene, stageRoot.transform, asphalt, sidewalk);
                CreateLighting(scene);
                CreateGlobalVolume(scene, look);
                CreateEventSystem(scene);

                Transform cameraHeading = CreateSceneRoot(
                    scene,
                    "CityHeroPocket_CameraHeading",
                    active: true).transform;
                cameraHeading.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                CameraPackage camera = CreateCamera(scene, cameraHeading);

                PlayerPackage player = CreatePlayer(scene, camera.Camera);
                EnemyPackage enemy = CreateEnemy(scene, player.Health);
                ConfigureCombatCamera(camera, cameraHeading, player, enemy);
                ConfigurePlayerForScene(player, enemy, camera);

                GameObject runtimeRoot = CreateSceneRoot(scene, RuntimeRootName, active: false);
                Transform playerProjectiles = CreateChild(
                    runtimeRoot.transform,
                    PlayerProjectileRootName).transform;
                Transform enemyProjectiles = CreateChild(
                    runtimeRoot.transform,
                    EnemyProjectileRootName).transform;
                ConfigureProjectileOwnership(player, enemy, playerProjectiles, enemyProjectiles);

                CombatEncounterController encounter =
                    runtimeRoot.AddComponent<CombatEncounterController>();
                encounter.ConfigureCombatants(player.Health, enemy.Health);
                encounter.ConfigureTerminalResolutionPolicy(true);

                HudPackage hud = CreateHud(scene);
                ConfigureHud(hud, encounter, player, enemy, camera.Controller);
                CreateExitTransition(
                    scene,
                    runtimeRoot.transform,
                    encounter,
                    player,
                    enemy,
                    hud);

                stageRoot.SetActive(true);
                player.Root.SetActive(true);
                enemy.Root.SetActive(true);
                runtimeRoot.SetActive(true);
                hud.Root.SetActive(true);

                EditorSceneManager.MarkSceneDirty(scene);
                Require(EditorSceneManager.SaveScene(scene, ScenePath),
                    $"Failed to save City Hero Pocket scene at '{ScenePath}'.");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                // Reopen from disk before validation so prefab overrides and scene-local
                // object references cannot false-pass through transient authoring state.
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                CityHeroPocketAuthoredPackValidator.ValidateLoadedScene(scene);
            }
            finally
            {
                if (previousSetup != null && previousSetup.Length > 0)
                {
                    EditorSceneManager.RestoreSceneManagerSetup(previousSetup);
                }
            }
        }

        private static void RequireSourceAssets()
        {
            LoadRequired<SceneAsset>(SourceStationScenePath);
            LoadRequired<RuntimeAnimatorController>(InoriAnimatorControllerPath);
            LoadRequired<PlayerActionProfile>(DodgeProfilePath);
            LoadRequired<GameObject>(PlayerProjectilePrefabPath);
            LoadRequired<GameObject>(EnemyPrefabPath);
            LoadRequired<GameObject>(HudPrefabPath);
            LoadRequired<GameObject>(ExitPortalPrefabPath);
            for (int i = 0; i < RequiredTokyoPrefabPaths.Length; i++)
            {
                LoadRequired<GameObject>(RequiredTokyoPrefabPaths[i]);
            }
        }

        private static void EnsureOutputFolders()
        {
            EnsureFolderForAsset(PlayerPrefabPath);
            EnsureFolderForAsset(CityLookProfilePath);
            EnsureFolderForAsset(AsphaltMaterialPath);
            EnsureFolderForAsset(ScenePath);
        }

        private static void EnsureCompactInoriRangedPlayerPrefab()
        {
            Scene sourceScene = EditorSceneManager.OpenScene(
                SourceStationScenePath,
                OpenSceneMode.Single);
            GameObject source = FindUniqueSceneObject(sourceScene, SourceStationPlayerName);
            GameObject candidate = UnityEngine.Object.Instantiate(source);
            candidate.name = "PF_Player_Inori_RangedActionFoundation";
            candidate.SetActive(false);

            try
            {
                UnpackNestedPrefabInstances(candidate);
                for (int i = 0; i < ObsoletePlayerChildNames.Length; i++)
                {
                    DestroyDescendantIfPresent(candidate.transform, ObsoletePlayerChildNames[i]);
                }

                RemoveComponents<SummonEnergyLadder>(candidate);
                RemoveComponents<PlayerSummonSlot1Action>(candidate);
                RemoveComponents<PlayerSupportSummonSlotAction>(candidate);
                RemoveComponents<PlayerSkill1Action>(candidate);
                RemoveComponents<PlayerSkill1LaserSweepAction>(candidate);
                RemoveComponents<SummonEnergyVfxCuePresenter>(candidate);
                RemoveComponentsByNamespace(candidate, "MagicaCloth2");

                StripSceneExternalObjectReferences(candidate);
                ConfigureCompactPlayerInternalContract(candidate);

                candidate.transform.localPosition = Vector3.zero;
                candidate.transform.localRotation = Quaternion.identity;
                candidate.transform.localScale = Vector3.one;
                candidate.SetActive(true);

                GameObject saved = PrefabUtility.SaveAsPrefabAsset(candidate, PlayerPrefabPath);
                Require(saved != null,
                    $"Failed to save compact Inori ranged player prefab at '{PlayerPrefabPath}'.");
                AssetDatabase.ImportAsset(
                    PlayerPrefabPath,
                    ImportAssetOptions.ForceSynchronousImport
                    | ImportAssetOptions.ForceUpdate);
                CityHeroPocketAuthoredPackValidator.ValidatePlayerPrefab();
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(candidate);
            }
        }

        private static void ConfigureCompactPlayerInternalContract(GameObject player)
        {
            player.tag = "Player";
            Transform visual = FindDescendant(player.transform, SourceInoriVisualName);
            Require(visual != null,
                $"Station player source is missing the canonical Inori visual '{SourceInoriVisualName}'.");
            visual.name = "Inori_RangedVisual";

            Animator animator = RequireSingle<Animator>(visual.gameObject);
            animator.runtimeAnimatorController =
                LoadRequired<RuntimeAnimatorController>(InoriAnimatorControllerPath);
            animator.applyRootMotion = false;

            CombatHealth health = RequireSingle<CombatHealth>(player);
            CharacterController capsule = RequireSingle<CharacterController>(player);
            PlayerMovementController movement = RequireSingle<PlayerMovementController>(player);
            PlayerCombatTargetSelector selector = RequireSingle<PlayerCombatTargetSelector>(player);
            PlayerActionController action = RequireSingle<PlayerActionController>(player);
            PlayerCombatModeController mode = RequireSingle<PlayerCombatModeController>(player);
            PlayerRangedAimController aim = RequireSingle<PlayerRangedAimController>(player);
            PlayerRangedBasicAttackAction ranged =
                RequireSingle<PlayerRangedBasicAttackAction>(player);
            PlayerLockTargetController lockTarget =
                RequireSingle<PlayerLockTargetController>(player);
            RifleGirlNativeGameplayAnimatorBridge nativeBridge =
                RequireSingle<RifleGirlNativeGameplayAnimatorBridge>(player);
            PlayerDodgeFeedback dodgeFeedback =
                RequireSingle<PlayerDodgeFeedback>(player);
            CombatHitFeedback hitFeedback =
                RequireSingle<CombatHitFeedback>(player);
            Transform rangedWeapon = FindDescendant(
                player.transform,
                "BossBarrageLaneReview_RangedWeapon_Rifle");
            Require(rangedWeapon != null,
                "Station player source is missing the reviewed ranged rifle root.");
            Renderer[] visualRenderers = visual.GetComponentsInChildren<Renderer>(true);
            Require(visualRenderers.Length > 0,
                "Canonical Inori visual has no renderers for dodge/hit feedback.");

            SerializedObject serializedDodgeFeedback = new(dodgeFeedback);
            SetObjectArray(serializedDodgeFeedback, "targetRenderers", visualRenderers);
            serializedDodgeFeedback.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject serializedHitFeedback = new(hitFeedback);
            SetObjectArray(serializedHitFeedback, "flashRenderers", visualRenderers);
            serializedHitFeedback.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject serializedHealth = new(health);
            RequireProperty(serializedHealth, "team").intValue = (int)DamageTeam.Player;
            SetFloat(serializedHealth, "maxHealth", 480f);
            serializedHealth.ApplyModifiedPropertiesWithoutUndo();
            capsule.radius = 0.45f;
            capsule.height = 1.8f;
            capsule.center = new Vector3(0f, 0.9f, 0f);
            movement.SetLaneConstraintEnabled(false);
            action.SetActionProfile(LoadRequired<PlayerActionProfile>(DodgeProfilePath));
            aim.SetAnimator(animator);
            ranged.SetAnimator(animator);
            ranged.SetLockTargetController(lockTarget);
            mode.SetRangedMode();

            SerializedObject serializedMovement = new(movement);
            SetObject(serializedMovement, "referenceCamera", null);
            SetObject(serializedMovement, "laneSpace", null);
            SetObject(serializedMovement, "animator", null);
            SetBool(serializedMovement, "cameraRelativeMovement", true);
            serializedMovement.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject serializedAction = new(action);
            SetObject(serializedAction, "movement", movement);
            SetObject(serializedAction, "health", health);
            SetObject(serializedAction, "targetSelector", selector);
            SetObject(serializedAction, "combatModeController", mode);
            SetObject(serializedAction, "animator", null);
            SetObject(serializedAction, "actionProfile",
                LoadRequired<PlayerActionProfile>(DodgeProfilePath));
            serializedAction.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject serializedMode = new(mode);
            SetObject(serializedMode, "actionController", action);
            SetObject(serializedMode, "movementController", movement);
            SetObject(serializedMode, "rangedAimController", aim);
            SetObject(serializedMode, "rangedBasicAttackAction", ranged);
            SetObject(serializedMode, "combatModeSwapAction", null);
            SetBool(serializedMode, "useKeyboardWhenActionMissing", false);
            SetObject(serializedMode, "rangedVisualRoot", visual.gameObject);
            SetObject(serializedMode, "rangedWeaponRoot", rangedWeapon.gameObject);
            SetObject(serializedMode, "rangedAnimator", animator);
            SetObject(serializedMode, "rangedAnimatorController",
                LoadRequired<RuntimeAnimatorController>(InoriAnimatorControllerPath));
            SetBool(serializedMode, "routeAnimatorsByMode", true);
            SetBool(serializedMode, "rangedAnimatorUsesExternalPresentationBridge", true);
            SetBool(serializedMode, "useSingleCharacterVisual", true);
            SetObject(serializedMode, "rangedActionProfile",
                LoadRequired<PlayerActionProfile>(DodgeProfilePath));
            SetObject(serializedMode, "meleeVisualRoot", null);
            SetObject(serializedMode, "meleeWeaponRoot", null);
            SetObject(serializedMode, "meleeAnimator", null);
            SetObject(serializedMode, "meleeAnimatorController", null);
            SetObject(serializedMode, "meleeActionProfile", null);
            RequireProperty(serializedMode, "startingMode").intValue =
                (int)PlayerCombatMode.Ranged;
            serializedMode.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject serializedNativeBridge = new(nativeBridge);
            SetObject(serializedNativeBridge, "animator", animator);
            SetObject(serializedNativeBridge, "movement", movement);
            SetObject(serializedNativeBridge, "actionController", action);
            SetObject(serializedNativeBridge, "combatModeController", mode);
            SetObject(serializedNativeBridge, "rangedAimController", aim);
            SetObject(serializedNativeBridge, "rangedBasicAttackAction", ranged);
            serializedNativeBridge.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject serializedSelector = new(selector);
            SetObject(serializedSelector, "selfHealth", health);
            SetObject(serializedSelector, "selectionOrigin", player.transform);
            SetObject(serializedSelector, "viewReference", null);
            SetObjectArray(serializedSelector, "targetCandidates",
                Array.Empty<UnityEngine.Object>());
            serializedSelector.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject serializedRanged = new(ranged);
            SetObject(serializedRanged, "cameraController", null);
            SetObject(serializedRanged, "projectileRoot", null);
            SetString(serializedRanged, "fireTrigger", string.Empty);
            serializedRanged.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject serializedAim = new(aim);
            SetObject(serializedAim, "cameraController", null);
            SetString(serializedAim, "aimingParameter", string.Empty);
            serializedAim.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject serializedLock = new(lockTarget);
            SetObject(serializedLock, "cameraController", null);
            serializedLock.ApplyModifiedPropertiesWithoutUndo();

            RebindObjectReferenceByPropertyName(
                player,
                "cameraController",
                null,
                applyToInactive: true);
            EditorUtility.SetDirty(player);
        }

        private static void UnpackNestedPrefabInstances(GameObject root)
        {
            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
            var outermost = new List<GameObject>();
            for (int i = 0; i < transforms.Length; i++)
            {
                GameObject candidate = transforms[i].gameObject;
                if (candidate != root
                    && PrefabUtility.IsOutermostPrefabInstanceRoot(candidate))
                {
                    outermost.Add(candidate);
                }
            }

            for (int i = 0; i < outermost.Count; i++)
            {
                GameObject candidate = outermost[i];
                if (candidate != null && PrefabUtility.IsPartOfPrefabInstance(candidate))
                {
                    PrefabUtility.UnpackPrefabInstance(
                        candidate,
                        PrefabUnpackMode.Completely,
                        InteractionMode.AutomatedAction);
                }
            }
        }

        private static void StripSceneExternalObjectReferences(GameObject root)
        {
            Component[] components = root.GetComponentsInChildren<Component>(true);
            for (int componentIndex = 0; componentIndex < components.Length; componentIndex++)
            {
                Component component = components[componentIndex];
                if (component == null)
                {
                    continue;
                }

                SerializedObject serialized = new(component);
                SerializedProperty property = serialized.GetIterator();
                bool changed = false;
                while (property.Next(enterChildren: true))
                {
                    if (property.propertyType != SerializedPropertyType.ObjectReference
                        || string.Equals(property.propertyPath, "m_Script", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    UnityEngine.Object referenced = property.objectReferenceValue;
                    if (IsExternalSceneObject(referenced, root.transform))
                    {
                        property.objectReferenceValue = null;
                        changed = true;
                    }
                }

                if (changed)
                {
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                }
            }
        }

        private static bool IsExternalSceneObject(
            UnityEngine.Object referenced,
            Transform ownedRoot)
        {
            GameObject referencedObject = referenced switch
            {
                GameObject gameObject => gameObject,
                Component component => component.gameObject,
                _ => null,
            };

            return referencedObject != null
                && referencedObject.scene.IsValid()
                && !referencedObject.transform.IsChildOf(ownedRoot);
        }

        private static void RemoveComponents<T>(GameObject root)
            where T : Component
        {
            T[] components = root.GetComponentsInChildren<T>(true);
            for (int i = 0; i < components.Length; i++)
            {
                UnityEngine.Object.DestroyImmediate(components[i]);
            }
        }

        private static void RemoveComponentsByNamespace(
            GameObject root,
            string namespacePrefix)
        {
            Component[] components = root.GetComponentsInChildren<Component>(true);
            for (int i = components.Length - 1; i >= 0; i--)
            {
                Component component = components[i];
                if (component == null)
                {
                    continue;
                }
                string componentNamespace = component.GetType().Namespace ?? string.Empty;
                if (string.Equals(
                        componentNamespace,
                        namespacePrefix,
                        StringComparison.Ordinal)
                    || componentNamespace.StartsWith(
                        namespacePrefix + ".",
                        StringComparison.Ordinal))
                {
                    UnityEngine.Object.DestroyImmediate(component);
                }
            }
        }

        private static void DestroyDescendantIfPresent(Transform root, string objectName)
        {
            Transform found = FindDescendant(root, objectName);
            if (found != null && found != root)
            {
                UnityEngine.Object.DestroyImmediate(found.gameObject);
            }
        }

        private static void CreateCityMap(
            Scene scene,
            Transform stageRoot,
            Material asphalt,
            Material sidewalk)
        {
            GameObject map = new(MapRootName);
            map.transform.SetParent(stageRoot, false);

            Transform authoredSurfaces = CreateChild(map.transform, "AuthoredSurfaces").transform;
            CreateSurface(
                authoredSurfaces,
                "Road_Asphalt",
                new Vector3(0f, -0.15f, 0f),
                new Vector3(12f, 0.3f, 20f),
                asphalt);
            CreateSurface(
                authoredSurfaces,
                "Sidewalk_West",
                new Vector3(-6.85f, -0.02f, 0f),
                new Vector3(1.3f, 0.16f, 20f),
                sidewalk);
            CreateSurface(
                authoredSurfaces,
                "Sidewalk_East",
                new Vector3(6.85f, -0.02f, 0f),
                new Vector3(1.3f, 0.16f, 20f),
                sidewalk);
            CreateSurface(
                authoredSurfaces,
                "EndPlatform_North",
                new Vector3(0f, -0.15f, 12.2f),
                new Vector3(12f, 0.3f, 4.4f),
                asphalt);

            Transform boundaries = CreateChild(map.transform, "CityCombatBoundaries").transform;
            CreateBoundary(boundaries, "Boundary_West", new Vector3(-6.15f, 1f, 0f),
                new Vector3(0.3f, 2f, 18.6f));
            CreateBoundary(boundaries, "Boundary_East", new Vector3(6.15f, 1f, 0f),
                new Vector3(0.3f, 2f, 18.6f));
            CreateBoundary(boundaries, "Boundary_South", new Vector3(0f, 1f, -9.15f),
                new Vector3(12.6f, 2f, 0.3f));
            CreateBoundary(boundaries, "Boundary_North", new Vector3(0f, 1f, 9.15f),
                new Vector3(12.6f, 2f, 0.3f));

            Transform modules = CreateChild(map.transform, "TokyoStreet_CuratedHeroBlock").transform;
            CreateTokyoModules(scene, modules);
        }

        private static void CreateTokyoModules(Scene scene, Transform parent)
        {
            Require(TokyoModuleSpecs.Length == TokyoModuleInstanceCount,
                $"Layout recipe requires {TokyoModuleInstanceCount} module instances; " +
                $"found {TokyoModuleSpecs.Length} specs.");
            for (int i = 0; i < TokyoModuleSpecs.Length; i++)
            {
                ModuleSpec spec = TokyoModuleSpecs[i];
                InstantiateModule(
                    scene,
                    spec.PrefabPath,
                    parent,
                    spec.Id,
                    spec.Position,
                    spec.Euler,
                    spec.Scale);
            }
        }

        private static GameObject InstantiateModule(
            Scene scene,
            string prefabPath,
            Transform parent,
            string name,
            Vector3 localPosition,
            Vector3 localEuler,
            Vector3 localScale)
        {
            GameObject prefab = LoadRequired<GameObject>(prefabPath);
            GameObject instance = PrefabUtility.InstantiatePrefab(prefab, scene) as GameObject;
            Require(instance != null, $"Failed to instantiate Tokyo module '{prefabPath}'.");
            instance.name = name;
            instance.transform.SetParent(parent, false);
            instance.transform.localPosition = localPosition;
            instance.transform.localRotation = Quaternion.Euler(localEuler);
            instance.transform.localScale = localScale;
            return instance;
        }

        private static void CreateSurface(
            Transform parent,
            string name,
            Vector3 localPosition,
            Vector3 localScale,
            Material material)
        {
            GameObject surface = GameObject.CreatePrimitive(PrimitiveType.Cube);
            surface.name = name;
            surface.transform.SetParent(parent, false);
            surface.transform.localPosition = localPosition;
            surface.transform.localScale = localScale;
            surface.GetComponent<MeshRenderer>().sharedMaterial = material;
        }

        private static void CreateBoundary(
            Transform parent,
            string name,
            Vector3 localPosition,
            Vector3 size)
        {
            GameObject boundary = CreateChild(parent, name);
            boundary.transform.localPosition = localPosition;
            // See BoundaryLayerDeviation. This keeps the product pocket isolated from
            // shared project settings while excluding capture/gameplay raycasts.
            boundary.layer = 2;
            BoxCollider collider = boundary.AddComponent<BoxCollider>();
            collider.size = size;
        }

        private static CameraPackage CreateCamera(Scene scene, Transform cameraHeading)
        {
            GameObject cameraObject = CreateSceneRoot(scene, "Main Camera", active: true);
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetPositionAndRotation(
                CameraPosition,
                Quaternion.LookRotation(CameraLookAt - CameraPosition, Vector3.up));

            Camera camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 52f;
            camera.nearClipPlane = 0.08f;
            camera.farClipPlane = 220f;
            camera.allowHDR = true;
            camera.allowMSAA = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.57f, 0.68f, 0.79f, 1f);
            cameraObject.AddComponent<AudioListener>();

            UniversalAdditionalCameraData cameraData =
                cameraObject.AddComponent<UniversalAdditionalCameraData>();
            cameraData.renderPostProcessing = true;
            cameraData.renderShadows = true;
            cameraData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            cameraData.antialiasingQuality = AntialiasingQuality.High;
            cameraData.stopNaN = true;
            cameraData.dithering = true;
            cameraData.volumeLayerMask = 1 << 0;

            ActionCameraController controller =
                cameraObject.AddComponent<ActionCameraController>();
            SerializedObject serialized = new(controller);
            SetVector3(serialized, "cameraOffset", new Vector3(0.85f, 1.25f, -3.8f));
            SetVector3(serialized, "lookOffset", new Vector3(0f, 1.1f, 0f));
            SetFloat(serialized, "followSmoothTime", 0.08f);
            SetFloat(serialized, "rotationSmooth", 18f);
            SetBool(serialized, "useFixedRearYaw", true);
            SetObject(serialized, "fixedRearYawReference", cameraHeading);
            // Keep the position rig anchored to player pivot + shoulder so the saved
            // G02 pose does not pop on the first LateUpdate, while retaining the
            // reviewed player-to-enemy look focus for rotation/framing.
            SetBool(serialized, "threatFocusAffectsCameraPosition", false);
            SetFloat(serialized, "threatBias", 0.67f);
            SetFloat(serialized, "maxThreatFocusOffset", 8.1f);
            SetFloat(serialized, "maxLeadFromPlayerSpeed", 0f);
            SetFloat(serialized, "liveFireFeedbackScale", 0.82f);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return new CameraPackage(camera, cameraData, controller);
        }

        private static PlayerPackage CreatePlayer(Scene scene, Camera camera)
        {
            GameObject prefab = LoadRequired<GameObject>(PlayerPrefabPath);
            GameObject root = PrefabUtility.InstantiatePrefab(prefab, scene) as GameObject;
            Require(root != null, "Failed to instantiate compact Inori ranged player prefab.");
            root.name = PlayerRootName;
            root.SetActive(false);
            root.transform.SetPositionAndRotation(PlayerPosition, Quaternion.identity);

            Animator animator = RequireSingle<Animator>(root);
            animator.runtimeAnimatorController =
                LoadRequired<RuntimeAnimatorController>(InoriAnimatorControllerPath);
            animator.applyRootMotion = false;

            return new PlayerPackage(
                root,
                RequireSingle<CombatHealth>(root),
                RequireSingle<PlayerMovementController>(root),
                RequireSingle<PlayerCombatTargetSelector>(root),
                RequireSingle<PlayerActionController>(root),
                RequireSingle<PlayerCombatModeController>(root),
                RequireSingle<PlayerRangedAimController>(root),
                RequireSingle<PlayerRangedBasicAttackAction>(root),
                RequireSingle<PlayerLockTargetController>(root),
                animator);
        }

        private static EnemyPackage CreateEnemy(Scene scene, CombatHealth playerHealth)
        {
            GameObject prefab = LoadRequired<GameObject>(EnemyPrefabPath);
            GameObject root = PrefabUtility.InstantiatePrefab(prefab, scene) as GameObject;
            Require(root != null, "Failed to instantiate reviewed RifleCrossfire enemy.");
            root.name = EnemyRootName;
            root.SetActive(false);
            root.transform.SetPositionAndRotation(
                EnemyPosition,
                Quaternion.Euler(0f, 180f, 0f));

            CombatHealth health = RequireSingle<CombatHealth>(root);
            BasicSoldierEnemy soldier = RequireSingle<BasicSoldierEnemy>(root);
            CombatTargetSensor sensor = RequireSingle<CombatTargetSensor>(root);
            BasicSoldierProjectileAttackDriver projectileDriver =
                RequireSingle<BasicSoldierProjectileAttackDriver>(root);

            SerializedObject serializedHealth = new(health);
            RequireProperty(serializedHealth, "team").intValue = (int)DamageTeam.Enemy;
            SetFloat(serializedHealth, "maxHealth", 90f);
            serializedHealth.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject serializedSensor = new(sensor);
            SetFloat(serializedSensor, "searchRadius", 24f);
            SetObjectArray(serializedSensor, "targetCandidates",
                new UnityEngine.Object[] { playerHealth });
            serializedSensor.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject serializedSoldier = new(soldier);
            SetObject(serializedSoldier, "target", playerHealth.transform);
            SetObject(serializedSoldier, "targetHealth", playerHealth);
            serializedSoldier.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.RecordPrefabInstancePropertyModifications(root.transform);
            return new EnemyPackage(root, health, soldier, sensor, projectileDriver);
        }

        private static void ConfigureCombatCamera(
            CameraPackage camera,
            Transform cameraHeading,
            PlayerPackage player,
            EnemyPackage enemy)
        {
            cameraHeading.rotation = Quaternion.identity;
            camera.Controller.ConfigureTargets(player.Root.transform, enemy.Root.transform);
            camera.Controller.CaptureBaseFieldOfViewFromControlledCamera();
        }

        private static void ConfigurePlayerForScene(
            PlayerPackage player,
            EnemyPackage enemy,
            CameraPackage camera)
        {
            player.Movement.SetLaneConstraintEnabled(false);
            player.Action.SetActionProfile(LoadRequired<PlayerActionProfile>(DodgeProfilePath));
            player.Mode.SetRangedMode();
            player.Aim.ConfigureReferences(
                player.Mode,
                camera.Controller,
                player.Animator,
                player.Movement);
            player.Ranged.ConfigureReferences(
                player.Mode,
                player.Aim,
                player.Movement,
                player.TargetSelector,
                player.Health,
                camera.Controller,
                player.Animator);
            player.Ranged.SetLockTargetController(player.LockTarget);
            player.LockTarget.ConfigureReferences(
                player.TargetSelector,
                player.Health,
                camera.Controller,
                player.Root.transform);

            SerializedObject serializedMovement = new(player.Movement);
            SetObject(serializedMovement, "referenceCamera", camera.Camera);
            SetObject(serializedMovement, "animator", null);
            SetObject(serializedMovement, "laneSpace", null);
            SetBool(serializedMovement, "cameraRelativeMovement", true);
            serializedMovement.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject serializedAction = new(player.Action);
            SetObject(serializedAction, "movement", player.Movement);
            SetObject(serializedAction, "health", player.Health);
            SetObject(serializedAction, "targetSelector", player.TargetSelector);
            SetObject(serializedAction, "combatModeController", player.Mode);
            SetObject(serializedAction, "animator", null);
            SetObject(serializedAction, "actionProfile",
                LoadRequired<PlayerActionProfile>(DodgeProfilePath));
            serializedAction.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject serializedSelector = new(player.TargetSelector);
            SetObject(serializedSelector, "selfHealth", player.Health);
            SetObject(serializedSelector, "selectionOrigin", player.Root.transform);
            SetObject(serializedSelector, "viewReference", camera.Camera.transform);
            SetObjectArray(serializedSelector, "targetCandidates",
                new UnityEngine.Object[] { enemy.Health });
            SetBool(serializedSelector, "includeActiveHostileSummons", true);
            serializedSelector.ApplyModifiedPropertiesWithoutUndo();
            player.TargetSelector.ConfigureTargetCandidates(new[] { enemy.Health }, refreshNow: false);

            RebindObjectReferenceByPropertyName(
                player.Root,
                "cameraController",
                camera.Controller,
                applyToInactive: true);
        }

        private static void ConfigureProjectileOwnership(
            PlayerPackage player,
            EnemyPackage enemy,
            Transform playerProjectiles,
            Transform enemyProjectiles)
        {
            GameObject projectilePrefab = LoadRequired<GameObject>(PlayerProjectilePrefabPath);
            LaneActionProjectile projectile = projectilePrefab.GetComponent<LaneActionProjectile>();
            Require(projectile != null,
                $"Player projectile prefab has no {nameof(LaneActionProjectile)} root component.");

            SerializedObject serializedRanged = new(player.Ranged);
            SetObject(serializedRanged, "projectilePrefab", projectile);
            SetObject(serializedRanged, "projectilePrefabObject", projectilePrefab);
            SetObject(serializedRanged, "projectileRoot", playerProjectiles);
            serializedRanged.ApplyModifiedPropertiesWithoutUndo();
            GameObject runtimeOwner = enemyProjectiles.parent.gameObject;
            CityHeroPocketEnemyProjectileRootBinder binder =
                runtimeOwner.AddComponent<CityHeroPocketEnemyProjectileRootBinder>();
            binder.Configure(enemy.ProjectileDriver, enemyProjectiles);
        }

        private static void CreateExitTransition(
            Scene scene,
            Transform runtimeRoot,
            CombatEncounterController encounter,
            PlayerPackage player,
            EnemyPackage enemy,
            HudPackage hud)
        {
            Transform transitionFocus = CreateChild(
                runtimeRoot,
                TransitionFocusName).transform;
            transitionFocus.localPosition = TransitionFocusPosition;

            Transform dodgeBeatAnchor = CreateChild(
                runtimeRoot,
                DodgeBeatAnchorName).transform;
            dodgeBeatAnchor.localPosition = DodgeBeatAnchorPosition;

            Transform reserveEnemyAnchor = CreateChild(
                runtimeRoot,
                ReserveEnemyAnchorName).transform;
            reserveEnemyAnchor.localPosition = ReserveEnemyAnchorPosition;

            GameObject portalPrefab = LoadRequired<GameObject>(ExitPortalPrefabPath);
            GameObject portal = PrefabUtility.InstantiatePrefab(portalPrefab, scene) as GameObject;
            Require(portal != null,
                "Failed to instantiate the promoted City exit portal prefab.");
            portal.name = ExitPortalRootName;
            portal.transform.SetParent(runtimeRoot, worldPositionStays: false);
            portal.transform.localPosition = TransitionFocusPosition;
            portal.transform.localRotation = Quaternion.Euler(ExitPortalEuler);
            portal.transform.localScale = ExitPortalAuthoredScale
                * CityHeroPocketExitTransitionController.InitialPortalScaleFactor;
            ParticleSystem[] particles = portal.GetComponentsInChildren<ParticleSystem>(true);
            Require(particles.Length > 0,
                "Promoted City exit portal contains no deterministic particle systems.");
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].useAutoRandomSeed = false;
                particles[i].randomSeed =
                    CityHeroPocketExitTransitionController.FirstParticleRandomSeed + (uint)i;
                RecordPrefabOverride(particles[i]);
            }
            portal.SetActive(false);
            RecordPrefabOverride(portal.transform);
            RecordPrefabOverride(portal);

            GameObject coverRoot = new(
                ExitCoverRootName,
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(CanvasGroup));
            SceneManager.MoveGameObjectToScene(coverRoot, scene);
            Canvas coverCanvas = coverRoot.GetComponent<Canvas>();
            coverCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            coverCanvas.sortingOrder = 32000;
            CanvasScaler coverScaler = coverRoot.GetComponent<CanvasScaler>();
            coverScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            coverScaler.referenceResolution = new Vector2(1920f, 1080f);
            coverScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            coverScaler.matchWidthOrHeight = 0.5f;
            CanvasGroup coverGroup = coverRoot.GetComponent<CanvasGroup>();
            coverGroup.alpha = 0f;
            coverGroup.interactable = false;
            coverGroup.blocksRaycasts = false;

            GameObject coverImageObject = new(
                "CyanWhiteFullCover",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            coverImageObject.transform.SetParent(coverRoot.transform, worldPositionStays: false);
            RectTransform coverRect = (RectTransform)coverImageObject.transform;
            coverRect.anchorMin = Vector2.zero;
            coverRect.anchorMax = Vector2.one;
            coverRect.pivot = new Vector2(0.5f, 0.5f);
            coverRect.anchoredPosition = Vector2.zero;
            coverRect.sizeDelta = Vector2.zero;
            Image coverImage = coverImageObject.GetComponent<Image>();
            coverImage.color = ExitCoverColor;
            coverImage.raycastTarget = false;

            GameObject triggerObject = CreateChild(runtimeRoot, ExitTriggerName);
            triggerObject.transform.localPosition = ExitTriggerPosition;
            BoxCollider trigger = triggerObject.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = ExitTriggerSize;
            trigger.center = ExitTriggerCenter;
            Rigidbody triggerBody = triggerObject.AddComponent<Rigidbody>();
            triggerBody.isKinematic = true;
            triggerBody.useGravity = false;
            triggerBody.interpolation = RigidbodyInterpolation.None;
            triggerBody.collisionDetectionMode = CollisionDetectionMode.Discrete;

            CanvasGroup hudGroup = hud.Root.GetComponent<CanvasGroup>();
            Require(hudGroup != null,
                "City combat HUD root requires its product-instance CanvasGroup.");
            CityHeroPocketExitTransitionController transition =
                triggerObject.AddComponent<CityHeroPocketExitTransitionController>();
            transition.Configure(
                encounter,
                RequireSingle<CharacterController>(player.Root),
                trigger,
                transitionFocus,
                portal.transform,
                ExitPortalAuthoredScale,
                hudGroup,
                coverGroup,
                player.Movement,
                player.Action,
                player.Mode,
                player.Ranged,
                enemy.Soldier,
                enemy.ProjectileDriver);
        }

        private static HudPackage CreateHud(Scene scene)
        {
            GameObject prefab = LoadRequired<GameObject>(HudPrefabPath);
            GameObject root = PrefabUtility.InstantiatePrefab(prefab, scene) as GameObject;
            Require(root != null, "Failed to instantiate shared combat HUD prefab.");
            root.name = HudRootName;
            root.SetActive(false);

            Canvas canvas = root.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = root.AddComponent<Canvas>();
            }
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = root.AddComponent<CanvasScaler>();
            }
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            if (root.GetComponent<GraphicRaycaster>() == null)
            {
                root.AddComponent<GraphicRaycaster>();
            }
            RecordPrefabOverride(canvas);
            RecordPrefabOverride(scaler);

            BossBarrageLaneReviewCombatHudBinder[] routeBinders =
                root.GetComponentsInChildren<BossBarrageLaneReviewCombatHudBinder>(true);
            for (int i = 0; i < routeBinders.Length; i++)
            {
                UnityEngine.Object.DestroyImmediate(routeBinders[i]);
            }

            CombatHudPresenter presenter = RequireSingle<CombatHudPresenter>(root);
            CombatHudInputBridge input = RequireSingle<CombatHudInputBridge>(root);
            CombatHudVirtualJoystick joystick = EnsureVirtualJoystick(root);
            EnsureAimDragArea(root);
            CombatSessionOverlayPresenter session =
                RequireSingle<CombatSessionOverlayPresenter>(root);
            DisableDirectLoadRetry(session);
            OneRowCombatHudBinder binder = root.GetComponent<OneRowCombatHudBinder>();
            if (binder == null)
            {
                binder = root.AddComponent<OneRowCombatHudBinder>();
            }
            return new HudPackage(root, presenter, input, joystick, session, binder);
        }

        private static void DisableDirectLoadRetry(CombatSessionOverlayPresenter session)
        {
            SerializedObject serialized = new(session);
            SerializedProperty retryProperty = RequireProperty(serialized, "retryButton");
            Button retryButton = retryProperty.objectReferenceValue as Button;
            if (retryButton != null)
            {
                retryButton.gameObject.SetActive(false);
                RecordPrefabOverride(retryButton.gameObject);
            }
            retryProperty.objectReferenceValue = null;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            RecordPrefabOverride(session);
        }

        private static void ConfigureHud(
            HudPackage hud,
            CombatEncounterController encounter,
            PlayerPackage player,
            EnemyPackage enemy,
            ActionCameraController camera)
        {
            SerializedObject serializedInput = new(hud.Input);
            SetObject(serializedInput, "presenter", hud.Presenter);
            serializedInput.ApplyModifiedPropertiesWithoutUndo();
            RecordPrefabOverride(hud.Input);

            hud.Joystick.Configure(player.Movement, hud.JoystickKnob);
            RecordPrefabOverride(hud.Joystick);

            CombatHudAimDragInput aimInput =
                RequireSingle<CombatHudAimDragInput>(hud.Root);
            SerializedObject serializedAimInput = new(aimInput);
            SetObject(serializedAimInput, "movementController", player.Movement);
            SetObject(serializedAimInput, "combatModeController", player.Mode);
            SetObject(serializedAimInput, "aimController", player.Aim);
            SetObject(serializedAimInput, "rangedBasicAttackAction", player.Ranged);
            SetObject(serializedAimInput, "cameraController", camera);
            SetBool(serializedAimInput, "routeAimToMovementLook", false);
            serializedAimInput.ApplyModifiedPropertiesWithoutUndo();
            RecordPrefabOverride(aimInput);

            ConfigurePointerAction(
                hud.Root,
                hud.Input,
                "BasicAttackButton",
                CombatHudActionId.BasicAttack,
                sendHoldState: true);
            ConfigurePointerAction(
                hud.Root,
                hud.Input,
                "DodgeButton",
                CombatHudActionId.Dodge,
                sendHoldState: false);
            ConfigurePointerAction(
                hud.Root,
                hud.Input,
                "PauseButton",
                CombatHudActionId.Pause,
                sendHoldState: false);

            DisableUnavailableAction(hud.Root, "Skill1Button");
            DisableUnavailableAction(hud.Root, "UltimateButton");
            DisableUnavailableAction(hud.Root, "SummonSlot1Button");
            DisableUnavailableAction(hud.Root, "SummonSlot2Button");
            DisableUnavailableAction(hud.Root, "SummonSlot3Button");

            hud.Binder.Configure(
                hud.Presenter,
                hud.Input,
                hud.Joystick,
                hud.Session,
                encounter,
                player.Health,
                enemy.Health,
                player.Movement,
                player.Action,
                player.Mode,
                player.Ranged);
            RecordPrefabOverride(hud.Binder);

            SerializedObject serializedBinder = new(hud.Binder);
            SetString(
                serializedBinder,
                "objectiveText",
                ProductObjectiveText);
            serializedBinder.ApplyModifiedPropertiesWithoutUndo();
            RecordPrefabOverride(hud.Binder);
        }

        private static CombatHudVirtualJoystick EnsureVirtualJoystick(GameObject hudRoot)
        {
            CombatHudVirtualJoystick[] existing =
                hudRoot.GetComponentsInChildren<CombatHudVirtualJoystick>(true);
            Require(existing.Length <= 1,
                $"'{hudRoot.name}' has duplicate {nameof(CombatHudVirtualJoystick)} components.");
            Transform ring = FindDescendant(hudRoot.transform, "MoveJoystickRing");
            Transform knob = FindDescendant(hudRoot.transform, "MoveJoystickKnob");
            Require(ring is RectTransform,
                "Shared combat HUD is missing MoveJoystickRing RectTransform.");
            Require(knob is RectTransform,
                "Shared combat HUD is missing MoveJoystickKnob RectTransform.");
            CombatHudVirtualJoystick joystick = existing.Length == 1
                ? existing[0]
                : ring.gameObject.AddComponent<CombatHudVirtualJoystick>();
            joystick.Configure(null, (RectTransform)knob);
            return joystick;
        }

        private static CombatHudAimDragInput EnsureAimDragArea(GameObject hudRoot)
        {
            CombatHudAimDragInput[] existing =
                hudRoot.GetComponentsInChildren<CombatHudAimDragInput>(true);
            Require(existing.Length <= 1,
                $"'{hudRoot.name}' has duplicate {nameof(CombatHudAimDragInput)} components.");
            if (existing.Length == 1)
            {
                return existing[0];
            }

            GameObject area = new(
                "AimDragArea",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(CombatHudAimDragInput));
            RectTransform rect = (RectTransform)area.transform;
            rect.SetParent(hudRoot.transform, worldPositionStays: false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
            rect.SetAsFirstSibling();

            Image image = area.GetComponent<Image>();
            image.color = Color.clear;
            image.raycastTarget = true;
            return area.GetComponent<CombatHudAimDragInput>();
        }

        private static void ConfigurePointerAction(
            GameObject hudRoot,
            CombatHudInputBridge input,
            string buttonName,
            CombatHudActionId actionId,
            bool sendHoldState)
        {
            Transform buttonTransform = FindDescendant(hudRoot.transform, buttonName);
            Require(buttonTransform != null,
                $"Shared combat HUD is missing '{buttonName}'.");
            Button button = buttonTransform.GetComponent<Button>();
            Require(button != null, $"Shared combat HUD '{buttonName}' has no Button.");

            CombatHudPointerActionInput[] inputs =
                buttonTransform.GetComponents<CombatHudPointerActionInput>();
            CombatHudPointerActionInput pointer = inputs.Length > 0
                ? inputs[0]
                : buttonTransform.gameObject.AddComponent<CombatHudPointerActionInput>();
            for (int i = 1; i < inputs.Length; i++)
            {
                UnityEngine.Object.DestroyImmediate(inputs[i]);
            }

            SerializedObject serializedButton = new(button);
            SetBool(serializedButton, "m_Interactable", true);
            serializedButton.ApplyModifiedPropertiesWithoutUndo();
            SerializedObject serializedPointer = new(pointer);
            SetObject(serializedPointer, "inputBridge", input);
            RequireProperty(serializedPointer, "actionId").intValue = (int)actionId;
            SetBool(serializedPointer, "sendHoldState", sendHoldState);
            serializedPointer.ApplyModifiedPropertiesWithoutUndo();
            RecordPrefabOverride(button);
            RecordPrefabOverride(pointer);
        }

        private static void DisableUnavailableAction(GameObject hudRoot, string buttonName)
        {
            Transform buttonTransform = FindDescendant(hudRoot.transform, buttonName);
            if (buttonTransform == null)
            {
                return;
            }

            Button button = buttonTransform.GetComponent<Button>();
            if (button != null)
            {
                SerializedObject serializedButton = new(button);
                SetBool(serializedButton, "m_Interactable", false);
                serializedButton.ApplyModifiedPropertiesWithoutUndo();
                RecordPrefabOverride(button);
            }

            CombatHudPointerActionInput[] inputs =
                buttonTransform.GetComponents<CombatHudPointerActionInput>();
            for (int i = 0; i < inputs.Length; i++)
            {
                UnityEngine.Object.DestroyImmediate(inputs[i]);
            }
        }

        private static void CreateLighting(Scene scene)
        {
            GameObject keyObject = CreateSceneRoot(scene, "CityHeroPocket_NeutralKey", active: true);
            Light key = keyObject.AddComponent<Light>();
            key.type = LightType.Directional;
            key.color = Color.white;
            key.intensity = 1.28f;
            key.shadows = LightShadows.Soft;
            key.shadowStrength = 0.62f;
            keyObject.transform.rotation = Quaternion.Euler(46f, -32f, 0f);
            RenderSettings.sun = key;

            GameObject fillObject = CreateSceneRoot(scene, "CityHeroPocket_NeutralFill", active: true);
            Light fill = fillObject.AddComponent<Light>();
            fill.type = LightType.Directional;
            fill.color = Color.white;
            fill.intensity = 0.24f;
            fill.shadows = LightShadows.None;
            fillObject.transform.rotation = Quaternion.Euler(58f, 142f, 0f);

            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientIntensity = 1f;
            RenderSettings.ambientSkyColor = new Color(0.62f, 0.70f, 0.79f);
            RenderSettings.ambientEquatorColor = new Color(0.42f, 0.45f, 0.48f);
            RenderSettings.ambientGroundColor = new Color(0.19f, 0.20f, 0.22f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.58f, 0.66f, 0.73f);
            RenderSettings.fogStartDistance = 32f;
            RenderSettings.fogEndDistance = 105f;
        }

        private static VolumeProfile EnsureCityLookProfile()
        {
            VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(CityLookProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                profile.name = "DB_CityHeroPocket_PostProcess";
                AssetDatabase.CreateAsset(profile, CityLookProfilePath);
            }

            RemoveUnexpectedVolumeOverrides(profile);

            Tonemapping tonemapping = EnsureVolumeOverride<Tonemapping>(profile);
            tonemapping.SetAllOverridesTo(false);
            tonemapping.mode.Override(TonemappingMode.Neutral);

            WhiteBalance whiteBalance = EnsureVolumeOverride<WhiteBalance>(profile);
            whiteBalance.SetAllOverridesTo(false);
            whiteBalance.temperature.Override(0f);
            whiteBalance.tint.Override(0f);

            ColorAdjustments color = EnsureVolumeOverride<ColorAdjustments>(profile);
            color.SetAllOverridesTo(false);
            color.postExposure.Override(0.22f);
            color.contrast.Override(-4f);
            color.colorFilter.Override(Color.white);
            color.hueShift.Override(0f);
            color.saturation.Override(0f);

            LiftGammaGain wheels = EnsureVolumeOverride<LiftGammaGain>(profile);
            wheels.SetAllOverridesTo(false);
            wheels.lift.Override(new Vector4(1f, 1f, 1f, 0.015f));
            wheels.gamma.Override(new Vector4(1f, 1f, 1f, 0f));
            wheels.gain.Override(new Vector4(1f, 1f, 1f, 0f));

            Bloom bloom = EnsureVolumeOverride<Bloom>(profile);
            bloom.SetAllOverridesTo(false);
            bloom.threshold.Override(0.86f);
            bloom.intensity.Override(0.42f);
            bloom.scatter.Override(0.76f);
            bloom.downscale.Override(BloomDownscaleMode.Half);
            bloom.maxIterations.Override(7);
            bloom.highQualityFiltering.Override(true);

            Vignette vignette = EnsureVolumeOverride<Vignette>(profile);
            vignette.SetAllOverridesTo(false);
            vignette.color.Override(Color.black);
            vignette.center.Override(new Vector2(0.5f, 0.5f));
            vignette.intensity.Override(0.11f);
            vignette.smoothness.Override(0.58f);
            vignette.rounded.Override(false);

            DepthOfField depthOfField = EnsureVolumeOverride<DepthOfField>(profile);
            depthOfField.SetAllOverridesTo(false);
            depthOfField.active = false;
            EditorUtility.SetDirty(depthOfField);

            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static void RemoveUnexpectedVolumeOverrides(VolumeProfile profile)
        {
            var retainedTypes = new HashSet<Type>();
            for (int i = profile.components.Count - 1; i >= 0; i--)
            {
                VolumeComponent component = profile.components[i];
                Type type = component != null ? component.GetType() : null;
                bool allowed = type == typeof(Tonemapping)
                    || type == typeof(WhiteBalance)
                    || type == typeof(ColorAdjustments)
                    || type == typeof(LiftGammaGain)
                    || type == typeof(Bloom)
                    || type == typeof(Vignette)
                    || type == typeof(DepthOfField);
                if (allowed && retainedTypes.Add(type))
                {
                    continue;
                }

                profile.components.RemoveAt(i);
                if (component != null)
                {
                    UnityEngine.Object.DestroyImmediate(
                        component,
                        allowDestroyingAssets: true);
                }
            }
        }

        private static T EnsureVolumeOverride<T>(VolumeProfile profile)
            where T : VolumeComponent
        {
            if (profile.TryGet(out T component))
            {
                component.active = true;
                EditorUtility.SetDirty(component);
                return component;
            }

            component = profile.Add<T>(overrides: true);
            component.name = typeof(T).Name;
            if (!AssetDatabase.Contains(component))
            {
                AssetDatabase.AddObjectToAsset(component, profile);
            }

            component.active = true;
            EditorUtility.SetDirty(component);
            return component;
        }

        private static void CreateGlobalVolume(Scene scene, VolumeProfile profile)
        {
            GameObject volumeObject = CreateSceneRoot(scene, GlobalVolumeName, active: true);
            Volume volume = volumeObject.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 40f;
            volume.weight = 1f;
            volume.sharedProfile = profile;
        }

        private static Material EnsureOwnedLitMaterial(
            string path,
            Color color,
            float metallic,
            float smoothness)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            Require(shader != null, "URP Lit shader is unavailable.");
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader)
                {
                    name = System.IO.Path.GetFileNameWithoutExtension(path),
                };
                AssetDatabase.CreateAsset(material, path);
            }

            material.shader = shader;
            string[] textureProperties = material.GetTexturePropertyNames();
            for (int i = 0; i < textureProperties.Length; i++)
            {
                string property = textureProperties[i];
                material.SetTexture(property, null);
                material.SetTextureScale(property, Vector2.one);
                material.SetTextureOffset(property, Vector2.zero);
            }
            material.shaderKeywords = Array.Empty<string>();
            material.SetOverrideTag("RenderType", string.Empty);
            material.renderQueue = -1;
            material.enableInstancing = false;
            material.doubleSidedGI = false;
            material.globalIlluminationFlags =
                MaterialGlobalIlluminationFlags.EmissiveIsBlack;
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }
            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }
            if (material.HasProperty("_Metallic"))
            {
                material.SetFloat("_Metallic", metallic);
            }
            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", smoothness);
            }
            if (material.HasProperty("_EmissionColor"))
            {
                material.SetColor("_EmissionColor", Color.black);
            }
            SetMaterialFloatIfPresent(material, "_WorkflowMode", 1f);
            SetMaterialFloatIfPresent(material, "_Surface", 0f);
            SetMaterialFloatIfPresent(material, "_Blend", 0f);
            SetMaterialFloatIfPresent(material, "_AlphaClip", 0f);
            SetMaterialFloatIfPresent(material, "_SrcBlend", (float)BlendMode.One);
            SetMaterialFloatIfPresent(material, "_DstBlend", (float)BlendMode.Zero);
            SetMaterialFloatIfPresent(material, "_SrcBlendAlpha", (float)BlendMode.One);
            SetMaterialFloatIfPresent(material, "_DstBlendAlpha", (float)BlendMode.Zero);
            SetMaterialFloatIfPresent(material, "_ZWrite", 1f);
            SetMaterialFloatIfPresent(material, "_Cull", (float)CullMode.Back);
            SetMaterialFloatIfPresent(material, "_QueueOffset", 0f);
            SetMaterialFloatIfPresent(material, "_QueueControl", -1f);
            SetMaterialFloatIfPresent(material, "_ReceiveShadows", 1f);
            SetMaterialFloatIfPresent(material, "_SpecularHighlights", 1f);
            SetMaterialFloatIfPresent(material, "_EnvironmentReflections", 1f);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void SetMaterialFloatIfPresent(
            Material material,
            string propertyName,
            float value)
        {
            if (material.HasProperty(propertyName))
            {
                material.SetFloat(propertyName, value);
            }
        }

        private static void CreateEventSystem(Scene scene)
        {
            GameObject eventObject = CreateSceneRoot(scene, "EventSystem", active: true);
            eventObject.AddComponent<EventSystem>();
            eventObject.AddComponent<InputSystemUIInputModule>();
        }

        private static GameObject CreateSceneRoot(Scene scene, string name, bool active)
        {
            GameObject root = new(name);
            root.SetActive(active);
            SceneManager.MoveGameObjectToScene(root, scene);
            return root;
        }

        private static GameObject CreateChild(Transform parent, string name)
        {
            GameObject child = new(name);
            child.transform.SetParent(parent, false);
            return child;
        }

        private static GameObject FindUniqueSceneObject(Scene scene, string name)
        {
            GameObject found = null;
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                Transform match = FindDescendant(roots[i].transform, name);
                if (match == null)
                {
                    continue;
                }

                Require(found == null,
                    $"Scene '{scene.path}' contains duplicate object '{name}'.");
                found = match.gameObject;
            }

            Require(found != null,
                $"Scene '{scene.path}' is missing required object '{name}'.");
            return found;
        }

        private static Transform FindDescendant(Transform root, string name)
        {
            if (root == null)
            {
                return null;
            }
            if (string.Equals(root.name, name, StringComparison.Ordinal))
            {
                return root;
            }
            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindDescendant(root.GetChild(i), name);
                if (found != null)
                {
                    return found;
                }
            }
            return null;
        }

        private static T RequireSingle<T>(GameObject root)
            where T : Component
        {
            T[] components = root.GetComponentsInChildren<T>(true);
            Require(components.Length == 1,
                $"'{root.name}' requires exactly one {typeof(T).Name}; found {components.Length}.");
            return components[0];
        }

        private static T LoadRequired<T>(string path)
            where T : UnityEngine.Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            Require(asset != null, $"Required asset is missing: {path}");
            return asset;
        }

        private static void EnsureFolderForAsset(string assetPath)
        {
            string parent = assetPath.Replace('\\', '/');
            parent = parent.Substring(0, parent.LastIndexOf('/'));
            EnsureFolder(parent);
        }

        private static void EnsureFolder(string folder)
        {
            folder = folder.Replace('\\', '/').TrimEnd('/');
            if (AssetDatabase.IsValidFolder(folder))
            {
                return;
            }
            int slash = folder.LastIndexOf('/');
            Require(slash > 0, $"Invalid Unity folder path '{folder}'.");
            string parent = folder.Substring(0, slash);
            string leaf = folder.Substring(slash + 1);
            EnsureFolder(parent);
            string guid = AssetDatabase.CreateFolder(parent, leaf);
            Require(!string.IsNullOrWhiteSpace(guid), $"Failed to create folder '{folder}'.");
        }

        private static void RebindObjectReferenceByPropertyName(
            GameObject root,
            string propertyName,
            UnityEngine.Object value,
            bool applyToInactive)
        {
            MonoBehaviour[] behaviours = root.GetComponentsInChildren<MonoBehaviour>(applyToInactive);
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour == null)
                {
                    continue;
                }
                SerializedObject serialized = new(behaviour);
                SerializedProperty property = serialized.FindProperty(propertyName);
                if (property == null
                    || property.propertyType != SerializedPropertyType.ObjectReference)
                {
                    continue;
                }
                property.objectReferenceValue = value;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void RecordPrefabOverride(UnityEngine.Object value)
        {
            if (value == null)
            {
                return;
            }
            EditorUtility.SetDirty(value);
            if (PrefabUtility.IsPartOfPrefabInstance(value))
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(value);
            }
        }

        private static void SetObject(
            SerializedObject serialized,
            string propertyName,
            UnityEngine.Object value)
        {
            SerializedProperty property = RequireProperty(serialized, propertyName);
            Require(property.propertyType == SerializedPropertyType.ObjectReference,
                $"'{propertyName}' is not an object reference on {serialized.targetObject.name}.");
            property.objectReferenceValue = value;
        }

        private static void SetObjectArray(
            SerializedObject serialized,
            string propertyName,
            IReadOnlyList<UnityEngine.Object> values)
        {
            SerializedProperty property = RequireProperty(serialized, propertyName);
            Require(property.isArray, $"'{propertyName}' is not an array.");
            property.arraySize = values.Count;
            for (int i = 0; i < values.Count; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }
        }

        private static void SetBool(SerializedObject serialized, string propertyName, bool value)
        {
            RequireProperty(serialized, propertyName).boolValue = value;
        }

        private static void SetFloat(SerializedObject serialized, string propertyName, float value)
        {
            RequireProperty(serialized, propertyName).floatValue = value;
        }

        private static void SetString(SerializedObject serialized, string propertyName, string value)
        {
            RequireProperty(serialized, propertyName).stringValue = value;
        }

        private static void SetVector3(
            SerializedObject serialized,
            string propertyName,
            Vector3 value)
        {
            RequireProperty(serialized, propertyName).vector3Value = value;
        }

        private static SerializedProperty RequireProperty(
            SerializedObject serialized,
            string propertyName)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            Require(property != null,
                $"Serialized property '{propertyName}' is missing on {serialized.targetObject.GetType().Name}.");
            return property;
        }

        private static void RefuseDirtyOpenScenes()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                Require(!scene.isDirty,
                    $"Refusing to replace dirty scene '{scene.path}'. Save or discard it first.");
            }
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }

        internal readonly struct ModuleSpec
        {
            public ModuleSpec(
                string id,
                string prefabPath,
                Vector3 position,
                Vector3 euler,
                Vector3 scale)
            {
                Id = id;
                PrefabPath = prefabPath;
                Position = position;
                Euler = euler;
                Scale = scale;
            }

            public string Id { get; }
            public string PrefabPath { get; }
            public Vector3 Position { get; }
            public Vector3 Euler { get; }
            public Vector3 Scale { get; }
        }

        private readonly struct CameraPackage
        {
            public CameraPackage(
                Camera camera,
                UniversalAdditionalCameraData cameraData,
                ActionCameraController controller)
            {
                Camera = camera;
                CameraData = cameraData;
                Controller = controller;
            }

            public Camera Camera { get; }
            public UniversalAdditionalCameraData CameraData { get; }
            public ActionCameraController Controller { get; }
        }

        private readonly struct PlayerPackage
        {
            public PlayerPackage(
                GameObject root,
                CombatHealth health,
                PlayerMovementController movement,
                PlayerCombatTargetSelector targetSelector,
                PlayerActionController action,
                PlayerCombatModeController mode,
                PlayerRangedAimController aim,
                PlayerRangedBasicAttackAction ranged,
                PlayerLockTargetController lockTarget,
                Animator animator)
            {
                Root = root;
                Health = health;
                Movement = movement;
                TargetSelector = targetSelector;
                Action = action;
                Mode = mode;
                Aim = aim;
                Ranged = ranged;
                LockTarget = lockTarget;
                Animator = animator;
            }

            public GameObject Root { get; }
            public CombatHealth Health { get; }
            public PlayerMovementController Movement { get; }
            public PlayerCombatTargetSelector TargetSelector { get; }
            public PlayerActionController Action { get; }
            public PlayerCombatModeController Mode { get; }
            public PlayerRangedAimController Aim { get; }
            public PlayerRangedBasicAttackAction Ranged { get; }
            public PlayerLockTargetController LockTarget { get; }
            public Animator Animator { get; }
        }

        private readonly struct EnemyPackage
        {
            public EnemyPackage(
                GameObject root,
                CombatHealth health,
                BasicSoldierEnemy soldier,
                CombatTargetSensor sensor,
                BasicSoldierProjectileAttackDriver projectileDriver)
            {
                Root = root;
                Health = health;
                Soldier = soldier;
                Sensor = sensor;
                ProjectileDriver = projectileDriver;
            }

            public GameObject Root { get; }
            public CombatHealth Health { get; }
            public BasicSoldierEnemy Soldier { get; }
            public CombatTargetSensor Sensor { get; }
            public BasicSoldierProjectileAttackDriver ProjectileDriver { get; }
        }

        private readonly struct HudPackage
        {
            public HudPackage(
                GameObject root,
                CombatHudPresenter presenter,
                CombatHudInputBridge input,
                CombatHudVirtualJoystick joystick,
                CombatSessionOverlayPresenter session,
                OneRowCombatHudBinder binder)
            {
                Root = root;
                Presenter = presenter;
                Input = input;
                Joystick = joystick;
                Session = session;
                Binder = binder;
                Transform knob = FindDescendant(root.transform, "MoveJoystickKnob");
                Require(knob is RectTransform,
                    "Shared combat HUD is missing MoveJoystickKnob RectTransform.");
                JoystickKnob = (RectTransform)knob;
            }

            public GameObject Root { get; }
            public CombatHudPresenter Presenter { get; }
            public CombatHudInputBridge Input { get; }
            public CombatHudVirtualJoystick Joystick { get; }
            public RectTransform JoystickKnob { get; }
            public CombatSessionOverlayPresenter Session { get; }
            public OneRowCombatHudBinder Binder { get; }
        }
    }
}
