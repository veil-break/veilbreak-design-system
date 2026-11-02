/* @ds-bundle: {"format":4,"namespace":"Veilbreak","components":[{"name":"Icon"},{"name":"Panel"},{"name":"HoloFrame"},{"name":"Button"},{"name":"RailButton"},{"name":"Switch"},{"name":"Segmented"},{"name":"NavBar"},{"name":"MatchHeader"},{"name":"FighterPortrait"},{"name":"AbilityButton"},{"name":"PowerStrip"},{"name":"QueueChip"},{"name":"HealthBar"},{"name":"StatusChip"},{"name":"ConnectionPill"},{"name":"ConnectionBanner"},{"name":"CombatFeed"},{"name":"Minimap"},{"name":"OffscreenIndicator"},{"name":"WorldLabel"},{"name":"WorldMarker"},{"name":"Telegraph"},{"name":"CombatText"},{"name":"HoloPlatform"},{"name":"TeamTile"},{"name":"ItemCell"},{"name":"StatReadout"},{"name":"RevealCard"},{"name":"RuleRow"},{"name":"Timeline"}]} */
(function () {
  'use strict';
  var R = window.React;
  var h = R.createElement;
  var F = R.Fragment;

  function cx() { return Array.prototype.filter.call(arguments, Boolean).join(' '); }
  function clamp(n) { n = Number(n); return isNaN(n) ? 0 : Math.max(0, Math.min(1, n)); }
  function pct(n) { return (n * 100).toFixed(2) + '%'; }
  function fmt(n) { return typeof n === 'number' ? n.toLocaleString('en-US') : n; }
  function clock(s, dec) {
    var m = Math.floor(s / 60), r = s - m * 60;
    var ss = dec ? r.toFixed(1) : String(Math.floor(r));
    if (r < 10) ss = '0' + ss;
    return m + ':' + ss;
  }
  var uid = 0;
  function useUid(prefix) {
    var ref = R.useRef(null);
    if (ref.current === null) { uid += 1; ref.current = prefix + uid; }
    return ref.current;
  }

  /* ── Icons: 24 grid, 1.6 stroke, square caps, no fills ──────────────────── */
  var C = function (cx_, cy, r) { return 'M' + (cx_ + r) + ' ' + cy + 'A' + r + ' ' + r + ' 0 1 1 ' + (cx_ - r) + ' ' + cy + 'A' + r + ' ' + r + ' 0 1 1 ' + (cx_ + r) + ' ' + cy; };
  var ICONS = {
    /* ability shapes */
    arrow: 'M6 18L18 6M10 6H18V14',
    fan: 'M4 12H20M4 12L19 6M4 12L19 18',
    pierce: 'M4 12H20M15 7L20 12L15 17M4 8L7 12L4 16',
    projectile: 'M3 12H14M15 9H21V15H15Z',
    cone: 'M4 12L19 5M4 12L19 19M19 5Q22.5 12 19 19',
    burst: C(12, 12, 8) + 'M8 16L16 8M6.5 12.5L12.5 6.5M11.5 17.5L17.5 11.5',
    zone: C(12, 12, 8.5) + C(12, 12, 3.5),
    beam: 'M3 9H21V15H3ZM8 9L5 15M13 9L10 15M18 9L15 15',
    dash: 'M3 12H15M11 8L15 12L11 16M19 6V18',
    wall: 'M15 3V21M3 9L11 10M3 15L11 14',
    /* roles */
    ranged: C(12, 12, 6) + 'M12 3V8M12 16V21M3 12H8M16 12H21',
    'burst-role': C(12, 12, 3.2) + 'M12 2V6M12 18V22M2 12H6M18 12H22M5 5L7.8 7.8M16.2 16.2L19 19M19 5L16.2 7.8M7.8 16.2L5 19',
    disruptor: 'M13 3L6 13H12L10 21L18 10H12Z',
    debuffer: 'M12 3L20 12L12 21L4 12ZM12 8V15M9 12L12 15L15 12',
    protector: 'M12 3L19 6V12C19 16 16 19 12 21C8 19 5 16 5 12V6Z',
    skirmisher: 'M5 6L11 12L5 18M12 6L18 12L12 18',
    support: 'M3 12H7L9.5 6L14 18L16 12H21',
    healer: 'M10 4H14V10H20V14H14V20H10V14H4V10H10Z',
    /* statuses */
    exposed: 'M12 3L19 6V12C19 16 16 19 12 21C8 19 5 16 5 12V6ZM13 6L10 11H14L11 17',
    silenced: C(12, 12, 8) + 'M6.3 6.3L17.7 17.7',
    rooted: 'M12 3V16M7 11L12 16L17 11M5 20H19',
    slowed: 'M6 3H18M6 21H18M7 3L17 21M17 3L7 21',
    interrupted: 'M13 3L7 12H11.5M12.5 12H17L11 21',
    barrier: 'M12 3L20 7.5V16.5L12 21L4 16.5V7.5Z',
    /* gear slots */
    optic: 'M2 12C5 7 8.5 5 12 5C15.5 5 19 7 22 12C19 17 15.5 19 12 19C8.5 19 5 17 2 12Z' + C(12, 12, 2.6),
    core: 'M12 3L20 7.5V16.5L12 21L4 16.5V7.5Z' + C(12, 12, 2.6),
    plating: 'M12 3L19 6V12C19 16 16 19 12 21C8 19 5 16 5 12V6ZM5 11H19',
    weapon: 'M4 20L16 8M13 4H20V11M7 13L11 17',
    charm: 'M12 4L20 12L12 20L4 12ZM12 9L15 12L12 15L9 12Z',
    /* hud */
    queue: 'M5 7H19M5 12H15M5 17H11',
    menu: 'M4 7H20M4 12H20M4 17H20',
    signal: 'M5 19V16M10 19V12M15 19V8M20 19V4',
    settings: C(12, 12, 3.2) + 'M12 2V5M12 19V22M2 12H5M19 12H22M4.9 4.9L7 7M17 17L19.1 19.1M19.1 4.9L17 7M7 17L4.9 19.1',
    surrender: 'M6 21V4M6 4H18L15 8.5L18 13H6',
    autopilot: 'M19 12A7 7 0 1 1 16.95 7.05M17 3V7.05H13',
    cancel: 'M6 6L18 18M18 6L6 18',
    check: 'M5 12L10 17L19 7',
    lock: 'M6 11H18V20H6ZM9 11V8A3 3 0 0 1 15 8V11',
    plus: 'M12 5V19M5 12H19',
    'chevron-right': 'M9 6L15 12L9 18',
    'chevron-left': 'M15 6L9 12L15 18',
    back: 'M15 5L8 12L15 19',
    play: 'M8 5L18 12L8 19Z',
    pause: 'M8 5V19M16 5V19',
    prev: 'M18 6L10 12L18 18ZM6 6V18',
    next: 'M6 6L14 12L6 18ZM18 6V18',
    replay: 'M4 12A8 8 0 1 0 6.3 6.3M4 4V8H8M10 9L15 12L10 15Z',
    team: C(9, 8, 3) + 'M3 20C3 16 6 14 9 14C12 14 15 16 15 20M15 5.5A3 3 0 0 1 15 10.5M17 14C19.5 14.5 21 16.5 21 20',
    studio: 'M12 3L20 7.5V16.5L12 21L4 16.5V7.5ZM4 7.5L12 12L20 7.5M12 12V21',
    automation: 'M6 7H18V19H6ZM12 3V7M9.5 12V14M14.5 12V14M3 12H6M18 12H21',
    training: C(12, 12, 8) + C(12, 12, 4) + 'M12 12H12.01',
    key: C(8, 12, 4) + 'M12 12H21M18 12V16M21 12V15',
    rift: 'M12 3A9 9 0 1 1 3 12M12 7A5 5 0 1 1 7 12M12 11A1 1 0 1 1 11 12',
    'no-target': C(12, 12, 6) + 'M12 3V8M12 16V21M3 12H8M16 12H21M4 4L20 20',
    warning: 'M12 3L22 20H2ZM12 9V14M12 17V17.2',
    diamond: 'M12 4L20 12L12 20L4 12Z',
    circle: C(12, 12, 8)
  };

  function Icon(p) {
    var size = p.size || 24;
    var d = ICONS[p.name] || ICONS.diamond;
    return h('svg', {
      className: cx('vb-icon', p.className), width: size, height: size, viewBox: '0 0 24 24', style: p.style,
      role: p.title ? 'img' : undefined, 'aria-hidden': p.title ? undefined : 'true'
    }, p.title ? h('title', null, p.title) : null, h('path', { d: d }));
  }
  Icon.names = Object.keys(ICONS);

  /* ── surfaces ───────────────────────────────────────────────────────────── */
  function Panel(p) {
    var v = p.variant || 'hud';
    return h('section', {
      className: cx('vb vb-shape vb-panel', v !== 'hud' && 'vb-panel--' + v, 'vb-cut-' + (p.cut || 'md'), p.desktop && 'vb-panel--desktop',
        p.brackets && 'vb-brackets', p.scan && 'vb-scan', p.className),
      style: p.style, 'aria-label': p.title || undefined
    },
      (p.title || p.meta) && h('header', { className: 'vb-panel__head' },
        p.title && h('h3', { className: 'vb-panel__title', style: { margin: 0 } }, p.title),
        p.meta && h('span', { className: 'vb-panel__meta' }, p.meta)),
      p.children);
  }

  function HoloFrame(p) {
    var ref = R.useRef(null);
    var pose = p.pose || [0, 0];
    function set(rx, ry) {
      var el = ref.current; if (!el) return;
      el.style.setProperty('--vb-rx', rx.toFixed(2) + 'deg');
      el.style.setProperty('--vb-ry', ry.toFixed(2) + 'deg');
    }
    R.useEffect(function () { set(pose[0], pose[1]); }, [pose[0], pose[1]]);
    function move(e) {
      if (!p.tilt) return;
      var el = ref.current, r = el.getBoundingClientRect();
      var max = parseFloat(getComputedStyle(el).getPropertyValue('--holo-tilt')) || 6;
      var x = (e.clientX - r.left) / r.width - 0.5, y = (e.clientY - r.top) / r.height - 0.5;
      set(-y * 2 * max, x * 2 * max);
    }
    return h('div', { className: 'vb vb-holo', onPointerMove: move, onPointerLeave: function () { set(pose[0], pose[1]); }, style: p.style },
      h('div', {
        ref: ref, style: { width: p.width },
        className: cx('vb-shape vb-cut-' + (p.cut || 'lg'), 'vb-holo__card vb-brackets vb-scan', p.animate !== false && 'vb-flicker-in', p.className)
      },
        p.title && h('div', { className: 'vb-holo__title vb-chroma' }, p.title),
        p.children));
  }

  /* ── actions ────────────────────────────────────────────────────────────── */
  function Button(p) {
    var v = p.variant || 'secondary';
    var blocked = p.blocked;
    var btn = h('button', {
      type: 'button',
      className: cx('vb vb-shape vb-btn vb-press', 'vb-btn--' + v, p.size === 'lg' && 'vb-btn--lg', p.icon && 'vb-btn--icon', p.center && 'vb-btn--center', p.className),
      'aria-disabled': blocked ? 'true' : undefined, onClick: blocked ? undefined : p.onClick, style: p.style
    },
      p.icon && h(Icon, { name: p.icon, size: 18 }),
      h('span', { className: 'vb-btn__label' }, p.children),
      p.sub && !p.icon && h('span', { className: 'vb-btn__sub' }, p.sub));
    if (blocked) return h('span', { className: 'vb', style: { display: 'inline-flex', flexDirection: 'column' } }, btn, h('span', { className: 'vb-btn__reason' }, blocked));
    if (v === 'cta') return h('span', { className: 'vb-glow-cta', style: { display: 'inline-block' } }, btn);
    return btn;
  }

  function RailButton(p) {
    return h('span', { className: 'vb vb-rail__wrap' },
      h('button', {
        type: 'button', className: cx('vb-shape vb-rail vb-press', p.active && 'vb-rail--active', p.desktop && 'vb-rail--desktop'),
        'aria-current': p.active ? 'page' : undefined, 'aria-label': p.label ? undefined : (p.ariaLabel || p.icon), onClick: p.onClick
      },
        h(Icon, { name: p.icon || 'menu', size: 20 }),
        p.label && h('span', null, p.label)),
      p.isNew && h('span', { className: 'vb-rail__new', role: 'img', 'aria-label': 'New' }));
  }

  function Switch(p) {
    var st = R.useState(!!p.checked);
    var on = p.onChange ? !!p.checked : st[0];
    function toggle() { if (p.onChange) p.onChange(!on); else st[1](!on); }
    return h('button', { type: 'button', role: 'switch', 'aria-checked': on ? 'true' : 'false', className: 'vb vb-switch', onClick: toggle },
      h('span', { className: 'vb-switch__track' }, h('span', { className: 'vb-switch__knob' })),
      (p.label || p.sub) && h('span', null,
        p.label && h('span', { className: 'vb-switch__label' }, p.label),
        p.sub && h('span', { className: 'vb-switch__sub' }, p.sub)));
  }

  function Segmented(p) {
    var opts = p.options || [];
    var st = R.useState(p.value);
    var val = p.onChange ? p.value : st[0];
    return h('div', { role: 'radiogroup', 'aria-label': p.label, className: 'vb vb-shape vb-seg vb-cut-sm', style: p.style },
      opts.map(function (o) {
        var id = o.value != null ? o.value : o.label;
        return h('button', {
          key: id, type: 'button', role: 'radio', 'aria-checked': id === val ? 'true' : 'false', className: 'vb-seg__opt',
          onClick: function () { if (p.onChange) p.onChange(id); else st[1](id); }
        }, h('span', { className: 'vb-seg__label' }, o.label), o.sub && h('span', { className: 'vb-seg__sub' }, o.sub));
      }));
  }

  function NavBar(p) {
    var items = p.items || ['Play', 'Team', 'Automation', 'Studio', 'Training', 'Replays', 'Settings'];
    return h('nav', { className: 'vb vb-nav', 'aria-label': 'Main' },
      h('span', { className: 'vb-nav__brand' }, 'VEIL', h('b', null, '/'), 'BREAK'),
      h('div', { className: 'vb-nav__items' }, items.map(function (it) {
        return h('button', { key: it, type: 'button', className: 'vb-nav__item', 'aria-current': it === p.active ? 'page' : undefined, onClick: p.onNavigate && function () { p.onNavigate(it); } }, it);
      })),
      p.rating != null && h('span', { className: 'vb-nav__meta' }, 'Rating', h('b', null, fmt(p.rating))));
  }

  /* ── battle HUD ─────────────────────────────────────────────────────────── */
  function HealthBar(p) {
    var max = p.max || 100;
    var hp = clamp(p.value / max);
    var bar = p.barrier ? clamp(p.barrier / max) : 0;
    var st = p.stability == null ? null : clamp(p.stability / 100);
    return h('div', {
      className: cx('vb-hp', p.team === 'enemy' && 'vb-hp--enemy', p.size && p.size !== 'hud' && 'vb-hp--' + p.size, p.exposed && 'vb-hp--exposed'),
      role: 'meter', 'aria-label': p.label || 'Health', 'aria-valuenow': p.value, 'aria-valuemin': 0, 'aria-valuemax': max, style: p.style
    },
      h('div', { className: 'vb-hp__track' },
        p.ghost != null && h('i', { className: 'vb-hp__ghost', style: { left: 0, width: pct(clamp(p.ghost / max)) } }),
        h('i', { className: 'vb-hp__fill', style: { width: pct(hp) } }),
        bar > 0 && h('i', { className: 'vb-hp__barrier', style: { left: pct(hp), width: pct(Math.min(bar, 1 - hp)) } })),
      st != null && h('div', { className: 'vb-hp__stab' }, !p.exposed && h('i', { style: { width: pct(st) } })));
  }

  var ACTIVITY = { aiming: 'Aiming', idle: 'Idle', auto: 'Auto', moving: 'Moving', casting: 'Casting', defeated: 'Defeated' };
  function FighterPortrait(p) {
    var state = p.state || 'default';
    var card = p.variant === 'card';
    var enemy = p.team === 'enemy';
    var act = p.activity;
    var actText = act ? (act.indexOf('queued') === 0 ? act.toUpperCase() : (ACTIVITY[act] || act)) : null;
    var actKind = act ? (act.indexOf('queued') === 0 ? 'queued' : act) : null;
    var btn = h('button', {
      type: 'button', 'aria-pressed': state === 'selected', 'aria-label': (p.name || p.code) + (p.auto ? ', automated' : '') + (state !== 'default' ? ', ' + state : ''),
      onClick: p.onClick,
      className: cx('vb-shape vb-portrait vb-press', card && 'vb-portrait--card', state !== 'default' && 'vb-portrait--' + state, enemy && 'vb-portrait--enemy', state === 'selected' && 'vb-brackets')
    },
      !card && p.slot != null && h('span', { className: 'vb-portrait__slot' }, p.slot),
      !card && p.auto && state !== 'exposed' && h('span', { className: 'vb-portrait__tag' }, 'AUTO'),
      state === 'exposed' && h('span', { className: 'vb-portrait__exposed' }, 'EXPOSED'),
      p.queue && h('span', { className: 'vb-portrait__q' }, p.queue),
      h('span', { className: 'vb-portrait__code' }, p.code),
      card && p.name && h('span', { className: 'vb-portrait__name' }, p.name),
      h(HealthBar, { value: state === 'defeated' ? 0 : (p.health == null ? 100 : p.health), stability: p.stability, barrier: p.barrier, team: enemy ? 'enemy' : 'ally', exposed: state === 'exposed', size: card ? 'hud' : 'world', style: { width: '100%' } }),
      card && actText && h('span', { className: cx('vb-portrait__state', 'vb-portrait__state--' + actKind) }, actText));
    return h('span', { className: cx('vb vb-portrait__wrap', state === 'selected' && 'vb-glow-ally') }, btn,
      p.auto && !card && h('span', { className: 'vb-portrait__dot', 'aria-hidden': 'true' }));
  }

  var ABILITY_LABEL = { aiming: 'Aiming', invalid: 'No target', rejected: 'Rejected' };
  function AbilityButton(p) {
    var s = p.state || 'ready';
    var label = ABILITY_LABEL[s] || p.label;
    var t = p.time ? h('i', null, ' ' + p.time) : null;
    if (s === 'queued') label = p.queueIndex ? 'Queued' : 'Next';
    if (s === 'windup') label = 'Windup';
    if (s === 'short') label = h(F, null, 'In', t);
    if (s === 'invalid' && p.reason) label = p.reason;
    var glyph = s === 'invalid' ? 'no-target' : (s === 'rejected' ? 'warning' : (p.icon || 'fan'));
    var prog = clamp(p.progress == null ? 0.5 : p.progress);
    var btn = h('button', {
      type: 'button', onClick: p.onClick,
      'aria-label': (p.label || '') + (p.cost != null ? ', ' + p.cost + ' Power' : '') + (s !== 'ready' ? ', ' + s + (p.queueIndex ? ' ' + p.queueIndex : '') + (p.time ? ' ' + p.time : '') : ''),
      className: cx('vb-shape vb-ability vb-press', p.size === 'lg' && 'vb-ability--lg', s !== 'ready' && 'vb-ability--' + s, s === 'short' && 'vb-ability--short'),
      style: { '--vb-p': prog }
    },
      p.hotkey && s !== 'queued' && h('span', { className: 'vb-ability__key' }, p.hotkey),
      s === 'queued' && p.queueIndex && h('span', { className: 'vb-ability__q' }, p.queueIndex),
      s === 'windup' && h('i', { className: 'vb-ability__fill' }),
      s === 'cooldown' && h('i', { className: 'vb-ability__shutter' }),
      s === 'cooldown' ? h('span', { className: 'vb-ability__secs' }, p.time || '') : h(Icon, { name: glyph, size: p.size === 'lg' ? 28 : 24 }),
      s !== 'silenced' && h('span', { className: 'vb-ability__label' }, label),
      s === 'windup' ? h('span', { className: 'vb-ability__cost' }, p.time || '') : (p.cost != null && s !== 'silenced' && h('span', { className: 'vb-ability__cost' }, p.cost === 0 ? 'Free' : p.cost)),
      s === 'silenced' && h('span', { className: 'vb-ability__tag' }, 'SILENCED ' + (p.time || '')));
    var glow = s === 'aiming' ? 'vb-glow-ally' : (s === 'rejected' ? 'vb-glow-enemy' : null);
    return h('span', { className: cx('vb vb-ability__wrap', glow) }, btn);
  }

  function QueueChip(p) {
    return h('div', { className: cx('vb vb-shape vb-qchip', p.next && 'vb-qchip--next', p.preview && 'vb-qchip--preview') },
      h('span', { className: cx('vb-qchip__n', p.next && 'vb-pulse') }, p.index),
      p.who && h('span', { className: 'vb-qchip__who' }, p.who),
      h('span', { className: 'vb-qchip__skill' }, p.skill),
      h('span', { className: 'vb-qchip__cost' }, (p.cost != null ? p.cost : '') + (p.eta ? ' · ' + p.eta : '')),
      !p.preview && h('button', { type: 'button', className: 'vb-qchip__x', 'aria-label': 'Cancel ' + (p.skill || 'entry'), onClick: p.onCancel }, h(Icon, { name: 'cancel', size: 16 })));
  }

  function PowerStrip(p) {
    var max = p.max || 100;
    var v = p.value || 0;
    var marks = p.marks || [];
    var last = marks.reduce(function (a, m) { return Math.max(a, m.at); }, v);
    return h('div', { className: cx('vb vb-power', p.desktop && 'vb-power--desktop'), style: p.style },
      h('div', { className: 'vb-power__head' },
        h('span', { className: 'vb-power__label' }, 'POWER'),
        h('span', { className: 'vb-power__val' }, v, h('small', null, '/' + max + (p.regen ? ' · ' + p.regen : ''))),
        p.hint && h('span', { className: 'vb-power__hint' }, p.hint)),
      h('div', { className: 'vb-power__bar', role: 'meter', 'aria-label': 'Power', 'aria-valuenow': v, 'aria-valuemin': 0, 'aria-valuemax': max },
        h('i', { className: 'vb-power__fill', style: { width: pct(clamp((p.preview && p.preview[1] <= v ? p.preview[0] : v) / max)) } }),
        last > v && h('i', { className: 'vb-power__res vb-hatch', style: { left: pct(clamp(v / max)), width: pct(clamp((last - v) / max)) } }),
        p.preview && h('i', { className: 'vb-power__prev', style: { left: pct(clamp(p.preview[0] / max)), width: pct(clamp((p.preview[1] - p.preview[0]) / max)) } }),
        marks.map(function (m, i) { return h('i', { key: i, className: 'vb-power__mark', style: { left: pct(clamp(m.at / max)) } }, h('span', null, m.label != null ? m.label : i + 1)); }),
        p.reserve != null && h('i', { className: 'vb-power__reserve', title: 'Auto reserve ' + p.reserve, style: { left: pct(clamp(p.reserve / max)) } })),
      p.legend && h('div', { className: 'vb-power__legend' },
        h('span', null, h('i', { style: { background: 'var(--power)' } }), 'Available'),
        h('span', null, h('i', { className: 'vb-hatch' }), 'Queued · fires at mark'),
        p.preview && h('span', null, h('i', { style: { border: '1px dashed var(--power-text)' } }), 'Aim preview'),
        p.reserve != null && h('span', null, h('i', { style: { width: 2, background: 'var(--text)' } }), 'Soft reserve ' + p.reserve)),
      p.queue && p.queue.length > 0 && h('div', { className: 'vb-power__queue' }, p.queue.map(function (q, i) {
        return h(QueueChip, Object.assign({ key: i, index: i + 1, next: i === 0 }, q));
      })));
  }

  var STATUS = {
    exposed: ['exposed', 'Exposed'], silenced: ['silenced', 'Silenced'], rooted: ['rooted', 'Rooted'],
    slowed: ['slowed', 'Slowed'], interrupted: ['interrupted', 'Interrupted'], barrier: ['barrier', 'Barrier']
  };
  function StatusChip(p) {
    var s = STATUS[p.kind] || STATUS.exposed;
    return h('span', { className: cx('vb vb-shape vb-status', 'vb-status--' + (p.kind || 'exposed')) },
      h(Icon, { name: s[0], size: 14 }),
      h('span', null, p.text || s[1]),
      p.detail && h('b', null, '· ' + p.detail),
      p.time && h('b', { className: 'vb-num' }, '· ' + p.time));
  }

  function ConnectionPill(p) {
    var ms = p.ms || 0;
    var recon = p.state === 'reconnecting';
    var tone = recon || ms >= 200 ? 'bad' : (ms >= 120 ? 'warn' : 'good');
    var word = recon ? 'Reconnecting' : (tone === 'good' ? 'Synced' : 'Unstable');
    return h('div', { className: cx('vb vb-shape vb-conn', 'vb-conn--' + tone), role: 'status' },
      h(Icon, { name: 'signal', size: 16 }),
      !recon && h('span', { className: 'vb-conn__ms' }, ms + ' ms'),
      p.region && h('span', { className: 'vb-conn__region' }, p.region),
      p.showState !== false && h('span', { className: 'vb-conn__state' }, word));
  }

  function ConnectionBanner(p) {
    return h('div', { className: 'vb vb-shape vb-banner', role: 'status', 'aria-live': 'polite' },
      h('span', { className: 'vb-banner__title vb-pulse' }, p.title || 'Reconnecting'),
      h('span', { className: 'vb-banner__body' }, p.children || 'Resynchronizing until a full authoritative baseline arrives. Inputs are paused; the match continues.'),
      h('i', { className: 'vb-banner__bar' }));
  }

  function MatchHeader(p) {
    var list = p.fighters || [];
    function f(x) {
      return h('div', { key: x.name, className: cx('vb-match__f', x.team === 'ally' && 'vb-match__f--ally', x.defeated && 'vb-match__f--down') },
        h('span', { className: 'vb-match__n' }, x.name),
        h(HealthBar, { value: x.defeated ? 0 : x.health, team: x.team === 'ally' ? 'ally' : 'enemy', size: 'world', style: { width: '100%' } }));
    }
    var clk = h('div', { className: 'vb-match__clock' },
      h('span', { className: 'vb-match__time', role: 'timer' }, p.clock),
      p.phase === 'pressure' ? h('span', { className: 'vb-match__pressure' }, 'Pressure') : (p.sub && h('span', { className: 'vb-match__phase' }, p.sub)));
    if (p.compact) return h('div', { className: 'vb vb-shape vb-match vb-match--compact' }, clk, h('div', { className: 'vb-match__side' }, list.map(f)));
    var half = Math.ceil(list.length / 2);
    return h('div', { className: 'vb vb-shape vb-match' },
      h('div', { className: 'vb-match__side' }, list.slice(0, half).map(f)), clk,
      h('div', { className: 'vb-match__side' }, list.slice(half).map(f)));
  }

  function CombatFeed(p) {
    var rows = p.entries || [];
    function who(name, team) { return h('b', { className: team === 'enemy' ? 'vb-feed__e' : team === 'system' ? 'vb-feed__s' : 'vb-feed__a' }, name); }
    return h('div', { className: 'vb vb-shape vb-feed', role: 'log', 'aria-live': 'polite', style: p.style },
      h('div', { className: 'vb-feed__head' }, p.title || 'Combat feed'),
      rows.map(function (r, i) {
        return h('div', { key: i, className: cx('vb-feed__row', i < rows.length - 4 && 'vb-feed__row--old') },
          h('span', { className: 'vb-feed__t' }, r.t),
          h('span', null, who(r.actor, r.team), ' ' + r.text, r.target ? h(F, null, ' ', who(r.target, r.targetTeam || (r.team === 'enemy' ? 'ally' : 'enemy'))) : null, r.after ? ' ' + r.after : ''));
      }));
  }

  function Minimap(p) {
    var W = 40, H = 62, w = p.width || 124, hh = Math.round(w * H / W);
    var fighters = p.fighters || [], props = p.props || [];
    var lines = [];
    for (var x = 5; x < W; x += 5) lines.push(h('line', { key: 'x' + x, x1: x, y1: 0, x2: x, y2: H }));
    for (var y = 5; y < H; y += 5) lines.push(h('line', { key: 'y' + y, x1: 0, y1: y, x2: W, y2: y }));
    var PROP = {
      intact: { fill: 'var(--raised)', stroke: 'var(--line-strong)' },
      cracked: { fill: 'var(--raised)', stroke: 'var(--prop-hit)' },
      broken: { fill: 'none', stroke: 'var(--line-strong)', strokeDasharray: '1 .8' },
      rubble: { fill: 'none', stroke: 'var(--text-3)', strokeDasharray: '.2 .9' },
      anchor: { fill: 'var(--line-strong)', stroke: 'var(--text-2)' }
    };
    return h('div', { className: 'vb vb-shape vb-minimap vb-scan', role: 'img', 'aria-label': p.label || 'Tactical minimap' },
      h('svg', { width: w, height: hh, viewBox: '0 0 ' + W + ' ' + H },
        h('g', { stroke: 'var(--holo)', strokeOpacity: 0.12, strokeWidth: 0.12 }, lines),
        h('rect', { x: 0.2, y: 0.2, width: W - 0.4, height: H - 0.4, fill: 'none', stroke: 'var(--line-strong)', strokeWidth: 0.4 }),
        p.safe && h('rect', { x: p.safe[0], y: p.safe[1], width: p.safe[2], height: p.safe[3], fill: 'none', stroke: 'var(--danger-zone)', strokeWidth: 0.35, strokeDasharray: '1 .6' }),
        props.map(function (q, i) { return h('rect', Object.assign({ key: 'p' + i, x: q.x, y: q.y, width: q.w, height: q.h, strokeWidth: 0.25 }, PROP[q.state || 'intact'])); }),
        p.view && h('rect', { x: p.view[0], y: p.view[1], width: p.view[2], height: p.view[3], fill: 'var(--ally)', fillOpacity: 0.05, stroke: 'var(--ally)', strokeOpacity: 0.7, strokeWidth: 0.3 }),
        fighters.map(function (f, i) {
          if (f.defeated) return h('path', { key: 'f' + i, d: 'M' + (f.x - 0.8) + ' ' + (f.y - 0.8) + 'L' + (f.x + 0.8) + ' ' + (f.y + 0.8) + 'M' + (f.x + 0.8) + ' ' + (f.y - 0.8) + 'L' + (f.x - 0.8) + ' ' + (f.y + 0.8), stroke: 'var(--text-3)', strokeWidth: 0.35 });
          if (f.team === 'enemy') return h('rect', { key: 'f' + i, x: f.x - 0.95, y: f.y - 0.95, width: 1.9, height: 1.9, transform: 'rotate(45 ' + f.x + ' ' + f.y + ')', fill: 'var(--enemy)' });
          return h(F, { key: 'f' + i },
            h('circle', { cx: f.x, cy: f.y, r: 1.15, fill: 'var(--ally)' }),
            f.selected && h('circle', { cx: f.x, cy: f.y, r: 2.2, fill: 'none', stroke: 'var(--ally)', strokeWidth: 0.3 }));
        })),
      p.caption !== false && h('div', { className: 'vb-minimap__lbl' }, h('span', null, p.caption || 'Arena'), h('span', null, '40 × 62 m')));
  }

  function OffscreenIndicator(p) {
    return h('div', { className: cx('vb vb-shape vb-off', p.team === 'ally' && 'vb-off--ally', p.side === 'left' && 'vb-off--left', p.threat && 'vb-off--threat'), role: 'img', 'aria-label': p.name + ' off screen ' + (p.side || 'right') },
      p.side === 'left' && h(Icon, { name: 'chevron-left', size: 14 }),
      h('i', { className: 'vb-off__mark' }),
      h('span', null, p.name),
      p.distance != null && h('span', { className: 'vb-off__d' }, p.distance + ' m'),
      p.side !== 'left' && h(Icon, { name: 'chevron-right', size: 14 }));
  }

  /* ── world space ────────────────────────────────────────────────────────── */
  function WorldLabel(p) {
    var k = p.kind || 'aim';
    var details = p.detail ? [].concat(p.detail) : [];
    if (k === 'name') {
      return h('div', { className: cx('vb vb-wlabel vb-wlabel--name', p.team && 'vb-wlabel--' + p.team) },
        h('span', { className: 'vb-wlabel__line' }, p.title),
        p.health != null && h(HealthBar, { value: p.health, stability: p.stability, barrier: p.barrier, team: p.team === 'enemy' ? 'enemy' : 'ally', size: 'world', exposed: p.exposed }));
    }
    if (k === 'aim') {
      return h('div', { className: 'vb vb-wlabel vb-wlabel--aim' },
        h('div', { className: 'vb-shape vb-cut-sm vb-wlabel__box' },
          h('div', { className: 'vb-wlabel__line' }, p.title, p.cost != null && h('em', null, ' ' + p.cost + ' POWER')),
          details.map(function (d) { return h('div', { key: d, className: 'vb-wlabel__detail' }, d); })),
        p.stem !== false && h('div', { className: 'vb-wlabel__stem' }));
    }
    return h('div', { className: cx('vb vb-wlabel', 'vb-wlabel--' + k) },
      h('span', { className: 'vb-wlabel__line' }, k === 'blocked' && h(Icon, { name: 'cancel', size: 12 }), p.title || (k === 'blocked' ? 'Blocked' : '')),
      details.map(function (d) { return h('span', { key: d, className: 'vb-wlabel__detail' }, d); }));
  }

  function hatchDef(id, color) {
    return h('pattern', { id: id, width: 6, height: 6, patternUnits: 'userSpaceOnUse', patternTransform: 'rotate(45)' },
      h('rect', { width: 2, height: 6, fill: color }));
  }

  function WorldMarker(p) {
    var k = p.kind || 'ally';
    var id = useUid('vbwm');
    var s = p.size || 80;
    var body;
    if (k === 'enemy') {
      body = h('polygon', { points: '40,16 64,40 40,64 16,40', fill: 'var(--enemy)', fillOpacity: 0.16, stroke: 'var(--enemy)', strokeWidth: 2.5 });
    } else if (k === 'selected') {
      var b = function (x, y, dx, dy) { return 'M' + x + ' ' + (y + dy) + 'V' + y + 'H' + (x + dx); };
      body = h(F, null,
        h('circle', { cx: 40, cy: 40, r: 22, fill: 'var(--ally)', fillOpacity: 0.18, stroke: 'var(--ally)', strokeWidth: 3 }),
        h('circle', { cx: 40, cy: 40, r: 29, fill: 'none', stroke: 'var(--ally)', strokeOpacity: 0.55, strokeWidth: 1 }),
        h('path', { d: b(6, 6, 10, 10) + b(74, 6, -10, 10) + b(6, 74, 10, -10) + b(74, 74, -10, -10), fill: 'none', stroke: 'var(--ally)', strokeWidth: 2.5, strokeLinecap: 'square' }));
    } else if (k === 'queued') {
      body = h(F, null,
        h('defs', null, hatchDef(id, 'var(--power)')),
        h('circle', { cx: 40, cy: 40, r: 27, fill: 'none', stroke: 'url(#' + id + ')', strokeWidth: 8 }),
        h('circle', { cx: 40, cy: 40, r: 20, fill: 'var(--ally)', fillOpacity: 0.16, stroke: 'var(--ally)', strokeWidth: 2.5 }));
    } else if (k === 'move') {
      return h('svg', { className: 'vb-wmark', width: s * 1.6, height: s, viewBox: '0 0 128 80', role: 'img', 'aria-label': 'Move order' },
        h('circle', { cx: 16, cy: 62, r: 9, fill: 'var(--ally)', fillOpacity: 0.16, stroke: 'var(--ally)', strokeWidth: 2 }),
        h('path', { d: 'M26 58C52 48 70 46 104 22', fill: 'none', stroke: 'var(--ally)', strokeWidth: 2, strokeDasharray: '5 5', strokeOpacity: 0.85 }),
        h('circle', { cx: 108, cy: 19, r: 9, fill: 'none', stroke: 'var(--ally)', strokeWidth: 2, strokeDasharray: '4 3' }),
        h('path', { d: 'M104 19H112M108 15V23', stroke: 'var(--ally)', strokeWidth: 1.5 }));
    } else {
      body = h('circle', { cx: 40, cy: 40, r: 22, fill: 'var(--ally)', fillOpacity: 0.16, stroke: 'var(--ally)', strokeWidth: 2.5 });
    }
    return h('svg', { className: 'vb-wmark', width: s, height: s, viewBox: '0 0 80 80', role: 'img', 'aria-label': k + ' marker' }, body);
  }

  function teleShape(shape) {
    var cx_ = 30, cy = 70;
    switch (shape) {
      case 'projectile': return [h('rect', { key: 1, x: 40, y: 66, width: 150, height: 8 }), h('circle', { key: 2, cx: 196, cy: 70, r: 6 })];
      case 'fan': return [-12, 0, 12].map(function (a) { return h('rect', { key: a, x: 40, y: 67, width: 160, height: 6, transform: 'rotate(' + a + ' ' + cx_ + ' ' + cy + ')' }); });
      case 'cone': return [h('path', { key: 1, d: 'M30 70L127.4 7A120 120 0 0 1 127.4 133Z' })];
      case 'burst': return [h('circle', { key: 1, cx: 150, cy: 70, r: 44 })];
      case 'zone': return [h('circle', { key: 1, cx: 145, cy: 70, r: 52 }), h('circle', { key: 2, cx: 145, cy: 70, r: 22, fillOpacity: 0 })];
      case 'beam': return [h('rect', { key: 1, x: 40, y: 57, width: 170, height: 26 })];
      case 'dash': return [h('path', { key: 1, d: 'M40 66H130V58L150 70L130 82V74H40Z' }), h('circle', { key: 2, cx: 170, cy: 70, r: 14, fillOpacity: 0 })];
      case 'wall': return [h('rect', { key: 1, x: 140, y: 20, width: 10, height: 100 })];
      default: return [];
    }
  }

  function Telegraph(p) {
    var shape = p.shape || 'fan';
    var enemy = p.team === 'enemy';
    var col = enemy ? 'var(--enemy)' : 'var(--ally)';
    var id = useUid('vbtg');
    var predicted = p.state === 'predicted';
    var prog = p.progress == null ? null : clamp(p.progress);
    var w = p.width || 220;
    var shapes = teleShape(shape);
    var svg = h('svg', { className: 'vb-tele', width: w, height: w * 140 / 220, viewBox: '0 0 220 140', role: 'img', 'aria-label': (enemy ? 'Enemy ' : 'Ally ') + shape + ' telegraph' },
      h('defs', null,
        hatchDef(id + 'h', col),
        prog != null && h('clipPath', { id: id + 'c' }, h('rect', { x: 0, y: 0, width: 30 + prog * 190, height: 140 }))),
      (shape === 'burst' || shape === 'zone') && h('path', { d: 'M38 70H' + (shape === 'burst' ? 106 : 93), stroke: col, strokeWidth: 1.5, strokeDasharray: '4 4', strokeOpacity: 0.7 }),
      shape === 'wall' && h('path', { d: 'M215 40L160 52M215 100L160 88', stroke: 'var(--enemy-text)', strokeWidth: 1.5, strokeDasharray: '4 4' }),
      h('g', {
        fill: enemy ? 'url(#' + id + 'h)' : col, fillOpacity: enemy ? 0.9 : 0.2, stroke: col, strokeWidth: 2.5, strokeLinejoin: 'miter',
        strokeDasharray: predicted ? '7 5' : undefined, opacity: predicted ? 0.6 : 1
      }, shapes),
      prog != null && h('g', { clipPath: 'url(#' + id + 'c)', fill: col, fillOpacity: 0.38, stroke: 'none' }, teleShape(shape)),
      h('circle', { cx: 30, cy: 70, r: 9, fill: 'var(--ground)', stroke: enemy ? 'var(--enemy)' : 'var(--ally)', strokeWidth: 2.5 }),
      enemy && h('rect', { x: 25, y: 65, width: 10, height: 10, transform: 'rotate(45 30 70)', fill: 'var(--enemy)' }));
    return p.iso ? h('div', { className: 'vb-ground-wrap' }, h('div', { className: 'vb-ground' }, svg)) : svg;
  }

  function CombatText(p) {
    var k = p.kind || 'damage';
    var v = p.value;
    var main = k === 'miss' ? 'MISS' : k === 'heal' ? '+' + v : k === 'power' ? '+' + v : k === 'taken' ? '−' + v : String(v);
    var tag = k === 'exposed' ? 'EXPOSED' : k === 'barrier' ? 'BLOCKED' : k === 'power' ? 'POWER' : null;
    return h('span', { className: cx('vb vb-ctext', 'vb-ctext--' + k, p.animate && 'vb-ctext--anim'), role: 'status' }, main, tag && h('small', null, tag));
  }

  function HoloPlatform(p) {
    var st = p.state || 'occupied';
    var view = h('div', { className: 'vb-plat__view' },
      st !== 'open' && h('div', { className: 'vb-plat__beam' }),
      h('div', { className: 'vb-plat__disc' }),
      st === 'open' ? h('div', { className: 'vb-plat__plus' }, h(Icon, { name: 'plus', size: 20 })) : (p.marker !== false && h('div', { className: 'vb-plat__pawn' }, h('i'))));
    return h('div', { className: cx('vb vb-plat', 'vb-plat--' + st, p.team === 'enemy' && 'vb-plat--enemy'), onClick: p.onClick },
      st === 'selected' ? h('span', { className: 'vb-lock', style: { '--vb-off': '-2px' } }, view) : view,
      h('div', { className: 'vb-plat__label' }, st === 'open' ? 'Open slot' : p.label, (p.sub || st === 'open') && h('small', null, p.sub || 'weight shared')));
  }

  /* ── meta ───────────────────────────────────────────────────────────────── */
  var TIER = { blue: ['R', 'Rare'], violet: ['E', 'Epic'], gold: ['L', 'Legendary'] };
  function TeamTile(p) {
    var st = p.state || 'available';
    var t = TIER[p.tier];
    var btn = h('button', {
      type: 'button', onClick: p.onClick, 'aria-pressed': st === 'in' || st === 'selected', 'aria-disabled': st === 'locked' ? 'true' : undefined,
      className: cx('vb-shape vb-tile vb-press', st === 'in' && 'vb-tile--in', st === 'selected' && 'vb-tile--selected vb-brackets', st === 'locked' && 'vb-tile--locked')
    },
      t && h('span', { className: 'vb-tile__tier', style: { color: 'var(--tier-' + p.tier + ')' }, title: t[1] }, t[0]),
      h('span', { className: 'vb-tile__code' }, st === 'locked' ? h(Icon, { name: 'lock', size: 20 }) : p.code),
      h('span', { className: 'vb-tile__name' }, p.name),
      p.role && h('span', { className: 'vb-tile__role' }, p.role),
      (st === 'in' || st === 'selected') && h('span', { className: 'vb-tile__state' }, 'In team'),
      st === 'locked' && h('span', { className: 'vb-tile__state' }, 'Pull in rifts'));
    return h('span', { className: cx('vb', st === 'selected' && 'vb-glow-ally'), style: { display: 'inline-block' } }, btn);
  }

  var RARITY = { normal: 'N', magic: 'M', rare: 'R', set: 'S', unique: 'U' };
  function ItemCell(p) {
    var st = p.state || 'default';
    var slot = p.slot || 'core';
    var cell;
    if (st === 'empty') {
      cell = h('button', { type: 'button', className: 'vb-shape vb-item vb-item--empty vb-press', 'aria-label': 'Empty ' + slot + ' slot', onClick: p.onClick }, h(Icon, { name: 'plus', size: 18 }));
    } else {
      cell = h('button', {
        type: 'button', onClick: p.onClick, 'aria-label': (p.name || slot) + ', ' + (p.rarity || 'normal') + (p.upgrade ? ' +' + p.upgrade : ''),
        className: cx('vb-shape vb-item vb-press', 'vb-item--' + (p.rarity || 'normal'), st === 'selected' && 'vb-item--selected vb-brackets', st === 'locked' && 'vb-item--locked')
      },
        h(Icon, { name: slot, size: 22 }),
        h('span', { className: 'vb-item__r' }, RARITY[p.rarity || 'normal']),
        p.upgrade ? h('span', { className: 'vb-item__up' }, '+' + p.upgrade) : null,
        st === 'locked' && h('span', { className: 'vb-item__lock' }, h(Icon, { name: 'lock', size: 16 })));
    }
    var wrapped = st === 'selected' ? h('span', { className: 'vb-glow-ally', style: { display: 'inline-block' } }, cell) : cell;
    if (!p.showSlot) return h('span', { className: 'vb', style: { display: 'inline-block' } }, wrapped);
    return h('span', { className: 'vb', style: { display: 'inline-flex', flexDirection: 'column', alignItems: 'center' } }, wrapped, h('span', { className: 'vb-item__slot' }, slot));
  }

  function StatReadout(p) {
    var d = p.delta;
    var c0 = typeof d === 'string' ? d.charAt(0) : '';
    var dir = p.deltaTone || (c0 === '+' ? 'up' : (c0 === '-' || c0 === '−') ? 'down' : null);
    return h('div', { className: 'vb vb-stat' },
      h('span', { className: 'vb-stat__label' }, p.label),
      h('span', { className: 'vb-stat__value' }, fmt(p.value), p.unit && h('small', null, p.unit)),
      d && h('span', { className: cx('vb-stat__delta', dir && 'vb-stat__delta--' + dir) }, d),
      p.bar != null && h('div', { className: cx('vb-stat__bar', p.barTone && 'vb-stat__bar--' + p.barTone) }, h('i', { style: { width: pct(clamp(p.bar)) } })));
  }

  function RevealCard(p) {
    var tier = p.tier || 'blue';
    var st = R.useState(p.flipped !== false);
    var flipped = p.onFlip ? p.flipped !== false : st[0];
    var t = TIER[tier];
    var label = p.tierLabel || (p.kind === 'item' ? ({ blue: 'Magic', violet: 'Rare', gold: 'Unique' }[tier]) + ' item' : t[1]);
    var card = h('div', {
      className: cx('vb vb-reveal', 'vb-reveal--' + tier, flipped && 'vb-reveal--flipped'), role: 'button', tabIndex: 0, 'aria-pressed': flipped,
      'aria-label': flipped ? label + ' ' + p.name : 'Hidden card, tap to reveal',
      onClick: function () { if (p.onFlip) p.onFlip(!flipped); else st[1](!flipped); }
    },
      h('div', { className: 'vb-reveal__inner' },
        h('div', { className: 'vb-shape vb-reveal__face vb-reveal__back vb-scan' }, h(Icon, { name: 'rift', size: 44 })),
        h('div', { className: 'vb-shape vb-reveal__face vb-reveal__front' },
          h('span', { className: 'vb-reveal__tier' }, label),
          h('span', { className: 'vb-reveal__code' }, p.kind === 'item' ? h(Icon, { name: p.slot || 'core', size: 44, style: { color: 'var(--tier-' + tier + ')' } }) : p.code),
          h('span', { className: cx('vb-reveal__name', tier === 'gold' && flipped && 'vb-chroma') }, p.name),
          p.role && h('span', { className: 'vb-reveal__role' }, p.role),
          p.dupe && h('span', { className: 'vb-reveal__dupe' }, p.dupe),
          p.isNew && h('span', { className: 'vb-reveal__new' }, 'NEW'))));
    return h('span', { className: cx(flipped && tier === 'gold' && 'vb-glow-gold'), style: { display: 'inline-block' } }, card);
  }

  function RuleRow(p) {
    var st = R.useState(p.enabled !== false);
    var on = st[0];
    var conds = p.conditions || [];
    var a = p.action || {};
    var ifParts = [h('span', { key: 'if', className: 'vb-rule__kw' }, 'IF')];
    conds.forEach(function (c, i) {
      if (i > 0) ifParts.push(h('span', { key: 'k' + i, className: 'vb-rule__kw' }, p.join || 'AND'));
      ifParts.push(h('span', { key: 'c' + i, className: 'vb-rule__cond' }, c.text, c.value != null && h(F, null, ' ', h('b', null, c.value)), c.after ? ' ' + c.after : ''));
    });
    return h('div', { className: cx('vb vb-shape vb-rule', !on && 'vb-rule--off', p.warning && 'vb-rule--warn', p.draft && 'vb-rule--draft') },
      h('span', { className: 'vb-rule__grip', 'aria-hidden': 'true' }),
      h('span', { className: 'vb-rule__n' }, p.index),
      h('div', { className: 'vb-rule__if' }, ifParts),
      h('div', { className: 'vb-rule__then' },
        h('span', { className: 'vb-rule__kw' }, 'THEN'),
        h('span', { className: 'vb-rule__act' }, h('b', null, a.title), a.rest ? ' ' + a.rest : ''),
        a.cost && h('span', { className: 'vb-rule__cost' }, a.cost)),
      h('button', { type: 'button', role: 'checkbox', 'aria-checked': on ? 'true' : 'false', 'aria-label': 'Rule ' + p.index + ' enabled', className: 'vb-rule__check', onClick: function () { st[1](!on); } },
        on && h(Icon, { name: 'check', size: 14 })),
      (p.warning || p.note) && h('div', { className: 'vb-rule__note' }, p.warning || p.note));
  }

  function Timeline(p) {
    var dur = p.duration || 120;
    var pos = p.position || 0;
    var events = p.events || [];
    var sp = R.useState(p.speed || 1);
    var pl = R.useState(!!p.playing);
    var speeds = [0.25, 0.5, 1, 2];
    return h('div', { className: 'vb vb-shape vb-tl' },
      h('div', { className: 'vb-tl__track', role: 'slider', tabIndex: 0, 'aria-label': 'Replay position', 'aria-valuemin': 0, 'aria-valuemax': dur, 'aria-valuenow': pos, 'aria-valuetext': clock(pos, true) },
        h('i', { className: 'vb-tl__line' }),
        h('i', { className: 'vb-tl__played', style: { width: pct(clamp(pos / dur)) } }),
        p.pressureFrom != null && h('i', { className: 'vb-tl__pressure', style: { left: pct(clamp(p.pressureFrom / dur)) } }),
        events.map(function (e, i) { return h('i', { key: i, title: e.label, className: cx('vb-tl__ev', e.team === 'enemy' && 'vb-tl__ev--enemy', e.team === 'phase' && 'vb-tl__ev--phase'), style: { left: pct(clamp(e.t / dur)) } }); }),
        h('i', { className: 'vb-tl__head', style: { left: pct(clamp(pos / dur)) } })),
      h('div', { className: 'vb-tl__bar' },
        h('button', { type: 'button', className: 'vb-shape vb-tl__btn', 'aria-label': 'Previous event' }, h(Icon, { name: 'prev', size: 16 })),
        h('button', { type: 'button', className: cx('vb-shape vb-tl__btn', 'vb-tl__btn--on'), 'aria-label': pl[0] ? 'Pause' : 'Play', onClick: function () { pl[1](!pl[0]); } }, h(Icon, { name: pl[0] ? 'pause' : 'play', size: 16 })),
        h('button', { type: 'button', className: 'vb-shape vb-tl__btn', 'aria-label': 'Next event' }, h(Icon, { name: 'next', size: 16 })),
        h('span', { className: 'vb-tl__time' }, clock(pos, true), h('small', null, ' / ' + clock(dur))),
        h('div', { className: 'vb-tl__speeds', role: 'group', 'aria-label': 'Speed' }, speeds.map(function (s) {
          return h('button', { key: s, type: 'button', className: 'vb-tl__speed', 'aria-pressed': s === sp[0] ? 'true' : 'false', onClick: function () { sp[1](s); } }, s + '×');
        }))));
  }

  var api = {
    Icon: Icon, Panel: Panel, HoloFrame: HoloFrame, Button: Button, RailButton: RailButton, Switch: Switch, Segmented: Segmented, NavBar: NavBar,
    MatchHeader: MatchHeader, FighterPortrait: FighterPortrait, AbilityButton: AbilityButton, PowerStrip: PowerStrip, QueueChip: QueueChip,
    HealthBar: HealthBar, StatusChip: StatusChip, ConnectionPill: ConnectionPill, ConnectionBanner: ConnectionBanner, CombatFeed: CombatFeed,
    Minimap: Minimap, OffscreenIndicator: OffscreenIndicator, WorldLabel: WorldLabel, WorldMarker: WorldMarker, Telegraph: Telegraph,
    CombatText: CombatText, HoloPlatform: HoloPlatform, TeamTile: TeamTile, ItemCell: ItemCell, StatReadout: StatReadout,
    RevealCard: RevealCard, RuleRow: RuleRow, Timeline: Timeline
  };
  window.Veilbreak = Object.assign(window.Veilbreak || {}, api);
})();
