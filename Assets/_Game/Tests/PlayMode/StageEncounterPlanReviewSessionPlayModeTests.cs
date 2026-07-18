using System;
using System.Reflection;
using DimensionBrawl.LevelDesign;
using NUnit.Framework;
using UnityEngine;

namespace DimensionBrawl.Tests
{
    public sealed class StageEncounterPlanReviewSessionPlayModeTests
    {
        private const string ProductNamespace =
            "DimensionBrawl.UI.ContentFactoryReview.";

        [Test]
        public void ThreeWaveDefeatAllFlowResolvesOneCombatantAndCompletesExactlyOnce()
        {
            using var fixture = new ProfileFixture();
            object session = CreateSession(fixture.Profile);

            AssertState(session, "Ready");
            Assert.That(ReadInt(session, "AttemptGeneration"), Is.EqualTo(1));
            Assert.That(InvokeBool(session, "TryBegin"), Is.True);
            AssertWave(session, 0, "wave.0", 2, "Active");

            Assert.That(TryGetNextSpawn(session, out object firstSpawn, out int firstRemaining), Is.True);
            Assert.That(ReadString(firstSpawn, "SpawnId"), Is.EqualTo("spawn.0"));
            Assert.That(firstRemaining, Is.EqualTo(2));
            Assert.That(InvokeBool(session, "TryResolveCombatant", "spawn.0"), Is.True);
            Assert.That(ReadInt(session, "CurrentRemainingCombatantCount"), Is.EqualTo(1));
            Assert.That(TryGetRemaining(session, "spawn.0", out int remaining), Is.True);
            Assert.That(remaining, Is.EqualTo(1));
            Assert.That(InvokeBool(session, "TryResolveCombatant", "spawn.0"), Is.True);
            AssertState(session, "WaveTransition");
            Assert.That(ReadInt(session, "ClearedWaveCount"), Is.EqualTo(1));
            Assert.That(Invoke(session, "GetWaveStatus", 0).ToString(), Is.EqualTo("Cleared"));

            Assert.That(InvokeBool(session, "TryAdvanceWave"), Is.True);
            AssertWave(session, 1, "wave.1", 1, "Active");
            Assert.That(InvokeBool(session, "TryResolveCombatant", "spawn.1"), Is.True);
            Assert.That(InvokeBool(session, "TryAdvanceWave"), Is.True);
            AssertWave(session, 2, "wave.2", 3, "Active");

            Assert.That(InvokeBool(session, "TryResolveCombatant", "spawn.2a"), Is.True);
            Assert.That(InvokeBool(session, "TryResolveCombatant", "spawn.2b"), Is.True);
            Assert.That(InvokeBool(session, "TryResolveCombatant", "spawn.2b"), Is.True);
            AssertState(session, "Completed");
            Assert.That(ReadBool(session, "IsCompleted"), Is.True);
            Assert.That(ReadInt(session, "ClearedWaveCount"), Is.EqualTo(3));
            Assert.That(ReadInt(session, "CompletionCount"), Is.EqualTo(1));
            Assert.That(InvokeBool(session, "TryResolveCombatant", "spawn.2b"), Is.False);
            Assert.That(InvokeBool(session, "TryAdvanceWave"), Is.False);
            Assert.That(InvokeBool(session, "TryInterrupt"), Is.False);
            Assert.That(ReadInt(session, "CompletionCount"), Is.EqualTo(1));
            AssertWaveStatuses(session, "Cleared", "Cleared", "Cleared");
        }

        [Test]
        public void InvalidTransitionsAndDuplicateResolutionFailClosed()
        {
            using var fixture = new ProfileFixture();
            object session = CreateSession(fixture.Profile);

            Assert.That(InvokeBool(session, "TryResolveCombatant", "spawn.0"), Is.False);
            Assert.That(InvokeBool(session, "TryAdvanceWave"), Is.False);
            Assert.That(InvokeBool(session, "TryInterrupt"), Is.False);
            Assert.That(InvokeBool(session, "TryBegin"), Is.True);
            Assert.That(InvokeBool(session, "TryBegin"), Is.False);
            Assert.That(InvokeBool(session, "TryAdvanceWave"), Is.False);
            Assert.That(InvokeBool(session, "TryResolveCombatant", "unknown.spawn"), Is.False);
            Assert.That(InvokeBool(session, "TryResolveCombatant", string.Empty), Is.False);
            Assert.That(InvokeBool(session, "TryResolveCombatant", "spawn.1"), Is.False);

            Assert.That(InvokeBool(session, "TryResolveCombatant", "spawn.0"), Is.True);
            Assert.That(InvokeBool(session, "TryAdvanceWave"), Is.False);
            Assert.That(InvokeBool(session, "TryResolveCombatant", "spawn.0"), Is.True);
            Assert.That(InvokeBool(session, "TryResolveCombatant", "spawn.0"), Is.False);
            Assert.That(InvokeBool(session, "TryBegin"), Is.False);
            Assert.That(InvokeBool(session, "TryAdvanceWave"), Is.True);
            Assert.That(ReadInt(session, "CurrentWaveIndex"), Is.EqualTo(1));
        }

