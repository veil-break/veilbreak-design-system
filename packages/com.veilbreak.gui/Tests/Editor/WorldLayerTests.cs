using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;
namespace Veilbreak.Gui.Tests {
public sealed class WorldLayerTests {
 static WorldLayer Layer()=>new WorldLayer{Project=p=>new Vector2(p.x,p.z)};

 [Test] public void SameTargetHitsMergeAndThePoolIsBounded(){var layer=Layer();for(int i=0;i<100;i++)layer.Text(1,Vector3.zero,2,CombatTextKind.Damage);Assert.AreEqual(1,layer.LiveTexts);Assert.AreEqual("200",layer.Q<CombatText>().text,"Hits inside the merge window add up");
  layer.Text(1,Vector3.zero,7,CombatTextKind.Heal);Assert.AreEqual(2,layer.LiveTexts,"A different kind is its own number");
  for(int i=2;i<200;i++)layer.Text(i,Vector3.zero,1,CombatTextKind.Taken);Assert.AreEqual(layer.TextLimit,layer.Query<CombatText>().ToList().Count);
  layer.EndFrame(layer.TextSeconds+.1f);Assert.AreEqual(0,layer.LiveTexts);foreach(var t in layer.Query<CombatText>().ToList())Assert.AreEqual(DisplayStyle.None,t.style.display.value);}

 [Test] public void MergingStopsAfterAQuietGapAndATargetKeepsThreeNumbers(){var layer=Layer();layer.Text(1,Vector3.zero,5,CombatTextKind.Damage);layer.EndFrame(layer.MergeSeconds+.01f);layer.Text(1,Vector3.zero,5,CombatTextKind.Damage);Assert.AreEqual(2,layer.LiveTexts);
  layer.EndFrame(layer.MergeSeconds+.01f);layer.Text(1,Vector3.zero,5,CombatTextKind.Damage);layer.EndFrame(layer.MergeSeconds+.01f);layer.Text(1,Vector3.zero,9,CombatTextKind.Damage);Assert.AreEqual(layer.PerTarget,layer.LiveTexts,"A fourth number reuses the target's oldest");
  layer.Text(2,Vector3.zero,1,CombatTextKind.Miss);layer.Text(2,Vector3.zero,1,CombatTextKind.Miss);Assert.AreEqual(layer.PerTarget+2,layer.LiveTexts,"Misses never merge");}

 [Test] public void FormatsFollowTheCombatTextKinds(){Assert.AreEqual("1,250",CombatText.Format(CombatTextKind.Damage,1250));Assert.AreEqual("−40",CombatText.Format(CombatTextKind.Taken,40));Assert.AreEqual("+12",CombatText.Format(CombatTextKind.Heal,12));Assert.AreEqual("BLOCKED 30",CombatText.Format(CombatTextKind.Barrier,30));Assert.AreEqual("MISS",CombatText.Format(CombatTextKind.Miss,0));
  var text=new CombatText().Bind(CombatTextKind.Crit,88);Assert.IsTrue(text.ClassListContains("vb-combat-text--crit"));Assert.IsFalse(text.ClassListContains("vb-combat-text--damage"));}

 [Test] public void UnrefreshedElementsAreRemovedAtEndOfFrame(){var layer=Layer();layer.BeginFrame();layer.Bar(1,Vector3.zero,1);layer.Bar(2,Vector3.zero,.5f,.2f,WorldTeam.Ally,WorldRank.Boss);layer.Label(1,Vector3.zero,WorldLabelKind.Telegraph,"SLAM 1.2s");layer.Marker(1,Vector3.zero,WorldMarkerKind.Enemy);layer.EndFrame(.1f);Assert.AreEqual(2,layer.Bars);Assert.AreEqual(1,layer.Labels);Assert.AreEqual(1,layer.Markers);
  layer.BeginFrame();layer.Bar(2,Vector3.zero,.5f);layer.EndFrame(.1f);Assert.AreEqual(1,layer.Bars);Assert.AreEqual(0,layer.Labels);Assert.AreEqual(0,layer.Markers);Assert.AreEqual(1,layer.Query<WorldHealthBar>().ToList().Count);
  var behind=new WorldLayer{Project=_=>null};behind.BeginFrame();var bar=behind.Bar(1,Vector3.zero,1);Assert.AreEqual(DisplayStyle.None,bar.style.display.value,"Points behind the view hide");}

