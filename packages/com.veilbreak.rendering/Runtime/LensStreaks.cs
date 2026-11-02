using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Veilbreak.Rendering
{
    // Anamorphic lens streaks, the long stripe flares of a 35 mm video lens: a pale blue horizontal streak and a fainter
    // one tilted counter-clockwise (`angle`), drawn from the scene's HDR highlights, fainter the nearer the middle of the view they
    // start (`centerFade`, `centerRadius`). Set it on a Volume; the renderer's
    // LensStreakFeature draws it just before URP's post stack, so bloom, grading and grain apply on top.
    [Serializable, VolumeComponentMenu("Veilbreak/Lens Streaks")]
    public sealed class LensStreaks : VolumeComponent, IPostProcessComponent
    {
        public MinFloatParameter intensity = new MinFloatParameter(0, 0);
        [Tooltip("Scene luminance where highlights start to streak.")]
        public MinFloatParameter threshold = new MinFloatParameter(1.1f, 0);
        [Tooltip("Per-texel falloff along the streak: closer to 1 is longer.")]
        public ClampedFloatParameter length = new ClampedFloatParameter(.992f, .8f, .998f);
        public ColorParameter tint = new ColorParameter(new Color(.45f, .78f, 1.35f), true, false, true);
        [Tooltip("The second streak's tilt, degrees counter-clockwise from horizontal.")]
        public FloatParameter angle = new FloatParameter(40);
        [Tooltip("The tilted streak's strength relative to the horizontal one.")]
        public ClampedFloatParameter secondary = new ClampedFloatParameter(.45f, 0, 1);
        [Tooltip("How much fainter highlights in the middle of the view streak (1: not at all at the centre); flares gather toward the edges.")]
        public ClampedFloatParameter centerFade = new ClampedFloatParameter(.8f, 0, 1);
        [Tooltip("Distance from the centre (0) toward a corner (1) where highlights streak at full strength again.")]
        public ClampedFloatParameter centerRadius = new ClampedFloatParameter(.65f, .05f, 1);

        public bool IsActive() => intensity.value > 0;
    }
}
