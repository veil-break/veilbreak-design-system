using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
namespace Veilbreak.Gui.Tests {
/// <summary>The GUI polish batch (design-system#55, gui 0.9.0) on live portrait boards at 320, 390 and 430 units: the fluid scale, the type variants, the single-row menu and pause actions, content that follows the cut, the status row above a
/// unit card, the carousel's layout and its swipe, tap and wrap, and the overlay transitions.</summary>
public sealed class PolishFitTests {
 GameObject host;PanelSettings settings;RenderTexture target;VisualElement board;
 [TearDown] public void Close(){if(host!=null)Object.Destroy(host);if(settings!=null)Object.Destroy(settings);if(target!=null)target.Release();}
 // A themed live panel of the given width on the portrait aspect; the root carries the fluid step and the compact flag as SafeAreaRoot gives them.
 IEnumerator Boot(int width,params string[] rootClasses){
#if UNITY_EDITOR
  Close();int height=Mathf.RoundToInt(width*2.164f);target=new RenderTexture(width,height,0);
  var theme=UnityEditor.AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>("Packages/com.veilbreak.gui/Runtime/Styles/VeilbreakGui.tss");Assert.IsNotNull(theme);
  settings=ScriptableObject.CreateInstance<PanelSettings>();settings.themeStyleSheet=theme;settings.scaleMode=PanelScaleMode.ConstantPixelSize;settings.scale=1;settings.targetTexture=target;
  host=new GameObject("Polish board");var document=host.AddComponent<UIDocument>();document.panelSettings=settings;yield return null;
  board=Ui.Box(document.rootVisualElement,"vb-root");board.style.flexGrow=1;if(width<380)board.AddToClassList("vb-compact");VbFluid.Apply(board,width);foreach(var c in rootClasses)board.AddToClassList(c);
#else
  Assert.Ignore("Needs the editor asset database");yield break;
#endif
 }
 static IEnumerator Frames(int n=5){for(int i=0;i<n;i++)yield return null;}
 static bool Shown(VisualElement e){for(var p=e;p!=null;p=p.parent)if(p.resolvedStyle.display==DisplayStyle.None)return false;return true;}
 static EffectInstance Status(string id,float seconds,EffectPolarity p=EffectPolarity.Harm)=>new EffectInstance{id=id,definition=new EffectDefinition{id=id,title=id,icon="slow",polarity=p,description="Lasts a while."},remainingSeconds=seconds,magnitude="−20%"};

 // Text, gutters and margins scale with the panel: smaller boards get smaller values, the 390 reference is the baseline, and the label is 0.88× the body at every width.
 [UnityTest] public IEnumerator TextGuttersAndMarginsScaleWithThePanelWidth(){
  var body=new Dictionary<int,float>();var gutter=new Dictionary<int,float>();
  foreach(int width in new[]{320,390,430}){
   yield return Boot(width);var page=Ui.Box(board,"vb-page");var text=Ui.Text(page,"Drag an operative to move.","vb-t-body");var label=Ui.Text(page,"WAVE 07","vb-t-label");var heading=Ui.Text(page,"PAUSED","vb-t-heading");yield return Frames();
   float f=VbFluid.Factor(width);body[width]=text.resolvedStyle.fontSize;gutter[width]=page.resolvedStyle.paddingLeft;
   Assert.AreEqual(13*f,body[width],.06f,"Body text at "+width);Assert.AreEqual(16*f,gutter[width],.55f,"The page gutter at "+width+" (layout rounds to whole units)");Assert.AreEqual(.88f,label.resolvedStyle.fontSize/body[width],.015f,"Label is 0.88× body at "+width);
   Assert.AreEqual(20*f,heading.resolvedStyle.fontSize,.06f,"Heading at "+width);Assert.Less(heading.resolvedStyle.letterSpacing,0,"Heading tracking is negative");Assert.Greater(label.resolvedStyle.letterSpacing,0,"Label is tracked out");
}
  Assert.Less(body[320],body[390]);Assert.Less(body[390],body[430]);Assert.Less(gutter[320],gutter[390]);Assert.Less(gutter[390],gutter[430]);}

