#!/usr/bin/env python3
"""Generate the screen kit's reference vectors: cut-corner frames, ability states, markers, telegraphs and decals.

    uv run --frozen python Tools~/screen_kit.py            # write Tools~/references/screen-kit/
    uv run --frozen python Tools~/screen_kit.py --check    # name stale outputs and exit 1

Game-agnostic design-kit references (the shipped glyphs are `Runtime/Icons`, the mark is branding's). Colours come from
the veil theme of `Tokens~/tokens.json`, so a token change shows up as drift in --check. Unity does not import
the output (`Tools~`).
The generator moved here from veilbreak-main's `vb screens build`; the city meshes and rasters stayed in main.
"""

import argparse
import json
import sys
from pathlib import Path

PACKAGE = Path(__file__).resolve().parents[1]
TOKENS = PACKAGE / 'Tokens~' / 'tokens.json'
KIT_DIR = PACKAGE / 'Tools~' / 'references' / 'screen-kit'
COLOR_NAMES = (
    'ground',
    'panel',
    'raised',
    'text',
    'ally',
    'enemy',
    'power',
    'warn',
    'barrier',
    'heal',
    'tier-grey',
    'danger-zone',
)


def load_colors():
    """Veil-theme hex (rrggbb) for COLOR_NAMES, following the token source's {alias} references."""
    tokens = json.loads(TOKENS.read_text())
    theme = tokens['color']['themes'][0]['id']
    raw = {t['name']: t['value'] for t in tokens['color']['tokens']}

    def resolve(name, seen=()):
        value = raw[name]
        value = value.get(theme, next(iter(value.values()))) if isinstance(value, dict) else value
        if value.startswith('{'):
            if value[1:-1] in seen:
                raise ValueError('alias cycle at ' + name)
            return resolve(value[1:-1], (*seen, name))
        return value[:7]  # the panel token carries alpha; the generators apply their own opacity

    return {name: resolve(name) for name in COLOR_NAMES}


COLORS = load_colors()


def path(d):
    return f'<path d="{d}"/>'


def circle(x=12, y=12, r=7):
    return f'<circle cx="{x}" cy="{y}" r="{r}"/>'


# Glyph bodies the ability-state frames embed (24-unit grid; the full set is gui's Runtime/Icons).
GLYPHS = {
    'check': path('M3 12l6 6L21 5'),
    'power': path('M14 2 5 14h6l-1 8 9-13h-6Z'),
    'silence': path('M9 4h6v10H9Z M5 10v4a7 7 0 0 0 14 0v-4 M12 21v2 M2 2l20 20'),
    'exposed': path('M12 2 21 6v7l-9 9-9-9V6Z M13 4 9 10l6 3-4 7'),
    'queue': path('M3 4h18 M3 10h12 M3 16h8 M16 13l6 5-6 5'),
    'cooldown': circle(12, 12, 9) + path('M12 5v7l5 3'),
    'windup': path('M6 2h12 M6 22h12 M7 2v5l10 10v5 M17 2v5L7 17v5'),
    'invalid': circle(12, 12, 9) + path('M6 6l12 12'),
    'target': circle(12, 12, 6) + path('M12 1v7 M12 16v7 M1 12h7 M16 12h7'),
}


def svg(files, target, body, width=24, height=24, color=None, origin=(0, 0)):
    files[target] = (
        f'<svg xmlns="http://www.w3.org/2000/svg" viewBox="{origin[0]} {origin[1]} {width} {height}" width="{width}" '
        f'height="{height}" fill="none" stroke="{color or COLORS["text"]}" stroke-width="1.6" stroke-linecap="square" '
        f'stroke-linejoin="miter">{body}</svg>'
    )


