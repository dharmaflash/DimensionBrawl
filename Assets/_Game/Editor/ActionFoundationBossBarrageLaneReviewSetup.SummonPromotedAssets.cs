using System;
using System.IO;
using DimensionBrawl.Combat;
using DimensionBrawl.Presentation;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Rendering;

namespace DimensionBrawl.Editor
{
    public static partial class ActionFoundationBossBarrageLaneReviewSetup
    {
        private const string ImportedForge3DPlasmaBeamBluePrefabPath =
            "Assets/_Imported/AssetStore/FORGE3D/Sci-Fi Effects/Effects/Plasma Beam/plasma_beam_blue.prefab";
        private const string ImportedForge3DFlameRedPrefabPath =
            "Assets/_Imported/AssetStore/FORGE3D/Sci-Fi Effects/Effects/Flames/flames_flame_red.prefab";
        private const string ImportedSpecialSkillChargeImpactPrefabPath =
            "Assets/_Imported/SpecialSkillsEffectsPack/AllEffects/EffectsSet_1(NotScriptBased)/Effects/Effect_39_WindBlast/Effect_39_WindBlast.prefab";
        private const string ImportedSpecialSkillRushTrailPrefabPath =
            "Assets/_Imported/SpecialSkillsEffectsPack/AllEffects/EffectsSet_1(NotScriptBased)/Effects/Effect_13_DangerClose/Effect_13_Base/Effect_13_Trails.prefab";
        private const string ImportedVolcanoDragonModelPath =
            "Assets/_Imported/AssetStore/HEROIC FANTASY CREATURES FULL PACK VOL3/Elemental Dragons Pack/Volcano Dragon/FBX Files/SK_VolcanoDragon.FBX";
        private const string ImportedVolcanoDragonFlyStationaryClipPath =
            "Assets/_Imported/AssetStore/HEROIC FANTASY CREATURES FULL PACK VOL3/Elemental Dragons Pack/Volcano Dragon/FBX Files/VolcanoDragon@FlyStationary.FBX";
        private const string ImportedVolcanoDragonSpitFireClipPath =
            "Assets/_Imported/AssetStore/HEROIC FANTASY CREATURES FULL PACK VOL3/Elemental Dragons Pack/Volcano Dragon/FBX Files/VolcanoDragon@FlyStationarySpitFireBall.FBX";
        private const string ImportedVolcanoDragonFallingClipPath =
            "Assets/_Imported/AssetStore/HEROIC FANTASY CREATURES FULL PACK VOL3/Elemental Dragons Pack/Volcano Dragon/FBX Files/VolcanoDragon@FlyStationaryGetHitToFalling.FBX";

        private const string SummonPromotedVfxRoot =
            "Assets/_Game/Art/VFX/ActionFoundation/Summons";
        private const string SummonPromotedVfxPrefabRoot =
            SummonPromotedVfxRoot + "/Prefabs";
        private const string SummonPromotedVfxMaterialRoot =
            SummonPromotedVfxRoot + "/Materials";
        private const string SummonPromotedVfxTextureRoot =
            SummonPromotedVfxRoot + "/Textures";
        private const string SummonPromotedVfxMeshRoot =
            SummonPromotedVfxRoot + "/Meshes";
        private const string SummonRoleVisualTextureRoot =
            "Assets/_Game/Art/Characters/Enemies/SciFiSoldiers/RoleVisualTextures";
        private const string SummonSlot2PromotedLaserBeamPrefabPath =
            SummonPromotedVfxPrefabRoot + "/PF_SummonLaserBeam_FORGE3D.prefab";
        private const string SummonSlot3PromotedFireBreathPrefabPath =
            SummonPromotedVfxPrefabRoot + "/PF_SummonDragonFireBreath_FORGE3D.prefab";
        private const string SummonSlot1PromotedChargeImpactPrefabPath =
            SummonPromotedVfxPrefabRoot + "/PF_SummonChargeImpact_SPECIAL.prefab";
        private const string SummonSlot1PromotedRushTrailPrefabPath =
            SummonPromotedVfxPrefabRoot + "/PF_SummonChargeRushTrail_SPECIAL.prefab";

        private const string SummonDragonPromotedRoot =
            "Assets/_Game/Art/Characters/Enemies/Dragons/VolcanoDragon";
        private const string SummonDragonModelRoot =
            SummonDragonPromotedRoot + "/Models";
        private const string SummonDragonMaterialRoot =
            SummonDragonPromotedRoot + "/Materials";
        private const string SummonDragonTextureRoot =
            SummonDragonPromotedRoot + "/Textures";
        private const string SummonDragonAnimationRoot =
            SummonDragonPromotedRoot + "/Animations";
        private const string SummonSlot3DragonVisualPrefabPath =
            SummonDragonPromotedRoot + "/PF_SummonVisual_VolcanoDragon.prefab";
        private const string SummonSlot3DragonVisualPrefabName =
            "PF_SummonVisual_VolcanoDragon";
        private const string SummonSlot3DragonModelPath =
            SummonDragonModelRoot + "/SK_VolcanoDragon.FBX";
        private const string SummonSlot3DragonFlyStationaryClipPath =
            SummonDragonAnimationRoot + "/VolcanoDragon@FlyStationary.FBX";
        private const string SummonSlot3DragonSpitFireClipPath =
            SummonDragonAnimationRoot + "/VolcanoDragon@FlyStationarySpitFireBall.FBX";
        private const string SummonSlot3DragonFallingClipPath =
            SummonDragonAnimationRoot + "/VolcanoDragon@FlyStationaryGetHitToFalling.FBX";
        private const string SummonSlot3DragonControllerPath =
            SummonDragonAnimationRoot + "/DB_VolcanoDragon_Summon.controller";

        private enum SummonPromotedTextureUsage
        {
            Color,
            Linear,
            Normal
        }

        private static void EnsureSummonPromotedPresentationAssets()
        {
            EnsureSummonSlot1PromotedChargeImpactPrefab();
            EnsureSummonSlot1PromotedRushTrailPrefab();
            EnsureSummonSlot2PromotedLaserBeamPrefab();
            EnsureSummonSlot3PromotedFireBreathPrefab();
            EnsureSummonSlot3PromotedDragonVisualPrefab();
        }

