using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
namespace Veilbreak.Gui {
/// <summary>A flat track with a fill, an optional extra span after it (barrier) and an optional dashed preview span (a cost being aimed).</summary>
[UxmlElement] public partial class VbMeter:VisualElement {
 public const string ClassName="vb-meter";
 readonly VisualElement fill,extra,preview;readonly List<VisualElement> marks=new List<VisualElement>();
 public VbMeter(){AddToClassList(ClassName);pickingMode=PickingMode.Ignore;fill=Ui.Box(this,ClassName+"__fill");extra=Ui.Box(this,ClassName+"__extra");preview=Ui.Box(this,ClassName+"__preview");extra.Show(false);preview.Show(false);}
 public VbMeter Set(float value,float extraSpan=0){
  value=Mathf.Clamp01(value);Ui.Fill(fill,value);extraSpan=Mathf.Clamp(extraSpan,0,1-value);
  extra.Show(extraSpan>0);extra.style.left=Length.Percent(value*100);Ui.Fill(extra,extraSpan);return this;
 }
 /// <summary>Dashed span between two fractions; a negative <paramref name="from"/> hides it.</summary>
 public VbMeter Preview(float from,float to){bool on=from>=0&&to>from;preview.Show(on);if(on){preview.style.left=Length.Percent(Mathf.Clamp01(from)*100);preview.style.width=Length.Percent(Mathf.Clamp01(to-from)*100);}return this;}
 /// <summary>Numbered marks at fractions of the track (queue entries firing at a Power level).</summary>
 public VbMeter Marks(IReadOnlyList<float> at){
  for(int i=0;i<Mathf.Max(marks.Count,at?.Count??0);i++){
   if(i>=marks.Count){var m=Ui.Box(this,ClassName+"__mark");Ui.Text(m,(i+1).ToString(),ClassName+"__mark-label");marks.Add(m);}
   bool on=at!=null&&i<at.Count;marks[i].Show(on);if(on)marks[i].style.left=Length.Percent(Mathf.Clamp01(at[i])*100);
  }
  return this;
 }
}

/// <summary>Health over stability in team colour, barrier after the fill, a pale ghost of recent damage and an optional caption.
/// Exposed fills the stability track in enemy colour. Sizes: HUD (default), World (44 units, thinner) and Large.</summary>
[UxmlElement] public partial class HealthBar:VisualElement {
 public const string ClassName="vb-health";
 readonly VbMeter health,stability;readonly VisualElement ghost;readonly Label caption;
 public HealthBar(){AddToClassList(ClassName);pickingMode=PickingMode.Ignore;caption=Ui.Text(this,"",ClassName+"__label");caption.Show(false);health=new VbMeter().Classes(ClassName+"__hp").Into(this);ghost=Ui.Box(null,ClassName+"__ghost");health.Insert(0,ghost);ghost.Show(false);stability=new VbMeter().Classes(ClassName+"__stability").Into(this);}
 /// <summary><paramref name="ghost"/> is health before the last hit (same units as <paramref name="hp"/>); the span down to the fill fades as the game lowers it.</summary>
 public HealthBar Set(float hp,float max,float barrier=0,float stability01=-1,bool exposed=false,bool enemy=false,float ghost=-1){
  max=Mathf.Max(1,max);health.Set(hp/max,barrier/max);bool trail=ghost>hp;this.ghost.Show(trail);if(trail)Ui.Fill(this.ghost,ghost/max);stability.Show(stability01>=0);if(stability01>=0)stability.Set(exposed?1:stability01);
  EnableInClassList(ClassName+"--exposed",exposed);EnableInClassList(ClassName+"--enemy",enemy);return this;
 }
 public HealthBar Size(MeterSize size){Ui.State(this,ClassName,size);return this;}
 /// <summary>A caps caption over the bar ("HP 820 / 1000"); empty hides it.</summary>
 public HealthBar Caption(string text){caption.text=text??"";caption.Show(caption.text.Length>0);return this;}
}

/// <summary>The shared Power pool: value, regen, the live consequence of an aim ("Fan −18 → 16 left"), a dashed cost preview and queue marks.</summary>
[UxmlElement] public partial class PowerStrip:VisualElement {
 public const string ClassName="vb-power";
 readonly Label value,max,regen,hint,legend;readonly VbMeter meter;readonly VisualElement reserve,queue;readonly List<QueueChip> chips=new List<QueueChip>();Action<QueueView> cancel;
 public PowerStrip(){
  AddToClassList(ClassName);pickingMode=PickingMode.Ignore;var head=Ui.Box(this,ClassName+"__head");
  Ui.Text(head,"POWER",ClassName+"__label");value=Ui.Text(head,"0",ClassName+"__value");max=Ui.Text(head,"/100",ClassName+"__max");regen=Ui.Text(head,"",ClassName+"__regen");hint=Ui.Text(head,"",ClassName+"__hint");
  meter=new VbMeter().Classes(ClassName+"__meter").Into(this);reserve=Ui.Box(meter,ClassName+"__reserve");reserve.Show(false);
  queue=Ui.Box(this,ClassName+"__queue");queue.Show(false);legend=Ui.Text(this,"",ClassName+"__legend");legend.Show(false);
 }
 int shownMax=-1;
 public PowerStrip Set(int value,int max,string regen,string hint=null,int previewFrom=-1,int previewTo=-1,IReadOnlyList<float> marks=null){
  float m=Mathf.Max(1,max);this.value.text=Ui.Count(value);if(max!=shownMax){shownMax=max;this.max.text="/"+max;}this.regen.text=regen??"";this.hint.text=hint??"";this.hint.Show(!string.IsNullOrEmpty(hint));
  meter.Set(value/m).Preview(previewFrom<0?-1:previewFrom/m,previewTo/m).Marks(marks);return this;
 }
 /// <summary>The autopilot's soft reserve as a line on the track; zero or less hides it.</summary>
 public PowerStrip Reserve(int at,int max=100){reserve.Show(at>0);if(at>0)reserve.style.left=Length.Percent(Mathf.Clamp01(at/(float)Mathf.Max(1,max))*100);reserve.tooltip="Autoplay keeps "+at+" Power in reserve";return this;}
 /// <summary>Queue entries as chips under the track (at most four, first in first out). Manual chips raise <paramref name="cancel"/> from their ×.</summary>
 public PowerStrip Queue(IReadOnlyList<QueueView> entries,Action<QueueView> cancel=null){this.cancel=cancel;int n=Mathf.Min(4,entries?.Count??0);queue.Show(n>0);
  for(int i=0;i<Mathf.Max(n,chips.Count);i++){if(i>=chips.Count){var chip=new QueueChip().Into(queue);chip.Cancelled+=()=>this.cancel?.Invoke(chip.View);chips.Add(chip);}chips[i].Show(i<n);if(i<n)chips[i].Bind(entries[i],i+1);}
  return this;}
 /// <summary>A one-line key under the strip ("▮ queued · ┊ reserve"); empty hides it.</summary>
 public PowerStrip Legend(string text){legend.text=text??"";legend.Show(legend.text.Length>0);return this;}
}

/// <summary>Focus: drains in real time while targeting slows combat, regenerates outside it.</summary>
[UxmlElement] public partial class FocusMeter:VisualElement {
 public const string ClassName="vb-focus";
 readonly Label value,rate;readonly VbMeter meter;
 public FocusMeter(){AddToClassList(ClassName);pickingMode=PickingMode.Ignore;Ui.Text(this,"FOCUS",ClassName+"__label");meter=new VbMeter().Classes(ClassName+"__meter").Into(this);value=Ui.Text(this,"100",ClassName+"__value");rate=Ui.Text(this,"READY",ClassName+"__rate");}
 (int,float,float,float) rateKey=(-1,0,0,0);
 public FocusMeter Set(float focus,bool slow,float regenPerSecond=12.5f,float drainPerSecond=25,float slowScale=.2f){
  meter.Set(focus/100f);value.text=Ui.Count(Mathf.RoundToInt(focus));EnableInClassList(ClassName+"--slow",slow);
  // The rate line changes only with its mode or rates, so it is rebuilt only then.
  var key=(slow?2:focus<100?1:0,regenPerSecond,drainPerSecond,slowScale);if(key!=rateKey){rateKey=key;rate.text=slow?System.FormattableString.Invariant($"×{slowScale:0.0} · −{drainPerSecond:0}/s"):focus<100?System.FormattableString.Invariant($"+{regenPerSecond:0.#}/s"):"READY";}return this;
 }
}
}
