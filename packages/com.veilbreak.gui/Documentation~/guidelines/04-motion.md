# 4 · Motion

Motion is information first: it says *something changed* or *something is coming*. Default to `dur-ui` with
`ease-out`. The wave-number slam is the one sanctioned overshoot; other interface motion settles without bouncing.

Adapted from `veilbreak/design-system/guide/04-motion.md`.

## Two clocks

| Clock | Drives | Why |
| --- | --- | --- |
| Simulation time | `WorldLayer.EndFrame(delta)`: combat text rise and fade, health ghosts. `SkillView.progress`, `charged` and `seconds`: windup sweeps, cooldown fills and countdowns. | Readouts describe combat, so they freeze while paused, slow under focus and run double at 2×. |
| Real time | USS transitions, `HoloFrame` flicker, `RevealCard` flip, `SkillButton` refused flash, focus drain. | The interface answers the player at the same speed whatever the fight is doing. |

Pass the simulation's frame delta to `EndFrame`, never `Time.deltaTime`.

## Durations

| Token | Value | Use in this package |
| --- | --- | --- |
| `dur-press` | 100 ms | Button press scale (`vb-button` scale transition) |
| `dur-ui` | 220 ms | State changes: the switch knob slides, the holo flicker settles (4 × 60 ms) |
| `dur-glitch-in`, `dur-glitch-out` | 220, 120 ms | Every heads-up entrance, replacement and exit; short page deals |
| `dur-wave-slam` | 140 ms | Wave number 2.4 → .91 → 1, chromatic smear; 180 ms HUD/camera jolt |
| `dur-crossfade` | 450 ms | Screen fades (game-side; the kit swaps pages at once) |
| `dur-pan-in`, `dur-pan-out` | 180, 600 ms | Camera action pan in and return |
| `dur-zoom-out`, `dur-zoom-in` | 250, 900 ms | Camera zoom; no operative is clipped longer than `dur-zoom-out` |
| `dur-summon` | 1500 ms | Rift portal charge before a reveal |
| `dur-label-hold` | 2000 ms | A prop's structure bar after a hit |

Component timings that are not tokens:

| Where | Timing |
| --- | --- |
| Combat text | Lives 1.2 s (`TextSeconds`) and rises 24 units, fading over its last third. Crits, executes and breaks start at 1.3× and settle in 0.15 s. |
| Health ghost | Holds 0.25 s, then drains at 75% of the bar per second. |
| Refused flash | 400 ms. |
| Card flip | A 110 ms edge-on squash; violet waits 90 ms and gold 180 ms first. |

## Battle

- **Windups are exact.** A skill button's sweep and a telegraph's fill run for the kit's windup, tick for tick, from
  simulation progress. They are never eased to look nicer.
- **Cooldowns fill** from the bottom as the skill recharges, with the seconds counting down.
- **Queue entries** move up as earlier entries fire. The next one wears the thicker `--next` frame, or `›` when
  buffered.
- **Combat text** rises and fades; it does not shake or spin.
- **Exposed** inverts the skill label once and holds; it never strobes.

## Menus

- Holo panels flicker in within `dur-ui`.
- **Rift reveal:** the cards flip one by one, and higher tiers wait a beat. Skip and Reveal All are always available
  (game-side).
