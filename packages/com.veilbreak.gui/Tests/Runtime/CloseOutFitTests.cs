using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using MeasureMode=UnityEngine.UIElements.VisualElement.MeasureMode;
namespace Veilbreak.Gui.Tests {
/// <summary>gui 0.9.1 (design-system#69) on live portrait boards at 320, 390 and 430 units: no label of the perk draft is clipped or breaks a word (the 320 report), and the compact choice cards of the active perks and the merchant fit the board,
/// fill their cells and are targets of at least 44 units.</summary>
public sealed class CloseOutFitTests {
 GameObject host;PanelSettings settings;RenderTexture target;VisualElement board;
 [TearDown] public void Close(){if(host!=null)Object.Destroy(host);if(settings!=null)Object.Destroy(settings);if(target!=null)target.Release();}
 IEnumerator Boot(int width){
#if UNITY_EDITOR
  Close();int height=Mathf.RoundToInt(width*2.164f);target=new RenderTexture(width,height,0);
  var theme=UnityEditor.AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>("Packages/com.veilbreak.gui/Runtime/Styles/VeilbreakGui.tss");Assert.IsNotNull(theme);
  settings=ScriptableObject.CreateInstance<PanelSettings>();settings.themeStyleSheet=theme;settings.scaleMode=PanelScaleMode.ConstantPixelSize;settings.scale=1;settings.targetTexture=target;
  host=new GameObject("Close-out board");var document=host.AddComponent<UIDocument>();document.panelSettings=settings;yield return null;
  board=Ui.Box(document.rootVisualElement,"vb-root");board.style.flexGrow=1;if(width<380)board.AddToClassList("vb-compact");VbFluid.Apply(board,width);
#else
  Assert.Ignore("Needs the editor asset database");yield break;
#endif
 }
 static IEnumerator Frames(int n=5){for(int i=0;i<n;i++)yield return null;}
 static bool Shown(VisualElement e){for(var p=e;p!=null;p=p.parent)if(p.resolvedStyle.display==DisplayStyle.None)return false;return true;}
 static string Describe(VisualElement e){var s=e.GetType().Name;if(e.GetClasses().FirstOrDefault() is string c)s+="."+c;return s;}
 /// <summary>What the player would see cut: a label whose lines need more height than it has, a nowrap label wider than its box, a word wider than its box (it would break mid-word) or a label outside <paramref name="within"/>.</summary>
 static List<string> Clipped(VisualElement root,Rect within,System.Func<VisualElement,bool> skip=null){
  var problems=new List<string>();
  foreach(var label in root.Query<TextElement>().ToList()){
   if(!Shown(label)||string.IsNullOrEmpty(label.text)||skip!=null&&skip(label))continue;
   float w=label.contentRect.width,h=label.contentRect.height;string what=Describe(label)+" \""+label.text+"\"";
   if(!(w>0)||!(h>0)){problems.Add(what+" has an empty box");continue;}
   var wrapped=label.MeasureTextSize(label.text,w,MeasureMode.Exactly,0,MeasureMode.Undefined);if(wrapped.y>h+1.5f)problems.Add(what+" needs "+wrapped.y.ToString("0.#")+" high in "+h.ToString("0.#")+" (box "+w.ToString("0.#")+" wide, "+label.resolvedStyle.fontSize.ToString("0.#")+" px, natural "+label.MeasureTextSize(label.text,0,MeasureMode.Undefined,0,MeasureMode.Undefined).x.ToString("0.#")+")");
   bool nowrap=label.resolvedStyle.whiteSpace==WhiteSpace.NoWrap;var natural=label.MeasureTextSize(label.text,0,MeasureMode.Undefined,0,MeasureMode.Undefined);
   if(nowrap&&label.resolvedStyle.textOverflow!=TextOverflow.Ellipsis&&natural.x>w+1.5f)problems.Add(what+" is "+natural.x.ToString("0.#")+" wide in "+w.ToString("0.#"));
   if(!nowrap)foreach(var word in label.text.Split(' ')){if(word.Length<2)continue;var size=label.MeasureTextSize(word,0,MeasureMode.Undefined,0,MeasureMode.Undefined);if(size.x>w+1.5f)problems.Add(what+": \""+word+"\" is "+size.x.ToString("0.#")+" wide in "+w.ToString("0.#")+" and breaks");}
   var wb=label.worldBound;if(wb.xMin<within.xMin-.5f||wb.xMax>within.xMax+.5f)problems.Add(what+" leaves the board ("+wb.xMin.ToString("0.#")+".."+wb.xMax.ToString("0.#")+")");
  }
  return problems;
 }

