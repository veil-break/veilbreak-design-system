using UnityEngine.UIElements;
namespace Veilbreak.Gui {
public enum BadgeTone { Neutral, Ally, Enemy, Power, Warn, Inverted }

/// <summary>A small slim chip: a word, optionally a glyph before it and a value after it. Status, currency, counts and tags
/// (the menu's BEST 18,240 and the pause sheet's perk chips are plain badges). Tones recolour fill, word and glyph together.</summary>
[UxmlElement] public partial class BadgeComponent:VisualElement {
 public const string ClassName="vb-badge";
 readonly VbIcon glyph;readonly Label word,number;BadgeTone tone;
 [UxmlAttribute] public string Text {get=>word.text;set{word.text=(value??"").ToUpperInvariant();word.Show(word.text.Length>0);}}
 [UxmlAttribute] public string Icon {get=>glyph.Icon;set{glyph.Icon=value;glyph.Show(!string.IsNullOrEmpty(value));}}
 [UxmlAttribute] public string Value {get=>number.text;set{number.text=value??"";number.Show(number.text.Length>0);}}
 [UxmlAttribute] public BadgeTone Tone {get=>tone;set{tone=value;Ui.State(this,ClassName,value);}}
 public BadgeComponent():this(""){}
 public BadgeComponent(string text,string icon=null,string value=null,BadgeTone tone=BadgeTone.Neutral){
  AddToClassList(ClassName);VbShape.Attach(this);pickingMode=PickingMode.Ignore;
  glyph=new VbIcon("",12).Classes(ClassName+"__icon").Into(this);word=Ui.Text(this,"",ClassName+"__text");number=Ui.Text(this,"",ClassName+"__value");
  Text=text;Icon=icon;Value=value;Tone=tone;
 }
 /// <summary>Updates the value only when it changed (badges are bound every frame in the HUD).</summary>
 public BadgeComponent Set(string value){if(value!=number.text)Value=value;return this;}
}

/// <summary>Badge presets. <see cref="StatusChip"/> and <see cref="CreditsChip"/> are the status and currency badges.</summary>
public static class Badge {
 public static BadgeComponent Of(VisualElement parent,string text,string icon=null,string value=null,BadgeTone tone=BadgeTone.Neutral)=>new BadgeComponent(text,icon,value,tone).Into(parent);
 /// <summary>A status: glyph and word (never colour alone), optional detail and time.</summary>
 public static StatusChip Status(StatusKind kind,string text=null,string detail=null,string time=null)=>new StatusChip(kind,text,detail,time);
 /// <summary>Run currency.</summary>
 public static CreditsChip Credits(int credits)=>new CreditsChip().Set(credits);
}
}
