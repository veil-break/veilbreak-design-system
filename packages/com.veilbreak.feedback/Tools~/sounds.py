#!/usr/bin/env python3
"""Regenerate the feedback package's cue one-shots in Runtime/Audio. Deterministic: the same script writes the same
bytes. 32 kHz mono 16-bit; peaks at -3 dBFS so cue volumes stay comparable.

A_Splash        footstep on wet asphalt          A_Warning       breach alarm (two-tone, three beeps)
A_Zap           beam discharge                   A_Swap          manual cast pre-empting an autocast
A_Whoosh        dash and wall raise              A_Spawn         hostile arrival (falling swell)
A_Shield        barrier shimmer                  A_Tap           interface tap
A_Heal          mending chime                    A_Stinger_Wave  wave clear / upgrade (rising arpeggio)
A_Charge        hostile windup (rising whine)    A_Stinger_Boss  boss defeated (sub hit and swelling chord)

Standard library only: `uv run --project ../../tools python Tools~/sounds.py` from the package root.
"""

import array
import math
import pathlib
import random
import wave

out = pathlib.Path(__file__).resolve().parents[1] / 'Runtime' / 'Audio'
RATE = 32000


def onepole(signal, cutoff, high=False):
    a = math.exp(-2 * math.pi * cutoff / RATE)
    y, result = 0.0, []
    for x in signal:
        y = (1 - a) * x + a * y
        result.append(x - y if high else y)
    return result


def sweep_band(signal, start, end, curve=None):
    """Band-pass whose centre glides from start to end Hz (two one-pole stages at ±half an octave)."""
    lo = hi = 0.0
    result = []
    for i, x in enumerate(signal):
        t = i / len(signal)
        centre = start * (end / start) ** (curve(t) if curve else t)
        ah = math.exp(-2 * math.pi * centre * 1.41 / RATE)
        al = math.exp(-2 * math.pi * centre / 1.41 / RATE)
        hi = (1 - ah) * x + ah * hi
        lo = (1 - al) * hi + al * lo
        result.append(hi - lo)
    return result


def env(t, attack, decay):
    return min(1.0, t / attack) * math.exp(-max(0.0, t - attack) / decay) if attack > 0 else math.exp(-t / decay)


def tone(seconds, partials, attack, decay, vibrato=0.0, rate=0.0, glide=1.0):
    """Sum of (frequency, amplitude) partials with a shared envelope; glide multiplies every frequency by the end."""
    n = int(seconds * RATE)
    phases = [0.0] * len(partials)
    result = []
    for i in range(n):
        t = i / RATE
        bend = glide ** (t / seconds) * (1 + vibrato * math.sin(2 * math.pi * rate * t))
        s = 0.0
        for k, (freq, amp) in enumerate(partials):
            phases[k] += 2 * math.pi * freq * bend / RATE
            s += math.sin(phases[k]) * amp
        result.append(s * env(t, attack, decay))
    return result


def noise(seconds, seed):
    rnd = random.Random(seed)
    return [rnd.uniform(-1, 1) for _ in range(int(seconds * RATE))]


def mix(*parts):
    n = max(int(offset * RATE) + len(p) for p, _, offset in parts)
    result = [0.0] * n
    for part, gain, offset in parts:
        at = int(offset * RATE)
        for i, v in enumerate(part):
            if at + i < n:
                result[at + i] += v * gain
    return result


def shaped(signal, attack, decay):
    return [v * env(i / RATE, attack, decay) for i, v in enumerate(signal)]


def write(name, signal, peak=0.708):
    fade = int(0.004 * RATE)
    signal = signal[:]
    for i in range(min(fade, len(signal))):
        signal[-1 - i] *= i / fade
    top = max(abs(v) for v in signal) or 1
    frames = array.array('h', (max(-32767, min(32767, round(v / top * peak * 32767))) for v in signal))
    out.mkdir(parents=True, exist_ok=True)
    with wave.open(str(out / f'{name}.wav'), 'wb') as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(RATE)
        w.writeframes(frames.tobytes())
    print(f'wrote {name}.wav: {len(signal) / RATE:.2f} s')


