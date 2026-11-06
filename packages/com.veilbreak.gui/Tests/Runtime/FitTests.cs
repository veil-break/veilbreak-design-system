using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
namespace Veilbreak.Gui.Tests {
/// <summary>The kit on live portrait boards from 320 to 430 units wide, themed by VeilbreakGui.tss: nothing leaves the board and every
/// control a thumb hits is at least 44 units on both axes.</summary>
public sealed class FitTests {
 GameObject host;PanelSettings settings;RenderTexture target;
 [TearDown] public void Close(){if(host!=null)Object.Destroy(host);if(settings!=null)Object.Destroy(settings);if(target!=null)target.Release();}

 [UnityTest] public IEnumerator KitFitsPortraitBoardsWithThumbSizedTargets(){
#if UNITY_EDITOR
  var theme=UnityEditor.AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>("Packages/com.veilbreak.gui/Runtime/Styles/VeilbreakGui.tss");Assert.IsNotNull(theme);
  foreach(int width in new[]{320,360,390,430}){Close();int height=Mathf.RoundToInt(width*2.164f);target=new RenderTexture(width,height,0);
   settings=ScriptableObject.CreateInstance<PanelSettings>();settings.themeStyleSheet=theme;settings.scaleMode=PanelScaleMode.ConstantPixelSize;settings.scale=1;settings.targetTexture=target;
   host=new GameObject("Fit board");var document=host.AddComponent<UIDocument>();document.panelSettings=settings;yield return null;
   var parts=Build(document.rootVisualElement);for(int i=0;i<4;i++)yield return null;
   foreach(var part in parts){var r=part.worldBound;Assert.GreaterOrEqual(r.xMin,-.5f,$"{part.GetType().Name} {string.Join(" ",part.GetClasses())} starts on the {width} board");Assert.LessOrEqual(r.xMax,width+.5f,$"{part.GetType().Name} {string.Join(" ",part.GetClasses())} fits the {width} board");}
   int checkedTargets=0;foreach(var e in document.rootVisualElement.Query<VisualElement>().ToList()){if(!Target(e))continue;checkedTargets++;var r=e.worldBound;
    Assert.GreaterOrEqual(Mathf.Min(r.width,r.height),44-.5f,$"{e.GetType().Name} {string.Join(" ",e.GetClasses())} is a 44-unit target at {width} ({r.width:0}×{r.height:0})");}
   Assert.Greater(checkedTargets,20,"The fit board exercises the kit's controls");}
#else
  Assert.Ignore("Needs the editor asset database");yield break;
#endif
 }

 [UnityTest] public IEnumerator PopoversCarryTheRootAccessibilityFlags(){
  settings=ScriptableObject.CreateInstance<PanelSettings>();target=new RenderTexture(390,844,0);settings.targetTexture=target;host=new GameObject("Popover board");var document=host.AddComponent<UIDocument>();document.panelSettings=settings;yield return null;
  var board=Ui.Box(document.rootVisualElement,"vb-root");new SafeAreaRoot(board,settings).Flags(true,false,true,true);var chip=new ChipRow().Into(board).Bind(new PerkView{name="Overclock",now="+12% damage",stack=2,max=3});chip.Details.Pin();yield return null;
  var shown=document.rootVisualElement.panel.visualTree.Q(className:Popover.ClassName);Assert.IsNotNull(shown,"The pinned popover opens on the panel root");Assert.AreNotSame(board,shown.hierarchy.parent,"Popovers live outside the board");
  foreach(var c in new[]{SafeAreaRoot.Contrast,SafeAreaRoot.LargeText,SafeAreaRoot.ReducedFlashes})Assert.IsTrue(shown.ClassListContains(c),c+" reaches the popover");Assert.IsFalse(shown.ClassListContains(SafeAreaRoot.ReducedMotion));chip.Details.Close();}


