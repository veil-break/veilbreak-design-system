# Shaders and render features guide

`com.veilbreak.rendering` (assembly `Veilbreak.Rendering`, Unity 6000.3, URP 17.3) holds the URP render features, volume
overrides and shaders both games can use. Every type reads only Unity, URP and its own state. The host owns the renderer
assets, quality tiers, volume profiles and settings. veilbreak-main consumes the package on 7000.0 with URP 17.7. Source:
`packages/com.veilbreak.rendering/Runtime`.

The shaders for the other packages are not covered here: `com.veilbreak.feedback` has its own URP effects (see
[feedback.md](feedback.md)) and `com.veilbreak.environment` has the street, city, sky and reflection shaders (see
[environment.md](environment.md)).

## Baseline and version differences

- The package targets Unity 6000.3 with URP 17.3 (`com.unity.render-pipelines.universal` 17.3.0 in `package.json`). Its passes
  are written against the Render Graph API (`RecordRenderGraph`, `UniversalResourceData`, `RenderGraphUtils.AddBlitPass`).
- There is no version-specific code today: no `versionDefines` in `Veilbreak.Rendering.asmdef` and no `#if` on the engine
  version in the C# or the shaders. If a 17.7 difference ever appears, isolate it behind `versionDefines` so the 6000.3 build
  is unchanged (workspace rule).
- Author and reserialize the package's assets in 6000.3; never open it in a 7000.0 editor.

## What is in it

| shader | render feature | volume override / state | what it draws |
| --- | --- | --- | --- |
| `Hidden/Veilbreak/Streaks` (`SH_VeilStreaks.shader`) | `LensStreakFeature` | `LensStreaks` | anamorphic lens streaks from HDR highlights |
| `Veilbreak/Glitch` (`SH_VeilGlitch.shader`) | `ScreenGlitchFeature` | `Glitch`, the static `ScreenGlitch` | the full-screen signal glitch |
| `Hidden/Veilbreak/VisorLens` (`SH_VisorLens.shader`) | `VisorLensFeature` | static `VisorLens` | the visor's barrel curve on the camera image |

`MaterialPaint` is not a shader: it keeps per-renderer colours and floats in the SRP Batcher (below).

## LensStreaks

A volume override (`Veilbreak/Lens Streaks`) read by `LensStreakFeature`, which draws just before URP's post stack
(`BeforeRenderingPostProcessing`), so bloom, grading and grain apply on top. It runs for Game and Scene View cameras with
post-processing on, and only while `intensity` is above 0. The shader's three passes are a soft-threshold prefilter into a
quarter-width, half-height target, one streak iteration (15 taps; three iterations at strides 1, 8 and 64) per direction,
and an additive tinted composite.

| property | default | meaning |
| --- | --- | --- |
| `intensity` | 0 | 0 switches the effect off |
| `threshold` | 1.1 | scene luminance where highlights start to streak |
| `length` | 0.992 (0.8 to 0.998) | per-texel falloff; closer to 1 is longer |
| `tint` | (0.45, 0.78, 1.35) HDR | streak colour |
| `angle` | 40 | the second streak's tilt, degrees counter-clockwise from horizontal |
| `secondary` | 0.45 | the tilted streak's strength relative to the horizontal one |
| `centerFade` | 0.8 | how much fainter highlights in the middle of the view streak |
| `centerRadius` | 0.65 | distance from the centre (0) toward a corner (1) where streaks are full strength again |

Shader inputs (`RenderIds`): `_Texel`, `_Unit`, `_Direction`, `_Stride`, `_Falloff`, `_Threshold`, `_CenterFade`, `_TintA`,
`_TintB` and the second texture `_StreakB`.

Host wiring: add `LensStreakFeature` to each renderer, assign `SH_VeilStreaks.shader` to its `shader` field, set the override
on a volume profile.

## Glitch and ScreenGlitch

`Glitch` (`Veilbreak/Glitch`) is a volume override with `intensity` (0 to 1: tearing and channel split, 0 leaves the image
untouched) and `flash` (0 to 1: brightness pop on the torn bands). `ScreenGlitchFeature` extends URP's
`FullScreenPassRendererFeature`: it sets the globals `_VeilGlitch` and `_VeilGlitchFlash` from the blended volume, then
runs the full-screen pass, so play never edits the material asset. It skips the pass for non-Game cameras, with post-processing
off, while `Glitch` is inactive, and whenever `ScreenGlitch.Still` or `ScreenGlitch.NoFlashes` is set.

`ScreenGlitch` is the transient:

- `Kick(strength)` raises the amount to at least `strength` (0 to 1); it does nothing while `Still`.
- `Tick(dt)` decays it (`1.6 + amount * 2.5` per second) and raises `Changed(amount)`; `Clear()` sets 0 and raises it too.
- The host sets the weight of a volume that holds a `Glitch` override from `Changed`.
- `Still` (Reduced Motion) holds the amount at 0; `NoFlashes` (Reduced Flashes) makes the feature skip the pass entirely.

Host wiring: add `ScreenGlitchFeature` with a pass material on `SH_VeilGlitch.shader`, injected after post-processing with
`fetchColorBuffer` on (settings of the URP feature); call `ScreenGlitch.Tick(unscaledDelta)` every frame.

## VisorLens

One invertible barrel mapping shared by everything that draws or reads the screen, so the scene, the UI and pointer input agree.
The centre is magnified by `1 + Barrel`, the rim compressed, the four corners fixed, so nothing leaves the screen.

- Host-set state: `VisorLens.Barrel` (the design token `fx.visor-barrel`; 0 is the identity everywhere) and
  `VisorLens.FirstQuality` (the first quality level that bends; lower levels stay flat). `Active` and `Strength` combine them.
- `Source(screen)` gives the source pixel shown at a screen pixel (use it to convert pointer positions before UI Toolkit picking
  or world raycasts); `Shown(source)` is the inverse (fixed-point iteration, six steps). Both have overloads taking a size and `k`.
- `VisorLensFeature` bends the camera colour after post-processing (`AfterRenderingPostProcessing`) with one blit through
  `Hidden/Veilbreak/VisorLens`, which reads `_VisorBarrel`. Only Game cameras tagged `MainCamera` with post-processing on bend,
  so cameras that render into textures stay flat. Add it only to renderers that should bend and assign `SH_VisorLens.shader`.
- The `visor-*` fx tokens in `com.veilbreak.gui` (`visor-barrel`, `visor-curve`, `visor-fringe`, `visor-bloom` are 0 today, so
  the screens are flat panels; `visor-keystone-max` is a cap of 8px) feed the host's settings. See [tokens.md](tokens.md).

## MaterialPaint

A `MaterialPropertyBlock` opts a renderer out of SRP batching, so in play mode `MaterialPaint` writes the values onto material
instances that share the source's shader variant:

- `Color(id, value)` and `Float(id, value)` collect values; `Clear()` resets the list.
- `Shared(renderer, index, source)` paints material `index` with the one instance every renderer showing the same values
  shares, counted per holder; hand it back with `MaterialPaint.Return(material)` (the last return destroys it).
- `Owned(renderer, index)` makes an instance only that renderer draws, for values that change every frame;
  `MaterialPaint.Discard(renderer)` destroys it with the renderer.
- In edit mode it writes a property block instead, so serialized materials are never replaced.

## Checks

`Tests/Editor/RenderingTests.cs` (`Veilbreak.Rendering.Tests`) runs in a consumer that lists the package in `testables`. Compile
and visual checks of the passes need a Unity editor; nothing in the Python tooling here exercises these shaders.
