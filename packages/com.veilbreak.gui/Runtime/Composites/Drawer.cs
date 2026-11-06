using System;
using UnityEngine;
using UnityEngine.UIElements;
namespace Veilbreak.Gui {
/// <summary>How far a drawer is open. <see cref="Fit"/> (where it opens) is as tall as its content, up to 90% of the screen, and the body scrolls only
/// past that; <see cref="Half"/> is half the screen (opt in); <see cref="Full"/> is the full screen. The values keep their numbers: Fit was added last.</summary>
public enum DrawerSnap { Half, Full, Fit }

/// <summary>A bottom drawer: a slim solid sheet that slides up over a dimmed backdrop to the height of its content (at most 90% of the screen,
/// <see cref="DrawerSnap.Fit"/>), with a grab handle, an optional caps title, meta and close square, a body that scrolls only past that height and a
/// foot of actions (an <see cref="ActionRow"/>). Drag the handle or head: up snaps to the full screen, down snaps back to the content height or, from
/// below a quarter, closes it; a tap on the handle toggles full and back. <see cref="DrawerSnap.Half"/> (half the screen) is opt-in. Tapping the
/// backdrop or the close square dismisses it too (<see cref="Dismissed"/>); Back pops it when it was pushed. Children go to the body.
/// Inline (<see cref="Backdrop"/> off) it docks inside a view without blocking what is above the sheet. A Fit drawer that is open and whose content changes height (an operative swapped for another, a section opened) eases its
/// sheet to the new height (<see cref="MotionRecipes.DrawerFit"/>, real time) instead of jumping; reduced motion jumps, and the recipe has no flash to reduce.</summary>
[UxmlElement] public partial class DrawerOverlay:OverlayView {
 public const string ClassName="vb-drawer",OpenClass=ClassName+"--open",DraggingClass=ClassName+"--dragging",FittingClass=ClassName+"--fitting",FullClass=ClassName+"--full",HalfClass=ClassName+"--half";
 /// <summary>Units a released sheet may sit above its content height before it settles on Full.</summary>
 public const float FitSlop=24;
 /// <summary>Whether the sheet is easing to a new content height.</summary>
 public bool Fitting=>fitting;
 bool fitting,fitEnding;double fitBegan;float fitFrom,fitTo;IVisualElementScheduledItem fitTimer;

 readonly VisualElement backdrop,sheet,grip,head;readonly ActionRow foot;readonly ScrollView body;readonly Label title,meta;readonly IconButtonComposite close;
 bool open,dismissible=true,hasBackdrop=true,closeButton=true,moved;float dragFrom,dragStart,dragH;DrawerSnap snap=DrawerSnap.Fit,rest=DrawerSnap.Fit,dragFrom0=DrawerSnap.Fit;
 /// <summary>Raised when the player dismisses the drawer (backdrop, close square, drag down). Not raised by <see cref="Close"/>.</summary>
 public event Action Dismissed;
 public override VisualElement contentContainer=>body.contentContainer;
 public VisualElement Sheet=>sheet;
 public VisualElement Body=>body.contentContainer;
 /// <summary>Actions under the body (primary first), an <see cref="ActionRow"/> that fills the sheet and stacks when narrow; hidden while empty.</summary>
 public ActionRow Foot=>foot;
 public bool IsOpen=>open;
 /// <summary>The content height (default), half the screen or the full screen; the player changes it by dragging or tapping the handle.</summary>
 [UxmlAttribute] public DrawerSnap Snap {get=>snap;set{snap=value;EnableInClassList(FullClass,value==DrawerSnap.Full);EnableInClassList(HalfClass,value==DrawerSnap.Half);CancelFit();sheet.style.height=StyleKeyword.Null;SnapChanged?.Invoke(value);}}
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
  foot=new ActionRow().Classes(ClassName+"__foot").Into(sheet);Drag(grip);Drag(head);sheet.RegisterCallback<GeometryChangedEvent>(Refit);sheet.RegisterCallback<DetachFromPanelEvent>(_=>EndFit());
  this.Show(false);Refresh();RegisterCallback<GeometryChangedEvent>(_=>foot.Show(foot.childCount>0));
 }
 // The sheet's own height changed (its content did) while it sits open at its content height and nobody is dragging it: ease from the old height to the new one. The sheet's own USS height transition (for the snaps) is off meanwhile
 // (--fitting): left on, it chases every height this sets and the sheet is still short of its content when the height is handed back.
 void Refit(GeometryChangedEvent e){
  if(fitting||!open||Phase!=OverlayPhase.Open||snap!=DrawerSnap.Fit||ClassListContains(DraggingClass)||sheet.style.height.keyword!=StyleKeyword.Null)return;
  float from=e.oldRect.height,to=e.newRect.height;if(!(from>0)||!(to>0)||Mathf.Abs(to-from)<1.5f||VbMotion.Calm(this))return;
  fitting=true;fitEnding=false;fitFrom=from;fitTo=to;fitBegan=VbMotion.Clock();AddToClassList(FittingClass);sheet.style.height=from;fitTimer?.Pause();fitTimer=sheet.schedule.Execute(StepFit).Every(16);
 }
 void StepFit(){
  float p=Mathf.Clamp01((float)((VbMotion.Clock()-fitBegan)*1000/Mathf.Max(1,MotionRecipes.DrawerFit.milliseconds)));
  // A drag owns the height from the moment it starts.
  if(ClassListContains(DraggingClass)){CancelFit();return;}
  if(VbMotion.Calm(this)||fitEnding){CancelFit();sheet.style.height=StyleKeyword.Null;return;}
  // The last frame holds the target height; the next hands the height back to the content.
  if(p>=1){fitEnding=true;sheet.style.height=fitTo;return;}
  sheet.style.height=Mathf.Lerp(fitFrom,fitTo,VbMotion.Ease(p));
 }
 // Ends an ease and gives the height back to the content (leaving the panel mid-ease must not keep an in-between height).
 void EndFit(){bool was=fitting;CancelFit();if(was)sheet.style.height=StyleKeyword.Null;}
 void CancelFit(){fitTimer?.Pause();fitTimer=null;if(fitting)RemoveFromClassList(FittingClass);fitting=false;fitEnding=false;}
 static VisualElement Part(VisualElement parent,string cls){var e=new VisualElement{pickingMode=PickingMode.Ignore};e.AddToClassList(cls);parent.hierarchy.Add(e);return e;}
 void Refresh(){meta.Show(meta.text.Length>0);close.Show(closeButton&&dismissible);head.Show(title.text.Length>0||meta.text.Length>0||(closeButton&&dismissible));foot.Show(foot.childCount>0);}
 /// <summary>Adds actions to the foot and returns the drawer.</summary>
 public DrawerOverlay Actions(params VisualElement[] actions){foreach(var a in actions)foot.Add(a);Refresh();return this;}
 public override MotionRecipe Entrance=>MotionRecipes.DrawerIn;
 public override MotionRecipe Exit=>MotionRecipes.DrawerOut;
 public override void Present(){Open();}
 public override void Dismiss(Action done=null){Close(done);}
 /// <summary>Reversible recipe-driven sheet entrance; inline drawers leave surrounding input available.</summary>
 public DrawerOverlay Open(DrawerSnap at=DrawerSnap.Fit){open=true;rest=at==DrawerSnap.Half?DrawerSnap.Half:DrawerSnap.Fit;Snap=at;this.Show(true);AddToClassList(OpenClass);Phase=OverlayPhase.Opening;
  VbMotion.In(sheet,Entrance,0,()=>Phase=OverlayPhase.Open);VbMotion.In(backdrop,new MotionRecipe(Entrance.milliseconds));return this;}
 public void Close(Action done=null){open=false;CancelFit();RemoveFromClassList(DraggingClass);Phase=OverlayPhase.Closing;
  VbMotion.Out(backdrop,new MotionRecipe(Exit.milliseconds));VbMotion.Out(sheet,Exit,()=>{if(open)return;RemoveFromClassList(OpenClass);this.Show(false);sheet.style.height=StyleKeyword.Null;Phase=OverlayPhase.Closed;done?.Invoke();});}
 public bool Dismiss(){if(!open||!dismissible)return false;Dismissed?.Invoke();if(Owner==null)Close();return true;}
 /// <summary>The snap a released sheet of height <paramref name="h"/> settles on, in a drawer <paramref name="full"/> tall, for a half-height drawer: below a
 /// quarter closes (null), up to three quarters is half, above is full.</summary>
 public static DrawerSnap? SnapFor(float h,float full)=>SnapFor(h,full,DrawerSnap.Half,DrawerSnap.Half,0);
 /// <summary>The snap a released sheet settles on. Below a quarter closes (null). A half drawer settles as above. A fit drawer settles on Full when the
 /// sheet is pulled more than <see cref="FitSlop"/> above its content height <paramref name="fit"/> (or, dragged from Full, stays above three
 /// quarters), otherwise back on its content height.</summary>
 public static DrawerSnap? SnapFor(float h,float full,DrawerSnap from,DrawerSnap rest,float fit){
  if(h<full*.25f)return null;if(rest==DrawerSnap.Half)return h<full*.75f?DrawerSnap.Half:DrawerSnap.Full;
  return from==DrawerSnap.Full?(h<full*.75f?DrawerSnap.Fit:DrawerSnap.Full):(h>fit+FitSlop?DrawerSnap.Full:DrawerSnap.Fit);}
 void Drag(VisualElement handle){
  handle.RegisterCallback<PointerDownEvent>(e=>{if(!open)return;CancelFit();dragFrom=e.position.y;dragStart=sheet.resolvedStyle.height;dragH=dragStart;dragFrom0=snap;moved=false;handle.CapturePointer(e.pointerId);AddToClassList(DraggingClass);e.StopPropagation();});
  handle.RegisterCallback<PointerMoveEvent>(e=>{if(!handle.HasPointerCapture(e.pointerId))return;float dy=e.position.y-dragFrom;moved|=Mathf.Abs(dy)>4;
   dragH=Mathf.Clamp(dragStart-dy,dismissible?0:Mathf.Min(dragStart,resolvedStyle.height*.5f),resolvedStyle.height);sheet.style.height=dragH;});
  handle.RegisterCallback<PointerUpEvent>(e=>{if(!handle.HasPointerCapture(e.pointerId))return;handle.ReleasePointer(e.pointerId);RemoveFromClassList(DraggingClass);
   // A tap on the handle toggles half and full; a drag settles on the nearest snap, or closes below a quarter.
   if(!moved){if(handle==grip)Snap=snap==DrawerSnap.Full?rest:DrawerSnap.Full;else sheet.style.height=StyleKeyword.Null;return;}
   var to=SnapFor(dragH,resolvedStyle.height,dragFrom0,rest,dragFrom0==DrawerSnap.Full?0:dragStart);if(to==null&&Dismiss())return;Snap=to??rest;});
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
