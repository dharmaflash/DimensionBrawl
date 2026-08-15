Shader "DimensionBrawl/UI/CelestialFlow"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [NoScaleOffset] _FlowTex ("Seamless Flow Texture", 2D) = "gray" {}
        _FlowTint ("Flow Tint", Color) = (0.72,0.94,1,0.2)
        _FlowStrength ("Flow Strength", Range(0,0.1)) = 0.025
        _FlowTiling ("Flow Tiling XY", Vector) = (1.5,1,0,0)
        _FlowSpeed ("Flow Speed XY", Vector) = (0.012,0,0,0)
        _FlowPhase ("Flow Phase XY", Vector) = (0,0,0,0)

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
            sampler2D _FlowTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            fixed4 _FlowTint;
            float4 _ClipRect;
            float4 _FlowTiling;
            float4 _FlowSpeed;
            float4 _FlowPhase;
            float _FlowStrength;

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

            fixed4 frag(v2f i) : SV_Target
            {
                // Keep the sprite lookup identical to UI/Default. Filled Images therefore
                // retain their generated mesh and UV contract; only _FlowTex is scrolled.
                fixed4 color = (tex2D(_MainTex, i.texcoord) + _TextureSampleAdd) * i.color;

                float2 flowUv = frac(
                    i.texcoord * max(_FlowTiling.xy, float2(0.0001, 0.0001))
                    + _Time.y * _FlowSpeed.xy
                    + _FlowPhase.xy);
                fixed3 flowRgb = tex2D(_FlowTex, flowUv).rgb;
                fixed flowLuminance = dot(flowRgb, fixed3(0.299, 0.587, 0.114));
                fixed signedFlow = (flowLuminance - 0.5) * 2.0;
                fixed positiveFlow = saturate(signedFlow);

                // A mid-gray flow texture is neutral. Strength is intentionally capped at
                // ten percent so this remains ambient surface motion, not a VFX overlay.
                color.rgb *= 1.0 + signedFlow * _FlowStrength;
                color.rgb += _FlowTint.rgb
                    * positiveFlow
                    * _FlowTint.a
                    * _FlowStrength
                    * color.a;

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
