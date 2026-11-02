"""Assemble the design system artifact's project/ folder from this package, then check it.

build.sh has already written the kit's READMEs, index.d.ts and previews into OUT/project/components and the ported
CSS and icons into OUT/generated. This adds the bundle, the legacy web kit (renamed vb -> vbl, exported as
window.Veilbreak.Legacy), the brand book, guidelines, tokens, fonts and cover, and fails on anything the artifact
would reject or render wrong.

    python3 assemble.py PACKAGE OUT LEGACY_KEEP...
"""

import json
import pathlib
import re
import shutil
import subprocess
import sys

PKG, OUT = pathlib.Path(sys.argv[1]), pathlib.Path(sys.argv[2])
KEEP = sys.argv[3:]
WEB = pathlib.Path(__file__).resolve().parent
LEGACY = WEB / 'legacy'
PROJECT = OUT / 'project'
COMPONENTS = PROJECT / 'components'
GEN = OUT / 'generated'
VB = re.compile(r'(?<![A-Za-z0-9_])vb(?![A-Za-z0-9_])')
LEGACY_GROUP = 'Legacy web'


def legacy(text):
    """The legacy kit's classes and custom properties move from vb- to vbl-, so the kit owns vb-."""
    return VB.sub('vbl', text)


def card(html, subtitle, title=None):
    """Re-group a legacy preview under Legacy web and point it at window.Veilbreak.Legacy."""
    first, rest = html.split('\n', 1)
    marker = re.fullmatch(r'<!-- @dsCard (.*) -->', first)
    attrs = re.sub(r'\s*subtitle="[^"]*"', '', re.sub(r'group="[^"]*"', f'group="{LEGACY_GROUP}"', marker[1]))
    rest = legacy(rest).replace('var V = window.Veilbreak,', 'var V = window.Veilbreak.Legacy,')
    assert 'window.Veilbreak.Legacy' in rest
    if title:
        rest = re.sub(r'<title>[^<]*</title>', f'<title>{title}</title>', rest, count=1)
    return f'<!-- @dsCard {attrs} subtitle="{subtitle}" -->\n{rest}'


def readme(md, name):
    """Legacy README: the summary stays first; the deprecation note follows it."""
    head, summary, rest = legacy(md).split('\n\n', 2)
    note = (
        f'**Deprecated.** Part of the legacy landscape web kit: `window.Veilbreak.Legacy.{name}` (also'
        f' `window.Veilbreak.{name}`), with `vbl-` classes. The portrait kit (`com.veilbreak.gui`) has no equivalent'
        ' yet. Never mix the two kits on one screen.'
    )
    return f'{head}\n\n{summary}\n\n{note}\n\n{rest}'


def guideline(md):
    """Links to package files outside the guidelines become plain text; links between guidelines stay."""
    return re.sub(r'\[([^\]]+)\]\((?!\d\d-|#|https?:)[^)]*\)', r'\1', md)


# Styles: the legacy kit (vbl-) first, then the kit on UI Toolkit's layout defaults.
legacy_css = legacy((LEGACY / 'bundle.css').read_text()).rstrip('\n')
kit_css = '\n'.join(p.read_text().rstrip('\n') for p in (WEB / 'base.css', GEN / 'kit.css', WEB / 'supplement.css'))
css = legacy_css + '\n\n' + kit_css + '\n'
(COMPONENTS / 'bundle.css').write_text(css)

# Script: the legacy IIFE exports window.Veilbreak.Legacy (and the kept names), then the kit's IIFE.
first, legacy_js = (LEGACY / 'bundle.js').read_text().split('\n', 1)
assert first.startswith('/* @ds-bundle: ')
legacy_js = legacy(legacy_js).rstrip('\n')
export = 'window.Veilbreak = Object.assign(window.Veilbreak || {}, api);'
assert legacy_js.count(export) == 1
legacy_js = legacy_js.replace(
    export,
    'var NS = window.Veilbreak = window.Veilbreak || {};\n  NS.Legacy = api;\n'
    f'  {json.dumps(KEEP)}.forEach(function (n) {{ NS[n] = api[n]; }});',
)
kit_js = (
    (WEB / 'kit.js')
    .read_text()
    .replace('/*@@ICONS@@*/', (GEN / 'icons.js').read_text().rstrip('\n'))
    .replace('/*@@MOTION@@*/', (WEB / 'motion-recipes.js').read_text())
)
api = kit_js.split('var api = {')[1].split('};')[0]
names = [n for n in re.findall(r'(\w+): \w+', api) if n[0].isupper() and n != 'VbShape']
header = {'format': 4, 'namespace': 'Veilbreak', 'components': [{'name': n} for n in names + KEEP]}
body = legacy_js + '\n\n' + kit_js.rstrip('\n') + '\n'
(COMPONENTS / 'bundle.js').write_text('/* @ds-bundle: ' + json.dumps(header, separators=(',', ':')) + ' */\n' + body)

# Legacy components that stay, and the landscape board.
for name in KEEP:
    src, dst = LEGACY / 'components' / name, COMPONENTS / name
    dst.mkdir(parents=True, exist_ok=True)
    (dst / 'preview.html').write_text(card((src / 'preview.html').read_text(), 'Deprecated · landscape web kit'))
    (dst / 'README.md').write_text(readme((src / 'README.md').read_text(), name))
