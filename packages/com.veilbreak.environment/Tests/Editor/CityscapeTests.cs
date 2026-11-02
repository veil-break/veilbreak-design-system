using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
namespace Veilbreak.Cityscape.Tests {
public sealed class CityscapeTests {
 const string KitPath="Packages/com.veilbreak.environment/Runtime/CityKit.asset";
 /// <summary>Per-segment triangle budgets (plan T4.4): Standard and Low, which drops roof clutter, corner towers and upper-floor facade detail.</summary>
 const int SegmentBudget=25000,LowSegmentBudget=12000;
 static CityKit Kit(){var kit=AssetDatabase.LoadAssetAtPath<CityKit>(KitPath);Assert.IsNotNull(kit,"Run Veilbreak > Environment > Rebuild city kit");return kit;}
 // A calm theme and a ruined one, so the clearance and budget checks see every prop, wreck and fire path.
 static StreetTheme[] Themes(){var calm=ScriptableObject.CreateInstance<StreetTheme>();calm.name="calm";var ruined=ScriptableObject.CreateInstance<StreetTheme>();ruined.name="ruined";
  ruined.damage=1;ruined.wrecks=1;ruined.fires=1;ruined.gaps=.5f;ruined.trees=1;ruined.windowsFire=.2f;ruined.maxFloors=10;return new[]{calm,ruined};}

 [Test] public void KitHasEveryModelAndMaterial(){
  var kit=Kit();Assert.AreEqual(36,kit.models.Length);
  foreach(var m in kit.models){Assert.IsNotNull(m.mesh,m.name);Assert.AreEqual(m.mesh.subMeshCount,m.slots.Length,m.name);Assert.IsTrue(m.mesh.isReadable,m.name+" must stay readable for module building");
   Assert.IsTrue(m.slots.All(s=>s>=0&&s<CityKit.Palette.Length),m.name);Assert.AreEqual(m.mesh.vertexCount,m.mesh.colors32.Length,m.name+" carries node tags");}
  Assert.IsTrue(kit.Find("Facade").mesh.colors32.Any(c=>c.g==255),"Facade panes are tagged as windows");
  Assert.IsTrue(kit.Find("StreetLamp").mesh.colors32.All(c=>c.a==0),"Raw meshes emit nothing through the City shader");
  foreach(var m in kit.palette.Concat(new[]{kit.street,kit.sidewalk,kit.lightPool,kit.fog,kit.sky,kit.skyline,kit.ember,kit.smoke,kit.rain})){Assert.IsNotNull(m);Assert.IsTrue(m.shader.isSupported,m.name+" shader");}
  // Read the parsed SubShader: in EditMode no URP pipeline need be active, so FindPass sees only the fallback.
  var passes=ShaderUtil.GetShaderData(kit.Slot(CityKit.Glass).shader).GetSubshader(0);var names=Enumerable.Range(0,passes.PassCount).Select(i=>passes.GetPass(i).Name).ToArray();
  CollectionAssert.Contains(names,"VeilReflection","City draws its emission into the wet-street mirror");
  foreach(var slot in new[]{CityKit.Glass,CityKit.Violet,CityKit.Lamp,CityKit.Taillight,CityKit.Ember})Assert.AreEqual(kit.palette[CityKit.Glass].shader,kit.Slot(slot).shader,CityKit.Palette[slot]);
  Assert.IsNotNull(kit.noise);Assert.IsNotNull(kit.skyPlate);Assert.IsNotNull(kit.quad);Assert.IsNotNull(kit.dome);
 }

 [Test] public void SameSeedBuildsTheSameStreet(){
  var kit=Kit();var theme=Themes()[1];var a=new StreetModules(kit,theme,77);var b=new StreetModules(kit,theme,77);var c=new StreetModules(kit,theme,78);
  try{for(int i=0;i<a.sides.Length;i++){CollectionAssert.AreEqual(a.sides[i].mesh.vertices,b.sides[i].mesh.vertices,"variant "+i);CollectionAssert.AreEqual(a.sides[i].mesh.colors32,b.sides[i].mesh.colors32,"variant "+i+" windows");CollectionAssert.AreEqual(a.sides[i].emitters,b.sides[i].emitters);}
   Assert.IsTrue(Enumerable.Range(0,a.sides.Length).Any(i=>a.sides[i].mesh.vertexCount!=c.sides[i].mesh.vertexCount),"Another seed builds other blocks");}
  finally{a.Dispose();b.Dispose();c.Dispose();}
 }

