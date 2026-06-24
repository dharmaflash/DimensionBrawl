using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace DimensionBrawl.Presentation
{
    [DisallowMultipleComponent]
    public sealed class CinematicPlaylistPlayModeCaptureProbe : MonoBehaviour
    {
        [Serializable]
        public struct Sample
        {
            [SerializeField] private string label;
            [SerializeField, Min(0f)] private float routeSeconds;
            [SerializeField] private string expectedProfileId;
            [SerializeField] private string expectedCameraCueId;
            [SerializeField] private bool expectedWeaponVisible;

            public string Label => label;
            public float RouteSeconds => routeSeconds;
            public string ExpectedProfileId => expectedProfileId;
            public string ExpectedCameraCueId => expectedCameraCueId;
            public bool ExpectedWeaponVisible => expectedWeaponVisible;
        }

        private struct CapturedSample
        {
            public string Label;
            public float RouteSeconds;
            public string ExpectedProfileId;
            public string CurrentProfileId;
            public string ExpectedCameraCueId;
            public string LastCameraCueId;
            public string LastActorCueId;
            public string LastVfxCueId;
            public string LastTutorialCueId;
            public string ActivePromptCueId;
            public int CompletedEntryCount;
            public string LastCompletedProfileId;
            public bool ExpectedWeaponVisible;
            public bool WeaponVisible;
            public string FramePath;
            public bool CaptureSucceeded;
        }

        private struct CapturedTimelineFrame
        {
            public int Index;
            public float RouteSeconds;
            public string CurrentProfileId;
            public string LastCameraCueId;
            public string LastActorCueId;
            public string LastVfxCueId;
            public string LastTutorialCueId;
            public string ActivePromptCueId;
            public int CompletedEntryCount;
            public string LastCompletedProfileId;
            public string FramePath;
            public bool CaptureSucceeded;
        }

        [SerializeField] private string outputDirectory = "C:/tmp/DimensionBrawl-CinematicP0Review-PlayModeFrames";
        [SerializeField] private string stripPath = "C:/tmp/DimensionBrawl-CinematicP0Review-PlayModeRouteStrip.png";
        [SerializeField] private string reportPath = "C:/tmp/DimensionBrawl-CinematicP0Review-PlayModeRoute.md";
        [SerializeField] private string resultPath = "C:/tmp/DimensionBrawl-CinematicP0Review-PlayModeRoute.result";
        [SerializeField] private bool captureTimeline = true;
        [SerializeField] private string timelineDirectory = "C:/tmp/DimensionBrawl-CinematicP0Review-TimelineFrames";
        [SerializeField] private string timelineStripPath = "C:/tmp/DimensionBrawl-CinematicP0Review-TimelineStrip.png";
        [SerializeField] private string timelineReportPath = "C:/tmp/DimensionBrawl-CinematicP0Review-Timeline.md";
        [SerializeField, Min(0.25f)] private float timelineIntervalSeconds = 2.5f;
        [SerializeField, Min(1)] private int minimumTimelineFrameCount = 12;
        [SerializeField, Min(16)] private int timelineCaptureWidth = 640;
        [SerializeField, Min(16)] private int timelineCaptureHeight = 360;
        [SerializeField, Min(1)] private int timelineStripColumns = 5;
        [SerializeField, Min(16)] private int captureWidth = 1280;
        [SerializeField, Min(16)] private int captureHeight = 720;
        [SerializeField, Min(1f)] private float maxRouteSeconds = 60f;
        [SerializeField] private Sample[] samples = Array.Empty<Sample>();

        private bool captureStarted;

        private void Start()
        {
            BeginCapture();
        }

        public void BeginCapture()
        {
            if (captureStarted)
            {
                return;
            }

            captureStarted = true;
            Debug.Log("[CinematicPlaylistPlayModeCaptureProbe] Capture started.");
            StartCoroutine(CaptureRoutine());
        }

        private IEnumerator CaptureRoutine()
        {
            CapturedSample[] capturedSamples = Array.Empty<CapturedSample>();
            string failure = null;
            bool success = false;
            CinematicSequencePlaylistRunner playlistRunner = null;
            CinematicSequenceRunner runner = null;
            Camera camera = null;
            Sample[] resolvedSamples = samples ?? Array.Empty<Sample>();
            List<CapturedTimelineFrame> timelineFrames = captureTimeline
                ? new List<CapturedTimelineFrame>(Mathf.CeilToInt(maxRouteSeconds / Mathf.Max(0.25f, timelineIntervalSeconds)) + 1)
                : null;

            yield return null;
            yield return null;

            try
            {
                Directory.CreateDirectory(outputDirectory);
                if (captureTimeline)
                {
                    Directory.CreateDirectory(timelineDirectory);
                }

                playlistRunner = FindFirstObjectByType<CinematicSequencePlaylistRunner>();
                runner = FindFirstObjectByType<CinematicSequenceRunner>();
                camera = runner != null ? runner.CinematicCamera : Camera.main;

                if (playlistRunner == null)
                {
                    throw new InvalidOperationException("Missing CinematicSequencePlaylistRunner.");
                }

                if (runner == null)
                {
                    throw new InvalidOperationException("Missing CinematicSequenceRunner.");
                }

                if (camera == null)
                {
                    throw new InvalidOperationException("Missing cinematic camera.");
                }

                if (!playlistRunner.IsPlaying && playlistRunner.CompletedEntryCount == 0)
                {
                    playlistRunner.TryPlay();
                }
            }
            catch (Exception exception)
            {
                failure = exception.ToString();
                Debug.LogError($"[CinematicPlaylistPlayModeCaptureProbe] {failure}");
                WriteResult(false, failure, capturedSamples.Length, timelineFrames?.Count ?? 0);
                yield break;
            }

            float routeStartedAt = Time.realtimeSinceStartup;
            capturedSamples = new CapturedSample[resolvedSamples.Length];
            float nextTimelineSeconds = captureTimeline ? 0.5f : float.PositiveInfinity;
            int timelineFrameIndex = 0;

            for (int i = 0; i < resolvedSamples.Length; i++)
            {
                Sample sample = resolvedSamples[i];
                while ((Time.realtimeSinceStartup - routeStartedAt) < sample.RouteSeconds)
                {
                    float routeSeconds = Time.realtimeSinceStartup - routeStartedAt;
                    if (routeSeconds > maxRouteSeconds)
                    {
                        failure = $"Play mode route capture timed out before sample {sample.Label}.";
                        Debug.LogError($"[CinematicPlaylistPlayModeCaptureProbe] {failure}");
                        WriteResult(false, failure, capturedSamples.Length, timelineFrames?.Count ?? 0);
                        yield break;
                    }

                    if (captureTimeline && routeSeconds >= nextTimelineSeconds)
                    {
                        try
                        {
                            CaptureTimelineFrame(
                                camera,
                                runner,
                                playlistRunner,
                                ++timelineFrameIndex,
                                routeSeconds,
                                timelineFrames);
                        }
                        catch (Exception exception)
                        {
                            failure = exception.ToString();
                            Debug.LogError($"[CinematicPlaylistPlayModeCaptureProbe] {failure}");
                            WriteResult(false, failure, capturedSamples.Length, timelineFrames?.Count ?? 0);
                            yield break;
                        }

                        nextTimelineSeconds += Mathf.Max(0.25f, timelineIntervalSeconds);
                    }

                    yield return null;
                }

                yield return null;
                try
                {
                    string framePath = Path.Combine(outputDirectory, $"{i + 1:00}_{SanitizeFileName(sample.Label)}.png")
                        .Replace('\\', '/');
                    Debug.Log($"[CinematicPlaylistPlayModeCaptureProbe] Capturing {sample.Label} at route {sample.RouteSeconds:F2}s.");
                    bool captured = CaptureCamera(camera, framePath);
                    capturedSamples[i] = BuildCapturedSample(
                        sample,
                        framePath,
                        captured,
                        runner,
                        playlistRunner);
                }
                catch (Exception exception)
                {
                    failure = exception.ToString();
                    Debug.LogError($"[CinematicPlaylistPlayModeCaptureProbe] {failure}");
                    WriteResult(false, failure, capturedSamples.Length, timelineFrames?.Count ?? 0);
                    yield break;
                }
            }

            try
            {
                if (captureTimeline && timelineFrames != null)
                {
                    float finalRouteSeconds = Time.realtimeSinceStartup - routeStartedAt;
                    if (timelineFrames.Count == 0
                        || finalRouteSeconds - timelineFrames[timelineFrames.Count - 1].RouteSeconds > 0.25f)
                    {
                        CaptureTimelineFrame(
                            camera,
                            runner,
                            playlistRunner,
                            ++timelineFrameIndex,
                            finalRouteSeconds,
                            timelineFrames);
                    }
                }

                CreateContactSheet(capturedSamples, stripPath, 320, 180, 3);
                WriteReport(capturedSamples);
                CapturedTimelineFrame[] resolvedTimelineFrames = timelineFrames != null
                    ? timelineFrames.ToArray()
                    : Array.Empty<CapturedTimelineFrame>();
                if (captureTimeline)
                {
                    CreateTimelineContactSheet(
                        resolvedTimelineFrames,
                        timelineStripPath,
                        240,
                        135,
                        timelineStripColumns);
                    WriteTimelineReport(resolvedTimelineFrames);
                }

                success = ValidateCapturedSamples(capturedSamples, out failure);
                if (success && captureTimeline && resolvedTimelineFrames.Length < minimumTimelineFrameCount)
                {
                    failure = $"Expected at least {minimumTimelineFrameCount} timeline frames, got {resolvedTimelineFrames.Length}.";
                    success = false;
                }
            }
            catch (Exception exception)
            {
                failure = exception.ToString();
                Debug.LogError($"[CinematicPlaylistPlayModeCaptureProbe] {failure}");
            }

            WriteResult(success, failure, capturedSamples.Length, timelineFrames?.Count ?? 0);
        }

        private static CapturedSample BuildCapturedSample(
            Sample sample,
            string framePath,
            bool captured,
            CinematicSequenceRunner runner,
            CinematicSequencePlaylistRunner playlistRunner)
        {
            string currentProfileId = runner.SequenceProfile != null ? runner.SequenceProfile.SequenceId : string.Empty;
            return new CapturedSample
            {
                Label = sample.Label,
                RouteSeconds = sample.RouteSeconds,
                ExpectedProfileId = sample.ExpectedProfileId,
                CurrentProfileId = currentProfileId,
                ExpectedCameraCueId = sample.ExpectedCameraCueId,
                LastCameraCueId = runner.LastCameraCueId,
                LastActorCueId = runner.LastActorCueId,
                LastVfxCueId = runner.LastVfxCueId,
                LastTutorialCueId = runner.LastTutorialCueId,
                ActivePromptCueId = runner.TutorialPromptPresenter != null
                    ? runner.TutorialPromptPresenter.ActiveCueId
                    : string.Empty,
                CompletedEntryCount = playlistRunner.CompletedEntryCount,
                LastCompletedProfileId = playlistRunner.LastCompletedProfileId,
                ExpectedWeaponVisible = sample.ExpectedWeaponVisible,
                WeaponVisible = ResolveRifleVisibility(),
                FramePath = framePath,
                CaptureSucceeded = captured
            };
        }

        private void CaptureTimelineFrame(
            Camera camera,
            CinematicSequenceRunner runner,
            CinematicSequencePlaylistRunner playlistRunner,
            int frameIndex,
            float routeSeconds,
            List<CapturedTimelineFrame> timelineFrames)
        {
            if (timelineFrames == null)
            {
                return;
            }

            string framePath = Path.Combine(
                    timelineDirectory,
                    $"{frameIndex:000}_t{FormatTenths(routeSeconds)}.png")
                .Replace('\\', '/');
            Debug.Log($"[CinematicPlaylistPlayModeCaptureProbe] Timeline capture {frameIndex:000} at route {routeSeconds:F2}s.");
            bool captured = CaptureCamera(camera, framePath, timelineCaptureWidth, timelineCaptureHeight);
            string currentProfileId = runner.SequenceProfile != null ? runner.SequenceProfile.SequenceId : string.Empty;
            timelineFrames.Add(new CapturedTimelineFrame
            {
                Index = frameIndex,
                RouteSeconds = routeSeconds,
                CurrentProfileId = currentProfileId,
                LastCameraCueId = runner.LastCameraCueId,
                LastActorCueId = runner.LastActorCueId,
                LastVfxCueId = runner.LastVfxCueId,
                LastTutorialCueId = runner.LastTutorialCueId,
                ActivePromptCueId = runner.TutorialPromptPresenter != null
                    ? runner.TutorialPromptPresenter.ActiveCueId
                    : string.Empty,
                CompletedEntryCount = playlistRunner.CompletedEntryCount,
                LastCompletedProfileId = playlistRunner.LastCompletedProfileId,
                FramePath = framePath,
                CaptureSucceeded = captured
            });
        }

        private static bool ResolveRifleVisibility()
        {
            Transform[] transforms = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < transforms.Length; i++)
            {
                Transform target = transforms[i];
                if (target != null && target.name.IndexOf("InoriRifle", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return target.gameObject.activeInHierarchy;
                }
            }

            return false;
        }

        private static bool ValidateCapturedSamples(CapturedSample[] capturedSamples, out string failure)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < capturedSamples.Length; i++)
            {
                CapturedSample sample = capturedSamples[i];
                if (!sample.CaptureSucceeded)
                {
                    builder.AppendLine($"Sample {i + 1} failed to capture: {sample.Label}");
                }

                if (!string.IsNullOrWhiteSpace(sample.ExpectedProfileId)
                    && !string.Equals(sample.ExpectedProfileId, sample.CurrentProfileId, StringComparison.Ordinal))
                {
                    builder.AppendLine(
                        $"Sample {i + 1} expected profile {sample.ExpectedProfileId}, got {sample.CurrentProfileId}.");
                }

                if (!string.IsNullOrWhiteSpace(sample.ExpectedCameraCueId)
                    && !string.Equals(sample.ExpectedCameraCueId, sample.LastCameraCueId, StringComparison.Ordinal))
                {
                    builder.AppendLine(
                        $"Sample {i + 1} expected camera {sample.ExpectedCameraCueId}, got {sample.LastCameraCueId}.");
                }

                if (sample.ExpectedWeaponVisible != sample.WeaponVisible)
                {
                    builder.AppendLine(
                        $"Sample {i + 1} expected weapon visible={sample.ExpectedWeaponVisible}, got {sample.WeaponVisible}.");
                }

                if (!string.IsNullOrWhiteSpace(sample.LastTutorialCueId)
                    && !string.Equals(sample.LastTutorialCueId, sample.ActivePromptCueId, StringComparison.Ordinal))
                {
                    builder.AppendLine(
                        $"Sample {i + 1} expected active prompt {sample.LastTutorialCueId}, got {sample.ActivePromptCueId}.");
                }
            }

            failure = builder.ToString().Trim();
            return string.IsNullOrWhiteSpace(failure);
        }

        private bool CaptureCamera(Camera camera, string path)
        {
            return CaptureCamera(camera, path, captureWidth, captureHeight);
        }

        private bool CaptureCamera(Camera camera, string path, int width, int height)
        {
            RenderTexture previousTarget = camera.targetTexture;
            RenderTexture previousActive = RenderTexture.active;
            RenderTexture renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            Texture2D image = new Texture2D(width, height, TextureFormat.RGBA32, mipChain: false);
            try
            {
                camera.targetTexture = renderTexture;
                RenderTexture.active = renderTexture;
                camera.Render();
                image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                image.Apply();

                if (!IsUsableTexture(image))
                {
                    return false;
                }

                Directory.CreateDirectory(Path.GetDirectoryName(path) ?? outputDirectory);
                File.WriteAllBytes(path, image.EncodeToPNG());
                return true;
            }
            finally
            {
                camera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                Destroy(image);
                Destroy(renderTexture);
            }
        }

        private static bool IsUsableTexture(Texture2D image)
        {
            int stepX = Mathf.Max(1, image.width / 16);
            int stepY = Mathf.Max(1, image.height / 16);
            for (int y = 0; y < image.height; y += stepY)
            {
                for (int x = 0; x < image.width; x += stepX)
                {
                    Color32 pixel = image.GetPixel(x, y);
                    if (pixel.r > 8 || pixel.g > 8 || pixel.b > 8)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static void CreateContactSheet(CapturedSample[] capturedSamples, string outputPath, int width, int height, int columns)
        {
            if (capturedSamples == null || capturedSamples.Length == 0)
            {
                return;
            }

            int resolvedColumns = Mathf.Max(1, columns);
            int rows = Mathf.CeilToInt(capturedSamples.Length / (float)resolvedColumns);
            Texture2D sheet = new Texture2D(width * resolvedColumns, height * rows, TextureFormat.RGBA32, mipChain: false);
            try
            {
                Color[] background = new Color[sheet.width * sheet.height];
                for (int i = 0; i < background.Length; i++)
                {
                    background[i] = new Color(0.045f, 0.052f, 0.064f, 1f);
                }

                sheet.SetPixels(background);
                for (int i = 0; i < capturedSamples.Length; i++)
                {
                    CapturedSample sample = capturedSamples[i];
                    int column = i % resolvedColumns;
                    int row = i / resolvedColumns;
                    int targetX = column * width;
                    int targetY = sheet.height - ((row + 1) * height);

                    if (File.Exists(sample.FramePath))
                    {
                        Texture2D source = new Texture2D(2, 2, TextureFormat.RGBA32, mipChain: false);
                        Texture2D resized = null;
                        try
                        {
                            if (source.LoadImage(File.ReadAllBytes(sample.FramePath)))
                            {
                                resized = ResizeTexture(source, width, height);
                                sheet.SetPixels(targetX, targetY, width, height, resized.GetPixels());
                            }
                        }
                        finally
                        {
                            Destroy(source);
                            if (resized != null)
                            {
                                Destroy(resized);
                            }
                        }
                    }

                    DrawLabelOverlay(sheet, targetX, targetY, width, height, BuildSampleLabelLines(sample, i + 1));
                }

                sheet.Apply();
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? "C:/tmp");
                File.WriteAllBytes(outputPath, sheet.EncodeToPNG());
            }
            finally
            {
                Destroy(sheet);
            }
        }

        private static Texture2D ResizeTexture(Texture2D source, int width, int height)
        {
            Texture2D resized = new Texture2D(width, height, TextureFormat.RGBA32, mipChain: false);
            Color[] pixels = new Color[width * height];
            for (int y = 0; y < height; y++)
            {
                float v = height > 1 ? y / (float)(height - 1) : 0f;
                for (int x = 0; x < width; x++)
                {
                    float u = width > 1 ? x / (float)(width - 1) : 0f;
                    pixels[x + (y * width)] = source.GetPixelBilinear(u, v);
                }
            }

            resized.SetPixels(pixels);
            resized.Apply();
            return resized;
        }

        private static string[] BuildSampleLabelLines(CapturedSample sample, int index)
        {
            string state = sample.CaptureSucceeded ? "OK" : "CAPTURE FAILED";
            return new[]
            {
                $"{index:00} T+{sample.RouteSeconds:F1}s {state}",
                NormalizeLabel(sample.Label),
                $"PROF {NormalizeLabel(sample.CurrentProfileId)}",
                $"CAM {NormalizeLabel(sample.LastCameraCueId)}"
            };
        }

        private static string[] BuildTimelineLabelLines(CapturedTimelineFrame frame)
        {
            string prompt = string.IsNullOrWhiteSpace(frame.ActivePromptCueId)
                ? string.Empty
                : $" PROMPT {NormalizeLabel(frame.ActivePromptCueId)}";
            string camera = $"CAM {NormalizeLabel(frame.LastCameraCueId)}{prompt}";
            return new[]
            {
                $"{frame.Index:000} T+{frame.RouteSeconds:F1}s",
                $"PROF {NormalizeLabel(frame.CurrentProfileId)}",
                camera
            };
        }

        private static string NormalizeLabel(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "NONE";
            }

            return value
                .Replace('_', ' ')
                .Replace('-', ' ')
                .ToUpperInvariant();
        }

        private static void DrawLabelOverlay(Texture2D target, int tileX, int tileY, int tileWidth, int tileHeight, string[] lines)
        {
            if (target == null || lines == null || lines.Length == 0)
            {
                return;
            }

            const int scale = 2;
            const int glyphHeight = 7;
            const int lineGap = 3;
            int panelHeight = Mathf.Clamp(
                10 + (lines.Length * glyphHeight * scale) + ((lines.Length - 1) * lineGap),
                42,
                Mathf.Max(42, tileHeight / 2));
            int panelY = tileY + 4;
            int panelX = tileX + 4;
            int panelWidth = Mathf.Max(1, tileWidth - 8);

            DrawFilledRect(target, panelX, panelY, panelWidth, panelHeight, new Color(0.02f, 0.025f, 0.035f, 0.78f));
            DrawFilledRect(target, panelX, panelY + panelHeight - 3, panelWidth, 3, new Color(0.28f, 0.75f, 1f, 0.9f));

            int textX = panelX + 8;
            int textWidth = Mathf.Max(16, panelWidth - 16);
            for (int i = 0; i < lines.Length; i++)
            {
                string line = TrimForPixelWidth(lines[i], textWidth, scale);
                int lineBottomY = panelY + panelHeight - 7 - ((i + 1) * glyphHeight * scale) - (i * lineGap);
                Color color = i == 0
                    ? new Color(1f, 0.93f, 0.45f, 1f)
                    : Color.white;
                DrawBitmapText(target, line, textX + 1, lineBottomY - 1, scale, new Color(0f, 0f, 0f, 0.85f));
                DrawBitmapText(target, line, textX, lineBottomY, scale, color);
            }
        }

        private static void DrawFilledRect(Texture2D target, int x, int y, int width, int height, Color color)
        {
            int maxX = Mathf.Min(target.width, x + width);
            int maxY = Mathf.Min(target.height, y + height);
            for (int yy = Mathf.Max(0, y); yy < maxY; yy++)
            {
                for (int xx = Mathf.Max(0, x); xx < maxX; xx++)
                {
                    Color existing = target.GetPixel(xx, yy);
                    target.SetPixel(xx, yy, Color.Lerp(existing, color, color.a));
                }
            }
        }

        private static string TrimForPixelWidth(string value, int maxWidth, int scale)
        {
            string resolved = string.IsNullOrWhiteSpace(value) ? "NONE" : value;
            if (MeasureBitmapTextWidth(resolved, scale) <= maxWidth)
            {
                return resolved;
            }

            const string suffix = "...";
            while (resolved.Length > 0 && MeasureBitmapTextWidth(resolved + suffix, scale) > maxWidth)
            {
                resolved = resolved.Substring(0, resolved.Length - 1);
            }

            return string.IsNullOrEmpty(resolved) ? suffix : resolved + suffix;
        }

        private static int MeasureBitmapTextWidth(string text, int scale)
        {
            if (string.IsNullOrEmpty(text))
            {
                return 0;
            }

            int width = 0;
            for (int i = 0; i < text.Length; i++)
            {
                width += ((text[i] == ' ' ? 3 : 5) * scale) + scale;
            }

            return Mathf.Max(0, width - scale);
        }

        private static void DrawBitmapText(Texture2D target, string text, int x, int bottomY, int scale, Color color)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            int cursorX = x;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                string[] glyph = GetBitmapGlyph(c);
                if (glyph != null)
                {
                    DrawGlyph(target, glyph, cursorX, bottomY, scale, color);
                }

                cursorX += ((c == ' ' ? 3 : 5) * scale) + scale;
            }
        }

        private static void DrawGlyph(Texture2D target, string[] glyph, int x, int bottomY, int scale, Color color)
        {
            for (int row = 0; row < glyph.Length; row++)
            {
                string pattern = glyph[row];
                for (int column = 0; column < pattern.Length; column++)
                {
                    if (pattern[column] != '1')
                    {
                        continue;
                    }

                    int baseX = x + (column * scale);
                    int baseY = bottomY + ((glyph.Length - 1 - row) * scale);
                    for (int yy = 0; yy < scale; yy++)
                    {
                        int pixelY = baseY + yy;
                        if (pixelY < 0 || pixelY >= target.height)
                        {
                            continue;
                        }

                        for (int xx = 0; xx < scale; xx++)
                        {
                            int pixelX = baseX + xx;
                            if (pixelX < 0 || pixelX >= target.width)
                            {
                                continue;
                            }

                            Color existing = target.GetPixel(pixelX, pixelY);
                            target.SetPixel(pixelX, pixelY, Color.Lerp(existing, color, color.a));
                        }
                    }
                }
            }
        }

        private static string[] GetBitmapGlyph(char c)
        {
            switch (char.ToUpperInvariant(c))
            {
                case 'A': return new[] { "01110", "10001", "10001", "11111", "10001", "10001", "10001" };
                case 'B': return new[] { "11110", "10001", "10001", "11110", "10001", "10001", "11110" };
                case 'C': return new[] { "01111", "10000", "10000", "10000", "10000", "10000", "01111" };
                case 'D': return new[] { "11110", "10001", "10001", "10001", "10001", "10001", "11110" };
                case 'E': return new[] { "11111", "10000", "10000", "11110", "10000", "10000", "11111" };
                case 'F': return new[] { "11111", "10000", "10000", "11110", "10000", "10000", "10000" };
                case 'G': return new[] { "01111", "10000", "10000", "10011", "10001", "10001", "01111" };
                case 'H': return new[] { "10001", "10001", "10001", "11111", "10001", "10001", "10001" };
                case 'I': return new[] { "11111", "00100", "00100", "00100", "00100", "00100", "11111" };
                case 'J': return new[] { "00111", "00010", "00010", "00010", "00010", "10010", "01100" };
                case 'K': return new[] { "10001", "10010", "10100", "11000", "10100", "10010", "10001" };
                case 'L': return new[] { "10000", "10000", "10000", "10000", "10000", "10000", "11111" };
                case 'M': return new[] { "10001", "11011", "10101", "10101", "10001", "10001", "10001" };
                case 'N': return new[] { "10001", "11001", "10101", "10011", "10001", "10001", "10001" };
                case 'O': return new[] { "01110", "10001", "10001", "10001", "10001", "10001", "01110" };
                case 'P': return new[] { "11110", "10001", "10001", "11110", "10000", "10000", "10000" };
                case 'Q': return new[] { "01110", "10001", "10001", "10001", "10101", "10010", "01101" };
                case 'R': return new[] { "11110", "10001", "10001", "11110", "10100", "10010", "10001" };
                case 'S': return new[] { "01111", "10000", "10000", "01110", "00001", "00001", "11110" };
                case 'T': return new[] { "11111", "00100", "00100", "00100", "00100", "00100", "00100" };
                case 'U': return new[] { "10001", "10001", "10001", "10001", "10001", "10001", "01110" };
                case 'V': return new[] { "10001", "10001", "10001", "10001", "01010", "01010", "00100" };
                case 'W': return new[] { "10001", "10001", "10001", "10101", "10101", "10101", "01010" };
                case 'X': return new[] { "10001", "01010", "00100", "00100", "00100", "01010", "10001" };
                case 'Y': return new[] { "10001", "01010", "00100", "00100", "00100", "00100", "00100" };
                case 'Z': return new[] { "11111", "00001", "00010", "00100", "01000", "10000", "11111" };
                case '0': return new[] { "01110", "10001", "10011", "10101", "11001", "10001", "01110" };
                case '1': return new[] { "00100", "01100", "00100", "00100", "00100", "00100", "01110" };
                case '2': return new[] { "01110", "10001", "00001", "00010", "00100", "01000", "11111" };
                case '3': return new[] { "11110", "00001", "00001", "01110", "00001", "00001", "11110" };
                case '4': return new[] { "00010", "00110", "01010", "10010", "11111", "00010", "00010" };
                case '5': return new[] { "11111", "10000", "10000", "11110", "00001", "00001", "11110" };
                case '6': return new[] { "01110", "10000", "10000", "11110", "10001", "10001", "01110" };
                case '7': return new[] { "11111", "00001", "00010", "00100", "01000", "01000", "01000" };
                case '8': return new[] { "01110", "10001", "10001", "01110", "10001", "10001", "01110" };
                case '9': return new[] { "01110", "10001", "10001", "01111", "00001", "00001", "01110" };
                case '.': return new[] { "00000", "00000", "00000", "00000", "00000", "01100", "01100" };
                case ':': return new[] { "00000", "01100", "01100", "00000", "01100", "01100", "00000" };
                case '+': return new[] { "00000", "00100", "00100", "11111", "00100", "00100", "00000" };
                case '-': return new[] { "00000", "00000", "00000", "11111", "00000", "00000", "00000" };
                case '/': return new[] { "00001", "00010", "00010", "00100", "01000", "01000", "10000" };
                case '?': return new[] { "01110", "10001", "00001", "00010", "00100", "00000", "00100" };
                case ' ': return new[] { "00000", "00000", "00000", "00000", "00000", "00000", "00000" };
                default: return GetBitmapGlyph('?');
            }
        }

        private void WriteReport(CapturedSample[] capturedSamples)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# DimensionBrawl Cinematic P0 Play Mode Route Capture");
            builder.AppendLine();
            builder.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            builder.AppendLine($"Contact sheet: `{stripPath}`");
            builder.AppendLine();
            builder.AppendLine("| # | Time | Label | Profile | Camera | Actor | VFX | Tutorial | Prompt | Completed | Weapon | Frame |");
            builder.AppendLine("|---|------|-------|---------|--------|-------|-----|----------|--------|-----------|--------|-------|");
            for (int i = 0; i < capturedSamples.Length; i++)
            {
                CapturedSample sample = capturedSamples[i];
                builder.AppendLine(
                    $"| {i + 1} | {sample.RouteSeconds:F2}s | {sample.Label} | `{sample.CurrentProfileId}` | `{sample.LastCameraCueId}` | `{sample.LastActorCueId}` | `{sample.LastVfxCueId}` | `{sample.LastTutorialCueId}` | `{sample.ActivePromptCueId}` | {sample.CompletedEntryCount}/`{sample.LastCompletedProfileId}` | {sample.WeaponVisible} | `{sample.FramePath}` |");
            }

            Directory.CreateDirectory(Path.GetDirectoryName(reportPath) ?? "C:/tmp");
            File.WriteAllText(reportPath, builder.ToString());
        }

        private static void CreateTimelineContactSheet(
            CapturedTimelineFrame[] timelineFrames,
            string outputPath,
            int width,
            int height,
            int columns)
        {
            if (timelineFrames == null || timelineFrames.Length == 0)
            {
                return;
            }

            int resolvedColumns = Mathf.Max(1, columns);
            int rows = Mathf.CeilToInt(timelineFrames.Length / (float)resolvedColumns);
            Texture2D sheet = new Texture2D(width * resolvedColumns, height * rows, TextureFormat.RGBA32, mipChain: false);
            try
            {
                Color[] background = new Color[sheet.width * sheet.height];
                for (int i = 0; i < background.Length; i++)
                {
                    background[i] = new Color(0.045f, 0.052f, 0.064f, 1f);
                }

                sheet.SetPixels(background);
                for (int i = 0; i < timelineFrames.Length; i++)
                {
                    CapturedTimelineFrame frame = timelineFrames[i];
                    int column = i % resolvedColumns;
                    int row = i / resolvedColumns;
                    int targetX = column * width;
                    int targetY = sheet.height - ((row + 1) * height);

                    if (File.Exists(frame.FramePath))
                    {
                        Texture2D source = new Texture2D(2, 2, TextureFormat.RGBA32, mipChain: false);
                        Texture2D resized = null;
                        try
                        {
                            if (source.LoadImage(File.ReadAllBytes(frame.FramePath)))
                            {
                                resized = ResizeTexture(source, width, height);
                                sheet.SetPixels(targetX, targetY, width, height, resized.GetPixels());
                            }
                        }
                        finally
                        {
                            Destroy(source);
                            if (resized != null)
                            {
                                Destroy(resized);
                            }
                        }
                    }

                    DrawLabelOverlay(sheet, targetX, targetY, width, height, BuildTimelineLabelLines(frame));
                }

                sheet.Apply();
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? "C:/tmp");
                File.WriteAllBytes(outputPath, sheet.EncodeToPNG());
            }
            finally
            {
                Destroy(sheet);
            }
        }

        private void WriteTimelineReport(CapturedTimelineFrame[] timelineFrames)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# DimensionBrawl Cinematic P0 Timeline Capture");
            builder.AppendLine();
            builder.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            builder.AppendLine($"Contact sheet: `{timelineStripPath}`");
            builder.AppendLine($"Frame directory: `{timelineDirectory}`");
            builder.AppendLine();
            builder.AppendLine("| # | Time | Profile | Camera | Actor | VFX | Tutorial | Prompt | Completed | Frame |");
            builder.AppendLine("|---|------|---------|--------|-------|-----|----------|--------|-----------|-------|");
            for (int i = 0; i < timelineFrames.Length; i++)
            {
                CapturedTimelineFrame frame = timelineFrames[i];
                builder.AppendLine(
                    $"| {frame.Index} | {frame.RouteSeconds:F2}s | `{frame.CurrentProfileId}` | `{frame.LastCameraCueId}` | `{frame.LastActorCueId}` | `{frame.LastVfxCueId}` | `{frame.LastTutorialCueId}` | `{frame.ActivePromptCueId}` | {frame.CompletedEntryCount}/`{frame.LastCompletedProfileId}` | `{frame.FramePath}` |");
            }

            Directory.CreateDirectory(Path.GetDirectoryName(timelineReportPath) ?? "C:/tmp");
            File.WriteAllText(timelineReportPath, builder.ToString());
        }

        private static string FormatTenths(float seconds)
        {
            int tenths = Mathf.Max(0, Mathf.RoundToInt(seconds * 10f));
            return $"{tenths / 10:00}_{tenths % 10}";
        }

        private void WriteResult(bool success, string failure, int frameCount, int timelineFrameCount)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(success ? "RESULT=PASS" : "RESULT=FAIL");
            builder.AppendLine($"FRAMES={frameCount}");
            builder.AppendLine($"REPORT={reportPath}");
            builder.AppendLine($"STRIP={stripPath}");
            builder.AppendLine($"TIMELINE_FRAMES={timelineFrameCount}");
            builder.AppendLine($"TIMELINE_REPORT={timelineReportPath}");
            builder.AppendLine($"TIMELINE_STRIP={timelineStripPath}");
            if (!string.IsNullOrWhiteSpace(failure))
            {
                builder.AppendLine("ERROR<<");
                builder.AppendLine(failure);
                builder.AppendLine(">>");
            }

            Directory.CreateDirectory(Path.GetDirectoryName(resultPath) ?? "C:/tmp");
            File.WriteAllText(resultPath, builder.ToString());
            Debug.Log($"[CinematicPlaylistPlayModeCaptureProbe] Wrote result: {resultPath}");
        }

        private static string SanitizeFileName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "sample";
            }

            char[] invalid = Path.GetInvalidFileNameChars();
            StringBuilder builder = new StringBuilder(value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                bool isInvalid = false;
                for (int j = 0; j < invalid.Length; j++)
                {
                    if (c == invalid[j])
                    {
                        isInvalid = true;
                        break;
                    }
                }

                builder.Append(isInvalid || char.IsWhiteSpace(c) ? '_' : c);
            }

            return builder.ToString();
        }
    }
}
