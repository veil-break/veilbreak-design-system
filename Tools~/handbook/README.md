# Veilbreak GUI Handbook

A Unity-first page for the design system: every component sorted into four tiers (atoms, composites, layouts,
views) plus assets, each with the type that implements it, what it is built from, C# and UXML usage, the USS rules it
is painted from, the tokens those rules read, its C# API and a live preview from the web mirror (`Tools~/web`).

```sh
python3 Tools~/handbook/build_handbook.py . docs
```

`docs/` is the GitHub Pages site: `index.html`, `kit/` (the page's bundle, styles, tokens and fonts), `web/` (the web
kit's `dist/` and the `demo/` app, from `Web~`) and `.nojekyll`. Pages serves it from the branch with no workflow
(Settings → Pages → Deploy from a branch → `/docs`). It can also be published with the Artifact tool (`root` = docs).
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
