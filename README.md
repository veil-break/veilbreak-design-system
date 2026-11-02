# veilbreak design system

Shared presentation packages for Veilbreak games. Each Unity package lives in its own folder under
`packages/` and is consumed by the games through pinned git dependencies:

```json
"com.veilbreak.gui": "https://github.com/veil-break/veilbreak-design-system.git?path=/packages/com.veilbreak.gui#<full-reviewed-commit>"
```

| package | responsibility |
| --- | --- |
| [`com.veilbreak.gui`](packages/com.veilbreak.gui/README.md) | tokens, fonts, icons, controls, screen views, hud, world labels, navigation, accessibility; the web kit (`Web~`) and the GUI Handbook (`docs/`) |

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
```

[`.github/workflows/deploy-pages.yml`](.github/workflows/deploy-pages.yml) validates the tooling and publishes the handbook.
See the [multi-repo architecture](https://github.com/veil-break/.github-private/blob/main/docs/architecture/multi-repo-refactoring.md)
for the package plan (gui, rendering, feedback, environment, branding, presentation).
