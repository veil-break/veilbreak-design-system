# Changelog

## unreleased

- Moved from `veilbreak-arcade/packages` to `veilbreak-design-system/packages` with every file and GUID unchanged. Consume it from git at `?path=/packages/com.veilbreak.feedback`.

## 1.4.0 — 2026-11-02

Battle-surface helpers from veilbreak-main's battle VFX, generalised with the host's choices injected. The scripts keep their
GUIDs from main.

- `DecalPool` (`Feedback.URP`): pooled URP Decal Projectors, projected straight down and faded over the last third of their
  life; the oldest is evicted at the cap. The host sets the cap (`Initialize`), the draw distance and, for tests or replays,
  the `Clock`.
- `SurfaceIndicators` (`Feedback.URP`): plan lines projected through the world depth, one reused projector per segment, with a
  segment cap, a four-slot tint palette and the projected span (`Top`, `Bottom`) from the host.
- `CurtainBatch`: vertical light-curtain strips on ground outlines in one mesh, with `Bake` for builders and the shared
  `_CurtainPhase` drift (`Phase`). The host supplies the curtain material.
- `PooledEffect`: one-shot prefab instances that return to their pool through the particle Stop callback.

## 1.3.0 — 2026-11-01

- OverlayBlur volume and bounded two-pass Render Graph blur/dimming after world post-processing and before gamma UI composition.
- Half-resolution Standard / quarter-resolution Low scratch targets; zero weight bypasses the feature.
- GUI dependency updated to 0.5.0. Mobile shader compilation and GPU profiling remain unverified in this change.

## 1.2.0 — 2026-11-01

- Telegraphs restyled after Veilbreak: an over-bright solid rim with a soft hugging glow, a drifting hatch inside hostile
  footprints, a 20% wash inside allied previews and a heated windup front. `TelegraphRequest.curtain` raises a light
  curtain (vertical rays, dust) along the outline from a second shader pass; `TelegraphGeometry.Outline`;
  `GroundTelegraphs.CurtainStrips`. The curtain object is inactive while no request raises one.
- `ScreenImpacts`: four whole-screen impacts (shockwave refraction, radial zoom punch, chromatic split, tinted vignette),
  rendered by `CinematicPulseFeature`, which now also enqueues while an impact plays. `Motion`, `Flash` and `Gain` scale them.
- `EffectEmitters` frame budget raised to 560 Standard / 190 Low particles (was 420 / 150); tier caps are unchanged.

## 1.1.0 — 2026-10-30

- `EffectEmitters`: combat effects as ten shared particle systems (sparks, glow, embers, flame, smoke, rings, shards,
  heal crosses, splashes, flares) over one generated atlas (`T_FxAtlas`, `Tools~/fx_atlas.py`) and
  `FeedbackParticle.shader`. Set pieces (`EffectRoutine`): impact, explosion, muzzle, beam, cone, zone, dash, wall,
  mend, ward, charge, portal, pulse, reward, swap and splash, plus smolder, trail, mote, curtain and shell helpers.
  Particles live in a road-anchored space (`Drift`), caps are fixed per tier (Low ≈ 650 particles) and a per-frame
  budget drops pieces below priority 2 first.
- `GroundMarks` (scorch and crack marks, 96 Standard / 32 Low, oldest replaced) and `QuadBatch` (flat atlas quads in
  one dynamic mesh, one draw call); `LightFlashes` (pooled point-light flashes, capacity per tier); `AudioVoices`
  (eight 2D voices with repeat suppression and priority stealing).
- `CueCatalog.Cue` gains routine, magnitude, tint, shake, light, mark, sound and volume; `priority` now decides what
  survives a full budget. `Find(first, second, third)` resolves the most specific row. The catalog holds the
  particle, mark and hit-flash materials so player builds keep them.
- Twelve generated cue one-shots in `Runtime/Audio` (`Tools~/sounds.py`). Requires `com.unity.modules.audio`.

- World readouts moved to `com.veilbreak.gui` as `WorldLayer` (team bars with ghost and barrier, merged combat text, labels, markers, off-screen chips). `ProjectedReadouts` stays as a thin adapter over it; `Veilbreak.Feedback.UI` now references `Veilbreak.Gui` and the package depends on `com.veilbreak.gui` 0.2.0.
- `GroundTelegraphs`: bounded (64) pool of ground projections drawn by `Telegraph.shader` — circles, cones and capsule lanes with preview, windup countdown, active and eroding residue phases; reduced-motion freezes scrolling patterns.
- `TelegraphGeometry`: quad layout plus a C# mirror of the shader signed distance, so consumers can test drawn footprints against their own hit areas.
- `CueCatalog.telegraph` holds the material so player builds keep the shader.

## 1.0.0

Extract reusable runner, feedback and splash infrastructure; deliver runner-relative combat, versioned checkpoints and explicit dependency boundaries.
