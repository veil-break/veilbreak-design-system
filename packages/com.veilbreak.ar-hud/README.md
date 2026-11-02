# Veilbreak AR HUD

A Unity **7000.0+** UI Toolkit framework for game UI. Version 2.0.0. A `HudHost` owns a `PanelRenderer`'s panel. Its `HudView` and `HudNavigation` host every screen, modal and toast. The package also provides:

- boards that scale to the device (`HudStage`), with visor chrome
- the cut-shape kit, press feedback and effects
- themes and effects profiles, with a live editor workshop.

It needs no runtime packages, render-pipeline features, remote assets or services. The Input System is optional: when `com.unity.inputsystem` is installed, the `AR_HUD_INPUT_SYSTEM` define turns on hotkeys and Space polling, and the visor follows the screen pointer.

Veilbreak hosts all of its game UI with this package: the menu screens, the intro, the battle HUD, the outcome sheet and the scene-transition cover.

## Install

- **From git.** Add `"com.veilbreak.ar-hud": "https://github.com/veil-break/veilbreak-design-system.git?path=/packages/com.veilbreak.ar-hud#<full-reviewed-commit>"` to `Packages/manifest.json`. Veilbreak games let the workspace tool write it (`vb packages remote`).
- **From disk.** Add `"com.veilbreak.ar-hud": "file:<path>/com.veilbreak.ar-hud"` to `Packages/manifest.json` (`vb packages local --repo design-system`). To run its tests, also list it under `"testables"`.
- **Demo.** Choose **Veilbreak > AR HUD > Create Demo in Scene**. It adds a GameObject with a `PanelRenderer` and a `HudHost` showing the sample gallery: the menu, battle and loadout screens and a status dialog. It also creates editable theme, effects and PanelSettings assets under `Assets/ARHud`, or under the app's folder if its editor code sets `HudSetup.AssetFolder`.
- **Workshop.** Open **Veilbreak > AR HUD > Theme Workshop** to tune a theme and an effects profile beside a live preview of the gallery.

## Host screens

```csharp
[SerializeField] private HudHost host;

private void OnEnable() { host.Ready += OnView; if (host.View != null) OnView(host.View); }
private void OnDisable() => host.Ready -= OnView;

private void OnView(HudView view)
{
    var nav = view.Navigation;
    nav.Register("home", content => homeUxml.CloneTree(content));   // rebuilt on every Show
    nav.Register("inventory", content => BuildInventory(content));
    nav.Back = () => nav.Current == "home" ? null : () => nav.Show("home");
    nav.Show("home");
}

// Dialogs, intros, covers:
nav.OpenModal(dialog, HudModal.Dialog);        // dimmed, dismissible, focused, blocks the panel
nav.OpenModal(cover, HudModal.OwnInput);       // handles its own keys: navigation stands by
nav.Toast("LOADOUT SAVED");
```

| Type | Role |
|---|---|
| `HudHost` | MonoBehaviour on a `PanelRenderer`. Builds a `HudView` into the panel root whenever the panel builds or rebuilds it, then raises `Ready`. Falls back to `HudTheme.Default` and `HudEffects.Default`. Can install a screen-to-panel mapping on every host (`ScreenToPanel`). |
| `HudView` | The tree: `Root` (`.ar-root`), with `Content` (`.ar-content`) where the screens build, then the modal layers and the toast. `HudView.Of(element)` finds the hosting view. |
| `HudNavigation` | Screens, either persistent or rebuilt on every `Show` (built). Also modals (`HudModal` flags), toasts and keyboard/gamepad focus. Arrows, WASD and the d-pad move focus. Tab cycles. Space presses. Escape or Cancel closes a dismissible modal, otherwise it runs `Back`. Other members: `Route`, `Hotkeys`, `Mirrored`, `Hub`, `Animate` (`HudMotion` between builds), `Enabled` and `Cue`. |
| `HudViewModel` | Base class for observable view-models (`Set(ref field, value)` notifies by property name). `HudModel` is the sample. |
| `HudStage`, `HudAnchor` | A fixed-height artboard: 390 units on phones, 900 on desktops. It carries a `mobile`/`desktop` class, adds the safe area and mirrors for left-handed layouts. Anchors pin blocks to its edges in board units. |
| `HudVisor` | The stage's chrome (`HudChrome`), themed by the hosting view. Its edge interference follows `Glitch`. An app can install a full-panel `Lens` filter that follows the eye focus. |
| `HudKit`, `HudIcon`/`HudIcons`, `HudScreenFrame` | Builders for the kit's controls and binding helpers. Icon glyphs come from an icon library that the app installs at boot (`HudIcons.Active`). `HudScreenFrame` draws a screen's corner chrome. |
| `HudPress`, `HudFx`, `HudMotion`, `HudInput` | `HudPress` gives every button a hover/focus glow and a press sink. `HudFx` holds the screen-entry, glitch, sheen, pulse, burst, shake, flash and slam effects. `HudMotion` animates between two builds. `HudInput` converts between screen and panel coordinates and tests whether a point is over controls. |
| `HudShape`, `HudFrame`, `HudButton` | Chamfered vector shapes painted from USS custom properties. |
| `HudPanel`, `HudGauge`, `HudReticle`, `HudLoader` | Themed controls. They take the `HudTheme` inside `.ar-themed` subtrees. |
| `HudFlame` | Fire along the element's bottom edge: flipbook flame tongues (`--hud-flame-flipbook`, `--hud-flame-grid`), a heat glow and rising embers; still under reduced motion. |
| `HudTheme`, `HudEffects`, `HudStyling` | The palette, fonts and geometry; the chrome profile; and the theming of `.ar-themed` subtrees. A theme with `generatedFrom` set is read-only in the inspector and the workshop. |

The style sheets are:

- `Hud.uss`, theme level: the view's layers, the scrim and the toast. `RuntimeTheme.tss` imports it, or an app's own theme can.
- `Base.uss`: the kit's layout, the shape reset, focus rings, panels, presses and effect layers.
- `Kit.uss`: the `k-*` desktop components.
- The `--vb-color-*` variables come from `com.veilbreak.gui`'s `Runtime/Styles/Tokens.uss`, the Veilbreak design tokens. The demo host and the Theme Workshop attach it before `Base.uss`.

The themes `Runtime/Themes/HudTheme_Veil.asset` and `HudTheme_Contrast.asset` are generated from gui's `Tokens~/tokens.json` by `Tools~/themes.py` (`--check` names stale outputs). They are read-only in the inspector and the workshop.

The kit sheets set no typefaces. Attach `Base.uss` and `Kit.uss` per screen (a UXML `<Style>`), followed by the app's own face rules.

## Scope

This is a screen-space UI Toolkit framework, not an XR tracking system. The visor chrome is vector linework. A lens filter is the app's choice. Text and hit targets stay flat.

The package compiles and its EditMode tests run in Unity 7000.0.0a10 inside the veilbreak project. Run the tests and [the acceptance checklist](Documentation~/Validation.md) in your exact alpha before shipping.

See [the integration guide](Documentation~/Integration.md), [validation](Documentation~/Validation.md), [design notes](Documentation~/Design.md) and [the changelog](CHANGELOG.md).
