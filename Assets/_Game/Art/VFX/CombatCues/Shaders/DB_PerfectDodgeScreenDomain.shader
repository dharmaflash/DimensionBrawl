Shader "DimensionBrawl/CombatCues/PerfectDodgeScreenDomain"
{
    Properties
    {
        _DomainColor ("Domain Color", Color) = (0.035, 0.045, 0.055, 1)
        _EdgeColor ("Edge Color", Color) = (0.08, 0.95, 1, 1)
        _InvertColor ("Invert Color", Color) = (0.92, 1, 1, 1)
        _DomainAlpha ("Domain Alpha", Range(0, 1)) = 0.42
        _InvertAlpha ("Invert Alpha", Range(0, 1)) = 0.18
        _EdgeAlpha ("Edge Alpha", Range(0, 1)) = 0.48
        _BandAlpha ("Band Alpha", Range(0, 1)) = 0.13
        _Intensity ("Intensity", Range(0, 2)) = 0.92
        _Sustain ("Sustain", Range(0, 1)) = 0
        _Age01 ("Age", Range(0, 1)) = 0
        _Pulse ("Pulse", Range(0, 1)) = 0
        _RadialWarp ("Radial Warp", Range(0, 1)) = 0.72
        _RadialBlurStrength ("Radial Blur Strength", Range(0, 1)) = 0.54
        _ScanlineStrength ("Scanline Strength", Range(0, 1)) = 0.34
        _GridStrength ("Grid Strength", Range(0, 1)) = 0.68
        _FractureStrength ("Fracture Strength", Range(0, 1)) = 0.74
        _ChromaticStrength ("Chromatic Strength", Range(0, 1)) = 0.86
        _TimeSeconds ("Cue Time", Float) = 0
        _CueScreenSize ("Screen Size", Vector) = (1920, 1080, 0, 0)
        _DomainCenter ("Domain Center", Vector) = (0.5, 0.5, 0, 0)
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }

        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            Name "PerfectDodgeScreenDomain"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _DomainColor;
                half4 _EdgeColor;
                half4 _InvertColor;
                half _DomainAlpha;
                half _InvertAlpha;
                half _EdgeAlpha;
                half _BandAlpha;
                half _Intensity;
                half _Sustain;
                half _Age01;
                half _Pulse;
                half _RadialWarp;
                half _RadialBlurStrength;
                half _ScanlineStrength;
                half _GridStrength;
                half _FractureStrength;
                half _ChromaticStrength;
                float _TimeSeconds;
                float4 _CueScreenSize;
                float4 _DomainCenter;
            CBUFFER_END

            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 345.45));
                p += dot(p, p + 34.345);
                return frac(p.x * p.y);
            }

            half SoftRing(float distanceFromCenter, float radius, float width)
            {
                return (half)(1.0 - smoothstep(0.0, width, abs(distanceFromCenter - radius)));
            }

            float2 ClampScreenUv(float2 uv)
            {
                return clamp(uv, _BlitTexture_TexelSize.xy * 1.5, 1.0 - _BlitTexture_TexelSize.xy * 1.5);
            }

            half3 SampleSceneColor(float2 uv)
            {
                return SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, ClampScreenUv(uv)).rgb;
            }

            half4 Frag(Varyings input) : SV_Target0
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.texcoord.xy;
                half sustain = saturate(_Sustain);
                if (sustain <= 0.001h)
                {
                    return half4(SampleSceneColor(uv), 1.0h);
                }

                float aspect = max(_CueScreenSize.x / max(_CueScreenSize.y, 1.0), 1.0);
                float2 domainCenter = clamp(_DomainCenter.xy, float2(0.08, 0.08), float2(0.92, 0.92));
                float2 centered = (uv - domainCenter) * float2(aspect, 1.0);
                float dist = length(centered);
                float angle = atan2(centered.y, centered.x);
                float t = _TimeSeconds;

                half pulse = saturate(_Pulse);
                half age = saturate(_Age01);
                half entrySnap = (half)saturate(1.0 - age * 2.8);
                half exitLift = (half)saturate(1.0 - pow(age, 2.2) * 0.38);

                half vignette = (half)smoothstep(0.24, 0.86, dist);
                half core = (half)(1.0 - smoothstep(0.0, 0.36, dist));
                half outerFog = (half)smoothstep(0.34, 0.94, dist);

                half radialRays =
                    (half)(0.5 + 0.5 * sin(angle * 36.0 + t * 10.0 + sin(dist * 28.0 - t * 6.0)));
                radialRays = pow(radialRays, 5.0) * (half)smoothstep(0.08, 0.78, dist) * _RadialWarp;

                float jitterBand = floor(uv.y * 30.0 + t * 11.0);
                half sliceNoise = (half)Hash21(float2(jitterBand, floor(t * 19.0)));
                half sliceGate = step(0.80, sliceNoise);
                half sliceMask = (half)(1.0 - smoothstep(0.0, 0.010, abs(frac(uv.y * 14.0 + sliceNoise) - 0.5)));

                half scan =
                    (half)(0.5 + 0.5 * sin((uv.y * _CueScreenSize.y * 0.82 + t * 170.0) * 0.105));
                scan = pow(scan, 4.0) * _ScanlineStrength;

                half pulseRing = SoftRing(dist, lerp(0.12, 0.84, 1.0 - pulse), 0.040 + pulse * 0.045);
                half afterRing = SoftRing(dist, 0.30 + age * 0.42, 0.014 + pulse * 0.024);
                half thinRing = SoftRing(dist, 0.58 + sin(t * 2.8) * 0.035, 0.010);
                half ringMask = saturate(pulseRing * 0.82 + afterRing * 0.64 + thinRing * 0.28);

                float2 gridUv = centered * (7.4 + pulse * 4.0);
                half gridA = (half)pow(1.0 - abs(sin((gridUv.x * 0.866 + gridUv.y * 0.5) * 7.0 + t * 0.9)), 18.0);
                half gridB = (half)pow(1.0 - abs(sin((gridUv.x * -0.866 + gridUv.y * 0.5) * 7.0 - t * 0.7)), 18.0);
                half gridC = (half)pow(1.0 - abs(sin(gridUv.y * 7.0 + t * 0.45)), 18.0);
                half hexGrid = saturate((gridA + gridB + gridC) * 0.42) * _GridStrength;
                hexGrid *= (half)smoothstep(0.05, 0.78, dist) * (half)(1.0 - smoothstep(0.86, 1.15, dist));

                half fracture =
                    (half)pow(0.5 + 0.5 * sin(angle * 9.0 + dist * 58.0 - t * 8.0), 18.0);
                fracture *= (half)pow(0.5 + 0.5 * sin(angle * -5.0 + dist * 37.0 + t * 5.6), 9.0);
                fracture *= _FractureStrength * (half)smoothstep(0.12, 0.82, dist);

                half domainAlpha =
                    _DomainAlpha * sustain * exitLift * (0.40 + vignette * 0.52 + outerFog * 0.18);
                half edgeAlpha =
                    _EdgeAlpha * sustain * (vignette * 0.56 + pulseRing * 0.72 + afterRing * 0.36 + thinRing * 0.24);
                half invertAlpha =
                    _InvertAlpha * sustain * (pulseRing * 0.78 + afterRing * 0.52 + scan * 0.28 + sliceGate * sliceMask * 0.42);
                half bandAlpha =
                    _BandAlpha * sustain * (radialRays * 0.62 + scan * 0.16);

                float2 radialDir = dist > 0.0001 ? centered / dist / float2(aspect, 1.0) : float2(0.0, 0.0);
                float wave = sin(dist * 46.0 - t * 18.0) * 0.5 + 0.5;
                float shock = (pulseRing * 0.016 + afterRing * 0.007 + radialRays * 0.0025) * _RadialWarp * sustain;
                float sliceShift = (sliceGate * sliceMask) * (Hash21(float2(jitterBand, floor(t * 33.0))) - 0.5) * 0.010 * sustain;
                float2 warpedUv = uv + radialDir * shock * (0.45 + wave * 0.55) + float2(sliceShift, 0.0);

                float chroma = (0.0015 + edgeAlpha * 0.010 + radialRays * 0.004 + fracture * 0.003) * sustain * _ChromaticStrength;
                float2 chromaDir = dist > 0.0001 ? normalize(centered) / float2(aspect, 1.0) : float2(1.0, 0.0);
                half3 scene;
                scene.r = SampleSceneColor(warpedUv + chromaDir * chroma).r;
                scene.g = SampleSceneColor(warpedUv).g;
                scene.b = SampleSceneColor(warpedUv - chromaDir * chroma).b;

                float blurStep = (0.002 + pulseRing * 0.012 + afterRing * 0.006 + radialRays * 0.003)
                    * _RadialBlurStrength * sustain;
                half3 radialBlur =
                    SampleSceneColor(warpedUv - radialDir * blurStep * 0.8)
                    + SampleSceneColor(warpedUv - radialDir * blurStep * 1.6)
                    + SampleSceneColor(warpedUv + radialDir * blurStep * 0.55)
                    + SampleSceneColor(warpedUv + radialDir * blurStep * 1.15);
                radialBlur *= 0.25h;
                half blurMix = saturate((pulseRing * 0.58h + afterRing * 0.32h + vignette * 0.18h) * _RadialBlurStrength);
                scene = lerp(scene, radialBlur, blurMix);

                half luma = dot(scene, half3(0.2126h, 0.7152h, 0.0722h));
                half desatAmount = saturate(domainAlpha * 0.34 + outerFog * sustain * 0.12);
                half3 graded = lerp(scene, luma.xxx, desatAmount);
                half3 coldDomain = lerp(_DomainColor.rgb, half3(0.012h, 0.10h, 0.16h), hexGrid * 0.75h);
                graded = lerp(graded, graded * (1.0h - domainAlpha * 0.16h) + coldDomain * domainAlpha, domainAlpha);

                half3 edgeColor = _EdgeColor.rgb * (edgeAlpha * 1.06h + bandAlpha * 0.72h + ringMask * 0.34h + hexGrid * 0.10h);
                half3 invertColor = lerp(graded, 1.0h - graded, saturate(invertAlpha * (0.42h + entrySnap * 0.18h)));
                half invertMix = saturate(invertAlpha * 0.82h + core * pulse * 0.08h + fracture * 0.06h);
                half3 color = lerp(graded + edgeColor, invertColor + edgeColor * 0.62h, invertMix);

                half3 sliceTint = _EdgeColor.rgb * sliceGate * sliceMask * sustain * 0.06h;
                color += sliceTint
                    + _EdgeColor.rgb * radialRays * sustain * 0.08h
                    + _EdgeColor.rgb * hexGrid * sustain * 0.12h
                    + _InvertColor.rgb * fracture * sustain * 0.055h;
                color = lerp(color, _InvertColor.rgb, core * pulse * 0.055h);

                half effectAlpha = saturate(
                    domainAlpha
                    + edgeAlpha * 0.58h
                    + invertAlpha * 0.54h
                    + bandAlpha * 0.38h
                    + hexGrid * 0.22h
                    + fracture * 0.20h);
                color = lerp(scene, color, saturate(effectAlpha * _Intensity));
                return half4(color, 1.0h);
            }
            ENDHLSL
        }
    }

    Fallback Off
}
