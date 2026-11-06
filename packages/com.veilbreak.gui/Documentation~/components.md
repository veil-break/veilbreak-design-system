# Component reference

Every public part of `Veilbreak.Gui`, grouped as in `Runtime/`. For each one:

- **Build:** how to make it, from code or UXML.
- **API:** what to call.
- **View model:** what it binds.
- **States:** its modifier classes.
- **USS:** its block and element classes.
- **Access:** keyboard, touch and colour-independent cues.

Conventions shared by every component:

- **Names.** Classes are BEM: block `vb-skill`, element `vb-skill__icon`, state `vb-skill--cooldown`. States are
  modifier classes, so a game can restyle a state without code. `Ui.State(element, block, enumValue)` keeps exactly
  one modifier per enum set: `Cooldown` becomes `--cooldown`, `RarityTier.Gold` becomes `--gold`.
- **Binding.** Components are bound, not rebuilt. Call `Bind(view)` every frame if you like: it reuses its children,
  and rebuilds a sub-list only when its key changes.
- **UXML.** Every `[UxmlElement]` works in UXML with `xmlns:vb="Veilbreak.Gui"`, for example
  `<vb:VbButton title="PLAY" variant="Primary" sub="Squad · 4"/>`. `[UxmlAttribute]` properties are the attributes.