 // The simulation has no environment collision: actors roam |x| <= 5.5 m, so nothing but the road surface, kerb and
 // ground light pools may sit inside |x| < 6 below head height. Lamp arms overhang above it.
 [Test] public void DressingKeepsTheCombatLaneClear(){
  var kit=Kit();foreach(var theme in Themes()){var set=new StreetModules(kit,theme,5);
   try{foreach(var side in set.sides.Concat(set.corners)){var v=side.mesh.vertices;for(int s=0;s<side.keys.Length;s++){if(side.keys[s]==CityKit.Pool)continue;
     foreach(var index in side.mesh.GetIndices(s)){var p=v[index];if(p.y<2.5f)Assert.GreaterOrEqual(p.x,StreetModules.RoadHalf-1e-3f,$"{side.name}: {CityKit.Palette[side.keys[s]]} vertex {p} enters the combat lane");}}
     foreach(var e in side.emitters)Assert.Greater(e.position.x,StreetModules.RoadHalf-1.5f,side.name+" emitter");}}
   finally{set.Dispose();}}
 }

 [Test] public void SegmentsStayWithinTheTriangleBudget([Values(true,false)] bool high){
  var kit=Kit();foreach(var theme in Themes()){var set=new StreetModules(kit,theme,9,8,high);
   try{int worst=set.sides.Max(s=>s.triangles)*2+set.road.triangles+set.sidewalks.triangles;Assert.LessOrEqual(worst,high?SegmentBudget:LowSegmentBudget,theme.name);
    foreach(var side in set.sides)Assert.LessOrEqual(side.materials.Length,13,side.name+" draw calls");TestContext.WriteLine($"{(high?"Standard":"Low")} {theme.name}: worst segment {worst} triangles");}
   finally{set.Dispose();}}
 }
 // Both tiers lay out the same street: Low only leaves pieces out, so its lamps, fires and signs match Standard.
 [Test] public void TiersShareTheLayout(){
  var kit=Kit();var theme=Themes()[1];var a=new StreetModules(kit,theme,9,8,true);var b=new StreetModules(kit,theme,9,8,false);
  try{for(int i=0;i<a.sides.Length;i++){CollectionAssert.AreEqual(a.sides[i].emitters.Select(e=>e.position),b.sides[i].emitters.Select(e=>e.position),a.sides[i].name);Assert.LessOrEqual(b.sides[i].triangles,a.sides[i].triangles);}}
  finally{a.Dispose();b.Dispose();}
 }

 // 2,000 m of street with two theme changes: the segment pool, the reflection set, the light budget and the particle
 // counts all stay at their bounds, on both tiers.
 [Test] public void StreamingStaysBounded([Values(true,false)] bool high){
  var kit=Kit();var host=new GameObject("Street test");
  try{var director=host.AddComponent<EnvironmentDirector>();director.Initialize(kit,Themes(),31,null,high);
   director.Present(0,0,0,1/30f,new Vector3(0,0,4),3,false); // segments create their four parts on first show
   int children=CountChildren(host.transform),reflections=0;
   for(double d=0;d<=2000;d+=.5){int theme=d<700?0:d<1400?1:0;director.Present(d,theme,(float)d,1/30f,new Vector3(0,0,4),3,false);
    Assert.AreEqual(SegmentStreamer.Capacity,director.Street.SegmentCount);foreach(var block in director.Street.GetComponentsInChildren<MeshRenderer>().Where(r=>r.name.EndsWith(" block")))Assert.AreEqual(high?UnityEngine.Rendering.ShadowCastingMode.On:UnityEngine.Rendering.ShadowCastingMode.Off,block.shadowCastingMode,"block shadows follow the tier");Assert.LessOrEqual(director.Lights.Lit,high?EnvironmentDirector.HighLights:EnvironmentDirector.LowLights);
    Assert.LessOrEqual(director.Air.Particles,high?Atmospherics.HighEmbers+Atmospherics.HighRain:Atmospherics.LowEmbers+Atmospherics.LowRain);
    reflections=Mathf.Max(reflections,ReflectionSet.Entries.Count);}
   Assert.AreEqual(children,CountChildren(host.transform),"Streaming instantiates nothing after set-up");
   Assert.LessOrEqual(reflections,SegmentStreamer.Capacity*2*5,"At most five emissive submeshes per block");
   Assert.Greater(director.Lights.Lit,0,"Lamps near the action get real lights");
  }finally{Object.DestroyImmediate(host);}
  Assert.AreEqual(0,ReflectionSet.Entries.Count(e=>e.renderer!=null),"Destroying the street unregisters its reflections");
 }
 static int CountChildren(Transform t)=>t.GetComponentsInChildren<Transform>(true).Length;

