using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
namespace Veilbreak.Gui.Tests {
/// <summary>Captures the GUI polish batch (design-system#55) at 320, 390 and 430 units, twice the pixels, as PNGs under <c>VB_GUI_CAPTURE</c> (default <c>Reports/gui-polish</c> in the project) for a person to look at. Explicit: run it with
/// <c>-testFilter GuiPolishCaptureTests</c>. These are headless renders, not editor-observed or device results.</summary>
[Explicit("Writes PNGs for review")]
public sealed class GuiPolishCaptureTests {
 GameObject host;PanelSettings settings;RenderTexture target;VisualElement board;string folder;
 [TearDown] public void Close(){if(host!=null)Object.Destroy(host);if(settings!=null)Object.Destroy(settings);if(target!=null)target.Release();}
 IEnumerator Boot(int width,string group){
#if UNITY_EDITOR
  Close();int height=Mathf.RoundToInt(width*2.164f);target=new RenderTexture(width*2,height*2,0);
  var theme=UnityEditor.AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>("Packages/com.veilbreak.gui/Runtime/Styles/VeilbreakGui.tss");
  settings=ScriptableObject.CreateInstance<PanelSettings>();settings.themeStyleSheet=theme;settings.scaleMode=PanelScaleMode.ConstantPixelSize;settings.scale=2;settings.targetTexture=target;settings.clearColor=true;settings.colorClearValue=VbTokens.Ground;
  host=new GameObject("Polish capture");var document=host.AddComponent<UIDocument>();document.panelSettings=settings;yield return null;
  board=Ui.Box(document.rootVisualElement,"vb-root");board.style.flexGrow=1;board.style.backgroundColor=new Color(.09f,.05f,.14f);if(width<380)board.AddToClassList("vb-compact");VbFluid.Apply(board,width);
  var root=System.Environment.GetEnvironmentVariable("VB_GUI_CAPTURE");folder=Path.Combine(string.IsNullOrEmpty(root)?Path.Combine(Application.dataPath,"..","Reports","gui-polish"):root,width.ToString());Directory.CreateDirectory(folder);
#else
  Assert.Ignore("Needs the editor asset database");yield break;
#endif
 }
 IEnumerator Shot(string name,float wait=.15f){for(int i=0;i<6;i++)yield return null;yield return new WaitForSecondsRealtime(wait);yield return null;yield return null;
  var previous=RenderTexture.active;RenderTexture.active=target;var texture=new Texture2D(target.width,target.height,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,target.width,target.height),0,0);texture.Apply();File.WriteAllBytes(Path.Combine(folder,name+".png"),texture.EncodeToPNG());RenderTexture.active=previous;Object.DestroyImmediate(texture);}
 // A portrait with a bright corner and a diagonal, so a face that overflows the cutout shows.
 static Texture2D Face(Color tone){var t=new Texture2D(150,225,TextureFormat.RGB24,false);for(int y=0;y<225;y++)for(int x=0;x<150;x++){float d=Vector2.Distance(new Vector2(x,y),new Vector2(75,150));var c=Color.Lerp(tone,new Color(.9f,.8f,.7f),d<42?1:0);if(x<30&&y>195)c=Color.white;if(Mathf.Abs(x-(225-y)*.66f)<3)c=Color.black;t.SetPixel(x,y,c);}t.Apply();return t;}
 static EffectInstance Status(string id,float s,EffectPolarity p,string icon)=>new EffectInstance{id=id,definition=new EffectDefinition{id=id,title=id,icon=icon,polarity=p,description="Lasts a while."},remainingSeconds=s,magnitude="−20%"};
 static readonly int[] Widths={320,390,430};

 [UnityTest] public IEnumerator MenuAndPause(){
  foreach(int w in Widths){
   yield return Boot(w,"menu");var page=Ui.Box(board,"vb-page");page.style.flexGrow=1;page.style.justifyContent=Justify.FlexEnd;var menu=new MenuActionsSectionView().Into(page);menu.Bind(new CheckpointView{wave=7,score=18240,standing=3,perks=4},"Squad · Aria · Sol · Aegis · Nyra");yield return Shot("menu");
   yield return Boot(w,"pause");var pause=new PauseOverlayView().Into(board);pause.style.position=Position.Absolute;pause.style.left=pause.style.right=pause.style.top=pause.style.bottom=0;
   pause.Bind(new PauseViewModel{wave=7,score=18240,standing=4,perks=new[]{new PerkView{id="a",name="Overclock",icon="burst",stack=2},new PerkView{id="b",name="Plating",icon="armor"}},seed="698043EE"});yield return Shot("pause-reveal",.7f);yield return Shot("pause",1.6f);}}

