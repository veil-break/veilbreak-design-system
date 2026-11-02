// Full-screen signal glitch (FullScreenPassRendererFeature after post-processing). ScreenGlitchFeature sets the globals from
// the Glitch volume override's blended values: _VeilGlitch 0-1 strength (0 = untouched image), _VeilGlitchFlash 0-1 for the
// brightness pop. Globals rather than material values, so play never edits the material asset.
// Rows tear sideways in random bands, the channels split, blocks of the image repeat a frame of noise and scanlines crawl.
Shader "Veilbreak/Glitch"
{
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off ZTest Always Cull Off Blend Off
        Pass
        {
            Name "VeilGlitch"
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #pragma vertex Vert
            #pragma fragment Frag
            float _VeilGlitch, _VeilGlitchFlash;
            float H(float2 p) { return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453); }
            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float2 uv = input.texcoord; float g = saturate(_VeilGlitch);
                if (g < .002) return SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv);
                float frame = floor(_Time.y * 30);
                // Torn bands: coarse rows (their count changes every frame) shift sideways, a few far.
                float rows = 6 + floor(H(float2(frame, 1)) * 26); float row = floor(uv.y * rows);
                float tear = H(float2(row, frame)); float shift = tear > 1 - g * .55 ? (H(float2(row, frame + 7)) - .5) * g * .16 : 0;
                // Thin jitter lines and a slow roll bar.
                shift += (H(float2(floor(uv.y * 240), frame)) - .5) * g * .006;
                float2 st = float2(uv.x + shift, uv.y);
                // Blocky corruption: some cells sample a neighbouring cell.
                float2 cell = floor(st * float2(16, 9)); float bad = step(1 - g * .18, H(cell + frame));
                st = lerp(st, st + (float2(H(cell + 3), H(cell + 5)) - .5) * .08, bad);
                // Channel split, wider where the band tore.
                float split = g * (.004 + abs(shift) * .5);
                half3 col;
                col.r = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, st + float2(split, 0)).r;
                col.g = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, st).g;
                col.b = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, st - float2(split, 0)).b;
                // Scanlines, a cyan/magenta cast on corrupted cells and torn bands flaring.
                col *= 1 - g * .22 * (.5 + .5 * sin(uv.y * 900 + _Time.y * 40));
                col = lerp(col, col * half3(.6, 1.25, 1.35) + half3(.05, .0, .08), bad * g);
                col += _VeilGlitchFlash * g * .22 * step(.001, abs(shift)) * H(float2(row, frame + 3)); // only torn bands flare
                return half4(col, 1);
            }
            ENDHLSL
        }
    }
}
