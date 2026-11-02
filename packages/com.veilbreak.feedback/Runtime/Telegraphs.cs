using UnityEngine;
using UnityEngine.Rendering;
namespace Veilbreak.Feedback {
public enum TelegraphShape { Circle, Cone, Segment }
public enum TelegraphPhase { Preview, Windup, Active, Residue }
/// <summary>One ground projection in world metres on the horizontal plane through <see cref="origin"/>. Cone angle is the full opening in degrees; segments are capsules.
/// <see cref="curtain"/> raises a light curtain of that many metres from the footprint's outline (Veilbreak's god-ray telegraph edge); 0 draws none.</summary>
public struct TelegraphRequest {
 public TelegraphShape shape; public Vector3 origin,end; public float radius,halfWidth,angle;
 public Color color; public TelegraphPhase phase; public float progress; public bool hostile; public float curtain;
}
/// <summary>Quad placement and the signed distance Telegraph.shader evaluates, so tests can compare drawn footprints with simulation hit areas.</summary>
public static class TelegraphGeometry {
 /// <summary>Room around the footprint for the border and outer glow.</summary>
 public const float Margin=.25f;
 public static void Layout(in TelegraphRequest r,out Vector3 position,out Quaternion rotation,out Vector2 size){
  var flat=r.end-r.origin;flat.y=0;rotation=Quaternion.LookRotation(flat.sqrMagnitude>1e-6f?flat.normalized:Vector3.forward,Vector3.up);
  if(r.shape==TelegraphShape.Segment){position=(r.origin+r.end)*.5f;position.y=r.origin.y;float across=2*(r.halfWidth+Margin);size=new Vector2(across,flat.magnitude+across);}
  else{position=r.origin;float extent=2*(r.radius+Margin);size=new Vector2(extent,extent);}
 }
 /// <summary>Shader parameters: x shape, y radius or half width, z half angle in radians, w segment length.</summary>
 public static Vector4 Shape(in TelegraphRequest r){var flat=r.end-r.origin;flat.y=0;return new Vector4((int)r.shape,r.shape==TelegraphShape.Segment?r.halfWidth:r.radius,r.angle*.5f*Mathf.Deg2Rad,flat.magnitude);}
 public static float Distance(in TelegraphRequest r,Vector3 world){Layout(r,out var position,out var rotation,out _);var local=Quaternion.Inverse(rotation)*(world-position);return Distance(Shape(r),new Vector2(local.x,local.z));}
 /// <summary>Signed distance in metres with local +y facing the aim; keep identical to Telegraph.shader.</summary>
 public static float Distance(Vector4 shape,Vector2 p){
  if(shape.x>1.5f){p.y-=Mathf.Clamp(p.y,-shape.w*.5f,shape.w*.5f);return p.magnitude-shape.y;}
  if(shape.x>.5f){var c=new Vector2(Mathf.Sin(shape.z),Mathf.Cos(shape.z));p.x=Mathf.Abs(p.x);float l=p.magnitude-shape.y;float m=(p-c*Mathf.Clamp(Vector2.Dot(p,c),0,shape.y)).magnitude;float side=c.y*p.x-c.x*p.y;return Mathf.Max(l,side>0?m:side<0?-m:0);}
  return p.magnitude-shape.y;
 }
 public static bool Inside(in TelegraphRequest r,Vector3 world)=>Distance(r,world)<=0;
 /// <summary>Footprints of one side merge: a point belongs to the nearest footprint of its side (the lower index on a tie), which shades it with its own distance.
 /// So overlaps fill once and only the outline of the union is drawn. Hostile and friendly footprints never merge.</summary>
 public static int Owner(System.Collections.Generic.IReadOnlyList<TelegraphRequest> requests,int index,Vector3 world){var self=requests[index];int owner=index;float best=Distance(self,world);
  for(int k=0;k<requests.Count;k++){if(k==index||requests[k].hostile!=self.hostile)continue;float d=Distance(requests[k],world);if(d<best||d==best&&k<owner){best=d;owner=k;}}return owner;}
 /// <summary>Signed distance to the union of every footprint on <paramref name="hostile"/>'s side.</summary>
 public static float Union(System.Collections.Generic.IReadOnlyList<TelegraphRequest> requests,bool hostile,Vector3 world){float best=float.MaxValue;foreach(var r in requests)if(r.hostile==hostile)best=Mathf.Min(best,Distance(r,world));return best;}
 /// <summary>Points along the footprint's outline in world space (closed loop, about <paramref name="spacing"/> metres apart), into <paramref name="into"/>.</summary>
 public static void Outline(in TelegraphRequest r,System.Collections.Generic.List<Vector3> into,float spacing=.3f){into.Clear();var flat=r.end-r.origin;flat.y=0;var forward=flat.sqrMagnitude>1e-6f?flat.normalized:Vector3.forward;var side=Vector3.Cross(Vector3.up,forward);float y=r.origin.y;
  void Arc(Vector3 centre,float radius,float from,float to){int n=Mathf.Max(2,Mathf.CeilToInt(Mathf.Abs(to-from)*Mathf.Deg2Rad*radius/spacing));for(int i=0;i<=n;i++){float a=Mathf.Lerp(from,to,i/(float)n)*Mathf.Deg2Rad;into.Add(centre+(forward*Mathf.Cos(a)+side*Mathf.Sin(a))*radius);}}
  void Line(Vector3 a,Vector3 b){int n=Mathf.Max(1,Mathf.CeilToInt(Vector3.Distance(a,b)/spacing));for(int i=1;i<n;i++)into.Add(Vector3.Lerp(a,b,i/(float)n));}
  var o=new Vector3(r.origin.x,y,r.origin.z);
  if(r.shape==TelegraphShape.Segment){var e=o+forward*flat.magnitude;Arc(e,r.halfWidth,-90,90);Line(e-side*r.halfWidth,o-side*r.halfWidth);Arc(o,r.halfWidth,-270,-90);Line(o+side*r.halfWidth,e+side*r.halfWidth);}
  else if(r.shape==TelegraphShape.Cone){float half=r.angle*.5f;into.Add(o);Line(o,o+(forward*Mathf.Cos(-half*Mathf.Deg2Rad)+side*Mathf.Sin(-half*Mathf.Deg2Rad))*r.radius);Arc(o,r.radius,-half,half);Line(o+(forward*Mathf.Cos(half*Mathf.Deg2Rad)+side*Mathf.Sin(half*Mathf.Deg2Rad))*r.radius,o);}
  else Arc(o,r.radius,0,360);}
}
/// <summary>Bounded pool of ground projections, redrawn each frame from the caller's authoritative state. Each projection learns the
/// overlapping footprints of its own side (up to <see cref="Neighbours"/>) so the shader can draw their union, as <see cref="TelegraphGeometry.Owner"/> describes.</summary>
public sealed class GroundTelegraphs:MonoBehaviour {
 public const int Capacity=64,Neighbours=8;
 static readonly int ShapeId=Shader.PropertyToID("_Shape"),SizeId=Shader.PropertyToID("_Size"),ColorId=Shader.PropertyToID("_Color"),StateId=Shader.PropertyToID("_State"),OthersId=Shader.PropertyToID("_Others"),OtherCountId=Shader.PropertyToID("_OtherCount");
 readonly MeshRenderer[] renderers=new MeshRenderer[Capacity];readonly TelegraphRequest[] requests=new TelegraphRequest[Capacity];readonly Vector3[] centres=new Vector3[Capacity],forwards=new Vector3[Capacity];readonly float[] reach=new float[Capacity];
 readonly Vector4[] others=new Vector4[Neighbours*2];readonly int[] counts=new int[Capacity];MaterialPropertyBlock block;Material material;Mesh quad;int used;
 // Light curtains: one dynamic mesh of vertical strips along every outline that asks for one, drawn by Telegraph.shader's curtain pass.
 Mesh curtainMesh;MeshRenderer curtainRenderer;readonly System.Collections.Generic.List<Vector3> outline=new System.Collections.Generic.List<Vector3>(),cv=new System.Collections.Generic.List<Vector3>();
 readonly System.Collections.Generic.List<Color> cc=new System.Collections.Generic.List<Color>();readonly System.Collections.Generic.List<Vector2> cu=new System.Collections.Generic.List<Vector2>(),cf=new System.Collections.Generic.List<Vector2>();readonly System.Collections.Generic.List<int> ct=new System.Collections.Generic.List<int>();
 /// <summary>Curtain strips drawn in the last frame.</summary>
 public int CurtainStrips{get;private set;}
 /// <summary>Freezes scrolling dashes, chevrons and pulses.</summary>
 public bool reducedMotion;
 public int Count=>used;
 /// <summary>Same-side footprints that overlap projection <paramref name="index"/> in the last drawn frame.</summary>
 public int Overlaps(int index)=>index>=0&&index<used?counts[index]:0;
 public void Initialize(Material material){this.material=material;block=new MaterialPropertyBlock();quad=new Mesh{name="Telegraph quad",vertices=new[]{new Vector3(-.5f,0,-.5f),new Vector3(-.5f,0,.5f),new Vector3(.5f,0,.5f),new Vector3(.5f,0,-.5f)},uv=new[]{new Vector2(0,0),new Vector2(0,1),new Vector2(1,1),new Vector2(1,0)},uv2=new Vector2[4],triangles=new[]{0,1,2,0,2,3}};quad.RecalculateBounds();
  curtainMesh=new Mesh{name="Telegraph curtains",indexFormat=IndexFormat.UInt32};curtainMesh.MarkDynamic();var go=new GameObject("Telegraph curtains");go.transform.SetParent(transform,false);go.AddComponent<MeshFilter>().sharedMesh=curtainMesh;
  curtainRenderer=go.AddComponent<MeshRenderer>();curtainRenderer.sharedMaterial=material;curtainRenderer.shadowCastingMode=ShadowCastingMode.Off;curtainRenderer.receiveShadows=false;curtainRenderer.lightProbeUsage=LightProbeUsage.Off;curtainRenderer.reflectionProbeUsage=ReflectionProbeUsage.Off;go.SetActive(false);}
 public void Begin(){used=0;}
 /// <summary>Returns false once the budget is spent; submit the most important projections first.</summary>
 public bool Draw(in TelegraphRequest r){if(material==null||used>=Capacity)return false;var renderer=renderers[used]??(renderers[used]=Create(used));TelegraphGeometry.Layout(r,out var position,out var rotation,out var size);var t=renderer.transform;t.SetPositionAndRotation(position,rotation);t.localScale=new Vector3(size.x,1,size.y);
  requests[used]=r;centres[used]=position;forwards[used]=rotation*Vector3.forward;reach[used]=.5f*Mathf.Sqrt(size.x*size.x+size.y*size.y);if(!renderer.gameObject.activeSelf)renderer.gameObject.SetActive(true);used++;return true;}
 /// <summary>Binds every drawn projection with its overlapping neighbours and hides the rest of the pool.</summary>
 public void End(){
  for(int i=0;i<used;i++){var r=requests[i];int n=0;
   for(int k=0;k<used&&n<Neighbours;k++){if(k==i||requests[k].hostile!=r.hostile)continue;var gap=centres[k]-centres[i];gap.y=0;float limit=reach[i]+reach[k];if(gap.sqrMagnitude>limit*limit)continue;
    var shape=TelegraphGeometry.Shape(requests[k]);if(k<i)shape.x+=4;others[n*2]=new Vector4(centres[k].x,centres[k].z,forwards[k].x,forwards[k].z);others[n*2+1]=shape;n++;}
   for(int k=n*2;k<others.Length;k++)others[k]=Vector4.zero;counts[i]=n;
   block.SetVector(ShapeId,TelegraphGeometry.Shape(r));block.SetVector(SizeId,new Vector4(renderers[i].transform.localScale.x,renderers[i].transform.localScale.z,0,0));block.SetColor(ColorId,r.color);block.SetVector(StateId,new Vector4((int)r.phase,Mathf.Clamp01(r.progress),r.hostile?1:0,reducedMotion?0:1));
   block.SetVectorArray(OthersId,others);block.SetFloat(OtherCountId,n);renderers[i].SetPropertyBlock(block);}
  for(int i=used;i<Capacity;i++)if(renderers[i]!=null&&renderers[i].gameObject.activeSelf)renderers[i].gameObject.SetActive(false);
  Curtains();}
 /// <summary>Raises the light curtains: a strip per outline segment, skipping segments inside another footprint of the same side so a
 /// merged union keeps one rim. Hostile windups rise as they charge.</summary>
 void Curtains(){if(curtainMesh==null)return;cv.Clear();cc.Clear();cu.Clear();cf.Clear();ct.Clear();int strips=0;var local=curtainRenderer.transform.worldToLocalMatrix;
  for(int i=0;i<used;i++){var r=requests[i];if(r.curtain<=0)continue;float height=r.curtain*(r.phase==TelegraphPhase.Windup?.35f+.65f*Mathf.Clamp01(r.progress):1);
   TelegraphGeometry.Outline(r,outline);int n=outline.Count;float along=0;
   for(int k=0;k<n;k++){var a=outline[k];var b=outline[(k+1)%n];float length=Vector3.Distance(a,b);if(length<1e-4f)continue;
    var mid=(a+b)*.5f;bool buried=false;for(int j=0;j<used&&!buried;j++)if(j!=i&&requests[j].hostile==r.hostile&&TelegraphGeometry.Distance(requests[j],mid)<-.03f)buried=true;
    if(!buried){int s=cv.Count;cv.Add(local.MultiplyPoint3x4(a));cv.Add(local.MultiplyPoint3x4(b));cv.Add(local.MultiplyPoint3x4(a+Vector3.up*height));cv.Add(local.MultiplyPoint3x4(b+Vector3.up*height));for(int q=0;q<4;q++){cc.Add(r.color);cf.Add(new Vector2(1,r.hostile?1:0));}
     cu.Add(new Vector2(along,0));cu.Add(new Vector2(along+length,0));cu.Add(new Vector2(along,1));cu.Add(new Vector2(along+length,1));ct.Add(s);ct.Add(s+2);ct.Add(s+1);ct.Add(s+1);ct.Add(s+2);ct.Add(s+3);strips++;}
    along+=length;}}
  CurtainStrips=strips;curtainMesh.Clear();if(strips>0){curtainMesh.SetVertices(cv);curtainMesh.SetColors(cc);curtainMesh.SetUVs(0,cu);curtainMesh.SetUVs(1,cf);curtainMesh.SetTriangles(ct,0,true);
   block.Clear();block.SetVector(StateId,new Vector4(0,0,0,reducedMotion?0:1));curtainRenderer.SetPropertyBlock(block);}
  // The curtain object is inactive while nothing raises one, so it never counts as a live telegraph.
  if(curtainRenderer.gameObject.activeSelf!=strips>0)curtainRenderer.gameObject.SetActive(strips>0);}
 MeshRenderer Create(int index){var go=new GameObject("Telegraph "+index);go.transform.SetParent(transform,false);go.AddComponent<MeshFilter>().sharedMesh=quad;var renderer=go.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;renderer.lightProbeUsage=LightProbeUsage.Off;renderer.reflectionProbeUsage=ReflectionProbeUsage.Off;return renderer;}
 void OnDestroy(){foreach(var m in new[]{quad,curtainMesh}){if(m==null)continue;if(Application.isPlaying)Destroy(m);else DestroyImmediate(m);}}
}
}
