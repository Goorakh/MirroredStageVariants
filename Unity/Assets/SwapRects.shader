Shader "Hidden/SwapRects"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _InputTex ("Input Texture", 2D) = "white" {}
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

            #define SWAP_POSITIONS_MAX_SIZE 100

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
            sampler2D _InputTex;

            int _SwapCount;
            float4 _SwapPositions[SWAP_POSITIONS_MAX_SIZE];
            float4 _SwapSizes[SWAP_POSITIONS_MAX_SIZE];

            bool isInRect(float2 position, float2 rectOrigin, float2 size)
            {
                return position.x >= rectOrigin.x && position.x <= rectOrigin.x + size.x &&
                       position.y >= rectOrigin.y && position.y <= rectOrigin.y + size.y;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 baseUV = i.uv;
                float2 inputUV = i.uv;

                for (int i = 0; i < _SwapCount; i++)
                {
                    float2 rectA = _SwapPositions[i].xy;
                    float2 rectB = _SwapPositions[i].zw;

                    if (isInRect(inputUV, rectA, _SwapSizes[i]))
                    {
                        inputUV += rectB - rectA;
                    }
                    else if (isInRect(inputUV, rectB, _SwapSizes[i]))
                    {
                        inputUV += rectA - rectB;
                    }
                }
                
                fixed4 baseCol = tex2D(_MainTex, baseUV);
                fixed4 inputCol = tex2D(_InputTex, inputUV);

                float maxColorChannel = max(max(inputCol.r, inputCol.g), inputCol.b);
                return maxColorChannel > 0 ? inputCol : baseCol;
            }
            ENDCG
        }
    }
}
