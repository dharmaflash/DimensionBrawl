using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
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
    public sealed class CombatHudCelestialTargetAssemblyPlayModeTests
    {
        private const string StagingPrefabPath =
            "Assets/_Game/UI/CombatHud/PF_UI_CombatHud_CelestialTarget_Staging.prefab";
        private const string CanonicalPrefabPath =
            "Assets/_Game/UI/CombatHud/PF_UI_CombatHud.prefab";
        private const string AssemblySpecPath =
            "Assets/_Game/UI/CombatHud/CombatHudCelestialTargetAssemblySpec.json";
        private const string TargetArtRoot =
            "Assets/_Game/UI/CombatHud/Art/CelestialHudTarget/Runtime/";
        private const string StationScenePath =
            "Assets/_Game/Scenes/OlympusStationCombatStage.unity";
        private const string CorridorScenePath =
            "Assets/_Game/Scenes/OlympusCorridorInvasionStage.unity";
        private const string CourtyardScenePath =
            "Assets/_Game/Scenes/OlympusCourtyardDrillStage.unity";
        private const string CombatHudPrefabGuid = "4e5297b5734b6664b935ffb1ae9b48b6";
        private const string PointerInputScriptGuid = "e764d6dd84658b34d9df199b296e940b";
        private const string VirtualJoystickScriptGuid = "d85f5878113320a48a4d953bd098c390";
        private const string AimDragInputScriptGuid = "05e8d31be1fb44e4c8b3c828334d6c04";
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

        private static readonly string[] ManagedTargetRoots =
        {
            "TopLeftPanel", "BossHudRoot", "PauseButton",
            "SummonRailTargetRoot", "UltimateButton", "Skill1Button",
            "DodgeButton", "BasicAttackButton", "MoveJoystickRing",
            "MoveJoystickKnob", "PlayerHudTargetRoot", "CenterAimReticle"
        };

        private static readonly HashSet<string> SceneManagedVisualRoots =
            new HashSet<string>(StringComparer.Ordinal)
            {
                "DimensionHudSkinRoot", "TopLeftPanel", "Objective", "Timer",
                "MissionTimerBacking", "SettingsButton", "BossHudRoot", "BossSymbol",
                "BossNameArea", "BossHpBackground", "BossHpFill", "BossCostBackground",
                "BossCostFill", "ActionFeedback", "PauseButton", "SummonRailTargetRoot",
                "SummonSlot1Button", "SummonSlot2Button", "SummonSlot3Button",
                "UltimateButton", "Skill1Button", "DodgeButton", "BasicAttackButton",
                "MoveJoystickRing", "MoveJoystickKnob", "JoystickActivationHit",
                "PlayerHudTargetRoot", "CenterAimReticle"
            };
        private static readonly HashSet<string> PresenterDirectVisualBindings =
            new HashSet<string>(StringComparer.Ordinal)
            {
                "objectiveText", "timerText", "healthText", "resourceText",
                "inputModeText", "ammoText", "actionFeedbackText", "healthFill",
                "resourceFill", "bossHudRoot", "bossHealthText", "bossResourceText",
                "bossHealthFill", "bossResourceFill", "aimReticleRoot",
                "playerDamageOverlayImage"
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
        public void LayoutProfileLocksApprovedTargetAndEnlargedPlayerVitals()
        {
            AssertRect(
                CombatHudCelestialTargetLayoutProfile.ObjectiveFrame,
                new Rect(0f, 327f, 806f, 167f));
            AssertRect(
                CombatHudCelestialTargetLayoutProfile.BossChassis,
                new Rect(827f, 61f, 945f, 126f));
            AssertRect(
                CombatHudCelestialTargetLayoutProfile.PauseVisual,
                new Rect(2402f, 44f, 103f, 96f));
            AssertVector(
                CombatHudCelestialTargetLayoutProfile.PauseHit.size,
                new Vector2(160f, 160f));

            AssertRect(
                CombatHudCelestialTargetLayoutProfile.SummonSlot1,
                new Rect(2206f, 173f, 297f, 259f));
            AssertRect(
                CombatHudCelestialTargetLayoutProfile.SummonSlot2,
                new Rect(2212f, 430f, 276f, 197f));
            AssertRect(
                CombatHudCelestialTargetLayoutProfile.SummonSlot3,
                new Rect(2214f, 640f, 260f, 185f));
            AssertRect(
                CombatHudCelestialTargetLayoutProfile.WeaponSwap,
                new Rect(1991f, 928.5f, 208f, 208f));
            AssertRect(
                CombatHudCelestialTargetLayoutProfile.Ultimate,
                new Rect(2229f, 891.5f, 208f, 208f));
            AssertRect(
                CombatHudCelestialTargetLayoutProfile.Dash,
                new Rect(1909f, 1137.5f, 208f, 208f));
            AssertRect(
                CombatHudCelestialTargetLayoutProfile.BasicAttack,
                new Rect(2167f, 1120f, 260f, 260f));

            AssertRect(
                CombatHudCelestialTargetLayoutProfile.JoystickVisual,
                new Rect(190f, 966f, 296f, 305f));
            AssertRect(
                CombatHudCelestialTargetLayoutProfile.JoystickKnob,
                new Rect(287f, 1067f, 102f, 102f));
            AssertVector(
                CombatHudCelestialTargetLayoutProfile.JoystickActivation.size,
                new Vector2(381f, 381f));
            AssertVector(
                CombatHudCelestialTargetLayoutProfile.JoystickActivation.center,
                CombatHudCelestialTargetLayoutProfile.JoystickVisual.center);
            AssertVector(
                CombatHudCelestialTargetLayoutProfile.JoystickKnob.center,
                new Vector2(338f, 1118f));

            AssertRect(
                CombatHudCelestialTargetLayoutProfile.PlayerPortrait,
                new Rect(686f, 1262f, 153f, 153f));
            AssertRect(
                CombatHudCelestialTargetLayoutProfile.PlayerHpTrack,
                new Rect(888f, 1307f, 672f, 32f));
            AssertRect(
                CombatHudCelestialTargetLayoutProfile.PlayerHpFill,
                new Rect(898f, 1314f, 652f, 20f));
            AssertRect(
                CombatHudCelestialTargetLayoutProfile.PlayerCostTrack,
                new Rect(888f, 1347f, 672f, 28f));
            AssertRect(
                CombatHudCelestialTargetLayoutProfile.PlayerCostFill,
                new Rect(898f, 1353f, 652f, 16f));
            AssertRect(
                CombatHudCelestialTargetLayoutProfile.PlayerModeGlyph,
                new Rect(1580f, 1294f, 64f, 64f));
            AssertRect(
                CombatHudCelestialTargetLayoutProfile.PlayerAmmo,
                new Rect(1654f, 1290f, 194f, 68f));
            AssertRect(
                CombatHudCelestialTargetLayoutProfile.PlayerAmmoText,
                new Rect(1734f, 1290f, 104f, 68f));
            AssertVector(
                CombatHudCelestialTargetLayoutProfile.Reticle.center,
                new Vector2(1280f, 720f));
            Assert.That(
                CombatHudCelestialTargetLayoutProfile.MinimumPlayerActionGap,
                Is.EqualTo(41f).Within(Tolerance));
        }

        [Test]
        public void AssemblySpecIsRoleBasedAndEveryRequiredSpriteExists()
        {
            TextAsset json = AssetDatabase.LoadAssetAtPath<TextAsset>(AssemblySpecPath);
            Assert.That(json, Is.Not.Null);
            AssemblySpec spec = JsonUtility.FromJson<AssemblySpec>(json.text);
            Assert.That(spec, Is.Not.Null);
            Assert.That(
                spec.version,
                Is.EqualTo(CombatHudCelestialTargetLayoutProfile.LayoutVersion));
            Assert.That(
                spec.artRoot.Replace('\\', '/').TrimEnd('/'),
                Is.EqualTo(TargetArtRoot.TrimEnd('/')));
            Assert.That(spec.sprites, Is.Not.Empty);
            Assert.That(spec.sprites.Select(entry => entry.role), Is.Unique);
            Assert.That(spec.sprites.Select(entry => entry.path), Is.Unique);
            Assert.That(spec.sprites.All(entry => !entry.path.Contains("..")), Is.True);

            foreach (SpriteSpec entry in spec.sprites.Where(entry => entry.required))
            {
                string path = $"{spec.artRoot.TrimEnd('/')}/{entry.path.TrimStart('/')}";
                Assert.That(File.Exists(ToAbsolutePath(path)), Is.True, entry.role);
                Assert.That(
                    AssetDatabase.LoadAssetAtPath<Sprite>(path),
                    Is.Not.Null,
                    $"{entry.role} must import as Sprite: {path}");
            }
        }

        [Test]
        public void StagingPrefabUsesTargetMarkerAtomicLayersAndCompactPlayerReadouts()
        {
            GameObject prefab = RequireStagingOrIgnore();
            CombatHudCelestialTargetLayoutProfile marker =
                prefab.GetComponent<CombatHudCelestialTargetLayoutProfile>();
            Assert.That(marker, Is.Not.Null);
            Assert.That(marker.Version, Is.EqualTo(CombatHudCelestialTargetLayoutProfile.LayoutVersion));
            Assert.That(prefab.GetComponent<CombatHudCelestialV2LayoutProfile>(), Is.Null);

            Transform objective = Require(prefab.transform, "TopLeftPanel");
            AssertTargetSprite(objective.Find("ObjectiveBody"));
            AssertTargetSprite(objective.Find("ObjectiveTopFacets"));
            AssertTargetSprite(objective.Find("ObjectiveBottomFacets"));

            AssertTargetSprite(Require(prefab.transform, "BossTargetChassis"));
            AssertTargetSprite(Require(prefab.transform, "BossNameArea"));
            AssertTargetSprite(Require(prefab.transform, "BossHpBackground"));
            AssertTargetSprite(Require(prefab.transform, "BossHpFill"));
            AssertTargetSprite(Require(prefab.transform, "BossCostBackground"));
            AssertTargetSprite(Require(prefab.transform, "BossCostFill"));
            AssertFillAboveTrack(
                Require(prefab.transform, "BossHpBackground"),
                Require(prefab.transform, "BossHpFill"));
            AssertFillAboveTrack(
                Require(prefab.transform, "BossCostBackground"),
                Require(prefab.transform, "BossCostFill"));

            string[] actionRoots =
            {
                "UltimateButton", "Skill1Button", "DodgeButton", "BasicAttackButton"
            };
            foreach (string rootName in actionRoots)
            {
                Transform root = Require(prefab.transform, rootName);
                AssertTargetSprite(root.Find("Plate"));
                AssertTargetSprite(root.Find("Glyph"));
                AssertTargetSprite(root.Find("Cooldown"));
                AssertTargetSprite(root.Find("ReadyArc"));
            }
            AssertRightBottomDesignRect(
                Require(prefab.transform, "UltimateButton") as RectTransform,
                CombatHudCelestialTargetLayoutProfile.WeaponSwap);
            AssertRightBottomDesignRect(
                Require(prefab.transform, "Skill1Button") as RectTransform,
                CombatHudCelestialTargetLayoutProfile.Ultimate);
            AssertRightBottomDesignRect(
                Require(prefab.transform, "DodgeButton") as RectTransform,
                CombatHudCelestialTargetLayoutProfile.Dash);
            AssertRightBottomDesignRect(
                Require(prefab.transform, "BasicAttackButton") as RectTransform,
                CombatHudCelestialTargetLayoutProfile.BasicAttack);

            for (int slot = 1; slot <= 3; slot++)
            {
                Transform root = Require(prefab.transform, $"SummonSlot{slot}Button");
                AssertTargetSprite(root.Find("PortraitMask"));
                AssertTargetSprite(Require(root, "Icon"));
                AssertTargetSprite(root.Find("Frame"));
                AssertTargetSprite(root.Find("StateArc"));
                AssertTargetSprite(root.Find("CostTab"));
            }
            AssertRightTopDesignRect(
                Require(prefab.transform, "SummonSlot1Button") as RectTransform,
                CombatHudCelestialTargetLayoutProfile.SummonSlot1);
            AssertRightTopDesignRect(
                Require(prefab.transform, "SummonSlot2Button") as RectTransform,
                CombatHudCelestialTargetLayoutProfile.SummonSlot2);
            AssertRightTopDesignRect(
                Require(prefab.transform, "SummonSlot3Button") as RectTransform,
                CombatHudCelestialTargetLayoutProfile.SummonSlot3);

            AssertLeftBottomDesignRect(
                Require(prefab.transform, "MoveJoystickRing") as RectTransform,
                CombatHudCelestialTargetLayoutProfile.JoystickVisual);
            AssertLeftBottomDesignRect(
                Require(prefab.transform, "MoveJoystickKnob") as RectTransform,
                CombatHudCelestialTargetLayoutProfile.JoystickKnob);
            RectTransform joystickActivation = Require(
                prefab.transform,
                "JoystickActivationHit") as RectTransform;
            Assert.That(joystickActivation, Is.Not.Null);
            AssertVector(joystickActivation.anchorMin, new Vector2(0.5f, 0.5f));
            AssertVector(joystickActivation.anchorMax, new Vector2(0.5f, 0.5f));
            AssertVector(joystickActivation.anchoredPosition, Vector2.zero);
            AssertVector(
                joystickActivation.sizeDelta,
                CombatHudCelestialTargetLayoutProfile.JoystickActivation.size);

            Transform playerRoot = Require(prefab.transform, "PlayerHudTargetRoot");
            Assert.That(FindAll(prefab.transform, "PlayerHudV22Root"), Is.Empty);
            AssertTargetSprite(Require(playerRoot, "PlayerTargetChassis"));
            AssertTargetSprite(Require(playerRoot, "HealthBar_Track"));
            AssertTargetSprite(Require(playerRoot, "HealthBar"));
            AssertTargetSprite(Require(playerRoot, "ResourceBar_Track"));
            AssertTargetSprite(Require(playerRoot, "ResourceBar"));
            AssertFillAboveTrack(
                Require(playerRoot, "HealthBar_Track"),
                Require(playerRoot, "HealthBar"));
            AssertFillAboveTrack(
                Require(playerRoot, "ResourceBar_Track"),
                Require(playerRoot, "ResourceBar"));
            AssertTargetSprite(Require(playerRoot, "ModeGlyph"));
            AssertTargetSprite(Require(playerRoot, "PlayerAmmoChip"));
            Assert.That(Require(playerRoot, "InputMode").gameObject.activeSelf, Is.False);
            Assert.That(Require(playerRoot, "ResourceText").gameObject.activeSelf, Is.False);

            RectTransform hpTrack = Require(playerRoot, "HealthBar_Track") as RectTransform;
            RectTransform costTrack = Require(playerRoot, "ResourceBar_Track") as RectTransform;
            Assert.That(hpTrack.sizeDelta.x, Is.EqualTo(672f).Within(Tolerance));
            Assert.That(costTrack.sizeDelta.x, Is.EqualTo(672f).Within(Tolerance));
            AssertVector(
                (Require(playerRoot, "PlayerModeCell") as RectTransform).sizeDelta,
                new Vector2(64f, 64f));
            AssertVector(
                (Require(playerRoot, "AmmoText") as RectTransform).sizeDelta,
                new Vector2(104f, 68f));

            Transform reticle = Require(prefab.transform, "CenterAimReticle");
            foreach (string name in new[]
                     {
                         "Dot", "NeedleTop", "NeedleRight", "NeedleBottom", "NeedleLeft"
                     })
            {
                AssertTargetSprite(reticle.Find(name));
            }
        }

        [Test]
        public void CanonicalPrefabIsTargetV23AndMatchesStagingManagedHierarchy()
        {
            GameObject canonical = RequireCanonicalTargetPrefab();
            GameObject staging = RequireStagingOrIgnore();
            Assert.That(
                AssetDatabase.AssetPathToGUID(CanonicalPrefabPath),
                Is.EqualTo(CombatHudPrefabGuid));

            CombatHudCelestialV2LayoutProfile legacy =
                canonical.GetComponent<CombatHudCelestialV2LayoutProfile>();
            Assert.That(
                legacy,
                Is.Not.Null,
                "The disabled V22 marker preserves its pre-promotion canonical local ID.");
            Assert.That(legacy.enabled, Is.False);
            Assert.That(staging.GetComponent<CombatHudCelestialV2LayoutProfile>(), Is.Null);

            string canonicalHash = ComputeManagedHierarchyHash(canonical);
            string stagingHash = ComputeManagedHierarchyHash(staging);
            Assert.That(
                canonicalHash,
                Is.EqualTo(stagingHash),
                "Canonical and staging Target-managed hierarchies diverged.");
        }

        [Test]
        public void CanonicalPresenterVisualBindingsResolveToTargetChildren()
        {
            GameObject canonical = RequireCanonicalTargetPrefab();
            Type presenterType = RequireProductType("DimensionBrawl.UI.CombatHudPresenter");
            Component presenter = canonical.GetComponent(presenterType);
            Assert.That(presenter, Is.Not.Null);
            var serialized = new SerializedObject(presenter);
            AssertPresenterReference(serialized, "objectiveText", canonical, "Objective", typeof(Text));
            AssertPresenterReference(serialized, "timerText", canonical, "Timer", typeof(Text));
            AssertPresenterReference(serialized, "healthText", canonical, "HealthText", typeof(Text));
            AssertPresenterReference(serialized, "resourceText", canonical, "ResourceText", typeof(Text));
            AssertPresenterReference(serialized, "inputModeText", canonical, "InputMode", typeof(Text));
            AssertPresenterReference(serialized, "ammoText", canonical, "AmmoText", typeof(Text));
            AssertPresenterReference(
                serialized,
                "actionFeedbackText",
                canonical,
                "ActionFeedback",
                typeof(Text));
            AssertPresenterReference(serialized, "healthFill", canonical, "HealthBar", typeof(Image));
            AssertPresenterReference(serialized, "resourceFill", canonical, "ResourceBar", typeof(Image));
            AssertPresenterReference(
                serialized,
                "bossHudRoot",
                canonical,
                "BossHudRoot",
                typeof(RectTransform));
            AssertPresenterReference(
                serialized,
                "bossHealthText",
                canonical,
                "BossHpText",
                typeof(Text));
            AssertPresenterReference(
                serialized,
                "bossResourceText",
                canonical,
                "BossCostText",
                typeof(Text));
            AssertPresenterReference(
                serialized,
                "bossHealthFill",
                canonical,
                "BossHpFill",
                typeof(Image));
            AssertPresenterReference(
                serialized,
                "bossResourceFill",
                canonical,
                "BossCostFill",
                typeof(Image));
            AssertPresenterReference(
                serialized,
                "aimReticleRoot",
                canonical,
                "CenterAimReticle",
                typeof(RectTransform));
        }

        [Test]
        public void CanonicalTargetMarkerWinsWhileDisabledV22IdentityIsRetained()
        {
            GameObject canonical = RequireCanonicalTargetPrefab();
            GameObject instance = UnityEngine.Object.Instantiate(canonical);
            try
            {
                Assert.That(
                    instance.GetComponent<CombatHudCelestialV2LayoutProfile>(),
                    Is.Not.Null);
                Assert.That(
                    instance.GetComponent<CombatHudCelestialV2LayoutProfile>().enabled,
                    Is.False);
                Type presenterType = RequireProductType("DimensionBrawl.UI.CombatHudPresenter");
                Component presenter = instance.GetComponent(presenterType);
                Invoke(presenter, "SetTimer", 138f);
                Invoke(presenter, "SetInputMode", "FRONT READY LV2");

                Assert.That(
                    Require(instance.transform, "MissionTimerBacking").gameObject.activeSelf,
                    Is.False);
                Assert.That(
                    Require(instance.transform, "Timer").gameObject.activeSelf,
                    Is.False);
                Assert.That(
                    Require(instance.transform, "InputMode").gameObject.activeSelf,
                    Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(instance);
            }
        }

        [TestCase(CorridorScenePath, 7)]
        [TestCase(StationScenePath, 7)]
        [TestCase(CourtyardScenePath, 2)]
        public void CanonicalTargetScenesPreserveInputCanvasAndPrefabInheritance(
            string scenePath,
            int expectedActionCount)
        {
            GameObject prefab = RequireCanonicalTargetPrefab();
            string yaml = ReadAssetText(scenePath);
            string prefabSource =
                $"m_SourcePrefab: {{fileID: 100100000, guid: {CombatHudPrefabGuid}, type: 3}}";
            Assert.That(
                CountOccurrences(yaml, prefabSource),
                Is.EqualTo(1),
                $"{scenePath} must retain exactly one canonical Target HUD instance.");

            Dictionary<long, long> strippedGameObjects = ParseCanonicalStrippedGameObjects(yaml);
            Dictionary<long, int> actualBindings = ParseCanonicalPointerBindings(
                yaml,
                strippedGameObjects);
            Dictionary<long, int> expectedBindings = BuildExpectedSceneBindings(prefab, scenePath);
            Assert.That(actualBindings, Has.Count.EqualTo(expectedActionCount));
            Assert.That(
                actualBindings,
                Is.EquivalentTo(expectedBindings),
                $"{scenePath} changed a scene-added action-to-button route.");

            AssertCanonicalCanvasScaler(prefab, scenePath, yaml, strippedGameObjects);
            AssertCanonicalJoystickBinding(prefab, scenePath, yaml, strippedGameObjects);
            AssertNoCanonicalTargetVisualOverrides(prefab, scenePath, yaml);
            AssertCanonicalAddedPresentationCleanup(scenePath, yaml);
        }

        [Test]
        public void RuntimeHalfMetersKeepFullGeometryAndVisibleFillAboveOpaqueTracks()
        {
            GameObject prefab = RequireStagingOrIgnore();
            GameObject instance = UnityEngine.Object.Instantiate(prefab);
            try
            {
                Type presenterType = RequireProductType("DimensionBrawl.UI.CombatHudPresenter");
                Component presenter = instance.GetComponent(presenterType);
                Assert.That(presenter, Is.Not.Null);
                Invoke(presenter, "SetHealth", 50f, 100f);
                Invoke(presenter, "SetResource", 50f, 100f);
                Invoke(presenter, "SetBossHealth", 50f, 100f);
                Invoke(presenter, "SetBossResource", 50f, 100f);

                RectTransform playerGroup = Require(
                    instance.transform,
                    "PlayerHudTargetRoot") as RectTransform;
                Assert.That(playerGroup, Is.Not.Null);
                Assert.That(
                    playerGroup.anchoredPosition.x,
                    Is.EqualTo(0f).Within(Tolerance));
                Assert.That(
                    playerGroup.localScale.x,
                    Is.EqualTo(1f).Within(Tolerance));
                Text healthReadout = Require(instance.transform, "HealthText").GetComponent<Text>();
                Assert.That(healthReadout, Is.Not.Null);
                AssertColor(
                    healthReadout.color,
                    new Color32(0xF7, 0xF5, 0xEE, 0xFF),
                    "Target player HP readout");
                AssertRuntimeHalfFill(instance.transform, "HealthBar_Track", "HealthBar");
                AssertRuntimeHalfFill(instance.transform, "ResourceBar_Track", "ResourceBar");
                AssertRuntimeHalfFill(instance.transform, "BossHpBackground", "BossHpFill");
                AssertRuntimeHalfFill(instance.transform, "BossCostBackground", "BossCostFill");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(instance);
            }
        }

        [Test]
        public void TargetTimerRemainsHiddenWhenGameplayBinderSetsInitialValue()
        {
            GameObject prefab = RequireStagingOrIgnore();
            GameObject instance = UnityEngine.Object.Instantiate(prefab);
            try
            {
                Type presenterType = RequireProductType("DimensionBrawl.UI.CombatHudPresenter");
                Component presenter = instance.GetComponent(presenterType);
                Assert.That(presenter, Is.Not.Null);

                Invoke(presenter, "SetTimer", 138f);

                Assert.That(
                    Require(instance.transform, "MissionTimerBacking").gameObject.activeSelf,
                    Is.False);
                Assert.That(
                    Require(instance.transform, "Timer").gameObject.activeSelf,
                    Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(instance);
            }
        }

        [Test]
        public void TargetInactiveReticlePreservesAuthoredWhiteNeedlesAndCyanDot()
        {
            GameObject prefab = RequireStagingOrIgnore();
            GameObject instance = UnityEngine.Object.Instantiate(prefab);
            try
            {
                Type presenterType = RequireProductType("DimensionBrawl.UI.CombatHudPresenter");
                Component presenter = instance.GetComponent(presenterType);
                Assert.That(presenter, Is.Not.Null);
                Invoke(presenter, "SetAimReticleVisible", true, false);

                Transform reticle = Require(instance.transform, "CenterAimReticle");
                foreach (string name in new[]
                         {
                             "Dot", "NeedleTop", "NeedleRight", "NeedleBottom", "NeedleLeft"
                         })
                {
                    Image segment = Require(reticle, name).GetComponent<Image>();
                    Assert.That(segment, Is.Not.Null, name);
                    AssertColor(segment.color, Color.white, name);
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(instance);
            }
        }

        [Test]
        public void TargetAmmoCompactsReloadAndHidesTheWholeChipWithoutAmmo()
        {
            GameObject prefab = RequireStagingOrIgnore();
            GameObject instance = UnityEngine.Object.Instantiate(prefab);
            try
            {
                Type presenterType = RequireProductType("DimensionBrawl.UI.CombatHudPresenter");
                Component presenter = instance.GetComponent(presenterType);
                Assert.That(presenter, Is.Not.Null);
                Transform chip = Require(instance.transform, "PlayerAmmoChip");
                Text readout = Require(instance.transform, "AmmoText").GetComponent<Text>();
                Assert.That(readout, Is.Not.Null);

                Invoke(presenter, "SetAmmo", "24/24 RLD 1.2", true);
                Assert.That(chip.gameObject.activeSelf, Is.True);
                Assert.That(readout.gameObject.activeSelf, Is.True);
                Assert.That(readout.text, Is.EqualTo("RLD 1.2"));
                AssertColor(
                    readout.color,
                    new Color32(0x8D, 0xD4, 0xDF, 0xFF),
                    "Target reload readout");

                Invoke(presenter, "SetAmmo", "24/24", false);
                Assert.That(readout.text, Is.EqualTo("24 / 24"));
                AssertColor(
                    readout.color,
                    new Color32(0xF7, 0xF5, 0xEE, 0xFF),
                    "Target ammo readout");

                Invoke(presenter, "SetAmmo", string.Empty, false);
                Assert.That(chip.gameObject.activeSelf, Is.False);
                Assert.That(readout.gameObject.activeSelf, Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(instance);
            }
        }

        [Test]
        public void TargetAmmoVisibilityAuthoritativelyTogglesModeGlyphAndChip()
        {
            GameObject prefab = RequireStagingOrIgnore();
            GameObject instance = UnityEngine.Object.Instantiate(prefab);
            try
            {
                Type presenterType = RequireProductType("DimensionBrawl.UI.CombatHudPresenter");
                Component presenter = instance.GetComponent(presenterType);
                Assert.That(presenter, Is.Not.Null);
                Transform modeCell = Require(instance.transform, "PlayerModeCell");
                Transform ammoChip = Require(instance.transform, "PlayerAmmoChip");

                Invoke(presenter, "SetAmmo", string.Empty, false);
                Assert.That(modeCell.gameObject.activeSelf, Is.False);
                Assert.That(ammoChip.gameObject.activeSelf, Is.False);

                Invoke(presenter, "SetInputMode", "FRONT READY LV2");
                Assert.That(modeCell.gameObject.activeSelf, Is.False);
                Assert.That(ammoChip.gameObject.activeSelf, Is.False);

                Invoke(presenter, "SetAmmo", "24/24", false);
                Assert.That(modeCell.gameObject.activeSelf, Is.True);
                Assert.That(ammoChip.gameObject.activeSelf, Is.True);

                Invoke(presenter, "SetInputMode", "MELEE");
                Assert.That(modeCell.gameObject.activeSelf, Is.True);
                Assert.That(ammoChip.gameObject.activeSelf, Is.True);

                Invoke(presenter, "SetAmmo", string.Empty, false);
                Assert.That(modeCell.gameObject.activeSelf, Is.False);
                Assert.That(ammoChip.gameObject.activeSelf, Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(instance);
            }
        }

        [Test]
        public void StagingTouchTargetsAreIndependentAndDoNotOverlap()
        {
            GameObject prefab = RequireStagingOrIgnore();
            var visualRects = new Dictionary<string, Rect>
            {
                { "UltimateButton", CombatHudCelestialTargetLayoutProfile.WeaponSwap },
                { "Skill1Button", CombatHudCelestialTargetLayoutProfile.Ultimate },
                { "DodgeButton", CombatHudCelestialTargetLayoutProfile.Dash },
                { "BasicAttackButton", CombatHudCelestialTargetLayoutProfile.BasicAttack }
            };
            var expectedTouchInsets = new Dictionary<string, Vector4>
            {
                { "UltimateButton", new Vector4(8f, 8f, 8f, 26f) },
                { "Skill1Button", new Vector4(8f, 8f, 8f, 24f) },
                { "DodgeButton", new Vector4(8f, 14f, 8f, 8f) },
                { "BasicAttackButton", new Vector4(8f, 36f, 8f, 8f) }
            };
            var touchRects = new Dictionary<string, Rect>();
            foreach (KeyValuePair<string, Rect> pair in visualRects)
            {
                Transform root = Require(prefab.transform, pair.Key);
                Assert.That(root.GetComponent<Image>().raycastTarget, Is.False);
                RectTransform touch = root.Find("TouchTarget") as RectTransform;
                Assert.That(touch, Is.Not.Null, pair.Key);
                Assert.That(touch.GetComponent<Image>().raycastTarget, Is.True, pair.Key);
                AssertTouchInsets(touch, expectedTouchInsets[pair.Key], pair.Key);
                touchRects.Add(pair.Key, ResolveTouchRect(pair.Value, touch));
                AssertDecorativeImagesDoNotRaycast(root, touch.GetComponent<Image>());
            }

            AssertPairwiseGap(touchRects, 8f);

            var summonTouches = new Dictionary<string, Rect>();
            Rect[] summonVisuals =
            {
                CombatHudCelestialTargetLayoutProfile.SummonSlot1,
                CombatHudCelestialTargetLayoutProfile.SummonSlot2,
                CombatHudCelestialTargetLayoutProfile.SummonSlot3
            };
            for (int i = 0; i < summonVisuals.Length; i++)
            {
                string name = $"SummonSlot{i + 1}Button";
                Transform root = Require(prefab.transform, name);
                Assert.That(root.GetComponent<Image>().raycastTarget, Is.False);
                RectTransform touch = root.Find("TouchTarget") as RectTransform;
                Assert.That(touch, Is.Not.Null, name);
                Assert.That(touch.GetComponent<Image>().raycastTarget, Is.True, name);
                summonTouches.Add(name, ResolveTouchRect(summonVisuals[i], touch));
                AssertDecorativeImagesDoNotRaycast(root, touch.GetComponent<Image>());
            }

            AssertPairwiseGap(summonTouches, 8f);

            RectTransform pauseTouch = Require(
                Require(prefab.transform, "PauseButton"),
                "TouchTarget") as RectTransform;
            Assert.That(pauseTouch, Is.Not.Null);
            Assert.That(pauseTouch.GetComponent<Image>().raycastTarget, Is.True);
            Assert.That(
                pauseTouch.offsetMax.y,
                Is.EqualTo(0f).Within(Tolerance));
            RectTransform summonOneTouch = Require(
                Require(prefab.transform, "SummonSlot1Button"),
                "TouchTarget") as RectTransform;
            Assert.That(summonOneTouch, Is.Not.Null);
            Assert.That(
                summonOneTouch.offsetMax.y,
                Is.EqualTo(-8f).Within(Tolerance));
            var pauseAndSummonOne = new Dictionary<string, Rect>
            {
                {
                    "PauseButton",
                    ResolveTouchRect(
                        CombatHudCelestialTargetLayoutProfile.PauseHit,
                        pauseTouch)
                },
                {
                    "SummonSlot1Button",
                    ResolveTouchRect(
                        CombatHudCelestialTargetLayoutProfile.SummonSlot1,
                        summonOneTouch)
                }
            };
            AssertPairwiseGap(pauseAndSummonOne, 8f);
        }

        [UnityTest]
        public IEnumerator CaptureStationGameplayWithCanonicalTargetHudForGpuReview()
        {
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
            {
                Assert.Ignore("GPU capture requires a graphics device; do not use -nographics.");
            }

            GameObject canonicalPrefab = RequireCanonicalTargetPrefab();
            Screen.SetResolution(1672, 941, false);
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

            GameObject targetInstance = UnityEngine.Object.Instantiate(canonicalPrefab);
            targetInstance.name = "PF_UI_CombatHud_CelestialTarget_Canonical_GPU";
            SceneManager.MoveGameObjectToScene(targetInstance, scene);
            targetInstance.transform.SetParent(
                scenePresenter.transform.parent,
                worldPositionStays: false);
            CopyRootRect(
                scenePresenter.transform as RectTransform,
                targetInstance.transform as RectTransform);
            targetInstance.transform.SetSiblingIndex(scenePresenter.transform.GetSiblingIndex() + 1);
            ActivateHierarchy(targetInstance.transform, scene);

            Component presenter = targetInstance.GetComponent(presenterType);
            Assert.That(presenter, Is.Not.Null);
            Assert.That(
                targetInstance.GetComponent<CombatHudCelestialTargetLayoutProfile>(),
                Is.Not.Null,
                "GPU review must render the promoted canonical Target v23 prefab.");

            Invoke(presenter, "SetObjective", "Break the pressure line");
            Invoke(presenter, "SetTimer", 138f);
            Invoke(presenter, "SetHealth", 1840f, 2400f);
            Invoke(presenter, "SetResource", 64f, 100f);
            Invoke(presenter, "SetBossHealth", 1960f, 2400f);
            Invoke(presenter, "SetBossResource", 64f, 100f);
            Invoke(presenter, "SetAimReticleVisible", true, false);
            Invoke(presenter, "SetInputMode", "FRONT READY LV2");
            Invoke(presenter, "SetAmmo", "24/24", false);

            Type actionIdType = RequireProductType("DimensionBrawl.UI.CombatHudActionId");
            SetSkillCooldown(presenter, actionIdType, "BasicAttack", 0f, -1f);
            SetSkillCooldown(presenter, actionIdType, "Dodge", 0.35f, 1.7f);
            SetSkillCooldown(presenter, actionIdType, "Skill1", 0f, -1f);
            SetSkillCooldown(presenter, actionIdType, "Ultimate", 0f, -1f);
            SetSummonReviewState(presenter, actionIdType, "SummonSlot1", "24EN", true, 1f);
            SetSummonReviewState(presenter, actionIdType, "SummonSlot2", "18EN", false, 0.44f);
            SetSummonReviewState(presenter, actionIdType, "SummonSlot3", "12EN", false, 0.72f);

            for (int i = 0; i < 20; i++)
            {
                yield return null;
            }

            Assert.That(
                Require(targetInstance.transform, "MissionTimerBacking").gameObject.activeSelf,
                Is.False);
            Assert.That(
                Require(targetInstance.transform, "Timer").gameObject.activeSelf,
                Is.False);
            Canvas.ForceUpdateCanvases();
            yield return null;
            yield return null;
            yield return null;

            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            Assert.That(projectRoot, Is.Not.Null.And.Not.Empty);
            string logDirectory = Path.Combine(projectRoot, "Logs");
            Directory.CreateDirectory(logDirectory);
            string outputPath = Path.Combine(
                logDirectory,
                "combat_hud_target_canonical_gameplay.png");
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }

            Canvas captureCanvas = targetInstance.GetComponentInParent<Canvas>();
            Assert.That(captureCanvas, Is.Not.Null, "Canonical Target HUD is not below a Canvas.");
            Camera captureCamera = FindSceneCaptureCamera(scene);
            Assert.That(captureCamera, Is.Not.Null, "Station scene has no active gameplay camera.");
            CaptureCameraAndHud(
                captureCamera,
                targetInstance,
                outputPath,
                1672,
                941);

            Assert.That(File.Exists(outputPath), Is.True, $"Missing GPU capture: {outputPath}");
            Assert.That(new FileInfo(outputPath).Length, Is.GreaterThan(200 * 1024));
        }

        private static void SetSkillCooldown(
            Component presenter,
            Type actionIdType,
            string actionName,
            float normalizedRemaining,
            float secondsRemaining)
        {
            object actionId = Enum.Parse(actionIdType, actionName);
            Invoke(
                presenter,
                "SetSkillCooldown",
                actionId,
                normalizedRemaining,
                string.Empty,
                secondsRemaining);
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

        private static void AssertDecorativeImagesDoNotRaycast(
            Transform root,
            Image touchTarget)
        {
            Image[] images = root.GetComponentsInChildren<Image>(true);
            foreach (Image image in images)
            {
                if (image != touchTarget)
                {
                    Assert.That(image.raycastTarget, Is.False, GetPath(image.transform));
                }
            }
        }

        private static void AssertRuntimeHalfFill(
            Transform root,
            string trackName,
            string fillName)
        {
            Transform track = Require(root, trackName);
            Transform fill = Require(root, fillName);
            AssertFillAboveTrack(track, fill);
            Image image = fill.GetComponent<Image>();
            Assert.That(image, Is.Not.Null);
            Assert.That(image.gameObject.activeInHierarchy, Is.True);
            Assert.That(image.type, Is.EqualTo(Image.Type.Filled));
            Assert.That(image.fillMethod, Is.EqualTo(Image.FillMethod.Horizontal));
            Assert.That(image.fillAmount, Is.EqualTo(0.5f).Within(0.001f));
            Assert.That(image.rectTransform.sizeDelta.x, Is.GreaterThan(0f));
        }

        private static void AssertFillAboveTrack(Transform track, Transform fill)
        {
            Assert.That(track.parent, Is.EqualTo(fill.parent));
            Assert.That(
                fill.GetSiblingIndex(),
                Is.GreaterThan(track.GetSiblingIndex()),
                $"{fill.name} must render after opaque {track.name}.");
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
            Assert.That(
                candidates,
                Has.Length.EqualTo(1),
                $"Ambiguous or missing {methodName}({arguments.Length}).");
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
            Camera sceneCamera,
            GameObject hudRoot,
            string outputPath,
            int width,
            int height)
        {
            RenderTexture previousActive = RenderTexture.active;
            RenderTexture previousTarget = sceneCamera.targetTexture;
            var sceneTarget = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            var uiTarget = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
            var sceneFrame = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var uiFrame = new Texture2D(width, height, TextureFormat.RGBA32, false);
            GameObject captureCanvasObject = null;
            GameObject uiCameraObject = null;

            try
            {
                sceneTarget.Create();
                uiTarget.Create();

                const int uiLayer = 5;
                uiCameraObject = new GameObject("TargetHudCaptureCamera", typeof(Camera));
                SceneManager.MoveGameObjectToScene(uiCameraObject, sceneCamera.gameObject.scene);
                Camera uiCamera = uiCameraObject.GetComponent<Camera>();
                uiCamera.clearFlags = CameraClearFlags.SolidColor;
                uiCamera.backgroundColor = Color.clear;
                uiCamera.cullingMask = 1 << uiLayer;
                uiCamera.orthographic = true;
                uiCamera.orthographicSize = height * 0.5f;
                uiCamera.nearClipPlane = 0.1f;
                uiCamera.farClipPlane = 100f;
                uiCamera.allowHDR = false;
                uiCamera.allowMSAA = false;
                uiCamera.useOcclusionCulling = false;
                uiCamera.enabled = false;
                uiCamera.targetTexture = uiTarget;

                captureCanvasObject = new GameObject(
                    "TargetHudCaptureCanvas",
                    typeof(RectTransform),
                    typeof(Canvas),
                    typeof(CanvasScaler));
                SceneManager.MoveGameObjectToScene(
                    captureCanvasObject,
                    sceneCamera.gameObject.scene);
                captureCanvasObject.layer = uiLayer;
                Canvas captureCanvas = captureCanvasObject.GetComponent<Canvas>();
                captureCanvas.renderMode = RenderMode.ScreenSpaceCamera;
                captureCanvas.worldCamera = uiCamera;
                captureCanvas.planeDistance = 1f;
                captureCanvas.pixelPerfect = false;
                captureCanvas.sortingOrder = 32000;
                CanvasScaler scaler = captureCanvasObject.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(2560f, 1440f);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 1f;

                hudRoot.transform.SetParent(captureCanvasObject.transform, worldPositionStays: false);
                RectTransform hudRect = hudRoot.transform as RectTransform;
                Assert.That(hudRect, Is.Not.Null);
                hudRect.anchorMin = Vector2.zero;
                hudRect.anchorMax = Vector2.one;
                hudRect.pivot = new Vector2(0.5f, 0.5f);
                hudRect.anchoredPosition = Vector2.zero;
                hudRect.sizeDelta = Vector2.zero;
                hudRect.localScale = Vector3.one;
                SetLayerRecursively(hudRoot.transform, uiLayer);
                Canvas.ForceUpdateCanvases();

                captureCanvas.enabled = false;
                sceneCamera.targetTexture = sceneTarget;
                RenderTexture.active = sceneTarget;
                sceneCamera.Render();
                sceneFrame.ReadPixels(new Rect(0f, 0f, width, height), 0, 0, false);
                sceneFrame.Apply(updateMipmaps: false, makeNoLongerReadable: false);

                captureCanvas.enabled = true;
                Canvas.ForceUpdateCanvases();
                RenderTexture.active = uiTarget;
                uiCamera.Render();
                uiFrame.ReadPixels(new Rect(0f, 0f, width, height), 0, 0, false);
                uiFrame.Apply(updateMipmaps: false, makeNoLongerReadable: false);

                Color32[] scenePixels = sceneFrame.GetPixels32();
                Color32[] uiPixels = uiFrame.GetPixels32();
                AssertHudOverlayHasExpectedCoverage(uiPixels, width, height);
                AlphaComposite(uiPixels, scenePixels);
                sceneFrame.SetPixels32(scenePixels);
                sceneFrame.Apply(updateMipmaps: false, makeNoLongerReadable: false);
                AssertCaptureHasVariedColor(scenePixels);
                File.WriteAllBytes(outputPath, sceneFrame.EncodeToPNG());
            }
            finally
            {
                sceneCamera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                sceneTarget.Release();
                uiTarget.Release();
                UnityEngine.Object.DestroyImmediate(sceneFrame);
                UnityEngine.Object.DestroyImmediate(uiFrame);
                UnityEngine.Object.DestroyImmediate(sceneTarget);
                UnityEngine.Object.DestroyImmediate(uiTarget);
                if (captureCanvasObject != null)
                {
                    UnityEngine.Object.DestroyImmediate(captureCanvasObject);
                }
                if (uiCameraObject != null)
                {
                    UnityEngine.Object.DestroyImmediate(uiCameraObject);
                }
            }
        }

        private static void SetLayerRecursively(Transform root, int layer)
        {
            root.gameObject.layer = layer;
            for (int i = 0; i < root.childCount; i++)
            {
                SetLayerRecursively(root.GetChild(i), layer);
            }
        }

        private static void AlphaComposite(Color32[] overlay, Color32[] background)
        {
            Assert.That(overlay.Length, Is.EqualTo(background.Length));
            for (int i = 0; i < background.Length; i++)
            {
                int alpha = overlay[i].a;
                if (alpha <= 0)
                {
                    background[i].a = 255;
                    continue;
                }

                int inverse = 255 - alpha;
                background[i] = new Color32(
                    (byte)((overlay[i].r * alpha + background[i].r * inverse + 127) / 255),
                    (byte)((overlay[i].g * alpha + background[i].g * inverse + 127) / 255),
                    (byte)((overlay[i].b * alpha + background[i].b * inverse + 127) / 255),
                    255);
            }
        }

        private static void AssertHudOverlayHasExpectedCoverage(
            Color32[] pixels,
            int width,
            int height)
        {
            int opaque = 0;
            int bottom = 0;
            int right = 0;
            int bottomLimit = Mathf.RoundToInt(height * 0.22f);
            int rightStart = Mathf.RoundToInt(width * 0.73f);
            for (int y = 0; y < height; y++)
            {
                int row = y * width;
                for (int x = 0; x < width; x++)
                {
                    if (pixels[row + x].a <= 8)
                    {
                        continue;
                    }

                    opaque++;
                    if (y < bottomLimit)
                    {
                        bottom++;
                    }
                    if (x >= rightStart)
                    {
                        right++;
                    }
                }
            }

            Assert.That(opaque, Is.GreaterThan(10000), "HUD alpha plane is empty.");
            Assert.That(bottom, Is.GreaterThan(1000), "Bottom player/action HUD ROI is empty.");
            Assert.That(right, Is.GreaterThan(1000), "Right action/summon HUD ROI is empty.");
        }

        private static void AssertCaptureHasVariedColor(Color32[] pixels)
        {
            var sampledColors = new HashSet<uint>();
            for (int i = 0; i < pixels.Length; i += 97)
            {
                Color32 color = pixels[i];
                sampledColors.Add(
                    ((uint)color.r << 24)
                    | ((uint)color.g << 16)
                    | ((uint)color.b << 8)
                    | color.a);
            }

            Assert.That(
                sampledColors.Count,
                Is.GreaterThan(128),
                "Capture is effectively a flat clear frame.");
        }

        private static Rect ResolveTouchRect(Rect visualRect, RectTransform touch)
        {
            float left = visualRect.xMin + touch.offsetMin.x;
            float right = visualRect.xMax + touch.offsetMax.x;
            float top = visualRect.yMin - touch.offsetMax.y;
            float bottom = visualRect.yMax - touch.offsetMin.y;
            return Rect.MinMaxRect(left, top, right, bottom);
        }

        private static void AssertTouchInsets(
            RectTransform touch,
            Vector4 expectedInsets,
            string context)
        {
            AssertVector(
                touch.offsetMin,
                new Vector2(expectedInsets.x, expectedInsets.w));
            AssertVector(
                touch.offsetMax,
                new Vector2(-expectedInsets.z, -expectedInsets.y));
            Assert.That(touch.anchorMin, Is.EqualTo(Vector2.zero), context);
            Assert.That(touch.anchorMax, Is.EqualTo(Vector2.one), context);
        }

        private static void AssertPairwiseGap(
            IReadOnlyDictionary<string, Rect> rects,
            float minimumGap)
        {
            KeyValuePair<string, Rect>[] entries = rects.ToArray();
            for (int i = 0; i < entries.Length; i++)
            {
                for (int j = i + 1; j < entries.Length; j++)
                {
                    float horizontalGap = Mathf.Max(
                        entries[j].Value.xMin - entries[i].Value.xMax,
                        entries[i].Value.xMin - entries[j].Value.xMax);
                    float verticalGap = Mathf.Max(
                        entries[j].Value.yMin - entries[i].Value.yMax,
                        entries[i].Value.yMin - entries[j].Value.yMax);
                    float separation = Mathf.Max(horizontalGap, verticalGap);
                    Assert.That(
                        separation,
                        Is.GreaterThanOrEqualTo(minimumGap),
                        $"{entries[i].Key} and {entries[j].Key} touch regions overlap or are too close.");
                }
            }
        }

        private static GameObject RequireCanonicalTargetPrefab()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CanonicalPrefabPath);
            Assert.That(prefab, Is.Not.Null, $"Missing canonical combat HUD: {CanonicalPrefabPath}");
            CombatHudCelestialTargetLayoutProfile marker =
                prefab.GetComponent<CombatHudCelestialTargetLayoutProfile>();
            Assert.That(marker, Is.Not.Null, "Canonical combat HUD is not Target v23.");
            Assert.That(
                marker.Version,
                Is.EqualTo(CombatHudCelestialTargetLayoutProfile.LayoutVersion));
            return prefab;
        }

        private static void AssertPresenterReference(
            SerializedObject serialized,
            string propertyName,
            GameObject prefab,
            string expectedObjectName,
            Type expectedType)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            Assert.That(property, Is.Not.Null, propertyName);
            Component expected = Require(prefab.transform, expectedObjectName)
                .GetComponent(expectedType);
            Assert.That(expected, Is.Not.Null, $"{expectedObjectName}:{expectedType.Name}");
            Assert.That(
                property.objectReferenceValue,
                Is.SameAs(expected),
                $"CombatHudPresenter.{propertyName}");
            Assert.That(
                AssetDatabase.GetAssetPath(property.objectReferenceValue),
                Is.EqualTo(CanonicalPrefabPath),
                $"CombatHudPresenter.{propertyName} must reference the canonical prefab child.");
        }

        private static string ComputeManagedHierarchyHash(GameObject prefab)
        {
            var signature = new StringBuilder(32768);
            for (int i = 0; i < ManagedTargetRoots.Length; i++)
            {
                Transform managedRoot = Require(prefab.transform, ManagedTargetRoots[i]);
                AppendManagedSignature(managedRoot, ManagedTargetRoots[i], signature);
            }

            using (SHA256 sha = SHA256.Create())
            {
                byte[] digest = sha.ComputeHash(Encoding.UTF8.GetBytes(signature.ToString()));
                return BitConverter.ToString(digest).Replace("-", string.Empty);
            }
        }

        private static void AppendManagedSignature(
            Transform current,
            string path,
            StringBuilder signature)
        {
            signature.Append(path)
                .Append('|').Append(current.gameObject.activeSelf ? '1' : '0')
                .Append('|').Append(current.GetSiblingIndex());
            Component[] components = current.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] != null)
                {
                    signature.Append('|').Append(components[i].GetType().FullName);
                }
            }

            if (current is RectTransform rect)
            {
                AppendVector(signature, rect.anchorMin);
                AppendVector(signature, rect.anchorMax);
                AppendVector(signature, rect.pivot);
                AppendVector(signature, rect.anchoredPosition);
                AppendVector(signature, rect.sizeDelta);
                AppendVector(signature, rect.localScale);
                signature.Append('|').Append(
                    rect.localEulerAngles.z.ToString("R", CultureInfo.InvariantCulture));
            }

            Image image = current.GetComponent<Image>();
            if (image != null)
            {
                signature.Append("|IMG:")
                    .Append(AssetDatabase.GetAssetPath(image.sprite).Replace('\\', '/'))
                    .Append('|').Append(AssetDatabase.GetAssetPath(image.material).Replace('\\', '/'));
                AppendColor(signature, image.color);
                signature.Append('|').Append(image.raycastTarget ? '1' : '0')
                    .Append('|').Append((int)image.type)
                    .Append('|').Append((int)image.fillMethod)
                    .Append('|').Append(image.fillAmount.ToString("R", CultureInfo.InvariantCulture));
            }

            Text textComponent = current.GetComponent<Text>();
            if (textComponent != null)
            {
                signature.Append("|TXT:").Append(textComponent.text)
                    .Append('|').Append(AssetDatabase.GetAssetPath(textComponent.font).Replace('\\', '/'))
                    .Append('|').Append(textComponent.fontSize)
                    .Append('|').Append((int)textComponent.alignment)
                    .Append('|').Append(textComponent.raycastTarget ? '1' : '0');
                AppendColor(signature, textComponent.color);
            }

            signature.AppendLine();
            for (int i = 0; i < current.childCount; i++)
            {
                Transform child = current.GetChild(i);
                AppendManagedSignature(child, $"{path}/{child.name}", signature);
            }
        }

        private static void AppendVector(StringBuilder builder, Vector2 value)
        {
            builder.Append('|').Append(value.x.ToString("R", CultureInfo.InvariantCulture))
                .Append(',').Append(value.y.ToString("R", CultureInfo.InvariantCulture));
        }

        private static void AppendVector(StringBuilder builder, Vector3 value)
        {
            builder.Append('|').Append(value.x.ToString("R", CultureInfo.InvariantCulture))
                .Append(',').Append(value.y.ToString("R", CultureInfo.InvariantCulture))
                .Append(',').Append(value.z.ToString("R", CultureInfo.InvariantCulture));
        }

        private static void AppendColor(StringBuilder builder, Color value)
        {
            builder.Append('|').Append(value.r.ToString("R", CultureInfo.InvariantCulture))
                .Append(',').Append(value.g.ToString("R", CultureInfo.InvariantCulture))
                .Append(',').Append(value.b.ToString("R", CultureInfo.InvariantCulture))
                .Append(',').Append(value.a.ToString("R", CultureInfo.InvariantCulture));
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
                Transform button = Require(prefab.transform, buttonName);
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
                Assert.That(bridgeId, Is.Not.Zero, $"Scene action {actionId} lost its input bridge.");
                Assert.That(result.ContainsKey(sourceGameObjectId), Is.False);
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
            Assert.That(localRootIds, Has.Length.LessThanOrEqualTo(1));

            var candidateGameObjectIds = new List<long>(localRootIds);
            Dictionary<long, (long GameObjectId, long ParentTransformId)> transforms =
                ParseSceneTransformHierarchy(yaml);
            long parentTransformId = RequireCanonicalPrefabParentTransformId(yaml);
            var visitedTransforms = new HashSet<long>();
            while (parentTransformId != 0)
            {
                Assert.That(visitedTransforms.Add(parentTransformId), Is.True);
                Assert.That(
                    transforms.TryGetValue(parentTransformId, out var transform),
                    Is.True,
                    $"{scenePath} HUD parent transform {parentTransformId} is missing.");
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
                $"{scenePath} canonical Target HUD has no Canvas in its parent chain.");

            string[] scalers = EnumerateMonoBehaviourBodies(yaml)
                .Where(body => body.IndexOf(
                        "m_EditorClassIdentifier: UnityEngine.UI::UnityEngine.UI.CanvasScaler",
                        StringComparison.Ordinal) >= 0
                    && ReadLong(body, @"^  m_GameObject: \{fileID: (?<value>-?\d+)\}")
                    == nearestCanvasGameObjectId)
                .ToArray();
            Assert.That(scalers, Has.Length.EqualTo(1));
            Assert.That(scalers[0], Does.Match(@"(?m)^  m_UiScaleMode: 1$"));
            Assert.That(
                scalers[0],
                Does.Match(@"(?m)^  m_ReferenceResolution: \{x: 2560(?:\.0+)?, y: 1440(?:\.0+)?\}$"));
            Assert.That(scalers[0], Does.Match(@"(?m)^  m_ScreenMatchMode: 0$"));
            Assert.That(scalers[0], Does.Match(@"(?m)^  m_MatchWidthOrHeight: 1(?:\.0+)?$"));
        }

        private static void AssertCanonicalJoystickBinding(
            GameObject prefab,
            string scenePath,
            string yaml,
            IReadOnlyDictionary<long, long> strippedGameObjects)
        {
            long expectedRingId = RequireLocalFileId(
                Require(prefab.transform, "MoveJoystickRing").gameObject);
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
                    Is.Not.Zero);
            }

            Assert.That(found, Is.EqualTo(1), $"{scenePath} must bind one Target joystick.");
        }

        private static void AssertNoCanonicalTargetVisualOverrides(
            GameObject prefab,
            string scenePath,
            string yaml)
        {
            Dictionary<long, UnityEngine.Object> prefabObjects = BuildPrefabLocalObjectMap(prefab);
            string pattern =
                $@"^    - target: \{{fileID: (?<sourceId>-?\d+), guid: {CombatHudPrefabGuid}, type: 3\}}\r?\n"
                + @"      propertyPath: (?<property>[^\r\n]+)";
            var offenders = new List<string>();
            foreach (Match match in Regex.Matches(yaml, pattern, RegexOptions.Multiline))
            {
                long sourceId = long.Parse(match.Groups["sourceId"].Value);
                string property = match.Groups["property"].Value;
                if (!prefabObjects.TryGetValue(sourceId, out UnityEngine.Object sourceObject))
                {
                    continue;
                }

                bool managed = IsManagedTargetSourceObject(sourceObject, prefab);
                bool presenterBinding = sourceObject is Component component
                    && component.GetType().FullName == "DimensionBrawl.UI.CombatHudPresenter"
                    && IsPresenterVisualBindingProperty(property);
                bool targetVisual = managed
                    && ((sourceObject is RectTransform && IsSceneLayoutProperty(property))
                        || sourceObject is Text
                        || (sourceObject is Graphic && IsSceneGraphicProperty(property))
                        || (sourceObject is CanvasGroup
                            && (property == "m_Alpha"
                                || property == "m_Interactable"
                                || property == "m_BlocksRaycasts"
                                || property == "m_IgnoreParentGroups"))
                        || (sourceObject is GameObject && property == "m_IsActive"));
                if (presenterBinding || targetVisual)
                {
                    offenders.Add($"{sourceId}:{property}");
                }
            }

            Assert.That(
                offenders,
                Is.Empty,
                $"{scenePath} overrides Target Presenter/visual/layout/Text/active data: "
                    + string.Join(", ", offenders));
        }

        private static Dictionary<long, UnityEngine.Object> BuildPrefabLocalObjectMap(
            GameObject prefab)
        {
            var result = new Dictionary<long, UnityEngine.Object>();
            foreach (Transform transform in prefab.GetComponentsInChildren<Transform>(true))
            {
                AddPrefabObject(result, transform.gameObject);
                foreach (Component component in transform.GetComponents<Component>())
                {
                    if (component != null)
                    {
                        AddPrefabObject(result, component);
                    }
                }
            }

            return result;
        }

        private static void AddPrefabObject(
            IDictionary<long, UnityEngine.Object> objects,
            UnityEngine.Object assetObject)
        {
            bool found = AssetDatabase.TryGetGUIDAndLocalFileIdentifier(
                assetObject,
                out string guid,
                out long localId);
            Assert.That(found, Is.True, assetObject.name);
            Assert.That(guid, Is.EqualTo(CombatHudPrefabGuid));
            objects[localId] = assetObject;
        }

        private static bool IsManagedTargetSourceObject(
            UnityEngine.Object sourceObject,
            GameObject prefab)
        {
            GameObject gameObject = sourceObject as GameObject;
            if (sourceObject is Component component)
            {
                gameObject = component.gameObject;
            }
            if (gameObject == null || gameObject == prefab)
            {
                return false;
            }

            Transform current = gameObject.transform;
            while (current != null && current != prefab.transform)
            {
                if (SceneManagedVisualRoots.Contains(current.name))
                {
                    return true;
                }
                current = current.parent;
            }

            return false;
        }

        private static bool IsPresenterVisualBindingProperty(string path)
        {
            if (PresenterDirectVisualBindings.Contains(path)
                || path == "aimReticleSegments.Array.size"
                || path.StartsWith("aimReticleSegments.Array.data[", StringComparison.Ordinal))
            {
                return true;
            }

            if (path.StartsWith("actionSlots.Array.data[", StringComparison.Ordinal))
            {
                return HasSerializedFieldSuffix(
                    path,
                    "labelText", "cooldownText", "cooldownFill", "readyProgressFill",
                    "readyGlowImage", "canvasGroup");
            }
            return path.StartsWith("summonSlots.Array.data[", StringComparison.Ordinal)
                && HasSerializedFieldSuffix(
                    path,
                    "labelText", "stateText", "cooldownFill", "iconImage",
                    "unavailableIconImage", "readyGlowImage", "readyRingImage",
                    "readySparkImage", "canvasGroup");
        }

        private static bool HasSerializedFieldSuffix(string path, params string[] names)
        {
            int separator = path.LastIndexOf('.');
            string suffix = separator >= 0 ? path.Substring(separator + 1) : path;
            return names.Contains(suffix, StringComparer.Ordinal);
        }

        private static bool IsSceneLayoutProperty(string property)
        {
            return property.StartsWith("m_AnchoredPosition.", StringComparison.Ordinal)
                || property.StartsWith("m_AnchorMin.", StringComparison.Ordinal)
                || property.StartsWith("m_AnchorMax.", StringComparison.Ordinal)
                || property.StartsWith("m_Pivot.", StringComparison.Ordinal)
                || property.StartsWith("m_SizeDelta.", StringComparison.Ordinal)
                || property.StartsWith("m_LocalScale.", StringComparison.Ordinal)
                || property.StartsWith("m_LocalRotation.", StringComparison.Ordinal)
                || property.StartsWith("m_LocalPosition.", StringComparison.Ordinal)
                || property.StartsWith("m_LocalEulerAnglesHint.", StringComparison.Ordinal)
                || property == "m_RootOrder"
                || property == "m_ConstrainProportionsScale";
        }

        private static bool IsSceneGraphicProperty(string property)
        {
            return property == "m_Enabled"
                || property == "m_Sprite"
                || property == "m_Material"
                || property == "m_Type"
                || property == "m_PreserveAspect"
                || property == "m_FillMethod"
                || property == "m_FillOrigin"
                || property == "m_FillClockwise"
                || property == "m_FillAmount"
                || property == "m_RaycastTarget"
                || property.StartsWith("m_RaycastPadding.", StringComparison.Ordinal)
                || property.StartsWith("m_Color.", StringComparison.Ordinal);
        }

        private static void AssertCanonicalAddedPresentationCleanup(
            string scenePath,
            string yaml)
        {
            string instanceBody = RequireCanonicalPrefabInstanceBody(yaml);
            Match addedGameObjectsSection = Regex.Match(
                instanceBody,
                @"(?ms)^    m_AddedGameObjects:\r?\n(?<body>.*?)(?=^    m_AddedComponents:)");
            bool noAddedGameObjects = Regex.IsMatch(
                instanceBody,
                @"(?m)^    m_AddedGameObjects: \[\]$");
            Assert.That(
                addedGameObjectsSection.Success || noAddedGameObjects,
                Is.True,
                scenePath);
            long[] addedTransformIds = noAddedGameObjects
                ? Array.Empty<long>()
                : Regex.Matches(
                        addedGameObjectsSection.Groups["body"].Value,
                        @"(?m)^      addedObject: \{fileID: (?<value>-?\d+)\}$")
                    .Cast<Match>()
                    .Select(match => long.Parse(match.Groups["value"].Value))
                    .ToArray();
            string[] addedNames = addedTransformIds
                .Select(id => ReadAddedGameObjectName(yaml, id))
                .ToArray();
            string[] expectedNames = string.Equals(
                    scenePath,
                    CourtyardScenePath,
                    StringComparison.Ordinal)
                ? Array.Empty<string>()
                : new[] { "AimDragArea" };
            long[] expectedAddedTransformIds = expectedNames.Length == 0
                ? Array.Empty<long>()
                : new[] { 437249720L };
            Assert.That(
                addedNames,
                Is.EquivalentTo(expectedNames),
                $"{scenePath} retained a legacy scene-added visual GameObject.");
            Assert.That(
                addedTransformIds,
                Is.EquivalentTo(expectedAddedTransformIds),
                $"{scenePath} changed the preserved AimDragArea file ID.");

            Match addedComponentsSection = Regex.Match(
                instanceBody,
                @"(?ms)^    m_AddedComponents:\r?\n(?<body>.*?)(?=^  m_SourcePrefab:)");
            Assert.That(addedComponentsSection.Success, Is.True, scenePath);
            long[] addedComponentIds = Regex.Matches(
                    addedComponentsSection.Groups["body"].Value,
                    @"(?m)^      addedObject: \{fileID: (?<value>-?\d+)\}$")
                .Cast<Match>()
                .Select(match => long.Parse(match.Groups["value"].Value))
                .ToArray();
            int expectedActionCount = string.Equals(
                scenePath,
                CourtyardScenePath,
                StringComparison.Ordinal)
                ? 2
                : 7;
            int expectedAddedComponentCount = expectedActionCount == 2 ? 7 : 8;
            Assert.That(
                addedComponentIds,
                Has.Length.EqualTo(expectedAddedComponentCount));
            long[] expectedAddedComponentIds = expectedActionCount == 2
                ? new[]
                {
                    1456148931L, 1456148930L, 1456148929L, 1456148928L,
                    1456148926L, 780689904L, 1531466474L
                }
                : new[]
                {
                    1406907996L, 292477426L, 183828589L, 1554268320L,
                    1558434457L, 520126473L, 1839690757L, 1742448631L
                };
            Assert.That(
                addedComponentIds,
                Is.EquivalentTo(expectedAddedComponentIds),
                $"{scenePath} changed a preserved scene-owned component file ID.");
            (int ClassId, string Body)[] componentRecords = addedComponentIds
                .Select(id => ReadSerializedObjectRecordById(yaml, id))
                .ToArray();
            Assert.That(
                componentRecords.Count(record =>
                    record.Body.Contains($"guid: {PointerInputScriptGuid}")),
                Is.EqualTo(expectedActionCount));
            Assert.That(
                componentRecords.Count(record =>
                    record.Body.Contains($"guid: {VirtualJoystickScriptGuid}")),
                Is.EqualTo(1));
            Assert.That(
                componentRecords.Any(record => record.Body.Contains(
                    "m_EditorClassIdentifier: UnityEngine.UI::UnityEngine.UI.Outline")),
                Is.False,
                $"{scenePath} retained a legacy scene-added Outline component.");

            if (expectedActionCount == 2)
            {
                Assert.That(
                    componentRecords.Count(record => record.ClassId == 223),
                    Is.EqualTo(1),
                    "Courtyard must preserve its scene-owned Canvas component.");
                Assert.That(
                    componentRecords.Count(record => record.Body.Contains(
                        "m_EditorClassIdentifier: UnityEngine.UI::UnityEngine.UI.CanvasScaler")),
                    Is.EqualTo(1));
                Assert.That(
                    componentRecords.Count(record => record.Body.Contains(
                        "m_EditorClassIdentifier: UnityEngine.UI::UnityEngine.UI.GraphicRaycaster")),
                    Is.EqualTo(1));
                Assert.That(
                    componentRecords.Count(record => record.Body.Contains(
                        "DimensionBrawl.UI.OneRowCombatHudBinder")),
                    Is.EqualTo(1));
            }
            else
            {
                Assert.That(
                    componentRecords.All(record => record.ClassId == 114),
                    Is.True,
                    $"{scenePath} unexpected scene-added functional component type.");
            }

            int expectedAimDrag = expectedActionCount == 7 ? 1 : 0;
            Assert.That(
                CountOccurrences(yaml, $"guid: {AimDragInputScriptGuid}"),
                Is.EqualTo(expectedAimDrag),
                $"{scenePath} changed the scene-owned AimDragArea contract.");

            if (string.Equals(scenePath, CourtyardScenePath, StringComparison.Ordinal))
            {
                const string interactablePattern =
                    @"^    - target: \{fileID: -?\d+, guid: "
                    + CombatHudPrefabGuid
                    + @", type: 3\}\r?\n      propertyPath: m_Interactable\r?\n      value: 0$";
                Assert.That(
                    Regex.Matches(yaml, interactablePattern, RegexOptions.Multiline),
                    Has.Count.EqualTo(5),
                    "Courtyard must preserve its five functional disabled-button overrides.");
            }
        }

        private static string ReadAddedGameObjectName(string yaml, long transformId)
        {
            string transformPattern =
                $@"^--- !u!224 &{transformId}\r?\nRectTransform:\r?\n"
                + @"(?<body>.*?)(?=^--- !u!|\z)";
            Match transform = Regex.Match(
                yaml,
                transformPattern,
                RegexOptions.Multiline | RegexOptions.Singleline);
            Assert.That(transform.Success, Is.True, transformId.ToString());
            long gameObjectId = ReadLong(
                transform.Groups["body"].Value,
                @"^  m_GameObject: \{fileID: (?<value>-?\d+)\}");
            string gameObjectPattern =
                $@"^--- !u!1 &{gameObjectId}\r?\nGameObject:\r?\n"
                + @"(?<body>.*?)(?=^--- !u!|\z)";
            Match gameObject = Regex.Match(
                yaml,
                gameObjectPattern,
                RegexOptions.Multiline | RegexOptions.Singleline);
            Assert.That(gameObject.Success, Is.True, gameObjectId.ToString());
            Match name = Regex.Match(
                gameObject.Groups["body"].Value,
                @"^  m_Name: (?<value>[^\r\n]+)$",
                RegexOptions.Multiline);
            Assert.That(name.Success, Is.True, gameObjectId.ToString());
            return name.Groups["value"].Value.Trim();
        }

        private static (int ClassId, string Body) ReadSerializedObjectRecordById(
            string yaml,
            long componentId)
        {
            string pattern =
                $@"^--- !u!(?<classId>\d+) &{componentId}\r?\n[^\r\n]+:\r?\n"
                + @"(?<body>.*?)(?=^--- !u!|\z)";
            Match match = Regex.Match(
                yaml,
                pattern,
                RegexOptions.Multiline | RegexOptions.Singleline);
            Assert.That(match.Success, Is.True, componentId.ToString());
            return (
                int.Parse(match.Groups["classId"].Value),
                match.Groups["body"].Value);
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
            return ReadLong(
                RequireCanonicalPrefabInstanceBody(yaml),
                @"^    m_TransformParent: \{fileID: (?<value>-?\d+)\}");
        }

        private static string RequireCanonicalPrefabInstanceBody(string yaml)
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
            Assert.That(canonicalInstances, Has.Length.EqualTo(1));
            return canonicalInstances[0];
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
            Assert.That(found, Is.True, $"Could not resolve local ID for {assetObject.name}.");
            Assert.That(guid, Is.EqualTo(CombatHudPrefabGuid));
            return localFileId;
        }

        private static string ReadAssetText(string assetPath)
        {
            return File.ReadAllText(ToAbsolutePath(assetPath));
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

        private static GameObject RequireStagingOrIgnore()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(StagingPrefabPath);
            if (prefab == null)
            {
                Assert.Ignore("Target review staging prefab has not been assembled yet.");
            }
            return prefab;
        }

        private static void AssertTargetSprite(Transform transform)
        {
            Assert.That(transform, Is.Not.Null);
            Image image = transform.GetComponent<Image>();
            Assert.That(image, Is.Not.Null, GetPath(transform));
            Assert.That(image.sprite, Is.Not.Null, GetPath(transform));
            Assert.That(
                AssetDatabase.GetAssetPath(image.sprite).Replace('\\', '/'),
                Does.StartWith(TargetArtRoot),
                GetPath(transform));
            Assert.That(image.raycastTarget, Is.False, GetPath(transform));
        }

        private static Transform Require(Transform root, string name)
        {
            Transform[] matches = FindAll(root, name);
            Assert.That(matches.Length, Is.EqualTo(1), name);
            return matches[0];
        }

        private static Transform[] FindAll(Transform root, string name)
        {
            return root.GetComponentsInChildren<Transform>(true)
                .Where(candidate => string.Equals(candidate.name, name, StringComparison.Ordinal))
                .ToArray();
        }

        private static void AssertRect(Rect actual, Rect expected)
        {
            Assert.That(actual.x, Is.EqualTo(expected.x).Within(Tolerance));
            Assert.That(actual.y, Is.EqualTo(expected.y).Within(Tolerance));
            Assert.That(actual.width, Is.EqualTo(expected.width).Within(Tolerance));
            Assert.That(actual.height, Is.EqualTo(expected.height).Within(Tolerance));
        }

        private static void AssertRightBottomDesignRect(
            RectTransform actual,
            Rect expected)
        {
            Assert.That(actual, Is.Not.Null);
            AssertVector(actual.anchorMin, new Vector2(1f, 0f));
            AssertVector(actual.anchorMax, new Vector2(1f, 0f));
            AssertVector(actual.pivot, new Vector2(1f, 0f));
            AssertVector(actual.sizeDelta, expected.size);
            AssertVector(
                actual.anchoredPosition,
                new Vector2(
                    -(CombatHudCelestialTargetLayoutProfile.DesignWidth - expected.xMax),
                    CombatHudCelestialTargetLayoutProfile.DesignHeight - expected.yMax));
        }

        private static void AssertRightTopDesignRect(
            RectTransform actual,
            Rect expected)
        {
            Assert.That(actual, Is.Not.Null);
            AssertVector(actual.anchorMin, Vector2.one);
            AssertVector(actual.anchorMax, Vector2.one);
            AssertVector(actual.pivot, Vector2.one);
            AssertVector(actual.sizeDelta, expected.size);
            AssertVector(
                actual.anchoredPosition,
                new Vector2(
                    -(CombatHudCelestialTargetLayoutProfile.DesignWidth - expected.xMax),
                    -expected.yMin));
        }

        private static void AssertLeftBottomDesignRect(
            RectTransform actual,
            Rect expected)
        {
            Assert.That(actual, Is.Not.Null);
            AssertVector(actual.anchorMin, Vector2.zero);
            AssertVector(actual.anchorMax, Vector2.zero);
            AssertVector(actual.pivot, Vector2.zero);
            AssertVector(actual.sizeDelta, expected.size);
            AssertVector(
                actual.anchoredPosition,
                new Vector2(
                    expected.xMin,
                    CombatHudCelestialTargetLayoutProfile.DesignHeight - expected.yMax));
        }

        private static void AssertVector(Vector2 actual, Vector2 expected)
        {
            Assert.That(actual.x, Is.EqualTo(expected.x).Within(Tolerance));
            Assert.That(actual.y, Is.EqualTo(expected.y).Within(Tolerance));
        }

        private static void AssertColor(Color actual, Color expected, string context)
        {
            Assert.That(actual.r, Is.EqualTo(expected.r).Within(0.001f), context);
            Assert.That(actual.g, Is.EqualTo(expected.g).Within(0.001f), context);
            Assert.That(actual.b, Is.EqualTo(expected.b).Within(0.001f), context);
            Assert.That(actual.a, Is.EqualTo(expected.a).Within(0.001f), context);
        }

        private static string ToAbsolutePath(string assetPath)
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            Assert.That(projectRoot, Is.Not.Null.And.Not.Empty);
            return Path.Combine(projectRoot, assetPath.Replace('/', Path.DirectorySeparatorChar));
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
