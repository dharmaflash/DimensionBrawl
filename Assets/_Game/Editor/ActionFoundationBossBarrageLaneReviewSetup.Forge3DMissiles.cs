using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace DimensionBrawl.Editor
{
    public static partial class ActionFoundationBossBarrageLaneReviewSetup
    {
        private static GameObject AttachPromotedForge3DMissilePrefab(
            Transform parent,
            string childName,
            string sourcePrefabPath,
            Vector3 localPosition,
            Vector3 localEuler,
            Vector3 localScale)
        {
            DestroyChildIfPresent(parent, childName);
            EnsureFolderForAsset(Forge3DMissileMaterialRoot + "/.keep");
            EnsureFolderForAsset(Forge3DMissileTextureRoot + "/.keep");
            EnsureFolderForAsset(Forge3DMissileShaderRoot + "/.keep");
            EnsureFolderForAsset(Forge3DMissileMeshRoot + "/.keep");

            GameObject sourcePrefab = LoadAsset<GameObject>(sourcePrefabPath);
            GameObject missileInstance = PrefabUtility.InstantiatePrefab(sourcePrefab, parent.gameObject.scene) as GameObject;
            if (missileInstance == null)
            {
                missileInstance = UnityEngine.Object.Instantiate(sourcePrefab);
            }

            if (PrefabUtility.IsPartOfPrefabInstance(missileInstance))
            {
                PrefabUtility.UnpackPrefabInstance(
                    missileInstance,
                    PrefabUnpackMode.Completely,
                    InteractionMode.AutomatedAction);
            }

            missileInstance.name = childName;
            missileInstance.transform.SetParent(parent, worldPositionStays: false);
            missileInstance.transform.localPosition = localPosition;
            missileInstance.transform.localRotation = Quaternion.Euler(localEuler);
            missileInstance.transform.localScale = localScale;

            UnpackNestedPrefabInstances(missileInstance);
            StripNonGameMonoBehaviours(missileInstance);
            RemoveForge3DMissileRuntimePhysics(missileInstance);
            RemoveForge3DMissileRuntimeAudio(missileInstance);
            ConfigurePromotedForge3DMissileParticles(missileInstance);
            RemapPromotedForge3DMissileRendererDependencies(missileInstance);
            EditorUtility.SetDirty(missileInstance);
            return missileInstance;
        }

        private static void ConfigurePromotedForge3DMissileParticles(GameObject root)
        {
            ParticleSystem[] particleSystems = root.GetComponentsInChildren<ParticleSystem>(includeInactive: true);
            for (int i = 0; i < particleSystems.Length; i++)
            {
                ParticleSystem particleSystem = particleSystems[i];
                ParticleSystem.MainModule main = particleSystem.main;
                main.loop = true;
                main.playOnAwake = true;
                main.scalingMode = ParticleSystemScalingMode.Hierarchy;

                ParticleSystem.EmissionModule emission = particleSystem.emission;
                emission.enabled = true;
                particleSystem.Clear(withChildren: true);
                particleSystem.Play(withChildren: true);
                EditorUtility.SetDirty(particleSystem);
            }
        }

        private static void RemoveForge3DMissileRuntimePhysics(GameObject root)
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

        private static void RemoveForge3DMissileRuntimeAudio(GameObject root)
        {
            AudioSource[] audioSources = root.GetComponentsInChildren<AudioSource>(includeInactive: true);
            for (int i = audioSources.Length - 1; i >= 0; i--)
            {
                UnityEngine.Object.DestroyImmediate(audioSources[i]);
            }
        }

        private static void RemapPromotedForge3DMissileRendererDependencies(GameObject root)
        {
            MeshFilter[] meshFilters = root.GetComponentsInChildren<MeshFilter>(includeInactive: true);
            for (int i = 0; i < meshFilters.Length; i++)
            {
                MeshFilter meshFilter = meshFilters[i];
                if (meshFilter.sharedMesh != null)
                {
                    meshFilter.sharedMesh = EnsurePromotedForge3DMissileMesh(meshFilter.sharedMesh);
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
                    skinnedRenderer.sharedMesh = EnsurePromotedForge3DMissileMesh(skinnedRenderer.sharedMesh);
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
                        materials[materialIndex] = EnsurePromotedForge3DMissileMaterial(materials[materialIndex]);
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
                    particleRenderer.mesh = EnsurePromotedForge3DMissileMesh(mesh);
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
                            meshes[meshIndex] = EnsurePromotedForge3DMissileMesh(meshes[meshIndex]);
                        }
                    }

                    particleRenderer.SetMeshes(meshes, copiedCount);
                }

                EditorUtility.SetDirty(particleRenderer);
            }
        }

        private static Material EnsurePromotedForge3DMissileMaterial(Material sourceMaterial)
        {
            string sourcePath = AssetDatabase.GetAssetPath(sourceMaterial).Replace('\\', '/');
            if (sourcePath.StartsWith("Assets/_Game/", StringComparison.Ordinal)
                || string.IsNullOrWhiteSpace(sourcePath)
                || !sourcePath.StartsWith("Assets/", StringComparison.Ordinal))
            {
                return sourceMaterial;
            }

            string targetPath = Forge3DMissileMaterialRoot + "/DB_Forge3DMissile_"
                + SanitizeAssetFileName(sourceMaterial.name)
                + ".mat";
            EnsureFolderForAsset(targetPath);

            Shader promotedShader = EnsurePromotedForge3DMissileShader(sourceMaterial.shader);
            Material material = AssetDatabase.LoadAssetAtPath<Material>(targetPath);
            if (material == null)
            {
                material = new Material(promotedShader);
                AssetDatabase.CreateAsset(material, targetPath);
            }

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
                        EnsurePromotedForge3DMissileTexture(texture));
                }
            }

            RemapSerializedForge3DMissileTextures(material);
            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(targetPath, ImportAssetOptions.ForceUpdate);
            return material;
        }

        private static Shader EnsurePromotedForge3DMissileShader(Shader sourceShader)
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

            string targetPath = Forge3DMissileShaderRoot + "/DB_Forge3DMissile_"
                + SanitizeAssetFileName(Path.GetFileName(sourcePath));
            EnsureFolderForAsset(targetPath);
            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(targetPath) == null)
            {
                if (!AssetDatabase.CopyAsset(sourcePath, targetPath))
                {
                    throw new InvalidOperationException(
                        $"Failed to promote Forge3D missile shader from {sourcePath} to {targetPath}.");
                }
            }

            RemapPromotedForge3DMissileShaderDependencies(sourcePath, targetPath);
            AssetDatabase.ImportAsset(targetPath, ImportAssetOptions.ForceUpdate);
            Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(targetPath);
            if (shader == null)
            {
                throw new InvalidOperationException($"Failed to load promoted Forge3D missile shader at {targetPath}.");
            }

            return shader;
        }

        private static void RemapPromotedForge3DMissileShaderDependencies(
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
                    || !IsPromotableForge3DMissileShaderDependency(dependency))
                {
                    continue;
                }

                string promotedDependency = EnsurePromotedForge3DMissileShaderFile(dependency);
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

        private static bool IsPromotableForge3DMissileShaderDependency(string assetPath)
        {
            string extension = Path.GetExtension(assetPath).ToLowerInvariant();
            return extension == ".hlsl"
                || extension == ".cginc"
                || extension == ".shader"
                || extension == ".shadergraph"
                || extension == ".shadersubgraph"
                || extension == ".compute";
        }

        private static string EnsurePromotedForge3DMissileShaderFile(string sourcePath)
        {
            string targetPath = Forge3DMissileShaderRoot + "/DB_Forge3DMissile_"
                + SanitizeAssetFileName(Path.GetFileName(sourcePath));
            EnsureFolderForAsset(targetPath);
            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(targetPath) == null)
            {
                if (!AssetDatabase.CopyAsset(sourcePath, targetPath))
                {
                    throw new InvalidOperationException(
                        $"Failed to promote Forge3D missile shader dependency from {sourcePath} to {targetPath}.");
                }
            }

            AssetDatabase.ImportAsset(targetPath, ImportAssetOptions.ForceUpdate);
            return targetPath;
        }

        private static Texture EnsurePromotedForge3DMissileTexture(Texture sourceTexture)
        {
            string sourcePath = AssetDatabase.GetAssetPath(sourceTexture).Replace('\\', '/');
            if (sourcePath.StartsWith("Assets/_Game/", StringComparison.Ordinal)
                || string.IsNullOrWhiteSpace(sourcePath)
                || !sourcePath.StartsWith("Assets/", StringComparison.Ordinal))
            {
                return sourceTexture;
            }

            string targetPath = Forge3DMissileTextureRoot + "/DB_Forge3DMissile_"
                + SanitizeAssetFileName(Path.GetFileName(sourcePath));
            EnsureFolderForAsset(targetPath);
            if (AssetDatabase.LoadAssetAtPath<Texture>(targetPath) == null)
            {
                if (!AssetDatabase.CopyAsset(sourcePath, targetPath))
                {
                    throw new InvalidOperationException(
                        $"Failed to promote Forge3D missile texture from {sourcePath} to {targetPath}.");
                }
            }

            AssetDatabase.ImportAsset(targetPath, ImportAssetOptions.ForceUpdate);
            Texture texture = AssetDatabase.LoadAssetAtPath<Texture>(targetPath);
            if (texture == null)
            {
                throw new InvalidOperationException($"Failed to load promoted Forge3D missile texture at {targetPath}.");
            }

            return texture;
        }

        private static Mesh EnsurePromotedForge3DMissileMesh(Mesh sourceMesh)
        {
            string sourcePath = AssetDatabase.GetAssetPath(sourceMesh).Replace('\\', '/');
            if (sourcePath.StartsWith("Assets/_Game/", StringComparison.Ordinal)
                || string.IsNullOrWhiteSpace(sourcePath)
                || !sourcePath.StartsWith("Assets/", StringComparison.Ordinal))
            {
                return sourceMesh;
            }

            string targetPath = Forge3DMissileMeshRoot + "/DB_Forge3DMissile_"
                + SanitizeAssetFileName(sourceMesh.name)
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
                throw new InvalidOperationException($"Failed to load promoted Forge3D missile mesh at {targetPath}.");
            }

            return promotedMesh;
        }

        private static void RemapSerializedForge3DMissileTextures(Material material)
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
                    textureRef.objectReferenceValue = EnsurePromotedForge3DMissileTexture(texture);
                }
            }

            serializedMaterial.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(material);
        }

        private static void ValidatePromotedForge3DMissileVfx(
            Transform root,
            string label,
            int minimumParticleSystems)
        {
            if (root == null)
            {
                throw new InvalidOperationException($"{label} should be attached as promoted Forge3D missile VFX.");
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
                throw new InvalidOperationException($"{label} should preserve its authored Flame and SmokeTrail particle systems.");
            }

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(includeInactive: true);
            if (renderers.Length == 0)
            {
                throw new InvalidOperationException($"{label} should expose promoted Forge3D renderers.");
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
                    ValidatePromotedForge3DMissileProjectAssetIfPresent(
                        particleRenderer.mesh,
                        $"{label}.{renderer.name} mesh");
                }
            }

            MeshFilter[] meshFilters = root.GetComponentsInChildren<MeshFilter>(includeInactive: true);
            for (int i = 0; i < meshFilters.Length; i++)
            {
                if (meshFilters[i].sharedMesh != null)
                {
                    ValidatePromotedForge3DMissileProjectAssetIfPresent(
                        meshFilters[i].sharedMesh,
                        $"{label}.{meshFilters[i].name} mesh");
                }
            }
        }

        private static void ValidatePromotedForge3DMissileProjectAssetIfPresent(UnityEngine.Object asset, string label)
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