 // A crossroads: before a choice both arms show; the stream then continues the chosen arm in place with the same blocks, and once the
 // swing is done and the crossing is behind, the street re-bases with every segment where it was.
 [Test] public void CrossroadsTurnsOntoTheChosenArmSeamlessly([Values(-1,1)] int side){
  var kit=Kit();var host=new GameObject("Crossroads test");string near=side>0?"Right block":"Left block",far=side>0?"Left block":"Right block";
  try{var street=host.AddComponent<StreetStreamer>();street.Initialize(kit,Themes(),31,8,false);float length=SegmentStreamer.Length;double centre=40*length+length/2;
   street.Present(centre-60,0);street.Junction(centre,0,0);street.Present(centre,0);Assert.IsTrue(street.AtCrossroads);Assert.AreEqual(1,Shader.GetGlobalVector("_VeilCrossRoad").y);
   var arm=host.transform.Find("Crossroads/"+(side<0?"Left":"Right")+" arm 1");Assert.IsTrue(arm.gameObject.activeInHierarchy);Assert.IsFalse(arm.Find(near).gameObject.activeSelf,"The arm leaves its near corner to the incoming road's last block");
   Vector3 armAt=arm.position;var armTurn=arm.rotation;var armMesh=arm.Find(far).GetComponent<MeshFilter>().sharedMesh;
   street.Junction(centre,side,0);street.Present(centre,0);
   var match=host.GetComponentsInChildren<Transform>().Where(t=>t.name.StartsWith("Segment ")&&(t.position-armAt).sqrMagnitude<1e-6f).ToArray();
   Assert.AreEqual(1,match.Length,"The stream continues the chosen arm in place");Assert.Less(Quaternion.Angle(match[0].rotation,armTurn),.01f);
   Assert.AreSame(armMesh,match[0].Find(far).GetComponent<MeshFilter>().sharedMesh,"with the blocks the arm showed");Assert.IsFalse(match[0].Find(near).gameObject.activeSelf);Assert.IsFalse(arm.gameObject.activeInHierarchy,"and the arm itself steps aside");
   street.Junction(centre,side,1);double before=centre+StreetStreamer.RebaseAfter-.01,after=centre+StreetStreamer.RebaseAfter+.01;street.Present(before,0);Assert.IsTrue(street.AtCrossroads);
   var a=Shown(host,before);var road=street.Road(before+10,2);Assert.Less(Vector3.Distance(road.position,new Vector3(2,0,10)),1e-3f,"A finished swing leaves the chosen arm straight ahead");Assert.Less(Quaternion.Angle(road.rotation,Quaternion.identity),.01f);
   street.Junction(centre,side,1);street.Present(after,0);Assert.IsFalse(street.AtCrossroads,"The street re-bases once the crossing is behind");Assert.AreEqual(0,Shader.GetGlobalVector("_VeilCrossRoad").y);
   Assert.IsFalse(host.transform.Find("Crossroads").gameObject.activeSelf);var b=Shown(host,after);var shared=a.Keys.Intersect(b.Keys).ToArray();Assert.GreaterOrEqual(shared.Length,SegmentStreamer.Capacity-2,"Segments keep their places across the re-base");
   foreach(var key in shared){Assert.Less(Quaternion.Angle(a[key],Quaternion.identity),.01f);Assert.Less(Quaternion.Angle(b[key],Quaternion.identity),.01f);}
   Assert.IsTrue(host.GetComponentsInChildren<Transform>(true).Where(t=>t.name.StartsWith("Segment ")).All(t=>t.Find("Left block").gameObject.activeSelf&&t.Find("Right block").gameObject.activeSelf),"Re-basing restores every block");
   street.Junction(centre,side,1);street.Present(after+1,0);Assert.IsFalse(street.AtCrossroads,"A re-based crossroads stays released");
   street.Junction(-1,0,0);street.Junction(centre,0,0);street.Present(centre-20,0);Assert.IsTrue(street.AtCrossroads,"Clearing forgets a crossroads, so a new run can lay it out again");
  }finally{Object.DestroyImmediate(host);}
 }
 /// <summary>Active stream segments by road distance (cm), with their rotation; every one sits on the centre line.</summary>
 static System.Collections.Generic.Dictionary<long,Quaternion> Shown(GameObject host,double distance){var shown=new System.Collections.Generic.Dictionary<long,Quaternion>();
  foreach(var t in host.GetComponentsInChildren<Transform>().Where(t=>t.name.StartsWith("Segment "))){Assert.AreEqual(0,t.localPosition.x,1e-3f,t.name+" leaves the centre line");shown[(long)System.Math.Round((t.localPosition.z+distance)*100)]=t.localRotation;}return shown;}

