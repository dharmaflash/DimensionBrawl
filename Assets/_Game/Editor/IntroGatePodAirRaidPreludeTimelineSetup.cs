using System;
using System.Collections.Generic;
using System.IO;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;

namespace DimensionBrawl.Editor
{
    public static class IntroGatePodAirRaidPreludeTimelineSetup
    {
        public const string ReviewScenePath = "Assets/_Game/Scenes/IntroGatePodAirRaidPreludeReview.unity";
        public const string TimelinePath =
            "Assets/_Game/DesignData/Timelines/Cinematics/DB_Timeline_IntroAirRaidPrelude.playable";
        public const string ReportPath = "C:/tmp/DimensionBrawl-IntroAirRaidPrelude-Verification.md";

        private const string RootName = "IntroGatePodAirRaidPreludeReview";
        private const string DirectorName = "IntroGatePodAirRaidPrelude_TimelineDirector";
        private const string CameraRootName = "IntroGatePodAirRaidPrelude_CinemachineShots";
        private const string MainCameraName = "IntroGatePodAirRaidPrelude_MainCamera";
        private const string FormationRootName = "IntroGatePodAirRaidPrelude_AircraftFormationRig";
        private const string BomberRootName = "IntroGatePodAirRaidPrelude_BomberRunRig";
        private const string BombingRootName = "IntroGatePodAirRaidPrelude_AerialBombingEffect";
        private const string ImpactRootName = "IntroGatePodAirRaidPrelude_ImpactEffects";
        private const string CloudRootName = "IntroGatePodAirRaidPrelude_CloudDeck";

        private const string AnimationRoot = "Assets/_Game/Art/Animations/Cinematics/IntroAirRaid";
        private const string PromotedPrefabRoot = "Assets/_Game/Art/VFX/IntroAirRaid";
        private const string MaterialRoot = "Assets/_Game/Art/Materials/ActionFoundation/IntroGatePodAirRaid";

        private const string SourceJetBomberPrefabPath =
            "Assets/_Imported/SpecialSkillsEffectsPack/AllEffects/EffectsSet_2(ScriptBased)/Effects/Effect_46_CyberAirTroopInvader/Effect_46_Parts/Effect_46_JetBomber.prefab";
        private const string SourceJetStrikerPrefabPath =
            "Assets/_Imported/SpecialSkillsEffectsPack/AllEffects/EffectsSet_2(ScriptBased)/Effects/Effect_46_CyberAirTroopInvader/Effect_46_Parts/Effect_46_JetStriker.prefab";
        private const string SourceStealthBomberPrefabPath =
            "Assets/_Imported/SpecialSkillsEffectsPack/AllEffects/EffectsSet_2(ScriptBased)/Effects/Effect_46_CyberAirTroopInvader/Effect_46_CyberStealthBomber.prefab";
        private const string SourceAerialBombingPrefabPath =
            "Assets/_Imported/SpecialSkillsEffectsPack/AllEffects/EffectsSet_2(ScriptBased)/Effects/Effect_40_AerialBombing/Effect_40_AerialBombing.prefab";
        private const string SourceBombExplosionPrefabPath =
            "Assets/_Imported/SpecialSkillsEffectsPack/AllEffects/EffectsSet_1(NotScriptBased)/Effects/Effect_41_Airstrike/Effect_41_Base/Effect_41_BombExplosion.prefab";
        private const string SourceBulletExplosionPrefabPath =
            "Assets/_Imported/SpecialSkillsEffectsPack/AllEffects/EffectsSet_1(NotScriptBased)/Effects/Effect_41_Airstrike/Effect_41_Base/Effect_41_BulletExplosion.prefab";
        private const string SourceCloudSystemPrefabPath =
            "Assets/_Imported/AssetStore/_DLNK/Essential Terrain Pack/[PREFABS]/05 FX/Clouds/CloudSystem00.prefab";
        private const string SourceCloudTexturePath =
            "Assets/_Imported/AssetStore/_DLNK/Essential Terrain Pack/Source/Maps/[Texture Pack]/FX/Clouds/clouda.png";

        private const string PromotedJetBomberPrefabPath = PromotedPrefabRoot + "/PF_IntroAirRaid_JetBomber.prefab";
        private const string PromotedJetStrikerPrefabPath = PromotedPrefabRoot + "/PF_IntroAirRaid_JetStriker.prefab";
        private const string PromotedStealthBomberPrefabPath = PromotedPrefabRoot + "/PF_IntroAirRaid_CyberStealthBomber.prefab";
        private const string PromotedAerialBombingPrefabPath = PromotedPrefabRoot + "/PF_IntroAirRaid_AerialBombing.prefab";
        private const string PromotedBombExplosionPrefabPath = PromotedPrefabRoot + "/PF_IntroAirRaid_BombExplosion.prefab";
        private const string PromotedBulletExplosionPrefabPath = PromotedPrefabRoot + "/PF_IntroAirRaid_BulletExplosion.prefab";
        private const string PromotedCloudSystemPrefabPath = PromotedPrefabRoot + "/PF_IntroAirRaid_CloudSystem.prefab";

