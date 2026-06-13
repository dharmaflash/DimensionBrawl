using System.Collections;
using System.Collections.Generic;
using DimensionBrawl.AI;
using DimensionBrawl.Combat;
using DimensionBrawl.Enemies;
using DimensionBrawl.Player;
using DimensionBrawl.Presentation;
using DimensionBrawl.Test;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace DimensionBrawl.Tests
{
    public sealed class ActionFoundationPlayModeTests
    {
        private const string ScenePath = "Assets/_Game/Scenes/ActionFoundationTest.unity";
        private const string EnemyPrefabReviewScenePath = "Assets/_Game/Scenes/ActionFoundationEnemyPrefabReview.unity";
        private const string ClosePunishPatternPath = "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_BasicSoldier_ClosePunish.asset";
        private const string LungeStrikePatternPath = "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_BasicSoldier_LungeStrike.asset";
        private const string HeavyWindupPatternPath = "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_BasicSoldier_HeavyWindup.asset";
        private const string LinePressurePatternPath = "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_BasicSoldier_LinePressure.asset";
        private const string FanPressurePatternPath = "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_BasicSoldier_FanPressure.asset";
        private const string RetreatShotPatternPath = "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_BasicSoldier_RetreatShot.asset";
        private const string RetreatBlinkPatternPath = "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_BasicSoldier_RetreatBlink.asset";
        private const string GuardBreakPatternPath = "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_EliteSoldier_GuardBreak.asset";
        private const string BasicSoldierTrainingDeckPath = "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_BasicSoldier_TrainingDeck.asset";
        private const string GeneralPatternDeckPath = "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_BasicSoldier_GeneralPatternDeck.asset";
        private const string ElitePatternDeckPath = "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_EliteSoldier_PatternDeck.asset";
        private const string ElitePhaseTwoPatternDeckPath = "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_EliteSoldier_PhaseTwoPatternDeck.asset";
        private const string ShieldCycleEliteProfilePath = "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_ElitePattern_ShieldCycle.asset";
        private const string ArmorBreakEliteProfilePath = "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_ElitePattern_ArmorBreak.asset";
        private const string AuraBufferEliteProfilePath = "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_ElitePattern_AuraBuffer.asset";
        private const string SummonPackageEliteProfilePath = "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_ElitePattern_SummonPackage.asset";
        private const string PhaseSwapEliteProfilePath = "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_ElitePattern_PhaseSwap.asset";
        private const string CombatVfxCueProfilePath = "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_CombatVfxCues_ActionFoundation.asset";
        private const string CombatVfxPrefabRootPath = "Assets/_Game/Art/VFX/CombatCues/Prefabs/";
        private const string EnemyRoleRootPath = "Assets/_Game/DesignData/Profiles/ActionFoundation/EnemyRoles";
        private const string EnemyArchetypeRootPath = "Assets/_Game/DesignData/Profiles/ActionFoundation/EnemyArchetypes";
        private const string MeleeSoldierPrefabPath = "Assets/_Game/Prefabs/Enemies/ActionFoundation/PF_Enemy_SciFiSoldier_Melee_ClosePunish.prefab";
        private const string PlayerVisualName = "CombatGirlSwordShield_PlayerVisual";
        private const string EnemyVisualName = "MaintenanceWorker_BasicSoldierVisual";
        private const string EnemyPlaceholderBodyName = "SciFiSoldierPlaceholderBody";
        private const string ClosePunishEnemyRootName = "Enemy_SciFiSoldier_ClosePunish";
        private const string LungeStrikeEnemyRootName = "Enemy_SciFiSoldier_LungeStrike";
        private const string HeavyWindupEnemyRootName = "Enemy_SciFiSoldier_HeavyWindup";
        private const string LinePressureEnemyRootName = "Enemy_SciFiSoldier_LinePressure";
        private const string FanPressureEnemyRootName = "Enemy_SciFiSoldier_FanPressure";
        private const string TrainingDeckEnemyRootName = "Enemy_SciFiSoldier_TrainingDeck";
        private const string RetreatShotEnemyRootName = "Enemy_SciFiSoldier_RetreatShot";
        private const string RetreatBlinkEnemyRootName = "Enemy_SciFiSoldier_RetreatBlink";
        private const string GuardBreakEnemyRootName = "Enemy_SciFiSoldier_GuardBreak";
        private const string GeneralDeckEnemyRootName = "Enemy_SciFiSoldier_GeneralDeck";
        private const string EliteDeckEnemyRootName = "Enemy_SciFiSoldier_EliteDeck";
        private const string EliteTraitsEnemyRootName = "Enemy_SciFiSoldier_EliteTraits";
        private const string ReviewMeleeSoldierRootName = "EnemyPrefabReview_SciFiSoldier_Melee";
        private static readonly string[] EnemyRoleProfilePaths =
        {
            EnemyRoleRootPath + "/DB_Role_EntryProbe.asset",
            EnemyRoleRootPath + "/DB_Role_CloseGuard.asset",
            EnemyRoleRootPath + "/DB_Role_LungeChaser.asset",
            EnemyRoleRootPath + "/DB_Role_LineCaster.asset",
            EnemyRoleRootPath + "/DB_Role_FanSuppressor.asset",
            EnemyRoleRootPath + "/DB_Role_BacklineShooter.asset",
            EnemyRoleRootPath + "/DB_Role_Skirmisher.asset",
            EnemyRoleRootPath + "/DB_Role_ShieldBreakerElite.asset",
            EnemyRoleRootPath + "/DB_Role_AuraCaptainElite.asset",
            EnemyRoleRootPath + "/DB_Role_SummonCallerElite.asset",
            EnemyRoleRootPath + "/DB_Role_PhaseDuelistElite.asset",
            EnemyRoleRootPath + "/DB_Role_FinalStandCommanderElite.asset"
        };
        private static readonly string[] EnemyArchetypeProfilePaths =
        {
            EnemyArchetypeRootPath + "/DB_Archetype_SciFiSoldier_Melee.asset",
            EnemyArchetypeRootPath + "/DB_Archetype_SciFiSoldier_Ranged.asset",
            EnemyArchetypeRootPath + "/DB_Archetype_SciFiSoldier_Elite.asset",
            EnemyArchetypeRootPath + "/DB_Archetype_FORGE3D_LineTurret.asset",
            EnemyArchetypeRootPath + "/DB_Archetype_FORGE3D_MissileTurret.asset",
            EnemyArchetypeRootPath + "/DB_Archetype_DragonBoss_Future.asset"
        };

        [UnitySetUp]
        public IEnumerator LoadActionFoundationScene()
        {
            Time.timeScale = 1f;
            EditorSceneManager.LoadSceneInPlayMode(ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator ResetTimeScale()
        {
            Time.timeScale = 1f;
            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayerCanMoveDodgeAndDamageSoldier()
        {
            PlayerMovementController movement = RequireObject<PlayerMovementController>();
            PlayerActionController actions = RequireObject<PlayerActionController>();
            CombatHealth enemyHealth = RequireEnemyHealth();
            Animator playerAnimator = RequirePlayerAnimator();

            Vector3 startPosition = movement.transform.position;
            movement.SetMoveInput(Vector2.up);
            yield return new WaitForSeconds(0.35f);
            movement.SetMoveInput(Vector2.zero);
            yield return null;

            Assert.Greater(
                Vector3.Distance(startPosition, movement.transform.position),
                0.25f,
                "Player should move from shared Move input.");
            Assert.IsTrue(movement.IsStopSettling, "Releasing movement should briefly request stop-settle instead of snapping straight to idle.");
            yield return new WaitForSeconds(0.02f);
            Assert.IsTrue(
                AnimatorIsInOrTransitioningTo(playerAnimator, "StopStep"),
                "Movement release should route the CombatGirl Animator through the promoted StopStep state within the fast transition window.");

            movement.SetMoveInput(Vector2.left);
            yield return new WaitForSeconds(0.05f);
            Vector3 preDodgePosition = movement.transform.position;
            actions.QueueDodge();
            yield return new WaitForSeconds(0.1f);
            movement.SetMoveInput(Vector2.zero);

            Assert.IsTrue(actions.IsDodging, "Player should enter dodge state after QueueDodge.");
            Assert.IsTrue(
                AnimatorIsInAnyDirectionalDodge(playerAnimator),
                "Dodge should route through a directional Quickshift state instead of the old single generic dodge trigger.");
            yield return new WaitForSeconds(0.85f);
            Assert.Greater(
                Vector3.Distance(preDodgePosition, movement.transform.position),
                0.25f,
                "Dodge should apply a visible planar movement burst.");

            PositionPlayerForAttack(movement.transform, enemyHealth.transform);
            Physics.SyncTransforms();
            yield return null;

            float enemyStartHealth = enemyHealth.CurrentHealth;
            actions.QueueBasicAttack();
            yield return new WaitForSeconds(0.4f);

            Assert.Less(enemyHealth.CurrentHealth, enemyStartHealth, "Basic attack should damage the soldier.");
        }

        [UnityTest]
        public IEnumerator NoInputDodgeMovesBackwardFromFacing()
        {
            PlayerMovementController movement = RequireObject<PlayerMovementController>();
            PlayerActionController actions = RequireObject<PlayerActionController>();
            Animator playerAnimator = RequirePlayerAnimator();

            movement.SetMoveInput(Vector2.zero);
            movement.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
            Physics.SyncTransforms();
            yield return null;

            Vector3 startPosition = movement.transform.position;
            Vector3 facingDirection = movement.FacingDirection.normalized;

            actions.QueueDodge();
            yield return new WaitForSeconds(0.05f);

            Assert.IsTrue(actions.IsDodging, "No-input dodge should still start.");
            Assert.Less(
                Vector3.Dot(actions.LastDodgeDirection.normalized, facingDirection),
                -0.95f,
                "No-input dodge should resolve backward from the current facing direction.");
            Assert.IsTrue(
                AnimatorIsInOrTransitioningTo(playerAnimator, "DodgeBack"),
                "No-input dodge should route to the backward Quickshift state.");

            yield return new WaitForSeconds(0.2f);

            Assert.Less(
                Vector3.Dot(movement.transform.position - startPosition, facingDirection),
                -0.1f,
                "No-input dodge should move behind the current facing direction.");
        }

        [UnityTest]
        public IEnumerator PlayerRunStartAndSharpTurnUsePromotedLocomotionClips()
        {
            PlayerMovementController movement = RequireObject<PlayerMovementController>();
            ActionCameraController cameraController = RequireObject<ActionCameraController>();
            RequireObject<ActionCameraCueDriver>();
            Animator playerAnimator = RequirePlayerAnimator();

            movement.SetMoveInput(Vector2.up);
            yield return new WaitForSeconds(0.08f);

            Assert.IsTrue(
                AnimatorIsInOrTransitioningTo(playerAnimator, "StartRun"),
                "Movement start should route through the promoted StartRun clip before settling into Run.");
            Assert.IsTrue(cameraController.HasActiveCue, "Run start should request a short camera cue.");

            yield return new WaitForSeconds(0.75f);

            movement.SetMoveInput(Vector2.right);
            yield return new WaitForSeconds(0.10f);

            Assert.IsTrue(
                AnimatorIsInOrTransitioningTo(playerAnimator, "TurnLeft90")
                    || AnimatorIsInOrTransitioningTo(playerAnimator, "TurnRight90"),
                "A large movement-direction change while running should request a promoted 90-degree turn clip.");
            Assert.IsTrue(cameraController.HasActiveCue, "Sharp movement turn should request a short camera cue.");

            movement.SetMoveInput(Vector2.zero);
        }

        [UnityTest]
        public IEnumerator CombatGirlWeaponSocketsStayPinnedToHands()
        {
            CombatGirlWeaponSocketBinder weaponBinder = RequireObject<CombatGirlWeaponSocketBinder>();

            yield return null;

            Assert.AreEqual(2, weaponBinder.BindingCount, "CombatGirl weapon binder should keep one socket per hand.");
            Assert.IsTrue(weaponBinder.AllBindingsValid, "CombatGirl weapon binder should reference both hands and both weapon sockets.");

            weaponBinder.ApplyBindings();

            Assert.IsTrue(
                weaponBinder.AreBindingsAligned(0.01f, 1f),
                "Weapon sockets should stay aligned to captured hand offsets even when generic humanoid stop clips do not animate add_weapon bones.");
        }

        [UnityTest]
        public IEnumerator PlayerCanReachFiveHitBasicComboState()
        {
            PlayerMovementController movement = RequireObject<PlayerMovementController>();
            PlayerActionController actions = RequireObject<PlayerActionController>();
            CombatHealth enemyHealth = RequireEnemyHealth();
            Animator playerAnimator = RequirePlayerAnimator();

            Assert.IsNotNull(actions.ActionProfile, "Player action timing should come from a game-owned PlayerActionProfile asset.");
            PositionPlayerForAttack(movement.transform, enemyHealth.transform);
            Physics.SyncTransforms();
            yield return null;

            float enemyStartHealth = enemyHealth.CurrentHealth;
            bool reachedFifthHit = false;
            float timeout = 3f;
            float nextInputSeconds = 0.16f;

            actions.QueueBasicAttack();
            while (timeout > 0f)
            {
                yield return null;

                nextInputSeconds -= Time.deltaTime;
                if (nextInputSeconds <= 0f)
                {
                    actions.QueueBasicAttack();
                    nextInputSeconds = 0.18f;
                }

                reachedFifthHit = AnimatorIsInOrTransitioningTo(playerAnimator, "Attack5");
                if (reachedFifthHit)
                {
                    break;
                }

                timeout -= Time.deltaTime;
            }

            Assert.IsTrue(reachedFifthHit, "Queued basic attack input should be able to reach the promoted Attack5 finisher state.");
            Assert.Less(enemyHealth.CurrentHealth, enemyStartHealth, "The five-hit combo route should still apply damage through the combat validation path.");
        }

        [UnityTest]
        public IEnumerator BasicAttackHitDoesNotTriggerGlobalSlowMotion()
        {
            PlayerMovementController movement = RequireObject<PlayerMovementController>();
            PlayerActionController actions = RequireObject<PlayerActionController>();
            CombatHealth enemyHealth = RequireEnemyHealth();

            PositionPlayerForAttack(movement.transform, enemyHealth.transform);
            Physics.SyncTransforms();
            yield return null;

            float enemyStartHealth = enemyHealth.CurrentHealth;
            actions.QueueBasicAttack();

            float timeout = 0.8f;
            while (timeout > 0f && Mathf.Approximately(enemyHealth.CurrentHealth, enemyStartHealth))
            {
                Assert.AreEqual(1f, Time.timeScale, 0.001f, "Normal attack feedback should not drive global slow motion.");
                yield return null;
                timeout -= Time.deltaTime;
            }

            Assert.Less(enemyHealth.CurrentHealth, enemyStartHealth, "Basic attack should damage the soldier during the slow-motion guard test.");
            Assert.AreEqual(1f, Time.timeScale, 0.001f, "Successful normal attack damage should leave global time scale unchanged.");
        }

        [UnityTest]
        public IEnumerator BasicSoldierUsesSharedTargetSensorContract()
        {
            BasicSoldierEnemy soldier = RequirePrimarySoldier();
            ICombatAiAgent agent = soldier;
            CombatTargetSensor targetSensor = soldier.TargetSensor;
            CombatHealth playerHealth = RequirePlayerHealth();

            yield return null;

            Assert.IsNotNull(soldier.PatternProfile, "Basic soldier should read behavior timing from a game-owned EnemyPatternProfile asset.");
            Assert.AreSame(targetSensor, soldier.TargetSensor, "Basic soldier should use the shared combat target sensor instead of private-only target lookup.");
            Assert.AreSame(targetSensor, agent.TargetSensor, "Future enemy and summon logic should consume target sensing through the shared combat AI agent contract.");
            Assert.AreSame(soldier.PatternProfile, agent.PatternProfile, "Combat AI agent contract should expose the active profile without type-specific casts.");
            Assert.AreSame(soldier.SelfHealth, agent.SelfHealth, "Combat AI agent contract should expose self health for team and death handling.");
            Assert.AreEqual("SciFiSoldier.Basic", soldier.EnemyTypeId, "Enemy type should stay serialized so future models and Animator controllers can swap per prefab.");
            Assert.AreEqual("ClosePunish", soldier.PatternId, "The first soldier should declare the reference-backed ClosePunish pattern sample.");
            Assert.AreEqual("Attack", agent.AttackAnimationTrigger, "Combat AI agent contract should expose attack animation requests.");
            Assert.AreEqual("Hit", agent.HitAnimationTrigger, "Combat AI agent contract should expose hit reaction animation requests.");
            Assert.AreEqual("Death", agent.DeathAnimationTrigger, "Combat AI agent contract should expose death animation requests.");
            Assert.AreEqual(CombatAiPatternState.Tracking, agent.CurrentPatternState, "Combat AI agent contract should expose readable pattern state for camera/UI consumers.");
            Assert.AreEqual(1, targetSensor.TargetCandidateCount, "Shared target sensor should use authored candidates instead of scene-wide target searches.");
            Assert.IsTrue(targetSensor.TryGetCurrentTarget(out Transform sensedTarget, out CombatHealth sensedHealth), "Shared target sensor should find a hostile target in the action foundation scene.");
            Assert.AreSame(playerHealth, sensedHealth, "Enemy target sensing should resolve the current player as the hostile target.");
            Assert.AreSame(playerHealth.transform, sensedTarget, "Shared target sensor should expose both target Transform and CombatHealth.");
        }

        [UnityTest]
        public IEnumerator ActionFoundationSceneProvidesPatternSampleEnemiesAndPlayerTargetCandidates()
        {
            BasicSoldierEnemy closePunish = RequireNamedRootComponent<BasicSoldierEnemy>(ClosePunishEnemyRootName);
            BasicSoldierEnemy lungeStrike = RequireNamedRootComponent<BasicSoldierEnemy>(LungeStrikeEnemyRootName);
            BasicSoldierEnemy heavyWindup = RequireNamedRootComponent<BasicSoldierEnemy>(HeavyWindupEnemyRootName);
            BasicSoldierEnemy linePressure = RequireNamedRootComponent<BasicSoldierEnemy>(LinePressureEnemyRootName);
            BasicSoldierEnemy fanPressure = RequireNamedRootComponent<BasicSoldierEnemy>(FanPressureEnemyRootName);
            BasicSoldierEnemy trainingDeck = RequireNamedRootComponent<BasicSoldierEnemy>(TrainingDeckEnemyRootName);
            BasicSoldierEnemy retreatShot = RequireNamedRootComponent<BasicSoldierEnemy>(RetreatShotEnemyRootName);
            BasicSoldierEnemy retreatBlink = RequireNamedRootComponent<BasicSoldierEnemy>(RetreatBlinkEnemyRootName);
            BasicSoldierEnemy guardBreak = RequireNamedRootComponent<BasicSoldierEnemy>(GuardBreakEnemyRootName);
            BasicSoldierEnemy generalDeck = RequireNamedRootComponent<BasicSoldierEnemy>(GeneralDeckEnemyRootName);
            BasicSoldierEnemy eliteDeck = RequireNamedRootComponent<BasicSoldierEnemy>(EliteDeckEnemyRootName);
            BasicSoldierEnemy eliteTraits = RequireNamedRootComponent<BasicSoldierEnemy>(EliteTraitsEnemyRootName);
            PlayerCombatTargetSelector targetSelector = RequireObject<PlayerCombatTargetSelector>();
            CombatHealth playerHealth = RequirePlayerHealth();
            CombatAiPatternDeck trainingDeckAsset = LoadPatternDeck(BasicSoldierTrainingDeckPath);
            CombatAiPatternDeck generalDeckAsset = LoadPatternDeck(GeneralPatternDeckPath);
            CombatAiPatternDeck eliteDeckAsset = LoadPatternDeck(ElitePatternDeckPath);

            yield return null;

            Assert.AreEqual("ClosePunish", closePunish.PatternId, "Primary soldier should remain the close punish sample.");
            Assert.AreEqual("LungeStrike", lungeStrike.PatternId, "Second soldier should be the lunge pattern sample.");
            Assert.AreEqual("HeavyWindup", heavyWindup.PatternId, "Third soldier should be the heavy windup pattern sample.");
            Assert.AreEqual("LinePressure", linePressure.PatternId, "Fourth soldier should be the line pressure pattern sample.");
            Assert.AreEqual("FanPressure", fanPressure.PatternId, "Fifth soldier should be the fan pressure pattern sample.");
            Assert.AreEqual("ClosePunish", trainingDeck.PatternId, "Training deck sample should start from the close punish profile before distance selection runs.");
            Assert.AreSame(trainingDeckAsset, trainingDeck.PatternDeck, "Training deck sample should be authored with the reusable pattern deck asset.");
            Assert.IsTrue(trainingDeck.HasPatternDeck, "Training deck sample should expose that deck selection is active.");
            Assert.AreEqual("RetreatShot", retreatShot.PatternId, "Extended scene should include a retreat shot sample.");
            Assert.AreEqual("RetreatBlink", retreatBlink.PatternId, "Extended scene should include a retreat blink sample.");
            Assert.AreEqual("GuardBreak", guardBreak.PatternId, "Extended scene should include an elite guard-break attack sample.");
            Assert.AreEqual("ClosePunish", generalDeck.PatternId, "General deck sample should start from the close punish row before distance selection runs.");
            Assert.AreSame(generalDeckAsset, generalDeck.PatternDeck, "General deck sample should use the expanded general deck asset.");
            Assert.IsTrue(generalDeck.HasPatternDeck, "General deck sample should expose that deck selection is active.");
            Assert.AreEqual("GuardBreak", eliteDeck.PatternId, "Elite deck sample should start from the guard-break row.");
            Assert.AreSame(eliteDeckAsset, eliteDeck.PatternDeck, "Elite deck sample should use the elite pattern deck asset.");
            Assert.IsTrue(eliteDeck.HasPatternDeck, "Elite deck sample should expose that deck selection is active.");
            Assert.AreEqual("GuardBreak", eliteTraits.PatternId, "Elite trait sample should use an elite attack profile as its starting row.");
            Assert.AreSame(eliteDeckAsset, eliteTraits.PatternDeck, "Elite trait sample should share the elite deck before phase swap.");
            EnemyElitePatternController eliteController = eliteTraits.GetComponent<EnemyElitePatternController>();
            Assert.IsNotNull(eliteController, "Elite trait sample should own the elite pattern controller.");
            Assert.AreEqual(5, eliteController.ProfileCount, "Elite trait sample should wire ShieldCycle, ArmorBreak, AuraBuffer, SummonPackage, and PhaseSwap profiles.");
            Assert.AreEqual(12, targetSelector.TargetCandidateCount, "Player target selector should receive all authored sample enemies instead of scene-scanning.");
            AssertEnemyTargetsPlayer(closePunish, playerHealth);
            AssertEnemyTargetsPlayer(lungeStrike, playerHealth);
            AssertEnemyTargetsPlayer(heavyWindup, playerHealth);
            AssertEnemyTargetsPlayer(linePressure, playerHealth);
            AssertEnemyTargetsPlayer(fanPressure, playerHealth);
            AssertEnemyTargetsPlayer(trainingDeck, playerHealth);
            AssertEnemyTargetsPlayer(retreatShot, playerHealth);
            AssertEnemyTargetsPlayer(retreatBlink, playerHealth);
            AssertEnemyTargetsPlayer(guardBreak, playerHealth);
            AssertEnemyTargetsPlayer(generalDeck, playerHealth);
            AssertEnemyTargetsPlayer(eliteDeck, playerHealth);
            AssertEnemyTargetsPlayer(eliteTraits, playerHealth);
            Assert.IsNotNull(closePunish.GetComponent<EnemyActionCameraCueDriver>(), "Close sample should own an enemy camera cue driver.");
            Assert.IsNotNull(lungeStrike.GetComponent<EnemyActionCameraCueDriver>(), "Lunge sample should own an enemy camera cue driver.");
            Assert.IsNotNull(heavyWindup.GetComponent<EnemyActionCameraCueDriver>(), "Heavy sample should own an enemy camera cue driver.");
            Assert.IsNotNull(linePressure.GetComponent<EnemyActionCameraCueDriver>(), "Line pressure sample should own an enemy camera cue driver.");
            Assert.IsNotNull(fanPressure.GetComponent<EnemyActionCameraCueDriver>(), "Fan pressure sample should own an enemy camera cue driver.");
            Assert.IsNotNull(trainingDeck.GetComponent<EnemyActionCameraCueDriver>(), "Training deck sample should own an enemy camera cue driver.");
            Assert.IsNotNull(retreatShot.GetComponent<EnemyActionCameraCueDriver>(), "Retreat shot sample should own an enemy camera cue driver.");
            Assert.IsNotNull(retreatBlink.GetComponent<EnemyActionCameraCueDriver>(), "Retreat blink sample should own an enemy camera cue driver.");
            Assert.IsNotNull(guardBreak.GetComponent<EnemyActionCameraCueDriver>(), "Guard-break sample should own an enemy camera cue driver.");
            Assert.IsNotNull(generalDeck.GetComponent<EnemyActionCameraCueDriver>(), "General deck sample should own an enemy camera cue driver.");
            Assert.IsNotNull(eliteDeck.GetComponent<EnemyActionCameraCueDriver>(), "Elite deck sample should own an enemy camera cue driver.");
            Assert.IsNotNull(eliteTraits.GetComponent<EnemyActionCameraCueDriver>(), "Elite trait sample should own an enemy camera cue driver.");
        }

        [UnityTest]
        public IEnumerator PlayerTargetSelectorPrefersReadableForwardThreatAndFeedsCameraBridge()
        {
            PlayerMovementController movement = RequireObject<PlayerMovementController>();
            PlayerCombatTargetSelector targetSelector = RequireObject<PlayerCombatTargetSelector>();
            ActionCameraController cameraController = RequireObject<ActionCameraController>();
            ActionCameraTargetBridge cameraTargetBridge = RequireObject<ActionCameraTargetBridge>();
            BasicSoldierEnemy closePunish = RequireNamedRootComponent<BasicSoldierEnemy>(ClosePunishEnemyRootName);
            BasicSoldierEnemy lungeStrike = RequireNamedRootComponent<BasicSoldierEnemy>(LungeStrikeEnemyRootName);
            BasicSoldierEnemy heavyWindup = RequireNamedRootComponent<BasicSoldierEnemy>(HeavyWindupEnemyRootName);
            BasicSoldierEnemy linePressure = RequireNamedRootComponent<BasicSoldierEnemy>(LinePressureEnemyRootName);
            BasicSoldierEnemy fanPressure = RequireNamedRootComponent<BasicSoldierEnemy>(FanPressureEnemyRootName);
            BasicSoldierEnemy trainingDeck = RequireNamedRootComponent<BasicSoldierEnemy>(TrainingDeckEnemyRootName);
            BasicSoldierEnemy retreatShot = RequireNamedRootComponent<BasicSoldierEnemy>(RetreatShotEnemyRootName);
            BasicSoldierEnemy retreatBlink = RequireNamedRootComponent<BasicSoldierEnemy>(RetreatBlinkEnemyRootName);
            BasicSoldierEnemy guardBreak = RequireNamedRootComponent<BasicSoldierEnemy>(GuardBreakEnemyRootName);
            BasicSoldierEnemy generalDeck = RequireNamedRootComponent<BasicSoldierEnemy>(GeneralDeckEnemyRootName);
            BasicSoldierEnemy eliteDeck = RequireNamedRootComponent<BasicSoldierEnemy>(EliteDeckEnemyRootName);
            BasicSoldierEnemy eliteTraits = RequireNamedRootComponent<BasicSoldierEnemy>(EliteTraitsEnemyRootName);

            closePunish.enabled = false;
            lungeStrike.enabled = false;
            heavyWindup.enabled = false;
            linePressure.enabled = false;
            fanPressure.enabled = false;
            trainingDeck.enabled = false;
            retreatShot.enabled = false;
            retreatBlink.enabled = false;
            guardBreak.enabled = false;
            generalDeck.enabled = false;
            eliteDeck.enabled = false;
            eliteTraits.enabled = false;
            movement.transform.position = Vector3.zero;
            movement.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
            cameraController.transform.position = new Vector3(0f, 1.8f, -4f);
            cameraController.transform.rotation = Quaternion.LookRotation(new Vector3(0f, -0.25f, 1f), Vector3.up);
            closePunish.transform.position = Vector3.forward * 5f;
            lungeStrike.transform.position = Vector3.back * 1.2f;
            heavyWindup.transform.position = Vector3.right * 6f;
            linePressure.transform.position = Vector3.left * 7f;
            fanPressure.transform.position = Vector3.left * 9f;
            trainingDeck.transform.position = Vector3.left * 11f;
            retreatShot.transform.position = Vector3.back * 8f;
            retreatBlink.transform.position = Vector3.back * 9f;
            guardBreak.transform.position = Vector3.back * 10f;
            generalDeck.transform.position = Vector3.back * 11f;
            eliteDeck.transform.position = Vector3.back * 12f;
            eliteTraits.transform.position = Vector3.back * 13f;
            Physics.SyncTransforms();

            targetSelector.RefreshTarget();
            yield return null;

            Assert.AreSame(closePunish.SelfHealth, targetSelector.CurrentTargetHealth, "Selector should prefer the readable forward threat over a closer target behind the player.");
            Assert.AreSame(cameraController, cameraTargetBridge.CameraController, "Camera bridge should be serialized to the action camera.");
            Assert.AreSame(targetSelector, cameraTargetBridge.TargetSelector, "Camera bridge should read the player selector rather than doing its own target search.");
            Assert.AreSame(movement.transform, cameraController.Target, "Camera should continue following the player root.");
            Assert.AreSame(closePunish.transform, cameraController.Threat, "Camera bridge should feed the selected readable threat into the camera bias target.");
        }

        [UnityTest]
        public IEnumerator BasicAttackSoftAimsTowardAuthoredTargetInsidePocket()
        {
            PlayerMovementController movement = RequireObject<PlayerMovementController>();
            PlayerActionController actions = RequireObject<PlayerActionController>();
            PlayerCombatTargetSelector targetSelector = RequireObject<PlayerCombatTargetSelector>();
            BasicSoldierEnemy closePunish = RequireNamedRootComponent<BasicSoldierEnemy>(ClosePunishEnemyRootName);
            BasicSoldierEnemy lungeStrike = RequireNamedRootComponent<BasicSoldierEnemy>(LungeStrikeEnemyRootName);
            BasicSoldierEnemy heavyWindup = RequireNamedRootComponent<BasicSoldierEnemy>(HeavyWindupEnemyRootName);
            BasicSoldierEnemy linePressure = RequireNamedRootComponent<BasicSoldierEnemy>(LinePressureEnemyRootName);
            BasicSoldierEnemy fanPressure = RequireNamedRootComponent<BasicSoldierEnemy>(FanPressureEnemyRootName);
            BasicSoldierEnemy trainingDeck = RequireNamedRootComponent<BasicSoldierEnemy>(TrainingDeckEnemyRootName);

            closePunish.enabled = false;
            lungeStrike.enabled = false;
            heavyWindup.enabled = false;
            linePressure.enabled = false;
            fanPressure.enabled = false;
            trainingDeck.enabled = false;
            movement.SetMoveInput(Vector2.zero);
            movement.transform.position = Vector3.zero;
            movement.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
            closePunish.transform.position = Vector3.right * 3f;
            lungeStrike.transform.position = Vector3.forward * 18f;
            heavyWindup.transform.position = Vector3.back * 18f;
            linePressure.transform.position = Vector3.left * 18f;
            fanPressure.transform.position = Vector3.left * 20f;
            trainingDeck.transform.position = Vector3.left * 22f;
            Physics.SyncTransforms();
            targetSelector.RefreshTarget();
            yield return null;

            float enemyStartHealth = closePunish.SelfHealth.CurrentHealth;
            actions.QueueBasicAttack();
            yield return null;

            Assert.Greater(
                Vector3.Dot(actions.LastAttackDirection.normalized, Vector3.right),
                0.95f,
                "Basic attack should choose the authored soft-lock target direction even when it is outside melee reach.");
            Assert.Greater(
                Vector3.Dot(movement.FacingDirection.normalized, Vector3.right),
                0.95f,
                "Attack-facing requests should turn the player toward the soft target without camera-owned lock-on.");

            yield return new WaitForSeconds(0.45f);

            Assert.AreEqual(
                enemyStartHealth,
                closePunish.SelfHealth.CurrentHealth,
                0.001f,
                "Soft attack aim should not grant free damage when the target is outside the melee hit sphere.");

            yield return new WaitForSeconds(0.3f);

            movement.transform.position = Vector3.zero;
            movement.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
            closePunish.transform.position = Vector3.right * 1.25f;
            Physics.SyncTransforms();
            targetSelector.RefreshTarget();
            yield return null;

            actions.QueueBasicAttack();
            yield return new WaitForSeconds(0.4f);

            Assert.Less(
                closePunish.SelfHealth.CurrentHealth,
                enemyStartHealth,
                "The same soft attack direction should hit once the authored target is inside the melee pocket.");
        }

        [Test]
        public void BasicSoldierPatternProfilesUseSharedAiPatternContract()
        {
            CombatAiPatternProfile closePunish = LoadPatternProfile(ClosePunishPatternPath);
            CombatAiPatternProfile lungeStrike = LoadPatternProfile(LungeStrikePatternPath);
            CombatAiPatternProfile heavyWindup = LoadPatternProfile(HeavyWindupPatternPath);
            CombatAiPatternProfile linePressure = LoadPatternProfile(LinePressurePatternPath);
            CombatAiPatternProfile fanPressure = LoadPatternProfile(FanPressurePatternPath);
            CombatAiPatternDeck trainingDeck = LoadPatternDeck(BasicSoldierTrainingDeckPath);

            Assert.AreEqual("SciFiSoldier.Basic", closePunish.ActorTypeId, "ClosePunish should declare the actor type used by visual/Animator setup.");
            Assert.AreEqual("ClosePunish", closePunish.PatternId, "ClosePunish profile should keep the first readable melee pattern id.");
            Assert.AreEqual(CombatAiAttackShape.MeleeArc, closePunish.AttackShape, "ClosePunish should keep a compact melee attack shape.");
            Assert.AreEqual(0f, closePunish.ActiveLungeSpeed, 0.001f, "ClosePunish should stay a stationary melee release.");
            Assert.AreEqual(CombatAiCameraCueKind.ClosePunish, closePunish.CameraCueKind, "ClosePunish should expose camera cue semantics as profile data.");
            Assert.AreEqual("SciFiSoldier.Basic", lungeStrike.ActorTypeId, "LungeStrike should use the same actor type so model/Animator setup remains swappable.");
            Assert.AreEqual("LungeStrike", lungeStrike.PatternId, "Second soldier pattern should be a distinct profile instead of a hardcoded mode flag.");
            Assert.AreEqual(CombatAiAttackShape.ForwardLine, lungeStrike.AttackShape, "LungeStrike should use the same forward-line hit-shape contract that future line pressure patterns reuse.");
            Assert.IsTrue(lungeStrike.LockAttackDirectionOnWindup, "LungeStrike should commit its lunge lane once the warning starts.");
            Assert.AreEqual(CombatAiCameraCueKind.LungeStrike, lungeStrike.CameraCueKind, "LungeStrike should expose lunge camera cue semantics through the shared profile.");
            Assert.Greater(lungeStrike.AttackRange, closePunish.AttackRange, "LungeStrike should advertise longer reach through data.");
            Assert.Greater(lungeStrike.ActiveLungeSpeed, closePunish.ActiveLungeSpeed, "LungeStrike should add forward active movement through data.");
            Assert.Greater(lungeStrike.Damage, closePunish.Damage, "LungeStrike should be distinguishable as a heavier pattern through profile data.");
            Assert.AreEqual("SciFiSoldier.Basic", heavyWindup.ActorTypeId, "HeavyWindup should share the same actor type so the same visual can test pattern variety.");
            Assert.AreEqual("HeavyWindup", heavyWindup.PatternId, "HeavyWindup should be a third profile, not a hardcoded branch inside the soldier.");
            Assert.AreEqual(CombatAiAttackShape.MeleeArc, heavyWindup.AttackShape, "HeavyWindup should stay a wider melee warning rather than pretending to be a ranged line pattern.");
            Assert.AreEqual(CombatAiCameraCueKind.HeavyWindup, heavyWindup.CameraCueKind, "HeavyWindup should select its camera read cue through profile data.");
            Assert.Greater(heavyWindup.TelegraphSeconds, lungeStrike.TelegraphSeconds, "HeavyWindup should advertise a longer warning window through data.");
            Assert.Greater(heavyWindup.WindupThreatLevel, lungeStrike.WindupThreatLevel, "HeavyWindup should mark stronger camera/readability emphasis through data.");
            Assert.Greater(closePunish.RecoveryRetreatSpeed, 0f, "ClosePunish should include a short reference-backed backstep after the melee burst.");
            Assert.Greater(lungeStrike.TelegraphActiveScale.z, closePunish.TelegraphActiveScale.z, "LungeStrike should expose a longer forward telegraph than ClosePunish.");
            Assert.Less(lungeStrike.TelegraphActiveScale.x, closePunish.TelegraphActiveScale.x, "LungeStrike should expose a narrow line-shaped telegraph instead of the same melee footprint.");
            Assert.Greater(heavyWindup.TelegraphActiveScale.x, closePunish.TelegraphActiveScale.x, "HeavyWindup should expose a wider heavy-warning telegraph than ClosePunish.");
            Assert.Greater(lungeStrike.ActiveCameraCue.fieldOfViewDelta, closePunish.ActiveCameraCue.fieldOfViewDelta, "Pattern camera cue intensity should now be profile data instead of a driver enum switch.");
            Assert.Greater(heavyWindup.WindupCameraCue.durationSeconds, closePunish.WindupCameraCue.durationSeconds, "HeavyWindup should own its longer warning camera cue timing through profile data.");
            Assert.IsTrue(closePunish.DeathCameraCue.enabled, "Shared death readability should still be profile-owned so future monster types can override it without driver code.");
            Assert.AreNotEqual(closePunish.WindupEndColor, lungeStrike.WindupEndColor, "Pattern samples should not share identical warning colors when they are meant to be visually distinguishable.");
            Assert.AreEqual("SciFiSoldier.Basic", linePressure.ActorTypeId, "LinePressure should reuse the basic soldier actor until a dedicated ranged soldier visual is promoted.");
            Assert.AreEqual("LinePressure", linePressure.PatternId, "LinePressure should be a profile asset, not an enum branch inside the soldier.");
            Assert.AreEqual(CombatAiAttackShape.ForwardLine, linePressure.AttackShape, "LinePressure should advertise a forward-line attack shape through profile data.");
            Assert.IsTrue(linePressure.LockAttackDirectionOnWindup, "LinePressure should lock its warning lane instead of tracking the player until the hit frame.");
            Assert.AreEqual(CombatAiCameraCueKind.LinePressure, linePressure.CameraCueKind, "LinePressure should expose its camera read cue through profile data.");
            Assert.AreEqual("AttackLinePressure", linePressure.AttackTrigger, "LinePressure should request an authored line/aim animation instead of falling back to the close melee swing.");
            Assert.Greater(linePressure.AttackRange, lungeStrike.AttackRange, "LinePressure should reach farther than the lunge sample.");
            Assert.Less(linePressure.AttackHalfWidth, closePunish.AttackHalfWidth, "LinePressure should be dodgeable sideways through a narrow hit width.");
            Assert.Greater(linePressure.TelegraphActiveScale.z, lungeStrike.TelegraphActiveScale.z, "LinePressure should expose a longer forward warning strip than the lunge sample.");
            Assert.AreEqual("SciFiSoldier.Basic", fanPressure.ActorTypeId, "FanPressure should reuse the basic soldier actor until a dedicated ranged soldier visual is promoted.");
            Assert.AreEqual("FanPressure", fanPressure.PatternId, "FanPressure should be a profile asset, not a branch inside the soldier.");
            Assert.AreEqual(CombatAiAttackShape.ForwardFan, fanPressure.AttackShape, "FanPressure should advertise a forward-fan attack shape through profile data.");
            Assert.IsTrue(fanPressure.LockAttackDirectionOnWindup, "FanPressure should commit its cone once the warning starts.");
            Assert.AreEqual(CombatAiCameraCueKind.FanPressure, fanPressure.CameraCueKind, "FanPressure should expose its camera read cue through profile data.");
            Assert.AreEqual("AttackFanPressure", fanPressure.AttackTrigger, "FanPressure should request an authored wide-pressure animation through profile data.");
            Assert.Greater(fanPressure.AttackHalfAngleDegrees, linePressure.AttackHalfAngleDegrees, "FanPressure should be wider than the line pattern in angle space.");
            Assert.Greater(fanPressure.TelegraphActiveScale.x, linePressure.TelegraphActiveScale.x, "FanPressure should expose a wider warning footprint than LinePressure.");
            Assert.AreEqual(4, trainingDeck.EntryCount, "Training deck should show how one soldier can own several reusable pattern profiles.");
            Assert.AreSame(closePunish, trainingDeck.GetEntry(0).Profile, "Training deck entry 0 should be the close-range punish pattern.");
            Assert.AreSame(lungeStrike, trainingDeck.GetEntry(1).Profile, "Training deck entry 1 should be the mid-range lunge pattern.");
            Assert.AreSame(fanPressure, trainingDeck.GetEntry(2).Profile, "Training deck entry 2 should be the fan pressure pattern.");
            Assert.AreSame(linePressure, trainingDeck.GetEntry(3).Profile, "Training deck entry 3 should be the long line pressure pattern.");
            Assert.AreEqual("Attack", closePunish.AttackTrigger, "ClosePunish should keep the compact melee attack animation as the baseline soldier read.");
            Assert.AreEqual("AttackCombo2", lungeStrike.AttackTrigger, "LungeStrike should now use a promoted two-hit combat clip instead of sharing ClosePunish animation.");
            Assert.AreEqual("AttackHeavy", heavyWindup.AttackTrigger, "HeavyWindup should request a heavier promoted combo clip through data.");
            Assert.AreEqual("HitHeavy", heavyWindup.HitTrigger, "HeavyWindup should use the heavier reaction tag when interrupted.");
        }

        [Test]
        public void ExtendedEnemyPatternProfilesUseReferenceBackedData()
        {
            CombatAiPatternProfile closePunish = LoadPatternProfile(ClosePunishPatternPath);
            CombatAiPatternProfile lungeStrike = LoadPatternProfile(LungeStrikePatternPath);
            CombatAiPatternProfile heavyWindup = LoadPatternProfile(HeavyWindupPatternPath);
            CombatAiPatternProfile linePressure = LoadPatternProfile(LinePressurePatternPath);
            CombatAiPatternProfile fanPressure = LoadPatternProfile(FanPressurePatternPath);
            CombatAiPatternProfile retreatShot = LoadPatternProfile(RetreatShotPatternPath);
            CombatAiPatternProfile retreatBlink = LoadPatternProfile(RetreatBlinkPatternPath);
            CombatAiPatternProfile guardBreak = LoadPatternProfile(GuardBreakPatternPath);
            CombatAiPatternDeck generalDeck = LoadPatternDeck(GeneralPatternDeckPath);
            CombatAiPatternDeck eliteDeck = LoadPatternDeck(ElitePatternDeckPath);
            CombatAiPatternDeck phaseTwoDeck = LoadPatternDeck(ElitePhaseTwoPatternDeckPath);
            CombatAiElitePatternProfile shieldCycle = LoadElitePatternProfile(ShieldCycleEliteProfilePath);
            CombatAiElitePatternProfile armorBreak = LoadElitePatternProfile(ArmorBreakEliteProfilePath);
            CombatAiElitePatternProfile auraBuffer = LoadElitePatternProfile(AuraBufferEliteProfilePath);
            CombatAiElitePatternProfile summonPackage = LoadElitePatternProfile(SummonPackageEliteProfilePath);
            CombatAiElitePatternProfile phaseSwap = LoadElitePatternProfile(PhaseSwapEliteProfilePath);

            Assert.AreEqual("RetreatShot", retreatShot.PatternId, "RetreatShot should be authored as a profile asset, not a soldier branch.");
            Assert.AreEqual(CombatAiCameraCueKind.RetreatShot, retreatShot.CameraCueKind, "RetreatShot should expose its camera/readability cue through profile data.");
            Assert.Greater(retreatShot.PrepareSeconds, 0f, "RetreatShot should own a pre-attack backstep window from data.");
            Assert.Greater(retreatShot.PrepareRetreatSpeed, 0f, "RetreatShot should move away before aim/shot instead of using an instant teleport.");
            Assert.AreEqual("RetreatBackstep", retreatShot.PrepareTrigger, "RetreatShot should request a readable retreat setup animation before the shot.");
            Assert.AreEqual("AttackRetreatShot", retreatShot.AttackTrigger, "RetreatShot should request a distinct aim/shot animation trigger through profile data.");
            Assert.AreEqual(CombatAiAttackShape.ForwardLine, retreatShot.AttackShape, "RetreatShot should reuse the shared forward-line hit shape.");
            Assert.IsTrue(retreatShot.LockAttackDirectionAfterPrepare, "RetreatShot should commit its shot direction after the retreat/aim setup.");

            Assert.AreEqual("RetreatBlink", retreatBlink.PatternId, "RetreatBlink should be a reusable reposition profile.");
            Assert.Greater(retreatBlink.PrepareRetreatSpeed, retreatShot.PrepareRetreatSpeed, "RetreatBlink should read as a faster evasive reposition than RetreatShot.");
            Assert.AreEqual(CombatAiCameraCueKind.RetreatBlink, retreatBlink.CameraCueKind, "RetreatBlink should keep its cue semantics in profile data.");
            Assert.AreEqual("RetreatBlink", retreatBlink.PrepareTrigger, "RetreatBlink should request a faster crouch-forward reposition trigger during prepare.");
            Assert.AreEqual("AttackRetreatBlink", retreatBlink.AttackTrigger, "RetreatBlink should separate its follow-up attack read from RetreatShot.");

            Assert.AreEqual("GuardBreak", guardBreak.PatternId, "GuardBreak should be a profile consumed by elite decks, not a hardcoded state.");
            Assert.AreEqual("SciFiSoldier.Elite", guardBreak.ActorTypeId, "GuardBreak should declare the elite actor type for future model/Animator swaps.");
            Assert.Greater(guardBreak.TelegraphSeconds, heavyWindup.TelegraphSeconds, "GuardBreak should be a slower readable elite punish than HeavyWindup.");
            Assert.AreEqual("AttackGuardBreak", guardBreak.AttackTrigger, "GuardBreak should request a distinct elite-heavy attack animation through profile data.");
            Assert.AreEqual("HitHeavy", guardBreak.HitTrigger, "GuardBreak should share the heavier reaction tag so elite interruptions read clearly.");

            Assert.AreEqual(6, generalDeck.EntryCount, "General deck should expand the basic soldier vocabulary without replacing the training deck.");
            Assert.AreSame(closePunish, generalDeck.GetEntry(0).Profile, "General deck should keep close punish as the close-range anchor.");
            Assert.AreSame(lungeStrike, generalDeck.GetEntry(1).Profile, "General deck should keep lunge as the mid-range commitment.");
            Assert.AreSame(retreatShot, generalDeck.GetEntry(2).Profile, "General deck should add retreat shot as the first spacing-reset row.");
            Assert.AreSame(fanPressure, generalDeck.GetEntry(3).Profile, "General deck should preserve fan pressure as a mid-long spacing check.");
            Assert.AreSame(linePressure, generalDeck.GetEntry(4).Profile, "General deck should preserve line pressure as long straight pressure.");
            Assert.AreSame(retreatBlink, generalDeck.GetEntry(5).Profile, "General deck should keep fast retreat as a lower-priority escape row.");

            Assert.AreEqual(6, eliteDeck.EntryCount, "Elite deck should combine guard, heavy, pressure, and retreat rows without boss-only logic.");
            Assert.AreSame(guardBreak, eliteDeck.GetEntry(1).Profile, "Elite deck should promote GuardBreak as a guarded punish row.");
            Assert.AreSame(heavyWindup, eliteDeck.GetEntry(2).Profile, "Elite deck should reuse HeavyWindup instead of duplicating a second heavy attack script.");
            Assert.AreEqual(5, phaseTwoDeck.EntryCount, "Phase two deck should be a data swap target for PhaseSwap.");

            Assert.AreEqual(CombatAiElitePatternKind.ShieldCycle, shieldCycle.PatternKind, "ShieldCycle should be an elite trait profile.");
            Assert.AreEqual("EliteShieldCycle", shieldCycle.SignalAnimationTrigger, "ShieldCycle should own an authored signal animation trigger.");
            Assert.Greater(shieldCycle.GuardMeter, 0f, "ShieldCycle should own a breakable guard meter.");
            Assert.Less(shieldCycle.DamageTakenMultiplier, 1f, "ShieldCycle should mitigate damage while the shield is up.");
            Assert.AreEqual(CombatAiElitePatternKind.ArmorBreak, armorBreak.PatternKind, "ArmorBreak should be an elite trait profile.");
            Assert.AreEqual("EliteArmorBreak", armorBreak.SignalAnimationTrigger, "ArmorBreak should own a heavier break/stagger animation trigger.");
            Assert.Greater(armorBreak.GuardMeter, shieldCycle.GuardMeter, "ArmorBreak should require more commitment than the cycling shield.");
            Assert.AreEqual(0f, armorBreak.RefreshSeconds, 0.001f, "ArmorBreak should stay broken after the first armor break in this foundation slice.");
            Assert.AreEqual(CombatAiElitePatternKind.AuraBuffer, auraBuffer.PatternKind, "AuraBuffer should be represented as a reusable elite signal.");
            Assert.AreEqual("EliteAuraBuffer", auraBuffer.SignalAnimationTrigger, "AuraBuffer should use a maintenance/action clip as a readable buff signal.");
            Assert.Greater(auraBuffer.SignalSeconds, 100f, "AuraBuffer should stay readable as a persistent priority marker.");
            Assert.Less(auraBuffer.DamageTakenMultiplier, 1f, "AuraBuffer should protect authored receiver targets through damage-modification data.");
            Assert.AreEqual(CombatAiElitePatternKind.SummonPackage, summonPackage.PatternKind, "SummonPackage should exist as a signal profile before runtime spawning is introduced.");
            Assert.AreEqual("EliteSummonPackage", summonPackage.SignalAnimationTrigger, "SummonPackage should use a distinct command/call animation trigger.");
            Assert.Less(summonPackage.TriggerHealthRatio, 1f, "SummonPackage should not fire immediately at full health.");
            Assert.AreEqual(CombatAiElitePatternKind.PhaseSwap, phaseSwap.PatternKind, "PhaseSwap should be a data profile for deck/profile swapping.");
            Assert.AreEqual("ElitePhaseSwap", phaseSwap.SignalAnimationTrigger, "PhaseSwap should request a clear turn/phase signal animation through data.");
            Assert.AreSame(phaseTwoDeck, phaseSwap.ReplacementPatternDeck, "PhaseSwap should point at the phase-two deck through data.");
        }

        [Test]
        public void EnemyRoleProfilesCoverLinearRunSegments()
        {
            var coveredSegments = new HashSet<CombatEnemyRunSegment>();
            int generalCount = 0;
            int eliteCount = 0;

            foreach (string path in EnemyRoleProfilePaths)
            {
                CombatEnemyRoleProfile role = AssetDatabase.LoadAssetAtPath<CombatEnemyRoleProfile>(path);
                Assert.IsNotNull(role, $"Missing enemy role profile at {path}.");
                Assert.IsFalse(string.IsNullOrWhiteSpace(role.RoleId), $"{path} should have a role id.");
                Assert.IsNotNull(role.StartingPattern, $"{role.RoleId} should define a starting pattern.");
                Assert.IsNotNull(role.PatternDeck, $"{role.RoleId} should define a role pattern deck.");
                Assert.Greater(role.PatternDeck.EntryCount, 0, $"{role.RoleId} should have at least one deck row.");
                Assert.GreaterOrEqual(role.SuggestedMaxCount, role.SuggestedMinCount, $"{role.RoleId} should have a valid count range.");
                coveredSegments.Add(role.PreferredSegment);

                for (int i = 0; i < role.PatternDeck.EntryCount; i++)
                {
                    CombatAiPatternDeckEntry entry = role.PatternDeck.GetEntry(i);
                    Assert.IsNotNull(entry.Profile, $"{role.RoleId} deck row {i} should have a pattern profile.");
                    if (entry.MaximumDistance > 0f)
                    {
                        Assert.GreaterOrEqual(entry.MaximumDistance, entry.MinimumDistance, $"{role.RoleId} deck row {i} should have a valid distance band.");
                    }
                }

                if (role.EliteRole)
                {
                    eliteCount++;
                    Assert.Greater(role.EliteProfileCount, 0, $"{role.RoleId} should declare elite trait profiles.");
                }
                else
                {
                    generalCount++;
                    Assert.AreEqual(0, role.EliteProfileCount, $"{role.RoleId} should stay a general role without elite trait profiles.");
                }
            }

            foreach (CombatEnemyRunSegment segment in System.Enum.GetValues(typeof(CombatEnemyRunSegment)))
            {
                Assert.IsTrue(coveredSegments.Contains(segment), $"Enemy role catalog should cover {segment}.");
            }

            Assert.GreaterOrEqual(generalCount, 7, "Role catalog should expose at least seven general monster roles.");
            Assert.GreaterOrEqual(eliteCount, 5, "Role catalog should expose at least five elite monster roles.");
        }

        [Test]
        public void EnemyArchetypeProfilesMapRolesWithoutRawImportedPrefabRefs()
        {
            var coveredRoleIds = new HashSet<string>();
            bool foundStaticTurretCandidate = false;
            bool foundBossCandidate = false;

            foreach (string path in EnemyArchetypeProfilePaths)
            {
                CombatEnemyArchetypeProfile archetype = AssetDatabase.LoadAssetAtPath<CombatEnemyArchetypeProfile>(path);
                Assert.IsNotNull(archetype, $"Missing enemy archetype profile at {path}.");
                Assert.IsFalse(string.IsNullOrWhiteSpace(archetype.ArchetypeId), $"{path} should have an archetype id.");
                Assert.IsFalse(string.IsNullOrWhiteSpace(archetype.PromotionPlan), $"{archetype.ArchetypeId} should explain its promotion plan.");
                foundStaticTurretCandidate |= archetype.ArchetypeKind == CombatEnemyArchetypeKind.StaticTurret;
                foundBossCandidate |= archetype.ArchetypeKind == CombatEnemyArchetypeKind.BossCandidate;

                if (archetype.ParticipatesInActionFoundationRoleMap)
                {
                    Assert.Greater(archetype.CompatibleRoleCount, 0, $"{archetype.ArchetypeId} should map to at least one role.");
                }

                for (int i = 0; i < archetype.CompatibleRoleCount; i++)
                {
                    CombatEnemyRoleProfile role = archetype.GetCompatibleRole(i);
                    Assert.IsNotNull(role, $"{archetype.ArchetypeId} should not contain empty role refs.");
                    coveredRoleIds.Add(role.RoleId);
                }

                AssertGameOwnedObjectReference(archetype.GameplayPrefab, $"{archetype.ArchetypeId} gameplay prefab");
                AssertGameOwnedObjectReference(archetype.VisualPrefab, $"{archetype.ArchetypeId} visual prefab");

                if (archetype.ArchetypeKind == CombatEnemyArchetypeKind.StaticTurret)
                {
                    Assert.IsTrue(archetype.RequiresDedicatedPrefabPromotion, $"{archetype.ArchetypeId} should stay marked for promotion until a `_Game` turret prefab exists.");
                    Assert.IsTrue(archetype.SourceCandidate.Contains("FORGE3D"), $"{archetype.ArchetypeId} should record the FORGE3D source candidate text without referencing raw objects.");
                }
            }

            Assert.IsTrue(coveredRoleIds.Contains("SciFiSoldier.CloseGuard"), "Archetype catalog should cover CloseGuard.");
            Assert.IsTrue(coveredRoleIds.Contains("SciFiSoldier.LungeChaser"), "Archetype catalog should cover LungeChaser.");
            Assert.IsTrue(coveredRoleIds.Contains("SciFiSoldier.Skirmisher"), "Archetype catalog should cover Skirmisher.");
            Assert.IsTrue(coveredRoleIds.Contains("SciFiSoldier.LineCaster"), "Archetype catalog should cover LineCaster.");
            Assert.IsTrue(coveredRoleIds.Contains("SciFiSoldier.BacklineShooter"), "Archetype catalog should cover BacklineShooter.");
            Assert.IsTrue(coveredRoleIds.Contains("SciFiSoldier.Elite.ShieldBreaker"), "Archetype catalog should cover ShieldBreaker.");
            Assert.IsTrue(coveredRoleIds.Contains("SciFiSoldier.Elite.AuraCaptain"), "Archetype catalog should cover AuraCaptain.");
            Assert.IsTrue(coveredRoleIds.Contains("SciFiSoldier.Elite.FinalStandCommander"), "Archetype catalog should cover FinalStandCommander.");
            Assert.IsTrue(foundStaticTurretCandidate, "Archetype catalog should include at least one fixed sci-fi turret candidate.");
            Assert.IsTrue(foundBossCandidate, "Archetype catalog should track the dragon boss candidate outside soldier role decks.");
        }

        [Test]
        public void MeleeSoldierPrefabCandidateIsSceneFreeAndMappedToArchetype()
        {
            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(MeleeSoldierPrefabPath);
            Assert.IsNotNull(prefabAsset, $"Missing melee soldier prefab candidate at {MeleeSoldierPrefabPath}.");

            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(MeleeSoldierPrefabPath);
            try
            {
                BasicSoldierEnemy soldier = prefabRoot.GetComponent<BasicSoldierEnemy>();
                CombatHealth health = prefabRoot.GetComponent<CombatHealth>();
                CombatTargetSensor targetSensor = prefabRoot.GetComponent<CombatTargetSensor>();
                EnemyAttackTelegraphPresenter telegraphPresenter = prefabRoot.GetComponent<EnemyAttackTelegraphPresenter>();
                EnemyActionCameraCueDriver cameraCueDriver = prefabRoot.GetComponent<EnemyActionCameraCueDriver>();
                CombatVfxCuePlayer cuePlayer = prefabRoot.GetComponent<CombatVfxCuePlayer>();
                EnemyCombatVfxCueDriver vfxCueDriver = prefabRoot.GetComponent<EnemyCombatVfxCueDriver>();
                Animator animator = prefabRoot.GetComponentInChildren<Animator>(true);

                Assert.IsNotNull(soldier, "Melee soldier prefab should own BasicSoldierEnemy on the root.");
                Assert.IsNotNull(health, "Melee soldier prefab should own CombatHealth on the root.");
                Assert.IsNotNull(targetSensor, "Melee soldier prefab should own the shared target sensor on the root.");
                Assert.IsNotNull(telegraphPresenter, "Melee soldier prefab should own the telegraph presenter on the root.");
                Assert.IsNotNull(cameraCueDriver, "Melee soldier prefab should keep enemy camera cue emission available.");
                Assert.IsNotNull(cuePlayer, "Melee soldier prefab should own the combat VFX cue player on the root.");
                Assert.IsNotNull(vfxCueDriver, "Melee soldier prefab should own the combat VFX cue driver on the root.");
                Assert.IsNotNull(animator, "Melee soldier prefab should include the promoted MaintenanceWorker Animator.");

                Assert.AreSame(targetSensor, soldier.TargetSensor, "Prefab AI should use its local target sensor.");
                Assert.AreSame(health, soldier.SelfHealth, "Prefab AI should use its local health.");
                Assert.AreSame(LoadPatternProfile(ClosePunishPatternPath), soldier.PatternProfile, "Prefab should start from the ClosePunish reference profile.");
                Assert.IsNull(soldier.PatternDeck, "Single baseline prefab should not hide pattern-deck behavior behind its first candidate.");
                Assert.AreEqual(0, targetSensor.TargetCandidateCount, "Prefab target candidates should be injected by scenes or encounters, not carried from ActionFoundationTest.");

                SerializedObject cameraCueObject = new SerializedObject(cameraCueDriver);
                Assert.AreSame(soldier, cameraCueObject.FindProperty("agentSource").objectReferenceValue, "Enemy camera cue driver should listen to the local soldier agent.");
                Assert.IsNull(cameraCueObject.FindProperty("cameraController").objectReferenceValue, "Prefab should not serialize an ActionFoundationTest camera controller.");

                SerializedObject telegraphObject = new SerializedObject(telegraphPresenter);
                AssertLocalPrefabReference(prefabRoot, telegraphObject.FindProperty("telegraphObject").objectReferenceValue, "telegraph object");
                AssertLocalPrefabReference(prefabRoot, telegraphObject.FindProperty("telegraphRenderer").objectReferenceValue, "telegraph renderer");
                AssertLocalPrefabReference(prefabRoot, telegraphObject.FindProperty("poseRoot").objectReferenceValue, "telegraph pose root");

                SerializedObject cuePlayerObject = new SerializedObject(cuePlayer);
                UnityEngine.Object cueProfile = cuePlayerObject.FindProperty("profile").objectReferenceValue;
                string cueProfilePath = AssetDatabase.GetAssetPath(cueProfile).Replace('\\', '/');
                Assert.IsTrue(cueProfilePath.StartsWith("Assets/_Game/"), $"Prefab VFX profile should be game-owned, found {cueProfilePath}.");
                Assert.IsFalse(cueProfilePath.Contains("/_Imported/"), "Prefab VFX profile should not reference raw imported assets.");
                AssertLocalPrefabReference(prefabRoot, cuePlayerObject.FindProperty("pooledRoot").objectReferenceValue, "VFX pool root");

                SerializedObject vfxDriverObject = new SerializedObject(vfxCueDriver);
                Assert.AreSame(soldier, vfxDriverObject.FindProperty("agentSource").objectReferenceValue, "VFX driver should listen to the local soldier.");
                Assert.AreSame(health, vfxDriverObject.FindProperty("health").objectReferenceValue, "VFX driver should listen to the local health.");
                Assert.AreSame(cuePlayer, vfxDriverObject.FindProperty("cuePlayer").objectReferenceValue, "VFX driver should play through the local cue player.");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }

            CombatEnemyArchetypeProfile meleeArchetype =
                AssetDatabase.LoadAssetAtPath<CombatEnemyArchetypeProfile>(EnemyArchetypeRootPath + "/DB_Archetype_SciFiSoldier_Melee.asset");
            Assert.IsNotNull(meleeArchetype, "Missing sci-fi melee soldier archetype profile.");
            Assert.AreSame(prefabAsset, meleeArchetype.GameplayPrefab, "Melee archetype should point at the reusable game-owned prefab candidate.");
            Assert.IsFalse(meleeArchetype.RequiresDedicatedPrefabPromotion, "Melee soldier should no longer be marked as needing its first prefab promotion.");
        }

        [UnityTest]
        public IEnumerator EnemyPrefabReviewSceneWiresPrefabCandidateForManualCombat()
        {
            EditorSceneManager.LoadSceneInPlayMode(EnemyPrefabReviewScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;

            BasicSoldierEnemy soldier = RequireNamedRootComponent<BasicSoldierEnemy>(ReviewMeleeSoldierRootName);
            CombatHealth playerHealth = RequirePlayerHealth();
            CombatHealth enemyHealth = soldier.SelfHealth;
            PlayerCombatTargetSelector targetSelector = RequireObject<PlayerCombatTargetSelector>();
            ActionCameraController cameraController = RequireObject<ActionCameraController>();
            EnemyActionCameraCueDriver enemyCameraCueDriver = soldier.GetComponent<EnemyActionCameraCueDriver>();

            Assert.IsNotNull(enemyHealth, "Review soldier should expose local health.");
            Assert.AreEqual(1, soldier.TargetSensor.TargetCandidateCount, "Review soldier should receive one scene-provided player target.");
            Assert.IsTrue(soldier.TargetSensor.TryGetCurrentTarget(out Transform enemyTarget, out CombatHealth enemyTargetHealth), "Review soldier should resolve the player through scene-provided target candidates.");
            Assert.AreSame(playerHealth, enemyTargetHealth, "Review soldier should target the player health.");
            Assert.AreSame(playerHealth.transform, enemyTarget, "Review soldier should expose the player transform as current target.");

            Assert.AreEqual(1, targetSelector.TargetCandidateCount, "Review player selector should target only the reviewed prefab enemy.");
            Assert.IsTrue(targetSelector.TryGetCurrentTarget(out Transform playerTarget, out CombatHealth playerTargetHealth), "Review player selector should resolve the prefab enemy.");
            Assert.AreSame(enemyHealth, playerTargetHealth, "Review player selector should target the prefab enemy health.");
            Assert.AreSame(enemyHealth.transform, playerTarget, "Review player selector should expose the prefab enemy transform.");

            Assert.AreSame(playerHealth.transform, cameraController.Target, "Review camera should follow the player.");
            Assert.AreSame(enemyHealth.transform, cameraController.Threat, "Review camera should bias toward the reviewed prefab enemy.");
            Assert.IsNotNull(enemyCameraCueDriver, "Review prefab enemy should keep its enemy camera cue driver.");
            Assert.AreSame(cameraController, enemyCameraCueDriver.CameraController, "Review scene should provide the camera controller to the prefab instance.");
        }

        [Test]
        public void CombatVfxCueProfileUsesGameOwnedPatternCuePrefabs()
        {
            CombatVfxCueProfile profile = LoadCombatVfxCueProfile();

            foreach (CombatVfxCueId cueId in System.Enum.GetValues(typeof(CombatVfxCueId)))
            {
                Assert.IsTrue(profile.TryGetCue(cueId, out CombatVfxCue cue), $"{cueId} should be authored in the ActionFoundation combat VFX profile.");
                Assert.IsNotNull(cue.Prefab, $"{cueId} should reference a promoted game-owned VFX prefab.");

                string prefabPath = AssetDatabase.GetAssetPath(cue.Prefab).Replace('\\', '/');
                Assert.IsTrue(
                    prefabPath.StartsWith(CombatVfxPrefabRootPath),
                    $"{cueId} should reference a curated `_Game` VFX prefab, found {prefabPath}.");
                Assert.IsFalse(
                    prefabPath.Contains("/_Imported/"),
                    $"{cueId} should not reference raw imported VFX pack assets directly.");
                Assert.IsNotNull(
                    cue.Prefab.GetComponentInChildren<CombatVfxCueVisual>(true),
                    $"{cueId} should use a stable promoted mesh cue visual instead of raw particle shards.");
                Assert.IsEmpty(
                    cue.Prefab.GetComponentsInChildren<ParticleSystem>(true),
                    $"{cueId} should not use first-pass particle shards in the ActionFoundation readability slice.");
            }
        }

        [Test]
        public void ActionFoundationSceneBindsCombatVfxCueDrivers()
        {
            CombatVfxCueProfile profile = LoadCombatVfxCueProfile();
            PlayerActionController player = RequireObject<PlayerActionController>();
            CombatVfxCuePlayer playerCuePlayer = player.GetComponent<CombatVfxCuePlayer>();
            PlayerCombatVfxCueDriver playerDriver = player.GetComponent<PlayerCombatVfxCueDriver>();

            Assert.IsNotNull(playerCuePlayer, "Player root should own a CombatVfxCuePlayer for action VFX playback.");
            Assert.IsNotNull(playerDriver, "Player root should adapt player action events into VFX cues.");
            Assert.AreSame(profile, playerCuePlayer.Profile, "Player VFX player should read from the shared game-owned ActionFoundation combat VFX profile.");

            BasicSoldierEnemy closePunish = RequirePrimarySoldier();
            ValidateEnemyCombatVfxBinding(closePunish, profile, requireEliteController: false);

            BasicSoldierEnemy eliteTraits = RequireNamedRootComponent<BasicSoldierEnemy>(EliteTraitsEnemyRootName);
            ValidateEnemyCombatVfxBinding(eliteTraits, profile, requireEliteController: true);
        }

        [UnityTest]
        public IEnumerator EnemyElitePatternControllerReducesDamageThroughSharedHealthHook()
        {
            CombatAiElitePatternProfile shieldCycle = LoadElitePatternProfile(ShieldCycleEliteProfilePath);
            GameObject host = new GameObject("ElitePatternHook_TestHost");
            host.SetActive(false);
            CombatHealth health = host.AddComponent<CombatHealth>();
            EnemyElitePatternController controller = host.AddComponent<EnemyElitePatternController>();
            CombatAiElitePatternProfile signaledProfile = null;
            controller.SignalTriggered += profile => signaledProfile = profile;
            SerializedObject serializedObject = new SerializedObject(controller);
            SerializedProperty profiles = serializedObject.FindProperty("eliteProfiles");
            profiles.arraySize = 1;
            profiles.GetArrayElementAtIndex(0).objectReferenceValue = shieldCycle;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            host.SetActive(true);
            yield return null;

            Assert.AreSame(
                shieldCycle,
                signaledProfile,
                "Elite signal events should notify presentation drivers without making combat VFX part of the damage hook.");

            float healthBefore = health.CurrentHealth;
            bool damaged = health.TryApplyDamage(new DamageInfo(
                null,
                DamageTeam.Player,
                20f,
                host.transform.position,
                Vector3.forward,
                0f));

            Assert.IsTrue(damaged, "Elite health should still accept hostile damage through the shared health contract.");
            Assert.AreEqual(
                healthBefore - 20f * shieldCycle.DamageTakenMultiplier,
                health.CurrentHealth,
                0.01f,
                "ShieldCycle should reduce damage through CombatHealth.DamageModifying instead of hardcoded health branches.");
            Assert.IsTrue(
                controller.TryGetProfileState("ShieldCycle", out float guardMeter, out bool isBroken, out bool _),
                "Controller should expose the runtime guard state for tests and debug UI.");
            Assert.Less(guardMeter, shieldCycle.GuardMeter, "ShieldCycle guard meter should be consumed by incoming damage.");
            Assert.IsFalse(isBroken, "A single light hit should not break the authored ShieldCycle meter.");

            host.SetActive(false);
            host.SetActive(true);
            yield return null;

            Assert.IsTrue(
                controller.TryGetProfileState("ShieldCycle", out float resetGuardMeter, out _, out _),
                "Controller should rebuild profile state when an authored enemy is re-enabled.");
            Assert.AreEqual(
                shieldCycle.GuardMeter,
                resetGuardMeter,
                0.01f,
                "Elite profile runtime state should not leak stale guard values across disable/enable reuse.");

            UnityEngine.Object.Destroy(host);
        }

        [UnityTest]
        public IEnumerator EnemyElitePatternControllerAppliesAuthoredAuraAndSummonSignals()
        {
            CombatAiElitePatternProfile auraBuffer = LoadElitePatternProfile(AuraBufferEliteProfilePath);
            CombatAiElitePatternProfile summonPackage = LoadElitePatternProfile(SummonPackageEliteProfilePath);
            GameObject host = new GameObject("ElitePatternSignal_TestHost");
            GameObject protectedTarget = new GameObject("AuraProtected_TestTarget");
            GameObject summonSignal = new GameObject("SummonSignal_TestMarker");
            host.SetActive(false);
            summonSignal.SetActive(false);

            CombatHealth hostHealth = host.AddComponent<CombatHealth>();
            CombatHealth protectedHealth = protectedTarget.AddComponent<CombatHealth>();
            EnemyElitePatternController controller = host.AddComponent<EnemyElitePatternController>();
            SerializedObject serializedObject = new SerializedObject(controller);
            SerializedProperty profiles = serializedObject.FindProperty("eliteProfiles");
            profiles.arraySize = 2;
            profiles.GetArrayElementAtIndex(0).objectReferenceValue = auraBuffer;
            profiles.GetArrayElementAtIndex(1).objectReferenceValue = summonPackage;
            SerializedProperty auraTargets = serializedObject.FindProperty("auraProtectedTargets");
            auraTargets.arraySize = 1;
            auraTargets.GetArrayElementAtIndex(0).objectReferenceValue = protectedHealth;
            SerializedProperty summonSignals = serializedObject.FindProperty("summonSignalObjects");
            summonSignals.arraySize = 1;
            summonSignals.GetArrayElementAtIndex(0).objectReferenceValue = summonSignal;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            host.SetActive(true);
            yield return null;

            float protectedHealthBefore = protectedHealth.CurrentHealth;
            protectedHealth.TryApplyDamage(new DamageInfo(
                null,
                DamageTeam.Player,
                20f,
                protectedTarget.transform.position,
                Vector3.forward,
                0f));

            Assert.AreEqual(
                protectedHealthBefore - 20f * auraBuffer.DamageTakenMultiplier,
                protectedHealth.CurrentHealth,
                0.01f,
                "AuraBuffer should protect explicitly authored targets through the shared health hook without scene searches.");

            hostHealth.TryApplyDamage(new DamageInfo(
                null,
                DamageTeam.Player,
                30f,
                host.transform.position,
                Vector3.forward,
                0f));
            yield return null;

            Assert.IsTrue(summonSignal.activeSelf, "SummonPackage should activate authored signal objects without runtime Instantiate.");

            UnityEngine.Object.Destroy(host);
            UnityEngine.Object.Destroy(protectedTarget);
            UnityEngine.Object.Destroy(summonSignal);
        }

        [Test]
        public void TrainingDeckSelectsReadableRowsByDistancePriorityAndCooldown()
        {
            CombatAiPatternDeck trainingDeck = LoadPatternDeck(BasicSoldierTrainingDeckPath);
            CombatAiPatternProfile closePunish = LoadPatternProfile(ClosePunishPatternPath);
            CombatAiPatternProfile lungeStrike = LoadPatternProfile(LungeStrikePatternPath);
            CombatAiPatternProfile fanPressure = LoadPatternProfile(FanPressurePatternPath);
            CombatAiPatternProfile linePressure = LoadPatternProfile(LinePressurePatternPath);
            float[] lastUseTimes = { -1f, -1f, -1f, -1f };

            Assert.IsTrue(trainingDeck.TrySelectPattern(1.4f, null, 10f, lastUseTimes, out CombatAiPatternProfile selected, out int selectedIndex));
            Assert.AreSame(closePunish, selected, "Close range should prefer the compact punish row.");
            Assert.AreEqual(0, selectedIndex);

            Assert.IsTrue(trainingDeck.TrySelectPattern(2.4f, null, 10f, lastUseTimes, out selected, out selectedIndex));
            Assert.AreSame(lungeStrike, selected, "Mid range should prefer lunge over fan while both are valid because lunge has higher priority.");
            Assert.AreEqual(1, selectedIndex);

            Assert.IsTrue(trainingDeck.TrySelectPattern(4.0f, null, 10f, lastUseTimes, out selected, out selectedIndex));
            Assert.AreSame(fanPressure, selected, "Long-mid range should prefer fan pressure over line while both are valid.");
            Assert.AreEqual(2, selectedIndex);

            Assert.IsTrue(trainingDeck.TrySelectPattern(6.0f, null, 10f, lastUseTimes, out selected, out selectedIndex));
            Assert.AreSame(linePressure, selected, "Far range should fall to the line pressure row.");
            Assert.AreEqual(3, selectedIndex);

            lastUseTimes[2] = 10f;
            Assert.IsTrue(trainingDeck.TrySelectPattern(4.0f, fanPressure, 10.2f, lastUseTimes, out selected, out selectedIndex));
            Assert.AreSame(linePressure, selected, "A cooling fan row should let the next valid lower-priority pressure row take over instead of repeating immediately.");
            Assert.AreEqual(3, selectedIndex);

            lastUseTimes[3] = 10f;
            Assert.IsFalse(
                trainingDeck.TrySelectPattern(4.0f, linePressure, 10.2f, lastUseTimes, out selected, out selectedIndex),
                "When every valid row is cooling down, the deck should not silently reuse the previous profile.");
            Assert.IsNull(selected);
            Assert.AreEqual(-1, selectedIndex);
        }

        [UnityTest]
        public IEnumerator BasicSoldierCanSwapToLungeStrikePattern()
        {
            BasicSoldierEnemy soldier = RequirePrimarySoldier();
            ICombatAiAgent agent = soldier;
            CombatHealth playerHealth = RequirePlayerHealth();
            CombatAiPatternProfile lungeStrike = LoadPatternProfile(LungeStrikePatternPath);

            soldier.transform.position = Vector3.zero;
            soldier.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
            playerHealth.transform.position = Vector3.forward * 1.9f;
            Physics.SyncTransforms();

            Vector3 startPosition = soldier.transform.position;
            agent.ConfigurePattern(lungeStrike);
            yield return null;

            Assert.AreSame(lungeStrike, agent.PatternProfile, "Combat AI agent should accept a profile swap without prefab-specific manager routing.");
            Assert.AreEqual("LungeStrike", agent.PatternId, "Runtime pattern id should resolve from the active shared AI profile.");

            float timeout = 1.2f;
            while (timeout > 0f)
            {
                yield return null;
                timeout -= Time.deltaTime;
            }

            float forwardMovement = Vector3.Dot(soldier.transform.position - startPosition, Vector3.forward);
            Assert.Greater(forwardMovement, 0.1f, "LungeStrike should move the soldier forward during the active window using profile data.");
        }

        [UnityTest]
        public IEnumerator LinePressureLocksReadableLaneAndAllowsSideDodge()
        {
            BasicSoldierEnemy closePunish = RequireNamedRootComponent<BasicSoldierEnemy>(ClosePunishEnemyRootName);
            BasicSoldierEnemy lungeStrike = RequireNamedRootComponent<BasicSoldierEnemy>(LungeStrikeEnemyRootName);
            BasicSoldierEnemy heavyWindup = RequireNamedRootComponent<BasicSoldierEnemy>(HeavyWindupEnemyRootName);
            BasicSoldierEnemy linePressure = RequireNamedRootComponent<BasicSoldierEnemy>(LinePressureEnemyRootName);
            BasicSoldierEnemy fanPressure = RequireNamedRootComponent<BasicSoldierEnemy>(FanPressureEnemyRootName);
            BasicSoldierEnemy trainingDeck = RequireNamedRootComponent<BasicSoldierEnemy>(TrainingDeckEnemyRootName);
            CombatHealth playerHealth = RequirePlayerHealth();

            closePunish.enabled = false;
            lungeStrike.enabled = false;
            heavyWindup.enabled = false;
            fanPressure.enabled = false;
            trainingDeck.enabled = false;
            linePressure.enabled = true;
            linePressure.transform.position = Vector3.zero;
            linePressure.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
            playerHealth.transform.position = Vector3.forward * 4f;
            Physics.SyncTransforms();

            float timeout = 0.5f;
            while (timeout > 0f && linePressure.CurrentPatternState != CombatAiPatternState.Windup)
            {
                yield return null;
                timeout -= Time.deltaTime;
            }

            Assert.AreEqual(CombatAiPatternState.Windup, linePressure.CurrentPatternState, "LinePressure should enter a readable warning state before firing.");

            float playerHealthBeforeSideDodge = playerHealth.CurrentHealth;
            playerHealth.transform.position = new Vector3(1.2f, 0f, 4f);
            Physics.SyncTransforms();

            timeout = 1.0f;
            while (timeout > 0f && linePressure.CurrentPatternState != CombatAiPatternState.Recovery)
            {
                yield return null;
                timeout -= Time.deltaTime;
            }

            Assert.AreEqual(
                playerHealthBeforeSideDodge,
                playerHealth.CurrentHealth,
                0.001f,
                "LinePressure should not keep tracking until impact; a sideways dodge outside the narrow lane should avoid damage.");
        }

        [UnityTest]
        public IEnumerator PatternDeckSelectsFanPressureForMidRangeThreat()
        {
            BasicSoldierEnemy closePunish = RequireNamedRootComponent<BasicSoldierEnemy>(ClosePunishEnemyRootName);
            BasicSoldierEnemy lungeStrike = RequireNamedRootComponent<BasicSoldierEnemy>(LungeStrikeEnemyRootName);
            BasicSoldierEnemy heavyWindup = RequireNamedRootComponent<BasicSoldierEnemy>(HeavyWindupEnemyRootName);
            BasicSoldierEnemy linePressure = RequireNamedRootComponent<BasicSoldierEnemy>(LinePressureEnemyRootName);
            BasicSoldierEnemy fanPressure = RequireNamedRootComponent<BasicSoldierEnemy>(FanPressureEnemyRootName);
            BasicSoldierEnemy trainingDeckSoldier = RequireNamedRootComponent<BasicSoldierEnemy>(TrainingDeckEnemyRootName);
            CombatHealth playerHealth = RequirePlayerHealth();
            CombatAiPatternDeck trainingDeck = LoadPatternDeck(BasicSoldierTrainingDeckPath);

            closePunish.enabled = false;
            lungeStrike.enabled = false;
            heavyWindup.enabled = false;
            linePressure.enabled = false;
            fanPressure.enabled = false;
            trainingDeckSoldier.enabled = true;
            trainingDeckSoldier.transform.position = Vector3.zero;
            trainingDeckSoldier.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
            playerHealth.transform.position = new Vector3(0.9f, 0f, 4f);
            Physics.SyncTransforms();

            yield return null;

            Assert.AreSame(trainingDeck, trainingDeckSoldier.PatternDeck, "Authored scene sample should use the reusable pattern deck without prefab-specific manager routing.");
            Assert.AreEqual("FanPressure", trainingDeckSoldier.PatternId, "Training deck should choose FanPressure for a mid-range target inside the fan pressure band.");
            Assert.AreEqual(2, trainingDeckSoldier.ActivePatternDeckIndex, "Training deck sample should expose the selected entry index for test/debug readability.");
            Assert.IsTrue(trainingDeckSoldier.TryGetActivePatternDeckEntry(out CombatAiPatternDeckEntry activeEntry), "Training deck sample should expose the active entry data without pattern-id branching.");
            Assert.AreSame(trainingDeck.GetEntry(2).Profile, activeEntry.Profile, "Active entry should be the authored FanPressure deck row.");

            float healthBeforeFan = playerHealth.CurrentHealth;
            float timeout = 1.2f;
            while (timeout > 0f && trainingDeckSoldier.CurrentPatternState != CombatAiPatternState.Recovery)
            {
                yield return null;
                timeout -= Time.deltaTime;
            }

            Assert.Less(playerHealth.CurrentHealth, healthBeforeFan, "FanPressure should damage a target inside the committed fan cone.");
        }

        [UnityTest]
        public IEnumerator PatternDeckKeepsCommittedLungeUntilWindup()
        {
            BasicSoldierEnemy closePunish = RequireNamedRootComponent<BasicSoldierEnemy>(ClosePunishEnemyRootName);
            BasicSoldierEnemy lungeStrike = RequireNamedRootComponent<BasicSoldierEnemy>(LungeStrikeEnemyRootName);
            BasicSoldierEnemy heavyWindup = RequireNamedRootComponent<BasicSoldierEnemy>(HeavyWindupEnemyRootName);
            BasicSoldierEnemy linePressure = RequireNamedRootComponent<BasicSoldierEnemy>(LinePressureEnemyRootName);
            BasicSoldierEnemy fanPressure = RequireNamedRootComponent<BasicSoldierEnemy>(FanPressureEnemyRootName);
            BasicSoldierEnemy trainingDeckSoldier = RequireNamedRootComponent<BasicSoldierEnemy>(TrainingDeckEnemyRootName);
            CombatHealth playerHealth = RequirePlayerHealth();

            closePunish.enabled = false;
            lungeStrike.enabled = false;
            heavyWindup.enabled = false;
            linePressure.enabled = false;
            fanPressure.enabled = false;
            trainingDeckSoldier.enabled = true;
            trainingDeckSoldier.transform.position = Vector3.zero;
            trainingDeckSoldier.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
            playerHealth.transform.position = Vector3.forward * 2.4f;
            Physics.SyncTransforms();

            yield return null;

            Assert.AreEqual("LungeStrike", trainingDeckSoldier.PatternId, "Training deck should initially choose LungeStrike for this mid-range approach band.");
            Assert.AreEqual(1, trainingDeckSoldier.ActivePatternDeckIndex, "The selected lunge row should be visible while the soldier approaches.");

            float timeout = 0.5f;
            while (timeout > 0f && trainingDeckSoldier.CurrentPatternState != CombatAiPatternState.Windup)
            {
                yield return null;
                timeout -= Time.deltaTime;
            }

            Assert.AreEqual(CombatAiPatternState.Windup, trainingDeckSoldier.CurrentPatternState, "A committed lunge row should reach windup instead of being replaced by ClosePunish while closing distance.");
            Assert.AreEqual("LungeStrike", trainingDeckSoldier.PatternId, "The committed lunge row should remain active until its warning starts.");
            Assert.AreEqual(1, trainingDeckSoldier.ActivePatternDeckIndex, "The active deck index should still point at the committed lunge row during windup.");
        }

        [UnityTest]
        public IEnumerator BasicSoldierEmitsReadablePatternStateSignals()
        {
            BasicSoldierEnemy soldier = RequirePrimarySoldier();
            ICombatAiAgent agent = soldier;
            CombatHealth playerHealth = RequirePlayerHealth();
            CombatAiPatternProfile heavyWindup = LoadPatternProfile(HeavyWindupPatternPath);
            List<CombatAiPatternState> observedStates = new List<CombatAiPatternState>();

            soldier.transform.position = Vector3.zero;
            soldier.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
            playerHealth.transform.position = Vector3.forward * 8f;
            Physics.SyncTransforms();
            yield return new WaitForSeconds(1.2f);

            agent.PatternStateChanged += HandlePatternStateChanged;
            try
            {
                playerHealth.transform.position = Vector3.forward * 1.55f;
                agent.ConfigurePattern(heavyWindup);
                Physics.SyncTransforms();

                float timeout = 1.8f;
                while (timeout > 0f && !observedStates.Contains(CombatAiPatternState.Recovery))
                {
                    yield return null;
                    timeout -= Time.deltaTime;
                }
            }
            finally
            {
                agent.PatternStateChanged -= HandlePatternStateChanged;
            }

            Assert.Contains(CombatAiPatternState.Windup, observedStates, "Basic soldier should expose windup as a shared readable pattern state.");
            Assert.Contains(CombatAiPatternState.AttackActive, observedStates, "Basic soldier should expose attack active as a shared readable pattern state.");
            Assert.Contains(CombatAiPatternState.Recovery, observedStates, "Basic soldier should expose recovery as a shared readable pattern state.");

            void HandlePatternStateChanged(CombatAiPatternState state, CombatAiPatternProfile _)
            {
                observedStates.Add(state);
            }
        }

        [UnityTest]
        public IEnumerator EnemyPatternStateCanDriveActionCameraCue()
        {
            BasicSoldierEnemy soldier = RequirePrimarySoldier();
            ICombatAiAgent agent = soldier;
            CombatHealth playerHealth = RequirePlayerHealth();
            ActionCameraController cameraController = RequireObject<ActionCameraController>();
            EnemyActionCameraCueDriver enemyCameraCueDriver = soldier.GetComponent<EnemyActionCameraCueDriver>();
            CombatAiPatternProfile heavyWindup = LoadPatternProfile(HeavyWindupPatternPath);
            bool sawWindup = false;

            Assert.IsNotNull(enemyCameraCueDriver, "Primary soldier should own its enemy camera cue driver so multi-enemy cues do not depend on a camera-attached singleton.");
            Assert.AreSame(soldier, enemyCameraCueDriver.AgentSource, "Enemy camera cue driver should read a serialized combat AI agent source, not search the scene.");
            Assert.AreSame(cameraController, enemyCameraCueDriver.CameraController, "Enemy camera cue driver should request cues through the existing action camera controller.");

            soldier.transform.position = Vector3.zero;
            soldier.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
            playerHealth.transform.position = Vector3.forward * 8f;
            Physics.SyncTransforms();
            yield return new WaitForSeconds(1.2f);

            agent.PatternStateChanged += HandlePatternStateChanged;
            try
            {
                playerHealth.transform.position = Vector3.forward * 1.55f;
                agent.ConfigurePattern(heavyWindup);
                Physics.SyncTransforms();

                float timeout = 1.2f;
                while (timeout > 0f && !(sawWindup && cameraController.HasActiveCue))
                {
                    yield return null;
                    timeout -= Time.deltaTime;
                }
            }
            finally
            {
                agent.PatternStateChanged -= HandlePatternStateChanged;
            }

            Assert.IsTrue(sawWindup, "HeavyWindup should emit a Windup state that camera/UI consumers can observe.");
            Assert.IsTrue(cameraController.HasActiveCue, "Enemy windup state should request a short action camera cue through the enemy camera cue driver.");

            void HandlePatternStateChanged(CombatAiPatternState state, CombatAiPatternProfile _)
            {
                if (state == CombatAiPatternState.Windup)
                {
                    sawWindup = true;
                }
            }
        }

        [UnityTest]
        public IEnumerator BasicSoldierUsesPromotedMaintenanceWorkerVisualAndAnimator()
        {
            GameObject enemyVisual = RequireNamedGameObject(EnemyVisualName);
            GameObject placeholderBody = RequireNamedGameObject(EnemyPlaceholderBodyName);
            Animator enemyAnimator = enemyVisual.GetComponent<Animator>();
            EnemyAttackTelegraphPresenter telegraphPresenter = RequirePrimarySoldier().GetComponent<EnemyAttackTelegraphPresenter>();
            Renderer[] renderers = enemyVisual.GetComponentsInChildren<Renderer>(true);

            yield return null;

            Assert.IsTrue(enemyVisual.activeInHierarchy, "Promoted MaintenanceWorker visual should be active in the scene.");
            Assert.IsFalse(placeholderBody.activeSelf, "The old capsule placeholder should stay inactive after the promoted enemy visual is wired.");
            Assert.IsNotNull(enemyAnimator, "Promoted MaintenanceWorker visual should own the enemy Animator.");
            Assert.IsNotNull(enemyAnimator.avatar, "Promoted MaintenanceWorker Animator should use the imported humanoid avatar.");
            Assert.IsNotNull(enemyAnimator.runtimeAnimatorController, "Promoted MaintenanceWorker Animator should use a game-owned Animator Controller.");
            Assert.IsFalse(enemyAnimator.applyRootMotion, "Enemy animation should stay presentation-only while movement remains owned by BasicSoldierEnemy.");
            Assert.Greater(renderers.Length, 0, "Promoted MaintenanceWorker visual should expose renderers for hit feedback.");
            Assert.AreSame(enemyVisual.transform, telegraphPresenter.PoseRoot, "Enemy telegraph pose offsets should animate the promoted visual instead of the disabled placeholder.");
        }

        [UnityTest]
        public IEnumerator BasicSoldierPromotedAnimatorReceivesPatternTriggers()
        {
            PlayerMovementController movement = RequireObject<PlayerMovementController>();
            CombatHealth playerHealth = RequirePlayerHealth();
            CombatHealth enemyHealth = RequireEnemyHealth();
            Animator enemyAnimator = RequireNamedGameObject(EnemyVisualName).GetComponent<Animator>();
            Assert.IsNotNull(enemyAnimator, "Promoted enemy visual should have an Animator before pattern trigger validation.");

            PositionPlayerForAttack(movement.transform, enemyHealth.transform);
            Physics.SyncTransforms();
            yield return null;

            bool reachedAttack = false;
            float timeout = 1.2f;
            while (timeout > 0f)
            {
                if (AnimatorIsInOrTransitioningTo(enemyAnimator, "Attack"))
                {
                    reachedAttack = true;
                    break;
                }

                yield return null;
                timeout -= Time.deltaTime;
            }

            Assert.IsTrue(reachedAttack, "Basic soldier ClosePunish timing should request the promoted attack animation after readable windup.");

            enemyHealth.TryApplyDamage(new DamageInfo(
                playerHealth,
                DamageTeam.Player,
                1f,
                enemyHealth.transform.position,
                Vector3.forward,
                0f));

            bool reachedHit = false;
            timeout = 0.35f;
            while (timeout > 0f)
            {
                if (AnimatorIsInOrTransitioningTo(enemyAnimator, "Hit"))
                {
                    reachedHit = true;
                    break;
                }

                yield return null;
                timeout -= Time.deltaTime;
            }

            Assert.IsTrue(reachedHit, "Enemy light damage should request the promoted hit reaction animation.");

            enemyHealth.TryApplyDamage(new DamageInfo(
                playerHealth,
                DamageTeam.Player,
                enemyHealth.MaxHealth,
                enemyHealth.transform.position,
                Vector3.forward,
                0f));

            bool reachedDeath = false;
            timeout = 0.35f;
            while (timeout > 0f)
            {
                if (AnimatorIsInOrTransitioningTo(enemyAnimator, "Death"))
                {
                    reachedDeath = true;
                    break;
                }

                yield return null;
                timeout -= Time.deltaTime;
            }

            Assert.IsTrue(reachedDeath, "Enemy death should request the promoted death animation.");
        }

        [UnityTest]
        public IEnumerator BasicSoldierFatalDamageRoutesDirectlyToDeathAnimation()
        {
            CombatHealth playerHealth = RequirePlayerHealth();
            CombatHealth enemyHealth = RequireEnemyHealth();
            Animator enemyAnimator = RequireNamedGameObject(EnemyVisualName).GetComponent<Animator>();
            CharacterController enemyController = RequirePrimarySoldier().GetComponent<CharacterController>();
            Assert.IsNotNull(enemyAnimator, "Promoted enemy visual should have an Animator before fatal-damage validation.");
            Assert.IsNotNull(enemyController, "Basic soldier should keep a controller for ground-height validation.");

            yield return null;

            enemyHealth.TryApplyDamage(new DamageInfo(
                playerHealth,
                DamageTeam.Player,
                enemyHealth.MaxHealth,
                enemyHealth.transform.position,
                Vector3.forward,
                0f));

            bool reachedDeath = false;
            bool reachedHit = false;
            float timeout = 0.45f;
            while (timeout > 0f)
            {
                if (AnimatorIsInOrTransitioningTo(enemyAnimator, "Hit"))
                {
                    reachedHit = true;
                }

                if (AnimatorIsInOrTransitioningTo(enemyAnimator, "Death"))
                {
                    reachedDeath = true;
                    break;
                }

                yield return null;
                timeout -= Time.deltaTime;
            }

            Assert.IsTrue(reachedDeath, "Fatal damage should route the enemy directly to the promoted death animation.");
            Assert.IsFalse(reachedHit, "Fatal damage should not request the light hit reaction before death.");

            yield return new WaitForSeconds(0.35f);

            Bounds deathBounds = CollectRenderableBounds(enemyAnimator.gameObject);
            float controllerGroundY = enemyController.bounds.min.y;
            Assert.LessOrEqual(
                deathBounds.min.y,
                controllerGroundY + 0.08f,
                "Enemy death pose should settle near the ground instead of hovering above the controller base.");
        }

        [Test]
        public void PlayerAndAllySummonTeamsShareFriendlyFireRules()
        {
            Assert.IsTrue(CombatTeamUtility.AreAllied(DamageTeam.Player, DamageTeam.AllySummon), "Player and future AllySummon teams should share friendly-fire rules.");
            Assert.IsTrue(CombatTeamUtility.AreHostile(DamageTeam.Enemy, DamageTeam.AllySummon), "Enemies should treat future AllySummon actors as hostile targets.");
            Assert.IsTrue(CombatTeamUtility.AreHostile(DamageTeam.Enemy, DamageTeam.Player), "Enemies should still treat the player as hostile.");
        }

        [UnityTest]
        public IEnumerator BasicSoldierTelegraphPresenterMakesWindupReadable()
        {
            EnemyAttackTelegraphPresenter telegraphPresenter = RequirePrimarySoldier().GetComponent<EnemyAttackTelegraphPresenter>();
            Assert.IsNotNull(telegraphPresenter.TelegraphRenderer, "Basic soldier telegraph should have a renderer for visible attack warning.");
            Assert.IsNotNull(telegraphPresenter.PoseRoot, "Basic soldier telegraph should have a pose root so windup can read before real enemy animations are promoted.");

            telegraphPresenter.Hide();
            yield return null;

            Vector3 hiddenPosePosition = telegraphPresenter.PoseRoot.localPosition;
            Assert.IsFalse(telegraphPresenter.IsVisible, "Telegraph should start hidden outside the windup/active attack window.");

            telegraphPresenter.ShowWindup(0.75f);
            yield return null;

            Assert.IsTrue(telegraphPresenter.IsVisible, "Telegraph should become visible during enemy windup.");
            Assert.Greater(telegraphPresenter.TelegraphRenderer.transform.localScale.z, 1f, "Windup telegraph should grow forward enough to read attack reach.");
            Assert.Less(telegraphPresenter.PoseRoot.localPosition.z, hiddenPosePosition.z, "Windup should pull the promoted enemy visual back before attack release.");

            telegraphPresenter.ShowActive(0f);
            yield return null;

            Assert.Greater(telegraphPresenter.TelegraphRenderer.transform.localScale.z, 1.6f, "Active telegraph should flash larger than the windup range at release.");

            telegraphPresenter.Hide();
            yield return null;

            Assert.IsFalse(telegraphPresenter.IsVisible, "Telegraph should hide cleanly after the active attack cue.");
            Assert.Less(Vector3.Distance(hiddenPosePosition, telegraphPresenter.PoseRoot.localPosition), 0.001f, "Telegraph presenter should restore pose offset after hiding.");
        }

        [UnityTest]
        public IEnumerator DodgeAppliesAndClearsVisibleFeedbackTint()
        {
            PlayerActionController actions = RequireObject<PlayerActionController>();
            PlayerDodgeFeedback dodgeFeedback = RequireObject<PlayerDodgeFeedback>();
            Renderer[] targetRenderers = RequirePlayerFeedbackRenderers(dodgeFeedback.transform);
            MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();

            actions.QueueDodge();
            yield return null;

            Assert.IsTrue(
                AnyRendererHasColor(targetRenderers, propertyBlock, new Color(0.75f, 1f, 1f)),
                "Dodge feedback should tint the player renderer during the dodge movement window.");

            yield return new WaitForSeconds(0.7f);

            Assert.IsFalse(
                AnyRendererHasColor(targetRenderers, propertyBlock, new Color(0.75f, 1f, 1f)),
                "Dodge feedback should clear before or during recovery instead of sticking forever.");
        }

        [UnityTest]
        public IEnumerator ActionCameraCueDriverRequestsAndClearsShortCues()
        {
            PlayerMovementController movement = RequireObject<PlayerMovementController>();
            PlayerActionController actions = RequireObject<PlayerActionController>();
            ActionCameraController cameraController = RequireObject<ActionCameraController>();
            ActionCameraCueDriver cameraCueDriver = RequireObject<ActionCameraCueDriver>();
            CombatHealth enemyHealth = RequireEnemyHealth();
            Camera mainCamera = Camera.main;
            Assert.IsNotNull(mainCamera, "Action foundation scene should have a main camera.");
            Assert.IsNotNull(cameraCueDriver.CueProfile, "Action camera cues should come from a game-owned ActionCameraCueProfile asset.");
            float baseFieldOfView = mainCamera.fieldOfView;

            actions.QueueDodge();
            yield return null;

            Assert.IsTrue(cameraController.HasActiveCue, "Dodge should request a short action camera cue.");
            yield return new WaitForSeconds(0.05f);
            Assert.Greater(mainCamera.fieldOfView, baseFieldOfView + 0.2f, "Dodge camera cue should briefly widen FOV for speed readability.");

            yield return new WaitForSeconds(0.35f);

            Assert.IsFalse(cameraController.HasActiveCue, "Dodge camera cue should clear instead of sticking.");

            yield return new WaitForSeconds(0.55f);

            PositionPlayerForAttack(movement.transform, enemyHealth.transform);
            Physics.SyncTransforms();
            yield return null;

            actions.QueueBasicAttack();
            yield return null;

            Assert.IsTrue(cameraController.HasActiveCue, "Basic attack should request a short action camera cue.");
        }

        [UnityTest]
        public IEnumerator ActionCameraOrbitStaysIndependentFromPlayerFacing()
        {
            PlayerMovementController movement = RequireObject<PlayerMovementController>();
            ActionCameraController cameraController = RequireObject<ActionCameraController>();

            yield return null;

            float initialYaw = cameraController.OrbitYawDegrees;
            movement.transform.rotation = Quaternion.Euler(0f, 120f, 0f);
            yield return null;

            Assert.Less(
                Mathf.Abs(Mathf.DeltaAngle(initialYaw, cameraController.OrbitYawDegrees)),
                10f,
                "Player facing should not instantly rotate the base camera orbit.");

            cameraController.SetOrbitInput(Vector2.right);
            yield return new WaitForSeconds(0.12f);
            cameraController.SetOrbitInput(Vector2.zero);

            Assert.Greater(
                Mathf.Abs(Mathf.DeltaAngle(initialYaw, cameraController.OrbitYawDegrees)),
                5f,
                "Manual camera orbit input should rotate the camera independently from player movement.");
        }

        [UnityTest]
        public IEnumerator ActionCameraUsesCloseLowRearCombatPreset()
        {
            PlayerMovementController movement = RequireObject<PlayerMovementController>();
            Camera mainCamera = Camera.main;
            Assert.IsNotNull(mainCamera, "Action foundation scene should have a main camera.");

            yield return new WaitForSeconds(0.35f);

            Vector3 relative = mainCamera.transform.position - movement.transform.position;
            float planarDistance = Vector3.ProjectOnPlane(relative, Vector3.up).magnitude;

            Assert.Less(relative.y, 2.8f, "Camera should sit lower than the earlier top-down inspection preset.");
            Assert.Greater(relative.y, 1.5f, "Camera should remain above shoulder/torso height for target readability.");
            Assert.Less(planarDistance, 4.2f, "Camera should stay closer to the character's rear than the old distant overview preset.");
        }

        [UnityTest]
        public IEnumerator EncounterCanWinAndFail()
        {
            CombatHealth playerHealth = RequirePlayerHealth();
            CombatHealth enemyHealth = RequireEnemyHealth();
            ActionFoundationTestEncounter encounter = RequireObject<ActionFoundationTestEncounter>();

            enemyHealth.TryApplyDamage(new DamageInfo(
                playerHealth,
                DamageTeam.Player,
                enemyHealth.MaxHealth,
                enemyHealth.transform.position,
                Vector3.forward,
                0f));
            yield return null;

            Assert.IsTrue(encounter.IsWon, "Encounter should win when the soldier dies.");

            EditorSceneManager.LoadSceneInPlayMode(ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;

            playerHealth = RequirePlayerHealth();
            enemyHealth = RequireEnemyHealth();
            encounter = RequireObject<ActionFoundationTestEncounter>();

            playerHealth.TryApplyDamage(new DamageInfo(
                enemyHealth,
                DamageTeam.Enemy,
                playerHealth.MaxHealth,
                playerHealth.transform.position,
                Vector3.back,
                0f));
            yield return null;

            Assert.IsTrue(encounter.IsFailed, "Encounter should fail when the player dies.");
        }

        private static bool ApproximatelyColor(Color expected, Color actual)
        {
            return Mathf.Abs(expected.r - actual.r) <= 0.01f
                && Mathf.Abs(expected.g - actual.g) <= 0.01f
                && Mathf.Abs(expected.b - actual.b) <= 0.01f
                && Mathf.Abs(expected.a - actual.a) <= 0.01f;
        }

        private static bool AnyRendererHasColor(
            IReadOnlyList<Renderer> renderers,
            MaterialPropertyBlock propertyBlock,
            Color expectedColor)
        {
            for (int i = 0; i < renderers.Count; i++)
            {
                Renderer targetRenderer = renderers[i];
                if (targetRenderer == null)
                {
                    continue;
                }

                targetRenderer.GetPropertyBlock(propertyBlock);
                if (ApproximatelyColor(expectedColor, propertyBlock.GetColor("_BaseColor")))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool AnimatorIsInOrTransitioningTo(Animator animator, string stateName)
        {
            if (animator == null)
            {
                return false;
            }

            if (animator.GetCurrentAnimatorStateInfo(0).IsName(stateName))
            {
                return true;
            }

            return animator.IsInTransition(0) && animator.GetNextAnimatorStateInfo(0).IsName(stateName);
        }

        private static bool AnimatorIsInAnyDirectionalDodge(Animator animator)
        {
            return AnimatorIsInOrTransitioningTo(animator, "DodgeForward")
                || AnimatorIsInOrTransitioningTo(animator, "DodgeBack")
                || AnimatorIsInOrTransitioningTo(animator, "DodgeLeft")
                || AnimatorIsInOrTransitioningTo(animator, "DodgeRight");
        }

        private static Renderer[] RequirePlayerFeedbackRenderers(Transform playerRoot)
        {
            Renderer[] renderers = playerRoot.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                Assert.Fail("Player dodge feedback should have at least one child renderer to tint.");
            }

            return renderers;
        }

        private static void AssertGameOwnedObjectReference(GameObject asset, string label)
        {
            if (asset == null)
            {
                return;
            }

            string assetPath = AssetDatabase.GetAssetPath(asset).Replace('\\', '/');
            Assert.IsTrue(assetPath.StartsWith("Assets/_Game/"), $"{label} should reference a promoted `_Game` asset, found {assetPath}.");
            Assert.IsFalse(assetPath.Contains("/_Imported/"), $"{label} should not reference raw imported assets.");
        }

        private static void AssertLocalPrefabReference(GameObject prefabRoot, UnityEngine.Object reference, string label)
        {
            Assert.IsNotNull(reference, $"{label} should be assigned.");

            if (reference is GameObject gameObject)
            {
                Assert.IsTrue(
                    gameObject == prefabRoot || gameObject.transform.IsChildOf(prefabRoot.transform),
                    $"{label} should reference a local prefab GameObject.");
                return;
            }

            if (reference is Component component)
            {
                Assert.IsTrue(
                    component.gameObject == prefabRoot || component.transform.IsChildOf(prefabRoot.transform),
                    $"{label} should reference a local prefab component.");
                return;
            }

            Assert.Fail($"{label} should reference a local GameObject or Component.");
        }

        private static CombatAiPatternProfile LoadPatternProfile(string assetPath)
        {
            CombatAiPatternProfile profile = AssetDatabase.LoadAssetAtPath<CombatAiPatternProfile>(assetPath);
            if (profile == null)
            {
                Assert.Fail($"Missing combat AI pattern profile at {assetPath}.");
            }

            return profile;
        }

        private static CombatAiPatternDeck LoadPatternDeck(string assetPath)
        {
            CombatAiPatternDeck deck = AssetDatabase.LoadAssetAtPath<CombatAiPatternDeck>(assetPath);
            if (deck == null)
            {
                Assert.Fail($"Missing combat AI pattern deck at {assetPath}.");
            }

            return deck;
        }

        private static CombatAiElitePatternProfile LoadElitePatternProfile(string assetPath)
        {
            CombatAiElitePatternProfile profile = AssetDatabase.LoadAssetAtPath<CombatAiElitePatternProfile>(assetPath);
            if (profile == null)
            {
                Assert.Fail($"Missing combat AI elite pattern profile at {assetPath}.");
            }

            return profile;
        }

        private static CombatVfxCueProfile LoadCombatVfxCueProfile()
        {
            CombatVfxCueProfile profile = AssetDatabase.LoadAssetAtPath<CombatVfxCueProfile>(CombatVfxCueProfilePath);
            if (profile == null)
            {
                Assert.Fail($"Missing combat VFX cue profile at {CombatVfxCueProfilePath}.");
            }

            return profile;
        }

        private static void ValidateEnemyCombatVfxBinding(
            BasicSoldierEnemy soldier,
            CombatVfxCueProfile expectedProfile,
            bool requireEliteController)
        {
            CombatVfxCuePlayer cuePlayer = soldier.GetComponent<CombatVfxCuePlayer>();
            EnemyCombatVfxCueDriver driver = soldier.GetComponent<EnemyCombatVfxCueDriver>();
            Assert.IsNotNull(cuePlayer, $"{soldier.name} should own a CombatVfxCuePlayer.");
            Assert.IsNotNull(driver, $"{soldier.name} should adapt pattern/damage/death events into VFX cues.");
            Assert.AreSame(expectedProfile, cuePlayer.Profile, $"{soldier.name} should use the shared game-owned combat VFX profile.");

            SerializedObject serializedObject = new SerializedObject(driver);
            Assert.AreSame(soldier, serializedObject.FindProperty("agentSource").objectReferenceValue, $"{soldier.name} VFX driver should listen to its local AI agent.");
            Assert.AreSame(soldier.SelfHealth, serializedObject.FindProperty("health").objectReferenceValue, $"{soldier.name} VFX driver should listen to its local health.");
            Assert.AreSame(cuePlayer, serializedObject.FindProperty("cuePlayer").objectReferenceValue, $"{soldier.name} VFX driver should play through the local cue player.");
            Assert.AreEqual(8, serializedObject.FindProperty("patternCueOverrides").arraySize, $"{soldier.name} should carry pattern-profile VFX cue mappings for all authored general/elite attack profiles.");
            Assert.AreEqual(5, serializedObject.FindProperty("eliteCueOverrides").arraySize, $"{soldier.name} should carry elite-signal VFX cue mappings for future shared enemy/summon trait reads.");

            UnityEngine.Object eliteController = serializedObject.FindProperty("elitePatternController").objectReferenceValue;
            if (requireEliteController)
            {
                Assert.IsNotNull(eliteController, $"{soldier.name} should bind the authored elite pattern controller for elite VFX signals.");
            }
        }

        private static Bounds CollectRenderableBounds(GameObject root)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                Assert.Fail($"{root.name} should have at least one child renderer.");
            }

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            return bounds;
        }

        private static void PositionPlayerForAttack(Transform player, Transform enemy)
        {
            player.position = enemy.position - Vector3.forward * 1.25f;
            player.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
        }

        private static void AssertEnemyTargetsPlayer(BasicSoldierEnemy soldier, CombatHealth playerHealth)
        {
            Assert.IsNotNull(soldier.TargetSensor, $"{soldier.name} should expose a shared target sensor.");
            Assert.AreEqual(1, soldier.TargetSensor.TargetCandidateCount, $"{soldier.name} should target authored player candidates instead of scene scanning.");
            Assert.IsTrue(soldier.TargetSensor.TryGetCurrentTarget(out Transform target, out CombatHealth targetHealth), $"{soldier.name} should resolve the player target.");
            Assert.AreSame(playerHealth, targetHealth, $"{soldier.name} should target the player health component.");
            Assert.AreSame(playerHealth.transform, target, $"{soldier.name} should expose the player transform through target sensing.");
        }

        private static CombatHealth RequirePlayerHealth()
        {
            return RequireHealth(DamageTeam.Player);
        }

        private static CombatHealth RequireEnemyHealth()
        {
            return RequirePrimarySoldier().SelfHealth;
        }

        private static BasicSoldierEnemy RequirePrimarySoldier()
        {
            return RequireNamedRootComponent<BasicSoldierEnemy>(ClosePunishEnemyRootName);
        }

        private static Animator RequirePlayerAnimator()
        {
            Animator animator = RequireNamedGameObject(PlayerVisualName).GetComponent<Animator>();
            if (animator == null)
            {
                Assert.Fail($"{PlayerVisualName} is missing Animator.");
            }

            return animator;
        }

        private static GameObject RequireNamedGameObject(string objectName)
        {
            GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                Transform[] transforms = roots[i].GetComponentsInChildren<Transform>(true);
                for (int j = 0; j < transforms.Length; j++)
                {
                    if (transforms[j].name == objectName)
                    {
                        return transforms[j].gameObject;
                    }
                }
            }

            Assert.Fail($"Missing required GameObject {objectName}.");
            return null;
        }

        private static T RequireNamedRootComponent<T>(string rootName) where T : Component
        {
            GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();
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
                    Assert.Fail($"{rootName} is missing required component {typeof(T).Name}.");
                }

                return component;
            }

            Assert.Fail($"Missing required root GameObject {rootName}.");
            return null;
        }

        private static CombatHealth RequireHealth(DamageTeam team)
        {
            List<CombatHealth> healths = CollectObjects<CombatHealth>();
            for (int i = 0; i < healths.Count; i++)
            {
                if (healths[i].Team == team)
                {
                    return healths[i];
                }
            }

            Assert.Fail($"Missing CombatHealth for team {team}.");
            return null;
        }

        private static T RequireObject<T>() where T : Component
        {
            List<T> found = CollectObjects<T>();
            if (found.Count == 0)
            {
                Assert.Fail($"Missing required component {typeof(T).Name}.");
            }

            return found[0];
        }

        private static List<T> CollectObjects<T>() where T : Component
        {
            List<T> results = new List<T>();
            GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                results.AddRange(roots[i].GetComponentsInChildren<T>(true));
            }

            return results;
        }
    }
}
