#!/usr/bin/env python3
"""Generate the package's HUD themes from the Veilbreak design tokens (com.veilbreak.gui Tokens~/tokens.json).

Outputs: Runtime/Themes/HudTheme_Veil.asset and HudTheme_Contrast.asset, one per colour theme in the token source. Each
HUD role reads one colour token (ROLES); fonts are gui's Poppins Regular (body) and TeX Gyre Adventor Bold (heading).

    uv run --frozen python Tools~/themes.py            # write every output
    uv run --frozen python Tools~/themes.py --check    # name stale outputs and exit 1, writing nothing

Colours carry one value per theme ("veil" first, then "contrast") or one value for both, and may alias another colour
as "{name}". The assets are Unity YAML written with float32 shortest round-trip values, as the editor writes them, so a
reimport leaves them unchanged. The themes record their source in `generatedFrom`; the inspector and the Theme Workshop
show them read-only.
"""

import json
import re
import struct
import sys
from pathlib import Path

PACKAGE = Path(__file__).resolve().parents[1]
GUI = PACKAGE.parent / 'com.veilbreak.gui'
SOURCE = GUI / 'Tokens~' / 'tokens.json'
THEMES_DIR = PACKAGE / 'Runtime' / 'Themes'
SCRIPT_GUID = '9e6f717bed825786ae9cf093f6f5ff6a'  # Runtime/HudTheme.cs
GENERATED_FROM = 'Packages/com.veilbreak.gui/Tokens~/tokens.json (com.veilbreak.ar-hud Tools~/themes.py)'
# HudTheme role -> colour token. The HUD chrome draws in the holo frame colour.
ROLES = {
    'surface': 'panel',
    'foreground': 'text',
    'muted': 'text-2',
    'accent': 'holo-frame',
    'danger': 'enemy',
    'warning': 'warn',
    'power': 'power',
}
FONTS = {
    'bodyFont': GUI / 'Runtime' / 'Fonts' / 'Poppins-Regular.ttf',
    'headingFont': GUI / 'Runtime' / 'Fonts' / 'texgyreadventor-bold.otf',
}
LAYOUT = (('fontSize', 16), ('cornerCut', 0), ('lineWidth', 1), ('spacing', 16), ('targetHeight', 48))

T = json.loads(SOURCE.read_text())
THEMES = [t['id'] for t in T['color']['themes']]
COLORS = {t['name']: t['value'] for t in T['color']['tokens']}


def colour(name, theme, seen=()):
    value = COLORS[name]
    value = value.get(theme, value[THEMES[0]]) if isinstance(value, dict) else value
    if value.startswith('{'):
        alias = value[1:-1]
        if alias in seen:
            raise ValueError('alias cycle at ' + name)
        return colour(alias, theme, seen + (name,))
    return value


def channels(value):
    """#rrggbb or #rrggbbaa as four 0..1 channels."""
    digits = value.lstrip('#')
    if not re.fullmatch(r'[0-9a-fA-F]{6}([0-9a-fA-F]{2})?', digits):
        raise ValueError(f'not a hex colour: {value}')
    digits += 'ff' if len(digits) == 6 else ''
    return [int(digits[i : i + 2], 16) / 255 for i in (0, 2, 4, 6)]


def unity_float(value):
    """The shortest decimal that round-trips the float32, as Unity's YAML writer prints it."""
    single = struct.unpack('f', struct.pack('f', value))[0]
    if single == int(single):
        return str(int(single))
    for digits in range(1, 10):
        text = f'{single:.{digits}g}'
        if struct.unpack('f', struct.pack('f', float(text)))[0] == single:
            return text
    return repr(single)


def guid(path):
    return re.search(r'^guid: ([0-9a-f]{32})$', Path(f'{path}.meta').read_text(), re.MULTILINE)[1]


def asset(theme):
    name = f'HudTheme_{theme.capitalize()}'
    lines = [
        '%YAML 1.1',
        '%TAG !u! tag:unity3d.com,2011:',
        '--- !u!114 &11400000',
        'MonoBehaviour:',
        '  m_ObjectHideFlags: 0',
        '  m_CorrespondingSourceObject: {fileID: 0}',
        '  m_PrefabInstance: {fileID: 0}',
        '  m_PrefabAsset: {fileID: 0}',
        '  m_GameObject: {fileID: 0}',
        '  m_Enabled: 1',
        '  m_EditorHideFlags: 0',
        f'  m_Script: {{fileID: 11500000, guid: {SCRIPT_GUID}, type: 3}}',
        f'  m_Name: {name}',
        '  m_EditorClassIdentifier: Veilbreak.ARHud::Veilbreak.ARHud.HudTheme',
    ]
    for role, token in ROLES.items():
        r, g, b, a = (unity_float(c) for c in channels(colour(token, theme)))
        lines.append(f'  {role}: {{r: {r}, g: {g}, b: {b}, a: {a}}}')
    lines += [f'  {field}: {{fileID: 12800000, guid: {guid(path)}, type: 3}}' for field, path in FONTS.items()]
    lines += [f'  {field}: {value}' for field, value in LAYOUT]
    lines += [f'  highContrast: {int(theme != THEMES[0])}', f'  generatedFrom: {GENERATED_FROM}']
    return THEMES_DIR / f'{name}.asset', '\n'.join(lines) + '\n'


def main():
    check = '--check' in sys.argv
    stale = []
    for path, text in (asset(theme) for theme in THEMES):
        if path.exists() and path.read_text() == text:
            continue
        stale.append(path)
        if not check:
            path.parent.mkdir(parents=True, exist_ok=True)
            path.write_text(text)
    for path in stale:
        print(f'{"stale" if check else "wrote"}: {path.relative_to(PACKAGE)}')
    return 1 if check and stale else 0


if __name__ == '__main__':
    sys.exit(main())