- **Shapes.** Anything that calls `VbShape.Attach` carries `vb-shape` and is painted from `--vb-*` properties (see
  [VbShape](#vbshape)).
- **Caps.** UI Toolkit has no `text-transform`, so components uppercase names and titles themselves. Pass normal case.

---

## Core

### VbShape

The Veil cut shape: top-left and bottom-right chamfers, painted as a mesh because UI Toolkit has no `clip-path`.

- **API:** `VbShape.Attach(element)` returns the element. Attaching twice is a no-op.
- **USS:** reads these properties:

  | Property | Meaning |
  | --- | --- |
  | `--vb-cut` | Chamfer size in units |
  | `--vb-fill`, `--vb-fill-to`, `--vb-fill-dir` | Fill colour, optional gradient end, and its direction in degrees (0 = left → right) |
  | `--vb-edge`, `--vb-edge-to` | Edge colour and optional gradient end |
  | `--vb-bw` | Edge width |
  | `--vb-dash` | `1` draws a square dashed frame |
  | `--vb-bracket`, `--vb-bracket-len` | L marks on the two square corners |
  | `--vb-glow` | Soft outer bleed |

  `.vb-shape` resets them all, because custom properties inherit. These names are canonical for both painters:
  `VbVectorShape` reads its `--vb-vector-*` alias first, then the canonical name, then ar-hud's bare name.
- **Border rule:** a surface inside another surface sets `--vb-bw: 0` (no dashes, brackets or glow) and takes a fill one tone up.
  `Gui.uss` lists them between `nest:begin` and `nest:end`; `.vb-nest` opts any element in; `.vb-contrast` restores the
  edges (`Layout.uss`). Edges stay on floating surfaces, the one primary action, choice cards, selection and focus rings,
  danger actions and the empty drop target.
- **Notes:** the content box and hit area stay the element's rectangle. Token colours work directly:
  `--vb-edge: var(--vb-color-ally)`.

### VbFrame and panels

`VbFrame` is a `VisualElement` with a shape, for UXML: `<vb:VbFrame class="vb-panel vb-panel--solid"/>`. From code,
use `Ui.Panel(parent, classes…)`.

- **States:** `vb-panel--solid`, `--raised`, `--glass`, `--enemy`, `--ally`, `--power`, `--brackets`.
- **USS:** `vb-panel__head`, `__title`, `__meta`. `Ui.Head(panel, title, meta)` builds the head.

### Popover

Hover, focus and tap popovers. Content is built on demand and placed on the panel root, so it is never clipped. A popover is
one solid surface with padding, one edge and a small painted arrow toward its anchor (`vb-popover__arrow--up` or `--down`);
it is clamped to the panel (at most 340 wide, the panel's height less its margins, the content scrolls past that) and placed
again whenever the popover, the anchor or the panel changes size.

- **Build:** `Popover.Attach(anchor, () => content, PopoverPlacement.Below | Above, alignEnd)`.
- **API:**
  - `Open()`, `Close()`, `IsOpen`.
  - `Pin()` keeps it open (galleries, tutorials, captures).
  - `Popover.CloseAll()` returns whether any were open.
- **Content:** gets `vb-popover__content` (no surface of its own). `PopoverCard(title, hint)` is the title, hint and flat rows
  layout (`Row(key, value)`, `Rows`). `Popover.Fit` and `Popover.Limit` are the pure placement and size limit.
- **States:** the anchor carries `vb-popover-open`. The content is wrapped in a `ScrollView` with `vb-popover`.
- **Access:** a mouse opens it on hover. Touch toggles it on tap. Focus opens it and blur closes it. It copies the
  anchor's root flags (`SafeAreaRoot.RootClasses`), so contrast, larger text and the motion settings reach it.
- **Notes:** only theme sheets reach the panel root. Keep the kit in the theme (`VeilbreakGui.tss`).

### OverlayHost

Nested modal layers over a screen layer.

- **API:**
  - `Push(content, onClose)` returns a `vb-modal` layer and covers `content` (`vb-covered`, disabled).
  - `Pop()`, `Clear()`, `Count`.
- **Access:** focus moves to the new layer and returns to the previously focused element when it closes. Opening a
  layer closes all popovers.
- **Motion:** the scrim fades in and the layer's children rise in, staggered, on the next frame. `Pop` renames the layer
  `modal-layer-leaving`, disables it and fades it out before removing it; the covered content is restored at once.

### Navigator

One page at a time on a screen layer, with modal layers and popovers above it.

- **Build:** `new Navigator(screenLayer)`.
- **API:**
  - `Open(name, back)` clears the screen and returns an empty page, `vb-page vb-page--{name}`, on
    `vb-screen vb-screen--{name}`.
  - `Push(onClose)` adds a modal layer over the page.
  - `Close()`, `Back()`, `Page`, `Visible`, `Content`, `Modals`.
  - Events `Opening(name)` and `Closed`. Hooks `Blocked` (Back does nothing while true), `Fallback` and `PageBack`.
- **Back order:** popover, then modal, then `PageBack`, then `Fallback`. It returns false only when nothing handled it.
- **Motion:** `Open(name, back, enter = MotionKind.Rise)`. The old page is marked `vb-page--leaving` (absolute, disabled),
  fades out and is removed while the new one enters; a page opened on a hidden screen fades. Re-opening the open page
  swaps it at once. `Close()` fades the screen out and raises `Closed` immediately; `Visible` follows the target state.
- **USS:** `vb-screen` and `vb-page`, plus `vb-page--headed` (under a `ScreenHeader`), `vb-page__label`, `__caption`,
  `__footer` and `__scroll`, all in `Layout.uss`.

### VbMotion

Real-time UI motion for every show, hide and value change (`Runtime/Core/VbMotion.cs`). It runs on the panel scheduler
with one timer per element; a newer call cancels the older one, and the element's own translate, scale and opacity are
restored afterwards.

- **Show and hide:** `Show(e, on, kind, done)` (idempotent), `In(e, kind, delayMs, done)`, `Out(e, kind, done)` (remove the
  element in `done`). `MotionKind` is `Fade`, `Rise`, `Drop`, `Left`, `Right` or `Zoom` (from 0.94). Entrances take
  `dur-ui`; exits take `dur-glitch-out`.
- **Groups:** `Stagger(items, kind, start, step)` and `StaggerChildren(parent, ...)`, 50 ms apart; `Swap(e, rebuild, kind)`
  fades out, rebuilds and fades back in.
- **Value cues:** `Pop(e, amount)` swells once, `Shake(e)` shakes sideways (refusals), and `Count(label, value, format)` rolls
  a number over `dur-change`.
- **State:** `Shown(e)` (target state), `Displayed(e)` (inline display when code set one, else the resolved style, so a UXML or USS `display: none` counts), `Busy(e)`, `Finish(e)`, `Calm(e)` (under Reduced Motion), `Ease(p)` (ease-out cubic).
- **Rules:** Reduced Motion turns movement into fades and makes `Count` jump. Elements without a panel change at once, so
  binding stays synchronous off screen. Sweeps, cooldowns and windups never go through `VbMotion`: they are tick-exact
  data.

### SafeAreaRoot

The one owner of safe-area insets and root flags.

- **Build:** `new SafeAreaRoot(root, panelSettings, overlays…)`. The constructor also calls `FontPrimer.Prime(root)`. This adds
  the kit's glyphs to every face's dynamic atlas on the first layout pass, so title tracking never depends on the order a
  session first shows characters.
- **API:**
  - `Refresh()` each frame re-applies only when `Screen.safeArea` or the resolution changed.
  - `Apply(area, screen)` sets explicit insets.
  - `Insets` holds the left, top, right and bottom insets in panel units.
  - `UnitsPerPixel(screen)` covers Scale With Screen Size (any match) and Constant Pixel Size.
  - `Flags(largeText, reducedMotion, contrast, reducedFlashes)`.
  - Fields `MinTop` (48), `MinBottom` (34), `CompactWidth` (380).
- **Root classes:** `vb-compact` (board narrower than `CompactWidth`), `vb-large-text`, `vb-reduced-motion`,
  `vb-reduced-flashes`, `vb-contrast`.

### Ui

The fluent builders every component uses:

| Group | Helpers |
| --- | --- |
| Elements | `Box`, `Text`, `Panel`, `Section(parent, title)`, `Head`, `Scroll` (hidden scrollbars), `Spacer` (`vb-grow`), `Crop` (an illustration crop inside a frame) |
| Chaining | `Classes`, `Into`, `Flag`, `Show` |
| State and input | `Clickable(action)` (pointer, Enter and Space, focusable), `State` (one modifier per enum), `Under(e, cls)` (an ancestor carries a class), `Changed` (value callbacks for `VbSlider`, `VbSwitch` and `VbSegmented`) |
| Fills and images | `Image`, `Fill(e, fraction, vertical)` |
| Formatting | `Number` (1,234), `Compact` (12.3K, 4.5M), `Seconds` (4.2s, 1:05), `Kebab` |

### VbTokens

Generated colour constants for code that cannot read USS: rich text, meshes and materials.

- **API:** `VbTokens.Ally`, `VbTokens.Text3` and so on, one per colour token. `VbTokens.Kit(id)` returns an
  operative's accent. `VbTokens.Hex(color)` gives `#rrggbb` for rich-text tags.
- **Notes:** veil theme only. Code that must follow the contrast theme reads `var()` in USS instead. See the
  [token reference](tokens.md).

### MarkGeometry

The VEIL/BREAK mark as data: `Parts` holds 15 entries (id, blade flag and SVG path), and `Rings(d)` yields its
subpaths. `VbMark` and the branding package's intro splash both draw from it.

---

## Controls

### VbIcon

A 24-grid stroke glyph by name.

- **Build:** `new VbIcon("cooldown", 16)`, or `<vb:VbIcon icon="cooldown"/>`.
- **API:** `Icon` (any case; kebab-cased), `Size(px)`, `Tint(color)`.
- **USS:** `vb-icon vb-icon--{name}`. `Icons.uss` maps each class to its SVG: 99 glyphs, plus 8 design-system
  aliases (`add`, `silence`, `root`, `slow`, `replays`, `close`, `chevron`, `rifts`).
- **Access:** icons never stand alone for meaning. Statuses add a word, and buttons add a label or tooltip.

### VbMark

The slash-cut VEIL/BREAK mark.

- **Build:** `new VbMark(260)`. Its height follows the 513 × 259 artwork.
- **USS:** `vb-mark`.

### VbButton

A cut-corner button.

- **Build:** `new VbButton(title, ButtonVariant.Primary, sub, icon, click)`.
- **API:** `Title`, `Sub`, `Icon`, `Variant`, `Blocked`, `clicked`.
- **States:**
  - Variants: `vb-button--primary` (pink → teal glass with a gradient edge and glow; one per screen), `--secondary`,
    `--light`, `--danger`, `--ghost`.
  - `--blocked`: a non-empty `Blocked` reason disables the button, dashes its frame and shows the reason in place of
    the sub line.
- **USS:** `__text`, `__label`, `__sub`, `__icon`.
- **Access:** focusable, with Enter and Space (it is a `Button`). A blocked button stays legible and says why, in
  under five words.

### StatReadout

A labelled number with an optional bar.

- **Build:** `new StatReadout("HEALTH", "1,240", unit, bar01)`.
- **API:**
  - `Set(value, unit, bar)`.
  - `Delta("+12")`: a leading `+` reads up (teal) and `−` down (pink). `up` overrides when lower is better.
  - `Tone("power" | "text")`.
- **States:** `vb-stat--power`, `--text`, `--neutral`. On the delta: `__delta--up`, `__delta--down`.
- **USS:** `__label`, `__row`, `__value`, `__unit`, `__delta`, `__bar`, `__fill`.

### StatusChip

A status with its own glyph and word.

- **Build:** `new StatusChip(StatusKind.Silenced, text, detail, time)`. `StatusChip.Icon(kind)` gives the glyph name.
- **View model:** `StatusKind`: Exposed, Silenced, Rooted, Slowed, Interrupted, Barrier.
- **States:** `vb-status--{kind}`.
- **USS:** `__icon`, `__word`, `__detail`, `__time` (Share Tech Mono).
- **Access:** glyph and word together, so no two statuses read alike and none relies on colour.

### VbMeter

A flat track with a fill, an optional extra span after it (barrier), a dashed preview span and numbered marks.

- **API:** `Set(value01, extra01)`, `Preview(from, to)` (a negative `from` hides it), `Marks(fractions)`.
- **USS:** `vb-meter`, `__fill`, `__extra`, `__preview`, `__mark`, `__mark-label`.

### HealthBar

Health over stability, with barrier, ghost and caption.

- **API:**
  - `Set(hp, max, barrier, stability01, exposed, enemy, ghost)`. `ghost` is health before the last hit; the game
    lowers it over time.
  - `Size(MeterSize.Hud | World | Large)`.
  - `Caption("HP 820 / 1000")`.
- **States:** `vb-health--exposed` (the stability track fills in enemy colour), `--enemy`, `--hud`, `--world`, `--large`.
- **USS:** `__label`, `__hp` (a `VbMeter`), `__ghost`, `__stability`.
- **Access:** team colour plus a numeric caption or readout nearby; never a green-to-red ramp.

### PowerStrip

The shared Power pool.

- **API:**
  - `Set(value, max, regen, hint, previewFrom, previewTo, marks)`.
  - `Reserve(at, max)`: the autopilot's soft reserve as a line.
  - `Queue(entries, cancel)`: up to four `QueueChip`s, first in first out.
  - `Legend(text)`.
- **View model:** `QueueView` entries (see [QueueChip](#queuechip)).
- **USS:** `vb-power`, `__head`, `__label`, `__value`, `__max`, `__regen`, `__hint` (truncates with an ellipsis),
  `__meter`, `__reserve`, `__queue`, `__legend`.
- **Access:** the hint states the live consequence of an aim ("Fan −18 → 16 left", "Wall replaces auto · −40"). The
  dashed preview span shows the same cost on the track.

### FocusMeter

Focus drains in real time while targeting slows combat, and regenerates outside it.

- **API:** `Set(focus, slow, regenPerSecond, drainPerSecond, slowScale)`.
- **States:** `vb-focus--slow`.
- **USS:** `__label`, `__meter`, `__value`, `__rate` (`×0.2 · −25/s`, `+12.5/s` or READY).

### SkillButton

A compact 44 × 56 skill control for portrait cards.

- **Build:** `new SkillButton().Bind(view)`. Attach the game's drag or aim manipulator to it.
- **View model:** `SkillView`: `label`, `icon`, `slot` (A or S), `kind`, `cost`, `state`, `charged`, `seconds`, `need`,
  `queue`, `progress`, `reason`, `description`, `fullDescription`, `cooldownSeconds`.
- **States:** `vb-skill--{state}`. What each state shows:

  | State | Shows |
  | --- | --- |
  | `Ready` | The cost |
  | `Aiming` | AIM, with glow |
  | `Cooldown` | A fill rising from the bottom, with the seconds |
  | `Short` | NEED *n*, dashed |
  | `Silenced` | SIL with the seconds, struck |
  | `Fallen` | OUT, with a lock glyph |
  | `Casting` | CAST |
  | `Disabled` | LOCK |
  | `Queued` | QUEUE #*n*, dashed lavender |
  | `Buffered` | NEXT with the cost |
  | `Windup` | A left-to-right sweep, with the seconds |
  | `Invalid` | A struck no-target glyph and the reason |
  | `Rejected` | One 400 ms crimson `--flash` |
  | `Exposed` | EXPOSED, inverted |

- **USS:** `__charge`, `__sweep`, `__slot`, `__icon`, `__slash`, `__foot`, `__top`, `__value`. While dragged, the
  element carries `vb-dragging` (from `TargetDrag`).
- **Access:** every blocked state reads differently without colour. The tooltip carries the name, cost and, where it
  applies, readiness, reason or "refused". It is focusable.

### QueueChip

One entry of the Power queue.

- **Build:** `new QueueChip().Bind(view, position, preview)`.
- **View model:** `QueueView`: `who`, `skill`, `eta`, `cost`, `actor`, `slot`, `manual`, `next`.
- **API:** `Cancel()` raises `Cancelled` for manual entries only (not previews), and returns whether it did.
- **States:**
  - `--next`: the first entry, or a buffered one marked `›`.
  - `--manual`.
  - `--preview`: dashed; what aiming would add.
- **USS:** `__index`, `__text`, `__skill`, `__who`, `__cost`, `__eta`, `__cancel` (a 44 × 44 ×).
- **Access:** the × is full height and has a tooltip. Autoplay estimates cannot be cancelled and show no ×.

### VbSwitch

A square on/off switch with a caps label and a line saying what it does.

- **Build:** `new VbSwitch("REDUCED MOTION", "No camera shake", value)`.
- **API:** `value`, `SetValueWithoutNotify`, `Text`, `Detail`. Raises `ChangeEvent<bool>`; use `.Changed(v => …)`.
- **States:** `vb-switch--on`.
- **USS:** `__track`, `__knob` (slides in `dur-ui`), `__text`, `__label`, `__detail`.
- **Access:** the whole row is the target, and it is focusable with Enter and Space.

### VbSlider

The engine `Slider` in Veil dress: a cut track, a teal fill, a square thumb and the value as a number.

- **Build:** `new VbSlider("Music", 0.6f, 0, 1)`.
- **API:** as `Slider`; `.Changed(v => …)`.
- **USS:** `vb-slider`, `vb-slider__fill`, `vb-slider__value`.

### VbSegmented

Two to five mutually exclusive peers in one frame. Each can carry its consequence.

- **Build:** `new VbSegmented(new[] { ("LOW", "30 FPS"), ("STANDARD", "60 FPS") }, selected)`.
- **API:** `value` (index), `SetValueWithoutNotify`. Raises `ChangeEvent<int>`.
- **States:** `vb-segmented--fill` (three or more options share the row so they fit 320), `__option--on`.
- **USS:** `__option`, `__label`, `__sub`.
- **Access:** each option is a focusable target with Enter and Space.

---

## Input

### TargetDrag

One-pointer press, drag and release with capture, for aiming and moving.

- **Build:** `new TargetDrag(begin: (pos, id) => bool, move, end, cancel)`. `begin` returning false lets the press
  through.
- **States:** the target carries `vb-dragging` meanwhile.
- **Notes:** the left button or a touch only. Lost capture runs `cancel`. Hosts cancel drags when a page opens.

### ReorderManipulator

Reorders a container's children by dragging, long-pressing or the keyboard; the host owns the state.

- **Build:** `container.AddManipulator(new ReorderManipulator { CanLift = i => … })`, then `Reordered += (from, to) => …` and bind
  again. `SquadView.SlotMoved(from, to)` and `SquadView.Reorder` are the squad's.
- **Gesture:** a press on a child that may move lifts after 8 units of travel or 0.35 s held still. The child dims
  (`vb-reorder-source`), a ghost plate with a selection ring (`vb-reorder-ghost`) follows the pointer over the panel root and the
  slot under it shows a ring (`vb-reorder-over`). The drop slot is the child whose `worldBound` holds the pointer, else the
  nearest. A tap never lifts. The ghost lifts when it appears (`MotionRecipes.DragLift`: it rises 4 units and grows to 1.05 with a brief fade-in flash) and glides into the slot as it settles back to full size (`DropSettle`, 220 ms), then
  the slot pops once; under reduced motion it stays flat, and reduced flashes drops the fade-in. Reduced motion drops the ghost at once on release.
- **Keyboard:** Alt+Left/Up and Alt+Right/Down move the focused child one place and keep focus on it.
- **API:** `Begin`, `Drag`, `End`, `Cancel`, `Nudge(index, step)`, `Lifted`, `Over`, `Ghost`, `ReorderManipulator.IndexAt(rects, point)`.

### GestureSurface

Camera gestures on a surface that also hosts a targeting drag.

- **Build:** `new GestureSurface(busy: () => aiming)`. Register it after the targeting manipulator on the same
  element.
- **Events:**

  | Event | Raised by |
  | --- | --- |
  | `Panned(from, to)` | One finger or the left button on empty ground, two fingers, or a middle drag |
  | `Pinched(ratio)` | A two-finger pinch; above 1 when spreading |
  | `Twisted(degrees)` | A two-finger twist |
  | `Orbited(delta)` | A right drag |
  | `Scrolled(delta)` | The mouse wheel |
  | `Hovered(position?)` | A hovering mouse; `null` when it leaves |
  | `DoubleTapped(position)` | Two taps within 0.35 s and 40 units |

- **API:** `Press`, `Drag`, `Lift` and `Scroll` (for tests and custom input), `Cancel()`, `Count`. Constants `Slop` (8),
  `TapSeconds`, `DoubleTapSeconds`, `DoubleTapReach`.
- **Notes:** a pointer the targeting drag captured is never a camera gesture. While `busy` is true every pointer is
  ignored. Positions are panel units, with y down.

---

## HUD composites

### HudTop

The translucent portrait HUD band.

- **Build:** `new HudTop()`. Subscribe to `AutoClicked`, `SpeedClicked`, `PauseClicked` and `QueueCancel`.
- **API:** `Bind(HudView)`, `ShowFocus`, `Queue` (the resources popover).
- **View model:** `HudView`: `speed`, `wave`, `score`, `boss`, `auto`, `slow`, `power`, `maxPower`, `regen`, `hint`,
  `focus`, `previewFrom`, `previewTo`, `queue`, `queueTitle`, `queueHint`.
- **States:** `vb-hud--boss` (the label reads BOSS), `__auto--on`.
- **USS:** `__row`, `__stats`, `__stat`, `__key`, `__wave`, `__score`, `__auto`, `__auto-text`, `__auto-label`,
  `__auto-state`, `__speed`, `__pause`, `__resources`. Compact boards tighten it.
- **Access:**
  - The auto, speed and pause toggles are 44-unit-high targets. Auto reads AUTO ON or OFF in words; speed and pause
    have tooltips.
  - The resources panel is focusable and opens the full `QueueList`.
  - The score's tooltip carries the exact number.

### QueueList

Upcoming casts in firing order; popover content.

- **API:** `Bind(queue, title, hint, cancel)`, and `CancelRow(index)` (what a tap does).
- **States:** `__row--next`, `__row--manual`.
- **USS:** `vb-queue`, `__head`, `__title`, `__hint`, `__row`, `__index`, `__text`, `__skill`, `__who`, `__cost`,
  `__eta`, `__cancel`.
- **Access:** manual rows are buttons ("Cancel queued cast"); autoplay estimates are not.

### BossBar

The boss read-out.

- **API:** `Bind(BossView)`.
- **View model:** `BossView`: `name`, `health`, `stability`, `detail`, `shielded`, `shieldDetail`, `interruptible`.
- **USS:** `vb-boss`, `__head`, `__icon`, `__name`, `__detail`, `__health`, `__chips`. A broken stability bar shows as
  exposed.
- **Access:** the shield and interrupt states are `StatusChip`s with words.

### ScreenHeader

Back, title and a short meta read-out at the top of a page.

- **Build:** `new ScreenHeader("SETTINGS", "SAVED", backLabel)`. Subscribe to `Back`.
- **API:** `Title`, `Meta`, `Accent(on)`.
- **USS:** `vb-header`, `__back` (44 units, named `back`), `__title`, `__meta`, `__meta--accent`. Add `vb-page--headed`
  to the page (the arcade's `Header()` helper does).

### ScoreRow

One high-score line.

- **API:** `Bind(ScoreView)`.
- **View model:** `ScoreView`: `rank`, `score`, `wave`, `bosses`, `squad`, `current`.
- **States:** `vb-score--current` (framed, with THIS RUN), `__rank--top` (ranks 1–3).
- **USS:** `__rank`, `__text`, `__line`, `__value`, `__badge`, `__detail`, `__squad`, `__code`, `__code-text`.

---

## Operatives

### UnitCard (was `OperativeCard`)

The portrait battle card. It carries the name only (no code over it), up to three timed statuses as icons on the portrait, and two skills.

- **API:** `Bind(OperativeView)`; `Skill(0 | 1)` returns the A and S `SkillButton`s for the game's manipulators; `Statuses` is the `StatusRow` above the card; `Damage(strength)` plays the damage edge (`AutoDamage`, on by default, plays it when the bound hit points fall).
- **Falling:** a card that falls while it is on a panel slumps (`MotionRecipes.Fallen`: 6 units down and 0.97 of its size, back to rest in 420 ms, `VbMotion.Slump`) as its band, kit stripe and body drain to the fallen look and the damage flash plays. Reduced motion
  drains the colour only; reduced flashes drops the flash fill. Binding a card that is already fallen again does not move it.
- **View model:** `OperativeView`: `id`, `name`, `code`, `role`, `roleIcon`, `kit`, `portrait`, `hp`, `maxHp`, `barrier`, `armor`, `stability`,
  `maxStability`, `effects`, `mode`, `manualSeconds`, `canRevive`, `skills`, `passive…`.
- **States:** `vb-operative--auto`, `--manual`, `--aiming` (brackets and glow), `--fallen` (one FALLEN, a greyed portrait, the name struck through; "Revive via perk"
  only while `canRevive`). The mode chip shows only for a non-default mode (MANUAL with seconds, AIMING), truncates inside the card and is hidden for auto and fallen:
  the HUD toggle states autoplay once.
- **Statuses:** `StatusRow` is a row of tiny badge squares (about 20 × 20, at most six and a "+n") above the card for the effects that have a time left (`EffectInstance.Timed`);
  a passive or a perk is not a status and never shows. The row is one 44-unit target and hover, focus or tap opens `EffectList` vertically in a popover. A unit with no timed status shows nothing.
- **Clipping:** the portrait is a `VbClipImage` and the card's last skill (`vb-operative__skill-end`) takes the card's cut, so nothing overflows the top-left cutout or the bottom-right corner.
- **USS:** `__band`, `__portrait`, `__shade`, `__chip`, `__kit`, `__body`, `__name`, `__numbers`, `__hp`, `__barrier`, `__skills`, `__fallen`, `__fallen-title`, `__fallen-hint`;
  `vb-pips`, `__icon--harm`, `__icon--benefit`. (The class names keep the `vb-operative` prefix.)
- **Access:** the mode chip is a word; statuses are glyphs with a tooltip and a list; OUT when fallen.

### UnitTile (was `OperativeTile`)

A roster tile.

- **API:** `Bind(view, slot, focused, RarityTier tier, string locked)`. Subscribe to `Picked`.
- **States:** `vb-tile--in` (shows its squad slot), `--focused` (brackets and glow), `--locked`, `--blue`, `--violet`, `--gold`.
- **USS:** `__band`, `__portrait`, `__shade`, `__order`, `__kit`, `__body`, `__name`, `__role`, `__role-text`. On a narrow board (`vb-compact`, under 380 units) the role is its icon alone;
  a locked tile keeps its reason as text.
- **Access:**
  - The tooltip reads name, role and slot, or "locked: Pull in rifts".
  - A locked tile stays visible and says how to get the operative.
  - Tier colour comes with a frame weight.

### UnitSlot (was `SquadSlot`)

One of the four squad slots: the name alone (the code is not repeated), or an empty dashed slot.

- **API:** `Bind(view or null, slot)`. Subscribe to `Picked`.
- **States:** `vb-slot--empty` (dashed, + and SLOT *n*).
- **USS:** `__band`, `__portrait`, `__order`, `__kit`, `__body`, `__name`, `__empty`, `__empty-label`.

### UnitDetail (was `OperativeDetails`)

The unit sheet.

- **API:** `Bind(view, slot, squadFull, maxHp, maxArmor, maxStability, showStability)`. Subscribe to `Closed` and `Toggled`.
- **Shows:**
  - Health and armor bars, scaled to the roster maximums, and stability when `showStability` (`SquadView` passes `SquadView.StabilityVaries(roster)`: hidden while every operative reads the same).
  - One list of `SkillSummary` rows: the passive (`passive`, `passiveText`, `passiveIcon`), the basic attack (`OperativeView.basic`, `slot="B"`, `kind="Basic"`) and both skills, with cost and cooldown only where there is one; emphasis marks (`*Taxed*`) render as emphasis through `Markup`. `UnitDetail.Rows(view)` is that list.
  - A LORE prompt after the skills (`OperativeView.lore`; hidden when empty).
  - ADD TO SQUAD or REMOVE FROM SQUAD (danger). When the squad is full, adding is blocked with "Squad full · remove one first".
- **USS:** `vb-details`, `__head`, `__kit`, `__name`, `__role`, `__slot`, `__slot--in`, `__close` (a plain icon button, 44 units), `__stats`,
  `__skills`, `__lore-box`, `__lore`, `__skill`, `__skill-icon`, `__skill-name`, `__skill-desc`, `__skill-cost`, `__toggle`.

### UnitStats and ResultSummary (were `OperativeStats` and `SquadResult`)

The post-run contribution.

- `ResultSummary.Bind(ContributionView)` is the results cell: the name and, when the unit fell, FELL Wn (a unit still standing says nothing). Hover, focus or tap opens `UnitStats` above it (`Details`).
- **View model:** `ContributionView`: `operative`, `fellWave`, `damage`, `kills`, `casts`, `power`, `share`, `extraLabel`, `extraValue`.
- **States:** `vb-result--fallen`. On the stats: `__fell--alive`.
- **USS:**
  - `vb-contribution__head`, `__portrait`, `__name`, `__fell`, `__share`, `__share-fill`, `__grid`, `__cell`, `__key`, `__value`.
  - `vb-result__name`, `__fell`, `__kit`.

### The renamed names

0.8.0 renames the unit and choice composites to generic names, in place (the files moved with `git mv`, so every `.meta` GUID is unchanged). Each old name stays for one release as an `[Obsolete]`
type that derives from the new one, so a game that still says `OperativeCard` compiles (with a warning) and behaves the same. Remove the forwards in the release after the games adopt the new names.

| Old | New |
| --- | --- |
| `OperativeCard` | `UnitCard` |
| `OperativeTile` | `UnitTile` |
| `SquadSlot` | `UnitSlot` |
| `OperativeDetails` | `UnitDetail` |
| `OperativeStats` | `UnitStats` |
| `SquadResult` | `ResultSummary` |
| `PerkCard`, the road signs and the `RouteChoice` items | `ChoiceCard` |
| `PerkChip` | `ChipRow` |
| `CreditsChip` | `ChipRow.Credits()` (`ChipRow` with `SetNumber`) |
| the merchant's `OfferRow`, `SaleRow`, `HealthRow` and the perk detail rows | `StatRow` |

`PerkTier` moved out of `Core/Models.cs` into `Composites/Perks.cs`, next to the perk composites that read it (same namespace and assembly: nothing that used it changes).

### SkillButton

One layout in every state (44 × 56, or 44 high in a stacked compact card): the slot letter, the glyph and one cost line. The states are overlays, so nothing runs below the slot's foot at any width.

- **Overlays:** a cooldown fills from the bottom and counts its seconds over the glyph (`Seconds`; a windup and a silence count the same way); short-on-Power dims the slot and puts the shortfall in
  the corner (`Badge`, "−3"); a queued manual cast puts its place there ("#1") and a buffered one an arrow; a fallen operative's skills lock; an invalid aim strikes the glyph and the cost line
  becomes the reason; a refused command flashes crimson once; an exposed target inverts the cost.
- **API:** `Bind(SkillView)`, `Badge`, `Value`, `Seconds`. **USS:** `vb-skill--{state}`, `__slot`, `__icon`, `__seconds`, `__badge`, `__value`, `__charge`, `__sweep`, `__slash`.
- **Unaffordable** is a dimmed fill, never a dashed frame.
- **Press:** the button sinks under a press (`MotionRecipes.PressSink`: 1 unit down and 0.92 of its size in 100 ms, eased out, no bounce) and lets go when the press ends anywhere on the panel, even if the game's drag manipulator holds the pointer; its plate
  brightens while pressed. Reduced motion does not move it and dims it a little instead; reduced flashes keeps the sink and not the brightening. A fallen or locked skill does not sink. `Pressed` tells.

### ChoiceCard

One option the player takes by tapping it: a road, an extract choice, a perk. The whole card is the button and keeps its own edge. It stacks a short tag, a centred glyph, a title, a few short facts, rank cells (one
`ProgressBar` of segments: a road's threat, a perk's levels), the value after the pick and the action word at the foot. A slight tint of its tone fills it (`Tone`: Ally, Enemy, Power).

- **Sides:** `ChoiceSide.Left` cuts its top-left and bottom-left corners and aligns its text right; `Right` cuts its top-right and bottom-right corners and aligns its text left; `Single` keeps the regular
  cuts. A pair uses Left and Right (the regular top-left and bottom-right cut is not used for a pair). The cut sides come from `--vb-cut-side` (`VbShape.Left`, `Right`).
- **Compact:** the same card as a cell of a two-column grid (the active perks, the merchant): `Compact` drops the tag, shrinks the glyph and lets it be as tall as its content. `BindOwned(PerkView, selected)` shows a perk the run owns (its glyph, name,
  active value and levels), `BindOffer(MerchantOfferView, index, credits)` a merchant's offer (BUY with the price, SOLD, or the game's reason; dimmed and disabled when it cannot be bought, with no second price) and `BindSale(MerchantStackView)` a stack the
  merchant buys (SELL +payout). `Dimmed` greys a card that cannot be taken; `Command` names what picking it does (`buy-0`, `sell-perk-<id>`); `Pick()` picks it with no pointer.
- **API:** `Bind(RoadView road, string action)`, `Bind(PerkView, selected)`, `Side`, `Tone`, `Tag`, `Icon`, `Title`, `Text` (emphasis marks render), `Next`, `ActionText`, `Ranks(filled, cells, next, tone)`, `Picked`.
  `RoadView.rank` and `ranks` (default 5) are the threat; 0 hides the cells. `RouteChoice` builds a left and a right card, named `route-left` and `route-right`.
- **USS:** `vb-choice`, `--left`, `--right`, `--ally`, `--enemy`, `--power`, `--selected`, `--compact`, `--dimmed`, `__tag`, `__glyph`, `__title`, `__text`, `__rank`, `__next`, `__action`. The text is compact: USS has no line height, so the size does the work.
  A word is never broken inside: the title steps down on a compact board.

### StatRow

One fact on one row: a key that keeps its width, a value that wraps beside it, an optional glyph before them and a trailing action whose button is 44 units tall. It is a `KeyValueRow`, so everything that reads
one reads this (the perk detail rows, the merchant's offers, sales and health trades).

- **API:** `Key`, `Value`, `Good`, `Icon`, `Stacked` (the value under the key), `Dimmed` (a row that cannot be acted on, with no second price), `Plated` (one tone above its parent), `Trail`, `WithAction`, `Bar`, `Content`.
- **USS:** `vb-stat-row`, `--stacked`, `--dimmed`, `--plated`, `__icon`, `__text`, `__trail`, `__bar`.

### ChipRow

A glyph, a word and a value on one plate with no edge. A perk chip is tinted by its rarity (`vb-perk--{tier}`); where the chip is a target (the results page's perks, whose hover, focus or tap shows `PerkDetail`) its
hit area is 44 units tall with the 30-unit plate inside it. A chip that only reads (`ChipRow.Credits()`, the HUD's credits) is `Static` and as tall as its plate.

- **API:** `new ChipRow(alignEnd)`, `Bind(PerkView)`, `Details`, `Text`, `Icon`, `Value`, `Static`, `Plate`, `SetNumber(n)`. **USS:** `vb-chip-row`, `--static`, `__plate`, `__name`, `__count`, `__popover`.

### StatusPips and the effect detail

`StatusPips` is the card's strip of timed statuses (see `UnitCard`). `EffectDetails` prints only what is known: the title (caps, in the body face), the description, the source, the magnitude and the time
left; nothing stands in for a missing value, and a plain active effect has no state line. `EffectList` rows read "−20% · 2.5s". The popover title uses its own caps text style, which `FontPrimer` primes at its
size with the other title styles (`FontPrimer.Styles`), so its tracking does not depend on what was drawn first.

### Markup

`Markup.Rich(text)` turns `*word*` (or `**word**`) into a colour run and `Markup.Plain(text)` drops the marks (for tooltips). Every description label uses it (the passive, a skill, an effect, a perk, a choice card). A stray
or spaced asterisk ("5 * 3") stays, and text with no asterisk is returned as it came.

---

## Perks

`PerkView`: `id`, `name`, `category`, `tag`, `icon`, `effect`, `now`, `next`, `tier` (General, Operative, Rare), `art`,
`stack`, `max`, `waves`. Every perk part carries `vb-perk--{tier}`. Operative perks show their operative, and others
show a ringed glyph.

| Component | Use | API | USS hooks |
| --- | --- | --- | --- |
| `ChoiceCard` (was `PerkCard`) | One of three draft cards: tier tag, glyph, name, effect, level cells and the value after the pick (shown once: not when it repeats the effect) | `Bind(view, selected)`, `Picked` | `vb-choice`, `--selected` (brackets, glow); `__tag`, `__glyph`, `__title`, `__text`, `__rank`, `__next` |
| `PerkDetail` | Full explanation: draft (Now, when there is a value, then After pick) or owned (Active, stacks, waves picked); each fact is a `StatRow` | `Bind(view, draft)` | `__head`, `__thumb`, `__name`, `__tag`, `__effect`, `__rows`, `__row`, `__key`, `__value`, `__value--good` |
| `ChipRow` (was `PerkChip`) | Compact owned perk; hover, focus or tap shows `PerkDetail`; 44-unit hit area, tinted plate, no edge | `new ChipRow(alignEnd)`, `Bind(view)`, `Details` | `vb-chip-row`, `__plate`, `__name`, `__count`, `__popover` |
| `PerkRow` | A perk as a list row (kept; `ActivePerksView` shows the active perks as compact `ChoiceCard`s) | `Bind(view, selected)`, `Picked` | `--selected`; `__thumb`, `__text`, `__line`, `__name`, `__tag`, `__effect`, `__right`, `__value`, `__stacks` |

Stack wording: `STACK 2 / 3` in a draft, `×2 OF 3` when owned, `RARE DRAW` and `REPEATABLE` otherwise.

---

## Pages and dialogs

### Dialog

Confirmation sheets.

- **API:**
  - `Dialog.Sheet(parent, title, body, bodyClass, classes…)` returns a solid `vb-dialog` panel; add the buttons to it.
  - `Dialog.Push(navigator, title, body)` puts one on a new modal layer.
- **USS:** `vb-dialog`, `__title`, `__body` (in `Layout.uss`). Secondary and danger buttons in a dialog keep their
  48-unit height and align left.
- **Access:** the modal layer blurs and disables what is under it, and returns focus on close. Back closes it first.

### Backdrop

Full-bleed artwork behind a screen, scaled to cover, with a gradient shade along the bottom.

- **Build:** `new Backdrop(texture)`.
- **USS:** `vb-backdrop`, `__shade`.

### Layout classes (`Layout.uss`)

| Class | Use |
| --- | --- |
| `vb-root` | Full-size column root |
| `vb-grow` | Takes free space |
| `vb-row`, `--between`, `--wrap` | Rows |
| `vb-half` | Half-width cell |
| `vb-grid4` | Four tiles or slots per row (wraps) |
| `vb-grid2`, `__cell`, `--end` | Two-column grid |
| `vb-section` | A titled settings panel |
| `vb-action-row`, `--stacked`, `--fixed` | `ActionRow`: buttons that fill the row in equal shares, stacked below 112 units each; `--fixed` keeps one width (92) |
| `vb-nest` | A surface inside another surface: no edge, one tone up |
| `vb-large-text` | Larger body and caption type |

---

## Holographic menu surfaces

| Component | What it is | API | USS hooks |
| --- | --- | --- | --- |
| `ScanLines` | Horizontal lines over a holo surface, one mesh; colour and pitch from `--vb-scan` and `--vb-scan-pitch`; hidden under `vb-contrast` | `new ScanLines()` | `vb-scanlines` |
| `HoloFrame` | A diegetic panel: cut frame with cyan brackets, scanlines, a chromatic caps title and a flicker-in; menus only, one in focus | `new HoloFrame(title)`, `Title`, `FlickerIn()`; children go in its body | `vb-holo`, `__head`, `__title`, `__title--cyan`, `__title--pink`, `__body` |
| `HoloPlatform` | The 2D stand-in for a lineup plinth | `Bind(name, detail, PlatformState, WorldTeam, showMarker)`, `State`, `Picked` | `vb-platform--occupied`, `--selected`, `--open`, `--ally`, `--enemy`; `__marker`, `__tag`, `__label`, `__sub`, `__plate` |
| `RevealCard` | A rift result: a holographic back that flips to a tier-framed front | `Bind(tier, title, detail, item, isNew, dupe, flipped, tierLabel)`, `Flip()`, `Flipped`, `Flipping`, `RevealCard.TierWord(tier, item)` | `vb-reveal--blue`, `--violet`, `--gold`, `--flipped`; `__back`, `__front`, `__tier`, `__name`, `__line`, `__badge`, `__badge--dupe` |
| `RuleRow` | One IF … THEN automation rule, with a toggle and a note on when it can run | `Bind(RuleView)`, `Toggle()`, `Toggled`, `View` | `vb-rule--off`, `--warning`, `--draft`; `__index`, `__body`, `__if`, `__then`, `__keyword`, `__join`, `__text`, `__value`, `__action`, `__cost`, `__note`, `__toggle` (44 × 44), `__toggle--on`, `__knob` |

More detail on each:

- **`RevealCard` tier words** travel with the colour: RARE, EPIC and LEGENDARY, or MAGIC ITEM, RARE ITEM and UNIQUE
  ITEM for items. A duplicate shows its resonance gain in place of NEW.
- **`RuleView`:** `index`, `conditions` (`RuleCondition`: `text`, a highlighted `value`, `after`), `any` (OR rather
  than AND), `action`, `actionRest`, `cost`, `enabled`, `warning`, `draft`, `note`.
- **Access:**
  - Holo motion follows reduced motion and reduced flashes.
  - The rule toggle has an on/off tooltip.
  - A card flips on tap or when focused with Enter or Space.

---

## World layer

### WorldLayer

The world-space HUD over the 3D view; see [world space](guidelines/02-world-space.md).

- **API:**
  - Per frame: `BeginFrame()`; then `Bar(id, world, health01, barrier01, team, rank)`, `BarAt(id, point, …)`,
    `Label(id, world, kind, title, detail, team, group)`, `Marker(id, world, kind, size)` and
    `Offscreen(id, world, name, team, metres, threat)`; then `EndFrame(simulationDelta)`.
  - Event-driven: `Text(target, world, amount, kind)`.
  - Static: `WorldLayer.Pin(point, size, insets, edge, out pinned, out degrees)`.
  - Counters: `Bars`, `Labels`, `Markers`, `Chips`, `LiveTexts`, `Size`.
- **Settings:** `Project`, `TextLimit` (40), `PerTarget` (3), `MergeSeconds` (0.2), `TextSeconds` (1.2), `Rise` (24),
  `StackStep` (14), `BarLift` (27), `TextLift` (40), `LabelLift` (48), `Edge` (10), `Insets`, `ReducedMotion`.
- **USS:** `vb-world` (`World.uss`). Add `vb-covered` to hide it under a page.

### WorldHealthBar

A team bar with a ghost and barrier.

- **API:** `Set(health01, barrier01)`, `Step(delta)`, `Team`, `Rank`, `Tint(color?)`, `Health`, `Ghost`, `GhostHold`,
  `GhostRate`.
- **States:** `vb-world-health--ally`, `--enemy`, `--normal`, `--elite` (44 wide), `--boss` (56 wide).
- **USS:** `__ghost`, `__fill`, `__barrier`.

### CombatText

A `Label` that the layer pools.

- **API:** `Bind(kind, amount)`, `CombatText.Format(kind, amount)`.
- **States:** `vb-combat-text--{kind}`. The kinds and their wording are in
  [world space](guidelines/02-world-space.md#combat-text).

### WorldLabel

- **API:** `Bind(kind, title, detail, team)`, `ShowDetail(on)`, `Title`, `Detail`, `WorldLabel.Merged(title, count)`.
- **Merge and split:** when the count of a merged label changes it swells once (members joined, `MotionRecipes.LabelMerge`, 1.12) or shrinks once (members left, `LabelSplit`, 0.9) over 180 ms with a brief dip in opacity, on simulation time (`EndFrame(delta)`).
  Reduced motion keeps the dip and not the scale; reduced flashes (`ReducedFlashes`, or the class on a parent) keeps the scale and not the dip. `WorldLayer.LabelPose` is the pose.
- **Merging:** `WorldLayer.Label(…, group)` merges labels with the same group key and title into one "TETHERED ×3" at the
  centroid of their points; a lone one reads as itself.
- **States:** `vb-world-label--aim`, `--name`, `--telegraph`, `--estimate`, `--blocked` (prefixed with ×), `--elite` (one line:
  a diamond and the detail, so "ELITE" and "Voltaic" read "◆ Voltaic") and `--ally` or `--enemy`. Labels are unbordered text on a
  dark underlay; ownership reads from the text colour.
- **USS:** `__title`, `__detail`, `__mark`.

### WorldMarker

- **API:** `Bind(kind, size)`, `Kind`.
- **States:** `vb-world-marker--ally` (circle), `--enemy` (diamond), `--selected`, `--queued`, `--move`.
- **USS:** `__glyph`.

### OffscreenIndicator

- **API:** `Bind(name, team, metres, threat)`, `Point(degrees)`, `Name`, `Angle`.
- **States:** `vb-offscreen--ally`, `--enemy`, `--threat`.
- **USS:** `__arrow` (a chevron rotated toward the fighter), `__name`, `__distance`.

### EdgeWarning

- **API:** `Bind(caption, top)`, `Set(urgency, time, reducedMotion)`, `Urgency`, `Caption`.
- **States:** `vb-edge-warning--top` (docks to the top edge, chevrons point up), `--hot` (urgency above two thirds).
- **USS:** `__row`, `__chevron` (five chevrons pointing over the edge; they ripple outward unless reduced motion), `__caption`.
- **Use:** a threat about to cross a board edge, such as hostiles inside the breach zone. Urgency 0 hides the band.

## Projection and the road economy

| Component | Input | Events / behaviour |
| --- | --- | --- |
| `GlitchText` | Ordinary label `text` and USS typography | Eight clipped bands, red/cyan channels. `Glitch.In/Out/Pulse/Slam`; schedules cancel on replacement/detach. |
| `Announcer` | `Say(text, alert, milliseconds, interrupt)` | Bounded six-notice queue; critical pre-emption; out before replacement, in then hold then out. Dispose with its host. |
| `RouteChoice` | `RouteView` with wave and two `RoadView`s, shown as a mirrored pair of `ChoiceCard`s (left cut and aligned left, right cut and aligned right; threat as rank cells) | `Chosen(side)` once, left 0 / right 1. Whole-card targets; keyboard Enter/Space. `Choose` supports alternate input. |
| `MerchantScreen` | `MerchantView` with credits, offers, sellable stacks and HP consequences, offers and sales are compact `ChoiceCard`s in a two-column grid (the whole card is the button: BUY 80, SELL +30), health trades are `StatRow`s with a 44-unit danger action; an offer that cannot be bought is dimmed and out of play with no second price, an empty section hides | `Bought(index)`, `PerkSold(id)`, `HealthSold(actor)`, `Left`. Host validates and rebinds after each transaction. `Offer(index)` and `Sale(perkId)` find a card by its command (`buy-0`, `sell-perk-<id>`); a trade's button is `sell-hp-<actor>`. `ScrollOffset` preserves position. |
| `ChipRow.Credits()` (was `CreditsChip`) | `SetNumber(credits)` | Currency icon and integer; formats on change; read only. |

`PerkTier.Rare` is cyan; `Epic` is violet with a short shimmer. Both have explicit text labels.
Merchant offer states say why a purchase is blocked (credits, sold, stack cap). Health rows state the before/after HP;
fighters that cannot retain 1 HP cannot trade. Instant rewards are excluded from sellable stacks by the host.
`HudTop` places pause and the autoplay toggle at the top left. The toggle reads AUTO or MANUAL (it is the one place autoplay is stated), keeps its state tint and tooltip; speed stays
on the right, credits sit beside Power. While an aim is live its hint ("Needle Storm · −18 · replaces auto") is the first line of the cancel band, above RELEASE TO CANCEL. `WaveSlammed` lets the host request its own bounded camera jolt; the GUI has
no dependency on a camera or arcade simulation. The host must use actual safe-area insets, with a small battle fallback.

## GUI polish (gui 0.9.0, design-system#55)

### Type variants and the fluid scale

Three variants carry every run of text. **Body** is prose, slightly dimmed (`vb-t-body`, `vb-t-body-sm`, `vb-t-caption`; `--vb-font-size-body` 13). **Label** is the small tracked face of buttons (the primary included), badges, chips, eyebrows and subheadings:
0.88× body (11.5), 0.15em tracking, `vb-t-label-sm` (10, = `vb-t-badge`), `vb-t-label` and `vb-t-label-lg` (13.5). **Heading** is bold with negative tracking and a 0.9 line: `vb-t-heading-sm` (13), `vb-t-heading` (20, = `vb-t-title`)
and `vb-t-heading-lg` (32, = `vb-t-display`). `Gui.uss` lists each component's text class in its variant's block, so a size changes in one place; `TypeStyle` has a member for each (`LabelSize.Large` for eyebrows).
UI Toolkit has no `line-height`: a top-level heading trims its leading with a negative margin (`--vb-trim-*`); a heading that wraps keeps the font's own leading. letter-spacing is in hundredths of an em.

Every `font-size` in the sheets reads a token, and the **fluid scale** moves them with the panel's width: `Fluid.uss` (generated by `Tools~/fluid.py` from `Tokens~/fluid.json`) has one class per step, `vb-fluid-320` to `vb-fluid-600`, that overrides the type sizes,
the spacing and layout tokens (`--vb-gutter` 12, `--vb-pad` 16, `--vb-gap` 8, `--vb-margin` 24) and the size ladder (`--vb-fs-<n>`) with value × factor (0.92 at 320, 1 at 390, 1.06 at 430, 1.18 from 600).
`VbFluid.Apply(root, width)` sets the class (`SafeAreaRoot.Flags` does it from the panel's width); `VbFluid.Factor(width)` and `ClassFor(width)` are the pure functions. Popovers copy the class from their anchor.

### ActionRow.Single and the icon-only button

`ActionRow.Single` keeps its buttons on one line at any width: they share the row, drop their trailing glyph and may wrap their label, and an icon-only child (`VbButton.IconOnly`, `IconButtonComposite`) keeps its 48-unit square. `VbButton.IconOnly` shows
the glyph alone; the title is its accessible label (the tooltip). The menu is HIGH SCORES and a settings cog; the pause sheet is RESUME, then EXIT TO MENU, ABANDON and the cog in one row.

### VbEdgeFx

The animated border of the MONO wireframe kit on any shaped element: `VbEdgeFx.Attach(e)` plays it on click or submit (`VbButton` and `Clickable` attach it); `VbEdgeFx.Play(e, tone)`, `VbEdgeFx.Damage(e, strength)` and `UnitCard.Damage` play it by hand. **Activate** (520 ms):
two heads from opposite points of the cut outline run clockwise with fading tails and a soft glow in the element's edge colours, then an afterglow fades. **Damage** (560 ms): a translucent red flash and a red border glow. Reduced motion lights the whole edge and fades it in 100 ms;
reduced flashes drop the flash fill. The overlay is one `vb-edgefx` child, added when it plays and removed when it ends; `Active`, `Playing` and `Cancel` read and end it. Real time (`VbMotion.Clock`).

### VbClipFill and VbClipImage

`overflow: hidden` clips to a rectangle, so art that reaches a chamfer used to overflow it. `VbClipFill` (a solid block; its colour is `--vb-fill`) and `VbClipImage` (a picture scaled to fit; set `Image` or `Ui.Image`) paint themselves as polygons clipped to the outline of the nearest
`VbShape` ancestor (`VbClip.Polygon`, `VbClip.Intersect`). The unit card's portrait, the tile's and slot's picture and a skill's charge and sweep use them.

### StatusRow

`StatusRow` shows the timed statuses of a unit as `StatusSquare`s (about 20 × 20 `BadgeComponent`s: the glyph alone, harm or benefit in the tone, the stack count in the corner; at most six, then a "+n"). The whole row is a 44-unit target; hover, focus or tap opens one vertical
`EffectList` in `Details`. `Bind(EffectInstance[])` reuses the squares. `UnitCard.Statuses` is one.

### Carousel

Cards in an endless row: the active one takes 64% of the width and sits on top, its neighbours are tucked beneath it and tilted (two a side from five cards), and the row changes by swipe, a tap on a card beneath, the arrow keys or focus; it glides (320 ms, real time) and jumps under reduced motion.
`Bind(values, key, create, bind, selected)` keeps one element per key; `Select(i)` is the host's own move; `Changed(i)` reports the player's. `Index`, `Slot` (the continuous position), `Count`, `Depth`, `Active`, `Next()`, `Previous()`, and the pure `Offset`, `Pose` and `Shortest` for tests.
Any element is a card; `PerkDraftView` uses `ChoiceCard`s, which already carry the perk's effect and next value, so the draft shows no second copy of the active card.

### Overlay transitions and the title reveal

One recipe family: `MotionRecipes.OverlayIn` (220 ms) and `OverlayOut` (150), `PopoverIn` (140) and `PopoverOut` (90), `DrawerIn` (320) and `DrawerOut` (220). `OverlayView.Entrance` and `Exit` default to the overlay pair; the modal layer fades and its sheet rises; `AutoPresent` lets a pause sheet or dialog
present itself on attach. `Glitch.Reveal(e, ms = 2000)` decodes a title or eyebrow out of noise (4-unit sideways jitter, coarse opacity steps, bands and a chromatic split on a `GlitchText`) and ends on the real text at rest; elements with the class `vb-reveal-title` reveal when their overlay
or screen is presented (`Glitch.RevealTitles`), and `Announcer.RevealMilliseconds` gives heads-up notices the same recipe. Reduced motion or flashes: the whole text with a 300 ms fade.

## Text, labels, badges, tiles and cards

### TextComponent and LabelComponent

`TextComponent : Label` carries one type style (`TypeStyle.Display`, `Title`, `Label`, `Badge`, `Body`, `Caption`, `Num`, `Heading`, `HeadingSm`, `HeadingLg`, `LabelSm`, `LabelLg`, `BodySm`
→ `vb-t-*`; the three variants are described under GUI polish) and a tone (`TextTone.Ally`, `Enemy`, `Power`, `Muted` → `vb-t-ally` …; `Default` adds nothing). UXML:
`<vb:TextComponent type-style="Caption" tone="Muted" text="…"/>`. `LabelComponent` is the caps eyebrow: it uppercases its
text, takes `LabelSize.Label` or `Badge`, and adds `vb-label` (4 units under it). `Ui.Text(parent, text, TypeStyle, tone)`
and `Ui.Eyebrow(parent, text)` build them.

### BadgeComponent

A slim chip: optional glyph (`vb-badge__icon`), word (`__text`, uppercased) and value (`__value`), tone
`BadgeTone.Neutral | Ally | Enemy | Power | Warn | Inverted` as `vb-badge--{tone}`. `Set(value)` only touches the label
when the value changed. `Badge.Of(parent, …)`, and the presets `Badge.Status(kind, …)` (a `StatusChip`) and
`Badge.Credits(n)` (a `ChipRow`).

### TileComponent

The pickable cell `UnitTile` and `UnitSlot` extend: band (`__portrait`, `__shade`, `__glyph`, `__order`), `__kit`,
`__body` (`__name`, `__role` with its glyph and `__role-text`) and a lazily built `__empty`. `State` is `TileState` flags
(`In | Focused`, or `Locked`, or `Empty`) as `--in`, `--focused`, `--locked`, `--empty`; `Tier` adds the rarity frame.
`Set(title, caption, picture, glyph, state, order, captionIcon)` fills it; `Picked` fires on tap, click, Enter or Space.

### CardComponent

`vb-card`: `Media` (hidden until it has children), head (`__title` LabelComponent, `__meta`), `Body` (the content
container: `Add` and UXML children land here) and `Foot` (right-aligned actions, hidden while empty; `Actions(...)`).
`Selected` adds `vb-card--selected` (edge, brackets, cut 10).

### IconButtonComposite

A 44 × 44 chipped square with one glyph (`Icon`) or a short state (`Text`, "2×"), its tooltip as the accessible name,
`Active` (`--active`, the toggled look) and `Quiet` (`--quiet`, translucent over the world). `Clicked` fires on pointer,
Enter and Space.

## Bottom drawer

`DrawerOverlay` (`vb-drawer`): `__backdrop`, then `Sheet` (`__sheet`, slim solid, at most 88% of the board) with `__grip`
(`__handle`), `__head` (`__title`, `__meta`, `__close`), `Body` (`__body`, a ScrollView and the content container) and
`Foot` (`__foot`, an `ActionRow`; `Actions(...)`). A Fit drawer that is open and whose content changes height eases its sheet to the new height over `MotionRecipes.DrawerFit` (180 ms, real time) with
`vb-drawer--fitting` on (the sheet's USS height transition is off meanwhile); reduced motion jumps; `Fitting` tells. A drawer being dragged, a Half or Full drawer and one that is still opening do not ease. `Open(at)` slides it up to `DrawerSnap.Fit` (as tall as its content, at most
90% of the board, the body scrolls only past that; default), `Half` (half the screen, opt-in, `vb-drawer--half`) or `Full`
(`vb-drawer--full`); `Snap` and `SnapChanged` follow the player, who drags the handle or head up to full, down to the content
height or below a quarter to close, and taps the handle to toggle full. `Close(done)` slides it down and hides it; `Dismiss()` does what
the backdrop, × and a closing drag do: close, then raise `Dismissed`.
`Dismissible`, `CloseButton` and `Backdrop` (off: docked inline, `vb-drawer--inline`, only the sheet takes input) are
UXML attributes. `DrawerOverlay.Push(nav, title, closed)` puts one on a modal layer (`vb-modal--drawer`) that Back pops.
`SquadView.DetailsDrawer` is one.

## Forms

`FieldComposite(label, input, help)`: a `LabelComponent` (`__label`), the input (`__input`, the content container) and a
caption (`__help`). `FormComposite(header)`: a scroll (`__scroll`) of `Fieldset(title)` sections, `Note(icon, text)`,
`Footer(actions…)` and `Changed`, raised after any `ChangeEvent<float|bool|int>` from inside. `SavedMeta` shows on the
header's meta, or on the form's own `__status` line without a header.

## Layouts

`StackLayout(gap)` (`vb-stack vb-stack--{4|8|12|16}`; marks its last child `vb-stack__last`), `VbGrid(columns, wrap)` (the GridLayout entry; not `UnityEngine.GridLayout`)
(`vb-grid vb-grid--{2|3|4}`, `--wrap`), `ListLayout(empty)` (a `ScrollView` whose content is `vb-list`, 6 between rows,
with a `vb-list__empty` line) and `BoardLayout` (`Top`, `World`, `Bottom`). `Ui.Stack`, `Ui.Grid` and `Ui.List` build them.

## Views

Every view is a `ScreenView` (`vb-view vb-{block}`) that fills its Navigator page, binds a plain view model and raises
events; styles are in `Views.uss`.

| View | Bind | Events |
| --- | --- | --- |
| `MainMenuView` | `MainMenuViewModel` (status, best, bestWave, checkpoint, squad, notice, canExit). HIGH SCORES and SETTINGS carry distinct glyphs; EXIT is an icon button beside BEST | `Play`, `Resume`, `Scores`, `Settings`, `Exit` |
| `MenuActionsSectionView` | `Bind(CheckpointView, squadLine, notice)` | `Play`, `Resume`, `Scores`, `Settings` |
| `SquadView` | `SquadViewModel` (roster, selected, focus, locked, tiers, seed). No captions (the roster says so only while part of it is locked), tiles take the free height, details hide stability until it varies | `Picked(i)`, `SlotPicked(i)`, `Toggled(i)`, `Closed`, `Start`, `Back` |
| `SettingsView` | `SettingsViewModel` (written back as it changes). Audio, feel, camera, quality, Accessibility (reduced motion and a DISPLAY OPTIONS row) and Data (CLEAR LOCAL SCORES, a danger row that confirms) in one scroll; no footer | `Changed(model)`, `ClearScores`, `Accessibility`, `Back` |
| `PauseOverlayView` | `PauseViewModel` (wave, score, standing, perks, seed, detail). No header arrow (RESUME is the one way back), no captions; the seed and rules line is an info popover; SETTINGS and ABANDON share an `ActionRow` | `Resume`, `Settings`, `Perks`, `Abandon`, `Exit` |
| `ConfirmDialogView` | `Push(nav, title, body, confirm, cancel)` | the confirm and cancel actions, after the layer closes |
| `GuideDialogView` | `Push(nav, title, steps, ready)`; `DefaultSteps` | READY, after the layer closes |
| `PerkDraftView` | `PerkDraftViewModel` (wave, bonus, score, choices, pick, squad) | `Picked(i)`, `Take`, `Perks` (the carry frame) |
| `ResultsView` | `ResultsViewModel` (title, cause, score, rank, ledger, stats, squad, perks). `title` is the heading ("RUN OVER" by default; the view sets it in caps). The rank badge hides on a score of 0; units show FELL Wn only; perks are `ChipRow`s | `Again`, `Menu`, `Squad` |
| `ActivePerksView` | `ActivePerksViewModel` (wave, perks, pick, totals; zero totals are not listed). The stacks are compact `ChoiceCard`s in a two-column grid and the picked one is explained below | `Picked(i)`, `Back` |
| `RevivePickView` | the fallen operatives to bring back | `Picked(i)`, `Back` |
| `AccessibilityView` | `AccessibilityViewModel` (reducedFlashes, largerText, highContrast; written back); the header and its one section no longer say the same word three ways | `Changed(model)`, `Back` |
| `HighScoresView` | `Bind(ranked, historical, caption)` | `Back` |
| `RouteChoiceView` | `RouteView` | `Chosen(side)` |
| `MerchantScreenView` | `MerchantView` (keeps the scroll position) | `Bought`, `PerkSold`, `HealthSold`, `Left` |
| `BattleHudView` | `Bind(HudView, OperativeView[], BossView)` every frame; `Card(i)`, `World`, `Announcer`, `CancelBand`; `Aiming(on)`, `Reveal(t)`, `Bands(on)` | `Pause`, `Auto`, `Speed`, `QueueCancel` |

`ScreenView.Deal()` glitches every `GlitchText` in and rises the view's `DealItems()` in one after another; route, merchant
and perk pages call it when they open. `BattleHudView` names its bands `hud`, `battlefield` and `cards` and the announcer
lane `hud-message`. Its cancel band (`cancel-hint`) shows over the cards while a skill is being aimed, `Reveal(t)` slides
the bands in during deployment and `Bands(on)` shows or hides them. Re-binding the same boss leaves the bar alone.


## Shared foundations and live effects

GUI 0.5 groups types by both composition level and family. `ViewBase` owns lifecycle; `OverlayView` owns the overlay contract, `SurfaceView` painting, `CardView` selection and `DetailView` explanations. Native input and scroll bases are retained. `KeyedList`/`Keyed.Bind` preserve repeated child identity. See the [complete audit](reuse-audit.md) and [migration guide](migration-0.5.md).

`EffectDefinition` describes meaning. `EffectInstance` carries a unique ID, optional source/timing, magnitude, stacks and activation (`Active`, `Conditional`, `Ready`, `Spent`, `Expired`). `EffectBadge`, `EffectDetails`, `EffectList` and `EffectStrip` reuse the same models. The compact battle summary is a 44-unit target opening the complete list, with individual hover/focus/touch explanations. Unknown IDs retain a fallback.
