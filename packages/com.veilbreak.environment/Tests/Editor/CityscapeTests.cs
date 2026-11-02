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
}
}
