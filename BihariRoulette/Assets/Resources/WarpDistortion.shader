Shader "Custom/WarpDistortion"
{
    Properties
    {
        _MainTex ("Base (Hidden)", 2D) = "white" {}
        _DistortionStrength ("Distortion Strength", Range(0,0.5)) = 0
        _DistortionSpeed ("Distortion Speed", Range(0,10)) = 2
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Overlay" }
        Cull Off ZWrite Off ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float _DistortionStrength;
            float _DistortionSpeed;

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

            fixed4 frag (v2f i) : SV_Target
            {
                // time-based warp wobble
                float wave = sin(_Time.y * _DistortionSpeed + i.uv.x * 20) * 
                             cos(_Time.y * _DistortionSpeed + i.uv.y * 20);

                float2 offset = float2(wave, wave) * _DistortionStrength;

                // sample screen with offset
                fixed4 col = tex2D(_MainTex, i.uv + offset);

                return col;
            }
            ENDCG
        }
    }
}
