using System.Collections;
using System.Collections.Generic;
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
  var board=Ui.Box(document.rootVisualElement,"vb-root");new SafeAreaRoot(board,settings).Flags(true,false,true,true);var chip=new PerkChip().Into(board).Bind(new PerkView{name="Overclock",now="+12% damage",stack=2,max=3});chip.Details.Pin();yield return null;
  var shown=document.rootVisualElement.panel.visualTree.Q(className:Popover.ClassName);Assert.IsNotNull(shown,"The pinned popover opens on the panel root");Assert.AreNotSame(board,shown.hierarchy.parent,"Popovers live outside the board");
  foreach(var c in new[]{SafeAreaRoot.Contrast,SafeAreaRoot.LargeText,SafeAreaRoot.ReducedFlashes})Assert.IsTrue(shown.ClassListContains(c),c+" reaches the popover");Assert.IsFalse(shown.ClassListContains(SafeAreaRoot.ReducedMotion));chip.Details.Close();}

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
  var grid=Ui.Box(body,"vb-grid4");for(int i=0;i<4;i++)new OperativeTile().Into(grid).Bind(new OperativeView{name="Operative",role="Striker"},i+1,i==0,(RarityTier)(i%4));parts.Add(grid);
  var sheet=Dialog.Push(nav,"LEAVE GAME?","Your profile is saved. Resume returns to the last completed-wave checkpoint.");new VbButton("EXIT GAME",ButtonVariant.Primary,null,"back").Into(sheet);new VbButton("CANCEL",ButtonVariant.Secondary,null,"close").Into(sheet);parts.Add(sheet);
  return parts;}
}
}
