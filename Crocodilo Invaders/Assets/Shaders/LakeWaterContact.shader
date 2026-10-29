Shader "Crocodilo/LakeWaterContact"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _WaterY ("Water height", Float) = 0
        _Clock ("Scaled clock", Float) = 0
        _Reflection ("Reflection", Float) = 0
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
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 3.0
            #pragma multi_compile_instancing
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnitySprites.cginc"
            float _WaterY, _Clock, _Reflection;
            struct WaterVarying { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; float2 world : TEXCOORD1; fixed4 color : COLOR; };
            WaterVarying Vert(appdata_t v)
            {
                UNITY_SETUP_INSTANCE_ID(v);
                WaterVarying o;
                v.vertex = UnityFlipSprite(v.vertex, _Flip);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.world = mul(unity_ObjectToWorld, v.vertex).xy;
                o.uv = v.texcoord; o.color = v.color * _Color * _RendererColor;
                return o;
            }
            fixed4 Frag(WaterVarying i) : SV_Target
            {
                float level = _WaterY + sin(i.world.x * 4.2 - _Clock * 1.8) * .018;
                float depth = level - i.world.y;
                float2 uv = i.uv;
                if (_Reflection > .5) uv.x += sin(i.world.y * 48 + _Clock * 2.1) * .0035;
                fixed4 c = SampleSpriteTexture(uv) * i.color;
                if (_Reflection > .5)
                {
                    c.rgb = lerp(c.rgb, fixed3(.08,.37,.4), .65);
                    c.a *= saturate(depth / .04) * saturate(1 - depth / .85) * .24;
                }
                else
                {
                    c.a *= 1 - smoothstep(0, .09, depth);
                    c.rgb = lerp(c.rgb, fixed3(.12,.48,.48), saturate((depth + .045) * 8) * .3);
                }
                c.rgb *= c.a;
                return c;
            }
            ENDCG
        }
    }
}
