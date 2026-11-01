# GUI 0.5 validation

Evidence recorded 2026-11-01. Unity 6000.3.27f1 is the target editor. This environment has .NET 8, Python 3.12 and Chromium, but **no Unity editor**.

## Executed

- Clean isolated uv tooling checkout: locked dependency installation, Ruff lint/format, 6 original unittest checks, token/sample/meta checks and an 82-entry handbook build passed. There are no Python runtime dependencies or sibling-project requirements.
- Refactor checkout: locked uv sync, Ruff lint/format and **11 unittest checks** passed, including complete source inventory/base validation, shared overlay ancestry, public mirrors, generated motion and nonuniform scale conversion.
- Generated handbook: **89 entries, 85 public web exports**, shared motion stylesheet and recipe configuration. Both web build paths parse their generated JavaScript. Tracked Web~/dist was regenerated from source, including the pre-existing eleven-rule CSS drift.
- Chromium inspector regression: **178 entry checks** (all 89 entries at 390px and 1280px), all **14 skill states**, bounds/property inspection, navigation cleanup and open-inspector WCAG checks passed. The accessibility check excludes existing component mirrors; it is not a claim that every legacy mirror is fully accessible.
- Chromium interaction checks passed with mouse and emulated touch: twelve individually inspectable effects, hover transfer into nested details, Escape closing one level, outside dismissal, drawer coverage retained during exit, selectable staggered perk cards, stable DOM on rebind and reduced-motion settling. Effect and skill touch targets fit battle cards at **320px and 390px**.
- Arcade portable executable: **298 checks passed** (283 existing plus 15 effect-projection checks). The new checks cover every authored status plus an unknown fallback, unique IDs, explicit actor modifiers, all applicable lasting perks, all eight passive entries, boss shields/enrage/interruptible windups, hostile isolation, same-tick snapshot reuse, missing-attribution handling, cost/cooldown cap metadata, immutable expiry snapshots and unchanged simulation/RNG/commands.
- Roslyn parsed **98 C# files** across GUI runtime/tests, game runtime and URP feedback with C# 9 syntax and found no syntax errors. This is a syntax check, **not Unity compilation**.
- Arcade's standalone package consumer was prepared with `tools/verify-packages.py`. Package synchronization compares SHA256 content and preserves the existing relative package symlink and asset GUIDs.

## Required native acceptance

The new Unity EditMode reuse tests and attached-panel MotionLifecycleTests are authored but could not run here. Before native acceptance, run the pinned editor's full EditMode/PlayMode suites and ConsumerProbe.Verify against Reports/PackageConsumer. Confirm UXML generation and native focus/picking behavior, interruption/reversal and simultaneous/nested overlay closure.

Compile OverlayBlur on Metal and Android/iOS targets. Inspect Render Graph ordering (world post-processing → overlay blur → gamma UI), scratch texture lifetime, Low/Standard quality and bypass at zero weight. Measure actual Android/iPhone GPU cost, sustained frame time, thermal behavior, touch, safe areas, interruptions and animation feel. Desktop browser and source checks establish none of these device results.

No registry release, tag, application build publishing, store configuration or editor upgrade is included. Review the migration guide before adopting the breaking API.
