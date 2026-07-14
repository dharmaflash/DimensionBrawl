using System;
using System.Collections.Generic;
using UnityEngine;

namespace DimensionBrawl.Core
{
    [DisallowMultipleComponent]
    public sealed class OlympusMobileEnvironmentDetailCuller : MonoBehaviour
    {
        private const float BalancedCullDistance = 120f;
        private const float LowCullDistance = 90f;
        private const float BalancedColliderCullDistance = 45f;
        private const float LowColliderCullDistance = 32f;
        private const float MaxCandidateBoundsSize = 8f;
        private const long MaxCandidateTriangles = 3000L;

        private MeshRenderer[] renderers = Array.Empty<MeshRenderer>();
        private BoundingSphere[] boundingSpheres = Array.Empty<BoundingSphere>();
        private bool[] authoredEnabled = Array.Empty<bool>();
        private bool[] culled = Array.Empty<bool>();
        private long[] rendererTriangleCounts = Array.Empty<long>();
        private Collider[][] candidateColliders = Array.Empty<Collider[]>();
        private bool[][] colliderCulled = Array.Empty<bool[]>();
        private CullingGroup cullingGroup;
        private Transform mapRoot;
        private Camera targetCamera;
        private MobilePerformanceTier configuredTier = MobilePerformanceTier.High;
        private float cullDistance;
        private float colliderCullDistance;
        private long candidateTriangleCount;
        private long culledTriangleCount;
        private int culledRendererCount;
        private int candidateColliderCount;
        private int culledColliderCount;
        private bool cullCandidateColliders;

        public int CandidateCount => renderers.Length;
        public int CulledRendererCount => culledRendererCount;
        public long CandidateTriangleCount => candidateTriangleCount;
        public long CulledTriangleCount => culledTriangleCount;
        public float CullDistance => cullDistance;
        public float ColliderCullDistance => colliderCullDistance;
        public int CandidateColliderCount => candidateColliderCount;
        public int CulledColliderCount => culledColliderCount;

        public bool Configure(Transform stageMapRoot, Camera camera, MobilePerformanceTier tier)
        {
            if (stageMapRoot == null)
            {
                return false;
            }

            bool changed = false;
            if (mapRoot != stageMapRoot || renderers.Length == 0)
            {
                mapRoot = stageMapRoot;
                DiscoverCandidates();
                changed = true;
            }

            Camera resolvedCamera = camera != null ? camera : FindActiveCamera();
            bool cameraChanged = targetCamera != resolvedCamera;
            bool tierChanged = configuredTier != tier;
            targetCamera = resolvedCamera;
            configuredTier = tier;
            cullDistance = ResolveCullDistance(tier);
            colliderCullDistance = ResolveColliderCullDistance(tier);
            cullCandidateColliders = tier != MobilePerformanceTier.High;

            if (!cullCandidateColliders)
            {
                changed |= RestoreAllCandidateColliders();
            }

            if (tier == MobilePerformanceTier.High)
            {
                changed |= RestoreAllRenderers();
                DisposeCullingGroup();
                return changed || tierChanged || cameraChanged;
            }

            if (cameraChanged || cullingGroup == null)
            {
                RebuildCullingGroup();
                changed = true;
            }
            else if (tierChanged)
            {
                SetCullingDistances();
                changed = true;
            }

            changed |= RefreshNow();
            return changed;
        }

        public bool RefreshNow()
        {
            if (targetCamera == null)
            {
                targetCamera = FindActiveCamera();
                if (targetCamera == null)
                {
                    return false;
                }

                RebuildCullingGroup();
            }

            bool changed = false;
            Vector3 cameraPosition = targetCamera.transform.position;
            for (int i = 0; i < renderers.Length; i++)
            {
                MeshRenderer renderer = renderers[i];
                if (renderer == null)
                {
                    continue;
                }

                float distance = Vector3.Distance(
                    cameraPosition,
                    renderer.bounds.center);
                changed |= SetRendererCulled(i, distance > cullDistance);
                changed |= SetColliderCulled(
                    i,
                    cullCandidateColliders && distance > colliderCullDistance);
            }

            return changed;
        }

