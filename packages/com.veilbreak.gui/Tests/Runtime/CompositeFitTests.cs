using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
namespace Veilbreak.Gui.Tests {
/// <summary>The composites of gui 0.8.0 (design-system#39) on live portrait boards at 320, 390 and 430 units: nothing a slot, card or row says is clipped or run together, and every target is
/// 44 units where a thumb hits it.</summary>
public sealed class CompositeFitTests {
 GameObject host;PanelSettings settings;RenderTexture target;VisualElement board;
 [TearDown] public void Close(){if(host!=null)Object.Destroy(host);if(settings!=null)Object.Destroy(settings);if(target!=null)target.Release();}
 // A themed live panel of the given width on the portrait aspect; the test builds into <see cref="board"/>. Narrow boards carry vb-compact as SafeAreaRoot gives them.
 IEnumerator Boot(int width,params string[] rootClasses){
#if UNITY_EDITOR
  Close();int height=Mathf.RoundToInt(width*2.164f);target=new RenderTexture(width,height,0);
  var theme=UnityEditor.AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>("Packages/com.veilbreak.gui/Runtime/Styles/VeilbreakGui.tss");Assert.IsNotNull(theme);
  settings=ScriptableObject.CreateInstance<PanelSettings>();settings.themeStyleSheet=theme;settings.scaleMode=PanelScaleMode.ConstantPixelSize;settings.scale=1;settings.targetTexture=target;
  host=new GameObject("Composite board");var document=host.AddComponent<UIDocument>();document.panelSettings=settings;yield return null;
  board=Ui.Box(document.rootVisualElement,"vb-root");board.style.flexGrow=1;if(width<380)board.AddToClassList("vb-compact");foreach(var c in rootClasses)board.AddToClassList(c);
#else
  Assert.Ignore("Needs the editor asset database");yield break;
#endif
 }
 static IEnumerator Frames(int n=5){for(int i=0;i<n;i++)yield return null;}
 static bool Shown(VisualElement e){for(var p=e;p!=null;p=p.parent)if(p.resolvedStyle.display==DisplayStyle.None)return false;return true;}
 static void Inside(Rect inner,Rect outer,string what){Assert.GreaterOrEqual(inner.xMin,outer.xMin-.6f,what+": left");Assert.LessOrEqual(inner.xMax,outer.xMax+.6f,what+": right");Assert.GreaterOrEqual(inner.yMin,outer.yMin-.6f,what+": top");Assert.LessOrEqual(inner.yMax,outer.yMax+.6f,what+": bottom");}
 static OperativeView Operative(string name,params SkillView[] skills)=>new OperativeView{name=name,hp=900,maxHp=1000,skills=skills};

 // ds#21: the shortfall and the queue place are corner badges inside the slot, at every width (they used to run below the slot's foot, and vanished at 320).
 [UnityTest] public IEnumerator SkillButtonNeverClipsShortfallOrQueueAt320(){
  foreach(int width in new[]{320,390,430}){
   yield return Boot(width);var battle=new BattleHudView().Into(board);battle.style.flexGrow=1;
   var ops=new[]{Operative("Aria",new SkillView{state=SkillState.Short,cost=45,need=45},new SkillView{slot="S",state=SkillState.Queued,cost=40,queue=2}),
    Operative("Rook",new SkillView{state=SkillState.Cooldown,cost=18,seconds=14.2f,charged=.3f},new SkillView{slot="S",state=SkillState.Short,cost=30,need=12}),
    Operative("Aegis",new SkillView{state=SkillState.Queued,cost=20,queue=1},new SkillView{slot="S",state=SkillState.Invalid,cost=40,reason="Out of range"}),
    Operative("Nyra",new SkillView{state=SkillState.Ready,cost=16},new SkillView{slot="S",state=SkillState.Windup,cost=40,seconds=1.5f,progress=.5f})};
   battle.Bind(new HudView(),ops);yield return Frames();
   var skills=board.Query<SkillButton>().ToList().Where(Shown).ToList();Assert.AreEqual(8,skills.Count,"Eight slots show at "+width);
   foreach(var skill in skills){var rect=skill.worldBound;var card=skill.GetFirstAncestorOfType<UnitCard>();Inside(rect,card.worldBound,$"{skill.View.state} slot is inside its card at {width}");Inside(rect,board.worldBound,"…and on the board");
    foreach(var label in skill.Query<Label>().ToList().Where(Shown))Inside(label.worldBound,rect,$"'{label.text}' ({skill.View.state}) stays inside its slot at {width}");}
   var short1=skills[0];Assert.AreEqual("−45",short1.Badge.text);Assert.AreEqual("45",short1.Value.text);var queued=skills[1];Assert.AreEqual("#2",queued.Badge.text);
   Assert.Greater(short1.worldBound.height,43.5f,"A slot is still a 44-unit target");Assert.Greater(short1.worldBound.width,43.5f);}
 }

