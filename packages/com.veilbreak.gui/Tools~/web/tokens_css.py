"""Tokens~/tokens.json as CSS custom properties under their bare names (--text, --ally, --cut-xs …).

from tokens_css import tokens_css; tokens_css(tokens, font_url=lambda file: 'fonts/' + name)
"""

import pathlib
import re


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
    css.append(':root { ' + ' '.join(root) + ' }')
    return '\n'.join(css) + '\n'
