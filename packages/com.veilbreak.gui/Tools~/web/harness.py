"""Render previews locally the way the Design System page frames them.

tokens.css + fonts + bundle.css + React 18 + bundle.js, screenshotted by headless Chrome.
"""

import json
import os
import pathlib
import re
import shutil
import subprocess
import sys

# Usage: harness.py OUT TOKENS_JSON NAME...  (OUT holds build.sh's project/ output; writes OUT/render/NAME.{html,png})
PKG = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
D = os.path.abspath(sys.argv[1])
TOKENS = sys.argv[2]
names = sys.argv[3:]
FONTS = os.path.join(PKG, 'Runtime', 'Fonts') + '/'
t = json.loads(pathlib.Path(TOKENS).read_text())
first = t['color']['themes'][0]['id']


def val(v):
    v = v if isinstance(v, str) else v.get(first)
    return re.sub(r'^\{(.+)\}$', r'var(--\1)', v)


lines = [f'  --{x["name"]}: {val(x["value"])};' for x in t['color']['tokens']]
for fam, body in t.items():
    if isinstance(body, dict) and 'tokens' in body and fam != 'color':
        for x in body['tokens']:
            v = (
                x['value']
                if isinstance(x['value'], str)
                else (x['value'].get(first) if isinstance(x['value'], dict) else x['value'])
            )
            lines.append(f'  --{x["name"]}: {v if not isinstance(v, (int, float)) else str(v) + "px"};')
for k, s in t['type']['families'].items():
    lines.append(f'  --font-{k}: {s};')
faces = ''.join(
    f"@font-face{{font-family:'{f['family']}';src:url('file://{FONTS}{os.path.basename(f['file'])}');font-weight:{f['weight']};font-style:normal}}\n"
    for f in t['type']['fonts']
)
body = '\n'.join(lines)
tokens = faces + f':root, [data-theme="{first}"] {{\n{body}\n}}\n'
head = (
    '<style>' + tokens + '</style><link rel="stylesheet" href="file://%s/project/components/bundle.css">'
    '<script src="https://cdn.jsdelivr.net/npm/react@18.3.1/umd/react.production.min.js"></script>'
    '<script src="https://cdn.jsdelivr.net/npm/react-dom@18.3.1/umd/react-dom.production.min.js"></script>'
    '<script src="file://%s/project/components/bundle.js"></script>'
) % (D, D)
CHROME = os.environ.get('CHROME') or shutil.which('chromium') or shutil.which('google-chrome')
if not CHROME:
    sys.exit('Set CHROME to a Chromium executable')
os.makedirs(D + '/render', exist_ok=True)
for n in names:
    html = pathlib.Path(f'{D}/project/components/{n}/preview.html').read_text()
    first_line = html.split('\n', 1)[0]
    hgt = int(re.search(r'height=(\d+)', first_line).group(1))
    w = (
        int((re.search(r'width=(\d+)', first_line) or re.search('(x)?', '')).group(1) or 760)
        if re.search(r'width=(\d+)', first_line)
        else 760
    )
    html = html.replace('<html lang="en">', f'<html lang="en" data-theme="{first}">').replace(
        '</head>', head + '</head>'
    )
    out = f'{D}/render/{n}.html'
    pathlib.Path(out).write_text(html)
    subprocess.run(
        [
            CHROME,
            '--headless=new',
            '--disable-gpu',
            '--hide-scrollbars',
            '--allow-file-access-from-files',
            '--force-device-scale-factor=2',
            f'--window-size={max(w, 420) + 32},{hgt + 32}',
            '--virtual-time-budget=4000',
            f'--screenshot={D}/render/{n}.png',
            'file://' + out,
        ],
        check=True,
        stdout=subprocess.DEVNULL,
        stderr=subprocess.DEVNULL,
        timeout=60,
    )
    if not pathlib.Path(f'{D}/render/{n}.png').is_file():
        sys.exit('Missing screenshot: ' + n)
    print(n, 'rendered')
