# Timeline

The replay transport: events on a scrubber (allies circles, enemies diamonds, phases squares), the pressure band, play controls and speed.

**Provide** `duration` and `position` (seconds); optional `events` (`[{ t, team: 'ally'|'enemy'|'phase', label }]`), `pressureFrom` (seconds), `playing`, `speed` (0.25, 0.5, 1, 2).

- Clicking an event seeks to it and opens it in the inspector (caster, manual/auto, Power before → after, aim lock, result).
- Replays play redacted: the opponent's automation rules stay hidden unless both players agree to share.
- The replay camera (Follow, Whole arena, Free) bypasses the auto-framing director.
