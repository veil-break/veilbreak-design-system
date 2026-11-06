"""The Veilbreak GUI taxonomy: four tiers plus assets, mapped onto com.veilbreak.gui.

Each entry names a design-system component (ButtonComponent, FormComposite, GridLayout, PauseOverlayView …), the Unity
type or USS class that implements it, its status, what it is built from, and how to use it in Unity. Since 0.3.0 every
entry is shipped: the slim frame, the generic Text/Label/Badge/Tile/Card, the form and layout parts and the views all
live in the package, and the web kit (Tools~/web/kit.js, Web~) ports each of them.

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
  spec      longer notes (markdown); the Spec tab
  preview   key into Tools~/web/previews.py P, or JS (calls render(node)); board helpers in helpers.js
  stage     'free' (default) | 'board' (390 x 844 phone) | 'wide'
  height    preview height hint (px, free stage)
"""

SLIM_USS = """/* Gui.uss — FrameComponent kinds. VbShape reads both from the same properties: no C# change. */
/* Slim: the default container. Small chamfer (cut-xs), no edge, fill only. */
.vb-frame--slim { --vb-cut: 4; --vb-bw: 0; --vb-fill: var(--vb-color-panel); }
.vb-frame--slim.vb-frame--raised { --vb-fill: var(--vb-color-raised); }
.vb-frame--slim.vb-frame--solid { --vb-fill: var(--vb-color-panel-solid); }
.vb-frame--slim.vb-frame--ally { --vb-fill: var(--vb-color-ally-soft); }
.vb-frame--slim.vb-frame--enemy { --vb-fill: var(--vb-color-enemy-soft); }
.vb-frame--slim.vb-frame--power { --vb-fill: var(--vb-color-power-soft); }
/* Chipped: the cut + hairline look, kept for what you act on or what is selected. */
.vb-frame--chipped { --vb-cut: 10; --vb-bw: 1; --vb-edge: var(--vb-color-line); }
/* High contrast: a container with no edge must still separate from the world at 3:1. */
.vb-contrast .vb-frame--slim { --vb-bw: 1; --vb-edge: var(--vb-color-line-strong); }"""

SLIM_MIGRATION = """/* Layout.uss — the package's containers on the slim kind (same selectors, no markup change). */
.vb-section, .vb-dialog, .vb-details, .vb-score, .vb-perk-row, .vb-rule, .vb-result, .vb-queue__row, .vb-merchant__exchange { --vb-cut: 4; --vb-bw: 0; }
/* States that are selections or warnings keep an edge. */
.vb-score--current, .vb-perk-row--selected, .vb-rule--warning, .vb-rule--draft, .vb-queue__row--next, .vb-queue__row--manual { --vb-bw: 1; }
.vb-contrast .vb-section, .vb-contrast .vb-dialog, .vb-contrast .vb-details, .vb-contrast .vb-score, .vb-contrast .vb-perk-row,
.vb-contrast .vb-rule, .vb-contrast .vb-result, .vb-contrast .vb-queue__row, .vb-contrast .vb-merchant__exchange { --vb-bw: 1; --vb-edge: var(--vb-color-line-strong); }"""

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

e(
    id='text',
    tier='atom',
    name='TextComponent',
    unity=['TextComponent', 'TypeStyle', 'TextTone', 'Ui.Text'],
    status='shipped',
    summary='Every run of text: a Label with one type style and an optional tone. The style is the whole contract.',
    blocks=[
        'vb-t-display',
        'vb-t-title',
        'vb-t-label',
        'vb-t-label-sm',
        'vb-t-label-lg',
        'vb-t-badge',
        'vb-t-body',
        'vb-t-body-sm',
        'vb-t-caption',
        'vb-t-heading',
        'vb-t-heading-sm',
        'vb-t-heading-lg',
        'vb-t-num',
        'vb-t-ally',
        'vb-t-enemy',
        'vb-t-power',
        'vb-t-muted',
    ],
    csharp='Ui.Text(parent, "PAUSED", TypeStyle.Display);                       // caps are passed, not styled\nUi.Text(parent, "Combat, focus and world travel are frozen", TypeStyle.Caption, TextTone.Muted);\nvar score = new TextComponent(Ui.Number(18240), TypeStyle.Num).Into(row); // Share Tech Mono, tabular',
    uxml='<vb:TextComponent type-style="Display" text="PAUSED" />\n<vb:TextComponent type-style="Caption" tone="Muted" text="Combat, focus and world travel are frozen" />',
    rules=[
        'One type style per run: `Display`, `Title`, `Label`, `Badge`, `Body`, `Caption`, `Num` and the three variants (`vb-t-*`): body (`Body`, `BodySm` = caption), label (`LabelSm` = `Badge`, `Label`, `LabelLg`) and heading (`HeadingSm`, `Heading`, `HeadingLg`). Tones (`Ally`, `Enemy`, `Power`, `Muted`) only recolour.',
        'Every size, gutter and margin follows the panel width through the fluid scale (0.92× at 320, 1× at 390, 1.06× at 430, 1.18× from 600): `SafeAreaRoot` puts `vb-fluid-<width>` on the board, and the web kit does the same on every `.vb-root`.',
        'UI Toolkit has no `text-transform`. Uppercase in code (`ToUpperInvariant()`) and pass normal case in data, or use LabelComponent.',
        'Numbers that tick use `Num` and `Ui.Number`/`Ui.Seconds`/`Ui.Count`, which cache strings so per-frame binding makes no garbage.',
        'Text over the 3D world gets the soft drop shadow (header titles, HUD numbers already have it).',
    ],
    preview='TextComponent',
)

e(
    id='label',
    tier='atom',
    name='LabelComponent',
    unity=['LabelComponent', 'LabelSize', 'Ui.Eyebrow'],
    status='shipped',
    summary='The caps eyebrow over a section, field or value: tracked-out, small, secondary text with 4 under it.',
    uses=['text'],
    blocks=['vb-label', 'vb-t-label', 'vb-t-badge'],
    csharp='Ui.Eyebrow(section, "Audio");                                   // → AUDIO, vb-t-label vb-label\nnew LabelComponent("Roster · all 8 unlocked").Classes("vb-page__label").Into(page);\nnew LabelComponent("Wave 07 cleared", LabelSize.Label, TextTone.Ally).Into(page);',
    uxml='<vb:LabelComponent text="Carries forward" size="Badge" />',
    rules=[
        'A label names a thing; it is never a sentence. It uppercases what it is given.',
        '`Label` size over sections and pages, `Badge` size inside chips, cards and dense rows.',
    ],
    preview='LabelComponent',
)

e(
    id='glitch-text',
    tier='atom',
    name='GlitchTextComponent',
    unity=['GlitchText', 'Glitch.In/Out/Pulse/Slam'],
    status='shipped',
    summary='A Label that projects in and out with clipped red/cyan bands. Headlines and announcements only.',
    uses=['text'],
    blocks=['vb-glitch'],
    doc='Projection and the road economy',
    csharp='var title = new GlitchText { text = "CHOOSE YOUR ROAD" }.Classes("vb-t-display").Into(page);\nGlitch.In(title);            // falls back to a fade under vb-reduced-motion / vb-reduced-flashes',
    rules=[
        'One projected headline per screen. Body copy never glitches.',
        'Schedules cancel on replacement and detach; you never have to clean up.',
        'Titles that carry `vb-reveal-title` use `Glitch.Reveal` instead: a 2 s decode out of noise glyphs (see the Title reveal entry).',
    ],
    preview='GlitchText',
    height=140,
)

e(
    id='icon',
    tier='atom',
    name='IconComponent',
    unity=['VbIcon'],
    status='shipped',
    summary='A 24-grid stroke glyph by name, tinted by colour. 99 glyphs plus 8 design-system aliases.',
    blocks=['vb-icon'],
    doc='VbIcon',
    csharp='new VbIcon("cooldown", 16).Into(row);\nnew VbIcon("chevron", 16).Tint(VbTokens.AllyText).Into(more);',
    uxml='<vb:VbIcon icon="settings" style="width: 20px; height: 20px;" />',
    rules=[
        'Sizes: 12 in badges, 14 in status chips, 16 in rows, 20 in buttons, 36 on road signs.',
        'An icon never carries meaning alone: pair it with a word or a tooltip.',
        'Names are kebab-case in USS (`vb-icon--skill-aria-needle-storm`) and any case in C#.',
    ],
    preview='VbIcon',
    height=420,
)

e(
    id='mark',
    tier='atom',
    name='MarkComponent',
    unity=['VbMark', 'MarkGeometry'],
    status='shipped',
    summary='The slash-cut VEIL/BREAK mark at a width; its height follows the 513 × 259 artwork.',
    blocks=['vb-mark'],
    doc='VbMark',
    csharp='new VbMark(240).Into(brand);   // MarkGeometry.Parts drives the splash too',
    rules=['The mark appears once per screen at most, on the main menu and the splash.'],
    preview='VbMark',
    height=220,
)

e(
    id='frame',
    tier='atom',
    name='FrameComponent',
    unity=['VbShape', 'VbFrame', 'FrameKind', 'Ui.Frame', 'vb-panel'],
    status='shipped',
    summary='The surface everything sits on. Two kinds: slim (the default container) and chipped (things you act on or that are selected).',
    blocks=['vb-shape', 'vb-panel', 'vb-frame'],
    doc='VbShape',
    csharp='var box = Ui.Frame(parent, FrameKind.Slim);                       // vb-panel vb-frame--slim: sections, sheets, rows, cards\nvar tint = Ui.Frame(parent, FrameKind.Slim, "vb-frame--ally");     // ally-soft fill\nvar link = Ui.Frame(parent, FrameKind.Chipped);                    // cut + hairline: what you act on',
    uxml='<vb:VbFrame class="vb-panel" kind="Slim" />\n<vb:VbFrame class="vb-panel vb-frame--raised" kind="Slim" />\n<vb:VbFrame class="vb-panel" kind="Chipped" />',
    rules=[
        '**Slim** (cut-xs 4, no edge, fill only) for every container: sections, dialog sheets, list rows, cards, badges.',
        '**Chipped** (cut 6–14, hairline edge) only where the frame means something: buttons, tiles you pick, the selected thing, holo frames, focus.',
        'Never nest chipped in chipped. A chipped control inside a slim container is the normal case.',
        'Slim frames get their edge back under `vb-contrast` so containers still separate at 3:1. Selected and warning states keep their edge.',
        'Fills come from tokens: `panel`, `panel-solid`, `raised`, and the `*-soft` tints behind their own text colour.',
    ],
    spec='Slim is the default container since 0.3.0. `VbShape` reads `--vb-cut` and `--vb-bw`, so a kind is **pure USS**; `FrameKind` and `Ui.Frame` only pick the class:\n\n```css\n'
    + SLIM_USS
    + "\n```\n\nThe package's own containers moved to the slim kind with no markup change (`Layout.uss`):\n\n```css\n"
    + SLIM_MIGRATION
    + '\n```',
    preview=r"""function box(kind, cls, t, sub) { return h(V.VbFrame, { kind: kind, className: 'vb-panel ' + cls, style: { width: 168, height: 92, padding: 12 } }, h('div', { className: 'vb-l vb-t-label' }, t), h('div', { className: 'vb-l vb-t-caption' }, sub)); }
render(col([
  h('div', { className: 'vb-l vb-t-badge' }, 'SLIM · containers'),
  row([box('slim', '', 'SLIM', 'panel · default'), box('slim', 'vb-frame--raised', 'RAISED', 'raised fill'), box('slim', 'vb-frame--solid', 'SOLID', 'sheets · popovers')], 10),
  row([box('slim', 'vb-frame--ally', 'ALLY', 'ally-soft'), box('slim', 'vb-frame--power', 'POWER', 'power-soft'), box('slim', 'vb-frame--enemy', 'ENEMY', 'enemy-soft')], 10),
  h('div', { className: 'vb-l vb-t-badge', style: { marginTop: 8 } }, 'CHIPPED · interactive / selected'),
  row([box('chipped', '', 'CHIPPED', 'kind="Chipped"'), box('', 'vb-panel--solid', 'SOLID', 'vb-panel--solid'), box('', 'vb-panel--raised vb-panel--brackets', 'SELECTED', 'brackets')], 10)
], 10, { alignItems: 'flex-start' }));""",
    height=420,
)

