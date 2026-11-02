using System;
using System.Collections.Generic;
using UnityEngine;
namespace Veilbreak.Cityscape {
/// <summary>What a segment is for: the plan's structural role. A <see cref="Transition"/> segment blends the look from the previous
/// segment's <see cref="SegmentDescriptor.Look"/> to its own over its length.</summary>
public enum SegmentRole:byte { Straight,Transition,Landmark,Junction }

/// <summary>Everything the street needs to dress one segment index. Produced by <see cref="ISegmentPlan.Resolve"/>, read-only afterwards.</summary>
public readonly struct SegmentDescriptor:IEquatable<SegmentDescriptor> {
 public readonly long Index;
 /// <summary>The segment's deterministic seed (the plan hashes run seed, index and descriptor ids); it picks the built-in side blocks.</summary>
 public readonly uint Seed;
 /// <summary>Street-kit family: 0 is the built-in boulevard, anything else a kit registered in <see cref="StreetKitRegistry"/>.</summary>
 public readonly ushort Family;
 /// <summary>Index into the director's <see cref="StreetTheme"/> table (the look).</summary>
 public readonly ushort Look;
 public readonly SegmentRole Role;
 /// <summary>0 is none; otherwise a landmark id the family's kit understands.</summary>
 public readonly ushort Landmark;
 public readonly byte Weather,Damage;
 /// <summary>Reserved.</summary>
 public readonly uint Flags;
 public SegmentDescriptor(long index,uint seed,ushort family,ushort look,SegmentRole role,ushort landmark=0,byte weather=0,byte damage=0,uint flags=0){Index=index;Seed=seed;Family=family;Look=look;Role=role;Landmark=landmark;Weather=weather;Damage=damage;Flags=flags;}
 public bool Equals(SegmentDescriptor o)=>Index==o.Index&&Seed==o.Seed&&Family==o.Family&&Look==o.Look&&Role==o.Role&&Landmark==o.Landmark&&Weather==o.Weather&&Damage==o.Damage&&Flags==o.Flags;
 public override bool Equals(object obj)=>obj is SegmentDescriptor o&&Equals(o);
 public override int GetHashCode()=>HashCode.Combine(Index,Seed,Family,Look,(byte)Role,Landmark,Weather,Damage);
}

/// <summary>A pure function from segment index to descriptor. The streamer calls it once when a slot is (re)allocated for an index and never
/// again for that slot, so it may be expensive, but the same index must always give the same descriptor, whatever was resolved before.</summary>
public interface ISegmentPlan { SegmentDescriptor Resolve(long index); }

/// <summary>A game-owned street family. The streamer calls <see cref="Build"/> when a slot is allocated for one of its segments and
/// <see cref="Release"/> when that slot is recycled; between the two the kit owns the slot's contents and the built-in street parts of that
/// slot are hidden.</summary>
public interface IStreetKit {
 ushort Family { get; }
 /// <summary>Worst-case triangles of one segment built by this kit on the tier.</summary>
 int TriangleBudget(bool high);
 void Build(in SegmentDescriptor d,Transform slot,bool high);
 /// <summary>Returns the pooled resources <see cref="Build"/> took.</summary>
 void Release(Transform slot);
}

/// <summary>Process-wide table of <see cref="IStreetKit"/>s by family. Family 0 is the built-in boulevard and cannot be registered.</summary>
public static class StreetKitRegistry {
 static readonly Dictionary<ushort,IStreetKit> kits=new Dictionary<ushort,IStreetKit>();
 public static int Count=>kits.Count;
 /// <summary>Registers `kit` under its family, replacing an earlier kit of that family.</summary>
 public static void Register(IStreetKit kit){if(kit==null)throw new ArgumentNullException(nameof(kit));if(kit.Family==0)throw new ArgumentException("Family 0 is the built-in boulevard",nameof(kit));kits[kit.Family]=kit;}
 public static bool Unregister(ushort family)=>kits.Remove(family);
 public static void Clear()=>kits.Clear();
 /// <summary>The kit of `family`, or null for the built-in boulevard and unregistered families.</summary>
 public static IStreetKit Find(ushort family)=>kits.TryGetValue(family,out var kit)?kit:null;
 /// <summary>The kit's triangle budget on the tier, or -1 when no kit is registered for `family`.</summary>
 public static int Budget(ushort family,bool high)=>kits.TryGetValue(family,out var kit)?kit.TriangleBudget(high):-1;
}
}
