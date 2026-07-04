using System;
using DimensionBrawl.LevelDesign;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace DimensionBrawl.Editor
{
    public static class OlympusCorridorCombatPocketPlacementSetup
    {
        private const string StageScenePath = "Assets/_Game/Scenes/OlympusCorridorInvasionStage.unity";
        private const string PlacementRootName = "OlympusCorridor_CombatPocketPlacement";
        private const string AccidentalComponentNameRootName = "CombatPocketPlacementLaneSpace";
        private const string ExistingCombatPackageRootName = "OlympusCorridor_BossBarrageCombatPackage";
        private const string PlaneMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/M_OlympusCorridor_CombatPocketPlacementPlane.mat";
        private const string LineMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/M_OlympusCorridor_CombatPocketPlacementLine.mat";

        private static readonly LaneMetrics ReviewLaneMetrics = new LaneMetrics(
            halfWidth: 5.25f,
            backLimitZ: -12f,
            forwardBoundaryZ: 0f,
            bossProxyZ: 18f,
            summonEntryZ: 2.25f,
            playerStartZ: -8.5f);

        [MenuItem("DimensionBrawl/Apply Olympus Combat Pocket Placement Board")]
        public static void ApplyCombatPocketPlacementBoardMenu()
        {
            ApplyCombatPocketPlacementBoard();
        }

        public static void RunBatchApplyCombatPocketPlacementBoard()
        {
            ApplyCombatPocketPlacementBoard();
        }

        private static void ApplyCombatPocketPlacementBoard()
        {
            AssetDatabase.Refresh();
            Scene scene = EditorSceneManager.OpenScene(StageScenePath, OpenSceneMode.Single);
            Material planeMaterial = LoadOrCreatePlacementMaterial(
                PlaneMaterialPath,
                new Color(0.1f, 0.9f, 1f, 0.22f));
            Material lineMaterial = LoadOrCreatePlacementMaterial(
                LineMaterialPath,
                new Color(0.2f, 1f, 0.58f, 0.78f));

            int removedExistingPlacements = RemoveExistingPlacementRoots(scene);

            GameObject placementRoot = new GameObject(PlacementRootName);
            SceneManager.MoveGameObjectToScene(placementRoot, scene);
            ApplyInitialPlacementPose(scene, placementRoot.transform);

            SummonLaneSpace laneSpace = placementRoot.AddComponent<SummonLaneSpace>();
            ConfigureLaneSpace(laneSpace, ReviewLaneMetrics);

            GameObject visualRoot = CreateChild(placementRoot.transform, "PlacementVisuals");
            CreateAreaVisuals(visualRoot.transform, ReviewLaneMetrics, planeMaterial, lineMaterial);
            CreateAnchorSet(placementRoot.transform, ReviewLaneMetrics, lineMaterial);

            EditorUtility.SetDirty(placementRoot);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                $"Applied `{PlacementRootName}` from review lane metrics: "
                + $"width={ReviewLaneMetrics.Width:0.##}, depth={ReviewLaneMetrics.Depth:0.##}, "
                + $"backZ={ReviewLaneMetrics.BackLimitZ:0.##}, forwardZ={ReviewLaneMetrics.ForwardBoundaryZ:0.##}, "
                + $"summonZ={ReviewLaneMetrics.SummonEntryZ:0.##}, bossZ={ReviewLaneMetrics.BossProxyZ:0.##}, "
                + $"removedExistingPlacements={removedExistingPlacements}.");
        }

        private static int RemoveExistingPlacementRoots(Scene scene)
        {
            int removed = 0;
            removed += RemoveAllSceneObjectsNamed(scene, PlacementRootName);
            removed += RemoveAllSceneObjectsNamed(scene, AccidentalComponentNameRootName);
            return removed;
        }

        private static void ApplyInitialPlacementPose(Scene scene, Transform placementRoot)
        {
            GameObject existingCombatPackage = FindSceneObject(scene, ExistingCombatPackageRootName);
            if (existingCombatPackage == null)
            {
                throw new InvalidOperationException(
                    $"Missing `{ExistingCombatPackageRootName}` in `{scene.path}`.");
            }

            placementRoot.SetPositionAndRotation(
                existingCombatPackage.transform.position,
                existingCombatPackage.transform.rotation);
            placementRoot.localScale = Vector3.one;
        }

        private static void ConfigureLaneSpace(SummonLaneSpace laneSpace, LaneMetrics metrics)
        {
            var serialized = new SerializedObject(laneSpace);
            SetFloat(serialized, "halfWidth", metrics.HalfWidth);
            SetFloat(serialized, "backLimitZ", metrics.BackLimitZ);
            SetFloat(serialized, "forwardBoundaryZ", metrics.ForwardBoundaryZ);
            SetFloat(serialized, "bossProxyZ", metrics.BossProxyZ);
            SetFloat(serialized, "summonEntryZ", metrics.SummonEntryZ);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(laneSpace);
        }

        private static void CreateAreaVisuals(
            Transform visualRoot,
            LaneMetrics metrics,
            Material planeMaterial,
            Material lineMaterial)
        {
            CreateBox(
                visualRoot,
                "CombatAreaPlane_BacklineToBoss",
                new Vector3(0f, 0.02f, metrics.CenterZ),
                new Vector3(metrics.Width, 0.025f, metrics.Depth),
                planeMaterial);
            CreateBox(
                visualRoot,
                "BackLimitLine_PlayerClamp",
                new Vector3(0f, 0.08f, metrics.BackLimitZ),
                new Vector3(metrics.Width, 0.08f, 0.12f),
                lineMaterial);
            CreateBox(
                visualRoot,
                "ForwardBoundaryLine_PlayerStopsHere",
                new Vector3(0f, 0.1f, metrics.ForwardBoundaryZ),
                new Vector3(metrics.Width, 0.1f, 0.16f),
                lineMaterial);
            CreateBox(
                visualRoot,
                "SummonEntryLine_SlotsEnterHere",
                new Vector3(0f, 0.12f, metrics.SummonEntryZ),
                new Vector3(metrics.Width, 0.1f, 0.14f),
                lineMaterial);
            CreateBox(
                visualRoot,
                "BossProxyLine_TargetPressure",
                new Vector3(0f, 0.12f, metrics.BossProxyZ),
                new Vector3(metrics.Width, 0.1f, 0.16f),
                lineMaterial);
            CreateBox(
                visualRoot,
                "LeftLaneRail",
                new Vector3(-metrics.HalfWidth, 0.1f, metrics.CenterZ),
                new Vector3(0.1f, 0.08f, metrics.Depth),
                lineMaterial);
            CreateBox(
                visualRoot,
                "RightLaneRail",
                new Vector3(metrics.HalfWidth, 0.1f, metrics.CenterZ),
                new Vector3(0.1f, 0.08f, metrics.Depth),
                lineMaterial);
        }

        private static void CreateAnchorSet(
            Transform placementRoot,
            LaneMetrics metrics,
            Material markerMaterial)
        {
            GameObject anchorsRoot = CreateChild(placementRoot, "PlacementAnchors");
            CreateAnchor(anchorsRoot.transform, "PlayerStartAnchor", 0f, metrics.PlayerStartZ, markerMaterial);
            CreateAnchor(anchorsRoot.transform, "PlayerBackLimitAnchor", 0f, metrics.BackLimitZ, markerMaterial);
            CreateAnchor(anchorsRoot.transform, "PlayerForwardBoundaryAnchor", 0f, metrics.ForwardBoundaryZ, markerMaterial);
            CreateAnchor(anchorsRoot.transform, "SummonSlot1EntryAnchor_Left", -metrics.HalfWidth * 0.55f, metrics.SummonEntryZ, markerMaterial);
            CreateAnchor(anchorsRoot.transform, "SummonSlot2EntryAnchor_Center", 0f, metrics.SummonEntryZ, markerMaterial);
            CreateAnchor(anchorsRoot.transform, "SummonSlot3EntryAnchor_Right", metrics.HalfWidth * 0.55f, metrics.SummonEntryZ, markerMaterial);
            CreateAnchor(anchorsRoot.transform, "BossProxyAnchor", 0f, metrics.BossProxyZ, markerMaterial);
        }

        private static void CreateAnchor(
            Transform parent,
            string name,
            float localX,
            float localZ,
            Material markerMaterial)
        {
            GameObject anchor = CreateChild(parent, name);
            anchor.transform.localPosition = new Vector3(localX, 0f, localZ);
            CreateBox(
                anchor.transform,
                "Marker",
                new Vector3(0f, 0.38f, 0f),
                new Vector3(0.48f, 0.48f, 0.48f),
                markerMaterial);
        }

        private static GameObject CreateChild(Transform parent, string name)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(parent, worldPositionStays: false);
            child.transform.localPosition = Vector3.zero;
            child.transform.localRotation = Quaternion.identity;
            child.transform.localScale = Vector3.one;
            return child;
        }

        private static MeshRenderer CreateBox(
            Transform parent,
            string name,
            Vector3 localPosition,
            Vector3 localScale,
            Material material)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.SetParent(parent, worldPositionStays: false);
            box.transform.localPosition = localPosition;
            box.transform.localRotation = Quaternion.identity;
            box.transform.localScale = localScale;

            Collider collider = box.GetComponent<Collider>();
            if (collider != null)
            {
                UnityEngine.Object.DestroyImmediate(collider);
            }

            MeshRenderer renderer = box.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            EditorUtility.SetDirty(box);
            return renderer;
        }

        private static Material LoadOrCreatePlacementMaterial(string path, Color color)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                EnsureFolderForAsset(path);
                Shader shader = Shader.Find("Universal Render Pipeline/Unlit")
                    ?? Shader.Find("Sprites/Default")
                    ?? Shader.Find("Standard");
                material = new Material(shader)
                {
                    name = System.IO.Path.GetFileNameWithoutExtension(path)
                };
                AssetDatabase.CreateAsset(material, path);
            }

            ConfigureTransparentMaterial(material, color);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void ConfigureTransparentMaterial(Material material, Color color)
        {
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }

            if (material.HasProperty("_Surface"))
            {
                material.SetFloat("_Surface", 1f);
            }

            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0f);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.renderQueue = (int)RenderQueue.Transparent;
        }

        private static void EnsureFolderForAsset(string assetPath)
        {
            string folder = System.IO.Path.GetDirectoryName(assetPath)?.Replace('\\', '/');
            if (string.IsNullOrEmpty(folder) || AssetDatabase.IsValidFolder(folder))
            {
                return;
            }

            string[] segments = folder.Split('/');
            string current = segments[0];
            for (int i = 1; i < segments.Length; i++)
            {
                string next = $"{current}/{segments[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, segments[i]);
                }

                current = next;
            }
        }

        private static void SetFloat(SerializedObject serialized, string propertyName, float value)
        {
            SerializedProperty property = serialized.FindProperty(propertyName)
                ?? throw new InvalidOperationException(
                    $"{serialized.targetObject.name} has no serialized property `{propertyName}`.");
            property.floatValue = value;
        }

        private static GameObject FindSceneObject(Scene scene, string objectName)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                GameObject found = FindDescendantOrSelf(roots[i], objectName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static int RemoveAllSceneObjectsNamed(Scene scene, string objectName)
        {
            int removed = 0;
            GameObject found = FindSceneObject(scene, objectName);
            while (found != null)
            {
                UnityEngine.Object.DestroyImmediate(found);
                removed++;
                found = FindSceneObject(scene, objectName);
            }

            return removed;
        }

        private static GameObject FindDescendantOrSelf(GameObject root, string objectName)
        {
            if (root == null)
            {
                return null;
            }

            if (string.Equals(root.name, objectName, StringComparison.Ordinal))
            {
                return root;
            }

            Transform transform = root.transform;
            for (int i = 0; i < transform.childCount; i++)
            {
                GameObject found = FindDescendantOrSelf(transform.GetChild(i).gameObject, objectName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private readonly struct LaneMetrics
        {
            public LaneMetrics(
                float halfWidth,
                float backLimitZ,
                float forwardBoundaryZ,
                float bossProxyZ,
                float summonEntryZ,
                float playerStartZ)
            {
                HalfWidth = halfWidth;
                BackLimitZ = Mathf.Min(backLimitZ, forwardBoundaryZ);
                ForwardBoundaryZ = Mathf.Max(backLimitZ, forwardBoundaryZ);
                BossProxyZ = Mathf.Max(ForwardBoundaryZ, bossProxyZ);
                SummonEntryZ = Mathf.Max(ForwardBoundaryZ, summonEntryZ);
                PlayerStartZ = Mathf.Clamp(playerStartZ, BackLimitZ, ForwardBoundaryZ);
            }

            public float HalfWidth { get; }
            public float BackLimitZ { get; }
            public float ForwardBoundaryZ { get; }
            public float BossProxyZ { get; }
            public float SummonEntryZ { get; }
            public float PlayerStartZ { get; }
            public float Width => HalfWidth * 2f;
            public float Depth => BossProxyZ - BackLimitZ;
            public float CenterZ => BackLimitZ + Depth * 0.5f;
        }
    }
}
