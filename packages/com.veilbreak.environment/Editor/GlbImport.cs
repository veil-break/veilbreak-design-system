using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
namespace Veilbreak.Cityscape.Editor {
/// <summary>Reads the project's static GLB models (Models~) into meshes: one submesh per palette material in use, glTF's
/// right-handed axes mirrored on x into Unity's, winding reversed. Supports node TRS/matrix, float positions, normals and
/// uv0, indexed or not; no skins, morphs or textures (the city models have none). Vertex colours carry node data for
/// StreetModules, not colour: r is a per-node hash, g marks window panes (glass nodes named window*), a is zero, so a
/// raw mesh emits nothing through the City shader.</summary>
public static class GlbImport {
 public sealed class Result { public Mesh mesh; public int[] slots; }

 public static Result Read(string path,IList<string> palette){
  var bytes=File.ReadAllBytes(path);
  if(BitConverter.ToUInt32(bytes,0)!=0x46546C67)throw new InvalidDataException(path+" is not a GLB");
  int jsonLength=BitConverter.ToInt32(bytes,12);var json=Json.Parse(Encoding.UTF8.GetString(bytes,20,jsonLength)) as Dictionary<string,object>;
  int binStart=20+jsonLength+8;
  var accessors=(List<object>)json["accessors"];var views=(List<object>)json["bufferViews"];var meshes=(List<object>)json["meshes"];
  var materials=json.TryGetValue("materials",out var m)?(List<object>)m:new List<object>();
  var bySlot=new SortedDictionary<int,List<int>>();var positions=new List<Vector3>();var normals=new List<Vector3>();var uvs=new List<Vector2>();var colours=new List<Color32>();int nodeIndex=-1;
  foreach(Dictionary<string,object> node in (List<object>)json["nodes"]){
   nodeIndex++;if(node.ContainsKey("children"))throw new InvalidDataException(path+": nested nodes are not supported; export a flat node list");
   if(!node.TryGetValue("mesh",out var meshIndex))continue;var matrix=NodeMatrix(node);var normalMatrix=matrix.inverse.transpose;
   string nodeName=node.TryGetValue("name",out var nn0)?(string)nn0:"";byte nodeHash=(byte)(unchecked((uint)(nodeIndex+1)*2654435761u)>>24);
   foreach(Dictionary<string,object> prim in (List<object>)((Dictionary<string,object>)meshes[Convert.ToInt32(meshIndex)])["primitives"]){
    var attributes=(Dictionary<string,object>)prim["attributes"];
    var p=Floats(bytes,binStart,accessors,views,Convert.ToInt32(attributes["POSITION"]),3);
    var n=attributes.ContainsKey("NORMAL")?Floats(bytes,binStart,accessors,views,Convert.ToInt32(attributes["NORMAL"]),3):null;
    var t=attributes.ContainsKey("TEXCOORD_0")?Floats(bytes,binStart,accessors,views,Convert.ToInt32(attributes["TEXCOORD_0"]),2):null;
    int count=p.Length/3,first=positions.Count;
    for(int i=0;i<count;i++){
     var v=matrix.MultiplyPoint3x4(new Vector3(p[i*3],p[i*3+1],p[i*3+2]));positions.Add(new Vector3(-v.x,v.y,v.z));
     var nn=n==null?Vector3.up:normalMatrix.MultiplyVector(new Vector3(n[i*3],n[i*3+1],n[i*3+2])).normalized;normals.Add(new Vector3(-nn.x,nn.y,nn.z));
     uvs.Add(t==null?Vector2.zero:new Vector2(t[i*2],1-t[i*2+1]));
    }
    var index=prim.ContainsKey("indices")?Indices(bytes,binStart,accessors,views,Convert.ToInt32(prim["indices"])):null;
    string name=prim.TryGetValue("material",out var mi)?(string)((Dictionary<string,object>)materials[Convert.ToInt32(mi)])["name"]:palette[0];
    int slot=palette.IndexOf(name);if(slot<0)throw new InvalidDataException(path+": material '"+name+"' is not in the palette");
    var tag=new Color32(nodeHash,(byte)(name=="glass"&&nodeName.StartsWith("window")?255:0),0,0);for(int i=0;i<count;i++)colours.Add(tag);
    if(!bySlot.TryGetValue(slot,out var list))bySlot[slot]=list=new List<int>();
    int triangles=(index?.Length??count)/3;
    for(int k=0;k<triangles;k++){int a=index?[k*3]??k*3,b=index?[k*3+1]??k*3+1,c=index?[k*3+2]??k*3+2;list.Add(first+a);list.Add(first+c);list.Add(first+b);} // mirrored: reverse winding
   }
  }
  var mesh=new Mesh{name=Path.GetFileNameWithoutExtension(path)};
  if(positions.Count>65535)mesh.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;
  mesh.SetVertices(positions);mesh.SetNormals(normals);mesh.SetUVs(0,uvs);mesh.SetColors(colours);mesh.subMeshCount=bySlot.Count;
  var slots=new int[bySlot.Count];int s=0;foreach(var pair in bySlot){mesh.SetTriangles(pair.Value,s,false);slots[s++]=pair.Key;}
  mesh.RecalculateBounds();mesh.RecalculateTangents();mesh.UploadMeshData(false);
  return new Result{mesh=mesh,slots=slots};
 }

