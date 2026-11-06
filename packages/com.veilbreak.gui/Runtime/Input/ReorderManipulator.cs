using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using PointerType=UnityEngine.UIElements.PointerType;
namespace Veilbreak.Gui {
/// <summary>Lets the player reorder a container's children. Dragging a child past <see cref="Slop"/>, or holding it for <see cref="LongPressSeconds"/>, lifts a ghost
/// (a selection-ringed plate over the panel root) and dims the child; the child under the ghost, found from the children's <c>worldBound</c>, is the drop slot; releasing
/// raises <see cref="Reordered"/> (from, to) and glides the ghost into the slot, or removes it at once under reduced motion. Alt+arrow on a focused child raises the same event
/// for its neighbour and follows it with focus (Left and Up step back, Right and Down forward). The manipulator never moves a child: state stays with the host, which binds again.
/// A tap, which never lifts, still reaches the child's own click. Register it on the container; <see cref="CanLift"/> says which children may move.</summary>
public sealed class ReorderManipulator:PointerManipulator {
 public const float Slop=8,LongPressSeconds=.35f;
 public const string GhostClass="vb-reorder-ghost",SourceClass="vb-reorder-source",OverClass="vb-reorder-over";
 /// <summary>A child moved from one index to another.</summary>
 public event Action<int,int> Reordered;
 /// <summary>Which children may be lifted (all by default).</summary>
 public Func<int,bool> CanLift;
 readonly List<Rect> bounds=new List<Rect>();
 int pointer=-1,from=-1,over=-1;Vector2 press,at;bool lifted;VisualElement ghost;IVisualElementScheduledItem hold;
 /// <summary>The child being dragged, or -1.</summary>
 public int Lifted=>lifted?from:-1;
 /// <summary>The child the ghost is over, or -1.</summary>
 public int Over=>lifted?over:-1;
 /// <summary>The ghost while something is lifted.</summary>
 public VisualElement Ghost=>ghost;

 protected override void RegisterCallbacksOnTarget(){target.RegisterCallback<PointerDownEvent>(Down,TrickleDown.TrickleDown);target.RegisterCallback<PointerMoveEvent>(Move,TrickleDown.TrickleDown);target.RegisterCallback<PointerUpEvent>(Up,TrickleDown.TrickleDown);target.RegisterCallback<PointerCancelEvent>(Cancelled,TrickleDown.TrickleDown);target.RegisterCallback<PointerCaptureOutEvent>(CaptureLost);target.RegisterCallback<KeyDownEvent>(Key);}
 protected override void UnregisterCallbacksFromTarget(){target.UnregisterCallback<PointerDownEvent>(Down,TrickleDown.TrickleDown);target.UnregisterCallback<PointerMoveEvent>(Move,TrickleDown.TrickleDown);target.UnregisterCallback<PointerUpEvent>(Up,TrickleDown.TrickleDown);target.UnregisterCallback<PointerCancelEvent>(Cancelled,TrickleDown.TrickleDown);target.UnregisterCallback<PointerCaptureOutEvent>(CaptureLost);target.UnregisterCallback<KeyDownEvent>(Key);Cancel();}
 void Down(PointerDownEvent e){if(e.button!=0)return;Begin(e.pointerId,e.position);}
 void Move(PointerMoveEvent e){if(e.pointerId==pointer)Drag(e.position);if(lifted&&e.pointerId==pointer)e.StopPropagation();}
 void Up(PointerUpEvent e){if(e.pointerId!=pointer)return;bool was=lifted;End(e.position);if(was)e.StopPropagation();}
 void Cancelled(PointerCancelEvent e){if(e.pointerId==pointer)Cancel();}
 void CaptureLost(PointerCaptureOutEvent e){if(e.pointerId==pointer&&!lifted)Forget();}
 void Key(KeyDownEvent e){if(!e.altKey)return;int step=e.keyCode==KeyCode.LeftArrow||e.keyCode==KeyCode.UpArrow?-1:e.keyCode==KeyCode.RightArrow||e.keyCode==KeyCode.DownArrow?1:0;if(step==0)return;
  int index=IndexOfFocus();if(index>=0&&Nudge(index,step))e.StopPropagation();}
 int IndexOfFocus(){var focused=target.panel?.focusController?.focusedElement as VisualElement;for(var p=focused;p!=null&&p!=target;p=p.hierarchy.parent)if(p.hierarchy.parent==target)return target.IndexOf(p);return -1;}

