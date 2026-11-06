"""Component READMEs and index.d.ts types for the kit (com.veilbreak.gui), plus the legacy web kit's types.

python3 docs.py OUT/project legacy/index.d.ts LEGACY_KEEP...
"""

import os
import pathlib
import sys

OUT = sys.argv[1]
SRC = 'com.veilbreak.gui'


def doc(name, cs, summary, provide, rules):
    body = f'# {name}\n\n{summary}\n\n**Provide** {provide}\n\n' + ''.join(f'- {r}\n' for r in rules)
    body += (
        f'\nPorted from `{cs}` in the Unity package `{SRC}`, the source of this system: the same names, element tree and'
        ' USS classes. Render inside an element with class `vb-root`. The Unity API (Bind, states, USS hooks,'
        f" accessibility) is in the package's `Documentation~/components.md`.\n"
    )
    d = os.path.join(OUT, 'components', name)
    os.makedirs(d, exist_ok=True)
    pathlib.Path(d, 'README.md').write_text(body)


doc(
    'GlitchText',
    'Runtime/Controls/GlitchText.cs',
    'A short red/cyan projection transition for heads-up text.',
    '`text`, `effect` (in, out or slam), `reveal` (true or milliseconds: the 2 s title reveal on mount, `Glitch.reveal`), `className`.',
    [
        'The ordinary text remains readable; decorative copies are hidden from accessibility.',
        'Reduced Motion and Reduced Flashes use a fade. Unity also provides Glitch.In/Out/Pulse and a bounded Announcer queue.',
    ],
)
doc(
    'RouteChoice',
    'Runtime/Composites/RouteMerchant.cs',
    "Two roads as a mirrored pair of ChoiceCards: glyph, title, hint, the road's threat as rank cells and TAKE THIS ROAD.",
    '`view` (wave, left, right RoadView), `onChoose(side)`.',
    [
        'Left is 0 and right is 1; only one choice is accepted.',
        "The whole card is the keyboard and touch target; the tone follows the road's kind (elite and boss read as danger, a merchant as power, a fight as ally).",
    ],
)
doc(
    'MerchantScreen',
    'Runtime/Composites/RouteMerchant.cs',
    'A market: priced upgrades, stack sales and explicit HP trades, each a plated stacked StatRow with a 44-unit action.',
    '`view` (credits, offers, stacks, health), `onBuy(index)`, `onSellPerk(id)`, `onSellHealth(actor)`, `onLeave`.',
    [
        'The host validates every transaction and updates the view.',
        'An offer that cannot be bought is dimmed with no second price; a section with nothing in it does not show; a sale pays in credits (+30).',
        "Rare is cyan and Epic is violet, as a tint on the row's glyph.",
    ],
)

