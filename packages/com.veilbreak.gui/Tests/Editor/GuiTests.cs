using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;
namespace Veilbreak.Gui.Tests {
public sealed class GuiTests {
 [Test] public void StatesBecomeModifierClasses(){
  var b=new SkillButton();foreach(SkillState s in Enum.GetValues(typeof(SkillState))){b.Bind(new SkillView{state=s});Assert.IsTrue(b.ClassListContains("vb-skill--"+Ui.Kebab(s.ToString())));Assert.AreEqual(1,Enum.GetNames(typeof(SkillState)).Count(n=>b.ClassListContains("vb-skill--"+Ui.Kebab(n))));}
 }
 [Test] public void BlockedButtonSaysWhyAndCannotClick(){var b=new VbButton("Start run",ButtonVariant.Primary){Blocked="Pick 1 more"};Assert.IsFalse(b.enabledSelf);Assert.IsTrue(b.ClassListContains("vb-button--blocked"));Assert.IsTrue(b.Query<Label>().ToList().Any(l=>l.text=="Pick 1 more"));b.Blocked="";Assert.IsTrue(b.enabledSelf);}
 [Test] public void FallenCardsHideSkills(){var c=new OperativeCard().Bind(new OperativeView{name="Nyra",mode=ControlMode.Fallen});Assert.AreEqual(DisplayStyle.None,c.Q(className:"vb-operative__skills").style.display.value);Assert.IsTrue(c.ClassListContains("vb-operative--fallen"));}
 [Test] public void SwitchAndSegmentedRaiseChangeEvents(){var s=new VbSwitch("Haptics",null,false);s.value=true;Assert.IsTrue(s.ClassListContains("vb-switch--on"));var g=new VbSegmented(new[]{("A",""),("B","")},0);g.value=1;Assert.AreEqual(1,g.value);}
 [Test] public void EveryIconClassHasAnSvg(){var icons=PackageFiles.Path("Runtime/Icons");var lines=File.ReadAllLines(PackageFiles.Path("Runtime/Styles/Icons.uss")).Where(l=>l.StartsWith(".vb-icon--")).ToArray();Assert.Greater(lines.Length,100);
  foreach(var line in lines){var file=line.Split('"')[1].Replace("../Icons/","");Assert.IsTrue(File.Exists(Path.Combine(icons,file)),file);}
  foreach(var svg in Directory.GetFiles(icons,"*.svg")){var name=System.Text.RegularExpressions.Regex.Replace(Path.GetFileNameWithoutExtension(svg),"(?<!^)([A-Z])","-$1").ToLowerInvariant();Assert.IsTrue(lines.Any(l=>l.StartsWith(".vb-icon--"+name+" ")),"No class for "+name+"; run Tools~/icons.py");}}
 [Test] public void DesignSystemIconNamesResolveToPackageGlyphs(){var uss=PackageFiles.Read("Runtime/Styles/Icons.uss");
  foreach(var (alias,canonical) in new[]{("plus","Add"),("silenced","Silence"),("rooted","Root"),("slowed","Slow"),("replay","Replays"),("cancel","Close"),("chevron-right","Chevron"),("rift","Rifts")})StringAssert.Contains(".vb-icon--"+alias+" { background-image: url(\"../Icons/"+canonical+".svg\"); }",uss);
  foreach(var name in new[]{"arrow","diamond","circle","no-target","chevron-left","autopilot","marker"})StringAssert.Contains(".vb-icon--"+name+" ",uss,"design-system glyph "+name);
  Assert.IsFalse(uss.Contains(".vb-icon--mark "),"The brand mark is VbMark (Vectors/Mark.svg); the reticle glyph is 'marker'");}
 [Test] public void NestedModalsCoverEachLayerOnceAndRestoreInput(){var root=new VisualElement();var content=new VisualElement();root.Add(content);var host=new OverlayHost(root);var first=host.Push(content);Assert.IsFalse(content.enabledSelf);Assert.IsTrue(content.ClassListContains("vb-covered"));var second=host.Push(content);Assert.IsFalse(first.enabledSelf);Assert.IsFalse(second.ClassListContains("vb-covered"));Assert.AreEqual(2,host.Count);Assert.IsTrue(host.Pop());Assert.IsTrue(first.enabledSelf);Assert.IsFalse(content.enabledSelf);Assert.IsTrue(host.Pop());Assert.IsTrue(content.enabledSelf);Assert.IsFalse(content.ClassListContains("vb-covered"));Assert.IsFalse(host.Pop());}
 [Test] public void KebabMatchesIconFileNames()=>Assert.AreEqual("skill-aria-needle-storm",Ui.Kebab("SkillAriaNeedleStorm"));
 [Test] public void QueuedSkillsShowTheirPlaceAndBufferedReadNext(){var b=new SkillButton();b.Bind(new SkillView{state=SkillState.Queued,queue=2,cost=18});Assert.IsTrue(b.Query<Label>().ToList().Any(l=>l.text=="#2"));b.Bind(new SkillView{state=SkillState.Buffered,cost=18});Assert.IsTrue(b.Query<Label>().ToList().Any(l=>l.text=="NEXT"));}
 [Test] public void ManualQueueRowsCancelOnTap(){QueueView cancelled=null;var list=new QueueList().Bind(new[]{new QueueView{who="Aria",skill="Needle Storm",cost=18,actor=0,slot=1,manual=true},new QueueView{who="Rook",skill="Seizure",cost=30}},"MANUAL QUEUE","",q=>cancelled=q);
  var rows=list.Query(className:"vb-queue__row").ToList();Assert.AreEqual(2,rows.Count);Assert.IsTrue(rows[0].ClassListContains("vb-queue__row--manual"));Assert.IsFalse(rows[1].ClassListContains("vb-queue__row--manual"));
  Assert.IsFalse(list.CancelRow(1),"Autoplay estimates cannot be cancelled");Assert.IsNull(cancelled);
  Assert.IsTrue(list.CancelRow(0));Assert.AreEqual(0,cancelled.actor);Assert.AreEqual(1,cancelled.slot);Assert.IsNull(rows[0].parent);Assert.IsFalse(list.CancelRow(0),"A cancelled row cannot cancel twice");}
 [Test] public void ThreeOrMoreSegmentsShareTheRow(){Assert.IsFalse(new VbSegmented(new[]{("LOW",""),("HIGH","")},0).ClassListContains("vb-segmented--fill"),"Two peers keep their 100-unit options");Assert.IsTrue(new VbSegmented(new[]{("FULL",""),("REDUCED",""),("OFF","")},0).ClassListContains("vb-segmented--fill"),"Three peers fill the row so they fit a 320 board");}
}
}
