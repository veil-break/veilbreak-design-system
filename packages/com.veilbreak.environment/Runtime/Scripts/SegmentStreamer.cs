using System;
using UnityEngine;
namespace Veilbreak.Cityscape {
/// <summary>Bounded connector-aligned visual stream: <see cref="Capacity"/> segments of <see cref="Length"/> metres are recycled
/// around the presented distance. Content and themes are supplied by the consumer; nothing here advances progression or draws
/// from a simulation's random stream. The host supplies the deterministic per-segment seed (<see cref="SeedFunction"/>) and must
/// keep its own segment size equal to <see cref="Length"/>, which the street meshes are built for.</summary>
public sealed class SegmentStreamer:MonoBehaviour {
 public const int Capacity=9;public const float Length=StreetModules.Length;
 /// <summary>Deterministic seed of segment `index` under `theme` for a run `seed`; must not depend on visit order.</summary>
 public delegate uint SeedFunction(uint seed,int theme,long index);
 /// <summary>The default per-segment hash (an LCG step over the mixed run seed, theme and index).</summary>
 public static uint DefaultSeed(uint seed,int theme,long index){unchecked{return (seed^((uint)theme*0x9e3779b9u)^(uint)index*0x85ebca6bu)*1664525u+1013904223u;}}
 /// <summary>Local pose of `segment`, showing `index`, at the presented distance, for a road that turns; false hides it.</summary>
 public delegate bool Placement(GameObject segment,long index,double distance,out Vector3 position,out Quaternion rotation);
 /// <summary>Null lays every segment straight along +z.</summary>
 public Placement Place;
 readonly GameObject[] segments=new GameObject[Capacity];readonly long[] indices=new long[Capacity];
 Action<GameObject,uint,int> configure;uint seed;SeedFunction hash=DefaultSeed;
 public SeedFunction Hash=>hash;
 public void Initialize(uint seed,Action<GameObject,uint,int> configure,SeedFunction segmentSeed=null){this.seed=seed;this.configure=configure;hash=segmentSeed??DefaultSeed;for(int i=0;i<Capacity;i++){segments[i]=new GameObject("Segment "+i);segments[i].transform.SetParent(transform,false);indices[i]=long.MinValue;}}
 public void Present(double distance,int theme){if(configure==null)return;long first=(long)Math.Floor(distance/Length)-2;int nextBiome=Math.Max(0,theme);for(int i=0;i<Capacity;i++){long index=first+i;int slot=(int)((index%Capacity+Capacity)%Capacity);var segment=segments[slot];if(indices[slot]!=index){indices[slot]=index;configure(segment,hash(seed,nextBiome,index),nextBiome);}
   Vector3 position=new Vector3(0,0,(float)(index*Length-distance));var rotation=Quaternion.identity;bool shown=Place==null||Place(segment,index,distance,out position,out rotation);
   if(segment.activeSelf!=shown)segment.SetActive(shown);if(shown)segment.transform.SetLocalPositionAndRotation(position,rotation);}}
}
}
