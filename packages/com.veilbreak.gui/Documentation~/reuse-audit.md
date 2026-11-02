# Component reuse, effects and motion audit

Baseline: design-system main `920ef0ef16db69524a7e92eff2c19a2b6fa19142` (merged inspector PR #5), plus the matching embedded GUI and its host consumers in `veil-break/veilbreak-arcade`. This report audits source behavior, not physical-device performance. The private game's implementation details stay in its repository; this public report names only generic integration boundaries.

## Scope and component inventory

The baseline has **80 public visual class declarations in 44 runtime C# files**, including abstract ScreenView. The list below includes every original visual type; services, recipes and assets were inspected separately. The current typed inventory, including new foundations, is `Tokens~/components.json`.

### Controls (27)

`BadgeComponent`, `VbIcon`, `VbMark`, `VbButton`, `StatReadout`, `StatusChip`, `CardComponent`, `VbSwitch`, `VbSlider`, `VbSegmented`, `GlitchText`, `IconButtonComposite`, `VbMeter`, `HealthBar`, `PowerStrip`, `FocusMeter`, `ProgressBar`, `ListRow`, `PerkRow`, `ScoreRow`, `RuleRow`, `KeyValueRow`, `QueueChip`, `SkillButton`, `TextComponent`, `LabelComponent`, `TileComponent`.

### Composites (24)

`Backdrop`, `DrawerOverlay`, `FieldComposite`, `FormComposite`, `ScanLines`, `HoloFrame`, `HoloPlatform`, `RevealCard`, `HudTop`, `QueueList`, `BossBar`, `ScreenHeader`, `OperativeCard`, `OperativeTile`, `SquadSlot`, `OperativeDetails`, `OperativeStats`, `SquadResult`, `PerkCard`, `PerkDetail`, `PerkChip`, `CreditsChip`, `RouteChoice`, `MerchantScreen`.

### Core (1)

`VbFrame`.

### Layouts (4)

`StackLayout`, `VbGrid`, `ListLayout`, `BoardLayout`.

### Views (17)

`BattleHudView`, `RouteChoiceView`, `MerchantScreenView`, `MenuActionsSectionView`, `MainMenuView`, `PauseOverlayView`, `ConfirmDialogView`, `GuideDialogView`, `PerkDraftView`, `ResultsView`, `HighScoresView`, `ActivePerksView`, `RevivePickView`, `ScreenView`, `SettingsView`, `AccessibilityView`, `SquadView`.

### World (7)

`WorldHealthBar`, `CombatText`, `WorldLabel`, `WorldMarker`, `OffscreenIndicator`, `EdgeWarning`, `WorldLayer`.

Services and supporting systems reviewed: Navigator, ModalHost (replaced by OverlayHost), Popover, VbMotion, Glitch, Announcer, SafeAreaRoot, VbShape, Ui, FontPrimer, MarkGeometry, VbTokens, TargetDrag and GestureSurface; both web build paths, CSS converter, declarations, handbook/inspector schema, token/sample/meta generators and Pages workflow.

## Complete finding register

Each finding states the observed issue, the relevant evidence, and the implementation decision. “Added” describes source delivered in this change; execution evidence and remaining platform gates are recorded separately in `validation.md`.

### A01 — Existing inheritance already works

**Finding and evidence.** LabelComponent extends TextComponent, OperativeTile/SquadSlot extend TileComponent, and specialized rows extend ListRow. Preserve these useful contracts instead of introducing a universal widget superclass.

**Resolution.** Retained; SurfaceView adds common painting without replacing native control bases.

### A02 — Card selection and surface behavior were isolated

**Finding and evidence.** CardComponent supplied slots and selection while PerkCard and specialized surfaces reimplemented the same frame and selected state.

**Resolution.** CardView centralizes selection and SurfaceView centralizes painting; CardComponent retains optional slots and PerkCard uses CardView without extra layout wrappers.

### A03 — Detail and holographic surfaces duplicated construction

**Finding and evidence.** OperativeDetails, OperativeStats, PerkDetail and HoloFrame independently initialized shape and picking behavior.

**Resolution.** DetailView and SurfaceView now supply shared behavior; SkillSummary and FactCell are reused across detail consumers.

### A04 — Badge families were parallel implementations

**Finding and evidence.** StatusChip, CreditsChip and PerkChip did not share BadgeComponent slots; changes could drift.

**Resolution.** CreditsChip and EffectBadge now extend BadgeComponent. StatusChip remains a labelled static status recipe and PerkChip retains its authored art layout; neither is the live battle effect model.

### A05 — Collection patterns were reimplemented

**Finding and evidence.** QueueList built its own row tree, while route choices and merchant offers had unrelated discovery and binding paths.

**Resolution.** QueueList now consumes QueueChip; route signs are persistent specialized surfaces; merchant variants consume ListRow and keyed reconciliation.

### A06 — Some different bases are necessary

**Finding and evidence.** Buttons, sliders, labels, scrolls, world projections and painted meshes have distinct native behavior.

**Resolution.** Keep native control bases and WorldLayer projection/pooling. Reuse view data, motion and composition without a deep inheritance chain.

### A07 — Folder taxonomy mixed type families and composition levels

**Finding and evidence.** Controls, Composites and Views did not distinguish overlays, effects, details, forms, collections and surfaces consistently.

**Resolution.** Tokens~/components.json records kind, family, level, base, source and web availability. Handbook navigation uses families while retaining level metadata.

### A08 — Catalog recipes and services looked like concrete components

**Finding and evidence.** ScreenView is abstract; Popover/Announcer/Navigator are services; fieldsets/root recipes are not all instantiable component types.

**Resolution.** Catalog entries expose explicit kind and base metadata; the complete type inventory distinguishes services, foundations and concrete components.

### A09 — Three Unity views lacked React mirrors

**Finding and evidence.** ActivePerksView, RevivePickView and AccessibilityView were missing from the exported web kit.

**Resolution.** Added mirrors, declarations, inspectable properties and previews for all three.

### A10 — Overlay inheritance was fragmented

**Finding and evidence.** Pause derived from ScreenView, confirmation/guide/drawer derived directly from VisualElement, and Popover built an unrelated scroll.

**Resolution.** OverlayView is the shared abstract lifecycle. Typed dialogs, pause, drawer, dialog surfaces and popup/layer implementations inherit it; OverlayHost owns presentation.

### A11 — Modal exit restored input too early

**Finding and evidence.** ModalHost.Pop enabled the underlying layer, removed coverage and restored focus before the exit animation finished.

**Resolution.** OverlayHost keeps coverage and a pickable blocker until completion, then restores prior enabled/focus state.

### A12 — Drawer timing had independent owners

**Finding and evidence.** A fixed 240 ms timer, delayed open and an additional delayed modal pop could disagree on completion or reversal.

**Resolution.** Drawer uses shared entrance/exit recipes; the host waits for its actual close callback. Drag/snap height remains independent.

### A13 — Popover interaction was fragile

**Finding and evidence.** Pointer leave immediately closed it; PickingMode.Ignore prevented interaction with its content.

**Resolution.** Added a hover-transfer grace period, interactive scroll content, focus transfer, outside dismissal, touch pin/toggle and Escape handling.

### A14 — Popover ownership was global

**Finding and evidence.** Global CloseAll could dismiss unrelated panels; detached anchors could leave ownership unclear.

**Resolution.** OverlayHost scopes overlays and popup cleanup; navigation closes popovers within its scope. CloseAll remains an explicit application-wide teardown operation.

### A15 — Coverage could dim a layer more than once

**Finding and evidence.** Covered opacity .55 combined with modal and drawer scrims.

**Resolution.** Covered UI keeps full opacity and uses blur; scrims own UI dimming. World coverage has one bounded dim/blur weight.

### A16 — UI blur did not affect the 3D world

**Finding and evidence.** vb-covered only affected UI Toolkit; existing CinematicPulse is an impact effect, not an overlay blur.

**Resolution.** Arcade has a dedicated Render Graph world blur after world grading and before GammaUiFeature; zero weight bypasses it. Standard/Low use bounded downsampling.

### A17 — Bind cleared and rebuilt live UI

**Finding and evidence.** Merchant, route, queue, result grids/perks/ledger, score lists, revive tiles, operative skills/stats, perk detail rows and totals rebuilt children.

**Resolution.** Shared KeyedList/Keyed reconciliation updates existing instances; changed values preserve row identity, focus and enclosing scroll.

### A18 — Existing reuse was not generalized

**Finding and evidence.** Squad tiles and draft choices already reused slots, but other lists did not.

**Resolution.** Keyed reconciliation is now used by draft choices, active perks, revive tiles and list/detail consumers.

### A19 — Merchant dealing queried a missing class

**Finding and evidence.** MerchantScreenView queried vb-merchant__offer, but constructed rows lacked that class.

**Resolution.** Offers now have the class and Market.Choices exposes the actual registered items directly.

### A20 — Boss binding depended on object identity

**Finding and evidence.** BattleHudView skipped Boss.Bind when a cached BossView instance changed fields.

**Resolution.** Boss values are read on every bind; host caches may still avoid model allocation.

### A21 — Item identity was implicit

**Finding and evidence.** Rows often used array positions or freshly built nodes, with no stable host IDs.

**Resolution.** Effects require unique IDs; offers/queues/scores accept IDs; perk and operative IDs key their respective collections. Fixed left/right road slots have explicit identities.

### A22 — Live effects were a six-value status enum

**Finding and evidence.** StatusKind could not represent arbitrary buffs, conditional passives, run modifiers or unknown statuses.

**Resolution.** EffectDefinition/EffectInstance separate authored meaning from live stacks, value, scope, activation and optional timing/source.

### A23 — Battle statuses were tooltip-only icons

**Finding and evidence.** 14-unit glyphs had no persistent touch explanation.

**Resolution.** EffectBadge has a 44-unit target and shared EffectDetails; EffectStrip offers full individual overflow via EffectList.

### A24 — Arcade silently dropped effects

**Finding and evidence.** A whitelist and Math.Min(4, count) excluded unrecognized statuses and overflow.

**Resolution.** The read-only adapter retains every live status, including unknown IDs, and projects the full modifier set.

### A25 — Explicit actor modifiers were omitted

**Finding and evidence.** Barrier/plating/cadence/stagger/knockback and emergency-barrier eligibility existed outside StatusState.

**Resolution.** Adapter projects these fields with authoritative remaining simulation ticks and appropriate activation state.

### A26 — Passives and lasting perk modifiers were absent

**Finding and evidence.** Battle presentation omitted canonical character passives and owned run/character perks.

**Resolution.** Every operative gets its passive and applicable lasting perk entries; instant consumed rewards are excluded. Conditional effects remain explicitly conditional.

### A27 — Perk values were inferred from prose

**Finding and evidence.** A regular expression multiplied the first number of a description, potentially changing a radius or timing instead of a contribution.

**Resolution.** The adapter reads PerkDefinition.value/Stat, stacks and ArcadeRun.Bonus. Cost/cooldown caps and Surge's two contributions are explicit; narrative text is not rewritten.

### A28 — Sources and original durations were not recorded

**Finding and evidence.** StatusState aggregates by ID and stores remaining ticks/amount/age, without attribution or original duration.

**Resolution.** Nullable timing/source represents missing information. No attacker or total duration is invented; details label missing information.

### A29 — Complete effects could overwhelm narrow HUD cards

**Finding and evidence.** Showing every full badge in an 88-unit card would obscure skills and the field.

**Resolution.** A compact first badge plus count opens the complete scrollable list; every item can open details. Focused hostiles and bosses use the same effect presentation.

### A30 — Existing VbMotion had useful behavior

**Finding and evidence.** Real-time scheduling, rest-transform restoration, serial cancellation, off-panel completion and reduced-motion checks were already centralized.

**Resolution.** Retained and expanded as a recipe compositor; detached elements finalize presence, feedback and counts.

### A31 — Feedback interrupted presence

**Finding and evidence.** Show/In/Out and Pop/Shake shared a single timer and cancelled each other.

**Resolution.** Presence and feedback have separate state on a composited track; value counting has its own lifecycle.

### A32 — Count cleanup was incomplete

**Finding and evidence.** Finish returned when the presence timer was empty and did not finalize the count timer.

**Resolution.** Finish now settles the count target, pauses count scheduling and invalidates pending callbacks even without presence motion.

### A33 — Decorative motion used unrelated schedules

**Finding and evidence.** HoloFrame flicker, RevealCard flips and skill-rejection removal scheduled uncancelled work.

**Resolution.** Holo/reveal use VbMotion recipes. Rejection feedback owns and cancels its timer on detach; announcement holds remain intentional semantic durations.

### A34 — HUD reveal bypassed shared motion

**Finding and evidence.** BattleHudView.Reveal assigned opacity directly and Bands switched visibility; deployment had a separate real-time progress source.

**Resolution.** HUD top and ordered cards scrub shared recipes using host deployment progress; automatic band changes use the same motion service.

### A35 — Dealing required CSS discovery and manual timing

**Finding and evidence.** ScreenView.Deal queried or enumerated children and consumers could miss items; unchanged binds could replay entrances.

**Resolution.** Explicit Choices/DealItems registrations and keyed fresh-item motion keep ordering stable. Delay is 80 ms with a 400 ms cap; visible items are selectable immediately.

### A36 — Web builds omitted Motion.uss

**Finding and evidence.** Both build.sh and package.py assembled only Gui/Layout/Views/World sheets.

**Resolution.** Both Python build paths now include Motion.uss and share generated recipe configuration with Unity.

### A37 — USS scale conversion dropped an axis

**Finding and evidence.** uss2css took value.split()[0], breaking nonuniform scale and future flip effects.

**Resolution.** The converter preserves both axes and has a regression check.

### A38 — Inspector metadata could drift from Unity

**Finding and evidence.** TypeScript plus observed React props were its only schema authority.

**Resolution.** The canonical Unity type inventory and inheritance are attached to handbook entries. Typed React models cover new effect states; contract tests validate the inventory and public mirrors.

### A39 — Tracked web CSS was stale

**Finding and evidence.** Eleven rules for build, revive, accessibility, leaving pages, bottom bands and cancel controls differed from source output.

**Resolution.** Regenerated Web~/dist; CI compares generated tracked outputs after rebuilding.

### A40 — Motion evidence was mostly settled frames/off-panel tests

**Finding and evidence.** Prior checks did not establish interrupted animations, nested modal timing or phone GPU cost.

**Resolution.** Added attached-panel lifecycle tests, deterministic clock control and browser interaction/motion checks. Unity execution and physical-device measurements remain required and are not claimed.

### A41 — Arcade embeds the GUI package

**Finding and evidence.** Independent copying risks drift and symlink/GUID loss; Arcade held additional runtime tests.

**Resolution.** Sync tooling copies the canonical package with SHA256 provenance and retains package symlinks; all runtime tests are kept in the design-system source.

### A42 — No root Python project existed

**Finding and evidence.** There was no pyproject.toml, uv.lock or .python-version.

**Resolution.** Added a non-packaged Python >=3.12 tooling project, managed Python 3.12 selector, pinned Ruff and committed uv.lock.

### A43 — Generators are stdlib tools

**Finding and evidence.** There are no Python runtime dependencies; tests use unittest. Browser tooling is Node-based.

**Resolution.** Runtime dependencies stay empty; Ruff is a locked development dependency and browser packages remain in a separate Node tool project.

### A44 — Ruff depended on a sibling project

**Finding and evidence.** Tools~/web/README used uv run --project ../../../tools, despite a copied local Ruff config.

**Resolution.** Moved Ruff policy to the root pyproject and documented self-contained uv commands; verbatim embedded templates are excluded from formatting.

### A45 — Subprocess interpreters were inconsistent

**Finding and evidence.** The shell builder used python3 repeatedly and the handbook invoked that shell.

**Resolution.** An import-safe Python build driver uses sys.executable throughout. build.sh is a thin uv compatibility entry point.

### A46 — Pages CI used an unlocked environment

**Finding and evidence.** Actions set up Python directly and ran builders/tests with python3.

**Resolution.** CI pins uv, uses locked sync, explicit Node, lint/format/unit/generator checks, a reproducible build and the existing Pages deployment boundary.

### A47 — Build verification silently weakened

**Finding and evidence.** Node absence skipped syntax checks; screenshot harness defaulted to a macOS binary and ignored failures.

**Resolution.** Node is required; CHROME is discovered or supplied explicitly; failed/missing screenshots fail the harness.

### A48 — Generated output drift had no gate

**Finding and evidence.** Generators wrote tracked files without a standard clean-diff verification step.

**Resolution.** Generator --check commands plus rebuilt tracked web/token output comparison form the CI gate.

### A49 — Meta traversal entered virtual environments

**Finding and evidence.** rglob traversed .git/.venv before filtering hidden paths.

**Resolution.** os.walk prunes hidden, tilde, work and node_modules directories before descent.

### A50 — Cached skill state suppressed transition feedback

**Finding and evidence.** SkillButton compared View.state with the same mutable SkillView instance just passed by the host.

**Resolution.** A separate previousState value detects ready/rejected transitions even when the model is reused.

## Architecture and adoption

`ViewBase` holds identity and lifecycle. `ScreenView` and abstract `OverlayView` are separate families. `SurfaceView` provides painting, `CardView` selection, and `DetailView` explanation surfaces without forcing extra slots into specialized layouts. Native Button/Slider/Label/ScrollView bases retain their native semantics. `KeyedList` handles collection identity. A family is independent of composition level: an overlay can be a complete view or a small popup.

Motion recipes are authored once in `Tokens~/motion.json`, then generated into Unity and browser configuration. Presence uses real time; effect durations come from simulation snapshots. Count, feedback, presence and host deployment progress have distinct owners. Reduced motion removes translation/rotation/scale and stagger while retaining a brief fade. Reduced flashes remains an independent setting.

Effect definitions describe meaning; instances describe identity, magnitude, stacks, scope, activation and optional attribution/time. Battle overflow never truncates the source data. Perk and passive explanations use authored definitions rather than text parsing. The GUI has no game singleton or simulation dependency.

## Release and acceptance boundaries

This is a clean **0.5.0** API release. Update consumers together; see `migration-0.5.md`. Publishing registries, tagging releases and shipping store builds are separate tasks. Unity 6000.3.27f1 rendered EditMode/PlayMode, standalone package consumption, Metal/mobile shader compilation and structured Android/iPhone testing are required before claiming native acceptance. A source parser or desktop browser check is not a replacement for those gates.
