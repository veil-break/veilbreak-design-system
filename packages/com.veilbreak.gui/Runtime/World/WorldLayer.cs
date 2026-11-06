using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
namespace Veilbreak.Gui {
/// <summary>The world-space HUD over a 3D view: health bars, combat text, labels, markers and off-screen chips at constant
/// screen size. It knows no camera: <see cref="Project"/> maps a world point to layer units (null when behind the view).
/// Each frame call <see cref="BeginFrame"/>, then Bar/Label/Marker/Offscreen for everything that should show, then
/// <see cref="EndFrame"/>, which removes what was not refreshed and animates combat text. <see cref="Text"/> is event-driven.</summary>
public sealed class WorldLayer:VisualElement {
 public const string ClassName="vb-world";
 public Func<Vector3,Vector2?> Project;
 /// <summary>Pooled combat-text cap, and live numbers per target (a fourth reuses that target's oldest).</summary>
 public int TextLimit=40,PerTarget=3;
 /// <summary>Same-kind hits on one target merge while they arrive within <see cref="MergeSeconds"/> of each other.</summary>
 public float MergeSeconds=.2f,TextSeconds=1.2f,Rise=24,StackStep=14;
 /// <summary>Layer units above the projected point for bars, combat text and labels.</summary>
 public float BarLift=27,TextLift=40,LabelLift=48;
 /// <summary>Gap between an off-screen chip and the board edge.</summary>
 public float Edge=10;
 /// <summary>Edges off-screen chips stay clear of (left, top, right, bottom), such as a HUD band over the layer.</summary>
 public Vector4 Insets;
 /// <summary>Combat text fades in place, crits do not pop; a merged label that gains or loses members only fades. A layer under <c>vb-reduced-motion</c> counts too.</summary>
 public bool ReducedMotion;
 /// <summary>A merged label that gains or loses members does not dip its opacity for a beat. A layer under <c>vb-reduced-flashes</c> counts too.</summary>
 public bool ReducedFlashes;

 sealed class Slot<T> where T:VisualElement {public T element;public int frame;}
 sealed class Number {public CombatText label;public int target,amount,stack;public CombatTextKind kind;public Vector3 position;public float age=-1,quiet;public bool Live=>age>=0;}
 readonly Dictionary<int,Slot<WorldHealthBar>> bars=new Dictionary<int,Slot<WorldHealthBar>>();readonly Dictionary<int,Slot<WorldLabel>> labels=new Dictionary<int,Slot<WorldLabel>>();
 readonly Dictionary<int,Slot<WorldMarker>> markers=new Dictionary<int,Slot<WorldMarker>>();readonly Dictionary<int,Slot<OffscreenIndicator>> chips=new Dictionary<int,Slot<OffscreenIndicator>>();
 sealed class Group {public WorldLabel element;public int frame,count;public Vector2 sum;public WorldLabelKind kind;public WorldTeam team;public string title,detail,shown,shownTitle;public int shownCount,last;public float pulse;public bool merge;}
 readonly Dictionary<string,Group> groups=new Dictionary<string,Group>();readonly List<string> staleGroups=new List<string>();
 readonly List<Number> numbers=new List<Number>();readonly List<int> stale=new List<int>();readonly List<Rect> placed=new List<Rect>();int frame;

 public WorldLayer(){AddToClassList(ClassName);pickingMode=PickingMode.Ignore;}
 public int Bars=>bars.Count;public int Labels=>labels.Count+groups.Count;public int Markers=>markers.Count;public int Chips=>chips.Count;
 public int LiveTexts {get{int n=0;foreach(var t in numbers)if(t.Live)n++;return n;}}
 /// <summary>The layer's size in its own units (zero before layout).</summary>
 public Vector2 Size=>float.IsNaN(layout.width)?Vector2.zero:layout.size;

