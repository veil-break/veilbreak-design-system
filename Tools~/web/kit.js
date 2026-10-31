/* The kit: a web port of the Unity package com.veilbreak.gui (UI Toolkit, portrait 390 x 844).
   Same element trees, BEM classes (the USS vb- names; the legacy kit above is renamed to vbl-)
   and Bind() logic as the C# sources; Gui.uss, Layout.uss and World.uss are ported mechanically into bundle.css. */
(function () {
  'use strict';
  var R = window.React;
  var h = R.createElement;
/*@@ICONS@@*/

  function cx() { return Array.prototype.filter.call(arguments, Boolean).join(' '); }
  function clamp01(n) { n = Number(n); return isNaN(n) ? 0 : Math.max(0, Math.min(1, n)); }
  function pct(n) { return (clamp01(n) * 100) + '%'; }
  function kebab(n) {
    n = String(n == null ? '' : n); var b = '';
    for (var i = 0; i < n.length; i++) {
      var c = n[i];
      if (c >= 'A' && c <= 'Z' && i > 0 && n[i - 1] !== '-') b += '-';
      b += (c === ' ' || c === '_') ? '-' : c.toLowerCase();
    }
    return b;
  }
  function upper(s) { return String(s == null ? '' : s).toUpperCase(); }
  function num(v) { return Number(v || 0).toLocaleString('en-US'); }
  function trim1(v) { return String(Math.round(v * 10) / 10); }
  function compact(v) { return v >= 1e9 ? trim1(v / 1e9) + 'B' : v >= 1e6 ? trim1(v / 1e6) + 'M' : v >= 1e4 ? trim1(v / 1e3) + 'K' : num(v); }
  function seconds(s) {
    s = Number(s || 0);
    if (s < 60) return (s < 10 ? s.toFixed(1) : trim1(s)) + 's';
    var m = Math.floor(s / 60), r = Math.floor(s - m * 60);
    return m + ':' + (r < 10 ? '0' : '') + r;
  }
  function pad2(n) { n = Math.floor(Number(n || 0)); return (n < 10 && n >= 0 ? '0' : '') + n; }
  // Ui.State: block--{kebab(state)} for the one active value.
  function st(block, value) { return block + '--' + kebab(value); }
  function L(cls, text, style) { return h('div', { className: cx('vb-l', cls), style: style }, text); }
  function show(on) { return on ? null : { display: 'none' }; }
  function clickable(fn) {
    return {
      tabIndex: 0, role: 'button', onClick: fn,
      onKeyDown: function (e) { if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); if (fn) fn(e); } }
    };
  }

  /* ── VbShape: the cut-shape painter (VbShape.cs Paint) drawn as SVG from --vb-* properties ── */
  var DIAG = 0.4142, shapes = typeof Set === 'function' ? new Set() : null, queued = false, installed = false;
  function parseColor(s) {
    s = (s || '').trim(); var m;
    if (!s || s === 'transparent') return [0, 0, 0, 0];
    if (s[0] === '#') {
      var x = s.slice(1); if (x.length <= 4) x = x.split('').map(function (c) { return c + c; }).join('');
      return [parseInt(x.slice(0, 2), 16), parseInt(x.slice(2, 4), 16), parseInt(x.slice(4, 6), 16), x.length > 6 ? parseInt(x.slice(6, 8), 16) / 255 : 1];
    }
    if ((m = /rgba?\(([^)]*)\)/.exec(s))) {
      var p = m[1].split(/[\s,\/]+/).filter(Boolean).map(parseFloat);
      return [p[0], p[1], p[2], p.length > 3 ? p[3] : 1];
    }
    return [0, 0, 0, 0];
  }
  function rgba(c, a) { return 'rgba(' + c[0] + ',' + c[1] + ',' + c[2] + ',' + (a == null ? c[3] : a) + ')'; }
  function outline(w, hgt, cut, b) {
    if (cut <= 0) return [[b, b], [w - b, b], [w - b, hgt - b], [b, hgt - b]];
    var d = cut + b * DIAG;
    return [[d, b], [w - b, b], [w - b, hgt - d], [w - d, hgt - b], [b, hgt - b], [b, d]];
  }
  function poly(pts) { return 'M' + pts.map(function (p) { return p[0].toFixed(2) + ' ' + p[1].toFixed(2); }).join('L') + 'Z'; }
  function rect(x0, y0, x1, y1, fill) { return '<rect x="' + x0 + '" y="' + y0 + '" width="' + Math.max(0, x1 - x0) + '" height="' + Math.max(0, y1 - y0) + '" fill="' + fill + '"/>'; }
  function gradient(id, w, hgt, deg, from, to) {
    var r = deg * Math.PI / 180, dx = Math.cos(r), dy = Math.sin(r);
    var pr = [0, w * dx, hgt * dy, w * dx + hgt * dy], min = Math.min.apply(null, pr), max = Math.max.apply(null, pr);
    if (max - min < 0.001) max = min + 0.001;
    return '<linearGradient id="' + id + '" gradientUnits="userSpaceOnUse" x1="' + dx * min + '" y1="' + dy * min + '" x2="' + dx * max + '" y2="' + dy * max + '">' +
      '<stop offset="0" stop-color="' + rgba(from) + '"/><stop offset="1" stop-color="' + rgba(to) + '"/></linearGradient>';
  }
  var gid = 0;
  function paint(el) {
    var svg = el.firstElementChild;
    if (!svg || svg.getAttribute('data-vb-paint') == null) return;
    var w = el.offsetWidth, hgt = el.offsetHeight;
    if (w <= 0 || hgt <= 0) { svg.innerHTML = ''; return; }
    var cs = getComputedStyle(el);
    function n(k, d) { var v = parseFloat(cs.getPropertyValue('--vb-' + k)); return isNaN(v) ? d : v; }
    function c(k) { return parseColor(cs.getPropertyValue('--vb-' + k)); }
    var cut = Math.min(n('cut', 10), w / 3, hgt / 3), bw = n('bw', 1), dir = n('fill-dir', 0), dash = n('dash', 0) > 0.5, bl = n('bracket-len', 8);
    var fill = c('fill'), fillTo = c('fill-to'), edge = c('edge'), edgeTo = c('edge-to'), bracket = c('bracket'), glow = c('glow');
    var id = svg.__vbId || (svg.__vbId = 'vb-g' + (++gid)), defs = '', out = '';
    function paintOf(from, to, key) {
      if (to[3] <= 0 || (to[0] === from[0] && to[1] === from[1] && to[2] === from[2] && to[3] === from[3])) return rgba(from);
      defs += gradient(id + key, w, hgt, dir, from, to); return 'url(#' + id + key + ')';
    }
    if (glow[3] > 0) for (var i = 3; i >= 1; i--) {
      out += '<path fill-rule="evenodd" fill="' + rgba(glow, glow[3] * (0.55 - i * 0.15)) + '" d="' + poly(outline(w, hgt, cut, -i * 2.5)) + poly(outline(w, hgt, cut, -(i - 1) * 2.5)) + '"/>';
    }
    if (dash) {
      if (fill[3] > 0) out += rect(0, 0, w, hgt, rgba(fill));
      if (edge[3] > 0) {
        var t = Math.max(1, bw), e, x, y, col = rgba(edge);
        for (x = 0; x < w; x += 7) { e = Math.min(w, x + 4); out += rect(x, 0, e, t, col) + rect(x, hgt - t, e, hgt, col); }
        for (y = 0; y < hgt; y += 7) { e = Math.min(hgt, y + 4); out += rect(0, y, t, e, col) + rect(w - t, y, w, e, col); }
      }
    } else {
      if (fill[3] > 0 || fillTo[3] > 0) out += '<path fill="' + paintOf(fill[3] > 0 ? fill : fillTo, fillTo, 'f') + '" d="' + poly(outline(w, hgt, cut, 0)) + '"/>';
      if (bw > 0 && (edge[3] > 0 || edgeTo[3] > 0)) out += '<path fill-rule="evenodd" fill="' + paintOf(edge[3] > 0 ? edge : edgeTo, edgeTo, 'e') + '" d="' + poly(outline(w, hgt, cut, 0)) + poly(outline(w, hgt, cut, bw)) + '"/>';
    }
    if (bracket[3] > 0) {
      var bc = rgba(bracket);
      out += rect(w - bl, 0, w, 2, bc) + rect(w - 2, 0, w, bl, bc) + rect(0, hgt - 2, bl, hgt, bc) + rect(0, hgt - bl, 2, hgt, bc);
    }
    svg.setAttribute('viewBox', '-8 -8 ' + (w + 16) + ' ' + (hgt + 16));
    svg.innerHTML = (defs ? '<defs>' + defs + '</defs>' : '') + out;
  }
  function repaintAll() { queued = false; if (shapes) shapes.forEach(paint); }
  function schedule() { if (!queued) { queued = true; requestAnimationFrame(repaintAll); } }
  function install() {
    if (installed) return; installed = true;
    ['pointerover', 'pointerout', 'pointerdown', 'pointerup', 'focusin', 'focusout', 'transitionend'].forEach(function (t) { document.addEventListener(t, schedule, true); });
    window.addEventListener('resize', schedule);
    if (window.MutationObserver) new MutationObserver(schedule).observe(document.documentElement, { attributes: true, attributeFilter: ['class', 'style'], subtree: true });
    if (document.fonts && document.fonts.ready) document.fonts.ready.then(schedule);
  }
  var ro = window.ResizeObserver ? new ResizeObserver(schedule) : null;
  function useShape(ref) {
    R.useLayoutEffect(function () {
      var el = ref.current; install(); shapes.add(el); if (ro) ro.observe(el); paint(el);
      return function () { shapes.delete(el); if (ro) ro.unobserve(el); };
    }, []);
    R.useLayoutEffect(function () { schedule(); });
  }
  /** Any element painted as a cut shape (VbShape.Attach): first child is the paint layer. */
  var Shape = R.forwardRef(function (p, fwd) {
    var ref = R.useRef(null);
    useShape(ref);
    var props = Object.assign({}, p, {
      ref: function (el) { ref.current = el; if (typeof fwd === 'function') fwd(el); else if (fwd) fwd.current = el; },
      className: cx('vb-shape', p.className)
    });
    delete props.as;
    return h(p.as || 'div', props, h('svg', { className: 'vb-paint', 'data-vb-paint': '', 'aria-hidden': true }), p.children);
  });

  var ROOT_FLAGS = ['vb-compact', 'vb-large-text', 'vb-reduced-motion', 'vb-reduced-flashes', 'vb-contrast'];  // SafeAreaRoot.RootClasses
  /* ── Popover (Popover.cs): hover on a mouse, tap toggles on touch, focus opens; kept inside the viewport ── */
  function usePopover(build, placement, alignEnd, pinned) {
    var anchor = R.useRef(null), pop = R.useRef(null);
    var s = R.useState(false), open = s[0] || !!pinned, setOpen = s[1];
    var pos = R.useState(null), at = pos[0], setAt = pos[1], tick = R.useState(0)[1];
    // Popover.Place runs on every geometry change; re-place on resize, scroll and anchor resize.
    R.useEffect(function () {
      if (!open) return;
      function again() { tick(function (n) { return n + 1; }); }
      window.addEventListener('resize', again); window.addEventListener('scroll', again, true);
      var obs = window.ResizeObserver && anchor.current ? new ResizeObserver(again) : null; if (obs) obs.observe(anchor.current);
      return function () { window.removeEventListener('resize', again); window.removeEventListener('scroll', again, true); if (obs) obs.disconnect(); };
    }, [open]);
    R.useLayoutEffect(function () {
      if (!open || !anchor.current || !pop.current) return;
      var a = anchor.current.getBoundingClientRect(), wv = window.innerWidth, hv = window.innerHeight;
      var w = pop.current.offsetWidth, hh = pop.current.offsetHeight, gap = 6, edge = 8;
      var x = alignEnd ? a.right - w : a.left, y = placement === 'above' ? a.top - gap - hh : a.bottom + gap;
      if (placement !== 'above' && y + hh > hv - edge) y = a.top - gap - hh;
      if (placement === 'above' && y < edge) y = a.bottom + gap;
      x = Math.min(Math.max(x, edge), Math.max(edge, wv - w - edge)); y = Math.min(Math.max(y, edge), Math.max(edge, hv - hh - edge));
      if (!at || Math.abs(at[0] - x) > 0.5 || Math.abs(at[1] - y) > 0.5) setAt([x, y]);
    });
    var handlers = {
      onPointerEnter: function (e) { if (e.pointerType === 'mouse') setOpen(true); },
      onPointerLeave: function (e) { if (e.pointerType === 'mouse') setOpen(false); },
      onPointerDown: function (e) { if (e.pointerType !== 'mouse') setOpen(function (o) { return !o; }); },
      onFocus: function () { setOpen(true); }, onBlur: function () { setOpen(false); }
    };
    var content = open ? build() : null, a = content && anchor.current;
    // Popover.Open: the layer sits outside the board, so it copies the anchor's root flags and theme.
    var flags = a ? ROOT_FLAGS.filter(function (c) { return a.closest('.' + c); }) : [], themed = a && a.closest('[data-theme]');
    var layer = content ? window.ReactDOM.createPortal(h('div', {
      ref: pop, className: cx('vb-root vb-popover', flags.join(' ')), 'data-theme': themed ? themed.getAttribute('data-theme') : undefined,
      style: { position: 'fixed', left: at ? at[0] : -9999, top: at ? at[1] : -9999, maxWidth: Math.max(80, window.innerWidth - 16), maxHeight: Math.max(80, window.innerHeight - 32), overflowY: 'auto', zIndex: 50, pointerEvents: 'none' }
    }, content), document.body) : null;
    return { ref: anchor, open: !!content, handlers: handlers, layer: layer };
  }

  /* ── Controls ─────────────────────────────────────────────────────────── */
  function VbIcon(p) {
    var name = kebab(p.icon || ''), size = p.size == null ? 16 : p.size;
    var style = Object.assign({ width: size, height: size }, p.style || {}, name ? null : { display: 'none' });
    return h('div', { className: cx('vb-icon', name && 'vb-icon--' + name, p.className), style: style, title: p.title },
      ICONS[name] ? h('svg', { viewBox: '0 0 24 24', fill: 'none', stroke: 'currentColor', strokeWidth: 1.6, strokeLinecap: 'square', strokeLinejoin: 'miter', width: '100%', height: '100%', 'aria-hidden': true, dangerouslySetInnerHTML: { __html: ICONS[name] } }) : null);
  }
  VbIcon.names = Object.keys(ICONS);

  function VbMark(p) {
    var w = p.width == null ? 260 : p.width;
    return h('div', { className: cx('vb-mark', p.className), role: 'img', 'aria-label': 'Veilbreak', style: Object.assign({ width: w, height: Math.round(w * 259 / 513) }, p.style), dangerouslySetInnerHTML: { __html: MARK.replace('<svg ', '<svg width="100%" height="100%" ') } });
  }

  function VbFrame(p) { return h(Shape, { className: p.className, style: p.style }, p.children); }

  function VbButton(p) {
    var variant = p.variant || 'secondary', blocked = p.blocked || '', sub = blocked || p.sub || '', icon = p.icon || '';
    return h(Shape, {
      as: 'button', type: 'button', disabled: !!blocked, onClick: blocked ? null : p.onClick, style: p.style,
      className: cx('vb-button', st('vb-button', variant), blocked && 'vb-button--blocked', p.className)
    },
      h('div', { className: 'vb-button__text' }, L('vb-button__label', p.title || ''), L('vb-button__sub', sub, show(sub.length > 0))),
      h(VbIcon, { icon: icon, size: icon === 'chevron' || icon === 'play' ? 12 : 20, className: 'vb-button__icon' }));
  }

  // StatReadout.Delta: a leading + reads up (teal), − down (pink); `up` overrides when lower is better.
  function StatReadout(p) {
    var bar = p.bar == null ? -1 : p.bar, d = p.delta || '', c = d.charAt(0);
    var rise = p.up != null ? !!p.up : c === '+', fall = p.up != null ? !p.up : c === '−' || c === '-';
    return h('div', { className: cx('vb-stat', p.tone && 'vb-stat--' + p.tone, p.className) },
      L('vb-stat__label', p.label),
      h('div', { className: 'vb-stat__row' }, L('vb-stat__value', p.value), L('vb-stat__unit', p.unit || '', show(!!p.unit)),
        L(cx('vb-stat__delta', d && rise && 'vb-stat__delta--up', d && fall && !rise && 'vb-stat__delta--down'), d, show(d.length > 0))),
      h('div', { className: 'vb-stat__bar', style: show(bar >= 0) }, h('div', { className: 'vb-stat__fill', style: { width: pct(bar) } })));
  }

  var STATUS_ICON = { exposed: 'exposed', silenced: 'silence', rooted: 'root', slowed: 'slow', interrupted: 'warning', barrier: 'barrier' };
  function statusIcon(kind) { return STATUS_ICON[kebab(kind)] || 'barrier'; }
  function StatusChip(p) {
    var kind = kebab(p.kind || 'barrier');
    return h(Shape, { className: cx('vb-status', st('vb-status', kind), p.className) },
      h(VbIcon, { icon: statusIcon(kind), size: 14, className: 'vb-status__icon' }),
      L('vb-status__word', p.text || kind.charAt(0).toUpperCase() + kind.slice(1)),
      p.detail ? L('vb-status__detail', '· ' + p.detail) : null,
      p.time ? L('vb-status__time', p.time) : null);
  }

  function useValue(value, onChange) {
    var s = R.useState(value), v = s[0], set = s[1];
    R.useEffect(function () { set(value); }, [value]);
    return [v, function (n) { if (n === v) return; set(n); if (onChange) onChange(n); }];
  }

  function VbSwitch(p) {
    var s = useValue(!!p.value, p.onChange), on = s[0];
    return h('div', Object.assign({ className: cx('vb-switch', on && 'vb-switch--on', p.className), 'aria-checked': on, role: 'switch' }, clickable(function () { s[1](!on); }), { role: 'switch' }),
      h(Shape, { className: 'vb-switch__track' }, h('div', { className: 'vb-switch__knob' })),
      h('div', { className: 'vb-switch__text' }, L('vb-switch__label', p.text || ''), L('vb-switch__detail', p.detail || '', show(!!p.detail))));
  }

  function VbSlider(p) {
    var min = p.min == null ? 0 : p.min, max = p.max == null ? 1 : p.max;
    var s = useValue(p.value == null ? 0 : p.value, p.onChange), v = s[0], track = R.useRef(null);
    var t = max === min ? 0 : clamp01((v - min) / (max - min));
    function at(e) { var r = track.current.getBoundingClientRect(); s[1](min + clamp01((e.clientX - r.left - 6) / Math.max(1, r.width - 12)) * (max - min)); }
    return h('div', { className: cx('vb-slider', 'vb-u-base-field', p.className), tabIndex: 0, role: 'slider', 'aria-valuenow': Math.round(t * 100),
      onKeyDown: function (e) { var k = (max - min) / 20; if (e.key === 'ArrowRight') s[1](Math.min(max, v + k)); if (e.key === 'ArrowLeft') s[1](Math.max(min, v - k)); } },
      L('vb-u-base-field__label', p.label || ''),
      h('div', { className: 'vb-u-base-slider__input' },
        h('div', { className: 'vb-u-base-slider__drag-container', ref: track,
          onPointerDown: function (e) { e.currentTarget.setPointerCapture(e.pointerId); at(e); },
          onPointerMove: function (e) { if (e.buttons) at(e); } },
          h('div', { className: 'vb-u-base-slider__tracker' }, h('div', { className: 'vb-slider__fill', style: { width: pct(t) } })),
          h('div', { className: 'vb-u-base-slider__dragger', style: { left: 'calc((100% - 12px) * ' + t + ')' } }))),
      L('vb-slider__value', String(Math.round(t * 100))));
  }

  function VbSegmented(p) {
    var items = p.items || [], s = useValue(Math.max(0, Math.min(items.length - 1, p.selected || 0)), p.onChange);
    return h(Shape, { className: cx('vb-segmented', p.className), role: 'radiogroup' }, items.map(function (it, k) {
      return h('div', Object.assign({ key: k, className: cx('vb-segmented__option', k === s[0] && 'vb-segmented__option--on'), 'aria-checked': k === s[0] }, clickable(function () { s[1](k); }), { role: 'radio' }),
        L('vb-segmented__label', it.label), it.sub ? L('vb-segmented__sub', it.sub) : null);
    }));
  }

  /* ── Meters ───────────────────────────────────────────────────────────── */
  function VbMeter(p) {
    var v = clamp01(p.value), extra = Math.max(0, Math.min(Number(p.extra || 0), 1 - v));
    var from = p.previewFrom == null ? -1 : p.previewFrom, to = p.previewTo == null ? -1 : p.previewTo, on = from >= 0 && to > from;
    return h('div', { className: cx('vb-meter', p.className), style: p.style },
      p.before || null,
      h('div', { className: 'vb-meter__fill', style: { width: pct(v) } }),
      h('div', { className: 'vb-meter__extra', style: extra > 0 ? { left: pct(v), width: pct(extra) } : { display: 'none' } }),
      h('div', { className: 'vb-meter__preview', style: on ? { left: pct(from), width: pct(to - from) } : { display: 'none' } }),
      p.after || null,
      (p.marks || []).map(function (m, i) { return h('div', { key: i, className: 'vb-meter__mark', style: { left: pct(m) } }, L('vb-meter__mark-label', String(i + 1))); }));
  }

  // `ghost` is health before the last hit (same units as hp); `size` hud | world | large; `caption` "HP 820 / 1000".
  function HealthBar(p) {
    var max = Math.max(1, p.max || 1), stab = p.stability == null ? -1 : p.stability, hp = p.hp || 0, ghost = p.ghost == null ? -1 : p.ghost, cap = p.caption || '';
    return h('div', { className: cx('vb-health', p.exposed && 'vb-health--exposed', p.enemy && 'vb-health--enemy', p.size && st('vb-health', p.size), p.className), style: p.style },
      L('vb-health__label', cap, show(cap.length > 0)),
      h(VbMeter, { className: 'vb-health__hp', value: hp / max, extra: (p.barrier || 0) / max,
        before: h('div', { className: 'vb-health__ghost', style: ghost > hp ? { width: pct(ghost / max) } : { display: 'none' } }) }),
      h(VbMeter, { className: 'vb-health__stability', value: p.exposed ? 1 : stab, style: show(stab >= 0) }));
  }

  // `reserve`: the autopilot's soft reserve line; `queue`: up to four QueueChips (manual ones raise onCancel); `legend`: a key line.
  function PowerStrip(p) {
    var max = p.max || 100, m = Math.max(1, max), from = p.previewFrom == null ? -1 : p.previewFrom, res = p.reserve || 0, q = (p.queue || []).slice(0, 4), legend = p.legend || '';
    return h('div', { className: cx('vb-power', p.className), style: p.style },
      h('div', { className: 'vb-power__head' }, L('vb-power__label', 'POWER'), L('vb-power__value', String(p.value || 0)), L('vb-power__max', '/' + max),
        L('vb-power__regen', p.regen || ''), L('vb-power__hint', p.hint || '', show(!!p.hint))),
      h(VbMeter, { className: 'vb-power__meter', value: (p.value || 0) / m, previewFrom: from < 0 ? -1 : from / m, previewTo: (p.previewTo == null ? -1 : p.previewTo) / m, marks: p.marks,
        after: h('div', { className: 'vb-power__reserve', title: 'Autoplay keeps ' + res + ' Power in reserve', style: res > 0 ? { left: pct(res / m) } : { display: 'none' } }) }),
      h('div', { className: 'vb-power__queue', style: show(q.length > 0) }, q.map(function (e, i) { return h(QueueChip, { key: i, view: e, position: i + 1, onCancel: p.onCancel }); })),
      L('vb-power__legend', legend, show(legend.length > 0)));
  }

  /* QueueChip.cs: place, skill, operative, cost and time to fire; a full-height × on manual entries. */
  function QueueChip(p) {
    var v = Object.assign({ who: '', skill: '', eta: '', cost: 0, manual: false, next: false }, p.view), pos = p.position || 1, preview = !!p.preview, cancel = v.manual && !preview;
    return h(Shape, { className: cx('vb-queue-chip', (pos === 1 || v.next) && 'vb-queue-chip--next', preview && 'vb-queue-chip--preview', v.manual && 'vb-queue-chip--manual', p.className), style: p.style },
      L('vb-queue-chip__index', v.next ? '›' : String(pos)),
      h('div', { className: 'vb-queue-chip__text' }, L('vb-queue-chip__skill', upper(v.skill)), L('vb-queue-chip__who', v.who)),
      L('vb-queue-chip__cost', String(v.cost)), L('vb-queue-chip__eta', v.eta, show(!!v.eta)),
      h('div', Object.assign({ className: 'vb-queue-chip__cancel', title: 'Cancel queued cast', 'aria-label': 'Cancel queued cast', style: show(cancel) },
        clickable(function () { if (cancel && p.onCancel) p.onCancel(v); })), h(VbIcon, { icon: 'close', size: 14 })));
  }

  function FocusMeter(p) {
    var f = p.focus == null ? 100 : p.focus, regen = p.regenPerSecond == null ? 12.5 : p.regenPerSecond, drain = p.drainPerSecond == null ? 25 : p.drainPerSecond, scale = p.slowScale == null ? 0.2 : p.slowScale;
    var rate = p.slow ? '×' + scale.toFixed(1) + ' · −' + Math.round(drain) + '/s' : f < 100 ? '+' + trim1(regen) + '/s' : 'READY';
    return h('div', { className: cx('vb-focus', p.slow && 'vb-focus--slow', p.className), style: p.style },
      L('vb-focus__label', 'FOCUS'), h(VbMeter, { className: 'vb-focus__meter', value: f / 100 }), L('vb-focus__value', String(Math.round(f))), L('vb-focus__rate', rate));
  }

  /* ── SkillButton ──────────────────────────────────────────────────────── */
  var SKILL = { label: 'Skill', icon: 'projectile', slot: 'A', kind: 'Active', cost: 0, state: 'ready', charged: 1, seconds: 0, need: 0, queue: 0, progress: 0, reason: '' };
  var SKILL_STATES = ['ready', 'aiming', 'cooldown', 'short', 'silenced', 'fallen', 'casting', 'disabled', 'queued', 'buffered', 'windup', 'invalid', 'rejected', 'exposed'];
  // Entering Rejected flashes crimson once for 400 ms (SkillButton.Bind); `flash` holds it for previews.
  function useFlash(on) {
    var f = R.useState(false), was = R.useRef(false);
    R.useEffect(function () {
      var t = null; if (on && !was.current) { f[1](true); t = setTimeout(function () { f[1](false); }, 400); }
      was.current = on; return function () { if (t) clearTimeout(t); };
    }, [on]);
    return f[0];
  }
  function SkillButton(p) {
    var v = Object.assign({}, SKILL, p.view), s = kebab(v.state), top = '', value, flash = useFlash(s === 'rejected') || !!p.flash;
    switch (s) {
      case 'casting': value = 'CAST'; break;
      case 'disabled': value = 'LOCK'; break;
      case 'cooldown': value = seconds(v.seconds); break;
      case 'aiming': value = 'AIM'; break;
      case 'short': top = 'NEED'; value = String(v.need); break;
      case 'silenced': top = 'SIL'; value = seconds(v.seconds); break;
      case 'fallen': value = 'OUT'; break;
      case 'queued': top = 'QUEUE'; value = '#' + v.queue; break;
      case 'buffered': top = 'NEXT'; value = String(v.cost); break;
      case 'windup': value = seconds(v.seconds); break;
      case 'invalid': value = v.reason ? upper(v.reason) : 'AIM'; break;
      case 'exposed': top = 'EXPOSED'; value = String(v.cost); break;
      default: value = String(v.cost);
    }
    var tip = v.label + ' · ' + v.cost + ' Power' + (s === 'cooldown' ? ' · ' + Math.round(v.charged * 100) + '% ready' : s === 'invalid' && v.reason ? ' · ' + v.reason : s === 'rejected' ? ' · refused' : '');
    return h(Shape, Object.assign({ className: cx('vb-skill', st('vb-skill', s), flash && 'vb-skill--flash', p.dragging && 'vb-dragging', p.className), title: tip, tabIndex: 0, style: p.style }, p.onPointerDown ? { onPointerDown: p.onPointerDown } : null),
      h('div', { className: 'vb-skill__charge', style: s === 'cooldown' ? { height: pct(v.charged) } : { display: 'none' } }),
      h('div', { className: 'vb-skill__sweep', style: s === 'windup' ? { width: pct(v.progress) } : { display: 'none' } }),
      L('vb-skill__slot', v.slot),
      h(VbIcon, { icon: s === 'fallen' ? 'lock' : s === 'invalid' ? 'no-target' : v.icon, size: 20, className: 'vb-skill__icon' }),
      h('div', { className: 'vb-skill__slash', style: show(s === 'silenced' || s === 'invalid') }),
      h('div', { className: 'vb-skill__foot' }, L('vb-skill__top', top, show(top.length > 0)), L('vb-skill__value', value)));
  }
  SkillButton.states = SKILL_STATES;

  /* ── Battle HUD ───────────────────────────────────────────────────────── */
  // With onCancel, manual rows are buttons that cancel their cast; autoplay estimates are not.
  function QueueList(p) {
    var q = p.queue || [];
    return h(Shape, { className: cx('vb-queue', p.className) },
      h('div', { className: 'vb-queue__head' }, L('vb-queue__title', (p.title || 'AUTOPLAY QUEUE') + ' · ' + q.length), L('vb-queue__hint', p.hint == null ? 'Fires as Power reaches each cost' : p.hint)),
      q.map(function (e, i) {
        var manual = !!e.manual && !!p.onCancel;
        return h(Shape, Object.assign({ key: i, className: cx('vb-queue__row', i === 0 && 'vb-queue__row--next', manual && 'vb-queue__row--manual') },
          manual ? Object.assign(clickable(function () { p.onCancel(e); }), { title: 'Cancel queued cast' }) : null),
          L('vb-queue__index', e.next ? '›' : String(i + 1)),
          h('div', { className: 'vb-queue__text' }, L('vb-queue__skill', upper(e.skill)), L('vb-queue__who', e.who)),
          L('vb-queue__cost', String(e.cost || 0)), L('vb-queue__eta', e.eta || ''),
          manual ? h(VbIcon, { icon: 'cancel', size: 14, className: 'vb-queue__cancel' }) : null);
      }));
  }

  var HUD = { speed: 1, wave: 1, score: 0, boss: false, auto: true, slow: false, power: 0, maxPower: 100, regen: '+12/s', hint: null, focus: 100, previewFrom: -1, previewTo: -1, queue: [], queueTitle: 'AUTOPLAY QUEUE', queueHint: 'Fires as Power reaches each cost' };
  function HudTop(p) {
    var v = Object.assign({}, HUD, p.view), q = v.queue || [];
    var pop = usePopover(function () { return q.length ? h(QueueList, { queue: q, title: v.queueTitle, hint: v.queueHint, onCancel: p.onQueueCancel }) : null; }, 'below', false, p.queuePinned);
    return h('div', { className: cx('vb-hud', v.boss && 'vb-hud--boss', p.className), style: p.style },
      h('div', { className: 'vb-hud__row' },
        h(Shape, Object.assign({ className: 'vb-hud__pause', title: 'Pause', 'aria-label': 'Pause' }, clickable(p.onPauseClick)), h(VbIcon, { icon: 'pause', size: 20 })),
        h(Shape, Object.assign({ className: cx('vb-hud__auto', v.auto && 'vb-hud__auto--on'), title: v.auto ? 'Autoplay on' : 'Autoplay off', 'aria-label': 'Toggle autoplay', 'aria-pressed': !!v.auto }, clickable(p.onAutoClick)), h(VbIcon, { icon: 'automation', size: 20 })),
        h('div', { className: 'vb-hud__stats' },
          h('div', { className: 'vb-hud__stat' }, L('vb-hud__key', v.boss ? 'BOSS' : 'WAVE'), h(GlitchText, { key: v.wave, text: pad2(v.wave), className: 'vb-hud__wave', effect: 'slam' })),
          h('div', { className: 'vb-hud__stat' }, L('vb-hud__key', 'SCORE'), L('vb-hud__score', compact(v.score)))),
        h(Shape, Object.assign({ className: 'vb-hud__speed', title: 'Toggle simulation speed' }, clickable(p.onSpeedClick)), L('vb-hud__auto-state', v.speed + '×'))),
      h(Shape, Object.assign({ ref: pop.ref, className: cx('vb-hud__resources', pop.open && 'vb-popover-open'), tabIndex: 0 }, pop.handlers),
        h('div', { className: 'vb-hud__resource-row' }, h(PowerStrip, { value: v.power, max: v.maxPower, regen: v.regen, hint: v.hint, previewFrom: v.previewFrom, previewTo: v.previewTo, marks: q.length ? [q[0].cost / Math.max(1, v.maxPower)] : null }), h(CreditsChip, { credits: v.credits || 0 })),
        h(FocusMeter, { focus: v.focus, slow: v.slow, style: show(p.showFocus !== false) })), pop.layer);
  }

  function BossBar(p) {
    var b = Object.assign({ name: '', health: 1, stability: 1, detail: '', shielded: false, interruptible: false, shieldDetail: '' }, p.view);
    return h(Shape, { className: cx('vb-boss', p.className) },
      h('div', { className: 'vb-boss__head' }, h(VbIcon, { icon: 'marker', size: 14, className: 'vb-boss__icon' }), L('vb-boss__name', upper(b.name)), L('vb-boss__detail', b.detail)),
      h(HealthBar, { className: 'vb-boss__health', hp: b.health, max: 1, barrier: 0, stability: b.stability, exposed: b.stability <= 0, enemy: true }),
      h('div', { className: 'vb-boss__chips', style: show(b.shielded || b.interruptible) },
        b.shielded ? h(StatusChip, { kind: 'barrier', text: 'Shielded', detail: b.shieldDetail }) : null,
        b.interruptible ? h(StatusChip, { kind: 'interrupted', text: 'Interruptible' }) : null));
  }

  /* ── Screens ──────────────────────────────────────────────────────────── */
  function ScreenHeader(p) {
    return h('div', { className: cx('vb-header', p.className) },
      h(Shape, Object.assign({ className: 'vb-header__back', title: p.backLabel || 'Back', 'aria-label': p.backLabel || 'Back' }, clickable(p.onBack)), h(VbIcon, { icon: 'back', size: 20 })),
      L('vb-header__title', upper(p.title)),
      L(cx('vb-header__meta', p.accent && 'vb-header__meta--accent'), p.meta || ''));
  }

  function ScoreRow(p) {
    var s = Object.assign({ rank: 0, score: 0, wave: 0, bosses: 0, squad: [], current: false }, p.view);
    return h(Shape, { className: cx('vb-score', s.current && 'vb-score--current', p.className) },
      L(cx('vb-score__rank', s.rank <= 3 && 'vb-score__rank--top'), pad2(s.rank)),
      h('div', { className: 'vb-score__text' },
        h('div', { className: 'vb-score__line' }, L('vb-score__value', num(s.score)), L('vb-score__badge', 'THIS RUN', show(s.current))),
        L('vb-score__detail', 'Wave ' + s.wave + ' · ' + s.bosses + ' ' + (s.bosses === 1 ? 'boss' : 'bosses'))),
      h('div', { className: 'vb-score__squad' }, s.squad.map(function (c, i) { return h(Shape, { key: i, className: 'vb-score__code' }, L('vb-score__code-text', c)); })));
  }

  /* ── Operatives ───────────────────────────────────────────────────────── */
  var OPERATIVE = { id: '', name: '', code: '', role: '', roleIcon: 'striker', passive: '', passiveText: '', passiveFullText: '', kit: '#ffffff', portrait: null, hp: 0, maxHp: 1, barrier: 0, armor: 0, stability: 100, maxStability: 100, statuses: [], mode: 'auto', manualSeconds: 0, skills: [{}, { slot: 'S', kind: 'Signature' }] };
  function operative(v) { return Object.assign({}, OPERATIVE, v); }
  function fallenOf(v) { return kebab(v.mode) === 'fallen' || v.hp <= 0; }
  function img(url) { return url ? { backgroundImage: 'url("' + url + '")' } : null; }
  function Strike(on, text) { return on ? h('s', null, text) : text; }

  function OperativeCard(p) {
    var v = operative(p.view), fallen = fallenOf(v), mode = fallen ? 'fallen' : kebab(v.mode);
    var chip = mode === 'manual' ? (v.manualSeconds > 0 ? 'MANUAL ' + seconds(v.manualSeconds) : 'MANUAL') : mode === 'aiming' ? 'AIMING' : mode === 'fallen' ? 'FALLEN' : 'AUTO';
    var statuses = (v.statuses || []).map(kebab).filter(function (s, i, a) { return a.indexOf(s) === i; });
    var hp = fallen ? 0 : v.hp;
    return h(Shape, { className: cx('vb-operative', st('vb-operative', mode), p.className), style: p.style },
      h('div', { className: 'vb-operative__band' }, h('div', { className: 'vb-operative__portrait', style: img(v.portrait) }), h('div', { className: 'vb-operative__shade' }),
        L('vb-operative__code', v.code), L('vb-operative__chip', chip)),
      h('div', { className: 'vb-operative__kit', style: { backgroundColor: v.kit } }),
      h('div', { className: 'vb-operative__body' },
        L('vb-operative__name', Strike(fallen, upper(v.name))),
        h(HealthBar, { hp: hp, max: v.maxHp, barrier: v.barrier, stability: fallen ? 0 : v.stability / Math.max(1, v.maxStability), exposed: statuses.indexOf('exposed') >= 0 }),
        h('div', { className: 'vb-operative__numbers' },
          h('div', { className: 'vb-l vb-operative__hp', style: { fontSize: (String(v.hp) + v.maxHp).length > 7 ? 10 : 11 } }, String(hp), h('span', { style: { color: '#b6aeb9' } }, '/' + v.maxHp)),
          L('vb-operative__barrier', v.barrier > 0 ? '+' + v.barrier : '', show(v.barrier > 0))),
        h('div', { className: 'vb-operative__statuses' },
          fallen ? L('vb-operative__clear', 'OUT') : statuses.length === 0 ? L('vb-operative__clear', 'CLEAR') :
            statuses.map(function (s) { return h(VbIcon, { key: s, icon: statusIcon(s), size: 14, title: s.charAt(0).toUpperCase() + s.slice(1), className: cx('vb-operative__status', 'vb-operative__status--' + s) }); }))),
      h('div', { className: 'vb-operative__skills', style: show(!fallen) }, [0, 1].map(function (i) {
        return h(SkillButton, { key: i, view: v.skills[i] || OPERATIVE.skills[i], dragging: p.dragging === i, onPointerDown: p.onSkillPointerDown ? function (e) { p.onSkillPointerDown(i, e); } : null });
      })),
      h('div', { className: 'vb-operative__fallen', style: show(fallen) }, L('vb-operative__fallen-title', 'FALLEN'), L('vb-operative__fallen-hint', 'Revive via perk')));
  }

  // `tier` none | blue | violet | gold frames the tile; `locked` ("Pull in rifts") says how to get a locked operative.
  function OperativeTile(p) {
    var v = operative(p.view), slot = p.slot || 0, locked = p.locked || '', isLocked = locked.length > 0;
    return h(Shape, Object.assign({ className: cx('vb-tile', slot > 0 && !isLocked && 'vb-tile--in', p.focused && 'vb-tile--focused', isLocked && 'vb-tile--locked', st('vb-tile', p.tier || 'none'), p.className),
      title: v.name + ', ' + v.role + (isLocked ? ', locked: ' + locked : slot > 0 ? ', squad slot ' + slot : ', not in squad') }, clickable(p.onPick)),
      h('div', { className: 'vb-tile__band' }, h('div', { className: 'vb-tile__portrait', style: img(v.portrait) }), h('div', { className: 'vb-tile__shade' }), L('vb-tile__order', String(slot), show(slot > 0 && !isLocked))),
      h('div', { className: 'vb-tile__kit', style: { backgroundColor: v.kit } }),
      h('div', { className: 'vb-tile__body' }, L('vb-tile__name', upper(v.name)),
        h('div', { className: 'vb-tile__role' }, h(VbIcon, { icon: isLocked ? 'lock' : v.roleIcon, size: 12 }), L('vb-tile__role-text', upper(isLocked ? locked : v.role)))));
  }

  function SquadSlot(p) {
    var has = !!p.view, v = operative(p.view), slot = p.slot || 0;
    return h(Shape, Object.assign({ className: cx('vb-slot', !has && 'vb-slot--empty', p.className) }, clickable(p.onPick)),
      h('div', { className: 'vb-slot__filled', style: show(has) },
        h('div', { className: 'vb-slot__band' }, h('div', { className: 'vb-slot__portrait', style: has ? img(v.portrait) : null }), h('div', { className: 'vb-slot__shade' }), L('vb-slot__order', String(slot))),
        h('div', { className: 'vb-slot__kit', style: { backgroundColor: v.kit } }),
        h('div', { className: 'vb-slot__row' }, L('vb-slot__name', upper(v.name)), L('vb-slot__code', v.code))),
      h('div', { className: 'vb-slot__empty', style: show(!has) }, h(VbIcon, { icon: 'add', size: 20 }), L('vb-slot__empty-label', 'SLOT ' + slot)));
  }

  function OperativeDetails(p) {
    var v = operative(p.view), slot = p.slot || 0, inSquad = slot > 0;
    var maxHp = p.maxHp || 1580, maxArmor = p.maxArmor || 26, maxStab = p.maxStability || 140;
    return h(Shape, { className: cx('vb-details', p.className), style: p.style },
      h('div', { className: 'vb-details__head' },
        h('div', { className: 'vb-details__kit', style: { backgroundColor: v.kit } }), L('vb-details__name', upper(v.name)),
        h('div', { className: 'vb-details__role' }, h(VbIcon, { icon: v.roleIcon, size: 14 }), L('vb-details__role-text', upper(v.role))),
        L(cx('vb-details__slot', inSquad && 'vb-details__slot--in'), inSquad ? 'SLOT ' + slot : 'NOT IN SQUAD'),
        h(Shape, Object.assign({ className: 'vb-details__close', title: 'Close details', 'aria-label': 'Close details' }, clickable(p.onClose)), h(VbIcon, { icon: 'close', size: 12 }))),
      h('div', { className: 'vb-details__stats' },
        h(StatReadout, { label: 'HEALTH', value: num(v.maxHp), bar: v.maxHp / maxHp }),
        h(StatReadout, { label: 'ARMOR', value: String(v.armor), bar: v.armor / maxArmor, className: 'vb-stat--neutral' }),
        h(StatReadout, { label: 'STABILITY', value: String(v.maxStability), bar: v.maxStability / maxStab, className: 'vb-stat--neutral' })),
      h('div', { className: 'vb-details__passive' }, L('vb-details__passive-name', 'PASSIVE · ' + upper(v.passive)), h('div', { className: 'vb-l vb-details__passive-text', title: v.passiveFullText }, v.passiveText)),
      h('div', { className: 'vb-details__skills' }, v.skills.map(function (s0, i) {
        var s = Object.assign({}, SKILL, { description: '', fullDescription: '', cooldownSeconds: 0 }, s0);
        return h('div', { key: i, className: 'vb-details__skill', title: s.fullDescription },
          h(Shape, { className: 'vb-details__skill-icon' }, h(VbIcon, { icon: s.icon, size: 14 })),
          h('div', { className: 'vb-details__skill-text' },
            h('div', { className: 'vb-details__skill-line' }, L('vb-details__skill-name', s.label), L('vb-details__skill-slot', s.slot + ' · ' + upper(s.kind))),
            L('vb-details__skill-desc', s.description)),
          h('div', { className: 'vb-details__skill-cost' }, L('vb-details__skill-power', String(s.cost)), L('vb-details__skill-cd', seconds(s.cooldownSeconds) + ' CD')));
      })),
      h(VbButton, { className: 'vb-details__toggle', title: inSquad ? 'REMOVE FROM SQUAD' : 'ADD TO SQUAD', icon: inSquad ? 'remove' : 'add', variant: inSquad ? 'danger' : 'light', blocked: !inSquad && p.squadFull ? 'Squad full · remove one first' : '', onClick: p.onToggle }));
  }

  function OperativeStats(p) {
    var c = Object.assign({ fellWave: 0, damage: 0, kills: 0, casts: 0, power: 0, share: 0, extraLabel: 'Basics', extraValue: '0' }, p.view), v = operative(c.operative);
    var cells = [['DAMAGE', num(c.damage)], ['KILLS', num(c.kills)], ['SKILLS CAST', num(c.casts)], ['POWER SPENT', num(c.power)], [upper(c.extraLabel), c.extraValue], ['WAVES SURVIVED', c.fellWave > 0 ? String(c.fellWave - 1) : 'ALL']];
    return h(Shape, { className: cx('vb-contribution', p.className) },
      h('div', { className: 'vb-contribution__head' },
        h(Shape, { className: 'vb-contribution__portrait' }, h('div', { className: 'vb-crop', style: img(v.portrait) })),
        h('div', { className: 'vb-contribution__head-text' }, L('vb-contribution__name', upper(v.name)),
          L(cx('vb-contribution__fell', c.fellWave <= 0 && 'vb-contribution__fell--alive'), c.fellWave > 0 ? 'FELL WAVE ' + c.fellWave : 'STANDING'))),
      h('div', { className: 'vb-contribution__share' },
        h('div', { className: 'vb-contribution__share-line' }, L('vb-contribution__share-label', 'Share of squad damage'), L('vb-contribution__share-value', Math.round(c.share * 100) + '%')),
        h('div', { className: 'vb-contribution__share-track' }, h('div', { className: 'vb-contribution__share-fill', style: { width: pct(c.share), backgroundColor: v.kit } }))),
      h('div', { className: 'vb-contribution__grid' }, cells.map(function (kv, i) { return h('div', { key: i, className: 'vb-contribution__cell' }, L('vb-contribution__key', kv[0]), L('vb-contribution__value', kv[1])); })));
  }

  function SquadResult(p) {
    var c = Object.assign({ fellWave: 0 }, p.view), v = operative(c.operative), fell = c.fellWave > 0;
    var pop = usePopover(function () { return h(OperativeStats, { view: c }); }, 'above', false, p.pinned);
    return h(Shape, Object.assign({ ref: pop.ref, className: cx('vb-result', fell && 'vb-result--fallen', pop.open && 'vb-popover-open', p.className), tabIndex: 0 }, pop.handlers),
      L('vb-result__name', Strike(fell, upper(v.name))), L('vb-result__fell', fell ? 'FELL W' + c.fellWave : 'STANDING'),
      h('div', { className: 'vb-result__kit', style: { backgroundColor: v.kit } }), pop.layer);
  }

  /* ── Perks ────────────────────────────────────────────────────────────── */
  var PERK = { id: '', name: '', category: '', tag: 'General', icon: 'add', effect: '', now: '', next: '', tier: 'general', art: null, stack: 0, max: 0, waves: [] };
  function perk(p) { return Object.assign({}, PERK, p); }
  function stacks(p, draft) {
    return p.max > 0 ? (draft ? 'STACK ' + (p.stack + 1) + ' / ' + p.max : '×' + p.stack + ' OF ' + p.max) : kebab(p.tier) === 'rare' ? 'RARE DRAW' : p.stack > 1 ? '×' + p.stack : 'REPEATABLE';
  }
  function tier(p) { return st('vb-perk', p.tier); }
  function Crop(url) { return h('div', { className: 'vb-crop', style: Object.assign({}, img(url), show(!!url)) }); }

  function PerkCard(p) {
    var v = perk(p.view), sel = !!p.selected, art = !!v.art;
    return h(Shape, Object.assign({ className: cx('vb-perk-card', tier(v), sel && 'vb-perk-card--selected', p.className), style: p.style }, clickable(p.onPick)),
      h('div', { className: cx('vb-perk-card__art', 'vb-perk-art', art && 'vb-perk-art--portrait') }, Crop(v.art),
        h('div', { className: 'vb-perk-card__shade' }),
        h(Shape, { className: 'vb-perk-card__badge' }, h(VbIcon, { icon: v.icon, size: art ? 16 : 32, className: 'vb-perk-card__glyph' })),
        v.tier === 'epic' ? h('div', { className: 'vb-perk-card__shimmer' }) : null, L('vb-perk-card__tag', upper(v.tag)), h(VbIcon, { icon: 'check', size: 14, className: 'vb-perk-card__check', style: show(sel) })),
      h('div', { className: 'vb-perk-card__rule' }),
      h('div', { className: 'vb-perk-card__body' }, L('vb-perk-card__category', upper(v.category)), L('vb-perk-card__name', upper(v.name)), L('vb-perk-card__effect', v.effect),
        h('div', { className: 'vb-perk-card__foot' },
          h('div', { className: 'vb-perk-card__pips' }, Array.apply(null, Array(v.max)).map(function (_, i) {
            return h('div', { key: i, className: cx('vb-perk-card__pip', i < v.stack ? 'vb-perk-card__pip--owned' : i === v.stack ? 'vb-perk-card__pip--next' : null) });
          })),
          L('vb-perk-card__next', v.next), L('vb-perk-card__stacks', stacks(v, true)))));
  }

  function PerkDetail(p) {
    var v = perk(p.view), draft = !!p.draft, art = !!v.art;
    var rows = draft ? [['Now', v.now, ''], ['After pick', v.next, '--good'], ['Stacks', v.max > 0 ? v.stack + ' → ' + (v.stack + 1) + ' / ' + v.max : stacks(v, true), '']]
      : [['Active', v.now, '--good'], ['Stacks', v.max > 0 ? v.stack + ' / ' + v.max : stacks(v, false), ''], ['Picked', v.waves.length ? v.waves.map(function (w) { return 'wave ' + w; }).join(', ') : '—', '']];
    return h(Shape, { className: cx('vb-perk-detail', tier(v), p.className), style: p.style },
      h('div', { className: 'vb-perk-detail__head' },
        h(Shape, { className: cx('vb-perk-detail__thumb', 'vb-perk-art', art && 'vb-perk-art--portrait') }, Crop(v.art), h(VbIcon, { icon: v.icon, size: 24 })),
        h('div', { className: 'vb-perk-detail__head-text' }, L('vb-perk-detail__name', upper(v.name)), L('vb-perk-detail__tag', upper(v.category + ' · ' + v.tag)))),
      L('vb-perk-detail__effect', v.effect),
      h('div', { className: 'vb-perk-detail__rows' }, rows.map(function (r, i) {
        return h('div', { key: i, className: cx('vb-perk-detail__row', i === rows.length - 1 && 'vb-perk-detail__row--last') },
          L('vb-perk-detail__key', r[0]), L(cx('vb-perk-detail__value', r[2] && 'vb-perk-detail__value' + r[2]), r[1]));
      })));
  }

  function PerkChip(p) {
    var v = perk(p.view);
    var pop = usePopover(function () { return h(PerkDetail, { view: v, draft: false, className: 'vb-perk-chip__popover' }); }, 'below', !!p.alignEnd, p.pinned);
    return h(Shape, Object.assign({ ref: pop.ref, className: cx('vb-perk-chip', tier(v), pop.open && 'vb-popover-open', p.className), tabIndex: 0, title: v.name + ' · ' + v.now }, pop.handlers),
      h(VbIcon, { icon: v.icon, size: 14 }), L('vb-perk-chip__name', upper(v.name)), L('vb-perk-chip__count', '×' + v.stack, show(v.stack > 1)), pop.layer);
  }

  function PerkRow(p) {
    var v = perk(p.view), art = !!v.art;
    return h(Shape, Object.assign({ className: cx('vb-perk-row', tier(v), p.selected && 'vb-perk-row--selected', p.className) }, clickable(p.onPick)),
      h(Shape, { className: cx('vb-perk-row__thumb', 'vb-perk-art', art && 'vb-perk-art--portrait') }, Crop(v.art), h(VbIcon, { icon: v.icon, size: 20 })),
      h('div', { className: 'vb-perk-row__text' }, h('div', { className: 'vb-perk-row__line' }, L('vb-perk-row__name', upper(v.name)), L('vb-perk-row__tag', upper(v.tag))), L('vb-perk-row__effect', v.effect)),
      h('div', { className: 'vb-perk-row__right' }, L('vb-perk-row__value', v.now), L('vb-perk-row__stacks', stacks(v, false))));
  }

  /* ── Pages (Dialogs.cs) ───────────────────────────────────────────────── */
  /** Dialog.Sheet: a solid panel with a title and a line of body text; children are the actions. */
  function Dialog(p) {
    return h(Shape, { className: cx('vb-panel', 'vb-dialog', 'vb-panel--solid', p.className), style: p.style },
      p.title ? L('vb-t-title vb-dialog__title', p.title) : null,
      p.body ? L(cx(p.bodyClass || 'vb-t-caption', 'vb-dialog__body'), p.body) : null, p.children);
  }
  function Backdrop(p) {
    return h('div', { className: cx('vb-backdrop', p.className), style: Object.assign({}, img(p.image), p.style) }, h(Shape, { className: 'vb-backdrop__shade' }), p.children);
  }

  /* ── Holographic menu surfaces (Holo.cs) ──────────────────────────────── */
  function ScanLines(p) { return h('div', { className: cx('vb-scanlines', p && p.className), 'aria-hidden': true }); }
  function calm(el, flashes) { return !!(el && el.closest && el.closest(flashes ? '.vb-reduced-motion, .vb-reduced-flashes' : '.vb-reduced-motion')); }
  /** HoloFrame.FlickerIn: opacity .7 → .25 → .9 → 1 in 60 ms steps; a plain fade under reduced motion or flashes. */
  function HoloFrame(p) {
    var ref = R.useRef(null), t = upper(p.title || '');
    R.useLayoutEffect(function () {
      var el = ref.current; if (!el || p.still) return undefined;
      var steps = calm(el, true) ? [0.25, 0.5, 0.75, 1] : [0.7, 0.25, 0.9, 1]; el.style.opacity = 0;
      var timers = steps.map(function (o, i) { return setTimeout(function () { el.style.opacity = o; }, 60 * (i + 1)); });
      return function () { timers.forEach(clearTimeout); el.style.opacity = ''; };
    }, []);
    return h(Shape, { ref: ref, className: cx('vb-holo', p.className), style: p.style },
      h(ScanLines),
      h('div', { className: 'vb-holo__head', style: show(t.length > 0) },
        L('vb-holo__title vb-holo__title--cyan', t), L('vb-holo__title vb-holo__title--pink', t), L('vb-holo__title', t)),
      h('div', { className: 'vb-holo__body' }, p.children));
  }
  /** The 2D stand-in for a lineup plinth: occupied, selected (brackets) or open (dashed OPEN SLOT). */
  function HoloPlatform(p) {
    var state = kebab(p.state || 'occupied'), open = state === 'open', label = open ? '+ OPEN SLOT' : upper(p.name), sub = open ? 'weight shared' : (p.detail || '');
    return h('div', Object.assign({ className: cx('vb-platform', st('vb-platform', state), st('vb-platform', p.team || 'ally'), p.className), style: p.style, title: open ? 'Open slot' : p.name }, clickable(p.onPick)),
      h(VbIcon, { icon: 'diamond', size: 18, className: 'vb-platform__marker', style: show(p.showMarker !== false && !open) }),
      h('div', { className: 'vb-platform__tag' }, L('vb-platform__label', label), L('vb-platform__sub', sub, show(sub.length > 0))),
      h(Shape, { className: 'vb-platform__plate' }, h(ScanLines)));
  }
  function tierWord(tier, item) {
    tier = kebab(tier);
    return item ? (tier === 'gold' ? 'UNIQUE ITEM' : tier === 'violet' ? 'RARE ITEM' : 'MAGIC ITEM') : (tier === 'gold' ? 'LEGENDARY' : tier === 'violet' ? 'EPIC' : 'RARE');
  }
  /** RevealCard.Flip: an edge-on squash (violet waits 90 ms, gold 180 ms), or at once under reduced motion. */
  function RevealCard(p) {
    var tier = kebab(p.tier || 'blue'), f = R.useState(!!p.flipped), up = f[0], ref = R.useRef(null);
    R.useEffect(function () { f[1](!!p.flipped); }, [p.flipped]);
    function flip() {
      if (up) return; if (p.onFlip) p.onFlip(); var el = ref.current;
      if (!el || calm(el, false)) { f[1](true); return; }
      var wait = tier === 'gold' ? 180 : tier === 'violet' ? 90 : 0;
      setTimeout(function () { el.style.scale = '0.05 1'; }, wait);
      setTimeout(function () { f[1](true); }, wait + 110);
      setTimeout(function () { el.style.scale = ''; }, wait + 130);
    }
    var badge = p.dupe ? upper(p.dupe) : p.isNew !== false ? 'NEW' : '';
    return h('div', Object.assign({ ref: ref, className: cx('vb-reveal', st('vb-reveal', tier), up && 'vb-reveal--flipped', p.className), style: p.style }, clickable(flip)),
      h(Shape, { className: 'vb-reveal__back', style: show(!up) }, h(ScanLines), h(VbMark, { width: 56 })),
      h(Shape, { className: 'vb-reveal__front', style: show(up) },
        L('vb-reveal__tier', p.tierLabel || tierWord(tier, p.item)), L('vb-reveal__name', upper(p.title)), L('vb-reveal__line', p.detail || ''),
        L(cx('vb-reveal__badge', p.dupe && 'vb-reveal__badge--dupe'), badge, show(badge.length > 0))));
  }
  RevealCard.tierWord = tierWord;
  /** One IF … THEN automation rule (RuleView); the toggle raises onToggle(enabled). */
  function RuleRow(p) {
    var v = Object.assign({ index: 1, conditions: [], any: false, action: '', actionRest: '', cost: 0, enabled: true, warning: false, draft: false, note: '' }, p.view);
    var e = useValue(!!v.enabled, p.onToggle), on = e[0], parts = [L('vb-rule__keyword', 'IF')];
    v.conditions.forEach(function (c, i) {
      if (i > 0) parts.push(L('vb-rule__join', v.any ? 'OR' : 'AND'));
      parts.push(L('vb-rule__text', c.text)); if (c.value) parts.push(L('vb-rule__value', c.value)); if (c.after) parts.push(L('vb-rule__text', c.after));
    });
    return h(Shape, { className: cx('vb-rule', !on && 'vb-rule--off', v.warning && 'vb-rule--warning', v.draft && 'vb-rule--draft', p.className), style: p.style },
      L('vb-rule__index', String(v.index)),
      h('div', { className: 'vb-rule__body' },
        h.apply(null, ['div', { className: 'vb-rule__if' }].concat(parts)),
        h('div', { className: 'vb-rule__then' }, L('vb-rule__keyword', 'THEN'), L('vb-rule__action', upper(v.action) + (v.actionRest ? ' ' + v.actionRest : '')),
          L('vb-rule__cost', v.cost > 0 ? '−' + v.cost : '', show(v.cost > 0))),
        L('vb-rule__note', v.note, show(!!v.note))),
      h('div', Object.assign({ className: cx('vb-rule__toggle', on && 'vb-rule__toggle--on'), 'aria-pressed': on }, clickable(function () { e[1](!on); }), { title: on ? 'Turn rule off' : 'Turn rule on' }),
        h('div', { className: 'vb-rule__knob' })));
  }

  /* ── World layer (WorldElements.cs); WorldLayer itself places them over a 3D view ── */
  function WorldHealthBar(p) {
    var hp = clamp01(p.health == null ? 1 : p.health), ghost = Math.max(hp, clamp01(p.ghost == null ? hp : p.ghost)), b = Math.min(clamp01(p.barrier), 1 - hp);
    return h('div', { className: cx('vb-world-health', st('vb-world-health', p.team || 'enemy'), st('vb-world-health', p.rank || 'normal'), p.className), style: p.style },
      h('div', { className: 'vb-world-health__ghost', style: { width: pct(ghost) } }),
      h('div', { className: 'vb-world-health__fill', style: Object.assign({ width: pct(hp) }, p.tint ? { backgroundColor: p.tint } : null) }),
      h('div', { className: 'vb-world-health__barrier', style: b > 0 ? { left: pct(hp), width: pct(b) } : { display: 'none' } }));
  }
  function combatText(kind, amount) {
    var n = num(amount);
    switch (kebab(kind)) {
      case 'taken': return '−' + n; case 'crit': return n + '!'; case 'exposed': return 'EXPOSED ' + n; case 'heal': return '+' + n;
      case 'barrier': return 'BLOCKED ' + n; case 'power': return '+' + n + ' POWER'; case 'miss': return 'MISS'; case 'execute': return 'EXECUTE ' + n; case 'break': return 'BREAK';
      default: return n;
    }
  }
  function CombatText(p) { var k = kebab(p.kind || 'damage'); return L(cx('vb-combat-text', st('vb-combat-text', k), p.className), combatText(k, p.amount), p.style); }
  CombatText.format = combatText;
  function WorldLabel(p) {
    var k = kebab(p.kind || 'name'), d = p.detail || '';
    return h('div', { className: cx('vb-world-label', st('vb-world-label', k), st('vb-world-label', p.team || 'ally'), p.className), style: p.style },
      L('vb-world-label__title', (k === 'blocked' ? '× ' : '') + (p.title || '')), L('vb-world-label__detail', d, show(d.length > 0 && p.showDetail !== false)));
  }
  function WorldMarker(p) {
    var k = kebab(p.kind || 'ally');
    return h('div', { className: cx('vb-world-marker', st('vb-world-marker', k), p.className), style: Object.assign({}, p.size > 0 ? { width: p.size, height: p.size } : null, p.style) },
      h(VbIcon, { icon: k === 'enemy' ? 'diamond' : 'circle', className: 'vb-world-marker__glyph', style: { width: '100%', height: '100%' } }));
  }
  /** `angle`: degrees clockwise from screen-right toward the fighter (WorldLayer.Pin). */
  function OffscreenIndicator(p) {
    var m = p.metres == null ? -1 : p.metres;
    return h('div', { className: cx('vb-offscreen', st('vb-offscreen', p.team || 'enemy'), p.threat && 'vb-offscreen--threat', p.className), style: p.style },
      h(VbIcon, { icon: 'chevron', size: 12, className: 'vb-offscreen__arrow', style: { rotate: (p.angle || 0) + 'deg' } }),
      L('vb-offscreen__name', p.name || ''), L('vb-offscreen__distance', m >= 0 ? Math.round(m) + 'm' : '', show(m >= 0)));
  }

  /** The breach band: chevrons over the bottom (or `top`) edge; `urgency` 0 hides it, above 2/3 it runs hot. Static here (the Unity element ripples). */
  function EdgeWarning(p) {
    var u = p.urgency == null ? 1 : p.urgency;
    return h('div', { className: cx('vb-edge-warning', p.top && 'vb-edge-warning--top', u > 0.66 && 'vb-edge-warning--hot', p.className), style: Object.assign({ display: u > 0 ? 'flex' : 'none' }, p.style) },
      h('div', { className: 'vb-edge-warning__row' }, [0, 1, 2, 3, 4].map(function (i) {
        return h(VbIcon, { key: i, icon: 'chevron', size: 14, className: 'vb-edge-warning__chevron', style: { opacity: 0.35 + 0.65 * (1 - Math.abs(i - 2) / 2) } });
      })),
      L('vb-edge-warning__caption', p.caption || ''));
  }

  function GlitchText(p) {
    return h('div', { className: cx('vb-glitch', 'vb-web-glitch--' + (p.effect || 'in'), p.className), style: p.style },
      L('vb-glitch__main', p.text),
      h('div', { className: 'vb-glitch__copy vb-glitch__red', 'aria-hidden': true }, p.text),
      h('div', { className: 'vb-glitch__copy vb-glitch__cyan', 'aria-hidden': true }, p.text), Array.apply(null, Array(8)).map(function (_, i) { return h('div', { key: i, className: 'vb-glitch__clip', 'aria-hidden': true, style: { top: (i * 12.5) + '%', height: '12.5%', width: '100%', animationDelay: (i % 3 * -16) + 'ms' } }, h('div', { className: 'vb-glitch__copy vb-glitch__slice', style: { top: (-i * 100) + '%', height: '800%', width: '100%' } }, p.text)); }));
  }
  function CreditsChip(p) { return h(Shape, { className: cx('vb-credits', p.className), title: 'Credits' }, h(VbIcon, { icon: 'currency', size: 14 }), L('vb-credits__value', num(p.credits))); }
  function RouteChoice(p) {
    var v = p.view || {}, locked = R.useRef(false); R.useEffect(function () { locked.current = false; }, [v.wave]);
    return h('div', { className: cx('vb-route', p.className), style: p.style }, L('vb-t-label vb-route__eyebrow', 'CROSSROADS · WAVE ' + pad2(v.wave)), h(GlitchText, { text: 'CHOOSE YOUR ROAD', className: 'vb-t-display vb-route__title' }), L('vb-t-caption', 'Two roads. One choice. Your build carries forward.'),
      h('div', { className: 'vb-route__roads' }, [v.left || {}, v.right || {}].map(function (road, side) { return h(Shape, Object.assign({ key: side, className: 'vb-route__sign vb-route__sign--' + (road.kind || 'combat') }, clickable(function () { if (!locked.current && p.onChoose) { locked.current = true; p.onChoose(side); } })), L('vb-t-badge vb-route__direction', side ? 'RIGHT ROAD →' : '← LEFT ROAD'), h(VbIcon, { icon: road.icon || 'battle', size: 36, className: 'vb-route__glyph' }), L('vb-route__kind', upper(road.title)), L('vb-route__hint', road.hint), h('div', { className: 'vb-grow' }), L('vb-route__confirm', 'TAKE THIS ROAD')); })));
  }
  function MerchantScreen(p) {
    var v = Object.assign({ credits: 0, offers: [], stacks: [], health: [] }, p.view);
    function heading(t) { return L('vb-t-label vb-merchant__section', t); }
    return h('div', { className: cx('vb-merchant', p.className), style: p.style },
      h('div', { className: 'vb-merchant__head' }, h(GlitchText, { text: 'NIGHT MARKET', className: 'vb-t-display vb-merchant__title' }), h(CreditsChip, { credits: v.credits })),
      h('div', { className: 'vb-merchant__scroll', style: { overflowY: 'auto', flexShrink: 1 } }, L('vb-t-caption', 'Trade what you carry. Buy what changes the next fight.'), heading('RARE FINDS · ONE OF EACH'),
        h('div', { className: 'vb-merchant__offers' }, v.offers.map(function (o, i) { return h('div', { key: i, className: 'vb-merchant__offer' }, h(PerkCard, { view: o.perk }), h(VbButton, { title: o.taken ? 'SOLD' : 'BUY · ' + o.price, sub: o.price + ' credits', blocked: o.taken ? 'Sold' : v.credits < o.price ? 'Need ' + (o.price - v.credits) : o.blocked, className: 'vb-merchant__buy', onClick: function () { if (p.onBuy) p.onBuy(i); } })); })),
        heading('SELL STACKS'), v.stacks.length ? v.stacks.map(function (s, i) { return h(Shape, { key: i, className: 'vb-merchant__exchange' }, h('div', { className: 'vb-merchant__info' }, L('vb-t-label', upper(s.perk.name) + ' ×' + s.perk.stack), L('vb-t-caption', 'Sell one stack · ' + s.perk.now)), h(VbButton, { title: 'SELL +' + s.payout, className: 'vb-merchant__sell', onClick: function () { if (p.onSellPerk) p.onSellPerk(s.perk.id); } })); }) : L('vb-t-caption', 'No lasting stacks to sell.'),
        heading('TRADE HEALTH'), L('vb-t-caption', 'Pay 25% of max HP per trade. At least 1 HP must remain.'), v.health.map(function (hp) { return h(Shape, { key: hp.actor, className: 'vb-merchant__exchange' }, h('div', { className: 'vb-merchant__info' }, L('vb-t-label', upper(hp.name)), L('vb-t-caption', hp.hp + ' / ' + hp.maxHp + ' HP → ' + Math.max(0, hp.hp - hp.cost) + ' HP'), h(HealthBar, { hp: hp.hp, max: hp.maxHp })), h(VbButton, { title: 'SELL +' + hp.payout, variant: 'danger', className: 'vb-merchant__sell', blocked: hp.blocked, onClick: function () { if (p.onSellHealth) p.onSellHealth(hp.actor); } })); })),
      h(VbButton, { title: 'LEAVE MARKET', variant: 'primary', sub: 'Keep your credits · continue to the next wave', icon: 'chevron', className: 'vb-merchant__leave', onClick: p.onLeave }));
  }

  var api = {
    GlitchText: GlitchText, CreditsChip: CreditsChip, RouteChoice: RouteChoice, MerchantScreen: MerchantScreen,
    VbFrame: VbFrame, VbIcon: VbIcon, VbMark: VbMark, VbButton: VbButton, StatReadout: StatReadout, StatusChip: StatusChip,
    VbSwitch: VbSwitch, VbSlider: VbSlider, VbSegmented: VbSegmented, VbMeter: VbMeter, HealthBar: HealthBar, PowerStrip: PowerStrip,
    FocusMeter: FocusMeter, SkillButton: SkillButton, QueueChip: QueueChip, HudTop: HudTop, QueueList: QueueList, BossBar: BossBar, ScreenHeader: ScreenHeader,
    ScoreRow: ScoreRow, OperativeCard: OperativeCard, OperativeTile: OperativeTile, SquadSlot: SquadSlot, OperativeDetails: OperativeDetails,
    OperativeStats: OperativeStats, SquadResult: SquadResult, PerkCard: PerkCard, PerkDetail: PerkDetail, PerkChip: PerkChip, PerkRow: PerkRow,
    Dialog: Dialog, Backdrop: Backdrop, ScanLines: ScanLines, HoloFrame: HoloFrame, HoloPlatform: HoloPlatform, RevealCard: RevealCard, RuleRow: RuleRow,
    WorldHealthBar: WorldHealthBar, CombatText: CombatText, WorldLabel: WorldLabel, WorldMarker: WorldMarker, OffscreenIndicator: OffscreenIndicator, EdgeWarning: EdgeWarning,
    VbShape: Shape, format: { compact: compact, number: num, seconds: seconds, kebab: kebab }
  };
  window.Veilbreak = Object.assign(window.Veilbreak || {}, api);
})();
