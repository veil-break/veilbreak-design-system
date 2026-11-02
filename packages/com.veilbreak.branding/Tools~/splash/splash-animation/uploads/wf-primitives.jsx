/* eslint-disable */
/* MONO Wireframe — primitives (spinner, terrain, neural nets, icosahedron) */

const { useEffect, useRef, useState, useMemo } = React;

// ─────────────────────────────────────────────────────────────
// Shared ripple/propagation helpers (exposed on window for sibling files)
// ─────────────────────────────────────────────────────────────
function buildAdj(pairs) {
  const m = new Map();
  const add = (a, b) => { if (!m.has(a)) m.set(a, []); m.get(a).push(b); };
  pairs.forEach(([a, b]) => { add(a, b); add(b, a); });
  return m;
}
// breadth-first distance rings from a seed key
function bfsRings(start, adj) {
  const dist = new Map([[start, 0]]);
  let fr = [start], d = 0;
  while (fr.length) {
    const nx = [];
    for (const k of fr) for (const nb of (adj.get(k) || [])) {
      if (!dist.has(nb)) { dist.set(nb, d + 1); nx.push(nb); }
    }
    fr = nx; d++;
  }
  return dist;
}
// imperatively flash edges + nodes ring-by-ring (a ripple of light)
function runRipple(root, dist, opts = {}) {
  if (!root) return;
  const step = opts.step ?? 52;
  const dur  = opts.dur ?? 720;
  const eName = opts.edgeAnim || "wfRippleA";
  const nName = opts.nodeAnim || "wfRippleNodeA";
  const edges = root.querySelectorAll("[data-redge]");
  const nodes = root.querySelectorAll("[data-rnode]");
  edges.forEach((el) => { el.style.animation = "none"; });
  nodes.forEach((el) => { el.style.animation = "none"; });
  void root.getBoundingClientRect(); // force reflow → restart
  let maxRing = 0;
  edges.forEach((el) => {
    const [a, b] = el.getAttribute("data-redge").split("|");
    const du = dist.has(a) ? dist.get(a) : Infinity;
    const dv = dist.has(b) ? dist.get(b) : Infinity;
    const r = Math.min(du, dv);
    if (!isFinite(r)) return;
    maxRing = Math.max(maxRing, r);
    el.style.animation = `${eName} ${dur}ms ease-out ${r * step}ms`;
  });
  nodes.forEach((el) => {
    const k = el.getAttribute("data-rnode");
    const r = dist.has(k) ? dist.get(k) : Infinity;
    if (!isFinite(r)) return;
    maxRing = Math.max(maxRing, r);
    el.style.animation = `${nName} ${dur}ms ease-out ${r * step}ms`;
  });
  clearTimeout(root.__ripT);
  root.__ripT = setTimeout(() => {
    edges.forEach((el) => { el.style.animation = ""; });
    nodes.forEach((el) => { el.style.animation = ""; });
  }, dur + maxRing * step + 80);
}
// pointer → viewBox coords for a slice-fit svg
function pointerToVB(svg, ev, W, H) {
  const r = svg.getBoundingClientRect();
  const scale = Math.max(r.width / W, r.height / H);
  const offX = (r.width - W * scale) / 2;
  const offY = (r.height - H * scale) / 2;
  return [(ev.clientX - r.left - offX) / scale, (ev.clientY - r.top - offY) / scale];
}

const NN_ACCENT = (a) => a === "crimson" ? "var(--accent-crimson)" : a === "violet" ? "var(--accent-violet)" : a === "iris" ? "#5cd1ff" : a === "ember" ? "#ffd166" : "var(--accent-turquoise)";
const ACCENT_HEX = { turquoise: "#00e5d1", crimson: "#ff2e7e", violet: "#c8b6ff", iris: "#5cd1ff", ember: "#ffd166" };

// deterministic small PRNG from an integer seed
function lcg(seed) { let s = (seed | 0) || 1; return () => (s = (s * 1103515245 + 12345) & 0x7fffffff) / 0x7fffffff; }