 // A themed live panel of the given width on the portrait aspect; the test builds into <see cref="board"/>.
 VisualElement board;
 IEnumerator Boot(int width,int height=0,params string[] rootClasses){
#if UNITY_EDITOR
  Close();if(height<=0)height=Mathf.RoundToInt(width*2.164f);target=new RenderTexture(width,height,0);
  var theme=UnityEditor.AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>("Packages/com.veilbreak.gui/Runtime/Styles/VeilbreakGui.tss");Assert.IsNotNull(theme);
  settings=ScriptableObject.CreateInstance<PanelSettings>();settings.themeStyleSheet=theme;settings.scaleMode=PanelScaleMode.ConstantPixelSize;settings.scale=1;settings.targetTexture=target;
  host=new GameObject("Fit board");var document=host.AddComponent<UIDocument>();document.panelSettings=settings;yield return null;
  board=Ui.Box(document.rootVisualElement,"vb-root");board.style.flexGrow=1;foreach(var c in rootClasses)board.AddToClassList(c);
#else
  Assert.Ignore("Needs the editor asset database");yield break;
#endif
 }
 static IEnumerator Frames(int n=4){for(int i=0;i<n;i++)yield return null;}

 // ds#32: an action row's buttons fill it (equal shares, 8 between) and stack below the threshold; a fixed row keeps one width down a list.
 [UnityTest] public IEnumerator ActionRowFills(){
  foreach(int width in new[]{320,390,430}){
   yield return Boot(width);var page=Ui.Box(board,"vb-page");
   var pair=Ui.Actions(page,new VbButton("CANCEL"),new VbButton("CONFIRM",ButtonVariant.Primary));var trio=Ui.Actions(page,new VbButton("MENU"),new VbButton("SQUAD"),new VbButton("RUN AGAIN",ButtonVariant.Primary,null,"chevron"));
   trio.MinItemWidth=104;var rows=Ui.Box(page);var buy=new VbButton("BUY 120",ButtonVariant.Secondary);var cheap=new VbButton("BUY 80",ButtonVariant.Secondary);
   new ListRow("Overclock","+12% damage").WithAction(buy).Into(rows);new ListRow("Plating","+8% armour").WithAction(cheap).Into(rows);
   var confirm=new ConfirmDialogView("ABANDON RUN?","The run ends here.",("ABANDON","remove",null),("KEEP PLAYING","close"),()=>{},()=>{}).Into(board);
   var guide=new GuideDialogView("HOW TO PLAY",GuideDialogView.DefaultSteps,()=>{}).Into(board);
   yield return Frames();
   void Fills(ActionRow row,string what){float inner=row.worldBound.width;Assert.Greater(inner,0,what);var kids=row.Children().ToList();
    if(row.Stacked){foreach(var k in kids)Assert.AreEqual(inner,k.worldBound.width,.6f,$"A stacked row's {k.name} takes the full width: {what} at {width}");}
    else{float sum=0;foreach(var k in kids)sum+=k.worldBound.width;sum+=ActionRow.Gap*(kids.Count-1);Assert.GreaterOrEqual(sum/inner,.98f,$"{what} fills at {width} ({sum:0}/{inner:0})");
     for(int i=1;i<kids.Count;i++)Assert.AreEqual(kids[0].worldBound.width,kids[i].worldBound.width,1.1f,$"Children take equal shares (to a pixel): {what}");}}
   Fills(pair,"two buttons");Fills(trio,"three buttons");Fills(confirm.Q<ActionRow>(),"confirm dialog");Fills(guide.Q<ActionRow>(),"guide dialog");
   Assert.Greater(confirm.Q<ActionRow>().worldBound.width,(width-32)*.8f,"The confirm row spans its sheet");
   Assert.IsFalse(pair.Stacked,"Two buttons fit side by side even at 320");Assert.AreEqual(width<390,trio.Stacked,"Three buttons stack below 328 units");
   Assert.AreEqual(buy.worldBound.width,cheap.worldBound.width,.6f,"Trailing actions share one width down a list");}
 }