 // The primary button's text is the label size the other buttons use, in the same face and tracking.
 [UnityTest] public IEnumerator ThePrimaryButtonTextMatchesTheOthers(){
  foreach(int width in new[]{320,390,430}){
   yield return Boot(width);var page=Ui.Box(board,"vb-page");var primary=new VbButton("PLAY",ButtonVariant.Primary).Into(page);var secondary=new VbButton("SCORES",ButtonVariant.Secondary).Into(page);var danger=new VbButton("ABANDON",ButtonVariant.Danger).Into(page);yield return Frames();
   var p=primary.Q<Label>(className:"vb-button__label").resolvedStyle;var s=secondary.Q<Label>(className:"vb-button__label").resolvedStyle;var d=danger.Q<Label>(className:"vb-button__label").resolvedStyle;
   Assert.AreEqual(s.fontSize,p.fontSize,.01f,"Primary and secondary share a size at "+width);Assert.AreEqual(s.fontSize,d.fontSize,.01f);Assert.AreEqual(s.letterSpacing,p.letterSpacing,.01f,"…and a tracking");Assert.AreEqual(s.unityFontDefinition.fontAsset?.name??s.unityFontDefinition.font?.name,p.unityFontDefinition.fontAsset?.name??p.unityFontDefinition.font?.name,"…and a face");}}

 // The menu's and the pause sheet's secondary buttons are one row at every width, the settings button a cog of at least 44 units.
 [UnityTest] public IEnumerator MenuAndPauseSecondariesStayOnOneRowWithACog(){
  foreach(int width in new[]{320,390,430}){
   yield return Boot(width);var page=Ui.Box(board,"vb-page");page.style.flexGrow=1;var menu=new MenuActionsSectionView().Into(page);menu.Bind(null,"Squad · Aria · Sol");
   var pause=new PauseOverlayView().Into(board);pause.style.position=Position.Absolute;pause.style.left=pause.style.right=pause.style.top=pause.style.bottom=0;pause.Bind(new PauseViewModel{wave=7,score=1200,standing=4});yield return Frames(8);
   foreach(var row in new[]{menu.Q<ActionRow>(),pause.Q<ActionRow>(className:"vb-pause__row")}){
    Assert.IsFalse(row.Stacked,"A single row never stacks at "+width);var kids=row.Children().Where(Shown).ToList();Assert.GreaterOrEqual(kids.Count,2);
    Assert.AreEqual(1,kids.Select(k=>Mathf.RoundToInt(k.worldBound.yMin)).Distinct().Count(),"One line at "+width);
    var cog=(VbButton)kids.Last();Assert.IsTrue(cog.IconOnly);Assert.GreaterOrEqual(cog.worldBound.width,43.5f,"The cog is a 44-unit target");Assert.GreaterOrEqual(cog.worldBound.height,43.5f);
    foreach(var k in kids){Assert.GreaterOrEqual(k.worldBound.xMin,board.worldBound.xMin-.5f);Assert.LessOrEqual(k.worldBound.xMax,board.worldBound.xMax+.5f,"Nothing leaves the "+width+" board");}
    foreach(var b in kids.Take(kids.Count-1).OfType<VbButton>())Assert.Greater(b.worldBound.width,60,"The labelled buttons share what the cog leaves");}
   Assert.AreEqual(1,pause.Query<VbButton>().ToList().Count(b=>b.Variant==ButtonVariant.Primary));}}

