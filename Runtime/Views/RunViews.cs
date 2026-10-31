using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
namespace Veilbreak.Gui {
/// <summary>Wave cleared: three perk cards, the chosen one explained, the squad that carries forward (no healing between waves) and
/// TAKE PERK, whose line says what the pick does next.</summary>
[UxmlElement] public partial class PerkDraftView:ScreenView {
 public const string Block="vb-draft";
 readonly LabelComponent eyebrow;readonly Label caption;readonly VisualElement cards,squad;readonly PerkDetail detail;readonly VbButton take;readonly List<PerkCard> choices=new List<PerkCard>();PerkDraftViewModel view;
 public event Action<int> Picked;
 public event Action Take;
 public PerkDraftView():base(Block){
  var body=Ui.Scroll(this,"vb-page__scroll",Block+"__scroll");eyebrow=new LabelComponent("",LabelSize.Label,TextTone.Ally).Into(body);Ui.Text(body,"CHOOSE A PERK","vb-t-display");caption=Ui.Text(body,"","vb-t-caption","vb-page__caption");
  cards=Ui.Box(body,Block+"__cards");detail=new PerkDetail().Classes(Block+"__detail").Into(body);
  var carry=Ui.Frame(body,FrameKind.Slim,Block+"__squad");new LabelComponent("Carries forward · no healing between waves",LabelSize.Badge).Into(carry);squad=Ui.Box(carry,"vb-grid4");
  take=new VbButton("TAKE PERK",ButtonVariant.Primary,null,"check",()=>Take?.Invoke()){name="take"}.Into(this);
 }
 public PerkDraftView Bind(PerkDraftViewModel v){
  view=v;eyebrow.text="Wave "+v.wave.ToString("00")+" cleared";caption.text="+"+Ui.Number(v.bonus)+" wave bonus · score "+Ui.Number(v.score)+" · perks last the whole run";
  while(choices.Count<v.choices.Length){int k=choices.Count;var c=new PerkCard().Into(cards);c.Picked+=()=>Picked?.Invoke(k);choices.Add(c);}
  for(int i=0;i<choices.Count;i++){choices[i].Show(i<v.choices.Length);if(i<v.choices.Length)choices[i].Bind(v.choices[i],i==v.pick);}
  bool has=v.pick>=0&&v.pick<v.choices.Length;detail.Show(has);
  if(has){var p=v.choices[v.pick];detail.Bind(p,true);take.Sub=p.name+" → "+p.next+" · then wave "+(v.wave+1).ToString("00")+", focus refills";}
  take.Blocked=has?"":"Pick a perk";
  squad.Clear();foreach(var a in v.squad){var cell=Ui.Box(squad,Block+"__cell");var line=Ui.Box(cell,"vb-row","vb-row--between");Ui.Text(line,a.code,Block+"__code");
   Ui.Text(line,a.hp>0?UnityEngine.Mathf.RoundToInt(100f*a.hp/UnityEngine.Mathf.Max(1,a.maxHp))+"%":"OUT","vb-t-badge");new HealthBar().Set(a.hp,a.maxHp,0,a.stability).Into(cell);}
  return this;
 }
}

/// <summary>Run over: the score and how it was made, the squad's contribution (hover or tap a cell), the perks, then MENU and RUN AGAIN.</summary>
[UxmlElement] public partial class ResultsView:ScreenView {
 public const string Block="vb-results";
 readonly LabelComponent eyebrow,perksTitle;readonly Label value,rank;readonly VisualElement ledger,squad,perks;readonly StatReadout waves,bosses,distance,time;
 public event Action Again,Menu;
 public ResultsView():base(Block){
  var body=Ui.Scroll(this,"vb-page__scroll",Block+"__scroll");eyebrow=new LabelComponent("",LabelSize.Label,TextTone.Enemy).Into(body);Ui.Text(body,"RUN OVER","vb-t-display");
  var card=Ui.Frame(body,FrameKind.Slim,"vb-frame--solid",Block+"__score");var top=Ui.Box(card,"vb-row","vb-row--between");var left=Ui.Box(top);new LabelComponent("Score").Into(left);value=Ui.Text(left,"0",Block+"__value");rank=Ui.Text(top,"",Block+"__rank");
  ledger=Ui.Box(card,Block+"__ledger");var stats=Ui.Box(card,"vb-row",Block+"__stats");waves=new StatReadout("WAVES","0").Into(stats);bosses=new StatReadout("BOSSES","0").Into(stats);distance=new StatReadout("DISTANCE","").Into(stats);time=new StatReadout("SIM TIME","").Into(stats);
  new LabelComponent("Squad · hover or tap for contribution").Classes("vb-page__label").Into(body);squad=Ui.Box(body,"vb-grid4");
  perksTitle=new LabelComponent("").Classes("vb-page__label").Into(body);perks=Ui.Box(body,"vb-row","vb-row--wrap",Block+"__perks");
  var row=Ui.Box(this,"vb-row",Block+"__row");new VbButton("MENU",ButtonVariant.Secondary,null,null,()=>Menu?.Invoke()){name="menu"}.Classes(Block+"__menu").Into(row);
  new VbButton("RUN AGAIN",ButtonVariant.Primary,null,"chevron",()=>Again?.Invoke()){name="again"}.Classes(Block+"__again").Into(row);
 }
 public ResultsView Bind(ResultsViewModel v){
  eyebrow.text=v.cause+" · wave "+v.wave;value.text=Ui.Number(v.score);rank.text=v.rank>0?"#"+v.rank+" OF "+v.of:"";rank.Show(v.rank>0);
  ledger.Clear();foreach(var l in v.ledger)new KeyValueRow(l.label,l.value).Into(ledger);
  waves.Set(v.wave.ToString());bosses.Set(v.bosses.ToString());distance.Show(!string.IsNullOrEmpty(v.distance));if(!string.IsNullOrEmpty(v.distance)){var parts=v.distance.Split(' ');distance.Set(parts[0],parts.Length>1?parts[1]:null);}
  time.Show(!string.IsNullOrEmpty(v.simTime));time.Set(v.simTime??"");
  squad.Clear();foreach(var c in v.squad)new SquadResult().Bind(c).Into(squad);
  perksTitle.text="Perks · "+v.perks.Length;perksTitle.Show(v.perks.Length>0);perks.Clear();foreach(var p in v.perks)new PerkChip().Bind(p).Into(perks);
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
}
