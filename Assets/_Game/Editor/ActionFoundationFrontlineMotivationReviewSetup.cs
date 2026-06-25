using System;
using DimensionBrawl.Combat;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.Presentation;
using DimensionBrawl.Test;
using DimensionBrawl.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DimensionBrawl.Editor
{
    public static class ActionFoundationFrontlineMotivationReviewSetup
    {
        public const string ScenePath =
            "Assets/_Game/Scenes/ActionFoundationFrontlineMotivationReview.unity";
        public const string StageProfilePath =
            ActionFoundationProfileSetup.ProfileRoot + "/DB_FrontlineWaveStage_MotivationReview.asset";
        private const string PocketOwnerRootName = "BossBarrageLaneReview_PocketOwner";
        private const string HudRootName = "BossBarrageLaneReview_DebugHud";

        [MenuItem("DimensionBrawl/Create Action Foundation Frontline Motivation Review Scene")]
        public static void CreateFrontlineMotivationReviewSceneMenu()
        {
            EnsureFrontlineMotivationReviewScene();
            Debug.Log("Created or refreshed Action Foundation Frontline Motivation Review scene.");
        }

        [MenuItem("DimensionBrawl/Validate Action Foundation Frontline Motivation Review Scene")]
        public static void ValidateFrontlineMotivationReviewSceneMenu()
        {
            ValidateFrontlineMotivationReviewScene();
            Debug.Log("Action Foundation Frontline Motivation Review scene validation passed.");
        }

        public static void RunBatchSetup()
        {
            EnsureFrontlineMotivationReviewScene();
        }

        public static void RunBatchValidation()
        {
            ValidateFrontlineMotivationReviewScene();
        }

        public static void EnsureFrontlineMotivationReviewScene()
        {
            FrontlineWaveStageProfile profile = EnsureStageProfile();
            EnsureSceneAsset();

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            BossBarragePocketReviewOwner pocketOwner =
                RequireComponentOnRoot<BossBarragePocketReviewOwner>(PocketOwnerRootName);
            BossBarrageLaneReviewHud hud = RequireComponentOnRoot<BossBarrageLaneReviewHud>(HudRootName);
            BossBarrageLaneReviewOverlayHud overlayHud =
                RequireComponentOnRoot<BossBarrageLaneReviewOverlayHud>(HudRootName);

            SetObjectReference(pocketOwner, "stageProfile", profile);
            SetObjectReference(hud, "stageProfile", profile);
            pocketOwner.AssignStageProfileForReview(profile);
            hud.AssignStageProfileForReview(profile);
            MarkDirty(pocketOwner);
            MarkDirty(hud);
            SetString(hud, "stageEpisodeLabel", profile.StageEpisodeLabel);
            SetString(hud, "objectiveBadgeLabel", profile.ObjectiveBadgeLabel);
            SetString(hud, "bossDisplayName", "Dimensional Rift Curtain");
            SetString(overlayHud, "retrySceneName", "ActionFoundationFrontlineMotivationReview");
            SetString(overlayHud, "retryScenePath", ScenePath);

            ValidateSceneBindings(profile);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
            {
                throw new InvalidOperationException($"Failed to save {ScenePath}.");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.ForceReserializeAssets(new[] { ScenePath });
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            ValidateSceneBindings(profile);
        }

        public static void ValidateFrontlineMotivationReviewScene()
        {
            FrontlineWaveStageProfile profile =
                AssetDatabase.LoadAssetAtPath<FrontlineWaveStageProfile>(StageProfilePath);
            if (profile == null)
            {
                throw new InvalidOperationException($"Missing stage profile at {StageProfilePath}.");
            }

            if (profile.BeatCount < 6)
            {
                throw new InvalidOperationException($"{profile.name} should preserve at least six review beats.");
            }

            if (profile.SourceReferenceCount < 3)
            {
                throw new InvalidOperationException($"{profile.name} should preserve ArkData/project source references.");
            }

            if (profile.PressureSlotCount < 6)
            {
                throw new InvalidOperationException($"{profile.name} should preserve at least six pressure slots.");
            }

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid() || scene.path != ScenePath)
            {
                throw new InvalidOperationException($"Invalid review scene: {ScenePath}.");
            }

            BossBarragePocketReviewOwner pocketOwner =
                RequireComponentOnRoot<BossBarragePocketReviewOwner>(PocketOwnerRootName);
            BossBarrageLaneReviewHud hud = RequireComponentOnRoot<BossBarrageLaneReviewHud>(HudRootName);
            BossBarrageLaneReviewOverlayHud overlayHud =
                RequireComponentOnRoot<BossBarrageLaneReviewOverlayHud>(HudRootName);
            ValidateObjectReference(pocketOwner, "stageProfile", profile);
            ValidateObjectReference(hud, "stageProfile", profile);
            ValidateString(overlayHud, "retryScenePath", ScenePath);
            if (pocketOwner.ObjectiveStepCount != profile.ObjectiveStepCount)
            {
                throw new InvalidOperationException("Pocket owner objective count does not match the stage profile.");
            }
        }

        private static void ValidateSceneBindings(FrontlineWaveStageProfile profile)
        {
            BossBarragePocketReviewOwner pocketOwner =
                RequireComponentOnRoot<BossBarragePocketReviewOwner>(PocketOwnerRootName);
            BossBarrageLaneReviewHud hud = RequireComponentOnRoot<BossBarrageLaneReviewHud>(HudRootName);
            BossBarrageLaneReviewOverlayHud overlayHud =
                RequireComponentOnRoot<BossBarrageLaneReviewOverlayHud>(HudRootName);
            BossPressureActionDirector bossPressureActionDirector = RequireObject<BossPressureActionDirector>();
            BossSummonPressureAction bossSummonPressureAction = RequireObject<BossSummonPressureAction>();
            ValidateObjectReference(pocketOwner, "stageProfile", profile);
            ValidateObjectReference(hud, "stageProfile", profile);
            ValidateObjectReference(pocketOwner, "bossPressureActionDirector", bossPressureActionDirector);
            ValidateObjectReference(hud, "bossPressureActionDirector", bossPressureActionDirector);
            ValidateObjectReference(hud, "bossSummonPressureAction", bossSummonPressureAction);
            ValidateObjectReference(bossPressureActionDirector, "summonPressureAction", bossSummonPressureAction);
            if (pocketOwner.StageProfile != profile)
            {
                throw new InvalidOperationException($"{pocketOwner.name}.StageProfile is not bound to {profile.name}.");
            }

            if (hud.StageProfileForReview != profile)
            {
                throw new InvalidOperationException($"{hud.name}.StageProfileForReview is not bound to {profile.name}.");
            }

            ValidateString(overlayHud, "retrySceneName", "ActionFoundationFrontlineMotivationReview");
            ValidateString(overlayHud, "retryScenePath", ScenePath);
        }

        private static void EnsureSceneAsset()
        {
            SceneAsset sourceScene =
                AssetDatabase.LoadAssetAtPath<SceneAsset>(ActionFoundationBossBarrageLaneReviewSetup.ReviewScenePath);
            if (sourceScene == null)
            {
                throw new InvalidOperationException(
                    $"Missing source scene {ActionFoundationBossBarrageLaneReviewSetup.ReviewScenePath}.");
            }

            SceneAsset existingScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            if (existingScene != null)
            {
                return;
            }

            EnsureFolderForAsset(ScenePath);
            if (!AssetDatabase.CopyAsset(ActionFoundationBossBarrageLaneReviewSetup.ReviewScenePath, ScenePath))
            {
                throw new InvalidOperationException(
                    $"Could not copy {ActionFoundationBossBarrageLaneReviewSetup.ReviewScenePath} to {ScenePath}.");
            }

            AssetDatabase.ImportAsset(ScenePath);
        }

        private static FrontlineWaveStageProfile EnsureStageProfile()
        {
            EnsureFolderForAsset(StageProfilePath);
            FrontlineWaveStageProfile profile =
                AssetDatabase.LoadAssetAtPath<FrontlineWaveStageProfile>(StageProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<FrontlineWaveStageProfile>();
                AssetDatabase.CreateAsset(profile, StageProfilePath);
            }

            SerializedObject serializedObject = new SerializedObject(profile);
            SetString(serializedObject, "stageId", "FRONTLINE-MOTIVATION-REVIEW-01");
            SetString(serializedObject, "displayName", "Frontline Motivation Review");
            SetString(serializedObject, "stageEpisodeLabel", "EP 03 Frontline Stabilization");
            SetString(serializedObject, "objectiveBadgeLabel", "ROUTE");
            SetString(
                serializedObject,
                "combatPromise",
                "Bodies stay split; waves and summons contest the line");
            SetString(serializedObject, "entryCue", "Hold line; prove summon route");
            SetFloat(serializedObject, "targetDurationSeconds", 90f);
            SetString(
                serializedObject,
                "waveSlotPattern",
                "CloseProbe -> LineProjectile -> ScreenCurtain -> FrontlineBody -> CoreExpose");
            SetString(serializedObject, "spawnFamilyPattern", "Drop | Dash | Jump | Normal");
            SetString(
                serializedObject,
                "observerLoop",
                "condition gate -> combat observer -> completion record -> reward/state hook");
            SetString(serializedObject, "rewardHook", "Review-only route record; no payout or progression grant.");
            SetFloat(serializedObject, "routeStabilityStart01", 0.62f);
            SetFloat(serializedObject, "closeProbeRouteDrainPerSecond", 0.045f);
            SetFloat(serializedObject, "summonAnswerRouteDrainPerSecond", 0.06f);
            SetFloat(serializedObject, "counterWaveRouteDrainPerSecond", 0.08f);
            SetFloat(serializedObject, "closeProbeDefeatRouteBonus01", 0.12f);
            SetFloat(serializedObject, "summonBlockRouteBonus01", 0.18f);
            SetFloat(serializedObject, "followupHitRouteBonus01", 0.20f);
            SetFloat(serializedObject, "counterWaveStabilizeRouteBonus01", 0.14f);
            SetInt(serializedObject, "objectiveStepCount", 3);
            SetString(serializedObject, "stepPrefix", "Route");
            SetString(
                serializedObject,
                "preThreatChargeCue",
                "Hold the player line, build EN, then stop the close probe");
            SetString(
                serializedObject,
                "preThreatReadyCue",
                "Stop the close probe and keep SummonSlot1 ready for boss curtain");
            SetString(
                serializedObject,
                "summonChargeCue",
                "Build EN for SummonSlot1; boss curtain is returning");
            SetString(
                serializedObject,
                "summonReadyCue",
                "Send SummonSlot1 across the line to block boss curtain");
            SetString(serializedObject, "summonOpportunityCue", "Summon route is open");
            SetString(serializedObject, "followupReadyCue", "Confirm the summon opening with Skill1");
            SetString(serializedObject, "followupFiredCue", "Skill1 committed into the summon opening");
            SetString(serializedObject, "followupHitCue", "Summon route analyzed; Skill1 hit confirmed");
            SetString(
                serializedObject,
                "followupBlockedCue",
                "Boss screen absorbed the follow-up; rebuild the summon answer");
            SetString(serializedObject, "followupMissedCue", "Follow-up window missed; boss pressure is returning");
            SetString(
                serializedObject,
                "pressureBreakCue",
                "Boss curtain suppressed briefly; read the follow-up window");
            SetString(
                serializedObject,
                "counterWaveCue",
                "Counter wave entered the line; hold frontline and answer with summon");
            SetString(
                serializedObject,
                "counterWaveStabilizedCue",
                "Counter wave held by summon; rebuild the route opening");
            SetString(serializedObject, "clearObjectiveCue", "Frontline stabilized; summon route secured");
            SetString(serializedObject, "failObjectiveCue", "Player line collapsed before route stabilization");
            SetString(serializedObject, "clearTitle", "FRONTLINE STABILIZED");
            SetString(serializedObject, "clearFollowupDetail", "Summon route analyzed; Skill1 follow-up confirmed");
            SetString(serializedObject, "clearPressureDetail", "Boss curtain suppressed; frontline route recorded");
            SetString(serializedObject, "failTitle", "LINE COLLAPSED");
            SetString(serializedObject, "failDetail", "Player down before the frontline route could stabilize");
            SetString(
                serializedObject,
                "routeCollapseFailDetail",
                "Route stability collapsed before the frontline could stabilize");

            ConfigureBeats(serializedObject);
            ConfigurePressureSlots(serializedObject);
            ConfigureSourceReferences(serializedObject);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            return profile;
        }

        private static void ConfigureBeats(SerializedObject serializedObject)
        {
            SerializedProperty beats = RequireProperty(serializedObject, "beats");
            beats.arraySize = 6;
            ConfigureBeat(
                beats.GetArrayElementAtIndex(0),
                "B0.MatchRead",
                "Match Read",
                "Read the uncrossable player line and the rear boss curtain.",
                "stage_started + player_line_guarded",
                "Project spec beat 0; NIKKE stage row carries time, theme, scenario, wave, reward hooks.");
            ConfigureBeat(
                beats.GetArrayElementAtIndex(1),
                "B1.CloseProbe",
                "Probe Wave",
                "Stop the close probe before it turns the match into button pressure.",
                "close_probe_defeated",
                "NIKKE close/mid/far monster counts and Drop/Dash/Jump spawn families.");
            ConfigureBeat(
                beats.GetArrayElementAtIndex(2),
                "B2.SummonNeed",
                "First Summon Need",
                "Use SummonSlot1 because the boss curtain cannot be answered by player body crossing.",
                "summon_slot1_used + pressure_screen_intercept",
                "Project summon-first pivot; PGR combat observer maps local events to tutorial completion.");
            ConfigureBeat(
                beats.GetArrayElementAtIndex(3),
                "B3.Followup",
                "Follow-Up Window",
                "Confirm the summon route with Skill1 after the curtain is suppressed.",
                "skill1_used_during_followup + boss_damage",
                "Combat payload model: action -> target -> hit event -> presentation feedback.");
            ConfigureBeat(
                beats.GetArrayElementAtIndex(4),
                "B4.CounterWave",
                "Enemy Counter Wave",
                "If the follow-up is missed, the boss screen returns and asks for another summon answer.",
                "followup_missed_or_blocked",
                "NIKKE ScreenCurtain/FrontlineBody pressure slot adaptation from stage-wave slots.");
            ConfigureBeat(
                beats.GetArrayElementAtIndex(5),
                "B5.Result",
                "Suppression Result",
                "Record frontline stabilization, not boss HP death.",
                "route_record_committed",
                "Tutorial contract: completion record -> reward/state hook; review-only, no payout.");
        }

        private static void ConfigureSourceReferences(SerializedObject serializedObject)
        {
            SerializedProperty sources = RequireProperty(serializedObject, "sourceReferences");
            sources.arraySize = 4;
            ConfigureSourceReference(
                sources.GetArrayElementAtIndex(0),
                "NIKKE.StageWaveJoin",
                "ArkData/SubcultureGameData/games/nikke/raw/alt3ri-nikke-data/2026-06-13/files/stage-wave-join.csv",
                "Stage rows pair time limit, scenario hooks, reward id, wave group, monster slots, spawn families, and close/mid/far counts.");
            ConfigureSourceReference(
                sources.GetArrayElementAtIndex(1),
                "NIKKE.StageWaveMonsterSlots",
                "ArkData/SubcultureGameData/games/nikke/raw/alt3ri-nikke-data/2026-06-13/files/stage-wave-monster-slots.csv",
                "Encounter slots preserve spawn type, wave path, monster ratios, AI hints, and monster skill fire families.");
            ConfigureSourceReference(
                sources.GetArrayElementAtIndex(2),
                "PGR.TutorialRunnerContract",
                "ArkData/TutorialSystem_ApplyData_2026-06-24/normalized_enhanced/tutorial_runner_contract.json",
                "Local completion should be driven by combat observer events and idempotent completion records.");
            ConfigureSourceReference(
                sources.GetArrayElementAtIndex(3),
                "Project.FrontlineWaveSpec",
                "Assets/_Game/DesignDocs/ACTION_FOUNDATION_FRONTLINE_WAVE_STAGE_SPEC.md",
                "Canonical local adaptation: fixed rear boss, player line lock, summon-first frontline agency, review-only result hook.");
        }

        private static void ConfigurePressureSlots(SerializedObject serializedObject)
        {
            SerializedProperty slots = RequireProperty(serializedObject, "pressureSlots");
            slots.arraySize = 6;
            ConfigurePressureSlot(
                slots.GetArrayElementAtIndex(0),
                "S0.MatchRead.BackPressure",
                "BackPressure",
                "Normal",
                "rear_boss_fixed",
                "Read the fixed boss curtain before committing resources.",
                "stage_started + player_line_guarded",
                0.25f);
            ConfigurePressureSlot(
                slots.GetArrayElementAtIndex(1),
                "S1.CloseProbe.DropDashJump",
                "CloseProbe",
                "Drop|Dash|Jump",
                "path_grd_tutorial_001|path_grd_close_small",
                "Local defense must stop the close probe before route stability bleeds out.",
                "close_probe_defeated",
                1f);
            ConfigurePressureSlot(
                slots.GetArrayElementAtIndex(2),
                "S2.ScreenCurtain.DropNormal",
                "ScreenCurtain",
                "Drop|Normal",
                "boss_curtain_proxy|path_grd_tutorial_011",
                "SummonSlot1 answers screen pressure that the player body cannot cross.",
                "summon_slot1_used + pressure_screen_intercept",
                1.1f);
            ConfigurePressureSlot(
                slots.GetArrayElementAtIndex(3),
                "S3.CoreExpose.Normal",
                "CoreExpose",
                "Normal",
                "boss_core_followup_window",
                "Skill1 confirms the summon opening while the route is exposed.",
                "skill1_used_during_followup + boss_damage",
                0.55f);
            ConfigurePressureSlot(
                slots.GetArrayElementAtIndex(4),
                "S4.FrontlineBody.JumpDash",
                "FrontlineBody",
                "Jump|Dash",
                "path_grd_tutorial_002|path_grd_tutorial_003",
                "A missed follow-up returns body pressure and drains route stability faster.",
                "followup_missed_or_blocked",
                1.35f);
            ConfigurePressureSlot(
                slots.GetArrayElementAtIndex(5),
                "S5.Result.RecordHook",
                "RecordHook",
                "Normal",
                "completion_record",
                "Commit the observed route result without granting progression rewards.",
                "route_record_committed",
                0f);
        }

        private static void ConfigureBeat(
            SerializedProperty beat,
            string beatId,
            string label,
            string objectiveCue,
            string observedEvent,
            string sourcePattern)
        {
            RequireRelative(beat, "beatId").stringValue = beatId;
            RequireRelative(beat, "label").stringValue = label;
            RequireRelative(beat, "objectiveCue").stringValue = objectiveCue;
            RequireRelative(beat, "observedEvent").stringValue = observedEvent;
            RequireRelative(beat, "sourcePattern").stringValue = sourcePattern;
        }

        private static void ConfigureSourceReference(
            SerializedProperty source,
            string sourceId,
            string sourcePath,
            string localTakeaway)
        {
            RequireRelative(source, "sourceId").stringValue = sourceId;
            RequireRelative(source, "sourcePath").stringValue = sourcePath;
            RequireRelative(source, "localTakeaway").stringValue = localTakeaway;
        }

        private static void ConfigurePressureSlot(
            SerializedProperty slot,
            string slotId,
            string label,
            string spawnFamily,
            string wavePathPattern,
            string playerRead,
            string observerEvent,
            float routePressureWeight)
        {
            RequireRelative(slot, "slotId").stringValue = slotId;
            RequireRelative(slot, "label").stringValue = label;
            RequireRelative(slot, "spawnFamily").stringValue = spawnFamily;
            RequireRelative(slot, "wavePathPattern").stringValue = wavePathPattern;
            RequireRelative(slot, "playerRead").stringValue = playerRead;
            RequireRelative(slot, "observerEvent").stringValue = observerEvent;
            RequireRelative(slot, "routePressureWeight").floatValue = routePressureWeight;
        }

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

        private static T RequireObject<T>() where T : UnityEngine.Object
        {
            T[] matches = UnityEngine.Object.FindObjectsByType<T>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            if (matches == null || matches.Length == 0)
            {
                throw new InvalidOperationException($"Scene is missing {typeof(T).Name}.");
            }

            return matches[0];
        }

        private static T RequireComponentOnRoot<T>(string rootName) where T : Component
        {
            GameObject root = FindSceneObjectByName(rootName);
            if (root == null)
            {
                throw new InvalidOperationException($"Scene is missing root object {rootName}.");
            }

            T component = root.GetComponent<T>();
            if (component == null)
            {
                throw new InvalidOperationException($"{rootName} is missing {typeof(T).Name}.");
            }

            return component;
        }

        private static GameObject FindSceneObjectByName(string objectName)
        {
            Scene scene = SceneManager.GetActiveScene();
            GameObject[] roots = scene.GetRootGameObjects();
            foreach (GameObject root in roots)
            {
                Transform match = FindTransformByName(root.transform, objectName);
                if (match != null)
                {
                    return match.gameObject;
                }
            }

            return null;
        }

        private static Transform FindTransformByName(Transform root, string objectName)
        {
            if (root.name == objectName)
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform match = FindTransformByName(root.GetChild(i), objectName);
                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }

        private static void SetString(UnityEngine.Object target, string propertyName, string value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SetString(serializedObject, propertyName, value);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            MarkDirty(target);
        }

        private static void SetObjectReference(UnityEngine.Object target, string propertyName, UnityEngine.Object value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            RequireProperty(serializedObject, propertyName).objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            MarkDirty(target);
        }

        private static void MarkDirty(UnityEngine.Object target)
        {
            EditorUtility.SetDirty(target);
            if (target is Component component)
            {
                EditorUtility.SetDirty(component.gameObject);
                PrefabUtility.RecordPrefabInstancePropertyModifications(component);
            }
        }

        private static void SetString(SerializedObject serializedObject, string propertyName, string value)
        {
            RequireProperty(serializedObject, propertyName).stringValue = value;
        }

        private static void SetFloat(SerializedObject serializedObject, string propertyName, float value)
        {
            RequireProperty(serializedObject, propertyName).floatValue = value;
        }

        private static void SetInt(SerializedObject serializedObject, string propertyName, int value)
        {
            RequireProperty(serializedObject, propertyName).intValue = value;
        }

        private static void ValidateObjectReference(
            UnityEngine.Object target,
            string propertyName,
            UnityEngine.Object expected)
        {
            UnityEngine.Object actual = RequireProperty(new SerializedObject(target), propertyName).objectReferenceValue;
            if (actual != expected)
            {
                string actualName = actual != null ? actual.name : "null";
                throw new InvalidOperationException(
                    $"{target.name}.{propertyName} expected {expected.name}, found {actualName}.");
            }
        }

        private static void ValidateString(UnityEngine.Object target, string propertyName, string expected)
        {
            string actual = RequireProperty(new SerializedObject(target), propertyName).stringValue;
            if (!string.Equals(actual, expected, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"{target.name}.{propertyName} expected {expected}, found {actual}.");
            }
        }

        private static SerializedProperty RequireProperty(SerializedObject serializedObject, string propertyName)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                throw new InvalidOperationException(
                    $"{serializedObject.targetObject.name} is missing serialized property {propertyName}.");
            }

            return property;
        }

        private static SerializedProperty RequireRelative(SerializedProperty property, string propertyName)
        {
            SerializedProperty relative = property.FindPropertyRelative(propertyName);
            if (relative == null)
            {
                throw new InvalidOperationException($"{property.displayName} is missing {propertyName}.");
            }

            return relative;
        }
    }
}