doc(
    'VbFrame',
    'Runtime/Core/VbShape.cs',
    'A cut-corner panel: the Veil shape (top-left and bottom-right chamfers) painted from `--vb-*` CSS custom properties.',
    '`className` (the look) and `children`. Panel looks: `vb-panel`, plus `vb-panel--solid` (sheets, popovers), `--raised`, `--glass`, `--enemy`.',
    [
        'Shape properties are unitless numbers or colours: `--vb-cut` (px) `--vb-fill` [`--vb-fill-to` `--vb-fill-dir` (deg, 0 = left→right)] `--vb-edge` [`--vb-edge-to`] `--vb-bw` (px) `--vb-dash` (1 = square dashed frame, the blocked/queued look) `--vb-bracket` [`--vb-bracket-len`] (L marks on the square corners) `--vb-glow`.',
        'A glow bleeds outside the box, so it shows only on shapes without `overflow: hidden` (the primary button); cards with `overflow: hidden` clip it.',
        'Restyle a shape by giving a variant class its own `--vb-*` values; `.vb-shape` resets them, so never rely on inheriting them.',
        'Content and hit area stay the full rectangle; content is not clipped to the chamfer.',
        '`window.Veilbreak.VbShape` (`as`, `className`, children) paints any element this way; every kit component already uses it.',
    ],
)
doc(
    'VbIcon',
    'Runtime/Controls/Basics.cs (VbIcon)',
    "One of the kit's 99 stroke glyphs (24 grid, 1.6 stroke, square caps), chosen by name or by one of 8 design-system aliases, and coloured by CSS `color`.",
    '`icon` (kebab or PascalCase file name: `automation`, `SkillAriaNeedleStorm` → `skill-aria-needle-storm`), `size` (px, default 16), optional `className`, `title`.',
    [
        'Names: `window.Veilbreak.VbIcon.names`. Families: ability shapes (projectile, fan, cone, burst, zone, beam, dash, wall), roles (striker, breaker, bastion, operator), statuses (exposed, silence, root, slow, barrier, warning), HUD controls and one glyph per operative skill (`skill-<operative>-<skill>`).',
        'Tint with `color` (the kit uses `var(--text)`, `--text-2`, `--ally-text`, `--power-text`, `--warn`, `--on-cta`).',
        'An icon travels with a word, except inside a skill button and the HUD pause/back controls (those carry `title`/`aria-label`).',
    ],
)
doc(
    'VbMark',
    'Runtime/Controls/Basics.cs (VbMark)',
    'The slash-cut VEIL/BREAK mark with its pink and teal ghosts, drawn for dark grounds.',
    '`width` (px, default 260); height follows the 513 × 259 artwork.',
    [
        'Use on the main menu and splash only, on `ground` or a dark scene.',
        "Never recolour or redraw it; it is the package's `Runtime/Vectors/Mark.svg` inlined (MarkGeometry in C#).",
    ],
)
doc(
    'VbButton',
    'Runtime/Controls/Basics.cs (VbButton)',
    'Cut-corner button in five variants; the pink→teal primary is the one CTA per screen, and a blocked button stays legible and says why.',
    '`title` (in caps: the button does not upper-case it), `variant` (`primary`, `secondary` default, `light`, `danger`, `ghost`), `sub` (second line), `icon` (`chevron`/`play` draw at 12 px, others at 20), `blocked` (reason under five words; disables and replaces `sub`), `iconOnly` (a 48 px square with the glyph alone; `title` stays the accessible name and tooltip), `lead` (the glyph before the label; `back` and `chevron-left` lead by themselves), `dimmed` (dims and disables with no reason shown), `compact` (drops the glyph; `ActionRow` single sets it), `onClick`.',
    [
        'One `primary` per screen (START RUN, TAKE PERK). Its sub line states what will happen: "Aria · Rook · Aegis · Nyra · new seed".',
        '`light` confirms inside a sheet (ADD TO SQUAD); `danger` for removal (REMOVE FROM SQUAD); `secondary` and `danger` put the icon first, others after the label.',
        'Blocked = square dashed frame with the reason in `enemy-text` ("Pick 1 more to start", "Squad full · remove one first"). Never just dim.',
        "Min height 48 (primary 60); press scales to .97; focus shows white brackets. A click runs the activation edge (`VbEdgeFx`): two heads run clockwise round the cut outline in the button's own edge colours.",
        'A forward action ends with its chevron; an action that goes back or leaves leads with its glyph. The settings button is the `cog` glyph with `iconOnly`.',
    ],
)
doc(
    'VbSwitch',
    'Runtime/Controls/Fields.cs (VbSwitch)',
    'Square on/off switch with a caps label and a line saying what it does.',
    '`text` (caps label), `detail` (what it changes), `value`, `onChange(bool)`.',
    [
        'On = teal track with a dark knob at the right; off = raised track, grey knob.',
        'Every switch carries its consequence in `detail`.',
    ],
)
doc(
    'VbSlider',
    'Runtime/Controls/Fields.cs (VbSlider)',
    'Slider in Veil dress: a 14 px track with a teal fill, a square white thumb and the value as 0–100.',
    '`label` (sentence case, 72 px column), `value`, `min` (0), `max` (1), `onChange(number)`.',
    [
        'Volumes and intensities in Settings. The readout is always the percentage of the range.',
        'Arrow keys step by 5%.',
    ],
)
doc(
    'VbSegmented',
    'Runtime/Controls/Fields.cs (VbSegmented)',
    '2–5 mutually exclusive peers in one cut frame; each option can carry its consequence.',
    '`items` (`[{label, sub?}]`, labels in caps), `selected` (index), `onChange(index)`.',
    [
        'The selected option is a white fill with dark text.',
        'Use the sub line for the consequence ("60 FPS target"), not a description.',
    ],
)
doc(
    'SkillButton',
    'Runtime/Controls/SkillButton.cs',
    'Compact 44 × 56 skill control for portrait cards in which every blocked state reads differently.',
    '`view`: a SkillView `{label, icon, slot ("A"|"S"), kind, cost, state, charged (0–1), seconds, need, queue, progress (0–1), reason}`. Optional `dragging` (aim drag in progress), `flash` (hold the refused flash for a still), `onPointerDown`.',
    [
        "One layout in every state: the slot letter, the glyph and one cost line. The states are overlays on it, so nothing runs below the slot's foot.",
        "`cooldown` fills from the bottom and counts its seconds over the glyph (`windup` and `silenced` count the same way); the charge and the windup sweep are `VbClipFill`s that follow the button's bottom-right cut.",
        '`short` dims the slot and puts the shortfall in the corner ("−3"); `queued` puts its place in line there ("#1"); `buffered` an arrow; `fallen` and `disabled` lock the glyph; `aiming` is a 2 px teal frame; `casting` has no caption of its own.',
        '`invalid` strikes the glyph and replaces the cost with the `reason`; `rejected` flashes crimson once for 400 ms; `exposed` inverts the cost.',
        'The game owns input; the button only draws. Its tooltip reads "Needle Storm · 18 Power · 40% ready".',
    ],
)
doc(
    'ActionRow',
    'Runtime/Layouts/Layouts.cs',
    'Buttons side by side that fill the row: every visible child takes an equal share with 8 between, and the row stacks into a column when its children no longer fit.',
    '`children` (buttons), `minItemWidth` (112), `fixed` (one fixed width, never stacks: the trailing BUY and SELL of a list row), `single` (one line at any width: buttons lose their glyph and may wrap their label, an `iconOnly` button keeps its 48 px square), `className`, `style`.',
    [
        "Use it for every row of actions: the results row, the menu row, confirm and guide dialogs, the drawer's foot, a list row's trail.",
        '`single` is for the secondary buttons of a menu or a sheet (EXIT TO MENU, ABANDON and the cog) where several share one line.',
    ],
)
doc(
    'StatusRow',
    'Runtime/Composites/Effects.cs',
    'The timed statuses of a unit as a row of tiny squares (about 20 × 20, glyph alone, stack count in the corner), at most six and a "+n" square.',
    '`effects` (EffectInstance[]; only the ones with `remainingSeconds` show), `pinned`, `className`.',
    [
        'The whole row is one target, 44 units tall through padding the layout does not take back; hover, focus or a tap opens one vertical `EffectList` in a popover.',
        'Passives, perks and effects without a time are not statuses and never show: a unit with none shows nothing.',
    ],
)
doc(
    'PopoverCard',
    'Runtime/Core/Popover.cs (PopoverCard)',
    "Popover content in the kit's layout: a caps title, an optional hint under it, then flat rows one tone above the popover's surface.",
    '`title`, `hint`, `rows` (`[{key, value, good?}]`), `children` (your own rows), `className`.',
    [
        'The popover itself is one solid surface with a painted arrow toward its anchor, placed again on every resize and kept inside the board; its height is clamped and the content scrolls past that.',
        "Rows carry no edge. A queue row's cancel goes at the row's end.",
    ],
)
doc(
    'VbClipFill',
    'Runtime/Core/VbClip.cs (VbClipFill)',
    "A solid block that follows the cut of the shape it sits in: it never paints outside the host's chamfers.",
    "`className`, `style`; the colour is `--vb-fill` (set it on the element's own class).",
    [
        'Its box is clipped to the outline of the nearest `VbShape` ancestor with `clip-path` (`--vb-cut`, `--vb-cut-side`); with no shaped ancestor it is a plain block.'
    ],
)
doc(
    'VbClipImage',
    'Runtime/Core/VbClip.cs (VbClipImage)',
    'A picture scaled to fit its box and clipped to the cut of the shape it sits in, so a portrait never overflows the top-left cutout of a card or tile.',
    '`url`, `className`, `style` (size and offset come from the stylesheet as for any art box).',
    ['Same clipping as `VbClipFill`.'],
)
doc(
    'VbEdgeFx',
    'Runtime/Core/VbEdgeFx.cs',
    'The animated border of the MONO wireframe kit on the cut outline of any shaped element: `play` is the activation, `damage` a unit card taking a hit.',
    '`VbEdgeFx.play(element, tone?)`, `.damage(element, strength?)`, `.cancel(element)`, `.active(element)`. Every `VbButton` plays it on click.',
    [
        "Activate: two bright heads run clockwise from opposite points and meet where the other began (520 ms), in the element's own edge colours, then an afterglow fades. Damage: a translucent red flash and a red border glow (560 ms).",
        'Reduced motion lights the whole edge and fades it in 100 ms; reduced flashes drop the flash fill. It paints one overlay (`vb-edgefx`) that is added when it plays and removed when it ends.',
    ],
)
doc(
    'Glitch',
    'Runtime/Controls/GlitchText.cs (Glitch)',
    'The title reveal: the text decodes out of noise glyphs left to right while it jitters sideways in 4 px steps that shrink to nothing and its opacity climbs in coarse steps (2000 ms).',
    '`Glitch.reveal(element, ms?)` (resolves when done), `Glitch.revealTitles(root)` (every `.vb-reveal-title`), `Glitch.scramble(text, reveal, step)`.',
    [
        'Titles and eyebrows of a pause sheet, a results screen and a draft carry `vb-reveal-title` and reveal when their view mounts. A heads-up notice can use the same recipe.',
        'Reduced motion or flashes: the whole text with a 300 ms fade.',
    ],
)
doc(
    'StatReadout',
    'Runtime/Controls/Basics.cs (StatReadout)',
    'A labelled number with an optional bar, for inspectors, results and pause.',
    '`label` (caps), `value` (preformatted string), `unit`, `bar` (0–1; omit for none), `delta` ("+12", "−4%"), `up` (true when lower is better and the delta still reads good), `tone` (`power`, `text`), `className` (`vb-stat--neutral` for a grey bar).',
    [
        'Teal bar for the stat that matters (health), neutral bars for the rest.',
    ],
)
doc(
    'StatusChip',
    'Runtime/Controls/Basics.cs (StatusChip)',
    'A status with its own glyph and word, so no two statuses read alike and none relies on colour.',
    '`kind` (`exposed`, `silenced`, `rooted`, `slowed`, `interrupted`, `barrier`), `text` (defaults to the kind), `detail` (after a middle dot), `time` (remaining, in Share Tech Mono: "2.4s").',
    [
        "Exposed is the inverted white chip; barrier's glyph is pale cyan; the rest are amber.",
    ],
)
doc(
    'VbMeter',
    'Runtime/Controls/Meters.cs (VbMeter)',
    'A flat track with a fill, an optional barrier span after it, a dashed cost preview and numbered queue marks.',
    '`value` (0–1), `extra` (span after the fill), `previewFrom`/`previewTo` (fractions; negative hides), `marks` (fractions), `className`.',
    [
        'The building block of HealthBar, PowerStrip and FocusMeter; style it through their classes (`vb-power__meter` for the lavender Power track).'
    ],
)
doc(
    'HealthBar',
    'Runtime/Controls/Meters.cs (HealthBar)',
    'Health over stability in team colour, with barrier after the fill; exposed paints stability crimson.',
    '`hp`, `max`, `barrier`, `stability` (0–1; omit to hide), `exposed`, `enemy`, `ghost` (health before the last hit, same units as `hp`; the game lowers it), `size` (`hud`, `world`, `large`), `caption` ("HP 820 / 1000").',
    [
        'Ally fill `ally`, enemy fill `enemy`, barrier `barrier` (pale cyan), stability `text-2`.',
    ],
)
doc(
    'PowerStrip',
    'Runtime/Controls/Meters.cs (PowerStrip)',
    'The shared Power pool: value, regen, the live consequence of an aim, a dashed cost preview and the next queue mark.',
    '`value`, `max`, `regen` ("+12/s"), `hint` (consequence while aiming: "Needle Storm −18 → 26 left"), `previewFrom`/`previewTo` (Power values), `marks` (fractions), `reserve` (the autoplay reserve line, in Power), `queue` (QueueView[]; up to four chips), `onCancel(entry)` (a manual chip\'s ×), `legend`.',
    [
        'Power is lavender and nothing else is.',
        'Manual casts go before autocasts: the queue shows them first in, first out, and only manual entries carry a ×.',
    ],
)
doc(
    'FocusMeter',
    'Runtime/Controls/Meters.cs (FocusMeter)',
    'Focus: drains in real time while targeting slows combat and regenerates outside it.',
    '`focus` (0–100), `slow`, `regenPerSecond` (12.5), `drainPerSecond` (25), `slowScale` (0.2).',
    ['Reads READY at 100, "+12.5/s" while refilling, "×0.2 · −25/s" while slowed (bright white).'],
)
doc(
    'HudTop',
    'Runtime/Composites/Hud.cs (HudTop)',
    'The translucent portrait battle band: wave/boss and score, AUTO toggle, speed, pause, then shared Power and focus.',
    '`view`: a HudView `{wave, score, boss, auto, speed, power, maxPower, regen, hint, focus, slow, previewFrom, previewTo, queue: [{who, skill, cost, eta, manual, next}], queueTitle, queueHint}`; `onAutoClick`, `onSpeedClick`, `onPauseClick`, `onQueueCancel(entry)`; `showFocus` (default true); `queuePinned` keeps the queue open.',
    [
        'Hover, focus or tap on the resources panel opens the QueueList popover; the first queue entry is marked on the Power track.',
        'The band is 35% ground: the arena stays the hero. Score compacts above 9,999 (12.5K) with the exact value as tooltip.',
        'Boss waves read BOSS in `enemy-text`.',
    ],
)
doc(
    'QueueList',
    'Runtime/Composites/Hud.cs (QueueList)',
    'Upcoming casts in firing order: the manual queue first, then autoplay estimates.',
    '`queue` (`[{who, skill, cost, eta, manual, next}]`), `title` ("AUTOPLAY QUEUE" or "MANUAL QUEUE"), `hint` ("Fires as Power reaches each cost"), `onCancel(entry)` (makes manual rows tappable).',
    [
        'The next cast has the solid lavender frame; a buffered one shows › for its place; costs in `power-text`.',
        'With `onCancel`, manual rows are buttons that cancel their cast (and carry a ×); autoplay estimates never are.',
        'Usually shown by HudTop as a popover, not placed directly.',
    ],
)
doc(
    'BossBar',
    'Runtime/Composites/Hud.cs (BossBar)',
    'Boss read-out: name, health over stability, and shield and interrupt markers.',
    '`view`: a BossView `{name, health (0–1), stability (0–1), detail, shielded, shieldDetail, interruptible}`.',
    ['Stability at 0 shows as exposed (crimson).', 'Chips appear only when shielded or interruptible.'],
)
doc(
    'ScreenHeader',
    'Runtime/Composites/Hud.cs (ScreenHeader)',
    'Screen header: a 44 px back control, the title in Adventor caps and a short meta read-out.',
    '`title` (upper-cased for you), `meta` ("4 / 4"), `accent` (meta in `ally-text` for good news), `backLabel`, `onBack`.',
    ["One per menu screen, at the top; the title is the screen's name, not a sentence."],
)
doc(
    'ScoreRow',
    'Runtime/Composites/Hud.cs (ScoreRow)',
    'One high-score line: rank, score, depth and squad codes; the current run is framed.',
    '`view`: a ScoreView `{rank, score, wave, bosses, squad: ["AR", …], current}`.',
    ['Top three ranks read in full white.', 'The current run gets the teal frame, brackets and THIS RUN badge.'],
)
doc(
    'UnitCard',
    'Runtime/Composites/Units.cs (UnitCard)',
    'The 88 × 202 portrait battle card: face, control mode, health/barrier/stability, two skills (or FALLEN once out) and the timed statuses as a StatusRow above it.',
    '`view`: an OperativeView `{name, kit (CSS colour, e.g. var(--kit-aria)), portrait (image URL), hp, maxHp, barrier, stability, maxStability, effects: EffectInstance[], mode ("auto"|"manual"|"aiming"|"fallen"), manualSeconds, canRevive, skills: [SkillView, SkillView]}`. Optional `autoDamage` (default true), `dragging` (skill index), `onSkillPointerDown(index, event)`.',
    [
        'The name is the only identifier. A mode chip shows only for MANUAL (with its seconds) and AIMING; a fallen card says FALLEN once and "Revive via perk" only while `canRevive`.',
        "The portrait and the band backdrop follow the card's top-left cut (VbClipImage and VbClipFill); the last skill takes the card's own bottom-right cut.",
        'A drop in `hp` plays the damage edge by itself (VbEdgeFx.Damage: a translucent red flash and a red border glow); a host that raises its own hits sets `autoDamage` false.',
        'Four cards fill a 390 px portrait row at the bottom of the battle screen.',
    ],
)
doc(
    'UnitTile',
    'Runtime/Composites/Units.cs (UnitTile)',
    'Roster tile: portrait, name, role and, when picked, its squad slot number.',
    '`view` (OperativeView: `name`, `role`, `roleIcon`, `kit`, `portrait`), `slot` (1–4, 0 = not in squad), `focused`, `tier` (`none`, `blue`, `violet`, `gold`), `locked` (how to get it: "Pull in rifts"), `onPick`.',
    [
        'In-squad tiles take the dark teal fill; the focused one a 2 px teal frame and brackets.',
        'Tier colours frame the tile (gold adds a glow); a locked tile stays visible with a lock and its reason, and cannot join the squad.',
        'A narrow board (`vb-compact`) shows the role as its icon alone.',
    ],
)
doc(
    'UnitSlot',
    'Runtime/Composites/Units.cs (UnitSlot)',
    'One of the four squad slots on the selection screen: the unit (its name alone), or a dashed empty slot.',
    '`view` (OperativeView or `null` for empty), `slot` (1–4), `onPick`.',
    [
        'Empty slots are square and dashed with SLOT n; filled ones show the slot number on the portrait.',
        'SquadView lets the player drag, long-press or Alt+arrow a filled slot to reorder the squad.',
    ],
)
doc(
    'UnitDetail',
    'Runtime/Composites/Units.cs (UnitDetail)',
    'The unit sheet: stats, then the passive, the basic attack and both skills (cost and cooldown only where there is one), the lore prompt, close and add/remove.',
    '`view` (OperativeView incl. `armor`, `passive`, `passiveIcon`, `passiveText`, `basic` (a SkillView), `lore`, skill `description`/`cooldownSeconds`), `slot` (0 = not in squad), `squadFull`, `showStability` (false hides it), `maxHp`/`maxArmor`/`maxStability` (bar scales: 1580, 26, 140), `onClose`, `onToggle`.',
    [
        'The toggle is ADD TO SQUAD (light) or REMOVE FROM SQUAD (danger); adding to a full squad is blocked with "Squad full · remove one first".',
        'Emphasis marks (`*word*`) in the descriptions render as emphasis. SquadView hides stability until operatives differ in it.',
    ],
)
doc(
    'UnitStats',
    'Runtime/Composites/Units.cs (UnitStats)',
    "One unit's post-run contribution: share of squad damage and six figures.",
    '`view`: a ContributionView `{operative, fellWave (0 = standing), damage, kills, casts, power, share (0–1), extraLabel, extraValue}`.',
    [
        "The share bar takes the operative's kit colour; FELL WAVE n shows in `enemy-text` only for a unit that fell.",
        "Usually ResultSummary's popover content.",
    ],
)
doc(
    'ResultSummary',
    'Runtime/Composites/Units.cs (ResultSummary)',
    'Results cell for one unit: its name and, when it fell, FELL Wn; hover, focus or tap shows its UnitStats above it.',
    '`view` (ContributionView), `pinned` (keep the popover open).',
    [
        'A unit still standing says nothing; fallen units are struck through; the kit colour runs along the bottom edge.',
    ],
)
doc(
    'ChoiceCard',
    'Runtime/Composites/ChoiceCard.cs (ChoiceCard)',
    'One option the player takes by tapping it (a road, an extract choice, a perk); the whole card is the button.',
    '`road` (RoadView, with `action`, default TAKE THIS ROAD) or `perk` (PerkView) with `selected`; or the fields directly: `tag`, `icon`, `title`, `text`, `next`, `action`, `rank`/`ranks`/`rankNext`/`rankTone`. `side` (`single`, `left`, `right`: a pair cuts its outer corners), `tone` (`ally`, `enemy`, `power`), `onPick`.',
    [
        'A perk is a ChoiceCard wherever it is offered: tier tints the card (general neutral, operative teal, rare cyan, epic violet) and its tag and glyph; a selected card is framed with brackets.',
        'Emphasis marks (`*word*`) in `text` render as emphasis. Only the choice card keeps its edge under the border rule.',
    ],
)
doc(
    'PerkDetail',
    'Runtime/Composites/Perks.cs (PerkDetail)',
    'Full perk explanation: draft (the value now, if there is one, then after the pick) or owned (active value, stacks, waves picked); each fact is one StatRow.',
    '`view` (PerkView incl. `waves`), `draft`, optional `style` width (popovers use 260).',
    [
        'The value that improves reads in `ally-text`.',
        "Shown inline and as ChipRow's popover.",
    ],
)
doc(
    'ChipRow',
    'Runtime/Controls/ChipRow.cs (ChipRow)',
    'A chip on one plate with no edge: a glyph, a word and a value. A perk chip is tinted by its rarity; hover, focus or tap shows its PerkDetail.',
    "`view` (PerkView), `alignEnd` (popover aligns to the chip's right edge), `pinned`; or `text`, `icon`, `value` and `static` (read only, as tall as its plate: the HUD's credits).",
    [
        'A chip that is a target has a 44-unit hit area (the plate is 30). Stacks above one show ×n in `ally-text`.',
        'Chips wrap in a row (`vb-row vb-row--wrap`) with no gap of their own.',
    ],
)
doc(
    'PerkRow',
    'Runtime/Composites/Perks.cs (PerkRow)',
    'Active-perk list row: art, name, tag, effect, current value and stacks.',
    '`view` (PerkView), `selected`, `onPick`.',
    ['The effect ellipsizes; the current value sits right in `ally-text` over its stacks.'],
)