 static PerkView[] LongPerks()=>new[]{
  new PerkView{id="a",name="Overclock Matrix",tag="Operative",category="Damage",effect="*+12%* damage to marked targets for four seconds",next="+24% damage to marked targets",now="+12%",stack=1,max=3,icon="burst",tier=PerkTier.Operative},
  new PerkView{id="b",name="Hyperthreading",tag="General",category="Skills",effect="Skills recharge *faster* when two operatives cast together",next="+18% recharge speed",stack=0,max=5,icon="haste"},
  new PerkView{id="c",name="Ablative Plating Mk II",tag="Rare",category="Health",effect="Absorb the first hit of every wave",next="+40 barrier",stack=2,max=4,icon="armor",tier=PerkTier.Rare}};

 // The 320 report: the draft's labels (tag, title, effect, next, ranks, the eyebrow, the caption, TAKE PERK and its sub line) fit their boxes with the longest names, at every width, for each card as it becomes the active one.
 [UnityTest] public IEnumerator TheDraftLabelsFitTheirBoxesAtEveryWidth(){
  foreach(int width in new[]{320,390,430}){
   yield return Boot(width);var page=Ui.Box(board,"vb-page");page.style.flexGrow=1;var picks=LongPerks();
   var view=new PerkDraftView().Into(page);view.style.flexGrow=1;
   for(int pick=0;pick<picks.Length;pick++){
    view.Bind(new PerkDraftViewModel{wave=12,bonus=1250,score=48210,choices=picks,pick=pick,squad=new[]{new SquadHealthView{code="AR",hp=900,maxHp=1000},new SquadHealthView{code="RK",hp=500,maxHp=1000}}});yield return Frames(8);
    yield return new WaitForSecondsRealtime(.4f);yield return Frames(2);
    var carousel=view.Choices;var active=(ChoiceCard)carousel.Active;Assert.AreEqual(pick,carousel.Index);
    var problems=Clipped(view,board.worldBound,l=>carousel.Cards.Any(c=>c!=active&&c.Contains(l)));
    Assert.IsEmpty(problems,"Clipped labels at "+width+" with card "+pick+":\n"+string.Join("\n",problems));
    // The active card holds all of its content: nothing is below its foot.
    var wb=active.worldBound;foreach(var label in active.Query<Label>().ToList().Where(Shown))Assert.LessOrEqual(label.worldBound.yMax,wb.yMax+.5f,"A label runs out of the card's foot at "+width+": "+label.text);
    Assert.GreaterOrEqual(view.Q<VbButton>("take").worldBound.height,43.5f);}}}

