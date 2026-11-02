using UnityEngine;
using UnityEngine.UIElements;
namespace Veilbreak.Gui {
/// <summary>Small Painter2D helpers for vector-painted controls (gauges, reticles, loaders): a colour with another alpha, a line and an arc.</summary>
public static class VbPaint {
 public static Color Alpha(Color color,float alpha){color.a=alpha;return color;}
 public static void Line(Painter2D p,Vector2 a,Vector2 b,Color color,float width=1){p.strokeColor=color;p.lineWidth=width;p.BeginPath();p.MoveTo(a);p.LineTo(b);p.Stroke();}
 /// <summary>A stroked arc between two angles in degrees (0 = east, clockwise on screen); nothing when the radius is not positive or the span is empty.</summary>
 public static void Arc(Painter2D p,Vector2 center,float radius,float start,float end,Color color,float width){
  if(radius<=0||end<=start)return;
  p.strokeColor=color;p.lineWidth=width;p.BeginPath();p.Arc(center,radius,new Angle(start,AngleUnit.Degree),new Angle(end,AngleUnit.Degree));p.Stroke();
 }
}
}
