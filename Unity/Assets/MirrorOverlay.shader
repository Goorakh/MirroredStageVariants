Shader "Hidden/MirrorOverlay"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OverlayTex ("Overlay Texture", 2D) = "white" {}
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
            sampler2D _OverlayTex;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 baseCol = tex2D(_MainTex, i.uv);

                i.uv.x = 0.5 - (i.uv.x - 0.5);
                fixed4 overlayCol = tex2D(_OverlayTex, i.uv);
                
                float maxColorChannel = max(max(overlayCol.r, overlayCol.g), overlayCol.b);
                return maxColorChannel > 0 ? overlayCol : baseCol;
            }
            ENDCG
        }
    }
}
