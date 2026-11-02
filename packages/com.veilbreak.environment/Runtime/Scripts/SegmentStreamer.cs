using System;
using UnityEngine;
namespace Veilbreak.Cityscape {
/// <summary>Bounded connector-aligned visual stream: <see cref="Capacity"/> segments of <see cref="Length"/> metres are recycled
/// around the presented distance. Content and themes are supplied by the consumer; nothing here advances progression or draws
/// from a simulation's random stream. The host supplies the deterministic per-segment seed (<see cref="SeedFunction"/>) and must
/// keep its own segment size equal to <see cref="Length"/>, which the street meshes are built for.</summary>
public sealed class SegmentStreamer:MonoBehaviour {
 /// <summary>Segments kept behind and ahead of the segment under the presented distance; <see cref="Capacity"/> is Behind + 1 + Ahead. Hosts derive their
 /// lookahead from these (a boundary must be placed beyond <see cref="FurthestAllocated"/>, which reaches Ahead segments past the presented one).</summary>
 public const int Behind=2,Ahead=6;
 public const int Capacity=Behind+1+Ahead;public const float Length=StreetModules.Length;
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
 readonly SegmentDescriptor[] descriptors=new SegmentDescriptor[Capacity];readonly bool[] resolved=new bool[Capacity];
 /// <summary>Plan mode: the per-index plan. Null keeps the legacy global-theme path, <see cref="Present(double,int)"/>.</summary>
 public ISegmentPlan Plan {get;set;}
 /// <summary>Plan mode: dresses `segment` for a freshly resolved descriptor / returns what that dressing took before the slot is reused.</summary>
 public Action<GameObject,SegmentDescriptor> Apply,Retire;
 /// <summary>The highest segment index any slot has been allocated for since <see cref="Initialize"/> (long.MinValue before the first Present). Never decreases.</summary>
 public long FurthestAllocated {get;private set;}=long.MinValue;
 /// <summary>The descriptor resolved for `index` while its slot still holds it (plan mode).</summary>
 public bool TryDescriptor(long index,out SegmentDescriptor descriptor){int slot=(int)((index%Capacity+Capacity)%Capacity);if(resolved[slot]&&indices[slot]==index){descriptor=descriptors[slot];return true;}descriptor=default;return false;}
 public SeedFunction Hash=>hash;
 public void Initialize(uint seed,Action<GameObject,uint,int> configure,SeedFunction segmentSeed=null){this.seed=seed;this.configure=configure;hash=segmentSeed??DefaultSeed;for(int i=0;i<Capacity;i++){segments[i]=new GameObject("Segment "+i);segments[i].transform.SetParent(transform,false);indices[i]=long.MinValue;}}
 public void Present(double distance,int theme){if(configure==null)return;long first=(long)Math.Floor(distance/Length)-Behind;int nextBiome=Math.Max(0,theme);for(int i=0;i<Capacity;i++){long index=first+i;int slot=(int)((index%Capacity+Capacity)%Capacity);var segment=segments[slot];if(indices[slot]!=index){if(resolved[slot]){resolved[slot]=false;Retire?.Invoke(segment,descriptors[slot]);}indices[slot]=index;if(index>FurthestAllocated)FurthestAllocated=index;configure(segment,hash(seed,nextBiome,index),nextBiome);}
   Vector3 position=new Vector3(0,0,(float)(index*Length-distance));var rotation=Quaternion.identity;bool shown=Place==null||Place(segment,index,distance,out position,out rotation);
   if(segment.activeSelf!=shown)segment.SetActive(shown);if(shown)segment.transform.SetLocalPositionAndRotation(position,rotation);}}

 /// <summary>Plan mode: shows the segments around `distance`, resolving <see cref="Plan"/> only for a slot newly allocated to an index. An allocated slot is never
 /// re-resolved or re-dressed while it holds its index, however the distance moves. Does nothing without a plan and <see cref="Apply"/>.</summary>
 public void Present(double distance){if(Plan==null||Apply==null)return;long first=(long)Math.Floor(distance/Length)-Behind;
  for(int i=0;i<Capacity;i++){long index=first+i;int slot=(int)((index%Capacity+Capacity)%Capacity);var segment=segments[slot];
   if(indices[slot]!=index||!resolved[slot]){if(resolved[slot])Retire?.Invoke(segment,descriptors[slot]);indices[slot]=index;var d=Plan.Resolve(index);descriptors[slot]=d;resolved[slot]=true;if(index>FurthestAllocated)FurthestAllocated=index;Apply(segment,d);}
   Vector3 position=new Vector3(0,0,(float)(index*Length-distance));var rotation=Quaternion.identity;bool shown=Place==null||Place(segment,index,distance,out position,out rotation);
   if(segment.activeSelf!=shown)segment.SetActive(shown);if(shown)segment.transform.SetLocalPositionAndRotation(position,rotation);}}
}
}