doc(
    'StatRow',
    'Runtime/Controls/StatRow.cs (StatRow)',
    'One fact on one row: a key that keeps its width, a value that wraps beside it and, optionally, a glyph and a trailing action 44 units tall.',
    '`label`, `value` (text or nodes), `good`, `icon`, `action` (a button), `bar` (ProgressBar props), `stacked` (the value under the key), `dimmed`, `plated` (one tone above the parent), `tier`.',
    [
        "A KeyValueRow with a trailing slot: the merchant's BUY and SELL and a perk's detail rows are StatRows.",
        'A row that cannot be acted on is dimmed, with no second price or reason.',
    ],
)

doc(
    'Carousel',
    'Runtime/Composites/Carousel.cs (Carousel)',
    'Cards where the active one takes 64% of the width and its neighbours sit partly beneath it, tilted; endless both ways.',
    'children (the cards: any element that takes `className`, `style` and `onFocus`, a ChoiceCard for perks), `selected` (the active index), `onChange(index)`.',
    [
        'Swipe (the row follows the finger, settles on the nearest card, a flick goes one further), tap a card beneath, the arrow keys or focus change the card and raise `onChange`; it glides in 320 ms and is instant under reduced motion.',
        'The carousel is layout only: the active card shows its own information, so nothing repeats it elsewhere.',
    ],
)

