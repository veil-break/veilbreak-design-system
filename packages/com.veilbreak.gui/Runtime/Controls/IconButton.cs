using System;
using UnityEngine.UIElements;
namespace Veilbreak.Gui {
/// <summary>A 44 × 44 chipped square holding one glyph (or a short state such as "2×"), with a tooltip: back, pause, close, speed,
/// autoplay. <c>--active</c> is the toggled-on look (AUTO on); <c>--quiet</c> is translucent over the world (the HUD band).</summary>
[UxmlElement] public partial class IconButtonComposite:VisualElement {
 public const string ClassName="vb-icon-button";
 readonly VbIcon glyph;Label state;bool active,quiet;
 public event Action Clicked;
 [UxmlAttribute] public string Icon {get=>glyph.Icon;set{glyph.Icon=value;glyph.Show(!string.IsNullOrEmpty(value));}}
 /// <summary>A short state shown in place of (or after) the glyph: "1×", "2×".</summary>
 [UxmlAttribute] public string Text {get=>state?.text??"";set{value??="";if(state==null&&value.Length==0)return;if(TextLabel.text!=value)state.text=value;state.Show(value.Length>0);}}
 /// <summary>The state label, created the first time <see cref="Text"/> is set.</summary>
 public Label TextLabel=>state??=Ui.Text(this,"",ClassName+"__text");
 [UxmlAttribute] public bool Active {get=>active;set{active=value;EnableInClassList(ClassName+"--active",value);}}
 [UxmlAttribute] public bool Quiet {get=>quiet;set{quiet=value;EnableInClassList(ClassName+"--quiet",value);}}
 public IconButtonComposite():this("close",""){}
 public IconButtonComposite(string icon,string tooltip,Action click=null,float size=20){
  AddToClassList(ClassName);VbShape.Attach(this);this.Clickable(()=>Clicked?.Invoke());focusable=true;
  glyph=new VbIcon("",size).Classes(ClassName+"__icon").Into(this);
  Icon=icon;Text="";this.tooltip=tooltip??"";if(click!=null)Clicked+=click;
 }
}
}
