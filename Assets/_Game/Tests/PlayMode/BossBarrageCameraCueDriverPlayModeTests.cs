using System;
using System.Reflection;
using DimensionBrawl.Combat;
using DimensionBrawl.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace DimensionBrawl.Tests
{
    public sealed class BossBarrageCameraCueDriverPlayModeTests
    {
        private const string CrushNetPatternId = "AkazaCrushNet";
        private const float SustainSeconds = 3.2f;
        private const float ReleaseSeconds = 0.18f;
        private const float FieldOfViewDelta = -11.8f;
        private const float CameraDistanceDelta = -0.9f;

        [Test]
        public void CrushNetOverrideHoldsThroughFrame188AndPreservesItsFireRead()
        {
            using var fixture = new CameraCueFixture(CrushNetPatternId);
            fixture.ConfigureCrushNetOverride();

            InvokePrivate(
                fixture.Driver,
                "HandleWindupStarted",
                null,
                fixture.Pattern);

            Assert.That(fixture.Driver.WindupCueRequestCount, Is.EqualTo(1));
            Assert.That(fixture.Driver.PatternWindupOverrideRequestCount, Is.EqualTo(1));
            Assert.That(fixture.Driver.ActivePatternWindupOverrideId, Is.EqualTo(CrushNetPatternId));
            Assert.That(fixture.Camera.ActiveCueSustainSeconds, Is.EqualTo(SustainSeconds).Within(0.0001f));
            Assert.That(
                fixture.Camera.ActiveCueDuration,
                Is.EqualTo(SustainSeconds + ReleaseSeconds).Within(0.0001f));
            Assert.That(fixture.Camera.ActiveCueFieldOfViewDelta, Is.EqualTo(FieldOfViewDelta).Within(0.0001f));
            Assert.That(fixture.Camera.ActiveCueCameraDistanceDelta, Is.EqualTo(CameraDistanceDelta).Within(0.0001f));

            Assert.That(InvokeCueWeight(fixture.Camera, 71f / 60f), Is.EqualTo(1f));
            int windupRequestVersion = fixture.Camera.CueRequestVersion;
            InvokePrivate(
                fixture.Driver,
                "HandleWaveFired",
                null,
                fixture.Pattern,
                7);

            Assert.That(fixture.Driver.PreservedPatternFireCueCount, Is.EqualTo(1));
            Assert.That(fixture.Driver.FireCueRequestCount, Is.Zero);
            Assert.That(fixture.Camera.CueRequestVersion, Is.EqualTo(windupRequestVersion));
            Assert.That(InvokeCueWeight(fixture.Camera, (188f - 71f) / 60f), Is.EqualTo(1f));

            float playerScaleRatio = ResolveScreenHeightRatio(
                subjectDepth: 2.75f,
                baseFieldOfView: 54f,
                fieldOfViewDelta: FieldOfViewDelta,
                cameraDistanceDelta: CameraDistanceDelta);
            float closestBossScaleRatio = ResolveScreenHeightRatio(
                subjectDepth: 12.15f,
                baseFieldOfView: 54f,
                fieldOfViewDelta: FieldOfViewDelta,
                cameraDistanceDelta: CameraDistanceDelta);
            Assert.That(playerScaleRatio, Is.LessThanOrEqualTo(1f), "The player must not grow.");
            Assert.That(
                11.4583f * closestBossScaleRatio,
                Is.GreaterThanOrEqualTo(14f),
                "The baseline closest boss framing must clear 14% of frame height.");
        }

        [Test]
        public void NonCrushNetWindupKeepsTheOriginalDecayAndFireCue()
        {
            using var fixture = new CameraCueFixture("AkazaHoverLance");
            fixture.ConfigureCrushNetOverride();

            InvokePrivate(
                fixture.Driver,
                "HandleWindupStarted",
                null,
                fixture.Pattern);

            Assert.That(fixture.Driver.PatternWindupOverrideRequestCount, Is.Zero);
            Assert.That(fixture.Camera.ActiveCueSustainSeconds, Is.Zero);
            Assert.That(fixture.Camera.ActiveCueDuration, Is.EqualTo(0.26f).Within(0.0001f));
            Assert.That(fixture.Camera.ActiveCueFieldOfViewDelta, Is.EqualTo(1.06f).Within(0.0001f));
            Assert.That(fixture.Camera.ActiveCueCameraDistanceDelta, Is.EqualTo(-0.1272f).Within(0.0001f));
            Assert.That(InvokeCueWeight(fixture.Camera, 0.13f), Is.EqualTo(0.5f).Within(0.0001f));

            int windupRequestVersion = fixture.Camera.CueRequestVersion;
            InvokePrivate(
                fixture.Driver,
                "HandleWaveFired",
                null,
                fixture.Pattern,
                7);

            Assert.That(fixture.Driver.PreservedPatternFireCueCount, Is.Zero);
            Assert.That(fixture.Driver.FireCueRequestCount, Is.EqualTo(1));
            Assert.That(fixture.Camera.CueRequestVersion, Is.EqualTo(windupRequestVersion + 1));
            Assert.That(fixture.Camera.ActiveCueSustainSeconds, Is.Zero);
            Assert.That(fixture.Camera.ActiveCueDuration, Is.EqualTo(0.2f).Within(0.0001f));
            Assert.That(fixture.Camera.ActiveCueFieldOfViewDelta, Is.EqualTo(2f).Within(0.0001f));
            Assert.That(fixture.Camera.ActiveCueCameraDistanceDelta, Is.EqualTo(-0.225f).Within(0.0001f));
        }

        [Test]
        public void LaterCameraCueSupersedesCrushNetAndAllowsTheFireCue()
        {
            using var fixture = new CameraCueFixture(CrushNetPatternId);
            fixture.ConfigureCrushNetOverride();
            InvokePrivate(
                fixture.Driver,
                "HandleWindupStarted",
                null,
                fixture.Pattern);

            fixture.Camera.RequestCue(
                Vector3.zero,
                0.34f,
                3.6f,
                -0.34f,
                0.08f);
            int laterRequestVersion = fixture.Camera.CueRequestVersion;

            InvokePrivate(
                fixture.Driver,
                "HandleWaveFired",
                null,
                fixture.Pattern,
                7);

            Assert.That(fixture.Driver.PreservedPatternFireCueCount, Is.Zero);
            Assert.That(fixture.Driver.FireCueRequestCount, Is.EqualTo(1));
            Assert.That(fixture.Driver.ActivePatternWindupOverrideId, Is.Null);
            Assert.That(fixture.Camera.CueRequestVersion, Is.EqualTo(laterRequestVersion + 1));
            Assert.That(fixture.Camera.ActiveCueFieldOfViewDelta, Is.EqualTo(2f).Within(0.0001f));
        }

        private static float ResolveScreenHeightRatio(
            float subjectDepth,
            float baseFieldOfView,
            float fieldOfViewDelta,
            float cameraDistanceDelta)
        {
            float baseHalfFovRadians = baseFieldOfView * Mathf.Deg2Rad * 0.5f;
            float cueHalfFovRadians = (baseFieldOfView + fieldOfViewDelta) * Mathf.Deg2Rad * 0.5f;
            float movedDepth = subjectDepth - cameraDistanceDelta;
            return subjectDepth * Mathf.Tan(baseHalfFovRadians)
                / (movedDepth * Mathf.Tan(cueHalfFovRadians));
        }

        private static float InvokeCueWeight(ActionCameraController controller, float deltaTime)
        {
            return (float)InvokePrivate(controller, "UpdateCueWeight", deltaTime);
        }

        private static object InvokePrivate(object target, string methodName, params object[] arguments)
        {
            MethodInfo method = target.GetType().GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.NonPublic)
                ?? throw new MissingMethodException(target.GetType().Name, methodName);
            return method.Invoke(target, arguments);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic)
                ?? throw new MissingFieldException(target.GetType().Name, fieldName);
            field.SetValue(target, value);
        }

        private sealed class CameraCueFixture : IDisposable
        {
            private readonly GameObject cameraObject;
            private readonly GameObject cueSpaceObject;

            public CameraCueFixture(string patternId)
            {
                cameraObject = new GameObject(
                    "BossCameraCueTestCamera",
                    typeof(Camera),
                    typeof(ActionCameraController),
                    typeof(BossBarrageCameraCueDriver));
                cueSpaceObject = new GameObject("BossCameraCueTestSpace");
                Camera = cameraObject.GetComponent<ActionCameraController>();
                Driver = cameraObject.GetComponent<BossBarrageCameraCueDriver>();
                Pattern = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
                SetPrivateField(Pattern, "patternId", patternId);
                SetPrivateField(Camera, "maxCueFieldOfViewDelta", 12f);
                SetPrivateField(Camera, "maxCueCameraDistanceDelta", 0.95f);
                Driver.Configure(null, Camera, cueSpaceObject.transform);
            }

            public ActionCameraController Camera { get; }
            public BossBarrageCameraCueDriver Driver { get; }
            public BossBarragePatternProfile Pattern { get; }

            public void ConfigureCrushNetOverride()
            {
                var cue = new ActionCameraCueProfile.CameraCue
                {
                    enabled = true,
                    localOffset = Vector3.zero,
                    planarDirectionOffset = 0f,
                    fieldOfViewDelta = FieldOfViewDelta,
                    cameraDistanceDelta = CameraDistanceDelta,
                    focusHeightDelta = 0f,
                    durationSeconds = SustainSeconds,
                    finisherScale = 1f
                };
                Driver.ConfigurePatternWindupCueOverrides(
                    ReleaseSeconds,
                    new BossBarrageCameraCueDriver.PatternWindupCueOverride(
                        CrushNetPatternId,
                        cue));
            }

            public void Dispose()
            {
                UnityEngine.Object.DestroyImmediate(Pattern);
                UnityEngine.Object.DestroyImmediate(cueSpaceObject);
                UnityEngine.Object.DestroyImmediate(cameraObject);
            }
        }
    }
}
