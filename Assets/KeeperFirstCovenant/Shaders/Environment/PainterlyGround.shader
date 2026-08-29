Shader "Keeper First Covenant/Environment/Painterly Ground"
{
    Properties
    {
        _SurfaceA ("Primary Surface", 2D) = "white" {}
        _SurfaceB ("Secondary Surface", 2D) = "white" {}
        _Tint ("Tint", Color) = (1,1,1,1)
        _BlendMode ("Blend Mode", Float) = 0
        _PathHalfWidth ("Path Half Width", Range(0.05,0.48)) = 0.24
        _EdgeFeather ("Edge Feather", Range(0.005,0.2)) = 0.055
        _EdgeNoise ("Edge Noise", Range(0,0.12)) = 0.032
        _NoiseScale ("Noise Scale", Range(1,32)) = 11
        _RotationRadians ("Pattern Rotation", Range(-3.14159,3.14159)) = 0
        _ColorVariation ("Color Variation", Range(0,0.2)) = 0.035
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Opaque"
            "Queue"="Geometry"
        }

        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode"="UniversalForward" }
            Cull Off
            ZWrite On
            Blend One Zero

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uvA : TEXCOORD0;
                float2 uvB : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
            };

            TEXTURE2D(_SurfaceA);
            SAMPLER(sampler_SurfaceA);
            TEXTURE2D(_SurfaceB);
            SAMPLER(sampler_SurfaceB);

            CBUFFER_START(UnityPerMaterial)
                float4 _SurfaceA_ST;
                float4 _SurfaceB_ST;
                float4 _Tint;
                float _BlendMode;
                float _PathHalfWidth;
                float _EdgeFeather;
                float _EdgeNoise;
                float _NoiseScale;
                float _RotationRadians;
                float _ColorVariation;
            CBUFFER_END

            float Hash21(float2 value)
            {
                value = frac(value * float2(123.34, 456.21));
                value += dot(value, value + 45.32);
                return frac(value.x * value.y);
            }

            float SmoothNoise(float2 value)
            {
                float2 cell = floor(value);
                float2 local = frac(value);
                local = local * local * (3.0 - 2.0 * local);

                float a = Hash21(cell);
                float b = Hash21(cell + float2(1.0, 0.0));
                float c = Hash21(cell + float2(0.0, 1.0));
                float d = Hash21(cell + float2(1.0, 1.0));

                return lerp(lerp(a, b, local.x), lerp(c, d, local.x), local.y);
            }

            float2 RotateCentered(float2 uv, float radians)
            {
                float sine = sin(radians);
                float cosine = cos(radians);
                float2 centered = uv - 0.5;
                return float2(
                    centered.x * cosine - centered.y * sine,
                    centered.x * sine + centered.y * cosine);
            }

            float Band(float distanceToCenter, float edgeNoise)
            {
                return 1.0 - smoothstep(
                    _PathHalfWidth + edgeNoise,
                    _PathHalfWidth + _EdgeFeather + edgeNoise,
                    distanceToCenter);
            }

            float ResolveBlend(float2 uv)
            {
                if (_BlendMode < 0.5)
                    return 0.0;
                if (_BlendMode > 3.5 && _BlendMode < 4.5)
                    return 1.0;

                float2 q = RotateCentered(uv, _RotationRadians);
                float noise =
                    (SmoothNoise(uv * _NoiseScale) - 0.5) *
                    (_EdgeNoise * 2.0);

                float vertical = Band(abs(q.x), noise);
                float horizontal = Band(abs(q.y), noise);

                if (_BlendMode < 1.5)
                    return vertical;

                if (_BlendMode < 2.5)
                {
                    float verticalGate = 1.0 - smoothstep(0.02, 0.13, q.y);
                    float horizontalGate = smoothstep(-0.13, -0.02, q.x);
                    return max(vertical * verticalGate, horizontal * horizontalGate);
                }

                if (_BlendMode < 3.5)
                    return max(vertical, horizontal);

                if (_BlendMode < 5.5)
                {
                    float edge = q.x + noise;
                    return smoothstep(-_EdgeFeather, _EdgeFeather, edge);
                }

                float horizontalGate = smoothstep(-0.13, -0.02, q.x);
                return max(vertical, horizontal * horizontalGate);
            }

            Varyings Vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs positionInputs =
                    GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.uvA = TRANSFORM_TEX(input.uv, _SurfaceA);
                output.uvB = TRANSFORM_TEX(input.uv, _SurfaceB);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half4 primary = SAMPLE_TEXTURE2D(_SurfaceA, sampler_SurfaceA, input.uvA);
                half4 secondary = SAMPLE_TEXTURE2D(_SurfaceB, sampler_SurfaceB, input.uvB);
                float blend = ResolveBlend(input.uvA);

                float worldVariation =
                    (SmoothNoise(input.positionWS.xz * 0.31) - 0.5) *
                    (_ColorVariation * 2.0);

                half4 result = lerp(primary, secondary, blend) * _Tint;
                result.rgb *= 1.0 + worldVariation;
                result.a = 1.0;
                return result;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode"="DepthOnly" }
            ColorMask 0
            ZWrite On

            HLSLPROGRAM
            #pragma vertex DepthVert
            #pragma fragment DepthFrag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings DepthVert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }

            half4 DepthFrag(Varyings input) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
    }

    FallBack Off
}
