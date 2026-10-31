using UnityEngine;
using UnityEngine.UIElements;
using Veilbreak.Gui;
// Open Gallery.unity, or add to a GameObject with a UIDocument on VeilbreakPortrait.asset (portrait reference 390 × 844).
// Fills the "gallery" ScrollView of Gallery.uxml, or a new one.
[RequireComponent(typeof(UIDocument))]
public sealed class GuiGallery:MonoBehaviour {
 public Texture2D portrait;
 void OnEnable(){
  var root=GetComponent<UIDocument>().rootVisualElement;root.style.backgroundColor=new Color(.02f,.008f,.03f);
  var page=root.Q<ScrollView>("gallery")??new ScrollView().Into(root);page.Clear();page.style.flexGrow=1;page.contentContainer.style.paddingLeft=16;page.contentContainer.style.paddingRight=16;
  void Title(string t)=>Ui.Text(page,t,"vb-t-label").style.marginTop=18;
  var hud=new HudView{wave=8,score=12450,power=34,focus=62,slow=true,queue=new[]{new QueueView{who="Aegis",skill="Absolute Wall",cost=40,eta="0.5s"},new QueueView{who="Nyra",skill="White Noise",cost=40,eta="3.8s"}}};
  Title("HudTop · hover the resources for the queue");new HudTop().Into(page).Bind(hud);
  Title("BossBar");new BossBar().Into(page).Bind(new BossView{name="Signal Warden",health=.72f,stability=.4f,detail="72% · enrage at 35%",shielded=true,shieldDetail="2 pylons",interruptible=true});
  Title("SkillButton states");var skills=Ui.Box(page,"vb-gallery-row");skills.style.flexDirection=FlexDirection.Row;
  foreach(SkillState s in System.Enum.GetValues(typeof(SkillState))){var b=new SkillButton().Into(skills).Bind(new SkillView{label="Needle Storm",icon="SkillAriaNeedleStorm",cost=18,state=s,charged=.4f,need=6,seconds=1.4f});b.style.marginRight=8;}
  Title("OperativeCard modes");var cards=Ui.Box(page);cards.style.flexDirection=FlexDirection.Row;
  foreach(ControlMode m in System.Enum.GetValues(typeof(ControlMode))){var c=new OperativeCard().Into(cards).Bind(Operative(m));c.style.marginRight=8;}
  Title("PerkCard · general, operative, rare, epic");var perks=Ui.Box(page);perks.style.flexDirection=FlexDirection.Row;
  new PerkCard().Into(perks).Bind(Perk(PerkTier.General),true);new PerkCard().Into(perks).Bind(Perk(PerkTier.Operative),false);new PerkCard().Into(perks).Bind(Perk(PerkTier.Rare),false);
  new PerkCard().Into(perks).Bind(Perk(PerkTier.Epic),false);
  Title("PerkDetail · PerkChip · PerkRow");new PerkDetail().Into(page).Bind(Perk(PerkTier.General),true);new PerkChip().Into(page).Bind(Perk(PerkTier.General));new PerkRow().Into(page).Bind(Perk(PerkTier.Operative),true);
  Title("GlitchText · click the action to replay the projection");var projection=new GlitchText("WAVE 08 · SIGNAL ACQUIRED").Classes("vb-t-title").Into(page);new VbButton("PULSE",ButtonVariant.Secondary,null,null,()=>Glitch.Pulse(projection)).Into(page);
  Title("RouteChoice");new RouteChoice().Into(page).Bind(new RouteView{wave=3,left=new RoadView{title="Combat",hint="Common cache · chance of rare"},right=new RoadView{kind="merchant",title="Merchant",icon="currency",hint="Trade stacks or HP · buy rare and epic"}});
  Title("MerchantScreen");var market=new MerchantScreen().Into(page).Bind(new MerchantView{credits=120,offers=new[]{new MerchantOfferView{perk=Perk(PerkTier.Rare),price=80},new MerchantOfferView{perk=Perk(PerkTier.Rare),price=80,taken=true},new MerchantOfferView{perk=Perk(PerkTier.Epic),price=150}},health=new[]{new MerchantHealthView{actor=0,name="Aria",hp=800,maxHp=1000,cost=250,payout=30}}});market.style.height=650;
  Title("Buttons");new VbButton("Start run",ButtonVariant.Primary,"Aria · Rook · Aegis · Nyra","chevron").Into(page);new VbButton("Start run",ButtonVariant.Primary,null,"chevron"){Blocked="Pick 1 more to start"}.Into(page);
  new VbButton("Remove from squad",ButtonVariant.Danger,null,"remove").Into(page);new VbButton("Add to squad",ButtonVariant.Light,null,"add").Into(page);
  Title("Form controls");new VbSlider("Music",.6f).Into(page);new VbSwitch("HAPTICS","Light pulse when an order is accepted",true).Into(page);new VbSegmented(new[]{("LOW","Stable 30 FPS"),("STANDARD","60 FPS target")},1).Into(page);
  Title("Squad");var row=Ui.Box(page);row.style.flexDirection=FlexDirection.Row;new SquadSlot().Into(row).Bind(Operative(ControlMode.Auto),1);new SquadSlot().Into(row).Bind(null,2);new OperativeTile().Into(row).Bind(Operative(ControlMode.Auto),1,true);
  new OperativeDetails().Into(page).Bind(Operative(ControlMode.Auto),1,true);
  Title("QueueChip · PowerStrip with reserve and queue");var queue=new[]{new QueueView{who="AE",skill="Absolute Wall",cost=40,eta="0.5s",manual=true},new QueueView{who="NY",skill="White Noise",cost=40,eta="3.8s"}};
  new PowerStrip().Into(page).Set(34,100,"+12/s","Needle Storm −18 → 16 left",16,34).Reserve(25).Queue(queue,q=>Debug.Log("cancel "+q.skill)).Legend("┊ autoplay reserve · × cancels a manual entry");
  Title("HealthBar sizes, ghost and caption · StatReadout delta · StatusChip time");new HealthBar().Into(page).Set(620,1000,80,.7f,false,false,840).Caption("HP 620 / 1000");new HealthBar().Into(page).Set(300,1000,0,.2f,true,true,450).Size(MeterSize.Large);
  var stats=Ui.Box(page,"vb-row");new StatReadout("DAMAGE","1,240").Into(stats).Delta("+12%");new StatReadout("COST","40").Into(stats).Delta("+5",up:false);new StatusChip(StatusKind.Silenced,"SILENCED","no skills","2.1s").Into(stats);
  Title("Panels · ally, power, enemy, solid with head");var panels=Ui.Box(page,"vb-row");foreach(var v in new[]{"vb-panel--ally","vb-panel--power","vb-panel--enemy","vb-panel--solid"}){var panel=Ui.Panel(panels,v);Ui.Head(panel,v.Substring(10).ToUpperInvariant(),"META");Ui.Text(panel,"Body","vb-t-caption");panel.style.marginRight=6;}
  Title("Roster tiers and a locked operative");var tiers=Ui.Box(page,"vb-row");foreach(RarityTier t in System.Enum.GetValues(typeof(RarityTier)))new OperativeTile().Into(tiers).Bind(Operative(ControlMode.Auto),0,false,t,t==RarityTier.Gold?"Pull in rifts":null).style.marginRight=6;
  Title("WorldLabel · WorldMarker · OffscreenIndicator");var world=Ui.Box(page,"vb-row");new WorldLabel().Into(world).Bind(WorldLabelKind.Aim,"NEEDLE STORM","18 POWER · fires now");new WorldLabel().Into(world).Bind(WorldLabelKind.Telegraph,"SLAM 1.2s",team:WorldTeam.Enemy);new WorldMarker().Into(world).Bind(WorldMarkerKind.Enemy);new OffscreenIndicator().Into(world).Bind("BREACH",WorldTeam.Enemy,3,true);
  foreach(var e in world.Children()){e.style.position=Position.Relative;e.style.translate=new Translate(0,0);e.style.marginRight=10;}
  Title("Menu surfaces · HoloFrame, HoloPlatform, RevealCard");var holo=new HoloFrame("Last match").Into(page);Ui.Text(holo,"Wave 12 · 24,610 points · 2 bosses","vb-t-body");
  var lineup=Ui.Box(page,"vb-row");new HoloPlatform().Into(lineup).Bind("Aria","Lv 7 · front",PlatformState.Selected);new HoloPlatform().Into(lineup).Bind(null,null,PlatformState.Open);
  var cards2=Ui.Box(page,"vb-row");new RevealCard().Into(cards2).Bind(RarityTier.Gold,"Sol","SO · Breaker",flipped:true);new RevealCard().Into(cards2).Bind(RarityTier.Violet,"Hex","HX · Debuffer",dupe:"Resonance R2",flipped:true);new RevealCard().Into(cards2).Bind(RarityTier.Blue,"Visor","Optic",item:true);
  Title("RuleRow");new RuleRow().Into(page).Bind(new RuleView{index=1,conditions=new[]{new RuleCondition{text="Power ≥",value="80"},new RuleCondition{text="enemy within",value="6",after="m"}},action="Zero Line",cost=40,note="Runs only at 80+ Power: 40 cost + 40 reserve"});
  new RuleRow().Into(page).Bind(new RuleView{index=2,conditions=new[]{new RuleCondition{text="ally below",value="35%"}},action="Reconstruct",cost=40,warning=true,note="Never fires while the manual queue holds entries"});
  Title("Results");new SquadResult().Into(page).Bind(new ContributionView{operative=Operative(ControlMode.Fallen),fellWave=12,damage=48200,kills=162,casts=74,power=1960,share=.47f,extraValue="312"});
  new ScoreRow().Into(page).Bind(new ScoreView{rank=2,score=24610,wave=12,bosses=2,squad=new[]{"AR","RO","AE","NY"},current=true});
 }
 OperativeView Operative(ControlMode m)=>new OperativeView{id="aria",name="Aria",code="AR",role="Striker",roleIcon="striker",kit=new Color(.23f,.84f,.9f),portrait=portrait,hp=m==ControlMode.Fallen?0:820,maxHp=1000,barrier=60,armor=10,stability=70,maxStability=110,mode=m,manualSeconds=2.4f,passive="Momentum tax",passiveText="Basic kinetic hits slow the target by 2%, stacking to 10%.",
  statuses=new[]{StatusKind.Barrier},skills=new[]{new SkillView{label="Needle Storm",icon="SkillAriaNeedleStorm",cost=18,slot="A",kind="Active",description="Piercing fan and armor shred",cooldownSeconds=4},new SkillView{label="Terminal Vector",icon="SkillAriaTerminalVector",cost=40,slot="S",kind="Signature",state=SkillState.Cooldown,charged=.4f,description="Piercing execution beam",cooldownSeconds=13.2f}}};
 PerkView Perk(PerkTier t)=>t==PerkTier.Operative?new PerkView{name="Terminal Aperture",category="Skill",tag="Aria only",tier=t,icon="SkillAriaTerminalVector",art=portrait,effect="+25% Terminal Vector width",max=3,now="None",next="+25% Terminal Vector width"}
  :t==PerkTier.Epic?new PerkView{name="Surge Engine",category="Support",tag="Epic",tier=t,icon="power",effect="150 Power capacity and +25% regeneration",max=1,now="100 capacity",next="150 capacity · +25% regen"}
  :t==PerkTier.Rare?new PerkView{name="Emergency Reassembly",category="Defense",tag="Rare",tier=t,icon="add",effect="Revive one fallen fighter at 35% health",now="Revive one fallen fighter at 35% health",next="Revive one fallen fighter at 35% health"}
  :new PerkView{name="Sharpened Output",category="Offense",tag="General",icon="burst",effect="+8% damage",stack=2,max=5,now="+16% damage",next="+24% damage",waves=new long[]{2,5}};
}
