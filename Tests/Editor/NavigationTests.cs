using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;
namespace Veilbreak.Gui.Tests {
public sealed class NavigationTests {
 static (VisualElement root,VisualElement screen,Navigator nav) Board(){var root=new VisualElement();var screen=new VisualElement{name="screen"};root.Add(screen);return (root,screen,new Navigator(screen));}

 [Test] public void OpenReplacesThePageAndNamesTheScreen(){var (_,screen,nav)=Board();var opened=new List<string>();int closed=0;nav.Opening+=opened.Add;nav.Closed+=()=>closed++;
  var squad=nav.Open("squad");Assert.IsTrue(screen.ClassListContains("vb-screen")&&screen.ClassListContains("vb-screen--squad"));Assert.IsTrue(squad.ClassListContains("vb-page")&&squad.ClassListContains("vb-page--squad"));Assert.AreSame(squad,nav.Content);Assert.IsTrue(nav.Visible);
  var menu=nav.Open("menu");Assert.IsFalse(screen.ClassListContains("vb-screen--squad"),"The previous page's screen modifier goes");Assert.AreEqual(1,screen.childCount);Assert.AreSame(menu,screen[0]);Assert.AreEqual("menu",nav.Page);CollectionAssert.AreEqual(new[]{"squad","menu"},opened);
  nav.Close();Assert.IsFalse(nav.Visible);Assert.AreEqual(1,closed);Assert.AreEqual("",nav.Page);}

 [Test] public void BackClosesModalThenPageThenFallsBack(){var (_,screen,nav)=Board();var log=new List<string>();nav.Fallback=()=>log.Add("fallback");
  var page=nav.Open("squad",()=>log.Add("page"));var layer=nav.Push(()=>log.Add("modal closed"));Assert.AreEqual(1,nav.Modals.Count);Assert.IsTrue(page.ClassListContains("vb-covered"));Assert.IsFalse(page.enabledSelf,"The covered page stops taking input");Assert.AreSame(screen,layer.parent,"Modal layers sit on the screen, above the page, not inside it");
  Assert.IsTrue(nav.Back());CollectionAssert.AreEqual(new[]{"modal closed"},log);Assert.AreEqual(0,nav.Modals.Count);Assert.IsTrue(page.enabledSelf);Assert.IsFalse(page.ClassListContains("vb-covered"));
  Assert.IsTrue(nav.Back());CollectionAssert.AreEqual(new[]{"modal closed","page"},log);
  nav.PageBack=null;Assert.IsTrue(nav.Back());Assert.AreEqual("fallback",log[log.Count-1]);
  nav.Fallback=null;Assert.IsFalse(nav.Back(),"Nothing handled it");
  nav.Blocked=()=>true;nav.PageBack=()=>log.Add("blocked page");Assert.IsTrue(nav.Back());Assert.AreNotEqual("blocked page",log[log.Count-1],"A blocking intro swallows Back");}

 [Test] public void OpeningAPageDropsItsModals(){var (_,screen,nav)=Board();int closed=0;nav.Open("squad");nav.Push(()=>closed++);nav.Push(()=>closed++);Assert.AreEqual(2,nav.Modals.Count);
  nav.Open("menu");Assert.AreEqual(0,nav.Modals.Count);Assert.AreEqual(2,closed);Assert.IsNull(screen.Q("modal-layer"));}

 [Test] public void PushWithoutAPageCoversAnEmptyPage(){var (_,screen,nav)=Board();screen.style.display=DisplayStyle.None;var layer=nav.Push();Assert.IsTrue(nav.Visible,"A modal shows the screen layer");Assert.IsTrue(layer.enabledInHierarchy,"The layer is not inside the element it disables");Assert.IsTrue(screen[0].ClassListContains("vb-page"));}

 [Test] public void DialogSheetsCarryTitleBodyAndClasses(){var (_,_,nav)=Board();nav.Open("settings");var sheet=Dialog.Push(nav,"CLEAR LOCAL SCORES?","Kept: checkpoint and settings.","vb-t-caption","extra");
  Assert.IsTrue(sheet.ClassListContains(Dialog.ClassName)&&sheet.ClassListContains("vb-panel")&&sheet.ClassListContains("vb-panel--solid")&&sheet.ClassListContains("extra"));Assert.AreEqual("CLEAR LOCAL SCORES?",sheet.Q<Label>(className:"vb-dialog__title").text);Assert.IsTrue(sheet.Q<Label>(className:"vb-dialog__body").ClassListContains("vb-t-caption"));
  Assert.AreEqual(1,nav.Modals.Count);Assert.IsNull(Dialog.Sheet(new VisualElement(),null).Q<Label>(),"No title or body, no labels");}

