using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
namespace Veilbreak.Gui {
/// <summary>Shared detail surface; updates in place while its effect changes or expires.</summary>
public sealed class EffectDetails:DetailView {
 readonly Label title,description,source,value,time,state;
 public EffectDetails(){AddToClassList("vb-effect-detail");title=Ui.Text(this,"","vb-t-title");description=Ui.Text(this,"","vb-t-body");source=Ui.Text(this,"","vb-t-caption");value=Ui.Text(this,"","vb-t-label");time=Ui.Text(this,"","vb-t-caption");state=Ui.Text(this,"","vb-t-badge");}
 public EffectDetails Bind(EffectInstance e){var d=e.definition??EffectDefinition.Unknown(e.id);title.text=d.title;description.text=d.description;source.text=string.IsNullOrEmpty(e.source)?"Source unavailable":"Source · "+e.source;
  value.text=e.magnitude+(e.stacks>1?" · ×"+e.stacks:"");value.Show(value.text.Length>0);time.text=e.remainingSeconds.HasValue?Ui.Seconds(e.remainingSeconds.Value)+" remaining"+(e.totalSeconds.HasValue?" / "+Ui.Seconds(e.totalSeconds.Value):""):"Duration unavailable or persistent";state.text=d.scope+" · "+e.activation;return this;}
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
 sealed class EffectRow:ViewBase {public EffectBadge Badge=>badge;readonly EffectBadge badge;readonly Label title,detail;public EffectRow(){AddToClassList("vb-effect-row");badge=new EffectBadge().Into(this);var text=Ui.Box(this);title=Ui.Text(text,"","vb-t-label");detail=Ui.Text(text,"","vb-t-caption");}public void Bind(EffectInstance e){badge.Bind(e);title.text=(e.definition??EffectDefinition.Unknown(e.id)).title;detail.text=e.magnitude+" · "+(e.remainingSeconds.HasValue?Ui.Seconds(e.remainingSeconds.Value):e.activation.ToString());}}
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
}
