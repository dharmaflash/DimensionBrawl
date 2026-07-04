using System.Collections.Generic;
using DimensionBrawl.Combat;
using DimensionBrawl.Player;
using UnityEngine;
using UnityEngine.Rendering;

namespace DimensionBrawl.Presentation
{
    [DefaultExecutionOrder(140)]
    [DisallowMultipleComponent]
    public sealed class PlayerLockTargetVisualPresenter : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerLockTargetController lockTargetController;
        [SerializeField] private Material ringMaterial;
        [SerializeField] private Material fresnelMaterial;

        [Header("Target Cage")]
        [SerializeField] private bool showTargetCage = true;
        [SerializeField] private bool showTopRing;
        [SerializeField] private bool showVerticalCage;
        [SerializeField] private Color softLockColor = new Color(0.60f, 0.96f, 1f, 0.86f);
        [SerializeField] private Color hardLockColor = new Color(1f, 0.82f, 0.34f, 0.96f);
        [SerializeField, Min(0.1f)] private float minimumRadius = 0.72f;
        [SerializeField, Min(0.1f)] private float radiusPadding = 0.28f;
        [SerializeField, Min(0.1f)] private float minimumHeight = 1.35f;
        [SerializeField, Min(0f)] private float groundLift = 0.045f;
        [SerializeField, Min(0f)] private float verticalRibbonWidth = 0.035f;
        [SerializeField, Min(0f)] private float pulseScale = 0.08f;
        [SerializeField, Min(0f)] private float spinDegreesPerSecond = 72f;

        [Header("Target Outline")]
        [SerializeField] private bool showTargetOutline = true;
        [SerializeField] private Color softOutlineColor = new Color(0.58f, 0.92f, 1f, 0.62f);
        [SerializeField] private Color hardOutlineColor = new Color(1f, 0.78f, 0.36f, 0.72f);
        [SerializeField, Min(0f)] private float outlineWidth = 0.012f;
        [SerializeField, Min(0f)] private float outlinePulseWidth = 0.003f;
        [SerializeField, Min(0f)] private float outlineEmissionBoost = 1.12f;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
        private static readonly int IntensityId = Shader.PropertyToID("_Intensity");
        private static readonly int AlphaId = Shader.PropertyToID("_Alpha");
        private static readonly int OutlineWidthId = Shader.PropertyToID("_OutlineWidth");

        private readonly List<Renderer> outlineRenderers = new List<Renderer>();
        private readonly List<GameObject> outlineObjects = new List<GameObject>();
        private Transform visualRoot;
        private Renderer[] renderers;
        private Mesh ringMesh;
        private Mesh verticalRibbonMesh;
        private Material fallbackMaterial;
        private Material outlineMaterial;
        private MaterialPropertyBlock propertyBlock;
        private CombatHealth presentedTarget;
        private CombatHealth outlinedTarget;

        public PlayerLockTargetController LockTargetController => lockTargetController;

        public void Configure(
            PlayerLockTargetController newLockTargetController,
            Material newRingMaterial,
            Material newFresnelMaterial)
        {
            lockTargetController = newLockTargetController;
            ringMaterial = newRingMaterial;
            fresnelMaterial = newFresnelMaterial;
            RebuildVisuals();
        }

        private void Awake()
        {
            lockTargetController ??= GetComponent<PlayerLockTargetController>();
            RebuildVisuals();
        }

        private void OnEnable()
        {
            SetVisible(false);
            SetOutlineVisible(false);
        }

        private void OnDisable()
        {
            SetOutlineVisible(false);
        }

        private void OnDestroy()
        {
            ClearOutlineVisuals();
        }

