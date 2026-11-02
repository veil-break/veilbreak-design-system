# CombatFeed

A timestamped log of what just happened, for desktop players who want to read the fight.

**Provide** `entries` (`[{ t, actor, team, text, target?, targetTeam?, after? }]`, oldest first); optional `title`.

- Desktop only, collapsible, on the right edge; mobile surfaces the same events as world labels and the Results key moments.
- Names take their team text colour; everything else is `text-2`. Older rows fade.
- Write entries as facts with the cause first: "Rook stability 0 → Exposed", "Hex debuff missed Nyra".