 [Test] public void HealthBarGhostHoldsThenDrainsAndBarrierFollowsHealth(){var bar=new WorldHealthBar().Set(1);bar.Set(.4f,.3f);Assert.AreEqual(1,bar.Ghost,1e-4f);bar.Step(bar.GhostHold*.5f);Assert.AreEqual(1,bar.Ghost,1e-4f,"The ghost holds first");bar.Step(bar.GhostHold);bar.Step(.4f);Assert.AreEqual(1-.4f*bar.GhostRate,bar.Ghost,1e-3f);
  var barrier=bar.Q(className:"vb-world-health__barrier");Assert.AreEqual(40,barrier.style.left.value.value,1e-3f);Assert.AreEqual(30,barrier.style.width.value.value,1e-3f);bar.Set(.9f,.3f);Assert.AreEqual(10,barrier.style.width.value.value,1e-3f,"Barrier is capped by the empty part of the bar");Assert.AreEqual(.9f,bar.Ghost,1e-4f,"A heal past the ghost pulls it along");
  bar.Team(WorldTeam.Ally).Rank(WorldRank.Elite);Assert.IsTrue(bar.ClassListContains("vb-world-health--ally")&&bar.ClassListContains("vb-world-health--elite"));Assert.IsFalse(bar.ClassListContains("vb-world-health--enemy"));}

 [Test] public void OffscreenChipsPinToTheEdgeTowardThePoint(){var size=new Vector2(390,600);var insets=new Vector4(0,50,0,0);
  Assert.IsFalse(WorldLayer.Pin(new Vector2(200,300),size,insets,10,out _,out _),"A visible point has no chip");
  Assert.IsTrue(WorldLayer.Pin(new Vector2(195,1325),size,insets,10,out var below,out float down));Assert.AreEqual(195,below.x,1e-3f);Assert.AreEqual(590,below.y,1e-3f);Assert.AreEqual(90,down,1e-3f,"Straight below points down (clockwise from right)");
  Assert.IsTrue(WorldLayer.Pin(new Vector2(-805,325),size,insets,10,out var left,out float west));Assert.AreEqual(10,left.x,1e-3f);Assert.AreEqual(325,left.y,1e-3f);Assert.AreEqual(180,Mathf.Abs(west),1e-3f);
  Assert.IsTrue(WorldLayer.Pin(new Vector2(195,40),size,insets,10,out var top,out _));Assert.AreEqual(60,top.y,1e-3f,"Chips keep clear of the inset HUD band");
  var chip=new OffscreenIndicator().Bind("BREACH",WorldTeam.Enemy,3.4f,true);Assert.IsTrue(chip.ClassListContains("vb-offscreen--threat"));Assert.AreEqual("3m",chip.Q<Label>(className:"vb-offscreen__distance").text);}

