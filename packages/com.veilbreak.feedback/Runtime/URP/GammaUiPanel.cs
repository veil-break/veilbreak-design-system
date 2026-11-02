using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
namespace Veilbreak.Feedback {
/// <summary>Optional full-resolution gamma UI surface, composited by GammaUiFeature after world post effects.</summary>
[RequireComponent(typeof(UIDocument))] public sealed class GammaUiPanel:MonoBehaviour {
 public static GammaUiPanel Active {get;private set;}
 public RTHandle Surface {get;private set;}
 UIDocument document;PanelSettings original,panel;RenderTexture texture;
 void Awake(){document=GetComponent<UIDocument>();original=document.panelSettings;panel=Instantiate(original);panel.name=original.name+" (gamma surface)";panel.forceGammaRendering=true;panel.clearColor=true;panel.colorClearValue=Color.clear;document.panelSettings=panel;Active=this;Resize(Screen.width,Screen.height);}
 void LateUpdate(){var camera=Camera.main;var target=camera!=null?camera.targetTexture:null;Resize(target!=null?target.width:Screen.width,target!=null?target.height:Screen.height);panel.targetTexture=texture;}
 void Resize(int width,int height){width=Mathf.Max(1,width);height=Mathf.Max(1,height);if(texture!=null&&texture.width==width&&texture.height==height)return;Release();texture=new RenderTexture(new RenderTextureDescriptor(width,height){graphicsFormat=GraphicsFormat.R8G8B8A8_UNorm,depthBufferBits=0,msaaSamples=1}){name="Gamma UI",filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};texture.Create();Surface=RTHandles.Alloc(texture);panel.targetTexture=texture;}
 void Release(){Surface?.Release();Surface=null;if(texture!=null){texture.Release();Destroy(texture);texture=null;}}
 void OnDestroy(){if(Active==this)Active=null;if(document!=null&&document.panelSettings==panel)document.panelSettings=original;Release();if(panel!=null)Destroy(panel);}
}
}