 // Art and fills follow the cut: nothing a card's portrait or a skill's charge paints lies inside the chamfers, and the last skill's corner lies on the card's.
 [UnityTest] public IEnumerator PortraitsAndSkillFillsFollowTheCut(){
  var tex=new Texture2D(4,6);
  foreach(int width in new[]{320,390,430}){
   yield return Boot(width);var page=Ui.Box(board,"vb-page");var row=Ui.Box(page,"vb-row");var card=new UnitCard{AutoDamage=false}.Into(row);
   card.Bind(new OperativeView{name="Aria",portrait=tex,hp=900,maxHp=1000,skills=new[]{new SkillView{state=SkillState.Cooldown,charged=1,cost=10,seconds=3},new SkillView{slot="S",state=SkillState.Cooldown,charged=1,cost=40,seconds=9}}});yield return Frames();
   var portrait=card.Q<VbClipImage>();float cut=10;Assert.AreEqual(cut,card.customStyle.TryGetValue(new CustomStyleProperty<float>("--vb-cut"),out var c)?c:-1,"The card's cut");
   var polygon=VbClip.Polygon(portrait,new Rect(0,0,portrait.layout.width,portrait.layout.height));Assert.GreaterOrEqual(polygon.Count,3);float clippedArea=Area(polygon);Assert.Less(clippedArea,portrait.layout.width*portrait.layout.height,"The portrait is smaller than its box: the corner is taken off");
   foreach(var p in polygon){var inCard=card.WorldToLocal(portrait.LocalToWorld(p));Assert.GreaterOrEqual(inCard.x+inCard.y,cut-.05f,"The portrait stays out of the top-left cutout at "+width+": "+inCard);}
   var end=card.Skill(1);var charge=end.Q<VbClipFill>(className:"vb-skill__charge");Assert.IsTrue(Shown(charge));var fill=VbClip.Polygon(charge,new Rect(0,0,charge.layout.width,charge.layout.height));
   foreach(var p in fill){var inCard=card.WorldToLocal(charge.LocalToWorld(p));Assert.GreaterOrEqual((card.layout.width-inCard.x)+(card.layout.height-inCard.y),cut-.05f,"The charge stays out of the card's bottom-right cut at "+width+": "+inCard);}
   Assert.Less(Area(fill),charge.layout.width*charge.layout.height-1,"The charge's corner is taken off");
   var tile=new UnitTile().Into(row);tile.Bind(new OperativeView{name="Aria",portrait=tex},1,false);yield return Frames();var tp=tile.Q<VbClipImage>();var tpoly=VbClip.Polygon(tp,new Rect(0,0,tp.layout.width,tp.layout.height));
   foreach(var p in tpoly){var inTile=tile.WorldToLocal(tp.LocalToWorld(p));Assert.GreaterOrEqual(inTile.x+inTile.y,10-.05f,"A tile's picture stays out of its cutout too");}}
  Object.DestroyImmediate(tex);}
 static float Area(IReadOnlyList<Vector2> p){float a=0;for(int i=0;i<p.Count;i++){var q=p[(i+1)%p.Count];a+=p[i].x*q.y-q.x*p[i].y;}return Mathf.Abs(a)/2;}

 // Tiny squares above the card, a 44-unit hit area, and a vertical list in a popover.
 [UnityTest] public IEnumerator TheStatusRowIsTinySquaresAboveTheCardWithAVerticalPopover(){
  foreach(int width in new[]{320,390,430}){
   yield return Boot(width);var page=Ui.Box(board,"vb-page");page.style.paddingTop=60;var holder=Ui.Box(page,"vb-row");holder.style.paddingLeft=20;var card=new UnitCard{AutoDamage=false}.Into(holder);
   card.Bind(new OperativeView{name="Aria",hp=900,maxHp=1000,effects=new[]{Status("slow",3),Status("haste",5,EffectPolarity.Benefit),Status("exposed",2)}});yield return Frames();
   var row=card.Statuses;var squares=row.Squares.Where(Shown).ToList();Assert.AreEqual(3,squares.Count);
   foreach(var s in squares){Assert.AreEqual(20,s.worldBound.width,1f,"About 20 units across at "+width);Assert.AreEqual(20,s.worldBound.height,1f);Assert.LessOrEqual(s.worldBound.yMax,card.worldBound.yMin+.6f,"The squares sit above the card");}
   Assert.GreaterOrEqual(row.worldBound.height,43.5f,"The row is a 44-unit hit area");Assert.GreaterOrEqual(row.worldBound.width,43.5f);Assert.AreEqual(squares[0].worldBound.yMin,squares[2].worldBound.yMin,.6f,"One row");Assert.Less(squares[0].worldBound.xMin,squares[2].worldBound.xMin);
   row.Details.Open();yield return Frames(3);var popup=board.panel.visualTree.Q(className:Popover.ClassName);Assert.IsNotNull(popup,"Hover or tap opens the list");var rows=popup.Query(className:"vb-effect-row").ToList().Where(Shown).ToList();Assert.AreEqual(3,rows.Count);
   for(int i=1;i<rows.Count;i++)Assert.Greater(rows[i].worldBound.yMin,rows[i-1].worldBound.yMin+10,"The statuses are listed vertically");
   Assert.GreaterOrEqual(popup.worldBound.xMin,-.5f);Assert.LessOrEqual(popup.worldBound.xMax,width+.5f,"The popover stays on the "+width+" board");row.Details.Close();}}