 // ds#30: a drawer is as tall as its content up to 90% of the board, and its body scrolls only past that.
 [UnityTest] public IEnumerator DrawerFitsContent(){
  foreach(int width in new[]{320,390,430}){
   yield return Boot(width);int height=Mathf.RoundToInt(width*2.164f);var small=new DrawerOverlay{Title="DETAILS",CloseButton=false}.Into(board);Ui.Box(small,"probe").style.height=200;small.Actions(new VbButton("REMOVE FROM SQUAD",ButtonVariant.Danger));small.Open();
   yield return Frames(6);var scroll=small.Q<ScrollView>();
   Assert.AreEqual(DrawerSnap.Fit,small.Snap,"Fit is the default");float need=small.Sheet.resolvedStyle.height;Assert.Less(need,height*.9f-1,$"A short body does not fill the sheet at {width} ({need:0} of {height})");
   Assert.GreaterOrEqual(scroll.contentViewport.worldBound.height+.5f,scroll.contentContainer.worldBound.height,"A short body fits its viewport: no scrolling");
   Assert.Greater(need,200+24+48,"The sheet holds the body, handle, head and foot");
   var tall=new DrawerOverlay{Title="DETAILS"}.Into(board);Ui.Box(tall,"probe").style.height=1600;tall.Open();yield return Frames(6);
   Assert.AreEqual(height*.9f,tall.Sheet.resolvedStyle.height,1.5f,$"A tall body stops at 90% at {width}");Assert.Greater(tall.Q<ScrollView>().contentContainer.worldBound.height,tall.Q<ScrollView>().contentViewport.worldBound.height,"Past 90% the body scrolls");
   tall.Snap=DrawerSnap.Half;yield return Frames(6);Assert.AreEqual(height*.5f,tall.Sheet.resolvedStyle.height,1.5f,"Half stays opt-in");
   tall.Snap=DrawerSnap.Full;yield return Frames(6);Assert.AreEqual(height,tall.Sheet.resolvedStyle.height,1.5f);}
 }

 // ds#24: a popover never leaves the panel, however tall its content, and is clamped again when the anchor moves or the panel shrinks.
 [UnityTest] public IEnumerator PopoverClampsToPanel(){
  foreach(int width in new[]{320,390}){
   yield return Boot(width);int height=Mathf.RoundToInt(width*2.164f);var anchor=new VbButton("ANCHOR"){name="anchor"}.Into(board);anchor.style.position=Position.Absolute;anchor.style.left=width-120;anchor.style.top=height-160;anchor.style.width=100;
   var popover=Popover.Attach(anchor,()=>{var card=new PopoverCard("STATUS","Tap a row for more");for(int i=0;i<40;i++)card.Row("Effect "+i,"×"+i);return card;});popover.Pin();yield return Frames(6);
   var shown=board.panel.visualTree.Q(className:Popover.ClassName);Assert.IsNotNull(shown);var panel=board.panel.visualTree.worldBound;
   void Inside(string what){VbMotion.Finish(shown);var r=shown.worldBound;Assert.GreaterOrEqual(r.xMin,panel.xMin+Popover.Edge-.6f,what+": left edge");Assert.LessOrEqual(r.xMax,panel.xMax-Popover.Edge+.6f,what+": right edge");Assert.GreaterOrEqual(r.yMin,panel.yMin+Popover.Edge-.6f,what+": top edge");Assert.LessOrEqual(r.yMax,panel.yMax-Popover.Edge+.6f,what+": bottom edge");}
   Inside($"tall content at {width}");Assert.LessOrEqual(shown.worldBound.height,height-2*Popover.Edge+.6f,"Height is clamped to the panel");Assert.IsNotNull(shown.Q(className:Popover.ClassName+"__arrow"),"A popover carries its arrow");
   Assert.IsTrue(shown.Q(className:Popover.ClassName+"__content")!=null,"Content is marked flat so the popover is the one surface");
   anchor.style.top=20;yield return Frames(4);Inside("after the anchor moved to the top");
   anchor.style.top=height/2;anchor.style.left=4;yield return Frames(4);Inside("after the anchor moved to the left edge");
   target.Release();target.width=width-60;target.height=height-200;target.Create();yield return Frames(8);var smaller=board.panel.visualTree.worldBound;panel=smaller;
   Assert.Less(smaller.width,width,"The panel shrank");Inside("after the panel shrank");
   popover.Close();}
 }


