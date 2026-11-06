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
/// <summary>All sheets, dialogs and popovers share lifecycle, ownership and reversal semantics, and one short transition: <see cref="Entrance"/> and <see cref="Exit"/> default to
/// <see cref="MotionRecipes.OverlayIn"/> and <see cref="MotionRecipes.OverlayOut"/> (a fade with a small rise and scale, 220 and 150 ms); a popover uses <see cref="MotionRecipes.PopoverIn"/> and
/// <see cref="MotionRecipes.PopoverOut"/>, a drawer its slide. Reduced motion keeps the fade and skips the movement (<see cref="VbMotion"/>). An overlay that nobody presents (a pause sheet put straight on a
/// page) presents itself when it reaches a panel if it sets <see cref="AutoPresent"/>; its titles marked <c>vb-reveal-title</c> reveal with the glitch (<see cref="Glitch.Reveal"/>).</summary>
public abstract class OverlayView:ViewBase {
 public OverlayPhase Phase {get;internal set;}
 public OverlayHost Owner {get;internal set;}
 public virtual MotionRecipe Entrance=>MotionRecipes.OverlayIn;
 public virtual MotionRecipe Exit=>MotionRecipes.OverlayOut;
 /// <summary>Whether the overlay presents itself when it is attached to a panel with no <see cref="Owner"/> to do it.</summary>
 protected virtual bool AutoPresent=>false;
 public virtual void Present(){Phase=OverlayPhase.Opening;VbMotion.In(this,Entrance,0,()=>Phase=OverlayPhase.Open);Glitch.RevealTitles(this);}
 public virtual void Dismiss(Action done=null){Phase=OverlayPhase.Closing;VbMotion.Out(this,Exit,()=>{Phase=OverlayPhase.Closed;done?.Invoke();});}
 protected OverlayView(){RegisterCallback<DetachFromPanelEvent>(_=>{FinishMotion();Phase=OverlayPhase.Closed;});RegisterCallback<AttachToPanelEvent>(_=>{if(AutoPresent&&Owner==null&&Phase==OverlayPhase.Closed)Present();});}
}
}
