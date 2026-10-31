"""Write project/components/<Name>/preview.html for the kit (data from Samples~/Gallery, Samples~/BattleHud and the design canvas)."""

import os
import pathlib
import sys

OUT = sys.argv[1]

PRELUDE = r"""var V = window.Veilbreak, h = React.createElement;
function cap(t) { return h('div', { className: 'vb-l vb-t-badge', style: { marginTop: 6 } }, t); }
function title(t) { return h('div', { className: 'vb-l vb-t-label', style: { marginBottom: 8 } }, t); }
function row(kids, gap, style) { return h('div', { style: Object.assign({ flexDirection: 'row', alignItems: 'flex-start', gap: gap == null ? 8 : gap, flexWrap: 'wrap' }, style) }, kids); }
function col(kids, gap, style) { return h('div', { style: Object.assign({ gap: gap == null ? 8 : gap }, style) }, kids); }
function cell(node, caption, key) { return h('div', { key: key, style: { alignItems: 'center' } }, node, caption ? cap(caption) : null); }
function render(node) { ReactDOM.createRoot(document.getElementById('root')).render(node); }
function op(o) {
  return Object.assign({ id: 'aria', name: 'Aria', code: 'AR', role: 'Striker', roleIcon: 'striker', kit: 'var(--kit-aria)', hp: 820, maxHp: 1000, barrier: 60, armor: 10, stability: 70, maxStability: 110, mode: 'auto', manualSeconds: 2.4,
    passive: 'Momentum tax', passiveText: 'Basic kinetic hits slow the target by 2%, stacking to 10%.', statuses: ['barrier'],
    skills: [{ label: 'Needle Storm', icon: 'SkillAriaNeedleStorm', cost: 18, slot: 'A', kind: 'Active', description: 'Piercing fan and armor shred', cooldownSeconds: 4 },
             { label: 'Terminal Vector', icon: 'SkillAriaTerminalVector', cost: 40, slot: 'S', kind: 'Signature', state: 'cooldown', charged: 0.4, seconds: 7.9, description: 'Piercing execution beam', cooldownSeconds: 13.2 }] }, o);
}
var PERKS = {  // Samples~/Gallery/GuiGallery.cs
  general: { name: 'Sharpened Output', category: 'Offense', tag: 'General', tier: 'general', icon: 'burst', effect: '+8% damage', stack: 2, max: 5, now: '+16% damage', next: '+24% damage', waves: [2, 5] },
  operative: { name: 'Terminal Aperture', category: 'Skill', tag: 'Aria only', tier: 'operative', icon: 'SkillAriaTerminalVector', effect: '+25% Terminal Vector width', stack: 0, max: 3, now: 'None', next: '+25% width', waves: [] },
  rare: { name: 'Emergency Reassembly', category: 'Defense', tag: 'Rare', tier: 'rare', icon: 'add', effect: 'Revive one fallen fighter at 35% health', stack: 0, max: 0, now: 'Revive at 35%', next: 'Revive at 35%', waves: [] }
};
"""

P = {}


def card(name, group, height, subtitle, body, width=None, page=False):
    P[name] = (group, height, subtitle, body, width, page)


F, C, R_, B, S, Rs, Pk, Sc, M, W, SH = (
    'Foundations',
    'Controls',
    'Readouts',
    'Battle HUD',
    'Squad',
    'Results',
    'Perks',
    'Screens',
    'Menus',
    'World',
    'Showcase',
)

card(
    'GlitchText',
    B,
    140,
    'Heads-up projection · reduced modes fade',
    "render(h(V.GlitchText, { text: 'WAVE 08 · SIGNAL ACQUIRED', className: 'vb-t-title' }));",
)
card('CreditsChip', B, 100, 'Persistent merchant credits', 'render(h(V.CreditsChip, { credits: 120 }));')
card(
    'RouteChoice',
    Sc,
    620,
    'Crossroads · distinct roads and rewards',
    "render(h(V.RouteChoice, { view: { wave: 3, left: { kind: 'combat', title: 'Combat', icon: 'battle', hint: 'Common cache · chance of rare' }, right: { kind: 'merchant', title: 'Merchant', icon: 'currency', hint: 'Trade stacks or HP · rare and epic upgrades' } } }));",
    width=390,
    page=True,
)
card(
    'MerchantScreen',
    Sc,
    844,
    'Night market · prices, rarity and health trades',
    "render(h(V.MerchantScreen, { style: { height: 800 }, view: { credits: 120, offers: [{ perk: PERKS.rare, price: 80 }, { perk: PERKS.rare, price: 80, taken: true }, { perk: Object.assign({}, PERKS.rare, { name: 'Surge Engine', tier: 'epic', tag: 'Epic' }), price: 150 }], stacks: [], health: [{ actor: 0, name: 'Aria', hp: 800, maxHp: 1000, cost: 250, payout: 30 }] } }));",
    width=390,
    page=True,
)

card(
    'VbFrame',
    F,
    200,
    'Cut-corner panel: default, solid, raised, glass, enemy',
    r"""
var looks = [['vb-panel', 'panel'], ['vb-panel vb-panel--solid', 'panel--solid'], ['vb-panel vb-panel--raised', 'panel--raised'], ['vb-panel vb-panel--glass', 'panel--glass'], ['vb-panel vb-panel--enemy', 'panel--enemy']];
render(row(looks.map(function (l, i) {
  return cell(h(V.VbFrame, { className: l[0], style: { width: 150, height: 72 } }, h('div', { className: 'vb-l vb-t-label' }, 'SQUAD'), h('div', { className: 'vb-l vb-t-caption' }, '4 / 4 ready')), l[1], i);
}), 14));
""",
)

card(
    'VbIcon',
    F,
    780,
    '99 stroke glyphs by name, plus 8 design-system aliases; colour from CSS color',
    r"""
render(h('div', { style: { flexDirection: 'row', flexWrap: 'wrap', gap: 4 } }, V.VbIcon.names.map(function (n) {
  return h('div', { key: n, style: { width: 112, height: 70, alignItems: 'center', justifyContent: 'flex-start', paddingTop: 6 } },
    h(V.VbIcon, { icon: n, size: 24 }), h('div', { className: 'vb-l vb-t-badge', style: { marginTop: 6, fontSize: 9, letterSpacing: 0, whiteSpace: 'normal', textAlign: 'center', overflowWrap: 'anywhere' } }, n));
})));
""",
)

card(
    'VbMark',
    F,
    220,
    'The slash-cut VEIL/BREAK mark at 260 and 140 px',
    r"""
render(row([cell(h(V.VbMark, { width: 260 }), 'width 260', 0), cell(h(V.VbMark, { width: 140 }), 'width 140', 1)], 32, { alignItems: 'flex-end' }));
""",
)

card(
    'VbButton',
    C,
    440,
    'Primary (one per screen), blocked with its reason, secondary, light, danger, ghost',
    r"""
var B = V.VbButton;
render(col([
  cell(h(B, { title: 'START RUN', variant: 'primary', sub: 'Aria · Rook · Aegis · Nyra · new seed', icon: 'chevron', style: { width: 358 } }), 'primary'),
  cell(h(B, { title: 'START RUN', variant: 'primary', icon: 'chevron', blocked: 'Pick 1 more to start', style: { width: 358 } }), 'primary · blocked'),
  row([cell(h(B, { title: 'SETTINGS', icon: 'settings', style: { width: 175 } }), 'secondary', 0), cell(h(B, { title: 'ADD TO SQUAD', variant: 'light', icon: 'add', style: { width: 175 } }), 'light', 1)]),
  row([cell(h(B, { title: 'REMOVE FROM SQUAD', variant: 'danger', icon: 'remove', style: { width: 240 } }), 'danger', 0), cell(h(B, { title: 'SKIP', variant: 'ghost', style: { width: 110 } }), 'ghost', 1)])
], 10, { alignItems: 'flex-start' }));
""",
)