doc(
    'QueueChip',
    'Runtime/Controls/QueueChip.cs',
    'One entry of the Power queue: place, skill, operative, cost and time to fire, with a full-height × on manual entries.',
    '`view` (QueueView `{who, skill, cost, eta, manual, next}`), `position` (1-based), `preview` (dashed: what an aim would add), `onCancel(view)`.',
    [
        'The first entry, or a buffered one (`next`, shown as ›), has the thicker lavender frame.',
        'Only manual entries cancel; autoplay estimates and previews show no ×. The × is a 44 × 44 target.',
        'PowerStrip places up to four of these under its track.',
    ],
)
doc(
    'Dialog',
    'Runtime/Composites/Dialogs.cs (Dialog.Sheet)',
    'A confirmation sheet: a solid panel with a title and one line of body text; the actions are its children.',
    '`title`, `body`, `bodyClass` (default `vb-t-caption`), `children` (VbButtons), `className`, `style`.',
    [
        'One primary action at most; the safe choice is secondary. In Unity, Dialog.Push puts it on a modal layer that blurs the page and returns focus when it closes.',
        'Say what happens, not "Are you sure": "Your profile is saved. Resume returns to the last completed-wave checkpoint."',
    ],
)
doc(
    'Backdrop',
    'Runtime/Composites/Dialogs.cs (Backdrop)',
    'Full-bleed menu art scaled to cover, with a gradient shade along the bottom so text over it stays legible.',
    '`image` (URL), `style` (for a CSS gradient stand-in), `children`.',
    [
        "Fills its positioned parent. The arcade's 3D menu lobby replaces it on Standard; the image is the fallback on the low tier and under reduced motion."
    ],
)
doc(
    'ScanLines',
    'Runtime/Composites/Holo.cs (ScanLines)',
    'Horizontal projected-image lines over a holographic surface, from `--vb-scan` and `--vb-scan-pitch`.',
    '`className`. Place it as the first child of a positioned surface; it fills it and ignores the pointer.',
    ['Never under live battle numbers (Power, cooldowns, health). Hidden in the high-contrast theme.'],
)
doc(
    'HoloFrame',
    'Runtime/Composites/Holo.cs (HoloFrame)',
    'A diegetic holographic panel for menus: cut frame with cyan brackets, scanlines, a chromatic caps title and a flicker-in.',
    '`title`, `children` (the body), `still` (skip the flicker for captures), `className`, `style`.',
    [
        'Menus only, one in focus per screen; battle panels never flicker.',
        'Under `vb-reduced-motion` or `vb-reduced-flashes` on an ancestor the flicker becomes a plain fade and the chromatic copies hide.',
    ],
)
doc(
    'HoloPlatform',
    'Runtime/Composites/Holo.cs (HoloPlatform)',
    'The 2D stand-in for a lineup plinth: a holo plate with a caps name tag, a floating diamond, brackets when selected and a dashed OPEN SLOT.',
    '`name`, `detail`, `state` (`occupied`, `selected`, `open`), `team` (`ally`, `enemy`), `showMarker`, `onPick`.',
    ['The 3D menu lobby draws the real plates; this one serves 2D menus and the low tier.'],
)
doc(
    'RevealCard',
    'Runtime/Composites/Holo.cs (RevealCard)',
    'A rift result: a holographic back that flips to a tier-framed front.',
    '`tier` (`blue`, `violet`, `gold`), `title`, `detail` ("AR · Striker", or the item slot), `item`, `isNew` (default true), `dupe` ("Resonance R2", replaces NEW), `flipped`, `tierLabel`, `onFlip`.',
    [
        'Tier words travel with the colour: RARE, EPIC, LEGENDARY; items MAGIC ITEM, RARE ITEM, UNIQUE ITEM.',
        'Tap (or Enter/Space) flips it with an edge-on squash; violet waits 90 ms and gold 180 ms; reduced motion turns it at once. A Legendary front glows gold.',
    ],
)
doc(
    'RuleRow',
    'Runtime/Composites/Holo.cs (RuleRow)',
    'One IF … THEN automation rule with its place, highlighted condition values, the action and cost, a toggle and a note on when it can run.',
    '`view` (RuleView `{index, conditions: [{text, value, after}], any, action, actionRest, cost, enabled, warning, draft, note}`), `onToggle(enabled)`.',
    [
        'Conditions join with AND, or OR when `any`. Values are highlighted; the note says plainly when the rule can run ("Runs only at 80+ Power").',
        "A warning frames the row in `warn`; a draft is dashed; an off rule dims. The player's own command always wins over a rule.",
    ],
)
doc(
    'WorldHealthBar',
    'Runtime/World/WorldElements.cs (WorldHealthBar)',
    "A fighter's world bar: health in team colour, barrier after it, and a pale ghost of recent damage.",
    '`health` (0–1), `barrier` (0–1, capped by the empty part), `ghost` (0–1; health before the last hit), `team` (`ally`, `enemy`), `rank` (`normal` 36 px, `elite` 44, `boss` 56), `tint` (a kit accent).',
    [
        'Health is never a green-to-red ramp. In Unity the WorldLayer holds the ghost for 0.25 s, then drains it at 75% of the bar per second.'
    ],
)
doc(
    'CombatText',
    'Runtime/World/WorldElements.cs (CombatText)',
    'A floating combat number from a confirmed simulation event.',
    '`kind` (`damage`, `taken`, `crit`, `exposed`, `heal`, `barrier`, `power`, `miss`, `execute`, `break`), `amount`. `CombatText.format(kind, amount)` gives the text.',
    [
        'Damage dealt reads in text colour; damage taken carries a true minus in enemy text; crits end in !, executes and breaks flare.',
        'In Unity the WorldLayer merges same-kind hits within 0.2 s, keeps three per target and 40 in all, and lifts them 24 units over 1.2 s.',
    ],
)
doc(
    'WorldLabel',
    'Runtime/World/WorldElements.cs (WorldLabel)',
    'Billboard text tied to a fighter or a point: aim chips, name tags, telegraph timers, move estimates and blocked markers.',
    '`kind` (`aim`, `name`, `telegraph`, `estimate`, `blocked`), `title`, `detail`, `team`, `showDetail`.',
    [
        'Anchored at its bottom centre; constant screen size. Crowded labels stack upward and drop their detail first. A blocked label reads "× OUT OF RANGE". Labels never cover a telegraph.'
    ],
)
doc(
    'WorldMarker',
    'Runtime/World/WorldElements.cs (WorldMarker)',
    'A marker at a projected point: allies are circles and enemies diamonds, always, so ownership reads from shape.',
    '`kind` (`ally`, `enemy`, `selected`, `queued`, `move`), `size` (px).',
    [
        'Selected adds brackets, queued a lavender ring, move a dashed ring. Perspective ground rings belong to the 3D telegraph layer.'
    ],
)
doc(
    'OffscreenIndicator',
    'Runtime/World/WorldElements.cs (OffscreenIndicator)',
    'A chip pinned to the board edge on the line from the centre toward a fighter the camera cannot frame.',
    '`name`, `team`, `metres` (omit to hide), `threat` (crimson frame: the breach warning), `angle` (degrees clockwise from screen-right; the chevron points along it).',
    ['The arcade shows BREACH for an enemy past the breach line, with its distance to the rear line.'],
)
doc(
    'EdgeWarning',
    'Runtime/World/WorldElements.cs (EdgeWarning)',
    'A band along the bottom (or top) board edge warning that a threat is about to cross it: five chevrons pointing over the edge and a caption.',
    '`caption`, `urgency` (0 hides; above two thirds runs hot), `top` (dock to the top edge).',
    [
        'The arcade shows it while any hostile is inside the breach zone; urgency grows as the closest one nears the rear line.',
        'In Unity the chevrons ripple outward, faster with urgency, and hold still under reduced motion.',
    ],
)

