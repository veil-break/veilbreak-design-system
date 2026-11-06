using System;
using UnityEngine;
using UnityEngine.UIElements;
namespace Veilbreak.Gui {
public enum PopoverPlacement { Below, Above }

/// <summary>
/// Hover/focus popovers that also work on touch: a pointer that cannot hover toggles on tap.
/// Content is built on demand and placed on the panel's root (so it is never clipped), as one solid surface with padding and a small arrow
/// toward its anchor. Its height is clamped to the panel (the content scrolls past that) and it is placed again whenever the panel, the anchor
/// or the popover changes size, so it never leaves the board.
/// Styles reach it through the panel theme, so put the kit's stylesheets in the theme (VeilbreakGui.tss), not only on a document.
/// It copies the anchor's root flags (<see cref="SafeAreaRoot.RootClasses"/>), so high contrast, larger text and the motion settings reach it too.
/// </summary>
public sealed class Popover {
 public const string ClassName="vb-popover",OpenClass="vb-popover-open";
 /// <summary>Space between anchor and popover (room for the arrow), the margin kept to the panel's edges, the widest a popover grows and the arrow's inset from the corners.</summary>
 public const float Gap=9,Edge=8,MaxWidth=340,ArrowInset=14;
 static readonly System.Collections.Generic.List<Popover> active=new System.Collections.Generic.List<Popover>();
 public static bool CloseAll(){bool any=active.Count>0;foreach(var p in active.ToArray())p.Close();return any;}
 readonly VisualElement anchor;readonly Func<VisualElement> build;readonly PopoverPlacement placement;readonly bool alignEnd;
 sealed class Popup:OverlayView {public readonly ScrollView scroll=new ScrollView(ScrollViewMode.Vertical);public readonly VisualElement arrow=new VisualElement{pickingMode=PickingMode.Ignore};public Action closing;public Popup(){VbShape.Attach(this);hierarchy.Add(scroll);arrow.AddToClassList(ClassName+"__arrow");hierarchy.Add(arrow);PaintArrow(arrow);}public override void Dismiss(Action done=null){closing?.Invoke();base.Dismiss(done);}}
 // The arrow is painted, not bordered: a triangle in the popover's own --vb-fill with its two sides in --vb-edge (both inherited from the popover, so high contrast
 // reaches it), and a one-unit base in the fill that hides the popover's edge under it.
 static void PaintArrow(VisualElement arrow){Color fill=Color.clear,edge=Color.clear;var fillP=new CustomStyleProperty<Color>("--vb-fill");var edgeP=new CustomStyleProperty<Color>("--vb-edge");
  arrow.RegisterCallback<CustomStyleResolvedEvent>(_=>{arrow.customStyle.TryGetValue(fillP,out fill);arrow.customStyle.TryGetValue(edgeP,out edge);arrow.MarkDirtyRepaint();});
  arrow.RegisterCallback<GeometryChangedEvent>(_=>arrow.MarkDirtyRepaint());
  arrow.generateVisualContent+=ctx=>{float w=arrow.layout.width,h=arrow.layout.height;if(w<=0||h<=2)return;bool up=arrow.ClassListContains(ClassName+"__arrow--up");float tip=up?0:h-1,@base=up?h-1:0;var p=ctx.painter2D;
   p.fillColor=fill;p.BeginPath();p.MoveTo(new Vector2(0,@base));p.LineTo(new Vector2(w*.5f,tip));p.LineTo(new Vector2(w,@base));p.ClosePath();p.Fill();
   p.fillColor=fill;p.BeginPath();p.MoveTo(new Vector2(1,up?h-1:0));p.LineTo(new Vector2(w-1,up?h-1:0));p.LineTo(new Vector2(w-1,up?h:1));p.LineTo(new Vector2(1,up?h:1));p.ClosePath();p.Fill();
   p.strokeColor=edge;p.lineWidth=1;p.lineJoin=LineJoin.Miter;p.BeginPath();p.MoveTo(new Vector2(0,@base));p.LineTo(new Vector2(w*.5f,tip));p.LineTo(new Vector2(w,@base));p.Stroke();};}
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
  var content=build?.Invoke();if(content==null)return;var popup=new Popup();var scroll=popup.scroll;scroll.horizontalScrollerVisibility=ScrollerVisibility.Hidden;scroll.verticalScrollerVisibility=ScrollerVisibility.Auto;content.AddToClassList(ClassName+"__content");scroll.Add(content);shown=popup;popup.closing=()=>{if(shown==popup)Close();};
  active.Add(this);var limit=Limit(anchor.panel.visualTree.worldBound);shown.style.maxWidth=limit.x;shown.style.maxHeight=limit.y;shown.AddToClassList(ClassName);foreach(var c in SafeAreaRoot.RootClasses)if(Ui.Under(anchor,c))shown.AddToClassList(c);shown.pickingMode=PickingMode.Position;shown.style.position=Position.Absolute;
  shown.RegisterCallback<PointerEnterEvent>(_=>{hovered=true;pending?.Pause();});shown.RegisterCallback<PointerLeaveEvent>(_=>{hovered=false;Later();});shown.RegisterCallback<FocusOutEvent>(_=>Later());
  eventRoot=Root;eventRoot.RegisterCallback<PointerDownEvent>(Outside,TrickleDown.TrickleDown);eventRoot.RegisterCallback<KeyDownEvent>(Key,TrickleDown.TrickleDown);eventRoot.RegisterCallback<GeometryChangedEvent>(PanelResized);
  OverlayHost.For(anchor).AddPopup(shown);anchor.AddToClassList(OpenClass);
  shown.RegisterCallback<GeometryChangedEvent>(_=>Place());Place();
 }

