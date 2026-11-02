// The painted sky dome: VeilSky's cylindrical plate (the same lookup the wet street reflects), drawn behind everything
// and centred on the camera, with a few live stars in the clear top band.
Shader "Veilbreak/Environment/Sky"
{
    Properties { }
    SubShader
    {
        Tags { "Queue" = "Background" "RenderType" = "Background" "PreviewType" = "Skybox" "RenderPipeline" = "UniversalPipeline" }
        Cull Off ZWrite Off
        Pass
        {
            Name "Sky"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "VeilCity.hlsl"
            struct A { float4 positionOS : POSITION; };
            struct V { float4 positionCS : SV_POSITION; float3 dir : TEXCOORD0; };
            V vert(A i)
            {
                V o; o.dir = i.positionOS.xyz;
                float4 p = TransformWorldToHClip(_WorldSpaceCameraPos + mul((float3x3)UNITY_MATRIX_M, i.positionOS.xyz));
                #if UNITY_REVERSED_Z
                p.z = p.w * 1e-6;
                #else
                p.z = p.w * (1.0 - 1e-6);
                #endif
                o.positionCS = p; return o;
            }
            half4 frag(V i) : SV_Target
            {
                float3 d = normalize(i.dir); half3 c = VeilSky(d);
                float3 sp = d * 220.0; float3 cell = floor(sp); float r = VeilHash(cell.xz + cell.y * 17.0);
                float star = step(0.985, r) * pow(saturate(1.0 - length(frac(sp) - 0.5) * 2.4), 6.0) * smoothstep(0.35, 0.7, d.y);
                c += star * (0.6 + 0.4 * sin(_VeilParams.y * 2.1 + r * 60.0)) * 1.6;
                return half4(c, 1);
            }
            ENDHLSL
        }
    }
}
