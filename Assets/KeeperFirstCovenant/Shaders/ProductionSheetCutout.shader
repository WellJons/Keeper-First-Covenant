Shader "Keeper First Covenant/Production Sheet Cutout"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _KeyLuma ("Maximum keyed luminance", Range(0,1)) = 0.34
        _BlueBias ("Minimum blue/background bias", Range(0,0.4)) = 0.018
        _Softness ("Key softness", Range(0.001,0.25)) = 0.055
        _AlphaCutoff ("Alpha cutoff", Range(0,0.5)) = 0.02
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
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
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float _KeyLuma;
            float _BlueBias;
            float _Softness;
            float _AlphaCutoff;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, i.texcoord) * i.color;

                float luma = dot(c.rgb, float3(0.299, 0.587, 0.114));
                float blueBias = c.b - max(c.r, c.g);

                float darkFactor =
                    saturate((_KeyLuma - luma) / max(_Softness, 0.001));

                float blueFactor =
                    saturate((blueBias - _BlueBias) / max(_Softness, 0.001));

                float keyed = darkFactor * blueFactor;
                c.a *= (1.0 - keyed);

                clip(c.a - _AlphaCutoff);
                return c;
            }
            ENDCG
        }
    }
}
