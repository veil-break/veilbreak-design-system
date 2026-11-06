using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;
namespace Veilbreak.Gui.Tests {
/// <summary>The GUI polish batch (design-system#55, gui 0.9.0) that needs no live panel: the three type variants, the fluid scale, the single-row action row, content clipping, the carousel's wrap, the status row, the unit sheet's rows, the
/// glitch decode and the overlay recipes. What needs a layout or a clock is in <c>PolishFitTests</c> and <c>PolishMotionTests</c>.</summary>
public sealed class PolishTests {
 static string Sheet(string name)=>File.ReadAllText(PackageFiles.Path("Runtime/Styles/"+name));
 static float Token(string sheet,string name){var m=Regex.Match(Sheet(sheet),"--vb-"+name+":\\s*(-?[\\d.]+)px");Assert.IsTrue(m.Success,name+" in "+sheet);return float.Parse(m.Groups[1].Value,System.Globalization.CultureInfo.InvariantCulture);}
 // The declarations of the rule whose selector list contains selector (a class), or null.
 static string Rule(string sheet,string selector){foreach(Match m in Regex.Matches(Sheet(sheet),"([^{}]+)\\{([^{}]*)\\}")){var selectors=Regex.Replace(m.Groups[1].Value,"/\\*.*?\\*/","",RegexOptions.Singleline).Split(',').Select(s=>s.Trim());if(selectors.Contains(selector))return m.Groups[2].Value;}return null;}

 // ── type variants ──────────────────────────────────────────────────────
 [Test] public void LabelIsAboutEightyEightPercentOfBodyWithFifteenHundredthsTracking(){
  float body=Token("Tokens.uss","font-size-body"),label=Token("Tokens.uss","font-size-label"),small=Token("Tokens.uss","font-size-label-sm"),large=Token("Tokens.uss","font-size-label-lg");
  Assert.AreEqual(.88f,label/body,.01f,"label is 0.88× body");Assert.Less(small,label);Assert.Greater(large,label);
  foreach(var cls in new[]{".vb-t-label",".vb-t-label-sm",".vb-t-label-lg",".vb-button__label",".vb-badge__text",".vb-chip-row__name",".vb-t-badge"}){var rule=Rule("Gui.uss",cls);Assert.IsNotNull(rule,cls+" is in a label rule");StringAssert.Contains("letter-spacing: 15px",rule,cls+" is tracked 0.15em");}}
 [Test] public void HeadingIsBoldTightAndAboutNinetyPercentLine(){
  foreach(var size in new[]{"heading-sm","heading","heading-lg"}){float px=Token("Tokens.uss","font-size-"+size),line=Token("Tokens.uss","line-height-"+size);Assert.AreEqual(.9f,line/px,.03f,size+" has a 0.9 line");}
  var rule=Rule("Gui.uss",".vb-t-heading");StringAssert.Contains("display-bold",rule);StringAssert.Contains("letter-spacing: -2px",rule,"negative tracking");
  Assert.IsNotNull(Rule("Gui.uss",".vb-t-display"),"The display class is a heading");StringAssert.Contains("heading-lg",Rule("Gui.uss",".vb-t-display"),"…of the large size");}
 [Test] public void BodyIsProseInTheSecondaryColour(){var rule=Rule("Gui.uss",".vb-t-body");StringAssert.Contains("--vb-font-size-body",rule);StringAssert.Contains("text-2",rule,"slightly dimmed");}
 [Test] public void ThePrimaryButtonTextMatchesTheOtherButtons(){
  var primary=Rule("Gui.uss",".vb-button--primary .vb-button__label");Assert.IsTrue(primary==null||!primary.Contains("font-size")&&!primary.Contains("font-definition"),"The primary label sets no size or face of its own");
  StringAssert.Contains("--vb-font-size-label)",Rule("Gui.uss",".vb-button__label"),"Every button label is the label variant");}
 [Test] public void EveryFontSizeIsATokenSoTheFluidScaleMovesIt(){
  foreach(var sheet in new[]{"Gui.uss","Views.uss","Layout.uss","World.uss"})foreach(Match m in Regex.Matches(Regex.Replace(Sheet(sheet),"/\\*.*?\\*/","",RegexOptions.Singleline),"font-size:\\s*([^;}]+)"))StringAssert.StartsWith("var(--vb-",m.Groups[1].Value.Trim(),sheet+" has a literal font-size: "+m.Value);}

