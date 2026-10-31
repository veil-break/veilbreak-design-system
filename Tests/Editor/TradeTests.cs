using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;
namespace Veilbreak.Gui.Tests {
public sealed class TradeTests {
 [Test] public void RoadsHaveDistinctTargetsAndOnlyOneChoiceIsAccepted(){var route=new RouteChoice().Bind(new RouteView{left=new RoadView{title="Combat"},right=new RoadView{kind="merchant",title="Merchant"}});int chosen=-1,count=0;route.Chosen+=i=>{chosen=i;count++;};Assert.IsTrue(route.Choose(1));Assert.IsFalse(route.Choose(0));Assert.AreEqual(1,chosen);Assert.AreEqual(1,count);}
 [Test] public void MerchantBlocksUnaffordableTakenAndCappedOffers(){var screen=new MerchantScreen().Bind(new MerchantView{credits=90,offers=new[]{new MerchantOfferView{price=80},new MerchantOfferView{price=150},new MerchantOfferView{price=80,taken=true},new MerchantOfferView{price=80,blocked="Stack cap"}}});Assert.IsTrue(screen.Q<VbButton>("buy-0").enabledSelf);for(int i=1;i<4;i++)Assert.IsFalse(screen.Q<VbButton>("buy-"+i).enabledSelf);}
 [Test] public void HealthTradesShowTheConsequenceAndDoNotTradeTheLastHitPoint(){var screen=new MerchantScreen().Bind(new MerchantView{health=new[]{new MerchantHealthView{actor=0,name="Aria",hp=250,maxHp=1000,cost=250,payout=30,blocked="Keep 1 HP"},new MerchantHealthView{actor=1,name="Rook",hp=600,maxHp=1000,cost=250,payout=30}}});Assert.IsFalse(screen.Q<VbButton>("sell-hp-0").enabledSelf);Assert.IsTrue(screen.Q<VbButton>("sell-hp-1").enabledSelf);Assert.IsTrue(screen.Query<Label>().ToList().Exists(l=>l.text=="600 / 1000 HP → 350 HP"));}
 [Test] public void RarityRebindingRemovesStaleTier(){var card=new PerkCard().Bind(new PerkView{tier=PerkTier.Epic},false);Assert.IsTrue(card.ClassListContains("vb-perk--epic"));card.Bind(new PerkView{tier=PerkTier.Rare},false);Assert.IsFalse(card.ClassListContains("vb-perk--epic"));Assert.IsTrue(card.ClassListContains("vb-perk--rare"));}
 [Test] public void BattleFallbackPreservesActualNotchInsets(){var root=new VisualElement();var safe=new SafeAreaRoot(root,null){MinTop=8};safe.Apply(new Rect(0,0,390,844),new Vector2(390,844));Assert.AreEqual(8,safe.Insets.y);safe.Apply(new Rect(0,0,390,800),new Vector2(390,844));Assert.AreEqual(44,safe.Insets.y);}
 [Test] public void HudUsesPauseThenIconOnlyAutomation(){var hud=new HudTop().Bind(new HudView());var row=hud.Q(className:"vb-hud__row");Assert.AreEqual("pause",row[0].name);Assert.AreEqual("auto",row[1].name);Assert.AreEqual(0,row[1].Query<Label>().Where(l=>l.style.display!=DisplayStyle.None).ToList().Count,"icon only: the state label stays hidden");StringAssert.Contains("Autoplay on",row[1].tooltip);}
}
}
