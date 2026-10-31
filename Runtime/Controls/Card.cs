using UnityEngine.UIElements;
namespace Veilbreak.Gui {
/// <summary>The base card: a slim container with an optional media band, a head (caps title and a meta), a body and a foot of
/// right-aligned actions. One frame, one padding rhythm (12 / 14) and one selected state (edge + brackets) for every card.
/// Children added in UXML or with <c>Add</c> go to the body.</summary>
[UxmlElement] public partial class CardComponent:VisualElement {
 public const string ClassName="vb-card";
 readonly VisualElement media,head,body,foot;readonly LabelComponent title;readonly Label meta;bool selected;
 public override VisualElement contentContainer=>body;
 /// <summary>An optional band above the head for art, a crop or a backdrop; hidden until something is added.</summary>
 public VisualElement Media=>media;
 public VisualElement Body=>body;
 /// <summary>Actions, right-aligned; hidden while empty.</summary>
 public VisualElement Foot=>foot;
 [UxmlAttribute] public string Title {get=>title.text;set{title.text=value;Refresh();}}
 [UxmlAttribute] public string Meta {get=>meta.text;set{meta.text=value??"";meta.Show(meta.text.Length>0);Refresh();}}
 [UxmlAttribute] public bool Selected {get=>selected;set{selected=value;EnableInClassList(ClassName+"--selected",value);}}
 public CardComponent():this(null){}
 public CardComponent(string title,string meta=null){
  AddToClassList(ClassName);VbShape.Attach(this);pickingMode=PickingMode.Ignore;
  media=Part(ClassName+"__media");head=Part(ClassName+"__head");this.title=new LabelComponent("").Classes(ClassName+"__title").Into(head);this.meta=Ui.Text(head,"","vb-t-badge",ClassName+"__meta");
  body=Part(ClassName+"__body");foot=Part(ClassName+"__foot");
  Title=title;Meta=meta;RegisterCallback<GeometryChangedEvent>(_=>Refresh());
 }
 VisualElement Part(string cls){var e=new VisualElement{pickingMode=PickingMode.Ignore};e.AddToClassList(cls);hierarchy.Add(e);return e;}
 void Refresh(){head.Show(title.text.Length>0||meta.text.Length>0);media.Show(media.childCount>0);foot.Show(foot.childCount>0);}
 /// <summary>Adds actions to the foot and returns the card.</summary>
 public CardComponent Actions(params VisualElement[] actions){foreach(var a in actions)foot.Add(a);Refresh();return this;}
}
}