 // The carousel: the active card takes most of the width and sits on top, its neighbours are partly beneath and tilted, a swipe or a tap on one changes the card, and it wraps both ways.
 [UnityTest] public IEnumerator TheCarouselShowsTheActiveCardBigWithTiltedNeighboursAndSwipes(){
  foreach(int width in new[]{320,390,430}){
   yield return Boot(width);var page=Ui.Box(board,"vb-page");page.style.flexGrow=1;var picks=new[]{new PerkView{id="a",name="Overclock",effect="+12% damage",next="+24% damage",stack=1,max=3,icon="burst"},new PerkView{id="b",name="Plating",effect="+8 armour",next="+16 armour",icon="armor"},new PerkView{id="c",name="Quickdraw",effect="*Faster* skills",next="+12%",icon="haste",tier=PerkTier.Rare}};
   var view=new PerkDraftView().Into(page);view.style.flexGrow=1;view.Bind(new PerkDraftViewModel{wave=3,choices=picks,pick=1});yield return Frames(8);
   var carousel=view.Choices;var changes=new List<int>();carousel.Changed+=changes.Add;Assert.AreEqual(1,carousel.Index);Assert.AreEqual(3,carousel.Count);
   var active=(ChoiceCard)carousel.Active;float cw=carousel.worldBound.width*Carousel.ActiveWidth;Assert.AreEqual(cw,active.worldBound.width,2f,"The active card takes most of the width at "+width);Assert.AreEqual(carousel.worldBound.center.x,active.worldBound.center.x,2f,"…centred");
   Assert.AreEqual(carousel.Cards.Count-1,carousel.Cards.Select(c=>c.parent.IndexOf(c)).Max()-carousel.Cards.Select(c=>c.parent.IndexOf(c)).Min(),"Cards are siblings");Assert.AreEqual(active.parent.childCount-1,active.parent.IndexOf(active),"The active card paints on top");
   var left=(ChoiceCard)carousel.Cards[0];var right=(ChoiceCard)carousel.Cards[2];Assert.Less(left.worldBound.center.x,active.worldBound.center.x);Assert.Greater(right.worldBound.center.x,active.worldBound.center.x);
   Assert.Greater(left.worldBound.xMax,active.worldBound.xMin,"The left neighbour is partly beneath the active card");Assert.Less(right.worldBound.xMin,active.worldBound.xMax);Assert.Less(left.worldBound.xMin,active.worldBound.xMin,"…and peeks out");
   Assert.AreNotEqual(0,left.resolvedStyle.rotate.angle.value,"Slightly rotated");Assert.AreEqual(-left.resolvedStyle.rotate.angle.value,right.resolvedStyle.rotate.angle.value,.01f);Assert.Less(Mathf.Abs(right.resolvedStyle.rotate.angle.value),15);
   Assert.GreaterOrEqual(active.worldBound.xMin,board.worldBound.xMin-.5f);Assert.LessOrEqual(active.worldBound.xMax,board.worldBound.xMax+.5f,"The active card fits the "+width+" board");
   Assert.IsNull(view.Q(className:"vb-perk-detail"),"The draft does not repeat the active card's information");
   // Tap the card beneath on the right: it becomes the active one.
   Tap(right,right.worldBound.center);yield return Frames(3);Assert.AreEqual(2,carousel.Index);CollectionAssert.AreEqual(new[]{2},changes);
   yield return new WaitForSecondsRealtime(.45f);yield return Frames(2);Assert.IsFalse(carousel.Animating);Assert.AreEqual(2,carousel.Slot,1e-3f);Assert.AreEqual(carousel.Cards[2].parent.childCount-1,carousel.Cards[2].parent.IndexOf(carousel.Cards[2]),"The tapped card is on top now");
   // Swipe left one card: forward, wrapping from the last to the first.
   Swipe(carousel,active:(VisualElement)carousel.Active,-carousel.worldBound.width*.55f);yield return new WaitForSecondsRealtime(.45f);yield return Frames(2);Assert.AreEqual(0,carousel.Index,"A swipe past the last card wraps to the first");CollectionAssert.AreEqual(new[]{2,0},changes);
   Swipe(carousel,active:(VisualElement)carousel.Active,carousel.worldBound.width*.55f);yield return new WaitForSecondsRealtime(.45f);yield return Frames(2);Assert.AreEqual(2,carousel.Index,"…and back from the first to the last");
   carousel.Next();yield return new WaitForSecondsRealtime(.45f);Assert.AreEqual(0,carousel.Index);}}
 static void Pointer<T>(VisualElement target,Vector2 at,EventType type) where T:PointerEventBase<T>,new(){var system=new Event{type=type,mousePosition=at,button=0};using(var e=PointerEventBase<T>.GetPooled(system)){e.target=target;target.SendEvent(e);}}
 static void Tap(VisualElement target,Vector2 at){Pointer<PointerDownEvent>(target,at,EventType.MouseDown);Pointer<PointerUpEvent>(target,at,EventType.MouseUp);}
 static void Swipe(Carousel carousel,VisualElement active,float dx){var at=active.worldBound.center;Pointer<PointerDownEvent>(active,at,EventType.MouseDown);foreach(float k in new[]{.1f,.3f,.6f,1f})Pointer<PointerMoveEvent>(carousel,at+new Vector2(dx*k,0),EventType.MouseDrag);Pointer<PointerUpEvent>(carousel,at+new Vector2(dx,0),EventType.MouseUp);}

