using System;
using UnityEngine;
using UnityEngine.UIElements;
namespace Veilbreak.Gui {
/// <summary>One-pointer press-drag-release with capture: begin decides whether the press starts a drag (return false to let it
/// through), move and end get panel positions, cancel runs when capture is lost. The target carries vb-dragging meanwhile.</summary>
public sealed class TargetDrag:PointerManipulator {
 readonly Func<Vector2,int,bool> begin;readonly Action<Vector2> move;readonly Action<Vector2> end;readonly Action cancel;int pointer=-1;
 public TargetDrag(Func<Vector2,int,bool> begin,Action<Vector2> move,Action<Vector2> end,Action cancel){this.begin=begin;this.move=move;this.end=end;this.cancel=cancel;}
 protected override void RegisterCallbacksOnTarget(){target.RegisterCallback<PointerDownEvent>(Down);target.RegisterCallback<PointerMoveEvent>(Move);target.RegisterCallback<PointerUpEvent>(Up);target.RegisterCallback<PointerCancelEvent>(Cancel);target.RegisterCallback<PointerCaptureOutEvent>(Lost);}
 protected override void UnregisterCallbacksFromTarget(){target.UnregisterCallback<PointerDownEvent>(Down);target.UnregisterCallback<PointerMoveEvent>(Move);target.UnregisterCallback<PointerUpEvent>(Up);target.UnregisterCallback<PointerCancelEvent>(Cancel);target.UnregisterCallback<PointerCaptureOutEvent>(Lost);}
 void Down(PointerDownEvent e){if(pointer>=0||e.button!=0)return;if(!begin(e.position,e.pointerId))return;pointer=e.pointerId;target.CapturePointer(pointer);target.AddToClassList("vb-dragging");e.StopPropagation();}
 void Move(PointerMoveEvent e){if(e.pointerId!=pointer)return;move(e.position);e.StopPropagation();}
 void Up(PointerUpEvent e){if(e.pointerId!=pointer)return;end(e.position);Release();e.StopPropagation();}
 void Cancel(PointerCancelEvent e){if(e.pointerId==pointer){cancel();Release();}}
 void Lost(PointerCaptureOutEvent e){if(e.pointerId==pointer){cancel();pointer=-1;target.RemoveFromClassList("vb-dragging");}}
 void Release(){int p=pointer;pointer=-1;target.RemoveFromClassList("vb-dragging");if(target.HasPointerCapture(p))target.ReleasePointer(p);}
}
}
