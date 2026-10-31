"""The Veilbreak GUI taxonomy: four tiers plus assets, mapped onto com.veilbreak.gui.

Each entry names a design-system component (ButtonComponent, FormComposite, GridLayout, PauseOverlayView …), the Unity
type or USS class that implements it today, its status, what it is built from, and how to use it in Unity. Specs for
things the package does not have yet (the slim frame, generic Card/Tile/Badge, the views) are proposals: they describe
the contract a future package version should ship, and the handbook marks them so.

Fields
  id        stable handbook id (hash route)
  tier      atom | composite | layout | view | asset
  name      design-system name
  unity     Unity types / USS hooks that implement it today (first one is "the" type)
  status    shipped (exists as described) | partial (exists, contract incomplete) | proposed (spec only)
  summary   one sentence
  uses      ids of the lower-tier parts it is built from
  blocks    USS block classes whose rules the handbook extracts from Gui.uss / Layout.uss / World.uss / Arcade.uss
  doc       heading in Documentation~/components.md to embed (optional)
  csharp    C# usage
  uxml      UXML usage (optional)
  rules     do / don't bullets
  spec      proposal text (markdown) for partial/proposed entries
  preview   key into Tools~/web/previews.py P, or JS (calls render(node)); views render inside board()
  stage     'free' (default) | 'board' (390 x 844 phone) | 'wide'
  height    preview height hint (px, free stage)
"""

SLIM_USS = """/* Gui.uss — FrameComponent kinds. Add after .vb-panel rules. VbShape already supports both: no C# change. */
/* Slim: the default container. Small chamfer, no edge, fill only. */
.vb-frame--slim { --vb-cut: 4; --vb-bw: 0; --vb-fill: var(--vb-color-panel); }
.vb-frame--slim.vb-frame--raised { --vb-fill: var(--vb-color-raised); }
.vb-frame--slim.vb-frame--solid  { --vb-fill: var(--vb-color-panel-solid); }
.vb-frame--slim.vb-frame--ally   { --vb-fill: var(--vb-color-ally-soft); }
.vb-frame--slim.vb-frame--enemy  { --vb-fill: var(--vb-color-enemy-soft); }
.vb-frame--slim.vb-frame--power  { --vb-fill: var(--vb-color-power-soft); }
/* Chipped: the existing look, reserved for things you act on or that are selected. */
.vb-frame--chipped { --vb-cut: var(--vb-cut-md); --vb-bw: 1; --vb-edge: var(--vb-color-line); }
/* High contrast: a container with no edge must still separate from the world at 3:1. */
.vb-contrast .vb-frame--slim { --vb-bw: 1; --vb-edge: var(--vb-color-line-strong); }"""

SLIM_MIGRATION = """/* Arcade.uss / Gui.uss — move today's containers to the slim kind (same selectors, no markup change). */
.vb-section, .vb-dialog, .vb-details, .vb-score, .vb-perk-row, .vb-rule, .vb-result,
.vb-queue__row, .vb-merchant__exchange, .menu__checkpoint, .pause__perks, .results__score,
.settings__note, .menu__best, .pause__chip {
    --vb-cut: 4; --vb-bw: 0;
}
.vb-contrast .vb-section, .vb-contrast .vb-dialog, .vb-contrast .vb-details, .vb-contrast .vb-score,
.vb-contrast .vb-perk-row, .vb-contrast .vb-rule, .vb-contrast .vb-result {
    --vb-bw: 1; --vb-edge: var(--vb-color-line-strong);
}"""

ENTRIES = []


def e(**kw):
    kw.setdefault('uses', [])
    kw.setdefault('blocks', [])
    kw.setdefault('rules', [])
    kw.setdefault('stage', 'free')
    kw.setdefault('height', 220)
    ENTRIES.append(kw)


# ════════════════════════════════════════════════════════════════════════════
# ATOMS — base components. They may (and should) use other atoms.
# ════════════════════════════════════════════════════════════════════════════

e(id='text', tier='atom', name='TextComponent', unity=['Label', 'Ui.Text', 'vb-t-*'], status='partial',
  summary='Every run of text: a Label with one type class. The type class is the whole contract.',
  blocks=['vb-t-display', 'vb-t-title', 'vb-t-label', 'vb-t-badge', 'vb-t-body', 'vb-t-caption', 'vb-t-num', 'vb-t-ally', 'vb-t-enemy', 'vb-t-power'],
  csharp='Ui.Text(parent, "Combat, focus and world travel are frozen", "vb-t-caption");\nUi.Text(parent, "PAUSED", "vb-t-display");      // caps are passed, not styled\nvar n = Ui.Text(row, Ui.Number(18240), "vb-t-num"); // Share Tech Mono, tabular',
  uxml='<ui:Label text="PAUSED" class="vb-t-display" />\n<ui:Label text="Combat, focus and world travel are frozen" class="vb-t-caption" />',
  rules=['One type class per label: `vb-t-display`, `-title`, `-label`, `-badge`, `-body`, `-caption`, `-num`. Tone classes (`vb-t-ally`, `-enemy`, `-power`) only recolour.',
         'UI Toolkit has no `text-transform`. Uppercase in code (`ToUpperInvariant()`) and pass normal case in data.',
         'Numbers that tick use `vb-t-num` (Share Tech Mono) or `Ui.Number`/`Ui.Seconds`, which cache strings so per-frame binding makes no garbage.',
         'Text over the 3D world gets the soft drop shadow (header titles, HUD numbers already have it).'],
  spec='**Proposed:** a `TextComponent : Label` with a `Style` attribute (`Display … Caption`, `Num`) and a `Tone` (`Default`, `Ally`, `Enemy`, `Power`, `Muted`), so UXML can write `<vb:TextComponent style="Caption" tone="Muted" text="…"/>` and the class juggling disappears. Until then, `Ui.Text` + a `vb-t-*` class is the contract.',
  preview=r'''render(col([
  h('div', { className: 'vb-l vb-t-display' }, 'PAUSED'),
  h('div', { className: 'vb-l vb-t-title' }, 'SETTINGS'),
  h('div', { className: 'vb-l vb-t-label' }, 'ACTIVE PERKS · 3'),
  h('div', { className: 'vb-l vb-t-badge' }, 'OFFLINE · LOCAL SCORES'),
  h('div', { className: 'vb-l vb-t-body', style: { whiteSpace: 'normal', maxWidth: 340 } }, 'Drag an operative to move. Drag A or S onto the battlefield to aim.'),
  h('div', { className: 'vb-l vb-t-caption' }, 'Seed 80A2EB4E · rules 3 · checkpoint after wave 06'),
  row([h('div', { className: 'vb-l vb-t-num', style: { fontSize: 22 } }, '18,240'), h('div', { className: 'vb-l vb-t-label vb-t-ally' }, 'ALLY'), h('div', { className: 'vb-l vb-t-label vb-t-enemy' }, 'ENEMY'), h('div', { className: 'vb-l vb-t-label vb-t-power' }, 'POWER')], 14, { alignItems: 'baseline' })
], 10, { alignItems: 'flex-start' }));''', height=300)

e(id='label', tier='atom', name='LabelComponent', unity=['vb-t-label', 'vb-t-badge', 'vb-panel__title'], status='partial',
  summary='The caps eyebrow over a section, field or value: tracked-out, small, secondary text.',
  uses=['text'], blocks=['vb-panel__title', 'vb-page__label', 'vb-section__title'],
  csharp='Ui.Text(section, "AUDIO", "vb-t-label", "vb-section__title");\nUi.Text(page, "ROSTER · ALL 8 UNLOCKED", "vb-t-label", "vb-page__label");',
  rules=['Always caps, always short: a noun or "NOUN · COUNT". Never a sentence.',
         'Labels name what follows; they are never tappable on their own.',
         'Use `vb-t-badge` (10 px) inside chips and cards, `vb-t-label` (11 px) over sections and fields.'],
  spec='**Proposed:** `LabelComponent(text, size: Label|Badge, tone)` uppercases for you and carries the section rhythm (`margin-bottom: 4`). It replaces the three ad-hoc classes above.',
  preview=r'''render(col([
  h(V.VbFrame, { className: 'vb-panel vb-frame--slim', style: { width: 320, padding: 14 } }, h('div', { className: 'vb-l vb-t-label' }, 'AUDIO'), h('div', { className: 'vb-l vb-t-caption' }, 'Section label · vb-t-label')),
  row([cell(h('div', { className: 'vb-l vb-t-badge' }, 'WAVE 12'), 'badge'), cell(h('div', { className: 'vb-l vb-t-label vb-t-ally' }, 'CHECKPOINT'), 'label · ally'), cell(h('div', { className: 'vb-l vb-t-label' }, 'SQUAD · TAP A SLOT'), 'page label')], 18)
], 14, { alignItems: 'flex-start' }));''', height=170)

e(id='glitch-text', tier='atom', name='GlitchTextComponent', unity=['GlitchText', 'Glitch.In/Out/Pulse/Slam'], status='shipped',
  summary='A Label that projects in and out with clipped red/cyan bands. Headlines and announcements only.',
  uses=['text'], blocks=['vb-glitch'], doc='Projection and the road economy',
  csharp='var title = new GlitchText { text = "CHOOSE YOUR ROAD" }.Classes("vb-t-display").Into(page);\nGlitch.In(title);            // falls back to a fade under vb-reduced-motion / vb-reduced-flashes',
  rules=['One projected headline per screen. Body copy never glitches.', 'Schedules cancel on replacement and detach; you never have to clean up.'],
  preview='GlitchText', height=140)

e(id='icon', tier='atom', name='IconComponent', unity=['VbIcon'], status='shipped',
  summary='A 24-grid stroke glyph by name, tinted by colour. 99 glyphs plus 8 design-system aliases.',
  blocks=['vb-icon'], doc='VbIcon',
  csharp='new VbIcon("cooldown", 16).Into(row);\nnew VbIcon("chevron", 16).Tint(VbTokens.AllyText).Into(more);',
  uxml='<vb:VbIcon icon="settings" style="width: 20px; height: 20px;" />',
  rules=['Sizes: 12 in badges, 14 in status chips, 16 in rows, 20 in buttons, 36 on road signs.', 'An icon never carries meaning alone: pair it with a word or a tooltip.', 'Names are kebab-case in USS (`vb-icon--skill-aria-needle-storm`) and any case in C#.'],
  preview='VbIcon', height=420)

e(id='mark', tier='atom', name='MarkComponent', unity=['VbMark', 'MarkGeometry'], status='shipped',
  summary='The slash-cut VEIL/BREAK mark at a width; its height follows the 513 × 259 artwork.',
  blocks=['vb-mark'], doc='VbMark',
  csharp='new VbMark(240).Into(brand);   // MarkGeometry.Parts drives the splash too',
  rules=['The mark appears once per screen at most, on the main menu and the splash.'],
  preview='VbMark', height=220)

