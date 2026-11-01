#!/usr/bin/env python3
"""Build the web package (Web~/dist) from the package sources: the React kit without the legacy landscape kit.

    uv run --frozen python Tools~/web/package.py [OUT]     (default: Web~/dist)

dist/veilbreak.js    IIFE: reads window.React (and window.ReactDOM for popovers), sets window.Veilbreak
dist/veilbreak.mjs   ES module: imports react and react-dom, exports every component by name and `Veilbreak`
dist/veilbreak.css   UI Toolkit layout defaults + Gui/Layout/Views/World.uss ported + web supplement
dist/tokens.css      tokens.json as custom properties (dark Veil theme on :root, [data-theme="contrast"])
dist/index.d.ts      the kit's types
dist/fonts/          Poppins, Share Tech Mono, TeX Gyre Adventor
"""

import json
import pathlib
import re
import shutil
import subprocess
import sys

WEB = pathlib.Path(__file__).resolve().parent
PKG = WEB.parent.parent
sys.path.insert(0, str(WEB))
from tokens_css import tokens_css  # noqa: E402

OUT = pathlib.Path(sys.argv[1]).resolve() if len(sys.argv) > 1 else PKG / 'Web~' / 'dist'
STYLES = PKG / 'Runtime' / 'Styles'
if OUT.exists():
    shutil.rmtree(OUT)
(OUT / 'fonts').mkdir(parents=True)

head = '/* @veilbreak/gui — generated from com.veilbreak.gui by Tools~/web/package.py. Do not edit. */\n'
kit_css = subprocess.run(
    [sys.executable, str(WEB / 'uss2css.py')]
    + [str(STYLES / f) for f in ('Gui.uss', 'Layout.uss', 'Views.uss', 'World.uss', 'Motion.uss')],
    check=True,
    capture_output=True,
    text=True,
).stdout
css = '\n'.join(p.rstrip('\n') for p in ((WEB / 'base.css').read_text(), kit_css, (WEB / 'supplement.css').read_text()))
(OUT / 'veilbreak.css').write_text(head + css + '\n')
tokens = json.loads((PKG / 'Tokens~' / 'tokens.json').read_text())
(OUT / 'tokens.css').write_text(head + tokens_css(tokens))
for f in (PKG / 'Runtime' / 'Fonts').iterdir():
    if f.suffix in ('.ttf', '.otf'):
        shutil.copy(f, OUT / 'fonts' / f.name)

icons = subprocess.run(
    [sys.executable, str(WEB / 'icons_js.py'), str(PKG / 'Runtime')], check=True, capture_output=True, text=True
).stdout
kit = (WEB / 'kit.js').read_text().replace('/*@@ICONS@@*/', icons.rstrip('\n'))
(OUT / 'veilbreak.js').write_text(head + kit)
api = kit.split('var api = {')[1].split('};')[0]
names = [n for n in re.findall(r'(\w+): \w+', api) if n[0].isupper()]
# The ES module runs the same IIFE against a scope: React and ReactDOM come from imports, other globals from the page.
esm = (
    head + "import * as React from 'react';\nimport * as ReactDOM from 'react-dom';\n"
    'const window = new Proxy({ React, ReactDOM, Veilbreak: {} }, { get: (t, k) => (k in t ? t[k] : globalThis[k]),'
    ' set: (t, k, v) => { t[k] = v; return true; }, has: () => true });\n'
    + kit.replace('(function () {', '(function (window) {', 1).rstrip().rstrip(';').removesuffix('})()')
    + '})(window);\n'
    'export const Veilbreak = window.Veilbreak;\nexport default Veilbreak;\n'
    + ''.join(f'export const {n} = Veilbreak.{n};\n' for n in names)
    + 'export const format = Veilbreak.format;\n'
)
(OUT / 'veilbreak.mjs').write_text(esm)

# Types: the kit's declarations from docs.py, without the legacy namespace.
tmp = OUT / '.types'
subprocess.run(
    [sys.executable, str(WEB / 'docs.py'), str(tmp), str(WEB / 'legacy' / 'index.d.ts')],
    check=True,
    capture_output=True,
)
dts = (tmp / 'components' / 'index.d.ts').read_text().split('\n/* ── Legacy web')[0]
(OUT / 'index.d.ts').write_text(
    head.replace('/*', '//').replace(' */', '')
    + dts.rstrip('\n')
    + '\nexport declare const Veilbreak: Record<string, unknown>;\nexport default Veilbreak;\n'
)
shutil.rmtree(tmp)

if not shutil.which('node'):
    sys.exit('Node.js is required to validate the generated JavaScript')
if shutil.which('node'):
    check = subprocess.run(
        [
            'node',
            '--input-type=module',
            '-e',
            (
                f"import('{(OUT / 'veilbreak.mjs').as_uri()}')"
                '.catch(e => { if (!/react/.test(e.message)) { console.log(e.message); process.exit(1); } })'
            ),
        ],
        capture_output=True,
        text=True,
        check=False,
    )
    parse = subprocess.run(
        ['node', '-e', f"new Function(require('fs').readFileSync('{OUT / 'veilbreak.js'}', 'utf8'))"],
        capture_output=True,
        text=True,
        check=False,
    )
    if parse.returncode or check.returncode:
        sys.exit(parse.stderr + check.stdout + check.stderr)
print(f'{len(names)} exports · {OUT}')
