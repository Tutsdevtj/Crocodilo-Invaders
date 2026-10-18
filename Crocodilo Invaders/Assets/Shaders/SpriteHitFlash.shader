Shader "Crocodilo/SpriteHitFlash"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [PerRendererData] _FlashAmount ("Flash Amount", Range(0,1)) = 0
        [PerRendererData] _DashOutline ("Dash outline", Range(0,1)) = 0
        [PerRendererData] _DashOutlineColor ("Dash outline color", Color) = (.667,.918,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }
        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex SpriteVert
            #pragma fragment FlashFrag
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnitySprites.cginc"

            half _FlashAmount;
            half _DashOutline;
            fixed4 _DashOutlineColor;
            float4 _MainTex_TexelSize;

            fixed4 FlashFrag(v2f input) : SV_Target
            {
                fixed4 color = SampleSpriteTexture(input.texcoord) * input.color;
                color.rgb = lerp(color.rgb, fixed3(1,1,1), _FlashAmount);
                color.rgb *= color.a;
                if (_DashOutline > .5)
                {
                    float2 pixel = _MainTex_TexelSize.xy;
                    half neighbor = max(max(SampleSpriteTexture(input.texcoord + float2(pixel.x,0)).a, SampleSpriteTexture(input.texcoord - float2(pixel.x,0)).a),
                        max(SampleSpriteTexture(input.texcoord + float2(0,pixel.y)).a, SampleSpriteTexture(input.texcoord - float2(0,pixel.y)).a));
                    half edge = saturate(neighbor - SampleSpriteTexture(input.texcoord).a) * input.color.a;
                    color.rgb += _DashOutlineColor.rgb * edge;
                    color.a = max(color.a, edge);
                }
                return color;
            }
            ENDCG
        }
    }
}
