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
 long shownWave=-1,shownScore=-1;int shownSpeed;bool? shownAuto;
 public HudTop Bind(HudView v){
  view=v;EnableInClassList(ClassName+"--boss",v.boss);waveWord.text=v.boss?"BOSS":"WAVE";
  // Numbers are formatted only when they change: the band is bound every frame.
  if(v.wave!=wantedWave){wantedWave=v.wave;long next=v.wave;if(shownWave<0){shownWave=next;wave.text=next.ToString("00");}else Glitch.Out(wave,()=>{shownWave=next;wave.text=next.ToString("00");Glitch.Slam(wave);if(!Ui.Under(this,SafeAreaRoot.ReducedMotion)){Glitch.Jitter(this);WaveSlammed?.Invoke();}});}if(v.score!=shownScore){shownScore=v.score;VbMotion.Count(score,v.score,Ui.Compact);score.tooltip=Ui.Number(v.score);}
  if(v.speed!=shownSpeed){if(shownSpeed>0)VbMotion.Pop(speed);shownSpeed=v.speed;speed.Text=v.speed==1?"1×":v.speed==2?"2×":v.speed+"×";}
  if(v.auto!=shownAuto){if(shownAuto.HasValue)VbMotion.Pop(auto);shownAuto=v.auto;}
  auto.EnableInClassList(ClassName+"__auto--on",v.auto);auto.Active=v.auto;auto.tooltip=v.auto?"Autoplay on · tap to disable":"Autoplay off · tap to enable";credits.Set(v.credits);
  float[] marks=null;if(v.queue.Length>0){mark[0]=v.queue[0].cost/(float)Mathf.Max(1,v.maxPower);marks=mark;}
  power.Set(v.power,v.maxPower,v.regen,v.hint,v.previewFrom,v.previewTo,marks);focus.Set(v.focus,v.slow);
  return this;
 }
}

/// <summary>Upcoming casts in firing order. With a cancel callback, manual entries are tappable and leave the list when tapped.</summary>
public sealed class QueueList:VisualElement {
 public const string ClassName="vb-queue";
 Action<QueueView> cancel;readonly Label title,hint;readonly VisualElement body;readonly System.Collections.Generic.List<QueueChip> rows=new System.Collections.Generic.List<QueueChip>();
 public QueueList(){AddToClassList(ClassName);VbShape.Attach(this);pickingMode=PickingMode.Ignore;var head=Ui.Box(this,ClassName+"__head");title=Ui.Text(head,"",ClassName+"__title");hint=Ui.Text(head,"",ClassName+"__hint");body=Ui.Box(this);}
 public QueueList Bind(QueueView[] queue,string title="AUTOPLAY QUEUE",string hint="Fires as Power reaches each cost",Action<QueueView> cancel=null){this.cancel=cancel;this.title.text=title+" · "+queue.Length;this.hint.text=hint;
  Keyed.Bind(body,System.Linq.Enumerable.Select(queue,(q,i)=>(view:q,index:i)),x=>string.IsNullOrEmpty(x.view.id)?x.view.actor+":"+x.view.slot+":"+x.view.who+":"+x.view.skill:x.view.id,x=>{var chip=new QueueChip().Classes(ClassName+"__row");chip.Cancelled+=()=>{if(this.cancel!=null){chip.RemoveFromHierarchy();this.cancel(chip.View);}};return chip;},(chip,x)=>{chip.Bind(x.view,x.index+1);chip.EnableInClassList(ClassName+"__row--manual",x.view.manual);});
  rows.Clear();rows.AddRange(body.Query<QueueChip>().ToList());return this;}
 public bool CancelRow(int index){if(cancel==null||index<0||index>=rows.Count||rows[index].parent==null)return false;return rows[index].Cancel();}

}

/// <summary>Boss read-out: name, health over stability, shield and interrupt markers.</summary>
[UxmlElement] public partial class BossBar:VisualElement {
 public const string ClassName="vb-boss";
 readonly Label title,detail;readonly HealthBar health;readonly EffectStrip effects;
 public BossBar(){
  AddToClassList(ClassName);VbShape.Attach(this);pickingMode=PickingMode.Ignore;var head=Ui.Box(this,ClassName+"__head");new VbIcon("marker",14).Classes(ClassName+"__icon").Into(head);
  title=Ui.Text(head,"",ClassName+"__name");detail=Ui.Text(head,"",ClassName+"__detail");health=new HealthBar().Classes(ClassName+"__health").Into(this);effects=new EffectStrip().Classes(ClassName+"__chips").Into(this);
 }
 public BossBar Bind(BossView b){
  title.text=b.name.ToUpperInvariant();detail.text=b.detail;health.Set(b.health,1,0,b.stability,b.stability<=0,true);
  var all=new System.Collections.Generic.List<EffectInstance>(b.effects??Array.Empty<EffectInstance>());
  if(b.shielded&&!all.Exists(e=>e.definition?.id=="boss-protection"))all.Add(new EffectInstance{id="boss:shield",definition=new EffectDefinition{id="shielded",title="Shielded",description=b.shieldDetail,icon="barrier",polarity=EffectPolarity.Benefit},magnitude=b.shieldDetail});
  if(b.interruptible&&!all.Exists(e=>e.definition?.id=="interruptible"))all.Add(new EffectInstance{id="boss:interruptible",definition=new EffectDefinition{id="interruptible",title="Interruptible",description="This action can be interrupted.",icon="warning"}});
  effects.Bind(all);
  return this;
 }
}

/// <summary>Screen header: back, title, a short meta read-out.</summary>
[UxmlElement] public partial class ScreenHeader:VisualElement {
 public const string ClassName="vb-header";
 readonly Label title,meta;
 public event Action Back;
 [UxmlAttribute] public string Title {get=>title.text;set=>title.text=(value??"").ToUpperInvariant();}
 /// <summary>The meta read-out; a change on a shown header swells it once (SAVED, 3 / 4).</summary>
 [UxmlAttribute] public string Meta {get=>meta.text;set{value??="";if(meta.text==value)return;bool change=meta.text.Length>0;meta.text=value;if(change)VbMotion.Pop(meta,.14f);}}
 public ScreenHeader():this("",""){}
 public ScreenHeader(string title,string meta,string backLabel="Back"){
  AddToClassList(ClassName);new IconButtonComposite("back",backLabel,()=>Back?.Invoke()){name="back"}.Classes(ClassName+"__back").Into(this);
  this.title=Ui.Text(this,"",ClassName+"__title");this.meta=Ui.Text(this,"",ClassName+"__meta");Title=title;Meta=meta;
 }
 public ScreenHeader Accent(bool on){meta.EnableInClassList(ClassName+"__meta--accent",on);return this;}
}

}
