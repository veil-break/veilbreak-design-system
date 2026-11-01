"""Port com.veilbreak.gui's USS (Gui.uss, Layout.uss, World.uss) to web CSS for the design system artifact.

Mechanical, so values stay exact: class and --vb-* shape property names are kept (the legacy web kit is renamed to
vbl- instead), token variables -> the design system's names (--vb-color-text -> --text), :root -> .vb-root,
.unity-* -> .vb-u-*, Unity font files -> font-family/weight, Unity letter-spacing (hundredths of an em in TextCore)
-> em, -unity-text-align -> flex alignment, tint -> color.

    python3 uss2css.py Gui.uss [Layout.uss World.uss ...] > kit.css
"""

import pathlib
import re
import sys

FONTS = {
    'Poppins-Regular.ttf': ('var(--font-body)', 400),
    'Poppins-Medium.ttf': ('var(--font-body)', 500),
    'Poppins-SemiBold.ttf': ('var(--font-body)', 600),
    'Poppins-Bold.ttf': ('var(--font-body)', 700),
    'texgyreadventor-bold.otf': ('var(--font-display)', 700),
    'texgyreadventor-regular.otf': ('var(--font-display)', 400),
}
# Tokens.uss --vb-font-<family>-<weight> variables (after tokens() drops the vb- prefix).
FONT_WEIGHTS = {'regular': 400, 'medium': 500, 'semibold': 600, 'bold': 700}
ALIGN = {'left': 'flex-start', 'center': 'center', 'right': 'flex-end'}
# Unity tints an image by multiplying; these two greyed portraits get the nearest CSS filter.
TINT_FILTER = {
    'rgb(150, 140, 150)': 'grayscale(.6) brightness(.58)',
    'rgb(190, 180, 190)': 'grayscale(.4) brightness(.74)',
}


def tokens(value):
    """Package token variables to the design system CSS names: --vb-color-text -> --text, --vb-space-2 -> --space-2."""
    return re.sub(r'var\(--vb-(?:color-)?([a-z0-9-]+)\)', r'var(--\1)', value)


def decl(prop, value, selector):
    value = tokens(value.strip())
    if prop == '-unity-font-definition':
        var = re.fullmatch(r'var\(--font-(display|body|hud)-(regular|medium|semibold|bold)\)', value)
        if var:
            return [('font-family', f'var(--font-{var[1]})'), ('font-weight', str(FONT_WEIGHTS[var[2]]))]
        name = re.search(r'([\w-]+\.(?:ttf|otf))', value).group(1)
        family, weight = FONTS[name]
        return [('font-family', family), ('font-weight', str(weight))]
    if prop == '-unity-font-style':
        return [('font-weight', '700')] if value == 'bold' else []
    if prop == 'letter-spacing':
        n = float(value.rstrip('px'))
        return [('letter-spacing', f'{n / 100:.4f}'.rstrip('0').rstrip('.') + 'em')]
    if prop == '-unity-text-align':
        v, hz = value.split('-')
        return [
            ('display', 'flex'),
            ('align-items', 'center' if v == 'middle' else ALIGN.get(v, 'flex-start')),
            ('justify-content', ALIGN[hz]),
            ('text-align', hz),
        ]
    if prop == '-unity-background-scale-mode':
        return [('background-size', 'contain'), ('background-repeat', 'no-repeat'), ('background-position', 'center')]
    if prop == '-unity-background-image-tint-color':
        if value in TINT_FILTER:
            return [('filter', TINT_FILTER[value])]
        return [('color', value)]
    if prop == 'background-image' and value.startswith('url('):
        return []  # icons and the mark are inline SVG on the web
    if prop == 'background-image':
        return [('background-image', value)]
    if prop.startswith('-unity-'):
        raise SystemExit(f'unmapped {prop} in {selector}')
    if prop.startswith('--vb-'):
        return [(prop, tokens(value))]
    if prop == 'scale':
        return [('scale', value)]
    return [(prop, value)]


def port(src):
    src = re.sub(r'/\*.*?\*/', '', src, flags=re.DOTALL)
    out = []
    for selector, body in re.findall(r'([^{}]+)\{([^{}]*)\}', src):
        selector = ' '.join(selector.split())
        if selector == '.unity-label':
            continue
        if selector == ':root':
            selector = '.vb-root'
        selector = re.sub(r'\.unity-', '.vb-u-', selector)
        decls = []
        for part in body.split(';'):
            if ':' not in part:
                continue
            p, v = part.split(':', 1)
            decls += decl(p.strip(), v, selector)
        if decls:
            out.append(selector + ' { ' + ' '.join(f'{p}: {v};' for p, v in decls) + ' }')
    return '\n'.join(out)


if __name__ == '__main__':
    print('\n'.join(f'/* {pathlib.Path(f).name} */\n' + port(pathlib.Path(f).read_text()) for f in sys.argv[1:]))
