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
 readonly VisualElement fill;readonly List<VisualElement> cells=new List<VisualElement>();int segments;float value;bool next;
 /// <summary>0 = continuous; otherwise the number of levels.</summary>
 [UxmlAttribute] public int Segments {get=>segments;set{segments=Mathf.Max(0,value);Refresh();}}
 /// <summary>Filled amount: levels when segmented, 0–1 when continuous.</summary>
 [UxmlAttribute] public float Value {get=>value;set{this.value=value;Refresh();}}
 [UxmlAttribute] public bool Next {get=>next;set{next=value;Refresh();}}
 [UxmlAttribute] public ProgressTone Tone {set=>Ui.State(this,ClassName,value);}
 public ProgressBar():this(0,0){}
 public ProgressBar(int segments,float value,ProgressTone tone=ProgressTone.Ally){AddToClassList(ClassName);pickingMode=PickingMode.Ignore;fill=Ui.Box(this,ClassName+"__fill");Tone=tone;this.segments=segments;Value=value;}
 public ProgressBar Set(float value,int segments=-1,bool next=false){if(segments>=0)this.segments=segments;this.next=next;Value=value;return this;}
 void Refresh(){bool seg=segments>0;EnableInClassList(ClassName+"--segmented",seg);fill.Show(!seg);if(!seg){Ui.Fill(fill,value);}
  for(int i=0;i<Mathf.Max(segments,cells.Count);i++){if(i>=cells.Count)cells.Add(Ui.Box(this,ClassName+"__cell"));var c=cells[i];c.Show(i<segments);
   c.EnableInClassList(ClassName+"__cell--on",i<Mathf.RoundToInt(value));c.EnableInClassList(ClassName+"__cell--next",next&&i==Mathf.RoundToInt(value));}}
}

/// <summary>The one list row: an optional leading glyph, a caps title with a caption under it, an optional progress bar, and a trailing
/// value and/or action. Merchant offers, stack sales, health trades and perk lists are all ListRows.</summary>
[UxmlElement] public partial class ListRow:VisualElement {
 public const string ClassName="vb-list-row";
 readonly VbIcon glyph;readonly Label title,caption,value;readonly VisualElement text,trail;ProgressBar bar;
 public event Action Picked;
 [UxmlAttribute] public string Icon {get=>glyph.Icon;set{glyph.Icon=value;glyph.Show(!string.IsNullOrEmpty(value));}}
 [UxmlAttribute] public string Title {get=>title.text;set=>title.text=(value??"").ToUpperInvariant();}
 [UxmlAttribute] public string Caption {get=>caption.text;set{caption.text=value??"";caption.Show(caption.text.Length>0);}}
 [UxmlAttribute] public string Value {get=>value.text;set{this.value.text=value??"";this.value.Show(this.value.text.Length>0);}}
 /// <summary>The trailing slot for an action (a button); hidden while empty.</summary>
 public VisualElement Trail=>trail;
 /// <summary>A progress bar under the caption, created on first use.</summary>
 public ProgressBar Bar=>bar??=new ProgressBar().Classes(ClassName+"__bar").Into(text);
 public ListRow():this(""){}
 public ListRow(string title,string caption=null,string icon=null,string value=null){
  AddToClassList(ClassName);VbShape.Attach(this);pickingMode=PickingMode.Ignore;glyph=new VbIcon("",20).Classes(ClassName+"__icon").Into(this);
  text=Ui.Box(this,ClassName+"__text");this.title=Ui.Text(text,"",ClassName+"__title");this.caption=Ui.Text(text,"","vb-t-caption",ClassName+"__caption");
  this.value=Ui.Text(this,"",ClassName+"__value");trail=Ui.Box(this,ClassName+"__trail");Title=title;Caption=caption;Icon=icon;Value=value;
  RegisterCallback<GeometryChangedEvent>(_=>trail.Show(trail.childCount>0));
 }
 /// <summary>Makes the whole row a target (tap, click, Enter, Space).</summary>
 public ListRow Pickable(Action pick=null){this.Clickable(()=>Picked?.Invoke());if(pick!=null)Picked+=pick;return this;}
 public ListRow Action(VisualElement action){trail.Add(action);trail.Show(true);return this;}
 public ListRow Selected(bool on){EnableInClassList(ClassName+"--selected",on);return this;}
}
}