 // ds#22: what is nested paints no edge, what means a selection keeps its ring, and high contrast gives every edge back.
 [UnityTest] public IEnumerator NestedSurfacesPaintNoEdgeUnlessContrast(){
  yield return Boot(390);var card=new UnitCard().Into(board);card.Bind(new OperativeView{name="Aria",skills=new[]{new SkillView{slot="A"},new SkillView{slot="S"}}});
  var tiles=Ui.Box(board,"vb-grid4");var idle=new UnitTile().Into(tiles);idle.Bind(new OperativeView{name="Idle",role="Striker"},0,false);var focused=new UnitTile().Into(tiles);focused.Bind(new OperativeView{name="Focus",role="Striker"},1,true);
  var filled=new UnitSlot().Into(tiles);filled.Bind(new OperativeView{name="Slot",role="Striker"},1);var empty=new UnitSlot().Into(tiles);empty.Bind(null,2);yield return Frames();
  float Width(VisualElement e)=>e.customStyle.TryGetValue(new CustomStyleProperty<float>("--vb-bw"),out var w)?w:-1;float Dash(VisualElement e)=>e.customStyle.TryGetValue(new CustomStyleProperty<float>("--vb-dash"),out var d)?d:-1;
  var skill=card.Skill(0);Assert.AreEqual(0,Width(skill),"A skill button inside a battle card has no edge");Assert.AreEqual(0,Width(idle),"An idle tile has no edge");Assert.AreEqual(0,Width(filled),"A filled squad slot has no edge");
  Assert.AreEqual(2,Width(focused),"A focused tile keeps its selection ring");Assert.AreEqual(1,Dash(empty),"An empty slot keeps its dashed drop target");Assert.Greater(Width(card),0,"The floating battle card keeps its edge");
  board.AddToClassList(SafeAreaRoot.Contrast);yield return Frames();
  Assert.AreEqual(1,Width(skill),"High contrast gives the skill its edge back");Assert.AreEqual(1,Width(idle));Assert.AreEqual(2,Width(focused),"…and the focus ring stays a ring");
 }

 // ds#28: dragging a filled slot onto another raises SlotMoved, a tap does not, empty slots cannot be lifted and the keyboard moves a slot too.
 [UnityTest] public IEnumerator SquadSlotsReorderByDragging(){
  yield return Boot(390,0,SafeAreaRoot.ReducedMotion);var view=new SquadView().Into(board);var roster=new OperativeView[4];for(int i=0;i<4;i++)roster[i]=new OperativeView{name="Op"+i,role="Striker"};
  view.Bind(new SquadViewModel{roster=roster,selected=new[]{0,1,-1,-1}});var moves=new List<(int,int)>();view.SlotMoved+=(a,b)=>moves.Add((a,b));yield return Frames();
  var grid=view.Reorder;var slots=board.Query<UnitSlot>().ToList();Assert.AreEqual(4,slots.Count);Vector2 C(int i)=>slots[i].worldBound.center;
  Assert.IsTrue(grid.Begin(1,C(0)));grid.End(C(0));Assert.AreEqual(0,moves.Count,"A tap never lifts");Assert.AreEqual(-1,grid.Lifted);
  Assert.IsTrue(grid.Begin(1,C(0)));grid.Drag(C(0)+new Vector2(3,0));Assert.AreEqual(-1,grid.Lifted,"Under the slop nothing lifts");grid.Drag(C(1));Assert.AreEqual(0,grid.Lifted);Assert.AreEqual(1,grid.Over);
  Assert.IsNotNull(grid.Ghost);Assert.IsTrue(slots[0].ClassListContains(ReorderManipulator.SourceClass));Assert.IsTrue(slots[1].ClassListContains(ReorderManipulator.OverClass));Assert.IsNotNull(grid.Ghost.panel);
  grid.End(C(1));CollectionAssert.AreEqual(new[]{(0,1)},moves);Assert.IsNull(grid.Ghost,"Reduced motion removes the ghost at once");Assert.IsFalse(slots[0].ClassListContains(ReorderManipulator.SourceClass));Assert.IsFalse(slots[1].ClassListContains(ReorderManipulator.OverClass));
  moves.Clear();Assert.IsTrue(grid.Begin(1,C(0)));grid.Drag(C(3));grid.End(C(3));CollectionAssert.AreEqual(new[]{(0,1)},moves,"A drop on an empty slot reports the last filled one");
  moves.Clear();Assert.IsFalse(grid.Begin(1,C(3)),"An empty slot cannot be lifted");Assert.IsTrue(grid.Begin(1,C(1)));grid.Drag(C(1)+new Vector2(0,40));grid.End(C(1));Assert.AreEqual(0,moves.Count,"Dropping on its own slot is no move");
  Assert.IsTrue(grid.Nudge(1,-1));CollectionAssert.AreEqual(new[]{(1,0)},moves);
 }

