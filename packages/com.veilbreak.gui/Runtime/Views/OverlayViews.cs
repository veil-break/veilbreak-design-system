using System;
using System.Linq;
using UnityEngine.UIElements;
namespace Veilbreak.Gui {
/// <summary>The run, frozen: a <see cref="ScreenHeader"/> whose back square resumes (the same control as Settings and High Scores),
/// then one overlay panel centred on the screen: wave, score and who stands, the active perks (tap for details), the seed line,
/// RESUME (the one primary), settings, ABANDON (danger, confirms) and EXIT TO MENU (ghost). Back = Resume.</summary>
[UxmlElement] public partial class PauseOverlayView:OverlayView {
 public const string Block="vb-pause";
 readonly ScreenHeader header;readonly StatReadout wave,score,standing;readonly VisualElement perks,chips,panel;readonly LabelComponent perksTitle;readonly Label detail;
 public event Action Resume,Settings,Perks,Abandon,Exit;
 public ScreenHeader Header=>header;
 /// <summary>The centred overlay panel (a slim solid <see cref="Dialog"/> sheet).</summary>
 public VisualElement Panel=>panel;
 public PauseOverlayView(){AddToClassList("vb-view");AddToClassList("vb-view--headed");AddToClassList(Block);
  header=new ScreenHeader("PAUSED","","Resume").Into(this);header.Back+=()=>Resume?.Invoke();
  var center=Ui.Box(this,Block+"__center");Ui.Text(center,"Combat, focus and world travel are frozen","vb-t-caption",Block+"__caption");
  panel=Dialog.Sheet(center,null,null,"vb-t-caption",Block+"__panel");
  var stats=Ui.Box(panel,"vb-row",Block+"__stats");wave=new StatReadout("WAVE","01").Into(stats);score=new StatReadout("SCORE","0").Into(stats);standing=new StatReadout("STANDING","4","/4").Into(stats);
  perks=Ui.Frame(panel,FrameKind.Slim,"vb-frame--raised",Block+"__perks").Clickable(()=>Perks?.Invoke());perks.name="perks";perks.tooltip="Active perks";
  var head=Ui.Box(perks,"vb-row","vb-row--between");perksTitle=new LabelComponent("").Into(head);var more=Ui.Box(head,"vb-row");Ui.Text(more,"DETAILS","vb-t-badge","vb-t-ally");new VbIcon("chevron",16).Classes(Block+"__more").Into(more);
  chips=Ui.Box(perks,"vb-row","vb-row--wrap",Block+"__chips");detail=Ui.Text(panel,"","vb-t-caption");
  new VbButton("RESUME",ButtonVariant.Primary,"Returns to selected speed","chevron",()=>Resume?.Invoke()){name="resume"}.Into(panel);
  var row=Ui.Box(panel,"vb-row",Block+"__row");new VbButton("SETTINGS",ButtonVariant.Secondary,null,"settings",()=>Settings?.Invoke()){name="settings"}.Classes("vb-half",Block+"__button").Into(row);
  new VbButton("ABANDON",ButtonVariant.Danger,null,"remove",()=>Abandon?.Invoke()){name="abandon"}.Classes("vb-half",Block+"__button",Block+"__abandon").Into(row);
  new VbButton("EXIT TO MENU",ButtonVariant.Ghost,null,"back",()=>Exit?.Invoke()){name="exit"}.Classes(Block+"__exit").Into(panel);
 }
 public PauseOverlayView Bind(PauseViewModel v){
  header.Meta="WAVE "+v.wave.ToString("00");wave.Set(v.wave.ToString("00"));score.Set(Ui.Number(v.score));standing.Set(v.standing.ToString(),"/"+v.squadSize);
  perksTitle.text="Active perks · "+v.perks.Length;VbMotion.Show(perks,v.perks.Length>0);Keyed.Bind(chips,v.perks,p=>p.id,_=>new BadgeComponent().Classes(Block+"__chip"),(chip,p)=>{chip.Text=p.name;chip.Icon=p.icon;chip.Value=p.stack>1?"×"+p.stack:"";});
  var line=string.Join(" · ",new[]{string.IsNullOrEmpty(v.seed)?null:"Seed "+v.seed,v.detail}.Where(s=>!string.IsNullOrEmpty(s)));detail.text=line;detail.Show(line.Length>0);return this;
 }
}

/// <summary>One shape for every confirmation: the question, its consequence, the destructive action and the way back, in one <see cref="ActionRow"/>
/// (equal widths, stacked on a narrow sheet).
/// <see cref="Push"/> puts it on a modal layer (blurred page under it, focus returns on close); both actions close it first.</summary>
public sealed class ConfirmDialogView:OverlayView {
 public const string ClassName="vb-confirm";
 public VbButton Confirm {get;}
 public VbButton Cancel {get;}
 public ConfirmDialogView(string title,string body,(string label,string icon,string sub) confirm,(string label,string icon) cancel,Action onConfirm,Action onCancel,ButtonVariant confirmVariant=ButtonVariant.Danger){
  AddToClassList(ClassName);pickingMode=PickingMode.Ignore;var sheet=Dialog.Sheet(this,title,body);
  Confirm=new VbButton(confirm.label,confirmVariant,confirm.sub,confirm.icon,onConfirm){name="confirm"};Cancel=new VbButton(cancel.label,ButtonVariant.Secondary,null,cancel.icon,onCancel){name="cancel"};
  Ui.Actions(sheet,Confirm,Cancel).AddToClassList(ClassName+"__actions");
 }
 /// <summary>Pushes the dialog over the navigator's page. <paramref name="cancel"/>'s action is optional; closing always pops the layer.</summary>
 public static ConfirmDialogView Push(Navigator nav,string title,string body,(string label,string icon,Action action) confirm,(string label,string icon) cancel,string confirmSub=null,Action onCancel=null,ButtonVariant confirmVariant=ButtonVariant.Danger){
  var layer=nav.Push();return new ConfirmDialogView(title,body,(confirm.label,confirm.icon,confirmSub),cancel,()=>{nav.Modals.Pop();confirm.action?.Invoke();},()=>{nav.Modals.Pop();onCancel?.Invoke();},confirmVariant).Into(layer);
 }
}

/// <summary>The first-run guide: numbered steps (move, aim, cancel, autoplay, speed) and READY.</summary>
public sealed class GuideDialogView:OverlayView {
 public const string ClassName="vb-guide";
 public GuideDialogView(string title,string[] steps,Action ready){
  AddToClassList(ClassName);pickingMode=PickingMode.Ignore;var sheet=Dialog.Sheet(this,title,null);var list=Ui.Stack(sheet,8,ClassName+"__steps");
  for(int i=0;i<steps.Length;i++){var row=Ui.Box(list,"vb-row",ClassName+"__step");new BadgeComponent(Ui.Count(i+1),null,null,BadgeTone.Ally).Classes(ClassName+"__index").Into(row);Ui.Text(row,steps[i],"vb-t-body",ClassName+"__text");}
  Ui.Actions(sheet,new VbButton("READY",ButtonVariant.Primary,null,"check",ready){name="ready"}).AddToClassList(ClassName+"__actions");
 }
 /// <summary>Pushes the guide on a modal layer; READY closes it, then runs <paramref name="ready"/>.</summary>
 public static GuideDialogView Push(Navigator nav,string title,string[] steps,Action ready){var layer=nav.Push();return new GuideDialogView(title,steps,()=>{nav.Modals.Pop();ready?.Invoke();}).Into(layer);}
 /// <summary>The arcade's default steps.</summary>
 public static readonly string[] DefaultSteps={
  "Drag an operative to move.","Drag A or S onto the battlefield to aim; release over the cards to cancel.",
  "Auto controls tactics. Tap AUTO to take over.","2× speeds combat; aiming stays slow while focus lasts.","Stop enemies before they pass the rear line."};
}
}
