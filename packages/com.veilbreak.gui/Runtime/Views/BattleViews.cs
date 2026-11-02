using System;
using UnityEngine.UIElements;
namespace Veilbreak.Gui {
/// <summary>The whole battle board on a <see cref="BoardLayout"/>: the HUD band (wave, score, autoplay, speed, Power and focus), the boss
/// bar, the announcer lane, the world (put a <see cref="WorldLayer"/> in <see cref="BoardLayout.World"/>), four operative cards and the
/// cancel band that shows above them while an aim is live. Bind it every frame; the game owns input
/// (<c>Card(i).Skill(0).AddManipulator(aimDrag)</c>). The bands are named <c>hud</c>, <c>battlefield</c> and <c>cards</c> for hosts and tests.
/// <see cref="Reveal"/> fades the band and cards in or out as a whole (a run opening, a deploy); the boss bar, cards and cancel band animate
/// their own changes through <see cref="VbMotion"/>.</summary>
[UxmlElement] public partial class BattleHudView:BoardLayout {
 public new const string ClassName="vb-battle";
 readonly EffectStrip targetEffects;readonly Label targetTitle;
 readonly OperativeCard[] cards=new OperativeCard[4];
 public HudTop Hud {get;}
 public BossBar Boss {get;}
 /// <summary>The heads-up lane; feed confirmed events with <c>Announcer.Say</c>.</summary>
 public Announcer Announcer {get;}
 /// <summary>"RELEASE HERE TO CANCEL · NO POWER SPENT": a release over it cancels an aim.</summary>
 public Label CancelBand {get;}
 public event Action Pause,Auto,Speed;
 public event Action<QueueView> QueueCancel;
 public BattleHudView(){
  AddToClassList(ClassName);Top.name="hud";World.name="battlefield";Bottom.name="cards";
  Hud=new HudTop().Into(Top);Hud.PauseClicked+=()=>Pause?.Invoke();Hud.AutoClicked+=()=>Auto?.Invoke();Hud.SpeedClicked+=()=>Speed?.Invoke();Hud.QueueCancel+=q=>QueueCancel?.Invoke(q);
  Boss=new BossBar().Classes(ClassName+"__boss").Into(Top);Boss.Show(false);
  targetTitle=Ui.Text(Top,"","vb-t-badge");targetEffects=new EffectStrip().Into(Top);TargetEffects("",null);
  var lane=new GlitchText("").Classes("vb-announcement",ClassName+"__announcer").Into(Top);lane.name="hud-message";Announcer=new Announcer(lane);
  var row=Ui.Box(Bottom,ClassName+"__cards");for(int i=0;i<4;i++)cards[i]=new OperativeCard().Classes(ClassName+"__card").Into(row);
  CancelBand=Ui.Text(Bottom,"RELEASE HERE TO CANCEL · NO POWER SPENT",ClassName+"__cancel");CancelBand.name="cancel-hint";CancelBand.Show(false);
 }
 /// <summary>0–3, for attaching aim and move manipulators to its skills.</summary>
 public OperativeCard Card(int index)=>cards[index];
 public void TargetEffects(string title,EffectInstance[] effects){targetTitle.text=title;targetTitle.Show(!string.IsNullOrEmpty(title));targetEffects.Bind(effects);}
 /// <param name="boss">Null hides the boss bar; mutable cached models are read on every bind.</param>
 public BattleHudView Bind(HudView hud,OperativeView[] operatives,BossView boss=null){
  Hud.Bind(hud);VbMotion.Show(Boss,boss!=null,MotionKind.Drop);if(boss!=null)Boss.Bind(boss);
  for(int i=0;i<4;i++){bool on=operatives!=null&&i<operatives.Length&&operatives[i]!=null;VbMotion.Show(cards[i],on,MotionKind.Rise);if(on)cards[i].Bind(operatives[i]);}
  return this;
 }
 /// <summary>Shows the cancel band while an aim is live.</summary>
 public BattleHudView Aiming(bool on){VbMotion.Show(CancelBand,on,MotionKind.Rise);return this;}
 /// <summary>Fades the HUD band and the cards to <paramref name="shown"/> (0–1) and makes them unpickable while hidden.</summary>
 public BattleHudView Reveal(float shown){shown=UnityEngine.Mathf.Clamp01(shown);VbMotion.Scrub(Top,shown,MotionRecipes.HudTop);for(int i=0;i<cards.Length;i++)VbMotion.Scrub(cards[i],UnityEngine.Mathf.Clamp01(shown*1.25f-i*.08f),MotionRecipes.HudCard);Bottom.visible=shown>0;return this;}
 /// <summary>Shows or hides the band and cards at once (menus keep the board for the world's gestures).</summary>
 public BattleHudView Bands(bool on){VbMotion.Show(Top,on,MotionKind.Drop);VbMotion.Show(Bottom,on,MotionKind.Rise);return this;}
}

/// <summary>The crossroads screen around <see cref="RouteChoice"/>: page padding, the route backdrop and one accepted choice.</summary>
[UxmlElement] public partial class RouteChoiceView:ScreenView {
 public const string Block="vb-route-view";
 public RouteChoice Choice {get;}
 public event Action<int> Chosen;
 public RouteChoiceView():base(Block){Choice=new RouteChoice().Into(this);Choice.Chosen+=side=>Chosen?.Invoke(side);}
 protected override System.Collections.Generic.IEnumerable<VisualElement> DealItems()=>Choice.Choices;
 public RouteChoiceView Bind(RouteView view){Choice.Bind(view);return this;}
}

/// <summary>The night market screen (design name MerchantView; <see cref="MerchantView"/> is its view model) around <see cref="MerchantScreen"/>. Transactions are confirmed by the host, which binds again.</summary>
[UxmlElement] public partial class MerchantScreenView:ScreenView {
 public const string Block="vb-merchant-view";
 public MerchantScreen Market {get;}
 public event Action<int> Bought,HealthSold;public event Action<string> PerkSold;public event Action Left;
 public MerchantScreenView():base(Block){Market=new MerchantScreen().Into(this);Market.Bought+=i=>Bought?.Invoke(i);Market.HealthSold+=i=>HealthSold?.Invoke(i);Market.PerkSold+=id=>PerkSold?.Invoke(id);Market.Left+=()=>Left?.Invoke();}
 protected override System.Collections.Generic.IEnumerable<VisualElement> DealItems()=>Market.Choices;
 /// <summary>Rebinds and keeps the scroll position, so a confirmed trade does not jump the list.</summary>
 public MerchantScreenView Bind(MerchantView view){float y=Market.ScrollOffset;Market.Bind(view);Market.schedule.Execute(()=>Market.ScrollOffset=y);return this;}
}
}
