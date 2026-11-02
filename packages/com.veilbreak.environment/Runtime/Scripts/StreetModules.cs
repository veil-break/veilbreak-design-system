using System;
using System.Collections.Generic;
using UnityEngine;
namespace Veilbreak.Cityscape {
public enum EmitterKind { Lamp, Fire, Sign }
/// <summary>A light source in a module's local space: lamp heads, wreck fires, kiosk signs.</summary>
[Serializable] public struct Emitter { public Vector3 position; public EmitterKind kind; public Color color; public float intensity,range; }

/// <summary>Builds one theme's street modules from the city kit: the road, the sidewalks and `variants` side blocks.
/// A side block is laid out for the right-hand side of an 8 m segment (local z -4..4, road toward -x) and is turned 180
/// degrees for the left. Everything off the road stays beyond |x| = 6, the simulation's lateral bound (550 cm) plus an
/// actor's radius, so dressing never sits in the combat lane. Built once per theme from a seed, so a resumed run
/// streams the same blocks.</summary>
public sealed class StreetModules {
 public const float Length=8,RoadHalf=6,Curb=.15f,Building=9.5f;
 public sealed class Module { public string name; public Mesh mesh; public Material[] materials; public int[] keys,emissive; public Emitter[] emitters=Array.Empty<Emitter>(); public int triangles; public bool gap; }
 public readonly Module road,sidewalks;public readonly Module[] sides;
 /// <summary>A T crossroads in its own frame: the corner at the origin, the incoming road along -z, the arms along ±x and the frontage
 /// closing the T at +z. `crossing` is the road out to the arms' first segments at |x| = 8, `pavement` the two near kerb corners and
 /// the pavement under the frontage, `skyline` the taller blocks behind it.</summary>
 public readonly Module crossing,pavement,skyline;
 /// <summary>Built blocks with nothing behind them (no tower, never an empty lot), for the segment that meets a crossroads: a normal
 /// block's tower or lot backdrop reaches past its 8 m and would stand in the arm.</summary>
 public readonly Module[] corners;
 readonly CityKit kit;readonly StreetTheme theme;readonly bool high;

