using UnityEngine;
using UnityEngine.UIElements;
namespace Veilbreak.Gui {
/// <summary>The one owner of safe-area insets on a portrait board. It pads the root, insets absolute overlays (screen layers) by the
/// same amounts and keeps the root's responsive and accessibility classes. Call <see cref="Refresh"/> each frame; it only re-applies
/// when the safe area or resolution changed.</summary>
public sealed class SafeAreaRoot {
 public const string Compact="vb-compact",LargeText="vb-large-text",ReducedMotion="vb-reduced-motion",ReducedFlashes="vb-reduced-flashes",Contrast="vb-contrast";
 /// <summary>Every class <see cref="Flags"/> manages; popovers, which live on the panel root, copy them from their anchor.</summary>
 public static readonly string[] RootClasses={Compact,LargeText,ReducedMotion,ReducedFlashes,Contrast};
 /// <summary>Minimum top and bottom insets in panel units (status bar and home indicator room on devices that report none).</summary>
 public float MinTop=48,MinBottom=34;
 /// <summary>Boards narrower than this many panel units get <see cref="Compact"/>.</summary>
 public float CompactWidth=380;
 /// <summary>Whether <see cref="Flags"/> also sets the fluid scale's step class from the panel's width (<see cref="VbFluid"/>); on by default.</summary>
 public bool Fluid=true;
 readonly VisualElement root;readonly PanelSettings panel;readonly VisualElement[] overlays;Rect safe=new Rect(-1,-1,-1,-1);Vector2 size;
 public SafeAreaRoot(VisualElement root,PanelSettings panel,params VisualElement[] overlays){this.root=root;this.panel=panel;this.overlays=overlays;FontPrimer.Prime(root);}
 /// <summary>Left, top, right and bottom insets in panel units, as last applied.</summary>
 public Vector4 Insets {get;private set;}
 /// <summary>Re-applies the insets when <see cref="Screen.safeArea"/> or the resolution changed; true when it did.</summary>
 public bool Refresh(){var area=Screen.safeArea;var screen=new Vector2(Screen.width,Screen.height);if(area==safe&&screen==size)return false;Apply(area,screen);return true;}
 /// <summary>Insets for a safe area and screen size in pixels.</summary>
 public void Apply(Rect area,Vector2 screen){safe=area;size=screen;float unit=UnitsPerPixel(screen);
  float left=area.x*unit,right=(screen.x-area.xMax)*unit,top=Mathf.Max(MinTop,(screen.y-area.yMax)*unit),bottom=Mathf.Max(MinBottom,area.y*unit);Insets=new Vector4(left,top,right,bottom);
  root.style.paddingLeft=left;root.style.paddingTop=top;root.style.paddingRight=right;root.style.paddingBottom=bottom;
  foreach(var o in overlays){o.style.left=left;o.style.top=top;o.style.right=right;o.style.bottom=bottom;}}
 /// <summary>Panel units per screen pixel for the panel's scale mode (match width-or-height, constant pixel or physical size).</summary>
 public float UnitsPerPixel(Vector2 screen){if(panel==null)return 1;
  if(panel.scaleMode==PanelScaleMode.ScaleWithScreenSize){var reference=(Vector2)panel.referenceResolution;float w=Mathf.Log(Mathf.Max(1,screen.x)/Mathf.Max(1,reference.x),2),h=Mathf.Log(Mathf.Max(1,screen.y)/Mathf.Max(1,reference.y),2);return 1/Mathf.Pow(2,Mathf.Lerp(w,h,panel.match));}
  return panel.scaleMode==PanelScaleMode.ConstantPixelSize?1/Mathf.Max(.01f,panel.scale):1;}
 /// <summary>Sets the responsive and accessibility classes on the root: compact board by width, larger text, reduced motion, high contrast
 /// and reduced flashes (no glows, flicker, chromatic split or bright flashes), and the fluid step class for the panel's width.</summary>
 public void Flags(bool largeText,bool reducedMotion,bool contrast,bool reducedFlashes=false){float width=root.panel!=null?root.panel.visualTree.worldBound.width:float.NaN;root.EnableInClassList(Compact,width<CompactWidth);if(Fluid)VbFluid.Apply(root,width);root.EnableInClassList(LargeText,largeText);root.EnableInClassList(ReducedMotion,reducedMotion);root.EnableInClassList(Contrast,contrast);root.EnableInClassList(ReducedFlashes,reducedFlashes);}
}
}
