using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;
namespace Veilbreak.Gui {
/// <summary>How an element enters and leaves: a plain fade, or a fade with a short shift (from below, from above, from the left or the right)
/// or a slight scale. Reduced Motion turns every kind into the fade.</summary>
public enum MotionKind { Fade, Rise, Drop, Left, Right, Zoom }

/// <summary>The kit's one animator for interface state changes, on real time (the interface answers at the same speed whatever the fight
/// is doing). Every show, hide, page, modal, list entrance, value change and refusal goes through it, so motion is consistent and every
/// setting is honoured in one place:
/// <list type="bullet">
/// <item><see cref="Show"/> fades (and shifts) an element in or out and is idempotent, so per-frame binding may call it freely.</item>
/// <item><see cref="In"/>, <see cref="Out"/> and <see cref="Stagger"/> play entrances and exits; <see cref="Swap"/> fades content out, rebuilds it and fades it in.</item>
/// <item><see cref="Pop"/>, <see cref="Shake"/> and <see cref="Count"/> mark a value that changed, a refused action and a number rolling to its new value.</item>
/// </list>
/// Durations are tokens: entrances <c>dur-ui</c>, exits <c>dur-glitch-out</c>, pops <c>dur-press</c> twice, counts <c>dur-change</c>. Easing is
/// ease-out with no overshoot (the wave slam stays the one overshoot). Under <c>vb-reduced-motion</c> shifts, scales and shakes become fades; off a
/// panel everything is instant. A newer call replaces an older one on the same element, including its callback, and detaching finishes it.</summary>
public static class VbMotion {
 /// <summary>Units an entrance shifts by.</summary>
 public const float Shift=12;
 /// <summary>Scale a <see cref="MotionKind.Zoom"/> entrance starts from.</summary>
 public const float ZoomFrom=.94f;
 /// <summary>Milliseconds between items of a <see cref="Stagger"/> (the perk deal's 50 ms).</summary>
 public const int StaggerStep=50;
 sealed class Track {
  public IVisualElementScheduledItem timer;public int serial;public bool known,shown;public float opacity=-1;
  public StyleTranslate translate;public StyleScale scale;public StyleFloat restOpacity;public bool saved;public Action done;
  public long count;public bool counting;public int countSerial;public IVisualElementScheduledItem countTimer;
 }
 static readonly ConditionalWeakTable<VisualElement,Track> tracks=new ConditionalWeakTable<VisualElement,Track>();
 static Track Of(VisualElement e)=>tracks.GetValue(e,key=>{var t=new Track();key.RegisterCallback<DetachFromPanelEvent>(_=>Finish(key,t));return t;});

 /// <summary>True under Reduced Motion: movement becomes fades.</summary>
 public static bool Calm(VisualElement e)=>Ui.Under(e,SafeAreaRoot.ReducedMotion);
 /// <summary>The state <see cref="Show"/> is heading for: false once a hide has started, even while it still fades.</summary>
 /// <summary>Whether <paramref name="e"/> is displayed: its inline display when code set one, else the resolved style (a UXML
 /// <c>style="display: none"</c> or a USS rule is not visible through <c>style.display</c>).</summary>
 public static bool Displayed(VisualElement e){var d=e.style.display;return d.keyword==StyleKeyword.Undefined?d.value!=DisplayStyle.None:e.resolvedStyle.display!=DisplayStyle.None;}
 public static bool Shown(VisualElement e)=>e!=null&&(tracks.TryGetValue(e,out var t)&&t.known?t.shown:Displayed(e));
 /// <summary>True while an animation runs on <paramref name="e"/>.</summary>
 public static bool Busy(VisualElement e)=>e!=null&&tracks.TryGetValue(e,out var t)&&t.timer!=null;