card(
    'VbSwitch',
    C,
    150,
    'Square on/off switch with what it does',
    r"""
render(col([
  h(V.VbSwitch, { text: 'HAPTICS', detail: 'Light pulse when an order is accepted', value: true, key: 0 }),
  h(V.VbSwitch, { text: 'REDUCE FLASHING', detail: 'Hit flashes and glows stay off', value: false, key: 1 })
], 4, { width: 358 }));
""",
)

card(
    'VbSlider',
    C,
    130,
    'Cut track, teal fill, square thumb, value 0–100',
    r"""
render(col([h(V.VbSlider, { label: 'Music', value: 0.6, key: 0 }), h(V.VbSlider, { label: 'Effects', value: 0.85, key: 1 })], 0, { width: 358 }));
""",
)

card(
    'VbSegmented',
    C,
    90,
    '2–5 peers, each with its consequence',
    r"""
render(h(V.VbSegmented, { items: [{ label: 'LOW', sub: 'Stable 30 FPS' }, { label: 'STANDARD', sub: '60 FPS target' }], selected: 1 }));
""",
)

card(
    'SkillButton',
    C,
    250,
    'Every state reads differently, from ready to queued, windup, invalid, refused and exposed',
    r"""
render(row(V.SkillButton.states.map(function (s, i) {
  return cell(h(V.SkillButton, { view: { label: 'Needle Storm', icon: 'SkillAriaNeedleStorm', cost: 18, state: s, charged: 0.4, need: 6, seconds: 1.4, queue: 2, progress: 0.55, reason: 'Range' }, flash: s === 'rejected' }), s.toUpperCase(), i);
}), 14, { width: 460 }));
""",
)

card(
    'StatReadout',
    R_,
    190,
    'Labelled numbers with an optional bar; previewed deltas read up or down; power and neutral tones',
    r"""
var S = V.StatReadout;
render(row([
  h(S, { label: 'HEALTH', value: '1,000', bar: 1000 / 1580, key: 0 }),
  h(S, { label: 'ARMOR', value: '10', bar: 10 / 26, className: 'vb-stat--neutral', key: 1 }),
  h(S, { label: 'STABILITY', value: '110', bar: 110 / 140, className: 'vb-stat--neutral', key: 2 }),
  h(S, { label: 'REGEN', value: '+12', unit: '/s', key: 3 }),
  h(S, { label: 'DAMAGE', value: '1,240', delta: '+12%', bar: 0.7, key: 4 }),
  h(S, { label: 'COOLDOWN', value: '4.2', unit: 's', delta: '−0.6s', up: true, key: 5 }),
  h(S, { label: 'POWER', value: '62', delta: '−18', bar: 0.62, tone: 'power', key: 6 })
], 12, { width: 420 }));
""",
)

card(
    'StatusChip',
    R_,
    90,
    'Glyph + word per status; never colour alone',
    r"""
var K = V.StatusChip;
render(col([
  row(['exposed', 'silenced', 'rooted', 'slowed', 'interrupted', 'barrier'].map(function (k, i) { return h(K, { kind: k, key: i }); }), 6),
  row([h(K, { kind: 'barrier', text: 'Shielded', detail: '2 pylons', key: 0 }), h(K, { kind: 'interrupted', text: 'Interruptible', key: 1 }), h(K, { kind: 'silenced', time: '2.4s', key: 2 })], 6)
], 8));
""",
)

card(
    'VbMeter',
    R_,
    150,
    'Track + fill, barrier span, dashed cost preview, queue marks',
    r"""
var M = V.VbMeter;
render(col([
  cell(h(M, { value: 0.62, style: { width: 300 } }), 'fill 62%'),
  cell(h(M, { value: 0.55, extra: 0.12, style: { width: 300 } }), 'fill + barrier span'),
  cell(h(M, { value: 0.34, previewFrom: 0.16, previewTo: 0.34, marks: [0.4], className: 'vb-power__meter', style: { width: 300 } }), 'power: cost preview + queue mark')
], 10, { alignItems: 'flex-start' }));
""",
)

card(
    'HealthBar',
    R_,
    230,
    'Health over stability, barrier after the fill, a ghost of recent damage; exposed, enemy, sizes and caption',
    r"""
var HB = V.HealthBar;
render(row([
  cell(h('div', { style: { width: 150 } }, h(HB, { hp: 820, max: 1000, barrier: 60, stability: 0.64 })), 'ally · barrier', 0),
  cell(h('div', { style: { width: 150 } }, h(HB, { hp: 1300, max: 1420, stability: 0, exposed: true })), 'exposed', 1),
  cell(h('div', { style: { width: 150 } }, h(HB, { hp: 0.72, max: 1, stability: 0.4, enemy: true })), 'enemy', 2),
  cell(h('div', { style: { width: 150 } }, h(HB, { hp: 540, max: 1000, ghost: 760, stability: 0.5, caption: 'HP 540 / 1000' })), 'ghost · caption', 3),
  cell(h(HB, { hp: 640, max: 1000, ghost: 820, size: 'world' }), 'world size', 4),
  cell(h('div', { style: { width: 300 } }, h(HB, { hp: 0.64, max: 1, stability: 0.35, enemy: true, size: 'large' })), 'large', 5)
], 20, { width: 520 }));
""",
)

card(
    'PowerStrip',
    R_,
    330,
    'Shared Power: value, regen, the aimed cost consequence, the autoplay reserve and the manual queue',
    r"""
var PS = V.PowerStrip;
render(col([
  h(PS, { value: 34, max: 100, regen: '+12/s', marks: [0.4], key: 0 }),
  h(PS, { value: 44, max: 100, regen: '+12/s', hint: 'Needle Storm −18 → 26 left', previewFrom: 26, previewTo: 44, key: 1 }),
  h(PS, { value: 22, max: 100, regen: '+12/s', reserve: 25, legend: '┊ autoplay reserve · × cancels', key: 2,
    queue: [{ who: 'Aegis', skill: 'Absolute Wall', cost: 40, eta: '1.6s', manual: true }, { who: 'Nyra', skill: 'White Noise', cost: 40, eta: '4.9s', manual: true }] })
], 18, { width: 350 }));
""",
)

card(
    'FocusMeter',
    R_,
    110,
    'Focus: ready, regenerating, draining while targeting slows combat',
    r"""
render(col([h(V.FocusMeter, { focus: 100, key: 0 }), h(V.FocusMeter, { focus: 62, key: 1 }), h(V.FocusMeter, { focus: 48, slow: true, key: 2 })], 6, { width: 350 }));
""",
)

card(
    'HudTop',
    B,
    470,
    'Translucent portrait HUD band; the resources panel shows the autoplay queue (pinned open here)',
    r"""
var Q = [{ who: 'Aegis', skill: 'Absolute Wall', cost: 40, eta: '0.5s' }, { who: 'Nyra', skill: 'White Noise', cost: 40, eta: '3.8s' }];
render(col([
  h(V.HudTop, { view: { wave: 5, boss: true, score: 48200, power: 72, focus: 100, auto: false, speed: 2 }, key: 0 }),
  h(V.HudTop, { view: { wave: 8, score: 12450, power: 34, focus: 62, slow: true, queue: Q }, queuePinned: true, key: 1 })
], 14, { width: 390, marginLeft: -16 }));
""",
    width=390,
)

