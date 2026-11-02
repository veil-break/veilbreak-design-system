#!/bin/sh
# Compatibility entry point; all subprocesses use the locked project interpreter.
set -eu
PKG=$(CDPATH= cd -- "$(dirname -- "$0")/../.." && pwd)
exec uv run --project "$PKG" --frozen python "$PKG/Tools~/web/build.py" "${1:?usage: build.sh OUT}"
