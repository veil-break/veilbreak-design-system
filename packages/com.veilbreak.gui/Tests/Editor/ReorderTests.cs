using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;
namespace Veilbreak.Gui.Tests {
/// <summary>ReorderManipulator's drop-slot maths and keyboard path; the pointer drag on a live panel is SquadSlotsReorderByDragging in FitTests.</summary>
public sealed class ReorderTests {
 static VisualElement Row(int n){var row=new VisualElement();for(int i=0;i<n;i++)row.Add(new VisualElement{focusable=true});return row;}

 [Test] public void TheDropSlotIsTheRectUnderThePointOrTheNearestOne(){
  var rects=new List<Rect>{new Rect(0,0,80,70),new Rect(100,0,80,70),new Rect(200,0,80,70),new Rect(300,0,80,70)};
  Assert.AreEqual(1,ReorderManipulator.IndexAt(rects,new Vector2(130,30)),"Inside a slot");Assert.AreEqual(2,ReorderManipulator.IndexAt(rects,new Vector2(192,30)),"In the gap it is the nearer centre");
  Assert.AreEqual(3,ReorderManipulator.IndexAt(rects,new Vector2(900,500)),"Far away it is still the nearest");Assert.AreEqual(0,ReorderManipulator.IndexAt(rects,new Vector2(-50,-50)));
  rects[0]=default;Assert.AreEqual(1,ReorderManipulator.IndexAt(rects,new Vector2(-50,-50)),"A hidden child (no size) is never a slot");
  Assert.AreEqual(-1,ReorderManipulator.IndexAt(new List<Rect>(),Vector2.zero),"No children, no slot");Assert.AreEqual(-1,ReorderManipulator.IndexAt(new List<Rect>{default,default},Vector2.zero));}

 [Test] public void TheKeyboardMovesAChildOnePlaceAndStopsAtTheEnds(){var row=Row(4);var moves=new List<(int,int)>();var reorder=new ReorderManipulator();row.AddManipulator(reorder);
  Assert.IsFalse(reorder.Nudge(1,1),"Nobody listens yet");reorder.Reordered+=(a,b)=>moves.Add((a,b));
  Assert.IsTrue(reorder.Nudge(1,1));Assert.IsTrue(reorder.Nudge(2,-1));Assert.IsFalse(reorder.Nudge(0,-1),"Nothing before the first");Assert.IsFalse(reorder.Nudge(3,1),"Nothing after the last");
  CollectionAssert.AreEqual(new[]{(1,2),(2,1)},moves);
  reorder.CanLift=i=>i!=2;Assert.IsFalse(reorder.Nudge(2,1),"A child that may not move stays");Assert.IsTrue(reorder.Nudge(1,1));}

 [Test] public void AnUnlaidOutRowLiftsNothing(){var row=Row(3);var reorder=new ReorderManipulator();row.AddManipulator(reorder);
  Assert.IsFalse(reorder.Begin(1,new Vector2(10,10)),"Zero-size children are not slots");Assert.AreEqual(-1,reorder.Lifted);Assert.IsNull(reorder.Ghost);reorder.Cancel();Assert.AreEqual(-1,reorder.Over);}
}
}
