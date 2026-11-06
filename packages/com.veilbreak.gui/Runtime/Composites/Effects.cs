using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
namespace Veilbreak.Gui {
/// <summary>Shared detail surface; updates in place while its effect changes or expires. It prints only what is known: the title, the description (emphasis marks render,
/// <see cref="Markup"/>), the source, the magnitude and the time left; nothing stands in for a missing value, and a plain active effect carries no state line.</summary>
public sealed class EffectDetails:DetailView {
 public const string TitleClass="vb-effect-detail__title";
 readonly Label title,description,source,value,time,state;
 public EffectDetails(){AddToClassList("vb-effect-detail");title=Ui.Text(this,"",TitleClass);description=Ui.Text(this,"","vb-t-body");source=Ui.Text(this,"","vb-t-caption");value=Ui.Text(this,"","vb-t-label");time=Ui.Text(this,"","vb-t-caption");state=Ui.Text(this,"","vb-t-badge");}
 public EffectDetails Bind(EffectInstance e){var d=e.definition??EffectDefinition.Unknown(e.id);title.text=d.title.ToUpperInvariant();description.text=Markup.Rich(d.description);description.Show(description.text.Length>0);
  source.text=string.IsNullOrEmpty(e.source)?"":"Source · "+e.source;source.Show(source.text.Length>0);
  value.text=e.magnitude+(e.stacks>1?" · ×"+e.stacks:"");value.Show(value.text.Length>0);
  time.text=e.remainingSeconds.HasValue?Ui.Seconds(e.remainingSeconds.Value)+" remaining"+(e.totalSeconds.HasValue?" / "+Ui.Seconds(e.totalSeconds.Value):""):"";time.Show(time.text.Length>0);
  state.text=e.activation==EffectActivation.Active?"":e.activation.ToString().ToUpperInvariant();state.Show(state.text.Length>0);return this;}
}
/// <summary>44-unit inspectable badge. Hover, focus, touch and keyboard use the same details.</summary>
public sealed class EffectBadge:BadgeComponent {
 public EffectInstance View {get;private set;}public Popover Details {get;}
 EffectDetails detail;
 public EffectBadge(){AddToClassList("vb-effect-badge");focusable=true;pickingMode=PickingMode.Position;
  Details=Popover.Attach(this,()=>detail=new EffectDetails().Bind(View),PopoverPlacement.Above);
  RegisterCallback<KeyDownEvent>(e=>{if(e.keyCode==UnityEngine.KeyCode.Space||e.keyCode==UnityEngine.KeyCode.Return){if(Details.IsOpen)Details.Close();else Details.Open();e.StopPropagation();}});
 }
 public EffectBadge Bind(EffectInstance e){View=e;var d=e.definition??EffectDefinition.Unknown(e.id);Icon=d.icon;Text="";Value=e.stacks>1?"×"+e.stacks:"";Tone=d.polarity==EffectPolarity.Harm?BadgeTone.Enemy:d.polarity==EffectPolarity.Benefit?BadgeTone.Ally:BadgeTone.Neutral;
  Ui.State(this,"vb-effect",e.activation);tooltip=d.title+" · "+e.activation;detail?.Bind(e);return this;}
}
public sealed class EffectList:ViewBase {
 readonly KeyedList<EffectInstance,EffectRow> rows;
 public EffectList(){AddToClassList("vb-effect-list");rows=new KeyedList<EffectInstance,EffectRow>(this,e=>e.id,_=>new EffectRow(),(r,e)=>r.Bind(e));}
 public EffectList Bind(IEnumerable<EffectInstance> effects){var current=new List<EffectInstance>(effects);foreach(var row in rows.Elements)if(row.Badge.Details.IsOpen&&!current.Exists(e=>e.id==row.Badge.View.id))current.Add(row.Badge.View.Expired());rows.Bind(current);return this;}
 sealed class EffectRow:ViewBase {public EffectBadge Badge=>badge;readonly EffectBadge badge;readonly Label title,detail;public EffectRow(){AddToClassList("vb-effect-row");badge=new EffectBadge().Into(this);var text=Ui.Box(this);title=Ui.Text(text,"","vb-t-label");detail=Ui.Text(text,"","vb-t-caption");}public void Bind(EffectInstance e){badge.Bind(e);title.text=(e.definition??EffectDefinition.Unknown(e.id)).title;detail.text=e.remainingSeconds.HasValue?(e.magnitude.Length>0?e.magnitude+" · ":"")+Ui.Seconds(e.remainingSeconds.Value):e.magnitude+(e.activation==EffectActivation.Active||e.magnitude.Length==0?"":" · "+e.activation);detail.Show(detail.text.Length>0);}}
}
/// <summary>Up to <see cref="Max"/> timed statuses as small icons on a card's portrait (harm in enemy colour, benefit in ally colour); the whole strip is one target that opens
/// the list of them (<see cref="Details"/>). Passives, perks and other effects without a time are not statuses and never show here, so a unit with none shows nothing.
/// Binding takes an array so a card bound every frame makes no garbage.</summary>
public sealed class StatusPips:ViewBase {
 public const string ClassName="vb-pips";public const int Max=3;
 readonly VbIcon[] icons=new VbIcon[Max];readonly List<EffectInstance> timed=new List<EffectInstance>();EffectList list;int shownCount=-1;
 public Popover Details {get;}
 /// <summary>The statuses shown (all of the timed ones, in the order given); the icons show the first <see cref="Max"/>.</summary>
 public IReadOnlyList<EffectInstance> Timed=>timed;
 public StatusPips(){AddToClassList(ClassName);focusable=true;pickingMode=PickingMode.Position;
  for(int i=0;i<Max;i++)icons[i]=new VbIcon("",12).Classes(ClassName+"__icon").Into(this);
  Details=Popover.Attach(this,()=>timed.Count==0?null:list=new EffectList().Bind(timed),PopoverPlacement.Above);
  RegisterCallback<KeyDownEvent>(e=>{if(e.keyCode==UnityEngine.KeyCode.Space||e.keyCode==UnityEngine.KeyCode.Return){if(Details.IsOpen)Details.Close();else Details.Open();e.StopPropagation();}});
  Bind(null);
 }
 public StatusPips Bind(EffectInstance[] effects){timed.Clear();if(effects!=null)for(int i=0;i<effects.Length;i++)if(effects[i]!=null&&effects[i].Timed)timed.Add(effects[i]);
  for(int i=0;i<Max;i++){bool on=i<timed.Count;icons[i].Show(on);if(on){var d=timed[i].definition;icons[i].Icon=d?.icon??"info";Ui.State(icons[i],ClassName+"__icon",d?.polarity??EffectPolarity.Neutral);}}
  this.Show(timed.Count>0);pickingMode=timed.Count>0?PickingMode.Position:PickingMode.Ignore;if(timed.Count!=shownCount){shownCount=timed.Count;tooltip=shownCount>0?"Statuses · "+shownCount:"";}list?.Bind(timed);return this;}
}
/// <summary>One compact badge and an inspectable summary. Overflow contains every effect with no whitelist or truncation.</summary>
public sealed class EffectStrip:ViewBase {
 readonly VisualElement badges;readonly VbButton more;readonly KeyedList<EffectInstance,EffectBadge> items;readonly List<EffectInstance> all=new List<EffectInstance>();EffectList list;
 public bool Compact {get;set;}
 public Popover Overflow {get;}
 public EffectStrip(){AddToClassList("vb-effect-strip");badges=Ui.Box(this,"vb-effect-strip__badges");items=new KeyedList<EffectInstance,EffectBadge>(badges,e=>e.id,_=>new EffectBadge(),(b,e)=>b.Bind(e));
  more=new VbButton("",ButtonVariant.Ghost){name="all-effects"}.Classes("vb-effect-strip__more").Into(this);Overflow=Popover.Attach(more,()=>list=new EffectList().Bind(all),PopoverPlacement.Above);
 }
 public EffectStrip Bind(IEnumerable<EffectInstance> effects){all.Clear();if(effects!=null)all.AddRange(effects);list?.Bind(all);
  var visible=new List<EffectInstance>();if(all.Count>0)visible.Add(all[0]);
  // Keep an expired inspected badge mounted until the player closes its explanation.
  foreach(var badge in items.Elements)if(badge.Details.IsOpen&&!visible.Exists(e=>e.id==badge.View.id)){var old=all.Find(e=>e.id==badge.View.id);if(old==null){old=badge.View.Expired();}visible.Add(old);}
  items.Bind(visible,true);badges.Show(!Compact);more.Title=Compact?all.Count.ToString():"+"+Math.Max(0,all.Count-1);more.Icon=Compact&&all.Count>0?all[0].definition.icon:"";more.tooltip="All effects · "+all.Count;more.Show(all.Count>(Compact?0:1)||Overflow.IsOpen);VbMotion.Show(this,all.Count>0||visible.Count>0||Overflow.IsOpen,MotionKind.Rise);return this;}
}

/// <summary>One status as a small square built on the badge: its glyph alone (harm in enemy colour, benefit in ally colour, anything else neutral) and, past one stack, the stack count in the corner. Never colour alone: the glyph says which status it is, and
/// the row's list gives the word.</summary>
public sealed class StatusSquare:BadgeComponent {
 public const string SquareClass="vb-status-square";
 public StatusSquare(){AddToClassList(SquareClass);}
 public EffectInstance View {get;private set;}
 public StatusSquare Bind(EffectInstance e){View=e;var d=e.definition??EffectDefinition.Unknown(e.id);Icon=string.IsNullOrEmpty(d.icon)?"info":d.icon;Text="";Set(e.stacks>1?Ui.Count(e.stacks):"");
  Tone=d.polarity==EffectPolarity.Harm?BadgeTone.Enemy:d.polarity==EffectPolarity.Benefit?BadgeTone.Ally:BadgeTone.Neutral;return this;}
 /// <summary>The "+n" square that stands for statuses past the row's maximum.</summary>
 public StatusSquare Overflow(int hidden){View=null;Icon=null;Text="";Set("+"+Ui.Count(hidden));Tone=BadgeTone.Neutral;return this;}
}
/// <summary>The timed statuses of a unit as a row of tiny squares (about 20 × 20, <see cref="StatusSquare"/>, built on <see cref="BadgeComponent"/>), at most <see cref="Max"/> of them and a "+n" square past that. The whole row is one target, 44 units tall
/// through padding the layout does not take back; hover, keyboard focus or a tap opens one vertical list of every status (<see cref="EffectList"/>) in a <see cref="Popover"/> (<see cref="Details"/>). Passives, perks and effects without a time are not statuses and never
/// show, so a unit with none shows nothing. A unit card puts it above itself (<see cref="UnitCard.Statuses"/>). Binding takes an array and reuses its squares, so a card bound every frame makes no garbage.</summary>
public sealed class StatusRow:ViewBase {
 public const string ClassName="vb-status-row";public const int Max=6;
 readonly StatusSquare[] squares=new StatusSquare[Max];readonly List<EffectInstance> timed=new List<EffectInstance>();EffectList list;int shownCount=-1;
 public Popover Details {get;}
 /// <summary>Every timed status bound, in the order given (the squares show the first <see cref="Max"/>).</summary>
 public IReadOnlyList<EffectInstance> Timed=>timed;
 /// <summary>The squares, shown or not; the last one is the "+n" square when there is overflow.</summary>
 public IReadOnlyList<StatusSquare> Squares=>squares;
 public StatusRow(){AddToClassList(ClassName);focusable=true;pickingMode=PickingMode.Position;
  for(int i=0;i<Max;i++)squares[i]=new StatusSquare().Into(this);
  Details=Popover.Attach(this,()=>timed.Count==0?null:list=new EffectList().Bind(timed),PopoverPlacement.Above);
  RegisterCallback<KeyDownEvent>(e=>{if(e.keyCode==UnityEngine.KeyCode.Space||e.keyCode==UnityEngine.KeyCode.Return){if(Details.IsOpen)Details.Close();else Details.Open();e.StopPropagation();}});
  Bind(null);
 }
 public StatusRow Bind(EffectInstance[] effects){timed.Clear();if(effects!=null)for(int i=0;i<effects.Length;i++)if(effects[i]!=null&&effects[i].Timed)timed.Add(effects[i]);
  int overflow=timed.Count>Max?timed.Count-(Max-1):0,shown=overflow>0?Max-1:timed.Count;
  for(int i=0;i<Max;i++){bool on=i<shown||(overflow>0&&i==Max-1);squares[i].Show(on);if(!on)continue;if(i<shown)squares[i].Bind(timed[i]);else squares[i].Overflow(overflow);}
  this.Show(timed.Count>0);pickingMode=timed.Count>0?PickingMode.Position:PickingMode.Ignore;if(timed.Count!=shownCount){shownCount=timed.Count;tooltip=shownCount>0?"Statuses · "+shownCount:"";}list?.Bind(timed);return this;}
}
}
