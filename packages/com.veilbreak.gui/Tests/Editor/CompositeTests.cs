using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;
namespace Veilbreak.Gui.Tests {
/// <summary>The generic composites of gui 0.8.0 (design-system#39): the unit card, skill button, effect details, choice card, stat row, chip row, the markup formatter and the screen views' text cuts.
/// What needs a live layout (clipping, wrapping, hit areas) is in <c>CompositeFitTests</c>.</summary>
public sealed class CompositeTests {
 static string[] Texts(VisualElement e)=>e.Query<Label>().ToList().Where(l=>l.style.display!=DisplayStyle.None).Select(l=>l.text).ToArray();
 static EffectInstance Status(string id,float? seconds,EffectPolarity polarity=EffectPolarity.Harm)=>new EffectInstance{id=id,definition=new EffectDefinition{id=id,title=id,icon="slow",polarity=polarity},remainingSeconds=seconds};

 // ds#20: the card carries the name only, no effect row, a chip only for a non-default mode and one FALLEN.
 [Test] public void UnitCardDropsTheCodeTheEffectRowAndTheAutoChip(){
  var card=new UnitCard().Bind(new OperativeView{name="Aria",code="AR",hp=900,maxHp=1000});
  Assert.IsNull(card.Q(className:"vb-operative__code"),"No code over the name");Assert.IsNull(card.Q<EffectStrip>(),"No effect-count row");Assert.IsFalse(Texts(card).Contains("AR"));Assert.IsFalse(Texts(card).Contains("AUTO"),"AUTO is the HUD toggle's to say");
  var chip=card.Q<Label>(className:"vb-operative__chip");Assert.AreEqual(DisplayStyle.None,chip.style.display.value,"No chip for the default mode");
  card.Bind(new OperativeView{name="Aria",hp=900,maxHp=1000,mode=ControlMode.Manual,manualSeconds=3});Assert.AreEqual("MANUAL 3.0s",chip.text);Assert.AreEqual(DisplayStyle.Flex,chip.style.display.value);
  card.Bind(new OperativeView{name="Aria",hp=900,maxHp=1000,mode=ControlMode.Aiming});Assert.AreEqual("AIMING",chip.text);
  card.Bind(new OperativeView{name="Aria",hp=900,maxHp=1000,mode=ControlMode.Auto});Assert.AreEqual(DisplayStyle.None,chip.style.display.value);}
 [Test] public void AFallenCardSaysFallenOnceAndTheReviveHintOnlyWithARevive(){
  var card=new UnitCard().Bind(new OperativeView{name="Nyra",mode=ControlMode.Fallen});var hint=card.Q<Label>(className:"vb-operative__fallen-hint");
  Assert.AreEqual(1,Texts(card).Count(t=>t.Contains("FALLEN")),"One FALLEN, not a chip and a body");Assert.AreEqual(DisplayStyle.None,hint.style.display.value,"No revive owned: no hint");
  card.Bind(new OperativeView{name="Nyra",mode=ControlMode.Fallen,canRevive=true});Assert.AreEqual(DisplayStyle.Flex,hint.style.display.value);
  Assert.AreEqual(DisplayStyle.None,card.Statuses.style.display.value,"A fallen card shows no statuses");}
 // ds#20/#25: at most three timed statuses as icons; passives and perks (no time) are not statuses.
 [Test] public void StatusPipsShowAtMostThreeTimedStatusesAndNeverAPassive(){
  var pips=new StatusPips();var passive=Status("passive:momentum",null,EffectPolarity.Benefit);pips.Bind(new[]{passive});Assert.AreEqual(DisplayStyle.None,pips.style.display.value,"A passive alone shows nothing");Assert.AreEqual(0,pips.Timed.Count);
  pips.Bind(new[]{Status("a",4),passive,Status("b",2),Status("c",1),Status("d",9)});Assert.AreEqual(4,pips.Timed.Count,"All timed statuses are in the list");
  Assert.AreEqual(Math.Min(StatusPips.Max,pips.Timed.Count),pips.Query<VbIcon>().ToList().Count(i=>i.style.display!=DisplayStyle.None),"…and three icons show");Assert.IsFalse(pips.Timed.Contains(passive));
  Assert.AreEqual(PickingMode.Position,pips.pickingMode,"The strip is one target");Assert.IsNotNull(pips.Details);Assert.IsTrue(Status("t",1).Timed);Assert.IsFalse(passive.Timed);}
 [Test] public void UnitCardListsNoPassiveAsAStatus(){var card=new UnitCard().Bind(new OperativeView{name="Aria",hp=10,maxHp=10,effects=new[]{Status("passive:momentum-tax",null,EffectPolarity.Benefit),Status("slowed",3)}});
  Assert.AreEqual(1,card.Statuses.Timed.Count);Assert.AreEqual("slowed",card.Statuses.Timed[0].id);}
 // ds#25: the effect detail prints only what is known.
 [Test] public void EffectDetailsPrintNoPlaceholders(){
  var bare=new EffectDetails().Bind(new EffectInstance{id="x",definition=new EffectDefinition{id="x",title="Momentum Tax",description="Each *Taxed* stack slows."}});var texts=Texts(bare);
  foreach(var placeholder in new[]{"unavailable","persistent","Actor","Active"})Assert.IsFalse(texts.Any(t=>t.Contains(placeholder)),"No \""+placeholder+"\": "+string.Join(" | ",texts));
  Assert.AreEqual("MOMENTUM TAX",bare.Q<Label>(className:EffectDetails.TitleClass).text);Assert.IsFalse(texts.Any(t=>t.Contains("*")),"Emphasis renders, it is not printed");
  var full=new EffectDetails().Bind(new EffectInstance{id="y",source="Aria",magnitude="−20%",remainingSeconds=2.5f,definition=new EffectDefinition{id="y",title="Slowed"}});texts=Texts(full);
  Assert.IsTrue(texts.Contains("Source · Aria"));Assert.IsTrue(texts.Any(t=>t.StartsWith("2.5s remaining")));Assert.IsTrue(texts.Contains("−20%"));}
 // ds#21: one layout: the glyph and a cost line; the states are overlays.
 [Test] public void SkillButtonKeepsOneLayoutAndShowsStatesAsOverlays(){
  var b=new SkillButton();
  b.Bind(new SkillView{state=SkillState.Ready,cost=18});Assert.AreEqual("18",b.Value.text);Assert.AreEqual("",b.Badge.text);Assert.AreEqual("",b.Seconds.text);
  b.Bind(new SkillView{state=SkillState.Short,cost=45,need=3});Assert.AreEqual("45",b.Value.text,"The cost stays");Assert.AreEqual("−3",b.Badge.text,"The shortfall is a corner badge");Assert.AreEqual(DisplayStyle.Flex,b.Badge.style.display.value);
  b.Bind(new SkillView{state=SkillState.Queued,cost=45,queue=1});Assert.AreEqual("#1",b.Badge.text);Assert.AreEqual("45",b.Value.text);
  b.Bind(new SkillView{state=SkillState.Cooldown,cost=18,seconds=4.2f,charged=.5f});Assert.AreEqual("4.2s",b.Seconds.text,"The seconds count over the glyph");Assert.AreEqual("18",b.Value.text);
  b.Bind(new SkillView{state=SkillState.Fallen,cost=18});Assert.AreEqual(DisplayStyle.None,b.Value.style.display.value,"A fallen skill has no cost line");
  foreach(SkillState s in Enum.GetValues(typeof(SkillState))){b.Bind(new SkillView{state=s,cost=18,need=5,queue=2,seconds=3,reason="Out of range"});Assert.IsFalse(Texts(b).Any(t=>t=="NEED"||t=="QUEUE"||t=="CAST"||t=="AIM"),"No caption words: "+s);}}
 // ds#31: one formatter for emphasis.
 [Test] public void MarkupTurnsAsterisksIntoEmphasisAndLeavesStrayOnesAlone(){
  Assert.AreEqual("one <color="+VbTokens.Hex(VbTokens.Text)+">Taxed</color> stack",Markup.Rich("one *Taxed* stack"));Assert.AreEqual("one Taxed stack",Markup.Plain("one *Taxed* stack"));
  Assert.AreEqual("a Taxed b",Markup.Plain("a **Taxed** b"));Assert.AreEqual("5 * 3 and 2 * 4",Markup.Rich("5 * 3 and 2 * 4"),"Spaced asterisks are arithmetic");Assert.AreEqual("a *b",Markup.Plain("a *b"),"An unpaired mark stays");
  Assert.AreEqual("no marks",Markup.Rich("no marks"));Assert.AreSame("no marks",Markup.Rich("no marks"),"Text without a mark is returned as it came");Assert.AreEqual("",Markup.Rich(null));
  Assert.IsFalse(Markup.Rich("*a* and *b* and *c*").Contains("*"));}
 [Test] public void EveryDescriptionLabelFormatsEmphasis(){
  var detail=new UnitDetail().Bind(new OperativeView{name="Aria",passive="Momentum Tax",passiveText="Each hit adds one *Taxed* stack.",passiveFullText="*Taxed* stacks slow.",skills=new[]{new SkillView{description="Deals *double* damage."}}},0,false);
  Assert.IsFalse(Texts(detail).Any(t=>t.Contains("*")),string.Join(" | ",Texts(detail)));var passiveRow=detail.Query<SkillSummary>().ToList()[0];Assert.IsFalse(passiveRow.tooltip.Contains("*"),"The passive is the first row of the sheet's list");
  var perk=new PerkDetail().Bind(new PerkView{name="Quickdraw",effect="*Faster* skills",now="",next="+12%"},true);Assert.IsFalse(Texts(perk).Any(t=>t.Contains("*")));
  var card=new ChoiceCard().Bind(new PerkView{name="Quickdraw",effect="*Faster* skills"},false);Assert.IsFalse(Texts(card).Any(t=>t.Contains("*")));}
 // ds#34: a pair mirrors; rank cells; a perk says each thing once.
 [Test] public void AChoicePairMirrorsItsCutsAndAlignment(){
  var route=new RouteChoice().Bind(new RouteView{left=new RoadView{kind="merchant",title="Night market",rank=0},right=new RoadView{kind="elite",title="Elite patrol",rank=3,ranks=5}});var cards=route.Query<ChoiceCard>().ToList();Assert.AreEqual(2,cards.Count);
  Assert.AreEqual(ChoiceSide.Left,cards[0].Side);Assert.AreEqual(ChoiceSide.Right,cards[1].Side);Assert.IsTrue(cards[0].ClassListContains("vb-choice--left"));Assert.IsTrue(cards[1].ClassListContains("vb-choice--right"));
  Assert.AreEqual("route-left",cards[0].name);Assert.AreEqual("route-right",cards[1].name);Assert.AreEqual(TextTone.Power,cards[0].Tone);Assert.AreEqual(TextTone.Enemy,cards[1].Tone);
  Assert.IsFalse(Texts(route).Any(t=>t.StartsWith("Two roads")),"No subtitle");Assert.IsFalse(Texts(route).Any(t=>t.Contains("Threat")),"Threat is rank cells, not text");
  var cells=cards[1].Query(className:"vb-progress__cell").ToList().Where(c=>c.style.display!=DisplayStyle.None).ToList();Assert.AreEqual(5,cells.Count);Assert.AreEqual(3,cells.Count(c=>c.ClassListContains("vb-progress__cell--on")));
  Assert.AreEqual(DisplayStyle.None,cards[0].Q<ProgressBar>().style.display.value,"A road with no rank shows no cells");Assert.IsTrue(Texts(cards[0]).Contains("TAKE THIS ROAD"),"The action label sits at the foot of the card");}
 [Test] public void APerkChoiceShowsItsEffectOnceAndItsLevelsAsCells(){
  var card=new ChoiceCard().Bind(new PerkView{name="Crossfire Lattice",tag="Rare",effect="Shots chain.",next="Shots chain.",stack=1,max=5,tier=PerkTier.Rare,icon="burst"},true);
  Assert.AreEqual(1,Texts(card).Count(t=>t=="Shots chain."),"The effect is not repeated in a next block");Assert.IsTrue(card.Selected);Assert.IsTrue(card.ClassListContains("vb-perk--rare"));
  Assert.AreEqual(5,card.Query(className:"vb-progress__cell").ToList().Count(c=>c.style.display!=DisplayStyle.None));
  card.Bind(new PerkView{name="Ward",effect="Barrier.",next="+8 ward",stack=0,max=5},false);Assert.IsFalse(card.Selected);Assert.IsTrue(Texts(card).Contains("+8 ward"));}
 [Test] public void PerkDetailListsOnlyNowAndAfterPick(){
  var draft=new PerkDetail().Bind(new PerkView{name="Ward",effect="Barrier.",now="None",next="+8 ward",stack=0,max=5},true);var keys=draft.Query<StatRow>().ToList().Select(r=>r.Key).ToArray();CollectionAssert.AreEqual(new[]{"After pick"},keys,"\"Now · None\" is no row");
  draft.Bind(new PerkView{name="Ward",effect="Barrier.",now="+8 ward",next="+16 ward",stack=1,max=5},true);CollectionAssert.AreEqual(new[]{"Now","After pick"},draft.Query<StatRow>().ToList().Select(r=>r.Key).ToArray());
  Assert.IsInstanceOf<KeyValueRow>(draft.Q<StatRow>(),"A StatRow is a KeyValueRow, so hosts that read one still read this");}
 // ds#35: stat rows; offers dim instead of repeating a price; empty sections hide.
 [Test] public void StatRowKeepsItsKeyAndHoldsAnAction(){
  var row=new StatRow("Health","1,240",true).Classes("probe");Assert.AreEqual("Health",row.Key);Assert.AreEqual("1,240",row.Value);Assert.IsTrue(row.Good);Assert.AreSame(row.KeyLabel.hierarchy.parent,row.ValueLabel.hierarchy.parent,"Key and value share one column");
  Assert.AreEqual(DisplayStyle.None,row.Trail.style.display.value);var button=new VbButton("BUY");row.WithAction(button);Assert.AreEqual(DisplayStyle.Flex,row.Trail.style.display.value);Assert.IsTrue(row.Trail.Fixed,"Trailing actions share one width");
  row.Dimmed=true;Assert.IsTrue(row.ClassListContains("vb-stat-row--dimmed"));row.Icon="health";Assert.AreEqual("health",row.Q<VbIcon>().Icon);}
 [Test] public void MerchantOffersDimInsteadOfRepeatingThePriceAndEmptySectionsHide(){
  var screen=new MerchantScreen().Bind(new MerchantView{credits=90,offers=new[]{new MerchantOfferView{perk=new PerkView{id="a",name="Quickdraw",effect="−12% cooldown",icon="add"},price=80},new MerchantOfferView{perk=new PerkView{id="b",name="Fracture",effect="Power",icon="burst"},price=150},new MerchantOfferView{perk=new PerkView{id="c",name="Wind",effect="x"},price=80,taken=true}}});
  var cards=screen.Query<ChoiceCard>().ToList();Assert.AreEqual(3,cards.Count,"Offers are choice cards");Assert.IsTrue(cards.All(c=>c.Compact));Assert.IsTrue(screen.Offer(0).enabledSelf);Assert.IsFalse(screen.Offer(0).Dimmed);Assert.AreEqual("buy-0",screen.Offer(0).Command);
  Assert.IsFalse(screen.Offer(1).enabledSelf);Assert.IsTrue(screen.Offer(1).Dimmed,"A card that cannot be bought dims");Assert.IsFalse(Texts(screen).Any(t=>t.StartsWith("Need")),"No second price: "+string.Join(" | ",Texts(screen)));
  Assert.AreEqual("BUY 150",screen.Offer(1).ActionText);Assert.AreEqual("SOLD",screen.Offer(2).ActionText);
  Assert.IsFalse(Texts(screen).Any(t=>t.Contains("Nothing to sell")),"No empty line");var title=screen.Query<LabelComponent>().ToList().First(l=>l.text=="SELL STACKS");Assert.AreEqual(DisplayStyle.None,title.style.display.value,"An empty section hides");
  screen.Bind(new MerchantView{credits=200,stacks=new[]{new MerchantStackView{perk=new PerkView{id="a",name="Quickdraw",now="−12%",icon="add"},payout=30}}});Assert.AreEqual(DisplayStyle.Flex,title.style.display.value);
  var sell=screen.Sale("a");Assert.IsNotNull(sell);Assert.AreEqual("SELL +30",sell.ActionText,"+30 pays credits");Assert.AreEqual("sell-perk-a",sell.Command);Assert.IsTrue(sell.enabledSelf);}
 [Test] public void MerchantCardsAreReusedAndTheirCommandFollowsTheOfferAfterARebind(){var view=new MerchantView{credits=200,offers=new[]{new MerchantOfferView{id="o1",perk=new PerkView{id="a",name="Quickdraw"},price=80},new MerchantOfferView{id="o2",perk=new PerkView{id="b",name="Wind"},price=80}}};
  var screen=new MerchantScreen();screen.Bind(view);var o2=screen.Q<ChoiceCard>("o2");Assert.AreEqual("buy-1",o2.Command);Assert.AreEqual(1,(int)o2.userData);int bought=-1;screen.Bought+=i=>bought=i;
  view.offers=new[]{view.offers[1],view.offers[0]};screen.Bind(view);Assert.AreSame(o2,screen.Q<ChoiceCard>("o2"),"Keyed cards are reused");Assert.AreEqual("buy-0",o2.Command,"The command is named for the offer's place now");Assert.AreEqual(0,(int)o2.userData,"…and buys that offer");
  Assert.AreSame(o2,screen.Offer(0));o2.Pick();Assert.AreEqual(0,bought,"Picking the card buys the offer it is about now");}
 // ds#36: chips.
 [Test] public void ChipRowIsATintedPlateNotAFrame(){
  var chip=new ChipRow().Bind(new PerkView{name="Overclock",icon="burst",stack=3,tier=PerkTier.Epic,now="+12% damage"});Assert.AreEqual("OVERCLOCK",chip.Text);Assert.AreEqual("×3",chip.Value);Assert.IsTrue(chip.ClassListContains("vb-perk--epic"));
  chip.Bind(new PerkView{name="Overclock",icon="burst",stack=1,tier=PerkTier.Rare});Assert.IsFalse(chip.ClassListContains("vb-perk--epic"));Assert.IsTrue(chip.ClassListContains("vb-perk--rare"));Assert.AreEqual(DisplayStyle.None,chip.Q<Label>(className:"vb-chip-row__count").style.display.value);
  Assert.IsNotNull(chip.Details,"A perk chip opens its detail");Assert.IsTrue(chip.Plate.ClassListContains("vb-shape"));Assert.IsFalse(chip.Static);
  var credits=ChipRow.Credits().SetNumber(40);Assert.IsTrue(credits.Static);Assert.AreEqual(PickingMode.Ignore,credits.pickingMode);Assert.AreEqual("currency",credits.Icon);}
 // The edges: a pair's cuts.
 [Test] public void ShapeCutsTheTwoCornersOfItsSide(){
  CollectionAssert.AreEqual(new[]{new Vector2(10,0),new Vector2(100,0),new Vector2(100,40),new Vector2(10,40),new Vector2(0,30),new Vector2(0,10)},VbShape.Outline(100,40,10,0,VbShape.Left),"left: top-left and bottom-left");
  CollectionAssert.AreEqual(new[]{new Vector2(0,0),new Vector2(90,0),new Vector2(100,10),new Vector2(100,30),new Vector2(90,40),new Vector2(0,40)},VbShape.Outline(100,40,10,0,VbShape.Right),"right: top-right and bottom-right");
  CollectionAssert.AreEqual(VbShape.Outline(100,40,10,0),VbShape.Outline(100,40,10,0,0),"the default cut is unchanged");StringAssert.Contains("--vb-cut-side: 0",PackageFiles.Read("Runtime/Styles/Gui.uss"),"Shapes reset the side: custom properties inherit");}
 // ds#29, ds#33, ds#37: the screen views' text.
 [Test] public void TheSquadScreenCutsItsCaptionsAndHidesStabilityUntilItVaries(){
  var ops=Enumerable.Range(0,8).Select(i=>new OperativeView{name="Op"+i,role="Striker",maxStability=100}).ToArray();var v=new SquadView().Bind(new SquadViewModel{roster=ops,selected=new[]{0,1,-1,-1}});
  var texts=Texts(v);Assert.IsFalse(texts.Any(t=>t.Contains("TAP A SLOT")),"No instruction caption");Assert.IsFalse(texts.Any(t=>t.Contains("ALL 8 UNLOCKED")),"No roster caption when all are unlocked");
  v.Bind(new SquadViewModel{roster=ops,selected=new[]{0,1,-1,-1},locked=new[]{"","","","","Pull in rifts","","",""}});Assert.IsTrue(Texts(v).Any(t=>t.Contains("7 OF 8")),"…but it says so while part of the roster is locked");
  Assert.IsFalse(SquadView.StabilityVaries(ops));ops[3].maxStability=140;Assert.IsTrue(SquadView.StabilityVaries(ops));
  v.Bind(new SquadViewModel{roster=ops,focus=0});var stability=v.DetailsDrawer.Query<StatReadout>().ToList().Last();Assert.AreEqual(DisplayStyle.Flex,stability.style.display.value,"Shown once operatives differ");
  ops[3].maxStability=100;v.Bind(new SquadViewModel{roster=ops,focus=1});Assert.AreEqual(DisplayStyle.None,v.DetailsDrawer.Query<StatReadout>().ToList().Last().style.display.value,"Hidden while every operative reads the same");
}
 [Test] public void UnitSlotsAndTilesDoNotRepeatTheCode(){var slot=new UnitSlot().Bind(new OperativeView{name="Aria",code="AR",role="Striker"},1);Assert.IsFalse(Texts(slot).Contains("AR"));Assert.IsTrue(Texts(slot).Contains("ARIA"));}
 [Test] public void MenuGivesItsActionsOwnGlyphsAndExitIsAnIconButton(){
  var m=new MainMenuView().Bind(new MainMenuViewModel());var scores=m.Q<VbButton>("scores");var settings=m.Q<VbButton>("settings");Assert.AreNotEqual(scores.Icon,settings.Icon,"Distinct glyphs");Assert.IsFalse(string.IsNullOrEmpty(scores.Icon));
  Assert.IsNotNull(m.Q<IconButtonComposite>("exit"),"EXIT is an icon button");Assert.IsNull(m.Q<VbButton>("exit"));Assert.IsNotNull(m.Q(className:"vb-menu__row"),"Hosts add their action to this row");}
 [Test] public void PauseHasNoCaptionNoSubLineAndAnInfoPopoverForTheSeed(){
  var p=new PauseOverlayView().Bind(new PauseViewModel{wave=3,score=10,standing=4,seed="698043EE",detail="rules 7"});Assert.IsFalse(Texts(p).Any(t=>t.Contains("frozen")||t.Contains("Seed")||t.Contains("selected speed")));
  Assert.AreEqual(DisplayStyle.Flex,p.Q<IconButtonComposite>("run-info").style.display.value);Assert.IsNotNull(p.Info);var row=p.Q<ActionRow>();Assert.IsNotNull(row,"EXIT, ABANDON and the settings cog share one action row");Assert.IsNotNull(row.Q<VbButton>("abandon"));Assert.IsNotNull(row.Q<VbButton>("settings"));
  Assert.IsTrue(p.Q<VbButton>("exit").ClassListContains("vb-button--lead"),"EXIT TO MENU leads with ‹, it does not trail a chevron");
  p.Bind(new PauseViewModel{wave=3});Assert.AreEqual(DisplayStyle.None,p.Q<IconButtonComposite>("run-info").style.display.value,"No seed line, no info button");}
 [Test] public void SettingsPutClearScoresInADataSectionAndReducedMotionUnderAccessibility(){
  var s=new SettingsView();var clear=s.Q<VbButton>("clear-scores");Assert.IsNotNull(clear);Assert.AreEqual(ButtonVariant.Danger,clear.Variant);Assert.IsNotNull(clear.GetFirstAncestorOfType<ActionRow>(),"A row of the Data fieldset, not a footer");
  var titles=s.Query<LabelComponent>(className:"vb-section__title").ToList().Select(l=>l.text).ToArray();CollectionAssert.Contains(titles,"DATA");CollectionAssert.Contains(titles,"ACCESSIBILITY");
  var feel=s.Query<VisualElement>(className:"vb-section").ToList().First(e=>e.Q<LabelComponent>(className:"vb-section__title")?.text=="FEEL");Assert.AreEqual(1,feel.Query<VbSwitch>().ToList().Count,"FEEL keeps haptics; reduced motion moved");
  var access=s.Query<VisualElement>(className:"vb-section").ToList().First(e=>e.Q<LabelComponent>(className:"vb-section__title")?.text=="ACCESSIBILITY");Assert.IsNotNull(access.Q<VbSwitch>(),"Reduced motion lives in the Accessibility section");Assert.IsNotNull(access.Q<VbButton>("accessibility"));
  var a=new AccessibilityView();Assert.AreEqual("ACCESSIBILITY",a.Q<Label>(className:"vb-header__title").text);Assert.AreEqual("",a.Q<Label>(className:"vb-header__meta").text);}
 [Test] public void TheTextCutListIsGoneFromTheViews(){
  var all=string.Join("\n",Texts(new HighScoresView().Bind(new[]{new ScoreView{rank=1,score=1}}))
   .Concat(Texts(new RouteChoice().Bind(new RouteView())))
   .Concat(Texts(new ActivePerksView().Bind(new ActivePerksViewModel{perks=new[]{new PerkView{id="a",name="Ward"}},totals=new[]{new LedgerLine{label="DAMAGE",value="+0%"},new LedgerLine{label="HEALTH",value="+12%"}}})))
   .Concat(Texts(new ResultsView().Bind(new ResultsViewModel{score=0,rank=1,squad=new[]{new ContributionView{operative=new OperativeView{id="a",name="Aria"}}}}))));
  foreach(var gone in new[]{"not an online ranking","Two roads. One choice","Percent bonuses add","+0%","STANDING","HOVER OR TAP","#1 OF"})StringAssert.DoesNotContain(gone,all,gone);
  StringAssert.Contains("+12%",all,"A total that says something stays");Assert.IsTrue(ActivePerksView.Zero("+0%"));Assert.IsTrue(ActivePerksView.Zero("0"));Assert.IsFalse(ActivePerksView.Zero("+20%"));Assert.IsFalse(ActivePerksView.Zero("−5%"));
  var draft=new PerkDraftView().Bind(new PerkDraftViewModel{wave=1,choices=new[]{new PerkView{id="a",name="Ward",next="+8 ward"}},pick=0});Assert.AreEqual("Ward → +8 ward · then wave 02",draft.Q<VbButton>("take").Sub,"The sub line is one short line");
  var ranked=new ResultsView().Bind(new ResultsViewModel{score=900,rank=2,of=20});Assert.IsTrue(Texts(ranked).Contains("#2 OF 20"),"A rank on a real score still shows");}
 [Test] public void AimHintGoesToTheCancelBandWhileAiming(){
  var board=new BattleHudView();var hud=new HudView{hint="Needle Storm · −18 · replaces auto"};board.Bind(hud,new[]{new OperativeView{name="Aria"}});Assert.AreEqual("RELEASE TO CANCEL",board.CancelBand.text.Split('\n').Last());
  board.Aiming(true);board.Bind(hud,new[]{new OperativeView{name="Aria"}});Assert.AreEqual("NEEDLE STORM · −18 · REPLACES AUTO\nRELEASE TO CANCEL",board.CancelBand.text);
  Assert.IsFalse(board.Hud.Query<Label>().ToList().Any(l=>l.text.Contains("Needle")),"The Power strip no longer carries the hint while aiming");
  board.Aiming(false);board.Bind(new HudView(),new[]{new OperativeView{name="Aria"}});Assert.AreEqual("RELEASE TO CANCEL",board.CancelBand.text);}
 // The old names still compile and are the new types.
 [Test] public void ObsoleteForwardsAreTheNewTypesUnderTheirOldNames(){
#pragma warning disable CS0618
  Assert.IsInstanceOf<UnitCard>(new OperativeCard());Assert.IsInstanceOf<UnitTile>(new OperativeTile());Assert.IsInstanceOf<UnitSlot>(new SquadSlot());Assert.IsInstanceOf<UnitDetail>(new OperativeDetails());Assert.IsInstanceOf<UnitStats>(new OperativeStats());
  Assert.IsInstanceOf<ResultSummary>(new SquadResult());Assert.IsInstanceOf<ChoiceCard>(new PerkCard());Assert.IsInstanceOf<ChipRow>(new PerkChip().Bind(new PerkView{name="X"}));var credits=new CreditsChip().Set(12);Assert.IsInstanceOf<ChipRow>(credits);
  foreach(var type in new[]{typeof(OperativeCard),typeof(OperativeTile),typeof(SquadSlot),typeof(OperativeDetails),typeof(OperativeStats),typeof(SquadResult),typeof(PerkCard),typeof(PerkChip),typeof(CreditsChip)})Assert.IsTrue(Attribute.IsDefined(type,typeof(ObsoleteAttribute)),type.Name+" is obsolete");
#pragma warning restore CS0618
 }

