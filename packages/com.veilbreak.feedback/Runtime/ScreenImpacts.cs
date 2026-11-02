using UnityEngine;
namespace Veilbreak.Feedback {
/// <summary>Whole-screen hits: a shockwave ring that refracts the frame outward from where a blow landed, a radial zoom punch and
/// chromatic split around it, and a brief vignette in the hit's colour. Hosts push confirmed combat events (in viewport space) and advance
/// the decay each frame; <c>CinematicPulseFeature</c> renders the strongest few in its pass. Reduced Motion drops the refraction, punch
/// and split; Reduced Flashes drops the vignette and brightening. Presentation only, never read by the simulation.</summary>
public static class ScreenImpacts {
 public const int Capacity=4;
 /// <summary>Seconds an impact lasts at strength 1 (stronger ones last a little longer).</summary>
 public const float Seconds=.55f;
 struct Impact {public Vector2 at;public Color color;public float strength,age,life;}
 static readonly Impact[] impacts=new Impact[Capacity];
 static readonly Vector4[] centres=new Vector4[Capacity],colors=new Vector4[Capacity];
 /// <summary>Scales refraction, punch and chromatic split (0 under Reduced Motion).</summary>
 public static float Motion=1;
 /// <summary>Scales the vignette and brightening (0 under Reduced Flashes).</summary>
 public static float Flash=1;
 /// <summary>Overall gain (a setting or tier can soften every impact).</summary>
 public static float Gain=1;
 /// <summary>True while any impact is still playing.</summary>
 public static bool Active{get{for(int i=0;i<Capacity;i++)if(impacts[i].strength>0&&impacts[i].age<impacts[i].life)return true;return false;}}
 /// <summary>Starts an impact at <paramref name="viewport"/> (0–1, origin bottom-left) of <paramref name="strength"/> (about 0.2 for a heavy
 /// hit, 1 for a boss slam). It takes a free slot, else replaces the weakest remaining one when it is stronger.</summary>
 public static void Push(Vector2 viewport,Color color,float strength){if(!(strength>0))return;strength=Mathf.Min(1.5f,strength*Gain);int slot=-1;float weakest=float.MaxValue;
  for(int i=0;i<Capacity;i++){float left=impacts[i].age<impacts[i].life?impacts[i].strength*(1-impacts[i].age/impacts[i].life):0;if(left<weakest){weakest=left;slot=i;}}
  if(slot<0||weakest>=strength)return;impacts[slot]=new Impact{at=viewport,color=color,strength=strength,age=0,life=Seconds*(.8f+.4f*Mathf.Min(1,strength))};}
 /// <summary>Ages every impact by <paramref name="seconds"/> (real time: the screen answers at the same speed whatever the fight does).</summary>
 public static void Advance(float seconds){for(int i=0;i<Capacity;i++)if(impacts[i].strength>0){impacts[i].age+=seconds;if(impacts[i].age>=impacts[i].life)impacts[i].strength=0;}}
 public static void Clear(){for(int i=0;i<Capacity;i++)impacts[i]=default;}
 /// <summary>Shader data: per impact (viewport xy, progress 0–1, strength) and (colour rgb, flash weight); zero strength is inactive.
 /// The motion weight is folded into each strength.</summary>
 public static void Fill(out Vector4[] centre,out Vector4[] color){
  for(int i=0;i<Capacity;i++){var k=impacts[i];bool on=k.strength>0&&k.age<k.life;float p=on?k.age/k.life:1;
   centres[i]=on?new Vector4(k.at.x,k.at.y,p,k.strength*Motion):Vector4.zero;colors[i]=on?new Vector4(k.color.r,k.color.g,k.color.b,k.strength*Flash):Vector4.zero;}
  centre=centres;color=colors;}
}
}
