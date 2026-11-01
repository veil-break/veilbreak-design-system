using System;
using UnityEngine;
using UnityEngine.UIElements;
namespace Veilbreak.Gui {
public enum PopoverPlacement { Below, Above }

/// <summary>
/// Hover/focus popovers that also work on touch: a pointer that cannot hover toggles on tap.
/// Content is built on demand and placed on the panel's root (so it is never clipped), kept inside the panel's edges.
/// Styles reach it through the panel theme, so put the kit's stylesheets in the theme (VeilbreakGui.tss), not only on a document.
/// It copies the anchor's root flags (<see cref="SafeAreaRoot.RootClasses"/>), so high contrast, larger text and the motion settings reach it too.
/// </summary>
public sealed class Popover {
 public const string ClassName="vb-popover",OpenClass="vb-popover-open";
 const float Gap=6,Edge=8;
 static readonly System.Collections.Generic.List<Popover> active=new System.Collections.Generic.List<Popover>();
 public static bool CloseAll(){bool any=active.Count>0;foreach(var p in active.ToArray())p.Close();return any;}
 readonly VisualElement anchor;readonly Func<VisualElement> build;readonly PopoverPlacement placement;readonly bool alignEnd;
 sealed class Popup:OverlayView {public readonly ScrollView scroll=new ScrollView(ScrollViewMode.Vertical);public Action closing;public Popup(){hierarchy.Add(scroll);}public override void Dismiss(Action done=null){closing?.Invoke();base.Dismiss(done);}}
 VisualElement eventRoot;Popup shown;bool pinned,hovered,suppressFocus;IVisualElementScheduledItem pending;
 VisualElement Root=>anchor.panel?.visualTree;
 public static bool CloseWithin(VisualElement root){bool any=false;foreach(var p in active.ToArray())if(root==p.anchor||root.Contains(p.anchor)){p.Close();any=true;}return any;}
 bool ChildOpen(){if(shown==null)return false;foreach(var child in active)if(child!=this&&child.IsOpen&&shown.Contains(child.anchor))return true;return false;}
 void Later(){pending?.Pause();pending=anchor.schedule.Execute(()=>{if(!pinned&&!hovered&&!ChildOpen()&&!(shown?.Contains(anchor.panel?.focusController?.focusedElement as VisualElement)??false))Close();}).StartingIn(160);}
 void Outside(PointerDownEvent e){var target=e.target as VisualElement;if(target?.GetFirstAncestorOfType<Popup>()!=null)return;if(target==null||anchor==target||anchor.Contains(target)||shown==target||(shown?.Contains(target)??false))return;Close();}
 void Key(KeyDownEvent e){if(e.keyCode==KeyCode.Escape&&active.Count>0&&active[active.Count-1]==this){Close();suppressFocus=true;anchor.Focus();suppressFocus=false;e.StopPropagation();}}

 public bool IsOpen=>shown!=null;

 Popover(VisualElement anchor,Func<VisualElement> build,PopoverPlacement placement,bool alignEnd){
  this.anchor=anchor;this.build=build;this.placement=placement;this.alignEnd=alignEnd;
  anchor.RegisterCallback<PointerEnterEvent>(e=>{if(e.pointerType==UnityEngine.UIElements.PointerType.mouse){pending?.Pause();Open();}});
  anchor.RegisterCallback<PointerLeaveEvent>(e=>{if(e.pointerType==UnityEngine.UIElements.PointerType.mouse&&!pinned)Later();});
  anchor.RegisterCallback<PointerDownEvent>(e=>{if(e.pointerType!=UnityEngine.UIElements.PointerType.mouse){if(IsOpen)Close();else{Open();pinned=true;}e.StopPropagation();}});
  anchor.RegisterCallback<FocusInEvent>(_=>{if(!suppressFocus)Open();});
  anchor.RegisterCallback<FocusOutEvent>(_=>{if(!pinned)Later();});
  anchor.RegisterCallback<DetachFromPanelEvent>(_=>Close());
  anchor.RegisterCallback<GeometryChangedEvent>(_=>Place());
 }