 /// <summary>Starts watching a press: it lifts after <see cref="Slop"/> of movement or <see cref="LongPressSeconds"/> held still. False when the press is not on a child that may move.</summary>
 public bool Begin(int id,Vector2 position){if(pointer>=0)return false;int i=IndexAt(position);if(i<0||CanLift!=null&&!CanLift(i))return false;pointer=id;from=i;press=at=position;lifted=false;
  hold?.Pause();hold=target.schedule.Execute(()=>{if(pointer==id&&!lifted)Lift();}).StartingIn((long)(LongPressSeconds*1000));return true;}
 /// <summary>Moves the pointer; lifts once it has travelled past <see cref="Slop"/>, then the ghost follows and the drop slot updates.</summary>
 public void Drag(Vector2 position){if(pointer<0)return;at=position;if(!lifted){if((position-press).sqrMagnitude<Slop*Slop)return;Lift();}
  Follow();}
 /// <summary>Releases the pointer: a lifted child is dropped on the slot under it, which raises <see cref="Reordered"/> when it is another one.</summary>
 public void End(Vector2 position){if(pointer<0)return;at=position;if(!lifted){Forget();return;}
  int to=IndexAt(position);if(to<0)to=over>=0?over:from;int source=from;Settle(to);if(to!=source)Reordered?.Invoke(source,to);}
 /// <summary>Drops whatever is lifted without raising anything.</summary>
 public void Cancel(){if(pointer>=0&&target!=null&&target.HasPointerCapture(pointer))target.ReleasePointer(pointer);Unlift();Forget();}
 /// <summary>Moves child <paramref name="index"/> one place (the keyboard path); false at either end, for a child that may not move, or when nothing listens.</summary>
 public bool Nudge(int index,int step){int to=index+step;if(target==null||index<0||to<0||to>=target.childCount||CanLift!=null&&!CanLift(index)||Reordered==null)return false;
  Reordered(index,to);var moved=target[Mathf.Min(to,target.childCount-1)];target.schedule.Execute(()=>{if(moved.panel!=null&&moved.focusable)moved.Focus();});return true;}

 /// <summary>The index of the rect that holds <paramref name="point"/>, else the nearest one by centre, else -1 for none.</summary>
 public static int IndexAt(IReadOnlyList<Rect> rects,Vector2 point){int best=-1;float bestDistance=float.MaxValue;
  for(int i=0;i<rects.Count;i++){var r=rects[i];if(r.width<=0||r.height<=0)continue;if(r.Contains(point))return i;float d=(r.center-point).sqrMagnitude;if(d<bestDistance){bestDistance=d;best=i;}}return best;}
 /// <summary>The child index under a panel position, from the children's <c>worldBound</c>.</summary>
 public int IndexAt(Vector2 point){Measure();return IndexAt(bounds,point);}
 void Measure(){bounds.Clear();foreach(var child in target.Children())bounds.Add(child.style.display==DisplayStyle.None?default:child.worldBound);}

 void Lift(){hold?.Pause();hold=null;if(from<0||from>=target.childCount)return;lifted=true;over=from;if(!target.HasPointerCapture(pointer))target.CapturePointer(pointer);
  var child=target[from];child.AddToClassList(SourceClass);var root=target.panel?.visualTree;
  if(root!=null){var b=child.worldBound;ghost=VbShape.Attach(new VisualElement{pickingMode=PickingMode.Ignore}.Classes(GhostClass));ghost.style.position=Position.Absolute;ghost.style.width=b.width;ghost.style.height=b.height;root.Add(ghost);Place(b.position);}
  Mark(from);}
 void Follow(){if(!lifted)return;var b=target[from].worldBound;if(ghost!=null)Place(b.position+(at-press));int i=IndexAt(at);if(i>=0)Mark(i);}
 void Place(Vector2 worldPosition){var origin=ghost.parent.worldBound.position;ghost.style.left=worldPosition.x-origin.x;ghost.style.top=worldPosition.y-origin.y;}
 void Mark(int i){if(over>=0&&over<target.childCount)target[over].RemoveFromClassList(OverClass);over=i;if(i!=from&&i<target.childCount)target[i].AddToClassList(OverClass);}
 // The ghost glides into the drop slot over the UI beat and goes; under reduced motion it goes at once.
 void Settle(int to){var plate=ghost;ghost=null;Unlift();int id=pointer;Forget();if(target.HasPointerCapture(id))target.ReleasePointer(id);if(plate==null)return;
  if(to<0||to>=target.childCount||VbMotion.Calm(target)){plate.RemoveFromHierarchy();return;}
  var end=target[to].worldBound;var start=new Vector2(plate.resolvedStyle.left,plate.resolvedStyle.top);var origin=plate.parent?.worldBound.position??Vector2.zero;var goal=end.position-origin;double began=VbMotion.Clock();
  IVisualElementScheduledItem timer=null;timer=plate.schedule.Execute(()=>{float p=Mathf.Clamp01((float)((VbMotion.Clock()-began)*1000/VbTokens.DurUi));var v=Vector2.Lerp(start,goal,VbMotion.Ease(p));plate.style.left=v.x;plate.style.top=v.y;plate.style.opacity=1-p;if(p>=1){timer.Pause();plate.RemoveFromHierarchy();}}).Every(16);}
 void Unlift(){if(target==null)return;if(from>=0&&from<target.childCount)target[from].RemoveFromClassList(SourceClass);if(over>=0&&over<target.childCount)target[over].RemoveFromClassList(OverClass);ghost?.RemoveFromHierarchy();ghost=null;lifted=false;over=-1;}
 void Forget(){hold?.Pause();hold=null;pointer=-1;from=-1;lifted=false;}
}
}
