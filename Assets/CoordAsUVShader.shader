Shader "Unlit/CoordAsUVShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BorderThresh ("Border Threshold", Range(0,1)) = 0.3
        _Color ("Color", Color) = (1,0,0)
    }
    SubShader
    {
        Color [_Color]
        Tags { "RenderType"="Opaque"}
        LOD 100
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            fixed4 _Color;
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 a_uv : TEXCOORD1;
                float3 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            uniform float _BorderThresh;
            float4 _MainTex_ST;
            

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = mul(unity_ObjectToWorld, v.vertex);
                o.a_uv = TRANSFORM_TEX(v.uv, _MainTex);

                return o;
            }

            fixed4 frag(v2f i) : SV_TARGET
            {
                // sample the texture
                
                float c = (abs(i.a_uv.y - 0.5) * 2) < _BorderThresh;
                fixed4 col = tex2D(_MainTex, i.uv * c) * _Color ;
                // apply fog
                return col;
            }
            ENDCG
        }
    }
}
