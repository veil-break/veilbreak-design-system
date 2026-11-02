using System;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace Veilbreak.Feedback {
[Serializable,VolumeComponentMenu("Veilbreak/Cinematic Pulse")]
public sealed class CinematicPulse:VolumeComponent,IPostProcessComponent {
 public ClampedFloatParameter strength=new ClampedFloatParameter(0,0,1);
 public bool IsActive()=>active&&strength.value>.001f;
 public bool IsTileCompatible()=>false;
}
}
