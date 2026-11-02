using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace Veilbreak.Cityscape {
/// <summary>Appends kit meshes, boxes and ground quads into one mesh with a submesh per material key, so a street
/// module draws through one renderer. Vertex colours pass through a tint callback (windows get their light there).</summary>
public sealed class MeshBuilder {
 sealed class Source { public Vector3[] v,n; public Vector2[] uv; public Color32[] c; public int[][] tris; }
 static readonly Dictionary<Mesh,Source> sources=new Dictionary<Mesh,Source>();
 readonly List<Vector3> v=new List<Vector3>(),n=new List<Vector3>();readonly List<Vector2> uv=new List<Vector2>();readonly List<Color32> c=new List<Color32>();
 readonly SortedDictionary<int,List<int>> sub=new SortedDictionary<int,List<int>>();
 public int Triangles {get;private set;}
 static Source Read(Mesh mesh){if(sources.TryGetValue(mesh,out var s))return s;s=new Source{v=mesh.vertices,n=mesh.normals,uv=mesh.uv,c=mesh.colors32,tris=new int[mesh.subMeshCount][]};for(int i=0;i<mesh.subMeshCount;i++)s.tris[i]=mesh.GetTriangles(i);return sources[mesh]=s;}
 List<int> Indices(int key){if(!sub.TryGetValue(key,out var list))sub[key]=list=new List<int>();return list;}

 /// <summary>Adds `mesh` with its submeshes keyed by `slots`, transformed by `m`. `tint` maps each source vertex colour.</summary>
 public void Add(Mesh mesh,int[] slots,Matrix4x4 m,Func<Color32,Color32> tint=null){
  var s=Read(mesh);int b=v.Count;var nm=m.inverse.transpose;bool flip=m.determinant<0;
  for(int i=0;i<s.v.Length;i++){v.Add(m.MultiplyPoint3x4(s.v[i]));n.Add(nm.MultiplyVector(s.n.Length>0?s.n[i]:Vector3.up).normalized);uv.Add(s.uv.Length>0?s.uv[i]:Vector2.zero);var col=s.c.Length>0?s.c[i]:default;c.Add(tint!=null?tint(col):new Color32(0,0,0,0));}
  for(int k=0;k<s.tris.Length;k++){var list=Indices(slots[k]);var t=s.tris[k];for(int i=0;i<t.Length;i+=3){list.Add(b+t[i]);if(flip){list.Add(b+t[i+2]);list.Add(b+t[i+1]);}else{list.Add(b+t[i+1]);list.Add(b+t[i+2]);}}Triangles+=t.Length/3;}
 }
 /// <summary>An axis-aligned box (before `yaw`), every face lit by its own normal. `colour` is the vertex colour (the City shader glows by its alpha).</summary>
 public void Box(int key,Vector3 centre,Vector3 size,float yaw=0,Color32 colour=default){
  var q=Quaternion.Euler(0,yaw,0);var h=size*.5f;
  for(int f=0;f<6;f++){var axis=f/2;var sign=f%2==0?1f:-1f;var normal=Vector3.zero;normal[axis]=sign;var u=Vector3.zero;u[(axis+1)%3]=1;var w=Vector3.Cross(normal,u);
   var o=Vector3.Scale(normal,h);var du=Vector3.Scale(u,h);var dw=Vector3.Scale(w,h);Face(key,centre+q*(o-du-dw),centre+q*(o+du-dw),centre+q*(o+du+dw),centre+q*(o-du+dw),q*normal,colour);}
 }
 /// <summary>A flat quad facing up at `centre`, for ground light pools.</summary>
 public void Ground(int key,Vector3 centre,Vector2 size,Color32 colour){var a=v.Count;var h=new Vector3(size.x*.5f,0,size.y*.5f);
  v.Add(centre+new Vector3(-h.x,0,-h.z));v.Add(centre+new Vector3(-h.x,0,h.z));v.Add(centre+new Vector3(h.x,0,h.z));v.Add(centre+new Vector3(h.x,0,-h.z));
  uv.Add(new Vector2(0,0));uv.Add(new Vector2(0,1));uv.Add(new Vector2(1,1));uv.Add(new Vector2(1,0));for(int i=0;i<4;i++){n.Add(Vector3.up);c.Add(colour);}
  var list=Indices(key);list.Add(a);list.Add(a+1);list.Add(a+2);list.Add(a);list.Add(a+2);list.Add(a+3);Triangles+=2;}
 /// <summary>A quad from four corners (counter-clockwise seen from the side `normal` points to).</summary>
 public void Face(int key,Vector3 p0,Vector3 p1,Vector3 p2,Vector3 p3,Vector3 normal,Color32 colour=default){var a=v.Count;v.Add(p0);v.Add(p1);v.Add(p2);v.Add(p3);
  uv.Add(new Vector2(0,0));uv.Add(new Vector2(1,0));uv.Add(new Vector2(1,1));uv.Add(new Vector2(0,1));for(int i=0;i<4;i++){n.Add(normal);c.Add(colour);}
  // Unity's front faces wind clockwise seen from the front, which makes Cross(p1-p0,p2-p0) point along the normal.
  bool front=Vector3.Dot(Vector3.Cross(p1-p0,p2-p0),normal)>0;var list=Indices(key);if(front){list.Add(a);list.Add(a+1);list.Add(a+2);list.Add(a);list.Add(a+2);list.Add(a+3);}else{list.Add(a);list.Add(a+2);list.Add(a+1);list.Add(a);list.Add(a+3);list.Add(a+2);}Triangles+=2;}

 /// <summary>The finished mesh and, per submesh, the material key it was built with.</summary>
 public Mesh Build(string name,out int[] keys){
  var mesh=new Mesh{name=name};if(v.Count>65535)mesh.indexFormat=IndexFormat.UInt32;mesh.SetVertices(v);mesh.SetNormals(n);mesh.SetUVs(0,uv);mesh.SetColors(c);
  mesh.subMeshCount=sub.Count;keys=new int[sub.Count];int s=0;foreach(var pair in sub){mesh.SetTriangles(pair.Value,s,false);keys[s++]=pair.Key;}
  mesh.RecalculateBounds();mesh.UploadMeshData(false);return mesh; // stays readable for the budget and clearance checks
 }
 /// <summary>Drops cached source arrays (call after building every module).</summary>
 public static void Release()=>sources.Clear();
}
}
