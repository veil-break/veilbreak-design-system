using UnityEngine.UIElements;
namespace Veilbreak.Gui {
/// <summary>Reusable labelled fact cell for contribution grids and category totals.</summary>
public sealed class FactCell:ViewBase {
 readonly Label key,value;public FactCell(string block,string keyClass=null,string valueClass=null){AddToClassList(block);key=Ui.Text(this,"",keyClass??block+"__key","vb-t-badge");value=Ui.Text(this,"",valueClass??block+"__value");}
 public FactCell Bind(string key,string value){this.key.text=key;this.value.text=value;return this;}
}
public sealed class SquadHealthCell:ViewBase {
 readonly Label code,value;readonly HealthBar bar;
 public SquadHealthCell(){AddToClassList("vb-draft__cell");var row=Ui.Box(this,"vb-row","vb-row--between");code=Ui.Text(row,"","vb-draft__code");value=Ui.Text(row,"","vb-t-badge");bar=new HealthBar().Into(this);}
 public void Bind(SquadHealthView v){code.text=v.code;value.text=v.hp>0?UnityEngine.Mathf.RoundToInt(100f*v.hp/UnityEngine.Mathf.Max(1,v.maxHp))+"%":"OUT";bar.Set(v.hp,v.maxHp,0,v.stability);}
}
/// <summary>Shared skill explanation row; data changes preserve the instance and any enclosing scroll.</summary>
public sealed class SkillSummary:ViewBase {
 readonly VbIcon icon;readonly Label title,slot,description,power,cooldown;
 public SkillSummary(){const string b="vb-details";AddToClassList(b+"__skill");var art=Ui.Frame(this,FrameKind.Slim,b+"__skill-icon");icon=new VbIcon("info",14).Into(art);var text=Ui.Box(this,b+"__skill-text");var line=Ui.Box(text,b+"__skill-line");title=Ui.Text(line,"",b+"__skill-name");slot=Ui.Text(line,"",b+"__skill-slot");description=Ui.Text(text,"",b+"__skill-desc");var cost=Ui.Box(this,b+"__skill-cost");power=Ui.Text(cost,"",b+"__skill-power");cooldown=Ui.Text(cost,"",b+"__skill-cd");}
 public void Bind(SkillView v){tooltip=v.fullDescription;icon.Icon=v.icon;title.text=v.label;slot.text=v.slot+" · "+v.kind.ToUpperInvariant();description.text=v.description;power.text=v.cost.ToString();cooldown.text=Ui.Seconds(v.cooldownSeconds)+" CD";}
}
}
