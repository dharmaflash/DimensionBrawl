using UnityEngine;

namespace DimensionBrawl.Presentation
{
    public static class PerfectDodgeScreenDomainRuntime
    {
        private static readonly int DomainColorId = Shader.PropertyToID("_DomainColor");
        private static readonly int EdgeColorId = Shader.PropertyToID("_EdgeColor");
        private static readonly int InvertColorId = Shader.PropertyToID("_InvertColor");
        private static readonly int DomainAlphaId = Shader.PropertyToID("_DomainAlpha");
        private static readonly int InvertAlphaId = Shader.PropertyToID("_InvertAlpha");
        private static readonly int EdgeAlphaId = Shader.PropertyToID("_EdgeAlpha");
        private static readonly int BandAlphaId = Shader.PropertyToID("_BandAlpha");
        private static readonly int IntensityId = Shader.PropertyToID("_Intensity");
        private static readonly int SustainId = Shader.PropertyToID("_Sustain");
        private static readonly int AgeId = Shader.PropertyToID("_Age01");
        private static readonly int PulseId = Shader.PropertyToID("_Pulse");
        private static readonly int RadialWarpId = Shader.PropertyToID("_RadialWarp");
        private static readonly int RadialBlurStrengthId = Shader.PropertyToID("_RadialBlurStrength");
        private static readonly int ScanlineStrengthId = Shader.PropertyToID("_ScanlineStrength");
        private static readonly int GridStrengthId = Shader.PropertyToID("_GridStrength");
        private static readonly int FractureStrengthId = Shader.PropertyToID("_FractureStrength");
        private static readonly int ChromaticStrengthId = Shader.PropertyToID("_ChromaticStrength");
        private static readonly int TimeSecondsId = Shader.PropertyToID("_TimeSeconds");
        private static readonly int ScreenSizeId = Shader.PropertyToID("_CueScreenSize");
        private static readonly int DomainCenterId = Shader.PropertyToID("_DomainCenter");

        private static bool active;
        private static Color domainColor = new Color(0.025f, 0.035f, 0.045f, 1f);
        private static Color edgeColor = new Color(0.08f, 0.95f, 1f, 1f);
        private static Color invertColor = new Color(0.92f, 1f, 1f, 1f);
        private static float domainAlpha;
        private static float invertAlpha;
        private static float edgeAlpha;
        private static float bandAlpha;
        private static float intensity;
        private static float sustain;
        private static float age01;
        private static float pulse;
        private static float radialWarp;
        private static float radialBlurStrength;
        private static float scanlineStrength;
        private static float gridStrength;
        private static float fractureStrength;
        private static float chromaticStrength = 1f;
        private static float timeSeconds;
        private static Vector2 domainCenter = new Vector2(0.5f, 0.5f);

        public static bool HasActiveCue => active && sustain > 0.001f && intensity > 0.001f;

        public static void Publish(
            Color domain,
            Color edge,
            Color invert,
            float domainA,
            float invertA,
            float edgeA,
            float bandA,
            float shaderIntensity,
            float sustain01,
            float age,
            float openingPulse,
            float radialWarpStrength,
            float radialBlur,
            float scanline,
            float grid,
            float fracture,
            float chromatic,
            Vector2 center,
            float time)
        {
            active = sustain01 > 0.001f && shaderIntensity > 0.001f;
            domainColor = domain;
            edgeColor = edge;
            invertColor = invert;
            domainAlpha = Mathf.Clamp01(domainA);
            invertAlpha = Mathf.Clamp01(invertA);
            edgeAlpha = Mathf.Clamp01(edgeA);
            bandAlpha = Mathf.Clamp01(bandA);
            intensity = Mathf.Max(0f, shaderIntensity);
            sustain = Mathf.Clamp01(sustain01);
            age01 = Mathf.Clamp01(age);
            pulse = Mathf.Clamp01(openingPulse);
            radialWarp = Mathf.Clamp01(radialWarpStrength);
            radialBlurStrength = Mathf.Clamp01(radialBlur);
            scanlineStrength = Mathf.Clamp01(scanline);
            gridStrength = Mathf.Clamp01(grid);
            fractureStrength = Mathf.Clamp01(fracture);
            chromaticStrength = Mathf.Clamp01(chromatic);
            domainCenter = new Vector2(Mathf.Clamp01(center.x), Mathf.Clamp01(center.y));
            timeSeconds = time;
        }

        public static void Clear()
        {
            active = false;
            sustain = 0f;
            intensity = 0f;
            domainAlpha = 0f;
            invertAlpha = 0f;
            edgeAlpha = 0f;
            bandAlpha = 0f;
            pulse = 0f;
            radialBlurStrength = 0f;
            gridStrength = 0f;
            fractureStrength = 0f;
            chromaticStrength = 0f;
            domainCenter = new Vector2(0.5f, 0.5f);
        }

        public static void ApplyToMaterial(Material material, int width, int height)
        {
            if (material == null)
            {
                return;
            }

            if (!HasActiveCue)
            {
                material.SetFloat(SustainId, 0f);
                material.SetFloat(IntensityId, 0f);
                return;
            }

            material.SetColor(DomainColorId, domainColor);
            material.SetColor(EdgeColorId, edgeColor);
            material.SetColor(InvertColorId, invertColor);
            material.SetFloat(DomainAlphaId, domainAlpha);
            material.SetFloat(InvertAlphaId, invertAlpha);
            material.SetFloat(EdgeAlphaId, edgeAlpha);
            material.SetFloat(BandAlphaId, bandAlpha);
            material.SetFloat(IntensityId, intensity);
            material.SetFloat(SustainId, sustain);
            material.SetFloat(AgeId, age01);
            material.SetFloat(PulseId, pulse);
            material.SetFloat(RadialWarpId, radialWarp);
            material.SetFloat(RadialBlurStrengthId, radialBlurStrength);
            material.SetFloat(ScanlineStrengthId, scanlineStrength);
            material.SetFloat(GridStrengthId, gridStrength);
            material.SetFloat(FractureStrengthId, fractureStrength);
            material.SetFloat(ChromaticStrengthId, chromaticStrength);
            material.SetFloat(TimeSecondsId, timeSeconds);
            material.SetVector(ScreenSizeId, new Vector4(Mathf.Max(1, width), Mathf.Max(1, height), 0f, 0f));
            material.SetVector(DomainCenterId, new Vector4(domainCenter.x, domainCenter.y, 0f, 0f));
        }
    }
}
