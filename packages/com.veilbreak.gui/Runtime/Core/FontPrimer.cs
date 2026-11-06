using UnityEngine.UIElements;
namespace Veilbreak.Gui {
/// <summary>Adds the kit's glyphs to every font face's dynamic atlas before a screen lays out text (ARC-51): one off-screen label
/// per face is measured on the first layout pass and then removed. Glyph metrics then never depend on which characters a session
/// happened to show first, which is what dropped title tracking after long sessions. Each text style a popover or a card prints in
/// mixed case (<see cref="Styles"/>) is primed too, at its own face and size: a title first drawn in a face that had only been primed at another
/// size read with uneven tracking ("Tax e d", "Momentum  Tax", design-system#26).</summary>
public static class FontPrimer {
 public const string ClassName="vb-font-primer";
 /// <summary>Printable ASCII plus the symbols the kit prints (separators, multipliers, minus, ellipsis, dashes, quotes, arrows).</summary>
 public static readonly string Glyphs=Ascii()+"·×−…–—‘’“”•°←↑→↓›‹";
 /// <summary>The USS face modifiers, one per <c>--vb-font-*</c> token.</summary>
 public static readonly string[] Faces={"display-regular","display-bold","body-regular","body-medium","body-semibold","body-bold","hud-regular"};
 /// <summary>The USS classes of the text styles primed at their own size: the type styles that carry titles, the popover and effect-detail titles and the choice-card title.</summary>
 public static readonly string[] Styles={"vb-t-display","vb-t-title","vb-t-label","vb-t-badge","vb-effect-detail__title","vb-popover-card__title","vb-choice__title"};
 static string Ascii(){var c=new char[95];for(int i=0;i<c.Length;i++)c[i]=(char)(32+i);return new string(c);}
 /// <summary>Primes the atlases under <paramref name="host"/>; the labels remove themselves after their first layout.</summary>
 public static void Prime(VisualElement host){
  if(host==null||host.Q(className:ClassName)!=null)return;
  var box=new VisualElement{pickingMode=PickingMode.Ignore};box.AddToClassList(ClassName);
  foreach(var face in Faces){var label=new Label(Glyphs){pickingMode=PickingMode.Ignore};label.AddToClassList(ClassName+"__"+face);box.Add(label);}
  foreach(var style in Styles){var label=new Label(Glyphs){pickingMode=PickingMode.Ignore};label.AddToClassList(style);label.AddToClassList(ClassName+"__style");box.Add(label);}
  box.RegisterCallback<GeometryChangedEvent>(_=>box.RemoveFromHierarchy());host.Add(box);
 }
}
}
