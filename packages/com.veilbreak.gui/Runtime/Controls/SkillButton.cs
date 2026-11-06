using UnityEngine;
using UnityEngine.UIElements;
namespace Veilbreak.Gui {
/// <summary>
/// Compact 44 × 56 skill control for portrait cards, with one layout in every state: the slot letter, the glyph and one cost line. The states are overlays on that layout, so
/// nothing ever runs below the slot's foot: a cooldown fills from the bottom and counts its seconds over the glyph (a windup and a silence count the same way), short-on-Power
/// dims the slot and puts the shortfall in the corner ("−3"), a queued manual cast puts its place in line there ("#1") and a buffered one an arrow, a fallen operative's skills lock,
/// an invalid aim strikes its glyph and replaces the cost with the reason, a refused command flashes crimson once and an exposed target inverts the cost.
/// A windup sweeps left to right. Attach drag/aim manipulators from the game.
/// </summary>
[UxmlElement] public partial class SkillButton:VisualElement {
 public const string ClassName="vb-skill";
 readonly Label slot,value,seconds,badge;readonly VbIcon glyph;readonly VisualElement charge,sweep,slash;
 SkillState? previousState;IVisualElementScheduledItem rejected;
 public SkillView View {get;private set;}
 /// <summary>The corner overlay: the shortfall ("−3"), the queue place ("#1") or a buffered arrow; empty hides it.</summary>
 public Label Badge=>badge;
 /// <summary>The cost line, or the reason while an aim is invalid.</summary>
 public Label Value=>value;
 /// <summary>The countdown over the glyph (cooldown, windup, silence); empty hides it.</summary>
 public Label Seconds=>seconds;
 public SkillButton(){
  AddToClassList(ClassName);focusable=true;VbShape.Attach(this);
  charge=Ui.Box(this,ClassName+"__charge");sweep=Ui.Box(this,ClassName+"__sweep");slot=Ui.Text(this,"A",ClassName+"__slot");glyph=new VbIcon("projectile",20).Classes(ClassName+"__icon").Into(this);
  seconds=Ui.Text(this,"",ClassName+"__seconds");slash=Ui.Box(this,ClassName+"__slash");badge=Ui.Text(this,"",ClassName+"__badge");value=Ui.Text(this,"",ClassName+"__value");
  RegisterCallback<DetachFromPanelEvent>(_=>{rejected?.Pause();rejected=null;RemoveFromClassList(ClassName+"--flash");});Bind(new SkillView());
 }
 static readonly string[] Hashes={"#0","#1","#2","#3","#4","#5","#6","#7","#8"};
 static readonly string[] Shortfalls=new string[1000];
 /// <summary>"−n", cached so a card bound every frame makes no garbage.</summary>
 static string Shortfall(int n)=>n>=0&&n<Shortfalls.Length?Shortfalls[n]??=("−"+n):"−"+n;
 // The tooltip is rebuilt only when what it says changes; buttons are bound every frame.
 (string,int,SkillState,int,string) tipKey;
 public SkillButton Bind(SkillView v){
  if(v.state==SkillState.Rejected&&previousState!=SkillState.Rejected){AddToClassList(ClassName+"--flash");rejected?.Pause();rejected=schedule.Execute(()=>RemoveFromClassList(ClassName+"--flash")).StartingIn(MotionRecipes.Deal.milliseconds);VbMotion.Shake(this);}
  // Coming ready after a recharge, a silence or a windup swells once; the state colours cross-fade in VbShape.
  else if(v.state==SkillState.Ready&&previousState.HasValue&&previousState!=SkillState.Ready&&previousState!=SkillState.Aiming&&previousState!=SkillState.Rejected)VbMotion.Pop(this,.08f);
  previousState=v.state;View=v;Ui.State(this,ClassName,v.state);slot.text=v.slot;glyph.Icon=v.state==SkillState.Fallen||v.state==SkillState.Disabled?"lock":v.state==SkillState.Invalid?"no-target":v.icon;
  charge.Show(v.state==SkillState.Cooldown);Ui.Fill(charge,v.charged,true);sweep.Show(v.state==SkillState.Windup);Ui.Fill(sweep,v.progress);slash.Show(v.state==SkillState.Silenced||v.state==SkillState.Invalid);
  string count="",corner="",cost=v.state==SkillState.Fallen?"":Ui.Count(v.cost);
  switch(v.state){
   case SkillState.Cooldown:case SkillState.Windup:case SkillState.Silenced:count=Ui.Seconds(v.seconds);break;
   case SkillState.Short:corner=Shortfall(v.need);break;
   case SkillState.Queued:corner=v.queue>=0&&v.queue<Hashes.Length?Hashes[v.queue]:"#"+v.queue;break;
   case SkillState.Buffered:corner="›";break;
   case SkillState.Invalid:cost=string.IsNullOrEmpty(v.reason)?cost:v.reason.ToUpperInvariant();break;
  }
  seconds.text=count;seconds.Show(count.Length>0);badge.text=corner;badge.Show(corner.Length>0);value.text=cost;value.Show(cost.Length>0);
  var key=(v.label,v.cost,v.state,v.state==SkillState.Cooldown?Mathf.RoundToInt(v.charged*100):0,v.reason);if(!key.Equals(tipKey)){tipKey=key;
   tooltip=v.label+" · "+v.cost+" Power"+(v.state==SkillState.Cooldown?" · "+Mathf.RoundToInt(v.charged*100)+"% ready":v.state==SkillState.Short?" · "+v.need+" short":v.state==SkillState.Queued?" · queued":v.state==SkillState.Invalid&&!string.IsNullOrEmpty(v.reason)?" · "+v.reason:v.state==SkillState.Rejected?" · refused":"");}
  return this;
 }
}
}
