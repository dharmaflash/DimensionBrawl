using System;
using System.Reflection;
using DimensionBrawl.Presentation.Narrative;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.TestTools;

namespace DimensionBrawl.Tests
{
    public sealed class OlympusStoryTutorialTransitionReviewGatePlayModeTests
    {
        private const string GateTypeName =
            "DimensionBrawl.UI.NarrativeReview.OlympusStoryTutorialTransitionReviewGate";
        private const string ProbeTypeName =
            "DimensionBrawl.UI.NarrativeReview.ReviewTutorialStartProbe";

        [Test]
        public void CompletedRestoresExactNonDefaultStateAndDispatchesOnce()
        {
            AssertSuccessfulTerminalRestoresAndDispatchesOnce(
                StoryTutorialReviewTerminalReason.Completed,
                generation: 101);
        }

        [Test]
        public void SkippedRestoresExactNonDefaultStateAndDispatchesOnce()
        {
            AssertSuccessfulTerminalRestoresAndDispatchesOnce(
                StoryTutorialReviewTerminalReason.Skipped,
                generation: 102);
        }

        [UnityTest]
        public System.Collections.IEnumerator DisablingActiveGateRestoresWithoutDispatch()
        {
            using var fixture = new GateFixture();
            Assert.That(fixture.TryBeginStory(201, out string error), Is.True, error);
            fixture.PerturbEveryLeasedDomain();

            fixture.Gate.enabled = false;
            yield return null;

            fixture.AssertExactBaselineRestored();
            StoryTutorialReviewReceipt receipt = fixture.LastReceipt;
            {
                Assert.That(receipt.IsSealed, Is.True);
                Assert.That(receipt.Generation, Is.EqualTo(201));
                Assert.That(
                    receipt.TerminalReason,
                    Is.EqualTo(StoryTutorialReviewTerminalReason.OwnerDisabled));
                Assert.That(receipt.StoryOwnedWorkReleased, Is.False);
                Assert.That(receipt.StateRestoreSucceeded, Is.True);
                Assert.That(receipt.TutorialTargetAvailable, Is.False);
                Assert.That(receipt.CanDispatchReviewTutorialStart, Is.False);
                Assert.That(fixture.ProbeDispatchCount, Is.Zero);
                Assert.That(fixture.LastDispatchSucceeded, Is.False);
            }
        }

        [Test]
        public void CancelledRestoresExactNonDefaultStateWithoutDispatch()
        {
            using var fixture = new GateFixture();
            Assert.That(fixture.TryBeginStory(301, out string error), Is.True, error);
            fixture.PerturbEveryLeasedDomain();

            Assert.That(
                fixture.TryRequestTerminal(
                    301,
                    StoryTutorialReviewTerminalReason.Cancelled),
                Is.EqualTo(StoryTutorialReviewSignalResult.Accepted));
            Assert.That(
                fixture.RestoreAndSeal(
                    301,
                    storyOwnedWorkReleased: true,
                    tutorialTargetAvailable: true,
                    out StoryTutorialReviewReceipt receipt),
                Is.EqualTo(StoryTutorialReviewSignalResult.Accepted));

            fixture.AssertExactBaselineRestored();
            {
                Assert.That(
                    receipt.TerminalReason,
                    Is.EqualTo(StoryTutorialReviewTerminalReason.Cancelled));
                Assert.That(receipt.StoryOwnedWorkReleased, Is.True);
                Assert.That(receipt.StateRestoreSucceeded, Is.True);
                Assert.That(receipt.TutorialTargetAvailable, Is.True);
                Assert.That(receipt.CanDispatchReviewTutorialStart, Is.False);
                Assert.That(fixture.TryClaimTutorialStart(301), Is.False);
                Assert.That(fixture.ConfirmTutorialStarted(301), Is.False);
                Assert.That(fixture.ProbeDispatchCount, Is.Zero);
                Assert.That(fixture.LastDispatchSucceeded, Is.False);
            }
        }

