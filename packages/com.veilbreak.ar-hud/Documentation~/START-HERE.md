# Veilbreak AR HUD

Standalone Unity **7000.0+** UI Toolkit package for transparent visor HUDs, menus and in-game panels. Version 1.0.0. No external runtime packages, render-pipeline dependencies, remote assets or services.

## Install in two minutes

1. Extract the delivery ZIP. In Unity, open **Window > Package Management > Package Manager**, then **+ > Install package from tarball** (wording can vary by editor), and select `com.veilbreak.ar-hud-1.0.0.tgz`.
2. Alternatively, install from disk using `source/com.veilbreak.ar-hud/package.json`, or copy that whole folder into your project's `Packages/`. Use only one installation method.
3. Choose **Tools > Veilbreak AR HUD > Create Demo in Scene**. This adds a UIDocument and HudHost to the current scene and creates editable theme, effects and PanelSettings assets under `Assets/ARHud`. Save the scene.
4. Open **Tools > Veilbreak AR HUD > Theme Workshop**. Choose a palette, inspect a theme/effects asset, and tune it beside the live runtime preview. Temporary profiles can be saved as project assets.
5. Enter Play mode. Deploy opens the battle HUD; Pulse/Repair change bound integrity; Loadout opens a selectable list; System Status opens a dismissible dialog.

The demo renders over your existing camera output. The workshop has its own dark background. No gameplay, camera, render pipeline or global input configuration is changed. Scene-object creation is Undoable; generated assets are kept and never overwrite existing assets.

## Included

- Four palettes: cyan visor, Veilbreak semantic colors, amber, high contrast.
- Separate ScriptableObject theme and effects profiles; live asset changes and Undo in editor.
- UI Builder controls: `HudPanel`, `HudGauge`, `HudReticle` (`[UxmlElement]`).
- Cut-corner glass panels, segmented radial meters and vector reticles.
- Curved visor rails, layered edge glow, scanlines, sweep, pulse, pointer parallax and optional local interference.
- Screen navigation, one modal at a time, keyboard Tab cycling/Escape, focus restoration and timed toasts.
- Native runtime data bindings with an observable example model.
- Screen-space safe-area padding and compact layout below 1000 panel points.
- Editable UXML telemetry sample and EditMode tests.

## Important scope

This is a screen-space UI Toolkit package, not an XR tracking system. Curvature shapes the vector visor rails; it does not bend the entire UI onto a 3D mesh. The glow is layered translucent vector strokes. No HDR bloom or camera capture is required. Text and hit targets stay flat and stable. Full cylindrical world-space interaction and scene-depth occlusion are not implemented.

Targeted against the official Unity 7000.0 documentation available on 21 October 2026, which is marked Alpha. A Unity editor is not installed in the authoring environment: **Unity compilation, generated UXML code, rendering, tests and player builds have not been executed here**. The archive and structured files were checked locally. Run the supplied tests and acceptance checklist in your exact alpha before shipping.

See [the integration guide](Documentation~/Integration.md), [validation](Documentation~/Validation.md), and [design notes](Documentation~/Design.md).
