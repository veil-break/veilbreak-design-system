// Vertical blur for the mirrored emissive pass: lamp and window reflections smear down a wet street, not sideways.
// Nine taps spread over `_Spread` of the texture height, weighted toward the centre.
Shader "Hidden/Veilbreak/Environment/ReflectionBlur"
{
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off ZTest Always Cull Off Blend Off
        Pass
        {
            Name "Vertical"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            float _Spread;
            half4 frag(Varyings i) : SV_Target
            {
                static const float w[5] = { 0.2, 0.18, 0.13, 0.08, 0.04 };
                half3 c = SAMPLE_TEXTURE2D_LOD(_BlitTexture, sampler_LinearClamp, i.texcoord, 0).rgb * w[0];
                [unroll] for (int k = 1; k < 5; k++)
                {
                    float o = k * k * _Spread / 16.0;
                    c += (SAMPLE_TEXTURE2D_LOD(_BlitTexture, sampler_LinearClamp, i.texcoord + float2(0, o), 0).rgb
                        + SAMPLE_TEXTURE2D_LOD(_BlitTexture, sampler_LinearClamp, i.texcoord - float2(0, o), 0).rgb) * w[k];
                }
                // Environmental light is additive over sky; solid actors written afterward own coverage in alpha.
                return half4(c * 1.1, 0);
            }
            ENDHLSL
        }
    }
}