        private static GameObject EnsureSummonSlot1PromotedChargeImpactPrefab()
        {
            return EnsureSummonPromotedParticlePrefab(
                ImportedSpecialSkillChargeImpactPrefabPath,
                SummonSlot1PromotedChargeImpactPrefabPath,
                "PF_SummonChargeImpact_SPECIAL",
                loopParticles: false,
                playOnAwake: false,
                minimumParticleSystems: 3);
        }

        private static GameObject EnsureSummonSlot1PromotedRushTrailPrefab()
        {
            return EnsureSummonPromotedParticlePrefab(
                ImportedSpecialSkillRushTrailPrefabPath,
                SummonSlot1PromotedRushTrailPrefabPath,
                "PF_SummonChargeRushTrail_SPECIAL",
                loopParticles: true,
                playOnAwake: false,
                minimumParticleSystems: 2);
        }

        private static GameObject EnsureSummonSlot2PromotedLaserBeamPrefab()
        {
            return EnsureSummonPromotedParticlePrefab(
                ImportedForge3DPlasmaBeamBluePrefabPath,
                SummonSlot2PromotedLaserBeamPrefabPath,
                "PF_SummonLaserBeam_FORGE3D",
                loopParticles: true,
                playOnAwake: false,
                minimumParticleSystems: 4);
        }

        private static GameObject EnsureSummonSlot3PromotedFireBreathPrefab()
        {
            return EnsureSummonPromotedParticlePrefab(
                ImportedForge3DFlameRedPrefabPath,
                SummonSlot3PromotedFireBreathPrefabPath,
                "PF_SummonDragonFireBreath_FORGE3D",
                loopParticles: true,
                playOnAwake: false,
                minimumParticleSystems: 2);
        }

