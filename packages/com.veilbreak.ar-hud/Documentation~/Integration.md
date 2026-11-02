# Integration

## Responsibilities

| Asset or component | Owns |
|---|---|
| PanelSettings | Resolution scaling, render mode, sorting, the panel theme (import `Hud.uss` from it: `RuntimeTheme.tss` does) |
| PanelRenderer | The panel; it hands its root out only through its reload callback |
| HudHost | One `HudView` per panel root, built again whenever the panel builds a new root; theme and effects references; view style sheets; optional gallery or layout |
| HudView | The tree: `Root` (`.ar-root`, filling the panel) > `Content` (`.ar-content`, where screens build), then modal layers and the toast |
| HudNavigation | Screens, modals, toasts and keyboard / gamepad focus for one view |
| HudStage / HudAnchor | Board units, desktop / mobile board, safe area, left-handed mirroring |
| HudVisor | The stage's chrome, themed by the hosting view; optional app lens |
| HudTheme | Semantic colours, fonts, type size, cut corners, line weight, padding, target size, high contrast; `generatedFrom` when a tool writes it |
| HudEffects | Curvature, glow, parallax, scanlines, sweep, pulse, interference, refresh cap, reduced motion |
| HudViewModel | Base for observable presentation state; `HudModel` is the sample's |

## Host a game's UI

Put a `HudHost` on each `PanelRenderer` (it requires one). The host builds a `HudView` into the panel's root on the panel's first build and on every rebuild (a live UXML reload), and raises `Ready` each time: register the screens there, then show the first. Subscribe in `OnEnable`, call the handler at once if `host.View` is already up, and unsubscribe in `OnDisable`.

```csharp
private void OnView(HudView view)
{
    var nav = view.Navigation;
    foreach (var screen in screens) nav.Register(screen.id, content => Build(screen, content));
    nav.Back = () => nav.Current == "home" ? null : () => nav.Show("home");   // Escape / Cancel
    nav.Cue = cue => audio.Play(cue);                                         // "hover" (focus moved), "back"
    nav.Hub = "home"; nav.Animate = true;
    nav.Show(first);
}
```

- **Built screens** (`Register(id, Action<VisualElement>)`) are rebuilt into `Content` on every `Show`: the previous build is removed, persistent screens stay hidden. Showing the same id again rebuilds it in place (state changes, a language switch). With `Animate`, `HudMotion` animates between the builds: the whole content for an in-place rebuild, the element with the `HudNavigation.Shared` class (`ar-shared`, a nav bar both screens share) across screens.
- **Persistent screens** (`Register(id, VisualElement)`) are added once and shown or hidden.
- Focus after `Show`: the same element for an in-place rebuild, the remembered one when returning to `Hub`, otherwise the screen's `autofocus` element, otherwise its first focusable. `Leave` and `Arrive` run only while `Enabled`.
- `Route` overrides geometric moves per screen; `Mirrored` swaps Left and Right for left-handed layouts; `Hotkeys` (Input System) map keys to button names.
- `Enabled = false` makes the navigation ignore input and leave focus alone, and stands `HudPress` down: a battle HUD whose own keys rule until a sheet opens sets it false and back to true while a sheet is up.

Duplicate and unknown ids throw descriptive exceptions; `Has(id)` checks first. `Current` is the shown id and `ScreenChanged` reports changes.

## Modals and toasts

`OpenModal(body, flags, back)` puts the body on a layer above the content and below the toast. The layer copies the content's style sheets, so a body styled like the screens keeps their look, and sheets of its own still win. It closes with `CloseModal()`, when it is dismissed, when another screen is shown (dismissible ones) or when the body leaves the panel; focus then returns to what held it.

| Flag | Effect |
|---|---|
| `Dim` | The layer is a scrim (`modal-scrim`, Base.uss): it dims what is under it |
| `Dismissible` | Escape / Cancel closes it (or runs `back`); `Show` closes it |
| `Focus` | Focus moves into it when it opens |
| `OwnInput` | The body handles its own keys (an intro, a loading cover): navigation stands by while it is open, and focus is not scoped to it |
| `Block` | The layer takes the pointer over the whole panel |
| `Dialog` | `Dim | Dismissible | Focus | Block` |

Without `OwnInput`, keyboard and gamepad targets are scoped to the topmost modal. `Toast(message, ms)` shows one timed notice.

## Lay out on a stage

