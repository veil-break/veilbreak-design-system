Shader "Veilbreak/Feedback/GammaUi" {
 SubShader { Tags { "RenderPipeline"="UniversalPipeline" } Pass { ZWrite Off Cull Off ZTest Always
 HLSLPROGRAM
 #pragma vertex Vert
 #pragma fragment Frag
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
 TEXTURE2D(_GammaUi); SAMPLER(sampler_GammaUi);
 half4 Frag(Varyings input):SV_Target {
 half4 world=SAMPLE_TEXTURE2D_X(_BlitTexture,sampler_LinearClamp,input.texcoord);
 half4 ui=SAMPLE_TEXTURE2D(_GammaUi,sampler_GammaUi,input.texcoord);
 // UI Toolkit's surface is premultiplied gamma. Composite like the authored exports,
 // then return linear color so URP's final output conversion is applied exactly once.
 #if defined(UNITY_COLORSPACE_GAMMA)
 return half4(ui.rgb+world.rgb*(1-ui.a),world.a+ui.a*(1-world.a));
 #else
 half3 gamma=ui.rgb+LinearToSRGB(world.rgb)*(1-ui.a);
 return half4(SRGBToLinear(gamma),world.a+ui.a*(1-world.a));
 #endif
 }
 ENDHLSL
 } }
}
