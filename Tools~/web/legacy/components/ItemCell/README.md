# ItemCell

A gear slot or inventory cell: slot glyph and rarity letter in the tier colour, upgrade level, and selection.

**Provide** `slot` (`optic`, `core`, `plating`, `weapon`, `charm`); optional `rarity` (`normal` N grey, `magic` M blue, `rare` R violet, `set` S lime, `unique` U gold), `upgrade` (0–10), `state` (`default`, `selected`, `empty`, `locked`), `showSlot` (caption under the cell), `name`, `onClick`.

- `touch-min` square on every platform; inventory grids use `space-2` gaps.
- Rarity always shows its letter — colour is never the only signal.
- Selecting an item opens the detail with a swap comparison ("you lose 40 health") and EQUIP / SALVAGE.
