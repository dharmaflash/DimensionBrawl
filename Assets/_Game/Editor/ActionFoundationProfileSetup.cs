using System;
using DimensionBrawl.Enemies;
using DimensionBrawl.Player;
using DimensionBrawl.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DimensionBrawl.Editor
{
    public static class ActionFoundationProfileSetup
    {
        public const string ScenePath = "Assets/_Game/Scenes/ActionFoundationTest.unity";
        public const string ProfileRoot = "Assets/_Game/DesignData/Profiles/ActionFoundation";
        public const string PlayerActionProfilePath = ProfileRoot + "/DB_PlayerAction_ActionFoundation.asset";
        public const string CameraCueProfilePath = ProfileRoot + "/DB_ActionCameraCues_ActionFoundation.asset";
        public const string EnemyPatternProfilePath = ProfileRoot + "/DB_BasicSoldier_ClosePunish.asset";

        [MenuItem("DimensionBrawl/Reapply Action Foundation Gameplay Profiles")]
        public static void ReapplyGameplayProfilesMenu()
        {
            EnsureProfileAssets(out PlayerActionProfile playerActionProfile, out ActionCameraCueProfile cameraCueProfile, out EnemyPatternProfile enemyPatternProfile);

            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != ScenePath)
            {
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }

            GameObject[] roots = scene.GetRootGameObjects();
            PlayerActionController playerActions = RequireObject<PlayerActionController>(roots, "player actions");
            ActionCameraCueDriver cameraCueDriver = RequireObject<ActionCameraCueDriver>(roots, "action camera cue driver");
            BasicSoldierEnemy soldier = RequireObject<BasicSoldierEnemy>(roots, "basic soldier");

            SetObjectReference(playerActions, "actionProfile", playerActionProfile);
            SetObjectReference(cameraCueDriver, "cueProfile", cameraCueProfile);
            SetObjectReference(soldier, "patternProfile", enemyPatternProfile);

            EditorUtility.SetDirty(playerActions);
            EditorUtility.SetDirty(cameraCueDriver);
            EditorUtility.SetDirty(soldier);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            ValidatePersistedSceneReferences();
            Debug.Log("Reapplied ActionFoundation gameplay profile assets and scene references.");
        }

        public static void EnsureProfileAssets(
            out PlayerActionProfile playerActionProfile,
            out ActionCameraCueProfile cameraCueProfile,
            out EnemyPatternProfile enemyPatternProfile)
        {
            EnsureFolder(ProfileRoot);
            playerActionProfile = LoadOrCreate<PlayerActionProfile>(PlayerActionProfilePath);
            cameraCueProfile = LoadOrCreate<ActionCameraCueProfile>(CameraCueProfilePath);
            enemyPatternProfile = LoadOrCreate<EnemyPatternProfile>(EnemyPatternProfilePath);

            ConfigurePlayerActionProfile(playerActionProfile);
            ConfigureCameraCueProfile(cameraCueProfile);
            ConfigureEnemyPatternProfile(enemyPatternProfile);
            AssetDatabase.SaveAssets();
        }

        private static void ConfigurePlayerActionProfile(PlayerActionProfile profile)
        {
            SerializedObject serializedObject = new SerializedObject(profile);
            SerializedProperty combo = serializedObject.FindProperty("basicCombo");
            combo.arraySize = 5;
            SetAttackStep(combo.GetArrayElementAtIndex(0), "Attack1", 0.12f, 0.08f, 0.28f, 0.10f, 0.06f, 20f, 0.55f, 1.35f, 0.03f);
            SetAttackStep(combo.GetArrayElementAtIndex(1), "Attack2", 0.14f, 0.09f, 0.32f, 0.10f, 0.08f, 24f, 0.60f, 1.45f, 0.03f);
            SetAttackStep(combo.GetArrayElementAtIndex(2), "Attack3", 0.16f, 0.10f, 0.30f, 0.12f, 0.10f, 34f, 0.70f, 1.55f, 0.04f);
            SetAttackStep(combo.GetArrayElementAtIndex(3), "Attack4", 0.17f, 0.10f, 0.34f, 0.12f, 0.12f, 40f, 0.72f, 1.62f, 0.05f);
            SetAttackStep(combo.GetArrayElementAtIndex(4), "Attack5", 0.20f, 0.12f, 0.46f, 0.12f, 0.14f, 56f, 0.82f, 1.75f, 0.05f);
            SetFloat(serializedObject, "comboResetSeconds", 0.75f);
            SetFloat(serializedObject, "comboQueueOpenAfterSeconds", 0.10f);
            SetFloat(serializedObject, "comboChainRecoveryRatio", 0.45f);
            SetFloat(serializedObject, "dodgeDurationSeconds", 0.56f);
            SetFloat(serializedObject, "dodgeInvulnerableFromSeconds", 0.05f);
            SetFloat(serializedObject, "dodgeInvulnerableToSeconds", 0.32f);
            SetFloat(serializedObject, "dodgeRecoverySeconds", 0.14f);
            SetFloat(serializedObject, "dodgeSpeed", 10.2f);
            SetString(serializedObject, "dodgeTrigger", "DodgeForward");
            SetString(serializedObject, "dodgeBackTrigger", "DodgeBack");
            SetString(serializedObject, "dodgeLeftTrigger", "DodgeLeft");
            SetString(serializedObject, "dodgeRightTrigger", "DodgeRight");
            SetString(serializedObject, "dodgingParameter", "IsDodging");
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static void ConfigureCameraCueProfile(ActionCameraCueProfile profile)
        {
            SerializedObject serializedObject = new SerializedObject(profile);
            SetCameraCue(serializedObject.FindProperty("runStartCue"), new Vector3(0f, 0.02f, -0.10f), 0.08f, 0.8f, -0.08f, 0.01f, 0.20f, 1f);
            SetCameraCue(serializedObject.FindProperty("stopSettleCue"), new Vector3(0f, -0.02f, -0.06f), -0.02f, -0.8f, -0.12f, -0.02f, 0.22f, 1f);
            SetCameraCue(serializedObject.FindProperty("sharpTurnCue"), new Vector3(0.08f, 0f, -0.10f), 0.06f, 0.6f, -0.06f, 0f, 0.24f, 1f);
            SetCameraCue(serializedObject.FindProperty("dodgeCue"), new Vector3(0f, 0.04f, -0.20f), -0.18f, 2.2f, -0.20f, 0.03f, 0.28f, 1f);
            SetCameraCue(serializedObject.FindProperty("attackStartCue"), new Vector3(0f, -0.03f, 0.14f), 0.08f, -1.2f, 0.12f, -0.02f, 0.22f, 1.2f);
            SetCameraCue(serializedObject.FindProperty("attackHitCue"), new Vector3(0f, 0.03f, 0.12f), 0.06f, -1.8f, 0.16f, 0.01f, 0.18f, 1.3f);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static void ConfigureEnemyPatternProfile(EnemyPatternProfile profile)
        {
            SerializedObject serializedObject = new SerializedObject(profile);
            SetString(serializedObject, "enemyTypeId", "SciFiSoldier.Basic");
            SetString(serializedObject, "patternId", "ClosePunish");
            SetFloat(serializedObject, "approachSpeed", 2.7f);
            SetFloat(serializedObject, "turnRateDegrees", 540f);
            SetFloat(serializedObject, "gravity", -24f);
            SetFloat(serializedObject, "attackRange", 1.65f);
            SetFloat(serializedObject, "telegraphSeconds", 0.65f);
            SetFloat(serializedObject, "activeSeconds", 0.14f);
            SetFloat(serializedObject, "recoverySeconds", 0.45f);
            SetFloat(serializedObject, "damage", 15f);
            SetFloat(serializedObject, "hitStopSeconds", 0.03f);
            SetFloat(serializedObject, "hitReactionSeconds", 0.24f);
            SetFloat(serializedObject, "knockbackSpeed", 2f);
            SetString(serializedObject, "moveSpeedParameter", "MoveSpeed");
            SetString(serializedObject, "attackTrigger", "Attack");
            SetString(serializedObject, "hitTrigger", "Hit");
            SetString(serializedObject, "deathTrigger", "Death");
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static T LoadOrCreate<T>(string assetPath) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (asset != null)
            {
                return asset;
            }

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, assetPath);
            return asset;
        }

        private static void SetAttackStep(
            SerializedProperty step,
            string animationTrigger,
            float startupSeconds,
            float activeSeconds,
            float recoverySeconds,
            float inputBufferSeconds,
            float dodgeCancelAfterSeconds,
            float damage,
            float hitRadius,
            float hitDistance,
            float hitStopSeconds)
        {
            SetRelativeString(step, "animationTrigger", animationTrigger);
            SetRelativeFloat(step, "startupSeconds", startupSeconds);
            SetRelativeFloat(step, "activeSeconds", activeSeconds);
            SetRelativeFloat(step, "recoverySeconds", recoverySeconds);
            SetRelativeFloat(step, "inputBufferSeconds", inputBufferSeconds);
            SetRelativeFloat(step, "dodgeCancelAfterSeconds", dodgeCancelAfterSeconds);
            SetRelativeFloat(step, "damage", damage);
            SetRelativeFloat(step, "hitRadius", hitRadius);
            SetRelativeFloat(step, "hitDistance", hitDistance);
            SetRelativeFloat(step, "hitStopSeconds", hitStopSeconds);
        }

        private static void SetCameraCue(
            SerializedProperty cue,
            Vector3 localOffset,
            float planarDirectionOffset,
            float fieldOfViewDelta,
            float cameraDistanceDelta,
            float focusHeightDelta,
            float durationSeconds,
            float finisherScale)
        {
            SetRelativeBool(cue, "enabled", true);
            SetRelativeVector3(cue, "localOffset", localOffset);
            SetRelativeFloat(cue, "planarDirectionOffset", planarDirectionOffset);
            SetRelativeFloat(cue, "fieldOfViewDelta", fieldOfViewDelta);
            SetRelativeFloat(cue, "cameraDistanceDelta", cameraDistanceDelta);
            SetRelativeFloat(cue, "focusHeightDelta", focusHeightDelta);
            SetRelativeFloat(cue, "durationSeconds", durationSeconds);
            SetRelativeFloat(cue, "finisherScale", finisherScale);
        }

        private static void SetObjectReference(UnityEngine.Object target, string propertyName, UnityEngine.Object value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            serializedObject.Update();
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                throw new InvalidOperationException($"{target.name} is missing serialized property {propertyName}.");
            }

            property.objectReferenceValue = value;
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();

            if (property.objectReferenceValue != value)
            {
                string expectedName = value != null ? value.name : "null";
                string actualName = property.objectReferenceValue != null ? property.objectReferenceValue.name : "null";
                throw new InvalidOperationException($"{target.name}.{propertyName} expected {expectedName}, found {actualName} after assignment.");
            }

            EditorUtility.SetDirty(target);
        }

        private static void ValidatePersistedSceneReferences()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject[] roots = scene.GetRootGameObjects();
            PlayerActionController playerActions = RequireObject<PlayerActionController>(roots, "player actions");
            ActionCameraCueDriver cameraCueDriver = RequireObject<ActionCameraCueDriver>(roots, "action camera cue driver");
            BasicSoldierEnemy soldier = RequireObject<BasicSoldierEnemy>(roots, "basic soldier");

            ValidateObjectReferenceAssetPath(playerActions, "actionProfile", PlayerActionProfilePath);
            ValidateObjectReferenceAssetPath(cameraCueDriver, "cueProfile", CameraCueProfilePath);
            ValidateObjectReferenceAssetPath(soldier, "patternProfile", EnemyPatternProfilePath);
        }

        private static void ValidateObjectReferenceAssetPath(UnityEngine.Object target, string propertyName, string expectedPath)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                throw new InvalidOperationException($"{target.name} is missing serialized property {propertyName}.");
            }

            UnityEngine.Object actual = property.objectReferenceValue;
            if (actual == null)
            {
                throw new InvalidOperationException($"{target.name}.{propertyName} expected asset {expectedPath}, found null after scene save.");
            }

            string actualPath = AssetDatabase.GetAssetPath(actual).Replace('\\', '/');
            if (!string.Equals(actualPath, expectedPath, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"{target.name}.{propertyName} expected asset {expectedPath}, found {actualPath} after scene save.");
            }
        }

        private static void SetFloat(SerializedObject serializedObject, string propertyName, float value)
        {
            SerializedProperty property = RequireProperty(serializedObject, propertyName);
            property.floatValue = value;
        }

        private static void SetString(SerializedObject serializedObject, string propertyName, string value)
        {
            SerializedProperty property = RequireProperty(serializedObject, propertyName);
            property.stringValue = value;
        }

        private static void SetRelativeFloat(SerializedProperty owner, string propertyName, float value)
        {
            SerializedProperty property = owner.FindPropertyRelative(propertyName);
            property.floatValue = value;
        }

        private static void SetRelativeString(SerializedProperty owner, string propertyName, string value)
        {
            SerializedProperty property = owner.FindPropertyRelative(propertyName);
            property.stringValue = value;
        }

        private static void SetRelativeBool(SerializedProperty owner, string propertyName, bool value)
        {
            SerializedProperty property = owner.FindPropertyRelative(propertyName);
            property.boolValue = value;
        }

        private static void SetRelativeVector3(SerializedProperty owner, string propertyName, Vector3 value)
        {
            SerializedProperty property = owner.FindPropertyRelative(propertyName);
            property.vector3Value = value;
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
