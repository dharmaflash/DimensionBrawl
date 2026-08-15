using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using DimensionBrawl.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace DimensionBrawl.Tests
{
    public sealed class CombatHudCelestialV2AssemblyPlayModeTests
    {
        private const string StagingPrefabPath =
            "Assets/_Game/UI/CombatHud/PF_UI_CombatHud_CelestialV2_Staging.prefab";
        private const string AssemblySpecPath =
            "Assets/_Game/UI/CombatHud/CombatHudCelestialV2AssemblySpec.json";
        private const string V22ArtRoot =
            "Assets/_Game/UI/CombatHud/Art/CelestialHudV2/Runtime/";
        private const string StationScenePath =
            "Assets/_Game/Scenes/OlympusStationCombatStage.unity";
        private const string CorridorScenePath =
            "Assets/_Game/Scenes/OlympusCorridorInvasionStage.unity";
        private const string CourtyardScenePath =
            "Assets/_Game/Scenes/OlympusCourtyardDrillStage.unity";
        private const string CombatHudPrefabGuid = "4e5297b5734b6664b935ffb1ae9b48b6";
        private const string PointerInputScriptGuid = "e764d6dd84658b34d9df199b296e940b";
        private const string VirtualJoystickScriptGuid = "d85f5878113320a48a4d953bd098c390";
        private const float Tolerance = 0.1f;

        private static readonly (string ButtonName, int ActionId)[] SceneActionBindings =
        {
            ("BasicAttackButton", 100),
            ("DodgeButton", 110),
            ("Skill1Button", 120),
            ("UltimateButton", 130),
            ("SummonSlot1Button", 200),
            ("SummonSlot2Button", 210),
            ("SummonSlot3Button", 220)
        };

        [Serializable]
        private sealed class AssemblySpec
        {
            public int version;
            public string artRoot;
            public SpriteSpec[] sprites = Array.Empty<SpriteSpec>();
        }

        [Serializable]
        private sealed class SpriteSpec
        {
            public string role;
            public string path;
            public bool required;
        }

        [Test]
        public void AssemblySpecUsesUniqueVersionedRolePathsWithoutDirectoryCountContract()
        {
            AssemblySpec spec = RequireAssemblySpec();
            Assert.That(spec.version, Is.EqualTo(CombatHudCelestialV2LayoutProfile.LayoutVersion));
            Assert.That(
                spec.artRoot.Replace('\\', '/').TrimEnd('/'),
                Is.EqualTo(V22ArtRoot.TrimEnd('/')));

            string[] roles = spec.sprites.Select(entry => entry.role).ToArray();
            string[] paths = spec.sprites.Select(entry => entry.path).ToArray();
            Assert.That(roles, Is.Unique);
            Assert.That(paths, Is.Unique);
            Assert.That(spec.sprites.All(entry => entry.required), Is.True);
            Assert.That(paths.All(path => path.EndsWith(".png", StringComparison.OrdinalIgnoreCase)), Is.True);
            Assert.That(paths.All(path => !path.Contains("..", StringComparison.Ordinal)), Is.True);

            string[] approvedRoles =
            {
                "objective.frame",
                "boss.nameTab", "boss.hpTrack", "boss.hpFill", "boss.costTrack", "boss.costFill",
                "pause.plate", "pause.glyph",
                "action.plate", "action.readyArc", "action.cooldownDisc",
                "action.weaponSwapGlyph", "action.ultimateGlyph", "action.dashGlyph", "action.rangedGlyph",
                "summon.mask", "summon.frame1", "summon.frame2", "summon.frame3",
                "summon.stateArc", "summon.costTab",
                "summon.portrait1", "summon.portrait2", "summon.portrait3",
                "player.portraitFrame", "player.portraitMask", "player.portrait",
                "player.hpTrack", "player.hpFill", "player.enTrack", "player.enFill",
                "player.ammoPlate", "player.bulletGlyph", "player.modeGlyph",
                "joystick.base", "joystick.knob",
                "reticle.needle", "reticle.dot"
            };
            Assert.That(spec.sprites, Has.Length.EqualTo(38));
            CollectionAssert.AreEquivalent(approvedRoles, roles);

            for (int i = 0; i < spec.sprites.Length; i++)
            {
                SpriteSpec entry = spec.sprites[i];
                string assetPath = ResolveSpriteAssetPath(spec, entry);
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                Assert.That(sprite, Is.Not.Null, $"Role {entry.role} did not import as Sprite: {assetPath}");
                TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                Assert.That(importer, Is.Not.Null, $"Role {entry.role} has no TextureImporter.");
                Assert.That(importer.textureType, Is.EqualTo(TextureImporterType.Sprite), entry.role);
                Assert.That(importer.alphaIsTransparency, Is.True, entry.role);
            }
        }

        [Test]
        public void LayoutProfileMatchesApprovedMobileComposition()
        {
            AssertRect(CombatHudCelestialV2LayoutProfile.ObjectiveFrame, new Rect(0f, 327f, 806f, 167f));
            AssertRect(CombatHudCelestialV2LayoutProfile.ObjectiveText, new Rect(88f, 327f, 650f, 167f));
            AssertRect(CombatHudCelestialV2LayoutProfile.PauseHit, new Rect(2368.5f, 8.5f, 160f, 160f));
            AssertRect(CombatHudCelestialV2LayoutProfile.PauseVisual, new Rect(2404f, 44f, 89f, 89f));
            AssertRect(CombatHudCelestialV2LayoutProfile.Skill, new Rect(2261f, 926f, 187f, 187f));
            AssertRect(CombatHudCelestialV2LayoutProfile.BasicAttack, new Rect(2248f, 1131f, 273f, 272f));
            AssertRect(CombatHudCelestialV2LayoutProfile.JoystickVisual, new Rect(201f, 979f, 269f, 269f));
            AssertRect(CombatHudCelestialV2LayoutProfile.JoystickActivation, new Rect(145f, 923f, 381f, 381f));
            AssertRect(CombatHudCelestialV2LayoutProfile.Reticle, new Rect(1224f, 664f, 112f, 112f));
            AssertRect(CombatHudCelestialV2LayoutProfile.PlayerComposite, new Rect(686f, 1246f, 1182f, 169f));
            AssertRect(CombatHudCelestialV2LayoutProfile.PlayerHpTrack, new Rect(888f, 1317f, 456f, 26f));
            AssertRect(CombatHudCelestialV2LayoutProfile.PlayerHpFill, new Rect(892f, 1322f, 444f, 16f));

            Assert.That(
                CombatHudCelestialV2LayoutProfile.Skill.yMin
                    - CombatHudCelestialV2LayoutProfile.SummonSlot3.yMax,
                Is.GreaterThanOrEqualTo(24f),
                "Summon rail must remain separated from the upper action button.");
            Assert.That(
                CombatHudCelestialV2LayoutProfile.BasicAttack.yMin
                    - CombatHudCelestialV2LayoutProfile.Skill.yMax,
                Is.GreaterThanOrEqualTo(18f),
                "Skill and basic attack silhouettes must not collide.");
            Assert.That(
                CombatHudCelestialV2LayoutProfile.PlayerComposite.xMin
                    - CombatHudCelestialV2LayoutProfile.JoystickActivation.xMax,
                Is.GreaterThanOrEqualTo(CombatHudCelestialV2LayoutProfile.MinimumPlayerActionGap),
                "Joystick acquisition and player readout need a mobile touch gap.");
            Assert.That(
                CombatHudCelestialV2LayoutProfile.Dodge.xMin
                    - CombatHudCelestialV2LayoutProfile.PlayerComposite.xMax,
                Is.GreaterThanOrEqualTo(CombatHudCelestialV2LayoutProfile.MinimumPlayerActionGap),
                "Player readout and action cluster need a mobile touch gap.");
        }

        [Test]
        public void StagingPrefabUsesEveryApprovedSpriteRoleInItsIntendedComponent()
        {
            GameObject prefab = RequireStagingPrefabOrIgnore();
            AssemblySpec spec = RequireAssemblySpec();

            AssertRoleSprite(RequireImage(RequireRect(prefab.transform, "TopLeftPanel")), spec, "objective.frame");
            AssertRoleSprite(RequireImage(RequireRect(prefab.transform, "BossNameArea")), spec, "boss.nameTab");
            AssertRoleSprite(RequireImage(RequireRect(prefab.transform, "BossHpBackground")), spec, "boss.hpTrack");
            AssertRoleSprite(RequireImage(RequireRect(prefab.transform, "BossHpFill")), spec, "boss.hpFill");
            AssertRoleSprite(RequireImage(RequireRect(prefab.transform, "BossCostBackground")), spec, "boss.costTrack");
            AssertRoleSprite(RequireImage(RequireRect(prefab.transform, "BossCostFill")), spec, "boss.costFill");

            RectTransform pause = RequireRect(prefab.transform, "PauseButton");
            AssertRoleSprite(RequireNamedImage(pause, "Plate"), spec, "pause.plate");
            AssertRoleSprite(RequireNamedImage(pause, "Glyph"), spec, "pause.glyph");

            AssertActionSprites(
                RequireRect(prefab.transform, "UltimateButton"),
                spec,
                "action.weaponSwapGlyph");
            AssertActionSprites(
                RequireRect(prefab.transform, "Skill1Button"),
                spec,
                "action.ultimateGlyph");
            AssertActionSprites(
                RequireRect(prefab.transform, "DodgeButton"),
                spec,
                "action.dashGlyph");
            AssertActionSprites(
                RequireRect(prefab.transform, "BasicAttackButton"),
                spec,
                "action.rangedGlyph");

            AssertSummonSprites(
                RequireRect(prefab.transform, "SummonSlot1Button"),
                spec,
                "summon.frame1",
                "summon.portrait1");
            AssertSummonSprites(
                RequireRect(prefab.transform, "SummonSlot2Button"),
                spec,
                "summon.frame2",
                "summon.portrait2");
            AssertSummonSprites(
                RequireRect(prefab.transform, "SummonSlot3Button"),
                spec,
                "summon.frame3",
                "summon.portrait3");

            RectTransform playerPortrait = RequireRect(prefab.transform, "PlayerPortraitFrame");
            AssertRoleSprite(RequireNamedImage(playerPortrait, "PortraitMask"), spec, "player.portraitMask");
            AssertRoleSprite(RequireNamedImage(playerPortrait, "PlayerPortrait"), spec, "player.portrait");
            AssertRoleSprite(RequireNamedImage(playerPortrait, "FrameOverlay"), spec, "player.portraitFrame");
            AssertRoleSprite(RequireImage(RequireRect(prefab.transform, "HealthBar_Track")), spec, "player.hpTrack");
            AssertRoleSprite(RequireImage(RequireRect(prefab.transform, "HealthBar")), spec, "player.hpFill");
            AssertRoleSprite(RequireImage(RequireRect(prefab.transform, "ResourceBar_Track")), spec, "player.enTrack");
            AssertRoleSprite(RequireImage(RequireRect(prefab.transform, "ResourceBar")), spec, "player.enFill");

            RectTransform mode = RequireRect(prefab.transform, "PlayerModeCell");
            AssertRoleSprite(RequireImage(mode), spec, "player.ammoPlate");
            AssertRoleSprite(RequireNamedImage(mode, "ModeGlyph"), spec, "player.modeGlyph");
            RectTransform ammo = RequireRect(prefab.transform, "PlayerAmmoChip");
            AssertRoleSprite(RequireImage(ammo), spec, "player.ammoPlate");
            AssertRoleSprite(RequireNamedImage(ammo, "BulletGlyph"), spec, "player.bulletGlyph");

            AssertRoleSprite(RequireImage(RequireRect(prefab.transform, "MoveJoystickRing")), spec, "joystick.base");
            AssertRoleSprite(RequireImage(RequireRect(prefab.transform, "MoveJoystickKnob")), spec, "joystick.knob");

            RectTransform reticle = RequireRect(prefab.transform, "CenterAimReticle");
            AssertRoleSprite(RequireNamedImage(reticle, "Dot"), spec, "reticle.dot");
            string[] needleNames = { "NeedleTop", "NeedleRight", "NeedleBottom", "NeedleLeft" };
            for (int i = 0; i < needleNames.Length; i++)
            {
                AssertRoleSprite(RequireNamedImage(reticle, needleNames[i]), spec, "reticle.needle");
            }

            Image[] v22Images = prefab.GetComponentsInChildren<Image>(includeInactive: true)
                .Where(image => image.sprite != null
                    && AssetDatabase.GetAssetPath(image.sprite).Replace('\\', '/').StartsWith(
                        V22ArtRoot,
                        StringComparison.Ordinal))
                .ToArray();
            Assert.That(v22Images, Is.Not.Empty);
            Assert.That(
                v22Images.All(image => !image.raycastTarget),
                Is.True,
                "V22 sprite layers are visual-only; input belongs to their transparent hit roots.");
        }

        [Test]
        public void AssembledPrefabKeepsActionAndSummonHitRootsSeparateFromVisualLayers()
        {
            GameObject prefab = RequireStagingPrefabOrIgnore();
            var actionRects = new Dictionary<string, Rect>
            {
                { "UltimateButton", CombatHudCelestialV2LayoutProfile.WeaponSwap },
                { "Skill1Button", CombatHudCelestialV2LayoutProfile.Skill },
                { "DodgeButton", CombatHudCelestialV2LayoutProfile.Dodge },
                { "BasicAttackButton", CombatHudCelestialV2LayoutProfile.BasicAttack }
            };
            foreach (KeyValuePair<string, Rect> entry in actionRects)
            {
                RectTransform hitRoot = RequireRect(prefab.transform, entry.Key);
                AssertVector(hitRoot.sizeDelta, entry.Value.size, $"{entry.Key} hit size");
                Assert.That(RequireImage(hitRoot).raycastTarget, Is.True);
                AssertNonInteractiveV22Layer(hitRoot, "Plate");
                AssertNonInteractiveV22Layer(hitRoot, "Glyph");
                AssertNonInteractiveV22Layer(hitRoot, "ReadyArc");
                AssertNonInteractiveV22Layer(hitRoot, "Cooldown");
            }

            string[] summons = { "SummonSlot1Button", "SummonSlot2Button", "SummonSlot3Button" };
            for (int i = 0; i < summons.Length; i++)
            {
                RectTransform hitRoot = RequireRect(prefab.transform, summons[i]);
                Assert.That(RequireImage(hitRoot).raycastTarget, Is.True);
                AssertNonInteractiveV22Layer(hitRoot, "PortraitMask");
                AssertNonInteractiveV22Layer(hitRoot, "Icon");
                AssertNonInteractiveV22Layer(hitRoot, "Frame");
                AssertNonInteractiveV22Layer(hitRoot, "StateArc");
                AssertNonInteractiveV22Layer(hitRoot, "CostTab");
                Assert.That(RequireNamedText(hitRoot, "CostText").raycastTarget, Is.False);
                Assert.That(RequireNamedText(hitRoot, "StatusText").raycastTarget, Is.False);
                Assert.That(RequireNamedText(hitRoot, "CostUnitText").raycastTarget, Is.False);

                Mask mask = RequireNamedImage(hitRoot, "PortraitMask").GetComponent<Mask>();
                Assert.That(mask, Is.Not.Null);
                Assert.That(mask.enabled, Is.True);
                Assert.That(mask.showMaskGraphic, Is.False);
                Assert.That(RequireNamedImage(hitRoot, "Icon").transform.parent.name, Is.EqualTo("PortraitMask"));
                Assert.That(RequireNamedImage(hitRoot, "PortraitMask").transform.GetSiblingIndex(), Is.LessThan(
                    RequireNamedImage(hitRoot, "Frame").transform.GetSiblingIndex()));
                Assert.That(RequireNamedImage(hitRoot, "Frame").transform.GetSiblingIndex(), Is.LessThan(
                    RequireNamedImage(hitRoot, "StateArc").transform.GetSiblingIndex()));
                Assert.That(RequireNamedImage(hitRoot, "StateArc").transform.GetSiblingIndex(), Is.LessThan(
                    RequireNamedImage(hitRoot, "CostTab").transform.GetSiblingIndex()));
            }
        }

        [Test]
        public void PresenterBindingsUseCompactV22StateLayersWithoutLegacyGlowOrSpark()
        {
            GameObject prefab = RequireStagingPrefabOrIgnore();
            Component presenter = RequireProductComponent(prefab, "DimensionBrawl.UI.CombatHudPresenter");
            Assert.That(presenter, Is.Not.Null);
            var serialized = new SerializedObject(presenter);

            SerializedProperty actionSlots = serialized.FindProperty("actionSlots");
            for (int i = 0; i < actionSlots.arraySize; i++)
            {
                SerializedProperty slot = actionSlots.GetArrayElementAtIndex(i);
                AssertReferenceName(slot, "cooldownFill", "Cooldown");
                int actionId = slot.FindPropertyRelative("actionId").intValue;
                if (actionId == 110 || actionId == 120)
                {
                    AssertReferenceName(slot, "readyProgressFill", "ReadyArc");
                }
                else
                {
                    Assert.That(slot.FindPropertyRelative("readyProgressFill").objectReferenceValue, Is.Null);
                }
                Assert.That(slot.FindPropertyRelative("readyGlowImage").objectReferenceValue, Is.Null);
            }

            SerializedProperty summonSlots = serialized.FindProperty("summonSlots");
            for (int i = 0; i < summonSlots.arraySize; i++)
            {
                SerializedProperty slot = summonSlots.GetArrayElementAtIndex(i);
                AssertReferenceName(slot, "labelText", "CostText");
                AssertReferenceName(slot, "stateText", "StatusText");
                AssertReferenceName(slot, "cooldownFill", "StateArc");
                AssertReferenceName(slot, "iconImage", "Icon");
                Assert.That(slot.FindPropertyRelative("readyGlowImage").objectReferenceValue, Is.Null);
                Assert.That(slot.FindPropertyRelative("readyRingImage").objectReferenceValue, Is.Null);
                Assert.That(slot.FindPropertyRelative("readySparkImage").objectReferenceValue, Is.Null);
            }

            AssertReferenceName(serialized, "bossHealthText", "BossHpText");
            AssertReferenceName(serialized, "bossResourceText", "BossCostText");
            AssertReferenceName(serialized, "aimReticleRoot", "CenterAimReticle");
            SerializedProperty segments = serialized.FindProperty("aimReticleSegments");
            Assert.That(segments.arraySize, Is.EqualTo(5));
        }

        [Test]
        public void AssembledPrefabKeepsApprovedPlayerBossPauseJoystickAndReticleContracts()
        {
            GameObject prefab = RequireStagingPrefabOrIgnore();
            CombatHudCelestialV2LayoutProfile marker =
                prefab.GetComponent<CombatHudCelestialV2LayoutProfile>();
            Assert.That(marker, Is.Not.Null);
            Assert.That(marker.Version, Is.EqualTo(CombatHudCelestialV2LayoutProfile.LayoutVersion));

            RectTransform pause = RequireRect(prefab.transform, "PauseButton");
            AssertVector(pause.sizeDelta, new Vector2(160f, 160f), "Pause hit");
            Assert.That(RequireImage(pause).raycastTarget, Is.True);
            RectTransform pausePlate = RequireNamedImage(pause, "Plate").rectTransform;
            AssertVector(pausePlate.sizeDelta, new Vector2(89f, 89f), "Pause visible");

            RectTransform joystick = RequireRect(prefab.transform, "MoveJoystickRing");
            AssertVector(joystick.sizeDelta, new Vector2(269f, 269f), "Joystick input geometry");
            Assert.That(RequireImage(joystick).raycastTarget, Is.False);
            Image activationHit = RequireNamedImage(joystick, "JoystickActivationHit");
            Assert.That(activationHit.raycastTarget, Is.True);
            AssertVector(activationHit.rectTransform.sizeDelta, new Vector2(381f, 381f), "Joystick activation");

            AssertVector(
                RequireRect(prefab.transform, "HealthBar_Track").sizeDelta,
                CombatHudCelestialV2LayoutProfile.PlayerHpTrack.size,
                "Player HP track");
            AssertVector(
                RequireRect(prefab.transform, "HealthBar").sizeDelta,
                CombatHudCelestialV2LayoutProfile.PlayerHpFill.size,
                "Player HP fill");
            AssertVector(
                RequireRect(prefab.transform, "ResourceBar_Track").sizeDelta,
                CombatHudCelestialV2LayoutProfile.PlayerEnTrack.size,
                "Player EN track");

            RectTransform reticle = RequireRect(prefab.transform, "CenterAimReticle");
            AssertVector(reticle.sizeDelta, new Vector2(112f, 112f), "Reticle root");
            AssertVector(reticle.anchoredPosition, Vector2.zero, "Reticle center");
            Assert.That(RequireImage(reticle).sprite, Is.Null);
            string[] segmentNames = { "Dot", "NeedleTop", "NeedleRight", "NeedleBottom", "NeedleLeft" };
            float[] expectedAngles = { 0f, 0f, -90f, 180f, 90f };
            for (int i = 0; i < segmentNames.Length; i++)
            {
                Image segment = RequireNamedImage(reticle, segmentNames[i]);
                AssertNonInteractiveV22Layer(reticle, segmentNames[i]);
                AssertVector(segment.rectTransform.sizeDelta, new Vector2(112f, 112f), segmentNames[i]);
                AssertVector(segment.rectTransform.anchoredPosition, Vector2.zero, segmentNames[i]);
                Assert.That(
                    Mathf.Abs(Mathf.DeltaAngle(segment.rectTransform.localEulerAngles.z, expectedAngles[i])),
                    Is.LessThanOrEqualTo(Tolerance),
                    $"{segmentNames[i]} rotation");
            }

            RectTransform portraitFrame = RequireRect(prefab.transform, "PlayerPortraitFrame");
            Transform portraitMask = RequireUniqueTransform(portraitFrame, "PortraitMask");
            Transform portraitOverlay = RequireUniqueTransform(portraitFrame, "FrameOverlay");
            Assert.That(portraitMask.GetSiblingIndex(), Is.LessThan(portraitOverlay.GetSiblingIndex()));
            Assert.That(portraitOverlay.GetSiblingIndex(), Is.EqualTo(portraitFrame.childCount - 1));

            RectTransform playerGroup = RequireRect(prefab.transform, "PlayerHudV22Root");
            Assert.That(
                RequireRect(playerGroup, "HealthBar").GetSiblingIndex(),
                Is.LessThan(RequireRect(playerGroup, "HealthBar_Track").GetSiblingIndex()));
            Assert.That(
                RequireRect(playerGroup, "ResourceBar").GetSiblingIndex(),
                Is.LessThan(RequireRect(playerGroup, "ResourceBar_Track").GetSiblingIndex()));

            string[] flowFillNames = { "BossHpFill", "BossCostFill", "HealthBar", "ResourceBar" };
            for (int i = 0; i < flowFillNames.Length; i++)
            {
                Image fill = RequireImage(RequireRect(prefab.transform, flowFillNames[i]));
                Assert.That(fill.material, Is.Not.Null, $"{flowFillNames[i]} lost its shared flow material.");
                Assert.That(fill.type, Is.EqualTo(Image.Type.Filled));
                Assert.That(fill.fillMethod, Is.EqualTo(Image.FillMethod.Horizontal));
            }
        }

        [UnityTest]
        public IEnumerator StagingRuntimeKeepsFillGeometryAndCompactStateLayers()
        {
            GameObject prefab = RequireStagingPrefabOrIgnore();
            GameObject instance = UnityEngine.Object.Instantiate(prefab);
            instance.name = "PF_UI_CombatHud_CelestialV2_RuntimeContract";

            try
            {
                yield return null;
                Component presenter = RequireProductComponent(
                    instance,
                    "DimensionBrawl.UI.CombatHudPresenter");
                Type actionIdType = RequireProductType("DimensionBrawl.UI.CombatHudActionId");

                Image bossHp = RequireImage(RequireRect(instance.transform, "BossHpFill"));
                Image bossCost = RequireImage(RequireRect(instance.transform, "BossCostFill"));
                Vector2 bossHpSize = bossHp.rectTransform.sizeDelta;
                Vector2 bossCostSize = bossCost.rectTransform.sizeDelta;
                Invoke(presenter, "SetBossHealth", 1200f, 2400f);
                Invoke(presenter, "SetBossResource", 64f, 100f);
                Assert.That(bossHp.fillAmount, Is.EqualTo(0.5f).Within(0.001f));
                Assert.That(bossCost.fillAmount, Is.EqualTo(0.64f).Within(0.001f));
                AssertVector(bossHp.rectTransform.sizeDelta, bossHpSize, "Boss HP full-width geometry");
                AssertVector(bossCost.rectTransform.sizeDelta, bossCostSize, "Boss cost full-width geometry");

                Image playerHp = RequireImage(RequireRect(instance.transform, "HealthBar"));
                Image playerEn = RequireImage(RequireRect(instance.transform, "ResourceBar"));
                Invoke(presenter, "SetHealth", 1840f, 2400f);
                Invoke(presenter, "SetResource", 64f, 100f);
                Assert.That(playerHp.fillAmount, Is.EqualTo(1840f / 2400f).Within(0.001f));
                Assert.That(playerEn.fillAmount, Is.EqualTo(0.64f).Within(0.001f));
                AssertVector(
                    playerHp.rectTransform.sizeDelta,
                    CombatHudCelestialV2LayoutProfile.PlayerHpFill.size,
                    "Player HP fill geometry");
                AssertVector(
                    playerEn.rectTransform.sizeDelta,
                    CombatHudCelestialV2LayoutProfile.PlayerEnFill.size,
                    "Player EN fill geometry");

                object dodge = Enum.Parse(actionIdType, "Dodge");
                Invoke(presenter, "SetSkillCooldown", dodge, 0.35f, string.Empty, 1.7f);
                Text dodgeCooldown = RequireNamedText(
                    RequireRect(instance.transform, "DodgeButton"),
                    "CooldownText");
                Assert.That(dodgeCooldown.gameObject.activeSelf, Is.True);
                Assert.That(dodgeCooldown.text, Is.EqualTo("1.7s"));

                object summon1 = Enum.Parse(actionIdType, "SummonSlot1");
                Invoke(presenter, "SetSummonSlotVisible", summon1, true);
                Invoke(presenter, "SetSummonSlotState", summon1, string.Empty, "24EN\nREADY LV1", true, 1f);
                RectTransform summon1Root = RequireRect(instance.transform, "SummonSlot1Button");
                AssertLegacySummonEffectsInactive(summon1Root);

                object summon3 = Enum.Parse(actionIdType, "SummonSlot3");
                Invoke(presenter, "SetSummonSlotVisible", summon3, true);
                Invoke(presenter, "SetSummonSlotState", summon3, string.Empty, "READY LV1", true, 1f);
                RectTransform summon3Root = RequireRect(instance.transform, "SummonSlot3Button");
                Assert.That(RequireNamedText(summon3Root, "CostUnitText").gameObject.activeSelf, Is.False);
                AssertLegacySummonEffectsInactive(summon3Root);

                RectTransform reticle = RequireRect(instance.transform, "CenterAimReticle");
                AssertVector(reticle.anchoredPosition, Vector2.zero, "Runtime reticle center");
            }
            finally
            {
                UnityEngine.Object.Destroy(instance);
            }

            yield return null;
        }

        [Test]
        public void AsymmetricSafeAreaKeepsObjectiveTextInsideBleedFrame()
        {
            GameObject prefab = RequireStagingPrefabOrIgnore();
            GameObject instance = UnityEngine.Object.Instantiate(prefab);
            instance.name = "PF_UI_CombatHud_CelestialV2_SafeAreaContract";

            try
            {
                Component presenter = RequireProductComponent(
                    instance,
                    "DimensionBrawl.UI.CombatHudPresenter");
                FieldInfo insetsField = presenter.GetType().GetField(
                    "safeAreaInsets",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.That(insetsField, Is.Not.Null, "Presenter safe-area state is missing.");
                insetsField.SetValue(presenter, new ScreenSafeAreaInsets(96f, 24f, 36f, 0f));
                Rect resolved = CombatHudCelestialV2LayoutProfile.ResolveObjectiveText(96f);
                AssertRect(resolved, new Rect(184f, 327f, 554f, 167f));
                Invoke(presenter, "ApplyCelestialV22ObjectiveTextRect");

                RectTransform frame = RequireRect(instance.transform, "TopLeftPanel");
                RectTransform objective = RequireRect(instance.transform, "Objective");
                float frameLeft = frame.anchoredPosition.x;
                float frameRight = frameLeft + frame.sizeDelta.x;
                float textLeft = objective.anchoredPosition.x;
                float textRight = textLeft + objective.sizeDelta.x;
                Assert.That(textLeft, Is.EqualTo(resolved.xMin).Within(Tolerance));
                Assert.That(objective.sizeDelta.x, Is.EqualTo(resolved.width).Within(Tolerance));
                Assert.That(
                    textRight,
                    Is.LessThanOrEqualTo(frameRight + Tolerance),
                    "A large left cutout may move the text inward, but it must not escape the bleed frame on the right.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(instance);
            }
        }

        [TestCase(CorridorScenePath)]
        [TestCase(StationScenePath)]
        [TestCase(CourtyardScenePath)]
        public void CanonicalV22ScenesPreserveInputCanvasAndPrefabInheritance(string scenePath)
        {
            GameObject canonical = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_Game/UI/CombatHud/PF_UI_CombatHud.prefab");
            if (canonical != null
                && canonical.GetComponent<CombatHudCelestialTargetLayoutProfile>() != null)
            {
                Assert.Ignore(
                    "Canonical combat HUD is Target v23; the Target suite owns canonical scene contracts.");
            }

            GameObject prefab = RequireCanonicalV22Prefab();
            string yaml = ReadAssetText(scenePath);
            string prefabSource =
                $"m_SourcePrefab: {{fileID: 100100000, guid: {CombatHudPrefabGuid}, type: 3}}";
            Assert.That(
                CountOccurrences(yaml, prefabSource),
                Is.EqualTo(1),
                $"{scenePath} must retain exactly one canonical V22 HUD instance.");

            Dictionary<long, long> strippedGameObjects = ParseCanonicalStrippedGameObjects(yaml);
            Dictionary<long, int> actualBindings = ParseCanonicalPointerBindings(
                yaml,
                strippedGameObjects);
            Dictionary<long, int> expectedBindings = BuildExpectedSceneBindings(prefab, scenePath);
            Assert.That(
                actualBindings,
                Is.EquivalentTo(expectedBindings),
                $"{scenePath} changed a scene-added action-to-button route.");

            AssertCanonicalCanvasScaler(prefab, scenePath, yaml, strippedGameObjects);
            AssertCanonicalJoystickBinding(prefab, scenePath, yaml, strippedGameObjects);
            AssertNoCanonicalV22VisualOverrides(prefab, scenePath, yaml);
        }

        [UnityTest]
        public IEnumerator CaptureStationGameplayWithV22HudForGpuReview()
        {
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
            {
                Assert.Ignore("GPU capture requires a graphics device; do not run this test with -nographics.");
            }

            GameObject stagingPrefab = RequireStagingPrefabOrIgnore();

            Screen.SetResolution(1280, 720, false);
            AsyncOperation load = SceneManager.LoadSceneAsync(StationScenePath, LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null);
            while (!load.isDone)
            {
                yield return null;
            }

            yield return null;
            Scene scene = SceneManager.GetSceneByPath(StationScenePath);
            Assert.That(scene.IsValid() && scene.isLoaded, Is.True);
            Type presenterType = RequireProductType("DimensionBrawl.UI.CombatHudPresenter");
            Component scenePresenter = Resources.FindObjectsOfTypeAll(presenterType)
                .OfType<Component>()
                .Where(candidate => candidate.gameObject.scene == scene)
                .OrderByDescending(candidate => candidate.gameObject.activeInHierarchy)
                .FirstOrDefault();
            Assert.That(scenePresenter, Is.Not.Null, "Station scene has no HUD presenter host.");
            ActivateHierarchy(scenePresenter.transform, scene);

            CanvasGroup hiddenSceneHud = scenePresenter.GetComponent<CanvasGroup>();
            if (hiddenSceneHud == null)
            {
                hiddenSceneHud = scenePresenter.gameObject.AddComponent<CanvasGroup>();
            }

            hiddenSceneHud.alpha = 0f;
            hiddenSceneHud.interactable = false;
            hiddenSceneHud.blocksRaycasts = false;

            GameObject stagingInstance = UnityEngine.Object.Instantiate(stagingPrefab);
            stagingInstance.name = "PF_UI_CombatHud_CelestialV2_Staging_GPU";
            SceneManager.MoveGameObjectToScene(stagingInstance, scene);
            Transform sceneHudParent = scenePresenter.transform.parent;
            stagingInstance.transform.SetParent(sceneHudParent, worldPositionStays: false);
            CopyRootRect(scenePresenter.transform as RectTransform, stagingInstance.transform as RectTransform);
            stagingInstance.transform.SetSiblingIndex(scenePresenter.transform.GetSiblingIndex() + 1);
            ActivateHierarchy(stagingInstance.transform, scene);

            Component presenter = RequireProductComponent(
                stagingInstance,
                "DimensionBrawl.UI.CombatHudPresenter");
            Assert.That(
                stagingInstance.GetComponent<CombatHudCelestialV2LayoutProfile>(),
                Is.Not.Null,
                "GPU review must render the staging V22 prefab, never the canonical asset.");

            Invoke(presenter, "SetObjective", "Break the pressure line");
            Invoke(presenter, "SetTimer", 138f);
            Invoke(presenter, "SetHealth", 1840f, 2400f);
            Invoke(presenter, "SetResource", 64f, 100f);
            Invoke(presenter, "SetBossHealth", 1960f, 2400f);
            Invoke(presenter, "SetBossResource", 64f, 100f);
            Invoke(presenter, "SetAimReticleVisible", true, false);
            Invoke(presenter, "SetInputMode", "RANGED");
            Invoke(presenter, "SetAmmo", "24/24", false);
            Type actionIdType = RequireProductType("DimensionBrawl.UI.CombatHudActionId");
            object basicAttack = Enum.Parse(actionIdType, "BasicAttack");
            object dodge = Enum.Parse(actionIdType, "Dodge");
            object skill = Enum.Parse(actionIdType, "Skill1");
            object ultimate = Enum.Parse(actionIdType, "Ultimate");
            Invoke(presenter, "SetSkillCooldown", basicAttack, 0f, string.Empty, -1f);
            Invoke(presenter, "SetSkillCooldown", dodge, 0.35f, string.Empty, 1.7f);
            Invoke(presenter, "SetSkillCooldown", skill, 0f, string.Empty, -1f);
            Invoke(presenter, "SetSkillCooldown", ultimate, 0f, string.Empty, -1f);
            SetSummonReviewState(presenter, actionIdType, "SummonSlot1", "24EN\nREADY LV1", true, 1f);
            SetSummonReviewState(presenter, actionIdType, "SummonSlot2", "18EN\nCD 3.2s", false, 0.44f);
            SetSummonReviewState(presenter, actionIdType, "SummonSlot3", "12EN\nLV1", false, 0.72f);

            for (int i = 0; i < 5; i++)
            {
                yield return null;
            }

            Canvas.ForceUpdateCanvases();
            // WaitForEndOfFrame is never pumped by the Unity Test Framework in batch mode.
            // Ordinary frames still submit the screen capture request and keep this test CLI-safe.
            yield return null;
            yield return null;
            yield return null;

            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            Assert.That(projectRoot, Is.Not.Null.And.Not.Empty);
            string logDirectory = Path.Combine(projectRoot, "Logs");
            Directory.CreateDirectory(logDirectory);
            string outputPath = Path.Combine(logDirectory, "combat_hud_v22_gameplay.png");
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }

            Canvas captureCanvas = stagingInstance.GetComponentInParent<Canvas>();
            Assert.That(captureCanvas, Is.Not.Null, "Staging HUD is not parented below a Canvas.");
            Camera captureCamera = FindSceneCaptureCamera(scene);
            Assert.That(captureCamera, Is.Not.Null, "Station scene has no active gameplay camera.");
            CaptureCameraAndHud(captureCamera, captureCanvas, outputPath, 1280, 720);

            Assert.That(File.Exists(outputPath), Is.True, $"Missing GPU capture: {outputPath}");
            Assert.That(new FileInfo(outputPath).Length, Is.GreaterThan(1024));
        }

        private static void SetSummonReviewState(
            Component presenter,
            Type actionIdType,
            string actionName,
            string state,
            bool enabled,
            float fill)
        {
            object actionId = Enum.Parse(actionIdType, actionName);
            Invoke(presenter, "SetSummonSlotVisible", actionId, true);
            Invoke(presenter, "SetSummonSlotState", actionId, string.Empty, state, enabled, fill);
        }

        private static GameObject RequireCanonicalV22Prefab()
        {
            const string canonicalPath = "Assets/_Game/UI/CombatHud/PF_UI_CombatHud.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(canonicalPath);
            Assert.That(prefab, Is.Not.Null, $"Missing canonical combat HUD: {canonicalPath}");
            CombatHudCelestialV2LayoutProfile marker =
                prefab.GetComponent<CombatHudCelestialV2LayoutProfile>();
            Assert.That(marker, Is.Not.Null, "Canonical combat HUD has not been promoted to V22.");
            Assert.That(marker.Version, Is.EqualTo(CombatHudCelestialV2LayoutProfile.LayoutVersion));
            return prefab;
        }

        private static Dictionary<long, int> BuildExpectedSceneBindings(
            GameObject prefab,
            string scenePath)
        {
            IEnumerable<(string ButtonName, int ActionId)> expected =
                string.Equals(scenePath, CourtyardScenePath, StringComparison.Ordinal)
                    ? SceneActionBindings.Take(2)
                    : SceneActionBindings;
            var result = new Dictionary<long, int>();
            foreach ((string buttonName, int actionId) in expected)
            {
                Transform button = RequireUniqueTransform(prefab.transform, buttonName);
                result.Add(RequireLocalFileId(button.gameObject), actionId);
            }

            return result;
        }

        private static Dictionary<long, long> ParseCanonicalStrippedGameObjects(string yaml)
        {
            string pattern =
                $@"^--- !u!1 &(?<localId>-?\d+) stripped\r?\nGameObject:\r?\n"
                + $@"  m_CorrespondingSourceObject: \{{fileID: (?<sourceId>-?\d+), guid: {CombatHudPrefabGuid}, type: 3\}}";
            var result = new Dictionary<long, long>();
            foreach (Match match in Regex.Matches(yaml, pattern, RegexOptions.Multiline))
            {
                result[long.Parse(match.Groups["localId"].Value)] =
                    long.Parse(match.Groups["sourceId"].Value);
            }

            return result;
        }

        private static Dictionary<long, int> ParseCanonicalPointerBindings(
            string yaml,
            IReadOnlyDictionary<long, long> strippedGameObjects)
        {
            var result = new Dictionary<long, int>();
            foreach (string body in EnumerateMonoBehaviourBodies(yaml))
            {
                if (body.IndexOf($"guid: {PointerInputScriptGuid}", StringComparison.Ordinal) < 0)
                {
                    continue;
                }

                long localGameObjectId = ReadLong(
                    body,
                    @"^  m_GameObject: \{fileID: (?<value>-?\d+)\}");
                if (!strippedGameObjects.TryGetValue(localGameObjectId, out long sourceGameObjectId))
                {
                    continue;
                }

                int actionId = checked((int)ReadLong(body, @"^  actionId: (?<value>-?\d+)$"));
                long bridgeId = ReadLong(body, @"^  inputBridge: \{fileID: (?<value>-?\d+)\}");
                Assert.That(bridgeId, Is.Not.Zero, $"Scene action {actionId} lost its HUD input bridge.");
                Assert.That(
                    result.ContainsKey(sourceGameObjectId),
                    Is.False,
                    $"Duplicate scene pointer input on prefab source object {sourceGameObjectId}.");
                result.Add(sourceGameObjectId, actionId);
            }

            return result;
        }

        private static void AssertCanonicalCanvasScaler(
            GameObject prefab,
            string scenePath,
            string yaml,
            IReadOnlyDictionary<long, long> strippedGameObjects)
        {
            long prefabRootSourceId = RequireLocalFileId(prefab);
            long[] localRootIds = strippedGameObjects
                .Where(pair => pair.Value == prefabRootSourceId)
                .Select(pair => pair.Key)
                .ToArray();
            Assert.That(
                localRootIds,
                Has.Length.LessThanOrEqualTo(1),
                $"{scenePath} serialized duplicate canonical HUD root GameObjects.");

            var candidateGameObjectIds = new List<long>(localRootIds);
            Dictionary<long, (long GameObjectId, long ParentTransformId)> transforms =
                ParseSceneTransformHierarchy(yaml);
            long parentTransformId = RequireCanonicalPrefabParentTransformId(yaml);
            var visitedTransforms = new HashSet<long>();
            while (parentTransformId != 0)
            {
                Assert.That(
                    visitedTransforms.Add(parentTransformId),
                    Is.True,
                    $"{scenePath} canonical HUD parent chain contains a transform cycle.");
                Assert.That(
                    transforms.TryGetValue(parentTransformId, out var transform),
                    Is.True,
                    $"{scenePath} HUD parent transform {parentTransformId} is not serialized.");
                candidateGameObjectIds.Add(transform.GameObjectId);
                parentTransformId = transform.ParentTransformId;
            }

            HashSet<long> canvasGameObjectIds = EnumerateCanvasBodies(yaml)
                .Select(body => ReadLong(body, @"^  m_GameObject: \{fileID: (?<value>-?\d+)\}"))
                .ToHashSet();
            long nearestCanvasGameObjectId = candidateGameObjectIds
                .FirstOrDefault(canvasGameObjectIds.Contains);
            Assert.That(
                nearestCanvasGameObjectId,
                Is.Not.Zero,
                $"{scenePath} canonical V22 HUD has no Canvas in its parent chain.");

            string[] scalers = EnumerateMonoBehaviourBodies(yaml)
                .Where(body => body.IndexOf(
                        "m_EditorClassIdentifier: UnityEngine.UI::UnityEngine.UI.CanvasScaler",
                        StringComparison.Ordinal) >= 0
                    && ReadLong(body, @"^  m_GameObject: \{fileID: (?<value>-?\d+)\}")
                    == nearestCanvasGameObjectId)
                .ToArray();
            Assert.That(
                scalers,
                Has.Length.EqualTo(1),
                $"{scenePath} nearest V22 Canvas must own exactly one CanvasScaler.");
            string scaler = scalers[0];
            Assert.That(scaler, Does.Match(@"(?m)^  m_UiScaleMode: 1$"));
            Assert.That(
                scaler,
                Does.Match(@"(?m)^  m_ReferenceResolution: \{x: 2560(?:\.0+)?, y: 1440(?:\.0+)?\}$"));
            Assert.That(scaler, Does.Match(@"(?m)^  m_ScreenMatchMode: 0$"));
            Assert.That(scaler, Does.Match(@"(?m)^  m_MatchWidthOrHeight: 1(?:\.0+)?$"));
        }

        private static void AssertCanonicalJoystickBinding(
            GameObject prefab,
            string scenePath,
            string yaml,
            IReadOnlyDictionary<long, long> strippedGameObjects)
        {
            long expectedRingId = RequireLocalFileId(
                RequireUniqueTransform(prefab.transform, "MoveJoystickRing").gameObject);
            int found = 0;
            foreach (string body in EnumerateMonoBehaviourBodies(yaml))
            {
                if (body.IndexOf($"guid: {VirtualJoystickScriptGuid}", StringComparison.Ordinal) < 0)
                {
                    continue;
                }

                long localGameObjectId = ReadLong(
                    body,
                    @"^  m_GameObject: \{fileID: (?<value>-?\d+)\}");
                if (!strippedGameObjects.TryGetValue(localGameObjectId, out long sourceGameObjectId)
                    || sourceGameObjectId != expectedRingId)
                {
                    continue;
                }

                found++;
                Assert.That(ReadLong(body, @"^  knob: \{fileID: (?<value>-?\d+)\}"), Is.Not.Zero);
                Assert.That(
                    ReadLong(body, @"^  movementController: \{fileID: (?<value>-?\d+)\}"),
                    Is.Not.Zero,
                    $"{scenePath} joystick lost its movement target.");
            }

            Assert.That(found, Is.EqualTo(1), $"{scenePath} must bind one V22 joystick.");
        }

        private static void AssertNoCanonicalV22VisualOverrides(
            GameObject prefab,
            string scenePath,
            string yaml)
        {
            long prefabRootRectId = RequireLocalFileId(prefab.GetComponent<RectTransform>());
            string pattern =
                $@"^    - target: \{{fileID: (?<sourceId>-?\d+), guid: {CombatHudPrefabGuid}, type: 3\}}\r?\n"
                + @"      propertyPath: (?<property>[^\r\n]+)";
            var offenders = new List<string>();
            foreach (Match match in Regex.Matches(yaml, pattern, RegexOptions.Multiline))
            {
                long sourceId = long.Parse(match.Groups["sourceId"].Value);
                string property = match.Groups["property"].Value;
                bool spriteOrMaterial = property == "m_Sprite"
                    || property == "m_Material"
                    || property.StartsWith("m_Color.", StringComparison.Ordinal)
                    || property == "m_RaycastTarget";
                bool descendantLayout = property.StartsWith("m_AnchoredPosition.", StringComparison.Ordinal)
                    || property.StartsWith("m_AnchorMin.", StringComparison.Ordinal)
                    || property.StartsWith("m_AnchorMax.", StringComparison.Ordinal)
                    || property.StartsWith("m_Pivot.", StringComparison.Ordinal)
                    || property.StartsWith("m_SizeDelta.", StringComparison.Ordinal)
                    || property.StartsWith("m_LocalScale.", StringComparison.Ordinal);
                if (spriteOrMaterial || (descendantLayout && sourceId != prefabRootRectId))
                {
                    offenders.Add($"{sourceId}:{property}");
                }
            }

            Assert.That(
                offenders,
                Is.Empty,
                $"{scenePath} overrides V22 descendant sprite/material/layout data: "
                    + string.Join(", ", offenders));
        }

        private static IEnumerable<string> EnumerateMonoBehaviourBodies(string yaml)
        {
            const string pattern =
                @"^--- !u!114 &-?\d+\r?\nMonoBehaviour:\r?\n(?<body>.*?)(?=^--- !u!|\z)";
            foreach (Match match in Regex.Matches(
                         yaml,
                         pattern,
                         RegexOptions.Multiline | RegexOptions.Singleline))
            {
                yield return match.Groups["body"].Value;
            }
        }

        private static IEnumerable<string> EnumerateCanvasBodies(string yaml)
        {
            const string pattern =
                @"^--- !u!223 &-?\d+\r?\nCanvas:\r?\n(?<body>.*?)(?=^--- !u!|\z)";
            foreach (Match match in Regex.Matches(
                         yaml,
                         pattern,
                         RegexOptions.Multiline | RegexOptions.Singleline))
            {
                yield return match.Groups["body"].Value;
            }
        }

        private static Dictionary<long, (long GameObjectId, long ParentTransformId)>
            ParseSceneTransformHierarchy(string yaml)
        {
            const string pattern =
                @"^--- !u!(?:4|224) &(?<componentId>-?\d+)\r?\n(?:Transform|RectTransform):\r?\n"
                + @"(?<body>.*?)(?=^--- !u!|\z)";
            var result = new Dictionary<long, (long, long)>();
            foreach (Match match in Regex.Matches(
                         yaml,
                         pattern,
                         RegexOptions.Multiline | RegexOptions.Singleline))
            {
                string body = match.Groups["body"].Value;
                Match gameObject = Regex.Match(
                    body,
                    @"^  m_GameObject: \{fileID: (?<value>-?\d+)\}",
                    RegexOptions.Multiline);
                Match parent = Regex.Match(
                    body,
                    @"^  m_Father: \{fileID: (?<value>-?\d+)\}",
                    RegexOptions.Multiline);
                if (!gameObject.Success || !parent.Success)
                {
                    continue;
                }

                result[long.Parse(match.Groups["componentId"].Value)] = (
                    long.Parse(gameObject.Groups["value"].Value),
                    long.Parse(parent.Groups["value"].Value));
            }

            return result;
        }

        private static long RequireCanonicalPrefabParentTransformId(string yaml)
        {
            const string pattern =
                @"^--- !u!1001 &-?\d+\r?\nPrefabInstance:\r?\n(?<body>.*?)(?=^--- !u!|\z)";
            string[] canonicalInstances = Regex.Matches(
                    yaml,
                    pattern,
                    RegexOptions.Multiline | RegexOptions.Singleline)
                .Cast<Match>()
                .Select(match => match.Groups["body"].Value)
                .Where(body => body.IndexOf(
                    $"m_SourcePrefab: {{fileID: 100100000, guid: {CombatHudPrefabGuid}, type: 3}}",
                    StringComparison.Ordinal) >= 0)
                .ToArray();
            Assert.That(
                canonicalInstances,
                Has.Length.EqualTo(1),
                "Expected exactly one canonical V22 HUD PrefabInstance block.");
            return ReadLong(
                canonicalInstances[0],
                @"^    m_TransformParent: \{fileID: (?<value>-?\d+)\}");
        }

        private static long ReadLong(string source, string pattern)
        {
            Match match = Regex.Match(source, pattern, RegexOptions.Multiline);
            Assert.That(match.Success, Is.True, $"Missing YAML field matching {pattern}.");
            return long.Parse(match.Groups["value"].Value);
        }

        private static long RequireLocalFileId(UnityEngine.Object assetObject)
        {
            Assert.That(assetObject, Is.Not.Null);
            bool found = AssetDatabase.TryGetGUIDAndLocalFileIdentifier(
                assetObject,
                out string guid,
                out long localFileId);
            Assert.That(found, Is.True, $"Could not resolve local file ID for {assetObject.name}.");
            Assert.That(guid, Is.EqualTo(CombatHudPrefabGuid));
            return localFileId;
        }

        private static string ReadAssetText(string assetPath)
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            Assert.That(projectRoot, Is.Not.Null.And.Not.Empty);
            string absolutePath = Path.Combine(projectRoot, assetPath);
            Assert.That(File.Exists(absolutePath), Is.True, $"Missing asset text: {assetPath}");
            return File.ReadAllText(absolutePath);
        }

        private static int CountOccurrences(string source, string value)
        {
            int count = 0;
            int index = 0;
            while ((index = source.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
            {
                count++;
                index += value.Length;
            }

            return count;
        }

        private static AssemblySpec RequireAssemblySpec()
        {
            TextAsset source = AssetDatabase.LoadAssetAtPath<TextAsset>(AssemblySpecPath);
            Assert.That(source, Is.Not.Null, $"Missing {AssemblySpecPath}.");
            AssemblySpec spec = JsonUtility.FromJson<AssemblySpec>(source.text);
            Assert.That(spec, Is.Not.Null);
            Assert.That(spec.sprites, Is.Not.Null);
            return spec;
        }

        private static string ResolveSpriteAssetPath(AssemblySpec spec, SpriteSpec entry)
        {
            return $"{spec.artRoot.Replace('\\', '/').TrimEnd('/')}/{entry.path.Replace('\\', '/').TrimStart('/')}";
        }

        private static void AssertRoleSprite(Image image, AssemblySpec spec, string role)
        {
            SpriteSpec entry = spec.sprites.Single(candidate => string.Equals(
                candidate.role,
                role,
                StringComparison.Ordinal));
            Assert.That(image.sprite, Is.Not.Null, $"{GetPath(image.transform)} has no sprite for {role}.");
            string actualPath = AssetDatabase.GetAssetPath(image.sprite).Replace('\\', '/');
            Assert.That(actualPath, Is.EqualTo(ResolveSpriteAssetPath(spec, entry)), GetPath(image.transform));
            Assert.That(image.raycastTarget, Is.False, $"{GetPath(image.transform)} must be visual-only.");
        }

        private static void AssertActionSprites(
            RectTransform action,
            AssemblySpec spec,
            string glyphRole)
        {
            AssertRoleSprite(RequireNamedImage(action, "Plate"), spec, "action.plate");
            AssertRoleSprite(RequireNamedImage(action, "ReadyArc"), spec, "action.readyArc");
            AssertRoleSprite(RequireNamedImage(action, "Cooldown"), spec, "action.cooldownDisc");
            AssertRoleSprite(RequireNamedImage(action, "Glyph"), spec, glyphRole);
        }

        private static void AssertSummonSprites(
            RectTransform summon,
            AssemblySpec spec,
            string frameRole,
            string portraitRole)
        {
            AssertRoleSprite(RequireNamedImage(summon, "PortraitMask"), spec, "summon.mask");
            AssertRoleSprite(RequireNamedImage(summon, "Frame"), spec, frameRole);
            AssertRoleSprite(RequireNamedImage(summon, "StateArc"), spec, "summon.stateArc");
            AssertRoleSprite(RequireNamedImage(summon, "CostTab"), spec, "summon.costTab");
            AssertRoleSprite(RequireNamedImage(summon, "Icon"), spec, portraitRole);
        }

        private static void AssertLegacySummonEffectsInactive(RectTransform summon)
        {
            string[] legacyNames = { "ReadyGlow", "ReadyRing", "ReadySparkRing" };
            Transform[] descendants = summon.GetComponentsInChildren<Transform>(includeInactive: true);
            for (int i = 0; i < legacyNames.Length; i++)
            {
                Transform[] matches = descendants
                    .Where(candidate => string.Equals(candidate.name, legacyNames[i], StringComparison.Ordinal))
                    .ToArray();
                Assert.That(matches, Has.Length.LessThanOrEqualTo(1), legacyNames[i]);
                if (matches.Length == 1)
                {
                    Assert.That(
                        matches[0].gameObject.activeSelf,
                        Is.False,
                        $"Compact V22 must not reactivate legacy {legacyNames[i]}.");
                }
            }
        }

        private static void CopyRootRect(RectTransform source, RectTransform destination)
        {
            Assert.That(source, Is.Not.Null, "Scene HUD root is not a RectTransform.");
            Assert.That(destination, Is.Not.Null, "Staging HUD root is not a RectTransform.");
            destination.anchorMin = source.anchorMin;
            destination.anchorMax = source.anchorMax;
            destination.pivot = source.pivot;
            destination.anchoredPosition = source.anchoredPosition;
            destination.sizeDelta = source.sizeDelta;
            destination.localRotation = source.localRotation;
            destination.localScale = source.localScale;
        }

        private static Camera FindSceneCaptureCamera(Scene scene)
        {
            Camera taggedMain = Camera.main;
            if (taggedMain != null
                && taggedMain.gameObject.scene == scene
                && taggedMain.enabled
                && taggedMain.gameObject.activeInHierarchy)
            {
                return taggedMain;
            }

            return Resources.FindObjectsOfTypeAll<Camera>()
                .Where(camera => camera.gameObject.scene == scene
                    && camera.enabled
                    && camera.gameObject.activeInHierarchy)
                .OrderByDescending(camera => camera.depth)
                .FirstOrDefault();
        }

        private static void CaptureCameraAndHud(
            Camera camera,
            Canvas canvas,
            string outputPath,
            int width,
            int height)
        {
            RenderTexture previousActive = RenderTexture.active;
            RenderTexture previousTarget = camera.targetTexture;
            RenderMode previousRenderMode = canvas.renderMode;
            Camera previousWorldCamera = canvas.worldCamera;
            float previousPlaneDistance = canvas.planeDistance;
            var target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            var frame = new Texture2D(width, height, TextureFormat.RGBA32, false);

            try
            {
                target.Create();
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = camera;
                canvas.planeDistance = Mathf.Max(camera.nearClipPlane + 0.1f, 1f);
                Canvas.ForceUpdateCanvases();

                camera.targetTexture = target;
                RenderTexture.active = target;
                camera.Render();
                frame.ReadPixels(new Rect(0f, 0f, width, height), 0, 0, false);
                frame.Apply(updateMipmaps: false, makeNoLongerReadable: false);
                File.WriteAllBytes(outputPath, frame.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                canvas.renderMode = previousRenderMode;
                canvas.worldCamera = previousWorldCamera;
                canvas.planeDistance = previousPlaneDistance;
                target.Release();
                UnityEngine.Object.DestroyImmediate(frame);
                UnityEngine.Object.DestroyImmediate(target);
            }
        }

        private static Component RequireProductComponent(GameObject gameObject, string typeName)
        {
            Type type = RequireProductType(typeName);
            Component component = gameObject.GetComponent(type);
            Assert.That(component, Is.Not.Null, $"{gameObject.name} is missing {typeName}.");
            return component;
        }

        private static Type RequireProductType(string fullName)
        {
            Type type = AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly => assembly.GetType(fullName, throwOnError: false))
                .FirstOrDefault(candidate => candidate != null);
            Assert.That(type, Is.Not.Null, $"Missing product type {fullName}.");
            return type;
        }

        private static void Invoke(Component component, string methodName, params object[] arguments)
        {
            MethodInfo[] candidates = component.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(method => string.Equals(method.Name, methodName, StringComparison.Ordinal)
                    && method.GetParameters().Length == arguments.Length)
                .ToArray();
            Assert.That(candidates, Has.Length.EqualTo(1), $"Ambiguous or missing {methodName}({arguments.Length}).");
            candidates[0].Invoke(component, arguments);
        }

        private static void ActivateHierarchy(Transform leaf, Scene scene)
        {
            var hierarchy = new Stack<GameObject>();
            Transform current = leaf;
            while (current != null && current.gameObject.scene == scene)
            {
                hierarchy.Push(current.gameObject);
                current = current.parent;
            }

            while (hierarchy.Count > 0)
            {
                hierarchy.Pop().SetActive(true);
            }
        }

        private static GameObject RequireStagingPrefabOrIgnore()
        {
            GameObject staging = AssetDatabase.LoadAssetAtPath<GameObject>(StagingPrefabPath);
            if (staging != null && staging.GetComponent<CombatHudCelestialV2LayoutProfile>() != null)
            {
                return staging;
            }

            Assert.Ignore("V22 staging prefab has not been assembled yet.");
            return null;
        }

        private static void AssertNonInteractiveV22Layer(Transform root, string name)
        {
            Image image = RequireNamedImage(root, name);
            Assert.That(image.raycastTarget, Is.False, $"{name} must not consume taps.");
            AssertV22Sprite(image);
        }

        private static void AssertV22Sprite(Image image)
        {
            Assert.That(image.sprite, Is.Not.Null, $"{GetPath(image.transform)} has no sprite.");
            string path = AssetDatabase.GetAssetPath(image.sprite).Replace('\\', '/');
            Assert.That(path, Does.StartWith(V22ArtRoot), GetPath(image.transform));
        }

        private static void AssertReferenceName(
            SerializedProperty parent,
            string propertyName,
            string expectedName)
        {
            SerializedProperty property = parent.FindPropertyRelative(propertyName);
            Assert.That(property, Is.Not.Null, propertyName);
            Assert.That(property.objectReferenceValue, Is.Not.Null, propertyName);
            Assert.That(property.objectReferenceValue.name, Is.EqualTo(expectedName));
        }

        private static void AssertReferenceName(
            SerializedObject serialized,
            string propertyName,
            string expectedName)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            Assert.That(property, Is.Not.Null, propertyName);
            Assert.That(property.objectReferenceValue, Is.Not.Null, propertyName);
            Assert.That(property.objectReferenceValue.name, Is.EqualTo(expectedName));
        }

        private static Image RequireNamedImage(Transform root, string name)
        {
            Transform found = RequireUniqueTransform(root, name);
            return RequireImage(found);
        }

        private static Text RequireNamedText(Transform root, string name)
        {
            Transform found = RequireUniqueTransform(root, name);
            Text text = found.GetComponent<Text>();
            Assert.That(text, Is.Not.Null, $"{GetPath(found)} has no Text.");
            return text;
        }

        private static Image RequireImage(Transform transform)
        {
            Image image = transform.GetComponent<Image>();
            Assert.That(image, Is.Not.Null, $"{GetPath(transform)} has no Image.");
            return image;
        }

        private static RectTransform RequireRect(Transform root, string name)
        {
            Transform found = RequireUniqueTransform(root, name);
            RectTransform rect = found as RectTransform;
            Assert.That(rect, Is.Not.Null, $"{name} has no RectTransform.");
            return rect;
        }

        private static Transform RequireUniqueTransform(Transform root, string name)
        {
            Transform[] matches = root.GetComponentsInChildren<Transform>(includeInactive: true)
                .Where(candidate => string.Equals(candidate.name, name, StringComparison.Ordinal))
                .ToArray();
            Assert.That(matches, Has.Length.EqualTo(1), $"Expected one {name} under {root.name}.");
            return matches[0];
        }

        private static void AssertRect(Rect actual, Rect expected)
        {
            Assert.That(actual.x, Is.EqualTo(expected.x).Within(Tolerance));
            Assert.That(actual.y, Is.EqualTo(expected.y).Within(Tolerance));
            Assert.That(actual.width, Is.EqualTo(expected.width).Within(Tolerance));
            Assert.That(actual.height, Is.EqualTo(expected.height).Within(Tolerance));
        }

        private static void AssertVector(Vector2 actual, Vector2 expected, string label)
        {
            Assert.That(actual.x, Is.EqualTo(expected.x).Within(Tolerance), $"{label}.x");
            Assert.That(actual.y, Is.EqualTo(expected.y).Within(Tolerance), $"{label}.y");
        }

        private static string GetPath(Transform transform)
        {
            var names = new Stack<string>();
            Transform current = transform;
            while (current != null)
            {
                names.Push(current.name);
                current = current.parent;
            }

            return string.Join("/", names);
        }
    }
}