card(
    'QueueList',
    B,
    200,
    'Upcoming casts in firing order; manual entries cancel on tap, autoplay estimates do not',
    r"""
render(h(V.QueueList, { title: 'MANUAL QUEUE', hint: 'Fires before autoplay · tap to cancel', onCancel: function () {},
  queue: [{ who: 'Aegis', skill: 'Absolute Wall', cost: 40, eta: '0.5s', manual: true, next: true }, { who: 'Nyra', skill: 'White Noise', cost: 40, eta: '3.8s', manual: true }, { who: 'Aria', skill: 'Needle Storm', cost: 18, eta: '5.3s' }] }));
""",
)

card(
    'BossBar',
    B,
    170,
    'Boss read-out: health over stability, shield and interrupt markers',
    r"""
render(col([
  h(V.BossBar, { view: { name: 'Signal Warden', health: 0.72, stability: 0.4, detail: '72% · enrage at 35%', shielded: true, shieldDetail: '2 pylons', interruptible: true }, key: 0 }),
  h(V.BossBar, { view: { name: 'Null Choir', health: 0.31, stability: 0, detail: '31% · stagger' }, key: 1 })
], 10, { width: 358 }));
""",
)

card(
    'OperativeCard',
    B,
    210,
    'Portrait battle card in each control mode: auto, manual, aiming, fallen',
    r"""
render(row([
  cell(h(V.OperativeCard, { view: op({ name: 'Echo', code: 'EC', kit: 'var(--kit-echo)', hp: 880, maxHp: 1020, barrier: 0, statuses: [], skills: [{ icon: 'SkillEchoClocksplice', cost: 14 }, { slot: 'S', icon: 'SkillEchoOverclock', cost: 35, state: 'cooldown', charged: 0.55, seconds: 3.1 }] }) }), 'AUTO', 0),
  cell(h(V.OperativeCard, { view: op({ name: 'Sol', code: 'SO', kit: 'var(--kit-sol)', hp: 1010, maxHp: 1220, barrier: 0, statuses: [], mode: 'manual', manualSeconds: 2.9, skills: [{ icon: 'SkillSolZeroLine', cost: 20 }, { slot: 'S', icon: 'SkillSolDeadHorizon', cost: 45, state: 'short', need: 11 }] }) }), 'MANUAL', 1),
  cell(h(V.OperativeCard, { view: op({ name: 'Mira', code: 'MI', kit: 'var(--kit-mira)', hp: 1300, maxHp: 1420, barrier: 0, statuses: ['exposed', 'barrier'], mode: 'aiming', skills: [{ icon: 'SkillMiraCounterdrive', cost: 16 }, { slot: 'S', icon: 'SkillMiraGravityCage', cost: 30, state: 'aiming' }] }) }), 'AIMING', 2),
  cell(h(V.OperativeCard, { view: op({ name: 'Hex', code: 'HE', kit: 'var(--kit-hex)', hp: 0, maxHp: 1150, barrier: 0, mode: 'fallen' }) }), 'FALLEN', 3)
], 8));
""",
)

card(
    'OperativeTile',
    S,
    330,
    'Roster tile: in squad (numbered), focused, not picked, rarity tiers and locked',
    r"""
render(row([
  cell(h(V.OperativeTile, { view: op({}), slot: 1, focused: true }), 'slot 1 · focused', 0),
  cell(h(V.OperativeTile, { view: op({ name: 'Rook', code: 'RO', role: 'Breaker', roleIcon: 'breaker', kit: 'var(--kit-rook)' }), slot: 2 }), 'slot 2', 1),
  cell(h(V.OperativeTile, { view: op({ name: 'Sol', code: 'SO', role: 'Striker', kit: 'var(--kit-sol)' }), slot: 0 }), 'not in squad', 2),
  cell(h(V.OperativeTile, { view: op({ name: 'Echo', code: 'EC', role: 'Operator', roleIcon: 'operator', kit: 'var(--kit-echo)' }), slot: 0, tier: 'violet' }), 'violet tier', 3),
  cell(h(V.OperativeTile, { view: op({ name: 'Nyra', code: 'NY', role: 'Operator', roleIcon: 'operator', kit: 'var(--kit-nyra)' }), slot: 0, tier: 'gold', locked: 'Pull in rifts' }), 'gold · locked', 4)
], 8, { width: 460 }));
""",
)

card(
    'SquadSlot',
    S,
    110,
    'Squad slots: filled, and the dashed empty slot',
    r"""
render(row([
  h(V.SquadSlot, { view: op({}), slot: 1, key: 0 }),
  h(V.SquadSlot, { view: op({ name: 'Rook', code: 'RO', kit: 'var(--kit-rook)' }), slot: 2, key: 1 }),
  h(V.SquadSlot, { view: op({ name: 'Aegis', code: 'AE', kit: 'var(--kit-aegis)' }), slot: 3, key: 2 }),
  h(V.SquadSlot, { view: null, slot: 4, key: 3 })
], 8));
""",
)

card(
    'OperativeDetails',
    S,
    370,
    'Operative sheet; adding is blocked with its reason when the squad is full',
    r"""
render(h(V.OperativeDetails, { view: op({ name: 'Sol', code: 'SO', kit: 'var(--kit-sol)', maxHp: 1220, armor: 14, maxStability: 120, passive: 'Zero point', passiveText: 'Every third basic hit marks the target for Zero Line.',
  skills: [{ label: 'Zero Line', icon: 'SkillSolZeroLine', cost: 20, slot: 'A', kind: 'Active', description: 'Long line shot that marks', cooldownSeconds: 5 }, { label: 'Dead Horizon', icon: 'SkillSolDeadHorizon', cost: 45, slot: 'S', kind: 'Signature', description: 'Sweeping beam across the lane', cooldownSeconds: 14 }] }), slot: 0, squadFull: true }));
""",
)

card(
    'OperativeStats',
    Rs,
    380,
    "One operative's post-run contribution (popover content)",
    r"""
render(h(V.OperativeStats, { view: { operative: op({ mode: 'fallen' }), fellWave: 12, damage: 48200, kills: 162, casts: 74, power: 1960, share: 0.47, extraLabel: 'Armor shred', extraValue: '312 hits' } }));
""",
)

card(
    'SquadResult',
    Rs,
    420,
    'Results cell; hover, focus or tap shows OperativeStats (pinned open on the first)',
    r"""
var C = function (o, w, s) { return { operative: op(o), fellWave: w, damage: 48200 * s, kills: Math.round(162 * s), casts: Math.round(74 * s), power: Math.round(1960 * s), share: s, extraLabel: 'Armor shred', extraValue: '312 hits' }; };
render(h('div', { style: { height: 388, justifyContent: 'flex-end' } }, row([
  h(V.SquadResult, { view: C({}, 12, 0.47), pinned: true, key: 0, style: { width: 84 } }),
  h(V.SquadResult, { view: C({ name: 'Rook', code: 'RO', kit: 'var(--kit-rook)' }, 0, 0.21), key: 1 }),
  h(V.SquadResult, { view: C({ name: 'Aegis', code: 'AE', kit: 'var(--kit-aegis)' }, 0, 0.12), key: 2 }),
  h(V.SquadResult, { view: C({ name: 'Nyra', code: 'NY', kit: 'var(--kit-nyra)' }, 9, 0.2), key: 3 })
].map(function (n, i) { return h('div', { key: i, style: { width: 84 } }, n); }), 6)));
""",
)

