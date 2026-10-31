using System;
using UnityEngine;
namespace Veilbreak.Gui {
// Plain view models. Games map their own state onto these; the kit never reads game types.
public enum SkillState { Ready, Aiming, Cooldown, Short, Silenced, Fallen, Casting, Disabled, Queued, Buffered, Windup, Invalid, Rejected, Exposed }
public enum ControlMode { Auto, Manual, Aiming, Fallen }
public enum PerkTier { General, Operative, Rare, Epic }
public enum ButtonVariant { Primary, Secondary, Light, Danger, Ghost }
public enum StatusKind { Exposed, Silenced, Rooted, Slowed, Interrupted, Barrier }
public enum MeterSize { Hud, World, Large }
public enum RarityTier { None, Blue, Violet, Gold }

[Serializable] public sealed class SkillView {
 public string label="Skill",icon="projectile",slot="A",kind="Active",description="",fullDescription="";
 public int cost; public SkillState state; public float charged=1,seconds,cooldownSeconds; public int need;
 /// <summary>1-based position in the manual queue when <see cref="state"/> is <see cref="SkillState.Queued"/>.</summary>
 public int queue;
 /// <summary>0–1 through a windup (<see cref="SkillState.Windup"/>), from the simulation.</summary>
 public float progress;
 /// <summary>Why an aim is invalid (<see cref="SkillState.Invalid"/>): "Out of range", "No line of sight".</summary>
 public string reason="";
}

[Serializable] public sealed class OperativeView {
 public string id="",name="",code="",role="",roleIcon="striker",passive="",passiveText="",passiveFullText="";
 public Color kit=Color.white; public Texture2D portrait;
 public int hp,maxHp=1,barrier,armor,stability=100,maxStability=100;
 public StatusKind[] statuses=Array.Empty<StatusKind>(); public ControlMode mode; public float manualSeconds;
 public SkillView[] skills={new SkillView(),new SkillView{slot="S",kind="Signature"}};
 public bool Fallen=>mode==ControlMode.Fallen||hp<=0;
}

[Serializable] public sealed class PerkView {
 public string id="",name="",category="",tag="General",icon="add",effect="",now="",next="";
 public PerkTier tier; public Texture2D art; public int stack,max; public long[] waves=Array.Empty<long>();
}

[Serializable] public sealed class ScoreView { public int rank; public long score,wave,bosses; public string[] squad=Array.Empty<string>(); public bool current; }
/// <summary>One upcoming cast. Manual entries name their operative and slot so the HUD can cancel them; <see cref="next"/> marks a buffered cast that fires as soon as its operative is ready.</summary>
[Serializable] public sealed class QueueView { public string who="",skill="",eta=""; public int cost,actor=-1,slot; public bool manual,next; }

[Serializable] public sealed class ContributionView {
 public OperativeView operative=new OperativeView(); public long fellWave,damage,kills,casts,power; public float share; public string extraLabel="Basics",extraValue="0";
}

[Serializable] public sealed class HudView {
 public int speed=1,credits; public long wave=1,score; public bool boss,auto=true,slow; public int power,maxPower=100; public string regen="+12/s",hint;
 public float focus=100; public int previewFrom=-1,previewTo=-1; public QueueView[] queue=Array.Empty<QueueView>(); public string queueTitle="AUTOPLAY QUEUE",queueHint="Fires as Power reaches each cost";
}

[Serializable] public sealed class RoadView { public string kind="combat",title="Combat",icon="battle",hint="Common cache · occasional rare"; }
[Serializable] public sealed class RouteView { public long wave; public RoadView left=new RoadView(),right=new RoadView(); }
[Serializable] public sealed class MerchantOfferView { public PerkView perk=new PerkView(); public int price; public bool taken; public string blocked=""; }
[Serializable] public sealed class MerchantStackView { public PerkView perk=new PerkView(); public int payout; }
[Serializable] public sealed class MerchantHealthView { public int actor,hp,maxHp,cost,payout; public string name="",blocked=""; }
[Serializable] public sealed class MerchantView { public int credits; public MerchantOfferView[] offers=Array.Empty<MerchantOfferView>(); public MerchantStackView[] stacks=Array.Empty<MerchantStackView>(); public MerchantHealthView[] health=Array.Empty<MerchantHealthView>(); }

/// <summary>One IF … THEN automation rule. Condition values are highlighted; the first matching rule runs.</summary>
[Serializable] public sealed class RuleView {
 public int index=1; public RuleCondition[] conditions=Array.Empty<RuleCondition>(); public bool any; public string action="",actionRest=""; public int cost;
 public bool enabled=true,warning,draft; public string note="";
}
[Serializable] public sealed class RuleCondition { public string text="",value="",after=""; }

[Serializable] public sealed class BossView { public string name=""; public float health=1,stability=1; public string detail=""; public bool shielded,interruptible; public string shieldDetail=""; }
}