        [Test]
        public void DuplicateAndStaleSignalsCannotRestoreOrDispatchAgain()
        {
            using var fixture = new GateFixture();
            Assert.That(fixture.TryBeginStory(401, out string firstError), Is.True, firstError);
            fixture.PerturbEveryLeasedDomain();

            Assert.That(
                fixture.TryRequestTerminal(
                    401,
                    StoryTutorialReviewTerminalReason.Completed),
                Is.EqualTo(StoryTutorialReviewSignalResult.Accepted));
            Assert.That(
                fixture.TryRequestTerminal(
                    401,
                    StoryTutorialReviewTerminalReason.Skipped),
                Is.EqualTo(StoryTutorialReviewSignalResult.AlreadyAccepted));
            Assert.That(
                fixture.RestoreAndSeal(
                    401,
                    storyOwnedWorkReleased: true,
                    tutorialTargetAvailable: true,
                    out StoryTutorialReviewReceipt firstReceipt),
                Is.EqualTo(StoryTutorialReviewSignalResult.Accepted));
            Assert.That(fixture.ProbeDispatchCount, Is.Zero);
            Assert.That(fixture.TryClaimTutorialStart(401), Is.True);
            Assert.That(fixture.TryClaimTutorialStart(401), Is.False);
            Assert.That(fixture.ConfirmTutorialStarted(401), Is.True);
            Assert.That(fixture.ConfirmTutorialStarted(401), Is.False);
            Assert.That(fixture.ProbeDispatchCount, Is.EqualTo(1));

            Assert.That(
                fixture.RestoreAndSeal(
                    401,
                    storyOwnedWorkReleased: false,
                    tutorialTargetAvailable: false,
                    out StoryTutorialReviewReceipt duplicateReceipt),
                Is.EqualTo(StoryTutorialReviewSignalResult.AlreadyAccepted));
            {
                Assert.That(duplicateReceipt.Generation, Is.EqualTo(firstReceipt.Generation));
                Assert.That(
                    duplicateReceipt.TerminalReason,
                    Is.EqualTo(StoryTutorialReviewTerminalReason.Completed));
                Assert.That(duplicateReceipt.CanDispatchReviewTutorialStart, Is.True);
                Assert.That(fixture.ProbeDispatchCount, Is.EqualTo(1));
            }

            Assert.That(fixture.TryBeginStory(402, out string nextError), Is.True, nextError);
            fixture.PerturbEveryLeasedDomain();
            Assert.That(
                fixture.TryRequestTerminal(
                    401,
                    StoryTutorialReviewTerminalReason.Completed),
                Is.EqualTo(StoryTutorialReviewSignalResult.StaleGeneration));
            Assert.That(
                fixture.RestoreAndSeal(
                    401,
                    storyOwnedWorkReleased: true,
                    tutorialTargetAvailable: true,
                    out StoryTutorialReviewReceipt staleReceipt),
                Is.EqualTo(StoryTutorialReviewSignalResult.StaleGeneration));
            {
                Assert.That(staleReceipt.IsSealed, Is.False);
                Assert.That(fixture.TryClaimTutorialStart(401), Is.False);
                Assert.That(fixture.ConfirmTutorialStarted(401), Is.False);
                Assert.That(fixture.ProbeDispatchCount, Is.EqualTo(1));
                Assert.That(Time.timeScale, Is.EqualTo(GateFixture.PerturbedTimeScale).Within(0.0001f));
                Assert.That(fixture.GameplayHud.gameObject.activeSelf, Is.True);
            }

            Assert.That(
                fixture.TryRequestTerminal(
                    402,
                    StoryTutorialReviewTerminalReason.Cancelled),
                Is.EqualTo(StoryTutorialReviewSignalResult.Accepted));
            Assert.That(
                fixture.RestoreAndSeal(
                    402,
                    storyOwnedWorkReleased: true,
                    tutorialTargetAvailable: true,
                    out StoryTutorialReviewReceipt currentReceipt),
                Is.EqualTo(StoryTutorialReviewSignalResult.Accepted));
            fixture.AssertExactBaselineRestored();
            {
                Assert.That(currentReceipt.Generation, Is.EqualTo(402));
                Assert.That(currentReceipt.CanDispatchReviewTutorialStart, Is.False);
                Assert.That(fixture.TryClaimTutorialStart(402), Is.False);
                Assert.That(fixture.ConfirmTutorialStarted(402), Is.False);
                Assert.That(fixture.ProbeDispatchCount, Is.EqualTo(1));
            }
        }

