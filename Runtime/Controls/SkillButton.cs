using UnityEngine;
using UnityEngine.UIElements;
namespace Veilbreak.Gui {
/// <summary>
/// Compact 44 × 56 skill control for portrait cards. Every blocked state reads differently:
/// cooldown fills from the bottom as it recharges, short-on-Power is dashed with the amount missing,
/// silenced is struck, a fallen operative's skills lock, a queued manual cast shows its place in line
/// and a buffered one reads NEXT. A windup sweeps left to right, an invalid aim strikes its glyph and says why,
/// a refused command flashes crimson once and an exposed target inverts the label. Attach drag/aim manipulators from the game.
/// </summary>
[UxmlElement] public partial class SkillButton:VisualElement {
 public const string ClassName="vb-skill";
 readonly Label slot,top,value;readonly VbIcon glyph;readonly VisualElement charge,sweep,slash;
 public SkillView View {get;private set;}
 public SkillButton(){
  AddToClassList(ClassName);focusable=true;VbShape.Attach(this);
  charge=Ui.Box(this,ClassName+"__charge");sweep=Ui.Box(this,ClassName+"__sweep");slot=Ui.Text(this,"A",ClassName+"__slot");glyph=new VbIcon("projectile",20).Classes(ClassName+"__icon").Into(this);
  slash=Ui.Box(this,ClassName+"__slash");var foot=Ui.Box(this,ClassName+"__foot");top=Ui.Text(foot,"",ClassName+"__top");value=Ui.Text(foot,"",ClassName+"__value");
  Bind(new SkillView());
 }
 static readonly string[] Hashes={"#0","#1","#2","#3","#4","#5","#6","#7","#8"};
 // The tooltip is rebuilt only when what it says changes; buttons are bound every frame.
 (string,int,SkillState,int,string) tipKey;
 public SkillButton Bind(SkillView v){
  if(v.state==SkillState.Rejected&&View?.state!=SkillState.Rejected){AddToClassList(ClassName+"--flash");schedule.Execute(()=>RemoveFromClassList(ClassName+"--flash")).StartingIn(400);VbMotion.Shake(this);}
  // Coming ready after a recharge, a silence or a windup swells once; the state colours cross-fade in VbShape.
  else if(v.state==SkillState.Ready&&View!=null&&View.state!=SkillState.Ready&&View.state!=SkillState.Aiming&&View.state!=SkillState.Rejected)VbMotion.Pop(this,.08f);
  View=v;Ui.State(this,ClassName,v.state);slot.text=v.slot;glyph.Icon=v.state==SkillState.Fallen?"lock":v.state==SkillState.Invalid?"no-target":v.icon;
  charge.Show(v.state==SkillState.Cooldown);Ui.Fill(charge,v.charged,true);sweep.Show(v.state==SkillState.Windup);Ui.Fill(sweep,v.progress);slash.Show(v.state==SkillState.Silenced||v.state==SkillState.Invalid);
  switch(v.state){
   case SkillState.Casting:top.text="";value.text="CAST";break;
   case SkillState.Disabled:top.text="";value.text="LOCK";break;
   case SkillState.Cooldown:top.text="";value.text=Ui.Seconds(v.seconds);break;
   case SkillState.Aiming:top.text="";value.text="AIM";break;
   case SkillState.Short:top.text="NEED";value.text=Ui.Count(v.need);break;
   case SkillState.Silenced:top.text="SIL";value.text=Ui.Seconds(v.seconds);break;
   case SkillState.Fallen:top.text="";value.text="OUT";break;
   case SkillState.Queued:top.text="QUEUE";value.text=v.queue>=0&&v.queue<Hashes.Length?Hashes[v.queue]:"#"+v.queue;break;
   case SkillState.Buffered:top.text="NEXT";value.text=Ui.Count(v.cost);break;
   case SkillState.Windup:top.text="";value.text=Ui.Seconds(v.seconds);break;
   case SkillState.Invalid:top.text="";value.text=string.IsNullOrEmpty(v.reason)?"AIM":v.reason.ToUpperInvariant();break;
   case SkillState.Exposed:top.text="EXPOSED";value.text=Ui.Count(v.cost);break;
   default:top.text="";value.text=Ui.Count(v.cost);break;
  }
  top.Show(top.text.Length>0);
  var key=(v.label,v.cost,v.state,v.state==SkillState.Cooldown?Mathf.RoundToInt(v.charged*100):0,v.reason);if(!key.Equals(tipKey)){tipKey=key;
   tooltip=v.label+" · "+v.cost+" Power"+(v.state==SkillState.Cooldown?" · "+Mathf.RoundToInt(v.charged*100)+"% ready":v.state==SkillState.Invalid&&!string.IsNullOrEmpty(v.reason)?" · "+v.reason:v.state==SkillState.Rejected?" · refused":"");}
  return this;
 }
}
}