        [Test]
        public void InterruptionIsExactOnceAndResetStartsANewAttemptGeneration()
        {
            using var fixture = new ProfileFixture();
            object session = CreateSession(fixture.Profile);

            Assert.That(InvokeBool(session, "TryBegin"), Is.True);
            Assert.That(InvokeBool(session, "TryResolveCombatant", "spawn.0"), Is.True);
            Assert.That(InvokeBool(session, "TryInterrupt"), Is.True);
            AssertState(session, "Interrupted");
            Assert.That(ReadBool(session, "IsInterrupted"), Is.True);
            Assert.That(ReadInt(session, "InterruptionCount"), Is.EqualTo(1));
            Assert.That(Invoke(session, "GetWaveStatus", 0).ToString(), Is.EqualTo("Interrupted"));
            Assert.That(InvokeBool(session, "TryInterrupt"), Is.False);
            Assert.That(InvokeBool(session, "TryResolveCombatant", "spawn.0"), Is.False);
            Assert.That(ReadInt(session, "InterruptionCount"), Is.EqualTo(1));

            Invoke(session, "Reset");
            AssertState(session, "Ready");
            Assert.That(ReadInt(session, "AttemptGeneration"), Is.EqualTo(2));
            Assert.That(ReadInt(session, "CurrentWaveIndex"), Is.EqualTo(-1));
            Assert.That(ReadInt(session, "CurrentRemainingCombatantCount"), Is.Zero);
            Assert.That(ReadInt(session, "ClearedWaveCount"), Is.Zero);
            Assert.That(ReadInt(session, "InterruptionCount"), Is.EqualTo(1));
            AssertWaveStatuses(session, "Pending", "Pending", "Pending");

            Assert.That(InvokeBool(session, "TryBegin"), Is.True);
            AssertWave(session, 0, "wave.0", 2, "Active");
            Invoke(session, "Reset");
            Assert.That(ReadInt(session, "AttemptGeneration"), Is.EqualTo(3));
            AssertState(session, "Ready");
        }

