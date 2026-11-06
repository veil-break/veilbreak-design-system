using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;
namespace Veilbreak.Gui.Tests {
/// <summary>gui 0.9.1 (design-system#69) that needs no live panel: the choice card as a compact cell for the perks a run owns and for the merchant, the results title, and the motion recipes' data and poses. What needs a clock or a layout is in
/// <c>MotionLifecycleTests</c>, <c>PolishMotionTests</c> and <c>CloseOutFitTests</c>.</summary>
public sealed class CloseOutTests {
 static string Sheet(string name)=>File.ReadAllText(PackageFiles.Path("Runtime/Styles/"+name));
 static string[] Texts(VisualElement e)=>e.Query<Label>().ToList().Where(l=>l.style.display!=DisplayStyle.None).Select(l=>l.text).ToArray();

 [Test] public void TheMotionPassHasARecipeForEachOfItsPieces(){
  foreach(var recipe in new[]{MotionRecipes.DrawerFit,MotionRecipes.LabelMerge,MotionRecipes.LabelSplit,MotionRecipes.DragLift,MotionRecipes.DropSettle,MotionRecipes.PressSink,MotionRecipes.Fallen}){Assert.Greater(recipe.milliseconds,0);Assert.LessOrEqual(recipe.milliseconds,500,"A short beat, not a show");}
  Assert.Greater(MotionRecipes.LabelMerge.scale,1,"A merge swells");Assert.Less(MotionRecipes.LabelSplit.scale,1,"A split shrinks");Assert.Greater(MotionRecipes.DragLift.scale,1);Assert.Less(MotionRecipes.DragLift.y,0,"A lifted ghost rises");
  Assert.Less(MotionRecipes.PressSink.scale,1);Assert.Greater(MotionRecipes.PressSink.y,0,"A pressed skill sinks");Assert.AreEqual(VbTokens.DurPress,MotionRecipes.PressSink.milliseconds,"…on the press beat");Assert.AreEqual(VbTokens.DurUi,MotionRecipes.DropSettle.milliseconds,"A drop settles on the UI beat");
  Assert.Greater(MotionRecipes.Fallen.y,0);Assert.Less(MotionRecipes.Fallen.scale,1);}
 [Test] public void ALabelPoseSwellsAndEndsAtRestAndEachReductionDropsItsPart(){
  var recipe=MotionRecipes.LabelMerge;float total=recipe.milliseconds/1000f;
  WorldLayer.LabelPose(true,total,false,false,out float s0,out float o0);Assert.AreEqual(1,s0,1e-4f,"It starts at rest");Assert.AreEqual(.55f,o0,1e-4f,"…with the flash dim");
  WorldLayer.LabelPose(true,total/2,false,false,out float s1,out float o1);Assert.AreEqual(recipe.scale,s1,1e-4f,"Fullest half way");Assert.Less(o1,1);
  WorldLayer.LabelPose(true,0,false,false,out float s2,out float o2);Assert.AreEqual(1,s2,1e-4f);Assert.AreEqual(1,o2,1e-4f,"It ends at rest");
  WorldLayer.LabelPose(true,total/2,true,false,out float s3,out float o3);Assert.AreEqual(1,s3,"Reduced motion: no scale");Assert.Less(o3,1,"…the fade stays");
  WorldLayer.LabelPose(true,total/2,false,true,out float s4,out float o4);Assert.AreEqual(recipe.scale,s4,1e-4f,"Reduced flashes: the scale stays");Assert.AreEqual(1,o4,"…no dip");
  WorldLayer.LabelPose(false,total/2,false,true,out float s5,out _);Assert.AreEqual(MotionRecipes.LabelSplit.scale,s5,1e-4f,"A split shrinks");}

