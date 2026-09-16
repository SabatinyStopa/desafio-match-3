Shader "Custom/Particles/StandardUnlitGPU"
{
    Properties
    {
        [Enum(Opaque, 0, Cutout, 1, Fade, 2, Transparent, 3, Additive, 4, Subtractive, 5, Modulate, 6)] 
        _Mode ("Rendering Mode", Float) = 2.0

        [Enum(Multiply, 0, Additive, 1, Subtractive, 2, Overlay, 3, Color, 4, Difference, 5)] 
        _ColorMode ("Color Mode", Float) = 0.0

        _MainTex ("Albedo", 2D) = "white" {}
        [HDR] _Color ("Color", Color) = (1,1,1,1)

        _Cutoff ("Alpha Cutoff", Range(0, 1)) = 0.5
        
        [Toggle(_FLIPBOOK_BLENDING)] _FlipbookBlending ("Flip-Book Frame Blending", Float) = 0.0
        [Toggle(_TWO_SIDED_ON)] _TwoSided ("Two Sided", Float) = 1.0
        [Toggle(_SOFTPARTICLES_ON)] _SoftParticles ("Soft Particles", Float) = 0.0
        [Toggle(_FADING_ON)] _CameraFading ("Camera Fading", Float) = 0.0
        [Toggle(_DISTORTION_ON)] _Distortion ("Distortion", Float) = 0.0

        _SoftParticlesNearFadeDistance ("Soft Particles Near Fade", Float) = 0.0
        _SoftParticlesFarFadeDistance ("Soft Particles Far Fade", Float) = 1.0
        _CameraNearFadeDistance ("Camera Near Fade", Float) = 0.0
        _CameraFarFadeDistance ("Camera Far Fade", Float) = 1.0

        [HideInInspector] _BlendOp ("__blendop", Float) = 0.0
        [HideInInspector] _SrcBlend ("__src", Float) = 1.0
        [HideInInspector] _DstBlend ("__dst", Float) = 0.0
        [HideInInspector] _ZWrite ("__zw", Float) = 1.0
        [HideInInspector] _Cull ("__cull", Float) = 0.0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }

        BlendOp [_BlendOp]
        Blend [_SrcBlend] [_DstBlend]
        Cull [_Cull]
        ZWrite [_ZWrite]
        Lighting Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #pragma multi_compile_instancing
            #pragma shader_feature_local _FLIPBOOK_BLENDING
            #pragma shader_feature_local _SOFTPARTICLES_ON
            #pragma shader_feature_local _FADING_ON
            #pragma shader_feature_local _REQUIRE_UV2

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                #if defined(_FLIPBOOK_BLENDING)
                    float4 uv2 : TEXCOORD1;
                #endif
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                #if defined(_FLIPBOOK_BLENDING)
                    float3 uv2AndBlend : TEXCOORD1;
                #endif
                #if defined(_SOFTPARTICLES_ON) || defined(_FADING_ON)
                    float4 projPos : TEXCOORD2;
                #endif
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;

            #if defined(_SOFTPARTICLES_ON) || defined(_FADING_ON)
                sampler2D_float _CameraDepthTexture;
                float _SoftParticlesNearFadeDistance;
                float _SoftParticlesFarFadeDistance;
                float _CameraNearFadeDistance;
                float _CameraFarFadeDistance;
            #endif

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * _Color;

                #if defined(_FLIPBOOK_BLENDING)
                    o.uv2AndBlend.xy = TRANSFORM_TEX(v.uv2.xy, _MainTex);
                    o.uv2AndBlend.z = v.uv2.z;
                #endif

                #if defined(_SOFTPARTICLES_ON) || defined(_FADING_ON)
                    o.projPos = ComputeScreenPos(o.vertex);
                    COMPUTE_EYEDEPTH(o.projPos.z);
                #endif

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);

                fixed4 col = tex2D(_MainTex, i.uv);

                #if defined(_FLIPBOOK_BLENDING)
                    fixed4 col2 = tex2D(_MainTex, i.uv2AndBlend.xy);
                    col = lerp(col, col2, i.uv2AndBlend.z);
                #endif

                col *= i.color;

                #if defined(_SOFTPARTICLES_ON)
                    float sceneZ = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
                    float partZ = i.projPos.z;
                    float fade = saturate((sceneZ - partZ - _SoftParticlesNearFadeDistance) / (_SoftParticlesFarFadeDistance - _SoftParticlesNearFadeDistance));
                    col.a *= fade;
                #endif

                return col;
            }
            ENDCG
        }
    }

    Fallback "Particles/Standard Unlit"
}