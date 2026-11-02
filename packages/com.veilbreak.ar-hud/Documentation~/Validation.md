# Validation status and acceptance checklist

## Status

The package compiles with `-warnaserror+` and its EditMode tests run in Unity 7000.0.0a10 inside the veilbreak project (`testables`), where it hosts the game's whole UI; the menu screens are compared against the design captures there. Rendering on device, IL2CPP player builds and screen-reader behaviour are validated per product, not by the package.

## Run EditMode tests

Install Unity Test Framework in the host project. Add the following top-level entry to the project's `Packages/manifest.json`, preserving all existing content:

```json
"testables": ["com.veilbreak.ar-hud"]
```

Open **Window > General > Test Runner > EditMode**, then run Veilbreak.ARHud.Tests. For automation:

```sh
"$UNITY_EDITOR" -batchmode -nographics -projectPath "$PROJECT_PATH" \
  -runTests -testPlatform EditMode -testFilter Veilbreak.ARHud.Tests \
  -testResults "$PROJECT_PATH/ar-hud-tests.xml" \
  -logFile "$PROJECT_PATH/ar-hud-tests.log"
```

The tests cover finite/NaN gauge inputs, model and view-model notifications, panel child routing, duplicate/unknown routes, modal layers (above the content, carrying its sheets, the scrim class, picking only with Block), showing a screen closing dismissible modals but not OwnInput ones, built screens rebuilding into the content, finding a view from its elements, loader tracing, motion matching, shapes and outlines, and high-contrast preset reversibility.

## In-editor acceptance

1. Import without compilation errors or USS/UXML warnings. Confirm the custom elements (HudStage, HudAnchor, HudFrame, HudButton, HudIcon, HudScreenFrame, HudPanel, HudGauge, HudReticle, HudLoader) appear in UI Builder.
2. Create the demo in a saved test scene. Open menu, battle, loadout and system status; verify buttons and live integrity binding.
3. Tune theme/effect assets in Inspector and Workshop; test Undo, asset swaps and repeated window close/open. A generated theme (generatedFrom set) is read-only and its preset dropdown is disabled.
4. Check arrows, Tab/Shift+Tab, Space, Enter, pointer and touch. Open a dialog (HudModal.Dialog): the background cannot be pressed, focus stays in it, Escape closes it and focus returns. Verify the project's gamepad UI actions separately.
5. Toggle Reduced Motion: chrome should become static; toggle High Contrast: no decorative chrome, readable opaque panels.
6. Test 1920x1080, 1440x900, 844x390, ultrawide and a simulated notched display. Check the HudStage board (desktop/mobile), its safe area, wrapping and no cut-off controls.
7. Disable/re-enable HudHost and its PanelRenderer; reload a scene; enter/exit play with domain reload both on and off. Ensure one view remains, Ready fires once per new root and timers stop on removal.
8. Build a development player using the shipping backend (including IL2CPP). Confirm native bindings survive stripping and fonts render.
9. Profile representative mobile/desktop hardware with effects off/on. The package specifies a scheduler rate, not a measured frame-time or GC budget. Tune scanline spacing/glow and omit optional filters as necessary.
10. If using optional glass filters, verify correct screen-space panel mode and output on the target GPU. Test your own accessibility, localization and screen-reader integration.
