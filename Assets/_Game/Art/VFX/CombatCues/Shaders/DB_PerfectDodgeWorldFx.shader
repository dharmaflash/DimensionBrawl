Shader "DimensionBrawl/CombatCues/PerfectDodgeWorldFx"
{
    Properties
    {
        _ColorA ("Primary Color", Color) = (0.08, 0.96, 1, 0.82)
        _ColorB ("Secondary Color", Color) = (0.56, 0.24, 1, 0.72)
        _Alpha ("Alpha", Range(0, 1)) = 0.7
        _Intensity ("Intensity", Range(0, 4)) = 1
        _Age01 ("Age", Range(0, 1)) = 0
        _Pulse ("Pulse", Range(0, 1)) = 1
        _RimPower ("Rim Power", Range(0.5, 8)) = 2.8
        _NoiseScale ("Noise Scale", Range(0.1, 16)) = 7
        _LayerMode ("Layer Mode", Range(0, 3)) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Blend SrcAlpha One
        Cull Off
        ZWrite Off
        ZTest LEqual

        Pass
        {
            Name "PerfectDodgeWorldFx"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _ColorA;
                half4 _ColorB;
                half _Alpha;
                half _Intensity;
                half _Age01;
                half _Pulse;
                half _RimPower;
                half _NoiseScale;
                half _LayerMode;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 viewDirWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
                half4 color : COLOR;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS);
                output.positionCS = positionInputs.positionCS;
                output.normalWS = normalize(normalInputs.normalWS);
                output.viewDirWS = GetWorldSpaceViewDir(positionInputs.positionWS);
                output.uv = input.uv;
                output.color = input.color;
                return output;
            }

            half LinePulse(float x, float sharpness)
            {
                return (half)pow(1.0 - abs(frac(x) * 2.0 - 1.0), sharpness);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float age = saturate(_Age01);
                float life = 1.0 - age;
                float radial = saturate(input.uv.x);
                float angle01 = input.uv.y;
                float time = _Time.y;

                half rim = pow(1.0h - saturate(dot(normalize(input.normalWS), normalize(input.viewDirWS))), _RimPower);
                half radialEdge = saturate(smoothstep(0.02, 0.18, radial) * (1.0 - smoothstep(0.82, 1.0, radial)));
                half outerEdge = (half)smoothstep(0.64, 1.0, radial);
                half ticks = LinePulse(angle01 * (24.0 + _LayerMode * 10.0) + time * (0.12 + _LayerMode * 0.07), 18.0);
                half circuitsA = LinePulse((angle01 + radial * 0.21) * 6.0 + time * 0.09, 10.0);
                half circuitsB = LinePulse((angle01 - radial * 0.34) * 9.0 - time * 0.13, 12.0);
                half shimmer = (half)(0.5 + 0.5 * sin((angle01 * 38.0 + radial * _NoiseScale * 8.0 - time * 3.2)));
                shimmer = pow(shimmer, 5.0);

                half pulse = saturate(_Pulse);
                half entry = saturate(1.0h - age * 1.8h);
                half mask = saturate(radialEdge * 0.32h + outerEdge * 0.38h + ticks * 0.46h + circuitsA * 0.18h + circuitsB * 0.16h + rim * 0.34h);
                mask += shimmer * 0.12h + pulse * entry * (outerEdge * 0.28h + ticks * 0.16h);
                mask = saturate(mask);

                half3 color = lerp(_ColorA.rgb, _ColorB.rgb, saturate(circuitsB + rim * 0.32h + _LayerMode * 0.12h));
                color += _ColorA.rgb * (ticks * 0.42h + shimmer * 0.12h);
                color += _ColorB.rgb * circuitsA * 0.18h;

                half alpha = _Alpha * _Intensity * input.color.a * mask * (half)smoothstep(0.0, 0.16, life);
                alpha *= (half)(0.24 + life * 0.76);
                return half4(color * _Intensity, saturate(alpha));
            }
            ENDHLSL
        }
    }

    Fallback Off
}
