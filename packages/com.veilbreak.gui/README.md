# Veilbreak GUI

`com.veilbreak.gui` is a portable UI Toolkit kit in the Veilbreak **Veil** theme. It renders the
components of the *Veilbreak Arcade Screens* design canvas and has no dependency on any game:
you bind plain view models (`OperativeView`, `SkillView`, `PerkView`, `HudView` …) from your own state.

It is also the source of the Veilbreak design system: tokens, fonts, icons and guidelines live here, and every web
output is generated from this folder:

- **Web kit** (`Web~`, `@veilbreak/gui`): React 18 ports of every component and view, with the same names, classes and
  view models, plus tokens, fonts and types. See [`Web~/README.md`](Web~/README.md).
- **GUI Handbook** (`docs/`, published with GitHub Pages): every component in four tiers with its Unity type, USS,
  tokens, C# API and a live preview, the web kit page and a small web app demo that chains the views.
- **Design-system artifact** (`Tools~/web/build.sh`). Start with
[`Documentation~/index.md`](Documentation~/index.md): the brand book, integration guide, component reference, token
reference and guidelines.

Unity **6000.3** or newer, UI Toolkit runtime panels. No `Resources.Load`, no scene objects, no reflection.

## Install

Add it from Git in `Packages/manifest.json`, or embed or reference a checkout as a local package:

```json
"com.veilbreak.gui": "https://github.com/veil-break/veilbreak-design-system.git?path=/packages/com.veilbreak.gui#<full-reviewed-commit>"
```

Use `Runtime/Settings/VeilbreakPortrait.asset` as your `PanelSettings` (390 × 844 reference, match width), or copy
its values (`EditorUtility.CopySerialized`) into your own and set the theme to `Runtime/Styles/VeilbreakGui.tss`
(`VeilbreakGuiContrast.tss` for an always-high-contrast build). To add game styles, import the kit from your own theme:

```css
@import url("/Packages/com.veilbreak.gui/Runtime/Styles/VeilbreakGui.tss");
```

The kit's sheets belong in the **theme**, not only on a document: popovers are placed on the panel root
so they are never clipped, and only theme rules reach them. `Tokens.uss` defines the token variables
on `:root`; game sheets use them too (`var(--vb-color-ally-text)`, `-unity-font-definition: var(--vb-font-body-medium)`).

## What is in it

