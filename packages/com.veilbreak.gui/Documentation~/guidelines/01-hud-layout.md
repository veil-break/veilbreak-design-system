# 1 · Portrait HUD layout

The battle HUD is an instrument band over a 3D street. It frames the fight and never sits on it. Every screen is built
from the same parts (`HudTop`, `PowerStrip`, `QueueChip`, `OperativeCard`, `SkillButton`, `WorldLayer`) and the parts
move between zones; they are never redrawn per screen.

Adapted from the Veilbreak design system's landscape guide (`veilbreak/design-system/guide/01-hud-layout.md`) to the
portrait arcade. The landscape rail, minimap, combat feed and match header are not part of this kit (see
[web mirror](../web-mirror.md#legacy-web)).

## The board

- **Reference 390 × 844 units, match width.** `Runtime/Settings/VeilbreakPortrait.asset` scales the board to the screen
  width, so layout coordinates are the same on every phone and height is what varies.
- **Supported widths are 320 to 430 units.** `Tests/Runtime/FitTests` renders the kit at 320, 360, 390 and 430 and fails
  when a part leaves the board or a control is under 44 units on either axis.
- **Compact boards** are narrower than 380 units (`SafeAreaRoot.CompactWidth`). The root carries `vb-compact`, and
  layouts there trade width for height: operative cards stretch to share the row and stack their two skills.
- **Safe area.** `SafeAreaRoot` pads the root by the device's insets, with at least 48 units on top and 34 at the
  bottom for devices that report none. It insets the screen layer by the same amounts.

## Zones, top to bottom

| Zone | Holds | Rules |
| --- | --- | --- |
| HUD band (under the top inset) | `HudTop`: WAVE or BOSS and score, AUTO toggle, speed, pause. Under them the resources panel: `PowerStrip` (value, regen, live hint, a mark at the next queued cost) and `FocusMeter`. `BossBar` hangs below in boss waves. | Translucent and short. It is read-only apart from its three toggles. Tap, hover or focus on the resources panel opens the full `QueueList`, where a manual entry cancels on tap. A standalone `PowerStrip` can also carry the reserve line, queue chips with × and a legend, as in the BattleHud sample. |
| Battlefield (the rest) | The 3D view. Over it, `WorldLayer` draws bars, combat text, labels, markers and off-screen chips. `TargetDrag` and `GestureSurface` take input on it. | At least half the board. No panel floats over it during play. The camera frames the squad inside this band (see [the camera's safe band](#the-cameras-safe-band)). |
| Cards (above the bottom inset) | Four `OperativeCard`s (88 × 202; compact: equal shares, 250 tall). Each has portrait, mode chip, health over stability, statuses and two `SkillButton`s (44 × 56). | The thumb zone. Every target is at least 44 × 44. Skill drags start here and end on the battlefield. |
| Cancel band (arcade) | "RELEASE HERE TO CANCEL · NO POWER SPENT", just above the cards while an aim is live. | A release over it cancels without spending; it only shows during a drag. |

## Screen layer

`Navigator` owns one absolute screen layer (`vb-screen`) above the board, inset like the root. A page
(`vb-page vb-page--{name}`) replaces the screen's content. `OverlayHost` layers (`vb-modal`) stack over the page, blur it
and return focus when they close. Pages opened during a run cover the HUD, cards and world layer with `vb-covered`, so
nothing behind a page takes input. Combat itself only stops when the game pauses, and settings in a run open from Pause.

**Back** steps out in a fixed order:

1. The topmost popover closes.
2. Otherwise the top modal closes.
3. Otherwise the page's own back action runs.
4. Otherwise the host's fallback runs. In the arcade that pauses a run, or asks to exit from the menu.

While `Navigator.Blocked` returns true (the intro splash), Back does nothing.

## Layers

UI Toolkit draws in tree order, so the order of children is the order of layers. Back to front:

| Layer token | In the portrait client |
| --- | --- |
| `layer-scene`, `layer-telegraph`, `layer-entity`, `layer-vfx` | The 3D camera: street, ground telegraphs and decals, fighters, effects. Effects never cover a telegraph. |
| `layer-world-label` | `WorldLayer`, the first child of the root, under every panel. |
| `layer-hud` | The HUD band and the card row. |
| `layer-popover` | `Popover` content on the panel's root, so it is never clipped; it copies the root's accessibility flags. |
| `layer-modal` | `vb-screen` pages and their `vb-modal` layers. The intro splash sits above everything. |

A label never covers a telegraph, and the HUD never covers a living operative once the camera settles.

## The camera's safe band

The battle camera fits the formation inside the battlefield band, which is the space the HUD band and the cards
leave. A gesture offset (pan, rotate, zoom) is shrunk until every operative, the formation slots and the rear
clearance still project inside that band. So:

- A new panel over the battlefield must shrink the band the camera fits to, or an operative can end up under it.
  Prefer growing an existing zone to adding a floating panel.
- Popovers and pages are exempt. Popovers are short-lived, and pages cover the run.
- Off-screen chips keep clear of the HUD band with `WorldLayer.Insets`.

## Menus

Menus use the same tokens at a calmer pace:

- On Standard without Reduced Motion the arcade draws its 3D lobby behind the main menu. The menu reports the strip
  between its buttons and title, and the squad is fitted into it, so no operative stands under the title. On Low and
  under Reduced Motion a `Backdrop` image fills the screen instead.
- Pages stack a `ScreenHeader`, a scroll body (`vb-page__scroll`) and at most one `Primary` button at the bottom.
- Settings group into `Ui.Section` panels, with switches, sliders and segmented rows.
- Holographic panels (`HoloFrame`, `HoloPlatform`, `RevealCard`) are for menus only, one in focus per screen. Battle
  panels never flicker or tilt.
