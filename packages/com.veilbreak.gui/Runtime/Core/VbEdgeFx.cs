using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;
namespace Veilbreak.Gui {
/// <summary>What an edge effect says: <see cref="Activate"/> is the pressed or activated look of a button, card or tile; <see cref="Damage"/> is a unit card taking a hit.</summary>
public enum EdgeFxKind { Activate, Damage }

/// <summary>The animated border of the MONO wireframe kit (an accent line that fades along an edge, brightens and travels), on the cut outline of any shaped element. <b>Activate</b>: two bright heads start at opposite points of the outline
/// and run clockwise, each leaving a fading tail and a soft glow, until they meet where the other began (the kit's top edge sweeping right while the bottom sweeps left), then the afterglow fades; it takes the element's own edge colours
/// (<c>--vb-edge</c> and <c>--vb-edge-to</c>, or <c>--vb-glow</c>), so a danger button sweeps pink and a primary one pink into teal. <b>Damage</b>: a translucent red flash over the shape and a red border glow that decays, for unit cards
/// (<see cref="UnitCard.Damage"/>). It paints on one overlay child (<c>vb-edgefx</c>) that is added when it plays and removed when it ends, tracing <see cref="VbVectorShape.Outline(VisualElement,float)"/>'s chamfer from the host's
/// <c>--vb-cut</c> and <c>--vb-cut-side</c>. Real time (<see cref="VbMotion.Clock"/>). Reduced motion (<see cref="VbMotion.Calm"/>): no travelling, the whole edge lights at once and fades in 100 ms; reduced flashes: a damage hit has no fill
/// flash and a dimmer edge. A host that leaves its panel finishes at once.</summary>
public static class VbEdgeFx {
 public const string ClassName="vb-edgefx";
 /// <summary>How long an activation and a damage hit run, in milliseconds, and how long the calm (reduced motion) fade takes.</summary>
 public const int ActivateMilliseconds=520,DamageMilliseconds=560,CalmMilliseconds=100;
 const int Segments=14;
 static readonly CustomStyleProperty<float> CutP=new CustomStyleProperty<float>("--vb-cut"),SideP=new CustomStyleProperty<float>("--vb-cut-side");
 static readonly CustomStyleProperty<Color> EdgeP=new CustomStyleProperty<Color>("--vb-edge"),EdgeToP=new CustomStyleProperty<Color>("--vb-edge-to"),GlowP=new CustomStyleProperty<Color>("--vb-glow");
 sealed class Track {
  public VisualElement host,layer;public EdgeFxKind kind;public double began;public float strength=1;public Color a,b;public IVisualElementScheduledItem timer;public int frame=-1;
  public bool calm,flashless;public float progress;public Vector2[] outline;public float[] lengths;public float perimeter;public float width,height;public int side;public float cut;public Action done;
 }
 static readonly ConditionalWeakTable<VisualElement,Track> tracks=new ConditionalWeakTable<VisualElement,Track>();

 /// <summary>Plays the activation effect whenever <paramref name="e"/> is clicked or submitted (Enter, Space, gamepad). Safe to call twice.</summary>
 public static T Attach<T>(T e) where T:VisualElement {
  if(e==null||attached.TryGetValue(e,out _))return e;attached.Add(e,new object());
  e.RegisterCallback<ClickEvent>(_=>Play(e));e.RegisterCallback<NavigationSubmitEvent>(_=>Play(e));return e;
 }
 static readonly ConditionalWeakTable<VisualElement,object> attached=new ConditionalWeakTable<VisualElement,object>();

 /// <summary>Plays the activation effect on <paramref name="e"/> now. <paramref name="tone"/> overrides the colour the element's own edge would give.</summary>
 public static void Play(VisualElement e,Color? tone=null)=>Start(e,EdgeFxKind.Activate,1,tone,null);
 /// <summary>Plays the damage effect on <paramref name="e"/>: a translucent red flash and a red border glow. <paramref name="strength"/> (0 to 1) scales how hard it hits (a big hit is 1).</summary>
 public static void Damage(VisualElement e,float strength=1)=>Start(e,EdgeFxKind.Damage,Mathf.Clamp01(strength),VbTokens.Enemy,null);
 /// <summary>Whether an effect is playing on <paramref name="e"/>.</summary>
 public static bool Active(VisualElement e)=>e!=null&&tracks.TryGetValue(e,out var t)&&t.layer!=null;
 /// <summary>The kind that is playing on <paramref name="e"/>, or null.</summary>
 public static EdgeFxKind? Playing(VisualElement e)=>e!=null&&tracks.TryGetValue(e,out var t)&&t.layer!=null?t.kind:(EdgeFxKind?)null;
 /// <summary>Ends the effect on <paramref name="e"/> at once and removes its overlay.</summary>
 public static void Cancel(VisualElement e){if(e!=null&&tracks.TryGetValue(e,out var t))End(t);}