 // ── fluid scale ────────────────────────────────────────────────────────
 [Test] public void TheFluidFactorGrowsWithThePanelWidth(){
  Assert.AreEqual(.92f,VbFluid.Factor(320),1e-3f);Assert.AreEqual(1f,VbFluid.Factor(390),1e-3f);Assert.AreEqual(1.06f,VbFluid.Factor(430),1e-3f);
  Assert.AreEqual(VbFluid.Factor(320),VbFluid.Factor(200),"Below the first step the smallest factor holds");Assert.AreEqual(VbFluid.Factor(600),VbFluid.Factor(900),"Above the last step the largest holds");
  float last=0;foreach(int w in VbFluid.Steps){float f=VbFluid.Factor(w);Assert.GreaterOrEqual(f,last);last=f;}
  Assert.AreEqual("vb-fluid-390",VbFluid.ClassFor(395));Assert.AreEqual("vb-fluid-320",VbFluid.ClassFor(320));Assert.AreEqual("vb-fluid-430",VbFluid.ClassFor(444));}
 [Test] public void EveryStepHasAClassThatScalesTextGuttersAndMargins(){
  var fluid=Sheet("Fluid.uss");float body=Token("Tokens.uss","font-size-body"),gutter=Token("Tokens.uss","gutter"),pad=Token("Tokens.uss","pad");
  foreach(int step in VbFluid.Steps){var m=Regex.Match(fluid,"\\.vb-fluid-"+step+"\\s*\\{([^}]*)\\}");Assert.IsTrue(m.Success,"A class for "+step);float f=VbFluid.Factor(step);
   foreach(var (name,baseline) in new[]{("font-size-body",body),("font-size-label",Token("Tokens.uss","font-size-label")),("gutter",gutter),("pad",pad),("fs-13",13f)}){
    var v=Regex.Match(m.Groups[1].Value,"--vb-"+name+":\\s*(-?[\\d.]+)px");Assert.IsTrue(v.Success,name+" at "+step);Assert.AreEqual(baseline*f,float.Parse(v.Groups[1].Value,System.Globalization.CultureInfo.InvariantCulture),.02f,name+" at "+step);}}}
 [Test] public void ApplySwapsTheStepClassAndIgnoresNoWidth(){
  var root=new VisualElement();Assert.IsTrue(VbFluid.Apply(root,320));Assert.IsTrue(root.ClassListContains("vb-fluid-320"));Assert.IsFalse(VbFluid.Apply(root,325),"Same step");
  Assert.IsTrue(VbFluid.Apply(root,430));Assert.IsFalse(root.ClassListContains("vb-fluid-320"));Assert.IsTrue(root.ClassListContains("vb-fluid-430"));
  Assert.IsFalse(VbFluid.Apply(root,float.NaN));Assert.IsFalse(VbFluid.Apply(root,0));Assert.IsTrue(root.ClassListContains("vb-fluid-430"));
  var child=new VisualElement();root.Add(child);var popup=new VisualElement();VbFluid.Copy(child,popup);Assert.IsTrue(popup.ClassListContains("vb-fluid-430"),"A popover copies its anchor's step");}
 [Test] public void TheThemeLoadsTheFluidSheetAfterTheTokens(){var tss=Sheet("VeilbreakGui.tss");Assert.Less(tss.IndexOf("Tokens.uss"),tss.IndexOf("Fluid.uss"));Assert.Less(tss.IndexOf("Fluid.uss"),tss.IndexOf("Gui.uss"));}

