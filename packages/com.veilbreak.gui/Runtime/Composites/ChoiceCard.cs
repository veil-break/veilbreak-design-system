using System;
using UnityEngine.UIElements;
namespace Veilbreak.Gui {
/// <summary>Which edge of a pair a <see cref="ChoiceCard"/> is: <see cref="Left"/> cuts its top-left and bottom-left corners and aligns its text right, <see cref="Right"/> cuts its
/// top-right and bottom-right corners and aligns its text left, so the two cut sides face outwards; <see cref="Single"/> (a card on its own, one of three) keeps the regular cuts.</summary>
public enum ChoiceSide { Single, Left, Right }

/// <summary>One option the player takes by tapping it: a road, an extract choice, a perk. The whole card is the button. It stacks a short tag, a centred glyph, a title, a few short
/// facts, rank cells (a road's threat or a perk's levels, one <see cref="ProgressBar"/> of segments), the value after the pick and the action word at the foot. The text is
/// compact (no line height in USS: the size does the work), the card has its own edge and a slight tint of its tone (<see cref="Tone"/>). A pair of cards takes <see cref="ChoiceSide.Left"/> and <see cref="ChoiceSide.Right"/>.
/// It replaces the road sign, the perk card and the route choice items.</summary>
[UxmlElement] public partial class ChoiceCard:CardView {
 public const string ClassName="vb-choice";
 protected override string SelectionClass=>ClassName+"--selected";
 readonly Label tag,title,text,next,action;readonly VbIcon glyph;readonly ProgressBar rank;ChoiceSide side;TextTone tone;string kind;
 public PerkView View {get;private set;}
 public event Action Picked;
 [UxmlAttribute] public ChoiceSide Side {get=>side;set{EnableInClassList(ClassName+"--left",value==ChoiceSide.Left);EnableInClassList(ClassName+"--right",value==ChoiceSide.Right);side=value;}}
 /// <summary>The tint and edge colour: Ally (teal), Enemy (pink), Power (violet); Default is neutral.</summary>
 [UxmlAttribute] public TextTone Tone {get=>tone;set{if(tone!=TextTone.Default)RemoveFromClassList(ClassName+"--"+Ui.Kebab(tone.ToString()));tone=value;if(tone!=TextTone.Default)AddToClassList(ClassName+"--"+Ui.Kebab(tone.ToString()));}}
 [UxmlAttribute] public string Tag {get=>tag.text;set{tag.text=(value??"").ToUpperInvariant();tag.Show(tag.text.Length>0);}}
 [UxmlAttribute] public string Icon {get=>glyph.Icon;set{glyph.Icon=value;glyph.Show(!string.IsNullOrEmpty(value));}}
 [UxmlAttribute] public string Title {get=>title.text;set=>title.text=(value??"").ToUpperInvariant();}
 /// <summary>The facts under the title; emphasis marks render (<see cref="Markup"/>).</summary>
 [UxmlAttribute] public string Text {get=>text.text;set{text.text=Markup.Rich(value);text.Show(!string.IsNullOrEmpty(value));}}
 /// <summary>The line before the action (a perk's value after the pick); empty hides it.</summary>
 [UxmlAttribute] public string Next {get=>next.text;set{next.text=value??"";next.Show(next.text.Length>0);}}
 /// <summary>The action word at the foot (TAKE THIS ROAD, EXTRACT); empty hides it.</summary>
 [UxmlAttribute] public string ActionText {get=>action.text;set{action.text=(value??"").ToUpperInvariant();action.Show(action.text.Length>0);}}
 /// <summary>Rank cells: <paramref name="filled"/> of <paramref name="cells"/>; none hides them. <paramref name="next"/> outlines the cell a pick would add.</summary>
 public ChoiceCard Ranks(int filled,int cells,bool next=false,ProgressTone tone=ProgressTone.Ally){rank.Show(cells>0);if(cells>0){rank.Tone=tone;rank.Set(filled,cells,next);}return this;}
 public ChoiceCard():this(ChoiceSide.Single){}
 public ChoiceCard(ChoiceSide side){
  AddToClassList(ClassName);this.Clickable(()=>Picked?.Invoke());
  tag=Ui.Text(this,"","vb-t-badge",ClassName+"__tag");glyph=new VbIcon("add",28).Classes(ClassName+"__glyph").Into(this);
  title=Ui.Text(this,"",ClassName+"__title");text=Ui.Text(this,"","vb-t-caption",ClassName+"__text");rank=new ProgressBar().Classes(ClassName+"__rank").Into(this);Ui.Spacer(this);
  next=Ui.Text(this,"",ClassName+"__next");action=Ui.Text(this,"",ClassName+"__action");
  Side=side;Tag=null;Next=null;ActionText=null;Text=null;rank.Show(false);
 }
 /// <summary>A road of the crossroads: its glyph, title and hint, its threat as rank cells and <paramref name="action"/> at the foot; the tone follows the road's kind.</summary>
 public ChoiceCard Bind(RoadView road,string action="TAKE THIS ROAD"){
  if(kind!=null)RemoveFromClassList(ClassName+"--"+kind);kind=road.kind;AddToClassList(ClassName+"--"+kind);Tone=ToneOf(road.kind);
  Icon=road.icon;Title=road.title;Text=road.hint;Ranks(road.rank,road.rank>0?road.ranks:0,false,ProgressTone.Enemy);ActionText=action;tooltip=road.title+" · "+road.hint;return this;
 }
 /// <summary>A perk of the draft: its tier word, glyph, name, effect, level cells and the value after the pick; <paramref name="selected"/> frames it with brackets.</summary>
 public ChoiceCard Bind(PerkView p,bool selected){
  bool first=View==null;View=p;PerkArt.Tiers(this,p);bool was=Selected;Selected=selected;if(selected&&!was&&!first)VbMotion.Pop(this,.04f);
  Icon=p.icon;Tag=p.tag;Title=p.name;Text=p.effect;Next=p.next==p.effect?"":p.next;Ranks(p.stack,p.max>1?p.max:0,p.stack<p.max);ActionText=null;tooltip=p.name+" · "+PerkArt.Stacks(p,true);return this;
 }
 /// <summary>The tone a road of <paramref name="kind"/> takes: elites and bosses read as danger, markets as power, anything else (a plain fight) as ally.</summary>
 public static TextTone ToneOf(string kind)=>kind=="elite"||kind=="boss"?TextTone.Enemy:kind=="merchant"?TextTone.Power:TextTone.Ally;
}
}