 static void Start(VisualElement e,EdgeFxKind kind,float strength,Color? tone,Action done){
  if(e==null||e.panel==null)return;var t=tracks.GetValue(e,key=>{var value=new Track{host=key};key.RegisterCallback<DetachFromPanelEvent>(_=>End(value));return value;});
  if(t.layer!=null&&t.frame==Time.frameCount&&t.kind==kind)return;
  t.kind=kind;t.strength=strength;t.began=VbMotion.Clock();t.done=done;t.frame=Time.frameCount;Colours(e,t,kind,tone);
  if(t.layer==null){t.layer=new VisualElement{pickingMode=PickingMode.Ignore};t.layer.AddToClassList(ClassName);var s=t.layer.style;s.position=Position.Absolute;s.left=0;s.top=0;s.right=0;s.bottom=0;t.layer.generateVisualContent+=ctx=>Paint(ctx,t);e.Add(t.layer);}
  else e.Add(t.layer);
  t.timer?.Pause();t.timer=t.layer.schedule.Execute(()=>Tick(t)).Every(16);Tick(t);
 }

 // The activation takes the host's edge (a gradient's two ends), or its glow; a plain frame lights in the ally colour. A damage hit is always the enemy red.
 static void Colours(VisualElement e,Track t,EdgeFxKind kind,Color? tone){
  if(tone.HasValue){t.a=t.b=tone.Value;return;}
  var s=e.customStyle;s.TryGetValue(EdgeP,out var edge);s.TryGetValue(EdgeToP,out var edgeTo);s.TryGetValue(GlowP,out var glow);
  Color a=edge.a>.05f?edge:glow.a>.05f?glow:VbTokens.Ally;Color b=edgeTo.a>.05f?edgeTo:a;
  t.a=new Color(a.r,a.g,a.b,1);t.b=new Color(b.r,b.g,b.b,1);
 }

 static void Tick(Track t){
  var host=t.host;if(host==null||host.panel==null){End(t);return;}
  t.calm=VbMotion.Calm(host);t.flashless=Ui.Under(host,SafeAreaRoot.ReducedFlashes);
  int duration=t.calm?CalmMilliseconds:t.kind==EdgeFxKind.Damage?DamageMilliseconds:ActivateMilliseconds;
  t.progress=Mathf.Clamp01((float)((VbMotion.Clock()-t.began)*1000/duration));
  if(t.progress>=1){End(t);return;}
  t.layer?.MarkDirtyRepaint();
 }

 static void End(Track t){
  t.timer?.Pause();t.timer=null;var layer=t.layer;t.layer=null;layer?.RemoveFromHierarchy();var done=t.done;t.done=null;done?.Invoke();
 }

 // Outline of the host's cut, in the layer's own coordinates (the layer fills the host), rebuilt only when the size changes.
 static void Shape(Track t,float w,float h){
  var s=t.host.customStyle;float cut=s.TryGetValue(CutP,out var c)?c:10;int side=s.TryGetValue(SideP,out var sd)?(int)sd:0;
  if(t.outline!=null&&Mathf.Approximately(t.width,w)&&Mathf.Approximately(t.height,h)&&Mathf.Approximately(t.cut,cut)&&t.side==side)return;
  t.width=w;t.height=h;t.cut=cut;t.side=side;t.outline=VbShape.Outline(w,h,Mathf.Min(cut,w/3,h/3),.75f,side);
  int n=t.outline.Length;t.lengths=new float[n];t.perimeter=0;for(int i=0;i<n;i++){t.lengths[i]=Vector2.Distance(t.outline[i],t.outline[(i+1)%n]);t.perimeter+=t.lengths[i];}
 }

 /// <summary>The point at arc length <paramref name="s"/> (wrapping) along a closed polyline <paramref name="points"/> whose edges are <paramref name="lengths"/> long.</summary>
 public static Vector2 PointAt(Vector2[] points,float[] lengths,float perimeter,float s){
  s=((s%perimeter)+perimeter)%perimeter;for(int i=0;i<points.Length;i++){if(s<=lengths[i]||i==points.Length-1){float k=lengths[i]>0?Mathf.Min(1,s/lengths[i]):0;return Vector2.Lerp(points[i],points[(i+1)%points.Length],k);}s-=lengths[i];}
  return points[0];
 }

