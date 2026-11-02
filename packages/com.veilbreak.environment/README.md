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

## Segment plan (0.4.0)

The legacy call above pushes one global theme. Plan mode replaces it with a pure function of the segment index, so a host can lay out districts
and know what the road ahead will look like before it arrives:

```csharp
sealed class DistrictPlan : ISegmentPlan {
    public SegmentDescriptor Resolve(long index) =>   // pure: same index, same descriptor, in any call order
        new SegmentDescriptor(index, seed: Hash(runSeed, index), family: 0, look: LookOf(index), role: RoleOf(index), landmark: 0, weather: 0, damage: 1);
}
StreetKitRegistry.Register(new EvacuationKit());          // optional, an IStreetKit with Family >= 1
var director = new GameObject("Street").AddComponent<EnvironmentDirector>();
director.Initialize(cityKit, looks, seed, Camera.main, high, plan: new DistrictPlan());
// every frame; no theme argument
director.Present(distanceMetres, presentationTime, delta, actionFocus, groundSpeed, reducedMotion);
```

- **Resolve once.** `SegmentStreamer` calls `Plan.Resolve(index)` only when a slot is (re)allocated for an index, and never re-resolves or re-dresses a slot while it
  holds that index, so a visible segment is never recoloured. `Resolve` must be pure and may be called by the host too (the director does not).
- **Window.** `SegmentStreamer.Behind` (2) and `Ahead` (6) are public constants; `Capacity` is `Behind + 1 + Ahead`. `FurthestAllocated` is the highest index any slot has
  been allocated for since `Initialize` (never decreases), so a host places a district boundary beyond it (for example `Ahead + 2` segments past the squad).
- **Looks.** `SegmentDescriptor.Look` indexes the `StreetTheme[]` passed to `Initialize`. The look at the squad is the look of the segment under `distance`; across a
  `SegmentRole.Transition` segment it blends from the previous segment's look to its own by how far `distance` is through that segment, not by `BlendSeconds`
  (which stays for the legacy path). The blend is a function of distance only. `delta = 0` freezes lights, air and fires.
- **Families.** A segment whose `Family` is not 0 and has a registered `IStreetKit` is built by that kit (`Build(in d, slot, high)`, `Release(slot)` when the slot is
  recycled; `TriangleBudget(high)` should stay within 25k Standard / 12k Low). The kit owns the slot: the built-in road, pavement and blocks of that slot are hidden.
  Family 0, and a family without a kit, uses the built-in blocks, chosen by the descriptor's `Seed` from the `Look`'s theme. Family 0 cannot be registered.
- **Crossroads.** `Junction` works in plan mode; the crossing, its arms and its corner blocks use the look and seeds of the crossroads' own segment, but the arm
  segments streamed in later keep what the plan dressed them with.
- **Reduced motion and pause.** `Present(..., reducedMotion)` is kept as `EnvironmentDirector.ReducedMotion`: rain is thinned, and the light budget's fires stop
  flickering. Set `FireLight.Still` as well for authored `FireLight` components. The legacy `Present(distance, theme, ...)` is unchanged and still works without a plan.

## Parts

| Part | Role |
| --- | --- |
| `CityKit` | Baked model meshes and their palette slots. One material per slot, plus the street and effect materials. |
| `StreetModules` | Builds one theme's road, sidewalks and eight side blocks from a seed. The blocks are facade floors with hashed window light, roofs, lamps, trees, kiosks, barricades, wrecks, fires and towers behind. |
| `SegmentStreamer` | Recycles nine connector-aligned 8 m segments around the presented distance and asks the street to fill each one with its host-seeded hash. It never advances progression. |
| `StreetStreamer` | Shows the blocks on the segments. It creates each segment's four parts on first show and instantiates nothing else afterwards except one crossroads set, built the first time a crossroads is laid out and reused for every later one. |
| `ISegmentPlan`, `SegmentDescriptor`, `IStreetKit`, `StreetKitRegistry` | The per-index plan API: a pure `Resolve(index)` gives each segment its seed, kit family, look, role, landmark, weather and damage. See below. |
| `LightBudget` | Point lights for the emitters nearest the action: 8 on Standard, 2 on Low. The lights fade in and out and follow their emitter. Fires flicker on the accumulated delta (still when paused) and hold steady under Reduced Motion. |
| `FireLight` | Flickers an authored fire's point light around its intensity, low frequency only. It runs on the clock `EnvironmentDirector` advances by the host's delta (`FireLight.Advance`), so a paused game holds every fire still; a host that never drives it falls back to `Time.time`. Set the static `FireLight.Still` for Reduced Motion. veilbreak-main's builders put it on their fire prefabs. |
| `Atmospherics` | Two fog sheets, an ember field, rain and a pool of fire rigs, each with a positional crackle. Counts are fixed per tier. |
| `EnvironmentDirector` | Blends themes over 8 s (legacy) or across a plan's Transition segment (plan mode), exposes `ReducedMotion`. Publishes shader globals, trilight ambient, fog, the moon and the camera background, and plays the rain and hum beds. |
| `ReflectionSet`, `VeilReflectionFeature` | The mirrored wet-street pass. Registered emissive submeshes are drawn mirrored at a quarter of the resolution and blurred vertically. |

## Rules

- **Combat lane.** Below 2.5 m, nothing other than the road, kerb and ground light pools is closer to the centre than
  |x| = 6 m. The simulation has no environment collision, and its actors stay within |x| ≤ 5.5 m. The EditMode tests
  check every module.
- **Road coordinates.** Street patterns use world x and distance along the road. `_VeilParams.x` is that distance,
  wrapped to 1024 m, which is a multiple of every pattern period. Patterns therefore run seamlessly across segments.
- **Budgets.** At most 25k triangles per segment on Standard and 12k on Low. A heavily ruined theme peaks around 13.6k on
  Standard. Low builds its own, lighter blocks (no roof clutter, corner towers or upper-floor facade detail) while keeping the
  Standard layout of lamps, fires and signs, and it has fewer lights, particles and fires and no mirror pass.

## Regenerating

| What | How |
| --- | --- |
| Meshes, materials and kit | **Veilbreak > Environment > Rebuild city kit**. In batch mode: `-executeMethod Veilbreak.Cityscape.Editor.KitBuilder.Build`. Existing assets are updated in place, so their GUIDs stay. |
| Procedural textures | From the repository root: `uv run --with pillow python packages/com.veilbreak.environment/Tools~/textures.py`. The output is deterministic. Pillow is not in the locked tools project, hence `--with`. |
| Ambience loops | From the repository root: `uv run --frozen python packages/com.veilbreak.environment/Tools~/audio.py`. Deterministic, seamless, standard library only. |

## Assets

- **Models.** The `Models~` GLBs come from the Veilbreak source project's city kit and are project-authored. Their
  provenance lives with that project, not in this repository.
- **Textures.** `T_MenuSky_BC` and `T_MenuSkyline_BC` are AI-generated plates from the same project, and
  `T_Asphalt_BC/N` come from the same project too. `T_Noise`, `T_Glow` and `T_LensDirt_BC` are generated by
  `Tools~/textures.py`.
- **Audio.** `A_Rain_Loop`, `A_CityHum_Loop` and `A_Fire_Loop` are synthesised by `Tools~/audio.py`.
- **Ownership.** See `LICENSE.md`.
