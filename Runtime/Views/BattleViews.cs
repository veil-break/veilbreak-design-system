using System;
using UnityEngine.UIElements;
namespace Veilbreak.Gui {
/// <summary>The whole battle board on a <see cref="BoardLayout"/>: the HUD band (wave, score, autoplay, speed, Power and focus), the boss
/// bar, the announcer lane, the world (put a <see cref="WorldLayer"/> in <see cref="BoardLayout.World"/>) and four operative cards.
/// Bind it every frame; the game owns input (<c>Card(i).Skill(0).AddManipulator(aimDrag)</c>).</summary>
[UxmlElement] public partial class BattleHudView:BoardLayout {
 public new const string ClassName="vb-battle";
 readonly OperativeCard[] cards=new OperativeCard[4];
 public HudTop Hud {get;}
 public BossBar Boss {get;}
 /// <summary>The heads-up lane; feed confirmed events with <c>Announcer.Say</c>.</summary>
 public Announcer Announcer {get;}
 public event Action Pause,Auto,Speed;
 public event Action<QueueView> QueueCancel;
 public BattleHudView(){
  AddToClassList(ClassName);Hud=new HudTop().Into(Top);Hud.PauseClicked+=()=>Pause?.Invoke();Hud.AutoClicked+=()=>Auto?.Invoke();Hud.SpeedClicked+=()=>Speed?.Invoke();Hud.QueueCancel+=q=>QueueCancel?.Invoke(q);
  Boss=new BossBar().Classes(ClassName+"__boss").Into(Top);Boss.Show(false);
  Announcer=new Announcer(new GlitchText("").Classes("vb-announcement",ClassName+"__announcer").Into(Top));
  var row=Ui.Box(Bottom,ClassName+"__cards");for(int i=0;i<4;i++)cards[i]=new OperativeCard().Classes(ClassName+"__card").Into(row);
 }
 /// <summary>0–3, for attaching aim and move manipulators to its skills.</summary>
 public OperativeCard Card(int index)=>cards[index];
 /// <param name="boss">Null hides the boss bar.</param>
 public BattleHudView Bind(HudView hud,OperativeView[] operatives,BossView boss=null){
  Hud.Bind(hud);Boss.Show(boss!=null);if(boss!=null)Boss.Bind(boss);
  for(int i=0;i<4;i++){bool on=operatives!=null&&i<operatives.Length&&operatives[i]!=null;cards[i].Show(on);if(on)cards[i].Bind(operatives[i]);}
  return this;
 }
}

/// <summary>The crossroads screen around <see cref="RouteChoice"/>: page padding, the route backdrop and one accepted choice.</summary>
[UxmlElement] public partial class RouteChoiceView:ScreenView {
 public const string Block="vb-route-view";
 public RouteChoice Choice {get;}
 public event Action<int> Chosen;
 public RouteChoiceView():base(Block){Choice=new RouteChoice().Into(this);Choice.Chosen+=side=>Chosen?.Invoke(side);}
 public RouteChoiceView Bind(RouteView view){Choice.Bind(view);return this;}
}

/// <summary>The night market screen (design name MerchantView; <see cref="MerchantView"/> is its view model) around <see cref="MerchantScreen"/>. Transactions are confirmed by the host, which binds again.</summary>
[UxmlElement] public partial class MerchantScreenView:ScreenView {
 public const string Block="vb-merchant-view";
 public MerchantScreen Market {get;}
 public event Action<int> Bought,HealthSold;public event Action<string> PerkSold;public event Action Left;
 public MerchantScreenView():base(Block){Market=new MerchantScreen().Into(this);Market.Bought+=i=>Bought?.Invoke(i);Market.HealthSold+=i=>HealthSold?.Invoke(i);Market.PerkSold+=id=>PerkSold?.Invoke(id);Market.Left+=()=>Left?.Invoke();}
 /// <summary>Rebinds and keeps the scroll position, so a confirmed trade does not jump the list.</summary>
 public MerchantScreenView Bind(MerchantView view){float y=Market.ScrollOffset;Market.Bind(view);Market.schedule.Execute(()=>Market.ScrollOffset=y);return this;}
}
}
