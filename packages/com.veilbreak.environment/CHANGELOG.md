# Changelog

## 0.4.0 — 2026-11-02

Additive per-index segment plan; the legacy `Present(distance, theme, ...)` path, its seeds and `SegmentStreamer.DefaultSeed` are unchanged except for the fire-timing fixes below (fires follow the host delta and reduced motion holds them steady). The director resets the shared fire clock when destroyed.

- `SegmentRole`, `SegmentDescriptor` (Index, Seed, Family, Look, Role, Landmark, Weather, Damage, Flags), `ISegmentPlan.Resolve(long)` (must be pure), `IStreetKit` and
  `StreetKitRegistry` (new `SegmentPlan.cs`).
- `SegmentStreamer`: public `Behind = 2` / `Ahead = 6` (`Capacity = Behind + 1 + Ahead`), `FurthestAllocated`, `Plan`, `Present(double distance)` plan mode, `TryDescriptor`.
  A slot is resolved and dressed once when allocated for an index and never again while it holds it.
- `StreetStreamer`: plan-mode `Present(double)`, `Plan`, `Stream`; segments of a registered kit family are built and released by the kit.
- `EnvironmentDirector`: `Initialize(..., ISegmentPlan plan = null)`, plan-mode `Present(distance, time, delta, focus, groundSpeed, reducedMotion)` that blends the look by
  distance across Transition segments, `Plan`, `BlendProgress`, `ReducedMotion`.
- Fixes: `FireLight` flickers on a clock the host advances by its delta (`FireLight.Advance`, driven by the director), so pause freezes it (still `Time.time` for hosts that never
  drive it); `LightBudget` fires flicker on the accumulated delta and hold steady under Reduced Motion (new optional `still` argument).
- README: Low builds its own lighter blocks; the crossroads set is instantiated lazily; the nonexistent `docs/presentation-provenance.json` reference is gone; the texture and audio
  regeneration commands run from the repository root; the plan API is documented. EditMode tests cover the plan, slot stability, transition blend, pause, reduced motion,
  kit budgets, the window constants and the unchanged legacy path.

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