 public static Popover Attach(VisualElement anchor,Func<VisualElement> build,PopoverPlacement placement=PopoverPlacement.Below,bool alignEnd=false)=>new Popover(anchor,build,placement,alignEnd);

 /// <summary>Keeps the popover open until <see cref="Close"/> (galleries, tutorials, captures).</summary>
 public void Pin(){pinned=true;if(anchor.panel!=null)Open();else anchor.RegisterCallback<AttachToPanelEvent>(_=>{if(pinned)Open();});}

 public void Open(){
  if(shown!=null||anchor.panel==null)return;
  var content=build?.Invoke();if(content==null)return;var popup=new Popup();var scroll=popup.scroll;scroll.horizontalScrollerVisibility=ScrollerVisibility.Hidden;scroll.verticalScrollerVisibility=ScrollerVisibility.Auto;scroll.Add(content);shown=popup;popup.closing=()=>{if(shown==popup)Close();};
  active.Add(this);shown.style.maxWidth=Mathf.Max(80,anchor.panel.visualTree.worldBound.width-16);shown.style.maxHeight=Mathf.Max(80,anchor.panel.visualTree.worldBound.height-32);shown.AddToClassList(ClassName);foreach(var c in SafeAreaRoot.RootClasses)if(Ui.Under(anchor,c))shown.AddToClassList(c);shown.pickingMode=PickingMode.Position;shown.style.position=Position.Absolute;
  shown.RegisterCallback<PointerEnterEvent>(_=>{hovered=true;pending?.Pause();});shown.RegisterCallback<PointerLeaveEvent>(_=>{hovered=false;Later();});shown.RegisterCallback<FocusOutEvent>(_=>Later());
  eventRoot=Root;eventRoot.RegisterCallback<PointerDownEvent>(Outside,TrickleDown.TrickleDown);eventRoot.RegisterCallback<KeyDownEvent>(Key,TrickleDown.TrickleDown);
  OverlayHost.For(anchor).AddPopup(shown);anchor.AddToClassList(OpenClass);
  shown.RegisterCallback<GeometryChangedEvent>(_=>Place());Place();
 }

 public void Close(){pending?.Pause();pending=null;active.Remove(this);pinned=hovered=false;if(shown==null)return;var leaving=shown;shown=null;
  eventRoot?.UnregisterCallback<PointerDownEvent>(Outside,TrickleDown.TrickleDown);eventRoot?.UnregisterCallback<KeyDownEvent>(Key,TrickleDown.TrickleDown);
  leaving.Owner?.RemovePopup(leaving);anchor.RemoveFromClassList(OpenClass);foreach(var parent in active.ToArray())if(parent.shown!=null&&parent.shown.Contains(anchor))parent.Later();}


 void Place(){
  if(shown?.parent==null)return;
  Rect a=anchor.worldBound,host=shown.parent.worldBound;float w=shown.resolvedStyle.width,h=shown.resolvedStyle.height;
  if(float.IsNaN(w)||float.IsNaN(h))return;
  float x=alignEnd?a.xMax-w:a.xMin,y=placement==PopoverPlacement.Below?a.yMax+Gap:a.yMin-Gap-h;
  if(placement==PopoverPlacement.Below&&y+h>host.yMax-Edge)y=a.yMin-Gap-h;
  if(placement==PopoverPlacement.Above&&y<host.yMin+Edge)y=a.yMax+Gap;
  x=Mathf.Clamp(x,host.xMin+Edge,Mathf.Max(host.xMin+Edge,host.xMax-w-Edge));
  y=Mathf.Clamp(y,host.yMin+Edge,Mathf.Max(host.yMin+Edge,host.yMax-h-Edge));
  shown.style.left=x-host.xMin;shown.style.top=y-host.yMin;
 }
}
}
