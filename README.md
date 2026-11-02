# veilbreak design system

Shared presentation packages for Veilbreak games. Each Unity package lives in its own folder under
`packages/` and is consumed by the games through pinned git dependencies:

```json
"com.veilbreak.gui": "https://github.com/veil-break/veilbreak-design-system.git?path=/packages/com.veilbreak.gui#<full-reviewed-commit>"
```

| package | responsibility |
| --- | --- |
| [`com.veilbreak.gui`](packages/com.veilbreak.gui/README.md) | tokens, fonts, icons, controls, screen views, hud, world labels, navigation, accessibility; the web kit (`Web~`) and the GUI Handbook (`docs/`) |
| [`com.veilbreak.feedback`](packages/com.veilbreak.feedback/README.md) | cue catalog and cue stage (set pieces, heavy-moment punch, target reticle), pooled emitters, telegraphs and light curtains, ground marks, screen impacts, cue audio voices, battle surfaces (pooled URP decals, projected plan lines, outline curtains, pooled one-shot prefabs), projected readouts (`Feedback.UI`), URP effects: overlay blur, gamma UI, cinematic pulse (`Feedback.URP`) |
| [`com.veilbreak.branding`](packages/com.veilbreak.branding/README.md) | the intro splash and its slice sound; the slash-cut mark (menu vector and `mark` icon) and its generator; the host supplies completion and navigation |
| [`com.veilbreak.rendering`](packages/com.veilbreak.rendering/README.md) | URP render features, volume overrides and shaders: anamorphic lens streaks, the full-screen signal glitch and its decaying transient, the visor barrel lens (camera pass plus the shared `Source`/`Shown` mapping), SRP Batcher-safe `MaterialPaint`; the host owns renderers, tiers, volumes and settings |
| [`com.veilbreak.environment`](packages/com.veilbreak.environment/README.md) | the veil cityscape: wet street, lit facades, props, sky and skyline, fog, embers, rain, pooled lamp lights, flickering fire lights (`FireLight`) and street themes, streamed on its own bounded `SegmentStreamer`; the host supplies distance and either a theme plus the per-segment seed or a pure per-index `ISegmentPlan` (0.4.0) |
| [`com.veilbreak.ar-hud`](packages/com.veilbreak.ar-hud/README.md) | UI framework that veilbreak-main hosts every screen with, kept as a permanent adapter over `gui`: `HudHost`/`HudView`/`HudNavigation`, `HudStage` boards and visor chrome, the cut-shape kit, HUD themes generated from gui's tokens and the Theme Workshop. Unity 7000.0 baseline; its motion, gauge, shape and press-glow primitives are adapters over `gui` (`VbRebuild`, `VbGauge`, `VbVectorShape`, `VbPressGlow`); `HudNavigation` stays its own keyboard/gamepad focus engine |
| [`com.veilbreak.presentation`](packages/com.veilbreak.presentation/README.md) | entry package: declares the four packages above at tested versions; no assets, no assembly. Root it in a game spec to get the whole stack |

Dependency graph (acyclic): `gui` ← `feedback`, `gui` ← `branding`; `environment` and `rendering` depend only on URP and Unity modules. `presentation` depends on all four. `ar-hud` depends on `gui` (its design tokens; Input System optional) and is not part of the entry package; neither is `rendering`, which only veilbreak-main consumes so far. Binaries are plain git blobs (no LFS), so a git
package resolves without git-lfs.

Games do not edit these manifests by hand. The [workspace tool](https://github.com/veil-break/.github-private/blob/main/docs/development/workspace.md)
writes reviewed git pins (`vb packages remote`) or local `file:` sources for package authoring
(`vb packages local --repo design-system`). Package-to-package dependencies are semantic versions; Unity does not allow
git dependencies inside packages, so consumers list every git source themselves.

## tooling

Python tooling is one locked uv project at the repository root (`pyproject.toml`, `uv.lock`). Package tools run from the
repository root or the package folder; uv and Ruff discover the root configuration by walking up:

```sh
uv sync --locked
uv run --frozen ruff check .
uv run --frozen python packages/com.veilbreak.gui/Tools~/tokens.py --check
uv run --frozen python packages/com.veilbreak.gui/Tools~/screen_kit.py --check
uv run --frozen python packages/com.veilbreak.ar-hud/Tools~/themes.py --check
uv run --frozen python packages/com.veilbreak.branding/Tools~/logo.py --check
dotnet run --project packages/com.veilbreak.ar-hud/Tests~/InputChecks/Veilbreak.ARHud.InputChecks.csproj
```

[`.github/workflows/deploy-pages.yml`](.github/workflows/deploy-pages.yml) validates the tooling and publishes the handbook.
See the [multi-repo architecture](https://github.com/veil-break/.github-private/blob/main/docs/architecture/multi-repo-refactoring.md)
for the package plan (gui, rendering, feedback, environment, branding, presentation, ar-hud).

`vendor~/veilbreak-screen-assets` (about 30 MB, plain git blobs) is the original screen asset pack from veilbreak-main's `assets/source-packs`: project-authored procedural vectors and meshes plus AI-generated plates, kept as source for the games' generators with its README and metadata. It sits outside `packages/` so no game's package cache copies it, and the `~` suffix keeps Unity from importing it even if it is ever opened as part of a project.
