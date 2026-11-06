using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
namespace Veilbreak.Gui.Tests {
/// <summary>Attached-panel timing and ownership. These require the pinned Unity editor; source parsing is insufficient.</summary>
public sealed class MotionLifecycleTests {
 GameObject host;PanelSettings settings;System.Func<double> previousClock;double time;
 [SetUp]public void Before(){previousClock=VbMotion.Clock;VbMotion.Clock=()=>time;}
 [TearDown]public void After(){VbMotion.Clock=previousClock;if(host!=null)Object.Destroy(host);if(settings!=null)Object.Destroy(settings);}
 IEnumerator Panel(System.Action<VisualElement> build,bool themed=false){settings=ScriptableObject.CreateInstance<PanelSettings>();
#if UNITY_EDITOR
  if(themed){settings.themeStyleSheet=UnityEditor.AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>("Packages/com.veilbreak.gui/Runtime/Styles/VeilbreakGui.tss");Assert.IsNotNull(settings.themeStyleSheet);settings.scaleMode=PanelScaleMode.ConstantPixelSize;settings.scale=1;}
#endif
  host=new GameObject("Motion lifecycle");var doc=host.AddComponent<UIDocument>();doc.panelSettings=settings;yield return null;build(doc.rootVisualElement);yield return null;}
 [UnityTest] public IEnumerator ModalHoldsCoverageUntilExitAndRestoresNestedFocus(){VisualElement root=null,page=null;OverlayHost overlays=null;yield return Panel(r=>{root=r;page=new VisualElement().Into(r);overlays=new OverlayHost(r);});var one=overlays.Push(page);time+=1;yield return new WaitForSecondsRealtime(.04f);var two=overlays.Push(page);time+=1;yield return new WaitForSecondsRealtime(.04f);Assert.AreEqual(2,overlays.Coverage);overlays.Pop();Assert.AreEqual(2,overlays.Coverage);Assert.IsFalse(one.enabledSelf);time+=1;yield return new WaitForSecondsRealtime(.04f);Assert.AreEqual(1,overlays.Coverage);Assert.IsTrue(one.enabledSelf);Assert.IsFalse(page.enabledSelf);overlays.Pop();time+=1;yield return new WaitForSecondsRealtime(.04f);Assert.AreEqual(0,overlays.Coverage);Assert.IsTrue(page.enabledSelf);}
 [UnityTest] public IEnumerator FeedbackDoesNotCancelExitAndDetachFinalizesCount(){VisualElement item=null;Label count=null;yield return Panel(root=>{item=new VisualElement().Into(root);count=new Label().Into(root);});VbMotion.In(item,MotionKind.Deal);time+=.12;yield return new WaitForSecondsRealtime(.04f);VbMotion.Pop(item);VbMotion.Out(item);time+=1;yield return new WaitForSecondsRealtime(.04f);Assert.IsFalse(VbMotion.Shown(item));Assert.IsFalse(VbMotion.Busy(item));VbMotion.Count(count,1,null);VbMotion.Count(count,100,null);count.RemoveFromHierarchy();Assert.AreEqual("100",count.text);Assert.IsFalse(VbMotion.Busy(count));}
 [UnityTest] public IEnumerator ReversalAndReducedMotionSettleWithoutOldCallbacks(){VisualElement root=null,item=null;int closed=0;yield return Panel(r=>{root=r;item=new VisualElement().Into(r);});VbMotion.Out(item,MotionKind.Rise,()=>closed++);time+=.05;yield return new WaitForSecondsRealtime(.04f);VbMotion.In(item,MotionKind.Deal);root.AddToClassList(SafeAreaRoot.ReducedMotion);time+=1;yield return new WaitForSecondsRealtime(.04f);Assert.IsTrue(VbMotion.Shown(item));Assert.AreEqual(0,closed);Assert.IsFalse(VbMotion.Busy(item));}

