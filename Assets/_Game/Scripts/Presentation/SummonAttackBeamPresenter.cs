using System.Collections.Generic;
using DimensionBrawl.Combat;
using UnityEngine;

namespace DimensionBrawl.Presentation
{
    [DisallowMultipleComponent]
    public sealed class SummonAttackBeamPresenter : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
        private static readonly LineRenderer[] EmptyLineRenderers = System.Array.Empty<LineRenderer>();
        private static readonly Transform[] EmptyTransforms = System.Array.Empty<Transform>();

        [SerializeField] private SummonFrontlineProxy proxy;
        [SerializeField] private Transform beamRoot;
        [SerializeField] private Renderer[] beamRenderers = System.Array.Empty<Renderer>();
        [SerializeField] private ParticleSystem[] beamParticles = System.Array.Empty<ParticleSystem>();
        [SerializeField] private Color tierOneColor = new Color(0.28f, 0.95f, 1f, 0.78f);
        [SerializeField] private Color tierTwoColor = new Color(0.62f, 0.86f, 1f, 0.86f);
        [SerializeField] private Color tierThreeColor = new Color(1f, 0.72f, 0.22f, 0.92f);
        [SerializeField, Min(0f)] private float tierScaleStep = 0.22f;
        [SerializeField, Min(0f)] private float pulseScale = 0.08f;
        [SerializeField, Min(0.01f)] private float pulseSpeed = 18f;
        [SerializeField] private bool overrideBeamColor = true;
        [SerializeField] private float beamUvScrollSpeed = -6f;
        [SerializeField, Min(0f)] private float beamTextureScalePerMeter = 0.05f;
        [SerializeField, Min(0f)] private float beamMuzzleOffset;
        [SerializeField, Min(0f)] private float beamImpactBackOffset = 0.5f;
        [SerializeField, Min(0.01f)] private float beamWidthMultiplier = 1f;
        [SerializeField, Min(0.01f)] private float authoredBeamLength = 10f;

        private MaterialPropertyBlock propertyBlock;
        private BossLaserSummonPattern laserPattern;
        private Vector3 baseLocalScale = Vector3.one;
        private Vector3 worldBeamOrigin;
        private Vector3 worldBeamEnd;
        private LineRenderer[] beamLineRenderers = EmptyLineRenderers;
        private float[] baseLineRendererWidths = System.Array.Empty<float>();
        private float[] lineRendererUvOffsets = System.Array.Empty<float>();
        private Transform[] beamMuzzleRoots = EmptyTransforms;
        private Transform[] beamImpactRoots = EmptyTransforms;
        private bool hasBaseScale;
        private bool hasBaseLineRendererWidths;
        private bool hasBeamEndpointRoots;
        private bool hasWorldBeamEndpoints;

        public SummonFrontlineProxy Proxy => proxy;
        public Transform BeamRoot => beamRoot;
        public int BeamRendererCount => beamRenderers != null ? beamRenderers.Length : 0;
        public int BeamParticleCount => beamParticles != null ? beamParticles.Length : 0;

        public void SetWorldBeamEndpoints(Vector3 origin, Vector3 end)
        {
            worldBeamOrigin = origin;
            worldBeamEnd = end;
            hasWorldBeamEndpoints = true;
        }

        public void ClearWorldBeamEndpoints()
        {
            hasWorldBeamEndpoints = false;
        }

        private void Awake()
        {
            ResolveReferences();
            CaptureBaseScale();
            Refresh();
        }

        private void OnEnable()
        {
            ResolveReferences();
            CaptureBaseScale();
            Refresh();
        }

        private void OnDisable()
        {
            SetBeamVisible(false);
        }

        private void Update()
        {
            Refresh();
        }

