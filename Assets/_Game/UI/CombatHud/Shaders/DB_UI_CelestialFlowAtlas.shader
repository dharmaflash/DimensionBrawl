Shader "DimensionBrawl/UI/CelestialFlowAtlas"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [NoScaleOffset] _FlowAtlas ("Flow Frame Atlas", 2D) = "black" {}
        _FlowTint ("Flow Tint", Color) = (0.8,0.95,1,0.2)
        _FlowStrength ("Flow Strength", Range(0,0.15)) = 0.04
        _AtlasColumns ("Atlas Columns", Float) = 4
        _AtlasRows ("Atlas Rows", Float) = 3
        _FrameCount ("Frame Count", Float) = 12
        _FramesPerSecond ("Frames Per Second", Float) = 8
        _PhaseOffset ("Phase Offset", Float) = 0
        [Toggle] _PingPong ("Ping Pong", Float) = 1
        _FlowUvScale ("Flow UV Scale", Vector) = (1,1,0,0)

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
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

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            sampler2D _FlowAtlas;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            fixed4 _FlowTint;
            float4 _ClipRect;
            float4 _FlowAtlas_TexelSize;
            float4 _FlowUvScale;
            float _FlowStrength;
            float _AtlasColumns;
            float _AtlasRows;
            float _FrameCount;
            float _FramesPerSecond;
            float _PhaseOffset;
            float _PingPong;

            v2f vert(appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.worldPosition = v.vertex;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                o.color = v.color * _Color;
                return o;
            }

            float ResolveFrameIndex()
            {
                float frameCount = max(1.0, floor(_FrameCount + 0.5));
                float rawFrame = floor(_Time.y * max(0.0, _FramesPerSecond) + _PhaseOffset);
                if (_PingPong > 0.5 && frameCount > 1.0)
                {
                    float lastFrame = frameCount - 1.0;
                    float period = lastFrame * 2.0;
                    float wrapped = rawFrame - floor(rawFrame / period) * period;
                    return lastFrame - abs(wrapped - lastFrame);
                }

                return rawFrame - floor(rawFrame / frameCount) * frameCount;
            }

            float2 ResolveFlowUv(float2 localUv)
            {
                float columns = max(1.0, floor(_AtlasColumns + 0.5));
                float rows = max(1.0, floor(_AtlasRows + 0.5));
                float frameIndex = ResolveFrameIndex();
                float column = frameIndex - floor(frameIndex / columns) * columns;
                float row = floor(frameIndex / columns);
                float2 cellSize = 1.0 / float2(columns, rows);
                float2 halfTexel = _FlowAtlas_TexelSize.xy * 0.5;
                float2 scaledUv = saturate((localUv - 0.5) * _FlowUvScale.xy + 0.5);
                float2 cellOrigin = float2(column * cellSize.x, 1.0 - (row + 1.0) * cellSize.y);
                return cellOrigin + halfTexel + scaledUv * max(cellSize - halfTexel * 2.0, halfTexel);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 color = (tex2D(_MainTex, i.texcoord) + _TextureSampleAdd) * i.color;
                fixed3 flowSample = tex2D(_FlowAtlas, ResolveFlowUv(i.texcoord)).rgb;
                fixed flowLuminance = dot(flowSample, fixed3(0.299, 0.587, 0.114));
                fixed modulation = lerp(1.0 - _FlowStrength * 0.35, 1.0 + _FlowStrength, flowLuminance);
                color.rgb = saturate(color.rgb * modulation);
                color.rgb = saturate(
                    color.rgb
                    + _FlowTint.rgb * flowLuminance * _FlowTint.a * _FlowStrength * 0.35 * color.a);

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(color.a - 0.001);
                #endif

                return color;
            }
            ENDCG
        }
    }
}
