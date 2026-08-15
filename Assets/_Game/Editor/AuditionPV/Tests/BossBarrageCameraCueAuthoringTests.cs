using System;
using System.Linq;
using DimensionBrawl.Combat;
using DimensionBrawl.Presentation;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DimensionBrawl.Editor.AuditionPV.Tests
{
    public sealed class BossBarrageCameraCueAuthoringTests
    {
        [Test]
        public void StationSceneAuthorsCrushNetPerspectiveCompression()
        {
            Scene existing = SceneManager.GetSceneByPath(OlympusStationAkazaPhase2Setup.StationScenePath);
            bool openedForTest = !existing.IsValid() || !existing.isLoaded;
            Scene scene = openedForTest
                ? EditorSceneManager.OpenScene(
                    OlympusStationAkazaPhase2Setup.StationScenePath,
                    OpenSceneMode.Additive)
                : existing;

            try
            {
                AssertAuthoredComposition(
                    RequireSingle<BossBarrageCameraCueDriver>(scene),
                    RequireSingle<ActionCameraController>(scene));
            }
            finally
            {
                if (openedForTest && scene.IsValid())
                {
                    EditorSceneManager.CloseScene(scene, removeScene: true);
                }
            }
        }

        [Test]
        public void StationSetupCameraCompositionIsIdempotent()
        {
            GameObject cameraObject = null;
            GameObject bossObject = null;
            GameObject cueSpaceObject = null;
            try
            {
                cameraObject = new GameObject(
                    "StationCameraSetupTestCamera",
                    typeof(Camera),
                    typeof(ActionCameraController),
                    typeof(BossBarrageCameraCueDriver));
                bossObject = new GameObject(
                    "StationCameraSetupTestBoss",
                    typeof(BossBarrageEmitter),
                    typeof(BossPressureActionDirector));
                cueSpaceObject = new GameObject("StationCameraSetupTestCueSpace");

                ActionCameraController camera = cameraObject.GetComponent<ActionCameraController>();
                BossBarrageCameraCueDriver driver = cameraObject.GetComponent<BossBarrageCameraCueDriver>();
                BossBarrageEmitter emitter = bossObject.GetComponent<BossBarrageEmitter>();
                BossPressureActionDirector actions = bossObject.GetComponent<BossPressureActionDirector>();
                BossBarragePatternProfile crushNet = AssetDatabase.LoadAssetAtPath<BossBarragePatternProfile>(
                    "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_BossBarrage_Phase2_AkazaCrushNet.asset");
                Assert.That(crushNet, Is.Not.Null);

                OlympusStationAkazaPhase2Setup.ConfigureBossBarrageCameraComposition(
                    driver,
                    emitter,
                    actions,
                    camera,
                    cueSpaceObject.transform,
                    crushNet);
                string firstDriverJson = EditorJsonUtility.ToJson(driver);
                string firstCameraJson = EditorJsonUtility.ToJson(camera);

                OlympusStationAkazaPhase2Setup.ConfigureBossBarrageCameraComposition(
                    driver,
                    emitter,
                    actions,
                    camera,
                    cueSpaceObject.transform,
                    crushNet);

                Assert.That(EditorJsonUtility.ToJson(driver), Is.EqualTo(firstDriverJson));
                Assert.That(EditorJsonUtility.ToJson(camera), Is.EqualTo(firstCameraJson));
                AssertAuthoredComposition(driver, camera);
            }
            finally
            {
                if (cueSpaceObject != null)
                {
                    UnityEngine.Object.DestroyImmediate(cueSpaceObject);
                }

                if (bossObject != null)
                {
                    UnityEngine.Object.DestroyImmediate(bossObject);
                }

                if (cameraObject != null)
                {
                    UnityEngine.Object.DestroyImmediate(cameraObject);
                }
            }
        }

        private static void AssertAuthoredComposition(
            BossBarrageCameraCueDriver driver,
            ActionCameraController camera)
        {
            SerializedObject driverSerialized = new SerializedObject(driver);
            SerializedProperty release = driverSerialized.FindProperty("patternWindupCueReleaseSeconds");
            SerializedProperty overrides = driverSerialized.FindProperty("patternWindupCueOverrides");
            Assert.That(release, Is.Not.Null);
            Assert.That(overrides, Is.Not.Null);
            Assert.That(
                release.floatValue,
                Is.EqualTo(OlympusStationAkazaPhase2Setup.CrushNetCameraReleaseSeconds).Within(0.0001f));
            Assert.That(overrides.arraySize, Is.EqualTo(1));

            SerializedProperty entry = overrides.GetArrayElementAtIndex(0);
            SerializedProperty cue = entry.FindPropertyRelative("cue");
            Assert.That(
                entry.FindPropertyRelative("patternId").stringValue,
                Is.EqualTo(OlympusStationAkazaPhase2Setup.CrushNetCameraPatternId));
            Assert.That(cue.FindPropertyRelative("enabled").boolValue, Is.True);
            Assert.That(cue.FindPropertyRelative("localOffset").vector3Value, Is.EqualTo(Vector3.zero));
            Assert.That(cue.FindPropertyRelative("planarDirectionOffset").floatValue, Is.Zero);
            Assert.That(
                cue.FindPropertyRelative("fieldOfViewDelta").floatValue,
                Is.EqualTo(OlympusStationAkazaPhase2Setup.CrushNetCameraFieldOfViewDelta).Within(0.0001f));
            Assert.That(
                cue.FindPropertyRelative("cameraDistanceDelta").floatValue,
                Is.EqualTo(OlympusStationAkazaPhase2Setup.CrushNetCameraDistanceDelta).Within(0.0001f));
            Assert.That(cue.FindPropertyRelative("focusHeightDelta").floatValue, Is.Zero);
            Assert.That(
                cue.FindPropertyRelative("durationSeconds").floatValue,
                Is.EqualTo(OlympusStationAkazaPhase2Setup.CrushNetCameraSustainSeconds).Within(0.0001f));
            Assert.That(cue.FindPropertyRelative("finisherScale").floatValue, Is.EqualTo(1f));

            SerializedObject cameraSerialized = new SerializedObject(camera);
            Assert.That(
                cameraSerialized.FindProperty("maxCueFieldOfViewDelta").floatValue,
                Is.EqualTo(OlympusStationAkazaPhase2Setup.StationMaxCueFieldOfViewDelta).Within(0.0001f));
            Assert.That(
                cameraSerialized.FindProperty("maxCueCameraDistanceDelta").floatValue,
                Is.EqualTo(OlympusStationAkazaPhase2Setup.StationMaxCueCameraDistanceDelta).Within(0.0001f));
        }

        private static T RequireSingle<T>(Scene scene)
            where T : Component
        {
            return scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<T>(includeInactive: true))
                .Single();
        }
    }
}