 // ── ds#69: the recipes of the motion pass (arcade#54). Each runs with full motion, reduced motion, reduced flashes and both, reaches the same end state and leaves nothing behind. ──
 static readonly string[][] Modes={new string[0],new[]{SafeAreaRoot.ReducedMotion},new[]{SafeAreaRoot.ReducedFlashes},new[]{SafeAreaRoot.ReducedMotion,SafeAreaRoot.ReducedFlashes}};
 static bool Calm(string[] mode)=>System.Array.IndexOf(mode,SafeAreaRoot.ReducedMotion)>=0;
 static bool Flashless(string[] mode)=>System.Array.IndexOf(mode,SafeAreaRoot.ReducedFlashes)>=0;
 static string Name(string[] mode)=>mode.Length==0?"full motion":string.Join(" + ",mode);
 // A fresh panel whose root carries the mode's accessibility classes.
 IEnumerator Boot(string[] mode,System.Action<VisualElement> build,bool themed=false){if(host!=null)Object.Destroy(host);if(settings!=null)Object.Destroy(settings);yield return null;yield return Panel(root=>{foreach(var c in mode)root.AddToClassList(c);build(root);},themed);}
 // The recipes tick on the scheduler's real-time timers, so the injected clock moves and the test lets a few of them run.
 IEnumerator Advance(double seconds){time+=seconds;yield return new WaitForSecondsRealtime(.06f);yield return null;}
 static void Finger<T>(VisualElement target,int finger,Vector2 at,TouchPhase phase) where T:PointerEventBase<T>,new(){using(var e=PointerEventBase<T>.GetPooled(new Touch{fingerId=finger,position=at,phase=phase})){e.target=target;target.SendEvent(e);}}
 static void Pointer<T>(VisualElement target,Vector2 at,EventType type) where T:PointerEventBase<T>,new(){var system=new Event{type=type,mousePosition=at,button=0};using(var e=PointerEventBase<T>.GetPooled(system)){e.target=target;target.SendEvent(e);}}

 [UnityTest] public IEnumerator AFitDrawerEasesToItsNewContentHeightAndJumpsUnderReducedMotion(){
  foreach(var mode in Modes){
   DrawerOverlay drawer=null;yield return Boot(mode,root=>{root.AddToClassList("vb-root");drawer=new DrawerOverlay{Title="UNIT"}.Into(root);var s=drawer.style;s.position=Position.Absolute;s.left=s.right=s.top=s.bottom=0;Ui.Box(drawer,"first").style.height=60;},true);
   yield return null;drawer.Open();yield return Advance(1);yield return null;Assert.AreEqual(OverlayPhase.Open,drawer.Phase,Name(mode));Assert.IsFalse(drawer.Fitting,"Opening is the entrance's, not the fit's");
   float before=drawer.Sheet.resolvedStyle.height;Assert.Greater(before,60,Name(mode));
   Ui.Box(drawer,"second").style.height=60;yield return null;yield return null;
   if(Calm(mode)){Assert.IsFalse(drawer.Fitting,"Reduced motion jumps: "+Name(mode));}
   else{Assert.IsTrue(drawer.Fitting,"The sheet eases: "+Name(mode));yield return Advance(MotionRecipes.DrawerFit.milliseconds/2000.0);float mid=drawer.Sheet.resolvedStyle.height;Assert.Greater(mid,before+1,Name(mode));Assert.Less(mid,before+59,"Part-way mid-fit: "+Name(mode));}
   yield return Advance(1);yield return null;yield return null;
   Assert.IsFalse(drawer.Fitting,Name(mode));Assert.AreEqual(StyleKeyword.Null,drawer.Sheet.style.height.keyword,"The inline height is given back: "+Name(mode));Assert.AreEqual(before+60,drawer.Sheet.resolvedStyle.height,1.5f,"The same end state: "+Name(mode));
   // Closing mid-fit ends it, and a dragged or full-screen sheet is not eased.
   drawer.Snap=DrawerSnap.Full;Ui.Box(drawer,"third").style.height=20;yield return null;yield return null;Assert.IsFalse(drawer.Fitting,"Only a Fit drawer eases: "+Name(mode));drawer.Close();yield return Advance(1);Assert.AreEqual(OverlayPhase.Closed,drawer.Phase);}}