        private static GameObject EnsureSummonPromotedParticlePrefab(
            string sourcePrefabPath,
            string targetPrefabPath,
            string rootName,
            bool loopParticles,
            bool playOnAwake,
            int minimumParticleSystems)
        {
            EnsureFolderForAsset(targetPrefabPath);
            GameObject sourcePrefab = LoadAsset<GameObject>(sourcePrefabPath);
            GameObject instance = PrefabUtility.InstantiatePrefab(sourcePrefab) as GameObject;
            if (instance == null)
            {
                instance = UnityEngine.Object.Instantiate(sourcePrefab);
            }

            try
            {
                if (PrefabUtility.IsPartOfPrefabInstance(instance))
                {
                    PrefabUtility.UnpackPrefabInstance(
                        instance,
                        PrefabUnpackMode.Completely,
                        InteractionMode.AutomatedAction);
                }

                instance.name = rootName;
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localRotation = Quaternion.identity;
                instance.transform.localScale = Vector3.one;
                UnpackNestedPrefabInstances(instance);
                StripNonGameMonoBehaviours(instance);
                RemoveColliders(instance);
                DisableVfxAudioSources(instance);
                ConfigurePromotedVfxParticles(instance, loopParticles, playOnAwake);
                RemapSummonPromotedVfxRenderers(instance);
                ValidatePromotedParticleVfx(instance.transform, rootName, minimumParticleSystems);

                GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(instance, targetPrefabPath);
                if (savedPrefab == null)
                {
                    throw new InvalidOperationException($"Failed to save promoted summon VFX prefab at {targetPrefabPath}.");
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(instance);
            }

            ValidateNoImportedAssetReference(targetPrefabPath);
            return LoadAsset<GameObject>(targetPrefabPath);
        }

        private static GameObject EnsureSummonSlot3PromotedDragonVisualPrefab()
        {
            EnsureFolderForAsset(SummonSlot3DragonVisualPrefabPath);
            EnsureCopiedAsset(ImportedVolcanoDragonModelPath, SummonSlot3DragonModelPath);
            ConfigureSummonDragonModelImporter(SummonSlot3DragonModelPath);
            Avatar dragonAvatar = LoadPromotedDragonAvatar();
            EnsureSummonDragonAnimationClip(
                ImportedVolcanoDragonFlyStationaryClipPath,
                SummonSlot3DragonFlyStationaryClipPath,
                "FlyStationary",
                loopTime: true,
                dragonAvatar);
            EnsureSummonDragonAnimationClip(
                ImportedVolcanoDragonSpitFireClipPath,
                SummonSlot3DragonSpitFireClipPath,
                "FlyStationarySpitFireBall",
                loopTime: false,
                dragonAvatar);
            EnsureSummonDragonAnimationClip(
                ImportedVolcanoDragonFallingClipPath,
                SummonSlot3DragonFallingClipPath,
                "FlyStationaryGetHitToFalling",
                loopTime: false,
                dragonAvatar);
            AnimatorController controller = EnsureSummonDragonAnimatorController();

            GameObject sourcePrefab = LoadAsset<GameObject>(CinematicSupportDragonSourcePrefabPath);
            GameObject instance = PrefabUtility.InstantiatePrefab(sourcePrefab) as GameObject;
            if (instance == null)
            {
                instance = UnityEngine.Object.Instantiate(sourcePrefab);
            }

            try
            {
                if (PrefabUtility.IsPartOfPrefabInstance(instance))
                {
                    PrefabUtility.UnpackPrefabInstance(
                        instance,
                        PrefabUnpackMode.Completely,
                        InteractionMode.AutomatedAction);
                }

                instance.name = SummonSlot3DragonVisualPrefabName;
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localRotation = Quaternion.identity;
                instance.transform.localScale = Vector3.one;
                UnpackNestedPrefabInstances(instance);
                StripNonGameMonoBehaviours(instance);
                RemoveColliders(instance);
                DisableVfxAudioSources(instance);
                RemapSummonDragonRenderers(instance);

                Animator animator = instance.GetComponent<Animator>();
                if (animator == null)
                {
                    animator = instance.GetComponentInChildren<Animator>(includeInactive: true);
                }

                if (animator == null)
                {
                    animator = instance.AddComponent<Animator>();
                }

                animator.avatar = dragonAvatar;
                animator.runtimeAnimatorController = controller;
                animator.applyRootMotion = false;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                EditorUtility.SetDirty(animator);

                ValidateSummonActorRoleVisualContents(instance, SummonSlot3DragonVisualPrefabName);
                ValidateSummonDragonVisualContents(instance, SummonSlot3DragonVisualPrefabName);
                GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(instance, SummonSlot3DragonVisualPrefabPath);
                if (savedPrefab == null)
                {
                    throw new InvalidOperationException($"Failed to save promoted summon dragon visual at {SummonSlot3DragonVisualPrefabPath}.");
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(instance);
            }

            ValidateNoImportedAssetReference(SummonSlot3DragonVisualPrefabPath);
            return LoadAsset<GameObject>(SummonSlot3DragonVisualPrefabPath);
        }

        private static Transform AttachSummonDragonVisual(
            Transform parent,
            string targetVisualName,
            Vector3 localPosition,
            Vector3 localEulerAngles,
            Vector3 localScale)
        {
            EnsureSummonSlot3PromotedDragonVisualPrefab();
            string visualPrefix = targetVisualName.Contains("_", StringComparison.Ordinal)
                ? targetVisualName.Substring(0, targetVisualName.LastIndexOf('_') + 1)
                : targetVisualName;
            RemoveChildrenWithPrefix(parent, visualPrefix);

            GameObject sourcePrefab = LoadAsset<GameObject>(SummonSlot3DragonVisualPrefabPath);
            GameObject visual = PrefabUtility.InstantiatePrefab(sourcePrefab, parent.gameObject.scene) as GameObject;
            if (visual == null)
            {
                visual = UnityEngine.Object.Instantiate(sourcePrefab);
            }

            visual.name = targetVisualName;
            visual.transform.SetParent(parent, worldPositionStays: false);
            visual.transform.localPosition = localPosition;
            visual.transform.localRotation = Quaternion.Euler(localEulerAngles);
            visual.transform.localScale = localScale;
            ValidateSummonActorRoleVisualContents(visual, targetVisualName);
            ValidateSummonDragonVisualContents(visual, targetVisualName);
            EditorUtility.SetDirty(visual);
            return visual.transform;
        }

        private static void ConfigureSummonAttackPromotedParticleBeam(
            GameObject actorRoot,
            SummonFrontlineProxy proxy,
            string beamName,
            string promotedPrefabPath,
            Vector3 localPosition,
            Vector3 localEulerAngles,
            Vector3 localScale,
            Color tierOneColor,
            Color tierTwoColor,
            Color tierThreeColor,
            float tierScaleStep,
            float pulseScale,
            float pulseSpeed,
            int minimumParticleSystems,
            float beamWidthMultiplier = 1f,
            bool loopParticles = true)
        {
            DestroyChildIfPresent(actorRoot.transform, beamName);
            GameObject sourcePrefab = LoadAsset<GameObject>(promotedPrefabPath);
            GameObject beamRoot = PrefabUtility.InstantiatePrefab(sourcePrefab, actorRoot.scene) as GameObject;
            if (beamRoot == null)
            {
                beamRoot = UnityEngine.Object.Instantiate(sourcePrefab);
            }

            beamRoot.name = beamName;
            beamRoot.transform.SetParent(actorRoot.transform, worldPositionStays: false);
            beamRoot.transform.localPosition = localPosition;
            beamRoot.transform.localRotation = Quaternion.Euler(localEulerAngles);
            beamRoot.transform.localScale = localScale;
            RemoveColliders(beamRoot);
            DisableVfxAudioSources(beamRoot);
            ConfigurePromotedVfxParticles(beamRoot, loopParticles, playOnAwake: false);
            if (beamName.IndexOf("ChargeImpact", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                TamePromotedChargeParticles(
                    beamRoot,
                    emissionScale: 0.42f,
                    sizeScale: 0.55f,
                    speedScale: 0.65f,
                    lifetimeScale: 0.58f);
            }

            ValidatePromotedParticleVfx(beamRoot.transform, beamName, minimumParticleSystems);
            beamRoot.SetActive(false);

            Renderer[] renderers = beamRoot.GetComponentsInChildren<Renderer>(includeInactive: true);
            ParticleSystem[] particles = beamRoot.GetComponentsInChildren<ParticleSystem>(includeInactive: true);
            SummonAttackBeamPresenter beamPresenter = EnsureComponent<SummonAttackBeamPresenter>(actorRoot);
            SetObjectReference(beamPresenter, "proxy", proxy);
            SetObjectReference(beamPresenter, "beamRoot", beamRoot.transform);
            SetObjectReferenceArray(beamPresenter, "beamRenderers", ToObjectReferences(renderers));
            SetObjectReferenceArray(beamPresenter, "beamParticles", ToObjectReferences(particles));
            SetColor(beamPresenter, "tierOneColor", tierOneColor);
            SetColor(beamPresenter, "tierTwoColor", tierTwoColor);
            SetColor(beamPresenter, "tierThreeColor", tierThreeColor);
            SetFloat(beamPresenter, "tierScaleStep", tierScaleStep);
            SetFloat(beamPresenter, "pulseScale", pulseScale);
            SetFloat(beamPresenter, "pulseSpeed", pulseSpeed);
            SetFloat(beamPresenter, "beamWidthMultiplier", beamWidthMultiplier);
            EditorUtility.SetDirty(actorRoot);
        }

        private static Transform ConfigureSummonMovementPromotedParticleVfx(
            Transform parent,
            string vfxName,
            string promotedPrefabPath,
            Vector3 localPosition,
            Vector3 localEulerAngles,
            Vector3 localScale,
            int minimumParticleSystems)
        {
            DestroyChildIfPresent(parent, vfxName);
            GameObject sourcePrefab = LoadAsset<GameObject>(promotedPrefabPath);
            GameObject vfxRoot = PrefabUtility.InstantiatePrefab(sourcePrefab, parent.gameObject.scene) as GameObject;
            if (vfxRoot == null)
            {
                vfxRoot = UnityEngine.Object.Instantiate(sourcePrefab);
            }

            vfxRoot.name = vfxName;
            vfxRoot.transform.SetParent(parent, worldPositionStays: false);
            vfxRoot.transform.localPosition = localPosition;
            vfxRoot.transform.localRotation = Quaternion.Euler(localEulerAngles);
            vfxRoot.transform.localScale = localScale;
            RemoveColliders(vfxRoot);
            DisableVfxAudioSources(vfxRoot);
            ConfigurePromotedVfxParticles(vfxRoot, loopParticles: true, playOnAwake: false);
            if (vfxName.IndexOf("ChargeRush", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                TamePromotedChargeParticles(
                    vfxRoot,
                    emissionScale: 0.34f,
                    sizeScale: 0.62f,
                    speedScale: 0.72f,
                    lifetimeScale: 0.68f);
            }

            ValidatePromotedParticleVfx(vfxRoot.transform, vfxName, minimumParticleSystems);
            vfxRoot.SetActive(false);
            EditorUtility.SetDirty(vfxRoot);
            return vfxRoot.transform;
        }

        private static void TamePromotedChargeParticles(
            GameObject vfxRoot,
            float emissionScale,
            float sizeScale,
            float speedScale,
            float lifetimeScale)
        {
            ParticleSystem[] particleSystems = vfxRoot.GetComponentsInChildren<ParticleSystem>(includeInactive: true);
            for (int i = 0; i < particleSystems.Length; i++)
            {
                ParticleSystem particleSystem = particleSystems[i];
                ParticleSystem.MainModule main = particleSystem.main;
                main.startSizeMultiplier *= Mathf.Clamp(sizeScale, 0.05f, 1f);
                main.startSpeedMultiplier *= Mathf.Clamp(speedScale, 0.05f, 1f);
                main.startLifetimeMultiplier *= Mathf.Clamp(lifetimeScale, 0.05f, 1f);

                ParticleSystem.EmissionModule emission = particleSystem.emission;
                emission.rateOverTimeMultiplier *= Mathf.Clamp(emissionScale, 0.05f, 1f);
                emission.rateOverDistanceMultiplier *= Mathf.Clamp(emissionScale, 0.05f, 1f);

                EditorUtility.SetDirty(particleSystem);
            }

            ParticleSystemRenderer[] renderers =
                vfxRoot.GetComponentsInChildren<ParticleSystemRenderer>(includeInactive: true);
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].maxParticleSize = Mathf.Min(renderers[i].maxParticleSize, 0.35f);
                EditorUtility.SetDirty(renderers[i]);
            }
        }

        private static UnityEngine.Object[] ToObjectReferences(UnityEngine.Object[] values)
        {
            UnityEngine.Object[] references = new UnityEngine.Object[values != null ? values.Length : 0];
            if (values == null)
            {
                return references;
            }

            for (int i = 0; i < values.Length; i++)
            {
                references[i] = values[i];
            }

            return references;
        }

        private static void RemapSummonPromotedVfxRenderers(GameObject vfxRoot)
        {
            Renderer[] renderers = vfxRoot.GetComponentsInChildren<Renderer>(includeInactive: true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                Material[] materials = renderer.sharedMaterials;
                for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                {
                    if (materials[materialIndex] != null)
                    {
                        materials[materialIndex] = EnsurePromotedSummonVfxMaterial(materials[materialIndex]);
                    }
                }

                renderer.sharedMaterials = materials;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                renderer.allowOcclusionWhenDynamic = false;
                EditorUtility.SetDirty(renderer);
            }

            ParticleSystemRenderer[] particleRenderers =
                vfxRoot.GetComponentsInChildren<ParticleSystemRenderer>(includeInactive: true);
            for (int i = 0; i < particleRenderers.Length; i++)
            {
                if (particleRenderers[i].mesh != null)
                {
                    particleRenderers[i].mesh = EnsurePromotedSummonVfxMesh(particleRenderers[i].mesh);
                    EditorUtility.SetDirty(particleRenderers[i]);
                }
            }
        }

        private static Material EnsurePromotedSummonVfxMaterial(Material sourceMaterial)
        {
            return EnsurePromotedTransparentParticleMaterial(
                sourceMaterial,
                SummonPromotedVfxMaterialRoot,
                SummonPromotedVfxTextureRoot,
                "DB_SummonForge3D_");
        }

        private static Mesh EnsurePromotedSummonVfxMesh(Mesh sourceMesh)
        {
            return EnsurePromotedMeshAsset(sourceMesh, SummonPromotedVfxMeshRoot, "DB_SummonForge3D_");
        }

        private static void RemapSummonDragonRenderers(GameObject dragonRoot)
        {
            Renderer[] renderers = dragonRoot.GetComponentsInChildren<Renderer>(includeInactive: true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                Material[] materials = renderer.sharedMaterials;
                for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                {
                    materials[materialIndex] = EnsurePromotedDragonMaterial(materials[materialIndex]);
                }

                renderer.sharedMaterials = materials;
                renderer.shadowCastingMode = ShadowCastingMode.On;
                renderer.receiveShadows = true;
                renderer.allowOcclusionWhenDynamic = true;
                EditorUtility.SetDirty(renderer);
            }

            SkinnedMeshRenderer[] skinnedRenderers =
                dragonRoot.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true);
            for (int i = 0; i < skinnedRenderers.Length; i++)
            {
                if (skinnedRenderers[i].sharedMesh != null)
                {
                    skinnedRenderers[i].sharedMesh = EnsurePromotedDragonMesh(skinnedRenderers[i].sharedMesh);
                    EditorUtility.SetDirty(skinnedRenderers[i]);
                }
            }

            MeshFilter[] meshFilters = dragonRoot.GetComponentsInChildren<MeshFilter>(includeInactive: true);
            for (int i = 0; i < meshFilters.Length; i++)
            {
                if (meshFilters[i].sharedMesh != null)
                {
                    meshFilters[i].sharedMesh = EnsurePromotedDragonMesh(meshFilters[i].sharedMesh);
                    EditorUtility.SetDirty(meshFilters[i]);
                }
            }
        }

        private static Material EnsurePromotedDragonMaterial(Material sourceMaterial)
        {
            if (sourceMaterial == null)
            {
                return null;
            }

            string sourcePath = AssetDatabase.GetAssetPath(sourceMaterial).Replace('\\', '/');
            if (sourcePath.StartsWith("Assets/_Game/", StringComparison.Ordinal))
            {
                RemapImportedMaterialTextures(sourceMaterial, SummonDragonTextureRoot);
                RemapImportedSerializedMaterialTextures(sourceMaterial, SummonDragonTextureRoot);
                NormalizeDragonMetallicTextureSlots(sourceMaterial);
                SavePromotedMaterialAsset(sourceMaterial);
                return sourceMaterial;
            }

            EnsureFolderForAsset(SummonDragonMaterialRoot + "/.keep");
            EnsureFolderForAsset(SummonDragonTextureRoot + "/.keep");
            string targetPath = SummonDragonMaterialRoot + "/DB_VolcanoDragon_"
                + SanitizeAssetFileName(sourceMaterial.name)
                + ".mat";
            Material targetMaterial = AssetDatabase.LoadAssetAtPath<Material>(targetPath);
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? sourceMaterial.shader ?? Shader.Find("Standard");
            if (targetMaterial == null)
            {
                targetMaterial = new Material(shader);
                AssetDatabase.CreateAsset(targetMaterial, targetPath);
            }

            targetMaterial.shader = shader;
            targetMaterial.CopyPropertiesFromMaterial(sourceMaterial);
            string[] textureProperties = sourceMaterial.GetTexturePropertyNames();
            for (int i = 0; i < textureProperties.Length; i++)
            {
                Texture texture = sourceMaterial.GetTexture(textureProperties[i]);
                if (texture == null)
                {
                    continue;
                }

                Texture promotedTexture = EnsurePromotedTextureAsset(
                    texture,
                    SummonDragonTextureRoot,
                    ResolveTextureUsage(texture, textureProperties[i]));
                SetTextureIfPresent(targetMaterial, textureProperties[i], promotedTexture);
            }

            AssignPromotedDragonTextureByName(
                sourceMaterial,
                targetMaterial,
                "albedo",
                SummonPromotedTextureUsage.Color,
                "_BaseMap",
                "_MainTex");
            AssignPromotedDragonTextureByName(
                sourceMaterial,
                targetMaterial,
                "normal",
                SummonPromotedTextureUsage.Normal,
                "_BumpMap");
            AssignPromotedDragonTextureByName(
                sourceMaterial,
                targetMaterial,
                "metallic",
                SummonPromotedTextureUsage.Linear,
                "_MetallicGlossMap",
                "_SpecGlossMap");
            AssignPromotedDragonTextureByName(
                sourceMaterial,
                targetMaterial,
                "occlusion",
                SummonPromotedTextureUsage.Linear,
                "_OcclusionMap");
            AssignPromotedDragonTextureByName(
                sourceMaterial,
                targetMaterial,
                "emissive",
                SummonPromotedTextureUsage.Color,
                "_EmissionMap");
            RemapImportedMaterialTextures(targetMaterial, SummonDragonTextureRoot);
            RemapImportedSerializedMaterialTextures(targetMaterial, SummonDragonTextureRoot);
            NormalizeDragonMetallicTextureSlots(targetMaterial);
            if (targetMaterial.GetTexture("_BumpMap") != null)
            {
                targetMaterial.EnableKeyword("_NORMALMAP");
            }

            if (targetMaterial.GetTexture("_MetallicGlossMap") != null)
            {
                targetMaterial.EnableKeyword("_METALLICSPECGLOSSMAP");
            }

            if (targetMaterial.GetTexture("_EmissionMap") != null)
            {
                targetMaterial.EnableKeyword("_EMISSION");
            }

            EditorUtility.SetDirty(targetMaterial);
            SavePromotedMaterialAsset(targetMaterial);
            return targetMaterial;
        }

        private static void NormalizeDragonMetallicTextureSlots(Material material)
        {
            Texture metallicTexture = GetTextureIfPresent(material, "_MetallicGlossMap");
            Texture specularTexture = GetTextureIfPresent(material, "_SpecGlossMap");
            Texture texture = PreferGameTexture(metallicTexture)
                ?? PreferGameTexture(specularTexture)
                ?? metallicTexture
                ?? specularTexture;
            if (texture == null)
            {
                return;
            }

            Texture promotedTexture = EnsurePromotedTextureAsset(
                texture,
                SummonDragonTextureRoot,
                SummonPromotedTextureUsage.Linear);
            SetTextureIfPresent(material, "_MetallicGlossMap", promotedTexture);
            SetTextureIfPresent(material, "_SpecGlossMap", promotedTexture);
            EditorUtility.SetDirty(material);
        }

        private static Texture PreferGameTexture(Texture texture)
        {
            if (texture == null)
            {
                return null;
            }

            string texturePath = AssetDatabase.GetAssetPath(texture).Replace('\\', '/');
            return texturePath.StartsWith("Assets/_Game/", StringComparison.Ordinal)
                ? texture
                : null;
        }

        private static Texture GetTextureIfPresent(Material material, string propertyName)
        {
            if (material == null || !material.HasProperty(propertyName))
            {
                return null;
            }

            return material.GetTexture(propertyName);
        }

        private static void SavePromotedMaterialAsset(Material material)
        {
            string materialPath = AssetDatabase.GetAssetPath(material).Replace('\\', '/');
            if (string.IsNullOrWhiteSpace(materialPath)
                || !materialPath.StartsWith("Assets/_Game/", StringComparison.Ordinal))
            {
                return;
            }

            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(materialPath, ImportAssetOptions.ForceUpdate);
        }

        private static void AssignPromotedDragonTextureByName(
            Material sourceMaterial,
            Material targetMaterial,
            string nameFragment,
            SummonPromotedTextureUsage usage,
            params string[] targetProperties)
        {
            Texture sourceTexture = FindMaterialTextureByName(sourceMaterial, nameFragment);
            if (sourceTexture == null)
            {
                return;
            }

            Texture promotedTexture = EnsurePromotedTextureAsset(sourceTexture, SummonDragonTextureRoot, usage);
            for (int i = 0; i < targetProperties.Length; i++)
            {
                SetTextureIfPresent(targetMaterial, targetProperties[i], promotedTexture);
            }
        }

        private static Texture FindMaterialTextureByName(Material material, string nameFragment)
        {
            if (material == null || string.IsNullOrWhiteSpace(nameFragment))
            {
                return null;
            }

            string[] textureProperties = material.GetTexturePropertyNames();
            for (int i = 0; i < textureProperties.Length; i++)
            {
                Texture texture = material.GetTexture(textureProperties[i]);
                if (texture == null)
                {
                    continue;
                }

                string key = (textureProperties[i] + " " + texture.name).ToLowerInvariant();
                if (key.Contains(nameFragment, StringComparison.Ordinal))
                {
                    return texture;
                }
            }

            return null;
        }

        private static void RemapImportedMaterialTextures(Material material, string textureRoot)
        {
            if (material == null)
            {
                return;
            }

            string[] textureProperties = material.GetTexturePropertyNames();
            for (int i = 0; i < textureProperties.Length; i++)
            {
                string propertyName = textureProperties[i];
                Texture texture = material.GetTexture(propertyName);
                if (texture == null)
                {
                    continue;
                }

                string texturePath = AssetDatabase.GetAssetPath(texture).Replace('\\', '/');
                if (!texturePath.StartsWith("Assets/_Imported/", StringComparison.Ordinal))
                {
                    continue;
                }

                Texture promotedTexture = EnsurePromotedTextureAsset(
                    texture,
                    textureRoot,
                    ResolveTextureUsage(texture, propertyName));
                SetTextureIfPresent(material, propertyName, promotedTexture);
            }
        }

        private static void RemapImportedSerializedMaterialTextures(Material material, string textureRoot)
        {
            if (material == null)
            {
                return;
            }

            var serializedMaterial = new SerializedObject(material);
            SerializedProperty texEnvs = serializedMaterial.FindProperty("m_SavedProperties.m_TexEnvs");
            if (texEnvs == null || !texEnvs.isArray)
            {
                return;
            }

            for (int i = 0; i < texEnvs.arraySize; i++)
            {
                SerializedProperty entry = texEnvs.GetArrayElementAtIndex(i);
                SerializedProperty propertyName = entry.FindPropertyRelative("first");
                SerializedProperty textureRef = entry.FindPropertyRelative("second.m_Texture");
                if (textureRef == null || textureRef.objectReferenceValue is not Texture texture)
                {
                    continue;
                }

                string texturePath = AssetDatabase.GetAssetPath(texture).Replace('\\', '/');
                if (!texturePath.StartsWith("Assets/_Imported/", StringComparison.Ordinal))
                {
                    continue;
                }

                Texture promotedTexture = EnsurePromotedTextureAsset(
                    texture,
                    textureRoot,
                    ResolveTextureUsage(texture, propertyName != null ? propertyName.stringValue : string.Empty));
                textureRef.objectReferenceValue = promotedTexture;
            }

            serializedMaterial.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(material);
        }

        private static Material EnsurePromotedTransparentParticleMaterial(
            Material sourceMaterial,
            string materialRoot,
            string textureRoot,
            string assetPrefix)
        {
            if (sourceMaterial == null)
            {
                return null;
            }

            string sourcePath = AssetDatabase.GetAssetPath(sourceMaterial).Replace('\\', '/');
            if (sourcePath.StartsWith("Assets/_Game/", StringComparison.Ordinal))
            {
                return sourceMaterial;
            }

            EnsureFolderForAsset(materialRoot + "/.keep");
            EnsureFolderForAsset(textureRoot + "/.keep");
            string targetPath = materialRoot + "/" + assetPrefix
                + SanitizeAssetFileName(sourceMaterial.name)
                + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(targetPath);
            if (material == null)
            {
                material = new Material(ResolveUnlitShader());
                AssetDatabase.CreateAsset(material, targetPath);
            }

            material.shader = ResolveUnlitShader();
            ConfigureTransparentVfxMaterial(material, ResolveMagicMissilesMaterialColor(sourceMaterial));
            string[] textureProperties = sourceMaterial.GetTexturePropertyNames();
            for (int i = 0; i < textureProperties.Length; i++)
            {
                Texture texture = sourceMaterial.GetTexture(textureProperties[i]);
                if (texture == null)
                {
                    continue;
                }

                Texture promotedTexture = EnsurePromotedTextureAsset(
                    texture,
                    textureRoot,
                    SummonPromotedTextureUsage.Color);
                SetTextureIfPresent(material, textureProperties[i], promotedTexture);
                SetTextureIfPresent(material, "_MainTex", promotedTexture);
                SetTextureIfPresent(material, "_BaseMap", promotedTexture);
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        private static Texture EnsurePromotedTextureAsset(
            Texture sourceTexture,
            string textureRoot,
            SummonPromotedTextureUsage usage)
        {
            string sourcePath = AssetDatabase.GetAssetPath(sourceTexture).Replace('\\', '/');
            if (sourcePath.StartsWith("Assets/_Game/", StringComparison.Ordinal)
                || string.IsNullOrWhiteSpace(sourcePath)
                || !sourcePath.StartsWith("Assets/", StringComparison.Ordinal))
            {
                return sourceTexture;
            }

            string targetPath = textureRoot + "/"
                + SanitizeAssetFileName(Path.GetFileNameWithoutExtension(sourcePath))
                + Path.GetExtension(sourcePath);
            EnsureFolderForAsset(targetPath);
            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(targetPath) == null)
            {
                if (!AssetDatabase.CopyAsset(sourcePath, targetPath))
                {
                    throw new InvalidOperationException($"Failed to promote texture from {sourcePath} to {targetPath}.");
                }
            }

            ConfigureSummonPromotedTextureImporter(targetPath, usage);
            Texture promotedTexture = AssetDatabase.LoadAssetAtPath<Texture>(targetPath);
            if (promotedTexture == null)
            {
                throw new InvalidOperationException($"Failed to load promoted texture at {targetPath}.");
            }

            return promotedTexture;
        }

        private static Mesh EnsurePromotedDragonMesh(Mesh sourceMesh)
        {
            return EnsurePromotedMeshAsset(sourceMesh, SummonDragonModelRoot, "DB_VolcanoDragon_");
        }

        private static Mesh EnsurePromotedMeshAsset(Mesh sourceMesh, string meshRoot, string assetPrefix)
        {
            if (sourceMesh == null)
            {
                return null;
            }

            string sourcePath = AssetDatabase.GetAssetPath(sourceMesh).Replace('\\', '/');
            if (sourcePath.StartsWith("Assets/_Game/", StringComparison.Ordinal))
            {
                return sourceMesh;
            }

            if (string.IsNullOrWhiteSpace(sourcePath) || sourcePath.StartsWith("Library/", StringComparison.Ordinal))
            {
                string generatedTargetPath = meshRoot + "/" + assetPrefix
                    + SanitizeAssetFileName(sourceMesh.name)
                    + ".asset";
                EnsureFolderForAsset(generatedTargetPath);
                Mesh generatedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(generatedTargetPath);
                if (generatedMesh == null)
                {
                    generatedMesh = UnityEngine.Object.Instantiate(sourceMesh);
                    generatedMesh.name = sourceMesh.name;
                    AssetDatabase.CreateAsset(generatedMesh, generatedTargetPath);
                    AssetDatabase.ImportAsset(generatedTargetPath, ImportAssetOptions.ForceUpdate);
                }

                return generatedMesh;
            }

            string targetPath = string.Equals(meshRoot, SummonDragonModelRoot, StringComparison.Ordinal)
                ? SummonSlot3DragonModelPath
                : meshRoot + "/" + SanitizeAssetFileName(Path.GetFileName(sourcePath));
            EnsureCopiedAsset(sourcePath, targetPath);

            UnityEngine.Object[] promotedAssets = AssetDatabase.LoadAllAssetsAtPath(targetPath);
            for (int i = 0; i < promotedAssets.Length; i++)
            {
                if (promotedAssets[i] is Mesh promotedMesh
                    && string.Equals(promotedMesh.name, sourceMesh.name, StringComparison.Ordinal))
                {
                    return promotedMesh;
                }
            }

            for (int i = 0; i < promotedAssets.Length; i++)
            {
                if (promotedAssets[i] is Mesh promotedMesh)
                {
                    return promotedMesh;
                }
            }

            throw new InvalidOperationException($"Failed to promote mesh {sourceMesh.name} from {sourcePath} to {targetPath}.");
        }

        private static void EnsureCopiedAsset(string sourcePath, string targetPath)
        {
            EnsureFolderForAsset(targetPath);
            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(targetPath) != null)
            {
                return;
            }

            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(sourcePath) == null)
            {
                throw new InvalidOperationException($"Missing source asset at {sourcePath}.");
            }

            if (!AssetDatabase.CopyAsset(sourcePath, targetPath))
            {
                throw new InvalidOperationException($"Failed to copy asset from {sourcePath} to {targetPath}.");
            }

            AssetDatabase.ImportAsset(targetPath, ImportAssetOptions.ForceUpdate);
        }

