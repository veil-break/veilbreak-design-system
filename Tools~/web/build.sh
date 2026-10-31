#!/bin/sh
# Build the Veilbreak Design System artifact's project/ folder from this package.
# Usage: Tools~/web/build.sh OUT  ->  OUT/project (publish it with the Artifact tool); intermediates in OUT/generated.
# The kit comes from Runtime/ (USS, icons, fonts), Tokens~/ and Documentation~/; legacy/ is the artifact's original
# landscape web kit, of which KEEP stays visible (renamed to vbl- and window.Veilbreak.Legacy). See
# Documentation~/web-mirror.md.
set -e
W=$(cd "$(dirname "$0")" && pwd); PKG=$(cd "$W/../.." && pwd); OUT=${1:?usage: build.sh OUT}; GEN="$OUT/generated"
KEEP="ConnectionPill ConnectionBanner CombatFeed Minimap NavBar MatchHeader ItemCell RailButton Timeline FighterPortrait Telegraph"
rm -rf "$OUT/project" "$GEN"; mkdir -p "$GEN" "$OUT/project/components"
S="$PKG/Runtime/Styles"
python3 "$W/uss2css.py" "$S/Gui.uss" "$S/Layout.uss" "$S/Views.uss" "$S/World.uss" > "$GEN/kit.css"
python3 "$W/icons_js.py" "$PKG/Runtime" > "$GEN/icons.js"
# shellcheck disable=SC2086
python3 "$W/docs.py" "$OUT/project" "$W/legacy/index.d.ts" $KEEP
python3 "$W/previews.py" "$OUT/project/components"
# shellcheck disable=SC2086
python3 "$W/assemble.py" "$PKG" "$OUT" $KEEP
