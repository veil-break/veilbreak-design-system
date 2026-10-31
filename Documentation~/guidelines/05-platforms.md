# 5 · Platforms and the token pipeline

One token source feeds UI Toolkit, code and the web mirror. Names are identical in every output, so a value changed in
`Tokens~/tokens.json` means the same thing everywhere.

Adapted from `veilbreak/design-system/guide/05-platforms.md`. This package is now the source; the design-system
artifact is rebuilt from it.

## Token pipeline

```text
Tokens~/tokens.json ──Tools~/tokens.py──▶ Runtime/Styles/Tokens.uss          :root variables + the .vb-contrast class
                                       ├▶ Runtime/Styles/TokensContrast.uss  the contrast theme on :root (VeilbreakGuiContrast.tss)
                                       ├▶ Runtime/Core/VbTokens.cs           colours for code (rich text, meshes, materials)
                                       └▶ Documentation~/tokens.md           the reference
                    ──Tools~/web/build.sh──▶ the design-system artifact (web mirror)
```

- `python3 Tools~/tokens.py --check` exits 1 when an output is stale. The EditMode `TokenTests` fail when a sheet
  uses a variable `Tokens.uss` does not define, or when the contrast block and `TokensContrast.uss` differ.
- `--upstream FILE` lists colour differences from another `tokens.json`, such as the sibling design-system copy.

| Token | Family | USS | C# |
| --- | --- | --- | --- |
| `ally` | colour | `--vb-color-ally` | `VbTokens.Ally` |
| `teal-500` | colour primitive | `--vb-color-teal-500` | `VbTokens.Teal500` |
| `cut-md` | cut | `--vb-cut-md` | — |
| `touch-min` | size | `--vb-touch-min` | — |
| `dur-ui` | duration | `--vb-dur-ui` | — |
| `title` | type style | `--vb-font-size-title`, `--vb-line-height-title`, `--vb-tracking-title` | — |
| Poppins Medium | font file | `--vb-font-body-medium` | — |

Shadows, glows, easings, radii and layers have CSS-only values. They are listed in the [reference](../tokens.md) and
used by the web mirror; in UI Toolkit their intent is built into the components (`--vb-glow`, tree order for layers).

## UI Toolkit, and what replaces the web features

| Web feature | UI Toolkit in this package |
| --- | --- |
| `clip-path` cut corners | `VbShape` paints the polygon with `generateVisualContent` from `--vb-*` properties. The hit area stays the rectangle. |
| `filter: drop-shadow` glow | `--vb-glow`: three soft rings painted outside the cut. |
| `text-transform: uppercase` | Components uppercase their own text. Pass display names in normal case. |
| `:last-child`, `:nth-child` | Explicit modifier classes (`vb-perk-detail__row--last`). |
| `letter-spacing: .18em` | TextCore reads `letter-spacing` in hundredths of an em: `18px` ≈ 0.18em. |
| Custom properties | They inherit like CSS. `.vb-shape` resets every shape property, so a variant class sets its own. `var()` works inside custom properties, `-unity-font-definition` and standard properties. |
| `@keyframes` | Scheduled steps (`schedule.Execute`) and USS transitions; no infinite loops. |
| Stacking `z-index` | Tree order (see [layers](01-hud-layout.md#layers)). |

**Cascade.** Theme sheets (`VeilbreakGui.tss` imports Tokens, Gui, Layout, World, Icons in that order) apply before
sheets attached to elements. Game sheets on a UXML `<Style>` therefore win ties over the kit. Popovers live on the
panel root, which only the theme reaches, so the kit's sheets must be in the theme.

## The portrait board

| Setting | Value |
| --- | --- |
| PanelSettings | `Runtime/Settings/VeilbreakPortrait.asset`: Scale With Screen Size, 390 × 844 reference, match width (0), theme `VeilbreakGui.tss` |
| Supported widths | 320–430 units (`FitTests`) |
| Compact | Under 380 units: `vb-compact` |
| Safe area | Device insets in panel units, at least 48 top and 34 bottom (`SafeAreaRoot`) |
| Larger text | `vb-large-text`: `vb-t-body` 15 and `vb-t-caption` 13 units (`Layout.uss`); games add their own steps |

## Touch and pointer sizes

| Token | Value | Rule in this package |
| --- | --- | --- |
| `touch-min` | 48 | Preferred touch target on phones |
| `touch-min-pointer` | 44 | The kit's floor on both axes, enforced by `FitTests` |
| `ability-size` | 64 | Landscape reference; portrait cards use 44 × 56 skill buttons |
| `queue-chip-height` | 44 | `QueueChip` height; its × is a full-height 44 × 44 target |
| `cta-height` | 64 | Primary buttons on menu pages |
| `power-bar` | 18 | `PowerStrip` track |

Every interactive element takes pointer, touch and keyboard: `Ui.Clickable` makes an element focusable and fires on
click, Enter and Space. Hover-only affordances do not exist. A `Popover` opens on hover with a mouse and on tap with
touch, and also opens on focus.

## Fonts

| Family | Files | Use |
| --- | --- | --- |
| TeX Gyre Adventor (display) | Bold (Regular ships for games, unused by the kit) | Titles, names, CTAs; always caps and tracked |
| Poppins (body) | Regular, Medium, SemiBold, Bold | Labels, body, numbers |
| Share Tech Mono (hud) | Regular | Status timers (`vb-status__time`); available to games for tabular readouts |

Licences are in `Third Party Notices.md` (SIL OFL 1.1 for Poppins and Share Tech Mono, GUST for Adventor).

## Web

The web mirror (`Tools~/web`) ports `Gui.uss` to CSS and the components to React for the design-system artifact, so
designers and reviewers see the same kit in a browser. No web client ships. See [web mirror](../web-mirror.md).