// ─────────────────────────────────────────────────────────────
// TriangleSpinner — bold line traced over hairline triangle from corner
// ─────────────────────────────────────────────────────────────
function TriangleSpinner({ size = 56, color = "turquoise", tilt = -12 }) {
  const cx = 50, cy = 50, r = 40;
  const a = [cx, cy - r];
  const b = [cx + r * Math.cos(Math.PI / 6), cy + r * Math.sin(Math.PI / 6)];
  const c = [cx - r * Math.cos(Math.PI / 6), cy + r * Math.sin(Math.PI / 6)];
  const side = Math.hypot(b[0] - a[0], b[1] - a[1]);
  const peri = side * 3;
  return (
    <span className="wf-tri-spin" data-color={color} style={{ "--size": `${size}px`, "--peri": peri, transform: `rotate(${tilt}deg)` }}>
      <svg viewBox="0 0 100 100">
        <polygon className="base" points={`${a[0]},${a[1]} ${b[0]},${b[1]} ${c[0]},${c[1]}`} />
        <polyline className="trace" points={`${a[0]},${a[1]} ${b[0]},${b[1]} ${c[0]},${c[1]} ${a[0]},${a[1]}`} />
      </svg>
    </span>
  );
}

// ─────────────────────────────────────────────────────────────
// GroundPlane — triangulated terrain mesh with rolling hills,
// perspective projection, fog mask, click-ripple over the mesh.
// ─────────────────────────────────────────────────────────────
function GroundPlane({ rows = 12, cols = 20, hue = "turquoise", height = 320, amp = 1, seed = 7 }) {
  const W = 1200, H = height;
  const cam = { fov: 700, cx: W / 2, cy: H * 0.42 };
  const svgRef = useRef(null);

  // pseudo-random ridge offsets so each instance/seed gives a distinct landscape
  const terrain = useMemo(() => {
    const rnd = lcg(seed * 2654435761);
    const bumps = [];
    const nBumps = 4 + Math.floor(rnd() * 3);
    for (let i = 0; i < nBumps; i++) {
      bumps.push({
        x: (rnd() * 2 - 1) * 0.9,
        z: rnd(),
        s: 0.04 + rnd() * 0.10,
        h: (0.6 + rnd() * 0.9) * (rnd() > 0.25 ? 1 : -0.6), // mostly hills, occasional dip
      });
    }
    const pa = rnd() * 6.28, pb = rnd() * 6.28;
    return (nx, nz) => {
      let h = Math.sin(nx * 3.0 + pa) * 0.32 + Math.cos(nz * 4.2 + nx * 1.6 + pb) * 0.30;
      for (const b of bumps) {
        const d2 = ((nx - b.x) ** 2 + (nz - b.z) ** 2) / b.s;
        h += b.h * Math.exp(-d2);
      }
      // flatten toward the near edge (foreground), let hills rise into distance
      return h * (0.35 + nz * 0.85);
    };
  }, [seed]);

  const { segs, vertScreen } = useMemo(() => {
    const xMin = -1400, xMax = 1400, zMax = 2400;
    const P = {};      // key -> {sx,sy,j}
    const key = (i, j) => `${i}_${j}`;
    for (let j = 0; j <= rows; j++) {
      const t = j / rows;
      const worldZ = zMax * t * t;
      const nz = t;
      for (let i = 0; i <= cols; i++) {
        const worldX = xMin + (xMax - xMin) * (i / cols);
        const nx = (i / cols) * 2 - 1;
        const hWorld = terrain(nx, nz) * amp * 200;
        const zz = worldZ + 230;
        const sx = cam.cx + (worldX * cam.fov) / zz;
        const sy = cam.cy + ((215 - hWorld) * cam.fov) / zz;
        P[key(i, j)] = { sx, sy, j, h: hWorld };
      }
    }
    const segs = [];
    const push = (i1, j1, i2, j2) => {
      const a = P[key(i1, j1)], b = P[key(i2, j2)];
      const nearF = 1 - ((a.j + b.j) / 2) / rows;            // 1 near camera → 0 far
      const ridge = Math.max(0, (a.h + b.h) / 2) / 260;       // bright crests
      const op = Math.max(0.05, Math.min(0.75, 0.12 + nearF * 0.42 + ridge * 0.5));
      segs.push({ x1: a.sx, y1: a.sy, x2: b.sx, y2: b.sy, u: key(i1, j1), v: key(i2, j2), op });
    };
    for (let j = 0; j <= rows; j++)
      for (let i = 0; i < cols; i++) push(i, j, i + 1, j);   // lateral
    for (let j = 0; j < rows; j++)
      for (let i = 0; i <= cols; i++) push(i, j, i, j + 1);  // longitudinal
    for (let j = 0; j < rows; j++)
      for (let i = 0; i < cols; i++) push(i, j, i + 1, j + 1); // diagonal → triangles
    return { segs, vertScreen: P };
  }, [rows, cols, height, amp, terrain]);

  const adj = useMemo(() => buildAdj(segs.map((s) => [s.u, s.v])), [segs]);
  const stroke = hue === "crimson" ? "var(--accent-crimson)" : hue === "violet" ? "var(--accent-violet)" : hue === "ember" ? "#ffd166" : "var(--accent-turquoise)";
  const ripC = hue === "crimson" ? ACCENT_HEX.crimson : hue === "violet" ? ACCENT_HEX.violet : hue === "ember" ? ACCENT_HEX.ember : ACCENT_HEX.turquoise;

  const onClick = (ev) => {
    const svg = svgRef.current;
    const [vx, vy] = pointerToVB(svg, ev, W, H);
    let best = null, bestD = Infinity;
    for (const k in vertScreen) {
      const p = vertScreen[k];
      const d = (p.sx - vx) ** 2 + (p.sy - vy) ** 2;
      if (d < bestD) { bestD = d; best = k; }
    }
    if (best) runRipple(svg, bfsRings(best, adj), { step: 40, dur: 760 });
  };

  return (
    <svg ref={svgRef} className="wf-svg wf-ground" viewBox={`0 0 ${W} ${H}`} preserveAspectRatio="xMidYMid slice"
      style={{ width: "100%", height, "--rip-c": ripC }} onClick={onClick}>
      <defs>
        <linearGradient id={`gpFade-${hue}`} x1="0" y1="0" x2="0" y2="1">
          <stop offset="0%" stopColor="white" stopOpacity="0" />
          <stop offset="32%" stopColor="white" stopOpacity="1" />
          <stop offset="100%" stopColor="white" stopOpacity="0.35" />
        </linearGradient>
        <mask id={`gpMask-${hue}`}><rect width={W} height={H} fill={`url(#gpFade-${hue})`} /></mask>
      </defs>
      <g mask={`url(#gpMask-${hue})`}>
        {segs.map((s, i) => (
          <line key={i} data-redge={`${s.u}|${s.v}`}
            x1={s.x1} y1={s.y1} x2={s.x2} y2={s.y2}
            stroke={stroke} strokeOpacity={s.op} strokeWidth="1" vectorEffect="non-scaling-stroke" />
        ))}
      </g>
    </svg>
  );
}

