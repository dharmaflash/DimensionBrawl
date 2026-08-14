using System;
using UnityEngine;
using UnityEngine.Playables;

namespace DimensionBrawl.Presentation
{
    /// <summary>
    /// Recreates the shot-local C33/C34 light ownership without leaking it into
    /// Station gameplay. The legacy scene isolated its character and background
    /// keys on separate layers; this lease provides the same deterministic result
    /// while the transition root is active, then restores every Station light.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(10120)]
    public sealed class AkazaPhase2CinematicLookDriver : MonoBehaviour
    {
        [SerializeField] private PlayableDirector director;
        [SerializeField] private Light wingDeployKey;
        [SerializeField] private Light eyeOpenKey;
        [SerializeField] private Light backgroundKey;
        [SerializeField] private Light[] suppressedDirectionalLights = Array.Empty<Light>();
        [SerializeField, Min(0f)] private double eyeOpenStartSeconds = 1.6d;
        [Header("Legacy PPv1 Fog Parity")]
        [SerializeField] private bool applyCinematicFog = true;
        [SerializeField] private Color cinematicFogColor =
            new Color(0.2941f, 0.2197f, 0.1081f, 0.604f);
        [SerializeField] private FogMode cinematicFogMode = FogMode.Linear;
        [SerializeField] private float cinematicFogDensity = 0.13f;
        [SerializeField] private float cinematicFogStartDistance = -30.1f;
        [SerializeField] private float cinematicFogEndDistance = 600f;

        private bool lightingLeaseHeld;
        private bool[] suppressedLightEnabledStates = Array.Empty<bool>();
        private bool previousFogEnabled;
        private Color previousFogColor;
        private FogMode previousFogMode;
        private float previousFogDensity;
        private float previousFogStartDistance;
        private float previousFogEndDistance;

        public bool LightingLeaseHeld => lightingLeaseHeld;
        public int SuppressedDirectionalLightCount => suppressedDirectionalLights?.Length ?? 0;
        public Light WingDeployKey => wingDeployKey;
        public Light EyeOpenKey => eyeOpenKey;
        public Light BackgroundKey => backgroundKey;
        public double EyeOpenStartSeconds => eyeOpenStartSeconds;
        public bool AppliesCinematicFog => applyCinematicFog;
        public Color CinematicFogColor => cinematicFogColor;
        public FogMode CinematicFogMode => cinematicFogMode;
        public float CinematicFogStartDistance => cinematicFogStartDistance;
        public float CinematicFogEndDistance => cinematicFogEndDistance;

        public void BeginManualLightingLease()
        {
            AcquireLightingLease();
            ApplyCurrentTime();
        }

        public void EndManualLightingLease()
        {
            ReleaseLightingLease();
        }

        public void Configure(
            PlayableDirector sourceDirector,
            Light sourceWingDeployKey,
            Light sourceEyeOpenKey,
            Light sourceBackgroundKey,
            Light[] sourceSuppressedDirectionalLights,
            double sourceEyeOpenStartSeconds,
            bool sourceApplyCinematicFog,
            Color sourceFogColor,
            FogMode sourceFogMode,
            float sourceFogDensity,
            float sourceFogStartDistance,
            float sourceFogEndDistance)
        {
            ReleaseLightingLease();
            director = sourceDirector;
            wingDeployKey = sourceWingDeployKey;
            eyeOpenKey = sourceEyeOpenKey;
            backgroundKey = sourceBackgroundKey;
            suppressedDirectionalLights = sourceSuppressedDirectionalLights ?? Array.Empty<Light>();
            eyeOpenStartSeconds = Math.Max(0d, sourceEyeOpenStartSeconds);
            applyCinematicFog = sourceApplyCinematicFog;
            cinematicFogColor = sourceFogColor;
            cinematicFogMode = sourceFogMode;
            cinematicFogDensity = Mathf.Max(0f, sourceFogDensity);
            cinematicFogStartDistance = sourceFogStartDistance;
            cinematicFogEndDistance = Mathf.Max(sourceFogStartDistance, sourceFogEndDistance);
            if (Application.isPlaying && isActiveAndEnabled)
            {
                AcquireLightingLease();
                ApplyCurrentTime();
            }
        }

        public void ApplyCurrentTime()
        {
            if (!IsConfigured)
            {
                return;
            }

            bool showEyeOpen = director.time + 0.000001d >= eyeOpenStartSeconds;
            wingDeployKey.enabled = !showEyeOpen;
            eyeOpenKey.enabled = showEyeOpen;
            backgroundKey.enabled = true;
            if (lightingLeaseHeld && applyCinematicFog)
            {
                RenderSettings.fog = true;
                RenderSettings.fogColor = cinematicFogColor;
                RenderSettings.fogMode = cinematicFogMode;
                RenderSettings.fogDensity = cinematicFogDensity;
                RenderSettings.fogStartDistance = cinematicFogStartDistance;
                RenderSettings.fogEndDistance = cinematicFogEndDistance;
            }
        }

        private void OnEnable()
        {
            if (Application.isPlaying)
            {
                AcquireLightingLease();
                ApplyCurrentTime();
            }
        }

        private void LateUpdate()
        {
            ApplyCurrentTime();
        }

        private void OnDisable()
        {
            ReleaseLightingLease();
        }

        private void OnDestroy()
        {
            ReleaseLightingLease();
        }

        private void AcquireLightingLease()
        {
            if (!IsConfigured || lightingLeaseHeld)
            {
                return;
            }

            int count = suppressedDirectionalLights?.Length ?? 0;
            suppressedLightEnabledStates = new bool[count];
            for (int index = 0; index < count; index++)
            {
                Light candidate = suppressedDirectionalLights[index];
                if (candidate == null
                    || candidate == wingDeployKey
                    || candidate == eyeOpenKey
                    || candidate == backgroundKey)
                {
                    continue;
                }

                suppressedLightEnabledStates[index] = candidate.enabled;
                candidate.enabled = false;
            }

            previousFogEnabled = RenderSettings.fog;
            previousFogColor = RenderSettings.fogColor;
            previousFogMode = RenderSettings.fogMode;
            previousFogDensity = RenderSettings.fogDensity;
            previousFogStartDistance = RenderSettings.fogStartDistance;
            previousFogEndDistance = RenderSettings.fogEndDistance;

            lightingLeaseHeld = true;
        }

        private void ReleaseLightingLease()
        {
            if (!lightingLeaseHeld)
            {
                return;
            }

            int count = Mathf.Min(
                suppressedDirectionalLights?.Length ?? 0,
                suppressedLightEnabledStates?.Length ?? 0);
            for (int index = 0; index < count; index++)
            {
                Light candidate = suppressedDirectionalLights[index];
                if (candidate != null
                    && candidate != wingDeployKey
                    && candidate != eyeOpenKey
                    && candidate != backgroundKey)
                {
                    candidate.enabled = suppressedLightEnabledStates[index];
                }
            }

            suppressedLightEnabledStates = Array.Empty<bool>();
            RenderSettings.fog = previousFogEnabled;
            RenderSettings.fogColor = previousFogColor;
            RenderSettings.fogMode = previousFogMode;
            RenderSettings.fogDensity = previousFogDensity;
            RenderSettings.fogStartDistance = previousFogStartDistance;
            RenderSettings.fogEndDistance = previousFogEndDistance;
            lightingLeaseHeld = false;
        }

        private bool IsConfigured => director != null
            && wingDeployKey != null
            && eyeOpenKey != null
            && backgroundKey != null;
    }
}
