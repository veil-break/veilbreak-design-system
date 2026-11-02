using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
namespace Veilbreak.Gui.Tests {
public sealed class GestureTests {
 [Test] public void OneFingerPansOnlyPastTheSlop(){var g=new GestureSurface();var pans=new List<(Vector2 from,Vector2 to)>();g.Panned+=(a,b)=>pans.Add((a,b));
  Assert.IsTrue(g.Press(1,new Vector2(100,100),0,0));g.Drag(1,new Vector2(104,103));Assert.AreEqual(0,pans.Count,"A wobble inside the slop is still a tap");
  g.Drag(1,new Vector2(130,100));g.Drag(1,new Vector2(150,110));Assert.AreEqual(2,pans.Count);Assert.AreEqual(new Vector2(130,100),pans[1].from);Assert.AreEqual(new Vector2(150,110),pans[1].to);
  g.Lift(1,new Vector2(150,110),.5f);Assert.AreEqual(0,g.Count);}

 [Test] public void TwoFingersPinchTwistAndPan(){var g=new GestureSurface();float spread=1,twist=0;Vector2 travel=Vector2.zero;g.Pinched+=f=>spread*=f;g.Twisted+=d=>twist+=d;g.Panned+=(a,b)=>travel+=b-a;
  g.Press(1,new Vector2(100,200),0,0);g.Press(2,new Vector2(200,200),0,0);
  // Finger 2 swings a quarter turn clockwise on screen (y down) around finger 1 and moves twice as far out.
  for(int i=1;i<=9;i++){float a=i*10*Mathf.Deg2Rad;g.Drag(2,new Vector2(100,200)+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*(100+i*100/9f));}
  Assert.AreEqual(2,spread,.01f);Assert.AreEqual(90,twist,.5f);Assert.Greater(travel.magnitude,10,"The midpoint moved, so the view pans with it");}

 [Test] public void RightDragOrbitsAndWheelScrolls(){var g=new GestureSurface();Vector2 orbit=Vector2.zero;float wheel=0;g.Orbited+=d=>orbit+=d;g.Scrolled+=d=>wheel+=d;
  g.Press(1,new Vector2(0,0),1,0);g.Drag(1,new Vector2(30,-12));Assert.AreEqual(new Vector2(30,-12),orbit);Assert.IsTrue(g.Scroll(3));Assert.AreEqual(3,wheel);Assert.IsFalse(g.Scroll(0));}

 [Test] public void DoubleTapNeedsTwoQuickNearbyTaps(){var g=new GestureSurface();int taps=0;g.DoubleTapped+=_=>taps++;
  void Tap(Vector2 p,float t){g.Press(1,p,0,t);g.Lift(1,p,t+.05f);}
  Tap(new Vector2(50,50),0);Tap(new Vector2(60,55),.2f);Assert.AreEqual(1,taps);
  Tap(new Vector2(50,50),1);Tap(new Vector2(200,50),1.2f);Assert.AreEqual(1,taps,"Too far apart");
  Tap(new Vector2(50,50),2);Tap(new Vector2(50,50),3);Assert.AreEqual(1,taps,"Too slow");
  g.Press(1,new Vector2(50,50),0,4);g.Lift(1,new Vector2(50,50),4.6f);Tap(new Vector2(50,50),4.7f);Assert.AreEqual(1,taps,"A long press is not a tap");}

 [Test] public void BusySurfaceYieldsAndDropsTheGestureInProgress(){bool busy=false;var g=new GestureSurface(()=>busy);int pans=0;g.Panned+=(a,b)=>pans++;
  g.Press(1,Vector2.zero,0,0);g.Drag(1,new Vector2(40,0));Assert.AreEqual(1,pans);
  busy=true;g.Drag(1,new Vector2(80,0));Assert.AreEqual(1,pans,"A targeting drag owns input");Assert.AreEqual(0,g.Count,"The camera gesture is cancelled, not paused");
  Assert.IsFalse(g.Press(2,Vector2.zero,0,1),"A second finger during a skill aim is ignored");Assert.IsFalse(g.Scroll(1));
  busy=false;Assert.IsTrue(g.Press(3,Vector2.zero,0,2));Assert.IsTrue(g.Press(4,Vector2.zero,0,2));Assert.IsFalse(g.Press(5,Vector2.zero,0,2),"At most two pointers");}
}
}
