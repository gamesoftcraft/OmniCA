Shader "Unlit/StateDisplay"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

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
            int _Mask;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed2 uv = i.uv * 3;
                fixed2 guv = frac(uv) - .5;
                fixed2 gid = floor(uv); 
                fixed2 pguv = pow(guv * 2, 3);
                fixed3 cell = 1 - dot(pguv, pguv);
                int id = gid.x + gid.y * 3;
                float3 grd = (.87 - smoothstep(cell, .2, .4));

                float centr = sign(pow(id - 4, 2)) * 1.5;
                grd.r *= centr;
                cell.r *= centr;

                cell *= (_Mask >> id) & 1;
                
                cell -= grd;

                return fixed4(max(grd, cell), 1);
            }
            ENDCG
        }
    }
}
