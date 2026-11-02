# Web mirror of the package

These scripts build the Veilbreak Design System artifact from this package, so the web kit and the UI Toolkit kit
stay the same components. Folders ending in `~` are not imported by Unity. The workflow is described in
`Documentation~/web-mirror.md`.

| File | Role |
| --- | --- |
| `build.sh OUT` | Writes the artifact's whole `project/` folder to `OUT/project` and checks it; intermediates go to `OUT/generated/` |
| `uss2css.py` | Ports `Gui.uss`, `Layout.uss` and `World.uss` to CSS. Classes and `--vb-*` shape properties are kept. Token variables take the design system's names (`--vb-color-text` → `--text`). `:root` becomes `.vb-root`. Unity fonts become family and weight. TextCore letter-spacing (hundredths of an em) becomes `em`. |
| `icons_js.py` | Inlines `Runtime/Icons/*.svg`, their alias names and `Runtime/Vectors/Mark.svg` for the bundle |
| `kit.js` | React 18 ports of the package's components. `VbShape` paints the cut shape as SVG from the `--vb-*` properties. |
| `base.css`, `supplement.css` | UI Toolkit's layout defaults under `.vb-root`; the slider parts, scanlines and preview stage that UI Toolkit draws natively |
| `previews.py OUT/project/components` | One `preview.html` per component, with sample data from `Samples~/Gallery` and `Samples~/BattleHud` |
| `docs.py OUT/project legacy/index.d.ts KEEP...` | Component READMEs, plus `index.d.ts`: the kit's types, then the legacy types in `namespace Legacy` |
| `assemble.py PACKAGE OUT KEEP...` | Bundle, legacy kit (`vb` → `vbl`, `window.Veilbreak.Legacy`), brand book, guidelines, tokens, fonts, cover and the Icons note, then the checks |
| `cover.html` | The Overview cover card |
| `harness.py OUT TOKENS_JSON NAME...` | Renders previews in headless Chrome (`CHROME` overrides the binary) into `OUT/render/` |
| `legacy/` | The artifact's original landscape web kit, as published, plus the kept components' READMEs and previews |

```sh
OUT=$(mktemp -d)
Tools~/web/build.sh "$OUT"
uv run --frozen python Tools~/web/harness.py "$OUT" Tokens~/tokens.json BattleHUD SkillButton LandscapeHUD
```

To publish, use the Artifact tool. The steps are in `Documentation~/web-mirror.md`: read first, upload icons as
assets, send one files publish with `design-system.json` last.

To lint, run `uv run --project .. --frozen ruff check .` from `Tools~`. The settings are in the repository-root `pyproject.toml` (two levels above this package). Run `uv sync --locked` at the repository root first.
