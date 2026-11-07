using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
namespace Veilbreak.Gui {
/// <summary>A bounded heads-up message lane. Critical notices pre-empt routine ones; every replacement glitches out before in.
/// Hosts feed confirmed events, rather than mirroring persistent simulation strings every frame.</summary>
public sealed class Announcer:IDisposable {
 readonly GlitchText label;readonly Queue<(string text,bool alert,int hold)> pending=new Queue<(string,bool,int)>();
 IVisualElementScheduledItem hold;bool busy,disposed,parked;int serial;string current="";
 public GlitchText Label=>label;
 /// <summary>How long a notice takes to appear: the title reveal (<see cref="Glitch.Reveal"/>) at this length (700 reads well for a wave banner); 0, the default, keeps the short glitch-in.</summary>
 public int RevealMilliseconds {get;set;}
 public Announcer(GlitchText label){this.label=label;label.Show(false);label.RegisterCallback<DetachFromPanelEvent>(Park);label.RegisterCallback<AttachToPanelEvent>(Resume);}
 /// <summary>A detach is usually temporary (a move in the hierarchy, a screen swap), so the lane pauses and keeps working: the notice on show is retired and scheduled work stops, while queued notices, and any said while it is away, wait for the re-attach.</summary>
 void Park(DetachFromPanelEvent _){if(disposed)return;serial++;hold?.Pause();Glitch.Cancel(label);label.Show(false);current="";busy=false;parked=true;}
 void Resume(AttachToPanelEvent _){if(disposed||!parked)return;parked=false;if(!busy)Next();}
 public void Say(string text,bool alert=false,int milliseconds=1800,bool interrupt=false){
  if(disposed||string.IsNullOrEmpty(text))return;
  if(text==current&&busy&&!interrupt)return;
  if(interrupt||alert){pending.Clear();pending.Enqueue((text,alert,milliseconds));if(parked)return;if(busy)Replace();else Next();return;}
  if(pending.Count>=6)pending.Dequeue();pending.Enqueue((text,alert,milliseconds));if(!busy&&!parked)Next();
 }
 void Replace(){hold?.Pause();int id=++serial;busy=true;Glitch.Out(label,()=>{if(id==serial)Next();});}
 void Next(){if(disposed)return;if(pending.Count==0){busy=false;current="";label.Show(false);return;}
  var notice=pending.Dequeue();current=notice.text;busy=true;label.text=notice.text;label.EnableInClassList("vb-announcement--alert",notice.alert);int id=++serial;
  System.Action shown=()=>{if(id!=serial||disposed)return;hold=label.schedule.Execute(()=>{if(id!=serial||disposed)return;Glitch.Out(label,()=>{if(id==serial)Next();});});hold.ExecuteLater(notice.hold);};if(RevealMilliseconds>0)Glitch.Reveal(label,RevealMilliseconds,shown);else Glitch.In(label,shown);
 }
 public void Clear(){if(disposed)return;serial++;pending.Clear();hold?.Pause();Glitch.Cancel(label);label.Show(false);current="";busy=false;}
 /// <summary>Retires the lane for good and releases its panel callbacks. Only an explicit call does this; a panel detach alone never does.</summary>
 public void Dispose(){if(disposed)return;Clear();disposed=true;label.UnregisterCallback<DetachFromPanelEvent>(Park);label.UnregisterCallback<AttachToPanelEvent>(Resume);}
}
}
