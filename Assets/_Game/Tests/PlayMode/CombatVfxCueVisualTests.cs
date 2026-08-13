using System.Collections;
using DimensionBrawl.Presentation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace DimensionBrawl.Tests
{
    public sealed class CombatVfxCueVisualTests
    {
        private const string PerfectDodgeWindowPrefabPath =
            "Assets/_Game/Art/VFX/CombatCues/Prefabs/DB_VFX_PlayerPerfectDodgeWindow.prefab";
        private const string PerfectDodgeShieldMaterialPath =
            "Assets/_Game/Art/VFX/HovlSciFiEffects/Materials/DB_HovlSciFi_HexShield3shield.mat";
        private const string CombatVfxProfilePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_CombatVfxCues_ActionFoundation.asset";

        [UnityTest]
        public IEnumerator DisabledRendererColorOverridePreservesAuthoredMaterialColor()
        {
            GameObject root = GameObject.CreatePrimitive(PrimitiveType.Quad);
            root.name = "CombatVfxCueVisualColorPreservationTest";
            root.SetActive(false);
            Material material = null;
            try
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");
                Assert.IsNotNull(shader);

                Color authoredColor = new Color(0.12f, 0.72f, 1f, 0.84f);
                material = new Material(shader);
                SetMaterialColor(material, authoredColor);

                Renderer renderer = root.GetComponent<Renderer>();
                renderer.sharedMaterial = material;
                CombatVfxCueVisual visual = root.AddComponent<CombatVfxCueVisual>();
                ConfigureVisual(visual, renderer, overrideRendererColors: false);

                root.SetActive(true);
                yield return null;

                MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
                renderer.GetPropertyBlock(propertyBlock);
                Assert.IsTrue(propertyBlock.isEmpty, "Color preservation should not add renderer property overrides.");
                AssertMaterialColor(material, authoredColor);
            }
            finally
            {
                Object.DestroyImmediate(root);
                Object.DestroyImmediate(material);
            }
        }

        [Test]
        public void CanonicalPerfectDodgeWindowPreservesAuthoredRendererColors()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PerfectDodgeWindowPrefabPath);
            Assert.IsNotNull(prefab);

            CombatVfxCueVisual visual = prefab.GetComponent<CombatVfxCueVisual>();
            Assert.IsNotNull(visual);
            Assert.IsFalse(
                visual.OverridesRendererColors,
                "The canonical perfect-dodge shield should keep its authored blue material palette.");
        }

        [Test]
        public void CanonicalPerfectDodgeWindowIsCenteredAndKeepsPlayerReadable()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PerfectDodgeWindowPrefabPath);
            Assert.That(prefab, Is.Not.Null);
            Transform shieldRoot = prefab.transform.Find("PerfectDodgeVfx_HovlHexShield");
            Assert.That(shieldRoot, Is.Not.Null);
            Assert.That(
                shieldRoot.localPosition.z,
                Is.Zero.Within(0.001f),
                "The shield child must not add a second forward offset in front of the player.");

            CombatVfxCueProfile profile = AssetDatabase.LoadAssetAtPath<CombatVfxCueProfile>(CombatVfxProfilePath);
            Assert.That(profile, Is.Not.Null);
            Assert.That(profile.TryGetCue(CombatVfxCueId.PlayerPerfectDodgeWindow, out CombatVfxCue cue), Is.True);
            Assert.That(
                cue.LocalPositionOffset.z,
                Is.Zero.Within(0.001f),
                "The perfect-dodge cue must remain centered on its player anchor.");

            Material shieldMaterial = AssetDatabase.LoadAssetAtPath<Material>(PerfectDodgeShieldMaterialPath);
            Assert.That(shieldMaterial, Is.Not.Null);
            AssertShieldOpacity(shieldMaterial, "_Opacity");
            AssertShieldOpacity(shieldMaterial, "_Textureopacity");
            Assert.That(shieldMaterial.GetColor("_Color").a, Is.LessThanOrEqualTo(0.45f));
        }

        private static void ConfigureVisual(
            CombatVfxCueVisual visual,
            Renderer renderer,
            bool overrideRendererColors)
        {
            SerializedObject serializedObject = new SerializedObject(visual);
            SerializedProperty renderers = serializedObject.FindProperty("renderers");
            renderers.arraySize = 1;
            renderers.GetArrayElementAtIndex(0).objectReferenceValue = renderer;
            serializedObject.FindProperty("overrideRendererColors").boolValue = overrideRendererColors;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetMaterialColor(Material material, Color color)
        {
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }
        }

        private static void AssertMaterialColor(Material material, Color expected)
        {
            if (material.HasProperty("_BaseColor"))
            {
                AssertColor(material.GetColor("_BaseColor"), expected);
            }

            if (material.HasProperty("_Color"))
            {
                AssertColor(material.GetColor("_Color"), expected);
            }
        }

        private static void AssertColor(Color actual, Color expected)
        {
            Assert.That(actual.r, Is.EqualTo(expected.r).Within(0.001f));
            Assert.That(actual.g, Is.EqualTo(expected.g).Within(0.001f));
            Assert.That(actual.b, Is.EqualTo(expected.b).Within(0.001f));
            Assert.That(actual.a, Is.EqualTo(expected.a).Within(0.001f));
        }

        private static void AssertShieldOpacity(Material material, string propertyName)
        {
            Assert.That(material.HasProperty(propertyName), Is.True, $"Shield material is missing {propertyName}.");
            Assert.That(
                material.GetFloat(propertyName),
                Is.LessThanOrEqualTo(0.45f),
                $"{propertyName} makes the shield cover the player silhouette.");
        }
    }
}
