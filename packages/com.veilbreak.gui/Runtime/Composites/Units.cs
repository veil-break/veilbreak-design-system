using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
namespace Veilbreak.Gui {
/// <summary>The portrait battle card: face, control mode, health/barrier/stability, up to three timed statuses as icons on the portrait and two skills (or one "fallen" once out).
/// The name is the only identifier (no code over it); a mode chip shows only for a non-default mode (MANUAL with its seconds, AIMING), because the HUD toggle states autoplay once;
/// passives and perks are not statuses (they live in <see cref="UnitDetail"/>). A card that falls while on a panel slumps (<see cref="MotionRecipes.Fallen"/>) as its colour drains to the fallen look and the damage flash plays; reduced motion drains the colour only,
/// and reduced flashes drop the flash fill.</summary>
[UxmlElement] public partial class UnitCard:SurfaceView {
 public const string ClassName="vb-operative";
 readonly VisualElement portrait,kit,skills,fallen;readonly StatusRow statuses;readonly Label chip,title,hp,barrier,fallenHint;readonly HealthBar health;readonly SkillButton[] buttons=new SkillButton[2];
 (string,bool) titleKey;(ControlMode,int) chipKey=((ControlMode)(-1),0);(int,int) hpKey=(-1,-1);int barrierKey=-1;bool wasFallen,bound;
 public OperativeView View {get;private set;}
 /// <summary>0 = active (A), 1 = signature (S). Games attach aim/drag manipulators here.</summary>
 public SkillButton Skill(int index)=>buttons[index];
 /// <summary>The timed statuses as a row of small squares above the card; hover, focus or tap opens their list (<see cref="StatusRow"/>).</summary>
 public StatusRow Statuses=>statuses;
 /// <summary>Whether a drop in the bound hit points plays the damage effect by itself (<see cref="Damage"/>); on by default. A host that raises its own damage events turns it off.</summary>
 public bool AutoDamage {get;set;}=true;
 /// <summary>The unit takes a hit: a translucent red flash over the card and a red border glow (<see cref="VbEdgeFx.Damage"/>), scaled by <paramref name="strength"/> (0 to 1).</summary>
 public UnitCard Damage(float strength=1){VbEdgeFx.Damage(this,strength);return this;}
 public UnitCard(){
  AddToClassList(ClassName);
  var band=Ui.Box(this,ClassName+"__band");new VbClipFill().Classes(ClassName+"__band-fill").Into(band);portrait=new VbClipImage().Classes(ClassName+"__portrait").Into(band);Ui.Box(band,ClassName+"__shade");
  chip=Ui.Text(band,"",ClassName+"__chip");kit=Ui.Box(this,ClassName+"__kit");
  var body=Ui.Box(this,ClassName+"__body");title=Ui.Text(body,"",ClassName+"__name");health=new HealthBar().Into(body);
  var numbers=Ui.Box(body,ClassName+"__numbers");hp=Ui.Text(numbers,"",ClassName+"__hp");barrier=Ui.Text(numbers,"",ClassName+"__barrier");
  skills=Ui.Box(this,ClassName+"__skills");for(int i=0;i<2;i++)buttons[i]=new SkillButton().Into(skills);buttons[1].AddToClassList(ClassName+"__skill-end");
  fallen=Ui.Box(this,ClassName+"__fallen");Ui.Text(fallen,"FALLEN",ClassName+"__fallen-title");fallenHint=Ui.Text(fallen,"Revive via perk",ClassName+"__fallen-hint");
  statuses=new StatusRow().Classes(ClassName+"__statuses").Into(this);
  Bind(new OperativeView());bound=wasFallen=false; // the placeholder bind is not a state the card was in: only a bind after the first one can be a fall
 }
 public UnitCard Bind(OperativeView v){
  View=v;var mode=v.Fallen?ControlMode.Fallen:v.mode;Ui.State(this,ClassName,mode);
  if(bound&&v.Fallen&&!wasFallen&&panel!=null)VbMotion.Slump(this,MotionRecipes.Fallen);wasFallen=v.Fallen;bound=true;
  Ui.Image(portrait,v.portrait);portrait.MarkDirtyRepaint();kit.style.backgroundColor=v.kit;if((v.name,v.Fallen)!=titleKey){titleKey=(v.name,v.Fallen);title.text=v.Fallen?"<s>"+v.name.ToUpperInvariant()+"</s>":v.name.ToUpperInvariant();}
  // Cards are bound every frame: text is rebuilt only when its inputs change. A chip shows only for a mode that is not the default.
  var chipNow=(mode,mode==ControlMode.Manual?Mathf.RoundToInt(v.manualSeconds*10):0);if(chipNow!=chipKey){chipKey=chipNow;chip.text=mode switch{ControlMode.Manual=>v.manualSeconds>0?"MANUAL "+Ui.Seconds(v.manualSeconds):"MANUAL",ControlMode.Aiming=>"AIMING",_=>""};chip.Show(chip.text.Length>0);}
  bool exposed=v.effects.Any(e=>e.definition?.id=="exposed"&&e.Active);health.Set(v.Fallen?0:v.hp,v.maxHp,v.barrier,v.Fallen?0:v.stability/(float)Mathf.Max(1,v.maxStability),exposed);
  int shownHp=v.Fallen?0:v.hp;
  // A drop in hit points is a hit (a revive or a heal raises them, so it is not): the card flashes by itself unless the host raises its own damage events.
  if(AutoDamage&&panel!=null&&hpKey.Item1>0&&shownHp<hpKey.Item1)Damage(Mathf.Clamp01((hpKey.Item1-shownHp)/(float)Mathf.Max(1,v.maxHp)*6f+.25f));
  if((shownHp,v.maxHp)!=hpKey){hpKey=(shownHp,v.maxHp);hp.style.fontSize=(v.hp.ToString()+v.maxHp).Length>7?10:11;hp.text=shownHp+"<color="+VbTokens.Hex(VbTokens.Text3)+">/"+v.maxHp+"</color>";}
  if(v.barrier!=barrierKey){barrierKey=v.barrier;barrier.text=v.barrier>0?"+"+v.barrier:"";}VbMotion.Show(barrier,v.barrier>0,MotionKind.Zoom);
  statuses.Bind(v.Fallen?null:v.effects);
  VbMotion.Show(skills,!v.Fallen);VbMotion.Show(fallen,v.Fallen,MotionKind.Rise);fallenHint.Show(v.canRevive);
  for(int i=0;i<2;i++)if(i<v.skills.Length)buttons[i].Bind(v.skills[i]);
  return this;
 }
}

/// <summary>Roster tile: portrait, name, role and (when picked) its squad slot number. Tap opens details. A narrow board (<c>vb-compact</c>) shows the role as its icon alone.</summary>
[UxmlElement] public partial class UnitTile:TileComponent {
 public string ViewId {get;set;}="";
 public UnitTile():base(ClassName){}
 /// <summary><paramref name="tier"/> frames the tile in its rarity colour; <paramref name="locked"/> says how to get a locked operative
 /// ("Pull in rifts"), which stays visible but cannot be picked.</summary>
 public UnitTile Bind(OperativeView v,int slot,bool focused,RarityTier tier=RarityTier.None,string locked=null){bool isLocked=!string.IsNullOrEmpty(locked);
  Picture=v.portrait;Kit=v.kit;Title=v.name;Caption=isLocked?locked:v.role;CaptionIcon=isLocked?"lock":v.roleIcon;
  State=(slot>0&&!isLocked?TileState.In:TileState.Idle)|(focused?TileState.Focused:TileState.Idle)|(isLocked?TileState.Locked:TileState.Idle);Order=slot;Tier=tier;
  tooltip=v.name+", "+v.role+(isLocked?", locked: "+locked:slot>0?", squad slot "+slot:", not in squad");return this;
 }
}

/// <summary>One of the squad slots on the selection screen: the operative (its name alone: the code is not repeated), or a dashed empty slot.</summary>
[UxmlElement] public partial class UnitSlot:TileComponent {
 public new const string ClassName="vb-slot";
 public UnitSlot():base(ClassName){}
 public UnitSlot Bind(OperativeView v,int slot){
  bool has=v!=null;EmptyLabel="SLOT "+slot;State=has?TileState.In:TileState.Empty;
  if(has){Picture=v.portrait;Kit=v.kit;Order=slot;Title=v.name;Caption=null;}
  return this;
 }
}

/// <summary>The unit sheet: stats, then the passive, the basic attack and both skills (each with its cost and cooldown where it has one), the lore prompt after them, close and add/remove. Stability shows only when it varies between units;
/// emphasis marks in the passive text render as emphasis (<see cref="Markup"/>).</summary>
[UxmlElement] public partial class UnitDetail:DetailView {
 public const string ClassName="vb-details";
 readonly VisualElement kit,skills,loreBox;readonly Label title,role,slot,lore;readonly LabelComponent loreTitle;readonly VbIcon roleIcon;readonly StatReadout hp,armor,stability;readonly VbButton toggle;
 public event Action Closed,Toggled;
 public UnitDetail(){
  AddToClassList(ClassName);
  var head=Ui.Box(this,ClassName+"__head");kit=Ui.Box(head,ClassName+"__kit");title=Ui.Text(head,"",ClassName+"__name");
  var r=Ui.Box(head,ClassName+"__role");roleIcon=new VbIcon("striker",14).Into(r);role=Ui.Text(r,"",ClassName+"__role-text");slot=Ui.Text(head,"",ClassName+"__slot");
  new IconButtonComposite("close","Close details",()=>Closed?.Invoke(),16){Quiet=true}.Classes(ClassName+"__close").Into(head);
  var stats=Ui.Box(this,ClassName+"__stats");hp=new StatReadout("HEALTH","0").Into(stats);armor=new StatReadout("ARMOR","0").Into(stats);stability=new StatReadout("STABILITY","0").Into(stats);armor.AddToClassList("vb-stat--neutral");stability.AddToClassList("vb-stat--neutral");
  skills=Ui.Box(this,ClassName+"__skills");loreBox=Ui.Box(this,ClassName+"__lore-box");loreTitle=new LabelComponent("Lore").Into(loreBox);lore=Ui.Text(loreBox,"","vb-t-body",ClassName+"__lore");toggle=new VbButton("ADD TO SQUAD",ButtonVariant.Light,null,"add",()=>Toggled?.Invoke()).Classes(ClassName+"__toggle");Ui.Actions(this,toggle).AddToClassList(ClassName+"__actions");
 }
 /// <summary>What the sheet lists, in order: the passive, the basic attack, then the unit's skills. A passive or a basic attack the unit does not have is left out.</summary>
 public static System.Collections.Generic.List<SkillView> Rows(OperativeView v){
  var rows=new System.Collections.Generic.List<SkillView>(6);
  if(!string.IsNullOrEmpty(v.passive))rows.Add(new SkillView{label=v.passive,icon=v.passiveIcon,slot="P",kind="Passive",description=v.passiveText,fullDescription=v.passiveFullText});
  if(v.basic!=null&&!string.IsNullOrEmpty(v.basic.label))rows.Add(v.basic);
  if(v.skills!=null)rows.AddRange(v.skills);return rows;
 }
 /// <param name="slot">Squad slot (1–4), or 0 when not in the squad.</param>
 /// <param name="squadFull">Adding is blocked with a reason when the squad already has four.</param>
 /// <param name="showStability">Whether the stability read-out shows; <see cref="SquadView"/> passes false until it differs between operatives.</param>
 public UnitDetail Bind(OperativeView v,int slot,bool squadFull,float maxHp=1580,float maxArmor=26,float maxStability=140,bool showStability=true){
  kit.style.backgroundColor=v.kit;title.text=v.name.ToUpperInvariant();role.text=v.role.ToUpperInvariant();roleIcon.Icon=v.roleIcon;
  this.slot.text=slot>0?"SLOT "+slot:"NOT IN SQUAD";this.slot.EnableInClassList(ClassName+"__slot--in",slot>0);
  hp.Set(Ui.Number(v.maxHp),null,v.maxHp/maxHp);armor.Set(v.armor.ToString(),null,v.armor/maxArmor);stability.Set(v.maxStability.ToString(),null,v.maxStability/maxStability);stability.Show(showStability);
  Keyed.Bind(skills,Rows(v),s=>s.slot+"/"+s.kind,_=>new SkillSummary(),(row,s)=>row.Bind(s));
  lore.text=v.lore??"";loreBox.Show(lore.text.Length>0);
  bool inSquad=slot>0;toggle.Title=inSquad?"REMOVE FROM SQUAD":"ADD TO SQUAD";toggle.Icon=inSquad?"remove":"add";toggle.Variant=inSquad?ButtonVariant.Danger:ButtonVariant.Light;toggle.Blocked=!inSquad&&squadFull?"Squad full · remove one first":"";
  return this;
 }
}

/// <summary>Post-run contribution for one unit (popover content).</summary>
[UxmlElement] public partial class UnitStats:DetailView {
 public const string ClassName="vb-contribution";
 readonly VisualElement portrait,grid;readonly ProgressBar shareBar;readonly Label title,fell,share;
 public UnitStats(){
  AddToClassList(ClassName);
  var head=Ui.Box(this,ClassName+"__head");portrait=VbShape.Attach(Ui.Box(head,ClassName+"__portrait"));var h=Ui.Box(head,ClassName+"__head-text");title=Ui.Text(h,"",ClassName+"__name");fell=Ui.Text(h,"",ClassName+"__fell");
  var s=Ui.Box(this,ClassName+"__share");var line=Ui.Box(s,ClassName+"__share-line");Ui.Text(line,"Share of squad damage",ClassName+"__share-label");share=Ui.Text(line,"",ClassName+"__share-value");
  shareBar=new ProgressBar().Classes(ClassName+"__share-bar").Into(s);grid=Ui.Box(this,ClassName+"__grid");
 }
 public UnitStats Bind(ContributionView c){
  var v=c.operative;Ui.Image(Ui.Crop(portrait),v.portrait);title.text=v.name.ToUpperInvariant();fell.text=c.fellWave>0?"FELL WAVE "+c.fellWave:"";fell.Show(c.fellWave>0);
  share.text=Mathf.RoundToInt(c.share*100)+"%";shareBar.Tint(v.kit).Value=c.share;
  Keyed.Bind(grid,new[]{("DAMAGE",Ui.Number(c.damage)),("KILLS",Ui.Number(c.kills)),("SKILLS CAST",Ui.Number(c.casts)),("POWER SPENT",Ui.Number(c.power)),(c.extraLabel.ToUpperInvariant(),c.extraValue),("WAVES SURVIVED",c.fellWave>0?(c.fellWave-1).ToString():"ALL")},x=>x.Item1,_=>new FactCell(ClassName+"__cell",ClassName+"__key",ClassName+"__value"),(cell,x)=>cell.Bind(x.Item1,x.Item2));
  return this;
 }
}

/// <summary>Results cell: the unit's name and, when it fell, FELL Wn (a unit still standing says nothing); hover, focus or tap shows <see cref="UnitStats"/>.</summary>
[UxmlElement] public partial class ResultSummary:SurfaceView {
 public const string ClassName="vb-result";
 readonly Label title,fell;readonly VisualElement kit;ContributionView view;
 public Popover Details {get;}
 public ResultSummary(){
  AddToClassList(ClassName);pickingMode=PickingMode.Position;focusable=true;title=Ui.Text(this,"",ClassName+"__name");fell=Ui.Text(this,"",ClassName+"__fell");kit=Ui.Box(this,ClassName+"__kit");
  Details=Popover.Attach(this,()=>view==null?null:new UnitStats().Bind(view),PopoverPlacement.Above);
 }
 public ResultSummary Bind(ContributionView c){view=c;title.text=c.fellWave>0?"<s>"+c.operative.name.ToUpperInvariant()+"</s>":c.operative.name.ToUpperInvariant();fell.text=c.fellWave>0?"FELL W"+c.fellWave:"";fell.Show(c.fellWave>0);kit.style.backgroundColor=c.operative.kit;EnableInClassList(ClassName+"--fallen",c.fellWave>0);return this;}
}
}
