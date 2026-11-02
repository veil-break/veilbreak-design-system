Shader "Veilbreak/Feedback/OverlayBlur" {
 SubShader { Tags { "RenderPipeline"="UniversalPipeline" } Pass { ZWrite Off Cull Off ZTest Always
 HLSLPROGRAM
 #pragma vertex Vert
 #pragma fragment Frag
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
 float4 _Direction;float _Dim;
 half4 Frag(Varyings input):SV_Target {float2 uv=input.texcoord,d=_Direction.xy;
  half3 c=SAMPLE_TEXTURE2D_X(_BlitTexture,sampler_LinearClamp,uv).rgb*.227027;
  c+=(SAMPLE_TEXTURE2D_X(_BlitTexture,sampler_LinearClamp,uv+d*1.384615).rgb+SAMPLE_TEXTURE2D_X(_BlitTexture,sampler_LinearClamp,uv-d*1.384615).rgb)*.316216;
  c+=(SAMPLE_TEXTURE2D_X(_BlitTexture,sampler_LinearClamp,uv+d*3.230769).rgb+SAMPLE_TEXTURE2D_X(_BlitTexture,sampler_LinearClamp,uv-d*3.230769).rgb)*.070270;
  return half4(c*_Dim,1);
 }
 ENDHLSL
 } }
}
