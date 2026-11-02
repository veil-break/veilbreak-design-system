using System;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace Veilbreak.Feedback {
/// <summary>World-only overlay coverage. The UI compositor runs afterwards so foreground text stays sharp.</summary>
[Serializable,VolumeComponentMenu("Veilbreak/Overlay Blur")]
public sealed class OverlayBlur:VolumeComponent,IPostProcessComponent {
 public ClampedFloatParameter strength=new ClampedFloatParameter(0,0,1);
 public bool IsActive()=>strength.value>.001f;
}
}