        private void Refresh()
        {
            ResolveReferences();
            if (beamRoot == null)
            {
                return;
            }

            bool active = proxy != null
                && proxy.IsActive
                && (laserPattern != null
                    ? laserPattern.IsLaserPresentationActive
                    : proxy.CurrentState == SummonFrontlineProxyState.Attacking);
            SetBeamVisible(active);
            if (!active)
            {
                return;
            }

            int tier = Mathf.Clamp(proxy.ActiveTier, 1, 3);
            float tierScale = 1f + Mathf.Max(0, tier - 1) * tierScaleStep;
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseScale;
            float localBeamLength = authoredBeamLength;
            float worldBeamLength = authoredBeamLength;
            if (hasWorldBeamEndpoints)
            {
                Vector3 beamVector = worldBeamEnd - worldBeamOrigin;
                worldBeamLength = beamVector.magnitude;
                if (worldBeamLength <= 0.001f)
                {
                    return;
                }

                localBeamLength = ApplyWorldBeamTransform(tierScale, pulse, beamVector, worldBeamLength);
            }
            else
            {
                beamRoot.localScale = baseLocalScale * Mathf.Max(0.01f, tierScale * pulse * beamWidthMultiplier);
                worldBeamLength = ResolveApproximateWorldBeamLength(localBeamLength);
            }

            ApplyLineRendererPlayback(tierScale, pulse, localBeamLength, worldBeamLength);
            ApplyBeamEndpointTransforms(localBeamLength);
            if (overrideBeamColor)
            {
                ApplyColor(ResolveTierColor(tier));
            }
            else
            {
                ClearRendererPropertyBlocks();
            }
        }

        private float ApplyWorldBeamTransform(float tierScale, float pulse, Vector3 beamVector, float beamLength)
        {
            beamRoot.position = worldBeamOrigin;
            beamRoot.rotation = Quaternion.LookRotation(beamVector / beamLength, Vector3.up);

            float parentScale = ResolveParentUniformScale();
            float widthScale = Mathf.Max(0.01f, tierScale * pulse * beamWidthMultiplier) / parentScale;
            float forwardScale = Mathf.Max(0.01f, (Mathf.Abs(baseLocalScale.x) + Mathf.Abs(baseLocalScale.y)) * 0.5f * widthScale);
            beamRoot.localScale = new Vector3(
                baseLocalScale.x * widthScale,
                baseLocalScale.y * widthScale,
                forwardScale);
            return beamLength / Mathf.Max(0.01f, parentScale * forwardScale);
        }

        private float ResolveParentUniformScale()
        {
            if (beamRoot == null || beamRoot.parent == null)
            {
                return 1f;
            }

            Vector3 scale = beamRoot.parent.lossyScale;
            return Mathf.Max(
                0.01f,
                (Mathf.Abs(scale.x) + Mathf.Abs(scale.y) + Mathf.Abs(scale.z)) / 3f);
        }

        private float ResolveApproximateWorldBeamLength(float localBeamLength)
        {
            if (beamRoot == null)
            {
                return localBeamLength;
            }

            return Mathf.Max(0.01f, localBeamLength * ResolveBeamRootWorldForwardScale());
        }

        private void ApplyLineRendererPlayback(
            float tierScale,
            float pulse,
            float localBeamLength,
            float worldBeamLength)
        {
            ResolveLineRendererReferences();
            if (beamLineRenderers == null || beamLineRenderers.Length == 0)
            {
                return;
            }

            float scale = Mathf.Max(0.01f, tierScale * pulse * beamWidthMultiplier);
            float textureScale = Mathf.Max(0.01f, worldBeamLength * beamTextureScalePerMeter);
            for (int i = 0; i < beamLineRenderers.Length; i++)
            {
                LineRenderer lineRenderer = beamLineRenderers[i];
                if (lineRenderer == null)
                {
                    continue;
                }

                float baseWidth = i < baseLineRendererWidths.Length
                    ? baseLineRendererWidths[i]
                    : lineRenderer.widthMultiplier;
                lineRenderer.widthMultiplier = Mathf.Max(0.01f, baseWidth * scale);
                ApplyLineRendererShape(lineRenderer, localBeamLength, textureScale);
                ApplyLineRendererUvScroll(lineRenderer, i);
            }
        }

        private void ApplyLineRendererShape(LineRenderer lineRenderer, float localBeamLength, float textureScale)
        {
            if (lineRenderer.positionCount < 2)
            {
                lineRenderer.positionCount = 2;
            }

            lineRenderer.SetPosition(0, Vector3.zero);
            lineRenderer.SetPosition(1, new Vector3(0f, 0f, Mathf.Max(0.01f, localBeamLength)));

            Material material = lineRenderer.material;
            if (material == null)
            {
                return;
            }

            Vector2 tiling = new Vector2(textureScale, 1f);
            SetTextureScaleIfPresent(material, "_BaseMap", tiling);
            SetTextureScaleIfPresent(material, "_MainTex", tiling);
            SetTextureScaleIfPresent(material, "_MainTexture", tiling);
        }

