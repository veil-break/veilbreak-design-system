# Provenance

Delivered on 2026-10-21 as `veilbreak-ar-hud-unity7000.zip`
(SHA-256 `6c43a003ee918270bdb219d89ff187d9b0e5e1d14decc3cd623d46a8179c6009`), containing the source folder, an
unsigned `com.veilbreak.ar-hud-1.0.0.tgz` and `START-HERE.md` (kept here). On intake the 75 source files matched
`source-sha256.json` (kept here) and the tarball matched the source folder byte for byte; the source folder was added
as a local package (`file:../../packages/com.veilbreak.ar-hud`), the tarball was not kept.

Changes made on intake to follow the engineering manual (chapter 08 package layout, chapter 03 assemblies, chapter 04
component fields): `autoReferenced: false` and `csc.rsp` (`-warnaserror+`) on every assembly; `HudHost` fields are
`[SerializeField] private` with tooltips and public properties, and no longer set an execution order; menus moved under
`Veilbreak/AR HUD`; `HudSetup` writes to `HudSetup.AssetFolder` (the Veilbreak UI feature folder); `HudWorkshop`
resolves its package path through `PackageInfo`; `package.json` gained author, documentation URL and dependencies.