card(
    'ScoreRow',
    Sc,
    230,
    'High-score lines; the current run is framed',
    r"""
render(col([
  h(V.ScoreRow, { view: { rank: 1, score: 31880, wave: 15, bosses: 3, squad: ['AR', 'RO', 'AE', 'NY'] }, key: 0 }),
  h(V.ScoreRow, { view: { rank: 2, score: 24610, wave: 12, bosses: 2, squad: ['AR', 'RO', 'AE', 'NY'], current: true }, key: 1 }),
  h(V.ScoreRow, { view: { rank: 4, score: 9120, wave: 6, bosses: 1, squad: ['SO', 'MI', 'EC'] }, key: 2 })
], 0, { width: 358 }));
""",
)

card(
    'ScreenHeader',
    Sc,
    150,
    'Back, title, short meta read-out (accent when it is good news)',
    r"""
render(col([h(V.ScreenHeader, { title: 'Squad', meta: '4 / 4', accent: true, key: 0 }), h(V.ScreenHeader, { title: 'High scores', meta: 'TOP 10', key: 1 })], 0, { width: 390, marginLeft: -16 }));
""",
    width=390,
)

card(
    'PerkCard',
    Pk,
    260,
    'Draft cards: general (selected), operative, rare',
    r"""
render(row([
  h(V.PerkCard, { view: PERKS.general, selected: true, key: 0 }),
  h(V.PerkCard, { view: PERKS.operative, key: 1 }),
  h(V.PerkCard, { view: PERKS.rare, key: 2 })
], 10));
""",
)

card(
    'PerkDetail',
    Pk,
    230,
    'Draft (now → after pick) and owned (active, stacks, waves)',
    r"""
render(row([
  h(V.PerkDetail, { view: PERKS.general, draft: true, style: { width: 260 }, key: 0 }),
  h(V.PerkDetail, { view: Object.assign({}, PERKS.general, { stack: 3, now: '+24% damage', waves: [2, 5, 9] }), draft: false, style: { width: 260 }, key: 1 })
], 12));
""",
)

card(
    'PerkChip',
    Pk,
    300,
    'Owned-perk chips; hover, focus or tap shows PerkDetail (pinned on the first)',
    r"""
render(row([
  h(V.PerkChip, { view: Object.assign({}, PERKS.general, { stack: 3, now: '+24% damage', waves: [2, 5, 9] }), pinned: true, key: 0 }),
  h(V.PerkChip, { view: Object.assign({}, PERKS.operative, { stack: 1 }), key: 1 }),
  h(V.PerkChip, { view: Object.assign({}, PERKS.rare, { stack: 1 }), key: 2 })
], 0));
""",
)

card(
    'PerkRow',
    Pk,
    230,
    'Active-perk list rows; selected is framed',
    r"""
render(col([
  h(V.PerkRow, { view: Object.assign({}, PERKS.general, { stack: 3, now: '+24% damage' }), key: 0 }),
  h(V.PerkRow, { view: Object.assign({}, PERKS.operative, { stack: 1, now: '+25% width' }), selected: true, key: 1 }),
  h(V.PerkRow, { view: { name: 'Field Repairs', category: 'Defense', tag: 'General', tier: 'general', icon: 'cleanse', effect: 'Heal survivors for 10%; restore stability', now: 'Heal 10%', stack: 1, max: 0 }, key: 2 })
], 0, { width: 358 }));
""",
)

card(
    'QueueChip',
    B,
    200,
    'One queue entry: the next to fire, a buffered cast (›), a manual entry with its ×, a dashed aim preview',
    r"""
var Q = { who: 'Aegis', skill: 'Absolute Wall', cost: 40, eta: '1.6s' };
render(col([
  cell(h(V.QueueChip, { view: Object.assign({}, Q, { manual: true }), position: 1, onCancel: function () {} }), 'next · manual', 0),
  cell(h(V.QueueChip, { view: { who: 'Nyra', skill: 'White Noise', cost: 40, eta: 'ready', manual: true, next: true }, position: 2 }), 'buffered', 1),
  cell(h(V.QueueChip, { view: { who: 'Aria', skill: 'Needle Storm', cost: 18, eta: '5.3s' }, position: 3 }), 'autoplay estimate', 2),
  cell(h(V.QueueChip, { view: { who: 'Sol', skill: 'Zero Line', cost: 20, eta: '#4?' }, position: 4, preview: true }), 'aim preview', 3)
], 8, { width: 300, alignItems: 'stretch' }));
""",
)

card(
    'Dialog',
    Sc,
    300,
    'Confirmation sheet: title, one line of body, then the actions',
    r"""
render(h('div', { style: { width: 358 } }, h(V.Dialog, { title: 'LEAVE GAME?', body: 'Your profile is saved. Resume returns to the last completed-wave checkpoint.' },
  h(V.VbButton, { title: 'EXIT GAME', variant: 'primary', icon: 'back' }), h(V.VbButton, { title: 'CANCEL', variant: 'secondary', icon: 'close' }))));
""",
)

card(
    'Backdrop',
    Sc,
    260,
    'Cover-scaled menu art with a legibility shade along the bottom',
    r"""
var art = 'radial-gradient(80% 60% at 60% 30%, var(--scene-neon) 0%, transparent 60%), linear-gradient(180deg, var(--dark-50), var(--scene-ground))';
render(h('div', { style: { width: 358, height: 230, position: 'relative' } }, h(V.Backdrop, { style: { backgroundImage: art } }),
  h('div', { style: { position: 'absolute', left: 16, bottom: 14 } }, h('div', { className: 'vb-l vb-t-title' }, 'VEILBREAK'), h('div', { className: 'vb-l vb-t-caption' }, 'Text over art stays legible'))));
""",
)

card(
    'ScanLines',
    M,
    120,
    'Projected-image lines for holo surfaces; never behind live battle numbers',
    r"""
render(h(V.VbFrame, { className: 'vb-panel vb-panel--raised', style: { width: 300, height: 90, padding: 12 } }, h(V.ScanLines), h('div', { className: 'vb-l vb-t-label' }, 'HOLO SURFACE')));
""",
)

card(
    'HoloFrame',
    M,
    230,
    'Diegetic menu panel: brackets, scanlines, chromatic caps title and a flicker-in (menus only)',
    r"""
render(h('div', { style: { width: 320 } }, h(V.HoloFrame, { title: 'Squad link' },
  h('div', { className: 'vb-l vb-t-body' }, 'Four operatives synced.'), h('div', { className: 'vb-l vb-t-caption', style: { marginTop: 6 } }, 'Weight shared across the squad'))));
""",
)

card(
    'HoloPlatform',
    M,
    220,
    'Lineup plinth stand-in: occupied, selected with brackets, open slot',
    r"""
render(row([
  h(V.HoloPlatform, { name: 'Aria', detail: 'Striker · R2', key: 0 }),
  h(V.HoloPlatform, { name: 'Aegis', detail: 'Bastion', state: 'selected', key: 1 }),
  h(V.HoloPlatform, { state: 'open', key: 2 })
], 10));
""",
)

card(
    'RevealCard',
    M,
    260,
    'Rift result: holographic back (tap to flip), tier fronts with their words, duplicate resonance',
    r"""
render(row([
  cell(h(V.RevealCard, { tier: 'blue', title: 'Rook', detail: 'RO · Breaker' }), 'back · tap', 0),
  cell(h(V.RevealCard, { tier: 'blue', title: 'Rook', detail: 'RO · Breaker', flipped: true }), 'rare', 1),
  cell(h(V.RevealCard, { tier: 'violet', title: 'Optic Lens', detail: 'Optic', item: true, flipped: true }), 'rare item', 2),
  cell(h(V.RevealCard, { tier: 'gold', title: 'Nyra', detail: 'NY · Operator', dupe: 'Resonance R2', flipped: true }), 'legendary · dupe', 3)
], 10));
""",
)