        [Test]
        public void ConfigureAndEveryProjectionAreDeepCopiedWhileDigestDetectsTamper()
        {
            StageEncounterPlanProfile.SpawnDefinition authoredSpawn =
                CreateSpawn("spawn.0", 2);
            StageEncounterPlanProfile.WaveDefinition[] authoredWaves =
                CreateValidWaves(authoredSpawn);
            var authoredEncounter = new StageEncounterPlanProfile.EncounterDefinition(
                "encounter.review",
                authoredWaves);
            StageEncounterPlanProfile profile = CreateProfile(authoredEncounter);
            try
            {
                string sealedDigest = profile.CanonicalDigest;
                Assert.That(sealedDigest, Has.Length.EqualTo(64));
                Assert.That(profile.TryValidate(out string initialError), Is.True, initialError);

                authoredSpawn.Configure("mutated.spawn", "mutated.payload", "mutated.anchor", 99, 9f);
                authoredWaves[0].Configure(
                    "mutated.wave",
                    9,
                    StageEncounterWaveActivation.PreviousWaveDefeated,
                    StageEncounterObjective.None,
                    Array.Empty<StageEncounterPlanProfile.SpawnDefinition>());
                authoredEncounter.Configure(
                    "mutated.encounter",
                    Array.Empty<StageEncounterPlanProfile.WaveDefinition>());

                Assert.That(profile.EncounterId, Is.EqualTo("encounter.review"));
                Assert.That(profile.GetWave(0).WaveId, Is.EqualTo("wave.0"));
                Assert.That(profile.GetWave(0).GetSpawn(0).Count, Is.EqualTo(2));
                Assert.That(profile.CanonicalDigest, Is.EqualTo(sealedDigest));

                StageEncounterPlanProfile.EncounterDefinition exposed = profile.Encounter;
                exposed.Configure(
                    "mutated.exposed",
                    Array.Empty<StageEncounterPlanProfile.WaveDefinition>());
                StageEncounterPlanProfile.WaveDefinition exposedWave = profile.GetWave(0);
                StageEncounterPlanProfile.SpawnDefinition exposedSpawn = exposedWave.GetSpawn(0);
                exposedSpawn.Configure("mutated.view", "payload", "anchor", 88, 0f);
                Assert.That(profile.EncounterId, Is.EqualTo("encounter.review"));
                Assert.That(profile.GetWave(0).GetSpawn(0).SpawnId, Is.EqualTo("spawn.0"));

                object session = CreateSession(profile);
                profile.Configure(
                    1,
                    2,
                    "plan.reconfigured",
                    "stage.reconfigured",
                    StageEncounterPlanAdmissionDisposition.ReviewOnlyNotAdmitted,
                    StageEncounterPlanOutcomeOwner.ExistingStageRun,
                    StageEncounterPlanRewardOwner.ExternalRewardLedger,
                    CreateValidEncounter());
                Assert.That(ReadString(session, "PlanId"), Is.EqualTo("plan.review"));
                Assert.That(ReadInt(session, "Revision"), Is.EqualTo(1));
                Assert.That(
                    ReadString(Invoke(session, "GetWave", 0), "WaveId"),
                    Is.EqualTo("wave.0"));

                FieldInfo digestField = typeof(StageEncounterPlanProfile).GetField(
                    "canonicalDigest",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.That(digestField, Is.Not.Null);
                digestField.SetValue(profile, new string('0', 64));
                Assert.That(profile.TryValidate(out string tamperError), Is.False);
                Assert.That(tamperError, Does.Contain("canonical digest does not match"));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(profile);
            }
        }

        [Test]
        public void ValidationRejectsIdentityOrderingOwnershipAndSpawnContractViolations()
        {
            AssertInvalid(
                "plan id",
                CreateValidEncounter(),
                planId: "bad plan id");
            AssertInvalid(
                "admission must be ReviewOnlyNotAdmitted",
                CreateValidEncounter(),
                admission: StageEncounterPlanAdmissionDisposition.None);
            AssertInvalid(
                "outcome owner must be ExistingStageRun",
                CreateValidEncounter(),
                outcomeOwner: StageEncounterPlanOutcomeOwner.None);
            AssertInvalid(
                "reward owner must be ExternalRewardLedger",
                CreateValidEncounter(),
                rewardOwner: StageEncounterPlanRewardOwner.None);
            AssertInvalid(
                "at least two waves",
                new StageEncounterPlanProfile.EncounterDefinition(
                    "encounter.review",
                    new[]
                    {
                        CreateWave(
                            "wave.0",
                            0,
                            StageEncounterWaveActivation.EncounterStart,
                            StageEncounterObjective.DefeatAll,
                            CreateSpawn("spawn.0", 1))
                    }));
            AssertInvalid(
                "indices must be contiguous",
                new StageEncounterPlanProfile.EncounterDefinition(
                    "encounter.review",
                    new[]
                    {
                        CreateWave(
                            "wave.0",
                            0,
                            StageEncounterWaveActivation.EncounterStart,
                            StageEncounterObjective.DefeatAll,
                            CreateSpawn("spawn.0", 1)),
                        CreateWave(
                            "wave.1",
                            2,
                            StageEncounterWaveActivation.EncounterStart,
                            StageEncounterObjective.None,
                            CreateSpawn("spawn.1", 1))
                    }));
            AssertInvalid(
                "spawn id 'spawn.duplicate' is duplicated",
                new StageEncounterPlanProfile.EncounterDefinition(
                    "encounter.review",
                    new[]
                    {
                        CreateWave(
                            "wave.duplicate",
                            0,
                            StageEncounterWaveActivation.EncounterStart,
                            StageEncounterObjective.DefeatAll,
                            CreateSpawn("spawn.duplicate", 1)),
                        CreateWave(
                            "wave.duplicate",
                            1,
                            StageEncounterWaveActivation.PreviousWaveDefeated,
                            StageEncounterObjective.DefeatAll,
                            CreateSpawn("spawn.duplicate", 1))
                    }));
            AssertInvalid(
                "count must be positive",
                new StageEncounterPlanProfile.EncounterDefinition(
                    "encounter.review",
                    new[]
                    {
                        CreateWave(
                            "wave.0",
                            0,
                            StageEncounterWaveActivation.EncounterStart,
                            StageEncounterObjective.DefeatAll,
                            CreateSpawn("spawn.0", 0)),
                        CreateWave(
                            "wave.1",
                            1,
                            StageEncounterWaveActivation.PreviousWaveDefeated,
                            StageEncounterObjective.DefeatAll,
                            new StageEncounterPlanProfile.SpawnDefinition(
                                "spawn.1",
                                "payload.1",
                                "anchor.1",
                                1,
                                float.PositiveInfinity))
                    }));
        }

        [Test]
        public void ValidationRejectsWaveCombatantTotalThatWouldOverflowSessionCounter()
        {
            var overflowEncounter = new StageEncounterPlanProfile.EncounterDefinition(
                "encounter.review",
                new[]
                {
                    CreateWave(
                        "wave.0",
                        0,
                        StageEncounterWaveActivation.EncounterStart,
                        StageEncounterObjective.DefeatAll,
                        CreateSpawn("spawn.max.0", int.MaxValue),
                        CreateSpawn("spawn.max.1", int.MaxValue)),
                    CreateWave(
                        "wave.1",
                        1,
                        StageEncounterWaveActivation.PreviousWaveDefeated,
                        StageEncounterObjective.DefeatAll,
                        CreateSpawn("spawn.1", 1))
                });

            AssertInvalid(
                $"total combatant count must not exceed {int.MaxValue}",
                overflowEncounter);
            Assert.Throws<OverflowException>(
                () => _ = overflowEncounter.GetWave(0).TotalCombatantCount);
        }

        private static void AssertWave(
            object session,
            int waveIndex,
            string waveId,
            int remainingCombatants,
            string expectedStatus)
        {
            AssertState(session, "WaveActive");
            Assert.That(ReadInt(session, "CurrentWaveIndex"), Is.EqualTo(waveIndex));
            Assert.That(ReadString(session, "CurrentWaveId"), Is.EqualTo(waveId));
            Assert.That(
                ReadInt(session, "CurrentRemainingCombatantCount"),
                Is.EqualTo(remainingCombatants));
            Assert.That(
                ReadProperty(session, "CurrentWaveStatus").ToString(),
                Is.EqualTo(expectedStatus));
        }

        private static Type SessionType => RequireProductType(
            ProductNamespace + "StageEncounterPlanReviewSession");

        private static object CreateSession(StageEncounterPlanProfile profile)
        {
            return Activator.CreateInstance(SessionType, new object[] { profile });
        }

        private static bool TryGetNextSpawn(
            object session,
            out object spawn,
            out int remainingCount)
        {
            object[] arguments = { null, 0 };
            bool found = (bool)RequireMethod(
                    session.GetType(),
                    "TryGetNextUnresolvedSpawn",
                    2)
                .Invoke(session, arguments);
            spawn = arguments[0];
            remainingCount = Convert.ToInt32(arguments[1]);
            return found;
        }

        private static bool TryGetRemaining(
            object session,
            string spawnId,
            out int remainingCount)
        {
            object[] arguments = { spawnId, 0 };
            bool found = (bool)RequireMethod(
                    session.GetType(),
                    "TryGetRemainingCombatantCount",
                    2)
                .Invoke(session, arguments);
            remainingCount = Convert.ToInt32(arguments[1]);
            return found;
        }

        private static void AssertState(object session, string expected)
        {
            Assert.That(ReadProperty(session, "State").ToString(), Is.EqualTo(expected));
        }

        private static void AssertWaveStatuses(object session, params string[] expected)
        {
            var statuses = (Array)Invoke(session, "CreateWaveStatusSnapshot");
            Assert.That(statuses.Length, Is.EqualTo(expected.Length));
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.That(statuses.GetValue(i).ToString(), Is.EqualTo(expected[i]));
            }
        }

