Shader "Hidden/ToneMap"
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
            sampler2D _Bloom;
            

            fixed4 frag (v2f i) : SV_Target
            {
                float exposure = 10;
                const float gamma = 2.2;
                float3 bloom = tex2D(_Bloom, i.uv).rgb;
                float3 hdrColor = tex2D(_MainTex, i.uv).rgb;
                hdrColor+=bloom;
                // Exposure tone mapping
                //float3 mapped = float3(1,1,1) - exp(-hdrColor * exposure);
                // Gamma correction 
                //float correct = 1.0 / gamma;
                //mapped = pow(mapped, float3(correct,correct,correct));

                float L = float3(0.2126,0.7152,0.0722)*hdrColor*2;
                float x = max(0,L-0.004);
                x = (x*(6.2*x+0.5))/(x*(6.2*x+1.7)+0.06);

                float scale = x / L;
                float3 mapped = hdrColor*scale;

                float over = pow(dot(saturate(mapped-1),float3(1,1,1)),2.2);

                mapped+=over;

                

  
                return fixed4(mapped, 1.0);
            }
            ENDCG
        }
    }
}