card(
    'RuleRow',
    M,
    300,
    'IF … THEN automation rules: on, off, warning and draft',
    r"""
var c = [{ text: 'Power ≥', value: '80' }, { text: 'enemy within', value: '6', after: 'm' }];
render(col([
  h(V.RuleRow, { view: { index: 1, conditions: c, action: 'Zero Line', cost: 40, note: 'Runs only at 80+ Power: 40 cost + 40 reserve' }, key: 0 }),
  h(V.RuleRow, { view: { index: 2, conditions: [{ text: 'ally health <', value: '35', after: '%' }], action: 'Absolute Wall', cost: 40, enabled: false }, key: 1 }),
  h(V.RuleRow, { view: { index: 3, conditions: [{ text: 'boss', value: 'interruptible' }], any: true, action: 'White Noise', cost: 40, warning: true, note: 'Shares Power with rule 1; the player\'s command always wins' }, key: 2 }),
  h(V.RuleRow, { view: { index: 4, conditions: [{ text: 'wave ≥', value: '10' }], action: 'Needle Storm', cost: 18, draft: true }, key: 3 })
], 8, { width: 358 }));
""",
)

card(
    'WorldHealthBar',
    W,
    120,
    'Team bars over fighters: ally, enemy, elite and boss widths, barrier, ghost of recent damage',
    r"""
var still = { position: 'relative', translate: 'none' };
render(row([
  cell(h(V.WorldHealthBar, { team: 'ally', health: 0.7, barrier: 0.15, style: still }), 'ally · barrier', 0),
  cell(h(V.WorldHealthBar, { team: 'enemy', health: 0.45, ghost: 0.7, style: still }), 'enemy · ghost', 1),
  cell(h(V.WorldHealthBar, { team: 'enemy', rank: 'elite', health: 0.8, style: still }), 'elite', 2),
  cell(h(V.WorldHealthBar, { team: 'enemy', rank: 'boss', health: 0.62, ghost: 0.75, style: still }), 'boss', 3)
], 22));
""",
)

card(
    'CombatText',
    W,
    120,
    'Floating numbers from confirmed events; crits, executes and breaks flare',
    r"""
var K = [['damage', 1240], ['taken', 86], ['crit', 2310], ['exposed', 1240], ['heal', 120], ['barrier', 60], ['power', 12], ['miss', 0], ['execute', 2400], ['break', 0]];
render(row(K.map(function (k, i) { return cell(h(V.CombatText, { kind: k[0], amount: k[1], style: { position: 'relative', translate: 'none' } }), k[0], i); }), 16, { width: 460 }));
""",
)

card(
    'WorldLabel',
    W,
    130,
    'Billboards: aim chip, name tag, telegraph timer, move estimate, blocked',
    r"""
render(row([
  cell(h(V.WorldLabel, { kind: 'aim', title: 'A · NEEDLE STORM', detail: '18 POWER · queues as #3', style: { position: 'relative', translate: 'none' } }), 'aim', 0),
  cell(h(V.WorldLabel, { kind: 'telegraph', title: 'SLAM 0.6s', team: 'enemy', style: { position: 'relative', translate: 'none' } }), 'telegraph', 1),
  cell(h(V.WorldLabel, { kind: 'estimate', title: 'MOVE ≈ 8', style: { position: 'relative', translate: 'none' } }), 'estimate', 2),
  cell(h(V.WorldLabel, { kind: 'blocked', title: 'OUT OF RANGE', team: 'enemy', style: { position: 'relative', translate: 'none' } }), 'blocked', 3)
], 14, { width: 460 }));
""",
)

card(
    'WorldMarker',
    W,
    110,
    'Allies are circles and enemies diamonds, always; selected, queued and move rings',
    r"""
render(row(['ally', 'enemy', 'selected', 'queued', 'move'].map(function (k, i) { return cell(h(V.WorldMarker, { kind: k, size: 40, style: { position: 'relative', translate: 'none' } }), k, i); }), 18));
""",
)

card(
    'OffscreenIndicator',
    W,
    110,
    'Edge chips toward fighters the camera cannot frame; the breach warning',
    r"""
render(row([
  cell(h(V.OffscreenIndicator, { name: 'BREACH', metres: 4, threat: true, angle: 90, style: { position: 'relative', translate: 'none' } }), 'breach · below', 0),
  cell(h(V.OffscreenIndicator, { name: 'BRUTE', metres: 12, angle: -90, style: { position: 'relative', translate: 'none' } }), 'enemy · above', 1),
  cell(h(V.OffscreenIndicator, { name: 'AEGIS', team: 'ally', angle: 180, style: { position: 'relative', translate: 'none' } }), 'ally · left', 2)
], 18));
""",
)

card(
    'EdgeWarning',
    W,
    120,
    'The breach band along the bottom edge: calm, and hot as a hostile nears the rear line',
    r"""
render(h('div', { style: { display: 'flex', flexDirection: 'column', gap: 14, width: 340 } },
  h('div', { style: { position: 'relative', height: 30 } }, h(V.EdgeWarning, { caption: 'BREACH ZONE', urgency: 0.4 })),
  h('div', { style: { position: 'relative', height: 30 } }, h(V.EdgeWarning, { caption: 'BREACH ZONE', urgency: 0.9 }))));
""",
)

