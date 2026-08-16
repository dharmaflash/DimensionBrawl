using System;
using System.Collections;
using System.Reflection;
using DimensionBrawl.Combat;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.Player;
using DimensionBrawl.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.TestTools;

namespace DimensionBrawl.Tests
{
    public sealed class OlympusStationBossTerminalAftermathPlayModeTests
    {
        [Test]
        public void BossDeathMotionSettlesAtF116AfterFiftyFivePresentationClockSamples()
        {
            GameObject bossObject = new GameObject("ManualClockBossDeathMotion");
            try
            {
                AkazaPhase2CombatMotionDriver motion =
                    bossObject.AddComponent<AkazaPhase2CombatMotionDriver>();
                motion.Configure(
                    null,
                    null,
                    bossObject.transform,
                    Array.Empty<Transform>(),
                    null,
                    null);
                motion.PlayDeath();

                MethodInfo lateUpdate = typeof(AkazaPhase2CombatMotionDriver)
                    .GetMethod("LateUpdate", BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.That(lateUpdate, Is.Not.Null);

                using (PresentationClock.ManualLease lease =
                    PresentationClock.AcquireManual(this, 60))
                {
                    for (int frame = 1; frame <= 54; frame++)
                    {
                        lease.SetFrame(frame);
                        lateUpdate.Invoke(motion, null);
                        Assert.That(motion.DeathProgress01, Is.LessThan(1f),
                            $"frame={frame}");
                    }

                    Assert.That(motion.DeathProgress01,
                        Is.GreaterThan(0.99999f).And.LessThan(1f));
                    lease.SetFrame(55);
                    lateUpdate.Invoke(motion, null);
                    Assert.That(motion.DeathProgress01, Is.EqualTo(1f));
                }

                Assert.That(PresentationClock.IsManuallyDriven, Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(bossObject);
            }
        }

        [Test]
        public void AftermathUsesExactlyOneHundredFiftySixPresentationClockSamples()
        {
            GameObject gateObject = new GameObject("ManualClockAftermathGate");
            FinisherCameraFixture cameraFixture = null;
            OlympusStationBossTerminalAftermathPresenter aftermath = null;
            IEnumerator routine = null;
            int imminentCount = 0;
            int completedCount = 0;
            try
            {
                aftermath =
                    gateObject.AddComponent<OlympusStationBossTerminalAftermathPresenter>();
                cameraFixture = new FinisherCameraFixture();
                SetField(aftermath, "finisherCameraController", cameraFixture.Controller);
                Assert.That(
                    cameraFixture.Controller.TryAcquire(aftermath, out _),
                    Is.True,
                    cameraFixture.Controller.LastError);
                SetProperty(aftermath, nameof(aftermath.FinisherCameraSucceeded), true);
                aftermath.AftermathHandoffImminent += () => imminentCount++;
                aftermath.AftermathCompleted += () => completedCount++;
                SetField(aftermath, "started", true);
                SetField(aftermath, "aftermathDurationSeconds", 2.6f);
                MethodInfo method = typeof(OlympusStationBossTerminalAftermathPresenter)
                    .GetMethod("RunAftermath", BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.That(method, Is.Not.Null);
                routine = (IEnumerator)method.Invoke(aftermath, null);
                Assert.That(routine, Is.Not.Null);

                using (PresentationClock.ManualLease lease =
                    PresentationClock.AcquireManual(this, 60))
                {
                    lease.SetFrame(0);
                    Assert.That(routine.MoveNext(), Is.True);
                    Assert.That(aftermath.ElapsedUnscaledSeconds, Is.Zero);

                    for (int frame = 1; frame < 156; frame++)
                    {
                        lease.SetFrame(frame);
                        Assert.That(routine.MoveNext(), Is.True, $"frame={frame}");
                        Assert.That(aftermath.IsComplete, Is.False, $"frame={frame}");
                        Assert.That(imminentCount, Is.EqualTo(frame < 155 ? 0 : 1),
                            $"imminent frame={frame}");
                    }

                    Assert.That(aftermath.ElapsedUnscaledSeconds,
                        Is.EqualTo(155f / 60f).Within(0.00001f));
                    Assert.That(aftermath.IsHandoffImminent, Is.True);
                    Assert.That(aftermath.HandoffImminentCount, Is.EqualTo(1));
                    Assert.That(completedCount, Is.Zero);
                    lease.SetFrame(156);
                    Assert.That(routine.MoveNext(), Is.False);
                    Assert.That(aftermath.IsComplete, Is.True);
                    Assert.That(aftermath.CompleteCount, Is.EqualTo(1));
                    Assert.That(imminentCount, Is.EqualTo(1));
                    Assert.That(completedCount, Is.EqualTo(1));
                    Assert.That(aftermath.ElapsedUnscaledSeconds,
                        Is.EqualTo(2.6f).Within(0.00001f));
                }

                Assert.That(cameraFixture.Controller.SampleCount, Is.EqualTo(156));
                Assert.That(cameraFixture.Controller.HasReachedTerminalSample, Is.True);
                Assert.That(cameraFixture.Director.time, Is.EqualTo(2.6d).Within(0.0001d));
                Assert.That(PresentationClock.IsManuallyDriven, Is.False);
            }
            finally
            {
                (routine as IDisposable)?.Dispose();
                if (aftermath != null)
                {
                    aftermath.CancelAndRelease("manual aftermath test cleanup");
                }

                cameraFixture?.Dispose();
                UnityEngine.Object.DestroyImmediate(gateObject);
            }
        }

        [Test]
        public void FinisherCameraHardAcquireEvaluatesTimelineBeforeExclusiveSwitch()
        {
            using (FinisherCameraFixture fixture = new FinisherCameraFixture())
            {
                Assert.That(
                    fixture.FinisherCamera.GetComponentsInChildren<AudioListener>(true),
                    Is.Empty);
                Assert.That(
                    fixture.Timeline.duration,
                    Is.EqualTo(
                        OlympusStationBossTerminalFinisherCameraController
                            .RequiredTimelineDurationSeconds)
                        .Within(0.0001d));
                Assert.That(fixture.Controller.ValidateConfiguration(out string error),
                    Is.True,
                    error);

                Assert.That(
                    fixture.Controller.TryAcquire(fixture.Owner, out int version),
                    Is.True,
                    fixture.Controller.LastError);
                Assert.That(version, Is.EqualTo(1));
                Assert.That(fixture.Controller.RequestVersion, Is.EqualTo(1));
                Assert.That(fixture.Controller.AcquireCount, Is.EqualTo(1));
                Assert.That(fixture.Director.timeUpdateMode,
                    Is.EqualTo(DirectorUpdateMode.Manual));
                Assert.That(fixture.Director.time, Is.EqualTo(0d).Within(0.0001d));
                Assert.That(fixture.GameplayCamera.enabled, Is.False);
                Assert.That(fixture.FinisherCamera.enabled, Is.True);
                Assert.That(fixture.Controller.ActiveCamera,
                    Is.SameAs(fixture.FinisherCamera));

                Assert.That(
                    fixture.Controller.TryAcquire(
                        fixture.Owner,
                        out int idempotentVersion),
                    Is.True);
                Assert.That(idempotentVersion, Is.EqualTo(version));
                Assert.That(fixture.Controller.AcquireCount, Is.EqualTo(1));

                Assert.That(
                    fixture.Controller.TryAcquire(fixture.ForeignOwner, out int foreignVersion),
                    Is.False);
                Assert.That(foreignVersion, Is.EqualTo(version));
                Assert.That(fixture.Controller.IsOwnedBy(fixture.Owner), Is.True);
                Assert.That(fixture.Controller.IsOwnedBy(fixture.ForeignOwner), Is.False);
                Assert.That(fixture.GameplayCamera.enabled, Is.False);
                Assert.That(fixture.FinisherCamera.enabled, Is.True);

                Assert.That(
                    fixture.Controller.CancelAndRestore(fixture.Owner, "test cancellation"),
                    Is.True);
                Assert.That(fixture.Controller.IsLeaseActive, Is.False);
                Assert.That(fixture.Controller.ReleaseCount, Is.EqualTo(1));
                Assert.That(fixture.Controller.WasInterrupted, Is.True);
                Assert.That(fixture.Controller.LastError, Does.Contain("test cancellation"));
                Assert.That(fixture.GameplayCamera.enabled, Is.True);
                Assert.That(fixture.FinisherCamera.enabled, Is.False);
                Assert.That(fixture.Director.timeUpdateMode,
                    Is.EqualTo(DirectorUpdateMode.GameTime));
                Assert.That(fixture.Director.time, Is.Zero.Within(0.0001d));
            }
        }

        [Test]
        public void FinisherCameraRejectsAnyDedicatedCameraAudioListener()
        {
            using (FinisherCameraFixture fixture = new FinisherCameraFixture())
            {
                AudioListener forbiddenListener =
                    fixture.FinisherCamera.gameObject.AddComponent<AudioListener>();
                Assert.That(
                    fixture.Controller.TryAcquire(fixture.Owner, out _),
                    Is.False);
                Assert.That(fixture.Controller.LastError, Does.Contain("AudioListener"));
                Assert.That(fixture.Controller.AcquireCount, Is.Zero);
                Assert.That(fixture.GameplayCamera.enabled, Is.True);
                Assert.That(fixture.FinisherCamera.enabled, Is.False);

                UnityEngine.Object.DestroyImmediate(forbiddenListener);
                Assert.That(
                    fixture.Controller.TryAcquire(fixture.Owner, out _),
                    Is.True,
                    fixture.Controller.LastError);
            }
        }

        [Test]
        public void FinisherTimelineManualSamplingIgnoresPointOneEightTimeScale()
        {
            float previousTimeScale = Time.timeScale;
            using (FinisherCameraFixture fixture = new FinisherCameraFixture())
            {
                try
                {
                    Time.timeScale = 0.18f;
                    Assert.That(
                        fixture.Controller.TryAcquire(fixture.Owner, out _),
                        Is.True,
                        fixture.Controller.LastError);

                    using (PresentationClock.ManualLease lease =
                        PresentationClock.AcquireManual(this, 60))
                    {
                        for (int frame = 1; frame <= 156; frame++)
                        {
                            lease.SetFrame(frame);
                            Assert.That(
                                fixture.Controller.Sample(
                                    fixture.Owner,
                                    PresentationClock.UnscaledTime),
                                Is.True,
                                $"frame={frame}; {fixture.Controller.LastError}");
                        }
                    }

                    Assert.That(fixture.Controller.SampleCount, Is.EqualTo(156));
                    Assert.That(fixture.Controller.HasReachedTerminalSample, Is.True);
                    Assert.That(
                        fixture.Controller.LastSampledSeconds,
                        Is.EqualTo(2.6d).Within(0.0001d));
                    Assert.That(fixture.Director.time, Is.EqualTo(2.6d).Within(0.0001d));
                    Assert.That(Time.timeScale, Is.EqualTo(0.18f).Within(0.0001f));
                    Assert.That(PresentationClock.IsManuallyDriven, Is.False);
                }
                finally
                {
                    Time.timeScale = previousTimeScale;
                }
            }
        }

        [Test]
        public void ResultCoverReleaseRestoresOnTwentyEighthPresentationClockSample()
        {
            using (FinisherCameraFixture fixture = new FinisherCameraFixture())
            {
                Assert.That(
                    fixture.Controller.TryAcquire(fixture.Owner, out _),
                    Is.True,
                    fixture.Controller.LastError);
                Assert.That(fixture.Controller.Sample(fixture.Owner, 2.6f), Is.True);
                Assert.That(
                    fixture.Controller.ScheduleReleaseAfterResultCover(fixture.Owner),
                    Is.True,
                    fixture.Controller.LastError);

                MethodInfo update =
                    typeof(OlympusStationBossTerminalFinisherCameraController).GetMethod(
                        "Update",
                        BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.That(update, Is.Not.Null);
                using (PresentationClock.ManualLease lease =
                    PresentationClock.AcquireManual(this, 60))
                {
                    for (int frame = 1; frame <= 27; frame++)
                    {
                        lease.SetFrame(frame);
                        update.Invoke(fixture.Controller, null);
                        Assert.That(fixture.Controller.IsLeaseActive, Is.True,
                            $"frame={frame}");
                    }

                    lease.SetFrame(28);
                    update.Invoke(fixture.Controller, null);
                }

                Assert.That(fixture.Controller.IsLeaseActive, Is.False);
                Assert.That(fixture.Controller.ResultCoverReleaseSampleCount, Is.EqualTo(28));
                Assert.That(
                    fixture.Controller.ResultCoverReleaseElapsedSeconds,
                    Is.EqualTo(28f / 60f).Within(0.0001f));
                Assert.That(fixture.Controller.ReleaseCount, Is.EqualTo(1));
                Assert.That(fixture.Controller.WasInterrupted, Is.False);
                Assert.That(fixture.Controller.LastError, Is.Empty);
                Assert.That(fixture.GameplayCamera.enabled, Is.True);
                Assert.That(fixture.FinisherCamera.enabled, Is.False);
                Assert.That(fixture.Controller.ActiveCamera,
                    Is.SameAs(fixture.GameplayCamera));
                Assert.That(fixture.Director.timeUpdateMode,
                    Is.EqualTo(DirectorUpdateMode.GameTime));
                Assert.That(fixture.Director.time, Is.Zero.Within(0.0001d));
                Assert.That(PresentationClock.IsManuallyDriven, Is.False);
            }
        }

        [Test]
        public void CancelAndDisableRestoreOnlyStillOwnedCameraAndDirectorValues()
        {
            using (FinisherCameraFixture fixture = new FinisherCameraFixture())
            {
                Assert.That(
                    fixture.Controller.TryAcquire(fixture.Owner, out _),
                    Is.True,
                    fixture.Controller.LastError);

                fixture.GameplayCamera.enabled = true;
                fixture.FinisherCamera.enabled = false;
                fixture.Director.time = 1.25d;
                fixture.Director.timeUpdateMode = DirectorUpdateMode.UnscaledGameTime;
                Assert.That(
                    fixture.Controller.CancelAndRestore(fixture.Owner, "value-owned cancel"),
                    Is.True);
                Assert.That(fixture.GameplayCamera.enabled, Is.True);
                Assert.That(fixture.FinisherCamera.enabled, Is.False);
                Assert.That(fixture.Director.time, Is.EqualTo(1.25d).Within(0.0001d));
                Assert.That(fixture.Director.timeUpdateMode,
                    Is.EqualTo(DirectorUpdateMode.UnscaledGameTime));

                Assert.That(
                    fixture.Controller.TryAcquire(fixture.Owner, out int secondVersion),
                    Is.True,
                    fixture.Controller.LastError);
                Assert.That(secondVersion, Is.EqualTo(2));
                Assert.That(fixture.Director.time, Is.Zero.Within(0.0001d));
                fixture.Controller.enabled = false;

                Assert.That(fixture.Controller.IsLeaseActive, Is.False);
                Assert.That(fixture.Controller.ReleaseCount, Is.EqualTo(2));
                Assert.That(fixture.Controller.WasInterrupted, Is.True);
                Assert.That(fixture.GameplayCamera.enabled, Is.True);
                Assert.That(fixture.FinisherCamera.enabled, Is.False);
                Assert.That(fixture.Director.time, Is.EqualTo(1.25d).Within(0.0001d));
                Assert.That(fixture.Director.timeUpdateMode,
                    Is.EqualTo(DirectorUpdateMode.UnscaledGameTime));
            }
        }

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
            GameObject terminalBoundaryVisual =
                new GameObject("AftermathFaultTerminalBoundaryVisual");
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
                SetField(
                    aftermath,
                    "terminalBoundaryVisualRoot",
                    terminalBoundaryVisual);

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
                Assert.That(aftermath.TerminalBoundaryVisualHidden, Is.True);
                Assert.That(terminalBoundaryVisual.activeSelf, Is.False);
                Assert.That(aftermath.LastQualityWarning, Does.Contain("listener failed safely"));
                Assert.That(
                    movement.CinematicMoveInputLockSources.HasFlag(
                        PlayerInputLockSource.BossTerminalAftermath),
                    Is.True);

                aftermath.CancelAndRelease("injected terminal result failure");

                Assert.That(aftermath.IsCancelled, Is.True);
                Assert.That(aftermath.InputLeaseActive, Is.False);
                Assert.That(aftermath.TerminalBoundaryVisualHidden, Is.False);
                Assert.That(
                    terminalBoundaryVisual.activeSelf,
                    Is.True,
                    "A failed result handoff must restore the still-owned gameplay boundary visual.");
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
                UnityEngine.Object.DestroyImmediate(terminalBoundaryVisual);
            }
        }

        private sealed class FinisherCameraFixture : IDisposable
        {
            private readonly GameObject controllerObject;
            private readonly GameObject gameplayCameraObject;
            private readonly GameObject finisherCameraObject;
            private readonly GameObject timelineBindingObject;
            private readonly UnityEngine.Object timelineTrack;
            private readonly UnityEngine.Object timelineClipAsset;

            public FinisherCameraFixture()
            {
                controllerObject = new GameObject("RuntimeStationFinisherController");
                gameplayCameraObject = new GameObject("RuntimeStationGameplayCamera");
                finisherCameraObject = new GameObject("RuntimeStationFinisherCamera");
                timelineBindingObject = new GameObject("RuntimeStationFinisherTimelineBinding");
                Owner = new GameObject("RuntimeStationFinisherOwner");
                ForeignOwner = new GameObject("RuntimeStationFinisherForeignOwner");

                GameplayCamera = gameplayCameraObject.AddComponent<Camera>();
                FinisherCamera = finisherCameraObject.AddComponent<Camera>();
                GameplayCamera.enabled = true;
                FinisherCamera.enabled = false;

                Director = controllerObject.AddComponent<PlayableDirector>();
                Director.playOnAwake = false;
                Director.timeUpdateMode = DirectorUpdateMode.GameTime;
                Timeline = CreateRuntimeTimeline(
                    out timelineTrack,
                    out timelineClipAsset);
                Director.playableAsset = Timeline;
                Director.SetGenericBinding(timelineTrack, timelineBindingObject);

                Controller = controllerObject.AddComponent<
                    OlympusStationBossTerminalFinisherCameraController>();
                SetField(Controller, "finisherDirector", Director);
                SetField(Controller, "finisherTimeline", Timeline);
                SetField(Controller, "gameplayCamera", GameplayCamera);
                SetField(Controller, "finisherCamera", FinisherCamera);
                SetField(
                    Controller,
                    "resultCoverReleaseSeconds",
                    OlympusStationBossTerminalFinisherCameraController
                        .RequiredResultCoverReleaseSeconds);
            }

            public OlympusStationBossTerminalFinisherCameraController Controller { get; }
            public PlayableDirector Director { get; }
            public PlayableAsset Timeline { get; }
            public Camera GameplayCamera { get; }
            public Camera FinisherCamera { get; }
            public GameObject Owner { get; }
            public GameObject ForeignOwner { get; }

            public void Dispose()
            {
                if (Controller != null
                    && Owner != null
                    && Controller.IsOwnedBy(Owner))
                {
                    Controller.CancelAndRestore(Owner, "test fixture disposal");
                }

                UnityEngine.Object.DestroyImmediate(controllerObject);
                UnityEngine.Object.DestroyImmediate(gameplayCameraObject);
                UnityEngine.Object.DestroyImmediate(finisherCameraObject);
                UnityEngine.Object.DestroyImmediate(timelineBindingObject);
                UnityEngine.Object.DestroyImmediate(Owner);
                UnityEngine.Object.DestroyImmediate(ForeignOwner);
                if (timelineClipAsset != null)
                {
                    UnityEngine.Object.DestroyImmediate(timelineClipAsset);
                }

                if (timelineTrack != null)
                {
                    UnityEngine.Object.DestroyImmediate(timelineTrack);
                }

                UnityEngine.Object.DestroyImmediate(Timeline);
            }

            private static PlayableAsset CreateRuntimeTimeline(
                out UnityEngine.Object track,
                out UnityEngine.Object clipAsset)
            {
                Type timelineType = Type.GetType(
                    "UnityEngine.Timeline.TimelineAsset, Unity.Timeline",
                    throwOnError: true);
                Type activationTrackType = Type.GetType(
                    "UnityEngine.Timeline.ActivationTrack, Unity.Timeline",
                    throwOnError: true);
                ScriptableObject timelineObject =
                    ScriptableObject.CreateInstance(timelineType);
                timelineObject.name = "RuntimeStationBossTerminalFinisherTimeline";

                MethodInfo createTrack = null;
                MethodInfo[] timelineMethods = timelineType.GetMethods(
                    BindingFlags.Instance | BindingFlags.Public);
                for (int index = 0; index < timelineMethods.Length; index++)
                {
                    MethodInfo candidate = timelineMethods[index];
                    ParameterInfo[] parameters = candidate.GetParameters();
                    if (candidate.Name == "CreateTrack"
                        && !candidate.IsGenericMethod
                        && parameters.Length == 3
                        && parameters[0].ParameterType == typeof(Type))
                    {
                        createTrack = candidate;
                        break;
                    }
                }

                Assert.That(createTrack, Is.Not.Null);
                object trackObject = createTrack.Invoke(
                    timelineObject,
                    new object[]
                    {
                        activationTrackType,
                        null,
                        "RuntimeFinisherCameraActivationTrack",
                    });
                Assert.That(trackObject, Is.Not.Null);
                track = trackObject as UnityEngine.Object;
                Assert.That(track, Is.Not.Null);

                MethodInfo createDefaultClip = trackObject.GetType().GetMethod(
                    "CreateDefaultClip",
                    BindingFlags.Instance | BindingFlags.Public);
                Assert.That(createDefaultClip, Is.Not.Null);
                object timelineClip = createDefaultClip.Invoke(trackObject, null);
                Assert.That(timelineClip, Is.Not.Null);
                PropertyInfo startProperty = timelineClip.GetType().GetProperty("start");
                PropertyInfo durationProperty = timelineClip.GetType().GetProperty("duration");
                PropertyInfo assetProperty = timelineClip.GetType().GetProperty("asset");
                Assert.That(startProperty, Is.Not.Null);
                Assert.That(durationProperty, Is.Not.Null);
                Assert.That(assetProperty, Is.Not.Null);
                startProperty.SetValue(timelineClip, 0d);
                durationProperty.SetValue(
                    timelineClip,
                    OlympusStationBossTerminalFinisherCameraController
                        .RequiredTimelineDurationSeconds);
                clipAsset = assetProperty.GetValue(timelineClip) as UnityEngine.Object;
                Assert.That(clipAsset, Is.Not.Null);
                return (PlayableAsset)timelineObject;
            }
        }

        private static void SetProperty<T>(object target, string propertyName, T value)
        {
            PropertyInfo property = target.GetType().GetProperty(
                propertyName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.That(property, Is.Not.Null, $"Missing test property {propertyName}.");
            MethodInfo setter = property.GetSetMethod(nonPublic: true);
            Assert.That(setter, Is.Not.Null, $"Missing test setter {propertyName}.");
            setter.Invoke(target, new object[] { value });
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
