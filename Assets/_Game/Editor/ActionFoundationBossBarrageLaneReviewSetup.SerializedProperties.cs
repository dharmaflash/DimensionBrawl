using System;
using DimensionBrawl.Combat;
using DimensionBrawl.Presentation;
using UnityEditor;
using UnityEngine;

namespace DimensionBrawl.Editor
{
    public static partial class ActionFoundationBossBarrageLaneReviewSetup
    {
        private static void EnsureFolderForAsset(string assetPath)
        {
            string folder = System.IO.Path.GetDirectoryName(assetPath)?.Replace('\\', '/');
            if (string.IsNullOrEmpty(folder))
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

        private static void ValidateNoImportedAssetReference(string assetPath)
        {
            if (assetPath.Replace('\\', '/').Contains("/_Imported/", StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"{assetPath} must not point at raw _Imported assets.");
            }

            UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            if (asset != null)
            {
                ValidateNoImportedDependencies(asset, assetPath);
            }
        }

        private static void ValidateNoImportedDependencies(UnityEngine.Object asset, string label)
        {
            string assetPath = AssetDatabase.GetAssetPath(asset).Replace('\\', '/');
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return;
            }

            string[] dependencies = AssetDatabase.GetDependencies(assetPath, recursive: true);
            for (int i = 0; i < dependencies.Length; i++)
            {
                string dependency = dependencies[i].Replace('\\', '/');
                if (dependency.Contains("/_Imported/", StringComparison.Ordinal))
                {
                    throw new InvalidOperationException($"{label} must not depend on raw imported asset {dependency}.");
                }
            }
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

        private static void SetString(UnityEngine.Object target, string propertyName, string value)
        {
            var serializedObject = new SerializedObject(target);
            RequireProperty(serializedObject, propertyName).stringValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetFloat(UnityEngine.Object target, string propertyName, float value)
        {
            var serializedObject = new SerializedObject(target);
            RequireProperty(serializedObject, propertyName).floatValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetInt(UnityEngine.Object target, string propertyName, int value)
        {
            var serializedObject = new SerializedObject(target);
            RequireProperty(serializedObject, propertyName).intValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetEnum(UnityEngine.Object target, string propertyName, int value)
        {
            var serializedObject = new SerializedObject(target);
            RequireProperty(serializedObject, propertyName).enumValueIndex = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetVector3(UnityEngine.Object target, string propertyName, Vector3 value)
        {
            var serializedObject = new SerializedObject(target);
            RequireProperty(serializedObject, propertyName).vector3Value = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetColor(UnityEngine.Object target, string propertyName, Color value)
        {
            var serializedObject = new SerializedObject(target);
            RequireProperty(serializedObject, propertyName).colorValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static T ReadObjectReference<T>(UnityEngine.Object target, string propertyName) where T : UnityEngine.Object
        {
            UnityEngine.Object value = RequireProperty(new SerializedObject(target), propertyName).objectReferenceValue;
            return value as T;
        }

        private static T RequireReferencedObject<T>(UnityEngine.Object target, string propertyName) where T : UnityEngine.Object
        {
            T value = ReadObjectReference<T>(target, propertyName);
            if (value == null)
            {
                throw new InvalidOperationException($"{target.name}.{propertyName} must be assigned.");
            }

            return value;
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

        private static void ValidateCombatVfxCuePlayerReference(
            UnityEngine.Object target,
            string propertyName,
            CombatVfxCuePlayer expected)
        {
            CombatVfxCuePlayer actual = ReadObjectReference<CombatVfxCuePlayer>(target, propertyName);
            if (actual == expected || ResolveImplicitCombatVfxCuePlayer(target) == expected)
            {
                return;
            }

            string expectedName = expected != null ? expected.name : "null";
            string actualName = actual != null ? actual.name : "null";
            throw new InvalidOperationException(
                $"{target.name}.{propertyName} expected {expectedName}, found {actualName}.");
        }

        private static CombatVfxCuePlayer ResolveImplicitCombatVfxCuePlayer(UnityEngine.Object target)
        {
            Component component = target as Component;
            if (component == null)
            {
                return null;
            }

            CombatVfxCuePlayer localCuePlayer = component.GetComponent<CombatVfxCuePlayer>();
            if (localCuePlayer != null)
            {
                return localCuePlayer;
            }

            if (target is BossSummonPressureAction)
            {
                Transform trackedPlayer = ReadObjectReference<Transform>(target, "trackedPlayer");
                return trackedPlayer != null
                    ? trackedPlayer.GetComponent<CombatVfxCuePlayer>()
                    : null;
            }

            return null;
        }

        private static void ValidateAssignedObjectReference(UnityEngine.Object target, string propertyName)
        {
            UnityEngine.Object actual = RequireProperty(new SerializedObject(target), propertyName).objectReferenceValue;
            if (actual == null)
            {
                throw new InvalidOperationException($"{target.name}.{propertyName} must be assigned.");
            }
        }

        private static void ValidateArrayReference(UnityEngine.Object target, string propertyName, int index, UnityEngine.Object expected)
        {
            SerializedProperty array = RequireProperty(new SerializedObject(target), propertyName);
            if (!array.isArray || array.arraySize <= index)
            {
                throw new InvalidOperationException($"{target.name}.{propertyName} should contain index {index}.");
            }

            UnityEngine.Object actual = array.GetArrayElementAtIndex(index).objectReferenceValue;
            if (actual != expected)
            {
                string expectedName = expected != null ? expected.name : "null";
                string actualName = actual != null ? actual.name : "null";
                throw new InvalidOperationException($"{target.name}.{propertyName}[{index}] expected {expectedName}, found {actualName}.");
            }
        }

        private static void ValidateArrayContainsReference(
            UnityEngine.Object target,
            string propertyName,
            UnityEngine.Object expected,
            string label)
        {
            SerializedProperty array = RequireProperty(new SerializedObject(target), propertyName);
            if (!array.isArray)
            {
                throw new InvalidOperationException($"{target.name}.{propertyName} should be an array.");
            }

            for (int i = 0; i < array.arraySize; i++)
            {
                if (array.GetArrayElementAtIndex(i).objectReferenceValue == expected)
                {
                    return;
                }
            }

            string expectedName = expected != null ? expected.name : "null";
            throw new InvalidOperationException(
                $"{target.name}.{propertyName} should contain {label} ({expectedName}).");
        }

        private static UnityEngine.Object ValidateArrayAssignedReference(
            UnityEngine.Object target,
            string propertyName,
            int index)
        {
            return ValidateArrayAssignedReference<UnityEngine.Object>(target, propertyName, index);
        }

        private static T ValidateArrayAssignedReference<T>(
            UnityEngine.Object target,
            string propertyName,
            int index) where T : UnityEngine.Object
        {
            SerializedProperty array = RequireProperty(new SerializedObject(target), propertyName);
            if (!array.isArray || array.arraySize <= index)
            {
                throw new InvalidOperationException($"{target.name}.{propertyName} should contain index {index}.");
            }

            var actual = array.GetArrayElementAtIndex(index).objectReferenceValue as T;
            if (actual == null)
            {
                throw new InvalidOperationException($"{target.name}.{propertyName}[{index}] must be assigned.");
            }

            return actual;
        }

        private static void ValidateBool(UnityEngine.Object target, string propertyName, bool expected)
        {
            bool actual = RequireProperty(new SerializedObject(target), propertyName).boolValue;
            if (actual != expected)
            {
                throw new InvalidOperationException($"{target.name}.{propertyName} expected {expected}, found {actual}.");
            }
        }

        private static void ValidateString(UnityEngine.Object target, string propertyName, string expected)
        {
            string actual = RequireProperty(new SerializedObject(target), propertyName).stringValue;
            if (!string.Equals(actual, expected, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"{target.name}.{propertyName} expected {expected}, found {actual}.");
            }
        }

        private static void ValidateString(string actual, string expected, string errorMessage)
        {
            if (!string.Equals(actual, expected, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(errorMessage);
            }
        }

        private static void ValidateFloat(UnityEngine.Object target, string propertyName, float expected)
        {
            float actual = RequireProperty(new SerializedObject(target), propertyName).floatValue;
            if (!ApproximatelyEqual(actual, expected))
            {
                throw new InvalidOperationException($"{target.name}.{propertyName} expected {expected}, found {actual}.");
            }
        }

        private static void ValidateFloatAtLeast(UnityEngine.Object target, string propertyName, float minimum)
        {
            float actual = RequireProperty(new SerializedObject(target), propertyName).floatValue;
            if (actual < minimum)
            {
                throw new InvalidOperationException($"{target.name}.{propertyName} expected at least {minimum}, found {actual}.");
            }
        }

        private static void ValidateFloatValue(float actual, float expected, string errorMessage)
        {
            if (!ApproximatelyEqual(actual, expected))
            {
                throw new InvalidOperationException($"{errorMessage} Expected {expected}, found {actual}.");
            }
        }

        private static bool ApproximatelyEqual(float actual, float expected)
        {
            return Mathf.Abs(actual - expected) <= 0.0001f;
        }

        private static void ValidateInt(UnityEngine.Object target, string propertyName, int expected)
        {
            int actual = RequireProperty(new SerializedObject(target), propertyName).intValue;
            if (actual != expected)
            {
                throw new InvalidOperationException($"{target.name}.{propertyName} expected {expected}, found {actual}.");
            }
        }

        private static void ValidateEnum(UnityEngine.Object target, string propertyName, int expected)
        {
            int actual = RequireProperty(new SerializedObject(target), propertyName).enumValueIndex;
            if (actual != expected)
            {
                throw new InvalidOperationException($"{target.name}.{propertyName} expected enum index {expected}, found {actual}.");
            }
        }

        private static void ValidateVector3(UnityEngine.Object target, string propertyName, Vector3 expected)
        {
            Vector3 actual = RequireProperty(new SerializedObject(target), propertyName).vector3Value;
            if ((actual - expected).sqrMagnitude > 0.000001f)
            {
                throw new InvalidOperationException($"{target.name}.{propertyName} expected {expected}, found {actual}.");
            }
        }

        private static void ValidateColor(UnityEngine.Object target, string propertyName, Color expected)
        {
            Color actual = RequireProperty(new SerializedObject(target), propertyName).colorValue;
            float maxDelta = Mathf.Max(
                Mathf.Abs(actual.r - expected.r),
                Mathf.Abs(actual.g - expected.g),
                Mathf.Abs(actual.b - expected.b),
                Mathf.Abs(actual.a - expected.a));
            if (maxDelta > 0.000001f)
            {
                throw new InvalidOperationException($"{target.name}.{propertyName} expected {expected}, found {actual}.");
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

        private static SerializedProperty RequireRelativeProperty(SerializedProperty property, string relativeName)
        {
            SerializedProperty relative = property.FindPropertyRelative(relativeName);
            if (relative == null)
            {
                throw new InvalidOperationException($"{property.propertyPath} is missing serialized property {relativeName}.");
            }

            return relative;
        }
    }
}