- The camera drifts toward the pointer by at most the parallax limits in [world space](02-world-space.md#the-battle-camera).

## State changes

Every GUI and HUD state change animates through `VbMotion` (see the [component reference](../components.md#vbmotion)); a view
never flips `display` directly.

| Change | Motion |
| --- | --- |
| Page change (`Navigator.Open`) | The old page fades out while the new one rises in (`dur-ui`). |
| Screen close | Fades out (`dur-glitch-out`). |
| Modal layer | The scrim fades in and the content rises in, staggered. Closing fades the layer out; input under it returns at once. |
| Popover | Drops (or rises) in and fades out. |
| Shown or hidden part (hints, previews, chips, barrier, sheets, checkpoint card) | `VbMotion.Show`: rise, zoom or fade in, fade out. |
| Lists, grids and dealt cards | Staggered rise, 50 ms apart. |
| Content replaced in place | `Swap`: fade out, rebuild, fade in. |
| Number change (score, credits) | `Count` rolls it over `dur-change`. |
| Toggle or value change (speed, autoplay, header meta, skill ready) | `Pop`: a single 10% swell. |
| Refused action | `Shake`, with the refused flash. |
| State colours (VbShape fills, edges and glows; text and tint) | Cross-fade over `dur-ui` (`Motion.uss` and `VbShape`). |

UI motion runs on real time, so it keeps its speed under Clock focus. It never delays input: leaving pages and layers
stop taking input as soon as they start to leave.

## Reduced motion and reduced flashes

The game passes both settings to `SafeAreaRoot.Flags(largeText, reducedMotion, contrast, reducedFlashes)` and sets
`WorldLayer.ReducedMotion`:

| Setting | Effect |
| --- | --- |
| Reduced motion (`vb-reduced-motion`) | Every `VbMotion` movement becomes a fade, `Count` jumps and `Motion.uss` transitions stop. Combat text fades in place without rising or popping. `HoloFrame` fades instead of flickering and loses its chromatic copies. `RevealCard` turns face up at once. The camera drops parallax, drift and shake. |
| Reduced flashes (`vb-reduced-flashes`) | No glows. `HoloFrame` fades instead of flickering and loses its chromatic copies. The refused flash becomes a faint tint. Global brightness pulses stop (feedback package). |

Sweeps and cooldown fills stay in both modes: they are data, not decoration.

## Heads-up announcements and page deals

`GlitchText` keeps one readable label, two red/cyan copies and eight clipped horizontal bands. `Glitch.In`,
`Out` and `Pulse` use the generated timing and colour tokens; a replacement cancels the old callback.
`Announcer` queues at most six notices, glitches the old text out before replacement, and gives critical alerts priority.
Wave starts, incoming/boss names, clears, signal loss, breaches, fallen operatives, upgrades, route/market prompts,
transaction notices and command toasts enter through this pattern, once per confirmed event. The command lane is separate
from the headline; repeated identical commands still show via the host's feedback serial.

The wave number glitches out before a 140 ms slam. A brief 2-unit HUD jitter and 38 Hz camera jolt accompany its landing.
These effects run on real time; they never change simulation ticks. Perk, route and merchant pages deal in within 220 ms,
with perk cards offset by 50 ms. Epic cards carry a short violet shimmer every four seconds, never a continuous strobe.

Reduced Motion replaces all projection movement, sliced offsets, flicker and wave scaling with fades; it disables both jolts
and the epic shimmer. Reduced Flashes also uses fades, removes chromatic splitting and shimmer, and leaves the readable
rarity label and border. High Contrast suppresses decorative channel copies and shimmer. The settings are checked during
transitions, so changing a setting takes effect without waiting for the next message. Detaching a page cancels its work.

## Feel recipes (gui 0.9.1, design-system#69)

Seven small recipes (`Tokens~/motion.json`, generated into `MotionRecipes` and `Tools~/web/motion-recipes.js`) cover the places that jumped. Each has a branch for reduced motion and one for reduced flashes, reaches the same end state in all four settings
(full, reduced motion, reduced flashes, both) and leaves nothing behind (`MotionLifecycleTests`).

| Recipe | Time | What moves | Reduced motion | Reduced flashes |
| --- | --- | --- | --- | --- |
| `DrawerFit` | 180 ms, real time | a Fit drawer's sheet eases to its new content height | jumps | (no flash) |
| `LabelMerge`, `LabelSplit` | 180 ms, simulation time | a merged world label swells to 1.12 (members joined) or shrinks to 0.9 (members left) with a brief dip in opacity | opacity dip only | scale only |
| `DragLift` | 120 ms | a dragged child's ghost rises 4 and grows to 1.05, fading in from 55% | flat, fade only | no fade-in |
| `DropSettle` | 220 ms | the ghost glides into the slot back to full size, then the slot pops | ghost goes at once | (no flash) |
| `PressSink` | 100 ms | a pressed skill sinks 1 unit and to 0.92, back on release | does not move, dims 18% | no brightened plate |
| `Fallen` | 420 ms | a card that falls slumps 6 units and 0.97 back to rest as its colour drains | colour drains only | damage flash without its fill |

`VbMotion.Hold(e, recipe, on)` holds an element in a recipe's pose while it is pressed and eases back on release; `VbMotion.Slump(e, recipe)` eases from a pose to rest. Both run on real time, make no garbage per frame and finish when the element leaves its panel.
Combat readouts (the label merge) follow the simulation's `delta`, never the clock.

## Overlays, edges and the title reveal (gui 0.9.0)

Every overlay has a short transition from one recipe family (`MotionRecipes`): a dialog or pause sheet fades and rises in 220 ms (`OverlayIn`) and leaves in 150 (`OverlayOut`), a popover takes 140 and 90 (`PopoverIn`, `PopoverOut`), a drawer slides in 320
and out in 220. The modal layer that dims the page only fades; the sheet in it brings the rise. Reduced motion keeps the fade and drops the movement.

An activation edge (`VbEdgeFx`) answers a press: two bright heads run clockwise round the control's cut outline from opposite points, each with a fading tail, in the control's own edge colours, for 520 ms. A unit card taking damage flashes a translucent red
and glows red at its edge for 560 ms (`UnitCard.Damage`). Reduced motion lights the whole edge at once and fades it in 100 ms; reduced flashes drop the red flash fill and keep a dimmer edge.

A title or eyebrow reveals with `Glitch.Reveal`, 2000 ms: the text decodes out of noise glyphs left to right, jitters sideways in 4-unit steps that shrink to nothing, climbs in opacity in coarse steps (bit reduction) and, as a `GlitchText`, tears into bands with a
chromatic split on the same 4-unit grid, then rests. It is for the verdict and pause titles and heads-up notices (`Announcer.RevealMilliseconds`), one per screen. Reduced motion or flashes show the whole text and fade it in over 300 ms.
