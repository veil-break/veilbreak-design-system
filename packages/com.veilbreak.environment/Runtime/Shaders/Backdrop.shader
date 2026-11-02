// Painted backdrop layers (skyline cards), ported from the donor's Veilbreak/Backdrop: unlit, straight alpha, outside
// distance fog but blended toward the fog colour by `_FogMix`, so a painted plate sits in the haze at any distance.
Shader "Veilbreak/Environment/Backdrop"
{
    Properties
    {
        _BaseMap ("Plate", 2D) = "white" {}
        [HDR] _Tint ("Tint", Color) = (1, 1, 1, 1)
        _FogMix ("Fog mix", Range(0, 1)) = 0.2
    }
    SubShader
    {
        Tags { "Queue" = "Transparent-100" "RenderType" = "Transparent" "IgnoreProjector" = "True" "RenderPipeline" = "UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Back
        Pass
        {
            Name "Backdrop"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST; half4 _Tint; float _FogMix;
            CBUFFER_END
            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            struct A { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct V { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; };
            V vert(A i) { V o; o.positionCS = TransformObjectToHClip(i.positionOS.xyz); o.uv = TRANSFORM_TEX(i.uv, _BaseMap); return o; }
            half4 frag(V i) : SV_Target
            {
                half4 c = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv) * _Tint;
                c.rgb = lerp(c.rgb, unity_FogColor.rgb, _FogMix);
                return c;
            }
            ENDHLSL
        }
    }
}
