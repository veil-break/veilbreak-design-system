using UnityEngine.UIElements;
namespace Veilbreak.Gui {
/// <summary>Adds the kit's glyphs to every font face's dynamic atlas before a screen lays out text (ARC-51): one off-screen label
/// per face is measured on the first layout pass and then removed. Glyph metrics then never depend on which characters a session
/// happened to show first, which is what dropped title tracking after long sessions.</summary>
public static class FontPrimer {
 public const string ClassName="vb-font-primer";
 /// <summary>Printable ASCII plus the symbols the kit prints (separators, multipliers, ellipsis, dashes, quotes, arrows).</summary>
 public static readonly string Glyphs=Ascii()+"·×…–—‘’“”•°←↑→↓";
 /// <summary>The USS face modifiers, one per <c>--vb-font-*</c> token.</summary>
 public static readonly string[] Faces={"display-regular","display-bold","body-regular","body-medium","body-semibold","body-bold","hud-regular"};
 static string Ascii(){var c=new char[95];for(int i=0;i<c.Length;i++)c[i]=(char)(32+i);return new string(c);}
 /// <summary>Primes the atlases under <paramref name="host"/>; the labels remove themselves after their first layout.</summary>
 public static void Prime(VisualElement host){
  if(host==null||host.Q(className:ClassName)!=null)return;
  var box=new VisualElement{pickingMode=PickingMode.Ignore};box.AddToClassList(ClassName);
  foreach(var face in Faces){var label=new Label(Glyphs){pickingMode=PickingMode.Ignore};label.AddToClassList(ClassName+"__"+face);box.Add(label);}
  box.RegisterCallback<GeometryChangedEvent>(_=>box.RemoveFromHierarchy());host.Add(box);
 }
}
}
