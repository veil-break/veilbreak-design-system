using UnityEngine;
namespace Veilbreak.Cityscape {
/// <summary>Flickers a fire's point light around its authored intensity (set the light, then tune `flicker` here). Low
/// frequency only, so it never strobes; the host sets <see cref="Still"/> for Reduced Motion, which holds every fire steady.
/// Flicker time is the host's: <see cref="Advance"/> adds the delta the host passes (EnvironmentDirector does it each Present), so a
/// paused game, which passes 0, holds every fire still. A host that never calls Advance keeps the earlier behaviour (Time.time).</summary>
[RequireComponent(typeof(Light)),DisallowMultipleComponent]
public sealed class FireLight:MonoBehaviour {
 public static bool Still;
 static float clock;static bool driven;
 /// <summary>The accumulated host time the fires flicker on, in seconds.</summary>
 public static float Clock=>clock;
 /// <summary>Adds the host's frame delta (negative values count as 0) to the flicker clock and switches every fire from Time.time to it.</summary>
 public static void Advance(float delta){driven=true;clock+=Mathf.Max(0,delta);}
 /// <summary>Back to the undriven state (tests and scene reloads).</summary>
 public static void ResetClock(){clock=0;driven=false;}
 [Range(0,1)][SerializeField] float flicker=.3f;
 [SerializeField] float speed=5.5f;
 Light fire;float baseIntensity,seed;bool ready;
 void Awake()=>Ready();
 void Ready(){if(ready)return;ready=true;fire=GetComponent<Light>();baseIntensity=fire.intensity;seed=(transform.position.x*13.1f+transform.position.z*7.7f)%100;}
 void OnDisable(){if(fire!=null)fire.intensity=baseIntensity;}
 void Update()=>Step();
 /// <summary>Sets the light for the current clock (Update calls it every frame).</summary>
 public void Step(){
  Ready();
  if(Still){fire.intensity=baseIntensity;return;}
  var t=(driven?clock:Time.time)*speed;
  fire.intensity=baseIntensity*(1-flicker*.5f+flicker*Mathf.PerlinNoise(t,seed)-flicker*.25f*Mathf.PerlinNoise(t*3.1f,seed+3));
 }
}
}
