using UnityEngine.UIElements;
namespace Veilbreak.Gui {
/// <summary>One of the type styles (<c>vb-t-display</c> … <c>vb-t-num</c>). The three variants are body (<see cref="Body"/>, <see cref="BodySm"/>, <see cref="Caption"/>), label (<see cref="LabelSm"/> = <see cref="Badge"/>, <see cref="Label"/>, <see cref="LabelLg"/>) and heading (<see cref="HeadingSm"/>, <see cref="Heading"/> = <see cref="Title"/>, <see cref="HeadingLg"/> = <see cref="Display"/>); the old names stay as aliases and the new ones are appended, so no value moves.</summary>
public enum TypeStyle { Display, Title, Label, Badge, Body, Caption, Num, Heading, HeadingSm, HeadingLg, LabelSm, LabelLg, BodySm }
/// <summary>A recolour on top of a type style; <see cref="Default"/> keeps the style's own colour.</summary>
public enum TextTone { Default, Ally, Enemy, Power, Muted }
/// <summary>The caps eyebrow sizes: the label variant's regular, small (<see cref="Badge"/>) and large sizes.</summary>
public enum LabelSize { Label, Badge, Large }

/// <summary>Every run of text: a Label with one type style and an optional tone, so UXML reads
/// <c>&lt;vb:TextComponent type-style="Caption" tone="Muted" text="…"/&gt;</c> instead of juggling <c>vb-t-*</c> classes.
/// Pass caps yourself (UI Toolkit has no text-transform) or use <see cref="LabelComponent"/>.</summary>
[UxmlElement] public partial class TextComponent:Label {
 public const string ClassName="vb-text";
 TypeStyle type;TextTone tone;
 [UxmlAttribute] public TypeStyle TypeStyle {get=>type;set{RemoveFromClassList(StyleClass(type));type=value;AddToClassList(StyleClass(type));}}
 [UxmlAttribute] public TextTone Tone {get=>tone;set{if(tone!=TextTone.Default)RemoveFromClassList(ToneClass(tone));tone=value;if(tone!=TextTone.Default)AddToClassList(ToneClass(tone));}}
 public TextComponent():this("",TypeStyle.Body){}
 public TextComponent(string text,TypeStyle style=TypeStyle.Body,TextTone tone=TextTone.Default):base(text){AddToClassList(ClassName);pickingMode=PickingMode.Ignore;type=style;AddToClassList(StyleClass(style));Tone=tone;}
 /// <summary><c>vb-t-display</c>, <c>vb-t-caption</c> …</summary>
 public static string StyleClass(TypeStyle s)=>"vb-t-"+Ui.Kebab(s.ToString());
 /// <summary><c>vb-t-ally</c>, <c>vb-t-enemy</c>, <c>vb-t-power</c>, <c>vb-t-muted</c>; empty for the default tone.</summary>
 public static string ToneClass(TextTone t)=>t==TextTone.Default?"":"vb-t-"+Ui.Kebab(t.ToString());
}

/// <summary>The caps eyebrow over a section, field or value: tracked-out, small, secondary. It uppercases what it is given and
/// carries the section rhythm (4 units under it), replacing <c>vb-t-label</c> + <c>vb-section__title</c> / <c>vb-page__label</c>.</summary>
[UxmlElement] public partial class LabelComponent:TextComponent {
 public new const string ClassName="vb-label";
 [UxmlAttribute] public LabelSize Size {get=>TypeStyle==TypeStyle.Badge?LabelSize.Badge:TypeStyle==TypeStyle.LabelLg?LabelSize.Large:LabelSize.Label;set=>TypeStyle=StyleOf(value);}
 static TypeStyle StyleOf(LabelSize size)=>size==LabelSize.Badge?TypeStyle.Badge:size==LabelSize.Large?TypeStyle.LabelLg:TypeStyle.Label;
 public override string text {get=>base.text;set=>base.text=(value??"").ToUpperInvariant();}
 public LabelComponent():this(""){}
 public LabelComponent(string text,LabelSize size=LabelSize.Label,TextTone tone=TextTone.Default):base(text,StyleOf(size),tone){AddToClassList(ClassName);}
}

public static partial class Ui {
 /// <summary>A <see cref="TextComponent"/> on <paramref name="parent"/>.</summary>
 public static TextComponent Text(VisualElement parent,string text,TypeStyle style,TextTone tone=TextTone.Default)=>new TextComponent(text,style,tone).Into(parent);
 /// <summary>A <see cref="LabelComponent"/> on <paramref name="parent"/>.</summary>
 public static LabelComponent Eyebrow(VisualElement parent,string text,LabelSize size=LabelSize.Label,TextTone tone=TextTone.Default)=>new LabelComponent(text,size,tone).Into(parent);
}
}
