// Drifting ground fog sheets (ported from the donor's Veilbreak/Fog): two layers of the shared noise texture instead of
// per-pixel fBm, scrolling in road coordinates so the haze travels with the street, faded at the quad edges and near
// the camera.
Shader "Veilbreak/Environment/Fog"
{
    Properties
    {
        _Color ("Colour", Color) = (0.42, 0.3, 0.5, 1)
        _Density ("Density", Range(0, 1)) = 0.35
        _Scale ("Scale (m)", Float) = 14
    }
    SubShader
    {
        Tags { "Queue" = "Transparent-20" "RenderType" = "Transparent" "IgnoreProjector" = "True" "RenderPipeline" = "UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Off
        Pass
        {
            Name "Fog"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
            half4 _Color; float _Density, _Scale;
            CBUFFER_END
            #include "VeilCity.hlsl"
            struct A { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct V { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; float3 w : TEXCOORD1; };
            V vert(A i) { V o; o.w = TransformObjectToWorld(i.positionOS.xyz); o.positionCS = TransformWorldToHClip(o.w); o.uv = i.uv; return o; }
            half4 frag(V i) : SV_Target
            {
                float2 road = VeilRoad(i.w) / _Scale; float t = _VeilParams.y;
                half n = VeilNoise(road + float2(t * 0.012, t * 0.004)).r * 0.6 + VeilNoise(road * 2.3 - float2(t * 0.02, 0)).g * 0.4;
                float2 e = min(i.uv, 1.0 - i.uv); float mask = smoothstep(0.0, 0.25, e.x) * smoothstep(0.0, 0.25, e.y);
                float near = smoothstep(4.0, 12.0, distance(i.w, _WorldSpaceCameraPos));
                return half4(_Color.rgb * (0.7 + n * 0.6), saturate(smoothstep(0.3, 0.8, n) * _Density * mask * near));
            }
            ENDHLSL
        }
    }
}
