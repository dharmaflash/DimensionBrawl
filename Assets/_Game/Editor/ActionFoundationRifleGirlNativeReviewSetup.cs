using System;
using DimensionBrawl.Test;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.SceneManagement;

namespace DimensionBrawl.Editor
{
    public static class ActionFoundationRifleGirlNativeReviewSetup
    {
        public const string ReviewScenePath = "Assets/_Game/Scenes/ActionFoundationRifleGirlNativeReview.unity";

        private const string SourcePrefabPath =
            "Assets/_Imported/AssetStore/CombatGirlsCharacterPack_RifleGirl/RifleGirl/Prefab/Rifle_Full_Body.prefab";
        private const string SourceControllerPath =
            "Assets/_Imported/AssetStore/CombatGirlsCharacterPack_RifleGirl/RifleGirl/Animations/Rifle_Controller.controller";
        private const string RifleRootName = "RifleGirlNativeReview_SourcePrefab";
        private const string TargetName = "RifleGirlNativeReview_AimTarget";

        [MenuItem("DimensionBrawl/Reapply RifleGirl Native Motion Review Scene")]
        public static void ReapplyRifleGirlNativeReviewSceneMenu()
        {
            EnsureRifleGirlNativeReviewScene();
            Debug.Log("Reapplied RifleGirl native motion review scene.");
        }

        [MenuItem("DimensionBrawl/Validate RifleGirl Native Motion Review Scene")]
        public static void ValidateRifleGirlNativeReviewSceneMenu()
        {
            ValidateRifleGirlNativeReviewScene();
            Debug.Log("RifleGirl native motion review scene validation passed.");
        }

        public static void EnsureRifleGirlNativeReviewScene()
        {
            GameObject sourcePrefab = LoadAsset<GameObject>(SourcePrefabPath);
            RuntimeAnimatorController sourceController = LoadAsset<RuntimeAnimatorController>(SourceControllerPath);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateReviewCamera(scene);
            CreateDirectionalLight(scene);
            CreateReviewFloor(scene);
            CreateAimTarget(scene);

            GameObject rifleRoot = PrefabUtility.InstantiatePrefab(sourcePrefab, scene) as GameObject;
            if (rifleRoot == null)
            {
                throw new InvalidOperationException("Failed to instantiate original RifleGirl prefab for review.");
            }

            rifleRoot.name = RifleRootName;
            rifleRoot.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            rifleRoot.transform.localScale = Vector3.one;

            Animator animator = RequireComponent<Animator>(rifleRoot, "RifleGirl source Animator");
            animator.runtimeAnimatorController = sourceController;
            animator.applyRootMotion = true;

            RifleGirlNativeMotionReviewDriver reviewDriver =
                rifleRoot.GetComponent<RifleGirlNativeMotionReviewDriver>()
                ?? rifleRoot.AddComponent<RifleGirlNativeMotionReviewDriver>();
            reviewDriver.Configure(animator);
            EditorUtility.SetDirty(reviewDriver);
            EditorUtility.SetDirty(animator);
            EditorUtility.SetDirty(rifleRoot);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ReviewScenePath);
            AssetDatabase.SaveAssets();
            ValidateRifleGirlNativeReviewScene();
        }

