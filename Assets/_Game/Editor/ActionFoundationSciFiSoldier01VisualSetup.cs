using System;
using System.Collections.Generic;
using System.Linq;
using DimensionBrawl.Enemies;
using DimensionBrawl.Presentation;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace DimensionBrawl.Editor
{
    public static class ActionFoundationSciFiSoldier01VisualSetup
    {
        public const string VisualName = "SciFiSoldier01_GeneralDeckVisual";
        public const string AssaultRifleName = "SciFiSoldier01_AssaultRifle";
        public const string ModelPath = "Assets/_Game/Art/Characters/Enemies/SciFiSoldiers/SciFiSoldier01/Models/SK_SciFiSoldier01.fbx";
        public const string AssaultRifleModelPath = "Assets/_Game/Art/Characters/Enemies/SciFiSoldiers/SciFiSoldier01/Weapons/SM_SciFiAssaultRifle_01.fbx";
        public const string ControllerPath = "Assets/_Game/Art/Animations/Enemies/SciFiSoldiers/SciFiSoldier01/DB_SciFiSoldier01_GeneralDeck.controller";

        private const string OldMaintenanceVisualName = "MaintenanceWorker_BasicSoldierVisual";
        private const string SourceModelPath = "Assets/_Imported/AssetStore/Protofactor/Sci Fi/SciFiCharactersMegaPackVol3/SciFiShooterCharactersPackVol3/SciFiSoldier_01/FBX Files/SK_SciFiSoldier_01.fbx";
        private const string SourceVariantPrefabPath = "Assets/_Imported/AssetStore/Protofactor/Sci Fi/SciFiCharactersMegaPackVol3/SciFiShooterCharactersPackVol3/SciFiSoldier_01/Prefabs/SciFiSoldier_01_Commando.prefab";
        private const string SourceAssaultRifleModelPath = "Assets/_Imported/AssetStore/Protofactor/Sci Fi/Common/Weapons/FBX Files/SM_SciFiAssaultRifle_01.FBX";
        private const string SourceAssaultRifleMaterialPath = "Assets/_Imported/AssetStore/Protofactor/Sci Fi/Common/Weapons/Materials/M_AssaultRifle.mat";
        private const string SourceAnimationRoot = "Assets/_Imported/AssetStore/Protofactor/Sci Fi/Common/Animations";
        private const string MaterialRoot = "Assets/_Game/Art/Characters/Enemies/SciFiSoldiers/SciFiSoldier01/Materials";
        private const string TextureRoot = "Assets/_Game/Art/Characters/Enemies/SciFiSoldiers/SciFiSoldier01/Textures";
        private const string WeaponRoot = "Assets/_Game/Art/Characters/Enemies/SciFiSoldiers/SciFiSoldier01/Weapons";
        private const string WeaponMaterialRoot = WeaponRoot + "/Materials";
        private const string WeaponTextureRoot = WeaponRoot + "/Textures";
        private const string AnimationRoot = "Assets/_Game/Art/Animations/Enemies/SciFiSoldiers/SciFiSoldier01";

        private const string IdleClipPath = AnimationRoot + "/S01_IdleAimAssaultRifle.fbx";
        private const string RunClipPath = AnimationRoot + "/S01_RunForwardAssaultRifle.fbx";
        private const string WalkClipPath = AnimationRoot + "/S01_WalkForwardAssaultRifle.fbx";
        private const string ShootPrimaryClipPath = AnimationRoot + "/S01_ShootPrimaryAssaultRifle.fbx";
        private const string ShootSecondaryClipPath = AnimationRoot + "/S01_ShootSecondaryAssaultRifle.fbx";
        private const string ComboClipPath = AnimationRoot + "/S01_ComboAssaultRifle.fbx";
        private const string CrouchAimClipPath = AnimationRoot + "/S01_CrouchAimAssaultRifle.fbx";
        private const string CrouchForwardClipPath = AnimationRoot + "/S01_CrouchForwardAssaultRifle.fbx";
        private const string HitClipPath = AnimationRoot + "/S01_GetHitFrontLight.fbx";
        private const string HitHeavyClipPath = AnimationRoot + "/S01_GetHitFrontHeavy.fbx";
        private const string DeathClipPath = AnimationRoot + "/S01_DeathFront.fbx";

        [MenuItem("DimensionBrawl/Reapply Action Foundation SciFiSoldier01 GeneralDeck Visual Assets")]
        public static void ReapplyGeneralDeckVisualAssetsMenu()
        {
            EnsureGeneralDeckVisualAssets();
            Debug.Log("Reapplied SciFiSoldier01 GeneralDeck visual assets.");
        }

        public static void EnsureGeneralDeckVisualAssets()
        {
            PromoteModel();
            PromoteAssaultRifle();
            PromoteAnimationClip("Humanoid@IdleAimAssaultRifle.FBX", IdleClipPath);
            PromoteAnimationClip("Humanoid@RunForwardAssaultRifle.FBX", RunClipPath);
            PromoteAnimationClip("Humanoid@WalkForwardAssaultRifle.FBX", WalkClipPath);
            PromoteAnimationClip("Humanoid@ShootPrimaryAssaultRifle.FBX", ShootPrimaryClipPath);
            PromoteAnimationClip("Humanoid@ShootSecondaryAssaultRifle.FBX", ShootSecondaryClipPath);
            PromoteAnimationClip("Humanoid@2HitComboAssaultRifle.FBX", ComboClipPath);
            PromoteAnimationClip("Humanoid@CrouchAimAssaultRifle.FBX", CrouchAimClipPath);
            PromoteAnimationClip("Humanoid@CrouchForwardAssaultRifle.FBX", CrouchForwardClipPath);
            PromoteAnimationClip("Humanoid@GetHitFrontLightUnarmed.FBX", HitClipPath);
            PromoteAnimationClip("Humanoid@GetHitFrontHeavyUnarmed.FBX", HitHeavyClipPath);
            PromoteAnimationClip("Humanoid@DeathFrontUnarmed.FBX", DeathClipPath);

            ConfigureModelImporter(ModelPath);
            ConfigureWeaponModelImporter(AssaultRifleModelPath);
            Avatar avatar = LoadAvatar(ModelPath);
            ConfigureAnimationImporter(IdleClipPath, "S01_IdleAimAssaultRifle", true, avatar);
            ConfigureAnimationImporter(RunClipPath, "S01_RunForwardAssaultRifle", true, avatar);
            ConfigureAnimationImporter(WalkClipPath, "S01_WalkForwardAssaultRifle", true, avatar);
            ConfigureAnimationImporter(ShootPrimaryClipPath, "S01_ShootPrimaryAssaultRifle", false, avatar);
            ConfigureAnimationImporter(ShootSecondaryClipPath, "S01_ShootSecondaryAssaultRifle", false, avatar);
            ConfigureAnimationImporter(ComboClipPath, "S01_ComboAssaultRifle", false, avatar);
            ConfigureAnimationImporter(CrouchAimClipPath, "S01_CrouchAimAssaultRifle", false, avatar);
            ConfigureAnimationImporter(CrouchForwardClipPath, "S01_CrouchForwardAssaultRifle", false, avatar);
            ConfigureAnimationImporter(HitClipPath, "S01_GetHitFrontLight", false, avatar);
            ConfigureAnimationImporter(HitHeavyClipPath, "S01_GetHitFrontHeavy", false, avatar);
            ConfigureAnimationImporter(DeathClipPath, "S01_DeathFront", false, avatar, true);

            BuildAnimatorController();
        }

        public static void ApplyGeneralDeckVisual(GameObject root)
        {
            EnsureGeneralDeckVisualAssets();

            BasicSoldierEnemy soldier = RequireComponent<BasicSoldierEnemy>(root, root.name);
            EnemyAttackTelegraphPresenter telegraphPresenter = RequireComponent<EnemyAttackTelegraphPresenter>(root, root.name);
            CombatHitFeedback hitFeedback = RequireComponent<CombatHitFeedback>(root, root.name);

            RemoveVisualChild(root.transform, VisualName);
            RemoveVisualChild(root.transform, OldMaintenanceVisualName);

            GameObject visual = RecreateVisual(root.transform);
            ReapplyPromotedMaterials(visual);
            GameObject assaultRifle = AttachAssaultRifle(visual);
            Animator animator = EnsureAnimator(visual, LoadController());
            Renderer[] renderers = CollectPresentableRenderers(visual);
            if (renderers.Length == 0)
            {
                throw new InvalidOperationException($"{VisualName} has no renderers for GeneralDeck hit feedback.");
            }

            Renderer bodyRenderer = renderers.FirstOrDefault(renderer => !IsDescendantOf(renderer.transform, assaultRifle.transform)) ?? renderers[0];
            SetObjectReference(soldier, "animator", animator);
            SetObjectReference(soldier, "bodyRenderer", bodyRenderer);
            SetBool(soldier, "usePrototypeBodyColors", false);
            SetObjectReference(telegraphPresenter, "poseRoot", visual.transform);
            SetObjectReferenceArray(hitFeedback, "flashRenderers", renderers.Cast<UnityEngine.Object>().ToArray());
            SetBool(hitFeedback, "applyIdleColorOnEnable", false);

            EditorUtility.SetDirty(root);
        }

        public static void ValidateGeneralDeckVisual(GameObject root)
        {
            Transform visual = root.transform.Find(VisualName);
            if (visual == null)
            {
                throw new InvalidOperationException($"{root.name} should contain {VisualName}.");
            }

            if (FindVisualChild(root.transform, OldMaintenanceVisualName) != null)
            {
                throw new InvalidOperationException($"{root.name} should not keep the old MaintenanceWorker visual in the GeneralDeck prefab.");
            }

            Animator animator = visual.GetComponent<Animator>();
            if (animator == null)
            {
                throw new InvalidOperationException($"{VisualName} needs an Animator.");
            }

            string controllerPath = AssetDatabase.GetAssetPath(animator.runtimeAnimatorController).Replace('\\', '/');
            if (!string.Equals(controllerPath, ControllerPath, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"{VisualName} should use {ControllerPath}, found {controllerPath}.");
            }

            Renderer[] renderers = CollectPresentableRenderers(visual.gameObject);
            if (renderers.Length == 0)
            {
                throw new InvalidOperationException($"{VisualName} should keep promoted presentable renderers.");
            }

            Transform assaultRifle = FindDescendant(visual, AssaultRifleName);
            if (assaultRifle == null)
            {
                throw new InvalidOperationException($"{VisualName} should carry a promoted {AssaultRifleName} child.");
            }

            ValidatePrefabSourcePath(assaultRifle.gameObject, AssaultRifleModelPath);
            ValidateRendererAssets(renderers);

            BasicSoldierEnemy soldier = RequireComponent<BasicSoldierEnemy>(root, root.name);
            EnemyAttackTelegraphPresenter telegraphPresenter = RequireComponent<EnemyAttackTelegraphPresenter>(root, root.name);
            CombatHitFeedback hitFeedback = RequireComponent<CombatHitFeedback>(root, root.name);

            ValidateObjectReference(soldier, "animator", animator);
            ValidateObjectReference(telegraphPresenter, "poseRoot", visual);
            ValidateFlashRenderers(root, hitFeedback, renderers);
        }

        private static void PromoteModel()
        {
            EnsureFolder("Assets/_Game/Art/Characters");
            EnsureFolder("Assets/_Game/Art/Characters/Enemies");
            EnsureFolder("Assets/_Game/Art/Characters/Enemies/SciFiSoldiers");
            EnsureFolder("Assets/_Game/Art/Characters/Enemies/SciFiSoldiers/SciFiSoldier01");
            EnsureFolder("Assets/_Game/Art/Characters/Enemies/SciFiSoldiers/SciFiSoldier01/Models");

            if (AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath) != null)
            {
                return;
            }

            if (!AssetDatabase.CopyAsset(SourceModelPath, ModelPath))
            {
                throw new InvalidOperationException($"Failed to promote SciFiSoldier01 model from {SourceModelPath} to {ModelPath}.");
            }
        }

        private static void PromoteAssaultRifle()
        {
            EnsureFolder(WeaponRoot);

            if (AssetDatabase.LoadAssetAtPath<GameObject>(AssaultRifleModelPath) == null &&
                !AssetDatabase.CopyAsset(SourceAssaultRifleModelPath, AssaultRifleModelPath))
            {
                throw new InvalidOperationException($"Failed to promote assault rifle model from {SourceAssaultRifleModelPath} to {AssaultRifleModelPath}.");
            }

            Material sourceMaterial = AssetDatabase.LoadAssetAtPath<Material>(SourceAssaultRifleMaterialPath);
            if (sourceMaterial == null)
            {
                throw new InvalidOperationException($"Missing source assault rifle material at {SourceAssaultRifleMaterialPath}.");
            }

            PromoteWeaponMaterial(sourceMaterial);
        }

        private static void PromoteAnimationClip(string sourceFileName, string targetPath)
        {
            EnsureFolder(AnimationRoot);
            if (AssetDatabase.LoadAssetAtPath<AnimationClip>(targetPath) != null)
            {
                return;
            }

            string sourcePath = $"{SourceAnimationRoot}/{sourceFileName}";
            if (!AssetDatabase.CopyAsset(sourcePath, targetPath))
            {
                throw new InvalidOperationException($"Failed to promote SciFiSoldier01 animation from {sourcePath} to {targetPath}.");
            }
        }

        private static void ConfigureModelImporter(string path)
        {
            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null)
            {
                throw new InvalidOperationException($"Missing model importer at {path}.");
            }

            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.importAnimation = false;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.SaveAndReimport();
        }

        private static void ConfigureWeaponModelImporter(string path)
        {
            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null)
            {
                throw new InvalidOperationException($"Missing weapon model importer at {path}.");
            }

            importer.importAnimation = false;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.SaveAndReimport();
        }

        private static void ConfigureAnimationImporter(string path, string clipName, bool loopTime, Avatar avatar, bool heightFromFeet = false)
        {
            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null)
            {
                throw new InvalidOperationException($"Missing animation importer at {path}.");
            }

            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = ModelImporterAvatarSetup.CopyFromOther;
            importer.sourceAvatar = avatar;
            importer.importAnimation = true;

            ModelImporterClipAnimation[] clips = importer.defaultClipAnimations;
            if (clips.Length == 0)
            {
                clips = importer.clipAnimations;
            }

            if (clips.Length == 0)
            {
                throw new InvalidOperationException($"{path} has no imported clips.");
            }

            for (int i = 0; i < clips.Length; i++)
            {
                clips[i].name = clipName;
                clips[i].loopTime = loopTime;
                clips[i].keepOriginalOrientation = true;
                clips[i].keepOriginalPositionY = !heightFromFeet;
                clips[i].keepOriginalPositionXZ = true;
                clips[i].heightFromFeet = heightFromFeet;
            }

            importer.clipAnimations = clips;
            importer.SaveAndReimport();
        }

        private static AnimatorController BuildAnimatorController()
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            if (controller == null)
            {
                EnsureFolder(AnimationRoot);
                controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            }

            ClearParameters(controller);
            controller.AddParameter("MoveSpeed", AnimatorControllerParameterType.Float);
            controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("AttackCombo2", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("AttackCombo3", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("AttackHeavy", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("AttackLinePressure", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("AttackFanPressure", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("AttackRetreatShot", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("AttackRetreatBlink", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("AttackGuardBreak", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("RetreatBackstep", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("RetreatBlink", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("EliteShieldCycle", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("EliteArmorBreak", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("EliteAuraBuffer", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("EliteSummonPackage", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("ElitePhaseSwap", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Hit", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("HitHeavy", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Death", AnimatorControllerParameterType.Trigger);

            AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
            ClearStateMachine(stateMachine);

            AnimatorState idle = AddState(stateMachine, "IdleAim", IdleClipPath, new Vector3(250f, 80f, 0f));
            AnimatorState run = AddState(stateMachine, "RunForward", RunClipPath, new Vector3(250f, 170f, 0f));
            AnimatorState attack = AddState(stateMachine, "Attack", ShootPrimaryClipPath, new Vector3(520f, 80f, 0f));
            AnimatorState attackCombo2 = AddState(stateMachine, "AttackCombo2", ComboClipPath, new Vector3(520f, 170f, 0f), 1.05f);
            AnimatorState attackCombo3 = AddState(stateMachine, "AttackCombo3", ShootSecondaryClipPath, new Vector3(520f, 260f, 0f));
            AnimatorState attackHeavy = AddState(stateMachine, "AttackHeavy", ComboClipPath, new Vector3(520f, 350f, 0f), 0.82f);
            AnimatorState attackLinePressure = AddState(stateMachine, "AttackLinePressure", ShootPrimaryClipPath, new Vector3(800f, 80f, 0f), 0.92f);
            AnimatorState attackFanPressure = AddState(stateMachine, "AttackFanPressure", ShootSecondaryClipPath, new Vector3(800f, 170f, 0f), 0.96f);
            AnimatorState attackRetreatShot = AddState(stateMachine, "AttackRetreatShot", ShootPrimaryClipPath, new Vector3(800f, 260f, 0f), 1.08f);
            AnimatorState attackRetreatBlink = AddState(stateMachine, "AttackRetreatBlink", ShootSecondaryClipPath, new Vector3(800f, 350f, 0f), 1.12f);
            AnimatorState attackGuardBreak = AddState(stateMachine, "AttackGuardBreak", ComboClipPath, new Vector3(800f, 440f, 0f), 0.74f);
            AnimatorState retreatBackstep = AddState(stateMachine, "RetreatBackstep", WalkClipPath, new Vector3(1080f, 80f, 0f), 1.18f);
            AnimatorState retreatBlink = AddState(stateMachine, "RetreatBlink", CrouchForwardClipPath, new Vector3(1080f, 170f, 0f), 1.42f);
            AnimatorState eliteShieldCycle = AddState(stateMachine, "EliteShieldCycle", CrouchAimClipPath, new Vector3(1080f, 260f, 0f));
            AnimatorState eliteArmorBreak = AddState(stateMachine, "EliteArmorBreak", HitHeavyClipPath, new Vector3(1080f, 350f, 0f));
            AnimatorState eliteAuraBuffer = AddState(stateMachine, "EliteAuraBuffer", CrouchAimClipPath, new Vector3(1080f, 440f, 0f), 0.9f);
            AnimatorState eliteSummonPackage = AddState(stateMachine, "EliteSummonPackage", CrouchAimClipPath, new Vector3(1360f, 80f, 0f), 0.82f);
            AnimatorState elitePhaseSwap = AddState(stateMachine, "ElitePhaseSwap", CrouchForwardClipPath, new Vector3(1360f, 170f, 0f), 0.95f);
            AnimatorState hit = AddState(stateMachine, "Hit", HitClipPath, new Vector3(1360f, 260f, 0f));
            AnimatorState hitHeavy = AddState(stateMachine, "HitHeavy", HitHeavyClipPath, new Vector3(1360f, 350f, 0f));
            AnimatorState death = AddState(stateMachine, "Death", DeathClipPath, new Vector3(1360f, 440f, 0f));
            stateMachine.defaultState = idle;

            AddMoveTransition(idle, run, AnimatorConditionMode.Greater, 0.1f);
            AddMoveTransition(run, idle, AnimatorConditionMode.Less, 0.1f);
            AddAnyTriggerTransition(stateMachine, death, "Death", 0.05f);
            AddAnyTriggerTransition(stateMachine, hit, "Hit", 0.03f);
            AddAnyTriggerTransition(stateMachine, hitHeavy, "HitHeavy", 0.03f);
            AddAnyTriggerTransition(stateMachine, attack, "Attack", 0.04f);
            AddAnyTriggerTransition(stateMachine, attackCombo2, "AttackCombo2", 0.04f);
            AddAnyTriggerTransition(stateMachine, attackCombo3, "AttackCombo3", 0.04f);
            AddAnyTriggerTransition(stateMachine, attackHeavy, "AttackHeavy", 0.06f);
            AddAnyTriggerTransition(stateMachine, attackLinePressure, "AttackLinePressure", 0.04f);
            AddAnyTriggerTransition(stateMachine, attackFanPressure, "AttackFanPressure", 0.04f);
            AddAnyTriggerTransition(stateMachine, attackRetreatShot, "AttackRetreatShot", 0.04f);
            AddAnyTriggerTransition(stateMachine, attackRetreatBlink, "AttackRetreatBlink", 0.04f);
            AddAnyTriggerTransition(stateMachine, attackGuardBreak, "AttackGuardBreak", 0.06f);
            AddAnyTriggerTransition(stateMachine, retreatBackstep, "RetreatBackstep", 0.04f);
            AddAnyTriggerTransition(stateMachine, retreatBlink, "RetreatBlink", 0.04f);
            AddAnyTriggerTransition(stateMachine, eliteShieldCycle, "EliteShieldCycle", 0.05f);
            AddAnyTriggerTransition(stateMachine, eliteArmorBreak, "EliteArmorBreak", 0.03f);
            AddAnyTriggerTransition(stateMachine, eliteAuraBuffer, "EliteAuraBuffer", 0.05f);
            AddAnyTriggerTransition(stateMachine, eliteSummonPackage, "EliteSummonPackage", 0.05f);
            AddAnyTriggerTransition(stateMachine, elitePhaseSwap, "ElitePhaseSwap", 0.05f);

            AddExitTransition(attack, idle, 0.82f, 0.08f);
            AddExitTransition(attackCombo2, idle, 0.86f, 0.08f);
            AddExitTransition(attackCombo3, idle, 0.82f, 0.08f);
            AddExitTransition(attackHeavy, idle, 0.92f, 0.08f);
            AddExitTransition(attackLinePressure, idle, 0.76f, 0.08f);
            AddExitTransition(attackFanPressure, idle, 0.76f, 0.08f);
            AddExitTransition(attackRetreatShot, idle, 0.72f, 0.08f);
            AddExitTransition(attackRetreatBlink, idle, 0.68f, 0.08f);
            AddExitTransition(attackGuardBreak, idle, 0.92f, 0.08f);
            AddExitTransition(retreatBackstep, idle, 0.8f, 0.06f);
            AddExitTransition(retreatBlink, idle, 0.74f, 0.05f);
            AddExitTransition(eliteShieldCycle, idle, 0.82f, 0.06f);
            AddExitTransition(eliteArmorBreak, idle, 0.88f, 0.06f);
            AddExitTransition(eliteAuraBuffer, idle, 0.72f, 0.06f);
            AddExitTransition(eliteSummonPackage, idle, 0.88f, 0.06f);
            AddExitTransition(elitePhaseSwap, idle, 0.82f, 0.06f);
            AddExitTransition(hit, idle, 0.88f, 0.06f);
            AddExitTransition(hitHeavy, idle, 0.9f, 0.06f);

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            return controller;
        }

        private static void RemoveVisualChild(Transform parent, string childName)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Transform child = parent.GetChild(i);
                if (child.name == childName || child.name.StartsWith(childName + "_", StringComparison.Ordinal))
                {
                    UnityEngine.Object.DestroyImmediate(child.gameObject);
                }
            }
        }

        private static Transform FindVisualChild(Transform parent, string childName)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (child.name == childName || child.name.StartsWith(childName + "_", StringComparison.Ordinal))
                {
                    return child;
                }
            }

            return null;
        }

        private static GameObject RecreateVisual(Transform parent)
        {
            GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
            if (model == null)
            {
                throw new InvalidOperationException($"Missing promoted SciFiSoldier01 model at {ModelPath}.");
            }

            GameObject visual = PrefabUtility.InstantiatePrefab(model, parent) as GameObject;
            if (visual == null)
            {
                throw new InvalidOperationException("Failed to instantiate SciFiSoldier01 visual.");
            }

            visual.name = VisualName;
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = Vector3.one;
            return visual;
        }

        private static GameObject AttachAssaultRifle(GameObject visual)
        {
            Transform socket = FindDescendant(visual.transform, "RefPosAssaultRifle_Action");
            if (socket == null)
            {
                throw new InvalidOperationException($"{VisualName} is missing RefPosAssaultRifle_Action for authored rifle placement.");
            }

            GameObject weaponAsset = AssetDatabase.LoadAssetAtPath<GameObject>(AssaultRifleModelPath);
            if (weaponAsset == null)
            {
                throw new InvalidOperationException($"Missing promoted assault rifle model at {AssaultRifleModelPath}.");
            }

            GameObject weapon = PrefabUtility.InstantiatePrefab(weaponAsset, socket) as GameObject;
            if (weapon == null)
            {
                throw new InvalidOperationException("Failed to instantiate promoted assault rifle visual.");
            }

            weapon.name = AssaultRifleName;
            weapon.transform.localPosition = Vector3.zero;
            weapon.transform.localRotation = Quaternion.identity;
            weapon.transform.localScale = Vector3.one;
            AssignAssaultRifleMaterial(weapon);
            return weapon;
        }

        private static void ReapplyPromotedMaterials(GameObject visual)
        {
            GameObject sourcePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SourceVariantPrefabPath);
            if (sourcePrefab == null)
            {
                throw new InvalidOperationException($"Missing source SciFiSoldier01 material variant at {SourceVariantPrefabPath}.");
            }

            Dictionary<string, Queue<Renderer>> sourceRenderersByName = CollectPresentableRenderers(sourcePrefab)
                .GroupBy(renderer => renderer.name)
                .ToDictionary(group => group.Key, group => new Queue<Renderer>(group));

            Renderer[] targetRenderers = visual.GetComponentsInChildren<Renderer>(true);
            int reassignedCount = 0;
            for (int i = 0; i < targetRenderers.Length; i++)
            {
                Renderer targetRenderer = targetRenderers[i];
                if (!sourceRenderersByName.TryGetValue(targetRenderer.name, out Queue<Renderer> sourceQueue) || sourceQueue.Count == 0)
                {
                    DisableUnusedVariantRenderer(targetRenderer);
                    continue;
                }

                Material[] promotedMaterials = PromoteMaterials(sourceQueue.Dequeue().sharedMaterials);
                targetRenderer.gameObject.SetActive(true);
                targetRenderer.enabled = true;
                targetRenderer.sharedMaterials = promotedMaterials;
                EditorUtility.SetDirty(targetRenderer.gameObject);
                EditorUtility.SetDirty(targetRenderer);
                reassignedCount += promotedMaterials.Length;
            }

            if (reassignedCount == 0)
            {
                throw new InvalidOperationException($"{VisualName} did not match any source renderers for material promotion.");
            }
        }

        private static void AssignAssaultRifleMaterial(GameObject weapon)
        {
            Material material = PromoteWeaponMaterial(AssetDatabase.LoadAssetAtPath<Material>(SourceAssaultRifleMaterialPath));
            Renderer[] renderers = weapon.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                throw new InvalidOperationException($"{AssaultRifleName} should contain at least one renderer.");
            }

            for (int i = 0; i < renderers.Length; i++)
            {
                Material[] materials = renderers[i].sharedMaterials;
                if (materials.Length == 0)
                {
                    materials = new Material[1];
                }

                for (int j = 0; j < materials.Length; j++)
                {
                    materials[j] = material;
                }

                renderers[i].sharedMaterials = materials;
                EditorUtility.SetDirty(renderers[i]);
            }
        }

        private static Material[] PromoteMaterials(Material[] sourceMaterials)
        {
            Material[] promoted = new Material[sourceMaterials.Length];
            for (int i = 0; i < sourceMaterials.Length; i++)
            {
                promoted[i] = PromoteMaterial(sourceMaterials[i]);
            }

            return promoted;
        }

        private static Material PromoteMaterial(Material sourceMaterial)
        {
            return PromoteMaterial(sourceMaterial, MaterialRoot, TextureRoot);
        }

        private static Material PromoteWeaponMaterial(Material sourceMaterial)
        {
            return PromoteMaterial(sourceMaterial, WeaponMaterialRoot, WeaponTextureRoot);
        }

        private static Material PromoteMaterial(Material sourceMaterial, string materialRoot, string textureRoot)
        {
            if (sourceMaterial == null)
            {
                return null;
            }

            EnsureFolder(materialRoot);
            EnsureFolder(textureRoot);

            string targetPath = $"{materialRoot}/{SanitizeAssetName(sourceMaterial.name)}.mat";
            Material targetMaterial = AssetDatabase.LoadAssetAtPath<Material>(targetPath);
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? sourceMaterial.shader;
            if (targetMaterial == null)
            {
                targetMaterial = new Material(shader);
                AssetDatabase.CreateAsset(targetMaterial, targetPath);
            }
            else
            {
                targetMaterial.shader = shader;
            }

            targetMaterial.CopyPropertiesFromMaterial(sourceMaterial);
            CopyTextureProperty(sourceMaterial, targetMaterial, "_BaseMap", TextureUsage.Color, textureRoot);
            CopyTextureProperty(sourceMaterial, targetMaterial, "_MainTex", TextureUsage.Color, textureRoot);
            CopyTextureProperty(sourceMaterial, targetMaterial, "_BumpMap", TextureUsage.Normal, textureRoot);
            CopyTextureProperty(sourceMaterial, targetMaterial, "_EmissionMap", TextureUsage.Color, textureRoot);
            CopyTextureProperty(sourceMaterial, targetMaterial, "_MetallicGlossMap", TextureUsage.Linear, textureRoot);
            CopyTextureProperty(sourceMaterial, targetMaterial, "_OcclusionMap", TextureUsage.Linear, textureRoot);

            if (targetMaterial.GetTexture("_BumpMap") != null)
            {
                targetMaterial.EnableKeyword("_NORMALMAP");
            }

            if (targetMaterial.GetTexture("_MetallicGlossMap") != null)
            {
                targetMaterial.EnableKeyword("_METALLICSPECGLOSSMAP");
            }

            if (targetMaterial.GetTexture("_OcclusionMap") != null)
            {
                targetMaterial.EnableKeyword("_OCCLUSIONMAP");
            }

            if (targetMaterial.GetTexture("_EmissionMap") != null)
            {
                targetMaterial.EnableKeyword("_EMISSION");
            }

            EditorUtility.SetDirty(targetMaterial);
            return targetMaterial;
        }

        private static void CopyTextureProperty(Material sourceMaterial, Material targetMaterial, string propertyName, TextureUsage usage, string textureRoot)
        {
            if (!sourceMaterial.HasProperty(propertyName) || !targetMaterial.HasProperty(propertyName))
            {
                return;
            }

            Texture sourceTexture = sourceMaterial.GetTexture(propertyName);
            targetMaterial.SetTexture(propertyName, sourceTexture != null ? PromoteTexture(sourceTexture, usage, textureRoot) : null);
        }

        private static Texture PromoteTexture(Texture sourceTexture, TextureUsage usage, string textureRoot)
        {
            string sourcePath = AssetDatabase.GetAssetPath(sourceTexture).Replace('\\', '/');
            if (string.IsNullOrWhiteSpace(sourcePath) || !sourcePath.StartsWith("Assets/_Imported/", StringComparison.Ordinal))
            {
                return sourceTexture;
            }

            string fileName = sourcePath.Substring(sourcePath.LastIndexOf('/') + 1);
            string targetPath = $"{textureRoot}/{fileName}";
            if (AssetDatabase.LoadAssetAtPath<Texture>(targetPath) == null && !AssetDatabase.CopyAsset(sourcePath, targetPath))
            {
                throw new InvalidOperationException($"Failed to promote SciFiSoldier01 texture from {sourcePath} to {targetPath}.");
            }

            ConfigureTextureImporter(targetPath, usage);
            Texture promotedTexture = AssetDatabase.LoadAssetAtPath<Texture>(targetPath);
            if (promotedTexture == null)
            {
                throw new InvalidOperationException($"Promoted SciFiSoldier01 texture could not be loaded at {targetPath}.");
            }

            return promotedTexture;
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

        private static Animator EnsureAnimator(GameObject visual, AnimatorController controller)
        {
            Animator animator = visual.GetComponent<Animator>();
            if (animator == null)
            {
                animator = visual.AddComponent<Animator>();
            }

            animator.avatar = LoadAvatar(ModelPath);
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            EditorUtility.SetDirty(animator);
            return animator;
        }

        private static Renderer[] CollectPresentableRenderers(GameObject root)
        {
            return root
                .GetComponentsInChildren<Renderer>(true)
                .Where(renderer => renderer.enabled && IsActiveInPrefabHierarchy(renderer.transform, root.transform))
                .ToArray();
        }

        private static bool IsActiveInPrefabHierarchy(Transform candidate, Transform root)
        {
            for (Transform current = candidate; current != null; current = current.parent)
            {
                if (!current.gameObject.activeSelf)
                {
                    return false;
                }

                if (current == root)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsDescendantOf(Transform candidate, Transform root)
        {
            for (Transform current = candidate; current != null; current = current.parent)
            {
                if (current == root)
                {
                    return true;
                }
            }

            return false;
        }

        private static void DisableUnusedVariantRenderer(Renderer renderer)
        {
            renderer.sharedMaterials = Array.Empty<Material>();
            renderer.enabled = false;
            renderer.gameObject.SetActive(false);
            EditorUtility.SetDirty(renderer);
            EditorUtility.SetDirty(renderer.gameObject);
        }

        private static void ValidateRendererAssets(Renderer[] renderers)
        {
            int gameOwnedBaseTextures = 0;
            for (int i = 0; i < renderers.Length; i++)
            {
                Material[] materials = renderers[i].sharedMaterials;
                for (int j = 0; j < materials.Length; j++)
                {
                    Material material = materials[j];
                    if (material == null)
                    {
                        throw new InvalidOperationException($"{renderers[i].name} has a missing SciFiSoldier01 material slot.");
                    }

                    string materialPath = AssetDatabase.GetAssetPath(material).Replace('\\', '/');
                    if (!materialPath.StartsWith("Assets/_Game/", StringComparison.Ordinal) || materialPath.Contains("/_Imported/", StringComparison.Ordinal))
                    {
                        throw new InvalidOperationException($"{renderers[i].name} uses non-game-owned SciFiSoldier01 material: {materialPath}.");
                    }

                    string[] texturePropertyNames = material.GetTexturePropertyNames();
                    for (int textureIndex = 0; textureIndex < texturePropertyNames.Length; textureIndex++)
                    {
                        string propertyName = texturePropertyNames[textureIndex];
                        Texture texture = material.GetTexture(propertyName);
                        if (texture == null)
                        {
                            continue;
                        }

                        string texturePath = AssetDatabase.GetAssetPath(texture).Replace('\\', '/');
                        if (!texturePath.StartsWith("Assets/_Game/", StringComparison.Ordinal) || texturePath.Contains("/_Imported/", StringComparison.Ordinal))
                        {
                            throw new InvalidOperationException($"{material.name} uses non-game-owned SciFiSoldier01 texture on {propertyName}: {texturePath}.");
                        }

                        if (propertyName == "_BaseMap" || propertyName == "_MainTex")
                        {
                            gameOwnedBaseTextures++;
                        }
                    }
                }
            }

            if (gameOwnedBaseTextures == 0)
            {
                throw new InvalidOperationException($"{VisualName} should use at least one promoted SciFiSoldier01 base texture.");
            }
        }

        private static void ValidateFlashRenderers(GameObject root, CombatHitFeedback hitFeedback, Renderer[] expectedRenderers)
        {
            SerializedProperty flashRenderers = RequireProperty(new SerializedObject(hitFeedback), "flashRenderers");
            if (flashRenderers.arraySize != expectedRenderers.Length)
            {
                throw new InvalidOperationException($"{root.name} hit feedback should include every {VisualName} renderer.");
            }

            for (int i = 0; i < expectedRenderers.Length; i++)
            {
                UnityEngine.Object actual = flashRenderers.GetArrayElementAtIndex(i).objectReferenceValue;
                if (actual != expectedRenderers[i])
                {
                    throw new InvalidOperationException($"{root.name} hit feedback renderer {i} should reference {expectedRenderers[i].name}.");
                }
            }
        }

        private static void ValidatePrefabSourcePath(GameObject instance, string expectedPath)
        {
            GameObject source = PrefabUtility.GetCorrespondingObjectFromSource(instance);
            string sourcePath = AssetDatabase.GetAssetPath(source).Replace('\\', '/');
            if (!string.Equals(sourcePath, expectedPath, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"{instance.name} should come from {expectedPath}, found {sourcePath}.");
            }
        }

        private static Transform FindDescendant(Transform root, string childName)
        {
            if (root.name == childName)
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform match = FindDescendant(root.GetChild(i), childName);
                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }

        private static AnimatorController LoadController()
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            if (controller == null)
            {
                throw new InvalidOperationException($"Missing SciFiSoldier01 Animator Controller at {ControllerPath}.");
            }

            return controller;
        }

        private static AnimationClip LoadClip(string path)
        {
            AnimationClip clip = AssetDatabase.LoadAllAssetsAtPath(path)
                .OfType<AnimationClip>()
                .FirstOrDefault(clipAsset => !clipAsset.name.StartsWith("__preview__", StringComparison.Ordinal));
            if (clip == null)
            {
                throw new InvalidOperationException($"Missing animation clip at {path}.");
            }

            return clip;
        }

        private static Avatar LoadAvatar(string path)
        {
            Avatar avatar = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Avatar>().FirstOrDefault();
            if (avatar == null)
            {
                throw new InvalidOperationException($"Missing humanoid avatar at {path}.");
            }

            return avatar;
        }

        private static void ClearParameters(AnimatorController controller)
        {
            for (int i = controller.parameters.Length - 1; i >= 0; i--)
            {
                controller.RemoveParameter(controller.parameters[i]);
            }
        }

        private static void ClearStateMachine(AnimatorStateMachine stateMachine)
        {
            for (int i = stateMachine.anyStateTransitions.Length - 1; i >= 0; i--)
            {
                stateMachine.RemoveAnyStateTransition(stateMachine.anyStateTransitions[i]);
            }

            for (int i = stateMachine.states.Length - 1; i >= 0; i--)
            {
                stateMachine.RemoveState(stateMachine.states[i].state);
            }
        }

        private static AnimatorState AddState(AnimatorStateMachine stateMachine, string stateName, string clipPath, Vector3 position, float speed = 1f)
        {
            AnimatorState state = stateMachine.AddState(stateName, position);
            state.motion = LoadClip(clipPath);
            state.speed = speed;
            state.writeDefaultValues = true;
            return state;
        }

        private static void AddMoveTransition(AnimatorState from, AnimatorState to, AnimatorConditionMode mode, float threshold)
        {
            AnimatorStateTransition transition = from.AddTransition(to);
            transition.hasExitTime = false;
            transition.duration = 0.12f;
            transition.AddCondition(mode, threshold, "MoveSpeed");
        }

        private static void AddAnyTriggerTransition(AnimatorStateMachine stateMachine, AnimatorState destination, string trigger, float duration)
        {
            AnimatorStateTransition transition = stateMachine.AddAnyStateTransition(destination);
            transition.hasExitTime = false;
            transition.canTransitionToSelf = false;
            transition.duration = duration;
            transition.AddCondition(AnimatorConditionMode.If, 0f, trigger);
        }

        private static void AddExitTransition(AnimatorState from, AnimatorState to, float exitTime, float duration)
        {
            AnimatorStateTransition transition = from.AddTransition(to);
            transition.hasExitTime = true;
            transition.exitTime = exitTime;
            transition.duration = duration;
        }

        private static string SanitizeAssetName(string value)
        {
            return value
                .Replace(" ", "_")
                .Replace("@", "_")
                .Replace("/", "_")
                .Replace("\\", "_");
        }

        private static T RequireComponent<T>(GameObject root, string label) where T : Component
        {
            T component = root.GetComponent<T>();
            if (component == null)
            {
                throw new InvalidOperationException($"{label} is missing required component {typeof(T).Name}.");
            }

            return component;
        }

        private static void SetObjectReference(UnityEngine.Object target, string propertyName, UnityEngine.Object value)
        {
            var serializedObject = new SerializedObject(target);
            RequireProperty(serializedObject, propertyName).objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetObjectReferenceArray(UnityEngine.Object target, string propertyName, UnityEngine.Object[] values)
        {
            var serializedObject = new SerializedObject(target);
            SerializedProperty array = RequireProperty(serializedObject, propertyName);
            array.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                array.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetBool(UnityEngine.Object target, string propertyName, bool value)
        {
            var serializedObject = new SerializedObject(target);
            RequireProperty(serializedObject, propertyName).boolValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void ValidateObjectReference(UnityEngine.Object target, string propertyName, UnityEngine.Object expected)
        {
            UnityEngine.Object actual = RequireProperty(new SerializedObject(target), propertyName).objectReferenceValue;
            if (actual != expected)
            {
                string expectedName = expected != null ? expected.name : "null";
                string actualName = actual != null ? actual.name : "null";
                throw new InvalidOperationException($"{target.name}.{propertyName} expected {expectedName}, found {actualName}.");
            }
        }

        private static SerializedProperty RequireProperty(SerializedObject serializedObject, string propertyName)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                throw new InvalidOperationException($"{serializedObject.targetObject.name} is missing serialized property {propertyName}.");
            }

            return property;
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            int separatorIndex = folderPath.LastIndexOf('/');
            string parent = folderPath.Substring(0, separatorIndex);
            string name = folderPath.Substring(separatorIndex + 1);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }

        private enum TextureUsage
        {
            Color,
            Normal,
            Linear
        }
    }
}
