using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;
namespace Veilbreak.Gui.Tests {
/// <summary>VbVectorShape's outline maths and vocabularies, and VbPressGlow's hooks; the painting itself is checked in the editor.</summary>
public sealed class VectorShapeTests {
 [Test] public void OutlineCutsTwoOppositeCorners(){
  CollectionAssert.AreEqual(new[]{new Vector2(10,0),new Vector2(100,0),new Vector2(100,30),new Vector2(90,40),new Vector2(0,40),new Vector2(0,10)},VbVectorShape.Outline(false,100,40,10,0),"top-left and bottom-right");
  CollectionAssert.AreEqual(new[]{new Vector2(-2,-2),new Vector2(92,-2),new Vector2(102,8),new Vector2(102,42),new Vector2(8,42),new Vector2(-2,32)},VbVectorShape.Outline(true,100,40,10,2),"top-right and bottom-left, grown by the inset");}
 [Test] public void AnUnpaintedElementTracesItsRectangle(){Assert.AreEqual(4,VbVectorShape.Outline(new VisualElement(),0).Length);}
 [Test] public void AttachMarksTheElementWithTheVocabularyClass(){
  var a=VbVectorShape.Attach(new VisualElement());Assert.IsTrue(a.ClassListContains(VbVectorShape.ClassName));
  var b=VbVectorShape.Attach(new Button(),new VbVectorVocabulary("--hud-","hud-shape"));Assert.IsTrue(b.ClassListContains("hud-shape"));Assert.IsFalse(b.ClassListContains(VbVectorShape.ClassName));}
 [Test] public void VocabulariesCarryTheirPrefixAndBlend(){var v=new VbVectorVocabulary("--",  "ar-shape",.3f);Assert.AreEqual("--",v.Prefix);Assert.AreEqual(.3f,v.Blend);
  Assert.AreEqual("--vb-vector-",VbVectorShape.Default.Prefix);Assert.AreEqual(VbVectorShape.ClassName,VbVectorShape.Default.ClassName);Assert.AreEqual(.2f,VbVectorShape.Default.Blend);}
 // ds#38: --vb-cut, -fill, -edge, -bw, -bracket and -glow are canonical for both painters; --vb-vector-* are aliases; ar-hud's bare names are fallbacks for gui's own vocabulary.
 [Test] public void GuiVocabularyReadsAliasThenCanonicalThenBare(){var gui=VbVectorShape.Default;
  CollectionAssert.AreEqual(new[]{"--vb-vector-stroke","--vb-edge","--stroke"},gui.Candidates("stroke"));CollectionAssert.AreEqual(new[]{"--vb-vector-stroke-width","--vb-bw","--stroke-width"},gui.Candidates("stroke-width"));
  foreach(var name in new[]{"cut","fill","glow","bracket"})CollectionAssert.AreEqual(new[]{"--vb-vector-"+name,"--vb-"+name,"--"+name},gui.Candidates(name),name);
  CollectionAssert.AreEqual(new[]{"--vb-vector-flip","--flip"},gui.Candidates("flip"),"Properties only the vector painter has keep alias and bare names");}
 [Test] public void ArHudAndOtherVocabulariesReadOnlyTheirOwnPrefix(){var hud=new VbVectorVocabulary("--","hud-shape");CollectionAssert.AreEqual(new[]{"--stroke"},hud.Candidates("stroke"));CollectionAssert.AreEqual(new[]{"--cut"},hud.Candidates("cut"));
  var other=new VbVectorVocabulary("--hud-","hud-shape",.3f);CollectionAssert.AreEqual(new[]{"--hud-stroke-width"},other.Candidates("stroke-width"));CollectionAssert.AreEqual(new[]{"--hud-flip"},other.Candidates("flip"));}
 [Test] public void VbShapeAndTheVectorVocabularyShareTheCanonicalNames(){var shape=System.IO.File.ReadAllText(PackageFiles.Path("Runtime/Core/VbShape.cs"));foreach(var name in new[]{"--vb-cut","--vb-fill","--vb-edge","--vb-bw","--vb-bracket","--vb-glow"})StringAssert.Contains("\""+name+"\"",shape);
  var css=System.IO.File.ReadAllText(PackageFiles.Path("Runtime/Styles/Gui.uss"));StringAssert.Contains(".vb-vector {",css,"Vector shapes reset the canonical properties like every other shape");}
 [Test] public void LinearKeepsBothAlphas(){Assert.DoesNotThrow(()=>VbVectorShape.Linear(new Color(1,0,0,.2f),new Color(0,0,1,.8f),Vector2.zero,Vector2.right));}
 [Test] public void PressGlowOwnsOneOverlayUntilDisposed(){var tree=new VisualElement();var glow=new VbPressGlow(tree,new VbPressGlowOptions{LayerName="glow-layer"});
  var layer=tree.Q("glow-layer");Assert.IsNotNull(layer);Assert.AreEqual(PickingMode.Ignore,layer.pickingMode);glow.Dispose();Assert.IsNull(tree.Q("glow-layer"));}
 [Test] public void PressGlowDefaultsToTheGuiVocabulary(){var options=new VbPressGlowOptions();Assert.AreEqual("--vb-neon",options.NeonProperty);Assert.IsNull(options.KeyboardClass);Assert.IsNull(options.Still);Assert.AreEqual("button-fx",options.LayerName);
  var tree=new VisualElement();using(new VbPressGlow(tree))Assert.IsNotNull(tree.Q("button-fx"));}
}
}
