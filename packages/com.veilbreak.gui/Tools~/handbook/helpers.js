/* Handbook preview helpers (on top of Tools~/web/previews.py's PRELUDE: cap, title, row, col, cell, op, PERKS). */
var V = window.Veilbreak, h = React.createElement;
var STREET = 'linear-gradient(180deg, rgba(5,2,8,.15) 0%, rgba(5,2,8,.7) 100%), repeating-linear-gradient(90deg, transparent 0 62px, rgba(143,240,207,.04) 62px 63px),' +
  'radial-gradient(70% 45% at 50% 18%, rgba(180,108,255,.35) 0%, transparent 70%), linear-gradient(180deg, #2c263f 0%, #1b1526 100%)';
var MENU_BG = 'radial-gradient(60% 40% at 50% 30%, rgba(240,137,159,.28) 0%, transparent 70%), radial-gradient(80% 50% at 50% 100%, rgba(29,217,160,.10) 0%, transparent 70%), linear-gradient(180deg, #3a2a4d 0%, #1d1428 55%, #0b0710 100%)';
/** A 390 x 844 portrait board: safe area 48 / 34, a vb-root inside. Strings are taken as the background. */
function board() {
  var bg = STREET, kids = [];
  for (var i = 0; i < arguments.length; i++) { var a = arguments[i]; if (typeof a === 'string') bg = a; else if (a) kids.push(a); }
  return h('div', { className: 'hb-phone', style: { background: bg } },
    h('div', { className: 'hb-phone__status' }, h('span', null, '9:41'), h('span', null, '5G ▮▮▮')),
    h('div', { className: 'vb-root hb-phone__root' }, kids));
}
/** A labelled, dashed region for layout diagrams. */
function zone(label, style) {
  return h('div', { className: 'hb-zone', style: style }, h('div', { className: 'vb-l hb-zone__label' }, label));
}
/** Ui.Section: a titled settings panel. */
function section(title, kids) {
  return h(V.VbFrame, { className: 'vb-panel vb-section' }, h('div', { className: 'vb-l vb-t-label vb-section__title' }, title), kids);
}
/** The menu's thumb-zone block (MenuActionsSectionView). */
function menuActions() {
  return h('div', { className: 'menu__actions' },
    h(V.VbFrame, { className: 'vb-panel menu__checkpoint' },
      h('div', { className: 'menu__checkpoint-text' }, h('div', { className: 'vb-l vb-t-label' }, 'CHECKPOINT'), h('div', { className: 'vb-l menu__checkpoint-line' }, 'Wave 07 next · 9,420'), h('div', { className: 'vb-l vb-t-caption' }, '3 of 4 standing · 5 perks · seed 80A2EB4E')),
      h(V.VbButton, { title: 'RESUME', icon: 'play' })),
    h(V.VbButton, { title: 'PLAY', variant: 'primary', sub: 'Squad · Aria · Sol · Aegis · Nyra', icon: 'chevron', className: 'menu__play' }),
    h('div', { className: 'vb-row menu__row' }, h(V.VbButton, { title: 'HIGH SCORES', className: 'vb-half', style: { width: 124, marginRight: 60 } }), h(V.VbButton, { title: 'SETTINGS', icon: 'settings', className: 'vb-half', style: { width: 124 } })));
}
