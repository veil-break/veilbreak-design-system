using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;
namespace Veilbreak.Gui {
/// <summary>
/// The Veil cut shape: top-left and bottom-right chamfers, painted as a mesh because UI Toolkit has no clip-path.
/// Every value comes from USS custom properties, so stylesheets own the look:
///   --vb-cut (px) [--vb-cut-side (0: top-left and bottom-right, 1: the left side, 2: the right side: a pair of choice cards mirror each other)] --vb-fill [--vb-fill-to --vb-fill-dir (deg, 0 = left→right)] --vb-edge [--vb-edge-to] --vb-bw (px)
///   --vb-dash (1: square dashed frame, the "blocked/queued" look) --vb-bracket [--vb-bracket-len] (L marks on the square corners)
///   --vb-glow (soft outer bleed, the glow budget's CTA/selected look).
/// Content box and hit area stay the element's rectangle. <c>.vb-shape</c> resets every property in Gui.uss because custom properties inherit.
/// </summary>
public static class VbShape {
 public const string ClassName="vb-shape";
 const float Diagonal=.4142f;
 static readonly CustomStyleProperty<float> CutP=new CustomStyleProperty<float>("--vb-cut"),WidthP=new CustomStyleProperty<float>("--vb-bw"),DirP=new CustomStyleProperty<float>("--vb-fill-dir"),DashP=new CustomStyleProperty<float>("--vb-dash"),BracketLengthP=new CustomStyleProperty<float>("--vb-bracket-len"),SideP=new CustomStyleProperty<float>("--vb-cut-side");
 static readonly CustomStyleProperty<Color> FillP=new CustomStyleProperty<Color>("--vb-fill"),FillToP=new CustomStyleProperty<Color>("--vb-fill-to"),EdgeP=new CustomStyleProperty<Color>("--vb-edge"),EdgeToP=new CustomStyleProperty<Color>("--vb-edge-to"),BracketP=new CustomStyleProperty<Color>("--vb-bracket"),GlowP=new CustomStyleProperty<Color>("--vb-glow");
 /// <summary>The six colours a shape paints; state changes cross-fade between two of these.</summary>
 struct Palette {public Color fill,fillTo,edge,edgeTo,bracket,glow;
  public static Palette Lerp(in Palette a,in Palette b,float t)=>new Palette{fill=Color.Lerp(a.fill,b.fill,t),fillTo=Color.Lerp(a.fillTo,b.fillTo,t),edge=Color.Lerp(a.edge,b.edge,t),edgeTo=Color.Lerp(a.edgeTo,b.edgeTo,t),bracket=Color.Lerp(a.bracket,b.bracket,t),glow=Color.Lerp(a.glow,b.glow,t)};
  public bool Same(in Palette o)=>fill==o.fill&&fillTo==o.fillTo&&edge==o.edge&&edgeTo==o.edgeTo&&bracket==o.bracket&&glow==o.glow;}
 sealed class Look {public float cut=10,width=1,dir,bracketLength=8;public int side;public bool dash,painted;public Color fill,fillTo,edge,edgeTo,bracket,glow;public Palette from,to;public IVisualElementScheduledItem timer;public float began;
  public Palette Shown=>new Palette{fill=fill,fillTo=fillTo,edge=edge,edgeTo=edgeTo,bracket=bracket,glow=glow};
  public void Set(in Palette p){fill=p.fill;fillTo=p.fillTo;edge=p.edge;edgeTo=p.edgeTo;bracket=p.bracket;glow=p.glow;}}
 static readonly ConditionalWeakTable<VisualElement,Look> Looks=new ConditionalWeakTable<VisualElement,Look>();

 /// <summary>Paints <paramref name="element"/> as a cut shape from its stylesheet and returns it.</summary>
 public static T Attach<T>(T element) where T:VisualElement {
  if(Looks.TryGetValue(element,out _))return element;
  var look=new Look();Looks.Add(element,look);element.AddToClassList(ClassName);
  element.RegisterCallback<CustomStyleResolvedEvent>(_=>{Read(element.customStyle,look);Settle(element,look);element.MarkDirtyRepaint();});
  element.RegisterCallback<GeometryChangedEvent>(_=>element.MarkDirtyRepaint());
  element.generateVisualContent+=context=>{look.painted=true;Paint(context,element.layout.width,element.layout.height,look);};
  element.RegisterCallback<DetachFromPanelEvent>(_=>{look.timer?.Pause();look.timer=null;look.Set(look.to);look.painted=false;});
  return element;
 }

