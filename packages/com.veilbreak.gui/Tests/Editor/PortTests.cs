using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;
namespace Veilbreak.Gui.Tests {
// Components and states ported from the Veilbreak design system's web kit.
public sealed class PortTests {
 static string Text(VisualElement e,string cls)=>e.Q<Label>(className:cls).text;

 [Test] public void SkillButtonReadsWindupInvalidRejectedAndExposed(){var b=new SkillButton();
  b.Bind(new SkillView{state=SkillState.Windup,progress=.25f,seconds=.6f});Assert.IsTrue(b.ClassListContains("vb-skill--windup"));var sweep=b.Q(className:"vb-skill__sweep");Assert.AreEqual(DisplayStyle.Flex,sweep.style.display.value);Assert.AreEqual(25,sweep.style.width.value.value,1e-3f);
  b.Bind(new SkillView{state=SkillState.Invalid,reason="Out of range",icon="fan"});Assert.AreEqual("OUT OF RANGE",Text(b,"vb-skill__value"));Assert.IsTrue(b.Q<VbIcon>().ClassListContains("vb-icon--no-target"),"Invalid aim strikes the crosshair");StringAssert.Contains("Out of range",b.tooltip);Assert.AreEqual(DisplayStyle.None,sweep.style.display.value);
  b.Bind(new SkillView{state=SkillState.Rejected,cost=18});Assert.IsTrue(b.ClassListContains("vb-skill--flash"),"A refused command flashes once");Assert.AreEqual("18",Text(b,"vb-skill__value"));
  b.Bind(new SkillView{state=SkillState.Exposed,cost=30});Assert.IsTrue(b.ClassListContains("vb-skill--exposed"),"An exposed target inverts the cost line");Assert.AreEqual("30",Text(b,"vb-skill__value"));Assert.IsFalse(b.ClassListContains("vb-skill--rejected"));}

 [Test] public void QueueChipsCancelOnlyManualEntries(){var chip=new QueueChip();int cancelled=0;chip.Cancelled+=()=>cancelled++;
  chip.Bind(new QueueView{skill="Needle Storm",who="AR",cost=18,eta="0.8s"},1);Assert.IsTrue(chip.ClassListContains("vb-queue-chip--next"));Assert.AreEqual("NEEDLE STORM",Text(chip,"vb-queue-chip__skill"));Assert.IsFalse(chip.Cancel(),"Autoplay estimates cannot be cancelled");
  chip.Bind(new QueueView{skill="Zero Line",cost=40,manual=true},2);Assert.IsFalse(chip.ClassListContains("vb-queue-chip--next"));Assert.IsTrue(chip.Cancel());Assert.AreEqual(1,cancelled);
  chip.Bind(new QueueView{skill="Zero Line",cost=40,manual=true},3,preview:true);Assert.IsFalse(chip.Cancel(),"A preview is not in the queue yet");Assert.AreEqual(DisplayStyle.None,chip.Q(className:"vb-queue-chip__cancel").style.display.value);}

 [Test] public void PowerStripShowsReserveQueueAndLegend(){var strip=new PowerStrip();var cancelled=new List<QueueView>();var entries=new[]{new QueueView{skill="A",cost=10,manual=true},new QueueView{skill="B",cost=20},new QueueView{skill="C",cost=30},new QueueView{skill="D",cost=40},new QueueView{skill="E",cost=50}};
  strip.Set(60,100,"+12/s").Reserve(25).Queue(entries,cancelled.Add).Legend("┊ autoplay reserve");var reserve=strip.Q(className:"vb-power__reserve");Assert.AreEqual(25,reserve.style.left.value.value,1e-3f);
  var chips=strip.Query<QueueChip>().ToList();Assert.AreEqual(4,chips.Count,"At most four entries");Assert.IsTrue(chips[0].Cancel());Assert.AreSame(entries[0],cancelled[0]);
  strip.Queue(new QueueView[0]).Reserve(0).Legend(null);Assert.AreEqual(DisplayStyle.None,strip.Q(className:"vb-power__queue").style.display.value);Assert.AreEqual(DisplayStyle.None,reserve.style.display.value);Assert.AreEqual(DisplayStyle.None,strip.Q<Label>(className:"vb-power__legend").style.display.value);}

 [Test] public void HealthBarGhostCaptionAndSize(){var bar=new HealthBar().Set(400,1000,0,-1,false,true,700).Size(MeterSize.World).Caption("HP 400");var ghost=bar.Q(className:"vb-health__ghost");Assert.AreEqual(DisplayStyle.Flex,ghost.style.display.value);Assert.AreEqual(70,ghost.style.width.value.value,1e-3f);
  Assert.AreEqual(0,bar.Q(className:"vb-health__hp").IndexOf(ghost),"The ghost sits behind the fill");Assert.IsTrue(bar.ClassListContains("vb-health--world"));Assert.AreEqual("HP 400",Text(bar,"vb-health__label"));
  bar.Set(800,1000,0,-1,false,true,700);Assert.AreEqual(DisplayStyle.None,ghost.style.display.value,"No ghost once health is above it");}

