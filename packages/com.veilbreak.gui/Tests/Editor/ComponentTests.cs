using System.Linq;
using NUnit.Framework;
using UnityEngine.UIElements;
namespace Veilbreak.Gui.Tests {
/// <summary>The handbook's atoms, composites, layouts and views: their class contracts and events.</summary>
public sealed class ComponentTests {
 [Test] public void TextStylesAndTonesAreOneClassEach(){var t=new TextComponent("x",TypeStyle.Caption,TextTone.Muted);Assert.IsTrue(t.ClassListContains("vb-t-caption"));Assert.IsTrue(t.ClassListContains("vb-t-muted"));
  t.TypeStyle=TypeStyle.Num;t.Tone=TextTone.Default;Assert.IsFalse(t.ClassListContains("vb-t-caption"));Assert.IsTrue(t.ClassListContains("vb-t-num"));Assert.IsFalse(t.ClassListContains("vb-t-muted"));}
 [Test] public void LabelsUppercaseAndCarryTheRhythm(){var l=new LabelComponent("Roster · all 8",LabelSize.Badge);Assert.AreEqual("ROSTER · ALL 8",l.text);Assert.IsTrue(l.ClassListContains("vb-t-badge"));Assert.IsTrue(l.ClassListContains("vb-label"));}
 [Test] public void BadgeTonesAreModifiers(){var b=new BadgeComponent("best",null,"18,240",BadgeTone.Ally);Assert.IsTrue(b.ClassListContains("vb-badge--ally"));b.Tone=BadgeTone.Inverted;Assert.IsFalse(b.ClassListContains("vb-badge--ally"));Assert.IsTrue(b.ClassListContains("vb-badge--inverted"));
  Assert.IsTrue(b.Query<Label>().ToList().Any(x=>x.text=="BEST"));Assert.IsTrue(b.Query<Label>().ToList().Any(x=>x.text=="18,240"));}
 [Test] public void FrameKindsAreClasses(){var f=new VbFrame(FrameKind.Slim);Assert.IsTrue(f.ClassListContains("vb-frame--slim"));f.Kind=FrameKind.Chipped;Assert.IsFalse(f.ClassListContains("vb-frame--slim"));Assert.IsTrue(f.ClassListContains("vb-frame--chipped"));}
 [Test] public void IconButtonsToggleAndClick(){int n=0;var b=new IconButtonComposite("pause","Pause",()=>n++){Active=true,Quiet=true};Assert.IsTrue(b.ClassListContains("vb-icon-button--active"));Assert.IsTrue(b.ClassListContains("vb-icon-button--quiet"));Assert.AreEqual("Pause",b.tooltip);
  Assert.IsTrue(b.focusable);}
 [Test] public void TileStatesCombine(){var t=new UnitTile().Bind(new OperativeView{name="Aria",role="Striker"},2,true);Assert.IsTrue(t.ClassListContains("vb-tile--in"));Assert.IsTrue(t.ClassListContains("vb-tile--focused"));
  t.Bind(new OperativeView{name="Sol",role="Breaker"},0,false,RarityTier.Gold,"Pull in rifts");Assert.IsTrue(t.ClassListContains("vb-tile--locked"));Assert.IsFalse(t.ClassListContains("vb-tile--in"));Assert.IsTrue(t.Query<Label>().ToList().Any(x=>x.text=="PULL IN RIFTS"));}
 [Test] public void EmptySlotsAreDashedAndNamed(){var s=new UnitSlot().Bind(null,3);Assert.IsTrue(s.ClassListContains("vb-slot--empty"));Assert.IsTrue(s.Query<Label>().ToList().Any(x=>x.text=="SLOT 3"));s.Bind(new OperativeView{name="Aria",code="AR"},3);Assert.IsFalse(s.ClassListContains("vb-slot--empty"));}
 [Test] public void CardsTakeChildrenInTheBody(){var c=new CardComponent("Checkpoint","WAVE 07"){Selected=true};var l=new Label("x");c.Add(l);Assert.AreSame(c.Body,l.hierarchy.parent);Assert.AreSame(c,l.parent,"the logical parent is the card");Assert.IsTrue(c.ClassListContains("vb-card--selected"));Assert.IsTrue(c.Query<Label>().ToList().Any(x=>x.text=="CHECKPOINT"));}
 [Test] public void FieldsHaveLabelInputAndHelp(){var input=new VbSegmented(new[]{("A",""),("B","")},0);var f=new FieldComposite("Camera",input,"Pinch to zoom");Assert.AreSame(f,input.parent.parent);Assert.IsTrue(f.Query<Label>().ToList().Any(x=>x.text=="CAMERA"));}
 [Test] public void StacksSnapToTheRhythm(){Assert.AreEqual(4,StackLayout.Snap(3));Assert.AreEqual(12,StackLayout.Snap(10));var s=new StackLayout(16);Assert.IsTrue(s.ClassListContains("vb-stack--16"));var a=new VisualElement();var b=new VisualElement();s.Add(a);s.Add(b);s.MarkLast();Assert.IsTrue(b.ClassListContains("vb-stack__last"));Assert.IsFalse(a.ClassListContains("vb-stack__last"));}
 [Test] public void BoardHasThreeZones(){var b=new BoardLayout();Assert.AreEqual(3,b.childCount);Assert.IsTrue(b.World.ClassListContains("vb-board__world"));}
 [Test] public void MenuShowsTheCheckpointOnlyWhenThereIsOne(){var m=new MainMenuView().Bind(new MainMenuViewModel{best=18240,bestWave=12});var card=m.Q(className:"vb-menu__checkpoint");Assert.AreEqual(DisplayStyle.None,card.style.display.value);
  m.Bind(new MainMenuViewModel{checkpoint=new CheckpointView{wave=7,score=9420}});Assert.AreEqual(DisplayStyle.Flex,card.style.display.value);}
 [Test] public void SquadStartIsBlockedUntilFour(){var ops=Enumerable.Range(0,5).Select(i=>new OperativeView{name="Op"+i,code="O"+i}).ToArray();var v=new SquadView().Bind(new SquadViewModel{roster=ops,selected=new[]{0,1,-1,-1}});
  var start=v.Q<VbButton>("start");Assert.IsFalse(start.enabledSelf);Assert.AreEqual("Pick 2 more",start.Blocked);v.Bind(new SquadViewModel{roster=ops,selected=new[]{0,1,2,3}});Assert.IsTrue(start.enabledSelf);}
 [Test] public void ConfirmIsOneModalLayerWithBothActions(){var root=new VisualElement();var nav=new Navigator(root);nav.Open("pause");var d=ConfirmDialogView.Push(nav,"END THIS RUN?","Recorded.",("ABANDON","remove",()=>{}),("KEEP PLAYING","chevron"));
  Assert.AreEqual(1,nav.Modals.Count);Assert.AreEqual(ButtonVariant.Danger,d.Confirm.Variant);Assert.AreEqual("KEEP PLAYING",d.Cancel.Title);Assert.IsTrue(nav.Back());Assert.AreEqual(0,nav.Modals.Count);}
 [Test] public void HighScoresListOlderRulesAfterALabel(){var v=new HighScoresView().Bind(new[]{new ScoreView{rank=1,score=10}},new[]{new ScoreView{rank=1,score=5}});Assert.AreEqual(2,v.Query<ScoreRow>().ToList().Count);Assert.IsNotNull(v.Q(className:"vb-scores__older"));}
 [Test] public void BattleBoardHidesTheBossBarWithoutABoss(){var b=new BattleHudView().Bind(new HudView(),new[]{new OperativeView{name="Aria"}});Assert.AreEqual(DisplayStyle.None,b.Boss.style.display.value);Assert.AreEqual(DisplayStyle.None,b.Card(1).style.display.value);
  b.Bind(new HudView(),new OperativeView[0],new BossView{name="Warden"});Assert.AreEqual(DisplayStyle.Flex,b.Boss.style.display.value);}
 [Test] public void DrawersOpenDismissAndCloseQuietly(){var d=new DrawerOverlay{Title="Details"};int dismissed=0;d.Dismissed+=()=>dismissed++;Assert.AreEqual(DisplayStyle.None,d.style.display.value,"Closed until opened");
  d.Open();Assert.IsTrue(d.IsOpen);Assert.IsTrue(d.ClassListContains(DrawerOverlay.OpenClass));Assert.IsTrue(d.Dismiss());Assert.AreEqual(1,dismissed);Assert.IsFalse(d.IsOpen);Assert.IsFalse(d.Dismiss(),"Dismiss twice does nothing");
  d.Open();d.Close();Assert.AreEqual(1,dismissed,"Close is not a dismiss");d.Dismissible=false;d.Open();Assert.IsFalse(d.Dismiss());var l=new Label("x");d.Add(l);Assert.AreSame(d.Body,l.hierarchy.parent);}
 [Test] public void InlineDrawersLetTheViewTakeInput(){var d=new DrawerOverlay{Backdrop=false};Assert.AreEqual(PickingMode.Ignore,d.pickingMode);Assert.IsTrue(d.ClassListContains("vb-drawer--inline"));Assert.AreEqual(PickingMode.Position,d.Sheet.pickingMode);}
 [Test] public void PushedDrawersAreModalLayersThatBackCloses(){var root=new VisualElement();var nav=new Navigator(root);nav.Open("squad");int closed=0;var d=DrawerOverlay.Push(nav,"Aria",()=>closed++);
  Assert.AreEqual(1,nav.Modals.Count);Assert.IsTrue(d.IsOpen);Assert.IsTrue(nav.Back());Assert.AreEqual(0,nav.Modals.Count);Assert.AreEqual(1,closed);}
 [Test] public void SquadDetailsOpenInADrawer(){var ops=Enumerable.Range(0,4).Select(i=>new OperativeView{name="Op"+i,code="O"+i}).ToArray();var v=new SquadView();bool closed=false;v.Closed+=()=>closed=true;
  v.Bind(new SquadViewModel{roster=ops,focus=-1});Assert.IsFalse(v.DetailsDrawer.IsOpen);v.Bind(new SquadViewModel{roster=ops,focus=2});Assert.IsTrue(v.DetailsDrawer.IsOpen);Assert.IsNotNull(v.DetailsDrawer.Q<UnitDetail>());
  v.DetailsDrawer.Dismiss();Assert.IsTrue(closed);}
 [Test] public void PauseIsACentredOverlayWhoseResumeIsTheOnlyWayBack(){var p=new PauseOverlayView().Bind(new PauseViewModel{wave=7,score=9420,standing=3});bool resumed=false;p.Resume+=()=>resumed=true;
  Assert.IsNull(p.Q<DrawerOverlay>(),"The pause menu is not a drawer");Assert.IsTrue(p.ClassListContains("vb-view--headed"));Assert.AreEqual("WAVE 07",p.Header.Meta);
  // ds#33: RESUME is the one way back; the header carries no second arrow.
  Assert.IsFalse(p.Header.ShowBack);Assert.AreEqual(DisplayStyle.None,p.Header.Q<IconButtonComposite>("back").style.display.value);
  Assert.IsFalse(p.Query<Label>().ToList().Exists(l=>l.text.Contains("frozen")),"no tap-hint caption");Assert.AreEqual("",p.Panel.Q<VbButton>("resume").Sub,"RESUME has no sub line");
  Assert.IsTrue(p.Panel.parent.ClassListContains("vb-pause__center"));Assert.IsNotNull(p.Panel.Q<VbButton>("resume"));Assert.IsNotNull(p.Panel.Q<VbButton>("exit"));Assert.IsFalse(resumed);}
 [Test] public void DrawersOpenAtTheirContentHeightAndSnapByHeight(){var d=new DrawerOverlay().Open();Assert.AreEqual(DrawerSnap.Fit,d.Snap);Assert.IsFalse(d.ClassListContains(DrawerOverlay.FullClass));Assert.IsFalse(d.ClassListContains(DrawerOverlay.HalfClass));
  d.Snap=DrawerSnap.Full;Assert.IsTrue(d.ClassListContains(DrawerOverlay.FullClass));d.Close();d.Open();Assert.AreEqual(DrawerSnap.Fit,d.Snap,"Every open starts fitted");
  d.Close();d.Open(DrawerSnap.Half);Assert.AreEqual(DrawerSnap.Half,d.Snap,"Half is opt-in");Assert.IsTrue(d.ClassListContains(DrawerOverlay.HalfClass));
  Assert.IsNull(DrawerOverlay.SnapFor(150,844),"Below a quarter closes");Assert.AreEqual(DrawerSnap.Half,DrawerOverlay.SnapFor(500,844));Assert.AreEqual(DrawerSnap.Full,DrawerOverlay.SnapFor(700,844));
  Assert.IsNull(DrawerOverlay.SnapFor(200,844,DrawerSnap.Fit,DrawerSnap.Fit,380),"A fitted sheet pulled below a quarter closes");
  Assert.AreEqual(DrawerSnap.Fit,DrawerOverlay.SnapFor(390,844,DrawerSnap.Fit,DrawerSnap.Fit,380),"Within the slop it settles back on its content");
  Assert.AreEqual(DrawerSnap.Full,DrawerOverlay.SnapFor(600,844,DrawerSnap.Fit,DrawerSnap.Fit,380),"Pulled up it goes full");
  Assert.AreEqual(DrawerSnap.Fit,DrawerOverlay.SnapFor(500,844,DrawerSnap.Full,DrawerSnap.Fit,0),"Pulled down from full it returns to its content height");Assert.AreEqual(DrawerSnap.Full,DrawerOverlay.SnapFor(700,844,DrawerSnap.Full,DrawerSnap.Fit,0));}
}
}
