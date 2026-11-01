using System;
namespace Veilbreak.Gui {
public enum EffectPolarity { Neutral, Benefit, Harm }
public enum EffectScope { Actor, Squad, Run }
public enum EffectActivation { Active, Conditional, Ready, Spent, Expired }
/// <summary>Authored meaning, shared by HUD badges, lists and detail views. Unknown IDs remain inspectable.</summary>
[Serializable] public sealed class EffectDefinition {
 public string id="",title="",description="",icon="info";public EffectPolarity polarity;public EffectScope scope;
 public static EffectDefinition Unknown(string id)=>new EffectDefinition{id=id,title=string.IsNullOrEmpty(id)?"Unknown effect":id,description="No authored description is available for this effect."};
}
/// <summary>Presentation snapshot. Null times/source mean unavailable, never inferred attribution or duration.</summary>
[Serializable] public sealed class EffectInstance {
 public string id="",source="",magnitude="";public EffectDefinition definition=new EffectDefinition();public int stacks=1;
 public float? remainingSeconds,totalSeconds;public EffectActivation activation;public bool Active=>activation!=EffectActivation.Expired;
 public EffectInstance Expired()=>new EffectInstance{id=id,definition=definition,source=source,magnitude=magnitude,stacks=stacks,remainingSeconds=0,totalSeconds=totalSeconds,activation=EffectActivation.Expired};
}
}