        private void DiscoverCandidates()
        {
            List<MeshRenderer> candidateRenderers = new();
            List<BoundingSphere> spheres = new();
            List<bool> enabledStates = new();
            List<long> triangleCounts = new();
            List<Collider[]> colliders = new();
            List<Material> sharedMaterials = new();
            candidateTriangleCount = 0L;
            candidateColliderCount = 0;
            MeshRenderer[] allRenderers = mapRoot.GetComponentsInChildren<MeshRenderer>(true);
            for (int i = 0; i < allRenderers.Length; i++)
            {
                MeshRenderer renderer = allRenderers[i];
                if (!IsCandidate(renderer, sharedMaterials, out long triangleCount))
                {
                    continue;
                }

                Bounds bounds = renderer.bounds;
                candidateRenderers.Add(renderer);
                spheres.Add(new BoundingSphere(bounds.center, bounds.extents.magnitude));
                enabledStates.Add(renderer.enabled);
                triangleCounts.Add(triangleCount);
                Collider[] rendererColliders = CollectCandidateColliders(renderer);
                colliders.Add(rendererColliders);
                candidateColliderCount += rendererColliders.Length;
                candidateTriangleCount += triangleCount;
            }

            renderers = candidateRenderers.ToArray();
            boundingSpheres = spheres.ToArray();
            authoredEnabled = enabledStates.ToArray();
            culled = new bool[renderers.Length];
            rendererTriangleCounts = triangleCounts.ToArray();
            candidateColliders = colliders.ToArray();
            colliderCulled = new bool[candidateColliders.Length][];
            for (int i = 0; i < candidateColliders.Length; i++)
            {
                colliderCulled[i] = new bool[candidateColliders[i].Length];
            }

            culledRendererCount = 0;
            culledTriangleCount = 0L;
            culledColliderCount = 0;
        }

        private void RebuildCullingGroup()
        {
            DisposeCullingGroup();
            if (targetCamera == null || boundingSpheres.Length == 0)
            {
                return;
            }

            cullingGroup = new CullingGroup
            {
                targetCamera = targetCamera
            };
            cullingGroup.SetDistanceReferencePoint(targetCamera.transform);
            SetCullingDistances();
            cullingGroup.SetBoundingSpheres(boundingSpheres);
            cullingGroup.SetBoundingSphereCount(boundingSpheres.Length);
            cullingGroup.onStateChanged = HandleCullingStateChanged;
        }

        private void HandleCullingStateChanged(CullingGroupEvent state)
        {
            SetRendererCulled(state.index, state.currentDistance > 1);
            SetColliderCulled(
                state.index,
                cullCandidateColliders && state.currentDistance > 0);
        }

        private bool SetRendererCulled(int index, bool shouldCull)
        {
            if (index < 0 || index >= renderers.Length)
            {
                return false;
            }

            bool changed = false;
            if (culled[index] != shouldCull)
            {
                culled[index] = shouldCull;
                MeshRenderer renderer = renderers[index];
                if (renderer != null)
                {
                    renderer.enabled = authoredEnabled[index] && !shouldCull;
                    long triangles = rendererTriangleCounts[index];
                    if (shouldCull)
                    {
                        culledRendererCount++;
                        culledTriangleCount += triangles;
                    }
                    else
                    {
                        culledRendererCount--;
                        culledTriangleCount -= triangles;
                    }
                }

                changed = true;
            }
            return changed;
        }

        private bool SetColliderCulled(int rendererIndex, bool shouldCull)
        {
            if (rendererIndex < 0
                || rendererIndex >= candidateColliders.Length
                || rendererIndex >= colliderCulled.Length)
            {
                return false;
            }

            bool changed = false;
            Collider[] colliders = candidateColliders[rendererIndex];
            bool[] culledStates = colliderCulled[rendererIndex];
            for (int i = 0; i < colliders.Length; i++)
            {
                Collider collider = colliders[i];
                if (collider == null || culledStates[i] == shouldCull)
                {
                    continue;
                }

                culledStates[i] = shouldCull;
                collider.enabled = !shouldCull;
                culledColliderCount += shouldCull ? 1 : -1;
                changed = true;
            }

            return changed;
        }

        private bool RestoreAllRenderers()
        {
            bool changed = false;
            for (int i = 0; i < renderers.Length; i++)
            {
                changed |= SetRendererCulled(i, false);
            }

            return changed;
        }

        private bool RestoreAllCandidateColliders()
        {
            bool changed = false;
            for (int i = 0; i < candidateColliders.Length; i++)
            {
                changed |= SetColliderCulled(i, false);
            }

            return changed;
        }

        private static Collider[] CollectCandidateColliders(MeshRenderer renderer)
        {
            Collider[] attachedColliders = renderer.GetComponents<Collider>();
            if (attachedColliders.Length == 0)
            {
                return Array.Empty<Collider>();
            }

            List<Collider> colliders = new(attachedColliders.Length);
            for (int i = 0; i < attachedColliders.Length; i++)
            {
                Collider collider = attachedColliders[i];
                if (collider != null
                    && collider.enabled
                    && !collider.isTrigger
                    && collider.attachedRigidbody == null)
                {
                    colliders.Add(collider);
                }
            }

            return colliders.ToArray();
        }

