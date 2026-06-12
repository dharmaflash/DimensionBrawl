using System;
using System.Collections.Generic;
using System.Linq;
using DimensionBrawl.Enemies;
using DimensionBrawl.Presentation;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DimensionBrawl.Editor
{
    public static class ActionFoundationEnemyVisualSetup
    {
        private const string ScenePath = "Assets/_Game/Scenes/ActionFoundationTest.unity";
        private const string VisualName = "MaintenanceWorker_BasicSoldierVisual";
        private const string PlaceholderBodyName = "SciFiSoldierPlaceholderBody";
        private const string TelegraphName = "ReadableAttackTelegraph";
        private const string SourceVariantPrefabPath = "Assets/_Imported/AssetStore/Protofactor/Sci Fi/SciFiCharactersMegaPackVol3/SciFiShooterCharactersPackVol3/MaintenanceWorker/Prefabs/MaintenanceWorker@Gastarian_Orange Variant.prefab";
        private const string ModelPath = "Assets/_Game/Art/Characters/Enemies/SciFiSoldiers/MaintenanceWorker/Models/SK_MaintenanceWorkerAllMeshes.fbx";
        private const string MaterialRoot = "Assets/_Game/Art/Characters/Enemies/SciFiSoldiers/MaintenanceWorker/Materials";
        private const string TextureRoot = "Assets/_Game/Art/Characters/Enemies/SciFiSoldiers/MaintenanceWorker/Textures";
        private const string ControllerPath = "Assets/_Game/Art/Animations/Enemies/SciFiSoldiers/MaintenanceWorker/DB_MaintenanceWorker_BasicSoldier.controller";
        private const string IdleClipPath = "Assets/_Game/Art/Animations/Enemies/SciFiSoldiers/MaintenanceWorker/MW_IdleCombat.fbx";
        private const string RunClipPath = "Assets/_Game/Art/Animations/Enemies/SciFiSoldiers/MaintenanceWorker/MW_RunCombat.fbx";
        private const string AttackClipPath = "Assets/_Game/Art/Animations/Enemies/SciFiSoldiers/MaintenanceWorker/MW_AttackCombat.fbx";
        private const string HitClipPath = "Assets/_Game/Art/Animations/Enemies/SciFiSoldiers/MaintenanceWorker/MW_GetHitFrontLight.fbx";
        private const string DeathClipPath = "Assets/_Game/Art/Animations/Enemies/SciFiSoldiers/MaintenanceWorker/MW_DeathFront.fbx";

        [MenuItem("DimensionBrawl/Reapply Action Foundation MaintenanceWorker Enemy Visual")]
        public static void ReapplyMaintenanceWorkerEnemyVisualMenu()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != ScenePath)
            {
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }

            ConfigureImportedAssets();
            AnimatorController controller = BuildAnimatorController();
            GameObject[] roots = scene.GetRootGameObjects();
            BasicSoldierEnemy soldier = RequireObject<BasicSoldierEnemy>(roots, "basic soldier");
            GameObject visual = RecreateVisual(soldier.transform);
            ReapplyPromotedMaterials(visual);
            Animator animator = EnsureAnimator(visual, controller);
            Renderer[] renderers = CollectPresentableRenderers(visual);
            if (renderers.Length == 0)
            {
                throw new InvalidOperationException($"{VisualName} has no renderers to present or receive hit feedback.");
            }

            Renderer primaryRenderer = renderers.First();

            GameObject placeholder = FindNamedObject(roots, PlaceholderBodyName);
            if (placeholder != null)
            {
                placeholder.SetActive(false);
            }

            GameObject telegraphObject = RequireNamedObject(roots, TelegraphName, "attack telegraph");
            EnemyAttackTelegraphPresenter telegraphPresenter = RequireComponent<EnemyAttackTelegraphPresenter>(soldier.gameObject, "enemy telegraph presenter");
            CombatHitFeedback hitFeedback = RequireComponent<CombatHitFeedback>(soldier.gameObject, "enemy hit feedback");

            SetObjectReference(soldier, "animator", animator);
            SetObjectReference(soldier, "bodyRenderer", primaryRenderer);
            SetBool(soldier, "usePrototypeBodyColors", false);
            SetObjectReference(telegraphPresenter, "poseRoot", visual.transform);
            SetObjectReferenceArray(hitFeedback, "flashRenderers", renderers);
            SetBool(hitFeedback, "applyIdleColorOnEnable", false);

            EditorUtility.SetDirty(soldier);
            EditorUtility.SetDirty(telegraphPresenter);
            EditorUtility.SetDirty(hitFeedback);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Reapplied MaintenanceWorker enemy visual and Animator wiring in ActionFoundationTest.");
        }

        private static void ConfigureImportedAssets()
        {
            ConfigureModelImporter(ModelPath);
            Avatar avatar = LoadAvatar(ModelPath);
            ConfigureAnimationImporter(IdleClipPath, "MW_IdleCombat", true, avatar);
            ConfigureAnimationImporter(RunClipPath, "MW_RunCombat", true, avatar);
            ConfigureAnimationImporter(AttackClipPath, "MW_AttackCombat", false, avatar);
            ConfigureAnimationImporter(HitClipPath, "MW_GetHitFrontLight", false, avatar);
            ConfigureAnimationImporter(DeathClipPath, "MW_DeathFront", false, avatar, true);
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
            importer.SaveAndReimport();
        }

        private static void ConfigureAnimationImporter(
            string path,
            string clipName,
            bool loopTime,
            Avatar avatar,
            bool heightFromFeet = false)
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
                EnsureFolder("Assets/_Game/Art/Animations/Enemies");
                EnsureFolder("Assets/_Game/Art/Animations/Enemies/SciFiSoldiers");
                EnsureFolder("Assets/_Game/Art/Animations/Enemies/SciFiSoldiers/MaintenanceWorker");
                controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            }

            ClearParameters(controller);
            controller.AddParameter("MoveSpeed", AnimatorControllerParameterType.Float);
            controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Hit", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Death", AnimatorControllerParameterType.Trigger);

            AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
            for (int i = stateMachine.anyStateTransitions.Length - 1; i >= 0; i--)
            {
                stateMachine.RemoveAnyStateTransition(stateMachine.anyStateTransitions[i]);
            }

            for (int i = stateMachine.states.Length - 1; i >= 0; i--)
            {
                stateMachine.RemoveState(stateMachine.states[i].state);
            }

            AnimatorState idle = AddState(stateMachine, "Idle", IdleClipPath, new Vector3(250f, 80f, 0f));
            AnimatorState run = AddState(stateMachine, "Run", RunClipPath, new Vector3(250f, 170f, 0f));
            AnimatorState attack = AddState(stateMachine, "Attack", AttackClipPath, new Vector3(520f, 80f, 0f));
            AnimatorState hit = AddState(stateMachine, "Hit", HitClipPath, new Vector3(520f, 170f, 0f));
            AnimatorState death = AddState(stateMachine, "Death", DeathClipPath, new Vector3(520f, 260f, 0f));
            stateMachine.defaultState = idle;

            AddMoveTransition(idle, run, AnimatorConditionMode.Greater, 0.1f);
            AddMoveTransition(run, idle, AnimatorConditionMode.Less, 0.1f);
            AddAnyTriggerTransition(stateMachine, death, "Death", 0.05f);
            AddAnyTriggerTransition(stateMachine, hit, "Hit", 0.03f);
            AddAnyTriggerTransition(stateMachine, attack, "Attack", 0.04f);
            AddExitTransition(attack, idle, 0.82f, 0.08f);
            AddExitTransition(hit, idle, 0.88f, 0.06f);

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            return controller;
        }

        private static GameObject RecreateVisual(Transform parent)
        {
            Transform existing = parent.Find(VisualName);
            if (existing != null)
            {
                UnityEngine.Object.DestroyImmediate(existing.gameObject);
            }

            GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
            if (model == null)
            {
                throw new InvalidOperationException($"Missing promoted enemy model at {ModelPath}.");
            }

            GameObject visual = PrefabUtility.InstantiatePrefab(model, parent) as GameObject;
            if (visual == null)
            {
                throw new InvalidOperationException("Failed to instantiate MaintenanceWorker enemy visual.");
            }

            visual.name = VisualName;
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = Vector3.one;
            return visual;
        }

        private static void ReapplyPromotedMaterials(GameObject visual)
        {
            GameObject sourcePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SourceVariantPrefabPath);
            if (sourcePrefab == null)
            {
                throw new InvalidOperationException($"Missing source MaintenanceWorker material variant at {SourceVariantPrefabPath}.");
            }

            Dictionary<string, Queue<Renderer>> sourceRenderersByName = CollectPresentableRenderers(sourcePrefab)
                .GroupBy(renderer => renderer.name)
                .ToDictionary(group => group.Key, group => new Queue<Renderer>(group));

            Renderer[] targetRenderers = visual.GetComponentsInChildren<Renderer>(true);
            int reassignedCount = 0;
            int disabledCount = 0;
            for (int i = 0; i < targetRenderers.Length; i++)
            {
                Renderer targetRenderer = targetRenderers[i];
                if (!sourceRenderersByName.TryGetValue(targetRenderer.name, out Queue<Renderer> sourceQueue)
                    || sourceQueue.Count == 0)
                {
                    DisableUnusedVariantRenderer(targetRenderer);
                    disabledCount++;
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

            if (disabledCount > 0)
            {
                Debug.Log($"Disabled {disabledCount} unused MaintenanceWorker variant renderers while applying {SourceVariantPrefabPath}.");
            }
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

        private static void DisableUnusedVariantRenderer(Renderer renderer)
        {
            renderer.sharedMaterials = Array.Empty<Material>();
            renderer.enabled = false;
            renderer.gameObject.SetActive(false);
            EditorUtility.SetDirty(renderer);
            EditorUtility.SetDirty(renderer.gameObject);
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
            if (sourceMaterial == null)
            {
                return null;
            }

            EnsureFolder(MaterialRoot);
            EnsureFolder(TextureRoot);

            string targetPath = $"{MaterialRoot}/{SanitizeAssetName(sourceMaterial.name)}.mat";
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
            CopyTextureProperty(sourceMaterial, targetMaterial, "_BaseMap", TextureUsage.Color);
            CopyTextureProperty(sourceMaterial, targetMaterial, "_MainTex", TextureUsage.Color);
            CopyTextureProperty(sourceMaterial, targetMaterial, "_BumpMap", TextureUsage.Normal);
            CopyTextureProperty(sourceMaterial, targetMaterial, "_MetallicGlossMap", TextureUsage.Linear);
            CopyTextureProperty(sourceMaterial, targetMaterial, "_OcclusionMap", TextureUsage.Linear);

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

            EditorUtility.SetDirty(targetMaterial);
            return targetMaterial;
        }

        private static void CopyTextureProperty(Material sourceMaterial, Material targetMaterial, string propertyName, TextureUsage usage)
        {
            if (!sourceMaterial.HasProperty(propertyName) || !targetMaterial.HasProperty(propertyName))
            {
                return;
            }

            Texture sourceTexture = sourceMaterial.GetTexture(propertyName);
            if (sourceTexture == null)
            {
                targetMaterial.SetTexture(propertyName, null);
                return;
            }

            targetMaterial.SetTexture(propertyName, PromoteTexture(sourceTexture, usage));
        }

        private static Texture PromoteTexture(Texture sourceTexture, TextureUsage usage)
        {
            string sourcePath = AssetDatabase.GetAssetPath(sourceTexture).Replace('\\', '/');
            if (string.IsNullOrWhiteSpace(sourcePath) || !sourcePath.StartsWith("Assets/_Imported/", StringComparison.Ordinal))
            {
                return sourceTexture;
            }

            string fileName = sourcePath.Substring(sourcePath.LastIndexOf('/') + 1);
            string targetPath = $"{TextureRoot}/{fileName}";
            if (AssetDatabase.LoadAssetAtPath<Texture>(targetPath) == null && !AssetDatabase.CopyAsset(sourcePath, targetPath))
            {
                throw new InvalidOperationException($"Failed to promote MaintenanceWorker texture from {sourcePath} to {targetPath}.");
            }

            ConfigureTextureImporter(targetPath, usage);
            Texture promotedTexture = AssetDatabase.LoadAssetAtPath<Texture>(targetPath);
            if (promotedTexture == null)
            {
                throw new InvalidOperationException($"Promoted texture could not be loaded at {targetPath}.");
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

            TextureImporterType textureType = usage == TextureUsage.Normal
                ? TextureImporterType.NormalMap
                : TextureImporterType.Default;
            bool sRgb = usage == TextureUsage.Color;
            if (importer.textureType == textureType && importer.sRGBTexture == sRgb)
            {
                return;
            }

            importer.textureType = textureType;
            importer.sRGBTexture = sRgb;
            importer.SaveAndReimport();
        }

        private static string SanitizeAssetName(string value)
        {
            return value
                .Replace(" ", "_")
                .Replace("@", "_")
                .Replace("/", "_")
                .Replace("\\", "_");
        }

        private enum TextureUsage
        {
            Color,
            Normal,
            Linear
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

        private static void ClearParameters(AnimatorController controller)
        {
            for (int i = controller.parameters.Length - 1; i >= 0; i--)
            {
                controller.RemoveParameter(controller.parameters[i]);
            }
        }

        private static AnimatorState AddState(AnimatorStateMachine stateMachine, string stateName, string clipPath, Vector3 position)
        {
            AnimatorState state = stateMachine.AddState(stateName, position);
            state.motion = LoadClip(clipPath);
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

        private static AnimationClip LoadClip(string path)
        {
            AnimationClip clip = AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().FirstOrDefault(clipAsset => !clipAsset.name.StartsWith("__preview__", StringComparison.Ordinal));
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

        private static T RequireObject<T>(GameObject[] roots, string label) where T : Component
        {
            for (int i = 0; i < roots.Length; i++)
            {
                T component = roots[i].GetComponentInChildren<T>(true);
                if (component != null)
                {
                    return component;
                }
            }

            throw new InvalidOperationException($"Missing required {label}.");
        }

        private static GameObject RequireNamedObject(GameObject[] roots, string objectName, string label)
        {
            GameObject found = FindNamedObject(roots, objectName);
            if (found == null)
            {
                throw new InvalidOperationException($"Missing required {label}: {objectName}.");
            }

            return found;
        }

        private static GameObject FindNamedObject(GameObject[] roots, string objectName)
        {
            for (int i = 0; i < roots.Length; i++)
            {
                Transform[] transforms = roots[i].GetComponentsInChildren<Transform>(true);
                for (int j = 0; j < transforms.Length; j++)
                {
                    if (transforms[j] != null && transforms[j].name == objectName)
                    {
                        return transforms[j].gameObject;
                    }
                }
            }

            return null;
        }

        private static T RequireComponent<T>(GameObject owner, string label) where T : Component
        {
            if (!owner.TryGetComponent(out T component))
            {
                throw new InvalidOperationException($"{owner.name} is missing required {label}.");
            }

            return component;
        }

        private static void SetObjectReference(UnityEngine.Object target, string propertyName, UnityEngine.Object value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                throw new InvalidOperationException($"{target.name} is missing serialized property {propertyName}.");
            }

            property.objectReferenceValue = value;
            serializedObject.ApplyModifiedProperties();
        }

        private static void SetObjectReferenceArray(UnityEngine.Object target, string propertyName, UnityEngine.Object[] values)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || !property.isArray)
            {
                throw new InvalidOperationException($"{target.name} is missing serialized array property {propertyName}.");
            }

            property.ClearArray();
            for (int i = 0; i < values.Length; i++)
            {
                property.InsertArrayElementAtIndex(i);
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }

            serializedObject.ApplyModifiedProperties();
        }

        private static void SetBool(UnityEngine.Object target, string propertyName, bool value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                throw new InvalidOperationException($"{target.name} is missing serialized property {propertyName}.");
            }

            property.boolValue = value;
            serializedObject.ApplyModifiedProperties();
        }
    }
}
