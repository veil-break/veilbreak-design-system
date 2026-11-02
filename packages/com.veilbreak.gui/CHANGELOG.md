# Changelog

## 0.6.0 — 2026-11-02

Primitives that `com.veilbreak.ar-hud` used to carry alone now live here (veilbreak-design-system#13); ar-hud keeps its API as a
thin adapter over them.

- `VbRebuild`: animates the difference between two builds of a rebuilt tree (`Capture` before, `Play` after). Elements are matched
  by name path; changed classes replay from the old look, numbers count, text fades or decodes, bars grow, new elements fade in
  and named ones slide. Reduced motion keeps colour and opacity only. It runs on `VbMotion.Clock`, and its beat is the new
  `timings.rebuild` token in `Tokens~/motion.json` (generated into `MotionRecipes.Rebuild`). A host names its own skip and
  decode classes (`vb-no-motion` and `vb-decode` by default).
- `VbGauge` (`[UxmlElement]`): the radial segmented gauge from ar-hud (`Value`, `Segments`, `Tint`, centre `Readout`; colour from
  `--vb-gauge-color`, the ally colour by default) and its public `VbGauge.Draw` dial painter. `VbPaint`: Painter2D `Alpha`, `Line`
  and `Arc` helpers. `Gui.uss` styles `.vb-gauge`.
- `VbVectorShape`: the Painter2D cut shape from ar-hud's `HudShape`, next to the mesh-painted `VbShape`: a choice of cut corners
  (`flip`), lock-on brackets with a corner mask, a focus ring, glass (sheen, scanlines), halo and glow, blending between looks, and
  `Outline(element, inset)` for overlays that trace the painted edge. The custom properties it reads come from a
  `VbVectorVocabulary` (`--vb-vector-*` and the `vb-vector` class by default). Nothing in gui attaches it yet.
- `VbPressGlow` (with `VbPressGlowOptions`): the per-panel hover and keyboard-focus glow that traces a button's outline, plus the press
  sink, from ar-hud's `HudPress`. The neon property, keyboard class and reduced-motion check are options.
## 0.5.1 — 2026-11-01

- `MerchantScreen` names each row's action again (`buy-{index}`, `sell-perk-{id}`, `sell-hp-{actor}`); the 0.5.0 keyed rows
  had dropped the names, so hosts and the merchant tests could not find the buttons.

## 0.5.0 — 2026-11-01

- Shared view/surface/card/detail foundations and OverlayView/OverlayHost lifecycle; closing overlays retain coverage and focus ownership.
- Stable keyed collection binding across market, route, queue, draft, results, revive, details and perk consumers; fixed merchant deal discovery and cached boss/skill updates.
- Complete effect definition/instance model with touch/hover details, compact HUD summaries and untruncated lists. Battle cards grow to 202 units for a full status touch target.
- Generated shared motion recipes, separate presence/feedback/count channels, ordered theatrical deals, HUD scrubbing and cancellable drawers; reduced-motion handling throughout.
- React effect components and missing screen mirrors, family-based handbook catalog, source inventory and a complete 50-finding audit.
- Root pyproject, uv.lock, Python selector, locked Ruff and reproducible build checks. See Documentation~/migration-0.5.md and Documentation~/validation.md.

## 0.4.0 — 2026-11-01

Every state change in the kit now animates, and the arcade binds package views only.

- **VbMotion** (`Runtime/Core/VbMotion.cs`): real-time show/hide with `MotionKind` (Fade, Rise, Drop, Left, Right, Zoom),
  `In`, `Out`, `Show` (idempotent), `Stagger`/`StaggerChildren` (50 ms steps), `Swap` (fade out, rebuild, fade in), value
  cues `Pop`, `Shake` and `Count` (rolls a number over `dur-change`), plus `Finish`, `Shown`, `Displayed` (inline display,
  else the resolved style, so a UXML `style="display: none"` counts as hidden), `Busy` and `Calm`. Entrances
  take `dur-ui`, exits `dur-glitch-out`; Reduced Motion turns every movement into a fade and `Count` into a jump. Off-panel
  elements change at once, so view models still bind synchronously in tests. Detaching an element finishes its animation.
- **VbShape** cross-fades its fill, edge and glow colours over `dur-ui` when a state class changes (first paint snaps).
- **Navigator** pages leave (`vb-page--leaving`, fades out, then removed) while the next page rises in; closing fades the
  screen out. Re-opening the page that is already open swaps it at once. **ModalHost** fades the scrim in and staggers the
  layer's content; a popped layer is renamed `modal-layer-leaving`, stops taking input and fades out. **Popover** drops
  or rises in and fades out.
- **Motion.uss** (imported by `VeilbreakGui.tss`): transitions for text and tint colours, segmented options, the switch knob,
  press scale, meter widths, power previews and `vb-covered`, all removed under `vb-reduced-motion`.
- **Animated controls.** HUD score counts, speed and autoplay pop on change, `ScreenHeader.Meta` pops, credits count and pop,
  a refused `SkillButton` shakes and a skill coming ready pops; meter hints, previews, queue chips, the operative barrier,
  skills and fallen state, perk check marks, the menu checkpoint card and best badge, and the pause perks frame all
  show and hide through `VbMotion`.
- **Views.** `ScreenView.Deal()` (glitch-in for every `GlitchText`, then a staggered rise of `DealItems()`); route, merchant
  and perk pages deal. New `ActivePerksView` (`ActivePerksViewModel`), `RevivePickView` and `AccessibilityView`
  (`AccessibilityViewModel`). `PerkDraftView.Perks`, `ResultsView.Squad`, `SquadView`'s details drawer
  (`operative-details`, 0.3.1) binds roster maxima and fades between operatives; a newly selected `PerkCard` pops once. `BattleHudView` names its bands (`hud`, `battlefield`, `cards`) and the
  announcer lane (`hud-message`), adds a cancel band (`Aiming`), `Reveal(t)` and `Bands(on)`, and re-binds the boss bar only
  when a new boss arrives.
- **Tests.** `MotionTests` (off-panel motion, easing, navigator and modal leaving, same-page reopen, board names, theme import).
  The form and settings change-event checks live in `FormTests` (0.3.1), since change events dispatch only inside a panel.
- **Merged with 0.3.1–0.3.2** (drawer, ListRow, ProgressBar):
  - The drawer's × compiles; it called `Dismiss`, which returns a `bool`, as an `Action`.
  - `ProgressBar.Good` sets its value label's class; the setter's `value` hid the field.
  - Health trades read `600 / 1000 HP → 350 HP` again, in Unity and on the web.

## 0.3.2 — 2026-10-31

- **ProgressBar** (`vb-progress`): one bar for every read-out, continuous or `Segments` levels with the `Next` level
  outlined; tones ally, power, enemy, neutral. `StatReadout` draws its bar with it.
- **ListRow** (`vb-list-row`): one row primitive (lead, glyph, title, caption, optional bar, trailing value and action).
  `PerkRow`, `ScoreRow` and `RuleRow` are ListRows now (rule rows keep their highlighted IF values); the rule toggle is
  the shared `VbSwitch` in its compact form (`vb-switch--compact`, track only).
- **Minimal PerkCard**: tier word, glyph, name, effect, levels as a segmented ProgressBar, the value after the pick. The art
  band, badge, category, pips and shimmer are gone (and their USS).
- **Minimal MerchantScreen**: one ListLayout of ListRows (rare finds, stack sales, health trades) and LEAVE MARKET.
- **KeyValueRow** (`vb-kv`): one key/value line; PerkDetail rows and the results ledger use it. `ProgressBar.Tint(colour)`;
  the contribution card's damage-share bar is a tinted ProgressBar.
- Web: `ProgressBar`, `ListRow`, `KeyValueRow` and the same minimal PerkCard and MerchantScreen.

## 0.3.1 — 2026-10-31

- **Bottom drawer.** `DrawerOverlay` (`vb-drawer`): a slim solid sheet that slides up over a dimmed backdrop to **half the
  screen**, with a grab handle, optional caps title, meta and close square, a scrolling body and a foot of actions. Drag
  the handle or head: up snaps to the **full screen** (`DrawerSnap.Full`, `vb-drawer--full`), down back to half, and below
  a quarter of the screen it closes (`Dismissed`); a tap on the handle toggles half and full. The backdrop and × dismiss
  it too; `Close()` slides it away quietly. `DrawerOverlay.Push(nav, title, closed)` puts one on a modal layer that Back
  pops; `Backdrop = false` docks it inside a view. Reduced motion skips the slide; high contrast gives the sheet its edge.
  Web: `DrawerOverlay` with the same tree, `snap` and `onSnap`.
- `SquadView` opens operative details in a drawer over the roster (`DetailsDrawer`; dismissing raises `Closed`).
- `PauseOverlayView` is a headed view: a `ScreenHeader` whose back square (the same `IconButtonComposite` as Settings and
  High Scores) resumes, then one overlay panel centred on the screen (`Panel`). SETTINGS and ABANDON share a row and EXIT
  TO MENU is a ghost at the bottom of the panel.
- Handbook: DrawerOverlayComposite entry (80 entries).

## 0.3.0 — 2026-10-31

Everything the GUI Handbook described as partial or proposed now ships, in Unity and on the web. The handbook is
published from `docs/` (GitHub Pages) and the web kit is a package of its own (`Web~`).

- **Slim frame.** `.vb-frame--slim` (cut-xs 4, no edge) and `.vb-frame--chipped`, `FrameKind`, `VbFrame.Kind`,
  `Ui.Frame`, token `cut-xs` (`--vb-cut-xs`). The package's containers (sections, dialogs, details, score, perk and rule
  rows, results, queue rows, merchant exchanges) moved to slim; selected and warning states keep their edge, and every
  slim container gets an edge back under `vb-contrast`.
