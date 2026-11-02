using UnityEngine;

namespace Veilbreak.ARHud
{
    [CreateAssetMenu(menuName = "Veilbreak/AR HUD/Effects", fileName = "HudEffects")]
    public sealed class HudEffects : ScriptableObject
    {
        [Header("Comfort / performance")]
        public bool reducedMotion;
        [Range(10, 60)] public int framesPerSecond = 30;
        [Header("Visor chrome")]
        [Range(0, 1)] public float curvature = .45f;
        [Range(0, 1)] public float edgeOpacity = .65f;
        [Range(0, 1)] public float glow = .25f;
        [Range(0, 12)] public float parallax = 4;
        [Header("Decorative animation only")]
        [Range(0, .25f)] public float scanlineOpacity = .035f;
        [Range(4, 32)] public float scanlineSpacing = 10;
        [Range(0, .25f)] public float pulse = .04f;
        [Range(0, 1)] public float sweep = .12f;
        [Range(0, .25f)] public float glitch = 0;
        /// <summary>The profile a <see cref="HudHost"/>, <see cref="HudVisor"/> or preview without its own uses (installed by the app at boot and by its editor code for previews).</summary>
        public static HudEffects Default { get; set; }
        public int Revision { get; private set; }
        public void NotifyChanged() => Revision++;
        private void OnValidate() => NotifyChanged();
    }
}
