// The combat accent pass. _Pulse (the Cinematic Pulse volume) streaks highlights and splits red slightly. Up to four screen impacts
// (ScreenImpacts) each send a refracting shockwave ring out from where a blow landed, punch the frame toward it with a short radial
// zoom and chromatic split, and tint a vignette in the hit's colour. Each impact's xy is viewport position, z progress, w motion
// strength; its colour's alpha is the flash weight. Weights are zero under Reduced Motion / Reduced Flashes (set by the host).
Shader "Veilbreak/Feedback/CinematicPulse" {
 SubShader { Tags { "RenderPipeline"="UniversalPipeline" } Pass { ZWrite Off Cull Off ZTest Always
 HLSLPROGRAM
 #pragma vertex Vert
 #pragma fragment Frag
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
 float _Pulse; float4 _Impacts[4]; float4 _ImpactColors[4];
 half3 Sample(float2 uv){return SAMPLE_TEXTURE2D_X(_BlitTexture,sampler_LinearClamp,uv).rgb;}
 half4 Frag(Varyings input):SV_Target {
  float2 uv=input.texcoord;float aspect=_ScreenParams.x/max(_ScreenParams.y,1);
  float2 shift=0;float punch=0,split=0;float3 tint=0;float tintWeight=0;float2 toward=0;
  [unroll] for(int i=0;i<4;i++){float4 k=_Impacts[i];float4 c=_ImpactColors[i];if(k.w<=0&&c.a<=0)continue;
   float2 d=uv-k.xy;d.x*=aspect;float r=length(d);float2 dir=r>1e-4?d/r:0;dir.x/=aspect;float p=k.z;float fade=1-p;
   // The shockwave: a ring of refraction expanding to ~70% of the screen, strongest just behind its front.
   float front=p*.75;float ring=exp(-pow((r-front)/.045,2))*fade;shift+=dir*ring*.028*k.w;
   // The punch: an early, short pull toward the hit with a radial chromatic split.
   float early=saturate(1-p*3.2);punch+=early*k.w;split+=early*k.w*(.35+.65*saturate(r));toward+=k.xy*early*k.w;
   // The vignette: the hit's colour gathering at the edges and a brief lift near the blow.
   float2 e=uv*2-1;float edge=saturate(dot(e,e)*.55);float w=c.a*fade*fade;tint+=c.rgb*(edge*.55+exp(-r*6)*.25*early)*w;tintWeight+=w;}
  float2 base=uv+shift;
  // Radial zoom toward the punch centre (weighted), four taps.
  float2 centre=punch>0?toward/punch:float2(.5,.5);float zoom=saturate(punch)*.035;half3 col=0;
  [unroll] for(int t=0;t<4;t++){float s=1-zoom*t/3;col+=Sample(centre+(base-centre)*s);}col*=.25;
  // Chromatic split away from the centre.
  float2 axis=base-centre;float amount=saturate(split)*.012;
  col.r=lerp(col.r,Sample(base+axis*amount).r,saturate(split));col.b=lerp(col.b,Sample(base-axis*amount).b,saturate(split));
  col+=tint;
  // The original pulse: a faint scanline shear and horizontal highlight streaks.
  float p=saturate(_Pulse);float scan=step(.995,frac(uv.y*91+_Time.y*2));float2 sp=base;sp.x+=scan*p*.004;
  half3 streak=0;[unroll]for(int j=1;j<=4;j++){float2 o=float2(j*.008*p,0);streak+=max(0,Sample(sp+o)-1)+max(0,Sample(sp-o)-1);}
  col+=streak*(p*.035);col.r=lerp(col.r,Sample(sp+float2(.0015*p,0)).r,p*.25);
  return half4(col,1);
 }
 ENDHLSL
 } }
}
