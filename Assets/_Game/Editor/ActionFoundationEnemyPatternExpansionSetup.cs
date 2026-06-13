using System;
using System.Collections.Generic;
using DimensionBrawl.AI;
using DimensionBrawl.Combat;
using DimensionBrawl.Enemies;
using DimensionBrawl.Player;
using DimensionBrawl.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DimensionBrawl.Editor
{
    public static class ActionFoundationEnemyPatternExpansionSetup
    {
        public const string RetreatShotPatternPath = ActionFoundationProfileSetup.ProfileRoot + "/DB_BasicSoldier_RetreatShot.asset";
        public const string RetreatBlinkPatternPath = ActionFoundationProfileSetup.ProfileRoot + "/DB_BasicSoldier_RetreatBlink.asset";
        public const string GuardBreakPatternPath = ActionFoundationProfileSetup.ProfileRoot + "/DB_EliteSoldier_GuardBreak.asset";
        public const string GeneralPatternDeckPath = ActionFoundationProfileSetup.ProfileRoot + "/DB_BasicSoldier_GeneralPatternDeck.asset";
        public const string ElitePatternDeckPath = ActionFoundationProfileSetup.ProfileRoot + "/DB_EliteSoldier_PatternDeck.asset";
        public const string ElitePhaseTwoPatternDeckPath = ActionFoundationProfileSetup.ProfileRoot + "/DB_EliteSoldier_PhaseTwoPatternDeck.asset";
        public const string ShieldCycleEliteProfilePath = ActionFoundationProfileSetup.ProfileRoot + "/DB_ElitePattern_ShieldCycle.asset";
        public const string ArmorBreakEliteProfilePath = ActionFoundationProfileSetup.ProfileRoot + "/DB_ElitePattern_ArmorBreak.asset";
        public const string AuraBufferEliteProfilePath = ActionFoundationProfileSetup.ProfileRoot + "/DB_ElitePattern_AuraBuffer.asset";
        public const string SummonPackageEliteProfilePath = ActionFoundationProfileSetup.ProfileRoot + "/DB_ElitePattern_SummonPackage.asset";
        public const string PhaseSwapEliteProfilePath = ActionFoundationProfileSetup.ProfileRoot + "/DB_ElitePattern_PhaseSwap.asset";
        public const string RetreatShotEnemyRootName = "Enemy_SciFiSoldier_RetreatShot";
        public const string RetreatBlinkEnemyRootName = "Enemy_SciFiSoldier_RetreatBlink";
        public const string GuardBreakEnemyRootName = "Enemy_SciFiSoldier_GuardBreak";
        public const string GeneralDeckEnemyRootName = "Enemy_SciFiSoldier_GeneralDeck";
        public const string EliteDeckEnemyRootName = "Enemy_SciFiSoldier_EliteDeck";
        public const string EliteTraitsEnemyRootName = "Enemy_SciFiSoldier_EliteTraits";

        [MenuItem("DimensionBrawl/Reapply Action Foundation Extended Enemy Patterns")]
        public static void ReapplyExtendedEnemyPatternsMenu()
        {
            ActionFoundationProfileSetup.ReapplyGameplayProfilesMenu();
            EnsureExtendedPatternAssets();
            EnsureExtendedSceneSamples();
            Debug.Log("Reapplied ActionFoundation extended enemy pattern assets and scene samples.");
        }

        public static void EnsureExtendedPatternAssets()
        {
            ActionFoundationProfileSetup.EnsureProfileAssets(
                out _,
                out _,
                out CombatAiPatternProfile closePunish,
                out CombatAiPatternProfile lungeStrike,
                out CombatAiPatternProfile heavyWindup,
                out CombatAiPatternProfile linePressure,
                out CombatAiPatternProfile fanPressure,
                out _);

            CombatAiPatternProfile retreatShot = LoadOrCreateEnemyPatternProfile(RetreatShotPatternPath);
            CombatAiPatternProfile retreatBlink = LoadOrCreateEnemyPatternProfile(RetreatBlinkPatternPath);
            CombatAiPatternProfile guardBreak = LoadOrCreateEnemyPatternProfile(GuardBreakPatternPath);
            CombatAiPatternDeck generalDeck = LoadOrCreate<CombatAiPatternDeck>(GeneralPatternDeckPath);
            CombatAiPatternDeck eliteDeck = LoadOrCreate<CombatAiPatternDeck>(ElitePatternDeckPath);
            CombatAiPatternDeck phaseTwoDeck = LoadOrCreate<CombatAiPatternDeck>(ElitePhaseTwoPatternDeckPath);
            CombatAiElitePatternProfile shieldCycle = LoadOrCreate<CombatAiElitePatternProfile>(ShieldCycleEliteProfilePath);
            CombatAiElitePatternProfile armorBreak = LoadOrCreate<CombatAiElitePatternProfile>(ArmorBreakEliteProfilePath);
            CombatAiElitePatternProfile auraBuffer = LoadOrCreate<CombatAiElitePatternProfile>(AuraBufferEliteProfilePath);
            CombatAiElitePatternProfile summonPackage = LoadOrCreate<CombatAiElitePatternProfile>(SummonPackageEliteProfilePath);
            CombatAiElitePatternProfile phaseSwap = LoadOrCreate<CombatAiElitePatternProfile>(PhaseSwapEliteProfilePath);

            ConfigureEnemyPatternProfile(
                retreatShot,
                "SciFiSoldier.Ranged",
                "RetreatShot",
                2.15f,
                520f,
                -24f,
                0.32f,
                4.6f,
                true,
                5.4f,
                0.15f,
                CombatAiAttackShape.ForwardLine,
                0.34f,
                14f,
                true,
                0.62f,
                0.12f,
                0f,
                0.48f,
                14f,
                0.025f,
                0.22f,
                2.1f,
                0.25f,
                0.10f,
                CombatAiCameraCueKind.RetreatShot,
                0.9f,
                0.85f,
                0.55f,
                CreateCombatAiCameraCue(new Vector3(-0.04f, 0.03f, -0.12f), 0.08f, 0.55f, -0.05f, 0.02f, 0.24f, 1f),
                CreateCombatAiCameraCue(new Vector3(0f, 0.04f, 0.12f), 0.12f, 0.9f, -0.10f, 0.02f, 0.20f, 1f),
                CreateCombatAiCameraCue(new Vector3(0f, 0.02f, 0.10f), 0.05f, -0.5f, 0.07f, -0.02f, 0.22f, 1f),
                new Vector3(0.18f, 0.02f, 1.0f),
                new Vector3(0.34f, 0.02f, 4.6f),
                new Vector3(0.42f, 0.025f, 5.4f),
                new Vector3(0f, 0f, -0.18f),
                new Vector3(0f, 0f, 0.02f),
                new Color(0.45f, 0.9f, 1f, 1f),
                new Color(0.08f, 0.55f, 1f, 1f),
                new Color(0.78f, 0.96f, 1f, 1f),
                "RetreatBackstep",
                "AttackRetreatShot",
                "Hit",
                "Death");

            ConfigureEnemyPatternProfile(
                retreatBlink,
                "SciFiSoldier.Ranged",
                "RetreatBlink",
                2.3f,
                620f,
                -24f,
                0.18f,
                8.5f,
                true,
                4.4f,
                0.2f,
                CombatAiAttackShape.ForwardLine,
                0.48f,
                18f,
                true,
                0.52f,
                0.10f,
                0f,
                0.52f,
                12f,
                0.02f,
                0.20f,
                2.6f,
                0.65f,
                0.12f,
                CombatAiCameraCueKind.RetreatBlink,
                1.0f,
                0.95f,
                0.55f,
                CreateCombatAiCameraCue(new Vector3(-0.08f, 0.04f, -0.18f), 0.12f, 0.8f, -0.08f, 0.03f, 0.22f, 1f),
                CreateCombatAiCameraCue(new Vector3(0f, 0.04f, 0.16f), 0.16f, 1.0f, -0.12f, 0.03f, 0.18f, 1f),
                CreateCombatAiCameraCue(new Vector3(0f, 0.02f, 0.10f), 0.05f, -0.5f, 0.07f, -0.02f, 0.22f, 1f),
                new Vector3(0.24f, 0.02f, 0.9f),
                new Vector3(0.56f, 0.02f, 3.8f),
                new Vector3(0.68f, 0.025f, 4.4f),
                new Vector3(0f, 0f, -0.22f),
                new Vector3(0f, 0f, 0.02f),
                new Color(0.75f, 0.45f, 1f, 1f),
                new Color(0.38f, 0.12f, 1f, 1f),
                new Color(0.92f, 0.82f, 1f, 1f),
                "RetreatBlink",
                "AttackRetreatBlink",
                "Hit",
                "Death");

            ConfigureEnemyPatternProfile(
                guardBreak,
                "SciFiSoldier.Elite",
                "GuardBreak",
                1.85f,
                360f,
                -24f,
                0f,
                0f,
                false,
                2.05f,
                0.15f,
                CombatAiAttackShape.MeleeArc,
                1.15f,
                42f,
                false,
                1.12f,
                0.18f,
                0.9f,
                0.92f,
                28f,
                0.045f,
                0.34f,
                2.4f,
                0.2f,
                0.08f,
                CombatAiCameraCueKind.GuardBreak,
                1.45f,
                1.25f,
                0.65f,
                CreateCombatAiCameraCue(new Vector3(0.12f, 0.05f, -0.16f), 0.18f, 1.0f, -0.12f, 0.04f, 0.34f, 1f),
                CreateCombatAiCameraCue(new Vector3(0f, 0.06f, 0.18f), 0.14f, 1.6f, -0.16f, 0.04f, 0.24f, 1f),
                CreateCombatAiCameraCue(new Vector3(0f, 0.02f, 0.10f), 0.06f, -0.6f, 0.08f, -0.02f, 0.24f, 1f),
                new Vector3(0.62f, 0.02f, 0.9f),
                new Vector3(2.2f, 0.02f, 2.25f),
                new Vector3(2.55f, 0.03f, 2.55f),
                new Vector3(0f, 0f, -0.24f),
                new Vector3(0f, 0f, 0.18f),
                new Color(1f, 0.86f, 0.20f, 1f),
                new Color(1f, 0.28f, 0.04f, 1f),
                new Color(1f, 0.94f, 0.48f, 1f),
                string.Empty,
                "AttackGuardBreak",
                "HitHeavy",
                "Death");

            ConfigureCombatAiPatternDeck(
                generalDeck,
                "SciFiSoldier.GeneralExpanded",
                new[]
                {
                    CreateCombatAiPatternDeckEntry(closePunish, 0f, 1.9f, 0.55f, 5f),
                    CreateCombatAiPatternDeckEntry(lungeStrike, 1.2f, 3.1f, 0.85f, 4f),
                    CreateCombatAiPatternDeckEntry(retreatShot, 0f, 2.2f, 1.15f, 3.5f),
                    CreateCombatAiPatternDeckEntry(fanPressure, 2.0f, 5.1f, 1.0f, 3f),
                    CreateCombatAiPatternDeckEntry(linePressure, 3.2f, 6.8f, 1.1f, 2.5f),
                    CreateCombatAiPatternDeckEntry(retreatBlink, 0f, 3.4f, 2.6f, 1.8f)
                });

            ConfigureCombatAiPatternDeck(
                eliteDeck,
                "SciFiSoldier.EliteTraining",
                new[]
                {
                    CreateCombatAiPatternDeckEntry(closePunish, 0f, 1.9f, 0.65f, 4.5f),
                    CreateCombatAiPatternDeckEntry(guardBreak, 0f, 2.5f, 1.35f, 4f),
                    CreateCombatAiPatternDeckEntry(heavyWindup, 1.0f, 2.8f, 1.4f, 3.5f),
                    CreateCombatAiPatternDeckEntry(fanPressure, 2.0f, 5.1f, 1.15f, 3f),
                    CreateCombatAiPatternDeckEntry(linePressure, 3.2f, 7.2f, 1.2f, 2.5f),
                    CreateCombatAiPatternDeckEntry(retreatShot, 0f, 3.0f, 1.4f, 2f)
                });

            ConfigureCombatAiPatternDeck(
                phaseTwoDeck,
                "SciFiSoldier.ElitePhaseTwo",
                new[]
                {
                    CreateCombatAiPatternDeckEntry(guardBreak, 0f, 2.6f, 1.05f, 4.5f),
                    CreateCombatAiPatternDeckEntry(heavyWindup, 1.0f, 3.0f, 1.05f, 4f),
                    CreateCombatAiPatternDeckEntry(retreatBlink, 0f, 3.6f, 1.6f, 3.2f),
                    CreateCombatAiPatternDeckEntry(fanPressure, 2.0f, 5.4f, 0.95f, 3f),
                    CreateCombatAiPatternDeckEntry(linePressure, 3.0f, 7.5f, 1.0f, 2.8f)
                });

            ConfigureElitePatternProfile(
                shieldCycle,
                "ShieldCycle",
                CombatAiElitePatternKind.ShieldCycle,
                1f,
                1.1f,
                70f,
                0.35f,
                0.9f,
                4.8f,
                null,
                null,
                new Color(0.25f, 0.75f, 1f, 1f),
                "EliteShieldCycle");
            ConfigureElitePatternProfile(
                armorBreak,
                "ArmorBreak",
                CombatAiElitePatternKind.ArmorBreak,
                1f,
                1.2f,
                120f,
                0.55f,
                1.15f,
                0f,
                null,
                null,
                new Color(1f, 0.78f, 0.24f, 1f),
                "EliteArmorBreak");
            ConfigureElitePatternProfile(
                auraBuffer,
                "AuraBuffer",
                CombatAiElitePatternKind.AuraBuffer,
                1f,
                9999f,
                0f,
                0.85f,
                0f,
                0f,
                null,
                null,
                new Color(0.35f, 1f, 0.55f, 1f),
                "EliteAuraBuffer");
            ConfigureElitePatternProfile(
                summonPackage,
                "SummonPackage",
                CombatAiElitePatternKind.SummonPackage,
                0.75f,
                1.6f,
                0f,
                1f,
                0f,
                0f,
                null,
                null,
                new Color(0.78f, 0.38f, 1f, 1f),
                "EliteSummonPackage");
            ConfigureElitePatternProfile(
                phaseSwap,
                "PhaseSwap",
                CombatAiElitePatternKind.PhaseSwap,
                0.5f,
                1.35f,
                0f,
                1f,
                0f,
                0f,
                heavyWindup,
                phaseTwoDeck,
                new Color(1f, 0.28f, 0.18f, 1f),
                "ElitePhaseSwap");

            AssetDatabase.SaveAssets();
        }

        public static void EnsureExtendedSceneSamples()
        {
            Scene scene = EditorSceneManager.OpenScene(ActionFoundationProfileSetup.ScenePath, OpenSceneMode.Single);
            GameObject[] roots = scene.GetRootGameObjects();
            PlayerActionController playerActions = RequireObject<PlayerActionController>(roots, "player actions");
            PlayerCombatTargetSelector targetSelector = RequireComponent<PlayerCombatTargetSelector>(playerActions.gameObject, "player target selector");
            ActionCameraController cameraController = RequireObject<ActionCameraController>(roots, "action camera");
            BasicSoldierEnemy template = RequireRootComponent<BasicSoldierEnemy>(roots, ActionFoundationProfileSetup.ClosePunishEnemyRootName, "close punish sample");
            CombatHealth playerHealth = RequireComponent<CombatHealth>(playerActions.gameObject, "player health");

            CombatAiPatternProfile closePunish = LoadOrCreateEnemyPatternProfile(ActionFoundationProfileSetup.EnemyPatternProfilePath);
            CombatAiPatternProfile retreatShot = LoadOrCreateEnemyPatternProfile(RetreatShotPatternPath);
            CombatAiPatternProfile retreatBlink = LoadOrCreateEnemyPatternProfile(RetreatBlinkPatternPath);
            CombatAiPatternProfile guardBreak = LoadOrCreateEnemyPatternProfile(GuardBreakPatternPath);
            CombatAiPatternDeck generalDeck = LoadOrCreate<CombatAiPatternDeck>(GeneralPatternDeckPath);
            CombatAiPatternDeck eliteDeck = LoadOrCreate<CombatAiPatternDeck>(ElitePatternDeckPath);
            CombatAiElitePatternProfile shieldCycle = LoadOrCreate<CombatAiElitePatternProfile>(ShieldCycleEliteProfilePath);
            CombatAiElitePatternProfile armorBreak = LoadOrCreate<CombatAiElitePatternProfile>(ArmorBreakEliteProfilePath);
            CombatAiElitePatternProfile auraBuffer = LoadOrCreate<CombatAiElitePatternProfile>(AuraBufferEliteProfilePath);
            CombatAiElitePatternProfile summonPackage = LoadOrCreate<CombatAiElitePatternProfile>(SummonPackageEliteProfilePath);
            CombatAiElitePatternProfile phaseSwap = LoadOrCreate<CombatAiElitePatternProfile>(PhaseSwapEliteProfilePath);

            BasicSoldierEnemy retreatShotSoldier = ActionFoundationProfileSetup.EnsurePatternSampleEnemy(scene, template, RetreatShotEnemyRootName);
            BasicSoldierEnemy retreatBlinkSoldier = ActionFoundationProfileSetup.EnsurePatternSampleEnemy(scene, template, RetreatBlinkEnemyRootName);
            BasicSoldierEnemy guardBreakSoldier = ActionFoundationProfileSetup.EnsurePatternSampleEnemy(scene, template, GuardBreakEnemyRootName);
            BasicSoldierEnemy generalDeckSoldier = ActionFoundationProfileSetup.EnsurePatternSampleEnemy(scene, template, GeneralDeckEnemyRootName);
            BasicSoldierEnemy eliteDeckSoldier = ActionFoundationProfileSetup.EnsurePatternSampleEnemy(scene, template, EliteDeckEnemyRootName);
            BasicSoldierEnemy eliteTraitsSoldier = ActionFoundationProfileSetup.EnsurePatternSampleEnemy(scene, template, EliteTraitsEnemyRootName);

            ActionFoundationProfileSetup.ConfigurePatternSampleEnemy(
                retreatShotSoldier,
                RetreatShotEnemyRootName,
                "RetreatShot",
                retreatShot,
                new Vector3(-7.2f, 0f, 5.8f),
                playerActions.transform,
                playerHealth,
                cameraController,
                "RetreatShot");
            ActionFoundationProfileSetup.ConfigurePatternSampleEnemy(
                retreatBlinkSoldier,
                RetreatBlinkEnemyRootName,
                "RetreatBlink",
                retreatBlink,
                new Vector3(-8.4f, 0f, 3.8f),
                playerActions.transform,
                playerHealth,
                cameraController,
                "RetreatBlink");
            ActionFoundationProfileSetup.ConfigurePatternSampleEnemy(
                guardBreakSoldier,
                GuardBreakEnemyRootName,
                "GuardBreak",
                guardBreak,
                new Vector3(7.2f, 0f, 5.8f),
                playerActions.transform,
                playerHealth,
                cameraController,
                "GuardBreak");
            ActionFoundationProfileSetup.ConfigurePatternSampleEnemy(
                generalDeckSoldier,
                GeneralDeckEnemyRootName,
                "GeneralDeck",
                closePunish,
                generalDeck,
                new Vector3(-7.2f, 0f, 8.8f),
                playerActions.transform,
                playerHealth,
                cameraController,
                "GeneralDeck");
            ActionFoundationProfileSetup.ConfigurePatternSampleEnemy(
                eliteDeckSoldier,
                EliteDeckEnemyRootName,
                "EliteDeck",
                guardBreak,
                eliteDeck,
                new Vector3(7.2f, 0f, 8.8f),
                playerActions.transform,
                playerHealth,
                cameraController,
                "EliteDeck");
            ActionFoundationProfileSetup.ConfigurePatternSampleEnemy(
                eliteTraitsSoldier,
                EliteTraitsEnemyRootName,
                "EliteTraits",
                guardBreak,
                eliteDeck,
                new Vector3(0f, 0f, 10.8f),
                playerActions.transform,
                playerHealth,
                cameraController,
                "EliteTraits");

            ConfigureEliteTraitsSample(
                eliteTraitsSoldier,
                RequireComponent<CombatHealth>(eliteDeckSoldier.gameObject, "elite deck health"),
                new[] { shieldCycle, armorBreak, auraBuffer, summonPackage, phaseSwap });

            CombatHealth[] enemyCandidates = CollectAuthoredEnemyCandidates(scene.GetRootGameObjects());
            ActionFoundationProfileSetup.ConfigurePlayerTargetSelector(
                targetSelector,
                playerActions.transform,
                playerHealth,
                cameraController.transform,
                enemyCandidates);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }

        private static void ConfigureEliteTraitsSample(
            BasicSoldierEnemy soldier,
            CombatHealth auraProtectedTarget,
            CombatAiElitePatternProfile[] profiles)
        {
            EnemyElitePatternController controller = soldier.GetComponent<EnemyElitePatternController>();
            if (controller == null)
            {
                controller = soldier.gameObject.AddComponent<EnemyElitePatternController>();
            }

            GameObject summonSignal = EnsureSummonSignalObject(soldier.transform);
            Renderer cueRenderer = ResolveCueRenderer(soldier);
            Animator animator = soldier.GetComponentInChildren<Animator>(includeInactive: true);
            CombatHealth soldierHealth = RequireComponent<CombatHealth>(soldier.gameObject, $"{soldier.name} health");

            SerializedObject serializedObject = new SerializedObject(controller);
            SetObjectReference(serializedObject, "health", soldierHealth);
            SetObjectReference(serializedObject, "soldier", soldier);
            SetObjectReference(serializedObject, "animator", animator);
            SetObjectReference(serializedObject, "cueRenderer", cueRenderer);
            SetObjectReferenceArray(serializedObject, "eliteProfiles", profiles);
            SetObjectReferenceArray(serializedObject, "auraProtectedTargets", new UnityEngine.Object[] { auraProtectedTarget });
            SetObjectReferenceArray(serializedObject, "summonSignalObjects", new UnityEngine.Object[] { summonSignal });
            serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(controller);
            EditorUtility.SetDirty(summonSignal);
        }

        private static GameObject EnsureSummonSignalObject(Transform owner)
        {
            GameObject existing = FindChildObject(owner, "SummonPackageSignal_EliteTraits");
            if (existing != null)
            {
                existing.SetActive(false);
                return existing;
            }

            GameObject signal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            signal.name = "SummonPackageSignal_EliteTraits";
            signal.transform.SetParent(owner, false);
            signal.transform.localPosition = new Vector3(0f, 0.025f, 1.8f);
            signal.transform.localRotation = Quaternion.identity;
            signal.transform.localScale = new Vector3(1.8f, 0.02f, 1.8f);
            signal.SetActive(false);
            Collider collider = signal.GetComponent<Collider>();
            if (collider != null)
            {
                UnityEngine.Object.DestroyImmediate(collider);
            }

            return signal;
        }

        private static Renderer ResolveCueRenderer(BasicSoldierEnemy soldier)
        {
            Renderer[] renderers = soldier.GetComponentsInChildren<Renderer>(includeInactive: true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer != null && renderer.name.IndexOf("ReadableAttackTelegraph", StringComparison.Ordinal) < 0)
                {
                    return renderer;
                }
            }

            return null;
        }

        private static CombatHealth[] CollectAuthoredEnemyCandidates(GameObject[] roots)
        {
            string[] names =
            {
                ActionFoundationProfileSetup.ClosePunishEnemyRootName,
                ActionFoundationProfileSetup.LungeStrikeEnemyRootName,
                ActionFoundationProfileSetup.HeavyWindupEnemyRootName,
                ActionFoundationProfileSetup.LinePressureEnemyRootName,
                ActionFoundationProfileSetup.FanPressureEnemyRootName,
                ActionFoundationProfileSetup.TrainingDeckEnemyRootName,
                RetreatShotEnemyRootName,
                RetreatBlinkEnemyRootName,
                GuardBreakEnemyRootName,
                GeneralDeckEnemyRootName,
                EliteDeckEnemyRootName,
                EliteTraitsEnemyRootName
            };
            List<CombatHealth> candidates = new List<CombatHealth>(names.Length);
            for (int i = 0; i < names.Length; i++)
            {
                BasicSoldierEnemy soldier = RequireRootComponent<BasicSoldierEnemy>(roots, names[i], names[i]);
                candidates.Add(RequireComponent<CombatHealth>(soldier.gameObject, $"{names[i]} health"));
            }

            return candidates.ToArray();
        }

        private static void ConfigureEnemyPatternProfile(
            CombatAiPatternProfile profile,
            string actorTypeId,
            string patternId,
            float approachSpeed,
            float turnRateDegrees,
            float gravity,
            float prepareSeconds,
            float prepareRetreatSpeed,
            bool lockAttackDirectionAfterPrepare,
            float attackRange,
            float attackFacingDotThreshold,
            CombatAiAttackShape attackShape,
            float attackHalfWidth,
            float attackHalfAngleDegrees,
            bool lockAttackDirectionOnWindup,
            float telegraphSeconds,
            float activeSeconds,
            float activeLungeSpeed,
            float recoverySeconds,
            float damage,
            float hitStopSeconds,
            float hitReactionSeconds,
            float knockbackSpeed,
            float recoveryRetreatSpeed,
            float recoveryRetreatSeconds,
            CombatAiCameraCueKind cameraCueKind,
            float windupThreatLevel,
            float activeCameraCueStrength,
            float deathCameraCueStrength,
            CombatAiCameraCue windupCameraCue,
            CombatAiCameraCue activeCameraCue,
            CombatAiCameraCue deathCameraCue,
            Vector3 telegraphWindupStartScale,
            Vector3 telegraphWindupEndScale,
            Vector3 telegraphActiveScale,
            Vector3 windupPoseOffset,
            Vector3 activePoseOffset,
            Color windupStartColor,
            Color windupEndColor,
            Color activeColor,
            string prepareTrigger,
            string attackTrigger,
            string hitTrigger,
            string deathTrigger)
        {
            SerializedObject serializedObject = new SerializedObject(profile);
            SetString(serializedObject, "actorTypeId", actorTypeId);
            SetString(serializedObject, "patternId", patternId);
            SetFloat(serializedObject, "approachSpeed", approachSpeed);
            SetFloat(serializedObject, "turnRateDegrees", turnRateDegrees);
            SetFloat(serializedObject, "gravity", gravity);
            SetFloat(serializedObject, "prepareSeconds", prepareSeconds);
            SetFloat(serializedObject, "prepareRetreatSpeed", prepareRetreatSpeed);
            SetBool(serializedObject, "lockAttackDirectionAfterPrepare", lockAttackDirectionAfterPrepare);
            SetFloat(serializedObject, "attackRange", attackRange);
            SetFloat(serializedObject, "attackFacingDotThreshold", attackFacingDotThreshold);
            SetEnum(serializedObject, "attackShape", (int)attackShape);
            SetFloat(serializedObject, "attackHalfWidth", attackHalfWidth);
            SetFloat(serializedObject, "attackHalfAngleDegrees", attackHalfAngleDegrees);
            SetBool(serializedObject, "lockAttackDirectionOnWindup", lockAttackDirectionOnWindup);
            SetFloat(serializedObject, "telegraphSeconds", telegraphSeconds);
            SetFloat(serializedObject, "activeSeconds", activeSeconds);
            SetFloat(serializedObject, "activeLungeSpeed", activeLungeSpeed);
            SetFloat(serializedObject, "recoverySeconds", recoverySeconds);
            SetFloat(serializedObject, "damage", damage);
            SetFloat(serializedObject, "hitStopSeconds", hitStopSeconds);
            SetFloat(serializedObject, "hitReactionSeconds", hitReactionSeconds);
            SetFloat(serializedObject, "knockbackSpeed", knockbackSpeed);
            SetFloat(serializedObject, "recoveryRetreatSpeed", recoveryRetreatSpeed);
            SetFloat(serializedObject, "recoveryRetreatSeconds", recoveryRetreatSeconds);
            SetEnum(serializedObject, "cameraCueKind", (int)cameraCueKind);
            SetFloat(serializedObject, "windupThreatLevel", windupThreatLevel);
            SetFloat(serializedObject, "activeCameraCueStrength", activeCameraCueStrength);
            SetFloat(serializedObject, "deathCameraCueStrength", deathCameraCueStrength);
            SetCombatAiCameraCue(serializedObject.FindProperty("windupCameraCue"), windupCameraCue);
            SetCombatAiCameraCue(serializedObject.FindProperty("activeCameraCue"), activeCameraCue);
            SetCombatAiCameraCue(serializedObject.FindProperty("deathCameraCue"), deathCameraCue);
            SetVector3(serializedObject, "telegraphWindupStartScale", telegraphWindupStartScale);
            SetVector3(serializedObject, "telegraphWindupEndScale", telegraphWindupEndScale);
            SetVector3(serializedObject, "telegraphActiveScale", telegraphActiveScale);
            SetVector3(serializedObject, "windupPoseOffset", windupPoseOffset);
            SetVector3(serializedObject, "activePoseOffset", activePoseOffset);
            SetColor(serializedObject, "windupStartColor", windupStartColor);
            SetColor(serializedObject, "windupEndColor", windupEndColor);
            SetColor(serializedObject, "activeColor", activeColor);
            SetString(serializedObject, "moveSpeedParameter", "MoveSpeed");
            SetString(serializedObject, "prepareTrigger", prepareTrigger);
            SetString(serializedObject, "attackTrigger", attackTrigger);
            SetString(serializedObject, "hitTrigger", hitTrigger);
            SetString(serializedObject, "deathTrigger", deathTrigger);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static void ConfigureElitePatternProfile(
            CombatAiElitePatternProfile profile,
            string patternId,
            CombatAiElitePatternKind patternKind,
            float triggerHealthRatio,
            float signalSeconds,
            float guardMeter,
            float damageTakenMultiplier,
            float breakSeconds,
            float refreshSeconds,
            CombatAiPatternProfile replacementPatternProfile,
            CombatAiPatternDeck replacementPatternDeck,
            Color cueColor,
            string signalAnimationTrigger)
        {
            SerializedObject serializedObject = new SerializedObject(profile);
            SetString(serializedObject, "patternId", patternId);
            SetEnum(serializedObject, "patternKind", (int)patternKind);
            SetFloat(serializedObject, "triggerHealthRatio", triggerHealthRatio);
            SetFloat(serializedObject, "signalSeconds", signalSeconds);
            SetFloat(serializedObject, "guardMeter", guardMeter);
            SetFloat(serializedObject, "damageTakenMultiplier", damageTakenMultiplier);
            SetFloat(serializedObject, "breakSeconds", breakSeconds);
            SetFloat(serializedObject, "refreshSeconds", refreshSeconds);
            SetObjectReference(serializedObject, "replacementPatternProfile", replacementPatternProfile);
            SetObjectReference(serializedObject, "replacementPatternDeck", replacementPatternDeck);
            SetColor(serializedObject, "cueColor", cueColor);
            SetString(serializedObject, "signalAnimationTrigger", signalAnimationTrigger);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static void ConfigureCombatAiPatternDeck(
            CombatAiPatternDeck deck,
            string deckId,
            CombatAiPatternDeckEntry[] entries)
        {
            SerializedObject serializedObject = new SerializedObject(deck);
            SetString(serializedObject, "deckId", deckId);
            SerializedProperty entryArray = RequireProperty(serializedObject, "entries");
            entryArray.arraySize = entries.Length;
            for (int i = 0; i < entries.Length; i++)
            {
                SetCombatAiPatternDeckEntry(entryArray.GetArrayElementAtIndex(i), entries[i]);
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(deck);
        }

        private static CombatAiPatternDeckEntry CreateCombatAiPatternDeckEntry(
            CombatAiPatternProfile profile,
            float minimumDistance,
            float maximumDistance,
            float cooldownSeconds,
            float priority)
        {
            return new CombatAiPatternDeckEntry(profile, minimumDistance, maximumDistance, cooldownSeconds, priority);
        }

        private static void SetCombatAiPatternDeckEntry(SerializedProperty entry, CombatAiPatternDeckEntry value)
        {
            SetObjectReference(entry, "profile", value.Profile);
            SetRelativeFloat(entry, "minimumDistance", value.MinimumDistance);
            SetRelativeFloat(entry, "maximumDistance", value.MaximumDistance);
            SetRelativeFloat(entry, "cooldownSeconds", value.CooldownSeconds);
            SetRelativeFloat(entry, "priority", value.Priority);
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

        private static CombatAiPatternProfile LoadOrCreateEnemyPatternProfile(string assetPath)
        {
            CombatAiPatternProfile asset = AssetDatabase.LoadAssetAtPath<CombatAiPatternProfile>(assetPath);
            if (asset != null)
            {
                return asset;
            }

            asset = ScriptableObject.CreateInstance<EnemyPatternProfile>();
            AssetDatabase.CreateAsset(asset, assetPath);
            return asset;
        }

        private static CombatAiCameraCue CreateCombatAiCameraCue(
            Vector3 localOffset,
            float planarDirectionOffset,
            float fieldOfViewDelta,
            float cameraDistanceDelta,
            float focusHeightDelta,
            float durationSeconds,
            float finisherScale)
        {
            return new CombatAiCameraCue
            {
                enabled = true,
                localOffset = localOffset,
                planarDirectionOffset = planarDirectionOffset,
                fieldOfViewDelta = fieldOfViewDelta,
                cameraDistanceDelta = cameraDistanceDelta,
                focusHeightDelta = focusHeightDelta,
                durationSeconds = durationSeconds,
                finisherScale = finisherScale
            };
        }

        private static void SetCombatAiCameraCue(SerializedProperty cue, CombatAiCameraCue value)
        {
            SetRelativeBool(cue, "enabled", value.enabled);
            SetRelativeVector3(cue, "localOffset", value.localOffset);
            SetRelativeFloat(cue, "planarDirectionOffset", value.planarDirectionOffset);
            SetRelativeFloat(cue, "fieldOfViewDelta", value.fieldOfViewDelta);
            SetRelativeFloat(cue, "cameraDistanceDelta", value.cameraDistanceDelta);
            SetRelativeFloat(cue, "focusHeightDelta", value.focusHeightDelta);
            SetRelativeFloat(cue, "durationSeconds", value.durationSeconds);
            SetRelativeFloat(cue, "finisherScale", value.finisherScale);
        }

        private static void SetString(SerializedObject serializedObject, string propertyName, string value)
        {
            RequireProperty(serializedObject, propertyName).stringValue = value;
        }

        private static void SetFloat(SerializedObject serializedObject, string propertyName, float value)
        {
            RequireProperty(serializedObject, propertyName).floatValue = value;
        }

        private static void SetBool(SerializedObject serializedObject, string propertyName, bool value)
        {
            RequireProperty(serializedObject, propertyName).boolValue = value;
        }

        private static void SetEnum(SerializedObject serializedObject, string propertyName, int value)
        {
            RequireProperty(serializedObject, propertyName).enumValueIndex = value;
        }

        private static void SetVector3(SerializedObject serializedObject, string propertyName, Vector3 value)
        {
            RequireProperty(serializedObject, propertyName).vector3Value = value;
        }

        private static void SetColor(SerializedObject serializedObject, string propertyName, Color value)
        {
            RequireProperty(serializedObject, propertyName).colorValue = value;
        }

        private static void SetObjectReference(SerializedObject serializedObject, string propertyName, UnityEngine.Object value)
        {
            RequireProperty(serializedObject, propertyName).objectReferenceValue = value;
        }

        private static void SetObjectReferenceArray(SerializedObject serializedObject, string propertyName, UnityEngine.Object[] values)
        {
            SerializedProperty property = RequireProperty(serializedObject, propertyName);
            if (!property.isArray)
            {
                throw new InvalidOperationException($"{serializedObject.targetObject.name}.{propertyName} is not an array property.");
            }

            property.ClearArray();
            for (int i = 0; i < values.Length; i++)
            {
                property.InsertArrayElementAtIndex(i);
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }
        }

        private static void SetObjectReference(SerializedProperty owner, string propertyName, UnityEngine.Object value)
        {
            owner.FindPropertyRelative(propertyName).objectReferenceValue = value;
        }

        private static void SetRelativeFloat(SerializedProperty owner, string propertyName, float value)
        {
            owner.FindPropertyRelative(propertyName).floatValue = value;
        }

        private static void SetRelativeBool(SerializedProperty owner, string propertyName, bool value)
        {
            owner.FindPropertyRelative(propertyName).boolValue = value;
        }

        private static void SetRelativeVector3(SerializedProperty owner, string propertyName, Vector3 value)
        {
            owner.FindPropertyRelative(propertyName).vector3Value = value;
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

        private static T RequireRootComponent<T>(GameObject[] roots, string rootName, string label) where T : Component
        {
            for (int i = 0; i < roots.Length; i++)
            {
                GameObject root = roots[i];
                if (root != null && root.name == rootName)
                {
                    T component = root.GetComponent<T>();
                    if (component != null)
                    {
                        return component;
                    }
                }
            }

            throw new InvalidOperationException($"Missing required {label} root component named {rootName}.");
        }

        private static T RequireComponent<T>(GameObject owner, string label) where T : Component
        {
            if (!owner.TryGetComponent(out T component))
            {
                throw new InvalidOperationException($"{owner.name} is missing required {label}.");
            }

            return component;
        }

        private static GameObject FindChildObject(Transform root, string objectName)
        {
            Transform[] children = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < children.Length; i++)
            {
                if (children[i] != null && children[i].name == objectName)
                {
                    return children[i].gameObject;
                }
            }

            return null;
        }
    }
}
