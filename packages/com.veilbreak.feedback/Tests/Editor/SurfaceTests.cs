using NUnit.Framework;
using UnityEngine;
namespace Veilbreak.Feedback.Tests {
// The battle-surface helpers main's battle VFX runs on: pooled decals, outline curtains, projected plan lines, one-shots.
public sealed class SurfaceTests {
 static Material Any()=>new Material(Shader.Find("Hidden/InternalErrorShader"));

 [Test] public void DecalsEvictTheOldestAndFadeOverTheLastThird(){
  var host=new GameObject("Decals");var material=Any();
  try{float now=0;var pool=host.AddComponent<DecalPool>();pool.Clock=()=>now;pool.Initialize(2);Assert.AreEqual(2,pool.Capacity);
   pool.Place(material,Vector3.zero,1,10);now=1;pool.Place(material,Vector3.one,1,10);now=2;pool.Place(material,Vector3.right,1,10);
   Assert.AreEqual(2,pool.Live,"the cap evicts the oldest");
   pool.Place(null,Vector3.zero,1,10);pool.Place(material,Vector3.zero,0,10);Assert.AreEqual(2,pool.Live,"no material or size places nothing");
   Assert.AreEqual(1,DecalPool.Fade(0));Assert.AreEqual(1,DecalPool.Fade(.5f));Assert.Less(DecalPool.Fade(.9f),1);Assert.AreEqual(0,DecalPool.Fade(1),1e-4f);
   pool.Tick(11.5f);Assert.AreEqual(1,pool.Live,"the decal placed at 1 s expires after 10 s");pool.Tick(100);Assert.AreEqual(0,pool.Live);
   pool.Place(material,Vector3.zero,1,10);pool.Clear();Assert.AreEqual(0,pool.Live);}
  finally{Object.DestroyImmediate(host);Object.DestroyImmediate(material);}
 }

 [Test] public void CurtainsBuildOneStripPerEdge(){
  var host=new GameObject("Curtains");var material=Any();
  try{var curtains=new CurtainBatch(host.transform,material);var square=new[]{Vector3.zero,Vector3.right,new Vector3(1,0,1),Vector3.forward};
   curtains.Begin();curtains.Add(square,4,true,2,Color.white);Assert.AreEqual(4,curtains.Strips,"a closed loop has one strip per side");
   curtains.Add(square,3,false,2,Color.white);Assert.AreEqual(6,curtains.Strips,"an open polyline has one fewer");
   curtains.Add(square,4,true,0,Color.white);curtains.Add(square,4,true,2,Color.black);curtains.Add(square,1,false,2,Color.white);
   Assert.AreEqual(6,curtains.Strips,"no height, no light or a single point adds nothing");
   curtains.End();Assert.AreEqual(24,curtains.Mesh.vertexCount);Assert.Greater(curtains.Mesh.bounds.extents.y,1000,"world-space bounds never cull");
   var baked=curtains.Bake("baked");try{Assert.AreEqual(2,baked.bounds.size.y,1e-4f,"a baked copy has tight bounds");}finally{Object.DestroyImmediate(baked);}
   curtains.Begin();curtains.End();Assert.IsFalse(host.GetComponentInChildren<MeshRenderer>().enabled,"an empty batch draws nothing");}
  finally{Object.DestroyImmediate(host);Object.DestroyImmediate(material);}
 }

 [Test] public void PlanLinesStayWithinTheirCapAndPickTheirTint(){
  Assert.AreEqual(0,SurfaceIndicators.Slot(new Color(.2f,1,.5f)),"green");Assert.AreEqual(1,SurfaceIndicators.Slot(new Color(1,.3f,.5f)),"red");
  Assert.AreEqual(2,SurfaceIndicators.Slot(new Color(.3f,.4f,1)),"blue");Assert.AreEqual(3,SurfaceIndicators.Slot(Color.white),"neutral");
  var host=new GameObject("Indicators");var template=Any();var lines=new SurfaceIndicators(host.transform,template,3){Top=50,Bottom=-10};
  try{Assert.IsTrue(lines.Available);var square=new[]{Vector3.zero,Vector3.right,new Vector3(1,0,1),Vector3.forward};
   lines.Begin();lines.Draw(square,.1f,true,Color.white);Assert.AreEqual(3,lines.Used,"capped at three segments");lines.End();
   lines.Begin();lines.Draw(new[]{Vector3.zero,Vector3.right},.1f,false,Color.green);lines.End();Assert.AreEqual(1,lines.Used);
   var projectors=host.GetComponentsInChildren<UnityEngine.Rendering.Universal.DecalProjector>(true);Assert.AreEqual(3,projectors.Length,"projectors are reused");
   Assert.AreEqual(1,System.Array.FindAll(projectors,p=>p.enabled).Length,"unused projectors are switched off");
   Assert.AreEqual(60,projectors[0].size.z,1e-4f,"projects through Top to Bottom");
   Assert.IsFalse(new SurfaceIndicators(host.transform,null).Available,"no template, no lines");}
  finally{lines.Dispose();Object.DestroyImmediate(host);Object.DestroyImmediate(template);}
 }

 [Test] public void OneShotsBecomeNonLoopingCallbackEffects(){
  var prefab=new GameObject("Burst");var ps=prefab.AddComponent<ParticleSystem>();var main=ps.main;main.loop=true;main.playOnAwake=true;
  var host=new GameObject("Pool");PooledEffect released=null;
  try{var fx=PooledEffect.Create(prefab,host.transform,x=>released=x);var made=fx.GetComponent<ParticleSystem>().main;
   Assert.IsFalse(made.loop);Assert.IsFalse(made.playOnAwake);Assert.AreEqual(ParticleSystemStopAction.Callback,made.stopAction);
   Assert.IsFalse(fx.gameObject.activeSelf,"created inactive for the pool");Assert.AreSame(host.transform,fx.transform.parent);Assert.IsNull(released);}
  finally{Object.DestroyImmediate(host);Object.DestroyImmediate(prefab);}
 }
}
}
