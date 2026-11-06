using UnityEngine.UIElements;
namespace Veilbreak.Gui {
/// <summary>One fact on one row: a key that keeps its width, a value that wraps beside it and, optionally, a glyph before them and a trailing action whose button is 44 units tall
/// (the merchant's BUY and SELL, a perk's detail rows). It is a <see cref="KeyValueRow"/>, so everything that reads one reads this. <see cref="Stacked"/> puts the value under the key (a
/// long effect text under its name); <see cref="Dimmed"/> greys a row that cannot be acted on, with no second price or reason. The row is plain; <c>vb-stat-row--plated</c> gives it
/// one tone above its parent for a list.</summary>
[UxmlElement] public partial class StatRow:KeyValueRow {
 public new const string ClassName="vb-stat-row";
 /// <summary>The smallest a row with an action is, so the action is a target a thumb hits.</summary>
 public const float TargetHeight=44;
 readonly VbIcon glyph;readonly VisualElement text;readonly ActionRow trail;ProgressBar bar;
 [UxmlAttribute] public string Icon {get=>glyph.Icon;set{glyph.Icon=value;glyph.Show(!string.IsNullOrEmpty(value));}}
 /// <summary>The value under the key instead of beside it.</summary>
 [UxmlAttribute] public bool Stacked {get=>ClassListContains(ClassName+"--stacked");set=>EnableInClassList(ClassName+"--stacked",value);}
 /// <summary>A row that cannot be acted on: dimmed, nothing else changes.</summary>
 [UxmlAttribute] public bool Dimmed {get=>ClassListContains(ClassName+"--dimmed");set=>EnableInClassList(ClassName+"--dimmed",value);}
 /// <summary>A tone above the parent, for rows in a list.</summary>
 [UxmlAttribute] public bool Plated {get=>ClassListContains(ClassName+"--plated");set=>EnableInClassList(ClassName+"--plated",value);}
 /// <summary>The key and value column; composites add their own lines here.</summary>
 public VisualElement Content=>text;
 /// <summary>The trailing actions, one fixed width so they line up down a list; hidden while empty.</summary>
 public ActionRow Trail=>trail;
 /// <summary>A progress bar under the value, created on first use.</summary>
 public ProgressBar Bar=>bar??=new ProgressBar().Classes(ClassName+"__bar").Into(text);
 public StatRow():this("",""){}
 public StatRow(string key,string value,bool good=false):base(key,value,good){
  AddToClassList(ClassName);VbShape.Attach(this);
  glyph=new VbIcon("",20).Classes(ClassName+"__icon");glyph.Show(false);Insert(0,glyph);
  text=new VisualElement{pickingMode=PickingMode.Ignore};text.AddToClassList(ClassName+"__text");Insert(1,text);text.Add(KeyLabel);text.Add(ValueLabel);
  trail=new ActionRow{Fixed=true}.Classes(ClassName+"__trail").Into(this);trail.Show(false);
 }
 /// <summary>Adds an action (a button) to the trailing slot.</summary>
 public StatRow WithAction(VisualElement action){trail.Add(action);trail.Show(true);return this;}
}
}