        private const string CloudMaterialPath = MaterialRoot + "/AF_IntroAirRaidReviewCloud.mat";
        private const string SkyMaterialPath = MaterialRoot + "/AF_IntroAirRaidReviewSky.mat";
        private const double SequenceDurationSeconds = 6.8d;

        private static readonly ShotSpec[] Shots =
        {
            new ShotSpec(
                "AIR_01_CloudDeck_ThreeAircraftRearFollow",
                0.0d,
                2.4d,
                new Vector3(0f, 6.2f, -15.5f),
                new Vector3(0f, 5.1f, -1.5f),
                new Vector3(0f, 6.15f, -12.9f),
                new Vector3(0f, 5.0f, 2.5f),
                35f,
                0.0d),
            new ShotSpec(
                "AIR_02_BomberDrop_Reframe",
                2.4d,
                1.8d,
                new Vector3(-4.8f, 6.4f, -7.5f),
                new Vector3(0f, 3.9f, 4.5f),
                new Vector3(-3.8f, 6.1f, -4.4f),
                new Vector3(0f, 3.6f, 8.5f),
                42f,
                0.28d),
            new ShotSpec(
                "AIR_03_ImpactBelowClouds_RecoilReturn",
                4.2d,
                1.6d,
                new Vector3(6.0f, 4.8f, 10.5f),
                new Vector3(0f, 1.25f, 14.8f),
                new Vector3(6.0f, 4.9f, 10.5f),
                new Vector3(0f, 1.25f, 14.8f),
                45f,
                0.12d),
            new ShotSpec(
                "AIR_04_WarningBlackBridge",
                5.8d,
                1.0d,
                new Vector3(2.4f, 3.6f, 9.5f),
                new Vector3(0f, 2.0f, 15.0f),
                new Vector3(1.5f, 3.4f, 11.5f),
                new Vector3(0f, 1.8f, 15.0f),
                38f,
                0.2d)
        };

        [MenuItem("DimensionBrawl/Cinematics/Build Intro Air Raid Prelude Review Timeline")]
        public static void BuildReviewTimelineMenu()
        {
            BuildReviewTimeline();
            Debug.Log($"Built intro air-raid prelude review Timeline. Report: {ReportPath}");
        }

        public static void RunBatchBuildAndValidate()
        {
            BuildReviewTimeline();
        }

        public static void BuildReviewTimeline()
        {
            EnsureFolders();
            PromoteAirRaidPrefabs();

            AnimationClip formationMoveClip = CreateTransformClip(
                "CIN_IntroAirRaid_AircraftFormationMove",
                0f,
                3.2f,
                new Vector3(0f, 4.8f, -8.5f),
                new Vector3(0f, 4.6f, 9.5f),
                Vector3.zero,
                Vector3.zero);
            AnimationClip bomberRunClip = CreateTransformClip(
                "CIN_IntroAirRaid_BomberRunMove",
                0f,
                3.4f,
                new Vector3(1.8f, 5.4f, -7.0f),
                new Vector3(-1.1f, 4.2f, 11.0f),
                new Vector3(0f, -8f, 0f),
                new Vector3(0f, -8f, 0f));
            AnimationClip[] cameraMotionClips = CreateCameraMotionClips();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject root = new GameObject(RootName);
            SceneManager.MoveGameObjectToScene(root, scene);

            BuildEnvironment(root.transform);
            Camera mainCamera = CreateMainCamera(root.transform);
            CinemachineBrain brain = mainCamera.gameObject.AddComponent<CinemachineBrain>();

            GameObject cameraRoot = CreateChild(root.transform, CameraRootName, Vector3.zero, Quaternion.identity, Vector3.one);
            CinemachineCamera[] shotCameras = CreateCinemachineShots(cameraRoot.transform);

            GameObject formationRoot = BuildAircraftFormation(root.transform);
            GameObject bomberRoot = BuildBomberRun(root.transform);
            GameObject bombingRoot = BuildAerialBombing(root.transform);
            GameObject impactRoot = BuildImpactEffects(root.transform);

            TimelineAsset timeline = CreateFreshTimelineAsset();
            PlayableDirector director = CreateTimelineDirector(root.transform, timeline);
            CreateCinemachineTrack(timeline, director, brain, shotCameras);
            CreateAnimationTrack(
                timeline,
                director,
                "Aircraft Formation Move",
                formationRoot,
                formationMoveClip,
                0d,
                3.2d);
            CreateAnimationTrack(
                timeline,
                director,
                "Bomber Run Move",
                bomberRoot,
                bomberRunClip,
                2.0d,
                3.4d);
            CreateCameraMotionTracks(timeline, director, shotCameras, cameraMotionClips);
            CreateActivationTrack(timeline, director, "Aircraft Formation Active", formationRoot, 0d, 3.4d);
            CreateActivationTrack(timeline, director, "Bomber Run Active", bomberRoot, 2.0d, 3.6d);
            CreateActivationTrack(timeline, director, "Aerial Bombing Active", bombingRoot, 2.25d, 4.2d);
            CreateActivationTrack(timeline, director, "Impact Effects Active", impactRoot, 4.15d, 2.6d);

            EditorUtility.SetDirty(timeline);
            EditorUtility.SetDirty(director);
            AssetDatabase.SaveAssets();

            EditorSceneManager.SaveScene(scene, ReviewScenePath);
            ValidateReviewTimeline();
        }

