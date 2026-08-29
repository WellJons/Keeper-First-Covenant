Shader "Keeper First Covenant/Environment/Interactive Foliage"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _WindStrength ("Wind Strength", Range(0,0.35)) = 0.08
        _WindSpeed ("Wind Speed", Range(0.1,8)) = 1.45
        _WindScale ("Wind Spatial Scale", Range(0.1,8)) = 1.7
        _Phase ("Instance Phase", Range(0,6.28318)) = 0
        _BendVector ("Interaction Bend", Vector) = (0,0,0,0)
        _BendStrength ("Interaction Strength", Range(0,1)) = 0
        _AnchorHeight ("Anchored Base", Range(0,0.35)) = 0.13
        _AlphaCutoff ("Alpha Cutoff", Range(0,0.2)) = 0.015
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "CanUseSpriteAtlas"="True"
        }

        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode"="UniversalForward" }
            Cull Off
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _Color;
                float _WindStrength;
                float _WindSpeed;
                float _WindScale;
                float _Phase;
                float4 _BendVector;
                float _BendStrength;
                float _AnchorHeight;
                float _AlphaCutoff;
            CBUFFER_END

            float4 _KfcWind;
            float _KfcWindPulse;

            Varyings Vert(Attributes input)
            {
                Varyings output;
                float3 positionOS = input.positionOS.xyz;
                float3 worldBeforeBend = TransformObjectToWorld(positionOS);

                float heightMask = smoothstep(_AnchorHeight, 1.0, input.uv.y);
                heightMask *= heightMask;

                float windAmount =
                    _KfcWind.z +
                    _KfcWind.w * _KfcWindPulse;

                float phase =
                    _Time.y * _WindSpeed +
                    dot(worldBeforeBend.xz, float2(0.73, 1.17)) * _WindScale +
                    _Phase;

                float primaryWave = sin(phase);
                float secondaryWave = sin(phase * 0.47 + 1.71) * 0.36;
                float windWave = (primaryWave + secondaryWave) * _WindStrength * windAmount;

                float interaction = _BendVector.x * _BendStrength;
                positionOS.x += (windWave + interaction * 0.36) * heightMask;
                positionOS.y -= abs(interaction) * 0.055 * heightMask;

                output.positionCS = TransformObjectToHClip(positionOS);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color * _Color;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                color *= input.color;
                clip(color.a - _AlphaCutoff);
                return color;
            }
            ENDHLSL
        }
    }

    FallBack Off
}