 static void Paint(MeshGenerationContext ctx,Track t){
  var layer=t.layer;if(layer==null)return;float w=layer.layout.width,h=layer.layout.height;if(!(w>0)||!(h>0))return;Shape(t,w,h);
  var p=ctx.painter2D;p.lineJoin=LineJoin.Miter;p.lineCap=LineCap.Butt;var pts=t.outline;
  float u=t.progress,fade=1-Smooth(.72f,1,u);
  if(t.kind==EdgeFxKind.Damage)PaintDamage(p,t,pts,u);
  else if(t.calm)Ring(p,pts,t.a,.9f*(1-u),1.5f);
  else PaintSweep(p,t,pts,u,fade);
 }

 static float Smooth(float a,float b,float x){x=Mathf.Clamp01((x-a)/(b-a));return x*x*(3-2*x);}

 static void Ring(Painter2D p,Vector2[] pts,Color c,float alpha,float width){
  if(alpha<=.003f)return;p.strokeColor=new Color(c.r,c.g,c.b,Mathf.Clamp01(alpha));p.lineWidth=width;p.BeginPath();p.MoveTo(pts[0]);for(int i=1;i<pts.Length;i++)p.LineTo(pts[i]);p.ClosePath();p.Stroke();
 }

 static void PaintSweep(Painter2D p,Track t,Vector2[] pts,float u,float fade){
  float half=t.perimeter*.5f,head=VbMotion.Ease(u)*half,tail=half*.5f;
  // The afterglow: the whole edge lit at a low level that fades with the travel, so the outline reads once the heads have passed.
  Ring(p,pts,Color.Lerp(t.a,t.b,.5f),.30f*fade*(1-u*.5f),1f);
  for(int h=0;h<2;h++){
   var colour=h==0?t.a:t.b;float at=head+h*half;
   for(int i=0;i<Segments;i++){
    float k0=i/(float)Segments,k1=(i+1)/(float)Segments;float s0=at-tail*k1,s1=at-tail*k0;
    float alpha=Mathf.Pow(1-k0,1.7f)*fade;
    Stroke(p,t,pts,s0,s1,colour,alpha*.22f,5f);Stroke(p,t,pts,s0,s1,colour,alpha*.95f,1.6f);
   }
  }
 }

 static void Stroke(Painter2D p,Track t,Vector2[] pts,float s0,float s1,Color c,float alpha,float width){
  if(alpha<=.004f||s1<=s0)return;p.strokeColor=new Color(c.r,c.g,c.b,Mathf.Clamp01(alpha));p.lineWidth=width;
  float per=t.perimeter,lo=Mathf.Floor(s0/per)*per,a=s0-lo,b=s1-lo;p.BeginPath();p.MoveTo(PointAt(pts,t.lengths,per,a));
  // The outline's corners between the two ends, so the stroke follows the chamfer.
  for(int lap=0;lap<2;lap++){float cum=lap*per;for(int i=0;i<pts.Length;i++){cum+=t.lengths[i];if(cum>a+.01f&&cum<b-.01f)p.LineTo(pts[(i+1)%pts.Length]);}}
  p.LineTo(PointAt(pts,t.lengths,per,b));p.Stroke();
 }

 static void PaintDamage(Painter2D p,Track t,Vector2[] pts,float u){
  float strength=Mathf.Lerp(.55f,1,t.strength);
  if(t.calm){Ring(p,pts,t.a,.85f*strength*(1-u),2f);return;}
  // A quick rise in the first tenth, then a decay: the flash is translucent so the portrait stays readable under it, and reduced flashes drop it.
  float rise=Mathf.Clamp01(u/.1f),decay=Mathf.Pow(1-Smooth(.08f,1,u),1.4f),level=rise*decay;
  if(!t.flashless){p.fillColor=new Color(t.a.r,t.a.g,t.a.b,.34f*strength*level);p.BeginPath();p.MoveTo(pts[0]);for(int i=1;i<pts.Length;i++)p.LineTo(pts[i]);p.ClosePath();p.Fill();}
  float edge=t.flashless?.55f:1f;
  for(int r=3;r>=1;r--)Ring(p,pts,t.a,.16f*strength*level*edge/r,1.5f+r*2.4f);
  Ring(p,pts,t.a,.95f*strength*level*edge,1.6f);
 }
}
}