 [Test] public void LabelsAndMarkersCarryKindTeamAndShape(){var label=new WorldLabel().Bind(WorldLabelKind.Blocked,"NO TARGET","Out of range");Assert.AreEqual("× NO TARGET",label.Title);Assert.IsTrue(label.ClassListContains("vb-world-label--blocked"));label.ShowDetail(false);Assert.AreEqual(DisplayStyle.None,label.Q<Label>(className:"vb-world-label__detail").style.display.value);
  var marker=new WorldMarker().Bind(WorldMarkerKind.Enemy,40);Assert.IsTrue(marker.Q<VbIcon>().ClassListContains("vb-icon--diamond"),"Enemies are diamonds");Assert.AreEqual(40,marker.style.width.value.value);Assert.IsTrue(new WorldMarker().Bind(WorldMarkerKind.Ally).Q<VbIcon>().ClassListContains("vb-icon--circle"),"Allies are circles");}
 [Test] public void SameTitleLabelsInAGroupMergeAtTheirCentroid(){var layer=Layer();
  layer.BeginFrame();for(int i=0;i<3;i++)layer.Label(10+i,new Vector3(i*30,0,100),WorldLabelKind.Name,"TETHERED",null,WorldTeam.Enemy,"tether");layer.Label(20,new Vector3(0,0,50),WorldLabelKind.Name,"RELAY SHEPHERD","3 tethers",WorldTeam.Enemy);layer.EndFrame(.1f);
  Assert.AreEqual(2,layer.Labels,"Three tethers and the source are two labels");var labels=layer.Query<WorldLabel>().ToList();var merged=labels.Find(l=>l.Title.StartsWith("TETHERED"));Assert.AreEqual("TETHERED ×3",merged.Title);Assert.AreEqual("",merged.Detail,"A merged label drops its detail");
  Assert.AreEqual(30,merged.style.left.value.value,1e-3f,"At the centroid of its members");Assert.AreEqual(100-layer.LabelLift,merged.style.top.value.value,1e-3f);Assert.AreEqual("3 tethers",labels.Find(l=>l.Title=="RELAY SHEPHERD").Detail,"An ungrouped label is untouched");
  layer.BeginFrame();layer.Label(10,new Vector3(40,0,10),WorldLabelKind.Name,"TETHERED","Aria",WorldTeam.Enemy,"tether");layer.Label(11,new Vector3(0,0,10),WorldLabelKind.Name,"CHARGED",null,WorldTeam.Enemy,"tether");layer.EndFrame(.1f);
  Assert.AreEqual(2,layer.Labels,"The same group key with another title is its own label");Assert.AreSame(merged,layer.Query<WorldLabel>().Where(l=>l.Title=="TETHERED").First(),"The merged element is kept and reads as itself when one remains");Assert.AreEqual("Aria",merged.Detail);
  layer.BeginFrame();layer.EndFrame(.1f);Assert.AreEqual(0,layer.Labels,"Groups that are not refreshed go");Assert.AreEqual(0,layer.Query<WorldLabel>().ToList().Count);
  var behind=new WorldLayer{Project=_=>null};behind.BeginFrame();var hidden=behind.Label(1,Vector3.zero,WorldLabelKind.Name,"TETHERED",null,WorldTeam.Enemy,"tether");behind.EndFrame(.1f);Assert.AreEqual(DisplayStyle.None,hidden.style.display.value,"A group with no visible member hides");}

 [Test] public void EliteLabelsAreOneLineWithADiamondAndNoEdge(){var label=new WorldLabel().Bind(WorldLabelKind.Elite,"ELITE","Voltaic",WorldTeam.Enemy);Assert.AreEqual("Voltaic",label.Title,"The trait is the line");Assert.AreEqual("",label.Detail);Assert.IsTrue(label.ClassListContains("vb-world-label--elite"));
  Assert.AreEqual(DisplayStyle.Flex,label.Q<VbIcon>().style.display.value,"The diamond mark shows");Assert.AreEqual("ELITE",new WorldLabel().Bind(WorldLabelKind.Elite,"ELITE").Title);
  Assert.AreEqual(DisplayStyle.None,new WorldLabel().Bind(WorldLabelKind.Name,"ARIA").Q<VbIcon>().style.display.value,"Other kinds have no mark");
  var uss=System.IO.File.ReadAllText(PackageFiles.Path("Runtime/Styles/World.uss"));var rules=System.Text.RegularExpressions.Regex.Matches(uss,@"\.vb-world-label[^{]*\{[^}]*\}");Assert.Greater(rules.Count,6);
  foreach(System.Text.RegularExpressions.Match m in rules)StringAssert.DoesNotContain("border",m.Value,"World labels are unbordered text on an underlay");}
}
}
