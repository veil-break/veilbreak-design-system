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

  `.vb-shape` resets them all, because custom properties inherit.
- **Notes:** the content box and hit area stay the element's rectangle. Token colours work directly:
  `--vb-edge: var(--vb-color-ally)`.

### VbFrame and panels

`VbFrame` is a `VisualElement` with a shape, for UXML: `<vb:VbFrame class="vb-panel vb-panel--solid"/>`. From code,
use `Ui.Panel(parent, classes…)`.

- **States:** `vb-panel--solid`, `--raised`, `--glass`, `--enemy`, `--ally`, `--power`, `--brackets`.
- **USS:** `vb-panel__head`, `__title`, `__meta`. `Ui.Head(panel, title, meta)` builds the head.

### Popover

Hover, focus and tap popovers. Content is built on demand and placed on the panel root, so it is never clipped, and
clamped inside the panel's edges.

- **Build:** `Popover.Attach(anchor, () => content, PopoverPlacement.Below | Above, alignEnd)`.
- **API:**
  - `Open()`, `Close()`, `IsOpen`.
  - `Pin()` keeps it open (galleries, tutorials, captures).
  - `Popover.CloseAll()` returns whether any were open.
- **States:** the anchor carries `vb-popover-open`. The content is wrapped in a `ScrollView` with `vb-popover`.
- **Access:** a mouse opens it on hover. Touch toggles it on tap. Focus opens it and blur closes it. It copies the
  anchor's root flags (`SafeAreaRoot.RootClasses`), so contrast, larger text and the motion settings reach it.
- **Notes:** only theme sheets reach the panel root. Keep the kit in the theme (`VeilbreakGui.tss`).

### ModalHost

Nested modal layers over a screen layer.

- **API:**
  - `Push(content, onClose)` returns a `vb-modal` layer and covers `content` (`vb-covered`, disabled).
  - `Pop()`, `Clear()`, `Count`.
- **Access:** focus moves to the new layer and returns to the previously focused element when it closes. Opening a
  layer closes all popovers.

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
- **USS:** `vb-screen` and `vb-page`, plus `vb-page--headed` (under a `ScreenHeader`), `vb-page__label`, `__caption`,
  `__footer` and `__scroll`, all in `Layout.uss`.

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

### OperativeCard

The portrait battle card.

- **API:** `Bind(OperativeView)`, and `Skill(0 | 1)` returns the A and S `SkillButton`s for the game's manipulators.
- **View model:** `OperativeView`: `id`, `name`, `code`, `role`, `roleIcon`, `kit`, `portrait`, `hp`, `maxHp`,
  `barrier`, `armor`, `stability`, `maxStability`, `statuses`, `mode`, `manualSeconds`, `skills`, `passive…`.
- **States:** `vb-operative--auto`, `--manual` (MANUAL with seconds), `--aiming` (brackets and glow), `--fallen`
  (FALLEN, "Revive via perk", the name struck through).
- **USS:** `__band`, `__portrait`, `__shade`, `__code`, `__chip`, `__kit`, `__body`, `__name`, `__numbers`, `__hp`,
  `__barrier`, `__statuses`, `__status--{kind}`, `__clear`, `__skills`, `__fallen`.
- **Access:** the mode chip is a word. Statuses are glyphs with tooltips, CLEAR when there are none and OUT when
  fallen.

### OperativeTile

A roster tile.

- **API:** `Bind(view, slot, focused, RarityTier tier, string locked)`. Subscribe to `Picked`.
- **States:** `vb-tile--in` (shows its squad slot), `--focused` (brackets and glow), `--locked`, `--blue`, `--violet`,
  `--gold`.
- **USS:** `__band`, `__portrait`, `__shade`, `__order`, `__kit`, `__body`, `__name`, `__role`, `__role-text`.
- **Access:**
  - The tooltip reads name, role and slot, or "locked: Pull in rifts".
  - A locked tile stays visible and says how to get the operative.
  - Tier colour comes with a frame weight.

### SquadSlot

One of the four squad slots.

- **API:** `Bind(view or null, slot)`. Subscribe to `Picked`.
- **States:** `vb-slot--empty` (dashed, + and SLOT *n*).
- **USS:** `__filled`, `__band`, `__portrait`, `__order`, `__kit`, `__row`, `__name`, `__code`, `__empty`,
  `__empty-label`.

