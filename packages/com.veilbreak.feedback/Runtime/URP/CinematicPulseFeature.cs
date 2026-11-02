using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;
namespace Veilbreak.Feedback {
public sealed class CinematicPulseFeature:ScriptableRendererFeature {
 public Shader shader;Pass pass;
 public override void Create(){pass?.Dispose();pass=new Pass(shader){renderPassEvent=RenderPassEvent.BeforeRenderingPostProcessing};}
 // Runs while the pulse volume is weighted in or a screen impact (ScreenImpacts) is still playing.
 public override void AddRenderPasses(ScriptableRenderer renderer,ref RenderingData data){var volume=VolumeManager.instance.stack.GetComponent<CinematicPulse>();bool pulse=volume!=null&&volume.IsActive();if((pulse||ScreenImpacts.Active)&&pass.Ready&&data.cameraData.cameraType==CameraType.Game&&data.cameraData.postProcessEnabled)renderer.EnqueuePass(pass);}
 protected override void Dispose(bool disposing)=>pass?.Dispose();
 sealed class Pass:ScriptableRenderPass {
 readonly Material material;static readonly int Pulse=Shader.PropertyToID("_Pulse"),Centres=Shader.PropertyToID("_Impacts"),Colors=Shader.PropertyToID("_ImpactColors");public bool Ready=>material!=null;
 public Pass(Shader shader){if(shader!=null)material=CoreUtils.CreateEngineMaterial(shader);ConfigureInput(ScriptableRenderPassInput.Color);}
 public void Dispose()=>CoreUtils.Destroy(material);
 public override void RecordRenderGraph(RenderGraph graph,ContextContainer frame){var resources=frame.Get<UniversalResourceData>();if(resources.isActiveTargetBackBuffer)return;var source=resources.activeColorTexture;var desc=graph.GetTextureDesc(source);desc.name="Cinematic accents";desc.clearBuffer=false;desc.msaaSamples=MSAASamples.None;desc.depthBufferBits=DepthBits.None;var target=graph.CreateTexture(desc);var block=new MaterialPropertyBlock();var volume=VolumeManager.instance.stack.GetComponent<CinematicPulse>();block.SetFloat(Pulse,volume!=null&&volume.IsActive()?volume.strength.value:0);ScreenImpacts.Fill(out var centres,out var colors);block.SetVectorArray(Centres,centres);block.SetVectorArray(Colors,colors);graph.AddBlitPass(new RenderGraphUtils.BlitMaterialParameters(source,target,material,0){propertyBlock=block},"Cinematic streaks");resources.cameraColor=target;}
 }
}
}
