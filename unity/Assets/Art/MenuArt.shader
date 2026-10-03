Shader "Crocodilo/Arte menu original"
{
    Properties
    {
        [PerRendererData] _MainTex ("Arte original", 2D) = "white" {}
        _Color ("Cor", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            struct v2f { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            sampler2D _MainTex;
            fixed4 _Color;
            v2f vert(appdata input)
            {
                v2f output;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.uv = input.uv;
                output.color = input.color * _Color;
                return output;
            }
            fixed4 frag(v2f input) : SV_Target
            {
                fixed4 art = tex2D(_MainTex, input.uv);
                // Só o céu azul fica transparente. Título verde, moldura e letras são a arte original.
                if (art.b > art.r + 0.04 && art.b > art.g * 0.96) art.a = 0;
                return art * input.color;
            }
            ENDCG
        }
    }
}
