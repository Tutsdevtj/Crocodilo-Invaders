Shader "Crocodilo/UI Card Pixel Glow"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        Stencil { Ref [_Stencil] Comp [_StencilComp] Pass [_StencilOp] ReadMask [_StencilReadMask] WriteMask [_StencilWriteMask] }
        Cull Off Lighting Off ZWrite Off ZTest [unity_GUIZTestMode]
        Blend SrcAlpha One
        ColorMask [_ColorMask]
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct Input { float4 vertex:POSITION; float4 color:COLOR; float2 uv:TEXCOORD0; };
            struct Output { float4 vertex:SV_POSITION; fixed4 color:COLOR; float2 uv:TEXCOORD0; };
            fixed4 _Color;
            Output vert(Input v) { Output o; o.vertex=UnityObjectToClipPos(v.vertex); o.color=v.color*_Color; o.uv=v.uv; return o; }
            fixed4 frag(Output i):SV_Target
            {
                float2 p=(floor(i.uv*float2(104,144))+.5)/float2(104,144)-.5;
                float2 ellipse=p*float2(1.35,1.0);
                float radius=length(ellipse);
                float angle=atan2(ellipse.y,ellipse.x);
                float rays=pow(saturate(cos(angle*12.0)),14.0);
                float2 outside=max(abs(p)-float2(.26,.34),0)*float2(1.8,1.2);
                float halo=exp(-length(outside)*12.0)*.9;
                float edgeFade=saturate((.5-max(abs(p.x),abs(p.y)))*12.0);
                float glow=(halo+exp(-radius*3.0)*rays*.85)*edgeFade;
                return fixed4(i.color.rgb,i.color.a*glow);
            }
            ENDCG
        }
    }
}
