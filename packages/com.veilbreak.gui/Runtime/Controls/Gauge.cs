using System.Globalization;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;
namespace Veilbreak.Gui {
/// <summary>A radial gauge: <see cref="Segments"/> arc cells over a 260° sweep, lit up to <see cref="Value"/> (0-1), a faint inner track
/// and the value as a percentage in the middle. The colour is <c>--vb-gauge-color</c> (the ally colour by default) or <see cref="Tint"/>.
/// <see cref="Draw"/> is public so an element that cannot derive from it (a framework's own gauge) paints the same dial.</summary>
[UxmlElement] public partial class VbGauge:VisualElement {
 public const string ClassName="vb-gauge";
 public const float StartDegrees=140,SweepDegrees=260,Margin=8,TrackInset=9;
 public const int MinSegments=4,MaxSegments=96;
 static readonly CustomStyleProperty<Color> ColorP=new CustomStyleProperty<Color>("--vb-gauge-color");
 float amount=.72f;int segments=36;Color sheet=VbTokens.Teal500,tint;readonly Label readout;
 /// <summary>Filled fraction, clamped to 0-1; NaN reads as 0.</summary>
 [UxmlAttribute,CreateProperty] public float Value {get=>amount;set{amount=Clamp(value);readout.text=Percent(amount);MarkDirtyRepaint();}}
 /// <summary>Cells in the dial, 4 to 96.</summary>
 [UxmlAttribute] public int Segments {get=>segments;set{segments=Mathf.Clamp(value,MinSegments,MaxSegments);MarkDirtyRepaint();}}
 /// <summary>The centre read-out; style it from USS or recolour it.</summary>
 public Label Readout=>readout;
 public VbGauge(){AddToClassList(ClassName);pickingMode=PickingMode.Ignore;readout=Ui.Text(this,"",ClassName+"__value");readout.pickingMode=PickingMode.Ignore;Value=amount;
  RegisterCallback<CustomStyleResolvedEvent>(_=>{if(customStyle.TryGetValue(ColorP,out var c))sheet=c;MarkDirtyRepaint();});generateVisualContent+=context=>Draw(context.painter2D,contentRect,amount,segments,Colour);}
 /// <summary>The colour in use: <see cref="Tint"/> when one was set, else the stylesheet's.</summary>
 public Color Colour=>tint.a>0?tint:sheet;
 /// <summary>Overrides the stylesheet colour; <c>default</c> returns to it.</summary>
 public VbGauge Tint(Color c){tint=c;MarkDirtyRepaint();return this;}
 public static float Clamp(float value)=>float.IsNaN(value)?0:Mathf.Clamp01(value);
 public static string Percent(float value)=>(Clamp(value)*100).ToString("0",CultureInfo.CurrentCulture)+"%";
 /// <summary>How many cells are lit for a value.</summary>
 public static int Lit(float value,int segments)=>Mathf.CeilToInt(Clamp(value)*segments);
 /// <summary>Paints the dial inside <paramref name="area"/>: lit cells in <paramref name="accent"/>, unlit ones and the inner track faint.</summary>
 public static void Draw(Painter2D p,Rect area,float value,int segments,Color accent){
  var center=area.center;var radius=Mathf.Min(area.width,area.height)*.5f-Margin;int lit=Lit(value,segments);
  for(int i=0;i<segments;i++){float a=StartDegrees+SweepDegrees*i/segments;VbPaint.Arc(p,center,radius,a,a+SweepDegrees/segments-2,VbPaint.Alpha(accent,i<lit?1:.18f),4);}
  VbPaint.Arc(p,center,radius-TrackInset,StartDegrees,StartDegrees+SweepDegrees,VbPaint.Alpha(accent,.22f),1);
 }
}
}