        private static void ConfigureSummonDragonModelImporter(string targetPath)
        {
            if (AssetImporter.GetAtPath(targetPath) is not ModelImporter importer)
            {
                throw new InvalidOperationException($"{targetPath} should import as a model.");
            }

            bool changed = false;
            changed |= SetImporterValue(importer.animationType, ModelImporterAnimationType.Generic, value => importer.animationType = value);
            changed |= SetImporterValue(importer.avatarSetup, ModelImporterAvatarSetup.CreateFromThisModel, value => importer.avatarSetup = value);
            changed |= SetImporterValue(importer.materialImportMode, ModelImporterMaterialImportMode.None, value => importer.materialImportMode = value);
            if (changed)
            {
                importer.SaveAndReimport();
            }
        }

        private static AnimationClip EnsureSummonDragonAnimationClip(
            string sourcePath,
            string targetPath,
            string clipName,
            bool loopTime,
            Avatar avatar)
        {
            EnsureCopiedAsset(sourcePath, targetPath);
            if (AssetImporter.GetAtPath(targetPath) is ModelImporter importer)
            {
                bool changed = false;
                changed |= SetImporterValue(importer.animationType, ModelImporterAnimationType.Generic, value => importer.animationType = value);
                changed |= SetImporterValue(importer.avatarSetup, ModelImporterAvatarSetup.CopyFromOther, value => importer.avatarSetup = value);
                changed |= SetImporterValue(importer.materialImportMode, ModelImporterMaterialImportMode.None, value => importer.materialImportMode = value);
                if (importer.sourceAvatar != avatar)
                {
                    importer.sourceAvatar = avatar;
                    changed = true;
                }

                ModelImporterClipAnimation[] clips = importer.clipAnimations.Length > 0
                    ? importer.clipAnimations
                    : importer.defaultClipAnimations;
                for (int i = 0; i < clips.Length; i++)
                {
                    if (!string.Equals(clips[i].name, clipName, StringComparison.Ordinal)
                        || clips[i].loopTime != loopTime)
                    {
                        clips[i].name = clipName;
                        clips[i].loopTime = loopTime;
                        changed = true;
                    }
                }

                importer.clipAnimations = clips;
                if (changed)
                {
                    importer.SaveAndReimport();
                }
            }

            AnimationClip clip = LoadNamedSubAsset<AnimationClip>(targetPath, clipName);
            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            if (settings.loopTime != loopTime)
            {
                settings.loopTime = loopTime;
                AnimationUtility.SetAnimationClipSettings(clip, settings);
                EditorUtility.SetDirty(clip);
            }

            return clip;
        }