 /// <param name="high">Standard tier. Low leaves out roof clutter and the corner towers behind the blocks and uses the plain
 /// facade above the second floor; every roll is still taken, so both tiers lay out the same street from a seed.</param>
 public StreetModules(CityKit kit,StreetTheme theme,uint seed,int variants=8,bool high=true){
  this.kit=kit;this.theme=theme;this.high=high;
  var b=new MeshBuilder();b.Face(0,new Vector3(-RoadHalf,0,-Length/2),new Vector3(RoadHalf,0,-Length/2),new Vector3(RoadHalf,0,Length/2),new Vector3(-RoadHalf,0,Length/2),Vector3.up);
  road=Finish(b,"Road",kit.street);
  b=new MeshBuilder();foreach(var side in new[]{1f,-1f}){
   float x0=side*RoadHalf,x1=side*36;
   b.Face(0,new Vector3(x0,Curb,-Length/2),new Vector3(x1,Curb,-Length/2),new Vector3(x1,Curb,Length/2),new Vector3(x0,Curb,Length/2),Vector3.up);
   b.Face(0,new Vector3(x0,0,-Length/2),new Vector3(x0,Curb,-Length/2),new Vector3(x0,Curb,Length/2),new Vector3(x0,0,Length/2),new Vector3(-side,0,0));}
  sidewalks=Finish(b,"Sidewalks",kit.sidewalk);
  sides=new Module[variants];for(int i=0;i<variants;i++)sides[i]=Side(new System.Random(unchecked((int)(seed*2654435761u)^(i*7919))),theme.name+" side "+i);
  const float J=Length;var up=Vector3.up;
  b=new MeshBuilder();b.Face(0,new Vector3(-J,0,-RoadHalf),new Vector3(J,0,-RoadHalf),new Vector3(J,0,RoadHalf),new Vector3(-J,0,RoadHalf),up);
  b.Face(0,new Vector3(-RoadHalf,0,-J),new Vector3(RoadHalf,0,-J),new Vector3(RoadHalf,0,-RoadHalf),new Vector3(-RoadHalf,0,-RoadHalf),up);
  crossing=Finish(b,"Crossroads",kit.street);
  b=new MeshBuilder();foreach(var side in new[]{1f,-1f}){float x0=side*RoadHalf,x1=side*J;
   b.Face(0,new Vector3(x0,Curb,-J),new Vector3(x1,Curb,-J),new Vector3(x1,Curb,-RoadHalf),new Vector3(x0,Curb,-RoadHalf),up);
   b.Face(0,new Vector3(x0,0,-J),new Vector3(x0,Curb,-J),new Vector3(x0,Curb,-RoadHalf),new Vector3(x0,0,-RoadHalf),new Vector3(-side,0,0));
   b.Face(0,new Vector3(x0,0,-RoadHalf),new Vector3(x0,Curb,-RoadHalf),new Vector3(x1,Curb,-RoadHalf),new Vector3(x1,0,-RoadHalf),Vector3.forward);}
  b.Face(0,new Vector3(-J,Curb,RoadHalf),new Vector3(J,Curb,RoadHalf),new Vector3(J,Curb,36),new Vector3(-J,Curb,36),up);
  b.Face(0,new Vector3(-J,0,RoadHalf),new Vector3(J,0,RoadHalf),new Vector3(J,Curb,RoadHalf),new Vector3(-J,Curb,RoadHalf),Vector3.back);
  pavement=Finish(b,"Crossroads pavement",kit.sidewalk);
  corners=new Module[Math.Max(2,variants/2)];for(int i=0;i<corners.Length;i++)corners[i]=Side(new System.Random(unchecked((int)(seed*2654435761u)^(i*7919)^0x3c0)),theme.name+" corner "+i,true);
  skyline=Skyline(new System.Random(unchecked((int)(seed*2246822519u)^0x7a11)),theme.name+" crossroads skyline");
  MeshBuilder.Release();
 }

 Module Finish(MeshBuilder b,string name,Material material,List<Emitter> emitters=null){
  var mesh=b.Build(name,out var keys);var m=new Module{name=name,mesh=mesh,keys=keys,triangles=b.Triangles,emitters=emitters?.ToArray()??Array.Empty<Emitter>()};
  m.materials=new Material[keys.Length];var lit=new List<int>();for(int i=0;i<keys.Length;i++){m.materials[i]=material??kit.Slot(keys[i]);if(material==null&&CityKit.Emissive(keys[i]))lit.Add(i);}
  m.emissive=lit.ToArray();return m;
 }

