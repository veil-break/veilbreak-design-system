# veilbreak design system

Shared presentation packages for Veilbreak games. Each Unity package lives in its own folder under
`packages/` and is consumed by the games through pinned git dependencies:

```json
"com.veilbreak.gui": "https://github.com/veil-break/veilbreak-design-system.git?path=/packages/com.veilbreak.gui#<full-reviewed-commit>"
```

| package | responsibility |
| --- | --- |
| [`com.veilbreak.gui`](packages/com.veilbreak.gui/README.md) | tokens, fonts, icons, controls, screen views, hud, world labels, navigation, accessibility; the web kit (`Web~`) and the GUI Handbook (`docs/`) |
| [`com.veilbreak.feedback`](packages/com.veilbreak.feedback/README.md) | cue catalog, pooled emitters, telegraphs and light curtains, ground marks, screen impacts, cue audio voices, battle surfaces (pooled URP decals, projected plan lines, outline curtains, pooled one-shot prefabs), projected readouts (`Feedback.UI`), URP effects: overlay blur, gamma UI, cinematic pulse (`Feedback.URP`) |
| [`com.veilbreak.branding`](packages/com.veilbreak.branding/README.md) | the intro splash / slash-cut mark and its slice sound; the host supplies completion and navigation |
| [`com.veilbreak.environment`](packages/com.veilbreak.environment/README.md) | the veil cityscape: wet street, lit facades, props, sky and skyline, fog, embers, rain, pooled lamp lights and street themes, streamed on its own bounded `SegmentStreamer`; the host supplies distance, theme and the per-segment seed |
| [`com.veilbreak.ar-hud`](packages/com.veilbreak.ar-hud/README.md) | transitional UI framework that veilbreak-main hosts every screen with: `HudHost`/`HudView`/`HudNavigation`, `HudStage` boards and visor chrome, the cut-shape kit, themes and the Theme Workshop. Unity 7000.0 baseline; its navigation, shape, motion and gauge primitives consolidate into `gui` incrementally |
| [`com.veilbreak.presentation`](packages/com.veilbreak.presentation/README.md) | entry package: declares the four packages above at tested versions; no assets, no assembly. Root it in a game spec to get the whole stack |

Dependency graph (acyclic): `gui` ← `feedback`, `gui` ← `branding`; `environment` depends only on URP and Unity modules. `presentation` depends on all four. `ar-hud` depends on no other Veilbreak package (Input System optional) and is not part of the entry package yet. Binaries are plain git blobs (no LFS), so a git
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
uv run --frozen python packages/com.veilbreak.ar-hud/Tools~/kit_tokens.py --check
dotnet run --project packages/com.veilbreak.ar-hud/Tests~/InputChecks/Veilbreak.ARHud.InputChecks.csproj
```

[`.github/workflows/deploy-pages.yml`](.github/workflows/deploy-pages.yml) validates the tooling and publishes the handbook.
See the [multi-repo architecture](https://github.com/veil-break/.github-private/blob/main/docs/architecture/multi-repo-refactoring.md)
for the package plan (gui, rendering, feedback, environment, branding, presentation, ar-hud).
