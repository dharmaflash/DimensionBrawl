using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace DimensionBrawl.Editor.AuditionPV
{
    // Produces the physical S100 edit-start planning graphic only. Final wording,
    // official marks, animation, and delivery treatment remain After Effects work.
    internal static class AuditionPvEndCardPlaceholderProducer
    {
        internal const string ReceiptSchema =
            "dimension-brawl.audition-pv.s100-layout-placeholder.v1";
        internal const string ShotId = "PV_S100";
        internal const string GraphicSourceId = "layout-placeholder";
        internal const string GraphicProductionStatus = "layout-placeholder-approved";
        internal const string PendingApprovalStatus = "pending-approval";
        internal const string FinalGraphicStatus = "deferred-to-ae-picture-lock";
        internal const string RenderPolicy =
            "rgba8-integer-composite-source-pixels-no-authored-wording-v1";
        internal const string OutputFileName =
            "PV_S100_layout_placeholder_qhd.png";
        internal const string ReceiptFileName =
            "PV_S100_layout_placeholder_receipt.json";
        internal const string GateRelativeArtifactPath =
            "graphics/" + OutputFileName;
        internal const int Width = AuditionPvSixtySecondGateManifestValidator.Width;
        internal const int Height = AuditionPvSixtySecondGateManifestValidator.Height;

        internal const string KoreanLogoPath =
            "Assets/_Game/UI/Login/Art/Sprites/Login_Logo_KR.png";
        internal const string EnglishSublogoPath =
            "Assets/_Game/UI/Login/Art/Sprites/Login_Sublogo_EN.png";
        internal const string PretendardMediumPath =
            "Assets/_Game/Art/Fonts/Pretendard/Pretendard-Medium.otf";
        internal const string PretendardSemiBoldPath =
            "Assets/_Game/Art/Fonts/Pretendard/Pretendard-SemiBold.otf";
        internal const string PretendardLicensePath =
            "Assets/_Game/Art/Fonts/Pretendard/Pretendard_LICENSE.txt";

        private const string MenuPath =
            "DimensionBrawl/Audition PV/Produce S100 QHD Layout Placeholder";

        internal static readonly AuditionPvEndCardSourceSpec[] SourceSpecs =
        {
            new()
            {
                sourceId = "dimension-brawl-logo-kr",
                role = "composited-logo-kr",
                scope = "asset",
                assetPath = KoreanLogoPath
            },
            new()
            {
                sourceId = "dimension-brawl-sublogo-en",
                role = "composited-sublogo-en",
                scope = "asset",
                assetPath = EnglishSublogoPath
            },
            new()
            {
                sourceId = "pretendard-medium",
                role = "planned-slogan-font",
                scope = "font",
                assetPath = PretendardMediumPath
            },
            new()
            {
                sourceId = "pretendard-semibold",
                role = "planned-audition-notice-font",
                scope = "font",
                assetPath = PretendardSemiBoldPath
            },
            new()
            {
                sourceId = "pretendard-license",
                role = "font-license",
                scope = "font-license",
                assetPath = PretendardLicensePath
            }
        };

        internal static readonly AuditionPvEndCardLayoutZone[] LayoutZones =
        {
            new()
            {
                zoneId = "logo-lockup",
                x = 512,
                y = 674,
                width = 1536,
                height = 430,
                disposition = "source-logo-composite"
            },
            new()
            {
                zoneId = "slogan",
                x = 755,
                y = 505,
                width = 1050,
                height = 64,
                disposition = "pending-approval-ae-wording"
            },
            new()
            {
                zoneId = "audition-notice",
                x = 890,
                y = 399,
                width = 780,
                height = 44,
                disposition = "pending-approval-ae-wording"
            }
        };

        [MenuItem(MenuPath)]
        public static void ProduceMenu()
        {
            AuditionPvEndCardPlaceholderResult result = Produce(
                ResolveProjectRoot(),
                AuditionPvSixtySecondGateManifestValidator.ProductionGraphicsRoot);
            Debug.Log(
                "[AuditionPvEndCardPlaceholderProducer] PASS: "
                + result.outputPath
                + " sha256="
                + result.outputSha256);
            EditorUtility.RevealInFinder(result.outputPath);
        }

        public static void RunBatch()
        {
            try
            {
                AuditionPvEndCardPlaceholderResult result = Produce(
                    ResolveProjectRoot(),
                    AuditionPvSixtySecondGateManifestValidator.ProductionGraphicsRoot);
                Debug.Log(
                    "[AuditionPvEndCardPlaceholderProducer] BATCH_PASS: "
                    + result.outputPath
                    + " sha256="
                    + result.outputSha256);
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Debug.LogError("[AuditionPvEndCardPlaceholderProducer] BATCH_FAIL");
                EditorApplication.Exit(1);
            }
        }

        internal static AuditionPvEndCardPlaceholderResult Produce(
            string projectRoot,
            string graphicsRoot)
        {
            string normalizedProjectRoot = NormalizeAbsoluteDirectory(
                projectRoot,
                nameof(projectRoot));
            string normalizedGraphicsRoot = NormalizeAbsoluteDirectory(
                graphicsRoot,
                nameof(graphicsRoot));
            AuditionPvEndCardSourcePin[] sourcePins = BuildSourcePins(
                normalizedProjectRoot);

            byte[] pngBytes = RenderPlaceholder(
                ResolveSourcePath(normalizedProjectRoot, KoreanLogoPath),
                ResolveSourcePath(normalizedProjectRoot, EnglishSublogoPath));
            string outputSha256 = HashBytes(pngBytes);

            Directory.CreateDirectory(normalizedGraphicsRoot);
            string outputPath = Path.Combine(normalizedGraphicsRoot, OutputFileName);
            File.WriteAllBytes(outputPath, pngBytes);
            ValidateWrittenPng(outputPath, outputSha256);

            var receipt = new AuditionPvEndCardPlaceholderReceipt
            {
                schemaVersion = ReceiptSchema,
                shotId = ShotId,
                graphicSourceId = GraphicSourceId,
                graphicProductionStatus = GraphicProductionStatus,
                sloganApprovalStatus = PendingApprovalStatus,
                auditionNoticeApprovalStatus = PendingApprovalStatus,
                finalGraphicStatus = FinalGraphicStatus,
                renderPolicy = RenderPolicy,
                width = Width,
                height = Height,
                colorManagement =
                    AuditionPvSixtySecondGateManifestValidator.ColorManagement,
                artifact = new AuditionPvPinnedArtifact
                {
                    path = GateRelativeArtifactPath,
                    sha256 = outputSha256
                },
                sources = sourcePins,
                layoutZones = LayoutZones.Select(CloneZone).ToArray()
            };
            string receiptPath = Path.Combine(normalizedGraphicsRoot, ReceiptFileName);
            File.WriteAllText(
                receiptPath,
                JsonUtility.ToJson(receipt, prettyPrint: true) + Environment.NewLine,
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

            return new AuditionPvEndCardPlaceholderResult
            {
                outputPath = outputPath,
                outputSha256 = outputSha256,
                receiptPath = receiptPath,
                receiptSha256 = AuditionPvSha256.FileHash(receiptPath),
                receipt = receipt
            };
        }

        internal static AuditionPvEndCardSourcePin[] BuildSourcePins(string projectRoot)
        {
            string normalizedProjectRoot = NormalizeAbsoluteDirectory(
                projectRoot,
                nameof(projectRoot));
            return SourceSpecs.Select(specification =>
            {
                string sourcePath = ResolveSourcePath(
                    normalizedProjectRoot,
                    specification.assetPath);
                if (!File.Exists(sourcePath))
                {
                    throw new FileNotFoundException(
                        "Missing canonical S100 source asset.",
                        sourcePath);
                }

                return new AuditionPvEndCardSourcePin
                {
                    sourceId = specification.sourceId,
                    role = specification.role,
                    scope = specification.scope,
                    assetPath = specification.assetPath,
                    sha256 = AuditionPvSha256.FileHash(sourcePath)
                };
            }).ToArray();
        }

        private static string ResolveProjectRoot()
        {
            string assetsPath = Path.GetFullPath(Application.dataPath);
            DirectoryInfo parent = Directory.GetParent(assetsPath);
            if (parent == null)
                throw new InvalidOperationException(
                    "Could not resolve the Unity project root from Application.dataPath.");
            return parent.FullName;
        }

        private static string NormalizeAbsoluteDirectory(string path, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException(
                    "An absolute directory is required.",
                    parameterName);
            if (!Path.IsPathRooted(path))
                throw new ArgumentException(
                    "The directory must be absolute.",
                    parameterName);
            return Path.GetFullPath(path.Trim())
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }

        private static string ResolveSourcePath(string projectRoot, string assetPath)
        {
            string normalizedAssetPath = assetPath.Replace('/', Path.DirectorySeparatorChar);
            string candidate = Path.GetFullPath(Path.Combine(projectRoot, normalizedAssetPath));
            string prefix = projectRoot + Path.DirectorySeparatorChar;
            if (!candidate.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException(
                    "S100 source asset escapes the Unity project root: " + assetPath);
            return candidate;
        }

        private static byte[] RenderPlaceholder(
            string koreanLogoPath,
            string englishSublogoPath)
        {
            Texture2D koreanLogo = null;
            Texture2D englishSublogo = null;
            Texture2D output = null;
            try
            {
                koreanLogo = DecodePng(koreanLogoPath);
                englishSublogo = DecodePng(englishSublogoPath);
                Color32[] pixels = BuildBackground();

                DrawSafeFrame(pixels);
                CompositeCentered(
                    pixels,
                    koreanLogo.GetPixels32(),
                    koreanLogo.width,
                    koreanLogo.height,
                    centerY: 858);
                CompositeCentered(
                    pixels,
                    englishSublogo.GetPixels32(),
                    englishSublogo.width,
                    englishSublogo.height,
                    centerY: 700);
                DrawPendingWordingZones(pixels);

                output = new Texture2D(
                    Width,
                    Height,
                    TextureFormat.RGBA32,
                    mipChain: false,
                    linear: true);
                output.SetPixels32(pixels);
                output.Apply(updateMipmaps: false, makeNoLongerReadable: false);
                byte[] encoded = output.EncodeToPNG();
                if (encoded == null || encoded.Length < 33)
                    throw new InvalidDataException(
                        "Unity did not encode the S100 QHD placeholder as PNG.");
                return encoded;
            }
            finally
            {
                if (output != null) UnityEngine.Object.DestroyImmediate(output);
                if (englishSublogo != null) UnityEngine.Object.DestroyImmediate(englishSublogo);
                if (koreanLogo != null) UnityEngine.Object.DestroyImmediate(koreanLogo);
            }
        }

        private static Texture2D DecodePng(string path)
        {
            byte[] bytes = File.ReadAllBytes(path);
            var texture = new Texture2D(
                2,
                2,
                TextureFormat.RGBA32,
                mipChain: false,
                linear: true);
            if (!ImageConversion.LoadImage(texture, bytes, markNonReadable: false))
            {
                UnityEngine.Object.DestroyImmediate(texture);
                throw new InvalidDataException(
                    "Could not decode canonical S100 PNG source: " + path);
            }
            return texture;
        }

        private static Color32[] BuildBackground()
        {
            var pixels = new Color32[Width * Height];
            for (int y = 0; y < Height; y++)
            {
                int vertical = y * 255 / (Height - 1);
                byte red = LerpByte(5, 13, vertical);
                byte green = LerpByte(10, 27, vertical);
                byte blue = LerpByte(20, 45, vertical);
                for (int x = 0; x < Width; x++)
                {
                    int radial = Math.Max(
                        0,
                        255 - Math.Abs(x - Width / 2) * 255 / (Width / 2));
                    int cyanLift = radial * (96 + vertical) / (255 * 14);
                    int diagonal = ((x + y * 2) / 160) % 2 == 0 ? 2 : 0;
                    pixels[y * Width + x] = new Color32(
                        ClampByte(red + diagonal),
                        ClampByte(green + cyanLift + diagonal),
                        ClampByte(blue + cyanLift * 2 + diagonal),
                        255);
                }
            }
            return pixels;
        }

        private static void DrawSafeFrame(Color32[] pixels)
        {
            Color32 faint = new(45, 167, 198, 70);
            Color32 accent = new(72, 222, 240, 188);
            DrawOutline(pixels, 160, 90, Width - 320, Height - 180, 2, faint);
            DrawCorner(pixels, 205, 135, 120, 4, accent, left: true, bottom: true);
            DrawCorner(pixels, Width - 205, 135, 120, 4, accent, left: false, bottom: true);
            DrawCorner(pixels, 205, Height - 135, 120, 4, accent, left: true, bottom: false);
            DrawCorner(
                pixels,
                Width - 205,
                Height - 135,
                120,
                4,
                accent,
                left: false,
                bottom: false);
            DrawRect(pixels, 420, 624, Width - 840, 2, new Color32(53, 190, 220, 96));
        }

        private static void DrawPendingWordingZones(Color32[] pixels)
        {
            Color32 slotFill = new(19, 50, 67, 210);
            Color32 slotBorder = new(70, 197, 221, 180);
            Color32 notch = new(150, 236, 244, 210);
            foreach (AuditionPvEndCardLayoutZone zone in LayoutZones
                         .Where(value => value.zoneId != "logo-lockup"))
            {
                DrawRect(pixels, zone.x, zone.y, zone.width, zone.height, slotFill);
                DrawOutline(pixels, zone.x, zone.y, zone.width, zone.height, 2, slotBorder);
                DrawRect(pixels, zone.x, zone.y, 18, 3, notch);
                DrawRect(
                    pixels,
                    zone.x + zone.width - 18,
                    zone.y + zone.height - 3,
                    18,
                    3,
                    notch);
            }
        }

        private static void CompositeCentered(
            Color32[] destination,
            Color32[] source,
            int sourceWidth,
            int sourceHeight,
            int centerY)
        {
            int startX = (Width - sourceWidth) / 2;
            int startY = centerY - sourceHeight / 2;
            for (int sourceY = 0; sourceY < sourceHeight; sourceY++)
            {
                int destinationY = startY + sourceY;
                if (destinationY < 0 || destinationY >= Height) continue;
                for (int sourceX = 0; sourceX < sourceWidth; sourceX++)
                {
                    int destinationX = startX + sourceX;
                    if (destinationX < 0 || destinationX >= Width) continue;
                    Color32 foreground = source[sourceY * sourceWidth + sourceX];
                    if (foreground.a == 0) continue;
                    int destinationIndex = destinationY * Width + destinationX;
                    destination[destinationIndex] = AlphaBlend(
                        destination[destinationIndex],
                        foreground);
                }
            }
        }

        private static Color32 AlphaBlend(Color32 background, Color32 foreground)
        {
            int alpha = foreground.a;
            int inverse = 255 - alpha;
            return new Color32(
                (byte)((foreground.r * alpha + background.r * inverse + 127) / 255),
                (byte)((foreground.g * alpha + background.g * inverse + 127) / 255),
                (byte)((foreground.b * alpha + background.b * inverse + 127) / 255),
                255);
        }

        private static void DrawCorner(
            Color32[] pixels,
            int x,
            int y,
            int arm,
            int thickness,
            Color32 color,
            bool left,
            bool bottom)
        {
            int horizontalX = left ? x : x - arm;
            int verticalY = bottom ? y : y - arm;
            DrawRect(pixels, horizontalX, y - thickness / 2, arm, thickness, color);
            DrawRect(pixels, x - thickness / 2, verticalY, thickness, arm, color);
        }

        private static void DrawOutline(
            Color32[] pixels,
            int x,
            int y,
            int width,
            int height,
            int thickness,
            Color32 color)
        {
            DrawRect(pixels, x, y, width, thickness, color);
            DrawRect(pixels, x, y + height - thickness, width, thickness, color);
            DrawRect(pixels, x, y, thickness, height, color);
            DrawRect(pixels, x + width - thickness, y, thickness, height, color);
        }

        private static void DrawRect(
            Color32[] pixels,
            int x,
            int y,
            int width,
            int height,
            Color32 color)
        {
            int minimumX = Math.Max(0, x);
            int maximumX = Math.Min(Width, x + width);
            int minimumY = Math.Max(0, y);
            int maximumY = Math.Min(Height, y + height);
            for (int destinationY = minimumY; destinationY < maximumY; destinationY++)
            {
                for (int destinationX = minimumX; destinationX < maximumX; destinationX++)
                {
                    int index = destinationY * Width + destinationX;
                    pixels[index] = color.a == 255
                        ? color
                        : AlphaBlend(pixels[index], color);
                }
            }
        }

        private static byte LerpByte(int start, int end, int amount) =>
            (byte)((start * (255 - amount) + end * amount + 127) / 255);

        private static byte ClampByte(int value) =>
            (byte)Math.Max(0, Math.Min(255, value));

        private static string HashBytes(byte[] value)
        {
            using SHA256 hash = SHA256.Create();
            byte[] digest = hash.ComputeHash(value);
            var builder = new StringBuilder(digest.Length * 2);
            foreach (byte octet in digest)
                builder.Append(octet.ToString("x2"));
            return builder.ToString();
        }

        private static void ValidateWrittenPng(string path, string expectedSha256)
        {
            byte[] header = new byte[24];
            using (FileStream stream = File.OpenRead(path))
            {
                if (stream.Length < header.Length || stream.Read(header, 0, header.Length) != header.Length)
                    throw new InvalidDataException("S100 placeholder PNG is truncated: " + path);
            }
            byte[] signature = { 0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a };
            if (!header.Take(signature.Length).SequenceEqual(signature) ||
                ReadBigEndianInt32(header, 16) != Width ||
                ReadBigEndianInt32(header, 20) != Height)
                throw new InvalidDataException(
                    "S100 placeholder is not a decoded-header QHD PNG: " + path);
            if (AuditionPvSha256.FileHash(path) != expectedSha256)
                throw new InvalidDataException(
                    "S100 placeholder changed while it was being written: " + path);
        }

        private static int ReadBigEndianInt32(byte[] value, int offset) =>
            value[offset] << 24 |
            value[offset + 1] << 16 |
            value[offset + 2] << 8 |
            value[offset + 3];

        private static AuditionPvEndCardLayoutZone CloneZone(
            AuditionPvEndCardLayoutZone value) =>
            new()
            {
                zoneId = value.zoneId,
                x = value.x,
                y = value.y,
                width = value.width,
                height = value.height,
                disposition = value.disposition
            };
    }

    [Serializable]
    internal sealed class AuditionPvEndCardSourceSpec
    {
        public string sourceId = string.Empty;
        public string role = string.Empty;
        public string scope = string.Empty;
        public string assetPath = string.Empty;
    }

    [Serializable]
    internal sealed class AuditionPvEndCardSourcePin
    {
        public string sourceId = string.Empty;
        public string role = string.Empty;
        public string scope = string.Empty;
        public string assetPath = string.Empty;
        public string sha256 = string.Empty;
    }

    [Serializable]
    internal sealed class AuditionPvEndCardLayoutZone
    {
        public string zoneId = string.Empty;
        public int x;
        public int y;
        public int width;
        public int height;
        public string disposition = string.Empty;
    }

    [Serializable]
    internal sealed class AuditionPvEndCardPlaceholderReceipt
    {
        public string schemaVersion = string.Empty;
        public string shotId = string.Empty;
        public string graphicSourceId = string.Empty;
        public string graphicProductionStatus = string.Empty;
        public string sloganApprovalStatus = string.Empty;
        public string auditionNoticeApprovalStatus = string.Empty;
        public string finalGraphicStatus = string.Empty;
        public string renderPolicy = string.Empty;
        public int width;
        public int height;
        public string colorManagement = string.Empty;
        public AuditionPvPinnedArtifact artifact = new();
        public AuditionPvEndCardSourcePin[] sources =
            Array.Empty<AuditionPvEndCardSourcePin>();
        public AuditionPvEndCardLayoutZone[] layoutZones =
            Array.Empty<AuditionPvEndCardLayoutZone>();
    }

    internal sealed class AuditionPvEndCardPlaceholderResult
    {
        public string outputPath = string.Empty;
        public string outputSha256 = string.Empty;
        public string receiptPath = string.Empty;
        public string receiptSha256 = string.Empty;
        public AuditionPvEndCardPlaceholderReceipt receipt = new();
    }
}