e(
    id='button',
    tier='atom',
    name='ButtonComponent',
    unity=['VbButton', 'ButtonVariant'],
    status='shipped',
    summary='A chipped button: primary (one per screen), secondary, light, danger, ghost. Blocked buttons say why; icon-only, leading-glyph and dimmed forms; every click runs the edge sweep.',
    uses=['frame', 'text', 'icon'],
    blocks=['vb-button'],
    doc='VbButton',
    csharp='new VbButton("RESUME", ButtonVariant.Primary, "Returns to selected speed", "chevron", Resume) { name = "resume" }.Into(sheet);\nplay.Blocked = "Pick 1 more to start";   // disables, dashes, shows the reason\nnew VbButton("Settings", ButtonVariant.Secondary, null, "cog", Settings) { IconOnly = true };   // 48-unit square, the title is the tooltip\nnew VbButton("EXIT TO MENU", ButtonVariant.Secondary, null, "back", Exit);   // back and chevron-left lead by themselves (Lead)\nbuy.Dimmed = true;   // out of play with no reason shown',
    uxml='<vb:VbButton title="PLAY" variant="Primary" sub="Squad · 4" icon="chevron" />',
    rules=[
        'One primary per screen, bottom of the thumb zone.',
        'Blocked is never just greyed: the sub line becomes the reason, in under five words.',
        'Buttons stay chipped when containers go slim: the chamfer is how a thumb finds them.',
        'A forward action ends with its chevron; an action that goes back or leaves leads with its glyph or carries none. Settings is the `cog` glyph with `IconOnly`.',
        "A click or submit runs the activation edge (`VbEdgeFx`): two heads sweep the cut outline in the button's own edge colours; reduced motion lights the edge and fades it.",
    ],
    preview='VbButton',
    height=520,
)

e(
    id='badge',
    tier='atom',
    name='BadgeComponent',
    unity=['BadgeComponent', 'BadgeTone', 'Badge', 'StatusChip', 'CreditsChip'],
    status='shipped',
    summary='A small slim chip: a word, optionally a glyph and a value. Status, currency, counts and tags.',
    uses=['frame', 'text', 'icon'],
    blocks=['vb-badge', 'vb-status', 'vb-credits'],
    csharp='var best = new BadgeComponent("Best", null, Ui.Number(18240)).Into(top);\nBadge.Of(chips, "Sharpened Output ×2", "burst");\nBadge.Status(StatusKind.Exposed, null, "+40% dmg", "2.1s").Into(row);   // StatusChip preset\nBadge.Credits(120).Into(hud);                                          // CreditsChip preset',
    uxml='<vb:BadgeComponent text="Best" value="18,240" />\n<vb:BadgeComponent text="Exposed" icon="exposed" tone="Enemy" />',
    rules=[
        'Tones: `Neutral`, `Ally`, `Enemy`, `Power`, `Warn`, `Inverted`. A tone always comes with a word; colour never travels alone.',
        'Badges are slim and never clickable; a tappable chip is a PerkChip or a button.',
        '`Set(value)` only touches the label when the value changed (HUD binding).',
    ],
    preview='BadgeComponent',
)

e(
    id='marker',
    tier='atom',
    name='MarkerComponent',
    unity=['WorldMarker'],
    status='shipped',
    summary='Who something belongs to, by shape: allies are circles, enemies diamonds. Selected, queued and move variants.',
    uses=['icon'],
    blocks=['vb-world-marker'],
    doc='WorldMarker',
    csharp='layer.Marker(id, worldPos, MarkerKind.Ally, 28);   // inside WorldLayer.BeginFrame / EndFrame',
    rules=['Shape before colour, always: the high-contrast theme and colour-blind players read the shape.'],
    preview='WorldMarker',
    height=110,
)

e(
    id='meter',
    tier='atom',
    name='MeterComponent',
    unity=['VbMeter'],
    status='shipped',
    summary='A flat track with a fill, an extra span (barrier), a dashed cost preview and numbered marks.',
    blocks=['vb-meter'],
    doc='VbMeter',
    csharp='var m = new VbMeter().Into(row);\nm.Set(.62f, .08f);         // fill, barrier span\nm.Preview(.44f, .62f);     // the cost being aimed\nm.Marks(new[] { .18f, .58f });',
    rules=['Meters never ramp green → red. Team colour plus a number nearby.'],
    preview='VbMeter',
    height=150,
)

e(
    id='world-meter',
    tier='atom',
    name='WorldMeterComponent',
    unity=['WorldHealthBar'],
    status='shipped',
    summary='The over-head team bar: ghost trail, barrier, elite and boss widths.',
    uses=['meter'],
    blocks=['vb-world-health'],
    doc='WorldHealthBar',
    csharp='layer.Bar(id, worldPos, hp01, barrier01, WorldTeam.Enemy, BarRank.Elite);',
    preview='WorldHealthBar',
    height=120,
)

e(
    id='combat-text',
    tier='atom',
    name='CombatTextComponent',
    unity=['CombatText'],
    status='shipped',
    summary='Floating numbers from confirmed simulation events, merged per target and capped.',
    uses=['text'],
    blocks=['vb-combat-text'],
    doc='CombatText',
    csharp='layer.Text(targetId, worldPos, 412, CombatKind.Damage);   // event-driven, pooled',
    preview='CombatText',
    height=120,
)

e(
    id='progress-input',
    tier='atom',
    name='ProgressInputComponent',
    unity=['VbSlider'],
    status='shipped',
    summary='A continuous value: cut track, teal fill, square thumb and the value as a number.',
    uses=['meter', 'text'],
    blocks=['vb-slider'],
    doc='VbSlider',
    csharp='new VbSlider("Music", settings.music, 0, 1).Into(audio).Changed(v => settings.music = v);',
    uxml='<vb:VbSlider label="Music" value="0.6" low-value="0" high-value="1" />',
    preview='VbSlider',
    height=130,
)

e(
    id='toggle-input',
    tier='atom',
    name='ToggleInputComponent',
    unity=['VbSwitch'],
    status='shipped',
    summary='A square on/off switch with a caps label and one line saying what it does.',
    uses=['frame', 'text'],
    blocks=['vb-switch'],
    doc='VbSwitch',
    csharp='new VbSwitch("REDUCED MOTION", "No camera shake · fades replace flicker", settings.reducedMotion)\n    .Into(feel).Changed(v => settings.reducedMotion = v);',
    rules=['The detail line says what changes, not "Enable X".', 'The whole row is the target (≥ 44 high).'],
    preview='VbSwitch',
    height=150,
)

e(
    id='choice-input',
    tier='atom',
    name='ChoiceInputComponent',
    unity=['VbSegmented'],
    status='shipped',
    summary='Two to five mutually exclusive peers in one frame, each with its consequence.',
    uses=['frame', 'text'],
    blocks=['vb-segmented'],
    doc='VbSegmented',
    csharp='new VbSegmented(new[] { ("LOW", "Stable 30 FPS"), ("STANDARD", "60 FPS target") }, settings.quality)\n    .Into(quality).Changed(v => settings.quality = v);',
    preview='VbSegmented',
    height=90,
)

e(
    id='popover',
    tier='atom',
    name='PopoverComponent',
    unity=['Popover'],
    status='shipped',
    summary='Hover, focus and tap content on one solid surface with an arrow toward its anchor, placed on the panel root, never clipped, clamped to the panel and placed again on every resize.',
    uses=['frame'],
    blocks=['vb-popover'],
    doc='Popover',
    csharp='Popover.Attach(chip, () => new PerkDetail().Bind(view, false), PopoverPlacement.Below, alignEnd: true);\nPopover.CloseAll();   // Navigator.Back does this first',
    rules=[
        'A popover is one solid surface (`panel-solid`, one edge, padding) with a painted arrow; its content drops its own surface and sits flat (`vb-popover__content`). `PopoverCard` is the layout: a caps title, an optional hint, flat rows.',
        'Height and width are clamped to the panel (at most 340 wide); the content scrolls past that. It enters with `PopoverIn` (140 ms) and leaves with `PopoverOut` (90 ms).',
        'Keep the kit in the theme (`VeilbreakGui.tss`): only theme sheets reach the panel root.',
    ],
    spec="**Slim:** the popover body should use `vb-frame--slim vb-frame--solid` instead of today's chipped solid panel.",
    preview=r"""render(col([h('div', { className: 'vb-l vb-t-caption' }, 'Hover or tap a chip'), row([h(V.PerkChip, { view: PERKS.general }), h(V.PerkChip, { view: PERKS.rare })], 6)], 10, { alignItems: 'flex-start' }));""",
    height=300,
)

e(
    id='overlay',
    tier='atom',
    name='OverlayComponent',
    unity=['OverlayHost', 'vb-modal', 'Navigator.Push'],
    status='shipped',
    summary='A modal layer: dims and disables what is under it, takes focus, and returns focus when it closes.',
    blocks=['vb-modal', 'vb-covered'],
    doc='OverlayHost',
    csharp='var layer = nav.Push(onClose: Resume);   // vb-modal, bottom-docked\nDialog.Sheet(layer, "LEAVE GAME?", "Your profile is saved.");\nnav.Back();                              // popover → modal → page back → fallback',
    rules=[
        "Back order is fixed: popover, then modal, then the page's back, then the host fallback.",
        'An overlay is never the only way out: Back always closes it.',
    ],
    preview=r"""render(board(h('div', { className: 'vb-screen vb-screen--pause' }, h('div', { className: 'vb-page' }, h('div', { className: 'vb-l vb-t-display', style: { marginTop: 120, textAlign: 'center' } }, 'PAUSED'))),
  h('div', { className: 'vb-modal', style: { position: 'absolute', left: 0, right: 0, top: 0, bottom: 0, paddingBottom: 12 } }, h(V.Dialog, { title: 'LEAVE GAME?', body: 'Your profile is saved.' }, h(V.VbButton, { title: 'EXIT', variant: 'danger', icon: 'back' }), h(V.VbButton, { title: 'STAY', icon: 'chevron' })))));""",
    stage='board',
)

e(
    id='backdrop',
    tier='atom',
    name='BackdropComponent',
    unity=['Backdrop'],
    status='shipped',
    summary='Full-bleed artwork behind a screen, scaled to cover, with a legibility shade along the bottom.',
    uses=['frame'],
    blocks=['vb-backdrop'],
    doc='Backdrop',
    csharp='new Backdrop(menuArt).Into(screenLayer);',
    preview='Backdrop',
    height=260,
)

