# Minimap

A holographic top-down map of the 40 × 62 m arena: fighters, cover by state, the camera's view and the pressure region. *Intentional addition* for the MOBA layer; the camera auto-frames every fighter, so this is a reading aid, not navigation.

**Provide** `fighters` (`[{ x, y, team, selected?, defeated? }]` in metres), `props` (`[{ x, y, w, h, state }]`, state `intact`, `cracked`, `broken`, `rubble`, `anchor`); optional `view` (`[x, y, w, h]` camera rect), `safe` (pressure safe region), `width` (px, default 124), `caption`.

- Desktop: top-right corner. Mobile: off by default; offered as a setting that replaces the menu button with a collapsed map.
- Allies are circles, enemies diamonds, defeated an ×; cracked cover is outlined in `prop-hit`, broken is dashed, rubble dotted.
- Tapping the map never moves the camera in battle (the camera is automatic); in replays it sets the free camera.
