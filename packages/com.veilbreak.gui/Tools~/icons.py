#!/usr/bin/env python3
"""Rebuild Runtime/Styles/Icons.uss from Runtime/Icons/*.svg: one .vb-icon--{kebab-name} class per glyph, plus aliases.

The set is the union of the arcade's glyphs and the Veilbreak design system's web icons. Where both had a glyph for the
same idea under different names, the package glyph is canonical and the design-system name becomes an alias class.
"""

import pathlib
import re

root = pathlib.Path(__file__).resolve().parents[1] / 'Runtime'
# design-system name -> canonical package name
ALIASES = {
    'plus': 'add',
    'silenced': 'silence',
    'rooted': 'root',
    'slowed': 'slow',
    'replay': 'replays',
    'cancel': 'close',
    'chevron-right': 'chevron',
    'rift': 'rifts',
}


def kebab(n):
    return re.sub(r'(?<!^)([A-Z])', r'-\1', n).lower()


def main():
    files = {kebab(p.stem): p.name for p in sorted((root / 'Icons').glob('*.svg'))}
    missing = sorted(set(ALIASES.values()) - set(files))
    if missing:
        raise SystemExit(f'aliases point at missing icons: {missing}')
    clash = sorted(set(ALIASES) & set(files))
    if clash:
        raise SystemExit(f'alias names shadow icons: {clash}')
    header = 'Generated from Runtime/Icons/*.svg by Tools~/icons.py: VbIcon adds vb-icon--{kebab-name}. Do not edit.'
    lines = [f'/* {header} */']
    rule = '.vb-icon--{} {{ background-image: url("../Icons/{}"); }}'
    lines += [rule.format(name, file) for name, file in files.items()]
    lines.append('/* Aliases: design-system names for the same glyphs. */')
    lines += [rule.format(alias, files[name]) for alias, name in ALIASES.items()]
    (root / 'Styles' / 'Icons.uss').write_text('\n'.join(lines) + '\n')
    print(f'{len(files)} icons, {len(ALIASES)} aliases')


if __name__ == '__main__':
    main()
