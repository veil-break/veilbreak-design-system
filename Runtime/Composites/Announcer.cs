using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
namespace Veilbreak.Gui {
/// <summary>A bounded heads-up message lane. Critical notices pre-empt routine ones; every replacement glitches out before in.
/// Hosts feed confirmed events, rather than mirroring persistent simulation strings every frame.</summary>
public sealed class Announcer:IDisposable {
 readonly GlitchText label;readonly Queue<(string text,bool alert,int hold)> pending=new Queue<(string,bool,int)>();
 IVisualElementScheduledItem hold;bool busy,disposed;int serial;string current="";
 public GlitchText Label=>label;
 public Announcer(GlitchText label){this.label=label;label.Show(false);label.RegisterCallback<DetachFromPanelEvent>(_=>Dispose());}
 public void Say(string text,bool alert=false,int milliseconds=1800,bool interrupt=false){
  if(disposed||string.IsNullOrEmpty(text))return;
  if(text==current&&busy&&!interrupt)return;
  if(interrupt||alert){pending.Clear();pending.Enqueue((text,alert,milliseconds));if(busy)Replace();else Next();return;}
  if(pending.Count>=6)pending.Dequeue();pending.Enqueue((text,alert,milliseconds));if(!busy)Next();
 }
 void Replace(){hold?.Pause();int id=++serial;busy=true;Glitch.Out(label,()=>{if(id==serial)Next();});}
 void Next(){if(disposed)return;if(pending.Count==0){busy=false;current="";label.Show(false);return;}
  var notice=pending.Dequeue();current=notice.text;busy=true;label.text=notice.text;label.EnableInClassList("vb-announcement--alert",notice.alert);int id=++serial;
  Glitch.In(label,()=>{if(id!=serial||disposed)return;hold=label.schedule.Execute(()=>{if(id!=serial||disposed)return;Glitch.Out(label,()=>{if(id==serial)Next();});});hold.ExecuteLater(notice.hold);});
 }
 public void Clear(){if(disposed)return;serial++;pending.Clear();hold?.Pause();Glitch.Cancel(label);label.Show(false);current="";busy=false;}
 public void Dispose(){if(disposed)return;Clear();disposed=true;}
}
}
