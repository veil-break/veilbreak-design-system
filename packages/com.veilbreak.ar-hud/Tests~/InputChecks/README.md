# ar-hud input checks

A console runner that compiles the production `HudInput` and `HudStage` sources against headless stand-ins for Unity's
screen and UI Toolkit panel (`Shims.cs`) and checks:

- board geometry across device sizes, panel scales, origins and handedness: viewport/screen round trips, camera-projected
  labels and the safe area staying on the physical edge;
- pick blocking: which elements block the world under the HUD (buttons, fields, `HudInput.BlockClass` sheets) and that
  `HudInput.PassThrough` ignores picking recursively.

From the repository root, with .NET 8 or newer (Unity's bundled SDK works):

```sh
dotnet run --project packages/com.veilbreak.ar-hud/Tests~/InputChecks/Veilbreak.ARHud.InputChecks.csproj
```

The folder ends in `~`, so Unity does not import it. Games keep their own input checks (veilbreak-main's battle input runner)
and treat the HUD as a hit predicate.
