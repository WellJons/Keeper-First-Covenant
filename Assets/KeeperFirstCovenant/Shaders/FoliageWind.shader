Shader "Keeper First Covenant/Foliage Wind"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _WindStrength ("Wind Strength", Range(0,0.35)) = 0.05
        _WindSpeed ("Wind Speed", Range(0,8)) = 1.2
        _WindScale ("Wind Spatial Scale", Range(0,4)) = 0.7
        _WindPhase ("Wind Phase", Float) = 0
        _BaseLock ("Base Lock", Range(0.25,6)) = 2.2
        _GustStrength ("Gust Strength", Range(0,1)) = 0.2
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
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
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
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
            float _WindStrength;
            float _WindSpeed;
            float _WindScale;
            float _WindPhase;
            float _BaseLock;
            float _GustStrength;

            v2f vert(appdata_t IN)
            {
                v2f OUT;

                float3 worldPos = mul(unity_ObjectToWorld, IN.vertex).xyz;
                float heightWeight = pow(saturate(IN.texcoord.y), max(0.25, _BaseLock));

                float baseWave =
                    sin(_Time.y * _WindSpeed +
                        (worldPos.x + worldPos.z) * _WindScale +
                        _WindPhase);

                float gust =
                    sin(_Time.y * (_WindSpeed * 0.37) +
                        worldPos.x * 0.21 +
                        worldPos.z * 0.17 +
                        _WindPhase * 1.7);

                float sway =
                    (baseWave + gust * _GustStrength) *
                    _WindStrength *
                    heightWeight;

                IN.vertex.x += sway;
                IN.vertex.y += abs(sway) * 0.035 * heightWeight;

                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;

                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap(OUT.vertex);
                #endif

                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;
                c.rgb *= c.a;
                return c;
            }
            ENDCG
        }
    }

    Fallback "Sprites/Default"
}