// ─────────────────────────────────────────────────────────────
// computeLit — hovered node → direct (l1) vs second-hop (l2) sets
// ─────────────────────────────────────────────────────────────
function computeLit(hover, links) {
  const empty = { l1: new Set(), l2: new Set(), n1: new Set(), n2: new Set() };
  if (hover == null) return empty;
  const l1 = new Set(), n1 = new Set();
  links.forEach((l, i) => {
    if (l.from === hover || l.to === hover) { l1.add(i); n1.add(l.from === hover ? l.to : l.from); }
  });
  const l2 = new Set(), n2 = new Set();
  links.forEach((l, i) => {
    if (l1.has(i)) return;
    const fromN = n1.has(l.from), toN = n1.has(l.to);
    if (fromN || toN) { l2.add(i); const other = fromN ? l.to : l.from; if (other !== hover && !n1.has(other)) n2.add(other); }
  });
  return { l1, l2, n1, n2 };
}

// deterministic forward fan-out: node (li,ai) of a layer with nCur nodes →
// up to `fanout` nodes in the next layer (nNext). All targets in [0,nNext).
function fanTargets(li, ai, nCur, nNext, fanout) {
  const count = Math.max(1, Math.min(nNext, fanout));
  const out = new Set();
  const rnd = lcg((li * 9277 + ai * 2999 + 17));
  const ratio = nCur > 1 ? ai / (nCur - 1) : 0.5;       // 0..1 position in this layer
  const center = Math.max(0, Math.min(nNext - 1, Math.round(ratio * (nNext - 1))));
  out.add(center);
  let guard = 0;
  while (out.size < count && guard++ < 40) out.add(Math.floor(rnd() * nNext));
  return [...out];
}

