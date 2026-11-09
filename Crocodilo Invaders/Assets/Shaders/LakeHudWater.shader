Shader "Crocodilo/Lake HUD Water"
{
    // The boss bar's water drawn on one quad, so its animation never rebuilds the canvas.
    Properties
    {
        [PerRendererData] _MainTex ("Unused", 2D) = "white" {}
        _Level ("Level", Range(0, 1)) = 1
        _Clock ("Clock", Float) = 0
        _Hit ("Hit", Float) = 0
        _Size ("Rect Size", Vector) = (292, 18, 0, 0)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }
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
            #pragma target 3.5
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; };
            struct v2f { float4 pos : SV_POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; };
            float _Level, _Clock, _Hit;
            float4 _Size;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                o.uv = v.uv;
                return o;
            }
            float Repeat(float value, float length) { return value - floor(value / length) * length; }
            float Surface(float x)
            {
                float ripple = (sin(x * .10 + _Clock * 4.8) * .48 + sin(x * .23 - _Clock * 7.2) * .25) * (1 + _Hit * 1.6);
                return clamp(16.5 + ripple, .5, 17.7);
            }
            bool Inside(float2 p, float left, float bottom, float right, float top) { return p.x >= left && p.x < right && p.y >= bottom && p.y < top; }
            void Over(inout float3 color, float3 layer, float alpha) { color = lerp(color, layer, alpha); }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 p = i.uv * _Size.xy;
                float width = _Size.x * _Level;
                float top = Surface(p.x);
                if (p.x >= width || p.y >= top || p.y < .3 || width <= 0) discard;

                float3 color = lerp(float3(8, 81, 94), float3(26, 164, 158), saturate((p.y - .3) / max(.01, top - .3))) / 255;
                if (p.y >= top - 1) color = float3(147, 239, 214) / 255;

                float row = floor(p.y);
                if (row >= 1 && row < 16)
                {
                    float edge = max(0, width - 1.3 - sin(row * .42 + _Clock * 5) * .45);
                    if (p.x >= edge && p.x < edge + min(1.3, width)) Over(color, float3(144, 233, 216) / 255, 230.0 / 255);
                }
                if (_Level > .035)
                    for (int s = 0; s < 19; s++)
                    {
                        float x = Repeat(s * 19 - _Clock * (21 + (s % 3) * 5), width + 18) - 18;
                        float y = floor(Surface(max(0, x)) * (.22 + (s % 3) * .22));
                        float left = max(0, x), right = min(width, x + 7 + (s % 4) * 3);
                        if (right > left && Inside(p, left, y, right, y + 1)) Over(color, float3(59, 190, 184) / 255, 210.0 / 255);
                    }
                for (int b = 0; b < 7; b++)
                {
                    float x = Repeat(b * 43 - _Clock * 12, max(1, width - 1));
                    float y = Repeat(_Clock * 5 + b * 2.7, 18);
                    if (y + 2 < Surface(x) && Inside(p, x, y, x + 1, y + 2)) Over(color, float3(138, 228, 211) / 255, 190.0 / 255);
                }
                return fixed4(color, i.color.a);
            }
            ENDCG
        }
    }
}