e(
    id='scanlines',
    tier='atom',
    name='ScanLinesComponent',
    unity=['ScanLines'],
    status='shipped',
    summary='Projected-image lines for holo surfaces, one mesh. Hidden under vb-contrast.',
    blocks=['vb-scanlines'],
    doc='Holographic menu surfaces',
    csharp='new ScanLines().Into(holoBody);',
    rules=['Never behind numbers the player reads under pressure.'],
    preview='ScanLines',
    height=120,
)

e(
    id='progress',
    tier='atom',
    name='ProgressBarComponent',
    unity=['ProgressBar', 'ProgressTone'],
    status='shipped',
    summary='The one progress bar: continuous, or segments set to the number of levels, with the next level outlined.',
    blocks=['vb-progress'],
    csharp='new ProgressBar(segments: perk.max, value: perk.stack).Into(card);   // levels\nbar.Set(perk.stack, perk.max, next: true);                       // outline the level a pick adds\nnew ProgressBar(0, hp / (float)maxHp, ProgressTone.Ally).Into(row);   // continuous',
    uxml='<vb:ProgressBar segments="5" value="2" next="true" tone="Power" />',
    rules=[
        'Levels, stacks and steps are segments; health and progress are continuous.',
        'StatReadout, ListRow and PerkCard draw their bars with it; never hand-roll a track and fill.',
    ],
    preview='ProgressBar',
)

e(
    id='list-row',
    tier='atom',
    name='ListRowComponent',
    unity=['ListRow'],
    status='shipped',
    summary='The one list row: a glyph, a caps title and caption, an optional progress bar, a trailing value and action.',
    uses=['frame', 'icon', 'text', 'progress', 'button'],
    blocks=['vb-list-row'],
    csharp='var row = new ListRow(perk.name, perk.effect, perk.icon).Into(list);\nrow.Bar.Set(perk.stack, perk.max);\nrow.Action(new VbButton("BUY " + price, click: Buy));',
    rules=[
        'Slim; selected rows get the edge and brackets.',
        'Rows live in a ListLayout, which owns the 6-unit rhythm.',
    ],
    preview='ListRow',
)

e(
    id='tile',
    tier='atom',
    name='TileComponent',
    unity=['TileComponent', 'TileState', 'OperativeTile', 'SquadSlot'],
    status='shipped',
    summary='A square-ish, pickable cell: a picture or glyph, a short name, a caption, a state. Rosters, slots, inventories.',
    uses=['frame', 'text', 'icon', 'badge'],
    blocks=['vb-tile', 'vb-slot'],
    csharp='var cell = new TileComponent().Into(grid).Set("Visor", "Optic", glyph: "optic", state: TileState.In, order: 1, captionIcon: "optic");\ncell.Tier = RarityTier.Violet; cell.Picked += () => Inspect(item);\nnew OperativeTile().Into(grid).Bind(view, slot: 2, focused: true);   // a TileComponent\nnew SquadSlot().Into(slots).Bind(null, 4);                          // empty: dashed, "SLOT 4"',
    uxml='<vb:TileComponent title="Visor" caption="Optic" glyph="optic" state="In, Focused" order="1" />',
    rules=[
        'A tile is chipped: it is something you pick.',
        'States combine: `In | Focused`; `Locked` and `Empty` stand alone. Focused = brackets + glow (the glow budget\'s "selected thing").',
        'Locked tiles stay visible and say how to unlock. Lay tiles out in a GridLayout, never free-floating.',
    ],
    preview='TileComponent',
)

e(
    id='edge-fx',
    tier='atom',
    name='EdgeFxEffect',
    unity=['VbEdgeFx'],
    status='shipped',
    summary='The animated border of the MONO wireframe kit on the cut outline of any shaped element: the activation sweep and the damage flash.',
    uses=['frame', 'button'],
    blocks=['vb-edgefx'],
    doc='VbEdgeFx',
    csharp="VbEdgeFx.Attach(element);            // plays on click or submit; VbButton and Clickable already attach it\nVbEdgeFx.Play(element);              // now, in the element's own edge colours\nVbEdgeFx.Damage(card, strength: 1);  // a unit card taking a hit (UnitCard.AutoDamage does this)",
    rules=[
        'Activate (520 ms): two bright heads run clockwise from opposite points of the outline, each with a fading tail and a soft glow, then an afterglow fades. Damage (560 ms): a translucent red flash and a red border glow.',
        'Reduced motion lights the whole edge at once and fades it in 100 ms; reduced flashes drop the damage flash fill.',
        'One overlay child (`vb-edgefx`) is added when it plays and removed when it ends; a host that leaves its panel finishes it.',
    ],
    preview='VbEdgeFx',
    height=260,
)

e(
    id='glitch-reveal',
    tier='atom',
    name='GlitchReveal',
    unity=['Glitch'],
    status='shipped',
    summary='The title reveal: text decodes out of noise glyphs while it jitters sideways in steps that shrink to nothing, about two seconds.',
    uses=['text', 'glitch-text'],
    blocks=['vb-reveal-title'],
    doc='Overlay transitions and the title reveal',
    csharp='Glitch.Reveal(title);                    // 2000 ms (MotionRecipes.Reveal); GlitchText also tears into bands\nlabel.AddToClassList(Glitch.TitleClass);   // reveals when its overlay or screen is presented\nannouncer.RevealMilliseconds = 700;      // heads-up notices, off by default',
    rules=[
        "For titles, eyebrows and a few heads-up labels: the pause title and its eyebrow, the results title, the draft's. Never body copy.",
        'The text is whole at the end and the element rests at its own place; reduced motion or flashes show the whole text with a 300 ms fade.',
    ],
    preview='Glitch',
    height=200,
)

e(
    id='clip',
    tier='atom',
    name='ClipFillAndImage',
    unity=['VbClipFill', 'VbClipImage'],
    status='shipped',
    summary='A block or a picture that follows the cut of the shape it sits in, so a charge bar or a portrait never overflows a chamfered corner.',
    uses=['frame'],
    blocks=['vb-clip-fill', 'vb-clip-image'],
    doc='VbClipFill and VbClipImage',
    csharp='charge = new VbClipFill().Classes("vb-skill__charge").Into(skill);   // colour is --vb-fill; Ui.Fill sizes it\nportrait = new VbClipImage().Classes("vb-operative__portrait").Into(card);\nUi.Image(portrait, texture);',
    rules=[
        "UI Toolkit's `overflow: hidden` clips to a rectangle only. These paint themselves as polygons: their rectangle clipped to the outline of the nearest `VbShape` ancestor (`--vb-cut`, `--vb-cut-side`).",
        "Layout, picking and the parents' overflow are unchanged; with no shaped ancestor they are the plain rectangle.",
    ],
    preview='VbClipFill',
    height=200,
)

e(
    id='popover-card',
    tier='composite',
    name='PopoverCardComposite',
    unity=['PopoverCard'],
    status='shipped',
    summary='Popover content: a caps title, an optional hint line, then flat rows one tone above the popover surface.',
    uses=['popover', 'text'],
    blocks=['vb-popover-card'],
    doc='Popover',
    csharp='Popover.Attach(info, () => new PopoverCard("RUN", "Seed 80A2EB4E"), PopoverPlacement.Below, alignEnd: true);\nvar card = new PopoverCard("QUEUE"); card.Row("Wave", "07"); card.Row("Standing", "3 / 4", good: true);',
    rules=["Rows are flat: no edge, one tone above the popover surface. A queue row's cancel goes at the row's end."],
    preview='PopoverCard',
    height=260,
)

e(
    id='card',
    tier='atom',
    name='CardComponent',
    unity=['CardComponent'],
    status='shipped',
    summary='A slim container with an optional media band, a head, a body and a foot. The base every card composite extends.',
    uses=['frame', 'label', 'text'],
    blocks=['vb-card'],
    csharp='var card = new CardComponent("Checkpoint", "WAVE 07").Into(actions);\ncard.Add(new Label("3 of 4 standing · 5 perks"));             // children go to the body\ncard.Actions(new VbButton("RESUME", ButtonVariant.Secondary, icon: "play"));\ncard.Selected = true;                                          // chipped: edge + brackets',
    uxml='<vb:CardComponent title="Last run" meta="#3 OF 20" selected="true">\n  <ui:Label text="18,240" class="vb-t-num" />\n</vb:CardComponent>',
    rules=[
        'Cards are slim. Only a selected card becomes chipped (brackets + edge).',
        'Head: one LabelComponent and an optional meta. Foot: actions, right-aligned; it hides while empty.',
        'One padding rhythm: 12 / 14.',
    ],
    preview='CardComponent',
)

# ════════════════════════════════════════════════════════════════════════════
# COMPOSITES — atoms combined into one reusable part with its own view model
# ════════════════════════════════════════════════════════════════════════════

e(
    id='button-with-icon',
    tier='composite',
    name='ButtonWithIconComposite',
    unity=['VbButton (icon)'],
    status='shipped',
    summary='A button with a trailing glyph: chevron for "go on", play for "resume", a domain glyph for destinations.',
    uses=['button', 'icon'],
    blocks=['vb-button__icon'],
    csharp='new VbButton("SETTINGS", ButtonVariant.Secondary, null, "settings", OpenSettings);\nnew VbButton("PLAY", ButtonVariant.Primary, "Choose a squad", "chevron", Squad);',
    rules=[
        '`chevron` and `play` draw at 12, every other glyph at 20.',
        "The glyph repeats the label's meaning; never icon-only (that is IconButtonComposite).",
    ],
    preview=r"""var B = V.VbButton;
render(col([h(B, { title: 'PLAY', variant: 'primary', sub: 'Squad · Aria · Sol · Aegis · Nyra', icon: 'chevron', style: { width: 340 } }), row([h(B, { title: 'SETTINGS', icon: 'settings', style: { width: 166 } }), h(B, { title: 'RESUME', icon: 'play', style: { width: 166 } })]), h(B, { title: 'ABANDON RUN', variant: 'danger', icon: 'remove', style: { width: 220 } })], 10, { alignItems: 'flex-start' }));""",
    height=240,
)

e(
    id='icon-button',
    tier='composite',
    name='IconButtonComposite',
    unity=['IconButtonComposite'],
    status='shipped',
    summary='A 44 × 44 chipped square holding one glyph (or a short state), with a tooltip. Back, pause, close, speed, autoplay.',
    uses=['frame', 'icon'],
    blocks=['vb-icon-button', 'vb-header__back', 'vb-hud__pause', 'vb-hud__auto', 'vb-hud__speed', 'vb-details__close'],
    csharp='new IconButtonComposite("back", "Back", Back).Into(header);\nvar auto = new IconButtonComposite("automation", "Autoplay", ToggleAuto) { Quiet = true, Active = run.Auto }.Into(row);\nvar speed = new IconButtonComposite("", "Toggle simulation speed", Speed) { Text = "2×" }.Into(row);',
    uxml='<vb:IconButtonComposite icon="pause" tooltip="Pause" quiet="true" />',
    rules=[
        'Always 44 × 44 at least, always a tooltip (it is the accessible name).',
        'Chipped (cut 6): it is a control. `Quiet` over the world (the HUD band), `Active` when toggled on.',
        'ScreenHeader, HudTop and OperativeDetails build theirs from it; the owner classes only position it.',
    ],
    preview='IconButtonComposite',
)

