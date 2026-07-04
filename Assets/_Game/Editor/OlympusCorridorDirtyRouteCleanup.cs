using System;
using System.Collections.Generic;
using DimensionBrawl.LevelDesign;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DimensionBrawl.Editor
{
    public static class OlympusCorridorDirtyRouteCleanup
    {
        private const string StageScenePath = "Assets/_Game/Scenes/OlympusCorridorInvasionStage.unity";
        private const string FlowRootName = "OlympusCorridor_CombatFlowRoot";

        private static readonly string[] DirtyRouteObjectNames =
        {
            "OlympusCorridor_StairToCorridorCombatTrigger",
            "OlympusCorridor_CorridorCombatBounds",
            "OlympusCorridor_IntroStairTraversalSupport",
            "StageClear_CorridorExit"
        };

        [MenuItem("DimensionBrawl/Cleanup Olympus Dirty Route Objects")]
        public static void CleanupDirtyRouteObjectsMenu()
        {
            CleanupDirtyRouteObjects();
        }

        public static void RunBatchCleanupDirtyRouteObjects()
        {
            CleanupDirtyRouteObjects();
        }

        private static void CleanupDirtyRouteObjects()
        {
            AssetDatabase.Refresh();
            Scene scene = EditorSceneManager.OpenScene(StageScenePath, OpenSceneMode.Single);
            OlympusCorridorCombatFlowController flowController = ResolveFlowController(scene);

            ClearDirtyRouteReferences(flowController);
            List<string> removed = RemoveNamedObjects(scene, DirtyRouteObjectNames);
            int removedNullSceneBindingReferences = CompactSceneBindingReferences(scene);

            EditorUtility.SetDirty(flowController);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                "Cleaned Olympus dirty route objects: "
                + (removed.Count > 0 ? string.Join(", ", removed) : "<none>")
                + $"; removed null scene binding references={removedNullSceneBindingReferences}");
        }

        private static OlympusCorridorCombatFlowController ResolveFlowController(Scene scene)
        {
            GameObject flowRoot = FindSceneObject(scene, FlowRootName);
            if (flowRoot == null)
            {
                throw new InvalidOperationException($"Missing `{FlowRootName}` in {scene.path}.");
            }

            OlympusCorridorCombatFlowController flowController =
                flowRoot.GetComponent<OlympusCorridorCombatFlowController>();
            if (flowController == null)
            {
                throw new InvalidOperationException(
                    $"`{FlowRootName}` is missing {nameof(OlympusCorridorCombatFlowController)}.");
            }

            return flowController;
        }

        private static void ClearDirtyRouteReferences(OlympusCorridorCombatFlowController flowController)
        {
            var serialized = new SerializedObject(flowController);
            SetObjectReference(serialized, "stairTriggerCenter", null);
            SetFloat(serialized, "stairTriggerRadius", 0f);
            ClearArray(serialized, "corridorBoundsRoots");
            ClearArray(serialized, "corridorTargets");
            ClearArray(serialized, "corridorClearTargets");
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static List<string> RemoveNamedObjects(Scene scene, string[] objectNames)
        {
            var removed = new List<string>();
            for (int i = 0; i < objectNames.Length; i++)
            {
                GameObject found = FindSceneObject(scene, objectNames[i]);
                if (found == null)
                {
                    continue;
                }

                removed.Add(GetHierarchyPath(found.transform));
                UnityEngine.Object.DestroyImmediate(found);
            }

            return removed;
        }

        private static int CompactSceneBindingReferences(Scene scene)
        {
            int removed = 0;
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                StageDefinitionSceneBinding[] bindings =
                    roots[i].GetComponentsInChildren<StageDefinitionSceneBinding>(includeInactive: true);
                for (int bindingIndex = 0; bindingIndex < bindings.Length; bindingIndex++)
                {
                    StageDefinitionSceneBinding binding = bindings[bindingIndex];
                    if (binding == null)
                    {
                        continue;
                    }

                    var serialized = new SerializedObject(binding);
                    removed += RemoveNullArrayReferences(serialized.FindProperty("anchorPoints"));
                    removed += RemoveNullArrayReferences(serialized.FindProperty("cutscenePorts"));
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(binding);
                }
            }

            return removed;
        }

        private static void SetObjectReference(
            SerializedObject serialized,
            string propertyName,
            UnityEngine.Object value)
        {
            SerializedProperty property = serialized.FindProperty(propertyName)
                ?? throw new InvalidOperationException(
                    $"{serialized.targetObject.name} has no serialized property `{propertyName}`.");
            property.objectReferenceValue = value;
        }

        private static void SetFloat(SerializedObject serialized, string propertyName, float value)
        {
            SerializedProperty property = serialized.FindProperty(propertyName)
                ?? throw new InvalidOperationException(
                    $"{serialized.targetObject.name} has no serialized property `{propertyName}`.");
            property.floatValue = value;
        }

        private static void ClearArray(SerializedObject serialized, string propertyName)
        {
            SerializedProperty property = serialized.FindProperty(propertyName)
                ?? throw new InvalidOperationException(
                    $"{serialized.targetObject.name} has no serialized property `{propertyName}`.");
            if (!property.isArray)
            {
                throw new InvalidOperationException(
                    $"{serialized.targetObject.name}.{propertyName} is not an array property.");
            }

            property.ClearArray();
        }

        private static int RemoveNullArrayReferences(SerializedProperty property)
        {
            if (property == null || !property.isArray)
            {
                return 0;
            }

            int removed = 0;
            for (int i = property.arraySize - 1; i >= 0; i--)
            {
                SerializedProperty item = property.GetArrayElementAtIndex(i);
                if (item.propertyType != SerializedPropertyType.ObjectReference
                    || item.objectReferenceValue != null)
                {
                    continue;
                }

                property.DeleteArrayElementAtIndex(i);
                removed++;
            }

            return removed;
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

        private static string GetHierarchyPath(Transform transform)
        {
            if (transform == null)
            {
                return "<null>";
            }

            var names = new List<string>();
            Transform current = transform;
            while (current != null)
            {
                names.Add(current.name);
                current = current.parent;
            }

            names.Reverse();
            return string.Join("/", names);
        }
    }
}
