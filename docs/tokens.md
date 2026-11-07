# Tokens guide

`packages/com.veilbreak.gui/Tokens~/tokens.json` is the only authority for Veilbreak's design tokens. Everything below is
generated from it; never edit a generated file by hand. This guide is the procedure. The full list of tokens (value, USS
variable, C# name, usage) is the generated
[token reference](../packages/com.veilbreak.gui/Documentation~/tokens.md).

## The source files

All in `packages/com.veilbreak.gui/Tokens~/` (the `~` keeps Unity from importing them):

| file | holds | read by |
| --- | --- | --- |
| `tokens.json` | colour (two themes: `veil`, `contrast`), type (fonts, families, style groups), spacing, radius, shadow, cut, size, stroke, glow, fx, opacity, layer, duration, easing, layout | `Tools~/tokens.py` |
| `fluid.json` | the fluid scale: panel-width steps (320 to 600) and their factors | `Tools~/fluid.py` (also reads `tokens.json`) |
| `motion.json` | motion recipes (`viewIn`, `deal`, `pressSink` ...), the stagger and named timings | `Tools~/motion.py` |
| `components.json` | the component catalogue (`schema`, `types`) the handbook and web kit read | `Tools~/handbook/build_handbook.py` |

Every family is `{"tokens": [{"name", "value", "usage"}]}`. A colour carries one value per theme (`veil` first, then
`contrast`) or one value for both, and may alias another colour as `"{name}"`; prefer an alias or a semantic name
(`ally`, `panel`) over a primitive (`teal-500`) in components.

## Edit and regenerate

From the repository root (uv installs nothing beyond Ruff; the generators use the standard library):

```sh
uv sync --locked
# edit packages/com.veilbreak.gui/Tokens~/tokens.json
uv run --frozen python packages/com.veilbreak.gui/Tools~/tokens.py          # writes the four outputs below
uv run --frozen python packages/com.veilbreak.gui/Tools~/motion.py          # after motion.json
uv run --frozen python packages/com.veilbreak.gui/Tools~/fluid.py           # after fluid.json, or after a type/spacing/layout token
uv run --frozen python packages/com.veilbreak.gui/Tools~/tokens.py --check  # names stale outputs and exits 1
```

`--check` exists on `tokens.py`, `motion.py`, `fluid.py`, `screen_kit.py` and `samples.py`. CI runs it for all of them
and fails the build on drift (`.github/workflows/deploy-pages.yml`). `tokens.py --upstream FILE` lists the differences from
another `tokens.json`.

In the workspace, `vb design build` (from the hub checkout, `--check` to write nothing) runs `tokens.py`, `motion.py` and
`icons.py` in this checkout; `vb design tokens` runs the first two only, and `vb design icons` only `icons.py`. It does not
run `fluid.py`; run that by hand after a change to the type, spacing or layout tokens.

## What each output contains

| output | content |
| --- | --- |
| `Runtime/Styles/Tokens.uss` | every token UI Toolkit can hold as a custom property on `:root`, as `--vb-color-<name>`, `--vb-<name>` for the px, number and ms families (spacing, layout, cut, size, stroke, fx, opacity, duration), `--vb-font-size-<style>`, `--vb-line-height-<style>`, `--vb-tracking-<style>` and `--vb-font-<family>-<weight>` for `-unity-font-definition`. The high-contrast theme is the `.vb-contrast` class. |
| `Runtime/Styles/TokensContrast.uss` | the same contrast overrides on `:root`, for `VeilbreakGuiContrast.tss` (an always-high-contrast build) |
| `Runtime/Core/VbTokens.cs` | the veil colours as `VbTokens.<Name>` (for example `VbTokens.AllyText`), for code that cannot read USS: rich text, meshes, materials |
| `Documentation~/tokens.md` | the reference table of every token |
| `Runtime/Core/MotionRecipes.cs`, `Tools~/web/motion-recipes.js` | the motion recipes for Unity and the web kit (from `motion.py`) |
| `Runtime/Styles/Fluid.uss`, `Runtime/Core/VbFluidSteps.cs`, `Tools~/web/fluid-steps.js` | one `vb-fluid-<width>` class per panel-width step, overriding the type, spacing and layout tokens (from `fluid.py`) |

A token marked *reference* in the reference table has a CSS-only value (shadows, filters, curves): UI Toolkit cannot hold
it, so components apply its intent directly and the web kit uses the value as is. Colour and glow tokens follow the
accessibility classes the panel root carries (`vb-contrast`, `vb-reduced-motion`, `vb-reduced-flashes`, `vb-large-text`,
`vb-compact`; set by `SafeAreaRoot`).

## Using tokens

- In USS: `var(--vb-color-ally)`, `var(--vb-space-4)`, `-unity-font-definition: var(--vb-font-body-medium)`. Add
  `vb-contrast` to a root element for the high-contrast theme.
- In C#: `VbTokens.Ally`; durations and offsets from `MotionRecipes`.
- Add the sheets to the panel **theme** (`Runtime/Styles/VeilbreakGui.tss`, or `VeilbreakGuiContrast.tss`), not only to a
  document: popovers sit on the panel root, so only theme rules reach them.
- Colour never travels alone: a status carries a glyph and a word, a blocked control its reason.

## The shape vocabulary

Cut-corner shapes are driven by one set of `--vb-*` USS properties, read by both painters: `VbShape` (the mesh painter)
and `VbVectorShape` (the Painter2D sibling).

| property | meaning |
| --- | --- |
| `--vb-cut` | chamfer size in px (`cut-xs` to `cut-lg` tokens); `--vb-cut-side` 0 cuts top-left and bottom-right, 1 the left side, 2 the right (a pair of choice cards mirror each other) |
| `--vb-fill`, `--vb-fill-to`, `--vb-fill-dir` | fill colour, optional gradient end and direction in degrees (0 is left to right) |
| `--vb-edge`, `--vb-edge-to` | frame colour and optional gradient end |
| `--vb-bw` | frame width in px |
| `--vb-dash` | 1 draws the dashed frame (blocked or queued) |
| `--vb-bracket`, `--vb-bracket-len` | L marks on the two square corners |
| `--vb-glow` | the soft outer bleed (the glow budget: the primary action and the selected thing) |

`.vb-shape` resets these properties because custom properties inherit; a variant class sets its own values. A
`VbVectorVocabulary` can read the same names under another prefix (`--vb-vector-stroke`) or ar-hud's bare names
(`--stroke`); `VbVectorShape.cs` lists the extra properties (`ring`, `sheen`, `scan`, `halo` ...).

## Checklist for a token change

1. Edit `tokens.json` (and `fluid.json` / `motion.json` if the change reaches them).
2. Run the generators above, then `--check` for each.
3. Rebuild the web outputs the CI compares: `Tools~/web/package.py` and `Tools~/handbook/build_handbook.py` (the pages
   workflow diffs `Web~/dist`, `Tokens.uss` and `VbTokens.cs` against a rebuild).
4. `uv run --frozen ruff check .`
5. Bump the package version and CHANGELOG, then pin the new commit in the game specs (hub workflow).
