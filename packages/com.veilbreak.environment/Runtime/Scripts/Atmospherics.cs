using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace Veilbreak.Cityscape {
/// <summary>The air over the street: two low drifting fog sheets, an ember field, rain, and a small pool of fire rigs
/// (flame, smoke, sparks) parked on the wreck fires nearest the action. Counts are fixed per tier at creation; particles
/// advance by the presentation delta the director passes (pausing with the run) and drift with the road.</summary>
public sealed class Atmospherics:MonoBehaviour {
 public const int HighEmbers=400,LowEmbers=120,HighRain=500,LowRain=150,HighFires=4,LowFires=2;
 ParticleSystem embers,rain;Material fogMaterial,emberMaterial,flameMaterial,rainMaterial,smokeMaterial;readonly List<Transform> sheets=new List<Transform>();
 sealed class Fire { public Transform rig;public ParticleSystem[] systems;public AudioSource voice;public int key=-1; }
 Fire[] fires=new Fire[0];readonly List<(int key,Vector3 at,Emitter e)> candidates=new List<(int,Vector3,Emitter)>();
 static readonly int ColorId=Shader.PropertyToID("_Color"),DensityId=Shader.PropertyToID("_Density"),BaseColorId=Shader.PropertyToID("_BaseColor");
 public int Particles=>(embers!=null?embers.particleCount:0)+(rain!=null?rain.particleCount:0);
 public int FireCapacity=>fires.Length;
 /// <summary>Level of the fires' crackle (the consumer's effects level; master belongs on the AudioListener); 0 silences them.</summary>
 public float Volume=1;AudioClip fireLoop;

 public void Initialize(CityKit kit,bool high){
  fogMaterial=new Material(kit.fog){name="Fog sheets"};emberMaterial=new Material(kit.ember){name="Embers"};flameMaterial=new Material(kit.ember){name="Flames"};rainMaterial=new Material(kit.rain){name="Rain"};smokeMaterial=new Material(kit.smoke){name="Smoke"};
  emberMaterial.SetFloat("_Lamp",0);flameMaterial.SetFloat("_Lamp",0);fireLoop=kit.fireLoop;
  float[] heights={.6f,2.4f};foreach(var h in heights){var go=new GameObject("Fog sheet");go.transform.SetParent(transform,false);go.transform.localPosition=new Vector3(0,h,14);go.transform.localScale=new Vector3(44,1,64);
   go.AddComponent<MeshFilter>().sharedMesh=kit.quad;var r=go.AddComponent<MeshRenderer>();r.sharedMaterial=fogMaterial;r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;sheets.Add(go.transform);}
  embers=System("Embers",emberMaterial,high?HighEmbers:LowEmbers,ParticleSystemRenderMode.Billboard);
  { var m=embers.main;m.startLifetime=new ParticleSystem.MinMaxCurve(4,7);m.startSpeed=new ParticleSystem.MinMaxCurve(.1f,.4f);m.startSize=new ParticleSystem.MinMaxCurve(.03f,.09f);
   var s=embers.shape;s.shapeType=ParticleSystemShapeType.Box;s.position=new Vector3(0,2.5f,16);s.scale=new Vector3(30,5,52);
   var n=embers.noise;n.enabled=true;n.strength=.6f;n.frequency=.35f;n.scrollSpeed=.2f;
   var c=embers.colorOverLifetime;c.enabled=true;c.color=new ParticleSystem.MinMaxGradient(Fade(new Color(1,.85f,.6f),new Color(1,.35f,.1f)));}
  rain=System("Rain",rainMaterial,high?HighRain:LowRain,ParticleSystemRenderMode.Stretch);
  { var m=rain.main;m.startLifetime=new ParticleSystem.MinMaxCurve(.7f,.9f);m.startSpeed=0;m.startSize=new ParticleSystem.MinMaxCurve(.012f,.022f);
   var s=rain.shape;s.shapeType=ParticleSystemShapeType.Box;s.position=new Vector3(0,15,14);s.scale=new Vector3(26,.1f,46);
   var r=rain.GetComponent<ParticleSystemRenderer>();r.lengthScale=1;r.velocityScale=.045f;}
  fires=new Fire[high?HighFires:LowFires];for(int i=0;i<fires.Length;i++)fires[i]=Rig(i);
 }
 static Gradient Fade(Color a,Color b){var g=new Gradient();g.SetKeys(new[]{new GradientColorKey(a,0),new GradientColorKey(b,1)},new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(1,.15f),new GradientAlphaKey(.8f,.6f),new GradientAlphaKey(0,1)});return g;}
 ParticleSystem System(string name,Material material,int max,ParticleSystemRenderMode mode,Transform parent=null,bool world=true){
  var go=new GameObject(name);go.transform.SetParent(parent??transform,false);var ps=go.AddComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
  var m=ps.main;m.loop=true;m.playOnAwake=false;m.maxParticles=max;m.simulationSpace=world?ParticleSystemSimulationSpace.World:ParticleSystemSimulationSpace.Local;m.scalingMode=ParticleSystemScalingMode.Hierarchy;
  var r=go.GetComponent<ParticleSystemRenderer>();r.sharedMaterial=material;r.renderMode=mode;r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;
  var e=ps.emission;e.rateOverTime=0;ps.Play();ps.Pause();return ps;
 }
 Fire Rig(int index){
  var rig=new GameObject("Fire "+index).transform;rig.SetParent(transform,false);rig.gameObject.SetActive(false);
  var flame=System("Flame",flameMaterial,60,ParticleSystemRenderMode.Billboard,rig,false);
  { var m=flame.main;m.startLifetime=new ParticleSystem.MinMaxCurve(.5f,.9f);m.startSpeed=new ParticleSystem.MinMaxCurve(.6f,1.4f);m.startSize=new ParticleSystem.MinMaxCurve(.5f,1.1f);m.startRotation=new ParticleSystem.MinMaxCurve(0,6.28f);
   var s=flame.shape;s.shapeType=ParticleSystemShapeType.Cone;s.angle=12;s.radius=.35f;s.rotation=new Vector3(-90,0,0);var e=flame.emission;e.rateOverTime=55;
   var size=flame.sizeOverLifetime;size.enabled=true;size.size=new ParticleSystem.MinMaxCurve(1,AnimationCurve.Linear(0,1,1,.2f));
   var c=flame.colorOverLifetime;c.enabled=true;var g=new Gradient();g.SetKeys(new[]{new GradientColorKey(new Color(1,.9f,.6f),0),new GradientColorKey(new Color(1,.45f,.12f),.35f),new GradientColorKey(new Color(.7f,.12f,.1f),.8f)},new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(1,.12f),new GradientAlphaKey(.7f,.6f),new GradientAlphaKey(0,1)});c.color=g;}
  var smoke=System("Smoke",smokeMaterial,40,ParticleSystemRenderMode.Billboard,rig,false);
  { var m=smoke.main;m.startLifetime=new ParticleSystem.MinMaxCurve(2.5f,4);m.startSpeed=new ParticleSystem.MinMaxCurve(.8f,1.4f);m.startSize=new ParticleSystem.MinMaxCurve(.8f,1.6f);m.startRotation=new ParticleSystem.MinMaxCurve(0,6.28f);
   var s=smoke.shape;s.shapeType=ParticleSystemShapeType.Cone;s.angle=18;s.radius=.4f;s.rotation=new Vector3(-90,0,0);s.position=new Vector3(0,.8f,0);var e=smoke.emission;e.rateOverTime=10;
   var size=smoke.sizeOverLifetime;size.enabled=true;size.size=new ParticleSystem.MinMaxCurve(1,AnimationCurve.Linear(0,.6f,1,2.4f));
   var c=smoke.colorOverLifetime;c.enabled=true;var g=new Gradient();g.SetKeys(new[]{new GradientColorKey(new Color(.32f,.27f,.3f),0),new GradientColorKey(new Color(.16f,.13f,.2f),1)},new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(.5f,.2f),new GradientAlphaKey(0,1)});c.color=g;}
  var sparks=System("Sparks",flameMaterial,30,ParticleSystemRenderMode.Stretch,rig,false);
  { var m=sparks.main;m.startLifetime=new ParticleSystem.MinMaxCurve(.6f,1.2f);m.startSpeed=new ParticleSystem.MinMaxCurve(1.5f,3);m.startSize=new ParticleSystem.MinMaxCurve(.02f,.04f);m.gravityModifier=-.05f;
   var s=sparks.shape;s.shapeType=ParticleSystemShapeType.Cone;s.angle=30;s.radius=.3f;s.rotation=new Vector3(-90,0,0);var e=sparks.emission;e.rateOverTime=14;
   var n=sparks.noise;n.enabled=true;n.strength=.8f;n.frequency=1.2f;var r=sparks.GetComponent<ParticleSystemRenderer>();r.velocityScale=.05f;r.lengthScale=1;}
  AudioSource voice=null;if(fireLoop!=null){voice=rig.gameObject.AddComponent<AudioSource>();voice.clip=fireLoop;voice.loop=true;voice.playOnAwake=false;voice.spatialBlend=1;voice.rolloffMode=AudioRolloffMode.Linear;voice.minDistance=8;voice.maxDistance=45;voice.dopplerLevel=0;voice.volume=0;}
  return new Fire{rig=rig,systems=new[]{flame,smoke,sparks},voice=voice};
 }

 /// <summary>Advances the air by `delta` seconds; `ground` is the road speed in m/s so particles drift with it.</summary>
 public void Present(StreetStreamer street,in EnvironmentDirector.Look look,Vector3 focus,float ground,float delta,bool reducedMotion){
  fogMaterial.SetColor(ColorId,look.fogSheet);fogMaterial.SetFloat(DensityId,look.fogSheetDensity);
  emberMaterial.SetColor(ColorId,look.ember*3);
  Emit(embers,look.embers*embers.main.maxParticles/5.5f,ground,.25f,delta);
  Emit(rain,look.rain*(reducedMotion?.4f:1)*rain.main.maxParticles/.8f,ground,-18,delta);
  candidates.Clear();street?.Emitters(candidates);
  foreach(var f in fires){int index=f.key<0?-1:Find(f.key);if(index<0){f.key=-1;f.rig.gameObject.SetActive(false);}}
  for(int i=0;i<candidates.Count;i++){var c=candidates[i];if(c.e.kind!=EmitterKind.Fire||Vector3.Distance(c.at,focus)>40)continue;bool taken=false;foreach(var f in fires)if(f.key==c.key)taken=true;if(taken)continue;
   foreach(var f in fires)if(f.key<0){f.key=c.key;f.rig.gameObject.SetActive(true);foreach(var ps in f.systems){ps.Clear();ps.Play();ps.Pause();}if(f.voice!=null){f.voice.timeSamples=(c.key*7919)%Mathf.Max(1,fireLoop.samples);f.voice.Play();}break;}}
  foreach(var f in fires){if(f.key<0)continue;f.rig.position=candidates[Find(f.key)].at;foreach(var ps in f.systems)ps.Simulate(delta,false,false,false);if(f.voice!=null)f.voice.volume=Volume*.7f;}
 }
 static void Emit(ParticleSystem ps,float rate,float ground,float rise,float delta){var e=ps.emission;e.rateOverTime=rate;var v=ps.velocityOverLifetime;v.enabled=true;v.space=ParticleSystemSimulationSpace.World;
  v.x=new ParticleSystem.MinMaxCurve(0);v.y=new ParticleSystem.MinMaxCurve(rise);v.z=new ParticleSystem.MinMaxCurve(-ground);ps.Simulate(delta,true,false,false);}
 int Find(int key){for(int i=0;i<candidates.Count;i++)if(candidates[i].key==key)return i;return -1;}
 void OnDestroy(){foreach(var m in new[]{fogMaterial,emberMaterial,flameMaterial,rainMaterial,smokeMaterial})if(m!=null){if(Application.isPlaying)Destroy(m);else DestroyImmediate(m);}}
}
}
