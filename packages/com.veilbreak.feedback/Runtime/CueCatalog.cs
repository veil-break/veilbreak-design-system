using System;
using UnityEngine;
namespace Veilbreak.Feedback {
/// <summary>The set piece a cue plays through <see cref="EffectEmitters.Play"/>. Stored as an integer in cue assets: add new routines at the end.</summary>
public enum EffectRoutine { None, Impact, Explosion, Muzzle, Beam, Cone, Zone, Dash, Wall, Mend, Ward, Charge, Portal, Pulse, Reward, Swap, Splash }
/// <summary>The ground mark a cue leaves through <see cref="GroundMarks"/>.</summary>
public enum MarkKind { None, Scorch, Crack }
[CreateAssetMenu(menuName="Veilbreak/Feedback catalog")]
public sealed class CueCatalog:ScriptableObject {
 /// <summary>One row: what a presentation key plays. Keys are event kinds (<c>hit</c>), skill shapes (<c>burst</c>) or per-skill rows (<c>skill:aria.basic</c>); <see cref="Find(string,string,string)"/> takes the most specific.
 /// Priority decides which effects survive a full budget (2 and above always play); pulse weights the consumer's flash volumes.</summary>
 [Serializable] public sealed class Cue {
  public string key;public float size=.08f,duration=.25f,pulse;public int priority;
  public EffectRoutine routine;[Tooltip("Set-piece strength: 1 normal, above 1 heavy.")] public float magnitude=1;
  [Tooltip("Replaces the event colour (team, kit, heal) when alpha is above zero.")] public Color tint=Color.clear;
  [Range(0,1)] public float shake;[Tooltip("Peak intensity of a pooled point-light flash; 0 for none.")] public float light;
  public MarkKind mark;public float markSize=1.5f;
  public AudioClip sound;[Range(0,1)] public float volume=.5f;
 }
 public Cue[] cues=Array.Empty<Cue>();
 /// <summary>Material for <see cref="GroundTelegraphs"/>; referencing it here keeps the shader in player builds.</summary>
 public Material telegraph;
 [Tooltip("Particle materials for EffectEmitters and GroundMarks (FeedbackParticle.shader with T_FxAtlas).")] public Material additive,alpha,marks;
 [Tooltip("Opaque emissive material a consumer swaps onto a body for a hit flash.")] public Material hitFlash;
 public Cue Find(string key)=>key==null?null:Array.Find(cues,x=>x.key==key);
 /// <summary>The first row that exists of the given keys, most specific first.</summary>
 public Cue Find(string first,string second,string third=null)=>Find(first)??Find(second)??Find(third);
}
}
