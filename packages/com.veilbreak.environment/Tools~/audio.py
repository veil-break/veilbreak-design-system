#!/usr/bin/env python3
"""Regenerate the street's ambience loops in Runtime/Audio. Deterministic: the same script writes the same bytes.

A_Rain_Loop.wav     8 s stereo: distant hiss, thousands of droplet ticks, puddle drips and a low wash
A_CityHum_Loop.wav  12 s mono: low urban rumble with slow traffic swells and a faint mains hum
A_Fire_Loop.wav     6 s mono: a low roar with crackles and pops, for the positional wreck fires

Every loop is seamless: modulation periods divide the loop length, and the tail is crossfaded into the head.
Standard library only: `uv run --project ../../tools python Tools~/audio.py` from the package root.
"""

import array
import math
import pathlib
import random
import wave

out = pathlib.Path(__file__).resolve().parents[1] / 'Runtime' / 'Audio'
RATE = 32000


def onepole(signal, cutoff, high=False):
    """First-order low-pass (or the complementary high-pass) at cutoff Hz."""
    a = math.exp(-2 * math.pi * cutoff / RATE)
    y, result = 0.0, []
    for x in signal:
        y = (1 - a) * x + a * y
        result.append(x - y if high else y)
    return result


def band(signal, low, high):
    return onepole(onepole(signal, high), low, high=True)


def white(n, rnd):
    return [rnd.uniform(-1, 1) for _ in range(n)]


def brown(n, rnd):
    y, result = 0.0, []
    for _ in range(n):
        y = (y + rnd.uniform(-1, 1) * 0.02) * 0.998
        result.append(y)
    return result


def ping(buffer, at, freq, decay, amp, drop=0.0):
    """Adds a decaying sine starting at sample `at`; drop bends the pitch down (puddle bloops)."""
    phase, length = 0.0, int(decay * 6 * RATE)
    for i in range(min(length, len(buffer) - at)):
        t = i / RATE
        phase += 2 * math.pi * freq * (1 - drop * min(1.0, t / decay / 3)) / RATE
        buffer[at + i] += math.sin(phase) * amp * math.exp(-t / decay)


def loop(signal, fade):
    """Crossfades the extra `fade` samples at the end into the start (equal power), returning a seamless loop."""
    body = signal[:-fade]
    for i in range(fade):
        w = i / fade
        body[i] = body[i] * math.sin(w * math.pi / 2) + signal[len(signal) - fade + i] * math.cos(w * math.pi / 2)
    return body


def normalize(channels, peak):
    top = max(max(abs(v) for v in c) for c in channels) or 1
    return [[v / top * peak for v in c] for c in channels]


def write(name, channels):
    frames = array.array('h')
    for i in range(len(channels[0])):
        for c in channels:
            frames.append(max(-32767, min(32767, round(c[i] * 32767))))
    out.mkdir(parents=True, exist_ok=True)
    with wave.open(str(out / name), 'wb') as w:
        w.setnchannels(len(channels))
        w.setsampwidth(2)
        w.setframerate(RATE)
        w.writeframes(frames.tobytes())
    print(f'wrote {name}: {len(channels[0]) / RATE:.1f} s, {len(channels)} ch')


def rain():
    seconds, fade = 8, RATE // 2
    n = seconds * RATE + fade
    rnd = random.Random(101)
    channels = []
    for side in range(2):
        hiss = band(white(n, rnd), 500, 7000)
        wash = onepole(brown(n, rnd), 180)
        drops = [0.0] * n
        for _ in range(int(650 * n / RATE)):
            ping(
                drops, rnd.randrange(n), rnd.uniform(2200, 6500), rnd.uniform(0.0015, 0.004), rnd.expovariate(1) * 0.12
            )
        for _ in range(int(3 * n / RATE)):
            ping(
                drops, rnd.randrange(n), rnd.uniform(500, 1200), rnd.uniform(0.012, 0.025), rnd.uniform(0.1, 0.25), 0.5
            )
        mix = [hiss[i] * 0.35 + wash[i] * 2.2 + drops[i] * (0.8 if side else 0.7) for i in range(n)]
        channels.append(loop(mix, fade))
    write('A_Rain_Loop.wav', normalize(channels, 0.7))


def hum():
    seconds, fade = 12, RATE // 2
    n = seconds * RATE + fade
    rnd = random.Random(202)
    rumble = onepole(onepole(brown(n, rnd), 140), 140)
    traffic = band(white(n, rnd), 250, 900)
    mix = []
    for i in range(n):
        t = i / RATE
        swell = 0.5 + 0.3 * math.sin(2 * math.pi * t / 12 * 2) + 0.2 * math.sin(2 * math.pi * t / 12 * 3 + 1.3)
        mains = 0.02 * math.sin(2 * math.pi * 50 * t) + 0.01 * math.sin(2 * math.pi * 100 * t)
        mix.append(rumble[i] * 3.0 + traffic[i] * 0.05 * swell + mains)
    write('A_CityHum_Loop.wav', normalize([loop(mix, fade)], 0.6))


def fire():
    seconds, fade = 6, RATE // 4
    n = seconds * RATE + fade
    rnd = random.Random(303)
    roar = onepole(band(white(n, rnd), 60, 500), 400)
    crackle = [0.0] * n
    for _ in range(int(28 * n / RATE)):
        at, length, amp = rnd.randrange(n), rnd.randint(30, 140), rnd.expovariate(1) * 0.5
        for i in range(min(length, n - at)):
            crackle[at + i] += rnd.uniform(-1, 1) * amp * math.exp(-i / length * 4)
    crackle = onepole(crackle, 900, high=True)
    for _ in range(int(2 * n / RATE)):
        ping(crackle, rnd.randrange(n), rnd.uniform(180, 400), 0.01, rnd.uniform(0.4, 0.8))
    mix = []
    for i in range(n):
        t = i / RATE
        flicker = 0.7 + 0.3 * math.sin(2 * math.pi * t / 6 * 5) * math.sin(2 * math.pi * t / 6 * 3 + 0.7)
        mix.append(roar[i] * 1.6 * flicker + crackle[i] * 0.6)
    write('A_Fire_Loop.wav', normalize([loop(mix, fade)], 0.7))


if __name__ == '__main__':
    rain()
    hum()
    fire()