 // ── single-row action row, icon-only button ────────────────────────────
 [Test] public void ASingleRowNeverStacksAndAnIconOnlyButtonDescribesItself(){
  var cog=new VbButton("Settings",ButtonVariant.Secondary,null,"settings"){IconOnly=true};Assert.IsTrue(cog.ClassListContains("vb-button--icon"));Assert.AreEqual("Settings",cog.tooltip,"The title is the accessible label");
  var exit=new VbButton("EXIT TO MENU",ButtonVariant.Secondary,null,"back");var row=Ui.Actions(new VisualElement(),exit,new VbButton("ABANDON",ButtonVariant.Danger),cog);Assert.AreNotEqual(DisplayStyle.None,exit.Q<VbIcon>().style.display.value,"A button shows its glyph in a loose row");row.Single=true;Assert.IsTrue(row.ClassListContains(ActionRow.SingleClass));
  row.style.width=200;Assert.IsTrue(ActionRow.Stacks(200,3,ActionRow.DefaultMinItemWidth),"The same row would stack");row.Arrange();Assert.IsFalse(row.Stacked,"A single row never does");Assert.IsTrue(exit.Compact);Assert.AreEqual(DisplayStyle.None,exit.Q<VbIcon>().style.display.value,"The labelled buttons of a single row give their glyph up for the label's width");Assert.AreNotEqual(DisplayStyle.None,cog.Q<VbIcon>().style.display.value,"…the cog keeps its own");
  row.Single=false;Assert.IsFalse(exit.Compact);Assert.AreNotEqual(DisplayStyle.None,exit.Q<VbIcon>().style.display.value);row.Single=true;
  cog.IconOnly=false;Assert.AreEqual("",cog.tooltip,"Without the icon-only mode the tooltip goes");}
 [Test] public void TheMenuAndPauseSecondariesAreOneRowWithACog(){
  var menu=new MenuActionsSectionView();var row=menu.Q<ActionRow>();Assert.IsTrue(row.Single);Assert.AreEqual(2,row.childCount);Assert.IsTrue(((VbButton)row[1]).IconOnly);Assert.AreEqual("cog",((VbButton)row[1]).Icon,"A real cog (the settings glyph is a sun)");Assert.AreEqual("HIGH SCORES",((VbButton)row[0]).Title);
  var pause=new PauseOverlayView();var rows=pause.Query<ActionRow>(className:"vb-pause__row").ToList();Assert.AreEqual(1,rows.Count);Assert.IsTrue(rows[0].Single);
  CollectionAssert.AreEqual(new[]{"exit","abandon","settings"},rows[0].Children().Select(c=>c.name).ToArray(),"Leave, the destructive one, then the cog");Assert.IsTrue(((VbButton)rows[0][2]).IconOnly);
  Assert.AreEqual(1,pause.Query<VbButton>().ToList().Count(b=>b.Variant==ButtonVariant.Primary),"RESUME stays the one primary");}

 // ── content clipping ───────────────────────────────────────────────────
 static float Area(IReadOnlyList<Vector2> p){float a=0;for(int i=0;i<p.Count;i++){var q=p[(i+1)%p.Count];a+=p[i].x*q.y-q.x*p[i].y;}return Mathf.Abs(a)/2;}
 [Test] public void AShapeClipsARectangleToItsChamfers(){
  var shape=VbShape.Outline(100,60,10,0);var whole=new List<Vector2>{new Vector2(0,0),new Vector2(100,0),new Vector2(100,60),new Vector2(0,60)};
  var clipped=VbClip.Intersect(whole,shape);Assert.AreEqual(6000-100,Area(clipped),.5f,"Two corner triangles of 10×10 / 2 are gone");
  foreach(var p in clipped)Assert.IsTrue(p.x+p.y>=10-.01f&&(100-p.x)+(60-p.y)>=10-.01f,"No point inside the top-left or bottom-right cut: "+p);
  var inside=new List<Vector2>{new Vector2(20,20),new Vector2(60,20),new Vector2(60,40),new Vector2(20,40)};Assert.AreEqual(Area(inside),Area(VbClip.Intersect(inside,shape)),1e-3f,"A rectangle in the shape is unchanged");
  var outside=new List<Vector2>{new Vector2(200,0),new Vector2(300,0),new Vector2(300,50),new Vector2(200,50)};Assert.AreEqual(0,VbClip.Intersect(outside,shape).Count,"Nothing outside survives");
  var left=VbShape.Outline(100,60,10,0,VbShape.Left);Assert.AreEqual(6000-100,Area(VbClip.Intersect(whole,left)),.5f,"The Left side cuts the top-left and bottom-left corners");}
 [Test] public void APictureFitsItsBoxWithoutCropping(){
  var fit=VbClipImage.Fit(150,225,100,100);Assert.AreEqual(150,fit.width,1e-3f);Assert.AreEqual(150,fit.height,1e-3f);Assert.AreEqual(37.5f,fit.y,1e-3f,"Centred");
  var tall=VbClipImage.Fit(100,100,50,200);Assert.AreEqual(25,tall.width,1e-3f);Assert.AreEqual(100,tall.height,1e-3f);}
 [Test] public void PortraitsAndFillsAreClipElements(){
  var card=new UnitCard();Assert.IsInstanceOf<VbClipImage>(card.Q(className:"vb-operative__portrait"));var skill=card.Skill(0);Assert.IsInstanceOf<VbClipFill>(skill.Q(className:"vb-skill__charge"));Assert.IsInstanceOf<VbClipFill>(skill.Q(className:"vb-skill__sweep"));
  Assert.IsTrue(card.Skill(1).ClassListContains("vb-operative__skill-end"),"The last skill takes the card's own cut");Assert.IsInstanceOf<VbClipImage>(new UnitTile().Q(className:"vb-tile__portrait"));
  var tex=new Texture2D(2,2);Ui.Image(card.Q<VbClipImage>(),tex);Assert.AreSame(tex,card.Q<VbClipImage>().Image,"Ui.Image reaches a clip image");Object.DestroyImmediate(tex);}

