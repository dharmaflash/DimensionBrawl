using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DimensionBrawl.Editor
{
    /// <summary>
    /// Applies the reviewed v19 celestial HUD resources to the one canonical combat HUD.
    /// The operation deliberately preserves object names, serialized presenter bindings,
    /// and scene-added input components. Only the canonical prefab's presentation and the
    /// matching visual/layout property overrides in the two legacy stage scenes are changed.
    /// </summary>
    public static class CombatHudCelestialPrefabAssembler
    {
        private const string PrefabPath =
            "Assets/_Game/UI/CombatHud/PF_UI_CombatHud.prefab";
        private const string ArtRoot =
            "Assets/_Game/UI/CombatHud/Art/CelestialHud";
        private const string HudFontPath =
            "Assets/_Game/Art/Fonts/Pretendard/Pretendard-SemiBold.otf";
        private const string VitalMaterialPath =
            "Assets/_Game/UI/CombatHud/Materials/DB_UI_CelestialFlow_Vital.mat";
        private const string EnergyMaterialPath =
            "Assets/_Game/UI/CombatHud/Materials/DB_UI_CelestialFlow_Energy.mat";
        private const float DesignWidth = 2560f;
        private const float DesignHeight = 1440f;

        private static readonly string[] CanonicalScenePaths =
        {
            "Assets/_Game/Scenes/OlympusStationCombatStage.unity",
            "Assets/_Game/Scenes/OlympusCorridorInvasionStage.unity",
            "Assets/_Game/Scenes/OlympusCourtyardDrillStage.unity"
        };

        private static readonly string[] RequiredSpriteNames =
        {
            "objective_frame.png",
            "boss_frame.png",
            "boss_hp_fill.png",
            "boss_cost_fill.png",
            "pause.png",
            "summon_s1_frame.png",
            "summon_s1_portrait.png",
            "summon_s2_frame.png",
            "summon_s2_portrait.png",
            "summon_s3_frame.png",
            "summon_s3_portrait.png",
            "action_weapon_swap.png",
            "action_ultimate.png",
            "action_dash.png",
            "action_attack_ranged.png",
            "joystick_base.png",
            "joystick_knob.png",
            "player_portrait_frame.png",
            "player_portrait.png",
            "player_hp_rail.png",
            "player_hp_fill.png",
            "player_en_rail.png",
            "player_en_fill.png",
            "player_ammo_chip.png",
            "reticle.png"
        };

        private static readonly HashSet<string> CanonicalVisibilityPaths = new HashSet<string>(StringComparer.Ordinal)
        {
            "InputMode",
            "DimensionHudSkinRoot/BossHudRoot/BossSymbol",
            "DimensionHudSkinRoot/PlayerSymbol",
            "DimensionHudSkinRoot/PlayerNameArea",
            "DimensionHudSkinRoot/PlayerHpAmountArea",
            "DimensionHudSkinRoot/PlayerMpAmountArea"
        };

        private enum DesignAnchor
        {
            LeftTop,
            LeftBottom,
            RightTop,
            RightBottom,
            CenterTop,
            CenterBottom,
            CenterScreen
        }

        [MenuItem("DimensionBrawl/UI V1/Apply Celestial Combat HUD To Canonical Prefab")]
        public static void ApplyFromMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            ApplyForBatchMode();
        }

        /// <summary>
        /// Unity CLI entry point used by the project-level assembly/visual-QA pass.
        /// </summary>
        public static void ApplyForBatchMode()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            ConfigureUiSpriteImporters();
            Dictionary<string, Sprite> sprites = LoadRequiredSprites();

            // The setup owns only material assets. This assembler attaches those materials
            // to the four meter fills after every required input has been validated.
            CombatHudFlowPresentationSetup.CreateMaterials();
            Material vitalMaterial = RequireAsset<Material>(VitalMaterialPath);
            Material energyMaterial = RequireAsset<Material>(EnergyMaterialPath);

            AssembleCanonicalPrefab(sprites, vitalMaterial, energyMaterial);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            int removedOverrideCount = 0;
            int normalizedCanvasScalerCount = 0;
            for (int i = 0; i < CanonicalScenePaths.Length; i++)
            {
                removedOverrideCount += RemoveManagedSceneOverrides(
                    CanonicalScenePaths[i],
                    ref normalizedCanvasScalerCount);
            }

            AssetDatabase.SaveAssets();
            Debug.Log(
                "Applied the v19 celestial combat HUD to the canonical prefab. "
                + $"Removed {removedOverrideCount} legacy visual/layout property overrides; "
                + $"normalized {normalizedCanvasScalerCount} canonical HUD CanvasScaler component(s); "
                + "scene-added combat input components and all unrelated scene changes were preserved.");
        }

        private static void ConfigureUiSpriteImporters()
        {
            string[] textureGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { ArtRoot });
            int configuredCount = 0;
            for (int i = 0; i < textureGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(textureGuids[i]).Replace('\\', '/');
                string directory = Path.GetDirectoryName(path)?.Replace('\\', '/');
                if (!string.Equals(directory, ArtRoot, StringComparison.Ordinal)
                    || !string.Equals(Path.GetExtension(path), ".png", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null)
                {
                    throw new InvalidOperationException($"Missing texture importer for HUD sprite: {path}");
                }

                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.mipmapEnabled = false;
                importer.sRGBTexture = true;
                importer.alphaIsTransparency = true;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.filterMode = FilterMode.Bilinear;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.textureCompression = TextureImporterCompression.CompressedHQ;
                importer.maxTextureSize = 4096;
                importer.SaveAndReimport();
                configuredCount++;
            }

            if (configuredCount != 27)
            {
                throw new InvalidOperationException(
                    $"Expected exactly 27 runtime PNG sprites directly under {ArtRoot}, "
                    + $"but configured {configuredCount}. QA and Motion images must stay in subfolders.");
            }
        }

        private static Dictionary<string, Sprite> LoadRequiredSprites()
        {
            var sprites = new Dictionary<string, Sprite>(StringComparer.Ordinal);
            for (int i = 0; i < RequiredSpriteNames.Length; i++)
            {
                string fileName = RequiredSpriteNames[i];
                string path = $"{ArtRoot}/{fileName}";
                sprites.Add(fileName, RequireAsset<Sprite>(path));
            }

            return sprites;
        }

        private static void AssembleCanonicalPrefab(
            IReadOnlyDictionary<string, Sprite> sprites,
            Material vitalMaterial,
            Material energyMaterial)
        {
            GameObject prefabAsset = RequireAsset<GameObject>(PrefabPath);
            if (!string.Equals(prefabAsset.name, "PF_UI_CombatHud", StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Refusing to mutate unexpected prefab root '{prefabAsset.name}' at {PrefabPath}.");
            }

            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                Transform root = prefabRoot.transform;
                ValidateCanonicalBindingObjects(root);

                ConfigureTopLeft(root, sprites);
                ConfigureBoss(root, sprites, vitalMaterial, energyMaterial);
                ConfigurePause(root, sprites);
                ConfigureSummons(root, sprites);
                ConfigureActions(root, sprites);
                ConfigureJoystick(root, sprites);
                ConfigurePlayerVitals(root, sprites, vitalMaterial, energyMaterial);
                ConfigureReticle(root, sprites);

                GameObject saved = PrefabUtility.SaveAsPrefabAsset(prefabRoot, PrefabPath);
                if (saved == null)
                {
                    throw new InvalidOperationException($"Could not save canonical combat HUD prefab: {PrefabPath}");
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        private static void ValidateCanonicalBindingObjects(Transform root)
        {
            string[] requiredUniqueNames =
            {
                "TopLeftPanel",
                "Timer",
                "Objective",
                "ActionFeedback",
                "PauseButton",
                "BossHudRoot",
                "BossSymbol",
                "BossNameArea",
                "BossHpBackground",
                "BossHpFill",
                "BossCostBackground",
                "BossCostFill",
                "SummonSlot1Button",
                "SummonSlot2Button",
                "SummonSlot3Button",
                "UltimateButton",
                "Skill1Button",
                "DodgeButton",
                "BasicAttackButton",
                "MoveJoystickRing",
                "MoveJoystickKnob",
                "HealthBar_Track",
                "HealthBar",
                "HealthText",
                "ResourceBar_Track",
                "ResourceBar",
                "ResourceText",
                "AmmoText",
                "InputMode",
                "PlayerSymbol",
                "PlayerNameArea",
                "PlayerHpAmountArea",
                "PlayerMpAmountArea"
            };

            for (int i = 0; i < requiredUniqueNames.Length; i++)
            {
                RequireUniqueTransform(root, requiredUniqueNames[i]);
            }
        }

        private static void ConfigureTopLeft(
            Transform root,
            IReadOnlyDictionary<string, Sprite> sprites)
        {
            RectTransform panel = RequireRect(root, "TopLeftPanel");
            // PGR-style information hierarchy: the objective is a low, wide ribbon below
            // the boss header rather than a decorated card competing with it at y=32.
            SetDesignRect(panel, new Rect(24f, 316f, 760f, 160f), DesignAnchor.LeftTop);
            ConfigureStaticImage(panel.GetComponent<Image>(), sprites["objective_frame.png"]);

            Font font = RequireAsset<Font>(HudFontPath);
            RectTransform objective = RequireRect(root, "Objective");
            SetDesignRect(objective, new Rect(88f, 329f, 620f, 126f), DesignAnchor.LeftTop);
            Text objectiveText = objective.GetComponent<Text>();
            ConfigureHudText(
                objectiveText,
                font,
                "Break the pressure line",
                42,
                TextAnchor.MiddleLeft,
                new Color(0.94f, 0.97f, 1f, 1f));
            // Boss objectives can grow as the encounter state changes. Keep those cues
            // inside the authored ribbon instead of allowing legacy Text overflow to
            // cross the camera or boss HUD on narrow phones.
            objectiveText.horizontalOverflow = HorizontalWrapMode.Wrap;
            objectiveText.verticalOverflow = VerticalWrapMode.Truncate;

            Image timerBacking = EnsureRootImage(root, "MissionTimerBacking");
            SetDesignRect(
                timerBacking.rectTransform,
                new Rect(2014f, 47f, 184f, 86f),
                DesignAnchor.RightTop);
            ClearImage(timerBacking);
            timerBacking.color = new Color(0.02f, 0.025f, 0.035f, 0.22f);

            RectTransform timer = RequireRect(root, "Timer");
            SetDesignRect(timer, new Rect(2026f, 47f, 160f, 86f), DesignAnchor.RightTop);
            ConfigureHudText(
                timer.GetComponent<Text>(),
                font,
                "03:00",
                46,
                TextAnchor.MiddleCenter,
                new Color(0.97f, 0.985f, 1f, 1f));

            // These are independent root-level binding objects. Keep each background
            // immediately below its readout without disturbing any input component.
            int missionFrontIndex = Mathf.Min(panel.GetSiblingIndex(), objective.GetSiblingIndex());
            panel.SetSiblingIndex(missionFrontIndex);
            objective.SetSiblingIndex(missionFrontIndex + 1);
            int timerFrontIndex = Mathf.Min(timerBacking.transform.GetSiblingIndex(), timer.GetSiblingIndex());
            timerBacking.transform.SetSiblingIndex(timerFrontIndex);
            timer.SetSiblingIndex(timerFrontIndex + 1);
        }

        private static void ConfigureBoss(
            Transform root,
            IReadOnlyDictionary<string, Sprite> sprites,
            Material vitalMaterial,
            Material energyMaterial)
        {
            RectTransform nameArea = RequireRect(root, "BossNameArea");
            SetDesignRect(nameArea, new Rect(796f, 52f, 1056f, 132f), DesignAnchor.CenterTop);
            ConfigureStaticImage(nameArea.GetComponent<Image>(), sprites["boss_frame.png"]);

            // boss_frame is the reviewed two-line underlay. Keeping the old split track
            // images as well would double its borders, so those two nodes remain bound but clear.
            RectTransform hpTrack = RequireRect(root, "BossHpBackground");
            SetDesignRect(hpTrack, new Rect(839f, 104f, 913f, 18f), DesignAnchor.CenterTop);
            ClearImage(hpTrack.GetComponent<Image>());

            RectTransform costTrack = RequireRect(root, "BossCostBackground");
            SetDesignRect(costTrack, new Rect(839f, 147f, 913f, 14f), DesignAnchor.CenterTop);
            ClearImage(costTrack.GetComponent<Image>());

            RectTransform hpFillRect = RequireRect(root, "BossHpFill");
            // Fill through the frame's complete alpha aperture. The combined frame is
            // rendered above these strips, so its antialiased edges retain the authored
            // silhouette while the bar no longer reads as a thin line inside the opening.
            SetDesignRect(hpFillRect, new Rect(842f, 103f, 741f, 29f), DesignAnchor.CenterTop);
            ConfigureFillImage(
                hpFillRect.GetComponent<Image>(),
                sprites["boss_hp_fill.png"],
                vitalMaterial);

            RectTransform costFillRect = RequireRect(root, "BossCostFill");
            SetDesignRect(costFillRect, new Rect(842f, 138f, 821f, 13f), DesignAnchor.CenterTop);
            ConfigureFillImage(
                costFillRect.GetComponent<Image>(),
                sprites["boss_cost_fill.png"],
                energyMaterial);

            // The combined frame has transparent meter apertures. Render both fills behind
            // it so their bevels cannot paint over the frame edges, then keep runtime text
            // above the completed stack.
            hpFillRect.SetSiblingIndex(0);
            costFillRect.SetSiblingIndex(1);
            nameArea.SetSiblingIndex(2);

            RectTransform actionFeedback = RequireRect(root, "ActionFeedback");
            SetDesignRect(
                actionFeedback,
                new Rect(850f, 57f, 500f, 46f),
                DesignAnchor.CenterTop);
            ConfigureHudText(
                actionFeedback.GetComponent<Text>(),
                RequireAsset<Font>(HudFontPath),
                string.Empty,
                24,
                TextAnchor.MiddleCenter,
                new Color(0.90f, 0.97f, 1f, 1f));
            actionFeedback.SetAsLastSibling();
            RequireUniqueTransform(root, "BossSymbol").gameObject.SetActive(false);
        }

        private static void ConfigurePause(
            Transform root,
            IReadOnlyDictionary<string, Sprite> sprites)
        {
            RectTransform pause = RequireRect(root, "PauseButton");
            SetDesignRect(pause, new Rect(2404f, 44f, 89f, 89f), DesignAnchor.RightTop);
            ConfigureButtonImage(pause.GetComponent<Image>(), sprites["pause.png"]);
            HideDirectChild(pause, "Label");
        }

        private static void ConfigureSummons(
            Transform root,
            IReadOnlyDictionary<string, Sprite> sprites)
        {
            Font font = RequireAsset<Font>(HudFontPath);
            ConfigureSummon(
                RequireRect(root, "SummonSlot1Button"),
                new Rect(2263f, 171f, 211f, 226f),
                sprites["summon_s1_frame.png"],
                sprites["summon_s1_portrait.png"],
                sprites["player_portrait.png"],
                font);
            ConfigureSummon(
                RequireRect(root, "SummonSlot2Button"),
                new Rect(2275f, 413f, 193f, 211f),
                sprites["summon_s2_frame.png"],
                sprites["summon_s2_portrait.png"],
                sprites["player_portrait.png"],
                font);
            ConfigureSummon(
                RequireRect(root, "SummonSlot3Button"),
                new Rect(2275f, 640f, 193f, 211f),
                sprites["summon_s3_frame.png"],
                sprites["summon_s3_portrait.png"],
                sprites["player_portrait.png"],
                font);
        }

        private static void ConfigureSummon(
            RectTransform slot,
            Rect designRect,
            Sprite frame,
            Sprite portrait,
            Sprite circularMaskSprite,
            Font font)
        {
            SetDesignRect(slot, designRect, DesignAnchor.RightTop);
            Image buttonImage = slot.GetComponent<Image>();
            ConfigureButtonImage(buttonImage, frame);

            // The button's Image remains the target Graphic and retains the canonical frame
            // sprite for bindings/tests, but its visible copy is rendered by FrameOverlay.
            // This lets the portrait branch sit below the frame without duplicating its
            // translucent chrome.
            buttonImage.color = Color.clear;

            Image portraitMaskImage = EnsureChildImage(slot, "PortraitMask");
            ConfigureStaticImage(portraitMaskImage, circularMaskSprite, preserveAspect: true);
            portraitMaskImage.raycastTarget = false;
            Mask portraitMask = portraitMaskImage.GetComponent<Mask>();
            if (portraitMask == null)
            {
                portraitMask = portraitMaskImage.gameObject.AddComponent<Mask>();
            }

            portraitMask.enabled = true;
            portraitMask.showMaskGraphic = false;

            Image icon = RequireSummonPortraitImage(slot, portraitMaskImage.rectTransform, "Icon");
            ConfigureStaticImage(icon, portrait, preserveAspect: true);
            icon.maskable = true;
            float portraitSize = designRect.width * 0.80f;
            float maskSize = designRect.width * 0.75f;
            float portraitLift = designRect.height * 0.085f;
            SetCenteredChildRect(portraitMaskImage.rectTransform, maskSize, maskSize);
            portraitMaskImage.rectTransform.anchoredPosition = new Vector2(0f, portraitLift);
            SetCenteredChildRect(icon.rectTransform, portraitSize, portraitSize);

            Image disabledIcon = RequireSummonPortraitImage(
                slot,
                portraitMaskImage.rectTransform,
                "IconDisabled");
            ConfigureStaticImage(disabledIcon, portrait, preserveAspect: true);
            disabledIcon.maskable = true;
            SetCenteredChildRect(disabledIcon.rectTransform, portraitSize, portraitSize);

            Image frameOverlay = EnsureChildImage(slot, "FrameOverlay");
            ConfigureStaticImage(frameOverlay, frame, preserveAspect: true);
            StretchToParent(frameOverlay.rectTransform);

            Image cooldownFill = FindSummonLayerImage(
                slot,
                portraitMaskImage.rectTransform,
                "CooldownFill");
            if (cooldownFill != null)
            {
                cooldownFill.raycastTarget = false;
                StretchToParent(cooldownFill.rectTransform);
                cooldownFill.maskable = true;
            }

            Image readyRing = FindDirectImage(slot, "ReadyRing");
            if (readyRing != null)
            {
                readyRing.raycastTarget = false;
                SetCenteredChildRect(readyRing.rectTransform, designRect.width, designRect.height);
            }

            Image readyGlow = FindDirectImage(slot, "ReadyGlow");
            if (readyGlow != null)
            {
                readyGlow.raycastTarget = false;
                SetCenteredChildRect(readyGlow.rectTransform, designRect.width, designRect.height);
            }

            // ReadyGlow remains the back-most state layer. The clipped portrait follows,
            // then the frame overlay; cooldown/readiness/state children stay above them.
            if (readyGlow != null)
            {
                readyGlow.transform.SetSiblingIndex(0);
            }

            int portraitMaskIndex = readyGlow != null ? 1 : 0;
            portraitMaskImage.transform.SetSiblingIndex(portraitMaskIndex);
            frameOverlay.transform.SetSiblingIndex(portraitMaskIndex + 1);

            Image spark = FindDirectImage(slot, "ReadySparkRing");
            if (spark != null)
            {
                spark.raycastTarget = false;
            }

            HideDirectChild(slot, "Label");
            Transform state = slot.Find("State");
            if (state != null && state is RectTransform stateRect)
            {
                stateRect.anchorMin = new Vector2(0f, 0f);
                stateRect.anchorMax = new Vector2(1f, 0f);
                stateRect.pivot = new Vector2(0.5f, 0f);
                stateRect.anchoredPosition = new Vector2(0f, 8f);
                stateRect.sizeDelta = new Vector2(-16f, 42f);
                stateRect.localScale = Vector3.one;

                Text stateText = state.GetComponent<Text>();
                if (stateText != null)
                {
                    ConfigureHudText(
                        stateText,
                        font,
                        string.Empty,
                        24,
                        TextAnchor.MiddleCenter,
                        new Color(0.90f, 0.97f, 1f, 1f));
                }
            }
        }

        private static void ConfigureActions(
            Transform root,
            IReadOnlyDictionary<string, Sprite> sprites)
        {
            // Object/action bindings remain untouched. Only their visual roles and rectangles
            // are mapped to the reviewed upper-left/upper-right/lower-left/lower-right cluster.
            ConfigureAction(
                RequireRect(root, "UltimateButton"),
                new Rect(2059f, 967f, 171f, 171f),
                sprites["action_weapon_swap.png"]);
            ConfigureAction(
                RequireRect(root, "Skill1Button"),
                new Rect(2261f, 938f, 187f, 187f),
                sprites["action_ultimate.png"]);
            ConfigureAction(
                RequireRect(root, "DodgeButton"),
                new Rect(2046f, 1177f, 184f, 184f),
                sprites["action_dash.png"]);
            ConfigureAction(
                RequireRect(root, "BasicAttackButton"),
                new Rect(2248f, 1131f, 273f, 272f),
                sprites["action_attack_ranged.png"]);
        }

        private static void ConfigureAction(RectTransform button, Rect designRect, Sprite sprite)
        {
            SetDesignRect(button, designRect, DesignAnchor.RightBottom);
            ConfigureButtonImage(button.GetComponent<Image>(), sprite);
            HideDirectChild(button, "Label");

            Image cooldownFill = FindDirectImage(button, "CooldownFill");
            if (cooldownFill != null)
            {
                cooldownFill.raycastTarget = false;
                StretchToParent(cooldownFill.rectTransform);
            }

            Image readyGlow = FindDirectImage(button, "ReadyGlow")
                ?? FindDirectImage(button, "ActionReadyGlow");
            if (readyGlow != null)
            {
                readyGlow.raycastTarget = false;
                StretchToParent(readyGlow.rectTransform);
            }

            Image readyRing = FindDirectImage(button, "DodgeCooldownRing");
            if (readyRing != null)
            {
                readyRing.raycastTarget = false;
                StretchToParent(readyRing.rectTransform);
            }
        }

        private static void ConfigureJoystick(
            Transform root,
            IReadOnlyDictionary<string, Sprite> sprites)
        {
            RectTransform ring = RequireRect(root, "MoveJoystickRing");
            SetDesignRect(ring, new Rect(201f, 979f, 269f, 269f), DesignAnchor.LeftBottom);
            ConfigureButtonImage(ring.GetComponent<Image>(), sprites["joystick_base.png"]);

            RectTransform knob = RequireRect(root, "MoveJoystickKnob");
            SetDesignRect(knob, new Rect(285f, 1063f, 101f, 101f), DesignAnchor.LeftBottom);
            ConfigureStaticImage(knob.GetComponent<Image>(), sprites["joystick_knob.png"], preserveAspect: true);
        }

        private static void ConfigurePlayerVitals(
            Transform root,
            IReadOnlyDictionary<string, Sprite> sprites,
            Material vitalMaterial,
            Material energyMaterial)
        {
            RectTransform compactStatus = RequireRect(root, "InputMode");
            compactStatus.gameObject.SetActive(true);
            SetDesignRect(
                compactStatus,
                new Rect(805f, 1380f, 500f, 32f),
                DesignAnchor.CenterBottom);
            ConfigureHudText(
                compactStatus.GetComponent<Text>(),
                RequireAsset<Font>(HudFontPath),
                string.Empty,
                20,
                TextAnchor.MiddleCenter,
                new Color(0.90f, 0.98f, 1f, 1f));
            RequireUniqueTransform(root, "PlayerSymbol").gameObject.SetActive(false);
            RequireUniqueTransform(root, "PlayerNameArea").gameObject.SetActive(false);
            RequireUniqueTransform(root, "PlayerHpAmountArea").gameObject.SetActive(false);
            RequireUniqueTransform(root, "PlayerMpAmountArea").gameObject.SetActive(false);

            RectTransform hpTrack = RequireRect(root, "HealthBar_Track");
            // These two source rails contain substantial transparent padding. Expand only
            // their RectTransforms so the visible alpha bounds remain at the reviewed
            // 805..1601 meter bounds and contain the authored full-width fills.
            SetDesignRect(hpTrack, new Rect(731f, 1287f, 944f, 49f), DesignAnchor.CenterBottom);
            ConfigureStaticImage(hpTrack.GetComponent<Image>(), sprites["player_hp_rail.png"]);

            RectTransform hpFill = RequireRect(root, "HealthBar");
            SetDesignRect(hpFill, new Rect(818f, 1302f, 766f, 15f), DesignAnchor.CenterBottom);
            ConfigureFillImage(hpFill.GetComponent<Image>(), sprites["player_hp_fill.png"], vitalMaterial);

            RectTransform enTrack = RequireRect(root, "ResourceBar_Track");
            SetDesignRect(enTrack, new Rect(780f, 1333f, 846f, 40f), DesignAnchor.CenterBottom);
            ConfigureStaticImage(enTrack.GetComponent<Image>(), sprites["player_en_rail.png"]);

            RectTransform enFill = RequireRect(root, "ResourceBar");
            SetDesignRect(enFill, new Rect(818f, 1347f, 766f, 12f), DesignAnchor.CenterBottom);
            ConfigureFillImage(enFill.GetComponent<Image>(), sprites["player_en_fill.png"], energyMaterial);

            SetDesignRect(
                RequireRect(root, "HealthText"),
                new Rect(1390f, 1246f, 214f, 45f),
                DesignAnchor.CenterBottom);
            SetDesignRect(
                RequireRect(root, "ResourceText"),
                new Rect(1415f, 1324f, 180f, 43f),
                DesignAnchor.CenterBottom);
            SetDesignRect(
                RequireRect(root, "AmmoText"),
                new Rect(1623f, 1294f, 125f, 56f),
                DesignAnchor.CenterBottom);

            RectTransform portraitFrame = EnsureRootImage(root, "PlayerPortraitFrame").rectTransform;
            SetDesignRect(
                portraitFrame,
                new Rect(686f, 1262f, 153f, 153f),
                DesignAnchor.CenterBottom);
            ConfigureStaticImage(
                portraitFrame.GetComponent<Image>(),
                sprites["player_portrait_frame.png"],
                preserveAspect: true);

            Image portrait = EnsureChildImage(portraitFrame, "PlayerPortrait");
            ConfigureStaticImage(portrait, sprites["player_portrait.png"], preserveAspect: true);
            SetCenteredChildRect(portrait.rectTransform, 116f, 116f);
            portrait.transform.SetAsFirstSibling();

            RectTransform resourceFillRect = RequireRect(root, "ResourceBar");
            portraitFrame.SetSiblingIndex(resourceFillRect.GetSiblingIndex() + 1);

            Image ammoChip = EnsureRootImage(root, "PlayerAmmoChip");
            SetDesignRect(
                ammoChip.rectTransform,
                new Rect(1614f, 1284f, 144f, 77f),
                DesignAnchor.CenterBottom);
            ConfigureStaticImage(ammoChip, sprites["player_ammo_chip.png"], preserveAspect: true);
            ammoChip.transform.SetSiblingIndex(RequireRect(root, "AmmoText").GetSiblingIndex());
        }

        private static void ConfigureReticle(
            Transform root,
            IReadOnlyDictionary<string, Sprite> sprites)
        {
            Image reticle = EnsureRootImage(root, "CenterAimReticle");
            SetDesignRect(
                reticle.rectTransform,
                new Rect(1232.5f, 672.5f, 95f, 95f),
                DesignAnchor.CenterScreen);
            ConfigureStaticImage(reticle, sprites["reticle.png"], preserveAspect: true);

            // Keep the reticle under the touch controls and session overlay while remaining
            // above the world render.
            reticle.transform.SetSiblingIndex(RequireRect(root, "BasicAttackButton").GetSiblingIndex());
        }

        private static int RemoveManagedSceneOverrides(
            string scenePath,
            ref int normalizedCanvasScalerCount)
        {
            Scene scene = SceneManager.GetSceneByPath(scenePath);
            bool openedHere = !scene.IsValid() || !scene.isLoaded;
            if (openedHere)
            {
                scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            }

            try
            {
                if (!scene.IsValid() || !scene.isLoaded)
                {
                    throw new InvalidOperationException($"Could not open canonical scene: {scenePath}");
                }

                GameObject instanceRoot = RequireCanonicalPrefabInstance(scene);
                PropertyModification[] modifications =
                    PrefabUtility.GetPropertyModifications(instanceRoot) ?? Array.Empty<PropertyModification>();
                PropertyModification[] retained = modifications
                    .Where(modification => !IsManagedVisualOverride(modification))
                    .ToArray();
                int removed = modifications.Length - retained.Length;
                if (removed > 0)
                {
                    PrefabUtility.SetPropertyModifications(instanceRoot, retained);
                }

                bool scalerChanged = NormalizeCanonicalCanvasScaler(instanceRoot);
                if (scalerChanged)
                {
                    normalizedCanvasScalerCount++;
                }

                if (removed > 0 || scalerChanged)
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                    if (!EditorSceneManager.SaveScene(scene))
                    {
                        throw new InvalidOperationException($"Could not save canonical scene: {scenePath}");
                    }
                }

                return removed;
            }
            finally
            {
                if (openedHere && scene.IsValid() && scene.isLoaded)
                {
                    EditorSceneManager.CloseScene(scene, removeScene: true);
                }
            }
        }

        private static bool NormalizeCanonicalCanvasScaler(GameObject instanceRoot)
        {
            Canvas nearestCanvas = instanceRoot.GetComponentInParent<Canvas>(includeInactive: true);
            if (nearestCanvas == null || nearestCanvas.gameObject.scene != instanceRoot.scene)
            {
                throw new InvalidOperationException(
                    $"Canonical combat HUD instance '{instanceRoot.name}' has no Canvas ancestor in its scene.");
            }

            CanvasScaler nearestScaler =
                instanceRoot.GetComponentInParent<CanvasScaler>(includeInactive: true);
            CanvasScaler[] canvasScalers = nearestCanvas.GetComponents<CanvasScaler>();
            if (nearestScaler == null
                || nearestScaler.gameObject != nearestCanvas.gameObject
                || canvasScalers.Length != 1
                || canvasScalers[0] != nearestScaler)
            {
                throw new InvalidOperationException(
                    $"Nearest Canvas ancestor '{nearestCanvas.name}' for canonical combat HUD "
                    + $"'{instanceRoot.name}' must own exactly one CanvasScaler.");
            }

            CanvasScaler scaler = nearestScaler;
            var targetResolution = new Vector2(DesignWidth, DesignHeight);
            bool changed = scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize
                || scaler.referenceResolution != targetResolution
                || scaler.screenMatchMode != CanvasScaler.ScreenMatchMode.MatchWidthOrHeight
                || !Mathf.Approximately(scaler.matchWidthOrHeight, 1f);
            if (!changed)
            {
                return false;
            }

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = targetResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 1f;
            EditorUtility.SetDirty(scaler);
            PrefabUtility.RecordPrefabInstancePropertyModifications(scaler);
            return true;
        }

        private static GameObject RequireCanonicalPrefabInstance(Scene scene)
        {
            var matches = new HashSet<GameObject>();
            GameObject[] roots = scene.GetRootGameObjects();
            for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
            {
                Transform[] transforms =
                    roots[rootIndex].GetComponentsInChildren<Transform>(includeInactive: true);
                for (int i = 0; i < transforms.Length; i++)
                {
                    GameObject candidate = transforms[i].gameObject;
                    if (!PrefabUtility.IsAnyPrefabInstanceRoot(candidate))
                    {
                        continue;
                    }

                    GameObject source = PrefabUtility.GetCorrespondingObjectFromSource(candidate);
                    if (source != null
                        && string.Equals(
                            AssetDatabase.GetAssetPath(source),
                            PrefabPath,
                            StringComparison.Ordinal))
                    {
                        matches.Add(candidate);
                    }
                }
            }

            if (matches.Count != 1)
            {
                throw new InvalidOperationException(
                    $"Expected one canonical combat HUD instance in {scene.path}, found {matches.Count}.");
            }

            return matches.First();
        }

        private static bool IsManagedVisualOverride(PropertyModification modification)
        {
            if (modification == null || modification.target == null)
            {
                return false;
            }

            if (!string.Equals(
                    AssetDatabase.GetAssetPath(modification.target),
                    PrefabPath,
                    StringComparison.Ordinal))
            {
                return false;
            }

            GameObject targetGameObject = modification.target as GameObject;
            if (modification.target is Component component)
            {
                targetGameObject = component.gameObject;
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (targetGameObject == null || prefab == null)
            {
                return false;
            }

            string path = AnimationUtility.CalculateTransformPath(
                targetGameObject.transform,
                prefab.transform);
            string propertyPath = modification.propertyPath ?? string.Empty;
            bool isPrefabDescendant = !string.IsNullOrEmpty(path);
            if (isPrefabDescendant && modification.target is RectTransform)
            {
                return IsRectProperty(propertyPath);
            }

            // Text and other MaskableGraphic objects carry the same scene-level
            // color/material/raycast overrides as Image. This deliberately includes the
            // prefab root Graphic; only its RectTransform placement is instance-owned.
            if (modification.target is Graphic)
            {
                return IsGraphicVisualProperty(propertyPath);
            }

            return modification.target is GameObject
                && CanonicalVisibilityPaths.Contains(path)
                && string.Equals(propertyPath, "m_IsActive", StringComparison.Ordinal);
        }

        private static bool IsRectProperty(string propertyPath)
        {
            return propertyPath.StartsWith("m_AnchorMin.", StringComparison.Ordinal)
                || propertyPath.StartsWith("m_AnchorMax.", StringComparison.Ordinal)
                || propertyPath.StartsWith("m_AnchoredPosition.", StringComparison.Ordinal)
                || propertyPath.StartsWith("m_SizeDelta.", StringComparison.Ordinal)
                || propertyPath.StartsWith("m_Pivot.", StringComparison.Ordinal)
                || propertyPath.StartsWith("m_LocalScale.", StringComparison.Ordinal);
        }

        private static bool IsGraphicVisualProperty(string propertyPath)
        {
            return string.Equals(propertyPath, "m_Sprite", StringComparison.Ordinal)
                || string.Equals(propertyPath, "m_Material", StringComparison.Ordinal)
                || string.Equals(propertyPath, "m_Type", StringComparison.Ordinal)
                || string.Equals(propertyPath, "m_PreserveAspect", StringComparison.Ordinal)
                || string.Equals(propertyPath, "m_FillMethod", StringComparison.Ordinal)
                || string.Equals(propertyPath, "m_FillOrigin", StringComparison.Ordinal)
                || string.Equals(propertyPath, "m_FillClockwise", StringComparison.Ordinal)
                || string.Equals(propertyPath, "m_FillAmount", StringComparison.Ordinal)
                || string.Equals(propertyPath, "m_RaycastTarget", StringComparison.Ordinal)
                || propertyPath.StartsWith("m_RaycastPadding.", StringComparison.Ordinal)
                || propertyPath.StartsWith("m_Color.", StringComparison.Ordinal);
        }

        private static void SetDesignRect(RectTransform rectTransform, Rect designRect, DesignAnchor anchor)
        {
            float rightInset = DesignWidth - designRect.xMax;
            float bottomInset = DesignHeight - designRect.yMax;
            rectTransform.localScale = Vector3.one;

            switch (anchor)
            {
                case DesignAnchor.LeftTop:
                    rectTransform.anchorMin = new Vector2(0f, 1f);
                    rectTransform.anchorMax = new Vector2(0f, 1f);
                    rectTransform.pivot = new Vector2(0f, 1f);
                    rectTransform.anchoredPosition = new Vector2(designRect.xMin, -designRect.yMin);
                    break;
                case DesignAnchor.LeftBottom:
                    rectTransform.anchorMin = Vector2.zero;
                    rectTransform.anchorMax = Vector2.zero;
                    rectTransform.pivot = Vector2.zero;
                    rectTransform.anchoredPosition = new Vector2(designRect.xMin, bottomInset);
                    break;
                case DesignAnchor.RightTop:
                    rectTransform.anchorMin = Vector2.one;
                    rectTransform.anchorMax = Vector2.one;
                    rectTransform.pivot = Vector2.one;
                    rectTransform.anchoredPosition = new Vector2(-rightInset, -designRect.yMin);
                    break;
                case DesignAnchor.RightBottom:
                    rectTransform.anchorMin = new Vector2(1f, 0f);
                    rectTransform.anchorMax = new Vector2(1f, 0f);
                    rectTransform.pivot = new Vector2(1f, 0f);
                    rectTransform.anchoredPosition = new Vector2(-rightInset, bottomInset);
                    break;
                case DesignAnchor.CenterTop:
                    rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                    rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                    rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    rectTransform.anchoredPosition = new Vector2(
                        designRect.center.x - DesignWidth * 0.5f,
                        DesignHeight * 0.5f - designRect.center.y);
                    break;
                case DesignAnchor.CenterBottom:
                    rectTransform.anchorMin = new Vector2(0.5f, 0f);
                    rectTransform.anchorMax = new Vector2(0.5f, 0f);
                    rectTransform.pivot = new Vector2(0.5f, 0f);
                    rectTransform.anchoredPosition = new Vector2(
                        designRect.center.x - DesignWidth * 0.5f,
                        bottomInset);
                    break;
                case DesignAnchor.CenterScreen:
                    rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                    rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                    rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    rectTransform.anchoredPosition = new Vector2(
                        designRect.center.x - DesignWidth * 0.5f,
                        DesignHeight * 0.5f - designRect.center.y);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(anchor), anchor, null);
            }

            rectTransform.sizeDelta = designRect.size;
        }

        private static void ConfigureStaticImage(
            Image image,
            Sprite sprite,
            bool preserveAspect = false)
        {
            if (image == null)
            {
                throw new InvalidOperationException("Expected an Image on a managed combat HUD object.");
            }

            image.sprite = sprite;
            image.material = null;
            image.color = Color.white;
            image.raycastTarget = false;
            image.type = Image.Type.Simple;
            image.preserveAspect = preserveAspect;
            image.fillAmount = 1f;
        }

        private static void ConfigureHudText(
            Text text,
            Font font,
            string defaultValue,
            int fontSize,
            TextAnchor alignment,
            Color color)
        {
            if (text == null)
            {
                throw new InvalidOperationException(
                    "Expected a Text component on a managed combat HUD object.");
            }

            text.font = font;
            text.fontSize = fontSize;
            text.fontStyle = FontStyle.Normal;
            text.resizeTextForBestFit = false;
            text.alignment = alignment;
            text.alignByGeometry = true;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.color = color;
            text.raycastTarget = false;
            text.text = defaultValue;
        }

        private static void ConfigureButtonImage(Image image, Sprite sprite)
        {
            ConfigureStaticImage(image, sprite, preserveAspect: true);
            image.raycastTarget = true;
        }

        private static void ConfigureFillImage(Image image, Sprite sprite, Material material)
        {
            ConfigureStaticImage(image, sprite);
            image.material = material;
            image.type = Image.Type.Filled;
            image.fillMethod = Image.FillMethod.Horizontal;
            image.fillOrigin = (int)Image.OriginHorizontal.Left;
            image.fillClockwise = true;
            image.fillAmount = 1f;
        }

        private static void ClearImage(Image image)
        {
            if (image == null)
            {
                throw new InvalidOperationException("Expected an Image on a managed combat HUD object.");
            }

            image.sprite = null;
            image.material = null;
            image.color = Color.clear;
            image.raycastTarget = false;
            image.type = Image.Type.Simple;
            image.preserveAspect = false;
        }

        private static void SetCenteredChildRect(RectTransform rectTransform, float width, float height)
        {
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = new Vector2(width, height);
            rectTransform.localScale = Vector3.one;
        }

        private static void StretchToParent(RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.localScale = Vector3.one;
        }

        private static Image EnsureRootImage(Transform root, string objectName)
        {
            Transform existing = FindUniqueTransform(root, objectName, required: false);
            if (existing == null)
            {
                var gameObject = new GameObject(
                    objectName,
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image));
                gameObject.layer = root.gameObject.layer;
                gameObject.transform.SetParent(root, worldPositionStays: false);
                existing = gameObject.transform;
            }

            Image image = existing.GetComponent<Image>();
            if (image == null)
            {
                throw new InvalidOperationException(
                    $"Managed visual '{objectName}' already exists without an Image component.");
            }

            return image;
        }

        private static Image EnsureChildImage(RectTransform parent, string objectName)
        {
            Transform child = parent.Find(objectName);
            if (child == null)
            {
                var gameObject = new GameObject(
                    objectName,
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image));
                gameObject.layer = parent.gameObject.layer;
                gameObject.transform.SetParent(parent, worldPositionStays: false);
                child = gameObject.transform;
            }

            Image image = child.GetComponent<Image>();
            if (image == null)
            {
                throw new InvalidOperationException(
                    $"Managed child visual '{parent.name}/{objectName}' exists without an Image component.");
            }

            return image;
        }

        private static void HideDirectChild(RectTransform parent, string childName)
        {
            Transform child = parent.Find(childName);
            if (child != null)
            {
                child.gameObject.SetActive(false);
            }
        }

        private static RectTransform RequireRect(Transform root, string objectName)
        {
            RectTransform rect = RequireUniqueTransform(root, objectName) as RectTransform;
            if (rect == null)
            {
                throw new InvalidOperationException(
                    $"Canonical combat HUD object '{objectName}' is not a RectTransform.");
            }

            return rect;
        }

        private static Transform RequireUniqueTransform(Transform root, string objectName)
        {
            return FindUniqueTransform(root, objectName, required: true);
        }

        private static Transform FindUniqueTransform(Transform root, string objectName, bool required)
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

            if (count > 1 || (required && count != 1))
            {
                throw new InvalidOperationException(
                    $"Expected {(required ? "one" : "at most one")} '{objectName}' under {root.name}, found {count}.");
            }

            return match;
        }

        private static Image RequireDirectImage(RectTransform parent, string childName)
        {
            Image image = FindDirectImage(parent, childName);
            if (image == null)
            {
                throw new InvalidOperationException(
                    $"Missing Image '{parent.name}/{childName}' in canonical combat HUD.");
            }

            return image;
        }

        private static Image RequireSummonPortraitImage(
            RectTransform slot,
            RectTransform portraitMask,
            string childName)
        {
            Image image = FindSummonLayerImage(slot, portraitMask, childName);
            if (image == null)
            {
                throw new InvalidOperationException(
                    $"Missing Image '{slot.name}/{childName}' in canonical combat HUD.");
            }

            return image;
        }

        private static Image FindSummonLayerImage(
            RectTransform slot,
            RectTransform portraitMask,
            string childName)
        {
            Transform child = slot.Find(childName) ?? portraitMask.Find(childName);
            Image image = child != null ? child.GetComponent<Image>() : null;
            if (image == null)
            {
                return null;
            }

            if (child.parent != portraitMask)
            {
                child.SetParent(portraitMask, worldPositionStays: false);
            }

            return image;
        }

        private static Image FindDirectImage(RectTransform parent, string childName)
        {
            Transform child = parent.Find(childName);
            return child != null ? child.GetComponent<Image>() : null;
        }

        private static T RequireAsset<T>(string path) where T : UnityEngine.Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                throw new InvalidOperationException($"Missing required {typeof(T).Name}: {path}");
            }

            return asset;
        }
    }
}