 static PerkView[] Owned()=>new[]{
  new PerkView{id="a",name="Overclock Matrix",icon="burst",now="+12% damage",stack=1,max=3,tier=PerkTier.Operative},new PerkView{id="b",name="Hyperthreading",icon="haste",now="+18% recharge",stack=3,max=5},
  new PerkView{id="c",name="Ablative Plating Mk II",icon="armor",now="+40 barrier",stack=2,max=4,tier=PerkTier.Rare},new PerkView{id="d",name="Wind",icon="move",now="+6% speed",stack=1,max=0,tier=PerkTier.Epic},new PerkView{id="e",name="Ward",icon="shield",now="+8",stack=1,max=3}};
 // The active perks are compact choice cards in two columns: they fill their cells, stay on the board, are 44 units or more and clip nothing.
 [UnityTest] public IEnumerator ActivePerksAreTwoColumnsOfCompactCardsThatFitAtEveryWidth(){
  foreach(int width in new[]{320,390,430}){
   yield return Boot(width);var view=new ActivePerksView().Into(board);view.style.flexGrow=1;view.Bind(new ActivePerksViewModel{wave=7,perks=Owned(),pick=1,totals=new[]{new LedgerLine{label="DAMAGE",value="+12%"}}});yield return Frames(8);yield return new WaitForSecondsRealtime(.5f);yield return Frames(3);
   var cards=view.Query<ChoiceCard>().ToList().Where(Shown).ToList();Assert.AreEqual(5,cards.Count);
   foreach(var c in cards){Assert.GreaterOrEqual(c.worldBound.height,43.5f,"A 44-unit target at "+width);Assert.GreaterOrEqual(c.worldBound.xMin,board.worldBound.xMin-.5f);Assert.LessOrEqual(c.worldBound.xMax,board.worldBound.xMax+.5f,"On the "+width+" board");}
   Assert.AreEqual(cards[0].worldBound.yMin,cards[1].worldBound.yMin,1f,"Two to a row at "+width);Assert.Greater(cards[1].worldBound.xMin,cards[0].worldBound.xMax-1f,"…side by side");Assert.Greater(cards[2].worldBound.yMin,cards[0].worldBound.yMax-1f,"…and the third starts the next row");
   Assert.AreEqual(cards[0].worldBound.width,cards[1].worldBound.width,1.5f,"Cells share the width");
   var problems=Clipped(view,board.worldBound);Assert.IsEmpty(problems,"Clipped labels at "+width+":\n"+string.Join("\n",problems));
   Assert.IsNotNull(view.Q(className:"vb-perk-detail"),"The picked perk is still explained below");}}

 // The merchant's offers and sales are compact cards in two columns; a health trade stays a row with its danger action.
 [UnityTest] public IEnumerator TheMerchantsCardsFitAtEveryWidthAndAnUnaffordableOneIsDimmed(){
  foreach(int width in new[]{320,390,430}){
   yield return Boot(width);var view=new MerchantScreenView().Into(board);view.style.flexGrow=1;var perks=Owned();
   view.Bind(new MerchantView{credits=100,
    offers=new[]{new MerchantOfferView{perk=perks[0],price=80},new MerchantOfferView{perk=perks[1],price=150},new MerchantOfferView{perk=perks[2],price=80,taken=true},new MerchantOfferView{perk=perks[3],price=60,blocked="Stack full"}},
    stacks=new[]{new MerchantStackView{perk=perks[0],payout=30},new MerchantStackView{perk=perks[1],payout=45}},
    health=new[]{new MerchantHealthView{actor=0,name="Aria",hp=900,maxHp=1000,cost=250,payout=30}}});yield return Frames(8);yield return new WaitForSecondsRealtime(.6f);yield return Frames(3);
   var offers=view.Market.Choices.Cast<ChoiceCard>().ToList();Assert.AreEqual(4,offers.Count);
   foreach(var c in view.Market.Query<ChoiceCard>().ToList().Where(Shown)){Assert.GreaterOrEqual(c.worldBound.height,43.5f,"A 44-unit target at "+width);Assert.GreaterOrEqual(c.worldBound.xMin,board.worldBound.xMin-.5f);Assert.LessOrEqual(c.worldBound.xMax,board.worldBound.xMax+.5f,"On the "+width+" board");}
   Assert.AreEqual(offers[0].worldBound.yMin,offers[1].worldBound.yMin,1f,"Two to a row");Assert.IsFalse(view.Market.Offer(0).Dimmed);Assert.IsTrue(view.Market.Offer(1).Dimmed,"150 is more than 100");Assert.IsTrue(view.Market.Offer(2).Dimmed,"Sold");Assert.IsTrue(view.Market.Offer(3).Dimmed);
   Assert.Less(view.Market.Offer(1).resolvedStyle.opacity,.9f,"A card that cannot be bought is dimmed");Assert.AreEqual(1,view.Market.Offer(0).resolvedStyle.opacity,.01f);
   var trade=view.Market.Q<VbButton>("sell-hp-0");Assert.IsNotNull(trade);Assert.GreaterOrEqual(trade.worldBound.height,43.5f);
   var problems=Clipped(view.Market,board.worldBound,l=>!Shown(l));Assert.IsEmpty(problems,"Clipped labels at "+width+":\n"+string.Join("\n",problems));}}
}
}
