using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
namespace Veilbreak.Gui {
/// <summary>Pick four: the squad slots on top (drag, long-press or Alt+arrow a filled slot to reorder the squad), the roster grid below and the tapped
/// operative's details in a bottom drawer; START RUN is blocked with its reason until the squad is full. Selection is the game's: the view raises events and is bound again.</summary>
[UxmlElement] public partial class SquadView:ScreenView {
 public const string Block="vb-squad-view";
 readonly ScreenHeader header;readonly UnitSlot[] slots=new UnitSlot[4];readonly bool[] filled=new bool[4];readonly VisualElement roster;readonly DrawerOverlay drawer;readonly LabelComponent rosterLabel;readonly UnitDetail details;readonly VbButton start;
 readonly List<UnitTile> tiles=new List<UnitTile>();SquadViewModel view=new SquadViewModel();int shownFocus=-1;
 /// <summary>A roster tile was tapped (roster index): open its details.</summary>
 public event Action<int> Picked;
 /// <summary>A squad slot was tapped (0–3).</summary>
 public event Action<int> SlotPicked;
 /// <summary>A filled slot was dragged (or moved with Alt+arrow) onto another slot: (from, to), both 0–3. A drop on an empty slot reports the last filled one. The
 /// game reorders its squad and binds again; the view changes nothing itself.</summary>
 public event Action<int,int> SlotMoved;
 /// <summary>ADD TO / REMOVE FROM SQUAD on the open details (roster index).</summary>
 public event Action<int> Toggled;
 /// <summary>The details drawer was dismissed (backdrop, drag, ×): clear the focus and bind again.</summary>
 public event Action Closed;
 public event Action Start,Back;
 /// <summary>The manipulator that reorders the squad slots (for hosts that need its state, and for tests).</summary>
 public ReorderManipulator Reorder {get;}
 /// <summary>The drawer holding <see cref="UnitDetail"/>.</summary>
 public DrawerOverlay DetailsDrawer=>drawer;
 public SquadView():base(Block,true){
  header=new ScreenHeader("SQUAD","0 / 4").Into(this);header.Back+=()=>Back?.Invoke();
  var grid=Ui.Box(this,"vb-grid4",Block+"__slots");for(int i=0;i<4;i++){int k=i;slots[i]=new UnitSlot().Into(grid);slots[i].Picked+=()=>SlotPicked?.Invoke(k);}
  var reorder=Reorder=new ReorderManipulator{CanLift=i=>i>=0&&i<4&&filled[i]};grid.AddManipulator(reorder);reorder.Reordered+=(a,b)=>{int last=-1;for(int i=0;i<4;i++)if(filled[i])last=i;b=Math.Min(b,last);if(b!=a&&b>=0)SlotMoved?.Invoke(a,b);};
  rosterLabel=new LabelComponent("Roster").Classes("vb-page__label").Into(this);roster=Ui.Box(this,"vb-grid4","vb-grid4--wrap",Block+"__roster");
  start=new VbButton("START RUN",ButtonVariant.Primary,null,"chevron",()=>Start?.Invoke()){name="start"}.Classes(Block+"__start").Into(this);
  // Details open in a bottom drawer over the roster: tap the backdrop, drag the handle down or tap × to close.
  drawer=new DrawerOverlay{CloseButton=false,name="operative-details"}.Classes(Block+"__drawer").Into(this);drawer.Dismissed+=()=>Closed?.Invoke();
  details=new UnitDetail().Into(drawer);details.Closed+=()=>Closed?.Invoke();details.Toggled+=()=>{if(view.focus>=0)Toggled?.Invoke(view.focus);};
  Bind(view);
 }
 /// <summary>Stability is worth a read-out only once operatives differ in it.</summary>
 public static bool StabilityVaries(OperativeView[] roster){for(int i=1;i<roster.Length;i++)if(roster[i].maxStability!=roster[0].maxStability)return true;return false;}
 public SquadView Bind(SquadViewModel v){
  view=v;var picked=v.selected.Where(i=>i>=0&&i<v.roster.Length).ToArray();int count=picked.Length;
  header.Meta=count+" / 4";header.Accent(count==4);
  for(int i=0;i<4;i++){int r=i<v.selected.Length?v.selected[i]:-1;filled[i]=r>=0&&r<v.roster.Length;slots[i].Bind(filled[i]?v.roster[r]:null,i+1);}
  int unlocked=0;for(int i=0;i<v.roster.Length;i++)if(i>=v.locked.Length||string.IsNullOrEmpty(v.locked[i]))unlocked++;
  // The roster is its own heading: a caption says something only while part of it is locked.
  rosterLabel.text="Roster · "+unlocked+" of "+v.roster.Length+" unlocked";rosterLabel.Show(unlocked<v.roster.Length);
  while(tiles.Count<v.roster.Length){int k=tiles.Count;var t=new UnitTile().Into(roster);t.Picked+=()=>Picked?.Invoke(k);tiles.Add(t);}
  for(int i=0;i<tiles.Count;i++){bool on=i<v.roster.Length;tiles[i].Show(on);if(!on)continue;int slot=Array.IndexOf(v.selected,i)+1;
   tiles[i].Bind(v.roster[i],slot,i==v.focus,i<v.tiers.Length?v.tiers[i]:RarityTier.None,i<v.locked.Length?v.locked[i]:null);}
  bool open=v.focus>=0&&v.focus<v.roster.Length;if(open){details.Bind(v.roster[v.focus],Array.IndexOf(v.selected,v.focus)+1,count>=4,v.maxHp,v.maxArmor,v.maxStability,StabilityVaries(v.roster));if(!drawer.IsOpen)drawer.Open();else if(shownFocus>=0&&shownFocus!=v.focus)VbMotion.In(details,MotionKind.Fade);}else if(drawer.IsOpen)drawer.Close();shownFocus=open?v.focus:-1;
  start.Sub=count>0?string.Join(" · ",picked.Select(i=>v.roster[i].name))+(string.IsNullOrEmpty(v.seed)?"":" · "+v.seed):"";start.Blocked=count<4?"Pick "+(4-count)+" more":"";
  return this;
 }
}
}
