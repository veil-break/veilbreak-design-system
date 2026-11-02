# Changelog

## 3.1.0

Consolidation over `com.veilbreak.gui` (veilbreak-design-system#13). Public API, script GUIDs and UXML element names are
unchanged; the package references the `Veilbreak.Gui` assembly and needs gui 0.6.0.

- `HudMotion` is an adapter over gui's `VbRebuild`: same `Capture`/`Play`/`Snapshot`, same `ar-no-motion` and `ar-decode`
  classes. The engine and its tests moved to gui.
- `HudGauge` paints with gui's `VbGauge.Draw` and takes its clamping and read-out text from `VbGauge`. It keeps its own `Value`
  and `Segments` attributes and the `ar-gauge` classes, so data bindings and UXML are unchanged. `HudPaint` forwards to `VbPaint`.
- `HudShape` is the HUD's vocabulary over gui's `VbVectorShape`: the bare `--cut`, `--fill`, `--stroke` ... properties and the
  `ar-shape` class are read exactly as before, and `Outline`, `Linear` and `Attach` keep their signatures. `HudFrame` and `HudButton`
  stay here as the UXML elements. `HudPress` is an adapter over `VbPressGlow` with the `--neon` property, the `kbd` class and
  `HudFx.Reduced`.
## 3.0.0

- Depends on `com.veilbreak.gui`, the design-token authority.
- Removed `KitTokens.uss` and `Tools~/kit_tokens.py`. The demo host and the Theme Workshop now attach gui's
  `Runtime/Styles/Tokens.uss`.
- Added `Runtime/Themes/HudTheme_Veil.asset` and `HudTheme_Contrast.asset`. `Tools~/themes.py` generates them from gui's
  `Tokens~/tokens.json` (`--check` names stale outputs). They moved from veilbreak-main's `Features/UI/Hud` with their
  GUIDs unchanged and are byte-identical apart from `generatedFrom`.

## 2.0.0

- Moved from `veilbreak-main/packages` to `veilbreak-design-system/packages` with every file and GUID unchanged. Consume it
  from git at `?path=/packages/com.veilbreak.ar-hud`; the Unity paths (`Packages/com.veilbreak.ar-hud/...`) stay the same.
- `KitTokens.uss` is now a package-owned snapshot of the defaults that veilbreak-main's `vb tokens` used to write.
  `Tools~/kit_tokens.py --check` verifies that it defines exactly the `--vb-color-*` variables `Base.uss` and `Kit.uss`
  read.
- `Tests~/InputChecks` runs the `HudStage` board-geometry and `HudInput` pick-blocking checks on .NET without the editor.
- `HudFlame` (`[UxmlElement]`): realistic fire along an element's bottom edge, played from a flame flipbook given in USS (`--hud-flame-flipbook`, `--hud-flame-grid`), with a flickering heat glow and embers rising above the element. `HudFx.Reduced` holds one still frame without embers.
- Library glyphs take their colour again. `HudIcon` tints a glyph in `CustomStyleResolvedEvent`, which UI Toolkit raises only for an element with custom properties of its own, so every glyph was drawn in its white ink. `Base.uss` now gives `.icon` an `--icon` property.
- `HudIcon` also re-applies its colour when a colour transition on it ends: a library glyph is re-tinted and a stroked glyph is repainted. Before, the glyph kept the colour that the transition started from.

## 2.0.0

The package becomes the UI framework for all of a game's UI. Veilbreak's menu screens, intro, battle HUD, outcome sheet and scene-transition cover are now hosted by it.

**Hosting**

- `HudHost` requires a `PanelRenderer` in place of a `UIDocument`. It builds a `HudView` into the panel root on every new root and raises `Ready`.
- The theme and effects fall back to `HudTheme.Default` and `HudEffects.Default`, the app-wide profiles installed at boot.
- `HudHost.ScreenToPanel` installs one screen-to-panel mapping on every host's panel.
- The safe-area inset and the compact layout are gone: the safe area belongs to `HudStage` anchors.

**HudView**

- `Root` (`.ar-root`) holds `Content` (`.ar-content`), then the modal layers and the toast.
- `HudView.Of(element)` finds the hosting view.
- The theme reaches only `.ar-themed` subtrees (`ThemedClass`) and themed controls. Screens with their own sheets keep their look.

**HudNavigation** (merges the game's `MenuNav`)

- Screens:
  - Built screens: `Register(id, Action<VisualElement>)` rebuilds the screen into the content on every `Show`. Persistent screens remain.
  - `Show` animates with `HudMotion` (`Animate`, the `ar-shared` element).
  - `Show` places focus: the same element for an in-place rebuild, the remembered one on the `Hub`, otherwise the `autofocus` element.
- Input:
  - Arrows, WASD and the d-pad move focus geometrically or along the screen's `Route`. Tab cycles and Space presses.
  - Escape and Cancel close a dismissible modal or run `Back`.
  - Also: `Mirrored`, `Cue`, `Hotkeys` (Input System) and `Enabled`.
- Modals:
  - `OpenModal(body, HudModal flags, back)` stacks modals (`Dim`, `Dismissible`, `Focus`, `OwnInput`, `Block`, `Dialog`). Each layer carries the content's style sheets.
  - Focus is scoped to the topmost modal or scrim.
  - **Breaking:** a modal no longer disables the content under it; use `Block`.

**Moved from the game into the package, generalised**

- `HudStage`, `HudAnchor` and `HudVisor` (was `Stage`, `Anchor`, `Visor`). The visor's glitch, lens and pinned focus are app hooks (`HudVisor.Glitch`, `Lens`, `LensActive`, `PinnedFocus`). It never writes the shared effects asset.
- A new `HudStage` starts on the last laid-out board (`MobileBoard`) with its `.mobile`/`.desktop` class already set, so a screen rebuilt in place no longer builds its phone layout first on a desktop board (and `HudMotion` no longer diffs the real values against that half-built tree).
- `HudStage.FromViewport` maps a viewport point straight onto the panel's root instead of through `Screen.width`/`height`, so world-anchored elements line up when the panel draws into a render texture (the same result on a full-screen panel).
- `HudKit` (was `UiKit`), `HudFx` (was `UiFx`), `HudPress` (was `ButtonFx`), `HudInput`, `HudIcon` and `HudIcons` (was `Icon` and `IconLibrary`; same asset GUIDs), and `HudScreenFrame`.
- `HudViewModel`, the base for observable view-models. `HudModel` derives from it.

**Style sheets**

- `Base.uss` is new: the kit's layout, the shape reset (moved from `Hud.uss`), focus rings, panels, presses and `fx-*` layers.
- `Kit.uss` moved from the game and no longer sets a typeface; apps add their faces after it.
- `KitTokens.uss` holds default `--vb-color-*` values for projects without a token sheet.
- `Hud.uss` is theme level: it scopes button styling to `.ar-themed` and keeps the scrim and toast.

**Editor**

- The Theme Workshop opens on the installed default profiles. It previews with `HudWorkshop.PreviewStyles`, which falls back to the package sheets.
- A theme with `HudTheme.generatedFrom` set is read-only in the workshop and the inspector, and the preset dropdown cannot overwrite it. A saved copy is editable.
- Create Demo makes a `PanelRenderer` host with the kit sheets, under `HudSetup.AssetFolder` (default `Assets/ARHud`).

**Assembly and tests**

- The optional Input System is wired through the `AR_HUD_INPUT_SYSTEM` version define.
- Tests added for modal layers and sheets, dismissible modals versus `OwnInput`, built screens, `HudView.Of` and view-model notifications.

## 1.3.0

- `HudShape`: optional glass layers again, without the visor wrap: `--sheen` (top light fading to clear at half height), `--scan` / `--scan-pitch` (scanlines clipped to the chamfers) and `--halo` (two soft strokes under `--stroke`). They blend between looks like the colours, and `.ar-shape` resets them.

## 1.2.0

- `HudShape`: chamfered vector shapes for any element, painted from USS custom properties (`--cut`, `--flip`, `--fill`, `--fill-to`, `--fill-dir`, `--stroke`, `--stroke-width`, `--glow`, `--bracket*`, `--ring*`), with a 0.2 s blend between looks and `Outline` for overlays that trace the same edge. `HudFrame` and `HudButton` are its UXML panel and button; `Hud.uss` resets the properties on `.ar-shape`. (Moved from the Veilbreak game's `CutShape`, without its visor wrap, sheen, scanlines and projector halo.)
- `HudMotion`: changed text fades in instead of decoding; labels opt in to the decode with `ar-decode` (`HudMotion.Decode`). Only named elements slide when they move, since unnamed ones are matched by sibling index and would slide from another row's place after a list changes. New elements' fade-in stagger is capped at 90 ms.
- `HudChrome`: the top tick ruler follows `HudEffects.edgeOpacity` like the rails and brackets (it was drawn at a fixed 40%).

## 1.1.0

- `HudLoader`: an indeterminate loader, a wire icosahedron whose 30 edges light one at a time along a traced path (six continuous strokes); after each lit edge the solid turns 60° counter-clockwise (ease-snap, about an axis tipped toward the viewer), and a full trace fades and restarts. Reduced motion keeps the trace without the turns. Themed by the accent, or the resolved USS colour when unthemed.
- `HudMotion`: animates the difference between two builds of the same UI (capture, rebuild, play): changed class lists replay from the old classes so transitions run, numbers count, other text decodes, moved elements slide, resized bars grow, new or newly shown elements fade in. Reduced motion keeps colour and opacity changes.

## 1.0.0

Initial standalone UI Toolkit package: theme/effects profiles, vector controls, navigation, bound gallery, editor workshop, setup command and integration sample.

Intake into the veilbreak repository (2026-10-22), before any release, to the engineering manual's package rules (chapter 08):

- Assemblies are `autoReferenced: false` with `rootNamespace`, and compile with `-warnaserror+` (`csc.rsp`); the modal's Tab handling uses `FocusController.IgnoreEvent` in place of the obsolete `EventBase.PreventDefault`.
- `HudHost` exposes its configuration through serialized private fields with tooltips and read-only properties; it no longer sets an execution order.
- Menus live under `Veilbreak/AR HUD/`; the workshop finds the package through `PackageInfo` instead of a fixed path; setup writes theme assets to `Assets/_Veilbreak/Features/UI/Hud/`.
- `Third Party Notices.md`, `documentationUrl` and `Documentation~/Provenance.md` (archive and per-file hashes) are added.
- `PanelRoutesChildrenIntoBody` checks `hierarchy.parent`: Unity 7000 reports the logical parent through `parent`.
- Compiled and EditMode-tested in Unity 7000.0.0a10 through `testables`.
