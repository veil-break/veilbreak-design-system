using NUnit.Framework;
using UnityEditor;
using UnityEngine;
namespace Veilbreak.Feedback.Tests {
public sealed class EffectTests {
 const string Root="Packages/com.veilbreak.feedback/Runtime/";
 static Material Particle(bool additive){var m=new Material(Shader.Find("Veilbreak/Feedback/Particle"));m.SetFloat("_SrcBlend",5);m.SetFloat("_DstBlend",additive?1:10);return m;}

 [Test] public void AtlasMatchesTheTileEnum(){
  var atlas=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"Textures/T_FxAtlas.png");Assert.IsNotNull(atlas,"Run Tools~/fx_atlas.py");Assert.AreEqual(512,atlas.width);Assert.AreEqual(512,atlas.height);
  Assert.AreEqual(16,System.Enum.GetValues(typeof(FxTile)).Length,"4 × 4 atlas");
  Assert.AreEqual(new Vector4(.25f,.25f,0,.75f),EffectEmitters.Tile(FxTile.Glow),"tile 0 is top left");Assert.AreEqual(new Vector4(.25f,.25f,.75f,0),EffectEmitters.Tile(FxTile.Band),"tile 15 is bottom right");
  Assert.IsTrue(Shader.Find("Veilbreak/Feedback/Particle").isSupported);
  foreach(var clip in new[]{"A_Splash","A_Zap","A_Whoosh","A_Shield","A_Heal","A_Charge","A_Warning","A_Swap","A_Spawn","A_Tap","A_Stinger_Wave","A_Stinger_Boss"})Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<AudioClip>(Root+"Audio/"+clip+".wav"),clip+": run Tools~/sounds.py");
 }

 // Both tiers hold their caps under a flood of set pieces; the Low tier stays far inside the 2,000-particle budget, and
 // a full frame budget drops low-priority pieces while priority 2 still plays.
 [Test] public void EmittersStayWithinTierCaps([Values(true,false)] bool high){
  var host=new GameObject("Effects");Material add=Particle(true),alpha=Particle(false);
  try{var fx=host.AddComponent<EffectEmitters>();fx.Initialize(add,alpha,high);Assert.LessOrEqual(fx.Capacity,high?2200:700);
   var call=new FxCall{from=Vector3.zero,to=new Vector3(0,.5f,4),color=Color.red,magnitude=2,radius=2,angle=60};
   for(int frame=0;frame<120;frame++){fx.Advance(1/30f);foreach(EffectRoutine r in System.Enum.GetValues(typeof(EffectRoutine)))fx.Play(r,call,frame%3);Assert.LessOrEqual(fx.Particles,fx.Capacity);}
   fx.Advance(1/30f);int dropped=0;for(int i=0;i<200;i++)if(!fx.Play(EffectRoutine.Explosion,call,0))dropped++;Assert.Greater(dropped,0,"a full budget drops priority 0");Assert.IsTrue(fx.Play(EffectRoutine.Explosion,call,2),"priority 2 always plays");
   fx.Drift(10);Assert.AreEqual(-10,fx.Ground.position.z,1e-4f,"effects drift with the road");
   for(int i=0;i<200;i++)fx.Advance(.1f);Assert.AreEqual(0,fx.Particles,"every particle expires");}
  finally{Object.DestroyImmediate(host);Object.DestroyImmediate(add);Object.DestroyImmediate(alpha);}
 }

 [Test] public void MarksAndQuadsAreBounded(){
  var host=new GameObject("Marks");var material=Particle(false);
  try{var marks=host.AddComponent<GroundMarks>();marks.Initialize(material,false);Assert.AreEqual(GroundMarks.LowCapacity,marks.Capacity);
   for(int i=0;i<200;i++)marks.Add(i%2==0?MarkKind.Scorch:MarkKind.Crack,new Vector3(i*.1f,0,0),2,5);marks.Advance(.1f);Assert.AreEqual(marks.Capacity,marks.Live,"the oldest marks are replaced");
   marks.Advance(6);Assert.AreEqual(0,marks.Live,"marks fade out over their life");
   var batch=new QuadBatch(host.transform,"Quads",material,8);batch.Begin();int added=0;for(int i=0;i<20;i++)if(batch.Add(Vector3.zero,1,0,FxTile.Blob,Color.black))added++;batch.End();Assert.AreEqual(8,added);Assert.AreEqual(8,batch.Count);
   Assert.AreEqual(1,host.GetComponentsInChildren<MeshRenderer>().Length-1,"one renderer per batch (plus the marks batch)");batch.Dispose();
   var lights=host.AddComponent<LightFlashes>();lights.Initialize(3);for(int i=0;i<10;i++)lights.Play(Vector3.zero,Color.white,4,6,.3f);Assert.AreEqual(3,lights.Lit);lights.Advance(1);Assert.AreEqual(0,lights.Lit);}
  finally{Object.DestroyImmediate(host);Object.DestroyImmediate(material);}
 }

 [Test] public void CatalogPrefersTheMostSpecificRow(){
  var catalog=ScriptableObject.CreateInstance<CueCatalog>();catalog.cues=new[]{new CueCatalog.Cue{key="cast"},new CueCatalog.Cue{key="beam"},new CueCatalog.Cue{key="skill:terminal_vector"}};
  try{Assert.AreEqual("skill:terminal_vector",catalog.Find("skill:terminal_vector","beam","cast").key);Assert.AreEqual("beam",catalog.Find("skill:other","beam","cast").key);Assert.AreEqual("cast",catalog.Find("skill:other",null,"cast").key);Assert.IsNull(catalog.Find(null));}
  finally{Object.DestroyImmediate(catalog);}
 }
}
}
