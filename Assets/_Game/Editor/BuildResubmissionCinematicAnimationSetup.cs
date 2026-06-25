using System;
using System.Collections.Generic;
using DimensionBrawl.Presentation;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace DimensionBrawl.Editor
{
    public static class BuildResubmissionCinematicAnimationSetup
    {
        public const string AnimationRoot = "Assets/_Game/Art/Animations/Cinematics/Inori/KawaiiP0";
        public const string CinematicControllerPath =
            "Assets/_Game/Art/Animations/Cinematics/Inori/DB_Inori_CinematicP0.controller";

        private const string KawaiiAnimationRoot =
            "Assets/_Imported/AssetStore/KAWAII_ANIMATIONS_100/Assets/Animations";
        private const string TwinSwordInPlaceAnimationRoot =
            "Assets/_Imported/AssetStore/TwinSwordAnimsetBase_V2/Animation/InPlace";
        private const string KnightZweihanderInPlaceAnimationRoot =
            "Assets/_Imported/AssetStore/Knight_Zweihander_Animset/Animation/Execution/Inplace";
        private const string GreatSwordBowAttackInPlaceAnimationRoot =
            "Assets/_Imported/AssetStore/GreatSword_Animset/Animation/Bow/Attack/Inplace";

        private static readonly ClipSpec[] ClipSpecs =
        {
            new ClipSpec("CIN_IntroLookAtHands", "@KA_Idle03_LookAtHands.FBX", false),
            new ClipSpec("CIN_IntroSurprised", "@KA_Idle29_Surprised.FBX", false),
            new ClipSpec("CIN_IntroStumble", "@KA_Idle17_StumbleAndFall.FBX", false),
            new ClipSpec("CIN_IntroPickUp", "@KA_Idle31_PickUp.FBX", false),
            new ClipSpec("CIN_QTEEntryDash", "@KA_Dash_Fwd.FBX", false),
            new ClipSpec("CIN_QTEMagicShot", "@KA_Combat_Witch_Magic_Shot.FBX", false),
            new ClipSpec("CIN_UltimateCharge", "@KA_Combat_Witch_Magic_Awakening.FBX", false),
            new ClipSpec("CIN_UltimateRelease", "@KA_Combat_Witch_Magic_Shot.FBX", false),
            new ClipSpec("CIN_UltimateImpact", "@KA_Combat_Witch_Magic_Impact.FBX", false),
            new ClipSpec("CIN_UltimateRecover", "@KA_Combat_Witch_Magic_Recovery.FBX", false),
            new ClipSpec("CIN_CombatReady", "@KA_Combat_OHSword01_Idle01.FBX", true),
            new ClipSpec("CIN_SwordCharge", "@KA_Combat_OHSword01_ChargeAttack.FBX", false),
            new ClipSpec("CIN_TwinSwordIdle", TwinSwordInPlaceAnimationRoot, "TwinSword_Idle_Inplace.FBX", true),
            new ClipSpec("CIN_BossIntroReady", TwinSwordInPlaceAnimationRoot, "TwinSword_attack01_Inplace.FBX", false),
            new ClipSpec("CIN_BossIntroAnswer", TwinSwordInPlaceAnimationRoot, "TwinSword_attack12_Inplace.FBX", false),
            new ClipSpec("CIN_PhaseCounterRelease", TwinSwordInPlaceAnimationRoot, "TwinSword_attack12_Inplace.FBX", false),
            new ClipSpec("CIN_BreakHitConfirm", TwinSwordInPlaceAnimationRoot, "TwinSword_attack05_Inplace.FBX", false),
            new ClipSpec("CIN_SummonProxyHit", TwinSwordInPlaceAnimationRoot, "TwinSword_attack08_Inplace.FBX", false),
            new ClipSpec("CIN_ResultSettle", TwinSwordInPlaceAnimationRoot, "TwinSword_Idle_General_Inplace.FBX", true),
            new ClipSpec("CIN_BackViewProjectileAim", GreatSwordBowAttackInPlaceAnimationRoot, "GhostSamurai_Bow_Shoot_Aim_Idle_Inplace.FBX", true),
            new ClipSpec("CIN_BackViewProjectileCharge", GreatSwordBowAttackInPlaceAnimationRoot, "GhostSamurai_Bow_Shoot_SP_Enhance_Hold_Inplace.FBX", true),
            new ClipSpec("CIN_BackViewProjectileFire", GreatSwordBowAttackInPlaceAnimationRoot, "GhostSamurai_Bow_Shoot_SP01_Inplace.FBX", false),
            new ClipSpec("CIN_BackViewProjectileBurst", GreatSwordBowAttackInPlaceAnimationRoot, "GhostSamurai_Bow_Shoot_SP_Burst_Inplace.FBX", false),
            new ClipSpec("CIN_BackViewProjectileRecover", GreatSwordBowAttackInPlaceAnimationRoot, "GhostSamurai_Bow_Shoot_End_Inplace.FBX", false),
            new ClipSpec("CIN_LucyAmbushAnswer", KnightZweihanderInPlaceAnimationRoot, "Lucy_Ambush01_Inplace.FBX", false),
            new ClipSpec("CIN_LucyExecutionFinisher", KnightZweihanderInPlaceAnimationRoot, "Lucy_Execution03_Inplace.FBX", false)
        };

        [MenuItem("DimensionBrawl/Cinematics/Rebuild Inori Cinematic P0 Animations")]
        public static void RebuildInoriCinematicP0AnimationsMenu()
        {
            RebuildInoriCinematicP0Animations();
            Debug.Log("Rebuilt Inori cinematic P0 animation assets.");
        }

        public static void RunBatchAnimationGeneration()
        {
            RebuildInoriCinematicP0Animations();
        }

        public static RuntimeAnimatorController RebuildInoriCinematicP0Animations()
        {
            ActionFoundationInoriPlayerVisualAssetSetup.EnsureInoriPlayerVisualAssets();
            EnsureFolder(AnimationRoot);

            Dictionary<string, AnimationClip> clipsByState = new Dictionary<string, AnimationClip>(StringComparer.Ordinal);
            for (int i = 0; i < ClipSpecs.Length; i++)
            {
                AnimationClip clip = PromoteAndConfigureClip(ClipSpecs[i]);
                clipsByState[ClipSpecs[i].StateName] = clip;
            }

            AnimatorController controller = RebuildController(clipsByState);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return controller;
        }

        public static IReadOnlyList<string> RequiredStateNames
        {
            get
            {
                string[] names = new string[ClipSpecs.Length];
                for (int i = 0; i < ClipSpecs.Length; i++)
                {
                    names[i] = ClipSpecs[i].StateName;
                }

                return names;
            }
        }

        private static AnimationClip PromoteAndConfigureClip(ClipSpec spec)
        {
            string sourcePath = $"{spec.SourceRoot}/{spec.SourceFileName}";
            string targetPath = $"{AnimationRoot}/{spec.StateName}.fbx";
            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(sourcePath) == null)
            {
                throw new InvalidOperationException($"Missing source animation at {sourcePath}.");
            }

            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(targetPath) == null)
            {
                if (!AssetDatabase.CopyAsset(sourcePath, targetPath))
                {
                    throw new InvalidOperationException($"Failed to promote {sourcePath} to {targetPath}.");
                }
            }

            ModelImporter importer = AssetImporter.GetAtPath(targetPath) as ModelImporter;
            if (importer == null)
            {
                throw new InvalidOperationException($"Missing ModelImporter for promoted animation {targetPath}.");
            }

            importer.importAnimation = true;
            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.sourceAvatar = null;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.importBlendShapes = false;
            importer.importCameras = false;
            importer.importLights = false;
            importer.resampleCurves = true;
            importer.animationCompression = ModelImporterAnimationCompression.Off;
            importer.SaveAndReimport();

            importer = AssetImporter.GetAtPath(targetPath) as ModelImporter;
            if (importer == null)
            {
                throw new InvalidOperationException($"Missing ModelImporter after reimport for promoted animation {targetPath}.");
            }

            ModelImporterClipAnimation[] clips = importer.defaultClipAnimations;
            if (clips == null || clips.Length == 0)
            {
                throw new InvalidOperationException($"Promoted animation has no default clip: {targetPath}.");
            }

            for (int i = 0; i < clips.Length; i++)
            {
                clips[i].name = i == 0 ? spec.StateName : $"{spec.StateName}_{i + 1}";
                clips[i].loopTime = spec.LoopTime;
                clips[i].loopPose = spec.LoopTime;
                clips[i].lockRootHeightY = true;
                clips[i].lockRootPositionXZ = true;
                clips[i].lockRootRotation = true;
                clips[i].keepOriginalPositionY = true;
                clips[i].keepOriginalPositionXZ = true;
                clips[i].keepOriginalOrientation = true;
            }

            importer.clipAnimations = clips;
            importer.SaveAndReimport();

            AnimationClip promotedClip = FindAnimationClip(targetPath, spec.StateName);
            if (promotedClip == null)
            {
                throw new InvalidOperationException($"Failed to load promoted clip {spec.StateName} from {targetPath}.");
            }

            return promotedClip;
        }

        private static AnimatorController RebuildController(Dictionary<string, AnimationClip> clipsByState)
        {
            EnsureFolder(PathParent(CinematicControllerPath));
            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(CinematicControllerPath) != null
                && !AssetDatabase.DeleteAsset(CinematicControllerPath))
            {
                throw new InvalidOperationException($"Failed to replace {CinematicControllerPath}.");
            }

            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(CinematicControllerPath);
            if (controller == null)
            {
                throw new InvalidOperationException($"Failed to create {CinematicControllerPath}.");
            }

            AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
            stateMachine.name = "Inori Cinematic P0";
            for (int i = stateMachine.states.Length - 1; i >= 0; i--)
            {
                stateMachine.RemoveState(stateMachine.states[i].state);
            }

            Vector3 position = new Vector3(240f, 40f, 0f);
            for (int i = 0; i < ClipSpecs.Length; i++)
            {
                ClipSpec spec = ClipSpecs[i];
                AnimatorState state = stateMachine.AddState(spec.StateName, position + new Vector3(0f, i * 58f, 0f));
                state.motion = clipsByState[spec.StateName];
                state.writeDefaultValues = true;
                if (i == 0)
                {
                    stateMachine.defaultState = state;
                }
            }

            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static AnimationClip FindAnimationClip(string assetPath, string clipName)
        {
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] is AnimationClip clip
                    && string.Equals(clip.name, clipName, StringComparison.Ordinal))
                {
                    return clip;
                }
            }

            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] is AnimationClip clip && !clip.name.StartsWith("__preview__", StringComparison.Ordinal))
                {
                    return clip;
                }
            }

            return null;
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string parent = PathParent(folderPath);
            if (!string.IsNullOrWhiteSpace(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, folderPath.Substring(folderPath.LastIndexOf('/') + 1));
        }

        private static string PathParent(string path)
        {
            string normalized = path.Replace('\\', '/');
            int separator = normalized.LastIndexOf('/');
            return separator > 0 ? normalized.Substring(0, separator) : string.Empty;
        }

        private readonly struct ClipSpec
        {
            public ClipSpec(string stateName, string sourceFileName, bool loopTime)
                : this(stateName, KawaiiAnimationRoot, sourceFileName, loopTime)
            {
            }

            public ClipSpec(string stateName, string sourceRoot, string sourceFileName, bool loopTime)
            {
                StateName = stateName;
                SourceRoot = sourceRoot;
                SourceFileName = sourceFileName;
                LoopTime = loopTime;
            }

            public string StateName { get; }
            public string SourceRoot { get; }
            public string SourceFileName { get; }
            public bool LoopTime { get; }
        }
    }
}
