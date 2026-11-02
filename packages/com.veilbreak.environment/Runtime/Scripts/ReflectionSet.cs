using System.Collections.Generic;
using UnityEngine;
namespace Veilbreak.Cityscape {
/// <summary>The renderer submeshes that give light (lamps, windows, signs, fires), drawn a second time by
/// <see cref="VeilReflectionFeature"/> through their material's VeilReflection pass into the mirrored wet-street texture.
/// Explicit registration instead of a renderer list keeps lights above the frame in the reflection: the camera's
/// culling would drop them.</summary>
public static class ReflectionSet {
 public struct Entry { public Renderer renderer;public Material material;public int submesh,pass; }
 static readonly List<Entry> entries=new List<Entry>();
 public static IReadOnlyList<Entry> Entries=>entries;
 public static void Set(Renderer renderer,Material[] materials,int[] submeshes){Remove(renderer);foreach(var s in submeshes){var m=materials[s];if(m==null)continue;int pass=m.FindPass("VeilReflection");if(pass>=0)entries.Add(new Entry{renderer=renderer,material=m,submesh=s,pass=pass});}}
 public static void Remove(Renderer renderer){for(int i=entries.Count-1;i>=0;i--)if(entries[i].renderer==renderer||entries[i].renderer==null)entries.RemoveAt(i);}
 public static void Clear()=>entries.Clear();
}
}