 [Test] public void TheEdgeHeadsRunAlongTheOutlineAndWrap(){
  var pts=new[]{new Vector2(0,0),new Vector2(10,0),new Vector2(10,10),new Vector2(0,10)};var lengths=new[]{10f,10f,10f,10f};
  void At(Vector2 expected,float s,string why=null)=>Assert.AreEqual(0,Vector2.Distance(expected,VbEdgeFx.PointAt(pts,lengths,40,s)),1e-3f,why);
  At(new Vector2(5,0),5);At(new Vector2(10,5),15);At(new Vector2(5,10),25);
  At(new Vector2(0,5),-5,"A negative arc length wraps round");At(new Vector2(5,0),45,"…and so does one past the end");
  var chamfer=VbShape.Outline(100,60,10,.75f);var l=new float[chamfer.Length];float per=0;for(int i=0;i<l.Length;i++){l[i]=Vector2.Distance(chamfer[i],chamfer[(i+1)%l.Length]);per+=l[i];}
  Assert.AreEqual(0,Vector2.Distance(chamfer[0],VbEdgeFx.PointAt(chamfer,l,per,0)),1e-3f);Assert.AreEqual(0,Vector2.Distance(chamfer[3],VbEdgeFx.PointAt(chamfer,l,per,l[0]+l[1]+l[2])),1e-3f,"The chamfer's own corners are on the path");}

