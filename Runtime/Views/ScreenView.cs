using UnityEngine.UIElements;
namespace Veilbreak.Gui {
/// <summary>Base of the screen views: a column that fills its Navigator page (<c>vb-view vb-{block}</c>). Views take a plain view model
/// in <c>Bind</c> and raise events; the game binds data and handles the events, and never builds screens inline.
/// <c>Headed</c> views carry a <see cref="ScreenHeader"/> that spans the board while their content keeps a 12-unit gutter.</summary>
public abstract class ScreenView:VisualElement {
 public const string ClassName="vb-view";
 protected ScreenView(string block,bool headed=false){AddToClassList(ClassName);AddToClassList(block);EnableInClassList(ClassName+"--headed",headed);pickingMode=PickingMode.Ignore;}
}
}
