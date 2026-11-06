using System;
namespace Veilbreak.Gui {
// Forwarding types for the names that 0.8.0 replaced with generic ones. Each is the new type under its old name, so a game that still says OperativeCard compiles and behaves the same;
// they are removed in the release after the games adopt the new names (arcade: design-system#39 G2). Nothing in the kit uses them.
#pragma warning disable CS1591
[Obsolete("Use UnitCard.")] public class OperativeCard:UnitCard {}
[Obsolete("Use UnitTile.")] public class OperativeTile:UnitTile {}
[Obsolete("Use UnitSlot.")] public class SquadSlot:UnitSlot {}
[Obsolete("Use UnitDetail.")] public class OperativeDetails:UnitDetail {}
[Obsolete("Use UnitStats.")] public class OperativeStats:UnitStats {}
[Obsolete("Use ResultSummary.")] public class SquadResult:ResultSummary {}
[Obsolete("Use ChoiceCard.")] public class PerkCard:ChoiceCard {}
[Obsolete("Use ChipRow.")] public class PerkChip:ChipRow {public PerkChip():base(false){} public PerkChip(bool alignEnd):base(alignEnd){}}
[Obsolete("Use ChipRow (ChipRow.Credits() and SetNumber).")] public class CreditsChip:ChipRow {
 public CreditsChip(){Static=true;Icon="currency";AddToClassList("vb-credits");tooltip="Credits · spend at a merchant";}
 public CreditsChip Set(int credits){SetNumber(credits);return this;}
}
#pragma warning restore CS1591
}
