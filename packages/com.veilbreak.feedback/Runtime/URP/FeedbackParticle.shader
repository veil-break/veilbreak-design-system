// Combat effect particles and ground marks from the shared atlas (Runtime/Textures/T_FxAtlas.png). Each emitter picks
// one tile with `_Tile` (uv scale and offset, set per renderer), so the whole effect set costs one texture. Vertex colour
// is LDR on particles, so additive materials lift it with `_Intensity` for bloom. Blend comes from the material:
// additive (SrcAlpha, One) for light, alpha (SrcAlpha, OneMinusSrcAlpha) for smoke, debris and marks. Additive light
// fades to black in fog; alpha shapes fade to the fog colour.
Shader "Veilbreak/Feedback/Particle"
{
    Properties
    {
        _MainTex ("Atlas", 2D) = "white" {}
        _Tile ("Tile (scale, offset)", Vector) = (1, 1, 0, 0)
        _Intensity ("Intensity", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Source blend", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Destination blend", Float) = 1
        [Toggle] _Additive ("Additive fog", Float) = 1
        _DepthOffset ("Depth offset", Float) = 0
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
        Pass
        {
            Name "Particle"
            Blend [_SrcBlend] [_DstBlend]
            ZWrite Off Cull Off
            Offset [_DepthOffset], [_DepthOffset]
            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST, _Tile; half _Intensity, _Additive, _SrcBlend, _DstBlend, _DepthOffset;
            CBUFFER_END
            struct Attributes { float4 positionOS : POSITION; half4 color : COLOR; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; half4 color : COLOR; float2 uv : TEXCOORD0; float fog : TEXCOORD1; };
            Varyings Vertex(Attributes v)
            {
                Varyings o; o.positionCS = TransformObjectToHClip(v.positionOS.xyz); o.color = v.color;
                o.uv = v.uv * _Tile.xy + _Tile.zw; o.fog = ComputeFogFactor(o.positionCS.z);
                return o;
            }
            half4 Fragment(Varyings i) : SV_Target
            {
                half4 t = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv) * i.color;
                t.rgb *= _Intensity;
                t.rgb = _Additive > 0.5 ? MixFogColor(t.rgb, half3(0, 0, 0), i.fog) : MixFog(t.rgb, i.fog);
                return t;
            }
            ENDHLSL
        }
    }
    FallBack Off
}
