// The visor's barrel curve on the camera image (VisorLensFeature). Same mapping as VisorLens.Source and the UI's
// VisorFilter: the centre is magnified by 1 + _VisorBarrel, the rim compressed, the corners fixed.
Shader "Hidden/Veilbreak/VisorLens"
{
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off ZTest Always Cull Off Blend Off

        Pass
        {
            Name "VisorLens"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float _VisorBarrel;

            half4 Frag (Varyings input) : SV_Target
            {
                float2 uv = (input.texcoord - _BlitScaleBias.zw) / _BlitScaleBias.xy; // 0..1 over the image, whatever the RTHandle scale
                float aspect = _ScreenParams.x / _ScreenParams.y;
                float2 d = (uv - 0.5) * float2(aspect, 1.0) * 2.0;
                float r2 = dot(d, d) / (aspect * aspect + 1.0);
                float2 source = 0.5 + (uv - 0.5) * ((1.0 + _VisorBarrel * r2) / (1.0 + _VisorBarrel));
                return SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, source * _BlitScaleBias.xy + _BlitScaleBias.zw);
            }
            ENDHLSL
        }
    }
}
