using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DimensionBrawl.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;

namespace DimensionBrawl.Editor
{
    public static class IntroGatePodBombDropTempoPolishSetup
    {
        private const string OlympusStageScenePath = "Assets/_Game/Scenes/OlympusCorridorInvasionStage.unity";
        private const string OlympusCombinedTimelinePath =
            "Assets/_Game/DesignData/Timelines/Cinematics/DB_Timeline_IntroGatePodAwakening_OlympusBombingPrelude.playable";
        private const string OlympusCombinedProfilePath =
            "Assets/_Game/DesignData/Profiles/Cinematics/DB_Cinematic_IntroGatePodAwakening_OlympusBombingPrelude.asset";
        private const string BombDropClipPath =
            "Assets/_Game/DesignData/Animations/Cinematics/IntroGatePodBombingReview/AC_OlympusBombingPrelude_BombDropMove.anim";
        private const string OpeningDollyClipName = "src_c01_capsule_left_dolly";
        private const string ReportPath = "C:/tmp/DimensionBrawl-IntroGatePodBombDropTempoPolish.md";

        private const double TargetPreludeDurationSeconds = 8.50d;
        private const float TailShiftStartSeconds = 5.86f;
        private const float BombReleaseStartSeconds = 3.72f;
        private const float OldBombImpactSeconds = 5.92f;
        private const float TargetBombImpactSeconds = 5.62f;
        private const float Epsilon = 0.005f;

        [MenuItem("Tools/DimensionBrawl/Intro GatePod/Polish Bomb Drop Tempo")]
        public static void PolishBombDropTempoMenu()
        {
            ApplyBombDropTempoPolish(writeReport: true);
        }

        public static void RunBatchApplyBombDropTempoPolish()
        {
            ApplyBombDropTempoPolish(writeReport: true);
        }

        private static void ApplyBombDropTempoPolish(bool writeReport)
        {
            AssetDatabase.Refresh();
            Scene scene = EditorSceneManager.OpenScene(OlympusStageScenePath, OpenSceneMode.Single);
            TimelineAsset timeline = LoadRequired<TimelineAsset>(OlympusCombinedTimelinePath);
            CinematicSequenceProfile profile = LoadRequired<CinematicSequenceProfile>(OlympusCombinedProfilePath);
            PlayableDirector director = FindDirectorBoundToTimeline(scene, timeline)
                ?? throw new InvalidOperationException("Could not find the Olympus intro PlayableDirector bound to the combined Timeline.");

            double openingStartBefore = FindClipStart(timeline, OpeningDollyClipName);
            float pullForwardSeconds = Mathf.Max(0f, (float)(openingStartBefore - TargetPreludeDurationSeconds));
            if (pullForwardSeconds > Epsilon)
            {
                ShiftTimelineAfterBombDrop(timeline, pullForwardSeconds);
                ShiftSceneTiming(scene, pullForwardSeconds);
                ShiftProfileTiming(profile, pullForwardSeconds);
            }

            RewriteBombDropClip();

            director.time = 0d;
            director.Evaluate();
            EditorUtility.SetDirty(director);
            EditorUtility.SetDirty(timeline);
            EditorUtility.SetDirty(profile);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            List<string> issues = ValidateSetup(timeline, profile);
            if (writeReport)
            {
                WriteReport(issues, openingStartBefore, FindClipStart(timeline, OpeningDollyClipName), pullForwardSeconds);
            }

            if (issues.Count > 0)
            {
                throw new InvalidOperationException(
                    "Intro GatePod bomb-drop tempo polish failed:\n" + string.Join("\n", issues));
            }
        }

