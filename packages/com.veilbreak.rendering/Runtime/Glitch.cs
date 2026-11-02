using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Veilbreak.Rendering
{
    // The signal glitch as a Volume override, so it blends with the rest of the stack: ScreenGlitch's kicks raise the weight of
    // the host's glitch volume (ScreenGlitch.Changed) and any scene profile may hold a standing glitch. The
    // renderer's ScreenGlitchFeature draws the blended value through Veilbreak/Glitch.
    [Serializable, VolumeComponentMenu("Veilbreak/Glitch")]
    public sealed class Glitch : VolumeComponent, IPostProcessComponent
    {
        [Tooltip("Tearing and channel split, 0 leaves the image untouched.")]
        public ClampedFloatParameter intensity = new ClampedFloatParameter(0, 0, 1);
        [Tooltip("Brightness pop on the torn bands; the reduce-flashes setting skips the effect entirely.")]
        public ClampedFloatParameter flash = new ClampedFloatParameter(1, 0, 1);

        public bool IsActive() => intensity.value > 0;
    }
}