 // ds#34 and ds#19: a pair at 320, 390 and 430: mirrored cuts and alignment, a centred glyph, and no title broken inside a word.
 [UnityTest] public IEnumerator AChoiceCardPairFitsMirroredAndBreaksNoWord(){
  foreach(int width in new[]{320,390,430}){
   yield return Boot(width);var page=Ui.Box(board,"vb-page");page.style.flexGrow=1;
   var route=new RouteChoice().Into(page);route.Bind(new RouteView{wave=3,left=new RoadView{kind="merchant",title="Transformer row",icon="inventory",hint="Trade stacks or HP for rare and epic cards"},
    right=new RoadView{kind="elite",title="Elite patrol",icon="warning",hint="2 elites with traits (x4 health), 50% more hostiles",rank=3,ranks=5}});yield return Frames();
   var cards=route.Query<ChoiceCard>().ToList();Assert.AreEqual(2,cards.Count);
   foreach(var card in cards){Inside(card.worldBound,board.worldBound,$"{card.name} fits the {width} board");
    var title=card.Q<Label>(className:"vb-choice__title");foreach(var word in title.text.Split(' ')){var size=title.MeasureTextSize(word,0,VisualElement.MeasureMode.Undefined,0,VisualElement.MeasureMode.Undefined);Assert.LessOrEqual(size.x,title.contentRect.width+.5f,$"'{word}' is not broken inside the word at {width} ({size.x:0} in {title.contentRect.width:0})");}
    var glyph=card.Q<VbIcon>();Assert.AreEqual(card.worldBound.center.x,glyph.worldBound.center.x,1.5f,"The glyph is centred");Assert.Greater(card.Q<Label>(className:"vb-choice__action").worldBound.height,43.5f,"The action is a 44-unit foot");}
   var left=cards[0];var right=cards[1];Assert.AreEqual(TextAnchor.UpperRight,left.Q<Label>(className:"vb-choice__title").resolvedStyle.unityTextAlign,"The left card aligns its text right");Assert.AreEqual(TextAnchor.UpperLeft,right.Q<Label>(className:"vb-choice__title").resolvedStyle.unityTextAlign,"The right card aligns its text left");
   float Side(VisualElement e)=>e.customStyle.TryGetValue(new CustomStyleProperty<float>("--vb-cut-side"),out var v)?v:-1;Assert.AreEqual(VbShape.Left,Side(left),"The left card cuts its left corners");Assert.AreEqual(VbShape.Right,Side(right),"The right card cuts its right corners");
   Assert.AreEqual(left.worldBound.width,right.worldBound.width,1.1f,"A pair shares the row");Assert.Less(left.worldBound.xMax,right.worldBound.xMin+.5f);
   var cells=right.Query(className:"vb-progress__cell").ToList().Where(Shown).ToList();Assert.AreEqual(5,cells.Count,"Threat is five rank cells");Assert.AreEqual(3,cells.Count(c=>c.ClassListContains("vb-progress__cell--on")));}
 }