// ─────────────────────────────────────────────────────────────
// NeuralNet — sparse layered graph (2-4 fan-out), hover relations,
// click-ripple forward through the network.
// ─────────────────────────────────────────────────────────────
function NeuralNet({ layers = [4, 7, 7, 3], width = 520, height = 260, accent = "turquoise", fanout = 3 }) {
  const padX = 30, padY = 26, W = width, H = height;
  const colW = (W - padX * 2) / (layers.length - 1);
  const [hover, setHover] = useState(null);
  const svgRef = useRef(null);

  const positions = layers.map((n, li) => {
    const arr = [];
    const rowH = (H - padY * 2) / Math.max(1, n - 1);
    for (let i = 0; i < n; i++) arr.push({ x: padX + colW * li, y: n === 1 ? H / 2 : padY + rowH * i, key: `${li}-${i}` });
    return arr;
  });

  const links = useMemo(() => {
    const out = [];
    for (let li = 0; li < layers.length - 1; li++) {
      positions[li].forEach((a, ai) => {
        fanTargets(li, ai, layers[li], layers[li + 1], fanout).forEach((bi) => {
          const b = positions[li + 1][bi];
          const hot = ((ai * 17 + bi * 13 + li * 7) % 4 === 0);
          const delay = ((ai + bi * 2 + li * 3) % 7) * 0.4;
          out.push({ a, b, hot, delay, from: a.key, to: b.key });
        });
      });
    }
    return out;
  }, [layers.join(","), fanout, width, height]);

  const adj = useMemo(() => buildAdj(links.map((l) => [l.from, l.to])), [links]);
  const accentVar = NN_ACCENT(accent);
  const ripC = ACCENT_HEX[accent] || ACCENT_HEX.turquoise;
  const lit = computeLit(hover, links);
  const hovering = hover != null;

  const fire = (key) => runRipple(svgRef.current, bfsRings(key, adj), { step: 80, dur: 700 });

  return (
    <svg ref={svgRef} className={"wf-svg wf-nn" + (hovering ? " is-hovering" : "")} viewBox={`0 0 ${W} ${H}`}
      style={{ width: "100%", height, "--nn-accent": accentVar, "--rip-c": ripC }} onMouseLeave={() => setHover(null)}>
      {links.map((l, i) => {
        const cls = lit.l1.has(i) ? " lit" : lit.l2.has(i) ? " lit2" : "";
        return (
          <line key={i} data-redge={`${l.from}|${l.to}`}
            className={"link" + (l.hot ? " hot" : "") + cls}
            x1={l.a.x} y1={l.a.y} x2={l.b.x} y2={l.b.y}
            style={l.hot ? { animationDelay: `${l.delay}s`, stroke: accentVar } : null} />
        );
      })}
      {positions.flatMap((layer, li) => layer.map((p, i) => {
        const active = (li + i) % 4 === 0;
        const ncls = hover === p.key ? " lit hot-node" : lit.n1.has(p.key) ? " lit" : lit.n2.has(p.key) ? " lit2" : (active ? " active" : "");
        return (
          <g key={p.key}>
            <circle data-rnode={p.key} cx={p.x} cy={p.y} r="4" className={"node wf-nodeflash" + ncls}
              style={active && !hovering ? { fill: accentVar, stroke: accentVar } : null} />
            <circle cx={p.x} cy={p.y} r="12" fill="transparent" style={{ cursor: "pointer" }}
              onMouseEnter={() => setHover(p.key)} onClick={() => fire(p.key)} />
          </g>
        );
      }))}
    </svg>
  );
}