 /// <summary>Shows or hides <paramref name="e"/> with an entrance or exit. Calling it again with the state it is already in (or heading for)
 /// does nothing, so binders may call it every frame; reversing mid-way continues from the current opacity.</summary>
 public static void Show(VisualElement e,bool on,MotionKind kind=MotionKind.Fade,Action done=null){
  if(e==null)return;var t=Of(e);bool displayed=Displayed(e);
  if(!t.known){t.known=true;t.shown=displayed;}
  if(t.shown==on&&(t.timer!=null||displayed==on)){if(t.timer==null)done?.Invoke();return;}
  t.shown=on;if(e.panel==null){Stop(e,t);e.Show(on);done?.Invoke();return;}
  Play(e,t,on,kind,on?VbTokens.DurUi:VbTokens.DurGlitchOut,0,done);
 }
 /// <summary>Plays an entrance on <paramref name="e"/> (now visible) after <paramref name="delay"/> ms, whatever state it was in.</summary>
 public static void In(VisualElement e,MotionKind kind=MotionKind.Rise,int delay=0,Action done=null){
  if(e==null)return;var t=Of(e);t.known=true;t.shown=true;if(e.panel==null&&delay<=0){Stop(e,t);e.Show(true);done?.Invoke();return;}
  t.opacity=0;Play(e,t,true,kind,VbTokens.DurUi,delay,done);
 }
 /// <summary>Plays an exit and hides <paramref name="e"/>; <paramref name="done"/> runs once it is hidden (remove it there).</summary>
 public static void Out(VisualElement e,MotionKind kind=MotionKind.Fade,Action done=null){
  if(e==null)return;var t=Of(e);t.known=true;t.shown=false;if(e.panel==null){Stop(e,t);e.Show(false);done?.Invoke();return;}
  Play(e,t,false,kind,VbTokens.DurGlitchOut,0,done);
 }
 /// <summary>Entrances one after another (<see cref="StaggerStep"/> apart): list rows, grid tiles, dealt cards.</summary>
 public static void Stagger(IEnumerable<VisualElement> items,MotionKind kind=MotionKind.Rise,int start=0,int step=StaggerStep){if(items==null)return;int delay=start;foreach(var e in items){if(e==null||!Displayed(e))continue;In(e,kind,delay);delay+=step;}}
 /// <summary>Entrances for <paramref name="parent"/>'s visible children, staggered.</summary>
 public static void StaggerChildren(VisualElement parent,MotionKind kind=MotionKind.Rise,int start=0,int step=StaggerStep){if(parent==null)return;var list=new List<VisualElement>(parent.Children());Stagger(list,kind,start,step);}
 /// <summary>Fades <paramref name="e"/> out, runs <paramref name="rebuild"/> and fades it back in: content replaced in place (a page re-bound
 /// with another subject, a sheet showing another operative).</summary>
 public static void Swap(VisualElement e,Action rebuild,MotionKind kind=MotionKind.Fade){if(e==null){rebuild?.Invoke();return;}
  if(e.panel==null||!Displayed(e)){rebuild?.Invoke();return;}Out(e,MotionKind.Fade,()=>{rebuild?.Invoke();In(e,kind);});}

 /// <summary>A short swell (1 → 1.1 → 1) on a value that just changed: credits, a skill coming ready, a toggle. A fade dip under Reduced Motion.</summary>
 public static void Pop(VisualElement e,float amount=.1f){Pulse(e,0,amount);}
 /// <summary>A brief sideways shake on a refused action; a fade dip under Reduced Motion.</summary>
 public static void Shake(VisualElement e){Pulse(e,1,4);}
 /// <summary>Rolls <paramref name="label"/> from the value it shows to <paramref name="to"/> over <c>dur-change</c>, formatted by
 /// <paramref name="format"/>; the first call (or an off-panel label) sets it at once. Under Reduced Motion it jumps.</summary>
 public static void Count(Label label,long to,Func<long,string> format){
  if(label==null)return;format??=Ui.Number;var t=Of(label);
  if(!t.counting||label.panel==null||Calm(label)){t.counting=true;t.count=to;label.text=format(to);return;}
  if(t.count==to)return;long from=t.count;t.count=to;int serial=++t.countSerial;t.countTimer?.Pause();float began=Time.realtimeSinceStartup;
  void Tick(){if(t.countSerial!=serial)return;float p=Progress(began,0,VbTokens.DurChange);label.text=format(from+(long)Math.Round((to-from)*Ease(p)));if(p<1)return;t.countTimer?.Pause();t.countTimer=null;}
  Tick();t.countTimer=label.schedule.Execute(Tick).Every(16);
 }

