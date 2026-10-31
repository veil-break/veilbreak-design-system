using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
namespace Veilbreak.Gui {
/// <summary>Owns blockers, focus restoration and blur for nested modal layers.</summary>
public sealed class ModalHost {
 readonly VisualElement root;readonly List<VisualElement> layers=new List<VisualElement>();readonly Stack<Action> closed=new Stack<Action>();readonly Stack<VisualElement> previous=new Stack<VisualElement>();
 public ModalHost(VisualElement root){this.root=root;}
 public int Count=>layers.Count;
 public VisualElement Push(VisualElement content,Action onClose=null){Popover.CloseAll();var under=layers.Count>0?layers[layers.Count-1]:content;under.AddToClassList("vb-covered");under.SetEnabled(false);previous.Push(root.panel?.focusController?.focusedElement as VisualElement);var layer=new VisualElement{name="modal-layer"};layer.AddToClassList("vb-modal");layer.pickingMode=PickingMode.Position;layer.focusable=true;root.Add(layer);layers.Add(layer);layer.Focus();closed.Push(()=>{under.RemoveFromClassList("vb-covered");under.SetEnabled(true);onClose?.Invoke();});return layer;}
 public bool Pop(){if(layers.Count==0)return false;var layer=layers[layers.Count-1];layers.RemoveAt(layers.Count-1);layer.RemoveFromHierarchy();closed.Pop()();previous.Pop()?.Focus();return true;}
 public void Clear(){while(Pop()){};}
}
}
