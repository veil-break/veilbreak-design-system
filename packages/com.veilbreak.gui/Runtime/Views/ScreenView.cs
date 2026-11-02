using UnityEngine.UIElements;
namespace Veilbreak.Gui {
/// <summary>Base of the screen views: a column that fills its Navigator page (<c>vb-view vb-{block}</c>). Views take a plain view model
/// in <c>Bind</c> and raise events; the game binds data and handles the events, and never builds screens inline.
/// <c>Headed</c> views carry a <see cref="ScreenHeader"/> that spans the board while their content keeps a 12-unit gutter.</summary>
public abstract class ScreenView:ViewBase {
 public const string ClassName="vb-view";
 protected ScreenView(string block,bool headed=false){AddToClassList(ClassName);AddToClassList(block);EnableInClassList(ClassName+"--headed",headed);pickingMode=PickingMode.Ignore;}
 /// <summary>What a deal brings in one after another (perk cards, road signs, offers); empty by default.</summary>
 protected virtual System.Collections.Generic.IEnumerable<VisualElement> DealItems(){yield break;}
 /// <summary>The page deal (guideline 4): heads-up titles glitch in and the deal items rise in <see cref="VbMotion.StaggerStep"/> apart, after
 /// the page's own entrance. Call it once the view is on its page.</summary>
 public ScreenView Deal(){foreach(var text in this.Query<GlitchText>().ToList())Glitch.In(text);VbMotion.Stagger(new System.Collections.Generic.List<VisualElement>(DealItems()),MotionKind.Deal,0);return this;}
}
}
