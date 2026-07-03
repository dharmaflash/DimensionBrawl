using System;
using DimensionBrawl.AI;
using DimensionBrawl.Combat;
using DimensionBrawl.Player;
using DimensionBrawl.Presentation;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace DimensionBrawl.Editor
{
    public static partial class ActionFoundationBossBarrageLaneReviewSetup
    {
        [MenuItem("DimensionBrawl/Reapply Action Foundation Summon Actor Health Bars")]
        public static void ReapplySummonActorHealthBarsMenu()
        {
            EnsureSummonActorPrefab();
            EnsureSummonSlot2ActorPrefab();
            EnsureSummonSlot3ActorPrefab();
            EnsureBossSummonPressureActorPrefab();
            EnsureBossLaserSummonActorPrefab();
            AssetDatabase.SaveAssets();
            Debug.Log("Reapplied ActionFoundation summon actor health bars.");
        }

        [MenuItem("DimensionBrawl/Reapply Action Foundation Summon Frontline VFX")]
        public static void ReapplySummonFrontlineVfxMenu()
        {
            EnsureSummonPromotedPresentationAssets();
            EnsureSummonEntryCuePrefab();
            EnsureSummonActorPrefab();
            EnsureSummonSlot2ActorPrefab();
            EnsureSummonSlot3ActorPrefab();
            EnsureBossSummonPressureActorPrefab();
            EnsureBossLaserSummonActorPrefab();
            EnsureSummonPresentationCandidateProfiles();
            AssetDatabase.SaveAssets();
            Debug.Log("Reapplied ActionFoundation summon frontline VFX assets.");
        }

        [MenuItem("DimensionBrawl/Reapply Action Foundation Boss Laser Summon")]
        public static void ReapplyBossLaserSummonMenu()
        {
            EnsureBossLaserSummonActorPrefab();
            EnsureBossSummonPressureProfile();
            AssetDatabase.SaveAssets();
            Debug.Log("Reapplied ActionFoundation boss laser summon assets.");
        }

        private static void EnsureSummonPresentationCandidateProfiles()
        {
            CombatVfxCueProfile vfxCueProfile =
                LoadAsset<CombatVfxCueProfile>(ActionFoundationCombatVfxSetup.CombatVfxCueProfilePath);

            ConfigureSummonPresentationCandidateProfile(
                LoadOrCreateSummonPresentationCandidateProfile(SummonSlot1PresentationCandidateProfilePath),
                "PlayerSummon.ChargeBruiser",
                "Player Summon - Charge Bruiser",
                SummonPresentationSide.PlayerSummon,
                SummonSlot1ActorPrefabPath,
                ActionFoundationEnemyRoleCandidateSetup.ShieldBreakerEliteCandidateProfilePath,
                SummonSlot1ActorVisualName,
                SummonSlot1ActorVisualRoleId,
                vfxCueProfile,
                "Promoted ShieldBreakerElite heavy armor Animator stands in for a forward charge, body check, and lingering melee pressure read.",
                "Magic-circle entry, ground rush trail, front impact burst, short shock screen, and charge shockwave separate it from ranged summons.",
                "Keep the bulky sci-fi helmet body as the reviewed ally bruiser unless a dedicated ally model is approved.");

            ConfigureSummonPresentationCandidateProfile(
                LoadOrCreateSummonPresentationCandidateProfile(SummonSlot2PresentationCandidateProfilePath),
                "PlayerSummon.LaserSoldier",
                "Player Summon - SciFi Laser Soldier",
                SummonPresentationSide.PlayerSummon,
                SummonSlot2ActorPrefabPath,
                ActionFoundationEnemyRoleCandidateSetup.LineCasterCandidateProfilePath,
                SummonSlot2ActorVisualName,
                SummonSlot2ActorVisualRoleId,
                vfxCueProfile,
                "Promoted SciFi LineCaster rifleman Animator stands in for the second ally summon laser-support read.",
                "Magic-circle entry, cyan muzzle beam strip, repeated narrow volleys, and quick attack flashes distinguish it from the slam slot.",
                "Keep this on the SciFi rifleman line until a dedicated ally laser soldier model is reviewed.");

            ConfigureSummonPresentationCandidateProfile(
                LoadOrCreateSummonPresentationCandidateProfile(SummonSlot3PresentationCandidateProfilePath),
                "PlayerSummon.FireDragon",
                "Player Summon - Fire Dragon Breath",
                SummonPresentationSide.PlayerSummon,
                SummonSlot3ActorPrefabPath,
                ActionFoundationEnemyRoleCandidateSetup.FinalStandCommanderEliteCandidateProfilePath,
                SummonSlot3ActorVisualName,
                SummonSlot3ActorVisualRoleId,
                vfxCueProfile,
                "Promoted VolcanoDragon_PBR model uses a summon controller with hover, entry, breath attack, and falling reads.",
                "Magic-circle entry, airborne dragon body, authored flame-breath particle stack, and wide fire volleys distinguish it from S1/S2.",
                "Keep the promoted VolcanoDragon summon visual and FORGE3D flame breath as the reviewed high-cost summon package.",
                SummonSlot3DragonVisualPrefabPath);

            ConfigureSummonPresentationCandidateProfile(
                LoadOrCreateSummonPresentationCandidateProfile(BossSummonPressurePresentationCandidateProfilePath),
                "BossPressure.AuraCaptain",
                "Boss Pressure Summon - Aura Captain Elite",
                SummonPresentationSide.BossPressure,
                BossSummonPressureActorPrefabPath,
                ActionFoundationEnemyRoleCandidateSetup.AuraCaptainEliteCandidateProfilePath,
                BossSummonPressureActorVisualName,
                BossSummonPressureActorVisualRoleId,
                vfxCueProfile,
                "Promoted AuraCaptainElite role Animator stands in for boss-side summon-pressure command reads.",
                "Enemy pressure screen, pressure pulse, and boss-side intercept colors distinguish it from the player summon.",
                "Replace with a dedicated boss pressure summon after boss roster art is reviewed, without changing boss cost data.");

            AssetDatabase.SaveAssets();
        }

        private static SummonPresentationCandidateProfile LoadOrCreateSummonPresentationCandidateProfile(string assetPath)
        {
            EnsureFolderForAsset(assetPath);
            SummonPresentationCandidateProfile profile =
                AssetDatabase.LoadAssetAtPath<SummonPresentationCandidateProfile>(assetPath);
            if (profile != null)
            {
                return profile;
            }

            profile = ScriptableObject.CreateInstance<SummonPresentationCandidateProfile>();
            AssetDatabase.CreateAsset(profile, assetPath);
            return profile;
        }

        private static void ConfigureSummonPresentationCandidateProfile(
            SummonPresentationCandidateProfile profile,
            string candidateId,
            string displayName,
            SummonPresentationSide side,
            string actorPrefabPath,
            string roleCandidateProfilePath,
            string visualChildName,
            string sourceRoleId,
            CombatVfxCueProfile vfxCueProfile,
            string animationRead,
            string vfxRead,
            string replacementPlan,
            string visualSourceOverridePath = null)
        {
            GameObject actorPrefab = LoadAsset<GameObject>(actorPrefabPath);
            CombatEnemyRoleCandidateProfile roleCandidate =
                LoadAsset<CombatEnemyRoleCandidateProfile>(roleCandidateProfilePath);
            GameObject visualSourceAsset = !string.IsNullOrWhiteSpace(visualSourceOverridePath)
                ? LoadAsset<GameObject>(visualSourceOverridePath)
                : roleCandidate.PromotedVisualSource;
            RuntimeAnimatorController animatorController =
                ResolveActorVisualAnimatorController(actorPrefab, visualChildName);

            SetString(profile, "candidateId", candidateId);
            SetString(profile, "displayName", displayName);
            SetEnum(profile, "side", (int)side);
            SetObjectReference(profile, "actorPrefab", actorPrefab);
            SetObjectReference(profile, "visualSourceAsset", visualSourceAsset);
            SetString(profile, "visualChildName", visualChildName);
            SetString(profile, "sourceRoleId", sourceRoleId);
            SetObjectReference(profile, "animatorController", animatorController);
            SetObjectReference(profile, "vfxCueProfile", vfxCueProfile);
            SetString(profile, "animationRead", animationRead);
            SetString(profile, "vfxRead", vfxRead);
            SetString(profile, "replacementPlan", replacementPlan);
            SetString(
                profile,
                "ownershipNotes",
                "Presentation candidate only; runtime cost, tier, projectile, and screen values remain in gameplay profiles.");
        }

        private static RuntimeAnimatorController ResolveActorVisualAnimatorController(
            GameObject actorPrefab,
            string visualChildName)
        {
            Transform visual = actorPrefab.transform.Find(visualChildName);
            if (visual == null)
            {
                throw new InvalidOperationException($"{actorPrefab.name} is missing visual child {visualChildName}.");
            }

            Animator animator = visual.GetComponent<Animator>();
            if (animator == null || animator.runtimeAnimatorController == null)
            {
                throw new InvalidOperationException($"{visualChildName} is missing an Animator controller.");
            }

            return animator.runtimeAnimatorController;
        }

        private static GameObject EnsureSummonEntryCuePrefab()
        {
            EnsureFolderForAsset(SummonSlot1EntryCuePrefabPath);
            Material material = LoadOrCreateMaterial(SummonSlot1EntryCueMaterialPath, new Color(0.25f, 1f, 0.68f, 1f));
            Material accentMaterial = LoadOrCreateTransparentMaterial(
                SummonSlot1EntryCueAccentMaterialPath,
                new Color(0.18f, 1f, 0.78f, 0.62f));
            bool prefabExists = AssetDatabase.LoadAssetAtPath<GameObject>(SummonSlot1EntryCuePrefabPath) != null;
            GameObject editableRoot = prefabExists
                ? PrefabUtility.LoadPrefabContents(SummonSlot1EntryCuePrefabPath)
                : GameObject.CreatePrimitive(PrimitiveType.Cylinder);

            try
            {
                editableRoot.name = "PF_SummonSlot1EntryCue_MagicCircle";
                editableRoot.transform.localPosition = Vector3.zero;
                editableRoot.transform.localRotation = Quaternion.identity;
                editableRoot.transform.localScale = new Vector3(1f, 0.04f, 1f);

                MeshRenderer renderer = EnsureComponent<MeshRenderer>(editableRoot);
                renderer.sharedMaterial = material;
                renderer.enabled = false;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                renderer.allowOcclusionWhenDynamic = false;

                Collider collider = editableRoot.GetComponent<Collider>();
                if (collider != null && !(collider is SphereCollider))
                {
                    UnityEngine.Object.DestroyImmediate(collider);
                }

                ConfigureSummonEntryCueVfx(editableRoot, accentMaterial);
                PrefabUtility.SaveAsPrefabAsset(editableRoot, SummonSlot1EntryCuePrefabPath);
            }
            finally
            {
                if (prefabExists)
                {
                    PrefabUtility.UnloadPrefabContents(editableRoot);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(editableRoot);
                }
            }

            return LoadAsset<GameObject>(SummonSlot1EntryCuePrefabPath);
        }

        private static SummonFrontlineProxy EnsureSummonActorPrefab()
        {
            EnsureSummonSlot1PromotedChargeImpactPrefab();
            EnsureSummonSlot1PromotedRushTrailPrefab();
            EnsureFolderForAsset(SummonSlot1ActorPrefabPath);
            Material material = LoadOrCreateMaterial(SummonSlot1ActorMaterialPath, new Color(0.2f, 1f, 0.78f, 1f));
            Material pressureScreenMaterial = LoadOrCreateTransparentMaterial(
                SummonPressureScreenMaterialPath,
                new Color(0.18f, 1f, 0.78f, 0.16f));
            Material pulseMaterial = LoadOrCreateTransparentMaterial(
                SummonSlot1ActorPulseMaterialPath,
                new Color(0.45f, 0.95f, 1f, 0.72f));
            bool prefabExists = AssetDatabase.LoadAssetAtPath<GameObject>(SummonSlot1ActorPrefabPath) != null;
            GameObject editableRoot = prefabExists
                ? PrefabUtility.LoadPrefabContents(SummonSlot1ActorPrefabPath)
                : GameObject.CreatePrimitive(PrimitiveType.Capsule);

            try
            {
                editableRoot.name = "PF_SummonSlot1Actor_Proxy";
                editableRoot.transform.localPosition = Vector3.zero;
                editableRoot.transform.localRotation = Quaternion.identity;
                editableRoot.transform.localScale = Vector3.one;

                MeshRenderer renderer = EnsureComponent<MeshRenderer>(editableRoot);
                renderer.sharedMaterial = material;
                renderer.enabled = false;

                Collider collider = editableRoot.GetComponent<Collider>();
                if (collider != null && !(collider is SphereCollider))
                {
                    UnityEngine.Object.DestroyImmediate(collider);
                }

                SummonPressureScreen rootPressureScreen = editableRoot.GetComponent<SummonPressureScreen>();
                if (rootPressureScreen != null)
                {
                    UnityEngine.Object.DestroyImmediate(rootPressureScreen);
                }

                SummonFrontlineProxy proxy = EnsureComponent<SummonFrontlineProxy>(editableRoot);
                CombatHealth proxyHealth = EnsureComponent<CombatHealth>(editableRoot);
                SetEnum(proxyHealth, "team", (int)DamageTeam.AllySummon);
                SetFloat(proxyHealth, "maxHealth", 260f);
                SetBool(proxyHealth, "startAtFullHealth", true);
                SetObjectReference(proxy, "health", proxyHealth);
                SetBool(proxy, "resetHealthOnActivate", true);
                SummonFrontlineClash clash = EnsureComponent<SummonFrontlineClash>(editableRoot);
                clash.ConfigureReferences(proxy, proxyHealth);
                clash.ConfigureTuning(34f, 0.35f, 0.16f, 0.24f);

                SphereCollider bodyCollider = EnsureComponent<SphereCollider>(editableRoot);
                bodyCollider.isTrigger = true;
                bodyCollider.center = new Vector3(0f, 0.9f, 0f);
                bodyCollider.radius = 0.85f;

                Rigidbody bodyRigidbody = EnsureComponent<Rigidbody>(editableRoot);
                bodyRigidbody.useGravity = false;
                bodyRigidbody.isKinematic = true;

                Transform projectileOrigin = EnsureChild(editableRoot.transform, "ProjectileOrigin");
                projectileOrigin.localPosition = new Vector3(0f, 0.85f, 0.35f);
                projectileOrigin.localRotation = Quaternion.identity;
                projectileOrigin.localScale = Vector3.one;
                SetObjectReference(proxy, "projectileOrigin", projectileOrigin);

                Transform pressureScreenRoot = EnsureChild(editableRoot.transform, "PressureScreen");
                pressureScreenRoot.localPosition = new Vector3(0f, 0.72f, 0.2f);
                pressureScreenRoot.localRotation = Quaternion.identity;
                pressureScreenRoot.localScale = Vector3.one;
                SummonPressureScreen pressureScreen = EnsureComponent<SummonPressureScreen>(pressureScreenRoot.gameObject);
                SphereCollider screenCollider = EnsureComponent<SphereCollider>(pressureScreenRoot.gameObject);
                screenCollider.isTrigger = true;
                screenCollider.center = Vector3.zero;
                screenCollider.radius = 1.35f;

                Rigidbody screenRigidbody = EnsureComponent<Rigidbody>(pressureScreenRoot.gameObject);
                screenRigidbody.useGravity = false;
                screenRigidbody.isKinematic = true;

                SetEnum(pressureScreen, "ownerTeam", (int)DamageTeam.AllySummon);
                SetInt(pressureScreen, "defaultMaxIntercepts", 2);
                SetFloat(pressureScreen, "defaultLifetimeSeconds", 1.2f);
                SetFloat(pressureScreen, "defaultRadius", 1.35f);
                SetObjectReference(proxy, "pressureScreen", pressureScreen);

                Transform pressureScreenVisual = EnsureChild(editableRoot.transform, "PressureScreenVisual");
                pressureScreenVisual.localPosition = new Vector3(0f, 0.72f, 0.2f);
                pressureScreenVisual.localRotation = Quaternion.identity;
                pressureScreenVisual.localScale = Vector3.one;
                MeshFilter visualFilter = EnsureComponent<MeshFilter>(pressureScreenVisual.gameObject);
                visualFilter.sharedMesh = LoadPrimitiveMesh(PrimitiveType.Sphere);
                MeshRenderer visualRenderer = EnsureComponent<MeshRenderer>(pressureScreenVisual.gameObject);
                visualRenderer.sharedMaterial = pressureScreenMaterial;
                visualRenderer.enabled = false;
                visualRenderer.shadowCastingMode = ShadowCastingMode.Off;
                visualRenderer.receiveShadows = false;
                visualRenderer.allowOcclusionWhenDynamic = false;
                pressureScreenVisual.gameObject.SetActive(false);
                Collider visualCollider = pressureScreenVisual.GetComponent<Collider>();
                if (visualCollider != null)
                {
                    UnityEngine.Object.DestroyImmediate(visualCollider);
                }

                SummonPressureScreenPresenter presenter = EnsureComponent<SummonPressureScreenPresenter>(editableRoot);
                SetObjectReference(presenter, "pressureScreen", pressureScreen);
                SetObjectReference(presenter, "visualRoot", pressureScreenVisual);
                SetObjectReferenceArray(presenter, "screenRenderers", new UnityEngine.Object[] { visualRenderer });
                SetBool(presenter, "renderVisuals", false);
                SetColor(presenter, "activeColor", new Color(0.22f, 1f, 0.82f, 0.09f));
                SetColor(presenter, "tierTwoColor", new Color(0.38f, 0.74f, 1f, 0.10f));
                SetColor(presenter, "tierThreeColor", new Color(1f, 0.76f, 0.24f, 0.12f));
                SetColor(presenter, "interceptColor", new Color(0.92f, 1f, 1f, 0.16f));
                SetFloat(presenter, "visualRadiusScale", 0.18f);
                SetFloat(presenter, "activationCueIntensity", 0.48f);
                SetFloat(presenter, "interceptCueIntensity", 0.58f);
                SetFloat(presenter, "tierCueIntensityStep", 0.08f);
                SetFloat(presenter, "activationFlashSeconds", 0.08f);
                SetFloat(presenter, "interceptFlashSeconds", 0.12f);
                SetFloat(presenter, "finalHitLingerSeconds", 0.1f);
                SetFloat(presenter, "pulseSpeed", 9f);
                SetFloat(presenter, "pulseScale", 0.025f);

                Transform tierPulseCore = EnsureChild(editableRoot.transform, "TierPulseCore");
                tierPulseCore.localPosition = new Vector3(0f, 1.08f, 0.08f);
                tierPulseCore.localRotation = Quaternion.identity;
                tierPulseCore.localScale = Vector3.one * 0.32f;
                MeshFilter pulseFilter = EnsureComponent<MeshFilter>(tierPulseCore.gameObject);
                pulseFilter.sharedMesh = LoadPrimitiveMesh(PrimitiveType.Sphere);
                MeshRenderer pulseRenderer = EnsureComponent<MeshRenderer>(tierPulseCore.gameObject);
                pulseRenderer.sharedMaterial = pulseMaterial;
                pulseRenderer.enabled = false;
                pulseRenderer.shadowCastingMode = ShadowCastingMode.Off;
                pulseRenderer.receiveShadows = false;
                pulseRenderer.allowOcclusionWhenDynamic = false;
                tierPulseCore.gameObject.SetActive(false);
                Collider pulseCollider = tierPulseCore.GetComponent<Collider>();
                if (pulseCollider != null)
                {
                    UnityEngine.Object.DestroyImmediate(pulseCollider);
                }

                ConfigureSummonShieldVfx(
                    editableRoot,
                    pressureScreenMaterial,
                    pressureScreenVisual.localPosition,
                    forwardSign: 1f,
                    radius: 1.35f);
                ConfigureSummonPulseVfx(
                    editableRoot,
                    "TierPulseCore",
                    pulseMaterial,
                    forwardSign: 1f,
                    radius: 0.48f);
                ConfigureSummonStateVfx(
                    editableRoot,
                    "ChargeReadyAura",
                    ImportedMagicMissilesHealingAuraPrefabPath,
                    new Vector3(0f, 0.1f, 0.02f),
                    Vector3.one * 0.52f);
                DestroyDescendantsIfPresent(
                    editableRoot.transform,
                    "JumpSlamReadyAura",
                    "SlamImpactBurst",
                    "PF_SummonJumpSlamImpact_SPECIAL",
                    "JumpSlamAirTrail",
                    "PF_SummonJumpSlamAirTrail_SPECIAL");

                Transform summonVisual = AttachRoleVisualOnly(
                    editableRoot.transform,
                    SummonSlot1ActorVisualRoleId,
                    ActionFoundationEnemyRoleCandidateSetup.ShieldBreakerElitePrefabPath,
                    SummonSlot1ActorVisualName,
                    new Vector3(0f, -0.04f, -0.08f),
                    Vector3.zero,
                    new Vector3(0.9f, 0.9f, 0.9f));
                DestroyDescendantsIfPresent(
                    summonVisual,
                    "JumpSlamAirTrail",
                    "PF_SummonJumpSlamAirTrail_SPECIAL");
                Transform chargeTrailVfx = ConfigureSummonMovementPromotedParticleVfx(
                    summonVisual,
                    "ChargeRushTrail",
                    SummonSlot1PromotedRushTrailPrefabPath,
                    new Vector3(0f, 0.36f, -0.5f),
                    Vector3.zero,
                    new Vector3(0.7f, 0.7f, 0.7f),
                    minimumParticleSystems: 2);
                EnsureSummonHealthBar(
                    editableRoot,
                    proxy,
                    proxyHealth,
                    DamageTeam.AllySummon,
                    new Vector3(0f, 1.78f, 0.02f),
                    0.78f);

                SummonFrontlineProxyPresenter actorPresenter =
                    EnsureComponent<SummonFrontlineProxyPresenter>(editableRoot);
                SetObjectReference(actorPresenter, "proxy", proxy);
                SetObjectReference(actorPresenter, "clash", clash);
                SetObjectReference(actorPresenter, "pulseRoot", tierPulseCore);
                SetObjectReferenceArray(
                    actorPresenter,
                    "actorRenderers",
                    BuildPulseRendererReferenceArray(pulseRenderer));
                SetObjectReferenceArray(
                    actorPresenter,
                    "damageFlashRenderers",
                    ToObjectArray(CollectEnabledRenderers(summonVisual.gameObject)));
                Transform damageVfxAnchor = EnsureSummonDamageVfxAnchor(editableRoot, summonVisual);
                SetObjectReference(actorPresenter, "damageVfxAnchor", damageVfxAnchor);
                SetBool(actorPresenter, "renderPulseVisuals", false);
                SetColor(actorPresenter, "tierOneColor", new Color(0.24f, 1f, 0.78f, 0.78f));
                SetColor(actorPresenter, "tierTwoColor", new Color(0.38f, 0.74f, 1f, 0.9f));
                SetColor(actorPresenter, "tierThreeColor", new Color(1f, 0.76f, 0.24f, 1f));
                SetColor(actorPresenter, "flashColor", Color.white);
                SetColor(actorPresenter, "clashFlashColor", new Color(1f, 0.92f, 0.35f, 1f));
                SetFloat(actorPresenter, "entryFlashSeconds", 0.22f);
                SetFloat(actorPresenter, "impactFlashSeconds", 0.18f);
                SetFloat(actorPresenter, "clashFlashSeconds", 0.14f);
                SetFloat(actorPresenter, "impactFlashProgress", 0.86f);
                SetFloat(actorPresenter, "pulseSpeed", 8f);
                SetFloat(actorPresenter, "pulseScale", 0.08f);
                SetFloat(actorPresenter, "tierScaleStep", 0.18f);
                SetFloat(actorPresenter, "flashScale", 0.22f);
                SetFloat(actorPresenter, "clashFlashScale", 0.16f);
                SetFloat(actorPresenter, "pressureDamageCueScale", 0.64f);
                ConfigureSummonActorAnimatorPresentation(
                    actorPresenter,
                    summonVisual,
                    animatorMoveSpeedScale: 0.42f);
                ConfigureSummonProxyVisualMotion(
                    editableRoot,
                    proxy,
                    summonVisual,
                    airborneHeight: 0f,
                    jumpArcHeight: 0f,
                    tierArcHeightStep: 0f,
                    landingDip: 0f,
                    arcEndProgress: 0.9f,
                    landingSettleSeconds: 0f,
                    movementVfxRoot: chargeTrailVfx);
                ConfigureSummonAttackPromotedParticleBeam(
                    editableRoot,
                    proxy,
                    "ChargeImpactBurst",
                    SummonSlot1PromotedChargeImpactPrefabPath,
                    new Vector3(0f, 0.42f, 0.86f),
                    Vector3.zero,
                    new Vector3(0.74f, 0.74f, 0.74f),
                    new Color(1f, 0.78f, 0.28f, 0.62f),
                    new Color(1f, 0.92f, 0.42f, 0.72f),
                    new Color(1f, 0.55f, 0.18f, 0.82f),
                    tierScaleStep: 0.34f,
                    pulseScale: 0.1f,
                    pulseSpeed: 14f,
                    minimumParticleSystems: 3,
                    loopParticles: false);

                PrefabUtility.SaveAsPrefabAsset(editableRoot, SummonSlot1ActorPrefabPath);
            }
            finally
            {
                if (prefabExists)
                {
                    PrefabUtility.UnloadPrefabContents(editableRoot);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(editableRoot);
                }
            }

            return LoadPrefabComponent<SummonFrontlineProxy>(SummonSlot1ActorPrefabPath);
        }

        private static SummonFrontlineProxy EnsureSummonSlot2ActorPrefab()
        {
            EnsureSummonSlot2PromotedLaserBeamPrefab();
            return EnsureSupportSummonActorPrefab(
                SummonSlot2ActorPrefabPath,
                "PF_SummonSlot2Actor_LaserSoldierProxy",
                SummonSlot2ActorMaterialPath,
                SummonSlot2ActorPulseMaterialPath,
                SummonSlot2ActorVisualRoleId,
                ActionFoundationEnemyRoleCandidateSetup.LineCasterPrefabPath,
                SummonSlot2ActorVisualName,
                new Vector3(-0.06f, -0.04f, -0.08f),
                new Vector3(0f, -8f, 0f),
                new Vector3(0.64f, 0.64f, 0.64f),
                new Color(0.34f, 0.94f, 1f, 0.82f),
                170f,
                0.78f,
                18f,
                0.1f,
                0.18f,
                includePressureScreen: false,
                pressureScreenMaterialPath: null,
                pressureScreenColor: Color.clear);
        }

        private static SummonFrontlineProxy EnsureBossLaserSummonActorPrefab()
        {
            EnsureSummonSlot2PromotedLaserBeamPrefab();
            SummonFrontlineProxy proxy = EnsureSupportSummonActorPrefab(
                BossLaserSummonActorPrefabPath,
                "PF_BossLaserSummonActor_Proxy",
                SummonSlot2ActorMaterialPath,
                SummonSlot2ActorPulseMaterialPath,
                SummonSlot2ActorVisualRoleId,
                ActionFoundationEnemyRoleCandidateSetup.LineCasterPrefabPath,
                SummonSlot2ActorVisualName,
                new Vector3(-0.06f, -0.04f, -0.08f),
                new Vector3(0f, -8f, 0f),
                new Vector3(0.64f, 0.64f, 0.64f),
                new Color(0.34f, 0.94f, 1f, 0.82f),
                760f,
                0.78f,
                0f,
                0f,
                0.12f,
                includePressureScreen: false,
                pressureScreenMaterialPath: null,
                pressureScreenColor: Color.clear,
                ownerTeam: DamageTeam.Enemy);

            GameObject editableRoot = PrefabUtility.LoadPrefabContents(BossLaserSummonActorPrefabPath);
            try
            {
                SummonFrontlineProxy editableProxy = EnsureComponent<SummonFrontlineProxy>(editableRoot);
                SetFloat(editableProxy, "advanceAcceleration", 9.5f);
                SetFloat(editableProxy, "advanceDeceleration", 12f);
                SetFloat(editableProxy, "advanceSlowdownDistance", 1.7f);
                SetFloat(editableProxy, "minimumAdvanceSpeedScale", 0.38f);
                SetFloat(editableProxy, "facingTurnSpeedDegrees", 540f);
                SetFloat(editableProxy, "turnAlignmentSpeedFloor", 0.34f);

                BossLaserSummonPattern laserPattern = EnsureComponent<BossLaserSummonPattern>(editableRoot);
                SetObjectReference(laserPattern, "proxy", editableProxy);
                SetObjectReference(laserPattern, "sourceHealth", editableProxy.Health);
                SetFloat(laserPattern, "telegraphSeconds", 0.78f);
                SetFloat(laserPattern, "aimLockSeconds", 0.2f);
                SetFloat(laserPattern, "activeSeconds", 0.92f);
                SetFloat(laserPattern, "recoverySeconds", 0.42f);
                SetFloat(laserPattern, "repositionSeconds", 0.62f);
                SetFloat(laserPattern, "laserLength", 22f);
                SetFloat(laserPattern, "hitRadius", 0.62f);
                SetFloat(laserPattern, "damagePerSecond", 58f);
                SetFloat(laserPattern, "damageIntervalSeconds", 0.12f);
                SetFloat(laserPattern, "desiredDistanceFromTarget", 4.2f);
                SetFloat(laserPattern, "strafeDistance", 1.45f);
                SetFloat(laserPattern, "repositionMoveSpeed", 4f);

                PrefabUtility.SaveAsPrefabAsset(editableRoot, BossLaserSummonActorPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(editableRoot);
            }

            return proxy;
        }

        private static SummonFrontlineProxy EnsureSummonSlot3ActorPrefab()
        {
            EnsureSummonSlot3PromotedFireBreathPrefab();
            EnsureSummonSlot3PromotedDragonVisualPrefab();
            return EnsureSupportSummonActorPrefab(
                SummonSlot3ActorPrefabPath,
                "PF_SummonSlot3Actor_FireDragonProxy",
                SummonSlot3ActorMaterialPath,
                SummonSlot3ActorPulseMaterialPath,
                SummonSlot3ActorVisualRoleId,
                ActionFoundationEnemyRoleCandidateSetup.FinalStandCommanderElitePrefabPath,
                SummonSlot3ActorVisualName,
                new Vector3(0f, 0.24f, -0.08f),
                Vector3.zero,
                Vector3.one * 0.18f,
                new Color(1f, 0.44f, 0.16f, 0.88f),
                520f,
                1.36f,
                46f,
                0.1f,
                0.34f,
                includePressureScreen: false,
                pressureScreenMaterialPath: null,
                pressureScreenColor: Color.clear);
        }

        private static SummonFrontlineProxy EnsureSupportSummonActorPrefab(
            string prefabPath,
            string prefabName,
            string materialPath,
            string pulseMaterialPath,
            string roleId,
            string rolePrefabPath,
            string visualName,
            Vector3 visualLocalPosition,
            Vector3 visualLocalEulerAngles,
            Vector3 visualLocalScale,
            Color pulseColor,
            float maxHealth,
            float bodyRadius,
            float clashDamagePerSecond,
            float clashTierDamageBonus,
            float clashHoldSeconds,
            bool includePressureScreen,
            string pressureScreenMaterialPath,
            Color pressureScreenColor,
            DamageTeam ownerTeam = DamageTeam.AllySummon)
        {
            EnsureFolderForAsset(prefabPath);
            Material material = LoadOrCreateMaterial(materialPath, pulseColor);
            Material pulseMaterial = LoadOrCreateTransparentMaterial(pulseMaterialPath, pulseColor);
            Material pressureScreenMaterial = includePressureScreen
                ? LoadOrCreateTransparentMaterial(pressureScreenMaterialPath, pressureScreenColor)
                : null;
            bool prefabExists = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null;
            GameObject editableRoot = prefabExists
                ? PrefabUtility.LoadPrefabContents(prefabPath)
                : GameObject.CreatePrimitive(PrimitiveType.Capsule);

            try
            {
                editableRoot.name = prefabName;
                editableRoot.transform.localPosition = Vector3.zero;
                editableRoot.transform.localRotation = Quaternion.identity;
                editableRoot.transform.localScale = Vector3.one;

                MeshRenderer renderer = EnsureComponent<MeshRenderer>(editableRoot);
                renderer.sharedMaterial = material;
                renderer.enabled = false;

                Collider collider = editableRoot.GetComponent<Collider>();
                if (collider != null)
                {
                    UnityEngine.Object.DestroyImmediate(collider);
                }

                SummonFrontlineProxy proxy = EnsureComponent<SummonFrontlineProxy>(editableRoot);
                CombatHealth proxyHealth = EnsureComponent<CombatHealth>(editableRoot);
                SetEnum(proxyHealth, "team", (int)ownerTeam);
                SetFloat(proxyHealth, "maxHealth", maxHealth);
                SetBool(proxyHealth, "startAtFullHealth", true);
                SetObjectReference(proxy, "health", proxyHealth);
                SetBool(proxy, "resetHealthOnActivate", true);
                SummonFrontlineClash clash = EnsureComponent<SummonFrontlineClash>(editableRoot);
                clash.ConfigureReferences(proxy, proxyHealth);
                clash.ConfigureTuning(clashDamagePerSecond, 0.35f, clashTierDamageBonus, clashHoldSeconds);

                SphereCollider bodyCollider = EnsureComponent<SphereCollider>(editableRoot);
                bodyCollider.isTrigger = true;
                bodyCollider.center = new Vector3(0f, 0.9f, 0f);
                bodyCollider.radius = bodyRadius;

                Rigidbody bodyRigidbody = EnsureComponent<Rigidbody>(editableRoot);
                bodyRigidbody.useGravity = false;
                bodyRigidbody.isKinematic = true;

                Transform projectileOrigin = EnsureChild(editableRoot.transform, "ProjectileOrigin");
                projectileOrigin.localPosition = roleId == SummonSlot3ActorVisualRoleId
                    ? new Vector3(0f, 0.58f, 0.28f)
                    : new Vector3(0f, 0.92f, 0.28f);
                projectileOrigin.localRotation = Quaternion.identity;
                projectileOrigin.localScale = Vector3.one;
                SetObjectReference(proxy, "projectileOrigin", projectileOrigin);
                SummonPressureScreen pressureScreen = includePressureScreen
                    ? EnsureSupportPressureScreen(
                        editableRoot,
                        pressureScreenMaterial,
                        pressureScreenColor,
                        bodyRadius)
                    : null;
                if (!includePressureScreen)
                {
                    RemoveSupportPressureScreen(editableRoot);
                    RemoveChildrenWithPrefix(editableRoot.transform, "SummonShieldVfx_");
                }

                SetObjectReference(proxy, "pressureScreen", pressureScreen);

                Transform tierPulseCore = EnsureChild(editableRoot.transform, "TierPulseCore");
                tierPulseCore.localPosition = new Vector3(0f, 1.12f, 0.1f);
                tierPulseCore.localRotation = Quaternion.identity;
                tierPulseCore.localScale = Vector3.one * 0.3f;
                MeshFilter pulseFilter = EnsureComponent<MeshFilter>(tierPulseCore.gameObject);
                pulseFilter.sharedMesh = LoadPrimitiveMesh(PrimitiveType.Sphere);
                MeshRenderer pulseRenderer = EnsureComponent<MeshRenderer>(tierPulseCore.gameObject);
                pulseRenderer.sharedMaterial = pulseMaterial;
                pulseRenderer.enabled = false;
                pulseRenderer.shadowCastingMode = ShadowCastingMode.Off;
                pulseRenderer.receiveShadows = false;
                pulseRenderer.allowOcclusionWhenDynamic = false;
                tierPulseCore.gameObject.SetActive(false);
                Collider pulseCollider = tierPulseCore.GetComponent<Collider>();
                if (pulseCollider != null)
                {
                    UnityEngine.Object.DestroyImmediate(pulseCollider);
                }

                if (includePressureScreen)
                {
                    ConfigureSummonShieldVfx(
                        editableRoot,
                        pressureScreenMaterial,
                        new Vector3(0f, 0.82f, 0.2f),
                        forwardSign: 1f,
                        radius: Mathf.Max(1.2f, bodyRadius * 1.18f));
                }

                ConfigureSummonPulseVfx(
                    editableRoot,
                    "TierPulseCore",
                    pulseMaterial,
                    forwardSign: 1f,
                    radius: Mathf.Max(0.42f, bodyRadius * 0.42f));
                ConfigureSummonStateVfx(
                    editableRoot,
                    roleId == SummonSlot2ActorVisualRoleId
                        ? "LaserFocusAura"
                        : "DragonFireAura",
                    roleId == SummonSlot2ActorVisualRoleId
                        ? ImportedMagicMissilesArcaneAuraPrefabPath
                        : ImportedMagicMissilesHealingAuraPrefabPath,
                    new Vector3(0f, 0.1f, 0.02f),
                    Vector3.one * (roleId == SummonSlot2ActorVisualRoleId ? 0.56f : 0.78f));

                Transform summonVisual = roleId == SummonSlot3ActorVisualRoleId
                    ? AttachSummonDragonVisual(
                        editableRoot.transform,
                        visualName,
                        visualLocalPosition,
                        visualLocalEulerAngles,
                        visualLocalScale)
                    : AttachRoleVisualOnly(
                        editableRoot.transform,
                        roleId,
                        rolePrefabPath,
                        visualName,
                        visualLocalPosition,
                        visualLocalEulerAngles,
                        visualLocalScale);
                EnsureSummonHealthBar(
                    editableRoot,
                    proxy,
                    proxyHealth,
                    ownerTeam,
                    roleId == SummonSlot3ActorVisualRoleId
                        ? new Vector3(0f, 1.18f, 0.02f)
                        : new Vector3(0f, 1.78f, 0.02f),
                    Mathf.Max(0.68f, bodyRadius * 0.95f));

                SummonFrontlineProxyPresenter actorPresenter =
                    EnsureComponent<SummonFrontlineProxyPresenter>(editableRoot);
                SetObjectReference(actorPresenter, "proxy", proxy);
                SetObjectReference(actorPresenter, "clash", clash);
                SetObjectReference(actorPresenter, "pulseRoot", tierPulseCore);
                SetObjectReferenceArray(
                    actorPresenter,
                    "actorRenderers",
                    BuildPulseRendererReferenceArray(pulseRenderer));
                SetObjectReferenceArray(
                    actorPresenter,
                    "damageFlashRenderers",
                    ToObjectArray(CollectEnabledRenderers(summonVisual.gameObject)));
                Transform damageVfxAnchor = EnsureSummonDamageVfxAnchor(editableRoot, summonVisual);
                SetObjectReference(actorPresenter, "damageVfxAnchor", damageVfxAnchor);
                SetBool(actorPresenter, "renderPulseVisuals", false);
                SetColor(actorPresenter, "tierOneColor", pulseColor);
                SetColor(actorPresenter, "tierTwoColor", Color.Lerp(pulseColor, Color.white, 0.25f));
                SetColor(actorPresenter, "tierThreeColor", Color.Lerp(pulseColor, new Color(1f, 0.78f, 0.22f, 1f), 0.45f));
                SetColor(actorPresenter, "flashColor", Color.white);
                SetColor(actorPresenter, "clashFlashColor", new Color(1f, 0.9f, 0.34f, 1f));
                SetFloat(actorPresenter, "entryFlashSeconds", 0.2f);
                SetFloat(actorPresenter, "impactFlashSeconds", 0.18f);
                SetFloat(actorPresenter, "clashFlashSeconds", 0.14f);
                SetFloat(actorPresenter, "impactFlashProgress", 0.86f);
                SetFloat(actorPresenter, "pulseSpeed", 7.8f);
                SetFloat(actorPresenter, "pulseScale", 0.07f);
                SetFloat(actorPresenter, "tierScaleStep", 0.16f);
                SetFloat(actorPresenter, "flashScale", 0.18f);
                SetFloat(actorPresenter, "clashFlashScale", 0.14f);
                SetFloat(actorPresenter, "pressureDamageCueScale", 0.64f);
                ConfigureSummonActorAnimatorPresentation(
                    actorPresenter,
                    summonVisual,
                    animatorMoveSpeedScale: 0.46f);
                if (roleId == SummonSlot2ActorVisualRoleId)
                {
                    ConfigureSummonAttackPromotedParticleBeam(
                        editableRoot,
                        proxy,
                        "LaserMuzzleBeam",
                        SummonSlot2PromotedLaserBeamPrefabPath,
                        new Vector3(0f, 1.08f, 1.32f),
                        Vector3.zero,
                        new Vector3(0.92f, 0.92f, 1.38f),
                        new Color(0.18f, 0.92f, 1f, 0.72f),
                        new Color(0.62f, 0.86f, 1f, 0.82f),
                        new Color(1f, 0.86f, 0.34f, 0.9f),
                        tierScaleStep: 0.18f,
                        pulseScale: 0.1f,
                        pulseSpeed: 22f,
                        minimumParticleSystems: 4);
                }
                else if (roleId == SummonSlot3ActorVisualRoleId)
                {
                    ConfigureSummonProxyVisualMotion(
                        editableRoot,
                        proxy,
                        summonVisual,
                        airborneHeight: 0.34f,
                        jumpArcHeight: 0f,
                        tierArcHeightStep: 0f,
                        landingDip: 0f,
                        arcEndProgress: 0.9f,
                        landingSettleSeconds: 0f);
                    ConfigureSummonAttackPromotedParticleBeam(
                        editableRoot,
                        proxy,
                        "DragonFireBreathBeam",
                        SummonSlot3PromotedFireBreathPrefabPath,
                        new Vector3(0f, 0.88f, 1.58f),
                        Vector3.zero,
                        new Vector3(1.18f, 1.18f, 2.3f),
                        new Color(1f, 0.38f, 0.08f, 0.72f),
                        new Color(1f, 0.62f, 0.14f, 0.82f),
                        new Color(1f, 0.86f, 0.32f, 0.9f),
                        tierScaleStep: 0.28f,
                        pulseScale: 0.14f,
                        pulseSpeed: 16f,
                        minimumParticleSystems: 2);
                }

                PrefabUtility.SaveAsPrefabAsset(editableRoot, prefabPath);
            }
            finally
            {
                if (prefabExists)
                {
                    PrefabUtility.UnloadPrefabContents(editableRoot);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(editableRoot);
                }
            }

            return LoadPrefabComponent<SummonFrontlineProxy>(prefabPath);
        }

        private static SummonPressureScreen EnsureSupportPressureScreen(
            GameObject editableRoot,
            Material pressureScreenMaterial,
            Color pressureScreenColor,
            float bodyRadius)
        {
            Transform pressureScreenRoot = EnsureChild(editableRoot.transform, "PressureScreen");
            pressureScreenRoot.localPosition = new Vector3(0f, 0.82f, 0.2f);
            pressureScreenRoot.localRotation = Quaternion.identity;
            pressureScreenRoot.localScale = Vector3.one;
            SummonPressureScreen pressureScreen = EnsureComponent<SummonPressureScreen>(pressureScreenRoot.gameObject);
            SphereCollider screenCollider = EnsureComponent<SphereCollider>(pressureScreenRoot.gameObject);
            screenCollider.isTrigger = true;
            screenCollider.center = Vector3.zero;
            screenCollider.radius = Mathf.Max(1.2f, bodyRadius * 1.18f);

            Rigidbody screenRigidbody = EnsureComponent<Rigidbody>(pressureScreenRoot.gameObject);
            screenRigidbody.useGravity = false;
            screenRigidbody.isKinematic = true;

            SetEnum(pressureScreen, "ownerTeam", (int)DamageTeam.AllySummon);
            SetInt(pressureScreen, "defaultMaxIntercepts", 2);
            SetFloat(pressureScreen, "defaultLifetimeSeconds", 2.2f);
            SetFloat(pressureScreen, "defaultRadius", screenCollider.radius);

            Transform pressureScreenVisual = EnsureChild(editableRoot.transform, "PressureScreenVisual");
            pressureScreenVisual.localPosition = pressureScreenRoot.localPosition;
            pressureScreenVisual.localRotation = Quaternion.identity;
            pressureScreenVisual.localScale = Vector3.one;
            MeshFilter visualFilter = EnsureComponent<MeshFilter>(pressureScreenVisual.gameObject);
            visualFilter.sharedMesh = LoadPrimitiveMesh(PrimitiveType.Sphere);
            MeshRenderer visualRenderer = EnsureComponent<MeshRenderer>(pressureScreenVisual.gameObject);
            visualRenderer.sharedMaterial = pressureScreenMaterial;
            visualRenderer.enabled = false;
            visualRenderer.shadowCastingMode = ShadowCastingMode.Off;
            visualRenderer.receiveShadows = false;
            visualRenderer.allowOcclusionWhenDynamic = false;
            pressureScreenVisual.gameObject.SetActive(false);
            Collider visualCollider = pressureScreenVisual.GetComponent<Collider>();
            if (visualCollider != null)
            {
                UnityEngine.Object.DestroyImmediate(visualCollider);
            }

            SummonPressureScreenPresenter screenPresenter =
                EnsureComponent<SummonPressureScreenPresenter>(editableRoot);
            SetObjectReference(screenPresenter, "pressureScreen", pressureScreen);
            SetObjectReference(screenPresenter, "visualRoot", pressureScreenVisual);
            SetObjectReferenceArray(screenPresenter, "screenRenderers", new UnityEngine.Object[] { visualRenderer });
            SetBool(screenPresenter, "renderVisuals", false);
            SetColor(screenPresenter, "activeColor", pressureScreenColor);
            SetColor(screenPresenter, "tierTwoColor", new Color(1f, 0.805f, 0.49f, 0.10f));
            SetColor(screenPresenter, "tierThreeColor", new Color(1f, 0.758f, 0.275f, 0.12f));
            SetColor(screenPresenter, "interceptColor", new Color(1f, 0.96f, 0.78f, 0.16f));
            SetFloat(screenPresenter, "visualRadiusScale", 0.18f);
            SetFloat(screenPresenter, "activationCueIntensity", 0.48f);
            SetFloat(screenPresenter, "interceptCueIntensity", 0.58f);
            SetFloat(screenPresenter, "tierCueIntensityStep", 0.08f);
            SetFloat(screenPresenter, "activationFlashSeconds", 0.08f);
            SetFloat(screenPresenter, "interceptFlashSeconds", 0.12f);
            SetFloat(screenPresenter, "finalHitLingerSeconds", 0.1f);
            SetFloat(screenPresenter, "pulseSpeed", 7.8f);
            SetFloat(screenPresenter, "pulseScale", 0.024f);

            return pressureScreen;
        }

        private static void RemoveSupportPressureScreen(GameObject editableRoot)
        {
            DestroyChildIfPresent(editableRoot.transform, "PressureScreen");
            DestroyChildIfPresent(editableRoot.transform, "PressureScreenVisual");
            SummonPressureScreenPresenter screenPresenter =
                editableRoot.GetComponent<SummonPressureScreenPresenter>();
            if (screenPresenter != null)
            {
                UnityEngine.Object.DestroyImmediate(screenPresenter);
            }
        }

        private static SummonFrontlineProxy EnsureBossSummonPressureActorPrefab()
        {
            EnsureFolderForAsset(BossSummonPressureActorPrefabPath);
            Material material = LoadOrCreateMaterial(BossSummonPressureActorMaterialPath, new Color(1f, 0.36f, 0.64f, 1f));
            Material pressureScreenMaterial = LoadOrCreateTransparentMaterial(
                BossSummonPressureScreenMaterialPath,
                new Color(1f, 0.22f, 0.55f, 0.16f));
            Material pulseMaterial = LoadOrCreateTransparentMaterial(
                BossSummonPressureActorPulseMaterialPath,
                new Color(1f, 0.62f, 0.28f, 0.74f));
            bool prefabExists = AssetDatabase.LoadAssetAtPath<GameObject>(BossSummonPressureActorPrefabPath) != null;
            GameObject editableRoot = prefabExists
                ? PrefabUtility.LoadPrefabContents(BossSummonPressureActorPrefabPath)
                : GameObject.CreatePrimitive(PrimitiveType.Capsule);

            try
            {
                editableRoot.name = "PF_BossSummonPressureActor_Proxy";
                editableRoot.transform.localPosition = Vector3.zero;
                editableRoot.transform.localRotation = Quaternion.identity;
                editableRoot.transform.localScale = Vector3.one;

                MeshRenderer renderer = EnsureComponent<MeshRenderer>(editableRoot);
                renderer.sharedMaterial = material;
                renderer.enabled = false;

                Collider collider = editableRoot.GetComponent<Collider>();
                if (collider != null && !(collider is SphereCollider))
                {
                    UnityEngine.Object.DestroyImmediate(collider);
                }

                SummonPressureScreen rootPressureScreen = editableRoot.GetComponent<SummonPressureScreen>();
                if (rootPressureScreen != null)
                {
                    UnityEngine.Object.DestroyImmediate(rootPressureScreen);
                }

                SummonFrontlineProxy proxy = EnsureComponent<SummonFrontlineProxy>(editableRoot);
                CombatHealth proxyHealth = EnsureComponent<CombatHealth>(editableRoot);
                SetEnum(proxyHealth, "team", (int)DamageTeam.Enemy);
                SetFloat(proxyHealth, "maxHealth", 320f);
                SetBool(proxyHealth, "startAtFullHealth", true);
                SetObjectReference(proxy, "health", proxyHealth);
                SetBool(proxy, "resetHealthOnActivate", true);
                SummonFrontlineClash clash = EnsureComponent<SummonFrontlineClash>(editableRoot);
                clash.ConfigureReferences(proxy, proxyHealth);
                clash.ConfigureTuning(36f, 0.35f, 0.16f, 0.24f);
                clash.ConfigurePlayerBodyDamage(0.32f, 7.5f);

                SphereCollider bodyCollider = EnsureComponent<SphereCollider>(editableRoot);
                bodyCollider.isTrigger = true;
                bodyCollider.center = new Vector3(0f, 0.9f, 0f);
                bodyCollider.radius = 0.9f;

                Rigidbody bodyRigidbody = EnsureComponent<Rigidbody>(editableRoot);
                bodyRigidbody.useGravity = false;
                bodyRigidbody.isKinematic = true;

                Transform projectileOrigin = EnsureChild(editableRoot.transform, "PressureOrigin");
                projectileOrigin.localPosition = new Vector3(0f, 0.92f, -0.28f);
                projectileOrigin.localRotation = Quaternion.identity;
                projectileOrigin.localScale = Vector3.one;
                SetObjectReference(proxy, "projectileOrigin", projectileOrigin);

                Transform pressureScreenRoot = EnsureChild(editableRoot.transform, "PressureScreen");
                pressureScreenRoot.localPosition = new Vector3(0f, 0.72f, -0.12f);
                pressureScreenRoot.localRotation = Quaternion.identity;
                pressureScreenRoot.localScale = Vector3.one;
                SummonPressureScreen pressureScreen = EnsureComponent<SummonPressureScreen>(pressureScreenRoot.gameObject);
                SphereCollider screenCollider = EnsureComponent<SphereCollider>(pressureScreenRoot.gameObject);
                screenCollider.isTrigger = true;
                screenCollider.center = Vector3.zero;
                screenCollider.radius = 1.45f;

                Rigidbody screenRigidbody = EnsureComponent<Rigidbody>(pressureScreenRoot.gameObject);
                screenRigidbody.useGravity = false;
                screenRigidbody.isKinematic = true;

                SetEnum(pressureScreen, "ownerTeam", (int)DamageTeam.Enemy);
                SetInt(pressureScreen, "defaultMaxIntercepts", 3);
                SetFloat(pressureScreen, "defaultLifetimeSeconds", 1.45f);
                SetFloat(pressureScreen, "defaultRadius", 1.45f);
                SetObjectReference(proxy, "pressureScreen", pressureScreen);

                Transform pressureScreenVisual = EnsureChild(editableRoot.transform, "PressureScreenVisual");
                pressureScreenVisual.localPosition = new Vector3(0f, 0.72f, -0.12f);
                pressureScreenVisual.localRotation = Quaternion.identity;
                pressureScreenVisual.localScale = Vector3.one;
                MeshFilter visualFilter = EnsureComponent<MeshFilter>(pressureScreenVisual.gameObject);
                visualFilter.sharedMesh = LoadPrimitiveMesh(PrimitiveType.Sphere);
                MeshRenderer visualRenderer = EnsureComponent<MeshRenderer>(pressureScreenVisual.gameObject);
                visualRenderer.sharedMaterial = pressureScreenMaterial;
                visualRenderer.enabled = false;
                visualRenderer.shadowCastingMode = ShadowCastingMode.Off;
                visualRenderer.receiveShadows = false;
                visualRenderer.allowOcclusionWhenDynamic = false;
                pressureScreenVisual.gameObject.SetActive(false);
                Collider visualCollider = pressureScreenVisual.GetComponent<Collider>();
                if (visualCollider != null)
                {
                    UnityEngine.Object.DestroyImmediate(visualCollider);
                }

                SummonPressureScreenPresenter presenter = EnsureComponent<SummonPressureScreenPresenter>(editableRoot);
                SetObjectReference(presenter, "pressureScreen", pressureScreen);
                SetObjectReference(presenter, "visualRoot", pressureScreenVisual);
                SetObjectReferenceArray(presenter, "screenRenderers", new UnityEngine.Object[] { visualRenderer });
                SetBool(presenter, "renderVisuals", false);
                SetColor(presenter, "activeColor", new Color(1f, 0.22f, 0.55f, 0.09f));
                SetColor(presenter, "tierTwoColor", new Color(1f, 0.62f, 0.24f, 0.10f));
                SetColor(presenter, "tierThreeColor", new Color(1f, 0.22f, 0.9f, 0.12f));
                SetColor(presenter, "interceptColor", new Color(1f, 0.86f, 0.64f, 0.16f));
                SetFloat(presenter, "visualRadiusScale", 0.18f);
                SetFloat(presenter, "activationCueIntensity", 0.48f);
                SetFloat(presenter, "interceptCueIntensity", 0.58f);
                SetFloat(presenter, "tierCueIntensityStep", 0.08f);
                SetFloat(presenter, "activationFlashSeconds", 0.09f);
                SetFloat(presenter, "interceptFlashSeconds", 0.13f);
                SetFloat(presenter, "finalHitLingerSeconds", 0.1f);
                SetFloat(presenter, "pulseSpeed", 8.2f);
                SetFloat(presenter, "pulseScale", 0.032f);

                Transform tierPulseCore = EnsureChild(editableRoot.transform, "TierPressureCore");
                tierPulseCore.localPosition = new Vector3(0f, 1.08f, -0.08f);
                tierPulseCore.localRotation = Quaternion.identity;
                tierPulseCore.localScale = Vector3.one * 0.34f;
                MeshFilter pulseFilter = EnsureComponent<MeshFilter>(tierPulseCore.gameObject);
                pulseFilter.sharedMesh = LoadPrimitiveMesh(PrimitiveType.Sphere);
                MeshRenderer pulseRenderer = EnsureComponent<MeshRenderer>(tierPulseCore.gameObject);
                pulseRenderer.sharedMaterial = pulseMaterial;
                pulseRenderer.enabled = false;
                pulseRenderer.shadowCastingMode = ShadowCastingMode.Off;
                pulseRenderer.receiveShadows = false;
                pulseRenderer.allowOcclusionWhenDynamic = false;
                tierPulseCore.gameObject.SetActive(false);
                Collider pulseCollider = tierPulseCore.GetComponent<Collider>();
                if (pulseCollider != null)
                {
                    UnityEngine.Object.DestroyImmediate(pulseCollider);
                }

                ConfigureSummonShieldVfx(
                    editableRoot,
                    pressureScreenMaterial,
                    pressureScreenVisual.localPosition,
                    forwardSign: -1f,
                    radius: 1.45f);
                ConfigureSummonPulseVfx(
                    editableRoot,
                    "TierPressureCore",
                    pulseMaterial,
                    forwardSign: -1f,
                    radius: 0.52f);
                ConfigureSummonStateVfx(
                    editableRoot,
                    "BossPressureAura",
                    ImportedMagicMissilesPressureAuraPrefabPath,
                    new Vector3(0f, 0.08f, -0.04f),
                    Vector3.one * 0.62f);

                Transform summonVisual = AttachRoleVisualOnly(
                    editableRoot.transform,
                    BossSummonPressureActorVisualRoleId,
                    ActionFoundationEnemyRoleCandidateSetup.AuraCaptainElitePrefabPath,
                    BossSummonPressureActorVisualName,
                    new Vector3(0f, -0.04f, 0.1f),
                    Vector3.zero,
                    new Vector3(0.66f, 0.66f, 0.66f));
                EnsureSummonHealthBar(
                    editableRoot,
                    proxy,
                    proxyHealth,
                    DamageTeam.Enemy,
                    new Vector3(0f, 1.82f, -0.02f),
                    0.84f);

                SummonFrontlineProxyPresenter actorPresenter =
                    EnsureComponent<SummonFrontlineProxyPresenter>(editableRoot);
                SetObjectReference(actorPresenter, "proxy", proxy);
                SetObjectReference(actorPresenter, "clash", clash);
                SetObjectReference(actorPresenter, "pulseRoot", tierPulseCore);
                SetObjectReferenceArray(
                    actorPresenter,
                    "actorRenderers",
                    BuildPulseRendererReferenceArray(pulseRenderer));
                SetObjectReferenceArray(
                    actorPresenter,
                    "damageFlashRenderers",
                    ToObjectArray(CollectEnabledRenderers(summonVisual.gameObject)));
                Transform damageVfxAnchor = EnsureSummonDamageVfxAnchor(editableRoot, summonVisual);
                SetObjectReference(actorPresenter, "damageVfxAnchor", damageVfxAnchor);
                SetBool(actorPresenter, "renderPulseVisuals", false);
                SetColor(actorPresenter, "tierOneColor", new Color(1f, 0.32f, 0.55f, 0.82f));
                SetColor(actorPresenter, "tierTwoColor", new Color(1f, 0.62f, 0.24f, 0.92f));
                SetColor(actorPresenter, "tierThreeColor", new Color(1f, 0.22f, 0.9f, 1f));
                SetColor(actorPresenter, "flashColor", new Color(1f, 0.95f, 0.84f, 1f));
                SetColor(actorPresenter, "clashFlashColor", new Color(1f, 0.78f, 0.22f, 1f));
                SetFloat(actorPresenter, "entryFlashSeconds", 0.24f);
                SetFloat(actorPresenter, "impactFlashSeconds", 0.2f);
                SetFloat(actorPresenter, "clashFlashSeconds", 0.14f);
                SetFloat(actorPresenter, "impactFlashProgress", 0.82f);
                SetFloat(actorPresenter, "pulseSpeed", 7.4f);
                SetFloat(actorPresenter, "pulseScale", 0.1f);
                SetFloat(actorPresenter, "tierScaleStep", 0.2f);
                SetFloat(actorPresenter, "flashScale", 0.24f);
                SetFloat(actorPresenter, "clashFlashScale", 0.18f);
                SetFloat(actorPresenter, "pressureDamageCueScale", 0.64f);
                ConfigureSummonActorAnimatorPresentation(
                    actorPresenter,
                    summonVisual,
                    animatorMoveSpeedScale: 0.52f);

                PrefabUtility.SaveAsPrefabAsset(editableRoot, BossSummonPressureActorPrefabPath);
            }
            finally
            {
                if (prefabExists)
                {
                    PrefabUtility.UnloadPrefabContents(editableRoot);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(editableRoot);
                }
            }

            return LoadPrefabComponent<SummonFrontlineProxy>(BossSummonPressureActorPrefabPath);
        }

        private static void ConfigureSummonEntryCueVfx(GameObject cueRoot, Material accentMaterial)
        {
            const string VisualPrefix = "SummonEntryVfx_";
            RemoveChildrenWithPrefix(cueRoot.transform, VisualPrefix);
            EditorUtility.SetDirty(cueRoot);
        }

        private static void ConfigureSummonProxyVisualMotion(
            GameObject actorRoot,
            SummonFrontlineProxy proxy,
            Transform motionRoot,
            float airborneHeight,
            float jumpArcHeight,
            float tierArcHeightStep,
            float landingDip,
            float arcEndProgress = 0.82f,
            float landingSettleSeconds = 0.12f,
            Transform movementVfxRoot = null)
        {
            SummonProxyVisualMotionPresenter motionPresenter =
                EnsureComponent<SummonProxyVisualMotionPresenter>(actorRoot);
            SetObjectReference(motionPresenter, "proxy", proxy);
            SetObjectReference(motionPresenter, "motionRoot", motionRoot);
            SetFloat(motionPresenter, "airborneHeight", airborneHeight);
            SetFloat(motionPresenter, "jumpArcHeight", jumpArcHeight);
            SetFloat(motionPresenter, "tierArcHeightStep", tierArcHeightStep);
            SetFloat(motionPresenter, "arcStartProgress", 0f);
            SetFloat(motionPresenter, "arcEndProgress", arcEndProgress);
            SetFloat(motionPresenter, "landingSettleSeconds", landingDip > 0f ? landingSettleSeconds : 0f);
            SetFloat(motionPresenter, "landingDip", landingDip);
            SetObjectReference(motionPresenter, "movementVfxRoot", movementVfxRoot);
            SetObjectReferenceArray(
                motionPresenter,
                "movementVfxParticles",
                movementVfxRoot != null
                    ? ToObjectReferences(movementVfxRoot.GetComponentsInChildren<ParticleSystem>(includeInactive: true))
                    : Array.Empty<UnityEngine.Object>());
        }

        private static Transform EnsureSummonDamageVfxAnchor(GameObject actorRoot, Transform visual)
        {
            const string AnchorName = "DamageVfxAnchor";
            Transform anchor = actorRoot.transform.Find(AnchorName);
            if (anchor == null)
            {
                var anchorObject = new GameObject(AnchorName);
                anchor = anchorObject.transform;
                anchor.SetParent(actorRoot.transform, worldPositionStays: false);
            }

            if (TryResolveEnabledRendererBounds(CollectEnabledRenderers(visual.gameObject), out Bounds bounds)
                && actorRoot.transform.InverseTransformPoint(bounds.center).y >= 0.35f)
            {
                anchor.position = bounds.center;
            }
            else
            {
                anchor.localPosition = ResolveFallbackDamageVfxAnchorLocalPosition(visual);
            }

            anchor.localRotation = Quaternion.identity;
            anchor.localScale = Vector3.one;
            EditorUtility.SetDirty(anchor.gameObject);
            EditorUtility.SetDirty(actorRoot);
            return anchor;
        }

        private static Vector3 ResolveFallbackDamageVfxAnchorLocalPosition(Transform visual)
        {
            float fallbackHeight = Mathf.Max(0.85f, visual.localPosition.y + Mathf.Max(visual.localScale.y, 0.8f));
            return new Vector3(visual.localPosition.x, fallbackHeight, visual.localPosition.z);
        }

        private static bool TryResolveEnabledRendererBounds(Renderer[] renderers, out Bounds bounds)
        {
            bounds = default;
            bool hasBounds = false;
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null || !renderer.enabled)
                {
                    continue;
                }

                if (!hasBounds)
                {
                    bounds = renderer.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            return hasBounds;
        }

        private static Transform AttachPrimitiveDragonVisual(
            Transform parent,
            string controllerRoleId,
            string controllerRolePrefabPath,
            string targetVisualName,
            Vector3 localPosition,
            Vector3 localEulerAngles,
            Vector3 localScale)
        {
            string visualPrefix = targetVisualName.Contains("_", StringComparison.Ordinal)
                ? targetVisualName.Substring(0, targetVisualName.LastIndexOf('_') + 1)
                : targetVisualName;
            RemoveChildrenWithPrefix(parent, visualPrefix);

            var root = new GameObject(targetVisualName);
            root.transform.SetParent(parent, worldPositionStays: false);
            root.transform.localPosition = localPosition;
            root.transform.localRotation = Quaternion.Euler(localEulerAngles);
            root.transform.localScale = localScale;

            Animator animator = EnsureComponent<Animator>(root);
            animator.runtimeAnimatorController =
                ResolveRoleVisualAnimatorController(controllerRoleId, controllerRolePrefabPath);
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

            Material bodyMaterial = LoadOrCreateMaterial(
                SummonSlot3DragonBodyMaterialPath,
                new Color(0.36f, 0.09f, 0.045f, 1f));
            Material wingMaterial = LoadOrCreateTransparentMaterial(
                SummonSlot3DragonWingMaterialPath,
                new Color(1f, 0.28f, 0.08f, 0.38f));

            AddPrimitiveVisualChild(
                root.transform,
                "DragonBody",
                PrimitiveType.Capsule,
                bodyMaterial,
                new Vector3(0f, 0.66f, 0.04f),
                new Vector3(90f, 0f, 0f),
                new Vector3(0.46f, 0.98f, 0.46f));
            AddPrimitiveVisualChild(
                root.transform,
                "DragonHead",
                PrimitiveType.Sphere,
                bodyMaterial,
                new Vector3(0f, 0.85f, 0.92f),
                Vector3.zero,
                new Vector3(0.34f, 0.28f, 0.42f));
            AddPrimitiveVisualChild(
                root.transform,
                "DragonTail",
                PrimitiveType.Capsule,
                bodyMaterial,
                new Vector3(0f, 0.55f, -0.88f),
                new Vector3(72f, 0f, 0f),
                new Vector3(0.2f, 0.72f, 0.2f));
            AddPrimitiveVisualChild(
                root.transform,
                "DragonLeftWing",
                PrimitiveType.Cube,
                wingMaterial,
                new Vector3(-0.62f, 0.94f, -0.08f),
                new Vector3(0f, 0f, -24f),
                new Vector3(0.08f, 0.58f, 1.16f));
            AddPrimitiveVisualChild(
                root.transform,
                "DragonRightWing",
                PrimitiveType.Cube,
                wingMaterial,
                new Vector3(0.62f, 0.94f, -0.08f),
                new Vector3(0f, 0f, 24f),
                new Vector3(0.08f, 0.58f, 1.16f));
            AddPrimitiveVisualChild(
                root.transform,
                "DragonMawGlow",
                PrimitiveType.Sphere,
                LoadOrCreateTransparentMaterial(SummonSlot3FireBreathMaterialPath, new Color(1f, 0.42f, 0.08f, 0.68f)),
                new Vector3(0f, 0.84f, 1.22f),
                Vector3.zero,
                new Vector3(0.16f, 0.16f, 0.16f));

            ValidateSummonActorRoleVisualContents(root, targetVisualName);
            return root.transform;
        }

        private static RuntimeAnimatorController ResolveRoleVisualAnimatorController(
            string roleId,
            string rolePrefabPath)
        {
            EnemyRoleVisualSpec visualSpec = ActionFoundationEnemyRoleVisualSetup.CreateForRole(roleId);
            GameObject prefabContents = PrefabUtility.LoadPrefabContents(rolePrefabPath);
            try
            {
                Transform sourceVisual = prefabContents.transform.Find(visualSpec.VisualName);
                if (sourceVisual == null)
                {
                    throw new InvalidOperationException($"{rolePrefabPath} is missing {visualSpec.VisualName}.");
                }

                Animator animator = sourceVisual.GetComponent<Animator>();
                if (animator == null || animator.runtimeAnimatorController == null)
                {
                    throw new InvalidOperationException($"{visualSpec.VisualName} is missing an Animator controller.");
                }

                return animator.runtimeAnimatorController;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabContents);
            }
        }

        private static Renderer AddPrimitiveVisualChild(
            Transform parent,
            string childName,
            PrimitiveType primitiveType,
            Material material,
            Vector3 localPosition,
            Vector3 localEulerAngles,
            Vector3 localScale)
        {
            Transform child = EnsureChild(parent, childName);
            child.localPosition = localPosition;
            child.localRotation = Quaternion.Euler(localEulerAngles);
            child.localScale = localScale;

            MeshFilter filter = EnsureComponent<MeshFilter>(child.gameObject);
            filter.sharedMesh = LoadPrimitiveMesh(primitiveType);
            MeshRenderer renderer = EnsureComponent<MeshRenderer>(child.gameObject);
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.allowOcclusionWhenDynamic = false;

            Collider[] colliders = child.GetComponents<Collider>();
            for (int i = colliders.Length - 1; i >= 0; i--)
            {
                UnityEngine.Object.DestroyImmediate(colliders[i]);
            }

            return renderer;
        }

        private static void ConfigureSummonShieldVfx(
            GameObject actorRoot,
            Material material,
            Vector3 screenCenter,
            float forwardSign,
            float radius)
        {
            const string VisualPrefix = "SummonShieldVfx_";
            RemoveChildrenWithPrefix(actorRoot.transform, VisualPrefix);
            EditorUtility.SetDirty(actorRoot);
        }

        private static void ConfigureSummonPulseVfx(
            GameObject actorRoot,
            string pulseRootName,
            Material material,
            float forwardSign,
            float radius)
        {
            const string VisualPrefix = "SummonPulseVfx_";
            RemoveChildrenWithPrefix(actorRoot.transform, VisualPrefix);
            EditorUtility.SetDirty(actorRoot);
        }

        private static void ConfigureSummonStateVfx(
            GameObject actorRoot,
            string stateName,
            string sourcePrefabPath,
            Vector3 localPosition,
            Vector3 localScale)
        {
            const string VisualPrefix = "SummonStateVfx_";
            RemoveChildrenWithPrefix(actorRoot.transform, VisualPrefix);
            EditorUtility.SetDirty(actorRoot);
        }

        private static SummonFrontlineHealthBarPresenter EnsureSummonHealthBar(
            GameObject editableRoot,
            SummonFrontlineProxy proxy,
            CombatHealth health,
            DamageTeam ownerTeam,
            Vector3 localPosition,
            float width)
        {
            Material backMaterial = LoadOrCreateTransparentMaterial(
                SummonHealthBarBackMaterialPath,
                new Color(0.015f, 0.018f, 0.022f, 0.78f));
            Material fillMaterial = LoadOrCreateMaterial(
                ownerTeam == DamageTeam.Enemy
                    ? SummonHealthBarEnemyFillMaterialPath
                    : SummonHealthBarAllyFillMaterialPath,
                ownerTeam == DamageTeam.Enemy
                    ? new Color(1f, 0.22f, 0.32f, 1f)
                    : new Color(0.18f, 1f, 0.48f, 1f));

            Transform barRoot = EnsureChild(editableRoot.transform, SummonHealthBarRootName);
            barRoot.localPosition = localPosition;
            barRoot.localRotation = Quaternion.identity;
            barRoot.localScale = Vector3.one;

            float clampedWidth = Mathf.Max(0.2f, width);
            MeshRenderer backRenderer = EnsureSummonHealthBarSegment(
                EnsureChild(barRoot, SummonHealthBarBackName),
                backMaterial,
                Vector3.zero,
                new Vector3(clampedWidth, 0.075f, 0.035f));
            MeshRenderer fillRenderer = EnsureSummonHealthBarSegment(
                EnsureChild(barRoot, SummonHealthBarFillName),
                fillMaterial,
                new Vector3(0f, 0f, -0.024f),
                new Vector3(clampedWidth - 0.08f, 0.045f, 0.04f));

            SummonFrontlineHealthBarPresenter healthBarPresenter =
                EnsureComponent<SummonFrontlineHealthBarPresenter>(editableRoot);
            healthBarPresenter.ConfigurePresentation(
                proxy,
                health,
                barRoot,
                fillRenderer.transform,
                new Renderer[] { backRenderer, fillRenderer });
            EditorUtility.SetDirty(editableRoot);
            return healthBarPresenter;
        }

        private static MeshRenderer EnsureSummonHealthBarSegment(
            Transform segment,
            Material material,
            Vector3 localPosition,
            Vector3 localScale)
        {
            segment.localPosition = localPosition;
            segment.localRotation = Quaternion.identity;
            segment.localScale = localScale;

            MeshFilter filter = EnsureComponent<MeshFilter>(segment.gameObject);
            filter.sharedMesh = LoadPrimitiveMesh(PrimitiveType.Cube);
            MeshRenderer renderer = EnsureComponent<MeshRenderer>(segment.gameObject);
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.allowOcclusionWhenDynamic = false;

            Collider[] colliders = segment.GetComponents<Collider>();
            for (int i = colliders.Length - 1; i >= 0; i--)
            {
                UnityEngine.Object.DestroyImmediate(colliders[i]);
            }

            return renderer;
        }

        private static void EnsureSupportSummonActionProfiles()
        {
            ConfigureSummonSlotActionProfile(
                LoadOrCreateSummonSlotActionProfile(SummonSlot1ActionProfilePath),
                "SummonSlot1.ChargeBruiser",
                new[]
                {
                    CreateSummonTierSettings(
                        58f,
                        24f,
                        1.25f,
                        0.46f,
                        1,
                        0.55f,
                        4.0f,
                        2.0f,
                        1,
                        0f,
                        1.45f,
                        "ChargeBruiser",
                        250f,
                        3.4f,
                        1.1f,
                        12f,
                        1.05f),
                    CreateSummonTierSettings(
                        92f,
                        24f,
                        1.35f,
                        0.52f,
                        2,
                        1.5f,
                        4.8f,
                        2.36f,
                        2,
                        0f,
                        1.8f,
                        "ChargeBruiser",
                        420f,
                        3.8f,
                        1.2f,
                        20f,
                        1.1f),
                    CreateSummonTierSettings(
                        126f,
                        25f,
                        1.45f,
                        0.58f,
                        3,
                        2.4f,
                        5.6f,
                        2.74f,
                        3,
                        0f,
                        2.15f,
                        "ChargeBruiser",
                        600f,
                        4.2f,
                        1.32f,
                        30f,
                        1.15f)
                },
                new[]
                {
                    CreateSummonReadout(
                        "LV1 Charge Break",
                        "Mid-cost bruiser that spends a saved bar for one obvious forward rush impact, then stays as melee pressure.",
                        "Hold EN until a boss summon or recovery window is worth a visible charge answer.",
                        "SciFi bruiser spawns on the frontline, rushes forward with a ground trail, hits with a clear impact burst, and keeps punching."),
                    CreateSummonReadout(
                        "LV2 Heavy Charge",
                        "Higher stored-EN version with a wider body-check screen and enough health to hold contact longer.",
                        "Use when the boss is about to stay in a punishable lane and a cheap laser will not change the exchange.",
                        "Longer rush, two shock bolts, a broader collision burst, and steadier melee lockdown."),
                    CreateSummonReadout(
                        "LV3 Breakthrough Rush",
                        "High-stored-EN payoff that should visibly interrupt the lane and keep fighting after impact.",
                        "Save for the exchange where one big arrival has to change the screen immediately.",
                        "Fast ground rush, three shock bolts, large forward impact, and a durable bruiser body that remains in melee without out-DPSing the boss summons.")
                });

            ConfigureSummonSlotActionProfile(
                LoadOrCreateSummonSlotActionProfile(SummonSlot2ActionProfilePath),
                "SummonSlot2.LaserSoldier",
                new[]
                {
                    CreateSummonTierSettings(16f, 30f, 0.95f, 0.16f, 1, 0.3f, 1.35f, 2.08f, 0, 0f, 0.2f, "LaserSoldier", 170f, 2.8f, 0.72f, 9f, 1.15f),
                    CreateSummonTierSettings(22f, 32f, 1.05f, 0.2f, 1, 0.8f, 1.6f, 2.32f, 0, 0f, 0.2f, "LaserSoldier", 205f, 3.1f, 0.78f, 12f, 1.25f),
                    CreateSummonTierSettings(28f, 34f, 1.15f, 0.24f, 2, 1.3f, 1.85f, 2.56f, 0, 0f, 0.2f, "LaserSoldier", 250f, 3.4f, 0.84f, 16f, 1.35f)
                },
                new[]
                {
                    CreateSummonReadout(
                        "LV1 Laser Tap",
                        "Low-return ranged helper that sets up cleanly but cannot block pressure.",
                        "Spend when the boss lane is open and a cheap ranged body is enough.",
                        "SciFi rifleman slides into a side lane, flashes a cyan muzzle beam, and fires one clean laser line per volley."),
                    CreateSummonReadout(
                        "LV2 Split Laser",
                        "Mid-tier ranged support with two visible lines and controlled sustained pressure.",
                        "Hold EN if the boss will stay exposed for more than one volley.",
                        "Laser soldier fires one sharper cyan line with a larger beam flash while staying fragile."),
                    CreateSummonReadout(
                        "LV3 Prism Burst",
                        "High-tier glass-cannon support that widens the lane punish without becoming a turret.",
                        "Use when the player has created a long punish window and does not need a blocker.",
                        "Wider two-line laser burst and stronger muzzle beam, but a slower cadence and low body safety.")
                });

            ConfigureSummonSlotActionProfile(
                LoadOrCreateSummonSlotActionProfile(SummonSlot3ActionProfilePath),
                "SummonSlot3.FireDragon",
                new[]
                {
                    CreateSummonTierSettings(90f, 18f, 1.35f, 0.4f, 1, 1.6f, 1.35f, 2.42f, 0, 0f, 0.2f, "FireDragon", 520f, 2.35f, 1.18f, 32f, 1.9f),
                    CreateSummonTierSettings(130f, 19.5f, 1.5f, 0.48f, 2, 2.6f, 1.6f, 2.72f, 0, 0f, 0.2f, "FireDragon", 680f, 2.65f, 1.26f, 46f, 2.1f),
                    CreateSummonTierSettings(220f, 21f, 1.65f, 0.56f, 3, 3.8f, 1.85f, 3.06f, 0, 0f, 0.2f, "FireDragon", 900f, 2.95f, 1.36f, 68f, 2.3f)
                },
                new[]
                {
                    CreateSummonReadout(
                        "LV1 Fire Breath",
                        "Expensive ranged summon that trades speed and cadence for a wide flame lane.",
                        "Spend only when the boss is committed and the player can live without a blocker.",
                        "Fire dragon hovers above the lane and breathes one broad fire lance from a visible orange beam."),
                    CreateSummonReadout(
                        "LV2 Furnace Sweep",
                        "Mid-tier dragon breath covers a wider lane and rewards a longer punish read.",
                        "Hold EN when the boss will remain exposed after the first breath tick.",
                        "Larger hovering dragon, two fire chunks, wider lateral spread, and a stronger breath beam."),
                    CreateSummonReadout(
                        "LV3 Inferno Beam",
                        "High-risk high-return dragon that should visibly dominate a punish window.",
                        "Save for the long boss recovery where raw damage matters more than defense.",
                        "Largest hover silhouette, three wide fire chunks, long orange breath beam, and slow high-cost burn pressure.")
                });

            AssetDatabase.SaveAssets();
        }

        private static BossSummonPressureProfile EnsureBossSummonPressureProfile()
        {
            SummonFrontlineProxy bossLaserActorPrefab = EnsureBossLaserSummonActorPrefab();
            ConfigureBossSummonPressureProfile(
                LoadOrCreateBossSummonPressureProfile(BossSummonPressureProfilePath),
                "BossSummonPressure.SummonCaller",
                new[]
                {
                    CreateBossSummonTierSettings(
                        entryForwardBlend01: 0.28f,
                        lateralOffset: 0.9f,
                        actorLifetimeSeconds: 0f,
                        actorScale: 2.16f,
                        actorAdvanceDistance: 2.8f,
                        actorAdvanceSeconds: 1.55f,
                        actorRoleId: "LaserSoldier",
                        actorMaxHealth: 460f,
                        actorMoveSpeed: 3.5f,
                        actorEngageRadius: 1.05f,
                        actorAttackDamagePerSecond: 34f,
                        actorAttackIntervalSeconds: 0.18f,
                        screenIntercepts: 0,
                        screenRadius: 1.15f,
                        screenLifetimeSeconds: 0.2f,
                        actorPrefabOverride: bossLaserActorPrefab),
                    CreateBossSummonTierSettings(
                        entryForwardBlend01: 0.38f,
                        lateralOffset: 1.4f,
                        actorLifetimeSeconds: 0f,
                        actorScale: 2.52f,
                        actorAdvanceDistance: 3.8f,
                        actorAdvanceSeconds: 1.85f,
                        actorRoleId: "PressureScreen",
                        actorMaxHealth: 700f,
                        actorMoveSpeed: 3.6f,
                        actorEngageRadius: 1.35f,
                        actorAttackDamagePerSecond: 62f,
                        actorAttackIntervalSeconds: 0.84f,
                        screenIntercepts: 5,
                        screenRadius: 1.55f,
                        screenLifetimeSeconds: 4.0f),
                    CreateBossSummonTierSettings(
                        entryForwardBlend01: 0.5f,
                        lateralOffset: 2.0f,
                        actorLifetimeSeconds: 0f,
                        actorScale: 2.48f,
                        actorAdvanceDistance: 4.4f,
                        actorAdvanceSeconds: 2.05f,
                        actorRoleId: "LaserSoldier",
                        actorMaxHealth: 760f,
                        actorMoveSpeed: 4.0f,
                        actorEngageRadius: 1.15f,
                        actorAttackDamagePerSecond: 58f,
                        actorAttackIntervalSeconds: 0.12f,
                        screenIntercepts: 0,
                        screenRadius: 1.15f,
                        screenLifetimeSeconds: 0.2f,
                        actorPrefabOverride: bossLaserActorPrefab)
                },
                new[]
                {
                    CreateBossSummonReadout(
                        "LV1 Laser Soldier",
                        "Low-cost boss rifleman that creates the first readable dodge-line check without waiting for a high-tier bank.",
                        "Read the thin aim line, dodge after the lock, then punish the rifleman before the next boss action.",
                        "A cheap summon can body-clash the rifleman, but the primary read is movement first."),
                    CreateBossSummonReadout(
                        "LV2 Pressure Screen",
                        "Boss-side summon pressure that contests the frontline for several seconds and blocks player follow-up shots.",
                        "Take EN only long enough to prepare a clean response, then break the screen before the next boss pattern layers on top.",
                        "Use SummonSlot1 or Vanguard support to absorb the curtain and reopen ranged punish time."),
                    CreateBossSummonReadout(
                        "LV3 Laser Soldier",
                        "High-cost boss laser summon that creates a dodgeable line threat instead of another pressure screen.",
                        "Read the thin line, dodge after the aim locks, then punish during the rifleman's recovery.",
                        "Boss laser soldier repositions, draws a cyan warning line, locks aim, then fires a short ticking beam.")
                });

            AssetDatabase.SaveAssets();
            return LoadAsset<BossSummonPressureProfile>(BossSummonPressureProfilePath);
        }

        private static BossSummonPressureProfile LoadOrCreateBossSummonPressureProfile(string assetPath)
        {
            EnsureFolderForAsset(assetPath);
            BossSummonPressureProfile profile = AssetDatabase.LoadAssetAtPath<BossSummonPressureProfile>(assetPath);
            if (profile != null)
            {
                return profile;
            }

            profile = ScriptableObject.CreateInstance<BossSummonPressureProfile>();
            AssetDatabase.CreateAsset(profile, assetPath);
            return profile;
        }

        private static void ConfigureBossSummonPressureProfile(
            BossSummonPressureProfile profile,
            string pressureId,
            BossSummonPressureAction.BossSummonTierSettings[] tierSettings,
            BossSummonPressureProfile.BossSummonTierReadout[] tierReadouts)
        {
            var serializedObject = new SerializedObject(profile);
            RequireProperty(serializedObject, "pressureId").stringValue = pressureId;

            SerializedProperty tierSettingsProperty = RequireProperty(serializedObject, "tierSettings");
            tierSettingsProperty.arraySize = tierSettings.Length;
            for (int i = 0; i < tierSettings.Length; i++)
            {
                SetBossSummonTierSettings(tierSettingsProperty.GetArrayElementAtIndex(i), tierSettings[i]);
            }

            SerializedProperty tierReadoutsProperty = RequireProperty(serializedObject, "tierReadouts");
            tierReadoutsProperty.arraySize = tierReadouts.Length;
            for (int i = 0; i < tierReadouts.Length; i++)
            {
                SetBossSummonTierReadout(tierReadoutsProperty.GetArrayElementAtIndex(i), tierReadouts[i]);
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static BossSummonPressureAction.BossSummonTierSettings CreateBossSummonTierSettings(
            float entryForwardBlend01,
            float lateralOffset,
            float actorLifetimeSeconds,
            float actorScale,
            float actorAdvanceDistance,
            float actorAdvanceSeconds,
            string actorRoleId,
            float actorMaxHealth,
            float actorMoveSpeed,
            float actorEngageRadius,
            float actorAttackDamagePerSecond,
            float actorAttackIntervalSeconds,
            int screenIntercepts,
            float screenRadius,
            float screenLifetimeSeconds,
            SummonFrontlineProxy actorPrefabOverride = null)
        {
            var settings = new BossSummonPressureAction.BossSummonTierSettings
            {
                EntryForwardBlend01 = entryForwardBlend01,
                LateralOffset = lateralOffset,
                EntryHeight = 0.2f,
                ActorLifetimeSeconds = actorLifetimeSeconds,
                ActorScale = actorScale,
                ActorRoleId = actorRoleId,
                ActorPrefabOverride = actorPrefabOverride,
                ActorMaxHealth = actorMaxHealth,
                ActorMoveSpeed = actorMoveSpeed,
                ActorAdvanceDistance = actorAdvanceDistance,
                ActorAdvanceSeconds = actorAdvanceSeconds,
                ActorEngageRadius = actorEngageRadius,
                ActorAttackDamagePerSecond = actorAttackDamagePerSecond,
                ActorAttackIntervalSeconds = actorAttackIntervalSeconds,
                ScreenIntercepts = screenIntercepts,
                ScreenRadius = screenRadius,
                ScreenLifetimeSeconds = screenLifetimeSeconds
            };
            settings.Normalize();
            return settings;
        }

        private static BossSummonPressureProfile.BossSummonTierReadout CreateBossSummonReadout(
            string tierLabel,
            string stageRole,
            string playerRead,
            string summonRead)
        {
            return new BossSummonPressureProfile.BossSummonTierReadout
            {
                TierLabel = tierLabel,
                StageRole = stageRole,
                PlayerRead = playerRead,
                SummonRead = summonRead
            };
        }

        private static void SetBossSummonTierSettings(
            SerializedProperty property,
            BossSummonPressureAction.BossSummonTierSettings settings)
        {
            property.FindPropertyRelative("EntryForwardBlend01").floatValue = settings.EntryForwardBlend01;
            property.FindPropertyRelative("LateralOffset").floatValue = settings.LateralOffset;
            property.FindPropertyRelative("EntryHeight").floatValue = settings.EntryHeight;
            property.FindPropertyRelative("ActorLifetimeSeconds").floatValue = settings.ActorLifetimeSeconds;
            property.FindPropertyRelative("ActorScale").floatValue = settings.ActorScale;
            property.FindPropertyRelative("ActorRoleId").stringValue = settings.ActorRoleId;
            property.FindPropertyRelative("ActorPrefabOverride").objectReferenceValue = settings.ActorPrefabOverride;
            property.FindPropertyRelative("ActorMaxHealth").floatValue = settings.ActorMaxHealth;
            property.FindPropertyRelative("ActorMoveSpeed").floatValue = settings.ActorMoveSpeed;
            property.FindPropertyRelative("ActorAdvanceDistance").floatValue = settings.ActorAdvanceDistance;
            property.FindPropertyRelative("ActorAdvanceSeconds").floatValue = settings.ActorAdvanceSeconds;
            property.FindPropertyRelative("ActorEngageRadius").floatValue = settings.ActorEngageRadius;
            property.FindPropertyRelative("ActorAttackDamagePerSecond").floatValue = settings.ActorAttackDamagePerSecond;
            property.FindPropertyRelative("ActorAttackIntervalSeconds").floatValue = settings.ActorAttackIntervalSeconds;
            property.FindPropertyRelative("ScreenIntercepts").intValue = settings.ScreenIntercepts;
            property.FindPropertyRelative("ScreenRadius").floatValue = settings.ScreenRadius;
            property.FindPropertyRelative("ScreenLifetimeSeconds").floatValue = settings.ScreenLifetimeSeconds;
        }

        private static void SetBossSummonTierReadout(
            SerializedProperty property,
            BossSummonPressureProfile.BossSummonTierReadout readout)
        {
            property.FindPropertyRelative("TierLabel").stringValue = readout.TierLabel;
            property.FindPropertyRelative("StageRole").stringValue = readout.StageRole;
            property.FindPropertyRelative("PlayerRead").stringValue = readout.PlayerRead;
            property.FindPropertyRelative("SummonRead").stringValue = readout.SummonRead;
        }

        private static SummonSlotActionProfile LoadOrCreateSummonSlotActionProfile(string assetPath)
        {
            EnsureFolderForAsset(assetPath);
            SummonSlotActionProfile profile = AssetDatabase.LoadAssetAtPath<SummonSlotActionProfile>(assetPath);
            if (profile != null)
            {
                return profile;
            }

            profile = ScriptableObject.CreateInstance<SummonSlotActionProfile>();
            AssetDatabase.CreateAsset(profile, assetPath);
            return profile;
        }

        private static void ConfigureSummonSlotActionProfile(
            SummonSlotActionProfile profile,
            string actionId,
            PlayerSummonSlot1Action.SummonTierSettings[] tierSettings,
            SummonSlotActionProfile.SummonTierReadout[] tierReadouts)
        {
            var serializedObject = new SerializedObject(profile);
            RequireProperty(serializedObject, "actionId").stringValue = actionId;
            SerializedProperty tierSettingsProperty = RequireProperty(serializedObject, "tierSettings");
            tierSettingsProperty.arraySize = tierSettings.Length;
            for (int i = 0; i < tierSettings.Length; i++)
            {
                SetSummonTierSettings(tierSettingsProperty.GetArrayElementAtIndex(i), tierSettings[i]);
            }

            SerializedProperty tierReadoutsProperty = RequireProperty(serializedObject, "tierReadouts");
            tierReadoutsProperty.arraySize = tierReadouts.Length;
            for (int i = 0; i < tierReadouts.Length; i++)
            {
                SetSummonTierReadout(tierReadoutsProperty.GetArrayElementAtIndex(i), tierReadouts[i]);
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static PlayerSummonSlot1Action.SummonTierSettings CreateSummonTierSettings(
            float damage,
            float projectileSpeed,
            float lifetimeSeconds,
            float radius,
            int projectileCount,
            float lateralReach,
            float actorAdvanceDistance,
            float actorScale,
            int screenIntercepts,
            float actorLifetimeSeconds,
            float screenLifetimeSeconds,
            string actorRoleId,
            float actorMaxHealth,
            float actorMoveSpeed,
            float actorEngageRadius,
            float actorAttackDamagePerSecond,
            float actorAttackIntervalSeconds,
            float counterDamage = -1f)
        {
            var settings = new PlayerSummonSlot1Action.SummonTierSettings
            {
                Damage = damage,
                ProjectileSpeed = projectileSpeed,
                LifetimeSeconds = lifetimeSeconds,
                Radius = radius,
                ProjectileCount = projectileCount,
                LateralReach = lateralReach,
                EntryHeight = 0.18f,
                TargetHeight = 1.35f,
                CueScale = 0.46f + Mathf.Max(0, projectileCount - 1) * 0.05f,
                CueLifetimeSeconds = 0.36f,
                ActorLifetimeSeconds = actorLifetimeSeconds,
                ActorScale = actorScale,
                ActorRoleId = actorRoleId,
                ActorMaxHealth = actorMaxHealth,
                ActorMoveSpeed = actorMoveSpeed,
                ActorAdvanceDistance = actorAdvanceDistance,
                ActorAdvanceSeconds = 0.85f + actorAdvanceDistance * 0.35f,
                ActorEngageRadius = actorEngageRadius,
                ActorAttackDamagePerSecond = actorAttackDamagePerSecond,
                ActorAttackIntervalSeconds = actorAttackIntervalSeconds,
                ScreenIntercepts = screenIntercepts,
                ScreenRadius = screenIntercepts > 0 ? 1.35f + screenIntercepts * 0.2f : 1.15f,
                ScreenLifetimeSeconds = screenLifetimeSeconds,
                CounterDamage = counterDamage >= 0f ? counterDamage : damage * 0.32f,
                CounterProjectileSpeed = projectileSpeed + 2f,
                CounterLifetimeSeconds = 1.45f,
                CounterRadius = Mathf.Max(0.2f, radius * 0.7f),
                CounterTargetHeight = 1.35f
            };
            settings.Normalize();
            return settings;
        }

        private static SummonSlotActionProfile.SummonTierReadout CreateSummonReadout(
            string tierLabel,
            string stageRole,
            string playerUse,
            string summonRead)
        {
            return new SummonSlotActionProfile.SummonTierReadout
            {
                TierLabel = tierLabel,
                StageRole = stageRole,
                PlayerUse = playerUse,
                SummonRead = summonRead
            };
        }

        private static void SetSummonTierSettings(
            SerializedProperty property,
            PlayerSummonSlot1Action.SummonTierSettings settings)
        {
            property.FindPropertyRelative("Damage").floatValue = settings.Damage;
            property.FindPropertyRelative("ProjectileSpeed").floatValue = settings.ProjectileSpeed;
            property.FindPropertyRelative("LifetimeSeconds").floatValue = settings.LifetimeSeconds;
            property.FindPropertyRelative("Radius").floatValue = settings.Radius;
            property.FindPropertyRelative("ProjectileCount").intValue = settings.ProjectileCount;
            property.FindPropertyRelative("LateralReach").floatValue = settings.LateralReach;
            property.FindPropertyRelative("EntryHeight").floatValue = settings.EntryHeight;
            property.FindPropertyRelative("TargetHeight").floatValue = settings.TargetHeight;
            property.FindPropertyRelative("CueScale").floatValue = settings.CueScale;
            property.FindPropertyRelative("CueLifetimeSeconds").floatValue = settings.CueLifetimeSeconds;
            property.FindPropertyRelative("ActorLifetimeSeconds").floatValue = settings.ActorLifetimeSeconds;
            property.FindPropertyRelative("ActorScale").floatValue = settings.ActorScale;
            property.FindPropertyRelative("ActorRoleId").stringValue = settings.ActorRoleId;
            property.FindPropertyRelative("ActorMaxHealth").floatValue = settings.ActorMaxHealth;
            property.FindPropertyRelative("ActorMoveSpeed").floatValue = settings.ActorMoveSpeed;
            property.FindPropertyRelative("ActorAdvanceDistance").floatValue = settings.ActorAdvanceDistance;
            property.FindPropertyRelative("ActorAdvanceSeconds").floatValue = settings.ActorAdvanceSeconds;
            property.FindPropertyRelative("ActorEngageRadius").floatValue = settings.ActorEngageRadius;
            property.FindPropertyRelative("ActorAttackDamagePerSecond").floatValue = settings.ActorAttackDamagePerSecond;
            property.FindPropertyRelative("ActorAttackIntervalSeconds").floatValue = settings.ActorAttackIntervalSeconds;
            property.FindPropertyRelative("ScreenIntercepts").intValue = settings.ScreenIntercepts;
            property.FindPropertyRelative("ScreenRadius").floatValue = settings.ScreenRadius;
            property.FindPropertyRelative("ScreenLifetimeSeconds").floatValue = settings.ScreenLifetimeSeconds;
            property.FindPropertyRelative("CounterDamage").floatValue = settings.CounterDamage;
            property.FindPropertyRelative("CounterProjectileSpeed").floatValue = settings.CounterProjectileSpeed;
            property.FindPropertyRelative("CounterLifetimeSeconds").floatValue = settings.CounterLifetimeSeconds;
            property.FindPropertyRelative("CounterRadius").floatValue = settings.CounterRadius;
            property.FindPropertyRelative("CounterTargetHeight").floatValue = settings.CounterTargetHeight;
        }

        private static void SetSummonTierReadout(
            SerializedProperty property,
            SummonSlotActionProfile.SummonTierReadout readout)
        {
            property.FindPropertyRelative("TierLabel").stringValue = readout.TierLabel;
            property.FindPropertyRelative("StageRole").stringValue = readout.StageRole;
            property.FindPropertyRelative("PlayerUse").stringValue = readout.PlayerUse;
            property.FindPropertyRelative("SummonRead").stringValue = readout.SummonRead;
        }

    }
}