e(id='frame', tier='atom', name='FrameComponent', unity=['VbShape', 'VbFrame', 'vb-panel', 'Ui.Panel'], status='partial',
  summary='The surface everything sits on. Two kinds: slim (the default container) and chipped (things you act on or that are selected).',
  blocks=['vb-shape', 'vb-panel', 'vb-frame'], doc='VbShape',
  csharp='// Slim container: the default for sections, sheets, rows and cards\nvar box = Ui.Panel(parent, "vb-frame--slim");\n// Chipped: kept for interactive or selected surfaces\nvar link = Ui.Panel(parent, "vb-panel--raised");   // today\'s look',
  uxml='<vb:VbFrame class="vb-panel vb-frame--slim" />\n<vb:VbFrame class="vb-panel vb-panel--solid" />',
  rules=['**Slim** (cut 4, no edge, fill only) for every container: sections, dialog sheets, list rows, cards, popovers, the HUD band.',
         '**Chipped** (cut 6–14, hairline edge) only where the frame means something: buttons, tiles you pick, the selected thing, holo frames, focus.',
         'Never nest chipped in chipped. A chipped control inside a slim container is the normal case.',
         'Slim frames get their edge back under `vb-contrast` so containers still separate at 3:1.',
         'Fills come from tokens: `panel`, `panel-solid`, `raised`, and the `*-soft` tints behind their own text colour.'],
  spec='**New container kind: slim.** The kit has one container look today: the chipped frame (10–14 cut, hairline edge). On a portrait screen every section, row and sheet draws its own border, and the screen reads as a stack of outlines (see Settings and Pause). The slim frame drops the edge and shrinks the chamfer to 4, so containers read as quiet tonal planes and the chipped look is left for what you act on.\n\n`VbShape` already reads `--vb-cut` and `--vb-bw`, so the slim kind is **pure USS**:\n\n```css\n' + SLIM_USS + '\n```\n\nProposed token: `cut-xs: 4px` (USS `--vb-cut-xs`). Migration of today\'s containers is a selector list, below. Use the **Slim containers** switch in the preview bar to see it on any screen.\n\n```css\n' + SLIM_MIGRATION + '\n```',
  preview=r'''function box(cls, t, sub) { return h(V.VbFrame, { className: 'vb-panel ' + cls, style: { width: 168, height: 92, padding: 12 } }, h('div', { className: 'vb-l vb-t-label' }, t), h('div', { className: 'vb-l vb-t-caption' }, sub)); }
render(col([
  h('div', { className: 'vb-l vb-t-badge' }, 'SLIM · containers'),
  row([box('vb-frame--slim', 'SLIM', 'panel · default'), box('vb-frame--slim vb-frame--raised', 'RAISED', 'raised fill'), box('vb-frame--slim vb-frame--solid', 'SOLID', 'sheets · popovers')], 10),
  row([box('vb-frame--slim vb-frame--ally', 'ALLY', 'ally-soft'), box('vb-frame--slim vb-frame--power', 'POWER', 'power-soft'), box('vb-frame--slim vb-frame--enemy', 'ENEMY', 'enemy-soft')], 10),
  h('div', { className: 'vb-l vb-t-badge', style: { marginTop: 8 } }, 'CHIPPED · interactive / selected'),
  row([box('', 'PANEL', 'vb-panel'), box('vb-panel--solid', 'SOLID', 'vb-panel--solid'), box('vb-panel--raised vb-panel--brackets', 'SELECTED', 'brackets')], 10)
], 10, { alignItems: 'flex-start' }));''', height=420)

e(id='button', tier='atom', name='ButtonComponent', unity=['VbButton', 'ButtonVariant'], status='shipped',
  summary='A chipped button: primary (one per screen), secondary, light, danger, ghost. Blocked buttons say why.',
  uses=['frame', 'text', 'icon'], blocks=['vb-button'], doc='VbButton',
  csharp='new VbButton("RESUME", ButtonVariant.Primary, "Returns to selected speed", "chevron", Resume) { name = "resume" }.Into(sheet);\nplay.Blocked = "Pick 1 more to start";   // disables, dashes, shows the reason',
  uxml='<vb:VbButton title="PLAY" variant="Primary" sub="Squad · 4" icon="chevron" />',
  rules=['One primary per screen, bottom of the thumb zone.', 'Blocked is never just greyed: the sub line becomes the reason, in under five words.', 'Buttons stay chipped when containers go slim: the chamfer is how a thumb finds them.'],
  preview='VbButton', height=440)

e(id='badge', tier='atom', name='BadgeComponent', unity=['StatusChip', 'CreditsChip', 'vb-t-badge', 'menu__best', 'pause__chip'], status='partial',
  summary='A small slim chip: a word, optionally a glyph and a value. Status, currency, counts and tags.',
  uses=['frame', 'text', 'icon'], blocks=['vb-status', 'vb-credits'],
  csharp='// Today: one class per use\nnew StatusChip(StatusKind.Exposed, null, "+40% dmg", "2.1s").Into(row);\nnew CreditsChip().Into(hud).Set(120);',
  rules=['Height 22, slim frame, never interactive by itself (a tappable chip is a PerkChip, a composite).', 'Tone carries meaning only with a word: EXPOSED, AUTO, NEW.', 'Exposed is the only inverted badge.'],
  spec='**Proposed:** one `BadgeComponent(text, icon = null, value = null, tone = Neutral)` with tones `Neutral`, `Ally`, `Enemy`, `Power`, `Warn`, `Inverted`. `StatusChip` and `CreditsChip` become presets (`Badge.Status(kind)`, `Badge.Credits(n)`), and the arcade\'s `menu__best` / `pause__chip` panels become plain badges.',
  preview=r'''var S = V.StatusChip;
render(col([
  row([h(S, { kind: 'exposed', detail: '+40% dmg', time: '2.1s' }), h(S, { kind: 'silenced' }), h(S, { kind: 'barrier', detail: '60' })], 6),
  row([h(V.CreditsChip, { credits: 120 }), h(V.VbFrame, { className: 'vb-badge-demo vb-frame--slim', style: { flexDirection: 'row', alignItems: 'center', height: 22, padding: '0 8px', gap: 6 } }, h(V.VbIcon, { icon: 'burst', size: 12 }), h('div', { className: 'vb-l vb-t-badge' }, 'SHARPENED OUTPUT ×2'))], 6),
  row([h(V.VbFrame, { className: 'vb-frame--slim vb-frame--ally', style: { flexDirection: 'row', alignItems: 'center', height: 22, padding: '0 8px' } }, h('div', { className: 'vb-l vb-t-badge vb-t-ally' }, 'AUTO')), h(V.VbFrame, { className: 'vb-frame--slim vb-frame--power', style: { flexDirection: 'row', alignItems: 'center', height: 22, padding: '0 8px' } }, h('div', { className: 'vb-l vb-t-badge vb-t-power' }, 'QUEUE #2'))], 6)
], 10, { alignItems: 'flex-start' }));''', height=160)

e(id='marker', tier='atom', name='MarkerComponent', unity=['WorldMarker'], status='shipped',
  summary='Who something belongs to, by shape: allies are circles, enemies diamonds. Selected, queued and move variants.',
  uses=['icon'], blocks=['vb-world-marker'], doc='WorldMarker',
  csharp='layer.Marker(id, worldPos, MarkerKind.Ally, 28);   // inside WorldLayer.BeginFrame / EndFrame',
  rules=['Shape before colour, always: the high-contrast theme and colour-blind players read the shape.'],
  preview='WorldMarker', height=110)

e(id='meter', tier='atom', name='MeterComponent', unity=['VbMeter'], status='shipped',
  summary='A flat track with a fill, an extra span (barrier), a dashed cost preview and numbered marks.',
  blocks=['vb-meter'], doc='VbMeter',
  csharp='var m = new VbMeter().Into(row);\nm.Set(.62f, .08f);         // fill, barrier span\nm.Preview(.44f, .62f);     // the cost being aimed\nm.Marks(new[] { .18f, .58f });',
  rules=['Meters never ramp green → red. Team colour plus a number nearby.'],
  preview='VbMeter', height=150)

e(id='world-meter', tier='atom', name='WorldMeterComponent', unity=['WorldHealthBar'], status='shipped',
  summary='The over-head team bar: ghost trail, barrier, elite and boss widths.',
  uses=['meter'], blocks=['vb-world-health'], doc='WorldHealthBar',
  csharp='layer.Bar(id, worldPos, hp01, barrier01, WorldTeam.Enemy, BarRank.Elite);',
  preview='WorldHealthBar', height=120)

e(id='combat-text', tier='atom', name='CombatTextComponent', unity=['CombatText'], status='shipped',
  summary='Floating numbers from confirmed simulation events, merged per target and capped.',
  uses=['text'], blocks=['vb-combat-text'], doc='CombatText',
  csharp='layer.Text(targetId, worldPos, 412, CombatKind.Damage);   // event-driven, pooled',
  preview='CombatText', height=120)

e(id='progress-input', tier='atom', name='ProgressInputComponent', unity=['VbSlider'], status='shipped',
  summary='A continuous value: cut track, teal fill, square thumb and the value as a number.',
  uses=['meter', 'text'], blocks=['vb-slider'], doc='VbSlider',
  csharp='new VbSlider("Music", settings.music, 0, 1).Into(audio).Changed(v => settings.music = v);',
  uxml='<vb:VbSlider label="Music" value="0.6" low-value="0" high-value="1" />',
  preview='VbSlider', height=130)

e(id='toggle-input', tier='atom', name='ToggleInputComponent', unity=['VbSwitch'], status='shipped',
  summary='A square on/off switch with a caps label and one line saying what it does.',
  uses=['frame', 'text'], blocks=['vb-switch'], doc='VbSwitch',
  csharp='new VbSwitch("REDUCED MOTION", "No camera shake · fades replace flicker", settings.reducedMotion)\n    .Into(feel).Changed(v => settings.reducedMotion = v);',
  rules=['The detail line says what changes, not "Enable X".', 'The whole row is the target (≥ 44 high).'],
  preview='VbSwitch', height=150)

e(id='choice-input', tier='atom', name='ChoiceInputComponent', unity=['VbSegmented'], status='shipped',
  summary='Two to five mutually exclusive peers in one frame, each with its consequence.',
  uses=['frame', 'text'], blocks=['vb-segmented'], doc='VbSegmented',
  csharp='new VbSegmented(new[] { ("LOW", "Stable 30 FPS"), ("STANDARD", "60 FPS target") }, settings.quality)\n    .Into(quality).Changed(v => settings.quality = v);',
  preview='VbSegmented', height=90)

e(id='popover', tier='atom', name='PopoverComponent', unity=['Popover'], status='shipped',
  summary='Hover, focus and tap content placed on the panel root, never clipped, clamped to the panel.',
  uses=['frame'], blocks=['vb-popover'], doc='Popover',
  csharp='Popover.Attach(chip, () => new PerkDetail().Bind(view, false), PopoverPlacement.Below, alignEnd: true);\nPopover.CloseAll();   // Navigator.Back does this first',
  rules=['Popover content is a slim solid frame.', 'Keep the kit in the theme (`VeilbreakGui.tss`): only theme sheets reach the panel root.'],
  spec='**Slim:** the popover body should use `vb-frame--slim vb-frame--solid` instead of today\'s chipped solid panel.',
  preview=r'''render(col([h('div', { className: 'vb-l vb-t-caption' }, 'Hover or tap a chip'), row([h(V.PerkChip, { view: PERKS.general }), h(V.PerkChip, { view: PERKS.rare })], 6)], 10, { alignItems: 'flex-start' }));''', height=300)

