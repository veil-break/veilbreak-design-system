using System;
using UnityEngine;
using UnityEngine.UIElements;
namespace Veilbreak.Gui {
/// <summary>How far a drawer is open: half the screen (where it opens) or the full screen.</summary>
public enum DrawerSnap { Half, Full }

/// <summary>A bottom drawer: a slim solid sheet that slides up over a dimmed backdrop to half the screen, with a grab handle, an
/// optional caps title, meta and close square, a scrolling body and a foot of actions. Drag the handle or head: up snaps to the full
/// screen, down snaps back to half or, from below a quarter, closes it; a tap on the handle toggles half and full. Tapping the
/// backdrop or the close square dismisses it too (<see cref="Dismissed"/>); Back pops it when it was pushed. Children go to the body.
/// Inline (<see cref="Backdrop"/> off) it docks inside a view without blocking what is above the sheet.</summary>
[UxmlElement] public partial class DrawerOverlay:OverlayView {
 public const string ClassName="vb-drawer",OpenClass=ClassName+"--open",DraggingClass=ClassName+"--dragging",FullClass=ClassName+"--full";

 readonly VisualElement backdrop,sheet,grip,head,foot;readonly ScrollView body;readonly Label title,meta;readonly IconButtonComposite close;
 bool open,dismissible=true,hasBackdrop=true,closeButton=true,moved;float dragFrom,dragStart,dragH;DrawerSnap snap;
 /// <summary>Raised when the player dismisses the drawer (backdrop, close square, drag down). Not raised by <see cref="Close"/>.</summary>
 public event Action Dismissed;
 public override VisualElement contentContainer=>body.contentContainer;
 public VisualElement Sheet=>sheet;
 public VisualElement Body=>body.contentContainer;
 /// <summary>Actions under the body (primary first); hidden while empty.</summary>
 public VisualElement Foot=>foot;
 public bool IsOpen=>open;
 /// <summary>Half the screen (where it opens) or the full screen; the player changes it by dragging or tapping the handle.</summary>
 [UxmlAttribute] public DrawerSnap Snap {get=>snap;set{snap=value;EnableInClassList(FullClass,value==DrawerSnap.Full);sheet.style.height=StyleKeyword.Null;SnapChanged?.Invoke(value);}}
 public event Action<DrawerSnap> SnapChanged;
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
  grip=Part(sheet,ClassName+"__grip");grip.pickingMode=PickingMode.Position;Part(grip,ClassName+"__handle");grip.tooltip="Drag up for full screen, down to close";
  head=Part(sheet,ClassName+"__head");head.pickingMode=PickingMode.Position;title=new Label{pickingMode=PickingMode.Ignore};title.AddToClassList("vb-t-title");title.AddToClassList(ClassName+"__title");head.Add(title);
  meta=new Label{pickingMode=PickingMode.Ignore};meta.AddToClassList("vb-t-badge");meta.AddToClassList(ClassName+"__meta");head.Add(meta);
  close=new IconButtonComposite("close","Close",()=>Dismiss()).Classes(ClassName+"__close").Into(head);
  body=new ScrollView(ScrollViewMode.Vertical){horizontalScrollerVisibility=ScrollerVisibility.Hidden,verticalScrollerVisibility=ScrollerVisibility.Hidden};body.AddToClassList(ClassName+"__body");sheet.Add(body);
  foot=Part(sheet,ClassName+"__foot");Drag(grip);Drag(head);
  this.Show(false);Refresh();RegisterCallback<GeometryChangedEvent>(_=>foot.Show(foot.childCount>0));
 }
 static VisualElement Part(VisualElement parent,string cls){var e=new VisualElement{pickingMode=PickingMode.Ignore};e.AddToClassList(cls);parent.hierarchy.Add(e);return e;}
 void Refresh(){meta.Show(meta.text.Length>0);close.Show(closeButton&&dismissible);head.Show(title.text.Length>0||meta.text.Length>0||(closeButton&&dismissible));foot.Show(foot.childCount>0);}
 /// <summary>Adds actions to the foot and returns the drawer.</summary>
 public DrawerOverlay Actions(params VisualElement[] actions){foreach(var a in actions)foot.Add(a);Refresh();return this;}
 public override MotionRecipe Entrance=>MotionRecipes.DrawerIn;
 public override MotionRecipe Exit=>MotionRecipes.DrawerOut;
 public override void Present(){Open();}
 public override void Dismiss(Action done=null){Close(done);}
 /// <summary>Reversible recipe-driven sheet entrance; inline drawers leave surrounding input available.</summary>
 public DrawerOverlay Open(DrawerSnap at=DrawerSnap.Half){open=true;Snap=at;this.Show(true);AddToClassList(OpenClass);Phase=OverlayPhase.Opening;
  VbMotion.In(sheet,Entrance,0,()=>Phase=OverlayPhase.Open);VbMotion.In(backdrop,new MotionRecipe(Entrance.milliseconds));return this;}
 public void Close(Action done=null){open=false;RemoveFromClassList(DraggingClass);Phase=OverlayPhase.Closing;
  VbMotion.Out(backdrop,new MotionRecipe(Exit.milliseconds));VbMotion.Out(sheet,Exit,()=>{if(open)return;RemoveFromClassList(OpenClass);this.Show(false);sheet.style.height=StyleKeyword.Null;Phase=OverlayPhase.Closed;done?.Invoke();});}
 public bool Dismiss(){if(!open||!dismissible)return false;Dismissed?.Invoke();if(Owner==null)Close();return true;}
 /// <summary>The snap a released sheet of height <paramref name="h"/> settles on, in a drawer <paramref name="full"/> tall: below a quarter
 /// closes (null), up to three quarters is half, above is full.</summary>
 public static DrawerSnap? SnapFor(float h,float full)=>h<full*.25f?(DrawerSnap?)null:h<full*.75f?DrawerSnap.Half:DrawerSnap.Full;
 void Drag(VisualElement handle){
  handle.RegisterCallback<PointerDownEvent>(e=>{if(!open)return;dragFrom=e.position.y;dragStart=sheet.resolvedStyle.height;dragH=dragStart;moved=false;handle.CapturePointer(e.pointerId);AddToClassList(DraggingClass);e.StopPropagation();});
  handle.RegisterCallback<PointerMoveEvent>(e=>{if(!handle.HasPointerCapture(e.pointerId))return;float dy=e.position.y-dragFrom;moved|=Mathf.Abs(dy)>4;
   dragH=Mathf.Clamp(dragStart-dy,dismissible?0:resolvedStyle.height*.5f,resolvedStyle.height);sheet.style.height=dragH;});
  handle.RegisterCallback<PointerUpEvent>(e=>{if(!handle.HasPointerCapture(e.pointerId))return;handle.ReleasePointer(e.pointerId);RemoveFromClassList(DraggingClass);
   // A tap on the handle toggles half and full; a drag settles on the nearest snap, or closes below a quarter.
   if(!moved){if(handle==grip)Snap=snap==DrawerSnap.Full?DrawerSnap.Half:DrawerSnap.Full;else sheet.style.height=StyleKeyword.Null;return;}
   var to=SnapFor(dragH,resolvedStyle.height);if(to==null&&Dismiss())return;Snap=to??DrawerSnap.Half;});
  handle.RegisterCallback<PointerCaptureOutEvent>(_=>{RemoveFromClassList(DraggingClass);if(open)sheet.style.height=StyleKeyword.Null;});
 }
 /// <summary>A drawer on a new modal layer over the navigator's page. Back pops it at once; a dismiss slides it down first.
 /// <paramref name="closed"/> runs once whichever way it closes.</summary>
 public static DrawerOverlay Push(Navigator nav,string title=null,Action closed=null){
  var layer=nav.Push(closed);layer.AddToClassList("vb-modal--drawer");var d=new DrawerOverlay{Title=title}.Into(layer);
  d.Owner=nav.Modals;d.Dismissed+=()=>nav.Modals.Pop();
  return d.Open();
 }
}
}