 Module Side(System.Random r,string name,bool compact=false){
  var b=new MeshBuilder();var emitters=new List<Emitter>();
  float Roll()=>(float)r.NextDouble();
  T Pick<T>(params T[] items)=>items[r.Next(items.Length)];
  string State()=>Roll()<theme.damage?Pick("Broken","Rubble","Cracked"):Roll()<.5f?"Cracked":"Intact";
  Matrix4x4 At(Vector3 p,float yaw,Vector3 scale)=>Matrix4x4.TRS(p,Quaternion.Euler(0,yaw,0),scale);
  void Put(string model,Matrix4x4 m,Func<Color32,Color32> tint=null){var k=kit.Find(model);if(k?.mesh!=null)b.Add(k.mesh,k.slots,m,tint);}
  Vector3 Centre(string model,int slot,Matrix4x4 m){var k=kit.Find(model);if(k?.mesh==null)return m.MultiplyPoint3x4(Vector3.zero);for(int s=0;s<k.slots.Length;s++)if(k.slots[s]==slot)return m.MultiplyPoint3x4(k.mesh.GetSubMesh(s).bounds.center);return m.MultiplyPoint3x4(Vector3.zero);}
  // Windows: each pane (tagged by GlbImport) rolls dark, warm, dim, violet or burning for this block.
  uint block=(uint)r.Next();
  Color32 Window(Color32 tag)=>Pane(tag,block);

  bool gap=Roll()<theme.gaps&&!compact;
  if(!gap){
   int floors=r.Next(theme.minFloors,theme.maxFloors+1);float height=floors*3;
   for(int f=0;f<floors;f++){var model=f==0?(Roll()<.6f?"FacadeEntrance":"Facade"):Roll()<theme.damage*.6f?"FacadeDamaged":Roll()<.35f?"FacadeBalcony":"Facade";if(!high&&f>=2)model="Facade";
    Put(model,At(new Vector3(Building,f*3,0),-90,new Vector3(Length/6,model=="FacadeEntrance"?3/3.4f:1,1)),Window);}
   b.Box(CityKit.Concrete,new Vector3(Building+4.2f,height/2,0),new Vector3(8,height,Length));
   // Roof: parapet, plant boxes and a mast, so the blocks read from the high camera.
   b.Box(CityKit.Concrete,new Vector3(Building+.35f,height+.3f,0),new Vector3(.3f,.6f,Length));b.Box(CityKit.Concrete,new Vector3(Building+8.05f,height+.3f,0),new Vector3(.3f,.6f,Length));
   for(int i=r.Next(1,4);i>0;i--){int slot=Roll()<.6f?CityKit.Metal:CityKit.Concrete;var p=new Vector3(Building+2+Roll()*5,height+.5f,Roll()*5-2.5f);var size=new Vector3(.8f+Roll()*1.6f,1+Roll()*.8f,.8f+Roll()*1.4f);int yaw=r.Next(4)*90;if(high)b.Box(slot,p,size,yaw);}
   if(Roll()<.3f){b.Box(CityKit.Metal,new Vector3(Building+5,height+3,1),new Vector3(.08f,6,.08f));b.Box(CityKit.Taillight,new Vector3(Building+5,height+6.1f,1),Vector3.one*.22f);} // mast and aviation light
  }else{
   for(int i=-1;i<=1;i+=2)Put("Fence4m",At(new Vector3(Building+.2f,Curb,i*2),90,Vector3.one));
   Put(Pick("Dumpster","DebrisPile"),At(new Vector3(Building+2,Curb,Roll()*4-2),r.Next(360),Vector3.one));
   if(Roll()<theme.wrecks){var bus=At(new Vector3(Building+3.4f,Curb,0),95+Roll()*10,Vector3.one);Put("Bus",bus);}
   var back=Pick("DistantBuilding12m","DistantBuilding18m");Put(back,At(new Vector3(Building+9,0,0),r.Next(4)*90,Vector3.one),Window);
  }
  // Behind the block, a taller tower now and then fills the corners of the frame.
  if(Roll()<.45f){var tower=Pick("DistantBuilding18m","DistantBuilding24m");var m=At(new Vector3(Building+15+Roll()*3,0,Roll()*2-1),r.Next(4)*90,Vector3.one);if(high&&!compact)Put(tower,m,Window);}

  // Sidewalk: a lamp at z = -2 with its light pool on the road edge.
  bool twin=Roll()<.2f;var lampModel=twin?"StreetLampDouble":"StreetLamp";var lamp=At(new Vector3(RoadHalf+.55f,Curb,-2),0,Vector3.one);Put(lampModel,lamp);
  var head=Centre(lampModel,CityKit.Lamp,lamp)+Vector3.down*.3f;
  emitters.Add(new Emitter{position=head,kind=EmitterKind.Lamp,color=theme.lamp,intensity=theme.lampIntensity,range=theme.lampRange});
  b.Ground(CityKit.Pool,new Vector3(head.x,.02f,head.z),new Vector2(5.5f,5.5f),new Color32(255,255,255,255));
  // Props sit in three slots beyond the lamp; a wreck pushed onto the kerb takes the middle and far slots and the tree.
  bool wreck=Roll()<theme.wrecks;
  if(wreck){var model=Pick("CarWreck","VehicleBarricade"+State());var m=At(new Vector3(RoadHalf+1.9f,Curb,1.2f),90+(Roll()-.5f)*12,Vector3.one);Put(model,m);
   if(model=="CarWreck"&&Roll()<theme.fires)emitters.Add(new Emitter{position=Centre(model,CityKit.Ember,m)+Vector3.up*.3f,kind=EmitterKind.Fire,color=theme.fire,intensity=3.5f,range=7});}
  else{
   if(Roll()<theme.trees)Put("TreeBare",At(new Vector3(RoadHalf+2.3f,Curb,2),r.Next(360),Vector3.one*(.9f+Roll()*.35f)));
   if(Roll()<.5f)Put(Pick("Bench","PlanterIntact","Planter"+Pick("Cracked","Broken","Rubble")),At(new Vector3(Building-1.4f,Curb,.3f),-90,Vector3.one));
   if(Roll()<.45f){var model="SignalKiosk"+State();var m=At(new Vector3(Building-1.2f,Curb,3.3f),r.Next(4)*90,Vector3.one);Put(model,m);
    if(!model.EndsWith("Rubble"))emitters.Add(new Emitter{position=Centre(model,CityKit.Violet,m),kind=EmitterKind.Sign,color=theme.windowViolet,intensity=1.4f,range=3.5f});}}
  if(Roll()<.35f)Put("UtilityBarricade"+State(),At(new Vector3(Building-1.4f,Curb,-3.2f),r.Next(2)*180,Vector3.one));
  var module=Finish(b,name,null,emitters);module.gap=gap;return module;
 }
 Color32 Pane(Color32 tag,uint block){if(tag.g<128)return new Color32(0,0,0,0);float x=((tag.r*2654435761u^block)*2246822519u>>8)/16777216f;
  if(x<theme.windowsFire)return Light(theme.windowFire,6,true);x-=theme.windowsFire;float lit=theme.windowsLit;
  return x<lit*.55f?Light(theme.windowWarm,2.4f,false):x<lit*.85f?Light(theme.windowDim,.8f,false):x<lit?Light(theme.windowViolet,1.8f,false):new Color32(0,0,0,0);}
 /// <summary>Tall blocks behind a crossroads' frontage, so the view past the T never shows the void where the straight road would go.</summary>
 Module Skyline(System.Random r,string name){
  var b=new MeshBuilder();uint block=(uint)r.Next();
  foreach(var x in new[]{-11f,0f,11f}){var model=r.Next(3)==0?"DistantBuilding18m":"DistantBuilding24m";var k=kit.Find(model);if(k?.mesh!=null)b.Add(k.mesh,k.slots,Matrix4x4.TRS(new Vector3(x,0,21+(float)r.NextDouble()*4),Quaternion.Euler(0,r.Next(4)*90,0),Vector3.one),tag=>Pane(tag,block));}
  return Finish(b,name,null);
 }

 static Color32 Light(Color hue,float level,bool fire){var a=fire?.5f+level/16:level/16;return new Color32((byte)(hue.r*255),(byte)(hue.g*255),(byte)(hue.b*255),(byte)Mathf.Clamp(Mathf.RoundToInt(a*255),0,255));}

 /// <summary>Destroys the generated meshes.</summary>
 public void Dispose(){void D(Module m){if(m?.mesh==null)return;if(Application.isPlaying)UnityEngine.Object.Destroy(m.mesh);else UnityEngine.Object.DestroyImmediate(m.mesh);}D(road);D(sidewalks);D(crossing);D(pavement);D(skyline);foreach(var m in sides)D(m);foreach(var m in corners)D(m);}
}
}
