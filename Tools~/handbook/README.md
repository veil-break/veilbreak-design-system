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

## Interactive preview inspector

Use **Inspect** beside a preview, then pick a component on the canvas or choose an instance from the component list.
The outline follows the component's viewport bounds, including portrait scaling; the inspector also reports the
unscaled layout size. On desktop the inspector replaces the facts column beside the canvas. On phones it follows
the canvas; **Show component** returns to the selection.

The attribute editor is generated from all public component declarations in `Web~/dist/index.d.ts`, including
nested view models, lists, union types, nullable objects and every declared enum value. It also includes properties
used by the runtime when the declarations omit them. Edit a value to render it immediately, use **Default** to
restore an example value, **Unset** to remove an optional prop, or **Reset component** to restore the whole instance. Callback props and rendered React
content keep their behavior and can be explored through their child component instances.

The inspector exposes rendered HTML attributes, matching CSS definitions with source/condition context, computed
styles, `--vb-*` property overrides, CSS modifier classes and forced hover/active/focus states. Forced states simulate
CSS appearance; callbacks stay intact. While picking, pointer actions select components rather than activate them.
**Done inspecting** or Escape hides the inspector; edits remain in the current preview until reset or navigation.
**Copy props** copies serializable prop values. A render error stays inside its preview so values can be corrected.

`inspector.js` uses public React Context/ref APIs and a build-time adapter in the handbook bundle. It adds no DOM
layout wrappers and does not patch React or the distributable web kit. `inspection_schema.py` derives the controls;
`inspector.css` owns presentation, and the generator derives pseudo-state rules from the kit stylesheet while
preserving media/support conditions. Inspector state is temporary, per instance, and is not stored or deployed.

Validation:

```sh
python3 -m unittest discover -s Tools~/handbook/tests
python3 Tools~/handbook/build_handbook.py . /tmp/veilbreak-handbook
python3 -m http.server 8766 --directory /tmp/veilbreak-handbook
# In another terminal, with Playwright installed in the Node environment:
node Tools~/handbook/tests/inspector.browser.cjs http://127.0.0.1:8766/
```

The optional browser suite checks picking, all skill states, nested model edits, prop reset/copy, native attributes,
CSS state/variable previews, scaled bounds, every entry route, and phone/desktop overflow. Set `CHROMIUM_PATH` to
use an installed Chromium. For offline checks, `PREVIEW_RUNTIME_DIR` can contain the page's React 18.3.1 and React DOM
18.3.1 UMD files (`react.production.min.js`, `react-dom.production.min.js`) plus `axe.min.js`; this also enables the
open-inspector accessibility audits. None of these browser test dependencies are required by the Pages build.
