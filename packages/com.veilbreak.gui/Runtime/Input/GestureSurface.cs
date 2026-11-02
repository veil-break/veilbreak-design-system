using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using PointerType=UnityEngine.UIElements.PointerType;
namespace Veilbreak.Gui {
/// <summary>Camera gestures on a surface that also hosts a targeting drag: one finger (or the left button) on empty ground pans, two fingers pan, pinch and twist,
/// the wheel zooms, a right drag orbits, a middle drag pans, a double tap resets and a hovering mouse reports its position.
/// Register it after the targeting manipulator; while <c>busy</c> returns true (a drag owns input) every pointer is ignored, and <see cref="Cancel"/> drops a gesture in progress.
/// Positions and deltas are panel units, y down.</summary>
public sealed class GestureSurface:PointerManipulator {
 /// <summary>Movement in panel units before a single pointer becomes a pan; below it, a release is a tap.</summary>
 public const float Slop=8,TapSeconds=.3f,DoubleTapSeconds=.35f,DoubleTapReach=40;
 public event Action<Vector2,Vector2> Panned;    // from, to
 public event Action<float> Pinched;             // ratio of finger spread, above 1 when spreading
 public event Action<float> Twisted;             // degrees, clockwise on screen
 public event Action<Vector2> Orbited;           // right-drag delta
 public event Action<float> Scrolled;            // wheel delta, positive away from the user
 public event Action<Vector2?> Hovered;          // mouse position over the surface, null when it leaves
 public event Action<Vector2> DoubleTapped;
 sealed class Contact {public Vector2 start,last;public float down;public int button;public bool moved;}
 readonly Dictionary<int,Contact> contacts=new Dictionary<int,Contact>();readonly Func<bool> busy;float lastTap=-1;Vector2 lastTapAt;
 public GestureSurface(Func<bool> busy=null){this.busy=busy??(()=>false);}
 /// <summary>Number of pointers the surface is tracking.</summary>
 public int Count=>contacts.Count;

 protected override void RegisterCallbacksOnTarget(){target.RegisterCallback<PointerDownEvent>(Down);target.RegisterCallback<PointerMoveEvent>(Move);target.RegisterCallback<PointerUpEvent>(Up);target.RegisterCallback<PointerCancelEvent>(Cancelled);target.RegisterCallback<PointerCaptureOutEvent>(CaptureLost);target.RegisterCallback<PointerLeaveEvent>(Leave);target.RegisterCallback<WheelEvent>(Wheel);}
 protected override void UnregisterCallbacksFromTarget(){target.UnregisterCallback<PointerDownEvent>(Down);target.UnregisterCallback<PointerMoveEvent>(Move);target.UnregisterCallback<PointerUpEvent>(Up);target.UnregisterCallback<PointerCancelEvent>(Cancelled);target.UnregisterCallback<PointerCaptureOutEvent>(CaptureLost);target.UnregisterCallback<PointerLeaveEvent>(Leave);target.UnregisterCallback<WheelEvent>(Wheel);Cancel();}
 // A pointer the targeting drag already took (it captured it or stopped the event) is never a camera gesture.
 void Down(PointerDownEvent e){if(e.isPropagationStopped||target.panel?.GetCapturingElement(e.pointerId)!=null&&!target.HasPointerCapture(e.pointerId))return;if(Press(e.pointerId,e.position,e.button,Time.unscaledTime)&&!target.HasPointerCapture(e.pointerId))target.CapturePointer(e.pointerId);}
 void Move(PointerMoveEvent e){if(contacts.ContainsKey(e.pointerId)){Drag(e.pointerId,e.position);return;}if(e.pointerType==PointerType.mouse&&e.pressedButtons==0&&!busy())Hovered?.Invoke(e.position);}
 void Up(PointerUpEvent e){if(!contacts.ContainsKey(e.pointerId))return;Lift(e.pointerId,e.position,Time.unscaledTime);if(target.HasPointerCapture(e.pointerId))target.ReleasePointer(e.pointerId);}
 void Cancelled(PointerCancelEvent e){if(contacts.Remove(e.pointerId)&&target.HasPointerCapture(e.pointerId))target.ReleasePointer(e.pointerId);}
 void CaptureLost(PointerCaptureOutEvent e){contacts.Remove(e.pointerId);}
 void Leave(PointerLeaveEvent e){if(e.pointerType==PointerType.mouse)Hovered?.Invoke(null);}
 void Wheel(WheelEvent e){if(Scroll(-e.delta.y))e.StopPropagation();}

 /// <summary>Starts tracking a pointer; false when the surface is busy or the button has no gesture.</summary>
 public bool Press(int id,Vector2 position,int button,float time){if(busy()){Cancel();return false;}if(button>2||contacts.Count>=2&&!contacts.ContainsKey(id))return false;contacts[id]=new Contact{start=position,last=position,down=time,button=button};return true;}
 /// <summary>Moves a tracked pointer and raises the gesture it makes.</summary>
 public void Drag(int id,Vector2 position){if(!contacts.TryGetValue(id,out var c))return;if(busy()){Cancel();return;}
  if(!c.moved&&(position-c.start).sqrMagnitude<Slop*Slop)return;c.moved=true;
  if(contacts.Count==2){Contact other=null;foreach(var pair in contacts)if(pair.Key!=id)other=pair.Value;
   Vector2 before=c.last-other.last,after=position-other.last;var from=(c.last+other.last)/2;var to=(position+other.last)/2;c.last=position;
   if(from!=to)Panned?.Invoke(from,to);if(before.sqrMagnitude>1&&after.sqrMagnitude>1){Pinched?.Invoke(after.magnitude/before.magnitude);float turn=Vector2.SignedAngle(before,after);if(Mathf.Abs(turn)>0)Twisted?.Invoke(turn);}return;}
  var last=c.last;c.last=position;if(c.button==1)Orbited?.Invoke(position-last);else Panned?.Invoke(last,position);}
 /// <summary>Stops tracking a pointer; a short, still release is a tap and a second tap nearby makes a double tap.</summary>
 public void Lift(int id,Vector2 position,float time){if(!contacts.TryGetValue(id,out var c))return;contacts.Remove(id);
  if(c.moved||c.button!=0||contacts.Count>0||time-c.down>TapSeconds){lastTap=-1;return;}
  if(lastTap>=0&&time-lastTap<=DoubleTapSeconds&&(position-lastTapAt).sqrMagnitude<=DoubleTapReach*DoubleTapReach){lastTap=-1;DoubleTapped?.Invoke(position);return;}
  lastTap=time;lastTapAt=position;}
 /// <summary>Wheel input; true when it was used.</summary>
 public bool Scroll(float delta){if(busy()||delta==0)return false;Scrolled?.Invoke(delta);return true;}
 /// <summary>Drops every tracked pointer without raising a gesture.</summary>
 public void Cancel(){if(target!=null)foreach(var id in contacts.Keys)if(target.HasPointerCapture(id))target.ReleasePointer(id);contacts.Clear();lastTap=-1;}
}
}
