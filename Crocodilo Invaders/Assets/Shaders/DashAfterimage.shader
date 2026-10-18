Shader "Crocodilo/DashAfterimage"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [PerRendererData] _DashAge ("Age (seconds)", Float) = 0
        [PerRendererData] _DashShade ("Lower 38 percent of silhouette", Float) = .38
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "CanUseSpriteAtlas"="True" }
        Cull Off Lighting Off ZWrite Off Blend One OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex SpriteVert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnitySprites.cginc"
            float4 _MainTex_TexelSize;
            float _DashAge;
            float _DashShade;
            fixed4 frag(v2f input) : SV_Target
            {
                float2 uv = input.texcoord, pixel = _MainTex_TexelSize.xy;
                float a = SampleSpriteTexture(uv).a;
                float up = SampleSpriteTexture(uv + float2(0,pixel.y)).a;
                float adjacent = max(max(up, SampleSpriteTexture(uv - float2(0,pixel.y)).a),
                    max(SampleSpriteTexture(uv + float2(pixel.x,0)).a, SampleSpriteTexture(uv - float2(pixel.x,0)).a));
                fixed3 young = fixed3(170,234,255)/255, middle = fixed3(70,196,250)/255, old = fixed3(36,124,220)/255;
                fixed3 color = _DashAge < .045 ? young : _DashAge < .09 ? middle : old;
                if (uv.y < _DashShade) color = _DashAge < .045 ? middle : _DashAge < .09 ? old : fixed3(24,72,160)/255;
                if (up < .5 && _DashAge < .09) color = _DashAge < .045 ? fixed3(240,252,255)/255 : young;
                if (a < .5) color = fixed3(24,72,160)/255;
                float alpha = max(a,adjacent) * .7 * saturate(1-_DashAge/.13) * input.color.a;
                return fixed4(color * alpha, alpha);
            }
            ENDCG
        }
    }
}
