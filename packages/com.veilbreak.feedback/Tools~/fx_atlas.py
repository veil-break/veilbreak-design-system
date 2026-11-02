#!/usr/bin/env python3
"""Regenerate Runtime/Textures/T_FxAtlas.png, the combat effect atlas. Deterministic: the same script writes the same
bytes.

512 px, a 4 x 4 grid of 128 px tiles (index = row * 4 + column, row 0 at the top). Shapes are white in rgb with the
shape in alpha, so particles and marks tint them through vertex colour; scorch, crack and blob are dark. Every tile
keeps an 8 px transparent border, so bilinear filtering and the first mip levels never bleed into a neighbour.

 0 glow    soft dot, hot core      4 plus     heal cross            8 scorch   burnt ground       12 smoke b
 1 spark   streak along v          5 shard    glass/metal shard     9 crack    broken asphalt     13 splash crown
 2 smoke   puff                    6 flame    tongue, rising       10 blob     contact shadow     14 ember dot
 3 ring    thin ring               7 flare    four-point star      11 chevron  pointing up        15 band (beams)

Keep the indices in sync with `FxTile` in Runtime/EffectEmitters.cs.
Run with Pillow: `uv run --project ../../tools python Tools~/fx_atlas.py` from the package root.
"""

import math
import pathlib
import random

from PIL import Image

out = pathlib.Path(__file__).resolve().parents[1] / 'Runtime' / 'Textures' / 'T_FxAtlas.png'
TILE, GRID, BORDER = 128, 4, 8


def lattice(period, seed):
    rnd = random.Random(seed)
    return [[rnd.random() for _ in range(period)] for _ in range(period)]


def noise(grid, period, x, y):
    """Tileable smoothstep value noise; x, y in lattice units."""
    xi, yi = math.floor(x), math.floor(y)
    fx, fy = x - xi, y - yi
    fx, fy = fx * fx * (3 - 2 * fx), fy * fy * (3 - 2 * fy)
    a, b = grid[yi % period][xi % period], grid[yi % period][(xi + 1) % period]
    c, d = grid[(yi + 1) % period][xi % period], grid[(yi + 1) % period][(xi + 1) % period]
    return (a + (b - a) * fx) + ((c + (d - c) * fx) - (a + (b - a) * fx)) * fy


def fbm(seed, octaves=4, base=4):
    grids = [(base << k, lattice(base << k, seed + k)) for k in range(octaves)]

    def sample(u, v):
        s, amp, norm = 0.0, 0.5, 0.0
        for period, grid in grids:
            s += amp * noise(grid, period, u * period, v * period)
            norm += amp
            amp *= 0.5
        return s / norm

    return sample


def smooth(e0, e1, x):
    t = min(1.0, max(0.0, (x - e0) / (e1 - e0)))
    return t * t * (3 - 2 * t)


def clamp(x):
    return min(1.0, max(0.0, x))


# Each shape maps tile-local coordinates (x, y in -1..1, y up) to (luminance, alpha).


def glow(x, y):
    r = math.hypot(x, y)
    a = clamp(math.exp(-r * r * 9) * 0.75 + math.exp(-r * r * 60) * 0.6) * (1 - smooth(0.85, 1, r))
    return 1, a


def spark(x, y):
    a = math.exp(-x * x * 160) * clamp(1 - abs(y) ** 1.6) + math.exp(-(x * x * 40 + y * y * 3)) * 0.25
    return 1, clamp(a) * (1 - smooth(0.9, 1, abs(y)))


def puff(seed):
    shape = fbm(seed, 5, 3)

    def tile(x, y):
        r = math.hypot(x, y)
        n = shape(x * 0.5 + 0.5, y * 0.5 + 0.5)
        a = smooth(0.95, 0.25, r + (n - 0.5) * 0.7) * (0.55 + 0.45 * n)
        return 0.75 + 0.25 * n, clamp(a)

    return tile


def ring(x, y):
    r = math.hypot(x, y)
    return 1, clamp(math.exp(-((r - 0.78) ** 2) * 900) + math.exp(-((r - 0.78) ** 2) * 60) * 0.3) * (
        1 - smooth(0.92, 1, r)
    )


def plus(x, y):
    arm = max(0.0, min(1 - smooth(0.14, 0.2, abs(x)) * 1.0, 1 - smooth(0.55, 0.62, abs(y))))
    arm = max(arm, min(1 - smooth(0.14, 0.2, abs(y)), 1 - smooth(0.55, 0.62, abs(x))))
    halo = math.exp(-(x * x + y * y) * 5) * 0.35
    return 1, clamp(arm + halo)


def shard(x, y):
    # A long, slightly skewed triangle with a bright cutting edge.
    u, v = x * 0.9 - y * 0.25, y
    if v < -0.8 or v > 0.85:
        return 1, 0
    half = (0.85 - v) / 1.65 * 0.42
    inside = 1 - smooth(half - 0.04, half, abs(u + 0.06 * v))
    edge = math.exp(-((u + 0.06 * v - half * 0.8) ** 2) * 400) * inside
    return 0.8 + 0.2 * edge, clamp(inside * 0.75 + edge * 0.4)


