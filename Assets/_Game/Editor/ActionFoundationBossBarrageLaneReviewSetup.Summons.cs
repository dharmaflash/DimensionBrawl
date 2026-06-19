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
            AssetDatabase.SaveAssets();
            Debug.Log("Reapplied ActionFoundation summon actor health bars.");
        }

        private static GameObject EnsureSummonEntryCuePrefab()
        {
            EnsureFolderForAsset(SummonSlot1EntryCuePrefabPath);
            Material material = LoadOrCreateMaterial(SummonSlot1EntryCueMaterialPath, new Color(0.25f, 1f, 0.68f, 1f));
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

                Collider collider = editableRoot.GetComponent<Collider>();
                if (collider != null && !(collider is SphereCollider))
                {
                    UnityEngine.Object.DestroyImmediate(collider);
                }

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
            EnsureFolderForAsset(SummonSlot1ActorPrefabPath);
            Material material = LoadOrCreateMaterial(SummonSlot1ActorMaterialPath, new Color(0.2f, 1f, 0.78f, 1f));
            Material pressureScreenMaterial = LoadOrCreateTransparentMaterial(
                SummonPressureScreenMaterialPath,
                new Color(0.18f, 1f, 0.78f, 0.38f));
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
                visualRenderer.shadowCastingMode = ShadowCastingMode.Off;
                visualRenderer.receiveShadows = false;
                visualRenderer.allowOcclusionWhenDynamic = false;
                Collider visualCollider = pressureScreenVisual.GetComponent<Collider>();
                if (visualCollider != null)
                {
                    UnityEngine.Object.DestroyImmediate(visualCollider);
                }

                SummonPressureScreenPresenter presenter = EnsureComponent<SummonPressureScreenPresenter>(editableRoot);
                SetObjectReference(presenter, "pressureScreen", pressureScreen);
                SetObjectReference(presenter, "visualRoot", pressureScreenVisual);
                SetObjectReferenceArray(presenter, "screenRenderers", new UnityEngine.Object[] { visualRenderer });
                SetColor(presenter, "activeColor", new Color(0.22f, 1f, 0.82f, 0.42f));
                SetColor(presenter, "interceptColor", new Color(0.92f, 1f, 1f, 0.88f));
                SetFloat(presenter, "activationFlashSeconds", 0.12f);
                SetFloat(presenter, "interceptFlashSeconds", 0.18f);
                SetFloat(presenter, "finalHitLingerSeconds", 0.16f);
                SetFloat(presenter, "pulseSpeed", 9f);
                SetFloat(presenter, "pulseScale", 0.04f);

                Transform tierPulseCore = EnsureChild(editableRoot.transform, "TierPulseCore");
                tierPulseCore.localPosition = new Vector3(0f, 1.08f, 0.08f);
                tierPulseCore.localRotation = Quaternion.identity;
                tierPulseCore.localScale = Vector3.one * 0.32f;
                MeshFilter pulseFilter = EnsureComponent<MeshFilter>(tierPulseCore.gameObject);
                pulseFilter.sharedMesh = LoadPrimitiveMesh(PrimitiveType.Sphere);
                MeshRenderer pulseRenderer = EnsureComponent<MeshRenderer>(tierPulseCore.gameObject);
                pulseRenderer.sharedMaterial = pulseMaterial;
                pulseRenderer.shadowCastingMode = ShadowCastingMode.Off;
                pulseRenderer.receiveShadows = false;
                pulseRenderer.allowOcclusionWhenDynamic = false;
                Collider pulseCollider = tierPulseCore.GetComponent<Collider>();
                if (pulseCollider != null)
                {
                    UnityEngine.Object.DestroyImmediate(pulseCollider);
                }

                Transform summonVisual = AttachRoleVisualOnly(
                    editableRoot.transform,
                    SummonSlot1ActorVisualRoleId,
                    ActionFoundationEnemyRoleCandidateSetup.ShieldBreakerElitePrefabPath,
                    SummonSlot1ActorVisualName,
                    new Vector3(0f, -0.04f, -0.08f),
                    Vector3.zero,
                    new Vector3(0.62f, 0.62f, 0.62f));
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
            return EnsureSupportSummonActorPrefab(
                SummonSlot2ActorPrefabPath,
                "PF_SummonSlot2Actor_MarksmanProxy",
                SummonSlot2ActorMaterialPath,
                SummonSlot2ActorPulseMaterialPath,
                SummonSlot2ActorVisualRoleId,
                ActionFoundationEnemyRoleCandidateSetup.BacklineShooterPrefabPath,
                SummonSlot2ActorVisualName,
                new Vector3(-0.1f, -0.04f, -0.08f),
                new Vector3(0f, -12f, 0f),
                new Vector3(0.58f, 0.58f, 0.58f),
                new Color(0.74f, 0.9f, 1f, 0.76f),
                160f,
                0.78f,
                20f,
                0.1f,
                0.18f,
                includePressureScreen: false,
                pressureScreenMaterialPath: null,
                pressureScreenColor: Color.clear);
        }

        private static SummonFrontlineProxy EnsureSummonSlot3ActorPrefab()
        {
            return EnsureSupportSummonActorPrefab(
                SummonSlot3ActorPrefabPath,
                "PF_SummonSlot3Actor_VanguardProxy",
                SummonSlot3ActorMaterialPath,
                SummonSlot3ActorPulseMaterialPath,
                SummonSlot3ActorVisualRoleId,
                ActionFoundationEnemyRoleCandidateSetup.FinalStandCommanderElitePrefabPath,
                SummonSlot3ActorVisualName,
                new Vector3(0.05f, -0.04f, -0.08f),
                new Vector3(0f, 10f, 0f),
                new Vector3(0.66f, 0.66f, 0.66f),
                new Color(1f, 0.74f, 0.32f, 0.82f),
                360f,
                1.18f,
                24f,
                0.1f,
                0.34f,
                includePressureScreen: true,
                pressureScreenMaterialPath: SummonSlot3PressureScreenMaterialPath,
                pressureScreenColor: new Color(1f, 0.74f, 0.32f, 0.42f));
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
            Color pressureScreenColor)
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
                SetEnum(proxyHealth, "team", (int)DamageTeam.AllySummon);
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
                projectileOrigin.localPosition = new Vector3(0f, 0.92f, 0.28f);
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
                pulseRenderer.shadowCastingMode = ShadowCastingMode.Off;
                pulseRenderer.receiveShadows = false;
                pulseRenderer.allowOcclusionWhenDynamic = false;
                Collider pulseCollider = tierPulseCore.GetComponent<Collider>();
                if (pulseCollider != null)
                {
                    UnityEngine.Object.DestroyImmediate(pulseCollider);
                }

                AttachRoleVisualOnly(
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
                    DamageTeam.AllySummon,
                    new Vector3(0f, 1.78f, 0.02f),
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
            visualRenderer.shadowCastingMode = ShadowCastingMode.Off;
            visualRenderer.receiveShadows = false;
            visualRenderer.allowOcclusionWhenDynamic = false;
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
            SetColor(screenPresenter, "activeColor", pressureScreenColor);
            SetColor(screenPresenter, "interceptColor", new Color(1f, 0.96f, 0.78f, 0.9f));
            SetFloat(screenPresenter, "activationFlashSeconds", 0.12f);
            SetFloat(screenPresenter, "interceptFlashSeconds", 0.18f);
            SetFloat(screenPresenter, "finalHitLingerSeconds", 0.16f);
            SetFloat(screenPresenter, "pulseSpeed", 7.8f);
            SetFloat(screenPresenter, "pulseScale", 0.035f);

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
                new Color(1f, 0.22f, 0.55f, 0.38f));
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
                visualRenderer.shadowCastingMode = ShadowCastingMode.Off;
                visualRenderer.receiveShadows = false;
                visualRenderer.allowOcclusionWhenDynamic = false;
                Collider visualCollider = pressureScreenVisual.GetComponent<Collider>();
                if (visualCollider != null)
                {
                    UnityEngine.Object.DestroyImmediate(visualCollider);
                }

                SummonPressureScreenPresenter presenter = EnsureComponent<SummonPressureScreenPresenter>(editableRoot);
                SetObjectReference(presenter, "pressureScreen", pressureScreen);
                SetObjectReference(presenter, "visualRoot", pressureScreenVisual);
                SetObjectReferenceArray(presenter, "screenRenderers", new UnityEngine.Object[] { visualRenderer });
                SetColor(presenter, "activeColor", new Color(1f, 0.22f, 0.55f, 0.42f));
                SetColor(presenter, "interceptColor", new Color(1f, 0.86f, 0.64f, 0.9f));
                SetFloat(presenter, "activationFlashSeconds", 0.14f);
                SetFloat(presenter, "interceptFlashSeconds", 0.2f);
                SetFloat(presenter, "finalHitLingerSeconds", 0.16f);
                SetFloat(presenter, "pulseSpeed", 8.2f);
                SetFloat(presenter, "pulseScale", 0.055f);

                Transform tierPulseCore = EnsureChild(editableRoot.transform, "TierPressureCore");
                tierPulseCore.localPosition = new Vector3(0f, 1.08f, -0.08f);
                tierPulseCore.localRotation = Quaternion.identity;
                tierPulseCore.localScale = Vector3.one * 0.34f;
                MeshFilter pulseFilter = EnsureComponent<MeshFilter>(tierPulseCore.gameObject);
                pulseFilter.sharedMesh = LoadPrimitiveMesh(PrimitiveType.Sphere);
                MeshRenderer pulseRenderer = EnsureComponent<MeshRenderer>(tierPulseCore.gameObject);
                pulseRenderer.sharedMaterial = pulseMaterial;
                pulseRenderer.shadowCastingMode = ShadowCastingMode.Off;
                pulseRenderer.receiveShadows = false;
                pulseRenderer.allowOcclusionWhenDynamic = false;
                Collider pulseCollider = tierPulseCore.GetComponent<Collider>();
                if (pulseCollider != null)
                {
                    UnityEngine.Object.DestroyImmediate(pulseCollider);
                }

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
                LoadOrCreateSummonSlotActionProfile(SummonSlot2ActionProfilePath),
                "SummonSlot2.BacklineMarksman",
                new[]
                {
                    CreateSummonTierSettings(38f, 19f, 2.1f, 0.28f, 2, 1.4f, 1.2f, 0.95f, 0, 0f, 0.2f),
                    CreateSummonTierSettings(44f, 20.5f, 2.35f, 0.3f, 3, 2.8f, 1.45f, 1.05f, 0, 0f, 0.2f),
                    CreateSummonTierSettings(52f, 22f, 2.6f, 0.32f, 4, 4.2f, 1.75f, 1.16f, 0, 0f, 0.2f)
                },
                new[]
                {
                    CreateSummonReadout(
                        "LV1 Cover Shot",
                        "Quick ranged support that adds two clean bolts without blocking boss pressure.",
                        "Spend when the boss is open but the shield slot is not needed yet.",
                        "BacklineShooter enters left of the player lane, advances toward the boss lane, and fires a narrow cover pair."),
                    CreateSummonReadout(
                        "LV2 Focus Volley",
                        "Mid-tier support that pressures the boss lane with a wider three-shot answer.",
                        "Hold EN when you can stay forward long enough for a stronger punish.",
                        "BacklineShooter keeps a side-lane advance toward the boss and fires a three-bolt focused volley."),
                    CreateSummonReadout(
                        "LV3 Marksman Burst",
                        "High-tier ranged support for a clear boss punish window after surviving pressure.",
                        "Use when the next exchange should convert defense into visible boss damage.",
                        "BacklineShooter stays alive long enough to send repeated four-bolt volleys across the contested lane.")
                });

            ConfigureSummonSlotActionProfile(
                LoadOrCreateSummonSlotActionProfile(SummonSlot3ActionProfilePath),
                "SummonSlot3.VanguardCommander",
                new[]
                {
                    CreateSummonTierSettings(34f, 16f, 2.2f, 0.36f, 1, 0.8f, 1.35f, 1.05f, 2, 0f, 2.2f),
                    CreateSummonTierSettings(42f, 17.5f, 2.55f, 0.4f, 2, 2.2f, 1.7f, 1.18f, 4, 0f, 3.0f),
                    CreateSummonTierSettings(50f, 19f, 2.9f, 0.45f, 3, 3.4f, 2.1f, 1.34f, 7, 0f, 3.8f)
                },
                new[]
                {
                    CreateSummonReadout(
                        "LV1 Body Block",
                        "Short vanguard entry that brings an actual guard screen before answering once.",
                        "Spend if the player is cornered near the backline and needs breathing room.",
                        "FinalStandCommander enters right of the lane and pushes toward the frontline with a body-and-screen block."),
                    CreateSummonReadout(
                        "LV2 Hold Line",
                        "Tankier frontline actor that can absorb a small boss curtain before counterfire.",
                        "Hold EN when the next boss pressure wave is readable but dense.",
                        "FinalStandCommander advances until contested, holds a three-hit screen, then fires two heavy bolts."),
                    CreateSummonReadout(
                        "LV3 Break Wall",
                        "High-cost vanguard actor and screen for stabilizing a bad exchange and forcing a counter window.",
                        "Save for the committed boss pattern that would otherwise push the player back.",
                        "FinalStandCommander drives into the frontline with a five-hit screen and heavy three-shot response.")
                });

            AssetDatabase.SaveAssets();
        }

        private static BossSummonPressureProfile EnsureBossSummonPressureProfile()
        {
            ConfigureBossSummonPressureProfile(
                LoadOrCreateBossSummonPressureProfile(BossSummonPressureProfilePath),
                "BossSummonPressure.SummonCaller",
                new[]
                {
                    CreateBossSummonTierSettings(
                        entryForwardBlend01: 0.28f,
                        lateralOffset: 0.9f,
                        actorLifetimeSeconds: 0f,
                        actorScale: 0.92f,
                        actorAdvanceDistance: 2.4f,
                        actorAdvanceSeconds: 1.4f,
                        screenIntercepts: 2,
                        screenRadius: 1.2f,
                        screenLifetimeSeconds: 2.6f),
                    CreateBossSummonTierSettings(
                        entryForwardBlend01: 0.38f,
                        lateralOffset: 1.4f,
                        actorLifetimeSeconds: 0f,
                        actorScale: 1.12f,
                        actorAdvanceDistance: 3.8f,
                        actorAdvanceSeconds: 1.85f,
                        screenIntercepts: 4,
                        screenRadius: 1.55f,
                        screenLifetimeSeconds: 3.4f),
                    CreateBossSummonTierSettings(
                        entryForwardBlend01: 0.5f,
                        lateralOffset: 2.0f,
                        actorLifetimeSeconds: 0f,
                        actorScale: 1.36f,
                        actorAdvanceDistance: 5.2f,
                        actorAdvanceSeconds: 2.35f,
                        screenIntercepts: 7,
                        screenRadius: 1.95f,
                        screenLifetimeSeconds: 4.2f)
                },
                new[]
                {
                    CreateBossSummonReadout(
                        "LV1 Escort Probe",
                        "Low-cost boss proxy that holds the lane long enough for the player to answer with fire or a saved summon.",
                        "Strafe and keep firing; spend SummonSlot1 only if the next barrage overlaps this proxy.",
                        "A short relief answer should remove the screen and keep the lane from being locked."),
                    CreateBossSummonReadout(
                        "LV2 Pressure Screen",
                        "Boss-side summon pressure that contests the frontline for several seconds and blocks player follow-up shots.",
                        "Take EN only long enough to prepare a clean response, then break the screen before the next boss pattern layers on top.",
                        "Use SummonSlot1 or Vanguard support to absorb the curtain and reopen ranged punish time."),
                    CreateBossSummonReadout(
                        "LV3 Clamp Guard",
                        "High-cost boss proxy that punishes overextension and demands a committed high-tier answer or retreat.",
                        "Back off from forward-risk lanes unless a summon answer is already charged.",
                        "A saved LV2/LV3 summon should create a visible pressure-break window before counterfire.")
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
            int screenIntercepts,
            float screenRadius,
            float screenLifetimeSeconds)
        {
            var settings = new BossSummonPressureAction.BossSummonTierSettings
            {
                EntryForwardBlend01 = entryForwardBlend01,
                LateralOffset = lateralOffset,
                EntryHeight = 0.2f,
                ActorLifetimeSeconds = actorLifetimeSeconds,
                ActorScale = actorScale,
                ActorAdvanceDistance = actorAdvanceDistance,
                ActorAdvanceSeconds = actorAdvanceSeconds,
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
            property.FindPropertyRelative("ActorAdvanceDistance").floatValue = settings.ActorAdvanceDistance;
            property.FindPropertyRelative("ActorAdvanceSeconds").floatValue = settings.ActorAdvanceSeconds;
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
            float screenLifetimeSeconds)
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
                CueScale = 1.45f + Mathf.Max(0, projectileCount - 1) * 0.12f,
                CueLifetimeSeconds = 0.82f,
                ActorLifetimeSeconds = actorLifetimeSeconds,
                ActorScale = actorScale,
                ActorAdvanceDistance = actorAdvanceDistance,
                ActorAdvanceSeconds = 0.85f + actorAdvanceDistance * 0.35f,
                ScreenIntercepts = screenIntercepts,
                ScreenRadius = 1.15f + screenIntercepts * 0.16f,
                ScreenLifetimeSeconds = screenLifetimeSeconds,
                CounterDamage = damage * 0.32f,
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
            property.FindPropertyRelative("ActorAdvanceDistance").floatValue = settings.ActorAdvanceDistance;
            property.FindPropertyRelative("ActorAdvanceSeconds").floatValue = settings.ActorAdvanceSeconds;
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
