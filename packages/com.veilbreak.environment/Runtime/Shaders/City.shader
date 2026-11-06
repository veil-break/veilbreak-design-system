// One lit shader for every palette surface of the city kit (concrete, metal, glass, wood, foliage, paint, hazard and the
// emissive lamp, violet, tail-light and ember slots). Flat palette colours get world-space grime and rain streaks from
// the shared noise; up-facing faces darken and gloss with the director's wetness. Windows carry their light in the
// vertex colour, baked per module by StreetModules: rgb is the hue, a < 0.5 a steady intensity (a * 16), a >= 0.5 a
// flickering fire ((a - 0.5) * 16). The VeilReflection pass draws only the emission, for the mirrored wet-street pass.
Shader "Veilbreak/Environment/City"
{
    Properties
    {
        _BaseColor ("Colour", Color) = (0.58, 0.54, 0.62, 1)
        _Smoothness ("Smoothness", Range(0, 1)) = 0.2
        _Metallic ("Metallic", Range(0, 1)) = 0
        [HDR] _EmissionColor ("Emission", Color) = (0, 0, 0, 1)
        _Grime ("Grime", Range(0, 1)) = 0.6
        _Wet ("Takes rain", Range(0, 1)) = 1
        _Flicker ("Flicker", Range(0, 1)) = 0
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" "RenderPipeline" = "UniversalPipeline" }
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        CBUFFER_START(UnityPerMaterial)
        half4 _BaseColor, _EmissionColor; float _Smoothness, _Metallic, _Grime, _Wet, _Flicker;
        CBUFFER_END
        #include "VeilCity.hlsl"
        half3 Emission(float3 w, half4 colour)
        {
            float flick = VeilNoise(float2(_VeilParams.y * 0.7 + w.x * 0.11, w.z * 0.13 + w.y * 0.05)).g;
            half3 e = _EmissionColor.rgb * _VeilLight.y * lerp(1.0, 0.35 + flick * 1.3, _Flicker);
            float fire = step(0.5, colour.a), level = (colour.a - fire * 0.5) * 16.0;
            return e + colour.rgb * level * _VeilLight.x * lerp(1.0, 0.3 + flick * 1.4, fire);
        }
        ENDHLSL
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            struct A { float4 positionOS : POSITION; float3 normalOS : NORMAL; half4 color : COLOR; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct V { float4 positionCS : SV_POSITION; float3 w : TEXCOORD0; float3 n : TEXCOORD1; half4 color : TEXCOORD2; float fog : TEXCOORD3; };
            V vert(A i)
            {
                V o; UNITY_SETUP_INSTANCE_ID(i);
                o.w = TransformObjectToWorld(i.positionOS.xyz); o.n = TransformObjectToWorldNormal(i.normalOS); o.color = i.color;
                o.positionCS = TransformWorldToHClip(o.w); o.fog = ComputeFogFactor(o.positionCS.z);
                return o;
            }
            half4 frag(V i) : SV_Target
            {
                float3 n = normalize(i.n), w = i.w; float up = saturate(n.y), wall = 1.0 - abs(n.y);
                half grime = lerp(VeilNoise(float2(w.x + w.z, w.y) * 0.19).g, VeilNoise(w.xz * 0.19).g, up);
                half streak = VeilNoise(float2((w.x + w.z) * 0.9, w.y * 0.04)).r;
                half3 albedo = _BaseColor.rgb * lerp(1.0, lerp(0.62, 1.12, grime) * (1.0 - streak * wall * 0.35), _Grime);
                float wet = _VeilParams.z * _Wet * smoothstep(0.3, 0.9, up);
                albedo *= lerp(1.0, 0.55, wet);
                InputData input = (InputData)0;
                input.positionWS = w; input.normalWS = n; input.viewDirectionWS = SafeNormalize(GetWorldSpaceViewDir(w));
                input.shadowCoord = TransformWorldToShadowCoord(w); input.fogCoord = i.fog; input.bakedGI = SampleSH(n);
                input.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(i.positionCS); input.shadowMask = 1;
                SurfaceData s = (SurfaceData)0;
                s.albedo = albedo; s.metallic = _Metallic; s.smoothness = saturate(lerp(_Smoothness * lerp(0.75, 1.1, grime), 0.88, wet));
                s.normalTS = float3(0, 0, 1); s.occlusion = 1; s.alpha = 1; s.emission = Emission(w, i.color);
                half4 c = UniversalFragmentPBR(input, s);
                c.rgb = MixFog(c.rgb, i.fog);
                return half4(c.rgb, 1);
            }
            ENDHLSL
        }
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On ColorMask 0
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            float3 _LightDirection;
            struct A { float4 positionOS : POSITION; float3 normalOS : NORMAL; UNITY_VERTEX_INPUT_INSTANCE_ID };
            float4 vert(A i) : SV_POSITION
            {
                UNITY_SETUP_INSTANCE_ID(i);
                float3 w = TransformObjectToWorld(i.positionOS.xyz), n = TransformObjectToWorldNormal(i.normalOS);
                float4 p = TransformWorldToHClip(ApplyShadowBias(w, n, _LightDirection));
                #if UNITY_REVERSED_Z
                p.z = min(p.z, UNITY_NEAR_CLIP_VALUE);
                #else
                p.z = max(p.z, UNITY_NEAR_CLIP_VALUE);
                #endif
                return p;
            }
            half4 frag() : SV_Target { return 0; }
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
        Pass
        {
            Name "VeilReflection"
            Tags { "LightMode" = "VeilReflection" }
            Blend One One ZWrite Off ZTest Always Cull Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            float4x4 _VeilMirrorClip;
            struct A { float4 positionOS : POSITION; half4 color : COLOR; };
            struct V { float4 positionCS : SV_POSITION; float3 w : TEXCOORD0; half4 color : TEXCOORD1; };
            V vert(A i) { V o; o.w = TransformObjectToWorld(i.positionOS.xyz); o.positionCS = mul(_VeilMirrorClip, float4(o.w, 1)); o.color = i.color; return o; }
            half4 frag(V i) : SV_Target { return half4(Emission(i.w, i.color) * smoothstep(-0.2, 0.6, i.w.y), 1); }
            ENDHLSL
        }
    }
    // No FallBack: with one, Material.FindPass sees only the fallback pass in the editor and in play, so ReflectionSet could not find VeilReflection.
    FallBack Off
}