e(id='overlay', tier='atom', name='OverlayComponent', unity=['ModalHost', 'vb-modal', 'Navigator.Push'], status='shipped',
  summary='A modal layer: dims and disables what is under it, takes focus, and returns focus when it closes.',
  blocks=['vb-modal', 'vb-covered'], doc='ModalHost',
  csharp='var layer = nav.Push(onClose: Resume);   // vb-modal, bottom-docked\nDialog.Sheet(layer, "LEAVE GAME?", "Your profile is saved.");\nnav.Back();                              // popover → modal → page back → fallback',
  rules=['Back order is fixed: popover, then modal, then the page\'s back, then the host fallback.', 'An overlay is never the only way out: Back always closes it.'],
  preview=r'''render(board(h('div', { className: 'vb-screen vb-screen--pause' }, h('div', { className: 'vb-page' }, h('div', { className: 'vb-l vb-t-display', style: { marginTop: 120, textAlign: 'center' } }, 'PAUSED'))),
  h('div', { className: 'vb-modal', style: { position: 'absolute', left: 0, right: 0, top: 0, bottom: 0, paddingBottom: 12 } }, h(V.Dialog, { title: 'LEAVE GAME?', body: 'Your profile is saved.' }, h(V.VbButton, { title: 'EXIT', variant: 'danger', icon: 'back' }), h(V.VbButton, { title: 'STAY', icon: 'chevron' })))));''', stage='board')

e(id='backdrop', tier='atom', name='BackdropComponent', unity=['Backdrop'], status='shipped',
  summary='Full-bleed artwork behind a screen, scaled to cover, with a legibility shade along the bottom.',
  uses=['frame'], blocks=['vb-backdrop'], doc='Backdrop',
  csharp='new Backdrop(menuArt).Into(screenLayer);',
  preview='Backdrop', height=260)

e(id='scanlines', tier='atom', name='ScanLinesComponent', unity=['ScanLines'], status='shipped',
  summary='Projected-image lines for holo surfaces, one mesh. Hidden under vb-contrast.',
  blocks=['vb-scanlines'], doc='Holographic menu surfaces',
  csharp='new ScanLines().Into(holoBody);',
  rules=['Never behind numbers the player reads under pressure.'],
  preview='ScanLines', height=120)

e(id='tile', tier='atom', name='TileComponent', unity=['vb-tile', 'vb-slot'], status='partial',
  summary='A square-ish, pickable cell: a picture or glyph, a short name, a state. Rosters, slots, inventories.',
  uses=['frame', 'text', 'icon', 'badge'], blocks=['vb-tile', 'vb-slot'],
  csharp='// Today only as OperativeTile / SquadSlot (composites)\nvar tile = new OperativeTile().Into(grid).Bind(view, slot: 2, focused: true);',
  rules=['A tile is chipped: it is something you pick.', 'Selected = brackets + glow (the glow budget\'s "selected thing"). Locked tiles stay visible and say how to unlock.', 'Lay tiles out in a GridLayout, never free-floating.'],
  spec='**Proposed:** a generic `TileComponent` (`Picture`, `Glyph`, `Title`, `Caption`, `State: Idle|In|Focused|Locked`, `Picked`) that `OperativeTile`, `SquadSlot` and an inventory cell extend. The `vb-tile` block already carries `--in`, `--focused`, `--locked` and tier modifiers, so this is a C# extraction, not a restyle.',
  preview='OperativeTile', height=330)

e(id='card', tier='atom', name='CardComponent', unity=['vb-panel', 'OperativeCard', 'PerkCard'], status='proposed',
  summary='A slim container with an optional media band, a head, a body and a foot. The base every card composite extends.',
  uses=['frame', 'label', 'text'], blocks=[],
  csharp='// Proposed API\nvar card = new CardComponent { Title = "CHECKPOINT", Meta = "WAVE 07" }.Into(actions);\ncard.Body.Add(new Label("3 of 4 standing · 5 perks"));\ncard.Foot.Add(new VbButton("RESUME", ButtonVariant.Secondary));',
  rules=['Cards are slim. Only a selected card becomes chipped (brackets + edge).', 'Head: one LabelComponent and an optional meta. Foot: actions, right-aligned.'],
  spec='**Proposed.** There is no generic card today: each card composite (`OperativeCard`, `PerkCard`, the arcade\'s checkpoint and score panels) re-declares its frame. A `CardComponent` gives them one frame, one head, one padding rhythm (12 / 14) and one selected state:\n\n| Part | Class | Notes |\n| --- | --- | --- |\n| root | `vb-card` | slim frame; `--selected` adds edge + brackets |\n| media | `vb-card__media` | optional band, crop or art |\n| head | `vb-card__head`, `__title`, `__meta` | LabelComponent + meta |\n| body | `vb-card__body` | free content |\n| foot | `vb-card__foot` | actions, right-aligned |',
  preview=r'''function Card(p) {
  return h(V.VbFrame, { className: 'vb-panel vb-frame--slim' + (p.selected ? ' vb-card--selected' : ''), style: Object.assign({ width: 300, padding: '12px 14px', gap: 6 }, p.style) },
    h('div', { className: 'vb-row vb-row--between' }, h('div', { className: 'vb-l vb-t-label' }, p.title), p.meta ? h('div', { className: 'vb-l vb-t-badge' }, p.meta) : null),
    p.children,
    p.foot ? h('div', { className: 'vb-row', style: { justifyContent: 'flex-end', marginTop: 6, gap: 8 } }, p.foot) : null);
}
render(col([
  h(Card, { title: 'CHECKPOINT', meta: 'WAVE 07', foot: h(V.VbButton, { title: 'RESUME', icon: 'play', style: { width: 120 } }) }, h('div', { className: 'vb-l', style: { fontSize: 14 } }, 'Wave 07 next · 9,420'), h('div', { className: 'vb-l vb-t-caption' }, '3 of 4 standing · 5 perks · seed 80A2EB4E')),
  h(Card, { title: 'LAST RUN', meta: '#3 OF 20', selected: true }, row([h(V.StatReadout, { label: 'SCORE', value: '18,240' }), h(V.StatReadout, { label: 'WAVES', value: '12' }), h(V.StatReadout, { label: 'BOSSES', value: '2' })], 22))
], 12, { alignItems: 'flex-start' }));''', height=300)

# ════════════════════════════════════════════════════════════════════════════
# COMPOSITES — atoms combined into one reusable part with its own view model
# ════════════════════════════════════════════════════════════════════════════

e(id='button-with-icon', tier='composite', name='ButtonWithIconComposite', unity=['VbButton (icon)'], status='shipped',
  summary='A button with a trailing glyph: chevron for "go on", play for "resume", a domain glyph for destinations.',
  uses=['button', 'icon'], blocks=['vb-button__icon'],
  csharp='new VbButton("SETTINGS", ButtonVariant.Secondary, null, "settings", OpenSettings);\nnew VbButton("PLAY", ButtonVariant.Primary, "Choose a squad", "chevron", Squad);',
  rules=['`chevron` and `play` draw at 12, every other glyph at 20.', 'The glyph repeats the label\'s meaning; never icon-only (that is IconButtonComposite).'],
  preview=r'''var B = V.VbButton;
render(col([h(B, { title: 'PLAY', variant: 'primary', sub: 'Squad · Aria · Sol · Aegis · Nyra', icon: 'chevron', style: { width: 340 } }), row([h(B, { title: 'SETTINGS', icon: 'settings', style: { width: 166 } }), h(B, { title: 'RESUME', icon: 'play', style: { width: 166 } })]), h(B, { title: 'ABANDON RUN', variant: 'danger', icon: 'remove', style: { width: 220 } })], 10, { alignItems: 'flex-start' }));''', height=240)

e(id='icon-button', tier='composite', name='IconButtonComposite', unity=['vb-header__back', 'vb-hud__pause', 'vb-details__close'], status='partial',
  summary='A 44 × 44 chipped square holding one glyph, with a tooltip. Back, pause, close, speed.',
  uses=['frame', 'icon'], blocks=['vb-header__back', 'vb-hud__pause', 'vb-details__close'],
  csharp='// Today each owner builds its own square\nvar back = VbShape.Attach(Ui.Box(header, "vb-header__back")).Clickable(Back);\nnew VbIcon("back", 20).Into(back); back.tooltip = "Back";',
  rules=['Always 44 × 44 at least, always a tooltip and an accessible name.', 'Chipped (cut 6): it is a control.'],
  spec='**Proposed:** `IconButtonComposite(icon, tooltip, Action click)` with `--active` (toggled: AUTO on) and `--quiet` (translucent over the world, the HUD band look). The header back, HUD pause, details close and speed toggle collapse into it.',
  preview=r'''function sq(icon, cls, tip) { return h(V.VbShape, Object.assign({ className: cls, title: tip, style: { width: 44, height: 44, alignItems: 'center', justifyContent: 'center' } }), h(V.VbIcon, { icon: icon, size: 20 })); }
render(row([cell(sq('back', 'vb-header__back', 'Back'), 'back'), cell(sq('pause', 'vb-hud__pause', 'Pause'), 'pause · quiet'), cell(sq('close', 'vb-details__close', 'Close'), 'close')], 18));''', height=110)

e(id='field', tier='composite', name='FieldComposite', unity=['VbSlider', 'VbSwitch', 'VbSegmented'], status='partial',
  summary='One setting: a label, its input, and one line of what it does. The three inputs already include their label.',
  uses=['label', 'progress-input', 'toggle-input', 'choice-input', 'text'], blocks=['vb-section'],
  csharp='Slider(audio, "Music", settings.music, v => settings.music = v);   // arcade helper → VbSlider\nSwitch(feel, "HAPTICS", "Light pulse when an order is accepted", settings.haptics, v => settings.haptics = v);',
  rules=['Height rhythm inside a fieldset: slider 54, switch 62, segmented + 8 gap.', 'A field explains its consequence in its own detail line or in a caption right under it.'],
  spec='**Proposed:** `FieldComposite(label, input, help)` for inputs that do not carry their own label (segmented, custom pickers), so every row in a form has the same left label column and help line.',
  preview=r'''render(h(V.VbFrame, { className: 'vb-panel vb-section vb-frame--slim', style: { width: 358 } }, h('div', { className: 'vb-l vb-t-label vb-section__title' }, 'CAMERA'),
  h(V.VbSegmented, { className: 'vb-segmented--fill', items: [{ label: 'FULL', sub: 'Parallax' }, { label: 'REDUCED', sub: 'No drift' }, { label: 'OFF', sub: 'Fixed' }], selected: 0 }),
  h(V.VbSlider, { label: 'Sensitivity', value: 0.5 }),
  h(V.VbSwitch, { text: 'REDUCED MOTION', detail: 'No camera shake · fades replace flicker', value: false })));''', height=270)

