# Changelog

## 0.3.2 — 2026-10-31

- **ProgressBar** (`vb-progress`): one bar for every read-out, continuous or `Segments` levels with the `Next` level
  outlined; tones ally, power, enemy, neutral. `StatReadout` draws its bar with it.
- **ListRow** (`vb-list-row`): one row primitive (glyph, title, caption, optional bar, trailing value and action).
- **Minimal PerkCard**: tier word, glyph, name, effect, levels as a segmented ProgressBar, the value after the pick. The art
  band, badge, category, pips and shimmer are gone (and their USS).
- **Minimal MerchantScreen**: one ListLayout of ListRows (rare finds, stack sales, health trades) and LEAVE MARKET.
- Web: `ProgressBar`, `ListRow` and the same minimal PerkCard and MerchantScreen.

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
