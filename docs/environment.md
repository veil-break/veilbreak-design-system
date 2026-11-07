# Environment guide

`com.veilbreak.environment` (version 0.5.1, namespace and assemblies `Veilbreak.Cityscape`, Unity 6000.3, URP 17.3) is the veil
cityscape for runner streets, built for phones. It streams a wet boulevard on its own bounded `SegmentStreamer` and has no game
or runner dependency: the host presents a distance every frame, and either a theme (legacy) or a pure per-index plan. The
namespace is not `Veilbreak.Environment` because that would hide `System.Environment` in every `Veilbreak.*` consumer. Source:
`packages/com.veilbreak.environment/Runtime`.

## Set up

```csharp
var director = new GameObject("Street").AddComponent<EnvironmentDirector>();
director.Initialize(cityKit, themes, seed, Camera.main, high: tier > 0);
// every frame (legacy mode, one global theme)
director.Present(distanceMetres, themeIndex, presentationTime, delta, actionFocus, groundSpeedMetresPerSecond, reducedMotion);
```

- `cityKit` is `Runtime/CityKit.asset`; `themes` are `StreetTheme` assets the consumer owns (`StreetTheme` is a `ScriptableObject`
  of sky, fog, ambient, moon, wetness, rain, reflection, lamp and ember values).
- Pass simulation time as `delta` so the street, its lights and its air pause and speed up with the game; `delta = 0` freezes
  lights, air and fires.
- `director.Volume` is the game's effects level (it levels the rain and hum beds and the positional fire crackle; master volume
  belongs on the AudioListener).
- `Initialize(..., segmentSeed: ...)` takes a `SegmentStreamer.SeedFunction`; the default is `SegmentStreamer.DefaultSeed`.
  `high` selects the Standard tier (rain ripples, the larger light, particle and fire budgets).

## The segment plan API (environment 0.4.0)

Plan mode replaces the global theme with a pure function of the segment index, so a host can lay out districts and know what the
road ahead will look like before it arrives.

```csharp
sealed class DistrictPlan : ISegmentPlan {
    public SegmentDescriptor Resolve(long index) =>   // pure: same index, same descriptor, in any call order
        new SegmentDescriptor(index, seed: Hash(runSeed, index), family: 0, look: LookOf(index), role: RoleOf(index), landmark: 0, weather: 0, damage: 1);
}
StreetKitRegistry.Register(new EvacuationKit());          // optional, an IStreetKit with Family >= 1
director.Initialize(cityKit, looks, seed, Camera.main, high, plan: new DistrictPlan());
// every frame; no theme argument
director.Present(distanceMetres, presentationTime, delta, actionFocus, groundSpeed, reducedMotion);
```

| type | contract |
| --- | --- |
| `ISegmentPlan` | `SegmentDescriptor Resolve(long index)`. Called by `SegmentStreamer` once when a slot is allocated for an index and never re-resolved while the slot holds it; the same index must always give the same descriptor, whatever was resolved before. The host may call it too |
| `SegmentDescriptor` | read-only struct: `Index`, `Seed` (picks the built-in side blocks), `Family` (0 is the built-in boulevard, anything else a registered kit), `Look` (index into the director's `StreetTheme[]`), `Role`, `Landmark` (0 none; otherwise an id the family's kit understands), `Weather`, `Damage`, `Flags` (reserved) |
| `SegmentRole` | `Straight`, `Transition`, `Landmark`, `Junction`. Across a `Transition` segment the look blends from the previous segment's `Look` to its own by how far `distance` is through that segment (a function of distance only; `BlendSeconds`, 8 s, stays for the legacy path) |
| `IStreetKit` | a game-owned street family: `Family`, `TriangleBudget(bool high)` (stay within 25k Standard and 12k Low), `Build(in SegmentDescriptor, Transform slot, bool high)`, `Release(Transform slot)`. The kit owns the slot; the built-in road, pavement and blocks of that slot are hidden |
| `StreetKitRegistry` | process-wide table by family: `Register` (throws for family 0, replaces an earlier kit), `Unregister`, `Clear`, `Find`, `Budget`, `Count` |

`SegmentStreamer` holds `Behind` (2) and `Ahead` (6) as public constants, `Capacity = Behind + 1 + Ahead` and `Length` (8 m, the
size the street meshes are built for, so a host that aligns gameplay to segments must use the same length). `FurthestAllocated` is the
highest index any slot has been allocated for since `Initialize` (it never decreases), so a host places a district boundary
beyond it. `TryDescriptor(index, out descriptor)` reads the descriptor while its slot still holds the index. `Apply` and
`Retire` are the per-segment dressing hooks. A family 0 segment, or a family with no registered kit, uses the built-in blocks
chosen by the descriptor's `Seed` from the `Look`'s theme. `Junction` works in plan mode: the crossing, its arms and its corner
blocks use the look and seeds of the crossroads' own segment, but arm segments streamed in later keep what the plan dressed them with.

