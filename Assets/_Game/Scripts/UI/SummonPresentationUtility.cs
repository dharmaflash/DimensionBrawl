using TMPro;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace IsekaiBrawl.Gameplay
{
    public static class SummonPresentationUtility
    {
        public static string GetShortLabel(SummonData summonData)
        {
            if (summonData == null)
            {
                return string.Empty;
            }

            string rawLabel = string.IsNullOrWhiteSpace(summonData.shortLabel) ? summonData.summonName : summonData.shortLabel;
            string normalizedLabel = rawLabel.Trim().ToLowerInvariant();

            if (normalizedLabel.Contains("rush"))
            {
                return "\uB3CC\uC9C4";
            }

            if (normalizedLabel.Contains("break"))
            {
                return "\uB3CC\uD30C";
            }

            if (normalizedLabel.Contains("tank") || normalizedLabel.Contains("golem"))
            {
                return "\uC720\uC9C0";
            }

            if (normalizedLabel.Contains("arrow") || normalizedLabel.Contains("archer"))
            {
                return "\uACAC\uC81C";
            }

            if (normalizedLabel.Contains("meteor") || normalizedLabel.Contains("splash"))
            {
                return "\uAD11\uC5ED";
            }

            if (normalizedLabel.Contains("heal") || normalizedLabel.Contains("pixie"))
            {
                return "\uC9C0\uC6D0";
            }

            return summonData.summonType switch
            {
                SummonType.Melee when summonData.structureDamageMultiplier >= 1.8f => "\uB3CC\uD30C",
                SummonType.Melee when summonData.moveSpeed >= 3.2f => "\uB3CC\uC9C4",
                SummonType.Melee => "\uACB0\uD22C",
                SummonType.Tank => "\uC720\uC9C0",
                SummonType.Ranged when summonData.splashRadius > 0.1f => "\uAD11\uC5ED",
                SummonType.Ranged when summonData.attackRange >= 7f => "\uACAC\uC81C",
                SummonType.Ranged => "\uC0AC\uACA9",
                SummonType.Support => "\uC9C0\uC6D0",
                _ => rawLabel
            };
        }

        public static string GetRoleLabel(SummonData summonData)
        {
            if (summonData == null)
            {
                return string.Empty;
            }

            return summonData.summonType switch
            {
                SummonType.Melee when summonData.structureDamageMultiplier >= 1.8f => "\uB3CC\uD30C",
                SummonType.Melee when summonData.moveSpeed >= 3.2f => "\uB3CC\uC9C4",
                SummonType.Melee => "\uACB0\uD22C",
                SummonType.Tank => "\uC720\uC9C0",
                SummonType.Ranged when summonData.splashRadius > 0.1f => "\uAD11\uC5ED",
                SummonType.Ranged when summonData.attackRange >= 7f => "\uACAC\uC81C",
                SummonType.Ranged => "\uC0AC\uACA9",
                SummonType.Support => "\uC9C0\uC6D0",
                _ => summonData.summonType.ToString()
            };
        }

        public static string GetCardDetail(SummonData summonData)
        {
            if (summonData == null)
            {
                return string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(summonData.roleDescription))
            {
                return summonData.roleDescription;
            }

            return GetRoleLabel(summonData);
        }

        public static Color GetCardColor(SummonData summonData)
        {
            if (summonData == null)
            {
                return new Color(0.2f, 0.2f, 0.2f, 0.8f);
            }

            return summonData.summonType switch
            {
                SummonType.Melee => new Color(0.86f, 0.34f, 0.34f, 1f),
                SummonType.Tank => new Color(0.38f, 0.8f, 0.95f, 1f),
                SummonType.Ranged => new Color(0.62f, 0.46f, 0.92f, 1f),
                SummonType.Support => new Color(0.42f, 0.86f, 0.56f, 1f),
                _ => Color.white
            };
        }
    }

    public static class RuntimeUIFontUtility
    {
        private static readonly string[] KoreanProjectFontResources =
        {
            "Fonts/NotoSansKR-VF",
            "Fonts/malgun",
            "Fonts/malgunbd"
        };

        private static readonly string[] KoreanOsFontNames =
        {
            "Malgun Gothic",
            "\uB9D1\uC740 \uACE0\uB515",
            "MalgunGothic",
            "NanumBarunGothic",
            "Noto Sans KR",
            "NotoSansKR",
            "Noto Sans CJK KR",
            "Arial Unicode MS"
        };

        private static TMP_FontAsset cachedKoreanFallback;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetRuntimeCache()
        {
            cachedKoreanFallback = null;
        }

        public static TMP_FontAsset EnsureKoreanFallback()
        {
            if (cachedKoreanFallback != null)
            {
                InstallFallback(cachedKoreanFallback);
                return cachedKoreanFallback;
            }

            Font sourceFont = ResolveSourceFont();
            if (sourceFont != null)
            {
                cachedKoreanFallback = TMP_FontAsset.CreateFontAsset(
                    sourceFont,
                    90,
                    9,
                    GlyphRenderMode.SDFAA,
                    1024,
                    1024,
                    AtlasPopulationMode.Dynamic,
                    true);

                if (cachedKoreanFallback != null)
                {
                    cachedKoreanFallback.name = "Runtime Korean UI Font";
                    cachedKoreanFallback.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
                }
            }

            InstallFallback(cachedKoreanFallback);
            return cachedKoreanFallback != null ? cachedKoreanFallback : TMP_Settings.defaultFontAsset;
        }

        private static Font ResolveSourceFont()
        {
            for (int index = 0; index < KoreanProjectFontResources.Length; index++)
            {
                Font projectFont = Resources.Load<Font>(KoreanProjectFontResources[index]);
                if (projectFont != null)
                {
                    return projectFont;
                }
            }

            for (int index = 0; index < KoreanOsFontNames.Length; index++)
            {
                Font osFont = Font.CreateDynamicFontFromOSFont(KoreanOsFontNames[index], 32);
                if (osFont != null)
                {
                    return osFont;
                }
            }

            return Font.CreateDynamicFontFromOSFont(KoreanOsFontNames, 32);
        }

        public static void ApplyRecursively(Transform root)
        {
            TMP_FontAsset fallback = EnsureKoreanFallback();
            if (root == null || fallback == null)
            {
                return;
            }

            TMP_Text[] texts = root.GetComponentsInChildren<TMP_Text>(true);
            for (int index = 0; index < texts.Length; index++)
            {
                ApplyToText(texts[index], fallback);
            }
        }

        public static void ApplyToText(TMP_Text text)
        {
            ApplyToText(text, EnsureKoreanFallback());
        }

        private static void ApplyToText(TMP_Text text, TMP_FontAsset fallback)
        {
            if (text == null || fallback == null)
            {
                return;
            }

            TMP_FontAsset currentFont = text.font;
            if (currentFont != null &&
                currentFont != fallback &&
                !currentFont.fallbackFontAssetTable.Contains(fallback))
            {
                currentFont.fallbackFontAssetTable.Add(fallback);
            }

            if (text.font != fallback)
            {
                text.font = fallback;
            }

            text.havePropertiesChanged = true;
            text.ForceMeshUpdate(false, true);
        }

        private static void InstallFallback(TMP_FontAsset fallback)
        {
            if (fallback == null)
            {
                return;
            }

            if (TMP_Settings.fallbackFontAssets != null &&
                !TMP_Settings.fallbackFontAssets.Contains(fallback))
            {
                TMP_Settings.fallbackFontAssets.Add(fallback);
            }

            TMP_FontAsset defaultFont = TMP_Settings.defaultFontAsset;
            if (defaultFont != null &&
                defaultFont != fallback &&
                !defaultFont.fallbackFontAssetTable.Contains(fallback))
            {
                defaultFont.fallbackFontAssetTable.Add(fallback);
            }
        }
    }

    public static class RuntimeUISpriteUtility
    {
        private static Sprite cachedPanelSprite;

        public static Sprite GetPanelSprite()
        {
            if (cachedPanelSprite != null)
            {
                return cachedPanelSprite;
            }

            Texture2D texture = new Texture2D(4, 4, TextureFormat.RGBA32, false)
            {
                name = "RuntimePanelSpriteTexture",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };

            Color[] pixels = new Color[16];
            for (int index = 0; index < pixels.Length; index++)
            {
                pixels[index] = Color.white;
            }

            texture.SetPixels(pixels);
            texture.Apply(false, true);

            cachedPanelSprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect,
                new Vector4(1f, 1f, 1f, 1f));

            cachedPanelSprite.name = "RuntimePanelSprite";
            cachedPanelSprite.hideFlags = HideFlags.HideAndDontSave;
            return cachedPanelSprite;
        }
    }
}
