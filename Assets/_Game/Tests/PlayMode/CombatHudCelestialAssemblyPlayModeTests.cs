using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using DimensionBrawl.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace DimensionBrawl.Tests
{
    public sealed class CombatHudCelestialAssemblyPlayModeTests
    {
        private const string CombatHudPrefabPath =
            "Assets/_Game/UI/CombatHud/PF_UI_CombatHud.prefab";
        private const string CombatHudActionCatalogPath =
            "Assets/_Game/DesignData/UI/DB_CombatHudActions.asset";
        private const string CelestialHudArtRoot =
            "Assets/_Game/UI/CombatHud/Art/CelestialHud/";
        private const string HudFontPath =
            "Assets/_Game/Art/Fonts/Pretendard/Pretendard-SemiBold.otf";
        private const string FlowShaderName = "DimensionBrawl/UI/CelestialFlow";
        private const float LayoutTolerance = 0.1f;
        private const float MinimumMobileTouchSize = 88f;
        private const float MinimumMobileEdgeMargin = 32f;
        private const string CombatHudPrefabGuid = "4e5297b5734b6664b935ffb1ae9b48b6";
        private const string PointerInputScriptGuid = "e764d6dd84658b34d9df199b296e940b";
        private const string VirtualJoystickScriptGuid = "d85f5878113320a48a4d953bd098c390";

        private const string CorridorScenePath =
            "Assets/_Game/Scenes/OlympusCorridorInvasionStage.unity";
        private const string StationScenePath =
            "Assets/_Game/Scenes/OlympusStationCombatStage.unity";
        private const string CourtyardScenePath =
            "Assets/_Game/Scenes/OlympusCourtyardDrillStage.unity";

        private static readonly string[] CelestialSpriteObjectNames =
        {
            "TopLeftPanel",
            "BossNameArea",
            "BossHpFill",
            "BossCostFill",
            "PlayerPortraitFrame",
            "PlayerPortrait",
            "HealthBar_Track",
            "HealthBar",
            "ResourceBar_Track",
            "ResourceBar",
            "PlayerAmmoChip",
            "CenterAimReticle",
            "BasicAttackButton",
            "DodgeButton",
            "Skill1Button",
            "UltimateButton",
            "PauseButton",
            "MoveJoystickRing",
            "MoveJoystickKnob",
            "SummonSlot1Button",
            "SummonSlot2Button",
            "SummonSlot3Button"
        };

        private static readonly string[] FlowFillObjectNames =
        {
            "HealthBar",
            "ResourceBar",
            "BossHpFill",
            "BossCostFill"
        };

        private static readonly (string ButtonName, int ActionId)[] ActionButtonBindings =
        {
            ("BasicAttackButton", 100),
            ("DodgeButton", 110),
            ("Skill1Button", 120),
            ("UltimateButton", 130),
            ("SummonSlot1Button", 200),
            ("SummonSlot2Button", 210),
            ("SummonSlot3Button", 220)
        };

        private enum MobileEdgeAnchor
        {
            LeftBottom,
            RightTop,
            RightBottom
        }

        [SetUp]
        public void IgnoreLegacyV1ContractsAfterExplicitReviewedPromotion()
        {
            GameObject canonical = AssetDatabase.LoadAssetAtPath<GameObject>(CombatHudPrefabPath);
            if (canonical != null
                && (canonical.GetComponent<CombatHudCelestialTargetLayoutProfile>() != null
                    || canonical.GetComponent<CombatHudCelestialV2LayoutProfile>() != null))
            {
                Assert.Ignore(
                    "Canonical combat HUD uses a reviewed post-V1 layout. "
                    + "The matching versioned assembly suite owns the replacement contracts; "
                    + "this suite remains active only for the rollback V1 prefab.");
            }
        }

        [Test]
        public void CanonicalPrefabUsesCelestialSpritesForEveryPrimaryHudSurface()
        {
            GameObject prefab = RequireCombatHudPrefab();
            RequireUniqueNamedTransform(prefab.transform, "DimensionHudSkinRoot");

            for (int i = 0; i < CelestialSpriteObjectNames.Length; i++)
            {
                Image image = RequireUniqueNamedImage(prefab.transform, CelestialSpriteObjectNames[i]);
                AssertCelestialSprite(image);
            }

            for (int slot = 1; slot <= 3; slot++)
            {
                Transform button = RequireUniqueNamedTransform(prefab.transform, $"SummonSlot{slot}Button");
                AssertCelestialSprite(RequireUniqueNamedImage(button, "Icon"));
                AssertCelestialSprite(RequireUniqueNamedImage(button, "IconDisabled"));
            }

            AssertClearedImage(prefab.transform, "BossHpBackground");
            AssertClearedImage(prefab.transform, "BossCostBackground");
        }

        [Test]
        public void SummonPortraitsAreClippedBehindNonInteractiveFrameOverlays()
        {
            GameObject prefab = RequireCombatHudPrefab();
            for (int slotIndex = 1; slotIndex <= 3; slotIndex++)
            {
                RectTransform slot = RequireRectTransform(
                    prefab.transform,
                    $"SummonSlot{slotIndex}Button");
                Image slotFrame = RequireImage(slot);
                Image icon = RequireUniqueNamedImage(slot, "Icon");
                Image disabledIcon = RequireUniqueNamedImage(slot, "IconDisabled");

                Assert.That(icon.raycastTarget, Is.False, $"S{slotIndex} portrait must not consume taps.");
                Assert.That(disabledIcon.raycastTarget, Is.False, $"S{slotIndex} disabled portrait must not consume taps.");
                Assert.That(icon.maskable, Is.True, $"S{slotIndex} portrait must participate in UI clipping.");
                Assert.That(disabledIcon.maskable, Is.True, $"S{slotIndex} disabled portrait must participate in UI clipping.");

                Transform iconClip = RequireClipAncestor(icon.transform, slot);
                Transform disabledClip = RequireClipAncestor(disabledIcon.transform, slot);
                Assert.That(
                    disabledClip,
                    Is.SameAs(iconClip),
                    $"S{slotIndex} enabled and disabled portraits must share one aperture.");
                Assert.That(iconClip.name, Is.EqualTo("PortraitMask"));
                Assert.That(icon.transform.parent, Is.SameAs(iconClip));
                Assert.That(disabledIcon.transform.parent, Is.SameAs(iconClip));
                RectTransform clipRect = iconClip as RectTransform;
                Assert.That(clipRect, Is.Not.Null);
                AssertRectContains(slot, clipRect, $"S{slotIndex} portrait aperture");
                Assert.That(
                    clipRect.rect.width,
                    Is.LessThan(slot.rect.width - 1f),
                    $"S{slotIndex} aperture must stay inside the horizontal frame edge.");
                Assert.That(
                    clipRect.rect.height,
                    Is.LessThan(slot.rect.height - 1f),
                    $"S{slotIndex} aperture must stay inside the vertical frame edge.");

                Mask stencilMask = iconClip.GetComponent<Mask>();
                RectMask2D rectMask = iconClip.GetComponent<RectMask2D>();
                Assert.That(
                    (stencilMask != null && stencilMask.enabled)
                    || (rectMask != null && rectMask.enabled),
                    Is.True,
                    $"S{slotIndex} portrait aperture has no enabled Mask or RectMask2D.");
                Graphic clipGraphic = iconClip.GetComponent<Graphic>();
                Assert.That(clipGraphic, Is.Not.Null, $"S{slotIndex} aperture needs a mask graphic.");
                Image clipImage = clipGraphic as Image;
                Assert.That(clipImage, Is.Not.Null, $"S{slotIndex} aperture graphic must be an Image.");
                AssertCelestialSprite(clipImage);
                if (clipGraphic != null)
                {
                    Assert.That(clipGraphic.raycastTarget, Is.False, $"S{slotIndex} aperture must not consume taps.");
                }
                if (stencilMask != null)
                {
                    Assert.That(stencilMask.showMaskGraphic, Is.False, $"S{slotIndex} aperture graphic must stay hidden.");
                }

                Image cooldownFill = RequireUniqueNamedImage(slot, "CooldownFill");
                Transform cooldownClip = RequireClipAncestor(cooldownFill.transform, slot);
                Assert.That(
                    cooldownClip,
                    Is.SameAs(iconClip),
                    $"S{slotIndex} cooldown must share the portrait aperture.");
                Assert.That(cooldownFill.transform.parent, Is.SameAs(iconClip));
                Assert.That(cooldownFill.raycastTarget, Is.False, $"S{slotIndex} cooldown must not consume taps.");
                Assert.That(cooldownFill.maskable, Is.True, $"S{slotIndex} cooldown must participate in UI clipping.");
                RectTransform cooldownRect = cooldownFill.rectTransform;
                AssertVector2(cooldownRect.anchorMin, Vector2.zero, $"S{slotIndex} cooldown anchorMin");
                AssertVector2(cooldownRect.anchorMax, Vector2.one, $"S{slotIndex} cooldown anchorMax");
                AssertVector2(cooldownRect.pivot, new Vector2(0.5f, 0.5f), $"S{slotIndex} cooldown pivot");
                AssertVector2(cooldownRect.anchoredPosition, Vector2.zero, $"S{slotIndex} cooldown position");
                AssertVector2(cooldownRect.sizeDelta, Vector2.zero, $"S{slotIndex} cooldown sizeDelta");
                AssertVector2(cooldownRect.rect.size, clipRect.rect.size, $"S{slotIndex} cooldown aperture size");

                Image[] frameOverlays = slot.Cast<Transform>()
                    .Select(child => child.GetComponent<Image>())
                    .Where(image => image != null
                        && image.enabled
                        && image.sprite == slotFrame.sprite)
                    .ToArray();
                Assert.That(
                    frameOverlays,
                    Has.Length.EqualTo(1),
                    $"S{slotIndex} needs one direct-child copy of the frame above its clipped portrait.");
                Image frameOverlay = frameOverlays[0];
                Assert.That(frameOverlay.name, Is.EqualTo("FrameOverlay"));
                Assert.That(frameOverlay.raycastTarget, Is.False, $"S{slotIndex} frame overlay must not consume taps.");
                AssertCelestialSprite(frameOverlay);
                Assert.That(frameOverlay.color.a, Is.EqualTo(1f).Within(0.001f));
                Assert.That(slotFrame.raycastTarget, Is.True);
                Assert.That(
                    slotFrame.color.a,
                    Is.EqualTo(0f).Within(0.001f),
                    $"S{slotIndex} root hit graphic must not double-render beneath the portrait.");
                AssertVector2(
                    frameOverlay.rectTransform.rect.size,
                    slot.rect.size,
                    $"S{slotIndex} frame overlay size");

                Transform clipBranch = RequireDirectChildBranch(slot, iconClip);
                Assert.That(
                    frameOverlay.transform.GetSiblingIndex(),
                    Is.GreaterThan(clipBranch.GetSiblingIndex()),
                    $"S{slotIndex} frame must render after the portrait aperture branch.");
            }
        }

        [Test]
        public void PresenterKeepsActionSummonMeterAndModeSwapContracts()
        {
            GameObject prefab = RequireCombatHudPrefab();
            Component presenter = RequireComponentByTypeName(prefab, "DimensionBrawl.UI.CombatHudPresenter");
            var serializedPresenter = new SerializedObject(presenter);

            AssertObjectReferenceName(serializedPresenter, "healthFill", "HealthBar");
            AssertObjectReferenceName(serializedPresenter, "resourceFill", "ResourceBar");
            AssertObjectReferenceName(serializedPresenter, "bossHudRoot", "BossHudRoot");
            AssertObjectReferenceName(serializedPresenter, "bossHealthFill", "BossHpFill");
            AssertObjectReferenceName(serializedPresenter, "bossResourceFill", "BossCostFill");
            Assert.That(
                serializedPresenter.FindProperty("actionCatalog")?.objectReferenceValue,
                Is.Not.Null,
                "The assembled HUD must retain its action catalog reference.");

            AssertBindingArray(
                serializedPresenter.FindProperty("actionSlots"),
                new[] { 100, 110, 120, 130 },
                new[] { "labelText", "cooldownText", "cooldownFill", "canvasGroup" });
            AssertBindingArray(
                serializedPresenter.FindProperty("summonSlots"),
                new[] { 200, 210, 220 },
                new[] { "labelText", "stateText", "cooldownFill", "canvasGroup" });
            AssertBindingOwners(serializedPresenter.FindProperty("actionSlots"), prefab.transform);
            AssertBindingOwners(serializedPresenter.FindProperty("summonSlots"), prefab.transform);

            UnityEngine.Object actionCatalog =
                AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(CombatHudActionCatalogPath);
            Assert.That(actionCatalog, Is.Not.Null, $"Missing {CombatHudActionCatalogPath}.");
            var serializedCatalog = new SerializedObject(actionCatalog);
            SerializedProperty actions = serializedCatalog.FindProperty("actions");
            Assert.That(actions, Is.Not.Null);
            Assert.That(actions.isArray, Is.True);

            SerializedProperty modeSwap = FindBindingByActionId(actions, 130);
            Assert.That(modeSwap.FindPropertyRelative("canonicalName").stringValue, Is.EqualTo("Ultimate"));
            Assert.That(
                modeSwap.FindPropertyRelative("displayName").stringValue,
                Is.EqualTo("Mode Swap"),
                "The live mode-swap affordance intentionally routes through CombatHudActionId.Ultimate.");
        }

        [Test]
        public void FlowMaterialIsRestrictedToTheFourRuntimeFillImages()
        {
            GameObject prefab = RequireCombatHudPrefab();
            RequireUniqueNamedTransform(prefab.transform, "DimensionHudSkinRoot");
            var expectedFlowNames = new HashSet<string>(FlowFillObjectNames, StringComparer.Ordinal);
            var foundFlowNames = new HashSet<string>(StringComparer.Ordinal);

            Image[] images = prefab.GetComponentsInChildren<Image>(includeInactive: true);
            for (int i = 0; i < images.Length; i++)
            {
                Image image = images[i];
                Material material = image.material;
                bool usesFlow = material != null
                    && material.shader != null
                    && string.Equals(material.shader.name, FlowShaderName, StringComparison.Ordinal);

                if (expectedFlowNames.Contains(image.name))
                {
                    Assert.That(
                        usesFlow,
                        Is.True,
                        $"{image.name} must use the shared celestial-flow UI shader.");
                    Assert.That(material.HasProperty("_FlowStrength"), Is.True);
                    Assert.That(
                        material.GetFloat("_FlowStrength"),
                        Is.InRange(0f, 0.1f),
                        $"{image.name} flow must stay presentation-subtle.");
                    foundFlowNames.Add(image.name);
                    continue;
                }

                Assert.That(
                    usesFlow,
                    Is.False,
                    $"Flow leaked outside a runtime fill onto {GetHierarchyPath(image.transform)}.");
            }

            Assert.That(foundFlowNames.SetEquals(expectedFlowNames), Is.True);
        }

        [Test]
        public void ActionAndJoystickSurfacesKeepTheirPointerOwnership()
        {
            GameObject prefab = RequireCombatHudPrefab();
            RequireUniqueNamedTransform(prefab.transform, "DimensionHudSkinRoot");
            string[] actionButtons =
            {
                "BasicAttackButton",
                "DodgeButton",
                "Skill1Button",
                "UltimateButton",
                "SummonSlot1Button",
                "SummonSlot2Button",
                "SummonSlot3Button",
                "PauseButton"
            };

            for (int i = 0; i < actionButtons.Length; i++)
            {
                Transform buttonTransform = RequireUniqueNamedTransform(prefab.transform, actionButtons[i]);
                Image hitGraphic = RequireImage(buttonTransform);
                Assert.That(buttonTransform.GetComponent<Button>(), Is.Not.Null, $"{actionButtons[i]} lost Button.");
                Assert.That(hitGraphic.raycastTarget, Is.True, $"{actionButtons[i]} must remain tappable.");
            }

            Image joystickRing = RequireUniqueNamedImage(prefab.transform, "MoveJoystickRing");
            Image joystickKnob = RequireUniqueNamedImage(prefab.transform, "MoveJoystickKnob");
            Assert.That(joystickRing.raycastTarget, Is.True, "The joystick ring owns the pointer surface.");
            Assert.That(joystickKnob.raycastTarget, Is.False, "The knob must not steal drag/up events.");

            for (int i = 0; i < FlowFillObjectNames.Length; i++)
            {
                Image fill = RequireUniqueNamedImage(prefab.transform, FlowFillObjectNames[i]);
                Assert.That(fill.raycastTarget, Is.False, $"{fill.name} must not block gameplay input.");
            }
        }

        [Test]
        public void PrimaryMobileControlsKeepVisibleAndHitRectsLargeAndInset()
        {
            GameObject instance = UnityEngine.Object.Instantiate(RequireCombatHudPrefab());
            try
            {
                Canvas.ForceUpdateCanvases();
                AssertMobileTouchSurface(instance.transform, "PauseButton", MobileEdgeAnchor.RightTop);
                AssertMobileTouchSurface(instance.transform, "UltimateButton", MobileEdgeAnchor.RightBottom);
                AssertMobileTouchSurface(instance.transform, "Skill1Button", MobileEdgeAnchor.RightBottom);
                AssertMobileTouchSurface(instance.transform, "DodgeButton", MobileEdgeAnchor.RightBottom);
                AssertMobileTouchSurface(instance.transform, "BasicAttackButton", MobileEdgeAnchor.RightBottom);
                AssertMobileTouchSurface(
                    instance.transform,
                    "SummonSlot1Button",
                    MobileEdgeAnchor.RightTop,
                    "FrameOverlay");
                AssertMobileTouchSurface(
                    instance.transform,
                    "SummonSlot2Button",
                    MobileEdgeAnchor.RightTop,
                    "FrameOverlay");
                AssertMobileTouchSurface(
                    instance.transform,
                    "SummonSlot3Button",
                    MobileEdgeAnchor.RightTop,
                    "FrameOverlay");
                AssertMobileTouchSurface(
                    instance.transform,
                    "MoveJoystickRing",
                    MobileEdgeAnchor.LeftBottom,
                    requireButton: false);
                AssertActionHitSpacing(instance.transform);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(instance);
            }
        }

        [Test]
        public void RuntimePresenterStillDrivesHpEnBossAndReticle()
        {
            GameObject instance = UnityEngine.Object.Instantiate(RequireCombatHudPrefab());
            try
            {
                Component presenter = RequireComponentByTypeName(instance, "DimensionBrawl.UI.CombatHudPresenter");
                Invoke(presenter, "SetHealth", 75f, 100f);
                Invoke(presenter, "SetResource", 40f, 100f);
                Invoke(presenter, "SetBossHudVisible", true);
                Invoke(presenter, "SetBossHealth", 55f, 100f);
                Invoke(presenter, "SetBossResource", 25f, 100f);
                Invoke(presenter, "SetAimReticleVisible", true, true);

                Assert.That(RequireUniqueNamedImage(instance.transform, "HealthBar").fillAmount, Is.EqualTo(0.75f).Within(0.001f));
                Assert.That(RequireUniqueNamedImage(instance.transform, "ResourceBar").fillAmount, Is.EqualTo(0.4f).Within(0.001f));
                Assert.That(RequireUniqueNamedImage(instance.transform, "BossHpFill").fillAmount, Is.EqualTo(0.55f).Within(0.001f));
                Assert.That(RequireUniqueNamedImage(instance.transform, "BossCostFill").fillAmount, Is.EqualTo(0.25f).Within(0.001f));

                Transform reticle = RequireUniqueNamedTransform(instance.transform, "CenterAimReticle");
                Assert.That(reticle.gameObject.activeSelf, Is.True);
                Image[] segments = reticle.GetComponentsInChildren<Image>(includeInactive: true);
                Assert.That(segments, Has.Length.GreaterThanOrEqualTo(1));
                Assert.That(segments.All(segment => !segment.raycastTarget), Is.True);
                Assert.That(
                    segments.Any(segment => segment.sprite != null),
                    Is.True,
                    "The assembled reticle should use the authored Celestial HUD sprite.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(instance);
            }
        }

        [Test]
        public void RuntimeLayoutCentersReticleAndKeepsMeterFillsInsideTheirTracks()
        {
            GameObject instance = UnityEngine.Object.Instantiate(RequireCombatHudPrefab());
            try
            {
                Component presenter = RequireComponentByTypeName(instance, "DimensionBrawl.UI.CombatHudPresenter");
                Invoke(presenter, "SetHealth", 50f, 100f);
                Invoke(presenter, "SetResource", 50f, 100f);
                Invoke(presenter, "SetBossHudVisible", true);
                Invoke(presenter, "SetBossHealth", 50f, 100f);
                Invoke(presenter, "SetBossResource", 50f, 100f);
                Invoke(presenter, "SetAimReticleVisible", true, true);
                Canvas.ForceUpdateCanvases();

                RectTransform reticle = RequireRectTransform(instance.transform, "CenterAimReticle");
                AssertVector2(reticle.anchorMin, new Vector2(0.5f, 0.5f), "reticle anchorMin");
                AssertVector2(reticle.anchorMax, new Vector2(0.5f, 0.5f), "reticle anchorMax");
                AssertVector2(reticle.pivot, new Vector2(0.5f, 0.5f), "reticle pivot");
                AssertVector2(
                    reticle.anchoredPosition,
                    Vector2.zero,
                    "reticle anchoredPosition must stay at the gameplay viewport center");
                AssertVector2(reticle.rect.size, new Vector2(95f, 95f), "reticle size");

                AssertMeterFillContract(
                    instance.transform,
                    "HealthBar_Track",
                    "HealthBar",
                    new Vector2(944f, 49f),
                    new Vector2(766f, 15f),
                    0.5f);
                AssertMeterFillContract(
                    instance.transform,
                    "ResourceBar_Track",
                    "ResourceBar",
                    new Vector2(846f, 40f),
                    new Vector2(766f, 12f),
                    0.5f);
                AssertMeterFillContract(
                    instance.transform,
                    "BossNameArea",
                    "BossHpFill",
                    new Vector2(1056f, 132f),
                    new Vector2(741f, 29f),
                    0.5f);
                AssertMeterFillContract(
                    instance.transform,
                    "BossNameArea",
                    "BossCostFill",
                    new Vector2(1056f, 132f),
                    new Vector2(821f, 13f),
                    0.5f);
                AssertBossFrameOverlayContract(instance.transform);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(instance);
            }
        }

        [Test]
        public void PgrMissionAndTimerReadoutsKeepIndependentReviewedHierarchy()
        {
            GameObject instance = UnityEngine.Object.Instantiate(RequireCombatHudPrefab());
            try
            {
                RectTransform hudRoot = instance.transform as RectTransform;
                Assert.That(hudRoot, Is.Not.Null);
                hudRoot.anchorMin = new Vector2(0.5f, 0.5f);
                hudRoot.anchorMax = new Vector2(0.5f, 0.5f);
                hudRoot.pivot = new Vector2(0.5f, 0.5f);
                hudRoot.sizeDelta = new Vector2(2560f, 1440f);

                Component presenter = RequireComponentByTypeName(instance, "DimensionBrawl.UI.CombatHudPresenter");
                RectTransform panel = RequireRectTransform(instance.transform, "TopLeftPanel");
                RectTransform objectiveRect = RequireRectTransform(instance.transform, "Objective");
                RectTransform timerBackingRect = RequireRectTransform(instance.transform, "MissionTimerBacking");
                RectTransform timerRect = RequireRectTransform(instance.transform, "Timer");
                Text objective = RequireText(objectiveRect);
                Text timer = RequireText(timerRect);
                Assert.That(objective.text, Is.EqualTo("Break the pressure line"));
                Assert.That(timer.text, Is.EqualTo("03:00"));

                Invoke(presenter, "SetObjective", "Break the pressure line");
                Invoke(presenter, "SetTimer", 138.9f);
                Canvas.ForceUpdateCanvases();

                AssertLeftTopRectContract(panel, new Vector2(760f, 160f));
                AssertLeftTopRectContract(objectiveRect, new Vector2(620f, 126f));
                AssertVector2(panel.anchoredPosition, new Vector2(24f, -316f), "objective panel position");
                AssertVector2(objectiveRect.anchoredPosition, new Vector2(88f, -329f), "objective text position");
                Assert.That(-panel.anchoredPosition.y, Is.GreaterThanOrEqualTo(300f));
                AssertRectContains(panel, objectiveRect, "Objective");
                AssertRelativeTopLeftRect(
                    panel,
                    objectiveRect,
                    new Vector2(64f, 13f),
                    new Vector2(620f, 126f));

                AssertRightTopRectContract(timerBackingRect, new Vector2(184f, 86f));
                AssertRightTopRectContract(timerRect, new Vector2(160f, 86f));
                AssertVector2(timerBackingRect.anchoredPosition, new Vector2(-362f, -47f), "timer backing position");
                AssertVector2(timerRect.anchoredPosition, new Vector2(-374f, -47f), "timer text position");
                AssertRectContains(timerBackingRect, timerRect, "Timer");
                Assert.That(timerRect.IsChildOf(panel), Is.False, "Timer must remain independent from the objective ribbon.");
                AssertHorizontalSeparation(
                    RequireRectTransform(instance.transform, "BossNameArea"),
                    timerBackingRect,
                    hudRoot,
                    162f,
                    "boss frame/timer backing");
                AssertHorizontalSeparation(
                    timerBackingRect,
                    RequireFirstNamedRectTransform(instance.transform, "SettingsButton"),
                    hudRoot,
                    52f,
                    "timer backing/settings");
                Assert.That(RequireImage(panel).raycastTarget, Is.False);

                Image timerBacking = RequireImage(timerBackingRect);
                Assert.That(timerBacking.sprite, Is.Null);
                Assert.That(timerBacking.type, Is.EqualTo(Image.Type.Simple));
                Assert.That(timerBacking.raycastTarget, Is.False);
                Assert.That(timerBacking.color.a, Is.LessThanOrEqualTo(0.25f));
                AssertColor(timerBacking.color, new Color(0.02f, 0.025f, 0.035f, 0.22f), "timer backing color");
                var serializedBacking = new SerializedObject(timerBacking);
                Assert.That(serializedBacking.FindProperty("m_Material").objectReferenceValue, Is.Null);

                Assert.That(objective.text, Is.EqualTo("Break the pressure line"));
                Assert.That(timer.text, Is.EqualTo("02:18"));
                AssertTopLeftTextContract(
                    objective,
                    42,
                    TextAnchor.MiddleLeft,
                    new Color(0.94f, 0.97f, 1f, 1f),
                    HorizontalWrapMode.Wrap,
                    VerticalWrapMode.Truncate);
                AssertTopLeftTextContract(
                    timer,
                    46,
                    TextAnchor.MiddleCenter,
                    new Color(0.97f, 0.985f, 1f, 1f));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(instance);
            }
        }

        [Test]
        public void PgrMissionAndTimerReadoutsHonorAsymmetricSafeAreaIndependently()
        {
            GameObject instance = UnityEngine.Object.Instantiate(RequireCombatHudPrefab());
            try
            {
                RectTransform canvasRoot = instance.transform as RectTransform;
                Assert.That(canvasRoot, Is.Not.Null);
                canvasRoot.anchorMin = new Vector2(0.5f, 0.5f);
                canvasRoot.anchorMax = new Vector2(0.5f, 0.5f);
                canvasRoot.pivot = new Vector2(0.5f, 0.5f);
                canvasRoot.sizeDelta = new Vector2(2560f, 1440f);

                var screenSize = new Vector2(2400f, 1080f);
                var asymmetricSafeArea = new Rect(90f, 0f, 2160f, 1080f);
                ScreenSafeAreaInsets insets = ScreenSafeAreaUtility.ResolveCanvasInsets(
                    asymmetricSafeArea,
                    screenSize,
                    canvasRoot.rect.size);
                Assert.That(insets.Left, Is.EqualTo(96f).Within(LayoutTolerance));
                Assert.That(insets.Right, Is.EqualTo(160f).Within(LayoutTolerance));

                Component presenter = RequireComponentByTypeName(instance, "DimensionBrawl.UI.CombatHudPresenter");
                SetPrivateField(presenter, "safeAreaInsets", insets);
                InvokeResponsiveDesignRect(
                    presenter,
                    "TopLeftPanel",
                    new Rect(24f, 316f, 760f, 160f),
                    "LeftTop");
                InvokeResponsiveDesignRect(
                    presenter,
                    "Objective",
                    new Rect(88f, 329f, 620f, 126f),
                    "LeftTop");
                InvokeResponsiveDesignRect(
                    presenter,
                    "SettingsButton",
                    new Rect(2250f, 47f, 100f, 95f),
                    "RightTop");
                Invoke(presenter, "ApplyResponsiveMissionTimerLayout", canvasRoot.rect.size);
                Canvas.ForceUpdateCanvases();

                RectTransform panel = RequireRectTransform(instance.transform, "TopLeftPanel");
                RectTransform objectiveRect = RequireRectTransform(instance.transform, "Objective");
                RectTransform timerBackingRect = RequireRectTransform(instance.transform, "MissionTimerBacking");
                RectTransform timerRect = RequireRectTransform(instance.transform, "Timer");
                RectTransform bossFrame = RequireRectTransform(instance.transform, "BossNameArea");
                RectTransform settings = RequireFirstNamedRectTransform(instance.transform, "SettingsButton");
                AssertRectContains(canvasRoot, panel, "safe-area mission panel");
                AssertRectContains(panel, objectiveRect, "safe-area Objective");
                AssertRectContains(canvasRoot, timerBackingRect, "safe-area timer backing");
                AssertRectContains(timerBackingRect, timerRect, "safe-area Timer");
                Assert.That(timerRect.IsChildOf(panel), Is.False);
                AssertHorizontalSeparation(
                    bossFrame,
                    timerBackingRect,
                    canvasRoot,
                    24f,
                    "safe-area boss frame/timer backing");
                AssertHorizontalSeparation(
                    timerBackingRect,
                    settings,
                    canvasRoot,
                    52f,
                    "safe-area timer backing/settings");

                Rect canvasBounds = GetRectInRootSpace(canvasRoot, canvasRoot);
                Rect panelBounds = GetRectInRootSpace(panel, canvasRoot);
                Rect timerBackingBounds = GetRectInRootSpace(timerBackingRect, canvasRoot);
                Assert.That(
                    panelBounds.xMin - canvasBounds.xMin,
                    Is.GreaterThanOrEqualTo(insets.Left),
                    "The objective ribbon must stay beyond the representative left cutout.");
                Assert.That(
                    canvasBounds.xMax - timerBackingBounds.xMax,
                    Is.GreaterThanOrEqualTo(insets.Right),
                    "The timer backing must remain inside the representative right safe edge.");
                Assert.That(-panel.anchoredPosition.y, Is.GreaterThanOrEqualTo(300f));

                Text objective = RequireText(objectiveRect);
                Text timer = RequireText(timerRect);
                Assert.That(objective.fontSize, Is.EqualTo(42));
                Assert.That(timer.fontSize, Is.GreaterThanOrEqualTo(40));
                Assert.That(objective.resizeTextForBestFit, Is.False);
                Assert.That(timer.resizeTextForBestFit, Is.False);
                Assert.That(objective.raycastTarget, Is.False);
                Assert.That(timer.raycastTarget, Is.False);
                Assert.That(RequireImage(panel).raycastTarget, Is.False);
                Assert.That(RequireImage(timerBackingRect).raycastTarget, Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(instance);
            }
        }

        [Test]
        public void AuthoredRuntimeReadoutsDoNotShipDeveloperPlaceholderCopy()
        {
            GameObject prefab = RequireCombatHudPrefab();

            Text actionFeedback = RequireText(
                RequireRectTransform(prefab.transform, "ActionFeedback"));
            Assert.That(actionFeedback.text, Is.Empty);
            Assert.That(actionFeedback.font, Is.Not.Null);
            Assert.That(actionFeedback.font.name, Does.Contain("Pretendard-SemiBold"));
            Assert.That(actionFeedback.fontSize, Is.EqualTo(24));
            Assert.That(actionFeedback.color, Is.EqualTo(new Color(0.90f, 0.97f, 1f, 1f)));

            Text inputMode = RequireText(
                RequireRectTransform(prefab.transform, "InputMode"));
            Assert.That(inputMode.text, Is.Empty);

            for (int slot = 1; slot <= 3; slot++)
            {
                Transform button = RequireUniqueNamedTransform(
                    prefab.transform,
                    $"SummonSlot{slot}Button");
                Text state = RequireText(
                    RequireUniqueNamedTransform(button, "State") as RectTransform);
                Assert.That(state.text, Is.Empty, $"Summon slot {slot} still contains placeholder copy.");
            }
        }

        [Test]
        public void InputModeKeepsDecisionInformationButSuppressesRedundantWeaponMode()
        {
            GameObject prefab = RequireCombatHudPrefab();
            Transform authoredInputMode = RequireUniqueNamedTransform(prefab.transform, "InputMode");
            Assert.That(
                authoredInputMode.gameObject.activeSelf,
                Is.True,
                "InputMode must not be disabled in the assembled prefab; the boss binder owns this decision readout.");

            GameObject instance = UnityEngine.Object.Instantiate(prefab);
            try
            {
                Component presenter = RequireComponentByTypeName(instance, "DimensionBrawl.UI.CombatHudPresenter");
                Transform inputModeTransform = RequireUniqueNamedTransform(instance.transform, "InputMode");
                RectTransform inputModeRect = inputModeTransform as RectTransform;
                Text inputModeText = inputModeTransform.GetComponent<Text>();
                Assert.That(inputModeRect, Is.Not.Null);
                Assert.That(inputModeText, Is.Not.Null);
                Assert.That(inputModeRect.sizeDelta.x, Is.EqualTo(500f).Within(0.01f));
                Assert.That(inputModeRect.sizeDelta.y, Is.EqualTo(32f).Within(0.01f));
                Assert.That(inputModeText.raycastTarget, Is.False);

                Invoke(presenter, "SetInputMode", "FRONT READY LV2 x1.6");
                Assert.That(inputModeText.text, Is.EqualTo("FRONT READY LV2 x1.6"));
                Assert.That(inputModeTransform.gameObject.activeSelf, Is.True);

                Invoke(presenter, "SetInputMode", "RANGED");
                Assert.That(
                    inputModeText.text,
                    Is.Empty,
                    "Weapon mode is already communicated by the action feedback and must not replace decision information.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(instance);
            }
        }

        [TestCase(CorridorScenePath)]
        [TestCase(StationScenePath)]
        [TestCase(CourtyardScenePath)]
        public void CanonicalScenesPreserveInputBindingsAndInheritPrefabVisuals(string scenePath)
        {
            GameObject prefab = RequireCombatHudPrefab();
            string yaml = ReadAssetText(scenePath);
            string prefabSource =
                $"m_SourcePrefab: {{fileID: 100100000, guid: {CombatHudPrefabGuid}, type: 3}}";
            Assert.That(
                CountOccurrences(yaml, prefabSource),
                Is.EqualTo(1),
                $"{scenePath} must retain exactly one canonical HUD instance.");

            Dictionary<long, long> strippedGameObjects = ParseCanonicalStrippedGameObjects(yaml);
            Dictionary<long, int> actualBindings = ParseCanonicalPointerBindings(yaml, strippedGameObjects);
            Dictionary<long, int> expectedBindings = BuildExpectedSceneBindings(prefab, scenePath);
            Assert.That(actualBindings, Is.EquivalentTo(expectedBindings),
                $"{scenePath} changed an action-to-button route.");

            AssertCanonicalCanvasScaler(prefab, scenePath, yaml, strippedGameObjects);
            AssertCanonicalJoystickBinding(prefab, scenePath, yaml, strippedGameObjects);
            AssertNoCanonicalVisualOverrides(prefab, scenePath, yaml);
        }

        private static GameObject RequireCombatHudPrefab()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CombatHudPrefabPath);
            Assert.That(prefab, Is.Not.Null, $"Missing canonical combat HUD prefab: {CombatHudPrefabPath}");
            return prefab;
        }

        private static Component RequireComponentByTypeName(GameObject root, string fullTypeName)
        {
            Type type = Type.GetType($"{fullTypeName}, Assembly-CSharp", throwOnError: false);
            Assert.That(type, Is.Not.Null, $"Missing product type {fullTypeName}.");
            Component component = root.GetComponentInChildren(type, includeInactive: true);
            Assert.That(component, Is.Not.Null, $"Missing {fullTypeName} under {root.name}.");
            return component;
        }

        private static void AssertCelestialSprite(Image image)
        {
            Assert.That(image.sprite, Is.Not.Null, $"{GetHierarchyPath(image.transform)} has no sprite.");
            string path = AssetDatabase.GetAssetPath(image.sprite).Replace('\\', '/');
            Assert.That(
                path,
                Does.StartWith(CelestialHudArtRoot),
                $"{GetHierarchyPath(image.transform)} still references pre-Celestial HUD art: {path}");
        }

        private static void AssertClearedImage(Transform root, string objectName)
        {
            Image image = RequireUniqueNamedImage(root, objectName);
            Assert.That(image.sprite, Is.Null, $"{objectName} would double-render the authored combined rail.");
            Assert.That(image.color.a, Is.EqualTo(0f).Within(0.001f));
            Assert.That(image.raycastTarget, Is.False);
        }

        private static void AssertObjectReferenceName(
            SerializedObject serializedObject,
            string propertyName,
            string expectedObjectName)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            Assert.That(property, Is.Not.Null, $"Missing serialized field {propertyName}.");
            Component component = property.objectReferenceValue as Component;
            Assert.That(component, Is.Not.Null, $"{propertyName} is not assigned.");
            Assert.That(component.gameObject.name, Is.EqualTo(expectedObjectName));
        }

        private static void AssertBindingArray(
            SerializedProperty bindings,
            IReadOnlyCollection<int> expectedActionIds,
            IReadOnlyList<string> requiredReferences)
        {
            Assert.That(bindings, Is.Not.Null);
            Assert.That(bindings.isArray, Is.True);
            Assert.That(bindings.arraySize, Is.EqualTo(expectedActionIds.Count));
            var actualActionIds = new HashSet<int>();
            for (int i = 0; i < bindings.arraySize; i++)
            {
                SerializedProperty binding = bindings.GetArrayElementAtIndex(i);
                int actionId = binding.FindPropertyRelative("actionId").intValue;
                Assert.That(actualActionIds.Add(actionId), Is.True, $"Duplicate action binding {actionId}.");
                for (int referenceIndex = 0; referenceIndex < requiredReferences.Count; referenceIndex++)
                {
                    string referenceName = requiredReferences[referenceIndex];
                    Assert.That(
                        binding.FindPropertyRelative(referenceName)?.objectReferenceValue,
                        Is.Not.Null,
                        $"Action {actionId} is missing {referenceName}.");
                }
            }

            Assert.That(actualActionIds, Is.EquivalentTo(expectedActionIds));
        }

        private static SerializedProperty FindBindingByActionId(SerializedProperty bindings, int actionId)
        {
            for (int i = 0; i < bindings.arraySize; i++)
            {
                SerializedProperty binding = bindings.GetArrayElementAtIndex(i);
                if (binding.FindPropertyRelative("actionId").intValue == actionId)
                {
                    return binding;
                }
            }

            Assert.Fail($"Missing action catalog entry {actionId}.");
            return null;
        }

        private static void AssertBindingOwners(SerializedProperty bindings, Transform prefabRoot)
        {
            var expectedButtonByAction = ActionButtonBindings.ToDictionary(
                pair => pair.ActionId,
                pair => pair.ButtonName);
            for (int i = 0; i < bindings.arraySize; i++)
            {
                SerializedProperty binding = bindings.GetArrayElementAtIndex(i);
                int actionId = binding.FindPropertyRelative("actionId").intValue;
                Assert.That(expectedButtonByAction.ContainsKey(actionId), Is.True);
                Transform expectedButton = RequireUniqueNamedTransform(
                    prefabRoot,
                    expectedButtonByAction[actionId]);
                Component label = binding.FindPropertyRelative("labelText").objectReferenceValue as Component;
                Assert.That(label, Is.Not.Null, $"Action {actionId} has no label owner.");
                Assert.That(
                    label.transform.IsChildOf(expectedButton),
                    Is.True,
                    $"Action {actionId} is wired to {label.name}, outside {expectedButton.name}.");
            }
        }

        private static Transform RequireUniqueNamedTransform(Transform root, string objectName)
        {
            Transform match = null;
            int count = 0;
            Transform[] transforms = root.GetComponentsInChildren<Transform>(includeInactive: true);
            for (int i = 0; i < transforms.Length; i++)
            {
                Transform candidate = transforms[i];
                if (candidate == null || !string.Equals(candidate.name, objectName, StringComparison.Ordinal))
                {
                    continue;
                }

                match = candidate;
                count++;
            }

            Assert.That(count, Is.EqualTo(1), $"Expected exactly one {objectName} under {root.name}.");
            return match;
        }

        private static Transform RequireFirstNamedTransform(Transform root, string objectName)
        {
            Assert.That(root, Is.Not.Null);
            if (string.Equals(root.name, objectName, StringComparison.Ordinal))
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindFirstNamedTransform(root.GetChild(i), objectName);
                if (found != null)
                {
                    return found;
                }
            }

            Assert.Fail($"Missing {objectName} under {root.name}.");
            return null;
        }

        private static Transform FindFirstNamedTransform(Transform root, string objectName)
        {
            if (string.Equals(root.name, objectName, StringComparison.Ordinal))
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindFirstNamedTransform(root.GetChild(i), objectName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static RectTransform RequireFirstNamedRectTransform(Transform root, string objectName)
        {
            RectTransform rectTransform = RequireFirstNamedTransform(root, objectName) as RectTransform;
            Assert.That(rectTransform, Is.Not.Null, $"{objectName} is missing RectTransform.");
            return rectTransform;
        }

        private static Image RequireUniqueNamedImage(Transform root, string objectName)
        {
            return RequireImage(RequireUniqueNamedTransform(root, objectName));
        }

        private static Image RequireImage(Transform transform)
        {
            Image image = transform.GetComponent<Image>();
            Assert.That(image, Is.Not.Null, $"{GetHierarchyPath(transform)} is missing Image.");
            return image;
        }

        private static Text RequireText(Transform transform)
        {
            Text text = transform.GetComponent<Text>();
            Assert.That(text, Is.Not.Null, $"{GetHierarchyPath(transform)} is missing Text.");
            return text;
        }

        private static RectTransform RequireRectTransform(Transform root, string objectName)
        {
            RectTransform rectTransform = RequireUniqueNamedTransform(root, objectName) as RectTransform;
            Assert.That(rectTransform, Is.Not.Null, $"{objectName} is missing RectTransform.");
            return rectTransform;
        }

        private static Transform RequireClipAncestor(Transform portrait, Transform slot)
        {
            Transform current = portrait.parent;
            while (current != null)
            {
                if (current != slot
                    && (current.GetComponent<Mask>() != null
                        || current.GetComponent<RectMask2D>() != null))
                {
                    return current;
                }

                if (current == slot)
                {
                    break;
                }

                current = current.parent;
            }

            Assert.Fail($"{GetHierarchyPath(portrait)} is not clipped by a portrait aperture under {slot.name}.");
            return null;
        }

        private static Transform RequireDirectChildBranch(Transform parent, Transform descendant)
        {
            Transform branch = descendant;
            while (branch != null && branch.parent != parent)
            {
                branch = branch.parent;
            }

            Assert.That(
                branch,
                Is.Not.Null,
                $"{GetHierarchyPath(descendant)} is not a descendant of {GetHierarchyPath(parent)}.");
            return branch;
        }

        private static void AssertMeterFillContract(
            Transform root,
            string trackName,
            string fillName,
            Vector2 expectedTrackSize,
            Vector2 expectedFillSize,
            float expectedFillAmount)
        {
            RectTransform track = RequireRectTransform(root, trackName);
            RectTransform fillRect = RequireRectTransform(root, fillName);
            Image fill = RequireImage(fillRect);

            AssertVector2(track.rect.size, expectedTrackSize, $"{trackName} size");
            AssertVector2(fillRect.rect.size, expectedFillSize, $"{fillName} authored size");
            Assert.That(fill.type, Is.EqualTo(Image.Type.Filled), $"{fillName} must clip via Image.fillAmount.");
            Assert.That(fill.fillMethod, Is.EqualTo(Image.FillMethod.Horizontal));
            Assert.That(fill.fillOrigin, Is.EqualTo((int)Image.OriginHorizontal.Left));
            Assert.That(fill.fillAmount, Is.EqualTo(expectedFillAmount).Within(0.001f));
            Assert.That(
                fillRect.rect.width * fill.fillAmount,
                Is.EqualTo(expectedFillSize.x * expectedFillAmount).Within(LayoutTolerance),
                $"{fillName} must apply its ratio once, through Image.fillAmount only.");
            AssertRectContains(track, fillRect, fillName);
        }

        private static void AssertMobileTouchSurface(
            Transform root,
            string objectName,
            MobileEdgeAnchor edgeAnchor,
            string visibleChildName = null,
            bool requireButton = true)
        {
            RectTransform hitRect = RequireRectTransform(root, objectName);
            Image hitGraphic = RequireImage(hitRect);
            Assert.That(hitGraphic.raycastTarget, Is.True, $"{objectName} lost its pointer hit graphic.");
            if (requireButton)
            {
                Button button = hitRect.GetComponent<Button>();
                Assert.That(button, Is.Not.Null, $"{objectName} lost Button.");
                Assert.That(button.interactable, Is.True);
                Assert.That(
                    button.targetGraphic,
                    Is.SameAs(hitGraphic),
                    $"{objectName} Button must use the full root rect as its hit target.");
            }

            Image visibleGraphic = string.IsNullOrEmpty(visibleChildName)
                ? hitGraphic
                : RequireUniqueNamedImage(hitRect, visibleChildName);
            Assert.That(visibleGraphic.sprite, Is.Not.Null, $"{objectName} has no visible sprite.");
            Assert.That(visibleGraphic.color.a, Is.GreaterThan(0.5f), $"{objectName} visible graphic is transparent.");
            Assert.That(visibleGraphic.raycastTarget, Is.EqualTo(visibleGraphic == hitGraphic));

            Vector2 hitSize = hitRect.rect.size;
            Assert.That(hitSize.x, Is.GreaterThanOrEqualTo(MinimumMobileTouchSize), $"{objectName} hit width");
            Assert.That(hitSize.y, Is.GreaterThanOrEqualTo(MinimumMobileTouchSize), $"{objectName} hit height");
            Vector2 visibleSize = ResolveVisibleGraphicSize(visibleGraphic);
            Assert.That(visibleSize.x, Is.GreaterThanOrEqualTo(MinimumMobileTouchSize), $"{objectName} visible width");
            Assert.That(visibleSize.y, Is.GreaterThanOrEqualTo(MinimumMobileTouchSize), $"{objectName} visible height");
            Assert.That(visibleSize.x, Is.LessThanOrEqualTo(hitSize.x + LayoutTolerance));
            Assert.That(visibleSize.y, Is.LessThanOrEqualTo(hitSize.y + LayoutTolerance));

            switch (edgeAnchor)
            {
                case MobileEdgeAnchor.LeftBottom:
                    AssertVector2(hitRect.anchorMin, Vector2.zero, $"{objectName} anchorMin");
                    AssertVector2(hitRect.anchorMax, Vector2.zero, $"{objectName} anchorMax");
                    AssertVector2(hitRect.pivot, Vector2.zero, $"{objectName} pivot");
                    Assert.That(
                        hitRect.anchoredPosition.x,
                        Is.GreaterThanOrEqualTo(MinimumMobileEdgeMargin),
                        $"{objectName} left margin");
                    Assert.That(
                        hitRect.anchoredPosition.y,
                        Is.GreaterThanOrEqualTo(MinimumMobileEdgeMargin),
                        $"{objectName} bottom margin");
                    break;
                case MobileEdgeAnchor.RightTop:
                    AssertVector2(hitRect.anchorMin, Vector2.one, $"{objectName} anchorMin");
                    AssertVector2(hitRect.anchorMax, Vector2.one, $"{objectName} anchorMax");
                    AssertVector2(hitRect.pivot, Vector2.one, $"{objectName} pivot");
                    Assert.That(
                        -hitRect.anchoredPosition.x,
                        Is.GreaterThanOrEqualTo(MinimumMobileEdgeMargin),
                        $"{objectName} right margin");
                    Assert.That(
                        -hitRect.anchoredPosition.y,
                        Is.GreaterThanOrEqualTo(MinimumMobileEdgeMargin),
                        $"{objectName} top margin");
                    break;
                case MobileEdgeAnchor.RightBottom:
                    AssertVector2(hitRect.anchorMin, new Vector2(1f, 0f), $"{objectName} anchorMin");
                    AssertVector2(hitRect.anchorMax, new Vector2(1f, 0f), $"{objectName} anchorMax");
                    AssertVector2(hitRect.pivot, new Vector2(1f, 0f), $"{objectName} pivot");
                    Assert.That(
                        -hitRect.anchoredPosition.x,
                        Is.GreaterThanOrEqualTo(MinimumMobileEdgeMargin),
                        $"{objectName} right margin");
                    Assert.That(
                        hitRect.anchoredPosition.y,
                        Is.GreaterThanOrEqualTo(MinimumMobileEdgeMargin),
                        $"{objectName} bottom margin");
                    break;
            }
        }

        private static Vector2 ResolveVisibleGraphicSize(Image image)
        {
            Vector2 rectSize = image.rectTransform.rect.size;
            if (!image.preserveAspect || image.sprite == null || image.sprite.rect.height <= 0f)
            {
                return rectSize;
            }

            float spriteAspect = image.sprite.rect.width / image.sprite.rect.height;
            float rectAspect = rectSize.x / Mathf.Max(0.001f, rectSize.y);
            return rectAspect > spriteAspect
                ? new Vector2(rectSize.y * spriteAspect, rectSize.y)
                : new Vector2(rectSize.x, rectSize.x / spriteAspect);
        }

        private static void AssertActionHitSpacing(Transform root)
        {
            string[] actionNames =
            {
                "UltimateButton",
                "Skill1Button",
                "DodgeButton",
                "BasicAttackButton"
            };
            RectTransform commonRoot = root as RectTransform;
            Assert.That(commonRoot, Is.Not.Null);
            var rects = actionNames.ToDictionary(
                objectName => objectName,
                objectName => GetRectInRootSpace(
                    RequireRectTransform(root, objectName),
                    commonRoot));

            float minimumGap = float.PositiveInfinity;
            string closestPair = string.Empty;
            for (int leftIndex = 0; leftIndex < actionNames.Length; leftIndex++)
            {
                for (int rightIndex = leftIndex + 1; rightIndex < actionNames.Length; rightIndex++)
                {
                    string leftName = actionNames[leftIndex];
                    string rightName = actionNames[rightIndex];
                    Rect left = rects[leftName];
                    Rect right = rects[rightName];
                    Assert.That(
                        left.Overlaps(right),
                        Is.False,
                        $"{leftName} and {rightName} pointer hit rects overlap.");
                    float gap = CalculateRectGap(left, right);
                    if (gap < minimumGap)
                    {
                        minimumGap = gap;
                        closestPair = $"{leftName}/{rightName}";
                    }
                }
            }

            Assert.That(minimumGap, Is.GreaterThanOrEqualTo(0f));
            TestContext.Progress.WriteLine(
                $"ERGONOMIC WARNING: closest action hit pair {closestPair} has "
                + $"{minimumGap:0.##} design px separation (current reviewed layout: 6 px).");
        }

        private static float CalculateRectGap(Rect left, Rect right)
        {
            float horizontalGap = Mathf.Max(0f, Mathf.Max(right.xMin - left.xMax, left.xMin - right.xMax));
            float verticalGap = Mathf.Max(0f, Mathf.Max(right.yMin - left.yMax, left.yMin - right.yMax));
            return Mathf.Sqrt(horizontalGap * horizontalGap + verticalGap * verticalGap);
        }

        private static void AssertBossFrameOverlayContract(Transform root)
        {
            RectTransform frame = RequireRectTransform(root, "BossNameArea");
            RectTransform healthFill = RequireRectTransform(root, "BossHpFill");
            RectTransform costFill = RequireRectTransform(root, "BossCostFill");
            RectTransform healthBackground = RequireRectTransform(root, "BossHpBackground");
            RectTransform costBackground = RequireRectTransform(root, "BossCostBackground");
            RectTransform actionFeedback = RequireRectTransform(root, "ActionFeedback");
            AssertRectContains(frame, healthFill, "BossHpFill");
            AssertRectContains(frame, costFill, "BossCostFill");
            AssertRectContains(frame, healthBackground, "BossHpBackground");
            AssertRectContains(frame, costBackground, "BossCostBackground");
            AssertRelativeTopLeftRect(frame, healthFill, new Vector2(46f, 51f), new Vector2(741f, 29f));
            AssertRelativeTopLeftRect(frame, costFill, new Vector2(46f, 86f), new Vector2(821f, 13f));
            AssertRelativeTopLeftRect(frame, healthBackground, new Vector2(43f, 52f), new Vector2(913f, 18f));
            AssertRelativeTopLeftRect(frame, costBackground, new Vector2(43f, 95f), new Vector2(913f, 14f));

            Assert.That(healthFill.parent, Is.SameAs(frame.parent));
            Assert.That(costFill.parent, Is.SameAs(frame.parent));
            Assert.That(
                frame.GetSiblingIndex(),
                Is.GreaterThan(healthFill.GetSiblingIndex()),
                "The combined boss frame must render above the HP fill.");
            Assert.That(
                frame.GetSiblingIndex(),
                Is.GreaterThan(costFill.GetSiblingIndex()),
                "The combined boss frame must render above the cost fill.");

            Transform frameBranch = RequireDirectChildBranch(root, frame);
            Transform feedbackBranch = RequireDirectChildBranch(root, actionFeedback);
            Assert.That(
                feedbackBranch.GetSiblingIndex(),
                Is.GreaterThan(frameBranch.GetSiblingIndex()),
                "Boss decision text must remain readable above the combined frame.");

            Image frameImage = RequireImage(frame);
            AssertCelestialSprite(frameImage);
            Assert.That(frameImage.raycastTarget, Is.False);
        }

        private static void AssertRelativeTopLeftRect(
            RectTransform container,
            RectTransform content,
            Vector2 expectedTopLeftOffset,
            Vector2 expectedSize)
        {
            RectTransform commonRoot = container.root as RectTransform;
            Assert.That(commonRoot, Is.Not.Null);
            Rect containerBounds = GetRectInRootSpace(container, commonRoot);
            Rect contentBounds = GetRectInRootSpace(content, commonRoot);
            Assert.That(
                contentBounds.xMin - containerBounds.xMin,
                Is.EqualTo(expectedTopLeftOffset.x).Within(LayoutTolerance),
                $"{content.name} left inset within {container.name}");
            Assert.That(
                containerBounds.yMax - contentBounds.yMax,
                Is.EqualTo(expectedTopLeftOffset.y).Within(LayoutTolerance),
                $"{content.name} top inset within {container.name}");
            AssertVector2(contentBounds.size, expectedSize, $"{content.name} bounds size");
        }

        private static void AssertLeftTopRectContract(RectTransform rect, Vector2 expectedSize)
        {
            AssertVector2(rect.anchorMin, new Vector2(0f, 1f), $"{rect.name} anchorMin");
            AssertVector2(rect.anchorMax, new Vector2(0f, 1f), $"{rect.name} anchorMax");
            AssertVector2(rect.pivot, new Vector2(0f, 1f), $"{rect.name} pivot");
            AssertVector2(rect.rect.size, expectedSize, $"{rect.name} size");
        }

        private static void AssertRightTopRectContract(RectTransform rect, Vector2 expectedSize)
        {
            AssertVector2(rect.anchorMin, Vector2.one, $"{rect.name} anchorMin");
            AssertVector2(rect.anchorMax, Vector2.one, $"{rect.name} anchorMax");
            AssertVector2(rect.pivot, Vector2.one, $"{rect.name} pivot");
            AssertVector2(rect.rect.size, expectedSize, $"{rect.name} size");
        }

        private static void AssertTopLeftTextContract(
            Text text,
            int expectedFontSize,
            TextAnchor expectedAlignment,
            Color expectedColor,
            HorizontalWrapMode expectedHorizontalOverflow = HorizontalWrapMode.Overflow,
            VerticalWrapMode expectedVerticalOverflow = VerticalWrapMode.Overflow)
        {
            Assert.That(text.font, Is.Not.Null, $"{text.name} has no font.");
            string fontPath = AssetDatabase.GetAssetPath(text.font).Replace('\\', '/');
            Assert.That(fontPath, Is.EqualTo(HudFontPath), $"{text.name} lost the reviewed HUD font.");
            Assert.That(text.fontSize, Is.EqualTo(expectedFontSize));
            Assert.That(text.fontStyle, Is.EqualTo(FontStyle.Normal));
            Assert.That(text.resizeTextForBestFit, Is.False, $"{text.name} must not silently shrink on phones.");
            Assert.That(text.alignment, Is.EqualTo(expectedAlignment));
            Assert.That(text.alignByGeometry, Is.True);
            Assert.That(text.horizontalOverflow, Is.EqualTo(expectedHorizontalOverflow));
            Assert.That(text.verticalOverflow, Is.EqualTo(expectedVerticalOverflow));
            Assert.That(text.raycastTarget, Is.False);
            AssertColor(text.color, expectedColor, $"{text.name} color");
        }

        private static void AssertRectContains(
            RectTransform container,
            RectTransform content,
            string contentLabel)
        {
            RectTransform commonRoot = container.root as RectTransform;
            Assert.That(commonRoot, Is.Not.Null);
            Rect containerBounds = GetRectInRootSpace(container, commonRoot);
            Rect contentBounds = GetRectInRootSpace(content, commonRoot);
            Assert.That(
                contentBounds.xMin,
                Is.GreaterThanOrEqualTo(containerBounds.xMin - LayoutTolerance),
                $"{contentLabel} leaks past {container.name}'s left edge.");
            Assert.That(
                contentBounds.xMax,
                Is.LessThanOrEqualTo(containerBounds.xMax + LayoutTolerance),
                $"{contentLabel} leaks past {container.name}'s right edge.");
            Assert.That(
                contentBounds.yMin,
                Is.GreaterThanOrEqualTo(containerBounds.yMin - LayoutTolerance),
                $"{contentLabel} leaks past {container.name}'s bottom edge.");
            Assert.That(
                contentBounds.yMax,
                Is.LessThanOrEqualTo(containerBounds.yMax + LayoutTolerance),
                $"{contentLabel} leaks past {container.name}'s top edge.");
        }

        private static void AssertHorizontalSeparation(
            RectTransform left,
            RectTransform right,
            RectTransform commonHudRoot,
            float expectedGap,
            string label)
        {
            Assert.That(commonHudRoot, Is.Not.Null);
            Assert.That(left.IsChildOf(commonHudRoot), Is.True, $"{left.name} is outside the common HUD root.");
            Assert.That(right.IsChildOf(commonHudRoot), Is.True, $"{right.name} is outside the common HUD root.");
            Rect leftBounds = GetRectInRootSpace(left, commonHudRoot);
            Rect rightBounds = GetRectInRootSpace(right, commonHudRoot);
            Assert.That(leftBounds.Overlaps(rightBounds), Is.False, $"{label} overlap.");
            Assert.That(
                rightBounds.xMin - leftBounds.xMax,
                Is.EqualTo(expectedGap).Within(LayoutTolerance),
                $"{label} horizontal gap");
        }

        private static Rect GetRectInRootSpace(RectTransform rect, RectTransform root)
        {
            var corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            Vector3 bottomLeft = root.InverseTransformPoint(corners[0]);
            Vector3 topRight = root.InverseTransformPoint(corners[2]);
            return Rect.MinMaxRect(bottomLeft.x, bottomLeft.y, topRight.x, topRight.y);
        }

        private static void AssertVector2(Vector2 actual, Vector2 expected, string label)
        {
            Assert.That(actual.x, Is.EqualTo(expected.x).Within(LayoutTolerance), $"{label}.x");
            Assert.That(actual.y, Is.EqualTo(expected.y).Within(LayoutTolerance), $"{label}.y");
        }

        private static void AssertColor(Color actual, Color expected, string label)
        {
            Assert.That(actual.r, Is.EqualTo(expected.r).Within(0.005f), $"{label}.r");
            Assert.That(actual.g, Is.EqualTo(expected.g).Within(0.005f), $"{label}.g");
            Assert.That(actual.b, Is.EqualTo(expected.b).Within(0.005f), $"{label}.b");
            Assert.That(actual.a, Is.EqualTo(expected.a).Within(0.005f), $"{label}.a");
        }

        private static string GetHierarchyPath(Transform transform)
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

        private static void Invoke(Component component, string methodName, params object[] arguments)
        {
            MethodInfo method = component.GetType().GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, $"Missing {component.GetType().Name}.{methodName}.");
            method.Invoke(component, arguments);
        }

        private static void InvokeResponsiveDesignRect(
            Component component,
            string objectName,
            Rect designRect,
            string anchorName)
        {
            Type anchorType = component.GetType().GetNestedType(
                "ResponsiveHudAnchor",
                BindingFlags.NonPublic);
            Assert.That(anchorType, Is.Not.Null);
            object anchor = Enum.Parse(anchorType, anchorName);
            MethodInfo method = component.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                .SingleOrDefault(candidate =>
                {
                    if (!string.Equals(candidate.Name, "ApplyResponsiveDesignRect", StringComparison.Ordinal))
                    {
                        return false;
                    }

                    ParameterInfo[] parameters = candidate.GetParameters();
                    return parameters.Length == 3
                        && parameters[0].ParameterType == typeof(string)
                        && parameters[1].ParameterType == typeof(Rect)
                        && parameters[2].ParameterType == anchorType;
                });
            Assert.That(method, Is.Not.Null, "Missing string-based responsive design rect helper.");
            method.Invoke(component, new[] { (object)objectName, designRect, anchor });
        }

        private static void SetPrivateField(Component component, string fieldName, object value)
        {
            FieldInfo field = component.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"Missing {component.GetType().Name}.{fieldName}.");
            field.SetValue(component, value);
        }

        private static Dictionary<long, int> BuildExpectedSceneBindings(GameObject prefab, string scenePath)
        {
            IEnumerable<(string ButtonName, int ActionId)> expected =
                string.Equals(scenePath, CourtyardScenePath, StringComparison.Ordinal)
                    ? ActionButtonBindings.Take(2)
                    : ActionButtonBindings;
            var result = new Dictionary<long, int>();
            foreach ((string buttonName, int actionId) in expected)
            {
                Transform button = RequireUniqueNamedTransform(prefab.transform, buttonName);
                long sourceFileId = RequireLocalFileId(button.gameObject);
                result.Add(sourceFileId, actionId);
            }

            return result;
        }

        private static Dictionary<long, long> ParseCanonicalStrippedGameObjects(string yaml)
        {
            string pattern =
                $@"^--- !u!1 &(?<localId>-?\d+) stripped\r?\nGameObject:\r?\n" +
                $@"  m_CorrespondingSourceObject: \{{fileID: (?<sourceId>-?\d+), guid: {CombatHudPrefabGuid}, type: 3\}}";
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

                long localGameObjectId = ReadLong(body, @"^  m_GameObject: \{fileID: (?<value>-?\d+)\}");
                if (!strippedGameObjects.TryGetValue(localGameObjectId, out long sourceGameObjectId))
                {
                    continue;
                }

                int actionId = checked((int)ReadLong(body, @"^  actionId: (?<value>-?\d+)$"));
                long bridgeId = ReadLong(body, @"^  inputBridge: \{fileID: (?<value>-?\d+)\}");
                Assert.That(bridgeId, Is.Not.Zero, $"Scene action {actionId} lost its HUD input bridge.");
                Assert.That(result.ContainsKey(sourceGameObjectId), Is.False,
                    $"Duplicate pointer input on prefab source object {sourceGameObjectId}.");
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

            // A prefab root only receives a stripped GameObject record when a scene-owned
            // component references it. Station/Corridor therefore begin at m_TransformParent,
            // while Courtyard's scene-owned Canvas makes the optional self candidate visible.
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
                    $"{scenePath} canonical HUD parent transform {parentTransformId} is not serialized in the scene.");
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
                $"{scenePath} canonical HUD must have a Canvas on its own object or parent chain.");

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
                $"{scenePath} canonical HUD's nearest Canvas must own exactly one CanvasScaler.");
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
                RequireUniqueNamedTransform(prefab.transform, "MoveJoystickRing").gameObject);
            int found = 0;
            foreach (string body in EnumerateMonoBehaviourBodies(yaml))
            {
                if (body.IndexOf($"guid: {VirtualJoystickScriptGuid}", StringComparison.Ordinal) < 0)
                {
                    continue;
                }

                long localGameObjectId = ReadLong(body, @"^  m_GameObject: \{fileID: (?<value>-?\d+)\}");
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

            Assert.That(found, Is.EqualTo(1), $"{scenePath} must bind one joystick to MoveJoystickRing.");
        }

        private static void AssertNoCanonicalVisualOverrides(
            GameObject prefab,
            string scenePath,
            string yaml)
        {
            long prefabRootRectId = RequireLocalFileId(prefab.GetComponent<RectTransform>());
            string pattern =
                $@"^    - target: \{{fileID: (?<sourceId>-?\d+), guid: {CombatHudPrefabGuid}, type: 3\}}\r?\n" +
                @"      propertyPath: (?<property>[^\r\n]+)";
            var offenders = new List<string>();
            foreach (Match match in Regex.Matches(yaml, pattern, RegexOptions.Multiline))
            {
                long sourceId = long.Parse(match.Groups["sourceId"].Value);
                string property = match.Groups["property"].Value;
                bool spriteOrMaterial = property == "m_Sprite"
                    || property == "m_Material"
                    || property.StartsWith("m_Color.", StringComparison.Ordinal)
                    || property == "m_RaycastTarget";
                bool layout = property.StartsWith("m_AnchoredPosition.", StringComparison.Ordinal)
                    || property.StartsWith("m_AnchorMin.", StringComparison.Ordinal)
                    || property.StartsWith("m_AnchorMax.", StringComparison.Ordinal)
                    || property.StartsWith("m_Pivot.", StringComparison.Ordinal)
                    || property.StartsWith("m_SizeDelta.", StringComparison.Ordinal)
                    || property.StartsWith("m_LocalScale.", StringComparison.Ordinal);
                if (spriteOrMaterial || (layout && sourceId != prefabRootRectId))
                {
                    offenders.Add($"{sourceId}:{property}");
                }
            }

            Assert.That(
                offenders,
                Is.Empty,
                $"{scenePath} still overrides canonical HUD visual/layout data: {string.Join(", ", offenders)}");
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
                @"^--- !u!(?:4|224) &(?<componentId>-?\d+)\r?\n(?:Transform|RectTransform):\r?\n" +
                @"(?<body>.*?)(?=^--- !u!|\z)";
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

                long componentId = long.Parse(match.Groups["componentId"].Value);
                result[componentId] = (
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
                "Expected exactly one canonical HUD PrefabInstance block.");
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
    }
}
