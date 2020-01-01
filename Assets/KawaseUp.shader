Shader "Hidden/KawaseUp"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            sampler2D _CurTex;
            uniform float4 _MainTex_TexelSize;

            float4 frag (v2f i) : SV_Target
            {
                float2 hp = _MainTex_TexelSize*0.5;
                float4 sum = tex2D(_MainTex,i.uv + float2(-hp.x*2.,0.));
                sum += tex2D(_MainTex, i.uv + float2(-hp.x,hp.y))*2.;
                sum += tex2D(_MainTex, i.uv + float2(0.0,hp.y*2.0));
                sum += tex2D(_MainTex, i.uv + hp)*2.;
                sum += tex2D(_MainTex, i.uv + float2(hp.x*2.0, 0.0));
                sum += tex2D(_MainTex, i.uv + float2(hp.x, -hp.y))*2.;
                sum += tex2D(_MainTex, i.uv + float2(0.0,-hp.y*2.0));
                sum += tex2D(_MainTex, i.uv - hp)*2.;
                return sum/10 + tex2D(_CurTex,i.uv)/6;
            }
            ENDCG
        }
    }
}
