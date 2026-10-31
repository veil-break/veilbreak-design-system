using System;
using UnityEngine.UIElements;
namespace Veilbreak.Gui {
/// <summary>Persistent run currency, formatted only when its value changes.</summary>
[UxmlElement] public partial class CreditsChip:VisualElement {
 readonly Label value;int shown=-1;
 public CreditsChip(){AddToClassList("vb-credits");VbShape.Attach(this);pickingMode=PickingMode.Ignore;new VbIcon("currency",14).Into(this);value=Ui.Text(this,"0","vb-credits__value");tooltip="Credits · spend at a merchant";}
 public CreditsChip Set(int credits){if(shown!=credits){bool change=shown>=0;shown=credits;VbMotion.Count(value,credits,Ui.Number);if(change)VbMotion.Pop(this);}return this;}
}
/// <summary>Two distinct, fully labelled road signs. The entire sign is a keyboard and touch target.</summary>
[UxmlElement] public partial class RouteChoice:VisualElement {
 public event Action<int> Chosen;bool chosen;
 public RouteChoice(){AddToClassList("vb-route");}
 public bool Choose(int side){if(chosen||side<0||side>1)return false;chosen=true;SetEnabled(false);Chosen?.Invoke(side);return true;}
 public RouteChoice Bind(RouteView view){Clear();chosen=false;Ui.Text(this,"CROSSROADS · WAVE "+view.wave.ToString("00"),"vb-t-label","vb-route__eyebrow");new GlitchText("CHOOSE YOUR ROAD").Classes("vb-t-display","vb-route__title").Into(this);Ui.Text(this,"Two roads. One choice. Your build carries forward.","vb-t-caption");
  var roads=Ui.Box(this,"vb-route__roads");for(int i=0;i<2;i++){int side=i;var road=i==0?view.left:view.right;var sign=VbShape.Attach(Ui.Box(roads,"vb-route__sign","vb-route__sign--"+road.kind));sign.name=i==0?"route-left":"route-right";sign.tooltip=road.title+" · "+road.hint;sign.Clickable(()=>Choose(side));
   Ui.Text(sign,i==0?"← LEFT ROAD":"RIGHT ROAD →","vb-t-badge","vb-route__direction");new VbIcon(road.icon,36).Classes("vb-route__glyph").Into(sign);Ui.Text(sign,road.title.ToUpperInvariant(),"vb-route__kind");Ui.Text(sign,road.hint,"vb-route__hint");Ui.Spacer(sign);Ui.Text(sign,"TAKE THIS ROAD","vb-route__confirm");}SetEnabled(true);return this;
 }
}
/// <summary>Portable merchant page: priced offers, one-stack sales, per-operative HP trades and leave.
/// Prices, eligibility and consequences come from the host; transactions are confirmed by the simulation.</summary>
[UxmlElement] public partial class MerchantScreen:VisualElement {
 public event Action<int> Bought,HealthSold;public event Action<string> PerkSold;public event Action Left;
 public MerchantScreen(){AddToClassList("vb-merchant");}
 public MerchantScreen Bind(MerchantView view){Clear();var head=Ui.Box(this,"vb-merchant__head");new GlitchText("NIGHT MARKET").Classes("vb-t-display","vb-merchant__title").Into(head);new CreditsChip().Set(view.credits).Into(head);
  var scroll=Ui.Scroll(this,"vb-merchant__scroll");Ui.Text(scroll,"Trade what you carry. Buy what changes the next fight.","vb-t-caption");Ui.Text(scroll,"RARE FINDS · ONE OF EACH","vb-t-label","vb-merchant__section");var offers=Ui.Box(scroll,"vb-merchant__offers");
  for(int i=0;i<view.offers.Length;i++){int index=i;var offer=view.offers[i];var column=Ui.Box(offers,"vb-merchant__offer");new PerkCard().Bind(offer.perk,false).Into(column);var buy=new VbButton(offer.taken?"SOLD":"BUY · "+offer.price,ButtonVariant.Secondary,offer.taken?"Installed":offer.price+" credits",null,()=>Bought?.Invoke(index)){name="buy-"+i}.Classes("vb-merchant__buy").Into(column);buy.Blocked=offer.taken?"Sold":view.credits<offer.price?"Need "+(offer.price-view.credits):offer.blocked;}
  Ui.Text(scroll,"SELL STACKS","vb-t-label","vb-merchant__section");if(view.stacks.Length==0)Ui.Text(scroll,"No lasting stacks to sell. Instant rewards cannot be traded.","vb-t-caption");
  foreach(var stack in view.stacks){var row=Ui.Panel(scroll,"vb-merchant__exchange");var info=Ui.Box(row,"vb-merchant__info");Ui.Text(info,stack.perk.name.ToUpperInvariant()+" ×"+stack.perk.stack,"vb-t-label");Ui.Text(info,"Sell one stack · "+stack.perk.now,"vb-t-caption");new VbButton("SELL +"+stack.payout,ButtonVariant.Secondary,null,null,()=>PerkSold?.Invoke(stack.perk.id)){name="sell-perk-"+stack.perk.id}.Classes("vb-merchant__sell").Into(row);}
  Ui.Text(scroll,"TRADE HEALTH","vb-t-label","vb-merchant__section");Ui.Text(scroll,"Pay 25% of max HP per trade. At least 1 HP must remain. No automatic healing.","vb-t-caption");
  foreach(var hp in view.health){var row=Ui.Panel(scroll,"vb-merchant__exchange");var info=Ui.Box(row,"vb-merchant__info");Ui.Text(info,hp.name.ToUpperInvariant(),"vb-t-label");Ui.Text(info,hp.hp+" / "+hp.maxHp+" HP → "+Math.Max(0,hp.hp-hp.cost)+" HP","vb-t-caption");new HealthBar().Set(hp.hp,hp.maxHp).Into(info);var sell=new VbButton("SELL +"+hp.payout,ButtonVariant.Danger,null,null,()=>HealthSold?.Invoke(hp.actor)){name="sell-hp-"+hp.actor}.Classes("vb-merchant__sell").Into(row);sell.Blocked=hp.blocked;}
  new VbButton("LEAVE MARKET",ButtonVariant.Primary,"Keep your credits · continue to the next wave","chevron",()=>Left?.Invoke()){name="leave-merchant"}.Classes("vb-merchant__leave").Into(this);return this;
 }
 public float ScrollOffset {get=>this.Q<ScrollView>()?.scrollOffset.y??0;set{var scroll=this.Q<ScrollView>();if(scroll!=null)scroll.scrollOffset=new UnityEngine.Vector2(0,value);}}
}
}
