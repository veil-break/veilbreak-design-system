# Changelog

## 1.0.0 — 2026-11-02

First release, moved from veilbreak-main with the script and shader GUIDs unchanged.

- `LensStreaks` and `LensStreakFeature` with `SH_VeilStreaks.shader`.
- `Glitch`, `ScreenGlitchFeature` and `ScreenGlitch` with `SH_VeilGlitch.shader`. The host now sets its glitch volume's weight
  from `ScreenGlitch.Changed` instead of the transient calling into the game's volume controller.
- `VisorLens` and `VisorLensFeature` with `SH_VisorLens.shader`. `Barrel` and `FirstQuality` are host-set fields instead of
  the game's token value and desktop tier.
- `MaterialPaint`.
- The shader property ids are package-owned (`RenderIds`) instead of the game's generated `ShaderIds`.
