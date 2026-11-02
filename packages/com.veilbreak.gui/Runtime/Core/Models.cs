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
 public EffectInstance[] effects=Array.Empty<EffectInstance>(); public ControlMode mode; public float manualSeconds;
 public SkillView[] skills={new SkillView(),new SkillView{slot="S",kind="Signature"}};
 public bool Fallen=>mode==ControlMode.Fallen||hp<=0;
}

[Serializable] public sealed class PerkView {
 public string id="",name="",category="",tag="General",icon="add",effect="",now="",next="";
 public PerkTier tier; public Texture2D art; public int stack,max; public long[] waves=Array.Empty<long>();
}

[Serializable] public sealed class ScoreView { public string id=""; public int rank; public long score,wave,bosses; public string[] squad=Array.Empty<string>(); public bool current; }
/// <summary>One upcoming cast. Manual entries name their operative and slot so the HUD can cancel them; <see cref="next"/> marks a buffered cast that fires as soon as its operative is ready.</summary>
[Serializable] public sealed class QueueView { public string id=""; public string who="",skill="",eta=""; public int cost,actor=-1,slot; public bool manual,next; }

[Serializable] public sealed class ContributionView {
 public OperativeView operative=new OperativeView(); public long fellWave,damage,kills,casts,power; public float share; public string extraLabel="Basics",extraValue="0";
}

[Serializable] public sealed class HudView {
 public int speed=1,credits; public long wave=1,score; public bool boss,auto=true,slow; public int power,maxPower=100; public string regen="+12/s",hint;
 public float focus=100; public int previewFrom=-1,previewTo=-1; public QueueView[] queue=Array.Empty<QueueView>(); public string queueTitle="AUTOPLAY QUEUE",queueHint="Fires as Power reaches each cost";
}

[Serializable] public sealed class RoadView { public string kind="combat",title="Combat",icon="battle",hint="Common cache · occasional rare"; }
[Serializable] public sealed class RouteView { public long wave; public RoadView left=new RoadView(),right=new RoadView(); }
[Serializable] public sealed class MerchantOfferView { public string id=""; public PerkView perk=new PerkView(); public int price; public bool taken; public string blocked=""; }
[Serializable] public sealed class MerchantStackView { public PerkView perk=new PerkView(); public int payout; }
[Serializable] public sealed class MerchantHealthView { public int actor,hp,maxHp,cost,payout; public string name="",blocked=""; }
[Serializable] public sealed class MerchantView { public int credits; public MerchantOfferView[] offers=Array.Empty<MerchantOfferView>(); public MerchantStackView[] stacks=Array.Empty<MerchantStackView>(); public MerchantHealthView[] health=Array.Empty<MerchantHealthView>(); }

/// <summary>One IF … THEN automation rule. Condition values are highlighted; the first matching rule runs.</summary>
[Serializable] public sealed class RuleView {
 public int index=1; public RuleCondition[] conditions=Array.Empty<RuleCondition>(); public bool any; public string action="",actionRest=""; public int cost;
 public bool enabled=true,warning,draft; public string note="";
}
[Serializable] public sealed class RuleCondition { public string text="",value="",after=""; }

[Serializable] public sealed class BossView { public EffectInstance[] effects=Array.Empty<EffectInstance>(); public string name=""; public float health=1,stability=1; public string detail=""; public bool shielded,interruptible; public string shieldDetail=""; }

// ── view models for the screen views (Runtime/Views) ────────────────────────
/// <summary>A run that can resume: the wave it resumes at, its score, who stands and what it carries.</summary>
[Serializable] public sealed class CheckpointView { public long wave=1,score; public int standing=4,squadSize=4,perks; public string seed=""; }
[Serializable] public sealed class MainMenuViewModel {
 public string status="OFFLINE · LOCAL SCORES",subtitle="ARCADE",notice=""; public long best,bestWave;
 /// <summary>Null when there is no run to resume; the checkpoint card only exists when there is one.</summary>
 public CheckpointView checkpoint; public string[] squad=Array.Empty<string>(); public bool canExit=true;
}
[Serializable] public sealed class SquadViewModel {
 public OperativeView[] roster=Array.Empty<OperativeView>();
 /// <summary>Roster indices in squad order (1–4); -1 is an empty slot.</summary>
 public int[] selected={-1,-1,-1,-1};
 /// <summary>The roster index whose details are open, or -1.</summary>
 public int focus=-1;
 /// <summary>Per roster index: how to unlock a locked operative ("Pull in rifts"), or empty.</summary>
 public string[] locked=Array.Empty<string>(); public RarityTier[] tiers=Array.Empty<RarityTier>(); public string seed="new seed";
 /// <summary>Roster maxima the details' attribute bars scale to.</summary>
 public float maxHp=1580,maxArmor=26,maxStability=140;
}
[Serializable] public sealed class SettingsViewModel {
 public float master=.8f,music=.55f,effects=.8f,sensitivity=.5f; public bool haptics=true,reducedMotion; public int camera,quality=1;
 public string saved="SAVED",note="Autoplay lives in the battle HUD and starts on for every new run.";
}
[Serializable] public sealed class PauseViewModel { public long wave,score; public int standing,squadSize=4; public PerkView[] perks=Array.Empty<PerkView>(); public string seed="",detail=""; }
[Serializable] public sealed class SquadHealthView { public string code=""; public int hp,maxHp=1; public float stability=1; }
[Serializable] public sealed class PerkDraftViewModel { public long wave,bonus,score; public PerkView[] choices=Array.Empty<PerkView>(); public int pick; public SquadHealthView[] squad=Array.Empty<SquadHealthView>(); }
/// <summary>The run's cumulative perks: the picked one explained and the category totals ("DAMAGE", "+20%").</summary>
[Serializable] public sealed class ActivePerksViewModel { public long wave; public PerkView[] perks=Array.Empty<PerkView>(); public int pick; public LedgerLine[] totals=Array.Empty<LedgerLine>(); }
[Serializable] public sealed class AccessibilityViewModel { public bool reducedFlashes,largerText,highContrast; }
[Serializable] public sealed class LedgerLine { public string label="",value=""; }
[Serializable] public sealed class ResultsViewModel {
 public string cause="SQUAD DOWN",distance="",simTime=""; public long score,wave,bosses; public int rank,of=20;
 public LedgerLine[] ledger=Array.Empty<LedgerLine>(); public ContributionView[] squad=Array.Empty<ContributionView>(); public PerkView[] perks=Array.Empty<PerkView>();
}
}