        [Test]
        public void DestroyedSingleBindingRestoresSurvivorsAndBlocksDispatch()
        {
            using var fixture = new GateFixture();
            Assert.That(fixture.TryBeginStory(501, out string error), Is.True, error);
            fixture.PerturbEveryLeasedDomain();
            UnityEngine.Object.DestroyImmediate(fixture.GameplayInput);

            Assert.That(
                fixture.TryRequestTerminal(
                    501,
                    StoryTutorialReviewTerminalReason.Completed),
                Is.EqualTo(StoryTutorialReviewSignalResult.Accepted));
            Assert.That(
                fixture.RestoreAndSeal(
                    501,
                    storyOwnedWorkReleased: true,
                    tutorialTargetAvailable: true,
                    out StoryTutorialReviewReceipt receipt),
                Is.EqualTo(StoryTutorialReviewSignalResult.Accepted));

            fixture.AssertExactBaselineRestored(includeInput: false);
            {
                Assert.That(fixture.GameplayInput == null, Is.True);
                Assert.That(receipt.IsSealed, Is.True);
                Assert.That(
                    receipt.TerminalReason,
                    Is.EqualTo(StoryTutorialReviewTerminalReason.Completed));
                Assert.That(receipt.StoryOwnedWorkReleased, Is.True);
                Assert.That(receipt.StateRestoreSucceeded, Is.False);
                Assert.That(receipt.TutorialTargetAvailable, Is.True);
                Assert.That(receipt.CanDispatchReviewTutorialStart, Is.False);
                Assert.That(fixture.TryClaimTutorialStart(501), Is.False);
                Assert.That(fixture.ConfirmTutorialStarted(501), Is.False);
                Assert.That(fixture.ProbeDispatchCount, Is.Zero);
                Assert.That(fixture.LastDispatchSucceeded, Is.False);
            }
        }

        private static void AssertSuccessfulTerminalRestoresAndDispatchesOnce(
            StoryTutorialReviewTerminalReason terminalReason,
            long generation)
        {
            using var fixture = new GateFixture();
            Assert.That(fixture.TryBeginStory(generation, out string error), Is.True, error);
            fixture.AssertStoryOverridesApplied();
            fixture.PerturbEveryLeasedDomain();

            Assert.That(
                fixture.TryRequestTerminal(generation, terminalReason),
                Is.EqualTo(StoryTutorialReviewSignalResult.Accepted));
            Assert.That(
                fixture.RestoreAndSeal(
                    generation,
                    storyOwnedWorkReleased: true,
                    tutorialTargetAvailable: true,
                    out StoryTutorialReviewReceipt receipt),
                Is.EqualTo(StoryTutorialReviewSignalResult.Accepted));

            fixture.AssertExactBaselineRestored();
            Assert.That(fixture.ProbeDispatchCount, Is.Zero);
            Assert.That(fixture.TryClaimTutorialStart(generation), Is.True);
            Assert.That(fixture.HasTutorialStartClaimFor(generation), Is.True);
            Assert.That(fixture.ProbeDispatchCount, Is.Zero);
            Assert.That(fixture.ConfirmTutorialStarted(generation), Is.True);
            {
                Assert.That(receipt.Generation, Is.EqualTo(generation));
                Assert.That(receipt.TerminalReason, Is.EqualTo(terminalReason));
                Assert.That(receipt.StoryOwnedWorkReleased, Is.True);
                Assert.That(receipt.StateRestoreSucceeded, Is.True);
                Assert.That(receipt.TutorialTargetAvailable, Is.True);
                Assert.That(receipt.CanDispatchReviewTutorialStart, Is.True);
                Assert.That(fixture.ProbeDispatchCount, Is.EqualTo(1));
                Assert.That(fixture.ProbeLastGeneration, Is.EqualTo(generation));
                Assert.That(fixture.LastDispatchSucceeded, Is.True);
                Assert.That(fixture.WasTutorialStartConfirmedFor(generation), Is.True);
            }
        }

        private static Type RequireProductType(string fullName)
        {
            Type type = Type.GetType(fullName + ", DimensionBrawl.Runtime")
                ?? Type.GetType(fullName + ", Assembly-CSharp");
            Assert.That(type, Is.Not.Null, $"Missing product type {fullName}.");
            return type;
        }