e(
    id='drawer',
    tier='composite',
    name='DrawerOverlayComposite',
    unity=['DrawerOverlay'],
    status='shipped',
    summary='A bottom drawer: a slim sheet that slides up over a dimmed backdrop to the height of its content (at most 90% of the screen); drag its top up for full screen or down to close.',
    uses=['overlay', 'frame', 'icon-button', 'label', 'button'],
    blocks=['vb-drawer', 'vb-modal--drawer'],
    csharp='var d = DrawerOverlay.Push(nav, "Aria", closed: Refocus);    // modal layer; Back pops it\nnew OperativeDetails().Into(d).Bind(view, slot, squadFull);   // children go to the body\nd.Actions(new VbButton("REMOVE FROM SQUAD", ButtonVariant.Danger, icon: "remove"));\nvar docked = new DrawerOverlay { Backdrop = false }.Into(view).Open();   // inline: only the sheet takes input\ndocked.Dismissed += Resume;   // backdrop tap, × or a drag below a quarter of the screen\nd.Snap = DrawerSnap.Full;   // or let the player drag up / tap the handle\nd.Open(DrawerSnap.Half);   // half the screen is opt-in; the default is DrawerSnap.Fit',
    uxml='<vb:DrawerOverlay title="Aria" meta="SLOT 1" backdrop="true" close-button="false" />',
    rules=[
        'Use a drawer for detail and secondary decisions that keep the page behind them: operative details, the paused run, filters. A yes/no confirmation stays a ConfirmDialogView.',
        'Opens at the height of its content (`DrawerSnap.Fit`, at most 90% of the screen; `Half` is opt-in). Drag the handle or head: up snaps to full screen, down back to fit, below a quarter closes it. A tap on the handle toggles full and back.',
        'The backdrop and × dismiss it too; Back pops a pushed drawer. The body scrolls only past the content height; actions sit in the foot, an `ActionRow` that fills the sheet and stacks when narrow, primary first.',
        'Reduced motion: it appears and goes at once. High contrast: the sheet gets its edge and the handle goes white.',
    ],
    preview='DrawerOverlay',
    stage='board',
)

e(
    id='status-row',
    tier='composite',
    name='StatusRowComposite',
    unity=['StatusRow', 'StatusSquare'],
    status='shipped',
    summary='The timed statuses of a unit as a row of tiny squares; hover, focus or a tap lists them all in a popover.',
    uses=['badge', 'popover', 'effect-list'],
    blocks=['vb-status-row', 'vb-status-square'],
    doc='StatusRow',
    csharp='var statuses = new StatusRow().Into(card);     // UnitCard.Statuses is one, above the card\nstatuses.Bind(operative.effects);                // only effects with a time show; at most six and a "+n" square\nstatuses.Details.Pin();                          // open for captures',
    rules=[
        'About 20 × 20 a square, glyph alone (harm in enemy colour, benefit in ally colour), the stack count in the corner: never colour alone.',
        'The row is one 44-unit target through padding a negative margin takes back; its popover is one vertical `EffectList`.',
        'Passives, perks and effects without a time are not statuses and never show; a unit with none shows nothing.',
    ],
    preview='StatusRow',
    height=300,
)

e(
    id='field',
    tier='composite',
    name='FieldComposite',
    unity=['FieldComposite', 'VbSlider', 'VbSwitch', 'VbSegmented'],
    status='shipped',
    summary='One setting: a label, its input, and one line of what it does.',
    uses=['label', 'progress-input', 'toggle-input', 'choice-input', 'text'],
    blocks=['vb-field', 'vb-section'],
    csharp='new FieldComposite("Camera motion", new VbSegmented(options, s.camera).Changed(v => s.camera = v), "Pinch to zoom, twist to turn.").Into(fieldset);\nnew VbSlider("Music", s.music).Into(fieldset).Changed(v => s.music = v);          // carries its own label\nnew VbSwitch("HAPTICS", "Light pulse when an order is accepted", s.haptics).Into(fieldset);',
    uxml='<vb:FieldComposite label="Camera motion" help="Pinch to zoom, twist to turn.">\n  <vb:VbSegmented />\n</vb:FieldComposite>',
    rules=[
        'Use FieldComposite for inputs without their own label (segmented, custom pickers); VbSlider and VbSwitch include theirs.',
        'Height rhythm inside a fieldset: slider 54, switch 62, field + 8.',
        'A field explains its consequence in its help line.',
    ],
    preview='FieldComposite',
)

e(
    id='fieldset',
    tier='composite',
    name='FieldsetComposite',
    unity=['Ui.Section', 'vb-section'],
    status='shipped',
    summary='A titled group of related fields in one slim container.',
    uses=['frame', 'label', 'field'],
    blocks=['vb-section'],
    doc='Layout classes (`Layout.uss`)',
    csharp='var audio = Ui.Section(scroll, "AUDIO");\nnew VbSlider("Master", s.master).Into(audio).Changed(v => s.master = v);',
    rules=[
        'One topic per fieldset, 1–4 fields. A fieldset title is a LabelComponent, never a sentence.',
        'Slim frame: the biggest visual change of 0.3.0.',
    ],
    preview=r"""render(col([h(V.VbFrame, { className: 'vb-panel vb-section vb-frame--slim', style: { width: 358 } }, h('div', { className: 'vb-l vb-t-label vb-section__title' }, 'AUDIO'), h(V.VbSlider, { label: 'Master', value: 0.8 }), h(V.VbSlider, { label: 'Music', value: 0.55 }), h(V.VbSlider, { label: 'Effects', value: 0.8 }))], 8, { alignItems: 'flex-start' }));""",
    height=240,
)

e(
    id='form',
    tier='composite',
    name='FormComposite',
    unity=['FormComposite'],
    status='shipped',
    summary='Fieldsets in a scroll, the saved state in the header, a note and a footer of secondary actions.',
    uses=['fieldset', 'field', 'button', 'text'],
    blocks=['vb-form'],
    csharp='var form = new FormComposite(header) { SavedMeta = "SAVED" }.Into(page);\nvar audio = form.Fieldset("AUDIO");\nnew VbSlider("Master", s.master).Into(audio).Changed(v => s.master = v);\nform.Note("automation", "Autoplay lives in the battle HUD.");\nform.Footer(new VbButton("CLEAR LOCAL SCORES", ButtonVariant.Danger, click: ClearScores));\nform.Changed += SaveProfile;   // any slider, switch or segmented inside; no Submit',
    rules=[
        'Settings apply as they change. There is no Submit or OK button; the header meta says SAVED.',
        'Destructive actions live in the footer, danger variant, and confirm with ConfirmDialogView.',
        'On a compact board the footer stacks.',
    ],
    preview='FormComposite',
)

e(
    id='stat-readout',
    tier='composite',
    name='StatReadoutComposite',
    unity=['StatReadout'],
    status='shipped',
    summary='A labelled number with a unit, a previewed delta and an optional bar.',
    uses=['label', 'text', 'meter'],
    blocks=['vb-stat'],
    doc='StatReadout',
    csharp='new StatReadout("STANDING", "3", "/4").Into(numbers);\nhp.Set("1,240", null, .82f).Delta("+60");',
    preview='StatReadout',
    height=190,
)

e(
    id='status-badge',
    tier='composite',
    name='StatusBadgeComposite',
    unity=['StatusChip'],
    status='shipped',
    summary='A status with its own glyph and word, so no two statuses read alike and none relies on colour.',
    uses=['badge', 'icon', 'text'],
    blocks=['vb-status'],
    doc='StatusChip',
    csharp='new StatusChip(StatusKind.Silenced, null, "no skills", "1.4s").Into(chips);',
    preview='StatusChip',
    height=90,
)

e(
    id='chip-row',
    tier='composite',
    name='ChipRowComposite',
    unity=['ChipRow', 'CreditsChip', 'PerkChip'],
    status='shipped',
    summary='A glyph, a word and a value on one plate with no edge: a perk chip (tinted by rarity; hover, focus or tap shows PerkDetail) or the read-only credits chip.',
    uses=['badge', 'popover', 'perk-detail', 'icon'],
    blocks=['vb-chip-row', 'vb-credits'],
    doc='ChipRow',
    csharp='new ChipRow(alignEnd: false).Into(chips).Bind(view);                         // a perk: glyph, name, ×stacks; its detail opens on hover, focus or tap\nvar credits = ChipRow.Credits().Into(power);                                          // read only, as tall as its plate\ncredits.SetNumber(run.Credits);                                                       // counts up when it changes',
    rules=[
        "A chip that is a target has a 44-unit hit area: the plate is 30 and the element's padding makes up the rest. A chip that only reads is `Static`.",
        'A perk chip is tinted by its rarity (`vb-perk--{tier}`), never framed. Chips wrap in a row (`vb-row vb-row--wrap`) with no gap of their own.',
        'Replaces `PerkChip` and `CreditsChip`, which stay one release as `[Obsolete]` forwarding types.',
    ],
    preview='ChipRow',
    height=300,
)

e(
    id='health-bar',
    tier='composite',
    name='HealthBarComposite',
    unity=['HealthBar'],
    status='shipped',
    summary='Health over stability, with barrier, a ghost of recent damage and an optional caption.',
    uses=['meter', 'label'],
    blocks=['vb-health'],
    doc='HealthBar',
    csharp='new HealthBar().Into(cell).Set(a.hp, a.maxHp, a.barrier, a.stability / (float)a.maxStability);',
    preview='HealthBar',
    height=230,
)

e(
    id='power-strip',
    tier='composite',
    name='PowerStripComposite',
    unity=['PowerStrip'],
    status='shipped',
    summary='The shared Power pool: value, regen, the live consequence of an aim, the reserve line and the queue.',
    uses=['meter', 'label', 'text', 'queue-chip'],
    blocks=['vb-power'],
    doc='PowerStrip',
    csharp='strip.Set(hud.power, hud.maxPower, hud.regen, hud.hint, hud.previewFrom, hud.previewTo, marks);\nstrip.Reserve(25, 100); strip.Queue(hud.queue, q => run.CancelQueued(q.actor, q.slot));',
    preview='PowerStrip',
    height=330,
)

e(
    id='focus-meter',
    tier='composite',
    name='FocusMeterComposite',
    unity=['FocusMeter'],
    status='shipped',
    summary='Focus drains in real time while targeting slows combat and regenerates outside it.',
    uses=['meter', 'label', 'text'],
    blocks=['vb-focus'],
    doc='FocusMeter',
    csharp='focus.Set(run.Focus, slow: aiming, regenPerSecond: 12.5f, drainPerSecond: 25, slowScale: .2f);',
    preview='FocusMeter',
    height=110,
)

e(
    id='skill-button',
    tier='composite',
    name='SkillButtonComposite',
    unity=['SkillButton', 'SkillView', 'SkillState'],
    status='shipped',
    summary='The 44 × 56 skill control on a unit card: one layout in every state, with the states as overlays.',
    uses=['button', 'icon', 'meter', 'badge'],
    blocks=['vb-skill'],
    doc='SkillButton',
    csharp='var skill = card.Skill(0);                 // A slot; Skill(1) is S\nskill.AddManipulator(new TargetDrag(BeginAim, MoveAim, Release, CancelAim));',
    rules=[
        'Slot letter, glyph and one cost line in every state; cooldown, windup and silence count their seconds over the glyph, a shortfall ("−3"), a queue place ("#1") or a buffered arrow sits in the corner.',
        "The charge and the windup sweep are `VbClipFill`s, so they follow the button's bottom-right cut instead of overflowing it.",
    ],
    preview='SkillButton',
    height=250,
)

