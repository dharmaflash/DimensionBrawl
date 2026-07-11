#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using DimensionBrawl.LevelDesign;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DimensionBrawl.Editor
{
    public static class OlympusStationCombatStageNoCrossLineSetup
    {
        private const string ScenePath = "Assets/_Game/Scenes/OlympusStationCombatStage.unity";
        private const string RootName = "OlympusStation_NoCrossCenterLine";
        private const string SourceRedCubeZonePrefabPath =
            "Assets/_Imported/AssetStore/VFX/Hovl Studio/Sci-fi effects 2/Prefabs/Red cubes zone.prefab";
        private const string CanonicalRedCubeZonePrefabPath =
            "Assets/_Game/Prefabs/VFX/Environment/PF_OlympusStation_NoCrossRedCubeZone.prefab";
        private const float RedCubeZoneDepth = 0.22f;

        [MenuItem("DimensionBrawl/Stage/Olympus Station/Apply No-Cross Center Line")]
        public static void ApplyMenu()
        {
            ApplyToScene();
        }

        public static void RunBatchApplyNoCrossLine()
        {
            try
            {
                ApplyToScene();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        public static void ApplyToScene()
        {
            AssetDatabase.Refresh();
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            SummonLaneSpace laneSpace = FindSceneComponent<SummonLaneSpace>(scene);
            if (laneSpace == null)
            {
                throw new InvalidOperationException(
                    $"Could not find a {nameof(SummonLaneSpace)} in {ScenePath}.");
            }

            GameObject existing = FindRoot(scene, RootName);
            if (existing != null)
            {
                UnityEngine.Object.DestroyImmediate(existing);
            }

            GameObject redCubeZonePrefab = EnsureCanonicalRedCubeZonePrefab(scene);

            Vector3 center = laneSpace.GetLaneWorldPoint(0f, laneSpace.ForwardBoundaryZ, 0f);
            float halfWidth = laneSpace.HalfWidth;
            float lineWidth = halfWidth * 2f + 0.9f;

            GameObject root = new GameObject(RootName);
            SceneManager.MoveGameObjectToScene(root, scene);
            root.transform.position = center;
            root.transform.rotation = laneSpace.transform.rotation;

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(redCubeZonePrefab, scene);
            instance.name = "NoCross_RedCubeZone_Line";
            instance.transform.SetParent(root.transform, false);
            instance.transform.localPosition = new Vector3(0f, 0.13f, RedCubeZoneDepth);
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;
            ConfigureRedCubeZone(instance, lineWidth);

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene))
            {
                throw new InvalidOperationException($"Failed to save {ScenePath}.");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Applied no-cross center line to {ScenePath}.");
        }

        private static GameObject EnsureCanonicalRedCubeZonePrefab(Scene scene)
        {
            GameObject sourcePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SourceRedCubeZonePrefabPath);
            if (sourcePrefab == null)
            {
                throw new InvalidOperationException($"Could not load {SourceRedCubeZonePrefabPath}.");
            }

            EnsureFolderForAsset(CanonicalRedCubeZonePrefabPath);
            GameObject workingInstance = PrefabUtility.InstantiatePrefab(sourcePrefab, scene) as GameObject;
            if (workingInstance == null)
            {
                workingInstance = UnityEngine.Object.Instantiate(sourcePrefab);
                SceneManager.MoveGameObjectToScene(workingInstance, scene);
            }

            try
            {
                workingInstance.name = "PF_OlympusStation_NoCrossRedCubeZone";
                ActionFoundationBossBarrageLaneReviewSetup.SanitizePromotedHovlSciFiVfx(
                    workingInstance,
                    loopParticles: true,
                    playOnAwake: true);

                GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(
                    workingInstance,
                    CanonicalRedCubeZonePrefabPath,
                    out bool saveSucceeded);
                if (!saveSucceeded || savedPrefab == null)
                {
                    throw new InvalidOperationException(
                        $"Failed to save canonical no-cross VFX prefab at {CanonicalRedCubeZonePrefabPath}.");
                }
            }
            finally
            {
                if (workingInstance != null)
                {
                    UnityEngine.Object.DestroyImmediate(workingInstance);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(CanonicalRedCubeZonePrefabPath, ImportAssetOptions.ForceUpdate);
            ValidateNoImportedDependencies(CanonicalRedCubeZonePrefabPath);
            GameObject canonicalPrefab =
                AssetDatabase.LoadAssetAtPath<GameObject>(CanonicalRedCubeZonePrefabPath);
            if (canonicalPrefab == null)
            {
                throw new InvalidOperationException(
                    $"Could not reload canonical no-cross VFX at {CanonicalRedCubeZonePrefabPath}.");
            }

            return canonicalPrefab;
        }

        private static void EnsureFolderForAsset(string assetPath)
        {
            string folder = System.IO.Path.GetDirectoryName(assetPath)?.Replace('\\', '/');
            if (string.IsNullOrWhiteSpace(folder))
            {
                throw new InvalidOperationException($"Could not resolve folder for {assetPath}.");
            }

            string[] parts = folder.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }

        private static void ValidateNoImportedDependencies(string assetPath)
        {
            string[] dependencies = AssetDatabase.GetDependencies(assetPath, recursive: true);
            var importedDependencies = new List<string>();
            for (int i = 0; i < dependencies.Length; i++)
            {
                if (dependencies[i].StartsWith("Assets/_Imported/", StringComparison.Ordinal))
                {
                    importedDependencies.Add(dependencies[i]);
                }
            }

            if (importedDependencies.Count > 0)
            {
                throw new InvalidOperationException(
                    $"Canonical no-cross VFX retains raw dependencies: {string.Join(", ", importedDependencies)}.");
            }
        }

        private static void ConfigureRedCubeZone(GameObject instance, float lineWidth)
        {
            ParticleSystem[] systems = instance.GetComponentsInChildren<ParticleSystem>(includeInactive: true);
            for (int i = 0; i < systems.Length; i++)
            {
                ParticleSystem system = systems[i];
                ParticleSystem.MainModule main = system.main;
                main.loop = true;
                main.prewarm = true;
                main.playOnAwake = true;
                main.cullingMode = ParticleSystemCullingMode.AlwaysSimulate;
                main.startSpeed = new ParticleSystem.MinMaxCurve(0f);

                ParticleSystem.ShapeModule shape = system.shape;
                if (shape.enabled)
                {
                    shape.shapeType = ParticleSystemShapeType.Box;
                    shape.scale = new Vector3(lineWidth, 0.12f, RedCubeZoneDepth);
                    shape.randomPositionAmount = 0.03f;
                }

                ParticleSystem.VelocityOverLifetimeModule velocity = system.velocityOverLifetime;
                velocity.enabled = true;
                velocity.space = ParticleSystemSimulationSpace.Local;
                velocity.x = new ParticleSystem.MinMaxCurve(-0.18f, 0.18f);
                velocity.y = new ParticleSystem.MinMaxCurve(0f);
                velocity.z = new ParticleSystem.MinMaxCurve(0f);

                system.Stop(withChildren: false, ParticleSystemStopBehavior.StopEmittingAndClear);
                system.Play(withChildren: false);
            }
        }

        private static T FindSceneComponent<T>(Scene scene)
            where T : Component
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                T component = roots[i].GetComponentInChildren<T>(includeInactive: true);
                if (component != null)
                {
                    return component;
                }
            }

            return null;
        }

        private static GameObject FindRoot(Scene scene, string rootName)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                if (string.Equals(roots[i].name, rootName, StringComparison.Ordinal))
                {
                    return roots[i];
                }
            }

            return null;
        }
    }
}
#endif
