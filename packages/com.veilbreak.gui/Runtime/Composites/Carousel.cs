using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
namespace Veilbreak.Gui {
/// <summary>Where one card of a <see cref="Carousel"/> sits at an offset <c>d</c> from the active one (0 is the active card, ±1 the neighbours): a shift, a drop, a tilt, a scale and an opacity.</summary>
public readonly struct CarouselPose {
 public readonly float x,y,rotation,scale,opacity;
 public CarouselPose(float x,float y,float rotation,float scale,float opacity){this.x=x;this.y=y;this.rotation=rotation;this.scale=scale;this.opacity=opacity;}
}

/// <summary>Cards in a row where the active one takes most of the width and its neighbours sit partly beneath it, slightly tilted: the active card is <see cref="ActiveWidth"/> of the carousel wide, the cards at ±1 are tucked under its
/// edges and rotated a few degrees, the ones at ±2 (when there are five or more) peek out beyond them. The row is endless in both directions: the card after the last is the first. The player changes the active card by swiping (a
/// drag follows the finger and settles on the nearest card, a flick goes one further), by tapping a card beneath the active one, with the arrow keys, or by focusing a card; <see cref="Changed"/> reports it and the game binds again. It
/// animates between cards (<see cref="Milliseconds"/>, ease-out, on real time) and is instant under reduced motion. The carousel is layout only: any <see cref="VisualElement"/> is a card (a <see cref="ChoiceCard"/> for perks) and shows
/// its own information, so nothing needs to repeat the active card elsewhere. Reordering cards is by <see cref="Bind"/>, which keeps a card's element per key.</summary>
[UxmlElement] public partial class Carousel:VisualElement {
 public const string ClassName="vb-carousel",StageClass=ClassName+"__stage",CardClass=ClassName+"__card",ActiveClass=ClassName+"__card--active";
 /// <summary>The active card's width as a fraction of the carousel's, and how far a neighbour is tucked beneath it (a fraction of the card's width, from the centre).</summary>
 public const float ActiveWidth=.64f,Tuck=.6f;
 /// <summary>How long a move takes, in milliseconds, and how far a pointer travels before a press is a swipe, in units.</summary>
 public const int Milliseconds=320;public const float Slop=8;
 /// <summary>The most a flick adds to where a drag was let go, in cards: a quick short swipe carries on to the next card, a slow or a long one lands on the nearest.</summary>
 public const float Flick=.4f;
 readonly VisualElement stage;readonly List<VisualElement> cards=new List<VisualElement>();readonly Dictionary<string,VisualElement> byKey=new Dictionary<string,VisualElement>();
 float position,from,to,pressX,pressPosition,lastX,velocity;double began,lastTime;int index,order=int.MinValue,pointer=-1;bool pressed,dragging;IVisualElementScheduledItem timer;VisualElement pressTarget;
 public override VisualElement contentContainer=>stage;
 public Carousel(){
  AddToClassList(ClassName);focusable=true;stage=new VisualElement{pickingMode=PickingMode.Ignore};stage.AddToClassList(StageClass);hierarchy.Add(stage);
  RegisterCallback<GeometryChangedEvent>(_=>Layout());
  RegisterCallback<PointerDownEvent>(Down,TrickleDown.TrickleDown);RegisterCallback<PointerMoveEvent>(Move,TrickleDown.TrickleDown);RegisterCallback<PointerUpEvent>(Up,TrickleDown.TrickleDown);RegisterCallback<PointerCancelEvent>(_=>Release(false));
  RegisterCallback<KeyDownEvent>(Key);RegisterCallback<DetachFromPanelEvent>(_=>Stop());
 }
 /// <summary>Raised when the player moves to another card (swipe, tap, key or focus): the new index. Binding does not raise it.</summary>
 public event Action<int> Changed;
 public int Count=>cards.Count;
 /// <summary>The active card's index, 0 when there are no cards.</summary>
 public int Index=>index;
 /// <summary>The continuous position while a move or a drag is in flight (equal to <see cref="Index"/> at rest, in cards); card <c>i</c> sits at offset <c>i − Position</c>, wrapped to the shortest way round.</summary>
 public float Slot=>position;
 public bool Animating=>timer!=null;
 public IReadOnlyList<VisualElement> Cards=>cards;
 public VisualElement Active=>cards.Count>0?cards[index]:null;
 /// <summary>How many cards show on each side of the active one: two with five or more cards, one with two to four, none with one.</summary>
 public int Depth=>Mathf.Min(2,cards.Count/2);

 /// <summary>The offset of card <paramref name="i"/> from <paramref name="position"/> among <paramref name="count"/> cards, wrapped to (−count/2, count/2]: −1 is the card just before the active one, even for the first card of the row.</summary>
 public static float Offset(int i,float position,int count){if(count<=1)return 0;float d=i-position;return d-count*Mathf.Round(d/count);}
 /// <summary>The pose at offset <paramref name="d"/> in a carousel <paramref name="width"/> wide with <paramref name="depth"/> cards on a side: the active card is still; a neighbour is shifted so it sits partly under the active card's edge, drops a few units, tilts 7° a card, shrinks and
 /// dims; a card past the last shown fades out.</summary>
 public static CarouselPose Pose(float d,float width,int depth){
  float a=Mathf.Abs(d),s=Mathf.Sign(d),cw=width*ActiveWidth,step=cw*(.5f+Tuck*.5f);
  float x=s*(step*Mathf.Min(a,1)+step*.55f*Mathf.Max(0,Mathf.Min(a,2)-1)),y=Mathf.Min(a,2)*7f,rot=Mathf.Clamp(d*7f,-14f,14f),scale=1-.1f*Mathf.Min(a,2),opacity=(1-.14f*Mathf.Min(a,2))*Mathf.Clamp01(depth+1-a);
  return new CarouselPose(x,y,rot,scale,opacity);
 }
 /// <summary>The way round to <paramref name="target"/> from <paramref name="current"/> among <paramref name="count"/> cards: the shortest signed step (a tie goes forward).</summary>
 public static int Shortest(int current,int target,int count){if(count<=1)return 0;int d=((target-current)%count+count)%count;return d>count/2?d-count:d;}

 /// <summary>Shows the cards <paramref name="values"/> in order, one per value, each made once per key by <paramref name="create"/> and filled by <paramref name="bind"/> (value, index); a card whose key goes away is removed.
 /// <paramref name="selected"/> is the active index. The first bind, and a bind under reduced motion, put the active card in place at once; later ones glide.</summary>
 public Carousel Bind<T>(IReadOnlyList<T> values,Func<T,string> key,Func<VisualElement> create,Action<VisualElement,T,int> bind,int selected){
  var seen=new HashSet<string>();var next=new List<VisualElement>(values.Count);
  for(int i=0;i<values.Count;i++){string k=key(values[i]);if(!seen.Add(k))throw new ArgumentException("Duplicate carousel key "+k);
   if(!byKey.TryGetValue(k,out var card)){card=create();byKey[k]=card;card.AddToClassList(CardClass);Adopt(card);}
   if(card.parent!=stage)stage.Add(card);bind(card,values[i],i);next.Add(card);}
  var gone=new List<string>();foreach(var pair in byKey)if(!seen.Contains(pair.Key))gone.Add(pair.Key);foreach(var k in gone){byKey[k].RemoveFromHierarchy();byKey.Remove(k);}
  bool first=cards.Count==0;cards.Clear();cards.AddRange(next);order=int.MinValue;Select(selected,!first);return this;
 }
 // A card that takes focus becomes the active one, so the keyboard walks the carousel; its size sets the carousel's height.
 void Adopt(VisualElement card){card.RegisterCallback<FocusInEvent>(_=>{int i=cards.IndexOf(card);if(i>=0&&i!=index&&!dragging&&!pressed)Go(i,true);});card.RegisterCallback<GeometryChangedEvent>(_=>Fit());}

 /// <summary>Makes card <paramref name="i"/> the active one without raising <see cref="Changed"/> (the game's own state), gliding there unless <paramref name="animate"/> is false or motion is reduced.</summary>
 public void Select(int i,bool animate=true){
  if(cards.Count==0){index=0;position=0;return;}i=((i%cards.Count)+cards.Count)%cards.Count;
  if(i==index&&(Animating||pressed))return;
  MoveTo(i,animate&&panel!=null&&!VbMotion.Calm(this));
 }
 /// <summary>Moves to card <paramref name="i"/> and raises <see cref="Changed"/> when that is another card.</summary>
 public void Go(int i,bool animate=true){if(cards.Count==0)return;i=((i%cards.Count)+cards.Count)%cards.Count;bool other=i!=index;MoveTo(i,animate&&panel!=null&&!VbMotion.Calm(this));if(other)Changed?.Invoke(index);}
 /// <summary>One card on, wrapping from the last to the first.</summary>
 public void Next(bool animate=true)=>Go(index+1,animate);
 /// <summary>One card back, wrapping from the first to the last.</summary>
 public void Previous(bool animate=true)=>Go(index-1,animate);

 void MoveTo(int target,bool animate){
  Stop();int count=cards.Count;float current=Mathf.Repeat(position,count);int step=Shortest(Mathf.RoundToInt(current)%count,target,count);index=target;
  if(!animate){position=index;Layout();return;}
  from=position;to=Mathf.Round(position)+step;if(Mathf.Approximately(from,to)){position=index;Layout();return;}began=VbMotion.Clock();timer=schedule.Execute(Tick).Every(16);Tick();
 }
 void Tick(){float p=VbMotion.Calm(this)?1:Mathf.Clamp01((float)((VbMotion.Clock()-began)*1000/Milliseconds));position=Mathf.Lerp(from,to,VbMotion.Ease(p));
  if(p>=1){Stop();position=index;}Layout();}
 void Stop(){timer?.Pause();timer=null;}

 void Fit(){float h=0;foreach(var c in cards)h=Mathf.Max(h,c.layout.height);if(h>0&&!Mathf.Approximately(stage.resolvedStyle.height,h+16))stage.style.height=h+16;}

 void Layout(){
  float w=resolvedStyle.width;int n=cards.Count;if(!(w>0)||n==0)return;float cw=Mathf.Round(w*ActiveWidth);int depth=Depth,near=Mathf.RoundToInt(Mathf.Repeat(position,n))%n;
  if(near!=order){order=near;Sort(n);}
  for(int i=0;i<n;i++){var c=cards[i];float d=Offset(i,position,n);var pose=Pose(d,w,depth);
   c.style.position=Position.Absolute;c.style.top=8;c.style.width=cw;c.style.left=(w-cw)*.5f;
   c.style.translate=new Translate(pose.x,pose.y);c.style.rotate=new Rotate(new Angle(pose.rotation));c.style.scale=new Scale(new Vector3(pose.scale,pose.scale,1));c.style.opacity=pose.opacity;
   bool active=i==index;c.EnableInClassList(ActiveClass,active);c.pickingMode=pose.opacity>.05f?PickingMode.Position:PickingMode.Ignore;}
  Fit();
 }
 // Siblings paint in order, so the cards go back to front: the farthest first, the active card last (on top).
 void Sort(int n){var idx=new List<int>(n);for(int i=0;i<n;i++)idx.Add(i);idx.Sort((a,b)=>Mathf.Abs(Offset(b,order,n)).CompareTo(Mathf.Abs(Offset(a,order,n))));foreach(var i in idx)cards[i].BringToFront();}

 // ── swipe, tap and keys ──────────────────────────────────────────────────
 int CardAt(VisualElement target){for(var e=target;e!=null&&e!=this;e=e.hierarchy.parent){int i=cards.IndexOf(e);if(i>=0)return i;}return -1;}
 void Down(PointerDownEvent e){
  if(cards.Count<2||e.button!=0&&e.pointerType==UnityEngine.UIElements.PointerType.mouse)return;Stop();
  pressed=true;dragging=false;pointer=e.pointerId;pressX=lastX=e.position.x;pressPosition=position;velocity=0;lastTime=Time.realtimeSinceStartupAsDouble;int i=CardAt(e.target as VisualElement);pressTarget=i>=0?cards[i]:null;
  // A card beneath the active one is not pressed: the carousel takes the press, so the card's own action never fires from a tap that only moves the row.
  if(i>=0&&i!=index)e.StopPropagation();
 }
 void Move(PointerMoveEvent e){
  if(!pressed||e.pointerId!=pointer)return;float dx=e.position.x-pressX;
  if(!dragging&&Mathf.Abs(dx)>Slop){dragging=true;this.CapturePointer(pointer);}
  if(!dragging)return;float step=Mathf.Max(1,resolvedStyle.width*ActiveWidth*(.5f+Tuck*.5f));double now=Time.realtimeSinceStartupAsDouble;float dt=Mathf.Max(1/60f,(float)(now-lastTime));velocity=Mathf.Lerp(velocity,(lastX-e.position.x)/step/dt,.5f);lastX=e.position.x;lastTime=now;
  position=pressPosition-dx/step;Layout();e.StopPropagation();
 }
 void Up(PointerUpEvent e){
  if(!pressed||e.pointerId!=pointer)return;bool moved=dragging;var target=pressTarget;Release(true);
  if(moved){int n=cards.Count;float settle=position+Mathf.Clamp(velocity*.1f,-Flick,Flick);int i=((Mathf.RoundToInt(settle)%n)+n)%n;GoTo(i);e.StopPropagation();return;}
  int tapped=target==null?-1:cards.IndexOf(target);if(tapped>=0&&tapped!=index){GoTo(tapped);e.StopPropagation();}
 }
 // Settling after a drag glides from where the finger left the row.
 void GoTo(int i){int n=cards.Count;bool other=i!=index;Stop();index=i;from=position;float nearest=Mathf.Round(position);to=nearest+Shortest(((int)Mathf.Repeat(nearest,n)),i,n);
  if(VbMotion.Calm(this)||Mathf.Approximately(from,to)||panel==null){position=index;Layout();}else{began=VbMotion.Clock();timer=schedule.Execute(Tick).Every(16);Tick();}
  if(other)Changed?.Invoke(index);}
 void Release(bool settle){if(dragging&&pointer>=0&&this.HasPointerCapture(pointer))this.ReleasePointer(pointer);pressed=false;dragging=false;pointer=-1;pressTarget=null;if(!settle&&cards.Count>0){position=index;Layout();}}
 void Key(KeyDownEvent e){if(cards.Count<2)return;if(e.keyCode==KeyCode.RightArrow||e.keyCode==KeyCode.D){Next();e.StopPropagation();}else if(e.keyCode==KeyCode.LeftArrow||e.keyCode==KeyCode.A){Previous();e.StopPropagation();}}
}
}
