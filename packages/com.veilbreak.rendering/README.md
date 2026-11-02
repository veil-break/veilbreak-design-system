# com.veilbreak.rendering

URP render features, volume overrides and shaders shared by the Veilbreak games. Every type reads only Unity, URP and its own
state. The host owns the renderer assets, quality tiers, volume profiles and settings, and it wires the hooks below.

| type | what it does | host wiring |
| --- | --- | --- |
| `LensStreaks` + `LensStreakFeature` | anamorphic lens streaks from HDR highlights (a horizontal and a tilted streak), drawn just before URP's post stack | add the feature to each renderer, assign `Runtime/Shaders/SH_VeilStreaks.shader` (`Hidden/Veilbreak/Streaks`) to its `shader`, then set the override on a volume profile |
| `Glitch` + `ScreenGlitchFeature` + `ScreenGlitch` | the full-screen signal glitch as a volume override; `ScreenGlitch.Kick(strength)` starts a decaying transient | add the feature with a material on `SH_VeilGlitch.shader` (`Veilbreak/Glitch`) after post-processing with `fetchColorBuffer`; call `ScreenGlitch.Tick(unscaledDelta)` each frame and set a glitch volume's weight from `ScreenGlitch.Changed`; set `Still` (reduced motion) and `NoFlashes` |
| `VisorLens` + `VisorLensFeature` | one invertible barrel mapping (`Source`, `Shown`) for the camera image, the UI and pointer input | set `VisorLens.Barrel` (0 is the identity) and `FirstQuality` (lower quality levels stay flat); add the feature to the renderers that bend and assign `SH_VisorLens.shader` (`Hidden/Veilbreak/VisorLens`); pass `VisorLens.Source` to UI Toolkit and to raycasts |
| `MaterialPaint` | per-renderer colours and floats that keep the renderer in the SRP Batcher: `Shared` instances counted per value set, `Owned` instances for per-frame values | call `Return` / `Discard` when the renderer goes away |

Assembly: `Veilbreak.Rendering`. Unity 6000.3 with URP 17.3 is the baseline; veilbreak-main consumes it on 7000.0 with URP 17.7.
Package tests are in `Tests/Editor` (`Veilbreak.Rendering.Tests`); list the package in the consumer's `testables` to run them.

Install from git with a reviewed commit,
`"com.veilbreak.rendering": "https://github.com/veil-break/veilbreak-design-system.git?path=/packages/com.veilbreak.rendering#<full-reviewed-commit>"`.
Veilbreak games get this entry from `vb packages remote`; use `vb packages local --package com.veilbreak.rendering` to edit
this folder from a game.

Moved from `veilbreak-main/unity/Assets/_Veilbreak/Presentation/Scripts/World` and `Shaders` with every script and shader GUID
unchanged, so renderer assets and volume profiles that serialize these types keep resolving.