 static Matrix4x4 NodeMatrix(Dictionary<string,object> node){
  if(node.TryGetValue("matrix",out var mo)){var a=(List<object>)mo;var r=new Matrix4x4();for(int i=0;i<16;i++)r[i%4,i/4]=Convert.ToSingle(a[i]);return r;}
  var t=node.TryGetValue("translation",out var to)?V((List<object>)to):Vector3.zero;var s=node.TryGetValue("scale",out var so)?V((List<object>)so):Vector3.one;
  var q=Quaternion.identity;if(node.TryGetValue("rotation",out var ro)){var r=(List<object>)ro;q=new Quaternion(Convert.ToSingle(r[0]),Convert.ToSingle(r[1]),Convert.ToSingle(r[2]),Convert.ToSingle(r[3]));}
  return Matrix4x4.TRS(t,q,s); // still glTF space; Read mirrors after transforming
 }
 static Vector3 V(List<object> a)=>new Vector3(Convert.ToSingle(a[0]),Convert.ToSingle(a[1]),Convert.ToSingle(a[2]));

 static float[] Floats(byte[] bytes,int bin,List<object> accessors,List<object> views,int accessor,int width){
  var a=(Dictionary<string,object>)accessors[accessor];if(Convert.ToInt32(a["componentType"])!=5126)throw new InvalidDataException("Only float attributes are supported");
  var view=(Dictionary<string,object>)views[Convert.ToInt32(a["bufferView"])];int count=Convert.ToInt32(a["count"]);
  int offset=bin+(view.TryGetValue("byteOffset",out var vo)?Convert.ToInt32(vo):0)+(a.TryGetValue("byteOffset",out var ao)?Convert.ToInt32(ao):0);
  int stride=view.TryGetValue("byteStride",out var so)?Convert.ToInt32(so):width*4;var r=new float[count*width];
  for(int i=0;i<count;i++)for(int j=0;j<width;j++)r[i*width+j]=BitConverter.ToSingle(bytes,offset+i*stride+j*4);
  return r;
 }
 static int[] Indices(byte[] bytes,int bin,List<object> accessors,List<object> views,int accessor){
  var a=(Dictionary<string,object>)accessors[accessor];var view=(Dictionary<string,object>)views[Convert.ToInt32(a["bufferView"])];int count=Convert.ToInt32(a["count"]),type=Convert.ToInt32(a["componentType"]);
  int offset=bin+(view.TryGetValue("byteOffset",out var vo)?Convert.ToInt32(vo):0)+(a.TryGetValue("byteOffset",out var ao)?Convert.ToInt32(ao):0);var r=new int[count];
  for(int i=0;i<count;i++)r[i]=type==5125?(int)BitConverter.ToUInt32(bytes,offset+i*4):type==5123?BitConverter.ToUInt16(bytes,offset+i*2):bytes[offset+i];
  return r;
 }

 /// <summary>A minimal JSON reader for GLB headers: objects, arrays, strings, numbers, booleans and null.</summary>
 static class Json {
  public static object Parse(string s){int i=0;var v=Value(s,ref i);return v;}
  static bool Is(string s,int i,string word)=>string.CompareOrdinal(s,i,word,0,word.Length)==0;
  static void Skip(string s,ref int i){while(i<s.Length&&char.IsWhiteSpace(s[i]))i++;}
  static object Value(string s,ref int i){
   Skip(s,ref i);char c=s[i];
   if(c=='{'){var d=new Dictionary<string,object>();i++;Skip(s,ref i);if(s[i]=='}'){i++;return d;}while(true){Skip(s,ref i);var k=(string)Value(s,ref i);Skip(s,ref i);i++;d[k]=Value(s,ref i);Skip(s,ref i);if(s[i++]=='}')return d;}}
   if(c=='['){var l=new List<object>();i++;Skip(s,ref i);if(s[i]==']'){i++;return l;}while(true){l.Add(Value(s,ref i));Skip(s,ref i);if(s[i++]==']')return l;}}
   if(c=='"'){var b=new StringBuilder();i++;while(s[i]!='"'){if(s[i]=='\\'){i++;char e=s[i];b.Append(e=='n'?'\n':e=='t'?'\t':e=='u'?(char)Convert.ToInt32(s.Substring(i+1,4),16):e);if(e=='u')i+=4;}else b.Append(s[i]);i++;}i++;return b.ToString();}
   if(Is(s,i,"true")){i+=4;return true;}if(Is(s,i,"false")){i+=5;return false;}if(Is(s,i,"null")){i+=4;return null;}
   int start=i;while(i<s.Length&&"+-0123456789.eE".IndexOf(s[i])>=0)i++;return double.Parse(s.Substring(start,i-start),System.Globalization.CultureInfo.InvariantCulture);
  }
 }
}
}
