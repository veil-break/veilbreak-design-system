# Veilbreak

Veilbreak Arcade is a portrait squad survival game for phones. Four of eight operatives hold a ruined boulevard against
endless waves. Autoplay moves and casts for them, and the player steps in with aimed skills, a shared Power pool and
focus-powered slow motion. This system is its interface: the Veil theme, the Unity package `com.veilbreak.gui` that
draws it, and this web mirror of the same package.

The identity comes from Pastel Black: the void, the teal → lavender → crimson range and the fonts. The web brand's
blur and rounded softness are swapped for what a thumb on a 6-inch screen needs in a live fight. That means cut
corners, solid panels, big targets, a strict glow budget, and a translucent HUD band over a 3D street that never
hides the fight.

The design goal is legible tactics on a small screen. At every moment the player knows three things:

- what they can afford;
- what is about to hit their squad;
- why the last wave went the way it did.

Every rule below serves that goal first and the sci-fi mood second.

## Principles

- **The street is the hero.** At least half of the portrait board stays battlefield. The HUD lives in two bands:
  - read-only state along the top;
  - the squad's cards and skills in the thumb zone at the bottom.

  Nothing floats over the middle except world readouts tied to a fighter. See *Portrait HUD layout*.
- **Every choice shows its cost.** Power costs appear before you commit:
  - on the skill button;
  - in the aim label;
  - as a mark on the Power strip.

  A manual cast that cannot fire yet joins the visible queue with its place number. A player never discovers a cost
  by failing.
- **Every blocked state reads differently.** There is never one grey "disabled" button:
  - short on Power is dashed;
  - cooldown is a draining charge with seconds;
  - silenced carries the status word;
  - an invalid aim strikes the target glyph and names the reason;
  - a refused command flashes once and says why.
- **Colour never travels alone.** Allies are teal circles and enemies crimson diamonds. Queued things are lavender
  and dashed. Every status has its own glyph and word. The high-contrast theme and colour-blind players read the same
  information from shape.
- **Diegetic, not decorative.** Holo effects mark *where information lives* and *what just changed*: brackets,
  scanlines and the flicker-in. If an effect could be switched off without losing information, it must be subtle
  enough to be. If it carries information, it is a state, not an effect.
- **The player's command wins.** Autoplay uses the same queue, the same costs and the same refusals as the player. A
  manual cast always goes before an autocast, and the queue shows it.

## Content fundamentals

**Voice.** A tactical readout, not a narrator. Short, factual, present tense. The UI states what is true and what will
happen:

- "Needle Storm queued · #2"
- "Release to cast"
- "Release here to cancel · no Power spent"
- "Need 6"

No exclamation marks, no jokes, no "Oops". Hints are imperative and start with the verb: "Tap an operative to
inspect", "Drag to aim".

**Casing.** Names, labels, buttons and statuses are set in caps by the type styles and the components. They are never
typed in caps in the data: store "Aria" and render ARIA. Explanations and hints are sentence case. Operative codes are
two letters: AR, SO, RO, HE, AE, MI, EC, NY.

**Numbers.** Always tabular figures.

- Costs and changes use a true minus and the arrow: "−18 → 16 left", "+12/s", "×1.25".
- Time is in seconds, with one decimal under 10 s ("0.8s", "7.0s"), and in clock format from a minute up ("1:12").
  `Ui.Seconds` formats it.
- Power reads "34/100". Scores from 10,000 up are compact ("18.2K"), from `Ui.Compact`.

**Separators.** The middle dot joins facts on one line ("Striker · AR", "18 POWER · queues as #3"). Arrows (→) mean
"becomes" or "leads to". The slash appears only in values ("34/100") and the wordmark.

**Reasons.** Anything refused says why in under five words: "Out of range", "No target", "Need 6", "Still on cooldown",
"Silenced", "Queue full".

**No emoji, ever.** Glyphs come from the icon set, and status meaning from glyph plus word.

**Language.** English. Text fields leave about 30% room for other languages, so a label never sits in a fixed-width box
without space to grow.