 [UnityTest] public IEnumerator BattleCardsStatusesAndDamage(){
  var faces=new[]{Face(new Color(.2f,.5f,.7f)),Face(new Color(.6f,.3f,.3f)),Face(new Color(.3f,.6f,.4f)),Face(new Color(.5f,.4f,.7f))};
  foreach(int w in Widths){
   yield return Boot(w,"battle");var battle=new BattleHudView().Into(board);var ops=new OperativeView[4];
   for(int i=0;i<4;i++)ops[i]=new OperativeView{name=new[]{"Aria","Rook","Aegis","Nyra"}[i],hp=900,maxHp=1000,portrait=faces[i],kit=new[]{VbTokens.KitAria,VbTokens.KitRook,VbTokens.KitAegis,VbTokens.KitNyra}[i],
    skills=new[]{new SkillView{state=SkillState.Cooldown,charged=.6f,cost=12,seconds=4,icon="projectile"},new SkillView{slot="S",state=SkillState.Cooldown,charged=.85f,cost=40,seconds=9,icon="burst"}}};
   ops[0].effects=new[]{Status("slow",3,EffectPolarity.Harm,"slow"),Status("haste",5,EffectPolarity.Benefit,"haste"),Status("exposed",2,EffectPolarity.Harm,"exposed")};ops[1].effects=new[]{Status("barrier",4,EffectPolarity.Benefit,"barrier")};
   battle.Bind(new HudView{wave=7,score=18240,power=70},ops);for(int i=0;i<4;i++)battle.Card(i).AutoDamage=false;yield return Shot("battle");
   battle.Card(2).Damage(1);battle.Card(1).Damage(.4f);yield return Shot("battle-damage",.12f);battle.Card(0).Statuses.Details.Open();yield return Shot("battle-statuses",.5f);}
  foreach(var f in faces)Object.DestroyImmediate(f);}

 [UnityTest] public IEnumerator SquadDrawerAndCarousel(){
  var face=Face(new Color(.2f,.5f,.7f));
  foreach(int w in Widths){
   yield return Boot(w,"squad");var page=Ui.Box(board,"vb-page","vb-page--headed");page.style.flexGrow=1;var view=new SquadView().Into(page);view.style.flexGrow=1;
   var roster=new OperativeView[8];for(int i=0;i<8;i++)roster[i]=new OperativeView{id="o"+i,name=new[]{"Aria","Rook","Aegis","Nyra","Hex","Sol","Mira","Echo"}[i],role="Striker",portrait=face,maxHp=1200+i*40,armor=14+i,
    passive="Momentum Tax",passiveText="Each hit adds one *Taxed* stack; three slow the target.",basic=new SkillView{label="Pulse Shot",slot="B",kind="Basic",icon="projectile",description="A quick shot at the nearest hostile."},
    lore="Raised in the relay stacks, Aria learned to read a signal before it spoke.",skills=new[]{new SkillView{label="Needle Storm",slot="A",kind="Active",cost=40,cooldownSeconds=8,icon="burst",description="Fires a *spread* of needles."},new SkillView{label="Terminal Vector",slot="S",kind="Signature",cost=90,cooldownSeconds=20,icon="beam",description="Locks a line and *erases* what crosses it."}}};
   view.Bind(new SquadViewModel{roster=roster,selected=new[]{0,1,2,-1},focus=0,locked=new string[8]});yield return Shot("squad-drawer",.6f);
   yield return Boot(w,"draft");var draftPage=Ui.Box(board,"vb-page");draftPage.style.flexGrow=1;var draft=new PerkDraftView().Into(draftPage);draft.style.flexGrow=1;var choices=new[]{new PerkView{id="a",name="Overclock",tag="Offense",icon="burst",effect="*+12%* damage while Power is over half",next="+24% damage",stack=1,max=3},new PerkView{id="b",name="Plating",tag="Defense",icon="armor",effect="*+8* armour for every operative",next="+16 armour",stack=0,max=3},new PerkView{id="c",name="Quickdraw",tag="Utility",icon="haste",effect="Breaking stability restores *3 Power*",next="+4 Power",tier=PerkTier.Rare}};
   draft.Bind(new PerkDraftViewModel{wave=7,bonus=700,score=18240,choices=choices,pick=1,squad=new[]{new SquadHealthView{code="AR",hp=900,maxHp=1000},new SquadHealthView{code="RO",hp=500,maxHp=1000},new SquadHealthView{code="AE",hp=1000,maxHp=1000},new SquadHealthView{code="NY",hp=0,maxHp=1000}}});yield return Shot("perk-carousel",.6f);
   draft.Choices.Next();yield return Shot("perk-carousel-gliding",.12f);yield return Shot("perk-carousel-next",.6f);}
  Object.DestroyImmediate(face);}

 [UnityTest] public IEnumerator ActivationAndResults(){
  foreach(int w in Widths){
   yield return Boot(w,"activation");var page=Ui.Box(board,"vb-page");page.style.flexGrow=1;page.style.justifyContent=Justify.Center;var play=new VbButton("PLAY",ButtonVariant.Primary,null,"chevron").Into(page);play.style.marginBottom=12;var scores=new VbButton("HIGH SCORES",ButtonVariant.Secondary,null,"ranked").Into(page);scores.style.marginBottom=12;var abandon=new VbButton("ABANDON",ButtonVariant.Danger,null,"remove").Into(page);
   yield return null;yield return null;VbEdgeFx.Play(play);VbEdgeFx.Play(scores);VbEdgeFx.Play(abandon);yield return Shot("activation",.18f);
   yield return Boot(w,"results");var resultsPage=Ui.Box(board,"vb-page");resultsPage.style.flexGrow=1;var results=new ResultsView().Into(resultsPage);results.style.flexGrow=1;results.Bind(new ResultsViewModel{cause="SQUAD DOWN",wave=7,score=18240,rank=3,bosses=1,distance="1,240 m",simTime="6:32"});results.Deal();yield return Shot("results-reveal",.8f);}}
}
}
