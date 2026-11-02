using System;
using System.Collections.Generic;
using UnityEngine;
namespace Veilbreak.Cityscape {
/// <summary>A fixed pool of real point lights handed to the emitters nearest the action (Standard 8, Low 2). Every other
/// emitter still shows its emission and ground light pool. A light follows its emitter by key as the street scrolls,
/// fades in and out over a quarter second and only changes emitter once dark, so reassignment never pops; fires
/// flicker.</summary>
public sealed class LightBudget:MonoBehaviour {
 Light[] pool=new Light[0];int[] keys=new int[0];float[] levels=new float[0];bool[] wanted=new bool[0];
 readonly List<(int key,Vector3 at,Emitter e)> candidates=new List<(int,Vector3,Emitter)>();readonly List<int> desired=new List<int>();
 Vector3 focus;Comparison<(int key,Vector3 at,Emitter e)> byScore;
 public int Capacity=>pool.Length;
 public int Lit {get{int n=0;foreach(var l in pool)if(l.enabled)n++;return n;}}
 public void Initialize(int capacity){foreach(var l in pool)if(l!=null){if(Application.isPlaying)Destroy(l.gameObject);else DestroyImmediate(l.gameObject);}pool=new Light[capacity];keys=new int[capacity];levels=new float[capacity];wanted=new bool[capacity];
  for(int i=0;i<capacity;i++){var go=new GameObject("Budget light "+i);go.transform.SetParent(transform,false);var l=go.AddComponent<Light>();l.type=LightType.Point;l.shadows=LightShadows.None;l.renderMode=LightRenderMode.ForcePixel;l.enabled=false;pool[i]=l;keys[i]=-1;}}

 /// <summary>Gives the pool to the emitters nearest `focus` (fires preferred, signs last) and updates each light.</summary>
 public void Present(StreetStreamer street,Vector3 focus,float lampLevel,float time,float delta){
  if(pool.Length==0||street==null)return;candidates.Clear();street.Emitters(candidates);
  this.focus=focus;byScore??=(a,b)=>Score(a,this.focus).CompareTo(Score(b,this.focus));candidates.Sort(byScore);desired.Clear();
  foreach(var c in candidates){if(desired.Count==pool.Length)break;if(c.e.intensity>0)desired.Add(c.key);}
  for(int p=0;p<pool.Length;p++){wanted[p]=keys[p]>=0&&desired.Remove(keys[p]);}
  foreach(var key in desired)for(int p=0;p<pool.Length;p++)if(!wanted[p]&&levels[p]<=0){keys[p]=key;wanted[p]=true;break;}
  for(int p=0;p<pool.Length;p++){
   var l=pool[p];int index=keys[p]<0?-1:Find(keys[p]);if(index<0)wanted[p]=false;
   levels[p]=Mathf.MoveTowards(levels[p],wanted[p]?1:0,delta*4);
   if(index<0||levels[p]<=0){l.enabled=false;if(!wanted[p])keys[p]=-1;continue;}
   var (_,at,e)=candidates[index];float flicker=e.kind==EmitterKind.Fire?.7f+.6f*Mathf.PerlinNoise(time*7,p*3.1f):1;
   l.transform.position=at;l.color=e.color;l.range=e.range;l.intensity=e.intensity*flicker*(e.kind==EmitterKind.Lamp?lampLevel:1)*levels[p];l.enabled=true;
  }
 }
 int Find(int key){for(int i=0;i<candidates.Count;i++)if(candidates[i].key==key)return i;return -1;}
 static float Score((int key,Vector3 at,Emitter e) c,Vector3 focus)=>Vector3.Distance(c.at,focus)-(c.e.kind==EmitterKind.Fire?6:c.e.kind==EmitterKind.Sign?-4:0);
}
}
