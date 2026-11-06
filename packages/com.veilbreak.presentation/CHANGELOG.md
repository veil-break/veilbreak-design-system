# Changelog

## 1.4.1 — 2026-11-06

- Requires environment 0.5.1 (the City shader's reflection pass is found again).

## 1.4.0 — 2026-11-06

- Requires environment 0.5.0: full actor reflections and a mirror pass that the host may enable on either tier.

## 1.3.0 — 2026-11-02

- Tested version of `com.veilbreak.environment` moves to 0.4.0 (per-index `ISegmentPlan`, `IStreetKit`, pause and reduced-motion fixes). The other three stay.

## 1.2.0 — 2026-11-02

- Tested version of `com.veilbreak.gui` moves to 0.7.0 (`VbFocus`, the screen-kit generator and vendor pack). The other three stay.

## 1.1.0 — 2026-11-02

- Tested versions move to `com.veilbreak.feedback` 1.5.0 (`CueStage`), `com.veilbreak.branding` 1.2.0 (the intro's `ended` callback)
  and `com.veilbreak.environment` 0.3.0 (`FireLight`); `com.veilbreak.gui` stays at 0.5.1. Verified in veilbreak-arcade on 6000.3.

## 1.0.0 — 2026-11-02

- Entry package for the shared presentation stack: depends on `com.veilbreak.gui` 0.5.1, `com.veilbreak.feedback` 1.3.0, `com.veilbreak.branding` 1.1.0, `com.veilbreak.environment` 0.2.0.
  No assets and no assembly. The id was previously used by arcade's game catalog, now `com.veilbreak.arcade.presentation`.