# ── The handbook's atoms, composites, layouts and views (0.3.0) ──────────────
for name, cs, summary, provide, rules in [
    (
        'ProgressBar',
        'Runtime/Controls/Progress.cs',
        'The one progress bar: continuous, or `segments` equal levels.',
        '`segments` (0 = continuous), `value` (levels, or 0–1), `next` (outline the level a pick adds), `tone` (ally, power, enemy, neutral).',
        ['Perk levels, stacks and steps are segments; health and progress are continuous.'],
    ),
    (
        'KeyValueRow',
        'Runtime/Controls/Progress.cs',
        'One key/value line: a caption key, a value on the right.',
        '`label`, `value`, `good` (ally colour).',
        ['Perk details, the results ledger and fact lists use it.'],
    ),
    (
        'ListRow',
        'Runtime/Controls/Progress.cs',
        'The one list row: glyph, caps title, caption, optional progress bar, trailing value and action.',
        '`icon`, `title`, `caption`, `bar` (ProgressBar props), `value`, `action`, `selected`, `onPick`.',
        ['Merchant offers, stack sales and health trades are ListRows; put rows in a ListLayout.'],
    ),
    (
        'TextComponent',
        'Runtime/Controls/Text.cs',
        'Every run of text: one type style and an optional tone.',
        '`typeStyle` (display, title, label, badge, body, caption, num and the three variants: body / body-sm, label-sm (= badge) / label / label-lg, heading-sm / heading / heading-lg), `tone` (ally, enemy, power, muted), `text`.',
        [
            "One type style per run; tones only recolour. The three variants (body, label, heading) are what the 0.9.0 type scale is built from; every size follows the board's width through the fluid scale (`vb-fluid-<step>`, set on each `.vb-root` by the kit).",
            'Pass caps yourself, or use LabelComponent.',
        ],
    ),
    (
        'LabelComponent',
        'Runtime/Controls/Text.cs',
        'The caps eyebrow over a section, field or value, with 4 units under it.',
        '`text` (uppercased for you), `size` (label or badge), `tone`.',
        ['A label names a thing; it is never a sentence.'],
    ),
    (
        'BadgeComponent',
        'Runtime/Controls/Badge.cs',
        'A small slim chip: a word, an optional glyph before it and a value after it.',
        '`text`, `icon`, `value`, `tone` (neutral, ally, enemy, power, warn, inverted), children.',
        [
            'StatusChip and ChipRow (`ChipRow.Credits()`) are the status and currency presets (`Badge.Status`, `Badge.Credits` in Unity).',
            'Colour never travels alone: a tone always comes with a word.',
        ],
    ),
    (
        'IconButtonComposite',
        'Runtime/Controls/IconButton.cs',
        'A 44 × 44 chipped square with one glyph or a short state, and a tooltip.',
        '`icon` or `text`, `tooltip` (required: it is the accessible name), `active`, `quiet`, `size`, `onClick`.',
        [
            'Back, pause, close, speed and autoplay are all this control.',
            '`quiet` is the translucent HUD look over the world; `active` is toggled on.',
        ],
    ),
    (
        'TileComponent',
        'Runtime/Controls/Tile.cs',
        'A pickable cell: picture or glyph, name, caption with its glyph, order and state.',
        '`title`, `caption`, `captionIcon`, `picture`, `glyph`, `kit`, `order`, `state` (in, focused, locked, empty; in and focused combine), `tier`, `emptyLabel`, `onPick`, `block`.',
        [
            'UnitTile and UnitSlot are tiles (`block` vb-tile, vb-slot).',
            'Locked tiles stay visible and say how to unlock; lay tiles out in a grid.',
        ],
    ),
    (
        'CardComponent',
        'Runtime/Controls/Card.cs',
        'A slim container with an optional media band, a head, a body and a foot of actions.',
        '`title`, `meta`, `media`, `actions`, `selected`, children (the body).',
        ['Cards are slim; only a selected card is chipped (edge and brackets).', 'Foot actions are right-aligned.'],
    ),
    (
        'DrawerOverlay',
        'Runtime/Composites/Drawer.cs',
        'A bottom drawer: a slim sheet that slides up over a backdrop to the height of its content (at most 90% of the screen); drag its top up for full screen or down to close; half the screen is opt-in.',
        '`open` (default true), `snap` (half or full, where it opens), `onSnap`, `title`, `meta`, `backdrop` (false docks it inside a view), `dismissible`, `closeButton`, `actions`, `onDismiss`, children (the body).',
        [
            'Drag the grip or head: up snaps to full screen, down back to half, below a quarter of the screen closes it. A tap on the grip toggles half and full.',
            'The backdrop and × dismiss it too; `onDismiss` fires, then it slides away. SquadView opens operative details in one.',
            'Unity: `DrawerOverlay.Push(nav, title, closed)` puts one on a modal layer that Back pops.',
        ],
    ),
    (
        'FieldComposite',
        'Runtime/Composites/Forms.cs',
        'One setting: a caps label, its input and one line of what it does.',
        '`label`, `help`, children (the input).',
        ['Use it for inputs without their own label; VbSlider and VbSwitch carry theirs.'],
    ),
    (
        'Fieldset',
        'Runtime/Core/Ui.cs (Ui.Section)',
        'A titled group of related fields in one slim container.',
        '`title`, children (fields).',
        ['One topic per fieldset, 1–4 fields.'],
    ),
    (
        'FormComposite',
        'Runtime/Composites/Forms.cs',
        'Fieldsets in a scroll, an optional note, a footer of secondary actions and a saved line.',
        '`saved`, `headed` (the saved state shows in a ScreenHeader instead), `note` ({ icon, text }), `footer`, children (fieldsets).',
        [
            'Settings apply as they change; there is no Submit.',
            'Destructive actions live in the footer as danger buttons and confirm.',
        ],
    ),
    (
        'StackLayout',
        'Runtime/Layouts/Layouts.cs',
        'A column with one rhythm: the gap under every child but the last.',
        '`gap` (4, 8, 12 or 16), children.',
        ['Use it instead of per-element margins.'],
    ),
    (
        'VbGrid',
        'Runtime/Layouts/Layouts.cs',
        'Cells 2, 3 or 4 across; `wrap` adds the row rhythm.',
        '`columns` (2, 3 or 4), `wrap`, children.',
        ['Pick the count the items fill; never a half-empty row.'],
    ),
    (
        'ListLayout',
        'Runtime/Layouts/Layouts.cs',
        'Rows of one kind in a scroll, 6 between rows, with an empty-state line.',
        '`empty`, children (rows).',
        ['ScoreRow, PerkRow and RuleRow carry no margin of their own; put them in a list.'],
    ),
    (
        'BoardLayout',
        'Runtime/Layouts/Layouts.cs',
        'The battle board: top band, the world (the rest, at least half) and the bottom band.',
        '`top`, `world`, `bottom`.',
        ['Top and bottom never shrink.'],
    ),
    (
        'MainMenuView',
        'Runtime/Views/MenuViews.cs',
        'The title screen: offline badge, best and the EXIT icon button, the mark and the menu actions.',
        '`view` (status, subtitle, best, bestWave, checkpoint, squad, notice, canExit), `onPlay`, `onResume`, `onScores`, `onSettings`, `onExit`.',
        ['PLAY is the one primary.'],
    ),
    (
        'MenuActionsSectionView',
        'Runtime/Views/MenuViews.cs',
        'The thumb-zone block: checkpoint card, PLAY, then one row of HIGH SCORES (filling it) and the settings cog (an icon-only button).',
        '`checkpoint` (wave, score, standing, squadSize, perks, seed) or null, `squadLine`, `notice`, `onPlay`, `onResume`, `onScores`, `onSettings`.',
        ['RESUME is secondary and lives in the checkpoint card.', 'The card only exists when there is a checkpoint.'],
    ),
    (
        'SquadView',
        'Runtime/Views/SquadView.cs',
        'Pick four: squad slots, the roster grid, the open details and START RUN.',
        '`view` (roster, selected, focus, locked, tiers, seed), `onPick(i)`, `onSlotPick(i)`, `onToggle(i)`, `onClose`, `onStart`, `onBack`.',
        ['START RUN is blocked with its reason until the squad has four.'],
    ),
    (
        'SettingsView',
        'Runtime/Views/SettingsView.cs',
        'Audio, feel, camera, quality, accessibility (reduced motion and a row to the display options) and data as fieldsets in one form; applies as it changes.',
        '`view` (master, music, effects, sensitivity, haptics, reducedMotion, camera, quality, saved, note), `onChange(model)`, `onClearScores`, `onAccessibility`, `onBack`.',
        ['Every change reports the whole model.'],
    ),
    (
        'PauseOverlayView',
        'Runtime/Views/OverlayViews.cs',
        'The run, frozen: the screen header without a back arrow (its title reveals), then one overlay panel centred on the screen: stats with an info popover, the active perks, RESUME and one row of EXIT TO MENU, ABANDON and the settings cog.',
        '`view` (wave, score, standing, squadSize, perks, seed, detail), `onResume`, `onSettings`, `onPerks`, `onAbandon`, `onExit`.',
        [
            'RESUME is the one primary; the header back square and Back resume too.',
            'ABANDON is danger and confirms; EXIT TO MENU is a ghost at the bottom of the panel.',
        ],
    ),
    (
        'ConfirmDialogView',
        'Runtime/Views/OverlayViews.cs',
        'One shape for every confirmation: question, consequence, destructive action, way back.',
        '`title`, `body`, `confirm` ({ label, icon, sub, variant }), `cancel` ({ label, icon }), `onConfirm`, `onCancel`.',
        ['Unity: `ConfirmDialogView.Push(nav, …)` puts it on a modal layer.'],
    ),
    (
        'GuideDialogView',
        'Runtime/Views/OverlayViews.cs',
        'The first-run guide: numbered steps and READY.',
        '`title`, `steps`, `onReady`.',
        ['Unity: `GuideDialogView.Push(nav, title, steps, ready)`.'],
    ),
    (
        'PerkDraftView',
        'Runtime/Views/RunViews.cs',
        'Wave cleared: three perk cards, the pick explained, the squad that carries forward and TAKE PERK.',
        '`view` (wave, bonus, score, choices, pick, squad: code, hp, maxHp, stability), `onPick(i)`, `onTake`.',
        ['TAKE PERK says what the pick does next.'],
    ),
    (
        'ResultsView',
        'Runtime/Views/RunViews.cs',
        "Run over: the score and its ledger, stats, the squad's contribution, the perks, MENU and RUN AGAIN.",
        '`view` (cause, score, wave, bosses, rank, of, ledger, distance, simTime, squad, perks), `onMenu`, `onAgain`.',
        ['RUN AGAIN is the one primary.'],
    ),
    (
        'HighScoresView',
        'Runtime/Views/RunViews.cs',
        'Top 20 on this device with the current run framed, then older-rules runs.',
        '`ranked`, `historical`, `caption`, `onBack`.',
        ['Local scores only; say so in the footer.'],
    ),
    (
        'RouteChoiceView',
        'Runtime/Views/BattleViews.cs',
        'The crossroads screen around RouteChoice.',
        '`view` (RouteView), `onChoose(side)`.',
        ['Only one choice is accepted.'],
    ),
    (
        'MerchantScreenView',
        'Runtime/Views/BattleViews.cs',
        'The night market screen (design name MerchantView) around MerchantScreen.',
        '`view` (MerchantView), `onBuy`, `onSellPerk`, `onSellHealth`, `onLeave`.',
        ['The host confirms every trade and binds again.'],
    ),
    (
        'BattleHudView',
        'Runtime/Views/BattleViews.cs',
        'The whole battle board: HUD band, boss bar, announcer, the world and four operative cards.',
        '`hud` (HudView), `operatives`, `boss` (or null), `announcement`, `world`, `onPause`, `onAuto`, `onSpeed`, `onQueueCancel`.',
        ['Unity: bind it every frame; the game attaches its aim drag to `Card(i).Skill(n)`.'],
    ),
]:
    doc(name, cs, summary, provide, rules)

