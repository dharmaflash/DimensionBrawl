using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DimensionBrawl.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace DimensionBrawl.Editor
{
    /// <summary>
    /// Assembles the componentized celestial V22 HUD without replacing canonical binding
    /// GameObjects. BuildStagingForBatchMode is the review-safe entry point; the canonical
    /// entry point is deliberately separate and must be invoked explicitly after approval.
    /// </summary>
    public static class CombatHudCelestialV2PrefabAssembler
    {
        public const string CanonicalPrefabPath =
            "Assets/_Game/UI/CombatHud/PF_UI_CombatHud.prefab";
        public const string StagingPrefabPath =
            "Assets/_Game/UI/CombatHud/PF_UI_CombatHud_CelestialV2_Staging.prefab";
        public const string AssemblySpecPath =
            "Assets/_Game/UI/CombatHud/CombatHudCelestialV2AssemblySpec.json";

        private const string HudFontPath =
            "Assets/_Game/Art/Fonts/Pretendard/Pretendard-SemiBold.otf";
        private const string VitalMaterialPath =
            "Assets/_Game/UI/CombatHud/Materials/DB_UI_CelestialFlow_Vital.mat";
        private const string EnergyMaterialPath =
            "Assets/_Game/UI/CombatHud/Materials/DB_UI_CelestialFlow_Energy.mat";

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

        private sealed class SpriteCatalog
        {
            private readonly Dictionary<string, Sprite> sprites;

            public SpriteCatalog(Dictionary<string, Sprite> sprites)
            {
                this.sprites = sprites;
            }

            public Sprite Require(string role)
            {
                if (!sprites.TryGetValue(role, out Sprite sprite) || sprite == null)
                {
                    throw new InvalidOperationException($"V22 sprite role '{role}' is not loaded.");
                }

                return sprite;
            }
        }

        [MenuItem("DimensionBrawl/UI V22/Validate Component Asset Pack")]
        public static void ValidateFromMenu()
        {
            ValidateAssetsForBatchMode();
            Debug.Log("Celestial V22 HUD asset manifest is valid.");
        }

        [MenuItem("DimensionBrawl/UI V22/Build Review Staging Prefab")]
        public static void BuildStagingFromMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            BuildStagingForBatchMode();
        }

        [MenuItem("DimensionBrawl/UI V22/Apply Approved Layout To Canonical Prefab")]
        public static void ApplyCanonicalFromMenu()
        {
            if (!EditorUtility.DisplayDialog(
                    "Apply approved V22 HUD?",
                    "This updates the canonical combat HUD prefab. The existing CelestialHud art pack remains untouched for rollback.",
                    "Apply V22",
                    "Cancel"))
            {
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            ApplyForBatchMode();
        }

        public static void ValidateAssetsForBatchMode()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            AssemblySpec spec = LoadAndValidateSpec();
            ValidateReferencedFiles(spec);
        }

        public static void BuildStagingForBatchMode()
        {
            Assemble(CanonicalPrefabPath, StagingPrefabPath, "review staging");
        }

        /// <summary>
        /// Explicit CLI entry point for the approved canonical mutation. This method does
        /// not touch scene instances, so scene-added pointer/joystick bindings remain owned
        /// by their scenes and the prior CelestialHud pack remains a direct rollback source.
        /// </summary>
        public static void ApplyForBatchMode()
        {
            Assemble(CanonicalPrefabPath, CanonicalPrefabPath, "canonical");
        }

        private static void Assemble(string sourcePrefabPath, string destinationPrefabPath, string label)
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            AssemblySpec spec = LoadAndValidateSpec();
            ValidateReferencedFiles(spec);
            ConfigureReferencedSpriteImporters(spec);
            SpriteCatalog sprites = LoadSprites(spec);

            CombatHudFlowPresentationSetup.CreateMaterials();
            Material vitalMaterial = RequireAsset<Material>(VitalMaterialPath);
            Material energyMaterial = RequireAsset<Material>(EnergyMaterialPath);
            Font font = RequireAsset<Font>(HudFontPath);

            GameObject source = RequireAsset<GameObject>(sourcePrefabPath);
            if (!string.Equals(source.name, "PF_UI_CombatHud", StringComparison.Ordinal)
                && !string.Equals(source.name, "PF_UI_CombatHud_CelestialV2_Staging", StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Refusing to assemble unexpected prefab root '{source.name}' at {sourcePrefabPath}.");
            }

            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(sourcePrefabPath);
            try
            {
                Transform root = prefabRoot.transform;
                ValidateCanonicalBindingObjects(root);
                EnsureLayoutProfile(prefabRoot);

                ConfigureObjectiveAndSystem(root, sprites, font);
                ConfigureBoss(root, sprites, font, vitalMaterial, energyMaterial);
                ConfigurePause(root, sprites);
                ConfigureSummons(root, sprites, font);
                ConfigureActions(root, sprites, font);
                ConfigureJoystick(root, sprites);
                ConfigurePlayer(root, sprites, font, vitalMaterial, energyMaterial);
                ConfigureReticle(root, sprites);
                ConfigurePresenterBindings(prefabRoot);
                ValidateRaycastOwnership(root);

                GameObject saved = PrefabUtility.SaveAsPrefabAsset(prefabRoot, destinationPrefabPath);
                if (saved == null)
                {
                    throw new InvalidOperationException(
                        $"Could not save celestial V22 {label} prefab: {destinationPrefabPath}");
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            Debug.Log(
                $"Assembled celestial V22 {label} prefab at {destinationPrefabPath}. "
                + "No scene instance or legacy CelestialHud PNG was modified.");
        }

        private static AssemblySpec LoadAndValidateSpec()
        {
            TextAsset json = AssetDatabase.LoadAssetAtPath<TextAsset>(AssemblySpecPath);
            if (json == null)
            {
                throw new InvalidOperationException($"Missing V22 assembly spec: {AssemblySpecPath}");
            }

            AssemblySpec spec = JsonUtility.FromJson<AssemblySpec>(json.text);
            if (spec == null
                || spec.version != CombatHudCelestialV2LayoutProfile.LayoutVersion
                || string.IsNullOrWhiteSpace(spec.artRoot)
                || spec.sprites == null)
            {
                throw new InvalidOperationException(
                    $"Invalid or wrong-version V22 assembly spec: {AssemblySpecPath}");
            }

            string normalizedRoot = NormalizeAssetPath(spec.artRoot).TrimEnd('/');
            if (!normalizedRoot.StartsWith("Assets/_Game/UI/CombatHud/Art/CelestialHudV2/", StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"V22 artRoot must stay under the versioned CelestialHudV2 folder: {spec.artRoot}");
            }

            spec.artRoot = normalizedRoot;
            var roles = new HashSet<string>(StringComparer.Ordinal);
            var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < spec.sprites.Length; i++)
            {
                SpriteSpec entry = spec.sprites[i];
                if (entry == null
                    || string.IsNullOrWhiteSpace(entry.role)
                    || string.IsNullOrWhiteSpace(entry.path)
                    || !roles.Add(entry.role))
                {
                    throw new InvalidOperationException(
                        $"V22 assembly spec contains an invalid or duplicate role at index {i}.");
                }

                entry.path = NormalizeAssetPath(entry.path).TrimStart('/');
                if (entry.path.Contains("..", StringComparison.Ordinal)
                    || !entry.path.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                    || !paths.Add(entry.path))
                {
                    throw new InvalidOperationException(
                        $"V22 sprite role '{entry.role}' has an invalid or duplicate path '{entry.path}'.");
                }
            }

            return spec;
        }

        private static void ValidateReferencedFiles(AssemblySpec spec)
        {
            for (int i = 0; i < spec.sprites.Length; i++)
            {
                SpriteSpec entry = spec.sprites[i];
                string assetPath = $"{spec.artRoot}/{entry.path}";
                if (entry.required && !File.Exists(ToAbsoluteProjectPath(assetPath)))
                {
                    throw new InvalidOperationException(
                        $"Missing required V22 sprite '{entry.role}': {assetPath}");
                }
            }
        }

        private static void ConfigureReferencedSpriteImporters(AssemblySpec spec)
        {
            for (int i = 0; i < spec.sprites.Length; i++)
            {
                SpriteSpec entry = spec.sprites[i];
                string assetPath = $"{spec.artRoot}/{entry.path}";
                if (!File.Exists(ToAbsoluteProjectPath(assetPath)))
                {
                    continue;
                }

                TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (importer == null)
                {
                    throw new InvalidOperationException($"Missing TextureImporter for {assetPath}");
                }

                bool changed = importer.textureType != TextureImporterType.Sprite
                    || importer.spriteImportMode != SpriteImportMode.Single
                    || importer.mipmapEnabled
                    || !importer.sRGBTexture
                    || !importer.alphaIsTransparency
                    || importer.wrapMode != TextureWrapMode.Clamp
                    || importer.filterMode != FilterMode.Bilinear
                    || importer.npotScale != TextureImporterNPOTScale.None
                    || importer.maxTextureSize != 4096;
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
                if (changed)
                {
                    importer.SaveAndReimport();
                }
            }
        }

        private static SpriteCatalog LoadSprites(AssemblySpec spec)
        {
            var result = new Dictionary<string, Sprite>(StringComparer.Ordinal);
            for (int i = 0; i < spec.sprites.Length; i++)
            {
                SpriteSpec entry = spec.sprites[i];
                string assetPath = $"{spec.artRoot}/{entry.path}";
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                if (sprite == null && entry.required)
                {
                    throw new InvalidOperationException(
                        $"Required V22 sprite did not import as a Sprite: {assetPath}");
                }

                if (sprite != null)
                {
                    result.Add(entry.role, sprite);
                }
            }

            return new SpriteCatalog(result);
        }

        private static void EnsureLayoutProfile(GameObject prefabRoot)
        {
            CombatHudCelestialV2LayoutProfile[] profiles =
                prefabRoot.GetComponents<CombatHudCelestialV2LayoutProfile>();
            if (profiles.Length > 1)
            {
                throw new InvalidOperationException("Canonical HUD has duplicate V22 layout profiles.");
            }

            if (profiles.Length == 0)
            {
                prefabRoot.AddComponent<CombatHudCelestialV2LayoutProfile>();
            }
        }

        private static void ConfigureObjectiveAndSystem(
            Transform root,
            SpriteCatalog sprites,
            Font font)
        {
            RectTransform panel = RequireRect(root, "TopLeftPanel");
            SetDesignRect(panel, CombatHudCelestialV2LayoutProfile.ObjectiveFrame, DesignAnchor.LeftTop);
            ConfigureStaticImage(panel.GetComponent<Image>(), sprites.Require("objective.frame"));

            RectTransform objective = RequireRect(root, "Objective");
            SetDesignRect(objective, CombatHudCelestialV2LayoutProfile.ObjectiveText, DesignAnchor.LeftTop);
            Text objectiveText = RequireText(objective);
            ConfigureText(
                objectiveText,
                font,
                "Break the pressure line",
                48,
                TextAnchor.MiddleLeft,
                new Color(0.96f, 0.97f, 0.98f, 1f));
            objectiveText.horizontalOverflow = HorizontalWrapMode.Wrap;
            objectiveText.verticalOverflow = VerticalWrapMode.Truncate;

            Image timerBacking = EnsureRootImage(root, "MissionTimerBacking");
            SetDesignRect(
                timerBacking.rectTransform,
                CombatHudCelestialV2LayoutProfile.MissionTimerBacking,
                DesignAnchor.RightTop);
            ClearImage(timerBacking, raycastTarget: false);
            timerBacking.color = new Color(0.02f, 0.025f, 0.035f, 0.22f);

            RectTransform timer = RequireRect(root, "Timer");
            SetDesignRect(timer, CombatHudCelestialV2LayoutProfile.MissionTimerText, DesignAnchor.RightTop);
            ConfigureText(
                RequireText(timer),
                font,
                string.Empty,
                46,
                TextAnchor.MiddleCenter,
                new Color(0.97f, 0.985f, 1f, 1f));
            timerBacking.gameObject.SetActive(false);
            timer.gameObject.SetActive(false);

            // The prefab also contains a pause-overlay SettingsButton. Hide only the
            // shallow combat-HUD shortcut and leave the overlay-owned control intact.
            Transform settings = FindShallowestTransform(root, "SettingsButton");
            if (settings != null)
            {
                settings.gameObject.SetActive(false);
            }
        }

        private static void ConfigureBoss(
            Transform root,
            SpriteCatalog sprites,
            Font font,
            Material vitalMaterial,
            Material energyMaterial)
        {
            RectTransform bossRoot = RequireRect(root, "BossHudRoot");
            RectTransform frame = RequireRect(root, "BossNameArea");
            // BossFrame is the logical group envelope. The name-tab sprite itself stays
            // compact so it cannot paint over either independent meter rail.
            SetDesignRect(frame, CombatHudCelestialV2LayoutProfile.BossName, DesignAnchor.CenterTop);
            ConfigureStaticImage(frame.GetComponent<Image>(), sprites.Require("boss.nameTab"));

            RectTransform hpTrack = RequireRect(root, "BossHpBackground");
            SetDesignRect(hpTrack, CombatHudCelestialV2LayoutProfile.BossHpTrack, DesignAnchor.CenterTop);
            ConfigureStaticImage(hpTrack.GetComponent<Image>(), sprites.Require("boss.hpTrack"));
            RectTransform hpFill = RequireRect(root, "BossHpFill");
            SetDesignRect(hpFill, CombatHudCelestialV2LayoutProfile.BossHpFill, DesignAnchor.CenterTop);
            ConfigureHorizontalFill(
                hpFill.GetComponent<Image>(),
                sprites.Require("boss.hpFill"),
                vitalMaterial);

            RectTransform costTrack = RequireRect(root, "BossCostBackground");
            SetDesignRect(costTrack, CombatHudCelestialV2LayoutProfile.BossCostTrack, DesignAnchor.CenterTop);
            ConfigureStaticImage(costTrack.GetComponent<Image>(), sprites.Require("boss.costTrack"));
            RectTransform costFill = RequireRect(root, "BossCostFill");
            SetDesignRect(costFill, CombatHudCelestialV2LayoutProfile.BossCostFill, DesignAnchor.CenterTop);
            ConfigureHorizontalFill(
                costFill.GetComponent<Image>(),
                sprites.Require("boss.costFill"),
                energyMaterial);

            Text nameText = EnsureDirectText(bossRoot, "BossNameText", font);
            SetDesignRect(
                nameText.rectTransform,
                CombatHudCelestialV2LayoutProfile.BossName,
                DesignAnchor.CenterTop);
            ConfigureText(
                nameText,
                font,
                "BOSS",
                30,
                TextAnchor.MiddleLeft,
                new Color(0.96f, 0.97f, 0.98f, 1f));

            Text hpText = EnsureDirectText(bossRoot, "BossHpText", font);
            SetDesignRect(
                hpText.rectTransform,
                CombatHudCelestialV2LayoutProfile.BossHpValue,
                DesignAnchor.CenterTop);
            ConfigureText(
                hpText,
                font,
                "2400/2400",
                26,
                TextAnchor.MiddleRight,
                new Color(0.97f, 0.97f, 0.96f, 1f));

            Text costText = EnsureDirectText(bossRoot, "BossCostText", font);
            SetDesignRect(
                costText.rectTransform,
                CombatHudCelestialV2LayoutProfile.BossCostValue,
                DesignAnchor.CenterTop);
            ConfigureText(
                costText,
                font,
                "64/100",
                23,
                TextAnchor.MiddleRight,
                new Color(0.55f, 0.93f, 1f, 1f));

            hpFill.SetSiblingIndex(0);
            hpTrack.SetSiblingIndex(1);
            costFill.SetSiblingIndex(2);
            costTrack.SetSiblingIndex(3);
            frame.SetSiblingIndex(4);
            nameText.transform.SetAsLastSibling();
            hpText.transform.SetAsLastSibling();
            costText.transform.SetAsLastSibling();

            RequireUniqueTransform(root, "BossSymbol").gameObject.SetActive(false);
            RequireUniqueTransform(root, "ActionFeedback").gameObject.SetActive(false);
        }

        private static void ConfigurePause(Transform root, SpriteCatalog sprites)
        {
            RectTransform pause = RequireRect(root, "PauseButton");
            SetDesignRect(pause, CombatHudCelestialV2LayoutProfile.PauseHit, DesignAnchor.RightTop);
            ConfigureInvisibleHitRoot(pause.GetComponent<Image>());

            Image plate = EnsureDirectImage(pause, "Plate");
            ConfigureStaticImage(plate, sprites.Require("pause.plate"), preserveAspect: true);
            SetCenteredChildRect(
                plate.rectTransform,
                CombatHudCelestialV2LayoutProfile.PauseVisual.size);
            Image glyph = EnsureDirectImage(pause, "Glyph");
            ConfigureStaticImage(glyph, sprites.Require("pause.glyph"), preserveAspect: true);
            SetCenteredChildRect(glyph.rectTransform, new Vector2(42f, 42f));
            plate.transform.SetSiblingIndex(0);
            glyph.transform.SetSiblingIndex(1);
            HideDirectChild(pause, "Label");
        }

        private static void ConfigureActions(Transform root, SpriteCatalog sprites, Font font)
        {
            ConfigureAction(
                RequireRect(root, "UltimateButton"),
                CombatHudCelestialV2LayoutProfile.WeaponSwap,
                sprites.Require("action.weaponSwapGlyph"),
                sprites,
                font);
            ConfigureAction(
                RequireRect(root, "Skill1Button"),
                CombatHudCelestialV2LayoutProfile.Skill,
                sprites.Require("action.ultimateGlyph"),
                sprites,
                font);
            ConfigureAction(
                RequireRect(root, "DodgeButton"),
                CombatHudCelestialV2LayoutProfile.Dodge,
                sprites.Require("action.dashGlyph"),
                sprites,
                font);
            ConfigureAction(
                RequireRect(root, "BasicAttackButton"),
                CombatHudCelestialV2LayoutProfile.BasicAttack,
                sprites.Require("action.rangedGlyph"),
                sprites,
                font);
        }

        private static void ConfigureAction(
            RectTransform button,
            Rect designRect,
            Sprite glyphSprite,
            SpriteCatalog sprites,
            Font font)
        {
            SetDesignRect(button, designRect, DesignAnchor.RightBottom);
            ConfigureInvisibleHitRoot(button.GetComponent<Image>());

            Image plate = EnsureDirectImage(button, "Plate");
            ConfigureStaticImage(plate, sprites.Require("action.plate"), preserveAspect: true);
            StretchToParent(plate.rectTransform);

            Image glyph = EnsureDirectImage(button, "Glyph");
            ConfigureStaticImage(glyph, glyphSprite, preserveAspect: true);
            float glyphSize = Mathf.Min(designRect.width, designRect.height)
                * (button.name == "BasicAttackButton" ? 0.61f : 0.56f);
            SetCenteredChildRect(glyph.rectTransform, new Vector2(glyphSize, glyphSize));

            Image cooldown = TakeOrEnsureDirectImage(button, "Cooldown", "CooldownFill");
            ConfigureRadialFill(cooldown, sprites.Require("action.cooldownDisc"), Color.white);
            StretchToParent(cooldown.rectTransform);

            Image readyArc = EnsureDirectImage(button, "ReadyArc");
            ConfigureRadialFill(readyArc, sprites.Require("action.readyArc"), Color.white);
            StretchToParent(readyArc.rectTransform);

            Text cooldownText = RequireDescendantText(button, "CooldownText");
            ConfigureText(
                cooldownText,
                font,
                string.Empty,
                30,
                TextAnchor.MiddleCenter,
                new Color(0.98f, 0.99f, 1f, 1f));
            StretchToParent(cooldownText.rectTransform);
            cooldownText.gameObject.SetActive(true);

            HideDirectChild(button, "Label");
            DisableNamedDescendant(button, "DodgeCooldownRing");
            DisableNamedDescendant(button, "ActionReadyGlow");
            DisableNamedDescendant(button, "ReadyGlow");

            plate.transform.SetSiblingIndex(0);
            glyph.transform.SetSiblingIndex(1);
            cooldown.transform.SetSiblingIndex(2);
            readyArc.transform.SetSiblingIndex(3);
            cooldownText.transform.SetAsLastSibling();
        }

        private static void ConfigureSummons(Transform root, SpriteCatalog sprites, Font font)
        {
            RectTransform rail = EnsureFullStretchGroup(root, "SummonRailV22Root", new Vector2(1f, 1f));
            ConfigureSummon(
                MoveIntoGroup(RequireRect(root, "SummonSlot1Button"), rail),
                CombatHudCelestialV2LayoutProfile.SummonSlot1,
                sprites.Require("summon.frame1"),
                sprites.Require("summon.portrait1"),
                sprites,
                font);
            ConfigureSummon(
                MoveIntoGroup(RequireRect(root, "SummonSlot2Button"), rail),
                CombatHudCelestialV2LayoutProfile.SummonSlot2,
                sprites.Require("summon.frame2"),
                sprites.Require("summon.portrait2"),
                sprites,
                font);
            ConfigureSummon(
                MoveIntoGroup(RequireRect(root, "SummonSlot3Button"), rail),
                CombatHudCelestialV2LayoutProfile.SummonSlot3,
                sprites.Require("summon.frame3"),
                sprites.Require("summon.portrait3"),
                sprites,
                font);
        }

        private static void ConfigureSummon(
            RectTransform slot,
            Rect designRect,
            Sprite frameSprite,
            Sprite portraitSprite,
            SpriteCatalog sprites,
            Font font)
        {
            SetDesignRect(slot, designRect, DesignAnchor.RightTop);
            ConfigureInvisibleHitRoot(slot.GetComponent<Image>());

            Image maskImage = EnsureDirectImage(slot, "PortraitMask");
            ConfigureStaticImage(maskImage, sprites.Require("summon.mask"), preserveAspect: true);
            float maskSize = designRect.width * 0.76f;
            SetCenteredChildRect(maskImage.rectTransform, new Vector2(maskSize, maskSize));
            maskImage.rectTransform.anchoredPosition = new Vector2(0f, designRect.height * 0.075f);
            Mask mask = maskImage.GetComponent<Mask>();
            if (mask == null)
            {
                mask = maskImage.gameObject.AddComponent<Mask>();
            }

            mask.enabled = true;
            mask.showMaskGraphic = false;

            Image portrait = TakeOrEnsureChildImage(maskImage.rectTransform, "Icon", slot);
            ConfigureStaticImage(portrait, portraitSprite, preserveAspect: true);
            portrait.maskable = true;
            SetCenteredChildRect(portrait.rectTransform, new Vector2(maskSize * 1.08f, maskSize * 1.08f));

            Image disabledPortrait = TakeOrEnsureChildImage(maskImage.rectTransform, "IconDisabled", slot);
            ConfigureStaticImage(disabledPortrait, portraitSprite, preserveAspect: true);
            disabledPortrait.maskable = true;
            disabledPortrait.color = new Color(0.34f, 0.37f, 0.41f, 0.96f);
            SetCenteredChildRect(
                disabledPortrait.rectTransform,
                new Vector2(maskSize * 1.08f, maskSize * 1.08f));

            Image frame = TakeOrEnsureDirectImage(slot, "Frame", "FrameOverlay");
            ConfigureStaticImage(frame, frameSprite, preserveAspect: true);
            StretchToParent(frame.rectTransform);

            Image stateArc = TakeOrEnsureDirectImage(slot, "StateArc", "CooldownFill");
            ConfigureRadialFill(stateArc, sprites.Require("summon.stateArc"), Color.white);
            SetCenteredChildRect(stateArc.rectTransform, new Vector2(designRect.width, designRect.height));

            Image costTab = EnsureDirectImage(slot, "CostTab");
            ConfigureStaticImage(costTab, sprites.Require("summon.costTab"), preserveAspect: true);
            SetBottomLeftChildRect(
                costTab.rectTransform,
                new Rect(
                    designRect.width * 0.055f,
                    designRect.height * 0.035f,
                    designRect.width * 0.42f,
                    designRect.height * 0.265f));

            Text costText = TakeOrEnsureDirectText(slot, "CostText", "Label", font);
            SetBottomLeftChildRect(
                costText.rectTransform,
                new Rect(
                    designRect.width * 0.07f,
                    designRect.height * 0.045f,
                    designRect.width * 0.27f,
                    designRect.height * 0.22f));
            ConfigureText(
                costText,
                font,
                slot.name.Replace("SummonSlot", "S").Replace("Button", string.Empty),
                36,
                TextAnchor.MiddleCenter,
                new Color(0.97f, 0.98f, 1f, 1f));

            Text unitText = EnsureDirectText(slot, "CostUnitText", font);
            SetBottomLeftChildRect(
                unitText.rectTransform,
                new Rect(
                    designRect.width * 0.32f,
                    designRect.height * 0.055f,
                    designRect.width * 0.13f,
                    designRect.height * 0.18f));
            ConfigureText(
                unitText,
                font,
                "EN",
                18,
                TextAnchor.LowerLeft,
                new Color(0.64f, 0.91f, 0.98f, 1f));

            Text statusText = TakeOrEnsureDirectText(slot, "StatusText", "State", font);
            SetBottomLeftChildRect(statusText.rectTransform, GetChildTopLeftRect(costText.rectTransform));
            ConfigureText(
                statusText,
                font,
                string.Empty,
                32,
                TextAnchor.MiddleCenter,
                new Color(0.97f, 0.98f, 1f, 1f));

            DisableNamedDescendant(slot, "ReadyGlow");
            DisableNamedDescendant(slot, "ReadyRing");
            DisableNamedDescendant(slot, "ReadySparkRing");
            maskImage.transform.SetSiblingIndex(0);
            frame.transform.SetSiblingIndex(1);
            stateArc.transform.SetSiblingIndex(2);
            costTab.transform.SetSiblingIndex(3);
            costText.transform.SetAsLastSibling();
            unitText.transform.SetAsLastSibling();
            statusText.transform.SetAsLastSibling();
        }

        private static void ConfigureJoystick(Transform root, SpriteCatalog sprites)
        {
            RectTransform ring = RequireRect(root, "MoveJoystickRing");
            SetDesignRect(ring, CombatHudCelestialV2LayoutProfile.JoystickVisual, DesignAnchor.LeftBottom);
            ConfigureStaticImage(ring.GetComponent<Image>(), sprites.Require("joystick.base"), preserveAspect: true);
            ring.GetComponent<Image>().raycastTarget = false;

            Image activationHit = EnsureDirectImage(ring, "JoystickActivationHit");
            ClearImage(activationHit, raycastTarget: true);
            SetCenteredChildRect(
                activationHit.rectTransform,
                CombatHudCelestialV2LayoutProfile.JoystickActivation.size);
            activationHit.transform.SetAsLastSibling();

            RectTransform knob = RequireRect(root, "MoveJoystickKnob");
            SetDesignRect(knob, CombatHudCelestialV2LayoutProfile.JoystickKnob, DesignAnchor.LeftBottom);
            ConfigureStaticImage(knob.GetComponent<Image>(), sprites.Require("joystick.knob"), preserveAspect: true);
        }

        private static void ConfigurePlayer(
            Transform root,
            SpriteCatalog sprites,
            Font font,
            Material vitalMaterial,
            Material energyMaterial)
        {
            RectTransform group = EnsureFullStretchGroup(root, "PlayerHudV22Root", new Vector2(0.5f, 0.5f));
            RectTransform portraitFrame = MoveIntoGroup(
                EnsureRootImage(root, "PlayerPortraitFrame").rectTransform,
                group);
            RectTransform hpTrack = MoveIntoGroup(RequireRect(root, "HealthBar_Track"), group);
            RectTransform hpFill = MoveIntoGroup(RequireRect(root, "HealthBar"), group);
            RectTransform hpText = MoveIntoGroup(RequireRect(root, "HealthText"), group);
            RectTransform enTrack = MoveIntoGroup(RequireRect(root, "ResourceBar_Track"), group);
            RectTransform enFill = MoveIntoGroup(RequireRect(root, "ResourceBar"), group);
            RectTransform enText = MoveIntoGroup(RequireRect(root, "ResourceText"), group);
            RectTransform inputMode = MoveIntoGroup(RequireRect(root, "InputMode"), group);
            RectTransform ammoText = MoveIntoGroup(RequireRect(root, "AmmoText"), group);
            RectTransform ammoCell = MoveIntoGroup(EnsureRootImage(root, "PlayerAmmoChip").rectTransform, group);
            RectTransform modeCell = MoveIntoGroup(EnsureRootImage(root, "PlayerModeCell").rectTransform, group);

            SetDesignRect(portraitFrame, CombatHudCelestialV2LayoutProfile.PlayerPortrait, DesignAnchor.CenterBottom);
            // The root is layout-only. Rendering the frame here would place it behind the
            // clipped portrait; a dedicated overlay child owns the visible bezel.
            ClearImage(portraitFrame.GetComponent<Image>(), raycastTarget: false);
            Image portraitMask = EnsureDirectImage(portraitFrame, "PortraitMask");
            ConfigureStaticImage(
                portraitMask,
                sprites.Require("player.portraitMask"),
                preserveAspect: true);
            SetCenteredChildRect(portraitMask.rectTransform, new Vector2(116f, 116f));
            Mask mask = portraitMask.GetComponent<Mask>();
            if (mask == null)
            {
                mask = portraitMask.gameObject.AddComponent<Mask>();
            }

            mask.enabled = true;
            mask.showMaskGraphic = false;
            Image portrait = TakeOrEnsureChildImage(portraitMask.rectTransform, "PlayerPortrait", portraitFrame);
            ConfigureStaticImage(portrait, sprites.Require("player.portrait"), preserveAspect: true);
            portrait.maskable = true;
            SetCenteredChildRect(portrait.rectTransform, new Vector2(116f, 116f));
            portraitMask.transform.SetAsFirstSibling();
            Image portraitOverlay = EnsureDirectImage(portraitFrame, "FrameOverlay");
            ConfigureStaticImage(
                portraitOverlay,
                sprites.Require("player.portraitFrame"),
                preserveAspect: true);
            StretchToParent(portraitOverlay.rectTransform);
            portraitOverlay.transform.SetAsLastSibling();

            SetDesignRect(hpText, CombatHudCelestialV2LayoutProfile.PlayerHpText, DesignAnchor.CenterBottom);
            ConfigureText(
                RequireText(hpText),
                font,
                "2400/2400",
                28,
                TextAnchor.MiddleLeft,
                new Color(0.98f, 0.94f, 0.82f, 1f));
            SetDesignRect(hpTrack, CombatHudCelestialV2LayoutProfile.PlayerHpTrack, DesignAnchor.CenterBottom);
            ConfigureStaticImage(hpTrack.GetComponent<Image>(), sprites.Require("player.hpTrack"));
            SetDesignRect(hpFill, CombatHudCelestialV2LayoutProfile.PlayerHpFill, DesignAnchor.CenterBottom);
            ConfigureHorizontalFill(hpFill.GetComponent<Image>(), sprites.Require("player.hpFill"), vitalMaterial);

            SetDesignRect(enTrack, CombatHudCelestialV2LayoutProfile.PlayerEnTrack, DesignAnchor.CenterBottom);
            ConfigureStaticImage(enTrack.GetComponent<Image>(), sprites.Require("player.enTrack"));
            SetDesignRect(enFill, CombatHudCelestialV2LayoutProfile.PlayerEnFill, DesignAnchor.CenterBottom);
            ConfigureHorizontalFill(enFill.GetComponent<Image>(), sprites.Require("player.enFill"), energyMaterial);
            SetDesignRect(enText, CombatHudCelestialV2LayoutProfile.PlayerEnText, DesignAnchor.CenterBottom);
            ConfigureText(
                RequireText(enText),
                font,
                "64/100",
                23,
                TextAnchor.MiddleRight,
                new Color(0.55f, 0.93f, 1f, 1f));

            SetDesignRect(modeCell, CombatHudCelestialV2LayoutProfile.PlayerMode, DesignAnchor.CenterBottom);
            ConfigureStaticImage(modeCell.GetComponent<Image>(), sprites.Require("player.ammoPlate"));
            Image modeGlyph = EnsureDirectImage(modeCell, "ModeGlyph");
            ConfigureStaticImage(modeGlyph, sprites.Require("player.modeGlyph"), preserveAspect: true);
            SetCenteredChildRect(modeGlyph.rectTransform, new Vector2(48f, 48f));
            modeGlyph.rectTransform.anchoredPosition = new Vector2(-76f, 0f);
            SetDesignRect(inputMode, CombatHudCelestialV2LayoutProfile.PlayerMode, DesignAnchor.CenterBottom);
            ConfigureText(
                RequireText(inputMode),
                font,
                "RANGED",
                24,
                TextAnchor.MiddleCenter,
                new Color(0.90f, 0.98f, 1f, 1f));
            inputMode.SetAsLastSibling();

            SetDesignRect(ammoCell, CombatHudCelestialV2LayoutProfile.PlayerAmmo, DesignAnchor.CenterBottom);
            ConfigureStaticImage(ammoCell.GetComponent<Image>(), sprites.Require("player.ammoPlate"));
            Image bulletGlyph = EnsureDirectImage(ammoCell, "BulletGlyph");
            ConfigureStaticImage(bulletGlyph, sprites.Require("player.bulletGlyph"), preserveAspect: true);
            SetCenteredChildRect(bulletGlyph.rectTransform, new Vector2(58f, 58f));
            bulletGlyph.rectTransform.anchoredPosition = new Vector2(-82f, 0f);
            SetDesignRect(ammoText, CombatHudCelestialV2LayoutProfile.PlayerAmmo, DesignAnchor.CenterBottom);
            ConfigureText(
                RequireText(ammoText),
                font,
                "24/24",
                30,
                TextAnchor.MiddleRight,
                new Color(0.98f, 0.91f, 0.70f, 1f));

            // Meter fills are clipped by runtime fillAmount, then the rail sprites paint
            // their crisp rims above. Readouts/portrait/cells remain above both meters.
            hpFill.SetSiblingIndex(0);
            hpTrack.SetSiblingIndex(1);
            enFill.SetSiblingIndex(2);
            enTrack.SetSiblingIndex(3);
            modeCell.SetSiblingIndex(4);
            ammoCell.SetSiblingIndex(5);
            portraitFrame.SetSiblingIndex(6);
            hpText.SetSiblingIndex(7);
            enText.SetSiblingIndex(8);
            inputMode.SetSiblingIndex(9);
            ammoText.SetSiblingIndex(10);

            RequireUniqueTransform(root, "PlayerSymbol").gameObject.SetActive(false);
            RequireUniqueTransform(root, "PlayerNameArea").gameObject.SetActive(false);
            RequireUniqueTransform(root, "PlayerHpAmountArea").gameObject.SetActive(false);
            RequireUniqueTransform(root, "PlayerMpAmountArea").gameObject.SetActive(false);
        }

        private static void ConfigureReticle(Transform root, SpriteCatalog sprites)
        {
            Image rootImage = EnsureRootImage(root, "CenterAimReticle");
            RectTransform reticle = rootImage.rectTransform;
            SetDesignRect(reticle, CombatHudCelestialV2LayoutProfile.Reticle, DesignAnchor.CenterScreen);
            ClearImage(rootImage, raycastTarget: false);

            Image dot = EnsureDirectImage(reticle, "Dot");
            ConfigureStaticImage(dot, sprites.Require("reticle.dot"), preserveAspect: true);
            SetCenteredChildRect(dot.rectTransform, CombatHudCelestialV2LayoutProfile.Reticle.size);

            ConfigureReticleNeedle(reticle, "NeedleTop", sprites.Require("reticle.needle"), 0f);
            ConfigureReticleNeedle(reticle, "NeedleRight", sprites.Require("reticle.needle"), -90f);
            ConfigureReticleNeedle(reticle, "NeedleBottom", sprites.Require("reticle.needle"), 180f);
            ConfigureReticleNeedle(reticle, "NeedleLeft", sprites.Require("reticle.needle"), 90f);

            dot.transform.SetAsLastSibling();
            reticle.SetSiblingIndex(RequireRect(root, "BasicAttackButton").GetSiblingIndex());
        }

        private static Image ConfigureReticleNeedle(
            RectTransform parent,
            string name,
            Sprite sprite,
            float rotationDegrees)
        {
            Image image = EnsureDirectImage(parent, name);
            ConfigureStaticImage(image, sprite, preserveAspect: true);
            // Needle/dot files are authored as full 192x192 pivot canvases. Preserve that
            // common canvas and rotate instances around the exact reticle center.
            SetCenteredChildRect(image.rectTransform, CombatHudCelestialV2LayoutProfile.Reticle.size);
            image.rectTransform.localRotation = Quaternion.Euler(0f, 0f, rotationDegrees);
            return image;
        }

        private static void ConfigurePresenterBindings(GameObject prefabRoot)
        {
            CombatHudPresenter presenter = prefabRoot.GetComponent<CombatHudPresenter>();
            if (presenter == null)
            {
                throw new InvalidOperationException("Canonical HUD root is missing CombatHudPresenter.");
            }

            var serialized = new SerializedObject(presenter);
            SetObjectReference(serialized, "bossHealthText", RequireUniqueComponent<Text>(prefabRoot.transform, "BossHpText"));
            SetObjectReference(serialized, "bossResourceText", RequireUniqueComponent<Text>(prefabRoot.transform, "BossCostText"));
            SetObjectReference(serialized, "aimReticleRoot", RequireRect(prefabRoot.transform, "CenterAimReticle"));

            SerializedProperty segments = serialized.FindProperty("aimReticleSegments");
            string[] segmentNames = { "Dot", "NeedleTop", "NeedleRight", "NeedleBottom", "NeedleLeft" };
            segments.arraySize = segmentNames.Length;
            for (int i = 0; i < segmentNames.Length; i++)
            {
                segments.GetArrayElementAtIndex(i).objectReferenceValue =
                    RequireUniqueComponent<Image>(prefabRoot.transform, segmentNames[i]);
            }

            BindAction(serialized.FindProperty("actionSlots"), 100, prefabRoot.transform, "BasicAttackButton");
            BindAction(serialized.FindProperty("actionSlots"), 110, prefabRoot.transform, "DodgeButton");
            BindAction(serialized.FindProperty("actionSlots"), 120, prefabRoot.transform, "Skill1Button");
            BindAction(serialized.FindProperty("actionSlots"), 130, prefabRoot.transform, "UltimateButton");
            BindSummon(serialized.FindProperty("summonSlots"), 200, prefabRoot.transform, "SummonSlot1Button");
            BindSummon(serialized.FindProperty("summonSlots"), 210, prefabRoot.transform, "SummonSlot2Button");
            BindSummon(serialized.FindProperty("summonSlots"), 220, prefabRoot.transform, "SummonSlot3Button");
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(presenter);
        }

        private static void BindAction(
            SerializedProperty bindings,
            int actionId,
            Transform root,
            string buttonName)
        {
            SerializedProperty binding = RequireBinding(bindings, actionId);
            RectTransform button = RequireRect(root, buttonName);
            binding.FindPropertyRelative("cooldownFill").objectReferenceValue =
                RequireDescendantImage(button, "Cooldown");
            bool priorityReadyArc = actionId == 110 || actionId == 120;
            Image readyArc = RequireDescendantImage(button, "ReadyArc");
            readyArc.gameObject.SetActive(priorityReadyArc);
            binding.FindPropertyRelative("readyProgressFill").objectReferenceValue =
                priorityReadyArc ? readyArc : null;
            // V22 intentionally omits the legacy scale-breathing glow. The ready arc is
            // sufficient and stays inside the reviewed static silhouette.
            binding.FindPropertyRelative("readyGlowImage").objectReferenceValue = null;
        }

        private static void BindSummon(
            SerializedProperty bindings,
            int actionId,
            Transform root,
            string buttonName)
        {
            SerializedProperty binding = RequireBinding(bindings, actionId);
            RectTransform button = RequireRect(root, buttonName);
            binding.FindPropertyRelative("labelText").objectReferenceValue =
                RequireDescendantText(button, "CostText");
            binding.FindPropertyRelative("stateText").objectReferenceValue =
                RequireDescendantText(button, "StatusText");
            binding.FindPropertyRelative("cooldownFill").objectReferenceValue =
                RequireDescendantImage(button, "StateArc");
            binding.FindPropertyRelative("iconImage").objectReferenceValue =
                RequireDescendantImage(button, "Icon");
            binding.FindPropertyRelative("unavailableIconImage").objectReferenceValue =
                RequireDescendantImage(button, "IconDisabled");
            // No legacy glow, breathing ring, or rotating spark in the compact V22 rail.
            binding.FindPropertyRelative("readyGlowImage").objectReferenceValue = null;
            binding.FindPropertyRelative("readyRingImage").objectReferenceValue = null;
            binding.FindPropertyRelative("readySparkImage").objectReferenceValue = null;
        }

        private static SerializedProperty RequireBinding(SerializedProperty bindings, int actionId)
        {
            if (bindings == null || !bindings.isArray)
            {
                throw new InvalidOperationException("CombatHudPresenter binding array is missing.");
            }

            for (int i = 0; i < bindings.arraySize; i++)
            {
                SerializedProperty candidate = bindings.GetArrayElementAtIndex(i);
                if (candidate.FindPropertyRelative("actionId").intValue == actionId)
                {
                    return candidate;
                }
            }

            throw new InvalidOperationException($"Missing HUD presenter binding for action {actionId}.");
        }

        private static void SetObjectReference(
            SerializedObject serialized,
            string propertyName,
            UnityEngine.Object value)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property == null)
            {
                throw new InvalidOperationException(
                    $"CombatHudPresenter is missing serialized property '{propertyName}'.");
            }

            property.objectReferenceValue = value;
        }

        private static void ValidateRaycastOwnership(Transform root)
        {
            string[] rootHitNames =
            {
                "PauseButton",
                "UltimateButton",
                "Skill1Button",
                "DodgeButton",
                "BasicAttackButton",
                "SummonSlot1Button",
                "SummonSlot2Button",
                "SummonSlot3Button"
            };
            for (int i = 0; i < rootHitNames.Length; i++)
            {
                RectTransform hitRoot = RequireRect(root, rootHitNames[i]);
                Image rootImage = hitRoot.GetComponent<Image>();
                if (rootImage == null || !rootImage.raycastTarget)
                {
                    throw new InvalidOperationException($"{rootHitNames[i]} must own its root hit graphic.");
                }

                Image[] children = hitRoot.GetComponentsInChildren<Image>(includeInactive: false);
                for (int childIndex = 0; childIndex < children.Length; childIndex++)
                {
                    if (children[childIndex].transform != hitRoot
                        && children[childIndex].raycastTarget)
                    {
                        throw new InvalidOperationException(
                            $"Decorative child {GetPath(children[childIndex].transform)} consumes taps.");
                    }
                }
            }

            Image joystickHit = RequireUniqueComponent<Image>(root, "JoystickActivationHit");
            if (!joystickHit.raycastTarget)
            {
                throw new InvalidOperationException("JoystickActivationHit must receive pointer acquisition.");
            }
        }

        private static void ValidateCanonicalBindingObjects(Transform root)
        {
            string[] names =
            {
                "TopLeftPanel", "Timer", "Objective", "ActionFeedback", "PauseButton",
                "BossHudRoot", "BossSymbol", "BossNameArea", "BossHpBackground",
                "BossHpFill", "BossCostBackground", "BossCostFill", "SummonSlot1Button",
                "SummonSlot2Button", "SummonSlot3Button", "UltimateButton", "Skill1Button",
                "DodgeButton", "BasicAttackButton", "MoveJoystickRing", "MoveJoystickKnob",
                "HealthBar_Track", "HealthBar", "HealthText", "ResourceBar_Track",
                "ResourceBar", "ResourceText", "AmmoText", "InputMode", "PlayerSymbol",
                "PlayerNameArea", "PlayerHpAmountArea", "PlayerMpAmountArea"
            };
            for (int i = 0; i < names.Length; i++)
            {
                RequireUniqueTransform(root, names[i]);
            }
        }

        private static RectTransform EnsureFullStretchGroup(
            Transform root,
            string name,
            Vector2 pivot)
        {
            Transform existing = FindUniqueTransform(root, name, required: false);
            if (existing == null)
            {
                var gameObject = new GameObject(name, typeof(RectTransform));
                gameObject.layer = root.gameObject.layer;
                gameObject.transform.SetParent(root, worldPositionStays: false);
                existing = gameObject.transform;
            }

            RectTransform rect = existing as RectTransform;
            if (rect == null)
            {
                throw new InvalidOperationException($"Managed V22 group '{name}' is not a RectTransform.");
            }

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = pivot;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
            return rect;
        }

        private static RectTransform MoveIntoGroup(RectTransform child, RectTransform group)
        {
            if (child.parent != group)
            {
                child.SetParent(group, worldPositionStays: false);
            }

            return child;
        }

        private static Image TakeOrEnsureDirectImage(
            RectTransform parent,
            string targetName,
            string priorName)
        {
            Transform target = parent.Find(targetName);
            if (target == null)
            {
                target = FindUniqueTransform(parent, priorName, required: false);
                if (target != null)
                {
                    target.name = targetName;
                    target.SetParent(parent, worldPositionStays: false);
                }
            }

            return target != null ? RequireImage(target) : EnsureDirectImage(parent, targetName);
        }

        private static Text TakeOrEnsureDirectText(
            RectTransform parent,
            string targetName,
            string priorName,
            Font font)
        {
            Transform target = parent.Find(targetName);
            if (target == null)
            {
                target = FindUniqueTransform(parent, priorName, required: false);
                if (target != null)
                {
                    target.name = targetName;
                    target.SetParent(parent, worldPositionStays: false);
                }
            }

            if (target != null)
            {
                return RequireText(target);
            }

            return EnsureDirectText(parent, targetName, font);
        }

        private static Image TakeOrEnsureChildImage(
            RectTransform targetParent,
            string name,
            RectTransform searchRoot)
        {
            Transform existing = targetParent.Find(name)
                ?? FindUniqueTransform(searchRoot, name, required: false);
            if (existing == null)
            {
                return EnsureDirectImage(targetParent, name);
            }

            if (existing.parent != targetParent)
            {
                existing.SetParent(targetParent, worldPositionStays: false);
            }

            return RequireImage(existing);
        }

        private static Image EnsureRootImage(Transform root, string name)
        {
            Transform existing = FindUniqueTransform(root, name, required: false);
            if (existing == null)
            {
                var gameObject = new GameObject(
                    name,
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image));
                gameObject.layer = root.gameObject.layer;
                gameObject.transform.SetParent(root, worldPositionStays: false);
                existing = gameObject.transform;
            }

            return RequireImage(existing);
        }

        private static Image EnsureDirectImage(RectTransform parent, string name)
        {
            Transform existing = parent.Find(name);
            if (existing == null)
            {
                var gameObject = new GameObject(
                    name,
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image));
                gameObject.layer = parent.gameObject.layer;
                gameObject.transform.SetParent(parent, worldPositionStays: false);
                existing = gameObject.transform;
            }

            return RequireImage(existing);
        }

        private static Text EnsureDirectText(RectTransform parent, string name, Font font)
        {
            Transform existing = parent.Find(name);
            if (existing == null)
            {
                var gameObject = new GameObject(
                    name,
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Text));
                gameObject.layer = parent.gameObject.layer;
                gameObject.transform.SetParent(parent, worldPositionStays: false);
                existing = gameObject.transform;
            }

            Text text = RequireText(existing);
            text.font = font;
            return text;
        }

        private static void ConfigureInvisibleHitRoot(Image image)
        {
            if (image == null)
            {
                throw new InvalidOperationException("Managed input root is missing its Image target graphic.");
            }

            image.sprite = null;
            image.material = null;
            image.color = Color.clear;
            image.raycastTarget = true;
            image.type = Image.Type.Simple;
            image.preserveAspect = false;
            image.alphaHitTestMinimumThreshold = 0f;
        }

        private static void ConfigureStaticImage(Image image, Sprite sprite, bool preserveAspect = false)
        {
            if (image == null || sprite == null)
            {
                throw new InvalidOperationException("Managed V22 image or sprite is null.");
            }

            image.sprite = sprite;
            image.material = null;
            image.color = Color.white;
            image.raycastTarget = false;
            image.type = Image.Type.Simple;
            image.preserveAspect = preserveAspect;
            image.fillAmount = 1f;
        }

        private static void ConfigureHorizontalFill(Image image, Sprite sprite, Material material)
        {
            ConfigureStaticImage(image, sprite);
            image.material = material;
            image.type = Image.Type.Filled;
            image.fillMethod = Image.FillMethod.Horizontal;
            image.fillOrigin = (int)Image.OriginHorizontal.Left;
            image.fillClockwise = true;
            image.fillAmount = 1f;
        }

        private static void ConfigureRadialFill(Image image, Sprite sprite, Color color)
        {
            ConfigureStaticImage(image, sprite, preserveAspect: true);
            image.color = color;
            image.type = Image.Type.Filled;
            image.fillMethod = Image.FillMethod.Radial360;
            image.fillOrigin = (int)Image.Origin360.Top;
            image.fillClockwise = true;
            image.fillAmount = 1f;
        }

        private static void ConfigureText(
            Text text,
            Font font,
            string value,
            int fontSize,
            TextAnchor alignment,
            Color color)
        {
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
            text.text = value;
        }

        private static void ClearImage(Image image, bool raycastTarget)
        {
            image.sprite = null;
            image.material = null;
            image.color = Color.clear;
            image.raycastTarget = raycastTarget;
            image.type = Image.Type.Simple;
            image.preserveAspect = false;
        }

        private static void DisableNamedDescendant(RectTransform parent, string name)
        {
            Transform found = FindUniqueTransform(parent, name, required: false);
            if (found != null)
            {
                found.gameObject.SetActive(false);
            }
        }

        private static void HideDirectChild(RectTransform parent, string name)
        {
            Transform child = parent.Find(name);
            if (child != null)
            {
                child.gameObject.SetActive(false);
            }
        }

        private static void SetDesignRect(RectTransform rect, Rect designRect, DesignAnchor anchor)
        {
            float rightInset = CombatHudCelestialV2LayoutProfile.DesignWidth - designRect.xMax;
            float bottomInset = CombatHudCelestialV2LayoutProfile.DesignHeight - designRect.yMax;
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
            switch (anchor)
            {
                case DesignAnchor.LeftTop:
                    rect.anchorMin = new Vector2(0f, 1f);
                    rect.anchorMax = new Vector2(0f, 1f);
                    rect.pivot = new Vector2(0f, 1f);
                    rect.anchoredPosition = new Vector2(designRect.xMin, -designRect.yMin);
                    break;
                case DesignAnchor.LeftBottom:
                    rect.anchorMin = Vector2.zero;
                    rect.anchorMax = Vector2.zero;
                    rect.pivot = Vector2.zero;
                    rect.anchoredPosition = new Vector2(designRect.xMin, bottomInset);
                    break;
                case DesignAnchor.RightTop:
                    rect.anchorMin = Vector2.one;
                    rect.anchorMax = Vector2.one;
                    rect.pivot = Vector2.one;
                    rect.anchoredPosition = new Vector2(-rightInset, -designRect.yMin);
                    break;
                case DesignAnchor.RightBottom:
                    rect.anchorMin = new Vector2(1f, 0f);
                    rect.anchorMax = new Vector2(1f, 0f);
                    rect.pivot = new Vector2(1f, 0f);
                    rect.anchoredPosition = new Vector2(-rightInset, bottomInset);
                    break;
                case DesignAnchor.CenterBottom:
                    rect.anchorMin = new Vector2(0.5f, 0f);
                    rect.anchorMax = new Vector2(0.5f, 0f);
                    rect.pivot = new Vector2(0.5f, 0f);
                    rect.anchoredPosition = new Vector2(
                        designRect.center.x - CombatHudCelestialV2LayoutProfile.DesignWidth * 0.5f,
                        bottomInset);
                    break;
                case DesignAnchor.CenterTop:
                case DesignAnchor.CenterScreen:
                    rect.anchorMin = new Vector2(0.5f, 0.5f);
                    rect.anchorMax = new Vector2(0.5f, 0.5f);
                    rect.pivot = new Vector2(0.5f, 0.5f);
                    rect.anchoredPosition = new Vector2(
                        designRect.center.x - CombatHudCelestialV2LayoutProfile.DesignWidth * 0.5f,
                        CombatHudCelestialV2LayoutProfile.DesignHeight * 0.5f - designRect.center.y);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(anchor), anchor, null);
            }

            rect.sizeDelta = designRect.size;
        }

        private static void SetCenteredChildRect(RectTransform rect, Vector2 size)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = size;
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
        }

        private static void SetBottomLeftChildRect(RectTransform rect, Rect localTopLeftRect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.zero;
            rect.pivot = Vector2.zero;
            rect.anchoredPosition = new Vector2(localTopLeftRect.x, localTopLeftRect.y);
            rect.sizeDelta = localTopLeftRect.size;
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
        }

        private static Rect GetChildTopLeftRect(RectTransform rect)
        {
            return new Rect(rect.anchoredPosition, rect.sizeDelta);
        }

        private static void StretchToParent(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
        }

        private static RectTransform RequireRect(Transform root, string name)
        {
            Transform found = RequireUniqueTransform(root, name);
            RectTransform rect = found as RectTransform;
            if (rect == null)
            {
                throw new InvalidOperationException($"Managed HUD object '{name}' is not a RectTransform.");
            }

            return rect;
        }

        private static Image RequireDescendantImage(RectTransform root, string name)
        {
            return RequireImage(RequireUniqueTransform(root, name));
        }

        private static Text RequireDescendantText(RectTransform root, string name)
        {
            return RequireText(RequireUniqueTransform(root, name));
        }

        private static T RequireUniqueComponent<T>(Transform root, string name) where T : Component
        {
            Transform found = RequireUniqueTransform(root, name);
            T component = found.GetComponent<T>();
            if (component == null)
            {
                throw new InvalidOperationException($"HUD object '{name}' is missing {typeof(T).Name}.");
            }

            return component;
        }

        private static Image RequireImage(Transform transform)
        {
            Image image = transform.GetComponent<Image>();
            if (image == null)
            {
                throw new InvalidOperationException($"HUD object '{GetPath(transform)}' is missing Image.");
            }

            return image;
        }

        private static Text RequireText(Transform transform)
        {
            Text text = transform.GetComponent<Text>();
            if (text == null)
            {
                throw new InvalidOperationException($"HUD object '{GetPath(transform)}' is missing Text.");
            }

            return text;
        }

        private static Transform RequireUniqueTransform(Transform root, string name)
        {
            return FindUniqueTransform(root, name, required: true);
        }

        private static Transform FindUniqueTransform(Transform root, string name, bool required)
        {
            Transform match = null;
            int count = 0;
            Transform[] transforms = root.GetComponentsInChildren<Transform>(includeInactive: true);
            for (int i = 0; i < transforms.Length; i++)
            {
                if (!string.Equals(transforms[i].name, name, StringComparison.Ordinal))
                {
                    continue;
                }

                match = transforms[i];
                count++;
            }

            if (count > 1 || (required && count != 1))
            {
                throw new InvalidOperationException(
                    $"Expected {(required ? "one" : "at most one")} '{name}' under {root.name}, found {count}.");
            }

            return match;
        }

        private static Transform FindShallowestTransform(Transform root, string name)
        {
            return root.GetComponentsInChildren<Transform>(includeInactive: true)
                .Where(candidate => string.Equals(candidate.name, name, StringComparison.Ordinal))
                .OrderBy(candidate => GetDepthFrom(candidate, root))
                .FirstOrDefault();
        }

        private static int GetDepthFrom(Transform transform, Transform root)
        {
            int depth = 0;
            Transform current = transform;
            while (current != null && current != root)
            {
                depth++;
                current = current.parent;
            }

            return current == root ? depth : int.MaxValue;
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

        private static string NormalizeAssetPath(string path)
        {
            return path.Replace('\\', '/').Trim();
        }

        private static string ToAbsoluteProjectPath(string assetPath)
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            if (string.IsNullOrEmpty(projectRoot))
            {
                throw new InvalidOperationException("Could not resolve Unity project root.");
            }

            return Path.Combine(projectRoot, NormalizeAssetPath(assetPath));
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