`<ar:HudStage>` is a fixed-height artboard scaled to the screen: 390 units on phones, 900 on desktops. Screens are laid out in their mockups' own units. The stage carries `mobile` or `desktop`, so USS can restyle per board, and `.only-mobile` / `.only-desktop` (Base.uss) show on one board alone. `<ar:HudAnchor left="24" top="10">` pins a block to the stage's edges with the safe area added; on the desktop board `d-left` / `d-top` / `d-right` / `d-bottom` take over. `Mirror` flips anchors for left-handed play. `Place`, `FromViewport` and `ToScreen` place elements over 3D content.

Each stage carries a `HudVisor`: the `HudChrome` rails and brackets under the board, themed by the hosting view's theme and effects (`HudTheme.Default` / `HudEffects.Default` outside a host). Only the newest attached visor shows, so an overlay stage does not stack a second chrome. An app can install:

- `HudVisor.Glitch`: the chrome's edge interference, 0 to 0.25 (a global glitch amount, say);
- `HudVisor.Lens` and `HudVisor.LensActive`: a full-panel filter for the eye focus (0..1 from the top-left), applied to the top of the panel's tree while active;
- `HudVisor.PinnedFocus`: where the eye rests in captures;
- `HudHost.ScreenToPanel`: the screen-to-panel mapping every host's panel uses, so input bends with a lens.

## Build with the kit

Attach the kit per screen, in this order: the app's tokens and base sheet, `Base.uss`, the app's board and face rules, `Kit.uss`, the app's kit faces, then screen sheets. `.ar-shape` (Base.uss) must outrank element-level button rules, and the kit sets no `-unity-font`: the app adds its faces for the kit classes in a sheet after each kit sheet. Colours come from `--vb-color-*` variables; `com.veilbreak.gui`'s `Runtime/Styles/Tokens.uss` defines them.

