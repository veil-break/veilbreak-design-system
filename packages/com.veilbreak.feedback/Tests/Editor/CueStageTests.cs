using NUnit.Framework;
using UnityEngine;
namespace Veilbreak.Feedback.Tests {
public sealed class CueStageTests {
 static CueCatalog.Cue Cue(int priority,float shake=0,float pulse=0,float light=0)=>new CueCatalog.Cue{key="test",priority=priority,shake=shake,pulse=pulse,light=light,routine=EffectRoutine.Impact};

 // Without particles a piece below priority 2 is dropped whole, and priority 2 still raises its shake and pulse.
 [Test] public void DroppedPiecesKeepOnlyPriorityFeedback(){
  float shaken=0,pulsed=0;var stage=new CueStage{Shake=s=>shaken=s,Pulsed=p=>pulsed=p};var call=new FxCall{color=Color.red,magnitude=1};
  Assert.IsFalse(stage.Play(Cue(1,.4f,.5f),EffectRoutine.Impact,call,Vector3.zero));Assert.AreEqual(0,shaken);Assert.AreEqual(0,pulsed);
  Assert.IsFalse(stage.Play(Cue(2,.4f,.5f),EffectRoutine.Impact,call,Vector3.zero),"no emitters, no particles");
  Assert.AreEqual(.4f,shaken,1e-6f,"the host scales the shake");Assert.AreEqual(.5f,pulsed,1e-6f);
  Assert.IsFalse(stage.Play(null,EffectRoutine.Impact,call,Vector3.zero),"no cue and no emitters plays nothing");}

 // Reduced Flashes skips the light flash and nothing else.
 [Test] public void ReducedFlashesSkipTheLight(){
  var host=new GameObject("Flashes");try{var flashes=host.AddComponent<LightFlashes>();flashes.Initialize(2);var stage=new CueStage{Flashes=flashes,ReducedFlashes=true};
   var call=new FxCall{color=Color.white,magnitude=1};stage.Play(Cue(2,light:3),EffectRoutine.Impact,call,Vector3.zero);Assert.AreEqual(0,flashes.Lit,"no flash under Reduced Flashes");
   stage.ReducedFlashes=false;stage.Play(Cue(2,light:3),EffectRoutine.Impact,call,Vector3.zero);Assert.AreEqual(1,flashes.Lit,"the cue's flash otherwise");}
  finally{Object.DestroyImmediate(host);}}

 // The reticle is four chevrons and a ring, and a null batch is ignored.
 [Test] public void ReticleAddsFiveQuads(){
  var root=new GameObject("Batch");var material=new Material(Shader.Find("Veilbreak/Feedback/Particle"));
  try{var batch=new QuadBatch(root.transform,"Rings",material,8);batch.Begin();CueStage.Reticle(batch,Vector3.zero,1.2f,Color.yellow,3);Assert.AreEqual(5,batch.Count);batch.End();batch.Dispose();
   CueStage.Reticle(null,Vector3.zero,1,Color.white,0);}
  finally{Object.DestroyImmediate(root);Object.DestroyImmediate(material);}}
}
}