        private void LateUpdate()
        {
            if ((!showTargetCage && !showTargetOutline)
                || lockTargetController == null
                || !lockTargetController.HasLockTarget
                || lockTargetController.CurrentTargetHealth == null)
            {
                presentedTarget = null;
                SetVisible(false);
                SetOutlineVisible(false);
                return;
            }

            EnsureVisuals();
            presentedTarget = lockTargetController.CurrentTargetHealth;
            EnsureOutlineVisuals(presentedTarget);
            Bounds bounds = ResolveTargetBounds(presentedTarget);
            Vector3 center = bounds.center;
            float radius = Mathf.Max(minimumRadius, Mathf.Max(bounds.extents.x, bounds.extents.z) + radiusPadding);
            float height = Mathf.Max(minimumHeight, bounds.size.y + groundLift);
            float pulse = 1f + Mathf.Sin(Time.time * 8.5f) * pulseScale;
            float resolvedRadius = radius * pulse;

            visualRoot.position = new Vector3(center.x, bounds.min.y + groundLift, center.z);
            visualRoot.rotation = Quaternion.identity;
            visualRoot.localScale = Vector3.one;

            Transform baseRing = visualRoot.Find("LockBaseRing");
            Transform topRing = visualRoot.Find("LockTopRing");
            Transform frontRibbon = visualRoot.Find("LockRibbonFront");
            Transform backRibbon = visualRoot.Find("LockRibbonBack");
            Transform leftRibbon = visualRoot.Find("LockRibbonLeft");
            Transform rightRibbon = visualRoot.Find("LockRibbonRight");
            if (baseRing != null)
            {
                baseRing.localPosition = Vector3.zero;
                baseRing.localRotation = Quaternion.Euler(0f, Time.time * spinDegreesPerSecond, 0f);
                baseRing.localScale = new Vector3(resolvedRadius, 1f, resolvedRadius);
            }

            if (topRing != null)
            {
                topRing.localPosition = Vector3.up * height;
                topRing.localRotation = Quaternion.Euler(0f, -Time.time * spinDegreesPerSecond * 0.58f, 0f);
                topRing.localScale = new Vector3(resolvedRadius * 0.74f, 1f, resolvedRadius * 0.74f);
                topRing.gameObject.SetActive(showTopRing);
            }

            PositionRibbon(frontRibbon, new Vector3(0f, 0f, resolvedRadius), Quaternion.identity, height);
            PositionRibbon(backRibbon, new Vector3(0f, 0f, -resolvedRadius), Quaternion.Euler(0f, 180f, 0f), height);
            PositionRibbon(leftRibbon, new Vector3(-resolvedRadius, 0f, 0f), Quaternion.Euler(0f, -90f, 0f), height);
            PositionRibbon(rightRibbon, new Vector3(resolvedRadius, 0f, 0f), Quaternion.Euler(0f, 90f, 0f), height);
            SetRibbonActive(frontRibbon);
            SetRibbonActive(backRibbon);
            SetRibbonActive(leftRibbon);
            SetRibbonActive(rightRibbon);

            ApplyMaterialProperties();
            ApplyOutlineProperties();
            SetVisible(true);
            SetOutlineVisible(showTargetOutline);
        }

        private void PositionRibbon(Transform ribbon, Vector3 position, Quaternion rotation, float height)
        {
            if (ribbon == null)
            {
                return;
            }

            ribbon.localPosition = position;
            ribbon.localRotation = rotation;
            ribbon.localScale = new Vector3(1f, height, 1f);
        }

        private void SetRibbonActive(Transform ribbon)
        {
            if (ribbon != null && ribbon.gameObject.activeSelf != showVerticalCage)
            {
                ribbon.gameObject.SetActive(showVerticalCage);
            }
        }

        private void ApplyMaterialProperties()
        {
            propertyBlock ??= new MaterialPropertyBlock();
            Color color = lockTargetController.CurrentLockType == PlayerLockTargetController.LockTargetType.HardLock
                ? hardLockColor
                : softLockColor;
            float intensity = Mathf.Lerp(1.2f, 2.1f, lockTargetController.LockStrength01);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null)
                {
                    continue;
                }

                renderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor(BaseColorId, color);
                propertyBlock.SetColor(ColorId, color);
                propertyBlock.SetColor(EmissionColorId, color * intensity);
                propertyBlock.SetFloat(IntensityId, intensity);
                propertyBlock.SetFloat(AlphaId, color.a);
                renderer.SetPropertyBlock(propertyBlock);
            }
        }

        private void ApplyOutlineProperties()
        {
            if (!showTargetOutline || outlineRenderers.Count <= 0)
            {
                return;
            }

            propertyBlock ??= new MaterialPropertyBlock();
            Color color = lockTargetController.CurrentLockType == PlayerLockTargetController.LockTargetType.HardLock
                ? hardOutlineColor
                : softOutlineColor;
            float pulse = 0.5f + Mathf.Sin(Time.time * 9.25f) * 0.5f;
            float resolvedWidth = Mathf.Max(0f, outlineWidth + outlinePulseWidth * pulse);
            Color emission = color * Mathf.Max(1f, outlineEmissionBoost);
            for (int i = outlineRenderers.Count - 1; i >= 0; i--)
            {
                Renderer renderer = outlineRenderers[i];
                if (renderer == null)
                {
                    outlineRenderers.RemoveAt(i);
                    continue;
                }

                renderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor(BaseColorId, color);
                propertyBlock.SetColor(ColorId, color);
                propertyBlock.SetColor(EmissionColorId, emission);
                propertyBlock.SetFloat(OutlineWidthId, resolvedWidth);
                renderer.SetPropertyBlock(propertyBlock);
            }
        }

        private void RebuildVisuals()
        {
            EnsureVisuals();
            SetVisible(false);
        }

        private void EnsureVisuals()
        {
            if (visualRoot != null && renderers != null && renderers.Length > 0)
            {
                return;
            }

            visualRoot = transform.Find("LockTargetVisuals");
            if (visualRoot == null)
            {
                visualRoot = new GameObject("LockTargetVisuals").transform;
                visualRoot.SetParent(transform, worldPositionStays: false);
            }

            ringMesh ??= CreateRingMesh(128, 0.86f, 1f);
            verticalRibbonMesh ??= CreateVerticalRibbonMesh(Mathf.Max(0.01f, verticalRibbonWidth), 1f);
            Material resolvedRingMaterial = ringMaterial != null ? ringMaterial : ResolveFallbackMaterial();
            Material resolvedFresnelMaterial = fresnelMaterial != null ? fresnelMaterial : resolvedRingMaterial;

            Renderer baseRing = EnsureMeshRenderer("LockBaseRing", ringMesh, resolvedRingMaterial);
            Renderer topRing = EnsureMeshRenderer("LockTopRing", ringMesh, resolvedRingMaterial);
            Renderer front = EnsureMeshRenderer("LockRibbonFront", verticalRibbonMesh, resolvedFresnelMaterial);
            Renderer back = EnsureMeshRenderer("LockRibbonBack", verticalRibbonMesh, resolvedFresnelMaterial);
            Renderer left = EnsureMeshRenderer("LockRibbonLeft", verticalRibbonMesh, resolvedFresnelMaterial);
            Renderer right = EnsureMeshRenderer("LockRibbonRight", verticalRibbonMesh, resolvedFresnelMaterial);
            renderers = new[] { baseRing, topRing, front, back, left, right };
        }

        private void EnsureOutlineVisuals(CombatHealth targetHealth)
        {
            if (!showTargetOutline || targetHealth == null)
            {
                ClearOutlineVisuals();
                return;
            }

            if (outlinedTarget == targetHealth && outlineRenderers.Count > 0)
            {
                return;
            }

            ClearOutlineVisuals();
            outlinedTarget = targetHealth;
            Material material = ResolveOutlineMaterial();
            Renderer[] sourceRenderers = targetHealth.GetComponentsInChildren<Renderer>(includeInactive: false);
            for (int i = 0; i < sourceRenderers.Length; i++)
            {
                Renderer sourceRenderer = sourceRenderers[i];
                if (!IsUsableOutlineSource(sourceRenderer))
                {
                    continue;
                }

                Renderer outlineRenderer = CreateOutlineRenderer(sourceRenderer, material);
                if (outlineRenderer == null)
                {
                    continue;
                }

                outlineRenderers.Add(outlineRenderer);
                outlineObjects.Add(outlineRenderer.gameObject);
            }
        }

        private Renderer CreateOutlineRenderer(Renderer sourceRenderer, Material material)
        {
            GameObject outlineObject = new GameObject(sourceRenderer.name + "_TargetOutline");
            outlineObject.hideFlags = HideFlags.DontSave;
            outlineObject.layer = sourceRenderer.gameObject.layer;
            outlineObject.transform.SetParent(sourceRenderer.transform, worldPositionStays: false);
            outlineObject.transform.localPosition = Vector3.zero;
            outlineObject.transform.localRotation = Quaternion.identity;
            outlineObject.transform.localScale = Vector3.one;

            Renderer outlineRenderer = null;
            if (sourceRenderer is SkinnedMeshRenderer skinnedSource && skinnedSource.sharedMesh != null)
            {
                SkinnedMeshRenderer skinnedOutline = outlineObject.AddComponent<SkinnedMeshRenderer>();
                skinnedOutline.sharedMesh = skinnedSource.sharedMesh;
                skinnedOutline.bones = skinnedSource.bones;
                skinnedOutline.rootBone = skinnedSource.rootBone;
                skinnedOutline.localBounds = skinnedSource.localBounds;
                skinnedOutline.updateWhenOffscreen = true;
                outlineRenderer = skinnedOutline;
            }
            else if (sourceRenderer is MeshRenderer)
            {
                MeshFilter sourceFilter = sourceRenderer.GetComponent<MeshFilter>();
                if (sourceFilter == null || sourceFilter.sharedMesh == null)
                {
                    Destroy(outlineObject);
                    return null;
                }

                MeshFilter outlineFilter = outlineObject.AddComponent<MeshFilter>();
                outlineFilter.sharedMesh = sourceFilter.sharedMesh;
                outlineRenderer = outlineObject.AddComponent<MeshRenderer>();
            }

            if (outlineRenderer == null)
            {
                Destroy(outlineObject);
                return null;
            }

            outlineRenderer.sharedMaterial = material;
            outlineRenderer.shadowCastingMode = ShadowCastingMode.Off;
            outlineRenderer.receiveShadows = false;
            outlineRenderer.allowOcclusionWhenDynamic = false;
            outlineRenderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
            return outlineRenderer;
        }

        private static bool IsUsableOutlineSource(Renderer renderer)
        {
            return renderer != null
                && renderer.enabled
                && renderer.gameObject.activeInHierarchy
                && renderer.GetComponentInParent<SummonPressureScreen>() == null
                && renderer.GetComponentInParent<PlayerLockTargetVisualPresenter>() == null
                && renderer.name.IndexOf("_TargetOutline", System.StringComparison.OrdinalIgnoreCase) < 0
                && (renderer is SkinnedMeshRenderer || renderer is MeshRenderer);
        }

        private Material ResolveOutlineMaterial()
        {
            if (outlineMaterial != null)
            {
                return outlineMaterial;
            }

            Shader shader = Shader.Find("DimensionBrawl/TargetOutlineSilhouette")
                ?? Shader.Find("Universal Render Pipeline/Unlit")
                ?? Shader.Find("Unlit/Color");
            outlineMaterial = new Material(shader)
            {
                name = "RuntimeLockTargetOutline",
                hideFlags = HideFlags.HideAndDontSave
            };
            outlineMaterial.SetColor(BaseColorId, softOutlineColor);
            outlineMaterial.SetColor(ColorId, softOutlineColor);
            outlineMaterial.SetColor(EmissionColorId, softOutlineColor * outlineEmissionBoost);
            outlineMaterial.SetFloat(OutlineWidthId, outlineWidth);
            return outlineMaterial;
        }

        private void SetOutlineVisible(bool visible)
        {
            for (int i = outlineObjects.Count - 1; i >= 0; i--)
            {
                GameObject outlineObject = outlineObjects[i];
                if (outlineObject == null)
                {
                    outlineObjects.RemoveAt(i);
                    continue;
                }

                if (outlineObject.activeSelf != visible)
                {
                    outlineObject.SetActive(visible);
                }
            }
        }

        private void ClearOutlineVisuals()
        {
            for (int i = 0; i < outlineObjects.Count; i++)
            {
                GameObject outlineObject = outlineObjects[i];
                if (outlineObject != null)
                {
                    Destroy(outlineObject);
                }
            }

            outlineObjects.Clear();
            outlineRenderers.Clear();
            outlinedTarget = null;
        }

        private Renderer EnsureMeshRenderer(string childName, Mesh mesh, Material material)
        {
            Transform child = visualRoot.Find(childName);
            if (child == null)
            {
                child = new GameObject(childName).transform;
                child.SetParent(visualRoot, worldPositionStays: false);
            }

            MeshFilter filter = child.GetComponent<MeshFilter>();
            if (filter == null)
            {
                filter = child.gameObject.AddComponent<MeshFilter>();
            }

            filter.sharedMesh = mesh;
            MeshRenderer renderer = child.GetComponent<MeshRenderer>();
            if (renderer == null)
            {
                renderer = child.gameObject.AddComponent<MeshRenderer>();
            }

            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.allowOcclusionWhenDynamic = false;
            renderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
            return renderer;
        }

        private Bounds ResolveTargetBounds(CombatHealth targetHealth)
        {
            if (targetHealth == null)
            {
                return new Bounds(transform.position + Vector3.up, Vector3.one);
            }

            Collider[] colliders = targetHealth.GetComponentsInChildren<Collider>();
            Bounds bounds = default;
            bool hasBounds = false;
            for (int i = 0; i < colliders.Length; i++)
            {
                Collider collider = colliders[i];
                if (collider == null
                    || !collider.enabled
                    || !collider.gameObject.activeInHierarchy
                    || collider.GetComponentInParent<SummonPressureScreen>() != null)
                {
                    continue;
                }

                if (!hasBounds)
                {
                    bounds = collider.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(collider.bounds);
                }
            }

            if (hasBounds)
            {
                return bounds;
            }

            return new Bounds(targetHealth.transform.position + Vector3.up * fallbackHeight, new Vector3(1f, minimumHeight, 1f));
        }

        private float fallbackHeight => Mathf.Max(0.5f, minimumHeight * 0.5f);

        private void SetVisible(bool visible)
        {
            if (visualRoot != null && visualRoot.gameObject.activeSelf != visible)
            {
                visualRoot.gameObject.SetActive(visible);
            }
        }

        private Material ResolveFallbackMaterial()
        {
            if (fallbackMaterial != null)
            {
                return fallbackMaterial;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");
            fallbackMaterial = new Material(shader)
            {
                name = "RuntimeLockTargetFallback",
                hideFlags = HideFlags.HideAndDontSave
            };
            fallbackMaterial.SetColor("_BaseColor", softLockColor);
            fallbackMaterial.SetColor("_Color", softLockColor);
            fallbackMaterial.SetColor("_EmissionColor", softLockColor * 1.5f);
            return fallbackMaterial;
        }

        private static Mesh CreateRingMesh(int segments, float innerRadius, float outerRadius)
        {
            int safeSegments = Mathf.Max(12, segments);
            Vector3[] vertices = new Vector3[(safeSegments + 1) * 2];
            Vector3[] normals = new Vector3[vertices.Length];
            Vector2[] uvs = new Vector2[vertices.Length];
            int[] triangles = new int[safeSegments * 6];

            for (int i = 0; i <= safeSegments; i++)
            {
                float angle01 = i / (float)safeSegments;
                float angle = angle01 * Mathf.PI * 2f;
                Vector3 radial = new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle));
                int vertexIndex = i * 2;
                vertices[vertexIndex] = radial * innerRadius;
                vertices[vertexIndex + 1] = radial * outerRadius;
                normals[vertexIndex] = Vector3.up;
                normals[vertexIndex + 1] = Vector3.up;
                uvs[vertexIndex] = new Vector2(0f, angle01);
                uvs[vertexIndex + 1] = new Vector2(1f, angle01);
            }

            for (int i = 0; i < safeSegments; i++)
            {
                int vertexIndex = i * 2;
                int triangleIndex = i * 6;
                triangles[triangleIndex] = vertexIndex;
                triangles[triangleIndex + 1] = vertexIndex + 1;
                triangles[triangleIndex + 2] = vertexIndex + 2;
                triangles[triangleIndex + 3] = vertexIndex + 1;
                triangles[triangleIndex + 4] = vertexIndex + 3;
                triangles[triangleIndex + 5] = vertexIndex + 2;
            }

            var mesh = new Mesh { name = "PlayerLockTarget_Ring" };
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            mesh.hideFlags = HideFlags.HideAndDontSave;
            return mesh;
        }

        private static Mesh CreateVerticalRibbonMesh(float width, float height)
        {
            float halfWidth = width * 0.5f;
            var mesh = new Mesh { name = "PlayerLockTarget_VerticalRibbon" };
            mesh.SetVertices(new[]
            {
                new Vector3(-halfWidth, 0f, 0f),
                new Vector3(halfWidth, 0f, 0f),
                new Vector3(-halfWidth, height, 0f),
                new Vector3(halfWidth, height, 0f)
            });
            mesh.SetNormals(new[] { Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward });
            mesh.SetUVs(0, new[] { new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 1f), new Vector2(1f, 1f) });
            mesh.SetTriangles(new[] { 0, 2, 1, 1, 2, 3 }, 0);
            mesh.RecalculateBounds();
            mesh.hideFlags = HideFlags.HideAndDontSave;
            return mesh;
        }
    }
}
