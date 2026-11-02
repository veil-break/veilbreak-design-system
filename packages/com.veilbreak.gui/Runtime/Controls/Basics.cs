using System;
using UnityEngine;
using UnityEngine.UIElements;
namespace Veilbreak.Gui {
/// <summary>A 24-grid stroke glyph from the Veilbreak icon set, chosen by name (Icons.uss maps <c>vb-icon--{kebab-name}</c> to its SVG).</summary>
[UxmlElement] public partial class VbIcon:VisualElement {
 public const string ClassName="vb-icon";
 string icon="",source="";
 // Rebinding the same name (per-frame view models) is a no-op, so it makes no garbage.
 [UxmlAttribute] public string Icon {get=>icon;set{value=value??"";if(value==source)return;source=value;if(icon.Length>0)RemoveFromClassList(ClassName+"--"+icon);icon=Ui.Kebab(value);if(icon.Length>0)AddToClassList(ClassName+"--"+icon);}}
 public VbIcon():this("add"){}
 public VbIcon(string icon,float size=16){AddToClassList(ClassName);pickingMode=PickingMode.Ignore;Icon=icon;Size(size);}
 public VbIcon Size(float px){style.width=px;style.height=px;return this;}
 public VbIcon Tint(Color c){style.unityBackgroundImageTintColor=c;return this;}
}

/// <summary>The slash-cut VEIL/BREAK mark at a given width.</summary>
[UxmlElement] public partial class VbMark:VisualElement {
 public const string ClassName="vb-mark";
 float width=260;
 [UxmlAttribute] public float Width {get=>width;set{width=value;style.width=value;style.height=Mathf.Round(value*259f/513f);}}
 public VbMark():this(260){}
 public VbMark(float width){AddToClassList(ClassName);pickingMode=PickingMode.Ignore;Width=width;}
}

/// <summary>Cut-corner button. One <see cref="ButtonVariant.Primary"/> per screen (pink→teal glass, gradient outline). Blocked buttons stay legible and say why.</summary>
[UxmlElement] public partial class VbButton:Button {
 public const string ClassName="vb-button";
 readonly Label title,subtitle;readonly VbIcon glyph;ButtonVariant variant;string sub="",blocked="";
 [UxmlAttribute] public string Title {get=>title.text;set=>title.text=value??"";}
 [UxmlAttribute] public string Sub {get=>sub;set{sub=value??"";Refresh();}}
 [UxmlAttribute] public string Icon {get=>glyph.Icon;set{glyph.Icon=value;glyph.Size(value=="chevron"||value=="play"?12:20);glyph.Show(!string.IsNullOrEmpty(value));}}
 [UxmlAttribute] public ButtonVariant Variant {get=>variant;set{variant=value;Ui.State(this,ClassName,value);}}
 /// <summary>A short reason (under five words). Non-empty disables the button and shows the reason in place of the sub line.</summary>
 [UxmlAttribute] public string Blocked {get=>blocked;set{blocked=value??"";Refresh();}}
 public VbButton():this(""){}
 public VbButton(string title,ButtonVariant variant=ButtonVariant.Secondary,string sub=null,string icon=null,Action click=null){
  text="";AddToClassList(ClassName);VbShape.Attach(this);
  var column=Ui.Box(this,ClassName+"__text");this.title=Ui.Text(column,title??"",ClassName+"__label");subtitle=Ui.Text(column,"",ClassName+"__sub");
  glyph=new VbIcon("",20).Classes(ClassName+"__icon").Into(this);
  Variant=variant;Sub=sub;Icon=icon;if(click!=null)clicked+=click;
 }
 void Refresh(){bool isBlocked=blocked.Length>0;EnableInClassList(ClassName+"--blocked",isBlocked);SetEnabled(!isBlocked);subtitle.text=isBlocked?blocked:sub;subtitle.Show(subtitle.text.Length>0);}
}

/// <summary>A labelled number with an optional bar (inspectors, results, pause).</summary>
[UxmlElement] public partial class StatReadout:VisualElement {
 public const string ClassName="vb-stat";
 readonly Label label,value,unit,delta;readonly ProgressBar bar;
 public StatReadout():this("","",null){}
 public StatReadout(string label,string value,string unit=null,float bar=-1){
  AddToClassList(ClassName);pickingMode=PickingMode.Ignore;this.label=Ui.Text(this,label,ClassName+"__label");
  var row=Ui.Box(this,ClassName+"__row");this.value=Ui.Text(row,value,ClassName+"__value");this.unit=Ui.Text(row,"",ClassName+"__unit");delta=Ui.Text(row,"",ClassName+"__delta");delta.Show(false);
  this.bar=new ProgressBar().Classes(ClassName+"__bar").Into(this);Set(value,unit,bar);
 }
 /// <summary>The change being previewed ("+12", "−4%"). A leading + reads up (teal), − reads down (pink); <paramref name="up"/> overrides when lower is better.</summary>
 public StatReadout Delta(string change,bool? up=null){change??="";delta.text=change;delta.Show(change.Length>0);bool rise=up??change.StartsWith("+"),fall=up.HasValue?!up.Value:change.StartsWith("−")||change.StartsWith("-");
  delta.EnableInClassList(ClassName+"__delta--up",change.Length>0&&rise);delta.EnableInClassList(ClassName+"__delta--down",change.Length>0&&fall&&!rise);return this;}
 /// <summary>Bar tone: ally (default), power or text.</summary>
 public StatReadout Tone(string tone){foreach(var t in new[]{"power","text"})EnableInClassList(ClassName+"--"+t,t==tone);return this;}
 public StatReadout Set(string value,string unit=null,float bar=-1){this.value.text=value;this.unit.text=unit??"";this.unit.Show(!string.IsNullOrEmpty(unit));this.bar.Show(bar>=0);if(bar>=0)this.bar.Value=bar;return this;}
}

/// <summary>A status with its own glyph and word, so no two statuses read alike and none relies on colour.</summary>
public sealed class StatusChip:VisualElement {
 public const string ClassName="vb-status";
 public static string Icon(StatusKind kind)=>kind switch{StatusKind.Exposed=>"exposed",StatusKind.Silenced=>"silence",StatusKind.Rooted=>"root",StatusKind.Slowed=>"slow",StatusKind.Interrupted=>"warning",_=>"barrier"};
 public StatusChip(StatusKind kind,string text=null,string detail=null,string time=null){
  AddToClassList(ClassName);Ui.State(this,ClassName,kind);VbShape.Attach(this);pickingMode=PickingMode.Ignore;
  new VbIcon(Icon(kind),14).Classes(ClassName+"__icon").Into(this);Ui.Text(this,text??kind.ToString(),ClassName+"__word");
  if(!string.IsNullOrEmpty(detail))Ui.Text(this,"· "+detail,ClassName+"__detail");
  if(!string.IsNullOrEmpty(time))Ui.Text(this,time,ClassName+"__time");
 }
}
}
