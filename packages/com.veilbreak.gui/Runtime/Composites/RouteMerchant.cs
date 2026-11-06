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
/// <summary>Stable keyed offers, sales and trades. An offer and a sale are compact <see cref="ChoiceCard"/>s in a two-column grid (the perk, its effect or value, and BUY with the price or SELL with the payout at the foot: the whole card is the button);
/// a health trade is a <see cref="StatRow"/> with its bar and a danger action. The host confirms transactions and supplies eligibility. Each card carries its <see cref="ChoiceCard.Command"/> (<c>buy-{index}</c>, <c>sell-perk-{id}</c>) and
/// <see cref="Offer"/> and <see cref="Sale"/> find them; a trade's button is named <c>sell-hp-{actor}</c>. An offer that cannot be bought is dimmed and disabled, with no second price; a section with nothing in it does not show; a sale pays in credits.</summary>
[UxmlElement] public partial class MerchantScreen:ViewBase {
 public event Action<int> Bought,HealthSold;public event Action<string> PerkSold;public event Action Left;
 readonly ChipRow credits;readonly ListLayout list;readonly VisualElement offers,stacks,health;readonly LabelComponent stacksTitle;
 readonly KeyedList<(MerchantOfferView view,int index),ChoiceCard> offerCards;
 readonly KeyedList<MerchantStackView,ChoiceCard> saleCards;readonly KeyedList<MerchantHealthView,StatRow> healthRows;int balance;
 public IEnumerable<VisualElement> Choices=>offerCards.Elements;
 /// <summary>The card of the offer at <paramref name="index"/> in the bound view, or null.</summary>
 public ChoiceCard Offer(int index){string command="buy-"+index;foreach(var c in offerCards.Elements)if(c.Command==command)return c;return null;}
 /// <summary>The card of the stack of perk <paramref name="perkId"/> the merchant buys, or null.</summary>
 public ChoiceCard Sale(string perkId){string command="sell-perk-"+perkId;foreach(var c in saleCards.Elements)if(c.Command==command)return c;return null;}
 public MerchantScreen(){AddToClassList("vb-merchant");var head=Ui.Box(this,"vb-merchant__head");Ui.Text(head,"NIGHT MARKET",TypeStyle.Title);credits=ChipRow.Credits().Into(head);list=Ui.List(this,null,"vb-merchant__scroll");
  Section(list,"Rare finds");offers=Ui.Grid(list,2,true,"vb-merchant__offers");stacksTitle=Section(list,"Sell stacks");stacks=Ui.Grid(list,2,true,"vb-merchant__stacks");Section(list,"Trade health · 25% max HP");health=Ui.Box(list);
  offerCards=new KeyedList<(MerchantOfferView view,int index),ChoiceCard>(offers,x=>string.IsNullOrEmpty(x.view.id)?x.view.perk.id+":"+x.index:x.view.id,x=>Card("vb-merchant__offer",card=>Bought?.Invoke((int)card.userData)),(c,x)=>{c.userData=x.index;c.BindOffer(x.view,x.index,balance);});
  saleCards=new KeyedList<MerchantStackView,ChoiceCard>(stacks,x=>x.perk.id,x=>Card("vb-merchant__stack",card=>PerkSold?.Invoke((string)card.userData)),(c,x)=>{c.userData=x.perk.id;c.BindSale(x);});
  healthRows=new KeyedList<MerchantHealthView,StatRow>(health,x=>x.actor.ToString(),x=>new StatRow().Classes("vb-merchant__trade"),(r,x)=>Trade(r,x));
  new VbButton("LEAVE MARKET",ButtonVariant.Primary,null,"chevron",()=>Left?.Invoke()){name="leave-merchant"}.Classes("vb-merchant__leave").Into(this);
 }
 static ChoiceCard Card(string cls,Action<ChoiceCard> picked){var card=new ChoiceCard{Compact=true}.Classes(cls);card.Picked+=()=>picked(card);return card;}
 public MerchantScreen Bind(MerchantView view){balance=view.credits;credits.SetNumber(balance);offerCards.Bind(view.offers.Select((v,i)=>(v,i)),true);saleCards.Bind(view.stacks);stacksTitle.Show(view.stacks.Length>0);stacks.Show(view.stacks.Length>0);healthRows.Bind(view.health);return this;}
 static LabelComponent Section(VisualElement parent,string title)=>new LabelComponent(title).Classes("vb-merchant__section").Into(parent);
 /// <summary>The row's one action button, made on first use; <paramref name="click"/> gets the row, whose <c>userData</c> holds what the row is about now (keyed rows are reused).</summary>
 static VbButton Act(StatRow row,string name,ButtonVariant variant,Action<StatRow> click){var button=row.Trail.Q<VbButton>();if(button==null){button=new VbButton("",variant,null,null,()=>click(row));row.WithAction(button);}button.name=name;button.Variant=variant;return button;}
 void Trade(StatRow r,MerchantHealthView v){
  r.Plated=true;r.Stacked=true;r.Icon="health";r.Key=v.name.ToUpperInvariant();r.Value=v.hp+" / "+v.maxHp+" HP → "+Math.Max(0,v.hp-v.cost)+" HP";r.Bar.Set(v.hp/(float)Math.Max(1,v.maxHp));
  r.userData=v.actor;var sell=Act(r,"sell-hp-"+v.actor,ButtonVariant.Danger,row=>HealthSold?.Invoke((int)row.userData));sell.Title="+"+v.payout;sell.Icon="currency";sell.tooltip="Trade health for "+v.payout+" credits";sell.Blocked=v.blocked;
 }
 public float ScrollOffset {get=>list.scrollOffset.y;set=>list.scrollOffset=new UnityEngine.Vector2(0,value);}
}
}