e(id='fieldset', tier='composite', name='FieldsetComposite', unity=['Ui.Section', 'vb-section'], status='shipped',
  summary='A titled group of related fields in one slim container.',
  uses=['frame', 'label', 'field'], blocks=['vb-section'], doc='Layout classes (`Layout.uss`)',
  csharp='var audio = Ui.Section(scroll, "AUDIO");\nnew VbSlider("Master", s.master).Into(audio).Changed(v => s.master = v);',
  rules=['One topic per fieldset, 1–4 fields. A fieldset title is a LabelComponent, never a sentence.', 'Slim frame (this is the biggest visual change of the slim proposal).'],
  preview=r'''render(col([h(V.VbFrame, { className: 'vb-panel vb-section vb-frame--slim', style: { width: 358 } }, h('div', { className: 'vb-l vb-t-label vb-section__title' }, 'AUDIO'), h(V.VbSlider, { label: 'Master', value: 0.8 }), h(V.VbSlider, { label: 'Music', value: 0.55 }), h(V.VbSlider, { label: 'Effects', value: 0.8 }))], 8, { alignItems: 'flex-start' }));''', height=240)

e(id='form', tier='composite', name='FormComposite', unity=['ArcadeUI.Settings()'], status='proposed',
  summary='Fieldsets in a scroll, a saved-state meta in the header, and a footer of secondary actions.',
  uses=['fieldset', 'field', 'button', 'text'], blocks=['settings__footer', 'settings__note'],
  csharp='// Proposed API\nvar form = new FormComposite { SavedMeta = "SAVED" }.Into(page);\nvar audio = form.Fieldset("AUDIO");\naudio.Add(new VbSlider("Master", s.master));\nform.Footer(new VbButton("CLEAR LOCAL SCORES", ButtonVariant.Danger, click: ClearScores));\nform.Changed += SaveProfile;   // applies immediately; no Submit',
  rules=['Settings apply as they change. There is no Submit or OK button; the header meta says SAVED.', 'Destructive actions live in the footer, danger variant, and confirm with a dialog.'],
  spec='**Proposed.** `ArcadeUI.Settings()` builds this by hand today (scroll body, `Ui.Section` ×4, a note panel, a footer). A `FormComposite` owns the scroll, the fieldset rhythm, the footer and the change event, so SettingsView and any future options screen stay identical.',
  preview=r'''render(col([
  h(V.VbFrame, { className: 'vb-panel vb-section vb-frame--slim', style: { width: 358 } }, h('div', { className: 'vb-l vb-t-label vb-section__title' }, 'FEEL'), h(V.VbSwitch, { text: 'HAPTICS', detail: 'Light pulse when an order is accepted', value: true }), h(V.VbSwitch, { text: 'REDUCED MOTION', detail: 'No camera shake · fades replace flicker', value: false })),
  h(V.VbFrame, { className: 'vb-panel vb-section vb-frame--slim', style: { width: 358 } }, h('div', { className: 'vb-l vb-t-label vb-section__title' }, 'QUALITY'), h(V.VbSegmented, { items: [{ label: 'LOW', sub: 'Stable 30 FPS' }, { label: 'STANDARD', sub: '60 FPS target' }], selected: 1 })),
  h('div', { className: 'vb-row vb-row--between', style: { width: 358 } }, h(V.VbButton, { title: 'CLEAR LOCAL SCORES', variant: 'danger' }), h(V.VbButton, { title: 'ACCESSIBILITY', variant: 'ghost' }))
], 0, { alignItems: 'flex-start' }));''', height=380)

e(id='stat-readout', tier='composite', name='StatReadoutComposite', unity=['StatReadout'], status='shipped',
  summary='A labelled number with a unit, a previewed delta and an optional bar.',
  uses=['label', 'text', 'meter'], blocks=['vb-stat'], doc='StatReadout',
  csharp='new StatReadout("STANDING", "3", "/4").Into(numbers);\nhp.Set("1,240", null, .82f).Delta("+60");',
  preview='StatReadout', height=190)

e(id='status-badge', tier='composite', name='StatusBadgeComposite', unity=['StatusChip'], status='shipped',
  summary='A status with its own glyph and word, so no two statuses read alike and none relies on colour.',
  uses=['badge', 'icon', 'text'], blocks=['vb-status'], doc='StatusChip',
  csharp='new StatusChip(StatusKind.Silenced, null, "no skills", "1.4s").Into(chips);',
  preview='StatusChip', height=90)

e(id='credits-badge', tier='composite', name='CreditsBadgeComposite', unity=['CreditsChip'], status='shipped',
  summary='The currency glyph and an integer; formats on change.',
  uses=['badge', 'icon'], blocks=['vb-credits'], doc='Projection and the road economy',
  csharp='var credits = new CreditsChip().Into(power); credits.Set(run.Credits);',
  preview='CreditsChip', height=100)

e(id='health-bar', tier='composite', name='HealthBarComposite', unity=['HealthBar'], status='shipped',
  summary='Health over stability, with barrier, a ghost of recent damage and an optional caption.',
  uses=['meter', 'label'], blocks=['vb-health'], doc='HealthBar',
  csharp='new HealthBar().Into(cell).Set(a.hp, a.maxHp, a.barrier, a.stability / (float)a.maxStability);',
  preview='HealthBar', height=230)

e(id='power-strip', tier='composite', name='PowerStripComposite', unity=['PowerStrip'], status='shipped',
  summary='The shared Power pool: value, regen, the live consequence of an aim, the reserve line and the queue.',
  uses=['meter', 'label', 'text', 'queue-chip'], blocks=['vb-power'], doc='PowerStrip',
  csharp='strip.Set(hud.power, hud.maxPower, hud.regen, hud.hint, hud.previewFrom, hud.previewTo, marks);\nstrip.Reserve(25, 100); strip.Queue(hud.queue, q => run.CancelQueued(q.actor, q.slot));',
  preview='PowerStrip', height=330)

e(id='focus-meter', tier='composite', name='FocusMeterComposite', unity=['FocusMeter'], status='shipped',
  summary='Focus drains in real time while targeting slows combat and regenerates outside it.',
  uses=['meter', 'label', 'text'], blocks=['vb-focus'], doc='FocusMeter',
  csharp='focus.Set(run.Focus, slow: aiming, regenPerSecond: 12.5f, drainPerSecond: 25, slowScale: .2f);',
  preview='FocusMeter', height=110)

e(id='skill-button', tier='composite', name='SkillButtonComposite', unity=['SkillButton', 'SkillView', 'SkillState'], status='shipped',
  summary='The 44 × 56 skill control on an operative card. Every blocked state reads differently.',
  uses=['button', 'icon', 'meter', 'badge'], blocks=['vb-skill'], doc='SkillButton',
  csharp='var skill = card.Skill(0);                 // A slot; Skill(1) is S\nskill.AddManipulator(new TargetDrag(BeginAim, MoveAim, Release, CancelAim));',
  preview='SkillButton', height=250)

e(id='queue-chip', tier='composite', name='QueueChipComposite', unity=['QueueChip', 'QueueView'], status='shipped',
  summary='One entry of the Power queue: place, skill, cost, time, and × for manual entries.',
  uses=['badge', 'text', 'icon-button'], blocks=['vb-queue-chip'], doc='QueueChip',
  csharp='new QueueChip().Bind(view, position: 1, preview: false).Cancelled += Cancel;',
  preview='QueueChip', height=200)

e(id='queue-list', tier='composite', name='QueueListComposite', unity=['QueueList'], status='shipped',
  summary='Upcoming casts in firing order; popover content of the HUD band. Manual rows cancel on tap.',
  uses=['queue-chip', 'label'], blocks=['vb-queue'], doc='QueueList',
  csharp='list.Bind(hud.queue, hud.queueTitle, hud.queueHint, q => run.CancelQueued(q.actor, q.slot));',
  rules=['Rows are slim (they are list items, not controls); the × inside is the control.'],
  preview='QueueList', height=200)

e(id='screen-header', tier='composite', name='ScreenHeaderComposite', unity=['ScreenHeader'], status='shipped',
  summary='Back, title and a short meta read-out at the top of a page.',
  uses=['icon-button', 'text', 'label'], blocks=['vb-header'], doc='ScreenHeader',
  csharp='var header = new ScreenHeader("SETTINGS", "SAVED").Into(page);\nheader.Back += Leave; page.AddToClassList("vb-page--headed");',
  preview='ScreenHeader', height=150)

e(id='hud-band', tier='composite', name='HudBandComposite', unity=['HudTop', 'HudView'], status='shipped',
  summary='The translucent portrait HUD band: pause, autoplay, wave and score, speed, Power and focus.',
  uses=['icon-button', 'power-strip', 'focus-meter', 'credits-badge', 'queue-list', 'popover'], blocks=['vb-hud'], doc='HudTop',
  csharp='hudTop = new HudTop().Into(hud);\nhudTop.PauseClicked += Pause; hudTop.AutoClicked += run.ToggleAuto;\nhudTop.Bind(hudView);   // every frame',
  preview='HudTop', height=470)

e(id='boss-bar', tier='composite', name='BossBarComposite', unity=['BossBar', 'BossView'], status='shipped',
  summary='The boss read-out: name, health over stability, shield and interrupt badges.',
  uses=['health-bar', 'status-badge', 'icon', 'text'], blocks=['vb-boss'], doc='BossBar',
  csharp='bossBar.Bind(new BossView { name = "Signal Warden", health = .64f, stability = .35f, shielded = true });',
  preview='BossBar', height=170)

e(id='score-row', tier='composite', name='ScoreRowComposite', unity=['ScoreRow', 'ScoreView'], status='shipped',
  summary='One high-score line: rank, score, wave and bosses, the squad codes; the current run is framed.',
  uses=['frame', 'text', 'badge'], blocks=['vb-score'], doc='ScoreRow',
  csharp='new ScoreRow().Into(list).Bind(new ScoreView { rank = 1, score = 18240, wave = 12, bosses = 2, squad = codes });',
  rules=['Rows are slim; the current run is the one chipped row (edge + THIS RUN badge).'],
  preview='ScoreRow', height=230)

e(id='operative-card', tier='composite', name='OperativeCardComposite', unity=['OperativeCard', 'OperativeView'], status='shipped',
  summary='The portrait battle card: band, name, HP and barrier, statuses, A and S skills.',
  uses=['card', 'health-bar', 'skill-button', 'status-badge', 'badge'], blocks=['vb-operative'], doc='OperativeCard',
  csharp='var card = new OperativeCard().Classes("cards__card").Into(cards);\ncard.Bind(view);   // every frame; reuses children',
  preview='OperativeCard', height=210)

e(id='operative-tile', tier='composite', name='OperativeTileComposite', unity=['OperativeTile'], status='shipped',
  summary='A roster tile: in squad (numbered), focused, rarity tiers, locked with how to unlock.',
  uses=['tile', 'badge', 'icon'], blocks=['vb-tile'], doc='OperativeTile',
  csharp='var tile = new OperativeTile().Into(grid).Bind(view, slot, focused, RarityTier.Gold, locked: null);\ntile.Picked += () => Focus(id);',
  preview='OperativeTile', height=330)