## Visual foundations

### Colour

The palette is Pastel Black's, with game meanings. Components use semantic tokens. Primitives (`teal-500`,
`crimson-400` …) appear only inside token aliases.

- **Ground and panels.** Everything sits on `ground`, the void (`dark-300`). Panels are `panel`: translucent, never
  blurred, so the street reads faintly through without fighting the text.
- **Text.** `text` goes on `panel`, `raised` and `ground`. Use `text-2` for secondary lines and `text-3` for hints and
  units. Nothing on a panel goes below `text-3`.
- **Sides.** `ally`, `ally-text` and `ally-soft` are your squad. `enemy`, `enemy-text` and `enemy-soft` are the
  hostiles. Use `enemy` as text only on `panel`; on `raised`, use `enemy-text`.
- **Power.** `power`, `power-text` and `power-soft` mean the shared resource and nothing else. `queued` is the same
  hue, always dashed.
- **The CTA.** The `cta` → `cta-2` gradient is the one glowing button per screen (START RUN, CONTINUE), with its label
  in `on-cta`. A second action on the same screen is a secondary button.
- **Signals.**

  | Token | Meaning |
  | --- | --- |
  | `warn` | Amber cautions |
  | `barrier` | Pale cyan shields |
  | `heal` | Light teal |
  | `exposed` | An inverted chip |
  | `focus` | The keyboard and selection frame |
  | `signal-pink` | The intro splash only |

- **Rarity** is named by the colour players learn:

  | Token | Tier |
  | --- | --- |
  | `tier-blue` | Rare perks and fighters |
  | `tier-violet` | Epic |
  | `tier-gold` | Legendary |
  | `tier-grey` | Common items |
  | `tier-lime` | Set items, always with the set glyph |

  The tier word always travels with the colour.
- **Kit tints.** `kit-aria` … `kit-nyra` belong to the operatives as characters: card accents, skill art and lineup
  rims. They never carry team meaning. In battle every gameplay mark is `ally` or `enemy`, whichever kit cast it.
- **Scene tints.** `scene-*` and `holo` colour the 3D street: fog, neon, rim light and holographic lines.

### Type

- **TeX Gyre Adventor Bold, caps, tracked out:** titles, operative names and CTAs. Styles `display-xl`, `display`,
  `title`, `cta-label`, `name` and `code`.
- **Poppins:** everything else.
  - `clock` and `numeral` for numbers, always tabular.
  - `label` and `badge` for caps labels.
  - `ui` for live HUD text.
  - `body` and `caption` for prose.
  - `readout-label`, `readout` and `readout-num` for stat readouts.
- **Share Tech Mono:** only status timers.
- **Limits:**
  - Nothing below Regular weight.
  - Nothing below 10 units (`badge`).
  - Text directly over the street gets the `text-halo` shadow.
- **Larger text** (`vb-large-text`) raises body to 15 and caption to 13.
- **Never** set Poppins at display sizes or Adventor in body text.

### Shape

- **Cut corners, not rounded ones.** Cut only the top-left and bottom-right corners:
  - `cut-sm` for chips and cells;
  - `cut-md` for buttons, cards and panels;
  - `cut-lg` for sheets and the CTA.

  Everything else is square. Circles appear only where they *mean* something: ally markers and selection rings.
- **Brackets** go on the two square corners (top-right, bottom-left). They are `bracket-length` L-marks in the frame
  colour, on selected, focused, aiming and holographic elements.
- **Frames** are a hairline in `line`. When selected, focused or team-tagged, they become 2 units in a meaning colour.
  Focus is a 2-unit `focus` frame, solid, at least 3:1 on every surface.

In Unity, `VbShape` paints every cut shape from `--vb-*` USS properties. On the web, the mirror paints the same polygon
as SVG from the same properties.

### Surfaces

- Use `panel` for HUD panels and `panel-solid` for popovers, sheets and dialogs. Idle controls on a panel are `raised`.
  Each `*-soft` tint sits only behind its own text colour.