        public static void ValidateReviewTimeline()
        {
            List<string> issues = new List<string>();

            TimelineAsset timeline = AssetDatabase.LoadAssetAtPath<TimelineAsset>(TimelinePath);
            if (timeline == null)
            {
                issues.Add($"Missing TimelineAsset: `{TimelinePath}`.");
            }

            Scene scene = EditorSceneManager.OpenScene(ReviewScenePath, OpenSceneMode.Single);
            GameObject root = FindRoot(scene, RootName);
            if (root == null)
            {
                issues.Add($"Missing scene root: `{RootName}`.");
            }

            PlayableDirector director = root != null
                ? root.GetComponentInChildren<PlayableDirector>(includeInactive: true)
                : null;
            if (director == null)
            {
                issues.Add("Missing PlayableDirector.");
            }
            else if (director.playableAsset != timeline)
            {
                issues.Add("PlayableDirector is not bound to the air-raid Timeline asset.");
            }

            if (timeline != null)
            {
                CheckTrack<CinemachineTrack>(timeline, "Cinemachine Shots", issues);
                CheckTrack<AnimationTrack>(timeline, "Aircraft Formation Move", issues);
                CheckTrack<AnimationTrack>(timeline, "Bomber Run Move", issues);
                CheckTrack<ActivationTrack>(timeline, "Aerial Bombing Active", issues);
                CheckTrack<ActivationTrack>(timeline, "Impact Effects Active", issues);

                int clipCount = 0;
                foreach (TrackAsset track in timeline.GetOutputTracks())
                {
                    foreach (TimelineClip unused in track.GetClips())
                    {
                        clipCount++;
                    }
                }

                if (clipCount < 10)
                {
                    issues.Add($"Timeline has too few editable clips for review: {clipCount}.");
                }
            }

            CheckSceneObject(root, FormationRootName, issues);
            CheckSceneObject(root, BomberRootName, issues);
            CheckSceneObject(root, BombingRootName, issues);
            CheckSceneObject(root, ImpactRootName, issues);
            CheckPromotedPrefab(PromotedJetBomberPrefabPath, issues);
            CheckPromotedPrefab(PromotedJetStrikerPrefabPath, issues);
            CheckPromotedPrefab(PromotedStealthBomberPrefabPath, issues);
            CheckPromotedPrefab(PromotedAerialBombingPrefabPath, issues);

            WriteReport(issues);
            if (issues.Count > 0)
            {
                throw new InvalidOperationException("Intro air-raid prelude validation failed:\n" + string.Join("\n", issues));
            }
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/_Game/Scenes");
            EnsureFolder("Assets/_Game/DesignData/Timelines/Cinematics");
            EnsureFolder(AnimationRoot);
            EnsureFolder(PromotedPrefabRoot);
            EnsureFolder(MaterialRoot);
        }

        private static void PromoteAirRaidPrefabs()
        {
            CopyAsset(SourceJetBomberPrefabPath, PromotedJetBomberPrefabPath);
            CopyAsset(SourceJetStrikerPrefabPath, PromotedJetStrikerPrefabPath);
            CopyAsset(SourceStealthBomberPrefabPath, PromotedStealthBomberPrefabPath);
            CopyAsset(SourceAerialBombingPrefabPath, PromotedAerialBombingPrefabPath);
            CopyAsset(SourceBombExplosionPrefabPath, PromotedBombExplosionPrefabPath);
            CopyAsset(SourceBulletExplosionPrefabPath, PromotedBulletExplosionPrefabPath);
            CopyAsset(SourceCloudSystemPrefabPath, PromotedCloudSystemPrefabPath);
        }

        private static void BuildEnvironment(Transform root)
        {
            GameObject lightObject = new GameObject("IntroGatePodAirRaidPrelude_KeyLight");
            lightObject.transform.SetParent(root, worldPositionStays: false);
            lightObject.transform.SetPositionAndRotation(new Vector3(-7f, 9f, -8f), Quaternion.Euler(52f, -32f, 0f));
            Light keyLight = lightObject.AddComponent<Light>();
            keyLight.type = LightType.Directional;
            keyLight.intensity = 1.05f;
            keyLight.color = new Color(0.78f, 0.83f, 0.88f);

            GameObject fillObject = new GameObject("IntroGatePodAirRaidPrelude_CloudFill");
            fillObject.transform.SetParent(root, worldPositionStays: false);
            fillObject.transform.position = new Vector3(0f, 6f, -5f);
            Light fillLight = fillObject.AddComponent<Light>();
            fillLight.type = LightType.Point;
            fillLight.range = 26f;
            fillLight.intensity = 1.75f;
            fillLight.color = new Color(0.72f, 0.83f, 1.0f);

            BuildCloudDeck(root);
            BuildSkyBackdrop(root);
        }