 public void Close(){pending?.Pause();pending=null;active.Remove(this);pinned=hovered=false;if(shown==null)return;var leaving=shown;shown=null;
  eventRoot?.UnregisterCallback<PointerDownEvent>(Outside,TrickleDown.TrickleDown);eventRoot?.UnregisterCallback<KeyDownEvent>(Key,TrickleDown.TrickleDown);eventRoot?.UnregisterCallback<GeometryChangedEvent>(PanelResized);
  leaving.Owner?.RemovePopup(leaving);anchor.RemoveFromClassList(OpenClass);foreach(var parent in active.ToArray())if(parent.shown!=null&&parent.shown.Contains(anchor))parent.Later();}


 void PanelResized(GeometryChangedEvent e){if(shown!=null)Place();}
 /// <summary>Where a popover <paramref name="size"/> big goes, in <paramref name="host"/>'s own coordinates: below or above <paramref name="anchor"/> (it flips to the
 /// other side when it would leave the panel), aligned to the anchor's start or end, then clamped inside the panel's edges. <paramref name="below"/> says
 /// which side it ended on and <paramref name="arrow"/> is where its arrow points, from the popover's left edge (clamped to the corners).</summary>
 public static Vector2 Fit(Rect host,Rect anchor,Vector2 size,PopoverPlacement placement,bool alignEnd,out bool below,out float arrow){
  float w=size.x,h=size.y;float x=alignEnd?anchor.xMax-w:anchor.xMin,y=placement==PopoverPlacement.Below?anchor.yMax+Gap:anchor.yMin-Gap-h;
  if(placement==PopoverPlacement.Below&&y+h>host.yMax-Edge)y=anchor.yMin-Gap-h;
  if(placement==PopoverPlacement.Above&&y<host.yMin+Edge)y=anchor.yMax+Gap;
  x=Mathf.Clamp(x,host.xMin+Edge,Mathf.Max(host.xMin+Edge,host.xMax-w-Edge));
  y=Mathf.Clamp(y,host.yMin+Edge,Mathf.Max(host.yMin+Edge,host.yMax-h-Edge));
  below=y+h*.5f>anchor.center.y;arrow=Mathf.Clamp(anchor.center.x-x,Mathf.Min(ArrowInset,w*.5f),Mathf.Max(w-ArrowInset,w*.5f));
  return new Vector2(x-host.xMin,y-host.yMin);}
 /// <summary>The widest and tallest a popover may be on a panel <paramref name="host"/> big: the panel less its edges, and at most <see cref="MaxWidth"/> wide.</summary>
 public static Vector2 Limit(Rect host)=>new Vector2(Mathf.Min(MaxWidth,Mathf.Max(80,host.width-2*Edge)),Mathf.Max(80,host.height-2*Edge));
 void Place(){
  if(shown?.parent==null)return;
  Rect a=anchor.worldBound,host=shown.parent.worldBound;
  var limit=Limit(host);shown.style.maxWidth=limit.x;shown.style.maxHeight=limit.y;
  float w=shown.resolvedStyle.width,h=shown.resolvedStyle.height;
  if(float.IsNaN(w)||float.IsNaN(h))return;
  var at=Fit(host,a,new Vector2(w,h),placement,alignEnd,out bool below,out float arrow);
  shown.style.left=at.x;shown.style.top=at.y;shown.arrow.style.left=arrow-shown.arrow.resolvedStyle.width*.5f;
  shown.arrow.EnableInClassList(ClassName+"__arrow--up",below);shown.arrow.EnableInClassList(ClassName+"__arrow--down",!below);shown.arrow.MarkDirtyRepaint();
 }
}

/// <summary>Popover content in the kit's layout: a caps title, an optional hint line under it, then flat rows (<see cref="KeyValueRow"/> or your own) that
/// sit one tone above the popover's surface and carry no edge. A queue row's cancel goes at the row's end.</summary>
public sealed class PopoverCard:ViewBase {
 public const string ClassName="vb-popover-card";
 readonly Label title,hint;readonly VisualElement rows;
 public Label TitleLabel=>title;public Label HintLabel=>hint;
 /// <summary>Where the rows go.</summary>
 public VisualElement Rows=>rows;
 public PopoverCard(string title=null,string hint=null){AddToClassList(ClassName);pickingMode=PickingMode.Ignore;this.title=Ui.Text(this,"",ClassName+"__title");this.hint=Ui.Text(this,"","vb-t-caption",ClassName+"__hint");rows=Ui.Box(this,ClassName+"__rows");Bind(title,hint);}
 public PopoverCard Bind(string title,string hint=null){this.title.text=(title??"").ToUpperInvariant();this.title.Show(this.title.text.Length>0);this.hint.text=hint??"";this.hint.Show(this.hint.text.Length>0);return this;}
 /// <summary>Adds a key and value row.</summary>
 public KeyValueRow Row(string key,string value,bool good=false)=>new KeyValueRow(key,value,good).Classes(ClassName+"__row").Into(rows);
}
}
