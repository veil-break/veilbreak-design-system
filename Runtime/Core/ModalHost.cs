using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
namespace Veilbreak.Gui {
/// <summary>Owns blockers, focus restoration and blur for nested modal layers. A layer's scrim fades in and its content rises
/// (<see cref="VbMotion"/>); a popped layer is renamed <c>modal-layer-leaving</c>, stops taking input and fades out before it is removed,
/// while the page under it is restored at once.</summary>
public sealed class ModalHost {
 public const string LayerName="modal-layer",LeavingName="modal-layer-leaving";
 readonly VisualElement root;readonly List<VisualElement> layers=new List<VisualElement>();readonly Stack<Action> closed=new Stack<Action>();readonly Stack<VisualElement> previous=new Stack<VisualElement>();
 public ModalHost(VisualElement root){this.root=root;}
 public int Count=>layers.Count;
 public VisualElement Push(VisualElement content,Action onClose=null){Popover.CloseAll();var under=layers.Count>0?layers[layers.Count-1]:content;under.AddToClassList("vb-covered");under.SetEnabled(false);previous.Push(root.panel?.focusController?.focusedElement as VisualElement);
  var layer=new VisualElement{name=LayerName};layer.AddToClassList("vb-modal");layer.pickingMode=PickingMode.Position;layer.focusable=true;root.Add(layer);layers.Add(layer);layer.Focus();
  closed.Push(()=>{under.RemoveFromClassList("vb-covered");under.SetEnabled(true);onClose?.Invoke();});
  // The scrim fades in now; whatever the caller puts in the layer rises in on the next frame.
  VbMotion.In(layer,MotionKind.Fade);layer.schedule.Execute(()=>{if(layer.name==LayerName)VbMotion.StaggerChildren(layer,MotionKind.Rise,0,0);});
  return layer;}
 public bool Pop(){if(layers.Count==0)return false;var layer=layers[layers.Count-1];layers.RemoveAt(layers.Count-1);layer.name=LeavingName;layer.pickingMode=PickingMode.Ignore;layer.SetEnabled(false);
  VbMotion.Out(layer,MotionKind.Fade,layer.RemoveFromHierarchy);closed.Pop()();previous.Pop()?.Focus();return true;}
 public void Clear(){while(Pop()){};}
}
}