- **Atoms.** `TextComponent` (`TypeStyle`, `TextTone`, new `vb-t-muted`), `LabelComponent` (`LabelSize`, `vb-label`),
  `BadgeComponent` (`BadgeTone`) with `Badge.Of/Status/Credits` presets, `TileComponent` (`TileState` flags; `OperativeTile`
  and `SquadSlot` now extend it), `CardComponent` (media, head, body, foot, selected).
- **Composites.** `IconButtonComposite` (`Active`, `Quiet`); `ScreenHeader`'s back, `HudTop`'s pause, autoplay and speed and
  `OperativeDetails`' close are built from it. `FieldComposite`, `FormComposite` (fieldsets, note, footer, `Changed`,
  `SavedMeta`). `Ui.Section` titles are `LabelComponent`s.
- **Layouts.** `StackLayout` (`vb-stack--{4|8|12|16}`), `ListLayout` (`vb-list`, `vb-list__empty`), `VbGrid`
  (GridLayout; `vb-grid--{2|3|4}`; named so it never clashes with `UnityEngine.GridLayout`), `BoardLayout` (`vb-board__top/__world/__bottom`); `Ui.Stack/List/Grid`.
- **Views** (`Runtime/Views`, `Views.uss`, view models in `Models.cs`): `MainMenuView`, `MenuActionsSectionView`,
  `SquadView`, `SettingsView`, `PauseOverlayView`, `ConfirmDialogView`, `GuideDialogView`, `PerkDraftView`, `ResultsView`,
  `HighScoresView`, `RouteChoiceView`, `MerchantScreenView`, `BattleHudView`. The arcade's screen styles moved into the
  package with `vb-` names, so the handbook no longer needs the game repository.
