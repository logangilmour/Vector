Shader "Hidden/Bez"
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
            #include "Assets/Tests.cginc"
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
            
            uniform float2 _B0;
            uniform float2 _B1;
            uniform float2 _B2;
            uniform int _TileLines;
            

            int _Xtiles;
            int _Ytiles;
            
            uniform StructuredBuffer<int> _Tiles;
            uniform StructuredBuffer<float4> _Buffer;
            uniform StructuredBuffer<int> _Counts;

            fixed4 frag (v2f inp) : SV_Target
            {
                
                fixed4 col = fixed4(0,0,0,1);
                int2 tile = (int2)(inp.uv*float2(_Xtiles,_Ytiles));

                float c = 0;

                //float2 flipped = i.uv.y;
                //flipped.y = 1-flipped.y;
                
                float2 off = inp.uv*2-1;
                int count = _Counts[tile.x+tile.y*_Xtiles];
                float mindist = 1000;
                for(int i=0; i<count; i++){
                    int ln_id = _Tiles[tile.x+tile.y*_Xtiles+i*(_Xtiles*_Ytiles)];
                    float4 ln = _Buffer[ln_id]-float4(off,off);
                    mindist = min(get_line(ln.xy,float2(0,0),ln.zw),mindist);
                    
                    
                }
                c = 1-saturate(clamp(mindist-0.003,0,0.003)/0.003);
                col.r = c;//clamp(x,0,0.01)*100;//clamp((length(x)-0.001)*100,0,1);   
                col.g = (float)count/_TileLines;   
                return col;
            }
            ENDCG
        }
    }
}
