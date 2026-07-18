using System;
using System.Collections;
using System.Reflection;
using DimensionBrawl.LevelDesign;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace DimensionBrawl.Tests
{
    public sealed class ContentFactoryEncounterPlanReviewControllerPlayModeTests
    {
        private const string ControllerTypeName =
            "DimensionBrawl.UI.ContentFactoryReview.ContentFactoryEncounterPlanReviewController";

        [UnityTest]
        public IEnumerator InitialViewRendersThreePendingWavesAndExplicitOwnershipBoundary()
        {
            using var fixture = new ControllerFixture();
            fixture.Activate();
            yield return null;

            Assert.That(ReadProperty(fixture.Controller, "Session"), Is.Not.Null);
            AssertState(fixture, "Ready");
            Assert.That(
                ReadProperty<bool>(fixture.Controller, "HasExactWaveCardArrays"),
                Is.True);
            Assert.That(
                ReadText(fixture.AdmissionBoundary),
                Does.Contain("ReviewOnlyNotAdmitted"));
            Assert.That(ReadText(fixture.Identity), Does.Contain("review.cf01.plan.local"));
            Assert.That(ReadText(fixture.Objective), Does.Contain("DefeatAll"));
            Assert.That(
                ReadText(fixture.OwnershipBoundary),
                Does.Contain("ExistingStageRun"));
            Assert.That(
                ReadText(fixture.OwnershipBoundary),
                Does.Contain("ExternalRewardLedger"));
            Assert.That(
                ReadText(fixture.OwnershipBoundary),
                Does.Contain("NO STAGE ADMISSION"));
            AssertWaveStates(fixture, "PENDING", "PENDING", "PENDING");
            AssertButtons(fixture, true, false, false, false, false);
        }

        [UnityTest]
        public IEnumerator ButtonsDriveTruthfulThreeWaveProgressionToLocalCompletion()
        {
            using var fixture = new ControllerFixture();
            fixture.Activate();
            yield return null;

            fixture.BeginButton.onClick.Invoke();
            AssertState(fixture, "WaveActive");
            AssertWaveStates(fixture, "ACTIVE", "PENDING", "PENDING");
            Assert.That(ReadText(fixture.CurrentSpawn), Does.Contain("wave-01.primary"));
            Assert.That(ReadText(fixture.CurrentSpawn), Does.Contain("REMAINING 02"));
            AssertButtons(fixture, false, true, false, true, true);

            fixture.ResolveButton.onClick.Invoke();
            AssertState(fixture, "WaveActive");
            Assert.That(ReadText(fixture.CurrentSpawn), Does.Contain("REMAINING 01"));

            fixture.ResolveButton.onClick.Invoke();
            AssertState(fixture, "WaveTransition");
            AssertWaveStates(fixture, "CLEARED", "PENDING", "PENDING");
            Assert.That(ReadText(fixture.CurrentSpawn), Does.Contain("ADVANCE AVAILABLE"));
            AssertButtons(fixture, false, false, true, true, true);

            fixture.AdvanceButton.onClick.Invoke();
            AssertState(fixture, "WaveActive");
            AssertWaveStates(fixture, "CLEARED", "ACTIVE", "PENDING");
            Assert.That(ReadText(fixture.CurrentSpawn), Does.Contain("wave-02.primary"));

            fixture.ResolveButton.onClick.Invoke();
            AssertState(fixture, "WaveActive");
            Assert.That(ReadText(fixture.CurrentSpawn), Does.Contain("wave-02.support"));
            fixture.ResolveButton.onClick.Invoke();
            AssertState(fixture, "WaveTransition");

            fixture.AdvanceButton.onClick.Invoke();
            AssertState(fixture, "WaveActive");
            AssertWaveStates(fixture, "CLEARED", "CLEARED", "ACTIVE");
            fixture.ResolveButton.onClick.Invoke();

            AssertState(fixture, "Completed");
            AssertWaveStates(fixture, "CLEARED", "CLEARED", "CLEARED");
            Assert.That(ReadText(fixture.Progress), Does.Contain("WAVES 03 / 03"));
            Assert.That(ReadText(fixture.CurrentSpawn), Does.Contain("REVIEW COMPLETE"));
            Assert.That(ReadSessionInt(fixture, "CompletionCount"), Is.EqualTo(1));
            AssertButtons(fixture, false, false, false, false, true);
        }

        [UnityTest]
        public IEnumerator InterruptResetAndReenableKeepOneListenerAndOneSessionAttempt()
        {
            using var fixture = new ControllerFixture();
            fixture.Activate();
            yield return null;

            fixture.BeginButton.onClick.Invoke();
            AssertState(fixture, "WaveActive");
            Assert.That(ReadSessionInt(fixture, "AttemptGeneration"), Is.EqualTo(1));

            fixture.Root.SetActive(false);
            yield return null;
            fixture.Root.SetActive(true);
            yield return null;
            fixture.Root.SetActive(false);
            yield return null;
            fixture.Root.SetActive(true);
            yield return null;

            fixture.ResolveButton.onClick.Invoke();
            AssertState(fixture, "WaveActive");
            Assert.That(ReadSessionInt(fixture, "CurrentRemainingCombatantCount"), Is.EqualTo(1));

            fixture.InterruptButton.onClick.Invoke();
            AssertState(fixture, "Interrupted");
            AssertWaveStates(fixture, "INTERRUPTED", "PENDING", "PENDING");
            Assert.That(ReadSessionInt(fixture, "InterruptionCount"), Is.EqualTo(1));
            AssertButtons(fixture, false, false, false, false, true);

            fixture.InterruptButton.onClick.Invoke();
            Assert.That(ReadSessionInt(fixture, "InterruptionCount"), Is.EqualTo(1));

            fixture.ResetButton.onClick.Invoke();
            AssertState(fixture, "Ready");
            AssertWaveStates(fixture, "PENDING", "PENDING", "PENDING");
            Assert.That(ReadSessionInt(fixture, "AttemptGeneration"), Is.EqualTo(2));
            AssertButtons(fixture, true, false, false, false, false);

            fixture.ResetButton.onClick.Invoke();
            Assert.That(ReadSessionInt(fixture, "AttemptGeneration"), Is.EqualTo(2));
        }

        [UnityTest]
        public IEnumerator InvalidProfileDoesNotCreateSessionOrEnableActions()
        {
            using var fixture = new ControllerFixture(createValidProfile: false);
            fixture.Activate();
            yield return null;

            Assert.That(ReadProperty(fixture.Controller, "Session"), Is.Null);
            Assert.That(
                ReadProperty<string>(fixture.Controller, "ProfileValidationError"),
                Is.Not.Empty);
            Assert.That(
                ReadText(fixture.AdmissionBoundary),
                Does.Contain("REVIEW UNAVAILABLE"));
            AssertWaveStates(fixture, "UNAVAILABLE", "UNAVAILABLE", "UNAVAILABLE");
            AssertButtons(fixture, false, false, false, false, false);
        }

        private static void AssertState(ControllerFixture fixture, string expected)
        {
            Assert.That(
                ReadProperty(fixture.Controller, "CurrentState").ToString(),
                Is.EqualTo(expected));
            Assert.That(ReadText(fixture.State), Does.Contain(expected));
        }

        private static void AssertWaveStates(
            ControllerFixture fixture,
            string first,
            string second,
            string third)
        {
            string[] expected = { first, second, third };
            for (int index = 0; index < expected.Length; index++)
            {
                Assert.That(
                    ReadText(fixture.WaveStates[index]),
                    Is.EqualTo(expected[index]),
                    $"Wave {index + 1} status");
            }
        }

        private static void AssertButtons(
            ControllerFixture fixture,
            bool begin,
            bool resolve,
            bool advance,
            bool interrupt,
            bool reset)
        {
            Assert.That(fixture.BeginButton.interactable, Is.EqualTo(begin));
            Assert.That(fixture.ResolveButton.interactable, Is.EqualTo(resolve));
            Assert.That(fixture.AdvanceButton.interactable, Is.EqualTo(advance));
            Assert.That(fixture.InterruptButton.interactable, Is.EqualTo(interrupt));
            Assert.That(fixture.ResetButton.interactable, Is.EqualTo(reset));
        }

        private static int ReadSessionInt(ControllerFixture fixture, string propertyName)
        {
            object session = ReadProperty(fixture.Controller, "Session");
            Assert.That(session, Is.Not.Null);
            return ReadProperty<int>(session, propertyName);
        }

        private static object Invoke(object target, string methodName, params object[] arguments)
        {
            object[] safeArguments = arguments ?? Array.Empty<object>();
            return RequireMethod(target.GetType(), methodName, safeArguments.Length)
                .Invoke(target, safeArguments);
        }

        private static MethodInfo RequireMethod(Type type, string methodName, int parameterCount)
        {
            MethodInfo match = null;
            foreach (MethodInfo method in type.GetMethods(
                         BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (method.Name != methodName
                    || method.GetParameters().Length != parameterCount)
                {
                    continue;
                }

                Assert.That(match, Is.Null, $"Ambiguous {type.Name}.{methodName}.");
                match = method;
            }

            Assert.That(match, Is.Not.Null, $"Missing {type.Name}.{methodName}.");
            return match;
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

        private static T ReadProperty<T>(object target, string propertyName)
        {
            return (T)ReadProperty(target, propertyName);
        }

        private static string ReadText(Component text)
        {
            return text != null
                ? ReadProperty<string>(text, "text") ?? string.Empty
                : string.Empty;
        }

        private static Type RequireProductType(string fullName)
        {
            Type type = Type.GetType(fullName + ", Assembly-CSharp");
            Assert.That(type, Is.Not.Null, fullName);
            return type;
        }

        private sealed class ControllerFixture : IDisposable
        {
            public ControllerFixture(bool createValidProfile = true)
            {
                Root = new GameObject("CF01ControllerFixture", typeof(RectTransform));
                Root.SetActive(false);
                Controller = Root.AddComponent(RequireProductType(ControllerTypeName));
                Profile = ScriptableObject.CreateInstance<StageEncounterPlanProfile>();
                if (createValidProfile)
                {
                    ConfigureValidProfile(Profile);
                }

                AdmissionBoundary = CreateText(Root.transform, "AdmissionBoundary");
                Title = CreateText(Root.transform, "Title");
                Identity = CreateText(Root.transform, "Identity");
                Objective = CreateText(Root.transform, "Objective");
                State = CreateText(Root.transform, "State");
                Progress = CreateText(Root.transform, "Progress");
                CurrentSpawn = CreateText(Root.transform, "CurrentSpawn");
                OwnershipBoundary = CreateText(Root.transform, "OwnershipBoundary");

                Array waveTitles = CreateTextArray(
                    Root.transform,
                    "WaveTitle",
                    out Component[] waveTitleViews);
                WaveTitles = waveTitleViews;
                Array waveStates = CreateTextArray(
                    Root.transform,
                    "WaveState",
                    out Component[] waveStateViews);
                WaveStates = waveStateViews;
                Array waveDetails = CreateTextArray(
                    Root.transform,
                    "WaveDetail",
                    out Component[] waveDetailViews);
                WaveDetails = waveDetailViews;
                WaveAccents = new Image[3];
                for (int index = 0; index < WaveAccents.Length; index++)
                {
                    WaveAccents[index] = CreateImage(
                        Root.transform,
                        $"WaveAccent{index + 1}");
                }

                BeginButton = CreateButton(Root.transform, "BeginButton");
                ResolveButton = CreateButton(Root.transform, "ResolveButton");
                AdvanceButton = CreateButton(Root.transform, "AdvanceButton");
                InterruptButton = CreateButton(Root.transform, "InterruptButton");
                ResetButton = CreateButton(Root.transform, "ResetButton");

                Invoke(Controller, "ConfigureCore", Profile);
                Invoke(
                    Controller,
                    "ConfigureTextView",
                    AdmissionBoundary,
                    Title,
                    Identity,
                    Objective,
                    State,
                    Progress,
                    CurrentSpawn,
                    OwnershipBoundary);
                Invoke(
                    Controller,
                    "ConfigureWaveCards",
                    waveTitles,
                    waveStates,
                    waveDetails,
                    WaveAccents);
                Invoke(
                    Controller,
                    "ConfigureActions",
                    BeginButton,
                    ResolveButton,
                    AdvanceButton,
                    InterruptButton,
                    ResetButton);
            }

            public GameObject Root { get; }
            public Component Controller { get; }
            public StageEncounterPlanProfile Profile { get; }
            public Component AdmissionBoundary { get; }
            public Component Title { get; }
            public Component Identity { get; }
            public Component Objective { get; }
            public Component State { get; }
            public Component Progress { get; }
            public Component CurrentSpawn { get; }
            public Component OwnershipBoundary { get; }
            public Component[] WaveTitles { get; }
            public Component[] WaveStates { get; }
            public Component[] WaveDetails { get; }
            public Image[] WaveAccents { get; }
            public Button BeginButton { get; }
            public Button ResolveButton { get; }
            public Button AdvanceButton { get; }
            public Button InterruptButton { get; }
            public Button ResetButton { get; }

            public void Activate()
            {
                Root.SetActive(true);
            }

            public void Dispose()
            {
                UnityEngine.Object.DestroyImmediate(Root);
                UnityEngine.Object.DestroyImmediate(Profile);
            }

            private static void ConfigureValidProfile(StageEncounterPlanProfile profile)
            {
                var waves = new[]
                {
                    new StageEncounterPlanProfile.WaveDefinition(
                        "review.cf01.wave-01",
                        0,
                        StageEncounterWaveActivation.EncounterStart,
                        StageEncounterObjective.DefeatAll,
                        new[]
                        {
                            new StageEncounterPlanProfile.SpawnDefinition(
                                "review.cf01.spawn.wave-01.primary",
                                "review.cf01.payload.unit-a",
                                "review.cf01.anchor.entry-a",
                                2,
                                0f)
                        }),
                    new StageEncounterPlanProfile.WaveDefinition(
                        "review.cf01.wave-02",
                        1,
                        StageEncounterWaveActivation.PreviousWaveDefeated,
                        StageEncounterObjective.DefeatAll,
                        new[]
                        {
                            new StageEncounterPlanProfile.SpawnDefinition(
                                "review.cf01.spawn.wave-02.primary",
                                "review.cf01.payload.unit-b",
                                "review.cf01.anchor.entry-b",
                                1,
                                0.25f),
                            new StageEncounterPlanProfile.SpawnDefinition(
                                "review.cf01.spawn.wave-02.support",
                                "review.cf01.payload.unit-c",
                                "review.cf01.anchor.entry-c",
                                1,
                                0.5f)
                        }),
                    new StageEncounterPlanProfile.WaveDefinition(
                        "review.cf01.wave-03",
                        2,
                        StageEncounterWaveActivation.PreviousWaveDefeated,
                        StageEncounterObjective.DefeatAll,
                        new[]
                        {
                            new StageEncounterPlanProfile.SpawnDefinition(
                                "review.cf01.spawn.wave-03.primary",
                                "review.cf01.payload.unit-d",
                                "review.cf01.anchor.entry-d",
                                1,
                                0.75f)
                        })
                };
                profile.Configure(
                    1,
                    1,
                    "review.cf01.plan.local",
                    "review.cf01.stage.local",
                    StageEncounterPlanAdmissionDisposition.ReviewOnlyNotAdmitted,
                    StageEncounterPlanOutcomeOwner.ExistingStageRun,
                    StageEncounterPlanRewardOwner.ExternalRewardLedger,
                    new StageEncounterPlanProfile.EncounterDefinition(
                        "review.cf01.encounter.local",
                        waves));
                Assert.That(profile.TryValidate(out string error), Is.True, error);
            }
        }

        private static Array CreateTextArray(
            Transform parent,
            string prefix,
            out Component[] views)
        {
            Type textType = ResolveTextType();
            Array array = Array.CreateInstance(textType, 3);
            views = new Component[3];
            for (int index = 0; index < views.Length; index++)
            {
                Component view = CreateText(parent, $"{prefix}{index + 1}");
                array.SetValue(view, index);
                views[index] = view;
            }

            return array;
        }

        private static Component CreateText(Transform parent, string name)
        {
            var owner = new GameObject(
                name,
                typeof(RectTransform),
                typeof(CanvasRenderer));
            owner.transform.SetParent(parent, false);
            return owner.AddComponent(ResolveTextType());
        }

        private static Type ResolveTextType()
        {
            Type type = Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro");
            Assert.That(type, Is.Not.Null, "TextMeshProUGUI type is unavailable.");
            return type;
        }

        private static Image CreateImage(Transform parent, string name)
        {
            var owner = new GameObject(
                name,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            owner.transform.SetParent(parent, false);
            return owner.GetComponent<Image>();
        }

        private static Button CreateButton(Transform parent, string name)
        {
            var owner = new GameObject(
                name,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Button));
            owner.transform.SetParent(parent, false);
            return owner.GetComponent<Button>();
        }
    }
}