e(
    id='queue-chip',
    tier='composite',
    name='QueueChipComposite',
    unity=['QueueChip', 'QueueView'],
    status='shipped',
    summary='One entry of the Power queue: place, skill, cost, time, and × for manual entries.',
    uses=['badge', 'text', 'icon-button'],
    blocks=['vb-queue-chip'],
    doc='QueueChip',
    csharp='new QueueChip().Bind(view, position: 1, preview: false).Cancelled += Cancel;',
    preview='QueueChip',
    height=200,
)

e(
    id='queue-list',
    tier='composite',
    name='QueueListComposite',
    unity=['QueueList'],
    status='shipped',
    summary='Upcoming casts in firing order; popover content of the HUD band. Manual rows cancel on tap.',
    uses=['queue-chip', 'label'],
    blocks=['vb-queue'],
    doc='QueueList',
    csharp='list.Bind(hud.queue, hud.queueTitle, hud.queueHint, q => run.CancelQueued(q.actor, q.slot));',
    rules=['Rows are slim (they are list items, not controls); the × inside is the control.'],
    preview='QueueList',
    height=200,
)

e(
    id='screen-header',
    tier='composite',
    name='ScreenHeaderComposite',
    unity=['ScreenHeader'],
    status='shipped',
    summary='Back, title and a short meta read-out at the top of a page; the back arrow hides for a screen that has its own way back.',
    uses=['icon-button', 'text', 'label'],
    blocks=['vb-header'],
    doc='ScreenHeader',
    csharp='var header = new ScreenHeader("SETTINGS", "SAVED").Into(page);\nheader.Back += Leave; page.AddToClassList("vb-page--headed");\nvar paused = new ScreenHeader("PAUSED", "", "Resume") { ShowBack = false };   // the pause overlay: RESUME is the way back\npaused.TitleLabel.AddToClassList(Glitch.TitleClass);   // reveals when the overlay is presented',
    preview='ScreenHeader',
    height=150,
)

e(
    id='hud-band',
    tier='composite',
    name='HudBandComposite',
    unity=['HudTop', 'HudView'],
    status='shipped',
    summary='The translucent portrait HUD band: pause, the autoplay toggle (AUTO or MANUAL), wave and score, speed, Power and focus.',
    uses=['icon-button', 'power-strip', 'focus-meter', 'chip-row', 'queue-list', 'popover'],
    blocks=['vb-hud'],
    doc='HudTop',
    csharp='hudTop = new HudTop().Into(hud);\nhudTop.PauseClicked += Pause; hudTop.AutoClicked += run.ToggleAuto;\nhudTop.Bind(hudView);   // every frame',
    preview='HudTop',
    height=470,
)

e(
    id='boss-bar',
    tier='composite',
    name='BossBarComposite',
    unity=['BossBar', 'BossView'],
    status='shipped',
    summary='The boss read-out: name, health over stability, shield and interrupt badges.',
    uses=['health-bar', 'status-badge', 'icon', 'text'],
    blocks=['vb-boss'],
    doc='BossBar',
    csharp='bossBar.Bind(new BossView { name = "Signal Warden", health = .64f, stability = .35f, shielded = true });',
    preview='BossBar',
    height=170,
)

e(
    id='score-row',
    tier='composite',
    name='ScoreRowComposite',
    unity=['ScoreRow', 'ScoreView'],
    status='shipped',
    summary='One high-score line: rank, score, wave and bosses, the squad codes; the current run is framed.',
    uses=['frame', 'text', 'badge'],
    blocks=['vb-score'],
    doc='ScoreRow',
    csharp='new ScoreRow().Into(list).Bind(new ScoreView { rank = 1, score = 18240, wave = 12, bosses = 2, squad = codes });',
    rules=['Rows are slim; the current run is the one chipped row (edge + THIS RUN badge).'],
    preview='ScoreRow',
    height=230,
)

e(
    id='unit-card',
    tier='composite',
    name='UnitCardComposite',
    unity=['UnitCard', 'OperativeCard', 'OperativeView'],
    status='shipped',
    summary='The portrait battle card: band, name, HP and barrier, A and S skills, and the timed statuses as a row of small squares above it.',
    uses=['card', 'health-bar', 'skill-button', 'status-row', 'clip', 'edge-fx'],
    blocks=['vb-operative'],
    doc='UnitCard (was `OperativeCard`)',
    csharp='var card = new UnitCard().Classes("cards__card").Into(cards);\ncard.Bind(view);   // every frame; reuses children\ncard.AutoDamage = true;   // a drop in hit points plays the damage edge; card.Damage(strength) from a host event',
    rules=[
        'The name is the only identifier: no code over it. A mode chip shows only for MANUAL (with its seconds) and AIMING.',
        "The portrait and the band backdrop follow the card's top-left cut; the last skill takes the card's own bottom-right cut.",
        'Timed statuses show as `StatusRow` squares above the card; a passive or a perk is not a status.',
    ],
    preview='UnitCard',
    height=260,
)

e(
    id='unit-tile',
    tier='composite',
    name='UnitTileComposite',
    unity=['UnitTile', 'OperativeTile'],
    status='shipped',
    summary='A roster tile: in squad (numbered), focused, rarity tiers, locked with how to unlock; the role is its icon alone on a narrow board.',
    uses=['tile', 'badge', 'icon', 'clip'],
    blocks=['vb-tile'],
    doc='UnitTile (was `OperativeTile`)',
    csharp='var tile = new UnitTile().Into(grid).Bind(view, slot, focused, RarityTier.Gold, locked: null);\ntile.Picked += () => Focus(id);',
    preview='UnitTile',
    height=330,
)

e(
    id='unit-slot',
    tier='composite',
    name='UnitSlotComposite',
    unity=['UnitSlot', 'SquadSlot'],
    status='shipped',
    summary='One of four squad slots: filled (its name alone), or dashed and empty with SLOT n.',
    uses=['tile', 'text'],
    blocks=['vb-slot'],
    doc='UnitSlot (was `SquadSlot`)',
    csharp='new UnitSlot().Into(slots).Bind(who != null ? Kit(who) : null, i + 1);\n// SquadView reorders filled slots: drag, long-press or Alt+arrow (ReorderManipulator)',
    preview='UnitSlot',
    height=110,
)

e(
    id='unit-detail',
    tier='composite',
    name='UnitDetailComposite',
    unity=['UnitDetail', 'OperativeDetails'],
    status='shipped',
    summary='The unit sheet: bars scaled to the roster, then the passive, the basic attack and both skills as one list, the lore prompt, and add/remove.',
    uses=['card', 'health-bar', 'button', 'icon-button', 'text'],
    blocks=['vb-details'],
    doc='UnitDetail (was `OperativeDetails`)',
    csharp='details.Bind(view, slot, squadFull, maxHp, maxArmor, maxStability, showStability: StabilityVaries);\ndetails.Toggled += () => ToggleSquad(view.id);\nUnitDetail.Rows(view);   // the passive, the basic attack, then the skills',
    rules=[
        'Cost and cooldown show only where a row has one; emphasis marks in descriptions render as emphasis.',
        'Stability shows only when it varies between units (`SquadView.StabilityVaries`).',
    ],
    preview='UnitDetail',
    height=520,
)

e(
    id='result-summary',
    tier='composite',
    name='ResultSummaryComposite',
    unity=['ResultSummary', 'UnitStats', 'SquadResult', 'OperativeStats', 'ContributionView'],
    status='shipped',
    summary="The results cell: the unit's name and FELL Wn when it fell; hover, focus or tap opens its contribution.",
    uses=['tile', 'popover', 'stat-readout'],
    blocks=['vb-result', 'vb-contribution'],
    doc='UnitStats and ResultSummary (were `OperativeStats` and `SquadResult`)',
    csharp='new ResultSummary().Into(squad).Bind(contribution);',
    preview='ResultSummary',
    height=420,
)

e(
    id='choice-card',
    tier='composite',
    name='ChoiceCardComposite',
    unity=['ChoiceCard', 'PerkCard', 'PerkView', 'RoadView'],
    status='shipped',
    summary='One option the player takes by tapping it (a road, an extract choice, a perk): tag, centred glyph, title, facts, rank cells, the value after the pick and the action word.',
    uses=['card', 'badge', 'text', 'progress', 'icon'],
    blocks=['vb-choice'],
    doc='ChoiceCard',
    csharp='var card = new ChoiceCard().Into(row).Bind(perkView, selected: i == pick);                  // a perk\nvar road = new ChoiceCard(ChoiceSide.Left).Into(roads).Bind(roadView, "TAKE THIS ROAD");   // a road of a pair\ncard.Picked += () => Pick(i);',
    rules=[
        'A perk is a ChoiceCard wherever it is offered: its tier tints the card, tag and glyph (rare cyan, epic violet). A pair of roads takes `Left` and `Right` so the cut sides face outwards.',
        'Emphasis marks in `Text` render as emphasis. Only the choice card keeps its edge under the border rule.',
    ],
    preview='ChoiceCard',
    height=620,
)

e(
    id='perk-detail',
    tier='composite',
    name='PerkDetailComposite',
    unity=['PerkDetail'],
    status='shipped',
    summary='The full explanation of a perk: draft (now → after pick) or owned (active, stacks, waves).',
    uses=['card', 'text', 'label'],
    blocks=['vb-perk-detail'],
    doc='Perks',
    csharp='new PerkDetail().Into(page).Bind(view, draft: true);',
    preview='PerkDetail',
    height=230,
)

e(
    id='stat-row',
    tier='composite',
    name='StatRowComposite',
    unity=['StatRow'],
    status='shipped',
    summary='One fact on one row: a key that keeps its width, a value that wraps beside it, an optional glyph and a trailing action 44 units tall.',
    uses=['list-row', 'progress', 'button', 'icon'],
    blocks=['vb-stat-row'],
    doc='StatRow',
    csharp='var row = new StatRow("ARIA", "800 / 1000 HP → 550 HP") { Plated = true, Stacked = true, Icon = "health" }.Into(list);\nrow.Bar.Set(0.8f);\nrow.WithAction(new VbButton("+30", ButtonVariant.Danger, null, "currency", Sell));',
    rules=[
        'It is a `KeyValueRow`: everything that reads one reads this. `Stacked` puts the value under the key; `Dimmed` greys a row that cannot be acted on, with no second price.',
        "The perk detail rows and the merchant's offers, sales and health trades are StatRows.",
    ],
    preview='StatRow',
    height=330,
)

e(
    id='carousel',
    tier='composite',
    name='CarouselComposite',
    unity=['Carousel'],
    status='shipped',
    summary='Cards where the active one takes 64% of the width and its neighbours sit partly beneath it, tilted; endless both ways, swipe, tap, arrow keys or focus.',
    uses=['choice-card'],
    blocks=['vb-carousel'],
    doc='Carousel',
    csharp='var carousel = new Carousel().Into(page);\ncarousel.Bind(perks, p => p.id, () => new ChoiceCard(), (card, p, i) => ((ChoiceCard)card).Bind(p, i == pick), pick);\ncarousel.Changed += i => Pick(i);',
    rules=[
        'A swipe follows the finger and settles on the nearest card; a flick goes one further. It glides in 320 ms on real time and is instant under reduced motion.',
        'The carousel is layout only: any element is a card, and the active card shows its own information, so nothing repeats it elsewhere.',
    ],
    preview='Carousel',
    height=400,
)

