using System;
using UnityEngine.UIElements;
namespace Veilbreak.Gui {
/// <summary>One setting for an input that does not carry its own label (segmented choices, custom pickers): a caps label column,
/// the input, and one line saying what it does. <see cref="VbSlider"/> and <see cref="VbSwitch"/> already include theirs.</summary>
[UxmlElement] public partial class FieldComposite:VisualElement {
 public const string ClassName="vb-field";
 readonly LabelComponent label;readonly Label help;readonly VisualElement input;
 public override VisualElement contentContainer=>input;
 [UxmlAttribute("label")] public string Title {get=>label.text;set{label.text=value;label.Show(label.text.Length>0);}}
 [UxmlAttribute] public string Help {get=>help.text;set{help.text=value??"";help.Show(help.text.Length>0);}}
 public FieldComposite():this("",null,null){}
 public FieldComposite(string label,VisualElement input,string help=null){
  AddToClassList(ClassName);pickingMode=PickingMode.Ignore;
  this.label=new LabelComponent("",LabelSize.Badge).Classes(ClassName+"__label");hierarchy.Add(this.label);
  this.input=new VisualElement{pickingMode=PickingMode.Ignore};this.input.AddToClassList(ClassName+"__input");hierarchy.Add(this.input);
  this.help=new Label{pickingMode=PickingMode.Ignore};this.help.AddToClassList("vb-t-caption");this.help.AddToClassList(ClassName+"__help");hierarchy.Add(this.help);
  Title=label;Help=help;if(input!=null)this.input.Add(input);
 }
}

/// <summary>An options form: fieldsets in a scroll, an optional note, a footer of secondary actions, and one change event.
/// Settings apply as they change; there is no Submit. The saved state reads in the screen header's meta (<see cref="Header"/>)
/// or, without a header, in the form's own status line. Destructive actions go in the footer as danger buttons and confirm.</summary>
[UxmlElement] public partial class FormComposite:VisualElement {
 public const string ClassName="vb-form";
 readonly ScrollView scroll;readonly VisualElement footer;readonly Label status;string saved="";
 /// <summary>Raised after any input inside the form changes (slider, switch, segmented, any <see cref="ChangeEvent{T}"/> of those types).</summary>
 public event Action Changed;
 public ScreenHeader Header {get;set;}
 public override VisualElement contentContainer=>scroll.contentContainer;
 [UxmlAttribute] public string SavedMeta {get=>saved;set{saved=value??"";if(Header!=null){Header.Meta=saved;Header.Accent(false);status.Show(false);}else{status.text=saved;status.Show(saved.Length>0);}}}
 public FormComposite():this(null){}
 public FormComposite(ScreenHeader header){
  AddToClassList(ClassName);pickingMode=PickingMode.Ignore;Header=header;
  status=new Label{pickingMode=PickingMode.Ignore};status.AddToClassList("vb-t-badge");status.AddToClassList(ClassName+"__status");hierarchy.Add(status);status.Show(false);
  scroll=new ScrollView(ScrollViewMode.Vertical){horizontalScrollerVisibility=ScrollerVisibility.Hidden,verticalScrollerVisibility=ScrollerVisibility.Hidden};scroll.AddToClassList(ClassName+"__scroll");scroll.AddToClassList("vb-page__scroll");hierarchy.Add(scroll);
  footer=new VisualElement{pickingMode=PickingMode.Ignore};footer.AddToClassList(ClassName+"__footer");hierarchy.Add(footer);footer.Show(false);
  RegisterCallback<ChangeEvent<float>>(_=>Raise());RegisterCallback<ChangeEvent<bool>>(_=>Raise());RegisterCallback<ChangeEvent<int>>(_=>Raise());
 }
 void Raise(){if(saved.Length>0)SavedMeta=saved;Changed?.Invoke();}
 /// <summary>A titled fieldset (<c>vb-section</c>) in the scroll; add fields to it.</summary>
 public VisualElement Fieldset(string title)=>Ui.Section(scroll.contentContainer,title);
 /// <summary>A raised note with a glyph (what lives elsewhere, what applies later).</summary>
 public VisualElement Note(string icon,string text){var n=Ui.Panel(scroll.contentContainer,"vb-panel--raised",ClassName+"__note");new VbIcon(icon,16).Into(n);Ui.Text(n,text,"vb-t-caption");return n;}
 /// <summary>Secondary actions under the scroll, spread across the row (stacked on a compact board).</summary>
 public FormComposite Footer(params VisualElement[] actions){foreach(var a in actions)footer.Add(a);footer.Show(footer.childCount>0);return this;}
}
}