e(id='squad-slot', tier='composite', name='SquadSlotComposite', unity=['SquadSlot'], status='shipped',
  summary='One of four squad slots: filled, or dashed and empty with SLOT n.',
  uses=['tile', 'text'], blocks=['vb-slot'], doc='SquadSlot',
  csharp='new SquadSlot().Into(slots).Bind(who != null ? Kit(who) : null, i + 1);',
  preview='SquadSlot', height=110)

e(id='operative-details', tier='composite', name='OperativeDetailsComposite', unity=['OperativeDetails'], status='shipped',
  summary='The operative sheet: bars scaled to the roster, passive, both skills, and add/remove.',
  uses=['card', 'health-bar', 'button', 'icon-button', 'text'], blocks=['vb-details'], doc='OperativeDetails',
  csharp='details.Bind(view, slot, squadFull, maxHp, maxArmor, maxStability);\ndetails.Toggled += () => ToggleSquad(view.id);',
  preview='OperativeDetails', height=370)

e(id='squad-result', tier='composite', name='SquadResultComposite', unity=['SquadResult', 'OperativeStats', 'ContributionView'], status='shipped',
  summary='The results cell; hover, focus or tap opens the operative\'s contribution.',
  uses=['tile', 'popover', 'stat-readout'], blocks=['vb-result', 'vb-contribution'], doc='OperativeStats and SquadResult',
  csharp='new SquadResult().Into(squad).Bind(contribution);',
  preview='SquadResult', height=420)

e(id='perk-card', tier='composite', name='PerkCardComposite', unity=['PerkCard', 'PerkView'], status='shipped',
  summary='One of three draft cards: art, tier tag, name, effect, stack pips and the value after the pick.',
  uses=['card', 'badge', 'text'], blocks=['vb-perk-card'], doc='Perks',
  csharp='var card = new PerkCard().Into(row).Bind(view, selected: i == pick);\ncard.Picked += () => Pick(i);',
  preview='PerkCard', height=260)

e(id='perk-detail', tier='composite', name='PerkDetailComposite', unity=['PerkDetail'], status='shipped',
  summary='The full explanation of a perk: draft (now → after pick) or owned (active, stacks, waves).',
  uses=['card', 'text', 'label'], blocks=['vb-perk-detail'], doc='Perks',
  csharp='new PerkDetail().Into(page).Bind(view, draft: true);',
  preview='PerkDetail', height=230)

e(id='perk-chip', tier='composite', name='PerkChipComposite', unity=['PerkChip'], status='shipped',
  summary='A compact owned perk; hover, focus or tap shows PerkDetail.',
  uses=['badge', 'popover', 'perk-detail'], blocks=['vb-perk-chip'], doc='Perks',
  csharp='new PerkChip(alignEnd: false).Into(chips).Bind(view);',
  preview='PerkChip', height=300)

e(id='perk-row', tier='composite', name='PerkRowComposite', unity=['PerkRow'], status='shipped',
  summary='An active-perk list row; selected is framed.',
  uses=['frame', 'icon', 'text'], blocks=['vb-perk-row'], doc='Perks',
  csharp='new PerkRow().Into(list).Bind(view, selected);',
  preview='PerkRow', height=230)

e(id='dialog', tier='composite', name='DialogComposite', unity=['Dialog.Sheet', 'Dialog.Push'], status='shipped',
  summary='A confirmation sheet: title, one line of body, then the actions. Usually on an overlay.',
  uses=['overlay', 'frame', 'text', 'button'], blocks=['vb-dialog'], doc='Dialog',
  csharp='var sheet = Dialog.Push(nav, "END THIS RUN?", "Your current run will be recorded and its checkpoint removed.");\nnew VbButton("ABANDON", ButtonVariant.Danger, null, "remove", Abandon).Into(sheet);\nnew VbButton("KEEP PLAYING", ButtonVariant.Secondary, null, "chevron", nav.Back).Into(sheet);',
  rules=['Titles ask the question ("END THIS RUN?"); the body states the consequence.', 'The destructive action is danger, the safe one secondary; Back = the safe one.'],
  preview='Dialog', height=300)

e(id='holo-frame', tier='composite', name='HoloFrameComposite', unity=['HoloFrame'], status='shipped',
  summary='A diegetic menu panel: cut frame with brackets, scanlines, a chromatic caps title and a flicker-in.',
  uses=['frame', 'scanlines', 'glitch-text'], blocks=['vb-holo'], doc='Holographic menu surfaces',
  csharp='var holo = new HoloFrame("LAST RUN").Into(menu); holo.FlickerIn();',
  rules=['Menus only, one in focus per screen. Never in battle.'],
  preview='HoloFrame', height=230)

e(id='holo-platform', tier='composite', name='HoloPlatformComposite', unity=['HoloPlatform'], status='shipped',
  summary='The 2D stand-in for a lineup plinth: occupied, selected with brackets, open slot.',
  uses=['marker', 'frame', 'text'], blocks=['vb-platform'], doc='Holographic menu surfaces',
  csharp='platform.Bind("Aria", "Lv 7", PlatformState.Selected, WorldTeam.Ally, showMarker: false);',
  preview='HoloPlatform', height=220)

e(id='reveal-card', tier='composite', name='RevealCardComposite', unity=['RevealCard'], status='shipped',
  summary='A rift result: a holographic back that flips to a tier-framed front with its tier word.',
  uses=['card', 'scanlines', 'badge'], blocks=['vb-reveal'], doc='Holographic menu surfaces',
  csharp='card.Bind(RarityTier.Gold, "Nyra", "Healer", item: false, isNew: true, dupe: null, flipped: false); card.Flip();',
  preview='RevealCard', height=260)

e(id='rule-row', tier='composite', name='RuleRowComposite', unity=['RuleRow', 'RuleView'], status='shipped',
  summary='One IF … THEN automation rule with a toggle and a note on when it can run.',
  uses=['frame', 'text', 'toggle-input'], blocks=['vb-rule'], doc='Holographic menu surfaces',
  csharp='new RuleRow().Into(list).Bind(rule).Toggled += SaveRules;',
  preview='RuleRow', height=300)

e(id='world-label', tier='composite', name='WorldLabelComposite', unity=['WorldLabel'], status='shipped',
  summary='Billboards in the world: aim chip, name tag, telegraph timer, move estimate, blocked.',
  uses=['text', 'badge'], blocks=['vb-world-label'], doc='WorldLabel',
  csharp='layer.Label(id, worldPos, LabelKind.Aim, "A · NEEDLE STORM", "18 POWER · queues as #3", WorldTeam.Ally);',
  preview='WorldLabel', height=130)

e(id='offscreen', tier='composite', name='OffscreenIndicatorComposite', unity=['OffscreenIndicator'], status='shipped',
  summary='An edge chip pointing at a fighter the camera cannot frame.',
  uses=['badge', 'icon', 'text'], blocks=['vb-offscreen'], doc='OffscreenIndicator',
  csharp='layer.Offscreen(id, worldPos, "BREACH", WorldTeam.Enemy, metres: 4, threat: true);',
  preview='OffscreenIndicator', height=110)

e(id='edge-warning', tier='composite', name='EdgeWarningComposite', unity=['EdgeWarning'], status='shipped',
  summary='A band of rippling chevrons and a caption along a board edge for a threat about to cross it.',
  uses=['icon', 'text'], blocks=['vb-edge-warning'], doc='EdgeWarning',
  csharp='warning.Bind("BREACH · 2 HOSTILES", top: false); warning.Set(urgency, Time.time, reducedMotion);',
  preview='EdgeWarning', height=120)

# ════════════════════════════════════════════════════════════════════════════
# LAYOUTS — how parts are arranged on the 390 × 844 board
# ════════════════════════════════════════════════════════════════════════════

e(id='root-layout', tier='layout', name='RootLayout', unity=['vb-root', 'SafeAreaRoot', 'VeilbreakPortrait.asset'], status='shipped',
  summary='The panel root: a full-size column inside the device safe area, carrying the accessibility root classes.',
  blocks=['vb-root'], doc='SafeAreaRoot',
  csharp='root.AddToClassList("vb-root");\nvar safe = new SafeAreaRoot(root, panelSettings, screenLayer);   // also primes fonts\nvoid Update() { safe.Refresh(); safe.Flags(s.largeText, s.reducedMotion, s.contrast, s.reducedFlashes); }',
  rules=['Reference 390 × 844, match width (`VeilbreakPortrait.asset`). Insets are panel units with 48 top / 34 bottom minimums.', 'Root classes: `vb-compact` (< 380 wide), `vb-large-text`, `vb-reduced-motion`, `vb-reduced-flashes`, `vb-contrast`. Popovers copy them.'],
  preview=r'''render(board(zone('SAFE TOP · ≥ 48', { position: 'absolute', left: 0, right: 0, top: -48, height: 48 }), zone('vb-root · 390 × 762 here', { position: 'absolute', left: 8, right: 8, top: 8, bottom: 8 }), zone('SAFE BOTTOM · ≥ 34', { position: 'absolute', left: 0, right: 0, bottom: -34, height: 34 })));''', stage='board')

e(id='screen-layout', tier='layout', name='ScreenLayout', unity=['Navigator', 'vb-screen', 'vb-page'], status='shipped',
  summary='One page at a time on the screen layer: optional header, a scroll body, a footer.',
  uses=['screen-header'], blocks=['vb-screen', 'vb-page'], doc='Navigator',
  csharp='var page = nav.Open("settings", back: Leave);   // vb-screen--settings > vb-page--settings\nvar header = new ScreenHeader("SETTINGS", "SAVED").Into(page); page.AddToClassList("vb-page--headed");\nvar body = Ui.Scroll(page, "vb-page__scroll").contentContainer;',
  rules=['Pages are 16 wide gutters; headed pages span the header and keep a 12 gutter.', 'Back order: popover → modal → page back → fallback. `nav.Blocked` freezes it.'],
  preview=r'''render(board(h('div', { className: 'vb-screen' }, h('div', { className: 'vb-page vb-page--headed' },
  h(V.ScreenHeader, { title: 'SETTINGS', meta: 'SAVED' }),
  zone('vb-page__scroll · flex-grow', { flexGrow: 1, marginTop: 8 }),
  zone('vb-page__footer', { height: 60, marginTop: 8 })))));''', stage='board')

e(id='stack-layout', tier='layout', name='StackLayout', unity=['(UI Toolkit default column)', 'vb-grow'], status='partial',
  summary='Children in a column with a fixed rhythm. The default flow; `Ui.Spacer` pushes the rest to the end.',
  blocks=['vb-grow'],
  csharp='var col = Ui.Box(page);          // a column by default\nUi.Spacer(page);                  // vb-grow: push the CTA to the bottom',
  rules=['Gaps come from the spacing tokens (4, 8, 12, 16, 24). UI Toolkit has no `gap`: give children `margin-bottom` via a stack class.'],
  spec='**Proposed:** `vb-stack`, `vb-stack--{4|8|12|16}` set a bottom margin on every child but the last, so stacks stop needing per-element margins.',
  preview=r'''render(col([zone('child', { width: 300, height: 44 }), zone('child', { width: 300, height: 44 }), zone('vb-grow', { width: 300, height: 70, borderStyle: 'dotted' }), h(V.VbButton, { title: 'TAKE PERK', variant: 'primary', icon: 'check', style: { width: 300 } })], 8, { alignItems: 'flex-start' }));''', height=300)