 public void BeginFrame()=>frame++;
 public WorldHealthBar Bar(int id,Vector3 world,float health,float barrier=0,WorldTeam team=WorldTeam.Enemy,WorldRank rank=WorldRank.Normal){var bar=Keep(bars,id,()=>new WorldHealthBar());bar.Team(team).Rank(rank).Set(health,barrier);Place(bar,Project?.Invoke(world),BarLift);return bar;}
 /// <summary>A bar at a point already in layer units, for callers that project themselves.</summary>
 public WorldHealthBar BarAt(int id,Vector2 point,float health,float barrier=0,WorldTeam team=WorldTeam.Enemy,WorldRank rank=WorldRank.Normal){var bar=Keep(bars,id,()=>new WorldHealthBar());bar.Team(team).Rank(rank).Set(health,barrier);Place(bar,point,BarLift);return bar;}
 /// <param name="group">Labels with the same group key and title merge into one, "TETHERED ×3", placed at the centroid of their points; a lone label reads as itself. When the count changes the label
 /// swells (members joined, <see cref="MotionRecipes.LabelMerge"/>) or shrinks (members left, <see cref="MotionRecipes.LabelSplit"/>) once, on simulation time (<see cref="EndFrame"/>'s delta).
 /// The merged label is returned for each call and drops its detail line; a null key keeps the label on its own.</param>
 public WorldLabel Label(int id,Vector3 world,WorldLabelKind kind,string title,string detail=null,WorldTeam team=WorldTeam.Ally,string group=null){
  if(group!=null)return Merge(group,world,kind,title,detail,team);
  var label=Keep(labels,id,()=>new WorldLabel());label.Bind(kind,title,detail,team);Place(label,Project?.Invoke(world),LabelLift);return label;}
 WorldLabel Merge(string key,Vector3 world,WorldLabelKind kind,string title,string detail,WorldTeam team){key=key+"\n"+title;
  if(!groups.TryGetValue(key,out var g))groups[key]=g=new Group{element=new WorldLabel().Into(this),frame=-1};
  if(g.frame!=frame){g.frame=frame;g.count=0;g.sum=Vector2.zero;}
  var p=Project?.Invoke(world);if(p.HasValue){g.count++;g.sum+=p.Value;}g.kind=kind;g.team=team;g.title=title;g.detail=detail;return g.element;}
 // Each merged label is bound once its members are known: a lone one reads as itself, several as "title ×n" at their centroid, none hides it.
 void Settle(){foreach(var g in groups.Values){if(g.frame!=frame)continue;g.element.Show(g.count>0);if(g.count==0){g.last=0;continue;}
   if(g.last>0&&g.count!=g.last){g.merge=g.count>g.last;g.pulse=(g.merge?MotionRecipes.LabelMerge:MotionRecipes.LabelSplit).milliseconds/1000f;}g.last=g.count;
   if(g.count>1){if(g.shown==null||g.shownCount!=g.count||g.shownTitle!=g.title){g.shownCount=g.count;g.shownTitle=g.title;g.shown=WorldLabel.Merged(g.title,g.count);}g.element.Bind(g.kind,g.shown,null,g.team);}
   else g.element.Bind(g.kind,g.title,g.detail,g.team);
   Place(g.element,g.sum/g.count,LabelLift);}
  staleGroups.Clear();foreach(var pair in groups)if(pair.Value.frame!=frame)staleGroups.Add(pair.Key);foreach(var key in staleGroups){groups[key].element.RemoveFromHierarchy();groups.Remove(key);}}
 public WorldMarker Marker(int id,Vector3 world,WorldMarkerKind kind,float size=0){var marker=Keep(markers,id,()=>new WorldMarker());marker.Bind(kind,size);Place(marker,Project?.Invoke(world),0);return marker;}
 /// <summary>A chip on the board edge toward <paramref name="world"/>; hidden while that point is on the board or behind the view.</summary>
 public OffscreenIndicator Offscreen(int id,Vector3 world,string name,WorldTeam team=WorldTeam.Enemy,float metres=-1,bool threat=false){var chip=Keep(chips,id,()=>new OffscreenIndicator());chip.Bind(name,team,metres,threat);
  var p=Project?.Invoke(world);Vector2 at=default;float degrees=0;bool show=p.HasValue&&Pin(p.Value,Size,Insets,Edge,out at,out degrees);chip.Show(show);if(show){At(chip,at);chip.Point(degrees);}return chip;}

 /// <summary>A combat number on <paramref name="target"/> at <paramref name="world"/> (it follows that point while it rises).</summary>
 public void Text(int target,Vector3 world,int amount,CombatTextKind kind){int live=0;Number oldest=null;bool merges=kind!=CombatTextKind.Miss&&kind!=CombatTextKind.Break;
  foreach(var t in numbers){if(!t.Live||t.target!=target)continue;live++;if(merges&&t.kind==kind&&t.quiet<MergeSeconds){t.amount+=amount;t.position=world;t.quiet=0;t.age=Mathf.Min(t.age,MergeSeconds);t.label.Bind(kind,t.amount);return;}if(oldest==null||t.age>oldest.age)oldest=t;}
  var slot=live>=PerTarget?oldest:numbers.Find(t=>!t.Live);if(slot==null){if(numbers.Count>=TextLimit)return;slot=new Number{label=new CombatText().Into(this)};numbers.Add(slot);}
  slot.stack=live>=PerTarget?slot.stack:live;slot.target=target;slot.kind=kind;slot.amount=amount;slot.position=world;slot.age=0;slot.quiet=0;slot.label.Bind(kind,amount);slot.label.Show(false);}