 // When the swing completes on the crossing, the frontage, the arms and the incoming road all sit beside or behind the squad: nothing
 // but road and kerb is inside the combat lane where the battlefield is.
 [Test] public void CrossroadsKeepTheCombatLaneClear([Values(-1,1)] int side){
  var kit=Kit();var host=new GameObject("Crossroads lane test");
  try{var street=host.AddComponent<StreetStreamer>();street.Initialize(kit,Themes(),5,8,true);float length=SegmentStreamer.Length;double centre=25*length+length/2;
   street.Present(centre-40,1);street.Junction(centre,side,1);street.Present(centre,1);string[] ground={"Crossing","Pavement","Road","Sidewalks"};int checkedParts=0;
   foreach(var r in host.GetComponentsInChildren<MeshRenderer>()){if(ground.Contains(r.name))continue;var mesh=r.GetComponent<MeshFilter>().sharedMesh;var keys=mesh.subMeshCount;checkedParts++;
    var v=mesh.vertices;for(int s=0;s<keys;s++){if(r.sharedMaterials[s]==kit.lightPool)continue;foreach(var index in mesh.GetIndices(s)){var p=host.transform.InverseTransformPoint(r.transform.TransformPoint(v[index]));
     if(p.y>.2f&&p.y<2.5f&&p.z>-10&&p.z<16)Assert.GreaterOrEqual(Mathf.Abs(p.x),StreetModules.RoadHalf-1e-3f,$"{r.transform.parent.name}/{r.name} vertex {p} enters the combat lane");}}}
   Assert.Greater(checkedParts,12);
  }finally{Object.DestroyImmediate(host);}
 }

 // Three crossroads over 700 m reuse one crossroads set: the segment pool and the object count stay put.
 [Test] public void CrossroadsReuseOneSet(){
  var kit=Kit();var host=new GameObject("Crossroads reuse test");
  try{var street=host.AddComponent<StreetStreamer>();street.Initialize(kit,Themes(),9,8,false);float length=SegmentStreamer.Length;int children=-1;
   for(int n=0;n<3;n++){double centre=(20+30*n)*length+length/2;int side=n%2==0?1:-1;
    for(double d=centre-80;d<=centre+StreetStreamer.RebaseAfter+16;d+=.5){float turn=d<centre?0:Mathf.Clamp01((float)(d-centre)/4);street.Junction(centre,d<centre?0:side,turn);street.Present(d,0);
     Assert.AreEqual(SegmentStreamer.Capacity,street.SegmentCount);}
    Assert.IsFalse(street.AtCrossroads);if(children<0)children=CountChildren(host.transform);else Assert.AreEqual(children,CountChildren(host.transform),"crossroads "+n);}
  }finally{Object.DestroyImmediate(host);}
 }
 [Test] public void LongTravelReusesNineContinuousSegments(){var host=new GameObject("Stream test");try{var stream=host.AddComponent<SegmentStreamer>();int configurations=0;stream.Initialize(73,(segment,seed,theme)=>configurations++);for(int i=0;i<1000;i++)stream.Present(i*11.25,2);Assert.AreEqual(SegmentStreamer.Capacity,host.transform.childCount);var positions=host.GetComponentsInChildren<Transform>().Where(t=>t!=host.transform).Select(t=>t.localPosition.z).OrderBy(z=>z).ToArray();for(int i=1;i<positions.Length;i++)Assert.AreEqual(SegmentStreamer.Length,positions[i]-positions[i-1],.001f);Assert.Greater(configurations,1000);}finally{Object.DestroyImmediate(host);}}
 [Test] public void HostSeedFunctionDrivesSegmentContent(){var host=new GameObject("Seed test");try{var stream=host.AddComponent<SegmentStreamer>();var seen=new System.Collections.Generic.List<uint>();stream.Initialize(5,(segment,seed,theme)=>seen.Add(seed),(seed,theme,index)=>seed*100u+(uint)theme);stream.Present(0,3);Assert.AreEqual(SegmentStreamer.Capacity,seen.Count);Assert.IsTrue(seen.All(s=>s==503u),"run seed and theme reach the host hash");
   var first=SegmentStreamer.DefaultSeed(7,2,900);for(int i=0;i<10000;i++)SegmentStreamer.DefaultSeed(7,1,i);Assert.AreEqual(first,SegmentStreamer.DefaultSeed(7,2,900));Assert.AreNotEqual(first,SegmentStreamer.DefaultSeed(7,1,900));}finally{Object.DestroyImmediate(host);}}