 [UnityTest] public IEnumerator MergedLabelsSwellAndShrinkOnSimulationTime(){
  foreach(var mode in Modes){
   WorldLayer layer=null;yield return Boot(mode,root=>{layer=new WorldLayer{Project=w=>new Vector2(w.x,w.y)}.Into(root);layer.style.width=300;layer.style.height=300;});
   void Frame(int members,float delta){layer.BeginFrame();for(int i=0;i<members;i++)layer.Label(i,new Vector3(40+i*10,100,0),WorldLabelKind.Name,"Tethered",null,WorldTeam.Enemy,"tether");layer.EndFrame(delta);}
   Frame(1,.016f);yield return null;var label=layer.Q<WorldLabel>();Assert.AreEqual("Tethered",label.Title);Assert.AreEqual(1,label.resolvedStyle.scale.value.x,1e-4f,"A lone label is at rest: "+Name(mode));
   // Two members join: it swells (merge). The pose follows the simulation delta, not the clock.
   Frame(3,.016f);Frame(3,.05f);yield return null;float left=MotionRecipes.LabelMerge.milliseconds/1000f-.066f;WorldLayer.LabelPose(true,left,Calm(mode),Flashless(mode),out float scale,out float opacity);
   Assert.AreEqual(scale,label.resolvedStyle.scale.value.x,.01f,"Merge scale: "+Name(mode));Assert.AreEqual(opacity,label.resolvedStyle.opacity,.01f,"Merge opacity: "+Name(mode));
   if(Calm(mode))Assert.AreEqual(1,label.resolvedStyle.scale.value.x,1e-4f,"Reduced motion does not scale");else Assert.Greater(label.resolvedStyle.scale.value.x,1.05f,"It swells");
   if(Flashless(mode))Assert.AreEqual(1,label.resolvedStyle.opacity,1e-4f,"Reduced flashes does not dip");else Assert.Less(label.resolvedStyle.opacity,.95f,"It dips for a beat");
   Frame(3,.3f);yield return null;Assert.AreEqual(1,label.resolvedStyle.scale.value.x,1e-4f,"At rest again: "+Name(mode));Assert.AreEqual(1,label.resolvedStyle.opacity,1e-4f);
   // The members leave but one: it shrinks (split) and the text reads as itself.
   Frame(1,.016f);Frame(1,.05f);yield return null;Assert.AreEqual("Tethered",label.Title);
   if(Calm(mode))Assert.AreEqual(1,label.resolvedStyle.scale.value.x,1e-4f);else Assert.Less(label.resolvedStyle.scale.value.x,.95f,"It shrinks: "+Name(mode));
   Frame(1,.3f);yield return null;Assert.AreEqual(1,label.resolvedStyle.scale.value.x,1e-4f,"At rest: "+Name(mode));Assert.AreEqual(1,label.resolvedStyle.opacity,1e-4f);}}

 [UnityTest] public IEnumerator ADraggedChildLiftsAndItsGhostSettlesIntoTheSlot(){
  foreach(var mode in Modes){
   VisualElement row=null;ReorderManipulator reorder=null;yield return Boot(mode,root=>{row=new VisualElement().Into(root);row.style.flexDirection=FlexDirection.Row;for(int i=0;i<3;i++){var c=new VisualElement{focusable=true}.Into(row);c.style.width=80;c.style.height=60;}reorder=new ReorderManipulator();row.AddManipulator(reorder);});
   yield return null;yield return null;var moves=new List<(int,int)>();reorder.Reordered+=(a,b)=>moves.Add((a,b));int Ghosts()=>row.panel.visualTree.Query(className:ReorderManipulator.GhostClass).ToList().Count;
   var start=row[0].worldBound.center;Assert.IsTrue(reorder.Begin(1,start));reorder.Drag(start+new Vector2(30,0));var ghost=reorder.Ghost;Assert.IsNotNull(ghost,Name(mode));Assert.AreEqual(1,Ghosts());
   if(!Flashless(mode))Assert.AreEqual(.55f,ghost.style.opacity.value,.01f,"The pick-up flash starts dim: "+Name(mode));
   yield return Advance(MotionRecipes.DragLift.milliseconds/1000.0+.05);
   if(Calm(mode)){Assert.AreEqual(1,ghost.resolvedStyle.scale.value.x,1e-4f,"Reduced motion keeps the ghost flat");Assert.AreEqual(0,ghost.resolvedStyle.translate.y,1e-4f);}
   else{Assert.AreEqual(MotionRecipes.DragLift.scale,ghost.resolvedStyle.scale.value.x,.01f,"Lifted: "+Name(mode));Assert.AreEqual(MotionRecipes.DragLift.y,ghost.resolvedStyle.translate.y,.1f);}
   if(!Flashless(mode))Assert.AreEqual(1,ghost.resolvedStyle.opacity,.01f,"The flash is over");
   reorder.End(row[2].worldBound.center);CollectionAssert.AreEqual(new[]{(0,2)},moves,Name(mode));Assert.IsNull(reorder.Ghost);Assert.AreEqual(-1,reorder.Lifted);
   if(Calm(mode))Assert.AreEqual(0,Ghosts(),"Reduced motion drops it at once");
   else{Assert.AreEqual(1,Ghosts(),"The ghost glides into the slot");yield return Advance(MotionRecipes.DropSettle.milliseconds/2000.0);Assert.AreEqual(1,Ghosts());var plate=row.panel.visualTree.Q(className:ReorderManipulator.GhostClass);Assert.Less(plate.resolvedStyle.scale.value.x,MotionRecipes.DragLift.scale,"Settling back to full size");Assert.Greater(plate.resolvedStyle.scale.value.x,1);}
   yield return Advance(1);Assert.AreEqual(0,Ghosts(),"Nothing left behind: "+Name(mode));Assert.IsFalse(row[2].ClassListContains(ReorderManipulator.OverClass));Assert.IsFalse(row[0].ClassListContains(ReorderManipulator.SourceClass));yield return Advance(.5);Assert.IsFalse(VbMotion.Busy(row[2]),"The slot's pop is over");}}

