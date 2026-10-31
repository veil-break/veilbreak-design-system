using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
namespace Veilbreak.Gui {
static class PerkArt {
 // Operative perks show their operative; others a ringed glyph. Rare perks are cyan; epic perks are violet.
 public static void Fill(VisualElement art,VbIcon glyph,PerkView p){var crop=Ui.Crop(art);Ui.Image(crop,p.art);crop.Show(p.art!=null);art.EnableInClassList("vb-perk-art--portrait",p.art!=null);glyph.Icon=p.icon;if(glyph.ClassListContains("vb-perk-card__glyph"))glyph.Size(p.art!=null?16:32);}
 public static void Tiers(VisualElement e,PerkView p)=>Ui.State(e,"vb-perk",p.tier);
 public static string Stacks(PerkView p,bool draft)=>p.max>0?(draft?$"STACK {p.stack+1} / {p.max}":$"×{p.stack} OF {p.max}"):p.tier==PerkTier.Rare?"RARE DRAW":p.stack>1?"×"+p.stack:"REPEATABLE";
}

/// <summary>Draft card (one of three): art, tier tag, name, effect, stack pips and the value after the pick.</summary>
[UxmlElement] public partial class PerkCard:VisualElement {
 public const string ClassName="vb-perk-card";
 readonly VisualElement art,pips,shimmer;IVisualElementScheduledItem shimmerTick;readonly VbIcon glyph,check;readonly Label tag,category,title,effect,next,stacks;
 public PerkView View {get;private set;}
 public event Action Picked;
 public PerkCard(){
  AddToClassList(ClassName);VbShape.Attach(this);this.Clickable(()=>Picked?.Invoke());
  art=Ui.Box(this,ClassName+"__art","vb-perk-art");Ui.Box(art,ClassName+"__shade");var badge=VbShape.Attach(Ui.Box(art,ClassName+"__badge"));glyph=new VbIcon("add",32).Classes(ClassName+"__glyph").Into(badge);
  shimmer=Ui.Box(art,ClassName+"__shimmer");shimmer.Show(false);RegisterCallback<AttachToPanelEvent>(_=>StartShimmer());RegisterCallback<DetachFromPanelEvent>(_=>shimmerTick?.Pause());
  tag=Ui.Text(art,"",ClassName+"__tag");check=new VbIcon("check",14).Classes(ClassName+"__check").Into(art);Ui.Box(this,ClassName+"__rule");
  var body=Ui.Box(this,ClassName+"__body");category=Ui.Text(body,"",ClassName+"__category");title=Ui.Text(body,"",ClassName+"__name");effect=Ui.Text(body,"",ClassName+"__effect");
  var foot=Ui.Box(body,ClassName+"__foot");pips=Ui.Box(foot,ClassName+"__pips");next=Ui.Text(foot,"",ClassName+"__next");stacks=Ui.Text(foot,"",ClassName+"__stacks");
 }
 void StartShimmer(){shimmerTick?.Pause();bool epic=View?.tier==PerkTier.Epic;shimmer.Show(epic);if(!epic||panel==null)return;
  shimmerTick=schedule.Execute(()=>{bool on=!Glitch.Calm(this)&&!Ui.Under(this,SafeAreaRoot.Contrast);float p=UnityEngine.Time.unscaledTime%4f;shimmer.Show(on&&p<.8f);if(on&&p<.8f)shimmer.style.left=-40+(resolvedStyle.width+80)*p/.8f;}).Every(33);
 }
 public PerkCard Bind(PerkView p,bool selected){
  View=p;StartShimmer();PerkArt.Tiers(this,p);PerkArt.Fill(art,glyph,p);EnableInClassList(ClassName+"--selected",selected);check.Show(selected);
  tag.text=p.tag.ToUpperInvariant();category.text=p.category.ToUpperInvariant();title.text=p.name.ToUpperInvariant();effect.text=p.effect;next.text=p.next;stacks.text=PerkArt.Stacks(p,true);
  pips.Clear();for(int i=0;i<p.max;i++)Ui.Box(pips,ClassName+"__pip",i<p.stack?ClassName+"__pip--owned":i==p.stack?ClassName+"__pip--next":null);
  return this;
 }
}

/// <summary>Full perk explanation: draft (now → after pick) or owned (active value, stacks, waves picked). Used inline and as popover content.</summary>
[UxmlElement] public partial class PerkDetail:VisualElement {
 public const string ClassName="vb-perk-detail";
 readonly VisualElement thumb,rows;readonly VbIcon glyph;readonly Label title,tag,effect;
 public PerkDetail(){
  AddToClassList(ClassName);VbShape.Attach(this);pickingMode=PickingMode.Ignore;
  var head=Ui.Box(this,ClassName+"__head");thumb=VbShape.Attach(Ui.Box(head,ClassName+"__thumb","vb-perk-art"));glyph=new VbIcon("add",24).Into(thumb);
  var h=Ui.Box(head,ClassName+"__head-text");title=Ui.Text(h,"",ClassName+"__name");tag=Ui.Text(h,"",ClassName+"__tag");effect=Ui.Text(this,"",ClassName+"__effect");rows=Ui.Box(this,ClassName+"__rows");
 }
 public PerkDetail Bind(PerkView p,bool draft){
  PerkArt.Tiers(this,p);PerkArt.Fill(thumb,glyph,p);title.text=p.name.ToUpperInvariant();tag.text=(p.category+" · "+p.tag).ToUpperInvariant();effect.text=p.effect;
  rows.Clear();
  var lines=draft?new[]{("Now",p.now,""),("After pick",p.next,"--good"),("Stacks",p.max>0?$"{p.stack} → {p.stack+1} / {p.max}":PerkArt.Stacks(p,true),"")}
                 :new[]{("Active",p.now,"--good"),("Stacks",p.max>0?$"{p.stack} / {p.max}":PerkArt.Stacks(p,false),""),("Picked",p.waves.Length>0?string.Join(", ",p.waves.Select(w=>"wave "+w)):"—","")};
  foreach(var (k,v,tone) in lines){var r=Ui.Box(rows,ClassName+"__row");Ui.Text(r,k,ClassName+"__key");Ui.Text(r,v,ClassName+"__value",tone.Length>0?ClassName+"__value"+tone:null);}
  rows[rows.childCount-1].AddToClassList(ClassName+"__row--last"); // UI Toolkit has no :last-child
  return this;
 }
}

/// <summary>Compact owned-perk chip; hover, focus or tap shows its <see cref="PerkDetail"/>.</summary>
[UxmlElement] public partial class PerkChip:VisualElement {
 public const string ClassName="vb-perk-chip";
 readonly VbIcon glyph;readonly Label title,count;PerkView view;
 public Popover Details {get;private set;}
 public PerkChip():this(false){}
 public PerkChip(bool alignEnd){
  AddToClassList(ClassName);VbShape.Attach(this);focusable=true;glyph=new VbIcon("add",14).Into(this);title=Ui.Text(this,"",ClassName+"__name");count=Ui.Text(this,"",ClassName+"__count");
  Details=Popover.Attach(this,()=>view==null?null:new PerkDetail().Bind(view,false).Classes(ClassName+"__popover"),PopoverPlacement.Below,alignEnd);
 }
 public PerkChip Bind(PerkView p){view=p;PerkArt.Tiers(this,p);glyph.Icon=p.icon;title.text=p.name.ToUpperInvariant();count.text="×"+p.stack;count.Show(p.stack>1);tooltip=p.name+" · "+p.now;return this;}
}

/// <summary>Active-perk list row: art, name, tag, effect, value and stacks. Tap selects.</summary>
[UxmlElement] public partial class PerkRow:VisualElement {
 public const string ClassName="vb-perk-row";
 readonly VisualElement thumb;readonly VbIcon glyph;readonly Label title,tag,effect,value,stacks;
 public event Action Picked;
 public PerkRow(){
  AddToClassList(ClassName);VbShape.Attach(this);this.Clickable(()=>Picked?.Invoke());
  thumb=VbShape.Attach(Ui.Box(this,ClassName+"__thumb","vb-perk-art"));glyph=new VbIcon("add",20).Into(thumb);
  var text=Ui.Box(this,ClassName+"__text");var line=Ui.Box(text,ClassName+"__line");title=Ui.Text(line,"",ClassName+"__name");tag=Ui.Text(line,"",ClassName+"__tag");effect=Ui.Text(text,"",ClassName+"__effect");
  var right=Ui.Box(this,ClassName+"__right");value=Ui.Text(right,"",ClassName+"__value");stacks=Ui.Text(right,"",ClassName+"__stacks");
 }
 public PerkRow Bind(PerkView p,bool selected){PerkArt.Tiers(this,p);PerkArt.Fill(thumb,glyph,p);EnableInClassList(ClassName+"--selected",selected);title.text=p.name.ToUpperInvariant();tag.text=p.tag.ToUpperInvariant();effect.text=p.effect;value.text=p.now;stacks.text=PerkArt.Stacks(p,false);return this;}
}
}
