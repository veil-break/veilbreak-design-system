#!/usr/bin/env python3
"""Write missing Unity .meta files for the package (immutable packages ignore assets without one).

    uv run --frozen python Tools~/metas.py [--check]

GUIDs are an MD5 of the package-relative path, so reruns are stable. Folders ending in ~, dotfiles and docs/ site files
that already have metas are left as they are. --check lists missing metas and exits 1 when there are any.
"""

import hashlib
import os
import pathlib
import sys

PKG = pathlib.Path(__file__).resolve().parent.parent
USS = (PKG / 'Runtime/Styles/Gui.uss.meta').read_text().split('\n', 2)[2]
DEFAULT = 'DefaultImporter:\n  externalObjects: {}\n  userData:\n  assetBundleName:\n  assetBundleVariant:\n'
FOLDER = 'folderAsset: yes\n' + DEFAULT
TEXT = 'TextScriptImporter:\n  externalObjects: {}\n  userData:\n  assetBundleName:\n  assetBundleVariant:\n'


def hidden(p):
    rel = p.relative_to(PKG)
    return (
        any(part.startswith('.') or part.endswith('~') for part in rel.parts)
        or p.suffix == '.meta'
        or rel.parts[0] == 'node_modules'
    )


def body(p):
    if p.is_dir():
        return FOLDER
    if p.suffix == '.cs':
        return None
    if p.suffix in ('.uss', '.tss'):
        return USS
    if p.suffix in ('.md', '.txt', '.html', '.json', '.xml', '.csv', '.yaml', '.bytes'):
        return TEXT
    return DEFAULT


missing = []


def assets():
    for root, dirs, files in os.walk(PKG):
        dirs[:] = sorted(
            d for d in dirs if not d.startswith('.') and not d.endswith('~') and d not in {'node_modules', 'work'}
        )
        yield from (pathlib.Path(root) / name for name in sorted(dirs + files))


for p in assets():
    if hidden(p) or p.parent == PKG and p.name == 'package.json' and (PKG / 'package.json.meta').exists():
        continue
    meta = p.with_name(p.name + '.meta')
    if meta.exists():
        continue
    missing.append(meta)
    if '--check' in sys.argv:
        continue
    guid = hashlib.md5(('com.veilbreak.gui/' + str(p.relative_to(PKG))).encode()).hexdigest()
    b = body(p)
    meta.write_text(f'fileFormatVersion: 2\nguid: {guid}' + ('' if b is None else '\n' + b))
for m in missing:
    print(('missing ' if '--check' in sys.argv else 'wrote ') + str(m.relative_to(PKG)))
sys.exit(1 if missing and '--check' in sys.argv else 0)
