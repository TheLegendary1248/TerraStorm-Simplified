Shader "Unlit/Water"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent"}
        LOD 100
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull back
        Pass
        {
            CGINCLUDE
            float rand(float2 co) {
                return frac(sin(dot(co, float2(12.9898, 78.233))) * 43758.5453);
            }
            ENDCG
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

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
                float4 world : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.world = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                //Wavy Patterns
                i.world.y = abs(i.world.y);
                float2 scaled = i.world / 4;
                i.uv.y += (sin(scaled.x * 20 + _Time.y) + cos(scaled.x * 30 - (_Time.y * 1.22))) / 200;
                i.uv.x += sin(i.uv.y * 20 + _Time.y) / 200;

                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                float2 uvCenter = abs(i.uv - 0.5) * 2;
                
                float uvProx = uvCenter.y;
                //uvProx += (uvCenter.y - uvCenter.x) * (uvCenter.y > uvCenter.x);
                float wave = ((uvProx - (_Time.x * 2.5)) % 0.1) * uvProx;
                wave = wave;
                return fixed4(0.1, 0.8, 1, uvProx + wave);
                //Sideways Checker Grid
                float2 ten = i.world * 1;
                ten.x += _Time.y / 2;
                i.world.x += _Time.y / 2;
                
                ten.y %= 2;
                ten.y -= 1;
                ten.y = abs(ten.y);
                
                ten.x %= 2;
                ten.x -= 1;
                ten.x = abs(ten.x);
                float odd = (ten.x > ten.y) * 2 - 1;
                i.world /= 2;
                float2 tile = float2(odd + floor(i.world.x), odd * (frac(i.world.y) > 0.5) + floor(i.world.y));
                //float wave = ((uvCenter.x * uvCenter.x) + (uvCenter.y * uvCenter.y)) % .2;
                //return float4(i.world.x,i.world.y,0,uvProx);
                float edge = uvProx - 0.9;
                edge = edge * (edge > 0);
                edge *= 10;
                return lerp(fixed4(0.1,0.8,1,(uvProx + wave) - 0.3), fixed4(1,1,1,1), (rand(tile + float2(_Time.x,0)) * edge) + 0.3 > 0.5);
            }
            
            ENDCG
        }
    }
}