        private static Camera CreateMainCamera(Transform root)
        {
            GameObject cameraObject = new GameObject(MainCameraName);
            cameraObject.transform.SetParent(root, worldPositionStays: false);
            cameraObject.transform.SetPositionAndRotation(Shots[0].CameraStart, ResolveLookRotation(Shots[0].CameraStart, Shots[0].LookAtStart));
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = Shots[0].FieldOfView;
            camera.nearClipPlane = 0.03f;
            camera.farClipPlane = 260f;
            camera.clearFlags = CameraClearFlags.Skybox;
            cameraObject.AddComponent<AudioListener>();
            return camera;
        }

        private static CinemachineCamera[] CreateCinemachineShots(Transform cameraRoot)
        {
            CinemachineCamera[] cameras = new CinemachineCamera[Shots.Length];
            for (int i = 0; i < Shots.Length; i++)
            {
                ShotSpec shot = Shots[i];
                GameObject shotObject = new GameObject($"CM_{i + 1:00}_{shot.Id}");
                shotObject.transform.SetParent(cameraRoot, worldPositionStays: false);
                shotObject.transform.SetPositionAndRotation(shot.CameraStart, ResolveLookRotation(shot.CameraStart, shot.LookAtStart));

                GameObject lookAtObject = new GameObject($"{shotObject.name}_LookAt");
                lookAtObject.transform.SetParent(cameraRoot, worldPositionStays: false);
                lookAtObject.transform.position = shot.LookAtStart;

                CinemachineCamera cinemachineCamera = shotObject.AddComponent<CinemachineCamera>();
                cinemachineCamera.Priority = 0;
                cinemachineCamera.StandbyUpdate = CinemachineVirtualCameraBase.StandbyUpdateMode.Never;
                cinemachineCamera.LookAt = lookAtObject.transform;

                LensSettings lens = LensSettings.Default;
                lens.ModeOverride = LensSettings.OverrideModes.Perspective;
                lens.FieldOfView = shot.FieldOfView;
                lens.NearClipPlane = 0.03f;
                lens.FarClipPlane = 260f;
                cinemachineCamera.Lens = lens;
                shotObject.AddComponent<CinemachineHardLookAt>();

                cameras[i] = cinemachineCamera;
            }

            return cameras;
        }

        private static GameObject BuildAircraftFormation(Transform root)
        {
            GameObject formationRoot = CreateChild(
                root,
                FormationRootName,
                new Vector3(0f, 4.8f, -8.5f),
                Quaternion.identity,
                Vector3.one);

            InstantiatePromotedPrefab(
                PromotedJetBomberPrefabPath,
                formationRoot.transform,
                "Lead_Effect46_JetBomber_AutoForward",
                new Vector3(0f, 0f, 0f),
                Quaternion.identity,
                new Vector3(0.34f, 0.34f, 0.34f));
            InstantiatePromotedPrefab(
                PromotedJetStrikerPrefabPath,
                formationRoot.transform,
                "LeftWing_Effect46_JetStriker_AutoForward",
                new Vector3(-2.6f, -0.28f, -2.2f),
                Quaternion.Euler(0f, 2f, 0f),
                new Vector3(0.28f, 0.28f, 0.28f));
            InstantiatePromotedPrefab(
                PromotedJetStrikerPrefabPath,
                formationRoot.transform,
                "RightWing_Effect46_JetStriker_AutoForward",
                new Vector3(2.6f, -0.28f, -2.2f),
                Quaternion.Euler(0f, -2f, 0f),
                new Vector3(0.28f, 0.28f, 0.28f));

            Animator animator = formationRoot.AddComponent<Animator>();
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            return formationRoot;
        }

        private static GameObject BuildBomberRun(Transform root)
        {
            GameObject bomberRoot = CreateChild(
                root,
                BomberRootName,
                new Vector3(1.8f, 5.4f, -7.0f),
                Quaternion.Euler(0f, -8f, 0f),
                Vector3.one);

            InstantiatePromotedPrefab(
                PromotedStealthBomberPrefabPath,
                bomberRoot.transform,
                "Effect46_CyberStealthBomber_AutoForwardAndShotPoints",
                Vector3.zero,
                Quaternion.identity,
                new Vector3(0.42f, 0.42f, 0.42f));

            Animator animator = bomberRoot.AddComponent<Animator>();
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            return bomberRoot;
        }

        private static GameObject BuildAerialBombing(Transform root)
        {
            GameObject bombingRoot = CreateChild(
                root,
                BombingRootName,
                new Vector3(0f, 1.2f, 11.8f),
                Quaternion.Euler(0f, 0f, 0f),
                new Vector3(0.8f, 0.8f, 0.8f));
            InstantiatePromotedPrefab(
                PromotedAerialBombingPrefabPath,
                bombingRoot.transform,
                "Effect40_AerialBombing_AutoBombSpawner",
                Vector3.zero,
                Quaternion.identity,
                Vector3.one);
            bombingRoot.SetActive(false);
            return bombingRoot;
        }

