using UnityEngine;
namespace Veilbreak.Cityscape {
/// <summary>Flickers a fire's point light around its authored intensity (set the light, then tune `flicker` here). Low
/// frequency only, so it never strobes; the host sets <see cref="Still"/> for Reduced Motion, which holds every fire steady.</summary>
[RequireComponent(typeof(Light)),DisallowMultipleComponent]
public sealed class FireLight:MonoBehaviour {
 public static bool Still;
 [Range(0,1)][SerializeField] float flicker=.3f;
 [SerializeField] float speed=5.5f;
 Light fire;float baseIntensity,seed;
 void Awake(){fire=GetComponent<Light>();baseIntensity=fire.intensity;seed=(transform.position.x*13.1f+transform.position.z*7.7f)%100;}
 void OnDisable(){if(fire!=null)fire.intensity=baseIntensity;}
 void Update(){
  if(Still){fire.intensity=baseIntensity;return;}
  var t=Time.time*speed;
  fire.intensity=baseIntensity*(1-flicker*.5f+flicker*Mathf.PerlinNoise(t,seed)-flicker*.25f*Mathf.PerlinNoise(t*3.1f,seed+3));
 }
}
}
