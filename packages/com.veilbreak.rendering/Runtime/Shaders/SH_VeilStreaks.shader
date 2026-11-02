// LensStreakFeature's passes: 0 prefilter (soft-threshold HDR highlights into a quarter-res target, dimmed toward the middle of
// the view by _CenterFade: x = how much is gone at the centre, y = the distance, centre 0 to corner 1, where it is back to full), 1 one streak iteration
// along _Direction (15 taps at _Stride texels, each weighted by _Falloff per step, so three iterations at strides 1/8/64
// reach far across the screen), 2 additive composite of both finished streaks (_BlitTexture horizontal, _StreakB tilted), tinted, onto the camera colour.
Shader "Hidden/Veilbreak/Streaks"
{
    HLSLINCLUDE
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
    float4 _Texel, _Unit, _TintA, _TintB;
    TEXTURE2D_X(_StreakB); float2 _Direction; float _Threshold, _Stride, _Falloff;
    float4 _CenterFade;

    half3 Tap(float2 uv) { return SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv, 0).rgb; }

    half4 Prefilter(Varyings i) : SV_Target
    {
        // A 4-tap box over the quarter-res footprint, then a soft knee above the threshold, clamped so a single hot pixel can't blow out.
        float2 uv = i.texcoord; float2 o = _Texel.xy * 0.25;
        half3 c = (Tap(uv + float2(-o.x, -o.y)) + Tap(uv + float2(o.x, -o.y)) + Tap(uv + float2(-o.x, o.y)) + Tap(uv + float2(o.x, o.y))) * 0.25;
        half luma = max(c.r, max(c.g, c.b));
        half knee = 0.5 * _Threshold; half soft = clamp(luma - _Threshold + knee, 0, 2 * knee); soft = soft * soft / (4 * knee + 1e-4);
        half w = max(soft, luma - _Threshold) / max(luma, 1e-4);
        // A highlight near the middle of the view streaks less, so the flares gather toward the edges instead of crossing the subject.
        float aspect = _Unit.y / max(_Unit.x, 1e-6);
        float reach = length((uv - 0.5) * float2(aspect, 1)) / length(float2(0.5 * aspect, 0.5));
        w *= lerp(1 - _CenterFade.x, 1, smoothstep(0, max(_CenterFade.y, 1e-3), reach));
        // Anamorphic streaks take the lens coating's colour, not the light's: keep the highlight's energy, mostly drop its hue.
        return half4(min(lerp(luma.xxx, c, 0.2) * w, 24.0), 1);
    }

    half4 Streak(Varyings i) : SV_Target
    {
        float2 step = _Direction * _Unit.xy * _Stride;
        half3 sum = Tap(i.texcoord); half total = 1; half w = 1;
        [unroll] for (int k = 1; k <= 7; k++)
        {
            w *= _Falloff;
            sum += (Tap(i.texcoord + step * k) + Tap(i.texcoord - step * k)) * w; total += 2 * w;
        }
        // Normalised but slightly energy-keeping, so the long iterations still read as a bright line rather than a smear.
        return half4(sum / pow(abs(total), 0.85), 1); // total >= 1; abs() tells the compiler the base is non-negative
    }

    half4 Composite(Varyings i) : SV_Target { return half4(Tap(i.texcoord) * _TintA.rgb + SAMPLE_TEXTURE2D_X_LOD(_StreakB, sampler_LinearClamp, i.texcoord, 0).rgb * _TintB.rgb, 0); }
    ENDHLSL

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        ZTest Always ZWrite Off Cull Off
        Pass { Name "Prefilter" HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Prefilter
            ENDHLSL }
        Pass { Name "Streak" HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Streak
            ENDHLSL }
        Pass { Name "Composite" Blend One One HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Composite
            ENDHLSL }
    }
}