 // ---- Per-index segment plan (environment 0.4.0) ----
 /// <summary>A pure plan: a hash of the index picks the seed; look 0 before `shift`, a one-segment transition at `shift`, look 1 after; counts every Resolve.</summary>
 sealed class TestPlan:ISegmentPlan {
  public readonly long shift;public readonly ushort family;public readonly System.Collections.Generic.Dictionary<long,int> calls=new System.Collections.Generic.Dictionary<long,int>();public int total;
  public TestPlan(long shift=long.MaxValue/4,ushort family=0){this.shift=shift;this.family=family;}
  public SegmentDescriptor Resolve(long index){total++;calls[index]=calls.TryGetValue(index,out var n)?n+1:1;
   return new SegmentDescriptor(index,SegmentStreamer.DefaultSeed(11,0,index),index>=shift+2?family:(ushort)0,(ushort)(index>=shift?1:0),index==shift?SegmentRole.Transition:SegmentRole.Straight,0,(byte)(index&3),(byte)(index&1));}
 }
 sealed class TestKit:IStreetKit {
  public ushort Family{get;}public readonly System.Collections.Generic.List<long> built=new System.Collections.Generic.List<long>(),released=new System.Collections.Generic.List<long>();public bool lastHigh;
  public TestKit(ushort family){Family=family;}
  public int TriangleBudget(bool high)=>high?20000+Family:9000+Family;
  public void Build(in SegmentDescriptor d,Transform slot,bool high){built.Add(d.Index);lastHigh=high;slot.gameObject.name="Kit segment "+d.Index;}
  public void Release(Transform slot){released.Add(long.Parse(slot.gameObject.name.Substring("Kit segment ".Length)));slot.gameObject.name="Segment";}
 }
 // Three looks that differ in fog density (a blended value), so a blend is measurable.
 static StreetTheme[] Looks(){var looks=new StreetTheme[3];for(int i=0;i<3;i++){looks[i]=ScriptableObject.CreateInstance<StreetTheme>();looks[i].name="look "+i;looks[i].fogDensity=.01f+.015f*i;}return looks;}
 const float L=SegmentStreamer.Length;

 [Test] public void WindowConstantsMatchTheCapacity(){
  Assert.AreEqual(2,SegmentStreamer.Behind);Assert.AreEqual(6,SegmentStreamer.Ahead);Assert.AreEqual(SegmentStreamer.Capacity,SegmentStreamer.Behind+1+SegmentStreamer.Ahead);
  var host=new GameObject("Window test");
  try{var stream=host.AddComponent<SegmentStreamer>();var plan=new TestPlan();stream.Initialize(1,null);stream.Plan=plan;stream.Apply=(go,d)=>{};
   Assert.AreEqual(long.MinValue,stream.FurthestAllocated);stream.Present(40*L+1);
   Assert.AreEqual(40+SegmentStreamer.Ahead,stream.FurthestAllocated,"Ahead segments are allocated past the one under the squad");
   CollectionAssert.AreEquivalent(Enumerable.Range(40-SegmentStreamer.Behind,SegmentStreamer.Capacity).Select(i=>(long)i),plan.calls.Keys,"and Behind segments behind it");
   stream.Present(35*L);Assert.AreEqual(40+SegmentStreamer.Ahead,stream.FurthestAllocated,"FurthestAllocated never decreases");
  }finally{Object.DestroyImmediate(host);}
 }

 [Test] public void PlanIsPureAndDeterministic(){
  var a=new TestPlan(20);var b=new TestPlan(20);var forward=Enumerable.Range(-5,60).Select(i=>a.Resolve(i)).ToArray();var backward=Enumerable.Range(-5,60).Reverse().Select(i=>b.Resolve(i)).Reverse().ToArray();
  CollectionAssert.AreEqual(forward,backward,"Resolve does not depend on the order it is asked in");Assert.AreEqual(a.Resolve(33),a.Resolve(33));
  Assert.AreNotEqual(a.Resolve(33).Seed,a.Resolve(34).Seed);Assert.AreEqual(SegmentRole.Transition,a.Resolve(20).Role);Assert.AreEqual(SegmentStreamer.DefaultSeed(11,0,33),a.Resolve(33).Seed);
 }

