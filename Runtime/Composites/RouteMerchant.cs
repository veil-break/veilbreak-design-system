using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UIElements;
namespace Veilbreak.Gui {
/// <summary>Persistent run currency, formatted only when its value changes.</summary>
[UxmlElement] public partial class CreditsChip:BadgeComponent {
 int shown=-1;
 public CreditsChip():base("","currency","0"){AddToClassList("vb-credits");glyph.Size(14);number.AddToClassList("vb-credits__value");tooltip="Credits · spend at a merchant";}
 public CreditsChip Set(int credits){if(shown!=credits){bool change=shown>=0;shown=credits;VbMotion.Count(number,credits,Ui.Number);if(change)VbMotion.Pop(this);}return this;}
}
/// <summary>Two distinct, fully labelled road signs. The entire sign is a keyboard and touch target.</summary>
[UxmlElement] public partial class RouteChoice:ViewBase {
 public event Action<int> Chosen;bool chosen;readonly Label eyebrow;readonly RoadSign[] signs=new RoadSign[2];
 public IEnumerable<VisualElement> Choices=>signs;
 public RouteChoice(){AddToClassList("vb-route");eyebrow=Ui.Text(this,"","vb-t-label","vb-route__eyebrow");new GlitchText("CHOOSE YOUR ROAD").Classes("vb-t-display","vb-route__title").Into(this);Ui.Text(this,"Two roads. One choice. Your build carries forward.","vb-t-caption");var roads=Ui.Box(this,"vb-route__roads");for(int i=0;i<2;i++){int side=i;signs[i]=new RoadSign(i,()=>Choose(side)).Into(roads);}}
 public bool Choose(int side){if(chosen||side<0||side>1)return false;chosen=true;SetEnabled(false);Chosen?.Invoke(side);return true;}
 public RouteChoice Bind(RouteView view){chosen=false;eyebrow.text="CROSSROADS · WAVE "+view.wave.ToString("00");signs[0].Bind(view.left);signs[1].Bind(view.right);SetEnabled(true);return this;}
 sealed class RoadSign:SurfaceView {string kind;readonly VbIcon icon;readonly Label title,hint;public RoadSign(int side,Action choose){AddToClassList("vb-route__sign");name=side==0?"route-left":"route-right";this.Clickable(choose);Ui.Text(this,side==0?"← LEFT ROAD":"RIGHT ROAD →","vb-t-badge","vb-route__direction");icon=new VbIcon("battle",36).Classes("vb-route__glyph").Into(this);title=Ui.Text(this,"","vb-route__kind");hint=Ui.Text(this,"","vb-route__hint");Ui.Spacer(this);Ui.Text(this,"TAKE THIS ROAD","vb-route__confirm");}public void Bind(RoadView road){if(kind!=null)RemoveFromClassList("vb-route__sign--"+kind);kind=road.kind;AddToClassList("vb-route__sign--"+kind);icon.Icon=road.icon;title.text=road.title.ToUpperInvariant();hint.text=road.hint;tooltip=road.title+" · "+road.hint;}}
}
/// <summary>Stable keyed offer and sale rows. The host confirms transactions and supplies eligibility.</summary>
[UxmlElement] public partial class MerchantScreen:ViewBase {
 public event Action<int> Bought,HealthSold;public event Action<string> PerkSold;public event Action Left;
 readonly CreditsChip credits;readonly ListLayout list;readonly VisualElement offers,stacks,health;readonly Label empty;
 readonly KeyedList<(MerchantOfferView view,int index),OfferRow> offerRows;
 readonly KeyedList<MerchantStackView,SaleRow> stackRows;readonly KeyedList<MerchantHealthView,HealthRow> healthRows;int balance;
 public IEnumerable<VisualElement> Choices=>offerRows.Elements;
 public MerchantScreen(){AddToClassList("vb-merchant");var head=Ui.Box(this,"vb-merchant__head");Ui.Text(head,"NIGHT MARKET",TypeStyle.Title);credits=new CreditsChip().Into(head);list=Ui.List(this,null,"vb-merchant__scroll");
  Section(list,"Rare finds");offers=Ui.Box(list);Section(list,"Sell stacks");stacks=Ui.Box(list);empty=Ui.Text(list,"Nothing to sell yet.","vb-t-caption","vb-list__empty");Section(list,"Trade health · 25% max HP");health=Ui.Box(list);
  offerRows=new KeyedList<(MerchantOfferView view,int index),OfferRow>(offers,x=>string.IsNullOrEmpty(x.view.id)?x.view.perk.id+":"+x.index:x.view.id,x=>{var row=new OfferRow();row.Buy+=()=>Bought?.Invoke(row.Index);return row;},(r,x)=>{r.Index=x.index;r.Bind(x.view,balance);});
  stackRows=new KeyedList<MerchantStackView,SaleRow>(stacks,x=>x.perk.id,x=>new SaleRow(()=>PerkSold?.Invoke(x.perk.id)),(r,x)=>r.Bind(x));
  healthRows=new KeyedList<MerchantHealthView,HealthRow>(health,x=>x.actor.ToString(),x=>new HealthRow(()=>HealthSold?.Invoke(x.actor)),(r,x)=>r.Bind(x));
  new VbButton("LEAVE MARKET",ButtonVariant.Primary,null,"chevron",()=>Left?.Invoke()){name="leave-merchant"}.Classes("vb-merchant__leave").Into(this);
 }
 public MerchantScreen Bind(MerchantView view){balance=view.credits;credits.Set(balance);offerRows.Bind(view.offers.Select((v,i)=>(v,i)),true);stackRows.Bind(view.stacks);empty.Show(view.stacks.Length==0);healthRows.Bind(view.health);return this;}
 static void Section(VisualElement parent,string title)=>new LabelComponent(title).Classes("vb-merchant__section").Into(parent);
 sealed class OfferRow:ListRow {readonly VbButton buy;public int Index;public event Action Buy;public OfferRow(){AddToClassList("vb-merchant__offer");buy=new VbButton("BUY",ButtonVariant.Secondary,null,null,()=>Buy?.Invoke()).Classes("vb-merchant__buy");WithAction(buy);}public void Bind(MerchantOfferView v,int credits){Title=v.perk.name;Caption=v.perk.effect;Icon=v.perk.icon;PerkArt.Tiers(this,v.perk);Bar.Show(v.perk.max>0);if(v.perk.max>0)Bar.Set(v.perk.stack,v.perk.max);buy.Title=v.taken?"SOLD":"BUY "+v.price;buy.Blocked=v.taken?"Sold":credits<v.price?"Need "+(v.price-credits):v.blocked;}}
 sealed class SaleRow:ListRow {readonly VbButton sell;public SaleRow(Action action){sell=new VbButton("",ButtonVariant.Secondary,null,null,action).Classes("vb-merchant__sell");WithAction(sell);}public void Bind(MerchantStackView v){Title=v.perk.name;Caption=v.perk.now;Icon=v.perk.icon;Bar.Show(v.perk.max>0);if(v.perk.max>0)Bar.Set(v.perk.stack,v.perk.max);sell.Title="+"+v.payout;}}
 sealed class HealthRow:ListRow {readonly VbButton sell;public HealthRow(Action action){Icon="health";sell=new VbButton("",ButtonVariant.Danger,null,null,action).Classes("vb-merchant__sell");WithAction(sell);}public void Bind(MerchantHealthView v){Title=v.name;Caption=v.hp+" / "+v.maxHp+" HP → "+Math.Max(0,v.hp-v.cost)+" HP";Bar.Set(v.hp/(float)Math.Max(1,v.maxHp));sell.Title="+"+v.payout;sell.Blocked=v.blocked;}}
 public float ScrollOffset {get=>list.scrollOffset.y;set=>list.scrollOffset=new UnityEngine.Vector2(0,value);}
}
}
