Shader "Unlit/Ui"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _CullMode ("CullMode", float) =2
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
        }
          Cull Back
        ZTest Always
        ZWrite Off
        Lighting Off
        Fog
        {
            Mode Off
        }
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag alpha

            struct appdata
            {
                float2 vertex : POSITION;
                half4 color: COLOR;
                float2 uv : TEXCOORD0;
                half4 data : TEXCOORD1;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                half4 color: COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            inline float4 ScreenToClip(float2 screen)
            {
                return float4((2 * screen.x / _ScreenParams.x - 1),
                              _ProjectionParams.x * (2 * screen.y / _ScreenParams.y - 1), 0, 1);;
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.uv = v.uv;
                o.vertex = ScreenToClip(v.vertex);
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;
                clip(col.a - 0.01);
                return col;
            }
            ENDCG
        }
    }
}