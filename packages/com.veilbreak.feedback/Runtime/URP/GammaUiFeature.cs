using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;
namespace Veilbreak.Feedback {
/// <summary>Two explicitly tracked inputs: post-processed world and gamma UI. No global shader state.</summary>
public sealed class GammaUiFeature:ScriptableRendererFeature {
 public Shader shader;Pass pass;
 public override void Create(){pass?.Dispose();pass=new Pass(shader){renderPassEvent=RenderPassEvent.AfterRenderingPostProcessing};}
 public override void AddRenderPasses(ScriptableRenderer renderer,ref RenderingData data){if(pass.Ready&&GammaUiPanel.Active?.Surface!=null&&data.cameraData.cameraType==CameraType.Game)renderer.EnqueuePass(pass);}
 protected override void Dispose(bool disposing)=>pass?.Dispose();
 sealed class Pass:ScriptableRenderPass {
  readonly Material material;public bool Ready=>material!=null;static readonly int UiTexture=Shader.PropertyToID("_GammaUi");
  sealed class Data {public TextureHandle world,ui;public Material material;public MaterialPropertyBlock properties;}
  public Pass(Shader shader){if(shader!=null)material=CoreUtils.CreateEngineMaterial(shader);ConfigureInput(ScriptableRenderPassInput.Color);}
  public void Dispose()=>CoreUtils.Destroy(material);
  public override void RecordRenderGraph(RenderGraph graph,ContextContainer frame){var resources=frame.Get<UniversalResourceData>();if(resources.isActiveTargetBackBuffer||GammaUiPanel.Active?.Surface==null)return;var source=resources.activeColorTexture;var desc=graph.GetTextureDesc(source);desc.name="World and gamma UI";desc.width=GammaUiPanel.Active.Surface.rt.width;desc.height=GammaUiPanel.Active.Surface.rt.height;desc.clearBuffer=false;desc.msaaSamples=MSAASamples.None;desc.depthBufferBits=DepthBits.None;var target=graph.CreateTexture(desc);using(var builder=graph.AddRasterRenderPass<Data>("Gamma UI composition",out var data)){data.world=source;data.ui=graph.ImportTexture(GammaUiPanel.Active.Surface);data.material=material;data.properties=new MaterialPropertyBlock();builder.UseTexture(data.world,AccessFlags.Read);builder.UseTexture(data.ui,AccessFlags.Read);builder.SetRenderAttachment(target,0,AccessFlags.Write);builder.SetRenderFunc(static (Data d,RasterGraphContext context)=>{d.properties.SetTexture(UiTexture,d.ui);d.properties.SetTexture("_BlitTexture",d.world);d.properties.SetVector("_BlitScaleBias",new Vector4(1,1,0,0));context.cmd.DrawProcedural(Matrix4x4.identity,d.material,0,MeshTopology.Triangles,3,1,d.properties);});}resources.cameraColor=target;}
 }
}
}
