using System;
using UnityEngine.UIElements;
namespace Veilbreak.Gui {
/// <summary>The thumb-zone block of the menu: the checkpoint card when a run can resume, PLAY (the screen's one primary), then high
/// scores and settings. RESUME is secondary and lives inside the checkpoint card; nothing else moves when the card appears.</summary>
[UxmlElement] public partial class MenuActionsSectionView:VisualElement {
 public const string ClassName="vb-menu-actions";
 readonly CardComponent checkpoint;readonly Label line,detail,notice;readonly VbButton play;
 public event Action Play,Resume,Scores,Settings;
 public MenuActionsSectionView(){
  AddToClassList(ClassName);pickingMode=PickingMode.Ignore;
  checkpoint=new CardComponent("CHECKPOINT").Classes("vb-menu__checkpoint").Into(this);line=Ui.Text(checkpoint,"","vb-menu__checkpoint-line");detail=Ui.Text(checkpoint,"","vb-t-caption");
  checkpoint.Actions(new VbButton("RESUME",ButtonVariant.Secondary,null,"play",()=>Resume?.Invoke()){name="resume"});
  play=new VbButton("PLAY",ButtonVariant.Primary,null,"chevron",()=>Play?.Invoke()){name="play"}.Classes("vb-menu__play").Into(this);
  var row=Ui.Box(this,"vb-row","vb-menu__row");new VbButton("HIGH SCORES",ButtonVariant.Secondary,null,null,()=>Scores?.Invoke()){name="scores"}.Into(row);new VbButton("SETTINGS",ButtonVariant.Secondary,null,"settings",()=>Settings?.Invoke()){name="settings"}.Into(row);
  notice=Ui.Text(this,"","vb-t-caption","vb-menu__notice");Bind(null,"",null);
 }
 /// <param name="checkpoint">Null hides the card.</param><param name="squadLine">"Squad · Aria · Sol · Aegis · Nyra" under PLAY.</param>
 public MenuActionsSectionView Bind(CheckpointView checkpoint,string squadLine,string notice=null){
  this.checkpoint.Show(checkpoint!=null);
  if(checkpoint!=null){this.checkpoint.Meta="WAVE "+checkpoint.wave.ToString("00");line.text="Wave "+checkpoint.wave.ToString("00")+" next · "+Ui.Number(checkpoint.score);
   detail.text=checkpoint.standing+" of "+checkpoint.squadSize+" standing · "+checkpoint.perks+(checkpoint.perks==1?" perk":" perks")+(string.IsNullOrEmpty(checkpoint.seed)?"":" · seed "+checkpoint.seed);}
  play.Sub=squadLine??"";this.notice.text=notice??"";this.notice.Show(this.notice.text.Length>0);return this;
 }
}

/// <summary>The title screen: the offline badge and best score, the mark, then the menu actions; EXIT is a ghost, top-right.</summary>
[UxmlElement] public partial class MainMenuView:ScreenView {
 public const string Block="vb-menu";
 readonly Label status,subtitle,bestWave;readonly BadgeComponent best;readonly VbButton exit;
 public MenuActionsSectionView Actions {get;}
 public event Action Play,Resume,Scores,Settings,Exit;
 public MainMenuView():base(Block){
  var top=Ui.Box(this,Block+"__top");status=Ui.Text(top,"","vb-t-badge");best=new BadgeComponent("BEST").Classes(Block+"__best").Into(top);bestWave=Ui.Text(best,"","vb-t-badge","vb-t-ally",Block+"__best-wave");
  var brand=Ui.Box(this,Block+"__brand");new VbMark(240).Into(brand);subtitle=Ui.Text(brand,"",Block+"__arcade");
  Ui.Spacer(this);Actions=new MenuActionsSectionView().Into(this);
  Actions.Play+=()=>Play?.Invoke();Actions.Resume+=()=>Resume?.Invoke();Actions.Scores+=()=>Scores?.Invoke();Actions.Settings+=()=>Settings?.Invoke();
  exit=new VbButton("EXIT",ButtonVariant.Ghost,null,null,()=>Exit?.Invoke()){name="exit"}.Classes(Block+"__exit").Into(this);
  Bind(new MainMenuViewModel());
 }
 public MainMenuView Bind(MainMenuViewModel v){
  status.text=v.status??"";best.Show(v.best>0);best.Value=Ui.Number(v.best);bestWave.text=v.bestWave>0?"WAVE "+v.bestWave:"";subtitle.text=(v.subtitle??"").ToUpperInvariant();subtitle.Show(subtitle.text.Length>0);
  Actions.Bind(v.checkpoint,v.squad.Length>0?"Squad · "+string.Join(" · ",v.squad):"Choose a squad",v.notice);exit.Show(v.canExit);return this;
 }
}
}