 [UnityTest] public IEnumerator ASkillSinksUnderAPressAndLetsGoWhereverThePressEnds(){
  foreach(var mode in Modes){
   SkillButton skill=null;VisualElement other=null;yield return Boot(mode,root=>{skill=new SkillButton().Into(root);other=new VisualElement().Into(root);other.style.width=50;other.style.height=50;});yield return null;
   Pointer<PointerDownEvent>(skill,Vector2.one,EventType.MouseDown);Assert.IsTrue(skill.Pressed,Name(mode));Assert.IsTrue(VbMotion.Held(skill));Assert.AreEqual(!Flashless(mode),skill.ClassListContains(SkillButton.PressFlashClass),"The plate brightens unless flashes are reduced: "+Name(mode));
   yield return Advance(MotionRecipes.PressSink.milliseconds/1000.0+.1);
   if(Calm(mode)){Assert.AreEqual(1,skill.resolvedStyle.scale.value.x,1e-4f,"Reduced motion does not sink");Assert.Less(skill.resolvedStyle.opacity,.9f,"It dims instead: "+Name(mode));}
   else{Assert.AreEqual(MotionRecipes.PressSink.scale,skill.resolvedStyle.scale.value.x,.01f,"Sunk: "+Name(mode));Assert.AreEqual(MotionRecipes.PressSink.y,skill.resolvedStyle.translate.y,.1f);Assert.AreEqual(1,skill.resolvedStyle.opacity,.01f);}
   // The press ends on another element (a game's drag manipulator may hold the pointer): the skill still lets go.
   Pointer<PointerUpEvent>(other,Vector2.one,EventType.MouseUp);Assert.IsFalse(skill.Pressed,Name(mode));Assert.IsFalse(skill.ClassListContains(SkillButton.PressFlashClass));
   yield return Advance(MotionRecipes.PressSink.milliseconds/1000.0+.1);
   Assert.IsFalse(VbMotion.Held(skill),Name(mode));Assert.IsFalse(VbMotion.Busy(skill));Assert.AreEqual(1,skill.resolvedStyle.scale.value.x,1e-4f,"Back at rest: "+Name(mode));Assert.AreEqual(0,skill.resolvedStyle.translate.y,1e-4f);Assert.AreEqual(1,skill.resolvedStyle.opacity,.01f);
   // Two fingers: another finger lifting or being cancelled does not let go of the finger that holds the skill, and a second finger on it does not take the press over.
   Finger<PointerDownEvent>(skill,0,Vector2.one,TouchPhase.Began);Assert.IsTrue(skill.Pressed,"A finger presses it: "+Name(mode));Finger<PointerDownEvent>(skill,1,Vector2.one,TouchPhase.Began);
   Finger<PointerUpEvent>(other,1,Vector2.one,TouchPhase.Ended);Assert.IsTrue(skill.Pressed,"Finger 1 lifting does not release finger 0: "+Name(mode));
   using(var cancel=PointerCancelEvent.GetPooled(new Touch{fingerId=1,position=Vector2.one,phase=TouchPhase.Canceled})){cancel.target=other;other.SendEvent(cancel);}Assert.IsTrue(skill.Pressed,"…nor does finger 1 being cancelled");
   Finger<PointerUpEvent>(other,0,Vector2.one,TouchPhase.Ended);Assert.IsFalse(skill.Pressed,"Finger 0 lifting releases it: "+Name(mode));
   // A second press and a cancel, and a fallen skill that does not sink at all.
   Pointer<PointerDownEvent>(skill,Vector2.one,EventType.MouseDown);Assert.IsTrue(skill.Pressed);using(var cancel=PointerCancelEvent.GetPooled()){cancel.target=other;other.SendEvent(cancel);}Assert.IsFalse(skill.Pressed,"A cancelled press lets go");
   skill.Bind(new SkillView{state=SkillState.Fallen});Pointer<PointerDownEvent>(skill,Vector2.one,EventType.MouseDown);Assert.IsFalse(skill.Pressed,"A locked skill does not sink: "+Name(mode));
   // Leaving the panel mid-press finishes it.
   skill.Bind(new SkillView());Pointer<PointerDownEvent>(skill,Vector2.one,EventType.MouseDown);Assert.IsTrue(skill.Pressed);skill.RemoveFromHierarchy();Assert.IsFalse(skill.Pressed);Assert.IsFalse(VbMotion.Held(skill));}}

