using UnityEngine;
using UnityEngine.Rendering;
namespace Veilbreak.Cityscape {
/// <summary>Owns the street for one scene: the streamer, the light budget, the air and the sky dome. Each frame the
/// consumer passes the road distance, the theme it wants, presentation time and the action focus; the director blends
/// the look toward that theme over <see cref="BlendSeconds"/> and publishes it: the city shader globals, trilight
/// ambient, fog, the moon and the camera background. `high` (Standard tier) turns on the mirrored reflection path,
/// rain ripples and the larger light, particle and fire budgets.</summary>
public sealed class EnvironmentDirector:MonoBehaviour {
 public const float BlendSeconds=8;public const int HighLights=8,LowLights=2;
 /// <summary>Every blended value of a <see cref="StreetTheme"/>.</summary>
 public struct Look {
  public Color skyTint,horizon,fog,ambientSky,ambientEquator,ambientGround,moon,lamp,ember,fogSheet;public Vector2 moonAngles;
  public float fogDensity,moonIntensity,wetness,rain,skyReflection,mirrorReflection,lampBrightness,windowBrightness,embers,fogSheetDensity;
  public static Look From(StreetTheme t)=>new Look{skyTint=t.skyTint,horizon=t.horizon,fog=t.fog,ambientSky=t.ambientSky,ambientEquator=t.ambientEquator,ambientGround=t.ambientGround,moon=t.moon,lamp=t.lamp,ember=t.ember,fogSheet=t.fogSheet,moonAngles=t.moonAngles,
   fogDensity=t.fogDensity,moonIntensity=t.moonIntensity,wetness=t.wetness,rain=t.rain,skyReflection=t.skyReflection,mirrorReflection=t.mirrorReflection,lampBrightness=t.lampBrightness,windowBrightness=t.windowBrightness,embers=t.embers,fogSheetDensity=t.fogSheetDensity};
  public static Look Lerp(in Look a,in Look b,float k)=>new Look{skyTint=Color.Lerp(a.skyTint,b.skyTint,k),horizon=Color.Lerp(a.horizon,b.horizon,k),fog=Color.Lerp(a.fog,b.fog,k),ambientSky=Color.Lerp(a.ambientSky,b.ambientSky,k),ambientEquator=Color.Lerp(a.ambientEquator,b.ambientEquator,k),ambientGround=Color.Lerp(a.ambientGround,b.ambientGround,k),
   moon=Color.Lerp(a.moon,b.moon,k),lamp=Color.Lerp(a.lamp,b.lamp,k),ember=Color.Lerp(a.ember,b.ember,k),fogSheet=Color.Lerp(a.fogSheet,b.fogSheet,k),moonAngles=Vector2.Lerp(a.moonAngles,b.moonAngles,k),
   fogDensity=Mathf.Lerp(a.fogDensity,b.fogDensity,k),moonIntensity=Mathf.Lerp(a.moonIntensity,b.moonIntensity,k),wetness=Mathf.Lerp(a.wetness,b.wetness,k),rain=Mathf.Lerp(a.rain,b.rain,k),skyReflection=Mathf.Lerp(a.skyReflection,b.skyReflection,k),mirrorReflection=Mathf.Lerp(a.mirrorReflection,b.mirrorReflection,k),
   lampBrightness=Mathf.Lerp(a.lampBrightness,b.lampBrightness,k),windowBrightness=Mathf.Lerp(a.windowBrightness,b.windowBrightness,k),embers=Mathf.Lerp(a.embers,b.embers,k),fogSheetDensity=Mathf.Lerp(a.fogSheetDensity,b.fogSheetDensity,k)};
 }
 static readonly int Params=Shader.PropertyToID("_VeilParams"),LightId=Shader.PropertyToID("_VeilLight"),SkyTint=Shader.PropertyToID("_VeilSkyTint"),Horizon=Shader.PropertyToID("_VeilHorizon"),
  Noise=Shader.PropertyToID("_VeilNoise"),SkyTex=Shader.PropertyToID("_VeilSkyTex"),ReflectionTex=Shader.PropertyToID("_VeilReflectionTex");
 static GlobalKeyword High;static bool keyword; // created on first use: Unity forbids it in static initialisers

 public StreetStreamer Street {get;private set;}public LightBudget Lights {get;private set;}public Atmospherics Air {get;private set;}
 public Look Current {get;private set;}public int Theme {get;private set;}public bool Blending=>blend<1;
 CityKit kit;StreetTheme[] themes;Camera view;Light moon;bool high;Look from;float blend=1;Transform dome;
 AudioSource rainBed,humBed;
 /// <summary>Ambience level (the consumer's effects level; master belongs on the AudioListener): the rain bed follows the theme's rain, the hum stays low,
 /// and wreck fires crackle positionally. 0 silences the street.</summary>
 public float Volume=1;

