using System;
using UnityEngine;
using UnityEngine.UIElements;
namespace Veilbreak.Gui {
/// <summary>Horizontal scanlines over a holographic surface, painted as one mesh. Colour and pitch come from
/// <c>--vb-scan</c> and <c>--vb-scan-pitch</c>; the high-contrast theme hides them. Never put them behind live battle numbers.</summary>
[UxmlElement] public partial class ScanLines:VisualElement {
 public const string ClassName="vb-scanlines";
 static readonly CustomStyleProperty<Color> ColorP=new CustomStyleProperty<Color>("--vb-scan");static readonly CustomStyleProperty<float> PitchP=new CustomStyleProperty<float>("--vb-scan-pitch");
 Color color=new Color(1,1,1,.05f);float pitch=3;
 public ScanLines(){AddToClassList(ClassName);pickingMode=PickingMode.Ignore;RegisterCallback<CustomStyleResolvedEvent>(_=>{if(customStyle.TryGetValue(ColorP,out var c))color=c;if(customStyle.TryGetValue(PitchP,out var p))pitch=Mathf.Max(2,p);MarkDirtyRepaint();});generateVisualContent+=Paint;}
 void Paint(MeshGenerationContext context){float w=layout.width,h=layout.height;if(!(w>0&&h>0)||color.a<=0)return;int lines=Mathf.Min(400,Mathf.FloorToInt(h/pitch));if(lines==0)return;
  var mesh=context.Allocate(lines*4,lines*6);for(int i=0;i<lines;i++){float y=i*pitch;mesh.SetNextVertex(new Vertex{position=new Vector3(0,y,Vertex.nearZ),tint=color});mesh.SetNextVertex(new Vertex{position=new Vector3(w,y,Vertex.nearZ),tint=color});mesh.SetNextVertex(new Vertex{position=new Vector3(w,y+1,Vertex.nearZ),tint=color});mesh.SetNextVertex(new Vertex{position=new Vector3(0,y+1,Vertex.nearZ),tint=color});}
  for(int i=0;i<lines;i++){ushort b=(ushort)(i*4);mesh.SetNextIndex(b);mesh.SetNextIndex((ushort)(b+1));mesh.SetNextIndex((ushort)(b+2));mesh.SetNextIndex(b);mesh.SetNextIndex((ushort)(b+2));mesh.SetNextIndex((ushort)(b+3));}}
}

/// <summary>A diegetic holographic panel for menus: cut frame with brackets, scanlines, a chromatic caps title and a flicker-in.
/// Menus only, one in focus per screen; battle panels never use it. Reduced motion or reduced flashes turn the flicker into a fade.
/// UI Toolkit draws flat, so the pointer tilt of the web version is left to the 3D menu camera.</summary>
[UxmlElement] public partial class HoloFrame:VisualElement {
 public const string ClassName="vb-holo";
 readonly VisualElement body,head;readonly Label title,cyan,pink;
 public override VisualElement contentContainer=>body;
 public HoloFrame():this(null){}
 public HoloFrame(string title){AddToClassList(ClassName);VbShape.Attach(this);hierarchy.Add(new ScanLines());head=new VisualElement{pickingMode=PickingMode.Ignore};head.AddToClassList(ClassName+"__head");hierarchy.Add(head);
  cyan=Ui.Text(head,"",ClassName+"__title",ClassName+"__title--cyan");pink=Ui.Text(head,"",ClassName+"__title",ClassName+"__title--pink");this.title=Ui.Text(head,"",ClassName+"__title");
  body=new VisualElement();body.AddToClassList(ClassName+"__body");hierarchy.Add(body);Title=title;RegisterCallback<AttachToPanelEvent>(_=>FlickerIn());}
 [UxmlAttribute] public string Title {get=>title.text;set{title.text=cyan.text=pink.text=(value??"").ToUpperInvariant();head.Show(title.text.Length>0);}}
 /// <summary>Opacity steps 0 → .7 → .25 → 1 over ~240 ms, or a plain fade under reduced motion or reduced flashes.</summary>
 public void FlickerIn(){bool calm=Calm(this)||Ui.Under(this,SafeAreaRoot.ReducedFlashes);float[] steps=calm?new[]{.25f,.5f,.75f,1}:new[]{.7f,.25f,.9f,1};style.opacity=0;for(int i=0;i<steps.Length;i++){float o=steps[i];schedule.Execute(()=>style.opacity=o).StartingIn(60*(i+1));}}
 internal static bool Calm(VisualElement e)=>Ui.Under(e,SafeAreaRoot.ReducedMotion);
}

public enum PlatformState { Occupied, Selected, Open }

/// <summary>The 2D stand-in for a lineup plinth: a holo plate with a caps name tag, a floating diamond when no 3D fighter
/// stands on it, lock-on brackets when selected and a dashed OPEN SLOT when empty. The 3D menu draws the real plate.</summary>
[UxmlElement] public partial class HoloPlatform:VisualElement {
 public const string ClassName="vb-platform";
 readonly VisualElement plate;readonly VbIcon marker;readonly Label label,sub;
 public event Action Picked;
 public HoloPlatform(){AddToClassList(ClassName);this.Clickable(()=>Picked?.Invoke());marker=new VbIcon("diamond",18).Classes(ClassName+"__marker").Into(this);var tag=Ui.Box(this,ClassName+"__tag");label=Ui.Text(tag,"",ClassName+"__label");sub=Ui.Text(tag,"",ClassName+"__sub");
  plate=VbShape.Attach(Ui.Box(this,ClassName+"__plate"));new ScanLines().Into(plate);Bind("",null);}
 public PlatformState State {get;private set;}
 public HoloPlatform Bind(string name,string detail,PlatformState state=PlatformState.Occupied,WorldTeam team=WorldTeam.Ally,bool showMarker=true){State=state;Ui.State(this,ClassName,state);Ui.State(this,ClassName,team);
  bool open=state==PlatformState.Open;label.text=open?"+ OPEN SLOT":(name??"").ToUpperInvariant();sub.text=open?"weight shared":detail??"";sub.Show(sub.text.Length>0);marker.Show(showMarker&&!open);tooltip=open?"Open slot":name;return this;}
}

/// <summary>A rift result: a holographic back that flips to a tier-framed front. Tier words travel with the colour
/// (RARE, EPIC, LEGENDARY; items MAGIC, RARE, UNIQUE ITEM). Duplicates show their resonance gain instead of NEW; a Legendary glows gold.</summary>
[UxmlElement] public partial class RevealCard:VisualElement {
 public const string ClassName="vb-reveal";
 readonly VisualElement back,front;readonly Label tierWord,heading,line,badge;
 public bool Flipped {get;private set;}
 public event Action Flipping;
 public RevealCard(){AddToClassList(ClassName);this.Clickable(()=>{if(!Flipped)Flip();});
  back=VbShape.Attach(Ui.Box(this,ClassName+"__back"));new ScanLines().Into(back);new VbMark(56).Into(back);
  front=VbShape.Attach(Ui.Box(this,ClassName+"__front"));tierWord=Ui.Text(front,"",ClassName+"__tier");heading=Ui.Text(front,"",ClassName+"__name");line=Ui.Text(front,"",ClassName+"__line");badge=Ui.Text(front,"",ClassName+"__badge");
  Bind(RarityTier.Blue,"",null);}
 public static string TierWord(RarityTier tier,bool item)=>item?tier switch{RarityTier.Gold=>"UNIQUE ITEM",RarityTier.Violet=>"RARE ITEM",_=>"MAGIC ITEM"}:tier switch{RarityTier.Gold=>"LEGENDARY",RarityTier.Violet=>"EPIC",_=>"RARE"};
 /// <summary><paramref name="detail"/> is "AR · Striker" for a fighter or the slot for an item; <paramref name="dupe"/> ("Resonance R2") replaces NEW.</summary>
 public RevealCard Bind(RarityTier tier,string title,string detail,bool item=false,bool isNew=true,string dupe=null,bool flipped=false,string tierLabel=null){Ui.State(this,ClassName,tier);
  tierWord.text=tierLabel??TierWord(tier,item);heading.text=(title??"").ToUpperInvariant();line.text=detail??"";badge.text=!string.IsNullOrEmpty(dupe)?dupe.ToUpperInvariant():isNew?"NEW":"";badge.Show(badge.text.Length>0);badge.EnableInClassList(ClassName+"__badge--dupe",!string.IsNullOrEmpty(dupe));
  Face(flipped);return this;}
 /// <summary>Turns the card face up: a quick edge-on squash (higher tiers wait a beat), or at once under reduced motion.</summary>
 public void Flip(){if(Flipped)return;Flipping?.Invoke();if(HoloFrame.Calm(this)){Face(true);return;}int wait=ClassListContains(ClassName+"--gold")?180:ClassListContains(ClassName+"--violet")?90:0;
  schedule.Execute(()=>style.scale=new Scale(new Vector3(.05f,1,1))).StartingIn(wait);schedule.Execute(()=>{Face(true);style.scale=new Scale(new Vector3(.05f,1,1));}).StartingIn(wait+110);schedule.Execute(()=>style.scale=new Scale(Vector3.one)).StartingIn(wait+130);}
 void Face(bool up){Flipped=up;back.Show(!up);front.Show(up);EnableInClassList(ClassName+"--flipped",up);}
}

}