 // design-system#18: the lane outlives a temporary detach (a move in the hierarchy, a screen swap); only an explicit Dispose retires it. Synchronous: Say sets the text at once, whatever the clock does.
 [Test] public void TheAnnouncerSurvivesATemporaryPanelDetach(){
  var settings=ScriptableObject.CreateInstance<PanelSettings>();var go=new GameObject("Announcer detach test");
  try{
   var document=go.AddComponent<UIDocument>();document.panelSettings=settings;var host=document.rootVisualElement;var text=new GlitchText().Into(host);var lane=new Announcer(text);
   Assert.IsNotNull(text.panel,"The lane starts on a live panel");lane.Say("ONE");lane.Say("QUEUED");Assert.AreEqual("ONE",text.text);
   text.RemoveFromHierarchy();Assert.IsNull(text.panel);Assert.AreEqual(DisplayStyle.None,text.style.display.value,"A detach retires the notice that was showing");
   lane.Say("WHILE AWAY");Assert.AreNotEqual("WHILE AWAY",text.text,"Off a panel the notice waits rather than stalling the lane");
   host.Add(text);Assert.AreEqual("QUEUED",text.text,"Re-attached, the notices that waited are shown in order");
   lane.Clear();text.RemoveFromHierarchy();lane.Say("AWAY");lane.Say("AWAY");Assert.AreNotEqual("AWAY",text.text,"Said while away, a notice waits");host.Add(text);Assert.AreEqual("AWAY",text.text,"A notice said while away shows on re-attach");
   lane.Clear();lane.Say("BREACH",true);Assert.AreEqual("BREACH",text.text);text.RemoveFromHierarchy();host.Add(text);Assert.AreEqual("BREACH",text.text,"An alert on show when the lane was detached plays again");
   lane.Clear();text.RemoveFromHierarchy();lane.Say("ALARM",true);Assert.AreNotEqual("ALARM",text.text);host.Add(text);Assert.AreEqual("ALARM",text.text,"An alert said while away shows on re-attach");
   lane.Clear();lane.Say("TWO");Assert.AreEqual("TWO",text.text,"The lane answers after the detach");
   text.RemoveFromHierarchy();host.Add(text);text.RemoveFromHierarchy();host.Add(text);lane.Clear();lane.Say("THREE");Assert.AreEqual("THREE",text.text,"Repeated detaches leave it working");
   lane.Dispose();text.text="";text.RemoveFromHierarchy();host.Add(text);lane.Say("FOUR");Assert.AreEqual("",text.text,"An explicit Dispose is final, and a re-attach does not wake it");
  }finally{UnityEngine.Object.DestroyImmediate(go);UnityEngine.Object.DestroyImmediate(settings);}
 }
}
}