 // ── carousel ───────────────────────────────────────────────────────────
 [Test] public void CarouselOffsetsWrapTheShortestWay(){
  Assert.AreEqual(0,Carousel.Offset(0,0,5));Assert.AreEqual(1,Carousel.Offset(1,0,5));Assert.AreEqual(-1,Carousel.Offset(4,0,5),"The last card sits just before the first");Assert.AreEqual(2,Carousel.Offset(2,0,5));Assert.AreEqual(-2,Carousel.Offset(3,0,5));
  Assert.AreEqual(-.5f,Carousel.Offset(0,.5f,3),1e-4f);Assert.AreEqual(0,Carousel.Offset(7,7,9));Assert.AreEqual(0,Carousel.Offset(3,0,1),"One card has no neighbours");
  Assert.AreEqual(1,Carousel.Shortest(0,1,3));Assert.AreEqual(-1,Carousel.Shortest(0,2,3),"Back from the first to the last is one step");Assert.AreEqual(1,Carousel.Shortest(2,0,3),"Forward from the last to the first is one step");Assert.AreEqual(0,Carousel.Shortest(1,1,3));}
 [Test] public void ThePoseOfTheActiveCardIsStillAndNeighboursTuckUnderItTilted(){
  var active=Carousel.Pose(0,390,1);Assert.AreEqual(0,active.x);Assert.AreEqual(0,active.rotation);Assert.AreEqual(1,active.scale);Assert.AreEqual(1,active.opacity);
  var right=Carousel.Pose(1,390,1);var left=Carousel.Pose(-1,390,1);float cw=390*Carousel.ActiveWidth;
  Assert.AreEqual(-left.x,right.x,1e-3f,"Mirrored");Assert.AreEqual(-left.rotation,right.rotation,1e-3f);Assert.Greater(right.rotation,0,"Slightly rotated");Assert.Less(right.rotation,15);
  Assert.Less(right.x-cw/2,cw/2,"The neighbour's inner edge lies beneath the active card");Assert.Greater(right.x+cw/2,cw/2,"and its outer edge peeks out");Assert.Less(right.scale,1);Assert.Less(right.opacity,1);
  Assert.AreEqual(0,Carousel.Pose(2,390,1).opacity,1e-4f,"A card past the shown depth is gone");Assert.Greater(Carousel.Pose(2,390,2).opacity,0);}
 [Test] public void TheCarouselIsEndlessAndOnlyAUsersMoveRaisesChanged(){
  var carousel=new Carousel();var values=new[]{"a","b","c"};var changes=new List<int>();carousel.Changed+=changes.Add;
  carousel.Bind(values,v=>v,()=>new Label(),(card,v,i)=>((Label)card).text=v,0);Assert.AreEqual(3,carousel.Count);Assert.AreEqual(0,carousel.Index);Assert.AreEqual(1,carousel.Depth);Assert.IsEmpty(changes,"Binding raises nothing");
  carousel.Previous();Assert.AreEqual(2,carousel.Index,"Before the first is the last");carousel.Next();Assert.AreEqual(0,carousel.Index,"After the last is the first");carousel.Next();carousel.Next();carousel.Next();Assert.AreEqual(0,carousel.Index);
  CollectionAssert.AreEqual(new[]{2,0,1,2,0},changes);carousel.Select(1);Assert.AreEqual(1,carousel.Index);Assert.AreEqual(5,changes.Count,"Select is the host's own move: no event");Assert.AreEqual("b",((Label)carousel.Active).text);
  var first=carousel.Cards[0];carousel.Bind(new[]{"a","b","c","d"},v=>v,()=>new Label(),(card,v,i)=>((Label)card).text=v,1);Assert.AreSame(first,carousel.Cards[0],"A card is made once per key");Assert.AreEqual(4,carousel.Count);Assert.AreEqual(2,carousel.Depth);
  carousel.Bind(new[]{"b"},v=>v,()=>new Label(),(card,v,i)=>((Label)card).text=v,0);Assert.AreEqual(1,carousel.Count);Assert.IsEmpty(carousel.Query<Label>().ToList().Where(l=>l.text!="b"),"Cards whose key went are removed");
  Assert.Throws<System.ArgumentException>(()=>carousel.Bind(new[]{"x","x"},v=>v,()=>new Label(),(card,v,i)=>{},0),"Duplicate keys throw");}

 // ── status row ─────────────────────────────────────────────────────────
 static EffectInstance Status(string id,float? seconds,EffectPolarity p=EffectPolarity.Harm,int stacks=1)=>new EffectInstance{id=id,stacks=stacks,definition=new EffectDefinition{id=id,title=id,icon="slow",polarity=p},remainingSeconds=seconds};
 [Test] public void TheStatusRowIsBadgeSquaresForTimedStatusesOnly(){
  var row=new StatusRow();Assert.AreEqual(DisplayStyle.None,row.style.display.value,"Nothing to show hides the row");
  row.Bind(new[]{Status("slow",3),Status("passive",null),Status("haste",5,EffectPolarity.Benefit,stacks:3)});Assert.AreEqual(2,row.Timed.Count,"A passive is not a status");
  var shown=row.Squares.Where(s=>s.style.display!=DisplayStyle.None).ToList();Assert.AreEqual(2,shown.Count);Assert.IsInstanceOf<BadgeComponent>(shown[0]);Assert.AreEqual("slow",shown[0].Icon);Assert.AreEqual("3",shown[1].Value,"The stack count is in the corner");
  Assert.IsTrue(shown[0].ClassListContains("vb-badge--enemy"));Assert.IsTrue(shown[1].ClassListContains("vb-badge--ally"),"Harm and benefit tone the square");
  var many=Enumerable.Range(0,9).Select(i=>Status("s"+i,i+1f)).ToArray();row.Bind(many);Assert.AreEqual(9,row.Timed.Count);var visible=row.Squares.Where(s=>s.style.display!=DisplayStyle.None).ToList();Assert.AreEqual(StatusRow.Max,visible.Count);
  Assert.AreEqual("+"+(9-(StatusRow.Max-1)),visible.Last().Value,"The last square counts what does not fit");Assert.IsNotNull(row.Details,"Hover or tap opens the list");
  var card=new UnitCard();Assert.IsInstanceOf<StatusRow>(card.Statuses);}

