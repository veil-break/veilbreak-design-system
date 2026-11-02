using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Veilbreak.Rendering
{
    // Draws the Glitch volume override through URP's Render Graph full-screen pass, skipping its colour copy and draw entirely
    // while the blended intensity is zero or the reduced-motion / reduce-flashes settings are on (ScreenGlitch.Still, ScreenGlitch.NoFlashes).
    public sealed class ScreenGlitchFeature : FullScreenPassRendererFeature
    {
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData data)
        {
            if (ScreenGlitch.Still || ScreenGlitch.NoFlashes) return;
            if (!data.cameraData.postProcessEnabled || data.cameraData.cameraType != CameraType.Game) return;
            var glitch = VolumeManager.instance.stack.GetComponent<Glitch>();
            if (glitch == null || !glitch.IsActive() || passMaterial == null) return;
            Shader.SetGlobalFloat(RenderIds.VeilGlitch, glitch.intensity.value); Shader.SetGlobalFloat(RenderIds.VeilGlitchFlash, glitch.flash.value);
            base.AddRenderPasses(renderer, ref data);
        }
    }
}
