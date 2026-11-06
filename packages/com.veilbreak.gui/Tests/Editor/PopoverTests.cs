using NUnit.Framework;
using UnityEngine;
namespace Veilbreak.Gui.Tests {
/// <summary>Popover placement is a pure function of the panel, the anchor and the popover's size; the live clamping is PopoverClampsToPanel in FitTests.</summary>
public sealed class PopoverTests {
 static readonly Rect Panel=new Rect(0,0,390,844);

 [Test] public void APopoverSitsBelowItsAnchorWithAnArrowOnIt(){var anchor=new Rect(40,100,100,44);var at=Popover.Fit(Panel,anchor,new Vector2(240,120),PopoverPlacement.Below,false,out bool below,out float arrow);
  Assert.AreEqual(144+Popover.Gap,at.y,1e-3f);Assert.AreEqual(40,at.x,1e-3f);Assert.IsTrue(below,"The arrow is on the top edge");Assert.AreEqual(anchor.center.x-at.x,arrow,1e-3f,"The arrow points at the anchor's centre");}

 [Test] public void APopoverFlipsToTheOtherSideAndStaysInsideThePanel(){var anchor=new Rect(300,760,80,44);var at=Popover.Fit(Panel,anchor,new Vector2(260,200),PopoverPlacement.Below,false,out bool below,out float arrow);
  Assert.IsFalse(below,"No room below: it opens above, the arrow on its bottom edge");Assert.AreEqual(760-Popover.Gap-200,at.y,1e-3f);Assert.AreEqual(390-260-Popover.Edge,at.x,1e-3f,"Clamped to the right edge");
  Assert.GreaterOrEqual(arrow,Popover.ArrowInset);Assert.LessOrEqual(arrow,260-Popover.ArrowInset,"The arrow keeps clear of the corners");
  var end=Popover.Fit(Panel,new Rect(300,200,80,44),new Vector2(240,100),PopoverPlacement.Above,true,out _,out _);Assert.AreEqual(380-240,end.x,1e-3f,"Aligned to the anchor's end");}

 [Test] public void ATallPopoverIsLimitedToThePanelAndNeverLeavesIt(){var limit=Popover.Limit(Panel);Assert.AreEqual(Popover.MaxWidth,limit.x,"No wider than the maximum");Assert.AreEqual(844-2*Popover.Edge,limit.y,1e-3f,"No taller than the panel less its edges");
  Assert.AreEqual(320-2*Popover.Edge,Popover.Limit(new Rect(0,0,320,640)).x,1e-3f,"On a narrow panel the edges bound the width");
  var small=new Rect(0,0,260,500);var at=Popover.Fit(small,new Rect(100,250,60,40),new Vector2(small.width-2*Popover.Edge,small.height-2*Popover.Edge),PopoverPlacement.Below,false,out _,out _);
  Assert.AreEqual(Popover.Edge,at.x,1e-3f);Assert.AreEqual(Popover.Edge,at.y,1e-3f,"A popover as big as the limit sits in the panel's margins");}
}
}