- Scanlines (`scan-pitch`, `scan-alpha`) appear only on holographic surfaces: holo frames, lineup plates and rift card
  backs. Never put them under live battle numbers.
- Dashes mean blocked, short, queued, a preview or empty. Hatch means danger on the ground, drawn by the 3D telegraphs.
  Neither is ever decorative.

### Glow budget

Glows go on **the CTA and the selected or acting thing only**:

- the aiming skill and its operative;
- the focused tile;
- the selected perk;
- a Legendary reveal;
- the one-shot flash of a refused command.

A screen with three glowing objects is broken. Glows switch off in high contrast and with reduced flashes, and nothing
depends on them.

### States

- **Press** eases over `dur-press`.
- **Hover** (pointer only) lifts the frame from `line` to `line-strong`.
- **Selected** is a 2-unit frame plus brackets plus glow.
- **State changes** ease over `dur-ui`.
- **Disabled** is never just dimmed: it carries its reason.
- **Simulation data never animates on its own.** Sweeps, cooldown fills and health follow the simulation clock tick by
  tick.

### Layout

The reference board is **portrait, 390 × 844 units, scaled to match width**:

- Layout coordinates are the same on every phone; height is what varies.
- Supported widths are 320 to 430 units. Under 380, the root carries `vb-compact`.
- Safe-area insets pad the root, with at least 48 units on top and 34 at the bottom.

Read-only state sits along the top, under the inset:

- wave and score;
- the toggles;
- Power with its queue;
- focus.

The four operative cards sit in the thumb zone above the bottom inset. Every target is at least 44 × 44 units. Details
are in *Portrait HUD layout*.

### Imagery

The setting is the cityscape the game runs through, a ruined boulevard at dusk:

- purple cloud sky and distant neon towers;
- dark apartment blocks and a fallen glass skybridge;
- wet asphalt, sodium lamps, bare trees, wrecks, fires and embers.

Operatives are anime-styled fighters in pale tech-wear, each with one glowing kit colour. Lighting is cool fill with a
warm rim and the operative's kit tint. Never daylight, never a saturated rainbow.

## Iconography

- **One set, drawn to one spec:** a 24-unit grid, 1.6-unit square-cap strokes, no fills, geometric.
  - In Unity, `VbIcon` draws it from `Runtime/Icons` (99 glyphs plus 8 alias names).
  - On the web, `VbIcon` inlines the same SVGs with `currentColor`.
  - The Icons asset group holds the same files as single-ink SVGs (ink `#faeef4`).
- **Families:** skill shapes, roles, statuses, operative skills, HUD controls and menu destinations.
- **An icon always travels with a word.** The only exceptions are a skill button whose card names the operative, and
  icon-only buttons with a tooltip.
- No emoji, no filled icons, no third-party icon fonts.

## The mark

The VEIL/BREAK mark is a slash-cut wordmark:

- **Source:** `Runtime/Vectors/Mark.svg`.
- **Data:** `MarkGeometry`, 15 parts.
- **Drawn by:** `VbMark` (513 × 259) and the intro splash.

Set the name in text as **VEIL/BREAK**: TeX Gyre Adventor Bold, caps, `0.3em` tracking, with the slash in `text-3`.
Never redraw Pastel Black's slashed wordmark for Veilbreak.

## Where things live

| Need | In Unity (`com.veilbreak.gui`) | On the web (this system) |
| --- | --- | --- |
| Tokens | `Tokens~/tokens.json` → `Runtime/Styles/Tokens.uss` (`--vb-color-ally`, `--vb-cut-md`), `VbTokens` | `tokens.json` → CSS custom properties without the prefix (`--ally`, `--cut-md`, `--font-display`) |
| Components | `Veilbreak.Gui` (UI Toolkit), USS classes `vb-…` | `window.Veilbreak` (React 18), the same `vb-…` classes in `components/bundle.css` |
| Fonts | `Runtime/Fonts` | `fonts/` |
| Icons | `Runtime/Icons` | `VbIcon`, and the Icons asset group |
| Guidelines | `Documentation~/guidelines` | The sections after this one |
| Component reference | `Documentation~/components.md` | Each component's README and `components/index.d.ts` |

