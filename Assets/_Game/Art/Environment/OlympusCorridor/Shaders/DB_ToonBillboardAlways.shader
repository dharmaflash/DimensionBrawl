Shader "DimensionBrawl/Lookdev/ToonBillboardAlways"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        _EdgeColor ("Edge Color", Color) = (1, 1, 1, 1)
        _Shape ("Shape", Float) = 0
        _GlowPower ("Glow Power", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent+80"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "ToonBillboardAlways"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest Always
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _EdgeColor;
                half _Shape;
                half _GlowPower;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half Ellipse(float2 p, float2 center, float2 radius, half feather)
            {
                float2 q = (p - center) / radius;
                return 1.0 - smoothstep(1.0, 1.0 + feather, dot(q, q));
            }

            half Band(half value, half center, half width, half feather)
            {
                return 1.0 - smoothstep(width, width + feather, abs(value - center));
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 p = input.uv * 2.0f - 1.0f;
                half alpha = 0.0;
                half edge = 0.0;

                if (_Shape < 0.5)
                {
                    half r = length(p);
                    half outerRing = Band(r, 0.72, 0.035, 0.055);
                    half innerRing = Band(r, 0.43, 0.02, 0.045);
                    half diamondRing = Band(abs(p.x) + abs(p.y), 0.82, 0.025, 0.055);
                    half verticalBolt = (1.0 - smoothstep(0.035, 0.12, abs(p.x))) * (1.0 - smoothstep(0.65, 1.0, abs(p.y)));
                    half haze = (1.0 - smoothstep(0.0, 0.95, r)) * 0.16;
                    alpha = saturate(max(max(outerRing, innerRing * 0.72), max(diamondRing * 0.58, verticalBolt * 0.52)) + haze);
                    edge = saturate(max(outerRing, diamondRing) + verticalBolt * 0.35);
                }
                else if (_Shape < 1.5)
                {
                    half head = Ellipse(p, float2(0.0f, 0.55f), float2(0.22f, 0.22f), 0.18);
                    half chest = Ellipse(p, float2(0.0f, 0.05f), float2(0.34f, 0.58f), 0.18);
                    half hips = Ellipse(p, float2(0.0f, -0.45f), float2(0.24f, 0.25f), 0.18);
                    half leftArm = Ellipse(p, float2(-0.37f, -0.05f), float2(0.14f, 0.55f), 0.2);
                    half rightArm = Ellipse(p, float2(0.37f, -0.05f), float2(0.14f, 0.55f), 0.2);
                    half horns = max(Band(abs(p.x) + p.y, 0.74, 0.035, 0.05), Band(abs(p.x) - p.y, 0.58, 0.025, 0.05)) * smoothstep(0.2, 0.72, p.y);
                    alpha = saturate(max(max(head, chest), max(max(hips, max(leftArm, rightArm)), horns * 0.86)));
                    edge = saturate(Band(abs(p.x) + abs(p.y * 0.72), 0.92, 0.035, 0.07) + head * 0.22);
                }
                else if (_Shape < 2.5)
                {
                    half diamond = 1.0 - smoothstep(0.62, 0.72, abs(p.x) + abs(p.y));
                    half diamondEdge = Band(abs(p.x) + abs(p.y), 0.68, 0.035, 0.055);
                    half core = 1.0 - smoothstep(0.0, 0.36, length(p));
                    alpha = saturate(max(diamond * 0.62, max(diamondEdge, core * 0.8)));
                    edge = saturate(diamondEdge + core * 0.45);
                }
                else
                {
                    half spine = 1.0 - smoothstep(0.045, 0.14, abs(p.y + sin(p.x * 7.0f) * 0.08));
                    half forkA = Band(p.y + p.x * 0.35, 0.04, 0.035, 0.075) * smoothstep(-0.85, -0.1, p.x);
                    half forkB = Band(p.y - p.x * 0.42, -0.03, 0.03, 0.07) * smoothstep(0.05, 0.82, p.x);
                    half taper = 1.0 - smoothstep(0.82, 1.0, abs(p.x));
                    alpha = saturate(max(spine, max(forkA, forkB)) * taper);
                    edge = saturate(alpha);
                }

                half visibleAlpha = saturate(alpha * _BaseColor.a);
                clip(visibleAlpha - 0.012);

                half3 color = lerp(_BaseColor.rgb, _EdgeColor.rgb, saturate(edge));
                color += _BaseColor.rgb * edge * _GlowPower * 0.45;
                return half4(color, visibleAlpha);
            }
            ENDHLSL
        }
    }

    Fallback "Sprites/Default"
}
