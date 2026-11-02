using System;
using UnityEngine.UIElements;
namespace Veilbreak.Gui {
/// <summary>One entry of the Power queue: place, operative, skill, cost and time to fire, with a full-height × on manual entries.
/// The first entry (or a buffered one) gets the thicker <c>--next</c> frame; a preview chip is dashed (what aiming would add).</summary>
[UxmlElement] public partial class QueueChip:VisualElement {
 public const string ClassName="vb-queue-chip";
 readonly Label index,skill,who,cost,eta;readonly VisualElement close;
 public QueueView View {get;private set;}
 public event Action Cancelled;
 public QueueChip(){AddToClassList(ClassName);VbShape.Attach(this);pickingMode=PickingMode.Ignore;
  index=Ui.Text(this,"1",ClassName+"__index");var text=Ui.Box(this,ClassName+"__text");skill=Ui.Text(text,"",ClassName+"__skill");who=Ui.Text(text,"",ClassName+"__who");
  cost=Ui.Text(this,"",ClassName+"__cost");eta=Ui.Text(this,"",ClassName+"__eta");close=Ui.Box(this,ClassName+"__cancel").Clickable(()=>Cancel());close.tooltip="Cancel queued cast";new VbIcon("close",14).Into(close);
  Bind(new QueueView(),1);}
 public QueueChip Bind(QueueView v,int position,bool preview=false){View=v;index.text=v.next?"›":position.ToString();skill.text=(v.skill??"").ToUpperInvariant();who.text=v.who??"";cost.text=v.cost.ToString();eta.text=v.eta??"";eta.Show(eta.text.Length>0);
  EnableInClassList(ClassName+"--next",position==1||v.next);EnableInClassList(ClassName+"--preview",preview);EnableInClassList(ClassName+"--manual",v.manual);close.Show(v.manual&&!preview);return this;}
 /// <summary>What the × does; false for autoplay estimates and previews, which cannot be cancelled.</summary>
 public bool Cancel(){if(View==null||!View.manual||ClassListContains(ClassName+"--preview"))return false;Cancelled?.Invoke();return true;}
}
}
