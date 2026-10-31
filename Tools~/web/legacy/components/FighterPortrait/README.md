# FighterPortrait

A fighter's tile: two-letter code, health and stability, and its live state. `rail` tiles fill the left rail in landscape; `card`s fill the bottom of the portrait HUD.

**Provide** `code` ("AR"), `name`; optional `health`, `stability` (0–100), `barrier`, `team` (`ally` default / `enemy`), `state` (`default`, `selected`, `exposed`, `defeated`), `auto` (automation is driving it), `queue` (its next queue number), `slot` (desktop hotkey 1–4), `variant` (`rail` default / `card`), `activity` on cards (`aiming`, `idle`, `auto`, `moving`, `casting`, or `"queued #2"`), `onClick`.

- Tap selects; the rail's bars mirror the fighter's world bars exactly.
- Selected: 2px teal frame, brackets, `glow-ally`. One selected at a time.
- Automated fighters show AUTO and a teal dot on the rail edge; giving them a command makes them manual for 3 s.
- Exposed inverts a band across the top; defeated dims and strikes through, never disappears.
- Enemy portraits (desktop header) use the diamond and crimson; they are never tappable for commands.
