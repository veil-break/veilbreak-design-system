using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

namespace Veilbreak.Rendering
{
    // Bends the main camera's image through the visor's barrel curve (VisorLens) after post-processing, so the world is seen
    // through the same curved glass the UI is projected on. Add it only to renderers that should bend (below
    // VisorLens.FirstQuality the mapping is the identity anyway); only MainCamera-tagged game cameras bend, so cameras that
    // render into textures stay flat.
    public sealed class VisorLensFeature : ScriptableRendererFeature
    {
        [SerializeField] private Shader shader;
        private LensPass pass;

        public override void Create()
        {
            pass?.Dispose();
            pass = new LensPass(shader) { renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData data)
        {
            if (data.cameraData.cameraType != CameraType.Game || !data.cameraData.postProcessEnabled) return;
            if (!data.cameraData.camera.CompareTag("MainCamera") || VisorLens.Strength <= 0 || !pass.Ready) return;
            renderer.EnqueuePass(pass);
        }

        protected override void Dispose(bool disposing) => pass?.Dispose();

        // One blit from the camera colour into a copy through the curve, which then becomes the camera colour.
        private sealed class LensPass : ScriptableRenderPass
        {
            private readonly Material material;

            public LensPass(Shader shader)
            {
                if (shader != null) material = CoreUtils.CreateEngineMaterial(shader);
            }

            public bool Ready => material != null;
            public void Dispose() => CoreUtils.Destroy(material);

            public override void RecordRenderGraph(RenderGraph graph, ContextContainer frame)
            {
                var resources = frame.Get<UniversalResourceData>();
                if (resources.isActiveTargetBackBuffer) return;
                var source = resources.activeColorTexture;
                var desc = graph.GetTextureDesc(source);
                desc.name = "visor lens"; desc.clearBuffer = false; desc.msaaSamples = MSAASamples.None; desc.depthBufferBits = DepthBits.None;
                desc.filterMode = FilterMode.Bilinear; desc.wrapMode = TextureWrapMode.Clamp;
                var bent = graph.CreateTexture(desc);
                var block = new MaterialPropertyBlock(); block.SetFloat(RenderIds.VisorBarrel, VisorLens.Strength);
                graph.AddBlitPass(new RenderGraphUtils.BlitMaterialParameters(source, bent, material, 0) { propertyBlock = block }, "Visor lens");
                resources.cameraColor = bent;
            }
        }
    }
}
