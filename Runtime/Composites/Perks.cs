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

/// <summary>Draft card (one of three), minimal: tier word, glyph, name, effect, a segmented progress bar of its levels (the next one
/// outlined) and the value after the pick. Selected = brackets./summary>
[UxmlElement] public partial class PerkCard:VisualElement {
 public const string ClassName="vb-perk-card";
 readonly VbIcon glyph;readonly Label tag,title,effect,next;readonly ProgressBar levels;
 public PerkView View {get;private set;}
 public event Action Picked;
 public PerkCard(){
  AddToClassList(ClassName);VbShape.Attach(this);this.Clickable(()=>Picked?.Invoke());
  tag=Ui.Text(this,"","vb-t-badge",ClassName+"__tag");glyph=new VbIcon("add",28).Classes(ClassName+"__glyph").Into(this);
  title=Ui.Text(this,"",ClassName+"__name");effect=Ui.Text(this,"","vb-t-caption",ClassName+"__effect");Ui.Spacer(this);
  levels=new ProgressBar().Classes(ClassName+"__levels").Into(this);next=Ui.Text(this,"",ClassName+"__next");
 }
 public PerkCard Bind(PerkView p,bool selected){
  View=p;PerkArt.Tiers(this,p);EnableInClassList(ClassName+"--selected",selected);glyph.Icon=p.icon;
  tag.text=p.tag.ToUpperInvariant();title.text=p.name.ToUpperInvariant();effect.text=p.effect;next.text=p.next;
  levels.Show(p.max>0);levels.Set(p.stack,p.max,p.stack<p.max);tooltip=p.name+" · "+PerkArt.Stacks(p,true);return this;
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

}
