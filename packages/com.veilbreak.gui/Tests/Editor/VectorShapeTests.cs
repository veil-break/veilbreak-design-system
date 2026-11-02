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
 [Test] public void LinearKeepsBothAlphas(){Assert.DoesNotThrow(()=>VbVectorShape.Linear(new Color(1,0,0,.2f),new Color(0,0,1,.8f),Vector2.zero,Vector2.right));}
 [Test] public void PressGlowOwnsOneOverlayUntilDisposed(){var tree=new VisualElement();var glow=new VbPressGlow(tree,new VbPressGlowOptions{LayerName="glow-layer"});
  var layer=tree.Q("glow-layer");Assert.IsNotNull(layer);Assert.AreEqual(PickingMode.Ignore,layer.pickingMode);glow.Dispose();Assert.IsNull(tree.Q("glow-layer"));}
 [Test] public void PressGlowDefaultsToTheGuiVocabulary(){var options=new VbPressGlowOptions();Assert.AreEqual("--vb-neon",options.NeonProperty);Assert.IsNull(options.KeyboardClass);Assert.IsNull(options.Still);Assert.AreEqual("button-fx",options.LayerName);
  var tree=new VisualElement();using(new VbPressGlow(tree))Assert.IsNotNull(tree.Q("button-fx"));}
}
}