| Area | Types |
| --- | --- |
| Core | `VbShape` (cut-corner painter driven by `--vb-*` USS properties), `VbVectorShape` (the Painter2D sibling: choice of cut corners, brackets, focus ring, glass, halo; its properties come from a `VbVectorVocabulary`), `VbPressGlow` (hover and keyboard-focus glow with a press sink for every button on a panel), `VbFrame`, `Popover` (hover, focus and tap), `OverlayHost` (blurred modal layers with focus return), `VbRebuild` (animates the difference between two builds of a rebuilt tree), `Navigator` (one page at a time; Back closes the popover, then the modal, then runs the page's back, then the host fallback), `SafeAreaRoot` (safe-area insets in panel units with 48/34 minimums, plus the `vb-compact`, `vb-large-text`, `vb-reduced-motion`, `vb-reduced-flashes` and `vb-contrast` root classes, which popovers copy), `Ui` (fluent builders: `Box`, `Text`, `Panel`, `Section`, `Scroll`, `Spacer`, `Changed`), `VbTokens` (generated colours for code) |
| Controls | `VbGauge` (radial segmented gauge), `VbPaint` (Painter2D helpers), `VbButton` (primary / secondary / light / danger / ghost, blocked-with-reason), `VbIcon` (99 stroke glyphs by name, plus 8 design-system alias names), `VbMark`, `StatReadout`, `StatusChip`, `VbMeter`, `HealthBar`, `PowerStrip` (reserve line, queue chips, legend), `FocusMeter`, `SkillButton`, `QueueChip`, `VbSwitch`, `VbSlider`, `VbSegmented` |
| Input | `TargetDrag`: one-pointer press, drag and release with capture for aiming and moving; the target carries `vb-dragging`. `GestureSurface`: camera gestures that share a surface with the game's targeting drag. One finger on empty ground or two fingers pan, pinch zooms, twist turns, the wheel zooms, a right drag orbits, a double tap resets and a hovering mouse reports its position. It yields while the game says a drag is busy. |
| World | `WorldLayer` over a projector you supply (`Func<Vector3,Vector2?>`), advanced on simulation time: `WorldHealthBar` (ghost trail, barrier, elite and boss sizes), merged and capped `CombatText`, `WorldLabel`, `WorldMarker` and `OffscreenIndicator`; `World.uss` |
| Layout | `Layout.uss`: `vb-row`, `vb-grid4`, `vb-grid2`, `vb-half`, `vb-grow`; Navigator `vb-screen`/`vb-page` (`--headed` under a `ScreenHeader`, `__label`, `__caption`, `__footer`, `__scroll`); `vb-section`; `vb-dialog`; `vb-backdrop`; larger-text sizes |
| Motion (0.4.0) | `VbMotion`: `Show`/`In`/`Out` with `MotionKind`, `Stagger`, `Swap`, `Pop`, `Shake`, `Count`; animated `Navigator` pages, `OverlayHost` layers and `Popover`s; `VbShape` colour cross-fades; `Motion.uss` transitions. All of them become fades under Reduced Motion. |
| Atoms (0.3.0) | `TextComponent` (`TypeStyle`, `TextTone`), `LabelComponent`, `BadgeComponent` (+ `Badge` presets), `TileComponent` (`TileState`; `OperativeTile` and `SquadSlot` extend it), `CardComponent`, `VbFrame.Kind` (`FrameKind.Slim`/`Chipped`) |
| Forms and overlays | `IconButtonComposite`, `FieldComposite`, `FormComposite`, `DrawerOverlay` (bottom drawer; `Push(nav, …)` or docked) |
| Layouts | `StackLayout`, `VbGrid` (GridLayout), `ListLayout`, `BoardLayout`; `Ui.Stack/Grid/List/Frame` |
| Views | `MainMenuView`, `MenuActionsSectionView`, `SquadView`, `SettingsView`, `PauseOverlayView`, `ConfirmDialogView`, `GuideDialogView`, `PerkDraftView`, `ResultsView`, `HighScoresView`, `RouteChoiceView`, `MerchantScreenView`, `BattleHudView` (`Runtime/Views`, `Views.uss`), `ActivePerksView`, `RevivePickView`, `AccessibilityView`; `ScreenView.Deal()` |
| Composites | `Dialog` (confirmation sheets, pushed as a modal over the current page), `Backdrop` (cover-scaled artwork with a legibility shade), `HoloFrame`, `ScanLines`, `HoloPlatform`, `RevealCard`, `RuleRow`, `HudTop` (translucent band; hover shows the queue), `QueueList` (manual rows cancel on tap), `BossBar`, `OperativeCard`, `OperativeTile`, `SquadSlot`, `OperativeDetails`, `OperativeStats`, `SquadResult`, `PerkCard`, `PerkDetail`, `PerkChip`, `PerkRow`, `ScreenHeader`, `ScoreRow` |

Every element is also a `[UxmlElement]`, so it can be placed in UXML and styled from USS.

```csharp
var card = new OperativeCard().Bind(view);          // update every frame: Bind reuses its children
card.Skill(0).AddManipulator(myAimDrag);             // the game owns input; the kit only draws
var chip = new PerkChip().Bind(perk);                // hover/focus/tap shows PerkDetail
var hud = new HudTop(); hud.PauseClicked += Pause;   // hud.Bind(hudView) each frame
var nav = new Navigator(screenLayer);                // nav.Open("settings", back) returns the page; nav.Back() on Escape
var sheet = Dialog.Push(nav, "LEAVE GAME?", "Your profile is saved.");
var safe = new SafeAreaRoot(root, panelSettings, screenLayer);  // safe.Refresh() and safe.Flags(...) each frame
var camera = new GestureSurface(() => aiming);       // register after the targeting drag on the same element
camera.Pinched += f => rig.ZoomBy(f);                // events carry panel units; the game maps them to its camera
```

## Styling contract

- Shapes read `--vb-cut --vb-fill [--vb-fill-to --vb-fill-dir] --vb-edge [--vb-edge-to] --vb-bw --vb-dash --vb-bracket --vb-glow`.
  `.vb-shape` resets them (custom properties inherit), so give a variant class its own values.
- Component classes follow BEM: `vb-skill`, `vb-skill__icon`, `vb-skill--cooldown`. States are modifier classes,
  so a game can restyle a state without code.
- UI Toolkit has no `text-transform`: components pass caps themselves; pass display names in normal case.
- Colour never travels alone: statuses carry glyph + word, blocked controls carry their reason, cooldowns fill.
- Colours come from tokens: `var(--vb-color-<name>)` in USS (shape properties included), `VbTokens.<Name>` in code.
  Add `vb-contrast` to a root element for the high-contrast theme.

## Regenerating

- `uv run --frozen python Tools~/icons.py` rebuilds `Styles/Icons.uss` from `Runtime/Icons/*.svg` (kebab-case class per file).
- `uv run --frozen python Tools~/screen_kit.py` rebuilds the screen-kit reference vectors (cut-corner frames, ability states, markers, telegraphs, decals) in `Tools~/references/screen-kit/`; `--check` reports drift. Moved from veilbreak-main's `vb screens build`.
- `uv run --frozen python Tools~/tokens.py` rebuilds `Styles/Tokens.uss`, `Styles/TokensContrast.uss`, `Core/VbTokens.cs` and
  `Documentation~/tokens.md` from `Tokens~/tokens.json`, the design tokens this package owns (`--check` reports stale
  outputs; `--upstream FILE` lists differences from another `tokens.json`).
- `uv run --frozen python Tools~/samples.py` rewrites each sample's scene, UXML and asmdef scaffolding (`--check` reports drift).
- `Tools~/web/build.sh OUT` rebuilds the design-system artifact's files (see `Documentation~/web-mirror.md`).
- `uv run --frozen python Tools~/handbook/build_handbook.py . docs` rebuilds the web kit (`Web~/dist`) and the GitHub Pages site (`docs/`).
- `uv run --frozen python Tools~/metas.py` writes `.meta` files (stable GUIDs) for new files; `--check` lists missing ones.

## Samples

Import from Package Manager → Samples. Both run on import (scene, UXML and panel settings included).

- **Gallery** builds every component in its main states from sample data.
- **BattleHud** is a portrait battle board with fake data that ticks: HUD band, boss bar, a manual queue whose ×
  cancels, four operative cards and the world layer.

## Provenance

Icons, the mark and the Veil palette come from the Veilbreak design system (same author). Fonts: Poppins
and Share Tech Mono (SIL OFL 1.1) and TeX Gyre Adventor (GUST Font License); see `Third Party Notices.md`.

### Route and merchant GUI

`RouteChoice`, `MerchantScreen` and `CreditsChip` consume the plain view models in `Core/Models.cs`; they never read
or mutate game state. Map confirmed transactions onto their events and rebind after success. Rarity extends through
`PerkTier.Epic` (violet shimmer); Rare is cyan. Gallery includes example data for both pages.

Use `GlitchText` with `Glitch.In/Out/Pulse`, and `Announcer` for short heads-up notices. Reduced Motion / Reduced
Flashes fall back to fades; transitions cancel cleanly on replacement and detach. `HudTop.WaveSlammed` is an optional
camera hook. The battle top row is pause, icon-only autoplay, wave/score, then speed; Power carries a credits chip.


## Tooling and GUI 0.5

Python generators have no runtime dependencies. Install uv, then run from the repository root:

```sh
uv sync --locked
uv run --frozen ruff check .
uv run --frozen ruff format --check .
uv run --frozen python -m unittest discover -s Tools~/handbook/tests
uv run --frozen python Tools~/motion.py --check
uv run --frozen python Tools~/tokens.py --check
uv run --frozen python Tools~/samples.py --check
uv run --frozen python Tools~/metas.py --check
uv run --frozen python Tools~/handbook/build_handbook.py . docs
```

Node 22 is required for generated JavaScript checks. `Tools~/web/build.sh OUT` remains a uv-backed compatibility entry point; Python subprocesses use the active project interpreter. Ruff configuration lives in pyproject.toml. Embedded verbatim JS/Markdown templates are linted without bulk reformatting.

Read the [complete audit](Documentation~/reuse-audit.md), [migration guide](Documentation~/migration-0.5.md), [type inventory](Tokens~/components.json) and [validation evidence](Documentation~/validation.md). Unity is the primary component contract; the web kit documents and demonstrates it. Families (overlays, effects, forms, surfaces, collections, details, screens and world) are independent of composition level.

Browser checks use a separate locked Node project:

```sh
npm ci --prefix Tools~/browser --ignore-scripts
Tools~/browser/node_modules/.bin/playwright install chromium
node Tools~/browser/run.cjs docs
```
