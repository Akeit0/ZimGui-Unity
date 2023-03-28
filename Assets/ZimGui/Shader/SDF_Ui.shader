Shader "Unlit/SDF_Ui"
{
    Properties
    {
        _ScaleOffSet("ScaleOffSet",Range(-1,1))=1
        _ScaleFactor("ScaleFactor",Range(0.5,200))=1

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
            #pragma fragment frag //alpha

            float _ScaleFactor;
            float _ScaleOffSet;

            sampler2D _MainTex;

            struct appdata
            {
                float2 vertex : POSITION;
                half4 color: COLOR;
                float2 uv : TEXCOORD0;
                half4 data : TEXCOORD1;
            };

            struct v2f
            {
                half4 vertex : SV_POSITION;
                half2 uv: TEXCOORD0;
                half4 color: COLOR;
                half2 param :TEXCOORD1;
            };

            inline float4 ScreenToClip(float2 screen)
            {
                return float4((2 * screen.x / _ScreenParams.x - 1),
                              _ProjectionParams.x * (2 * screen.y / _ScreenParams.y - 1), 0, 1);;
            }

            v2f vert(const appdata v)
            {
                v2f o;
                o.uv = v.uv;
                o.vertex = ScreenToClip(v.vertex);
                o.color = v.color;
                const half x = _ScaleFactor * v.data.x;
                const half offset = 0.5 - 1 / x  * 0.15;
                const half scale = x * 3.5 + _ScaleOffSet;
                o.param = half2(offset, scale);
                return o;
            }

            fixed4 frag(const v2f i) : SV_Target
            {
                return saturate((tex2D(_MainTex, i.uv).a - i.param.x) * i.param.y) * i.color;
            }
            ENDCG
        }
    }
}