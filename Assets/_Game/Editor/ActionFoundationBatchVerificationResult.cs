using System;
using System.Collections.Generic;
using System.IO;

namespace DimensionBrawl.Editor
{
    internal static class ActionFoundationBatchVerificationResult
    {
        private const string ResultPassLine = "RESULT=PASS";
        private const string ResultFailLine = "RESULT=FAIL";

        public static void DeleteIfExists(string path)
        {
            if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
            {
                File.Delete(path);
            }
        }

        public static void WriteResult(
            string resultPath,
            bool passed,
            string state,
            string reportPath,
            IEnumerable<string> reportLines)
        {
            if (string.IsNullOrWhiteSpace(resultPath))
            {
                throw new ArgumentException("Result path must be provided.", nameof(resultPath));
            }

            string directory = Path.GetDirectoryName(resultPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var lines = new List<string>
            {
                $"STATE={state}",
                passed ? ResultPassLine : ResultFailLine,
                $"EXIT_CODE={(passed ? 0 : 1)}"
            };

            if (!string.IsNullOrWhiteSpace(reportPath))
            {
                lines.Add($"REPORT_PATH={reportPath}");
            }

            if (reportLines != null)
            {
                lines.Add("REPORT_BEGIN");
                lines.AddRange(reportLines);
                lines.Add("REPORT_END");
            }

            File.WriteAllLines(resultPath, lines);
        }

        public static void WriteResult(
            string resultPath,
            bool passed,
            string state,
            string reportPath,
            string report)
        {
            WriteResult(resultPath, passed, state, reportPath, SplitLines(report));
        }

        public static void WriteException(
            string resultPath,
            string state,
            string reportPath,
            Exception exception)
        {
            WriteResult(
                resultPath,
                false,
                state,
                reportPath,
                exception == null ? Array.Empty<string>() : SplitLines(exception.ToString()));
        }

        public static bool IsPassMarkerFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return false;
            }

            foreach (string line in File.ReadLines(path))
            {
                if (string.Equals(line, ResultPassLine, StringComparison.Ordinal))
                {
                    return true;
                }

                if (line.StartsWith("RESULT=", StringComparison.Ordinal))
                {
                    return false;
                }
            }

            return false;
        }

        public static void RequirePassMarker(string path, string label)
        {
            if (!IsPassMarkerFile(path))
            {
                throw new InvalidOperationException(
                    $"{label} did not produce an exact {ResultPassLine} marker at {path}.");
            }
        }

        private static IEnumerable<string> SplitLines(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return Array.Empty<string>();
            }

            return value.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        }
    }
}
