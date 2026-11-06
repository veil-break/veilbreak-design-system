using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
namespace Veilbreak.Gui {
/// <summary>Clips content to the cut-corner shape it sits in. UI Toolkit's <c>overflow: hidden</c> clips to a rectangle, so a portrait or a charge bar that reaches a chamfered corner (the top-left cut of a battle card or
/// tile, the bottom-right cut of a skill button) used to overflow the cut. A <see cref="VbClipFill"/> or <see cref="VbClipImage"/> paints itself as a polygon: its own rectangle clipped against the outline of the nearest
/// ancestor painted by <see cref="VbShape"/> (its <c>--vb-cut</c> and <c>--vb-cut-side</c>), so the corner stays clean. Everything else about the element (layout, overflow of its parents, picking) is unchanged.</summary>
public static class VbClip {
 static readonly CustomStyleProperty<float> CutP=new CustomStyleProperty<float>("--vb-cut"),SideP=new CustomStyleProperty<float>("--vb-cut-side");
 /// <summary>The nearest ancestor of <paramref name="e"/> painted by <see cref="VbShape"/>, or null.</summary>
 public static VisualElement Host(VisualElement e){for(var p=e.hierarchy.parent;p!=null;p=p.hierarchy.parent)if(p.ClassListContains(VbShape.ClassName))return p;return null;}
 /// <summary>The convex polygon <paramref name="subject"/> (a rectangle's corners, clockwise in UI units) clipped to <paramref name="shape"/> (a convex outline from <see cref="VbShape.Outline(float,float,float,float,int)"/>), Sutherland–Hodgman.
 /// The two must be in the same space; the result may be empty.</summary>
 public static List<Vector2> Intersect(IReadOnlyList<Vector2> subject,IReadOnlyList<Vector2> shape){
  var output=new List<Vector2>(subject);
  for(int i=0;i<shape.Count&&output.Count>0;i++){
   Vector2 a=shape[i],b=shape[(i+1)%shape.Count];var input=output;output=new List<Vector2>(input.Count+2);
   for(int j=0;j<input.Count;j++){
    Vector2 p=input[j],q=input[(j+1)%input.Count];float sp=Side(a,b,p),sq=Side(a,b,q);
    if(sp>=0){output.Add(p);if(sq<0)output.Add(Cross(a,b,p,q));}else if(sq>=0)output.Add(Cross(a,b,p,q));
   }
  }
  return output;
 }
 // Positive on the inside of an edge of a clockwise (y down) outline.
 static float Side(Vector2 a,Vector2 b,Vector2 p)=>(b.x-a.x)*(p.y-a.y)-(b.y-a.y)*(p.x-a.x);
 static Vector2 Cross(Vector2 a,Vector2 b,Vector2 p,Vector2 q){float sp=Side(a,b,p),sq=Side(a,b,q);float t=sp/(sp-sq);return p+(q-p)*t;}
 /// <summary>The outline of a shaped <paramref name="host"/> in its own coordinates: its layout size and its <c>--vb-cut</c> and <c>--vb-cut-side</c>, as <see cref="VbShape"/> paints it.</summary>
 public static Vector2[] HostOutline(VisualElement host){
  float w=host.layout.width,h=host.layout.height;if(!(w>0)||!(h>0))return null;
  float cut=host.customStyle.TryGetValue(CutP,out var c)?c:10;int side=host.customStyle.TryGetValue(SideP,out var s)?(int)s:0;
  return VbShape.Outline(w,h,Mathf.Min(cut,w/3,h/3),0,side);
 }
 /// <summary>The rectangle <paramref name="local"/> of <paramref name="e"/> (in its own coordinates) clipped to the outline of its shaped host, as polygon points in <paramref name="e"/>'s coordinates; with no host the rectangle itself.</summary>
 public static List<Vector2> Polygon(VisualElement e,Rect local){
  var rect=new List<Vector2>{new Vector2(local.xMin,local.yMin),new Vector2(local.xMax,local.yMin),new Vector2(local.xMax,local.yMax),new Vector2(local.xMin,local.yMax)};
  var host=Host(e);var outline=host==null?null:HostOutline(host);if(outline==null)return rect;
  for(int i=0;i<rect.Count;i++)rect[i]=host.WorldToLocal(e.LocalToWorld(rect[i]));
  var clipped=Intersect(rect,outline);
  for(int i=0;i<clipped.Count;i++)clipped[i]=e.WorldToLocal(host.LocalToWorld(clipped[i]));
  return clipped;
 }
 internal static void Watch(VisualElement e){
  e.RegisterCallback<GeometryChangedEvent>(_=>e.MarkDirtyRepaint());e.RegisterCallback<CustomStyleResolvedEvent>(_=>e.MarkDirtyRepaint());
  VisualElement host=null;EventCallback<GeometryChangedEvent> again=_=>e.MarkDirtyRepaint();
  e.RegisterCallback<AttachToPanelEvent>(_=>{host=Host(e);host?.RegisterCallback(again);e.MarkDirtyRepaint();});
  e.RegisterCallback<DetachFromPanelEvent>(_=>{host?.UnregisterCallback(again);host=null;});
 }
 internal static void Fan(MeshGenerationContext ctx,IReadOnlyList<Vector2> pts,Color tint,Texture texture,Rect uvRect){
  int n=pts.Count;if(n<3)return;var mesh=texture==null?ctx.Allocate(n,(n-2)*3):ctx.Allocate(n,(n-2)*3,texture);var region=texture==null?new Rect(0,0,1,1):mesh.uvRegion;
  for(int i=0;i<n;i++){var p=pts[i];float u=uvRect.width>0?(p.x-uvRect.xMin)/uvRect.width:0,v=uvRect.height>0?(p.y-uvRect.yMin)/uvRect.height:0;
   mesh.SetNextVertex(new Vertex{position=new Vector3(p.x,p.y,Vertex.nearZ),tint=tint,uv=new Vector2(region.xMin+u*region.width,region.yMin+(1-v)*region.height)});}
  for(int i=1;i<n-1;i++){mesh.SetNextIndex(0);mesh.SetNextIndex((ushort)i);mesh.SetNextIndex((ushort)(i+1));}
 }
}

/// <summary>A solid block that follows the cut of the shape it sits in: its colour is <c>--vb-fill</c> (set it on the element's own class; it inherits the host's otherwise) and it never paints outside the host's chamfers.
/// A skill button's charge and windup sweep are these, so they do not overflow the bottom-right cut.</summary>
[UxmlElement] public partial class VbClipFill:VisualElement {
 public const string ClassName="vb-clip-fill";
 static readonly CustomStyleProperty<Color> FillP=new CustomStyleProperty<Color>("--vb-fill");
 Color fill=Color.clear;
 public VbClipFill(){AddToClassList(ClassName);pickingMode=PickingMode.Ignore;VbClip.Watch(this);RegisterCallback<CustomStyleResolvedEvent>(_=>{fill=customStyle.TryGetValue(FillP,out var c)?c:Color.clear;});generateVisualContent+=Paint;}
 void Paint(MeshGenerationContext ctx){float w=layout.width,h=layout.height;if(!(w>0)||!(h>0)||fill.a<=0)return;VbClip.Fan(ctx,VbClip.Polygon(this,new Rect(0,0,w,h)),fill,null,new Rect(0,0,w,h));}
}

/// <summary>A picture that follows the cut of the shape it sits in: scaled to fit its box (a portrait), and clipped to the host's chamfers, so a face never overflows the top-left cutout of a card or tile.
/// Set <see cref="Image"/> (or call <see cref="Ui.Image"/>); the element's size and offset come from its stylesheet as for any art box.</summary>
[UxmlElement] public partial class VbClipImage:VisualElement {
 public const string ClassName="vb-clip-image";
 Texture2D image;
 public VbClipImage(){AddToClassList(ClassName);pickingMode=PickingMode.Ignore;VbClip.Watch(this);generateVisualContent+=Paint;}
 public Texture2D Image {get=>image;set{if(image==value)return;image=value;MarkDirtyRepaint();}}
 /// <summary>The rectangle the picture covers when scaled to fit a <paramref name="w"/> × <paramref name="h"/> box (centred, the whole picture visible).</summary>
 public static Rect Fit(float w,float h,float imageWidth,float imageHeight){
  if(imageWidth<=0||imageHeight<=0)return new Rect(0,0,w,h);float scale=Mathf.Min(w/imageWidth,h/imageHeight),iw=imageWidth*scale,ih=imageHeight*scale;return new Rect((w-iw)*.5f,(h-ih)*.5f,iw,ih);
 }
 void Paint(MeshGenerationContext ctx){
  float w=layout.width,h=layout.height;if(image==null||!(w>0)||!(h>0))return;var area=Fit(w,h,image.width,image.height);
  VbClip.Fan(ctx,VbClip.Polygon(this,area),resolvedStyle.unityBackgroundImageTintColor,image,area);
 }
}
}
