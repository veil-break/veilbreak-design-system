using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine.UIElements;
namespace Veilbreak.Gui {
/// <summary>Owns modal and non-modal overlays. Coverage and focus remain held until the closing animation completes.</summary>
public sealed class OverlayHost {
 public const string LayerName="modal-layer",LeavingName="modal-layer-leaving";
 sealed class Layer:OverlayView {
  // The layer dims the page: it fades, and the sheet in it (a DialogSurface) brings its own rise.
  public override MotionRecipe Entrance=>new MotionRecipe(MotionRecipes.OverlayIn.milliseconds);
  public override MotionRecipe Exit=>new MotionRecipe(MotionRecipes.OverlayOut.milliseconds);
  public override void Dismiss(Action done=null){var drawer=this.Q<DrawerOverlay>();if(drawer!=null){Phase=OverlayPhase.Closing;drawer.Close(()=>{this.Show(false);Phase=OverlayPhase.Closed;done?.Invoke();});}else base.Dismiss(done);}
 }
 sealed class Entry {public OverlayView view;public VisualElement under,focus;public bool enabled;public Action closed;public bool leaving,finished,restoreFocus;}
 static readonly ConditionalWeakTable<VisualElement,OverlayHost> hosts=new ConditionalWeakTable<VisualElement,OverlayHost>();
 readonly VisualElement root;readonly List<Entry> layers=new List<Entry>();readonly List<OverlayView> popups=new List<OverlayView>();
 public event Action<int> CoverageChanged;
 public int Count {get{int count=0;foreach(var e in layers)if(!e.leaving)count++;return count;}}
 public int Coverage=>layers.Count;
 public OverlayHost(VisualElement root){this.root=root;hosts.Remove(root);hosts.Add(root,this);root.RegisterCallback<DetachFromPanelEvent>(_=>Clear(true));}
 public static OverlayHost For(VisualElement anchor){for(var e=anchor;e!=null;e=e.parent)if(hosts.TryGetValue(e,out var host))return host;var root=anchor.panel?.visualTree??anchor;return hosts.TryGetValue(root,out var found)?found:new OverlayHost(root);}
 public void AddPopup(OverlayView popup){popup.Owner=this;popups.Add(popup);(root.panel?.visualTree??root).Add(popup);popup.Present();}
 public void RemovePopup(OverlayView popup){if(!popups.Remove(popup))return;popup.SetEnabled(false);popup.Dismiss(()=>{popup.Owner=null;popup.RemoveFromHierarchy();});}
 public bool ClosePopups(){bool any=popups.Count>0;foreach(var p in popups.ToArray())RemovePopup(p);return any;}
 public VisualElement Push(VisualElement content,Action onClose=null){ClosePopups();Popover.CloseWithin(content);VisualElement under=content;
  for(int i=layers.Count-1;i>=0;i--)if(!layers[i].leaving){under=layers[i].view;break;}
  bool enabled=under.enabledSelf;foreach(var existing in layers)if(existing.under==under)enabled=existing.enabled;
  var entry=new Entry{view=new Layer{name=LayerName},under=under,enabled=enabled,focus=root.panel?.focusController?.focusedElement as VisualElement,closed=onClose};
  under.AddToClassList("vb-covered");under.SetEnabled(false);var layer=entry.view;layer.Owner=this;layer.AddToClassList("vb-modal");layer.pickingMode=PickingMode.Position;layer.focusable=true;root.Add(layer);layers.Add(entry);layer.Focus();CoverageChanged?.Invoke(Coverage);layer.Present();return layer;
 }
 public bool Pop(){for(int i=layers.Count-1;i>=0;i--){var entry=layers[i];if(entry.leaving)continue;entry.leaving=true;entry.view.name=LeavingName;
   // Keep the blocker pickable throughout exit: no click-through to the page or battlefield.
   entry.view.SetEnabled(false);entry.view.Dismiss(()=>Remove(entry,true));return true;}return false;}
 void Remove(Entry entry,bool focus){entry.finished=true;entry.restoreFocus|=focus;if(layers.Exists(e=>e.under==entry.view)||!layers.Remove(entry))return;entry.view.Owner=null;entry.view.RemoveFromHierarchy();
  // Nested closures can finish out of order. Restore only when no other entry covers this element.
  bool covered=layers.Exists(e=>e.under==entry.under);if(!covered){entry.under.RemoveFromClassList("vb-covered");entry.under.SetEnabled(entry.enabled);}
  CoverageChanged?.Invoke(Coverage);entry.closed?.Invoke();if(entry.restoreFocus&&entry.focus?.panel!=null&&entry.focus.enabledInHierarchy)entry.focus.Focus();foreach(var parent in layers.ToArray())if(parent.finished)Remove(parent,parent.restoreFocus);
 }
 public void Clear(bool immediate=false){ClosePopups();foreach(var entry in new List<Entry>(layers).ToArray()){if(immediate){entry.view.FinishMotion();Remove(entry,false);}else if(!entry.leaving){entry.leaving=true;entry.view.name=LeavingName;entry.view.SetEnabled(false);entry.view.Dismiss(()=>Remove(entry,false));}}}
}
}