        private static void ShiftTimelineAfterBombDrop(TimelineAsset timeline, float pullForwardSeconds)
        {
            HashSet<AnimationClip> shiftedAnimationClips = new HashSet<AnimationClip>();
            foreach (TrackAsset track in timeline.GetOutputTracks())
            {
                if (track == null)
                {
                    continue;
                }

                foreach (TimelineClip clip in track.GetClips())
                {
                    double originalStart = clip.start;
                    double originalEnd = clip.end;
                    if (originalStart >= TailShiftStartSeconds - Epsilon)
                    {
                        clip.start = Math.Max(0d, originalStart - pullForwardSeconds);
                    }
                    else if (originalEnd > TailShiftStartSeconds + Epsilon)
                    {
                        clip.duration = Math.Max(0.01d, clip.duration - pullForwardSeconds);
                        ShiftAnimationClipKeys(clip, pullForwardSeconds, shiftedAnimationClips);
                    }

                    UnityEngine.Object clipAsset = clip.asset as UnityEngine.Object;
                    if (clipAsset != null)
                    {
                        EditorUtility.SetDirty(clipAsset);
                    }
                }

                EditorUtility.SetDirty(track);
            }

            if (timeline.durationMode == TimelineAsset.DurationMode.FixedLength)
            {
                timeline.fixedDuration = Math.Max(0.01d, timeline.fixedDuration - pullForwardSeconds);
            }
        }

        private static void ShiftAnimationClipKeys(
            TimelineClip timelineClip,
            float pullForwardSeconds,
            HashSet<AnimationClip> shiftedAnimationClips)
        {
            AnimationPlayableAsset playableAsset = timelineClip.asset as AnimationPlayableAsset;
            AnimationClip animationClip = playableAsset != null ? playableAsset.clip : null;
            if (animationClip == null || shiftedAnimationClips.Contains(animationClip))
            {
                return;
            }

            float localThreshold = Mathf.Max(0f, TailShiftStartSeconds - (float)timelineClip.start);
            EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(animationClip);
            for (int i = 0; i < bindings.Length; i++)
            {
                AnimationCurve curve = AnimationUtility.GetEditorCurve(animationClip, bindings[i]);
                if (curve == null || curve.length == 0)
                {
                    continue;
                }

                Keyframe[] keys = curve.keys;
                bool changed = false;
                for (int keyIndex = 0; keyIndex < keys.Length; keyIndex++)
                {
                    if (keys[keyIndex].time >= localThreshold - Epsilon)
                    {
                        keys[keyIndex].time = Mathf.Max(0f, keys[keyIndex].time - pullForwardSeconds);
                        changed = true;
                    }
                }

                if (!changed)
                {
                    continue;
                }

                Array.Sort(keys, (left, right) => left.time.CompareTo(right.time));
                AnimationCurve shiftedCurve = new AnimationCurve(keys);
                shiftedCurve.preWrapMode = curve.preWrapMode;
                shiftedCurve.postWrapMode = curve.postWrapMode;
                AnimationUtility.SetEditorCurve(animationClip, bindings[i], shiftedCurve);
            }

            EditorUtility.SetDirty(animationClip);
            shiftedAnimationClips.Add(animationClip);
        }

        private static void RewriteBombDropClip()
        {
            AnimationClip clip = LoadRequired<AnimationClip>(BombDropClipPath);
            clip.frameRate = 30f;
            clip.wrapMode = WrapMode.ClampForever;
            ClearAnimationClipCurves(clip);

            float compressedMidA = CompressBombDropTime(BombReleaseStartSeconds + 0.36f);
            float compressedMidB = CompressBombDropTime(4.84f);
            SetVector3Curves(
                clip,
                (0f, new Vector3(0f, 6.35f, 6.25f)),
                (BombReleaseStartSeconds, new Vector3(0f, 6.35f, 6.25f)),
                (compressedMidA, new Vector3(0.05f, 5.88f, 6.28f)),
                (compressedMidB, new Vector3(0.18f, 3.15f, 6.42f)),
                (TargetBombImpactSeconds, new Vector3(0.42f, 0.36f, 6.65f)),
                ((float)TargetPreludeDurationSeconds, new Vector3(0.42f, 0.36f, 6.65f)));
            SetCurve(clip, "m_LocalRotation.x", LinearValueKeyed(0f, (0f, 0f), ((float)TargetPreludeDurationSeconds, 0f)));
            SetCurve(clip, "m_LocalRotation.y", LinearValueKeyed(0f, (0f, 0f), ((float)TargetPreludeDurationSeconds, 0f)));
            SetCurve(clip, "m_LocalRotation.z", LinearValueKeyed(0f, (0f, 0f), ((float)TargetPreludeDurationSeconds, 0f)));
            SetCurve(clip, "m_LocalRotation.w", LinearValueKeyed(1f, (0f, 1f), ((float)TargetPreludeDurationSeconds, 1f)));
            EditorUtility.SetDirty(clip);
        }

