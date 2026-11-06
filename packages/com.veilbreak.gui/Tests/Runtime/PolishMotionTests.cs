using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
namespace Veilbreak.Gui.Tests {
/// <summary>The motion lifecycle of each recipe of the GUI polish batch (design-system#55) on an attached panel with an injected clock: the activation and damage edge, the carousel's glide, the overlay in and out, and the glitch reveal. Each runs, reaches
/// the same end state and leaves nothing behind, with full motion and with reduced motion.</summary>
public sealed class PolishMotionTests {
 GameObject host;PanelSettings settings;System.Func<double> previousClock;double time;VisualElement root;
 [SetUp] public void Before(){previousClock=VbMotion.Clock;VbMotion.Clock=()=>time;time=100;}
 [TearDown] public void After(){VbMotion.Clock=previousClock;if(host!=null)Object.Destroy(host);if(settings!=null)Object.Destroy(settings);}
 IEnumerator Panel(params string[] classes){settings=ScriptableObject.CreateInstance<PanelSettings>();settings.referenceResolution=new Vector2Int(390,844);host=new GameObject("Polish motion");var doc=host.AddComponent<UIDocument>();doc.panelSettings=settings;yield return null;
  root=new VisualElement{name="board"};root.style.flexGrow=1;foreach(var c in classes)root.AddToClassList(c);doc.rootVisualElement.Add(root);yield return null;}
 // The recipes tick on the scheduler's real-time timers, so the clock moves and the test lets a few of them run.
 IEnumerator Advance(double seconds){time+=seconds;yield return new WaitForSecondsRealtime(.06f);yield return null;}
 static int Layers(VisualElement e)=>e.Query(className:VbEdgeFx.ClassName).ToList().Count;

