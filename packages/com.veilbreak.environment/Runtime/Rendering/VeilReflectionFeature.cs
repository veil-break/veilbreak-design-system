using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;
namespace Veilbreak.Cityscape {
/// <summary>The wet street's mirror, cheap enough for phones: before opaques, the registered emissive submeshes
/// (<see cref="ReflectionSet"/>) are drawn additively through their VeilReflection pass with the camera mirrored in the
/// ground plane at a fraction of screen resolution. Lights smear vertically; solid actors composite afterward with depth.
/// Both tiers can register the feature; without it the mirror stays black and streets reflect the painted sky.</summary>
public sealed class VeilReflectionFeature:ScriptableRendererFeature {
 public Shader blur;[Range(2,8)] public int downsample=4;[Range(0,.3f)] public float spread=.12f;
 Pass pass;
 public override void Create(){pass?.Dispose();pass=new Pass(blur){renderPassEvent=RenderPassEvent.BeforeRenderingOpaques};}
 public override void AddRenderPasses(ScriptableRenderer renderer,ref RenderingData data){var type=data.cameraData.cameraType;if(pass.Ready&&ReflectionSet.Entries.Count>0&&(type==CameraType.Game||type==CameraType.SceneView)){pass.downsample=downsample;pass.spread=spread;renderer.EnqueuePass(pass);}}
 protected override void Dispose(bool disposing)=>pass?.Dispose();

 sealed class Pass:ScriptableRenderPass {
  static readonly int ReflectionTex=Shader.PropertyToID("_VeilReflectionTex"),MirrorClip=Shader.PropertyToID("_VeilMirrorClip"),MirrorVP=Shader.PropertyToID("_VeilMirrorVP"),Spread=Shader.PropertyToID("_Spread");
  static readonly Matrix4x4 Mirror=Matrix4x4.Scale(new Vector3(1,-1,1));
  readonly Material material;public int downsample=4;public float spread=.12f;public bool Ready=>material!=null;
  sealed class DrawData {public Matrix4x4 clip,vp;}
  sealed class BlurData {public TextureHandle source;public Material material;public float spread;}
  public Pass(Shader shader){if(shader!=null)material=CoreUtils.CreateEngineMaterial(shader);}
  public void Dispose()=>CoreUtils.Destroy(material);
  static GraphicsFormat Format(){if(SystemInfo.IsFormatSupported(GraphicsFormat.R16G16B16A16_SFloat,GraphicsFormatUsage.Render))return GraphicsFormat.R16G16B16A16_SFloat;return GraphicsFormat.R8G8B8A8_UNorm;}
  static void Draw(DrawData d,RasterGraphContext context,bool solid){context.cmd.SetGlobalMatrix(MirrorClip,d.clip);context.cmd.SetGlobalMatrix(MirrorVP,d.vp);
   var entries=ReflectionSet.Entries;for(int i=0;i<entries.Count;i++){var e=entries[i];if(e.solid==solid&&e.renderer!=null&&e.renderer.enabled&&e.renderer.gameObject.activeInHierarchy)context.cmd.DrawRenderer(e.renderer,e.material,e.submesh,e.pass);}}
  public override void RecordRenderGraph(RenderGraph graph,ContextContainer frame){
   var camera=frame.Get<UniversalCameraData>();var size=camera.cameraTargetDescriptor;
   var desc=new TextureDesc(Mathf.Max(1,size.width/downsample),Mathf.Max(1,size.height/downsample)){colorFormat=Format(),name="Veil reflection",clearBuffer=true,clearColor=Color.clear,filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};
   var mirror=graph.CreateTexture(desc);desc.name="Veil reflection blurred";desc.clearBuffer=false;var blurred=graph.CreateTexture(desc);
   var view=camera.GetViewMatrix();var projection=camera.GetProjectionMatrix();
   using(var builder=graph.AddRasterRenderPass<DrawData>("Veil reflection",out var data)){
    data.clip=GL.GetGPUProjectionMatrix(projection,true)*view*Mirror;data.vp=projection*view;
    builder.SetRenderAttachment(mirror,0,AccessFlags.Write);builder.AllowGlobalStateModification(true);builder.AllowPassCulling(false);
    builder.SetRenderFunc(static (DrawData d,RasterGraphContext context)=>Draw(d,context,false));
   }
   using(var builder=graph.AddRasterRenderPass<BlurData>("Veil reflection blur",out var data)){
    data.source=mirror;data.material=material;data.spread=spread;builder.UseTexture(mirror,AccessFlags.Read);builder.SetRenderAttachment(blurred,0,AccessFlags.Write);
    builder.AllowGlobalStateModification(true);builder.AllowPassCulling(false);
    builder.SetRenderFunc(static (BlurData d,RasterGraphContext context)=>{context.cmd.SetGlobalFloat(Spread,d.spread);Blitter.BlitTexture(context.cmd,d.source,new Vector4(1,1,0,0),d.material,0);});
   }
   var depth=graph.CreateTexture(new TextureDesc(desc.width,desc.height){name="Actor reflection depth",colorFormat=GraphicsFormat.None,depthBufferBits=DepthBits.Depth24,clearBuffer=true});
   using(var builder=graph.AddRasterRenderPass<DrawData>("Veil actor reflections",out var data)){
    data.clip=GL.GetGPUProjectionMatrix(projection,true)*view*Mirror;data.vp=projection*view;
    builder.SetRenderAttachment(blurred,0,AccessFlags.ReadWrite);builder.SetRenderAttachmentDepth(depth,AccessFlags.ReadWrite);
    builder.AllowGlobalStateModification(true);builder.SetGlobalTextureAfterPass(blurred,ReflectionTex);builder.AllowPassCulling(false);
    builder.SetRenderFunc(static (DrawData d,RasterGraphContext context)=>Draw(d,context,true));
   }
  }
 }
}
}
