using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace DimensionBrawl.Editor
{
    public static class MobilePerformanceBaselineReporter
    {
        private const string MarkdownReportPath = "C:/tmp/DimensionBrawl-MobilePerformanceBaseline.md";
        private const string JsonReportPath = "C:/tmp/DimensionBrawl-MobilePerformanceBaseline.json";

        private static readonly SceneTarget[] SceneTargets =
        {
            new("Olympus Corridor", "Assets/_Game/Scenes/OlympusCorridorInvasionStage.unity", "Runtime stage"),
            new("Olympus Station", "Assets/_Game/Scenes/OlympusStationCombatStage.unity", "Runtime stage")
        };

        [MenuItem("DimensionBrawl/Performance/Generate Mobile Performance Baseline")]
        public static void GenerateMenuReport()
        {
            GenerateBatchReport();
        }

        public static void GenerateBatchReport()
        {
            if (HasDirtyOpenScene(out string dirtyScenePath))
            {
                throw new InvalidOperationException(
                    $"Cannot inspect scenes while an open scene is dirty: {dirtyScenePath}");
            }

            SceneSetup[] originalSetup = EditorSceneManager.GetSceneManagerSetup();
            BaselineReport report = new()
            {
                GeneratedUtc = DateTime.UtcNow.ToString("O"),
                UnityVersion = Application.unityVersion,
                ActiveBuildTarget = EditorUserBuildSettings.activeBuildTarget.ToString()
            };

            try
            {
                for (int i = 0; i < SceneTargets.Length; i++)
                {
                    SceneTarget target = SceneTargets[i];
                    if (AssetDatabase.LoadAssetAtPath<SceneAsset>(target.Path) == null)
                    {
                        report.Errors.Add($"Scene asset is missing: {target.Path}");
                        continue;
                    }

                    Scene scene = EditorSceneManager.OpenScene(target.Path, OpenSceneMode.Single);
                    bool dirtyBefore = scene.isDirty;
                    SceneMetrics metrics = InspectScene(scene, target);
                    bool dirtyAfter = scene.isDirty;

                    if (dirtyBefore != dirtyAfter || dirtyAfter)
                    {
                        report.Errors.Add($"Read-only inspection changed the scene dirty state: {target.Path}");
                    }

                    report.Scenes.Add(metrics);
                }
            }
            finally
            {
                if (originalSetup.Length > 0)
                {
                    EditorSceneManager.RestoreSceneManagerSetup(originalSetup);
                }
            }

            WriteReports(report);
            if (report.Errors.Count > 0)
            {
                throw new InvalidOperationException(
                    $"Mobile performance baseline completed with {report.Errors.Count} error(s). See {MarkdownReportPath}");
            }

            Debug.Log(
                $"Mobile performance baseline generated for {report.Scenes.Count} canonical combat scenes. " +
                $"Reports: {MarkdownReportPath} and {JsonReportPath}");
        }

        private static bool HasDirtyOpenScene(out string dirtyScenePath)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (!scene.isDirty)
                {
                    continue;
                }

                dirtyScenePath = string.IsNullOrWhiteSpace(scene.path) ? scene.name : scene.path;
                return true;
            }

            dirtyScenePath = string.Empty;
            return false;
        }

        private static SceneMetrics InspectScene(Scene scene, SceneTarget target)
        {
            SceneMetrics metrics = new()
            {
                Label = target.Label,
                ScenePath = target.Path,
                SceneKind = target.Kind
            };
            HashSet<int> materialIds = new();
            HashSet<int> meshIds = new();
            Dictionary<int, MaterialUsageMetrics> materialUsages = new();
            Dictionary<int, MeshUsageMetrics> meshUsages = new();
            HashSet<string> colliderModelAssetPaths = new(StringComparer.Ordinal);
            Dictionary<string, FrameLoopMetrics> loopMetrics = new(StringComparer.Ordinal);
            GameObject[] roots = scene.GetRootGameObjects();
            metrics.RootCount = roots.Length;

            for (int i = 0; i < roots.Length; i++)
            {
                CollectRootMetrics(
                    roots[i],
                    metrics,
                    materialIds,
                    meshIds,
                    materialUsages,
                    meshUsages,
                    colliderModelAssetPaths,
                    loopMetrics);
                CollectHierarchyBranches(roots[i].transform, roots[i].name, 0, metrics.Branches);
            }

            metrics.UniqueMaterialCount = materialIds.Count;
            metrics.UniqueMeshCount = meshIds.Count;
            metrics.Roots.Sort(CompareRootPressure);
            metrics.Branches.Sort(CompareRootPressure);
            metrics.Lights.Sort(CompareLights);
            foreach (MaterialUsageMetrics materialUsage in materialUsages.Values)
            {
                metrics.MaterialUsages.Add(materialUsage);
            }

            metrics.MaterialUsages.Sort(CompareMaterialUsages);
            foreach (MeshUsageMetrics meshUsage in meshUsages.Values)
            {
                metrics.MeshUsages.Add(meshUsage);
                if (meshUsage.InstanceCount > 0 && meshUsage.ReadWriteEnabled)
                {
                    metrics.ReadWriteRenderedMeshAssetCount++;
                    if (!colliderModelAssetPaths.Contains(meshUsage.AssetPath))
                    {
                        metrics.ReadWriteRenderOnlyMeshAssetCount++;
                        metrics.ReadWriteRenderOnlyMeshRuntimeBytes += meshUsage.RuntimeMemoryBytes;
                    }
                }
            }

            metrics.MeshUsages.Sort(CompareMeshUsages);
            metrics.ColliderModelAssetCount = colliderModelAssetPaths.Count;
            CollectColliderUsages(scene, metrics);
            metrics.ColliderUsages.Sort(CompareColliderUsages);

            foreach (FrameLoopMetrics loop in loopMetrics.Values)
            {
                metrics.FrameLoops.Add(loop);
            }

            metrics.FrameLoops.Sort(CompareFrameLoops);
            AddObservations(metrics);
            return metrics;
        }

        private static void CollectRootMetrics(
            GameObject root,
            SceneMetrics scene,
            HashSet<int> materialIds,
            HashSet<int> meshIds,
            Dictionary<int, MaterialUsageMetrics> materialUsages,
            Dictionary<int, MeshUsageMetrics> meshUsages,
            HashSet<string> colliderModelAssetPaths,
            Dictionary<string, FrameLoopMetrics> loopMetrics)
        {
            RootMetrics rootMetrics = new()
            {
                Name = root.name,
                Active = root.activeInHierarchy
            };

            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
            rootMetrics.GameObjectCount = transforms.Length;
            scene.GameObjectCount += transforms.Length;
            for (int i = 0; i < transforms.Length; i++)
            {
                GameObject gameObject = transforms[i].gameObject;
                if (gameObject.activeInHierarchy)
                {
                    scene.ActiveGameObjectCount++;
                }

                scene.MissingScriptCount +=
                    GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(gameObject);
            }

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            rootMetrics.RendererCount = renderers.Length;
            scene.RendererCount += renderers.Length;
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer is MeshRenderer)
                {
                    scene.MeshRendererCount++;
                }
                else if (renderer is SkinnedMeshRenderer)
                {
                    scene.SkinnedMeshRendererCount++;
                }

                bool isActive = renderer.enabled && renderer.gameObject.activeInHierarchy;
                if (!isActive)
                {
                    continue;
                }

                rootMetrics.ActiveRendererCount++;
                scene.ActiveRendererCount++;
                if (renderer.gameObject.isStatic)
                {
                    scene.StaticActiveRendererCount++;
                }

                if (renderer.shadowCastingMode != ShadowCastingMode.Off)
                {
                    rootMetrics.ShadowCasterCount++;
                    scene.ShadowCasterCount++;
                    float largestBoundsAxis = MaxAxis(renderer.bounds.size);
                    if (largestBoundsAxis < 1f)
                    {
                        scene.SmallShadowCasterCount++;
                    }
                    else if (largestBoundsAxis < 5f)
                    {
                        scene.MediumShadowCasterCount++;
                    }
                    else
                    {
                        scene.LargeShadowCasterCount++;
                    }
                }

                if (renderer.receiveShadows)
                {
                    scene.ShadowReceiverCount++;
                }

                Material[] materials = renderer.sharedMaterials;
                for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                {
                    if (materials[materialIndex] != null)
                    {
                        Material material = materials[materialIndex];
                        int materialId = material.GetInstanceID();
                        materialIds.Add(materialId);
                        if (!materialUsages.TryGetValue(materialId, out MaterialUsageMetrics materialUsage))
                        {
                            materialUsage = new MaterialUsageMetrics
                            {
                                MaterialName = material.name,
                                AssetPath = AssetDatabase.GetAssetPath(material),
                                ShaderName = material.shader != null ? material.shader.name : "Missing",
                                InstancingEnabled = material.enableInstancing
                            };
                            materialUsages.Add(materialId, materialUsage);
                        }

                        materialUsage.RendererReferenceCount++;
                    }
                }

                Mesh mesh = GetSharedMesh(renderer);
                if (mesh == null)
                {
                    continue;
                }

                long triangleCount = CountTriangles(mesh);
                rootMetrics.InstancedTriangleCount += triangleCount;
                scene.InstancedTriangleCount += triangleCount;
                if (meshIds.Add(mesh.GetInstanceID()))
                {
                    scene.UniqueMeshTriangleCount += triangleCount;
                }

                int meshId = mesh.GetInstanceID();
                if (!meshUsages.TryGetValue(meshId, out MeshUsageMetrics meshUsage))
                {
                    meshUsage = new MeshUsageMetrics
                    {
                        MeshName = mesh.name,
                        AssetPath = AssetDatabase.GetAssetPath(mesh),
                        TriangleCount = triangleCount,
                        RuntimeMemoryBytes = Profiler.GetRuntimeMemorySizeLong(mesh),
                        SamplePath = GetHierarchyPath(renderer.transform)
                    };
                    ModelImporter modelImporter = AssetImporter.GetAtPath(meshUsage.AssetPath) as ModelImporter;
                    meshUsage.ReadWriteEnabled = modelImporter != null && modelImporter.isReadable;
                    meshUsages.Add(meshId, meshUsage);
                }

                meshUsage.InstanceCount++;
                meshUsage.TotalInstancedTriangleCount += triangleCount;
                meshUsage.MaxWorldBoundsAxis = Mathf.Max(
                    meshUsage.MaxWorldBoundsAxis,
                    MaxAxis(renderer.bounds.size));
                if (renderer.shadowCastingMode != ShadowCastingMode.Off)
                {
                    meshUsage.ShadowCasterInstanceCount++;
                }
            }

            Light[] lights = root.GetComponentsInChildren<Light>(true);
            rootMetrics.LightCount = lights.Length;
            scene.LightCount += lights.Length;
            for (int i = 0; i < lights.Length; i++)
            {
                Light light = lights[i];
                if (!light.enabled || !light.gameObject.activeInHierarchy)
                {
                    continue;
                }

                rootMetrics.ActiveLightCount++;
                scene.ActiveLightCount++;
                switch (light.lightmapBakeType)
                {
                    case LightmapBakeType.Realtime:
                        rootMetrics.RealtimeLightCount++;
                        scene.RealtimeLightCount++;
                        break;
                    case LightmapBakeType.Mixed:
                        scene.MixedLightCount++;
                        break;
                    case LightmapBakeType.Baked:
                        scene.BakedLightCount++;
                        break;
                }

                if (light.shadows != LightShadows.None)
                {
                    scene.ShadowedLightCount++;
                }

                scene.Lights.Add(new LightMetrics
                {
                    Path = GetHierarchyPath(light.transform),
                    Type = light.type.ToString(),
                    BakeType = light.lightmapBakeType.ToString(),
                    Shadows = light.shadows.ToString(),
                    Range = light.range,
                    Intensity = light.intensity,
                    CullingMask = light.cullingMask
                });
            }

            ParticleSystem[] particles = root.GetComponentsInChildren<ParticleSystem>(true);
            rootMetrics.ParticleSystemCount = particles.Length;
            scene.ParticleSystemCount += particles.Length;
            for (int i = 0; i < particles.Length; i++)
            {
                ParticleSystem particle = particles[i];
                if (!particle.gameObject.activeInHierarchy)
                {
                    continue;
                }

                scene.ActiveParticleSystemCount++;
                ParticleSystem.MainModule main = particle.main;
                scene.ParticleMaxCountBudget += main.maxParticles;
                if (main.playOnAwake)
                {
                    scene.PlayOnAwakeParticleSystemCount++;
                }
            }

            Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
            rootMetrics.ColliderCount = colliders.Length;
            scene.ColliderCount += colliders.Length;
            for (int i = 0; i < colliders.Length; i++)
            {
                Collider collider = colliders[i];
                if (collider is MeshCollider)
                {
                    scene.MeshColliderCount++;
                    MeshCollider meshCollider = (MeshCollider)collider;
                    Mesh collisionMesh = meshCollider.sharedMesh;
                    if (collisionMesh != null)
                    {
                        string collisionAssetPath = AssetDatabase.GetAssetPath(collisionMesh);
                        if (!string.IsNullOrEmpty(collisionAssetPath))
                        {
                            colliderModelAssetPaths.Add(collisionAssetPath);
                        }

                        int collisionMeshId = collisionMesh.GetInstanceID();
                        if (!meshUsages.TryGetValue(collisionMeshId, out MeshUsageMetrics meshUsage))
                        {
                            string assetPath = AssetDatabase.GetAssetPath(collisionMesh);
                            ModelImporter modelImporter = AssetImporter.GetAtPath(assetPath) as ModelImporter;
                            meshUsage = new MeshUsageMetrics
                            {
                                MeshName = collisionMesh.name,
                                AssetPath = assetPath,
                                TriangleCount = CountTriangles(collisionMesh),
                                RuntimeMemoryBytes = Profiler.GetRuntimeMemorySizeLong(collisionMesh),
                                SamplePath = GetHierarchyPath(meshCollider.transform),
                                ReadWriteEnabled = modelImporter != null && modelImporter.isReadable
                            };
                            meshUsages.Add(collisionMeshId, meshUsage);
                        }

                        meshUsage.MeshColliderInstanceCount++;
                    }
                }
                else if (collider is BoxCollider)
                {
                    scene.BoxColliderCount++;
                }

                if (collider.enabled && collider.gameObject.activeInHierarchy)
                {
                    rootMetrics.ActiveColliderCount++;
                    scene.ActiveColliderCount++;
                    if (collider is MeshCollider)
                    {
                        scene.ActiveMeshColliderCount++;
                    }
                    else
                    {
                        scene.ActivePrimitiveColliderCount++;
                    }
                }
            }

            CountColliderOverlaps(transforms, scene);

            LODGroup[] lodGroups = root.GetComponentsInChildren<LODGroup>(true);
            rootMetrics.LodGroupCount = lodGroups.Length;
            scene.LodGroupCount += lodGroups.Length;
            for (int i = 0; i < lodGroups.Length; i++)
            {
                if (lodGroups[i].enabled && lodGroups[i].gameObject.activeInHierarchy)
                {
                    scene.ActiveLodGroupCount++;
                }
            }

            Camera[] cameras = root.GetComponentsInChildren<Camera>(true);
            scene.CameraCount += cameras.Length;
            for (int i = 0; i < cameras.Length; i++)
            {
                Camera camera = cameras[i];
                if (!camera.enabled || !camera.gameObject.activeInHierarchy)
                {
                    continue;
                }

                scene.ActiveCameraCount++;
                UniversalAdditionalCameraData cameraData = camera.GetComponent<UniversalAdditionalCameraData>();
                if (cameraData != null && cameraData.renderPostProcessing)
                {
                    scene.PostProcessingCameraCount++;
                }
            }

            AudioSource[] audioSources = root.GetComponentsInChildren<AudioSource>(true);
            scene.AudioSourceCount += audioSources.Length;
            for (int i = 0; i < audioSources.Length; i++)
            {
                AudioSource audioSource = audioSources[i];
                if (audioSource.enabled && audioSource.gameObject.activeInHierarchy)
                {
                    scene.ActiveAudioSourceCount++;
                    if (audioSource.playOnAwake)
                    {
                        scene.PlayOnAwakeAudioSourceCount++;
                    }
                }
            }

            MonoBehaviour[] behaviours = root.GetComponentsInChildren<MonoBehaviour>(true);
            rootMetrics.MonoBehaviourCount = behaviours.Length;
            scene.MonoBehaviourCount += behaviours.Length;
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour == null || !behaviour.enabled || !behaviour.gameObject.activeInHierarchy)
                {
                    continue;
                }

                rootMetrics.ActiveMonoBehaviourCount++;
                scene.ActiveMonoBehaviourCount++;
                CountFrameLoops(behaviour.GetType(), scene, loopMetrics);
            }

            scene.Roots.Add(rootMetrics);
        }

        private static void CollectHierarchyBranches(
            Transform parent,
            string parentPath,
            int depth,
            List<RootMetrics> branches)
        {
            if (depth >= 2)
            {
                return;
            }

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                string path = $"{parentPath}/{child.name}";
                branches.Add(InspectBranch(child.gameObject, path));
                CollectHierarchyBranches(child, path, depth + 1, branches);
            }
        }

        private static RootMetrics InspectBranch(GameObject branch, string path)
        {
            RootMetrics metrics = new()
            {
                Name = path,
                Active = branch.activeInHierarchy
            };
            Transform[] transforms = branch.GetComponentsInChildren<Transform>(true);
            metrics.GameObjectCount = transforms.Length;

            Renderer[] renderers = branch.GetComponentsInChildren<Renderer>(true);
            metrics.RendererCount = renderers.Length;
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (!renderer.enabled || !renderer.gameObject.activeInHierarchy)
                {
                    continue;
                }

                metrics.ActiveRendererCount++;
                if (renderer.shadowCastingMode != ShadowCastingMode.Off)
                {
                    metrics.ShadowCasterCount++;
                }

                Mesh mesh = GetSharedMesh(renderer);
                if (mesh != null)
                {
                    metrics.InstancedTriangleCount += CountTriangles(mesh);
                }
            }

            Light[] lights = branch.GetComponentsInChildren<Light>(true);
            metrics.LightCount = lights.Length;
            for (int i = 0; i < lights.Length; i++)
            {
                Light light = lights[i];
                if (!light.enabled || !light.gameObject.activeInHierarchy)
                {
                    continue;
                }

                metrics.ActiveLightCount++;
                if (light.lightmapBakeType == LightmapBakeType.Realtime)
                {
                    metrics.RealtimeLightCount++;
                }
            }

            Collider[] colliders = branch.GetComponentsInChildren<Collider>(true);
            metrics.ColliderCount = colliders.Length;
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i].enabled && colliders[i].gameObject.activeInHierarchy)
                {
                    metrics.ActiveColliderCount++;
                }
            }

            LODGroup[] lodGroups = branch.GetComponentsInChildren<LODGroup>(true);
            metrics.LodGroupCount = lodGroups.Length;

            MonoBehaviour[] behaviours = branch.GetComponentsInChildren<MonoBehaviour>(true);
            metrics.MonoBehaviourCount = behaviours.Length;
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour != null && behaviour.enabled && behaviour.gameObject.activeInHierarchy)
                {
                    metrics.ActiveMonoBehaviourCount++;
                }
            }

            return metrics;
        }

        private static void CountColliderOverlaps(Transform[] transforms, SceneMetrics scene)
        {
            for (int i = 0; i < transforms.Length; i++)
            {
                Collider[] colliders = transforms[i].GetComponents<Collider>();
                bool hasActiveMeshCollider = false;
                bool hasActivePrimitiveCollider = false;
                for (int colliderIndex = 0; colliderIndex < colliders.Length; colliderIndex++)
                {
                    Collider collider = colliders[colliderIndex];
                    if (!collider.enabled || !collider.gameObject.activeInHierarchy)
                    {
                        continue;
                    }

                    if (collider is MeshCollider)
                    {
                        hasActiveMeshCollider = true;
                    }
                    else
                    {
                        hasActivePrimitiveCollider = true;
                    }
                }

                if (hasActiveMeshCollider && hasActivePrimitiveCollider)
                {
                    scene.MeshAndPrimitiveColliderObjectCount++;
                }
            }
        }

        private static void CollectColliderUsages(Scene scene, SceneMetrics metrics)
        {
            var usages = new Dictionary<string, ColliderUsageMetrics>(StringComparer.Ordinal);
            Collider[] colliders = Resources.FindObjectsOfTypeAll<Collider>();
            for (int i = 0; i < colliders.Length; i++)
            {
                Collider collider = colliders[i];
                if (collider == null
                    || collider.gameObject.scene != scene
                    || !collider.enabled
                    || !collider.gameObject.activeInHierarchy)
                {
                    continue;
                }

                MeshFilter meshFilter = collider.GetComponent<MeshFilter>();
                Mesh renderMesh = meshFilter != null ? meshFilter.sharedMesh : null;
                Mesh collisionMesh = collider is MeshCollider meshCollider
                    ? meshCollider.sharedMesh
                    : null;
                Mesh groupingMesh = renderMesh != null ? renderMesh : collisionMesh;
                string sourceName = groupingMesh != null ? groupingMesh.name : collider.name;
                string key = $"{collider.GetType().Name}:{(groupingMesh != null ? groupingMesh.GetInstanceID() : sourceName)}";
                if (!usages.TryGetValue(key, out ColliderUsageMetrics usage))
                {
                    usage = new ColliderUsageMetrics
                    {
                        ColliderType = collider.GetType().Name,
                        SourceName = sourceName,
                        AssetPath = groupingMesh != null
                            ? AssetDatabase.GetAssetPath(groupingMesh)
                            : string.Empty,
                        TriangleCount = groupingMesh != null ? CountTriangles(groupingMesh) : 0L,
                        SamplePath = GetHierarchyPath(collider.transform)
                    };
                    usages.Add(key, usage);
                }

                usage.InstanceCount++;
                usage.TriggerCount += collider.isTrigger ? 1 : 0;
                if (collider is MeshCollider currentMeshCollider && currentMeshCollider.convex)
                {
                    usage.ConvexCount++;
                }

                usage.MaxWorldBoundsAxis = Mathf.Max(
                    usage.MaxWorldBoundsAxis,
                    MaxAxis(collider.bounds.size));
            }

            foreach (ColliderUsageMetrics usage in usages.Values)
            {
                metrics.ColliderUsages.Add(usage);
            }
        }

        private static float MaxAxis(Vector3 value)
        {
            return Mathf.Max(value.x, Mathf.Max(value.y, value.z));
        }

        private static string GetHierarchyPath(Transform transform)
        {
            StringBuilder builder = new(transform.name);
            Transform current = transform.parent;
            while (current != null)
            {
                builder.Insert(0, '/');
                builder.Insert(0, current.name);
                current = current.parent;
            }

            return builder.ToString();
        }

        private static Mesh GetSharedMesh(Renderer renderer)
        {
            if (renderer is SkinnedMeshRenderer skinnedMeshRenderer)
            {
                return skinnedMeshRenderer.sharedMesh;
            }

            if (renderer is MeshRenderer)
            {
                MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
                return meshFilter != null ? meshFilter.sharedMesh : null;
            }

            return null;
        }

        private static long CountTriangles(Mesh mesh)
        {
            long triangleCount = 0;
            for (int subMeshIndex = 0; subMeshIndex < mesh.subMeshCount; subMeshIndex++)
            {
                if (mesh.GetTopology(subMeshIndex) == MeshTopology.Triangles)
                {
                    triangleCount += (long)mesh.GetIndexCount(subMeshIndex) / 3L;
                }
            }

            return triangleCount;
        }

        private static void CountFrameLoops(
            Type behaviourType,
            SceneMetrics scene,
            Dictionary<string, FrameLoopMetrics> loopMetrics)
        {
            bool hasUpdate = HasUnityMessage(behaviourType, "Update");
            bool hasLateUpdate = HasUnityMessage(behaviourType, "LateUpdate");
            bool hasFixedUpdate = HasUnityMessage(behaviourType, "FixedUpdate");
            if (!hasUpdate && !hasLateUpdate && !hasFixedUpdate)
            {
                return;
            }

            string typeName = behaviourType.FullName ?? behaviourType.Name;
            if (!loopMetrics.TryGetValue(typeName, out FrameLoopMetrics metrics))
            {
                metrics = new FrameLoopMetrics { TypeName = typeName };
                loopMetrics.Add(typeName, metrics);
            }

            scene.ActiveFrameLoopBehaviourCount++;
            if (hasUpdate)
            {
                scene.UpdateBehaviourCount++;
                metrics.UpdateInstances++;
            }

            if (hasLateUpdate)
            {
                scene.LateUpdateBehaviourCount++;
                metrics.LateUpdateInstances++;
            }

            if (hasFixedUpdate)
            {
                scene.FixedUpdateBehaviourCount++;
                metrics.FixedUpdateInstances++;
            }
        }

        private static bool HasUnityMessage(Type type, string methodName)
        {
            while (type != null && type != typeof(MonoBehaviour))
            {
                MethodInfo method = type.GetMethod(
                    methodName,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly,
                    null,
                    Type.EmptyTypes,
                    null);
                if (method != null)
                {
                    return true;
                }

                type = type.BaseType;
            }

            return false;
        }

        private static int CompareRootPressure(RootMetrics left, RootMetrics right)
        {
            int rendererComparison = right.ActiveRendererCount.CompareTo(left.ActiveRendererCount);
            if (rendererComparison != 0)
            {
                return rendererComparison;
            }

            return right.InstancedTriangleCount.CompareTo(left.InstancedTriangleCount);
        }

        private static int CompareFrameLoops(FrameLoopMetrics left, FrameLoopMetrics right)
        {
            int leftCount = left.UpdateInstances + left.LateUpdateInstances + left.FixedUpdateInstances;
            int rightCount = right.UpdateInstances + right.LateUpdateInstances + right.FixedUpdateInstances;
            return rightCount.CompareTo(leftCount);
        }

        private static int CompareLights(LightMetrics left, LightMetrics right)
        {
            int leftShadowPriority = string.Equals(left.Shadows, LightShadows.None.ToString(), StringComparison.Ordinal) ? 0 : 1;
            int rightShadowPriority = string.Equals(right.Shadows, LightShadows.None.ToString(), StringComparison.Ordinal) ? 0 : 1;
            int shadowComparison = rightShadowPriority.CompareTo(leftShadowPriority);
            if (shadowComparison != 0)
            {
                return shadowComparison;
            }

            int rangeComparison = right.Range.CompareTo(left.Range);
            return rangeComparison != 0 ? rangeComparison : right.Intensity.CompareTo(left.Intensity);
        }

        private static int CompareMeshUsages(MeshUsageMetrics left, MeshUsageMetrics right)
        {
            int triangleComparison = right.TotalInstancedTriangleCount.CompareTo(left.TotalInstancedTriangleCount);
            return triangleComparison != 0
                ? triangleComparison
                : right.InstanceCount.CompareTo(left.InstanceCount);
        }

        private static int CompareMaterialUsages(MaterialUsageMetrics left, MaterialUsageMetrics right)
        {
            return right.RendererReferenceCount.CompareTo(left.RendererReferenceCount);
        }

        private static int CompareColliderUsages(ColliderUsageMetrics left, ColliderUsageMetrics right)
        {
            int instanceComparison = right.InstanceCount.CompareTo(left.InstanceCount);
            if (instanceComparison != 0)
            {
                return instanceComparison;
            }

            int triangleComparison = right.TriangleCount.CompareTo(left.TriangleCount);
            return triangleComparison != 0
                ? triangleComparison
                : string.Compare(left.SourceName, right.SourceName, StringComparison.Ordinal);
        }

        private static void AddObservations(SceneMetrics metrics)
        {
            if (metrics.ShadowCasterCount > 300)
            {
                metrics.Observations.Add(
                    $"High shadow caster inventory: {metrics.ShadowCasterCount:N0}. Mobile stages should limit dynamic shadow participation by role and distance.");
            }

            if (metrics.RealtimeLightCount > 8)
            {
                metrics.Observations.Add(
                    $"High realtime light inventory: {metrics.RealtimeLightCount:N0}. Prefer baked/emissive environment lighting and a small combat-light budget.");
            }

            if (metrics.ActiveRendererCount > 200 && metrics.ActiveLodGroupCount == 0)
            {
                metrics.Observations.Add(
                    $"No active LOD groups were found across {metrics.ActiveRendererCount:N0} active renderers.");
            }

            if (metrics.ActiveColliderCount > 500)
            {
                metrics.Observations.Add(
                    $"High active collider inventory: {metrics.ActiveColliderCount:N0}. Broadphase and contact cost need runtime profiling.");
            }

            if (metrics.ActiveFrameLoopBehaviourCount > 60)
            {
                metrics.Observations.Add(
                    $"High active per-frame behaviour inventory: {metrics.ActiveFrameLoopBehaviourCount:N0}. Consolidate repeated decorative updates after profiling.");
            }

            if (metrics.PostProcessingCameraCount > 1)
            {
                metrics.Observations.Add(
                    $"Multiple active cameras render post-processing: {metrics.PostProcessingCameraCount:N0}.");
            }

            if (metrics.MissingScriptCount > 0)
            {
                metrics.Observations.Add($"Missing MonoBehaviour slots: {metrics.MissingScriptCount:N0}.");
            }
        }

        private static void WriteReports(BaselineReport report)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(MarkdownReportPath) ?? "C:/tmp");
            File.WriteAllText(JsonReportPath, JsonUtility.ToJson(report, true), Encoding.UTF8);
            File.WriteAllText(MarkdownReportPath, BuildMarkdown(report), Encoding.UTF8);
        }

        private static string BuildMarkdown(BaselineReport report)
        {
            StringBuilder builder = new();
            builder.AppendLine("# DimensionBrawl Mobile Performance Baseline");
            builder.AppendLine();
            builder.AppendLine($"- Generated UTC: {report.GeneratedUtc}");
            builder.AppendLine($"- Unity: {report.UnityVersion}");
            builder.AppendLine($"- Active build target: {report.ActiveBuildTarget}");
            builder.AppendLine("- Scope: static, read-only inventory of canonical combat scenes");
            builder.AppendLine("- Note: triangle and component totals are scene inventory, not visibility, draw-call, frame-time, memory, or thermal measurements.");
            builder.AppendLine();
            builder.AppendLine("## Summary");
            builder.AppendLine();
            builder.AppendLine("| Scene | Active renderers | Instanced triangles | Materials | Shadow casters | Lights (RT/shadowed) | Active colliders | LOD groups | Frame-loop behaviours |");
            builder.AppendLine("|---|---:|---:|---:|---:|---:|---:|---:|---:|");

            for (int i = 0; i < report.Scenes.Count; i++)
            {
                SceneMetrics scene = report.Scenes[i];
                builder.AppendLine(
                    $"| {scene.Label} | {scene.ActiveRendererCount:N0} | {scene.InstancedTriangleCount:N0} | " +
                    $"{scene.UniqueMaterialCount:N0} | {scene.ShadowCasterCount:N0} | " +
                    $"{scene.ActiveLightCount:N0} ({scene.RealtimeLightCount:N0}/{scene.ShadowedLightCount:N0}) | " +
                    $"{scene.ActiveColliderCount:N0} | {scene.ActiveLodGroupCount:N0} | {scene.ActiveFrameLoopBehaviourCount:N0} |");
            }

            builder.AppendLine();
            builder.AppendLine("## Mobile Budget Direction");
            builder.AppendLine();
            builder.AppendLine("These are optimization directions, not pass/fail gates. Runtime profiler captures will establish device-specific budgets.");
            builder.AppendLine();
            builder.AppendLine("- Environment shadow casters: move toward a small, distance-bounded set; characters and major combat props retain priority.");
            builder.AppendLine("- Realtime lights: keep a tightly controlled combat-light pool; static fixtures should prefer baked or emissive presentation.");
            builder.AppendLine("- Geometry: add LOD or chunk visibility control before reducing authored visual identity.");
            builder.AppendLine("- Physics: separate traversal blockers from decorative mesh collision and profile broadphase cost.");
            builder.AppendLine("- CPU: consolidate repeated decorative frame loops only after a measured PlayerLoop capture.");
            builder.AppendLine();

            for (int i = 0; i < report.Scenes.Count; i++)
            {
                AppendSceneDetails(builder, report.Scenes[i]);
            }

            if (report.Errors.Count > 0)
            {
                builder.AppendLine("## Inspection Errors");
                builder.AppendLine();
                for (int i = 0; i < report.Errors.Count; i++)
                {
                    builder.AppendLine($"- {report.Errors[i]}");
                }
            }

            return builder.ToString();
        }

        private static void AppendSceneDetails(StringBuilder builder, SceneMetrics scene)
        {
            builder.AppendLine($"## {scene.Label}");
            builder.AppendLine();
            builder.AppendLine($"- Scene: `{scene.ScenePath}`");
            builder.AppendLine($"- Role: {scene.SceneKind}");
            builder.AppendLine($"- GameObjects: {scene.GameObjectCount:N0} total, {scene.ActiveGameObjectCount:N0} active, {scene.RootCount:N0} roots");
            builder.AppendLine($"- Renderers: {scene.RendererCount:N0} total, {scene.ActiveRendererCount:N0} active, {scene.StaticActiveRendererCount:N0} static, {scene.MeshRendererCount:N0} mesh, {scene.SkinnedMeshRendererCount:N0} skinned");
            builder.AppendLine($"- Geometry: {scene.InstancedTriangleCount:N0} instanced triangles, {scene.UniqueMeshTriangleCount:N0} unique-mesh triangles, {scene.UniqueMeshCount:N0} meshes");
            builder.AppendLine($"- Mesh CPU copies: {scene.ReadWriteRenderedMeshAssetCount:N0} rendered mesh assets have Read/Write enabled; {scene.ReadWriteRenderOnlyMeshAssetCount:N0} are in model assets with no MeshCollider refs ({scene.ReadWriteRenderOnlyMeshRuntimeBytes / (1024d * 1024d):N2} MiB measured mesh footprint); {scene.ColliderModelAssetCount:N0} model assets are preserved for collision");
            builder.AppendLine($"- Materials and shadows: {scene.UniqueMaterialCount:N0} materials, {scene.ShadowCasterCount:N0} casters ({scene.SmallShadowCasterCount:N0} under 1m, {scene.MediumShadowCasterCount:N0} under 5m, {scene.LargeShadowCasterCount:N0} 5m+), {scene.ShadowReceiverCount:N0} receivers");
            builder.AppendLine($"- Lights: {scene.LightCount:N0} total, {scene.ActiveLightCount:N0} active, {scene.RealtimeLightCount:N0} realtime, {scene.MixedLightCount:N0} mixed, {scene.BakedLightCount:N0} baked, {scene.ShadowedLightCount:N0} shadowed");
            builder.AppendLine($"- Physics: {scene.ColliderCount:N0} colliders, {scene.ActiveColliderCount:N0} active ({scene.ActiveMeshColliderCount:N0} mesh, {scene.ActivePrimitiveColliderCount:N0} primitive), {scene.MeshColliderCount:N0} mesh total, {scene.BoxColliderCount:N0} box total, {scene.MeshAndPrimitiveColliderObjectCount:N0} objects with both active mesh and primitive collision");
            builder.AppendLine($"- Particles: {scene.ParticleSystemCount:N0} total, {scene.ActiveParticleSystemCount:N0} active, {scene.PlayOnAwakeParticleSystemCount:N0} play-on-awake, max-particle sum {scene.ParticleMaxCountBudget:N0}");
            builder.AppendLine($"- Cameras: {scene.CameraCount:N0} total, {scene.ActiveCameraCount:N0} active, {scene.PostProcessingCameraCount:N0} active with post-processing");
            builder.AppendLine($"- Audio: {scene.AudioSourceCount:N0} total, {scene.ActiveAudioSourceCount:N0} active, {scene.PlayOnAwakeAudioSourceCount:N0} play-on-awake");
            builder.AppendLine($"- Scripts: {scene.MonoBehaviourCount:N0} total, {scene.ActiveMonoBehaviourCount:N0} active, {scene.ActiveFrameLoopBehaviourCount:N0} frame-loop behaviours ({scene.UpdateBehaviourCount:N0} Update, {scene.LateUpdateBehaviourCount:N0} LateUpdate, {scene.FixedUpdateBehaviourCount:N0} FixedUpdate), {scene.MissingScriptCount:N0} missing slots");
            builder.AppendLine($"- LOD: {scene.LodGroupCount:N0} total, {scene.ActiveLodGroupCount:N0} active");
            builder.AppendLine();

            builder.AppendLine("### Top Roots By Active Renderer Inventory");
            builder.AppendLine();
            builder.AppendLine("| Root | Active renderers | Triangles | Shadow casters | Active lights (RT) | Active colliders | Frame components | LOD groups |");
            builder.AppendLine("|---|---:|---:|---:|---:|---:|---:|---:|");
            int rootCount = Math.Min(12, scene.Roots.Count);
            for (int i = 0; i < rootCount; i++)
            {
                RootMetrics root = scene.Roots[i];
                builder.AppendLine(
                    $"| {EscapeTableCell(root.Name)} | {root.ActiveRendererCount:N0} | {root.InstancedTriangleCount:N0} | " +
                    $"{root.ShadowCasterCount:N0} | {root.ActiveLightCount:N0} ({root.RealtimeLightCount:N0}) | " +
                    $"{root.ActiveColliderCount:N0} | {root.ActiveMonoBehaviourCount:N0} | {root.LodGroupCount:N0} |");
            }

            builder.AppendLine();
            builder.AppendLine("### Top Materials By Renderer References");
            builder.AppendLine();
            builder.AppendLine("| Material | Renderer refs | Instancing | Shader | Asset |");
            builder.AppendLine("|---|---:|---|---|---|");
            int materialUsageCount = Math.Min(24, scene.MaterialUsages.Count);
            for (int i = 0; i < materialUsageCount; i++)
            {
                MaterialUsageMetrics material = scene.MaterialUsages[i];
                builder.AppendLine(
                    $"| {EscapeTableCell(material.MaterialName)} | {material.RendererReferenceCount:N0} | " +
                    $"{material.InstancingEnabled} | {EscapeTableCell(material.ShaderName)} | " +
                    $"{EscapeTableCell(material.AssetPath)} |");
            }

            builder.AppendLine();
            builder.AppendLine("### Top Meshes By Instanced Triangle Inventory");
            builder.AppendLine();
            builder.AppendLine("| Mesh | Instances | Triangles each | Total triangles | Shadow instances | MeshCollider refs | Read/Write | Memory | Max world bounds | Asset | Sample path |");
            builder.AppendLine("|---|---:|---:|---:|---:|---:|---|---:|---:|---|---|");
            int meshUsageCount = Math.Min(24, scene.MeshUsages.Count);
            for (int i = 0; i < meshUsageCount; i++)
            {
                MeshUsageMetrics mesh = scene.MeshUsages[i];
                builder.AppendLine(
                    $"| {EscapeTableCell(mesh.MeshName)} | {mesh.InstanceCount:N0} | {mesh.TriangleCount:N0} | " +
                    $"{mesh.TotalInstancedTriangleCount:N0} | {mesh.ShadowCasterInstanceCount:N0} | {mesh.MeshColliderInstanceCount:N0} | " +
                    $"{mesh.ReadWriteEnabled} | {mesh.RuntimeMemoryBytes / (1024d * 1024d):N2} MiB | " +
                    $"{mesh.MaxWorldBoundsAxis:0.###}m | {EscapeTableCell(mesh.AssetPath)} | {EscapeTableCell(mesh.SamplePath)} |");
            }

            builder.AppendLine();
            builder.AppendLine("### Top Hierarchy Branches By Active Renderer Inventory");
            builder.AppendLine();
            builder.AppendLine("| Branch | Active renderers | Triangles | Shadow casters | Active lights (RT) | Active colliders | Frame components | LOD groups |");
            builder.AppendLine("|---|---:|---:|---:|---:|---:|---:|---:|");
            int branchCount = Math.Min(18, scene.Branches.Count);
            for (int i = 0; i < branchCount; i++)
            {
                RootMetrics branch = scene.Branches[i];
                builder.AppendLine(
                    $"| {EscapeTableCell(branch.Name)} | {branch.ActiveRendererCount:N0} | {branch.InstancedTriangleCount:N0} | " +
                    $"{branch.ShadowCasterCount:N0} | {branch.ActiveLightCount:N0} ({branch.RealtimeLightCount:N0}) | " +
                    $"{branch.ActiveColliderCount:N0} | {branch.ActiveMonoBehaviourCount:N0} | {branch.LodGroupCount:N0} |");
            }

            builder.AppendLine();
            builder.AppendLine("### Active Collider Groups");
            builder.AppendLine();
            builder.AppendLine("| Collider | Source mesh/object | Instances | Triggers | Convex | Source triangles | Max world bounds | Asset | Sample path |");
            builder.AppendLine("|---|---|---:|---:|---:|---:|---:|---|---|");
            int colliderUsageCount = Math.Min(40, scene.ColliderUsages.Count);
            for (int i = 0; i < colliderUsageCount; i++)
            {
                ColliderUsageMetrics collider = scene.ColliderUsages[i];
                builder.AppendLine(
                    $"| {collider.ColliderType} | {EscapeTableCell(collider.SourceName)} | " +
                    $"{collider.InstanceCount:N0} | {collider.TriggerCount:N0} | {collider.ConvexCount:N0} | " +
                    $"{collider.TriangleCount:N0} | {collider.MaxWorldBoundsAxis:0.###}m | " +
                    $"{EscapeTableCell(collider.AssetPath)} | {EscapeTableCell(collider.SamplePath)} |");
            }

            builder.AppendLine();
            builder.AppendLine("### Active Lights");
            builder.AppendLine();
            builder.AppendLine("| Path | Type | Bake | Shadows | Range | Intensity | Culling mask |");
            builder.AppendLine("|---|---|---|---|---:|---:|---:|");
            int lightCount = Math.Min(40, scene.Lights.Count);
            for (int i = 0; i < lightCount; i++)
            {
                LightMetrics light = scene.Lights[i];
                builder.AppendLine(
                    $"| {EscapeTableCell(light.Path)} | {light.Type} | {light.BakeType} | {light.Shadows} | " +
                    $"{light.Range:0.###} | {light.Intensity:0.###} | {light.CullingMask} |");
            }

            builder.AppendLine();
            builder.AppendLine("### Active Frame Loops");
            builder.AppendLine();
            builder.AppendLine("| Type | Update | LateUpdate | FixedUpdate |");
            builder.AppendLine("|---|---:|---:|---:|");
            int loopCount = Math.Min(15, scene.FrameLoops.Count);
            for (int i = 0; i < loopCount; i++)
            {
                FrameLoopMetrics loop = scene.FrameLoops[i];
                builder.AppendLine(
                    $"| `{loop.TypeName}` | {loop.UpdateInstances:N0} | {loop.LateUpdateInstances:N0} | {loop.FixedUpdateInstances:N0} |");
            }

            if (scene.Observations.Count > 0)
            {
                builder.AppendLine();
                builder.AppendLine("### Observations");
                builder.AppendLine();
                for (int i = 0; i < scene.Observations.Count; i++)
                {
                    builder.AppendLine($"- {scene.Observations[i]}");
                }
            }

            builder.AppendLine();
        }

        private static string EscapeTableCell(string value)
        {
            return string.IsNullOrEmpty(value) ? string.Empty : value.Replace("|", "\\|");
        }

        private readonly struct SceneTarget
        {
            public SceneTarget(string label, string path, string kind)
            {
                Label = label;
                Path = path;
                Kind = kind;
            }

            public string Label { get; }
            public string Path { get; }
            public string Kind { get; }
        }

        [Serializable]
        private sealed class BaselineReport
        {
            public string GeneratedUtc;
            public string UnityVersion;
            public string ActiveBuildTarget;
            public List<SceneMetrics> Scenes = new();
            public List<string> Errors = new();
        }

        [Serializable]
        private sealed class SceneMetrics
        {
            public string Label;
            public string ScenePath;
            public string SceneKind;
            public int RootCount;
            public int GameObjectCount;
            public int ActiveGameObjectCount;
            public int RendererCount;
            public int ActiveRendererCount;
            public int StaticActiveRendererCount;
            public int MeshRendererCount;
            public int SkinnedMeshRendererCount;
            public int UniqueMaterialCount;
            public int UniqueMeshCount;
            public int ReadWriteRenderedMeshAssetCount;
            public int ReadWriteRenderOnlyMeshAssetCount;
            public int ColliderModelAssetCount;
            public long ReadWriteRenderOnlyMeshRuntimeBytes;
            public long InstancedTriangleCount;
            public long UniqueMeshTriangleCount;
            public int ShadowCasterCount;
            public int ShadowReceiverCount;
            public int SmallShadowCasterCount;
            public int MediumShadowCasterCount;
            public int LargeShadowCasterCount;
            public int LightCount;
            public int ActiveLightCount;
            public int RealtimeLightCount;
            public int MixedLightCount;
            public int BakedLightCount;
            public int ShadowedLightCount;
            public int ParticleSystemCount;
            public int ActiveParticleSystemCount;
            public int PlayOnAwakeParticleSystemCount;
            public int ParticleMaxCountBudget;
            public int ColliderCount;
            public int ActiveColliderCount;
            public int ActiveMeshColliderCount;
            public int ActivePrimitiveColliderCount;
            public int MeshAndPrimitiveColliderObjectCount;
            public int MeshColliderCount;
            public int BoxColliderCount;
            public int LodGroupCount;
            public int ActiveLodGroupCount;
            public int CameraCount;
            public int ActiveCameraCount;
            public int PostProcessingCameraCount;
            public int AudioSourceCount;
            public int ActiveAudioSourceCount;
            public int PlayOnAwakeAudioSourceCount;
            public int MonoBehaviourCount;
            public int ActiveMonoBehaviourCount;
            public int ActiveFrameLoopBehaviourCount;
            public int UpdateBehaviourCount;
            public int LateUpdateBehaviourCount;
            public int FixedUpdateBehaviourCount;
            public int MissingScriptCount;
            public List<RootMetrics> Roots = new();
            public List<RootMetrics> Branches = new();
            public List<LightMetrics> Lights = new();
            public List<MaterialUsageMetrics> MaterialUsages = new();
            public List<MeshUsageMetrics> MeshUsages = new();
            public List<ColliderUsageMetrics> ColliderUsages = new();
            public List<FrameLoopMetrics> FrameLoops = new();
            public List<string> Observations = new();
        }

        [Serializable]
        private sealed class RootMetrics
        {
            public string Name;
            public bool Active;
            public int GameObjectCount;
            public int RendererCount;
            public int ActiveRendererCount;
            public long InstancedTriangleCount;
            public int ShadowCasterCount;
            public int LightCount;
            public int ActiveLightCount;
            public int RealtimeLightCount;
            public int ParticleSystemCount;
            public int ColliderCount;
            public int ActiveColliderCount;
            public int LodGroupCount;
            public int MonoBehaviourCount;
            public int ActiveMonoBehaviourCount;
        }

        [Serializable]
        private sealed class FrameLoopMetrics
        {
            public string TypeName;
            public int UpdateInstances;
            public int LateUpdateInstances;
            public int FixedUpdateInstances;
        }

        [Serializable]
        private sealed class LightMetrics
        {
            public string Path;
            public string Type;
            public string BakeType;
            public string Shadows;
            public float Range;
            public float Intensity;
            public int CullingMask;
        }

        [Serializable]
        private sealed class MeshUsageMetrics
        {
            public string MeshName;
            public string AssetPath;
            public string SamplePath;
            public int InstanceCount;
            public int ShadowCasterInstanceCount;
            public int MeshColliderInstanceCount;
            public bool ReadWriteEnabled;
            public long TriangleCount;
            public long TotalInstancedTriangleCount;
            public long RuntimeMemoryBytes;
            public float MaxWorldBoundsAxis;
        }

        [Serializable]
        private sealed class MaterialUsageMetrics
        {
            public string MaterialName;
            public string AssetPath;
            public string ShaderName;
            public int RendererReferenceCount;
            public bool InstancingEnabled;
        }

        [Serializable]
        private sealed class ColliderUsageMetrics
        {
            public string ColliderType;
            public string SourceName;
            public string AssetPath;
            public string SamplePath;
            public int InstanceCount;
            public int TriggerCount;
            public int ConvexCount;
            public long TriangleCount;
            public float MaxWorldBoundsAxis;
        }
    }
}
