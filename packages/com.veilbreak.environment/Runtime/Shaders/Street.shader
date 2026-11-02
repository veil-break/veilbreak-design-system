// Wet asphalt and paving for the streamed boulevard. Patterns are laid out in road coordinates (world x, distance along
// the road), so they run seamlessly across segments: lane paint, cracks, manholes and standing water grown from the
// shared noise. Water darkens and flattens the surface and reflects the painted sky plus, on Standard, the mirrored
// emissive pass (`_VEIL_HIGH`, set by EnvironmentDirector with the reflection feature). `_SIDEWALK` swaps the lane paint
// for 1 m paving slabs and keeps curb faces on their mesh normals.
Shader "Veilbreak/Environment/Street"
{
    Properties
    {
        _BaseMap ("Albedo", 2D) = "white" {}
        [Normal] _BumpMap ("Normal", 2D) = "bump" {}
        _Tint ("Tint", Color) = (0.62, 0.58, 0.7, 1)
        _Paint ("Lane paint", Color) = (0.78, 0.76, 0.7, 1)
        _Tiling ("Tiling (per metre)", Float) = 0.25
        _BumpScale ("Normal strength", Float) = 0.8
        _Smoothness ("Dry smoothness", Range(0, 1)) = 0.32
        _Puddles ("Puddle bias", Range(-0.3, 0.3)) = 0
        [Toggle(_SIDEWALK)] _Sidewalk ("Paving", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" "RenderPipeline" = "UniversalPipeline" }
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        CBUFFER_START(UnityPerMaterial)
        float4 _BaseMap_ST; half4 _Tint, _Paint; float _Tiling, _BumpScale, _Smoothness, _Puddles, _Sidewalk;
        CBUFFER_END
        ENDHLSL
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature_local _SIDEWALK
            #pragma multi_compile_fragment _ _VEIL_HIGH
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "VeilCity.hlsl"
            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            TEXTURE2D(_BumpMap); SAMPLER(sampler_BumpMap);
            struct A { float4 positionOS : POSITION; float3 normalOS : NORMAL; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct V { float4 positionCS : SV_POSITION; float3 w : TEXCOORD0; float3 n : TEXCOORD1; float fog : TEXCOORD2; };
            V vert(A i)
            {
                V o; UNITY_SETUP_INSTANCE_ID(i);
                o.w = TransformObjectToWorld(i.positionOS.xyz); o.n = TransformObjectToWorldNormal(i.normalOS);
                o.positionCS = TransformWorldToHClip(o.w); o.fog = ComputeFogFactor(o.positionCS.z);
                return o;
            }
            half4 frag(V i) : SV_Target
            {
                float open; float2 road = VeilStreet(i.w, open); float ax = abs(road.x);
                half4 big = VeilNoise(road / 32.0), fine = VeilNoise(road / 6.4 + 0.37);
                float2 tuv = road * _Tiling;
                half3 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, tuv).rgb * _Tint.rgb;
                half3 nt = UnpackNormalScale(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, tuv), _BumpScale);
                albedo *= lerp(0.8, 1.15, fine.g) * lerp(0.85, 1.05, big.a);
                float metal = 0, crack = 0, level = big.r * 0.8 + fine.r * 0.2 + _Puddles;
                #if _SIDEWALK
                float2 sf = frac(road), sid = floor(road);
                float joint = 1.0 - smoothstep(0.015, 0.045, min(min(sf.x, 1.0 - sf.x), min(sf.y, 1.0 - sf.y)));
                albedo *= lerp(0.82, 1.12, VeilHash(sid)) * (1.0 - joint * 0.5);
                level += joint * 0.08 - 0.1;
                #else
                // Lane paint: solid edge lines and a dashed centre line (4 m in every 8), worn where the grime is light.
                float edge = 1.0 - smoothstep(0.07, 0.09, abs(ax - 5.35));
                float centre = (1.0 - smoothstep(0.06, 0.08, ax)) * step(frac(road.y / 8.0), 0.5);
                float paint = saturate(edge + centre) * smoothstep(0.2, 0.5, fine.g) * (1.0 - open);
                albedo = lerp(albedo, _Paint.rgb, paint * 0.85);
                crack = fine.b * smoothstep(0.5, 0.75, big.a);
                albedo *= 1.0 - crack * 0.65;
                // A manhole every 32 m, alternating lanes; its grate stays dry.
                float cell = floor(road.y / 32.0);
                float2 hole = float2((VeilHash(float2(cell, 1.7)) > 0.5 ? 1.0 : -1.0) * 2.7, cell * 32.0 + 10.0 + VeilHash(float2(cell, 9.1)) * 12.0);
                float d = length(road - hole), cover = (1.0 - smoothstep(0.4, 0.42, d)) * (1.0 - open);
                float grate = cover * (step(0.5, frac((road.x - hole.x) * 5.0)) * (1.0 - smoothstep(0.3, 0.32, d)) + smoothstep(0.34, 0.36, d));
                albedo = lerp(albedo, half3(0.09, 0.085, 0.1) * (0.6 + grate * 0.8), cover);
                metal = cover * 0.6; level -= cover;
                // Gutters collect water where the road cambers down to the curb.
                level += smoothstep(4.0, 5.9, ax) * 0.14 * (1.0 - open);
                #endif
                float threshold = lerp(0.86, 0.6, _VeilParams.z);
                float wet = smoothstep(threshold, threshold + 0.035, level) * (1.0 - crack * 0.5);
                float damp = saturate(_VeilParams.z * 0.8 + wet);
                albedo *= lerp(1.0, 0.42, damp);
                float smooth = lerp(_Smoothness * lerp(0.7, 1.2, fine.g), 0.62, _VeilParams.z);
                smooth = lerp(smooth, 0.97, wet);
                float3 n = i.n.y > 0.5 ? normalize(float3(nt.x, nt.z, nt.y)) : normalize(i.n);
                n = normalize(lerp(n, float3(0, 1, 0), wet * 0.95));
                #if _VEIL_HIGH
                float2 rip = VeilRipple(road) * _VeilParams.w * wet;
                n = normalize(n + float3(rip.x, 0, rip.y) * 0.4);
                #endif
                InputData input = (InputData)0;
                input.positionWS = i.w; input.normalWS = n; input.viewDirectionWS = SafeNormalize(GetWorldSpaceViewDir(i.w));
                input.shadowCoord = TransformWorldToShadowCoord(i.w); input.fogCoord = i.fog; input.bakedGI = SampleSH(n);
                input.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(i.positionCS); input.shadowMask = 1;
                SurfaceData s = (SurfaceData)0;
                s.albedo = albedo; s.metallic = metal; s.smoothness = saturate(smooth); s.normalTS = float3(0, 0, 1); s.occlusion = 1.0 - crack * 0.4; s.alpha = 1;
                half4 c = UniversalFragmentPBR(input, s);
                float f = 0.04 + 0.96 * pow(1.0 - saturate(dot(n, input.viewDirectionWS)), 5.0);
                float k = saturate(lerp(_VeilParams.z * 0.22, 0.92, wet) * lerp(0.55, 1.0, f));
                #if _VEIL_HIGH
                half3 reflection = VeilReflect(n, input.viewDirectionWS, VeilMirrorUV(i.w), 1.0 - smooth);
                #else
                half3 reflection = VeilReflectedSky(reflect(-input.viewDirectionWS, n)) * _VeilLight.z;
                #endif
                c.rgb = c.rgb * (1.0 - k * 0.45) + reflection * k;
                c.rgb = MixFog(c.rgb, i.fog);
                return half4(c.rgb, 1);
            }
            ENDHLSL
        }
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On ColorMask R
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            struct A { float4 positionOS : POSITION; UNITY_VERTEX_INPUT_INSTANCE_ID };
            float4 vert(A i) : SV_POSITION { UNITY_SETUP_INSTANCE_ID(i); return TransformObjectToHClip(i.positionOS.xyz); }
            half frag() : SV_Target { return 0; }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
