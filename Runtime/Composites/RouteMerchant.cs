using System;
using UnityEngine.UIElements;
namespace Veilbreak.Gui {
/// <summary>Persistent run currency, formatted only when its value changes.</summary>
[UxmlElement] public partial class CreditsChip:VisualElement {
 readonly Label value;int shown=-1;
 public CreditsChip(){AddToClassList("vb-credits");VbShape.Attach(this);pickingMode=PickingMode.Ignore;new VbIcon("currency",14).Into(this);value=Ui.Text(this,"0","vb-credits__value");tooltip="Credits · spend at a merchant";}
 public CreditsChip Set(int credits){if(shown!=credits){shown=credits;value.text=Ui.Number(credits);}return this;}
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
 public MerchantScreen Bind(MerchantView view){Clear();var head=Ui.Box(this,"vb-merchant__head");Ui.Text(head,"NIGHT MARKET",TypeStyle.Title);new CreditsChip().Set(view.credits).Into(head);
  var list=Ui.List(this,null,"vb-merchant__scroll");
  Section(list,"Rare finds");for(int i=0;i<view.offers.Length;i++){int index=i;var o=view.offers[i];var row=Row(list,o.perk.name,o.perk.effect,o.perk.icon,o.perk);PerkArt.Tiers(row,o.perk);
   var buy=new VbButton(o.taken?"SOLD":"BUY "+o.price,ButtonVariant.Secondary,null,null,()=>Bought?.Invoke(index)){name="buy-"+i}.Classes("vb-merchant__buy");buy.Blocked=o.taken?"Sold":view.credits<o.price?"Need "+(o.price-view.credits):o.blocked;row.WithAction(buy);}
  Section(list,"Sell stacks");if(view.stacks.Length==0)Ui.Text(list,"Nothing to sell yet.","vb-t-caption","vb-list__empty");
  foreach(var st in view.stacks){var row=Row(list,st.perk.name,st.perk.now,st.perk.icon,st.perk);row.WithAction(new VbButton("+"+st.payout,ButtonVariant.Secondary,null,null,()=>PerkSold?.Invoke(st.perk.id)){name="sell-perk-"+st.perk.id}.Classes("vb-merchant__sell"));}
  Section(list,"Trade health · 25% max HP");
  foreach(var hp in view.health){var row=new ListRow(hp.name,hp.hp+" → "+Math.Max(0,hp.hp-hp.cost)+" HP","health").Into(list);row.Bar.Set(hp.hp/(float)Math.Max(1,hp.maxHp));
   var sell=new VbButton("+"+hp.payout,ButtonVariant.Danger,null,null,()=>HealthSold?.Invoke(hp.actor)){name="sell-hp-"+hp.actor}.Classes("vb-merchant__sell");sell.Blocked=hp.blocked;row.WithAction(sell);}
  new VbButton("LEAVE MARKET",ButtonVariant.Primary,null,"chevron",()=>Left?.Invoke()){name="leave-merchant"}.Classes("vb-merchant__leave").Into(this);return this;
 }
 static void Section(VisualElement list,string title)=>new LabelComponent(title).Classes("vb-merchant__section").Into(list);
 /// <summary>A perk as a list row: glyph, name, line, and its levels as a segmented bar.</summary>
 static ListRow Row(VisualElement list,string title,string caption,string icon,PerkView p){var r=new ListRow(title,caption,icon).Into(list);if(p.max>0)r.Bar.Set(p.stack,p.max);return r;}
 public float ScrollOffset {get=>this.Q<ScrollView>()?.scrollOffset.y??0;set{var scroll=this.Q<ScrollView>();if(scroll!=null)scroll.scrollOffset=new UnityEngine.Vector2(0,value);}}
}
}
