using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Veilbreak.Gui;
// A portrait battle board with fake data: HUD band, boss bar, the Power strip with reserve and a manual queue, operative cards,
// and a world layer (bars, combat text, labels, markers and a breach chip) over a flat "arena" projected top-down.
// Escape opens a pause sheet through the Navigator. Add to a GameObject with a UIDocument using VeilbreakPortrait.asset.
[RequireComponent(typeof(UIDocument))]
public sealed class BattleHudSample:MonoBehaviour {
 public Texture2D portrait;
 VisualElement root,field;HudTop hud;BossBar boss;OperativeCard[] cards;WorldLayer world;Navigator nav;SafeAreaRoot safe;readonly HudView view=new HudView{wave=10,score=18240,boss=true,credits=90,regen="+12/s"};
 readonly List<QueueView> queue=new List<QueueView>{new QueueView{who="AE",skill="Absolute Wall",cost=40,eta="1.2s",manual=true,actor=2,slot=2},new QueueView{who="NY",skill="White Noise",cost=40,eta="4.1s",manual=true,actor=3,slot=1}};
 float power=22,clock,hitAt;readonly Vector3[] allies={new Vector3(-1.4f,0,-1),new Vector3(1.4f,0,-1),new Vector3(-1.4f,0,-3),new Vector3(1.4f,0,-3)};
 void OnEnable(){var document=GetComponent<UIDocument>();var top=document.rootVisualElement;top.Clear();top.style.backgroundColor=new Color(.06f,.04f,.08f);
  var screen=new VisualElement{name="screen"};root=Ui.Box(top,"vb-root");root.pickingMode=PickingMode.Position;
  hud=new HudTop().Into(root);hud.PauseClicked+=Pause;boss=new BossBar().Into(root);boss.style.marginLeft=boss.style.marginRight=10;
  field=Ui.Box(root,"vb-grow");world=new WorldLayer{Project=Project}.Into(field);
  var strip=Ui.Box(root);strip.style.paddingLeft=strip.style.paddingRight=10;new PowerStrip{name="strip"}.Into(strip);
  var row=Ui.Box(root,"vb-row","vb-row--between");row.style.paddingLeft=row.style.paddingRight=row.style.paddingBottom=10;cards=new OperativeCard[4];for(int i=0;i<4;i++)cards[i]=new OperativeCard().Into(row);
  top.Add(screen);screen.Show(false);nav=new Navigator(screen){Fallback=Pause};safe=new SafeAreaRoot(root,document.panelSettings,screen){MinTop=8};
  top.RegisterCallback<KeyDownEvent>(e=>{if(e.keyCode==KeyCode.Escape){nav.Back();e.StopPropagation();}});}
 Vector2? Project(Vector3 p){var size=world.Size;if(size.x<=0)return null;return new Vector2(size.x*.5f+p.x*size.x/8,size.y*.55f-p.z*size.y/14);}
 void Pause(){if(nav.Visible)return;var page=nav.Open("pause",nav.Close);Ui.Spacer(page);var sheet=Dialog.Sheet(page,"PAUSED","Combat, focus and world travel are frozen");new VbButton("RESUME",ButtonVariant.Primary,null,"chevron",nav.Close).Into(sheet);}
 void Update(){if(root==null)return;safe.Refresh();safe.Flags(false,false,false);float dt=nav.Visible?0:Time.deltaTime;clock+=dt;power=Mathf.Min(100,power+12*dt);if(power>=40&&queue.Count>0){power-=40;queue.RemoveAt(0);}
  view.power=(int)power;view.focus=60;view.queue=queue.ToArray();view.queueTitle="MANUAL QUEUE";view.queueHint="Fires before autoplay · tap to cancel";hud.Bind(view);
  boss.Bind(new BossView{name="Signal Warden",health=.64f,stability=.35f,detail="64% · enrage at 35%",shielded=true,shieldDetail="2 pylons",interruptible=true});
  root.Q<PowerStrip>("strip").Set((int)power,100,"+12/s").Reserve(25).Queue(queue,q=>queue.Remove(q)).Legend("┊ autoplay reserve · × cancels");
  string[] names={"Aria","Rook","Aegis","Nyra"};for(int i=0;i<4;i++)cards[i].Bind(new OperativeView{id=names[i].ToLowerInvariant(),name=names[i],code=names[i].Substring(0,2).ToUpperInvariant(),kit=VbTokens.Kit(names[i].ToLowerInvariant()),portrait=portrait,hp=600+i*90,maxHp=1000,stability=80,maxStability=100,mode=i==2?ControlMode.Manual:ControlMode.Auto,skills=new[]{new SkillView{label="Skill",icon="projectile",cost=18,slot="A",state=i==2?SkillState.Queued:SkillState.Ready,queue=1},new SkillView{label="Signature",icon="burst",cost=40,slot="S",state=i==3?SkillState.Windup:SkillState.Cooldown,charged=.5f,seconds=2.2f,progress=Mathf.Repeat(clock,1)}}});
  world.ReducedMotion=false;world.BeginFrame();var bossAt=new Vector3(Mathf.Sin(clock*.5f)*1.5f,0,4);
  for(int i=0;i<4;i++){world.Bar(i,allies[i],(600+i*90)/1000f,i==2?.15f:0,WorldTeam.Ally);world.Marker(i,allies[i],i==2?WorldMarkerKind.Queued:WorldMarkerKind.Ally);}
  world.Bar(10,bossAt,.64f,0,WorldTeam.Enemy,WorldRank.Boss);world.Marker(10,bossAt,WorldMarkerKind.Enemy,40);world.Label(10,bossAt,WorldLabelKind.Telegraph,"SLAM "+(2-Mathf.Repeat(clock,2)).ToString("0.0")+"s",null,WorldTeam.Enemy);
  world.Label(20,allies[0],WorldLabelKind.Aim,"NEEDLE STORM","18 POWER · queues as #3");var diver=new Vector3(2.5f,0,-9-Mathf.Repeat(clock,3));world.Offscreen(30,diver,"BREACH",WorldTeam.Enemy,Mathf.Max(0,diver.z+13),true);
  if(clock-hitAt>.35f&&dt>0){hitAt=clock;world.Text(10,bossAt,Random.Range(40,120),Random.value<.15f?CombatTextKind.Crit:CombatTextKind.Damage);if(Random.value<.3f)world.Text(1,allies[1],Random.Range(10,40),CombatTextKind.Taken);if(Random.value<.2f)world.Text(3,allies[3],25,CombatTextKind.Heal);}
  world.EndFrame(dt);}
}
