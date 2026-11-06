using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
namespace Veilbreak.Gui {
public enum ProgressTone { Ally, Power, Enemy, Neutral }
/// <summary>One progress bar for every "how far along" read-out: a continuous fill, or <see cref="Segments"/> equal cells (perk levels,
/// stacks, steps). Segments fill up to <see cref="Value"/>; <see cref="Next"/> outlines the cell a pick or purchase would add.</summary>
[UxmlElement] public partial class ProgressBar:VisualElement {
 public const string ClassName="vb-progress";
 readonly VisualElement fill;readonly List<VisualElement> cells=new List<VisualElement>();int segments;float value;bool next;Color tint;
 /// <summary>0 = continuous; otherwise the number of levels.</summary>
 [UxmlAttribute] public int Segments {get=>segments;set{segments=Mathf.Max(0,value);Refresh();}}
 /// <summary>Filled amount: levels when segmented, 0–1 when continuous.</summary>
 [UxmlAttribute] public float Value {get=>value;set{this.value=value;Refresh();}}
 [UxmlAttribute] public bool Next {get=>next;set{next=value;Refresh();}}
 [UxmlAttribute] public ProgressTone Tone {set=>Ui.State(this,ClassName,value);}
 public ProgressBar():this(0,0){}
 public ProgressBar(int segments,float value,ProgressTone tone=ProgressTone.Ally){AddToClassList(ClassName);pickingMode=PickingMode.Ignore;fill=Ui.Box(this,ClassName+"__fill");Tone=tone;this.segments=segments;Value=value;}
 /// <summary>Colours the fill and filled cells (an operative's kit colour); <c>default</c> returns to the tone.</summary>
 public ProgressBar Tint(Color c){tint=c;Refresh();return this;}
 public ProgressBar Set(float value,int segments=-1,bool next=false){if(segments>=0)this.segments=segments;this.next=next;Value=value;return this;}
 void Refresh(){bool seg=segments>0;EnableInClassList(ClassName+"--segmented",seg);fill.Show(!seg);if(!seg){Ui.Fill(fill,value);}fill.style.backgroundColor=tint.a>0?new StyleColor(tint):new StyleColor(StyleKeyword.Null);
  for(int i=0;i<Mathf.Max(segments,cells.Count);i++){if(i>=cells.Count)cells.Add(Ui.Box(this,ClassName+"__cell"));var c=cells[i];c.Show(i<segments);
   c.EnableInClassList(ClassName+"__cell--on",i<Mathf.RoundToInt(value));c.EnableInClassList(ClassName+"__cell--next",next&&i==Mathf.RoundToInt(value));c.style.backgroundColor=tint.a>0&&i<Mathf.RoundToInt(value)?new StyleColor(tint):new StyleColor(StyleKeyword.Null);}}
}

/// <summary>The one list row: an optional leading glyph, a caps title with a caption under it, an optional progress bar, and a trailing
/// value and/or action. Merchant offers, stack sales, health trades and perk lists are all ListRows.</summary>
[UxmlElement] public partial class ListRow:SurfaceView {
 public const string ClassName="vb-list-row";
 readonly VbIcon glyph;readonly Label lead,title,caption,value;readonly VisualElement text,trail;ProgressBar bar;
 public event Action Picked;
 [UxmlAttribute] public string Icon {get=>glyph.Icon;set{glyph.Icon=value;glyph.Show(!string.IsNullOrEmpty(value));}}
 [UxmlAttribute] public string Title {get=>title.text;set=>title.text=(value??"").ToUpperInvariant();}
 [UxmlAttribute] public string Caption {get=>caption.text;set{caption.text=value??"";caption.Show(caption.text.Length>0);}}
 [UxmlAttribute] public string Value {get=>value.text;set{this.value.text=value??"";this.value.Show(this.value.text.Length>0);}}
 /// <summary>Short text before everything (a rank, an index); empty hides it.</summary>
 [UxmlAttribute] public string Lead {get=>lead.text;set{lead.text=value??"";lead.Show(lead.text.Length>0);}}
 public Label LeadLabel=>lead;public Label TitleLabel=>title;public Label CaptionLabel=>caption;public Label ValueLabel=>value;
 /// <summary>The text column (title, caption, bar): composites add their own lines here.</summary>
 public VisualElement Content=>text;
 /// <summary>The trailing slot for an action (a button); hidden while empty.</summary>
 public VisualElement Trail=>trail;
 /// <summary>A progress bar under the caption, created on first use.</summary>
 public ProgressBar Bar=>bar??=new ProgressBar().Classes(ClassName+"__bar").Into(text);
 public ListRow():this(""){}
 public ListRow(string title,string caption=null,string icon=null,string value=null){
  AddToClassList(ClassName);lead=Ui.Text(this,"",ClassName+"__lead");lead.Show(false);glyph=new VbIcon("",20).Classes(ClassName+"__icon").Into(this);
  text=Ui.Box(this,ClassName+"__text");this.title=Ui.Text(text,"",ClassName+"__title");this.caption=Ui.Text(text,"","vb-t-caption",ClassName+"__caption");
  this.value=Ui.Text(this,"",ClassName+"__value");trail=new ActionRow{Fixed=true}.Classes(ClassName+"__trail").Into(this);Title=title;Caption=caption;Icon=icon;Value=value;
  RegisterCallback<GeometryChangedEvent>(_=>trail.Show(trail.childCount>0));
 }
 /// <summary>Makes the whole row a target (tap, click, Enter, Space).</summary>
 public ListRow Pickable(Action pick=null){this.Clickable(()=>Picked?.Invoke());if(pick!=null)Picked+=pick;return this;}
 public ListRow WithAction(VisualElement action){trail.Add(action);trail.Show(true);return this;}
 public ListRow Selected(bool on){EnableInClassList(ClassName+"--selected",on);return this;}
}

/// <summary>Active-perk list row: a ListRow with the perk's glyph, name, tag and effect, its value and its levels. Tap selects.</summary>
[UxmlElement] public partial class PerkRow:ListRow {
 public new const string ClassName="vb-perk-row";
 public PerkRow(){AddToClassList(ClassName);Pickable();}
 public PerkRow Bind(PerkView p,bool selected){PerkArt.Tiers(this,p);Selected(selected);Icon=p.icon;Title=p.name;Caption=p.tag+" · "+p.effect;Value=p.now;
  Bar.Show(p.max>0);if(p.max>0)Bar.Set(p.stack,p.max);tooltip=p.name+" · "+PerkArt.Stacks(p,false);return this;}
}

/// <summary>One high-score line: a ListRow with the rank, the score, depth and squad codes; the current run is framed and badged.</summary>
[UxmlElement] public partial class ScoreRow:ListRow {
 public new const string ClassName="vb-score";
 public ScoreRow(){AddToClassList(ClassName);}
 public ScoreRow Bind(ScoreView s){
  Lead=s.rank.ToString("00");LeadLabel.EnableInClassList(ClassName+"__rank--top",s.rank<=3);Title=Ui.Number(s.score);Value=s.current?"THIS RUN":"";Selected(s.current);EnableInClassList(ClassName+"--current",s.current);
  Caption=$"Wave {s.wave} · {s.bosses} {(s.bosses==1?"boss":"bosses")}"+(s.squad.Length>0?" · "+string.Join(" ",s.squad):"");return this;
 }
}

/// <summary>One IF … THEN automation rule as a ListRow: its place, the highlighted condition values, the action, its cost, a plain
/// note on when it can run and an on/off toggle. A warning frames it in warn; a draft is dashed. The player's command always wins.</summary>
[UxmlElement] public partial class RuleRow:ListRow {
 public new const string ClassName="vb-rule";
 readonly VisualElement conditions;readonly VbSwitch toggle;
 public RuleView View {get;private set;}
 public event Action<bool> Toggled;
 public RuleRow(){AddToClassList(ClassName);conditions=Ui.Box(Content,ClassName+"__if");conditions.SendToBack();ValueLabel.AddToClassList(ClassName+"__cost");
  // The shared switch, compact: the row is its label.
  toggle=new VbSwitch("",null,true).Classes(ClassName+"__toggle","vb-switch--compact");toggle.RegisterValueChangedCallback(e=>{if(View!=null&&e.newValue!=View.enabled)Toggle();});WithAction(toggle);Bind(new RuleView());}
 public RuleRow Bind(RuleView v){View=v;Lead=v.index.ToString();var words=new System.Collections.Generic.List<(string id,string text,string cls)>{("if","IF",ClassName+"__keyword")};
  for(int i=0;i<v.conditions.Length;i++){var c=v.conditions[i];if(i>0)words.Add((i+":join",v.any?"OR":"AND",ClassName+"__join"));words.Add((i+":text",c.text,ClassName+"__text"));if(!string.IsNullOrEmpty(c.value))words.Add((i+":value",c.value,ClassName+"__value"));if(!string.IsNullOrEmpty(c.after))words.Add((i+":after",c.after,ClassName+"__text"));}
  Keyed.Bind(conditions,words,x=>x.id,x=>new Label{pickingMode=PickingMode.Ignore}.Classes(x.cls),(label,x)=>label.text=x.text);
  Title="THEN "+(v.action??"")+(string.IsNullOrEmpty(v.actionRest)?"":" "+v.actionRest);Value=v.cost>0?"−"+v.cost:"";Caption=v.note;
  EnableInClassList(ClassName+"--off",!v.enabled);EnableInClassList(ClassName+"--warning",v.warning);EnableInClassList(ClassName+"--draft",v.draft);toggle.SetValueWithoutNotify(v.enabled);toggle.tooltip=v.enabled?"Turn rule off":"Turn rule on";return this;}
 public void Toggle(){if(View==null)return;View.enabled=!View.enabled;Bind(View);Toggled?.Invoke(View.enabled);}
}

/// <summary>One key/value line: a caption key on the left, a value on the right (<c>good</c> reads in ally colour). Perk details, the
/// results ledger and any fact list use it.</summary>
[UxmlElement] public partial class KeyValueRow:VisualElement {
 public const string ClassName="vb-kv";
 readonly Label key,value;
 [UxmlAttribute] public string Key {get=>key.text;set=>key.text=value??"";}
 [UxmlAttribute] public string Value {get=>value.text;set=>this.value.text=value??"";}
 [UxmlAttribute] public bool Good {get=>value.ClassListContains(ClassName+"__value--good");set=>this.value.EnableInClassList(ClassName+"__value--good",value);}
 public Label KeyLabel=>key;public Label ValueLabel=>value;
 public KeyValueRow():this("",""){}
 public KeyValueRow(string key,string value,bool good=false){AddToClassList(ClassName);pickingMode=PickingMode.Ignore;this.key=Ui.Text(this,"","vb-t-caption",ClassName+"__key");this.value=Ui.Text(this,"",ClassName+"__value");Key=key;Value=value;Good=good;}
}
}
