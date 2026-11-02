using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;
namespace Veilbreak.Gui.Tests {
/// <summary>VbGauge's value handling and dial maths; painting itself is checked in the editor.</summary>
public sealed class GaugeTests {
 [TestCase(-1f,0f)][TestCase(.5f,.5f)][TestCase(2f,1f)][TestCase(float.NaN,0f)]
 public void ValueClampsAndRejectsNaN(float input,float expected){Assert.AreEqual(expected,new VbGauge{Value=input}.Value);}
 [Test] public void ReadoutShowsAWholePercentage(){var gauge=new VbGauge{Value=.724f};Assert.AreEqual("72%",gauge.Readout.text);gauge.Value=1;Assert.AreEqual("100%",gauge.Readout.text);Assert.IsTrue(gauge.Readout.ClassListContains("vb-gauge__value"));}
 [Test] public void SegmentsStayInRange(){var gauge=new VbGauge{Segments=1};Assert.AreEqual(VbGauge.MinSegments,gauge.Segments);gauge.Segments=500;Assert.AreEqual(VbGauge.MaxSegments,gauge.Segments);gauge.Segments=20;Assert.AreEqual(20,gauge.Segments);}
 [Test] public void ACellIsLitOnceTheValueReachesIt(){Assert.AreEqual(0,VbGauge.Lit(0,36));Assert.AreEqual(1,VbGauge.Lit(.001f,36));Assert.AreEqual(18,VbGauge.Lit(.5f,36));Assert.AreEqual(36,VbGauge.Lit(5,36));}
 [Test] public void TintOverridesTheStylesheetColour(){var gauge=new VbGauge();Assert.AreEqual(VbTokens.Teal500,gauge.Colour);gauge.Tint(Color.red);Assert.AreEqual(Color.red,gauge.Colour);gauge.Tint(default);Assert.AreEqual(VbTokens.Teal500,gauge.Colour);}
 [Test] public void GaugeIsInertAndMarked(){var gauge=new VbGauge();Assert.AreEqual(PickingMode.Ignore,gauge.pickingMode);Assert.IsTrue(gauge.ClassListContains(VbGauge.ClassName));Assert.AreEqual(.5f,VbPaint.Alpha(Color.white,.5f).a);}
}
}