 [Test] public void ASlotIsResolvedOncePerAllocation(){
  var host=new GameObject("Resolve test");
  try{var stream=host.AddComponent<SegmentStreamer>();var plan=new TestPlan();int applied=0,retired=0;stream.Initialize(1,null);stream.Plan=plan;stream.Apply=(go,d)=>applied++;stream.Retire=(go,d)=>retired++;
   stream.Present(0);Assert.AreEqual(SegmentStreamer.Capacity,plan.total);
   foreach(var at in new[]{0.0,.5,L-.01,0,.25})stream.Present(at);Assert.AreEqual(SegmentStreamer.Capacity,plan.total,"Moving inside a segment resolves nothing");
   stream.Present(3.5*L);Assert.AreEqual(SegmentStreamer.Capacity+3,plan.total);Assert.AreEqual(SegmentStreamer.Capacity+3,applied);Assert.AreEqual(3,retired);
   for(double d=3.5*L;d<=50*L;d+=.7)stream.Present(d);
   foreach(var pair in plan.calls)Assert.AreEqual(1,pair.Value,"index "+pair.Key+" is resolved once on a forward run");
   stream.Present(50*L+3);var count=plan.total;for(int i=0;i<20;i++)stream.Present(50*L+3);Assert.AreEqual(count,plan.total);
  }finally{Object.DestroyImmediate(host);}
 }

 [Test] public void AnAllocatedVisibleSlotIsNeverReDressed(){
  var kit=Kit();var host=new GameObject("Slot stability test");
  try{var street=host.AddComponent<StreetStreamer>();street.Initialize(kit,Looks(),31,8,false);var plan=new TestPlan(12);street.Plan=plan;
   var applied=new System.Collections.Generic.Dictionary<long,int>();var inner=street.Stream.Apply;street.Stream.Apply=(go,d)=>{applied[d.Index]=applied.TryGetValue(d.Index,out var n)?n+1:1;inner(go,d);};
   var seen=new System.Collections.Generic.Dictionary<long,Mesh>();
   for(double d=0;d<=40*L;d+=1.3){street.Present(d);
    foreach(var t in host.GetComponentsInChildren<Transform>().Where(t=>t.name.StartsWith("Segment "))){long index=(long)System.Math.Round((t.localPosition.z+d)/L);if(!t.gameObject.activeSelf||!street.Stream.TryDescriptor(index,out _))continue;
     var mesh=t.Find("Left block").GetComponent<MeshFilter>().sharedMesh;if(seen.TryGetValue(index,out var before))Assert.AreSame(before,mesh,"segment "+index+" kept its block while visible");else seen[index]=mesh;}}
   foreach(var pair in applied)Assert.AreEqual(1,pair.Value,"segment "+pair.Key+" is dressed once on a forward run");Assert.Greater(applied.Count,40);
   Assert.AreEqual(SegmentStreamer.Capacity,street.SegmentCount);
  }finally{Object.DestroyImmediate(host);}
 }

 [Test] public void TransitionBlendFollowsTheRoadNotATimer(){
  var kit=Kit();var host=new GameObject("Transition test");
  try{var director=host.AddComponent<EnvironmentDirector>();director.Initialize(kit,Looks(),31,null,false,0,null,new TestPlan(20));var focus=new Vector3(0,0,4);
   float calm=.01f,next=.025f;
   director.Present(20*L-1,0,0,focus,0,false);Assert.AreEqual(calm,director.Current.fogDensity,1e-5f);Assert.IsFalse(director.Blending);
   director.Present(20*L+.5f*L,0,0,focus,0,false);Assert.AreEqual((calm+next)/2,director.Current.fogDensity,1e-5f,"Halfway through the segment is halfway through the blend");Assert.AreEqual(.5f,director.BlendProgress,1e-4f);Assert.IsTrue(director.Blending);
   var mid=director.Current;for(int i=0;i<10;i++)director.Present(20*L+.5f*L,i,10,focus,0,false);
   Assert.AreEqual(mid.fogDensity,director.Current.fogDensity,"Time and delta do not move a plan blend: only distance does");
   director.Present(20*L+.25f*L,0,0,focus,0,false);var quarter=director.Current.fogDensity;director.Present(20*L+.75f*L,0,0,focus,0,false);Assert.Less(quarter,director.Current.fogDensity);
   director.Present(20*L+.25f*L,0,0,focus,0,false);Assert.AreEqual(quarter,director.Current.fogDensity,"The blend is a function of distance, also backwards");
   director.Present(21*L+.1f,0,0,focus,0,false);Assert.AreEqual(next,director.Current.fogDensity,1e-5f);Assert.IsFalse(director.Blending);Assert.AreEqual(1,director.Theme);
  }finally{Object.DestroyImmediate(host);}
 }