 // ── unit sheet ─────────────────────────────────────────────────────────
 [Test] public void TheUnitSheetListsPassiveBasicAndSkillsThenTheLore(){
  var v=new OperativeView{name="Aria",passive="Momentum Tax",passiveText="Hits *tax* the target.",basic=new SkillView{label="Pulse Shot",slot="B",kind="Basic",icon="projectile"},lore="Raised in the relay stacks.",skills={}};
  v.skills=new[]{new SkillView{label="Needle Storm",slot="A",kind="Active",cost=40,cooldownSeconds=8},new SkillView{label="Terminal Vector",slot="S",kind="Signature",cost=90,cooldownSeconds=20}};
  CollectionAssert.AreEqual(new[]{"P","B","A","S"},UnitDetail.Rows(v).Select(s=>s.slot).ToArray(),"Passive, basic, then the unit's skills");
  var sheet=new UnitDetail().Bind(v,0,false);Assert.AreEqual(4,sheet.Query<SkillSummary>().ToList().Count);var lore=sheet.Q<Label>(className:"vb-details__lore");Assert.AreEqual("Raised in the relay stacks.",lore.text);
  Assert.Less(sheet.IndexOf(sheet.Q(className:"vb-details__skills")),sheet.IndexOf(sheet.Q(className:"vb-details__lore-box")),"The lore follows the skills");
  var basicRow=sheet.Query<SkillSummary>().ToList()[1];Assert.IsFalse(basicRow.Query<Label>(className:"vb-details__skill-power").ToList().Any(l=>l.style.display!=DisplayStyle.None),"A basic attack with no cost shows none");
  v.basic=null;v.lore="";v.passive="";sheet.Bind(v,0,false);Assert.AreEqual(2,sheet.Query<SkillSummary>().ToList().Count,"A unit without a passive or a basic attack lists the skills only");Assert.AreEqual(DisplayStyle.None,sheet.Q(className:"vb-details__lore-box").style.display.value);}

 // ── glitch decode, recipes ─────────────────────────────────────────────
 [Test] public void TheDecodeKeepsSpacesAndLengthAndEndsOnTheText(){
  const string title="RUN OVER";Assert.AreEqual(title,Glitch.Scramble(title,1,5));Assert.AreEqual(title.Length,Glitch.Scramble(title,.3f,2).Length);
  var half=Glitch.Scramble(title,.5f,3);Assert.AreEqual("RUN ",half.Substring(0,4),"The first half is real");Assert.AreEqual(' ',half[3]);Assert.AreEqual(Glitch.Scramble(title,.3f,2),Glitch.Scramble(title,.3f,2),"The same step gives the same noise");
  Assert.AreNotEqual(Glitch.Scramble(title,0,1),Glitch.Scramble(title,0,2),"A new step re-rolls the noise");Assert.AreEqual("",Glitch.Scramble("",.5f,0));Assert.AreEqual(2000,MotionRecipes.Reveal,"About two seconds");}
 [Test] public void EveryOverlayHasAShortTransitionThroughOneRecipeFamily(){
  Assert.LessOrEqual(MotionRecipes.OverlayIn.milliseconds,250);Assert.LessOrEqual(MotionRecipes.OverlayOut.milliseconds,MotionRecipes.OverlayIn.milliseconds);Assert.LessOrEqual(MotionRecipes.PopoverIn.milliseconds,MotionRecipes.OverlayIn.milliseconds);
  Assert.LessOrEqual(MotionRecipes.DrawerIn.milliseconds,350);Assert.LessOrEqual(MotionRecipes.DrawerOut.milliseconds,MotionRecipes.DrawerIn.milliseconds);Assert.AreEqual(MotionRecipes.OverlayIn.milliseconds,new DialogSurface().Entrance.milliseconds);
  Assert.AreEqual(MotionRecipes.PopoverIn.milliseconds,PopoverEntrance().milliseconds);Assert.AreEqual(MotionRecipes.DrawerIn.milliseconds,new DrawerOverlay().Entrance.milliseconds);Assert.AreEqual(MotionRecipes.OverlayIn.milliseconds,new PauseOverlayView().Entrance.milliseconds);}
 static MotionRecipe PopoverEntrance(){var anchor=new VisualElement();Popover.Attach(anchor,()=>new Label("x"));var type=typeof(Popover).GetNestedType("Popup",System.Reflection.BindingFlags.NonPublic);return ((OverlayView)System.Activator.CreateInstance(type)).Entrance;}
}
}
