using System;
using System.Collections.Generic;
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
        private const string SourceControllerPath = SourceRoot + "/Animations/Rifle_Controller.controller";
        private const string MaterialRoot = "Assets/_Game/Art/Characters/Player/RifleGirl/Materials";
        private const string TextureRoot = "Assets/_Game/Art/Characters/Player/RifleGirl/Textures";
        private const string AnimationRoot = "Assets/_Game/Art/Animations/Player/RifleGirl";
        private const string ReferenceToonMaterialPath =
            "Assets/_Game/Art/Characters/Player/CombatGirlSwordShield/Materials/DB_CombatGirl_Body.mat";

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
            new AnimationSpec("Aiming/R_Crouch_AimIdle.fbx", "RG_CrouchAimIdle", true),
            new AnimationSpec("Aiming/R_Crouch_AimWalk_F.fbx", "RG_CrouchAimWalkForward", true),
            new AnimationSpec("Aiming/R_Crouch_AimWalk_B.fbx", "RG_CrouchAimWalkBack", true),
            new AnimationSpec("Aiming/R_Crouch_AimWalk_FL.fbx", "RG_CrouchAimWalkForwardLeft", true),
            new AnimationSpec("Aiming/R_Crouch_AimWalk_FR.fbx", "RG_CrouchAimWalkForwardRight", true),
            new AnimationSpec("Aiming/R_Crouch_AimWalk_BL.fbx", "RG_CrouchAimWalkBackLeft", true),
            new AnimationSpec("Aiming/R_Crouch_AimWalk_BR.fbx", "RG_CrouchAimWalkBackRight", true),
            new AnimationSpec("Aiming/R_Crouch_AimTurn_L90.fbx", "RG_CrouchAimTurnLeft90", false),
            new AnimationSpec("Aiming/R_Crouch_AimTurn_R90.fbx", "RG_CrouchAimTurnRight90", false),
            new AnimationSpec("Aiming/R_Crouch_Shoot.fbx", "RG_CrouchShoot", false),
            new AnimationSpec("Aiming/R_Crouch_AutoShoot.fbx", "RG_CrouchAutoShoot", true),
            new AnimationSpec("Aiming/R_Crouch_Reload.fbx", "RG_CrouchReload", false),
            new AnimationSpec("Normal/R_Idle.fbx", "RG_Idle", true),
            new AnimationSpec("Normal/R_Walk.fbx", "RG_Walk", true),
            new AnimationSpec("Normal/R_Run.fbx", "RG_Run", true),
            new AnimationSpec("Normal/R_Crouch_Idle.fbx", "RG_CrouchIdle", true),
            new AnimationSpec("Normal/R_Crouch_Walk.fbx", "RG_CrouchWalk", true),
            new AnimationSpec("Normal/R_Crouch_Jog.fbx", "RG_CrouchJog", true),
            new AnimationSpec("Normal/R_TakeGun.fbx", "RG_DrawRangedFocus", false),
            new AnimationSpec("Normal/R_PutGun.fbx", "RG_HolsterRangedFocus", false),
            new AnimationSpec("Normal/R_Evade.fbx", "RG_Evade", false),
            new AnimationSpec("Normal/R_Hit_Upper.fbx", "RG_HitUpper", false),
            new AnimationSpec("Normal/R_Hit_Low.fbx", "RG_HitLow", false),
            new AnimationSpec("Normal/R_Stun.fbx", "RG_Stun", false),
            new AnimationSpec("Normal/R_Die_F.fbx", "RG_DieFront", false, true),
            new AnimationSpec("Normal/R_Die_B.fbx", "RG_DieBack", false, true)
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
                ModelImporter sourceImporter = RequireModelImporter(sourcePath);
                importer.animationType = ModelImporterAnimationType.Human;
                importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                importer.humanDescription = sourceImporter.humanDescription;
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

            targetMaterial.shader = ResolvePlayerToonShader() ?? sourceMaterial.shader;
            string[] textureProperties = sourceMaterial.GetTexturePropertyNames();
            for (int i = 0; i < textureProperties.Length; i++)
            {
                CopyTextureProperty(sourceMaterial, targetMaterial, textureProperties[i]);
            }

            CopyMainTextureToCommonBaseSlots(sourceMaterial, targetMaterial);
            EditorUtility.SetDirty(targetMaterial);
        }

        private static Shader ResolvePlayerToonShader()
        {
            Material referenceMaterial = AssetDatabase.LoadAssetAtPath<Material>(ReferenceToonMaterialPath);
            if (referenceMaterial != null &&
                referenceMaterial.shader != null &&
                !string.Equals(referenceMaterial.shader.name, "Hidden/InternalErrorShader", StringComparison.Ordinal))
            {
                return referenceMaterial.shader;
            }

            return Shader.Find("UnityChanToonShader/Toon_DoubleShadeWithFeather")
                ?? Shader.Find("UnityChanToonShader/Toon_DoubleShadeWithFeather_Clipping")
                ?? Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("Standard");
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

        private static void CopyMainTextureToCommonBaseSlots(Material sourceMaterial, Material targetMaterial)
        {
            if (!sourceMaterial.HasProperty("_MainTex"))
            {
                return;
            }

            Texture sourceTexture = sourceMaterial.GetTexture("_MainTex");
            if (sourceTexture == null)
            {
                return;
            }

            Texture promotedTexture = PromoteTexture(sourceTexture, TextureUsage.Color);
            SetTextureIfPresent(targetMaterial, "_BaseMap", promotedTexture);
            SetTextureIfPresent(targetMaterial, "_1st_ShadeMap", promotedTexture);
        }

        private static void SetTextureIfPresent(Material material, string propertyName, Texture texture)
        {
            if (material.HasProperty(propertyName))
            {
                material.SetTexture(propertyName, texture);
            }
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
            importer.avatarSetup = ModelImporterAvatarSetup.CopyFromOther;
            importer.sourceAvatar = LoadRangedCandidateAvatar();
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
                ApplySourceAnimationClipSettings(clips[i], sourcePath, i, spec);
            }

            importer.clipAnimations = clips;
            importer.SaveAndReimport();
        }

        private static void ApplySourceAnimationClipSettings(
            ModelImporterClipAnimation targetClip,
            string sourcePath,
            int clipIndex,
            AnimationSpec spec)
        {
            ModelImporterClipAnimation sourceClip = ResolveSourceAnimationClip(sourcePath, clipIndex);
            targetClip.name = spec.TargetClipName;
            targetClip.keepOriginalOrientation = sourceClip.keepOriginalOrientation;
            targetClip.keepOriginalPositionY = sourceClip.keepOriginalPositionY;
            targetClip.keepOriginalPositionXZ = sourceClip.keepOriginalPositionXZ;
            targetClip.heightFromFeet = sourceClip.heightFromFeet;
            targetClip.loopTime = spec.LoopTime;
            targetClip.loopPose = sourceClip.loopPose;
            targetClip.maskType = sourceClip.maskType;
            targetClip.maskSource = sourceClip.maskSource;
            targetClip.events = CopyAnimationEvents(sourceClip.events);
        }

        private static AnimationEvent[] CopyAnimationEvents(AnimationEvent[] sourceEvents)
        {
            if (sourceEvents == null || sourceEvents.Length == 0)
            {
                return Array.Empty<AnimationEvent>();
            }

            AnimationEvent[] copiedEvents = new AnimationEvent[sourceEvents.Length];
            for (int i = 0; i < sourceEvents.Length; i++)
            {
                AnimationEvent sourceEvent = sourceEvents[i];
                copiedEvents[i] = new AnimationEvent
                {
                    time = sourceEvent.time,
                    functionName = sourceEvent.functionName,
                    stringParameter = sourceEvent.stringParameter,
                    floatParameter = sourceEvent.floatParameter,
                    intParameter = sourceEvent.intParameter,
                    objectReferenceParameter = sourceEvent.objectReferenceParameter,
                    messageOptions = sourceEvent.messageOptions
                };
            }

            return copiedEvents;
        }

        private static Avatar LoadRangedCandidateAvatar()
        {
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(RangedCandidateModelPath);
            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] is Avatar avatar && avatar.isHuman && avatar.isValid)
                {
                    return avatar;
                }
            }

            throw new InvalidOperationException($"Missing valid promoted RifleGirl humanoid Avatar at {RangedCandidateModelPath}.");
        }

        private static ModelImporterClipAnimation ResolveSourceAnimationClip(string sourcePath, int clipIndex)
        {
            ModelImporter sourceImporter = RequireModelImporter(sourcePath);
            ModelImporterClipAnimation[] sourceClips = sourceImporter.clipAnimations;
            if (sourceClips.Length == 0)
            {
                sourceClips = sourceImporter.defaultClipAnimations;
            }

            if (sourceClips.Length == 0)
            {
                throw new InvalidOperationException($"{sourcePath} has no source clips to promote.");
            }

            int sourceIndex = Mathf.Clamp(clipIndex, 0, sourceClips.Length - 1);
            return sourceClips[sourceIndex];
        }

        private static AnimatorController BuildAnimatorController()
        {
            EnsureFolder(AnimationRoot);
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(RangedCandidateControllerPath);
            if (ShouldReplaceControllerWithNativeSource(controller))
            {
                if (controller != null && !AssetDatabase.DeleteAsset(RangedCandidateControllerPath))
                {
                    throw new InvalidOperationException($"Failed to replace RifleGirl candidate controller at {RangedCandidateControllerPath}.");
                }

                if (!AssetDatabase.CopyAsset(SourceControllerPath, RangedCandidateControllerPath))
                {
                    throw new InvalidOperationException($"Failed to promote RifleGirl native controller from {SourceControllerPath}.");
                }

                controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(RangedCandidateControllerPath);
            }

            if (controller == null)
            {
                throw new InvalidOperationException($"Missing promoted RifleGirl candidate controller at {RangedCandidateControllerPath}.");
            }

            controller.name = Path.GetFileNameWithoutExtension(RangedCandidateControllerPath);
            RemapControllerMotions(controller);

            AnimatorControllerLayer[] layers = controller.layers;
            if (layers.Length > 0)
            {
                layers[0].iKPass = true;
            }

            controller.layers = layers;
            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static bool ShouldReplaceControllerWithNativeSource(AnimatorController controller)
        {
            if (controller == null)
            {
                return true;
            }

            return !HasParameter(controller, "IDLE 0")
                || !HasParameter(controller, "SHOOT")
                || !HasParameter(controller, "WALK F");
        }

        private static bool HasParameter(AnimatorController controller, string parameterName)
        {
            AnimatorControllerParameter[] parameters = controller.parameters;
            for (int i = 0; i < parameters.Length; i++)
            {
                if (string.Equals(parameters[i].name, parameterName, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static void RemapControllerMotions(AnimatorController controller)
        {
            Dictionary<string, AnimationClip> promotedClipsBySourcePath = BuildPromotedClipMap();
            AnimatorControllerLayer[] layers = controller.layers;
            for (int i = 0; i < layers.Length; i++)
            {
                RemapStateMachineMotions(layers[i].stateMachine, promotedClipsBySourcePath);
            }
        }

        private static Dictionary<string, AnimationClip> BuildPromotedClipMap()
        {
            var clips = new Dictionary<string, AnimationClip>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < AnimationSpecs.Length; i++)
            {
                AnimationSpec spec = AnimationSpecs[i];
                clips[NormalizeAssetPath(SourceAnimationPath(spec))] = LoadClip(spec.TargetClipName);
            }

            return clips;
        }

        private static void RemapStateMachineMotions(
            AnimatorStateMachine stateMachine,
            Dictionary<string, AnimationClip> promotedClipsBySourcePath)
        {
            ChildAnimatorState[] states = stateMachine.states;
            for (int i = 0; i < states.Length; i++)
            {
                AnimatorState state = states[i].state;
                state.motion = RemapMotion(state.motion, promotedClipsBySourcePath, state.name);
                EditorUtility.SetDirty(state);
            }

            ChildAnimatorStateMachine[] machines = stateMachine.stateMachines;
            for (int i = 0; i < machines.Length; i++)
            {
                RemapStateMachineMotions(machines[i].stateMachine, promotedClipsBySourcePath);
            }
        }

        private static Motion RemapMotion(
            Motion motion,
            Dictionary<string, AnimationClip> promotedClipsBySourcePath,
            string ownerName)
        {
            if (motion == null)
            {
                return null;
            }

            if (motion is BlendTree blendTree)
            {
                RemapBlendTree(blendTree, promotedClipsBySourcePath, ownerName);
                return blendTree;
            }

            if (motion is AnimationClip clip)
            {
                string clipPath = NormalizeAssetPath(AssetDatabase.GetAssetPath(clip));
                if (string.IsNullOrWhiteSpace(clipPath) || !clipPath.StartsWith("Assets/_Imported/", StringComparison.OrdinalIgnoreCase))
                {
                    return clip;
                }

                if (promotedClipsBySourcePath.TryGetValue(clipPath, out AnimationClip promotedClip))
                {
                    return promotedClip;
                }

                throw new InvalidOperationException(
                    $"RifleGirl native controller state {ownerName} references unpromoted raw clip {clipPath}.");
            }

            return motion;
        }

        private static void RemapBlendTree(
            BlendTree blendTree,
            Dictionary<string, AnimationClip> promotedClipsBySourcePath,
            string ownerName)
        {
            ChildMotion[] children = blendTree.children;
            bool changed = false;
            for (int i = 0; i < children.Length; i++)
            {
                Motion remappedMotion = RemapMotion(children[i].motion, promotedClipsBySourcePath, ownerName);
                if (remappedMotion != children[i].motion)
                {
                    children[i].motion = remappedMotion;
                    changed = true;
                }
            }

            if (changed)
            {
                blendTree.children = children;
                EditorUtility.SetDirty(blendTree);
            }
        }

        private static string SourceAnimationPath(AnimationSpec spec)
        {
            return $"{SourceRoot}/Animations/{spec.SourceRelativePath}";
        }

        private static string NormalizeAssetPath(string path)
        {
            return string.IsNullOrWhiteSpace(path) ? string.Empty : path.Replace('\\', '/');
        }

        private static void AddExitTransition(
            AnimatorState fromState,
            AnimatorState toState,
            float exitTime,
            float duration)
        {
            AnimatorStateTransition transition = fromState.AddTransition(toState);
            transition.hasExitTime = true;
            transition.exitTime = exitTime;
            transition.duration = duration;
        }

        private static void AddBoolTransition(
            AnimatorState fromState,
            AnimatorState toState,
            string parameterName,
            bool expectedValue,
            float duration)
        {
            AnimatorStateTransition transition = fromState.AddTransition(toState);
            transition.hasExitTime = false;
            transition.duration = duration;
            transition.AddCondition(
                expectedValue ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot,
                0f,
                parameterName);
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