        private static void ClearAnimationClipCurves(AnimationClip clip)
        {
            clip.ClearCurves();
            EditorCurveBinding[] floatBindings = AnimationUtility.GetCurveBindings(clip);
            for (int i = 0; i < floatBindings.Length; i++)
            {
                AnimationUtility.SetEditorCurve(clip, floatBindings[i], null);
            }

            EditorCurveBinding[] objectBindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);
            for (int i = 0; i < objectBindings.Length; i++)
            {
                AnimationUtility.SetObjectReferenceCurve(clip, objectBindings[i], null);
            }
        }

        private static float CompressBombDropTime(float originalTime)
        {
            float newDuration = TargetBombImpactSeconds - BombReleaseStartSeconds;
            float t = Mathf.InverseLerp(BombReleaseStartSeconds, OldBombImpactSeconds, originalTime);
            return BombReleaseStartSeconds + (newDuration * t);
        }

        private static void ShiftSceneTiming(Scene scene, float pullForwardSeconds)
        {
            IntroGatePodCinemachineShotPlayer shotPlayer = FindComponentInScene<IntroGatePodCinemachineShotPlayer>(scene);
            if (shotPlayer != null)
            {
                SerializedObject serialized = new SerializedObject(shotPlayer);
                ShiftArrayFloatField(serialized.FindProperty("shots"), "startSeconds", pullForwardSeconds);
                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(shotPlayer);
            }

            IntroGatePodCutsceneCueDirector cueDirector = FindComponentInScene<IntroGatePodCutsceneCueDirector>(scene);
            if (cueDirector != null)
            {
                SerializedObject serialized = new SerializedObject(cueDirector);
                ShiftArrayFloatField(serialized.FindProperty("dollyCues"), "startSeconds", pullForwardSeconds);
                ShiftArrayFloatField(serialized.FindProperty("voiceCues"), "startSeconds", pullForwardSeconds);
                ShiftArrayFloatField(serialized.FindProperty("fadeCues"), "startSeconds", pullForwardSeconds);
                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(cueDirector);
            }

            IntroGatePodFirstPersonRendererMask rendererMask = FindComponentInScene<IntroGatePodFirstPersonRendererMask>(scene);
            if (rendererMask != null)
            {
                SerializedObject serialized = new SerializedObject(rendererMask);
                ShiftFloatProperty(serialized.FindProperty("hideStartSeconds"), pullForwardSeconds);
                ShiftFloatProperty(serialized.FindProperty("hideEndSeconds"), pullForwardSeconds);
                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(rendererMask);
            }

            IntroGatePodInvasionBridgeCue bridge = FindComponentInScene<IntroGatePodInvasionBridgeCue>(scene);
            if (bridge != null)
            {
                SerializedObject serialized = new SerializedObject(bridge);
                SerializedProperty commandos = serialized.FindProperty("commandos");
                ShiftArrayFloatField(commandos, "startSeconds", pullForwardSeconds);
                ShiftArrayFloatField(commandos, "attackStartSeconds", pullForwardSeconds);
                ShiftArrayFloatField(commandos, "hitStartSeconds", pullForwardSeconds);
                ShiftArrayFloatField(commandos, "endSeconds", pullForwardSeconds);
                SerializedProperty timedObjects = serialized.FindProperty("timedObjects");
                ShiftArrayFloatField(timedObjects, "startSeconds", pullForwardSeconds);
                ShiftArrayFloatField(timedObjects, "endSeconds", pullForwardSeconds);
                ShiftFloatProperty(serialized.FindProperty("explosionStartSeconds"), pullForwardSeconds);
                ShiftFloatArray(serialized.FindProperty("impactCueSeconds"), pullForwardSeconds);
                serialized.ApplyModifiedPropertiesWithoutUndo();
                bridge.Sample(0f);
                EditorUtility.SetDirty(bridge);
            }
        }