card(
    'BattleHUD',
    SH,
    844,
    'Portrait battle board, 390 × 844, built only from the kit (Samples~/BattleHud)',
    r"""
var names = ['Aria', 'Rook', 'Aegis', 'Nyra'], kits = ['aria', 'rook', 'aegis', 'nyra'];
var queue = [{ who: 'AE', skill: 'Absolute Wall', cost: 40, eta: '1.2s', manual: true }, { who: 'NY', skill: 'White Noise', cost: 40, eta: '4.1s', manual: true }];
function at(x, y, node, key) { return h('div', { key: key, style: { position: 'absolute', left: x, top: y } }, node); }
var street = 'linear-gradient(180deg, rgba(5,2,8,0) 0%, rgba(5,2,8,.6) 100%), repeating-linear-gradient(90deg, transparent 0 62px, rgba(143,240,207,.05) 62px 63px),' +
  'radial-gradient(70% 45% at 50% 20%, var(--scene-neon) 0%, transparent 70%), linear-gradient(180deg, var(--dark-50) 0%, var(--scene-ground) 100%)';
var field = h('div', { className: 'vb-grow', style: { position: 'relative', overflow: 'hidden' } },
  at(195, 96, h(V.WorldMarker, { kind: 'enemy', size: 44 }), 'bm'),
  at(195, 64, h(V.WorldHealthBar, { team: 'enemy', rank: 'boss', health: 0.64, ghost: 0.72 }), 'bb'),
  at(195, 52, h(V.WorldLabel, { kind: 'telegraph', title: 'SLAM 1.4s', team: 'enemy' }), 'bl'),
  at(262, 40, h(V.CombatText, { kind: 'crit', amount: 2310 }), 'c1'),
  at(132, 74, h(V.CombatText, { kind: 'damage', amount: 412 }), 'c2'),
  [[110, 186], [280, 186], [150, 232], [240, 232]].map(function (p, i) {
    return h(React.Fragment, { key: 'a' + i }, at(p[0], p[1], h(V.WorldMarker, { kind: i === 2 ? 'queued' : 'ally', size: 28 })), at(p[0], p[1] - 22, h(V.WorldHealthBar, { team: 'ally', health: [0.82, 0.6, 0.9, 0.7][i], barrier: i === 2 ? 0.1 : 0 })));
  }),
  at(110, 156, h(V.WorldLabel, { kind: 'aim', title: 'A · NEEDLE STORM', detail: '18 POWER · queues as #3' }), 'aim'),
  at(330, 236, h(V.OffscreenIndicator, { name: 'BREACH', metres: 4, threat: true, angle: 90 }), 'br'));
render(h(V.Backdrop, { style: { position: 'relative', width: 390, height: 844, backgroundImage: street, paddingTop: 48, paddingBottom: 34, margin: -16 } },
  h('div', { className: 'vb-root', style: { position: 'absolute', left: 0, right: 0, top: 48, bottom: 34 } },
    h(V.HudTop, { view: { wave: 10, boss: true, score: 18240, power: 22, focus: 60, queue: queue, queueTitle: 'MANUAL QUEUE', queueHint: 'Fires before autoplay · tap to cancel' } }),
    h(V.BossBar, { view: { name: 'Signal Warden', health: 0.64, stability: 0.35, detail: '64% · enrage at 35%', shielded: true, shieldDetail: '2 pylons', interruptible: true }, style: { margin: '6px 10px 0' } }),
    field,
    h('div', { style: { padding: '0 10px' } }, h(V.PowerStrip, { value: 22, max: 100, regen: '+12/s', reserve: 25, queue: queue, legend: '┊ autoplay reserve · × cancels' })),
    h('div', { className: 'vb-row vb-row--between', style: { padding: '6px 10px 0' } }, names.map(function (n, i) {
      return h(V.OperativeCard, { key: n, style: { width: 88, height: 172 }, view: op({ name: n, code: n.slice(0, 2).toUpperCase(), kit: 'var(--kit-' + kits[i] + ')', hp: 600 + i * 90, maxHp: 1000, barrier: 0, statuses: [], mode: i === 2 ? 'manual' : 'auto',
        skills: [{ icon: 'projectile', cost: 18, state: i === 2 ? 'queued' : 'ready', queue: 1 }, { slot: 'S', icon: 'burst', cost: 40, state: i === 3 ? 'windup' : 'cooldown', charged: 0.5, seconds: 2.2, progress: 0.6 }] }) });
    })))));
""",
    width=390,
    page=True,
)

