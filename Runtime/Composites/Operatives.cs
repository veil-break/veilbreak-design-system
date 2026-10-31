using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
namespace Veilbreak.Gui {
/// <summary>The portrait battle card: face, control mode, health/barrier/stability, statuses and two skills (or "fallen" once out).</summary>
[UxmlElement] public partial class OperativeCard:VisualElement {
 public const string ClassName="vb-operative";
 readonly VisualElement portrait,kit,statuses,skills,fallen;readonly Label code,chip,title,hp,barrier;readonly HealthBar health;readonly SkillButton[] buttons=new SkillButton[2];
 StatusKind[] statusKey;bool statusFallen;(string,bool) titleKey;(ControlMode,int) chipKey=((ControlMode)(-1),0);(int,int) hpKey=(-1,-1);int barrierKey=-1;
 static bool Same(StatusKind[] a,StatusKind[] b){if(a==null||b==null||a.Length!=b.Length)return false;for(int i=0;i<a.Length;i++)if(a[i]!=b[i])return false;return true;}
 public OperativeView View {get;private set;}
 /// <summary>0 = active (A), 1 = signature (S). Games attach aim/drag manipulators here.</summary>
 public SkillButton Skill(int index)=>buttons[index];
 public OperativeCard(){
  AddToClassList(ClassName);VbShape.Attach(this);pickingMode=PickingMode.Ignore;
  var band=Ui.Box(this,ClassName+"__band");portrait=Ui.Box(band,ClassName+"__portrait");Ui.Box(band,ClassName+"__shade");
  code=Ui.Text(band,"",ClassName+"__code");chip=Ui.Text(band,"",ClassName+"__chip");kit=Ui.Box(this,ClassName+"__kit");
  var body=Ui.Box(this,ClassName+"__body");title=Ui.Text(body,"",ClassName+"__name");health=new HealthBar().Into(body);
  var numbers=Ui.Box(body,ClassName+"__numbers");hp=Ui.Text(numbers,"",ClassName+"__hp");barrier=Ui.Text(numbers,"",ClassName+"__barrier");statuses=Ui.Box(body,ClassName+"__statuses");
  skills=Ui.Box(this,ClassName+"__skills");for(int i=0;i<2;i++)buttons[i]=new SkillButton().Into(skills);
  fallen=Ui.Box(this,ClassName+"__fallen");Ui.Text(fallen,"FALLEN",ClassName+"__fallen-title");Ui.Text(fallen,"Revive via perk",ClassName+"__fallen-hint");
  Bind(new OperativeView());
 }
 public OperativeCard Bind(OperativeView v){
  View=v;var mode=v.Fallen?ControlMode.Fallen:v.mode;Ui.State(this,ClassName,mode);
  Ui.Image(portrait,v.portrait);code.text=v.code;kit.style.backgroundColor=v.kit;if((v.name,v.Fallen)!=titleKey){titleKey=(v.name,v.Fallen);title.text=v.Fallen?"<s>"+v.name.ToUpperInvariant()+"</s>":v.name.ToUpperInvariant();}
  // Cards are bound every frame: text is rebuilt only when its inputs change.
  var chipNow=(mode,mode==ControlMode.Manual?Mathf.RoundToInt(v.manualSeconds*10):0);if(chipNow!=chipKey){chipKey=chipNow;chip.text=mode switch{ControlMode.Manual=>v.manualSeconds>0?"MANUAL "+Ui.Seconds(v.manualSeconds):"MANUAL",ControlMode.Aiming=>"AIMING",ControlMode.Fallen=>"FALLEN",_=>"AUTO"};}
  bool exposed=v.statuses.Contains(StatusKind.Exposed);health.Set(v.Fallen?0:v.hp,v.maxHp,v.barrier,v.Fallen?0:v.stability/(float)Mathf.Max(1,v.maxStability),exposed);
  int shownHp=v.Fallen?0:v.hp;if((shownHp,v.maxHp)!=hpKey){hpKey=(shownHp,v.maxHp);hp.style.fontSize=(v.hp.ToString()+v.maxHp).Length>7?10:11;hp.text=shownHp+"<color="+VbTokens.Hex(VbTokens.Text3)+">/"+v.maxHp+"</color>";}
  if(v.barrier!=barrierKey){barrierKey=v.barrier;barrier.text=v.barrier>0?"+"+v.barrier:"";}barrier.Show(v.barrier>0);
  if(v.Fallen!=statusFallen||!Same(v.statuses,statusKey)){statusFallen=v.Fallen;statusKey=(StatusKind[])v.statuses.Clone();statuses.Clear();if(v.Fallen)Ui.Text(statuses,"OUT",ClassName+"__clear");else if(v.statuses.Length==0)Ui.Text(statuses,"CLEAR",ClassName+"__clear");else foreach(var s in v.statuses.Distinct())new VbIcon(StatusChip.Icon(s),14).Classes(ClassName+"__status",ClassName+"__status--"+Ui.Kebab(s.ToString())).Into(statuses).tooltip=s.ToString();}
  skills.Show(!v.Fallen);fallen.Show(v.Fallen);
  for(int i=0;i<2;i++)if(i<v.skills.Length)buttons[i].Bind(v.skills[i]);
  return this;
 }
}

/// <summary>Roster tile: portrait, name, role and (when picked) its squad slot number. Tap opens details.</summary>
[UxmlElement] public partial class OperativeTile:TileComponent {
 public OperativeTile():base(ClassName){}
 /// <summary><paramref name="tier"/> frames the tile in its rarity colour; <paramref name="locked"/> says how to get a locked operative
 /// ("Pull in rifts"), which stays visible but cannot be picked.</summary>
 public OperativeTile Bind(OperativeView v,int slot,bool focused,RarityTier tier=RarityTier.None,string locked=null){bool isLocked=!string.IsNullOrEmpty(locked);
  Picture=v.portrait;Kit=v.kit;Title=v.name;Caption=isLocked?locked:v.role;CaptionIcon=isLocked?"lock":v.roleIcon;
  State=(slot>0&&!isLocked?TileState.In:TileState.Idle)|(focused?TileState.Focused:TileState.Idle)|(isLocked?TileState.Locked:TileState.Idle);Order=slot;Tier=tier;
  tooltip=v.name+", "+v.role+(isLocked?", locked: "+locked:slot>0?", squad slot "+slot:", not in squad");return this;
 }
}

/// <summary>One of the four squad slots on the selection screen: the operative, or a dashed empty slot.</summary>
[UxmlElement] public partial class SquadSlot:TileComponent {
 public new const string ClassName="vb-slot";
 public SquadSlot():base(ClassName){}
 public SquadSlot Bind(OperativeView v,int slot){
  bool has=v!=null;EmptyLabel="SLOT "+slot;State=has?TileState.In:TileState.Empty;
  if(has){Picture=v.portrait;Kit=v.kit;Order=slot;Title=v.name;Caption=v.code;}
  return this;
 }
}

/// <summary>The operative sheet: stats, passive, both skills with cost and cooldown, close and add/remove.</summary>
[UxmlElement] public partial class OperativeDetails:VisualElement {
 public const string ClassName="vb-details";
 readonly VisualElement kit,skills;readonly Label title,role,slot,passive,passiveText;readonly VbIcon roleIcon;readonly StatReadout hp,armor,stability;readonly VbButton toggle;
 public event Action Closed,Toggled;
 public OperativeDetails(){
  AddToClassList(ClassName);VbShape.Attach(this);
  var head=Ui.Box(this,ClassName+"__head");kit=Ui.Box(head,ClassName+"__kit");title=Ui.Text(head,"",ClassName+"__name");
  var r=Ui.Box(head,ClassName+"__role");roleIcon=new VbIcon("striker",14).Into(r);role=Ui.Text(r,"",ClassName+"__role-text");slot=Ui.Text(head,"",ClassName+"__slot");
  new IconButtonComposite("close","Close details",()=>Closed?.Invoke(),12).Classes(ClassName+"__close").Into(head);
  var stats=Ui.Box(this,ClassName+"__stats");hp=new StatReadout("HEALTH","0").Into(stats);armor=new StatReadout("ARMOR","0").Into(stats);stability=new StatReadout("STABILITY","0").Into(stats);armor.AddToClassList("vb-stat--neutral");stability.AddToClassList("vb-stat--neutral");
  var p=Ui.Box(this,ClassName+"__passive");passive=Ui.Text(p,"",ClassName+"__passive-name");passiveText=Ui.Text(p,"",ClassName+"__passive-text");
  skills=Ui.Box(this,ClassName+"__skills");toggle=new VbButton("ADD TO SQUAD",ButtonVariant.Light,null,"add",()=>Toggled?.Invoke()).Classes(ClassName+"__toggle").Into(this);
 }
 /// <param name="slot">Squad slot (1–4), or 0 when not in the squad.</param>
 /// <param name="squadFull">Adding is blocked with a reason when the squad already has four.</param>
 public OperativeDetails Bind(OperativeView v,int slot,bool squadFull,float maxHp=1580,float maxArmor=26,float maxStability=140){
  kit.style.backgroundColor=v.kit;title.text=v.name.ToUpperInvariant();role.text=v.role.ToUpperInvariant();roleIcon.Icon=v.roleIcon;
  this.slot.text=slot>0?"SLOT "+slot:"NOT IN SQUAD";this.slot.EnableInClassList(ClassName+"__slot--in",slot>0);
  hp.Set(Ui.Number(v.maxHp),null,v.maxHp/maxHp);armor.Set(v.armor.ToString(),null,v.armor/maxArmor);stability.Set(v.maxStability.ToString(),null,v.maxStability/maxStability);
  passive.text="PASSIVE · "+v.passive.ToUpperInvariant();passiveText.text=v.passiveText;passiveText.tooltip=v.passiveFullText;
  skills.Clear();
  foreach(var s in v.skills){
   var row=Ui.Box(skills,ClassName+"__skill");row.tooltip=s.fullDescription;var iconBox=VbShape.Attach(Ui.Box(row,ClassName+"__skill-icon"));new VbIcon(s.icon,14).Into(iconBox);
   var text=Ui.Box(row,ClassName+"__skill-text");var line=Ui.Box(text,ClassName+"__skill-line");Ui.Text(line,s.label,ClassName+"__skill-name");Ui.Text(line,s.slot+" · "+s.kind.ToUpperInvariant(),ClassName+"__skill-slot");Ui.Text(text,s.description,ClassName+"__skill-desc");
   var cost=Ui.Box(row,ClassName+"__skill-cost");Ui.Text(cost,s.cost.ToString(),ClassName+"__skill-power");Ui.Text(cost,Ui.Seconds(s.cooldownSeconds)+" CD",ClassName+"__skill-cd");
  }
  bool inSquad=slot>0;toggle.Title=inSquad?"REMOVE FROM SQUAD":"ADD TO SQUAD";toggle.Icon=inSquad?"remove":"add";toggle.Variant=inSquad?ButtonVariant.Danger:ButtonVariant.Light;toggle.Blocked=!inSquad&&squadFull?"Squad full · remove one first":"";
  return this;
 }
}

/// <summary>Post-run contribution for one operative (popover content).</summary>
[UxmlElement] public partial class OperativeStats:VisualElement {
 public const string ClassName="vb-contribution";
 readonly VisualElement portrait,shareFill,grid;readonly Label title,fell,share;
 public OperativeStats(){
  AddToClassList(ClassName);VbShape.Attach(this);pickingMode=PickingMode.Ignore;
  var head=Ui.Box(this,ClassName+"__head");portrait=VbShape.Attach(Ui.Box(head,ClassName+"__portrait"));var h=Ui.Box(head,ClassName+"__head-text");title=Ui.Text(h,"",ClassName+"__name");fell=Ui.Text(h,"",ClassName+"__fell");
  var s=Ui.Box(this,ClassName+"__share");var line=Ui.Box(s,ClassName+"__share-line");Ui.Text(line,"Share of squad damage",ClassName+"__share-label");share=Ui.Text(line,"",ClassName+"__share-value");
  shareFill=Ui.Box(Ui.Box(s,ClassName+"__share-track"),ClassName+"__share-fill");grid=Ui.Box(this,ClassName+"__grid");
 }
 public OperativeStats Bind(ContributionView c){
  var v=c.operative;Ui.Image(Ui.Crop(portrait),v.portrait);title.text=v.name.ToUpperInvariant();fell.text=c.fellWave>0?"FELL WAVE "+c.fellWave:"STANDING";fell.EnableInClassList(ClassName+"__fell--alive",c.fellWave<=0);
  share.text=Mathf.RoundToInt(c.share*100)+"%";Ui.Fill(shareFill,c.share);shareFill.style.backgroundColor=v.kit;
  grid.Clear();
  foreach(var (k,value) in new[]{("DAMAGE",Ui.Number(c.damage)),("KILLS",Ui.Number(c.kills)),("SKILLS CAST",Ui.Number(c.casts)),("POWER SPENT",Ui.Number(c.power)),(c.extraLabel.ToUpperInvariant(),c.extraValue),("WAVES SURVIVED",c.fellWave>0?(c.fellWave-1).ToString():"ALL")}){var cell=Ui.Box(grid,ClassName+"__cell");Ui.Text(cell,k,ClassName+"__key");Ui.Text(cell,value,ClassName+"__value");}
  return this;
 }
}

/// <summary>Results cell; hover, focus or tap shows <see cref="OperativeStats"/>.</summary>
[UxmlElement] public partial class SquadResult:VisualElement {
 public const string ClassName="vb-result";
 readonly Label title,fell;readonly VisualElement kit;ContributionView view;
 public Popover Details {get;}
 public SquadResult(){
  AddToClassList(ClassName);VbShape.Attach(this);focusable=true;title=Ui.Text(this,"",ClassName+"__name");fell=Ui.Text(this,"",ClassName+"__fell");kit=Ui.Box(this,ClassName+"__kit");
  Details=Popover.Attach(this,()=>view==null?null:new OperativeStats().Bind(view),PopoverPlacement.Above);
 }
 public SquadResult Bind(ContributionView c){view=c;title.text=c.fellWave>0?"<s>"+c.operative.name.ToUpperInvariant()+"</s>":c.operative.name.ToUpperInvariant();fell.text=c.fellWave>0?"FELL W"+c.fellWave:"STANDING";kit.style.backgroundColor=c.operative.kit;EnableInClassList(ClassName+"--fallen",c.fellWave>0);return this;}
}
}
