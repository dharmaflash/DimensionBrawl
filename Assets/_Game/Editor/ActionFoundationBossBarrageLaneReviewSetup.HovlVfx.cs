using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace DimensionBrawl.Editor
{
    public static partial class ActionFoundationBossBarrageLaneReviewSetup
    {
        private static GameObject AttachPromotedHovlSciFiVfxPrefab(
            Transform parent,
            string childName,
            string sourcePrefabPath,
            Vector3 localPosition,
            Vector3 localEuler,
            Vector3 localScale,
            bool? loopParticles,
            bool playOnAwake)
        {
            DestroyChildIfPresent(parent, childName);
            EnsureFolderForAsset(HovlSciFiEffectsMaterialRoot + "/.keep");
            EnsureFolderForAsset(HovlSciFiEffectsTextureRoot + "/.keep");
            EnsureFolderForAsset(HovlSciFiEffectsShaderRoot + "/.keep");
            EnsureFolderForAsset(HovlSciFiEffectsMeshRoot + "/.keep");

            GameObject sourcePrefab = LoadAsset<GameObject>(sourcePrefabPath);
            GameObject vfxInstance = PrefabUtility.InstantiatePrefab(sourcePrefab, parent.gameObject.scene) as GameObject;
            if (vfxInstance == null)
            {
                vfxInstance = UnityEngine.Object.Instantiate(sourcePrefab);
            }

            if (PrefabUtility.IsPartOfPrefabInstance(vfxInstance))
            {
                PrefabUtility.UnpackPrefabInstance(
                    vfxInstance,
                    PrefabUnpackMode.Completely,
                    InteractionMode.AutomatedAction);
            }

            vfxInstance.name = childName;
            vfxInstance.transform.SetParent(parent, worldPositionStays: false);
            vfxInstance.transform.localPosition = localPosition;
            vfxInstance.transform.localRotation = Quaternion.Euler(localEuler);
            vfxInstance.transform.localScale = localScale;

            UnpackNestedPrefabInstances(vfxInstance);
            StripNonGameMonoBehaviours(vfxInstance);
            RemoveHovlRuntimePhysics(vfxInstance);
            RemoveHovlRuntimeAudio(vfxInstance);
            ConfigurePromotedHovlSciFiParticles(vfxInstance, loopParticles, playOnAwake);
            RemapPromotedHovlSciFiRendererDependencies(vfxInstance);
            EditorUtility.SetDirty(vfxInstance);
            return vfxInstance;
        }

        private static void ConfigurePromotedHovlSciFiParticles(
            GameObject root,
            bool? loopParticles,
            bool playOnAwake)
        {
            ParticleSystem[] particleSystems = root.GetComponentsInChildren<ParticleSystem>(includeInactive: true);
            for (int i = 0; i < particleSystems.Length; i++)
            {
                ParticleSystem particleSystem = particleSystems[i];
                ParticleSystem.MainModule main = particleSystem.main;
                if (loopParticles.HasValue)
                {
                    main.loop = loopParticles.Value;
                }

                main.simulationSpace = ParticleSystemSimulationSpace.Local;
                main.scalingMode = ParticleSystemScalingMode.Hierarchy;
                main.playOnAwake = playOnAwake;
                ParticleSystem.EmissionModule emission = particleSystem.emission;
                emission.enabled = true;
                particleSystem.Clear(withChildren: true);
                if (playOnAwake)
                {
                    particleSystem.Play(withChildren: true);
                }

                EditorUtility.SetDirty(particleSystem);
            }
        }

        private static void RemoveHovlRuntimePhysics(GameObject root)
        {
            Rigidbody[] rigidbodies = root.GetComponentsInChildren<Rigidbody>(includeInactive: true);
            for (int i = rigidbodies.Length - 1; i >= 0; i--)
            {
                UnityEngine.Object.DestroyImmediate(rigidbodies[i]);
            }

            Collider[] colliders = root.GetComponentsInChildren<Collider>(includeInactive: true);
            for (int i = colliders.Length - 1; i >= 0; i--)
            {
                UnityEngine.Object.DestroyImmediate(colliders[i]);
            }
        }

        private static void RemoveHovlRuntimeAudio(GameObject root)
        {
            AudioSource[] audioSources = root.GetComponentsInChildren<AudioSource>(includeInactive: true);
            for (int i = audioSources.Length - 1; i >= 0; i--)
            {
                UnityEngine.Object.DestroyImmediate(audioSources[i]);
            }
        }

        private static void RemapPromotedHovlSciFiRendererDependencies(GameObject root)
        {
            MeshFilter[] meshFilters = root.GetComponentsInChildren<MeshFilter>(includeInactive: true);
            for (int i = 0; i < meshFilters.Length; i++)
            {
                MeshFilter meshFilter = meshFilters[i];
                if (meshFilter.sharedMesh != null)
                {
                    meshFilter.sharedMesh = EnsurePromotedHovlSciFiMesh(meshFilter.sharedMesh);
                    EditorUtility.SetDirty(meshFilter);
                }
            }

            SkinnedMeshRenderer[] skinnedRenderers =
                root.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true);
            for (int i = 0; i < skinnedRenderers.Length; i++)
            {
                SkinnedMeshRenderer skinnedRenderer = skinnedRenderers[i];
                if (skinnedRenderer.sharedMesh != null)
                {
                    skinnedRenderer.sharedMesh = EnsurePromotedHovlSciFiMesh(skinnedRenderer.sharedMesh);
                    EditorUtility.SetDirty(skinnedRenderer);
                }
            }

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(includeInactive: true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                Material[] materials = renderer.sharedMaterials;
                for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                {
                    if (materials[materialIndex] != null)
                    {
                        materials[materialIndex] = EnsurePromotedHovlSciFiMaterial(materials[materialIndex]);
                    }
                }

                renderer.sharedMaterials = materials;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                renderer.allowOcclusionWhenDynamic = false;
                EditorUtility.SetDirty(renderer);
            }

            ParticleSystemRenderer[] particleRenderers =
                root.GetComponentsInChildren<ParticleSystemRenderer>(includeInactive: true);
            for (int i = 0; i < particleRenderers.Length; i++)
            {
                ParticleSystemRenderer particleRenderer = particleRenderers[i];
                Mesh mesh = particleRenderer.mesh;
                if (mesh != null)
                {
                    particleRenderer.mesh = EnsurePromotedHovlSciFiMesh(mesh);
                }

                int meshCount = particleRenderer.meshCount;
                if (meshCount > 0)
                {
                    Mesh[] meshes = new Mesh[meshCount];
                    int copiedCount = particleRenderer.GetMeshes(meshes);
                    for (int meshIndex = 0; meshIndex < copiedCount; meshIndex++)
                    {
                        if (meshes[meshIndex] != null)
                        {
                            meshes[meshIndex] = EnsurePromotedHovlSciFiMesh(meshes[meshIndex]);
                        }
                    }

                    particleRenderer.SetMeshes(meshes, copiedCount);
                }

                EditorUtility.SetDirty(particleRenderer);
            }

            AssignMissingHovlParticleRendererMaterials(root);
        }

        private static void AssignMissingHovlParticleRendererMaterials(GameObject root)
        {
            Material fallbackMaterial = ResolveFirstAssignedHovlParticleMaterial(root)
                ?? LoadOrCreateTransparentMaterial(
                    HovlSciFiEffectsMaterialRoot + "/DB_HovlSciFi_DefaultParticleFallback.mat",
                    new Color(0.38f, 0.92f, 1f, 0.72f));

            ParticleSystemRenderer[] particleRenderers =
                root.GetComponentsInChildren<ParticleSystemRenderer>(includeInactive: true);
            for (int i = 0; i < particleRenderers.Length; i++)
            {
                ParticleSystemRenderer particleRenderer = particleRenderers[i];
                Material[] materials = particleRenderer.sharedMaterials;
                if (materials == null || materials.Length == 0)
                {
                    particleRenderer.sharedMaterial = fallbackMaterial;
                    EditorUtility.SetDirty(particleRenderer);
                    continue;
                }

                bool changed = false;
                for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                {
                    if (materials[materialIndex] != null)
                    {
                        continue;
                    }

                    materials[materialIndex] = fallbackMaterial;
                    changed = true;
                }

                if (changed)
                {
                    particleRenderer.sharedMaterials = materials;
                    EditorUtility.SetDirty(particleRenderer);
                }
            }
        }

        private static Material ResolveFirstAssignedHovlParticleMaterial(GameObject root)
        {
            ParticleSystemRenderer[] particleRenderers =
                root.GetComponentsInChildren<ParticleSystemRenderer>(includeInactive: true);
            for (int i = 0; i < particleRenderers.Length; i++)
            {
                Material[] materials = particleRenderers[i].sharedMaterials;
                if (materials == null)
                {
                    continue;
                }

                for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                {
                    if (materials[materialIndex] != null)
                    {
                        return materials[materialIndex];
                    }
                }
            }

            return null;
        }

        private static Material EnsurePromotedHovlSciFiMaterial(Material sourceMaterial)
        {
            string sourcePath = AssetDatabase.GetAssetPath(sourceMaterial).Replace('\\', '/');
            if (sourcePath.StartsWith("Assets/_Game/", StringComparison.Ordinal))
            {
                return sourceMaterial;
            }

            string targetPath = HovlSciFiEffectsMaterialRoot + "/DB_HovlSciFi_"
                + SanitizeAssetFileName(sourceMaterial.name)
                + ".mat";
            EnsureFolderForAsset(targetPath);

            Material material = AssetDatabase.LoadAssetAtPath<Material>(targetPath);
            if (material == null)
            {
                material = new Material(EnsurePromotedHovlSciFiShader(sourceMaterial.shader));
                AssetDatabase.CreateAsset(material, targetPath);
            }

            Shader promotedShader = EnsurePromotedHovlSciFiShader(sourceMaterial.shader);
            material.shader = promotedShader;
            material.CopyPropertiesFromMaterial(sourceMaterial);
            material.shader = promotedShader;
            material.renderQueue = sourceMaterial.renderQueue;

            string[] textureProperties = sourceMaterial.GetTexturePropertyNames();
            for (int i = 0; i < textureProperties.Length; i++)
            {
                Texture texture = sourceMaterial.GetTexture(textureProperties[i]);
                if (texture != null)
                {
                    SetMaterialTextureIfPresent(
                        material,
                        textureProperties[i],
                        EnsurePromotedHovlSciFiTexture(texture));
                }
            }

            RemapSerializedHovlSciFiTextures(material);
            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(targetPath, ImportAssetOptions.ForceUpdate);
            return material;
        }

        private static Shader EnsurePromotedHovlSciFiShader(Shader sourceShader)
        {
            if (sourceShader == null)
            {
                return ResolveUnlitShader();
            }

            string sourcePath = AssetDatabase.GetAssetPath(sourceShader).Replace('\\', '/');
            if (sourcePath.StartsWith("Assets/_Game/", StringComparison.Ordinal)
                || string.IsNullOrWhiteSpace(sourcePath)
                || !sourcePath.StartsWith("Assets/", StringComparison.Ordinal))
            {
                return sourceShader;
            }

            string targetPath = HovlSciFiEffectsShaderRoot + "/DB_HovlSciFi_"
                + SanitizeAssetFileName(Path.GetFileName(sourcePath));
            EnsureFolderForAsset(targetPath);
            if (AssetDatabase.LoadAssetAtPath<Shader>(targetPath) == null)
            {
                if (!AssetDatabase.CopyAsset(sourcePath, targetPath))
                {
                    throw new InvalidOperationException(
                        $"Failed to promote Hovl Sci-fi shader from {sourcePath} to {targetPath}.");
                }
            }

            RemapPromotedHovlSciFiShaderDependencies(sourcePath, targetPath);
            AssetDatabase.ImportAsset(targetPath, ImportAssetOptions.ForceUpdate);
            Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(targetPath);
            if (shader == null)
            {
                throw new InvalidOperationException($"Failed to load promoted Hovl Sci-fi shader at {targetPath}.");
            }

            return shader;
        }

        private static void RemapPromotedHovlSciFiShaderDependencies(
            string sourceShaderPath,
            string targetShaderPath)
        {
            string targetAbsolutePath = ToAbsoluteProjectAssetPath(targetShaderPath);
            if (!File.Exists(targetAbsolutePath))
            {
                return;
            }

            string contents = File.ReadAllText(targetAbsolutePath);
            string[] dependencies = AssetDatabase.GetDependencies(sourceShaderPath, recursive: true);
            bool changed = false;
            for (int i = 0; i < dependencies.Length; i++)
            {
                string dependency = dependencies[i].Replace('\\', '/');
                if (dependency == sourceShaderPath
                    || !dependency.StartsWith("Assets/_Imported/", StringComparison.Ordinal)
                    || !IsPromotableHovlShaderDependency(dependency))
                {
                    continue;
                }

                string promotedDependency = EnsurePromotedHovlSciFiShaderFile(dependency);
                string sourceGuid = AssetDatabase.AssetPathToGUID(dependency);
                string promotedGuid = AssetDatabase.AssetPathToGUID(promotedDependency);
                if (string.IsNullOrWhiteSpace(sourceGuid)
                    || string.IsNullOrWhiteSpace(promotedGuid)
                    || sourceGuid == promotedGuid)
                {
                    continue;
                }

                string rewritten = contents.Replace(sourceGuid, promotedGuid);
                if (!string.Equals(rewritten, contents, StringComparison.Ordinal))
                {
                    contents = rewritten;
                    changed = true;
                }
            }

            if (changed)
            {
                File.WriteAllText(targetAbsolutePath, contents);
            }
        }

        private static bool IsPromotableHovlShaderDependency(string assetPath)
        {
            string extension = Path.GetExtension(assetPath).ToLowerInvariant();
            return extension == ".hlsl"
                || extension == ".cginc"
                || extension == ".shader"
                || extension == ".shadergraph"
                || extension == ".compute";
        }

        private static string EnsurePromotedHovlSciFiShaderFile(string sourcePath)
        {
            string targetPath = HovlSciFiEffectsShaderRoot + "/DB_HovlSciFi_"
                + SanitizeAssetFileName(Path.GetFileName(sourcePath));
            EnsureFolderForAsset(targetPath);
            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(targetPath) == null)
            {
                if (!AssetDatabase.CopyAsset(sourcePath, targetPath))
                {
                    throw new InvalidOperationException(
                        $"Failed to promote Hovl Sci-fi shader dependency from {sourcePath} to {targetPath}.");
                }
            }

            AssetDatabase.ImportAsset(targetPath, ImportAssetOptions.ForceUpdate);
            return targetPath;
        }

        private static string ToAbsoluteProjectAssetPath(string assetPath)
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", assetPath));
        }

        private static Texture EnsurePromotedHovlSciFiTexture(Texture sourceTexture)
        {
            string sourcePath = AssetDatabase.GetAssetPath(sourceTexture).Replace('\\', '/');
            if (sourcePath.StartsWith("Assets/_Game/", StringComparison.Ordinal)
                || string.IsNullOrWhiteSpace(sourcePath)
                || !sourcePath.StartsWith("Assets/", StringComparison.Ordinal))
            {
                return sourceTexture;
            }

            string targetPath = HovlSciFiEffectsTextureRoot + "/DB_HovlSciFi_"
                + SanitizeAssetFileName(Path.GetFileName(sourcePath));
            EnsureFolderForAsset(targetPath);
            if (AssetDatabase.LoadAssetAtPath<Texture>(targetPath) == null)
            {
                if (!AssetDatabase.CopyAsset(sourcePath, targetPath))
                {
                    throw new InvalidOperationException(
                        $"Failed to promote Hovl Sci-fi texture from {sourcePath} to {targetPath}.");
                }
            }

            AssetDatabase.ImportAsset(targetPath, ImportAssetOptions.ForceUpdate);
            Texture texture = AssetDatabase.LoadAssetAtPath<Texture>(targetPath);
            if (texture == null)
            {
                throw new InvalidOperationException($"Failed to load promoted Hovl Sci-fi texture at {targetPath}.");
            }

            return texture;
        }

        private static Mesh EnsurePromotedHovlSciFiMesh(Mesh sourceMesh)
        {
            string sourcePath = AssetDatabase.GetAssetPath(sourceMesh).Replace('\\', '/');
            if (sourcePath.StartsWith("Assets/_Game/", StringComparison.Ordinal))
            {
                return sourceMesh;
            }

            string sourceName = string.IsNullOrWhiteSpace(sourceMesh.name) ? "BuiltinParticleMesh" : sourceMesh.name;
            string targetPath = HovlSciFiEffectsMeshRoot + "/DB_HovlSciFi_"
                + SanitizeAssetFileName(sourceName)
                + ".asset";
            EnsureFolderForAsset(targetPath);

            Mesh promotedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(targetPath);
            if (promotedMesh != null)
            {
                return promotedMesh;
            }

            promotedMesh = UnityEngine.Object.Instantiate(sourceMesh);
            promotedMesh.name = sourceMesh.name;
            AssetDatabase.CreateAsset(promotedMesh, targetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(targetPath, ImportAssetOptions.ForceUpdate);

            promotedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(targetPath);
            if (promotedMesh == null)
            {
                throw new InvalidOperationException($"Failed to load promoted Hovl Sci-fi mesh at {targetPath}.");
            }

            return promotedMesh;
        }

        private static void RemapSerializedHovlSciFiTextures(Material material)
        {
            SerializedObject serializedMaterial = new SerializedObject(material);
            SerializedProperty texEnvs = serializedMaterial.FindProperty("m_SavedProperties.m_TexEnvs");
            if (texEnvs == null || !texEnvs.isArray)
            {
                return;
            }

            for (int i = 0; i < texEnvs.arraySize; i++)
            {
                SerializedProperty entry = texEnvs.GetArrayElementAtIndex(i);
                SerializedProperty textureRef = entry.FindPropertyRelative("second.m_Texture");
                if (textureRef == null || textureRef.objectReferenceValue is not Texture texture)
                {
                    continue;
                }

                string texturePath = AssetDatabase.GetAssetPath(texture).Replace('\\', '/');
                if (texturePath.StartsWith("Assets/_Imported/", StringComparison.Ordinal))
                {
                    textureRef.objectReferenceValue = EnsurePromotedHovlSciFiTexture(texture);
                }
            }

            serializedMaterial.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(material);
        }

        private static void SetMaterialTextureIfPresent(Material material, string propertyName, Texture texture)
        {
            if (material != null && texture != null && material.HasProperty(propertyName))
            {
                material.SetTexture(propertyName, texture);
            }
        }

        private static void ValidatePromotedHovlSciFiParticleVfx(
            Transform root,
            string label,
            int minimumParticleSystems)
        {
            if (root == null)
            {
                throw new InvalidOperationException($"{label} should be attached as promoted Hovl Sci-fi VFX.");
            }

            if (root.GetComponentInChildren<Collider>(includeInactive: true) != null)
            {
                throw new InvalidOperationException($"{label} must remain visual-only and should not own a Collider.");
            }

            if (root.GetComponentInChildren<Rigidbody>(includeInactive: true) != null)
            {
                throw new InvalidOperationException($"{label} must remain visual-only and should not own a Rigidbody.");
            }

            if (root.GetComponentInChildren<AudioSource>(includeInactive: true) != null)
            {
                throw new InvalidOperationException($"{label} must keep projectile audio controlled by the game emitter, not the imported prefab.");
            }

            ParticleSystem[] particleSystems = root.GetComponentsInChildren<ParticleSystem>(includeInactive: true);
            if (particleSystems.Length < minimumParticleSystems)
            {
                throw new InvalidOperationException($"{label} should preserve its authored particle system stack.");
            }

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(includeInactive: true);
            if (renderers.Length == 0)
            {
                throw new InvalidOperationException($"{label} should expose promoted Hovl renderers.");
            }

            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                Material[] materials = renderer.sharedMaterials;
                for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                {
                    Material material = materials[materialIndex];
                    if (material == null)
                    {
                        continue;
                    }

                    ValidateGameOwnedAsset(material, $"{label}.{renderer.name} material");
                    ValidateRenderableMaterialShader(material, $"{label}.{renderer.name} material shader");
                    ValidateNoImportedDependencies(material, $"{label}.{renderer.name} material");
                }

                if (renderer is ParticleSystemRenderer particleRenderer && particleRenderer.mesh != null)
                {
                    ValidatePromotedHovlProjectAssetIfPresent(
                        particleRenderer.mesh,
                        $"{label}.{renderer.name} mesh");
                }
            }

            MeshFilter[] meshFilters = root.GetComponentsInChildren<MeshFilter>(includeInactive: true);
            for (int i = 0; i < meshFilters.Length; i++)
            {
                if (meshFilters[i].sharedMesh != null)
                {
                    ValidatePromotedHovlProjectAssetIfPresent(
                        meshFilters[i].sharedMesh,
                        $"{label}.{meshFilters[i].name} mesh");
                }
            }
        }

        private static void ValidatePromotedHovlProjectAssetIfPresent(UnityEngine.Object asset, string label)
        {
            string path = AssetDatabase.GetAssetPath(asset).Replace('\\', '/');
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            ValidateGameOwnedAsset(asset, label);
            ValidateNoImportedDependencies(asset, label);
        }
    }
}
