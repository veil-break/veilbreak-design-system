#!/usr/bin/env python3
"""Regenerate the splash's slicing sound, Runtime/Resources/VeilbreakSplashSlice.wav. Deterministic: the same script
writes the same bytes.

The blade whips in (a rising band of air), then cuts at SLICE_LEAD seconds into the clip: a sharp tick, a bright
metallic "shing" ringing down, the world tearing along the cut and a low thump as the mark breaks apart. IntroSplash
starts the clip SLICE_LEAD seconds before its impact, so the cut lands on the frame where the blade locks (keep the two
values equal: `IntroSplash.SliceLead`).

It reuses the feedback package's synthesis helpers (Tools~/sounds.py: 32 kHz mono 16-bit, -3 dBFS peak), so every cue in
the game shares one voice. Standard library only: `uv run --project ../../tools python Tools~/splash_sound.py` from the
package root, with com.veilbreak.feedback beside this package.
"""

import importlib.util
import math
import pathlib

package = pathlib.Path(__file__).resolve().parents[1]
spec = importlib.util.spec_from_file_location(
    'sounds', package.parent / 'com.veilbreak.feedback' / 'Tools~' / 'sounds.py'
)
dsp = importlib.util.module_from_spec(spec)
spec.loader.exec_module(dsp)
dsp.out = package / 'Runtime' / 'Resources'

SLICE_LEAD = 0.24


def slice_sound():
    cut = SLICE_LEAD
    # The blade's whip: air rising in pitch and level into the cut.
    whoosh = dsp.shaped(dsp.sweep_band(dsp.noise(cut + 0.04, 11), 450, 6200, lambda t: t * t), cut * 0.92, 0.03)
    # The cut itself: a hard tick of high noise.
    tick = dsp.shaped(dsp.onepole(dsp.noise(0.03, 12), 3200, high=True), 0.0004, 0.005)
    # Steel ringing down: inharmonic partials of a thin blade, bending flat a little as it decays.
    shing = dsp.tone(
        1.1,
        [(1870, 1.0), (1870 * 2.32, 0.7), (1870 * 3.87, 0.45), (1870 * 5.41, 0.3), (1870 * 7.1, 0.16)],
        0.001,
        0.28,
        glide=0.97,
    )
    shimmer = dsp.tone(1.0, [(3740, 1.0), (4810, 0.6)], 0.012, 0.36, vibrato=0.003, rate=7)
    # The world tearing along the cut: a band of noise falling from bright to dull.
    tear = dsp.shaped(dsp.sweep_band(dsp.noise(0.38, 13), 7200, 900, math.sqrt), 0.002, 0.11)
    # The break: a short low thump under it all.
    thump = dsp.tone(0.45, [(58, 1.0), (116, 0.35)], 0.002, 0.15, glide=0.6)
    return dsp.mix(
        (whoosh, 1.4, 0),
        (tick, 0.9, cut),
        (shing, 0.42, cut),
        (shimmer, 0.12, cut + 0.02),
        (tear, 0.5, cut),
        (thump, 0.85, cut),
    )


def main():
    dsp.write('VeilbreakSplashSlice', slice_sound())


if __name__ == '__main__':
    main()
