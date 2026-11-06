using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UIElements;
namespace Veilbreak.Gui {
/// <summary>Two roads as a mirrored pair of <see cref="ChoiceCard"/>s (the left one cut and aligned to the left, the right one to the right). The entire card is a keyboard and touch target.</summary>
[UxmlElement] public partial class RouteChoice:ViewBase {
 public event Action<int> Chosen;bool chosen;readonly Label eyebrow;readonly ChoiceCard[] signs=new ChoiceCard[2];
 public IEnumerable<VisualElement> Choices=>signs;
 public RouteChoice(){AddToClassList("vb-route");eyebrow=Ui.Text(this,"","vb-t-label","vb-route__eyebrow");new GlitchText("CHOOSE YOUR ROAD").Classes("vb-t-display","vb-route__title").Into(this);var roads=Ui.Box(this,"vb-route__roads");
  for(int i=0;i<2;i++){int side=i;var card=signs[i]=new ChoiceCard(i==0?ChoiceSide.Left:ChoiceSide.Right).Into(roads);card.name=i==0?"route-left":"route-right";card.Picked+=()=>Choose(side);}}
 public bool Choose(int side){if(chosen||side<0||side>1)return false;chosen=true;SetEnabled(false);Chosen?.Invoke(side);return true;}
 public RouteChoice Bind(RouteView view){chosen=false;eyebrow.text="CROSSROADS · WAVE "+view.wave.ToString("00");signs[0].Bind(view.left);signs[1].Bind(view.right);SetEnabled(true);return this;}
}
/// <summary>Stable keyed offer and sale rows, each a <see cref="StatRow"/> (the name over its effect, a 44-unit action at the end). The host confirms transactions and supplies eligibility. Each row's
/// action is named for its subject (<c>buy-{index}</c>, <c>sell-perk-{id}</c>, <c>sell-hp-{actor}</c>), so hosts and tests address it directly. An offer that cannot be bought is dimmed, with no second
/// price; a section with nothing in it does not show; a sale pays in credits (<c>+30 ⬡</c>).</summary>
[UxmlElement] public partial class MerchantScreen:ViewBase {
 public event Action<int> Bought,HealthSold;public event Action<string> PerkSold;public event Action Left;
 readonly ChipRow credits;readonly ListLayout list;readonly VisualElement offers,stacks,health;readonly LabelComponent stacksTitle;
 readonly KeyedList<(MerchantOfferView view,int index),StatRow> offerRows;
 readonly KeyedList<MerchantStackView,StatRow> stackRows;readonly KeyedList<MerchantHealthView,StatRow> healthRows;int balance;
 public IEnumerable<VisualElement> Choices=>offerRows.Elements;
 public MerchantScreen(){AddToClassList("vb-merchant");var head=Ui.Box(this,"vb-merchant__head");Ui.Text(head,"NIGHT MARKET",TypeStyle.Title);credits=ChipRow.Credits().Into(head);list=Ui.List(this,null,"vb-merchant__scroll");
  Section(list,"Rare finds");offers=Ui.Box(list);stacksTitle=Section(list,"Sell stacks");stacks=Ui.Box(list);Section(list,"Trade health · 25% max HP");health=Ui.Box(list);
  offerRows=new KeyedList<(MerchantOfferView view,int index),StatRow>(offers,x=>string.IsNullOrEmpty(x.view.id)?x.view.perk.id+":"+x.index:x.view.id,x=>new StatRow().Classes("vb-merchant__offer"),(r,x)=>Offer(r,x.view,x.index));
  stackRows=new KeyedList<MerchantStackView,StatRow>(stacks,x=>x.perk.id,x=>new StatRow().Classes("vb-merchant__stack"),(r,x)=>Sale(r,x));
  healthRows=new KeyedList<MerchantHealthView,StatRow>(health,x=>x.actor.ToString(),x=>new StatRow().Classes("vb-merchant__trade"),(r,x)=>Trade(r,x));
  new VbButton("LEAVE MARKET",ButtonVariant.Primary,null,"chevron",()=>Left?.Invoke()){name="leave-merchant"}.Classes("vb-merchant__leave").Into(this);
 }
 public MerchantScreen Bind(MerchantView view){balance=view.credits;credits.SetNumber(balance);offerRows.Bind(view.offers.Select((v,i)=>(v,i)),true);stackRows.Bind(view.stacks);stacksTitle.Show(view.stacks.Length>0);stacks.Show(view.stacks.Length>0);healthRows.Bind(view.health);return this;}
 static LabelComponent Section(VisualElement parent,string title)=>new LabelComponent(title).Classes("vb-merchant__section").Into(parent);
 /// <summary>The row's one action button, made on first use; <paramref name="click"/> gets the row, whose <c>userData</c> holds what the row is about now (keyed rows are reused).</summary>
 static VbButton Act(StatRow row,string name,ButtonVariant variant,Action<StatRow> click){var button=row.Trail.Q<VbButton>();if(button==null){button=new VbButton("",variant,null,null,()=>click(row));row.WithAction(button);}button.name=name;button.Variant=variant;return button;}
 void Offer(StatRow r,MerchantOfferView v,int index){
  r.Plated=true;r.Stacked=true;r.Icon=v.perk.icon;PerkArt.Tiers(r,v.perk);r.Key=v.perk.name.ToUpperInvariant();r.Value=Markup.Rich(v.perk.effect);
  r.userData=index;var buy=Act(r,"buy-"+index,ButtonVariant.Secondary,row=>Bought?.Invoke((int)row.userData));buy.Title=v.taken?"SOLD":"BUY "+v.price;
  // Short of credits or sold: the button dims and the row says no more; a game's own reason (a full stack) still shows.
  buy.Blocked=v.taken?"":v.blocked;buy.Dimmed=v.taken||balance<v.price;r.Dimmed=buy.Dimmed;
 }
 void Sale(StatRow r,MerchantStackView v){
  r.Plated=true;r.Stacked=true;r.Icon=v.perk.icon;r.Key=v.perk.name.ToUpperInvariant();r.Value=Markup.Rich(v.perk.now);
  r.userData=v.perk.id;var sell=Act(r,"sell-perk-"+v.perk.id,ButtonVariant.Secondary,row=>PerkSold?.Invoke((string)row.userData));sell.Title="+"+v.payout;sell.Icon="currency";sell.tooltip="Sell for "+v.payout+" credits";
 }
 void Trade(StatRow r,MerchantHealthView v){
  r.Plated=true;r.Stacked=true;r.Icon="health";r.Key=v.name.ToUpperInvariant();r.Value=v.hp+" / "+v.maxHp+" HP → "+Math.Max(0,v.hp-v.cost)+" HP";r.Bar.Set(v.hp/(float)Math.Max(1,v.maxHp));
  r.userData=v.actor;var sell=Act(r,"sell-hp-"+v.actor,ButtonVariant.Danger,row=>HealthSold?.Invoke((int)row.userData));sell.Title="+"+v.payout;sell.Icon="currency";sell.tooltip="Trade health for "+v.payout+" credits";sell.Blocked=v.blocked;
 }
 public float ScrollOffset {get=>list.scrollOffset.y;set=>list.scrollOffset=new UnityEngine.Vector2(0,value);}
}
}
