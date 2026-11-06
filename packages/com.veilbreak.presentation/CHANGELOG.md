# Changelog

## 1.6.0 — 2026-11-06

- Requires gui 0.9.0: the GUI polish batch (design-system#55): the three text variants and the fluid scale, single-row action rows with an icon-only button, the activation and damage edge effect, content that follows the cut corner, the status badge row, the unit sheet's basic skill, passive and lore, the `Carousel`, one short overlay transition and the glitch title reveal; release 0.19.0.

## 1.5.0 — 2026-11-06

- Requires gui 0.8.0: the border rule, `ActionRow`, fitted drawers, the restyled popover, merged world labels, draggable squad slots and one shape vocabulary (design-system#38), and the generic composites `UnitCard`, `SkillButton`, `ChoiceCard`, `StatRow`, `ChipRow` with the screen views' text cuts (design-system#39); release 0.18.0.

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
