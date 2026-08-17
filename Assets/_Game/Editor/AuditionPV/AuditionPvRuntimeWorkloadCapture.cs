using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DimensionBrawl.Editor.AuditionPV
{
    /// <summary>
    /// Captures renderer/material/HUD facts while the source frame is actually presented.
    /// The sealed NDJSON is only an input to the physical evidence producer; by itself it is
    /// never a passed Gate test and cannot be used as a self-attested workload artifact.
    /// </summary>
    internal sealed class AuditionPvRuntimeWorkloadCaptureSession : IDisposable
    {
        internal const string SealSchema =
            "dimension-brawl.audition-pv.capture-runtime-workload-spool-seal.v2";
        internal const string ToolVersion = "2";
        internal const int MaxRangeFrames = 4096;
        internal const int MaxStableIdsPerInventory = 4096;
        // A full snapshot for a 4,096-renderer/4,096-material scene can legitimately exceed
        // 512 KiB. Only the first frame carries complete stable-ID inventories; changed frames
        // carry sorted add/remove deltas, while unchanged frames carry only counts and hashes.
        internal const int MaxFrameLineUtf8Bytes = 16 * 1024 * 1024;
        internal const long MaxSpoolUtf8Bytes = 256L * 1024L * 1024L;
        internal const string HudAuthoredAndExcluded = "hud-authored-and-excluded";
        internal const string SceneContractNoHud = "scene-contract-no-hud";

        private readonly AuditionPvRuntimeWorkloadCaptureConfig config;
        private readonly string framesPath;
        private readonly string sealPath;
        private readonly StreamWriter writer;
        private readonly AuditionPvRuntimeWorkloadCarryForwardEncoder carryForwardEncoder =
            new();
        private int nextSourceFrame;
        private int capturedFrameCount;
        private long writtenUtf8Bytes;
        private long maxFrameLineUtf8Bytes;
        private int inventorySnapshotFrameCount;
        private int inventoryDeltaFrameCount;
        private string resolvedHudEvidenceMode = string.Empty;
        private long inspectedObjectCount;
        private bool completed;
        private bool disposed;

        private AuditionPvRuntimeWorkloadCaptureSession(
            AuditionPvRuntimeWorkloadCaptureConfig config,
            string framesPath,
            string sealPath,
            StreamWriter writer)
        {
            this.config = config;
            this.framesPath = framesPath;
            this.sealPath = sealPath;
            this.writer = writer;
            nextSourceFrame = config.sourceRangeStartFrame;
        }

        internal string FramesPath => framesPath;
        internal string SealPath => sealPath;
        internal int CapturedFrameCount => capturedFrameCount;

        internal static AuditionPvRuntimeWorkloadCaptureSession Open(
            AuditionPvRuntimeWorkloadCaptureConfig config)
        {
            ValidateConfig(config);
            string captureDirectory = Full(config.captureOutputDirectory);
            string evidenceDirectory = Path.Combine(
                captureDirectory,
                "evidence",
                "runtime_workload_capture",
                SafeComponent(config.sourceShotId));
            RequireUnder(evidenceDirectory, captureDirectory);
            Directory.CreateDirectory(evidenceDirectory);
            string framesPath = Path.Combine(evidenceDirectory, "frames.ndjson");
            string sealPath = Path.Combine(evidenceDirectory, "seal.json");
            if (File.Exists(framesPath) || File.Exists(sealPath))
            {
                throw new IOException(
                    "Runtime workload capture is immutable and will not overwrite an existing spool: "
                    + evidenceDirectory);
            }

            var stream = new FileStream(
                framesPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.Read,
                64 * 1024,
                FileOptions.SequentialScan);
            var writer = new StreamWriter(stream, new UTF8Encoding(false), 64 * 1024, false)
            {
                NewLine = "\n"
            };
            return new AuditionPvRuntimeWorkloadCaptureSession(
                config,
                framesPath,
                sealPath,
                writer);
        }

        /// <summary>
        /// Must be called exactly once for every canonical source frame, including handles,
        /// after the frame has been presented and before the next simulation tick.
        /// </summary>
        internal void CapturePresentedFrame(int sourceFrame)
        {
            CapturePresentedFrame(sourceFrame, config.captureCamera);
        }

        /// <summary>
        /// Camera-switching shots pass the exact camera that presented this source frame.
        /// </summary>
        internal void CapturePresentedFrame(int sourceFrame, Camera frameCaptureCamera)
        {
            ThrowIfClosed();
            if (sourceFrame != nextSourceFrame)
            {
                throw new InvalidOperationException(
                    $"Runtime workload source-frame sequence drifted: expected {nextSourceFrame}, "
                    + $"received {sourceFrame}.");
            }

            AuditionPvRuntimeFrameWorkload frame = AuditionPvRuntimeWorkloadProbe.Capture(
                sourceFrame,
                frameCaptureCamera,
                config.captureHudEvidence);
            if (frame.nullMaterialCount != 0 || frame.errorMaterialCount != 0)
            {
                throw new InvalidDataException(
                    $"Runtime workload found null/error material slots at source frame {sourceFrame}.");
            }
            if (config.captureHudEvidence)
            {
                bool hasCanvases = frame.inspectedCanvasCount > 0;
                bool hasHudRenderers = frame.inspectedHudRendererCount > 0;
                if (hasCanvases != hasHudRenderers)
                {
                    throw new InvalidDataException(
                        "HUD evidence requires both authored canvases and HUD renderers, or neither.");
                }
                string frameMode = frame.inspectedCanvasCount == 0 &&
                                   frame.inspectedHudRendererCount == 0
                    ? SceneContractNoHud
                    : HudAuthoredAndExcluded;
                if (string.IsNullOrEmpty(resolvedHudEvidenceMode))
                {
                    resolvedHudEvidenceMode = frameMode;
                    inspectedObjectCount = AuditionPvRuntimeWorkloadProbe
                        .CountLoadedSceneObjects();
                }
                else if (!string.Equals(
                             resolvedHudEvidenceMode,
                             frameMode,
                             StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        "Clean-plate HUD evidence mode changed inside one source range.");
                }

                if (frame.visibleUiElementCount != 0)
                {
                    throw new InvalidOperationException(
                        $"HUD/UI remained visible to the capture camera at source frame {sourceFrame}.");
                }
            }

            carryForwardEncoder.Compress(frame, config.captureHudEvidence);
            string json = JsonUtility.ToJson(frame, false);
            int lineBytes = Encoding.UTF8.GetByteCount(json) + 1;
            RequireWithinBudget(
                lineBytes,
                writtenUtf8Bytes,
                config.sourceShotId,
                sourceFrame,
                capturedFrameCount);
            maxFrameLineUtf8Bytes = Math.Max(maxFrameLineUtf8Bytes, lineBytes);
            if (carryForwardEncoder.LastFrameIncludedFullSnapshot)
                inventorySnapshotFrameCount++;
            if (carryForwardEncoder.LastFrameIncludedDelta)
                inventoryDeltaFrameCount++;

            writer.WriteLine(json);
            writtenUtf8Bytes += lineBytes;
            capturedFrameCount++;
            nextSourceFrame = checked(sourceFrame + 1);
        }

        internal static void RequireWithinBudget(
            long lineBytes,
            long writtenBytes,
            string sourceShotId,
            int sourceFrame,
            int capturedCount)
        {
            if (lineBytes <= 0 || lineBytes > MaxFrameLineUtf8Bytes)
                throw new InvalidDataException(
                    $"Runtime workload row exceeded its fixed byte limit: " +
                    $"shot={sourceShotId}; sourceFrame={sourceFrame}; " +
                    $"lineBytes={lineBytes}; maxLineBytes={MaxFrameLineUtf8Bytes}; " +
                    $"writtenBytes={writtenBytes}; capturedCount={capturedCount}.");
            if (writtenBytes < 0 || writtenBytes > MaxSpoolUtf8Bytes ||
                lineBytes > MaxSpoolUtf8Bytes - writtenBytes)
                throw new InvalidDataException(
                    $"Runtime workload spool exceeded its fixed byte limit: " +
                    $"shot={sourceShotId}; sourceFrame={sourceFrame}; " +
                    $"lineBytes={lineBytes}; maxLineBytes={MaxFrameLineUtf8Bytes}; " +
                    $"writtenBytes={writtenBytes}; maxSpoolBytes={MaxSpoolUtf8Bytes}; " +
                    $"capturedCount={capturedCount}.");
        }

        internal string Complete()
        {
            ThrowIfClosed();
            int expected = checked(
                config.sourceRangeEndFrame - config.sourceRangeStartFrame + 1);
            if (capturedFrameCount != expected || nextSourceFrame !=
                checked(config.sourceRangeEndFrame + 1))
            {
                throw new InvalidDataException(
                    $"Runtime workload capture is incomplete ({capturedFrameCount}/{expected}).");
            }
            if (config.captureHudEvidence && string.IsNullOrEmpty(resolvedHudEvidenceMode))
            {
                throw new InvalidDataException("Clean-plate HUD evidence was never sampled.");
            }

            writer.Flush();
            writer.Dispose();
            long framesUtf8Bytes = new FileInfo(framesPath).Length;
            if (framesUtf8Bytes != writtenUtf8Bytes || framesUtf8Bytes <= 0 ||
                framesUtf8Bytes > MaxSpoolUtf8Bytes || maxFrameLineUtf8Bytes <= 0 ||
                inventorySnapshotFrameCount <= 0)
                throw new InvalidDataException(
                    "Runtime workload spool byte/compression accounting drifted before seal.");
            string framesSha256 = AuditionPvSha256.FileHash(framesPath);
            var seal = new AuditionPvRuntimeWorkloadCaptureSeal
            {
                schemaVersion = SealSchema,
                captureId = config.captureId,
                sourceShotId = config.sourceShotId,
                sourceRangeStartFrame = config.sourceRangeStartFrame,
                sourceRangeEndFrame = config.sourceRangeEndFrame,
                frameCount = capturedFrameCount,
                framesPath = Full(framesPath).Replace('\\', '/'),
                framesSha256 = framesSha256,
                framesUtf8Bytes = framesUtf8Bytes,
                maxFrameLineUtf8Bytes = maxFrameLineUtf8Bytes,
                inventorySnapshotFrameCount = inventorySnapshotFrameCount,
                inventoryDeltaFrameCount = inventoryDeltaFrameCount,
                hudEvidenceMode = config.captureHudEvidence
                    ? resolvedHudEvidenceMode
                    : string.Empty,
                inspectedObjectCount = config.captureHudEvidence
                    ? inspectedObjectCount
                    : 0,
                authoredHudComponentCount = resolvedHudEvidenceMode == SceneContractNoHud
                    ? 0
                    : -1,
                tool = nameof(AuditionPvRuntimeWorkloadCaptureSession),
                toolVersion = ToolVersion,
                completedAtUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture)
            };
            WriteJsonNew(sealPath, seal);
            completed = true;
            disposed = true;
            return Full(sealPath).Replace('\\', '/');
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            writer.Dispose();
            // No seal is emitted for an interrupted capture. The producer rejects the orphaned
            // NDJSON, so partial runtime observations can never become a passed capture test.
        }

        private void ThrowIfClosed()
        {
            if (disposed || completed)
                throw new ObjectDisposedException(nameof(AuditionPvRuntimeWorkloadCaptureSession));
        }

        private static void ValidateConfig(AuditionPvRuntimeWorkloadCaptureConfig value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (string.IsNullOrWhiteSpace(value.captureId) ||
                string.IsNullOrWhiteSpace(value.sourceShotId) ||
                string.IsNullOrWhiteSpace(value.captureOutputDirectory))
                throw new ArgumentException("Runtime workload capture identity is incomplete.");
            long count = (long)value.sourceRangeEndFrame - value.sourceRangeStartFrame + 1L;
            if (value.sourceRangeStartFrame < 0 || count <= 0 || count > MaxRangeFrames)
                throw new ArgumentOutOfRangeException(nameof(value),
                    "Runtime workload range exceeds its fixed evidence limit.");
            if (value.captureHudEvidence && value.captureCamera == null)
                throw new ArgumentException(
                    "Clean-plate HUD evidence requires the exact Recorder capture camera.");
        }

        private static void WriteJsonNew<T>(string path, T value)
        {
            if (File.Exists(path)) throw new IOException("Evidence file already exists: " + path);
            string temporary = path + ".tmp-" + Guid.NewGuid().ToString("N");
            try
            {
                File.WriteAllText(
                    temporary,
                    JsonUtility.ToJson(value, true) + "\n",
                    new UTF8Encoding(false));
                File.Move(temporary, path);
            }
            finally
            {
                if (File.Exists(temporary)) File.Delete(temporary);
            }
        }

        private static string SafeComponent(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value == "." || value == ".." ||
                value.IndexOfAny(new[] { '/', '\\', ':' }) >= 0)
                throw new InvalidDataException("Unsafe runtime workload path component.");
            return value;
        }

        private static string Full(string value) => Path.GetFullPath(value);

        private static void RequireUnder(string value, string root)
        {
            string full = Full(value).TrimEnd(Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar);
            string parent = Full(root).TrimEnd(Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar);
            if (string.Equals(full, parent, StringComparison.OrdinalIgnoreCase) ||
                full.StartsWith(parent + Path.DirectorySeparatorChar,
                    StringComparison.OrdinalIgnoreCase) ||
                full.StartsWith(parent + Path.AltDirectorySeparatorChar,
                    StringComparison.OrdinalIgnoreCase)) return;
            throw new InvalidDataException("Runtime workload path escaped the capture directory.");
        }
    }

    internal sealed class AuditionPvRuntimeWorkloadCaptureConfig
    {
        public string captureId = string.Empty;
        public string captureOutputDirectory = string.Empty;
        public string sourceShotId = string.Empty;
        public int sourceRangeStartFrame;
        public int sourceRangeEndFrame;
        public bool captureHudEvidence;
        public Camera captureCamera;
    }

    [Serializable]
    internal sealed class AuditionPvRuntimeWorkloadCaptureSeal
    {
        public string schemaVersion = string.Empty;
        public string captureId = string.Empty;
        public string sourceShotId = string.Empty;
        public int sourceRangeStartFrame;
        public int sourceRangeEndFrame;
        public int frameCount;
        public string framesPath = string.Empty;
        public string framesSha256 = string.Empty;
        public long framesUtf8Bytes;
        public long maxFrameLineUtf8Bytes;
        public int inventorySnapshotFrameCount;
        public int inventoryDeltaFrameCount;
        public string hudEvidenceMode = string.Empty;
        public long inspectedObjectCount;
        public long authoredHudComponentCount = -1;
        public string tool = string.Empty;
        public string toolVersion = string.Empty;
        public string completedAtUtc = string.Empty;
    }

    /// <summary>
    /// Converts complete probe inventories into a streaming snapshot/delta representation.
    /// It retains one bounded prior inventory per domain, never a frame history or PNG, so
    /// capture memory remains independent of source-frame count.
    /// </summary>
    internal sealed class AuditionPvRuntimeWorkloadCarryForwardEncoder
    {
        private readonly InventoryIdentity renderer = new();
        private readonly InventoryIdentity material = new();
        private readonly InventoryIdentity canvas = new();
        private readonly InventoryIdentity hud = new();

        internal bool LastFrameIncludedFullSnapshot { get; private set; }
        internal bool LastFrameIncludedDelta { get; private set; }

        internal void Compress(AuditionPvRuntimeFrameWorkload frame, bool captureHudEvidence)
        {
            if (frame == null) throw new ArgumentNullException(nameof(frame));
            LastFrameIncludedFullSnapshot = false;
            LastFrameIncludedDelta = false;
            CompressInventory(
                frame.rendererStableIds,
                frame.inspectedRendererCount,
                frame.rendererInventorySha256,
                false,
                "renderer",
                renderer,
                out frame.rendererStableIds,
                out frame.rendererAddedStableIds,
                out frame.rendererRemovedStableIds);
            CompressInventory(
                frame.materialSlotStableIds,
                frame.inspectedMaterialSlotCount,
                frame.materialInventorySha256,
                false,
                "material-slot",
                material,
                out frame.materialSlotStableIds,
                out frame.materialSlotAddedStableIds,
                out frame.materialSlotRemovedStableIds);
            if (!captureHudEvidence) return;
            CompressInventory(
                frame.canvasStableIds,
                frame.inspectedCanvasCount,
                frame.canvasInventorySha256,
                true,
                "canvas",
                canvas,
                out frame.canvasStableIds,
                out frame.canvasAddedStableIds,
                out frame.canvasRemovedStableIds);
            CompressInventory(
                frame.hudRendererStableIds,
                frame.inspectedHudRendererCount,
                frame.hudInventorySha256,
                true,
                "hud-renderer",
                hud,
                out frame.hudRendererStableIds,
                out frame.hudRendererAddedStableIds,
                out frame.hudRendererRemovedStableIds);
        }

        private void CompressInventory(
            string[] stableIds,
            long declaredCount,
            string declaredSha256,
            bool allowEmpty,
            string label,
            InventoryIdentity previous,
            out string[] snapshotIds,
            out string[] addedIds,
            out string[] removedIds)
        {
            stableIds ??= Array.Empty<string>();
            if (declaredCount != stableIds.LongLength ||
                stableIds.Length > AuditionPvRuntimeWorkloadCaptureSession
                    .MaxStableIdsPerInventory ||
                !allowEmpty && stableIds.Length == 0 ||
                !AuditionPvSha256.IsSha256(declaredSha256) ||
                !SortedUnique(stableIds))
                throw new InvalidDataException(
                    "Runtime workload probe returned an invalid " + label + " snapshot.");

            snapshotIds = Array.Empty<string>();
            addedIds = Array.Empty<string>();
            removedIds = Array.Empty<string>();
            if (!previous.hasSnapshot)
            {
                snapshotIds = stableIds;
                Set(previous, stableIds, declaredCount, declaredSha256);
                LastFrameIncludedFullSnapshot = true;
                return;
            }

            bool unchanged = previous.hasSnapshot && previous.count == declaredCount &&
                             string.Equals(
                                 previous.sha256,
                                 declaredSha256,
                                 StringComparison.Ordinal);
            if (unchanged) return;

            Diff(previous.stableIds, stableIds, out addedIds, out removedIds);
            if (addedIds.Length == 0 && removedIds.Length == 0)
                throw new InvalidDataException(
                    "Runtime workload inventory identity changed without a stable-ID delta.");
            Set(previous, stableIds, declaredCount, declaredSha256);
            LastFrameIncludedDelta = true;
        }

        private static void Set(
            InventoryIdentity target,
            string[] ids,
            long count,
            string sha256)
        {
            target.hasSnapshot = true;
            target.stableIds = ids;
            target.count = count;
            target.sha256 = sha256;
        }

        private static bool SortedUnique(string[] values)
        {
            for (int index = 0; index < values.Length; index++)
            {
                if (string.IsNullOrWhiteSpace(values[index]) ||
                    index > 0 && string.CompareOrdinal(values[index - 1], values[index]) >= 0)
                    return false;
            }
            return true;
        }

        private static void Diff(
            string[] previous,
            string[] current,
            out string[] added,
            out string[] removed)
        {
            var addedValues = new List<string>();
            var removedValues = new List<string>();
            int previousIndex = 0;
            int currentIndex = 0;
            while (previousIndex < previous.Length || currentIndex < current.Length)
            {
                if (previousIndex >= previous.Length)
                {
                    addedValues.Add(current[currentIndex++]);
                    continue;
                }
                if (currentIndex >= current.Length)
                {
                    removedValues.Add(previous[previousIndex++]);
                    continue;
                }
                int comparison = string.CompareOrdinal(
                    previous[previousIndex],
                    current[currentIndex]);
                if (comparison == 0)
                {
                    previousIndex++;
                    currentIndex++;
                }
                else if (comparison < 0)
                    removedValues.Add(previous[previousIndex++]);
                else
                    addedValues.Add(current[currentIndex++]);
            }
            added = addedValues.ToArray();
            removed = removedValues.ToArray();
        }

        private sealed class InventoryIdentity
        {
            internal bool hasSnapshot;
            internal long count;
            internal string sha256 = string.Empty;
            internal string[] stableIds = Array.Empty<string>();
        }
    }

    internal static class AuditionPvRuntimeWorkloadProbe
    {
        private const string InvalidGlobalObjectId = "GlobalObjectId_V1-0-0-0-0";

        internal static AuditionPvRuntimeFrameWorkload Capture(
            int sourceFrame,
            Camera captureCamera,
            bool captureHudEvidence)
        {
            string[] rendererIds;
            string[] materialIds;
            long nullMaterials = 0;
            long errorMaterials = 0;
            CaptureRendererAndMaterialInventory(
                out rendererIds,
                out materialIds,
                ref nullMaterials,
                ref errorMaterials);

            string[] canvasIds = Array.Empty<string>();
            string[] hudRendererIds = Array.Empty<string>();
            long visibleUi = 0;
            if (captureHudEvidence)
            {
                CaptureHudInventory(
                    captureCamera,
                    out canvasIds,
                    out hudRendererIds,
                    out visibleUi);
            }

            long drawCommands = checked((long)rendererIds.Length + hudRendererIds.Length);
            if (drawCommands <= 0)
            {
                throw new InvalidDataException(
                    "Runtime workload observed no renderer/UI draw candidates.");
            }

            return new AuditionPvRuntimeFrameWorkload
            {
                sourceFrame = sourceFrame,
                inspectedRendererCount = rendererIds.LongLength,
                inspectedMaterialSlotCount = materialIds.LongLength,
                nullMaterialCount = nullMaterials,
                errorMaterialCount = errorMaterials,
                rendererStableIds = rendererIds,
                materialSlotStableIds = materialIds,
                rendererInventorySha256 = AuditionPvSixtySecondGateManifestValidator
                    .StableInventorySha256("renderers", rendererIds),
                materialInventorySha256 = AuditionPvSixtySecondGateManifestValidator
                    .StableInventorySha256("material-slots", materialIds),
                inspectedCanvasCount = canvasIds.LongLength,
                inspectedHudRendererCount = hudRendererIds.LongLength,
                inspectedDrawCommandCount = drawCommands,
                visibleUiElementCount = visibleUi,
                canvasStableIds = canvasIds,
                hudRendererStableIds = hudRendererIds,
                canvasInventorySha256 = AuditionPvSixtySecondGateManifestValidator
                    .StableInventorySha256("canvases", canvasIds),
                hudInventorySha256 = AuditionPvSixtySecondGateManifestValidator
                    .StableInventorySha256("hud-renderers", hudRendererIds)
            };
        }

        internal static long CountLoadedSceneObjects()
        {
            long count = 0;
            for (int index = 0; index < SceneManager.sceneCount; index++)
            {
                Scene scene = SceneManager.GetSceneAt(index);
                if (!scene.IsValid() || !scene.isLoaded) continue;
                foreach (GameObject root in scene.GetRootGameObjects())
                    count = checked(count + root.GetComponentsInChildren<Transform>(true).LongLength);
            }
            if (count <= 0)
                throw new InvalidDataException("No loaded scene objects were available for HUD proof.");
            return count;
        }

        private static void CaptureRendererAndMaterialInventory(
            out string[] rendererIds,
            out string[] materialIds,
            ref long nullMaterials,
            ref long errorMaterials)
        {
            Renderer[] renderers = Resources.FindObjectsOfTypeAll<Renderer>()
                .Where(IsLiveSceneComponent)
                .Where(value => value.enabled && value.gameObject.activeInHierarchy)
                .OrderBy(StableComponentId, StringComparer.Ordinal)
                .ToArray();
            if (renderers.Length == 0 || renderers.Length >
                AuditionPvRuntimeWorkloadCaptureSession.MaxStableIdsPerInventory)
                throw new InvalidDataException("Renderer inventory count is outside the Gate limit.");

            var rendererValues = new List<string>(renderers.Length);
            var materialValues = new List<string>();
            Shader internalError = Shader.Find("Hidden/InternalErrorShader");
            foreach (Renderer renderer in renderers)
            {
                string rendererId = StableComponentId(renderer);
                rendererValues.Add(rendererId);
                Material[] materials = renderer.sharedMaterials ?? Array.Empty<Material>();
                for (int slot = 0; slot < materials.Length; slot++)
                {
                    Material material = materials[slot];
                    if (material == null)
                    {
                        nullMaterials++;
                        materialValues.Add(rendererId + "/slot/" + slot + "/null");
                        continue;
                    }

                    if (material.shader == null || !material.shader.isSupported ||
                        internalError != null && material.shader == internalError)
                        errorMaterials++;
                    materialValues.Add(rendererId + "/slot/" + slot + "/" +
                                       StableMaterialId(material));
                }
            }

            rendererIds = SortedUnique(rendererValues, "renderer");
            materialIds = SortedUnique(materialValues, "material-slot");
            if (materialIds.Length == 0)
                throw new InvalidDataException("No material slots were inspected.");
        }

        private static void CaptureHudInventory(
            Camera captureCamera,
            out string[] canvasIds,
            out string[] hudRendererIds,
            out long visibleUi)
        {
            Canvas[] canvases = Resources.FindObjectsOfTypeAll<Canvas>()
                .Where(IsLiveSceneComponent)
                .Where(value => value.enabled && value.gameObject.activeInHierarchy)
                .OrderBy(StableComponentId, StringComparer.Ordinal)
                .ToArray();
            CanvasRenderer[] renderers = Resources.FindObjectsOfTypeAll<CanvasRenderer>()
                .Where(IsLiveSceneComponent)
                .Where(value => value.gameObject.activeInHierarchy)
                .OrderBy(StableComponentId, StringComparer.Ordinal)
                .ToArray();
            canvasIds = SortedUnique(canvases.Select(StableComponentId), "canvas", true);
            hudRendererIds = SortedUnique(renderers.Select(StableComponentId),
                "hud-renderer", true);
            visibleUi = 0;
            foreach (CanvasRenderer renderer in renderers)
            {
                Canvas canvas = renderer.GetComponentInParent<Canvas>();
                if (canvas == null || !CanvasCanReachCamera(canvas, captureCamera)) continue;
                if (!renderer.cull && renderer.GetInheritedAlpha() > 0.0001f) visibleUi++;
            }
        }

        private static bool CanvasCanReachCamera(Canvas canvas, Camera captureCamera)
        {
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay) return true;
            if (captureCamera == null) return false;
            return (captureCamera.cullingMask & (1 << canvas.gameObject.layer)) != 0;
        }

        private static bool IsLiveSceneComponent(Component value) =>
            value != null && value.gameObject != null && value.gameObject.scene.IsValid() &&
            value.gameObject.scene.isLoaded &&
            (value.hideFlags & HideFlags.HideAndDontSave) == 0;

        private static string[] SortedUnique(
            IEnumerable<string> values,
            string label,
            bool allowEmpty = false)
        {
            string[] result = (values ?? Array.Empty<string>())
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            if (!allowEmpty && result.Length == 0 ||
                result.Length > AuditionPvRuntimeWorkloadCaptureSession
                    .MaxStableIdsPerInventory ||
                result.Any(string.IsNullOrWhiteSpace) ||
                result.Distinct(StringComparer.Ordinal).Count() != result.Length)
                throw new InvalidDataException(label + " stable identity inventory is invalid.");
            return result;
        }

        private static string StableMaterialId(Material material)
        {
            if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(
                    material,
                    out string guid,
                    out long localId) &&
                !string.IsNullOrWhiteSpace(guid) && localId != 0)
                return "material/" + guid + "/" + localId.ToString(CultureInfo.InvariantCulture);

            // Product VFX legitimately instantiate material clones at runtime.  InstanceID and
            // object addresses are forbidden evidence identities, so bind those slots to a
            // deterministic shader provenance/settings signature instead.  Null, unsupported,
            // and InternalErrorShader materials are rejected before this method is reached.
            Shader shader = material.shader ?? throw new InvalidDataException(
                "A runtime material has no shader: " + material.name);
            string shaderIdentity;
            if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(
                    shader,
                    out string shaderGuid,
                    out long shaderLocalId) &&
                !string.IsNullOrWhiteSpace(shaderGuid) && shaderLocalId != 0)
            {
                shaderIdentity = "shader/" + shaderGuid + "/" +
                    shaderLocalId.ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                shaderIdentity = "builtin-shader/" + (shader.name ?? string.Empty);
            }

            string materialName = (material.name ?? string.Empty).Trim();
            const string InstanceSuffix = " (Instance)";
            if (materialName.EndsWith(InstanceSuffix, StringComparison.Ordinal))
                materialName = materialName.Substring(
                    0,
                    materialName.Length - InstanceSuffix.Length);
            string[] keywords = (material.shaderKeywords ?? Array.Empty<string>())
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            string canonical = string.Join("\n", new[]
            {
                shaderIdentity,
                "name=" + materialName,
                "render-queue=" + material.renderQueue.ToString(CultureInfo.InvariantCulture),
                "instancing=" + (material.enableInstancing ? "1" : "0"),
                "double-sided-gi=" + (material.doubleSidedGI ? "1" : "0"),
                "gi-flags=" + ((int)material.globalIlluminationFlags)
                    .ToString(CultureInfo.InvariantCulture),
                "keywords=" + string.Join(",", keywords)
            });
            return "runtime-material-signature/" +
                   AuditionPvSha256.TextHash(canonical);
        }

        internal static string StableMaterialIdForTest(Material material) =>
            StableMaterialId(material ?? throw new ArgumentNullException(nameof(material)));

        private static string StableComponentId(Component component)
        {
            GlobalObjectId global = GlobalObjectId.GetGlobalObjectIdSlow(component);
            string globalText = global.ToString();
            if (!string.IsNullOrWhiteSpace(globalText) && globalText != InvalidGlobalObjectId)
                return component.GetType().FullName + "/global/" + globalText;

            Component source = PrefabUtility.GetCorrespondingObjectFromSource(component);
            if (source != null)
            {
                string sourceGlobal = GlobalObjectId.GetGlobalObjectIdSlow(source).ToString();
                if (!string.IsNullOrWhiteSpace(sourceGlobal) &&
                    sourceGlobal != InvalidGlobalObjectId)
                    return component.GetType().FullName + "/prefab-global/" + sourceGlobal +
                           "/instance-slot/" + StableHierarchySlot(component);
            }

            // Runtime-only scene objects have no asset GlobalObjectId. The exact loaded scene
            // GUID plus sibling/component ordinals is deterministic for the seeded capture and
            // remains independent of transient InstanceID values.
            return component.GetType().FullName + "/scene-slot/" +
                   StableHierarchySlot(component);
        }

        private static string StableHierarchySlot(Component component)
        {
            Scene scene = component.gameObject.scene;
            string sceneGuid = string.IsNullOrWhiteSpace(scene.path)
                ? "unsaved-" + scene.name
                : AssetDatabase.AssetPathToGUID(scene.path);
            var slots = new Stack<int>();
            Transform current = component.transform;
            while (current != null)
            {
                slots.Push(current.GetSiblingIndex());
                current = current.parent;
            }
            Component[] peers = component.GetComponents(component.GetType());
            int componentOrdinal = Array.IndexOf(peers, component);
            if (componentOrdinal < 0)
                throw new InvalidDataException("Component ordinal could not be resolved.");
            return sceneGuid + "/" + string.Join(".", slots) + "/c" +
                   componentOrdinal.ToString(CultureInfo.InvariantCulture);
        }
    }
}
