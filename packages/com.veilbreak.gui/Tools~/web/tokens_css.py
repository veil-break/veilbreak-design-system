"""Tokens~/tokens.json as CSS custom properties under their bare names (--text, --ally, --cut-xs …).

from tokens_css import tokens_css; tokens_css(tokens, font_url=lambda file: 'fonts/' + name)
"""

import json
import pathlib
import re

FLUID = json.loads((pathlib.Path(__file__).resolve().parents[2] / 'Tokens~' / 'fluid.json').read_text())


def colour_value(v, theme, first):
    if isinstance(v, dict):
        v = v.get(theme, v.get(first))
    m = re.fullmatch(r'\{(.+)\}', v)
    return f'var(--{m.group(1)})' if m else v


def tokens_css(tokens, font_url=lambda f: 'fonts/' + pathlib.Path(f).name):
    themes = [t['id'] for t in tokens['color']['themes']]
    css = []
    for f in tokens['type']['fonts']:
        css.append(
            f"@font-face {{ font-family: '{f['family']}'; src: url('{font_url(f['file'])}'); "
            f'font-weight: {f["weight"]}; font-style: {f.get("style", "normal")}; font-display: swap; }}'
        )
    for i, th in enumerate(themes):
        sel = ':root' if i == 0 else f'[data-theme="{th}"]'
        decl = ' '.join(f'--{t["name"]}: {colour_value(t["value"], th, themes[0])};' for t in tokens['color']['tokens'])
        decl += ''.join(
            f' --{t["name"]}: {colour_value(t["value"], th, themes[0])};'
            for t in tokens.get('shadow', {}).get('tokens', [])
        )
        css.append(f'{sel} {{ {decl} }}')
    root = []
    for fam, body in tokens.items():
        if fam in ('name', 'version', 'color', 'type', 'shadow') or not isinstance(body, dict):
            continue
        for t in body.get('tokens', []):
            root.append(f'--{t["name"]}: {t["value"]};')
    for k, v in tokens['type']['families'].items():
        root.append(f'--font-{k}: {v};')
    # Type styles (tokens.py writes the same into Tokens.uss) and the fluid scale's baseline: the size ladder and the
    # heading trims. Fluid.uss overrides them per panel width (.vb-fluid-<w>).
    for group in tokens['type']['groups']:
        for s in group['styles']:
            size = float(s['fontSize'].rstrip('px'))
            root.append(f'--font-size-{s["name"]}: {s["fontSize"]};')
            root.append(f'--line-height-{s["name"]}: {s["lineHeight"]};')
            if s['name'].startswith('heading'):
                root.append(f'--trim-{s["name"]}: {-round(size * 0.17, 2):g}px;')
    root += [f'--fs-{n}: {n}px;' for n in FLUID['ladder']]
    css.append(':root { ' + ' '.join(root) + ' }')
    return '\n'.join(css) + '\n'