# ── 0.3.0: the handbook's atoms, composites, layouts and views ─────────────────
LY, VW = 'Layouts', 'Views'
SQUAD = r"""function kitOp(k, i) { return op({ id: k, name: k.charAt(0).toUpperCase() + k.slice(1), code: k.slice(0, 2).toUpperCase(), kit: 'var(--kit-' + k + ')', hp: 600 + i * 90 }); }
var ROSTER = ['aria', 'sol', 'aegis', 'nyra', 'rook', 'mira', 'hex', 'echo'].map(kitOp);
"""
card('TextComponent', F, 220, 'One type style per run; tones only recolour', r"""
var T = V.TextComponent;
render(col([h(T, { typeStyle: 'display', text: 'PAUSED', key: 0 }), h(T, { typeStyle: 'title', text: 'SQUAD', key: 1 }), h(T, { typeStyle: 'caption', text: 'Combat, focus and world travel are frozen', key: 2 }),
  row([h(T, { typeStyle: 'num', text: '18,240', key: 0 }), h(T, { typeStyle: 'num', tone: 'ally', text: '+700', key: 1 }), h(T, { typeStyle: 'num', tone: 'enemy', text: '−40', key: 2 }), h(T, { typeStyle: 'num', tone: 'power', text: '62', key: 3 }), h(T, { typeStyle: 'caption', tone: 'muted', text: 'muted', key: 4 })], 14)], 6, { alignItems: 'flex-start' }));
""")
card('LabelComponent', F, 120, 'The caps eyebrow, label and badge sizes, with its 4-unit rhythm', r"""
render(col([h(V.LabelComponent, { text: 'Roster · all 8 unlocked', key: 0 }), h(V.LabelComponent, { text: 'Carries forward', size: 'badge', key: 1 }), h(V.LabelComponent, { text: 'Wave 07 cleared', tone: 'ally', key: 2 }), h(V.LabelComponent, { text: 'Squad down · wave 12', tone: 'enemy', key: 3 })], 4, { alignItems: 'flex-start' }));
""")
card('BadgeComponent', C, 120, 'Slim chips: neutral, ally, enemy, power, warn, inverted; with a glyph and a value', r"""
var B = V.BadgeComponent;
render(row([h(B, { text: 'Best', value: '18,240', key: 0 }), h(B, { text: 'Standing', tone: 'ally', icon: 'check', key: 1 }), h(B, { text: 'Exposed', tone: 'enemy', icon: 'exposed', key: 2 }), h(B, { text: 'Power', tone: 'power', value: '62', key: 3 }),
  h(B, { text: 'Low HP', tone: 'warn', icon: 'warning', key: 4 }), h(B, { text: 'New', tone: 'inverted', key: 5 }), h(B, { text: 'Sharpened Output ×2', icon: 'burst', key: 6 })], 6, { width: 360 }));
""")
card('IconButtonComposite', C, 110, '44 × 44 glyph squares: back, pause (quiet), autoplay (active), speed, close', r"""
var I = V.IconButtonComposite;
render(row([cell(h(I, { icon: 'back', tooltip: 'Back' }), 'back', 0), cell(h(I, { icon: 'pause', tooltip: 'Pause', quiet: true }), 'quiet', 1), cell(h(I, { icon: 'automation', tooltip: 'Autoplay on', quiet: true, active: true }), 'active', 2),
  cell(h(I, { text: '2×', tooltip: 'Speed' }), 'state', 3), cell(h(I, { icon: 'close', tooltip: 'Close', size: 12 }), 'close', 4)], 16));
""")
card('TileComponent', C, 150, 'Pickable cells: picked and numbered, focused, a glyph tile, locked, and an empty slot', r"""
var T = V.TileComponent;
render(row([h(T, { title: 'Aria', caption: 'Striker', captionIcon: 'striker', kit: 'var(--kit-aria)', order: 1, state: ['in', 'focused'], key: 0 }), h(T, { title: 'Visor', caption: 'Optic', captionIcon: 'optic', glyph: 'optic', tier: 'violet', key: 1 }),
  h(T, { title: 'Hex', caption: 'Pull in rifts', captionIcon: 'lock', state: 'locked', tier: 'gold', key: 2 }), h(T, { block: 'vb-slot', state: 'empty', emptyLabel: 'Slot 4', key: 3 })], 8));
""")
card('CardComponent', C, 300, 'The base card: head, body, foot; selected is chipped', r"""
render(col([h(V.CardComponent, { title: 'Checkpoint', meta: 'WAVE 07', style: { width: 320 }, actions: h(V.VbButton, { title: 'RESUME', icon: 'play' }), key: 0 }, h('div', { className: 'vb-l', style: { fontSize: 14 } }, 'Wave 07 next · 9,420'), h('div', { className: 'vb-l vb-t-caption' }, '3 of 4 standing · 5 perks · seed 80A2EB4E')),
  h(V.CardComponent, { title: 'Last run', meta: '#3 OF 20', selected: true, style: { width: 320 }, key: 1 }, row([h(V.StatReadout, { label: 'SCORE', value: '18,240', key: 0 }), h(V.StatReadout, { label: 'WAVES', value: '12', key: 1 }), h(V.StatReadout, { label: 'BOSSES', value: '2', key: 2 })], 22))], 12, { alignItems: 'flex-start' }));
""")
card('FieldComposite', C, 200, 'A labelled input with its help line, in a fieldset', r"""
render(h(V.Fieldset, { title: 'CAMERA', style: { width: 358 } }, h(V.FieldComposite, { label: 'Camera motion', help: 'Pinch to zoom, twist to turn, double-tap to reset.' }, h(V.VbSegmented, { items: [{ label: 'FULL', sub: 'Parallax' }, { label: 'REDUCED', sub: 'No drift' }, { label: 'OFF', sub: 'Fixed' }], selected: 0 }))));
""")
card('Fieldset', C, 240, 'A titled group of fields in one slim container', r"""
render(h(V.Fieldset, { title: 'AUDIO', style: { width: 358 } }, h(V.VbSlider, { label: 'Master', value: 0.8 }), h(V.VbSlider, { label: 'Music', value: 0.55 }), h(V.VbSlider, { label: 'Effects', value: 0.8 })));
""")
card('FormComposite', C, 470, 'Fieldsets in a scroll, a note, a footer of secondary actions; applies as it changes', r"""
render(h(V.FormComposite, { saved: 'SAVED', style: { width: 358, height: 440 }, note: { icon: 'automation', text: 'Autoplay lives in the battle HUD and starts on for every new run.' },
  footer: [h(V.VbButton, { title: 'CLEAR LOCAL SCORES', variant: 'danger', key: 0 }), h(V.VbButton, { title: 'ACCESSIBILITY', variant: 'ghost', key: 1 })] },
  h(V.Fieldset, { title: 'FEEL' }, h(V.VbSwitch, { text: 'HAPTICS', detail: 'Light pulse when an order is accepted', value: true }), h(V.VbSwitch, { text: 'REDUCED MOTION', detail: 'No camera shake · fades replace flicker', value: false })),
  h(V.Fieldset, { title: 'QUALITY' }, h(V.VbSegmented, { items: [{ label: 'LOW', sub: 'Stable 30 FPS' }, { label: 'STANDARD', sub: '60 FPS target' }], selected: 1 }))));
""")
card('StackLayout', LY, 230, 'One rhythm for a column: 4, 8, 12 or 16 under every child but the last', r"""
function bar(t) { return h(V.VbFrame, { kind: 'slim', style: { width: 150, height: 30, padding: '6px 10px' } }, h('div', { className: 'vb-l vb-t-badge' }, t)); }
render(row([4, 8, 16].map(function (g) { return cell(h(V.StackLayout, { gap: g }, bar('ONE'), bar('TWO'), bar('THREE')), 'gap ' + g, g); }), 14));
""")
card('VbGrid', LY, 300, 'Two, three and four across', r"""
function cellBox(t, k) { return h(V.VbFrame, { kind: 'slim', key: k, style: { height: 40, alignItems: 'center', justifyContent: 'center' } }, h('div', { className: 'vb-l vb-t-badge' }, t)); }
render(col([2, 3, 4].map(function (n) { return h(V.VbGrid, { key: n, columns: n, wrap: true, style: { width: 358 } }, Array.apply(null, Array(n)).map(function (_, i) { return cellBox(n + ' · ' + (i + 1), i); })); }), 4, { alignItems: 'flex-start' }));
""")
card('ListLayout', LY, 280, 'Rows of one kind, 6 between them, and the empty line', r"""
render(row([h(V.ListLayout, { style: { width: 330, height: 220 }, key: 0 }, [1, 2, 3].map(function (r) { return h(V.ScoreRow, { key: r, view: { rank: r, score: [18240, 14120, 9420][r - 1], wave: [12, 10, 7][r - 1], bosses: [2, 2, 1][r - 1], squad: ['AR', 'SO', 'AE', 'NY'], current: r === 2 } }); })),
  h(V.ListLayout, { empty: 'No runs yet. Your first run lands here.', style: { width: 200, height: 60 }, key: 1 })], 16));
""")
card('BoardLayout', LY, 420, 'Top band, the world (the rest), bottom band', r"""
function zone(t, s) { return h('div', { style: Object.assign({ border: '1px dashed rgba(143,240,207,.45)', background: 'rgba(143,240,207,.05)', display: 'flex', alignItems: 'center', justifyContent: 'center' }, s) }, h('div', { className: 'vb-l vb-t-badge vb-t-ally' }, t)); }
render(h(V.BoardLayout, { style: { width: 300, height: 400 }, top: zone('TOP · HUD · BOSS · ANNOUNCER', { height: 90, margin: 6 }), world: zone('WORLD', { position: 'absolute', inset: 6 }), bottom: zone('BOTTOM · CARDS', { height: 110, margin: 6 }) }));
""")
MENU_BG = "radial-gradient(60% 40% at 50% 30%, rgba(240,137,159,.28) 0%, transparent 70%), radial-gradient(80% 50% at 50% 100%, rgba(29,217,160,.10) 0%, transparent 70%), linear-gradient(180deg, #3a2a4d 0%, #1d1428 55%, #0b0710 100%)"
BOARD = r"""function board(node, bg, screen) {
  return h('div', { style: { position: 'relative', width: 390, height: 844, margin: -16, background: bg || 'linear-gradient(180deg, #2c263f 0%, #1b1526 100%)' } },
    h('div', { className: 'vb-root', style: { position: 'absolute', left: 0, right: 0, top: 48, bottom: 34 } },
      h('div', { className: 'vb-screen vb-screen--' + (screen || 'page') }, h('div', { className: 'vb-page vb-page--' + (screen || 'page') }, node))));
}
"""
CHECKPOINT = "{ wave: 7, score: 9420, standing: 3, squadSize: 4, perks: 5, seed: '80A2EB4E' }"
card('MainMenuView', VW, 844, 'The title screen: badge and best, the mark, the menu actions, EXIT', BOARD + "render(board(h(V.MainMenuView, { view: { best: 18240, bestWave: 12, checkpoint: " + CHECKPOINT + ", squad: ['Aria', 'Sol', 'Aegis', 'Nyra'] } }), '" + MENU_BG + "', 'menu'));", width=390, page=True)
card('MenuActionsSectionView', VW, 300, 'Checkpoint card, PLAY, HIGH SCORES and SETTINGS', "render(h('div', { style: { width: 358 } }, h(V.MenuActionsSectionView, { checkpoint: " + CHECKPOINT + ", squadLine: 'Squad · Aria · Sol · Aegis · Nyra' })));")
card('SquadView', VW, 844, 'Four slots, the roster, the open details and START RUN', BOARD + SQUAD + "render(board(h(V.SquadView, { view: { roster: ROSTER, selected: [0, 1, 2, -1], focus: 3, locked: ['', '', '', '', '', '', 'Pull in rifts', ''] } }), null, 'squad'));", width=390, page=True)
card('SettingsView', VW, 844, 'Audio, feel, camera and quality in one form; SAVED in the header', BOARD + "render(board(h(V.SettingsView, {}), null, 'settings'));", width=390, page=True)
card('PauseOverlayView', VW, 844, 'PAUSED, the sheet with stats, perks and RESUME, and EXIT', BOARD + "render(board(h(V.PauseOverlayView, { view: { wave: 7, score: 9420, standing: 3, perks: [PERKS.general, PERKS.operative, Object.assign({}, PERKS.rare, { icon: 'barrier', name: 'Barrier Mesh' })], seed: '80A2EB4E', detail: 'rules 3 · checkpoint after wave 06' } }), null, 'pause'));", width=390, page=True)
card('ConfirmDialogView', VW, 844, 'Question, consequence, the destructive action and the way back', BOARD + "render(board([h('div', { key: 0, className: 'vb-grow' }), h(V.ConfirmDialogView, { key: 1, title: 'END THIS RUN?', body: 'Your current run will be recorded and its checkpoint removed.', confirm: { label: 'ABANDON', icon: 'remove', sub: 'Record the score and end the run' }, cancel: { label: 'KEEP PLAYING', icon: 'chevron' } })], null, 'confirm'));", width=390, page=True)
card('GuideDialogView', VW, 844, 'The first-run guide: numbered steps and READY', BOARD + "render(board([h('div', { key: 0, className: 'vb-grow' }), h(V.GuideDialogView, { key: 1 })], null, 'pause'));", width=390, page=True)
card('PerkDraftView', VW, 844, 'Three cards, the pick explained, the squad carrying forward, TAKE PERK', BOARD + "render(board(h(V.PerkDraftView, { view: { wave: 7, bonus: 700, score: 9420, pick: 0, choices: [PERKS.general, PERKS.operative, PERKS.rare], squad: [{ code: 'AR', hp: 820, maxHp: 1000, stability: 0.7 }, { code: 'SO', hp: 640, maxHp: 1000, stability: 0.5 }, { code: 'AE', hp: 910, maxHp: 1000, stability: 0.9 }, { code: 'NY', hp: 0, maxHp: 1000, stability: 0 }] } }), null, 'perks'));", width=390, page=True)
card('ResultsView', VW, 844, 'The score and its ledger, the squad, the perks, MENU and RUN AGAIN', BOARD + SQUAD + "render(board(h(V.ResultsView, { view: { score: 18240, wave: 12, bosses: 2, rank: 1, of: 20, distance: '1.4 km', simTime: '9:12', ledger: [{ label: 'Creep kills', value: '9,640' }, { label: 'Wave clears · 100 × wave', value: '7,800' }, { label: 'Bosses · 500 × wave', value: '800' }], squad: ROSTER.slice(0, 4).map(function (o, i) { return { operative: o, fellWave: i === 3 ? 11 : 0, damage: 40000 - i * 7000, kills: 120 - i * 20, casts: 40, power: 900, share: [0.34, 0.27, 0.21, 0.18][i] }; }), perks: [PERKS.general, PERKS.rare] } }), null, 'results'));", width=390, page=True)
card('HighScoresView', VW, 844, 'Top 20 with the current run framed, then older rules', BOARD + "render(board(h(V.HighScoresView, { ranked: [18240, 14120, 9420, 8800, 6120, 4410].map(function (s, i) { return { rank: i + 1, score: s, wave: 12 - i, bosses: i < 3 ? 2 : 1, squad: ['AR', 'SO', 'AE', 'NY'], current: i === 2 }; }), historical: [{ rank: 1, score: 21020, wave: 13, bosses: 2, squad: ['AR', 'RO', 'HX', 'NY'] }] }), null, 'scores'));", width=390, page=True)
card('RouteChoiceView', VW, 844, 'The crossroads screen', BOARD + "render(board(h(V.RouteChoiceView, { view: { wave: 3, left: { kind: 'combat', title: 'Combat', icon: 'battle', hint: 'Common cache · chance of rare' }, right: { kind: 'merchant', title: 'Merchant', icon: 'currency', hint: 'Trade stacks or HP · rare and epic upgrades' } } }), null, 'route'));", width=390, page=True)
card('MerchantScreenView', VW, 844, 'The night market screen (MerchantView)', BOARD + "render(board(h(V.MerchantScreenView, { view: { credits: 120, offers: [{ perk: PERKS.rare, price: 80 }, { perk: PERKS.rare, price: 80, taken: true }, { perk: Object.assign({}, PERKS.rare, { name: 'Surge Engine', tier: 'epic', tag: 'Epic' }), price: 150 }], stacks: [], health: [{ actor: 0, name: 'Aria', hp: 800, maxHp: 1000, cost: 250, payout: 30 }] } }), null, 'merchant'));", width=390, page=True)
card('BattleHudView', VW, 844, 'The whole battle board on BoardLayout', BOARD + SQUAD + r"""
var queue = [{ who: 'AE', skill: 'Absolute Wall', cost: 40, eta: '1.2s', manual: true }, { who: 'NY', skill: 'White Noise', cost: 40, eta: '4.1s', manual: true }];
var ops = ROSTER.slice(0, 4).map(function (o, i) { return Object.assign({}, o, { barrier: 0, statuses: [], mode: i === 2 ? 'manual' : 'auto', skills: [{ icon: 'projectile', cost: 18, state: i === 2 ? 'queued' : 'ready', queue: 1 }, { slot: 'S', icon: 'burst', cost: 40, state: i === 3 ? 'windup' : 'cooldown', charged: 0.5, seconds: 2.2, progress: 0.6 }] }); });
var world = [h('div', { key: 0, style: { position: 'absolute', left: 195, top: 120 } }, h(V.WorldMarker, { kind: 'enemy', size: 44 })), h('div', { key: 1, style: { position: 'absolute', left: 195, top: 88 } }, h(V.WorldHealthBar, { team: 'enemy', rank: 'boss', health: 0.64, ghost: 0.72 })),
  h('div', { key: 2, style: { position: 'absolute', left: 120, top: 260 } }, h(V.WorldMarker, { kind: 'ally', size: 28 })), h('div', { key: 3, style: { position: 'absolute', left: 270, top: 260 } }, h(V.WorldMarker, { kind: 'queued', size: 28 }))];
render(h('div', { style: { position: 'relative', width: 390, height: 844, margin: -16, background: 'radial-gradient(70% 45% at 50% 20%, var(--scene-neon) 0%, transparent 70%), linear-gradient(180deg, var(--dark-50) 0%, var(--scene-ground) 100%)' } },
  h('div', { className: 'vb-root', style: { position: 'absolute', left: 0, right: 0, top: 48, bottom: 34, display: 'flex' } },
    h(V.BattleHudView, { hud: { wave: 10, boss: true, score: 18240, power: 22, focus: 60, credits: 90, queue: queue, queueTitle: 'MANUAL QUEUE', queueHint: 'Fires before autoplay · tap to cancel' }, operatives: ops, announcement: 'BOSS · SIGNAL WARDEN',
      boss: { name: 'Signal Warden', health: 0.64, stability: 0.35, detail: '64% · enrage at 35%', shielded: true, shieldDetail: '2 pylons', interruptible: true }, world: world }))));
""", width=390, page=True)


for name, (group, height, subtitle, body, width, page) in P.items():
    d = os.path.join(OUT, name)
    os.makedirs(d, exist_ok=True)
    w = (f' width={width}' if width else '') + (' page' if page else '')
    html = (
        f'<!-- @dsCard group="{group}" height={height}{w} subtitle="{subtitle}" -->\n<!doctype html>\n<html lang="en">\n'
        f'<head><meta charset="utf-8"><title>{name} — Veilbreak</title></head>\n<body class="vb-stage">\n'
        f'<div id="root" class="vb-root"></div>\n<script>\n{PRELUDE}{body.strip()}\n</script>\n</body>\n</html>\n'
    )
    pathlib.Path(d, 'preview.html').write_text(html)
print(len(P), 'previews')
