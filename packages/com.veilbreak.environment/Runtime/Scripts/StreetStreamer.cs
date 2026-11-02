using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace Veilbreak.Cityscape {
/// <summary>Streams the boulevard through the bounded <see cref="SegmentStreamer"/>: each 8 m segment shows the
/// shared road and sidewalk meshes plus a left and a right side block chosen by the segment hash from the prebuilt
/// modules of its theme. Nothing is instantiated after <see cref="Initialize"/> except one crossroads set, built on first
/// use and reused; a theme change only affects segments that stream in afterwards. The road itself carries no collision:
/// the simulation owns combat space.</summary>
public sealed class StreetStreamer:MonoBehaviour {
 sealed class Slot { public GameObject root;public MeshFilter road,walks,left,right;public StreetModules.Module l,r;public int theme=-1;public long corner=long.MinValue; }
 /// <summary>The crossroads set: crossing, pavement, skyline, the two frontage blocks closing the T and each arm's first segments.</summary>
 sealed class Crossing { public GameObject root;public MeshFilter road,walks,skyline;public readonly MeshFilter[] wall=new MeshFilter[2];public readonly StreetModules.Module[] wallModules=new StreetModules.Module[2];
  public readonly Slot[] arms=new Slot[2*ArmSegments];public long index=long.MinValue;public double centre;public int side,theme=-1;public float turn; }
 /// <summary>Segments each arm shows ahead of the stream, and how far past the crossing the street re-bases onto the chosen arm
 /// (by then the crossing, the incoming road and the other arm are all behind the camera).</summary>
 public const int ArmSegments=2;public const float RebaseAfter=3.5f*SegmentStreamer.Length;
 const float L=SegmentStreamer.Length;
 static readonly int CrossId=Shader.PropertyToID("_VeilCross"),CrossRoadId=Shader.PropertyToID("_VeilCrossRoad");
 SegmentStreamer stream;StreetModules[] themes;CityKit kit;bool high=true;uint seed;readonly Dictionary<GameObject,Slot> slots=new Dictionary<GameObject,Slot>();
 Crossing cross;bool crossing;long released=long.MinValue;double presented;Quaternion turn=Quaternion.identity;Vector3 at;SegmentStreamer.Placement place;
 public IReadOnlyList<StreetModules> Themes=>themes;
 public int SegmentCount=>slots.Count;
 /// <summary>True while a crossroads is laid out (until the street re-bases onto the chosen arm).</summary>
 public bool AtCrossroads=>crossing;
 /// <summary>The crossroads' corner (its frame: incoming road along +z, arms along ±x), or null. Dressing parented to it follows the
 /// swing and hides with the crossroads on re-base.</summary>
 public Transform Corner=>crossing?cross.root.transform:null;

 /// <param name="segmentSeed">The host's per-segment seed; null uses <see cref="SegmentStreamer.DefaultSeed"/>.</param>
 public void Initialize(CityKit kit,StreetTheme[] looks,uint seed,int variants=8,bool high=true,SegmentStreamer.SeedFunction segmentSeed=null){
  this.kit=kit;this.high=high;this.seed=seed;themes=new StreetModules[looks.Length];for(int i=0;i<looks.Length;i++)themes[i]=new StreetModules(kit,looks[i],seed+(uint)i*977u,variants,high);
  stream=gameObject.AddComponent<SegmentStreamer>();stream.Initialize(seed,Configure,segmentSeed);place=Place;Shader.SetGlobalVector(CrossRoadId,Vector4.zero);
 }
 public void Present(double distance,int theme){
  presented=distance;
  if(crossing&&cross.side!=0&&cross.turn>=1&&distance>=cross.centre+RebaseAfter)Rebase();
  theme=Mathf.Clamp(theme,0,themes.Length-1);if(crossing)Frame(distance,theme);
  stream.Place=crossing?place:null;stream.Present(distance,theme);
  if(crossing){Shader.SetGlobalVector(CrossId,Global());Shader.SetGlobalVector(CrossRoadId,new Vector4((float)(cross.centre%1024),1,0,0));}
 }
 /// <summary>Blocks cast shadows on Standard only: on Low they were about 450 of the ~600 shadow casters in a dense fight
 /// (plan T6.2 cuts shadows before post effects). Characters keep theirs.</summary>
 public ShadowCastingMode BlockShadows=>high?ShadowCastingMode.On:ShadowCastingMode.Off;

 /// <summary>Lays a T crossroads at road distance `centre` metres, a segment boundary (the simulation places it on one; other values
 /// take their segment's forward boundary). The road ends in a frontage there with an arm to each side. `side` is the chosen arm (−1 left, +1 right, 0 undecided) and
 /// `turn` (0..1) how far the street has swung toward it: the swing is about the crossing, where the squad stops. Once a road is chosen
 /// the stream continues along that arm with the same segment seeds the arm already showed, and when the turn is complete and the
 /// crossing is <see cref="RebaseAfter"/> behind, the street re-bases onto it with nothing changing on screen; later calls for that
 /// crossing are ignored. A negative centre clears it and forgets it, so a new run may lay one out at the same place.</summary>
 public void Junction(double centre,int side,float turn){
  if(centre<0){if(crossing)Rebase();released=long.MinValue;return;}
  long index=(long)System.Math.Floor(centre/L);if(index==released)return;
  if(cross==null)cross=BuildCrossing();
  if(!crossing||cross.index!=index){cross.index=index;cross.centre=index*(double)L+L/2;cross.theme=-1;crossing=true;cross.root.SetActive(true);}
  cross.side=System.Math.Sign(side);cross.turn=cross.side==0?0:Mathf.Clamp01(turn);
 }
 /// <summary>Where road distance `at` metres, `lateral` metres right of the centre line, is now in this street's local space, and the
 /// way the road runs there: through a crossroads it follows the T, the swing and the chosen arm. Props on the road use it.</summary>
 /// <summary>True when a building's facade (at x = ±<see cref="StreetModules.Building"/>) stands on `side` (−1 left, 1 right) at road distance
 /// `at`: not an open lot, a crossroads corner or a segment out of view. Hostiles tear their arrival rifts only in a real wall.</summary>
 public bool Facade(double at,int side){if(crossing)return false;float z=(float)(at-presented);
  foreach(var slot in slots.Values){if(!slot.root.activeInHierarchy||Mathf.Abs(slot.root.transform.localPosition.z-z)>L/2)continue;var m=side<0?slot.l:slot.r;return m!=null&&!m.gap&&slot.corner==long.MinValue;}return false;}
 public Pose Road(double at,float lateral){
  if(!crossing)return new Pose(new Vector3(lateral,0,(float)(at-presented)),Quaternion.identity);
  float along=(float)(at-cross.centre);if(at<=cross.centre||cross.side==0)return new Pose(Local(new Vector3(lateral,0,along)),turn);
  var arm=Quaternion.Euler(0,cross.side*90,0);return new Pose(Local(arm*new Vector3(lateral,0,along)),turn*arm);
 }

 // The crossroads frame ("map"): the corner at the origin and the incoming road along +z. Local = turn * (map - at), where `at` is the
 // presented point on the route and `turn` swings the street by the chosen arm's share of 90 degrees.
 Vector3 Local(Vector3 map)=>turn*(map-at);
 void Frame(double distance,int theme){
  float yaw=cross.side*90f*cross.turn,along=(float)(distance-cross.centre);turn=Quaternion.Euler(0,-yaw,0);
  at=along<=0||cross.side==0?new Vector3(0,0,along):new Vector3(cross.side*along,0,0);
  cross.root.transform.SetLocalPositionAndRotation(Local(Vector3.zero),turn);
  if(cross.theme<0)Dress(theme); // a crossroads keeps the theme it was laid out in
  // Once a road is chosen the stream draws that arm; the other stays until the re-base.
  for(int s=0;s<2;s++){bool keep=cross.side!=(s==0?-1:1);for(int j=0;j<ArmSegments;j++){var root=cross.arms[s*ArmSegments+j].root;if(root.activeSelf!=keep)root.SetActive(keep);}}
 }
 /// <summary>The corner and the incoming road's heading in world space, for the street shader's crossroads paving.</summary>
 Vector4 Global(){var corner=transform.TransformPoint(Local(Vector3.zero));var heading=transform.TransformDirection(turn*Vector3.forward);return new Vector4(corner.x,corner.z,heading.x,heading.z);}

 bool Place(GameObject segment,long index,double distance,out Vector3 position,out Quaternion rotation){
  position=default;rotation=turn;var slot=slots[segment];long k=cross.index;bool nearRight=false,nearLeft=false,shown=true;
  if(index<=k-1){position=Local(new Vector3(0,0,(float)(index*(double)L-cross.centre)));if(index==k-1&&slot.corner!=index)CornerBlocks(slot,index);}
  else if(index<=k+1||cross.side==0){shown=false;Match(slot,index);}
  else{Match(slot,index);
   var arm=Quaternion.Euler(0,cross.side*90,0);position=Local(arm*new Vector3(0,0,(float)(index*(double)L-cross.centre)));rotation=turn*arm;
   // The arm's first segment leaves its near corner to the incoming road's last block.
   if(index==k+2){nearRight=cross.side>0;nearLeft=cross.side<0;}}
  Near(slot,nearLeft,nearRight);return shown;
 }
 /// <summary>The stream's first segments on an arm show what the arm itself showed, even if they streamed in under an earlier theme.</summary>
 void Match(Slot slot,long index){long k=cross.index;if(index>k+1&&index<=k+1+ArmSegments&&slot.theme!=cross.theme)Fill(slot,stream.Hash(seed,cross.theme,index),cross.theme);}
 /// <summary>The incoming road's last segment before a crossroads takes compact corner blocks.</summary>
 void CornerBlocks(Slot slot,long index){var set=themes[cross.theme];uint hash=stream.Hash(seed,cross.theme,index);Fill(slot,hash,cross.theme);
  slot.l=set.corners[(int)(hash%(uint)set.corners.Length)];slot.r=set.corners[(int)((hash>>11)%(uint)set.corners.Length)];Show(slot.left,slot.l);Show(slot.right,slot.r);slot.corner=index;}
 static void Near(Slot slot,bool hideLeft,bool hideRight){if(slot.left.gameObject.activeSelf==hideLeft)slot.left.gameObject.SetActive(!hideLeft);if(slot.right.gameObject.activeSelf==hideRight)slot.right.gameObject.SetActive(!hideRight);}
 void Rebase(){crossing=false;released=cross.index;cross.root.SetActive(false);turn=Quaternion.identity;at=Vector3.zero;stream.Place=null;foreach(var slot in slots.Values)Near(slot,false,false);
  Shader.SetGlobalVector(CrossRoadId,Vector4.zero);}

 Crossing BuildCrossing(){
  var c=new Crossing{root=new GameObject("Crossroads")};var t=c.root.transform;t.SetParent(transform,false);
  c.road=Part(t,"Crossing",0,ShadowCastingMode.Off);c.walks=Part(t,"Pavement",0,ShadowCastingMode.Off);c.skyline=Part(t,"Skyline",0,BlockShadows);
  // The frontage: two side blocks turned to face down the incoming road, each covering 8 m of the T.
  for(int i=0;i<2;i++){c.wall[i]=Part(t,"Frontage "+i,-90,BlockShadows);c.wall[i].transform.localPosition=new Vector3(i==0?-L/2:L/2,0,0);}
  for(int s=0;s<2;s++)for(int j=0;j<ArmSegments;j++){var go=new GameObject((s==0?"Left":"Right")+" arm "+(j+1));go.transform.SetParent(t,false);float side=s==0?-1:1;
   go.transform.SetLocalPositionAndRotation(new Vector3(side*(L/2+(j+1)*L),0,0),Quaternion.Euler(0,side*90,0));c.arms[s*ArmSegments+j]=Make(go);}
  return c;
 }
 /// <summary>Fills the crossroads from `theme`: frontage blocks by the crossing's hash (built blocks before empty lots, so the T stays
 /// closed) and each arm's segments with the seeds the stream uses for those indices, so the chosen arm continues seamlessly.</summary>
 void Dress(int theme){
  var c=cross;c.theme=theme;var set=themes[theme];Show(c.road,set.crossing);Show(c.walks,set.pavement);Show(c.skyline,set.skyline);
  uint hash=stream.Hash(seed,theme,c.index)^0x51ed27u;
  for(int i=0;i<2;i++){StreetModules.Module pick=null;for(int n=0;n<set.sides.Length&&pick==null;n++){var m=set.sides[(int)(((hash>>(i*7))+(uint)n)%(uint)set.sides.Length)];if(!m.gap)pick=m;}
   c.wallModules[i]=pick??set.sides[(int)(hash%(uint)set.sides.Length)];Show(c.wall[i],c.wallModules[i]);}
  for(int s=0;s<2;s++)for(int j=0;j<ArmSegments;j++){long index=c.index+2+j;var slot=c.arms[s*ArmSegments+j];Fill(slot,stream.Hash(seed,theme,index),theme);
   if(j==0)Near(slot,s==0,s==1);}
 }

 MeshFilter Part(Transform parent,string name,float yaw,ShadowCastingMode shadows){var go=new GameObject(name);go.transform.SetParent(parent,false);go.transform.localRotation=Quaternion.Euler(0,yaw,0);var r=go.AddComponent<MeshRenderer>();r.shadowCastingMode=shadows;r.lightProbeUsage=LightProbeUsage.Off;r.reflectionProbeUsage=ReflectionProbeUsage.Off;return go.AddComponent<MeshFilter>();}
 Slot Make(GameObject go){var t=go.transform;return new Slot{root=go,road=Part(t,"Road",0,ShadowCastingMode.Off),walks=Part(t,"Sidewalks",0,ShadowCastingMode.Off),left=Part(t,"Left block",180,BlockShadows),right=Part(t,"Right block",0,BlockShadows)};}
 void Configure(GameObject go,uint hash,int theme){if(!slots.TryGetValue(go,out var slot))slots[go]=slot=Make(go);Fill(slot,hash,theme);}
 void Fill(Slot slot,uint hash,int theme){
  var set=themes[theme];slot.theme=theme;slot.corner=long.MinValue;slot.l=set.sides[(int)(hash%(uint)set.sides.Length)];slot.r=set.sides[(int)((hash>>11)%(uint)set.sides.Length)];
  Show(slot.road,set.road);Show(slot.walks,set.sidewalks);Show(slot.left,slot.l);Show(slot.right,slot.r);
 }
 static void Show(MeshFilter f,StreetModules.Module m){f.sharedMesh=m.mesh;var r=f.GetComponent<MeshRenderer>();r.sharedMaterials=m.materials;ReflectionSet.Set(r,m.materials,m.emissive);}

 /// <summary>Appends every live emitter in world space with a stable key (segment slot and index) for the light budget.</summary>
 public void Emitters(List<(int key,Vector3 at,Emitter e)> into){
  int s=0;foreach(var pair in slots){var slot=pair.Value;if(slot.root.activeInHierarchy){Add(into,s*64,slot.left,slot.l);Add(into,s*64+32,slot.right,slot.r);}s++;}
  if(!crossing)return;for(int i=0;i<2;i++)Add(into,1024+i*32,cross.wall[i],cross.wallModules[i]);
  for(int a=0;a<cross.arms.Length;a++){var slot=cross.arms[a];Add(into,1088+a*64,slot.left,slot.l);Add(into,1088+a*64+32,slot.right,slot.r);}
 }
 static void Add(List<(int,Vector3,Emitter)> into,int key,MeshFilter f,StreetModules.Module m){if(m==null||!f.gameObject.activeInHierarchy)return;var t=f.transform;for(int i=0;i<m.emitters.Length&&i<32;i++)into.Add((key+i,t.TransformPoint(m.emitters[i].position),m.emitters[i]));}

 /// <summary>Triangles of the modules currently shown, for the per-tier budget checks.</summary>
 public int Triangles(int segment){int i=0;foreach(var slot in slots.Values){if(i++!=segment)continue;return (slot.l?.triangles??0)+(slot.r?.triangles??0)+themes[slot.theme].road.triangles+themes[slot.theme].sidewalks.triangles;}return 0;}

 void OnDestroy(){
  void Drop(MeshFilter f){if(f!=null)ReflectionSet.Remove(f.GetComponent<MeshRenderer>());}
  foreach(var slot in slots.Values){Drop(slot.road);Drop(slot.walks);Drop(slot.left);Drop(slot.right);}
  if(cross!=null){Drop(cross.road);Drop(cross.walks);Drop(cross.skyline);foreach(var f in cross.wall)Drop(f);foreach(var slot in cross.arms){Drop(slot.road);Drop(slot.walks);Drop(slot.left);Drop(slot.right);}}
  if(crossing)Shader.SetGlobalVector(CrossRoadId,Vector4.zero);if(themes!=null)foreach(var t in themes)t.Dispose();
 }
}
}
