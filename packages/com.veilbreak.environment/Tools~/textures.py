#!/usr/bin/env python3
"""Regenerate the environment's procedural textures in Runtime/Textures. Deterministic: the same script writes the
same bytes.

T_Noise.png    256 px, tiles: r large fBm (puddles), g fine fBm (grime), b crack network (Worley F2-F1), a broad patches
T_Glow.png     64 px soft radial falloff with a hot core: light pools, halos, embers, fog motes
T_LensDirt_BC.png  512 px smudges and specks for bloom's lens dirt (ports the donor's procedural T_LensDirt_BC)

Run with Pillow: `uv run --project ../../../tools python Tools~/textures.py` from the package root.
"""

import math
import pathlib
import random

from PIL import Image

out = pathlib.Path(__file__).resolve().parents[1] / 'Runtime' / 'Textures'


def lattice(period, seed):
    rnd = random.Random(seed)
    return [[rnd.random() for _ in range(period)] for _ in range(period)]


def value(grid, period, x, y):
    """Tileable smoothstep value noise; x, y in lattice units."""
    xi, yi = math.floor(x), math.floor(y)
    fx, fy = x - xi, y - yi
    fx, fy = fx * fx * (3 - 2 * fx), fy * fy * (3 - 2 * fy)
    a, b = grid[yi % period][xi % period], grid[yi % period][(xi + 1) % period]
    c, d = grid[(yi + 1) % period][xi % period], grid[(yi + 1) % period][(xi + 1) % period]
    return (a + (b - a) * fx) + ((c + (d - c) * fx) - (a + (b - a) * fx)) * fy


def fbm(size, base, octaves, seed):
    grids = [(base << k, lattice(base << k, seed + k)) for k in range(octaves)]
    rows = []
    for y in range(size):
        row = []
        for x in range(size):
            s, amp, norm = 0.0, 0.5, 0.0
            for period, grid in grids:
                s += amp * value(grid, period, x * period / size, y * period / size)
                norm += amp
                amp *= 0.5
            row.append(s / norm)
        rows.append(row)
    return rows


def stretch(rows):
    """Remap to the full 0..1 range so thresholds in the shaders mean the same thing for every channel."""
    lo = min(min(r) for r in rows)
    hi = max(max(r) for r in rows)
    return [[(v - lo) / (hi - lo) for v in r] for r in rows]


def cracks(size, cells, seed):
    rnd = random.Random(seed)
    points = [[(rnd.random(), rnd.random()) for _ in range(cells)] for _ in range(cells)]
    rows = []
    for y in range(size):
        row = []
        for x in range(size):
            gx, gy = x * cells / size, y * cells / size
            cx, cy = int(gx), int(gy)
            f1 = f2 = 9.0
            for oy in (-1, 0, 1):
                for ox in (-1, 0, 1):
                    px, py = points[(cy + oy) % cells][(cx + ox) % cells]
                    d = math.hypot(cx + ox + px - gx, cy + oy + py - gy)
                    if d < f1:
                        f1, f2 = d, f1
                    elif d < f2:
                        f2 = d
            row.append(max(0.0, 1.0 - (f2 - f1) * 9.0) ** 2)
        rows.append(row)
    return rows


def noise():
    size = 256
    r = stretch(fbm(size, 4, 5, 11))
    g = stretch(fbm(size, 16, 4, 31))
    b = cracks(size, 10, 47)
    a = stretch(fbm(size, 2, 3, 59))
    img = Image.new('RGBA', (size, size))
    img.putdata([tuple(round(c[y][x] * 255) for c in (r, g, b, a)) for y in range(size) for x in range(size)])
    img.save(out / 'T_Noise.png', optimize=True)


def glow():
    n = 64
    img = Image.new('RGBA', (n, n))
    px = []
    for y in range(n):
        for x in range(n):
            d = math.hypot(x + 0.5 - n / 2, y + 0.5 - n / 2) / (n / 2)
            a = min(1.0, max(0.0, 1 - d) ** 2.2 + max(0.0, 1 - d * 3) ** 2 * 0.6)
            px.append((255, 255, 255, round(a * 255)))
    img.putdata(px)
    img.save(out / 'T_Glow.png', optimize=True)


def lens_dirt():
    n = 512
    rnd = random.Random(73)
    acc = [[0.0] * n for _ in range(n)]

    def blob(cx, cy, radius, strength, ring):
        x0, x1 = max(0, int(cx - radius)), min(n, int(cx + radius) + 1)
        y0, y1 = max(0, int(cy - radius)), min(n, int(cy + radius) + 1)
        for y in range(y0, y1):
            for x in range(x0, x1):
                d = math.hypot(x - cx, y - cy) / radius
                if d < 1:
                    v = (1 - d * d) ** 2
                    if ring:
                        v = v * 0.4 + math.exp(-(((d - 0.85) / 0.08) ** 2)) * 0.6
                    acc[y][x] += v * strength

    for _ in range(46):  # large soft smudges, denser toward the corners where real lenses collect grime
        cx, cy = rnd.random() * n, rnd.random() * n
        edge = max(abs(cx / n - 0.5), abs(cy / n - 0.5)) * 2
        blob(cx, cy, rnd.uniform(30, 90), rnd.uniform(0.05, 0.16) * (0.5 + edge), False)
    for _ in range(90):  # bokeh-like rings from dried droplets
        blob(rnd.random() * n, rnd.random() * n, rnd.uniform(6, 22), rnd.uniform(0.15, 0.4), True)
    for _ in range(420):  # dust specks
        blob(rnd.random() * n, rnd.random() * n, rnd.uniform(1.2, 3.5), rnd.uniform(0.3, 0.8), False)
    img = Image.new('RGB', (n, n))
    levels = [min(1.0, v) for row in acc for v in row]
    img.putdata([(int(v * 235), int(v * 228), int(v * 255)) for v in levels])
    img.save(out / 'T_LensDirt_BC.png', optimize=True)


if __name__ == '__main__':
    noise()
    glow()
    lens_dirt()
    print('wrote T_Noise.png, T_Glow.png, T_LensDirt_BC.png to', out)