 // ds#35: the key stays on one line and the value wraps beside it; an action is a 44-unit button and its width is shared down the list.
 [UnityTest] public IEnumerator StatRowKeepsItsKeyWrapsItsValueAndHoldsA44Action(){
  foreach(int width in new[]{320,430}){
   yield return Boot(width);var page=Ui.Box(board,"vb-page");var plain=new StatRow("After pick","+8 ward for every operative, stacking with the run's other ward perks").Into(page);
   var buy=new VbButton("BUY 80",ButtonVariant.Secondary);var sold=new VbButton("BUY 150",ButtonVariant.Secondary);
   var list=Ui.Box(page);var offer=new StatRow("QUICKDRAW","Breaking a hostile's stability restores 3 Power, at most once every 2 seconds").Into(list);offer.Plated=offer.Stacked=true;offer.WithAction(buy);
   var other=new StatRow("WIND","Short").Into(list);other.Plated=other.Stacked=true;other.WithAction(sold);yield return Frames();
   Assert.Less(plain.KeyLabel.worldBound.height,24f,"The key stays on one line");Assert.Greater(plain.ValueLabel.worldBound.height,plain.KeyLabel.worldBound.height*1.5f,"The value wraps");Assert.LessOrEqual(plain.ValueLabel.worldBound.xMax,plain.worldBound.xMax+.5f,"…inside the row");
   Assert.GreaterOrEqual(plain.ValueLabel.worldBound.xMin,plain.KeyLabel.worldBound.xMax-.5f,"…and beside the key, not under it");
   Assert.GreaterOrEqual(buy.worldBound.height,43.5f,"The action is 44 tall");Assert.GreaterOrEqual(sold.worldBound.height,43.5f);Assert.AreEqual(buy.worldBound.width,sold.worldBound.width,.6f,"Actions share one width");
   Assert.LessOrEqual(offer.worldBound.yMax,other.worldBound.yMin+.5f,"Rows do not overlap");Assert.LessOrEqual(offer.ValueLabel.worldBound.yMax,offer.worldBound.yMax+.5f,"A long value stays inside its row");Inside(offer.worldBound,board.worldBound,"The row fits the board");}
 }

 // ds#36: a perk chip is a 44-unit target with a smaller plate in it, in a row that wraps; a reading chip is only as tall as its plate.
 [UnityTest] public IEnumerator ChipRowIsA44TargetAndWrapsInARow(){
  foreach(int width in new[]{320,430}){
   yield return Boot(width);var page=Ui.Box(board,"vb-page");var row=Ui.Box(page,"vb-row","vb-row--wrap");
   for(int i=0;i<9;i++)new ChipRow().Into(row).Bind(new PerkView{id="p"+i,name="Perk "+i,icon="burst",stack=i%3+1,tier=(PerkTier)(i%4),now="+"+i+"%"});var credits=ChipRow.Credits().Into(page).SetNumber(120);yield return Frames();
   var chips=row.Query<ChipRow>().ToList();foreach(var chip in chips){Assert.GreaterOrEqual(chip.worldBound.height,43.5f,"A chip's hit area is 44 tall");Assert.Less(chip.Plate.worldBound.height,40f,"…its plate is smaller");Inside(chip.worldBound,board.worldBound,"A chip fits the "+width+" board");}
   Assert.Greater(chips.Select(c=>Mathf.RoundToInt(c.worldBound.yMin)).Distinct().Count(),1,"Nine chips wrap onto more than one row at "+width);Assert.Less(credits.worldBound.height,40f,"A chip that only reads is as tall as its plate");
   var shape=chips[0].Plate;Assert.IsTrue(shape.customStyle.TryGetValue(new CustomStyleProperty<float>("--vb-bw"),out var bw)&&bw==0,"A chip's plate has no edge");}
 }