doc(
    'EffectDetails',
    'Runtime/Composites/Effects.cs',
    'Shared effect meaning, value, source, duration and activation.',
    '`view` or `effects`; callbacks report intent.',
    ['Unity owns the contract; React mirrors it.'],
)
doc(
    'EffectBadge',
    'Runtime/Composites/Effects.cs',
    'Inspectable touch target for a state modifier.',
    '`view` or `effects`; callbacks report intent.',
    ['Unity owns the contract; React mirrors it.'],
)
doc(
    'EffectList',
    'Runtime/Composites/Effects.cs',
    'Every modifier, individually inspectable without truncation.',
    '`view` or `effects`; callbacks report intent.',
    ['Unity owns the contract; React mirrors it.'],
)
doc(
    'EffectStrip',
    'Runtime/Composites/Effects.cs',
    'Compact battle status strip with complete overflow.',
    '`view` or `effects`; callbacks report intent.',
    ['Unity owns the contract; React mirrors it.'],
)
doc(
    'ActivePerksView',
    'Runtime/Views',
    'Owned perk stacks, category totals and selected explanation.',
    '`view` or `effects`; callbacks report intent.',
    ['Unity owns the contract; React mirrors it.'],
)
doc(
    'RevivePickView',
    'Runtime/Views',
    'Choose one fallen operative to restore.',
    '`view` or `effects`; callbacks report intent.',
    ['Unity owns the contract; React mirrors it.'],
)
doc(
    'AccessibilityView',
    'Runtime/Views',
    'Shared accessibility controls and saved state.',
    '`view` or `effects`; callbacks report intent.',
    ['Unity owns the contract; React mirrors it.'],
)

