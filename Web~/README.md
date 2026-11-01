# @veilbreak/gui — the web kit

The Veilbreak design system for web apps. It is generated from the Unity package in this repository
(`com.veilbreak.gui`), so a React component here and its UI Toolkit element in Unity have the same name, element tree,
BEM classes and states. Change the system in the package; never edit `dist/` by hand.

## Install

The folder ends in `~`, so Unity ignores it. npm cannot install a subfolder of a Git repository, so install it from a
checkout (or copy `Web~/dist` into your app):

```bash
npm install ../veilbreak-design-system/Web~
```

React 18 or newer is a peer dependency. `dist/` is committed, so no build step is needed.

## Use

```js
import '@veilbreak/gui/tokens.css';      // tokens as custom properties (+ @font-face for the bundled fonts)
import '@veilbreak/gui/veilbreak.css';   // the kit's styles (Gui, Layout, Views and World USS, ported)
import { PauseOverlayView, VbButton } from '@veilbreak/gui';

export function Pause({ run, onResume }) {
  return (
    <div className="vb-root">                     {/* every kit tree renders inside vb-root */}
      <PauseOverlayView view={{ wave: run.wave, score: run.score, standing: 3, perks: run.perks }} onResume={onResume} />
    </div>
  );
}
```

Without a bundler, load React 18 UMD, then `dist/veilbreak.js`; the kit is `window.Veilbreak`:

```html
<link rel="stylesheet" href="dist/tokens.css"><link rel="stylesheet" href="dist/veilbreak.css">
<script src="https://cdnjs.cloudflare.com/ajax/libs/react/18.3.1/umd/react.production.min.js"></script>
<script src="https://cdnjs.cloudflare.com/ajax/libs/react-dom/18.3.1/umd/react-dom.production.min.js"></script>
<script src="dist/veilbreak.js"></script>
```

`demo/index.html` is a small app that chains the views (menu → squad → battle → pause → perks → results → scores, plus
settings and the guide); it is published with the handbook.

## What is in it

| Tier | Components |
| --- | --- |
| Atoms | `TextComponent`, `LabelComponent`, `VbIcon`, `VbMark`, `VbFrame` (`kind="slim"`/`"chipped"`), `VbButton`, `BadgeComponent`, `StatusChip`, `CreditsChip`, `VbMeter`, `VbSwitch`, `VbSlider`, `VbSegmented`, `TileComponent`, `CardComponent`, `GlitchText`, `Backdrop`, `ScanLines`, world atoms |
| Composites | `IconButtonComposite`, `DrawerOverlay`, `FieldComposite`, `Fieldset`, `FormComposite`, `StatReadout`, `HealthBar`, `PowerStrip`, `FocusMeter`, `SkillButton`, `QueueChip`, `QueueList`, `HudTop`, `BossBar`, `ScreenHeader`, `ScoreRow`, operative and perk composites, `Dialog`, holo surfaces, `RuleRow` |
| Layouts | `StackLayout`, `VbGrid`, `ListLayout`, `BoardLayout`, plus the `vb-row`, `vb-grid4`, `vb-grid2`, `vb-screen`/`vb-page` classes |
| Views | `MainMenuView`, `MenuActionsSectionView`, `SquadView`, `SettingsView`, `PauseOverlayView`, `ConfirmDialogView`, `GuideDialogView`, `PerkDraftView`, `ResultsView`, `HighScoresView`, `RouteChoiceView`, `MerchantScreenView`, `BattleHudView` |

Types are in `dist/index.d.ts`. Views take the same plain view models as their Unity counterparts (`view` prop) and
report with `on*` callbacks; the app owns state.

## Build

```bash
uv run --frozen python Tools~/web/package.py
```

from the repository root. It ports the USS with `Tools~/web/uss2css.py`, inlines the icons and writes `dist/`.
