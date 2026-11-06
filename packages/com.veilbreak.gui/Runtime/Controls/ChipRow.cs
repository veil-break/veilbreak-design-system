using UnityEngine.UIElements;
namespace Veilbreak.Gui {
/// <summary>A chip: a glyph, a word and a value on one plate with no edge. A perk chip is tinted by its rarity (<see cref="PerkTier"/>), never framed, and where a chip is a target
/// (the results page's perks, whose hover, focus or tap shows <see cref="PerkDetail"/>) its hit area is 44 units tall: the plate is 30 and the element's padding makes up the rest. A chip that only
/// reads (the HUD's credits) is <see cref="Static"/> and is as tall as its plate. Chips wrap in a row (<c>vb-row vb-row--wrap</c>) with no gap of their own.</summary>
[UxmlElement] public partial class ChipRow:VisualElement {
 public const string ClassName="vb-chip-row",PlateClass=ClassName+"__plate";
 /// <summary>The hit area of a chip that is a target.</summary>
 public const float TargetHeight=44;
 readonly VisualElement plate;readonly VbIcon glyph;readonly Label word,number;PerkView view;int shown=-1;
 [UxmlAttribute] public string Text {get=>word.text;set{word.text=(value??"").ToUpperInvariant();word.Show(word.text.Length>0);}}
 [UxmlAttribute] public string Icon {get=>glyph.Icon;set{glyph.Icon=value;glyph.Show(!string.IsNullOrEmpty(value));}}
 [UxmlAttribute] public string Value {get=>number.text;set{number.text=value??"";number.Show(number.text.Length>0);}}
 /// <summary>True for a chip that only reads: it is as tall as its plate and takes no input.</summary>
 [UxmlAttribute] public bool Static {get=>ClassListContains(ClassName+"--static");set{EnableInClassList(ClassName+"--static",value);pickingMode=value?PickingMode.Ignore:PickingMode.Position;focusable=!value;}}
 /// <summary>The plate that paints the chip (inside the hit area).</summary>
 public VisualElement Plate=>plate;
 /// <summary>The perk's detail popover; null for a chip that is not bound to a perk.</summary>
 public Popover Details {get;private set;}
 public ChipRow():this(false){}
 public ChipRow(bool alignEnd){
  AddToClassList(ClassName);pickingMode=PickingMode.Position;focusable=true;
  plate=VbShape.Attach(Ui.Box(this,PlateClass));glyph=new VbIcon("",14).Into(plate);word=Ui.Text(plate,"",ClassName+"__name");number=Ui.Text(plate,"",ClassName+"__count");
  Icon=null;Text=null;Value=null;
  Details=Popover.Attach(this,()=>view==null?null:new PerkDetail().Bind(view,false).Classes(ClassName+"__popover"),PopoverPlacement.Below,alignEnd);
 }
 /// <summary>A chip for the run's credits: the currency glyph and the amount, read only.</summary>
 public static ChipRow Credits(string tooltip="Credits · spend at a merchant"){var chip=new ChipRow{Static=true,Icon="currency"}.Classes("vb-credits");chip.tooltip=tooltip;return chip;}
 /// <summary>Shows <paramref name="n"/> as the value, counting up when it changes on a shown chip; the same number again does nothing.</summary>
 public ChipRow SetNumber(int n){if(shown!=n){bool change=shown>=0;shown=n;VbMotion.Count(number,n,Ui.Number);number.Show(true);if(change)VbMotion.Pop(this);}return this;}
 /// <summary>Binds a perk: its glyph, name, stacks (×n when more than one) and rarity tint.</summary>
 public ChipRow Bind(PerkView p){view=p;PerkArt.Tiers(this,p);Icon=p.icon;Text=p.name;Value=p.stack>1?"×"+p.stack:"";tooltip=p.name+" · "+p.now;return this;}
}
}
