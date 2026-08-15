using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DimensionBrawl.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DimensionBrawl.Editor.Review.Cinematics
{
    /// <summary>
    /// Builds an editor-only proof that the C29 eye-opening camera grammar can be
    /// separated from its source actor and performed by DimensionBrawl's Inori.
    /// The source actor, environment, audio, effects, and completed timeline are
    /// intentionally excluded.
    /// </summary>
    public static class C29CloseupRetargetReviewSetup
    {
        public const string ReviewRoot =
            "Assets/_Game/Editor/Review/Cinematics/C29CloseupRetarget";
        public const string ScenePath = ReviewRoot + "/C29CloseupRetargetInoriReview.unity";
        public const string ProfilePath = ReviewRoot + "/DB_Cinematic_C29CloseupGrammar_Inori.asset";
        public const string EyesClosedMeshPath = ReviewRoot + "/DB_C29_InoriEyesClosed.asset";
        public const string EyesOpenMeshPath = ReviewRoot + "/DB_C29_InoriEyesOpen.asset";
        public const string CameraAssetPath =
            "Assets/_Game/Art/Animations/Cinematics/LegacyCameraGrammar/C29_cam.fbx";

        private const string CanonicalCorridorScenePath =
            "Assets/_Game/Scenes/OlympusCorridorInvasionStage.unity";
        private const string CanonicalInoriObjectName = "IntroGatePodReview_Inori";
        private const string InoriControllerPath =
            "Assets/_Game/Art/Animations/Cinematics/Inori/DB_Inori_CinematicP0.controller";
        private const string CameraCueId = "legacy_c29_camera_grammar";
        private const string InoriName = "C29Retarget_Inori";
        private const string MainCameraName = "C29Retarget_MainCamera";
        private const string SourceCameraWrapperName = "C29Retarget_SourceCameraWrapper";
        private const string RunnerName = "C29Retarget_SequenceRunner";
        private const string EyesClosed = "EyesClosed";
        private const string EyesOpen = "EyesOpen";
        private const string BodyHoldState = "CIN_CombatReady";
        private const float AuthoredDurationSeconds = 4.5f;
        private const float EyeOpenSeconds = 1.95f;
        private const float FramingReferenceSeconds = 0f;
        private const float SourceEyeForwardMeters = 1.138f;
        // Preserve C29's lens and dolly, but lower the eye line for Inori's shorter
        // face/torso proportions so both eyelids remain inside a 16:9 title-safe frame.
        private const float SourceEyeUpMeters = 0.095f;
        private const float SourceEyeRightMeters = -0.0387f;
        private const string CaptureDirectory = "C:/tmp/DimensionBrawl-C29CloseupRetarget-Frames";
        private const string VideoFrameDirectory = "C:/tmp/DimensionBrawl-C29CloseupRetarget-VideoFrames";
        private const string ContactSheetPath = "C:/tmp/DimensionBrawl-C29CloseupRetarget-ContactSheet.png";
        private const string ReportPath = "C:/tmp/DimensionBrawl-C29CloseupRetarget-Report.md";

        private static readonly float[] CaptureTimes = { 0.15f, 1.45f, 1.95f, 2.35f, 4.2f };
        private static readonly string[] CaptureLabels =
        {
            "closed-establish",
            "closed-hold",
            "eye-open-beat",
            "focused-reveal",
            "settle"
        };

        [MenuItem("DimensionBrawl/Review/Cinematics/Rebuild C29 Closeup Retarget Review")]
        public static void RebuildReviewMenu()
        {
            BuildReview(openScene: true);
            Debug.Log($"C29 closeup retarget review rebuilt at {ScenePath}.");
        }

        [MenuItem("DimensionBrawl/Review/Cinematics/Capture C29 Closeup Retarget Frames")]
        public static void CaptureReviewMenu()
        {
            BuildReview(openScene: true);
            CaptureReviewFrames();
            Debug.Log($"C29 closeup retarget contact sheet written to {ContactSheetPath}.");
        }

        [MenuItem("DimensionBrawl/Review/Cinematics/Capture C29 Closeup Retarget Video Frames")]
        public static void CaptureVideoFramesMenu()
        {
            BuildReview(openScene: true);
            CaptureReviewVideoFrames();
            Debug.Log($"C29 closeup retarget video frames written to {VideoFrameDirectory}.");
        }

        public static void RunBatchSetup()
        {
            RunBatch(() => BuildReview(openScene: false));
        }

        public static void RunBatchCapture()
        {
            RunBatch(() =>
            {
                BuildReview(openScene: true);
                CaptureReviewFrames();
            });
        }

        public static void RunBatchVideoFrames()
        {
            RunBatch(() =>
            {
                BuildReview(openScene: true);
                CaptureReviewVideoFrames();
            });
        }

        public static void BuildReview(bool openScene)
        {
            EnsureFolder(ReviewRoot);
            AssetDatabase.ImportAsset(CameraAssetPath, ImportAssetOptions.ForceUpdate);

            AnimationClip cameraClip = LoadPrimaryClip(CameraAssetPath);
            if (cameraClip == null)
            {
                throw new InvalidOperationException($"C29 camera clip missing at {CameraAssetPath}.");
            }

            CinematicSequenceProfile profile = EnsureProfile(cameraClip);
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "C29CloseupRetargetInoriReview";

            GameObject inori = InstantiateIsolatedInori(scene);
            Animator animator = inori.GetComponent<Animator>()
                ?? inori.GetComponentInChildren<Animator>(includeInactive: true)
                ?? throw new InvalidOperationException("The isolated Inori has no Animator.");
            RuntimeAnimatorController controller =
                AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(InoriControllerPath)
                ?? throw new InvalidOperationException($"Inori cinematic controller missing at {InoriControllerPath}.");
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
            animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

            CinematicBlendShapeExpressionPlayer expressionPlayer =
                inori.GetComponent<CinematicBlendShapeExpressionPlayer>()
                ?? inori.AddComponent<CinematicBlendShapeExpressionPlayer>();
            ConfigureEyes(expressionPlayer, inori);
            expressionPlayer.ApplyExpressionImmediate(EyesClosed);

            Transform head = animator.GetBoneTransform(HumanBodyBones.Head)
                ?? throw new InvalidOperationException("The isolated Inori humanoid has no Head bone.");
            Transform leftEye = animator.GetBoneTransform(HumanBodyBones.LeftEye);
            Transform rightEye = animator.GetBoneTransform(HumanBodyBones.RightEye);

            GameObject sourceWrapper = new GameObject(SourceCameraWrapperName);
            GameObject sourceRig = InstantiateAsset(CameraAssetPath, sourceWrapper.transform);
            sourceRig.name = "C29_cam";
            Camera sourceCamera = sourceRig.GetComponentInChildren<Camera>(includeInactive: true)
                ?? throw new InvalidOperationException("Imported C29 camera asset has no Camera component.");
            sourceCamera.enabled = false;
            sourceCamera.gameObject.SetActive(true);

            cameraClip.SampleAnimation(sourceRig, FramingReferenceSeconds);
            OrientActorTowardSourceCamera(inori.transform, sourceCamera.transform);
            animator.Play(BodyHoldState, 0, 0f);
            animator.Update(0f);
            AlignCameraWrapperToEyes(
                sourceWrapper.transform,
                sourceCamera.transform,
                ResolveEyeMidpoint(head, leftEye, rightEye));
            CreateBakedEyeMeshes(
                inori,
                out SkinnedMeshRenderer faceRenderer,
                out Mesh eyesClosedMesh,
                out Mesh eyesOpenMesh);

            C29RetargetHeadPoseDriver headPoseDriver =
                inori.GetComponent<C29RetargetHeadPoseDriver>()
                ?? inori.AddComponent<C29RetargetHeadPoseDriver>();
            headPoseDriver.Configure(
                animator,
                faceRenderer,
                eyesClosedMesh,
                eyesOpenMesh,
                EyeOpenSeconds,
                1.55f,
                0.78f,
                -3.5f,
                -11f);

            GameObject mainCameraObject = new GameObject(MainCameraName);
            mainCameraObject.tag = "MainCamera";
            Camera mainCamera = mainCameraObject.AddComponent<Camera>();
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = new Color(0.007f, 0.012f, 0.025f, 1f);
            mainCamera.nearClipPlane = 0.025f;
            mainCamera.farClipPlane = 250f;
            mainCamera.allowHDR = true;
            mainCamera.allowMSAA = true;
            ApplySourceCameraSample(cameraClip, sourceRig, sourceCamera, mainCamera, 0f);

            CreateLighting(inori.transform, head);
            CreateBackdrop(head, sourceCamera.transform);
            CreateFadeOverlay(out CanvasGroup fadeGroup, out Image fadeImage);

            GameObject runnerObject = new GameObject(RunnerName);
            CinematicSequenceRunner runner = runnerObject.AddComponent<CinematicSequenceRunner>();
            ConfigureRunner(
                runner,
                profile,
                animator,
                expressionPlayer,
                inori.transform,
                mainCamera,
                sourceRig,
                sourceCamera,
                controller,
                fadeGroup,
                fadeImage);

            CinematicSequenceAutoPlay autoPlay = runnerObject.AddComponent<CinematicSequenceAutoPlay>();
            SerializedObject autoPlayObject = new SerializedObject(autoPlay);
            SetObject(autoPlayObject, "runner", runner);
            SetBool(autoPlayObject, "playOnStart", true);
            SetFloat(autoPlayObject, "startDelaySeconds", 0.1f);
            autoPlayObject.ApplyModifiedPropertiesWithoutUndo();

            CreateReviewMarker();
            ValidateScene(scene, profile, cameraClip, inori, sourceCamera, mainCamera, expressionPlayer);

            EditorUtility.SetDirty(animator);
            EditorUtility.SetDirty(expressionPlayer);
            EditorUtility.SetDirty(headPoseDriver);
            EditorUtility.SetDirty(runner);
            EditorUtility.SetDirty(autoPlay);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
            {
                throw new InvalidOperationException($"Failed to save C29 retarget review scene at {ScenePath}.");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (!openScene)
            {
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            }
        }

        private static CinematicSequenceProfile EnsureProfile(AnimationClip cameraClip)
        {
            CinematicSequenceProfile profile =
                AssetDatabase.LoadAssetAtPath<CinematicSequenceProfile>(ProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CinematicSequenceProfile>();
                AssetDatabase.CreateAsset(profile, ProfilePath);
            }

            profile.Configure(
                "c29_closeup_grammar_inori_review",
                "C29 Closeup Grammar - Inori Retarget Review",
                CinematicSequenceProfile.SequenceCategory.BossIntro,
                "Editor-only proof: camera motion isolated from TPK C29 and re-performed by Inori with DimensionBrawl facial/body animation. No source actor, audio, VFX, environment, or completed timeline.",
                AuthoredDurationSeconds,
                100,
                newLockMovement: true,
                newLockInput: true,
                newHideHud: true,
                newCanSkip: true,
                newUseUnscaledClock: true,
                Array.Empty<CinematicSequenceProfile.CameraCue>(),
                new[]
                {
                    new CinematicSequenceProfile.ActorCue(
                        "inori_closeup_body_hold",
                        CinematicSequenceProfile.ActorRole.Inori,
                        CinematicSequenceProfile.ActorCueKind.BodyState,
                        0f,
                        0.08f,
                        BodyHoldState),
                    new CinematicSequenceProfile.ActorCue(
                        "inori_eyes_closed",
                        CinematicSequenceProfile.ActorRole.Inori,
                        CinematicSequenceProfile.ActorCueKind.FaceState,
                        0f,
                        0.08f,
                        EyesClosed,
                        faceStateName: EyesClosed),
                    new CinematicSequenceProfile.ActorCue(
                        "inori_eyes_open",
                        CinematicSequenceProfile.ActorRole.Inori,
                        CinematicSequenceProfile.ActorCueKind.FaceState,
                        EyeOpenSeconds,
                        0.42f,
                        EyesOpen,
                        faceStateName: EyesOpen)
                },
                Array.Empty<CinematicSequenceProfile.VfxCue>(),
                Array.Empty<CinematicSequenceProfile.TutorialCue>(),
                default);
            profile.ConfigureSourceCameraAnimation(
                new CinematicSequenceProfile.SourceCameraAnimationCue(
                    CameraCueId,
                    cameraClip,
                    0f,
                    0f,
                    Mathf.Min(AuthoredDurationSeconds, cameraClip.length)));
            profile.ConfigureSourceActorAnimations(Array.Empty<CinematicSequenceProfile.SourceActorAnimationCue>());
            profile.ConfigureSourceActorGrades(Array.Empty<CinematicSequenceProfile.SourceActorGradeCue>());
            profile.ConfigureScreenFades(new[]
            {
                new CinematicSequenceProfile.ScreenFadeCue(
                    "closeup_fade_from_black",
                    0f,
                    0.38f,
                    Color.black,
                    1f,
                    0f)
            });
            profile.ConfigureStageContext(null, string.Empty, string.Empty, string.Empty,
                "Editor-only review; deliberately excluded from product stage routing.", false);
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static GameObject InstantiateIsolatedInori(Scene destinationScene)
        {
            Scene sourceScene = EditorSceneManager.OpenScene(CanonicalCorridorScenePath, OpenSceneMode.Additive);
            try
            {
                GameObject source = sourceScene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                    .Select(transform => transform.gameObject)
                    .FirstOrDefault(candidate => string.Equals(
                        candidate.name,
                        CanonicalInoriObjectName,
                        StringComparison.Ordinal));
                if (source == null)
                {
                    throw new InvalidOperationException(
                        $"Canonical Inori object {CanonicalInoriObjectName} is missing from {CanonicalCorridorScenePath}.");
                }

                GameObject inori = UnityEngine.Object.Instantiate(source);
                SceneManager.MoveGameObjectToScene(inori, destinationScene);
                inori.name = InoriName;
                inori.transform.SetParent(null, worldPositionStays: false);
                inori.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                inori.transform.localScale = Vector3.one;
                inori.SetActive(true);

                MonoBehaviour[] behaviours = inori.GetComponentsInChildren<MonoBehaviour>(true);
                for (int i = 0; i < behaviours.Length; i++)
                {
                    if (behaviours[i] is CinematicBlendShapeExpressionPlayer)
                    {
                        continue;
                    }

                    UnityEngine.Object.DestroyImmediate(behaviours[i]);
                }

                CinematicBlendShapeExpressionPlayer expressionPlayer =
                    inori.GetComponent<CinematicBlendShapeExpressionPlayer>();
                if (expressionPlayer == null)
                {
                    throw new InvalidOperationException(
                        "Canonical Inori clone lost its expression renderer contract.");
                }

                HashSet<string> activeCostumeRenderers = new HashSet<string>(StringComparer.Ordinal)
                {
                    "Inner_Bottom",
                    "BodyBase",
                    "Hair",
                    "Costume1_Dress",
                    "Body",
                    "Costume1_HairAcc",
                    "Costume1_Jacket",
                    "Costume1_Shoes"
                };
                Renderer[] renderers = inori.GetComponentsInChildren<Renderer>(true);
                for (int i = 0; i < renderers.Length; i++)
                {
                    Renderer renderer = renderers[i];
                    bool shouldRender = activeCostumeRenderers.Contains(renderer.gameObject.name);
                    renderer.enabled = shouldRender;
                    if (!shouldRender)
                    {
                        renderer.gameObject.SetActive(false);
                        continue;
                    }

                    Transform cursor = renderer.transform;
                    while (cursor != null && cursor != inori.transform)
                    {
                        cursor.gameObject.SetActive(true);
                        cursor = cursor.parent;
                    }
                }

                return inori;
            }
            finally
            {
                EditorSceneManager.CloseScene(sourceScene, removeScene: true);
                SceneManager.SetActiveScene(destinationScene);
            }
        }

        private static void ConfigureEyes(
            CinematicBlendShapeExpressionPlayer expressionPlayer,
            GameObject inori)
        {
            string[] blinkCandidates =
            {
                "eyeBlinkLeft",
                "eyeBlinkRight",
                "Inori_Blink",
                "Inori_Blink_L",
                "Inori_Blink_R"
            };
            string[] blinkShapes = blinkCandidates
                .Where(candidate => HasBlendShape(inori, candidate))
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            if (blinkShapes.Length < 2)
            {
                throw new InvalidOperationException(
                    "Inori retarget review requires authored eyelid blend shapes.");
            }

            string[] authoredEyelids = blinkShapes
                .Where(shape => string.Equals(shape, "eyeBlinkLeft", StringComparison.Ordinal)
                    || string.Equals(shape, "eyeBlinkRight", StringComparison.Ordinal))
                .ToArray();
            if (authoredEyelids.Length < 2)
            {
                authoredEyelids = blinkShapes
                    .Where(shape => string.Equals(shape, "Inori_Blink_L", StringComparison.Ordinal)
                        || string.Equals(shape, "Inori_Blink_R", StringComparison.Ordinal))
                    .ToArray();
            }

            CinematicBlendShapeExpressionPlayer.ShapeWeight[] closedWeights = authoredEyelids
                .Select(shape => new CinematicBlendShapeExpressionPlayer.ShapeWeight(shape, 100f))
                .ToArray();
            CinematicBlendShapeExpressionPlayer.ShapeWeight[] openWeights = blinkShapes
                .Select(shape => new CinematicBlendShapeExpressionPlayer.ShapeWeight(shape, 0f))
                .ToArray();

            expressionPlayer.Configure(new[]
            {
                new CinematicBlendShapeExpressionPlayer.ExpressionPreset(
                    EyesClosed,
                    closedWeights),
                new CinematicBlendShapeExpressionPlayer.ExpressionPreset(
                    EyesOpen,
                    openWeights)
            });

            SerializedObject serialized = new SerializedObject(expressionPlayer);
            SetBool(serialized, "resetPresetShapesBeforePlay", true);
            SetFloat(serialized, "blendSpeed", 7.5f);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureRunner(
            CinematicSequenceRunner runner,
            CinematicSequenceProfile profile,
            Animator animator,
            CinematicBlendShapeExpressionPlayer expressionPlayer,
            Transform inori,
            Camera mainCamera,
            GameObject sourceRig,
            Camera sourceCamera,
            RuntimeAnimatorController controller,
            CanvasGroup fadeGroup,
            Image fadeImage)
        {
            SerializedObject serialized = new SerializedObject(runner);
            SetObject(serialized, "sequenceProfile", profile);
            SetObject(serialized, "bodyControllerOverride", controller);
            SetObject(serialized, "cinematicCamera", mainCamera);
            SetBool(serialized, "driveCameraTransformFromProfile", true);
            SetBool(serialized, "disableActionCameraControllerDuringPoseDrive", true);
            SetObject(serialized, "sourceCameraRigRoot", sourceRig);
            SetObject(serialized, "sourceCameraTransform", sourceCamera.transform);
            SetObject(serialized, "sourceCameraComponent", sourceCamera);
            SetObject(serialized, "cueSpace", inori);
            SetObject(serialized, "screenFadeCanvasGroup", fadeGroup);
            SetObject(serialized, "screenFadeImage", fadeImage);
            SetFloat(serialized, "maxPlaybackDeltaSeconds", 1f / 60f);

            SerializedProperty bindings = Require(serialized, "actorBindings");
            bindings.arraySize = 1;
            SerializedProperty binding = bindings.GetArrayElementAtIndex(0);
            RequireRelative(binding, "role").enumValueIndex =
                (int)CinematicSequenceProfile.ActorRole.Inori;
            RequireRelative(binding, "bodyAnimator").objectReferenceValue = animator;
            RequireRelative(binding, "faceAnimator").objectReferenceValue = animator;
            RequireRelative(binding, "expressionPlayer").objectReferenceValue = expressionPlayer;
            RequireRelative(binding, "anchor").objectReferenceValue = inori;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void OrientActorTowardSourceCamera(Transform inori, Transform sourceCamera)
        {
            // Inori's authored face axis is +Z. Aim it back toward the sampled
            // source camera while preserving the C29 camera curve itself.
            Vector3 facing = Vector3.ProjectOnPlane(-sourceCamera.forward, Vector3.up);
            if (facing.sqrMagnitude > 0.0001f)
            {
                inori.rotation = Quaternion.LookRotation(facing.normalized, Vector3.up);
            }
        }

        private static Vector3 ResolveEyeMidpoint(
            Transform head,
            Transform leftEye,
            Transform rightEye)
        {
            if (leftEye != null && rightEye != null)
            {
                return (leftEye.position + rightEye.position) * 0.5f;
            }

            return head.position + (head.forward * 0.085f) + (head.up * 0.035f);
        }

        private static void AlignCameraWrapperToEyes(
            Transform wrapper,
            Transform sourceCamera,
            Vector3 eyeMidpoint)
        {
            Vector3 sourceEyeFramingPoint = sourceCamera.position
                + (sourceCamera.forward * SourceEyeForwardMeters)
                + (sourceCamera.up * SourceEyeUpMeters)
                + (sourceCamera.right * SourceEyeRightMeters);
            wrapper.position += eyeMidpoint - sourceEyeFramingPoint;
        }

        private static void ApplySourceCameraSample(
            AnimationClip clip,
            GameObject rigRoot,
            Camera source,
            Camera destination,
            float seconds)
        {
            clip.SampleAnimation(rigRoot, Mathf.Clamp(seconds, 0f, clip.length));
            destination.transform.SetPositionAndRotation(source.transform.position, source.transform.rotation);
            destination.fieldOfView = source.fieldOfView;
        }

        private static void CreateBakedEyeMeshes(
            GameObject inori,
            out SkinnedMeshRenderer faceRenderer,
            out Mesh eyesClosedMesh,
            out Mesh eyesOpenMesh)
        {
            faceRenderer = inori.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true)
                .SingleOrDefault(renderer => string.Equals(
                    renderer.gameObject.name,
                    "Body",
                    StringComparison.Ordinal))
                ?? throw new InvalidOperationException("C29 retarget review cannot resolve Inori's Body face renderer.");
            Mesh source = faceRenderer.sharedMesh
                ?? throw new InvalidOperationException("Inori's Body face renderer has no source mesh.");

            eyesClosedMesh = CreateBakedEyeMesh(
                source,
                EyesClosedMeshPath,
                "DB_C29_InoriEyesClosed",
                ("eyeBlinkLeft", 100f),
                ("eyeBlinkRight", 100f));
            eyesOpenMesh = CreateBakedEyeMesh(
                source,
                EyesOpenMeshPath,
                "DB_C29_InoriEyesOpen",
                ("eyeWideLeft", 16f),
                ("eyeWideRight", 16f));
            faceRenderer.sharedMesh = eyesClosedMesh;
        }

        private static Mesh CreateBakedEyeMesh(
            Mesh source,
            string assetPath,
            string assetName,
            params (string ShapeName, float Weight)[] shapes)
        {
            Mesh baked = UnityEngine.Object.Instantiate(source);
            baked.name = assetName;

            Vector3[] vertices = baked.vertices;
            Vector3[] normals = baked.normals;
            Vector4[] tangents = baked.tangents;
            int vertexCount = baked.vertexCount;
            Vector3[] deltaVertices = new Vector3[vertexCount];
            Vector3[] deltaNormals = new Vector3[vertexCount];
            Vector3[] deltaTangents = new Vector3[vertexCount];

            for (int shape = 0; shape < shapes.Length; shape++)
            {
                int shapeIndex = source.GetBlendShapeIndex(shapes[shape].ShapeName);
                if (shapeIndex < 0)
                {
                    UnityEngine.Object.DestroyImmediate(baked);
                    throw new InvalidOperationException(
                        $"Inori Body mesh is missing required blend shape {shapes[shape].ShapeName}.");
                }

                int frameIndex = source.GetBlendShapeFrameCount(shapeIndex) - 1;
                float frameWeight = source.GetBlendShapeFrameWeight(shapeIndex, frameIndex);
                float scale = Mathf.Approximately(frameWeight, 0f)
                    ? 0f
                    : shapes[shape].Weight / frameWeight;
                source.GetBlendShapeFrameVertices(
                    shapeIndex,
                    frameIndex,
                    deltaVertices,
                    deltaNormals,
                    deltaTangents);
                for (int vertex = 0; vertex < vertexCount; vertex++)
                {
                    vertices[vertex] += deltaVertices[vertex] * scale;
                    if (normals.Length == vertexCount)
                    {
                        normals[vertex] += deltaNormals[vertex] * scale;
                    }

                    if (tangents.Length == vertexCount)
                    {
                        Vector4 tangent = tangents[vertex];
                        Vector3 delta = deltaTangents[vertex] * scale;
                        tangents[vertex] = new Vector4(
                            tangent.x + delta.x,
                            tangent.y + delta.y,
                            tangent.z + delta.z,
                            tangent.w);
                    }
                }
            }

            baked.vertices = vertices;
            if (normals.Length == vertexCount)
            {
                baked.normals = normals;
            }

            if (tangents.Length == vertexCount)
            {
                baked.tangents = tangents;
            }

            baked.ClearBlendShapes();
            baked.RecalculateBounds();
            AssetDatabase.DeleteAsset(assetPath);
            AssetDatabase.CreateAsset(baked, assetPath);
            return baked;
        }

        private static void CreateLighting(Transform inori, Transform head)
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.075f, 0.095f, 0.16f, 1f);
            RenderSettings.reflectionIntensity = 0.25f;

            GameObject keyObject = new GameObject("C29Retarget_KeyLight");
            Light key = keyObject.AddComponent<Light>();
            key.type = LightType.Directional;
            key.color = new Color(0.74f, 0.84f, 1f, 1f);
            key.intensity = 1.5f;
            key.shadows = LightShadows.Soft;
            keyObject.transform.rotation = Quaternion.Euler(28f, -34f, 0f);

            GameObject rimObject = new GameObject("C29Retarget_EyeRimLight");
            Light rim = rimObject.AddComponent<Light>();
            rim.type = LightType.Point;
            rim.color = new Color(0.12f, 0.72f, 1f, 1f);
            rim.intensity = 7f;
            rim.range = 4f;
            rim.shadows = LightShadows.None;
            rimObject.transform.position = head.position + (inori.right * 0.75f) + (Vector3.up * 0.25f) - (inori.forward * 0.65f);
        }

        private static void CreateBackdrop(Transform head, Transform sourceCamera)
        {
            GameObject backdrop = GameObject.CreatePrimitive(PrimitiveType.Quad);
            backdrop.name = "C29Retarget_Backdrop";
            UnityEngine.Object.DestroyImmediate(backdrop.GetComponent<Collider>());
            Vector3 fromCamera = Vector3.ProjectOnPlane(sourceCamera.forward, Vector3.up).normalized;
            if (fromCamera.sqrMagnitude < 0.001f)
            {
                fromCamera = Vector3.forward;
            }

            backdrop.transform.position = head.position + (fromCamera * 3.25f);
            backdrop.transform.rotation = Quaternion.LookRotation(-fromCamera, Vector3.up);
            backdrop.transform.localScale = new Vector3(12f, 7f, 1f);
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
            {
                shader = Shader.Find("Unlit/Color");
            }

            Material material = new Material(shader)
            {
                name = "C29Retarget_Backdrop_Runtime",
                color = new Color(0.008f, 0.018f, 0.05f, 1f),
                hideFlags = HideFlags.DontSaveInBuild
            };
            backdrop.GetComponent<MeshRenderer>().sharedMaterial = material;
        }

        private static void CreateFadeOverlay(out CanvasGroup group, out Image image)
        {
            GameObject canvasObject = new GameObject(
                "C29Retarget_FadeCanvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster),
                typeof(CanvasGroup));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32000;
            group = canvasObject.GetComponent<CanvasGroup>();
            group.alpha = 1f;
            group.interactable = false;
            group.blocksRaycasts = false;

            GameObject imageObject = new GameObject("Black", typeof(RectTransform), typeof(Image));
            RectTransform rect = imageObject.GetComponent<RectTransform>();
            rect.SetParent(canvasObject.transform, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            image = imageObject.GetComponent<Image>();
            image.color = Color.black;
            image.raycastTarget = false;
        }

        private static void CreateReviewMarker()
        {
            GameObject marker = new GameObject("EDITOR_ONLY_C29_RETARGET_REVIEW");
            marker.transform.position = new Vector3(0f, -1000f, 0f);
        }

        private static void ValidateScene(
            Scene scene,
            CinematicSequenceProfile profile,
            AnimationClip cameraClip,
            GameObject inori,
            Camera sourceCamera,
            Camera mainCamera,
            CinematicBlendShapeExpressionPlayer expressionPlayer)
        {
            if (!scene.IsValid() || !scene.isLoaded)
            {
                throw new InvalidOperationException("C29 review scene is not loaded.");
            }

            if (cameraClip.length + 0.02f < AuthoredDurationSeconds)
            {
                throw new InvalidOperationException(
                    $"C29 camera clip is only {cameraClip.length:0.###}s; expected at least {AuthoredDurationSeconds:0.###}s.");
            }

            if (profile.SourceActorAnimations.Length != 0)
            {
                throw new InvalidOperationException("C29 retarget review must not bind the source actor animation.");
            }

            if (inori.GetComponentInChildren<SkinnedMeshRenderer>(true) == null)
            {
                throw new InvalidOperationException("C29 retarget review Inori has no skinned renderer.");
            }

            if (expressionPlayer.ActiveTargetCount != 0)
            {
                throw new InvalidOperationException("Initial closed-eye expression did not settle immediately.");
            }

            if (sourceCamera.enabled || !mainCamera.enabled)
            {
                throw new InvalidOperationException("Review must render through exactly the main camera, not the source camera.");
            }

            if (UnityEngine.Object.FindObjectsByType<AudioSource>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length != 0)
            {
                throw new InvalidOperationException("C29 retarget review must not contain source or review audio.");
            }

            if (EditorBuildSettings.scenes.Any(entry =>
                    string.Equals(entry.path, ScenePath, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException("Editor-only C29 retarget review leaked into product build settings.");
            }
        }

        private static void CaptureReviewFrames()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            CinematicSequenceRunner runner = FindSceneComponent<CinematicSequenceRunner>(scene)
                ?? throw new InvalidOperationException("C29 review runner missing during capture.");
            CinematicSequenceProfile profile =
                AssetDatabase.LoadAssetAtPath<CinematicSequenceProfile>(ProfilePath)
                ?? throw new InvalidOperationException("C29 review profile missing during capture.");
            CinematicBlendShapeExpressionPlayer expressionPlayer =
                FindSceneComponent<CinematicBlendShapeExpressionPlayer>(scene)
                ?? throw new InvalidOperationException("C29 review expression player missing during capture.");
            C29RetargetHeadPoseDriver headPoseDriver =
                FindSceneComponent<C29RetargetHeadPoseDriver>(scene)
                ?? throw new InvalidOperationException("C29 review head-pose driver missing during capture.");
            Camera camera = runner.CinematicCamera
                ?? throw new InvalidOperationException("C29 review camera missing during capture.");

            Directory.CreateDirectory(CaptureDirectory);
            List<Texture2D> frames = new List<Texture2D>(CaptureTimes.Length);
            List<string> framePaths = new List<string>(CaptureTimes.Length);
            try
            {
                for (int i = 0; i < CaptureTimes.Length; i++)
                {
                    float sampleTime = CaptureTimes[i];
                    if (!runner.TryApplyProfileSampleForReview(profile, sampleTime, Vector3.forward))
                    {
                        throw new InvalidOperationException($"Failed to sample C29 review at {sampleTime:0.###}s.");
                    }

                    expressionPlayer.ApplyExpressionImmediate(
                        sampleTime < EyeOpenSeconds ? EyesClosed : EyesOpen);
                    headPoseDriver.ApplySample(sampleTime);
                    string framePath = Path.Combine(
                            CaptureDirectory,
                            $"{i + 1:00}_{CaptureLabels[i]}_t{sampleTime:0.00}.png")
                        .Replace('\\', '/');
                    Texture2D frame = CaptureCamera(camera, 1280, 720);
                    File.WriteAllBytes(framePath, frame.EncodeToPNG());
                    frames.Add(frame);
                    framePaths.Add(framePath);
                }

                Texture2D sheet = BuildContactSheet(frames, 384, 216);
                try
                {
                    File.WriteAllBytes(ContactSheetPath, sheet.EncodeToPNG());
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(sheet);
                }

                WriteReport(profile, framePaths, camera);
            }
            finally
            {
                for (int i = 0; i < frames.Count; i++)
                {
                    UnityEngine.Object.DestroyImmediate(frames[i]);
                }
            }
        }

        private static void CaptureReviewVideoFrames()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            CinematicSequenceRunner runner = FindSceneComponent<CinematicSequenceRunner>(scene)
                ?? throw new InvalidOperationException("C29 review runner missing during video capture.");
            CinematicSequenceProfile profile =
                AssetDatabase.LoadAssetAtPath<CinematicSequenceProfile>(ProfilePath)
                ?? throw new InvalidOperationException("C29 review profile missing during video capture.");
            CinematicBlendShapeExpressionPlayer expressionPlayer =
                FindSceneComponent<CinematicBlendShapeExpressionPlayer>(scene)
                ?? throw new InvalidOperationException("C29 review expression player missing during video capture.");
            C29RetargetHeadPoseDriver headPoseDriver =
                FindSceneComponent<C29RetargetHeadPoseDriver>(scene)
                ?? throw new InvalidOperationException("C29 review head-pose driver missing during video capture.");
            Camera camera = runner.CinematicCamera
                ?? throw new InvalidOperationException("C29 review camera missing during video capture.");

            if (Directory.Exists(VideoFrameDirectory))
            {
                Directory.Delete(VideoFrameDirectory, recursive: true);
            }

            Directory.CreateDirectory(VideoFrameDirectory);
            const int framesPerSecond = 60;
            int frameCount = Mathf.RoundToInt(AuthoredDurationSeconds * framesPerSecond) + 1;
            for (int frameIndex = 0; frameIndex < frameCount; frameIndex++)
            {
                float sampleTime = Mathf.Min(
                    AuthoredDurationSeconds,
                    frameIndex / (float)framesPerSecond);
                if (!runner.TryApplyProfileSampleForReview(profile, sampleTime, Vector3.forward))
                {
                    throw new InvalidOperationException(
                        $"Failed to sample C29 video frame {frameIndex} at {sampleTime:0.###}s.");
                }

                expressionPlayer.ApplyExpressionImmediate(
                    sampleTime < EyeOpenSeconds ? EyesClosed : EyesOpen);
                headPoseDriver.ApplySample(sampleTime);
                Texture2D frame = CaptureCamera(camera, 1280, 720);
                try
                {
                    string framePath = Path.Combine(
                        VideoFrameDirectory,
                        $"frame_{frameIndex:0000}.png");
                    File.WriteAllBytes(framePath, frame.EncodeToPNG());
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(frame);
                }
            }

            File.WriteAllText(
                Path.Combine(VideoFrameDirectory, "README.txt"),
                $"{frameCount} PNG frames, {framesPerSecond} fps, {AuthoredDurationSeconds:0.0}s, 1280x720, no audio.{Environment.NewLine}");
        }

        private static Texture2D CaptureCamera(Camera camera, int width, int height)
        {
            RenderTexture previousTarget = camera.targetTexture;
            RenderTexture previousActive = RenderTexture.active;
            RenderTexture renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, mipChain: false);
            try
            {
                camera.targetTexture = renderTexture;
                RenderTexture.active = renderTexture;
                camera.Render();
                texture.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
                texture.Apply();
                return texture;
            }
            catch
            {
                UnityEngine.Object.DestroyImmediate(texture);
                throw;
            }
            finally
            {
                camera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                UnityEngine.Object.DestroyImmediate(renderTexture);
            }
        }


        private static Texture2D BuildContactSheet(IReadOnlyList<Texture2D> frames, int width, int height)
        {
            Texture2D sheet = new Texture2D(width * frames.Count, height, TextureFormat.RGBA32, false);
            Color[] background = Enumerable.Repeat(new Color(0.01f, 0.015f, 0.025f, 1f), sheet.width * sheet.height).ToArray();
            sheet.SetPixels(background);
            for (int i = 0; i < frames.Count; i++)
            {
                Texture2D source = frames[i];
                Color[] pixels = new Color[width * height];
                for (int y = 0; y < height; y++)
                {
                    float v = y / (float)Mathf.Max(1, height - 1);
                    for (int x = 0; x < width; x++)
                    {
                        float u = x / (float)Mathf.Max(1, width - 1);
                        pixels[x + (y * width)] = source.GetPixelBilinear(u, v);
                    }
                }

                sheet.SetPixels(i * width, 0, width, height, pixels);
            }

            sheet.Apply();
            return sheet;
        }

        private static void WriteReport(
            CinematicSequenceProfile profile,
            IReadOnlyList<string> framePaths,
            Camera camera)
        {
            List<string> lines = new List<string>
            {
                "# C29 close-up camera grammar retarget review",
                string.Empty,
                "- Result: PASS",
                $"- Scene: `{ScenePath}`",
                $"- Profile: `{ProfilePath}`",
                $"- Camera-only source: `{CameraAssetPath}`",
                $"- Source actor animation bound: {profile.SourceActorAnimations.Length}",
                $"- Retargeted face assets: `{EyesClosedMeshPath}`, `{EyesOpenMeshPath}`",
                $"- Inori eye-open beat: {EyeOpenSeconds:0.00}s",
                $"- Runtime camera FOV after final sample: {camera.fieldOfView:0.##}",
                $"- Contact sheet: `{ContactSheetPath}`",
                string.Empty,
                "## Captured frames",
                string.Empty
            };
            for (int i = 0; i < framePaths.Count; i++)
            {
                lines.Add($"- T+{CaptureTimes[i]:0.00}s `{framePaths[i]}`");
            }

            lines.Add(string.Empty);
            lines.Add("The original actor, environment, audio, VFX, and completed timeline are not present.");
            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath) ?? "C:/tmp");
            File.WriteAllLines(ReportPath, lines);
        }

        private static AnimationClip LoadPrimaryClip(string path)
        {
            return AssetDatabase.LoadAllAssetsAtPath(path)
                .OfType<AnimationClip>()
                .FirstOrDefault(clip => !clip.name.StartsWith("__preview__", StringComparison.Ordinal));
        }

        private static GameObject InstantiateAsset(string path, Transform parent)
        {
            GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path)
                ?? throw new InvalidOperationException($"GameObject asset missing at {path}.");
            GameObject instance = PrefabUtility.InstantiatePrefab(asset) as GameObject
                ?? throw new InvalidOperationException($"Failed to instantiate {path}.");
            instance.transform.SetParent(parent, false);
            return instance;
        }

        private static bool HasBlendShape(GameObject root, string shapeName)
        {
            SkinnedMeshRenderer[] renderers = root.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Mesh mesh = renderers[i].sharedMesh;
                if (mesh != null && mesh.GetBlendShapeIndex(shapeName) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static Transform FindChildRecursive(Transform root, string name)
        {
            Transform[] transforms = root.GetComponentsInChildren<Transform>(includeInactive: true);
            return transforms.FirstOrDefault(candidate =>
                string.Equals(candidate.name, name, StringComparison.Ordinal));
        }

        private static T FindSceneComponent<T>(Scene scene) where T : Component
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

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string normalized = path.Replace('\\', '/');
            string parent = normalized.Substring(0, normalized.LastIndexOf('/'));
            string name = normalized.Substring(normalized.LastIndexOf('/') + 1);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }

        private static void SetObject(SerializedObject serialized, string propertyName, UnityEngine.Object value)
        {
            Require(serialized, propertyName).objectReferenceValue = value;
        }

        private static void SetBool(SerializedObject serialized, string propertyName, bool value)
        {
            Require(serialized, propertyName).boolValue = value;
        }

        private static void SetFloat(SerializedObject serialized, string propertyName, float value)
        {
            Require(serialized, propertyName).floatValue = value;
        }

        private static SerializedProperty Require(SerializedObject serialized, string propertyName)
        {
            return serialized.FindProperty(propertyName)
                ?? throw new InvalidOperationException(
                    $"Missing serialized property {serialized.targetObject.GetType().Name}.{propertyName}.");
        }

        private static SerializedProperty RequireRelative(SerializedProperty parent, string propertyName)
        {
            return parent.FindPropertyRelative(propertyName)
                ?? throw new InvalidOperationException($"Missing serialized relative property {propertyName}.");
        }

        private static void RunBatch(Action action)
        {
            try
            {
                action();
                Debug.Log("C29 closeup retarget batch operation passed.");
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(0);
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }

                throw;
            }
        }
    }
}