### OperativeDetails

The operative sheet.

- **API:** `Bind(view, slot, squadFull, maxHp, maxArmor, maxStability)`. Subscribe to `Closed` and `Toggled`.
- **Shows:**
  - Health, armor and stability bars, scaled to the roster maximums.
  - The passive.
  - Both skills, with cost and cooldown.
  - ADD TO SQUAD or REMOVE FROM SQUAD (danger). When the squad is full, adding is blocked with "Squad full · remove
    one first".
- **USS:** `vb-details`, `__head`, `__kit`, `__name`, `__role`, `__slot`, `__slot--in`, `__close`, `__stats`,
  `__passive`, `__skills`, `__skill`, `__skill-icon`, `__skill-name`, `__skill-desc`, `__skill-cost`, `__toggle`.

### OperativeStats and SquadResult

The post-run contribution.

- `SquadResult.Bind(ContributionView)` is the results cell. Hover, focus or tap opens `OperativeStats` above it
  (`Details`).
- **View model:** `ContributionView`: `operative`, `fellWave`, `damage`, `kills`, `casts`, `power`, `share`,
  `extraLabel`, `extraValue`.
- **States:** `vb-result--fallen`. On the stats: `__fell--alive`.
- **USS:**
  - `vb-contribution__head`, `__portrait`, `__name`, `__fell`, `__share`, `__share-fill`, `__grid`, `__cell`, `__key`,
    `__value`.
  - `vb-result__name`, `__fell`, `__kit`.

---

## Perks

`PerkView`: `id`, `name`, `category`, `tag`, `icon`, `effect`, `now`, `next`, `tier` (General, Operative, Rare), `art`,
`stack`, `max`, `waves`. Every perk part carries `vb-perk--{tier}`. Operative perks show their operative, and others
show a ringed glyph.

| Component | Use | API | USS hooks |
| --- | --- | --- | --- |
| `PerkCard` | One of three draft cards: art, tier tag, name, effect, stack pips and the value after the pick | `Bind(view, selected)`, `Picked` | `--selected` (check, brackets, glow); `__art`, `__badge`, `__glyph`, `__tag`, `__check`, `__body`, `__category`, `__name`, `__effect`, `__pips`, `__pip--owned`, `__pip--next`, `__next`, `__stacks` |
| `PerkDetail` | Full explanation: draft (Now → After pick, stacks) or owned (Active, stacks, waves picked) | `Bind(view, draft)` | `__head`, `__thumb`, `__name`, `__tag`, `__effect`, `__rows`, `__row`, `__row--last`, `__key`, `__value`, `__value--good` |
| `PerkChip` | Compact owned perk; hover, focus or tap shows `PerkDetail` | `new PerkChip(alignEnd)`, `Bind(view)`, `Details` | `__name`, `__count`, `__popover` |
| `PerkRow` | An active-perk list row | `Bind(view, selected)`, `Picked` | `--selected`; `__thumb`, `__text`, `__line`, `__name`, `__tag`, `__effect`, `__right`, `__value`, `__stacks` |

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
    `Label(id, world, kind, title, detail, team)`, `Marker(id, world, kind, size)` and
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

- **API:** `Bind(kind, title, detail, team)`, `ShowDetail(on)`, `Title`, `Detail`.
- **States:** `vb-world-label--aim`, `--name`, `--telegraph`, `--estimate`, `--blocked` (prefixed with ×), and
  `--ally` or `--enemy`.
- **USS:** `__title`, `__detail`.

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
| `RouteChoice` | `RouteView` with wave and two `RoadView` signs | `Chosen(side)` once, left 0 / right 1. Full signs exceed 44 units; keyboard Enter/Space. `Choose` supports alternate input. |
| `MerchantScreen` | `MerchantView` with credits, offers, sellable stacks and HP consequences | `Bought(index)`, `PerkSold(id)`, `HealthSold(actor)`, `Left`. Host validates and rebinds after each transaction. `ScrollOffset` preserves position. |
| `CreditsChip` | `Set(credits)` | Currency icon and integer; formats on change. |