e(
    id='perk-row',
    tier='composite',
    name='PerkRowComposite',
    unity=['PerkRow'],
    status='shipped',
    summary='An active-perk list row; selected is framed.',
    uses=['frame', 'icon', 'text'],
    blocks=['vb-perk-row'],
    doc='Perks',
    csharp='new PerkRow().Into(list).Bind(view, selected);',
    preview='PerkRow',
    height=230,
)

e(
    id='dialog',
    tier='composite',
    name='DialogComposite',
    unity=['Dialog.Sheet', 'Dialog.Push'],
    status='shipped',
    summary='A confirmation sheet: title, one line of body, then the actions. Usually on an overlay.',
    uses=['overlay', 'frame', 'text', 'button'],
    blocks=['vb-dialog'],
    doc='Dialog',
    csharp='var sheet = Dialog.Push(nav, "END THIS RUN?", "Your current run will be recorded and its checkpoint removed.");\nnew VbButton("ABANDON", ButtonVariant.Danger, null, "remove", Abandon).Into(sheet);\nnew VbButton("KEEP PLAYING", ButtonVariant.Secondary, null, "chevron", nav.Back).Into(sheet);',
    rules=[
        'Titles ask the question ("END THIS RUN?"); the body states the consequence.',
        'The destructive action is danger, the safe one secondary; Back = the safe one.',
    ],
    preview='Dialog',
    height=300,
)

e(
    id='holo-frame',
    tier='composite',
    name='HoloFrameComposite',
    unity=['HoloFrame'],
    status='shipped',
    summary='A diegetic menu panel: cut frame with brackets, scanlines, a chromatic caps title and a flicker-in.',
    uses=['frame', 'scanlines', 'glitch-text'],
    blocks=['vb-holo'],
    doc='Holographic menu surfaces',
    csharp='var holo = new HoloFrame("LAST RUN").Into(menu); holo.FlickerIn();',
    rules=['Menus only, one in focus per screen. Never in battle.'],
    preview='HoloFrame',
    height=230,
)

e(
    id='holo-platform',
    tier='composite',
    name='HoloPlatformComposite',
    unity=['HoloPlatform'],
    status='shipped',
    summary='The 2D stand-in for a lineup plinth: occupied, selected with brackets, open slot.',
    uses=['marker', 'frame', 'text'],
    blocks=['vb-platform'],
    doc='Holographic menu surfaces',
    csharp='platform.Bind("Aria", "Lv 7", PlatformState.Selected, WorldTeam.Ally, showMarker: false);',
    preview='HoloPlatform',
    height=220,
)

e(
    id='reveal-card',
    tier='composite',
    name='RevealCardComposite',
    unity=['RevealCard'],
    status='shipped',
    summary='A rift result: a holographic back that flips to a tier-framed front with its tier word.',
    uses=['card', 'scanlines', 'badge'],
    blocks=['vb-reveal'],
    doc='Holographic menu surfaces',
    csharp='card.Bind(RarityTier.Gold, "Nyra", "Healer", item: false, isNew: true, dupe: null, flipped: false); card.Flip();',
    preview='RevealCard',
    height=260,
)

e(
    id='rule-row',
    tier='composite',
    name='RuleRowComposite',
    unity=['RuleRow', 'RuleView'],
    status='shipped',
    summary='One IF … THEN automation rule with a toggle and a note on when it can run.',
    uses=['frame', 'text', 'toggle-input'],
    blocks=['vb-rule'],
    doc='Holographic menu surfaces',
    csharp='new RuleRow().Into(list).Bind(rule).Toggled += SaveRules;',
    preview='RuleRow',
    height=300,
)

e(
    id='world-label',
    tier='composite',
    name='WorldLabelComposite',
    unity=['WorldLabel'],
    status='shipped',
    summary='Billboards in the world: aim chip, name tag, telegraph timer, move estimate, blocked and elite traits; labels with the same group key merge.',
    uses=['text', 'badge'],
    blocks=['vb-world-label'],
    doc='WorldLabel',
    csharp='layer.Label(id, worldPos, WorldLabelKind.Aim, "A · NEEDLE STORM", "18 POWER · queues as #3", WorldTeam.Ally);\nlayer.Label(id, pos, WorldLabelKind.Elite, "ELITE", "Voltaic", WorldTeam.Enemy);        // "◆ Voltaic"\nlayer.Label(id, pos, WorldLabelKind.Telegraph, "TETHERED", team: WorldTeam.Enemy, group: "tether");   // three of them read "TETHERED ×3" at their centroid',
    rules=[
        'Plain text on a dark underlay with no edge; ownership reads from the text colour. Elite is one line: a diamond mark and the trait.',
        'Labels with the same `group` key and title merge into one "TETHERED ×3" at the centroid of their projected points and drop their detail; a lone label reads as itself.',
    ],
    preview='WorldLabel',
    height=130,
)

e(
    id='offscreen',
    tier='composite',
    name='OffscreenIndicatorComposite',
    unity=['OffscreenIndicator'],
    status='shipped',
    summary='An edge chip pointing at a fighter the camera cannot frame.',
    uses=['badge', 'icon', 'text'],
    blocks=['vb-offscreen'],
    doc='OffscreenIndicator',
    csharp='layer.Offscreen(id, worldPos, "BREACH", WorldTeam.Enemy, metres: 4, threat: true);',
    preview='OffscreenIndicator',
    height=110,
)

e(
    id='edge-warning',
    tier='composite',
    name='EdgeWarningComposite',
    unity=['EdgeWarning'],
    status='shipped',
    summary='A band of rippling chevrons and a caption along a board edge for a threat about to cross it.',
    uses=['icon', 'text'],
    blocks=['vb-edge-warning'],
    doc='EdgeWarning',
    csharp='warning.Bind("BREACH · 2 HOSTILES", top: false); warning.Set(urgency, Time.time, reducedMotion);',
    preview='EdgeWarning',
    height=120,
)

# ════════════════════════════════════════════════════════════════════════════
# LAYOUTS — how parts are arranged on the 390 × 844 board
# ════════════════════════════════════════════════════════════════════════════

e(
    id='root-layout',
    tier='layout',
    name='RootLayout',
    unity=['vb-root', 'SafeAreaRoot', 'VeilbreakPortrait.asset'],
    status='shipped',
    summary='The panel root: a full-size column inside the device safe area, carrying the accessibility root classes.',
    blocks=['vb-root'],
    doc='SafeAreaRoot',
    csharp='root.AddToClassList("vb-root");\nvar safe = new SafeAreaRoot(root, panelSettings, screenLayer);   // also primes fonts\nvoid Update() { safe.Refresh(); safe.Flags(s.largeText, s.reducedMotion, s.contrast, s.reducedFlashes); }',
    rules=[
        'Reference 390 × 844, match width (`VeilbreakPortrait.asset`). Insets are panel units with 48 top / 34 bottom minimums.',
        'Root classes: `vb-compact` (< 380 wide), `vb-large-text`, `vb-reduced-motion`, `vb-reduced-flashes`, `vb-contrast`. Popovers copy them.',
    ],
    preview=r"""render(board(zone('SAFE TOP · ≥ 48', { position: 'absolute', left: 0, right: 0, top: -48, height: 48 }), zone('vb-root · 390 × 762 here', { position: 'absolute', left: 8, right: 8, top: 8, bottom: 8 }), zone('SAFE BOTTOM · ≥ 34', { position: 'absolute', left: 0, right: 0, bottom: -34, height: 34 })));""",
    stage='board',
)

e(
    id='screen-layout',
    tier='layout',
    name='ScreenLayout',
    unity=['Navigator', 'vb-screen', 'vb-page'],
    status='shipped',
    summary='One page at a time on the screen layer: optional header, a scroll body, a footer.',
    uses=['screen-header'],
    blocks=['vb-screen', 'vb-page'],
    doc='Navigator',
    csharp='var page = nav.Open("settings", back: Leave);   // vb-screen--settings > vb-page--settings\nvar header = new ScreenHeader("SETTINGS", "SAVED").Into(page); page.AddToClassList("vb-page--headed");\nvar body = Ui.Scroll(page, "vb-page__scroll").contentContainer;',
    rules=[
        'Pages are 16 wide gutters; headed pages span the header and keep a 12 gutter.',
        'Back order: popover → modal → page back → fallback. `nav.Blocked` freezes it.',
    ],
    preview=r"""render(board(h('div', { className: 'vb-screen' }, h('div', { className: 'vb-page vb-page--headed' },
  h(V.ScreenHeader, { title: 'SETTINGS', meta: 'SAVED' }),
  zone('vb-page__scroll · flex-grow', { flexGrow: 1, marginTop: 8 }),
  zone('vb-page__footer', { height: 60, marginTop: 8 })))));""",
    stage='board',
)

e(
    id='action-row',
    tier='layout',
    name='ActionRowLayout',
    unity=['ActionRow', 'Ui.Actions'],
    status='shipped',
    summary='Buttons side by side that fill the row, an equal share each; the row stacks into a column when they no longer fit.',
    uses=['button', 'button-with-icon'],
    blocks=['vb-action-row'],
    doc='ActionRow.Single and the icon-only button',
    csharp='Ui.Actions(sheet, new VbButton("ABANDON", ButtonVariant.Danger), new VbButton("KEEP PLAYING"));   // equal widths, stacked below 112 each\nvar menu = Ui.Actions(page, scores, cogButton); menu.Single = true;   // one line at any width; the cog keeps its 48-unit square\nnew ActionRow { Fixed = true };   // trailing BUY and SELL line up down a list',
    rules=[
        "Every row of actions is an ActionRow: the results row, the menu row, confirm and guide dialogs, the drawer foot, a list row's trail.",
        '`Single` is for secondary buttons that share one line (EXIT TO MENU, ABANDON and the cog): their glyphs drop and their labels may wrap. `Fixed` keeps one width and never stacks.',
    ],
    preview='ActionRow',
    height=330,
)

e(
    id='stack-layout',
    tier='layout',
    name='StackLayout',
    unity=['StackLayout', 'Ui.Stack', 'vb-stack', 'vb-grow'],
    status='shipped',
    summary='Children in a column with one rhythm (4, 8, 12 or 16 under every child but the last); `Ui.Spacer` pushes the rest to the end.',
    blocks=['vb-stack', 'vb-grow'],
    csharp='var steps = Ui.Stack(sheet, 8);                 // vb-stack vb-stack--8\nforeach (var s in lines) Ui.Text(steps, s, TypeStyle.Body);\nUi.Spacer(page);                                  // vb-grow: push the CTA to the bottom',
    uxml='<vb:StackLayout gap="12">\n  <vb:CardComponent title="Checkpoint" />\n  <vb:CardComponent title="Last run" />\n</vb:StackLayout>',
    rules=[
        'Gaps come from the spacing tokens (4, 8, 12, 16). UI Toolkit has no `gap` or `:last-child`: StackLayout marks its last child `vb-stack__last` as children change.',
        'Stacks replace per-element margins; one rhythm per stack.',
    ],
    preview='StackLayout',
)

