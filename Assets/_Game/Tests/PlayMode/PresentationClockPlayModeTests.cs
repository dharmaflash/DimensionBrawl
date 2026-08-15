using System;
using System.Collections;
using System.Reflection;
using DimensionBrawl.Player;
using DimensionBrawl.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace DimensionBrawl.Tests
{
    public sealed class PresentationClockPlayModeTests
    {
        [Test]
        public void ManualLeaseSamplesExactSixtyFpsFramesAndRestoresUnityTime()
        {
            object owner = new object();
            float unityTimeBefore = Time.unscaledTime;

            using (PresentationClock.ManualLease lease = PresentationClock.AcquireManual(owner, 60))
            {
                Assert.That(PresentationClock.IsManuallyDriven, Is.True);
                Assert.That(lease.IsValid, Is.True);
                Assert.That(lease.FramesPerSecond, Is.EqualTo(60));

                lease.SetFrame(66);

                Assert.That(PresentationClock.UnscaledTime, Is.EqualTo(1.1f).Within(0.000001f));
                Assert.That(PresentationClock.UnscaledDeltaTime, Is.EqualTo(1f / 60f).Within(0.000001f));
                Assert.Throws<InvalidOperationException>(() =>
                    PresentationClock.AcquireManual(new object(), 60));
            }

            Assert.That(PresentationClock.IsManuallyDriven, Is.False);
            Assert.That(PresentationClock.UnscaledTime, Is.GreaterThanOrEqualTo(unityTimeBefore));
        }

        [Test]
        public void DisposingAnOldTokenCannotReleaseANewerManualLease()
        {
            object firstOwner = new object();
            PresentationClock.ManualLease first = PresentationClock.AcquireManual(firstOwner, 60);
            first.Dispose();

            object secondOwner = new object();
            using PresentationClock.ManualLease second = PresentationClock.AcquireManual(secondOwner, 60);
            second.SetFrame(12);

            first.Dispose();

            Assert.That(second.IsValid, Is.True);
            Assert.That(PresentationClock.IsManuallyDriven, Is.True);
            Assert.That(PresentationClock.UnscaledTime, Is.EqualTo(0.2f).Within(0.000001f));
        }

        [Test]
        public void ActionCameraMicroShakeSamplesIdenticallyAtTheSameManualFrame()
        {
            object clockOwner = new object();
            GameObject firstCamera = null;
            GameObject secondCamera = null;
            try
            {
                using PresentationClock.ManualLease lease = PresentationClock.AcquireManual(clockOwner, 60);
                lease.SetFrame(4);

                ActionCameraController first = CreateCameraController("DeterministicCameraA", out firstCamera);
                ActionCameraController second = CreateCameraController("DeterministicCameraB", out secondCamera);
                first.RequestMicroShake(0.2f, 0.05f, 0.5f, Vector3.forward, 18f);
                second.RequestMicroShake(0.2f, 0.05f, 0.5f, Vector3.forward, 18f);

                InvokeApplyMicroShake(first, 1f / 60f);
                InvokeApplyMicroShake(second, 1f / 60f);

                Assert.That(first.LastMicroShakeLocalOffset, Is.EqualTo(second.LastMicroShakeLocalOffset));
                Assert.That(first.LastMicroShakeEulerOffset, Is.EqualTo(second.LastMicroShakeEulerOffset));
                Assert.That(first.LastMicroShakeLocalOffset.sqrMagnitude, Is.GreaterThan(0f));
                Assert.That(first.LastMicroShakeEulerOffset.sqrMagnitude, Is.GreaterThan(0f));
            }
            finally
            {
                if (secondCamera != null)
                {
                    UnityEngine.Object.DestroyImmediate(secondCamera);
                }

                if (firstCamera != null)
                {
                    UnityEngine.Object.DestroyImmediate(firstCamera);
                }
            }
        }

        [Test]
        public void PerfectDodgeScreenCuePublishesTheManualPresentationTime()
        {
            object clockOwner = new object();
            GameObject presenterObject = null;
            try
            {
                using PresentationClock.ManualLease lease = PresentationClock.AcquireManual(clockOwner, 60);
                lease.SetFrame(178);

                presenterObject = new GameObject(
                    "DeterministicScreenCue",
                    typeof(ActionScreenCuePresenter));
                ActionScreenCuePresenter presenter = presenterObject.GetComponent<ActionScreenCuePresenter>();
                SetPrivateField(presenter, "perfectDodgeDomainDuration", 3f);
                SetPrivateField(presenter, "perfectDodgeDomainTimer", 2f);
                SetPrivateField(presenter, "perfectDodgeDomainIntensity", 1f);
                InvokePrivate(presenter, "PublishPerfectDodgeDomainState");

                float publishedTime = (float)(typeof(PerfectDodgeScreenDomainRuntime).GetField(
                    "timeSeconds",
                    BindingFlags.Static | BindingFlags.NonPublic)?.GetValue(null)
                    ?? throw new MissingFieldException(
                        nameof(PerfectDodgeScreenDomainRuntime),
                        "timeSeconds"));
                Assert.That(publishedTime, Is.EqualTo(178f / 60f).Within(0.000001f));
            }
            finally
            {
                PerfectDodgeScreenDomainRuntime.Clear();
                if (presenterObject != null)
                {
                    UnityEngine.Object.DestroyImmediate(presenterObject);
                }
            }
        }

        [Test]
        public void PerfectDodgeTimeWarpConsumesTheManualPresentationDelta()
        {
            object clockOwner = new object();
            GameObject warpObject = null;
            try
            {
                using PresentationClock.ManualLease lease = PresentationClock.AcquireManual(clockOwner, 60);
                lease.SetFrame(188);

                warpObject = new GameObject(
                    "DeterministicPerfectDodgeTimeWarp",
                    typeof(PlayerActionController),
                    typeof(PerfectDodgeTimeWarp));
                PerfectDodgeTimeWarp warp = warpObject.GetComponent<PerfectDodgeTimeWarp>();
                SetPrivateField(warp, "timer", 1f);

                IEnumerator routine = (IEnumerator)(typeof(PerfectDodgeTimeWarp).GetMethod(
                    "RefreshWarpUntilSettled",
                    BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(warp, null)
                    ?? throw new MissingMethodException(
                        nameof(PerfectDodgeTimeWarp),
                        "RefreshWarpUntilSettled"));

                Assert.That(routine.MoveNext(), Is.True, "The warp routine must yield once before sampling time.");
                Assert.That(routine.MoveNext(), Is.True, "The active warp must continue after one manual frame.");

                float remaining = (float)(typeof(PerfectDodgeTimeWarp).GetField(
                    "timer",
                    BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(warp)
                    ?? throw new MissingFieldException(nameof(PerfectDodgeTimeWarp), "timer"));
                Assert.That(remaining, Is.EqualTo(1f - 1f / 60f).Within(0.000001f));
            }
            finally
            {
                if (warpObject != null)
                {
                    UnityEngine.Object.DestroyImmediate(warpObject);
                }
            }
        }

        private static ActionCameraController CreateCameraController(
            string name,
            out GameObject cameraObject)
        {
            cameraObject = new GameObject(name, typeof(Camera), typeof(ActionCameraController));
            cameraObject.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            return cameraObject.GetComponent<ActionCameraController>();
        }

        private static void InvokeApplyMicroShake(ActionCameraController controller, float deltaTime)
        {
            InvokePrivate(controller, "ApplyMicroShake", deltaTime);
        }

        private static void InvokePrivate(object target, string methodName, params object[] arguments)
        {
            MethodInfo method = target.GetType().GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.NonPublic)
                ?? throw new MissingMethodException(target.GetType().Name, methodName);
            method.Invoke(target, arguments);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic)
                ?? throw new MissingFieldException(target.GetType().Name, fieldName);
            field.SetValue(target, value);
        }
    }
}
