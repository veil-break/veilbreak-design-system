# com.veilbreak.branding

Call IntroSplash.Play(host, reducedMotionOrFlashes, volume, ended). It consumes skip input and detaches at 4.33 seconds, then calls `ended` once (also when it is skipped through, stopped or detached), so a host that opened `host` as an own-input modal can close it; reduced mode fades the final mark. The blade's strike plays a slicing sound (Resources/VeilbreakSplashSlice.wav, timed so its cut lands on the impact; set IntroSplash.SliceSound to replace or silence it). It needs an AudioListener in the scene. To make the intro your game's splash, turn off Unity's splash screen (PlayerSettings.SplashScreen.show = false, available on every Unity 6 plan) and play it on your first frame. Call Stop on an explicit teardown. Pinned is only for captures. The original path geometry and phase timing are adapted from Veilbreak; portrait framing fits essential geometry. No arcade, roster or sibling navigation assembly is required.

The mark: `Runtime/Vectors/Mark.svg` (menu and loader mark, the intro's final pose) and `Runtime/Icons/Mark.svg` (the `mark` glyph for a `HudIcons` library). `Tools~/logo.py` regenerates the vector from `Tools~/source/veilbreak-mark-dark.svg`; run it with `--check` in CI.

Authoring sources (not imported by Unity): `Tools~/source/logo/` holds the slash-cut logo keyframes (`logo-start`, `logo-25-`, `logo-50-`, `logo-Artboard-4`) and the vectorcraft project, and `Tools~/splash/` holds the splash keyframes and the animation prototypes the intro was ported from. Both moved from veilbreak-main's `assets/logo/`.

Assemblies: `Veilbreak.Branding`. Unity 6000.3.

Install from git with a reviewed commit, `"com.veilbreak.branding": "https://github.com/veil-break/veilbreak-design-system.git?path=/packages/com.veilbreak.branding#<full-reviewed-commit>"`, plus the git sources of its first-party dependencies (Unity does not resolve git dependencies declared inside packages). Veilbreak games get these entries from `vb packages remote`; use `vb packages local --package com.veilbreak.branding` to edit this folder from a game. Moved here from veilbreak-arcade with its GUIDs unchanged.

See Samples~/Integration.md for an integration example. Engine-free regression checks are in tests/Portable in the development workspace; package-consumer validation copies packages into a clean Unity project.

Import the Minimal consumer sample from Package Manager. Package-local Editor tests are in `Tests/Editor`; enable package tests with Unity Test Framework (`testables` in the consumer manifest).
