Shader "DimensionBrawl/CombatCues/PerfectDodgeAfterimage"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.34, 0.98, 1, 0.42)
        _RimColor ("Rim Color", Color) = (0.72, 0.36, 1, 0.9)
        _Alpha ("Alpha", Range(0, 1)) = 0.42
        _Intensity ("Intensity", Range(0, 4)) = 1
        _Age01 ("Age", Range(0, 1)) = 0
        _FresnelPower ("Fresnel Power", Range(0.5, 8)) = 2.2
        _ScanStrength ("Scan Strength", Range(0, 1)) = 0.48
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
        Cull Back
        ZWrite Off
        ZTest LEqual

        Pass
        {
            Name "PerfectDodgeAfterimage"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _RimColor;
                half _Alpha;
                half _Intensity;
                half _Age01;
                half _FresnelPower;
                half _ScanStrength;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 viewDirWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
                float3 positionWS : TEXCOORD3;
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
                output.positionWS = positionInputs.positionWS;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half age = saturate(_Age01);
                half life = 1.0h - age;
                half fresnel = pow(1.0h - saturate(dot(normalize(input.normalWS), normalize(input.viewDirWS))), _FresnelPower);
                half scan = (half)(0.5 + 0.5 * sin((input.positionWS.y * 9.5 + input.uv.y * 18.0 - _Time.y * 7.0)));
                scan = pow(scan, 5.0) * _ScanStrength;
                half dissolve = saturate(life * 1.35h - scan * 0.22h);
                half alpha = _Alpha * _Intensity * dissolve * (0.24h + fresnel * 0.76h + scan * 0.22h);
                half3 color = _BaseColor.rgb * (0.55h + scan * 0.38h) + _RimColor.rgb * fresnel * 0.88h;
                return half4(color * _Intensity, saturate(alpha));
            }
            ENDHLSL
        }
    }

    Fallback Off
}