 /// <summary>The pose of a merged label <paramref name="left"/> of <paramref name="seconds"/> into its swell or shrink: its scale, and the opacity it dips to for the first beat (the flash). Reduced motion keeps the opacity and leaves the scale; reduced flashes
 /// keep the scale and leave the opacity; both end at rest (1, 1) when the time is up.</summary>
 public static void LabelPose(bool merge,float left,bool calm,bool flashless,out float scale,out float opacity){
  var recipe=merge?MotionRecipes.LabelMerge:MotionRecipes.LabelSplit;float total=recipe.milliseconds/1000f,k=total>0?Mathf.Clamp01(1-left/total):1;
  scale=calm||k>=1?1:1+(recipe.scale-1)*Mathf.Sin(k*Mathf.PI);opacity=flashless||k>=1?1:1-.45f*(1-k)*(1-k);}
 void Pulse(float delta){
  foreach(var g in groups.Values){if(g.pulse<=0)continue;g.pulse=Mathf.Max(0,g.pulse-delta);var e=g.element;
   if(g.pulse<=0){e.style.scale=StyleKeyword.Null;e.style.opacity=StyleKeyword.Null;continue;}
   bool calm=ReducedMotion||Ui.Under(e,SafeAreaRoot.ReducedMotion),flashless=ReducedFlashes||Ui.Under(e,SafeAreaRoot.ReducedFlashes);
   LabelPose(g.merge,g.pulse,calm,flashless,out float scale,out float opacity);e.style.scale=new Scale(new Vector3(scale,scale,1));e.style.opacity=opacity;}}
 public void EndFrame(float delta){Settle();Pulse(delta);Sweep(bars);Sweep(labels);Sweep(markers);Sweep(chips);foreach(var slot in bars.Values)slot.element.Step(delta);
  foreach(var t in numbers){if(!t.Live)continue;t.age+=delta;t.quiet+=delta;if(t.age>=TextSeconds){t.age=-1;t.label.Show(false);continue;}var p=Project?.Invoke(t.position);t.label.Show(p.HasValue);if(!p.HasValue)continue;
   float k=t.age/TextSeconds;At(t.label,p.Value+new Vector2(18,-TextLift-t.stack*StackStep-(ReducedMotion?0:Rise*k)));t.label.style.opacity=Mathf.Min(1,(1-k)*3);
   bool flare=t.kind==CombatTextKind.Crit||t.kind==CombatTextKind.Execute||t.kind==CombatTextKind.Break;float pop=!ReducedMotion&&flare&&t.age<.15f?1.3f-2*t.age:1;t.label.style.scale=new Scale(new Vector3(pop,pop,1));}
  Declutter();}

 /// <summary>Where an off-screen point pins to the edge of a board of <paramref name="size"/>: on the line from the board's centre
 /// to the point, inside <paramref name="insets"/> plus <paramref name="edge"/>. False while the point is on the board.
 /// <paramref name="degrees"/> is clockwise from right (layer y grows downward).</summary>
 public static bool Pin(Vector2 point,Vector2 size,Vector4 insets,float edge,out Vector2 pinned,out float degrees){pinned=point;degrees=0;
  var board=Rect.MinMaxRect(insets.x,insets.y,size.x-insets.z,size.y-insets.w);if(board.width<=0||board.height<=0||board.Contains(point))return false;
  var centre=board.center;var d=point-centre;float hx=Mathf.Max(0,board.width/2-edge),hy=Mathf.Max(0,board.height/2-edge);float t=Mathf.Min(Mathf.Abs(d.x)>1e-4f?hx/Mathf.Abs(d.x):float.MaxValue,Mathf.Abs(d.y)>1e-4f?hy/Mathf.Abs(d.y):float.MaxValue);
  pinned=centre+d*Mathf.Min(1,t);degrees=Mathf.Atan2(d.y,d.x)*Mathf.Rad2Deg;return true;}

 T Keep<T>(Dictionary<int,Slot<T>> map,int id,Func<T> make) where T:VisualElement {if(!map.TryGetValue(id,out var slot))map[id]=slot=new Slot<T>{element=make().Into(this)};slot.frame=frame;return slot.element;}
 void Sweep<T>(Dictionary<int,Slot<T>> map) where T:VisualElement {stale.Clear();foreach(var pair in map)if(pair.Value.frame!=frame)stale.Add(pair.Key);foreach(var id in stale){map[id].element.RemoveFromHierarchy();map.Remove(id);}}
 static void Place(VisualElement e,Vector2? point,float lift){e.Show(point.HasValue);if(point.HasValue)At(e,point.Value-new Vector2(0,lift));}
 static void At(VisualElement e,Vector2 p){e.style.left=p.x;e.style.top=p.y;}
 // Labels anchor at their bottom centre; one that overlaps a label placed before it this frame steps up and drops its detail line.
 void Declutter(){placed.Clear();foreach(var slot in labels.Values)Stack(slot.element);foreach(var g in groups.Values)if(g.frame==frame)Stack(g.element);}
 void Stack(WorldLabel label){if(label.style.display==DisplayStyle.None)return;var size=label.layout.size;if(float.IsNaN(size.x)||size.x<=0)return;
  var r=new Rect(label.style.left.value.value-size.x/2,label.style.top.value.value-size.y,size.x,size.y);bool moved=false;
  for(int i=0;i<4;i++){bool hit=false;foreach(var q in placed)if(q.Overlaps(r)){r.y=q.yMin-r.height-2;hit=moved=true;}if(!hit)break;}
  label.ShowDetail(!moved);if(moved)label.style.top=r.yMax;placed.Add(r);}
}
}
