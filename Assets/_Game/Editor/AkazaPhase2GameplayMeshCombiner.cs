using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace DimensionBrawl.Editor
{
    /// <summary>
    /// Builds a gameplay-only render proxy for Akaza. The imported prefab, renderer
    /// GameObjects, skeleton, animation paths, and cinematic actor remain untouched.
    /// Only the renderer components on the generated Station scene instance are
    /// disabled after their geometry has been copied into deterministic mesh assets.
    /// </summary>
    internal static class AkazaPhase2GameplayMeshCombiner
    {
        internal const string CombinedRootName =
            "DB_AkazaPhase2GameplayCombinedRenderers";
        internal const string WingRendererName =
            "CHakazaA:akArm_Phase2CombinedWingStructure";

        private const string GeneratedMeshFolder =
            "Assets/_Game/Art/Characters/Bosses/Akaza/Generated";

        private static readonly GroupDefinition[] GroupDefinitions =
        {
            new GroupDefinition(
                GroupKind.WingStructure,
                WingRendererName,
                "DB_Akaza_Phase2Gameplay_WingStructure.asset",
                castsShadows: false),
            new GroupDefinition(
                GroupKind.BodySilhouette,
                "DB_AkazaPhase2Combined_BodySilhouette",
                "DB_Akaza_Phase2Gameplay_BodySilhouette.asset",
                castsShadows: true),
            new GroupDefinition(
                GroupKind.FaceHairDetail,
                "DB_AkazaPhase2Combined_FaceHairDetail",
                "DB_Akaza_Phase2Gameplay_FaceHairDetail.asset",
                castsShadows: false),
            new GroupDefinition(
                GroupKind.AuraCore,
                "DB_AkazaPhase2Combined_AuraCore",
                "DB_Akaza_Phase2Gameplay_AuraCore.asset",
                castsShadows: false)
        };

        internal static CombineResult CombineGameplayInstance(GameObject gameplayVisual)
        {
            if (gameplayVisual == null)
            {
                throw new ArgumentNullException(nameof(gameplayVisual));
            }

            Transform existingCombinedRoot = gameplayVisual.transform.Find(CombinedRootName);
            SkinnedMeshRenderer[] sourceRenderers = gameplayVisual
                .GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true)
                .Where(renderer => renderer != null
                    && renderer.sharedMesh != null
                    && (existingCombinedRoot == null
                        || !renderer.transform.IsChildOf(existingCombinedRoot)))
                .OrderBy(renderer => GetRelativePath(gameplayVisual.transform, renderer.transform),
                    StringComparer.Ordinal)
                .ToArray();
            if (sourceRenderers.Length == 0)
            {
                throw new InvalidOperationException(
                    "Akaza gameplay mesh combine did not find any source renderers.");
            }

            if (sourceRenderers.Any(renderer => renderer.sharedMesh.blendShapeCount != 0))
            {
                throw new InvalidOperationException(
                    "Akaza gameplay mesh combine cannot silently discard an authored blend shape.");
            }

            if (existingCombinedRoot != null)
            {
                UnityEngine.Object.DestroyImmediate(existingCombinedRoot.gameObject);
            }

            EnsureAssetFolder(GeneratedMeshFolder);
            GameObject combinedRootObject = new GameObject(CombinedRootName);
            Transform combinedRoot = combinedRootObject.transform;
            combinedRoot.SetParent(gameplayVisual.transform, worldPositionStays: false);
            combinedRoot.localPosition = Vector3.zero;
            combinedRoot.localRotation = Quaternion.identity;
            combinedRoot.localScale = Vector3.one;

            Transform skeletonRoot = FindSkeletonRoot(gameplayVisual.transform);
            var combinedRenderers = new List<SkinnedMeshRenderer>(GroupDefinitions.Length);
            try
            {
                foreach (GroupDefinition definition in GroupDefinitions)
                {
                    SkinnedMeshRenderer[] groupSources = sourceRenderers
                        .Where(renderer => Classify(renderer.name) == definition.Kind)
                        .ToArray();
                    if (groupSources.Length == 0)
                    {
                        throw new InvalidOperationException(
                            $"Akaza gameplay mesh group {definition.Kind} has no source renderer.");
                    }

                    GameObject rendererObject = new GameObject(definition.RendererName);
                    Transform rendererTransform = rendererObject.transform;
                    rendererTransform.SetParent(combinedRoot, worldPositionStays: false);
                    rendererTransform.localPosition = Vector3.zero;
                    rendererTransform.localRotation = Quaternion.identity;
                    rendererTransform.localScale = Vector3.one;

                    Mesh mesh = LoadOrCreateMeshAsset(definition.MeshAssetName);
                    CombinedMeshData combined = BuildCombinedMesh(
                        groupSources,
                        rendererTransform,
                        skeletonRoot,
                        mesh);

                    SkinnedMeshRenderer renderer = rendererObject.AddComponent<SkinnedMeshRenderer>();
                    renderer.sharedMesh = mesh;
                    renderer.sharedMaterials = combined.Materials;
                    renderer.bones = combined.Bones;
                    renderer.rootBone = skeletonRoot;
                    renderer.localBounds = combined.CullingBounds;
                    renderer.quality = SkinQuality.Auto;
                    renderer.updateWhenOffscreen = false;
                    renderer.skinnedMotionVectors = false;
                    renderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
                    renderer.shadowCastingMode = definition.CastsShadows
                        ? ShadowCastingMode.On
                        : ShadowCastingMode.Off;
                    renderer.receiveShadows = definition.CastsShadows;
                    renderer.allowOcclusionWhenDynamic = true;
                    renderer.enabled = true;
                    EditorUtility.SetDirty(renderer);
                    combinedRenderers.Add(renderer);
                }

                ValidateGeometryCopy(sourceRenderers, combinedRenderers);
                foreach (SkinnedMeshRenderer sourceRenderer in sourceRenderers)
                {
                    sourceRenderer.enabled = false;
                    sourceRenderer.updateWhenOffscreen = false;
                    sourceRenderer.skinnedMotionVectors = false;
                    sourceRenderer.motionVectorGenerationMode =
                        MotionVectorGenerationMode.ForceNoMotion;
                    EditorUtility.SetDirty(sourceRenderer);
                }

                ValidateRuntimeBudget(sourceRenderers, combinedRenderers);
                EditorUtility.SetDirty(combinedRootObject);
                return new CombineResult(sourceRenderers, combinedRenderers.ToArray());
            }
            catch
            {
                if (combinedRootObject != null)
                {
                    UnityEngine.Object.DestroyImmediate(combinedRootObject);
                }

                throw;
            }
        }

        private static CombinedMeshData BuildCombinedMesh(
            IReadOnlyList<SkinnedMeshRenderer> sources,
            Transform target,
            Transform fallbackBone,
            Mesh targetMesh)
        {
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var tangents = new List<Vector4>();
            var colors = new List<Color32>();
            var uvs = Enumerable.Range(0, 8)
                .Select(_ => new List<Vector4>())
                .ToArray();
            var hasColors = false;
            var hasUvs = new bool[uvs.Length];
            var boneWeights = new List<BoneWeight>();
            var bones = new List<Transform>();
            var bindposes = new List<Matrix4x4>();
            var materials = new List<Material>();
            var trianglesByMaterial = new List<List<int>>();
            var materialIndices = new Dictionary<Material, int>();
            Bounds cullingBounds = default;
            bool hasCullingBounds = false;

            foreach (SkinnedMeshRenderer source in sources)
            {
                Mesh sourceMesh = source.sharedMesh;
                int vertexCount = sourceMesh.vertexCount;
                if (vertexCount == 0)
                {
                    throw new InvalidOperationException(
                        $"Akaza source renderer {source.name} has an empty mesh.");
                }

                Vector3[] sourceVertices = sourceMesh.vertices;
                Vector3[] sourceNormals = sourceMesh.normals;
                Vector4[] sourceTangents = sourceMesh.tangents;
                Color32[] sourceColors = sourceMesh.colors32;
                BoneWeight[] sourceWeights = sourceMesh.boneWeights;
                if (sourceVertices.Length != vertexCount
                    || sourceNormals.Length != vertexCount
                    || sourceTangents.Length != vertexCount)
                {
                    throw new InvalidOperationException(
                        $"Akaza source renderer {source.name} must retain positions, normals, and tangents.");
                }

                if (sourceColors.Length != 0 && sourceColors.Length != vertexCount)
                {
                    throw new InvalidOperationException(
                        $"Akaza source renderer {source.name} has an incomplete color channel.");
                }

                int vertexOffset = vertices.Count;
                Matrix4x4 sourceToTarget =
                    target.worldToLocalMatrix * source.transform.localToWorldMatrix;
                Matrix4x4 normalMatrix = sourceToTarget.inverse.transpose;
                bool reversesWinding = sourceToTarget.determinant < 0f;
                for (int vertexIndex = 0; vertexIndex < vertexCount; vertexIndex++)
                {
                    Vector3 normal = normalMatrix.MultiplyVector(sourceNormals[vertexIndex]).normalized;
                    Vector4 sourceTangent = sourceTangents[vertexIndex];
                    Vector3 tangentDirection = sourceToTarget.MultiplyVector(
                        new Vector3(sourceTangent.x, sourceTangent.y, sourceTangent.z));
                    tangentDirection -= normal * Vector3.Dot(normal, tangentDirection);
                    tangentDirection.Normalize();

                    vertices.Add(sourceToTarget.MultiplyPoint3x4(sourceVertices[vertexIndex]));
                    normals.Add(normal);
                    tangents.Add(new Vector4(
                        tangentDirection.x,
                        tangentDirection.y,
                        tangentDirection.z,
                        reversesWinding ? -sourceTangent.w : sourceTangent.w));
                    colors.Add(sourceColors.Length == vertexCount
                        ? sourceColors[vertexIndex]
                        : new Color32(255, 255, 255, 255));
                }

                hasColors |= sourceColors.Length == vertexCount;
                AppendUvs(sourceMesh, vertexCount, uvs, hasUvs);
                AppendBoneWeights(
                    source,
                    sourceMesh,
                    sourceWeights,
                    sourceToTarget,
                    fallbackBone,
                    boneWeights,
                    bones,
                    bindposes);
                AppendTriangles(
                    source,
                    vertexOffset,
                    reversesWinding,
                    materials,
                    trianglesByMaterial,
                    materialIndices);

                Bounds transformedBounds = TransformBounds(source.localBounds, sourceToTarget);
                if (!hasCullingBounds)
                {
                    cullingBounds = transformedBounds;
                    hasCullingBounds = true;
                }
                else
                {
                    cullingBounds.Encapsulate(transformedBounds.min);
                    cullingBounds.Encapsulate(transformedBounds.max);
                }
            }

            if (vertices.Count != boneWeights.Count || bones.Count != bindposes.Count)
            {
                throw new InvalidOperationException(
                    "Akaza combined skinning data did not preserve a weight for every vertex.");
            }

            targetMesh.Clear(keepVertexLayout: false);
            targetMesh.name = System.IO.Path.GetFileNameWithoutExtension(
                AssetDatabase.GetAssetPath(targetMesh));
            targetMesh.indexFormat = vertices.Count > ushort.MaxValue
                ? IndexFormat.UInt32
                : IndexFormat.UInt16;
            targetMesh.SetVertices(vertices);
            targetMesh.SetNormals(normals);
            targetMesh.SetTangents(tangents);
            if (hasColors)
            {
                targetMesh.SetColors(colors);
            }

            for (int channel = 0; channel < uvs.Length; channel++)
            {
                if (hasUvs[channel])
                {
                    targetMesh.SetUVs(channel, uvs[channel]);
                }
            }

            targetMesh.bindposes = bindposes.ToArray();
            targetMesh.boneWeights = boneWeights.ToArray();
            targetMesh.subMeshCount = materials.Count;
            for (int subMesh = 0; subMesh < materials.Count; subMesh++)
            {
                targetMesh.SetTriangles(
                    trianglesByMaterial[subMesh],
                    subMesh,
                    calculateBounds: false);
            }

            targetMesh.RecalculateBounds();
            EditorUtility.SetDirty(targetMesh);
            if (!hasCullingBounds)
            {
                cullingBounds = targetMesh.bounds;
            }
            else
            {
                // Source renderer bounds preserve authored animation padding, but
                // they are expressed around the original renderer hierarchy and
                // can be smaller than the transformed vertex envelope after the
                // renderers are merged under one root. Always include the actual
                // combined geometry so setup alignment cannot over-scale the boss
                // from an artificially small culling box.
                cullingBounds.Encapsulate(targetMesh.bounds.min);
                cullingBounds.Encapsulate(targetMesh.bounds.max);
            }

            cullingBounds.Expand(0.08f);
            return new CombinedMeshData(
                materials.ToArray(),
                bones.ToArray(),
                cullingBounds);
        }

        private static void AppendUvs(
            Mesh sourceMesh,
            int vertexCount,
            IReadOnlyList<List<Vector4>> destination,
            bool[] hasUvs)
        {
            for (int channel = 0; channel < destination.Count; channel++)
            {
                var sourceUvs = new List<Vector4>(vertexCount);
                sourceMesh.GetUVs(channel, sourceUvs);
                if (sourceUvs.Count != 0 && sourceUvs.Count != vertexCount)
                {
                    throw new InvalidOperationException(
                        $"Akaza mesh {sourceMesh.name} has an incomplete UV{channel} channel.");
                }

                bool hasSourceUv = sourceUvs.Count == vertexCount;
                hasUvs[channel] |= hasSourceUv;
                if (hasSourceUv)
                {
                    destination[channel].AddRange(sourceUvs);
                }
                else
                {
                    for (int i = 0; i < vertexCount; i++)
                    {
                        destination[channel].Add(Vector4.zero);
                    }
                }
            }
        }

        private static void AppendBoneWeights(
            SkinnedMeshRenderer source,
            Mesh sourceMesh,
            IReadOnlyList<BoneWeight> sourceWeights,
            Matrix4x4 sourceToTarget,
            Transform fallbackBone,
            ICollection<BoneWeight> destinationWeights,
            List<Transform> destinationBones,
            List<Matrix4x4> destinationBindposes)
        {
            int vertexCount = sourceMesh.vertexCount;
            if (sourceWeights.Count == 0)
            {
                Transform rigidBone = source.rootBone != null ? source.rootBone : fallbackBone;
                Matrix4x4 rigidBindpose =
                    rigidBone.worldToLocalMatrix * source.transform.localToWorldMatrix
                    * sourceToTarget.inverse;
                int rigidBoneIndex = FindOrAddBoneBinding(
                    rigidBone,
                    rigidBindpose,
                    destinationBones,
                    destinationBindposes);
                for (int vertexIndex = 0; vertexIndex < vertexCount; vertexIndex++)
                {
                    destinationWeights.Add(new BoneWeight
                    {
                        boneIndex0 = rigidBoneIndex,
                        weight0 = 1f
                    });
                }

                return;
            }

            if (sourceWeights.Count != vertexCount
                || sourceMesh.bindposes.Length != source.bones.Length)
            {
                throw new InvalidOperationException(
                    $"Akaza source renderer {source.name} has inconsistent bone weights or bindposes.");
            }

            Transform[] sourceBones = source.bones;
            Matrix4x4[] sourceBindposes = sourceMesh.bindposes;
            int[] remap = Enumerable.Repeat(-1, sourceBones.Length).ToArray();
            int RemapBone(int sourceBoneIndex)
            {
                if (sourceBoneIndex < 0 || sourceBoneIndex >= sourceBones.Length)
                {
                    throw new InvalidOperationException(
                        $"Akaza source renderer {source.name} references bone {sourceBoneIndex} out of range.");
                }

                if (remap[sourceBoneIndex] >= 0)
                {
                    return remap[sourceBoneIndex];
                }

                Transform bone = sourceBones[sourceBoneIndex];
                if (bone == null)
                {
                    throw new InvalidOperationException(
                        $"Akaza source renderer {source.name} references a missing bone.");
                }

                // Original world skinning is bone.localToWorld * sourceBindpose * sourceVertex.
                // Vertices above are moved by sourceToTarget, so multiplying the bindpose by
                // its inverse preserves that exact contract in the target renderer space.
                Matrix4x4 targetBindpose =
                    sourceBindposes[sourceBoneIndex] * sourceToTarget.inverse;
                remap[sourceBoneIndex] = FindOrAddBoneBinding(
                    bone,
                    targetBindpose,
                    destinationBones,
                    destinationBindposes);
                return remap[sourceBoneIndex];
            }

            foreach (BoneWeight sourceWeight in sourceWeights)
            {
                BoneWeight remapped = sourceWeight;
                remapped.boneIndex0 = sourceWeight.weight0 > 0f
                    ? RemapBone(sourceWeight.boneIndex0)
                    : 0;
                remapped.boneIndex1 = sourceWeight.weight1 > 0f
                    ? RemapBone(sourceWeight.boneIndex1)
                    : 0;
                remapped.boneIndex2 = sourceWeight.weight2 > 0f
                    ? RemapBone(sourceWeight.boneIndex2)
                    : 0;
                remapped.boneIndex3 = sourceWeight.weight3 > 0f
                    ? RemapBone(sourceWeight.boneIndex3)
                    : 0;
                destinationWeights.Add(remapped);
            }
        }

        private static int FindOrAddBoneBinding(
            Transform bone,
            Matrix4x4 bindpose,
            List<Transform> bones,
            List<Matrix4x4> bindposes)
        {
            for (int i = 0; i < bones.Count; i++)
            {
                if (bones[i] == bone && MatrixApproximately(bindposes[i], bindpose))
                {
                    return i;
                }
            }

            bones.Add(bone);
            bindposes.Add(bindpose);
            return bones.Count - 1;
        }

        private static void AppendTriangles(
            SkinnedMeshRenderer source,
            int vertexOffset,
            bool reversesWinding,
            List<Material> materials,
            List<List<int>> trianglesByMaterial,
            IDictionary<Material, int> materialIndices)
        {
            Mesh sourceMesh = source.sharedMesh;
            Material[] sourceMaterials = source.sharedMaterials;
            if (sourceMaterials.Length < sourceMesh.subMeshCount)
            {
                throw new InvalidOperationException(
                    $"Akaza source renderer {source.name} has fewer materials than submeshes.");
            }

            for (int subMesh = 0; subMesh < sourceMesh.subMeshCount; subMesh++)
            {
                if (sourceMesh.GetTopology(subMesh) != MeshTopology.Triangles)
                {
                    throw new InvalidOperationException(
                        $"Akaza source renderer {source.name} contains a non-triangle submesh.");
                }

                Material material = sourceMaterials[subMesh];
                if (material == null)
                {
                    throw new InvalidOperationException(
                        $"Akaza source renderer {source.name} contains a missing material.");
                }

                if (!materialIndices.TryGetValue(material, out int materialIndex))
                {
                    materialIndex = materials.Count;
                    materialIndices.Add(material, materialIndex);
                    materials.Add(material);
                    trianglesByMaterial.Add(new List<int>());
                }

                int[] triangles = sourceMesh.GetTriangles(subMesh, applyBaseVertex: true);
                if (triangles.Length % 3 != 0)
                {
                    throw new InvalidOperationException(
                        $"Akaza source renderer {source.name} has malformed triangle indices.");
                }

                List<int> destination = trianglesByMaterial[materialIndex];
                for (int triangle = 0; triangle < triangles.Length; triangle += 3)
                {
                    int first = triangles[triangle] + vertexOffset;
                    int second = triangles[triangle + 1] + vertexOffset;
                    int third = triangles[triangle + 2] + vertexOffset;
                    destination.Add(first);
                    destination.Add(reversesWinding ? third : second);
                    destination.Add(reversesWinding ? second : third);
                }
            }
        }

        private static Mesh LoadOrCreateMeshAsset(string assetName)
        {
            string assetPath = $"{GeneratedMeshFolder}/{assetName}";
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
            if (mesh != null)
            {
                return mesh;
            }

            mesh = new Mesh
            {
                name = System.IO.Path.GetFileNameWithoutExtension(assetName)
            };
            AssetDatabase.CreateAsset(mesh, assetPath);
            return mesh;
        }

        private static Transform FindSkeletonRoot(Transform visualRoot)
        {
            Transform reference = visualRoot
                .GetComponentsInChildren<Transform>(includeInactive: true)
                .FirstOrDefault(transform => transform.name == "CHakazaA:Reference");
            return reference != null ? reference : visualRoot;
        }

        private static GroupKind Classify(string rendererName)
        {
            if (rendererName == "CHakazaA:BackParts"
                || rendererName.StartsWith("CHakazaA:akArm", StringComparison.Ordinal)
                || rendererName.StartsWith("CHakazaA:akWp_", StringComparison.Ordinal))
            {
                return GroupKind.WingStructure;
            }

            if (rendererName.IndexOf("eyeHighLight", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return GroupKind.AuraCore;
            }

            string lowerName = rendererName.ToLowerInvariant();
            if (lowerName.Contains("hair")
                || lowerName.Contains("head")
                || lowerName.Contains("eye")
                || lowerName.Contains("mayu")
                || lowerName.Contains("mimi")
                || lowerName.Contains("tongue")
                || lowerName.Contains("tooth"))
            {
                return GroupKind.FaceHairDetail;
            }

            return GroupKind.BodySilhouette;
        }

        private static Bounds TransformBounds(Bounds source, Matrix4x4 matrix)
        {
            Vector3 center = matrix.MultiplyPoint3x4(source.center);
            Vector3 extents = source.extents;
            Vector3 axisX = matrix.MultiplyVector(new Vector3(extents.x, 0f, 0f));
            Vector3 axisY = matrix.MultiplyVector(new Vector3(0f, extents.y, 0f));
            Vector3 axisZ = matrix.MultiplyVector(new Vector3(0f, 0f, extents.z));
            Vector3 transformedExtents = new Vector3(
                Mathf.Abs(axisX.x) + Mathf.Abs(axisY.x) + Mathf.Abs(axisZ.x),
                Mathf.Abs(axisX.y) + Mathf.Abs(axisY.y) + Mathf.Abs(axisZ.y),
                Mathf.Abs(axisX.z) + Mathf.Abs(axisY.z) + Mathf.Abs(axisZ.z));
            return new Bounds(center, transformedExtents * 2f);
        }

        private static bool MatrixApproximately(Matrix4x4 left, Matrix4x4 right)
        {
            const float Epsilon = 0.0001f;
            for (int row = 0; row < 4; row++)
            {
                for (int column = 0; column < 4; column++)
                {
                    if (Mathf.Abs(left[row, column] - right[row, column]) > Epsilon)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static void ValidateGeometryCopy(
            IReadOnlyCollection<SkinnedMeshRenderer> sources,
            IReadOnlyCollection<SkinnedMeshRenderer> combined)
        {
            int sourceVertices = sources.Sum(renderer => renderer.sharedMesh.vertexCount);
            int combinedVertices = combined.Sum(renderer => renderer.sharedMesh.vertexCount);
            long sourceIndices = sources.Sum(renderer =>
                Enumerable.Range(0, renderer.sharedMesh.subMeshCount)
                    .Sum(subMesh => (long)renderer.sharedMesh.GetIndexCount(subMesh)));
            long combinedIndices = combined.Sum(renderer =>
                Enumerable.Range(0, renderer.sharedMesh.subMeshCount)
                    .Sum(subMesh => (long)renderer.sharedMesh.GetIndexCount(subMesh)));
            var sourceMaterials = new HashSet<Material>(
                sources.SelectMany(renderer => renderer.sharedMaterials));
            var combinedMaterials = new HashSet<Material>(
                combined.SelectMany(renderer => renderer.sharedMaterials));
            if (sourceVertices != combinedVertices
                || sourceIndices != combinedIndices
                || !sourceMaterials.SetEquals(combinedMaterials))
            {
                throw new InvalidOperationException(
                    "Akaza gameplay mesh combine did not preserve geometry and canonical materials.");
            }
        }

        private static void ValidateRuntimeBudget(
            IReadOnlyCollection<SkinnedMeshRenderer> sources,
            IReadOnlyCollection<SkinnedMeshRenderer> combined)
        {
            int rendererCount = combined.Count(renderer => renderer.enabled);
            int materialSlots = combined
                .Where(renderer => renderer.enabled)
                .Sum(renderer => renderer.sharedMaterials.Length);
            int shadowCasters = combined.Count(renderer =>
                renderer.enabled && renderer.shadowCastingMode != ShadowCastingMode.Off);
            if (sources.Any(renderer => renderer.enabled)
                || rendererCount > 4
                || materialSlots > 12
                || shadowCasters > 1)
            {
                throw new InvalidOperationException(
                    $"Akaza gameplay render budget failed: renderers={rendererCount}, "
                    + $"materialSlots={materialSlots}, shadowCasters={shadowCasters}.");
            }
        }

        private static string GetRelativePath(Transform root, Transform child)
        {
            var names = new Stack<string>();
            Transform current = child;
            while (current != null && current != root)
            {
                names.Push(current.name);
                current = current.parent;
            }

            return string.Join("/", names);
        }

        private static void EnsureAssetFolder(string folderPath)
        {
            string[] pieces = folderPath.Split('/');
            string current = pieces[0];
            for (int i = 1; i < pieces.Length; i++)
            {
                string next = current + "/" + pieces[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, pieces[i]);
                }

                current = next;
            }
        }

        private enum GroupKind
        {
            WingStructure,
            BodySilhouette,
            FaceHairDetail,
            AuraCore
        }

        private readonly struct GroupDefinition
        {
            public GroupDefinition(
                GroupKind kind,
                string rendererName,
                string meshAssetName,
                bool castsShadows)
            {
                Kind = kind;
                RendererName = rendererName;
                MeshAssetName = meshAssetName;
                CastsShadows = castsShadows;
            }

            public GroupKind Kind { get; }
            public string RendererName { get; }
            public string MeshAssetName { get; }
            public bool CastsShadows { get; }
        }

        private readonly struct CombinedMeshData
        {
            public CombinedMeshData(
                Material[] materials,
                Transform[] bones,
                Bounds cullingBounds)
            {
                Materials = materials;
                Bones = bones;
                CullingBounds = cullingBounds;
            }

            public Material[] Materials { get; }
            public Transform[] Bones { get; }
            public Bounds CullingBounds { get; }
        }

        internal readonly struct CombineResult
        {
            public CombineResult(
                SkinnedMeshRenderer[] sourceRenderers,
                SkinnedMeshRenderer[] combinedRenderers)
            {
                SourceRenderers = sourceRenderers;
                CombinedRenderers = combinedRenderers;
            }

            public SkinnedMeshRenderer[] SourceRenderers { get; }
            public SkinnedMeshRenderer[] CombinedRenderers { get; }
        }
    }
}