        private void ApplyLineRendererUvScroll(LineRenderer lineRenderer, int lineIndex)
        {
            if (lineRenderer == null || Mathf.Approximately(beamUvScrollSpeed, 0f))
            {
                return;
            }

            Material material = lineRenderer.material;
            if (material == null)
            {
                return;
            }

            float initialOffset = lineIndex < lineRendererUvOffsets.Length ? lineRendererUvOffsets[lineIndex] : 0f;
            Vector2 offset = new Vector2(Time.time * beamUvScrollSpeed + initialOffset, 0f);
            if (material.HasProperty("_Offset"))
            {
                material.SetVector("_Offset", offset);
            }

            SetTextureOffsetIfPresent(material, "_BaseMap", offset);
            SetTextureOffsetIfPresent(material, "_MainTex", offset);
            SetTextureOffsetIfPresent(material, "_MainTexture", offset);
        }

        private void ApplyBeamEndpointTransforms(float localBeamLength)
        {
            ResolveBeamEndpointReferences();
            float worldForwardScale = ResolveBeamRootWorldForwardScale();
            float muzzleZ = beamMuzzleOffset / worldForwardScale;
            float impactZ = Mathf.Max(0f, localBeamLength - beamImpactBackOffset / worldForwardScale);
            ApplyEndpointLocalZ(beamMuzzleRoots, muzzleZ);
            ApplyEndpointLocalZ(beamImpactRoots, impactZ);
        }

        private float ResolveBeamRootWorldForwardScale()
        {
            if (beamRoot == null)
            {
                return 1f;
            }

            return Mathf.Max(0.01f, ResolveParentUniformScale() * Mathf.Abs(beamRoot.localScale.z));
        }

        private static void ApplyEndpointLocalZ(Transform[] endpoints, float localZ)
        {
            if (endpoints == null)
            {
                return;
            }

            for (int i = 0; i < endpoints.Length; i++)
            {
                Transform endpoint = endpoints[i];
                if (endpoint == null)
                {
                    continue;
                }

                Vector3 localPosition = endpoint.localPosition;
                localPosition.z = localZ;
                endpoint.localPosition = localPosition;
            }
        }

        private static void SetTextureScaleIfPresent(Material material, string propertyName, Vector2 scale)
        {
            if (material != null && material.HasProperty(propertyName))
            {
                material.SetTextureScale(propertyName, scale);
            }
        }

        private static void SetTextureOffsetIfPresent(Material material, string propertyName, Vector2 offset)
        {
            if (material != null && material.HasProperty(propertyName))
            {
                material.SetTextureOffset(propertyName, offset);
            }
        }

        private void SetBeamVisible(bool visible)
        {
            if (beamRoot == null)
            {
                return;
            }

            if (beamRoot.gameObject.activeSelf == visible)
            {
                return;
            }

            beamRoot.gameObject.SetActive(visible);
            ResolveParticleReferences();
            if (beamParticles == null)
            {
                return;
            }

            for (int i = 0; i < beamParticles.Length; i++)
            {
                ParticleSystem particle = beamParticles[i];
                if (particle == null)
                {
                    continue;
                }

                if (visible)
                {
                    particle.Clear(withChildren: true);
                    particle.Play(withChildren: true);
                }
                else
                {
                    particle.Stop(
                        withChildren: true,
                        ParticleSystemStopBehavior.StopEmittingAndClear);
                }
            }
        }

        private Color ResolveTierColor(int tier)
        {
            return tier switch
            {
                1 => tierOneColor,
                2 => tierTwoColor,
                _ => tierThreeColor
            };
        }

        private void ApplyColor(Color color)
        {
            propertyBlock ??= new MaterialPropertyBlock();
            if (beamRenderers == null)
            {
                return;
            }

            for (int i = 0; i < beamRenderers.Length; i++)
            {
                Renderer renderer = beamRenderers[i];
                if (renderer == null)
                {
                    continue;
                }

                renderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor(BaseColorId, color);
                propertyBlock.SetColor(ColorId, color);
                propertyBlock.SetColor(EmissionColorId, color * 1.5f);
                renderer.SetPropertyBlock(propertyBlock);
            }
        }

