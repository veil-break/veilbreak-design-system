"""Inline the kit's icon SVGs (single-ink, stroke #faeef4), their design-system aliases and the mark for bundle.js."""

import glob
import json
import os
import pathlib
import re
import sys

R = sys.argv[1]
sys.path.insert(0, str(pathlib.Path(__file__).resolve().parents[1]))
from icons import ALIASES  # noqa: E402


def kebab(n):
    b = ''
    for i, c in enumerate(n):
        if c.isupper() and i > 0 and n[i - 1] != '-':
            b += '-'
        b += '-' if c in ' _' else c.lower()
    return b


icons = {}
for f in sorted(glob.glob(R + '/Icons/*.svg')):
    s = pathlib.Path(f).read_text().strip()
    m = re.match(r'<svg([^>]*)>(.*)</svg>$', s, re.DOTALL)
    attrs, inner = dict(re.findall(r'([\w:-]+)="([^"]*)"', m.group(1))), m.group(2)
    assert (
        attrs['viewBox'] == '0 0 24 24'
        and attrs['stroke'] == '#faeef4'
        and attrs['stroke-width'] == '1.6'
        and attrs['fill'] == 'none'
    ), f
    cap, join = attrs.get('stroke-linecap', 'butt'), attrs.get('stroke-linejoin', 'miter')
    if (cap, join) != ('square', 'miter'):  # keep a root's own caps (Burst.svg draws butt ends)
        inner = f'<g stroke-linecap="{cap}" stroke-linejoin="{join}">{inner}</g>'
    assert '#' not in inner and '<!--' not in inner, f
    icons[kebab(os.path.basename(f)[:-4])] = inner
icons.update({alias: icons[name] for alias, name in ALIASES.items()})
mark = re.sub(r'<!--.*?-->', '', pathlib.Path(R, 'Vectors', 'Mark.svg').read_text(), flags=re.DOTALL).strip()
mark = re.sub(r'\s*\n\s*', ' ', mark)
assert '</script' not in mark
print('  var ICONS = ' + json.dumps(icons, separators=(',', ':')) + ';')
print('  var MARK = ' + json.dumps(mark) + ';')
