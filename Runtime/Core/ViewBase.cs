using System;
using UnityEngine.UIElements;
namespace Veilbreak.Gui {
/// <summary>Shared view identity and lifecycle; native controls keep their native bases.</summary>
public abstract class ViewBase:VisualElement {
 public string Identity {get;set;}="";
 public void FinishMotion(){VbMotion.Finish(this);foreach(var e in this.Query<VisualElement>().ToList())if(e!=this)VbMotion.Finish(e);}
}
/// <summary>A painted surface without prescribed child slots.</summary>
public abstract class SurfaceView:ViewBase {
 protected SurfaceView(){VbShape.Attach(this);pickingMode=PickingMode.Ignore;}
}
/// <summary>Shared selection semantics for slot cards and specialized cards.</summary>
public abstract class CardView:SurfaceView {
 protected abstract string SelectionClass {get;}
 public bool Selected {get=>ClassListContains(SelectionClass);set=>EnableInClassList(SelectionClass,value);}
}
/// <summary>Read-only explanation surface, composed inside an overlay or inline.</summary>
public abstract class DetailView:SurfaceView {}
public enum OverlayPhase { Closed, Opening, Open, Closing }
/// <summary>All sheets, dialogs and popovers share lifecycle, ownership and reversal semantics.</summary>
public abstract class OverlayView:ViewBase {
 public OverlayPhase Phase {get;internal set;}
 public OverlayHost Owner {get;internal set;}
 public virtual MotionRecipe Entrance=>MotionRecipes.ViewIn;
 public virtual MotionRecipe Exit=>MotionRecipes.ViewOut;
 public virtual void Present(){Phase=OverlayPhase.Opening;VbMotion.In(this,Entrance,0,()=>Phase=OverlayPhase.Open);}
 public virtual void Dismiss(Action done=null){Phase=OverlayPhase.Closing;VbMotion.Out(this,Exit,()=>{Phase=OverlayPhase.Closed;done?.Invoke();});}
 protected OverlayView(){RegisterCallback<DetachFromPanelEvent>(_=>{FinishMotion();Phase=OverlayPhase.Closed;});}
}
}