        private static bool IsCandidate(
            MeshRenderer renderer,
            List<Material> sharedMaterials,
            out long triangleCount)
        {
            triangleCount = 0L;
            if (renderer == null
                || !renderer.enabled
                || !renderer.gameObject.activeInHierarchy
                || renderer.bounds.size.magnitude > MaxCandidateBoundsSize)
            {
                return false;
            }

            MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
            Mesh mesh = meshFilter != null ? meshFilter.sharedMesh : null;
            if (mesh == null)
            {
                return false;
            }

            string meshName = mesh.name;
            if (meshName.IndexOf("Floor", StringComparison.OrdinalIgnoreCase) >= 0
                || meshName.IndexOf("Stairs", StringComparison.OrdinalIgnoreCase) >= 0
                || meshName.IndexOf("Pine", StringComparison.OrdinalIgnoreCase) >= 0
                || meshName.IndexOf("Cloud", StringComparison.OrdinalIgnoreCase) >= 0
                || meshName.IndexOf("Flag", StringComparison.OrdinalIgnoreCase) >= 0
                || meshName.IndexOf("Chandellier", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return false;
            }

            triangleCount = GetRendererTriangleCount(renderer, sharedMaterials);
            return triangleCount > 0L && triangleCount <= MaxCandidateTriangles;
        }

        private static long GetRendererTriangleCount(
            MeshRenderer renderer,
            List<Material> sharedMaterials)
        {
            MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
            Mesh mesh = meshFilter != null ? meshFilter.sharedMesh : null;
            if (mesh == null)
            {
                return 0L;
            }

            int firstSubMesh = renderer.subMeshStartIndex;
            sharedMaterials.Clear();
            renderer.GetSharedMaterials(sharedMaterials);
            int subMeshCount = sharedMaterials.Count;
            if (subMeshCount <= 0 || firstSubMesh + subMeshCount > mesh.subMeshCount)
            {
                firstSubMesh = 0;
                subMeshCount = mesh.subMeshCount;
            }

            long triangleCount = 0L;
            int lastSubMesh = firstSubMesh + subMeshCount;
            for (int subMeshIndex = firstSubMesh; subMeshIndex < lastSubMesh; subMeshIndex++)
            {
                if (mesh.GetTopology(subMeshIndex) == MeshTopology.Triangles)
                {
                    triangleCount += (long)mesh.GetIndexCount(subMeshIndex) / 3L;
                }
            }

            return triangleCount;
        }

#if UNITY_EDITOR
        public bool TryGetFirstCulledRendererForTests(out MeshRenderer renderer)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                if (culled[i] && renderers[i] != null)
                {
                    renderer = renderers[i];
                    return true;
                }
            }

            renderer = null;
            return false;
        }

        public bool TryGetFirstCulledColliderForTests(out Collider collider)
        {
            for (int rendererIndex = 0; rendererIndex < candidateColliders.Length; rendererIndex++)
            {
                Collider[] colliders = candidateColliders[rendererIndex];
                bool[] culledStates = colliderCulled[rendererIndex];
                for (int colliderIndex = 0; colliderIndex < colliders.Length; colliderIndex++)
                {
                    if (culledStates[colliderIndex] && colliders[colliderIndex] != null)
                    {
                        collider = colliders[colliderIndex];
                        return true;
                    }
                }
            }

            collider = null;
            return false;
        }

        public bool TryGetFirstColliderOnlyCullForTests(
            out MeshRenderer renderer,
            out Collider collider)
        {
            for (int rendererIndex = 0; rendererIndex < candidateColliders.Length; rendererIndex++)
            {
                if (culled[rendererIndex])
                {
                    continue;
                }

                Collider[] colliders = candidateColliders[rendererIndex];
                bool[] culledStates = colliderCulled[rendererIndex];
                for (int colliderIndex = 0; colliderIndex < colliders.Length; colliderIndex++)
                {
                    if (!culledStates[colliderIndex] || colliders[colliderIndex] == null)
                    {
                        continue;
                    }

                    renderer = renderers[rendererIndex];
                    collider = colliders[colliderIndex];
                    return renderer != null;
                }
            }

            renderer = null;
            collider = null;
            return false;
        }
#endif

        private static float ResolveCullDistance(MobilePerformanceTier tier)
        {
            return tier == MobilePerformanceTier.Low
                ? LowCullDistance
                : BalancedCullDistance;
        }

        private static float ResolveColliderCullDistance(MobilePerformanceTier tier)
        {
            return tier == MobilePerformanceTier.Low
                ? LowColliderCullDistance
                : BalancedColliderCullDistance;
        }

        private void SetCullingDistances()
        {
            cullingGroup?.SetBoundingDistances(new[] { colliderCullDistance, cullDistance });
        }

        private static Camera FindActiveCamera()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null && mainCamera.enabled && mainCamera.gameObject.activeInHierarchy)
            {
                return mainCamera;
            }

            Camera[] cameras = FindObjectsByType<Camera>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);
            for (int i = 0; i < cameras.Length; i++)
            {
                Camera camera = cameras[i];
                if (camera.enabled && camera.gameObject.activeInHierarchy)
                {
                    return camera;
                }
            }

            return null;
        }

        private void DisposeCullingGroup()
        {
            if (cullingGroup == null)
            {
                return;
            }

            cullingGroup.onStateChanged = null;
            cullingGroup.Dispose();
            cullingGroup = null;
        }

        private void OnDestroy()
        {
            cullCandidateColliders = false;
            RestoreAllRenderers();
            RestoreAllCandidateColliders();
            DisposeCullingGroup();
        }
    }
}
