using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

namespace Veilbreak.Rendering
{
    // Draws the LensStreaks volume effect (add it to each renderer that should streak and assign Hidden/Veilbreak/Streaks).
    public sealed class LensStreakFeature : ScriptableRendererFeature
    {
        [SerializeField] private Shader shader;
        private StreakPass pass;

        public override void Create()
        {
            pass?.Dispose();
            pass = new StreakPass(shader) { renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData data)
        {
            if (data.cameraData.cameraType != CameraType.Game && data.cameraData.cameraType != CameraType.SceneView) return;
            if (!data.cameraData.postProcessEnabled) return;
            var settings = VolumeManager.instance.stack.GetComponent<LensStreaks>();
            if (settings == null || !settings.IsActive() || !pass.Ready) return;
            renderer.EnqueuePass(pass);
        }

        protected override void Dispose(bool disposing) => pass?.Dispose();

        // Prefilter to quarter resolution, three widening streak iterations per direction (each sums 15 taps at a growing
        // stride, Kawase style, so the streak runs most of the screen), then both add back over the scene, tinted, in one pass.
        private sealed class StreakPass : ScriptableRenderPass
        {
            private readonly Material material;
            private static readonly float[] Strides = { 1f, 8f, 64f };

            public StreakPass(Shader shader)
            {
                if (shader != null) material = CoreUtils.CreateEngineMaterial(shader);
            }

            public bool Ready => material != null;
            public void Dispose() => CoreUtils.Destroy(material);

            public override void RecordRenderGraph(RenderGraph graph, ContextContainer frame)
            {
                var resources = frame.Get<UniversalResourceData>();
                if (resources.isActiveTargetBackBuffer) return;
                var settings = VolumeManager.instance.stack.GetComponent<LensStreaks>();
                var source = resources.activeColorTexture;
                var desc = graph.GetTextureDesc(source);
                // A quarter across but half down, so the stripes stay thin; _Unit steps 4 screen pixels on both axes, so angles stay true.
                var unit = new Vector4(4f / desc.width, 4f / desc.height, 0, 0);
                desc.width = Mathf.Max(1, desc.width / 4); desc.height = Mathf.Max(1, desc.height / 2);
                desc.msaaSamples = MSAASamples.None; desc.clearBuffer = false; desc.depthBufferBits = DepthBits.None; desc.filterMode = FilterMode.Bilinear; desc.wrapMode = TextureWrapMode.Clamp;
                desc.name = "lens streak bright"; var bright = graph.CreateTexture(desc);
                var texel = new Vector4(1f / desc.width, 1f / desc.height, 0, 0);
                // Graph execution is deferred. Each recording owns its parameters, including when
                // Scene and Game cameras record before either graph has executed.
                MaterialPropertyBlock Next() { var p = new MaterialPropertyBlock(); p.SetVector(RenderIds.Texel, texel); p.SetVector(RenderIds.Unit, unit); return p; }

                var pre = Next(); pre.SetFloat(RenderIds.Threshold, settings.threshold.value); pre.SetVector(RenderIds.CenterFade, new Vector4(settings.centerFade.value, settings.centerRadius.value, 0, 0));
                graph.AddBlitPass(new RenderGraphUtils.BlitMaterialParameters(source, bright, material, 0) { propertyBlock = pre }, "Lens streak prefilter");

                var tilt = settings.angle.value * Mathf.Deg2Rad;
                TextureHandle Run(Vector2 dir, string name)
                {
                    desc.name = name + " a"; var x = graph.CreateTexture(desc); desc.name = name + " b"; var y = graph.CreateTexture(desc);
                    var from = bright; var to = x;
                    foreach (var stride in Strides)
                    {
                        var p = Next(); p.SetVector(RenderIds.Direction, dir); p.SetFloat(RenderIds.Stride, stride); p.SetFloat(RenderIds.Falloff, Mathf.Pow(settings.length.value, stride));
                        graph.AddBlitPass(new RenderGraphUtils.BlitMaterialParameters(from, to, material, 1) { propertyBlock = p }, "Lens streak");
                        from = to; to = to.Equals(x) ? y : x;
                    }
                    return from;
                }
                var level = settings.intensity.value;
                var horizontal = Run(new Vector2(1, 0), "lens streak horizontal");
                var tilted = Run(new Vector2(Mathf.Cos(tilt), Mathf.Sin(tilt)), "lens streak tilted");

                // Additive over the camera colour: a raster pass that keeps (ReadWrite) what is already there.
                using var builder = graph.AddRasterRenderPass<CompositeData>("Lens streak composite", out var data);
                data.material = material; data.horizontal = horizontal; data.tilted = tilted;
                data.tintA = settings.tint.value * level; data.tintB = settings.tint.value * (level * settings.secondary.value);
                builder.UseTexture(horizontal); builder.UseTexture(tilted);
                builder.SetRenderAttachment(source, 0, AccessFlags.ReadWrite);
                builder.SetRenderFunc(static (CompositeData d, RasterGraphContext context) =>
                {
                    d.material.SetColor(RenderIds.TintA, d.tintA); d.material.SetColor(RenderIds.TintB, d.tintB);
                    d.material.SetTexture(RenderIds.StreakB, d.tilted);
                    Blitter.BlitTexture(context.cmd, d.horizontal, new Vector4(1, 1, 0, 0), d.material, 2);
                });
            }

            private sealed class CompositeData { public Material material; public TextureHandle horizontal, tilted; public Color tintA, tintB; }
        }
    }
}

