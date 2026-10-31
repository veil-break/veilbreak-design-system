using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
namespace Veilbreak.Gui {
/// <summary>Square on/off switch with a caps label and a line saying what it does. Raises <see cref="ChangeEvent{T}"/> like any field.</summary>
[UxmlElement] public partial class VbSwitch:VisualElement,INotifyValueChanged<bool> {
 public const string ClassName="vb-switch";
 readonly Label title,detail;bool on;
 [UxmlAttribute] public string Text {get=>title.text;set=>title.text=value??"";}
 [UxmlAttribute] public string Detail {get=>detail.text;set{detail.text=value??"";detail.Show(detail.text.Length>0);}}
 public VbSwitch():this("",null,false){}
 public VbSwitch(string text,string detail,bool value){
  AddToClassList(ClassName);this.Clickable(()=>this.value=!on);
  var track=VbShape.Attach(Ui.Box(this,ClassName+"__track"));Ui.Box(track,ClassName+"__knob");
  var column=Ui.Box(this,ClassName+"__text");title=Ui.Text(column,text??"",ClassName+"__label");this.detail=Ui.Text(column,"",ClassName+"__detail");Detail=detail;
  SetValueWithoutNotify(value);
 }
 public bool value {get=>on;set{if(on==value)return;var previous=on;SetValueWithoutNotify(value);using(var e=ChangeEvent<bool>.GetPooled(previous,value)){e.target=this;SendEvent(e);}}}
 public void SetValueWithoutNotify(bool newValue){on=newValue;EnableInClassList(ClassName+"--on",on);}
}

/// <summary>The engine slider in Veil dress: a cut track with a teal fill, a square thumb and the value as a number.</summary>
[UxmlElement] public partial class VbSlider:Slider {
 public const string VbClassName="vb-slider";
 readonly Label readout;new VisualElement fill;
 public VbSlider():this("",0){}
 public VbSlider(string label,float value,float min=0,float max=1):base(label,min,max){
  AddToClassList(VbClassName);readout=new Label{pickingMode=PickingMode.Ignore}.Classes(VbClassName+"__value").Into(this);
  this.RegisterValueChangedCallback(_=>Refresh());RegisterCallback<GeometryChangedEvent>(_=>Refresh());SetValueWithoutNotify(value);Refresh();
 }
 void Refresh(){
  float t=Mathf.InverseLerp(lowValue,highValue,value);readout.text=Mathf.RoundToInt(t*100).ToString();
  if(fill==null){var tracker=this.Q(className:"unity-base-slider__tracker");if(tracker==null)return;fill=Ui.Box(tracker,VbClassName+"__fill");}
  Ui.Fill(fill,t);
 }
}

/// <summary>2–5 mutually exclusive peers in one cut frame; each option can carry its consequence ("60 FPS target").</summary>
[UxmlElement] public partial class VbSegmented:VisualElement,INotifyValueChanged<int> {
 public const string ClassName="vb-segmented";
 readonly List<VisualElement> options=new List<VisualElement>();int index;
 public VbSegmented():this(Array.Empty<(string,string)>(),0){}
 public VbSegmented(IEnumerable<(string label,string sub)> items,int selected){
  AddToClassList(ClassName);VbShape.Attach(this);int i=0;
  foreach(var (label,sub) in items){int k=i++;var o=Ui.Box(this,ClassName+"__option").Clickable(()=>value=k);Ui.Text(o,label,ClassName+"__label");if(!string.IsNullOrEmpty(sub))Ui.Text(o,sub,ClassName+"__sub");options.Add(o);}
  // Three or more peers share the full row instead of each holding a 100-unit minimum, so they fit a 320 board.
  EnableInClassList(ClassName+"--fill",options.Count>=3);SetValueWithoutNotify(selected);
 }
 public int value {get=>index;set{if(index==value)return;var previous=index;SetValueWithoutNotify(value);using(var e=ChangeEvent<int>.GetPooled(previous,value)){e.target=this;SendEvent(e);}}}
 public void SetValueWithoutNotify(int newValue){index=Mathf.Clamp(newValue,0,Mathf.Max(0,options.Count-1));for(int i=0;i<options.Count;i++)options[i].EnableInClassList(ClassName+"__option--on",i==index);}
}
}
