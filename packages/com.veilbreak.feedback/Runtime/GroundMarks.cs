using UnityEngine;
using UnityEngine.Rendering;
namespace Veilbreak.Feedback {
/// <summary>Flat atlas quads on the ground plane, rebuilt into one dynamic mesh each frame: one draw call for any number.
/// Positions are in the batch's local space; parent it to a road-anchored transform to make the quads drift with the street.</summary>
public sealed class QuadBatch {
 readonly Vector3[] vertices;readonly Color32[] colors;readonly Vector2[] uvs;readonly int[] indices;readonly Mesh mesh;int used;
 public readonly MeshRenderer renderer;public int Capacity=>indices.Length/6;public int Count=>used;
 public QuadBatch(Transform parent,string name,Material material,int capacity){vertices=new Vector3[capacity*4];colors=new Color32[capacity*4];uvs=new Vector2[capacity*4];indices=new int[capacity*6];
  for(int i=0;i<capacity;i++){int v=i*4,t=i*6;indices[t]=v;indices[t+1]=v+1;indices[t+2]=v+2;indices[t+3]=v;indices[t+4]=v+2;indices[t+5]=v+3;}
  mesh=new Mesh{name=name};mesh.MarkDynamic();var go=new GameObject(name);go.transform.SetParent(parent,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;renderer=go.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;
  renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;renderer.lightProbeUsage=LightProbeUsage.Off;renderer.reflectionProbeUsage=ReflectionProbeUsage.Off;}
 public void Begin(){used=0;}
 /// <summary>One quad at <paramref name="centre"/> on the local xz plane; size is the full width, aspect stretches it along its rotated z.</summary>
 public bool Add(Vector3 centre,float size,float degrees,FxTile tile,Color32 color,float aspect=1){if(used*6>=indices.Length)return false;
  float a=degrees*Mathf.Deg2Rad,c=Mathf.Cos(a)*size*.5f,s=Mathf.Sin(a)*size*.5f;var right=new Vector3(c,0,-s);var forward=new Vector3(s,0,c)*aspect;var t=EffectEmitters.Tile(tile);int v=used*4;
  vertices[v]=centre-right-forward;vertices[v+1]=centre-right+forward;vertices[v+2]=centre+right+forward;vertices[v+3]=centre+right-forward;
  uvs[v]=new Vector2(t.z,t.w);uvs[v+1]=new Vector2(t.z,t.w+t.y);uvs[v+2]=new Vector2(t.z+t.x,t.w+t.y);uvs[v+3]=new Vector2(t.z+t.x,t.w);
  for(int i=0;i<4;i++)colors[v+i]=color;used++;return true;}
 public void End(){mesh.Clear(true);if(used==0){renderer.enabled=false;return;}renderer.enabled=true;int n=used*4;mesh.SetVertices(vertices,0,n);mesh.SetColors(colors,0,n);mesh.SetUVs(0,uvs,0,n);mesh.SetIndices(indices,0,used*6,MeshTopology.Triangles,0,false);mesh.bounds=new Bounds(Vector3.zero,new Vector3(400,10,400));}
 public void Dispose(){if(mesh==null)return;if(Application.isPlaying){Object.Destroy(mesh);if(renderer!=null)Object.Destroy(renderer.gameObject);}else{Object.DestroyImmediate(mesh);if(renderer!=null)Object.DestroyImmediate(renderer.gameObject);}}
}
/// <summary>Scorch and crack marks left on the road by explosions and heavy impacts: a bounded ring buffer (oldest replaced)
/// that fades each mark out over its life. Lives under a road-anchored transform, so the marks drift away with the street.</summary>
public sealed class GroundMarks:MonoBehaviour {
 public const int HighCapacity=96,LowCapacity=32;
 struct Mark { public Vector3 at;public float size,degrees,life,age;public FxTile tile;public Color color; }
 Mark[] marks=new Mark[0];QuadBatch batch;int next;readonly System.Random random=new System.Random(23);
 public int Capacity=>marks.Length;
 public int Live{get{int n=0;foreach(var m in marks)if(m.age<m.life)n++;return n;}}
 public void Initialize(Material material,bool high){marks=new Mark[high?HighCapacity:LowCapacity];batch=new QuadBatch(transform,"Ground marks",material,marks.Length);}
 public void Add(MarkKind kind,Vector3 world,float size,float life=12){if(kind==MarkKind.None||marks.Length==0)return;var at=transform.InverseTransformPoint(world);at.y=.02f;
  marks[next]=new Mark{at=at,size=size,degrees=(float)random.NextDouble()*360,life=life,tile=kind==MarkKind.Crack?FxTile.Crack:FxTile.Scorch,color=new Color(1,1,1,kind==MarkKind.Crack?.85f:.8f)};next=(next+1)%marks.Length;}
 public void Advance(float delta){if(batch==null)return;batch.Begin();for(int i=0;i<marks.Length;i++){ref var m=ref marks[i];if(m.age>=m.life)continue;m.age+=delta;if(m.age>=m.life)continue;
   float t=m.age/m.life,alpha=Mathf.Clamp01(m.age/.08f)*(1-Mathf.SmoothStep(0,1,(t-.6f)/.4f));var c=m.color;c.a*=alpha;batch.Add(m.at,m.size*(1+Mathf.Min(m.age,.15f)),m.degrees,m.tile,c);}batch.End();}
 public void Clear(){for(int i=0;i<marks.Length;i++)marks[i].age=marks[i].life;batch?.Begin();batch?.End();}
 void OnDestroy()=>batch?.Dispose();
}
/// <summary>A few pooled point lights for explosion and cast flashes, faded out over their life. The capacity is the tier's
/// budget (zero turns flashes off); the oldest flash is reused when all are busy.</summary>
public sealed class LightFlashes:MonoBehaviour {
 sealed class Flash { public Light light;public float peak,life,age; }
 Flash[] flashes=new Flash[0];int next;
 public int Capacity=>flashes.Length;
 public int Lit{get{int n=0;foreach(var f in flashes)if(f.light.enabled)n++;return n;}}
 public void Initialize(int capacity){flashes=new Flash[capacity];for(int i=0;i<capacity;i++){var go=new GameObject("Flash "+i);go.transform.SetParent(transform,false);var l=go.AddComponent<Light>();l.type=LightType.Point;l.shadows=LightShadows.None;l.enabled=false;flashes[i]=new Flash{light=l};}}
 public void Play(Vector3 at,Color color,float intensity,float range,float life){if(flashes.Length==0||intensity<=0)return;var f=flashes[next];next=(next+1)%flashes.Length;f.light.transform.position=at;f.light.color=color;f.light.range=range;f.peak=intensity;f.life=Mathf.Max(.05f,life);f.age=0;f.light.intensity=intensity;f.light.enabled=true;}
 public void Advance(float delta){foreach(var f in flashes){if(!f.light.enabled)continue;f.age+=delta;float t=f.age/f.life;if(t>=1){f.light.enabled=false;continue;}f.light.intensity=f.peak*(1-t)*(1-t);}}
 public void Clear(){foreach(var f in flashes)f.light.enabled=false;}
}
}
