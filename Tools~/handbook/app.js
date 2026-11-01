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
  function toast(t) { var el = document.getElementById('toast'); el.textContent = t;  clearTimeout(toast.t); toast.t = setTimeout(function () { el.textContent = ''; }, 1600); }
  function copy(text, label) {
    function done() { toast('Copied · ' + (label || text.split('\n')[0].slice(0, 60))); }
    try { navigator.clipboard.writeText(text).then(done, function () { toast('Select the text to copy'); }); } catch (e) { toast('Select the text to copy'); }
  }

  /* ── C#, UXML and USS highlighting (small, safe: escape first) ── */
  function hl(code, lang) {
    var re, kinds;
    if (lang === 'uss') { re = /(\/\*[\s\S]*?\*\/)|(--vb-[a-z0-9-]+)|(\.[a-z][a-z0-9_-]*)/g; kinds = ['comment', 'string', 'keyword']; }
    else if (lang === 'uxml') { re = /(<\/?[\w:.]+)|([\w-]+)(?==")|("[^"]*")/g; kinds = ['keyword', 'type', 'string']; }
    else { re = /(\/\/[^\n]*|#[^\n]*)|("[^"\n]*")|\b(new|var|void|public|class|static|return|null|true|false|foreach|in|if|float|int|string|bool|readonly|const|event)\b|\b(Vb[A-Z]\w*|Ui|Popover|Dialog|Navigator|SafeAreaRoot|ButtonVariant|StatusKind|RarityTier|WorldTeam|PlatformState|[A-Z]\w*(?:Component|Composite|Layout|View|ViewModel))\b/g; kinds = ['comment', 'string', 'keyword', 'type']; }
    var out = '', last = 0, m;
    while ((m = re.exec(code))) {
      out += esc(code.slice(last, m.index));
      var k = 0; for (var g = 1; g < m.length; g++) if (m[g] != null) { k = g; break; }
      out += '<span data-syntax="' + kinds[k - 1] + '">' + esc(m[0]) + '</span>'; last = re.lastIndex;
      if (m[0].length === 0) re.lastIndex++;
    }
    return out + esc(code.slice(last));
  }
  function codebox(label, code, lang) {
    if (!code) return '';
    return '<figure data-content="code"><figcaption><span>' + esc(label) + '</span><button type="button" data-copy="' + esc(code) + '">Copy</button></figcaption><pre tabindex="0" role="region" aria-label="' + esc(label) + ' code"><code>' + hl(code, lang) + '</code></pre></figure>';
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
    } else { host.appendChild(el); }
    host.setAttribute('data-entry', x.id);
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

  /* Navigation is one DOM tree, moved into a native modal on narrow screens. */
  function navLink(route, title, count) {
    return '<li><a href="#' + route + '" data-r="' + route + '">' + esc(title) + (count == null ? '' : '<small>' + count + '</small>') + '</a></li>';
  }
  function navSection(id, title, links) {
    return '<section aria-labelledby="nav-' + id + '"><h2 id="nav-' + id + '">' + esc(title) + '</h2><ul>' + links + '</ul></section>';
  }
  function statusDot(status) { return '<span data-status="' + status + '" aria-hidden="true"></span>'; }
  function navHtml() {
    var html = navSection('start', 'Start', navLink('overview', 'Overview') + navLink('setup', 'Unity setup') + navLink('web', 'Web kit') +
      '<li><a href="web/demo/">Web app demo <small aria-hidden="true">↗</small></a></li>' + navLink('name-map', 'Name map', D.entries.length));
    html += navSection('foundations', 'Foundations', navLink('tokens', 'Tokens', D.tokens.length) + navLink('type', 'Type') + navLink('e-frame', 'Frames: slim & chipped'));
    D.tiers.forEach(function (tier) {
      var entries = D.entries.filter(function (entry) { return entry.tier === tier.id; });
      html += navSection(tier.id, tier.name + ' · ' + entries.length, entries.map(function (entry) {
        return '<li><a href="#e-' + entry.id + '" data-r="e-' + entry.id + '">' + statusDot(entry.status) + esc(entry.name) + '</a></li>';
      }).join(''));
    });
    return html + navSection('assets', 'Assets', navLink('icons', 'Icon pack', D.icons.length) + navLink('fonts', 'Fonts & mark'));
  }
  side.innerHTML = navHtml();
  var drawer = document.getElementById('navigation-drawer');
  var menuButton = document.getElementById('menu-btn');
  var sidebar = document.getElementById('navigation-sidebar');
  var mobile = window.matchMedia('(max-width: 860px)');
  function closeMenu() { if (drawer.open) drawer.close(); }
  function syncNavigation() {
    closeMenu();
    (mobile.matches ? drawer : sidebar).appendChild(side);
    menuButton.setAttribute('aria-expanded', 'false');
  }
  menuButton.addEventListener('click', function () {
    if (!mobile.matches) return;
    drawer.showModal();
    menuButton.setAttribute('aria-expanded', 'true');
    var current = side.querySelector('[aria-current="page"]');
    if (current) current.scrollIntoView({ block: 'nearest' });
  });
  drawer.addEventListener('close', function () { menuButton.setAttribute('aria-expanded', 'false'); });
  drawer.addEventListener('keydown', function (event) {
    if (event.key !== 'Tab') return;
    var controls = drawer.querySelectorAll('button, a[href]');
    var first = controls[0], last = controls[controls.length - 1];
    if (event.shiftKey && document.activeElement === first) { event.preventDefault(); last.focus(); }
    else if (!event.shiftKey && document.activeElement === last) { event.preventDefault(); first.focus(); }
  });
  drawer.addEventListener('click', function (event) {
    var rect = drawer.getBoundingClientRect();
    if (event.target === drawer && (event.clientX < rect.left || event.clientX > rect.right || event.clientY < rect.top || event.clientY > rect.bottom)) closeMenu();
  });
  side.addEventListener('click', function (event) { if (event.target.closest('a')) closeMenu(); });
  mobile.addEventListener('change', syncNavigation);
  syncNavigation();

  /* Shared document primitives. The component mirrors keep their own kit styles. */
  function lead(group, title, description) {
    return '<header><p>' + esc(group) + '</p><h1 tabindex="-1">' + esc(title) + '</h1>' + (description ? '<p>' + description + '</p>' : '') + '</header>';
  }
  function pill(status) { return '<span data-status-label="' + status + '">' + statusDot(status) + STATUS[status] + '</span>'; }
  function entryChip(id) { var entry = E[id]; return '<li><a href="#e-' + id + '">' + statusDot(entry.status) + '<span>' + esc(entry.name) + '</span></a></li>'; }
  function table(content, caption) { return '<div data-content="table" tabindex="0" role="region" aria-label="' + esc(caption) + '"><table><caption>' + esc(caption) + '</caption>' + content + '</table></div>'; }
  function heads(labels) { return '<thead><tr>' + labels.map(function (label) { return '<th scope="col">' + label + '</th>'; }).join('') + '</tr></thead>'; }
  function render(html, page) { view.innerHTML = html; view.setAttribute('data-page', page); }

  function overview() {
    var totals = { shipped: 0, partial: 0, proposed: 0 };
    D.entries.forEach(function (entry) { totals[entry.status]++; });
    var html = lead(D.package.name + ' · v' + D.package.version + ' · Unity ' + D.package.unity, 'Veilbreak GUI Handbook',
      'The Veil design system as you use it in Unity: every component sorted into atoms, composites, layouts and views, with the UI Toolkit type that implements it, the USS it is painted from, the tokens it reads and a live preview of its web mirror.');
    html += '<div id="overview-tiers">';
    D.tiers.forEach(function (tier) {
      var entries = D.entries.filter(function (entry) { return entry.tier === tier.id; });
      html += '<section><p><data value="' + entries.length + '">' + entries.length + '</data> components</p><h2>' + esc(tier.name) + '</h2><p>' + esc(tier.blurb) + '</p><ul>' + entries.map(function (entry) { return entryChip(entry.id); }).join('') + '</ul></section>';
    });
    html += '<section><p><data value="' + D.icons.length + '">' + D.icons.length + '</data> icons</p><h2>Assets</h2><p>Stroke icons, the mark and the fonts, with their USS classes and files.</p><ul><li><a href="#icons">Icon pack</a></li><li><a href="#fonts">Fonts &amp; mark</a></li><li><a href="#tokens">Tokens · ' + D.tokens.length + '</a></li></ul></section></div>';
    html += '<ul id="status-legend">' + Object.keys(totals).map(function (status) { return '<li>' + pill(status) + '<span>' + totals[status] + ' · ' + esc(STATUS_NOTE[status]) + '</span></li>'; }).join('') + '</ul>';
    html += '<section id="frame-introduction"><header><p>New in 0.3.0 · everything shipped</p><h2>Two kinds of frame, one kit for Unity and the web</h2><p>Containers sit on the <strong>slim</strong> frame: a 4-unit chamfer and no border. The chipped frame stays for what you act on or what is selected. Each component has a React twin in the <a href="#web">web kit</a>.</p><p>Try them together in the <a href="web/demo/">web app demo</a>: menu, squad, battle, pause, perks, results.</p><a href="#e-frame">FrameComponent →</a></header><div id="ov-preview" data-preview></div></section>';
    html += '<section><h2>From an empty scene to a running board</h2><ol id="setup-steps">' +
      step('Reference the package', '<code>"com.veilbreak.gui": "https://github.com/veil-break/veilbreak-design-system.git"</code> in <code>Packages/manifest.json</code>') +
      step('Use the panel settings', '<code>Runtime/Settings/VeilbreakPortrait.asset</code> · 390 × 844, match width, theme <code>VeilbreakGui.tss</code>') +
      step('Root and safe area', 'Class <code>vb-root</code> on the root, then <code>new SafeAreaRoot(root, settings, screenLayer)</code>; call <code>Refresh()</code> each frame.') +
      step('Bind, don\'t rebuild', 'Create components once and call <code>Bind(view)</code> every frame. They reuse their children.') + '</ol></section>';
    html += '<section><h2>How to read an entry</h2><ul><li><strong>Unity today</strong> names the type or USS hook that implements it now. The <a href="#name-map">name map</a> lists both design names and Unity types.</li><li><strong>Built from / used by</strong> show the composition: atoms build composites, composites build views.</li><li><strong>USS</strong> is extracted from the package; <strong>Tokens read</strong> lists the variables those rules use.</li><li>Previews are the web mirror, which builds the same element tree and classes as the C#.</li></ul></section>';
    render(html, 'overview');
    PREVIEWS.__ov = function (draw) {
      draw(h('div', { 'data-frame-comparison': '' },
        h(V.VbFrame, { kind: 'chipped', className: 'vb-panel' }, h('p', null, 'CHIPPED · ACT ON IT'), h(V.VbSwitch, { text: 'HAPTICS', detail: 'Light pulse on accept', value: true })),
        h(V.VbFrame, { kind: 'slim', className: 'vb-panel vb-section' }, h('p', null, 'SLIM · CONTAINERS'), h(V.VbSwitch, { text: 'HAPTICS', detail: 'Light pulse on accept', value: true }))));
    };
    mountPreview(document.getElementById('ov-preview'), { id: '__ov', stage: 'free' });
    function step(title, description) { return '<li><h3>' + title + '</h3><p>' + description + '</p></li>'; }
  }

  function entry(item) {
    var html = lead(TIER[item.tier].name, item.name, esc(item.summary));
    html += '<p>' + pill(item.status) + '</p><dl id="unity-types"><dt>Unity today</dt>' + item.unity.map(function (type) { return '<dd><code>' + esc(type) + '</code></dd>'; }).join('') + '</dl>';
    html += '<div id="entry-layout"><figure id="entry-preview"><figcaption>' + (item.stage === 'board' ? 'Portrait board · 390 × 844' : 'Preview') + ' · web mirror' +
      (item.stage === 'board' ? '<button type="button" id="actual" aria-pressed="false">Actual size</button>' : '') + '</figcaption><div id="stage-host" data-preview data-stage-kind="' + item.stage + '">' + (item.hasPreview ? '' : '<p>No preview.</p>') + '</div></figure>';
    html += '<aside aria-label="Component facts"><dl><div><dt>Status</dt><dd>' + STATUS_NOTE[item.status] + '</dd></div>';
    if (item.uses.length) html += '<div><dt>Built from</dt><dd><ul data-content="links">' + item.uses.map(entryChip).join('') + '</ul></dd></div>';
    if (item.usedBy.length) html += '<div><dt>Used by</dt><dd><ul data-content="links">' + item.usedBy.map(entryChip).join('') + '</ul></dd></div>';
    if (item.blocks.length) html += '<div><dt>USS blocks</dt><dd><ul data-content="links">' + item.blocks.map(function (block) { return '<li><button type="button" data-copy=".' + esc(block) + '"><code>.' + esc(block) + '</code></button></li>'; }).join('') + '</ul></dd></div>';
    if (item.tokens.length) html += '<div><dt>Tokens read · ' + item.tokens.length + '</dt><dd><ul data-content="swatches">' + item.tokens.slice(0, 18).map(function (name) {
      var color = name.indexOf('--vb-color-') === 0 ? name.slice(11) : '';
      return '<li><button type="button" data-copy="var(' + name + ')"><span data-color="' + color + '" aria-hidden="true"></span><code>' + esc(name) + '</code></button></li>';
    }).join('') + '</ul>' + (item.tokens.length > 18 ? '<p>+' + (item.tokens.length - 18) + ' more in the USS tab</p>' : '') + '</dd></div>';
    html += '</dl></aside></div>';
    var tabs = [['unity', 'Unity']];
    if (item.spec) tabs.push(['spec', item.status === 'proposed' ? 'Proposal' : 'Spec']);
    if (item.rules || item.doc) tabs.push(['guide', 'Guidelines']);
    if (item.uss.length) tabs.push(['uss', 'USS · ' + item.uss.length]);
    if (item.api.length) tabs.push(['api', 'C# API']);
    var selected = store('tab') || 'unity';
    if (!tabs.some(function (tab) { return tab[0] === selected; })) selected = 'unity';
    html += '<div role="tablist" aria-label="Component documentation">' + tabs.map(function (tab) {
      return '<button type="button" role="tab" id="tab-' + tab[0] + '" aria-controls="panel-' + tab[0] + '" data-tab="' + tab[0] + '" aria-selected="' + (tab[0] === selected) + '" tabindex="' + (tab[0] === selected ? '0' : '-1') + '">' + tab[1] + '</button>';
    }).join('') + '</div>';
    function panel(id, content) { return '<section role="tabpanel" id="panel-' + id + '" aria-labelledby="tab-' + id + '" tabindex="0"' + (selected === id ? '' : ' hidden') + '>' + content + '</section>'; }
    html += panel('unity', codebox('C#', item.csharp, 'cs') + codebox('UXML · xmlns:vb="Veilbreak.Gui"', item.uxml, 'uxml'));
    if (item.spec) html += panel('spec', item.spec);
    if (item.rules || item.doc) html += panel('guide', (item.rules ? '<h2>Rules</h2>' + item.rules : '') + (item.doc ? '<h2>Reference · ' + esc(item.docTitle) + '</h2>' + item.doc : ''));
    if (item.uss.length) html += panel('uss', codebox(item.uss.length + ' rules · from the package\'s USS', item.uss.map(function (rule) { return '/* ' + rule.file + ' */\n' + rule.sel + ' {\n  ' + rule.body.split(/;\s*/).filter(Boolean).join(';\n  ') + ';\n}'; }).join('\n\n'), 'uss'));
    if (item.api.length) html += panel('api', item.api.map(function (api) { return '<section><h2>' + esc(api.kind + ' ' + api.type) + '</h2><p><code>' + esc(api.file) + '</code></p>' + (api.summary ? '<p>' + esc(api.summary) + '</p>' : '') + codebox('C# API' + (api.base ? ' : ' + api.base : ''), api.members.join('\n'), 'cs') + '</section>'; }).join(''));
    render(html, 'entry');
    var buttons = Array.from(view.querySelectorAll('[role="tab"]'));
    function selectTab(button) {
      var id = button.getAttribute('data-tab'); store('tab', id);
      buttons.forEach(function (tab) { tab.setAttribute('aria-selected', String(tab === button)); tab.tabIndex = tab === button ? 0 : -1; });
      view.querySelectorAll('[role="tabpanel"]').forEach(function (panel) { panel.hidden = panel.id !== 'panel-' + id; });
    }
    buttons.forEach(function (button, index) {
      button.addEventListener('click', function () { selectTab(button); });
      button.addEventListener('keydown', function (event) {
        var next = event.key === 'ArrowRight' ? (index + 1) % buttons.length : event.key === 'ArrowLeft' ? (index + buttons.length - 1) % buttons.length : event.key === 'Home' ? 0 : event.key === 'End' ? buttons.length - 1 : -1;
        if (next < 0) return;
        event.preventDefault(); selectTab(buttons[next]); buttons[next].focus();
      });
    });
    var host = document.getElementById('stage-host');
    if (item.hasPreview) mountPreview(host, item);
    var actual = document.getElementById('actual');
    if (actual) actual.addEventListener('click', function () { var on = actual.getAttribute('aria-pressed') !== 'true'; actual.setAttribute('aria-pressed', String(on)); host.setAttribute('data-actual', String(on)); actual.textContent = on ? 'Fit to width' : 'Actual size'; });
  }

  function nameMap() {
    var html = lead('Start', 'Name map', 'Every design-system name beside what implements it in Unity today. Follow a component link to open its reference.');
    D.tiers.forEach(function (tier) {
      html += '<section><h2>' + esc(tier.name) + '</h2>' + table(heads(['Design name', 'Unity today', 'Status', 'Built from']) + '<tbody>' + D.entries.filter(function (entry) { return entry.tier === tier.id; }).map(function (entry) {
        return '<tr><th scope="row"><a href="#e-' + entry.id + '">' + esc(entry.name) + '</a></th><td>' + entry.unity.map(function (type) { return '<code>' + esc(type) + '</code>'; }).join(' ') + '</td><td>' + pill(entry.status) + '</td><td>' + entry.uses.map(function (id) { return esc(E[id].name.replace(/(Component|Composite|Layout|View)$/, '')); }).join(', ') + '</td></tr>';
      }).join('') + '</tbody>', tier.name + ' name map') + '</section>';
    });
    render(html, 'name-map');
  }

  function tokensPage() {
    var families = Array.from(new Set(D.tokens.map(function (token) { return token.family; })));
    var current = store('fam') || 'color';
    if (families.indexOf(current) < 0) current = families[0];
    render(lead('Foundations', 'Tokens', 'One source, <code>Tokens~/tokens.json</code>, generated into USS variables and C# colours. Use each token\'s copy button to copy its USS variable.') +
      '<fieldset id="token-families"><legend>Token family</legend>' + families.map(function (family) { return '<button type="button" data-family="' + family + '" aria-pressed="' + (family === current) + '">' + family + '</button>'; }).join('') + '</fieldset><div id="token-table"></div>', 'tokens');
    function draw() {
      var color = current === 'color';
      document.getElementById('token-table').innerHTML = table(heads(['Token', 'Veil'].concat(color ? ['Contrast'] : []).concat(['USS']).concat(color ? ['C#'] : []).concat(['Usage'])) + '<tbody>' + D.tokens.filter(function (token) { return token.family === current; }).map(function (token) {
        return '<tr><th scope="row"><button type="button" data-copy="' + esc(token.uss ? 'var(' + token.uss + ')' : token.name) + '" aria-label="Copy ' + esc(token.name) + '">' + (color ? '<span data-color="' + esc(token.name) + '" aria-hidden="true"></span>' : '') + '<code>' + esc(token.name) + '</code></button>' + (token.proposed ? pill('proposed') : '') + '</th><td><code>' + esc(token.veil) + '</code></td>' + (color ? '<td><code>' + esc(token.contrast || '—') + '</code></td>' : '') + '<td><code>' + esc(token.uss || '—') + '</code></td>' + (color ? '<td><code>' + esc(token.cs || '—') + '</code></td>' : '') + '<td>' + esc(token.usage) + '</td></tr>';
      }).join('') + '</tbody>', current + ' tokens');
    }
    view.querySelectorAll('[data-family]').forEach(function (button) { button.addEventListener('click', function () { current = button.getAttribute('data-family'); store('fam', current); view.querySelectorAll('[data-family]').forEach(function (item) { item.setAttribute('aria-pressed', String(item === button)); }); draw(); }); });
    draw();
  }

  function typePage() {
    render(lead('Foundations', 'Type', 'TeX Gyre Adventor for titles, names and CTAs; Poppins for everything else; Share Tech Mono for HUD numerals. UI Toolkit has no <code>text-transform</code>: caps are passed in code.') +
      '<dl id="type-specimens">' + D.type.map(function (style, index) { return '<div><dt><code>' + esc(style.name) + '</code></dt><dd><p>' + esc(style.group + ' · ' + style.fontSize + ' / ' + style.lineHeight + ' · ' + style.fontWeight) + '</p>' + (style.uss ? '<code>' + esc(style.uss) + '</code>' : '') + '</dd><dd><p data-specimen="' + index + '">' + esc(style.sample || style.name) + '</p><p>' + esc(style.usage || '') + '</p></dd></div>'; }).join('') + '</dl>', 'type');
  }

  function iconsPage() {
    var families = ['All'].concat(Array.from(new Set(D.icons.map(function (icon) { return icon.family; }))));
    var current = 'All', term = '';
    render(lead('Assets', 'Icon pack', D.icons.length + ' stroke glyphs on a 24 grid, plus ' + Object.keys(D.aliases).length + ' design-system aliases. Activate a glyph to copy its C# usage.') +
      '<fieldset id="icon-families"><legend>Icon family</legend>' + families.map(function (family) { return '<button type="button" data-family="' + esc(family) + '" aria-pressed="' + (family === current) + '">' + esc(family) + '</button>'; }).join('') + '</fieldset>' +
      '<search aria-label="Icons"><label for="iq">Filter icons</label><input id="iq" type="search" placeholder="Filter icons"></search><p id="icon-count" role="status"></p><ul id="icon-grid" aria-label="Icons"></ul>', 'icons');
    var grid = document.getElementById('icon-grid');
    function draw() {
      var icons = D.icons.filter(function (icon) { return (current === 'All' || icon.family === current) && (!term || (icon.name + ' ' + icon.aliases.join(' ')).indexOf(term) >= 0); });
      document.getElementById('icon-count').textContent = icons.length + ' icons';
      grid.innerHTML = icons.map(function (icon) {
        return '<li><button type="button" title=".vb-icon--' + esc(icon.name) + ' · ' + esc(icon.file) + '" aria-label="Copy ' + esc(icon.name) + ' icon" data-copy="new VbIcon(&quot;' + esc(icon.name) + '&quot;, 16)">' + icon.svg + '<code>' + esc(icon.name) + '</code>' + (icon.aliases.length ? '<small>= ' + esc(icon.aliases.join(', ')) + '</small>' : '') + '</button></li>';
      }).join('');
    }
    view.querySelectorAll('[data-family]').forEach(function (button) { button.addEventListener('click', function () { current = button.getAttribute('data-family'); view.querySelectorAll('[data-family]').forEach(function (item) { item.setAttribute('aria-pressed', String(item === button)); }); draw(); }); });
    document.getElementById('iq').addEventListener('input', function (event) { term = event.target.value.trim().toLowerCase(); draw(); });
    draw();
  }

  function fontsPage() {
    render(lead('Assets', 'Fonts & mark', 'Font files live in <code>Runtime/Fonts/</code>. The kit\'s font primer fills every face\'s dynamic atlas on the first layout pass.') +
      table(heads(['Family', 'Weight', 'File', 'USS variable']) + '<tbody>' + D.fonts.map(function (font, index) { return '<tr><th scope="row" data-font="' + index + '">' + esc(font.family) + '</th><td>' + esc(font.weight) + '</td><td><code>Runtime/' + esc(font.file.replace(/^fonts\//, 'Fonts/')) + '</code></td><td><code>' + esc(font.uss || '—') + '</code></td></tr>'; }).join('') + '</tbody>', 'Bundled fonts') +
      '<section><h2>The mark</h2><p><code>VbMark</code> draws the VEIL/BREAK mark from <code>MarkGeometry</code>. Open <a href="#e-mark">MarkComponent</a> for usage.</p><div id="mark-host" data-preview></div></section>', 'fonts');
    mountPreview(document.getElementById('mark-host'), E.mark);
  }
  function setupPage() {
    render(lead('Start', 'Unity setup', 'From <code>Documentation~/integration.md</code> in the package.') + '<section aria-label="Integration guide">' + D.integration + '</section>' +
      '<section><h2>Regenerate</h2>' + codebox('Shell', 'python3 Tools~/tokens.py\npython3 Tools~/icons.py\npython3 Tools~/metas.py\nTools~/web/build.sh OUT\npython3 Tools~/web/package.py\npython3 Tools~/handbook/build_handbook.py . docs', 'sh') + '</section><section><h2>Slim frame · USS</h2>' + codebox('Gui.uss', D.slimUss, 'uss') + codebox('Layout.uss · containers', D.slimMigration, 'uss') + '</section>', 'setup');
  }
  function webPage() { render(lead('Start', 'Web kit', 'React 18 ports with the same names, element trees, classes and view models, generated from the Unity package. Try the <a href="web/demo/">web app demo</a>.') + '<section aria-label="Web kit guide">' + D.web + '</section>', 'web'); }

  /* Search results stay in the content landmark; typing retains input focus. */
  var query = document.getElementById('q');
  function searchPage(term) {
    var normalized = term.toLowerCase();
    var hits = D.entries.filter(function (entry) { return (entry.name + ' ' + entry.unity.join(' ') + ' ' + entry.summary + ' ' + entry.blocks.join(' ')).toLowerCase().indexOf(normalized) >= 0; });
    var tokens = D.tokens.filter(function (token) { return (token.name + ' ' + (token.uss || '') + ' ' + (token.cs || '')).toLowerCase().indexOf(normalized) >= 0; }).slice(0, 30);
    var icons = D.icons.filter(function (icon) { return icon.name.indexOf(normalized) >= 0; }).slice(0, 40);
    var html = lead('Search', 'Results for “' + term + '”', '');
    html += '<section><h2>Components · ' + hits.length + '</h2>' + (hits.length ? table(heads(['Component', 'Tier', 'Unity today', 'Status']) + '<tbody>' + hits.map(function (entry) { return '<tr><th scope="row"><a href="#e-' + entry.id + '">' + esc(entry.name) + '</a></th><td>' + esc(TIER[entry.tier].name) + '</td><td>' + entry.unity.map(function (type) { return '<code>' + esc(type) + '</code>'; }).join(' ') + '</td><td>' + pill(entry.status) + '</td></tr>'; }).join('') + '</tbody>', 'Component search results') : '<p>No component matches.</p>') + '</section>';
    if (tokens.length) html += '<section><h2>Tokens · ' + tokens.length + '</h2><ul data-content="links">' + tokens.map(function (token) { return '<li><button type="button" data-copy="' + esc(token.uss ? 'var(' + token.uss + ')' : token.name) + '"><code>' + esc(token.uss || token.name) + '</code></button></li>'; }).join('') + '</ul></section>';
    if (icons.length) html += '<section><h2>Icons · ' + icons.length + '</h2><ul data-content="links">' + icons.map(function (icon) { return '<li><button type="button" data-copy="new VbIcon(&quot;' + esc(icon.name) + '&quot;, 16)"><code>' + esc(icon.name) + '</code></button></li>'; }).join('') + '</ul></section>';
    render(html, 'search');
    side.querySelectorAll('[aria-current]').forEach(function (link) { link.removeAttribute('aria-current'); });
    document.title = 'Search · Veilbreak GUI Handbook';
  }
  query.addEventListener('input', function () { var term = query.value.trim(); if (term.length >= 2) { unmount(); searchPage(term); } else { route(false, false); } });

  function route(focus, clearQuery) {
    unmount(); closeMenu();
    if (clearQuery !== false) query.value = '';
    var routeName = (location.hash || '#overview').slice(1);
    side.querySelectorAll('a').forEach(function (link) { if (link.getAttribute('data-r') === routeName) link.setAttribute('aria-current', 'page'); else link.removeAttribute('aria-current'); });
    if (routeName.indexOf('e-') === 0 && E[routeName.slice(2)]) entry(E[routeName.slice(2)]);
    else if (routeName === 'tokens') tokensPage(); else if (routeName === 'type') typePage(); else if (routeName === 'icons') iconsPage(); else if (routeName === 'fonts') fontsPage();
    else if (routeName === 'name-map') nameMap(); else if (routeName === 'setup') setupPage(); else if (routeName === 'web') webPage(); else overview();
    var heading = view.querySelector('h1');
    document.title = heading.textContent + ' · Veilbreak GUI Handbook';
    window.scrollTo(0, 0);
    if (focus !== false) heading.focus({ preventScroll: true });
    if (!mobile.matches) { var current = side.querySelector('[aria-current]'); if (current) current.scrollIntoView({ block: 'nearest' }); }
  }
  document.addEventListener('click', function (event) {
    var button = event.target.closest('[data-copy]');
    if (button) { event.preventDefault(); copy(button.getAttribute('data-copy')); }
    var link = event.target.closest('a[href]');
    if (link && link.hash && link.getAttribute('href') === location.hash && link.hash !== '#main') { event.preventDefault(); route(); }
  });
  window.addEventListener('hashchange', function () { if (location.hash !== '#main') route(); });
  applyFlags();
  route(false);
})();
