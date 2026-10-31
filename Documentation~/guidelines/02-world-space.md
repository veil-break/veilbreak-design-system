# 2 · World space

The arcade is 3D first: a lit, foggy street with the squad running up the screen. The HUD is a thin instrument layer
over it. This page covers what is drawn *in* the world: the screen-space readouts this package owns, and the rules
for the 3D layers it sits over.

Adapted from `veilbreak/design-system/guide/02-world-space.md`.

## `WorldLayer`: the readouts over the view

`WorldLayer` is a `VisualElement` sized to the battlefield band. It knows no camera; the host gives it a projector:

```csharp
layer = new WorldLayer { Project = world => /* layer units, or null when behind the view */ };
```

Every frame:

1. `BeginFrame()`.
2. Call `Bar`, `Label`, `Marker` and `Offscreen` for everything that should show this frame.
3. `EndFrame(delta)`. It removes what was not refreshed, drains health ghosts, animates combat text and declutters labels.

`Text` is event-driven: call it when the simulation confirms a hit, heal or break, never on a guess. Pass
**simulation** time to `EndFrame`, so numbers freeze while paused and slow under focus. Readouts follow the
simulation's clock, while menus and toggles animate in real time.

Elements are pooled by id. A fighter that dies or leaves the view simply stops being refreshed and its bar disappears
at the end of the frame. That is presentation only: a hostile is never removed from the simulation for leaving the
camera view.

## Health bars

- **Team colour.** Allies are teal and enemies crimson. Health is never a green-to-red ramp. `Tint` takes a kit accent
  where a game wants one.
- **Barrier** is drawn after the fill, capped by the empty part of the bar.
- **The ghost** holds recent damage for 0.25 s, then drains at 75% of the bar per second (`GhostHold`, `GhostRate`).
  A heal pulls it along.
- **Rank.** Normal bars are 36 × 4. Elite is 44 wide and boss 56, with an accent.
- Bars sit 27 units above the projected point (`BarLift`).

## Combat text

| Kind | Reads | Use |
| --- | --- | --- |
| `Damage` | `1,240` | Damage the squad deals, in text colour |
| `Taken` | `−86` | Damage an operative takes, enemy text colour |
| `Crit` | `1,240!` | Critical hits; pops |
| `Exposed` | `EXPOSED 1,240` | Hits on an exposed target |
| `Heal` | `+120` | Healing |
| `Barrier` | `BLOCKED 60` | Damage a barrier absorbed |
| `Power` | `+12 POWER` | Power gained from an event |
| `Miss` | `MISS` | Misses; never merges |
| `Execute` | `EXECUTE 2,400` | Executions; pops |
| `Break` | `BREAK` | Stability broken; pops, never merges |

- **Merging.** Same-kind numbers on one target that arrive within 0.2 s of each other add up into one
  (`MergeSeconds`).
- **Caps.** At most three live numbers per target, where a fourth reuses that target's oldest (`PerTarget`). At most
  40 across the layer (`TextLimit`), and further numbers are dropped.
- **Motion.** A number starts 40 units above the point and 18 to the right, so it clears the bar. It rises 24 units
  over 1.2 s and fades over the last third. Stacked numbers step up 14 units. Crit, execute and break start at 1.3×
  and settle within 0.15 s. Under reduced motion, numbers fade in place and never pop.

## Labels and markers

- **`WorldLabel`** kinds are Aim ("A · FAN −18"), Name, Telegraph ("BEAM · 0.4s"), Estimate ("MOVE ≈ 8") and Blocked
  ("× OUT OF RANGE"). A label is anchored at its bottom centre, 48 units over the point, and keeps a constant screen
  size. When labels overlap, later ones step upward and drop their detail line first. Labels never cover a telegraph.
- **`WorldMarker`**: allies are circles and enemies diamonds, always, so ownership reads from shape alone. Selected
  adds brackets, queued a lavender ring and move a dashed ring. Perspective rings on the ground belong to the 3D
  telegraph layer, not this one.

## Off-screen chips

`Offscreen(id, world, name, team, metres, threat)` pins an `OffscreenIndicator` to the board edge on the line from the
board's centre to the point, inside `Insets` plus `Edge` (10 units). The chip's chevron points at the fighter; it
hides while the point is on the board or behind the camera.

The arcade uses it for the **breach zone**. A non-summon enemy past z −600 shows a crimson BREACH chip with its
distance to the rear line. While any is inside the zone, an `EdgeWarning` band along the bottom edge ripples its
chevrons toward the rear line, faster and hotter as the closest one nears it. A diving interceptor also gets a
hostile lane on the ground from its position to the rear line.

## Ground telegraphs (3D, `com.veilbreak.feedback`)

Everything about *where* is drawn flat on the ground, in the simulation's own geometry:

- **Shapes:** projectile, fan, cone, burst, zone, beam, dash and wall.
- **Ally previews** have a solid `ally` edge.
- **Enemy threats** are crimson (`danger-zone`), with a hatched fill at `alpha-telegraph` (0.35) and a full-strength edge.
- **Windups:** an enemy's fill runs for its windup, tick-exact.

Telegraphs are unlit and sit above the ground. No effect, glow or label covers a telegraph edge.

## The battle camera

A raised perspective view up the street:

- **Base pose:** eye (0, 19, −14) looking at (0, 0, 1.5), 48° field of view, fitted to the battlefield band
  (`CameraFit`).
- **Player offsets**, on top of the base pose:
  - pan ±1.2 m
  - yaw ±10°
  - pitch ±6°
  - zoom 0.85–1.2×
- **Pointer parallax:** up to 3° yaw, 1.5° pitch and 0.25 m.
- **Idle:** the view springs back after 3 s without input, and a double tap resets it.

Gestures come from `GestureSurface` and feed `BattleCameraRig` (in `com.veilbreak.runner`). A skill or operative drag
always wins over a camera gesture. Reduced motion, or the camera setting at Reduced, turns off parallax, drift and
shake.

## Characters and light

Rules for the 3D layers, which the environment and feedback work follows:

- Rim light takes the team colour at low strength in battle, so silhouettes read ally or enemy at a glance.
- Neon and fire light the street, never a fighter's face at full strength.
- Only telegraph edges, team rings, neon and effect cores sit above the bloom threshold.

## Holographic staging (menus)

In the arcade's 3D lobby the squad stands on four holo platforms with rings in each operative's kit colour.
`HoloPlatform` is the 2D plate for menus without the lobby:

- **Occupied:** a teal plate and a caps name tag.
- **Selected:** brackets and a glow.
- **Open:** a dashed "+ OPEN SLOT".

Menu panels may be diegetic `HoloFrame`s, with scanlines, brackets and a flicker-in. Battle panels never are;
legibility beats immersion in a fight.
