using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
namespace Veilbreak.Gui {
/// <summary>Wave cleared: three perk cards, the chosen one explained, the squad that carries forward (no healing between waves) and
/// TAKE PERK, whose line says what the pick does next.</summary>
[UxmlElement] public partial class PerkDraftView:ScreenView {
 public const string Block="vb-draft";
 readonly LabelComponent eyebrow;readonly Label caption;readonly VisualElement cards,squad;readonly PerkDetail detail;readonly VbButton take;readonly List<PerkCard> choices=new List<PerkCard>();PerkDraftViewModel view;int shownPick=-1;
 public event Action<int> Picked;
 public event Action Take;
 /// <summary>The carry-forward frame was tapped: show the run's active perks.</summary>
 public event Action Perks;
 public PerkDraftView():base(Block){
  var body=Ui.Scroll(this,"vb-page__scroll",Block+"__scroll");eyebrow=new LabelComponent("",LabelSize.Label,TextTone.Ally).Into(body);Ui.Text(body,"CHOOSE A PERK","vb-t-display");caption=Ui.Text(body,"","vb-t-caption","vb-page__caption");
  cards=Ui.Box(body,Block+"__cards");detail=new PerkDetail().Classes(Block+"__detail").Into(body);
  var carry=Ui.Frame(body,FrameKind.Slim,Block+"__squad").Clickable(()=>Perks?.Invoke());carry.name="carry";carry.tooltip="View active perks";new LabelComponent("Carries forward · no healing between waves",LabelSize.Badge).Into(carry);squad=Ui.Box(carry,"vb-grid4");
  take=new VbButton("TAKE PERK",ButtonVariant.Primary,null,"check",()=>Take?.Invoke()){name="take"}.Into(this);
 }
 protected override IEnumerable<VisualElement> DealItems(){foreach(var c in choices)if(c.style.display!=DisplayStyle.None)yield return c;}
 public PerkDraftView Bind(PerkDraftViewModel v){
  view=v;eyebrow.text="Wave "+v.wave.ToString("00")+" cleared";caption.text="+"+Ui.Number(v.bonus)+" wave bonus · score "+Ui.Number(v.score)+" · perks last the whole run";
  while(choices.Count<v.choices.Length){int k=choices.Count;var c=new PerkCard().Into(cards);c.Picked+=()=>Picked?.Invoke(k);choices.Add(c);}
  for(int i=0;i<choices.Count;i++){choices[i].Show(i<v.choices.Length);if(i<v.choices.Length)choices[i].Bind(v.choices[i],i==v.pick);}
  bool has=v.pick>=0&&v.pick<v.choices.Length;VbMotion.Show(detail,has,MotionKind.Rise);
  if(has){var p=v.choices[v.pick];detail.Bind(p,true);if(shownPick>=0&&shownPick!=v.pick)VbMotion.In(detail,MotionKind.Fade);take.Sub=p.name+" → "+p.next+" · then wave "+(v.wave+1).ToString("00")+", focus refills";}
  shownPick=has?v.pick:-1;take.Blocked=has?"":"Pick a perk";
  squad.Clear();foreach(var a in v.squad){var cell=Ui.Box(squad,Block+"__cell");var line=Ui.Box(cell,"vb-row","vb-row--between");Ui.Text(line,a.code,Block+"__code");
   Ui.Text(line,a.hp>0?UnityEngine.Mathf.RoundToInt(100f*a.hp/UnityEngine.Mathf.Max(1,a.maxHp))+"%":"OUT","vb-t-badge");new HealthBar().Set(a.hp,a.maxHp,0,a.stability).Into(cell);}
  return this;
 }
}

/// <summary>Run over: the score and how it was made, the squad's contribution (hover or tap a cell), the perks, then MENU and RUN AGAIN.</summary>
[UxmlElement] public partial class ResultsView:ScreenView {
 public const string Block="vb-results";
 readonly LabelComponent eyebrow,perksTitle;readonly Label value,rank;readonly VisualElement ledger,squad,perks;readonly StatReadout waves,bosses,distance,time;
 public event Action Again,Menu,Squad;
 public ResultsView():base(Block){
  var body=Ui.Scroll(this,"vb-page__scroll",Block+"__scroll");eyebrow=new LabelComponent("",LabelSize.Label,TextTone.Enemy).Into(body);Ui.Text(body,"RUN OVER","vb-t-display");
  var card=Ui.Frame(body,FrameKind.Slim,"vb-frame--solid",Block+"__score");var top=Ui.Box(card,"vb-row","vb-row--between");var left=Ui.Box(top);new LabelComponent("Score").Into(left);value=Ui.Text(left,"0",Block+"__value");rank=Ui.Text(top,"",Block+"__rank");
  ledger=Ui.Box(card,Block+"__ledger");var stats=Ui.Box(card,"vb-row",Block+"__stats");waves=new StatReadout("WAVES","0").Into(stats);bosses=new StatReadout("BOSSES","0").Into(stats);distance=new StatReadout("DISTANCE","").Into(stats);time=new StatReadout("SIM TIME","").Into(stats);
  new LabelComponent("Squad · hover or tap for contribution").Classes("vb-page__label").Into(body);squad=Ui.Box(body,"vb-grid4");
  perksTitle=new LabelComponent("").Classes("vb-page__label").Into(body);perks=Ui.Box(body,"vb-row","vb-row--wrap",Block+"__perks");
  var row=Ui.Box(this,"vb-row",Block+"__row");new VbButton("MENU",ButtonVariant.Secondary,null,null,()=>Menu?.Invoke()){name="menu"}.Classes(Block+"__menu").Into(row);
  new VbButton("SQUAD",ButtonVariant.Secondary,null,null,()=>Squad?.Invoke()){name="squad"}.Classes(Block+"__menu").Into(row);
  new VbButton("RUN AGAIN",ButtonVariant.Primary,null,"chevron",()=>Again?.Invoke()){name="again"}.Classes(Block+"__again").Into(row);
 }
 public ResultsView Bind(ResultsViewModel v){
  eyebrow.text=v.cause+" · wave "+v.wave;value.text=Ui.Number(v.score);rank.text=v.rank>0?"#"+v.rank+" OF "+v.of:"";rank.Show(v.rank>0);
  ledger.Clear();foreach(var l in v.ledger)new KeyValueRow(l.label,l.value).Into(ledger);
  waves.Set(v.wave.ToString());bosses.Set(v.bosses.ToString());distance.Show(!string.IsNullOrEmpty(v.distance));if(!string.IsNullOrEmpty(v.distance)){var parts=v.distance.Split(' ');distance.Set(parts[0],parts.Length>1?parts[1]:null);}
  time.Show(!string.IsNullOrEmpty(v.simTime));time.Set(v.simTime??"");
  squad.Clear();foreach(var c in v.squad)new SquadResult().Bind(c).Into(squad);
  perksTitle.text="Perks · "+v.perks.Length;perksTitle.Show(v.perks.Length>0);perks.Clear();foreach(var p in v.perks)new PerkChip().Bind(p).Into(perks);
  VbMotion.StaggerChildren(squad,MotionKind.Rise,VbMotion.StaggerStep*2);VbMotion.StaggerChildren(perks,MotionKind.Rise,VbMotion.StaggerStep*6,30);
  return this;
 }
}

/// <summary>Top scores on this device with the current run framed, then runs kept from older rules, in one <see cref="ListLayout"/>.</summary>
[UxmlElement] public partial class HighScoresView:ScreenView {
 public const string Block="vb-scores";
 readonly ScreenHeader header;readonly Label caption;readonly ListLayout list;
 public event Action Back;
 public HighScoresView():base(Block,true){
  header=new ScreenHeader("HIGH SCORES","TOP 20").Into(this);header.Back+=()=>Back?.Invoke();caption=Ui.Text(this,"On this device. Ties break by wave, then shortest sim time.","vb-t-caption","vb-page__caption");
  list=Ui.List(this,"No runs yet. Your first run lands here.");Ui.Text(this,"Local scores only · not an online ranking","vb-t-caption","vb-page__footer");
 }
 /// <param name="ranked">Ranked runs (the view shows at most 20).</param><param name="historical">Runs under older rules, listed after a label.</param>
 public HighScoresView Bind(IReadOnlyList<ScoreView> ranked,IReadOnlyList<ScoreView> historical=null,string caption=null){
  if(caption!=null)this.caption.text=caption;list.Clear();int n=Math.Min(20,ranked?.Count??0);header.Meta="TOP "+Math.Max(n,0);
  for(int i=0;i<n;i++)new ScoreRow().Bind(ranked[i]).Into(list);
  if(historical!=null&&historical.Count>0){new LabelComponent("Older rules").Classes(Block+"__older").Into(list);foreach(var s in historical)new ScoreRow().Bind(s).Classes(Block+"__historical").Into(list);}
  return this;
 }
}

/// <summary>The run's perks: category totals, the stacks (tap one) and the picked one explained. Opens from the draft and from pause.</summary>
[UxmlElement] public partial class ActivePerksView:ScreenView {
 public const string Block="vb-build";
 readonly ScreenHeader header;readonly VisualElement totals;readonly ListLayout list;readonly PerkDetail detail;readonly List<PerkRow> rows=new List<PerkRow>();int shownPick=-1;
 public event Action<int> Picked;
 public event Action Back;
 public ActivePerksView():base(Block,true){
  header=new ScreenHeader("ACTIVE PERKS","").Into(this);header.Back+=()=>Back?.Invoke();totals=Ui.Frame(this,FrameKind.Slim,Block+"__summary");
  list=Ui.List(this,"No perks yet. Clear a wave to draft one.",Block+"__list");detail=new PerkDetail().Classes(Block+"__detail").Into(this);
  Ui.Text(this,"Percent bonuses add within a category, then apply once","vb-t-caption","vb-page__footer");
 }
 public ActivePerksView Bind(ActivePerksViewModel v){
  int stacks=0;foreach(var p in v.perks)stacks+=p.stack;header.Meta=stacks+" · WAVE "+v.wave.ToString("00");
  totals.Clear();foreach(var t in v.totals){var cell=Ui.Box(totals,Block+"__bonus");Ui.Text(cell,t.label,"vb-t-badge");Ui.Text(cell,t.value,Block+"__bonus-value");}totals.Show(v.totals.Length>0&&v.perks.Length>0);
  while(rows.Count<v.perks.Length){int k=rows.Count;var r=new PerkRow().Into(list);r.Picked+=()=>Picked?.Invoke(k);rows.Add(r);}
  for(int i=0;i<rows.Count;i++){bool on=i<v.perks.Length;rows[i].Show(on);if(on)rows[i].Bind(v.perks[i],i==v.pick);}
  bool has=v.pick>=0&&v.pick<v.perks.Length;VbMotion.Show(detail,has,MotionKind.Rise);if(has){detail.Bind(v.perks[v.pick],false);if(shownPick>=0&&shownPick!=v.pick)VbMotion.In(detail,MotionKind.Fade);}shownPick=has?v.pick:-1;
  return this;
 }
}

/// <summary>A fallen operative to restore (a revive perk): the fallen as tiles; tapping one is the choice.</summary>
[UxmlElement] public partial class RevivePickView:ScreenView {
 public const string Block="vb-revive";
 readonly ScreenHeader header;readonly Label caption;readonly VisualElement grid;
 public event Action<int> Picked;
 public event Action Back;
 public RevivePickView():base(Block,true){header=new ScreenHeader("RESTORE","CHOOSE ONE").Into(this);header.Back+=()=>Back?.Invoke();caption=Ui.Text(this,"","vb-t-caption","vb-page__caption");grid=Ui.Box(this,"vb-grid4","vb-grid4--wrap");}
 /// <param name="fallen">The fallen operatives; <see cref="Picked"/> reports an index into it.</param>
 public RevivePickView Bind(string perk,OperativeView[] fallen){
  caption.text=perk+" revives one fallen operative at 35% health.";grid.Clear();
  for(int i=0;i<fallen.Length;i++){int k=i;var tile=new OperativeTile().Into(grid).Bind(fallen[i],0,false);tile.Picked+=()=>Picked?.Invoke(k);}
  VbMotion.StaggerChildren(grid,MotionKind.Zoom);return this;
 }
}
}
