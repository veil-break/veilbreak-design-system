using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
namespace Veilbreak.Gui {
/// <summary>How a perk is tinted: General is neutral, Operative teal (one operative's), Rare cyan, Epic violet.</summary>
public enum PerkTier { General, Operative, Rare, Epic }

static class PerkArt {
 // Operative perks show their operative; others a ringed glyph. Rare perks are cyan; epic perks are violet.
 public static void Fill(VisualElement art,VbIcon glyph,PerkView p){var crop=Ui.Crop(art);Ui.Image(crop,p.art);crop.Show(p.art!=null);art.EnableInClassList("vb-perk-art--portrait",p.art!=null);glyph.Icon=p.icon;if(glyph.ClassListContains("vb-perk-card__glyph"))glyph.Size(p.art!=null?16:32);}
 public static void Tiers(VisualElement e,PerkView p)=>Ui.State(e,"vb-perk",p.tier);
 public static string Stacks(PerkView p,bool draft)=>p.max>0?(draft?$"STACK {p.stack+1} / {p.max}":$"×{p.stack} OF {p.max}"):p.tier==PerkTier.Rare?"RARE DRAW":p.stack>1?"×"+p.stack:"REPEATABLE";
 /// <summary>Whether a "now" value says anything: a perk not yet owned has none, and "None" is no value.</summary>
 public static bool Known(string value)=>!string.IsNullOrEmpty(value)&&value!="None"&&value!="—";
}

/// <summary>Full perk explanation: draft (the value now, if there is one, then after the pick) or owned (active value, stacks, waves picked). Used inline and as popover content. Each fact is one
/// <see cref="StatRow"/>: the key keeps its width and the value wraps beside it. Emphasis marks in the effect render (<see cref="Markup"/>).</summary>
[UxmlElement] public partial class PerkDetail:DetailView {
 public const string ClassName="vb-perk-detail";
 readonly VisualElement thumb,rows;readonly VbIcon glyph;readonly Label title,tag,effect;
 public PerkDetail(){
  AddToClassList(ClassName);
  var head=Ui.Box(this,ClassName+"__head");thumb=VbShape.Attach(Ui.Box(head,ClassName+"__thumb","vb-perk-art"));glyph=new VbIcon("add",24).Into(thumb);
  var h=Ui.Box(head,ClassName+"__head-text");title=Ui.Text(h,"",ClassName+"__name");tag=Ui.Text(h,"",ClassName+"__tag");effect=Ui.Text(this,"",ClassName+"__effect");rows=Ui.Box(this,ClassName+"__rows");
 }
 public PerkDetail Bind(PerkView p,bool draft){
  PerkArt.Tiers(this,p);PerkArt.Fill(thumb,glyph,p);title.text=p.name.ToUpperInvariant();tag.text=(p.category+" · "+p.tag).ToUpperInvariant();effect.text=Markup.Rich(p.effect);
  var lines=draft?new[]{("Now",p.now,""),("After pick",p.next,"--good")}.Where(x=>x.Item1!="Now"||PerkArt.Known(x.Item2)).ToArray()
                 :new[]{("Active",p.now,"--good"),("Stacks",p.max>0?$"{p.stack} / {p.max}":PerkArt.Stacks(p,false),""),("Picked",p.waves.Length>0?string.Join(", ",p.waves.Select(w=>"wave "+w)):"—","")};
  Keyed.Bind(rows,lines,x=>x.Item1,_=>new StatRow().Classes(ClassName+"__row"),(r,x)=>{r.Key=x.Item1;r.Value=x.Item2;r.Good=x.Item3=="--good";});
  return this;
 }
}
}
