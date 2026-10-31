using System;
using UnityEngine;
using UnityEngine.UIElements;
namespace Veilbreak.Gui {
/// <summary>A bottom drawer: a slim solid sheet that slides up over a dimmed backdrop, with a grab handle, an optional caps title,
/// meta and close square, a scrolling body and a foot of actions. Tapping the backdrop, the close square or dragging the handle
/// down past a quarter of the sheet dismisses it (<see cref="Dismissed"/>); Back pops it when it was pushed. Children go to the body.
/// Inline (<see cref="Backdrop"/> off) it docks inside a view without blocking what is above the sheet.</summary>
[UxmlElement] public partial class DrawerOverlay:VisualElement {
 public const string ClassName="vb-drawer",OpenClass=ClassName+"--open",DraggingClass=ClassName+"--dragging";
 const int SlideMs=240;
 readonly VisualElement backdrop,sheet,grip,head,foot;readonly ScrollView body;readonly Label title,meta;readonly IconButtonComposite close;
 bool open,dismissible=true,hasBackdrop=true,closeButton=true;float dragFrom,dragY;IVisualElementScheduledItem pending;
 /// <summary>Raised when the player dismisses the drawer (backdrop, close square, drag down). Not raised by <see cref="Close"/>.</summary>
 public event Action Dismissed;
 public override VisualElement contentContainer=>body.contentContainer;
 public VisualElement Sheet=>sheet;
 public VisualElement Body=>body.contentContainer;
 /// <summary>Actions under the body (primary first); hidden while empty.</summary>
 public VisualElement Foot=>foot;
 public bool IsOpen=>open;
 [UxmlAttribute] public string Title {get=>title.text;set{title.text=(value??"").ToUpperInvariant();Refresh();}}
 [UxmlAttribute] public string Meta {get=>meta.text;set{meta.text=value??"";Refresh();}}
 /// <summary>Dim and block what is under the drawer (default). Off: the drawer docks inside a view and only the sheet takes input.</summary>
 [UxmlAttribute] public bool Backdrop {get=>hasBackdrop;set{hasBackdrop=value;backdrop.Show(value);pickingMode=value?PickingMode.Position:PickingMode.Ignore;EnableInClassList(ClassName+"--inline",!value);}}
 /// <summary>Whether the player can dismiss it (backdrop, close, drag). Off: the handle stays as an affordance but does nothing.</summary>
 [UxmlAttribute] public bool Dismissible {get=>dismissible;set{dismissible=value;Refresh();}}
 [UxmlAttribute] public bool CloseButton {get=>closeButton;set{closeButton=value;Refresh();}}
 public DrawerOverlay(){
  AddToClassList(ClassName);pickingMode=PickingMode.Position;
  backdrop=Part(this,ClassName+"__backdrop");backdrop.pickingMode=PickingMode.Position;backdrop.RegisterCallback<PointerDownEvent>(e=>{Dismiss();e.StopPropagation();});
  sheet=VbShape.Attach(Part(this,ClassName+"__sheet"));sheet.pickingMode=PickingMode.Position;
  grip=Part(sheet,ClassName+"__grip");grip.pickingMode=PickingMode.Position;Part(grip,ClassName+"__handle");grip.tooltip="Drag down to close";
  head=Part(sheet,ClassName+"__head");head.pickingMode=PickingMode.Position;title=new Label{pickingMode=PickingMode.Ignore};title.AddToClassList("vb-t-title");title.AddToClassList(ClassName+"__title");head.Add(title);
  meta=new Label{pickingMode=PickingMode.Ignore};meta.AddToClassList("vb-t-badge");meta.AddToClassList(ClassName+"__meta");head.Add(meta);
  close=new IconButtonComposite("close","Close",Dismiss).Classes(ClassName+"__close").Into(head);
  body=new ScrollView(ScrollViewMode.Vertical){horizontalScrollerVisibility=ScrollerVisibility.Hidden,verticalScrollerVisibility=ScrollerVisibility.Hidden};body.AddToClassList(ClassName+"__body");sheet.Add(body);
  foot=Part(sheet,ClassName+"__foot");Drag(grip);Drag(head);
  this.Show(false);Refresh();RegisterCallback<GeometryChangedEvent>(_=>foot.Show(foot.childCount>0));
 }
 static VisualElement Part(VisualElement parent,string cls){var e=new VisualElement{pickingMode=PickingMode.Ignore};e.AddToClassList(cls);parent.hierarchy.Add(e);return e;}
 void Refresh(){meta.Show(meta.text.Length>0);close.Show(closeButton&&dismissible);head.Show(title.text.Length>0||meta.text.Length>0||(closeButton&&dismissible));foot.Show(foot.childCount>0);}
 /// <summary>Adds actions to the foot and returns the drawer.</summary>
 public DrawerOverlay Actions(params VisualElement[] actions){foreach(var a in actions)foot.Add(a);Refresh();return this;}
 bool Calm=>Ui.Under(this,SafeAreaRoot.ReducedMotion);
 /// <summary>Shows the drawer and slides the sheet up (at once under reduced motion).</summary>
 public DrawerOverlay Open(){pending?.Pause();open=true;this.Show(true);sheet.style.translate=StyleKeyword.Null;if(Calm||panel==null){pending=null;AddToClassList(OpenClass);}else pending=schedule.Execute(()=>{if(open)AddToClassList(OpenClass);}).StartingIn(16);return this;}
 /// <summary>Slides the sheet down, hides the drawer, then runs <paramref name="done"/>. Raises nothing.</summary>
 public void Close(Action done=null){pending?.Pause();open=false;RemoveFromClassList(OpenClass);RemoveFromClassList(DraggingClass);sheet.style.translate=StyleKeyword.Null;
  if(Calm||panel==null){pending=null;this.Show(false);done?.Invoke();return;}pending=schedule.Execute(()=>{if(!open)this.Show(false);done?.Invoke();}).StartingIn(SlideMs);}
 /// <summary>What the backdrop, the close square and a long enough drag do: close, then raise <see cref="Dismissed"/>.</summary>
 public bool Dismiss(){if(!open||!dismissible)return false;Close();Dismissed?.Invoke();return true;}
 void Drag(VisualElement handle){
  handle.RegisterCallback<PointerDownEvent>(e=>{if(!dismissible||!open)return;dragFrom=e.position.y;dragY=0;handle.CapturePointer(e.pointerId);AddToClassList(DraggingClass);e.StopPropagation();});
  handle.RegisterCallback<PointerMoveEvent>(e=>{if(!handle.HasPointerCapture(e.pointerId))return;dragY=Mathf.Max(0,e.position.y-dragFrom);sheet.style.translate=new Translate(0,dragY);});
  handle.RegisterCallback<PointerUpEvent>(e=>{if(!handle.HasPointerCapture(e.pointerId))return;handle.ReleasePointer(e.pointerId);RemoveFromClassList(DraggingClass);
   if(dragY>Mathf.Max(80,sheet.resolvedStyle.height*.25f))Dismiss();else sheet.style.translate=StyleKeyword.Null;dragY=0;});
  handle.RegisterCallback<PointerCaptureOutEvent>(_=>{RemoveFromClassList(DraggingClass);if(open)sheet.style.translate=StyleKeyword.Null;});
 }
 /// <summary>A drawer on a new modal layer over the navigator's page. Back pops it at once; a dismiss slides it down first.
 /// <paramref name="closed"/> runs once whichever way it closes.</summary>
 public static DrawerOverlay Push(Navigator nav,string title=null,Action closed=null){
  var layer=nav.Push(closed);layer.AddToClassList("vb-modal--drawer");var d=new DrawerOverlay{Title=title}.Into(layer);
  d.Dismissed+=()=>d.schedule.Execute(()=>{if(layer.parent!=null&&nav.Modals.Count>0)nav.Modals.Pop();}).StartingIn(d.Calm?0:SlideMs);
  return d.Open();
 }
}
}