 [Test] public void StatDeltaToneAndStatusTime(){var stat=new StatReadout("DAMAGE","1,200").Delta("+12%");var delta=stat.Q<Label>(className:"vb-stat__delta");Assert.IsTrue(delta.ClassListContains("vb-stat__delta--up"));
  stat.Delta("−4");Assert.IsTrue(delta.ClassListContains("vb-stat__delta--down"));stat.Delta("+5",up:false);Assert.IsTrue(delta.ClassListContains("vb-stat__delta--down"),"Lower is better for costs");stat.Delta("same");Assert.IsFalse(delta.ClassListContains("vb-stat__delta--up")||delta.ClassListContains("vb-stat__delta--down"));
  Assert.AreEqual("2.1s",Text(new StatusChip(StatusKind.Silenced,"SILENCED","no skills","2.1s"),"vb-status__time"));}

 [Test] public void TilesShowTierAndStayVisibleWhenLocked(){var tile=new UnitTile().Bind(new OperativeView{name="Mira",role="Disruptor"},2,false,RarityTier.Gold,"Pull in rifts");Assert.IsTrue(tile.ClassListContains("vb-tile--gold")&&tile.ClassListContains("vb-tile--locked"));Assert.IsFalse(tile.ClassListContains("vb-tile--in"),"A locked operative is not in the squad");Assert.AreEqual("PULL IN RIFTS",Text(tile,"vb-tile__role-text"));
  tile.Bind(new OperativeView{name="Mira",role="Disruptor"},1,true);Assert.IsFalse(tile.ClassListContains("vb-tile--gold")||tile.ClassListContains("vb-tile--locked"));Assert.IsTrue(tile.ClassListContains("vb-tile--in"));}

 [Test] public void RevealCardsNameTiersAndFlip(){Assert.AreEqual("LEGENDARY",RevealCard.TierWord(RarityTier.Gold,false));Assert.AreEqual("MAGIC ITEM",RevealCard.TierWord(RarityTier.Blue,true));
  var calm=new VisualElement();calm.AddToClassList(SafeAreaRoot.ReducedMotion);var card=new RevealCard().Bind(RarityTier.Violet,"Hex","HX · Debuffer",dupe:"Resonance R2").Into(calm);Assert.IsFalse(card.Flipped);Assert.AreEqual("RESONANCE R2",Text(card,"vb-reveal__badge"));
  bool flipping=false;card.Flipping+=()=>flipping=true;card.Flip();Assert.IsTrue(card.Flipped&&flipping,"Reduced motion turns the card at once");Assert.AreEqual(DisplayStyle.None,card.Q(className:"vb-reveal__back").style.display.value);}

 [Test] public void MarkGeometryHasEveryLetterAndBladeSlice(){Assert.AreEqual(15,MarkGeometry.Parts.Count);int blades=0;var all=new Rect(0,0,640,686);
  foreach(var (id,blade,d) in MarkGeometry.Parts){if(blade)blades++;var rings=MarkGeometry.Rings(d);Assert.Greater(rings.Length,0,id);foreach(var ring in rings)foreach(var v in ring)Assert.IsTrue(all.Contains(v),id+" stays on the artboard");}
  Assert.AreEqual(4,blades,"The blade is four slices");Assert.AreEqual(2,MarkGeometry.Rings("M0 0 L10 0 L10 10 Z M20 20 L30 20 L30 30 Z").Length,"Each M starts a ring");}

 [Test] public void RuleRowsBindConditionsAndToggle(){var row=new RuleRow().Bind(new RuleView{index=2,conditions=new[]{new RuleCondition{text="Power ≥",value="80"},new RuleCondition{text="enemy within",value="6",after="m"}},action="Zero Line",cost=40,note="Runs only at 80+ Power: 40 cost + 40 reserve"});
  var values=row.Query<Label>(className:"vb-rule__value").ToList();Assert.AreEqual(2,values.Count);Assert.AreEqual("AND",Text(row,"vb-rule__join"));Assert.AreEqual("−40",Text(row,"vb-rule__cost"));
  bool? state=null;row.Toggled+=on=>state=on;row.Toggle();Assert.AreEqual(false,state);Assert.IsTrue(row.ClassListContains("vb-rule--off"));
  var platform=new HoloPlatform().Bind(null,null,PlatformState.Open);Assert.AreEqual("+ OPEN SLOT",Text(platform,"vb-platform__label"));Assert.AreEqual(DisplayStyle.None,platform.Q<VbIcon>(className:"vb-platform__marker").style.display.value);
  var frame=new HoloFrame("Last match");frame.Add(new Label("body"));Assert.AreEqual("LAST MATCH",frame.Title);Assert.IsNotNull(frame.Q(className:"vb-holo__body").Q<Label>(),"Children go to the frame body");Assert.IsNotNull(frame.Q<ScanLines>());}
}
}