e(
    id='row-layout',
    tier='layout',
    name='RowLayout',
    unity=['vb-row', 'vb-row--between', 'vb-row--wrap'],
    status='shipped',
    summary='Children side by side; between pushes them apart, wrap lets chips flow.',
    blocks=['vb-row'],
    doc='Layout classes (`Layout.uss`)',
    csharp='var line = Ui.Box(score, "vb-row", "vb-row--between");\nUi.Text(line, "Wave clears", "vb-t-caption"); Ui.Text(line, "1,200", "vb-t-num");',
    preview=r"""render(col([h('div', { className: 'vb-row vb-row--between', style: { width: 340 } }, h('div', { className: 'vb-l vb-t-caption' }, 'Wave clears · 100 × wave'), h('div', { className: 'vb-l vb-t-num' }, '1,200')), h('div', { className: 'vb-row vb-row--wrap', style: { width: 340, gap: 6 } }, ['Sharpened Output ×2', 'Momentum Tax', 'Overclock', 'Barrier Mesh'].map(function (t, i) { return h(V.VbFrame, { key: i, className: 'vb-frame--slim vb-frame--raised', style: { flexDirection: 'row', alignItems: 'center', height: 22, padding: '0 8px' } }, h('div', { className: 'vb-l vb-t-badge' }, t.toUpperCase())); }))], 14, { alignItems: 'flex-start' }));""",
    height=130,
)

e(
    id='grid-layout',
    tier='layout',
    name='GridLayout',
    unity=['VbGrid', 'Ui.Grid', 'vb-grid', 'vb-grid4', 'vb-grid2'],
    status='shipped',
    summary='Cells 2, 3 or 4 across: slots, tiles and result cells take four, perk drafts three, chips and stats two.',
    blocks=['vb-grid', 'vb-grid4', 'vb-grid2'],
    doc='Layout classes (`Layout.uss`)',
    csharp='var slots = Ui.Grid(page, 4);\nvar roster = Ui.Grid(page, 4, wrap: true);\nvar draft = Ui.Grid(page, 3);\nvar perks = Ui.Box(page, "vb-grid2");   // two columns, end cells right-aligned',
    uxml='<vb:VbGrid columns="3" wrap="true" />',
    rules=[
        'Pick the column count the items fill: 4 operatives → 4 across; never a half-empty row.',
        '`vb-compact` boards keep the count; cells shrink, text drops a line.',
        '`vb-grid4` / `vb-grid4--wrap` stay as the four-across shorthand.',
    ],
    preview='VbGrid',
)

e(
    id='list-layout',
    tier='layout',
    name='ListLayout',
    unity=['ListLayout', 'Ui.List', 'vb-list'],
    status='shipped',
    summary='Rows of the same kind in a scroll: scores, perks, rules, queue entries. 6 between rows, and one empty-state line.',
    uses=['score-row', 'perk-row'],
    blocks=['vb-list'],
    csharp='var list = Ui.List(page, "No runs recorded yet. The boulevard is waiting.");\nforeach (var e in entries) new ScoreRow().Into(list).Bind(e);   // the empty line hides itself',
    uxml='<vb:ListLayout empty="No runs yet." />',
    rules=[
        'Rows carry no margin of their own (ScoreRow, PerkRow and RuleRow lost theirs in 0.3.0): the list owns the 6-unit rhythm.',
        'Rows are slim; a selected row is the only one with an edge.',
        'An empty list says what will appear and how.',
    ],
    preview='ListLayout',
)

e(
    id='split-layout',
    tier='layout',
    name='SplitLayout',
    unity=['vb-half'],
    status='shipped',
    summary='Two equal cells in a row, 5 apart. Paired secondary actions.',
    blocks=['vb-half'],
    doc='Layout classes (`Layout.uss`)',
    csharp='var row = Ui.Box(actions, "vb-row");\nnew VbButton("HIGH SCORES").Classes("vb-half").Into(row);\nnew VbButton("SETTINGS", icon: "settings").Classes("vb-half").Into(row);',
    preview=r"""render(h('div', { className: 'vb-row', style: { width: 358 } }, h(V.VbButton, { title: 'HIGH SCORES', className: 'vb-half' }), h(V.VbButton, { title: 'SETTINGS', icon: 'settings', className: 'vb-half' })));""",
    height=80,
)

e(
    id='sheet-layout',
    tier='layout',
    name='SheetLayout',
    unity=['vb-modal', 'Dialog.Sheet'],
    status='shipped',
    summary='A bottom-docked sheet over the page: the thumb reaches every action.',
    uses=['overlay', 'dialog'],
    blocks=['vb-modal', 'vb-dialog'],
    csharp='var sheet = Dialog.Sheet(page, null, null, null, "vb-pause__sheet");   // docked by Ui.Spacer above it',
    preview=r"""render(board(h('div', { className: 'vb-screen' }, h('div', { className: 'vb-page' }, zone('page content', { height: 220, marginTop: 40 }), h('div', { className: 'vb-grow' }), h(V.Dialog, { title: 'END THIS RUN?', body: 'Your current run will be recorded and its checkpoint removed.' }, h(V.VbButton, { title: 'ABANDON', variant: 'danger', icon: 'remove' }), h(V.VbButton, { title: 'KEEP PLAYING', icon: 'chevron' }))))));""",
    stage='board',
)

e(
    id='board-layout',
    tier='layout',
    name='BoardLayout',
    unity=['BoardLayout', 'vb-board', 'WorldLayer'],
    status='shipped',
    summary='The battle board: HUD band on top, the world in the middle, Power and the four operative cards at the bottom.',
    uses=['hud-band', 'unit-card', 'power-strip'],
    blocks=['vb-board'],
    csharp='var board = new BoardLayout().Into(root);\nnew HudTop().Into(board.Top);\nvar world = new WorldLayer { Project = Project }.Into(board.World);\nforeach (var a in run.Allies) new OperativeCard().Into(board.Bottom);',
    rules=[
        'At least half of the board stays world. Top and bottom never shrink; the world takes the rest.',
        'The camera frames fighters in the band the HUD leaves free; a new HUD panel must not cut into it.',
        'BattleHudView is this layout, filled.',
    ],
    preview='BoardLayout',
)

e(
    id='world-layer',
    tier='layout',
    name='WorldLayerLayout',
    unity=['WorldLayer'],
    status='shipped',
    summary='The world-space HUD over the 3D view: bars, labels, markers and edge chips, advanced on simulation time.',
    uses=['world-meter', 'combat-text', 'world-label', 'marker', 'offscreen'],
    blocks=['vb-world'],
    doc='WorldLayer',
    csharp='layer.BeginFrame();\nforeach (var a in allies) layer.Bar(a.id, a.head, a.hp01, a.barrier01, WorldTeam.Ally);\nlayer.EndFrame(simDelta);',
    preview='BattleHUD',
    stage='board',
)

# ════════════════════════════════════════════════════════════════════════════
# VIEWS — whole screens and screen sections (Runtime/Views), with plain view models and events
# ════════════════════════════════════════════════════════════════════════════


e(
    id='main-menu',
    tier='view',
    name='MainMenuView',
    unity=['MainMenuView', 'MainMenuViewModel'],
    status='shipped',
    summary='The title screen: offline badge, best score and the EXIT icon button, the mark, then the menu actions.',
    uses=['root-layout', 'screen-layout', 'mark', 'badge', 'menu-actions', 'button'],
    blocks=['vb-menu'],
    csharp='var menu = new MainMenuView().Into(nav.Open("menu"));\nmenu.Bind(new MainMenuViewModel { best = best.score, bestWave = best.wave, checkpoint = cp, squad = names });\nmenu.Play += Squad; menu.Resume += app.Resume; menu.Scores += Scores; menu.Settings += Settings; menu.Exit += ExitConfirm;',
    rules=[
        'PLAY is the one primary. EXIT is an icon button in the header, top-right, away from the thumb.',
        'HIGH SCORES and SETTINGS carry their own glyphs; settings is the icon-only cog at the end of the one row.',
        'The best badge only shows once there is a score.',
    ],
    preview='MainMenuView',
    stage='board',
)

e(
    id='menu-actions',
    tier='view',
    name='MenuActionsSectionView',
    unity=['MenuActionsSectionView', 'CheckpointView'],
    status='shipped',
    summary='The thumb-zone block of the menu: the checkpoint card when a run can resume, PLAY, then one row: HIGH SCORES filling it and the settings cog.',
    uses=['card', 'button', 'button-with-icon', 'split-layout', 'text'],
    blocks=['vb-menu-actions', 'vb-menu__checkpoint', 'vb-menu__play', 'vb-menu__row'],
    csharp='var actions = new MenuActionsSectionView().Into(page);\nactions.Bind(checkpoint: cp, squadLine: "Squad · Aria · Sol · Aegis · Nyra", notice: app.Notice);\nactions.Play += Squad; actions.Resume += app.Resume;',
    rules=[
        "PLAY is the screen's one primary. RESUME is secondary even though it continues a run: it lives inside the checkpoint card.",
        'The checkpoint card only exists when there is a checkpoint; nothing else moves when it appears.',
    ],
    preview='MenuActionsSectionView',
    height=300,
)

e(
    id='squad-view',
    tier='view',
    name='SquadView',
    unity=['SquadView', 'SquadViewModel'],
    status='shipped',
    summary='Pick four: the squad slots on top (drag, long-press or Alt+arrow to reorder), the roster grid below, details on tap, START RUN blocked until four.',
    uses=['screen-layout', 'screen-header', 'grid-layout', 'unit-slot', 'unit-tile', 'unit-detail', 'drawer', 'label'],
    blocks=['vb-squad-view'],
    csharp='var squad = new SquadView().Into(nav.Open("squad", MainMenu));\nsquad.Bind(new SquadViewModel { roster = roster, selected = picks, focus = focus, locked = locks });\nsquad.Picked += i => { focus = i; Rebind(); }; squad.SlotMoved += (from, to) => { Reorder(from, to); Rebind(); }; squad.Toggled += ToggleInSquad; squad.Start += StartRun; squad.Back += MainMenu;',
    rules=[
        "Selection is the game's: the view raises events, the game binds again.",
        'Details open in a bottom drawer over the roster (`DetailsDrawer`); dismissing it raises `Closed`.',
        'START RUN says why it is blocked ("Pick 2 more").',
    ],
    preview='SquadView',
    stage='board',
)

e(
    id='settings-view',
    tier='view',
    name='SettingsView',
    unity=['SettingsView', 'SettingsViewModel'],
    status='shipped',
    summary='Audio, feel, camera, quality, accessibility and data as fieldsets in one form; applies as it changes.',
    uses=['screen-layout', 'screen-header', 'form', 'fieldset', 'field', 'button'],
    blocks=['vb-settings', 'vb-form', 'vb-section'],
    csharp='var view = new SettingsView().Into(nav.Open("settings", Leave));\nview.Bind(app.Profile.settings);              // writes back into the same model\nview.Changed += s => { app.ApplySettings(); app.SaveProfile(); };\nview.ClearScores += ClearScoresConfirm; view.Accessibility += Accessibility; view.Back += Leave;',
    rules=[
        'Every container here is a slim fieldset.',
        'Accessibility carries reduced motion and a row to the display options; Data carries CLEAR LOCAL SCORES, a danger row that confirms. Nothing pins to the foot of the page.',
    ],
    preview='SettingsView',
    stage='board',
)

