using UnityEngine;

namespace Veilbreak.Rendering
{
    // Shader.PropertyToID for the properties and globals the package's shaders read.
    internal static class RenderIds
    {
        public static readonly int CenterFade = Shader.PropertyToID("_CenterFade");
        public static readonly int Direction = Shader.PropertyToID("_Direction");
        public static readonly int Falloff = Shader.PropertyToID("_Falloff");
        public static readonly int StreakB = Shader.PropertyToID("_StreakB");
        public static readonly int Stride = Shader.PropertyToID("_Stride");
        public static readonly int Texel = Shader.PropertyToID("_Texel");
        public static readonly int Threshold = Shader.PropertyToID("_Threshold");
        public static readonly int TintA = Shader.PropertyToID("_TintA");
        public static readonly int TintB = Shader.PropertyToID("_TintB");
        public static readonly int Unit = Shader.PropertyToID("_Unit");
        public static readonly int VeilGlitch = Shader.PropertyToID("_VeilGlitch");
        public static readonly int VeilGlitchFlash = Shader.PropertyToID("_VeilGlitchFlash");
        public static readonly int VisorBarrel = Shader.PropertyToID("_VisorBarrel");
    }
}