 [UnityTest] public IEnumerator ActivationSweepsTheEdgeThenLeavesNothing(){
  yield return Panel();var button=new VbButton("PLAY",ButtonVariant.Primary).Into(root);button.style.width=200;yield return null;
  Assert.IsFalse(VbEdgeFx.Active(button));VbEdgeFx.Play(button);Assert.IsTrue(VbEdgeFx.Active(button));Assert.AreEqual(EdgeFxKind.Activate,VbEdgeFx.Playing(button));Assert.AreEqual(1,Layers(button),"One overlay on the button");
  Assert.AreEqual(PickingMode.Ignore,button.Q(className:VbEdgeFx.ClassName).pickingMode,"The overlay never takes input");
  yield return Advance(.2);Assert.IsTrue(VbEdgeFx.Active(button),"Still sweeping at 200 ms");yield return Advance(.2);Assert.IsTrue(VbEdgeFx.Active(button));
  yield return Advance(.3);Assert.IsFalse(VbEdgeFx.Active(button),"Done after "+VbEdgeFx.ActivateMilliseconds+" ms");Assert.AreEqual(0,Layers(button),"The overlay is removed");
  using(var submit=NavigationSubmitEvent.GetPooled()){submit.target=button;button.SendEvent(submit);}Assert.IsTrue(VbEdgeFx.Active(button),"A submit (Enter, a gamepad button) plays it by itself");VbEdgeFx.Cancel(button);Assert.IsFalse(VbEdgeFx.Active(button));Assert.AreEqual(0,Layers(button));
  // Anything made clickable plays it too, from the keyboard as well.
  int taps=0;var tile=new VisualElement().Into(root);tile.style.width=80;tile.style.height=60;tile.Clickable(()=>taps++);yield return null;
  using(var key=KeyDownEvent.GetPooled('\n',KeyCode.Return,EventModifiers.None)){key.target=tile;tile.SendEvent(key);}Assert.AreEqual(1,taps);Assert.IsTrue(VbEdgeFx.Active(tile),"A clickable element plays the edge when it is activated");VbEdgeFx.Cancel(tile);}
 static void Pointer<T>(VisualElement target,Vector2 at,EventType type) where T:PointerEventBase<T>,new(){var system=new Event{type=type,mousePosition=at,button=0};using(var e=PointerEventBase<T>.GetPooled(system)){e.target=target;target.SendEvent(e);}}
 // ds#69: the edge plays from a pointer press that goes down and up on the element (Clickable, Ui.cs), not only from VbEdgeFx.Play; a press that is dragged off before it is released is no click and plays nothing.
 [UnityTest] public IEnumerator ARealPointerPressThroughClickableClicksAndLightsTheEdge(){
  yield return Panel();int taps=0;var tile=new VisualElement().Into(root);tile.style.width=80;tile.style.height=60;tile.Clickable(()=>taps++);var button=new VbButton("GO",ButtonVariant.Secondary,null,null,()=>taps+=10).Into(root);button.style.width=120;button.style.height=48;var card=new ChoiceCard().Into(root);card.style.width=120;card.style.height=80;int picks=0;card.Picked+=()=>picks++;yield return null;yield return null;
  var at=tile.worldBound.center;Pointer<PointerDownEvent>(tile,at,EventType.MouseDown);Assert.AreEqual(0,taps,"A press alone is not a click");Assert.IsFalse(VbEdgeFx.Active(tile),"…and does not light the edge");
  Pointer<PointerUpEvent>(tile,at,EventType.MouseUp);Assert.AreEqual(1,taps,"Down and up on the element clicks once");Assert.IsTrue(VbEdgeFx.Active(tile),"…and the release lights the edge");Assert.AreEqual(EdgeFxKind.Activate,VbEdgeFx.Playing(tile));Assert.AreEqual(1,Layers(tile));
  yield return Advance(VbEdgeFx.ActivateMilliseconds/1000.0+.1);Assert.IsFalse(VbEdgeFx.Active(tile),"It runs out");Assert.AreEqual(0,Layers(tile));
  // Pressed on the element and released far from it: nothing.
  Pointer<PointerDownEvent>(tile,at,EventType.MouseDown);Pointer<PointerUpEvent>(tile,at+new Vector2(400,400),EventType.MouseUp);Assert.AreEqual(1,taps,"A press released elsewhere is no click");Assert.IsFalse(VbEdgeFx.Active(tile));
  // The kit's own controls take the same path: a VbButton and a ChoiceCard.
  var b=button.worldBound.center;Pointer<PointerDownEvent>(button,b,EventType.MouseDown);Pointer<PointerUpEvent>(button,b,EventType.MouseUp);Assert.AreEqual(11,taps,"The button clicked");Assert.IsTrue(VbEdgeFx.Active(button));VbEdgeFx.Cancel(button);
  var c=card.worldBound.center;Pointer<PointerDownEvent>(card,c,EventType.MouseDown);Pointer<PointerUpEvent>(card,c,EventType.MouseUp);Assert.AreEqual(1,picks,"The card was picked");Assert.IsTrue(VbEdgeFx.Active(card),"A choice card lights its edge when it is picked");VbEdgeFx.Cancel(card);}
 [UnityTest] public IEnumerator ARealPointerPressUnderReducedMotionStillLightsAndEndsTheEdge(){
  yield return Panel(SafeAreaRoot.ReducedMotion);int taps=0;var tile=new VisualElement().Into(root);tile.style.width=80;tile.style.height=60;tile.Clickable(()=>taps++);yield return null;yield return null;
  var at=tile.worldBound.center;Pointer<PointerDownEvent>(tile,at,EventType.MouseDown);Pointer<PointerUpEvent>(tile,at,EventType.MouseUp);Assert.AreEqual(1,taps);Assert.IsTrue(VbEdgeFx.Active(tile));yield return Advance(VbEdgeFx.CalmMilliseconds/1000.0+.1);Assert.IsFalse(VbEdgeFx.Active(tile),"The calm edge is over in "+VbEdgeFx.CalmMilliseconds+" ms");Assert.AreEqual(0,Layers(tile));}
 [UnityTest] public IEnumerator ReducedMotionLightsTheEdgeAtOnceAndFadesInAHundredMilliseconds(){
  yield return Panel(SafeAreaRoot.ReducedMotion);var button=new VbButton("PLAY").Into(root);yield return null;VbEdgeFx.Play(button);Assert.IsTrue(VbEdgeFx.Active(button));
  yield return Advance(.05);Assert.IsTrue(VbEdgeFx.Active(button));yield return Advance(.08);Assert.IsFalse(VbEdgeFx.Active(button),"The calm effect is over in "+VbEdgeFx.CalmMilliseconds+" ms");Assert.AreEqual(0,Layers(button));}
 [UnityTest] public IEnumerator DamageFlashesTheCardRedAndEndsClean(){
  yield return Panel();var card=new UnitCard{AutoDamage=false}.Into(root);card.Bind(new OperativeView{name="Aria",hp=900,maxHp=1000});yield return null;
  card.Damage(1);Assert.AreEqual(EdgeFxKind.Damage,VbEdgeFx.Playing(card));yield return Advance(.3);Assert.IsTrue(VbEdgeFx.Active(card));yield return Advance(.4);Assert.IsFalse(VbEdgeFx.Active(card));Assert.AreEqual(0,Layers(card));
  // A fall in the bound hit points plays it by itself; a rise (a heal) or the first bind does not.
  var auto=new UnitCard().Into(root);auto.Bind(new OperativeView{name="Rook",hp=1000,maxHp=1000});yield return null;Assert.IsFalse(VbEdgeFx.Active(auto),"The first bind is not a hit");
  auto.Bind(new OperativeView{name="Rook",hp=700,maxHp=1000});Assert.AreEqual(EdgeFxKind.Damage,VbEdgeFx.Playing(auto),"Losing hit points is a hit");yield return Advance(.7);auto.Bind(new OperativeView{name="Rook",hp=900,maxHp=1000});Assert.IsFalse(VbEdgeFx.Active(auto),"A heal is not");}
 [UnityTest] public IEnumerator DamageUnderReducedFlashesAndMotionStillEnds(){
  foreach(var flag in new[]{SafeAreaRoot.ReducedFlashes,SafeAreaRoot.ReducedMotion}){
   yield return Panel(flag);var card=new UnitCard{AutoDamage=false}.Into(root);card.Bind(new OperativeView{name="Aria",hp=900,maxHp=1000});yield return null;card.Damage(.5f);Assert.IsTrue(VbEdgeFx.Active(card));yield return Advance(.7);Assert.IsFalse(VbEdgeFx.Active(card),"Ends under "+flag);Assert.AreEqual(0,Layers(card));
   if(host!=null)Object.Destroy(host);if(settings!=null)Object.Destroy(settings);yield return null;}}
 [UnityTest] public IEnumerator ATargetThatLeavesThePanelFinishesItsEffect(){
  yield return Panel();var button=new VbButton("PLAY").Into(root);yield return null;VbEdgeFx.Play(button);Assert.IsTrue(VbEdgeFx.Active(button));button.RemoveFromHierarchy();Assert.IsFalse(VbEdgeFx.Active(button),"Detaching finalizes it");Assert.AreEqual(0,Layers(button));}

