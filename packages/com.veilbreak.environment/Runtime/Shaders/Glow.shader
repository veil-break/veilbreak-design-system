// Additive soft light for lamp pools on the ground, halos, light shafts and ember particles: the glow texture's alpha
// times the colour and the vertex colour. `_Lamp` ties the brightness to the director's lamp level, so pools fade with
// their lamps; fog fades the glow toward black instead of toward the fog colour, as additive light should.
Shader "Veilbreak/Environment/Glow"
{
    Properties
    {
        _BaseMap ("Glow", 2D) = "white" {}
        [HDR] _Color ("Colour", Color) = (1, 0.62, 0.35, 1)
        _Lamp ("Follows lamp brightness", Range(0, 1)) = 1
        _Near ("Near fade (m)", Float) = 0
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" "RenderPipeline" = "UniversalPipeline" }
        Blend One One ZWrite Off Cull Off Offset -1, -1
        Pass
        {
            Name "Glow"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST; half4 _Color; float _Lamp, _Near;
            CBUFFER_END
            #include "VeilCity.hlsl"
            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            struct A { float4 positionOS : POSITION; float2 uv : TEXCOORD0; half4 color : COLOR; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct V { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; half4 color : TEXCOORD1; float fog : TEXCOORD2; float near : TEXCOORD3; };
            V vert(A i)
            {
                V o; UNITY_SETUP_INSTANCE_ID(i);
                float3 w = TransformObjectToWorld(i.positionOS.xyz);
                o.positionCS = TransformWorldToHClip(w); o.uv = TRANSFORM_TEX(i.uv, _BaseMap); o.color = i.color;
                o.fog = ComputeFogFactor(o.positionCS.z); o.near = _Near > 0 ? saturate(distance(w, _WorldSpaceCameraPos) / _Near - 0.5) : 1;
                return o;
            }
            half4 frag(V i) : SV_Target
            {
                half a = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv).a * i.color.a * i.near;
                half3 c = _Color.rgb * i.color.rgb * a * lerp(1.0, _VeilLight.y, _Lamp);
                return half4(MixFogColor(c, half3(0, 0, 0), i.fog), 1);
            }
            ENDHLSL
        }
    }
}