- **Web.** `Tools~/web/kit.js` ports every new type; `Tools~/web/package.py` writes `Web~/dist` (IIFE, ES module, CSS,
  tokens, fonts, types) and `Web~/demo` chains the views into a small app. Layout effects are server-render safe.
- **Handbook.** All 79 entries are shipped; `build_handbook.py PACKAGE docs` writes the page, the web kit and the demo
  for GitHub Pages. The "Slim containers" switch is gone (slim is the default); there is a Web kit page.
- `Tools~/metas.py` writes missing `.meta` files with stable GUIDs.
- Fixed: `BattleHudSample` did not compile (`top.Add(screen){…}`); `VbSlider.SetValueWithoutNotify` now refreshes its
  number and fill, so rebinding a visible slider shows the new value.
- **Breaking:** `vb-score`, `vb-perk-row` and `vb-rule` no longer carry `margin-bottom: 6px`; put rows in a `ListLayout`
  (or a `vb-list`). `SquadSlot`'s parts are `vb-slot__body` and `vb-slot__role-text` (were `__row`, `__code`), and its
  filled/empty state is `TileState`. HUD squares carry `vb-icon-button` alongside their old classes.

## 0.2.0 — 2026-10-30

The package is now the source of the Veilbreak design system: tokens, fonts, icons and the screen layer live here,
and the web mirror is generated from it.

- `FontPrimer`: `SafeAreaRoot` primes every font face's dynamic atlas with the kit's glyphs on the first layout pass,
  so title tracking no longer depends on the order a session first shows characters (arcade ARC-51).
- Header titles, header meta and HUD wave and score text carry the prototype's soft drop shadow over the world.
- Tokens: `Tokens~/tokens.json` is canonical; `Tools~/tokens.py` generates `Tokens.uss` (`--vb-color-*`, spacing, cut,
  size, stroke, fx, opacity, duration, type sizes and `--vb-font-<family>-<weight>` font files), `TokensContrast.uss`
  and `VbTokens.cs`. **Breaking:** colour variables are `--vb-color-<name>` (were `--<name>`).