 [UnityTest] public IEnumerator TheCarouselGlidesBetweenCardsAndJumpsUnderReducedMotion(){
  foreach(bool calm in new[]{false,true}){
   yield return Panel(calm?new[]{SafeAreaRoot.ReducedMotion}:new string[0]);var carousel=new Carousel().Into(root);carousel.style.width=360;var ids=new[]{"a","b","c","d","e"};
   carousel.Bind(ids,v=>v,()=>new Label{style={height=100}},(c,v,i)=>((Label)c).text=v,0);yield return Advance(0);int changes=0;carousel.Changed+=_=>changes++;
   carousel.Next();Assert.AreEqual(1,carousel.Index);Assert.AreEqual(1,changes);
   if(calm){Assert.IsFalse(carousel.Animating,"Instant under reduced motion");Assert.AreEqual(1,carousel.Slot,1e-4f);}
   else{Assert.IsTrue(carousel.Animating);yield return Advance(Carousel.Milliseconds/2000.0);Assert.Greater(carousel.Slot,0);Assert.Less(carousel.Slot,1,"Part-way mid-glide");yield return Advance(Carousel.Milliseconds/1000.0);Assert.IsFalse(carousel.Animating);Assert.AreEqual(1,carousel.Slot,1e-4f);}
   carousel.Previous();carousel.Previous();Assert.AreEqual(4,carousel.Index,"Wraps back past the first");yield return Advance(1);Assert.AreEqual(4,carousel.Slot,1e-3f);Assert.AreEqual(2,carousel.Depth,"Five cards show two a side");
   for(int i=0;i<5;i++){float d=Carousel.Offset(i,carousel.Slot,5);Assert.LessOrEqual(Mathf.Abs(d),2.5f);}
   if(host!=null)Object.Destroy(host);if(settings!=null)Object.Destroy(settings);yield return null;}}

