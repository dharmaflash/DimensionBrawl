using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace DimensionBrawl.Editor.CityHeroPocket.Tests
{
    public sealed class TokyoStreetCuratedAssetAdmissionTests
    {
        private const string AssetRoot =
            "Assets/_Game/Art/Environment/CityHeroPocket/TokyoStreet";
        private const string ManifestPath =
            "Assets/_Game/Art/Environment/CityHeroPocket/ThirdParty/" +
            "TokyoStreet_CurationManifest.json";
        private const int ExpectedLeafAssetCount = 169;
        private const int ExpectedTextureCount = 95;
        private const int ExpectedAuthoredAlbedoCount = 24;
        private const int ExpectedReducedMapCount = 71;

        private static readonly string[] SeedPaths =
        {
            "Prefabs/BG_House_05.prefab",
            "Prefabs/Decals/Crossroad_02_Marking.prefab",
            "Prefabs/Decals/Pedestrian_Crossing_01_Marking.prefab",
            "Prefabs/Environment/Bicycle_03.prefab",
            "Prefabs/Environment/Mini_Truck.prefab",
            "Prefabs/Environment/Signboard_05.prefab",
            "Prefabs/Environment/Tiers_Conditioners.prefab",
            "Prefabs/Environment/Vending_Machine_01.prefab",
            "Prefabs/House/Balcony_02.prefab",
            "Prefabs/House/External_Staircase_01.prefab",
            "Prefabs/House/Interior/Showcase_Store_01.prefab",
            "Prefabs/House/Interior/Wall_4m_01.prefab",
            "Prefabs/House/Interior/Windows_V01/Wall_Windows_01.3.prefab",
            "Prefabs/House/Visor_01.prefab",
            "Prefabs/House/Window_Blinds_03.prefab",
            "Prefabs/Street/Electric_Post_Big_01.prefab",
            "Prefabs/Street/Kerb_Stone_5m_01.prefab",
            "Prefabs/Street/Kerb_Stone_Angle_01.prefab",
            "Prefabs/Street/Step_Corner_5m_02.prefab",
            "Prefabs/Street/Traffic_Light_01.prefab",
            "Prefabs/Street/Traffic_Light_Pedestrian_02.prefab",
            "Prefabs/Street/Wires_10m_01.prefab",
            "Prefabs/Street/Wires_10m_02.prefab",
            "Prefabs/Street/Wires_10m_03.prefab",
        };

        [Serializable]
        private sealed class CurationReport
        {
            public CuratedAsset[] assets;
        }

        [Serializable]
        private sealed class CuratedAsset
        {
            public string target_path;
            public string guid;
            public string target_sha256;
            public long target_bytes;
            public int target_width;
            public int target_height;
        }

        [Test]
        public void CuratedClosureMatchesReviewedManifestAndHashes()
        {
            TextAsset manifestAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(ManifestPath);
            Assert.That(manifestAsset, Is.Not.Null, $"Missing curation manifest: {ManifestPath}");

            CurationReport report = JsonUtility.FromJson<CurationReport>(manifestAsset.text);
            Assert.That(report, Is.Not.Null);
            Assert.That(report.assets, Is.Not.Null);
            Assert.That(report.assets, Has.Length.EqualTo(ExpectedLeafAssetCount));

            var paths = new HashSet<string>(StringComparer.Ordinal);
            var guids = new HashSet<string>(StringComparer.Ordinal);
            foreach (CuratedAsset asset in report.assets)
            {
                Assert.That(asset, Is.Not.Null);
                Assert.That(asset.target_path, Does.StartWith(AssetRoot + "/"));
                Assert.That(paths.Add(asset.target_path), Is.True,
                    $"Duplicate manifest path: {asset.target_path}");
                Assert.That(guids.Add(asset.guid), Is.True,
                    $"Duplicate manifest GUID: {asset.guid}");
                Assert.That(AssetDatabase.GUIDToAssetPath(asset.guid), Is.EqualTo(asset.target_path),
                    $"GUID/path drift: {asset.guid}");

                string fullPath = Path.GetFullPath(asset.target_path);
                Assert.That(File.Exists(fullPath), Is.True, $"Missing curated file: {asset.target_path}");
                Assert.That(new FileInfo(fullPath).Length, Is.EqualTo(asset.target_bytes),
                    $"Curated byte count drift: {asset.target_path}");
                Assert.That(ComputeSha256(fullPath), Is.EqualTo(asset.target_sha256),
                    $"Curated hash drift: {asset.target_path}");
            }

            string[] importedPaths = FindLeafAssetPaths();
            Assert.That(importedPaths, Has.Length.EqualTo(ExpectedLeafAssetCount));
            Assert.That(importedPaths, Is.EquivalentTo(paths));
        }

        [Test]
        public void CuratedClosureHasNoRawPackageOrUnsupportedDependencies()
        {
            string[] paths = FindLeafAssetPaths();
            Assert.That(paths.Any(path => HasExtension(path, ".tga")), Is.False);
            Assert.That(paths.Any(path => HasExtension(path, ".cs")), Is.False);
            Assert.That(paths.Any(path => HasExtension(path, ".shader")), Is.False);
            Assert.That(paths.Any(path => HasExtension(path, ".shadergraph")), Is.False);
            Assert.That(paths.Any(path => HasExtension(path, ".unity")), Is.False);
            Assert.That(paths.Any(path => path.Contains("Roof_Wall_04", StringComparison.Ordinal)),
                Is.False);
            Assert.That(paths.Any(path => path.Contains("Wall_Door_04", StringComparison.Ordinal)),
                Is.False);
            Assert.That(paths.Any(path => path.Contains("Flowers", StringComparison.OrdinalIgnoreCase)),
                Is.False);

            string[] dependencies = AssetDatabase.GetDependencies(paths, true)
                .Where(path => path.StartsWith("Assets/", StringComparison.Ordinal) &&
                               !path.StartsWith(AssetRoot + "/", StringComparison.Ordinal))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray();
            Assert.That(dependencies, Is.Empty,
                "Curated Tokyo Street closure escaped its product asset root.");
        }

        [Test]
        public void CuratedTexturesUseReviewedTwoTierResolutionPolicy()
        {
            string[] texturePaths = FindLeafAssetPaths()
                .Where(path => HasExtension(path, ".png"))
                .ToArray();
            Assert.That(texturePaths, Has.Length.EqualTo(ExpectedTextureCount));

            int authoredAlbedoCount = 0;
            int reducedMapCount = 0;
            foreach (string path in texturePaths)
            {
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                Assert.That(texture, Is.Not.Null, $"Texture failed to load: {path}");
                bool isAuthoredAlbedo = Path.GetFileNameWithoutExtension(path)
                    .EndsWith("_A", StringComparison.OrdinalIgnoreCase);
                int expectedSize = isAuthoredAlbedo ? 2048 : 1024;
                Assert.That(texture.width, Is.EqualTo(expectedSize), path);
                Assert.That(texture.height, Is.EqualTo(expectedSize), path);
                if (isAuthoredAlbedo)
                    authoredAlbedoCount++;
                else
                    reducedMapCount++;
            }

            Assert.That(authoredAlbedoCount, Is.EqualTo(ExpectedAuthoredAlbedoCount));
            Assert.That(reducedMapCount, Is.EqualTo(ExpectedReducedMapCount));
        }

        [Test]
        public void CuratedPrefabsAndMaterialsLoadWithoutPresentationFailures()
        {
            foreach (string relativePath in SeedPaths)
            {
                string path = AssetRoot + "/" + relativePath;
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                Assert.That(prefab, Is.Not.Null, $"Missing reviewed seed prefab: {path}");
            }

            foreach (string path in FindLeafAssetPaths().Where(path => HasExtension(path, ".prefab")))
            {
                GameObject root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    foreach (Transform transform in root.GetComponentsInChildren<Transform>(true))
                    {
                        Assert.That(
                            GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject),
                            Is.Zero,
                            $"Missing script: {path} -> {HierarchyPath(transform)}");
                    }

                    foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
                    {
                        for (int index = 0; index < renderer.sharedMaterials.Length; index++)
                        {
                            Assert.That(renderer.sharedMaterials[index], Is.Not.Null,
                                $"Null material: {path} -> {HierarchyPath(renderer.transform)}[{index}]");
                        }
                    }
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }

            foreach (string path in FindLeafAssetPaths().Where(path => HasExtension(path, ".mat")))
            {
                Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
                Assert.That(material, Is.Not.Null, $"Material failed to load: {path}");
                Assert.That(material.shader, Is.Not.Null, $"Material shader is null: {path}");
                Assert.That(material.shader.name, Is.Not.EqualTo("Hidden/InternalErrorShader"), path);
                Assert.That(material.shader.isSupported, Is.True,
                    $"Material shader is unsupported: {path} -> {material.shader.name}");
            }
        }

        private static string[] FindLeafAssetPaths()
        {
            return AssetDatabase.FindAssets(string.Empty, new[] { AssetRoot })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => !string.IsNullOrEmpty(path) && !AssetDatabase.IsValidFolder(path))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray();
        }

        private static string ComputeSha256(string path)
        {
            using SHA256 sha = SHA256.Create();
            using FileStream stream = File.OpenRead(path);
            return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty);
        }

        private static bool HasExtension(string path, string extension) =>
            path.EndsWith(extension, StringComparison.OrdinalIgnoreCase);

        private static string HierarchyPath(Transform transform)
        {
            var names = new Stack<string>();
            for (Transform current = transform; current != null; current = current.parent)
                names.Push(current.name);
            return string.Join("/", names);
        }
    }
}
