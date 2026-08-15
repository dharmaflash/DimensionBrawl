using System;
using System.IO;
using System.Linq;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;
using UnityEngine;

namespace DimensionBrawl.Editor.AuditionPV
{
    internal sealed class AuditionPvRecorderSettingsBundle : IDisposable
    {
        public RecorderControllerSettings controllerSettings;
        public ImageRecorderSettings imageSettings;
        public string normalizedShotId = string.Empty;

        public void Dispose()
        {
            if (imageSettings != null)
            {
                UnityEngine.Object.DestroyImmediate(imageSettings);
                imageSettings = null;
            }

            if (controllerSettings != null)
            {
                UnityEngine.Object.DestroyImmediate(controllerSettings);
                controllerSettings = null;
            }
        }
    }

    internal static class AuditionPvRecorderSettingsFactory
    {
        public static AuditionPvRecorderSettingsBundle CreateLosslessPngSequence(
            string outputDirectory,
            string shotId)
        {
            if (string.IsNullOrWhiteSpace(outputDirectory) || !Path.IsPathRooted(outputDirectory))
            {
                throw new ArgumentException("Recorder output directory must be an absolute path.", nameof(outputDirectory));
            }

            string normalizedShotId = AuditionPvOutputPaths.SanitizeSegment(shotId);
            string frameDirectory = Path.Combine(Path.GetFullPath(outputDirectory), "frames", normalizedShotId);
            string outputPattern = Path.Combine(frameDirectory, "frame_").Replace('\\', '/') + DefaultWildcard.Frame;

            var controllerSettings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
            controllerSettings.name = "Audition PV Golden Source 1440p60";
            controllerSettings.FrameRatePlayback = FrameRatePlayback.Constant;
            controllerSettings.FrameRate = AuditionPvCaptureContract.Fps;
            controllerSettings.CapFrameRate = true;
            controllerSettings.ExitPlayMode = false;
            controllerSettings.SetRecordModeToManual();

            var imageSettings = ScriptableObject.CreateInstance<ImageRecorderSettings>();
            imageSettings.name = "Audition PV Lossless PNG Sequence";
            imageSettings.Enabled = true;
            imageSettings.OutputFormat = ImageRecorderSettings.ImageRecorderOutputFormat.PNG;
            imageSettings.OutputColorSpace = ImageRecorderSettings.ColorSpaceType.sRGB_sRGB;
            imageSettings.imageInputSettings = new GameViewInputSettings
            {
                OutputWidth = AuditionPvCaptureContract.Width,
                OutputHeight = AuditionPvCaptureContract.Height,
                FlipFinalOutput = false
            };
            imageSettings.CaptureAlpha = false;
            imageSettings.OutputFile = outputPattern;
            controllerSettings.AddRecorderSettings(imageSettings);

            return new AuditionPvRecorderSettingsBundle
            {
                controllerSettings = controllerSettings,
                imageSettings = imageSettings,
                normalizedShotId = normalizedShotId
            };
        }

        public static void Validate(AuditionPvRecorderSettingsBundle bundle)
        {
            if (bundle?.controllerSettings == null || bundle.imageSettings == null)
            {
                throw new InvalidOperationException("Recorder settings bundle is incomplete.");
            }

            if (bundle.controllerSettings.FrameRatePlayback != FrameRatePlayback.Constant ||
                Math.Abs(bundle.controllerSettings.FrameRate - AuditionPvCaptureContract.Fps) > 0.001f ||
                !bundle.controllerSettings.CapFrameRate)
            {
                throw new InvalidOperationException("Recorder controller is not configured for deterministic constant 60 fps capture.");
            }

            RecorderSettings[] recorders = bundle.controllerSettings.RecorderSettings.ToArray();
            if (recorders.Length != 1 || !ReferenceEquals(recorders[0], bundle.imageSettings))
            {
                throw new InvalidOperationException("Recorder controller must contain exactly one golden-source image recorder.");
            }

            if (!bundle.imageSettings.Enabled ||
                bundle.imageSettings.OutputFormat != ImageRecorderSettings.ImageRecorderOutputFormat.PNG ||
                bundle.imageSettings.CaptureAlpha)
            {
                throw new InvalidOperationException("Golden-source recorder must use opaque lossless PNG output.");
            }

            if (bundle.imageSettings.imageInputSettings is not GameViewInputSettings gameViewInput ||
                gameViewInput.OutputWidth != AuditionPvCaptureContract.Width ||
                gameViewInput.OutputHeight != AuditionPvCaptureContract.Height ||
                gameViewInput.FlipFinalOutput)
            {
                throw new InvalidOperationException("Golden-source recorder must use an unflipped 2560x1440 Game View input.");
            }

            if (!bundle.imageSettings.OutputFile.Contains(DefaultWildcard.Frame, StringComparison.Ordinal) ||
                !bundle.imageSettings.OutputFile.Replace('\\', '/').Contains("/frames/", StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Golden-source output path must be a per-shot frame sequence.");
            }
        }
    }
}
