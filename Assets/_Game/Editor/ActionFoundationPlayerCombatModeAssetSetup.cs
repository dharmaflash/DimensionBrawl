using System;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace DimensionBrawl.Editor
{
    public static class ActionFoundationPlayerCombatModeAssetSetup
    {
        public const string RangedCandidateModelPath =
            "Assets/_Game/Art/Characters/Player/RifleGirl/Models/Rifle_Full_Body.fbx";
        public const string RangedCandidateWeaponModelPath =
            "Assets/_Game/Art/Characters/Player/RifleGirl/Weapons/Weapon_Rifle.fbx";
        public const string RangedCandidateControllerPath =
            "Assets/_Game/Art/Animations/Player/RifleGirl/DB_RifleGirl_RangedCandidate.controller";

        private const string SourceRoot =
            "Assets/_Imported/AssetStore/CombatGirlsCharacterPack_RifleGirl/RifleGirl";
        private const string SourceModelPath = SourceRoot + "/Models/Rifle_Full_Body.FBX";
        private const string SourceWeaponModelPath = SourceRoot + "/Models/Parts/Weapon_Rifle.fbx";
        private const string MaterialRoot = "Assets/_Game/Art/Characters/Player/RifleGirl/Materials";
        private const string TextureRoot = "Assets/_Game/Art/Characters/Player/RifleGirl/Textures";
        private const string AnimationRoot = "Assets/_Game/Art/Animations/Player/RifleGirl";

        private static readonly MaterialSpec[] MaterialSpecs =
        {
            new MaterialSpec(SourceRoot + "/Materials/Body/Body.mat", MaterialRoot + "/DB_RifleGirl_Body.mat"),
            new MaterialSpec(SourceRoot + "/Materials/Body/Eye.mat", MaterialRoot + "/DB_RifleGirl_Eye.mat"),
            new MaterialSpec(SourceRoot + "/Materials/Body/Face.mat", MaterialRoot + "/DB_RifleGirl_Face.mat"),
            new MaterialSpec(SourceRoot + "/Materials/Cloth/Rifle_Cloth 1.mat", MaterialRoot + "/DB_RifleGirl_Cloth01.mat"),
            new MaterialSpec(SourceRoot + "/Materials/Hair/Rifle_Hair 1.mat", MaterialRoot + "/DB_RifleGirl_Hair01.mat"),
            new MaterialSpec(SourceRoot + "/Materials/Sportswear/Sportswear.mat", MaterialRoot + "/DB_RifleGirl_Sportswear.mat"),
            new MaterialSpec(SourceRoot + "/Materials/Weapon/Rifle_Weapon_Style_Wood.mat", MaterialRoot + "/DB_RifleGirl_RangedFocus.mat")
        };

        private static readonly AnimationSpec[] AnimationSpecs =
        {
            new AnimationSpec("Aiming/R_AimIdle.fbx", "RG_AimIdle", true),
            new AnimationSpec("Aiming/R_AimIdle_AutoShoot.fbx", "RG_AimIdleAutoShoot", true),
            new AnimationSpec("Aiming/R_AimJog.fbx", "RG_AimJog", true),
            new AnimationSpec("Aiming/R_AimWalk_F.fbx", "RG_AimWalkForward", true),
            new AnimationSpec("Aiming/R_AimWalk_B.fbx", "RG_AimWalkBack", true),
            new AnimationSpec("Aiming/R_AimWalk_FL.fbx", "RG_AimWalkForwardLeft", true),
            new AnimationSpec("Aiming/R_AimWalk_FR.fbx", "RG_AimWalkForwardRight", true),
            new AnimationSpec("Aiming/R_AimWalk_BL.fbx", "RG_AimWalkBackLeft", true),
            new AnimationSpec("Aiming/R_AimWalk_BR.fbx", "RG_AimWalkBackRight", true),
            new AnimationSpec("Aiming/R_AimTurn_L90.fbx", "RG_AimTurnLeft90", false),
            new AnimationSpec("Aiming/R_AimTurn_R90.fbx", "RG_AimTurnRight90", false),
            new AnimationSpec("Aiming/R_Shoot.fbx", "RG_Shoot", false),
            new AnimationSpec("Aiming/R_Reload.fbx", "RG_Reload", false),
            new AnimationSpec("Normal/R_Idle.fbx", "RG_Idle", true),
            new AnimationSpec("Normal/R_Walk.fbx", "RG_Walk", true),
            new AnimationSpec("Normal/R_Run.fbx", "RG_Run", true),
            new AnimationSpec("Normal/R_TakeGun.fbx", "RG_DrawRangedFocus", false),
            new AnimationSpec("Normal/R_PutGun.fbx", "RG_HolsterRangedFocus", false),
            new AnimationSpec("Normal/R_Evade.fbx", "RG_Evade", false),
            new AnimationSpec("Normal/R_Hit_Upper.fbx", "RG_HitUpper", false),
            new AnimationSpec("Normal/R_Die_F.fbx", "RG_DieFront", false, true)
        };

        [MenuItem("DimensionBrawl/Reapply Action Foundation Player Combat Mode Assets")]
        public static void ReapplyPlayerCombatModeAssetsMenu()
        {
            EnsureRangedCandidateAssets();
            Debug.Log("Reapplied ActionFoundation player combat mode candidate assets.");
        }

        public static void EnsureRangedCandidateAssets()
        {
            PromoteModel(SourceModelPath, RangedCandidateModelPath, true);
            PromoteModel(SourceWeaponModelPath, RangedCandidateWeaponModelPath, false);
            PromoteMaterials();

            for (int i = 0; i < AnimationSpecs.Length; i++)
            {
                PromoteAnimation(AnimationSpecs[i]);
            }

            BuildAnimatorController();
            AssetDatabase.SaveAssets();
        }

        private static void PromoteModel(string sourcePath, string targetPath, bool humanoid)
        {
            EnsureFolder(PathParent(targetPath));
            if (AssetDatabase.LoadAssetAtPath<GameObject>(targetPath) == null &&
                !AssetDatabase.CopyAsset(sourcePath, targetPath))
            {
                throw new InvalidOperationException($"Failed to promote player candidate model from {sourcePath} to {targetPath}.");
            }

            ModelImporter importer = RequireModelImporter(targetPath);
            importer.importAnimation = false;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            if (humanoid)
            {
                importer.animationType = ModelImporterAnimationType.Human;
                importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            }

            importer.SaveAndReimport();
        }

        private static void PromoteMaterials()
        {
            EnsureFolder(MaterialRoot);
            EnsureFolder(TextureRoot);
            for (int i = 0; i < MaterialSpecs.Length; i++)
            {
                PromoteMaterial(MaterialSpecs[i]);
            }
        }

        private static void PromoteMaterial(MaterialSpec spec)
        {
            Material sourceMaterial = AssetDatabase.LoadAssetAtPath<Material>(spec.SourcePath);
            if (sourceMaterial == null)
            {
                throw new InvalidOperationException($"Missing source player candidate material at {spec.SourcePath}.");
            }

            if (AssetDatabase.LoadAssetAtPath<Material>(spec.TargetPath) == null &&
                !AssetDatabase.CopyAsset(spec.SourcePath, spec.TargetPath))
            {
                throw new InvalidOperationException($"Failed to promote player candidate material from {spec.SourcePath} to {spec.TargetPath}.");
            }

            Material targetMaterial = AssetDatabase.LoadAssetAtPath<Material>(spec.TargetPath);
            if (targetMaterial == null)
            {
                throw new InvalidOperationException($"Promoted player candidate material missing at {spec.TargetPath}.");
            }

            targetMaterial.shader = sourceMaterial.shader;
            string[] textureProperties = sourceMaterial.GetTexturePropertyNames();
            for (int i = 0; i < textureProperties.Length; i++)
            {
                CopyTextureProperty(sourceMaterial, targetMaterial, textureProperties[i]);
            }

            EditorUtility.SetDirty(targetMaterial);
        }

        private static void CopyTextureProperty(Material sourceMaterial, Material targetMaterial, string propertyName)
        {
            if (!targetMaterial.HasProperty(propertyName))
            {
                return;
            }

            Texture sourceTexture = sourceMaterial.GetTexture(propertyName);
            targetMaterial.SetTexture(
                propertyName,
                sourceTexture != null ? PromoteTexture(sourceTexture, ClassifyTextureUsage(propertyName)) : null);
        }

        private static Texture PromoteTexture(Texture sourceTexture, TextureUsage usage)
        {
            string sourcePath = AssetDatabase.GetAssetPath(sourceTexture).Replace('\\', '/');
            if (string.IsNullOrWhiteSpace(sourcePath) || !sourcePath.StartsWith("Assets/_Imported/", StringComparison.Ordinal))
            {
                return sourceTexture;
            }

            string targetPath = $"{TextureRoot}/{Path.GetFileName(sourcePath)}";
            if (AssetDatabase.LoadAssetAtPath<Texture>(targetPath) == null &&
                !AssetDatabase.CopyAsset(sourcePath, targetPath))
            {
                throw new InvalidOperationException($"Failed to promote player candidate texture from {sourcePath} to {targetPath}.");
            }

            ConfigureTextureImporter(targetPath, usage);
            Texture promotedTexture = AssetDatabase.LoadAssetAtPath<Texture>(targetPath);
            if (promotedTexture == null)
            {
                throw new InvalidOperationException($"Promoted player candidate texture missing at {targetPath}.");
            }

            return promotedTexture;
        }

        private static void PromoteAnimation(AnimationSpec spec)
        {
            string sourcePath = $"{SourceRoot}/Animations/{spec.SourceRelativePath}";
            string targetPath = $"{AnimationRoot}/{spec.TargetClipName}.fbx";
            EnsureFolder(AnimationRoot);
            if (AssetDatabase.LoadAssetAtPath<AnimationClip>(targetPath) == null &&
                !AssetDatabase.CopyAsset(sourcePath, targetPath))
            {
                throw new InvalidOperationException($"Failed to promote player candidate animation from {sourcePath} to {targetPath}.");
            }

            ModelImporter importer = RequireModelImporter(targetPath);
            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.sourceAvatar = null;
            importer.importAnimation = true;

            ModelImporterClipAnimation[] clips = importer.defaultClipAnimations;
            if (clips.Length == 0)
            {
                clips = importer.clipAnimations;
            }

            if (clips.Length == 0)
            {
                throw new InvalidOperationException($"{targetPath} has no imported clips.");
            }

            for (int i = 0; i < clips.Length; i++)
            {
                clips[i].name = spec.TargetClipName;
                clips[i].loopTime = spec.LoopTime;
                clips[i].keepOriginalOrientation = true;
                clips[i].keepOriginalPositionY = !spec.HeightFromFeet;
                clips[i].keepOriginalPositionXZ = true;
                clips[i].heightFromFeet = spec.HeightFromFeet;
            }

            importer.clipAnimations = clips;
            importer.SaveAndReimport();
        }

        private static AnimatorController BuildAnimatorController()
        {
            EnsureFolder(AnimationRoot);
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(RangedCandidateControllerPath);
            if (controller == null)
            {
                controller = AnimatorController.CreateAnimatorControllerAtPath(RangedCandidateControllerPath);
            }

            ClearParameters(controller);
            controller.AddParameter("MoveSpeed", AnimatorControllerParameterType.Float);
            controller.AddParameter("Attack1", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Reload", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("DodgeForward", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Hit", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Death", AnimatorControllerParameterType.Trigger);

            if (controller.layers.Length == 0)
            {
                controller.AddLayer("Base Layer");
            }

            AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
            ClearStateMachine(stateMachine);
            AnimatorState idle = AddState(stateMachine, "AimIdle", "RG_AimIdle", new Vector3(250f, 80f, 0f));
            AnimatorState move = AddState(stateMachine, "AimMove", "RG_AimWalkForward", new Vector3(250f, 190f, 0f));
            AddState(stateMachine, "Shoot", "RG_Shoot", new Vector3(520f, 80f, 0f));
            AddState(stateMachine, "Reload", "RG_Reload", new Vector3(520f, 190f, 0f));
            AddState(stateMachine, "Evade", "RG_Evade", new Vector3(780f, 80f, 0f));
            AddState(stateMachine, "Hit", "RG_HitUpper", new Vector3(780f, 190f, 0f));
            AddState(stateMachine, "Death", "RG_DieFront", new Vector3(1040f, 80f, 0f));
            stateMachine.defaultState = idle;

            AnimatorStateTransition toMove = idle.AddTransition(move);
            toMove.hasExitTime = false;
            toMove.duration = 0.08f;
            toMove.AddCondition(AnimatorConditionMode.Greater, 0.08f, "MoveSpeed");
            AnimatorStateTransition toIdle = move.AddTransition(idle);
            toIdle.hasExitTime = false;
            toIdle.duration = 0.12f;
            toIdle.AddCondition(AnimatorConditionMode.Less, 0.08f, "MoveSpeed");

            AddAnyStateTrigger(stateMachine, "Shoot", "Attack1");
            AddAnyStateTrigger(stateMachine, "Reload", "Reload");
            AddAnyStateTrigger(stateMachine, "Evade", "DodgeForward");
            AddAnyStateTrigger(stateMachine, "Hit", "Hit");
            AddAnyStateTrigger(stateMachine, "Death", "Death");
            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static AnimatorState AddState(AnimatorStateMachine stateMachine, string stateName, string clipName, Vector3 position)
        {
            AnimatorState state = stateMachine.AddState(stateName, position);
            state.motion = LoadClip(clipName);
            return state;
        }

        private static void AddAnyStateTrigger(AnimatorStateMachine stateMachine, string targetStateName, string triggerName)
        {
            AnimatorState targetState = FindState(stateMachine, targetStateName);
            AnimatorStateTransition transition = stateMachine.AddAnyStateTransition(targetState);
            transition.hasExitTime = false;
            transition.duration = 0.05f;
            transition.AddCondition(AnimatorConditionMode.If, 0f, triggerName);
        }

        private static AnimatorState FindState(AnimatorStateMachine stateMachine, string stateName)
        {
            ChildAnimatorState[] states = stateMachine.states;
            for (int i = 0; i < states.Length; i++)
            {
                if (states[i].state.name == stateName)
                {
                    return states[i].state;
                }
            }

            throw new InvalidOperationException($"Missing Animator state {stateName}.");
        }

        private static AnimationClip LoadClip(string clipName)
        {
            string path = $"{AnimationRoot}/{clipName}.fbx";
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] is AnimationClip clip && clip.name == clipName)
                {
                    return clip;
                }
            }

            AnimationClip fallbackClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (fallbackClip == null)
            {
                throw new InvalidOperationException($"Missing promoted player candidate clip {clipName}.");
            }

            return fallbackClip;
        }

        private static void ClearParameters(AnimatorController controller)
        {
            while (controller.parameters.Length > 0)
            {
                controller.RemoveParameter(controller.parameters[0]);
            }
        }

        private static void ClearStateMachine(AnimatorStateMachine stateMachine)
        {
            ChildAnimatorState[] states = stateMachine.states;
            for (int i = states.Length - 1; i >= 0; i--)
            {
                stateMachine.RemoveState(states[i].state);
            }

            ChildAnimatorStateMachine[] machines = stateMachine.stateMachines;
            for (int i = machines.Length - 1; i >= 0; i--)
            {
                stateMachine.RemoveStateMachine(machines[i].stateMachine);
            }

            AnimatorStateTransition[] transitions = stateMachine.anyStateTransitions;
            for (int i = transitions.Length - 1; i >= 0; i--)
            {
                stateMachine.RemoveAnyStateTransition(transitions[i]);
            }
        }

        private static void ConfigureTextureImporter(string path, TextureUsage usage)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                return;
            }

            importer.textureType = usage == TextureUsage.Normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
            importer.sRGBTexture = usage == TextureUsage.Color;
            importer.SaveAndReimport();
        }

        private static TextureUsage ClassifyTextureUsage(string propertyName)
        {
            string lower = propertyName.ToLowerInvariant();
            if (lower.Contains("normal") || lower.Contains("bump"))
            {
                return TextureUsage.Normal;
            }

            if (lower.Contains("metal") || lower.Contains("spec") || lower.Contains("mask") || lower.Contains("matcap"))
            {
                return TextureUsage.Linear;
            }

            return TextureUsage.Color;
        }

        private static ModelImporter RequireModelImporter(string path)
        {
            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null)
            {
                throw new InvalidOperationException($"Missing model importer at {path}.");
            }

            return importer;
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

            AssetDatabase.CreateFolder(parent, Path.GetFileName(folderPath));
        }

        private static string PathParent(string path)
        {
            return path.Replace('\\', '/').Substring(0, path.Replace('\\', '/').LastIndexOf('/'));
        }

        private readonly struct MaterialSpec
        {
            public MaterialSpec(string sourcePath, string targetPath)
            {
                SourcePath = sourcePath;
                TargetPath = targetPath;
            }

            public string SourcePath { get; }
            public string TargetPath { get; }
        }

        private readonly struct AnimationSpec
        {
            public AnimationSpec(string sourceRelativePath, string targetClipName, bool loopTime, bool heightFromFeet = false)
            {
                SourceRelativePath = sourceRelativePath;
                TargetClipName = targetClipName;
                LoopTime = loopTime;
                HeightFromFeet = heightFromFeet;
            }

            public string SourceRelativePath { get; }
            public string TargetClipName { get; }
            public bool LoopTime { get; }
            public bool HeightFromFeet { get; }
        }

        private enum TextureUsage
        {
            Color,
            Normal,
            Linear
        }
    }
}
