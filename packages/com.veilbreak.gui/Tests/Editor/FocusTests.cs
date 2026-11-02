using NUnit.Framework;
using UnityEngine.UIElements;
namespace Veilbreak.Gui.Tests {
/// <summary>VbFocus keeps keyboard focus across redraws; off a panel there is no focus to keep, so a redraw runs and nothing else happens.</summary>
public sealed class FocusTests {
 [Test] public void ReadMakesTextAFocusStopAndNamesIt(){var label=VbFocus.Read(new Label("row"),"first");
  Assert.IsTrue(label.focusable);Assert.IsTrue(label.ClassListContains(VbFocus.ReadClass));Assert.AreEqual("first",label.name);}
 [Test] public void ReadLeavesTheNameAloneWhenNoneIsGiven(){var label=new Label{name="kept"};VbFocus.Read(label);Assert.AreEqual("kept",label.name);}
 [Test] public void KeepRunsTheRedrawWithoutAPanel(){var root=new VisualElement();var ran=0;VbFocus.Keep(root,()=>ran++);Assert.AreEqual(1,ran);}
 [Test] public void SoonIgnoresAMissingElement(){var root=new VisualElement();Assert.DoesNotThrow(()=>VbFocus.Soon(root,"gone",()=>"also-gone"));Assert.DoesNotThrow(()=>VbFocus.Soon(root,null));}
}
}
