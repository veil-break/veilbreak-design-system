using UnityEngine;
using UnityEngine.UIElements;
namespace Veilbreak.Gui {
/// <summary>Confirmation sheets: a solid panel with a title and a line of body text; callers add the actions.
/// <see cref="Push"/> puts one on a modal layer, which blurs what is under it and returns focus when it closes.</summary>
public static class Dialog {
 public const string ClassName="vb-dialog";
 /// <summary>A dialog sheet on <paramref name="parent"/>; add buttons to the returned panel.</summary>
 public static VisualElement Sheet(VisualElement parent,string title,string body=null,string bodyClass="vb-t-caption",params string[] classes){var box=Ui.Panel(parent,ClassName,"vb-panel--solid").Classes(classes);
  if(!string.IsNullOrEmpty(title))Ui.Text(box,title,"vb-t-title",ClassName+"__title");if(!string.IsNullOrEmpty(body))Ui.Text(box,body,bodyClass,ClassName+"__body");return box;}
 /// <summary>A dialog sheet on a new modal layer over the navigator's current page.</summary>
 public static VisualElement Push(Navigator navigator,string title,string body=null,string bodyClass="vb-t-caption",params string[] classes)=>Sheet(navigator.Push(),title,body,bodyClass,classes);
}

/// <summary>Full-bleed artwork behind a screen, scaled to cover and dimmed a little, with a gradient shade along the bottom so text over it stays legible.</summary>
[UxmlElement] public partial class Backdrop:VisualElement {
 public const string ClassName="vb-backdrop";
 public Backdrop():this(null){}
 public Backdrop(Texture2D image){AddToClassList(ClassName);pickingMode=PickingMode.Ignore;Ui.Image(this,image);VbShape.Attach(Ui.Box(this,ClassName+"__shade"));}
}
}