 // ds#31: no literal asterisks on a live panel.
 [UnityTest] public IEnumerator EmphasisMarkupRendersWithNoLiteralAsterisks(){
  yield return Boot(390);var page=Ui.Box(board,"vb-page");var op=new OperativeView{name="Aria",role="Striker",passive="Momentum Tax",passiveText="Each hit adds one *Taxed* stack; **three** slow.",skills=new[]{new SkillView{description="Marks the *Taxed*."}}};
  new UnitDetail().Into(page).Bind(op,1,false);new EffectDetails().Into(page).Bind(new EffectInstance{id="t",definition=new EffectDefinition{id="t",title="Taxed",description="One *Taxed* stack slows."},remainingSeconds=3});
  new PerkDetail().Into(page).Bind(new PerkView{name="Quickdraw",effect="*Faster* skills",next="+12%"},true);new ChoiceCard().Into(page).Bind(new PerkView{name="Quickdraw",effect="*Faster* skills"},false);yield return Frames();
  var texts=board.Query<Label>().ToList().Where(Shown).Select(l=>l.text).ToList();Assert.IsTrue(texts.Any(t=>t.Contains("Taxed")),"The text is there");foreach(var t in texts)StringAssert.DoesNotContain("*",t);}

 // ds#29: at 320 the squad tiles show the role as an icon (no cut text); the roster takes the free height.
 [UnityTest] public IEnumerator SquadTilesDropTheRoleTextAt320AndTheRosterFillsTheBoard(){
  foreach(int width in new[]{320,390,430}){
   yield return Boot(width);var page=Ui.Box(board,"vb-page");page.style.flexGrow=1;var view=new SquadView().Into(page);view.style.flexGrow=1;
   var roster=Enumerable.Range(0,8).Select(i=>new OperativeView{id="o"+i,name="Operative"+i,role="Striker",maxStability=100}).ToArray();var locked=new string[8];locked[7]="Pull in rifts";
   view.Bind(new SquadViewModel{roster=roster,selected=new[]{0,1,2,3},locked=locked});yield return Frames(8);
   var tiles=view.Query<UnitTile>().ToList().Where(Shown).ToList();Assert.AreEqual(8,tiles.Count);
   foreach(var tile in tiles){var role=tile.Q<Label>(className:"vb-tile__role-text");bool isLocked=tile.ClassListContains("vb-tile--locked");bool visible=Shown(role);
    if(width<380&&!isLocked)Assert.IsFalse(visible,"At 320 an unlocked tile shows its role as an icon only");else Assert.IsTrue(visible,"A role is text above 380, and a locked tile keeps its reason");
    if(visible&&width<380)Inside(role.worldBound,tile.worldBound,$"A locked tile's reason wraps inside its tile at {width}");}
   var start=view.Q<VbButton>("start");float gap=start.worldBound.yMin-tiles.Max(t=>t.worldBound.yMax);Assert.Less(gap,board.worldBound.height*.15f,$"No empty third above START RUN at {width} ({gap:0} units)");}
 }

 // ds#23/#25: the first line of the cancel band is the aim hint and it fits; the toggle's state label fits its button.
 [UnityTest] public IEnumerator TheHudToggleAndTheCancelBandFitAtEveryWidth(){
  foreach(int width in new[]{320,390,430}){
   yield return Boot(width);var battle=new BattleHudView().Into(board);battle.style.flexGrow=1;var hud=new HudView{auto=false,hint="Needle Storm · −18 · replaces auto"};
   battle.Aiming(true);battle.Bind(hud,new[]{Operative("Aria",new SkillView(),new SkillView())});yield return Frames();
   var auto=battle.Q("auto");var label=auto.Query<Label>().ToList().First(Shown);Assert.AreEqual("MANUAL",label.text);Inside(label.worldBound,auto.worldBound,$"MANUAL fits its toggle at {width}");Inside(auto.worldBound,board.worldBound,"The toggle is on the board");
   Assert.GreaterOrEqual(Mathf.Min(auto.worldBound.width,auto.worldBound.height),44.5f,"The toggle is still a target");
   var band=battle.CancelBand;Assert.IsTrue(Shown(band));Inside(band.worldBound,board.worldBound,"The cancel band is on the board");var size=band.MeasureTextSize(band.text,band.contentRect.width,VisualElement.MeasureMode.Exactly,0,VisualElement.MeasureMode.Undefined);Assert.LessOrEqual(size.y,band.contentRect.height+.5f,"Both lines of the band fit it at "+width);}
 }
}
}