        private static bool InvokeBool(
            object target,
            string methodName,
            params object[] arguments)
        {
            return (bool)Invoke(target, methodName, arguments);
        }

        private static object Invoke(
            object target,
            string methodName,
            params object[] arguments)
        {
            object[] safeArguments = arguments ?? Array.Empty<object>();
            return RequireMethod(target.GetType(), methodName, safeArguments.Length)
                .Invoke(target, safeArguments);
        }

        private static string ReadString(object target, string propertyName)
        {
            return ReadProperty(target, propertyName) as string ?? string.Empty;
        }

        private static int ReadInt(object target, string propertyName)
        {
            return Convert.ToInt32(ReadProperty(target, propertyName));
        }

        private static bool ReadBool(object target, string propertyName)
        {
            return Convert.ToBoolean(ReadProperty(target, propertyName));
        }

        private static object ReadProperty(object target, string propertyName)
        {
            Assert.That(target, Is.Not.Null);
            PropertyInfo property = target.GetType().GetProperty(
                propertyName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.That(property, Is.Not.Null, propertyName);
            return property.GetValue(target);
        }

        private static MethodInfo RequireMethod(
            Type type,
            string name,
            int parameterCount)
        {
            foreach (MethodInfo method in type.GetMethods(
                         BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (method.Name == name && method.GetParameters().Length == parameterCount)
                {
                    return method;
                }
            }

            Assert.Fail($"Missing {type.Name}.{name}/{parameterCount}.");
            return null;
        }

        private static Type RequireProductType(string fullName)
        {
            Type type = Type.GetType(fullName + ", Assembly-CSharp")
                ?? Type.GetType(fullName + ", DimensionBrawl.Runtime");
            Assert.That(type, Is.Not.Null, fullName);
            return type;
        }

        private static void AssertInvalid(
            string expectedError,
            StageEncounterPlanProfile.EncounterDefinition encounter,
            string planId = "plan.review",
            StageEncounterPlanAdmissionDisposition admission =
                StageEncounterPlanAdmissionDisposition.ReviewOnlyNotAdmitted,
            StageEncounterPlanOutcomeOwner outcomeOwner =
                StageEncounterPlanOutcomeOwner.ExistingStageRun,
            StageEncounterPlanRewardOwner rewardOwner =
                StageEncounterPlanRewardOwner.ExternalRewardLedger)
        {
            StageEncounterPlanProfile profile = ScriptableObject.CreateInstance<
                StageEncounterPlanProfile>();
            try
            {
                profile.Configure(
                    1,
                    1,
                    planId,
                    "stage.review",
                    admission,
                    outcomeOwner,
                    rewardOwner,
                    encounter);
                Assert.That(profile.TryValidate(out string error), Is.False);
                Assert.That(error, Does.Contain(expectedError));
                TargetInvocationException exception = Assert.Throws<
                    TargetInvocationException>(() => CreateSession(profile));
                Assert.That(exception.InnerException, Is.TypeOf<ArgumentException>());
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(profile);
            }
        }

        private static StageEncounterPlanProfile CreateProfile(
            StageEncounterPlanProfile.EncounterDefinition encounter)
        {
            StageEncounterPlanProfile profile = ScriptableObject.CreateInstance<
                StageEncounterPlanProfile>();
            profile.Configure(
                1,
                1,
                "plan.review",
                "stage.review",
                StageEncounterPlanAdmissionDisposition.ReviewOnlyNotAdmitted,
                StageEncounterPlanOutcomeOwner.ExistingStageRun,
                StageEncounterPlanRewardOwner.ExternalRewardLedger,
                encounter);
            return profile;
        }

        private static StageEncounterPlanProfile.EncounterDefinition CreateValidEncounter()
        {
            return new StageEncounterPlanProfile.EncounterDefinition(
                "encounter.review",
                CreateValidWaves(CreateSpawn("spawn.0", 2)));
        }

        private static StageEncounterPlanProfile.WaveDefinition[] CreateValidWaves(
            StageEncounterPlanProfile.SpawnDefinition firstSpawn)
        {
            return new[]
            {
                CreateWave(
                    "wave.0",
                    0,
                    StageEncounterWaveActivation.EncounterStart,
                    StageEncounterObjective.DefeatAll,
                    firstSpawn),
                CreateWave(
                    "wave.1",
                    1,
                    StageEncounterWaveActivation.PreviousWaveDefeated,
                    StageEncounterObjective.DefeatAll,
                    CreateSpawn("spawn.1", 1)),
                CreateWave(
                    "wave.2",
                    2,
                    StageEncounterWaveActivation.PreviousWaveDefeated,
                    StageEncounterObjective.DefeatAll,
                    CreateSpawn("spawn.2a", 1),
                    CreateSpawn("spawn.2b", 2))
            };
        }

        private static StageEncounterPlanProfile.WaveDefinition CreateWave(
            string waveId,
            int waveIndex,
            StageEncounterWaveActivation activation,
            StageEncounterObjective objective,
            params StageEncounterPlanProfile.SpawnDefinition[] spawns)
        {
            return new StageEncounterPlanProfile.WaveDefinition(
                waveId,
                waveIndex,
                activation,
                objective,
                spawns);
        }

        private static StageEncounterPlanProfile.SpawnDefinition CreateSpawn(
            string spawnId,
            int count)
        {
            return new StageEncounterPlanProfile.SpawnDefinition(
                spawnId,
                "payload." + spawnId,
                "anchor." + spawnId,
                count,
                0.25f);
        }

        private sealed class ProfileFixture : IDisposable
        {
            public ProfileFixture()
            {
                Profile = CreateProfile(CreateValidEncounter());
                Assert.That(Profile.TryValidate(out string error), Is.True, error);
            }

            public StageEncounterPlanProfile Profile { get; }

            public void Dispose()
            {
                UnityEngine.Object.DestroyImmediate(Profile);
            }
        }
    }
}
