using UnityEngine;
namespace Veilbreak.Feedback {
/// <summary>A small pool of 2D one-shot voices for cue sounds. Repeats of the same clip inside <see cref="Repeat"/> seconds
/// are dropped, a full pool steals its quietest lower-priority voice, and pitch varies slightly so rapid hits do not
/// phase. <see cref="Volume"/> is the consumer's channel level (for example its effects slider; master belongs on the AudioListener), applied to every voice.</summary>
public sealed class AudioVoices:MonoBehaviour {
 public const int Voices=8;public const float Repeat=.06f;
 sealed class Voice { public AudioSource source;public int priority;public AudioClip clip;public float started; }
 readonly Voice[] voices=new Voice[Voices];readonly System.Random random=new System.Random(5);float clock;
 public float Volume=1;
 public int Playing{get{int n=0;foreach(var v in voices)if(v!=null&&v.source.isPlaying)n++;return n;}}
 void Awake(){for(int i=0;i<Voices;i++){var s=gameObject.AddComponent<AudioSource>();s.playOnAwake=false;s.spatialBlend=0;voices[i]=new Voice{source=s};}}
 void Update()=>clock+=Time.unscaledDeltaTime;
 /// <summary>Plays a clip; pan is −1 (left) to 1. Returns false when it was dropped.</summary>
 public bool Play(AudioClip clip,float volume,float pan=0,int priority=0,float pitch=1){if(clip==null||Volume<=0||volume<=0)return false;Voice free=null;
  foreach(var v in voices){if(v.source.isPlaying&&v.clip==clip&&clock-v.started<Repeat)return false;if(!v.source.isPlaying&&free==null)free=v;}
  if(free==null)foreach(var v in voices)if(v.priority<=priority&&(free==null||v.priority<free.priority||v.priority==free.priority&&v.started<free.started))free=v;
  if(free==null)return false;var s=free.source;s.Stop();s.clip=clip;s.volume=Mathf.Clamp01(volume*Volume);s.panStereo=Mathf.Clamp(pan,-1,1);s.pitch=pitch*(1+((float)random.NextDouble()-.5f)*.08f);s.Play();free.clip=clip;free.priority=priority;free.started=clock;return true;}
 public void StopAll(){foreach(var v in voices)v.source.Stop();}
}
}
