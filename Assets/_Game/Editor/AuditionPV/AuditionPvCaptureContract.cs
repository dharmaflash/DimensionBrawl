using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace DimensionBrawl.Editor.AuditionPV
{
    internal static class AuditionPvCaptureContract
    {
        public const string SchemaVersion = "dimension-brawl.audition-pv.capture-manifest.v1";
        public const string OutputRoot = "D:/DimensionBrawl_PV/01_capture_video/PREEDIT_GOLD";
        public const string ManifestFileName = "capture_manifest.json";
        public const string SourceFormat = "png_sequence_srgb_8bit_lossless";
        public const string RecorderPackageName = "com.unity.recorder";
        public const string RecorderPackageVersion = "5.1.6";
        public const string UrpPackageName = "com.unity.render-pipelines.universal";
        public const int Width = 2560;
        public const int Height = 1440;
        public const int Fps = 60;

        public static readonly string[] CoreDependencyPaths =
        {
            "Packages/manifest.json",
            "Packages/packages-lock.json",
            "ProjectSettings/ProjectVersion.txt",
            "ProjectSettings/GraphicsSettings.asset",
            "ProjectSettings/QualitySettings.asset"
        };
    }

    internal static class AuditionPvOutputPaths
    {
        private const int MaximumCaptureIdLength = 128;
        private const int MaximumSlugLength = 48;

        private static readonly HashSet<string> WindowsReservedNames = new(StringComparer.OrdinalIgnoreCase)
        {
            "con", "prn", "aux", "nul",
            "com1", "com2", "com3", "com4", "com5", "com6", "com7", "com8", "com9",
            "lpt1", "lpt2", "lpt3", "lpt4", "lpt5", "lpt6", "lpt7", "lpt8", "lpt9"
        };

        public static string CreateOutputId(
            string label,
            DateTime utcNow,
            string gitCommitSha,
            bool isDirty,
            string dirtyStateHashSha256)
        {
            DateTime normalizedUtc = utcNow.Kind == DateTimeKind.Utc ? utcNow : utcNow.ToUniversalTime();
            string slug = SanitizeSegment(label, MaximumSlugLength);
            string commit = ShortHexOrFallback(gitCommitSha, 12, "unknown");
            string state = isDirty
                ? "dirty-" + ShortHexOrFallback(dirtyStateHashSha256, 12, "unknown")
                : "clean";

            string outputId = string.Format(
                CultureInfo.InvariantCulture,
                "{0}_{1}_g{2}_{3}",
                normalizedUtc.ToString("yyyyMMdd'T'HHmmss'Z'", CultureInfo.InvariantCulture).ToLowerInvariant(),
                slug,
                commit,
                state);

            ValidateOutputId(outputId);
            return outputId;
        }

        public static string ResolveGoldenOutputDirectory(string outputId)
        {
            return ResolveOutputDirectory(AuditionPvCaptureContract.OutputRoot, outputId);
        }

        public static string CreateUniqueGoldenOutputDirectory(string outputId)
        {
            return CreateUniqueOutputDirectory(AuditionPvCaptureContract.OutputRoot, outputId);
        }

        internal static string CreateUniqueOutputDirectory(string outputRoot, string outputId)
        {
            string normalizedRoot = NormalizeRoot(outputRoot);
            Directory.CreateDirectory(normalizedRoot);

            for (int revision = 1; revision <= 999; revision++)
            {
                string candidateId = revision == 1
                    ? outputId
                    : outputId + "_r" + revision.ToString("000", CultureInfo.InvariantCulture);
                string candidate = ResolveOutputDirectory(normalizedRoot, candidateId);
                if (Directory.Exists(candidate) || File.Exists(candidate))
                {
                    continue;
                }

                Directory.CreateDirectory(candidate);
                return candidate.Replace('\\', '/');
            }

            throw new IOException($"Could not reserve a unique capture directory for '{outputId}'.");
        }

        internal static string ResolveOutputDirectory(string outputRoot, string outputId)
        {
            ValidateOutputId(outputId);
            string normalizedRoot = NormalizeRoot(outputRoot);
            string candidate = Path.GetFullPath(Path.Combine(normalizedRoot, outputId));
            string parent = Path.GetDirectoryName(candidate)?.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (!string.Equals(parent, normalizedRoot, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Capture output must be a direct child of the configured output root.");
            }

            return candidate.Replace('\\', '/');
        }

        internal static string SanitizeSegment(string value, int maximumLength = MaximumSlugLength)
        {
            if (maximumLength <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maximumLength));
            }

            var builder = new StringBuilder(Math.Min(maximumLength, value?.Length ?? 0));
            bool previousWasSeparator = false;
            string source = value ?? string.Empty;
            for (int index = 0; index < source.Length && builder.Length < maximumLength; index++)
            {
                char character = char.ToLowerInvariant(source[index]);
                bool isAsciiLetter = character >= 'a' && character <= 'z';
                bool isDigit = character >= '0' && character <= '9';
                if (isAsciiLetter || isDigit)
                {
                    builder.Append(character);
                    previousWasSeparator = false;
                    continue;
                }

                if (builder.Length > 0 && !previousWasSeparator && builder.Length < maximumLength)
                {
                    builder.Append('-');
                    previousWasSeparator = true;
                }
            }

            string sanitized = builder.ToString().Trim('-');
            if (string.IsNullOrEmpty(sanitized))
            {
                sanitized = "capture";
            }

            if (WindowsReservedNames.Contains(sanitized))
            {
                sanitized = "x-" + sanitized;
            }

            return sanitized;
        }

        internal static void ValidateOutputId(string outputId)
        {
            if (string.IsNullOrWhiteSpace(outputId))
            {
                throw new ArgumentException("Capture output ID must not be empty.", nameof(outputId));
            }

            if (outputId.Length > MaximumCaptureIdLength)
            {
                throw new ArgumentException($"Capture output ID exceeds {MaximumCaptureIdLength} characters.", nameof(outputId));
            }

            if (outputId == "." || outputId == ".." || outputId.Contains("..", StringComparison.Ordinal))
            {
                throw new ArgumentException("Capture output ID must not contain traversal tokens.", nameof(outputId));
            }

            for (int index = 0; index < outputId.Length; index++)
            {
                char character = outputId[index];
                bool valid = character >= 'a' && character <= 'z' ||
                             character >= '0' && character <= '9' ||
                             character == '-' || character == '_';
                if (!valid)
                {
                    throw new ArgumentException(
                        "Capture output ID may contain only lowercase ASCII letters, digits, hyphens, and underscores.",
                        nameof(outputId));
                }
            }
        }

        private static string NormalizeRoot(string outputRoot)
        {
            if (string.IsNullOrWhiteSpace(outputRoot))
            {
                throw new ArgumentException("Capture output root must not be empty.", nameof(outputRoot));
            }

            string normalized = Path.GetFullPath(outputRoot)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (!Path.IsPathRooted(normalized))
            {
                throw new ArgumentException("Capture output root must be absolute.", nameof(outputRoot));
            }

            return normalized;
        }

        private static string ShortHexOrFallback(string value, int length, string fallback)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return fallback;
            }

            string normalized = new string(value
                .Trim()
                .ToLowerInvariant()
                .Where(character => character >= '0' && character <= '9' || character >= 'a' && character <= 'f')
                .ToArray());
            return normalized.Length == 0
                ? fallback
                : normalized.Substring(0, Math.Min(length, normalized.Length));
        }
    }

    internal static class AuditionPvSha256
    {
        public static string FileHash(string path)
        {
            using FileStream stream = File.OpenRead(path);
            using SHA256 sha256 = SHA256.Create();
            return ToLowerHex(sha256.ComputeHash(stream));
        }

        public static string TextHash(string value)
        {
            using SHA256 sha256 = SHA256.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(value ?? string.Empty);
            return ToLowerHex(sha256.ComputeHash(bytes));
        }

        public static bool IsSha256(string value)
        {
            return value != null && value.Length == 64 && value.All(character =>
                character >= '0' && character <= '9' || character >= 'a' && character <= 'f');
        }

        private static string ToLowerHex(byte[] value)
        {
            var builder = new StringBuilder(value.Length * 2);
            for (int index = 0; index < value.Length; index++)
            {
                builder.Append(value[index].ToString("x2", CultureInfo.InvariantCulture));
            }

            return builder.ToString();
        }
    }
}
