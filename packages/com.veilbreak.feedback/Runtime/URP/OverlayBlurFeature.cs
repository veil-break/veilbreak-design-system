using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;
namespace Veilbreak.Feedback {
/// <summary>Bounded, separable half-resolution world blur after grading and before gamma UI composition.</summary>
public sealed class OverlayBlurFeature:ScriptableRendererFeature {
 public Shader shader;[Range(1,4)]public int downsample=2;Pass pass;
 public override void Create(){pass?.Dispose();pass=new Pass(shader){renderPassEvent=(RenderPassEvent)((int)RenderPassEvent.AfterRenderingPostProcessing-1)};}
 public override void AddRenderPasses(ScriptableRenderer renderer,ref RenderingData data){var volume=VolumeManager.instance.stack.GetComponent<OverlayBlur>();if(pass.Ready&&volume!=null&&volume.IsActive()&&data.cameraData.cameraType==CameraType.Game&&data.cameraData.postProcessEnabled){pass.downsample=Mathf.Clamp(downsample,1,4);renderer.EnqueuePass(pass);}}
 protected override void Dispose(bool disposing)=>pass?.Dispose();
 sealed class Pass:ScriptableRenderPass {
  readonly Material material;public int downsample=2;public bool Ready=>material!=null;
  public Pass(Shader shader){if(shader!=null)material=CoreUtils.CreateEngineMaterial(shader);ConfigureInput(ScriptableRenderPassInput.Color);}
  public void Dispose()=>CoreUtils.Destroy(material);
  public override void RecordRenderGraph(RenderGraph graph,ContextContainer frame){var resources=frame.Get<UniversalResourceData>();if(resources.isActiveTargetBackBuffer)return;var volume=VolumeManager.instance.stack.GetComponent<OverlayBlur>();if(volume==null||!volume.IsActive())return;var source=resources.activeColorTexture;var desc=graph.GetTextureDesc(source);desc.msaaSamples=MSAASamples.None;desc.depthBufferBits=DepthBits.None;desc.clearBuffer=false;desc.width=Mathf.Max(1,desc.width/downsample);desc.height=Mathf.Max(1,desc.height/downsample);desc.name="Overlay blur horizontal";var a=graph.CreateTexture(desc);desc.name="Overlay blur vertical";var b=graph.CreateTexture(desc);var horizontal=new MaterialPropertyBlock();horizontal.SetVector("_Direction",new Vector4(volume.strength.value*3/desc.width,0,0,0));horizontal.SetFloat("_Dim",1);var vertical=new MaterialPropertyBlock();vertical.SetVector("_Direction",new Vector4(0,volume.strength.value*3/desc.height,0,0));vertical.SetFloat("_Dim",1-.28f*volume.strength.value);graph.AddBlitPass(new RenderGraphUtils.BlitMaterialParameters(source,a,material,0){propertyBlock=horizontal},"Overlay blur horizontal");graph.AddBlitPass(new RenderGraphUtils.BlitMaterialParameters(a,b,material,0){propertyBlock=vertical},"Overlay blur vertical and dim");resources.cameraColor=b;}
 }
}
}
