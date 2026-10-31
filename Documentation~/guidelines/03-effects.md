# 3 · HUD effects

The HUD should feel like a tactical instrument projected over the street: crisp, slightly luminous and quietly alive.
Every effect here comes from a USS property or a small painter in this package, means one thing, and turns off
cleanly.

Adapted from `veilbreak/design-system/guide/03-effects.md`. The web kit's utility classes become properties read by
`VbShape` (UI Toolkit has no `clip-path`, `filter` or keyframes).

| Effect | In this package | Means | Where | Off when |
| --- | --- | --- | --- | --- |
| Cut frame | `VbShape`: `--vb-cut`, `--vb-fill` (`--vb-fill-to`, `--vb-fill-dir`), `--vb-edge` (`--vb-edge-to`), `--vb-bw` | "This is a HUD object" | Every panel, button, chip, tile and card | Never |
| Brackets | `--vb-bracket`, `--vb-bracket-len` (L marks on the two square corners) | Selected, focused, locked on | Focused tile, aiming operative, selected perk, holo frame and plate | Never (it is a state) |
| Glow | `--vb-glow`: three soft rings outside the cut | The one thing to look at | Primary button, aiming skill and operative, focused tile, selected perk, Legendary tier, refused-command flash | High contrast, reduced flashes |
| Dashed frame | `--vb-dash: 1` | Blocked, short, queued, a preview, empty | Blocked button, short-on-Power and queued skills, preview queue chip, empty squad slot, draft rule | Never (it is a state) |
| Hatch | `Telegraph.shader` in `com.veilbreak.feedback` | Danger on the ground | Enemy telegraphs, the breach zone | Never (it is a state) |
| Scanlines | `ScanLines` (`--vb-scan`, `--vb-scan-pitch`), one mesh | "Projected image" | `HoloFrame`, `HoloPlatform` plates, `RevealCard` backs | High contrast. Never behind live battle numbers. |
| Flicker-in | `HoloFrame.FlickerIn()`: opacity .7 → .25 → .9 → 1 in 60 ms steps | Something just appeared | Holo panels as they attach | Reduced motion or reduced flashes: a plain fade |
| Chromatic edge | `HoloFrame` title copies at ±1 unit in cyan and pink | A projection settling | Holo titles | Reduced motion, reduced flashes, high contrast |
| Sweep | `SkillButton` `__sweep`, filled from `SkillView.progress` | A windup in progress | Skill buttons in `Windup` | Never: it is simulation data, tick-exact, with no animation of its own |
| Charge | `SkillButton` `__charge`, filled from `SkillView.charged` | Recharging | Skill buttons in `Cooldown` | Never |
| Flash | `SkillButton` `--flash` for 400 ms on entering `Rejected` | A command was refused | Skill buttons | Reduced flashes: a faint tint, no glow |
| Next frame | `QueueChip` `--next` (thicker frame, `›` for a buffered cast) | Fires next | First queue entry | Never |

The web kit's pulse and tilt have no UI Toolkit equivalent here. The next queue entry is marked by its frame instead
of a loop, and depth comes from the 3D menu camera.

## Rules

1. **An effect never carries information alone.**
   - Brackets come with a 2-unit frame.
   - A dashed frame comes with a number or a word (NEED 12, the blocked reason).
   - A glow comes with a selected frame.
   - Statuses carry a glyph and a word.
2. **Glow budget: one per cluster, two per screen at most.** These are the primary button and the selected or aiming
   thing. A `Primary` `VbButton` is the only CTA on its screen.
3. **Scanlines stay under 0.08 alpha** (the kit's holo surfaces use 0.06; the `scan-alpha` token for panels is 0.03). They are never painted behind
   numbers read under pressure: Power, cooldowns, health.
4. **Motion is short.**
   - Flicker-in and state changes settle within `dur-ui` (220 ms).
   - Sweeps last exactly the windup they show.
   - Nothing loops forever.
5. **Telegraphs win.** No effect, glow or label covers a ground telegraph edge.
6. **Settings are honoured everywhere** through root classes that `SafeAreaRoot.Flags` sets, and that popovers copy
   from their anchor:

   | Root class | Effect |
   | --- | --- |
   | `vb-reduced-motion` | Movement becomes fades: no combat-text rise or pop, no card flip, holo flicker becomes a fade, chromatic copies hide |
   | `vb-reduced-flashes` | No glows, no flicker, no chromatic split, and the refused flash is a faint tint |
   | `vb-contrast` | The contrast colour theme: no scanlines, no chromatic split, no glows; frames and brackets stay |

## Recipes

- **Holo panel:** `HoloFrame`, which is `VbShape` with cyan brackets, `ScanLines`, a chromatic caps title and a
  flicker-in.
- **Queued skill:** `SkillButton` in `Queued`, which shows a dashed lavender frame, QUEUE and its `#n` place in line, matching
  the `QueueChip` with the same number.
- **Refused command:** `SkillButton` in `Rejected` flashes crimson once. The game says why in the PowerStrip hint.
- **Rift reveal:** `RevealCard`. The back has scanlines and the mark. `Flip()` squashes edge-on, with higher tiers
  waiting a beat, and a Legendary front glows gold.

## Battle effects in 3D

Combat particles, decals, lights and post-processing belong to `com.veilbreak.feedback`. They are played from confirmed
simulation events through the cue catalog and bounded pools, never spawned ad hoc. They follow the same settings:
reduced flashes removes brightness pulses, and reduced motion removes shake.

- **Arrival fractures:** where a hostile arrives, the street or a wall cracks open. Lit fissures race out from a burning
  pit, then seal and fade. Grit, dust and motes burst out within the effect budget.
- **Telegraphs:** an over-bright solid rim with a soft glow, then a light curtain of vertical rays and dust rising from
  hostile outlines. The curtain is taller for elites and bosses.
- **Heavy hits:** a whole-screen impact (shockwave refraction, a zoom punch and chromatic split, a vignette in the hit's
  colour) with shake, sparks, a flare and a ground ring. Reduced motion drops the refraction, punch and split; reduced
  flashes drops the vignette.

## Heads-up projection pattern

Use `GlitchText` with `Glitch.In/Out/Pulse` and an `Announcer` for transient heads-up text. The red and cyan channels
use `glitch-red` / `glitch-cyan` and `--vb-chroma-offset`; eight clipped bands briefly displace independent strips.
The base text is always the layout source. Copies ignore pointer input. A completed or cancelled transition hides
all copies and restores transforms. Do not loop the distortion behind live numbers.

The wave number's brief 2.4× slam is the exception to the no-overshoot rule. Page deals use small displacement and opacity,
without overshoot. Rare cards use cyan and a RARE label; Epic cards use violet, an EPIC label and a short shimmer.
Reduced Motion and Reduced Flashes replace glitch effects with fades and remove shimmer. High Contrast keeps the text
and tier frames. These are UI Toolkit effects, independent of the game's URP surface or post-processing.