// ─────────────────────────────────────────────────────────────
// NeuralNet3D — sparse layered graph in 3D, rotates around Y,
// hover relation map, click-ripple.
// ─────────────────────────────────────────────────────────────
function NeuralNet3D({ size = 300, layers = [3, 6, 6, 3], speed = 16, accent = "iris", fanout = 3 }) {
  const [angle, setAngle] = useState(0.5);
  const [hover, setHover] = useState(null);
  const hoverRef = useRef(null); hoverRef.current = hover;
  const svgRef = useRef(null);
  const raf = useRef(0); const last = useRef(performance.now());
  useEffect(() => {
    const loop = (t) => {
      const dt = (t - last.current) / 1000; last.current = t;
      if (hoverRef.current == null) setAngle((a) => a + dt * (2 * Math.PI / speed));
      raf.current = requestAnimationFrame(loop);
    };
    raf.current = requestAnimationFrame(loop);
    return () => cancelAnimationFrame(raf.current);
  }, [speed]);

  const model = useMemo(() => {
    const L = layers.length;
    const nodes = [];
    layers.forEach((n, li) => {
      const lx = (L === 1 ? 0 : (li / (L - 1) - 0.5) * 2) * 1.05;
      const rad = 0.62, phase = li * 0.7;
      for (let i = 0; i < n; i++) {
        let my = 0, mz = 0;
        if (n > 1) { const ang = (2 * Math.PI * i) / n + phase; my = rad * Math.cos(ang); mz = rad * Math.sin(ang); }
        nodes.push({ key: `${li}-${i}`, li, mx: lx, my, mz });
      }
    });
    const byLayer = layers.map((n, li) => nodes.filter((nd) => nd.li === li));
    const links = [];
    for (let li = 0; li < L - 1; li++) {
      byLayer[li].forEach((a, ai) => {
        fanTargets(li, ai, byLayer[li].length, layers[li + 1], fanout).forEach((bi) => links.push({ from: a.key, to: byLayer[li + 1][bi].key }));
      });
    }
    return { nodes, links };
  }, [layers.join(","), fanout]);

  const adj = useMemo(() => buildAdj(model.links.map((l) => [l.from, l.to])), [model]);
  const accentVar = NN_ACCENT(accent);
  const ripC = ACCENT_HEX[accent] || ACCENT_HEX.iris;
  const lit = computeLit(hover, model.links);

  const cosA = Math.cos(angle), sinA = Math.sin(angle);
  const tilt = 0.34, cosT = Math.cos(tilt), sinT = Math.sin(tilt);
  const R = size * 0.36, fov = 3.0, P = {};
  model.nodes.forEach((nd) => {
    const x1 = cosA * nd.mx + sinA * nd.mz, z1 = -sinA * nd.mx + cosA * nd.mz;
    const y2 = cosT * nd.my - sinT * z1, z2 = sinT * nd.my + cosT * z1;
    const persp = fov / (fov - z2);
    P[nd.key] = { px: size / 2 + x1 * R * persp, py: size / 2 + y2 * R * persp, z: z2 };
  });

  const isLit = (i) => lit.l1.has(i) || lit.l2.has(i);
  const edgeOrder = model.links.map((l, i) => i).sort((a, b) => {
    const la = isLit(a) ? 1 : 0, lb = isLit(b) ? 1 : 0;
    if (la !== lb) return la - lb;
    return (P[model.links[a].from].z + P[model.links[a].to].z) - (P[model.links[b].from].z + P[model.links[b].to].z);
  });
  const nodeOrder = model.nodes.map((nd, i) => i).sort((a, b) => {
    const na = model.nodes[a], nb = model.nodes[b];
    const la = hover === na.key || lit.n1.has(na.key) || lit.n2.has(na.key) ? 1 : 0;
    const lb = hover === nb.key || lit.n1.has(nb.key) || lit.n2.has(nb.key) ? 1 : 0;
    if (la !== lb) return la - lb;
    return P[na.key].z - P[nb.key].z;
  });

  const fire = (key) => runRipple(svgRef.current, bfsRings(key, adj), { step: 78, dur: 700 });

  return (
    <span className={"wf-nn3d" + (hover != null ? " is-hovering" : "")} data-accent={accent}
      style={{ "--size": `${size}px`, "--nn-accent": accentVar, "--rip-c": ripC }}>
      <svg ref={svgRef} viewBox={`0 0 ${size} ${size}`} onMouseLeave={() => setHover(null)}>
        {edgeOrder.map((i) => {
          const l = model.links[i]; const a = P[l.from], b = P[l.to];
          const depth = (a.z + b.z) / 2;
          const cls = lit.l1.has(i) ? " lit" : lit.l2.has(i) ? " lit2" : "";
          return <line key={i} data-redge={`${l.from}|${l.to}`} className={"edge" + cls} x1={a.px} y1={a.py} x2={b.px} y2={b.py}
            style={cls ? null : { strokeOpacity: 0.08 + (depth + 1) / 2 * 0.22 }} />;
        })}
        {nodeOrder.map((i) => {
          const nd = model.nodes[i]; const p = P[nd.key];
          const r = 2.4 + (p.z + 1) / 2 * 2.2;
          const ncls = hover === nd.key ? " lit hot-node" : lit.n1.has(nd.key) ? " lit" : lit.n2.has(nd.key) ? " lit2" : "";
          return (
            <g key={nd.key}>
              <circle data-rnode={nd.key} cx={p.px} cy={p.py} r={r} className={"node wf-nodeflash" + ncls} style={ncls ? null : { fillOpacity: 0.32 + (p.z + 1) / 2 * 0.5 }} />
              <circle cx={p.px} cy={p.py} r="11" fill="transparent" style={{ cursor: "pointer" }} onMouseEnter={() => setHover(nd.key)} onClick={() => fire(nd.key)} />
            </g>
          );
        })}
      </svg>
    </span>
  );
}