 [Test] public void ZeroDeltaFreezesLightsAirAndFires(){
  var kit=Kit();var host=new GameObject("Pause test");FireLight.ResetClock();
  try{var director=host.AddComponent<EnvironmentDirector>();director.Initialize(kit,Looks(),31,null,true,0,null,new TestPlan());var focus=new Vector3(0,0,4);
   for(int i=0;i<30;i++)director.Present(i*.5,i/30f,1/30f,focus,3,false);
   float clock=FireLight.Clock;Assert.Greater(clock,0);int lit=director.Lights.Lit,particles=director.Air.Particles;var look=director.Current;
   for(int i=0;i<30;i++)director.Present(14.5,5+i,0,focus,3,false);
   Assert.AreEqual(clock,FireLight.Clock,"Fires flicker on the host's delta, so a zero delta holds them");Assert.AreEqual(lit,director.Lights.Lit);Assert.AreEqual(particles,director.Air.Particles);
   Assert.AreEqual(look.fogDensity,director.Current.fogDensity);
   director.Present(14.5,40,-1,focus,3,false);Assert.AreEqual(clock,FireLight.Clock,"A negative delta counts as zero");
  }finally{Object.DestroyImmediate(host);FireLight.ResetClock();}
 }

 [Test] public void FireLightFollowsTheHostClock(){
  var go=new GameObject("Fire");FireLight.ResetClock();FireLight.Still=false;
  try{var light=go.AddComponent<Light>();light.intensity=3;var fire=go.AddComponent<FireLight>();
   FireLight.Advance(.37f);fire.Step();float a=light.intensity;for(int i=0;i<5;i++){FireLight.Advance(0);fire.Step();Assert.AreEqual(a,light.intensity,"A paused host holds the flicker");}
   bool moved=false;for(int i=0;i<12;i++){FireLight.Advance(.21f);fire.Step();moved|=!Mathf.Approximately(a,light.intensity);}Assert.IsTrue(moved,"It flickers while the host's time runs");
   FireLight.Still=true;fire.Step();Assert.AreEqual(3,light.intensity,"Still holds the authored intensity");
  }finally{FireLight.Still=false;FireLight.ResetClock();Object.DestroyImmediate(go);}
 }

 [Test] public void ReducedMotionIsExposedAndStillsFires(){
  var kit=Kit();var host=new GameObject("Reduced motion test");
  try{var director=host.AddComponent<EnvironmentDirector>();director.Initialize(kit,Looks(),31,null,true,0,null,new TestPlan());Assert.IsFalse(director.ReducedMotion);
   director.Present(0,0,1/30f,Vector3.zero,3,true);Assert.IsTrue(director.ReducedMotion);director.Present(1,0,1/30f,Vector3.zero,3,false);Assert.IsFalse(director.ReducedMotion);
   director.Present(2,0,0,1/30f,Vector3.zero,3,true);Assert.IsTrue(director.ReducedMotion,"also on the legacy overload");
   var budget=director.Lights;budget.Present(director.Street,Vector3.zero,1,0,1/30f,true);Assert.AreEqual(8,budget.Capacity);
  }finally{Object.DestroyImmediate(host);}
 }

 [Test] public void KitRegistryReportsBudgetsAndRejectsFamilyZero(){
  StreetKitRegistry.Clear();
  try{Assert.AreEqual(-1,StreetKitRegistry.Budget(4,true));Assert.Throws<System.ArgumentException>(()=>StreetKitRegistry.Register(new TestKit(0)));Assert.Throws<System.ArgumentNullException>(()=>StreetKitRegistry.Register(null));
   StreetKitRegistry.Register(new TestKit(4));StreetKitRegistry.Register(new TestKit(5));Assert.AreEqual(2,StreetKitRegistry.Count);
   Assert.AreEqual(20004,StreetKitRegistry.Budget(4,true));Assert.AreEqual(9004,StreetKitRegistry.Budget(4,false));Assert.AreEqual(-1,StreetKitRegistry.Budget(6,true));
   Assert.LessOrEqual(StreetKitRegistry.Budget(4,true),SegmentBudget);Assert.LessOrEqual(StreetKitRegistry.Budget(4,false),LowSegmentBudget);
   var replacement=new TestKit(4);StreetKitRegistry.Register(replacement);Assert.AreSame(replacement,StreetKitRegistry.Find(4));Assert.AreEqual(2,StreetKitRegistry.Count);
   Assert.IsNull(StreetKitRegistry.Find(0));Assert.IsTrue(StreetKitRegistry.Unregister(4));Assert.IsNull(StreetKitRegistry.Find(4));
  }finally{StreetKitRegistry.Clear();}
 }

