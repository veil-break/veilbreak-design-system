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
    '`text`, `effect` (in, out or slam), `className`.',
    [
        'The ordinary text remains readable; decorative copies are hidden from accessibility.',
        'Reduced Motion and Reduced Flashes use a fade. Unity also provides Glitch.In/Out/Pulse and a bounded Announcer queue.',
    ],
)
doc(
    'CreditsChip',
    'Runtime/Composites/RouteMerchant.cs',
    'Run currency with a glyph and integer.',
    '`credits` and optional `className`.',
    ['Use beside Power or in a merchant header.'],
)
doc(
    'RouteChoice',
    'Runtime/Composites/RouteMerchant.cs',
    'Two large road signs with type, glyph and reward hints.',
    '`view` (wave, left, right RoadView), `onChoose(side)`.',
    [
        'Left is 0 and right is 1; only one choice is accepted.',
        'The entire sign is a keyboard and touch target above 44 units.',
    ],
)
doc(
    'MerchantScreen',
    'Runtime/Composites/RouteMerchant.cs',
    'A market with three priced upgrades, stack sales and explicit HP trades.',
    '`view` (credits, offers, stacks, health), `onBuy(index)`, `onSellPerk(id)`, `onSellHealth(actor)`, `onLeave`.',
    [
        'The host validates every transaction and updates the view.',
        'Blocked offers say Sold, Need credits or Stack cap; HP trades show before and after.',
        'Rare is cyan, Epic is violet with a short shimmer. Reduced settings retain rarity labels and frames.',
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
    '`title` (in caps: the button does not upper-case it), `variant` (`primary`, `secondary` default, `light`, `danger`, `ghost`), `sub` (second line), `icon` (`chevron`/`play` draw at 12 px, others at 20), `blocked` (reason under five words; disables and replaces `sub`), `onClick`.',
    [
        'One `primary` per screen (START RUN, TAKE PERK). Its sub line states what will happen: "Aria · Rook · Aegis · Nyra · new seed".',
        '`light` confirms inside a sheet (ADD TO SQUAD); `danger` for removal (REMOVE FROM SQUAD); `secondary` and `danger` put the icon first, others after the label.',
        'Blocked = square dashed frame with the reason in `enemy-text` ("Pick 1 more to start", "Squad full · remove one first"). Never just dim.',
        'Min height 48 (primary 60); press scales to .97; focus shows white brackets.',
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
        '`state`: `ready` (cost in `power-text`), `aiming` (2 px teal frame, AIM), `cooldown` (fills from the bottom, seconds), `short` (dashed lavender, NEED n), `silenced` (amber strike, SIL + seconds), `fallen` (lock, OUT), `casting` (CAST), `disabled` (LOCK).',
        'Manual priority: `queued` (dashed lavender, QUEUE #n, matching its QueueChip), `buffered` (NEXT and the cost: fires the moment the operative is ready), `windup` (a sweep from `progress`, tick-exact, with seconds).',
        'Feedback: `invalid` (struck no-target glyph and the `reason`, "Out of range"), `rejected` (one 400 ms crimson flash on entering it), `exposed` (EXPOSED inverted over the cost).',
        'The game owns input; the button only draws. Its tooltip reads "Needle Storm · 18 Power · 40% ready".',
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
    'OperativeCard',
    'Runtime/Composites/Operatives.cs (OperativeCard)',
    'The 88 × 172 portrait battle card: face, control mode, health/barrier/stability, statuses and two skills, or FALLEN once out.',
    '`view`: an OperativeView `{name, code, kit (CSS colour, e.g. var(--kit-aria)), portrait (image URL), hp, maxHp, barrier, stability, maxStability, statuses: [kind…], mode ("auto"|"manual"|"aiming"|"fallen"), manualSeconds, skills: [SkillView, SkillView]}`. Optional `dragging` (skill index), `onSkillPointerDown(index, event)`.',
    [
        '`portrait` is a 2:3 bust render; the band crops it to the face. Without one the band shows `scene-fog`.',
        'Mode chip: AUTO (teal on dark), MANUAL 2.9s (white), AIMING (teal fill; the card gets a 2 px teal frame and brackets), FALLEN.',
        'Four cards fill a 390 px portrait row at the bottom of the battle screen.',
    ],
)
doc(
    'OperativeTile',
    'Runtime/Composites/Operatives.cs (OperativeTile)',
    'Roster tile: portrait, name, role and, when picked, its squad slot number.',
    '`view` (OperativeView: `name`, `role`, `roleIcon`, `kit`, `portrait`), `slot` (1–4, 0 = not in squad), `focused`, `tier` (`none`, `blue`, `violet`, `gold`), `locked` (how to get it: "Pull in rifts"), `onPick`.',
    [
        'In-squad tiles take the dark teal fill; the focused one a 2 px teal frame and brackets.',
        'Tier colours frame the tile (gold adds a glow); a locked tile stays visible with a lock and its reason, and cannot join the squad.',
    ],
)
doc(
    'SquadSlot',
    'Runtime/Composites/Operatives.cs (SquadSlot)',
    'One of the four squad slots on the selection screen: the operative, or a dashed empty slot.',
    '`view` (OperativeView or `null` for empty), `slot` (1–4), `onPick`.',
    ['Empty slots are square and dashed with SLOT n; filled ones show the slot number on the portrait.'],
)
doc(
    'OperativeDetails',
    'Runtime/Composites/Operatives.cs (OperativeDetails)',
    'The 366 px operative sheet: stats, passive, both skills with cost and cooldown, close and add/remove.',
    '`view` (OperativeView incl. `armor`, `passive`, `passiveText`, skill `description`/`cooldownSeconds`), `slot` (0 = not in squad), `squadFull`, `maxHp`/`maxArmor`/`maxStability` (bar scales: 1580, 26, 140), `onClose`, `onToggle`.',
    [
        'The toggle is ADD TO SQUAD (light) or REMOVE FROM SQUAD (danger); adding to a full squad is blocked with "Squad full · remove one first".'
    ],
)
doc(
    'OperativeStats',
    'Runtime/Composites/Operatives.cs (OperativeStats)',
    "One operative's post-run contribution: share of squad damage and six figures.",
    '`view`: a ContributionView `{operative, fellWave (0 = standing), damage, kills, casts, power, share (0–1), extraLabel, extraValue}`.',
    [
        "The share bar takes the operative's kit colour; FELL WAVE n in `enemy-text`, STANDING in `ally-text`.",
        "Usually SquadResult's popover content.",
    ],
)
doc(
    'SquadResult',
    'Runtime/Composites/Operatives.cs (SquadResult)',
    'Results cell for one operative; hover, focus or tap shows its OperativeStats above it.',
    '`view` (ContributionView), `pinned` (keep the popover open).',
    ['Fallen operatives are struck through with FELL Wn; the kit colour runs along the bottom edge.'],
)
doc(
    'PerkCard',
    'Runtime/Composites/Perks.cs (PerkCard)',
    'Draft card (one of three): art, tier tag, name, effect, stack pips and the value after the pick.',
    '`view`: a PerkView `{name, category, tag, tier ("general"|"operative"|"rare"), icon, art (image URL, operative perks), effect, now, next, stack, max}`; `selected`; `onPick`.',
    [
        "Tiers: general (grey rule), operative (teal tag and rule, the operative's portrait as art), rare (violet frame and tag).",
        'Selected = dark teal fill, 2 px teal frame, brackets and a check. Owned pips are grey, the next one teal.',
    ],
)
doc(
    'PerkDetail',
    'Runtime/Composites/Perks.cs (PerkDetail)',
    'Full perk explanation: draft (now → after pick) or owned (active value, stacks, waves picked).',
    '`view` (PerkView incl. `waves`), `draft`, optional `style` width (popovers use 260).',
    ['The value that improves reads in `ally-text`.', "Shown inline under the draft cards and as PerkChip's popover."],
)
doc(
    'PerkChip',
    'Runtime/Composites/Perks.cs (PerkChip)',
    'Compact owned-perk chip; hover, focus or tap shows its PerkDetail.',
    "`view` (PerkView), `alignEnd` (popover aligns to the chip's right edge), `pinned`.",
    ['Stacks above one show ×n in `ally-text`; operative perks tint the glyph teal, rare perks frame violet.'],
)
doc(
    'PerkRow',
    'Runtime/Composites/Perks.cs (PerkRow)',
    'Active-perk list row: art, name, tag, effect, current value and stacks.',
    '`view` (PerkView), `selected`, `onPick`.',
    ['The effect ellipsizes; the current value sits right in `ally-text` over its stacks.'],
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
    ('ProgressBar', 'Runtime/Controls/Progress.cs', 'The one progress bar: continuous, or `segments` equal levels.', '`segments` (0 = continuous), `value` (levels, or 0–1), `next` (outline the level a pick adds), `tone` (ally, power, enemy, neutral).', ['Perk levels, stacks and steps are segments; health and progress are continuous.']),
    ('KeyValueRow', 'Runtime/Controls/Progress.cs', 'One key/value line: a caption key, a value on the right.', '`label`, `value`, `good` (ally colour).', ['Perk details, the results ledger and fact lists use it.']),
    ('ListRow', 'Runtime/Controls/Progress.cs', 'The one list row: glyph, caps title, caption, optional progress bar, trailing value and action.', '`icon`, `title`, `caption`, `bar` (ProgressBar props), `value`, `action`, `selected`, `onPick`.', ['Merchant offers, stack sales and health trades are ListRows; put rows in a ListLayout.']),
    ('TextComponent', 'Runtime/Controls/Text.cs', 'Every run of text: one type style and an optional tone.',
     '`typeStyle` (display, title, label, badge, body, caption, num), `tone` (ally, enemy, power, muted), `text`.',
     ['One type style per run; tones only recolour.', 'Pass caps yourself, or use LabelComponent.']),
    ('LabelComponent', 'Runtime/Controls/Text.cs', 'The caps eyebrow over a section, field or value, with 4 units under it.',
     '`text` (uppercased for you), `size` (label or badge), `tone`.', ['A label names a thing; it is never a sentence.']),
    ('BadgeComponent', 'Runtime/Controls/Badge.cs', 'A small slim chip: a word, an optional glyph before it and a value after it.',
     '`text`, `icon`, `value`, `tone` (neutral, ally, enemy, power, warn, inverted), children.',
     ['StatusChip and CreditsChip are the status and currency presets (`Badge.Status`, `Badge.Credits` in Unity).', 'Colour never travels alone: a tone always comes with a word.']),
    ('IconButtonComposite', 'Runtime/Controls/IconButton.cs', 'A 44 × 44 chipped square with one glyph or a short state, and a tooltip.',
     '`icon` or `text`, `tooltip` (required: it is the accessible name), `active`, `quiet`, `size`, `onClick`.',
     ['Back, pause, close, speed and autoplay are all this control.', '`quiet` is the translucent HUD look over the world; `active` is toggled on.']),
    ('TileComponent', 'Runtime/Controls/Tile.cs', 'A pickable cell: picture or glyph, name, caption with its glyph, order and state.',
     '`title`, `caption`, `captionIcon`, `picture`, `glyph`, `kit`, `order`, `state` (in, focused, locked, empty; in and focused combine), `tier`, `emptyLabel`, `onPick`, `block`.',
     ['OperativeTile and SquadSlot are tiles (`block` vb-tile, vb-slot).', 'Locked tiles stay visible and say how to unlock; lay tiles out in a grid.']),
    ('CardComponent', 'Runtime/Controls/Card.cs', 'A slim container with an optional media band, a head, a body and a foot of actions.',
     '`title`, `meta`, `media`, `actions`, `selected`, children (the body).', ['Cards are slim; only a selected card is chipped (edge and brackets).', 'Foot actions are right-aligned.']),
    ('DrawerOverlay', 'Runtime/Composites/Drawer.cs', 'A bottom drawer: a slim sheet that slides up over a backdrop to half the screen; drag its top up for full screen or down to close.',
     '`open` (default true), `snap` (half or full, where it opens), `onSnap`, `title`, `meta`, `backdrop` (false docks it inside a view), `dismissible`, `closeButton`, `actions`, `onDismiss`, children (the body).',
     ['Drag the grip or head: up snaps to full screen, down back to half, below a quarter of the screen closes it. A tap on the grip toggles half and full.',
      'The backdrop and × dismiss it too; `onDismiss` fires, then it slides away. SquadView opens operative details in one.',
      'Unity: `DrawerOverlay.Push(nav, title, closed)` puts one on a modal layer that Back pops.']),
    ('FieldComposite', 'Runtime/Composites/Forms.cs', 'One setting: a caps label, its input and one line of what it does.',
     '`label`, `help`, children (the input).', ['Use it for inputs without their own label; VbSlider and VbSwitch carry theirs.']),
    ('Fieldset', 'Runtime/Core/Ui.cs (Ui.Section)', 'A titled group of related fields in one slim container.',
     '`title`, children (fields).', ['One topic per fieldset, 1–4 fields.']),
    ('FormComposite', 'Runtime/Composites/Forms.cs', 'Fieldsets in a scroll, an optional note, a footer of secondary actions and a saved line.',
     '`saved`, `headed` (the saved state shows in a ScreenHeader instead), `note` ({ icon, text }), `footer`, children (fieldsets).',
     ['Settings apply as they change; there is no Submit.', 'Destructive actions live in the footer as danger buttons and confirm.']),
    ('StackLayout', 'Runtime/Layouts/Layouts.cs', 'A column with one rhythm: the gap under every child but the last.', '`gap` (4, 8, 12 or 16), children.', ['Use it instead of per-element margins.']),
    ('VbGrid', 'Runtime/Layouts/Layouts.cs', 'Cells 2, 3 or 4 across; `wrap` adds the row rhythm.', '`columns` (2, 3 or 4), `wrap`, children.', ['Pick the count the items fill; never a half-empty row.']),
    ('ListLayout', 'Runtime/Layouts/Layouts.cs', 'Rows of one kind in a scroll, 6 between rows, with an empty-state line.', '`empty`, children (rows).', ['ScoreRow, PerkRow and RuleRow carry no margin of their own; put them in a list.']),
    ('BoardLayout', 'Runtime/Layouts/Layouts.cs', 'The battle board: top band, the world (the rest, at least half) and the bottom band.', '`top`, `world`, `bottom`.', ['Top and bottom never shrink.']),
    ('MainMenuView', 'Runtime/Views/MenuViews.cs', 'The title screen: offline badge and best, the mark, the menu actions and EXIT.',
     '`view` (status, subtitle, best, bestWave, checkpoint, squad, notice, canExit), `onPlay`, `onResume`, `onScores`, `onSettings`, `onExit`.', ['PLAY is the one primary.']),
    ('MenuActionsSectionView', 'Runtime/Views/MenuViews.cs', 'The thumb-zone block: checkpoint card, PLAY, high scores and settings.',
     '`checkpoint` (wave, score, standing, squadSize, perks, seed) or null, `squadLine`, `notice`, `onPlay`, `onResume`, `onScores`, `onSettings`.',
     ['RESUME is secondary and lives in the checkpoint card.', 'The card only exists when there is a checkpoint.']),
    ('SquadView', 'Runtime/Views/SquadView.cs', 'Pick four: squad slots, the roster grid, the open details and START RUN.',
     '`view` (roster, selected, focus, locked, tiers, seed), `onPick(i)`, `onSlotPick(i)`, `onToggle(i)`, `onClose`, `onStart`, `onBack`.', ['START RUN is blocked with its reason until the squad has four.']),
    ('SettingsView', 'Runtime/Views/SettingsView.cs', 'Audio, feel, camera and quality as fieldsets in one form; applies as it changes.',
     '`view` (master, music, effects, sensitivity, haptics, reducedMotion, camera, quality, saved, note), `onChange(model)`, `onClearScores`, `onAccessibility`, `onBack`.', ['Every change reports the whole model.']),
    ('PauseOverlayView', 'Runtime/Views/OverlayViews.cs', 'The run, frozen: the screen header (its back square resumes), then one overlay panel centred on the screen.',
     '`view` (wave, score, standing, squadSize, perks, seed, detail), `onResume`, `onSettings`, `onPerks`, `onAbandon`, `onExit`.', ['RESUME is the one primary; the header back square and Back resume too.', 'ABANDON is danger and confirms; EXIT TO MENU is a ghost at the bottom of the panel.']),
    ('ConfirmDialogView', 'Runtime/Views/OverlayViews.cs', 'One shape for every confirmation: question, consequence, destructive action, way back.',
     '`title`, `body`, `confirm` ({ label, icon, sub, variant }), `cancel` ({ label, icon }), `onConfirm`, `onCancel`.', ['Unity: `ConfirmDialogView.Push(nav, …)` puts it on a modal layer.']),
    ('GuideDialogView', 'Runtime/Views/OverlayViews.cs', 'The first-run guide: numbered steps and READY.', '`title`, `steps`, `onReady`.', ['Unity: `GuideDialogView.Push(nav, title, steps, ready)`.']),
    ('PerkDraftView', 'Runtime/Views/RunViews.cs', 'Wave cleared: three perk cards, the pick explained, the squad that carries forward and TAKE PERK.',
     '`view` (wave, bonus, score, choices, pick, squad: code, hp, maxHp, stability), `onPick(i)`, `onTake`.', ['TAKE PERK says what the pick does next.']),
    ('ResultsView', 'Runtime/Views/RunViews.cs', 'Run over: the score and its ledger, stats, the squad\'s contribution, the perks, MENU and RUN AGAIN.',
     '`view` (cause, score, wave, bosses, rank, of, ledger, distance, simTime, squad, perks), `onMenu`, `onAgain`.', ['RUN AGAIN is the one primary.']),
    ('HighScoresView', 'Runtime/Views/RunViews.cs', 'Top 20 on this device with the current run framed, then older-rules runs.',
     '`ranked`, `historical`, `caption`, `onBack`.', ['Local scores only; say so in the footer.']),
    ('RouteChoiceView', 'Runtime/Views/BattleViews.cs', 'The crossroads screen around RouteChoice.', '`view` (RouteView), `onChoose(side)`.', ['Only one choice is accepted.']),
    ('MerchantScreenView', 'Runtime/Views/BattleViews.cs', 'The night market screen (design name MerchantView) around MerchantScreen.',
     '`view` (MerchantView), `onBuy`, `onSellPerk`, `onSellHealth`, `onLeave`.', ['The host confirms every trade and binds again.']),
    ('BattleHudView', 'Runtime/Views/BattleViews.cs', 'The whole battle board: HUD band, boss bar, announcer, the world and four operative cards.',
     '`hud` (HudView), `operatives`, `boss` (or null), `announcement`, `world`, `onPause`, `onAuto`, `onSpeed`, `onQueueCancel`.', ['Unity: bind it every frame; the game attaches its aim drag to `Card(i).Skill(n)`.']),
]:
    doc(name, cs, summary, provide, rules)

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
export interface OperativeView { id?: string; name: string; code: string; role?: string; roleIcon?: KitIconName; passive?: string; passiveText?: string; passiveFullText?: string; kit: string; portrait?: string | null; hp: number; maxHp: number; barrier?: number; armor?: number; stability?: number; maxStability?: number; statuses?: StatusKind[]; mode?: ControlMode; manualSeconds?: number; skills?: SkillView[] }
export interface PerkView { id?: string; name: string; category?: string; tag?: string; icon?: KitIconName; effect?: string; now?: string; next?: string; tier?: PerkTier; art?: string | null; stack?: number; max?: number; waves?: number[] }
export interface QueueView { who: string; skill: string; cost: number; eta: string; actor?: number; slot?: number; manual?: boolean; next?: boolean }
export interface HudView { credits?: number; speed?: number; wave?: number; score?: number; boss?: boolean; auto?: boolean; slow?: boolean; power?: number; maxPower?: number; regen?: string; hint?: string | null; focus?: number; previewFrom?: number; previewTo?: number; queue?: QueueView[]; queueTitle?: string; queueHint?: string }
export interface BossView { name: string; health: number; stability: number; detail?: string; shielded?: boolean; interruptible?: boolean; shieldDetail?: string }
export interface ScoreView { rank: number; score: number; wave: number; bosses: number; squad: string[]; current?: boolean }
export interface ContributionView { operative: OperativeView; fellWave?: number; damage?: number; kills?: number; casts?: number; power?: number; share?: number; extraLabel?: string; extraValue?: string }

export interface VbFrameProps { kind?: 'slim' | 'chipped'; className?: string; style?: React.CSSProperties; children?: React.ReactNode }
export declare function VbFrame(props: VbFrameProps): React.ReactElement;
export interface VbIconProps { icon: KitIconName; size?: number; title?: string; className?: string; style?: React.CSSProperties }
export declare function VbIcon(props: VbIconProps): React.ReactElement;
export interface VbMarkProps { width?: number; className?: string; style?: React.CSSProperties }
export declare function VbMark(props: VbMarkProps): React.ReactElement;
export interface VbButtonProps { title: string; variant?: 'primary' | 'secondary' | 'light' | 'danger' | 'ghost'; sub?: string; icon?: KitIconName; blocked?: string; onClick?: () => void; className?: string; style?: React.CSSProperties }
export declare function VbButton(props: VbButtonProps): React.ReactElement;
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
export interface RoadView { kind?: string; title: string; icon?: KitIconName; hint: string }
export interface RouteView { wave: number; left: RoadView; right: RoadView }
export interface MerchantOfferView { perk: PerkView; price: number; taken?: boolean; blocked?: string }
export interface MerchantStackView { perk: PerkView; payout: number }
export interface MerchantHealthView { actor: number; name: string; hp: number; maxHp: number; cost: number; payout: number; blocked?: string }
export interface MerchantView { credits: number; offers: MerchantOfferView[]; stacks: MerchantStackView[]; health: MerchantHealthView[] }
export declare function GlitchText(props: { text: string; effect?: 'in' | 'out' | 'slam'; className?: string; style?: React.CSSProperties }): React.ReactElement;
export declare function CreditsChip(props: { credits: number; className?: string }): React.ReactElement;
export declare function RouteChoice(props: { view: RouteView; onChoose?: (side: number) => void; className?: string; style?: React.CSSProperties }): React.ReactElement;
export declare function MerchantScreen(props: { view: MerchantView; onBuy?: (index: number) => void; onSellPerk?: (id: string) => void; onSellHealth?: (actor: number) => void; onLeave?: () => void; className?: string; style?: React.CSSProperties }): React.ReactElement;
export interface HudTopProps { view: HudView; showFocus?: boolean; queuePinned?: boolean; onAutoClick?: () => void; onSpeedClick?: () => void; onPauseClick?: () => void; onQueueCancel?: (entry: QueueView) => void; className?: string; style?: React.CSSProperties }
export declare function HudTop(props: HudTopProps): React.ReactElement;
export interface QueueListProps { queue: QueueView[]; title?: string; hint?: string; onCancel?: (entry: QueueView) => void; className?: string }
export declare function QueueList(props: QueueListProps): React.ReactElement;
export interface BossBarProps { view: BossView; className?: string }
export declare function BossBar(props: BossBarProps): React.ReactElement;
export interface ScreenHeaderProps { title: string; meta?: string; accent?: boolean; backLabel?: string; onBack?: () => void; className?: string }
export declare function ScreenHeader(props: ScreenHeaderProps): React.ReactElement;
export interface ScoreRowProps { view: ScoreView; className?: string }
export declare function ScoreRow(props: ScoreRowProps): React.ReactElement;
export interface OperativeCardProps { view: OperativeView; dragging?: number; onSkillPointerDown?: (index: number, e: React.PointerEvent) => void; className?: string; style?: React.CSSProperties }
export declare function OperativeCard(props: OperativeCardProps): React.ReactElement;
export interface OperativeTileProps { view: OperativeView; slot?: number; focused?: boolean; tier?: RarityTier; locked?: string; onPick?: () => void; className?: string }
export declare function OperativeTile(props: OperativeTileProps): React.ReactElement;
export interface SquadSlotProps { view: OperativeView | null; slot: number; onPick?: () => void; className?: string }
export declare function SquadSlot(props: SquadSlotProps): React.ReactElement;
export interface OperativeDetailsProps { view: OperativeView; slot?: number; squadFull?: boolean; maxHp?: number; maxArmor?: number; maxStability?: number; onClose?: () => void; onToggle?: () => void; className?: string; style?: React.CSSProperties }
export declare function OperativeDetails(props: OperativeDetailsProps): React.ReactElement;
export interface OperativeStatsProps { view: ContributionView; className?: string }
export declare function OperativeStats(props: OperativeStatsProps): React.ReactElement;
export interface SquadResultProps { view: ContributionView; pinned?: boolean; className?: string }
export declare function SquadResult(props: SquadResultProps): React.ReactElement;
export interface PerkCardProps { view: PerkView; selected?: boolean; onPick?: () => void; className?: string; style?: React.CSSProperties }
export declare function PerkCard(props: PerkCardProps): React.ReactElement;
export interface PerkDetailProps { view: PerkView; draft?: boolean; className?: string; style?: React.CSSProperties }
export declare function PerkDetail(props: PerkDetailProps): React.ReactElement;
export interface PerkChipProps { view: PerkView; alignEnd?: boolean; pinned?: boolean; className?: string }
export declare function PerkChip(props: PerkChipProps): React.ReactElement;
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
export interface WorldLabelProps { kind?: 'aim' | 'name' | 'telegraph' | 'estimate' | 'blocked'; title: string; detail?: string; team?: WorldTeam; showDetail?: boolean; className?: string; style?: React.CSSProperties }
export declare function WorldLabel(props: WorldLabelProps): React.ReactElement;
export interface WorldMarkerProps { kind?: 'ally' | 'enemy' | 'selected' | 'queued' | 'move'; size?: number; className?: string; style?: React.CSSProperties }
export declare function WorldMarker(props: WorldMarkerProps): React.ReactElement;
export interface OffscreenIndicatorProps { name: string; team?: WorldTeam; metres?: number; threat?: boolean; angle?: number; className?: string; style?: React.CSSProperties }
export declare function OffscreenIndicator(props: OffscreenIndicatorProps): React.ReactElement;
export interface EdgeWarningProps { caption?: string; urgency?: number; top?: boolean; className?: string; style?: React.CSSProperties }
export declare function EdgeWarning(props: EdgeWarningProps): React.ReactElement;
/* ── 0.3.0: the handbook's atoms, composites, layouts and views ── */
type Base = { className?: string; style?: React.CSSProperties };
export type TypeStyle = 'display' | 'title' | 'label' | 'badge' | 'body' | 'caption' | 'num';
export type TextTone = 'default' | 'ally' | 'enemy' | 'power' | 'muted';
export declare function ProgressBar(props: Base & { segments?: number; value?: number; next?: boolean; tint?: string; tone?: 'ally' | 'power' | 'enemy' | 'neutral' }): React.ReactElement;
export declare function KeyValueRow(props: Base & { label: string; value: string; good?: boolean }): React.ReactElement;
export declare function ListRow(props: Base & { icon?: KitIconName; title: string; caption?: string; bar?: { segments?: number; value?: number; next?: boolean; tone?: string } | null; value?: string; action?: React.ReactNode; selected?: boolean; onPick?: () => void }): React.ReactElement;
export declare function TextComponent(props: Base & { text?: React.ReactNode; typeStyle?: TypeStyle; tone?: TextTone; children?: React.ReactNode }): React.ReactElement;
export declare function LabelComponent(props: Base & { text: string; size?: 'label' | 'badge'; tone?: TextTone }): React.ReactElement;
export type BadgeTone = 'neutral' | 'ally' | 'enemy' | 'power' | 'warn' | 'inverted';
export declare function BadgeComponent(props: Base & { text?: string; icon?: KitIconName; value?: string | number; tone?: BadgeTone; tooltip?: string; children?: React.ReactNode }): React.ReactElement;
export declare function IconButtonComposite(props: Base & { icon?: KitIconName; text?: string; tooltip: string; active?: boolean; quiet?: boolean; size?: number; textClass?: string; onClick?: () => void }): React.ReactElement;
export type TileState = 'in' | 'focused' | 'locked' | 'empty';
export declare function TileComponent(props: Base & { title?: string; caption?: string; captionIcon?: KitIconName; picture?: string | null; glyph?: KitIconName; kit?: string; order?: number; state?: TileState | TileState[]; tier?: RarityTier; emptyLabel?: string; tooltip?: string; block?: string; onPick?: () => void }): React.ReactElement;
export declare function CardComponent(props: Base & { title?: string; meta?: string; media?: React.ReactNode; actions?: React.ReactNode; selected?: boolean; children?: React.ReactNode }): React.ReactElement;
export declare function DrawerOverlay(props: Base & { open?: boolean; snap?: 'half' | 'full'; onSnap?: (snap: 'half' | 'full') => void; title?: string; meta?: string; label?: string; backdrop?: boolean; dismissible?: boolean; closeButton?: boolean; actions?: React.ReactNode; onDismiss?: () => void; children?: React.ReactNode }): React.ReactElement;
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
export interface SquadViewModel { roster: OperativeView[]; selected?: number[]; focus?: number; locked?: string[]; tiers?: RarityTier[]; seed?: string }
export declare function SquadView(props: Base & { view: SquadViewModel; onPick?: (index: number) => void; onSlotPick?: (slot: number) => void; onToggle?: (index: number) => void; onClose?: () => void; onStart?: () => void; onBack?: () => void }): React.ReactElement;
export interface SettingsViewModel { master?: number; music?: number; effects?: number; sensitivity?: number; haptics?: boolean; reducedMotion?: boolean; camera?: number; quality?: number; saved?: string; note?: string }
export declare function SettingsView(props: Base & { view?: SettingsViewModel; onChange?: (model: SettingsViewModel) => void; onClearScores?: () => void; onAccessibility?: () => void; onBack?: () => void }): React.ReactElement;
export interface PauseViewModel { wave: number; score: number; standing: number; squadSize?: number; perks?: PerkView[]; seed?: string; detail?: string }
export declare function PauseOverlayView(props: Base & { view: PauseViewModel; onResume?: () => void; onSettings?: () => void; onPerks?: () => void; onAbandon?: () => void; onExit?: () => void }): React.ReactElement;
export declare function ConfirmDialogView(props: Base & { title: string; body?: string; confirm?: { label: string; icon?: KitIconName; sub?: string; variant?: VbButtonProps['variant'] }; cancel?: { label: string; icon?: KitIconName }; onConfirm?: () => void; onCancel?: () => void }): React.ReactElement;
export declare function GuideDialogView(props: Base & { title?: string; steps?: string[]; onReady?: () => void }): React.ReactElement;
export interface SquadHealthView { code: string; hp: number; maxHp: number; stability?: number }
export interface PerkDraftViewModel { wave: number; bonus?: number; score?: number; choices: PerkView[]; pick?: number; squad?: SquadHealthView[] }
export declare function PerkDraftView(props: Base & { view: PerkDraftViewModel; onPick?: (index: number) => void; onTake?: () => void }): React.ReactElement;
export interface ResultsViewModel { cause?: string; score: number; wave: number; bosses?: number; rank?: number; of?: number; ledger?: { label: string; value: string }[]; distance?: string; simTime?: string; squad?: ContributionView[]; perks?: PerkView[] }
export declare function ResultsView(props: Base & { view: ResultsViewModel; onMenu?: () => void; onAgain?: () => void }): React.ReactElement;
export declare function HighScoresView(props: Base & { ranked: ScoreView[]; historical?: ScoreView[]; caption?: string; onBack?: () => void }): React.ReactElement;
export declare function RouteChoiceView(props: Base & { view: RouteView; onChoose?: (side: number) => void }): React.ReactElement;
export declare function MerchantScreenView(props: Base & { view: MerchantView; onBuy?: (index: number) => void; onSellPerk?: (id: string) => void; onSellHealth?: (actor: number) => void; onLeave?: () => void }): React.ReactElement;
export declare function BattleHudView(props: Base & { hud?: HudView; operatives?: OperativeView[]; boss?: BossView | null; announcement?: string; world?: React.ReactNode; queuePinned?: boolean; onPause?: () => void; onAuto?: () => void; onSpeed?: () => void; onQueueCancel?: (entry: QueueView) => void }): React.ReactElement;
"""
# The legacy web kit's declarations move into a Legacy namespace; the components the package does not replace stay
# reachable at the top level too (window.Veilbreak.MatchHeader === window.Veilbreak.Legacy.MatchHeader).
legacy = pathlib.Path(sys.argv[2]).read_text().rstrip('\n').replace("import type * as React from 'react';\n", '')
legacy = '\n'.join(('  ' + line) if line else line for line in legacy.replace('export declare ', 'export ').split('\n'))
LEGACY = (
    '\n/* ── Legacy web (deprecated): the landscape and online kit, kept for reference; classes are vbl- ── */\n'
    'export declare namespace Legacy {\n'
    + legacy
    + '\n}\n'
    + ''.join(f'export declare const {n}: typeof Legacy.{n};\n' for n in sys.argv[3:])
)
pathlib.Path(OUT, 'components', 'index.d.ts').write_text(DTS.lstrip('\n') + LEGACY)
print('ok')