 // Interactive and visible: buttons, skill buttons, segmented options, HUD toggles, queue ×, rule toggles, switches.
 static bool Target(VisualElement e){if(!e.visible||e.resolvedStyle.display==DisplayStyle.None)return false;for(var p=e;p!=null;p=p.parent)if(p.resolvedStyle.display==DisplayStyle.None)return false;
  return e is VbButton||e is SkillButton||e is VbSwitch||e.ClassListContains("vb-segmented__option")||e.ClassListContains("vb-queue-chip__cancel")||e.ClassListContains("vb-rule__toggle")||e.ClassListContains("vb-hud__auto")||e.ClassListContains("vb-hud__speed")||e.ClassListContains("vb-hud__pause");}

 static List<VisualElement> Build(VisualElement root){var parts=new List<VisualElement>();var board=Ui.Box(root,"vb-root");board.style.flexGrow=1;
  var hud=new HudTop().Into(board).Bind(new HudView{wave=12,score=1234567,power=62,queue=new[]{new QueueView{who="AE",skill="Absolute Wall",cost=40,eta="1.2s",manual=true}}});parts.Add(hud);
  var queue=new[]{new QueueView{who="AE",skill="Absolute Wall",cost=40,eta="1.2s",manual=true},new QueueView{who="NY",skill="White Noise",cost=40,eta="4.1s",manual=true},new QueueView{who="SO",skill="Zero Line",cost=36,eta="6.0s",manual=true},new QueueView{who="AR",skill="Needle Storm",cost=18,eta="7.4s",manual=true}};
  var stripRow=Ui.Box(board);stripRow.style.paddingLeft=stripRow.style.paddingRight=10;parts.Add(new PowerStrip().Into(stripRow).Set(62,100,"+12/s","Absolute Wall replaces auto · −40",22,62).Reserve(25).Queue(queue,_=>{}).Legend("┊ autoplay reserve"));
  var cards=Ui.Box(board,"vb-row","vb-row--between");cards.style.paddingLeft=cards.style.paddingRight=10;for(int i=0;i<4;i++){var skills=Ui.Box(cards,"vb-row");new SkillButton().Into(skills).Bind(new SkillView{state=(SkillState)(i*3%14),cost=18,progress=.5f,reason="Out of range"});new SkillButton().Into(skills).Bind(new SkillView{slot="S",cost=40});parts.Add(skills);}
  var screen=Ui.Box(board);screen.style.flexGrow=1;var nav=new Navigator(screen);var page=nav.Open("settings");new ScreenHeader("SETTINGS","SAVED").Into(page);page.AddToClassList("vb-page--headed");var body=Ui.Scroll(page,"vb-page__scroll").contentContainer;
  var camera=Ui.Section(body,"CAMERA");new VbSegmented(new[]{("FULL","Parallax"),("REDUCED","No drift"),("OFF","Fixed")},0).Into(camera);new VbSlider("Sensitivity",.5f).Into(camera);new VbSwitch("REDUCED MOTION","No camera shake",false).Into(camera);parts.Add(camera);
  parts.Add(new RuleRow().Into(body).Bind(new RuleView{conditions=new[]{new RuleCondition{text="Power ≥",value="80"},new RuleCondition{text="enemy within",value="6",after="m"}},action="Zero Line",cost=40,note="Runs only at 80+ Power: 40 cost + 40 reserve"}));
  var grid=Ui.Box(body,"vb-grid4");for(int i=0;i<4;i++)new UnitTile().Into(grid).Bind(new OperativeView{name="Operative",role="Striker"},i+1,i==0,(RarityTier)(i%4));parts.Add(grid);
  var sheet=Dialog.Push(nav,"LEAVE GAME?","Your profile is saved. Resume returns to the last completed-wave checkpoint.");new VbButton("EXIT GAME",ButtonVariant.Primary,null,"back").Into(sheet);new VbButton("CANCEL",ButtonVariant.Secondary,null,"close").Into(sheet);parts.Add(sheet);
  return parts;}
}
}
