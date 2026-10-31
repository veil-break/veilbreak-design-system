using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
namespace Veilbreak.Gui {
/// <summary>Pick four: the squad slots on top, the roster grid below and the tapped operative's details; START RUN is blocked with
/// its reason until the squad is full. Selection is the game's: the view raises events and is bound again.</summary>
[UxmlElement] public partial class SquadView:ScreenView {
 public const string Block="vb-squad-view";
 readonly ScreenHeader header;readonly SquadSlot[] slots=new SquadSlot[4];readonly VisualElement roster,sheet;readonly LabelComponent rosterLabel;readonly OperativeDetails details;readonly VbButton start;
 readonly List<OperativeTile> tiles=new List<OperativeTile>();SquadViewModel view=new SquadViewModel();
 /// <summary>A roster tile was tapped (roster index): open its details.</summary>
 public event Action<int> Picked;
 /// <summary>A squad slot was tapped (0–3).</summary>
 public event Action<int> SlotPicked;
 /// <summary>ADD TO / REMOVE FROM SQUAD on the open details (roster index).</summary>
 public event Action<int> Toggled;
 public event Action Closed,Start,Back;
 public SquadView():base(Block,true){
  header=new ScreenHeader("SQUAD","0 / 4").Into(this);header.Back+=()=>Back?.Invoke();
  new LabelComponent("Squad · tap a slot for details").Classes("vb-page__label").Into(this);
  var grid=Ui.Box(this,"vb-grid4");for(int i=0;i<4;i++){int k=i;slots[i]=new SquadSlot().Into(grid);slots[i].Picked+=()=>SlotPicked?.Invoke(k);}
  rosterLabel=new LabelComponent("Roster").Classes("vb-page__label").Into(this);roster=Ui.Box(this,"vb-grid4","vb-grid4--wrap");
  sheet=Ui.Scroll(this,Block+"__sheet");details=new OperativeDetails().Into(sheet);details.Closed+=()=>Closed?.Invoke();details.Toggled+=()=>{if(view.focus>=0)Toggled?.Invoke(view.focus);};
  Ui.Spacer(this);start=new VbButton("START RUN",ButtonVariant.Primary,null,"chevron",()=>Start?.Invoke()){name="start"}.Classes(Block+"__start").Into(this);
  Bind(view);
 }
 public SquadView Bind(SquadViewModel v){
  view=v;var picked=v.selected.Where(i=>i>=0&&i<v.roster.Length).ToArray();int count=picked.Length;
  header.Meta=count+" / 4";header.Accent(count==4);
  for(int i=0;i<4;i++){int r=i<v.selected.Length?v.selected[i]:-1;slots[i].Bind(r>=0&&r<v.roster.Length?v.roster[r]:null,i+1);}
  int unlocked=0;for(int i=0;i<v.roster.Length;i++)if(i>=v.locked.Length||string.IsNullOrEmpty(v.locked[i]))unlocked++;
  rosterLabel.text=unlocked==v.roster.Length?"Roster · all "+v.roster.Length+" unlocked":"Roster · "+unlocked+" of "+v.roster.Length+" unlocked";
  while(tiles.Count<v.roster.Length){int k=tiles.Count;var t=new OperativeTile().Into(roster);t.Picked+=()=>Picked?.Invoke(k);tiles.Add(t);}
  for(int i=0;i<tiles.Count;i++){bool on=i<v.roster.Length;tiles[i].Show(on);if(!on)continue;int slot=Array.IndexOf(v.selected,i)+1;
   tiles[i].Bind(v.roster[i],slot,i==v.focus,i<v.tiers.Length?v.tiers[i]:RarityTier.None,i<v.locked.Length?v.locked[i]:null);}
  bool open=v.focus>=0&&v.focus<v.roster.Length;sheet.Show(open);if(open)details.Bind(v.roster[v.focus],Array.IndexOf(v.selected,v.focus)+1,count>=4);
  start.Sub=count>0?string.Join(" · ",picked.Select(i=>v.roster[i].name))+(string.IsNullOrEmpty(v.seed)?"":" · "+v.seed):"";start.Blocked=count<4?"Pick "+(4-count)+" more":"";
  return this;
 }
}
}
