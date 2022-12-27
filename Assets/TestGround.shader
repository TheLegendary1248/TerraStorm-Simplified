Shader "Unlit/TestGround"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed dirt = (i.uv.y - 0.1) / 0.9;
                //fixed4 col = ((i.uv.y > 0.9) * fixed4(0, 1, 0, 1)) + ((i.uv.y <= 0.9) * lerp(fixed4(0.2, 0.13, 0.04, 0), fixed4(0.44, 0.33, 0.07, 0), dirt));
                fixed4 col = lerp(fixed4(0.2, 0.13, 0.04, 0), fixed4(0.44, 0.33, 0.07, 0), dirt);
                return col;
            }
            ENDCG
        }
    }
}
