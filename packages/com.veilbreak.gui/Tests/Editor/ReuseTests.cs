using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine.UIElements;
namespace Veilbreak.Gui.Tests {
public sealed class ReuseTests {
 [Test] public void KeyedBindingRetainsReorderedItemsAndUsesFreshBindValues(){var parent=new VisualElement();var data=new[]{("a","A"),("b","B")};Keyed.Bind(parent,data,x=>x.Item1,_=>new Label(),(e,x)=>e.text=x.Item2);var a=parent[0];var b=parent[1];Keyed.Bind(parent,new[]{("b","changed"),("a","again")},x=>x.Item1,_=>new Label(),(e,x)=>e.text=x.Item2);Assert.AreSame(b,parent[0]);Assert.AreSame(a,parent[1]);Assert.AreEqual("changed",((Label)b).text);}
 [Test] public void DuplicateIdentityFailsBeforeChangingTheTree(){var parent=new VisualElement();var list=new KeyedList<string,Label>(parent,x=>x,x=>new Label(x),(e,x)=>{});list.Bind(new[]{"a"});Assert.Throws<ArgumentException>(()=>list.Bind(new[]{"b","b"}));Assert.AreEqual(1,parent.childCount);Assert.AreEqual("a",parent[0].name);}
 [Test] public void MerchantRebindKeepsOfferIdentityAndButton(){var market=new MerchantScreen();var model=new MerchantView{credits=100,offers=new[]{new MerchantOfferView{id="offer:1",perk=new PerkView{id="damage",name="Damage"},price=80}}};market.Bind(model);var card=market.Choices.First();Assert.IsInstanceOf<ChoiceCard>(card);Assert.IsTrue(card.enabledSelf);model.credits=20;market.Bind(model);Assert.AreSame(card,market.Choices.First());Assert.AreSame(card,market.Offer(0));Assert.IsFalse(card.enabledSelf,"Short of credits the card dims and is out of play");Assert.IsTrue(((ChoiceCard)card).Dimmed);Assert.AreEqual("BUY 80",((ChoiceCard)card).ActionText,"…and gives no second price");}
 [Test] public void LiveEffectsKeepUnknownIdsAndUseInspectableOverflow(){var effects=Enumerable.Range(0,12).Select(i=>new EffectInstance{id="actor:1:"+i,definition=EffectDefinition.Unknown("custom:"+i)}).ToArray();var strip=new EffectStrip().Bind(effects);Assert.AreEqual(1,strip.Query<EffectBadge>().ToList().Count);Assert.AreEqual("+11",strip.Q<VbButton>("all-effects").Title);var list=new EffectList().Bind(effects);Assert.AreEqual(12,list.Query<EffectBadge>().ToList().Count);}
 [Test] public void BossReadsMutableModels(){var board=new BattleHudView();var boss=new BossView{name="First"};board.Bind(new HudView(),Array.Empty<OperativeView>(),boss);boss.name="Changed";board.Bind(new HudView(),Array.Empty<OperativeView>(),boss);Assert.AreEqual("CHANGED",board.Boss.Q<Label>(className:"vb-boss__name").text);}
 [Test] public void OverlayFamiliesShareTheirLifecycle(){Assert.IsInstanceOf<OverlayView>(new DrawerOverlay());Assert.IsInstanceOf<OverlayView>(new PauseOverlayView());Assert.IsInstanceOf<OverlayView>(new DialogSurface());}
}
}