// ─────────────────────────────────────────────────────────────
// Icosahedron — rigid CSS-3D wireframe; one keyframe spins the scene.
// ─────────────────────────────────────────────────────────────
function Icosahedron({ size = 220, speed = 9, accent = false, paused = false }) {
  const { verts, edges } = useMemo(() => {
    const phi = (1 + Math.sqrt(5)) / 2;
    let v = [
      [-1, phi, 0], [1, phi, 0], [-1, -phi, 0], [1, -phi, 0],
      [0, -1, phi], [0, 1, phi], [0, -1, -phi], [0, 1, -phi],
      [phi, 0, -1], [phi, 0, 1], [-phi, 0, -1], [-phi, 0, 1],
    ];
    const norm = Math.sqrt(1 + phi * phi);
    v = v.map(([x, y, z]) => [x / norm, y / norm, z / norm]);
    const edges = [
      [0, 1], [0, 5], [0, 7], [0, 10], [0, 11], [1, 5], [1, 7], [1, 8], [1, 9],
      [2, 3], [2, 4], [2, 6], [2, 10], [2, 11], [3, 4], [3, 6], [3, 8], [3, 9],
      [4, 5], [4, 9], [4, 11], [5, 9], [5, 11], [6, 7], [6, 8], [6, 10],
      [7, 8], [7, 10], [8, 9], [10, 11],
    ];
    return { verts: v, edges };
  }, []);
  const R = size * 0.42;
  const edgeBars = useMemo(() => edges.map(([i, j]) => {
    const a = verts[i].map((c) => c * R), b = verts[j].map((c) => c * R);
    const dx = b[0] - a[0], dy = b[1] - a[1], dz = b[2] - a[2];
    const len = Math.hypot(dx, dy, dz);
    const ux = dx / len, uy = dy / len, uz = dz / len;
    const ry = Math.atan2(-uz, ux), rz = Math.atan2(uy, Math.hypot(ux, uz));
    return { len, transform: `translate3d(${a[0].toFixed(2)}px, ${a[1].toFixed(2)}px, ${a[2].toFixed(2)}px) rotateY(${ry.toFixed(4)}rad) rotateZ(${rz.toFixed(4)}rad)` };
  }), [verts, edges, R]);
  const playState = paused ? "paused" : "running";
  return (
    <span className="wf-ico3d" data-accent={accent ? "" : undefined}
      style={{ "--size": `${size}px`, "--spin": `${speed}s`, "--persp": `${(size * 2.4).toFixed(0)}px` }}>
      <span className="ico3d-tilt">
        <span className="ico3d-scene" style={{ animationPlayState: playState }}>
          {edgeBars.map((e, k) => <i key={k} className="ico3d-edge" style={{ width: `${e.len.toFixed(2)}px`, transform: e.transform }} />)}
          {verts.map((v, i) => <i key={"v" + i} className="ico3d-vert" style={{ transform: `translate3d(${(v[0] * R).toFixed(2)}px, ${(v[1] * R).toFixed(2)}px, ${(v[2] * R).toFixed(2)}px)` }} />)}
        </span>
      </span>
    </span>
  );
}

Object.assign(window, {
  TriangleSpinner, GroundPlane, NeuralNet, NeuralNet3D, Icosahedron,
  buildAdj, bfsRings, runRipple, pointerToVB, NN_ACCENT, ACCENT_HEX, lcg,
});