def harmonics(root, count, tilt):
    return [(root * k, (1 / k) ** tilt) for k in range(1, count + 1)]


def main():
    splash = mix(
        (shaped(sweep_band(noise(0.16, 1), 2600, 1400), 0.002, 0.04), 1.0, 0),
        (tone(0.08, [(720, 1)], 0.001, 0.02, glide=0.6), 0.25, 0.005),
    )
    write('A_Splash', splash)
    zap = mix(
        (tone(0.4, harmonics(180, 8, 0.8), 0.004, 0.12, vibrato=0.04, rate=31), 0.6, 0),
        (tone(0.3, [(2400, 1)], 0.002, 0.08, glide=0.35), 0.35, 0),
        (shaped(onepole(noise(0.4, 2), 1500, high=True), 0.002, 0.07), 0.4, 0),
    )
    write('A_Zap', zap)
    write('A_Whoosh', shaped(sweep_band(noise(0.45, 3), 380, 2200, lambda t: math.sin(t * math.pi)), 0.15, 0.12))
    write('A_Shield', tone(0.7, [(880, 1), (1320, 0.6), (1760, 0.35), (2640, 0.2)], 0.03, 0.22, vibrato=0.01, rate=17))
    write('A_Heal', tone(0.9, [(660, 1), (660 * 2.76, 0.45), (660 * 5.4, 0.2), (990, 0.5)], 0.004, 0.3))
    write(
        'A_Charge',
        mix(
            (tone(0.6, harmonics(220, 5, 1.2), 0.45, 0.12, glide=2.2), 0.7, 0),
            (shaped(sweep_band(noise(0.6, 4), 600, 3200), 0.5, 0.1), 0.3, 0),
        ),
    )
    beeps = [(tone(0.15, harmonics(f, 7, 1.0)[::2], 0.005, 0.12), 0.6, 0.2 * k) for k, f in enumerate((740, 560, 740))]
    write('A_Warning', mix(*beeps))
    write('A_Swap', tone(0.16, [(1200, 1), (2400, 0.3)], 0.003, 0.05, glide=1.5))
    spawn = mix(
        (shaped(sweep_band(noise(0.55, 5), 3000, 260), 0.25, 0.15), 0.8, 0),
        (tone(0.5, [(90, 1), (135, 0.4)], 0.2, 0.18), 0.5, 0.05),
    )
    write('A_Spawn', spawn)
    write(
        'A_Tap', mix((tone(0.05, [(2200, 1)], 0.0005, 0.008), 0.8, 0), (shaped(noise(0.02, 6), 0.0003, 0.003), 0.3, 0))
    )
    notes = (523.25, 659.25, 783.99, 1046.5)
    bell = [
        (tone(0.9, [(f, 1), (f * 2, 0.35), (f * 3.01, 0.15)], 0.004, 0.35), 0.5, 0.11 * k) for k, f in enumerate(notes)
    ]
    pad = (tone(1.4, [(261.6, 0.4), (392, 0.3), (523.25, 0.25)], 0.3, 0.6), 0.35, 0)
    write('A_Stinger_Wave', mix(*bell, pad))
    chord = [(tone(2.6, harmonics(f, 6, 1.3), 0.6, 1.1), 0.22, 0.1) for f in (110, 164.8, 220, 277.2)]
    boss = mix(
        (tone(1.2, [(52, 1), (104, 0.3)], 0.003, 0.4, glide=0.7), 1.0, 0),
        (shaped(sweep_band(noise(1.2, 7), 4000, 300), 0.005, 0.35), 0.5, 0),
        *chord,
        (tone(2.0, [(880, 1), (1318.5, 0.6)], 0.4, 0.8), 0.12, 0.5),
    )
    write('A_Stinger_Boss', boss)


if __name__ == '__main__':
    main()