## The street and its modular kits

| part | role |
| --- | --- |
| `CityKit` | baked model meshes and their palette slots (`Runtime/CityKit.asset`); one material per slot plus the street and effect materials. Rebuilt in the editor with *Veilbreak > Environment > Rebuild city kit* (or `-executeMethod Veilbreak.Cityscape.Editor.KitBuilder.Build` in batch mode); existing assets are updated in place so GUIDs stay. The source GLBs are in `Models~` (not imported by Unity) |
| `StreetModules` | builds one theme's road, sidewalks and eight side blocks from a seed: facade floors with hashed window light, roofs, lamps, trees, kiosks, barricades, wrecks, fires and towers behind. A block is laid out for the right of an 8 m segment and turned 180 degrees for the left. Constants: `Length` 8, `RoadHalf` 6, `Curb` 0.15, `Building` 9.5. Light sources are `Emitter`s of kind `Lamp`, `Fire` or `Sign` |
| `StreetStreamer` | shows the blocks on the segments |
| `LightBudget` | point lights for the emitters nearest the action: `HighLights` 8, `LowLights` 2 |
| `FireLight` | flickers an authored fire's point light; runs on the clock the director advances (`FireLight.Advance`), so a paused game holds every fire still. Set the static `FireLight.Still` for Reduced Motion |
| `Atmospherics` | two fog sheets, an ember field, rain and a pool of fire rigs with positional crackle; counts are fixed per tier |
| `EnvironmentDirector` | blends themes over `BlendSeconds` (legacy) or across a Transition segment (plan mode); exposes `Current` (a `Look`), `BlendProgress` and `ReducedMotion`; publishes the shader globals (`_VeilParams`, `_VeilLight`, `_VeilSkyTint`, `_VeilHorizon` ...), trilight ambient, fog, the moon and the camera background; plays the rain and hum beds |

Rules the package keeps:

- **Combat lane.** Below 2.5 m nothing other than the road, kerb and ground light pools is closer to the centre than |x| = 6 m.
  The simulation has no environment collision and its actors stay within |x| of 5.5 m.
- **Budgets.** At most 25k triangles per segment on Standard and 12k on Low. Low builds its own lighter blocks and has fewer
  lights, particles and fires.
- **Road coordinates.** Street patterns use world x and the distance along the road, wrapped to 1024 m (`_VeilParams.x`), a
  multiple of every pattern period, so patterns run seamlessly across segments.
- **Reduced Motion.** `Present(..., reducedMotion)` is kept as `EnvironmentDirector.ReducedMotion`: rain is thinned and fires stop
  flickering.

## Reflections

The wet street's mirror is a render feature. Register `VeilReflectionFeature` (with `Runtime/Shaders/ReflectionBlur.shader` in its
`blur` field; `downsample` 2 to 8, default 4; `spread` 0 to 0.3) on every renderer that needs mirrors. It runs before opaques and draws the
registered emissive submeshes mirrored in the ground plane at a fraction of the screen resolution, blurred vertically, for Game and
Scene View cameras. Either tier can run it (the host picks the resolution, for example half on Standard and quarter on Low); without
it the puddles reflect only the painted sky.

- `ReflectionSet.Set(renderer, materials, submeshes, solid: false)` registers a renderer's submeshes; `solid: true` registers a full actor
  silhouette that draws afterwards into the same texture with a depth attachment (body detail and mutual occlusion). The materials
  must provide a `VeilReflection` pass with depth writing and the `_VeilMirrorClip` projection. A shader that exposes that pass must
  not declare a `FallBack` (0.5.1 removed it from `City.shader`): with one, `Material.FindPass` sees only the fallback's pass and
  nothing registers.
- `ReflectionSet.Remove(renderer)` when a renderer is destroyed or pooled; `Clear()` resets.
- The host owns actor materials, shadows and quality settings.

## Regenerating

| what | how |
| --- | --- |
| Meshes, materials, kit | *Veilbreak > Environment > Rebuild city kit*, or batch `-executeMethod Veilbreak.Cityscape.Editor.KitBuilder.Build` (needs a 6000.3 editor) |
| Procedural textures | `uv run --with pillow python packages/com.veilbreak.environment/Tools~/textures.py` (Pillow is not in the locked project; deterministic) |
| Ambience loops | `uv run --frozen python packages/com.veilbreak.environment/Tools~/audio.py` (deterministic, standard library only) |
