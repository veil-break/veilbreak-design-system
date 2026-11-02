// Veilbreak boot splash — the blade cuts VEIL/BREAK and its momentum breaks the mark.
// Part geometry is copied from uploads/logo-start.svg; every keyframe pose
// (start, 25, 50, Artboard-4) is a pure translation of those parts.
(function () {
  const VB = {
    ground: '#0d0811', ink: '#faeef4', teal: '#1dd9a0',
    pink: 'rgb(255,64,160)', ghostTeal: 'rgb(29,217,160)',
  };
  const MOTION = { fade: Easing.easeOutCubic, cut: Easing.easeOutQuart, drift: Easing.easeInOutSine };

  const PARTS = [
    { id: 'V', d: 'M109.364 322.697 L89.348 272.297 L95.9 272.297 L106.196 298.721 C110.3 309.233 112.82 315.929 112.892 316.145 L113.036 316.145 C113.108 315.929 115.556 309.305 119.588 299.081 L130.028 272.297 L136.364 272.297 L116.348 322.697 Z' },
    { id: 'E1', d: 'M164.66 322.697 L164.66 272.297 L198.572 272.297 L198.572 277.769 L170.492 277.769 L170.492 293.609 L192.38 293.609 L192.38 299.081 L170.492 299.081 L170.492 317.225 L198.716 317.225 L198.716 322.697 Z' },
    { id: 'I', d: 'M229.46 322.697 L229.46 272.297 L235.292 272.297 L235.292 322.697 Z' },
    { id: 'Lstem', d: 'M267.98 322.697 L267.98 272.297 L273.812 272.297 L273.812 317.225 L276.246 317.225 L273.236 322.697 Z' },
    { id: 'Lfoot', d: 'M273.236 322.697 L276.246 317.225 L283.063 317.225 L300.38 317.225 L300.38 322.697 L279.905 322.697 Z' },
    { id: 'Bstem', d: 'M240.188 382.785 L240.188 346.297 L258.332 346.297 C258.976 346.297 259.604 346.323 260.214 346.374 L260.214 346.374 L257.483 351.34 C257.383 351.338 257.282 351.337 257.18 351.337 L246.02 351.337 L246.02 367.897 L248.376 367.897 Z' },
    { id: 'Bbowl', d: 'M262.774 352.381 L265.536 347.594 C270.479 349.588 273.38 353.705 273.38 358.681 C273.38 365.305 269.132 368.689 265.46 369.553 L265.46 369.769 C270.212 370.561 276.908 374.089 276.908 382.657 C276.908 391.153 270.788 396.697 259.7 396.697 L240.188 396.697 L240.188 390.28 L246.02 380.81 L246.02 391.657 L259.412 391.657 C266.18 391.657 270.788 388.561 270.788 382.369 C270.788 376.033 265.46 372.505 259.268 372.505 L251.134 372.505 L251.522 371.876 L253.818 367.897 L257.756 367.897 C263.372 367.897 267.476 365.017 267.476 359.545 C267.476 356.249 265.796 353.714 262.774 352.381 Z' },
    { id: 'R', d: 'M306.644 396.697 L306.644 346.297 L322.052 346.297 C333.86 346.297 340.844 352.129 340.844 361.993 C340.844 369.769 336.452 374.305 329.324 375.817 L341.78 396.697 L334.724 396.697 L323.276 376.897 C322.196 376.969 319.748 376.969 318.668 376.969 L312.476 376.969 L312.476 396.697 Z M312.476 351.697 L312.476 372.073 L322.628 372.073 C329.972 372.073 334.868 369.481 334.868 362.209 C334.868 355.081 330.116 351.697 322.268 351.697 Z' },
    { id: 'E2', d: 'M371.588 396.697 L371.588 346.297 L405.5 346.297 L405.5 351.769 L377.42 351.769 L377.42 367.609 L399.308 367.609 L399.308 373.081 L377.42 373.081 L377.42 391.225 L405.644 391.225 L405.644 396.697 Z' },
    { id: 'A', d: 'M432.5 396.697 L453.956 346.297 L460.364 346.297 L481.82 396.697 L475.556 396.697 L469.94 383.305 L444.38 383.305 L438.764 396.697 Z M457.052 352.993 C456.98 353.065 454.028 360.193 452.156 364.729 L446.612 377.977 L467.708 377.977 L462.164 364.801 C460.22 360.193 457.268 353.065 457.196 352.993 Z' },
    { id: 'K', d: 'M510.692 396.697 L510.692 346.297 L516.524 346.297 L516.524 361.993 L516.524 372.865 L516.668 372.865 L524.876 364.153 L541.652 346.297 L548.708 346.297 L529.772 366.457 L550.652 396.769 L543.74 396.697 L525.452 370.633 L516.524 379.921 L516.524 396.697 Z' },
    { id: 's1', blade: true, d: 'M418.646 88.291 L451.652 45 L401.071 132.64 L394.254 132.64 Z' },
    { id: 's2', blade: true, d: 'M378.222 161.789 L391.244 138.112 L397.913 138.112 L383.544 163.008 C381.969 162.373 380.185 161.952 378.222 161.789 Z' },
    { id: 's3', blade: true, d: 'M319.646 268.291 L358.196 198.2 L358.196 205.695 Z' },
    { id: 's4', blade: true, d: 'M375.491 166.754 L378.222 161.789 C380.185 161.952 381.969 162.373 383.544 163.008 L380.782 167.795 L371.826 183.312 L369.53 187.291 L369.142 187.92 L364.028 196.225 L358.196 205.695 L358.196 198.2 L366.384 183.312 Z' },
  ];

  // Offsets from logo-start.svg, read off the uploaded keyframes.
  const K25 = { text: [0, -8.23], blade: [-77.997, 108] };
  const K50 = { text: [1.652, -4.585], blade: [-116.356, 180] };
  const KA4 = {
    V: [-0.207, -2.297], E1: [-0.207, -2.297], I: [-0.207, -2.297], Lstem: [-0.207, -2.297],
    Lfoot: [-1.934, 0.063], Bstem: [-8.793, 18.503],
    Bbowl: [0.207, 13.519], R: [0.207, 13.519], E2: [0.207, 13.519], A: [0.207, 13.519], K: [0.207, 13.519],
    s1: [-144.207, 234.288], s2: [-137.518, 222.176], s3: [-147.201, 222.997], s4: [-147.201, 222.997],
  };
  // Blade travel (logo units). It enters from off-frame along its own axis, so the
  // keyframes become knots on one continuous, decelerating stroke.
  const PRE = 300, UP = [0.5855, -0.8107];
  const KNOTS = [0, PRE, PRE + 133.2, PRE + 214.78, PRE + 268.3];
  const D = KNOTS[4];

  function monotone(xs, ys) {
    const n = xs.length, dx = [], s = [], m = new Array(n);
    for (let i = 0; i < n - 1; i++) { dx[i] = xs[i + 1] - xs[i]; s[i] = (ys[i + 1] - ys[i]) / dx[i]; }
    m[0] = s[0]; m[n - 1] = s[n - 2];
    for (let i = 1; i < n - 1; i++) m[i] = s[i - 1] * s[i] <= 0 ? 0 : (s[i - 1] + s[i]) / 2;
    for (let i = 0; i < n - 1; i++) {
      if (s[i] === 0) { m[i] = 0; m[i + 1] = 0; continue; }
      const a = m[i] / s[i], b = m[i + 1] / s[i], h = a * a + b * b;
      if (h > 9) { const t = 3 / Math.sqrt(h); m[i] = t * a * s[i]; m[i + 1] = t * b * s[i]; }
    }
    return (x) => {
      if (x <= xs[0]) return ys[0];
      if (x >= xs[n - 1]) return ys[n - 1];
      let i = 0; while (x > xs[i + 1]) i++;
      const h = dx[i], t = (x - xs[i]) / h, t2 = t * t, t3 = t2 * t;
      return (2 * t3 - 3 * t2 + 1) * ys[i] + (t3 - 2 * t2 + t) * h * m[i] + (-2 * t3 + 3 * t2) * ys[i + 1] + (t3 - t2) * h * m[i + 1];
    };
  }

  const TRACKS = PARTS.map((p) => {
    const pre = p.blade ? [PRE * UP[0], PRE * UP[1]] : [0, 0];
    const k25 = p.blade ? K25.blade : K25.text, k50 = p.blade ? K50.blade : K50.text, a4 = KA4[p.id];
    const keys = [pre, [0, 0], k25, k50, a4];
    return [monotone(KNOTS, keys.map((k) => k[0])), monotone(KNOTS, keys.map((k) => k[1]))];
  });
  const posAt = (d) => TRACKS.map(([fx, fy]) => [fx(d), fy(d)]);

  const rnd = (a, b = 0, c = 0) => { const x = Math.sin(a * 127.1 + b * 311.7 + c * 74.7) * 43758.5453; return x - Math.floor(x); };
  const spread = (a, b, c) => rnd(a, b, c) * 2 - 1;

  function LogoLayer({ d, ink, blade, textOpacity = 1, bladeOnly = false }) {
    const pos = posAt(d);
    return (
      <g>
        {PARTS.map((p, i) => (bladeOnly && !p.blade) ? null : (
          <path key={p.id} d={p.d} fillRule="evenodd" fill={p.blade ? blade : ink}
                opacity={p.blade ? 1 : textOpacity}
                transform={`translate(${pos[i][0].toFixed(3)} ${pos[i][1].toFixed(3)})`} />
        ))}
      </g>
    );
  }

  function SplashPiece({ tw }) {
    const { T, CUES, time, authoredTotal } = useComposition();
    const cutLen = CUES.Hold - CUES.Cut, exitLen = authoredTotal - CUES.Exit;
    const dAt = (t) => D * MOTION.cut(clamp((t - CUES.Cut) / cutLen, 0, 1));
    const d = dAt(T);
    const bladeFill = tw.slashColor === 'lavender' ? 'url(#vbs-lav)' : tw.slashColor === 'ink' ? VB.ink : VB.teal;

    // Veil: letters fade in, gaps where the blade will pass already visible.
    const letters = MOTION.fade(clamp((T - CUES.Veil - 0.1) / 0.7, 0, 1));
    // Camera: one slow push across the whole piece.
    const S = 1.8 + 0.15 * MOTION.drift(clamp(T / authoredTotal, 0, 1));
    // Impact: the instant the blade sits fully through L and B (the 50% pose).
    const tHit = CUES.Cut + (1 - Math.pow(1 - KNOTS[3] / D, 0.25)) * cutLen;
    const since = T - tHit;
    const shake = since >= 0 ? Math.exp(-since / 0.09) : 0;
    const shx = 7 * shake * Math.sin(since * 95), shy = 4 * shake * Math.sin(since * 71 + 1);
    const burst = since >= 0 ? Math.exp(-since / 0.14) : 0;
    const cam = `translate(${(960 + shx).toFixed(2)} ${(540 + shy).toFixed(2)}) scale(${S.toFixed(4)}) translate(-320 -343)`;

    // Exit: IntroSplash's glitch (bands tear, channels split, snaps clean, then fades).
    const g = T - CUES.Exit;
    let overlay = 1, mainOn = 1, slices = null, ghost = null;
    if (g >= 0) {
      const p = clamp(g / exitLen, 0, 1);
      if (tw.exitStyle === 'fade') overlay = 1 - MOTION.fade(p);
      else {
        overlay = p < 0.65 ? 1 : 1 - (p - 0.65) / 0.35;
        const j = Math.floor(g / 0.055);
        const inten = 0.35 + 0.65 * clamp((j * 0.055) / exitLen, 0, 1);
        if (rnd(j, 1) >= 0.18) {
          mainOn = 0;
          slices = Array.from({ length: 8 }, (_, i) => ({
            tx: spread(j, i, 2) * (rnd(j, i, 3) < 0.45 ? 100 : 14) * inten,
            op: rnd(j, i, 4) < 0.22 * inten ? 0 : 1,
          }));
          const split = (4 + 16 * inten) * 1.8;
          ghost = { op: 0.85, pink: [-split + spread(j, 7) * 11, spread(j, 8) * 5.4], teal: [split + spread(j, 9) * 11, spread(j, 10) * 5.4] };
        }
      }
    } else if (tw.impactSplit && burst > 0.02) {
      const split = 3 + 22 * burst;
      ghost = { op: 0.85 * burst, pink: [-split, 0], teal: [split, 0] };
    }

    const bandTop = 540 + (258 - 343) * S, bandH = ((502 - 258) * S) / 8;
    const trail = [1, 2].map((k) => ({ k, d: dAt(T - 0.022 * k) })).filter((e) => d - e.d > 2);

    return (
      <div data-screen-label={`${Math.floor(time)}s`} style={{ position: 'absolute', inset: 0, background: VB.ground }}>
        <svg width="1920" height="1080" viewBox="0 0 1920 1080" style={{ position: 'absolute', inset: 0, display: 'block' }}>
          <defs>
            <linearGradient id="vbs-lav" gradientUnits="userSpaceOnUse" x1="319.646" y1="268.291" x2="451.652" y2="45">
              <stop offset="0" stopColor="#c084fc" />
              <stop offset="1" stopColor="#d9b8ff" />
            </linearGradient>
            {Array.from({ length: 8 }, (_, i) => (
              <clipPath key={i} id={`vbs-band-${i}`}>
                <rect x="0" y={bandTop + i * bandH} width="1920" height={bandH + 0.5} />
              </clipPath>
            ))}
          </defs>
          <g opacity={overlay}>
            {ghost && [['pink', VB.pink], ['teal', VB.ghostTeal]].map(([key, color]) => (
              <g key={key} opacity={ghost.op} transform={`translate(${ghost[key][0].toFixed(2)} ${ghost[key][1].toFixed(2)})`}>
                <g transform={cam}><LogoLayer d={d} ink={color} blade={color} textOpacity={letters} /></g>
              </g>
            ))}
            {trail.map((e) => (
              <g key={e.k} opacity={e.k === 1 ? 0.32 : 0.14} transform={cam}>
                <LogoLayer d={e.d} ink={VB.ink} blade={bladeFill} bladeOnly />
              </g>
            ))}
            <g opacity={mainOn} transform={cam}>
              <LogoLayer d={d} ink={VB.ink} blade={bladeFill} textOpacity={letters} />
            </g>
            {slices && slices.map((s, i) => (
              <g key={i} clipPath={`url(#vbs-band-${i})`} opacity={s.op}>
                <g transform={`translate(${s.tx.toFixed(2)} 0)`}>
                  <g transform={cam}><LogoLayer d={d} ink={VB.ink} blade={bladeFill} /></g>
                </g>
              </g>
            ))}
          </g>
        </svg>
      </div>
    );
  }

  const FALLBACK = { motionEditor: true, slashColor: 'teal', impactSplit: true, exitStyle: 'glitch' };

  function VeilbreakSplash() {
    const [t, setTweak] = useTweaks(Object.assign({}, FALLBACK, window.TWEAK_DEFAULTS || {}));
    return (
      <React.Fragment>
        <CompositionStage width={1920} height={1080} bg={VB.ground}
                          scenes={window.OM_SCENES} playback={window.OM_PLAYBACK}>
          <SplashPiece tw={t} />
        </CompositionStage>
        <TweaksPanel>
          <TweakSection label="Editor" />
          <TweakToggle label="Motion editor" value={t.motionEditor} onChange={(v) => setTweak('motionEditor', v)} />
          <TweakSection label="Mark" />
          <TweakRadio label="Blade" value={t.slashColor} options={['teal', 'lavender', 'ink']}
                      onChange={(v) => setTweak('slashColor', v)} />
          <TweakToggle label="RGB split on impact" value={t.impactSplit} onChange={(v) => setTweak('impactSplit', v)} />
          <TweakRadio label="Exit" value={t.exitStyle} options={['glitch', 'fade']}
                      onChange={(v) => setTweak('exitStyle', v)} />
        </TweaksPanel>
      </React.Fragment>
    );
  }

  window.VeilbreakSplash = VeilbreakSplash;
})();
