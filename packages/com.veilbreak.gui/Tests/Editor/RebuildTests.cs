using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;
namespace Veilbreak.Gui.Tests {
/// <summary>VbRebuild animates the difference between two builds of a tree; off a panel it matches by path and applies the first frame.</summary>
public sealed class RebuildTests {
 static VisualElement Tree(params VisualElement[] children){var root=new VisualElement();foreach(var c in children)root.Add(c);return root;}
 [Test] public void ElementsAreMatchedByNameThenNumbered(){var snapshot=VbRebuild.Capture(Tree(new Label("1,000"){name="rating"},new VisualElement{name="row"},new VisualElement{name="row"}));
  Assert.IsTrue(snapshot.Contains("/rating"));Assert.IsTrue(snapshot.Contains("/row"));Assert.IsTrue(snapshot.Contains("/row@2"));Assert.AreEqual(3,snapshot.Count);}
 [Test] public void UnnamedElementsAreMatchedByTypeAndIndex(){var snapshot=VbRebuild.Capture(Tree(new Label("a"),new VisualElement()));Assert.IsTrue(snapshot.Contains("/Label#0"));Assert.IsTrue(snapshot.Contains("/VisualElement#1"));}
 [Test] public void SkippedSubtreesAreNotCaptured(){var quiet=new VisualElement{name="quiet"};quiet.AddToClassList(VbRebuild.Skip);quiet.Add(new Label{name="inner"});
  var snapshot=VbRebuild.Capture(Tree(quiet,new VisualElement{name="kept"}));Assert.IsFalse(snapshot.Contains("/quiet"));Assert.IsFalse(snapshot.Contains("/quiet/inner"));Assert.IsTrue(snapshot.Contains("/kept"));
  var mine=new VisualElement{name="mine"};mine.AddToClassList("host-skip");Assert.IsFalse(VbRebuild.Capture(Tree(mine),"host-skip").Contains("/mine"),"A host names its own class");}
 [Test] public void NothingIsCapturedWithoutAScope(){Assert.AreEqual(0,VbRebuild.Capture(null).Count);Assert.DoesNotThrow(()=>VbRebuild.Play(null,null,false));}
 [Test] public void PlayReplaysTheOldClassesForOneFrame(){var snapshot=VbRebuild.Capture(Tree(new Label("1,000"){name="rating"}));
  var label=new Label("1,482"){name="rating"};label.AddToClassList("on");VbRebuild.Play(Tree(label),snapshot,reduced:true);
  Assert.IsFalse(label.ClassListContains("on"),"the old classes show first");Assert.AreEqual("1,482",label.text);}
 [Test] public void ChangedTextFadesUnlessItDecodes(){var was=new Label("RANGED"){name="role"};was.AddToClassList(VbRebuild.Decode);var snapshot=VbRebuild.Capture(Tree(new Label("ARIA"){name="name"},was));
  var name=new Label("NYRA"){name="name"};var role=new Label("HEALER"){name="role"};role.AddToClassList(VbRebuild.Decode);VbRebuild.Play(Tree(name,role),snapshot,reduced:false);
  Assert.AreEqual("NYRA",name.text);Assert.Less(name.style.opacity.value,1,"a plain label fades its new text in");Assert.AreNotEqual("HEALER",role.text,"a decode label starts scrambled");}
 [Test] public void AHostNamesItsOwnDecodeClass(){var was=new Label("RANGED"){name="role"};was.AddToClassList("host-decode");var snapshot=VbRebuild.Capture(Tree(was));
  var role=new Label("HEALER"){name="role"};role.AddToClassList("host-decode");VbRebuild.Play(Tree(role),snapshot,reduced:false,decode:"host-decode");Assert.AreNotEqual("HEALER",role.text);}
 [Test] public void ReducedMotionKeepsTheNewTextAtOnce(){var snapshot=VbRebuild.Capture(Tree(new Label("ARIA"){name="name"}));var name=new Label("NYRA"){name="name"};VbRebuild.Play(Tree(name),snapshot,reduced:true);
  Assert.AreEqual("NYRA",name.text);Assert.AreEqual(StyleKeyword.Undefined,name.style.opacity.keyword);}
 [Test] public void TheBeatComesFromTheMotionTokens(){Assert.AreEqual(MotionRecipes.Rebuild/1000f,VbRebuild.Duration,1e-6f);Assert.AreEqual(.18f,VbRebuild.Duration,1e-6f);}
}
}
