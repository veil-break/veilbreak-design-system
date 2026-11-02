# Battle HUD

A portrait battle board built only from the kit, with fake data that ticks: `HudTop`, `BossBar`, `PowerStrip` (autoplay
reserve, a manual queue whose × cancels), four `OperativeCard`s and a `WorldLayer` over a flat arena projected top-down
(team bars, merged combat text, an aim chip, a boss telegraph label, markers and a breach `OffscreenIndicator`).
Escape opens a pause sheet through `Navigator`; `SafeAreaRoot` owns the insets.

Open `BattleHud.unity` and enter Play mode, or add `BattleHudSample` to a GameObject with a `UIDocument` that uses
`Packages/com.veilbreak.gui/Runtime/Settings/VeilbreakPortrait.asset`. Use a 390 × 844 Game view.