 /// <summary>Ends any animation on <paramref name="e"/> at its target state at once.</summary>
 public static void Finish(VisualElement e){if(e!=null&&tracks.TryGetValue(e,out var t))Finish(e,t);}

 static void Finish(VisualElement e,Track t){if(t.timer==null)return;var done=t.done;Stop(e,t);if(t.known)e.Show(t.shown);done?.Invoke();}
 static void Stop(VisualElement e,Track t){t.serial++;t.timer?.Pause();t.timer=null;t.done=null;t.opacity=-1;Restore(e,t);}
 static void Save(VisualElement e,Track t){if(t.saved)return;t.saved=true;t.translate=e.style.translate;t.scale=e.style.scale;t.restOpacity=e.style.opacity;}
 static void Restore(VisualElement e,Track t){if(!t.saved)return;t.saved=false;e.style.translate=t.translate;e.style.scale=t.scale;e.style.opacity=t.restOpacity;}
 static float Progress(float began,int delay,int duration)=>Mathf.Clamp01(((Time.realtimeSinceStartup-began)*1000-delay)/Mathf.Max(1,duration));
 /// <summary>Ease-out cubic: fast start, settles without overshoot.</summary>
 public static float Ease(float p)=>1-(1-p)*(1-p)*(1-p);
 static Vector2 Offset(MotionKind kind)=>kind switch{MotionKind.Rise=>new Vector2(0,Shift),MotionKind.Drop=>new Vector2(0,-Shift),MotionKind.Left=>new Vector2(-Shift,0),MotionKind.Right=>new Vector2(Shift,0),_=>Vector2.zero};

 static void Play(VisualElement e,Track t,bool on,MotionKind kind,int duration,int delay,Action done){
  float from=t.opacity>=0?t.opacity:on?0:1;t.serial++;t.timer?.Pause();t.timer=null;t.done=done;
  Save(e,t);int serial=t.serial;float began=Time.realtimeSinceStartup,to=on?1:0;var offset=Offset(kind);bool zoom=kind==MotionKind.Zoom;
  e.Show(true);
  void Draw(){if(t.serial!=serial)return;float p=Progress(began,delay,duration),k=Ease(p),o=Mathf.Lerp(from,to,k);t.opacity=o;
   bool calm=Calm(e);float away=1-o;e.style.opacity=o;
   if(!calm&&offset!=Vector2.zero)e.style.translate=new Translate(offset.x*away,offset.y*away);
   if(!calm&&zoom){float s=Mathf.Lerp(1,ZoomFrom,away);e.style.scale=new Scale(new Vector3(s,s,1));}
   if(p<1)return;var finished=t.done;t.timer?.Pause();t.timer=null;t.done=null;t.opacity=-1;Restore(e,t);if(!on)e.Show(false);finished?.Invoke();}
  Draw();t.timer=e.schedule.Execute(Draw).Every(16);
 }
 // kind 0: swell by `amount` (scale), 1: shake by `amount` units.
 static void Pulse(VisualElement e,int kind,float amount){
  if(e==null||e.panel==null||!Displayed(e))return;var t=Of(e);if(t.timer!=null&&t.known&&!t.shown)return;
  if(t.timer!=null)Stop(e,t);Save(e,t);int serial=++t.serial;float began=Time.realtimeSinceStartup;int duration=VbTokens.DurPress*2;
  void Draw(){if(t.serial!=serial)return;float p=Progress(began,0,duration),wave=Mathf.Sin(p*Mathf.PI);
   if(Calm(e))e.style.opacity=1-.35f*wave;
   else if(kind==0){float s=1+amount*wave;e.style.scale=new Scale(new Vector3(s,s,1));}
   else e.style.translate=new Translate(Mathf.Sin(p*Mathf.PI*6)*amount*(1-p),0);
   if(p<1)return;t.timer?.Pause();t.timer=null;Restore(e,t);}
  Draw();t.timer=e.schedule.Execute(Draw).Every(16);
 }
}
}
