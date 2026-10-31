using System;
using UnityEngine;
using UnityEngine.UIElements;
namespace Veilbreak.Gui {
/// <summary>
/// Translucent portrait HUD band: wave/score, autoplay toggle, pause, then shared Power and focus.
/// Hovering, focusing or tapping the resources panel shows the queue; tapping a manual entry raises <see cref="QueueCancel"/>.
/// </summary>
[UxmlElement] public partial class HudTop:VisualElement {
 public const string ClassName="vb-hud";
 readonly Label waveWord,score;readonly GlitchText wave;readonly CreditsChip credits;long wantedWave=-1;public event Action WaveSlammed; readonly IconButtonComposite speed; public bool ShowFocus {get=>focus.resolvedStyle.display!=DisplayStyle.None;set=>focus.Show(value);}readonly IconButtonComposite auto;readonly VisualElement resources;readonly PowerStrip power;readonly FocusMeter focus;readonly float[] mark=new float[1];HudView view;
 public Popover Queue {get;}
 public event Action AutoClicked,PauseClicked,SpeedClicked;
 public event Action<QueueView> QueueCancel;
 public HudTop(){
  AddToClassList(ClassName);pickingMode=PickingMode.Ignore;var row=Ui.Box(this,ClassName+"__row");
  var pause=new IconButtonComposite("pause","Pause",()=>PauseClicked?.Invoke()){Quiet=true,name="pause"}.Classes(ClassName+"__pause").Into(row);
  auto=new IconButtonComposite("automation","Autoplay",()=>AutoClicked?.Invoke()){Quiet=true,name="auto"}.Classes(ClassName+"__auto").Into(row);
  var stats=Ui.Box(row,ClassName+"__stats");var w=Ui.Box(stats,ClassName+"__stat");waveWord=Ui.Text(w,"WAVE",ClassName+"__key");wave=new GlitchText("01").Classes(ClassName+"__wave").Into(w);wave.name="wave-number";
  var s=Ui.Box(stats,ClassName+"__stat");Ui.Text(s,"SCORE",ClassName+"__key");score=Ui.Text(s,"0",ClassName+"__score");
  speed=new IconButtonComposite("","Toggle simulation speed",()=>SpeedClicked?.Invoke()){Text="1×",name="speed"}.Classes(ClassName+"__speed").Into(row);speed.TextLabel.AddToClassList(ClassName+"__auto-state");
  resources=VbShape.Attach(Ui.Box(this,ClassName+"__resources"));resources.focusable=true;resources.pickingMode=PickingMode.Position;
  var resourceRow=Ui.Box(resources,ClassName+"__resource-row");power=new PowerStrip().Into(resourceRow);credits=new CreditsChip().Into(resourceRow);focus=new FocusMeter().Into(resources);
  Queue=Popover.Attach(resources,()=>view!=null&&view.queue.Length>0?new QueueList().Bind(view.queue,view.queueTitle,view.queueHint,q=>QueueCancel?.Invoke(q)):null);
 }
 long shownWave=-1,shownScore=-1;
 public HudTop Bind(HudView v){
  view=v;EnableInClassList(ClassName+"--boss",v.boss);waveWord.text=v.boss?"BOSS":"WAVE";
  // Numbers are formatted only when they change: the band is bound every frame.
  if(v.wave!=wantedWave){wantedWave=v.wave;long next=v.wave;if(shownWave<0){shownWave=next;wave.text=next.ToString("00");}else Glitch.Out(wave,()=>{shownWave=next;wave.text=next.ToString("00");Glitch.Slam(wave);if(!Ui.Under(this,SafeAreaRoot.ReducedMotion)){Glitch.Jitter(this);WaveSlammed?.Invoke();}});}if(v.score!=shownScore){shownScore=v.score;score.text=Ui.Compact(v.score);score.tooltip=Ui.Number(v.score);}
  speed.Text=v.speed==1?"1×":v.speed==2?"2×":v.speed+"×";
  auto.EnableInClassList(ClassName+"__auto--on",v.auto);auto.Active=v.auto;auto.tooltip=v.auto?"Autoplay on · tap to disable":"Autoplay off · tap to enable";credits.Set(v.credits);
  float[] marks=null;if(v.queue.Length>0){mark[0]=v.queue[0].cost/(float)Mathf.Max(1,v.maxPower);marks=mark;}
  power.Set(v.power,v.maxPower,v.regen,v.hint,v.previewFrom,v.previewTo,marks);focus.Set(v.focus,v.slow);
  return this;
 }
}

/// <summary>Upcoming casts in firing order. With a cancel callback, manual entries are tappable and leave the list when tapped.</summary>
public sealed class QueueList:VisualElement {
 public const string ClassName="vb-queue";
 Action<QueueView> cancel;readonly System.Collections.Generic.List<(VisualElement row,QueueView view)> rows=new System.Collections.Generic.List<(VisualElement,QueueView)>();
 public QueueList(){AddToClassList(ClassName);VbShape.Attach(this);pickingMode=PickingMode.Ignore;}
 public QueueList Bind(QueueView[] queue,string title="AUTOPLAY QUEUE",string hint="Fires as Power reaches each cost",Action<QueueView> cancel=null){
  Clear();rows.Clear();this.cancel=cancel;var head=Ui.Box(this,ClassName+"__head");Ui.Text(head,title+" · "+queue.Length,ClassName+"__title");Ui.Text(head,hint,ClassName+"__hint");
  for(int i=0;i<queue.Length;i++){var q=queue[i];var row=VbShape.Attach(Ui.Box(this,ClassName+"__row",i==0?ClassName+"__row--next":null));
   Ui.Text(row,q.next?"›":(i+1).ToString(),ClassName+"__index");var t=Ui.Box(row,ClassName+"__text");Ui.Text(t,q.skill.ToUpperInvariant(),ClassName+"__skill");Ui.Text(t,q.who,ClassName+"__who");
   Ui.Text(row,q.cost.ToString(),ClassName+"__cost");Ui.Text(row,q.eta,ClassName+"__eta");
   rows.Add((row,q));if(q.manual&&cancel!=null){row.AddToClassList(ClassName+"__row--manual");row.tooltip="Cancel queued cast";new VbIcon("cancel",14).Classes(ClassName+"__cancel").Into(row);int index=i;row.Clickable(()=>CancelRow(index));}}
  return this;
 }
 /// <summary>Cancels the manual entry shown at <paramref name="index"/> (what a tap does); autoplay estimates cannot be cancelled.</summary>
 public bool CancelRow(int index){if(cancel==null||index<0||index>=rows.Count||!rows[index].view.manual||rows[index].row.parent==null)return false;rows[index].row.RemoveFromHierarchy();cancel(rows[index].view);return true;}
}

/// <summary>Boss read-out: name, health over stability, shield and interrupt markers.</summary>
[UxmlElement] public partial class BossBar:VisualElement {
 public const string ClassName="vb-boss";
 readonly Label title,detail;readonly HealthBar health;readonly VisualElement chips;string chipKey;
 public BossBar(){
  AddToClassList(ClassName);VbShape.Attach(this);pickingMode=PickingMode.Ignore;var head=Ui.Box(this,ClassName+"__head");new VbIcon("marker",14).Classes(ClassName+"__icon").Into(head);
  title=Ui.Text(head,"",ClassName+"__name");detail=Ui.Text(head,"",ClassName+"__detail");health=new HealthBar().Classes(ClassName+"__health").Into(this);chips=Ui.Box(this,ClassName+"__chips");
 }
 public BossBar Bind(BossView b){
  title.text=b.name.ToUpperInvariant();detail.text=b.detail;health.Set(b.health,1,0,b.stability,b.stability<=0,true);
  var key=b.shielded+b.shieldDetail+b.interruptible;if(key!=chipKey){chipKey=key;chips.Clear();if(b.shielded)new StatusChip(StatusKind.Barrier,"Shielded",b.shieldDetail).Into(chips);if(b.interruptible)new StatusChip(StatusKind.Interrupted,"Interruptible").Into(chips);chips.Show(b.shielded||b.interruptible);}
  return this;
 }
}

/// <summary>Screen header: back, title, a short meta read-out.</summary>
[UxmlElement] public partial class ScreenHeader:VisualElement {
 public const string ClassName="vb-header";
 readonly Label title,meta;
 public event Action Back;
 [UxmlAttribute] public string Title {get=>title.text;set=>title.text=(value??"").ToUpperInvariant();}
 [UxmlAttribute] public string Meta {get=>meta.text;set=>meta.text=value??"";}
 public ScreenHeader():this("",""){}
 public ScreenHeader(string title,string meta,string backLabel="Back"){
  AddToClassList(ClassName);new IconButtonComposite("back",backLabel,()=>Back?.Invoke()){name="back"}.Classes(ClassName+"__back").Into(this);
  this.title=Ui.Text(this,"",ClassName+"__title");this.meta=Ui.Text(this,"",ClassName+"__meta");Title=title;Meta=meta;
 }
 public ScreenHeader Accent(bool on){meta.EnableInClassList(ClassName+"__meta--accent",on);return this;}
}

}
