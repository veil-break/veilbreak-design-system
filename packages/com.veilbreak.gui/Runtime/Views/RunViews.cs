using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
namespace Veilbreak.Gui {
/// <summary>Wave cleared: the perk choices as an endless <see cref="Carousel"/> of <see cref="ChoiceCard"/>s (the active card takes most of the width, its neighbours sit beneath it; swipe or tap one to change it), the squad that carries forward
/// (no healing between waves) and TAKE PERK, whose line says what the pick does next. The active card is the explanation (its effect, its levels and the value after the pick), so nothing under it repeats it.</summary>
[UxmlElement] public partial class PerkDraftView:ScreenView {
 public const string Block="vb-draft";
 readonly LabelComponent eyebrow;readonly Label caption,title;readonly VisualElement squad;readonly Carousel carousel;readonly VbButton take;PerkDraftViewModel view;
 public event Action<int> Picked;
 public event Action Take;
 /// <summary>The carry-forward frame was tapped: show the run's active perks.</summary>
 public event Action Perks;
 /// <summary>The carousel of perk cards.</summary>
 public Carousel Choices=>carousel;
 public PerkDraftView():base(Block){
  var body=Ui.Scroll(this,"vb-page__scroll",Block+"__scroll");eyebrow=new LabelComponent("",LabelSize.Label,TextTone.Ally).Classes(Glitch.TitleClass).Into(body);title=Ui.Text(body,"CHOOSE A PERK","vb-t-display",Glitch.TitleClass);caption=Ui.Text(body,"","vb-t-caption","vb-page__caption");
  carousel=new Carousel{name="choices"}.Classes(Block+"__carousel").Into(body);carousel.Changed+=i=>Picked?.Invoke(i);
  var carry=Ui.Frame(body,FrameKind.Slim,Block+"__squad").Clickable(()=>Perks?.Invoke());carry.name="carry";carry.tooltip="View active perks";new LabelComponent("Carries forward · no healing between waves",LabelSize.Badge).Into(carry);squad=Ui.Box(carry,"vb-grid4");
  take=new VbButton("TAKE PERK",ButtonVariant.Primary,null,"check",()=>Take?.Invoke()){name="take"}.Into(this);
 }
 protected override IEnumerable<VisualElement> DealItems(){yield return carousel;}
 public PerkDraftView Bind(PerkDraftViewModel v){
  view=v;eyebrow.text="Wave "+v.wave.ToString("00")+" cleared";caption.text="+"+Ui.Number(v.bonus)+" wave bonus · score "+Ui.Number(v.score)+" · perks last the whole run";
  carousel.Bind(v.choices,p=>p.id,()=>new ChoiceCard().Classes(Block+"__card"),(card,p,i)=>((ChoiceCard)card).Bind(p,i==v.pick),v.pick);
  bool has=v.pick>=0&&v.pick<v.choices.Length;
  if(has){var p=v.choices[v.pick];take.Sub=p.name+" → "+p.next+" · then wave "+(v.wave+1).ToString("00");}
  take.Blocked=has?"":"Pick a perk";
  Keyed.Bind(squad,v.squad,a=>a.code,_=>new SquadHealthCell(),(cell,a)=>cell.Bind(a));
  return this;
 }
}

/// <summary>Run over: the score and how it was made, the squad's contribution (hover or tap a cell), the perks, then MENU and RUN AGAIN.</summary>
[UxmlElement] public partial class ResultsView:ScreenView {
 public const string Block="vb-results";
 readonly LabelComponent eyebrow,perksTitle;readonly Label value,rank;readonly VisualElement ledger,squad,perks;readonly StatReadout waves,bosses,distance,time;
 public event Action Again,Menu,Squad;
 public ResultsView():base(Block){
  var body=Ui.Scroll(this,"vb-page__scroll",Block+"__scroll");eyebrow=new LabelComponent("",LabelSize.Label,TextTone.Enemy).Classes(Glitch.TitleClass).Into(body);Ui.Text(body,"RUN OVER","vb-t-display",Glitch.TitleClass);
  var card=Ui.Frame(body,FrameKind.Slim,"vb-frame--solid",Block+"__score");var top=Ui.Box(card,"vb-row","vb-row--between");var left=Ui.Box(top);new LabelComponent("Score").Into(left);value=Ui.Text(left,"0",Block+"__value");rank=Ui.Text(top,"",Block+"__rank");
  ledger=Ui.Box(card,Block+"__ledger");var stats=Ui.Box(card,"vb-row",Block+"__stats");waves=new StatReadout("WAVES","0").Into(stats);bosses=new StatReadout("BOSSES","0").Into(stats);distance=new StatReadout("DISTANCE","").Into(stats);time=new StatReadout("SIM TIME","").Into(stats);
  new LabelComponent("Squad").Classes("vb-page__label").Into(body);squad=Ui.Box(body,"vb-grid4");
  perksTitle=new LabelComponent("").Classes("vb-page__label").Into(body);perks=Ui.Box(body,"vb-row","vb-row--wrap",Block+"__perks");
  var row=Ui.Actions(this,new VbButton("MENU",ButtonVariant.Secondary,null,null,()=>Menu?.Invoke()){name="menu"}.Classes(Block+"__menu"),
   new VbButton("SQUAD",ButtonVariant.Secondary,null,null,()=>Squad?.Invoke()){name="squad"}.Classes(Block+"__menu"),
   new VbButton("RUN AGAIN",ButtonVariant.Primary,null,"chevron",()=>Again?.Invoke()){name="again"}.Classes(Block+"__again")).Classes(Block+"__row");row.MinItemWidth=104;
 }
 public ResultsView Bind(ResultsViewModel v){
  eyebrow.text=v.cause+" · wave "+v.wave;value.text=Ui.Number(v.score);bool ranked=v.rank>0&&v.score>0;rank.text=ranked?"#"+v.rank+" OF "+v.of:"";rank.Show(ranked);
  Keyed.Bind(ledger,v.ledger,l=>l.label,_=>new KeyValueRow(),(r,l)=>{r.Key=l.label;r.Value=l.value;});
  waves.Set(v.wave.ToString());bosses.Set(v.bosses.ToString());distance.Show(!string.IsNullOrEmpty(v.distance));if(!string.IsNullOrEmpty(v.distance)){var parts=v.distance.Split(' ');distance.Set(parts[0],parts.Length>1?parts[1]:null);}
  time.Show(!string.IsNullOrEmpty(v.simTime));time.Set(v.simTime??"");
  Keyed.Bind(squad,v.squad,c=>c.operative.id,_=>new ResultSummary(),(r,c)=>r.Bind(c),true);
  perksTitle.text="Perks · "+v.perks.Length;perksTitle.Show(v.perks.Length>0);Keyed.Bind(perks,v.perks,p=>p.id,_=>new ChipRow(),(r,p)=>r.Bind(p),true);
  return this;
 }
}

/// <summary>Top scores on this device with the current run framed, then runs kept from older rules, in one <see cref="ListLayout"/>.</summary>
[UxmlElement] public partial class HighScoresView:ScreenView {
 public const string Block="vb-scores";
 readonly ScreenHeader header;readonly Label caption;readonly ListLayout list;readonly VisualElement rankedRows,oldRows;readonly LabelComponent older;
 public event Action Back;
 public HighScoresView():base(Block,true){
  header=new ScreenHeader("HIGH SCORES","TOP 20").Into(this);header.Back+=()=>Back?.Invoke();caption=Ui.Text(this,"On this device. Ties break by wave, then shortest sim time.","vb-t-caption","vb-page__caption");
  list=Ui.List(this);rankedRows=Ui.Box(list);older=new LabelComponent("Older rules").Classes(Block+"__older").Into(list);oldRows=Ui.Box(list);
 }
 /// <param name="ranked">Ranked runs (the view shows at most 20).</param><param name="historical">Runs under older rules, listed after a label.</param>
 public HighScoresView Bind(IReadOnlyList<ScoreView> ranked,IReadOnlyList<ScoreView> historical=null,string caption=null){
  if(caption!=null)this.caption.text=caption;int n=Math.Min(20,ranked?.Count??0);header.Meta="TOP "+n;
  string Identity(ScoreView value)=>string.IsNullOrEmpty(value.id)?value.rank.ToString():value.id;
  Keyed.Bind(rankedRows,(ranked??Array.Empty<ScoreView>()).Take(n),Identity,_=>new ScoreRow(),(row,score)=>row.Bind(score));
  Keyed.Bind(oldRows,historical??Array.Empty<ScoreView>(),Identity,_=>new ScoreRow().Classes(Block+"__historical"),(row,score)=>row.Bind(score));older.Show(historical!=null&&historical.Count>0);list.Empty=n==0&&(historical==null||historical.Count==0)?"No runs yet. Your first run lands here.":"";
  return this;
 }
}

/// <summary>The run's perks: category totals, the stacks (tap one) and the picked one explained. Opens from the draft and from pause.</summary>
[UxmlElement] public partial class ActivePerksView:ScreenView {
 public const string Block="vb-build";
 readonly ScreenHeader header;readonly VisualElement totals;readonly ListLayout list;readonly PerkDetail detail;readonly List<PerkRow> rows=new List<PerkRow>();ActivePerksViewModel current;int shownPick=-1;
 public event Action<int> Picked;
 public event Action Back;
 public ActivePerksView():base(Block,true){
  header=new ScreenHeader("ACTIVE PERKS","").Into(this);header.Back+=()=>Back?.Invoke();totals=Ui.Frame(this,FrameKind.Slim,Block+"__summary");
  list=Ui.List(this,"No perks yet. Clear a wave to draft one.",Block+"__list");detail=new PerkDetail().Classes(Block+"__detail").Into(this);
 }
 /// <summary>A total that says nothing ("+0%", "0"): none of its digits is above zero. Such totals are not listed.</summary>
 public static bool Zero(string value){if(string.IsNullOrEmpty(value))return true;foreach(var c in value)if(c>='1'&&c<='9')return false;return true;}
 public ActivePerksView Bind(ActivePerksViewModel v){
  int stacks=0;foreach(var p in v.perks)stacks+=p.stack;header.Meta=stacks+" · WAVE "+v.wave.ToString("00");
  var shownTotals=v.totals.Where(t=>!Zero(t.value)).ToArray();Keyed.Bind(totals,shownTotals,t=>t.label,_=>new FactCell(Block+"__bonus","vb-t-badge",Block+"__bonus-value"),(cell,t)=>cell.Bind(t.label,t.value));totals.Show(shownTotals.Length>0&&v.perks.Length>0);
  current=v;Keyed.Bind(list.contentContainer,v.perks,p=>p.id,p=>{var row=new PerkRow();string id=p.id;row.Picked+=()=>Picked?.Invoke(Array.FindIndex(current.perks,x=>x.id==id));return row;},(row,p)=>row.Bind(p,Array.IndexOf(v.perks,p)==v.pick),true);
  bool has=v.pick>=0&&v.pick<v.perks.Length;VbMotion.Show(detail,has,MotionKind.Rise);if(has){detail.Bind(v.perks[v.pick],false);if(shownPick>=0&&shownPick!=v.pick)VbMotion.In(detail,MotionKind.Fade);}shownPick=has?v.pick:-1;
  return this;
 }
}

/// <summary>A fallen operative to restore (a revive perk): the fallen as tiles; tapping one is the choice.</summary>
[UxmlElement] public partial class RevivePickView:ScreenView {
 public const string Block="vb-revive";
 readonly ScreenHeader header;readonly Label caption;readonly VisualElement grid;OperativeView[] currentFallen=Array.Empty<OperativeView>();
 public event Action<int> Picked;
 public event Action Back;
 public RevivePickView():base(Block,true){header=new ScreenHeader("RESTORE","CHOOSE ONE").Into(this);header.Back+=()=>Back?.Invoke();caption=Ui.Text(this,"","vb-t-caption","vb-page__caption");grid=Ui.Box(this,"vb-grid4","vb-grid4--wrap");}
 /// <param name="fallen">The fallen operatives; <see cref="Picked"/> reports an index into it.</param>
 public RevivePickView Bind(string perk,OperativeView[] fallen){
  caption.text=perk+" revives one fallen operative at 35% health.";
  Keyed.Bind(grid,fallen.Select((v,i)=>(view:v,index:i)),x=>x.view.id,x=>{var tile=new UnitTile();tile.Picked+=()=>Picked?.Invoke(Array.FindIndex(currentFallen,a=>a.id==tile.ViewId));return tile;},(tile,x)=>{tile.ViewId=x.view.id;tile.Bind(x.view,0,false);},true);currentFallen=fallen;
  return this;
 }
}
}
