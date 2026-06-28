Shader "DimensionBrawl/UI/FirstPersonGlitchOverlay"
{
    Properties
    {
        _Tint ("Tint", Color) = (0.58, 0.92, 1.0, 1.0)
        _Alpha ("Alpha", Range(0, 1)) = 0
        _NoiseStrength ("Noise Strength", Range(0, 2)) = 1
        _ScanlineStrength ("Scanline Strength", Range(0, 1)) = 0.35
        _JitterStrength ("Jitter Strength", Range(0, 1)) = 0.25
        _Phase ("Phase", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            fixed4 _Tint;
            float _Alpha;
            float _NoiseStrength;
            float _ScanlineStrength;
            float _JitterStrength;
            float _Phase;

            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                o.uv = v.texcoord;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float coarseNoise = Hash21(floor(float2(uv.x * 220.0 + _Phase * 9.0, uv.y * 120.0)));
                float fineNoise = Hash21(floor(float2(uv.x * 840.0, uv.y * 420.0 + _Phase * 31.0)));
                float scan = pow(saturate(sin((uv.y + _Phase * 0.071) * 960.0) * 0.5 + 0.5), 10.0);
                float band = smoothstep(0.84, 1.0, frac((uv.y * 11.0) + (_Phase * 0.27)));
                float centerFalloff = 1.0 - smoothstep(0.0, 0.72, abs(uv.y - 0.5));
                float sideFalloff = smoothstep(0.14, 0.92, abs(uv.x - 0.5) * 2.0);
                float brokenLines = step(0.83, coarseNoise) * (0.35 + sideFalloff * 0.45);
                float jitter = step(0.90, Hash21(float2(floor(uv.y * 56.0), floor(_Phase * 8.0)))) * _JitterStrength;

                float signal =
                    (fineNoise * 0.18 * _NoiseStrength)
                    + (scan * _ScanlineStrength)
                    + (band * 0.22 * _NoiseStrength)
                    + brokenLines
                    + jitter;
                float alpha = saturate(signal) * saturate(_Alpha) * (0.35 + centerFalloff * 0.35 + sideFalloff * 0.30);
                fixed3 color = _Tint.rgb + (fineNoise * 0.18);
                return fixed4(color * i.color.rgb, alpha * i.color.a);
            }
            ENDCG
        }
    }
}
