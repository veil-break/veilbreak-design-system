#!/usr/bin/env python3
"""Check that Runtime/Styles/KitTokens.uss defines exactly the --vb-color-* variables the kit sheets read.

    uv run --frozen python Tools~/kit_tokens.py [--check]

KitTokens.uss holds the standalone defaults for hosts without their own token sheet (the demo host and the Theme
Workshop fallback). Its values are a snapshot of Veilbreak's first colour theme, owned by this package since it moved
to veilbreak-design-system; they change deliberately, when the token authority moves here. This check keeps the
snapshot complete: every variable Base.uss or Kit.uss reads must be defined, and nothing else. It prints the
differences and exits 1 when there are any.
"""

import pathlib
import re
import sys

STYLES = pathlib.Path(__file__).resolve().parent.parent / 'Runtime' / 'Styles'
SHEETS = ('Base.uss', 'Kit.uss')
READ = re.compile(r'var\(--vb-color-([a-z0-9-]+)')
DEFINED = re.compile(r'^\s*--vb-color-([a-z0-9-]+)\s*:', re.MULTILINE)


def read_variables():
    return {name for sheet in SHEETS for name in READ.findall((STYLES / sheet).read_text())}


def defined_variables():
    return set(DEFINED.findall((STYLES / 'KitTokens.uss').read_text()))


def differences(read, defined):
    missing = [f'missing --vb-color-{name} (read by the kit sheets)' for name in sorted(read - defined)]
    extra = [f'extra --vb-color-{name} (no kit sheet reads it)' for name in sorted(defined - read)]
    return missing + extra


def main():
    found = differences(read_variables(), defined_variables())
    for line in found:
        print(f'KitTokens.uss: {line}')
    if not found and '--check' not in sys.argv:
        print(f'KitTokens.uss defines all {len(read_variables())} kit colour variables')
    return 1 if found else 0


if __name__ == '__main__':
    sys.exit(main())
