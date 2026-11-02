# Changelog

## 0.3.0 — 2026-11-02

- `FireLight` from veilbreak-main (same GUID): flickers a fire's point light around its authored intensity, low frequency
  only. The host sets `FireLight.Still` for Reduced Motion instead of the component reading main's `FighterView.Still`.

## 0.2.0

- Moved from `veilbreak-arcade/packages` to `veilbreak-design-system/packages` with every file and GUID unchanged.
- No runner dependency. `SegmentStreamer` moved here from com.veilbreak.runner (namespace `Veilbreak.Cityscape`, same GUID) and uses `StreetModules.Length`. The per-segment seed is host-supplied through `SegmentStreamer.SeedFunction` on `StreetStreamer.Initialize` / `EnvironmentDirector.Initialize`; `SegmentStreamer.DefaultSeed` keeps the previous hash.

## 0.1.0 — 2026-10-30

First release: the veil cityscape for runner streets, built for phones.

- `CityKit` with 36 project-authored models baked from `Models~/*.glb` by `KitBuilder` (a small GLB reader, no glTFast),
  12 palette materials on one `City` shader, and street, light-pool, ember, rain, smoke, fog, sky and skyline materials.
- `Street` shader: wet asphalt and paving in road coordinates, lane paint, cracks, manholes, puddles, rain ripples, and
  reflections of the painted sky plus (Standard) the mirrored emissive pass.
- `VeilReflectionFeature`: Render Graph pass that draws registered emissive submeshes mirrored in the ground plane at a
  quarter of the resolution, blurs them vertically and publishes `_VeilReflectionTex`.
- `StreetModules`, `StreetStreamer`: seeded side blocks (facade floors with hashed window light, roofs, lamps, trees,
  kiosks, barricades, wrecks, fires, towers behind) that keep |x| < 6 m clear for combat, streamed on the runner's
  bounded `SegmentStreamer` without instantiating after set-up.
- `LightBudget` (8 or 2 pooled point lights), `Atmospherics` (fog sheets, embers, rain, fire rigs), `StreetTheme` and
  `EnvironmentDirector` (8 s theme blends, trilight ambient, fog, moon, shader globals).
- `Tools~/textures.py` generates `T_Noise`, `T_Glow` and `T_LensDirt_BC`.
- Ambience: `Tools~/audio.py` synthesises seamless rain (stereo), city-hum and fire loops into `Runtime/Audio`;
  `CityKit` carries them, `EnvironmentDirector.Volume` levels a rain bed that follows the theme's rain and a low hum,
  and each pooled wreck fire crackles positionally. Requires `com.unity.modules.audio`.
- Standing water dims near-vertical sky reflections (`VeilReflectedSky`), so puddles stay dark under a high camera.
