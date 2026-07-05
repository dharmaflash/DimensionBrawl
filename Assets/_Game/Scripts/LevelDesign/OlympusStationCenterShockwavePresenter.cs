using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DimensionBrawl.LevelDesign
{
    [DefaultExecutionOrder(900)]
    [DisallowMultipleComponent]
    public sealed class OlympusStationCenterShockwavePresenter : MonoBehaviour
    {
        private const string CombatSceneName = "OlympusStationCombatStage";
        private const string EffectAssetPath =
            "Assets/_Imported/AssetStore/VFX/Hovl Studio/Sci-fi effects 2/Prefabs/Ray shoots attack big.prefab";
        private const string AnchorName = "OlympusStation_NoCrossCenterLine";
        private const string GlowFloorName = "HardBoundary_GlowFloor";
        private const string CoreLineName = "HardBoundary_CoreLine";
        private const string StopWallName = "HardBoundary_StopWall";

        private static readonly Color CenterLineColor = new Color(0.06f, 0.74f, 1f, 0.22f);
        private static readonly Color CenterLineBrightColor = new Color(0.26f, 0.94f, 1f, 0.58f);
        private static readonly Color ShockwaveColor = new Color(0.14f, 0.82f, 1f, 0.28f);
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        private static readonly string[] DisabledChildNames =
        {
            "SparksLong",
            "Flash",
            "Crater"
        };

        [SerializeField] private Vector3 localOffset = new Vector3(0f, -0.08f, 0f);
        [SerializeField] private Vector3 effectEuler = new Vector3(90f, 0f, 0f);
        [SerializeField] private Vector3 effectScale = new Vector3(0.85f, 0.85f, 0.85f);
        [SerializeField, Min(0f)] private float freezeTimeSeconds = 0.36f;
        [SerializeField, Min(0f)] private float materialRefreshSeconds = 2f;

        private GameObject effectInstance;
        private float materialRefreshUntil;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;
            EnsureForScene(SceneManager.GetActiveScene());
        }

        private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            EnsureForScene(scene);
        }

        private static void EnsureForScene(Scene scene)
        {
            if (scene.name != CombatSceneName)
            {
                return;
            }

            if (FindFirstObjectByType<OlympusStationCenterShockwavePresenter>() != null)
            {
                return;
            }

            new GameObject(nameof(OlympusStationCenterShockwavePresenter))
                .AddComponent<OlympusStationCenterShockwavePresenter>();
        }

        private void Start()
        {
            materialRefreshUntil = Time.unscaledTime + materialRefreshSeconds;
            ApplyNoCrossLineRuntimeMaterials();
            StartCoroutine(InstallRoutine());
        }

        private void LateUpdate()
        {
            if (Time.unscaledTime > materialRefreshUntil)
            {
                return;
            }

            ApplyNoCrossLineRuntimeMaterials();
            if (effectInstance != null)
            {
                ApplyRuntimeHologramMaterials(effectInstance, ShockwaveColor);
            }
        }

        private IEnumerator InstallRoutine()
        {
            yield return null;

            Transform anchor = ResolveAnchor();
            ApplyNoCrossLineRuntimeMaterials();

            Vector3 position = anchor != null ? anchor.position + localOffset : localOffset;
            effectInstance = InstantiateEffect(position);
            if (effectInstance == null)
            {
                yield break;
            }

            effectInstance.name = "OlympusStation_CenterShockwave_Static";
            effectInstance.transform.SetPositionAndRotation(position, Quaternion.Euler(effectEuler));
            effectInstance.transform.localScale = effectScale;

            StripUnwantedChildren(effectInstance.transform);
            ApplyRuntimeHologramMaterials(effectInstance, ShockwaveColor);
            HoldExpandedState(effectInstance);
        }

        private static Transform ResolveAnchor()
        {
            GameObject anchor = GameObject.Find(AnchorName);
            if (anchor != null)
            {
                return anchor.transform;
            }

            GameObject coreLine = GameObject.Find(CoreLineName);
            return coreLine != null ? coreLine.transform : null;
        }

        private static GameObject InstantiateEffect(Vector3 position)
        {
#if UNITY_EDITOR
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(EffectAssetPath);
            if (prefab != null)
            {
                return Instantiate(prefab, position, Quaternion.identity);
            }
#endif

            return CreateFallbackShockwave(position);
        }

        private static GameObject CreateFallbackShockwave(Vector3 position)
        {
            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.transform.position = position;
            quad.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            quad.transform.localScale = new Vector3(6.5f, 0.55f, 1f);

            Renderer renderer = quad.GetComponent<Renderer>();
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
            {
                shader = Shader.Find("Unlit/Transparent");
            }

            if (renderer != null)
            {
                renderer.sharedMaterial = CreateRuntimeHologramMaterial(ShockwaveColor);
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }

            Collider collider = quad.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            return quad;
        }

        private static void ApplyNoCrossLineRuntimeMaterials()
        {
            GameObject stopWall = GameObject.Find(StopWallName);
            if (stopWall != null)
            {
                SetRenderersEnabled(stopWall, false);
            }

            GameObject glowFloor = GameObject.Find(GlowFloorName);
            if (glowFloor != null)
            {
                ApplyRuntimeHologramMaterials(glowFloor, CenterLineColor);
            }

            GameObject coreLine = GameObject.Find(CoreLineName);
            if (coreLine != null)
            {
                ApplyRuntimeHologramMaterials(coreLine, CenterLineBrightColor);
            }
        }

        private static void ApplyRuntimeHologramMaterials(GameObject root, Color color)
        {
            if (root == null)
            {
                return;
            }

            Material material = CreateRuntimeHologramMaterial(color);
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null)
                {
                    continue;
                }

                renderer.enabled = true;
                Material[] existingMaterials = renderer.sharedMaterials;
                if (existingMaterials == null || existingMaterials.Length == 0)
                {
                    renderer.sharedMaterial = material;
                }
                else
                {
                    for (int materialIndex = 0; materialIndex < existingMaterials.Length; materialIndex++)
                    {
                        existingMaterials[materialIndex] = material;
                    }

                    renderer.sharedMaterials = existingMaterials;
                }

                ApplyColorPropertyBlock(renderer, color);
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;

                if (renderer is ParticleSystemRenderer particleRenderer)
                {
                    particleRenderer.sharedMaterial = material;
                    particleRenderer.trailMaterial = material;
                }
            }
        }

        private static void SetRenderersEnabled(GameObject root, bool enabled)
        {
            if (root == null)
            {
                return;
            }

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    renderers[i].enabled = enabled;
                }
            }
        }

        private static Material CreateRuntimeHologramMaterial(Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
            {
                shader = Shader.Find("Unlit/Transparent");
            }

            if (shader == null)
            {
                shader = Shader.Find("Sprites/Default");
            }

            Material material = new Material(shader)
            {
                name = "Runtime_OlympusStation_CyanHologram",
                renderQueue = (int)RenderQueue.Transparent + 45
            };

            material.SetOverrideTag("RenderType", "Transparent");
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            SetFloatIfPresent(material, "_Surface", 1f);
            SetFloatIfPresent(material, "_Blend", 0f);
            SetFloatIfPresent(material, "_SrcBlend", (float)BlendMode.SrcAlpha);
            SetFloatIfPresent(material, "_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            SetFloatIfPresent(material, "_ZWrite", 0f);
            SetFloatIfPresent(material, "_Cull", (float)CullMode.Off);
            SetTextureIfPresent(material, "_BaseMap", Texture2D.whiteTexture);
            SetTextureIfPresent(material, "_MainTex", Texture2D.whiteTexture);
            SetColorIfPresent(material, BaseColorId, color);
            SetColorIfPresent(material, ColorId, color);
            if (material.HasProperty(EmissionColorId))
            {
                material.SetColor(EmissionColorId, new Color(0.26f, 0.95f, 1f, color.a) * 2.4f);
            }

            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
            return material;
        }

        private static void ApplyColorPropertyBlock(Renderer renderer, Color color)
        {
            var propertyBlock = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(BaseColorId, color);
            propertyBlock.SetColor(ColorId, color);
            propertyBlock.SetColor(EmissionColorId, new Color(0.26f, 0.95f, 1f, color.a) * 2.4f);
            renderer.SetPropertyBlock(propertyBlock);
        }

        private static void SetColorIfPresent(Material material, int propertyId, Color color)
        {
            if (material != null && material.HasProperty(propertyId))
            {
                material.SetColor(propertyId, color);
            }
        }

        private static void SetFloatIfPresent(Material material, string propertyName, float value)
        {
            if (material != null && material.HasProperty(propertyName))
            {
                material.SetFloat(propertyName, value);
            }
        }

        private static void SetTextureIfPresent(Material material, string propertyName, Texture texture)
        {
            if (material != null && material.HasProperty(propertyName))
            {
                material.SetTexture(propertyName, texture);
            }
        }

        private static void StripUnwantedChildren(Transform root)
        {
            for (int i = root.childCount - 1; i >= 0; i--)
            {
                Transform child = root.GetChild(i);
                if (ShouldDisable(child.name))
                {
                    child.gameObject.SetActive(false);
                    continue;
                }

                StripUnwantedChildren(child);
            }
        }

        private static bool ShouldDisable(string objectName)
        {
            for (int i = 0; i < DisabledChildNames.Length; i++)
            {
                if (objectName == DisabledChildNames[i])
                {
                    return true;
                }
            }

            return false;
        }

        private void HoldExpandedState(GameObject root)
        {
            ParticleSystem[] systems = root.GetComponentsInChildren<ParticleSystem>(true);
            for (int i = 0; i < systems.Length; i++)
            {
                ParticleSystem particleSystem = systems[i];
                if (particleSystem == null || !particleSystem.gameObject.activeInHierarchy)
                {
                    continue;
                }

                ParticleSystem.MainModule main = particleSystem.main;
                main.loop = false;
                main.playOnAwake = false;
                main.startLifetime = 999f;

                particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                particleSystem.Simulate(freezeTimeSeconds, true, true, true);
                if (particleSystem.particleCount == 0)
                {
                    particleSystem.Emit(1);
                }

                ParticleSystem.EmissionModule emission = particleSystem.emission;
                emission.enabled = false;
                particleSystem.Pause(true);
            }
        }
    }
}