        private static GameObject BuildImpactEffects(Transform root)
        {
            GameObject impactRoot = CreateChild(
                root,
                ImpactRootName,
                new Vector3(0f, 1.0f, 14.2f),
                Quaternion.identity,
                Vector3.one);

            InstantiatePromotedPrefab(
                PromotedBombExplosionPrefabPath,
                impactRoot.transform,
                "Effect41_PrimaryBombExplosion",
                new Vector3(-1.6f, 0f, 0.6f),
                Quaternion.identity,
                new Vector3(1.15f, 1.15f, 1.15f));
            InstantiatePromotedPrefab(
                PromotedBulletExplosionPrefabPath,
                impactRoot.transform,
                "Effect41_SecondaryBulletExplosion",
                new Vector3(2.2f, 0f, -0.8f),
                Quaternion.identity,
                new Vector3(0.9f, 0.9f, 0.9f));

            impactRoot.SetActive(false);
            return impactRoot;
        }

        private static void BuildCloudDeck(Transform root)
        {
            GameObject cloudRoot = CreateChild(root, CloudRootName, Vector3.zero, Quaternion.identity, Vector3.one);
            InstantiatePromotedPrefab(
                PromotedCloudSystemPrefabPath,
                cloudRoot.transform,
                "CloudSystem00_ReviewLayer",
                new Vector3(0f, 2.6f, 7.5f),
                Quaternion.identity,
                new Vector3(2.4f, 2.4f, 2.4f));

            Material cloudMaterial = EnsureCloudMaterial();
            for (int i = 0; i < 6; i++)
            {
                float x = -15f + i * 6f;
                float z = -2f + (i % 2) * 3.4f;
                GameObject card = GameObject.CreatePrimitive(PrimitiveType.Plane);
                card.name = $"CloudPlane_{i + 1:00}";
                card.transform.SetParent(cloudRoot.transform, worldPositionStays: false);
                card.transform.localPosition = new Vector3(x, 1.8f + 0.12f * i, z + 9f);
                card.transform.localRotation = Quaternion.Euler(0f, 14f - i * 4f, 0f);
                card.transform.localScale = new Vector3(3.4f, 1f, 2.3f);
                Collider collider = card.GetComponent<Collider>();
                if (collider != null)
                {
                    UnityEngine.Object.DestroyImmediate(collider);
                }

                Renderer renderer = card.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.sharedMaterial = cloudMaterial;
                }
            }
        }

