// Ground telegraphs in the Veilbreak style: a crisp, over-bright solid outline (bloom picks it up) with a soft glow hugging it, a danger
// hatch inside hostile footprints, a 20% wash inside allied previews and the windup sweeping out from the caster tick-exact. A second
// pass raises the light curtain GroundTelegraphs builds along each outline: thin vertical rays over a soft base, brightest at the floor,
// with dust drifting up through them (after Veilbreak's SH_VeilCurtain). Quads carry uv1 = 0 and curtain strips uv1.x = 1, so each pass
// collapses the other's geometry. _State.w = 0 holds every scroll and drift still (Reduced Motion).
Shader "Veilbreak/Feedback/Telegraph" {
 Properties { _Color("Color",Color)=(1,1,1,1) _Shape("Shape",Vector)=(0,1,0,0) _Size("Size",Vector)=(2,2,0,0) _State("State",Vector)=(0,0,0,1) }
 SubShader { Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent-10" "RenderType"="Transparent" "IgnoreProjector"="True" }
 HLSLINCLUDE
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 CBUFFER_START(UnityPerMaterial) half4 _Color; float4 _Shape,_Size,_State; CBUFFER_END
 float Hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
 float Hash1(float n){return frac(sin(n*12.9898)*43758.5453);}
 ENDHLSL
 Pass {
 Name "Telegraph"
 Blend SrcAlpha One ZWrite Off ZTest LEqual Cull Off Offset -1,-1
 HLSLPROGRAM
 #pragma vertex Vertex
 #pragma fragment Fragment
 // Overlapping footprints of the same side, set per projection by GroundTelegraphs: pairs of (centre xz, forward xz) and shape (+4 on x for a lower index).
 float4 _Others[16]; float _OtherCount;
 struct Attributes { float4 positionOS:POSITION;float2 uv:TEXCOORD0;float2 flag:TEXCOORD1; };
 struct Varyings { float4 positionCS:SV_POSITION;float2 local:TEXCOORD0;float2 world:TEXCOORD1; };
 Varyings Vertex(Attributes v){Varyings o;o.positionCS=v.flag.x>.5?float4(2,2,2,1):TransformObjectToHClip(v.positionOS.xyz);o.local=(v.uv-.5)*_Size.xy;o.world=TransformObjectToWorld(v.positionOS.xyz).xz;return o;}
 // Keep identical to TelegraphGeometry.Distance: x shape (0 circle, 1 cone, 2 capsule), y radius/half width, z half angle, w length.
 float Distance(float2 p,float4 s){
  if(s.x>1.5){p.y-=clamp(p.y,-s.w*.5,s.w*.5);return length(p)-s.y;}
  if(s.x>.5){float2 c=float2(sin(s.z),cos(s.z));p.x=abs(p.x);float l=length(p)-s.y;float m=length(p-c*clamp(dot(p,c),0,s.y));return max(l,m*sign(c.y*p.x-c.x*p.y));}
  return length(p)-s.y;}
 half4 Fragment(Varyings i):SV_Target {
  float2 p=i.local;float d=Distance(p,_Shape);float aa=max(fwidth(d),1e-4);
  // Merge with the same side (TelegraphGeometry.Owner): only the nearest footprint shades a pixel, so the union is filled once and outlined once.
  // Derivatives are taken first; the ownership test only decides whether this projection keeps the pixel.
  bool owned=true;[loop] for(int k=0;k<8;k++){if(k>=(int)_OtherCount)break;float4 a=_Others[k*2],s=_Others[k*2+1];bool lower=s.x>3.5;s.x-=lower?4:0;float2 q=i.world-a.xy;float o=Distance(float2(q.x*a.w-q.y*a.z,dot(q,a.zw)),s);owned=owned&&!(o<d||lower&&o==d);}
  if(!owned)discard;
  float phase=_State.x,progress=_State.y,hostile=_State.z,time=_Time.y*_State.w;
  bool lane=_Shape.x>1.5,cone=_Shape.x>.5&&!lane;
  float inside=1-smoothstep(-aa,aa,d);
  // A crisp solid edge 5 cm wide just inside the footprint, then a soft glow hugging it on both sides.
  float rim=1-smoothstep(0,aa*1.25,abs(d+.03)-.025);
  float hug=exp(-abs(d)*10)*(1-rim);
  // Danger hatch inside hostile footprints (drifting slowly), a plain wash inside allied ones; chevrons point along lanes and cones.
  float hatch=step(frac((p.x+p.y)*2.6-time*.25),.38);
  float chevron=(lane||cone)?step(frac((p.y-abs(p.x)*.7)*.9-time*.8),.16):0;
  float wash=hostile>.5?.05+hatch*.1:.2;
  float3 color=_Color.rgb;float edge=hostile>.5?2.4:1.8;float alpha;
  if(phase<.5){alpha=inside*(wash*.7+chevron*.1)+rim*edge+hug*.35;}
  else if(phase<1.5){
   // Countdown sweep from the origin, tick-exact; the leading edge heats toward white-gold on hostile attacks and the rim pulses faster as it lands.
   float reach=lane?p.y/max(_Shape.w,1e-3)+.5:length(p)/max(_Shape.y,1e-3);
   float swept=step(reach,progress),front=1-smoothstep(0,.035,abs(reach-progress));
   float urgency=.75+.25*sin(time*(6+progress*16));
   alpha=inside*(wash+swept*.16+front*.7+chevron*.05)+rim*edge*urgency+hug*(.3+.35*progress);
   color=lerp(color,float3(1,.82,.45),front*hostile);}
  else if(phase<2.5){float pulse=.85+.15*sin(time*5);alpha=inside*(wash*pulse+chevron*.06)+rim*edge*.8+hug*.3;}
  else{float keep=step(progress,Hash(floor(p*6)));alpha=(inside*.16+rim*.9)*keep*(1-progress);}
  return half4(color*max(1,alpha),saturate(alpha)*_Color.a);
 }
 ENDHLSL
 }
 Pass {
 Name "TelegraphCurtain"
 Tags { "LightMode"="UniversalForward" }
 Blend One One ZWrite Off ZTest LEqual Cull Off
 HLSLPROGRAM
 #pragma vertex Vertex
 #pragma fragment Fragment
 #pragma multi_compile_fog
 struct Attributes { float4 positionOS:POSITION;float2 uv:TEXCOORD0;float2 flag:TEXCOORD1;half4 color:COLOR; };
 struct Varyings { float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0;half4 color:COLOR;float3 world:TEXCOORD1;float fog:TEXCOORD2;float hostile:TEXCOORD3; };
 Varyings Vertex(Attributes v){Varyings o;float3 w=TransformObjectToWorld(v.positionOS.xyz);o.positionCS=v.flag.x<.5?float4(2,2,2,1):TransformWorldToHClip(w);o.uv=v.uv;o.color=v.color;o.world=w;o.fog=ComputeFogFactor(o.positionCS.z);o.hostile=v.flag.y;return o;}
 float Noise3(float3 p){float3 i=floor(p),f=frac(p);f=f*f*(3-2*f);float n=dot(i,float3(1,57,113));
  return lerp(lerp(lerp(Hash1(n),Hash1(n+1),f.x),lerp(Hash1(n+57),Hash1(n+58),f.x),f.y),lerp(lerp(Hash1(n+113),Hash1(n+114),f.x),lerp(Hash1(n+170),Hash1(n+171),f.x),f.y),f.z);}
 half4 Fragment(Varyings i):SV_Target {
  float t=_Time.y*_State.w,y=saturate(i.uv.y),x=i.uv.x;
  // Soft at the floor seam, then a steep fall upward: a glow hugging the edge with rays reaching higher.
  float hug=smoothstep(0,.04,y)*exp(-y*7);
  float reach=smoothstep(0,.08,y)*exp(-y*2.4)*(1-smoothstep(.75,1,y));
  // Vertical rays: two layers of value noise along the edge, sharpened into thin shafts that slowly drift.
  float cell=x*7+t*.09;float k=floor(cell);float f=frac(cell);f=f*f*(3-2*f);float rays=lerp(Hash1(k),Hash1(k+1),f);
  cell=x*17-t*.13;k=floor(cell);f=frac(cell);f=f*f*(3-2*f);rays=rays*.55+lerp(Hash1(k+17),Hash1(k+18),f)*.45;
  rays=pow(saturate(rays),5)*2.4;
  float rise=hug*.55+reach*rays;
  // Dust lifting through the light.
  float dust=.65+.55*Noise3(i.world*float3(1.6,.8,1.6)+float3(0,-t*.45,0));
  float gain=i.hostile>.5?1.1:.8;
  float3 col=i.color.rgb*rise*dust*gain;
  return half4(MixFogColor(col,0,i.fog),0);
 }
 ENDHLSL
 } }
}
