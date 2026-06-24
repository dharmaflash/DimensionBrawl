using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace DimensionBrawl.Editor
{
    public static class CinematicInoriFoundationVerifier
    {
        private const string SourcePrefabPath =
            "Assets/_Imported/AssetStore/RoloArt/Inori/Prefabs/Inori_MagicaCloth2_Costume1.prefab";
        private const string PromotedModelPath =
            "Assets/_Game/Art/Characters/Player/Inori/Models/Inori_Unity.fbx";
        private const string BodyControllerPath =
            "Assets/_Game/Art/Animations/Player/Inori/DB_Inori_Rifle_ActionFoundation.controller";
        private const string FaceControllerPath =
            "Assets/_Imported/AssetStore/RoloArt/Inori/FaceAnimations/Inorianim.controller";
        private const string FaceExpressionRoot =
            "Assets/_Imported/AssetStore/RoloArt/Inori/FaceAnimations/FaceExpressions";
        private const string KawaiiAnimationRoot =
            "Assets/_Imported/AssetStore/KAWAII_ANIMATIONS_100/Assets/Animations";
        private const string ReportPath =
            "C:/tmp/DimensionBrawl-CinematicInoriFoundationVerifier.md";

        private static readonly string[] RequiredFaceClips =
        {
            "Surprised",
            "Angry",
            "Confused",
            "CalmEye",
            "Smile",
            "Sad",
            "Joy"
        };

        private static readonly string[] RequiredBodyStates =
        {
            "R_Idle",
            "R_AimIdle",
            "R_Run",
            "R_Evade",
            "R_Shoot"
        };

        private static readonly string[] CandidateKawaiiClips =
        {
            "@KA_Idle03_LookAtHands.FBX",
            "@KA_Idle04_LookAtFeet.FBX",
            "@KA_Idle11_LookingBack.FBX",
            "@KA_Idle17_StumbleAndFall.FBX",
            "@KA_Idle29_Surprised.FBX",
            "@KA_Idle31_PickUp.FBX",
            "@KA_Dash_Fwd.FBX",
            "@KA_Combat_Witch_Magic_Awakening.FBX",
            "@KA_Combat_Witch_Magic_Shot.FBX",
            "@KA_Combat_Witch_Magic_Impact.FBX",
            "@KA_Combat_OHSword01_ChargeAttack.FBX",
            "@KA_Combat_OHSword01_Idle01.FBX"
        };

        [MenuItem("DimensionBrawl/Cinematics/Verify Inori Foundation")]
        public static void VerifyInoriFoundationMenu()
        {
            VerifyInoriFoundation();
            Debug.Log($"Wrote cinematic Inori foundation report to {ReportPath}.");
        }

        public static void RunBatchVerification()
        {
            bool passed = VerifyInoriFoundation();
            if (!passed)
            {
                EditorApplication.Exit(1);
            }
        }

        public static bool VerifyInoriFoundation()
        {
            List<string> report = new List<string>
            {
                "# Cinematic Inori Foundation Verification",
                string.Empty,
                $"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                string.Empty
            };

            VerificationState state = new VerificationState(report);

            VerifyPromotedModel(state);
            VerifySourcePrefab(state);
            VerifyBodyController(state);
            VerifyFaceController(state);
            VerifyFaceClips(state);
            VerifyKawaiiCandidates(state);

            report.Add("## Result");
            report.Add(string.Empty);
            report.Add(state.FailCount == 0 ? "PASS" : "FAIL");
            report.Add(string.Empty);
            report.Add($"Failures: {state.FailCount}");
            report.Add($"Warnings: {state.WarningCount}");
            report.Add(string.Empty);

            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath) ?? "C:/tmp");
            File.WriteAllLines(ReportPath, report);
            AssetDatabase.Refresh();

            if (state.FailCount > 0)
            {
                Debug.LogError($"Cinematic Inori foundation verification failed. See {ReportPath}.");
                return false;
            }

            Debug.Log($"Cinematic Inori foundation verification passed. See {ReportPath}.");
            return true;
        }

        private static void VerifyPromotedModel(VerificationState state)
        {
            state.Header("Promoted Inori Model");
            GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(PromotedModelPath);
            state.Check(model != null, $"Model exists: `{PromotedModelPath}`");

            ModelImporter importer = AssetImporter.GetAtPath(PromotedModelPath) as ModelImporter;
            state.Check(importer != null, "ModelImporter exists.");
            if (importer != null)
            {
                state.Check(importer.animationType == ModelImporterAnimationType.Human, "Promoted model imports as Humanoid.");
                state.Check(importer.avatarSetup == ModelImporterAvatarSetup.CreateFromThisModel, "Promoted model creates its Avatar from this model.");
                state.Check(!importer.importAnimation, "Promoted model does not import body animation.");
                state.Check(importer.importBlendShapes, "Promoted model imports blend shapes for facial animation.");
            }

            Avatar avatar = LoadPromotedAvatar();
            state.Check(avatar != null, "Valid promoted humanoid Avatar exists.");
            if (avatar != null)
            {
                state.Check(avatar.isValid, "Promoted Avatar is valid.");
                state.Check(avatar.isHuman, "Promoted Avatar is human.");
            }

            state.Blank();
        }

        private static Avatar LoadPromotedAvatar()
        {
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(PromotedModelPath);
            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] is Avatar avatar && avatar.isHuman && avatar.isValid)
                {
                    return avatar;
                }
            }

            return null;
        }

        private static void VerifySourcePrefab(VerificationState state)
        {
            state.Header("Source Inori Prefab");
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(SourcePrefabPath);
            state.Check(prefab != null, $"Source prefab exists: `{SourcePrefabPath}`");
            if (prefab == null)
            {
                state.Blank();
                return;
            }

            Animator animator = prefab.GetComponentInChildren<Animator>(true);
            state.Check(animator != null, "Source prefab has an Animator.");
            if (animator != null)
            {
                state.Check(animator.avatar != null && animator.avatar.isHuman && animator.avatar.isValid, "Source prefab Animator has a valid human Avatar.");
                state.Warn(animator.runtimeAnimatorController == null, "Source prefab has no controller assigned; our setup must assign a gameplay/cutscene controller.");
            }

            SkinnedMeshRenderer[] renderers = prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            int blendShapeRendererCount = renderers.Count(renderer => renderer.sharedMesh != null && renderer.sharedMesh.blendShapeCount > 0);
            int blendShapeCount = renderers.Sum(renderer => renderer.sharedMesh != null ? renderer.sharedMesh.blendShapeCount : 0);
            state.Check(renderers.Length > 0, $"Source prefab has skinned renderers: {renderers.Length}.");
            state.Check(blendShapeRendererCount > 0, $"Source prefab has blend-shape renderers: {blendShapeRendererCount}, blend shapes: {blendShapeCount}.");

            Transform[] transforms = prefab.GetComponentsInChildren<Transform>(true);
            int magicaClothObjects = transforms.Count(transform => transform.name.IndexOf("Magica Cloth", StringComparison.OrdinalIgnoreCase) >= 0);
            int magicaColliderObjects = transforms.Count(transform => transform.name.IndexOf("Magica", StringComparison.OrdinalIgnoreCase) >= 0 &&
                                                                      transform.name.IndexOf("Collider", StringComparison.OrdinalIgnoreCase) >= 0);
            state.Check(magicaClothObjects > 0, $"Source prefab has Magica Cloth objects: {magicaClothObjects}.");
            state.Check(magicaColliderObjects > 0, $"Source prefab has Magica collider objects: {magicaColliderObjects}.");
            state.Check(transforms.Any(transform => string.Equals(transform.name, "hand.r", StringComparison.OrdinalIgnoreCase)), "Right hand bone `hand.r` exists.");
            state.Check(transforms.Any(transform => string.Equals(transform.name, "hand.l", StringComparison.OrdinalIgnoreCase)), "Left hand bone `hand.l` exists.");

            state.Blank();
        }

        private static void VerifyBodyController(VerificationState state)
        {
            state.Header("Body Animator Controller");
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(BodyControllerPath);
            state.Check(controller != null, $"Body controller exists: `{BodyControllerPath}`");
            if (controller == null)
            {
                state.Blank();
                return;
            }

            HashSet<string> stateNames = new HashSet<string>(GetStateNames(controller), StringComparer.Ordinal);
            foreach (string requiredState in RequiredBodyStates)
            {
                state.Check(stateNames.Contains(requiredState), $"Required body state exists: `{requiredState}`.");
            }

            int motionCount = GetStateMotions(controller).Count(motion => motion != null);
            state.Check(motionCount >= RequiredBodyStates.Length, $"Body controller has bound motions: {motionCount}.");
            state.Blank();
        }

        private static void VerifyFaceController(VerificationState state)
        {
            state.Header("Face Animator Controller");
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(FaceControllerPath);
            state.Check(controller != null, $"Face controller exists: `{FaceControllerPath}`");
            if (controller == null)
            {
                state.Blank();
                return;
            }

            HashSet<string> stateNames = new HashSet<string>(GetStateNames(controller), StringComparer.Ordinal);
            foreach (string requiredClip in RequiredFaceClips)
            {
                state.Check(stateNames.Contains(requiredClip), $"Required face state exists: `{requiredClip}`.");
            }

            state.Blank();
        }

        private static void VerifyFaceClips(VerificationState state)
        {
            state.Header("Face Expression Clips");
            foreach (string clipName in RequiredFaceClips)
            {
                string path = $"{FaceExpressionRoot}/{clipName}.anim";
                AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                state.Check(clip != null, $"Face clip exists: `{clipName}`.");
                if (clip == null)
                {
                    continue;
                }

                EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(clip);
                int blendShapeCurveCount = curveBindings.Count(binding => binding.propertyName.StartsWith("blendShape.", StringComparison.Ordinal));
                bool targetsBody = curveBindings.Any(binding => string.Equals(binding.path, "Body", StringComparison.Ordinal));
                state.Check(blendShapeCurveCount > 0, $"Face clip `{clipName}` has blend-shape curves: {blendShapeCurveCount}.");
                state.Check(targetsBody, $"Face clip `{clipName}` targets `Body`.");
            }

            state.Blank();
        }

        private static void VerifyKawaiiCandidates(VerificationState state)
        {
            state.Header("Kawaii Animation Candidate Clips");
            foreach (string clipFile in CandidateKawaiiClips)
            {
                string path = $"{KawaiiAnimationRoot}/{clipFile}";
                ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
                state.Check(importer != null, $"Candidate exists and has ModelImporter: `{clipFile}`.");
                if (importer == null)
                {
                    continue;
                }

                state.Check(importer.importAnimation, $"Candidate imports animation: `{clipFile}`.");
                state.Check(importer.animationType == ModelImporterAnimationType.Human, $"Candidate is Humanoid: `{clipFile}`.");

                AnimationClip[] clips = AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>()
                    .Where(clip => !clip.name.StartsWith("__preview__", StringComparison.Ordinal))
                    .ToArray();
                state.Check(clips.Length > 0, $"Candidate exposes animation clips: `{clipFile}` count={clips.Length}.");
            }

            state.Blank();
        }

        private static IEnumerable<string> GetStateNames(AnimatorController controller)
        {
            foreach (AnimatorControllerLayer layer in controller.layers)
            {
                foreach (ChildAnimatorState state in layer.stateMachine.states)
                {
                    yield return state.state.name;
                }
            }
        }

        private static IEnumerable<Motion> GetStateMotions(AnimatorController controller)
        {
            foreach (AnimatorControllerLayer layer in controller.layers)
            {
                foreach (ChildAnimatorState state in layer.stateMachine.states)
                {
                    yield return state.state.motion;
                }
            }
        }

        private sealed class VerificationState
        {
            private readonly List<string> report;

            public VerificationState(List<string> report)
            {
                this.report = report;
            }

            public int FailCount { get; private set; }
            public int WarningCount { get; private set; }

            public void Header(string title)
            {
                report.Add($"## {title}");
                report.Add(string.Empty);
            }

            public void Blank()
            {
                report.Add(string.Empty);
            }

            public void Check(bool condition, string message)
            {
                if (condition)
                {
                    report.Add($"- PASS: {message}");
                    return;
                }

                FailCount++;
                report.Add($"- FAIL: {message}");
            }

            public void Warn(bool condition, string message)
            {
                if (!condition)
                {
                    return;
                }

                WarningCount++;
                report.Add($"- WARN: {message}");
            }
        }
    }
}
