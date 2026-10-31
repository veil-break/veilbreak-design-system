# Veilbreak GUI Handbook

A Unity-first page for the design system: every component sorted into four tiers (atoms, composites, layouts,
views) plus assets, each with the type that implements it today, its status, what it is built from, C# and UXML
usage, the USS rules it is painted from, the tokens those rules read, its C# API and a live preview from the web
mirror (`Tools~/web`).

```sh
python3 Tools~/handbook/build_handbook.py . OUT --arcade ../..
```

`OUT/index.html` plus `OUT/kit/` is the page; publish it with the Artifact tool (`root` = OUT, the `kit/` files as
supporting files). The build runs `Tools~/web/build.sh` for the kit, so the previews always match the package.

| File | Role |
| --- | --- |
| `catalog.py` | The taxonomy: tier, design name, Unity types, status, composition, usage, rules, proposals (slim frame, Card, Tile, Badge, the views), preview scenes |
| `build_handbook.py` | Reads tokens, USS, C# API, icons, `components.md` and `previews.py`; writes the page |
| `shell.html`, `app.js`, `helpers.js`, `handbook.css` | The page, its app, preview helpers (`board`, `zone`, `section`) and the slim-frame CSS |

Statuses: **shipped** (in the package as described), **partial** (exists, contract incomplete), **proposed** (spec
only). When a component changes in C#, update its entry in `catalog.py` alongside `kit.js`, `docs.py` and `previews.py`.
