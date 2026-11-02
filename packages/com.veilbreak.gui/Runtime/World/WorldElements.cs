using System.Globalization;
using UnityEngine;
using UnityEngine.UIElements;
namespace Veilbreak.Gui {
public enum WorldTeam { Ally, Enemy }
public enum WorldRank { Normal, Elite, Boss }
/// <summary>Damage dealt reads in text colour; damage taken carries a minus in enemy text; crits pop; execute and break flare.</summary>
public enum CombatTextKind { Damage, Taken, Crit, Exposed, Heal, Barrier, Power, Miss, Execute, Break }
public enum WorldLabelKind { Aim, Name, Telegraph, Estimate, Blocked }
public enum WorldMarkerKind { Ally, Enemy, Selected, Queued, Move }

/// <summary>A fighter's world bar: health in team colour, barrier drawn after it, and a ghost trail of recent damage.
/// Elite and boss ranks get a longer, accented bar. Health is never a green-to-red ramp.</summary>
[UxmlElement] public partial class WorldHealthBar:VisualElement {
 public const string ClassName="vb-world-health";
 /// <summary>Seconds the ghost holds before it drains, and the share of the bar it drains per second.</summary>
 public float GhostHold=.25f,GhostRate=.75f;
 readonly VisualElement ghost,fill,barrier;float health=1,shield,trail=1,hold;
 public WorldHealthBar(){AddToClassList(ClassName);pickingMode=PickingMode.Ignore;ghost=Ui.Box(this,ClassName+"__ghost");fill=Ui.Box(this,ClassName+"__fill");barrier=Ui.Box(this,ClassName+"__barrier");Team(WorldTeam.Enemy);Rank(WorldRank.Normal);Draw();}
 public float Health=>health;public float Ghost=>trail;
 public WorldHealthBar Team(WorldTeam team){Ui.State(this,ClassName,team);return this;}
 public WorldHealthBar Rank(WorldRank rank){Ui.State(this,ClassName,rank);return this;}
 /// <summary>Overrides the team fill (kit accents); null returns to the team colour.</summary>
 public WorldHealthBar Tint(Color? color){fill.style.backgroundColor=color.HasValue?new StyleColor(color.Value):new StyleColor(StyleKeyword.Null);return this;}
 /// <summary>Health and barrier as fractions of max health. A drop leaves the ghost behind; a heal pulls it along.</summary>
 public WorldHealthBar Set(float health,float barrier=0){health=Mathf.Clamp01(health);if(health<this.health)hold=GhostHold;if(health>=trail)trail=health;this.health=health;shield=Mathf.Clamp01(barrier);Draw();return this;}
 /// <summary>Drains the ghost toward health.</summary>
 public void Step(float delta){if(trail<=health)return;if(hold>0){hold-=delta;return;}trail=Mathf.MoveTowards(trail,health,delta*GhostRate);Draw();}
 void Draw(){Ui.Fill(fill,health);Ui.Fill(ghost,trail);float b=Mathf.Min(shield,1-health);barrier.style.left=Length.Percent(health*100);Ui.Fill(barrier,b);barrier.Show(b>0);}
}

/// <summary>A floating combat number. <see cref="WorldLayer"/> pools, merges and animates them.</summary>
[UxmlElement] public partial class CombatText:Label {
 public const string ClassName="vb-combat-text";
 public CombatText(){AddToClassList(ClassName);pickingMode=PickingMode.Ignore;}
 public CombatText Bind(CombatTextKind kind,int amount){Ui.State(this,ClassName,kind);text=Format(kind,amount);return this;}
 public static string Format(CombatTextKind kind,int amount){string n=amount.ToString("N0",CultureInfo.InvariantCulture);
  return kind switch{CombatTextKind.Taken=>"−"+n,CombatTextKind.Crit=>n+"!",CombatTextKind.Exposed=>"EXPOSED "+n,CombatTextKind.Heal=>"+"+n,CombatTextKind.Barrier=>"BLOCKED "+n,CombatTextKind.Power=>"+"+n+" POWER",CombatTextKind.Miss=>"MISS",CombatTextKind.Execute=>"EXECUTE "+n,CombatTextKind.Break=>"BREAK",_=>n};}
}

/// <summary>Billboard text tied to a fighter or a point: aim chips, name tags, telegraph timers, move estimates and blocked markers.
/// Constant screen size; when labels crowd, <see cref="WorldLayer"/> stacks them upward and drops their detail first.</summary>
[UxmlElement] public partial class WorldLabel:VisualElement {
 public const string ClassName="vb-world-label";
 readonly Label title,detail;
 public WorldLabel(){AddToClassList(ClassName);pickingMode=PickingMode.Ignore;title=Ui.Text(this,"",ClassName+"__title");detail=Ui.Text(this,"",ClassName+"__detail");Bind(WorldLabelKind.Name,"");}
 public string Title=>title.text;public string Detail=>detail.text;
 public WorldLabel Bind(WorldLabelKind kind,string title,string detail=null,WorldTeam team=WorldTeam.Ally){Ui.State(this,ClassName,kind);Ui.State(this,ClassName,team);this.title.text=kind==WorldLabelKind.Blocked?"× "+title:title;this.detail.text=detail??"";ShowDetail(true);return this;}
 /// <summary>Hides the detail line (crowding); it returns on the next <see cref="Bind"/>.</summary>
 public void ShowDetail(bool on)=>detail.Show(on&&detail.text.Length>0);
}

/// <summary>A marker at a projected point: allies are circles and enemies diamonds, always, so ownership reads from shape;
/// selected adds brackets, queued a lavender ring, move a dashed ring. Perspective ground decals belong to the 3D telegraph layer.</summary>
[UxmlElement] public partial class WorldMarker:VisualElement {
 public const string ClassName="vb-world-marker";
 readonly VbIcon glyph;
 public WorldMarker(){AddToClassList(ClassName);pickingMode=PickingMode.Ignore;glyph=new VbIcon("circle").Classes(ClassName+"__glyph").Into(this);glyph.style.width=glyph.style.height=Length.Percent(100);Bind(WorldMarkerKind.Ally);}
 public WorldMarkerKind Kind {get;private set;}
 public WorldMarker Bind(WorldMarkerKind kind,float size=0){Kind=kind;Ui.State(this,ClassName,kind);glyph.Icon=kind==WorldMarkerKind.Enemy?"diamond":"circle";if(size>0){style.width=style.height=size;}else{style.width=style.height=StyleKeyword.Null;}return this;}
}

/// <summary>A chip pinned to the board edge, on the line from the centre to a fighter the camera cannot frame.</summary>
[UxmlElement] public partial class OffscreenIndicator:VisualElement {
 public const string ClassName="vb-offscreen";
 readonly VisualElement arrow;readonly Label title,distance;
 public OffscreenIndicator(){AddToClassList(ClassName);pickingMode=PickingMode.Ignore;arrow=new VbIcon("chevron",12).Classes(ClassName+"__arrow").Into(this);title=Ui.Text(this,"",ClassName+"__name");distance=Ui.Text(this,"",ClassName+"__distance");Bind("",WorldTeam.Enemy);}
 public string Name=>title.text;
 /// <summary>Degrees clockwise from screen-right toward the fighter, as last placed.</summary>
 public float Angle {get;private set;}
 public OffscreenIndicator Bind(string name,WorldTeam team,float metres=-1,bool threat=false){Ui.State(this,ClassName,team);EnableInClassList(ClassName+"--threat",threat);title.text=name??"";distance.text=metres>=0?Mathf.RoundToInt(metres)+"m":"";distance.Show(metres>=0);return this;}
 public void Point(float degrees){Angle=degrees;arrow.style.rotate=new Rotate(new Angle(degrees,AngleUnit.Degree));}
}

/// <summary>A band along the bottom (or top) edge of the board warning that something is about to cross it: a row of
/// chevrons pointing over the edge and a short caption. Urgency 0 hides it; the chevrons ripple outward, faster as
/// urgency rises, and run hot above two thirds. Reduced motion holds them steady.</summary>
[UxmlElement] public partial class EdgeWarning:VisualElement {
 public const string ClassName="vb-edge-warning";public const int Chevrons=5;
 readonly VisualElement row;readonly Label caption;
 public EdgeWarning(){AddToClassList(ClassName);pickingMode=PickingMode.Ignore;row=new VisualElement().Classes(ClassName+"__row").Into(this);for(int i=0;i<Chevrons;i++)new VbIcon("chevron",14).Classes(ClassName+"__chevron").Into(row);caption=Ui.Text(this,"",ClassName+"__caption");Set(0);}
 public float Urgency {get;private set;}
 public string Caption=>caption.text;
 public EdgeWarning Bind(string text,bool top=false){caption.text=text??"";EnableInClassList(ClassName+"--top",top);return this;}
 /// <summary>Shows the band at <paramref name="urgency"/> (0..1); <paramref name="time"/> drives the ripple.</summary>
 public void Set(float urgency,float time=0,bool reducedMotion=false){Urgency=Mathf.Clamp01(urgency);this.Show(Urgency>0);if(Urgency<=0)return;EnableInClassList(ClassName+"--hot",Urgency>.66f);
  int i=0;foreach(var chevron in row.Children()){float wave=Mathf.Sin(time*(4+6*Urgency)-Mathf.Abs(i-(Chevrons-1)*.5f)*.9f)*.5f+.5f;chevron.style.opacity=reducedMotion?1:.35f+.65f*wave;i++;}}
}
}