        private static MethodInfo RequireMethod(Type type, string methodName)
        {
            MethodInfo method = type.GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, $"Missing method {type.Name}.{methodName}.");
            return method;
        }

        private static object Invoke(Component target, string methodName, params object[] arguments)
        {
            return RequireMethod(target.GetType(), methodName).Invoke(target, arguments);
        }

        private static T ReadProperty<T>(Component target, string propertyName)
        {
            PropertyInfo property = target.GetType().GetProperty(
                propertyName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.That(property, Is.Not.Null, $"Missing property {target.GetType().Name}.{propertyName}.");
            object value = property.GetValue(target);
            return value == null ? default : (T)value;
        }

        private sealed class GateFixture : IDisposable
        {
            public const float BaselineHudAlpha = 0.37f;
            public const float BaselineTimeScale = 0.43f;
            public const float PerturbedTimeScale = 0.81f;

            private readonly float initialTimeScale;

            public GateFixture()
            {
                initialTimeScale = Time.timeScale;
                Root = new GameObject("OlympusStoryTutorialTransitionGateTest");
                Root.SetActive(false);

                GameplayCamera = CreateCamera("ReviewGameplayCamera", out AudioListener gameplayListener);
                GameplayListener = gameplayListener;
                NarrativeCamera = CreateCamera(
                    "ReviewNarrativePresentationCamera",
                    out AudioListener narrativeListener);
                NarrativeListener = narrativeListener;

                GameObject hudOwner = CreateChild("ReviewGameplayHud");
                GameplayHud = hudOwner.AddComponent<CanvasGroup>();

                GameObject inputOwner = CreateChild("ReviewGameplayInput");
                inputOwner.AddComponent<EventSystem>();
                GameplayInput = inputOwner.AddComponent<InputSystemUIInputModule>();

                Probe = Root.AddComponent(RequireProductType(ProbeTypeName));
                Gate = (Behaviour)Root.AddComponent(RequireProductType(GateTypeName));
                Invoke(
                    Gate,
                    "Configure",
                    GameplayCamera,
                    NarrativeCamera,
                    GameplayHud,
                    GameplayInput,
                    GameplayListener,
                    NarrativeListener,
                    Probe);

                ApplyExactNonDefaultBaseline();
                Root.SetActive(true);
            }

            public GameObject Root { get; }
            public Behaviour Gate { get; }
            public Component Probe { get; }
            public Camera GameplayCamera { get; }
            public Camera NarrativeCamera { get; }
            public CanvasGroup GameplayHud { get; }
            public Behaviour GameplayInput { get; }
            public AudioListener GameplayListener { get; }
            public AudioListener NarrativeListener { get; }

            public StoryTutorialReviewReceipt LastReceipt =>
                ReadProperty<StoryTutorialReviewReceipt>(Gate, "LastReceipt");
            public bool LastDispatchSucceeded =>
                ReadProperty<bool>(Gate, "LastDispatchSucceeded");
            public int ProbeDispatchCount => ReadProperty<int>(Probe, "DispatchCount");
            public long ProbeLastGeneration => ReadProperty<long>(Probe, "LastGeneration");

            public bool TryBeginStory(long generation, out string error)
            {
                object[] arguments = { generation, null };
                bool accepted = (bool)Invoke(Gate, "TryBeginStory", arguments);
                error = arguments[1] as string ?? string.Empty;
                return accepted;
            }

            public StoryTutorialReviewSignalResult TryRequestTerminal(
                long generation,
                StoryTutorialReviewTerminalReason terminalReason)
            {
                return (StoryTutorialReviewSignalResult)Invoke(
                    Gate,
                    "TryRequestTerminal",
                    generation,
                    terminalReason);
            }

            public StoryTutorialReviewSignalResult RestoreAndSeal(
                long generation,
                bool storyOwnedWorkReleased,
                bool tutorialTargetAvailable,
                out StoryTutorialReviewReceipt receipt)
            {
                object[] arguments =
                {
                    generation,
                    storyOwnedWorkReleased,
                    tutorialTargetAvailable,
                    null
                };
                StoryTutorialReviewSignalResult result =
                    (StoryTutorialReviewSignalResult)Invoke(
                        Gate,
                        "RestoreAndSeal",
                        arguments);
                receipt = arguments[3] is StoryTutorialReviewReceipt resolved
                    ? resolved
                    : default;
                return result;
            }

            public bool TryClaimTutorialStart(long generation)
            {
                return (bool)Invoke(
                    Gate,
                    "TryClaimTutorialStart",
                    generation);
            }

            public bool ConfirmTutorialStarted(long generation)
            {
                return (bool)Invoke(
                    Gate,
                    "ConfirmTutorialStarted",
                    generation);
            }

            public bool HasTutorialStartClaimFor(long generation)
            {
                return (bool)Invoke(
                    Gate,
                    "HasTutorialStartClaimFor",
                    generation);
            }

            public bool WasTutorialStartConfirmedFor(long generation)
            {
                return (bool)Invoke(
                    Gate,
                    "WasTutorialStartConfirmedFor",
                    generation);
            }

            public void AssertStoryOverridesApplied()
            {
                {
                    Assert.That(GameplayCamera.enabled, Is.False);
                    Assert.That(NarrativeCamera.enabled, Is.True);
                    Assert.That(GameplayHud.gameObject.activeSelf, Is.False);
                    Assert.That(GameplayHud.alpha, Is.Zero.Within(0.0001f));
                    Assert.That(GameplayHud.interactable, Is.False);
                    Assert.That(GameplayHud.blocksRaycasts, Is.False);
                    Assert.That(GameplayInput.enabled, Is.False);
                    Assert.That(GameplayListener.enabled, Is.False);
                    Assert.That(NarrativeListener.enabled, Is.True);
                    Assert.That(Time.timeScale, Is.Zero.Within(0.0001f));
                }
            }

            public void PerturbEveryLeasedDomain()
            {
                GameplayCamera.enabled = true;
                NarrativeCamera.enabled = true;
                GameplayHud.gameObject.SetActive(true);
                GameplayHud.alpha = 0.91f;
                GameplayHud.interactable = false;
                GameplayHud.blocksRaycasts = true;
                GameplayInput.enabled = true;
                GameplayListener.enabled = true;
                NarrativeListener.enabled = true;
                Time.timeScale = PerturbedTimeScale;
            }

            public void AssertExactBaselineRestored(bool includeInput = true)
            {
                {
                    Assert.That(GameplayCamera.enabled, Is.False);
                    Assert.That(NarrativeCamera.enabled, Is.False);
                    Assert.That(GameplayHud.gameObject.activeSelf, Is.False);
                    Assert.That(GameplayHud.alpha, Is.EqualTo(BaselineHudAlpha).Within(0.0001f));
                    Assert.That(GameplayHud.interactable, Is.True);
                    Assert.That(GameplayHud.blocksRaycasts, Is.False);
                    if (includeInput)
                    {
                        Assert.That(GameplayInput.enabled, Is.False);
                    }

                    Assert.That(GameplayListener.enabled, Is.False);
                    Assert.That(NarrativeListener.enabled, Is.False);
                    Assert.That(Time.timeScale, Is.EqualTo(BaselineTimeScale).Within(0.0001f));
                }
            }

            public void Dispose()
            {
                if (Root != null)
                {
                    UnityEngine.Object.DestroyImmediate(Root);
                }

                Time.timeScale = initialTimeScale;
            }

            private Camera CreateCamera(string name, out AudioListener listener)
            {
                GameObject owner = CreateChild(name);
                Camera camera = owner.AddComponent<Camera>();
                listener = owner.AddComponent<AudioListener>();
                return camera;
            }

            private GameObject CreateChild(string name)
            {
                var child = new GameObject(name);
                child.transform.SetParent(Root.transform, false);
                return child;
            }

            private void ApplyExactNonDefaultBaseline()
            {
                GameplayCamera.enabled = false;
                NarrativeCamera.enabled = false;
                GameplayHud.gameObject.SetActive(false);
                GameplayHud.alpha = BaselineHudAlpha;
                GameplayHud.interactable = true;
                GameplayHud.blocksRaycasts = false;
                GameplayInput.enabled = false;
                GameplayListener.enabled = false;
                NarrativeListener.enabled = false;
                Time.timeScale = BaselineTimeScale;
            }
        }
    }
}
