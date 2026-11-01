"""Build the preview artifact using the caller's Python interpreter."""

import argparse
import pathlib
import shutil
import subprocess
import sys

WEB = pathlib.Path(__file__).resolve().parent
PKG = WEB.parent.parent
KEEP = (
    'ConnectionPill ConnectionBanner CombatFeed Minimap NavBar MatchHeader '
    'ItemCell RailButton Timeline FighterPortrait Telegraph'
).split()


def build(out):
    out = pathlib.Path(out).resolve()
    generated = out / 'generated'
    project = out / 'project'
    # Never allow a build output to contain package sources.
    if (
        out == PKG
        or out in PKG.parents
        or PKG in out.parents
        and out.parts[len(PKG.parts)] in {'Runtime', 'Tools~', 'Tokens~'}
    ):
        raise ValueError('Output must not contain package sources')
    for directory in (project, generated):
        if directory.exists():
            shutil.rmtree(directory)
        directory.mkdir(parents=True)
    (project / 'components').mkdir()

    def run(script, *args, output=None):
        result = subprocess.run(
            [sys.executable, str(WEB / script), *map(str, args)],
            check=True,
            stdout=subprocess.PIPE if output else None,
            text=True,
        )
        if output:
            output.write_text(result.stdout)

    styles = PKG / 'Runtime' / 'Styles'
    run(
        'uss2css.py',
        *(styles / f'{name}.uss' for name in ('Gui', 'Layout', 'Views', 'World', 'Motion')),
        output=generated / 'kit.css',
    )
    run('icons_js.py', PKG / 'Runtime', output=generated / 'icons.js')
    run('docs.py', project, WEB / 'legacy' / 'index.d.ts', *KEEP)
    run('previews.py', project / 'components')
    run('assemble.py', PKG, out, *KEEP)


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('out', type=pathlib.Path)
    build(parser.parse_args().out)