`PerkTier.Rare` is cyan; `Epic` is violet with a short shimmer. Both have explicit text labels.
Merchant offer states say why a purchase is blocked (credits, sold, stack cap). Health rows state the before/after HP;
fighters that cannot retain 1 HP cannot trade. Instant rewards are excluded from sellable stacks by the host.
`HudTop` places pause and icon-only autoplay at the top left. Autoplay retains its state tint and tooltip, speed stays
on the right, credits sit beside Power. `WaveSlammed` lets the host request its own bounded camera jolt; the GUI has
no dependency on a camera or arcade simulation. The host must use actual safe-area insets, with a small battle fallback.

## Text, labels, badges, tiles and cards

### TextComponent and LabelComponent

`TextComponent : Label` carries one type style (`TypeStyle.Display`, `Title`, `Label`, `Badge`, `Body`, `Caption`, `Num`
→ `vb-t-*`) and a tone (`TextTone.Ally`, `Enemy`, `Power`, `Muted` → `vb-t-ally` …; `Default` adds nothing). UXML:
`<vb:TextComponent type-style="Caption" tone="Muted" text="…"/>`. `LabelComponent` is the caps eyebrow: it uppercases its
text, takes `LabelSize.Label` or `Badge`, and adds `vb-label` (4 units under it). `Ui.Text(parent, text, TypeStyle, tone)`
and `Ui.Eyebrow(parent, text)` build them.

### BadgeComponent

A slim chip: optional glyph (`vb-badge__icon`), word (`__text`, uppercased) and value (`__value`), tone
`BadgeTone.Neutral | Ally | Enemy | Power | Warn | Inverted` as `vb-badge--{tone}`. `Set(value)` only touches the label
when the value changed. `Badge.Of(parent, …)`, and the presets `Badge.Status(kind, …)` (a `StatusChip`) and
`Badge.Credits(n)` (a `CreditsChip`).

### TileComponent

The pickable cell `OperativeTile` and `SquadSlot` extend: band (`__portrait`, `__shade`, `__glyph`, `__order`), `__kit`,
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
`Foot` (`__foot`; `Actions(...)`). `Open()` slides it up (`vb-drawer--open`), `Close(done)` slides it down and hides it,
`Dismiss()` does what the backdrop, × and a drag past a quarter of the sheet do: close, then raise `Dismissed`.
`Dismissible`, `CloseButton` and `Backdrop` (off: docked inline, `vb-drawer--inline`, only the sheet takes input) are
UXML attributes. `DrawerOverlay.Push(nav, title, closed)` puts one on a modal layer (`vb-modal--drawer`) that Back pops.
`SquadView.DetailsDrawer` and `PauseOverlayView.Drawer` are drawers.

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
| `MainMenuView` | `MainMenuViewModel` (status, best, bestWave, checkpoint, squad, notice, canExit) | `Play`, `Resume`, `Scores`, `Settings`, `Exit` |
| `MenuActionsSectionView` | `Bind(CheckpointView, squadLine, notice)` | `Play`, `Resume`, `Scores`, `Settings` |
| `SquadView` | `SquadViewModel` (roster, selected, focus, locked, tiers, seed) | `Picked(i)`, `SlotPicked(i)`, `Toggled(i)`, `Closed`, `Start`, `Back` |
| `SettingsView` | `SettingsViewModel` (written back as it changes) | `Changed(model)`, `ClearScores`, `Accessibility`, `Back` |
| `PauseOverlayView` | `PauseViewModel` (wave, score, standing, perks, seed, detail) | `Resume`, `Settings`, `Perks`, `Abandon`, `Exit` |
| `ConfirmDialogView` | `Push(nav, title, body, confirm, cancel)` | the confirm and cancel actions, after the layer closes |
| `GuideDialogView` | `Push(nav, title, steps, ready)`; `DefaultSteps` | READY, after the layer closes |
| `PerkDraftView` | `PerkDraftViewModel` (wave, bonus, score, choices, pick, squad) | `Picked(i)`, `Take` |
| `ResultsView` | `ResultsViewModel` (cause, score, rank, ledger, stats, squad, perks) | `Again`, `Menu` |
| `HighScoresView` | `Bind(ranked, historical, caption)` | `Back` |
| `RouteChoiceView` | `RouteView` | `Chosen(side)` |
| `MerchantScreenView` | `MerchantView` (keeps the scroll position) | `Bought`, `PerkSold`, `HealthSold`, `Left` |
| `BattleHudView` | `Bind(HudView, OperativeView[], BossView)` every frame; `Card(i)`, `World`, `Announcer` | `Pause`, `Auto`, `Speed`, `QueueCancel` |
