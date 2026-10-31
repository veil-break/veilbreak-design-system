# MatchHeader

The read-only top band: the match clock with its phase, and the opposing fighters' names and health.

**Provide** `clock` (string, "0:58"), `fighters` (`[{ name, health, team?, defeated? }]`, enemies by default); optional `phase` (`'pressure'`), `sub` (a phase line such as "Pressure in 0:28"), `compact` (portrait layout).

- Landscape splits the enemies left and right of the clock; portrait stacks clock then strip.
- From 4:00 set `phase="pressure"`: a crimson PRESSURE tag replaces the sub line.
- Defeated fighters dim to `alpha-defeated` with a struck-through name; they never disappear, so the count stays legible.
- Never tappable. Desktop may add both teams' portraits beside it with stability and statuses.