 [UnityTest] public IEnumerator AFallenCardSlumpsDrainsAndRestsInTheFallenLook(){
  foreach(var mode in Modes){
   UnitCard card=null;yield return Boot(mode,root=>{card=new UnitCard().Into(root);card.style.width=100;card.style.height=160;});
   card.Bind(new OperativeView{name="Aria",hp=900,maxHp=1000});yield return null;Assert.IsFalse(VbMotion.Busy(card),"A card that is up does not move: "+Name(mode));Assert.IsFalse(VbEdgeFx.Active(card));
   card.Bind(new OperativeView{name="Aria",hp=0,maxHp=1000});Assert.AreEqual(EdgeFxKind.Damage,VbEdgeFx.Playing(card),"The fall is a hit: "+Name(mode));
   if(Calm(mode))Assert.IsFalse(VbMotion.Busy(card),"Reduced motion does not slump");
   else{Assert.IsTrue(VbMotion.Busy(card),Name(mode));yield return null;Assert.AreEqual(MotionRecipes.Fallen.y,card.resolvedStyle.translate.y,.1f,"It starts slumped");Assert.AreEqual(MotionRecipes.Fallen.scale,card.resolvedStyle.scale.value.x,.01f);}
   yield return Advance(MotionRecipes.Fallen.milliseconds/1000.0+.3);
   Assert.IsFalse(VbMotion.Busy(card),Name(mode));Assert.IsFalse(VbEdgeFx.Active(card),"The flash is over");Assert.AreEqual(0,card.resolvedStyle.translate.y,1e-4f,"Back at rest: "+Name(mode));Assert.AreEqual(1,card.resolvedStyle.scale.value.x,1e-4f);Assert.AreEqual(1,card.resolvedStyle.opacity,.01f);
   Assert.IsTrue(card.ClassListContains("vb-operative--fallen"),"…in the fallen look");
   // A revive and a second fall slump again; binding a fallen card twice does not.
   card.Bind(new OperativeView{name="Aria",hp=0,maxHp=1000});yield return null;Assert.IsFalse(VbMotion.Busy(card),"A fallen card bound again stays still: "+Name(mode));}}

 // A card that is on a panel when its first real bind is a fallen unit (a checkpoint resume) is not falling: it rests in the fallen look.
 [UnityTest] public IEnumerator ACardWhoseFirstBindIsFallenDoesNotSlump(){
  foreach(var mode in Modes){
   UnitCard card=null;yield return Boot(mode,root=>{card=new UnitCard().Into(root);card.style.width=100;card.style.height=160;});
   card.Bind(new OperativeView{name="Aria",hp=0,maxHp=1000});yield return null;Assert.IsFalse(VbMotion.Busy(card),"No slump: "+Name(mode));Assert.IsFalse(VbEdgeFx.Active(card),"…and no flash");Assert.AreEqual(0,card.resolvedStyle.translate.y,1e-4f);Assert.IsTrue(card.ClassListContains("vb-operative--fallen"));
   card.Bind(new OperativeView{name="Aria",hp=500,maxHp=1000});card.Bind(new OperativeView{name="Aria",hp=0,maxHp=1000});Assert.AreEqual(!Calm(mode),VbMotion.Busy(card),"A real fall after that does slump: "+Name(mode));}}

