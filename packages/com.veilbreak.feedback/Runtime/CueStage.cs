using System;
using UnityEngine;
namespace Veilbreak.Feedback {
/// <summary>Draws catalogue set pieces through the pooled sinks, so a host only maps its events to a <see cref="CueCatalog.Cue"/>, an
/// <see cref="FxCall"/> and a ground point. <see cref="Play"/> plays the particles (budgeted by priority), then the ground mark, a light
/// flash (skipped under <see cref="ReducedFlashes"/>), the shake and the sound; <see cref="Punch"/> is a heavy moment's screen impact and
/// particle burst; <see cref="Reticle"/> draws closing chevrons around a marked body. Any sink may be null. The host scales and applies
/// shake (<see cref="Shake"/>) and pulses (<see cref="Pulsed"/>) and owns every game decision: which events play, colours and strengths.</summary>
public sealed class CueStage {
 public EffectEmitters Emitters;public GroundMarks Marks;public LightFlashes Flashes;public AudioVoices Voices;
 /// <summary>Reduced Flashes: cues play without their light flash.</summary>
 public bool ReducedFlashes;
 /// <summary>Metres from the centre line to full stereo pan.</summary>
 public float PanWidth=7;
 /// <summary>Raised with a played cue's shake (0–1); the host scales it onto its camera rig.</summary>
 public Action<float> Shake;
 /// <summary>Raised with a played cue's pulse (a flash volume weight).</summary>
 public Action<float> Pulsed;

 /// <summary>One set piece at `ground` (the point on the ground under it). The cue's routine wins over `fallback`, its magnitude scales
 /// `call` and a tint with alpha replaces the call colour. A piece the particle budget drops keeps its mark, light, shake and sound only
 /// at priority 2 and up. Returns whether the particles played.</summary>
 public bool Play(CueCatalog.Cue cue,EffectRoutine fallback,FxCall call,Vector3 ground,bool audible=true){
  int priority=cue?.priority??0;var routine=cue!=null&&cue.routine!=EffectRoutine.None?cue.routine:fallback;
  if(cue!=null){call.magnitude*=cue.magnitude;if(cue.tint.a>0)call.color=cue.tint;}
  bool played=Emitters!=null&&Emitters.Play(routine,call,priority);if(!played&&priority<2)return false;
  if(cue==null)return played;if(cue.pulse>0)Pulsed?.Invoke(cue.pulse);
  if(cue.mark!=MarkKind.None)Marks?.Add(cue.mark,ground,cue.markSize*Mathf.Sqrt(call.magnitude));
  if(cue.light>0&&!ReducedFlashes)Flashes?.Play(ground+Vector3.up*1.2f,Color.Lerp(call.color,new Color(1,.65f,.35f),routine==EffectRoutine.Explosion?.6f:0),cue.light,4+2*call.magnitude,.25f+.1f*call.magnitude);
  if(cue.shake>0)Shake?.Invoke(cue.shake);
  if(audible&&cue.sound!=null&&Voices!=null)Voices.Play(cue.sound,cue.volume,ground.x/PanWidth,priority);
  return played;}

 /// <summary>A heavy moment of `strength` (about 0.2 for a heavy blow, 1 for a boss slam) at `at`: the screen impact at its projection
 /// through `view`, then sparks flung out, a flare, a flat shock ring and a glow. Strength 0.6 and up plays at priority 2. Shake and any HUD
 /// jolt stay with the host.</summary>
 public void Punch(Vector3 at,Color color,float strength,Camera view){
  if(view!=null){var v=view.WorldToViewportPoint(at);if(v.z>0)ScreenImpacts.Push(new Vector2(v.x,v.y),color,strength);}
  if(Emitters==null)return;int n=Mathf.RoundToInt(8+14*strength);if(!Emitters.Allow(strength>=.6f?2:1,n+3))return;var ground=new Vector3(at.x,.06f,at.z);var hot=Color.Lerp(color,Color.white,.45f);
  for(int i=0;i<n;i++){var dir=(UnityEngine.Random.onUnitSphere+Vector3.up*.6f).normalized;Emitters.Emit(EffectEmitters.Layer.Sparks,at,dir*UnityEngine.Random.Range(4,10)*(.6f+strength),UnityEngine.Random.Range(.05f,.1f),UnityEngine.Random.Range(.25f,.55f),hot*1.6f);}
  Emitters.Emit(EffectEmitters.Layer.Flares,at,Vector3.zero,1.6f+2.4f*strength,.18f+.12f*strength,hot*2);
  Emitters.Emit(EffectEmitters.Layer.Rings,ground,Vector3.zero,2.5f+6*strength,.35f+.2f*strength,color*1.4f);
  Emitters.Emit(EffectEmitters.Layer.Glow,at,Vector3.zero,2+3*strength,.3f,color);}

 /// <summary>Four chevrons closing on a marked body of size `body` at `local` (the batch's space) and turning slowly at `time` seconds
 /// (pass real time so the mark stays alive while paused), over a faint ring. Adds five quads.</summary>
 public static void Reticle(QuadBatch batch,Vector3 local,float body,Color color,float time){if(batch==null)return;float spin=time*40,breathe=1+.08f*Mathf.Sin(time*6),r=body*.62f*breathe;
  for(int i=0;i<4;i++){float a=spin+i*90;var dir=Quaternion.Euler(0,a,0)*Vector3.forward;batch.Add(local+dir*r+Vector3.up*.01f,body*.32f,a+180,FxTile.Chevron,color);}
  batch.Add(local+Vector3.up*.008f,body*1.35f,0,FxTile.Ring,new Color(color.r,color.g,color.b,.3f));}
}
}