 // A dialog and a pause sheet fade and rise in on a short recipe and leave by it; none is left half-transparent.
 [UnityTest] public IEnumerator OverlaysTransitionInAndOutThroughOneRecipe(){
  yield return Boot(390);var screen=Ui.Box(board,"vb-screen");screen.style.position=Position.Absolute;screen.style.left=screen.style.right=screen.style.top=screen.style.bottom=0;var nav=new Navigator(screen);nav.Open("pause");
  var dialog=ConfirmDialogView.Push(nav,"END THIS RUN?","It is recorded.",("ABANDON","remove",()=>{}),("KEEP","close"));var layer=dialog.parent;yield return null;
  Assert.IsTrue(layer.ClassListContains("vb-motion-active"),"The layer is in motion at once");Assert.Less(layer.resolvedStyle.opacity,1f,"…and not yet opaque");
  yield return new WaitForSecondsRealtime(.35f);yield return Frames(2);Assert.IsFalse(layer.ClassListContains("vb-motion-active"));Assert.AreEqual(1,layer.resolvedStyle.opacity,.01f);Assert.AreEqual(OverlayPhase.Open,((OverlayView)layer).Phase);
   nav.Modals.Pop();yield return null;Assert.IsTrue(layer.ClassListContains("vb-motion-active"),"It leaves by its exit recipe");Assert.IsNotNull(layer.parent,"…still there while it fades");yield return new WaitForSecondsRealtime(.3f);yield return Frames(2);Assert.IsNull(layer.parent,"…and gone after it");
  var pause=new PauseOverlayView().Into(board);pause.Bind(new PauseViewModel{wave=3});yield return null;Assert.AreEqual(OverlayPhase.Opening,pause.Phase,"A pause sheet presents itself when it reaches a panel");yield return new WaitForSecondsRealtime(.35f);yield return Frames(2);Assert.AreEqual(OverlayPhase.Open,pause.Phase);Assert.AreEqual(1,pause.resolvedStyle.opacity,.01f);}

 // The pause title and its eyebrow decode out of noise over about two seconds and end on the real text at rest.
 [UnityTest] public IEnumerator PauseTitleAndEyebrowRevealThenSettle(){
  yield return Boot(390);var pause=new PauseOverlayView().Into(board);pause.Bind(new PauseViewModel{wave=7});yield return Frames(2);
  var title=pause.Header.TitleLabel;var meta=pause.Header.MetaLabel;Assert.IsTrue(title.ClassListContains(Glitch.TitleClass));Assert.IsTrue(meta.ClassListContains(Glitch.TitleClass),"The eyebrow reveals with its title");
  Assert.AreNotEqual("PAUSED",title.text,"Early in the reveal the title is still noise");yield return new WaitForSecondsRealtime(2.3f);yield return Frames(2);Assert.AreEqual("PAUSED",title.text);Assert.AreEqual("WAVE 07",meta.text);Assert.AreEqual(1f,title.resolvedStyle.opacity,.01f);Assert.AreEqual(0f,title.resolvedStyle.translate.x,.01f,"The title ends at rest");}
}
}