def build():
    files = {}
    frames = KIT_DIR / 'frames'
    for cut in (6, 10, 14):
        for state, color in [
            ('idle', COLORS['text']),
            ('selected', COLORS['ally']),
            ('disabled', COLORS['text']),
            ('danger', COLORS['enemy']),
            ('focus', COLORS['barrier']),
        ]:
            d = f'M{cut} 1H319V{95 - cut}L{320 - cut} 95H1V{cut}Z'
            body = f'<path d="{d}" fill="{COLORS["panel"]}" fill-opacity=".86" stroke="{color}" stroke-width="{2 if state in ("selected", "focus") else 1}"/>'
            if state in ('selected', 'focus'):
                body += path('M299 4h17v17 M4 75v17h17')
            if state == 'disabled':
                body += '<path d="M18 35h12v16H18Z M20 35v-6a4 4 0 0 1 8 0v6"/>'
            svg(files, frames / f'panel-{cut}-{state}.svg', body, 320, 96, color)
    for name, w, h, cut in [
        ('inspector', 320, 420, 14),
        ('portrait', 104, 80, 10),
        ('ability', 80, 80, 10),
        ('power-strip', 480, 22, 6),
        ('queue-chip', 120, 40, 6),
        ('connection-pill', 180, 44, 6),
        ('match-header', 420, 56, 10),
        ('nav', 240, 64, 10),
    ]:
        svg(
            files,
            frames / f'{name}.svg',
            f'<path d="M{cut} 1H{w - 1}V{h - cut}L{w - cut} {h - 1}H1V{cut}Z" fill="{COLORS["panel"]}" fill-opacity=".86"/>',
            w,
            h,
        )
    svg(
        files,
        frames / 'battle-cta.svg',
        f'<defs><linearGradient id="g"><stop stop-color="{COLORS["ally"]}"/><stop offset="1" stop-color="{COLORS["heal"]}"/></linearGradient></defs><path d="M14 1H319V74L306 87H1V14Z" fill="url(#g)" stroke="{COLORS["ally"]}"/>',
        320,
        88,
    )
    svg(
        files,
        frames / 'high-contrast-panel.svg',
        '<path d="M10 1H319V85L310 95H1V10Z" fill="black" stroke="white" stroke-width="2"/>',
        320,
        96,
    )
    states = {
        'ready': 'check',
        'aiming': 'target',
        'queued': 'queue',
        'windup': 'windup',
        'cooldown': 'cooldown',
        'insufficient-power': 'power',
        'silenced': 'silence',
        'exposed': 'exposed',
        'invalid-aim': 'invalid',
    }
    for state, icon in states.items():
        c = (
            COLORS['ally']
            if state == 'ready'
            else COLORS['warn']
            if state in ('insufficient-power', 'invalid-aim')
            else COLORS['power']
        )
        body = f'<path d="M10 1H79V70L70 79H1V10Z" fill="{COLORS["panel"]}"/><g transform="translate(16 16) scale(2)">{GLYPHS[icon]}</g>'
        if state == 'queued':
            body += '<path d="M7 66l12-12 M17 70l16-16 M31 70l16-16 M45 70l16-16 M59 70l12-12"/>'
        svg(files, frames / f'ability-{state}.svg', body, 80, 80, c)
    markers = KIT_DIR / 'markers'
    svg(
        files,
        markers / 'ally.svg',
        circle(32, 32, 24) + path('M32 3v8 M32 53v8 M3 32h8 M53 32h8'),
        64,
        64,
        COLORS['ally'],
    )
    svg(files, markers / 'enemy.svg', path('M32 3 61 32 32 61 3 32Z'), 64, 64, COLORS['enemy'])
    svg(
        files, markers / 'selected.svg', path('M2 18V2h16 M46 2h16v16 M62 46v16H46 M18 62H2V46'), 64, 64, COLORS['ally']
    )
    svg(files, markers / 'offscreen.svg', path('M8 56 32 8l24 48-24-12Z'), 64, 64, COLORS['warn'])
    for state, c in [('selected', COLORS['ally']), ('idle', COLORS['power']), ('empty', COLORS['text'])]:
        dash = 'stroke-dasharray="8 8"' if state == 'empty' else ''
        svg(
            files,
            markers / f'platform-{state}.svg',
            f'<ellipse cx="128" cy="44" rx="116" ry="31" {dash}/><ellipse cx="128" cy="44" rx="105" ry="24" opacity=".4"/><path d="M16 44h20 M220 44h20 M128 6v15 M128 67v15"/>',
            256,
            88,
            c,
        )
    telegraphs = KIT_DIR / 'telegraphs'
    for shape in ['circle', 'cone', 'beam', 'wall', 'fan', 'dash']:
        body = {
            'circle': circle(128, 128, 120),
            'cone': path('M128 248 12 65Q128-24 244 65Z'),
            'beam': '<rect x="112" y="8" width="32" height="240"/>',
            'wall': path('M8 128h240 M8 104v48 M248 104v48'),
            'fan': path('M128 248 72 8 M128 248V8 M128 248 184 8'),
            'dash': path('M112 240V56H88l40-48 40 48h-24v184Z'),
        }[shape]
        for team, c in [('ally', COLORS['ally']), ('enemy', COLORS['enemy'])]:
            svg(files, telegraphs / f'{shape}-{team}.svg', body, 256, 256, c)
    dz = COLORS['danger-zone']
    svg(
        files,
        telegraphs / 'pressure-hatch.svg',
        f'<defs><pattern id="h" width="16" height="16" patternUnits="userSpaceOnUse"><path d="M-4 4 4-4 M0 16 16 0 M12 20 20 12" stroke="{dz}" stroke-width="2"/></pattern></defs><rect x="2" y="2" width="252" height="252" fill="url(#h)" stroke="{dz}" stroke-width="3"/>',
        256,
        256,
        dz,
    )
    decals = KIT_DIR / 'decals'
    svg(
        files,
        decals / 'cracks.svg',
        path(
            'M3 73 42 64 69 89 113 70 142 72 166 35 209 21 252 3 M69 89 76 127 54 154 63 199 32 241 M113 70 135 109 164 116 189 162 229 179 M164 116 201 89 250 94'
        ),
        256,
        256,
        COLORS['ground'],
    )
    svg(
        files,
        decals / 'road-stripe.svg',
        f'<path d="M64 0v256 M192 0v256" stroke="{COLORS["tier-grey"]}" stroke-width="8" stroke-dasharray="48 32"/>',
        256,
        256,
    )
    svg(
        files,
        decals / 'rubble-footprint.svg',
        f'<path d="M16 91 49 28 160 9 237 55 249 177 181 241 68 226 9 162Z" fill="{COLORS["raised"]}" fill-opacity=".6" stroke="{COLORS["tier-grey"]}" stroke-dasharray="7 7"/>',
        256,
        256,
    )
    return files


def main(argv=None):
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument('--check', action='store_true', help='name stale or missing outputs and exit 1')
    args = parser.parse_args(argv)
    files = {target.relative_to(KIT_DIR).as_posix(): text for target, text in build().items()}
    existing = {p.relative_to(KIT_DIR).as_posix() for p in KIT_DIR.rglob('*.svg')} if KIT_DIR.is_dir() else set()
    stale = sorted(
        name for name, text in files.items() if not (KIT_DIR / name).is_file() or (KIT_DIR / name).read_text() != text
    ) + sorted(existing - files.keys())
    if args.check:
        for name in stale:
            print('stale:', name)
        print(f'{len(files)} screen-kit svgs,', 'stale outputs: ' + str(len(stale)) if stale else 'up to date')
        return 1 if stale else 0
    for name in sorted(existing - files.keys()):
        (KIT_DIR / name).unlink()
    for name, text in files.items():
        target = KIT_DIR / name
        target.parent.mkdir(parents=True, exist_ok=True)
        target.write_text(text, encoding='utf-8', newline='\n')
    print(f'wrote {len(files)} screen-kit svgs to', KIT_DIR.relative_to(PACKAGE))
    return 0


if __name__ == '__main__':
    sys.exit(main())