e(
    id='pause-overlay',
    tier='view',
    name='PauseOverlayView',
    unity=['PauseOverlayView', 'PauseViewModel'],
    status='shipped',
    summary='The run, frozen: the screen header without a back arrow, then one overlay panel centred on the screen with wave, score, standing, perks, resume and one row of the exits.',
    uses=[
        'overlay',
        'screen-header',
        'icon-button',
        'dialog',
        'stat-readout',
        'badge',
        'button',
        'button-with-icon',
        'split-layout',
        'text',
    ],
    blocks=['vb-pause'],
    csharp='var pause = new PauseOverlayView().Into(nav.Open("pause", Resume));\npause.Bind(new PauseViewModel { wave = run.Wave, score = run.Score, standing = alive, perks = build, seed = run.Seed });\npause.Resume += Resume;   // RESUME and the header\'s back square\npause.Settings += Settings; pause.Perks += ActivePerks;\npause.Abandon += AbandonConfirm; pause.Exit += ExitConfirm;',
    rules=[
        'There is no back arrow: RESUME is the one way back (the key or gesture also resumes). The title and its eyebrow reveal with the glitch (`vb-reveal-title`) and the sheet enters with `OverlayIn`.',
        'RESUME is the one primary. One single `ActionRow` holds the rest: EXIT TO MENU (led by its ‹), ABANDON (danger, confirms) and the settings cog (icon only). The seed and rules line is behind an info button that opens a popover.',
        'The panel is centred in the screen under the header, not docked: pausing is a stop, not a detail.',
        'Active perks are plain badges in a slim, tappable panel.',
    ],
    preview='PauseOverlayView',
    stage='board',
)

e(
    id='confirm-view',
    tier='view',
    name='ConfirmDialogView',
    unity=['ConfirmDialogView'],
    status='shipped',
    summary='A question, its consequence, the destructive action and the way back. One shape for every confirmation.',
    uses=['sheet-layout', 'dialog', 'button'],
    blocks=['vb-confirm', 'vb-dialog'],
    csharp='ConfirmDialogView.Push(nav, "END THIS RUN?", "Your current run will be recorded and its checkpoint removed.",\n    confirm: ("ABANDON", "remove", Abandon), cancel: ("KEEP PLAYING", "chevron"), confirmSub: "Record the score and end the run");',
    rules=[
        'The destructive action is danger and first; the way back is secondary and says where it goes. Both sit in one `ActionRow`: equal widths, stacked on a narrow sheet.',
        'Both actions close the modal layer before they run; Back closes it too.',
    ],
    preview='ConfirmDialogView',
    stage='board',
)

e(
    id='perk-draft',
    tier='view',
    name='PerkDraftView',
    unity=['PerkDraftView', 'PerkDraftViewModel', 'SquadHealthView'],
    status='shipped',
    summary='Wave cleared: the perk choices as an endless carousel of choice cards, the squad that carries forward, TAKE PERK.',
    uses=['screen-layout', 'carousel', 'choice-card', 'grid-layout', 'health-bar', 'button'],
    blocks=['vb-draft'],
    csharp='var draft = new PerkDraftView().Into(nav.Open("perks"));\ndraft.Bind(new PerkDraftViewModel { wave = run.Wave, bonus = bonus, score = run.Score, choices = offer, pick = pick, squad = health });\ndraft.Picked += i => { pick = i; Rebind(); };   // the carousel changed its active card\ndraft.Take += TakePerk;',
    rules=['TAKE PERK says what the pick does next.', 'No healing between waves: the squad panel says so.'],
    preview='PerkDraftView',
    stage='board',
)

e(
    id='results-view',
    tier='view',
    name='ResultsView',
    unity=['ResultsView', 'ResultsViewModel', 'LedgerLine'],
    status='shipped',
    summary="Run over: the score and how it was made, the squad's contribution, the perks, and what next.",
    uses=['screen-layout', 'card', 'stat-readout', 'grid-layout', 'result-summary', 'chip-row', 'button'],
    blocks=['vb-results'],
    csharp='var results = new ResultsView().Into(nav.Open("results"));\nresults.Bind(new ResultsViewModel { score = run.Score, wave = run.Wave, rank = rank, ledger = run.Ledger, squad = contributions, perks = build });\nresults.Again += Again; results.Squad += Squad; results.Menu += MainMenu;',
    rules=[
        'RUN AGAIN is the one primary; MENU, SQUAD and RUN AGAIN share one row, which stacks on a narrow board.',
        'Contribution is on hover, focus or tap of each squad cell; a unit still standing says nothing.',
    ],
    preview='ResultsView',
    stage='board',
)

e(
    id='scores-view',
    tier='view',
    name='HighScoresView',
    unity=['HighScoresView', 'ScoreView'],
    status='shipped',
    summary='Top 20 on this device, the current run framed, and older-rules runs below.',
    uses=['screen-layout', 'screen-header', 'list-layout', 'score-row', 'text'],
    blocks=['vb-scores'],
    csharp='var scores = new HighScoresView().Into(nav.Open("scores", MainMenu));\nscores.Bind(ranked, historical);   // at most 20; older rules after a label\nscores.Back += MainMenu;',
    rules=['Local scores only.', 'The current run is framed (ScoreView.current).'],
    preview='HighScoresView',
    stage='board',
)

e(
    id='route-view',
    tier='view',
    name='RouteChoiceView',
    unity=['RouteChoiceView', 'RouteChoice', 'RouteView'],
    status='shipped',
    summary='The crossroads between waves: two choice cards, one choice.',
    uses=['glitch-text', 'choice-card', 'icon', 'text'],
    blocks=['vb-route-view', 'vb-route'],
    csharp='var route = new RouteChoiceView().Into(nav.Open("route"));\nroute.Bind(new RouteView { wave = 3, left = combat, right = merchant });\nroute.Chosen += side => run.ChooseRoad(side);',
    rules=[
        'Only one choice is accepted; the cards lock after it.',
        'The pair mirrors: the left card is cut and aligned to the left, the right one to the right.',
    ],
    doc='Projection and the road economy',
    preview='RouteChoiceView',
    stage='board',
)

e(
    id='merchant-view',
    tier='view',
    name='MerchantView',
    unity=['MerchantScreenView', 'MerchantScreen', 'MerchantView'],
    status='shipped',
    summary='The night market: one list of plated stacked rows (rare finds, stack sales, health trades), each with its effect or health bar and one 44-unit action; what cannot be bought is dimmed.',
    uses=['stat-row', 'progress', 'button', 'chip-row', 'list-layout', 'label'],
    blocks=['vb-merchant-view', 'vb-merchant'],
    csharp='var market = new MerchantScreenView().Into(nav.Open("merchant"));\nmarket.Bind(view);   // keeps the scroll position across confirmed trades\nmarket.Bought += Buy; market.PerkSold += SellStack; market.HealthSold += SellHp; market.Left += Leave;',
    rules=[
        'The host confirms every trade, then binds again.',
        'MerchantView is the view model; the screen type is MerchantScreenView.',
    ],
    doc='Projection and the road economy',
    preview='MerchantScreenView',
    stage='board',
)

e(
    id='battle-hud-view',
    tier='view',
    name='BattleHudView',
    unity=['BattleHudView', 'BoardLayout', 'Announcer'],
    status='shipped',
    summary='The whole battle board: HUD band, boss bar, announcer, world layer, Power and four operative cards.',
    uses=['board-layout', 'hud-band', 'boss-bar', 'world-layer', 'power-strip', 'unit-card', 'glitch-text'],
    blocks=['vb-battle', 'vb-board'],
    csharp='var battle = new BattleHudView().Into(root);\nvar world = new WorldLayer { Project = Project }.Into(battle.World);\nbattle.Bind(hudView, operatives, boss);   // every frame; boss null hides the bar\nbattle.Pause += Pause; battle.QueueCancel += q => run.CancelQueued(q.actor, q.slot);\nbattle.Card(i).Skill(0).AddManipulator(aimDrag); battle.Announcer.Say("WAVE 08");',
    rules=[
        'The game owns input: attach aim and move manipulators to the cards.',
        'Feed the announcer confirmed events, not per-frame strings.',
    ],
    preview='BattleHudView',
    stage='board',
)

e(
    id='guide-view',
    tier='view',
    name='GuideDialogView',
    unity=['GuideDialogView'],
    status='shipped',
    summary='The first-run guide: how to move, aim, cancel and when to use autoplay, then READY.',
    uses=['overlay', 'dialog', 'button'],
    blocks=['vb-guide'],
    csharp='GuideDialogView.Push(nav, "RUN THE BOULEVARD", GuideDialogView.DefaultSteps, ready: () => { settings.tutorialSeen = true; Resume(); });',
    rules=['Numbered steps, one sentence each; READY is the one primary.'],
    preview='GuideDialogView',
    stage='board',
)

e(
    id='effect-details',
    tier='composite',
    name='EffectDetails',
    unity=['EffectDetails'],
    status='shipped',
    summary='Shared effect meaning, value, source, duration and activation.',
    family='effects',
    uses=[],
    blocks=['vb-effect-details'],
    csharp='new EffectDetails();',
    preview='EffectDetails',
    height=320,
)
e(
    id='effect-badge',
    tier='atom',
    name='EffectBadge',
    unity=['EffectBadge'],
    status='shipped',
    summary='Inspectable touch target for a state modifier.',
    family='effects',
    uses=[],
    blocks=['vb-effect-badge'],
    csharp='new EffectBadge();',
    preview='EffectBadge',
    height=200,
)
e(
    id='effect-list',
    tier='composite',
    name='EffectList',
    unity=['EffectList'],
    status='shipped',
    summary='Every modifier, individually inspectable without truncation.',
    family='effects',
    uses=[],
    blocks=['vb-effect-list'],
    csharp='new EffectList();',
    preview='EffectList',
    height=320,
)
e(
    id='effect-strip',
    tier='composite',
    name='EffectStrip',
    unity=['EffectStrip'],
    status='shipped',
    summary='Compact battle status strip with complete overflow.',
    family='effects',
    uses=[],
    blocks=['vb-effect-strip'],
    csharp='new EffectStrip();',
    preview='EffectStrip',
    height=200,
)
e(
    id='active-perks-view',
    tier='view',
    name='ActivePerksView',
    unity=['ActivePerksView'],
    status='shipped',
    summary='Owned perk stacks, category totals and selected explanation.',
    family='screens',
    uses=[],
    blocks=['vb-active-perks'],
    csharp='new ActivePerksView();',
    preview='ActivePerksView',
    height=844,
)
e(
    id='revive-pick-view',
    tier='view',
    name='RevivePickView',
    unity=['RevivePickView'],
    status='shipped',
    summary='Choose one fallen operative to restore.',
    family='screens',
    uses=[],
    blocks=['vb-revive-pick'],
    csharp='new RevivePickView();',
    preview='RevivePickView',
    height=844,
)
e(
    id='accessibility-view',
    tier='view',
    name='AccessibilityView',
    unity=['AccessibilityView'],
    status='shipped',
    summary='Shared accessibility controls and saved state.',
    family='screens',
    uses=[],
    blocks=['vb-accessibility'],
    csharp='new AccessibilityView();',
    preview='AccessibilityView',
    height=844,
)

TIERS = [
    ('atom', 'Atoms', 'Base components. The smallest parts with a look and a contract; atoms may use other atoms.'),
    ('composite', 'Composites', 'Atoms combined into one reusable part with its own view model and events.'),
    (
        'layout',
        'Layouts',
        'How parts are arranged on the 390 × 844 board: roots, pages, stacks, rows, grids, lists, sheets.',
    ),
    (
        'view',
        'Views',
        'Whole screens and named screen sections, each with a plain view model and events: the game binds data and handles the events.',
    ),
]