def flame():
    shape = fbm(41, 4, 4)

    def tile(x, y):
        # Teardrop: wide at the base (y = -0.6), tapering to a tip at y = 0.85, licked by noise.
        t = (y + 0.6) / 1.45
        if t < -0.3 or t > 1:
            return 1, 0
        width = 0.5 * math.sqrt(max(0.0, 1 - t)) * (1 - smooth(-0.3, 0.05, -t) * 0.4)
        n = shape(x * 0.5 + 0.5, y * 0.4 + 0.5)
        body = 1 - smooth(width * 0.6, width + 0.02, abs(x + (n - 0.5) * 0.35 * t))
        base = 1 - smooth(0.0, 0.35, -t) if t < 0 else 1
        core = math.exp(-(x * x * 30 + (t - 0.15) ** 2 * 10))
        return clamp(0.8 + core * 0.2), clamp(body * base * (0.55 + 0.45 * n) + core * 0.35)

    return tile


def flare(x, y):
    r = math.hypot(x, y)
    rays = math.exp(-abs(x) * 60) * (1 - smooth(0, 1, abs(y))) + math.exp(-abs(y) * 60) * (1 - smooth(0, 1, abs(x)))
    return 1, clamp(rays * 0.8 + math.exp(-r * r * 40) * 0.8 + math.exp(-r * r * 6) * 0.25) * (1 - smooth(0.85, 1, r))


def scorch():
    shape = fbm(57, 5, 4)

    def tile(x, y):
        r = math.hypot(x, y)
        n = shape(x * 0.5 + 0.5, y * 0.5 + 0.5)
        a = smooth(1.0, 0.3, r + (n - 0.5) * 0.6) * (0.7 + 0.3 * n)
        return 0.06 + 0.06 * n, clamp(a)

    return tile


def crack():
    rnd = random.Random(73)
    segments = []
    for k in range(7):
        angle = k / 7 * math.tau + rnd.uniform(-0.3, 0.3)
        px, py = 0.0, 0.0
        for _ in range(5):
            angle += rnd.uniform(-0.6, 0.6)
            step = rnd.uniform(0.12, 0.2)
            nx, ny = px + math.cos(angle) * step, py + math.sin(angle) * step
            segments.append((px, py, nx, ny, 1 - math.hypot(px, py)))
            px, py = nx, ny

    def tile(x, y):
        best, strength = 9.0, 0.0
        for ax, ay, bx, by, s in segments:
            dx, dy = bx - ax, by - ay
            t = clamp(((x - ax) * dx + (y - ay) * dy) / (dx * dx + dy * dy))
            d = math.hypot(x - ax - dx * t, y - ay - dy * t)
            if d < best:
                best, strength = d, s
        a = math.exp(-best * best * 4000) * (0.4 + 0.6 * strength) + math.exp(-(x * x + y * y) * 30) * 0.5
        return 0.04, clamp(a) * (1 - smooth(0.85, 1, math.hypot(x, y)))

    return tile


def blob(x, y):
    r = math.hypot(x, y)
    return 0, clamp(1 - smooth(0.15, 0.95, r)) ** 1.5


def chevron(x, y):
    # Upward chevron: two arms meeting at (0, 0.45).
    d = abs((y - 0.45) + abs(x) * 0.9) / math.sqrt(1.81)
    arm = 1 - smooth(0.1, 0.15, d)
    return 1, clamp(arm * (1 - smooth(0.6, 0.7, abs(x))) + math.exp(-d * d * 30) * 0.2 * (1 - smooth(0.6, 0.8, abs(x))))


def splash():
    rnd = random.Random(91)
    drops = [
        (math.cos(a) * r, math.sin(a) * r, s)
        for a, r, s in (
            (k / 11 * math.tau + rnd.uniform(-0.2, 0.2), rnd.uniform(0.5, 0.75), rnd.uniform(0.05, 0.09))
            for k in range(11)
        )
    ]

    def tile(x, y):
        r = math.hypot(x, y)
        a = math.exp(-((r - 0.42) ** 2) * 300) * 0.8
        for dx, dy, s in drops:
            a += math.exp(-((x - dx) ** 2 + (y - dy) ** 2) / (s * s))
        return 1, clamp(a)

    return tile


def ember(x, y):
    r = math.hypot(x, y)
    return 1, clamp(math.exp(-r * r * 40) + math.exp(-r * r * 8) * 0.2) * (1 - smooth(0.8, 1, r))


def band(x, y):
    # Beams: a feathered band across v; u stays uniform so lines stretch it freely.
    return 1, clamp(math.exp(-y * y * 9) * 0.6 + math.exp(-y * y * 60) * 0.6)


SHAPES = [
    glow,
    spark,
    puff(11),
    ring,
    plus,
    shard,
    flame(),
    flare,
    scorch(),
    crack(),
    blob,
    chevron,
    puff(29),
    splash(),
    ember,
    band,
]


def main():
    image = Image.new('RGBA', (TILE * GRID, TILE * GRID), (0, 0, 0, 0))
    pixels = image.load()
    inner = TILE - 2 * BORDER
    for index, shape in enumerate(SHAPES):
        ox, oy = (index % GRID) * TILE + BORDER, (index // GRID) * TILE + BORDER
        for j in range(inner):
            for i in range(inner):
                x = (i + 0.5) / inner * 2 - 1
                y = 1 - (j + 0.5) / inner * 2
                lum, alpha = shape(x, y)
                v = round(clamp(lum) * 255)
                pixels[ox + i, oy + j] = (v, v, v, round(clamp(alpha) * 255))
    out.parent.mkdir(parents=True, exist_ok=True)
    image.save(out, optimize=True)
    print(f'wrote {out.name}: {len(SHAPES)} tiles')


if __name__ == '__main__':
    main()
