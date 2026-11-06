# Changelog

## 0.9.0 — 2026-11-06

The owner's GUI polish list (design-system#55; one release, design-system 0.20.0). Phase 1, the generic parts; arcade adopts them in arcade#74.

- **Three text variants (ds#56).** `Tokens~/tokens.json` has one new type group: *body* (13, prose, slightly dimmed) and `body-sm`; *label* (11.5, 0.88× body, bold, 0.15em tracking) with `label-sm` (10) and `label-lg` (13.5); *heading* (bold, negative tracking, a 0.9 line) with `heading-sm` (13), `heading` (20) and `heading-lg` (32). The old HUD `label` and `badge` and the Text group's `body` and `caption` are folded into them. `Gui.uss` has one block that gives every text class its face, size and tracking from its variant (`.vb-t-body`, `-body-sm`, `-label`, `-label-sm`, `-label-lg`, `-heading`, `-heading-sm`, `-heading-lg`, and the old `.vb-t-display`, `-title`, `-badge`, `-caption` and the component classes: buttons, badges, chips, eyebrows, names, titles), so a size never changes in one place only. The primary button's label is the label variant like every other button (it was 16 px display against 12 px). `TypeStyle` gained `Heading`, `HeadingSm`, `HeadingLg`, `LabelSm`, `LabelLg` and `BodySm`. UI Toolkit has no `line-height` (the import warns "Unknown property"): a top-level heading's leading is trimmed with a negative margin (`--vb-trim-*`) and `--vb-line-height-heading*` stays the 0.9 reference for the web kit; a heading that wraps keeps the font's own leading between its lines. letter-spacing is in hundredths of an em (TextCore), so 0.15em is `15px`.
- **Fluid scale (ds#57).** The text sizes, gutters and margins follow the panel's width: 0.92× at 320, 1× at the 390 reference, 1.06× at 430, 1.18× from 600. UI Toolkit has no `clamp()` or `calc()`, so `Tools~/fluid.py` (from `Tokens~/fluid.json`) writes `Styles/Fluid.uss`, one class `vb-fluid-<width>` per step (320 to 430 every 10, then 460, 500, 540, 600) that overrides every type size, spacing and layout token (`--vb-gutter`, `--vb-pad`, `--vb-gap`, `--vb-margin`, `--vb-pad-neg`) and the font-size ladder (`--vb-fs-<n>`) that every `font-size` in the sheets now reads; `VbFluid.Apply(root,width)` sets the class and `SafeAreaRoot.Flags` calls it from the panel's width (`SafeAreaRoot.Fluid` turns it off). Popovers copy the class from their anchor. The page gutter, the section and stack rhythm, the header and the modal padding read the tokens.
- **Single-row actions and an icon-only button (ds#58).** `ActionRow.Single` keeps its buttons on one line at any width (they share the row, lose their trailing glyph and may wrap their label). `VbButton.IconOnly` is a 48-unit square with its glyph alone; the title stays as the accessible label (the tooltip). A new `cog` glyph (`Runtime/Icons/Cog.svg`, `vb-icon--cog`; the `settings` glyph is a sun) is the settings button. The main menu is `HIGH SCORES` and the cog in one row; the pause sheet is RESUME and one row of `EXIT TO MENU`, `ABANDON` and the cog.
- **Activation and damage edge (ds#59).** `VbEdgeFx`, from the MONO wireframe kit's animated gradient edges: on a click or submit (`VbButton`, anything `Clickable`) two bright heads start at opposite points of the element's cut outline and run clockwise, each with a fading tail and soft glow, in the element's own edge colours, then an afterglow fades (520 ms). `VbEdgeFx.Damage` / `UnitCard.Damage(strength)` is a translucent red flash over the card and a red border glow (560 ms); `UnitCard.AutoDamage` (on) plays it when the bound hit points fall. Reduced motion lights the whole edge and fades it in 100 ms; reduced flashes drop the flash fill. The overlay is `vb-edgefx`, added when it plays and removed when it ends, and a host that leaves its panel finishes it.
- **Content follows the cut (ds#60).** `VbClipFill` and `VbClipImage` paint themselves as polygons clipped to the outline of the nearest `VbShape` ancestor (`VbClip.Polygon`, a Sutherland–Hodgman clip), so a card's portrait no longer overflows its top-left cutout and a skill's charge and windup sweep no longer overflow its bottom-right cut (`overflow: hidden` clips to a rectangle only). `UnitCard`, `UnitTile`, `UnitSlot` and `SkillButton` use them; the last skill of a card takes the card's own cut (`vb-operative__skill-end`) so its corner lies on the card's. `Ui.Image` reaches a `VbClipImage`.
- **Status badge row (ds#61).** `StatusRow` (`StatusSquare` on `BadgeComponent`): tiny squares of about 20 × 20, at most six and a "+n" square, glyph alone (never colour alone) with the stack count in the corner; hover, focus or tap opens one vertical `EffectList` in a popover; the row's hit area is 44 units through padding that a negative margin takes back. `UnitCard.Statuses` is a `StatusRow` above the card (it was `StatusPips` on the portrait; `StatusPips` stays).
- **Unit sheet content (ds#62).** `OperativeView` gained `basic` (a `SkillView`, `slot="B"`, `kind="Basic"`), `passiveIcon` and `lore`. `UnitDetail` lists the passive, the basic attack and both skills as one list of `SkillSummary` rows (cost and cooldown only where there is one), then a LORE prompt after them (`UnitDetail.Rows`). The sheet's fixed height is gone, so the drawer fits the longer content.
- **`Carousel` (ds#63).** The active card takes 64% of the width and sits on top; its neighbours are tucked beneath it and tilted 7° a card (two a side with five or more cards); endless both ways. Swipe (the row follows the finger, settles on the nearest card, a flick goes one further), tap a card beneath, the arrow keys or focus change the card and raise `Changed`; it glides in 320 ms on real time and is instant under reduced motion. `Carousel.Bind(values,key,create,bind,selected)` keeps one element per key. `PerkDraftView` is a carousel of `ChoiceCard`s and no longer repeats the active card in a `PerkDetail`.
- **One card look (ds#64).** A perk, a road and an extract choice are one `ChoiceCard` (its tier tint, edge and rank cells); the draft's cards no longer override its cut and edge, and the carousel adds nothing of its own to the card.
- **Overlay transitions (ds#65).** `OverlayView.Entrance` and `Exit` default to `MotionRecipes.OverlayIn` (220 ms, a fade with a 12-unit rise and a 0.97 scale) and `OverlayOut` (150 ms); popovers use `PopoverIn` (140) and `PopoverOut` (90); the drawer's slide is 320 and 220 (it was 460 and 300). The modal layer fades and the `DialogSurface` in it rises. An overlay with `AutoPresent` presents itself when it reaches a panel (`DialogSurface`, `PauseOverlayView`).
- **Glitch title reveal (ds#66).** `Glitch.Reveal(element,ms)`, 2000 ms by default (`MotionRecipes.Reveal`): the text decodes out of noise glyphs left to right while it jitters sideways in 4-unit steps that shrink to nothing, its opacity climbs in coarse steps, and a `GlitchText` also tears into bands with a chromatic split snapped to the same grid. Elements with the class `vb-reveal-title` reveal when their overlay or screen is presented (`Glitch.RevealTitles`, called by `OverlayView.Present` and `ScreenView.Deal`): the pause title and its eyebrow, the results title and eyebrow, the draft's. `Announcer.RevealMilliseconds` (off by default; a game sets about 700) gives heads-up notices the same recipe. Reduced motion or flashes: the whole text with a 300 ms fade.
- Tests: `PolishTests` (EditMode), `PolishFitTests` (live boards at 320, 390 and 430), `PolishMotionTests` (each recipe's lifecycle, reduced motion included), `GuiPolishCaptureTests` (explicit, writes the review PNGs). `Tokens~/components.json` lists the 0.8.0 names it had missed.

## 0.8.0 — 2026-11-06

Arcade GUI review (design-system#38 primitives, design-system#39 composites; one release, design-system 0.18.0).

### Composites and views (design-system#39)

- **Generic names (renamed in place with `git mv`, every GUID kept).** `OperativeCard` → `UnitCard`, `OperativeTile` → `UnitTile`, `SquadSlot` → `UnitSlot`, `OperativeDetails` → `UnitDetail`, `OperativeStats` → `UnitStats`, `SquadResult` → `ResultSummary`; `PerkCard`, the road signs and the `RouteChoice` items → `ChoiceCard`; `PerkChip` and `CreditsChip` → `ChipRow`; the merchant's `OfferRow`, `SaleRow`, `HealthRow` and the perk detail rows → `StatRow`. Each old public name stays one release as an `[Obsolete]` type deriving from the new one (`Composites/Obsolete.cs`), so a game that has not adopted the names compiles with a warning. `PerkTier` moved from `Core/Models.cs` to `Composites/Perks.cs` (same namespace and assembly: nothing that used it changes). CSS class names keep their prefixes (`vb-operative`, `vb-tile`, …).
- **`UnitCard` (ds#20).** The name is the only identifier: the `__code` label and the squad slot's caption are gone. The "1 ↺" effect row is gone: `StatusPips` shows at most three timed statuses (`EffectInstance.Timed`) as icons on the portrait and opens their list on tap; passives and perks are not statuses. The mode chip shows only for MANUAL (with seconds) or AIMING, truncates inside the card and never overruns it at 320. A fallen card says FALLEN once with a greyed portrait and "Revive via perk" only while `OperativeView.canRevive`.
- **`SkillButton` (ds#21).** One layout in every state: slot letter, glyph and a cost line. The states are overlays: the cooldown seconds over the glyph, a corner badge ("−3" shortfall, "#1" queue place, "›" buffered), a dimmed fill instead of a dashed frame. NEED, QUEUE, CAST, AIM, LOCK and OUT captions are gone, so nothing clips below the slot at any width (`CompositeFitTests.SkillButtonNeverClipsShortfallOrQueueAt320`).
- **HUD (ds#23).** The autoplay toggle reads AUTO or MANUAL at every width. While an aim is live its hint ("Needle Storm · −18 · replaces auto") is the first line of the cancel band, which now reads RELEASE TO CANCEL.
- **Effects (ds#25, ds#26).** `EffectDetails` prints only what is known (title, description, source, magnitude, time left): no "Duration unavailable or persistent", "Source unavailable" or "Actor · Active". The list row no longer wraps "· Active". The title is caps in the body face (`vb-effect-detail__title`), and `FontPrimer.Styles` primes it and the other title styles at their own size (`FontPrimerPrimesTheTitleStylesAtTheirOwnSize`), so tracking no longer depends on what was drawn first.
- **`Markup` (ds#31).** One formatter for emphasis: `Markup.Rich` turns `*word*` into a colour run, `Markup.Plain` drops the marks. Every description label uses it (the passive, a skill, an effect, a perk, a choice card).
- **`ChoiceCard` (ds#34, ds#19).** The whole card is the button. `ChoiceSide.Left` cuts its top-left and bottom-left corners and aligns text right, `Right` mirrors it, `Single` keeps the regular cuts (`VbShape` gained `--vb-cut-side` and `VbShape.Outline(w, h, cut, inset, side)`). A centred glyph, rank cells (`RoadView.rank`/`ranks`, a perk's levels), compact text, a tint per tone (`TextTone` Ally/Enemy/Power) and the action word at the foot. The title steps down on a compact board so no word breaks (the 320 transformer title).
- **`StatRow` (ds#35).** A `KeyValueRow` (so hosts that read one still read this) whose key keeps its width, whose value wraps and whose trailing action is 44 units. The merchant's offers, sales and health trades are plated stacked rows: an offer that cannot be bought is dimmed with no second price (`VbButton.Dimmed`), an empty "Sell stacks" section hides, sales read `+30 ⬡`. `PerkDetail` rows are stat rows and its draft rows are Now (only with a value) and After pick.
- **`ChipRow` (ds#36).** A tinted plate (rarity is a tint, never an edge) with a 44-unit hit area; `ChipRow.Credits()` is the read-only HUD chip.
- **Views (ds#29, ds#33, ds#37).** Squad: no captions, role text hidden at 320 (icon only), tiles take the free height, stability shows only once operatives differ. Details close is a plain icon button. Menu: HIGH SCORES has its own glyph, EXIT is an icon button beside BEST. Pause: no header arrow (`ScreenHeader.ShowBack`), no caption or RESUME sub line, SETTINGS and ABANDON in an `ActionRow`, the seed line behind an info popover. `VbButton.Lead` puts a glyph before its label (the `back` and `chevron-left` glyphs lead by themselves). Settings: Accessibility (reduced motion and a DISPLAY OPTIONS row) and Data (CLEAR LOCAL SCORES, a danger row) are fieldsets, the footer is gone, the scroll pads its end. Accessibility: the header and its section no longer repeat each other. Also cut: "Two roads. One choice…", "Local scores only…", "Percent bonuses add…", zero totals, "STANDING", "SQUAD · HOVER OR TAP FOR CONTRIBUTION", the rank badge on a score of 0, and the draft's ", focus refills".
- **Not done here.** Folding the accessibility page into Settings needs the game to merge its two view models (`SettingsViewModel` has no flashes, text-size or contrast fields): Settings carries reduced motion and a row to the display options for now. The debrief's route-history section and the playtest note are the game's.
- Tests: `CompositeTests` (24 EditMode tests), `CompositeFitTests` (live boards at 320, 390 and 430), `TokenTests.FontPrimerPrimesTheTitleStylesAtTheirOwnSize`; the older tests moved to the new names.

### Primitives (design-system#38)

- **Border rule (ds#22).** A surface inside another surface has no edge: `--vb-bw: 0`, no dashes, brackets or glow, and a fill one tone up. `Gui.uss` lists the nested surfaces between `nest:begin` and `nest:end` (skill buttons inside a battle card, tiles and slots, queue rows, the Power panel in the HUD, perk chips, the thumbnails of perk rows and details, status chips, secondary buttons inside dialogs, drawers and sections) and `.vb-nest` opts any element in. Edges stay on what floats over the world (battle card, popover, drawer sheet, dialog), the one primary action, a choice card, a selection or focus ring (focused tile, aiming card), a danger action and an empty drop target (the dashed empty slot). Dashed frames for blocked and short actions are gone: they dim instead. Hairline separators in details, contribution, perk detail and results are spacing. `.vb-contrast` gives every nested surface its edge back (`Layout.uss`). `TokenTests.NestedSurfacesDropTheirEdge` reads the list; the measured border depth over the 42 arcade review captures (at 320, 390 and 430) goes from 2 (13 captures) to 0, and bordered elements from about 915 to 461.
- **`ActionRow` (ds#32, `Layouts`).** Buttons in a row that fill it: every visible child takes an equal share (`flex-grow:1; flex-basis:0; min-width:0`, 8 between) and the row stacks into a column below `MinItemWidth` (112) per child. `Fixed` keeps one width (92) for trailing actions. `Ui.Actions(parent, …)`. Used by the results row, the menu row, the confirm and guide dialogs, `DrawerOverlay.Foot` (now an `ActionRow`), the details toggle and `ListRow.Trail` (merchant BUY and SELL line up).
- **`DrawerSnap.Fit` (ds#30).** The new default: the sheet is as tall as its content, at most 90% of the board, and the body scrolls only past that. Dragging up goes to `Full`, down below a quarter closes, a tap on the handle toggles `Full`. `Half` stays, opt-in (`Open(DrawerSnap.Half)`, `vb-drawer--half`); `DrawerOverlay.SnapFor(h, full, from, rest, fit)` extends the rule. The enum keeps its numbers (`Fit` is last).
- **Popover (ds#24).** One solid surface (`--vb-fill: panel-solid`, one edge, padding) with a painted arrow toward the anchor; its content (`vb-popover__content`) drops its own surface and sits flat. Height and width are clamped to the panel and re-clamped on every geometry change of the popover, the anchor or the panel (`Popover.Fit` and `Popover.Limit` are the pure placement). `PopoverCard`: a caps title, an optional hint line, then flat rows (`KeyValueRow`). The queue popover stacks its title over its hint and its rows are flat with the × at the row's end.
- **World labels (ds#27).** `WorldLayer.Label(…, group)`: labels with the same group key and title merge into one "TETHERED ×3" at the centroid of their projected points (a lone label reads as itself; the merged label drops its detail). `WorldLabelKind.Elite`: one line, a diamond mark and the trait ("◆ Voltaic"). Labels are unbordered text on a dark underlay; ownership reads from the text colour.
- **`ReorderManipulator` (ds#28, `Input`).** Drag past a slop or long-press (0.35 s) lifts a ghost over the panel root; the drop slot comes from the children's `worldBound`; `Reordered(from, to)`. Alt+arrow moves a focused child and keeps focus on it. The ghost glides into the slot, or goes at once under reduced motion. `SquadView.SlotMoved(from, to)` (a drop on an empty slot reports the last filled one) and `SquadView.Reorder`.
- **One shape vocabulary.** `--vb-cut`, `--vb-fill` (`-to`, `-dir`), `--vb-edge`, `--vb-bw`, `--vb-bracket` (`-len`) and `--vb-glow` are canonical for `VbShape` and `VbVectorShape`; `--vb-vector-*` stay as aliases and win when set; ar-hud's bare `--cut` … are fallbacks for gui's own vocabulary. A vocabulary made with the public constructor (ar-hud's `--`) reads only its own prefix, so ar-hud compiles and behaves unchanged. `.vb-vector` resets the canonical properties. `VbVectorVocabulary.Candidates(name)` lists the names tried.
- `.vb-progress__cell--next` is a tone, not a border; the meter preview and the skill charge and sweep lose their hairlines.
- Tests: `FitTests` (`ActionRowFills`, `DrawerFitsContent`, `PopoverClampsToPanel`, `NestedSurfacesPaintNoEdgeUnlessContrast`, `SquadSlotsReorderByDragging`), `ReorderTests`, `PopoverTests`, `TokenTests` (nested surfaces), `VectorShapeTests` (vocabulary), `WorldLayerTests` (label merge, elite labels).
- Arcade's own `Arcade.uss` still overrides `.vb-results__menu` and `.vb-results__again` (G2 adoption).

## 0.7.0 — 2026-11-02

- `VbFocus` (veilbreak-main's `MenuKit`, M5/3.4.6): keyboard focus across in-place redraws. `Keep(root, redraw, fallback)` remembers the focused element's name and focuses the new element of that name after the redraw; `Soon(root, name, fallback)` focuses a named element now or after a fresh layout and scrolls it into its list; `Read(element, name)` makes plain text a focus stop so the keyboard can scroll read-only lists. Pure UI Toolkit, no host types. Unit tests: `FocusTests`.

- `Tools~/screen_kit.py` (from veilbreak-main's `vb screens build`): generates the screen kit's 57 reference vectors (frames, ability states, markers, telegraphs, decals) from the veil tokens into `Tools~/references/screen-kit/`; `--check` is part of CI's tooling validation. Unity does not import them. The 72-icon reference render and the original wordmark were not carried over: `Runtime/Icons` and branding's mark are the shipped sources.
- `Tools~/references/atlases/` holds the VEIL_BREAK tactical-icon and rarity-badge atlases (moved from veilbreak-main's `assets/`), the reference art for gui's icons and badges. Unity does not import them.
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
