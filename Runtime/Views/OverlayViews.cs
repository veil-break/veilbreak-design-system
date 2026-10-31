using System;
using System.Linq;
using UnityEngine.UIElements;
namespace Veilbreak.Gui {
/// <summary>The run, frozen: PAUSED, then a bottom drawer with wave, score and who stands, the active perks (tap for details), the
/// seed line, RESUME (the one primary), settings and ABANDON (danger, confirms). EXIT is a ghost in the top-left corner.
/// Back = Resume, and so is dragging the drawer down.</summary>
[UxmlElement] public partial class PauseOverlayView:ScreenView {
 public const string Block="vb-pause";
 readonly StatReadout wave,score,standing;readonly VisualElement perks,chips;readonly LabelComponent perksTitle;readonly Label detail,caption;readonly DrawerOverlay drawer;
 public event Action Resume,Settings,Perks,Abandon,Exit;
 /// <summary>The docked drawer holding the run's sheet.</summary>
 public DrawerOverlay Drawer=>drawer;
 public PauseOverlayView():base(Block){
  var title=Ui.Box(this,Block+"__title");Ui.Text(title,"PAUSED","vb-t-display");caption=Ui.Text(title,"Combat, focus and world travel are frozen","vb-t-caption");
  // Docked, no backdrop (the screen layer already dims the run): the corner EXIT stays reachable; dragging the sheet down resumes.
  drawer=new DrawerOverlay{Backdrop=false,CloseButton=false}.Classes(Block+"__drawer").Into(this);drawer.Dismissed+=()=>Resume?.Invoke();
  var stats=Ui.Box(drawer,"vb-row",Block+"__stats");wave=new StatReadout("WAVE","01").Into(stats);score=new StatReadout("SCORE","0").Into(stats);standing=new StatReadout("STANDING","4","/4").Into(stats);
  perks=Ui.Frame(drawer,FrameKind.Slim,"vb-frame--raised",Block+"__perks").Clickable(()=>Perks?.Invoke());perks.name="perks";perks.tooltip="Active perks";
  var head=Ui.Box(perks,"vb-row","vb-row--between");perksTitle=new LabelComponent("").Into(head);var more=Ui.Box(head,"vb-row");Ui.Text(more,"DETAILS","vb-t-badge","vb-t-ally");new VbIcon("chevron",16).Classes(Block+"__more").Into(more);
  chips=Ui.Box(perks,"vb-row","vb-row--wrap",Block+"__chips");detail=Ui.Text(drawer,"","vb-t-caption");
  drawer.Actions(new VbButton("RESUME",ButtonVariant.Primary,"Returns to selected speed","chevron",()=>Resume?.Invoke()){name="resume"},
   new VbButton("SETTINGS",ButtonVariant.Secondary,null,"settings",()=>Settings?.Invoke()){name="settings"}.Classes(Block+"__button"),
   new VbButton("ABANDON RUN",ButtonVariant.Danger,null,"remove",()=>Abandon?.Invoke()){name="abandon"}.Classes(Block+"__button"));
  new VbButton("EXIT",ButtonVariant.Ghost,null,"back",()=>Exit?.Invoke()){name="exit"}.Classes(Block+"__exit").Into(this);
  drawer.Open();
 }
 public PauseOverlayView Bind(PauseViewModel v){
  wave.Set(v.wave.ToString("00"));score.Set(Ui.Number(v.score));standing.Set(v.standing.ToString(),"/"+v.squadSize);
  perksTitle.text="Active perks · "+v.perks.Length;perks.Show(v.perks.Length>0);chips.Clear();
  foreach(var p in v.perks)new BadgeComponent(p.name+(p.stack>1?" ×"+p.stack:""),p.icon).Classes(Block+"__chip").Into(chips);
  var line=string.Join(" · ",new[]{string.IsNullOrEmpty(v.seed)?null:"Seed "+v.seed,v.detail}.Where(s=>!string.IsNullOrEmpty(s)));detail.text=line;detail.Show(line.Length>0);
  if(!drawer.IsOpen)drawer.Open();return this;
 }
}

/// <summary>One shape for every confirmation: the question, its consequence, the destructive action and the way back.
/// <see cref="Push"/> puts it on a modal layer (blurred page under it, focus returns on close); both actions close it first.</summary>
public sealed class ConfirmDialogView:VisualElement {
 public const string ClassName="vb-confirm";
 public VbButton Confirm {get;}
 public VbButton Cancel {get;}
 public ConfirmDialogView(string title,string body,(string label,string icon,string sub) confirm,(string label,string icon) cancel,Action onConfirm,Action onCancel,ButtonVariant confirmVariant=ButtonVariant.Danger){
  AddToClassList(ClassName);pickingMode=PickingMode.Ignore;var sheet=Dialog.Sheet(this,title,body);
  Confirm=new VbButton(confirm.label,confirmVariant,confirm.sub,confirm.icon,onConfirm){name="confirm"}.Into(sheet);Cancel=new VbButton(cancel.label,ButtonVariant.Secondary,null,cancel.icon,onCancel){name="cancel"}.Into(sheet);
 }
 /// <summary>Pushes the dialog over the navigator's page. <paramref name="cancel"/>'s action is optional; closing always pops the layer.</summary>
 public static ConfirmDialogView Push(Navigator nav,string title,string body,(string label,string icon,Action action) confirm,(string label,string icon) cancel,string confirmSub=null,Action onCancel=null,ButtonVariant confirmVariant=ButtonVariant.Danger){
  var layer=nav.Push();return new ConfirmDialogView(title,body,(confirm.label,confirm.icon,confirmSub),cancel,()=>{nav.Modals.Pop();confirm.action?.Invoke();},()=>{nav.Modals.Pop();onCancel?.Invoke();},confirmVariant).Into(layer);
 }
}

/// <summary>The first-run guide: numbered steps (move, aim, cancel, autoplay, speed) and READY.</summary>
public sealed class GuideDialogView:VisualElement {
 public const string ClassName="vb-guide";
 public GuideDialogView(string title,string[] steps,Action ready){
  AddToClassList(ClassName);pickingMode=PickingMode.Ignore;var sheet=Dialog.Sheet(this,title,null);var list=Ui.Stack(sheet,8,ClassName+"__steps");
  for(int i=0;i<steps.Length;i++){var row=Ui.Box(list,"vb-row",ClassName+"__step");new BadgeComponent(Ui.Count(i+1),null,null,BadgeTone.Ally).Classes(ClassName+"__index").Into(row);Ui.Text(row,steps[i],"vb-t-body",ClassName+"__text");}
  new VbButton("READY",ButtonVariant.Primary,null,"check",ready){name="ready"}.Into(sheet);
 }
 /// <summary>Pushes the guide on a modal layer; READY closes it, then runs <paramref name="ready"/>.</summary>
 public static GuideDialogView Push(Navigator nav,string title,string[] steps,Action ready){var layer=nav.Push();return new GuideDialogView(title,steps,()=>{nav.Modals.Pop();ready?.Invoke();}).Into(layer);}
 /// <summary>The arcade's default steps.</summary>
 public static readonly string[] DefaultSteps={
  "Drag an operative to move.","Drag A or S onto the battlefield to aim; release over the cards to cancel.",
  "Auto controls tactics. Tap AUTO to take over.","2× speeds combat; aiming stays slow while focus lasts.","Stop enemies before they pass the rear line."};
}
}
