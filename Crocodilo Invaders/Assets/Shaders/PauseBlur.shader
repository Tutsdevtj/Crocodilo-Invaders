Shader "Crocodilo/PauseBlur"
{
    Properties
    {
        _MainTex ("Scene", 2D) = "white" {}
        _Radius ("Blur radius", Float) = 2.2
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        CGINCLUDE
        #include "UnityCG.cginc"
        sampler2D _MainTex;
        float4 _MainTex_TexelSize;
        float _Radius;
        fixed4 Blur(float2 uv, float2 direction)
        {
            float2 offset = direction * _MainTex_TexelSize.xy * _Radius;
            fixed4 color = tex2D(_MainTex, uv) * .227027;
            color += tex2D(_MainTex, uv + offset * 1.384615) * .316216;
            color += tex2D(_MainTex, uv - offset * 1.384615) * .316216;
            color += tex2D(_MainTex, uv + offset * 3.230769) * .070270;
            color += tex2D(_MainTex, uv - offset * 3.230769) * .070270;
            return color;
        }
        fixed4 Horizontal(v2f_img input) : SV_Target { return Blur(input.uv, float2(1, 0)); }
        fixed4 Vertical(v2f_img input) : SV_Target { return Blur(input.uv, float2(0, 1)); }
        ENDCG
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment Horizontal
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment Vertical
            ENDCG
        }
    }
}