board = COMPONENTS / 'LandscapeHUD'
board.mkdir(exist_ok=True)
(board / 'preview.html').write_text(
    card(
        (LEGACY / 'components' / 'BattleHUD' / 'preview.html').read_text(),
        'Deprecated · the landscape arena board, 844 × 390, from window.Veilbreak.Legacy',
        'Landscape HUD (legacy) — Veilbreak',
    )
)
(COMPONENTS / 'Cover').mkdir(exist_ok=True)
shutil.copy(WEB / 'cover.html', COMPONENTS / 'Cover' / 'preview.html')

# Brand book, guidelines, tokens and fonts.
brand = (PKG / 'Documentation~' / 'brand.md').read_text()
(PROJECT / 'README.md').write_text(re.sub(r'\A# [^\n]*\n\n', '', brand))
for md in sorted((PKG / 'Documentation~' / 'guidelines').glob('*.md')):
    (PROJECT / md.name).write_text(guideline(md.read_text()))
tokens = json.loads((PKG / 'Tokens~' / 'tokens.json').read_text())
shutil.copy(PKG / 'Tokens~' / 'tokens.json', PROJECT / 'tokens.json')
(PROJECT / 'fonts').mkdir(exist_ok=True)
for font in tokens['type']['fonts']:
    shutil.copy(PKG / 'Runtime' / 'Fonts' / pathlib.Path(font['file']).name, PROJECT / font['file'])
# The Icons asset group holds Runtime/Icons/*.svg as uploads named by their VbIcon (kebab-case) names.
(PROJECT / 'assets' / 'Icons').mkdir(parents=True, exist_ok=True)
(PROJECT / 'assets' / 'Icons' / 'README.md').write_text(
    "The kit's glyph set as single-ink SVGs, one per `VbIcon` name: 24 px grid, 1.6 stroke, square caps, no fills."
    ' Ink is `#faeef4` (the `text` token) because an `<img>` cannot inherit colour; in code use `VbIcon`, which draws'
    ' the same files in `currentColor`.\n\nFamilies, in order: skill shapes, roles, statuses, gear, operative skills'
    ' (`skill-<operative>-<skill>`), HUD controls, menu destinations and the two marker shapes. The sources are'
    ' `Runtime/Icons/*.svg` in `com.veilbreak.gui`; change a glyph there and rebuild, and both this group and'
    ' `components/bundle.js` follow.\n'
)

# Checks.
errors = []
for f in sorted(PROJECT.rglob('*')):
    if f.suffix in {'.md', '.html', '.js', '.css', '.ts', '.json'}:
        text = f.read_text()
        if re.search(r'(?<![A-Za-z0-9_])vba(?![A-Za-z0-9_])', text):
            errors.append(f'{f.relative_to(PROJECT)}: a vba name is left')
uncommented = [re.sub(r'/\*.*?\*/', '', c, flags=re.DOTALL) for c in (kit_css, legacy_css)]
if re.search(r'(?<![A-Za-z0-9_-])vbl-', uncommented[0]) or re.search(r'(?<![A-Za-z0-9_-])vb-', uncommented[1]):
    errors.append('bundle.css: the kit and legacy class names meet')
if re.search(r'</script|<!--', body, re.IGNORECASE):
    errors.append('bundle.js: contains </script or <!--')
if not shutil.which('node'):
    sys.exit('Node.js is required to validate the generated JavaScript')
if shutil.which('node'):
    # bundle.js and every preview's inline script must parse.
    scripts = {'bundle.js': body}
    for f in sorted(COMPONENTS.glob('*/preview.html')):
        for i, js in enumerate(re.findall(r'<script>(.*?)</script>', f.read_text(), re.DOTALL)):
            scripts[f'{f.relative_to(PROJECT)} script {i + 1}'] = js
    check = (
        'const s = JSON.parse(require("fs").readFileSync(0, "utf8"));'
        'for (const k in s) try { new Function(s[k]); } catch (e) { console.log(k + ": " + e.message); }'
    )
    parse = subprocess.run(
        ['node', '-e', check], check=False, input=json.dumps(scripts), text=True, capture_output=True
    )
    errors += [line for line in parse.stdout.splitlines() if line]
for c in header['components']:
    for part in ('README.md', 'preview.html'):
        if not (COMPONENTS / c['name'] / part).exists():
            errors.append(f'components/{c["name"]}/{part}: missing')
for f in sorted(COMPONENTS.glob('*/preview.html')):
    if not re.match(r'<!-- @dsCard [^\n]*-->\n', f.read_text()):
        errors.append(f'{f.relative_to(PROJECT)}: line 1 is not the @dsCard marker')
defined = {t['name'] for family in tokens.values() if isinstance(family, dict) for t in family.get('tokens', [])}
defined |= {f'font-{k}' for k in tokens['type']['families']}
defined |= set(re.findall(r'(--[\w-]+)\s*:', css))
defined = {d.removeprefix('--') for d in defined}
undefined = sorted(set(re.findall(r'var\(--([\w-]+)\)', css)) - defined)  # var(--x, fallback) is set by script
if undefined:
    errors.append('bundle.css: undefined variables ' + ', '.join(undefined))
if errors:
    sys.exit('\n'.join(errors))
print(f'{len(header["components"])} components ({len(names)} kit, {len(KEEP)} legacy) · bundle.js {len(body)} B')
