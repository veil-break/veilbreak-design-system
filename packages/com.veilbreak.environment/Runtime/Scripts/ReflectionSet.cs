using System.Collections.Generic;
using UnityEngine;
namespace Veilbreak.Cityscape {
/// <summary>Environmental lights and solid actor submeshes, drawn by <see cref="VeilReflectionFeature"/>
/// through their material's VeilReflection pass into the mirrored wet-street texture.
/// Explicit registration instead of a renderer list keeps lights above the frame in the reflection: the camera's
/// culling would drop them.</summary>
public static class ReflectionSet {
 public struct Entry { public Renderer renderer;public Material material;public int submesh,pass;public bool solid; }
 static readonly List<Entry> entries=new List<Entry>();
 public static IReadOnlyList<Entry> Entries=>entries;
 /// <summary>Solid actors draw after the blurred lights, with depth, so their complete silhouettes remain readable in puddles.</summary>
 public static void Set(Renderer renderer,Material[] materials,int[] submeshes,bool solid=false){Remove(renderer);foreach(var s in submeshes){var m=materials[s];if(m==null)continue;int pass=m.FindPass("VeilReflection");if(pass>=0)entries.Add(new Entry{renderer=renderer,material=m,submesh=s,pass=pass,solid=solid});}}
 public static void Remove(Renderer renderer){for(int i=entries.Count-1;i>=0;i--)if(entries[i].renderer==renderer||entries[i].renderer==null)entries.RemoveAt(i);}
 public static void Clear()=>entries.Clear();
}
}