        public static void ValidateRifleGirlNativeReviewScene()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != ReviewScenePath)
            {
                scene = EditorSceneManager.OpenScene(ReviewScenePath, OpenSceneMode.Single);
            }

            GameObject rifleRoot = FindRoot(scene, RifleRootName)
                ?? throw new InvalidOperationException($"Missing {RifleRootName} in {ReviewScenePath}.");

            Animator animator = RequireComponent<Animator>(rifleRoot, "RifleGirl source Animator");
            if (animator.runtimeAnimatorController != LoadAsset<RuntimeAnimatorController>(SourceControllerPath))
            {
                throw new InvalidOperationException("RifleGirl review must use the original Rifle_Controller.controller.");
            }

            if (!animator.applyRootMotion)
            {
                throw new InvalidOperationException("RifleGirl review should keep original root motion enabled.");
            }

            if (rifleRoot.GetComponent("Character_Weapon_Controller") == null)
            {
                throw new InvalidOperationException("RifleGirl review is missing the original Character_Weapon_Controller.");
            }

            if (rifleRoot.GetComponent<RifleGirlNativeMotionReviewDriver>() == null)
            {
                throw new InvalidOperationException("RifleGirl review is missing the native motion review driver.");
            }

            RequireDescendant(rifleRoot.transform, "Hand_R_Socket");
            RequireDescendant(rifleRoot.transform, "Put_Socket_Rifle");
            RequireDescendant(rifleRoot.transform, "R_Weapon_Bone_Dymmy_R");

            ParentConstraint[] constraints = rifleRoot.GetComponentsInChildren<ParentConstraint>(includeInactive: true);
            if (constraints.Length == 0)
            {
                throw new InvalidOperationException("RifleGirl review should preserve the weapon ParentConstraint.");
            }

            bool hasMultiSourceConstraint = false;
            for (int i = 0; i < constraints.Length; i++)
            {
                hasMultiSourceConstraint |= constraints[i].sourceCount >= 2;
            }

            if (!hasMultiSourceConstraint)
            {
                throw new InvalidOperationException("RifleGirl review weapon constraint should keep socket source options.");
            }

            if (FindRoot(scene, TargetName) == null)
            {
                throw new InvalidOperationException($"Missing {TargetName} aim target.");
            }
        }

        private static void CreateReviewCamera(Scene scene)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            SceneManager.MoveGameObjectToScene(cameraObject, scene);
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 32f;
            camera.nearClipPlane = 0.03f;
            camera.farClipPlane = 120f;
            cameraObject.AddComponent<AudioListener>();
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetPositionAndRotation(
                new Vector3(0.72f, 1.32f, 4.55f),
                Quaternion.Euler(6f, 180f, 0f));
        }

        private static void CreateDirectionalLight(Scene scene)
        {
            GameObject lightObject = new GameObject("Directional Light");
            SceneManager.MoveGameObjectToScene(lightObject, scene);
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.15f;
            lightObject.transform.rotation = Quaternion.Euler(48f, -32f, 0f);
        }

        private static void CreateReviewFloor(Scene scene)
        {
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "RifleGirlNativeReview_Floor";
            SceneManager.MoveGameObjectToScene(floor, scene);
            floor.transform.position = new Vector3(0f, -0.04f, -2.5f);
            floor.transform.localScale = new Vector3(6f, 0.08f, 12f);
            Renderer renderer = floor.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = CreateRuntimeReviewMaterial("RifleGirlNativeReview_FloorMat", new Color(0.12f, 0.14f, 0.16f, 1f));
            }
        }

        private static void CreateAimTarget(Scene scene)
        {
            GameObject target = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            target.name = TargetName;
            SceneManager.MoveGameObjectToScene(target, scene);
            target.transform.position = new Vector3(0f, 0.9f, -7f);
            target.transform.localScale = new Vector3(0.35f, 0.9f, 0.35f);
            Renderer renderer = target.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = CreateRuntimeReviewMaterial("RifleGirlNativeReview_TargetMat", new Color(0.9f, 0.25f, 0.22f, 1f));
            }
        }

        private static Material CreateRuntimeReviewMaterial(string materialName, Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("Universal Render Pipeline/Unlit")
                ?? Shader.Find("Standard");
            Material material = new Material(shader) { name = materialName, color = color };
            return material;
        }

        private static T LoadAsset<T>(string path) where T : UnityEngine.Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                throw new InvalidOperationException($"Missing required asset at {path}.");
            }

            return asset;
        }

        private static T RequireComponent<T>(GameObject gameObject, string label) where T : Component
        {
            T component = gameObject.GetComponent<T>();
            if (component == null)
            {
                throw new InvalidOperationException($"{label} is missing on {gameObject.name}.");
            }

            return component;
        }

        private static GameObject FindRoot(Scene scene, string objectName)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                if (roots[i].name == objectName)
                {
                    return roots[i];
                }
            }

            return null;
        }

        private static Transform RequireDescendant(Transform root, string childName)
        {
            Transform child = FindDescendant(root, childName);
            if (child == null)
            {
                throw new InvalidOperationException($"{root.name} is missing descendant {childName}.");
            }

            return child;
        }

        private static Transform FindDescendant(Transform root, string childName)
        {
            if (root.name == childName)
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindDescendant(root.GetChild(i), childName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }
    }
}