e(id='row-layout', tier='layout', name='RowLayout', unity=['vb-row', 'vb-row--between', 'vb-row--wrap'], status='shipped',
  summary='Children side by side; between pushes them apart, wrap lets chips flow.',
  blocks=['vb-row'], doc='Layout classes (`Layout.uss`)',
  csharp='var line = Ui.Box(score, "vb-row", "vb-row--between");\nUi.Text(line, "Wave clears", "vb-t-caption"); Ui.Text(line, "1,200", "vb-t-num");',
  preview=r'''render(col([h('div', { className: 'vb-row vb-row--between', style: { width: 340 } }, h('div', { className: 'vb-l vb-t-caption' }, 'Wave clears · 100 × wave'), h('div', { className: 'vb-l vb-t-num' }, '1,200')), h('div', { className: 'vb-row vb-row--wrap', style: { width: 340, gap: 6 } }, ['Sharpened Output ×2', 'Momentum Tax', 'Overclock', 'Barrier Mesh'].map(function (t, i) { return h(V.VbFrame, { key: i, className: 'vb-frame--slim vb-frame--raised', style: { flexDirection: 'row', alignItems: 'center', height: 22, padding: '0 8px' } }, h('div', { className: 'vb-l vb-t-badge' }, t.toUpperCase())); }))], 14, { alignItems: 'flex-start' }));''', height=130)

e(id='grid-layout', tier='layout', name='GridLayout', unity=['vb-grid4', 'vb-grid4--wrap', 'vb-grid2'], status='shipped',
  summary='Four across for slots, tiles and result cells; two across for chips and stats.',
  blocks=['vb-grid4', 'vb-grid2'], doc='Layout classes (`Layout.uss`)',
  csharp='var slots = Ui.Box(page, "vb-grid4");\nvar roster = Ui.Box(page, "vb-grid4", "vb-grid4--wrap");\nvar perks = Ui.Box(page, "vb-grid2");',
  rules=['Pick the column count the items fill: 4 operatives → grid4; never a half-empty row.', '`vb-compact` boards keep four across; tiles shrink, text drops a line.'],
  spec='**Proposed:** generalise to `vb-grid vb-grid--{2|3|4}` (3 is missing today: perk drafts use their own row).',
  preview=r'''render(col([h('div', { className: 'vb-grid4', style: { width: 358 } }, ['aria', 'sol', 'aegis', null].map(function (k, i) { return h(V.SquadSlot, { key: i, slot: i + 1, view: k ? op({ name: k.charAt(0).toUpperCase() + k.slice(1), code: k.slice(0, 2).toUpperCase(), kit: 'var(--kit-' + k + ')' }) : null, style: { width: '23%' } }); })), h('div', { className: 'vb-grid2', style: { width: 358 } }, [PERKS.general, PERKS.operative, PERKS.rare].map(function (p, i) { return h('div', { key: i, className: 'vb-grid2__cell' + (i % 2 ? ' vb-grid2__cell--end' : '') }, h(V.PerkChip, { view: p })); }))], 14, { alignItems: 'flex-start' }));''', height=260)

e(id='list-layout', tier='layout', name='ListLayout', unity=['Ui.Scroll', 'vb-page__scroll'], status='partial',
  summary='Rows of the same kind in a scroll: scores, perks, rules, queue entries. 6 between rows.',
  uses=['score-row', 'perk-row'], blocks=['vb-page__scroll'],
  csharp='var list = Ui.Scroll(page, "vb-page__scroll");\nforeach (var e in entries) new ScoreRow().Into(list).Bind(e);',
  rules=['Rows are slim; a selected row is the only chipped one.', 'An empty list says what will appear and how ("No runs recorded yet. The boulevard is waiting.").'],
  spec='**Proposed:** `vb-list` (column, 6 row gap via child margin) and `vb-list__empty` for the empty-state line, replacing the per-row `margin-bottom: 6px` repeated in `vb-score`, `vb-perk-row`, `vb-rule`.',
  preview=r'''render(col([1, 2, 3].map(function (r) { return h(V.ScoreRow, { key: r, view: { rank: r, score: [18240, 14120, 9420][r - 1], wave: [12, 10, 7][r - 1], bosses: [2, 2, 1][r - 1], squad: ['AR', 'SO', 'AE', 'NY'], current: r === 2 }, style: { width: 358 } }); }), 0, { alignItems: 'flex-start' }));''', height=240)

e(id='split-layout', tier='layout', name='SplitLayout', unity=['vb-half'], status='shipped',
  summary='Two equal cells in a row, 5 apart. Paired secondary actions.',
  blocks=['vb-half'], doc='Layout classes (`Layout.uss`)',
  csharp='var row = Ui.Box(actions, "vb-row");\nnew VbButton("HIGH SCORES").Classes("vb-half").Into(row);\nnew VbButton("SETTINGS", icon: "settings").Classes("vb-half").Into(row);',
  preview=r'''render(h('div', { className: 'vb-row', style: { width: 358 } }, h(V.VbButton, { title: 'HIGH SCORES', className: 'vb-half' }), h(V.VbButton, { title: 'SETTINGS', icon: 'settings', className: 'vb-half' })));''', height=80)

e(id='sheet-layout', tier='layout', name='SheetLayout', unity=['vb-modal', 'Dialog.Sheet'], status='shipped',
  summary='A bottom-docked sheet over the page: the thumb reaches every action.',
  uses=['overlay', 'dialog'], blocks=['vb-modal', 'vb-dialog'],
  csharp='var sheet = Dialog.Sheet(page, null, null, null, "pause__sheet");   // docked by Ui.Spacer above it',
  preview=r'''render(board(h('div', { className: 'vb-screen' }, h('div', { className: 'vb-page' }, zone('page content', { height: 220, marginTop: 40 }), h('div', { className: 'vb-grow' }), h(V.Dialog, { title: 'END THIS RUN?', body: 'Your current run will be recorded and its checkpoint removed.' }, h(V.VbButton, { title: 'ABANDON', variant: 'danger', icon: 'remove' }), h(V.VbButton, { title: 'KEEP PLAYING', icon: 'chevron' }))))));''', stage='board')

e(id='board-layout', tier='layout', name='BoardLayout', unity=['hud', 'battlefield', 'cards', 'WorldLayer'], status='partial',
  summary='The battle board: HUD band on top, the world in the middle, Power and the four operative cards at the bottom.',
  uses=['hud-band', 'operative-card', 'power-strip'], blocks=['hud', 'battlefield', 'cards'],
  csharp='// ArcadeUI.BuildHud / BuildCards\nhudTop = new HudTop().Into(hud);\nforeach (var a in run.Allies) new OperativeCard().Classes("cards__card").Into(cards);',
  rules=['At least half of the board stays world.', 'The camera frames fighters in the band the HUD leaves free; a new HUD panel must not cut into it.'],
  spec='**Proposed:** move the `hud` / `battlefield` / `cards` zones into the package as `vb-board`, `vb-board__top`, `__world`, `__bottom`, so any battle screen gets the same safe band.',
  preview=r'''render(board(h('div', { style: { position: 'absolute', inset: 0 } }, zone('TOP · HudBand · BossBar · announcer', { height: 150, margin: 8 }), zone('WORLD · WorldLayer · ≥ 50% of the board', { flexGrow: 1, margin: '0 8px', borderStyle: 'dotted' }), zone('BOTTOM · Power · 4 × OperativeCard', { height: 230, margin: 8 }))));''', stage='board')

e(id='world-layer', tier='layout', name='WorldLayerLayout', unity=['WorldLayer'], status='shipped',
  summary='The world-space HUD over the 3D view: bars, labels, markers and edge chips, advanced on simulation time.',
  uses=['world-meter', 'combat-text', 'world-label', 'marker', 'offscreen'], blocks=['vb-world'], doc='WorldLayer',
  csharp='layer.BeginFrame();\nforeach (var a in allies) layer.Bar(a.id, a.head, a.hp01, a.barrier01, WorldTeam.Ally);\nlayer.EndFrame(simDelta);',
  preview='BattleHUD', stage='board')

# ════════════════════════════════════════════════════════════════════════════
# VIEWS — whole screens and screen sections (today: ArcadeUI.cs in the game)
# ════════════════════════════════════════════════════════════════════════════

V_NOTE = 'Built inline by `ArcadeUI.{fn}()` in veilbreak-arcade today. **Proposed:** move it into the package as `{name}` with a plain view model and events, so the game only binds data and handles the events.'

e(id='main-menu', tier='view', name='MainMenuView', unity=['ArcadeUI.MainMenu()'], status='proposed',
  summary='The title screen: offline badge and best score, the mark, then the menu actions.',
  uses=['root-layout', 'screen-layout', 'mark', 'badge', 'menu-actions', 'button'], blocks=['menu__top', 'menu__best', 'menu__brand', 'menu__arcade', 'menu__exit'],
  csharp='// Proposed\nvar menu = new MainMenuView().Into(nav.Open("menu"));\nmenu.Bind(new MainMenuViewModel { best = best, checkpoint = cp, squad = names });\nmenu.Play += Squad; menu.Resume += app.Resume; menu.Scores += Scores; menu.Settings += Settings; menu.Exit += ExitConfirm;',
  spec=V_NOTE.format(fn='MainMenu', name='MainMenuView'),
  preview=r'''render(board(h('div', { className: 'vb-screen vb-screen--menu' }, h('div', { className: 'vb-page vb-page--menu' },
  h('div', { className: 'menu__top' }, h('div', { className: 'vb-l vb-t-badge' }, 'OFFLINE · LOCAL SCORES'), h(V.VbFrame, { className: 'vb-panel menu__best' }, h('div', { className: 'vb-l vb-t-badge' }, 'BEST'), h('div', { className: 'vb-l menu__best-score' }, '18,240'), h('div', { className: 'vb-l vb-t-badge vb-t-ally' }, 'WAVE 12'))),
  h('div', { className: 'menu__brand' }, h(V.VbMark, { width: 240 }), h('div', { className: 'vb-l menu__arcade' }, 'ARCADE')),
  h('div', { className: 'vb-grow' }), menuActions(),
  h(V.VbButton, { title: 'EXIT', variant: 'ghost', className: 'menu__exit' }))), MENU_BG));''', stage='board')

e(id='menu-actions', tier='view', name='MenuActionsSectionView', unity=['ArcadeUI.MainMenu() · menu__actions'], status='proposed',
  summary='The thumb-zone block of the menu: the checkpoint card when a run can resume, PLAY, then scores and settings.',
  uses=['card', 'button', 'button-with-icon', 'split-layout', 'text'], blocks=['menu__actions', 'menu__checkpoint', 'menu__play', 'menu__row'],
  csharp='// Proposed\nvar actions = new MenuActionsSectionView().Into(page);\nactions.Bind(checkpoint: cp, squadLine: "Squad · Aria · Sol · Aegis · Nyra", notice: app.Notice);\nactions.Play += Squad; actions.Resume += app.Resume;',
  rules=['PLAY is the screen\'s one primary. RESUME is secondary even though it continues a run: it lives inside the checkpoint card.', 'The checkpoint card only exists when there is a checkpoint; nothing else moves when it appears.'],
  spec=V_NOTE.format(fn='MainMenu', name='MenuActionsSectionView'),
  preview=r'''render(h('div', { className: 'vb-root', style: { width: 358 } }, menuActions()));''', height=280)

