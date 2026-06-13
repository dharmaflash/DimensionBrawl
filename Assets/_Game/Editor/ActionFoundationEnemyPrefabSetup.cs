using System;
using System.Collections.Generic;
using DimensionBrawl.AI;
using DimensionBrawl.Combat;
using DimensionBrawl.Enemies;
using DimensionBrawl.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DimensionBrawl.Editor
{
    public static class ActionFoundationEnemyPrefabSetup
    {
        public const string PrefabRoot = "Assets/_Game/Prefabs/Enemies/ActionFoundation";
        public const string MeleeSoldierPrefabPath = PrefabRoot + "/PF_Enemy_SciFiSoldier_Melee_ClosePunish.prefab";

        private const string MeleeSoldierPrefabName = "PF_Enemy_SciFiSoldier_Melee_ClosePunish";
        private const string VfxPoolChildName = "CombatVfxPool";

        [MenuItem("DimensionBrawl/Reapply Action Foundation Enemy Prefab Candidates")]
        public static void ReapplyEnemyPrefabCandidatesMenu()
        {
            EnsureEnemyPrefabCandidates();
            Debug.Log("Reapplied ActionFoundation enemy prefab candidates.");
        }

        [MenuItem("DimensionBrawl/Validate Action Foundation Enemy Prefab Candidates")]
        public static void ValidateEnemyPrefabCandidatesMenu()
        {
            ValidateEnemyPrefabCandidates();
            Debug.Log("ActionFoundation enemy prefab candidate validation passed.");
        }

        public static void EnsureEnemyPrefabCandidates()
        {
            EnsureFolder(PrefabRoot);

            Scene scene = EditorSceneManager.OpenScene(ActionFoundationProfileSetup.ScenePath, OpenSceneMode.Single);
            BasicSoldierEnemy source = RequireRootComponent<BasicSoldierEnemy>(scene, ActionFoundationProfileSetup.ClosePunishEnemyRootName);
            GameObject candidate = UnityEngine.Object.Instantiate(source.gameObject);
            candidate.name = MeleeSoldierPrefabName;
            candidate.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            candidate.transform.localScale = Vector3.one;
            candidate.SetActive(true);

            try
            {
                SanitizeMeleeSoldierCandidate(candidate, source.transform);
                GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(candidate, MeleeSoldierPrefabPath);
                if (savedPrefab == null)
                {
                    throw new InvalidOperationException($"Failed to save enemy prefab candidate at {MeleeSoldierPrefabPath}.");
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(candidate);
            }

            ActionFoundationEnemyArchetypeSetup.EnsureEnemyArchetypeAssets();
            AssetDatabase.SaveAssets();
        }

        public static void ValidateEnemyPrefabCandidates()
        {
            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(MeleeSoldierPrefabPath);
            if (prefabAsset == null)
            {
                throw new InvalidOperationException($"Missing melee soldier prefab candidate at {MeleeSoldierPrefabPath}.");
            }

            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(MeleeSoldierPrefabPath);
            try
            {
                ValidateMeleeSoldierPrefab(prefabRoot);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }

            CombatEnemyArchetypeProfile meleeArchetype =
                LoadAsset<CombatEnemyArchetypeProfile>(ActionFoundationEnemyArchetypeSetup.SciFiMeleeSoldierPath);
            string gameplayPrefabPath = AssetDatabase.GetAssetPath(meleeArchetype.GameplayPrefab).Replace('\\', '/');
            if (!string.Equals(gameplayPrefabPath, MeleeSoldierPrefabPath, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Sci-fi melee archetype should use {MeleeSoldierPrefabPath}, found {gameplayPrefabPath}.");
            }
        }

        private static void SanitizeMeleeSoldierCandidate(GameObject root, Transform sourceRoot)
        {
            BasicSoldierEnemy soldier = RequireComponent<BasicSoldierEnemy>(root, "melee soldier prefab candidate");
            CombatHealth health = RequireComponent<CombatHealth>(root, "melee soldier prefab candidate");
            CombatTargetSensor targetSensor = RequireComponent<CombatTargetSensor>(root, "melee soldier prefab candidate");
            CharacterController controller = RequireComponent<CharacterController>(root, "melee soldier prefab candidate");
            EnemyAttackTelegraphPresenter telegraphPresenter = RequireComponent<EnemyAttackTelegraphPresenter>(root, "melee soldier prefab candidate");
            Animator animator = root.GetComponentInChildren<Animator>(includeInactive: true);
            if (animator == null)
            {
                throw new InvalidOperationException("Melee soldier prefab candidate needs the promoted enemy Animator.");
            }

            GameObject telegraphObject = EnsureLocalTelegraphObject(root, sourceRoot, telegraphPresenter);
            Renderer telegraphRenderer = telegraphObject.GetComponentInChildren<Renderer>(includeInactive: true);
            Renderer bodyRenderer = FindPreferredBodyRenderer(root, telegraphObject);
            Renderer[] feedbackRenderers = CollectFeedbackRenderers(root, telegraphObject);

            SetObjectReference(soldier, "patternProfile", LoadAsset<CombatAiPatternProfile>(ActionFoundationProfileSetup.EnemyPatternProfilePath));
            SetObjectReference(soldier, "patternDeck", null);
            SetObjectReference(soldier, "targetSensor", targetSensor);
            SetObjectReference(soldier, "target", null);
            SetObjectReference(soldier, "targetHealth", null);
            SetObjectReference(soldier, "selfHealth", health);
            SetObjectReference(soldier, "characterController", controller);
            SetObjectReference(soldier, "animator", animator);
            SetObjectReference(soldier, "telegraphIndicator", telegraphObject);
            SetObjectReference(soldier, "telegraphPresenter", telegraphPresenter);
            SetObjectReference(soldier, "bodyRenderer", bodyRenderer);

            SetObjectReference(targetSensor, "selfHealth", health);
            SetObjectReferenceArray(targetSensor, "targetCandidates", Array.Empty<UnityEngine.Object>());

            SetObjectReference(telegraphPresenter, "telegraphObject", telegraphObject);
            SetObjectReference(telegraphPresenter, "telegraphTransform", telegraphObject.transform);
            SetObjectReference(telegraphPresenter, "telegraphRenderer", telegraphRenderer);
            SetObjectReference(telegraphPresenter, "poseRoot", animator.transform);

            CombatHitFeedback hitFeedback = root.GetComponent<CombatHitFeedback>();
            if (hitFeedback != null)
            {
                SetObjectReference(hitFeedback, "health", health);
                SetObjectReferenceArray(hitFeedback, "flashRenderers", feedbackRenderers);
            }

            EnemyActionCameraCueDriver cameraCueDriver = root.GetComponent<EnemyActionCameraCueDriver>();
            if (cameraCueDriver != null)
            {
                SetObjectReference(cameraCueDriver, "agentSource", soldier);
                SetObjectReference(cameraCueDriver, "cameraController", null);
                SetObjectReference(cameraCueDriver, "cueSpace", root.transform);
            }

            CombatVfxCuePlayer cuePlayer = root.GetComponent<CombatVfxCuePlayer>();
            if (cuePlayer != null)
            {
                SetObjectReference(cuePlayer, "pooledRoot", EnsureLocalChild(root.transform, VfxPoolChildName));
            }

            EnemyElitePatternController eliteController = root.GetComponent<EnemyElitePatternController>();
            if (eliteController != null)
            {
                SetObjectReference(eliteController, "health", health);
                SetObjectReference(eliteController, "soldier", soldier);
                SetObjectReference(eliteController, "animator", animator);
                SetObjectReference(eliteController, "cueRenderer", bodyRenderer);
                SetObjectReferenceArray(eliteController, "auraProtectedTargets", Array.Empty<UnityEngine.Object>());
                SetObjectReferenceArray(eliteController, "summonSignalObjects", Array.Empty<UnityEngine.Object>());
            }

            EnemyCombatVfxCueDriver vfxCueDriver = root.GetComponent<EnemyCombatVfxCueDriver>();
            if (vfxCueDriver != null)
            {
                SetObjectReference(vfxCueDriver, "agentSource", soldier);
                SetObjectReference(vfxCueDriver, "health", health);
                SetObjectReference(vfxCueDriver, "cuePlayer", cuePlayer);
                SetObjectReference(vfxCueDriver, "cueAnchor", root.transform);
                SetObjectReference(vfxCueDriver, "elitePatternController", eliteController);
            }
        }

        private static void ValidateMeleeSoldierPrefab(GameObject root)
        {
            if (!string.Equals(root.name, MeleeSoldierPrefabName, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"Melee soldier prefab root should be named {MeleeSoldierPrefabName}.");
            }

            BasicSoldierEnemy soldier = RequireComponent<BasicSoldierEnemy>(root, root.name);
            CombatHealth health = RequireComponent<CombatHealth>(root, root.name);
            CombatTargetSensor targetSensor = RequireComponent<CombatTargetSensor>(root, root.name);
            CharacterController controller = RequireComponent<CharacterController>(root, root.name);
            EnemyAttackTelegraphPresenter telegraphPresenter = RequireComponent<EnemyAttackTelegraphPresenter>(root, root.name);
            Animator animator = root.GetComponentInChildren<Animator>(includeInactive: true);
            if (animator == null)
            {
                throw new InvalidOperationException($"{root.name} is missing the promoted enemy Animator.");
            }

            ValidateObjectReference(soldier, "patternProfile", LoadAsset<CombatAiPatternProfile>(ActionFoundationProfileSetup.EnemyPatternProfilePath));
            ValidateObjectReference(soldier, "patternDeck", null);
            ValidateObjectReference(soldier, "targetSensor", targetSensor);
            ValidateObjectReference(soldier, "target", null);
            ValidateObjectReference(soldier, "targetHealth", null);
            ValidateObjectReference(soldier, "selfHealth", health);
            ValidateObjectReference(soldier, "characterController", controller);
            ValidateObjectReference(soldier, "animator", animator);
            ValidateObjectReference(soldier, "telegraphPresenter", telegraphPresenter);

            if (targetSensor.TargetCandidateCount != 0)
            {
                throw new InvalidOperationException($"{root.name} prefab target sensor should not carry scene target candidates.");
            }

            ValidateObjectReference(targetSensor, "selfHealth", health);
            ValidateLocalReference(telegraphPresenter, "telegraphObject", root);
            ValidateLocalReference(telegraphPresenter, "telegraphTransform", root);
            ValidateLocalReference(telegraphPresenter, "telegraphRenderer", root);
            ValidateLocalReference(telegraphPresenter, "poseRoot", root);

            EnemyActionCameraCueDriver cameraCueDriver = root.GetComponent<EnemyActionCameraCueDriver>();
            if (cameraCueDriver != null)
            {
                ValidateObjectReference(cameraCueDriver, "agentSource", soldier);
                ValidateObjectReference(cameraCueDriver, "cameraController", null);
                ValidateObjectReference(cameraCueDriver, "cueSpace", root.transform);
            }

            CombatVfxCuePlayer cuePlayer = RequireComponent<CombatVfxCuePlayer>(root, root.name);
            ValidateAssetReferencePath(cuePlayer, "profile", "Assets/_Game/");
            ValidateLocalReference(cuePlayer, "pooledRoot", root);

            EnemyCombatVfxCueDriver vfxCueDriver = RequireComponent<EnemyCombatVfxCueDriver>(root, root.name);
            ValidateObjectReference(vfxCueDriver, "agentSource", soldier);
            ValidateObjectReference(vfxCueDriver, "health", health);
            ValidateObjectReference(vfxCueDriver, "cuePlayer", cuePlayer);
            ValidateObjectReference(vfxCueDriver, "cueAnchor", root.transform);

            ValidateNoRawImportedOrExternalSceneReferences(root);
        }

        private static GameObject EnsureLocalTelegraphObject(
            GameObject root,
            Transform sourceRoot,
            EnemyAttackTelegraphPresenter presenter)
        {
            GameObject telegraphObject = GetObjectReference<GameObject>(presenter, "telegraphObject");
            if (telegraphObject != null && IsLocalPrefabReference(telegraphObject, root))
            {
                return telegraphObject;
            }

            GameObject localTelegraph;
            if (telegraphObject != null)
            {
                localTelegraph = UnityEngine.Object.Instantiate(telegraphObject);
                localTelegraph.name = telegraphObject.name;
                localTelegraph.transform.SetParent(root.transform, worldPositionStays: false);
                ApplySourceRelativeTransform(localTelegraph.transform, telegraphObject.transform, sourceRoot);
            }
            else
            {
                localTelegraph = GameObject.CreatePrimitive(PrimitiveType.Cube);
                localTelegraph.name = "ReadableAttackTelegraph";
                localTelegraph.transform.SetParent(root.transform, worldPositionStays: false);
                localTelegraph.transform.localPosition = new Vector3(0f, 0.03f, 1f);
                localTelegraph.transform.localRotation = Quaternion.identity;
                localTelegraph.transform.localScale = new Vector3(1.05f, 0.02f, 1.55f);
            }

            localTelegraph.SetActive(false);
            return localTelegraph;
        }

        private static void ApplySourceRelativeTransform(Transform target, Transform source, Transform sourceRoot)
        {
            if (sourceRoot == null)
            {
                target.localPosition = source.localPosition;
                target.localRotation = source.localRotation;
                target.localScale = source.localScale;
                return;
            }

            target.localPosition = sourceRoot.InverseTransformPoint(source.position);
            target.localRotation = Quaternion.Inverse(sourceRoot.rotation) * source.rotation;
            target.localScale = source.localScale;
        }

        private static Transform EnsureLocalChild(Transform parent, string childName)
        {
            Transform existing = parent.Find(childName);
            if (existing != null)
            {
                return existing;
            }

            var child = new GameObject(childName);
            child.transform.SetParent(parent, worldPositionStays: false);
            return child.transform;
        }

        private static Renderer FindPreferredBodyRenderer(GameObject root, GameObject telegraphObject)
        {
            Renderer[] renderers = CollectFeedbackRenderers(root, telegraphObject);
            if (renderers.Length == 0)
            {
                throw new InvalidOperationException($"{root.name} needs at least one non-telegraph renderer for hit feedback.");
            }

            return renderers[0];
        }

        private static Renderer[] CollectFeedbackRenderers(GameObject root, GameObject telegraphObject)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(includeInactive: true);
            var feedbackRenderers = new List<Renderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null || IsInside(renderer.transform, telegraphObject))
                {
                    continue;
                }

                feedbackRenderers.Add(renderer);
            }

            return feedbackRenderers.ToArray();
        }

        private static bool IsInside(Transform transform, GameObject possibleParent)
        {
            return possibleParent != null && transform != null && transform.IsChildOf(possibleParent.transform);
        }

        private static void ValidateNoRawImportedOrExternalSceneReferences(GameObject root)
        {
            Component[] components = root.GetComponentsInChildren<Component>(includeInactive: true);
            for (int i = 0; i < components.Length; i++)
            {
                Component component = components[i];
                if (component == null)
                {
                    continue;
                }

                var serializedObject = new SerializedObject(component);
                SerializedProperty property = serializedObject.GetIterator();
                bool enterChildren = true;
                while (property.NextVisible(enterChildren))
                {
                    enterChildren = true;
                    if (property.propertyType != SerializedPropertyType.ObjectReference)
                    {
                        continue;
                    }

                    UnityEngine.Object reference = property.objectReferenceValue;
                    if (reference == null)
                    {
                        continue;
                    }

                    string assetPath = AssetDatabase.GetAssetPath(reference).Replace('\\', '/');
                    if (assetPath.Contains("/_Imported/", StringComparison.Ordinal))
                    {
                        throw new InvalidOperationException(
                            $"{root.name}.{component.GetType().Name}.{property.propertyPath} references raw imported asset {assetPath}.");
                    }

                    if (!string.IsNullOrEmpty(assetPath) || EditorUtility.IsPersistent(reference))
                    {
                        continue;
                    }

                    if (!IsLocalPrefabReference(reference, root))
                    {
                        throw new InvalidOperationException(
                            $"{root.name}.{component.GetType().Name}.{property.propertyPath} carries an external scene reference to {reference.name}.");
                    }
                }
            }
        }

        private static bool IsLocalPrefabReference(UnityEngine.Object reference, GameObject root)
        {
            if (reference is GameObject gameObject)
            {
                return gameObject == root || gameObject.transform.IsChildOf(root.transform);
            }

            if (reference is Component component)
            {
                return component.gameObject == root || component.transform.IsChildOf(root.transform);
            }

            return false;
        }

        private static T RequireRootComponent<T>(Scene scene, string rootName) where T : Component
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                GameObject root = roots[i];
                if (root == null || root.name != rootName)
                {
                    continue;
                }

                T component = root.GetComponent<T>();
                if (component == null)
                {
                    throw new InvalidOperationException($"{rootName} is missing {typeof(T).Name}.");
                }

                return component;
            }

            throw new InvalidOperationException($"Missing root GameObject {rootName} in {scene.path}.");
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

        private static T LoadAsset<T>(string assetPath) where T : UnityEngine.Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (asset == null)
            {
                throw new InvalidOperationException($"Missing required asset at {assetPath}.");
            }

            return asset;
        }

        private static T GetObjectReference<T>(UnityEngine.Object target, string propertyName) where T : UnityEngine.Object
        {
            SerializedProperty property = RequireProperty(new SerializedObject(target), propertyName);
            return property.objectReferenceValue as T;
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

        private static void ValidateLocalReference(UnityEngine.Object target, string propertyName, GameObject root)
        {
            UnityEngine.Object reference = RequireProperty(new SerializedObject(target), propertyName).objectReferenceValue;
            if (reference == null || !IsLocalPrefabReference(reference, root))
            {
                string referenceName = reference != null ? reference.name : "null";
                throw new InvalidOperationException($"{target.name}.{propertyName} should reference a local prefab object, found {referenceName}.");
            }
        }

        private static void ValidateAssetReferencePath(UnityEngine.Object target, string propertyName, string requiredPrefix)
        {
            UnityEngine.Object reference = RequireProperty(new SerializedObject(target), propertyName).objectReferenceValue;
            if (reference == null)
            {
                throw new InvalidOperationException($"{target.name}.{propertyName} is not assigned.");
            }

            string assetPath = AssetDatabase.GetAssetPath(reference).Replace('\\', '/');
            if (!assetPath.StartsWith(requiredPrefix, StringComparison.Ordinal) || assetPath.Contains("/_Imported/", StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"{target.name}.{propertyName} should reference {requiredPrefix}, found {assetPath}.");
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
    }
}
