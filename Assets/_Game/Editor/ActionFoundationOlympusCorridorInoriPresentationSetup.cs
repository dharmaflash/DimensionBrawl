using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace DimensionBrawl.Editor
{
    public static class ActionFoundationOlympusCorridorInoriPresentationSetup
    {
        private const string DenseScenePath = "Assets/_Game/Scenes/Lookdev/OlympusCorridorDenseLookdev.unity";
        private const string InvasionScenePath = "Assets/_Game/Scenes/Lookdev/OlympusCorridorInvasionLookdev.unity";
        private const string InoriModelPath = ActionFoundationInoriPlayerVisualAssetSetup.ModelPath;
        private const string InoriWalkPosePath = "Assets/_Imported/AssetStore/RoloArt/Inori/Demo/UnityChan Animations/WALK00_F.anim";
        private const string PresentationProfilePath = "Assets/_Game/Art/Environment/OlympusCorridor/Profiles/DB_OlympusCorridor_InoriPresentationPostProcess.asset";
        private const string LegacyInoriInstanceName = "Inori_MagicaCloth2_Costume1";
        private const string InoriInstanceName = "Inori_PromotedLookdev";
        private const string PresentationRootName = "OlympusCorridor_InoriPresentationLookdev";
        private const string PresentationVolumeName = "InoriPresentation_WarmPostProcess";
        private const string PortraitCameraName = "OlympusCorridor_InoriPortraitCamera";
        private const string FullBodyCameraName = "OlympusCorridor_InoriFullBodyCamera";
        private const string PreviewFileName = "olympus-corridor-inori-presentation-preview.png";
        private const string FullBodyPreviewFileName = "olympus-corridor-inori-fullbody-preview.png";
        private const string BlueRiftSanctuaryRootName = "OlympusCorridor_BlueRiftSanctuary";
        private const string RetiredDefaultDirectionalLightName = "DirectionalLight";
        private const string DefaultPointLightName = "Point Light";
        private const string DefaultDirectionalLightName = "Directional Light";

        private static readonly Vector3 InoriPosition = new Vector3(-6.792f, 0.5f, 0.02f);
        private static readonly Quaternion InoriRotation = Quaternion.Euler(0f, 270f, 0f);

        [MenuItem("DimensionBrawl/Apply Olympus Corridor Inori Presentation Lookdev")]
        public static void ApplyInoriPresentationLookdevMenu()
        {
            ApplyInoriPresentationLookdev();
            Debug.Log("Applied Olympus corridor Inori presentation lookdev.");
        }

        [MenuItem("DimensionBrawl/Render Olympus Corridor Inori Presentation Preview")]
        public static void RenderInoriPresentationPreviewMenu()
        {
            string previewPath = RenderInoriPresentationPreview();
            Debug.Log($"Rendered Olympus corridor Inori presentation preview: {previewPath}");
        }

        [MenuItem("DimensionBrawl/Render Olympus Corridor Inori Full Body Preview")]
        public static void RenderInoriFullBodyPreviewMenu()
        {
            string previewPath = RenderInoriFullBodyPreview();
            Debug.Log($"Rendered Olympus corridor Inori full body preview: {previewPath}");
        }

        [MenuItem("DimensionBrawl/Apply Olympus Corridor Inori Presentation To Invasion Lookdev")]
        public static void ApplyInoriPresentationToInvasionLookdevMenu()
        {
            ApplyInoriPresentationLookdev(InvasionScenePath);
            Debug.Log("Applied Olympus corridor Inori presentation lookdev to the invasion scene.");
        }

        public static void ApplyInoriPresentationLookdev()
        {
            ApplyInoriPresentationLookdev(DenseScenePath);
        }

        private static void ApplyInoriPresentationLookdev(string scenePath)
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                throw new InvalidOperationException($"Scene is invalid: {scenePath}");
            }

            RemoveRoot(scene, LegacyInoriInstanceName);
            GameObject inori = FindSceneObjectByName(scene, InoriInstanceName) ?? InstantiateInori(scene);
            RevertInoriPrefabOverrides(inori);
            ApplyPromotedInoriMaterials(inori);
            ConfigureInoriPlacement(inori);
            ConfigureInoriPoseAndExpression(inori);
            ConfigurePresentationSceneBaseline(scene);

            RemoveRoot(scene, PresentationRootName);
            GameObject root = new GameObject(PresentationRootName);
            SceneManager.MoveGameObjectToScene(root, scene);

            ConfigurePresentationAmbient();
            ConfigurePresentationVolume(root.transform);
            ConfigurePresentationLights(root.transform, inori.transform);
            ConfigurePortraitCamera(root.transform, inori.transform);
            ConfigureFullBodyCamera(root.transform, inori.transform);

            EditorUtility.SetDirty(root);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }

        public static string RenderInoriPresentationPreview()
        {
            ApplyInoriPresentationLookdev();
            Scene scene = EditorSceneManager.OpenScene(DenseScenePath, OpenSceneMode.Single);
            Camera camera = RequireCamera(scene, PortraitCameraName);
            return RenderPreview(camera, PreviewFileName);
        }

        public static string RenderInoriFullBodyPreview()
        {
            ApplyInoriPresentationLookdev();
            Scene scene = EditorSceneManager.OpenScene(DenseScenePath, OpenSceneMode.Single);
            Camera camera = RequireCamera(scene, FullBodyCameraName);
            return RenderPreview(camera, FullBodyPreviewFileName);
        }

        private static GameObject InstantiateInori(Scene scene)
        {
            GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(InoriModelPath);
            if (model == null)
            {
                throw new InvalidOperationException($"Missing promoted Inori model: {InoriModelPath}");
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(model, scene) as GameObject;
            if (instance == null)
            {
                instance = UnityEngine.Object.Instantiate(model);
                SceneManager.MoveGameObjectToScene(instance, scene);
            }

            instance.name = InoriInstanceName;
            return instance;
        }

        private static void RevertInoriPrefabOverrides(GameObject inori)
        {
            if (PrefabUtility.GetPrefabInstanceStatus(inori) != PrefabInstanceStatus.Connected)
            {
                return;
            }

            PrefabUtility.RevertPrefabInstance(inori, InteractionMode.AutomatedAction);
            inori.name = InoriInstanceName;
            EditorUtility.SetDirty(inori);
        }

        private static void ConfigureInoriPlacement(GameObject inori)
        {
            inori.transform.SetPositionAndRotation(InoriPosition, InoriRotation);
            inori.transform.localScale = Vector3.one;
            EditorUtility.SetDirty(inori.transform);
        }

        private static void ApplyPromotedInoriMaterials(GameObject inori)
        {
            foreach (Renderer renderer in inori.GetComponentsInChildren<Renderer>(includeInactive: true))
            {
                Material[] materials = renderer.sharedMaterials;
                for (int i = 0; i < materials.Length; i++)
                {
                    string sourceName = materials[i] != null ? materials[i].name : string.Empty;
                    materials[i] = ActionFoundationInoriPlayerVisualAssetSetup.ResolvePromotedMaterial(sourceName, i);
                }

                renderer.sharedMaterials = materials;
                EditorUtility.SetDirty(renderer);
            }
        }

        private static void ConfigureInoriPoseAndExpression(GameObject inori)
        {
            Vector3 position = inori.transform.position;
            Quaternion rotation = inori.transform.rotation;
            Vector3 scale = inori.transform.localScale;

            AnimationClip walkPose = AssetDatabase.LoadAssetAtPath<AnimationClip>(InoriWalkPosePath);
            if (walkPose != null)
            {
                float sampleTime = walkPose.length > 0.01f ? Mathf.Min(walkPose.length * 0.38f, walkPose.length - 0.001f) : 0f;
                walkPose.SampleAnimation(inori, sampleTime);
                inori.transform.SetPositionAndRotation(position, rotation);
                inori.transform.localScale = scale;
            }

            foreach (Transform transform in inori.GetComponentsInChildren<Transform>(includeInactive: true))
            {
                EditorUtility.SetDirty(transform);
            }

            ResetBlendShapes(inori);
            ApplyBlendShape(inori, "mouthSmileLeft", 34f);
            ApplyBlendShape(inori, "mouthSmileRight", 34f);
            ApplyBlendShape(inori, "mouthStretchLeft", 10f);
            ApplyBlendShape(inori, "mouthStretchRight", 10f);
            ApplyBlendShape(inori, "mouthDimpleLeft", 12f);
            ApplyBlendShape(inori, "mouthDimpleRight", 12f);
            ApplyBlendShape(inori, "\u7167\u308C2", 30f);
        }

        private static void ResetBlendShapes(GameObject root)
        {
            foreach (SkinnedMeshRenderer renderer in root.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true))
            {
                Mesh mesh = renderer.sharedMesh;
                if (mesh == null)
                {
                    continue;
                }

                for (int i = 0; i < mesh.blendShapeCount; i++)
                {
                    renderer.SetBlendShapeWeight(i, 0f);
                }

                EditorUtility.SetDirty(renderer);
            }
        }

        private static void ApplyBlendShape(GameObject root, string blendShapeName, float weight)
        {
            foreach (SkinnedMeshRenderer renderer in root.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true))
            {
                Mesh mesh = renderer.sharedMesh;
                if (mesh == null)
                {
                    continue;
                }

                for (int i = 0; i < mesh.blendShapeCount; i++)
                {
                    if (!string.Equals(mesh.GetBlendShapeName(i), blendShapeName, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    renderer.SetBlendShapeWeight(i, weight);
                    EditorUtility.SetDirty(renderer);
                    return;
                }
            }
        }

        private static void ConfigurePresentationAmbient()
        {
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = Color.white;
            RenderSettings.ambientEquatorColor = new Color(0.88f, 0.86f, 0.95f, 1f);
            RenderSettings.ambientGroundColor = new Color(0.76f, 0.74f, 0.86f, 1f);
            RenderSettings.ambientIntensity = 0.66f;
            RenderSettings.fog = false;
        }

        private static void ConfigurePresentationVolume(Transform root)
        {
            VolumeProfile profile = EnsurePresentationProfile();
            GameObject volumeObject = CreateChild(root, PresentationVolumeName, Vector3.zero, Quaternion.identity);
            Volume volume = volumeObject.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 95f;
            volume.weight = 1f;
            volume.sharedProfile = profile;
            EditorUtility.SetDirty(volume);
        }

        private static VolumeProfile EnsurePresentationProfile()
        {
            EnsureFolder("Assets/_Game/Art/Environment/OlympusCorridor/Profiles");
            VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(PresentationProfilePath);
            if (ProfileHasMissingComponents(profile))
            {
                AssetDatabase.DeleteAsset(PresentationProfilePath);
                profile = null;
            }

            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, PresentationProfilePath);
                AssetDatabase.SaveAssets();
            }

            Bloom bloom = GetOrAddVolumeComponent<Bloom>(profile);
            bloom.active = true;
            SetParameter(bloom.threshold, 0.98f);
            SetParameter(bloom.intensity, 0.04f);
            SetParameter(bloom.scatter, 0.56f);
            SetParameter(bloom.clamp, 3.2f);
            SetParameter(bloom.tint, Color.white);
            SetParameter(bloom.highQualityFiltering, false);
            SetParameter(bloom.maxIterations, 4);

            Tonemapping tonemapping = GetOrAddVolumeComponent<Tonemapping>(profile);
            tonemapping.active = true;
            SetParameter(tonemapping.mode, TonemappingMode.Neutral);

            ColorAdjustments color = GetOrAddVolumeComponent<ColorAdjustments>(profile);
            color.active = true;
            SetParameter(color.postExposure, 0.2f);
            SetParameter(color.contrast, -2f);
            SetParameter(color.colorFilter, new Color(1f, 0.99f, 1f, 1f));
            SetParameter(color.hueShift, 0f);
            SetParameter(color.saturation, 18f);

            WhiteBalance whiteBalance = GetOrAddVolumeComponent<WhiteBalance>(profile);
            whiteBalance.active = true;
            SetParameter(whiteBalance.temperature, 0f);
            SetParameter(whiteBalance.tint, 4f);

            Vignette vignette = GetOrAddVolumeComponent<Vignette>(profile);
            vignette.active = true;
            SetParameter(vignette.color, new Color(0.08f, 0.045f, 0.07f, 1f));
            SetParameter(vignette.center, new Vector2(0.5f, 0.48f));
            SetParameter(vignette.intensity, 0f);
            SetParameter(vignette.smoothness, 0.35f);

            DepthOfField depthOfField = GetOrAddVolumeComponent<DepthOfField>(profile);
            depthOfField.active = true;
            SetParameter(depthOfField.mode, DepthOfFieldMode.Gaussian);
            SetParameter(depthOfField.gaussianStart, 18f);
            SetParameter(depthOfField.gaussianEnd, 52f);
            SetParameter(depthOfField.gaussianMaxRadius, 0.08f);
            SetParameter(depthOfField.highQualitySampling, false);

            foreach (VolumeComponent component in profile.components)
            {
                if (component != null)
                {
                    EditorUtility.SetDirty(component);
                }
            }

            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            return profile;
        }

        private static bool ProfileHasMissingComponents(VolumeProfile profile)
        {
            if (profile == null)
            {
                return false;
            }

            foreach (VolumeComponent component in profile.components)
            {
                if (component == null)
                {
                    return true;
                }
            }

            return false;
        }

        private static void ConfigurePresentationLights(Transform root, Transform inori)
        {
            GameObject lightingRoot = CreateChild(root, "InoriPresentation_Lighting", Vector3.zero, Quaternion.identity);
            Vector3 basePosition = inori.position;

            Light key = CreateLight(lightingRoot.transform, "SoftWhiteProductKey", basePosition + new Vector3(-2.75f, 2.15f, -1.25f));
            key.type = LightType.Spot;
            key.color = Color.white;
            key.intensity = 1.42f;
            key.range = 9.2f;
            key.spotAngle = 78f;
            key.innerSpotAngle = 52f;
            key.transform.rotation = Quaternion.LookRotation((basePosition + new Vector3(0f, 1.18f, 0f) - key.transform.position).normalized, Vector3.up);
            key.shadows = LightShadows.None;
            key.bounceIntensity = 0f;

            Light faceFill = CreateLight(lightingRoot.transform, "MilkyFaceFill", basePosition + new Vector3(-1.35f, 1.38f, 0.34f));
            faceFill.type = LightType.Point;
            faceFill.color = Color.white;
            faceFill.intensity = 0.4f;
            faceFill.range = 5.2f;
            faceFill.shadows = LightShadows.None;
            faceFill.bounceIntensity = 0f;

            Light lavenderFill = CreateLight(lightingRoot.transform, "SoftLavenderShadowFill", basePosition + new Vector3(-0.2f, 2.1f, 1.45f));
            lavenderFill.type = LightType.Point;
            lavenderFill.color = new Color(0.82f, 0.78f, 1f, 1f);
            lavenderFill.intensity = 0.32f;
            lavenderFill.range = 6.4f;
            lavenderFill.shadows = LightShadows.None;
            lavenderFill.bounceIntensity = 0f;

            Light hairRim = CreateLight(lightingRoot.transform, "WhiteHairSeparationRim", basePosition + new Vector3(1.2f, 1.95f, 1.05f));
            hairRim.type = LightType.Point;
            hairRim.color = Color.white;
            hairRim.intensity = 0.68f;
            hairRim.range = 5.4f;
            hairRim.shadows = LightShadows.None;
            hairRim.bounceIntensity = 0f;

            Light floorBounce = CreateLight(lightingRoot.transform, "SoftFloorBounce", basePosition + new Vector3(-0.85f, 0.28f, 0.72f));
            floorBounce.type = LightType.Point;
            floorBounce.color = new Color(0.92f, 0.9f, 1f, 1f);
            floorBounce.intensity = 0.22f;
            floorBounce.range = 4.8f;
            floorBounce.shadows = LightShadows.None;
            floorBounce.bounceIntensity = 0f;

            Light corridorWash = CreateLight(lightingRoot.transform, "PaleProductStageWash", basePosition + new Vector3(-2.2f, 4.5f, -2.7f));
            corridorWash.type = LightType.Directional;
            corridorWash.transform.rotation = Quaternion.Euler(42f, 112f, 0f);
            corridorWash.color = Color.white;
            corridorWash.intensity = 0.3f;
            corridorWash.shadows = LightShadows.None;
            corridorWash.bounceIntensity = 0f;

            foreach (Light light in lightingRoot.GetComponentsInChildren<Light>())
            {
                EditorUtility.SetDirty(light);
            }
        }

        private static void ConfigurePortraitCamera(Transform root, Transform inori)
        {
            Vector3 target = inori.position + new Vector3(0f, 1.28f, 0.02f);
            Vector3 position = inori.position + new Vector3(-2.9f, 1.18f, -0.42f);
            Camera camera = CreateCamera(root, PortraitCameraName, position, target);
            camera.fieldOfView = 28f;
            camera.nearClipPlane = 0.05f;
            camera.farClipPlane = 140f;
        }

        private static void ConfigureFullBodyCamera(Transform root, Transform inori)
        {
            Vector3 target = inori.position + new Vector3(0f, 0.98f, 0.02f);
            Vector3 position = inori.position + new Vector3(-3.95f, 1.08f, -0.36f);
            Camera camera = CreateCamera(root, FullBodyCameraName, position, target);
            camera.fieldOfView = 30f;
            camera.nearClipPlane = 0.05f;
            camera.farClipPlane = 140f;
        }

        private static Camera CreateCamera(Transform root, string name, Vector3 position, Vector3 target)
        {
            GameObject cameraObject = CreateChild(root, name, position, Quaternion.LookRotation((target - position).normalized, Vector3.up));
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.allowHDR = true;
            camera.allowMSAA = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.white;
            camera.depth = 30f;

            UniversalAdditionalCameraData cameraData = camera.GetUniversalAdditionalCameraData();
            cameraData.renderPostProcessing = true;
            cameraData.antialiasing = AntialiasingMode.FastApproximateAntialiasing;
            cameraData.antialiasingQuality = AntialiasingQuality.High;

            EditorUtility.SetDirty(camera);
            EditorUtility.SetDirty(cameraData);
            return camera;
        }

        private static void ConfigurePresentationSceneBaseline(Scene scene)
        {
            SetSceneObjectActive(scene, BlueRiftSanctuaryRootName, false);
            SetSceneLightEnabled(scene, RetiredDefaultDirectionalLightName, false);
            RemoveRoot(scene, DefaultPointLightName);
            RemoveRoot(scene, DefaultDirectionalLightName);
        }

        private static void SetSceneObjectActive(Scene scene, string objectName, bool active)
        {
            GameObject sceneObject = FindSceneObjectByName(scene, objectName);
            if (sceneObject == null)
            {
                return;
            }

            sceneObject.SetActive(active);
            EditorUtility.SetDirty(sceneObject);
        }

        private static void SetSceneLightEnabled(Scene scene, string objectName, bool enabled)
        {
            GameObject sceneObject = FindSceneObjectByName(scene, objectName);
            if (sceneObject == null)
            {
                return;
            }

            Light light = sceneObject.GetComponent<Light>();
            if (light == null)
            {
                return;
            }

            light.enabled = enabled;
            EditorUtility.SetDirty(light);
        }

        private static string RenderPreview(Camera camera, string fileName)
        {
            const int width = 1280;
            const int height = 720;
            string previewPath = Path.Combine(Path.GetTempPath(), fileName);

            RenderTexture previousTarget = camera.targetTexture;
            RenderTexture previousActive = RenderTexture.active;
            RenderTexture renderTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
            Texture2D preview = new Texture2D(width, height, TextureFormat.RGB24, mipChain: false);

            try
            {
                camera.targetTexture = renderTexture;
                RenderTexture.active = renderTexture;
                camera.Render();
                preview.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
                preview.Apply();
                File.WriteAllBytes(previewPath, preview.EncodeToPNG());
                return previewPath;
            }
            finally
            {
                camera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                RenderTexture.ReleaseTemporary(renderTexture);
                UnityEngine.Object.DestroyImmediate(preview);
            }
        }

        private static Camera RequireCamera(Scene scene, string cameraName)
        {
            GameObject cameraObject = FindSceneObjectByName(scene, cameraName);
            if (cameraObject == null)
            {
                throw new InvalidOperationException($"Missing camera in scene: {cameraName}");
            }

            Camera camera = cameraObject.GetComponent<Camera>();
            if (camera == null)
            {
                throw new InvalidOperationException($"{cameraName} is missing a Camera component.");
            }

            return camera;
        }

        private static GameObject CreateChild(Transform parent, string name, Vector3 position, Quaternion rotation)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(parent, worldPositionStays: false);
            child.transform.position = position;
            child.transform.rotation = rotation;
            return child;
        }

        private static Light CreateLight(Transform parent, string name, Vector3 position)
        {
            GameObject lightObject = CreateChild(parent, name, position, Quaternion.identity);
            return lightObject.AddComponent<Light>();
        }

        private static GameObject FindSceneObjectByName(Scene scene, string objectName)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (string.Equals(root.name, objectName, StringComparison.Ordinal))
                {
                    return root;
                }

                Transform found = FindChildRecursive(root.transform, objectName);
                if (found != null)
                {
                    return found.gameObject;
                }
            }

            return null;
        }

        private static Transform FindChildRecursive(Transform parent, string childName)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (string.Equals(child.name, childName, StringComparison.Ordinal))
                {
                    return child;
                }

                Transform nested = FindChildRecursive(child, childName);
                if (nested != null)
                {
                    return nested;
                }
            }

            return null;
        }

        private static void RemoveRoot(Scene scene, string rootName)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (string.Equals(root.name, rootName, StringComparison.Ordinal))
                {
                    UnityEngine.Object.DestroyImmediate(root);
                    return;
                }
            }
        }

        private static T GetOrAddVolumeComponent<T>(VolumeProfile profile) where T : VolumeComponent
        {
            if (!profile.TryGet(out T component))
            {
                component = profile.Add<T>(overrides: true);
            }

            if (!AssetDatabase.Contains(component))
            {
                component.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
                AssetDatabase.AddObjectToAsset(component, profile);
            }

            EditorUtility.SetDirty(component);
            EditorUtility.SetDirty(profile);
            return component;
        }

        private static void SetParameter<T>(VolumeParameter<T> parameter, T value)
        {
            parameter.overrideState = true;
            parameter.value = value;
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string parent = Path.GetDirectoryName(folderPath)?.Replace('\\', '/');
            string folderName = Path.GetFileName(folderPath);
            if (!string.IsNullOrEmpty(parent))
            {
                EnsureFolder(parent);
            }

            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder(parent, folderName);
            }
        }
    }
}
