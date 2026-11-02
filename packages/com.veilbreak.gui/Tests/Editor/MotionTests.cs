using NUnit.Framework;
using UnityEngine.UIElements;
namespace Veilbreak.Gui.Tests {
/// <summary>VbMotion's contract off a panel (instant, idempotent, target-tracking) and the kit's motion wiring.</summary>
public sealed class MotionTests {
 [Test] public void ShowIsInstantOffAPanelAndTracksItsTarget(){var e=new VisualElement();int done=0;
  VbMotion.Show(e,false,MotionKind.Rise,()=>done++);Assert.AreEqual(DisplayStyle.None,e.style.display.value);Assert.IsFalse(VbMotion.Shown(e));Assert.AreEqual(1,done);
  VbMotion.Show(e,false,MotionKind.Rise,()=>done++);Assert.AreEqual(2,done,"Asking for the state it is in completes at once");Assert.IsFalse(VbMotion.Busy(e));
  VbMotion.Show(e,true);Assert.AreEqual(DisplayStyle.Flex,e.style.display.value);Assert.IsTrue(VbMotion.Shown(e));}
 [Test] public void OutRunsItsCallbackOnceHidden(){var parent=new VisualElement();var e=new VisualElement().Into(parent);VbMotion.Out(e,MotionKind.Fade,e.RemoveFromHierarchy);Assert.IsNull(e.parent);Assert.IsFalse(VbMotion.Shown(e));}
 [Test] public void CountSetsTheFirstValueAtOnce(){var label=new Label();VbMotion.Count(label,12450,Ui.Number);Assert.AreEqual("12,450",label.text);VbMotion.Count(label,13000,Ui.Number);Assert.AreEqual("13,000",label.text,"Off a panel a count jumps");}
 [Test] public void PopAndShakeIgnoreHiddenOrDetachedElements(){var e=new VisualElement();Assert.DoesNotThrow(()=>{VbMotion.Pop(e);VbMotion.Shake(e);});Assert.IsFalse(VbMotion.Busy(e));}
 [Test] public void EaseSettlesWithoutOvershoot(){float last=0;for(int i=0;i<=20;i++){float v=VbMotion.Ease(i/20f);Assert.GreaterOrEqual(v,last);Assert.LessOrEqual(v,1);last=v;}Assert.AreEqual(1,VbMotion.Ease(1),1e-6f);}
 [Test] public void NavigatorClosesThroughMotionAndModalsRenameWhileLeaving(){var screen=new VisualElement{name="screen"};var nav=new Navigator(screen);nav.Open("menu");var layer=nav.Push();Assert.AreEqual(OverlayHost.LayerName,layer.name);
  nav.Modals.Pop();Assert.AreEqual(OverlayHost.LeavingName,layer.name);Assert.IsNull(screen.Q(OverlayHost.LayerName));nav.Close();Assert.IsFalse(nav.Visible);Assert.AreEqual(DisplayStyle.None,screen.style.display.value);}
 [Test] public void ReopeningTheSamePageReplacesItInPlace(){var screen=new VisualElement();var nav=new Navigator(screen);var first=nav.Open("squad");var second=nav.Open("squad");Assert.AreNotSame(first,second);Assert.IsNull(first.parent);Assert.AreEqual(1,screen.childCount);}
 [Test] public void BattleBoardNamesItsBandsAndOwnsTheCancelBand(){var board=new BattleHudView();Assert.AreEqual("hud",board.Top.name);Assert.AreEqual("battlefield",board.World.name);Assert.AreEqual("cards",board.Bottom.name);
  Assert.AreEqual(DisplayStyle.None,board.CancelBand.style.display.value);board.Aiming(true);Assert.IsTrue(VbMotion.Shown(board.CancelBand));board.Reveal(0);Assert.IsFalse(board.Top.visible);board.Reveal(1);Assert.IsTrue(board.Bottom.visible);}
 [Test] public void MotionSheetIsInTheTheme(){StringAssert.Contains("Motion.uss",System.IO.File.ReadAllText(PackageFiles.Path("Runtime/Styles/VeilbreakGui.tss")));
  var sheet=System.IO.File.ReadAllText(PackageFiles.Path("Runtime/Styles/Motion.uss"));StringAssert.DoesNotContain(".vb-skill__sweep {",sheet,"Windup sweeps are data and never transition");StringAssert.Contains(".vb-reduced-motion",sheet);}
 [Test] public void DisplayedReadsInlineDisplayThenTheResolvedStyle(){var e=new VisualElement();Assert.IsTrue(VbMotion.Displayed(e));e.style.display=DisplayStyle.None;Assert.IsFalse(VbMotion.Displayed(e));
  e.style.display=StyleKeyword.Null;Assert.IsTrue(VbMotion.Displayed(e),"unset inline display falls back to the resolved style");}
}
}
