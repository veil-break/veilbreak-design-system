using UnityEngine;
using UnityEngine.Rendering;
namespace Veilbreak.Feedback {
/// <summary>Tiles of Runtime/Textures/T_FxAtlas.png (4 × 4, row 0 at the top); keep in sync with Tools~/fx_atlas.py.</summary>
public enum FxTile { Glow, Spark, Smoke, Ring, Plus, Shard, Flame, Flare, Scorch, Crack, Blob, Chevron, SmokeB, Splash, Ember, Band }
/// <summary>Where a set piece plays, in world metres. Colours are LDR (additive layers lift them); magnitude 1 is the normal piece.</summary>
public struct FxCall { public Vector3 from,to; public Color color; public float magnitude,radius,angle; }
/// <summary>Combat effects as a handful of shared particle systems, one per look (sparks, glow, embers, flame, smoke, rings,
/// shards, heal crosses, splashes, flares), all drawing from one atlas. Set pieces <see cref="Emit"/> into them, so any
/// number of effects costs at most ten draw calls. Particles live in the space of <see cref="Ground"/>, a transform the
/// consumer slides with the road, so smoke and debris drift away with the street. Caps are fixed per tier at
/// <see cref="Initialize"/>; a per-frame budget drops low-priority pieces first. Advance with the simulation delta: nothing
/// moves while the game is paused, and effects speed up with the clock.</summary>
public sealed class EffectEmitters:MonoBehaviour {
 public enum Layer { Sparks, Glow, Embers, Flame, Smoke, Rings, Shards, Plus, Splash, Flares }
 /// <summary>Particles per frame before pieces below priority 2 are skipped.</summary>
 public const int HighBudget=560,LowBudget=190;
 struct Spec { public FxTile tile;public bool additive;public ParticleSystemRenderMode mode;public float gravity,drag;public int high,low;public AnimationCurve size;public Gradient fade; }
 static AnimationCurve Curve(params float[] keys){var c=new AnimationCurve();for(int i=0;i<keys.Length;i+=2)c.AddKey(keys[i],keys[i+1]);return c;}
 static Gradient Fade(Color end,params float[] alpha){var g=new Gradient();var a=new GradientAlphaKey[alpha.Length/2];for(int i=0;i<a.Length;i++)a[i]=new GradientAlphaKey(alpha[i*2+1],alpha[i*2]);g.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(end,1)},a);return g;}
 static Spec[] Specs()=>new[]{
  new Spec{tile=FxTile.Spark,additive=true,mode=ParticleSystemRenderMode.Stretch,gravity=.9f,drag=.8f,high=600,low=220,size=Curve(0,1,1,.25f),fade=Fade(Color.white,0,1,.7f,.7f,1,0)},
  new Spec{tile=FxTile.Glow,additive=true,mode=ParticleSystemRenderMode.Billboard,high=160,low=60,size=Curve(0,1,1,.2f),fade=Fade(Color.white,0,1,.5f,.6f,1,0)},
  new Spec{tile=FxTile.Ember,additive=true,mode=ParticleSystemRenderMode.Billboard,gravity=-.08f,drag=.6f,high=400,low=140,size=Curve(0,.5f,.2f,1,1,0),fade=Fade(Color.white,0,0,.1f,1,.8f,.8f,1,0)},
  new Spec{tile=FxTile.Flame,additive=true,mode=ParticleSystemRenderMode.Billboard,gravity=-.3f,drag=1.5f,high=160,low=50,size=Curve(0,.5f,.3f,1,1,.35f),fade=Fade(new Color(.55f,.18f,.1f),0,0,.08f,1,.6f,.7f,1,0)},
  new Spec{tile=FxTile.Smoke,additive=false,mode=ParticleSystemRenderMode.Billboard,gravity=-.05f,drag=1.2f,high=160,low=50,size=Curve(0,.35f,1,1),fade=Fade(Color.white,0,0,.12f,1,1,0)},
  new Spec{tile=FxTile.Ring,additive=true,mode=ParticleSystemRenderMode.HorizontalBillboard,high=48,low=20,size=Curve(0,.15f,1,1),fade=Fade(Color.white,0,1,1,0)},
  new Spec{tile=FxTile.Shard,additive=false,mode=ParticleSystemRenderMode.Billboard,gravity=1.5f,drag=.3f,high=160,low=50,size=Curve(0,1,1,.8f),fade=Fade(Color.white,0,1,.8f,1,1,0)},
  new Spec{tile=FxTile.Plus,additive=true,mode=ParticleSystemRenderMode.Billboard,drag=1,high=60,low=24,size=Curve(0,.5f,.2f,1,1,.7f),fade=Fade(Color.white,0,0,.1f,1,.7f,.8f,1,0)},
  new Spec{tile=FxTile.Splash,additive=false,mode=ParticleSystemRenderMode.HorizontalBillboard,high=64,low=24,size=Curve(0,.3f,1,1),fade=Fade(Color.white,0,1,1,0)},
  new Spec{tile=FxTile.Flare,additive=true,mode=ParticleSystemRenderMode.Billboard,high=48,low=16,size=Curve(0,1,1,.3f),fade=Fade(Color.white,0,1,1,0)}};
 static readonly int TileId=Shader.PropertyToID("_Tile");
 ParticleSystem[] systems;bool high;int spent;readonly System.Random random=new System.Random(17);
 /// <summary>The road-anchored space every particle lives in.</summary>
 public Transform Ground{get;private set;}
 public bool High=>high;
 public int Budget=>high?HighBudget:LowBudget;
 /// <summary>Live particles across every layer.</summary>
 public int Particles{get{int n=0;if(systems!=null)foreach(var ps in systems)n+=ps.particleCount;return n;}}
 /// <summary>Most particles the layers can hold together on this tier.</summary>
 public int Capacity{get{int n=0;if(systems!=null)foreach(var ps in systems)n+=ps.main.maxParticles;return n;}}
 public static Vector4 Tile(FxTile tile){int i=(int)tile;return new Vector4(.25f,.25f,(i%4)*.25f,1-(i/4+1)*.25f);}
 public void Initialize(Material additive,Material alpha,bool high){this.high=high;Ground=new GameObject("Effects ground").transform;Ground.SetParent(transform,false);var specs=Specs();systems=new ParticleSystem[specs.Length];var block=new MaterialPropertyBlock();
  for(int i=0;i<specs.Length;i++){var s=specs[i];var go=new GameObject(((Layer)i).ToString());go.transform.SetParent(transform,false);var ps=go.AddComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
   var main=ps.main;main.loop=false;main.playOnAwake=false;main.maxParticles=high?s.high:s.low;main.simulationSpace=ParticleSystemSimulationSpace.Custom;main.customSimulationSpace=Ground;main.gravityModifier=s.gravity;main.scalingMode=ParticleSystemScalingMode.Shape;
   var emission=ps.emission;emission.enabled=false;var shape=ps.shape;shape.enabled=false;
   var size=ps.sizeOverLifetime;size.enabled=true;size.size=new ParticleSystem.MinMaxCurve(1,s.size);var colour=ps.colorOverLifetime;colour.enabled=true;colour.color=new ParticleSystem.MinMaxGradient(s.fade);
   if(s.drag>0){var limit=ps.limitVelocityOverLifetime;limit.enabled=true;limit.limit=new ParticleSystem.MinMaxCurve(100);limit.drag=new ParticleSystem.MinMaxCurve(s.drag);limit.multiplyDragByParticleVelocity=true;}
   var renderer=go.GetComponent<ParticleSystemRenderer>();renderer.renderMode=s.mode;renderer.sharedMaterial=s.additive?additive:alpha;renderer.maxParticleSize=1.5f;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;renderer.lightProbeUsage=LightProbeUsage.Off;renderer.reflectionProbeUsage=ReflectionProbeUsage.Off;
   if(s.mode==ParticleSystemRenderMode.Stretch){renderer.velocityScale=.035f;renderer.lengthScale=1.2f;}if(!s.additive)renderer.sortingFudge=10;block.SetVector(TileId,Tile(s.tile));renderer.SetPropertyBlock(block);
   ps.Play();ps.Pause();systems[i]=ps;}}
 /// <summary>Slides the effect space with the road; positive distance moves everything toward −z.</summary>
 public void Drift(float metres){if(Ground!=null)Ground.position-=new Vector3(0,0,metres);}
 public void Advance(float delta){spent=0;if(systems==null||delta<=0)return;foreach(var ps in systems)ps.Simulate(delta,false,false,false);}
 public void Clear(){if(systems!=null)foreach(var ps in systems)ps.Clear(false);}
 /// <summary>Spends <paramref name="cost"/> particles of this frame's budget; pieces at priority 2 or above always play.</summary>
 public bool Allow(int priority,int cost){if(priority<2&&spent+cost>Budget)return false;spent+=cost;return true;}
 float R(float a,float b)=>a+(float)random.NextDouble()*(b-a);
 Vector3 Sphere(){var v=new Vector3(R(-1,1),R(-1,1),R(-1,1));return v.sqrMagnitude<1e-4f?Vector3.up:v.normalized;}
 int N(float count)=>Mathf.Max(1,Mathf.RoundToInt(count*(high?1:.45f)));
 /// <summary>One particle into a layer, at a world position.</summary>
 public void Emit(Layer layer,Vector3 at,Vector3 velocity,float size,float life,Color color,float spin=0){if(systems==null)return;
  var p=new ParticleSystem.EmitParams{position=Ground.InverseTransformPoint(at),velocity=velocity,startSize=size,startLifetime=life,startColor=color,rotation=R(0,360),angularVelocity=spin*R(-1,1)};systems[(int)layer].Emit(p,1);}
 /// <summary>Plays a set piece. Returns false when the frame budget dropped it.</summary>
 public bool Play(EffectRoutine routine,in FxCall c,int priority=0){
  float m=Mathf.Max(.2f,c.magnitude);var col=c.color;var to=c.to;var from=c.from;var dir=to-from;dir.y=0;dir=dir.sqrMagnitude>1e-4f?dir.normalized:Vector3.forward;
  switch(routine){
   case EffectRoutine.Impact:if(!Allow(priority,N(9+8*m)))return false;
    Emit(Layer.Flares,to,Vector3.zero,.45f*m,.12f,Color.Lerp(col,Color.white,.45f));Emit(Layer.Glow,to,Vector3.zero,.7f*m,.18f,col);Emit(Layer.Rings,Flat(to),Vector3.zero,.9f*m,.2f,col);
    for(int i=N(6+8*m);i>0;i--)Emit(Layer.Sparks,to,(dir*.8f+Sphere()+Vector3.up*.4f).normalized*R(3,7)*Mathf.Sqrt(m),R(.05f,.09f),R(.18f,.4f),col);return true;
   case EffectRoutine.Explosion:return Explosion(to,col,m,priority);
   case EffectRoutine.Muzzle:if(!Allow(priority,N(6)))return false;
    Emit(Layer.Flares,from,Vector3.zero,.4f*m,.1f,Color.Lerp(col,Color.white,.4f));Emit(Layer.Glow,from+dir*.25f,dir*1.5f,.6f*m,.15f,col);
    for(int i=N(4*m);i>0;i--)Emit(Layer.Sparks,from,(dir*1.5f+Sphere()*.5f).normalized*R(4,8),R(.04f,.07f),R(.1f,.2f),col);
    for(int i=N(2);i>0;i--)Emit(Layer.Sparks,from,dir*R(14,20),R(.07f,.1f),.06f,Color.Lerp(col,Color.white,.6f));return true;
   case EffectRoutine.Beam:if(!Allow(priority,N(20*m)))return false;
    Emit(Layer.Glow,from,Vector3.zero,.9f*m,.25f,col);Emit(Layer.Flares,from,Vector3.zero,.7f*m,.12f,Color.Lerp(col,Color.white,.5f));Emit(Layer.Flares,to,Vector3.zero,.8f*m,.18f,col);Emit(Layer.Rings,Flat(to),Vector3.zero,1.2f*m,.3f,col);
    for(int i=N(5*m);i>0;i--)Emit(Layer.Sparks,Vector3.Lerp(from,to,R(0,.3f)),dir*R(16,24),R(.06f,.09f),R(.12f,.2f),Color.Lerp(col,Color.white,.5f));
    for(int i=N(12*m);i>0;i--){var at=Vector3.Lerp(from,to,R(0,1));Emit(Layer.Sparks,at,(Sphere()+Vector3.up*.3f)*R(1,3),R(.04f,.08f),R(.2f,.35f),col);}
    for(int i=N(5*m);i>0;i--)Emit(Layer.Embers,Vector3.Lerp(from,to,R(0,1)),Vector3.up*R(.2f,.8f),R(.06f,.12f),R(.5f,.9f),col);return true;
   case EffectRoutine.Cone:{if(!Allow(priority,N(18*m)))return false;float reach=Mathf.Max(1,c.radius),half=Mathf.Max(5,c.angle*.5f);
    Emit(Layer.Glow,from,Vector3.zero,1.1f*m,.2f,col);
    for(int i=N(16*m);i>0;i--){var d=Quaternion.Euler(0,R(-half,half),0)*dir;Emit(Layer.Sparks,from+Vector3.up*.2f,(d+Vector3.up*R(0,.25f))*reach/.3f*R(.6f,1),R(.05f,.1f),R(.22f,.32f),col);}
    for(int i=N(3*m);i>0;i--){var d=Quaternion.Euler(0,R(-half,half),0)*dir;Emit(Layer.Flame,from+d*reach*R(.3f,.8f),d*R(1,2)+Vector3.up,R(.5f,.9f)*m,R(.3f,.45f),col);}
    // The front: glow motes in a fan that travel out to the reach together, so the arc reads as one wave.
    int front=N(9);for(int i=0;i<front;i++){var d=Quaternion.Euler(0,Mathf.Lerp(-half,half,(i+.5f)/front),0)*dir;Emit(Layer.Glow,from+d*.4f+Vector3.up*.3f,d*reach/.28f,.55f*m,.28f,col);}
    for(int i=N(4*m);i>0;i--){var d=Quaternion.Euler(0,R(-half,half),0)*dir;Emit(Layer.Shards,from+d*reach*R(.85f,1)+Vector3.up*.2f,(d+Vector3.up*1.4f)*R(1.5f,3),R(.08f,.15f),R(.5f,.8f),new Color(.3f,.29f,.34f),400);}return true;}
   case EffectRoutine.Zone:{if(!Allow(priority,N(14*m)))return false;float r=Mathf.Max(.5f,c.radius);
    Emit(Layer.Rings,Flat(to),Vector3.zero,r*2.1f,.55f,col);Emit(Layer.Rings,Flat(to),Vector3.zero,r*1.2f,.3f,Color.Lerp(col,Color.white,.4f));Emit(Layer.Glow,to,Vector3.zero,r*.9f,.35f,col);Pillar(to,col,.8f*m);
    for(int i=N(12*m);i>0;i--){var o=Random2(r);Emit(Layer.Embers,Flat(to)+o+Vector3.up*.1f,Vector3.up*R(.6f,1.6f),R(.08f,.14f),R(.7f,1.2f),col);}return true;}
   case EffectRoutine.Dash:if(!Allow(priority,N(14*m)))return false;
    for(int i=N(6*m);i>0;i--){var at=Vector3.Lerp(from,to,R(0,1));at.y=.2f;Emit(Layer.Smoke,at,Vector3.up*R(.2f,.5f)+Sphere()*.3f,R(.6f,1),R(.6f,1),new Color(.42f,.4f,.46f,.32f),40);}
    for(int i=N(8*m);i>0;i--)Emit(Layer.Sparks,Vector3.Lerp(from,to,R(0,1))+Vector3.up*.5f,dir*R(4,8),R(.05f,.08f),R(.15f,.3f),col);
    for(int i=N(7*m);i>0;i--){var side=Vector3.Cross(Vector3.up,dir)*R(-.5f,.5f);Emit(Layer.Sparks,Vector3.Lerp(from,to,R(0,.8f))+side+Vector3.up*R(.3f,1.3f),dir*R(12,18),R(.03f,.05f),R(.08f,.14f),Color.Lerp(col,Color.white,.55f));}
    Emit(Layer.Glow,from+Vector3.up*.8f,Vector3.zero,1f,.3f,col*.7f);Emit(Layer.Glow,to,Vector3.zero,.8f,.2f,col);return true;
   case EffectRoutine.Wall:if(!Allow(priority,N(18*m)))return false;
    for(int i=N(8*m);i>0;i--){var at=Vector3.Lerp(from,to,R(0,1));at.y=.25f;Emit(Layer.Smoke,at,Vector3.up*R(.4f,1)+Sphere()*.4f,R(.8f,1.3f),R(.8f,1.4f),new Color(.4f,.38f,.44f,.38f),30);}
    for(int i=N(6*m);i>0;i--)Emit(Layer.Shards,Vector3.Lerp(from,to,R(0,1))+Vector3.up*.3f,(Vector3.up*2+Sphere())*R(1.5f,3),R(.1f,.2f),R(.6f,1),new Color(.33f,.32f,.38f),400);
    for(int i=N(6*m);i>0;i--)Emit(Layer.Sparks,Vector3.Lerp(from,to,R(0,1))+Vector3.up*.6f,(Sphere()+Vector3.up)*R(2,4),R(.05f,.08f),R(.2f,.35f),col);return true;
   case EffectRoutine.Mend:if(!Allow(priority,N(12*m)))return false;
    Emit(Layer.Glow,to,Vector3.zero,1.2f*m,.4f,col);Emit(Layer.Rings,Flat(to),Vector3.zero,1.6f*m,.5f,col);
    for(int i=N(3+2*m);i>0;i--)Emit(Layer.Plus,to+new Vector3(R(-.4f,.4f),R(0,.4f),R(-.4f,.4f)),Vector3.up*R(.8f,1.4f),R(.22f,.32f),R(.8f,1.1f),col);
    for(int i=N(8*m);i>0;i--)Emit(Layer.Embers,to+Random2(.6f),Vector3.up*R(.6f,1.4f),R(.06f,.1f),R(.6f,1),col);
    // Two strands winding up the body.
    int turns=N(12);for(int i=0;i<turns;i++){float a=i*.85f+(i%2)*Mathf.PI;var radial=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));Emit(Layer.Embers,Flat(to)+radial*.45f+Vector3.up*(.1f+i*.07f),Vector3.up*1.3f+new Vector3(-radial.z,0,radial.x)*1.1f,.09f,.75f,Color.Lerp(col,Color.white,.3f));}return true;
   case EffectRoutine.Ward:if(!Allow(priority,N(16*m)))return false;
    Emit(Layer.Flares,to,Vector3.zero,1.1f*m,.16f,col);Emit(Layer.Rings,Flat(to),Vector3.zero,1.8f*m,.35f,col);
    for(int i=N(6*m);i>0;i--)Emit(Layer.Shards,to,(Sphere()+Vector3.up*.6f)*R(3,5),R(.08f,.16f),R(.5f,.8f),Color.Lerp(col,Color.white,.6f),500);
    for(int i=N(8*m);i>0;i--)Emit(Layer.Sparks,to,(Sphere()+Vector3.up*.3f)*R(3,6),R(.05f,.08f),R(.2f,.35f),col);return true;
   case EffectRoutine.Charge:if(!Allow(priority,N(6*m)))return false;
    Emit(Layer.Glow,from+Vector3.up*.6f,Vector3.zero,.55f*m,.3f,col);
    for(int i=N(6*m);i>0;i--){var o=Sphere()*R(.7f,1.1f);Emit(Layer.Embers,from+Vector3.up*.6f+o,-o/.4f,R(.07f,.11f),.4f,col);}return true;
   case EffectRoutine.Portal:if(!Allow(priority,N(14*m)))return false;
    Emit(Layer.Rings,Flat(to),Vector3.zero,2.2f*m,.6f,col);Emit(Layer.Flares,to+Vector3.up*.8f,Vector3.zero,.9f*m,.2f,col);
    for(int i=N(3*m);i>0;i--)Emit(Layer.Smoke,Flat(to)+Random2(.6f)+Vector3.up*.3f,Vector3.up*R(.3f,.7f),R(1,1.6f),R(.8f,1.3f),new Color(.2f,.12f,.25f,.5f),30);
    for(int i=N(9*m);i>0;i--)Emit(Layer.Embers,Flat(to)+Random2(.5f),Vector3.up*R(1.5f,3),R(.07f,.12f),R(.5f,.8f),col);return true;
   case EffectRoutine.Pulse:if(!Allow(priority,N(16*m)))return false;
    Emit(Layer.Rings,Flat(to),Vector3.zero,3*m,.6f,col);Emit(Layer.Rings,Flat(to),Vector3.zero,5*m,.9f,col);Emit(Layer.Flares,to,Vector3.zero,1.4f*m,.2f,col);
    for(int i=N(12*m);i>0;i--)Emit(Layer.Sparks,to,(Sphere()+Vector3.up*1.5f).normalized*R(3,7),R(.05f,.09f),R(.3f,.6f),col);return true;
   case EffectRoutine.Reward:if(!Allow(priority,N(16*m)))return false;
    Emit(Layer.Rings,Flat(to),Vector3.zero,2.4f*m,.7f,col);Emit(Layer.Flares,to+Vector3.up*.6f,Vector3.zero,1.2f*m,.25f,col);
    for(int i=N(14*m);i>0;i--){float a=R(0,Mathf.PI*2);var o=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a))*R(.3f,.8f);Emit(Layer.Embers,Flat(to)+o,Vector3.up*R(1.5f,3)+new Vector3(-o.z,0,o.x)*2,R(.08f,.13f),R(.7f,1.1f),col);}return true;
   case EffectRoutine.Swap:if(!Allow(priority,N(8)))return false;
    Emit(Layer.Flares,to+Vector3.up*.7f,Vector3.zero,.9f*m,.14f,col);Emit(Layer.Rings,Flat(to),Vector3.zero,1.3f*m,.25f,col);
    for(int i=N(6);i>0;i--)Emit(Layer.Sparks,to+Vector3.up*.7f,(Sphere()+Vector3.up*.5f)*R(2,4),R(.04f,.07f),R(.15f,.25f),col);return true;
   case EffectRoutine.Splash:if(!Allow(priority,N(4)))return false;
    Emit(Layer.Splash,Flat(to),Vector3.zero,R(.45f,.7f)*m,R(.25f,.35f),new Color(.62f,.66f,.78f,.55f));
    for(int i=N(3);i>0;i--)Emit(Layer.Sparks,Flat(to)+Vector3.up*.05f,(Random2(1)+Vector3.up*2).normalized*R(1.2f,2.2f),R(.025f,.04f),R(.2f,.3f),new Color(.6f,.68f,.8f,.7f));return true;
  }
  return false;}
 bool Explosion(Vector3 at,Color col,float m,int priority){if(!Allow(priority,N(40*m)))return false;var hot=Color.Lerp(col,new Color(1,.62f,.3f),.55f);
  Emit(Layer.Flares,at+Vector3.up*.3f,Vector3.zero,2.2f*m,.15f,Color.Lerp(hot,Color.white,.4f));Emit(Layer.Glow,at+Vector3.up*.3f,Vector3.zero,1.8f*m,.3f,hot);Emit(Layer.Glow,at+Vector3.up*.5f,Vector3.zero,1.1f*m,.45f,col);
  Emit(Layer.Rings,Flat(at),Vector3.zero,3.4f*m,.4f,hot);Emit(Layer.Rings,Flat(at),Vector3.zero,1.6f*m,.22f,Color.Lerp(hot,Color.white,.5f));Pillar(at,hot,m);
  for(int i=N(6+4*m);i>0;i--)Emit(Layer.Flame,at+Sphere()*.3f*m+Vector3.up*.3f,Sphere()*R(1,2.5f)*m+Vector3.up*R(.5f,1.5f),R(.8f,1.4f)*m,R(.35f,.6f),hot,90);
  for(int i=N(18+10*m);i>0;i--)Emit(Layer.Sparks,at+Vector3.up*.3f,(Sphere()+Vector3.up*.6f).normalized*R(5,11)*Mathf.Sqrt(m),R(.05f,.1f),R(.4f,.8f),hot);
  for(int i=N(5+3*m);i>0;i--)Emit(Layer.Smoke,at+Sphere()*.4f*m+Vector3.up*.4f,Vector3.up*R(.6f,1.4f)+Sphere()*.6f,R(1.2f,2)*m,R(1.6f,2.6f),new Color(.12f,.11f,.13f,.72f),40);
  for(int i=N(6+4*m);i>0;i--)Emit(Layer.Shards,at+Vector3.up*.3f,(Sphere()+Vector3.up*1.2f).normalized*R(3,6),R(.12f,.25f),R(.8f,1.2f),new Color(.08f,.07f,.08f),600);
  return true;}
 /// <summary>A column of light rising from the ground: stacked glows drifting up.</summary>
 void Pillar(Vector3 at,Color col,float m){var foot=Flat(at);for(int i=0;i<3;i++)Emit(Layer.Glow,foot+Vector3.up*(.4f+i*.7f)*m,Vector3.up*R(1.5f,2.5f),(.9f-i*.2f)*m,R(.3f,.4f),col*(1-i*.25f));}
 /// <summary>Slow smoke and an ember from a burning wreck; call a few times a second.</summary>
 public void Smolder(Vector3 at,float m=1){if(!Allow(0,2))return;Emit(Layer.Smoke,at+Random2(.3f)+Vector3.up*.4f,Vector3.up*R(.5f,1)+Random2(.2f),R(.9f,1.4f)*m,R(1.6f,2.4f),new Color(.1f,.09f,.11f,.55f),30);if(random.NextDouble()<.5)Emit(Layer.Embers,at+Random2(.4f)+Vector3.up*.3f,Vector3.up*R(.6f,1.4f),R(.05f,.09f),R(.6f,1),new Color(1,.55f,.25f));}
 /// <summary>A glow puff trailing an engine or a floating aura mote.</summary>
 public void Trail(Vector3 at,Vector3 velocity,Color color,float size=.35f,float life=.25f){if(!Allow(0,1))return;Emit(Layer.Glow,at,velocity,size,life,color);}
 public void Mote(Vector3 at,float radius,Color color){if(!Allow(0,1))return;Emit(Layer.Embers,at+Random2(radius)+Vector3.up*R(0,.6f),Vector3.up*R(.3f,1),R(.06f,.11f),R(.6f,1),color);}
 /// <summary>Thin sparks rising along a line: vertical light at the edge of a boss hazard.</summary>
 public void Curtain(Vector3 a,Vector3 b,Color color,int count=2){if(!Allow(0,count))return;for(int i=0;i<count;i++)Emit(Layer.Sparks,Vector3.Lerp(a,b,R(0,1))+Vector3.up*.05f,Vector3.up*R(2.5f,4.5f),R(.05f,.08f),R(.25f,.45f),color);}
 /// <summary>A shimmer ring around a shielded body.</summary>
 public void Shell(Vector3 at,float radius,Color color){if(!Allow(0,1))return;Emit(Layer.Rings,Flat(at)+Vector3.up*R(.2f,1.2f),Vector3.zero,radius*2,.5f,color);}
 static Vector3 Flat(Vector3 p)=>new Vector3(p.x,.06f,p.z);
 Vector3 Random2(float r){float a=R(0,Mathf.PI*2),d=Mathf.Sqrt(R(0,1))*r;return new Vector3(Mathf.Cos(a)*d,0,Mathf.Sin(a)*d);}
}
}
