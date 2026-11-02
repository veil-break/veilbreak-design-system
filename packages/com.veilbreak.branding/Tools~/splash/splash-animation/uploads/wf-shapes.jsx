/* eslint-disable */
/* MONO Wireframe — wire polyhedra (CSS 3D), static + rotating + click ripple */
const { useMemo: useMemoS, useRef: useRefS } = React;

function genShape(shape) {
  const phi = (1 + Math.sqrt(5)) / 2;
  let verts, edges;
  if (shape === "tetra") {
    verts = [[1, 1, 1], [1, -1, -1], [-1, 1, -1], [-1, -1, 1]];
    edges = [[0, 1], [0, 2], [0, 3], [1, 2], [1, 3], [2, 3]];
  } else if (shape === "cube") {
    verts = [[-1, -1, -1], [1, -1, -1], [1, 1, -1], [-1, 1, -1], [-1, -1, 1], [1, -1, 1], [1, 1, 1], [-1, 1, 1]];
    edges = [[0, 1], [1, 2], [2, 3], [3, 0], [4, 5], [5, 6], [6, 7], [7, 4], [0, 4], [1, 5], [2, 6], [3, 7]];
  } else if (shape === "octa") {
    verts = [[1, 0, 0], [-1, 0, 0], [0, 1, 0], [0, -1, 0], [0, 0, 1], [0, 0, -1]];
    edges = [[0, 2], [0, 3], [0, 4], [0, 5], [1, 2], [1, 3], [1, 4], [1, 5], [2, 4], [2, 5], [3, 4], [3, 5]];
  } else if (shape === "diamond") {
    // brilliant-cut gem: octagonal table ring (top), girdle ring (mid, wide), culet apex (bottom)
    const N = 8, table = [], girdle = [];
    for (let i = 0; i < N; i++) {
      const a = (i / N) * Math.PI * 2;
      table.push([Math.cos(a) * 0.52, 0.62, Math.sin(a) * 0.52]);
      const ag = a + Math.PI / N;
      girdle.push([Math.cos(ag) * 1.0, 0.06, Math.sin(ag) * 1.0]);
    }
    const apex = [0, -1.15, 0];
    verts = [...table, ...girdle, apex];
    const apexI = 2 * N;
    edges = [];
    for (let i = 0; i < N; i++) {
      edges.push([i, (i + 1) % N]);                 // table polygon
      edges.push([N + i, N + (i + 1) % N]);          // girdle polygon
      edges.push([i, N + i]);                        // table → girdle (crown facet)
      edges.push([i, N + (i + N - 1) % N]);          // table → girdle (other side)
      edges.push([N + i, apexI]);                    // girdle → culet (pavilion)
    }
  } else { // icosa
    verts = [
      [-1, phi, 0], [1, phi, 0], [-1, -phi, 0], [1, -phi, 0],
      [0, -1, phi], [0, 1, phi], [0, -1, -phi], [0, 1, -phi],
      [phi, 0, -1], [phi, 0, 1], [-phi, 0, -1], [-phi, 0, 1],
    ];
    edges = [
      [0, 1], [0, 5], [0, 7], [0, 10], [0, 11], [1, 5], [1, 7], [1, 8], [1, 9],
      [2, 3], [2, 4], [2, 6], [2, 10], [2, 11], [3, 4], [3, 6], [3, 8], [3, 9],
      [4, 5], [4, 9], [4, 11], [5, 9], [5, 11], [6, 7], [6, 8], [6, 10],
      [7, 8], [7, 10], [8, 9], [10, 11],
    ];
  }
  // normalize to unit-ish radius
  const maxR = Math.max(...verts.map((v) => Math.hypot(...v)));
  verts = verts.map((v) => v.map((c) => c / maxR));
  return { verts, edges };
}

// ─────────────────────────────────────────────────────────────
// WirePolyhedron — CSS-3D wireframe. axis: y|x|z|xy|yz, static bool,
// click ripples light outward over the edge graph.
// ─────────────────────────────────────────────────────────────
function WirePolyhedron({ shape = "icosa", size = 160, speed = 14, axis = "y", accent = "turquoise", isStatic = false, tiltX = -22, tiltY = 0, tiltZ = 0 }) {
  const ref = useRefS(null);
  const { verts, edges } = useMemoS(() => genShape(shape), [shape]);
  const R = size * 0.42;

  const edgeBars = useMemoS(() => edges.map(([i, j]) => {
    const a = verts[i].map((c) => c * R), b = verts[j].map((c) => c * R);
    const dx = b[0] - a[0], dy = b[1] - a[1], dz = b[2] - a[2];
    const len = Math.hypot(dx, dy, dz);
    const ux = dx / len, uy = dy / len, uz = dz / len;
    const ry = Math.atan2(-uz, ux), rz = Math.atan2(uy, Math.hypot(ux, uz));
    return { key: `${i}|${j}`, len, transform: `translate3d(${a[0].toFixed(2)}px, ${a[1].toFixed(2)}px, ${a[2].toFixed(2)}px) rotateY(${ry.toFixed(4)}rad) rotateZ(${rz.toFixed(4)}rad)` };
  }), [verts, edges, R]);

  const vertTfs = useMemoS(() => verts.map((v) => `translate3d(${(v[0] * R).toFixed(2)}px, ${(v[1] * R).toFixed(2)}px, ${(v[2] * R).toFixed(2)}px)`), [verts, R]);
  const adj = useMemoS(() => window.buildAdj(edges.map(([i, j]) => [String(i), String(j)])), [edges]);

  const onClick = () => {
    // seed ripple at vertex nearest a deterministic-ish entry (top vertex)
    let top = 0, ty = Infinity;
    verts.forEach((v, i) => { if (v[1] < ty) { ty = v[1]; top = i; } });
    const dist = window.bfsRings(String(top), adj);
    window.runRipple(ref.current, dist, { step: 70, dur: 760, edgeAnim: "wfRipple3dA", nodeAnim: "wfRippleVertA" });
  };

  return (
    <span ref={ref} className="wf-poly3d" data-shape={shape} data-axis={axis} data-accent={accent}
      {...(isStatic ? { "data-static": "" } : {})}
      onClick={onClick}
      style={{ "--size": `${size}px`, "--spin": `${speed}s`, "--persp": `${(size * 3).toFixed(0)}px`, "--tiltX": `${tiltX}deg`, "--tiltY": `${tiltY}deg`, "--tiltZ": `${tiltZ}deg`, "--rip-c": window.ACCENT_HEX[accent] }}>
      <span className="poly-tilt">
        <span className="poly-scene">
          {edgeBars.map((e) => <i key={e.key} className="poly-edge" data-redge={e.key} style={{ width: `${e.len.toFixed(2)}px`, transform: e.transform }} />)}
          {vertTfs.map((tf, i) => <i key={"v" + i} className="poly-vert" data-rnode={String(i)} style={{ transform: tf, "--vtf": tf }} />)}
        </span>
      </span>
    </span>
  );
}

Object.assign(window, { WirePolyhedron, genShape });
