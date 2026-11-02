# Veilbreak Environment

The veil cityscape for runner streets, built for phones. It streams a wet boulevard with lit blocks, sodium lamps, bare
trees, wrecks, fires, embers, rain and fog on its own bounded `SegmentStreamer`, and blends between street themes.
It has no game or runner dependency: the host presents a distance and a theme each frame and may supply the
per-segment seed (`SegmentStreamer.SeedFunction`, passed to `EnvironmentDirector.Initialize`); the default is
`SegmentStreamer.DefaultSeed`. Segments are `SegmentStreamer.Length` (8 m) long, the size the street meshes are built for,
so a host that aligns gameplay to segments must use the same length.

Install from git with a reviewed commit, `"com.veilbreak.environment": "https://github.com/veil-break/veilbreak-design-system.git?path=/packages/com.veilbreak.environment#<full-reviewed-commit>"`. Veilbreak games get the entry from `vb packages remote`; use `vb packages local --package com.veilbreak.environment` to edit this folder from a game. The source models in `Models~` are not imported by Unity.
Namespace and assemblies: `Veilbreak.Cityscape`. (The package is not named `Veilbreak.Environment`, because that
namespace would hide `System.Environment` in every `Veilbreak.*` consumer.)

## Using it

```csharp
var director = new GameObject("Street").AddComponent<EnvironmentDirector>();
director.Initialize(cityKit, themes, seed, Camera.main, high: tier > 0);
// every frame
director.Present(distanceMetres, themeIndex, presentationTime, delta, actionFocus, groundSpeedMetresPerSecond, reducedMotion);
```

`cityKit` is `Runtime/CityKit.asset`. `themes` are `StreetTheme` assets that the consumer owns. Pass simulation time as
`delta`, so the street, its lights and its air pause and speed up with the game. Set `director.Volume` to the game's
effects level each frame (master volume belongs on the AudioListener): it levels the rain and hum beds and the
positional fire crackle. On the Standard tier, add
`VeilReflectionFeature` (with `Runtime/Shaders/ReflectionBlur.shader`) to the renderer. Without that feature, puddles
reflect only the painted sky.

## Parts

| Part | Role |
| --- | --- |
| `CityKit` | Baked model meshes and their palette slots. One material per slot, plus the street and effect materials. |
| `StreetModules` | Builds one theme's road, sidewalks and eight side blocks from a seed. The blocks are facade floors with hashed window light, roofs, lamps, trees, kiosks, barricades, wrecks, fires and towers behind. |
| `SegmentStreamer` | Recycles nine connector-aligned 8 m segments around the presented distance and asks the street to fill each one with its host-seeded hash. It never advances progression. |
| `StreetStreamer` | Shows the blocks on the segments. It creates each segment's four parts on first show and never instantiates after that. |
| `LightBudget` | Point lights for the emitters nearest the action: 8 on Standard, 2 on Low. The lights fade in and out and follow their emitter. |
| `Atmospherics` | Two fog sheets, an ember field, rain and a pool of fire rigs, each with a positional crackle. Counts are fixed per tier. |
| `EnvironmentDirector` | Blends themes over 8 s. Publishes shader globals, trilight ambient, fog, the moon and the camera background, and plays the rain and hum beds. |
| `ReflectionSet`, `VeilReflectionFeature` | The mirrored wet-street pass. Registered emissive submeshes are drawn mirrored at a quarter of the resolution and blurred vertically. |

## Rules

- **Combat lane.** Below 2.5 m, nothing other than the road, kerb and ground light pools is closer to the centre than
  |x| = 6 m. The simulation has no environment collision, and its actors stay within |x| ≤ 5.5 m. The EditMode tests
  check every module.
- **Road coordinates.** Street patterns use world x and distance along the road. `_VeilParams.x` is that distance,
  wrapped to 1024 m, which is a multiple of every pattern period. Patterns therefore run seamlessly across segments.
- **Budgets.** At most 25k triangles per segment on Standard. A heavily ruined theme peaks around 13.6k. Low uses the
  same blocks, with fewer lights, particles and fires, and no mirror pass.

## Regenerating

| What | How |
| --- | --- |
| Meshes, materials and kit | **Veilbreak > Environment > Rebuild city kit**. In batch mode: `-executeMethod Veilbreak.Cityscape.Editor.KitBuilder.Build`. Existing assets are updated in place, so their GUIDs stay. |
| Procedural textures | `uv run --project ../../tools python Tools~/textures.py`. The output is deterministic. |
| Ambience loops | `uv run --project ../../tools python Tools~/audio.py`. Deterministic, seamless, standard library only. |

## Assets

- **Models.** The `Models~` GLBs come from the Veilbreak source project's city kit and are project-authored. Their
  provenance is recorded in the repository's `docs/presentation-provenance.json`.
- **Textures.** `T_MenuSky_BC` and `T_MenuSkyline_BC` are AI-generated plates from the same project, and
  `T_Asphalt_BC/N` come from the same project too. `T_Noise`, `T_Glow` and `T_LensDirt_BC` are generated by
  `Tools~/textures.py`.
- **Audio.** `A_Rain_Loop`, `A_CityHum_Loop` and `A_Fire_Loop` are synthesised by `Tools~/audio.py`.
- **Ownership.** See `LICENSE.md`.