`HudKit` builds the pieces in code (`Panel`, `Press`, `Cta`, `Rail`, `IconButton`, `Switch`, `Segment`, `Bar`, `Tag`, `Text`, `Box`) and binds widgets declared in UXML (`BindToggle`, `BindSlider`, `BindChoice`, `BindSegment`, `BindSwitch`, `Click`, `SetText`). `HudIcon` draws glyphs from the installed `HudIcons` library (`HudIcons.Active`; UI Builder and edit-mode previews use the project's first library), tinted with the text colour. `HudScreenFrame` draws a screen's corner chrome; give it the `HudFx.Static` class so it never fades in.

`HudFx` effects run on the element scheduler in unscaled time: `Enter` (staggered entry of a board's children), `Glitch`, `AddSheen`, `Pulse`, `Burst`, `Shake`, `Flash`, `Slam` and `Tween`. `HudFx.Reduced` (the player's reduced motion) turns movement into fades and stills `HudPress` and `HudVisor`; `HudFx.NoFlash` caps flashes to a glow. `HudPress` (hooked by the navigation) gives every button a hover / focus glow tracing its outline and a press sink; a button picks its colour with `--neon`.

`HudInput` converts between bottom-left screen pixels and top-left panel units (`ScreenToPanel`, `SourceToPanel`, `PanelToScreen`) and tells whether a screen point is over controls (`IsOverControls`, elements with `blocks-arena-input`), so a game can ignore world input under the HUD.

## Hosting new screens

A new screen is a builder registered with the view's navigation; a sheet or card over a screen is a modal.

1. Author a UXML on `<ar:HudStage>` with `<ar:HudAnchor>` blocks, using kit classes and `ar:HudFrame` / `ar:HudButton` shapes, and list the kit sheets as above.
2. Keep its state in a `HudViewModel` subclass (`Set(ref field, value)` notifies by property name) and bind with UXML `DataBinding`s, or rebuild it with `nav.Show(id)` after a change.
3. Register it from `Ready`: `nav.Register("crafting", content => craftingUxml.CloneTree(content))`, then bind in the builder.
4. Open details over a screen with `nav.OpenModal(card, HudModal.Dialog)`; `HudNavigation.Modal` on an element inside a board marks a scrim region for focus scoping.

## Bind a game snapshot

Your game assembly must reference `Veilbreak.ARHud`. Write view-model properties on Unity's main thread from your presenter, after receiving a simulation snapshot. The sample host's `Model` is a `HudModel`:

```csharp
host.Model.Integrity = normalizedSuitHealth;
host.Model.Objective = localizedObjective;
host.Model.Ammunition = formattedAmmo;
```

Properties notify only when changed. For another source, set a screen's `dataSource` to your own `HudViewModel`.

## Animate rebuilt screens

Screens that are cleared and cloned on every change can still animate. Call `var before = HudMotion.Capture(scope)` before clearing, build the new tree, then `HudMotion.Play(scope, before, reducedMotion)` (`HudNavigation.Animate` does this around `Show`). Elements are matched by their path of names (type and sibling index when unnamed), so give stable names to anything that should carry across: only named elements slide when they move. A changed number counts to its new value; other changed text fades in, or decodes left to right on a label with the `HudMotion.Decode` class (`ar-decode`). Add the `HudMotion.Skip` class (`ar-no-motion`) to subtrees that animate themselves.

## Cut shapes

`HudShape.Attach(element)` paints any element as a chamfered shape from USS custom properties, so a screen's stylesheet owns every colour: `--cut` and `--flip` (which two corners are cut), `--fill` with an optional `--fill-to` / `--fill-dir` gradient, `--stroke` / `--stroke-width`, `--glow`, corner `--bracket`s, a `--ring` focus frame and optional glass: a `--sheen` top light, `--scan` lines every `--scan-pitch` px and a `--halo` bleed under the stroke. In UXML use `<ar:HudFrame class="...">` for panels and `<ar:HudButton class="...">` for buttons. A change of look (hover, selection, a class HudMotion replays) blends over 0.2 s. `.ar-shape` in Base.uss resets the geometry, glow, brackets, ring and glass, because custom properties inherit into nested shapes; `--fill` and `--stroke` inherit, so a nested shape without its own look shares its parent's. `HudShape.Outline(element, inset)` returns the painted outline for overlays that trace the same edge. The content box and hit area stay the element's rectangle.

## Themed controls and the gallery

`HudPanel`, `HudGauge`, `HudReticle` and `HudLoader` (`[UxmlElement]`) take the view's `HudTheme` inside `.ar-themed` subtrees (`HudView.ThemedClass`); there `HudStyling` also sets the text colour, size and face, the `ar-muted` / `ar-danger` / `ar-power` / `ar-heading` roles and plain button surfaces, and `.ar-high-contrast` when the theme is high contrast. Outside a themed subtree the controls keep their USS look (a `HudLoader` takes its USS `color`). Call `view.RefreshTheme()` after adding themed content. The gallery (`HudGallery`, shown by a host without a layout) is a themed sample on a `HudStage`.

## Change themes and effects

Assign assets on HudHost, install app-wide defaults (`HudTheme.Default`, `HudEffects.Default`) at boot, or edit them in the workshop. Inspector changes apply live. For script edits, call `theme.NotifyChanged()` or `effects.NotifyChanged()` after mutation. `theme.ApplyPreset(HudPalette.Veilbreak)` sets a complete palette (not on a generated theme). The visor never writes the shared assets: reduced motion and interference are overrides on its chrome. Clone shared assets if individual players need different settings.

A theme written by a tool records its source in `generatedFrom`; the inspector and the workshop show it read-only, and the workshop's preset dropdown is disabled for it. Edit the source and regenerate, or save a copy from the workshop. `HudWorkshop.PreviewStyles` lets an app preview with its own sheets; otherwise the workshop uses `Hud.uss`, gui's `Tokens.uss`, `Base.uss` and `Kit.uss`. The package's own `HudTheme_Veil` and `HudTheme_Contrast` are generated from gui's tokens by `Tools~/themes.py`.

## Optional Unity 7000 filters

`Runtime/Styles/OptionalGlass.uss` is deliberately not attached by default. Add it to your custom tree and add `.ar-glass` or `.ar-optical-glow` only to small, appropriate elements. Remove these classes for high-contrast or reduced-effects modes. Backdrop filtering requires a screen-space overlay panel and can incur intermediate render target costs. Avoid applying a filter to the full screen except through a lens you control.

## Input and scaling

The package uses UI Toolkit pointer, navigation and key events. With the Input System installed, `AR_HUD_INPUT_SYSTEM` (an asmdef version define) adds Space and `Hotkeys` polling and the visor reads the screen pointer; without it the visor follows the panel's pointer events. Use one event system.

The package uses panel-space coordinates, not physical screen pixels. `HudStage` adds `Screen.safeArea` to its anchors. Check portrait and ultrawide layouts for your product.

## Packaging and dependencies

The runtime assembly uses UnityEditor only inside `#if UNITY_EDITOR` preview fallbacks (the editor icon library). The editor assembly is Editor-only. Runtime assets are referenced explicitly; there are no Resources.Load calls or network requests. The app installs its icon library, default theme and effects, lens and glitch hooks at boot. Tests are opt-in.
