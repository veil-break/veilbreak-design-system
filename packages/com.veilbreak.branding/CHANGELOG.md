# Changelog

## unreleased

- Moved from `veilbreak-arcade/packages` to `veilbreak-design-system/packages` with every file and GUID unchanged. Consume it from git at `?path=/packages/com.veilbreak.branding`.

## 1.2.0 — 2026-11-02

- `IntroSplash.Play` takes an optional `ended` callback, called once when the intro finishes, is skipped through, stopped or
  detached. veilbreak-main hosts the splash in its navigation's own-input modal and closes the modal from it; main's own copy
  of the intro is retired.

## 1.1.0 — 2026-11-01

- **Slicing sound.** The splash plays `Runtime/Resources/VeilbreakSplashSlice.wav` as the blade strikes: a rising whip of air,
  then at the cut a hard tick, a ringing steel "shing", the world tearing and a low thump. It starts `SliceLead` (0.24 s)
  before the impact, so the cut lands on the frame where the blade locks (`SliceStart`). It plays once and stays silent
  when the intro is skipped past the cut, pinned for a capture or reduced (no strike). `Play(host, reduced, volume)` sets
  its level; `IntroSplash.SliceSound` swaps or silences the clip. `Tools~/splash_sound.py` regenerates it deterministically
  with the feedback package's synthesis helpers. The package now depends on `com.unity.modules.audio`.

## 1.0.1 — 2026-11-01

- Adopt GUI 0.5.0 with shared view, effect and motion foundations. Splash behavior is unchanged.

## Unreleased (folded into 1.0.1)

- `IntroSplash` draws the mark from `Veilbreak.Gui.MarkGeometry` and colours it with `VbTokens` (`SplashGround`, `Text`, `SignalPink`, `Ally`) instead of private copies; the package now depends on `com.veilbreak.gui` 0.2.0. Timeline, skip and reduced-motion behaviour are unchanged.

## 1.0.0

Extract reusable runner, feedback and splash infrastructure; deliver runner-relative combat, versioned checkpoints and explicit dependency boundaries.