e(id='squad-view', tier='view', name='SquadView', unity=['ArcadeUI.Squad()'], status='proposed',
  summary='Pick four: the squad slots on top, the roster grid below, details on tap.',
  uses=['screen-layout', 'screen-header', 'grid-layout', 'squad-slot', 'operative-tile', 'operative-details', 'label'],
  csharp='// Proposed\nvar squad = new SquadView().Into(nav.Open("squad", MainMenu));\nsquad.Bind(selected, roster, focus);\nsquad.Picked += Focus; squad.Start += StartRun;',
  spec=V_NOTE.format(fn='Squad', name='SquadView'),
  preview=r'''var ids = ['aria', 'sol', 'aegis', 'nyra', 'rook', 'mira', 'hex', 'echo'], picked = ['aria', 'sol', 'aegis', 'nyra'];
function kit(k) { return op({ id: k, name: k.charAt(0).toUpperCase() + k.slice(1), code: k.slice(0, 2).toUpperCase(), kit: 'var(--kit-' + k + ')' }); }
render(board(h('div', { className: 'vb-screen vb-screen--squad' }, h('div', { className: 'vb-page vb-page--squad vb-page--headed' },
  h(V.ScreenHeader, { title: 'SQUAD', meta: '4 / 4', accent: true }),
  h('div', { className: 'vb-l vb-t-label vb-page__label' }, 'SQUAD · TAP A SLOT FOR DETAILS'),
  h('div', { className: 'vb-grid4' }, picked.map(function (k, i) { return h(V.SquadSlot, { key: k, slot: i + 1, view: kit(k), style: { width: '23%' } }); })),
  h('div', { className: 'vb-l vb-t-label vb-page__label' }, 'ROSTER · ALL 8 UNLOCKED'),
  h('div', { className: 'vb-grid4 vb-grid4--wrap' }, ids.map(function (k) { return h(V.OperativeTile, { key: k, view: kit(k), slot: picked.indexOf(k) + 1, focused: k === 'aria', style: { width: '23%' } }); })),
  h('div', { className: 'vb-grow' }),
  h(V.VbButton, { title: 'START RUN', variant: 'primary', sub: 'Aria · Sol · Aegis · Nyra · new seed', icon: 'chevron' })))));''', stage='board')

e(id='settings-view', tier='view', name='SettingsView', unity=['ArcadeUI.Settings()'], status='proposed',
  summary='Audio, feel, camera and quality as fieldsets in one form; applies as it changes.',
  uses=['screen-layout', 'screen-header', 'form', 'fieldset', 'field', 'button'], blocks=['settings__note', 'settings__footer', 'vb-section'],
  csharp='// Proposed\nvar view = new SettingsView().Into(nav.Open("settings", Leave));\nview.Bind(app.Profile.settings);\nview.Changed += s => { app.ApplySettings(); app.SaveProfile(); };\nview.ClearScores += ClearScoresConfirm; view.Accessibility += Accessibility;',
  rules=['Every container here is a fieldset: this screen shows the slim proposal most clearly. Toggle **Slim containers** above.'],
  spec=V_NOTE.format(fn='Settings', name='SettingsView'),
  preview=r'''render(board(h('div', { className: 'vb-screen vb-screen--settings' }, h('div', { className: 'vb-page vb-page--settings vb-page--headed' },
  h(V.ScreenHeader, { title: 'SETTINGS', meta: 'SAVED' }),
  h('div', { className: 'vb-page__scroll', style: { overflowY: 'auto', paddingTop: 12 } },
    section('AUDIO', [h(V.VbSlider, { key: 1, label: 'Master', value: 0.8 }), h(V.VbSlider, { key: 2, label: 'Music', value: 0.55 }), h(V.VbSlider, { key: 3, label: 'Effects', value: 0.8 })]),
    section('FEEL', [h(V.VbSwitch, { key: 1, text: 'HAPTICS', detail: 'Light pulse when an order is accepted', value: true }), h(V.VbSwitch, { key: 2, text: 'REDUCED MOTION', detail: 'No camera shake · fades replace flicker · flat panels', value: false })]),
    section('CAMERA', [h(V.VbSegmented, { key: 1, className: 'vb-segmented--fill', items: [{ label: 'FULL', sub: 'Parallax' }, { label: 'REDUCED', sub: 'No drift' }, { label: 'OFF', sub: 'Fixed' }], selected: 0 }), h(V.VbSlider, { key: 2, label: 'Sensitivity', value: 0.5 }), h('div', { key: 3, className: 'vb-l vb-t-caption', style: { whiteSpace: 'normal' } }, 'Pinch to zoom, twist to turn, drag empty ground to pan, double-tap to reset.')]),
    section('QUALITY', [h(V.VbSegmented, { key: 1, items: [{ label: 'LOW', sub: 'Stable 30 FPS' }, { label: 'STANDARD', sub: '60 FPS target' }], selected: 1 }), h('div', { key: 2, className: 'vb-l vb-t-caption', style: { whiteSpace: 'normal' } }, 'Applies on the next wave. Telegraphs stay fully visible on both tiers.')]),
    h(V.VbFrame, { className: 'vb-panel vb-panel--raised settings__note' }, h(V.VbIcon, { icon: 'automation', size: 16 }), h('div', { className: 'vb-l vb-t-caption', style: { whiteSpace: 'normal', marginLeft: 8 } }, 'Autoplay lives in the battle HUD and starts on for every new run.'))),
  h('div', { className: 'settings__footer vb-row vb-row--between' }, h(V.VbButton, { title: 'CLEAR LOCAL SCORES', variant: 'danger' }), h(V.VbButton, { title: 'ACCESSIBILITY', variant: 'ghost' }))))));''', stage='board')

e(id='pause-overlay', tier='view', name='PauseOverlayView', unity=['ArcadeUI.PauseScreen()'], status='proposed',
  summary='The run, frozen: a title, then a bottom sheet with wave, score, standing, active perks, resume and the exits.',
  uses=['overlay', 'sheet-layout', 'stat-readout', 'badge', 'button', 'button-with-icon', 'text'], blocks=['pause__title', 'pause__sheet', 'pause__stats', 'pause__perks', 'pause__chip', 'pause__button', 'pause__exit'],
  csharp='// Proposed\nvar pause = new PauseOverlayView().Into(nav.Open("pause", Resume));\npause.Bind(new PauseViewModel { wave = run.Wave, score = run.Score, standing = alive, perks = build, seed = run.Seed });\npause.Resume += Resume; pause.Settings += Settings; pause.Perks += ActivePerks;\npause.Abandon += AbandonConfirm; pause.Exit += ExitConfirm;',
  rules=['RESUME is the one primary. ABANDON is danger and confirms; EXIT is ghost, top-right, away from the thumb.', 'Back = Resume.'],
  spec=V_NOTE.format(fn='PauseScreen', name='PauseOverlayView'),
  preview=r'''render(board(h('div', { className: 'vb-screen vb-screen--pause' }, h('div', { className: 'vb-page vb-page--pause' },
  h('div', { className: 'vb-page__scroll', style: { flexGrow: 1 } },
    h('div', { className: 'pause__title' }, h('div', { className: 'vb-l vb-t-display' }, 'PAUSED'), h('div', { className: 'vb-l vb-t-caption' }, 'Combat, focus and world travel are frozen')),
    h('div', { className: 'vb-grow' }),
    h(V.Dialog, { className: 'pause__sheet' },
      h('div', { className: 'vb-row pause__stats' }, h(V.StatReadout, { label: 'WAVE', value: '07' }), h(V.StatReadout, { label: 'SCORE', value: '9,420' }), h(V.StatReadout, { label: 'STANDING', value: '3', unit: '/4' })),
      h(V.VbFrame, { className: 'vb-panel vb-panel--raised pause__perks' }, h('div', { className: 'vb-row vb-row--between' }, h('div', { className: 'vb-l vb-t-label' }, 'ACTIVE PERKS · 5'), h('div', { className: 'vb-row' }, h('div', { className: 'vb-l vb-t-badge vb-t-ally' }, 'DETAILS'), h(V.VbIcon, { icon: 'chevron', size: 16, style: { color: 'var(--ally-text)' } }))),
        h('div', { className: 'vb-row vb-row--wrap', style: { marginTop: 8 } }, [['burst', 'SHARPENED OUTPUT ×2'], ['projectile', 'MOMENTUM TAX'], ['add', 'BARRIER MESH']].map(function (c, i) { return h(V.VbFrame, { key: i, className: 'vb-panel pause__chip' }, h(V.VbIcon, { icon: c[0], size: 12 }), h('div', { className: 'vb-l vb-t-badge' }, c[1])); }))),
      h('div', { className: 'vb-l vb-t-caption' }, 'Seed 80A2EB4E · rules 3 · checkpoint after wave 06'),
      h(V.VbButton, { title: 'RESUME', variant: 'primary', sub: 'Returns to selected speed', icon: 'chevron' }),
      h(V.VbButton, { title: 'SETTINGS', icon: 'settings', className: 'pause__button' }),
      h(V.VbButton, { title: 'ABANDON RUN', variant: 'danger', icon: 'remove', className: 'pause__button' }))),
  h(V.VbButton, { title: 'EXIT', variant: 'ghost', icon: 'back', className: 'pause__exit' })))));''', stage='board')

e(id='confirm-view', tier='view', name='ConfirmDialogView', unity=['ArcadeUI.AbandonConfirm()', 'ExitConfirm()', 'ClearScoresConfirm()'], status='proposed',
  summary='A question, its consequence, the destructive action and the way back. One shape for every confirmation.',
  uses=['sheet-layout', 'dialog', 'button'],
  csharp='// Proposed\nConfirmDialogView.Push(nav, "END THIS RUN?", "Your current run will be recorded and its checkpoint removed.",\n    confirm: ("ABANDON", "remove", Abandon), cancel: ("KEEP PLAYING", "chevron"));',
  spec=V_NOTE.format(fn='AbandonConfirm', name='ConfirmDialogView'),
  preview=r'''render(board(h('div', { className: 'vb-screen vb-screen--confirm' }, h('div', { className: 'vb-page' }, h('div', { className: 'vb-grow' }),
  h(V.Dialog, { title: 'END THIS RUN?', body: 'Your current run will be recorded and its checkpoint removed.' }, h(V.VbButton, { title: 'ABANDON', variant: 'danger', sub: 'Record the score and end the run', icon: 'remove' }), h(V.VbButton, { title: 'KEEP PLAYING', icon: 'chevron' }))))));''', stage='board')