The package is the source. This system is rebuilt from it by `Tools~/web/build.sh`, so change the package, not the
files here.

## Using this system on the web

- **Root.** Put class `vb-root` on the screen's root element, and on nothing else. It carries UI Toolkit's layout
  defaults (every element a column flexbox), which the ported rules assume. Popovers render into `document.body` with
  their own `vb-root`. They copy the anchor's root flags and theme.
- **Classes.** Every class is the Unity USS class: BEM blocks, `__parts` and `--state` modifiers
  (`.vb-skill--cooldown`). Restyle a state by its modifier class, and never add new `vb-` names of your own.
  - Type utilities: `vb-t-display`, `vb-t-title`, `vb-t-label`, `vb-t-badge`, `vb-t-caption`, `vb-t-body`, `vb-t-num`,
    `vb-t-ally`, `vb-t-enemy`, `vb-t-power`.
  - Layout: `vb-row`, `vb-grow`, `vb-half`, `vb-grid2`, `vb-grid4`.
- **Shapes.** Cut shapes are painted from unitless custom properties, not borders:
  - `--vb-cut`, `--vb-fill`, `--vb-fill-to`, `--vb-fill-dir`;
  - `--vb-edge`, `--vb-edge-to`, `--vb-bw`, `--vb-dash`;
  - `--vb-bracket`, `--vb-glow`.

  A panel of your own is `<VbFrame className="vb-panel">` (`--solid`, `--raised`, `--glass`, `--enemy`).
- **Data.** Composites take the game's plain view models as `view` (`OperativeView`, `SkillView`, `QueueView`,
  `PerkView`, `HudView`, `BossView`, `ScoreView`, `ContributionView`, `RuleView`; see `components/index.d.ts`) and
  draw them. The game owns input and state. Kit colours go in as tokens: `kit: 'var(--kit-aria)'`.
- **Caps.** Components upper-case names, roles, tags and titles themselves, so pass normal case ("Aria", "Needle
  Storm"). Button titles are the exception: pass `VbButton` titles in caps.
- **Settings.** Use these on the `vb-root`:

  | Setting | How |
  | --- | --- |
  | High contrast | `data-theme="contrast"` (the token theme) plus class `vb-contrast` (drops glows) |
  | Reduced motion | `vb-reduced-motion` |
  | Reduced flashes | `vb-reduced-flashes` |
  | Larger text | `vb-large-text` |
  | Narrow board | `vb-compact` |

- **Popovers.** HudTop's queue, SquadResult's stats and PerkChip's detail open on hover (mouse), focus, or tap
  (touch). Never put information a phone player needs mid-fight only in a popover.
- **Legacy web.** The original landscape arena kit is still in the bundle as `window.Veilbreak.Legacy`, with `vbl-`
  classes:
  - the cards in the *Legacy web* group;
  - the landscape HUD board.

  Its eleven components with no kit equivalent are also exported at the top level. It is deprecated: build new
  screens from the kit, and never mix the two on one screen.

```js
const V = window.Veilbreak, h = React.createElement;
h('div', { className: 'vb-root', style: { width: 390, background: 'var(--ground)' } },
  h(V.HudTop, { view: { wave: 8, score: 12450, power: 34, focus: 62, queue: [] } }),
  h('div', { className: 'vb-row', style: { gap: 6, padding: 10 } },
    h(V.OperativeCard, { view: { name: 'Aria', code: 'AR', kit: 'var(--kit-aria)', hp: 820, maxHp: 1000, mode: 'auto',
      skills: [{ icon: 'SkillAriaNeedleStorm', cost: 18 }, { slot: 'S', icon: 'SkillAriaTerminalVector', cost: 40 }] } })),
  h(V.VbButton, { title: 'START RUN', variant: 'primary', sub: 'Aria · Rook · Aegis · Nyra', icon: 'chevron' }));
```