        private static Avatar LoadPromotedDragonAvatar()
        {
            Avatar avatar = LoadNamedSubAsset<Avatar>(SummonSlot3DragonModelPath, "SK_VolcanoDragonAvatar");
            if (avatar == null)
            {
                throw new InvalidOperationException($"Failed to load promoted VolcanoDragon avatar from {SummonSlot3DragonModelPath}.");
            }

            return avatar;
        }

        private static T LoadNamedSubAsset<T>(string assetPath, string preferredName) where T : UnityEngine.Object
        {
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            T fallback = null;
            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] is not T typed)
                {
                    continue;
                }

                if (fallback == null && !typed.name.StartsWith("__preview__", StringComparison.Ordinal))
                {
                    fallback = typed;
                }

                if (string.Equals(typed.name, preferredName, StringComparison.Ordinal)
                    || typed.name.Contains(preferredName, StringComparison.Ordinal))
                {
                    return typed;
                }
            }

            if (fallback != null)
            {
                return fallback;
            }

            throw new InvalidOperationException($"Failed to load {typeof(T).Name} sub-asset {preferredName} from {assetPath}.");
        }

        private static AnimatorController EnsureSummonDragonAnimatorController()
        {
            AnimatorController controller =
                AssetDatabase.LoadAssetAtPath<AnimatorController>(SummonSlot3DragonControllerPath);
            if (controller != null && IsSummonDragonAnimatorControllerCurrent(controller))
            {
                return controller;
            }

            if (controller != null && !AssetDatabase.DeleteAsset(SummonSlot3DragonControllerPath))
            {
                throw new InvalidOperationException($"Failed to replace {SummonSlot3DragonControllerPath}.");
            }

            EnsureFolderForAsset(SummonSlot3DragonControllerPath);
            controller = AnimatorController.CreateAnimatorControllerAtPath(SummonSlot3DragonControllerPath);
            controller.AddParameter(SummonActorMoveSpeedParameter, AnimatorControllerParameterType.Float);
            controller.AddParameter(SummonActorSpawnTrigger, AnimatorControllerParameterType.Trigger);
            controller.AddParameter(SummonActorAttackTrigger, AnimatorControllerParameterType.Trigger);
            controller.AddParameter(SummonActorDeathTrigger, AnimatorControllerParameterType.Trigger);

            AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
            stateMachine.name = "Volcano Dragon Summon";
            AnimatorState idle = AddSummonDragonState(
                stateMachine,
                "FlyStationary",
                SummonSlot3DragonFlyStationaryClipPath,
                true,
                new Vector3(80f, 80f, 0f));
            AnimatorState spawn = AddSummonDragonState(
                stateMachine,
                "SpawnHover",
                SummonSlot3DragonFlyStationaryClipPath,
                true,
                new Vector3(360f, 80f, 0f));
            AnimatorState attack = AddSummonDragonState(
                stateMachine,
                "FlyStationarySpitFireBall",
                SummonSlot3DragonSpitFireClipPath,
                false,
                new Vector3(640f, 80f, 0f));
            AnimatorState death = AddSummonDragonState(
                stateMachine,
                "FlyStationaryGetHitToFalling",
                SummonSlot3DragonFallingClipPath,
                false,
                new Vector3(920f, 80f, 0f));
            stateMachine.defaultState = idle;
            AddAnyTriggerTransition(stateMachine, SummonActorSpawnTrigger, spawn);
            AddAnyTriggerTransition(stateMachine, SummonActorAttackTrigger, attack);
            AddAnyTriggerTransition(stateMachine, SummonActorDeathTrigger, death);
            AddReturnTransition(spawn, idle);
            AddReturnTransition(attack, idle);
            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            return controller;
        }

        private static bool IsSummonDragonAnimatorControllerCurrent(AnimatorController controller)
        {
            if (controller == null
                || controller.layers.Length == 0
                || controller.layers[0].stateMachine == null)
            {
                return false;
            }

            AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
            return HasAnimatorParameter(controller, SummonActorMoveSpeedParameter, AnimatorControllerParameterType.Float)
                && HasAnimatorParameter(controller, SummonActorSpawnTrigger, AnimatorControllerParameterType.Trigger)
                && HasAnimatorParameter(controller, SummonActorAttackTrigger, AnimatorControllerParameterType.Trigger)
                && HasAnimatorParameter(controller, SummonActorDeathTrigger, AnimatorControllerParameterType.Trigger)
                && HasState(stateMachine, "FlyStationary")
                && HasState(stateMachine, "FlyStationarySpitFireBall")
                && HasState(stateMachine, "FlyStationaryGetHitToFalling");
        }

        private static bool HasAnimatorParameter(
            AnimatorController controller,
            string parameterName,
            AnimatorControllerParameterType type)
        {
            AnimatorControllerParameter[] parameters = controller.parameters;
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].type == type
                    && string.Equals(parameters[i].name, parameterName, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static AnimatorState AddSummonDragonState(
            AnimatorStateMachine stateMachine,
            string stateName,
            string clipPath,
            bool loopTime,
            Vector3 position)
        {
            AnimatorState state = stateMachine.AddState(stateName, position);
            AnimationClip clip = LoadNamedSubAsset<AnimationClip>(clipPath, stateName);
            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            if (settings.loopTime != loopTime)
            {
                settings.loopTime = loopTime;
                AnimationUtility.SetAnimationClipSettings(clip, settings);
                EditorUtility.SetDirty(clip);
            }

            state.motion = clip;
            state.writeDefaultValues = true;
            EditorUtility.SetDirty(state);
            return state;
        }

        private static void ConfigureSummonPromotedTextureImporter(string path, SummonPromotedTextureUsage usage)
        {
            if (AssetImporter.GetAtPath(path) is not TextureImporter importer)
            {
                return;
            }

            bool changed = false;
            TextureImporterType textureType = usage == SummonPromotedTextureUsage.Normal
                ? TextureImporterType.NormalMap
                : TextureImporterType.Default;
            changed |= SetImporterValue(importer.textureType, textureType, value => importer.textureType = value);
            changed |= SetImporterValue(importer.mipmapEnabled, true, value => importer.mipmapEnabled = value);
            changed |= SetImporterValue(importer.sRGBTexture, usage == SummonPromotedTextureUsage.Color, value => importer.sRGBTexture = value);
            if (changed)
            {
                importer.SaveAndReimport();
            }
        }

        private static SummonPromotedTextureUsage ResolveTextureUsage(Texture texture, string propertyName)
        {
            string key = (propertyName + " " + texture.name).ToLowerInvariant();
            if (key.Contains("normal") || key.Contains("bump"))
            {
                return SummonPromotedTextureUsage.Normal;
            }

            if (key.Contains("metallic") || key.Contains("smoothness") || key.Contains("occlusion") || key.Contains("mask"))
            {
                return SummonPromotedTextureUsage.Linear;
            }

            return SummonPromotedTextureUsage.Color;
        }

        private static bool SetImporterValue<T>(T currentValue, T desiredValue, Action<T> applyValue)
        {
            if (Equals(currentValue, desiredValue))
            {
                return false;
            }

            applyValue(desiredValue);
            return true;
        }

        private static void ValidateSummonDragonVisualContents(GameObject visual, string label)
        {
            Animator animator = visual.GetComponent<Animator>();
            if (animator == null
                || animator.runtimeAnimatorController != LoadAsset<RuntimeAnimatorController>(SummonSlot3DragonControllerPath)
                || animator.avatar == null)
            {
                throw new InvalidOperationException($"{label} must use the promoted VolcanoDragon summon Animator contract.");
            }

            ValidateGameOwnedAsset(animator.runtimeAnimatorController, $"{label} dragon Animator Controller");
            ValidateNoImportedDependencies(animator.runtimeAnimatorController, $"{label} dragon Animator Controller");
            ValidateGameOwnedAsset(animator.avatar, $"{label} dragon Avatar");
            ValidateNoImportedDependencies(animator.avatar, $"{label} dragon Avatar");

            SkinnedMeshRenderer[] skinnedRenderers =
                visual.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true);
            if (skinnedRenderers.Length == 0)
            {
                throw new InvalidOperationException($"{label} must use the promoted VolcanoDragon skinned mesh, not generated primitives.");
            }

            if (visual.transform.Find("DragonBody") != null
                || visual.transform.Find("DragonHead") != null
                || visual.transform.Find("DragonLeftWing") != null
                || visual.transform.Find("DragonRightWing") != null)
            {
                throw new InvalidOperationException($"{label} must not fall back to the generated primitive dragon proxy.");
            }

            ValidateNoImportedDependencies(visual, label);
        }
    }
}
