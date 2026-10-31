(function () {
  'use strict';
  var D = JSON.parse(document.getElementById('hb-data').textContent);
  var E = {}; D.entries.forEach(function (x) { E[x.id] = x; });
  var TIER = {}; D.tiers.forEach(function (t) { TIER[t.id] = t; });
  var STATUS = { shipped: 'Shipped', partial: 'Partial', proposed: 'Proposed' };
  var STATUS_NOTE = { shipped: 'Exists in com.veilbreak.gui as described.', partial: 'Exists, but the contract here is incomplete or spread over several types.', proposed: 'Spec only. Not in the package yet.' };
  var view = document.getElementById('view'), side = document.getElementById('side');
  var roots = [];

  function esc(s) { return String(s == null ? '' : s).replace(/[&<>"]/g, function (c) { return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c]; }); }
  function store(k, v) { try { if (v === undefined) return localStorage.getItem('hb.' + k); localStorage.setItem('hb.' + k, v); } catch (e) { return null; } }
  function toast(t) { var el = document.getElementById('toast'); el.textContent = t; el.hidden = false; clearTimeout(toast.t); toast.t = setTimeout(function () { el.hidden = true; }, 1600); }
  function copy(text, label) {
    function done() { toast('Copied · ' + (label || text.split('\n')[0].slice(0, 60))); }
    try { navigator.clipboard.writeText(text).then(done, function () { toast('Select the text to copy'); }); } catch (e) { toast('Select the text to copy'); }
  }

  /* ── C#, UXML and USS highlighting (small, safe: escape first) ── */
  function hl(code, lang) {
    var re, kinds;
    if (lang === 'uss') { re = /(\/\*[\s\S]*?\*\/)|(--vb-[a-z0-9-]+)|(\.[a-z][a-z0-9_-]*)/g; kinds = ['c', 's', 'k']; }
    else if (lang === 'uxml') { re = /(<\/?[\w:.]+)|([\w-]+)(?==")|("[^"]*")/g; kinds = ['k', 't', 's']; }
    else { re = /(\/\/[^\n]*|#[^\n]*)|("[^"\n]*")|\b(new|var|void|public|class|static|return|null|true|false|foreach|in|if|float|int|string|bool|readonly|const|event)\b|\b(Vb[A-Z]\w*|Ui|Popover|Dialog|Navigator|SafeAreaRoot|ButtonVariant|StatusKind|RarityTier|WorldTeam|PlatformState|[A-Z]\w*(?:Component|Composite|Layout|View|ViewModel))\b/g; kinds = ['c', 's', 'k', 't']; }
    var out = '', last = 0, m;
    while ((m = re.exec(code))) {
      out += esc(code.slice(last, m.index));
      var k = 0; for (var g = 1; g < m.length; g++) if (m[g] != null) { k = g; break; }
      out += '<span class="' + kinds[k - 1] + '">' + esc(m[0]) + '</span>'; last = re.lastIndex;
      if (m[0].length === 0) re.lastIndex++;
    }
    return out + esc(code.slice(last));
  }
  function codebox(label, code, lang) {
    if (!code) return '';
    return '<div class="codebox slim"><div class="codebox__head"><span>' + esc(label) + '</span><button data-copy="' + esc(code) + '">Copy</button></div><pre>' + hl(code, lang) + '</pre></div>';
  }

  /* ── stage flags (global, remembered per viewer) ── */
  var flags = { contrast: store('contrast') === '1', motion: store('motion') === '1' };
  function stageClasses() { return (flags.contrast ? ' vb-contrast' : '') + (flags.motion ? ' vb-reduced-motion vb-reduced-flashes' : ''); }
  function applyFlags() {
    ['contrast', 'motion'].forEach(function (k) { document.getElementById('t-' + k).setAttribute('aria-pressed', flags[k] ? 'true' : 'false'); });
    document.querySelectorAll('[data-stage]').forEach(function (el) {
      el.className = el.getAttribute('data-base') + stageClasses();
      if (flags.contrast) el.setAttribute('data-theme', 'contrast'); else el.removeAttribute('data-theme');
    });
  }
  ['contrast', 'motion'].forEach(function (k) {
    document.getElementById('t-' + k).addEventListener('click', function () { flags[k] = !flags[k]; store(k, flags[k] ? '1' : '0'); applyFlags(); });
  });

  /* ── previews ── */
  function unmount() { roots.forEach(function (r) { try { r.unmount(); } catch (e) { } }); roots = []; }
  function mountPreview(host, x) {
    var fn = PREVIEWS[x.id]; if (!fn) return;
    var phone = x.stage === 'board';
    var base = phone ? 'hb-scale' : 'hb-free vb-root';
    var el = document.createElement('div'); el.setAttribute('data-stage', ''); el.setAttribute('data-base', base);
    if (phone) {
      var hold = document.createElement('div'); hold.className = 'phone-hold';
      host.appendChild(hold); hold.appendChild(el);
      var fit = function () {
        var actual = host.getAttribute('data-actual') === '1';
        var avail = Math.max(240, host.clientWidth - 24), s = actual ? 1 : Math.min(1, avail / 390, 640 / 844);
        hold.style.width = (390 * s) + 'px'; hold.style.height = (844 * s) + 'px'; el.style.transform = 'scale(' + s + ')';
      };
      fit(); window.addEventListener('resize', fit); host.__fit = fit;
    } else { host.appendChild(el); el.style.minHeight = Math.min(900, x.height || 200) + 'px'; }
    applyFlags();
    var root = ReactDOM.createRoot(el); roots.push(root);
    try {
      fn(function (node) {
        var isBoard = node && node.props && node.props.className === 'hb-phone';
        if (phone && !isBoard) node = h('div', { className: 'hb-phone hb-phone--raw' }, h('div', { className: 'vb-root hb-phone__root' }, node));
        root.render(node);
      });
    } catch (err) { el.textContent = 'Preview failed: ' + err.message; }
  }

  /* ── nav ── */
  function counts(list) { var c = { shipped: 0, partial: 0, proposed: 0 }; list.forEach(function (x) { c[x.status]++; }); return c; }
  function navHtml() {
    var o = '<h3>Start</h3>' + link('overview', 'Overview') + link('setup', 'Unity setup') + link('web', 'Web kit') + '<a href="web/demo/">Web app demo<small>↗</small></a>' + link('name-map', 'Name map', D.entries.length) +
      '<h3>Foundations</h3>' + link('tokens', 'Tokens', D.tokens.length) + link('type', 'Type') + link('e-frame', 'Frames: slim & chipped');
    D.tiers.forEach(function (t) {
      var list = D.entries.filter(function (x) { return x.tier === t.id; });
      o += '<h3><span>' + esc(t.name) + '</span><span>' + list.length + '</span></h3>';
      list.forEach(function (x) { o += '<a href="#e-' + x.id + '" data-r="e-' + x.id + '"><i class="dot dot--' + x.status + '" title="' + STATUS[x.status] + '"></i>' + esc(x.name) + '</a>'; });
    });
    o += '<h3>Assets</h3>' + link('icons', 'Icon pack', D.icons.length) + link('fonts', 'Fonts & mark');
    return o;
    function link(r, t, n) { return '<a href="#' + r + '" data-r="' + r + '">' + esc(t) + (n != null ? '<small>' + n + '</small>' : '') + '</a>'; }
  }
  side.innerHTML = navHtml();
  document.getElementById('menu-btn').addEventListener('click', function () { var on = document.body.classList.toggle('nav-open'); this.setAttribute('aria-expanded', on); });

  /* ── pages ── */
  function pill(st) { return '<span class="pill pill--' + st + '"><i class="dot dot--' + st + '"></i>' + STATUS[st] + '</span>'; }
  function entryChip(id) { var x = E[id]; return '<a class="chip" href="#e-' + id + '"><i class="dot dot--' + x.status + '"></i>' + esc(x.name) + '</a>'; }
  function tokenVal(name) {
    var t = D.tokens.filter(function (k) { return '--vb-color-' + k.name === name || '--vb-' + k.name === name; })[0];
    return t ? (t.family === 'color' ? t.veil : t.veil) : '';
  }
  function swatch(v) { return /^(#|rgb|var\()/.test(v) ? v.replace(/var\(--([a-z0-9-]+)\)/, 'var(--$1)') : 'transparent'; }

  function overview() {
    var all = counts(D.entries);
    var o = '<div class="eyebrow">' + esc(D.package.name) + ' · v' + esc(D.package.version) + ' · Unity ' + esc(D.package.unity) + '</div>' +
      '<h1>Veilbreak GUI Handbook</h1><p class="lede">The Veil design system as you use it in Unity: every component sorted into atoms, composites, layouts and views, with the UI Toolkit type that implements it, the USS it is painted from, the tokens it reads and a live preview of its web mirror.</p>';
    o += '<div class="tiers">';
    D.tiers.forEach(function (t) {
      var list = D.entries.filter(function (x) { return x.tier === t.id; }), c = counts(list), n = list.length;
      o += '<section class="tier slim"><div class="tier__n">' + n + '</div><div class="tier__name">' + esc(t.name) + '</div><p>' + esc(t.blurb) + '</p><div class="tier__list">' +
        list.map(function (x) { return '<a href="#e-' + x.id + '"><i class="dot dot--' + x.status + '"></i><span>' + esc(x.name) + '</span></a>'; }).join('') + '</div>' +
        '<div class="statusbar" title="' + c.shipped + ' shipped · ' + c.partial + ' partial · ' + c.proposed + ' proposed"><i style="flex:' + c.shipped + ';background:var(--hb-teal)"></i><i style="flex:' + c.partial + ';background:var(--hb-amber)"></i><i style="flex:' + c.proposed + ';background:var(--hb-lav)"></i></div></section>';
    });
    o += '<section class="tier slim"><div class="tier__n">' + D.icons.length + '</div><div class="tier__name">Assets</div><p>Stroke icons, the mark and the fonts, with their USS classes and files.</p><div class="tier__list"><a href="#icons"><i class="dot dot--shipped"></i><span>Icon pack</span></a><a href="#fonts"><i class="dot dot--shipped"></i><span>Fonts &amp; mark</span></a><a href="#tokens"><i class="dot dot--shipped"></i><span>Tokens · ' + D.tokens.length + '</span></a></div></section></div>';
    o += '<div class="legend"><span><i class="dot dot--shipped"></i>' + all.shipped + ' shipped · in the package as described</span><span><i class="dot dot--partial"></i>' + all.partial + ' partial · exists, contract incomplete</span><span><i class="dot dot--proposed"></i>' + all.proposed + ' proposed · spec only</span></div>';
    o += '<div class="callout slim"><div><div class="eyebrow">New in 0.3.0 · everything shipped</div><h3>Two kinds of frame, one kit for Unity and the web</h3><p>Containers sit on the <strong>slim</strong> frame: a 4-unit chamfer and no border. The chipped frame stays for what you act on or what is selected. Text, labels, badges, tiles, cards, forms, stacks, lists, grids, the board and all thirteen screen views now ship in the package, each with a React twin in the <a class="ref" href="#web">web kit</a>.</p><p>Try them together in the <a class="ref" href="web/demo/">web app demo</a>: menu, squad, battle, pause, perks, results.</p><a class="go" href="#e-frame">FrameComponent →</a></div><div id="ov-preview"></div></div>';
    o += '<h2>From an empty scene to a running board</h2><div class="steps">' +
      step('Reference the package', '<code>"com.veilbreak.gui": "https://github.com/veil-break/veilbreak-design-system.git"</code> in <code>Packages/manifest.json</code>') +
      step('Use the panel settings', '<code>Runtime/Settings/VeilbreakPortrait.asset</code> · 390 × 844, match width, theme <code>VeilbreakGui.tss</code>') +
      step('Root and safe area', 'Class <code>vb-root</code> on the root, then <code>new SafeAreaRoot(root, settings, screenLayer)</code>; call <code>Refresh()</code> each frame.') +
      step('Bind, don\'t rebuild', 'Create components once and call <code>Bind(view)</code> every frame. They reuse their children.') + '</div>';
    o += '<h2>How to read an entry</h2><div class="prose"><ul><li><strong>Unity today</strong> names the type or USS hook that implements it now. The design name (<code>ButtonComponent</code>) is the tier contract; the <a class="ref" href="#name-map">name map</a> lists both.</li><li><strong>Built from / used by</strong> show the composition: atoms build composites, composites build views.</li><li><strong>USS</strong> is extracted from <code>Gui.uss</code>, <code>Layout.uss</code>, <code>World.uss</code> and the arcade\'s <code>Arcade.uss</code>; <strong>Tokens read</strong> lists every <code>--vb-*</code> variable those rules use.</li><li>Previews are the web mirror (<code>Tools~/web/kit.js</code>), which builds the same element tree and classes as the C#.</li></ul></div>';
    view.innerHTML = o;
    var host = document.getElementById('ov-preview');
    PREVIEWS.__ov = function (render) {
      render(h('div', { style: { display: 'flex', flexDirection: 'row', gap: 10, flexWrap: 'wrap' } },
        h(V.VbFrame, { kind: 'chipped', className: 'vb-panel', style: { width: 210, padding: '12px 14px' } }, h('div', { className: 'vb-l vb-t-label vb-section__title' }, 'CHIPPED · ACT ON IT'), h(V.VbSwitch, { text: 'HAPTICS', detail: 'Light pulse on accept', value: true })),
        h(V.VbFrame, { kind: 'slim', className: 'vb-panel vb-section', style: { width: 210 } }, h('div', { className: 'vb-l vb-t-label vb-section__title' }, 'SLIM · CONTAINERS'), h(V.VbSwitch, { text: 'HAPTICS', detail: 'Light pulse on accept', value: true }))));
    };
    mountPreview(host, { id: '__ov', stage: 'free', height: 150 });
    function step(t, d) { return '<div class="step slim"><b>' + t + '</b><p>' + d + '</p></div>'; }
  }

  function entry(x) {
    var t = TIER[x.tier];
    var o = '<div class="entry-head"><div class="eyebrow"><a href="#overview" style="text-decoration:none">' + esc(t.name) + '</a> ' + pill(x.status) + '</div><h1>' + esc(x.name) + '</h1><p class="lede">' + esc(x.summary) + '</p>' +
      '<div class="chips"><span class="chip" style="background:none;padding-left:0">Unity today</span>' + x.unity.map(function (u) { return '<span class="chip chip--code">' + esc(u) + '</span>'; }).join('') + '</div></div>';
    o += '<div class="entry-grid"><section class="stage slim" aria-label="Preview">' +
      '<div class="stage__bar"><span>' + (x.stage === 'board' ? 'Portrait board · 390 × 844' : 'Preview') + ' · web mirror</span>' + (x.stage === 'board' ? '<button id="actual">Actual size</button>' : '') + '</div>' +
      '<div class="stage__body' + (x.stage === 'board' ? '' : ' stage__body--free') + '" id="stage-host">' + (x.hasPreview ? '' : '<p style="padding:16px;color:var(--hb-ink-3)">No preview.</p>') + '</div></section>';
    o += '<aside class="facts">';
    o += '<div class="fact slim"><h5>Status</h5><p>' + STATUS_NOTE[x.status] + '</p></div>';
    if (x.uses.length) o += '<div class="fact slim"><h5>Built from</h5><div class="chips">' + x.uses.map(entryChip).join('') + '</div></div>';
    if (x.usedBy.length) o += '<div class="fact slim"><h5>Used by</h5><div class="chips">' + x.usedBy.map(entryChip).join('') + '</div></div>';
    if (x.blocks.length) o += '<div class="fact slim"><h5>USS blocks</h5><div class="chips">' + x.blocks.map(function (b) { return '<button class="chip chip--code" style="border:0;cursor:pointer" data-copy=".' + esc(b) + '">.' + esc(b) + '</button>'; }).join('') + '</div></div>';
    if (x.tokens.length) o += '<div class="fact slim"><h5>Tokens read · ' + x.tokens.length + '</h5><div class="swatches">' + x.tokens.slice(0, 18).map(function (k) { var v = tokenVal(k); return '<button class="sw" style="background:none;border:0;padding:0;cursor:pointer;text-align:left" data-copy="var(' + k + ')"><i style="background:' + (k.indexOf('--vb-color-') === 0 ? 'var(--' + k.slice(11) + ')' : 'transparent') + '"></i><span>' + esc(k) + '</span></button>'; }).join('') + (x.tokens.length > 18 ? '<p>+' + (x.tokens.length - 18) + ' more in the USS tab</p>' : '') + '</div></div>';
    o += '</aside></div>';
    var tabs = [['unity', 'Unity', 1]];
    if (x.spec) tabs.push(['spec', x.status === 'proposed' ? 'Proposal' : 'Spec', 1]);
    if (x.rules || x.doc) tabs.push(['guide', 'Guidelines', 1]);
    if (x.uss.length) tabs.push(['uss', 'USS', x.uss.length]);
    if (x.api.length) tabs.push(['api', 'C# API', x.api.reduce(function (a, b) { return a + b.members.length; }, 0)]);
    var cur = store('tab') || 'unity'; if (!tabs.some(function (t) { return t[0] === cur; })) cur = 'unity';
    o += '<div class="tabs" role="tablist">' + tabs.map(function (t) { return '<button class="tab" role="tab" data-tab="' + t[0] + '" aria-selected="' + (t[0] === cur) + '">' + t[1] + (t[0] === 'uss' || t[0] === 'api' ? '<sup>' + t[2] + '</sup>' : '') + '</button>'; }).join('') + '</div>';
    o += '<div class="panel" data-panel="unity"' + (cur === 'unity' ? '' : ' hidden') + '>' + codebox('C#', x.csharp, 'cs') + codebox('UXML · xmlns:vb="Veilbreak.Gui"', x.uxml, 'uxml') +
      '' + '</div>';
    if (x.spec) o += '<div class="panel prose" data-panel="spec"' + (cur === 'spec' ? '' : ' hidden') + '>' + x.spec + '</div>';
    if (x.rules || x.doc) o += '<div class="panel prose" data-panel="guide"' + (cur === 'guide' ? '' : ' hidden') + '>' + (x.rules ? '<h4>Rules</h4>' + x.rules : '') + (x.doc ? '<h4>Reference · ' + esc(x.docTitle) + '</h4>' + x.doc : '') + '</div>';
    if (x.uss.length) o += '<div class="panel" data-panel="uss"' + (cur === 'uss' ? '' : ' hidden') + '><div class="codebox slim"><div class="codebox__head"><span>' + x.uss.length + ' rules · from the package\'s USS</span><button data-copy="' + esc(x.uss.map(function (r) { return r.sel + ' { ' + r.body + ' }'; }).join('\n')) + '">Copy all</button></div><div style="padding:6px 14px">' +
      x.uss.map(function (r) { return '<div class="rule"><em>' + esc(r.file) + '</em><b>' + esc(r.sel) + '</b><div>' + r.body.split(/;\s*/).filter(Boolean).map(function (d) { return esc(d) + ';'; }).join('<br>').replace(/(--vb-[a-z0-9-]+)/g, '<span class="s">$1</span>') + '</div></div>'; }).join('') + '</div></div></div>';
    if (x.api.length) o += '<div class="panel" data-panel="api"' + (cur === 'api' ? '' : ' hidden') + '>' + x.api.map(function (a) {
      return '<div class="codebox slim"><div class="codebox__head"><span>' + esc(a.kind) + ' ' + esc(a.type) + (a.base ? ' : ' + esc(a.base) : '') + '</span><span style="text-transform:none;letter-spacing:0;font-family:var(--hb-mono)">' + esc(a.file) + '</span></div>' +
        (a.summary ? '<p style="margin:0;padding:10px 14px 0;color:var(--hb-ink-2);font-size:13px">' + esc(a.summary) + '</p>' : '') + '<pre>' + hl(a.members.join('\n'), 'cs') + '</pre></div>';
    }).join('') + '</div>';
    view.innerHTML = o;
    view.querySelectorAll('.tab').forEach(function (b) {
      b.addEventListener('click', function () {
        var k = b.getAttribute('data-tab'); store('tab', k);
        view.querySelectorAll('.tab').forEach(function (t) { t.setAttribute('aria-selected', t === b); });
        view.querySelectorAll('[data-panel]').forEach(function (p) { p.hidden = p.getAttribute('data-panel') !== k; });
      });
    });
    var host = document.getElementById('stage-host');
    if (x.hasPreview) mountPreview(host, x);
    var act = document.getElementById('actual');
    if (act) act.addEventListener('click', function () { var on = host.getAttribute('data-actual') !== '1'; host.setAttribute('data-actual', on ? '1' : '0'); act.textContent = on ? 'Fit' : 'Actual size'; if (host.__fit) host.__fit(); });
  }

  function nameMap() {
    var o = '<div class="eyebrow">Start</div><h1>Name map</h1><p class="lede">Every design-system name beside what implements it in Unity today. Use it to find the type you need now, and as the checklist for moving the package onto the four tiers.</p>';
    D.tiers.forEach(function (t) {
      var list = D.entries.filter(function (x) { return x.tier === t.id; });
      o += '<h2>' + esc(t.name) + '</h2><div class="tablewrap"><table><thead><tr><th>Design name</th><th>Unity today</th><th>Status</th><th>Built from</th></tr></thead><tbody>' +
        list.map(function (x) { return '<tr class="click" data-go="e-' + x.id + '"><td class="mono">' + esc(x.name) + '</td><td>' + x.unity.map(function (u) { return '<code>' + esc(u) + '</code>'; }).join(' ') + '</td><td>' + pill(x.status) + '</td><td>' + x.uses.map(function (u) { return esc(E[u].name.replace(/(Component|Composite|Layout|View)$/, '')); }).join(', ') + '</td></tr>'; }).join('') + '</tbody></table></div>';
    });
    view.innerHTML = o;
  }

  function tokensPage() {
    var fams = []; D.tokens.forEach(function (t) { if (fams.indexOf(t.family) < 0) fams.push(t.family); });
    var cur = store('fam') || 'color';
    var o = '<div class="eyebrow">Foundations</div><h1>Tokens</h1><p class="lede">One source, <code>Tokens~/tokens.json</code>, generated by <code>Tools~/tokens.py</code> into <code>Tokens.uss</code> (variables), <code>TokensContrast.uss</code> and <code>VbTokens.cs</code> (colours for code). Click a row to copy its USS variable.</p>' +
      '<div class="fams" role="group" aria-label="Family">' + fams.map(function (f) { return '<button class="fam" data-fam="' + f + '" aria-pressed="' + (f === cur) + '">' + f + '</button>'; }).join('') + '</div><div id="tok"></div>';
    view.innerHTML = o;
    function draw() {
      var rows = D.tokens.filter(function (t) { return t.family === cur; });
      var c = cur === 'color';
      document.getElementById('tok').innerHTML = '<div class="tablewrap"><table><thead><tr>' + (c ? '<th></th>' : '') + '<th>Token</th><th>Veil</th>' + (c ? '<th>Contrast</th>' : '') + '<th>USS</th>' + (c ? '<th>C#</th>' : '') + '<th>Usage</th></tr></thead><tbody>' +
        rows.map(function (t) {
          return '<tr class="click" data-copy="' + esc(t.uss ? 'var(' + t.uss + ')' : t.name) + '">' + (c ? '<td><span class="sw"><i style="background:var(--' + esc(t.name) + ')"></i></span></td>' : '') +
            '<td class="mono">' + esc(t.name) + (t.proposed ? ' <span class="pill pill--proposed" style="padding:3px 6px">proposed</span>' : '') + '</td><td class="mono">' + esc(t.veil) + '</td>' + (c ? '<td class="mono">' + esc(t.contrast || '—') + '</td>' : '') +
            '<td class="mono">' + esc(t.uss || '—') + '</td>' + (c ? '<td class="mono">' + esc(t.cs || '—') + '</td>' : '') + '<td style="min-width:240px">' + esc(t.usage) + '</td></tr>';
        }).join('') + '</tbody></table></div>';
    }
    view.querySelectorAll('.fam').forEach(function (b) { b.addEventListener('click', function () { cur = b.getAttribute('data-fam'); store('fam', cur); view.querySelectorAll('.fam').forEach(function (x) { x.setAttribute('aria-pressed', x === b); }); draw(); }); });
    draw();
  }

  function typePage() {
    var fam = { display: 'var(--font-display)', body: 'var(--font-body)', hud: 'var(--font-hud)' };
    var o = '<div class="eyebrow">Foundations</div><h1>Type</h1><p class="lede">TeX Gyre Adventor for titles, names and CTAs; Poppins for everything else; Share Tech Mono for HUD numerals. UI Toolkit has no <code>text-transform</code>: caps are passed in code. Font files are referenced as <code>-unity-font-definition: var(--vb-font-&lt;family&gt;-&lt;weight&gt;)</code>.</p>';
    o += D.type.map(function (s) {
      return '<div class="spec-row"><div class="meta"><b>' + esc(s.name) + '</b>' + esc(s.group) + ' · ' + esc(s.fontSize) + ' / ' + esc(s.lineHeight) + ' · ' + esc(s.fontWeight) + (s.letterSpacing ? ' · ' + esc(s.letterSpacing) : '') + (s.uss ? '<br>' + esc(s.uss) : '') + '</div>' +
        '<div><div class="sample" style="font-family:' + fam[s.family] + ';font-size:' + s.fontSize + ';line-height:' + s.lineHeight + ';font-weight:' + s.fontWeight + ';letter-spacing:' + (s.letterSpacing || 'normal') + '">' + esc(s.sample || s.name) + '</div><div style="font-size:12.5px;color:var(--hb-ink-3);margin-top:4px">' + esc(s.usage || '') + '</div></div></div>';
    }).join('');
    view.innerHTML = o;
  }

  function iconsPage() {
    var fams = ['All']; D.icons.forEach(function (i) { if (fams.indexOf(i.family) < 0) fams.push(i.family); });
    var cur = 'All', q = '';
    var o = '<div class="eyebrow">Assets</div><h1>Icon pack</h1><p class="lede">' + D.icons.length + ' stroke glyphs on a 24 grid (<code>Runtime/Icons/*.svg</code>, imported as vector images) plus ' + Object.keys(D.aliases).length + ' design-system aliases. <code>Icons.uss</code> maps <code>vb-icon--{name}</code> to each file. Click a glyph to copy <code>new VbIcon("name", 16)</code>.</p>' +
      '<div class="fams">' + fams.map(function (f) { return '<button class="fam" data-f="' + esc(f) + '" aria-pressed="' + (f === cur) + '">' + esc(f) + '</button>'; }).join('') + '</div>' +
      '<label class="search" style="display:block;max-width:360px;margin-bottom:12px"><input id="iq" type="search" placeholder="Filter icons" aria-label="Filter icons"></label><div class="icons" id="ig"></div>';
    view.innerHTML = o;
    var grid = document.getElementById('ig'), root = ReactDOM.createRoot(grid); roots.push(root);
    function draw() {
      var list = D.icons.filter(function (i) { return (cur === 'All' || i.family === cur) && (!q || i.name.indexOf(q) >= 0 || i.aliases.join(' ').indexOf(q) >= 0); });
      root.render(list.map(function (i) {
        return h('button', { key: i.name, className: 'ic slim', title: '.vb-icon--' + i.name + ' · ' + i.file, onClick: function () { copy('new VbIcon("' + i.name + '", 16)'); } },
          h(V.VbIcon, { icon: i.name, size: 28 }), h('span', null, i.name), i.aliases.length ? h('small', null, '= ' + i.aliases.join(', ')) : null);
      }));
    }
    view.querySelectorAll('.fam').forEach(function (b) { b.addEventListener('click', function () { cur = b.getAttribute('data-f'); view.querySelectorAll('.fam').forEach(function (x) { x.setAttribute('aria-pressed', x === b); }); draw(); }); });
    document.getElementById('iq').addEventListener('input', function (e) { q = e.target.value.trim().toLowerCase(); draw(); });
    draw();
  }

  function fontsPage() {
    var o = '<div class="eyebrow">Assets</div><h1>Fonts &amp; mark</h1><p class="lede">Font files live in <code>Runtime/Fonts/</code>; <code>FontPrimer</code> fills every face\'s dynamic atlas with the kit\'s glyphs on the first layout pass, so tracking never depends on which characters a session showed first.</p>';
    o += '<div class="tablewrap"><table><thead><tr><th>Family</th><th>Weight</th><th>File</th><th>USS variable</th></tr></thead><tbody>' + D.fonts.map(function (f) { return '<tr><td style="font-family:\'' + esc(f.family) + '\';font-weight:' + f.weight + ';font-size:16px;color:var(--hb-ink)">' + esc(f.family) + '</td><td class="mono">' + esc(f.weight) + '</td><td class="mono">Runtime/' + esc(f.file.replace(/^fonts\//, 'Fonts/')) + '</td><td class="mono">' + esc(f.uss || '—') + '</td></tr>'; }).join('') + '</tbody></table></div>';
    o += '<h2>The mark</h2><p class="prose"><code>VbMark</code> draws the VEIL/BREAK mark from <code>MarkGeometry</code> (15 parts) at a width; the splash uses the same data. Open <a class="ref" href="#e-mark">MarkComponent</a> for usage.</p><div class="stage slim" style="max-width:520px"><div class="stage__body stage__body--free" id="mark-host"></div></div>';
    view.innerHTML = o;
    mountPreview(document.getElementById('mark-host'), E.mark);
  }

  function setupPage() {
    view.innerHTML = '<div class="eyebrow">Start</div><h1>Unity setup</h1><p class="lede">From <code>Documentation~/integration.md</code> in the package.</p><div class="prose">' + D.integration + '</div>' +
      '<h2>Regenerate</h2>' + codebox('Shell', 'python3 Tools~/tokens.py          # Tokens.uss, TokensContrast.uss, VbTokens.cs, tokens.md\npython3 Tools~/icons.py           # Icons.uss from Runtime/Icons/*.svg\npython3 Tools~/metas.py            # .meta files for new package files\nTools~/web/build.sh OUT           # the design-system artifact\npython3 Tools~/web/package.py     # the web kit (Web~/dist)\npython3 Tools~/handbook/build_handbook.py . docs   # this handbook, the web kit and the demo (GitHub Pages)', 'sh') +
      '<h2>Slim frame · USS</h2>' + codebox('Gui.uss', D.slimUss, 'uss') + codebox('Layout.uss · the package\'s containers', D.slimMigration, 'uss');
  }

  function webPage() {
    view.innerHTML = '<div class="eyebrow">Start</div><h1>Web kit</h1><p class="lede">The same components for web apps: React 18 ports with the same names, element trees, classes and view models, generated from the package. Open the <a class="ref" href="web/demo/">web app demo</a> to see the views chained into a running app.</p><div class="prose">' + D.web + '</div>';
  }

  /* ── search ── */
  var q = document.getElementById('q');
  function searchPage(term) {
    term = term.toLowerCase();
    var hits = D.entries.filter(function (x) { return (x.name + ' ' + x.unity.join(' ') + ' ' + x.summary + ' ' + x.blocks.join(' ')).toLowerCase().indexOf(term) >= 0; });
    var toks = D.tokens.filter(function (t) { return (t.name + ' ' + (t.uss || '') + ' ' + (t.cs || '')).toLowerCase().indexOf(term) >= 0; }).slice(0, 30);
    var ics = D.icons.filter(function (i) { return i.name.indexOf(term) >= 0; }).slice(0, 40);
    var o = '<div class="eyebrow">Search</div><h1>“' + esc(term) + '”</h1>';
    o += '<h2>Components · ' + hits.length + '</h2>' + (hits.length ? '<div class="tablewrap"><table><tbody>' + hits.map(function (x) { return '<tr class="click" data-go="e-' + x.id + '"><td class="mono">' + esc(x.name) + '</td><td>' + esc(TIER[x.tier].name) + '</td><td>' + x.unity.map(function (u) { return '<code>' + esc(u) + '</code>'; }).join(' ') + '</td><td>' + pill(x.status) + '</td></tr>'; }).join('') + '</tbody></table></div>' : '<p class="prose">No component matches.</p>');
    if (toks.length) o += '<h2>Tokens · ' + toks.length + '</h2><div class="chips">' + toks.map(function (t) { return '<button class="chip chip--code" style="border:0;cursor:pointer" data-copy="' + esc(t.uss ? 'var(' + t.uss + ')' : t.name) + '">' + esc(t.uss || t.name) + '</button>'; }).join('') + '</div>';
    if (ics.length) o += '<h2>Icons · ' + ics.length + '</h2><div class="chips">' + ics.map(function (i) { return '<button class="chip chip--code" style="border:0;cursor:pointer" data-copy="new VbIcon(&quot;' + esc(i.name) + '&quot;, 16)">' + esc(i.name) + '</button>'; }).join('') + '</div>';
    view.innerHTML = o;
  }
  q.addEventListener('input', function () { var t = q.value.trim(); if (t.length >= 2) { unmount(); searchPage(t); } else if (!t) route(); });

  /* ── router ── */
  function route() {
    unmount(); document.body.classList.remove('nav-open');
    var r = (location.hash || '#overview').slice(1);
    side.querySelectorAll('a').forEach(function (a) { if (a.getAttribute('data-r') === r) a.setAttribute('aria-current', 'page'); else a.removeAttribute('aria-current'); });
    if (r.indexOf('e-') === 0 && E[r.slice(2)]) entry(E[r.slice(2)]);
    else if (r === 'tokens') tokensPage(); else if (r === 'type') typePage(); else if (r === 'icons') iconsPage(); else if (r === 'fonts') fontsPage();
    else if (r === 'name-map') nameMap(); else if (r === 'setup') setupPage(); else if (r === 'web') webPage(); else overview();
    window.scrollTo(0, 0);
    var cur = side.querySelector('[aria-current]'); if (cur && cur.scrollIntoView) cur.scrollIntoView({ block: 'nearest' });
  }
  document.addEventListener('click', function (e) {
    var c = e.target.closest('[data-copy]'); if (c) { e.preventDefault(); copy(c.getAttribute('data-copy')); return; }
    var g = e.target.closest('[data-go]'); if (g) { location.hash = g.getAttribute('data-go'); }
  });
  window.addEventListener('hashchange', route);
  applyFlags();
  route();
})();