e(id='perk-draft', tier='view', name='PerkDraftView', unity=['ArcadeUI.PerkScreen()'], status='proposed',
  summary='Wave cleared: three perk cards, the chosen one explained, the squad that carries forward, TAKE PERK.',
  uses=['screen-layout', 'perk-card', 'perk-detail', 'grid-layout', 'health-bar', 'button'], blocks=['perks__cards', 'perks__detail', 'perks__squad', 'perks__cell', 'perks__code'],
  csharp='// Proposed\nvar draft = new PerkDraftView().Into(nav.Open("perks"));\ndraft.Bind(wave, choices, pick, squad);\ndraft.Picked += i => pick = i; draft.Take += TakePerk;',
  spec=V_NOTE.format(fn='PerkScreen', name='PerkDraftView'),
  preview=r'''var cards = [PERKS.general, PERKS.operative, PERKS.rare];
render(board(h('div', { className: 'vb-screen vb-screen--perks' }, h('div', { className: 'vb-page vb-page--perks' },
  h('div', { className: 'vb-page__scroll', style: { flexGrow: 1 } },
    h('div', { className: 'vb-l vb-t-label vb-t-ally' }, 'WAVE 07 CLEARED'), h('div', { className: 'vb-l vb-t-display' }, 'CHOOSE A PERK'), h('div', { className: 'vb-l vb-t-caption vb-page__caption' }, '+700 wave bonus · score 9,420 · perks last the whole run'),
    h('div', { className: 'perks__cards' }, cards.map(function (p, i) { return h(V.PerkCard, { key: i, view: p, selected: i === 0 }); })),
    h(V.PerkDetail, { className: 'perks__detail', view: PERKS.general, draft: true }),
    h(V.VbFrame, { className: 'vb-panel perks__squad' }, h('div', { className: 'vb-l vb-t-badge' }, 'CARRIES FORWARD · NO HEALING BETWEEN WAVES'), h('div', { className: 'vb-grid4' }, [['AR', 82], ['SO', 64], ['AE', 91], ['NY', 0]].map(function (a) { return h('div', { key: a[0], className: 'perks__cell' }, h('div', { className: 'vb-row vb-row--between' }, h('div', { className: 'vb-l perks__code' }, a[0]), h('div', { className: 'vb-l vb-t-badge' }, a[1] ? a[1] + '%' : 'OUT')), h(V.HealthBar, { hp: a[1] * 10, max: 1000, stability: 0.7 })); })))),
  h(V.VbButton, { title: 'TAKE PERK', variant: 'primary', sub: 'Sharpened Output → +24% damage · then wave 08, focus refills', icon: 'check' })))));''', stage='board')

e(id='results-view', tier='view', name='ResultsView', unity=['ArcadeUI.Results()'], status='proposed',
  summary='Run over: the score and how it was made, the squad\'s contribution, the perks, and what next.',
  uses=['screen-layout', 'card', 'stat-readout', 'grid-layout', 'squad-result', 'perk-chip', 'button'], blocks=['results__score', 'results__value', 'results__rank', 'results__points', 'results__stats', 'results__row'],
  csharp='// Proposed\nvar results = new ResultsView().Into(nav.Open("results"));\nresults.Bind(new ResultsViewModel { score = run.Score, rank = rank, ledger = run.Ledger, squad = contributions, perks = build });\nresults.Again += Squad; results.Menu += MainMenu;',
  spec=V_NOTE.format(fn='Results', name='ResultsView'),
  preview=r'''function line(a, b) { return h('div', { className: 'vb-row vb-row--between' }, h('div', { className: 'vb-l vb-t-caption' }, a), h('div', { className: 'vb-l results__points' }, b)); }
render(board(h('div', { className: 'vb-screen vb-screen--results' }, h('div', { className: 'vb-page vb-page--results' },
  h('div', { className: 'vb-page__scroll', style: { flexGrow: 1 } },
    h('div', { className: 'vb-l vb-t-label vb-t-enemy' }, 'SQUAD DOWN · WAVE 12'), h('div', { className: 'vb-l vb-t-display' }, 'RUN OVER'),
    h(V.VbFrame, { className: 'vb-panel vb-panel--solid results__score' },
      h('div', { className: 'vb-row vb-row--between' }, h('div', null, h('div', { className: 'vb-l vb-t-label' }, 'SCORE'), h('div', { className: 'vb-l results__value' }, '18,240')), h('div', { className: 'vb-l results__rank' }, '#1 OF 20')),
      line('Creep kills', '9,640'), line('Wave clears · 100 × wave', '7,800'), line('Bosses · 500 × wave', '800'),
      h('div', { className: 'vb-row results__stats' }, h(V.StatReadout, { label: 'WAVES', value: '12' }), h(V.StatReadout, { label: 'BOSSES', value: '2' }), h(V.StatReadout, { label: 'DISTANCE', value: '1.4', unit: 'km' }), h(V.StatReadout, { label: 'SIM TIME', value: '9:12' }))),
    h('div', { className: 'vb-l vb-t-label vb-page__label' }, 'SQUAD · HOVER OR TAP FOR CONTRIBUTION'),
    h('div', { className: 'vb-grid4' }, ['aria', 'sol', 'aegis', 'nyra'].map(function (k, i) { return h(V.SquadResult, { key: k, view: { operative: op({ name: k.charAt(0).toUpperCase() + k.slice(1), code: k.slice(0, 2).toUpperCase(), kit: 'var(--kit-' + k + ')' }), fellWave: i === 3 ? 11 : 0, damage: 40000 - i * 7000, kills: 120 - i * 20, casts: 40, power: 900, share: [0.34, 0.27, 0.21, 0.18][i] }, style: { width: '23%' } }); }))),
  h('div', { className: 'vb-row results__row' }, h(V.VbButton, { title: 'MENU', className: 'vb-half' }), h('div', { style: { width: 10 } }), h(V.VbButton, { title: 'RUN AGAIN', variant: 'primary', icon: 'chevron', className: 'vb-half', style: { flexGrow: 1 } }))))));''', stage='board')

e(id='scores-view', tier='view', name='HighScoresView', unity=['ArcadeUI.Scores()'], status='proposed',
  summary='Top 20 on this device, the current run framed, and older-rules runs below.',
  uses=['screen-layout', 'screen-header', 'list-layout', 'score-row', 'text'],
  csharp='// Proposed\nvar scores = new HighScoresView().Into(nav.Open("scores", MainMenu));\nscores.Bind(ranked.Take(20), historical, currentRunId);',
  spec=V_NOTE.format(fn='Scores', name='HighScoresView'),
  preview=r'''render(board(h('div', { className: 'vb-screen vb-screen--scores' }, h('div', { className: 'vb-page vb-page--scores vb-page--headed' },
  h(V.ScreenHeader, { title: 'HIGH SCORES', meta: 'TOP 20' }),
  h('div', { className: 'vb-l vb-t-caption vb-page__caption' }, 'On this device · rules 3. Ties break by wave, then shortest sim time.'),
  h('div', { className: 'vb-page__scroll', style: { overflowY: 'auto' } }, [18240, 14120, 9420, 8800, 6120, 4410, 2950].map(function (s, i) { return h(V.ScoreRow, { key: i, view: { rank: i + 1, score: s, wave: 12 - i, bosses: i < 3 ? 2 : 1, squad: ['AR', 'SO', 'AE', 'NY'], current: i === 2 } }); })),
  h('div', { className: 'vb-l vb-t-caption vb-page__footer' }, 'Local scores only · not an online ranking')))));''', stage='board')

e(id='route-view', tier='view', name='RouteChoiceView', unity=['RouteChoice', 'ArcadeGui.RouteScreen()'], status='partial',
  summary='The crossroads between waves: two road signs, one choice. Already a package composite; the screen wrapper is in the game.',
  uses=['glitch-text', 'card', 'icon', 'text'], blocks=['vb-route'], doc='Projection and the road economy',
  csharp='var route = new RouteChoice().Into(nav.Open("route"));\nroute.Bind(new RouteView { wave = 3, left = combat, right = merchant });\nroute.Chosen += side => run.ChooseRoad(side);',
  preview='RouteChoice', stage='board')

e(id='merchant-view', tier='view', name='MerchantView', unity=['MerchantScreen', 'ArcadeGui.MarketScreen()'], status='partial',
  summary='The night market: rare finds, sell stacks, trade health. Already a package composite; the screen wrapper is in the game.',
  uses=['perk-card', 'button', 'credits-badge', 'health-bar', 'list-layout'], blocks=['vb-merchant'], doc='Projection and the road economy',
  csharp='var market = new MerchantScreen().Into(nav.Open("merchant"));\nmarket.Bind(view); market.Bought += Buy; market.Left += Leave;',
  preview='MerchantScreen', stage='board')

e(id='battle-hud-view', tier='view', name='BattleHudView', unity=['ArcadeUI.BuildHud()', 'BuildCards()'], status='proposed',
  summary='The whole battle board: HUD band, boss bar, announcer, world layer, Power and four operative cards.',
  uses=['board-layout', 'hud-band', 'boss-bar', 'world-layer', 'power-strip', 'operative-card', 'glitch-text'],
  csharp='// Proposed\nvar battle = new BattleHudView().Into(root);\nbattle.Bind(hudView, operatives, boss);   // every frame\nbattle.Pause += Pause; battle.QueueCancel += q => run.CancelQueued(q.actor, q.slot);\nbattle.Card(i).Skill(0).AddManipulator(aimDrag);',
  spec=V_NOTE.format(fn='BuildHud', name='BattleHudView'),
  preview='BattleHUD', stage='board')

e(id='guide-view', tier='view', name='GuideDialogView', unity=['ArcadeUI.ShowGuide()'], status='proposed',
  summary='The first-run guide: how to move, aim, cancel and when to use autoplay, then READY.',
  uses=['overlay', 'dialog', 'button'],
  csharp='// Proposed\nGuideDialogView.Push(nav, "RUN THE BOULEVARD", steps, ready: () => { settings.tutorialSeen = true; Resume(); });',
  spec=V_NOTE.format(fn='ShowGuide', name='GuideDialogView'),
  preview=r'''render(board(h('div', { className: 'vb-screen vb-screen--pause' }, h('div', { className: 'vb-page' }, h('div', { className: 'vb-grow' }),
  h(V.Dialog, { title: 'RUN THE BOULEVARD', body: 'Drag an operative to move. Drag A or S onto the battlefield to aim; release over the cards to cancel. Auto controls tactics. 2× speeds combat; aiming stays slow while focus lasts. Stop enemies before they pass the rear line.', bodyClass: 'vb-t-body' }, h(V.VbButton, { title: 'READY', variant: 'primary', icon: 'check' }))))));''', stage='board')

TIERS = [
    ('atom', 'Atoms', 'Base components. The smallest parts with a look and a contract; atoms may use other atoms.'),
    ('composite', 'Composites', 'Atoms combined into one reusable part with its own view model and events.'),
    ('layout', 'Layouts', 'How parts are arranged on the 390 × 844 board: roots, pages, stacks, rows, grids, lists, sheets.'),
    ('view', 'Views', 'Whole screens and named screen sections. Today most are built inline in the game; the specs here are where they move.'),
]
