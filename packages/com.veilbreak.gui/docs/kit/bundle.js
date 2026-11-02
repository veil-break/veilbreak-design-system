/* @ds-bundle: {"format":4,"namespace":"Veilbreak","components":[{"name":"EffectDetails"},{"name":"EffectBadge"},{"name":"EffectList"},{"name":"EffectStrip"},{"name":"ActivePerksView"},{"name":"RevivePickView"},{"name":"AccessibilityView"},{"name":"GlitchText"},{"name":"CreditsChip"},{"name":"RouteChoice"},{"name":"MerchantScreen"},{"name":"VbFrame"},{"name":"VbIcon"},{"name":"VbMark"},{"name":"VbButton"},{"name":"StatReadout"},{"name":"StatusChip"},{"name":"VbSwitch"},{"name":"VbSlider"},{"name":"VbSegmented"},{"name":"VbMeter"},{"name":"HealthBar"},{"name":"PowerStrip"},{"name":"FocusMeter"},{"name":"SkillButton"},{"name":"QueueChip"},{"name":"HudTop"},{"name":"QueueList"},{"name":"BossBar"},{"name":"ScreenHeader"},{"name":"ScoreRow"},{"name":"OperativeCard"},{"name":"OperativeTile"},{"name":"SquadSlot"},{"name":"OperativeDetails"},{"name":"OperativeStats"},{"name":"SquadResult"},{"name":"PerkCard"},{"name":"PerkDetail"},{"name":"PerkChip"},{"name":"PerkRow"},{"name":"Dialog"},{"name":"Backdrop"},{"name":"ScanLines"},{"name":"HoloFrame"},{"name":"HoloPlatform"},{"name":"RevealCard"},{"name":"RuleRow"},{"name":"WorldHealthBar"},{"name":"CombatText"},{"name":"WorldLabel"},{"name":"WorldMarker"},{"name":"OffscreenIndicator"},{"name":"EdgeWarning"},{"name":"ProgressBar"},{"name":"ListRow"},{"name":"KeyValueRow"},{"name":"TextComponent"},{"name":"LabelComponent"},{"name":"BadgeComponent"},{"name":"IconButtonComposite"},{"name":"TileComponent"},{"name":"CardComponent"},{"name":"DrawerOverlay"},{"name":"FieldComposite"},{"name":"Fieldset"},{"name":"FormComposite"},{"name":"StackLayout"},{"name":"VbGrid"},{"name":"ListLayout"},{"name":"BoardLayout"},{"name":"MainMenuView"},{"name":"MenuActionsSectionView"},{"name":"SquadView"},{"name":"SettingsView"},{"name":"PauseOverlayView"},{"name":"ConfirmDialogView"},{"name":"GuideDialogView"},{"name":"PerkDraftView"},{"name":"ResultsView"},{"name":"HighScoresView"},{"name":"RouteChoiceView"},{"name":"MerchantScreenView"},{"name":"BattleHudView"},{"name":"ConnectionPill"},{"name":"ConnectionBanner"},{"name":"CombatFeed"},{"name":"Minimap"},{"name":"NavBar"},{"name":"MatchHeader"},{"name":"ItemCell"},{"name":"RailButton"},{"name":"Timeline"},{"name":"FighterPortrait"},{"name":"Telegraph"}]} */
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
      className: cx('vbl-icon', p.className), width: size, height: size, viewBox: '0 0 24 24', style: p.style,
      role: p.title ? 'img' : undefined, 'aria-hidden': p.title ? undefined : 'true'
    }, p.title ? h('title', null, p.title) : null, h('path', { d: d }));
  }
  Icon.names = Object.keys(ICONS);

  /* ── surfaces ───────────────────────────────────────────────────────────── */
  function Panel(p) {
    var v = p.variant || 'hud';
    return h('section', {
      className: cx('vbl vbl-shape vbl-panel', v !== 'hud' && 'vbl-panel--' + v, 'vbl-cut-' + (p.cut || 'md'), p.desktop && 'vbl-panel--desktop',
        p.brackets && 'vbl-brackets', p.scan && 'vbl-scan', p.className),
      style: p.style, 'aria-label': p.title || undefined
    },
      (p.title || p.meta) && h('header', { className: 'vbl-panel__head' },
        p.title && h('h3', { className: 'vbl-panel__title', style: { margin: 0 } }, p.title),
        p.meta && h('span', { className: 'vbl-panel__meta' }, p.meta)),
      p.children);
  }

  function HoloFrame(p) {
    var ref = R.useRef(null);
    var pose = p.pose || [0, 0];
    function set(rx, ry) {
      var el = ref.current; if (!el) return;
      el.style.setProperty('--vbl-rx', rx.toFixed(2) + 'deg');
      el.style.setProperty('--vbl-ry', ry.toFixed(2) + 'deg');
    }
    R.useEffect(function () { set(pose[0], pose[1]); }, [pose[0], pose[1]]);
    function move(e) {
      if (!p.tilt) return;
      var el = ref.current, r = el.getBoundingClientRect();
      var max = parseFloat(getComputedStyle(el).getPropertyValue('--holo-tilt')) || 6;
      var x = (e.clientX - r.left) / r.width - 0.5, y = (e.clientY - r.top) / r.height - 0.5;
      set(-y * 2 * max, x * 2 * max);
    }
    return h('div', { className: 'vbl vbl-holo', onPointerMove: move, onPointerLeave: function () { set(pose[0], pose[1]); }, style: p.style },
      h('div', {
        ref: ref, style: { width: p.width },
        className: cx('vbl-shape vbl-cut-' + (p.cut || 'lg'), 'vbl-holo__card vbl-brackets vbl-scan', p.animate !== false && 'vbl-flicker-in', p.className)
      },
        p.title && h('div', { className: 'vbl-holo__title vbl-chroma' }, p.title),
        p.children));
  }

  /* ── actions ────────────────────────────────────────────────────────────── */
  function Button(p) {
    var v = p.variant || 'secondary';
    var blocked = p.blocked;
    var btn = h('button', {
      type: 'button',
      className: cx('vbl vbl-shape vbl-btn vbl-press', 'vbl-btn--' + v, p.size === 'lg' && 'vbl-btn--lg', p.icon && 'vbl-btn--icon', p.center && 'vbl-btn--center', p.className),
      'aria-disabled': blocked ? 'true' : undefined, onClick: blocked ? undefined : p.onClick, style: p.style
    },
      p.icon && h(Icon, { name: p.icon, size: 18 }),
      h('span', { className: 'vbl-btn__label' }, p.children),
      p.sub && !p.icon && h('span', { className: 'vbl-btn__sub' }, p.sub));
    if (blocked) return h('span', { className: 'vbl', style: { display: 'inline-flex', flexDirection: 'column' } }, btn, h('span', { className: 'vbl-btn__reason' }, blocked));
    if (v === 'cta') return h('span', { className: 'vbl-glow-cta', style: { display: 'inline-block' } }, btn);
    return btn;
  }

  function RailButton(p) {
    return h('span', { className: 'vbl vbl-rail__wrap' },
      h('button', {
        type: 'button', className: cx('vbl-shape vbl-rail vbl-press', p.active && 'vbl-rail--active', p.desktop && 'vbl-rail--desktop'),
        'aria-current': p.active ? 'page' : undefined, 'aria-label': p.label ? undefined : (p.ariaLabel || p.icon), onClick: p.onClick
      },
        h(Icon, { name: p.icon || 'menu', size: 20 }),
        p.label && h('span', null, p.label)),
      p.isNew && h('span', { className: 'vbl-rail__new', role: 'img', 'aria-label': 'New' }));
  }

  function Switch(p) {
    var st = R.useState(!!p.checked);
    var on = p.onChange ? !!p.checked : st[0];
    function toggle() { if (p.onChange) p.onChange(!on); else st[1](!on); }
    return h('button', { type: 'button', role: 'switch', 'aria-checked': on ? 'true' : 'false', className: 'vbl vbl-switch', onClick: toggle },
      h('span', { className: 'vbl-switch__track' }, h('span', { className: 'vbl-switch__knob' })),
      (p.label || p.sub) && h('span', null,
        p.label && h('span', { className: 'vbl-switch__label' }, p.label),
        p.sub && h('span', { className: 'vbl-switch__sub' }, p.sub)));
  }

  function Segmented(p) {
    var opts = p.options || [];
    var st = R.useState(p.value);
    var val = p.onChange ? p.value : st[0];
    return h('div', { role: 'radiogroup', 'aria-label': p.label, className: 'vbl vbl-shape vbl-seg vbl-cut-sm', style: p.style },
      opts.map(function (o) {
        var id = o.value != null ? o.value : o.label;
        return h('button', {
          key: id, type: 'button', role: 'radio', 'aria-checked': id === val ? 'true' : 'false', className: 'vbl-seg__opt',
          onClick: function () { if (p.onChange) p.onChange(id); else st[1](id); }
        }, h('span', { className: 'vbl-seg__label' }, o.label), o.sub && h('span', { className: 'vbl-seg__sub' }, o.sub));
      }));
  }

  function NavBar(p) {
    var items = p.items || ['Play', 'Team', 'Automation', 'Studio', 'Training', 'Replays', 'Settings'];
    return h('nav', { className: 'vbl vbl-nav', 'aria-label': 'Main' },
      h('span', { className: 'vbl-nav__brand' }, 'VEIL', h('b', null, '/'), 'BREAK'),
      h('div', { className: 'vbl-nav__items' }, items.map(function (it) {
        return h('button', { key: it, type: 'button', className: 'vbl-nav__item', 'aria-current': it === p.active ? 'page' : undefined, onClick: p.onNavigate && function () { p.onNavigate(it); } }, it);
      })),
      p.rating != null && h('span', { className: 'vbl-nav__meta' }, 'Rating', h('b', null, fmt(p.rating))));
  }

  /* ── battle HUD ─────────────────────────────────────────────────────────── */
  function HealthBar(p) {
    var max = p.max || 100;
    var hp = clamp(p.value / max);
    var bar = p.barrier ? clamp(p.barrier / max) : 0;
    var st = p.stability == null ? null : clamp(p.stability / 100);
    return h('div', {
      className: cx('vbl-hp', p.team === 'enemy' && 'vbl-hp--enemy', p.size && p.size !== 'hud' && 'vbl-hp--' + p.size, p.exposed && 'vbl-hp--exposed'),
      role: 'meter', 'aria-label': p.label || 'Health', 'aria-valuenow': p.value, 'aria-valuemin': 0, 'aria-valuemax': max, style: p.style
    },
      h('div', { className: 'vbl-hp__track' },
        p.ghost != null && h('i', { className: 'vbl-hp__ghost', style: { left: 0, width: pct(clamp(p.ghost / max)) } }),
        h('i', { className: 'vbl-hp__fill', style: { width: pct(hp) } }),
        bar > 0 && h('i', { className: 'vbl-hp__barrier', style: { left: pct(hp), width: pct(Math.min(bar, 1 - hp)) } })),
      st != null && h('div', { className: 'vbl-hp__stab' }, !p.exposed && h('i', { style: { width: pct(st) } })));
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
      className: cx('vbl-shape vbl-portrait vbl-press', card && 'vbl-portrait--card', state !== 'default' && 'vbl-portrait--' + state, enemy && 'vbl-portrait--enemy', state === 'selected' && 'vbl-brackets')
    },
      !card && p.slot != null && h('span', { className: 'vbl-portrait__slot' }, p.slot),
      !card && p.auto && state !== 'exposed' && h('span', { className: 'vbl-portrait__tag' }, 'AUTO'),
      state === 'exposed' && h('span', { className: 'vbl-portrait__exposed' }, 'EXPOSED'),
      p.queue && h('span', { className: 'vbl-portrait__q' }, p.queue),
      h('span', { className: 'vbl-portrait__code' }, p.code),
      card && p.name && h('span', { className: 'vbl-portrait__name' }, p.name),
      h(HealthBar, { value: state === 'defeated' ? 0 : (p.health == null ? 100 : p.health), stability: p.stability, barrier: p.barrier, team: enemy ? 'enemy' : 'ally', exposed: state === 'exposed', size: card ? 'hud' : 'world', style: { width: '100%' } }),
      card && actText && h('span', { className: cx('vbl-portrait__state', 'vbl-portrait__state--' + actKind) }, actText));
    return h('span', { className: cx('vbl vbl-portrait__wrap', state === 'selected' && 'vbl-glow-ally') }, btn,
      p.auto && !card && h('span', { className: 'vbl-portrait__dot', 'aria-hidden': 'true' }));
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
      className: cx('vbl-shape vbl-ability vbl-press', p.size === 'lg' && 'vbl-ability--lg', s !== 'ready' && 'vbl-ability--' + s, s === 'short' && 'vbl-ability--short'),
      style: { '--vbl-p': prog }
    },
      p.hotkey && s !== 'queued' && h('span', { className: 'vbl-ability__key' }, p.hotkey),
      s === 'queued' && p.queueIndex && h('span', { className: 'vbl-ability__q' }, p.queueIndex),
      s === 'windup' && h('i', { className: 'vbl-ability__fill' }),
      s === 'cooldown' && h('i', { className: 'vbl-ability__shutter' }),
      s === 'cooldown' ? h('span', { className: 'vbl-ability__secs' }, p.time || '') : h(Icon, { name: glyph, size: p.size === 'lg' ? 28 : 24 }),
      s !== 'silenced' && h('span', { className: 'vbl-ability__label' }, label),
      s === 'windup' ? h('span', { className: 'vbl-ability__cost' }, p.time || '') : (p.cost != null && s !== 'silenced' && h('span', { className: 'vbl-ability__cost' }, p.cost === 0 ? 'Free' : p.cost)),
      s === 'silenced' && h('span', { className: 'vbl-ability__tag' }, 'SILENCED ' + (p.time || '')));
    var glow = s === 'aiming' ? 'vbl-glow-ally' : (s === 'rejected' ? 'vbl-glow-enemy' : null);
    return h('span', { className: cx('vbl vbl-ability__wrap', glow) }, btn);
  }

  function QueueChip(p) {
    return h('div', { className: cx('vbl vbl-shape vbl-qchip', p.next && 'vbl-qchip--next', p.preview && 'vbl-qchip--preview') },
      h('span', { className: cx('vbl-qchip__n', p.next && 'vbl-pulse') }, p.index),
      p.who && h('span', { className: 'vbl-qchip__who' }, p.who),
      h('span', { className: 'vbl-qchip__skill' }, p.skill),
      h('span', { className: 'vbl-qchip__cost' }, (p.cost != null ? p.cost : '') + (p.eta ? ' · ' + p.eta : '')),
      !p.preview && h('button', { type: 'button', className: 'vbl-qchip__x', 'aria-label': 'Cancel ' + (p.skill || 'entry'), onClick: p.onCancel }, h(Icon, { name: 'cancel', size: 16 })));
  }

  function PowerStrip(p) {
    var max = p.max || 100;
    var v = p.value || 0;
    var marks = p.marks || [];
    var last = marks.reduce(function (a, m) { return Math.max(a, m.at); }, v);
    return h('div', { className: cx('vbl vbl-power', p.desktop && 'vbl-power--desktop'), style: p.style },
      h('div', { className: 'vbl-power__head' },
        h('span', { className: 'vbl-power__label' }, 'POWER'),
        h('span', { className: 'vbl-power__val' }, v, h('small', null, '/' + max + (p.regen ? ' · ' + p.regen : ''))),
        p.hint && h('span', { className: 'vbl-power__hint' }, p.hint)),
      h('div', { className: 'vbl-power__bar', role: 'meter', 'aria-label': 'Power', 'aria-valuenow': v, 'aria-valuemin': 0, 'aria-valuemax': max },
        h('i', { className: 'vbl-power__fill', style: { width: pct(clamp((p.preview && p.preview[1] <= v ? p.preview[0] : v) / max)) } }),
        last > v && h('i', { className: 'vbl-power__res vbl-hatch', style: { left: pct(clamp(v / max)), width: pct(clamp((last - v) / max)) } }),
        p.preview && h('i', { className: 'vbl-power__prev', style: { left: pct(clamp(p.preview[0] / max)), width: pct(clamp((p.preview[1] - p.preview[0]) / max)) } }),
        marks.map(function (m, i) { return h('i', { key: i, className: 'vbl-power__mark', style: { left: pct(clamp(m.at / max)) } }, h('span', null, m.label != null ? m.label : i + 1)); }),
        p.reserve != null && h('i', { className: 'vbl-power__reserve', title: 'Auto reserve ' + p.reserve, style: { left: pct(clamp(p.reserve / max)) } })),
      p.legend && h('div', { className: 'vbl-power__legend' },
        h('span', null, h('i', { style: { background: 'var(--power)' } }), 'Available'),
        h('span', null, h('i', { className: 'vbl-hatch' }), 'Queued · fires at mark'),
        p.preview && h('span', null, h('i', { style: { border: '1px dashed var(--power-text)' } }), 'Aim preview'),
        p.reserve != null && h('span', null, h('i', { style: { width: 2, background: 'var(--text)' } }), 'Soft reserve ' + p.reserve)),
      p.queue && p.queue.length > 0 && h('div', { className: 'vbl-power__queue' }, p.queue.map(function (q, i) {
        return h(QueueChip, Object.assign({ key: i, index: i + 1, next: i === 0 }, q));
      })));
  }

  var STATUS = {
    exposed: ['exposed', 'Exposed'], silenced: ['silenced', 'Silenced'], rooted: ['rooted', 'Rooted'],
    slowed: ['slowed', 'Slowed'], interrupted: ['interrupted', 'Interrupted'], barrier: ['barrier', 'Barrier']
  };
  function StatusChip(p) {
    var s = STATUS[p.kind] || STATUS.exposed;
    return h('span', { className: cx('vbl vbl-shape vbl-status', 'vbl-status--' + (p.kind || 'exposed')) },
      h(Icon, { name: s[0], size: 14 }),
      h('span', null, p.text || s[1]),
      p.detail && h('b', null, '· ' + p.detail),
      p.time && h('b', { className: 'vbl-num' }, '· ' + p.time));
  }

  function ConnectionPill(p) {
    var ms = p.ms || 0;
    var recon = p.state === 'reconnecting';
    var tone = recon || ms >= 200 ? 'bad' : (ms >= 120 ? 'warn' : 'good');
    var word = recon ? 'Reconnecting' : (tone === 'good' ? 'Synced' : 'Unstable');
    return h('div', { className: cx('vbl vbl-shape vbl-conn', 'vbl-conn--' + tone), role: 'status' },
      h(Icon, { name: 'signal', size: 16 }),
      !recon && h('span', { className: 'vbl-conn__ms' }, ms + ' ms'),
      p.region && h('span', { className: 'vbl-conn__region' }, p.region),
      p.showState !== false && h('span', { className: 'vbl-conn__state' }, word));
  }

  function ConnectionBanner(p) {
    return h('div', { className: 'vbl vbl-shape vbl-banner', role: 'status', 'aria-live': 'polite' },
      h('span', { className: 'vbl-banner__title vbl-pulse' }, p.title || 'Reconnecting'),
      h('span', { className: 'vbl-banner__body' }, p.children || 'Resynchronizing until a full authoritative baseline arrives. Inputs are paused; the match continues.'),
      h('i', { className: 'vbl-banner__bar' }));
  }

  function MatchHeader(p) {
    var list = p.fighters || [];
    function f(x) {
      return h('div', { key: x.name, className: cx('vbl-match__f', x.team === 'ally' && 'vbl-match__f--ally', x.defeated && 'vbl-match__f--down') },
        h('span', { className: 'vbl-match__n' }, x.name),
        h(HealthBar, { value: x.defeated ? 0 : x.health, team: x.team === 'ally' ? 'ally' : 'enemy', size: 'world', style: { width: '100%' } }));
    }
    var clk = h('div', { className: 'vbl-match__clock' },
      h('span', { className: 'vbl-match__time', role: 'timer' }, p.clock),
      p.phase === 'pressure' ? h('span', { className: 'vbl-match__pressure' }, 'Pressure') : (p.sub && h('span', { className: 'vbl-match__phase' }, p.sub)));
    if (p.compact) return h('div', { className: 'vbl vbl-shape vbl-match vbl-match--compact' }, clk, h('div', { className: 'vbl-match__side' }, list.map(f)));
    var half = Math.ceil(list.length / 2);
    return h('div', { className: 'vbl vbl-shape vbl-match' },
      h('div', { className: 'vbl-match__side' }, list.slice(0, half).map(f)), clk,
      h('div', { className: 'vbl-match__side' }, list.slice(half).map(f)));
  }

  function CombatFeed(p) {
    var rows = p.entries || [];
    function who(name, team) { return h('b', { className: team === 'enemy' ? 'vbl-feed__e' : team === 'system' ? 'vbl-feed__s' : 'vbl-feed__a' }, name); }
    return h('div', { className: 'vbl vbl-shape vbl-feed', role: 'log', 'aria-live': 'polite', style: p.style },
      h('div', { className: 'vbl-feed__head' }, p.title || 'Combat feed'),
      rows.map(function (r, i) {
        return h('div', { key: i, className: cx('vbl-feed__row', i < rows.length - 4 && 'vbl-feed__row--old') },
          h('span', { className: 'vbl-feed__t' }, r.t),
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
    return h('div', { className: 'vbl vbl-shape vbl-minimap vbl-scan', role: 'img', 'aria-label': p.label || 'Tactical minimap' },
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
      p.caption !== false && h('div', { className: 'vbl-minimap__lbl' }, h('span', null, p.caption || 'Arena'), h('span', null, '40 × 62 m')));
  }

  function OffscreenIndicator(p) {
    return h('div', { className: cx('vbl vbl-shape vbl-off', p.team === 'ally' && 'vbl-off--ally', p.side === 'left' && 'vbl-off--left', p.threat && 'vbl-off--threat'), role: 'img', 'aria-label': p.name + ' off screen ' + (p.side || 'right') },
      p.side === 'left' && h(Icon, { name: 'chevron-left', size: 14 }),
      h('i', { className: 'vbl-off__mark' }),
      h('span', null, p.name),
      p.distance != null && h('span', { className: 'vbl-off__d' }, p.distance + ' m'),
      p.side !== 'left' && h(Icon, { name: 'chevron-right', size: 14 }));
  }

  /* ── world space ────────────────────────────────────────────────────────── */
  function WorldLabel(p) {
    var k = p.kind || 'aim';
    var details = p.detail ? [].concat(p.detail) : [];
    if (k === 'name') {
      return h('div', { className: cx('vbl vbl-wlabel vbl-wlabel--name', p.team && 'vbl-wlabel--' + p.team) },
        h('span', { className: 'vbl-wlabel__line' }, p.title),
        p.health != null && h(HealthBar, { value: p.health, stability: p.stability, barrier: p.barrier, team: p.team === 'enemy' ? 'enemy' : 'ally', size: 'world', exposed: p.exposed }));
    }
    if (k === 'aim') {
      return h('div', { className: 'vbl vbl-wlabel vbl-wlabel--aim' },
        h('div', { className: 'vbl-shape vbl-cut-sm vbl-wlabel__box' },
          h('div', { className: 'vbl-wlabel__line' }, p.title, p.cost != null && h('em', null, ' ' + p.cost + ' POWER')),
          details.map(function (d) { return h('div', { key: d, className: 'vbl-wlabel__detail' }, d); })),
        p.stem !== false && h('div', { className: 'vbl-wlabel__stem' }));
    }
    return h('div', { className: cx('vbl vbl-wlabel', 'vbl-wlabel--' + k) },
      h('span', { className: 'vbl-wlabel__line' }, k === 'blocked' && h(Icon, { name: 'cancel', size: 12 }), p.title || (k === 'blocked' ? 'Blocked' : '')),
      details.map(function (d) { return h('span', { key: d, className: 'vbl-wlabel__detail' }, d); }));
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
      return h('svg', { className: 'vbl-wmark', width: s * 1.6, height: s, viewBox: '0 0 128 80', role: 'img', 'aria-label': 'Move order' },
        h('circle', { cx: 16, cy: 62, r: 9, fill: 'var(--ally)', fillOpacity: 0.16, stroke: 'var(--ally)', strokeWidth: 2 }),
        h('path', { d: 'M26 58C52 48 70 46 104 22', fill: 'none', stroke: 'var(--ally)', strokeWidth: 2, strokeDasharray: '5 5', strokeOpacity: 0.85 }),
        h('circle', { cx: 108, cy: 19, r: 9, fill: 'none', stroke: 'var(--ally)', strokeWidth: 2, strokeDasharray: '4 3' }),
        h('path', { d: 'M104 19H112M108 15V23', stroke: 'var(--ally)', strokeWidth: 1.5 }));
    } else {
      body = h('circle', { cx: 40, cy: 40, r: 22, fill: 'var(--ally)', fillOpacity: 0.16, stroke: 'var(--ally)', strokeWidth: 2.5 });
    }
    return h('svg', { className: 'vbl-wmark', width: s, height: s, viewBox: '0 0 80 80', role: 'img', 'aria-label': k + ' marker' }, body);
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
    var svg = h('svg', { className: 'vbl-tele', width: w, height: w * 140 / 220, viewBox: '0 0 220 140', role: 'img', 'aria-label': (enemy ? 'Enemy ' : 'Ally ') + shape + ' telegraph' },
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
    return p.iso ? h('div', { className: 'vbl-ground-wrap' }, h('div', { className: 'vbl-ground' }, svg)) : svg;
  }

  function CombatText(p) {
    var k = p.kind || 'damage';
    var v = p.value;
    var main = k === 'miss' ? 'MISS' : k === 'heal' ? '+' + v : k === 'power' ? '+' + v : k === 'taken' ? '−' + v : String(v);
    var tag = k === 'exposed' ? 'EXPOSED' : k === 'barrier' ? 'BLOCKED' : k === 'power' ? 'POWER' : null;
    return h('span', { className: cx('vbl vbl-ctext', 'vbl-ctext--' + k, p.animate && 'vbl-ctext--anim'), role: 'status' }, main, tag && h('small', null, tag));
  }

  function HoloPlatform(p) {
    var st = p.state || 'occupied';
    var view = h('div', { className: 'vbl-plat__view' },
      st !== 'open' && h('div', { className: 'vbl-plat__beam' }),
      h('div', { className: 'vbl-plat__disc' }),
      st === 'open' ? h('div', { className: 'vbl-plat__plus' }, h(Icon, { name: 'plus', size: 20 })) : (p.marker !== false && h('div', { className: 'vbl-plat__pawn' }, h('i'))));
    return h('div', { className: cx('vbl vbl-plat', 'vbl-plat--' + st, p.team === 'enemy' && 'vbl-plat--enemy'), onClick: p.onClick },
      st === 'selected' ? h('span', { className: 'vbl-lock', style: { '--vbl-off': '-2px' } }, view) : view,
      h('div', { className: 'vbl-plat__label' }, st === 'open' ? 'Open slot' : p.label, (p.sub || st === 'open') && h('small', null, p.sub || 'weight shared')));
  }

  /* ── meta ───────────────────────────────────────────────────────────────── */
  var TIER = { blue: ['R', 'Rare'], violet: ['E', 'Epic'], gold: ['L', 'Legendary'] };
  function TeamTile(p) {
    var st = p.state || 'available';
    var t = TIER[p.tier];
    var btn = h('button', {
      type: 'button', onClick: p.onClick, 'aria-pressed': st === 'in' || st === 'selected', 'aria-disabled': st === 'locked' ? 'true' : undefined,
      className: cx('vbl-shape vbl-tile vbl-press', st === 'in' && 'vbl-tile--in', st === 'selected' && 'vbl-tile--selected vbl-brackets', st === 'locked' && 'vbl-tile--locked')
    },
      t && h('span', { className: 'vbl-tile__tier', style: { color: 'var(--tier-' + p.tier + ')' }, title: t[1] }, t[0]),
      h('span', { className: 'vbl-tile__code' }, st === 'locked' ? h(Icon, { name: 'lock', size: 20 }) : p.code),
      h('span', { className: 'vbl-tile__name' }, p.name),
      p.role && h('span', { className: 'vbl-tile__role' }, p.role),
      (st === 'in' || st === 'selected') && h('span', { className: 'vbl-tile__state' }, 'In team'),
      st === 'locked' && h('span', { className: 'vbl-tile__state' }, 'Pull in rifts'));
    return h('span', { className: cx('vbl', st === 'selected' && 'vbl-glow-ally'), style: { display: 'inline-block' } }, btn);
  }

  var RARITY = { normal: 'N', magic: 'M', rare: 'R', set: 'S', unique: 'U' };
  function ItemCell(p) {
    var st = p.state || 'default';
    var slot = p.slot || 'core';
    var cell;
    if (st === 'empty') {
      cell = h('button', { type: 'button', className: 'vbl-shape vbl-item vbl-item--empty vbl-press', 'aria-label': 'Empty ' + slot + ' slot', onClick: p.onClick }, h(Icon, { name: 'plus', size: 18 }));
    } else {
      cell = h('button', {
        type: 'button', onClick: p.onClick, 'aria-label': (p.name || slot) + ', ' + (p.rarity || 'normal') + (p.upgrade ? ' +' + p.upgrade : ''),
        className: cx('vbl-shape vbl-item vbl-press', 'vbl-item--' + (p.rarity || 'normal'), st === 'selected' && 'vbl-item--selected vbl-brackets', st === 'locked' && 'vbl-item--locked')
      },
        h(Icon, { name: slot, size: 22 }),
        h('span', { className: 'vbl-item__r' }, RARITY[p.rarity || 'normal']),
        p.upgrade ? h('span', { className: 'vbl-item__up' }, '+' + p.upgrade) : null,
        st === 'locked' && h('span', { className: 'vbl-item__lock' }, h(Icon, { name: 'lock', size: 16 })));
    }
    var wrapped = st === 'selected' ? h('span', { className: 'vbl-glow-ally', style: { display: 'inline-block' } }, cell) : cell;
    if (!p.showSlot) return h('span', { className: 'vbl', style: { display: 'inline-block' } }, wrapped);
    return h('span', { className: 'vbl', style: { display: 'inline-flex', flexDirection: 'column', alignItems: 'center' } }, wrapped, h('span', { className: 'vbl-item__slot' }, slot));
  }

  function StatReadout(p) {
    var d = p.delta;
    var c0 = typeof d === 'string' ? d.charAt(0) : '';
    var dir = p.deltaTone || (c0 === '+' ? 'up' : (c0 === '-' || c0 === '−') ? 'down' : null);
    return h('div', { className: 'vbl vbl-stat' },
      h('span', { className: 'vbl-stat__label' }, p.label),
      h('span', { className: 'vbl-stat__value' }, fmt(p.value), p.unit && h('small', null, p.unit)),
      d && h('span', { className: cx('vbl-stat__delta', dir && 'vbl-stat__delta--' + dir) }, d),
      p.bar != null && h('div', { className: cx('vbl-stat__bar', p.barTone && 'vbl-stat__bar--' + p.barTone) }, h('i', { style: { width: pct(clamp(p.bar)) } })));
  }

  function RevealCard(p) {
    var tier = p.tier || 'blue';
    var st = R.useState(p.flipped !== false);
    var flipped = p.onFlip ? p.flipped !== false : st[0];
    var t = TIER[tier];
    var label = p.tierLabel || (p.kind === 'item' ? ({ blue: 'Magic', violet: 'Rare', gold: 'Unique' }[tier]) + ' item' : t[1]);
    var card = h('div', {
      className: cx('vbl vbl-reveal', 'vbl-reveal--' + tier, flipped && 'vbl-reveal--flipped'), role: 'button', tabIndex: 0, 'aria-pressed': flipped,
      'aria-label': flipped ? label + ' ' + p.name : 'Hidden card, tap to reveal',
      onClick: function () { if (p.onFlip) p.onFlip(!flipped); else st[1](!flipped); }
    },
      h('div', { className: 'vbl-reveal__inner' },
        h('div', { className: 'vbl-shape vbl-reveal__face vbl-reveal__back vbl-scan' }, h(Icon, { name: 'rift', size: 44 })),
        h('div', { className: 'vbl-shape vbl-reveal__face vbl-reveal__front' },
          h('span', { className: 'vbl-reveal__tier' }, label),
          h('span', { className: 'vbl-reveal__code' }, p.kind === 'item' ? h(Icon, { name: p.slot || 'core', size: 44, style: { color: 'var(--tier-' + tier + ')' } }) : p.code),
          h('span', { className: cx('vbl-reveal__name', tier === 'gold' && flipped && 'vbl-chroma') }, p.name),
          p.role && h('span', { className: 'vbl-reveal__role' }, p.role),
          p.dupe && h('span', { className: 'vbl-reveal__dupe' }, p.dupe),
          p.isNew && h('span', { className: 'vbl-reveal__new' }, 'NEW'))));
    return h('span', { className: cx(flipped && tier === 'gold' && 'vbl-glow-gold'), style: { display: 'inline-block' } }, card);
  }

  function RuleRow(p) {
    var st = R.useState(p.enabled !== false);
    var on = st[0];
    var conds = p.conditions || [];
    var a = p.action || {};
    var ifParts = [h('span', { key: 'if', className: 'vbl-rule__kw' }, 'IF')];
    conds.forEach(function (c, i) {
      if (i > 0) ifParts.push(h('span', { key: 'k' + i, className: 'vbl-rule__kw' }, p.join || 'AND'));
      ifParts.push(h('span', { key: 'c' + i, className: 'vbl-rule__cond' }, c.text, c.value != null && h(F, null, ' ', h('b', null, c.value)), c.after ? ' ' + c.after : ''));
    });
    return h('div', { className: cx('vbl vbl-shape vbl-rule', !on && 'vbl-rule--off', p.warning && 'vbl-rule--warn', p.draft && 'vbl-rule--draft') },
      h('span', { className: 'vbl-rule__grip', 'aria-hidden': 'true' }),
      h('span', { className: 'vbl-rule__n' }, p.index),
      h('div', { className: 'vbl-rule__if' }, ifParts),
      h('div', { className: 'vbl-rule__then' },
        h('span', { className: 'vbl-rule__kw' }, 'THEN'),
        h('span', { className: 'vbl-rule__act' }, h('b', null, a.title), a.rest ? ' ' + a.rest : ''),
        a.cost && h('span', { className: 'vbl-rule__cost' }, a.cost)),
      h('button', { type: 'button', role: 'checkbox', 'aria-checked': on ? 'true' : 'false', 'aria-label': 'Rule ' + p.index + ' enabled', className: 'vbl-rule__check', onClick: function () { st[1](!on); } },
        on && h(Icon, { name: 'check', size: 14 })),
      (p.warning || p.note) && h('div', { className: 'vbl-rule__note' }, p.warning || p.note));
  }

  function Timeline(p) {
    var dur = p.duration || 120;
    var pos = p.position || 0;
    var events = p.events || [];
    var sp = R.useState(p.speed || 1);
    var pl = R.useState(!!p.playing);
    var speeds = [0.25, 0.5, 1, 2];
    return h('div', { className: 'vbl vbl-shape vbl-tl' },
      h('div', { className: 'vbl-tl__track', role: 'slider', tabIndex: 0, 'aria-label': 'Replay position', 'aria-valuemin': 0, 'aria-valuemax': dur, 'aria-valuenow': pos, 'aria-valuetext': clock(pos, true) },
        h('i', { className: 'vbl-tl__line' }),
        h('i', { className: 'vbl-tl__played', style: { width: pct(clamp(pos / dur)) } }),
        p.pressureFrom != null && h('i', { className: 'vbl-tl__pressure', style: { left: pct(clamp(p.pressureFrom / dur)) } }),
        events.map(function (e, i) { return h('i', { key: i, title: e.label, className: cx('vbl-tl__ev', e.team === 'enemy' && 'vbl-tl__ev--enemy', e.team === 'phase' && 'vbl-tl__ev--phase'), style: { left: pct(clamp(e.t / dur)) } }); }),
        h('i', { className: 'vbl-tl__head', style: { left: pct(clamp(pos / dur)) } })),
      h('div', { className: 'vbl-tl__bar' },
        h('button', { type: 'button', className: 'vbl-shape vbl-tl__btn', 'aria-label': 'Previous event' }, h(Icon, { name: 'prev', size: 16 })),
        h('button', { type: 'button', className: cx('vbl-shape vbl-tl__btn', 'vbl-tl__btn--on'), 'aria-label': pl[0] ? 'Pause' : 'Play', onClick: function () { pl[1](!pl[0]); } }, h(Icon, { name: pl[0] ? 'pause' : 'play', size: 16 })),
        h('button', { type: 'button', className: 'vbl-shape vbl-tl__btn', 'aria-label': 'Next event' }, h(Icon, { name: 'next', size: 16 })),
        h('span', { className: 'vbl-tl__time' }, clock(pos, true), h('small', null, ' / ' + clock(dur))),
        h('div', { className: 'vbl-tl__speeds', role: 'group', 'aria-label': 'Speed' }, speeds.map(function (s) {
          return h('button', { key: s, type: 'button', className: 'vbl-tl__speed', 'aria-pressed': s === sp[0] ? 'true' : 'false', onClick: function () { sp[1](s); } }, s + '×');
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
  var NS = window.Veilbreak = window.Veilbreak || {};
  NS.Legacy = api;
  ["ConnectionPill", "ConnectionBanner", "CombatFeed", "Minimap", "NavBar", "MatchHeader", "ItemCell", "RailButton", "Timeline", "FighterPortrait", "Telegraph"].forEach(function (n) { NS[n] = api[n]; });
})();

/* The kit: a web port of the Unity package com.veilbreak.gui (UI Toolkit, portrait 390 x 844).
   Same element trees, BEM classes (the USS vb- names; the legacy kit above is renamed to vbl-)
   and Bind() logic as the C# sources; Gui.uss, Layout.uss and World.uss are ported mechanically into bundle.css. */
(function () {
  'use strict';
  var R = window.React;
  var h = R.createElement;
  // Layout effects run in the browser only, so server rendering (Next.js and the like) is quiet; the paint happens on hydration.
  var useIsoLayout = typeof document !== 'undefined' ? R.useLayoutEffect : R.useEffect;
  var ICONS = {"add":"<path d=\"M12 4v16 M4 12h16\"/>","armor":"<path d=\"M8 2h8l2 4 4 2-3 6-3-1v9H8v-9l-3 1-3-6 4-2Z\"/>","arrow":"<path d=\"M6 18L18 6M10 6H18V14\"/>","automation":"<path d=\"M19 6a8 8 0 1 0 1 10 M19 2v5h-5\"/>","autopilot":"<path d=\"M19 12A7 7 0 1 1 16.95 7.05M17 3V7.05H13\"/>","back":"<path d=\"M15 4 7 12l8 8\"/>","barrier":"<path d=\"M12 2 21 6v7l-9 9-9-9V6Z M7 11l4 4 6-7\"/>","bastion":"<path d=\"M3 4h18v10l-9 8-9-8Z M12 4v18\"/>","battle":"<path d=\"M4 3 20 19 M20 3 4 19 M3 15l6 6 M15 21l6-6 M4 3v5 M20 3v5\"/>","beam":"<path d=\"M2 8h20 M2 16h20 M5 12h14 M17 5l5 7-5 7\"/>","breaker":"<path d=\"M3 4h8v7H3Z M14 4h7v7h-7Z M3 14h8v7H3Z M17 14l4 7 M14 17l7-3\"/>","burst":"<g stroke-linecap=\"butt\" stroke-linejoin=\"miter\"><circle cx=\"12\" cy=\"12\" r=\"9\"/><path d=\"M5 15L15 5M7 18L18 7M10 20L20 10M4 11L11 4\"/></g>","burst-role":"<path d=\"M15.2 12A3.2 3.2 0 1 1 8.8 12A3.2 3.2 0 1 1 15.2 12M12 2V6M12 18V22M2 12H6M18 12H22M5 5L7.8 7.8M16.2 16.2L19 19M19 5L16.2 7.8M7.8 16.2L5 19\"/>","charm":"<path d=\"M12 4L20 12L12 20L4 12ZM12 9L15 12L12 15L9 12Z\"/>","check":"<path d=\"M3 12l6 6L21 5\"/>","chevron":"<path d=\"M8 4l8 8-8 8\"/>","chevron-left":"<path d=\"M15 6L9 12L15 18\"/>","circle":"<path d=\"M20 12A8 8 0 1 1 4 12A8 8 0 1 1 20 12\"/>","cleanse":"<path d=\"M12 2l3 7 7 3-7 3-3 7-3-7-7-3 7-3Z\"/>","close":"<path d=\"M5 5l14 14 M19 5 5 19\"/>","cone":"<path d=\"M12 21 2 5Q12-1 22 5Z\"/>","connection":"<path d=\"M2 7a15 15 0 0 1 20 0 M5 11a10 10 0 0 1 14 0 M8 15a5 5 0 0 1 8 0\"/><circle cx=\"12\" cy=\"19\" r=\"1\"/>","cooldown":"<circle cx=\"12\" cy=\"12\" r=\"9\"/><path d=\"M12 5v7l5 3\"/>","core":"<path d=\"M12 3L20 7.5V16.5L12 21L4 16.5V7.5ZM14.6 12A2.6 2.6 0 1 1 9.4 12A2.6 2.6 0 1 1 14.6 12\"/>","counter":"<path d=\"M5 7h10a6 6 0 0 1 0 12h-4 M9 2 4 7l5 5\"/>","currency":"<path d=\"M12 2 21 7v10l-9 5-9-5V7Z M8 9l4-2 4 2v6l-4 2-4-2Z\"/>","dash":"<path d=\"M2 6h8 M2 12h12 M2 18h8 M13 3l9 9-9 9\"/>","debuffer":"<path d=\"M12 3L20 12L12 21L4 12ZM12 8V15M9 12L12 15L15 12\"/>","diamond":"<path d=\"M12 4L20 12L12 20L4 12Z\"/>","disruptor":"<path d=\"M13 3L6 13H12L10 21L18 10H12Z\"/>","exposed":"<path d=\"M12 2 21 6v7l-9 9-9-9V6Z M13 4 9 10l6 3-4 7\"/>","fan":"<path d=\"M12 21V3 M12 21 3 6 M12 21 21 6 M8 6l4-3 4 3\"/>","fullscreen":"<path d=\"M3 9V3h6 M15 3h6v6 M21 15v6h-6 M9 21H3v-6\"/>","haste":"<path d=\"M2 5l8 7-8 7 M12 5l8 7-8 7\"/>","healer":"<path d=\"M10 4H14V10H20V14H14V20H10V14H4V10H10Z\"/>","health":"<path d=\"M9 3h6v6h6v6h-6v6H9v-6H3V9h6Z\"/>","info":"<circle cx=\"12\" cy=\"12\" r=\"9\"/><path d=\"M12 11v7 M12 6v1\"/>","interrupted":"<path d=\"M13 3L7 12H11.5M12.5 12H17L11 21\"/>","invalid":"<circle cx=\"12\" cy=\"12\" r=\"9\"/><path d=\"M6 6l12 12\"/>","inventory":"<path d=\"M3 7l9-5 9 5v13H3Z M3 7h18 M8 11h8v5H8Z\"/>","key":"<circle cx=\"8\" cy=\"8\" r=\"5\"/><path d=\"M12 12l9 9 M16 16l3-3 M19 19l3-3\"/>","lock":"<path d=\"M5 10h14v11H5Z M8 10V6a4 4 0 0 1 8 0v4 M12 14v3\"/>","marker":"<path d=\"M12 2 22 12 12 22 2 12Z\"/><circle cx=\"12\" cy=\"12\" r=\"3\"/>","menu":"<path d=\"M4 7H20M4 12H20M4 17H20\"/>","next":"<path d=\"M6 6L14 12L6 18ZM18 6V18\"/>","no-target":"<path d=\"M18 12A6 6 0 1 1 6 12A6 6 0 1 1 18 12M12 3V8M12 16V21M3 12H8M16 12H21M4 4L20 20\"/>","offline":"<path d=\"M2 7a15 15 0 0 1 20 0 M5 11a10 10 0 0 1 14 0 M3 3l18 18\"/>","operator":"<circle cx=\"12\" cy=\"12\" r=\"4\"/><circle cx=\"4\" cy=\"4\" r=\"2\"/><circle cx=\"20\" cy=\"4\" r=\"2\"/><circle cx=\"12\" cy=\"21\" r=\"2\"/><path d=\"M6 6l3 3 M18 6l-3 3 M12 16v3\"/>","optic":"<path d=\"M2 12C5 7 8.5 5 12 5C15.5 5 19 7 22 12C19 17 15.5 19 12 19C8.5 19 5 17 2 12ZM14.6 12A2.6 2.6 0 1 1 9.4 12A2.6 2.6 0 1 1 14.6 12\"/>","pause":"<path d=\"M8 5v14 M16 5v14\"/>","pierce":"<path d=\"M4 12H20M15 7L20 12L15 17M4 8L7 12L4 16\"/>","plating":"<path d=\"M12 3L19 6V12C19 16 16 19 12 21C8 19 5 16 5 12V6ZM5 11H19\"/>","play":"<path d=\"M7 4v16l12-8z\"/>","power":"<path d=\"M14 2 5 14h6l-1 8 9-13h-6Z\"/>","prev":"<path d=\"M18 6L10 12L18 18ZM6 6V18\"/>","projectile":"<path d=\"M3 9h10V5l8 7-8 7v-4H3\"/>","protector":"<path d=\"M12 3L19 6V12C19 16 16 19 12 21C8 19 5 16 5 12V6Z\"/>","queue":"<path d=\"M3 4h18 M3 10h12 M3 16h8 M16 13l6 5-6 5\"/>","ranged":"<path d=\"M18 12A6 6 0 1 1 6 12A6 6 0 1 1 18 12M12 3V8M12 16V21M3 12H8M16 12H21\"/>","ranked":"<path d=\"M5 3h14v9l-7 5-7-5Z M12 17v4 M7 21h10 M2 4v5l3 2 M22 4v5l-3 2\"/>","remove":"<path d=\"m5 5 14 14 M19 5 5 19\"/>","replays":"<path d=\"M9 5v14l11-7Z M3 5v14\"/>","rifts":"<path d=\"M12 2 20 7 18 18 12 22 4 17 6 6Z M13 5 9 11 15 13 11 19\"/>","root":"<path d=\"M12 2v11 M5 7l7 6 7-6 M12 13l-7 8 M12 13l7 8 M12 13v9\"/>","set":"<path d=\"M3 3h7v7H3Z M14 3h7v7h-7Z M3 14h7v7H3Z M14 14h7v7h-7Z\"/>","settings":"<circle cx=\"12\" cy=\"12\" r=\"4\"/><path d=\"M12 1v3 M12 20v3 M1 12h3 M20 12h3 M4 4l2 2 M18 18l2 2 M4 20l2-2 M18 6l2-2\"/>","signal":"<path d=\"M5 19V16M10 19V12M15 19V8M20 19V4\"/>","silence":"<path d=\"M9 4h6v10H9Z M5 10v4a7 7 0 0 0 14 0v-4 M12 21v2 M2 2l20 20\"/>","skill-aegis-absolute-wall":"<path d=\"M4 3v18 M20 3v18 M4 6h16 M4 12h16 M4 18h16 M9 6v6 M15 12v6\"/>","skill-aegis-intercept-field":"<path d=\"M12 2 21 6v7l-9 9-9-9V6Z M7 11l4 4 6-7\"/>","skill-aria-needle-storm":"<path d=\"M19 5 5 12l14 7 M6 12h13\"/>","skill-aria-terminal-vector":"<path d=\"M3 8h18v8H3z M7 9l-3 6 M13 9l-4 6 M19 9l-4 6\"/>","skill-echo-clocksplice":"<path d=\"M2 5l8 7-8 7 M12 5l8 7-8 7\"/>","skill-echo-overclock":"<circle cx=\"12\" cy=\"12\" r=\"9\"/><circle cx=\"12\" cy=\"12\" r=\"5\"/><circle cx=\"12\" cy=\"12\" r=\"1\"/>","skill-hex-black-signal":"<path d=\"M12 2 22 12 12 22 2 12Z\"/><circle cx=\"12\" cy=\"12\" r=\"3\"/>","skill-hex-contagion":"<path d=\"M12 1v5 M12 18v5 M1 12h5 M18 12h5 M4 4l4 4 M16 16l4 4 M4 20l4-4 M16 8l4-4\"/><circle cx=\"12\" cy=\"12\" r=\"3\"/>","skill-mira-counterdrive":"<path d=\"M2 6h8 M2 12h12 M2 18h8 M13 3l9 9-9 9\"/>","skill-mira-gravity-cage":"<circle cx=\"12\" cy=\"12\" r=\"9\"/><circle cx=\"12\" cy=\"12\" r=\"5\"/><circle cx=\"12\" cy=\"12\" r=\"1\"/>","skill-nyra-reconstruct":"<path d=\"M9 3h6v6h6v6h-6v6H9v-6H3V9h6Z\"/>","skill-nyra-white-noise":"<path d=\"M12 2l3 7 7 3-7 3-3 7-3-7-7-3 7-3Z\"/>","skill-rook-crash-pulse":"<path d=\"M12 21 2 5Q12-1 22 5Z\"/>","skill-rook-system-seizure":"<circle cx=\"12\" cy=\"12\" r=\"9\"/><circle cx=\"12\" cy=\"12\" r=\"5\"/><circle cx=\"12\" cy=\"12\" r=\"1\"/>","skill-sol-dead-horizon":"<path d=\"M2 8h20 M2 16h20 M5 12h14 M17 5l5 7-5 7\"/>","skill-sol-zero-line":"<path d=\"M3 9h10V5l8 7-8 7v-4H3\"/>","skirmisher":"<path d=\"M5 6L11 12L5 18M12 6L18 12L12 18\"/>","slow":"<path d=\"M3 7l6 5-6 5 M10 7l6 5-6 5 M21 3v18\"/>","striker":"<circle cx=\"12\" cy=\"12\" r=\"4\"/><path d=\"M12 1v3 M12 20v3 M1 12h3 M20 12h3 M4 4l2 2 M18 18l2 2 M4 20l2-2 M18 6l2-2\"/>","studio":"<path d=\"M5 20 3 15 15 3 21 9 9 21Z M12 6l6 6 M3 15l6 6\"/>","support":"<path d=\"M3 12H7L9.5 6L14 18L16 12H21\"/>","surrender":"<path d=\"M6 21V4M6 4H18L15 8.5L18 13H6\"/>","target":"<circle cx=\"12\" cy=\"12\" r=\"6\"/><path d=\"M12 1v7 M12 16v7 M1 12h7 M16 12h7\"/>","team":"<circle cx=\"8\" cy=\"7\" r=\"3\"/><circle cx=\"17\" cy=\"9\" r=\"2\"/><path d=\"M2 21v-4a6 6 0 0 1 12 0v4 M16 15h2a4 4 0 0 1 4 4v2\"/>","training":"<circle cx=\"12\" cy=\"12\" r=\"7\"/><circle cx=\"12\" cy=\"12\" r=\"2\"/><path d=\"M12 1v4 M12 19v4 M1 12h4 M19 12h4\"/>","volume":"<path d=\"M3 9h5l6-6v18l-6-6H3Z M18 7a8 8 0 0 1 0 10\"/>","wall":"<path d=\"M4 3v18 M20 3v18 M4 6h16 M4 12h16 M4 18h16 M9 6v6 M15 12v6\"/>","warning":"<path d=\"M12 2 23 21H1Z M12 8v6 M12 17v1\"/>","weapon":"<path d=\"M4 20L16 8M13 4H20V11M7 13L11 17\"/>","windup":"<path d=\"M6 2h12 M6 22h12 M7 2v5l10 10v5 M17 2v5L7 17v5\"/>","zone":"<circle cx=\"12\" cy=\"12\" r=\"9\"/><circle cx=\"12\" cy=\"12\" r=\"5\"/><circle cx=\"12\" cy=\"12\" r=\"1\"/>","plus":"<path d=\"M12 4v16 M4 12h16\"/>","silenced":"<path d=\"M9 4h6v10H9Z M5 10v4a7 7 0 0 0 14 0v-4 M12 21v2 M2 2l20 20\"/>","rooted":"<path d=\"M12 2v11 M5 7l7 6 7-6 M12 13l-7 8 M12 13l7 8 M12 13v9\"/>","slowed":"<path d=\"M3 7l6 5-6 5 M10 7l6 5-6 5 M21 3v18\"/>","replay":"<path d=\"M9 5v14l11-7Z M3 5v14\"/>","cancel":"<path d=\"M5 5l14 14 M19 5 5 19\"/>","chevron-right":"<path d=\"M8 4l8 8-8 8\"/>","rift":"<path d=\"M12 2 20 7 18 18 12 22 4 17 6 6Z M13 5 9 11 15 13 11 19\"/>"};
  var MARK = "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"64 245 513 259\" width=\"513\" height=\"259\"> <g transform=\"translate(-2.5 0)\" fill=\"#ff40a0\" fill-opacity=\".55\"> <g> <g> <path d=\"M172.445 491.288 L210.995 421.197 L210.995 428.692 Z\"/> <path d=\"M231.395 401.288 L231.395 364.8 L249.539 364.8 C250.183 364.8 250.81 364.826 251.421 364.877 L251.421 364.877 L248.69 369.843 C248.589 369.841 248.488 369.84 248.387 369.84 L237.227 369.84 L237.227 386.4 L239.583 386.4 Z\"/> <path d=\"M510.899 410.216 L510.899 359.816 L516.731 359.816 L516.731 375.512 L516.731 386.384 L516.875 386.384 L525.083 377.672 L541.859 359.816 L548.915 359.816 L529.979 379.976 L550.859 410.288 L543.947 410.216 L525.659 384.152 L516.731 393.44 L516.731 410.216 Z\"/> <path d=\"M432.707 410.216 L454.163 359.816 L460.571 359.816 L482.027 410.216 L475.763 410.216 L470.147 396.824 L444.587 396.824 L438.971 410.216 Z M457.259 366.512 C457.187 366.584 454.235 373.712 452.363 378.248 L446.819 391.496 L467.915 391.496 L462.371 378.32 C460.427 373.712 457.475 366.584 457.403 366.512 Z\"/> <path d=\"M371.795 410.216 L371.795 359.816 L405.707 359.816 L405.707 365.288 L377.627 365.288 L377.627 381.128 L399.515 381.128 L399.515 386.6 L377.627 386.6 L377.627 404.744 L405.851 404.744 L405.851 410.216 Z\"/> <path d=\"M306.851 410.216 L306.851 359.816 L322.259 359.816 C334.067 359.816 341.051 365.648 341.051 375.512 C341.051 383.288 336.659 387.824 329.531 389.336 L341.987 410.216 L334.931 410.216 L323.483 390.416 C322.403 390.488 319.955 390.488 318.875 390.488 L312.683 390.488 L312.683 410.216 Z M312.683 365.216 L312.683 385.592 L322.835 385.592 C330.179 385.592 335.075 383 335.075 375.728 C335.075 368.6 330.323 365.216 322.475 365.216 Z\"/> <path d=\"M262.98 365.899 L265.743 361.112 C270.686 363.107 273.587 367.224 273.587 372.2 C273.587 378.824 269.339 382.208 265.667 383.072 L265.667 383.288 C270.419 384.08 277.115 387.608 277.115 396.176 C277.115 404.672 270.995 410.216 259.907 410.216 L240.395 410.216 L240.395 403.799 L246.227 394.329 L246.227 405.176 L259.619 405.176 C266.387 405.176 270.995 402.08 270.995 395.888 C270.995 389.552 265.667 386.024 259.475 386.024 L251.341 386.024 L251.729 385.395 L254.025 381.416 L257.963 381.416 C263.579 381.416 267.683 378.536 267.683 373.064 C267.683 369.768 266.003 367.233 262.98 365.899 Z\"/> <path d=\"M228.29 389.752 L231.021 384.786 C232.985 384.95 234.768 385.37 236.344 386.006 L233.581 390.793 L224.626 406.309 L222.329 410.288 L221.942 410.917 L216.827 419.222 L210.995 428.692 L210.995 421.197 L219.184 406.309 Z\"/> <path d=\"M274.439 322.579 L307.445 279.288 L256.865 366.928 L250.047 366.928 Z\"/> <path d=\"M240.704 383.965 L253.726 360.288 L260.395 360.288 L246.026 385.184 C244.45 384.549 242.667 384.128 240.704 383.965 Z\"/> <path d=\"M271.302 322.76 L274.311 317.288 L281.129 317.288 L298.445 317.288 L298.445 322.76 L277.971 322.76 Z\"/> <g> <path d=\"M267.773 320.4 L267.773 270 L273.605 270 L273.605 314.928 L276.039 314.928 L273.03 320.4 Z\"/> <path d=\"M229.253 320.4 L229.253 270 L235.085 270 L235.085 320.4 Z\"/> <path d=\"M164.453 320.4 L164.453 270 L198.365 270 L198.365 275.472 L170.285 275.472 L170.285 291.312 L192.173 291.312 L192.173 296.784 L170.285 296.784 L170.285 314.928 L198.509 314.928 L198.509 320.4 Z\"/> <path d=\"M109.157 320.4 L89.141 270 L95.693 270 L105.989 296.424 C110.093 306.936 112.613 313.632 112.685 313.848 L112.829 313.848 C112.901 313.632 115.349 307.008 119.381 296.784 L129.821 270 L136.157 270 L116.141 320.4 Z\"/> </g> </g> </g> </g> <g transform=\"translate(2.5 0)\" fill=\"#1dd9a0\" fill-opacity=\".55\"> <g> <g> <path d=\"M172.445 491.288 L210.995 421.197 L210.995 428.692 Z\"/> <path d=\"M231.395 401.288 L231.395 364.8 L249.539 364.8 C250.183 364.8 250.81 364.826 251.421 364.877 L251.421 364.877 L248.69 369.843 C248.589 369.841 248.488 369.84 248.387 369.84 L237.227 369.84 L237.227 386.4 L239.583 386.4 Z\"/> <path d=\"M510.899 410.216 L510.899 359.816 L516.731 359.816 L516.731 375.512 L516.731 386.384 L516.875 386.384 L525.083 377.672 L541.859 359.816 L548.915 359.816 L529.979 379.976 L550.859 410.288 L543.947 410.216 L525.659 384.152 L516.731 393.44 L516.731 410.216 Z\"/> <path d=\"M432.707 410.216 L454.163 359.816 L460.571 359.816 L482.027 410.216 L475.763 410.216 L470.147 396.824 L444.587 396.824 L438.971 410.216 Z M457.259 366.512 C457.187 366.584 454.235 373.712 452.363 378.248 L446.819 391.496 L467.915 391.496 L462.371 378.32 C460.427 373.712 457.475 366.584 457.403 366.512 Z\"/> <path d=\"M371.795 410.216 L371.795 359.816 L405.707 359.816 L405.707 365.288 L377.627 365.288 L377.627 381.128 L399.515 381.128 L399.515 386.6 L377.627 386.6 L377.627 404.744 L405.851 404.744 L405.851 410.216 Z\"/> <path d=\"M306.851 410.216 L306.851 359.816 L322.259 359.816 C334.067 359.816 341.051 365.648 341.051 375.512 C341.051 383.288 336.659 387.824 329.531 389.336 L341.987 410.216 L334.931 410.216 L323.483 390.416 C322.403 390.488 319.955 390.488 318.875 390.488 L312.683 390.488 L312.683 410.216 Z M312.683 365.216 L312.683 385.592 L322.835 385.592 C330.179 385.592 335.075 383 335.075 375.728 C335.075 368.6 330.323 365.216 322.475 365.216 Z\"/> <path d=\"M262.98 365.899 L265.743 361.112 C270.686 363.107 273.587 367.224 273.587 372.2 C273.587 378.824 269.339 382.208 265.667 383.072 L265.667 383.288 C270.419 384.08 277.115 387.608 277.115 396.176 C277.115 404.672 270.995 410.216 259.907 410.216 L240.395 410.216 L240.395 403.799 L246.227 394.329 L246.227 405.176 L259.619 405.176 C266.387 405.176 270.995 402.08 270.995 395.888 C270.995 389.552 265.667 386.024 259.475 386.024 L251.341 386.024 L251.729 385.395 L254.025 381.416 L257.963 381.416 C263.579 381.416 267.683 378.536 267.683 373.064 C267.683 369.768 266.003 367.233 262.98 365.899 Z\"/> <path d=\"M228.29 389.752 L231.021 384.786 C232.985 384.95 234.768 385.37 236.344 386.006 L233.581 390.793 L224.626 406.309 L222.329 410.288 L221.942 410.917 L216.827 419.222 L210.995 428.692 L210.995 421.197 L219.184 406.309 Z\"/> <path d=\"M274.439 322.579 L307.445 279.288 L256.865 366.928 L250.047 366.928 Z\"/> <path d=\"M240.704 383.965 L253.726 360.288 L260.395 360.288 L246.026 385.184 C244.45 384.549 242.667 384.128 240.704 383.965 Z\"/> <path d=\"M271.302 322.76 L274.311 317.288 L281.129 317.288 L298.445 317.288 L298.445 322.76 L277.971 322.76 Z\"/> <g> <path d=\"M267.773 320.4 L267.773 270 L273.605 270 L273.605 314.928 L276.039 314.928 L273.03 320.4 Z\"/> <path d=\"M229.253 320.4 L229.253 270 L235.085 270 L235.085 320.4 Z\"/> <path d=\"M164.453 320.4 L164.453 270 L198.365 270 L198.365 275.472 L170.285 275.472 L170.285 291.312 L192.173 291.312 L192.173 296.784 L170.285 296.784 L170.285 314.928 L198.509 314.928 L198.509 320.4 Z\"/> <path d=\"M109.157 320.4 L89.141 270 L95.693 270 L105.989 296.424 C110.093 306.936 112.613 313.632 112.685 313.848 L112.829 313.848 C112.901 313.632 115.349 307.008 119.381 296.784 L129.821 270 L136.157 270 L116.141 320.4 Z\"/> </g> </g> </g> </g> <g fill=\"#faeef4\"> <g> <g> <path d=\"M172.445 491.288 L210.995 421.197 L210.995 428.692 Z\"/> <path d=\"M231.395 401.288 L231.395 364.8 L249.539 364.8 C250.183 364.8 250.81 364.826 251.421 364.877 L251.421 364.877 L248.69 369.843 C248.589 369.841 248.488 369.84 248.387 369.84 L237.227 369.84 L237.227 386.4 L239.583 386.4 Z\"/> <path d=\"M510.899 410.216 L510.899 359.816 L516.731 359.816 L516.731 375.512 L516.731 386.384 L516.875 386.384 L525.083 377.672 L541.859 359.816 L548.915 359.816 L529.979 379.976 L550.859 410.288 L543.947 410.216 L525.659 384.152 L516.731 393.44 L516.731 410.216 Z\"/> <path d=\"M432.707 410.216 L454.163 359.816 L460.571 359.816 L482.027 410.216 L475.763 410.216 L470.147 396.824 L444.587 396.824 L438.971 410.216 Z M457.259 366.512 C457.187 366.584 454.235 373.712 452.363 378.248 L446.819 391.496 L467.915 391.496 L462.371 378.32 C460.427 373.712 457.475 366.584 457.403 366.512 Z\"/> <path d=\"M371.795 410.216 L371.795 359.816 L405.707 359.816 L405.707 365.288 L377.627 365.288 L377.627 381.128 L399.515 381.128 L399.515 386.6 L377.627 386.6 L377.627 404.744 L405.851 404.744 L405.851 410.216 Z\"/> <path d=\"M306.851 410.216 L306.851 359.816 L322.259 359.816 C334.067 359.816 341.051 365.648 341.051 375.512 C341.051 383.288 336.659 387.824 329.531 389.336 L341.987 410.216 L334.931 410.216 L323.483 390.416 C322.403 390.488 319.955 390.488 318.875 390.488 L312.683 390.488 L312.683 410.216 Z M312.683 365.216 L312.683 385.592 L322.835 385.592 C330.179 385.592 335.075 383 335.075 375.728 C335.075 368.6 330.323 365.216 322.475 365.216 Z\"/> <path d=\"M262.98 365.899 L265.743 361.112 C270.686 363.107 273.587 367.224 273.587 372.2 C273.587 378.824 269.339 382.208 265.667 383.072 L265.667 383.288 C270.419 384.08 277.115 387.608 277.115 396.176 C277.115 404.672 270.995 410.216 259.907 410.216 L240.395 410.216 L240.395 403.799 L246.227 394.329 L246.227 405.176 L259.619 405.176 C266.387 405.176 270.995 402.08 270.995 395.888 C270.995 389.552 265.667 386.024 259.475 386.024 L251.341 386.024 L251.729 385.395 L254.025 381.416 L257.963 381.416 C263.579 381.416 267.683 378.536 267.683 373.064 C267.683 369.768 266.003 367.233 262.98 365.899 Z\"/> <path d=\"M228.29 389.752 L231.021 384.786 C232.985 384.95 234.768 385.37 236.344 386.006 L233.581 390.793 L224.626 406.309 L222.329 410.288 L221.942 410.917 L216.827 419.222 L210.995 428.692 L210.995 421.197 L219.184 406.309 Z\"/> <path d=\"M274.439 322.579 L307.445 279.288 L256.865 366.928 L250.047 366.928 Z\"/> <path d=\"M240.704 383.965 L253.726 360.288 L260.395 360.288 L246.026 385.184 C244.45 384.549 242.667 384.128 240.704 383.965 Z\"/> <path d=\"M271.302 322.76 L274.311 317.288 L281.129 317.288 L298.445 317.288 L298.445 322.76 L277.971 322.76 Z\"/> <g> <path d=\"M267.773 320.4 L267.773 270 L273.605 270 L273.605 314.928 L276.039 314.928 L273.03 320.4 Z\"/> <path d=\"M229.253 320.4 L229.253 270 L235.085 270 L235.085 320.4 Z\"/> <path d=\"M164.453 320.4 L164.453 270 L198.365 270 L198.365 275.472 L170.285 275.472 L170.285 291.312 L192.173 291.312 L192.173 296.784 L170.285 296.784 L170.285 314.928 L198.509 314.928 L198.509 320.4 Z\"/> <path d=\"M109.157 320.4 L89.141 270 L95.693 270 L105.989 296.424 C110.093 306.936 112.613 313.632 112.685 313.848 L112.829 313.848 C112.901 313.632 115.349 307.008 119.381 296.784 L129.821 270 L136.157 270 L116.141 320.4 Z\"/> </g> </g> </g> </g> </svg>";
/* Generated by Tools~/motion.py. */
var MOTION = {"stagger": 80, "maxDealDelay": 400, "recipes": {"viewIn": {"milliseconds": 320, "y": 12}, "viewOut": {"milliseconds": 180, "y": -8}, "deal": {"milliseconds": 420, "y": 24, "rotation": -6, "scale": 0.96}, "drawerIn": {"milliseconds": 460, "y": 80}, "drawerOut": {"milliseconds": 300, "y": 80}, "hudTop": {"milliseconds": 320, "y": -24}, "hudCard": {"milliseconds": 360, "y": 32, "rotation": -2, "scale": 0.98}}};


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
    useIsoLayout(function () {
      var el = ref.current; install(); shapes.add(el); if (ro) ro.observe(el); paint(el);
      return function () { shapes.delete(el); if (ro) ro.unobserve(el); };
    }, []);
    useIsoLayout(function () { schedule(); });
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
  // Recipes are generated from the same contract as Unity MotionRecipes.
  function reducedMotion(node) { return !!(node && node.closest('.vb-reduced-motion')) || (window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches); }
  var runningMotion = new WeakMap();
  function playMotion(node, recipeName, index, leaving) {
    if (!node || !node.animate) return Promise.resolve();
    var recipe = MOTION.recipes[recipeName], calm = reducedMotion(node), old = runningMotion.get(node);
    if (old) old.cancel();
    var delay = calm ? 0 : Math.min((index || 0) * MOTION.stagger, MOTION.maxDealDelay);
    var origin = { opacity: 0, translate: calm ? '0 0' : (recipe.x || 0) + 'px ' + (recipe.y || 0) + 'px', rotate: calm ? '0deg' : (recipe.rotation || 0) + 'deg', scale: calm ? 1 : (recipe.scale || 1) };
    var rest = { opacity: 1, translate: '0 0', rotate: '0deg', scale: 1 };
    var animation = node.animate(leaving ? [rest, origin] : [origin, rest], { duration: calm ? 80 : recipe.milliseconds, delay: delay, fill: 'both', easing: 'cubic-bezier(.16,1,.3,1)' });
    runningMotion.set(node, animation);
    var previousInert = node.inert; if (delay) node.inert = true;
    var timer = delay ? setTimeout(function () { node.inert = previousInert; }, delay) : null;
    function cleanup() { clearTimeout(timer); node.inert = previousInert; if (runningMotion.get(node) === animation) { runningMotion.delete(node); animation.cancel(); } }
    var query = window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)');
    function settle() { if (reducedMotion(node)) animation.finish(); }
    if (query) query.addEventListener('change', settle);
    var observer = new MutationObserver(settle);observer.observe(document.documentElement, { attributes: true, subtree: true, attributeFilter: ['class'] });
    return animation.finished.then(function(){return true;},function(){return false;}).then(function (completed) { observer.disconnect();if(query)query.removeEventListener('change',settle);cleanup();return completed; });
  }
  function MotionSurface(p) {
    var ref = R.useRef(null), seen = R.useRef(new WeakSet());
    useIsoLayout(function () { if (!ref.current) return; var fresh = Array.from(ref.current.querySelectorAll('[data-motion-item]')).filter(function (node) { return !seen.current.has(node); });fresh.forEach(function (node, i) { seen.current.add(node);playMotion(node, 'deal', i); }); });
    useIsoLayout(function () { var node=ref.current;playMotion(node, 'viewIn');return function(){if(node){[node].concat(Array.from(node.querySelectorAll('[data-motion-item]'))).forEach(function(e){var a=runningMotion.get(e);if(a)a.cancel();});}}; }, []);
    return h('section', { ref: ref, className: p.className, style: p.style, 'aria-label': p.label }, p.children);
  }
  // Modal ownership is reference-counted: nested overlays restore only their own siblings after exit.
  var coverage = new WeakMap();
  function coverSiblings(node) { var held=[],boundary=node.closest('[data-preview]')||document.body;for(var parent=node.parentElement;parent;parent=parent.parentElement){Array.from(parent.children).forEach(function(sibling){if(sibling===node||sibling.contains(node)||sibling.matches('script,style,link,svg')||sibling.matches('[data-overlay-root]')&&(node.compareDocumentPosition(sibling)&Node.DOCUMENT_POSITION_FOLLOWING))return;var old=coverage.get(sibling);if(old)old.count++;else {old={count:1,inert:sibling.inert};coverage.set(sibling,old);sibling.inert=true;sibling.classList.add('vb-overlay-covered');}held.push(sibling);});node=parent;if(parent===boundary)break;}
    return function(){held.forEach(function(sibling){var old=coverage.get(sibling);if(!old)return;if(--old.count===0){sibling.inert=old.inert;sibling.classList.remove('vb-overlay-covered');coverage.delete(sibling);}});};
  }
  /* ── Popover (Popover.cs): hover on a mouse, tap toggles on touch, focus opens; kept inside the viewport ── */
  var popoverStack=[];
  function usePopover(build, placement, alignEnd, pinned) {
    var anchor = R.useRef(null), pop = R.useRef(null), closeTimer = R.useRef(null), touchPinned = R.useRef(false), skipFocus=R.useRef(false);
    function cancelClose(){clearTimeout(closeTimer.current);}
    function later(){cancelClose();closeTimer.current=setTimeout(function(){var child=popoverStack.some(function(a){return a!==anchor&&pop.current&&pop.current.contains(a.current);});var hovered=anchor.current&&anchor.current.matches(':hover')||pop.current&&pop.current.matches(':hover');if(!pinned&&!touchPinned.current&&!child&&!hovered&&!(pop.current&&pop.current.contains(document.activeElement)))setOpen(false);},160);}
    var s = R.useState(false), open = s[0] || !!pinned, setOpen = s[1];
    var pos = R.useState(null), at = pos[0], setAt = pos[1], tick = R.useState(0)[1];
    R.useEffect(function(){if(!open)return;popoverStack.push(anchor);function outside(e){if(e.target.closest&&e.target.closest('.vb-popover'))return;if(anchor.current&&anchor.current.contains(e.target)||pop.current&&pop.current.contains(e.target))return;touchPinned.current=false;setOpen(false);}function key(e){if(e.key==='Escape'&&popoverStack[popoverStack.length-1]===anchor){e.stopImmediatePropagation();touchPinned.current=false;skipFocus.current=true;setOpen(false);if(anchor.current)anchor.current.focus();setTimeout(function(){skipFocus.current=false;},0);}}document.addEventListener('pointerdown',outside,true);document.addEventListener('keydown',key,true);document.addEventListener('vb-popover-closed',later);return function(){popoverStack=popoverStack.filter(function(a){return a!==anchor;});cancelClose();document.removeEventListener('pointerdown',outside,true);document.removeEventListener('keydown',key,true);document.removeEventListener('vb-popover-closed',later);document.dispatchEvent(new Event('vb-popover-closed'));};},[open]);
    // Popover.Place runs on every geometry change; re-place on resize, scroll and anchor resize.
    R.useEffect(function () {
      if (!open) return;
      function again() { tick(function (n) { return n + 1; }); }
      window.addEventListener('resize', again); window.addEventListener('scroll', again, true);
      var obs = window.ResizeObserver && anchor.current ? new ResizeObserver(again) : null; if (obs) obs.observe(anchor.current);
      return function () { window.removeEventListener('resize', again); window.removeEventListener('scroll', again, true); if (obs) obs.disconnect(); };
    }, [open]);
    useIsoLayout(function () {
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
      onPointerEnter: function (e) { if (e.pointerType === 'mouse') {cancelClose();setOpen(true);} },
      onPointerLeave: function (e) { if (e.pointerType === 'mouse') later(); },
      onPointerDown: function (e) { if (e.pointerType !== 'mouse') {e.stopPropagation();touchPinned.current=!open;setOpen(!open);} },
      onFocus: function () { if(skipFocus.current)return;cancelClose();setOpen(true); }, onBlur: later,
      onKeyDown: function(e){if(e.key==='Enter'||e.key===' '){e.preventDefault();e.stopPropagation();setOpen(!open);}}
    };
    var content = open ? build() : null, a = content && anchor.current;
    // Popover.Open: the layer sits outside the board, so it copies the anchor's root flags and theme.
    var flags = a ? ROOT_FLAGS.filter(function (c) { return a.closest('.' + c); }) : [], themed = a && a.closest('[data-theme]');
    var layer = content ? window.ReactDOM.createPortal(h('div', {
      ref: pop, role: 'region', 'aria-label': 'Details', onPointerEnter:cancelClose, onPointerLeave:later, onFocus:cancelClose, onBlur:later, className: cx('vb-root vb-popover', flags.join(' ')), 'data-theme': themed ? themed.getAttribute('data-theme') : undefined,
      style: { position: 'fixed', left: at ? at[0] : -9999, top: at ? at[1] : -9999, maxWidth: Math.max(80, window.innerWidth - 16), maxHeight: Math.max(80, window.innerHeight - 32), overflowY: 'auto', zIndex: 50, pointerEvents: 'auto' }
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

  function VbFrame(p) { return h(Shape, { className: cx(frameClass(p.kind), p.className), style: p.style }, p.children); }

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
      bar >= 0 ? h(ProgressBar, { value: bar, className: 'vb-stat__bar' }) : null);
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
      q.map(function(e,i){return h(QueueChip,{key:e.id||(e.actor+':'+e.slot+':'+e.who+':'+e.skill),view:e,position:i+1,onCancel:p.onCancel,className:cx('vb-queue__row',e.manual&&'vb-queue__row--manual')});}));
  }

  var HUD = { speed: 1, wave: 1, score: 0, boss: false, auto: true, slow: false, power: 0, maxPower: 100, regen: '+12/s', hint: null, focus: 100, previewFrom: -1, previewTo: -1, queue: [], queueTitle: 'AUTOPLAY QUEUE', queueHint: 'Fires as Power reaches each cost' };
  function HudTop(p) {
    var v = Object.assign({}, HUD, p.view), q = v.queue || [];
    var pop = usePopover(function () { return q.length ? h(QueueList, { queue: q, title: v.queueTitle, hint: v.queueHint, onCancel: p.onQueueCancel }) : null; }, 'below', false, p.queuePinned);
    return h('div', { className: cx('vb-hud', v.boss && 'vb-hud--boss', p.className), style: p.style },
      h('div', { className: 'vb-hud__row' },
        h(IconButtonComposite, { icon: 'pause', tooltip: 'Pause', quiet: true, className: 'vb-hud__pause', onClick: p.onPauseClick }),
        h(IconButtonComposite, { icon: 'automation', tooltip: v.auto ? 'Autoplay on · tap to disable' : 'Autoplay off · tap to enable', quiet: true, active: !!v.auto, className: cx('vb-hud__auto', v.auto && 'vb-hud__auto--on'), onClick: p.onAutoClick }),
        h('div', { className: 'vb-hud__stats' },
          h('div', { className: 'vb-hud__stat' }, L('vb-hud__key', v.boss ? 'BOSS' : 'WAVE'), h(GlitchText, { key: v.wave, text: pad2(v.wave), className: 'vb-hud__wave', effect: 'slam' })),
          h('div', { className: 'vb-hud__stat' }, L('vb-hud__key', 'SCORE'), L('vb-hud__score', compact(v.score)))),
        h(IconButtonComposite, { text: v.speed + '×', tooltip: 'Toggle simulation speed', className: 'vb-hud__speed', textClass: 'vb-hud__auto-state', onClick: p.onSpeedClick })),
      h(Shape, Object.assign({ ref: pop.ref, className: cx('vb-hud__resources', pop.open && 'vb-popover-open'), tabIndex: 0 }, pop.handlers),
        h('div', { className: 'vb-hud__resource-row' }, h(PowerStrip, { value: v.power, max: v.maxPower, regen: v.regen, hint: v.hint, previewFrom: v.previewFrom, previewTo: v.previewTo, marks: q.length ? [q[0].cost / Math.max(1, v.maxPower)] : null }), h(CreditsChip, { credits: v.credits || 0 })),
        h(FocusMeter, { focus: v.focus, slow: v.slow, style: show(p.showFocus !== false) })), pop.layer);
  }

  function BossBar(p) {
    var b = Object.assign({ name: '', health: 1, stability: 1, detail: '', shielded: false, interruptible: false, shieldDetail: '' }, p.view);
    var effects=(b.effects||[]).slice();if(b.shielded&&!effects.some(function(e){return e.definition.id==='boss-protection';}))effects.push({id:'boss:shield',definition:{id:'shielded',title:'Shielded',description:b.shieldDetail,icon:'barrier',polarity:'benefit'}});if(b.interruptible&&!effects.some(function(e){return e.definition.id==='interruptible';}))effects.push({id:'boss:interruptible',definition:{id:'interruptible',title:'Interruptible',description:'This action can be interrupted.',icon:'warning'}});
    return h(Shape, { className: cx('vb-boss', p.className) },
      h('div', { className: 'vb-boss__head' }, h(VbIcon, { icon: 'marker', size: 14, className: 'vb-boss__icon' }), L('vb-boss__name', upper(b.name)), L('vb-boss__detail', b.detail)),
      h(HealthBar, { className: 'vb-boss__health', hp: b.health, max: 1, barrier: 0, stability: b.stability, exposed: b.stability <= 0, enemy: true }),
      h(EffectStrip, {className:'vb-boss__chips',effects:effects}));
  }

  /* ── Screens ──────────────────────────────────────────────────────────── */
  function ScreenHeader(p) {
    return h('div', { className: cx('vb-header', p.className) },
      h(IconButtonComposite, { icon: 'back', tooltip: p.backLabel || 'Back', className: 'vb-header__back', onClick: p.onBack }),
      L('vb-header__title', upper(p.title)),
      L(cx('vb-header__meta', p.accent && 'vb-header__meta--accent'), p.meta || ''));
  }

  function ScoreRow(p) {
    var v = Object.assign({ rank: 0, score: 0, wave: 0, bosses: 0, squad: [], current: false }, p.view);
    return h(ListRow, { className: cx('vb-score', v.current && 'vb-score--current', p.className), style: p.style, selected: v.current, lead: pad2(v.rank), leadClass: v.rank <= 3 && 'vb-score__rank--top',
      title: num(v.score), value: v.current ? 'THIS RUN' : '', caption: 'Wave ' + v.wave + ' · ' + v.bosses + ' ' + (v.bosses === 1 ? 'boss' : 'bosses') + (v.squad.length ? ' · ' + v.squad.join(' ') : '') });
  }


  /* ── Operatives ───────────────────────────────────────────────────────── */
  var OPERATIVE = { id: '', name: '', code: '', role: '', roleIcon: 'striker', passive: '', passiveText: '', passiveFullText: '', kit: '#ffffff', portrait: null, hp: 0, maxHp: 1, barrier: 0, armor: 0, stability: 100, maxStability: 100, effects: [], mode: 'auto', manualSeconds: 0, skills: [{}, { slot: 'S', kind: 'Signature' }] };
  function operative(v) { return Object.assign({}, OPERATIVE, v); }
  function fallenOf(v) { return kebab(v.mode) === 'fallen' || v.hp <= 0; }
  function img(url) { return url ? { backgroundImage: 'url("' + url + '")' } : null; }
  function Strike(on, text) { return on ? h('s', null, text) : text; }

  function effectModel(v){return Object.assign({id:'unknown',definition:{title:'Unknown effect',description:'No authored description is available.',icon:'info',scope:'actor'},stacks:1,activation:'active'},v);}
  function EffectDetails(p){var e=effectModel(p.view),d=e.definition;return h('article',{className:cx('vb-effect-detail',p.className)},h('h3',{className:'vb-t-title'},d.title),h('p',null,d.description),h('dl',null,h('dt',null,'Source'),h('dd',null,e.source||'Unavailable'),h('dt',null,'Value'),h('dd',null,(e.magnitude||'—')+(e.stacks>1?' · ×'+e.stacks:'')),h('dt',null,'Duration'),h('dd',null,e.remainingSeconds!=null?seconds(e.remainingSeconds)+' remaining':'Unavailable or persistent'),h('dt',null,'State'),h('dd',null,(d.scope||'actor')+' · '+e.activation)));}
  function EffectBadge(p){var e=effectModel(p.view),d=e.definition,pop=usePopover(function(){return h(EffectDetails,{view:e});},'above',false,p.pinned);R.useEffect(function(){if(p.onOpenChange)p.onOpenChange(pop.open);},[pop.open]);return h(R.Fragment,null,h(Shape,Object.assign({as:'button',type:'button',ref:pop.ref,className:cx('vb-badge',st('vb-badge',d.polarity==='harm'?'enemy':d.polarity==='benefit'?'ally':'neutral'),'vb-effect-badge',st('vb-effect',e.activation),p.className),'data-effect-state':e.activation,'aria-label':d.title,'aria-expanded':pop.open},pop.handlers),h(VbIcon,{icon:d.icon||'info',size:14}),e.stacks>1?h('span',null,'×'+e.stacks):null),pop.layer);}
  function EffectList(p){var cache=R.useRef(new Map()),held=R.useState(new Set()),open=held[0],setOpen=held[1];var effects=p.effects||[],current=new Set(effects.map(function(e){return e.id;}));effects.forEach(function(e){cache.current.set(e.id,e);});var shown=effects.slice();cache.current.forEach(function(e,id){if(!current.has(id)){if(open.has(id))shown.push(Object.assign({},e,{activation:'expired',remainingSeconds:0}));else cache.current.delete(id);}});return h('ul',{className:cx('vb-effect-list',p.className)},shown.map(function(e){return h('li',{className:'vb-effect-row',key:e.id},h(EffectBadge,{view:e,onOpenChange:function(on){setOpen(function(previous){var next=new Set(previous);if(on)next.add(e.id);else next.delete(e.id);return next;});}}),h('span',null,e.definition.title+' · '+(e.magnitude||e.activation||'active')));}));}
  function EffectStrip(p){var last=R.useRef([]),wasOpen=R.useRef(false),all=p.effects||[];if(all.length)last.current=all;var visible=all.length?all:wasOpen.current?last.current.map(function(e){return Object.assign({},e,{activation:'expired',remainingSeconds:0});}):[];var pop=usePopover(function(){return h(EffectList,{effects:visible});},'above',false,p.pinned);if(!all.length&&!pop.open)visible=[];wasOpen.current=pop.open;if(!visible.length)return null;return h('div',{className:cx('vb-effect-strip',p.className)},!p.compact?h(EffectBadge,{view:visible[0]}):null,visible.length>(p.compact?0:1)?h('button',Object.assign({type:'button',ref:pop.ref,className:'vb-effect-strip__more','aria-label':'All effects · '+all.length,'aria-expanded':pop.open},pop.handlers),p.compact?h(VbIcon,{icon:visible[0].definition.icon||'info',size:14}):null,(p.compact?'':'+')+(p.compact?all.length:Math.max(0,all.length-1))):null,pop.layer);}
  function OperativeCard(p) {
    var v = operative(p.view), fallen = fallenOf(v), mode = fallen ? 'fallen' : kebab(v.mode);
    var chip = mode === 'manual' ? (v.manualSeconds > 0 ? 'MANUAL ' + seconds(v.manualSeconds) : 'MANUAL') : mode === 'aiming' ? 'AIMING' : mode === 'fallen' ? 'FALLEN' : 'AUTO';
    var effects = v.effects || [], statuses = effects.map(function(e){return e.definition.id;});
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
        h(EffectStrip, { className: 'vb-operative__statuses', compact:true, effects: effects })),
      h('div', { className: 'vb-operative__skills', style: show(!fallen) }, [0, 1].map(function (i) {
        return h(SkillButton, { key: i, view: v.skills[i] || OPERATIVE.skills[i], dragging: p.dragging === i, onPointerDown: p.onSkillPointerDown ? function (e) { p.onSkillPointerDown(i, e); } : null });
      })),
      h('div', { className: 'vb-operative__fallen', style: show(fallen) }, L('vb-operative__fallen-title', 'FALLEN'), L('vb-operative__fallen-hint', 'Revive via perk')));
  }

  /** TileComponent (Tile.cs): picture or glyph, name, caption with its glyph, order; `state` combines in | focused, or is locked | empty. */
  function TileComponent(p) {
    var b = p.block || 'vb-tile', state = [].concat(p.state || []).map(kebab), has = function (k) { return state.indexOf(k) >= 0; };
    var locked = has('locked'), empty = has('empty'), order = p.order || 0, glyph = p.glyph && !p.picture;
    return h(Shape, Object.assign({ className: cx(b, has('in') && !locked && b + '--in', has('focused') && b + '--focused', locked && b + '--locked', empty && b + '--empty', p.tier && st(b, p.tier), p.className), style: p.style, title: p.tooltip }, clickable(p.onPick)),
      h('div', { className: b + '__band', style: show(!empty) }, h('div', { className: b + '__portrait', style: img(p.picture) }), h('div', { className: b + '__shade' }),
        glyph ? h(VbIcon, { icon: p.glyph, size: 32, className: b + '__glyph' }) : null, L(b + '__order', String(order), show(order > 0 && !locked))),
      h('div', { className: b + '__kit', style: Object.assign({ backgroundColor: p.kit }, show(!empty)) }),
      h('div', { className: b + '__body', style: show(!empty) }, L(b + '__name', upper(p.title)),
        h('div', { className: b + '__role' }, h(VbIcon, { icon: p.captionIcon || '', size: 12 }), L(b + '__role-text', upper(p.caption)))),
      empty ? h('div', { className: b + '__empty' }, h(VbIcon, { icon: 'add', size: 20 }), L(b + '__empty-label', upper(p.emptyLabel))) : null);
  }
  // `tier` none | blue | violet | gold frames the tile; `locked` ("Pull in rifts") says how to get a locked operative.
  function OperativeTile(p) {
    var v = operative(p.view), slot = p.slot || 0, locked = p.locked || '', isLocked = locked.length > 0;
    return h(TileComponent, { className: p.className, style: p.style, onPick: p.onPick, tier: p.tier || 'none', picture: v.portrait, kit: v.kit, title: v.name, caption: isLocked ? locked : v.role, captionIcon: isLocked ? 'lock' : v.roleIcon, order: slot,
      state: [slot > 0 && !isLocked && 'in', p.focused && 'focused', isLocked && 'locked'].filter(Boolean),
      tooltip: v.name + ', ' + v.role + (isLocked ? ', locked: ' + locked : slot > 0 ? ', squad slot ' + slot : ', not in squad') });
  }

  function SquadSlot(p) {
    var has = !!p.view, v = operative(p.view), slot = p.slot || 0;
    return h(TileComponent, { block: 'vb-slot', className: p.className, style: p.style, onPick: p.onPick, picture: has ? v.portrait : null, kit: v.kit, title: v.name, caption: v.code, order: has ? slot : 0, state: has ? 'in' : 'empty', emptyLabel: 'SLOT ' + slot });
  }

  function OperativeDetails(p) {
    var v = operative(p.view), slot = p.slot || 0, inSquad = slot > 0;
    var maxHp = p.maxHp || 1580, maxArmor = p.maxArmor || 26, maxStab = p.maxStability || 140;
    return h(Shape, { className: cx('vb-details', p.className), style: p.style },
      h('div', { className: 'vb-details__head' },
        h('div', { className: 'vb-details__kit', style: { backgroundColor: v.kit } }), L('vb-details__name', upper(v.name)),
        h('div', { className: 'vb-details__role' }, h(VbIcon, { icon: v.roleIcon, size: 14 }), L('vb-details__role-text', upper(v.role))),
        L(cx('vb-details__slot', inSquad && 'vb-details__slot--in'), inSquad ? 'SLOT ' + slot : 'NOT IN SQUAD'),
        h(IconButtonComposite, { icon: 'close', tooltip: 'Close details', size: 12, className: 'vb-details__close', onClick: p.onClose })),
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
        h(ProgressBar, { value: c.share, tint: v.kit, className: 'vb-contribution__share-bar' })),
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

  /** ProgressBar (Progress.cs): continuous (`value` 0–1) or `segments` levels filled to `value`; `next` outlines the level a pick adds. */
  function ProgressBar(p) {
    var n = p.segments || 0, v = p.value || 0, filled = Math.round(v);
    return h('div', { className: cx('vb-progress', st('vb-progress', p.tone || 'ally'), n > 0 && 'vb-progress--segmented', p.className), style: p.style, role: 'progressbar', 'aria-valuenow': n ? filled : Math.round(clamp01(v) * 100), 'aria-valuemax': n || 100 },
      n > 0 ? Array.apply(null, Array(n)).map(function (_, i) { return h('div', { key: i, className: cx('vb-progress__cell', i < filled && 'vb-progress__cell--on', p.next && i === filled && 'vb-progress__cell--next'), style: p.tint && i < filled ? { backgroundColor: p.tint } : null }); })
        : h('div', { className: 'vb-progress__fill', style: { width: pct(v), backgroundColor: p.tint } }));
  }
  /** KeyValueRow (Progress.cs): a caption key on the left, a value on the right; `good` reads in ally colour. */
  function KeyValueRow(p) { return h('div', { className: cx('vb-kv', p.className), style: p.style }, L('vb-t-caption vb-kv__key', p.label), L(cx('vb-kv__value', p.good && 'vb-kv__value--good'), p.value)); }
  /** ListRow (Progress.cs): glyph, caps title, caption, optional `bar` (ProgressBar props), trailing `value` and `action`. */
  function ListRow(p) {
    return h(Shape, Object.assign({ 'data-motion-item': p.motionId, className: cx('vb-list-row', p.selected && 'vb-list-row--selected', p.className), style: p.style }, p.onPick ? clickable(p.onPick) : null),
      p.lead ? L(cx('vb-list-row__lead', p.leadClass), p.lead) : null, p.icon ? h(VbIcon, { icon: p.icon, size: 20, className: 'vb-list-row__icon' }) : null,
      h('div', { className: 'vb-list-row__text' }, p.children, L('vb-list-row__title', upper(p.title)), p.caption ? L('vb-t-caption vb-list-row__caption', p.caption) : null,
        p.bar ? h(ProgressBar, Object.assign({ className: 'vb-list-row__bar' }, p.bar)) : null),
      p.value ? L(cx('vb-list-row__value', p.valueClass), p.value) : null, p.action ? h('div', { className: 'vb-list-row__trail' }, p.action) : null);
  }
  /** PerkCard, minimal: tier word, glyph, name, effect, its levels as a segmented ProgressBar and the value after the pick. */
  function PerkCard(p) {
    var v = perk(p.view);
    return h(Shape, Object.assign({ 'data-motion-item': v.id, className: cx('vb-perk-card', tier(v), p.selected && 'vb-perk-card--selected', p.className), style: p.style, title: v.name + ' · ' + stacks(v, true) }, clickable(p.onPick)),
      L('vb-t-badge vb-perk-card__tag', upper(v.tag)), h(VbIcon, { icon: v.icon, size: 28, className: 'vb-perk-card__glyph' }),
      L('vb-perk-card__name', upper(v.name)), L('vb-t-caption vb-perk-card__effect', v.effect), h('div', { className: 'vb-grow' }),
      v.max > 0 ? h(ProgressBar, { segments: v.max, value: v.stack, next: v.stack < v.max, className: 'vb-perk-card__levels' }) : null,
      L('vb-perk-card__next', v.next));
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
        return h(KeyValueRow, { key: i, label: r[0], value: r[1], good: r[2] === '--good', className: cx('vb-perk-detail__row', i === rows.length - 1 && 'vb-perk-detail__row--last') });
      })));
  }

  function PerkChip(p) {
    var v = perk(p.view);
    var pop = usePopover(function () { return h(PerkDetail, { view: v, draft: false, className: 'vb-perk-chip__popover' }); }, 'below', !!p.alignEnd, p.pinned);
    return h(Shape, Object.assign({ ref: pop.ref, className: cx('vb-perk-chip', tier(v), pop.open && 'vb-popover-open', p.className), tabIndex: 0, title: v.name + ' · ' + v.now }, pop.handlers),
      h(VbIcon, { icon: v.icon, size: 14 }), L('vb-perk-chip__name', upper(v.name)), L('vb-perk-chip__count', '×' + v.stack, show(v.stack > 1)), pop.layer);
  }

  function PerkRow(p) {
    var v = perk(p.view);
    return h(ListRow, { className: cx('vb-perk-row', tier(v), p.className), style: p.style, selected: p.selected, onPick: p.onPick || function () { }, icon: v.icon, title: v.name, caption: v.tag + ' · ' + v.effect, value: v.now,
      bar: v.max > 0 ? { segments: v.max, value: v.stack } : null });
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
    useIsoLayout(function () {
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
    var init = Object.assign({ index: 1, conditions: [], any: false, action: '', actionRest: '', cost: 0, enabled: true, warning: false, draft: false, note: '' }, p.view);
    var e = R.useState(init.enabled), on = e[0], set = e[1]; R.useEffect(function () { set(init.enabled); }, [init.enabled]);
    function toggle() { set(!on); if (p.onToggle) p.onToggle(!on); }
    var cond = [h('div', { key: 'k', className: 'vb-l vb-rule__keyword' }, 'IF')];
    init.conditions.forEach(function (c, i) {
      if (i) cond.push(h('div', { key: 'j' + i, className: 'vb-l vb-rule__join' }, init.any ? 'OR' : 'AND'));
      cond.push(h('div', { key: 't' + i, className: 'vb-l vb-rule__text' }, c.text)); if (c.value) cond.push(h('div', { key: 'v' + i, className: 'vb-l vb-rule__value' }, c.value)); if (c.after) cond.push(h('div', { key: 'a' + i, className: 'vb-l vb-rule__text' }, c.after));
    });
    return h(ListRow, { className: cx('vb-rule', !on && 'vb-rule--off', init.warning && 'vb-rule--warning', init.draft && 'vb-rule--draft', p.className), style: p.style, lead: String(init.index),
      title: 'THEN ' + init.action + (init.actionRest ? ' ' + init.actionRest : ''), caption: init.note, value: init.cost > 0 ? '−' + init.cost : '', valueClass: 'vb-rule__cost',
      action: h('div', { title: on ? 'Turn rule off' : 'Turn rule on' }, h(VbSwitch, { value: on, onChange: function (x) { if (x !== on) toggle(); }, className: 'vb-rule__toggle vb-switch--compact' })) },
      h('div', { className: 'vb-rule__if' }, cond));
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
      h('div', { className: 'vb-route__roads' }, [v.left || {}, v.right || {}].map(function (road, side) { return h(Shape, Object.assign({ key: side, 'data-motion-item': side, className: 'vb-route__sign vb-route__sign--' + (road.kind || 'combat') }, clickable(function () { if (!locked.current && p.onChoose) { locked.current = true; p.onChoose(side); } })), L('vb-t-badge vb-route__direction', side ? 'RIGHT ROAD →' : '← LEFT ROAD'), h(VbIcon, { icon: road.icon || 'battle', size: 36, className: 'vb-route__glyph' }), L('vb-route__kind', upper(road.title)), L('vb-route__hint', road.hint), h('div', { className: 'vb-grow' }), L('vb-route__confirm', 'TAKE THIS ROAD')); })));
  }
  /** MerchantScreen, minimal: one list of ListRows (rare finds, stack sales, health trades) and LEAVE MARKET. */
  function MerchantScreen(p) {
    var v = Object.assign({ credits: 0, offers: [], stacks: [], health: [] }, p.view);
    function section(t) { return h(LabelComponent, { key: 's' + t, text: t, className: 'vb-merchant__section' }); }
    function bar(k) { return k.max > 0 ? { segments: k.max, value: k.stack } : null; }
    return h('div', { className: cx('vb-merchant', p.className), style: p.style },
      h('div', { className: 'vb-merchant__head' }, L('vb-t-title', 'NIGHT MARKET'), h(CreditsChip, { credits: v.credits })),
      h(ListLayout, { className: 'vb-merchant__scroll' },
        section('Rare finds'), v.offers.map(function (o, i) { return h(ListRow, { key: o.id || o.perk.id+':'+i, motionId:o.id || o.perk.id+':'+i, className: cx('vb-merchant__offer',tier(o.perk)), title: o.perk.name, caption: o.perk.effect, icon: o.perk.icon, bar: bar(o.perk),
          action: h(VbButton, { title: o.taken ? 'SOLD' : 'BUY ' + o.price, blocked: o.taken ? 'Sold' : v.credits < o.price ? 'Need ' + (o.price - v.credits) : o.blocked, className: 'vb-merchant__buy', onClick: function () { call(p.onBuy)(i); } }) }); }),
        section('Sell stacks'), v.stacks.length ? v.stacks.map(function (k, i) { return h(ListRow, { key: k.perk.id, title: k.perk.name, caption: k.perk.now, icon: k.perk.icon, bar: bar(k.perk), action: h(VbButton, { title: '+' + k.payout, className: 'vb-merchant__sell', onClick: function () { call(p.onSellPerk)(k.perk.id); } }) }); }) : h('div', { key: 'e', className: 'vb-l vb-t-caption vb-list__empty' }, 'Nothing to sell yet.'),
        section('Trade health · 25% max HP'), v.health.map(function (x) { return h(ListRow, { key: 'h' + x.actor, title: x.name, caption: x.hp + ' / ' + x.maxHp + ' HP → ' + Math.max(0, x.hp - x.cost) + ' HP', icon: 'health', bar: { value: x.hp / Math.max(1, x.maxHp) },
          action: h(VbButton, { title: '+' + x.payout, variant: 'danger', blocked: x.blocked, className: 'vb-merchant__sell', onClick: function () { call(p.onSellHealth)(x.actor); } }) }); })),
      h(VbButton, { title: 'LEAVE MARKET', variant: 'primary', icon: 'chevron', className: 'vb-merchant__leave', onClick: p.onLeave }));
  }


  /* ── Text, label, badge, icon button, card (Text.cs, Badge.cs, IconButton.cs, Card.cs) ── */
  /** TextComponent: one type style (display | title | label | badge | body | caption | num) and a tone (ally | enemy | power | muted). */
  function TextComponent(p) {
    var tone = p.tone && kebab(p.tone) !== 'default' ? 'vb-t-' + kebab(p.tone) : null;
    return h('div', { className: cx('vb-l', 'vb-text', 'vb-t-' + kebab(p.typeStyle || 'body'), tone, p.className), style: p.style }, p.text != null ? p.text : p.children);
  }
  /** LabelComponent: the caps eyebrow (size label | badge); uppercases what it is given and keeps 4 under it. */
  function LabelComponent(p) {
    return h(TextComponent, { typeStyle: kebab(p.size) === 'badge' ? 'badge' : 'label', tone: p.tone, className: cx('vb-label', p.className), style: p.style, text: upper(p.text) });
  }
  /** BadgeComponent: a slim chip with a word, an optional glyph and value; tones neutral | ally | enemy | power | warn | inverted. */
  function BadgeComponent(p) {
    var value = p.value == null ? '' : String(p.value);
    return h(Shape, { className: cx('vb-badge', st('vb-badge', p.tone || 'neutral'), p.className), style: p.style, title: p.tooltip },
      p.icon ? h(VbIcon, { icon: p.icon, size: 12, className: 'vb-badge__icon' }) : null,
      p.text ? L('vb-badge__text', upper(p.text)) : null, value ? L('vb-badge__value', value) : null, p.children);
  }
  /** IconButtonComposite: a 44 × 44 chipped square with one glyph (or a short state) and a tooltip; `active` and `quiet` looks. */
  function IconButtonComposite(p) {
    return h(Shape, Object.assign({ className: cx('vb-icon-button', p.active && 'vb-icon-button--active', p.quiet && 'vb-icon-button--quiet', p.className), style: p.style,
      title: p.tooltip, 'aria-label': p.tooltip, 'aria-pressed': p.active == null ? undefined : !!p.active }, clickable(p.onClick)),
      p.icon ? h(VbIcon, { icon: p.icon, size: p.size || 20, className: 'vb-icon-button__icon' }) : null,
      p.text ? L(cx('vb-icon-button__text', p.textClass), p.text) : null);
  }
  /** VbFrame with a kind: slim (containers) or chipped (what you act on). */
  function frameClass(kind) { kind = kebab(kind || ''); return kind === 'slim' ? 'vb-frame--slim' : kind === 'chipped' ? 'vb-frame--chipped' : null; }
  /** CardComponent: slim frame, optional media band, head (title + meta), body (children), foot (actions, right-aligned). */
  function CardComponent(p) {
    var foot = [].concat(p.actions || []).filter(Boolean);
    return h(Shape, { className: cx('vb-card', p.selected && 'vb-card--selected', p.className), style: p.style },
      p.media ? h('div', { className: 'vb-card__media' }, p.media) : null,
      p.title || p.meta ? h('div', { className: 'vb-card__head' }, h(LabelComponent, { text: p.title || '', className: 'vb-card__title' }), p.meta ? L('vb-t-badge vb-card__meta', p.meta) : null) : null,
      h('div', { className: 'vb-card__body' }, p.children),
      foot.length ? h('div', { className: 'vb-card__foot' }, foot) : null);
  }

  /* ── Field, form (Forms.cs) ── */
  /** FieldComposite: a caps label, the input (children) and one help line. */
  function FieldComposite(p) {
    return h('div', { className: cx('vb-field', p.className), style: p.style },
      p.label ? h(LabelComponent, { text: p.label, size: 'badge', className: 'vb-field__label' }) : null,
      h('div', { className: 'vb-field__input' }, p.children),
      p.help ? L('vb-t-caption vb-field__help', p.help) : null);
  }
  /** Ui.Section: a titled fieldset (vb-section) for FormComposite. */
  function Fieldset(p) {
    return h(Shape, { className: cx('vb-panel', 'vb-section', p.className), style: p.style }, h(LabelComponent, { text: p.title, className: 'vb-section__title' }), p.children);
  }
  /** FormComposite: fieldsets (children) in a scroll, an optional `note` { icon, text }, a `footer` of actions and a `saved` line. */
  function FormComposite(p) {
    var footer = [].concat(p.footer || []).filter(Boolean);
    return h('div', { className: cx('vb-form', p.className), style: p.style },
      p.saved && !p.headed ? L('vb-t-badge vb-form__status', p.saved) : null,
      h('div', { className: 'vb-form__scroll vb-page__scroll', style: { overflowY: 'auto' } },
        h('div', { className: 'vb-u-scroll-view__content-container' }, p.children,
          p.note ? h(Shape, { className: 'vb-panel vb-panel--raised vb-form__note' }, h(VbIcon, { icon: p.note.icon || 'info', size: 16 }), L('vb-t-caption', p.note.text)) : null)),
      footer.length ? h('div', { className: 'vb-form__footer' }, footer) : null);
  }

  /* ── Layouts (Layouts.cs) ── */
  function StackLayout(p) { var g = p.gap <= 4 ? 4 : p.gap <= 8 || p.gap == null ? 8 : p.gap <= 12 ? 12 : 16; return h('div', { className: cx('vb-stack', 'vb-stack--' + g, p.className), style: p.style }, p.children); }
  function ListLayout(p) {
    var kids = R.Children.toArray(p.children);
    return h('div', { className: cx('vb-list-scroll vb-page__scroll', p.className), style: Object.assign({ overflowY: 'auto' }, p.style) },
      p.empty && !kids.length ? L('vb-t-caption vb-list__empty', p.empty) : null, h('div', { className: 'vb-list' }, kids));
  }
  function VbGrid(p) { var n = p.columns <= 2 ? 2 : p.columns >= 4 || p.columns == null ? 4 : 3; return h('div', { className: cx('vb-grid', 'vb-grid--' + n, p.wrap && 'vb-grid--wrap', p.className), style: p.style }, p.children); }
  var BoardLayout=R.forwardRef(function BoardLayout(p,ref) {
    return h('div', { ref:ref, className: cx('vb-board', p.className), style: p.style },
      h('div', { className: 'vb-board__top' }, p.top), h('div', { className: 'vb-board__world' }, p.world), h('div', { className: 'vb-board__bottom' }, p.bottom));
  });

  /* ── Bottom drawer (Drawer.cs) ── */
  /** DrawerOverlay: a slim sheet that slides up over a backdrop to half the screen. Drag the grip or head: up snaps to full screen,
      down back to half or, below a quarter, closes it; a tap on the grip toggles half/full. The backdrop and × dismiss it too.
      `open` (default true) slides it in and out; `snap` 'half' | 'full' is where it opens; `backdrop: false` docks it in a view. */
  function DrawerOverlay(p) {
    var want = p.open !== false, dismissible = p.dismissible !== false, hasBackdrop = p.backdrop !== false, closeButton = p.closeButton !== false && dismissible;
    var sh = R.useState(false), shown = sh[0], setShown = sh[1], sn = R.useState(p.snap || 'half'), snap = sn[0], setSnapState = sn[1];
    var dh = R.useState(null), dragH = dh[0], setDragH = dh[1], live = R.useState(want), mounted = live[0], setMounted = live[1];
    var root = R.useRef(null), sheet = R.useRef(null), d = R.useRef(null);
    function setSnap(v) { setSnapState(v); if (p.onSnap) p.onSnap(v); }
    var closing=R.useRef(false),release=R.useRef(null),focusBefore=R.useRef(null),serial=R.useRef(0);
    function exit(){if(closing.current)return;closing.current=true;var id=++serial.current;playMotion(sheet.current,'drawerOut',0,true).then(function(){if(id!==serial.current)return;setShown(false);setMounted(false);setDragH(null);closing.current=false;if(release.current){release.current();release.current=null;}if(focusBefore.current&&focusBefore.current.isConnected)focusBefore.current.focus();});}
    useIsoLayout(function(){if(!mounted||!root.current)return;if(!release.current&&hasBackdrop){release.current=coverSiblings(root.current);focusBefore.current=document.activeElement;}var id=++serial.current;closing.current=false;setShown(true);playMotion(sheet.current,'drawerIn');if(hasBackdrop&&sheet.current)sheet.current.focus();return function(){if(id===serial.current)serial.current++;};},[mounted]);
    R.useEffect(function(){if(want){serial.current++;closing.current=false;setMounted(true);setSnapState(p.snap||'half');if(mounted){setShown(true);playMotion(sheet.current,'drawerIn');}}else if(mounted)exit();},[want]);
    R.useEffect(function(){return function(){serial.current++;if(release.current)release.current();var a=sheet.current&&runningMotion.get(sheet.current);if(a)a.cancel();};},[]);
    function dismiss(){if(!dismissible||closing.current)return false;exit();if(p.onDismiss)p.onDismiss();return true;}
    function drag(isGrip) {
      return {
        onPointerDown: function (e) { if (!shown) return; d.current = { y: e.clientY, h: sheet.current.offsetHeight, moved: false, grip: isGrip }; e.currentTarget.setPointerCapture(e.pointerId); },
        onPointerMove: function (e) {
          var g = d.current; if (!g) return; var dy = e.clientY - g.y; if (Math.abs(dy) > 4) g.moved = true; if (!g.moved) return;
          var full = root.current.offsetHeight; setDragH(Math.max(dismissible ? 0 : full / 2, Math.min(full, g.h - dy)));
        },
        onPointerUp: function () {
          var g = d.current; d.current = null; if (!g) return;
          if (!g.moved) { if (g.grip) setSnap(snap === 'full' ? 'half' : 'full'); setDragH(null); return; }
          var full = root.current.offsetHeight, hgt = dragH == null ? g.h : dragH;
          if (hgt < full * 0.25 && dismiss()) return;   // keep the let-go height while it slides away
          setSnap(hgt < full * 0.75 ? 'half' : 'full'); setDragH(null);
        },
        onPointerCancel: function () { d.current = null; setDragH(null); }
      };
    }
    if (!mounted && !want) return null;
    var dragging = dragH != null && !!d.current, head = p.title || p.meta || closeButton, foot = [].concat(p.actions || []).filter(Boolean);
    return h('div', { ref: root, 'data-overlay-root': '', className: cx('vb-drawer', shown && 'vb-drawer--open', snap === 'full' && 'vb-drawer--full', dragging && 'vb-drawer--dragging', !hasBackdrop && 'vb-drawer--inline', p.className), style: Object.assign({ pointerEvents: hasBackdrop && mounted ? 'auto' : 'none' }, p.style) },
      hasBackdrop ? h('div', { className: 'vb-drawer__backdrop', onPointerDown: dismiss }) : null,
      h(Shape, { ref: sheet, className: 'vb-drawer__sheet', role: 'dialog', tabIndex: -1, onKeyDown:function(e){if(e.key==='Escape'){e.stopPropagation();dismiss();}if(e.key==='Tab'&&hasBackdrop){var nodes=Array.from(sheet.current.querySelectorAll('button:not(:disabled),input,select,textarea,[tabindex="0"]'));if(!nodes.length){e.preventDefault();return;}var first=nodes[0],last=nodes[nodes.length-1];if(e.shiftKey&&(document.activeElement===first||document.activeElement===sheet.current)){e.preventDefault();last.focus();}else if(!e.shiftKey&&(document.activeElement===last||document.activeElement===sheet.current)){e.preventDefault();first.focus();}}}, 'aria-modal': hasBackdrop || undefined, 'aria-label': p.title || p.label, style: Object.assign({ pointerEvents: 'auto' }, dragH != null ? { height: dragH } : null) },
        h('div', Object.assign({ className: 'vb-drawer__grip', role: 'button', tabIndex: 0, 'aria-label': snap === 'full' ? 'Half screen' : 'Full screen', title: 'Drag up for full screen, down to close', style: { touchAction: 'none', cursor: 'grab' },
          onKeyDown: function (e) { if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); setSnap(snap === 'full' ? 'half' : 'full'); } } }, drag(true)), h('div', { className: 'vb-drawer__handle' })),
        head ? h('div', Object.assign({ className: 'vb-drawer__head', style: { touchAction: 'none' } }, drag(false)),
          p.title ? L('vb-t-title vb-drawer__title', upper(p.title)) : h('div', { className: 'vb-drawer__title' }), p.meta ? L('vb-t-badge vb-drawer__meta', p.meta) : null,
          closeButton ? h('div', { onPointerDown: function (e) { e.stopPropagation(); } }, h(IconButtonComposite, { icon: 'close', tooltip: 'Close', className: 'vb-drawer__close', onClick: dismiss })) : null) : null,
        h('div', { className: 'vb-drawer__body vb-page__scroll', style: { overflowY: 'auto' } }, p.children),
        foot.length ? h('div', { className: 'vb-drawer__foot' }, foot) : null));
  }

  /* ── Views (Runtime/Views) ── */
  function View(block, p, kids, headed) { return h(MotionSurface, { className: cx('vb-view', block, headed && 'vb-view--headed', p.className), style: p.style }, kids); }
  function call(fn) { return function () { if (fn) fn.apply(null, arguments); }; }
  /** MenuActionsSectionView: checkpoint card (when there is one), PLAY, then HIGH SCORES and SETTINGS. */
  function MenuActionsSectionView(p) {
    var c = p.checkpoint;
    return h('div', { className: cx('vb-menu-actions', p.className), style: p.style },
      c ? h(CardComponent, { title: 'CHECKPOINT', meta: 'WAVE ' + pad2(c.wave), className: 'vb-menu__checkpoint', actions: h(VbButton, { title: 'RESUME', icon: 'play', onClick: call(p.onResume) }) },
        L('vb-menu__checkpoint-line', 'Wave ' + pad2(c.wave) + ' next · ' + num(c.score)),
        L('vb-t-caption', (c.standing == null ? 4 : c.standing) + ' of ' + (c.squadSize || 4) + ' standing · ' + (c.perks || 0) + (c.perks === 1 ? ' perk' : ' perks') + (c.seed ? ' · seed ' + c.seed : ''))) : null,
      h(VbButton, { title: 'PLAY', variant: 'primary', sub: p.squadLine || '', icon: 'chevron', className: 'vb-menu__play', onClick: call(p.onPlay) }),
      h('div', { className: 'vb-row vb-menu__row' }, h(VbButton, { title: 'HIGH SCORES', onClick: call(p.onScores) }), h(VbButton, { title: 'SETTINGS', icon: 'settings', onClick: call(p.onSettings) })),
      p.notice ? L('vb-t-caption vb-menu__notice', p.notice) : null);
  }
  /** MainMenuView: the offline badge and best, the mark, the menu actions and EXIT. `view`: MainMenuViewModel. */
  function MainMenuView(p) {
    var v = Object.assign({ status: 'OFFLINE · LOCAL SCORES', subtitle: 'ARCADE', notice: '', best: 0, bestWave: 0, checkpoint: null, squad: [], canExit: true }, p.view);
    return View('vb-menu', p, [
      h('div', { key: 't', className: 'vb-menu__top' }, L('vb-t-badge', v.status), v.best > 0 ? h(BadgeComponent, { text: 'BEST', value: num(v.best), className: 'vb-menu__best' }, v.bestWave ? L('vb-t-badge vb-t-ally vb-menu__best-wave', 'WAVE ' + v.bestWave) : null) : null),
      h('div', { key: 'b', className: 'vb-menu__brand' }, h(VbMark, { width: 240 }), v.subtitle ? L('vb-menu__arcade', upper(v.subtitle)) : null),
      h('div', { key: 'g', className: 'vb-grow' }),
      h(MenuActionsSectionView, { key: 'a', checkpoint: v.checkpoint, squadLine: v.squad.length ? 'Squad · ' + v.squad.join(' · ') : 'Choose a squad', notice: v.notice, onPlay: p.onPlay, onResume: p.onResume, onScores: p.onScores, onSettings: p.onSettings }),
      v.canExit ? h(VbButton, { key: 'x', title: 'EXIT', variant: 'ghost', className: 'vb-menu__exit', onClick: call(p.onExit) }) : null]);
  }
  /** SquadView: four slots, the roster, the open details and START RUN (blocked until four). `view`: SquadViewModel. */
  function SquadView(p) {
    var v = Object.assign({ roster: [], selected: [-1, -1, -1, -1], focus: -1, locked: [], tiers: [], seed: 'new seed' }, p.view);
    var picked = v.selected.filter(function (i) { return i >= 0 && i < v.roster.length; }), n = picked.length;
    var unlocked = v.roster.filter(function (_, i) { return !v.locked[i]; }).length, open = v.focus >= 0 && v.focus < v.roster.length;
    var shownFocus = R.useRef(-1); if (open) shownFocus.current = v.focus; var fi = shownFocus.current;  // keep the details while the drawer slides away
    return View('vb-squad-view', p, [
      h(ScreenHeader, { key: 'h', title: 'SQUAD', meta: n + ' / 4', accent: n === 4, onBack: p.onBack }),
      h(LabelComponent, { key: 'l1', text: 'Squad · tap a slot for details', className: 'vb-page__label' }),
      h('div', { key: 's', className: 'vb-grid4' }, [0, 1, 2, 3].map(function (i) { var r = v.selected[i]; return h(SquadSlot, { key: i, slot: i + 1, view: r >= 0 ? v.roster[r] : null, onPick: function () { call(p.onSlotPick)(i); } }); })),
      h(LabelComponent, { key: 'l2', text: unlocked === v.roster.length ? 'Roster · all ' + v.roster.length + ' unlocked' : 'Roster · ' + unlocked + ' of ' + v.roster.length + ' unlocked', className: 'vb-page__label' }),
      h('div', { key: 'r', className: 'vb-grid4 vb-grid4--wrap' }, v.roster.map(function (o, i) { return h(OperativeTile, { key: o.id, view: o, slot: v.selected.indexOf(i) + 1, focused: i === v.focus, tier: v.tiers[i], locked: v.locked[i], onPick: function () { call(p.onPick)(i); } }); })),
      h('div', { key: 'g', className: 'vb-grow' }),
      h(VbButton, { key: 'go', title: 'START RUN', variant: 'primary', icon: 'chevron', className: 'vb-squad-view__start', sub: n ? picked.map(function (i) { return v.roster[i].name; }).join(' · ') + (v.seed ? ' · ' + v.seed : '') : '', blocked: n < 4 ? 'Pick ' + (4 - n) + ' more' : '', onClick: call(p.onStart) }),
      h(DrawerOverlay, { key: 'd', open: open, closeButton: false, className: 'vb-squad-view__drawer', label: 'Operative details', onDismiss: p.onClose },
        fi >= 0 && v.roster[fi] ? h(OperativeDetails, { view: v.roster[fi], slot: v.selected.indexOf(fi) + 1, squadFull: n >= 4, onClose: p.onClose, onToggle: function () { call(p.onToggle)(v.focus); } }) : null)], true);
  }
  /** SettingsView: AUDIO, FEEL, CAMERA, QUALITY in a FormComposite; applies as it changes (`onChange(model)`). */
  function SettingsView(p) {
    var init = Object.assign({ master: 0.8, music: 0.55, effects: 0.8, sensitivity: 0.5, haptics: true, reducedMotion: false, camera: 0, quality: 1, saved: 'SAVED', note: 'Autoplay lives in the battle HUD and starts on for every new run.' }, p.view);
    var s = R.useState(init), v = s[0];
    function set(k) { return function (x) { var n = Object.assign({}, v); n[k] = x; s[1](n); if (p.onChange) p.onChange(n); }; }
    return View('vb-settings', p, [
      h(ScreenHeader, { key: 'h', title: 'SETTINGS', meta: v.saved, onBack: p.onBack }),
      h(FormComposite, { key: 'f', headed: true, note: v.note ? { icon: 'automation', text: v.note } : null,
        footer: [h(VbButton, { key: 'c', title: 'CLEAR LOCAL SCORES', variant: 'danger', onClick: call(p.onClearScores) }), h(VbButton, { key: 'a', title: 'ACCESSIBILITY', variant: 'ghost', onClick: call(p.onAccessibility) })] },
        h(Fieldset, { key: 1, title: 'AUDIO' }, h(VbSlider, { label: 'Master', value: v.master, onChange: set('master') }), h(VbSlider, { label: 'Music', value: v.music, onChange: set('music') }), h(VbSlider, { label: 'Effects', value: v.effects, onChange: set('effects') })),
        h(Fieldset, { key: 2, title: 'FEEL' }, h(VbSwitch, { text: 'HAPTICS', detail: 'Light pulse when an order is accepted', value: v.haptics, onChange: set('haptics') }), h(VbSwitch, { text: 'REDUCED MOTION', detail: 'No camera shake · fades replace flicker · flat panels', value: v.reducedMotion, onChange: set('reducedMotion') })),
        h(Fieldset, { key: 3, title: 'CAMERA' }, h(VbSegmented, { items: [{ label: 'FULL', sub: 'Parallax' }, { label: 'REDUCED', sub: 'No drift' }, { label: 'OFF', sub: 'Fixed' }], selected: v.camera, onChange: set('camera') }), h(VbSlider, { label: 'Sensitivity', value: v.sensitivity, onChange: set('sensitivity') }), L('vb-t-caption', 'Pinch to zoom, twist to turn, drag empty ground to pan, double-tap to reset.')),
        h(Fieldset, { key: 4, title: 'QUALITY' }, h(VbSegmented, { items: [{ label: 'LOW', sub: 'Stable 30 FPS' }, { label: 'STANDARD', sub: '60 FPS target' }], selected: v.quality, onChange: set('quality') }), L('vb-t-caption', 'Applies on the next wave. Telegraphs stay fully visible on both tiers.')))], true);
  }
  /** PauseOverlayView: a ScreenHeader whose back square resumes, then one overlay panel centred on the screen. `view`: PauseViewModel. */
  function PauseOverlayView(p) {
    var v = Object.assign({ wave: 1, score: 0, standing: 4, squadSize: 4, perks: [], seed: '', detail: '' }, p.view);
    var line = [v.seed ? 'Seed ' + v.seed : '', v.detail].filter(Boolean).join(' · ');
    return View('vb-pause', p, [
      h(ScreenHeader, { key: 'h', title: 'PAUSED', meta: 'WAVE ' + pad2(v.wave), backLabel: 'Resume', onBack: p.onResume }),
      h('div', { key: 'c', className: 'vb-pause__center' }, L('vb-t-caption vb-pause__caption', 'Combat, focus and world travel are frozen'),
        h(Dialog, { className: 'vb-pause__panel' },
          h('div', { className: 'vb-row vb-pause__stats' }, h(StatReadout, { label: 'WAVE', value: pad2(v.wave) }), h(StatReadout, { label: 'SCORE', value: num(v.score) }), h(StatReadout, { label: 'STANDING', value: String(v.standing), unit: '/' + v.squadSize })),
          v.perks.length ? h(Shape, Object.assign({ className: 'vb-panel vb-frame--slim vb-frame--raised vb-pause__perks', title: 'Active perks' }, clickable(p.onPerks)),
            h('div', { className: 'vb-row vb-row--between' }, h(LabelComponent, { text: 'Active perks · ' + v.perks.length }), h('div', { className: 'vb-row' }, L('vb-t-badge vb-t-ally', 'DETAILS'), h(VbIcon, { icon: 'chevron', size: 16, className: 'vb-pause__more' }))),
            h('div', { className: 'vb-row vb-row--wrap vb-pause__chips' }, v.perks.map(function (k, i) { return h(BadgeComponent, { key: i, icon: k.icon, text: k.name + (k.stack > 1 ? ' ×' + k.stack : ''), className: 'vb-pause__chip' }); }))) : null,
          line ? L('vb-t-caption', line) : null,
          h(VbButton, { title: 'RESUME', variant: 'primary', sub: 'Returns to selected speed', icon: 'chevron', onClick: call(p.onResume) }),
          h('div', { className: 'vb-row vb-pause__row' }, h(VbButton, { title: 'SETTINGS', icon: 'settings', className: 'vb-half vb-pause__button', onClick: call(p.onSettings) }),
            h(VbButton, { title: 'ABANDON', variant: 'danger', icon: 'remove', className: 'vb-half vb-pause__button vb-pause__abandon', onClick: call(p.onAbandon) })),
          h(VbButton, { title: 'EXIT TO MENU', variant: 'ghost', icon: 'back', className: 'vb-pause__exit', onClick: call(p.onExit) })))], true);
  }
  /** ConfirmDialogView: the question, its consequence, the destructive action and the way back. */
  function ConfirmDialogView(p) {
    var c = p.confirm || {}, k = p.cancel || {};
    return h('div', { className: cx('vb-confirm', p.className), style: p.style }, h(Dialog, { title: p.title, body: p.body },
      h(VbButton, { title: c.label || 'CONFIRM', variant: c.variant || 'danger', sub: c.sub, icon: c.icon, onClick: call(p.onConfirm) }),
      h(VbButton, { title: k.label || 'CANCEL', icon: k.icon, onClick: call(p.onCancel) })));
  }
  var GUIDE_STEPS = ['Drag an operative to move.', 'Drag A or S onto the battlefield to aim; release over the cards to cancel.', 'Auto controls tactics. Tap AUTO to take over.', '2× speeds combat; aiming stays slow while focus lasts.', 'Stop enemies before they pass the rear line.'];
  /** GuideDialogView: numbered first-run steps and READY. */
  function GuideDialogView(p) {
    return h('div', { className: cx('vb-guide', p.className), style: p.style }, h(Dialog, { title: p.title || 'RUN THE BOULEVARD' },
      h(StackLayout, { gap: 8, className: 'vb-guide__steps' }, (p.steps || GUIDE_STEPS).map(function (t, i) { return h('div', { key: i, className: 'vb-row vb-guide__step' }, h(BadgeComponent, { text: String(i + 1), tone: 'ally', className: 'vb-guide__index' }), L('vb-t-body vb-guide__text', t)); })),
      h(VbButton, { title: 'READY', variant: 'primary', icon: 'check', onClick: call(p.onReady) })));
  }
  /** PerkDraftView: three cards, the pick explained, the squad that carries forward and TAKE PERK. `view`: PerkDraftViewModel. */
  function PerkDraftView(p) {
    var v = Object.assign({ wave: 1, bonus: 0, score: 0, choices: [], pick: 0, squad: [] }, p.view), pick = v.choices[v.pick];
    return View('vb-draft', p, [
      h('div', { key: 's', className: 'vb-page__scroll vb-draft__scroll', style: { overflowY: 'auto' } },
        h(LabelComponent, { text: 'Wave ' + pad2(v.wave) + ' cleared', tone: 'ally' }), L('vb-t-display', 'CHOOSE A PERK'),
        L('vb-t-caption vb-page__caption', '+' + num(v.bonus) + ' wave bonus · score ' + num(v.score) + ' · perks last the whole run'),
        h('div', { className: 'vb-draft__cards' }, v.choices.map(function (c, i) { return h(PerkCard, { key: c.id, view: c, selected: i === v.pick, onPick: function () { call(p.onPick)(i); } }); })),
        pick ? h(PerkDetail, { className: 'vb-draft__detail', view: pick, draft: true }) : null,
        h(Shape, { className: 'vb-panel vb-frame--slim vb-draft__squad' }, h(LabelComponent, { text: 'Carries forward · no healing between waves', size: 'badge' }),
          h('div', { className: 'vb-grid4' }, v.squad.map(function (a, i) {
            return h('div', { key: i, className: 'vb-draft__cell' }, h('div', { className: 'vb-row vb-row--between' }, L('vb-draft__code', a.code), L('vb-t-badge', a.hp > 0 ? Math.round(100 * a.hp / Math.max(1, a.maxHp)) + '%' : 'OUT')),
              h(HealthBar, { hp: a.hp, max: a.maxHp, stability: a.stability == null ? 1 : a.stability }));
          })))),
      h(VbButton, { key: 'b', title: 'TAKE PERK', variant: 'primary', icon: 'check', sub: pick ? pick.name + ' → ' + pick.next + ' · then wave ' + pad2(v.wave + 1) + ', focus refills' : '', blocked: pick ? '' : 'Pick a perk', onClick: call(p.onTake) })]);
  }
  /** ResultsView: the score and its ledger, stats, the squad's contribution, the perks, MENU and RUN AGAIN. `view`: ResultsViewModel. */
  function ResultsView(p) {
    var v = Object.assign({ cause: 'SQUAD DOWN', score: 0, wave: 0, bosses: 0, rank: 0, of: 20, ledger: [], distance: '', simTime: '', squad: [], perks: [] }, p.view), d = (v.distance || '').split(' ');
    return View('vb-results', p, [
      h('div', { key: 's', className: 'vb-page__scroll vb-results__scroll', style: { overflowY: 'auto' } },
        h(LabelComponent, { text: v.cause + ' · wave ' + v.wave, tone: 'enemy' }), L('vb-t-display', 'RUN OVER'),
        h(Shape, { className: 'vb-panel vb-frame--slim vb-frame--solid vb-results__score' },
          h('div', { className: 'vb-row vb-row--between' }, h('div', null, h(LabelComponent, { text: 'Score' }), L('vb-results__value', num(v.score))), v.rank > 0 ? L('vb-results__rank', '#' + v.rank + ' OF ' + v.of) : null),
          h('div', { className: 'vb-results__ledger' }, v.ledger.map(function (l, i) { return h(KeyValueRow, { key: i, label: l.label, value: l.value }); })),
          h('div', { className: 'vb-row vb-results__stats' }, h(StatReadout, { label: 'WAVES', value: String(v.wave) }), h(StatReadout, { label: 'BOSSES', value: String(v.bosses) }),
            v.distance ? h(StatReadout, { label: 'DISTANCE', value: d[0], unit: d[1] }) : null, v.simTime ? h(StatReadout, { label: 'SIM TIME', value: v.simTime }) : null)),
        h(LabelComponent, { text: 'Squad · hover or tap for contribution', className: 'vb-page__label' }),
        h('div', { className: 'vb-grid4' }, v.squad.map(function (c, i) { return h(SquadResult, { key: c.operative.id, view: c }); })),
        v.perks.length ? h(LabelComponent, { text: 'Perks · ' + v.perks.length, className: 'vb-page__label' }) : null,
        h('div', { className: 'vb-row vb-row--wrap vb-results__perks' }, v.perks.map(function (k, i) { return h(PerkChip, { key: k.id, view: k }); }))),
      h('div', { key: 'r', className: 'vb-row vb-results__row' }, h(VbButton, { title: 'MENU', className: 'vb-results__menu', onClick: call(p.onMenu) }), h(VbButton, { title: 'RUN AGAIN', variant: 'primary', icon: 'chevron', className: 'vb-results__again', onClick: call(p.onAgain) }))]);
  }
  /** HighScoresView: up to 20 ranked runs (current framed) and older-rules runs after a label, in a ListLayout. */
  function HighScoresView(p) {
    var ranked = (p.ranked || []).slice(0, 20), old = p.historical || [];
    return View('vb-scores', p, [
      h(ScreenHeader, { key: 'h', title: 'HIGH SCORES', meta: 'TOP ' + ranked.length, onBack: p.onBack }),
      h('div', { key: 'c', className: 'vb-l vb-t-caption vb-page__caption' }, p.caption || 'On this device. Ties break by wave, then shortest sim time.'),
      h(ListLayout, { key: 'l', empty: 'No runs yet. Your first run lands here.' }, ranked.map(function (s, i) { return h(ScoreRow, { key: 'r:'+(s.id||s.rank), view: s }); }),
        old.length ? h(LabelComponent, { key: 'o', text: 'Older rules', className: 'vb-scores__older' }) : null,
        old.map(function (s, i) { return h(ScoreRow, { key: 'h:'+(s.id||s.rank), view: s, className: 'vb-scores__historical' }); })),
      h('div', { key: 'f', className: 'vb-l vb-t-caption vb-page__footer' }, 'Local scores only · not an online ranking')], true);
  }
  function RouteChoiceView(p) { return View('vb-route-view', p, h(RouteChoice, { view: p.view, onChoose: p.onChoose })); }
  function MerchantScreenView(p) { return View('vb-merchant-view', p, h(MerchantScreen, { view: p.view, onBuy: p.onBuy, onSellPerk: p.onSellPerk, onSellHealth: p.onSellHealth, onLeave: p.onLeave })); }
  /** BattleHudView: BoardLayout with the HUD band, boss bar and announcer on top, `world` in the middle and four cards at the bottom. */
  function BattleHudView(p) {
    var ops = p.operatives || [],boardRef=R.useRef(null),generation=R.useRef(0);
    useIsoLayout(function(){var board=boardRef.current;if(!board)return;var nodes=[board.querySelector('.vb-board__top')].concat(Array.from(board.querySelectorAll('.vb-battle__card'))),id=++generation.current;
      nodes.forEach(function(node,i){if(!node)return;var recipeName=i?'hudCard':'hudTop';node.style.visibility='';if(p.reveal!=null){var v=clamp01(p.reveal),recipe=MOTION.recipes[recipeName],away=1-v;node.style.opacity=v;node.style.translate=reducedMotion(node)?'0 0':(recipe.x||0)*away+'px '+(recipe.y||0)*away+'px';node.style.rotate=reducedMotion(node)?'0deg':(recipe.rotation||0)*away+'deg';node.inert=v<=0;return;}node.style.opacity='';node.style.translate='';node.style.rotate='';node.inert=p.shown===false;playMotion(node,recipeName,i?i-1:0,p.shown===false).then(function(done){if(done&&id===generation.current&&p.shown===false)node.style.visibility='hidden';});});
      return function(){generation.current++;nodes.forEach(function(node){var animation=node&&runningMotion.get(node);if(animation)animation.cancel();});};
    },[p.shown,p.reveal]);
    return h(BoardLayout, { ref:boardRef, className: cx('vb-battle', p.className), style: p.style,
      top: [h(HudTop, { key: 'h', view: p.hud, onPauseClick: p.onPause, onAutoClick: p.onAuto, onSpeedClick: p.onSpeed, onQueueCancel: p.onQueueCancel, queuePinned: p.queuePinned }),
        p.boss ? h(BossBar, { key: 'b', view: p.boss, className: 'vb-battle__boss' }) : null,
        h('div', { key: 'a', className: 'vb-battle__announcer vb-announcement' }, p.announcement ? h(GlitchText, { key: p.announcement, text: p.announcement }) : null)],
      world: p.world,
      bottom: h('div', { className: 'vb-battle__cards' }, ops.slice(0, 4).map(function (o, i) { return o ? h(OperativeCard, { key: o.id||i, view: o, className: 'vb-battle__card' }) : null; })) });
  }

  function ActivePerksView(p){var v=Object.assign({wave:1,perks:[],totals:[],pick:0},p.view);return View('vb-build',p,[h(ScreenHeader,{key:'h',title:'ACTIVE PERKS',meta:'WAVE '+pad2(v.wave),onBack:p.onBack}),h('dl',{key:'t',className:'vb-build__summary'},v.totals.map(function(t){return h(R.Fragment,{key:t.label},h('dt',null,t.label),h('dd',null,t.value));})),h(ListLayout,{key:'l',empty:'No perks yet.'},v.perks.map(function(e,i){return h(PerkRow,{key:e.id,view:e,selected:i===v.pick,onPick:function(){call(p.onPick)(i);}});})),v.perks[v.pick]?h(PerkDetail,{key:'d',view:v.perks[v.pick]}):null],true);}
  function RevivePickView(p){var fallen=p.fallen||[];return View('vb-revive',p,[h(ScreenHeader,{key:'h',title:'RESTORE',meta:'CHOOSE ONE',onBack:p.onBack}),h('p',{key:'c',className:'vb-page__caption'},(p.perk||'Restore')+' revives one fallen operative at 35% health.'),h('div',{key:'g',className:'vb-grid4 vb-grid4--wrap'},fallen.map(function(e,i){return h(OperativeTile,{key:e.id,view:e,onPick:function(){call(p.onPick)(i);}});}))],true);}
  function AccessibilityView(p){var v=Object.assign({reducedFlashes:false,largerText:false,highContrast:false},p.view);return View('vb-accessibility',p,[h(ScreenHeader,{key:'h',title:'ACCESSIBILITY',meta:'SAVED',onBack:p.onBack}),h(FormComposite,{key:'f'},['reducedFlashes','largerText','highContrast'].map(function(key){return h(VbSwitch,{key:key,text:{reducedFlashes:'Reduced flashes',largerText:'Larger text',highContrast:'High contrast'}[key],value:v[key],onChange:function(value){call(p.onChange)(key,value);}});}))],true);}

  var api = {
    EffectDetails: EffectDetails, EffectBadge: EffectBadge, EffectList: EffectList, EffectStrip: EffectStrip,
    ActivePerksView: ActivePerksView, RevivePickView: RevivePickView, AccessibilityView: AccessibilityView,
    GlitchText: GlitchText, CreditsChip: CreditsChip, RouteChoice: RouteChoice, MerchantScreen: MerchantScreen,
    VbFrame: VbFrame, VbIcon: VbIcon, VbMark: VbMark, VbButton: VbButton, StatReadout: StatReadout, StatusChip: StatusChip,
    VbSwitch: VbSwitch, VbSlider: VbSlider, VbSegmented: VbSegmented, VbMeter: VbMeter, HealthBar: HealthBar, PowerStrip: PowerStrip,
    FocusMeter: FocusMeter, SkillButton: SkillButton, QueueChip: QueueChip, HudTop: HudTop, QueueList: QueueList, BossBar: BossBar, ScreenHeader: ScreenHeader,
    ScoreRow: ScoreRow, OperativeCard: OperativeCard, OperativeTile: OperativeTile, SquadSlot: SquadSlot, OperativeDetails: OperativeDetails,
    OperativeStats: OperativeStats, SquadResult: SquadResult, PerkCard: PerkCard, PerkDetail: PerkDetail, PerkChip: PerkChip, PerkRow: PerkRow,
    Dialog: Dialog, Backdrop: Backdrop, ScanLines: ScanLines, HoloFrame: HoloFrame, HoloPlatform: HoloPlatform, RevealCard: RevealCard, RuleRow: RuleRow,
    WorldHealthBar: WorldHealthBar, CombatText: CombatText, WorldLabel: WorldLabel, WorldMarker: WorldMarker, OffscreenIndicator: OffscreenIndicator, EdgeWarning: EdgeWarning,
    ProgressBar: ProgressBar, ListRow: ListRow, KeyValueRow: KeyValueRow, TextComponent: TextComponent, LabelComponent: LabelComponent, BadgeComponent: BadgeComponent, IconButtonComposite: IconButtonComposite, TileComponent: TileComponent, CardComponent: CardComponent,
    DrawerOverlay: DrawerOverlay, FieldComposite: FieldComposite, Fieldset: Fieldset, FormComposite: FormComposite, StackLayout: StackLayout, VbGrid: VbGrid, ListLayout: ListLayout, BoardLayout: BoardLayout,
    MainMenuView: MainMenuView, MenuActionsSectionView: MenuActionsSectionView, SquadView: SquadView, SettingsView: SettingsView, PauseOverlayView: PauseOverlayView, ConfirmDialogView: ConfirmDialogView,
    GuideDialogView: GuideDialogView, PerkDraftView: PerkDraftView, ResultsView: ResultsView, HighScoresView: HighScoresView, RouteChoiceView: RouteChoiceView, MerchantScreenView: MerchantScreenView, BattleHudView: BattleHudView,
    VbShape: Shape, format: { compact: compact, number: num, seconds: seconds, kebab: kebab }
  };
  window.VeilbreakInspector.install(api, function (factory) { h = factory; }, schedule);
  window.Veilbreak = Object.assign(window.Veilbreak || {}, api);
})();
