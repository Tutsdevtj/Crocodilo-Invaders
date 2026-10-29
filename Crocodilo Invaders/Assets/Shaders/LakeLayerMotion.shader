Shader "Crocodilo/LakeLayerMotion"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Clock ("Scaled clock", Float) = 0
        _Mode ("Water base / reflection / foliage", Float) = 0
        _Strength ("Movement", Float) = 1
        _Root ("Root UV", Float) = .26
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "CanUseSpriteAtlas"="True" }
        Cull Off Lighting Off ZWrite Off
        Blend One OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex SpriteVert
            #pragma fragment Frag
            #pragma target 3.0
            #pragma multi_compile_instancing
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnitySprites.cginc"
            float _Clock, _Mode, _Strength, _Root;
            float4 _MainTex_TexelSize;
            fixed4 Frag(v2f i) : SV_Target
            {
                float2 uv = i.texcoord;
                if (_Mode < 1.5)
                {
                    float band = floor(uv.y * 160) / 160;
                    float flow = sin(band * 93 - _Clock * 1.35) * .0036 + sin(band * 211 + _Clock * .83) * .0017;
                    // Different speeds across depth deform reflections, not the lake's level.
                    uv.x += flow * _Strength;
                    if (_Mode > .5)
                        uv.y += sin(uv.x * 58 + _Clock * 1.8) * .0018;
                }
                else
                {
                    float bend = saturate((uv.y - _Root) * 3.5);
                    float gust = sin(_Clock * 1.35 + uv.x * 12) + .35 * sin(_Clock * 2.3 + uv.x * 27);
                    uv.x += gust * bend * bend * .0045 * _Strength;
                    uv.y += sin(_Clock * 1.4 + uv.x * 17) * bend * .0008 * _Strength;
                }
                // Sample native square pixels; motion remains continuous in time.
                uv = (floor(uv / _MainTex_TexelSize.xy) + .5) * _MainTex_TexelSize.xy;
                fixed4 c = SampleSpriteTexture(uv) * i.color;
                if (_Mode < .5)
                {
                    float depth = saturate((_Root - uv.y) * 3.2);
                    float caustic = pow(saturate(sin(uv.x * 77 + sin(uv.y * 45 + _Clock * .9) * 2 + _Clock * .65)), 10);
                    c.rgb += fixed3(.018,.045,.04) * caustic * depth;
                }
                c.rgb *= c.a;
                return c;
            }
            ENDCG
        }
    }
}
