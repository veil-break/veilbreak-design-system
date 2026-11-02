using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;
namespace Veilbreak.Gui {
public enum MotionKind { Fade, Rise, Drop, Left, Right, Zoom, Deal }
/// <summary>Real-time, reversible presence with independent feedback and number tracks. Detach finalizes every track.</summary>
public static class VbMotion {
 public const float Shift=12,ZoomFrom=.94f;public const int StaggerStep=MotionRecipes.Stagger;
 /// <summary>Inject a monotonic clock for deterministic attached-panel tests/captures; restore it afterwards.</summary>
 public static Func<double> Clock=()=>Time.realtimeSinceStartupAsDouble;
 sealed class Track {
  public bool known,shown,saved,counting,presence,feedback,pending,enabled;public float opacity=1,feedbackValue;public int feedbackKind;
  public double began,feedbackBegan;public int delay;public float from,to;public MotionRecipe recipe;public Action done,countDone;
  public IVisualElementScheduledItem timer,countTimer;public long count;public int countSerial;
  public StyleTranslate translate;public StyleRotate rotate;public StyleScale scale;public StyleFloat restOpacity;
 }
 static readonly ConditionalWeakTable<VisualElement,Track> tracks=new ConditionalWeakTable<VisualElement,Track>();
 static Track Of(VisualElement e)=>tracks.GetValue(e,key=>{var t=new Track();key.RegisterCallback<DetachFromPanelEvent>(_=>Finish(key,t));return t;});
 public static bool Calm(VisualElement e)=>Ui.Under(e,SafeAreaRoot.ReducedMotion);
 public static bool Displayed(VisualElement e){var d=e.style.display;return d.keyword==StyleKeyword.Undefined?d.value!=DisplayStyle.None:e.resolvedStyle.display!=DisplayStyle.None;}
 public static bool Shown(VisualElement e)=>e!=null&&(tracks.TryGetValue(e,out var t)&&t.known?t.shown:Displayed(e));
 public static bool Busy(VisualElement e)=>e!=null&&tracks.TryGetValue(e,out var t)&&(t.presence||t.feedback||t.countTimer!=null);
 public static float Ease(float p)=>1-(1-p)*(1-p)*(1-p);
 public static void Show(VisualElement e,bool on,MotionKind kind=MotionKind.Fade,Action done=null){if(e==null)return;var t=Of(e);
  if(!t.known){t.known=true;t.shown=Displayed(e);t.opacity=t.shown?1:0;}
  if(t.shown==on&&(t.presence||Displayed(e)==on)){if(!t.presence)done?.Invoke();return;}
  Start(e,t,on,MotionRecipes.For(kind,on),0,done,false);
 }
 public static void In(VisualElement e,MotionKind kind=MotionKind.Rise,int delay=0,Action done=null)=>In(e,MotionRecipes.For(kind),delay,done);
 public static void In(VisualElement e,MotionRecipe recipe,int delay=0,Action done=null){if(e!=null)Start(e,Of(e),true,recipe,delay,done,true);}
 public static void Out(VisualElement e,MotionKind kind=MotionKind.Fade,Action done=null)=>Out(e,MotionRecipes.For(kind,false),done);
 public static void Out(VisualElement e,MotionRecipe recipe,Action done=null){if(e!=null)Start(e,Of(e),false,recipe,0,done,false);}
 public static void Stagger(IEnumerable<VisualElement> items,MotionKind kind=MotionKind.Deal,int start=0,int step=StaggerStep){if(items==null)return;int i=0;foreach(var e in items)if(e!=null&&Displayed(e))In(e,kind,start+Mathf.Min(i++*step,MotionRecipes.MaxDealDelay));}
 public static void StaggerChildren(VisualElement parent,MotionKind kind=MotionKind.Deal,int start=0,int step=StaggerStep){if(parent!=null)Stagger(new List<VisualElement>(parent.Children()),kind,start,step);}
 public static void Swap(VisualElement e,Action rebuild,MotionKind kind=MotionKind.Fade){if(e==null||e.panel==null||!Displayed(e)){rebuild?.Invoke();return;}Out(e,MotionKind.Fade,()=>{rebuild?.Invoke();In(e,kind);});}
 public static void Pop(VisualElement e,float amount=.1f)=>Pulse(e,0,amount);
 public static void Shake(VisualElement e)=>Pulse(e,1,4);
 public static void Count(Label label,long to,Func<long,string> format){if(label==null)return;format??=Ui.Number;var t=Of(label);
  if(t.counting&&t.count==to)return;long from=t.count;t.count=to;t.countSerial++;t.countTimer?.Pause();t.countTimer=null;t.countDone=()=>label.text=format(to);
  if(!t.counting||label.panel==null||Calm(label)){t.counting=true;t.countDone();t.countDone=null;return;}
  int serial=t.countSerial;double began=Clock();t.counting=true;
  void Tick(){if(t.countSerial!=serial)return;float p=Calm(label)?1:Progress(began,0,VbTokens.DurChange);label.text=format(from+(long)Math.Round((to-from)*Ease(p)));if(p<1)return;t.countTimer?.Pause();t.countTimer=null;t.countDone=null;}
  t.countTimer=label.schedule.Execute(Tick).Every(16);Tick();
 }
 public static void Finish(VisualElement e){if(e!=null&&tracks.TryGetValue(e,out var t))Finish(e,t);}
 static void Finish(VisualElement e,Track t){var done=t.done;t.done=null;t.timer?.Pause();t.timer=null;t.presence=t.feedback=false;t.countTimer?.Pause();t.countTimer=null;t.countSerial++;t.countDone?.Invoke();t.countDone=null;if(t.pending){e.SetEnabled(t.enabled);t.pending=false;}e.RemoveFromClassList("vb-motion-pending");Restore(e,t);if(t.known)e.Show(t.shown);done?.Invoke();}
 static void Save(VisualElement e,Track t){if(t.saved)return;t.saved=true;t.translate=e.style.translate;t.rotate=e.style.rotate;t.scale=e.style.scale;t.restOpacity=e.style.opacity;e.AddToClassList("vb-motion-active");}
 static void Restore(VisualElement e,Track t){if(!t.saved)return;t.saved=false;e.style.translate=t.translate;e.style.rotate=t.rotate;e.style.scale=t.scale;e.style.opacity=t.restOpacity;e.RemoveFromClassList("vb-motion-active");}
 static float Progress(double began,int delay,int duration)=>Mathf.Clamp01((float)((Clock()-began)*1000-delay)/Mathf.Max(1,duration));
 static void Start(VisualElement e,Track t,bool on,MotionRecipe recipe,int delay,Action done,bool restart){bool was=t.known;if(!t.presence)t.opacity=Displayed(e)?1:0;
  t.known=true;t.shown=on;t.done=done;t.recipe=recipe;t.delay=delay;t.began=Clock();t.from=restart&&!t.presence?0:was?t.opacity:on?0:1;t.to=on?1:0;
  if(e.panel==null){Finish(e,t);return;}Save(e,t);t.presence=true;e.Show(true);Ensure(e,t);Draw(e,t);
 }
 static void Ensure(VisualElement e,Track t){if(t.timer==null)t.timer=e.schedule.Execute(()=>Draw(e,t)).Every(16);}
 static void Draw(VisualElement e,Track t){bool calm=Calm(e);Action done=null;
  if(t.presence){float p=Progress(t.began,calm?0:t.delay,calm?Mathf.Min(100,t.recipe.milliseconds):t.recipe.milliseconds);t.opacity=Mathf.Lerp(t.from,t.to,Ease(p));if(p>=1){t.presence=false;done=t.done;t.done=null;}}
  float wave=0,shake=0;if(t.feedback){float p=Progress(t.feedbackBegan,0,VbTokens.DurPress*2);wave=Mathf.Sin(p*Mathf.PI)*t.feedbackValue;shake=Mathf.Sin(p*Mathf.PI*6)*t.feedbackValue*(1-p);if(p>=1)t.feedback=false;}
  float away=1-t.opacity;var recipe=t.recipe;e.style.opacity=t.opacity*(calm?1-Mathf.Abs(wave)*.35f:1);
  if(!calm){e.style.translate=new Translate(recipe.x*away+(t.feedbackKind==1?shake:0),recipe.y*away);e.style.rotate=new Rotate(new Angle(recipe.rotation*away));float scale=Mathf.Lerp(1,recipe.scale,away)+(t.feedbackKind==0?wave:0);e.style.scale=new Scale(new Vector3(scale,scale,1));}
  else{e.style.translate=t.translate;e.style.rotate=t.rotate;e.style.scale=t.scale;}
  // Delayed choices are neither pointer nor keyboard targets; the first visible frame enables them.
  bool pending=t.presence&&t.shown&&t.opacity<=0;if(pending&&!t.pending){t.enabled=e.enabledSelf;e.SetEnabled(false);}else if(!pending&&t.pending)e.SetEnabled(t.enabled);t.pending=pending;e.EnableInClassList("vb-motion-pending",pending);
  if(!t.presence&&!t.feedback){t.timer?.Pause();t.timer=null;e.RemoveFromClassList("vb-motion-pending");Restore(e,t);e.Show(t.shown);}
  done?.Invoke();
 }
 static void Pulse(VisualElement e,int kind,float amount){if(e==null||e.panel==null||!Displayed(e))return;var t=Of(e);if(t.known&&!t.shown)return;Save(e,t);if(!t.known){t.known=t.shown=true;t.opacity=1;}t.feedback=true;t.feedbackKind=kind;t.feedbackValue=amount;t.feedbackBegan=Clock();Ensure(e,t);Draw(e,t);}
 /// <summary>Host-controlled deployment timeline, using the same recipe as automatic HUD entrances.</summary>
 public static void Scrub(VisualElement e,float progress,MotionRecipe recipe){if(e==null)return;progress=Mathf.Clamp01(progress);e.visible=progress>0;e.style.opacity=progress;float away=1-Ease(progress);e.style.translate=Calm(e)?new Translate(0,0):new Translate(recipe.x*away,recipe.y*away);e.style.rotate=new Rotate(new Angle(Calm(e)?0:recipe.rotation*away));}
}
}