- `Gui.uss` reads every token-equal colour and font through `var()`; HUD band spacing, compact (`vb-compact`) HUD and
  bottom-docked `vb-modal` sheets are part of the kit rather than game overrides.
- New: `Navigator`, `SafeAreaRoot`, `Dialog`, `Backdrop`, `Ui.Panel/Section/Scroll/Spacer/Changed`, `Layout.uss`,
  `VeilbreakGuiContrast.tss`, `Runtime/Settings/VeilbreakPortrait.asset`.
- `TargetDrag` moved here from Veilbreak Arcade (namespace `Veilbreak.Gui`). **Breaking:** its drag class is `vb-dragging`.
- Icons: 26 design-system glyphs added (99 in all) and 8 alias classes for design-system names. **Breaking:**
  `Icons/Mark.svg` is `Marker.svg` (`marker`); the brand mark stays `Vectors/Mark.svg`.
- Share Tech Mono added for HUD numerals; `LICENSE.md`; third-party notices list every font file.

- `SkillButton`: `Queued` (QUEUE with its #n place) and `Buffered` (NEXT with the reserved cost) states for manual casts that wait for their operative.
- `QueueList`: manual rows carry a cancel mark and raise `HudTop.QueueCancel` when tapped; `QueueView` gains `actor`, `slot`, `manual` and `next`.
- `VbSegmented`: three or more options get `vb-segmented--fill` and share the row, so they fit a 320 board.
- `GestureSurface` (`Runtime/Input`): pinch, twist, two-finger and empty-ground pan, wheel, right-drag orbit, double-tap and hover, yielding to a busy targeting drag. Engine-camera free; events are panel units.
- Ported from the design system's web kit: `QueueChip` (place, skill, cost and time, × on manual entries); `PowerStrip`
  `Reserve`, `Queue` and `Legend`; `HealthBar` ghost, sizes and `Caption`; `StatReadout.Delta`; `StatusChip` time;
  `OperativeTile` tier and locked; `SkillButton` windup sweep, invalid, rejected (400 ms flash) and exposed states.
- New holographic menu parts: `HoloFrame` (flicker-in, chromatic title), `ScanLines`, `HoloPlatform`, `RevealCard`
  (tier flip) and `RuleRow`.
- World space: `WorldLayer` (projector-agnostic, simulation time) with `WorldHealthBar` (ghost trail, barrier, elite and
  boss sizes), `CombatText` (merged per target, capped), `WorldLabel`, `WorldMarker` and `OffscreenIndicator`;
  `World.uss`. It replaces the feedback package's projected readouts.
- `MarkGeometry`: the VEIL/BREAK mark as data, shared by `VbMark` and the branding splash.
- Accessibility: `vb-reduced-flashes` (`SafeAreaRoot.Flags(..., reducedFlashes)`) drops glows, calms `HoloFrame` and
  turns the refused flash into a tint; high contrast drops glows too. Popovers copy the anchor's root flags, so they
  follow contrast, larger text, reduced motion and reduced flashes. `Ui.Under(element, class)` tests an ancestor class.
- Samples: **BattleHud**, a portrait board with a manual queue and the world layer; both samples run on import.
- Documentation: `Documentation~` holds the brand book, integration guide, component reference, generated token
  reference (`Tools~/tokens.py`) and guidelines 1–5 (layout, world space, effects, motion, platforms).
- Web mirror: `Tools~/web/build.sh` regenerates the whole design-system artifact from the package. The kit keeps its
  `vb-` names there; the original landscape web kit becomes `vbl-` and `window.Veilbreak.Legacy`.
- `EdgeWarning`: a band of rippling chevrons and a caption along the bottom (or top) board edge for threats about to
  cross it; urgency drives speed and a hot state, reduced motion holds it steady. Styles in `World.uss`.

## 0.1.0 — 2026-10-29

- First release, extracted for Veilbreak Arcade from the *Veilbreak Arcade Screens* design canvas.
- Mesh-painted cut shapes from USS custom properties; hover/focus/tap popovers placed on the panel root.
- Controls, meters, skill button, themed switch/slider/segmented control, HUD band, operative and perk composites.
- 72 Veilbreak stroke icons, the slash-cut mark, Poppins and TeX Gyre Adventor fonts, Veil tokens.