        private void ClearRendererPropertyBlocks()
        {
            if (beamRenderers == null)
            {
                return;
            }

            propertyBlock ??= new MaterialPropertyBlock();
            for (int i = 0; i < beamRenderers.Length; i++)
            {
                Renderer renderer = beamRenderers[i];
                if (renderer == null)
                {
                    continue;
                }

                propertyBlock.Clear();
                renderer.SetPropertyBlock(propertyBlock);
            }
        }

        private void ResolveReferences()
        {
            if (proxy == null)
            {
                proxy = GetComponent<SummonFrontlineProxy>();
            }

            if (laserPattern == null)
            {
                laserPattern = GetComponent<BossLaserSummonPattern>();
            }

            if (beamRenderers == null || beamRenderers.Length == 0)
            {
                beamRenderers = beamRoot != null
                    ? beamRoot.GetComponentsInChildren<Renderer>(includeInactive: true)
                    : System.Array.Empty<Renderer>();
            }

            ResolveParticleReferences();
            ResolveLineRendererReferences();
            ResolveBeamEndpointReferences();
        }

        private void ResolveParticleReferences()
        {
            if (beamParticles == null || beamParticles.Length == 0)
            {
                beamParticles = beamRoot != null
                    ? beamRoot.GetComponentsInChildren<ParticleSystem>(includeInactive: true)
                    : System.Array.Empty<ParticleSystem>();
            }
        }

        private void ResolveLineRendererReferences()
        {
            if (beamLineRenderers != null && beamLineRenderers.Length > 0)
            {
                CaptureBaseLineRendererWidths();
                return;
            }

            beamLineRenderers = beamRoot != null
                ? beamRoot.GetComponentsInChildren<LineRenderer>(includeInactive: true)
                : EmptyLineRenderers;
            CaptureBaseLineRendererWidths();
        }

        private void CaptureBaseLineRendererWidths()
        {
            if (hasBaseLineRendererWidths
                || beamLineRenderers == null
                || beamLineRenderers.Length == 0)
            {
                return;
            }

            baseLineRendererWidths = new float[beamLineRenderers.Length];
            for (int i = 0; i < beamLineRenderers.Length; i++)
            {
                baseLineRendererWidths[i] = beamLineRenderers[i] != null
                    ? Mathf.Max(0.01f, beamLineRenderers[i].widthMultiplier)
                    : 1f;
            }

            lineRendererUvOffsets = new float[beamLineRenderers.Length];
            for (int i = 0; i < lineRendererUvOffsets.Length; i++)
            {
                lineRendererUvOffsets[i] = Random.Range(0f, 5f);
            }

            hasBaseLineRendererWidths = true;
        }

        private void ResolveBeamEndpointReferences()
        {
            if (hasBeamEndpointRoots)
            {
                return;
            }

            if (beamRoot == null)
            {
                beamMuzzleRoots = EmptyTransforms;
                beamImpactRoots = EmptyTransforms;
                hasBeamEndpointRoots = true;
                return;
            }

            List<Transform> muzzles = new();
            List<Transform> impacts = new();
            Transform[] transforms = beamRoot.GetComponentsInChildren<Transform>(includeInactive: true);
            for (int i = 0; i < transforms.Length; i++)
            {
                Transform candidate = transforms[i];
                if (candidate == null || candidate == beamRoot || candidate.parent != beamRoot)
                {
                    continue;
                }

                string name = candidate.name.ToLowerInvariant();
                if (name.Contains("muzzle"))
                {
                    muzzles.Add(candidate);
                }
                else if (name.Contains("impact") || name.Contains("flare"))
                {
                    impacts.Add(candidate);
                }
            }

            beamMuzzleRoots = muzzles.Count > 0 ? muzzles.ToArray() : EmptyTransforms;
            beamImpactRoots = impacts.Count > 0 ? impacts.ToArray() : EmptyTransforms;
            hasBeamEndpointRoots = true;
        }

        private void CaptureBaseScale()
        {
            if (hasBaseScale || beamRoot == null)
            {
                return;
            }

            baseLocalScale = beamRoot.localScale;
            hasBaseScale = true;
        }
    }
}