 [UnityTest] public IEnumerator OverlayRecipesRunInAndOutAndRestoreTheElement(){
  yield return Panel();foreach(var recipe in new[]{MotionRecipes.OverlayIn,MotionRecipes.PopoverIn,MotionRecipes.DrawerIn}){
   var e=new VisualElement{style={height=40}}.Into(root);yield return null;bool shown=false;VbMotion.In(e,recipe,0,()=>shown=true);Assert.IsTrue(e.ClassListContains("vb-motion-active"));
   yield return Advance(recipe.milliseconds/1000.0+.05);Assert.IsTrue(shown,"In finished");Assert.IsFalse(e.ClassListContains("vb-motion-active"));Assert.AreEqual(1,e.resolvedStyle.opacity,.01f);Assert.AreEqual(0,e.resolvedStyle.translate.y,.01f,"Back at rest");
   bool gone=false;VbMotion.Out(e,recipe.milliseconds==MotionRecipes.PopoverIn.milliseconds?MotionRecipes.PopoverOut:MotionRecipes.OverlayOut,()=>gone=true);yield return Advance(.4);Assert.IsTrue(gone,"Out finished");Assert.IsFalse(VbMotion.Shown(e));e.RemoveFromHierarchy();}}
 [UnityTest] public IEnumerator ADrawerOpensOnItsShortRecipeAndClosesByIt(){
  yield return Panel();var drawer=new DrawerOverlay{Title="UNIT"}.Into(root);drawer.style.position=Position.Absolute;drawer.style.left=drawer.style.right=drawer.style.top=drawer.style.bottom=0;Ui.Text(drawer,"Body");yield return null;
  drawer.Open();Assert.AreEqual(OverlayPhase.Opening,drawer.Phase);yield return Advance(MotionRecipes.DrawerIn.milliseconds/1000.0+.05);Assert.AreEqual(OverlayPhase.Open,drawer.Phase);
  bool closed=false;drawer.Close(()=>closed=true);Assert.AreEqual(OverlayPhase.Closing,drawer.Phase);yield return Advance(MotionRecipes.DrawerOut.milliseconds/1000.0+.05);Assert.IsTrue(closed);Assert.AreEqual(OverlayPhase.Closed,drawer.Phase);Assert.IsFalse(drawer.IsOpen);}

 // The reveal runs on real time (a Glitch track), so it is asked for a short run here.
 [UnityTest] public IEnumerator TheGlitchRevealDecodesTheTextAndEndsAtRest(){
  yield return Panel();var label=Ui.Text(root,"RUN OVER","vb-t-heading-lg");var glitch=new GlitchText("GAME OVER").Into(root);yield return null;
  Glitch.Reveal(label,300);Glitch.Reveal(glitch,300);Assert.AreNotEqual("RUN OVER",label.text,"It starts as noise");Assert.AreNotEqual("GAME OVER",glitch.text);Assert.AreEqual(8,label.text.Length);
  yield return new WaitForSecondsRealtime(.15f);Assert.AreNotEqual("RUN OVER",label.text,"Still decoding half way");
  yield return new WaitForSecondsRealtime(.35f);yield return null;Assert.AreEqual("RUN OVER",label.text);Assert.AreEqual("GAME OVER",glitch.text);Assert.AreEqual(1,label.resolvedStyle.opacity,.01f);Assert.AreEqual(0,label.resolvedStyle.translate.x,.01f);Assert.AreEqual(1,glitch.resolvedStyle.opacity,.01f);
  // An overlay's marked titles reveal together when it is presented, and a re-bound text during the reveal wins.
  var title=Ui.Text(root,"PAUSED","vb-t-heading",Glitch.TitleClass);yield return null;Glitch.RevealTitles(root,300);Assert.AreNotEqual("PAUSED",title.text);title.text="RESUMED";yield return new WaitForSecondsRealtime(.5f);yield return null;Assert.AreEqual("RESUMED",title.text,"Text set during the reveal is the one it ends on");}
 [UnityTest] public IEnumerator TheRevealUnderReducedMotionShowsTheWholeTextAndFadesIn(){
  yield return Panel(SafeAreaRoot.ReducedMotion);var label=Ui.Text(root,"RUN OVER","vb-t-heading-lg");yield return null;Glitch.Reveal(label,2000);yield return null;Assert.AreEqual("RUN OVER",label.text,"No noise under reduced motion");Assert.AreEqual(0,label.resolvedStyle.translate.x,.01f);
  yield return new WaitForSecondsRealtime(.4f);Assert.AreEqual(1,label.resolvedStyle.opacity,.01f,"Faded in within 300 ms");}
 [UnityTest] public IEnumerator TheAnnouncerRevealsEachNoticeAndHidesAfterwards(){
  yield return Panel();var lane=new GlitchText("").Into(root);var announcer=new Announcer(lane){RevealMilliseconds=200};yield return null;announcer.Say("WAVE 4",false,150);yield return null;Assert.IsTrue(VbMotion.Displayed(lane));Assert.AreNotEqual("WAVE 4",lane.text,"The notice decodes in");
  yield return new WaitForSecondsRealtime(.3f);Assert.AreEqual("WAVE 4",lane.text);yield return new WaitForSecondsRealtime(1f);Assert.IsFalse(VbMotion.Displayed(lane),"…and glitches out after its hold");}
}
}