 public void Initialize(CityKit kit,StreetTheme[] themes,uint seed,Camera view,bool high,int theme=0,SegmentStreamer.SeedFunction segmentSeed=null){
  if(!keyword){High=GlobalKeyword.Create("_VEIL_HIGH");keyword=true;}
  this.kit=kit;this.themes=themes;this.view=view;this.high=high;Theme=Mathf.Clamp(theme,0,themes.Length-1);Current=Look.From(themes[Theme]);blend=1;
  Street=Child<StreetStreamer>("Street");Street.Initialize(kit,themes,seed,8,high,segmentSeed);
  Lights=Child<LightBudget>("Light budget");Lights.Initialize(high?HighLights:LowLights);
  Air=Child<Atmospherics>("Atmospherics");Air.Initialize(kit,high);
  if(kit.dome!=null&&kit.sky!=null){dome=new GameObject("Sky dome").transform;dome.SetParent(transform,false);dome.localScale=Vector3.one*400;dome.gameObject.AddComponent<MeshFilter>().sharedMesh=kit.dome;var r=dome.gameObject.AddComponent<MeshRenderer>();r.sharedMaterial=kit.sky;r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;}
  rainBed=Bed("Rain bed",kit.rainLoop);humBed=Bed("City hum",kit.humLoop);
  moon=FindMoon();Publish(0,0);
 }
 AudioSource Bed(string name,AudioClip clip){if(clip==null)return null;var source=Child<AudioSource>(name);source.clip=clip;source.loop=true;source.playOnAwake=false;source.spatialBlend=0;source.volume=0;source.Play();return source;}
 T Child<T>(string name) where T:Component{var go=new GameObject(name);go.transform.SetParent(transform,false);return go.AddComponent<T>();}
 static Light FindMoon(){foreach(var l in FindObjectsByType<Light>(FindObjectsSortMode.None))if(l.type==LightType.Directional)return l;return null;}

 /// <summary>Moves the street to `distance` metres, blends toward `theme`, and steps lights and air by `delta`.</summary>
 public void Present(double distance,int theme,float time,float delta,Vector3 focus,float groundSpeed,bool reducedMotion){
  theme=Mathf.Clamp(theme,0,themes.Length-1);if(theme!=Theme){from=Current;Theme=theme;blend=0;}
  if(blend<1){blend=Mathf.Min(1,blend+Mathf.Max(0,delta)/BlendSeconds);Current=Look.Lerp(from,Look.From(themes[Theme]),Mathf.SmoothStep(0,1,blend));}else Current=Look.From(themes[Theme]);
  Street.Present(distance,theme);Publish(distance,time);
  var look=Current;Lights.Present(Street,focus,look.lampBrightness,time,Mathf.Max(0,delta));Air.Present(Street,look,focus,groundSpeed,Mathf.Max(0,delta),reducedMotion);
  if(dome!=null&&view!=null)dome.position=view.transform.position;
  float level=Mathf.Clamp01(Volume);if(rainBed!=null)rainBed.volume=level*Mathf.Lerp(.15f,.55f,look.rain);if(humBed!=null)humBed.volume=level*.3f;Air.Volume=level;
 }
 /// <summary>Finishes a theme blend at once (captures and tests).</summary>
 public void Settle(){blend=1;Current=Look.From(themes[Theme]);}
 void Publish(double distance,float time){
  var l=Current;
  Shader.SetGlobalVector(Params,new Vector4((float)(distance%1024),time,l.wetness,l.rain));
  Shader.SetGlobalVector(LightId,new Vector4(l.windowBrightness,l.lampBrightness,l.skyReflection,high?l.mirrorReflection:0));
  Shader.SetGlobalColor(SkyTint,l.skyTint);Shader.SetGlobalColor(Horizon,l.horizon);
  if(kit.noise!=null)Shader.SetGlobalTexture(Noise,kit.noise);if(kit.skyPlate!=null)Shader.SetGlobalTexture(SkyTex,kit.skyPlate);
  if(!high)Shader.SetGlobalTexture(ReflectionTex,Texture2D.blackTexture);Shader.SetKeyword(High,high);
  RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=l.ambientSky;RenderSettings.ambientEquatorColor=l.ambientEquator;RenderSettings.ambientGroundColor=l.ambientGround;
  RenderSettings.fog=true;RenderSettings.fogMode=FogMode.Exponential;RenderSettings.fogColor=l.fog;RenderSettings.fogDensity=l.fogDensity;
  if(view!=null){view.clearFlags=CameraClearFlags.SolidColor;view.backgroundColor=l.horizon;}
  if(moon!=null){moon.color=l.moon;moon.intensity=l.moonIntensity;moon.transform.rotation=Quaternion.Euler(l.moonAngles.x,l.moonAngles.y,0);moon.shadows=high?LightShadows.Soft:LightShadows.Hard;}
 }
 void OnDestroy(){if(keyword)Shader.SetKeyword(High,false);}
}
}