 static void Read(ICustomStyle s,Look k){
  float N(CustomStyleProperty<float> p,float d)=>s.TryGetValue(p,out var v)?v:d;
  Color C(CustomStyleProperty<Color> p)=>s.TryGetValue(p,out var v)?v:Color.clear;
  k.cut=N(CutP,10);k.width=N(WidthP,1);k.dir=N(DirP,0);k.side=(int)N(SideP,0);k.dash=N(DashP,0)>.5f;k.bracketLength=N(BracketLengthP,8);
  k.to=new Palette{fill=C(FillP),fillTo=C(FillToP),edge=C(EdgeP),edgeTo=C(EdgeToP),bracket=C(BracketP),glow=C(GlowP)};
 }

 /// <summary>State changes (selected, pressed, blocked, a skill coming ready) cross-fade the colours over <c>dur-ui</c> on real time; the first
 /// paint, and a shape off a panel, take them at once. Shape, width and dashes switch at once: they are states, not decoration.</summary>
 static void Settle(VisualElement e,Look k){
  if(!k.painted||e.panel==null){k.timer?.Pause();k.timer=null;k.Set(k.to);return;}
  var shown=k.Shown;if(shown.Same(k.to)){k.timer?.Pause();k.timer=null;return;}
  k.from=shown;k.began=Time.realtimeSinceStartup;
  if(k.timer==null)k.timer=e.schedule.Execute(()=>{float p=Mathf.Clamp01((Time.realtimeSinceStartup-k.began)*1000/VbTokens.DurUi);k.Set(Palette.Lerp(k.from,k.to,VbMotion.Ease(p)));e.MarkDirtyRepaint();if(p>=1){k.timer.Pause();k.timer=null;}}).Every(16);
  else k.timer.Resume();
 }

 /// <summary>The chamfered outline of a <paramref name="w"/>×<paramref name="h"/> shape, moved inward by <paramref name="inset"/> (negative grows it).</summary>
 public static Vector2[] Outline(float w,float h,float cut,float inset)=>Outline(w,h,cut,inset,0);
 /// <summary>The outline with its cuts on <paramref name="side"/>: 0 cuts the top-left and bottom-right corners, <see cref="Left"/> the top-left and bottom-left, <see cref="Right"/> the
 /// top-right and bottom-right. A pair of cards uses Left and Right so the two cut sides face outwards.</summary>
 public static Vector2[] Outline(float w,float h,float cut,float inset,int side){
  float b=inset;
  if(cut<=0)return new[]{new Vector2(b,b),new Vector2(w-b,b),new Vector2(w-b,h-b),new Vector2(b,h-b)};
  float d=cut+b*Diagonal;
  if(side==Left)return new[]{new Vector2(d,b),new Vector2(w-b,b),new Vector2(w-b,h-b),new Vector2(d,h-b),new Vector2(b,h-d),new Vector2(b,d)};
  if(side==Right)return new[]{new Vector2(b,b),new Vector2(w-d,b),new Vector2(w-b,d),new Vector2(w-b,h-d),new Vector2(w-d,h-b),new Vector2(b,h-b)};
  return new[]{new Vector2(d,b),new Vector2(w-b,b),new Vector2(w-b,h-d),new Vector2(w-d,h-b),new Vector2(b,h-b),new Vector2(b,d)};
 }
 /// <summary>The values of <c>--vb-cut-side</c> besides the default (0).</summary>
 public const int Left=1,Right=2;

 static void Paint(MeshGenerationContext ctx,float w,float h,Look k){
  if(w<=0||h<=0)return;
  float c=Mathf.Min(k.cut,w/3,h/3);var g=new Gradient2(w,h,k.dir);
  if(k.glow.a>0)for(int i=3;i>=1;i--){var a=new Color(k.glow.r,k.glow.g,k.glow.b,k.glow.a*(.55f-i*.15f));Ring(ctx,Outline(w,h,c,-i*2.5f,k.side),Outline(w,h,c,-(i-1)*2.5f,k.side),a,a,g);}
  if(k.dash){
   if(k.fill.a>0)Fan(ctx,Outline(w,h,0,0),k.fill,k.fill,g);
   if(k.edge.a>0)Dashes(ctx,w,h,Mathf.Max(1,k.width),k.edge);
  }else{
   if(k.fill.a>0||k.fillTo.a>0)Fan(ctx,Outline(w,h,c,0,k.side),k.fill,k.fillTo.a>0?k.fillTo:k.fill,g);
   if(k.width>0&&(k.edge.a>0||k.edgeTo.a>0))Ring(ctx,Outline(w,h,c,0,k.side),Outline(w,h,c,k.width,k.side),k.edge,k.edgeTo.a>0?k.edgeTo:k.edge,g);
  }
  if(k.bracket.a>0){float l=k.bracketLength,t=2;
   // L marks sit on the square corners: top-right and bottom-left, or both on the side that is not cut.
   if(k.side!=Right){Quad(ctx,w-l,0,w,t,k.bracket);Quad(ctx,w-t,0,w,l,k.bracket);}else{Quad(ctx,0,0,l,t,k.bracket);Quad(ctx,0,0,t,l,k.bracket);}
   if(k.side==Left){Quad(ctx,w-l,h-t,w,h,k.bracket);Quad(ctx,w-t,h-l,w,h,k.bracket);}else{Quad(ctx,0,h-t,l,h,k.bracket);Quad(ctx,0,h-l,t,h,k.bracket);}}
 }