 [Test] public void SafeAreaInsetsUsePanelUnitsAndMinimums(){var panel=ScriptableObject.CreateInstance<PanelSettings>();panel.scaleMode=PanelScaleMode.ScaleWithScreenSize;panel.referenceResolution=new Vector2Int(390,844);panel.screenMatchMode=PanelScreenMatchMode.MatchWidthOrHeight;panel.match=0;
  try{var root=new VisualElement();var overlay=new VisualElement();var safe=new SafeAreaRoot(root,panel,overlay);var screen=new Vector2(780,1688);Assert.AreEqual(.5f,safe.UnitsPerPixel(screen),1e-4f,"A 2× screen at match-width is half a unit per pixel");
   safe.Apply(new Rect(0,0,780,1688),screen);Assert.AreEqual(new Vector4(0,48,0,34),safe.Insets,"No reported inset still keeps status-bar and home-indicator room");
   safe.Apply(new Rect(40,68,700,1688-68-200),screen);Assert.AreEqual(new Vector4(20,100,20,34),safe.Insets);Assert.AreEqual(100,root.style.paddingTop.value.value);Assert.AreEqual(20,overlay.style.left.value.value);Assert.AreEqual(34,overlay.style.bottom.value.value,"Overlays inset by the same amounts");
   safe.MinTop=0;safe.MinBottom=0;safe.Apply(new Rect(0,0,780,1688),screen);Assert.AreEqual(Vector4.zero,safe.Insets);
   panel.match=1;Assert.AreEqual(.5f,safe.UnitsPerPixel(screen),1e-4f);panel.scaleMode=PanelScaleMode.ConstantPixelSize;panel.scale=2;Assert.AreEqual(.5f,safe.UnitsPerPixel(screen),1e-4f);}
  finally{Object.DestroyImmediate(panel);}}

 [Test] public void RootFlagsToggleAccessibilityClasses(){var root=new VisualElement();var safe=new SafeAreaRoot(root,null);safe.Flags(true,false,true);Assert.IsTrue(root.ClassListContains(SafeAreaRoot.LargeText));Assert.IsFalse(root.ClassListContains(SafeAreaRoot.ReducedMotion));Assert.IsTrue(root.ClassListContains(SafeAreaRoot.Contrast));Assert.IsFalse(root.ClassListContains(SafeAreaRoot.Compact),"Off a panel the board width is unknown");
  safe.Flags(false,true,false);Assert.IsFalse(root.ClassListContains(SafeAreaRoot.LargeText));Assert.IsTrue(root.ClassListContains(SafeAreaRoot.ReducedMotion));Assert.IsFalse(root.ClassListContains(SafeAreaRoot.Contrast));Assert.IsFalse(root.ClassListContains(SafeAreaRoot.ReducedFlashes));
  safe.Flags(false,false,false,true);Assert.IsTrue(root.ClassListContains(SafeAreaRoot.ReducedFlashes));}

 [Test] public void ContrastThemeOverridesTheSameColoursAsTheClass(){var root=System.IO.File.ReadAllText(PackageFiles.Path("Runtime/Styles/TokensContrast.uss"));var tokens=System.IO.File.ReadAllText(PackageFiles.Path("Runtime/Styles/Tokens.uss"));var block=tokens.Substring(tokens.IndexOf(".vb-contrast {"));
  Assert.AreEqual(block.Substring(block.IndexOf('{')),root.Substring(root.IndexOf('{')),"TokensContrast.uss is the .vb-contrast block on :root; run Tools~/tokens.py");
  StringAssert.Contains("TokensContrast.uss",System.IO.File.ReadAllText(PackageFiles.Path("Runtime/Styles/VeilbreakGuiContrast.tss")));StringAssert.Contains("Layout.uss",System.IO.File.ReadAllText(PackageFiles.Path("Runtime/Styles/VeilbreakGui.tss")));}
}
}