 // The drop pops the child that was dropped, where the host put it, not the one it displaced.
 [UnityTest] public IEnumerator TheDropPopsTheDroppedChildAfterTheHostReorders(){
  VisualElement row=null,a=null,b=null,c=null;ReorderManipulator reorder=null;yield return Boot(Modes[0],root=>{row=new VisualElement().Into(root);row.style.flexDirection=FlexDirection.Row;VisualElement Cell(string n){var e=new VisualElement{name=n,focusable=true}.Into(row);e.style.width=80;e.style.height=60;return e;}a=Cell("A");b=Cell("B");c=Cell("C");reorder=new ReorderManipulator();row.AddManipulator(reorder);});
  yield return null;yield return null;reorder.Reordered+=(from,to)=>{var moved=row[from];moved.RemoveFromHierarchy();row.Insert(to,moved);};
  var start=a.worldBound.center;Assert.IsTrue(reorder.Begin(1,start));reorder.Drag(start+new Vector2(30,0));yield return Advance(.2);reorder.End(c.worldBound.center);
  Assert.AreEqual("B,C,A",string.Join(",",row.Children().Select(e=>e.name)),"The host moved A to the end");yield return Advance(1);
  Assert.IsTrue(VbMotion.Busy(a),"The pop lands on A");Assert.IsFalse(VbMotion.Busy(b),"…not on B");Assert.IsFalse(VbMotion.Busy(c),"…nor C");}

 // A fit in progress yields to a drag at once (the drag owns the height), and a drawer that leaves its panel mid-fit keeps no in-between height.
 [UnityTest] public IEnumerator AFitYieldsToADragAndALeavingDrawerKeepsNoInBetweenHeight(){
  for(int scenario=0;scenario<2;scenario++){
   DrawerOverlay drawer=null;yield return Boot(Modes[0],root=>{root.AddToClassList("vb-root");drawer=new DrawerOverlay{Title="UNIT"}.Into(root);var s=drawer.style;s.position=Position.Absolute;s.left=s.right=s.top=s.bottom=0;Ui.Box(drawer,"first").style.height=60;},true);
   yield return null;drawer.Open();yield return Advance(1);yield return null;Ui.Box(drawer,"second").style.height=60;yield return null;yield return null;Assert.IsTrue(drawer.Fitting,"Easing");yield return Advance(.05);
   if(scenario==0){
    var grip=drawer.Q(className:"vb-drawer__grip");var at=grip.worldBound.center;Pointer<PointerDownEvent>(grip,at,EventType.MouseDown);Assert.IsFalse(drawer.Fitting,"A drag ends the ease");
    float held=drawer.Sheet.style.height.value.value;yield return Advance(1);Assert.AreEqual(held,drawer.Sheet.style.height.value.value,.01f,"…and the ease does not write over the drag's height");Pointer<PointerUpEvent>(grip,at,EventType.MouseUp);}
   else{
    var parent=drawer.parent;drawer.RemoveFromHierarchy();Assert.IsFalse(drawer.Fitting);Assert.AreEqual(StyleKeyword.Null,drawer.Sheet.style.height.keyword,"Leaving the panel mid-ease gives the height back");
    parent.Add(drawer);yield return null;yield return null;Assert.AreEqual(StyleKeyword.Null,drawer.Sheet.style.height.keyword,"…so a re-attach starts clean");}}}

 // A card a rebind disables during its deal-in delay is still disabled when it arrives, and one it enables is still enabled: the deal-in holds picking and focus, not the enabled state.
 [UnityTest] public IEnumerator TheDealInLeavesTheFinalEnabledStateToTheHost(){
  foreach(var mode in Modes){
   ChoiceCard disabled=null,enabled=null;yield return Boot(mode,root=>{disabled=new ChoiceCard().Into(root);enabled=new ChoiceCard().Into(root);disabled.style.width=enabled.style.width=100;disabled.style.height=enabled.style.height=80;enabled.SetEnabled(false);});yield return null;
   VbMotion.In(disabled,MotionKind.Deal,400);VbMotion.In(enabled,MotionKind.Deal,400);yield return Advance(.05);
   if(!Calm(mode)){Assert.IsTrue(disabled.ClassListContains("vb-motion-pending"),"Waiting its turn: "+Name(mode));Assert.AreEqual(PickingMode.Ignore,disabled.pickingMode);Assert.IsFalse(disabled.focusable);}
   disabled.SetEnabled(false);enabled.SetEnabled(true);yield return Advance(2);
   Assert.IsFalse(disabled.enabledSelf,"The rebind's disable wins: "+Name(mode));Assert.IsTrue(enabled.enabledSelf,"…and its enable");
   Assert.AreEqual(PickingMode.Position,disabled.pickingMode,"Picking is given back: "+Name(mode));Assert.IsTrue(disabled.focusable);Assert.IsFalse(disabled.ClassListContains("vb-motion-pending"));}}

}
}
