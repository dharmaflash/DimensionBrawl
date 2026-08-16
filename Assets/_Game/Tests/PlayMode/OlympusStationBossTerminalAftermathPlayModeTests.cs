using System;
using System.Collections;
using System.Reflection;
using DimensionBrawl.Combat;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.Player;
using DimensionBrawl.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace DimensionBrawl.Tests
{
    public sealed class OlympusStationBossTerminalAftermathPlayModeTests
    {
        [UnityTest]
        public IEnumerator TerminalTakeoverStopsLateShotsAndPreservesExternalScaleAndForeignLocks()
        {
            GameObject playerObject = new GameObject("TerminalCinematicPlayer");
            GameObject cameraObject = new GameObject("TerminalCinematicCamera");
            ActionCinematicCueProfile profile =
                ScriptableObject.CreateInstance<ActionCinematicCueProfile>();
            playerObject.SetActive(false);
            cameraObject.SetActive(false);
            try
            {
                PlayerMovementController movement =
                    playerObject.AddComponent<PlayerMovementController>();
                PlayerActionController action =
                    playerObject.AddComponent<PlayerActionController>();
                ActionCameraController camera =
                    cameraObject.AddComponent<ActionCameraController>();
                ActionCinematicCueDirector cinematic =
                    cameraObject.AddComponent<ActionCinematicCueDirector>();
                SetField(cinematic, "cueProfile", profile);
                SetField(cinematic, "cameraController", camera);
                SetField(cinematic, "cueSpace", playerObject.transform);
                SetField(cinematic, "movement", movement);
                SetField(cinematic, "actionController", action);

                playerObject.SetActive(true);
                cameraObject.SetActive(true);
                movement.SetCinematicMoveInputLocked(PlayerInputLockSource.EditorVerification, true);
                movement.SetCinematicMoveInputLocked(PlayerInputLockSource.BossTerminalAftermath, true);
                action.SetCinematicInputLocked(PlayerInputLockSource.EditorVerification, true);
                action.SetCinematicInputLocked(PlayerInputLockSource.BossTerminalAftermath, true);

                Assert.That(
                    cinematic.TryPlay(ActionCinematicCueProfile.CueKind.SummonEmpower, 2),
                    Is.True);
                Assert.That(cinematic.IsPlaying, Is.True);
                Assert.That(cinematic.HasOwnedTimeScaleLease, Is.True);
                Assert.That(Time.timeScale, Is.EqualTo(0.9f).Within(0.0001f));
                Assert.That(
                    movement.CinematicMoveInputLockSources.HasFlag(PlayerInputLockSource.CinematicCue),
                    Is.True);
                int firstShotVersion = camera.CueRequestVersion;
                Assert.That(firstShotVersion, Is.GreaterThan(0));

                Time.timeScale = 0.18f;
                Assert.That(cinematic.CancelForBossTerminalAftermath(), Is.True);
                Assert.That(cinematic.TerminalPlaybackSuppressed, Is.True);
                Assert.That(cinematic.TerminalCameraStreamSecured, Is.True);
                Assert.That(cinematic.LastBossTerminalStopRequestSucceeded, Is.True);
                Assert.That(cinematic.LastBossTerminalOwnedStateCleanupSucceeded, Is.True);
                Assert.That(cinematic.BossTerminalCancellationCount, Is.EqualTo(1));
                Assert.That(cinematic.LastBossTerminalCancellationStoppedActiveCue, Is.True);
                Assert.That(cinematic.IsPlaying, Is.False);
                Assert.That(cinematic.HasOwnedTimeScaleLease, Is.False);
                Assert.That(Time.timeScale, Is.EqualTo(0.18f).Within(0.0001f));
                Assert.That(
                    movement.CinematicMoveInputLockSources.HasFlag(PlayerInputLockSource.CinematicCue),
                    Is.False);
                Assert.That(
                    movement.CinematicMoveInputLockSources.HasFlag(
                        PlayerInputLockSource.BossTerminalAftermath),
                    Is.True);
                Assert.That(
                    movement.CinematicMoveInputLockSources.HasFlag(PlayerInputLockSource.EditorVerification),
                    Is.True);
                PlayerInputLockSource actionSources =
                    ReadField<PlayerInputLockSource>(action, "cinematicInputLockSources");
                Assert.That(actionSources.HasFlag(PlayerInputLockSource.CinematicCue), Is.False);
                Assert.That(actionSources.HasFlag(PlayerInputLockSource.BossTerminalAftermath), Is.True);
                Assert.That(actionSources.HasFlag(PlayerInputLockSource.EditorVerification), Is.True);

                Assert.That(
                    cinematic.TryPlay(ActionCinematicCueProfile.CueKind.PocketClear, 3),
                    Is.False);
                yield return new WaitForSecondsRealtime(0.75f);
                Assert.That(camera.CueRequestVersion, Is.EqualTo(firstShotVersion));
                Assert.That(cinematic.TotalPlayCount, Is.EqualTo(1));
            }
            finally
            {
                Time.timeScale = 1f;
                UnityEngine.Object.DestroyImmediate(cameraObject);
                UnityEngine.Object.DestroyImmediate(playerObject);
                UnityEngine.Object.DestroyImmediate(profile);
            }
        }

        [UnityTest]
        public IEnumerator NonTerminalPocketClearKeepsMultiShotPlaybackAndOwnedScaleRestore()
        {
            GameObject playerObject = new GameObject("PocketClearCinematicPlayer");
            GameObject cameraObject = new GameObject("PocketClearCinematicCamera");
            ActionCinematicCueProfile profile =
                ScriptableObject.CreateInstance<ActionCinematicCueProfile>();
            playerObject.SetActive(false);
            cameraObject.SetActive(false);
            try
            {
                PlayerMovementController movement =
                    playerObject.AddComponent<PlayerMovementController>();
                PlayerActionController action =
                    playerObject.AddComponent<PlayerActionController>();
                ActionCameraController camera =
                    cameraObject.AddComponent<ActionCameraController>();
                ActionCinematicCueDirector cinematic =
                    cameraObject.AddComponent<ActionCinematicCueDirector>();
                SetField(cinematic, "cueProfile", profile);
                SetField(cinematic, "cameraController", camera);
                SetField(cinematic, "cueSpace", playerObject.transform);
                SetField(cinematic, "movement", movement);
                SetField(cinematic, "actionController", action);

                playerObject.SetActive(true);
                cameraObject.SetActive(true);
                Assert.That(
                    cinematic.TryPlay(ActionCinematicCueProfile.CueKind.PocketClear, 3),
                    Is.True);
                Assert.That(cinematic.TerminalPlaybackSuppressed, Is.False);
                Assert.That(cinematic.IsPlaying, Is.True);
                Assert.That(cinematic.HasOwnedTimeScaleLease, Is.True);
                Assert.That(Time.timeScale, Is.EqualTo(0.86f).Within(0.0001f));
                int firstShotVersion = camera.CueRequestVersion;

                yield return new WaitForSecondsRealtime(0.35f);

                Assert.That(camera.CueRequestVersion, Is.GreaterThan(firstShotVersion));
                Assert.That(cinematic.IsPlaying, Is.True);
                Assert.That(cinematic.LastPlayedKind,
                    Is.EqualTo(ActionCinematicCueProfile.CueKind.PocketClear));
                Assert.That(cinematic.CancelForBossTerminalAftermath(), Is.True);
                Assert.That(Time.timeScale, Is.EqualTo(1f).Within(0.0001f));
            }
            finally
            {
                Time.timeScale = 1f;
                UnityEngine.Object.DestroyImmediate(cameraObject);
                UnityEngine.Object.DestroyImmediate(playerObject);
                UnityEngine.Object.DestroyImmediate(profile);
            }
        }

        [UnityTest]
        public IEnumerator TerminalSuppressionBlocksLateShotsWhenActiveCoroutineHandleIsLost()
        {
            GameObject cameraObject = new GameObject("LostHandleTerminalCinematicCamera");
            ActionCinematicCueProfile profile =
                ScriptableObject.CreateInstance<ActionCinematicCueProfile>();
            cameraObject.SetActive(false);
            try
            {
                ActionCameraController camera =
                    cameraObject.AddComponent<ActionCameraController>();
                ActionCinematicCueDirector cinematic =
                    cameraObject.AddComponent<ActionCinematicCueDirector>();
                SetField(cinematic, "cueProfile", profile);
                SetField(cinematic, "cameraController", camera);
                cameraObject.SetActive(true);

                Assert.That(
                    cinematic.TryPlay(ActionCinematicCueProfile.CueKind.SummonEmpower, 2),
                    Is.True);
                Coroutine runningRoutine = ReadField<Coroutine>(cinematic, "activeRoutine");
                Assert.That(runningRoutine, Is.Not.Null);
                int firstShotVersion = camera.CueRequestVersion;

                // Simulate the narrow failure mode where an in-flight iterator
                // survives but its explicit stop handle cannot be used.
                SetField<Coroutine>(cinematic, "activeRoutine", null);
                Assert.That(cinematic.CancelForBossTerminalAftermath(), Is.True);
                Assert.That(cinematic.TerminalCameraStreamSecured, Is.True);
                Assert.That(cinematic.LastBossTerminalCancellationStoppedActiveCue, Is.False);

                yield return new WaitForSecondsRealtime(0.75f);

                Assert.That(camera.CueRequestVersion, Is.EqualTo(firstShotVersion));
                Assert.That(cinematic.TerminalPlaybackSuppressed, Is.True);
            }
            finally
            {
                Time.timeScale = 1f;
                UnityEngine.Object.DestroyImmediate(cameraObject);
                UnityEngine.Object.DestroyImmediate(profile);
            }
        }

        [Test]
        public void EarlierDiedListenerCanDisableOwnerWithoutLeakingDelegateSnapshotLease()
        {
            GameObject playerObject = new GameObject("AftermathDisablePlayer");
            GameObject bossObject = new GameObject("AftermathDisableBoss");
            GameObject gateObject = new GameObject("AftermathDisabledGate");
            playerObject.SetActive(false);
            bossObject.SetActive(false);
            gateObject.SetActive(false);
            bool previousIgnore = LogAssert.ignoreFailingMessages;
            try
            {
                PlayerMovementController movement =
                    playerObject.AddComponent<PlayerMovementController>();
                PlayerActionController action =
                    playerObject.AddComponent<PlayerActionController>();
                PlayerSkill1Action skill1 = playerObject.AddComponent<PlayerSkill1Action>();
                PlayerSummonSlot1Action summon1 =
                    playerObject.AddComponent<PlayerSummonSlot1Action>();
                PlayerSupportSummonSlotAction summon2 =
                    playerObject.AddComponent<PlayerSupportSummonSlotAction>();
                PlayerSupportSummonSlotAction summon3 =
                    playerObject.AddComponent<PlayerSupportSummonSlotAction>();
                PlayerRangedBasicAttackAction ranged =
                    playerObject.AddComponent<PlayerRangedBasicAttackAction>();
                PlayerCombatModeController combatMode =
                    playerObject.AddComponent<PlayerCombatModeController>();
                CombatHealth bossHealth = bossObject.AddComponent<CombatHealth>();
                bossHealth.ConfigureTeam(DamageTeam.Enemy);
                OlympusStationBossTerminalAftermathPresenter aftermath =
                    gateObject.AddComponent<OlympusStationBossTerminalAftermathPresenter>();
                SetField(aftermath, "bossHealth", bossHealth);
                SetField(aftermath, "playerMovement", movement);
                SetField(aftermath, "playerActionController", action);
                SetField(aftermath, "playerSkill1Action", skill1);
                SetField(aftermath, "playerSummonSlot1Action", summon1);
                SetField(aftermath, "playerSummonSlot2Action", summon2);
                SetField(aftermath, "playerSummonSlot3Action", summon3);
                SetField(aftermath, "playerRangedBasicAttackAction", ranged);
                SetField(aftermath, "playerCombatModeController", combatMode);

                playerObject.SetActive(true);
                bossObject.SetActive(true);
                bossHealth.Died += () => gateObject.SetActive(false);
                gateObject.SetActive(true);
                LogAssert.ignoreFailingMessages = true;

                Assert.DoesNotThrow(() => bossHealth.TryApplyDamage(new DamageInfo(
                    null,
                    DamageTeam.Player,
                    bossHealth.MaxHealth + 1f,
                    bossHealth.transform.position,
                    Vector3.forward,
                    0f,
                    DamageResponsePolicy.DamageOnly,
                    CombatControlLockPolicy.None)));

                Assert.That(gateObject.activeSelf, Is.False);
                Assert.That(aftermath.IsStarted, Is.True);
                Assert.That(aftermath.IsCancelled, Is.True);
                Assert.That(aftermath.InputLeaseActive, Is.False);
                Assert.That(
                    movement.CinematicMoveInputLockSources.HasFlag(
                        PlayerInputLockSource.BossTerminalAftermath),
                    Is.False);
            }
            finally
            {
                LogAssert.ignoreFailingMessages = previousIgnore;
                UnityEngine.Object.DestroyImmediate(gateObject);
                UnityEngine.Object.DestroyImmediate(bossObject);
                UnityEngine.Object.DestroyImmediate(playerObject);
            }
        }

        [Test]
        public void BossDiedObserverSwallowsListenerFaultAndReleasesOnlyItsInputBit()
        {
            GameObject playerObject = new GameObject("AftermathFaultPlayer");
            GameObject bossObject = new GameObject("AftermathFaultBoss");
            GameObject gateObject = new GameObject("AftermathFaultGate");
            playerObject.SetActive(false);
            bossObject.SetActive(false);
            gateObject.SetActive(false);
            bool previousIgnore = LogAssert.ignoreFailingMessages;
            try
            {
                PlayerMovementController movement =
                    playerObject.AddComponent<PlayerMovementController>();
                PlayerActionController action =
                    playerObject.AddComponent<PlayerActionController>();
                PlayerSkill1Action skill1 = playerObject.AddComponent<PlayerSkill1Action>();
                PlayerSummonSlot1Action summon1 =
                    playerObject.AddComponent<PlayerSummonSlot1Action>();
                PlayerSupportSummonSlotAction summon2 =
                    playerObject.AddComponent<PlayerSupportSummonSlotAction>();
                PlayerSupportSummonSlotAction summon3 =
                    playerObject.AddComponent<PlayerSupportSummonSlotAction>();
                PlayerRangedBasicAttackAction ranged =
                    playerObject.AddComponent<PlayerRangedBasicAttackAction>();
                PlayerCombatModeController combatMode =
                    playerObject.AddComponent<PlayerCombatModeController>();
                CombatHealth bossHealth = bossObject.AddComponent<CombatHealth>();
                bossHealth.ConfigureTeam(DamageTeam.Enemy);

                OlympusStationBossTerminalAftermathPresenter aftermath =
                    gateObject.AddComponent<OlympusStationBossTerminalAftermathPresenter>();
                SetField(aftermath, "bossHealth", bossHealth);
                SetField(aftermath, "playerMovement", movement);
                SetField(aftermath, "playerActionController", action);
                SetField(aftermath, "playerSkill1Action", skill1);
                SetField(aftermath, "playerSummonSlot1Action", summon1);
                SetField(aftermath, "playerSummonSlot2Action", summon2);
                SetField(aftermath, "playerSummonSlot3Action", summon3);
                SetField(aftermath, "playerRangedBasicAttackAction", ranged);
                SetField(aftermath, "playerCombatModeController", combatMode);

                playerObject.SetActive(true);
                bossObject.SetActive(true);
                gateObject.SetActive(true);
                movement.SetCinematicMoveInputLocked(
                    PlayerInputLockSource.EditorVerification,
                    true);
                action.SetCinematicInputLocked(PlayerInputLockSource.EditorVerification, true);
                skill1.SetCinematicInputLocked(PlayerInputLockSource.EditorVerification, true);
                summon1.SetCinematicInputLocked(PlayerInputLockSource.EditorVerification, true);
                summon2.SetCinematicInputLocked(PlayerInputLockSource.EditorVerification, true);
                summon3.SetCinematicInputLocked(PlayerInputLockSource.EditorVerification, true);
                ranged.SetCinematicInputLocked(PlayerInputLockSource.EditorVerification, true);
                combatMode.SetCinematicInputLocked(PlayerInputLockSource.EditorVerification, true);
                aftermath.AftermathStarted += () =>
                    throw new InvalidOperationException("injected aftermath listener fault");
                LogAssert.ignoreFailingMessages = true;

                bool applied = false;
                Assert.DoesNotThrow(() =>
                {
                    applied = bossHealth.TryApplyDamage(new DamageInfo(
                        null,
                        DamageTeam.Player,
                        bossHealth.MaxHealth + 1f,
                        bossHealth.transform.position,
                        Vector3.forward,
                        0f,
                        DamageResponsePolicy.DamageOnly,
                        CombatControlLockPolicy.None));
                });

                Assert.That(applied, Is.True);
                Assert.That(bossHealth.IsAlive, Is.False);
                Assert.That(aftermath.IsStarted, Is.True);
                Assert.That(aftermath.BeginCount, Is.EqualTo(1));
                Assert.That(aftermath.InputLeaseFullyAcquired, Is.True);
                Assert.That(aftermath.LastQualityWarning, Does.Contain("listener failed safely"));
                Assert.That(
                    movement.CinematicMoveInputLockSources.HasFlag(
                        PlayerInputLockSource.BossTerminalAftermath),
                    Is.True);

                aftermath.CancelAndRelease("injected terminal result failure");

                Assert.That(aftermath.IsCancelled, Is.True);
                Assert.That(aftermath.InputLeaseActive, Is.False);
                Assert.That(
                    movement.CinematicMoveInputLockSources.HasFlag(
                        PlayerInputLockSource.BossTerminalAftermath),
                    Is.False);
                Assert.That(
                    movement.CinematicMoveInputLockSources.HasFlag(
                        PlayerInputLockSource.EditorVerification),
                    Is.True,
                    "Aftermath cleanup must not clear a foreign input-lock owner.");
                Assert.That(action.IsCinematicInputLocked, Is.True);
                Assert.That(skill1.IsCinematicInputLocked, Is.True);
                Assert.That(summon1.IsCinematicInputLocked, Is.True);
                Assert.That(summon2.IsCinematicInputLocked, Is.True);
                Assert.That(summon3.IsCinematicInputLocked, Is.True);
                Assert.That(ranged.IsCinematicInputLocked, Is.True);
                Assert.That(combatMode.IsCinematicInputLocked, Is.True);

                movement.SetCinematicMoveInputLocked(PlayerInputLockSource.EditorVerification, false);
                action.SetCinematicInputLocked(PlayerInputLockSource.EditorVerification, false);
                skill1.SetCinematicInputLocked(PlayerInputLockSource.EditorVerification, false);
                summon1.SetCinematicInputLocked(PlayerInputLockSource.EditorVerification, false);
                summon2.SetCinematicInputLocked(PlayerInputLockSource.EditorVerification, false);
                summon3.SetCinematicInputLocked(PlayerInputLockSource.EditorVerification, false);
                ranged.SetCinematicInputLocked(PlayerInputLockSource.EditorVerification, false);
                combatMode.SetCinematicInputLocked(PlayerInputLockSource.EditorVerification, false);
                Assert.That(movement.IsCinematicMoveInputLocked, Is.False);
                Assert.That(action.IsCinematicInputLocked, Is.False);
                Assert.That(skill1.IsCinematicInputLocked, Is.False);
                Assert.That(summon1.IsCinematicInputLocked, Is.False);
                Assert.That(summon2.IsCinematicInputLocked, Is.False);
                Assert.That(summon3.IsCinematicInputLocked, Is.False);
                Assert.That(ranged.IsCinematicInputLocked, Is.False);
                Assert.That(combatMode.IsCinematicInputLocked, Is.False);
            }
            finally
            {
                LogAssert.ignoreFailingMessages = previousIgnore;
                UnityEngine.Object.DestroyImmediate(gateObject);
                UnityEngine.Object.DestroyImmediate(bossObject);
                UnityEngine.Object.DestroyImmediate(playerObject);
            }
        }

        private static void SetField<T>(object target, string fieldName, T value)
        {
            FieldInfo field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"Missing test field {fieldName}.");
            field.SetValue(target, value);
        }

        private static T ReadField<T>(object target, string fieldName)
        {
            FieldInfo field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"Missing test field {fieldName}.");
            return (T)field.GetValue(target);
        }
    }
}