 // Linear gradient across the box: t is the projection of a point onto the direction, normalised over the box's extent.
 readonly struct Gradient2 {
  readonly Vector2 d;readonly float min,span;
  public Gradient2(float w,float h,float degrees){var r=degrees*Mathf.Deg2Rad;d=new Vector2(Mathf.Cos(r),Mathf.Sin(r));float a=0,b=Vector2.Dot(new Vector2(w,0),d),c=Vector2.Dot(new Vector2(0,h),d),e=Vector2.Dot(new Vector2(w,h),d);min=Mathf.Min(Mathf.Min(a,b),Mathf.Min(c,e));span=Mathf.Max(.001f,Mathf.Max(Mathf.Max(a,b),Mathf.Max(c,e))-min);}
  public Color At(Vector2 p,Color from,Color to)=>Color.Lerp(from,to,(Vector2.Dot(p,d)-min)/span);
 }

 static Vertex V(Vector2 p,Color c)=>new Vertex{position=new Vector3(p.x,p.y,Vertex.nearZ),tint=c};

 static void Fan(MeshGenerationContext ctx,Vector2[] pts,Color from,Color to,Gradient2 g){
  int n=pts.Length;var m=ctx.Allocate(n,(n-2)*3);
  foreach(var p in pts)m.SetNextVertex(V(p,g.At(p,from,to)));
  for(int i=1;i<n-1;i++){m.SetNextIndex(0);m.SetNextIndex((ushort)i);m.SetNextIndex((ushort)(i+1));}
 }

 static void Ring(MeshGenerationContext ctx,Vector2[] outer,Vector2[] inner,Color from,Color to,Gradient2 g){
  int n=outer.Length;var m=ctx.Allocate(n*2,n*6);
  foreach(var p in outer)m.SetNextVertex(V(p,g.At(p,from,to)));
  foreach(var p in inner)m.SetNextVertex(V(p,g.At(p,from,to)));
  for(int i=0;i<n;i++){ushort o0=(ushort)i,o1=(ushort)((i+1)%n),i0=(ushort)(n+i),i1=(ushort)(n+(i+1)%n);m.SetNextIndex(o0);m.SetNextIndex(o1);m.SetNextIndex(i1);m.SetNextIndex(o0);m.SetNextIndex(i1);m.SetNextIndex(i0);}
 }

 static void Quad(MeshGenerationContext ctx,float x0,float y0,float x1,float y1,Color c){
  var m=ctx.Allocate(4,6);
  m.SetNextVertex(V(new Vector2(x0,y0),c));m.SetNextVertex(V(new Vector2(x1,y0),c));m.SetNextVertex(V(new Vector2(x1,y1),c));m.SetNextVertex(V(new Vector2(x0,y1),c));
  m.SetNextIndex(0);m.SetNextIndex(1);m.SetNextIndex(2);m.SetNextIndex(0);m.SetNextIndex(2);m.SetNextIndex(3);
 }

 // Dashed frames are square: a cut corner means a real, solid object.
 static void Dashes(MeshGenerationContext ctx,float w,float h,float t,Color c){
  const float dash=4,gap=3;
  for(float x=0;x<w;x+=dash+gap){float e=Mathf.Min(w,x+dash);Quad(ctx,x,0,e,t,c);Quad(ctx,x,h-t,e,h,c);}
  for(float y=0;y<h;y+=dash+gap){float e=Mathf.Min(h,y+dash);Quad(ctx,0,y,t,e,c);Quad(ctx,w-t,y,w,e,c);}
 }
}

/// <summary>The two container looks: slim (cut-xs, no edge; the default for sections, sheets, rows and cards) and chipped
/// (cut + hairline; for what you act on or what is selected). <see cref="Custom"/> leaves the frame to its own classes.</summary>
public enum FrameKind { Custom, Slim, Chipped }

/// <summary>A cut-corner panel for UXML: <c>&lt;Veilbreak.Gui.VbFrame class="vb-panel" kind="Slim"/&gt;</c>.</summary>
[UxmlElement] public partial class VbFrame:VisualElement {
 FrameKind kind;
 [UxmlAttribute] public FrameKind Kind {get=>kind;set{kind=value;EnableInClassList("vb-frame--slim",value==FrameKind.Slim);EnableInClassList("vb-frame--chipped",value==FrameKind.Chipped);}}
 public VbFrame(){VbShape.Attach(this);}
 public VbFrame(FrameKind kind):this(){Kind=kind;}
}
}