DTS = r"""
import type * as React from 'react';

/* ── The kit (com.veilbreak.gui ported; render inside class="vb-root") ── */
export type KitIconName = string; // window.Veilbreak.VbIcon.names (kebab or PascalCase file name, or a design-system alias)
export type SkillState = 'ready' | 'aiming' | 'cooldown' | 'short' | 'silenced' | 'fallen' | 'casting' | 'disabled' | 'queued' | 'buffered' | 'windup' | 'invalid' | 'rejected' | 'exposed';
export type RarityTier = 'none' | 'blue' | 'violet' | 'gold';
export type WorldTeam = 'ally' | 'enemy';
export type ControlMode = 'auto' | 'manual' | 'aiming' | 'fallen';
export type PerkTier = 'general' | 'operative' | 'rare' | 'epic';
export type StatusKind = 'exposed' | 'silenced' | 'rooted' | 'slowed' | 'interrupted' | 'barrier';
export interface SkillView { label?: string; icon?: KitIconName; slot?: 'A' | 'S' | string; kind?: string; description?: string; fullDescription?: string; cost?: number; state?: SkillState; charged?: number; seconds?: number; cooldownSeconds?: number; need?: number; queue?: number; progress?: number; reason?: string }
export interface OperativeView { id?: string; name: string; code: string; role?: string; roleIcon?: KitIconName; passive?: string; passiveText?: string; passiveFullText?: string; passiveIcon?: KitIconName; basic?: SkillView | null; lore?: string; canRevive?: boolean; kit: string; portrait?: string | null; hp: number; maxHp: number; barrier?: number; armor?: number; stability?: number; maxStability?: number; effects?: EffectInstance[]; mode?: ControlMode; manualSeconds?: number; skills?: SkillView[] }
export interface PerkView { id?: string; name: string; category?: string; tag?: string; icon?: KitIconName; effect?: string; now?: string; next?: string; tier?: PerkTier; art?: string | null; stack?: number; max?: number; waves?: number[] }
export interface QueueView { id?: string; who: string; skill: string; cost: number; eta: string; actor?: number; slot?: number; manual?: boolean; next?: boolean }
export interface HudView { credits?: number; speed?: number; wave?: number; score?: number; boss?: boolean; auto?: boolean; slow?: boolean; power?: number; maxPower?: number; regen?: string; hint?: string | null; focus?: number; previewFrom?: number; previewTo?: number; queue?: QueueView[]; queueTitle?: string; queueHint?: string }
export interface BossView { effects?: EffectInstance[]; name: string; health: number; stability: number; detail?: string; shielded?: boolean; interruptible?: boolean; shieldDetail?: string }
export interface ScoreView { id?: string; rank: number; score: number; wave: number; bosses: number; squad: string[]; current?: boolean }
export interface ContributionView { operative: OperativeView; fellWave?: number; damage?: number; kills?: number; casts?: number; power?: number; share?: number; extraLabel?: string; extraValue?: string }

export interface VbFrameProps { kind?: 'slim' | 'chipped'; className?: string; style?: React.CSSProperties; children?: React.ReactNode }
export declare function VbFrame(props: VbFrameProps): React.ReactElement;
export interface VbIconProps { icon: KitIconName; size?: number; title?: string; className?: string; style?: React.CSSProperties }
export declare function VbIcon(props: VbIconProps): React.ReactElement;
export interface VbMarkProps { width?: number; className?: string; style?: React.CSSProperties }
export declare function VbMark(props: VbMarkProps): React.ReactElement;
export interface VbButtonProps { title: string; variant?: 'primary' | 'secondary' | 'light' | 'danger' | 'ghost'; sub?: string; icon?: KitIconName; blocked?: string; iconOnly?: boolean; lead?: boolean; dimmed?: boolean; compact?: boolean; onClick?: () => void; className?: string; style?: React.CSSProperties }
export declare function VbButton(props: VbButtonProps): React.ReactElement;
export interface ActionRowProps { single?: boolean; fixed?: boolean; minItemWidth?: number; className?: string; style?: React.CSSProperties; children?: React.ReactNode }
export declare function ActionRow(props: ActionRowProps): React.ReactElement;
export interface PopoverCardProps { title?: string; hint?: string; rows?: { key: string; value: string; good?: boolean }[]; className?: string; style?: React.CSSProperties; children?: React.ReactNode }
export declare function PopoverCard(props: PopoverCardProps): React.ReactElement;
export declare function VbClipFill(props: { className?: string; style?: React.CSSProperties }): React.ReactElement;
export declare function VbClipImage(props: { url?: string; className?: string; style?: React.CSSProperties }): React.ReactElement;
export declare const VbEdgeFx: { play(element: Element, tone?: number[]): void; damage(element: Element, strength?: number): void; cancel(element: Element): void; active(element: Element): boolean; milliseconds: { activate: number; damage: number; calm: number } };
export declare const Glitch: { reveal(element: Element, milliseconds?: number): Promise<void>; revealTitles(root: Element, milliseconds?: number): void; scramble(text: string, reveal: number, step: number): string; titleClass: string; milliseconds: number };
export interface StatReadoutProps { label: string; value: string; unit?: string; bar?: number; delta?: string; up?: boolean; tone?: 'power' | 'text'; className?: string }
export declare function StatReadout(props: StatReadoutProps): React.ReactElement;
export interface StatusChipProps { kind: StatusKind; text?: string; detail?: string; time?: string; className?: string }
export declare function StatusChip(props: StatusChipProps): React.ReactElement;
export interface VbSwitchProps { text: string; detail?: string; value?: boolean; onChange?: (value: boolean) => void; className?: string }
export declare function VbSwitch(props: VbSwitchProps): React.ReactElement;
export interface VbSliderProps { label: string; value?: number; min?: number; max?: number; onChange?: (value: number) => void; className?: string }
export declare function VbSlider(props: VbSliderProps): React.ReactElement;
export interface VbSegmentedProps { items: { label: string; sub?: string }[]; selected?: number; onChange?: (index: number) => void; className?: string }
export declare function VbSegmented(props: VbSegmentedProps): React.ReactElement;
export interface VbMeterProps { value: number; extra?: number; previewFrom?: number; previewTo?: number; marks?: number[]; before?: React.ReactNode; after?: React.ReactNode; className?: string; style?: React.CSSProperties }
export declare function VbMeter(props: VbMeterProps): React.ReactElement;
export interface HealthBarProps { hp: number; max: number; barrier?: number; stability?: number; exposed?: boolean; enemy?: boolean; ghost?: number; size?: 'hud' | 'world' | 'large'; caption?: string; className?: string; style?: React.CSSProperties }
export declare function HealthBar(props: HealthBarProps): React.ReactElement;
export interface PowerStripProps { value: number; max?: number; regen?: string; hint?: string; previewFrom?: number; previewTo?: number; marks?: number[]; reserve?: number; queue?: QueueView[]; onCancel?: (entry: QueueView) => void; legend?: string; className?: string; style?: React.CSSProperties }
export declare function PowerStrip(props: PowerStripProps): React.ReactElement;
export interface FocusMeterProps { focus?: number; slow?: boolean; regenPerSecond?: number; drainPerSecond?: number; slowScale?: number; className?: string; style?: React.CSSProperties }
export declare function FocusMeter(props: FocusMeterProps): React.ReactElement;
export interface SkillButtonProps { view: SkillView; dragging?: boolean; flash?: boolean; onPointerDown?: (e: React.PointerEvent) => void; className?: string; style?: React.CSSProperties }
export declare function SkillButton(props: SkillButtonProps): React.ReactElement;
export interface RoadView { kind?: string; title: string; icon?: KitIconName; hint: string; rank?: number; ranks?: number }
export interface RouteView { wave: number; left: RoadView; right: RoadView }
export interface MerchantOfferView { id?: string; perk: PerkView; price: number; taken?: boolean; blocked?: string }
export interface MerchantStackView { perk: PerkView; payout: number }
export interface MerchantHealthView { actor: number; name: string; hp: number; maxHp: number; cost: number; payout: number; blocked?: string }
export interface MerchantView { credits: number; offers: MerchantOfferView[]; stacks: MerchantStackView[]; health: MerchantHealthView[] }
export interface StatusRowProps { effects?: EffectInstance[]; pinned?: boolean; className?: string; style?: React.CSSProperties }
export declare function StatusRow(props: StatusRowProps): React.ReactElement;
export declare function GlitchText(props: { text: string; effect?: 'in' | 'out' | 'slam'; reveal?: boolean | number; className?: string; style?: React.CSSProperties }): React.ReactElement;
export declare function RouteChoice(props: { view: RouteView; onChoose?: (side: number) => void; className?: string; style?: React.CSSProperties }): React.ReactElement;
export declare function MerchantScreen(props: { view: MerchantView; onBuy?: (index: number) => void; onSellPerk?: (id: string) => void; onSellHealth?: (actor: number) => void; onLeave?: () => void; className?: string; style?: React.CSSProperties }): React.ReactElement;
export interface HudTopProps { view: HudView; showFocus?: boolean; showHint?: boolean; queuePinned?: boolean; onAutoClick?: () => void; onSpeedClick?: () => void; onPauseClick?: () => void; onQueueCancel?: (entry: QueueView) => void; className?: string; style?: React.CSSProperties }
export declare function HudTop(props: HudTopProps): React.ReactElement;
export interface QueueListProps { queue: QueueView[]; title?: string; hint?: string; onCancel?: (entry: QueueView) => void; className?: string }
export declare function QueueList(props: QueueListProps): React.ReactElement;
export interface BossBarProps { view: BossView; className?: string }
export declare function BossBar(props: BossBarProps): React.ReactElement;
export interface ScreenHeaderProps { title: string; meta?: string; accent?: boolean; backLabel?: string; showBack?: boolean; revealTitles?: boolean; onBack?: () => void; className?: string }
export declare function ScreenHeader(props: ScreenHeaderProps): React.ReactElement;
export interface ScoreRowProps { view: ScoreView; className?: string }
export declare function ScoreRow(props: ScoreRowProps): React.ReactElement;
export interface UnitCardProps { view: OperativeView; autoDamage?: boolean; dragging?: number; onSkillPointerDown?: (index: number, e: React.PointerEvent) => void; className?: string; style?: React.CSSProperties }
export declare function UnitCard(props: UnitCardProps): React.ReactElement;
export interface UnitTileProps { view: OperativeView; slot?: number; focused?: boolean; tier?: RarityTier; locked?: string; onPick?: () => void; className?: string }
export declare function UnitTile(props: UnitTileProps): React.ReactElement;
export interface UnitSlotProps { view: OperativeView | null; slot: number; onPick?: () => void; className?: string }
export declare function UnitSlot(props: UnitSlotProps): React.ReactElement;
export interface UnitDetailProps { view: OperativeView; slot?: number; squadFull?: boolean; showStability?: boolean; maxHp?: number; maxArmor?: number; maxStability?: number; onClose?: () => void; onToggle?: () => void; className?: string; style?: React.CSSProperties }
export declare function UnitDetail(props: UnitDetailProps): React.ReactElement;
export interface UnitStatsProps { view: ContributionView; className?: string }
export declare function UnitStats(props: UnitStatsProps): React.ReactElement;
export interface ResultSummaryProps { view: ContributionView; pinned?: boolean; className?: string }
export declare function ResultSummary(props: ResultSummaryProps): React.ReactElement;
export type ChoiceSide = 'single' | 'left' | 'right';
export type ChoiceTone = 'ally' | 'enemy' | 'power';
export interface ChoiceCardProps { road?: RoadView; perk?: PerkView; action?: string; selected?: boolean; side?: ChoiceSide; tone?: ChoiceTone; tag?: string; icon?: KitIconName; title?: string; text?: string; next?: string; rank?: number; ranks?: number; rankNext?: boolean; rankTone?: 'ally' | 'enemy' | 'power' | 'neutral'; tooltip?: string; motionId?: string | number; onPick?: () => void; onFocus?: () => void; className?: string; style?: React.CSSProperties }
export declare function ChoiceCard(props: ChoiceCardProps): React.ReactElement;
export interface StatRowProps { label: string; value?: React.ReactNode; good?: boolean; icon?: KitIconName; stacked?: boolean; dimmed?: boolean; plated?: boolean; tier?: string; bar?: { segments?: number; value?: number; next?: boolean; tone?: string } | null; action?: React.ReactNode; motionId?: string | number; className?: string; style?: React.CSSProperties }
export declare function StatRow(props: StatRowProps): React.ReactElement;
export interface ChipRowProps { view?: PerkView; text?: string; icon?: KitIconName; value?: string | number; static?: boolean; alignEnd?: boolean; pinned?: boolean; tooltip?: string; className?: string; style?: React.CSSProperties }
export declare function ChipRow(props: ChipRowProps): React.ReactElement;
export interface CarouselProps { selected?: number; onChange?: (index: number) => void; motionId?: string | number; children?: React.ReactNode; className?: string; style?: React.CSSProperties }
export declare function Carousel(props: CarouselProps): React.ReactElement;
export interface PerkDetailProps { view: PerkView; draft?: boolean; className?: string; style?: React.CSSProperties }
export declare function PerkDetail(props: PerkDetailProps): React.ReactElement;
export interface PerkRowProps { view: PerkView; selected?: boolean; onPick?: () => void; className?: string }
export declare function PerkRow(props: PerkRowProps): React.ReactElement;
export interface QueueChipProps { view: QueueView; position?: number; preview?: boolean; onCancel?: (entry: QueueView) => void; className?: string; style?: React.CSSProperties }
export declare function QueueChip(props: QueueChipProps): React.ReactElement;
export interface DialogProps { title?: string; body?: string; bodyClass?: string; className?: string; style?: React.CSSProperties; children?: React.ReactNode }
export declare function Dialog(props: DialogProps): React.ReactElement;
export interface BackdropProps { image?: string; className?: string; style?: React.CSSProperties; children?: React.ReactNode }
export declare function Backdrop(props: BackdropProps): React.ReactElement;
export declare function ScanLines(props?: { className?: string }): React.ReactElement;
export interface HoloFrameProps { title?: string; still?: boolean; className?: string; style?: React.CSSProperties; children?: React.ReactNode }
export declare function HoloFrame(props: HoloFrameProps): React.ReactElement;
export interface HoloPlatformProps { name?: string; detail?: string; state?: 'occupied' | 'selected' | 'open'; team?: WorldTeam; showMarker?: boolean; onPick?: () => void; className?: string; style?: React.CSSProperties }
export declare function HoloPlatform(props: HoloPlatformProps): React.ReactElement;
export interface RevealCardProps { tier?: 'blue' | 'violet' | 'gold'; title: string; detail?: string; item?: boolean; isNew?: boolean; dupe?: string; flipped?: boolean; tierLabel?: string; onFlip?: () => void; className?: string; style?: React.CSSProperties }
export declare function RevealCard(props: RevealCardProps): React.ReactElement;
export interface RuleCondition { text: string; value?: string; after?: string }
export interface RuleView { index?: number; conditions: RuleCondition[]; any?: boolean; action: string; actionRest?: string; cost?: number; enabled?: boolean; warning?: boolean; draft?: boolean; note?: string }
export interface RuleRowProps { view: RuleView; onToggle?: (enabled: boolean) => void; className?: string; style?: React.CSSProperties }
export declare function RuleRow(props: RuleRowProps): React.ReactElement;
export interface WorldHealthBarProps { health?: number; barrier?: number; ghost?: number; team?: WorldTeam; rank?: 'normal' | 'elite' | 'boss'; tint?: string; className?: string; style?: React.CSSProperties }
export declare function WorldHealthBar(props: WorldHealthBarProps): React.ReactElement;
export type CombatTextKind = 'damage' | 'taken' | 'crit' | 'exposed' | 'heal' | 'barrier' | 'power' | 'miss' | 'execute' | 'break';
export interface CombatTextProps { kind?: CombatTextKind; amount?: number; className?: string; style?: React.CSSProperties }
export declare function CombatText(props: CombatTextProps): React.ReactElement;
export interface WorldLabelProps { kind?: 'aim' | 'name' | 'telegraph' | 'estimate' | 'blocked' | 'elite'; title: string; detail?: string; count?: number; team?: WorldTeam; showDetail?: boolean; className?: string; style?: React.CSSProperties }
export declare function WorldLabel(props: WorldLabelProps): React.ReactElement;
export interface WorldMarkerProps { kind?: 'ally' | 'enemy' | 'selected' | 'queued' | 'move'; size?: number; className?: string; style?: React.CSSProperties }
export declare function WorldMarker(props: WorldMarkerProps): React.ReactElement;
export interface OffscreenIndicatorProps { name: string; team?: WorldTeam; metres?: number; threat?: boolean; angle?: number; className?: string; style?: React.CSSProperties }
export declare function OffscreenIndicator(props: OffscreenIndicatorProps): React.ReactElement;
export interface EdgeWarningProps { caption?: string; urgency?: number; top?: boolean; className?: string; style?: React.CSSProperties }
export declare function EdgeWarning(props: EdgeWarningProps): React.ReactElement;
/* ── 0.3.0: the handbook's atoms, composites, layouts and views ── */
type Base = { className?: string; style?: React.CSSProperties };
export type TypeStyle = 'display' | 'title' | 'label' | 'badge' | 'body' | 'caption' | 'num' | 'heading' | 'heading-sm' | 'heading-lg' | 'label-sm' | 'label-lg' | 'body-sm';
export type TextTone = 'default' | 'ally' | 'enemy' | 'power' | 'muted';
export declare function ProgressBar(props: Base & { segments?: number; value?: number; next?: boolean; tint?: string; tone?: 'ally' | 'power' | 'enemy' | 'neutral' }): React.ReactElement;
export declare function KeyValueRow(props: Base & { label: string; value: string; good?: boolean }): React.ReactElement;
export declare function ListRow(props: Base & { icon?: KitIconName; title: string; caption?: string; bar?: { segments?: number; value?: number; next?: boolean; tone?: string } | null; value?: string; action?: React.ReactNode; selected?: boolean; onPick?: () => void }): React.ReactElement;
export declare function TextComponent(props: Base & { text?: React.ReactNode; typeStyle?: TypeStyle; tone?: TextTone; children?: React.ReactNode }): React.ReactElement;
export declare function LabelComponent(props: Base & { text: string; size?: 'label' | 'badge' | 'large'; tone?: TextTone }): React.ReactElement;
export type BadgeTone = 'neutral' | 'ally' | 'enemy' | 'power' | 'warn' | 'inverted';
export declare function BadgeComponent(props: Base & { text?: string; icon?: KitIconName; value?: string | number; tone?: BadgeTone; tooltip?: string; children?: React.ReactNode }): React.ReactElement;
export declare function IconButtonComposite(props: Base & { icon?: KitIconName; text?: string; tooltip: string; active?: boolean; quiet?: boolean; size?: number; textClass?: string; onClick?: () => void }): React.ReactElement;
export type TileState = 'in' | 'focused' | 'locked' | 'empty';
export declare function TileComponent(props: Base & { title?: string; caption?: string; captionIcon?: KitIconName; picture?: string | null; glyph?: KitIconName; kit?: string; order?: number; state?: TileState | TileState[]; tier?: RarityTier; emptyLabel?: string; tooltip?: string; block?: string; onPick?: () => void }): React.ReactElement;
export declare function CardComponent(props: Base & { title?: string; meta?: string; media?: React.ReactNode; actions?: React.ReactNode; selected?: boolean; children?: React.ReactNode }): React.ReactElement;
export declare function DrawerOverlay(props: Base & { open?: boolean; snap?: 'fit' | 'half' | 'full'; onSnap?: (snap: 'fit' | 'half' | 'full') => void; title?: string; meta?: string; label?: string; backdrop?: boolean; dismissible?: boolean; closeButton?: boolean; actions?: React.ReactNode; onDismiss?: () => void; children?: React.ReactNode }): React.ReactElement;
export declare function FieldComposite(props: Base & { label?: string; help?: string; children?: React.ReactNode }): React.ReactElement;
export declare function Fieldset(props: Base & { title: string; children?: React.ReactNode }): React.ReactElement;
export declare function FormComposite(props: Base & { saved?: string; headed?: boolean; note?: { icon?: KitIconName; text: string } | null; footer?: React.ReactNode; children?: React.ReactNode }): React.ReactElement;
export declare function StackLayout(props: Base & { gap?: 4 | 8 | 12 | 16; children?: React.ReactNode }): React.ReactElement;
export declare function VbGrid(props: Base & { columns?: 2 | 3 | 4; wrap?: boolean; children?: React.ReactNode }): React.ReactElement;
export declare function ListLayout(props: Base & { empty?: string; children?: React.ReactNode }): React.ReactElement;
export declare function BoardLayout(props: Base & { top?: React.ReactNode; world?: React.ReactNode; bottom?: React.ReactNode }): React.ReactElement;
export interface CheckpointView { wave: number; score: number; standing?: number; squadSize?: number; perks?: number; seed?: string }
export interface MainMenuViewModel { status?: string; subtitle?: string; notice?: string; best?: number; bestWave?: number; checkpoint?: CheckpointView | null; squad?: string[]; canExit?: boolean }
export declare function MainMenuView(props: Base & { view?: MainMenuViewModel; onPlay?: () => void; onResume?: () => void; onScores?: () => void; onSettings?: () => void; onExit?: () => void }): React.ReactElement;
export declare function MenuActionsSectionView(props: Base & { checkpoint?: CheckpointView | null; squadLine?: string; notice?: string; onPlay?: () => void; onResume?: () => void; onScores?: () => void; onSettings?: () => void }): React.ReactElement;
export interface SquadViewModel { roster: OperativeView[]; selected?: number[]; focus?: number; locked?: string[]; tiers?: RarityTier[]; seed?: string; maxHp?: number; maxArmor?: number; maxStability?: number }
export declare function SquadView(props: Base & { view: SquadViewModel; onPick?: (index: number) => void; onSlotPick?: (slot: number) => void; onSlotMove?: (from: number, to: number) => void; onToggle?: (index: number) => void; onClose?: () => void; onStart?: () => void; onBack?: () => void }): React.ReactElement;
export interface SettingsViewModel { master?: number; music?: number; effects?: number; sensitivity?: number; haptics?: boolean; reducedMotion?: boolean; camera?: number; quality?: number; saved?: string; note?: string }
export declare function SettingsView(props: Base & { view?: SettingsViewModel; onChange?: (model: SettingsViewModel) => void; onClearScores?: () => void; onAccessibility?: () => void; onBack?: () => void }): React.ReactElement;
export interface PauseViewModel { wave: number; score: number; standing: number; squadSize?: number; perks?: PerkView[]; seed?: string; detail?: string }
export declare function PauseOverlayView(props: Base & { view: PauseViewModel; onResume?: () => void; onSettings?: () => void; onPerks?: () => void; onAbandon?: () => void; onExit?: () => void }): React.ReactElement;
export declare function ConfirmDialogView(props: Base & { title: string; body?: string; confirm?: { label: string; icon?: KitIconName; sub?: string; variant?: VbButtonProps['variant'] }; cancel?: { label: string; icon?: KitIconName }; onConfirm?: () => void; onCancel?: () => void }): React.ReactElement;
export declare function GuideDialogView(props: Base & { title?: string; steps?: string[]; onReady?: () => void }): React.ReactElement;
export interface SquadHealthView { code: string; hp: number; maxHp: number; stability?: number }
export interface PerkDraftViewModel { wave: number; bonus?: number; score?: number; choices: PerkView[]; pick?: number; squad?: SquadHealthView[] }
export declare function PerkDraftView(props: Base & { view: PerkDraftViewModel; onPick?: (index: number) => void; onTake?: () => void; onPerks?: () => void }): React.ReactElement;
export interface ResultsViewModel { cause?: string; score: number; wave: number; bosses?: number; rank?: number; of?: number; ledger?: { label: string; value: string }[]; distance?: string; simTime?: string; squad?: ContributionView[]; perks?: PerkView[] }
export declare function ResultsView(props: Base & { view: ResultsViewModel; onMenu?: () => void; onSquad?: () => void; onAgain?: () => void }): React.ReactElement;
export declare function HighScoresView(props: Base & { ranked: ScoreView[]; historical?: ScoreView[]; caption?: string; onBack?: () => void }): React.ReactElement;
export declare function RouteChoiceView(props: Base & { view: RouteView; onChoose?: (side: number) => void }): React.ReactElement;
export declare function MerchantScreenView(props: Base & { view: MerchantView; onBuy?: (index: number) => void; onSellPerk?: (id: string) => void; onSellHealth?: (actor: number) => void; onLeave?: () => void }): React.ReactElement;
export declare function BattleHudView(props: Base & { hud?: HudView; shown?: boolean; reveal?: number; operatives?: OperativeView[]; boss?: BossView | null; announcement?: string; world?: React.ReactNode; queuePinned?: boolean; onPause?: () => void; onAuto?: () => void; onSpeed?: () => void; onQueueCancel?: (entry: QueueView) => void }): React.ReactElement;
"""
# The legacy web kit's declarations move into a Legacy namespace; the components the package does not replace stay
# reachable at the top level too (window.Veilbreak.MatchHeader === window.Veilbreak.Legacy.MatchHeader).
legacy = pathlib.Path(sys.argv[2]).read_text().rstrip('\n').replace("import type * as React from 'react';\n", '')
legacy = '\n'.join(('  ' + line) if line else line for line in legacy.replace('export declare ', 'export ').split('\n'))
DTS += """

export type EffectActivation = 'active' | 'conditional' | 'ready' | 'spent' | 'expired';
export interface EffectDefinition { id: string; title: string; description: string; icon?: KitIconName; polarity?: 'neutral' | 'benefit' | 'harm'; scope?: 'actor' | 'squad' | 'run' }
export interface EffectInstance { id: string; definition: EffectDefinition; source?: string; magnitude?: string; stacks?: number; remainingSeconds?: number | null; totalSeconds?: number | null; activation?: EffectActivation }
export declare function EffectDetails(props: { view: EffectInstance; className?: string }): React.ReactElement;
export declare function EffectBadge(props: { view: EffectInstance; pinned?: boolean; onOpenChange?: (open: boolean) => void; className?: string }): React.ReactElement;
export declare function EffectList(props: { effects: EffectInstance[]; className?: string }): React.ReactElement;
export declare function EffectStrip(props: { effects: EffectInstance[]; compact?: boolean; pinned?: boolean; className?: string }): React.ReactElement;
export interface ActivePerksViewModel { wave: number; perks: PerkView[]; totals?: { label: string; value: string }[]; pick?: number }
export declare function ActivePerksView(props: { view: ActivePerksViewModel; onPick?: (index: number) => void; onBack?: () => void; className?: string }): React.ReactElement;
export declare function RevivePickView(props: { perk?: string; fallen: OperativeView[]; onPick?: (index: number) => void; onBack?: () => void; className?: string }): React.ReactElement;
export interface AccessibilityViewModel { reducedFlashes?: boolean; largerText?: boolean; highContrast?: boolean }
export declare function AccessibilityView(props: { view?: AccessibilityViewModel; onChange?: (key: string, value: boolean) => void; onBack?: () => void; className?: string }): React.ReactElement;
"""

LEGACY = (
    '\n/* ── Legacy web (deprecated): the landscape and online kit, kept for reference; classes are vbl- ── */\n'
    'export declare namespace Legacy {\n'
    + legacy
    + '\n}\n'
    + ''.join(f'export declare const {n}: typeof Legacy.{n};\n' for n in sys.argv[3:])
)
pathlib.Path(OUT, 'components', 'index.d.ts').write_text(DTS.lstrip('\n') + LEGACY)
print('ok')
