# Veilbreak GUI Handbook

A Unity-first page for the design system: every component sorted into four tiers (atoms, composites, layouts,
views) plus assets, each with the type that implements it, what it is built from, C# and UXML usage, the USS rules it
is painted from, the tokens those rules read, its C# API and a live preview from the web mirror (`Tools~/web`).

```sh
python3 Tools~/handbook/build_handbook.py . docs
```

`docs/` is the GitHub Pages site: `index.html`, `kit/` (the page's bundle, styles, tokens, specimen styles and fonts), `web/` (the web
kit's `dist/` and the `demo/` app, from `Web~`) and `.nojekyll`. It can also be published with the Artifact tool
(`root` = docs).

## GitHub Pages deployment

In the repository's **Settings → Pages → Build and deployment**, select **GitHub Actions** as the source.
The [Deploy GitHub Pages workflow](../../.github/workflows/deploy-pages.yml) rebuilds the handbook and demo from
source on every push to `main`, then publishes the generated site with the official Pages actions. No npm install,
Unity editor or custom deployment secret is needed. Pull requests targeting `main` build and upload an artifact
for validation; deployment runs only from `main`. To redeploy manually, open **Actions → Deploy GitHub Pages →
Run workflow** and select `main`.

The handbook is served at <https://veil-break.github.io/veilbreak-design-system/> and the demo app at
<https://veil-break.github.io/veilbreak-design-system/web/demo/>. Relative asset paths support the repository subpath.
The workflow builds into the runner's temporary directory, so committed `docs/` outputs do not need to be refreshed
for deployment.

## Build sources

The build runs `Tools~/web/build.sh` and `Tools~/web/package.py`, so the previews always match the package. Run
`python3 Tools~/metas.py` afterwards so Unity has `.meta` files for the site.

| File | Role |
| --- | --- |
| `catalog.py` | The taxonomy: tier, design name, Unity types, status, composition, usage, rules, specs (slim frame), preview scenes |
| `build_handbook.py` | Reads tokens, USS, C# API, icons, `components.md` and `previews.py`; writes the page |
| `shell.html`, `app.js`, `helpers.js`, `handbook.css` | The page, its app, preview helpers (`board`, `zone`, `section`) and the slim-frame CSS |

Statuses: **shipped** (in the package as described), **partial** (exists, contract incomplete), **proposed** (spec
only). Since 0.3.0 every entry is shipped. When a component changes in C#, update its entry in `catalog.py` alongside
`kit.js`, `docs.py` and `previews.py`.

## Semantic page structure

The handbook follows the [Semantic Nodes style guide](https://semantic-nodes.vercel.app/style-guide/)
([source](https://github.com/tuomashatakka/semantic-nodes/blob/main/docs/pdf-generator/document-base.md)):
HTML elements describe content, CSS describes presentation, and JavaScript handles interactions.
Keep the Veil palette and font tokens; use the handbook's CSS layers and structural selectors instead of utility
classes or inline presentation styles. IDs identify unique page regions; `data-*` attributes identify domain data
such as tokens, specimen indexes and preview types. Use native state attributes (`hidden`, `open`, `aria-current`,
`aria-selected`, `aria-pressed`) for state styling.

- `shell.html` provides the document, viewport, search, content landmark and native navigation dialog.
- `app.js` renders navigation lists, document headings, figures, tables and definition lists. Links navigate;
  buttons act. Documentation tabs support arrow keys, Home and End.
- `handbook.css` owns the page tokens, elements, layout, components and responsive rules. The mobile breakpoint is
  860px: the same navigation tree moves into a modal drawer with its own scrolling, Escape/Close/backdrop dismissal
  and focus containment. Desktop retains the sidebar.
- `build_handbook.py` generates `kit/specimens.css` for token swatches, type specimens, font samples and preview
  heights directly from source metadata. Do not reintroduce inline styles for these values.
- The preview layer and live component mirrors keep the kit's existing `vb-*` / `hb-*` class and rendering contracts.
  Keep handbook element rules out of `[data-preview]` descendants when they would restyle a showcased component.

After changes, rebuild the site and check phone (320px and 390px), tablet and desktop widths. Verify no page-wide
horizontal overflow, drawer dismissal and focus return, mobile/desktop resizing while the menu is open, search,
tab keyboard navigation, copy actions and portrait preview fit/actual-size modes. Run an accessibility audit on
the handbook interface separately from the component previews.
