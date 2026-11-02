using UnityEngine;
namespace Veilbreak.Cityscape {
/// <summary>One look of the street (the plan's BiomeProfile): sky, air, light, wetness, how lit and how ruined the blocks
/// are, and the atmospheric particles. EnvironmentDirector blends every value between themes; StreetModules reads the
/// dressing values once per theme when it builds that theme's modules.</summary>
[CreateAssetMenu(menuName="Veilbreak/Environment/Street theme")]
public sealed class StreetTheme:ScriptableObject {
 [Header("Sky and air")]
 [Tooltip("Multiplies the painted sky plate (seen in puddles and the menu).")] public Color skyTint=Color.white;
 public Color horizon=new Color(.16f,.11f,.21f);
 public Color fog=new Color(.16f,.11f,.21f);
 [Range(0,.05f)] public float fogDensity=.012f;
 [Tooltip("Trilight ambient: sky, equator, ground.")] public Color ambientSky=new Color(.21f,.16f,.3f),ambientEquator=new Color(.16f,.11f,.16f),ambientGround=new Color(.05f,.04f,.06f);
 public Color moon=new Color(.62f,.6f,.85f);[Range(0,2)] public float moonIntensity=.45f;public Vector2 moonAngles=new Vector2(32,196);
 [Header("Street")]
 [Range(0,1)] public float wetness=.75f;
 [Range(0,1)] public float rain=.35f;
 [Range(0,2)] public float skyReflection=.9f;
 [Range(0,3)] public float mirrorReflection=.7f;
 [Header("Light")]
 public Color lamp=new Color(1,.62f,.34f);
 [Range(0,12)] public float lampIntensity=5;[Range(2,20)] public float lampRange=11;
 [Range(0,3)] public float lampBrightness=1;
 [Range(0,3)] public float windowBrightness=1;
 public Color fire=new Color(1,.42f,.12f);
 [Header("Windows")]
 [Range(0,1)] public float windowsLit=.3f;
 [Range(0,1)] public float windowsFire=.02f;
 public Color windowWarm=new Color(1,.58f,.28f),windowDim=new Color(1,.62f,.36f),windowViolet=new Color(.66f,.42f,1),windowFire=new Color(1,.32f,.06f);
 [Header("Dressing")]
 [Range(0,1)] public float damage=.3f;
 [Range(0,1)] public float wrecks=.25f;
 [Range(0,1)] public float fires=.3f;
 [Range(0,1)] public float trees=.6f;
 [Range(0,1)] public float gaps=.15f;
 [Range(1,10)] public int minFloors=3;[Range(1,10)] public int maxFloors=7;
 [Header("Particles")]
 public Color ember=new Color(1,.45f,.12f);[Range(0,1)] public float embers=.4f;
 public Color fogSheet=new Color(.3f,.22f,.36f);[Range(0,1)] public float fogSheetDensity=.1f;
}
}
