# RailButton

A `rail-width` × `rail-height` navigation tile: icon over a caps label, with an optional "new" diamond.

**Provide** `icon` and `label` (or `ariaLabel` for the icon-only menu button); optional `active`, `isNew`, `desktop` (104 × 80), `onClick`.

- Stack them in the left rail of menus (TEAM, AUTO, STUDIO, REPLAYS) with `space-2` between; the menu button sits alone top-right in battle.
- `active` marks the current destination (teal frame, `ally-soft` fill).
- `isNew` adds the `badge-new` diamond: something unseen is behind this button. Clear it when the player opens the screen.
- Labels are one word in caps; shorten AUTOMATION to AUTO on mobile.
