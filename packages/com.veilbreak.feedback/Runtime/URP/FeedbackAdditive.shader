Shader "Veilbreak/Feedback/Additive" {
 SubShader { Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" } Pass {
 Blend SrcAlpha One ZWrite Off Cull Off
 HLSLPROGRAM
 #pragma vertex Vertex
 #pragma fragment Fragment
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 struct Attributes { float4 positionOS:POSITION;half4 color:COLOR;float2 uv:TEXCOORD0; };
 struct Varyings { float4 positionCS:SV_POSITION;half4 color:COLOR;float2 uv:TEXCOORD0; };
 Varyings Vertex(Attributes v){Varyings o;o.positionCS=TransformObjectToHClip(v.positionOS.xyz);o.color=v.color;o.uv=v.uv;return o;}
 half4 Fragment(Varyings v):SV_Target {float feather=saturate(1-abs(v.uv.y*2-1));return half4(v.color.rgb,v.color.a*feather);}
 ENDHLSL
 } }
}