        private static void BuildSkyBackdrop(Transform root)
        {
            GameObject sky = GameObject.CreatePrimitive(PrimitiveType.Quad);
            sky.name = "IntroGatePodAirRaidPrelude_SoftSkyBackdrop";
            sky.transform.SetParent(root, worldPositionStays: false);
            sky.transform.localPosition = new Vector3(0f, 6f, 24f);
            sky.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            sky.transform.localScale = new Vector3(42f, 20f, 1f);
            Collider collider = sky.GetComponent<Collider>();
            if (collider != null)
            {
                UnityEngine.Object.DestroyImmediate(collider);
            }

            Renderer renderer = sky.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = EnsureSkyMaterial();
            }
        }

        private static PlayableDirector CreateTimelineDirector(Transform root, TimelineAsset timeline)
        {
            GameObject directorObject = new GameObject(DirectorName);
            directorObject.transform.SetParent(root, worldPositionStays: false);
            PlayableDirector director = directorObject.AddComponent<PlayableDirector>();
            director.playableAsset = timeline;
            director.playOnAwake = true;
            director.timeUpdateMode = DirectorUpdateMode.GameTime;
            director.extrapolationMode = DirectorWrapMode.Hold;
            return director;
        }

        private static void CreateCinemachineTrack(
            TimelineAsset timeline,
            PlayableDirector director,
            CinemachineBrain brain,
            CinemachineCamera[] shotCameras)
        {
            CinemachineTrack track = timeline.CreateTrack<CinemachineTrack>("Cinemachine Shots");
            track.TrackPriority = 200;
            director.SetGenericBinding(track, brain);

            for (int i = 0; i < Shots.Length; i++)
            {
                ShotSpec shot = Shots[i];
                TimelineClip clip = track.CreateClip<CinemachineShot>();
                clip.displayName = shot.Id;
                clip.start = Math.Max(0d, shot.StartSeconds - shot.BlendInSeconds);
                clip.duration = shot.DurationSeconds + shot.BlendInSeconds;
                if (shot.BlendInSeconds > 0.001d)
                {
                    clip.blendInDuration = shot.BlendInSeconds;
                    clip.easeInDuration = shot.BlendInSeconds;
                }

                CinemachineShot shotAsset = clip.asset as CinemachineShot;
                if (shotAsset == null)
                {
                    continue;
                }

                PropertyName exposedName = new PropertyName($"intro_airraid_cm_{i + 1:00}");
                shotAsset.VirtualCamera.exposedName = exposedName;
                director.SetReferenceValue(exposedName, shotCameras[i]);
                EditorUtility.SetDirty(shotAsset);
            }
        }

        private static AnimationClip[] CreateCameraMotionClips()
        {
            AnimationClip[] clips = new AnimationClip[Shots.Length];
            for (int i = 0; i < Shots.Length; i++)
            {
                ShotSpec shot = Shots[i];
                clips[i] = i == 2
                    ? CreateImpactRecoilClip("CIN_IntroAirRaid_Camera03ImpactRecoil", shot)
                    : CreateTransformClip(
                        $"CIN_IntroAirRaid_Camera{i + 1:00}Drift",
                        0f,
                        (float)shot.DurationSeconds,
                        shot.CameraStart,
                        shot.CameraEnd,
                        ResolveLookRotation(shot.CameraStart, shot.LookAtStart).eulerAngles,
                        ResolveLookRotation(shot.CameraEnd, shot.LookAtEnd).eulerAngles);
            }

            return clips;
        }

        private static void CreateCameraMotionTracks(
            TimelineAsset timeline,
            PlayableDirector director,
            CinemachineCamera[] shotCameras,
            AnimationClip[] cameraMotionClips)
        {
            for (int i = 0; i < shotCameras.Length; i++)
            {
                GameObject cameraObject = shotCameras[i].gameObject;
                Animator animator = cameraObject.AddComponent<Animator>();
                animator.applyRootMotion = false;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

                ShotSpec shot = Shots[i];
                CreateAnimationTrack(
                    timeline,
                    director,
                    $"Camera {i + 1:00} Motion",
                    cameraObject,
                    cameraMotionClips[i],
                    shot.StartSeconds,
                    shot.DurationSeconds);
            }
        }

        private static void CreateAnimationTrack(
            TimelineAsset timeline,
            PlayableDirector director,
            string trackName,
            GameObject target,
            AnimationClip clip,
            double startSeconds,
            double durationSeconds)
        {
            Animator animator = target.GetComponent<Animator>();
            if (animator == null)
            {
                animator = target.AddComponent<Animator>();
            }

            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

            AnimationTrack track = timeline.CreateTrack<AnimationTrack>(trackName);
            track.trackOffset = TrackOffset.ApplySceneOffsets;
            director.SetGenericBinding(track, animator);
            TimelineClip timelineClip = track.CreateClip(clip);
            timelineClip.displayName = clip.name;
            timelineClip.start = startSeconds;
            timelineClip.duration = durationSeconds;
        }

        private static void CreateActivationTrack(
            TimelineAsset timeline,
            PlayableDirector director,
            string trackName,
            GameObject target,
            double startSeconds,
            double durationSeconds)
        {
            ActivationTrack track = timeline.CreateTrack<ActivationTrack>(trackName);
            director.SetGenericBinding(track, target);
            TimelineClip clip = track.CreateDefaultClip();
            clip.displayName = trackName;
            clip.start = startSeconds;
            clip.duration = durationSeconds;
        }

        private static AnimationClip CreateTransformClip(
            string clipName,
            float startSeconds,
            float endSeconds,
            Vector3 startPosition,
            Vector3 endPosition,
            Vector3 startEuler,
            Vector3 endEuler)
        {
            AnimationClip clip = CreateOrReplaceAnimationClip(clipName);
            clip.frameRate = 30f;
            clip.wrapMode = WrapMode.ClampForever;
            clip.SetCurve(string.Empty, typeof(Transform), "m_LocalPosition.x", AnimationCurve.Linear(startSeconds, startPosition.x, endSeconds, endPosition.x));
            clip.SetCurve(string.Empty, typeof(Transform), "m_LocalPosition.y", AnimationCurve.Linear(startSeconds, startPosition.y, endSeconds, endPosition.y));
            clip.SetCurve(string.Empty, typeof(Transform), "m_LocalPosition.z", AnimationCurve.Linear(startSeconds, startPosition.z, endSeconds, endPosition.z));
            clip.SetCurve(string.Empty, typeof(Transform), "localEulerAnglesRaw.x", AnimationCurve.Linear(startSeconds, startEuler.x, endSeconds, endEuler.x));
            clip.SetCurve(string.Empty, typeof(Transform), "localEulerAnglesRaw.y", AnimationCurve.Linear(startSeconds, startEuler.y, endSeconds, endEuler.y));
            clip.SetCurve(string.Empty, typeof(Transform), "localEulerAnglesRaw.z", AnimationCurve.Linear(startSeconds, startEuler.z, endSeconds, endEuler.z));
            EditorUtility.SetDirty(clip);
            return clip;
        }

        private static AnimationClip CreateImpactRecoilClip(string clipName, ShotSpec shot)
        {
            AnimationClip clip = CreateOrReplaceAnimationClip(clipName);
            clip.frameRate = 30f;
            clip.wrapMode = WrapMode.ClampForever;

            Quaternion rotation = ResolveLookRotation(shot.CameraStart, shot.LookAtStart);
            Vector3 baseEuler = rotation.eulerAngles;
            Vector3 recoil = shot.CameraStart + new Vector3(-0.12f, 0.08f, -0.24f);
            AnimationCurve x = new AnimationCurve(
                new Keyframe(0f, shot.CameraStart.x),
                new Keyframe(0.12f, recoil.x),
                new Keyframe(0.34f, shot.CameraStart.x),
                new Keyframe((float)shot.DurationSeconds, shot.CameraEnd.x));
            AnimationCurve y = new AnimationCurve(
                new Keyframe(0f, shot.CameraStart.y),
                new Keyframe(0.12f, recoil.y),
                new Keyframe(0.34f, shot.CameraStart.y),
                new Keyframe((float)shot.DurationSeconds, shot.CameraEnd.y));
            AnimationCurve z = new AnimationCurve(
                new Keyframe(0f, shot.CameraStart.z),
                new Keyframe(0.12f, recoil.z),
                new Keyframe(0.34f, shot.CameraStart.z),
                new Keyframe((float)shot.DurationSeconds, shot.CameraEnd.z));
            clip.SetCurve(string.Empty, typeof(Transform), "m_LocalPosition.x", x);
            clip.SetCurve(string.Empty, typeof(Transform), "m_LocalPosition.y", y);
            clip.SetCurve(string.Empty, typeof(Transform), "m_LocalPosition.z", z);
            clip.SetCurve(string.Empty, typeof(Transform), "localEulerAnglesRaw.x", AnimationCurve.Constant(0f, (float)shot.DurationSeconds, baseEuler.x));
            clip.SetCurve(string.Empty, typeof(Transform), "localEulerAnglesRaw.y", AnimationCurve.Constant(0f, (float)shot.DurationSeconds, baseEuler.y));
            clip.SetCurve(string.Empty, typeof(Transform), "localEulerAnglesRaw.z", AnimationCurve.Constant(0f, (float)shot.DurationSeconds, baseEuler.z));
            EditorUtility.SetDirty(clip);
            return clip;
        }

        private static TimelineAsset CreateFreshTimelineAsset()
        {
            TimelineAsset existing = AssetDatabase.LoadAssetAtPath<TimelineAsset>(TimelinePath);
            if (existing != null)
            {
                AssetDatabase.DeleteAsset(TimelinePath);
            }

            TimelineAsset timeline = ScriptableObject.CreateInstance<TimelineAsset>();
            timeline.durationMode = TimelineAsset.DurationMode.FixedLength;
            timeline.fixedDuration = SequenceDurationSeconds;
            timeline.editorSettings.frameRate = 30d;
            AssetDatabase.CreateAsset(timeline, TimelinePath);
            return timeline;
        }

        private static AnimationClip CreateOrReplaceAnimationClip(string clipName)
        {
            string path = $"{AnimationRoot}/{clipName}.anim";
            AnimationClip existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (existing != null)
            {
                AssetDatabase.DeleteAsset(path);
            }

            AnimationClip clip = new AnimationClip { name = clipName };
            AssetDatabase.CreateAsset(clip, path);
            return clip;
        }

        private static GameObject InstantiatePromotedPrefab(
            string path,
            Transform parent,
            string name,
            Vector3 localPosition,
            Quaternion localRotation,
            Vector3 localScale)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                throw new InvalidOperationException($"Missing promoted prefab at {path}.");
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null)
            {
                throw new InvalidOperationException($"Failed to instantiate prefab at {path}.");
            }

            instance.name = name;
            instance.transform.SetParent(parent, worldPositionStays: false);
            instance.transform.localPosition = localPosition;
            instance.transform.localRotation = localRotation;
            instance.transform.localScale = localScale;
            return instance;
        }

        private static Material EnsureCloudMaterial()
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(CloudMaterialPath);
            if (material != null)
            {
                return material;
            }

            material = CreateMaterial(CloudMaterialPath, new Color(0.82f, 0.86f, 0.82f, 0.68f));
            Texture2D cloudTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(SourceCloudTexturePath);
            if (cloudTexture != null)
            {
                material.mainTexture = cloudTexture;
            }

            material.SetFloat("_Surface", 1f);
            material.renderQueue = 3000;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material EnsureSkyMaterial()
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(SkyMaterialPath);
            if (material != null)
            {
                return material;
            }

            material = CreateMaterial(SkyMaterialPath, new Color(0.63f, 0.70f, 0.72f, 1f));
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material CreateMaterial(string path, Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
            {
                shader = Shader.Find("Unlit/Color") ?? Shader.Find("Standard");
            }

            Material material = new Material(shader);
            material.color = color;
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static GameObject CreateChild(
            Transform parent,
            string name,
            Vector3 localPosition,
            Quaternion localRotation,
            Vector3 localScale)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(parent, worldPositionStays: false);
            child.transform.localPosition = localPosition;
            child.transform.localRotation = localRotation;
            child.transform.localScale = localScale;
            return child;
        }

        private static void CopyAsset(string sourcePath, string destinationPath)
        {
            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(sourcePath) == null)
            {
                throw new InvalidOperationException($"Missing source asset: {sourcePath}");
            }

            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(destinationPath) != null)
            {
                AssetDatabase.DeleteAsset(destinationPath);
            }

            if (!AssetDatabase.CopyAsset(sourcePath, destinationPath))
            {
                throw new InvalidOperationException($"Failed to copy asset from {sourcePath} to {destinationPath}.");
            }
        }

        private static void EnsureFolder(string folderPath)
        {
            string[] parts = folderPath.Split('/');
            if (parts.Length == 0 || parts[0] != "Assets")
            {
                throw new ArgumentException($"Unity asset folder must start with Assets: {folderPath}");
            }

            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }

        private static Quaternion ResolveLookRotation(Vector3 position, Vector3 lookAt)
        {
            Vector3 direction = lookAt - position;
            if (direction.sqrMagnitude < 0.0001f)
            {
                return Quaternion.identity;
            }

            return Quaternion.LookRotation(direction.normalized, Vector3.up);
        }

        private static GameObject FindRoot(Scene scene, string name)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (string.Equals(root.name, name, StringComparison.Ordinal))
                {
                    return root;
                }
            }

            return null;
        }

        private static void CheckTrack<T>(TimelineAsset timeline, string trackName, List<string> issues)
            where T : TrackAsset
        {
            foreach (TrackAsset track in timeline.GetOutputTracks())
            {
                if (track is T && string.Equals(track.name, trackName, StringComparison.Ordinal))
                {
                    return;
                }
            }

            issues.Add($"Timeline is missing `{trackName}` {typeof(T).Name}.");
        }

        private static void CheckSceneObject(GameObject root, string name, List<string> issues)
        {
            if (root == null)
            {
                return;
            }

            Transform[] children = root.GetComponentsInChildren<Transform>(includeInactive: true);
            for (int i = 0; i < children.Length; i++)
            {
                if (string.Equals(children[i].name, name, StringComparison.Ordinal))
                {
                    return;
                }
            }

            issues.Add($"Missing scene object `{name}`.");
        }

        private static void CheckPromotedPrefab(string path, List<string> issues)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null)
            {
                issues.Add($"Missing promoted prefab: `{path}`.");
            }
        }

        private static void WriteReport(IReadOnlyCollection<string> issues)
        {
            List<string> lines = new List<string>
            {
                "# Intro Air Raid Prelude Verification",
                string.Empty,
                $"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                string.Empty,
                "## Artifacts",
                string.Empty,
                $"- Scene: `{ReviewScenePath}`",
                $"- Timeline: `{TimelinePath}`",
                $"- PlayableDirector: `{RootName}/{DirectorName}`",
                $"- Root: `{RootName}`",
                string.Empty,
                "## Timeline Surface",
                string.Empty,
                "- `Cinemachine Shots`: four editable shot clips.",
                "- `Aircraft Formation Move`: Timeline-keyed parent motion for three auto-forward aircraft.",
                "- `Bomber Run Move`: Timeline-keyed parent motion for the scripted stealth bomber.",
                "- `Aerial Bombing Active`: activation clip for `Effect_40_AerialBombing`.",
                "- `Impact Effects Active`: activation clip for `Effect_41` explosion prefabs.",
                string.Empty,
                "## Source Logic Used",
                string.Empty,
                "- `Effect_46_JetBomber` / `Effect_46_JetStriker`: `VariousTranslateMove` auto-forward logic.",
                "- `Effect_46_CyberStealthBomber`: auto-forward bomber plus delayed shot-point makers.",
                "- `Effect_40_AerialBombing`: repeated rising/drop/impact bomb spawners.",
                string.Empty,
                "## Protected Area",
                string.Empty,
                "- Existing `IntroGatePodCutsceneReview.unity` was not the target scene.",
                "- Existing `DB_Timeline_IntroGatePodAwakening.playable` was not the target Timeline.",
                "- No first-person/capsule marker is part of this review scene.",
                string.Empty,
                "## Result",
                string.Empty,
                issues.Count == 0 ? "PASS" : "FAIL",
                string.Empty
            };

            if (issues.Count > 0)
            {
                lines.Add("## Issues");
                lines.Add(string.Empty);
                foreach (string issue in issues)
                {
                    lines.Add($"- {issue}");
                }
                lines.Add(string.Empty);
            }

            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath) ?? "C:/tmp");
            File.WriteAllLines(ReportPath, lines);
        }

        private readonly struct ShotSpec
        {
            public ShotSpec(
                string id,
                double startSeconds,
                double durationSeconds,
                Vector3 cameraStart,
                Vector3 lookAtStart,
                Vector3 cameraEnd,
                Vector3 lookAtEnd,
                float fieldOfView,
                double blendInSeconds)
            {
                Id = id;
                StartSeconds = startSeconds;
                DurationSeconds = durationSeconds;
                CameraStart = cameraStart;
                LookAtStart = lookAtStart;
                CameraEnd = cameraEnd;
                LookAtEnd = lookAtEnd;
                FieldOfView = fieldOfView;
                BlendInSeconds = blendInSeconds;
            }

            public readonly string Id;
            public readonly double StartSeconds;
            public readonly double DurationSeconds;
            public readonly Vector3 CameraStart;
            public readonly Vector3 LookAtStart;
            public readonly Vector3 CameraEnd;
            public readonly Vector3 LookAtEnd;
            public readonly float FieldOfView;
            public readonly double BlendInSeconds;
        }
    }
}