        private static void ShiftProfileTiming(CinematicSequenceProfile profile, float pullForwardSeconds)
        {
            SerializedObject serialized = new SerializedObject(profile);
            SerializedProperty duration = serialized.FindProperty("authoredDurationSeconds");
            if (duration != null)
            {
                duration.floatValue = Mathf.Max(0.01f, duration.floatValue - pullForwardSeconds);
            }

            ShiftArrayFloatField(serialized.FindProperty("cameraCues"), "startSeconds", pullForwardSeconds);
            ShiftArrayFloatField(serialized.FindProperty("actorCues"), "startSeconds", pullForwardSeconds);
            ShiftArrayFloatField(serialized.FindProperty("vfxCues"), "startSeconds", pullForwardSeconds);
            ShiftArrayFloatField(serialized.FindProperty("tutorialCues"), "startSeconds", pullForwardSeconds);
            SerializedProperty gameplayHandoff = serialized.FindProperty("gameplayHandoff");
            if (gameplayHandoff != null)
            {
                ShiftFloatProperty(gameplayHandoff.FindPropertyRelative("startSeconds"), pullForwardSeconds);
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static void ShiftArrayFloatField(
            SerializedProperty array,
            string fieldName,
            float pullForwardSeconds)
        {
            if (array == null || !array.isArray)
            {
                return;
            }

            for (int i = 0; i < array.arraySize; i++)
            {
                ShiftFloatProperty(array.GetArrayElementAtIndex(i).FindPropertyRelative(fieldName), pullForwardSeconds);
            }
        }

        private static void ShiftFloatArray(SerializedProperty array, float pullForwardSeconds)
        {
            if (array == null || !array.isArray)
            {
                return;
            }

            for (int i = 0; i < array.arraySize; i++)
            {
                ShiftFloatProperty(array.GetArrayElementAtIndex(i), pullForwardSeconds);
            }
        }

        private static void ShiftFloatProperty(SerializedProperty property, float pullForwardSeconds)
        {
            if (property == null || property.propertyType != SerializedPropertyType.Float)
            {
                return;
            }

            if (property.floatValue >= TailShiftStartSeconds - Epsilon)
            {
                property.floatValue = Mathf.Max(0f, property.floatValue - pullForwardSeconds);
            }
        }

        private static List<string> ValidateSetup(TimelineAsset timeline, CinematicSequenceProfile profile)
        {
            List<string> issues = new List<string>();
            double openingStart = FindClipStart(timeline, OpeningDollyClipName);
            if (Math.Abs(openingStart - TargetPreludeDurationSeconds) > 0.01d)
            {
                issues.Add($"Opening dolly starts at {openingStart:0.000}s, expected {TargetPreludeDurationSeconds:0.000}s.");
            }

            if (profile.AuthoredDurationSeconds > 0f
                && Math.Abs(profile.AuthoredDurationSeconds - (float)timeline.fixedDuration) > 0.05f)
            {
                issues.Add("Combined profile authored duration is not aligned with the shortened Timeline.");
            }

            AnimationClip bombDropClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(BombDropClipPath);
            if (bombDropClip == null)
            {
                issues.Add("Missing shortened bomb-drop animation clip.");
            }

            return issues;
        }

        private static void WriteReport(
            IReadOnlyCollection<string> issues,
            double openingStartBefore,
            double openingStartAfter,
            float pullForwardSeconds)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# Intro GatePod Bomb Drop Tempo Polish");
            builder.AppendLine();
            builder.AppendLine(issues.Count == 0 ? "Status: PASS" : "Status: FAIL");
            builder.AppendLine();
            builder.AppendLine($"- Bomb release remains: `{BombReleaseStartSeconds:0.000}s`");
            builder.AppendLine($"- Bomb impact moved: `{OldBombImpactSeconds:0.000}s` -> `{TargetBombImpactSeconds:0.000}s`");
            builder.AppendLine($"- Pull-forward applied this run: `{pullForwardSeconds:0.000}s`");
            builder.AppendLine($"- Opening dolly start: `{openingStartBefore:0.000}s` -> `{openingStartAfter:0.000}s`");
            builder.AppendLine($"- Timeline: `{OlympusCombinedTimelinePath}`");
            builder.AppendLine($"- Scene: `{OlympusStageScenePath}`");
            if (issues.Count > 0)
            {
                builder.AppendLine();
                builder.AppendLine("## Issues");
                foreach (string issue in issues)
                {
                    builder.AppendLine("- " + issue);
                }
            }

            File.WriteAllText(ReportPath, builder.ToString(), Encoding.UTF8);
        }

        private static void SetVector3Curves(AnimationClip clip, params (float Time, Vector3 Value)[] positions)
        {
            SetCurve(clip, "m_LocalPosition.x", LinearValueKeyed(positions[0].Value.x, positions.Select(p => (p.Time, p.Value.x)).ToArray()));
            SetCurve(clip, "m_LocalPosition.y", LinearValueKeyed(positions[0].Value.y, positions.Select(p => (p.Time, p.Value.y)).ToArray()));
            SetCurve(clip, "m_LocalPosition.z", LinearValueKeyed(positions[0].Value.z, positions.Select(p => (p.Time, p.Value.z)).ToArray()));
        }

        private static AnimationCurve LinearValueKeyed(float defaultValue, params (float Time, float Value)[] keys)
        {
            List<Keyframe> frames = new List<Keyframe>();
            if (keys.Length == 0 || keys[0].Time > 0.0001f)
            {
                frames.Add(new Keyframe(0f, defaultValue));
            }

            for (int i = 0; i < keys.Length; i++)
            {
                frames.Add(new Keyframe(keys[i].Time, keys[i].Value));
            }

            AnimationCurve curve = new AnimationCurve(frames.ToArray());
            for (int i = 0; i < curve.length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
                AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
            }

            return curve;
        }

        private static void SetCurve(AnimationClip clip, string propertyName, AnimationCurve curve)
        {
            EditorCurveBinding binding = EditorCurveBinding.FloatCurve(string.Empty, typeof(Transform), propertyName);
            AnimationUtility.SetEditorCurve(clip, binding, curve);
        }

        private static double FindClipStart(TimelineAsset timeline, string displayName)
        {
            foreach (TrackAsset track in timeline.GetOutputTracks())
            {
                if (track == null)
                {
                    continue;
                }

                foreach (TimelineClip clip in track.GetClips())
                {
                    if (string.Equals(clip.displayName, displayName, StringComparison.Ordinal))
                    {
                        return clip.start;
                    }
                }
            }

            throw new InvalidOperationException($"Timeline is missing clip `{displayName}`.");
        }

        private static T FindComponentInScene<T>(Scene scene)
            where T : Component
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                T component = roots[i].GetComponentInChildren<T>(includeInactive: true);
                if (component != null)
                {
                    return component;
                }
            }

            return null;
        }

        private static PlayableDirector FindDirectorBoundToTimeline(Scene scene, TimelineAsset timeline)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                PlayableDirector[] directors = roots[i].GetComponentsInChildren<PlayableDirector>(includeInactive: true);
                for (int j = 0; j < directors.Length; j++)
                {
                    if (directors[j].playableAsset == timeline)
                    {
                        return directors[j];
                    }
                }
            }

            return null;
        }

        private static T LoadRequired<T>(string assetPath)
            where T : UnityEngine.Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (asset == null)
            {
                throw new InvalidOperationException($"Missing asset `{assetPath}`.");
            }

            return asset;
        }
    }
}