 // ── the choice card as a compact cell ──
 [Test] public void AnOwnedPerkIsACompactChoiceCardWithItsLevels(){
  var card=new ChoiceCard().BindOwned(new PerkView{id="a",name="Overclock",icon="burst",tag="Damage",now="+12% damage",stack=2,max=3,tier=PerkTier.Epic},true);
  Assert.IsTrue(card.Compact);Assert.IsTrue(card.Selected);Assert.IsTrue(card.ClassListContains("vb-perk--epic"));Assert.AreEqual("OVERCLOCK",card.Title);Assert.AreEqual("+12% damage",card.Text);Assert.AreEqual("",card.Tag,"No tag in a cell");Assert.AreEqual("",card.ActionText);Assert.AreEqual("",card.Next);
  card.BindOwned(new PerkView{id="a",name="Overclock",tier=PerkTier.Rare,now="+12%",stack=3,max=0},false);Assert.IsFalse(card.Selected);Assert.IsFalse(card.ClassListContains("vb-perk--epic"),"Rarity rebinding removes the stale tier");Assert.AreEqual("+12% ×3",card.Text,"A repeatable perk counts its stacks");}
 [Test] public void AnOfferCardSaysBuyWithThePriceAndDimsInsteadOfRepeatingIt(){
  var perk=new PerkView{id="q",name="Quickdraw",effect="−12% cooldown",icon="haste"};var card=new ChoiceCard();
  card.BindOffer(new MerchantOfferView{perk=perk,price=80},2,100);Assert.AreEqual("buy-2",card.Command);Assert.AreEqual("BUY 80",card.ActionText);Assert.IsFalse(card.Dimmed);Assert.IsTrue(card.enabledSelf);Assert.AreEqual("−12% cooldown",card.Text);Assert.IsTrue(card.Compact);
  card.BindOffer(new MerchantOfferView{perk=perk,price=80},2,50);Assert.IsTrue(card.Dimmed,"Short of credits");Assert.IsFalse(card.enabledSelf,"…out of play");Assert.AreEqual("BUY 80",card.ActionText,"…and no second price");
  card.BindOffer(new MerchantOfferView{perk=perk,price=80,taken=true},2,500);Assert.AreEqual("SOLD",card.ActionText);Assert.IsTrue(card.Dimmed);Assert.IsFalse(card.enabledSelf);
  card.BindOffer(new MerchantOfferView{perk=perk,price=80,blocked="Stack full"},2,500);Assert.AreEqual("STACK FULL",card.ActionText,"A game's reason takes the action word's place");Assert.IsFalse(card.enabledSelf);
  card.BindOffer(new MerchantOfferView{perk=perk,price=80},2,500);Assert.IsFalse(card.Dimmed,"Funded again");Assert.IsTrue(card.enabledSelf);}
 [Test] public void ASaleCardSaysSellWithThePayout(){
  var card=new ChoiceCard().BindSale(new MerchantStackView{perk=new PerkView{id="q",name="Quickdraw",now="−12%",icon="haste"},payout=30});Assert.AreEqual("sell-perk-q",card.Command);Assert.AreEqual("SELL +30",card.ActionText);Assert.AreEqual("−12%",card.Text);Assert.IsTrue(card.enabledSelf);Assert.IsTrue(card.Compact);}
 [Test] public void ActivePerksAreChoiceCardsInATwoColumnGridAndPickingOneReportsItsIndex(){
  var view=new ActivePerksView().Bind(new ActivePerksViewModel{wave=3,pick=1,perks=new[]{new PerkView{id="a",name="Ward",stack=1,max=3,now="+8"},new PerkView{id="b",name="Quickdraw",stack=2,max=3,now="−12%"}}});
  var cards=view.Query<ChoiceCard>().ToList();Assert.AreEqual(2,cards.Count);Assert.IsTrue(cards.All(c=>c.Compact));Assert.IsFalse(cards[0].Selected);Assert.IsTrue(cards[1].Selected,"The picked one is framed");Assert.IsNull(view.Q<PerkRow>(),"No list rows");
  int picked=-1;view.Picked+=i=>picked=i;cards[0].Pick();Assert.AreEqual(0,picked);cards[1].Pick();Assert.AreEqual(1,picked);
  var again=view.Query<ChoiceCard>().ToList();view.Bind(new ActivePerksViewModel{wave=3,pick=0,perks=new[]{new PerkView{id="b",name="Quickdraw",stack=2,max=3},new PerkView{id="a",name="Ward",stack=1,max=3}}});Assert.AreSame(again[0],view.Q<ChoiceCard>("a"),"Keyed cards are reused");
  Assert.IsTrue(view.Q<ChoiceCard>("b").Selected,"Pick 0 is now b");Assert.IsFalse(view.Q<ChoiceCard>("a").Selected);}
 [Test] public void TheCompactAndDimmedCardsAreStyledAndTheGridCellsHaveTheirWidth(){
  var gui=Sheet("Gui.uss");StringAssert.Contains(".vb-choice--compact {",gui);StringAssert.Contains(".vb-choice--dimmed {",gui);StringAssert.Contains(".vb-skill.vb-skill--press-flash",gui);
  StringAssert.Contains(".vb-build__grid > .vb-choice { width: 48.5%; }",Sheet("Views.uss"));StringAssert.Contains(".vb-draft__squad > .vb-label { margin-bottom: 7px; white-space: normal; }",Sheet("Views.uss"),"The carry-forward line wraps");}

 // ── the results title ──
 [Test] public void TheResultsTitleIsTheViewModelsAndDefaultsToRunOver(){
  var model=new ResultsViewModel();Assert.AreEqual("RUN OVER",model.title);var view=new ResultsView().Bind(model);var title=view.Query<Label>().ToList().First(l=>l.ClassListContains("vb-t-display"));Assert.AreEqual("RUN OVER",title.text);
  model.title="Victory";view.Bind(model);Assert.AreEqual("VICTORY",title.text,"Set in caps by the view");model.title="";view.Bind(model);Assert.AreEqual("RUN OVER",title.text,"Empty keeps the default");model.title=null;view.Bind(model);Assert.AreEqual("RUN OVER",title.text);}
}
}