 [Test] public void RegisteredKitsBuildAndReleaseTheirSlots([Values(true,false)] bool high){
  var kit=Kit();var host=new GameObject("Kit test");var kit3=new TestKit(3);StreetKitRegistry.Clear();StreetKitRegistry.Register(kit3);
  try{var street=host.AddComponent<StreetStreamer>();street.Initialize(kit,Looks(),31,8,high);street.Plan=new TestPlan(10,3); // index 12 on is family 3
   street.Present(5*L);Assert.AreEqual(0,kit3.built.Count,"no slot of the window is family 3 yet");
   street.Present(12*L);CollectionAssert.AreEqual(new long[]{12,13,14,15,16,17,18},kit3.built,"the kit builds each slot of its family once");
   Assert.AreEqual(high,kit3.lastHigh);var built=kit3.built.ToArray();
   street.Present(12*L+3);CollectionAssert.AreEqual(built,kit3.built,"nothing is built again inside a segment");
   var kitSegment=host.GetComponentsInChildren<Transform>().First(t=>t.name=="Kit segment 12");Assert.IsFalse(kitSegment.Find("Left block").gameObject.activeSelf,"The kit owns the slot: built-in blocks are hidden");
   street.Present(60*L);CollectionAssert.AreEquivalent(new long[]{12,13,14,15,16,17,18},kit3.released,"a recycled slot is released to its kit");
   Assert.AreEqual(StreetKitRegistry.Budget(3,high),Enumerable.Range(0,SegmentStreamer.Capacity).Max(i=>street.Triangles(i)),"Triangles reports the kit's budget for its slots");
  }finally{Object.DestroyImmediate(host);StreetKitRegistry.Clear();}
 }

 [Test] public void LegacyPresentIsUnchanged(){
  Assert.AreEqual(324183268u,SegmentStreamer.DefaultSeed(7,2,900),"The default hash is part of every game's golden seeds");Assert.AreEqual(3329153427u,SegmentStreamer.DefaultSeed(5,3,-2));
  var host=new GameObject("Legacy test");
  try{var stream=host.AddComponent<SegmentStreamer>();var seen=new System.Collections.Generic.List<(uint seed,int theme)>();stream.Initialize(5,(go,seed,theme)=>seen.Add((seed,theme)));
   Assert.IsNull(stream.Plan);stream.Present(0,3);CollectionAssert.AreEqual(Enumerable.Range(-2,SegmentStreamer.Capacity).Select(i=>(SegmentStreamer.DefaultSeed(5,3,i),3)).ToArray(),seen.ToArray(),"index -2..6, in order, under the global theme");
   stream.Present(12.5);Assert.AreEqual(SegmentStreamer.Capacity,seen.Count,"plan-mode Present without a plan does nothing");
   Assert.AreEqual(-2+SegmentStreamer.Capacity-1,stream.FurthestAllocated);
   seen.Clear();stream.Present(L*2,3);CollectionAssert.AreEqual(new[]{SegmentStreamer.DefaultSeed(5,3,7),SegmentStreamer.DefaultSeed(5,3,8)},seen.Select(x=>x.seed).ToArray());
  }finally{Object.DestroyImmediate(host);}
 }
 [Test] public void LegacyDirectorStillBlendsOnTheTimer(){
  var kit=Kit();var host=new GameObject("Legacy director test");
  try{var director=host.AddComponent<EnvironmentDirector>();var looks=Looks();director.Initialize(kit,looks,31,null,false);Assert.IsNull(director.Plan);var focus=new Vector3(0,0,4);
   director.Present(0,1,0,0,focus,0,false);Assert.AreEqual(.01f,director.Current.fogDensity,1e-5f,"delta 0 holds the blend");
   director.Present(0,1,0,EnvironmentDirector.BlendSeconds/2,focus,0,false);Assert.AreEqual(.5f,director.BlendProgress,1e-4f);Assert.IsTrue(director.Blending);
   director.Present(0,1,0,EnvironmentDirector.BlendSeconds,focus,0,false);Assert.AreEqual(.025f,director.Current.fogDensity,1e-5f);Assert.IsFalse(director.Blending);
  }finally{Object.DestroyImmediate(host);}
 }
}
}
